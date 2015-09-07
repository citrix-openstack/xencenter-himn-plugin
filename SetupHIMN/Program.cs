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
    partial class HIMNForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dgv_vms = new System.Windows.Forms.DataGridView();
            this.XenServer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PowerState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pv_installed = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Log = new System.Windows.Forms.DataGridViewLinkColumn();
            this.HIMN_Existed = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Remarks = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_vms)).BeginInit();
            this.SuspendLayout();
            // 
            // dgv_vms
            // 
            this.dgv_vms.AllowUserToAddRows = false;
            this.dgv_vms.AllowUserToDeleteRows = false;
            this.dgv_vms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_vms.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.XenServer,
            this.VM,
            this.PowerState,
            this.pv_installed,
            this.Log,
            this.HIMN_Existed,
            this.Remarks});
            this.dgv_vms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_vms.Location = new System.Drawing.Point(0, 0);
            this.dgv_vms.Name = "dgv_vms";
            this.dgv_vms.ReadOnly = true;
            this.dgv_vms.Size = new System.Drawing.Size(984, 562);
            this.dgv_vms.TabIndex = 0;
            this.dgv_vms.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_vms_CellClick);
            // 
            // XenServer
            // 
            this.XenServer.HeaderText = "XenServer";
            this.XenServer.Name = "XenServer";
            this.XenServer.ReadOnly = true;
            // 
            // VM
            // 
            this.VM.HeaderText = "VM";
            this.VM.Name = "VM";
            this.VM.ReadOnly = true;
            // 
            // PowerState
            // 
            this.PowerState.HeaderText = "Power State";
            this.PowerState.Name = "PowerState";
            this.PowerState.ReadOnly = true;
            // 
            // pv_installed
            // 
            this.pv_installed.HeaderText = "PV";
            this.pv_installed.Name = "pv_installed";
            this.pv_installed.ReadOnly = true;
            this.pv_installed.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.pv_installed.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.pv_installed.Width = 50;
            // 
            // Log
            // 
            this.Log.HeaderText = "Log";
            this.Log.LinkBehavior = System.Windows.Forms.LinkBehavior.AlwaysUnderline;
            this.Log.Name = "Log";
            this.Log.ReadOnly = true;
            this.Log.Text = "Log";
            // 
            // HIMN_Existed
            // 
            this.HIMN_Existed.HeaderText = "Existed";
            this.HIMN_Existed.Name = "HIMN_Existed";
            this.HIMN_Existed.ReadOnly = true;
            this.HIMN_Existed.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.HIMN_Existed.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.HIMN_Existed.Width = 50;
            // 
            // Remarks
            // 
            this.Remarks.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Remarks.HeaderText = "Remarks";
            this.Remarks.Name = "Remarks";
            this.Remarks.ReadOnly = true;
            // 
            // HIMNForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 562);
            this.Controls.Add(this.dgv_vms);
            this.Name = "HIMNForm";
            this.Text = "HIMN Tool";
            this.Load += new System.EventHandler(this.HIMNForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_vms)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.DataGridView dgv_vms;
        private System.Windows.Forms.DataGridViewTextBoxColumn XenServer;
        private System.Windows.Forms.DataGridViewTextBoxColumn VM;
        private System.Windows.Forms.DataGridViewTextBoxColumn PowerState;
        private System.Windows.Forms.DataGridViewCheckBoxColumn pv_installed;
        private System.Windows.Forms.DataGridViewLinkColumn Log;
        private System.Windows.Forms.DataGridViewCheckBoxColumn HIMN_Existed;
        private System.Windows.Forms.DataGridViewTextBoxColumn Remarks;

    }

    public partial class HIMNForm : Form
    {
        public HIMNForm()
        {
            InitializeComponent();
        }

        private void HIMNForm_Load(object sender, EventArgs e)
        {

        }

        private void dgv_vms_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex > 0)
            {
                DataGridViewCell cell = dgv_vms.Rows[e.RowIndex].Cells[e.ColumnIndex];

                if (cell.GetType() == typeof(DataGridViewLinkCell))
                {
                    string path = (string)cell.Value;
                    if (path != null)
                    {
                        Process.Start("notepad", path);
                    }
                }
            }

        }
    }

    static class Program
    {
        const string LOG_ROOT = @"Logs";

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
            row.Cells[4].Value = logpath;

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
                    VM_guest_metrics.get_PV_drivers_version(session, vm.guest_metrics);
                foreach (string k in vm_guest_metrics.Keys)
                {
                    logger.WriteLine("vm_guest_metrics/{0}: {1}", k, vm_guest_metrics[k]);
                }
                pvInstalled = (vm_guest_metrics.Keys.Count == 0);
            }
            row.Cells[3].Value = pvInstalled;

            //himn existed
            string netRef = getNetwork(session, "xenapi");
            string vifRef = getVIF(session, netRef, vmRef);
            VIF vif;
            bool himn_existed = !string.IsNullOrEmpty(vifRef);
            row.Cells[5].Value = himn_existed;
            if (himn_existed)
            {
                logger.WriteLine(string.Format("vif {0} existed", vifRef));
                vif = VIF.get_record(session, vifRef);
                row.Cells[6].Value = string.Format(
                    "Management network already existed as VIF '{0}' {1}.",
                    vif.device, vif.MAC);

            }
            else
            {
                //himn + power_state
                if (vm.power_state != vm_power_state.Halted && !pvInstalled)
                {
                    row.Cells[6].Value = string.Format(
                        "Management network need to be added when VM '{0}' is powered off. " +
                        "Please shut down VM '{0}' first then redo.", vm.name_label);
                    logger.WriteLine(string.Format("VM '{0}' need to be shutdown", vm.name_label));
                    return;
                }

                row.Cells[6].Value = string.Format("Adding network 'xenapi' to VM '{0}'...", vm.name_label);

                string device = "9";
                vifRef = createVIF(session, netRef, vmRef, device);
                vif = VIF.get_record(session, vifRef);
                logger.WriteLine(string.Format("vif {0} created", vifRef));

                row.Cells[6].Value = string.Format("Management network added as VIF '{0}' {1}, " +
                    "but not be visible in XenCenter.", vif.device, vif.MAC);
                //VIF.plug(session, vifRef);
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
                int n = form.dgv_vms.Rows.Add("Connecting..", "Discovering..", "Unknown", false, "", false, "....", null);
                DataGridViewRow row = form.dgv_vms.Rows[n];
                Thread thread = new Thread(new ParameterizedThreadStart(AddHIMNStart));
                thread.Start(new object[] { row, url, sessionRef, cls, vm_uuid });
            }
            Application.Run(form);
        }
    }
}
