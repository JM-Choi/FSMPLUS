namespace FSMPlus.Forms
{
    partial class PgeLog
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.logDb = new System.Windows.Forms.RichTextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.logRv = new System.Windows.Forms.RichTextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.logDebug = new System.Windows.Forms.RichTextBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.logComm = new System.Windows.Forms.RichTextBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Alignment = System.Windows.Forms.TabAlignment.Right;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(447, 355);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.logDb);
            this.tabPage1.Location = new System.Drawing.Point(4, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(416, 347);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // logDb
            // 
            this.logDb.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.logDb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logDb.Location = new System.Drawing.Point(3, 3);
            this.logDb.Name = "logDb";
            this.logDb.Size = new System.Drawing.Size(410, 341);
            this.logDb.TabIndex = 0;
            this.logDb.Text = "";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.logRv);
            this.tabPage2.Location = new System.Drawing.Point(4, 4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(416, 347);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // logRv
            // 
            this.logRv.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.logRv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logRv.Location = new System.Drawing.Point(3, 3);
            this.logRv.Name = "logRv";
            this.logRv.Size = new System.Drawing.Size(410, 341);
            this.logRv.TabIndex = 0;
            this.logRv.Text = "";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.logDebug);
            this.tabPage3.Location = new System.Drawing.Point(4, 4);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(416, 347);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // logDebug
            // 
            this.logDebug.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.logDebug.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logDebug.Location = new System.Drawing.Point(3, 3);
            this.logDebug.Name = "logDebug";
            this.logDebug.Size = new System.Drawing.Size(410, 341);
            this.logDebug.TabIndex = 0;
            this.logDebug.Text = "";
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.logComm);
            this.tabPage4.Location = new System.Drawing.Point(4, 4);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(416, 347);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "tabPage4";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // logComm
            // 
            this.logComm.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.logComm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logComm.Location = new System.Drawing.Point(3, 3);
            this.logComm.Name = "logComm";
            this.logComm.Size = new System.Drawing.Size(410, 341);
            this.logComm.TabIndex = 0;
            this.logComm.Text = "";
            // 
            // PgeLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(447, 355);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("맑은 고딕", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "PgeLog";
            this.Text = "PgeLog";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.RichTextBox logDb;
        private System.Windows.Forms.RichTextBox logRv;
        private System.Windows.Forms.RichTextBox logDebug;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.RichTextBox logComm;
    }
}