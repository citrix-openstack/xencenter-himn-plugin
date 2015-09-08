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
            this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_vms)).BeginInit();
            this.SuspendLayout();
            // 
            // dgv_vms
            // 
            this.dgv_vms.AllowUserToAddRows = false;
            this.dgv_vms.AllowUserToDeleteRows = false;
            this.dgv_vms.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_vms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_vms.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.XenServer,
            this.VM,
            this.PowerState,
            this.pv_installed,
            this.Status});
            this.dgv_vms.Location = new System.Drawing.Point(0, 0);
            this.dgv_vms.Name = "dgv_vms";
            this.dgv_vms.ReadOnly = true;
            this.dgv_vms.RowHeadersVisible = false;
            this.dgv_vms.Size = new System.Drawing.Size(784, 228);
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
            this.PowerState.FillWeight = 70F;
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
            this.pv_installed.Width = 30;
            // 
            // Status
            // 
            this.Status.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Status.HeaderText = "Status";
            this.Status.Name = "Status";
            this.Status.ReadOnly = true;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(695, 231);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(87, 28);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // HIMNForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 262);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.dgv_vms);
            this.Name = "HIMNForm";
            this.Text = "Internal Management Network Tool";
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
        private System.Windows.Forms.DataGridViewTextBoxColumn Status;
        private System.Windows.Forms.Button btnClose;

    }
}