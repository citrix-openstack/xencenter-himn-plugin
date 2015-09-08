using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using XenAPI;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace HIMN
{
    static class Program
    {
        const string LOG_ROOT = @"Logs";
        const string APP_ICON = "AppIcon.ico";

        #region Business Code
        static string getVM(Session session, string vm_uuid)
        {
            List<XenRef<VM>> vmRefs = VM.get_all(session);
            foreach (XenRef<VM> vmRef in vmRefs)
            {
                VM vm = VM.get_record(session, vmRef);
                if (vm.uuid == vm_uuid)
                {
                    return vmRef.opaque_ref;
                }
            }
            return null;
        }

        static string getNetwork(Session session, string bridge)
        {
            List<XenRef<Network>> netRefs = Network.get_all(session);
            foreach (XenRef<Network> netRef in netRefs)
            {
                Network net = Network.get_record(session, netRef);
                if (net.bridge == bridge)
                {
                    return netRef.opaque_ref;
                }
            }
            return null;
        }

        static string getVIF(Session session, string netRef, string vmRef)
        {
            List<XenRef<VIF>> vifRefs = VIF.get_all(session);
            foreach (XenRef<VIF> vifRef in vifRefs)
            {
                VIF vif = VIF.get_record(session, vifRef);

                if (vif.network.opaque_ref == netRef &&
                    vif.VM.opaque_ref == vmRef)
                {
                    return vifRef.opaque_ref;
                }
            }
            return null;
        }

        static string createVIF(Session session, string netRef, string vmRef, string device)
        {
            VIF vif = new VIF()
            {
                VM = new XenRef<VM>(vmRef),
                network = new XenRef<Network>(netRef),
                device = device
            };
            XenRef<VIF> vifRef = VIF.create(session, vif);

            return vifRef.opaque_ref;
        }

        static void AddHIMN(DataGridViewRow row, string url, string sessionRef, string cls, string vm_uuid)
        {
            //log
            string logroot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LOG_ROOT);
            if (!Directory.Exists(logroot))
            {
                Directory.CreateDirectory(logroot);
            }
            string logfile = DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + vm_uuid;
            string logpath = Path.Combine(logroot, logfile + ".log");
            StreamWriter logger = new StreamWriter(logpath) { AutoFlush = true };

            //params
            logger.WriteLine(string.Format("url: {0}", url));
            logger.WriteLine(string.Format("sessionRef: {0}", sessionRef));
            logger.WriteLine(string.Format("cls: {0}", cls));
            logger.WriteLine(string.Format("vm_uuid: {0}", vm_uuid));

            //validation
            if (cls != "VM")
            {
                logger.WriteLine("cls type must be 'VM'");
                return;
            }

            //session
            Session session = new Session(url, sessionRef);
            logger.WriteLine("session created");

            //host
            Host host = Host.get_record(session, session.get_this_host());
            row.Cells[0].Value = host.name_label;

            //vm
            string vmRef = getVM(session, vm_uuid);
            if (string.IsNullOrEmpty(vmRef))
            {
                logger.WriteLine("vm not founded");
                return;
            }
            else
            {
                logger.WriteLine("vm founded");
            }
            VM vm = VM.get_record(session, vmRef);
            logger.WriteLine("vm.name_label:" + vm.name_label);
            row.Cells[1].Value = vm.name_label;

            //power_state
            logger.WriteLine("vm.power_state:" + vm.power_state);
            row.Cells[2].Value = vm.power_state.ToString();

            //pv installed
            bool pvInstalled = false;
            if (vm.guest_metrics.opaque_ref == "OpaqueRef:NULL")
            {
                pvInstalled = false;
            }
            else
            {
                Dictionary<string, string> vm_guest_metrics =
                    VM_guest_metrics.get_PV_drivers_version(session, vm.guest_metrics.opaque_ref);
                foreach (string k in vm_guest_metrics.Keys)
                {
                    logger.WriteLine("vm_guest_metrics/{0}: {1}", k, vm_guest_metrics[k]);
                }
                pvInstalled = (vm_guest_metrics.Keys.Count > 0);
            }
            row.Cells[3].Value = pvInstalled;

            //himn exists
            string netRef = getNetwork(session, "xenapi");
            string vifRef = getVIF(session, netRef, vmRef);
            VIF vif;
            bool himn_exists = !string.IsNullOrEmpty(vifRef);
            if (himn_exists)
            {
                logger.WriteLine(string.Format("vif {0} exists", vifRef));
                vif = VIF.get_record(session, vifRef);
                row.Cells[4].Value = string.Format(
                    "Already added as VIF '{0}' with MAC '{1}'. ",
                    vif.device, vif.MAC.ToUpper());

            }
            else
            {
                //himn + power_state
                if (vm.power_state != vm_power_state.Halted && !pvInstalled)
                {
                    row.Cells[4].Value = string.Format(
                        "PV tools missing. Requires shutdown to re-add.", vm.name_label);
                    logger.WriteLine(string.Format("PV tools missing. Requires shutdown to re-add.",
                        vm.name_label));
                    return;
                }

                row.Cells[4].Value = string.Format("Adding internal management network...",
                    vm.name_label);

                string device = "9";
                vifRef = createVIF(session, netRef, vmRef, device);
                vif = VIF.get_record(session, vifRef);
                logger.WriteLine(string.Format("vif {0} created", vifRef));

                row.Cells[4].Value = string.Format("Added as VIF '{0}' with MAC '{1}'. ",
                    vif.device, vif.MAC.ToUpper());
                if (pvInstalled && vm.power_state == vm_power_state.Running)
                {
                    VIF.plug(session, vifRef);
                    row.Cells[4].Value = string.Format("Added and plugged as VIF '{0}' with MAC '{1}'. " +
                        "No reboot required.", vif.device, vif.MAC.ToUpper());
                }
            }

            logger.WriteLine(string.Format("himn_mac: {0}", vif.MAC));

            string himn_mac_key = "vm-data/himn_mac";
            Dictionary<string, string> xenstore_data = VM.get_xenstore_data(session, vmRef);
            if (xenstore_data.ContainsKey(himn_mac_key))
            {
                logger.WriteLine(string.Format("{0}: {1} => {2}",
                    himn_mac_key, xenstore_data[himn_mac_key], vif.MAC));
                xenstore_data[himn_mac_key] = vif.MAC;

            }
            else
            {
                xenstore_data.Add(himn_mac_key, vif.MAC);
                logger.WriteLine(string.Format("{0}: {1}", himn_mac_key, vif.MAC));
            }

            VM.set_xenstore_data(session, vmRef, xenstore_data);
            logger.WriteLine("xenstore written");

            logger.Close();
        }

        static void AddHIMNStart(object obj)
        {
            object[] parameters = (object[])obj;

            DataGridViewRow row = (DataGridViewRow)parameters[0];
            string url = (string)parameters[1];
            string sessionRef = (string)parameters[2];
            string cls = (string)parameters[3];
            string vm_uuid = (string)parameters[4];

            AddHIMN(row, url, sessionRef, cls, vm_uuid);
        }
        #endregion

        [STAThread]
        static void Main(string[] args)
        {
            HIMNForm form = new HIMNForm();

            if (File.Exists(APP_ICON))
            {
                form.Icon = new Icon(APP_ICON);
            }

            Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            if (args.Length < 4 || args.Length % 4 != 0)
            {
                MessageBox.Show(string.Format("Invalid paramenter length: {0}", args.Length));
                return;
            }

            for (int i = 0; i < args.Length; i += 4)
            {
                string url = args[i];
                string sessionRef = args[i + 1];
                string cls = args[i + 2];
                string vm_uuid = args[i + 3];
                int n = form.dgv_vms.Rows.Add("Connecting..", "Discovering..", "Unknown", false, "....");
                DataGridViewRow row = form.dgv_vms.Rows[n];
                Thread thread = new Thread(new ParameterizedThreadStart(AddHIMNStart));
                thread.Start(new object[] { row, url, sessionRef, cls, vm_uuid });
            }
            Application.Run(form);
        }
    }
}
