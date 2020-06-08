namespace HexDiff
{
    partial class frmMain
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
            this.dynamicTabControl1 = new DynamicTabControl.DynamicTabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.hexEdit1 = new HexEdit.HexEdit();
            this.dynamicTabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dynamicTabControl1
            // 
            this.dynamicTabControl1.AllowDrop = true;
            this.dynamicTabControl1.Controls.Add(this.tabPage1);
            this.dynamicTabControl1.Location = new System.Drawing.Point(0, 12);
            this.dynamicTabControl1.Name = "dynamicTabControl1";
            this.dynamicTabControl1.SelectedIndex = 0;
            this.dynamicTabControl1.Size = new System.Drawing.Size(483, 268);
            this.dynamicTabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.hexEdit1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(475, 242);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // hexEdit1
            // 
            this.hexEdit1.CaretAt = 0;
            this.hexEdit1.FontSize = 12;
            this.hexEdit1.HorzHexCount = 16;
            this.hexEdit1.Location = new System.Drawing.Point(0, 0);
            this.hexEdit1.Name = "hexEdit1";
            this.hexEdit1.Size = new System.Drawing.Size(430, 202);
            this.hexEdit1.TabIndex = 0;
            this.hexEdit1.VertHexCount = 8;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(615, 292);
            this.Controls.Add(this.dynamicTabControl1);
            this.Name = "frmMain";
            this.Text = "HexDiff";
            this.dynamicTabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DynamicTabControl.DynamicTabControl dynamicTabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private HexEdit.HexEdit hexEdit1;
    }
}