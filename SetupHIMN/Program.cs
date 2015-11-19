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
using System.Security.Principal;

namespace HIMN
{
    static class Program
    {
        const string LOG_ROOT = "XCHIMN.log";
        const string APP_ICON = "AppIcon.ico";
        const string HIMN_MAC = "vm-data/himn_mac";
        const string HIMN_NAME_LABEL = "Host internal management network";

        #region Business Code
        static VIF getVIF(Session session, string netRef, VM vm)
        {
            List<XenRef<VIF>> vifRefs = Network.get_VIFs(session, netRef);

            foreach (XenRef<VIF> vifRef in vifRefs)
            {
                foreach (XenRef<VIF> vifRef2 in vm.VIFs)
                {
                    if (vifRef.opaque_ref == vifRef2.opaque_ref)
                    {
                        return VIF.get_record(session, vifRef.opaque_ref);
                    }
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

        private static void SetXenStore(Session session, string vmRef, string key, string value)
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

        private static void RemoveXenStore(Session session, string vmRef, string key)
        {
            Dictionary<string, string> xenstore_data = VM.get_xenstore_data(session, vmRef);
            if (xenstore_data.ContainsKey(key))
            {
                xenstore_data.Remove(key);
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
            DataGridViewRow row = form.dgv_vms.Rows[i];

            try
            {
                Log(vm_uuid, "DetectStatus");

                //params
                Log(vm_uuid, string.Format("url: {0}", url));
                Log(vm_uuid, string.Format("sessionRef: {0}", sessionRef));
                Log(vm_uuid, string.Format("vm_uuid: {0}", vm_uuid));

                //session
                Session session = new Session(url, sessionRef);
                Log(vm_uuid, "session created");

                //host
                Host host = Host.get_record(session, session.get_this_host());
                Log(vm_uuid, "host: " + host.name_label);
                row.Cells[0].Value = host.name_label;

                //vm
                XenRef<VM> _vm = VM.get_by_uuid(session, vm_uuid);
                string vmRef = _vm.opaque_ref;
                VM vm = VM.get_record(session, vmRef);
                Log(vm_uuid, "vm:" + vm.name_label);
                row.Cells[1].Value = vm.name_label;

                //power_state
                Log(vm_uuid, "power_state:" + vm.power_state);
                row.Cells[2].Value = vm.power_state.ToString();

                //pv installed
                bool pvInstalled = GetPVInstalled(session, vm);
                Log(vm_uuid, "pv_installed:" + pvInstalled);
                if (vm.power_state == vm_power_state.Running)
                {
                    row.Cells[3].Value = pvInstalled ? "Installed" : "Not installed";
                }
                else
                {
                    row.Cells[3].Value = "Unknown";
                }

                //himn exists
                XenRef<Network> _network = Network.get_by_name_label(session, HIMN_NAME_LABEL)[0];
                string netRef = _network.opaque_ref;
                VIF vif = getVIF(session, netRef, vm);

                bool HIMNExists = (vif != null);
                Log(vm_uuid, "himn_exists:" + HIMNExists);
                form.himn_states[i] = HIMNExists;
                if (HIMNExists)
                {
                    row.Cells[4].Value = string.Format(
                        "Already added as VIF '{0}' with MAC '{1}'. ",
                        vif.device, vif.MAC);
                }
                else
                {
                    bool RebootRequired = (vm.power_state != vm_power_state.Halted && !pvInstalled);
                    row.Cells[4].Value = "Ready. " +
                        (RebootRequired ? "Requires reboot." : "No reboot required.");

                    row.Cells[5].Value = true;
                }
            }
            catch (Exception ex)
            {
                Log(vm_uuid, ex.ToString());
                row.Cells[4].Value = ex.ToString();
            }
            finally
            {
                form.CheckedCounter -= 1;
                if (form.CheckedCounter == 0)
                {
                    form.btnAdd.Enabled = true;
                    form.btnRemove.Enabled = true;
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
            DataGridViewRow row = form.dgv_vms.Rows[i];

            try
            {
                DataGridViewCheckBoxCell checkbox = row.Cells[5] as DataGridViewCheckBoxCell;

                Log(vm_uuid, "Add HIMN");

                //params
                Log(vm_uuid, string.Format("url: {0}", url));
                Log(vm_uuid, string.Format("sessionRef: {0}", sessionRef));
                Log(vm_uuid, string.Format("vm_uuid: {0}", vm_uuid));

                if (form.himn_states[i])
                {
                    Log(vm_uuid, "himn exists");
                }
                else
                {
                    row.Cells[4].Value = "Adding internal management network...";

                    //session
                    Session session = new Session(url, sessionRef);
                    Log(vm_uuid, "session created");

                    //vm
                    XenRef<VM> _vm = VM.get_by_uuid(session, vm_uuid);
                    string vmRef = _vm.opaque_ref;
                    VM vm = VM.get_record(session, vmRef);
                    Log(vm_uuid, "vm:" + vm.name_label);

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
                    XenRef<Network> _network = Network.get_by_name_label(session, HIMN_NAME_LABEL)[0];
                    string netRef = _network.opaque_ref;
                    string vifRef = createVIF(session, netRef, vmRef, device);
                    VIF vif = VIF.get_record(session, vifRef);
                    Log(vm_uuid, string.Format("vif {0} created", vifRef));
                    string MAC = vif.MAC;
                    Log(vm_uuid, string.Format("himn_mac: {0}", MAC));

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
                    SetXenStore(session, vmRef, HIMN_MAC, MAC);
                    Log(vm_uuid, "xenstore written");

                    row.Cells[4].Value = string.Format("Added as VIF '{0}' with MAC '{1}'. ",
                        vif.device, MAC);
                    form.himn_states[i] = true;

                    row.Cells[5].Value = false;
                }
            }
            catch (Exception ex)
            {
                Log(vm_uuid, ex.ToString());
                row.Cells[4].Value = ex.ToString();
            }
            finally
            {
                form.CheckedCounter -= 1;
                if (form.CheckedCounter == 0)
                {
                    form.btnAdd.Enabled = true;
                    form.btnRemove.Enabled = true;
                    threads.Clear();
                }
            }
        }

        static void RemoveHIMN(object obj)
        {
            object[] args = (object[])obj;

            HIMNForm form = args[0] as HIMNForm;
            int i = (int)args[1];

            string url = form.urls[i];
            string sessionRef = form.sessionRefs[i];
            string vm_uuid = form.vm_uuids[i];
            DataGridViewRow row = form.dgv_vms.Rows[i];

            try
            {
                DataGridViewCheckBoxCell checkbox = row.Cells[5] as DataGridViewCheckBoxCell;

                Log(vm_uuid, "Remove HIMN");

                if (!form.himn_states[i])
                {
                    Log(vm_uuid, "himn doesn't exist");
                }
                else
                {
                    //params
                    Log(vm_uuid, string.Format("url: {0}", url));
                    Log(vm_uuid, string.Format("sessionRef: {0}", sessionRef));
                    Log(vm_uuid, string.Format("vm_uuid: {0}", vm_uuid));

                    row.Cells[4].Value = "Removing internal management network...";
                    //session
                    Session session = new Session(url, sessionRef);
                    Log(vm_uuid, "session created");

                    //vm
                    XenRef<VM> _vm = VM.get_by_uuid(session, vm_uuid);
                    string vmRef = _vm.opaque_ref;
                    VM vm = VM.get_record(session, vmRef);
                    Log(vm_uuid, "vm:" + vm.name_label);

                    bool RebootRequired = (vm.power_state != vm_power_state.Halted);
                    bool pvInstalled = GetPVInstalled(session, vm);

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
                        row.Cells[4].Value = "Removing internal management network...";
                    }

                    //remove himn
                    XenRef<Network> _network = Network.get_by_name_label(session, HIMN_NAME_LABEL)[0];
                    string netRef = _network.opaque_ref;
                    VIF vif = getVIF(session, netRef, vm);
                    XenRef<VIF> _vif = VIF.get_by_uuid(session, vif.uuid);
                    if (_vif != null)
                    {
                        VIF.destroy(session, _vif.opaque_ref);
                    }
                    Log(vm_uuid, string.Format("vif {0} destroyed", vif.device));

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

                    //write to xenstore
                    row.Cells[4].Value = "Removing from xenstore...";
                    RemoveXenStore(session, vmRef, HIMN_MAC);
                    Log(vm_uuid, "xenstore removed");

                    row.Cells[4].Value = "Removed";
                    form.himn_states[i] = false;
                    row.Cells[5].Value = false;
                }
            }
            catch (Exception ex)
            {
                Log(vm_uuid, ex.ToString());
                row.Cells[4].Value = ex.ToString();
            }
            finally
            {
                form.CheckedCounter -= 1;
                if (form.CheckedCounter == 0)
                {
                    form.btnAdd.Enabled = true;
                    form.btnRemove.Enabled = true;
                    threads.Clear();
                }
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

                    Thread thread = new Thread(new ParameterizedThreadStart(AddHIMN));
                    thread.Start(new object[] { form, i });
                    threads.Add(thread);
                }
            }
            if (form.CheckedCounter > 0)
            {
                form.btnAdd.Enabled = false;
                form.btnRemove.Enabled = false;
            }

            form.btnClose.Focus();
        }

        static void btnRemove_Click(object sender, EventArgs e)
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

                    Thread thread = new Thread(new ParameterizedThreadStart(RemoveHIMN));
                    thread.Start(new object[] { form, i });
                    threads.Add(thread);
                }
            }
            if (form.CheckedCounter > 0)
            {
                form.btnAdd.Enabled = false;
                form.btnRemove.Enabled = false;
            }

            form.btnClose.Focus();
        }
        #endregion

        static List<Thread> threads = new List<Thread>();

        static StreamWriter logger = null;

        static void Log(string vm, string msg)
        {
            if (logger != null && logger.BaseStream != null)
            {
                logger.WriteLine(string.Format("{0}\t[{1}]\t{2}",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), vm, msg));
            }
        }

