using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace HIMN
{
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

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }

}
