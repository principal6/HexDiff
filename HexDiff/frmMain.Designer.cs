namespace HexDiff
{
    partial class frmMain
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.dynamicTabControl1 = new HDUtility.DynamicTabControl();
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
            this.dynamicTabControl1.Location = new System.Drawing.Point(12, 12);
            this.dynamicTabControl1.Name = "dynamicTabControl1";
            this.dynamicTabControl1.SelectedIndex = 0;
            this.dynamicTabControl1.Size = new System.Drawing.Size(435, 157);
            this.dynamicTabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.hexEdit1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(427, 131);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // hexEdit1
            // 
            this.hexEdit1.AccessibleRole = System.Windows.Forms.AccessibleRole.Text;
            this.hexEdit1.BackColor = System.Drawing.Color.LightGray;
            this.hexEdit1.FontSize = 12;
            this.hexEdit1.HorzHexCount = 16;
            this.hexEdit1.Location = new System.Drawing.Point(3, 3);
            this.hexEdit1.Name = "hexEdit1";
            this.hexEdit1.SelectionStart = 0;
            this.hexEdit1.Size = new System.Drawing.Size(372, 68);
            this.hexEdit1.TabIndex = 0;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(567, 250);
            this.Controls.Add(this.dynamicTabControl1);
            this.Name = "frmMain";
            this.Text = "HexDiff";
            this.dynamicTabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private HDUtility.DynamicTabControl dynamicTabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private HexEdit.HexEdit hexEdit1;
    }
}

