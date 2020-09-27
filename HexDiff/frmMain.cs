using System;
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

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                hexDiffView1.loadHexViewFromFile(openFileDlg.FileName);
            }
        }
    }
}
