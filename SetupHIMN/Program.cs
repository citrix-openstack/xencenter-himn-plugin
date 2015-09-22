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

        private static bool GetPVInstalled(Session session, VM vm)
        {
            if (vm.guest_metrics.opaque_ref != "OpaqueRef:NULL")
            {
                Dictionary<string, string> vm_guest_metrics =
                    VM_guest_metrics.get_PV_drivers_version(session, vm.guest_metrics.opaque_ref);
                return (vm_guest_metrics.Keys.Count > 0);
            }

            return false;
        }

        private static void WriteXenStore(Session session, string vmRef, string key, string value)
        {
            Dictionary<string, string> xenstore_data = VM.get_xenstore_data(session, vmRef);
            if (xenstore_data.ContainsKey(key))
            {
                xenstore_data[key] = value;

            }
            else
            {
                xenstore_data.Add(key, value);
            }

            VM.set_xenstore_data(session, vmRef, xenstore_data);
        }

        static void DetectStatus(object obj)
        {
            object[] args = (object[])obj;

            HIMNForm form = args[0] as HIMNForm;
            int i = (int)args[1];

            string url = form.urls[i];
            string sessionRef = form.sessionRefs[i];
            string vm_uuid = form.vm_uuids[i];
            string logpath = form.logpaths[i];
            DataGridViewRow row = form.dgv_vms.Rows[i];

            using (StreamWriter logger = new StreamWriter(logpath) { AutoFlush = true })
            {
                logger.WriteLine("DetectStatus");

                //params
                logger.WriteLine(string.Format("url: {0}", url));
                logger.WriteLine(string.Format("sessionRef: {0}", sessionRef));
                logger.WriteLine(string.Format("vm_uuid: {0}", vm_uuid));

                //session
                Session session = new Session(url, sessionRef);
                logger.WriteLine("session created");

                //host
                Host host = Host.get_record(session, session.get_this_host());
                logger.WriteLine("host: " + host.name_label);
                row.Cells[0].Value = host.name_label;

                //vm
                string vmRef = getVM(session, vm_uuid);
                VM vm = VM.get_record(session, vmRef);
                logger.WriteLine("vm:" + vm.name_label);
                row.Cells[1].Value = vm.name_label;

                //power_state
                logger.WriteLine("power_state:" + vm.power_state);
                row.Cells[2].Value = vm.power_state.ToString();

                //pv installed
                bool pvInstalled = GetPVInstalled(session, vm);
                logger.WriteLine("pv_installed:" + pvInstalled);
                if (vm.power_state == vm_power_state.Running)
                {
                    row.Cells[3].Value = pvInstalled ? "Installed" : "Not installed";
                }
                else
                {
                    row.Cells[3].Value = "Unknown";
                }

                //himn exists
                string netRef = getNetwork(session, "xenapi");
                string vifRef = getVIF(session, netRef, vmRef);
                bool HIMNExists = !string.IsNullOrEmpty(vifRef);
                logger.WriteLine("himn_exists:" + HIMNExists);

                if (HIMNExists)
                {
                    VIF vif = VIF.get_record(session, vifRef);
                    row.Cells[4].Value = string.Format(
                        "Already added as VIF '{0}' with MAC '{1}'. ",
                        vif.device, vif.MAC);
                }
                else
                {
                    bool RebootRequired = (vm.power_state != vm_power_state.Halted && !pvInstalled);
                    row.Cells[4].Value = "Ready. " +
                        (RebootRequired ? "Requires reboot." : "No reboot required.");

                    row.Cells[5] = new DataGridViewCheckBoxCell();
                    while (!row.Cells[5].Displayed)
                    {
                        Thread.Sleep(100);
                    }
                    row.Cells[5].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    row.Cells[5].Value = true;
                    row.Cells[5].ReadOnly = false;
                }

                form.ReadyCounter += 1;
                if (form.ReadyCounter >= form.dgv_vms.Rows.Count)
                {
                    form.btnAdd.Enabled = true;
                    threads.Clear();
                }
            }
        }

        static void AddHIMN(object obj)
        {
            object[] args = (object[])obj;

            HIMNForm form = args[0] as HIMNForm;
            int i = (int)args[1];

            string url = form.urls[i];
            string sessionRef = form.sessionRefs[i];
            string vm_uuid = form.vm_uuids[i];
            string logpath = form.logpaths[i];
            DataGridViewRow row = form.dgv_vms.Rows[i];

            using (StreamWriter logger = new StreamWriter(logpath, true))
            {
                row.Cells[4].Value = "Adding internal management network...";

                DataGridViewCheckBoxCell checkbox = row.Cells[5] as DataGridViewCheckBoxCell;

                logger.WriteLine("DetectStatus");

                //params
                logger.WriteLine(string.Format("url: {0}", url));
                logger.WriteLine(string.Format("sessionRef: {0}", sessionRef));
                logger.WriteLine(string.Format("vm_uuid: {0}", vm_uuid));

                //session
                Session session = new Session(url, sessionRef);
                logger.WriteLine("session created");

                //vm
                string vmRef = getVM(session, vm_uuid);
                VM vm = VM.get_record(session, vmRef);
                logger.WriteLine("vm:" + vm.name_label);

                bool pvInstalled = GetPVInstalled(session, vm);
                bool RebootRequired = (vm.power_state != vm_power_state.Halted && !pvInstalled);
                bool AutoPlug = pvInstalled && vm.power_state == vm_power_state.Running;

                //shutdown
                if (RebootRequired)
                {
                    row.Cells[4].Value = "Shuting down...";

                    VM.shutdown(session, vmRef);
                    while (vm.power_state != vm_power_state.Halted)
                    {
                        vm = VM.get_record(session, vmRef);
                        Thread.Sleep(100);
                    }
                    row.Cells[2].Value = vm.power_state.ToString();
                    row.Cells[4].Value = "Adding internal management network...";
                }

                //adding himn
                string device = "9";
                string netRef = getNetwork(session, "xenapi");
                string vifRef = createVIF(session, netRef, vmRef, device);
                VIF vif = VIF.get_record(session, vifRef);
                logger.WriteLine(string.Format("vif {0} created", vifRef));
                string MAC = vif.MAC;
                logger.WriteLine(string.Format("himn_mac: {0}", MAC));

                //start vm
                if (RebootRequired)
                {
                    row.Cells[4].Value = "Starting VM...";
                    VM.start(session, vmRef, false, true);
                    while (vm.power_state != vm_power_state.Running)
                    {
                        vm = VM.get_record(session, vmRef);
                        Thread.Sleep(100);
                    }
                    row.Cells[2].Value = vm.power_state.ToString();
                }

                //autoplug
                if (AutoPlug)
                {
                    VIF.plug(session, vifRef);
                }

                //write to xenstore
                row.Cells[4].Value = "Writing to xenstore...";
                WriteXenStore(session, vmRef, "vm-data/himn_mac", MAC);
                logger.WriteLine("xenstore written");

                row.Cells[4].Value = string.Format("Added as VIF '{0}' with MAC '{1}'. ",
                    vif.device, MAC);
            }

            row.Cells[5] = new DataGridViewTextBoxCell();
            while (!row.Cells[5].Displayed)
            {
                Thread.Sleep(100);
            }
            //row.Cells[5].Value = "";
            row.Cells[5].ReadOnly = true;
            form.CheckedCounter -= 1;

            if (form.CheckedCounter <= 0)
            {
                form.btnAdd.Enabled = true;
                threads.Clear();
            }
        }

        static void btnAdd_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;

            HIMNForm form = button.FindForm() as HIMNForm;
            for (int i = 0; i < form.dgv_vms.Rows.Count; i++)
            {
                DataGridViewRow row = form.dgv_vms.Rows[i];
                DataGridViewCheckBoxCell checkbox = row.Cells[5] as DataGridViewCheckBoxCell;
                if (checkbox != null && (bool)checkbox.Value == true)
                {
                    form.CheckedCounter += 1;
                    checkbox.ReadOnly = true;

                    Thread thread = new Thread(new ParameterizedThreadStart(AddHIMN));
                    thread.Start(new object[] { form, i });
                    threads.Add(thread);
                }
            }

            if (form.CheckedCounter > 0)
            {
                button.Enabled = false;
            }
        }

        #endregion

        static List<Thread> threads = new List<Thread>();

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                HIMNForm form = new HIMNForm();
                form.btnAdd.Click += btnAdd_Click;
                form.btnRemove.Click += btnRemove_Click;
                if (File.Exists(APP_ICON))
                {
                    form.Icon = new Icon(APP_ICON);
                }

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                if (args.Length < 4 || args.Length % 4 != 0)
                {
                    MessageBox.Show(string.Format("Invalid paramenter length: {0}", args.Length));
                    return;
                }

                string logroot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LOG_ROOT);
                if (!Directory.Exists(logroot))
                {
                    Directory.CreateDirectory(logroot);
                }

                for (int i = 0; i < args.Length; i += 4)
                {
                    string url = args[i];
                    string sessionRef = args[i + 1];
                    string cls = args[i + 2];
                    string vm_uuid = args[i + 3];

                    if (cls != "VM")
                    {
                        continue;
                    }

                    string logfile = DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + vm_uuid;
                    string logpath = Path.Combine(logroot, logfile + ".log");

                    form.urls.Add(url);
                    form.sessionRefs.Add(sessionRef);
                    form.vm_uuids.Add(vm_uuid);
                    form.logpaths.Add(logpath);

                    int n = form.dgv_vms.Rows.Add(
                        "Connecting...", "Discovering...", "Unknown", "Unknown", "Detecting status...", "");
                    DataGridViewRow row = form.dgv_vms.Rows[n];

                    Thread thread = new Thread(new ParameterizedThreadStart(DetectStatus));
                    thread.Start(new object[] { form, n });
                    threads.Add(thread);
                }

                Application.EnableVisualStyles();
                //Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(form);

                foreach (Thread thread in threads)
                {
                    try
                    {
                        thread.Abort();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                string logroot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LOG_ROOT);
                if (!Directory.Exists(logroot))
                {
                    Directory.CreateDirectory(logroot);
                }
                string errorlog = Path.Combine(logroot, "error.log");
                using (StreamWriter writer = new StreamWriter(errorlog, true))
                {
                    writer.WriteLine("----------------------");
                    writer.WriteLine(DateTime.Now.ToString());
                    writer.WriteLine();
                    writer.WriteLine(ex.ToString());
                }
            }

        }


    }
}
