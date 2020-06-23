using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HexDiff
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            dynamicTabControl1.TabPages.RemoveAt(1);
            dynamicTabControl1.TabPages[0].Text = "Untitled";
        }

        private void frmMain_Activated(object sender, EventArgs e)
        {
            hexEdit1.Focus();
        }
    }
}
