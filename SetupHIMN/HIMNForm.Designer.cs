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
            this.pv_installed = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Selected = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_vms)).BeginInit();
            this.SuspendLayout();
            // 
            // dgv_vms
            // 
            this.dgv_vms.AllowUserToAddRows = false;
            this.dgv_vms.AllowUserToDeleteRows = false;
            this.dgv_vms.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_vms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_vms.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.XenServer,
            this.VM,
            this.PowerState,
            this.pv_installed,
            this.Status,
            this.Selected});
            this.dgv_vms.Location = new System.Drawing.Point(0, 0);
            this.dgv_vms.Name = "dgv_vms";
            this.dgv_vms.RowHeadersVisible = false;
            this.dgv_vms.Size = new System.Drawing.Size(784, 228);
            this.dgv_vms.TabIndex = 0;
            // 
            // XenServer
            // 
            this.XenServer.HeaderText = "XenServer";
            this.XenServer.Name = "XenServer";
            this.XenServer.ReadOnly = true;
            this.XenServer.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // VM
            // 
            this.VM.HeaderText = "VM";
            this.VM.Name = "VM";
            this.VM.ReadOnly = true;
            this.VM.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // PowerState
            // 
            this.PowerState.FillWeight = 70F;
            this.PowerState.HeaderText = "Power State";
            this.PowerState.Name = "PowerState";
            this.PowerState.ReadOnly = true;
            this.PowerState.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // pv_installed
            // 
            this.pv_installed.HeaderText = "PV";
            this.pv_installed.Name = "pv_installed";
            this.pv_installed.ReadOnly = true;
            this.pv_installed.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.pv_installed.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Status
            // 
            this.Status.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Status.HeaderText = "Status";
            this.Status.Name = "Status";
            this.Status.ReadOnly = true;
            this.Status.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Selected
            // 
            this.Selected.HeaderText = "";
            this.Selected.Name = "Selected";
            this.Selected.ReadOnly = true;
            this.Selected.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Selected.Width = 30;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClose.Location = new System.Drawing.Point(0, 231);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(87, 28);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Enabled = false;
            this.btnAdd.Location = new System.Drawing.Point(660, 231);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(124, 28);
            this.btnAdd.TabIndex = 2;
            this.btnAdd.Text = "Add Internal Network";
            this.btnAdd.UseVisualStyleBackColor = true;
            // 
            // HIMNForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 262);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.dgv_vms);
            this.Name = "HIMNForm";
            this.Text = "Internal Management Network Tool";
            ((System.ComponentModel.ISupportInitialize)(this.dgv_vms)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.DataGridView dgv_vms;
        private System.Windows.Forms.Button btnClose;
        public System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.DataGridViewTextBoxColumn XenServer;
        private System.Windows.Forms.DataGridViewTextBoxColumn VM;
        private System.Windows.Forms.DataGridViewTextBoxColumn PowerState;
        private System.Windows.Forms.DataGridViewTextBoxColumn pv_installed;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status;
        private System.Windows.Forms.DataGridViewTextBoxColumn Selected;

    }
}