        static void Log(string msg)
        {
            Log("Main", msg);
        }

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                logger = new StreamWriter(Path.Combine(Path.GetTempPath(), LOG_ROOT), true) { AutoFlush = true };

                HIMNForm form = new HIMNForm();
                form.btnAdd.Click += btnAdd_Click;
                form.btnRemove.Click += btnRemove_Click;
                if (File.Exists(APP_ICON))
                {
                    form.Icon = new Icon(APP_ICON);
                }

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

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

                    form.urls.Add(url);
                    form.sessionRefs.Add(sessionRef);
                    form.vm_uuids.Add(vm_uuid);
                    form.himn_states.Add(false);

                    int n = form.dgv_vms.Rows.Add(
                        "Connecting...", "Discovering...", "Detecting...", "Detecting...", "Detecting status...", false);
                    DataGridViewRow row = form.dgv_vms.Rows[n];

                    Thread thread = new Thread(new ParameterizedThreadStart(DetectStatus));
                    thread.Start(new object[] { form, n });
                    threads.Add(thread);
                }
                form.CheckedCounter = form.dgv_vms.RowCount;

                Application.EnableVisualStyles();
                //Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(form);

                foreach (Thread thread in threads)
                {
                    thread.Abort();
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
            finally
            {
                try
                {
                    if (logger != null && logger.BaseStream != null)
                    {
                        logger.Close();
                    }
                }
                catch (Exception)
                {
                }
            }
        }



    }
}
