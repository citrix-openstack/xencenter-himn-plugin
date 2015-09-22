using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HIMN
{
    public partial class HIMNForm : Form
    {
        public int ReadyCounter = 0;
        public int CheckedCounter = 0;

        public List<string> urls = new List<string>();
        public List<string> sessionRefs = new List<string>();
        public List<string> vm_uuids = new List<string>();
        public List<string> logpaths = new List<string>();
        public List<bool> himn_states = new List<bool>();

        //url, sessionRef, cls, vm_uuid,
        public HIMNForm()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }

}
