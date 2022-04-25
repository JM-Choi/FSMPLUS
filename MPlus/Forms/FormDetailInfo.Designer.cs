namespace FSMPlus.Forms
{
    partial class FormDetailInfo
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.dataGridViewPart = new System.Windows.Forms.DataGridView();
            this.panel2 = new System.Windows.Forms.Panel();
            this.buttonStopMag = new System.Windows.Forms.Button();
            this.buttonResumeMag = new System.Windows.Forms.Button();
            this.buttonInsertMag = new System.Windows.Forms.Button();
            this.buttonDeleteMag = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.dataGridViewVec = new System.Windows.Forms.DataGridView();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonReconnect = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPart)).BeginInit();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewVec)).BeginInit();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.dataGridViewPart);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.dataGridViewVec);
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Location = new System.Drawing.Point(6, 20);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(634, 269);
            this.panel1.TabIndex = 0;
            // 
            // dataGridViewPart
            // 
            this.dataGridViewPart.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridViewPart.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridViewPart.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewPart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewPart.Location = new System.Drawing.Point(0, 98);
            this.dataGridViewPart.MultiSelect = false;
            this.dataGridViewPart.Name = "dataGridViewPart";
            this.dataGridViewPart.RowHeadersVisible = false;
            this.dataGridViewPart.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dataGridViewPart.RowTemplate.Height = 17;
            this.dataGridViewPart.RowTemplate.ReadOnly = true;
            this.dataGridViewPart.Size = new System.Drawing.Size(634, 171);
            this.dataGridViewPart.TabIndex = 11;
            this.dataGridViewPart.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewPart_CellClick);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.DimGray;
            this.panel2.Controls.Add(this.buttonReconnect);
            this.panel2.Controls.Add(this.buttonStopMag);
            this.panel2.Controls.Add(this.buttonResumeMag);
            this.panel2.Controls.Add(this.buttonInsertMag);
            this.panel2.Controls.Add(this.buttonDeleteMag);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.panel2.ForeColor = System.Drawing.Color.White;
            this.panel2.Location = new System.Drawing.Point(0, 75);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(634, 23);
            this.panel2.TabIndex = 10;
            // 
            // buttonStopMag
            // 
            this.buttonStopMag.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStopMag.BackColor = System.Drawing.Color.Crimson;
            this.buttonStopMag.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonStopMag.Location = new System.Drawing.Point(298, 0);
            this.buttonStopMag.Name = "buttonStopMag";
            this.buttonStopMag.Size = new System.Drawing.Size(70, 22);
            this.buttonStopMag.TabIndex = 16;
            this.buttonStopMag.Text = "Stop";
            this.buttonStopMag.UseVisualStyleBackColor = false;
            this.buttonStopMag.Click += new System.EventHandler(this.buttonStopMag_Click);
            // 
            // buttonResumeMag
            // 
            this.buttonResumeMag.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonResumeMag.BackColor = System.Drawing.Color.LimeGreen;
            this.buttonResumeMag.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonResumeMag.Location = new System.Drawing.Point(394, 0);
            this.buttonResumeMag.Name = "buttonResumeMag";
            this.buttonResumeMag.Size = new System.Drawing.Size(70, 22);
            this.buttonResumeMag.TabIndex = 15;
            this.buttonResumeMag.Text = "Resume";
            this.buttonResumeMag.UseVisualStyleBackColor = false;
            this.buttonResumeMag.Click += new System.EventHandler(this.buttonResumeMag_Click);
            // 
            // buttonInsertMag
            // 
            this.buttonInsertMag.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonInsertMag.BackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonInsertMag.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonInsertMag.Location = new System.Drawing.Point(106, 0);
            this.buttonInsertMag.Name = "buttonInsertMag";
            this.buttonInsertMag.Size = new System.Drawing.Size(70, 22);
            this.buttonInsertMag.TabIndex = 14;
            this.buttonInsertMag.Text = "Insert";
            this.buttonInsertMag.UseVisualStyleBackColor = false;
            this.buttonInsertMag.Click += new System.EventHandler(this.buttonInsertMag_Click);
            // 
            // buttonDeleteMag
            // 
            this.buttonDeleteMag.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDeleteMag.BackColor = System.Drawing.Color.Salmon;
            this.buttonDeleteMag.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonDeleteMag.Location = new System.Drawing.Point(202, 0);
            this.buttonDeleteMag.Name = "buttonDeleteMag";
            this.buttonDeleteMag.Size = new System.Drawing.Size(70, 22);
            this.buttonDeleteMag.TabIndex = 13;
            this.buttonDeleteMag.Text = "Delete";
            this.buttonDeleteMag.UseVisualStyleBackColor = false;
            this.buttonDeleteMag.Click += new System.EventHandler(this.buttonDeleteMag_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Partition Info";
            // 
            // dataGridViewVec
            // 
            this.dataGridViewVec.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridViewVec.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewVec.Dock = System.Windows.Forms.DockStyle.Top;
            this.dataGridViewVec.Location = new System.Drawing.Point(0, 20);
            this.dataGridViewVec.Name = "dataGridViewVec";
            this.dataGridViewVec.RowHeadersVisible = false;
            this.dataGridViewVec.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dataGridViewVec.RowTemplate.Height = 17;
            this.dataGridViewVec.RowTemplate.ReadOnly = true;
            this.dataGridViewVec.Size = new System.Drawing.Size(634, 55);
            this.dataGridViewVec.TabIndex = 9;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.DimGray;
            this.panel3.Controls.Add(this.label3);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.panel3.ForeColor = System.Drawing.Color.White;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(634, 20);
            this.panel3.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "Vehicle";
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.BackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonClose.Location = new System.Drawing.Point(609, -2);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(30, 20);
            this.buttonClose.TabIndex = 12;
            this.buttonClose.Text = "X";
            this.buttonClose.UseVisualStyleBackColor = false;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonReconnect
            // 
            this.buttonReconnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReconnect.BackColor = System.Drawing.Color.Plum;
            this.buttonReconnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonReconnect.Location = new System.Drawing.Point(490, 0);
            this.buttonReconnect.Name = "buttonReconnect";
            this.buttonReconnect.Size = new System.Drawing.Size(87, 22);
            this.buttonReconnect.TabIndex = 17;
            this.buttonReconnect.Text = "Reconnect";
            this.buttonReconnect.UseVisualStyleBackColor = false;
            this.buttonReconnect.Click += new System.EventHandler(this.buttonReconnect_Click);
            // 
            // FormDetailInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Indigo;
            this.ClientSize = new System.Drawing.Size(644, 295);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormDetailInfo";
            this.Text = "FormDetailInfo";
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPart)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewVec)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dataGridViewVec;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonInsertMag;
        private System.Windows.Forms.Button buttonDeleteMag;
        public System.Windows.Forms.DataGridView dataGridViewPart;
        public System.Windows.Forms.Button buttonResumeMag;
        public System.Windows.Forms.Button buttonStopMag;
        public System.Windows.Forms.Button buttonReconnect;
    }
}