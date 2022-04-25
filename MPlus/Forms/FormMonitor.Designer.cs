namespace FSMPlus.Forms
{
    partial class FormMonitor
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
            this.components = new System.ComponentModel.Container();
            this.panel4 = new System.Windows.Forms.Panel();
            this.buttonAlarmClear = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelWindowtime = new System.Windows.Forms.Label();
            this.labelExecutetime = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonCmdDel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.dataGridViewAlarm = new System.Windows.Forms.DataGridView();
            this.dataGridViewCmd = new System.Windows.Forms.DataGridView();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panelVecList = new System.Windows.Forms.Panel();
            this.panel4.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAlarm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewCmd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.DimGray;
            this.panel4.Controls.Add(this.buttonAlarmClear);
            this.panel4.Controls.Add(this.label4);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.panel4.ForeColor = System.Drawing.Color.White;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(843, 39);
            this.panel4.TabIndex = 9;
            // 
            // buttonAlarmClear
            // 
            this.buttonAlarmClear.BackColor = System.Drawing.Color.OrangeRed;
            this.buttonAlarmClear.FlatAppearance.BorderSize = 0;
            this.buttonAlarmClear.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonAlarmClear.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.buttonAlarmClear.ForeColor = System.Drawing.Color.White;
            this.buttonAlarmClear.Location = new System.Drawing.Point(144, 5);
            this.buttonAlarmClear.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.buttonAlarmClear.Name = "buttonAlarmClear";
            this.buttonAlarmClear.Size = new System.Drawing.Size(115, 30);
            this.buttonAlarmClear.TabIndex = 4;
            this.buttonAlarmClear.Text = "Delete Alarm";
            this.buttonAlarmClear.UseVisualStyleBackColor = false;
            this.buttonAlarmClear.Click += new System.EventHandler(this.buttonAlarmClear_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "Alarm List";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.DimGray;
            this.panel1.Controls.Add(this.labelWindowtime);
            this.panel1.Controls.Add(this.labelExecutetime);
            this.panel1.Controls.Add(this.buttonCancel);
            this.panel1.Controls.Add(this.buttonCmdDel);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.panel1.ForeColor = System.Drawing.Color.White;
            this.panel1.Location = new System.Drawing.Point(0, 199);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(843, 39);
            this.panel1.TabIndex = 11;
            // 
            // labelWindowtime
            // 
            this.labelWindowtime.AutoSize = true;
            this.labelWindowtime.Location = new System.Drawing.Point(592, 15);
            this.labelWindowtime.Name = "labelWindowtime";
            this.labelWindowtime.Size = new System.Drawing.Size(88, 12);
            this.labelWindowtime.TabIndex = 5;
            this.labelWindowtime.Text = "WindowTime";
            // 
            // labelExecutetime
            // 
            this.labelExecutetime.AutoSize = true;
            this.labelExecutetime.Location = new System.Drawing.Point(463, 15);
            this.labelExecutetime.Name = "labelExecutetime";
            this.labelExecutetime.Size = new System.Drawing.Size(91, 12);
            this.labelExecutetime.TabIndex = 4;
            this.labelExecutetime.Text = "ExecuteTime";
            // 
            // buttonCancel
            // 
            this.buttonCancel.BackColor = System.Drawing.Color.Blue;
            this.buttonCancel.FlatAppearance.BorderSize = 0;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.buttonCancel.ForeColor = System.Drawing.Color.White;
            this.buttonCancel.Location = new System.Drawing.Point(259, 5);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(115, 30);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = false;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancelAbort_Click);
            // 
            // buttonCmdDel
            // 
            this.buttonCmdDel.BackColor = System.Drawing.Color.OrangeRed;
            this.buttonCmdDel.FlatAppearance.BorderSize = 0;
            this.buttonCmdDel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCmdDel.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.buttonCmdDel.ForeColor = System.Drawing.Color.White;
            this.buttonCmdDel.Location = new System.Drawing.Point(144, 5);
            this.buttonCmdDel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.buttonCmdDel.Name = "buttonCmdDel";
            this.buttonCmdDel.Size = new System.Drawing.Size(115, 30);
            this.buttonCmdDel.TabIndex = 2;
            this.buttonCmdDel.Text = "Delete Command";
            this.buttonCmdDel.UseVisualStyleBackColor = false;
            this.buttonCmdDel.Click += new System.EventHandler(this.buttonCmdDel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Command List";
            // 
            // dataGridViewAlarm
            // 
            this.dataGridViewAlarm.AllowUserToAddRows = false;
            this.dataGridViewAlarm.AllowUserToDeleteRows = false;
            this.dataGridViewAlarm.AllowUserToOrderColumns = true;
            this.dataGridViewAlarm.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridViewAlarm.BackgroundColor = System.Drawing.Color.White;
            this.dataGridViewAlarm.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewAlarm.Dock = System.Windows.Forms.DockStyle.Top;
            this.dataGridViewAlarm.Location = new System.Drawing.Point(0, 39);
            this.dataGridViewAlarm.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dataGridViewAlarm.MultiSelect = false;
            this.dataGridViewAlarm.Name = "dataGridViewAlarm";
            this.dataGridViewAlarm.ReadOnly = true;
            this.dataGridViewAlarm.RowHeadersVisible = false;
            this.dataGridViewAlarm.RowTemplate.Height = 15;
            this.dataGridViewAlarm.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewAlarm.Size = new System.Drawing.Size(843, 160);
            this.dataGridViewAlarm.TabIndex = 14;
            this.dataGridViewAlarm.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewAlarm_CellClick);
            // 
            // dataGridViewCmd
            // 
            this.dataGridViewCmd.AllowUserToAddRows = false;
            this.dataGridViewCmd.AllowUserToDeleteRows = false;
            this.dataGridViewCmd.AllowUserToOrderColumns = true;
            this.dataGridViewCmd.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridViewCmd.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridViewCmd.BackgroundColor = System.Drawing.Color.White;
            this.dataGridViewCmd.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewCmd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewCmd.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridViewCmd.Location = new System.Drawing.Point(0, 238);
            this.dataGridViewCmd.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dataGridViewCmd.MultiSelect = false;
            this.dataGridViewCmd.Name = "dataGridViewCmd";
            this.dataGridViewCmd.RowHeadersVisible = false;
            this.dataGridViewCmd.RowTemplate.Height = 15;
            this.dataGridViewCmd.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewCmd.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewCmd.Size = new System.Drawing.Size(843, 146);
            this.dataGridViewCmd.TabIndex = 15;
            this.dataGridViewCmd.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewCmd_CellClick);
            this.dataGridViewCmd.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dataGridViewCmd_CellFormatting);
            this.dataGridViewCmd.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dataGridViewCmd_DataError);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panelVecList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dataGridViewCmd);
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Panel2.Controls.Add(this.dataGridViewAlarm);
            this.splitContainer1.Panel2.Controls.Add(this.panel4);
            this.splitContainer1.Size = new System.Drawing.Size(843, 614);
            this.splitContainer1.SplitterDistance = 221;
            this.splitContainer1.SplitterWidth = 9;
            this.splitContainer1.TabIndex = 0;
            // 
            // panelVecList
            // 
            this.panelVecList.AutoScroll = true;
            this.panelVecList.BackColor = System.Drawing.Color.Gray;
            this.panelVecList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelVecList.Location = new System.Drawing.Point(0, 0);
            this.panelVecList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelVecList.Name = "panelVecList";
            this.panelVecList.Size = new System.Drawing.Size(843, 221);
            this.panelVecList.TabIndex = 20;
            // 
            // FormMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(843, 614);
            this.Controls.Add(this.splitContainer1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FormMonitor";
            this.Load += new System.EventHandler(this.FormMonitor_Load);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAlarm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewCmd)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dataGridViewAlarm;
        private System.Windows.Forms.DataGridView dataGridViewCmd;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button buttonAlarmClear;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panelVecList;
        private System.Windows.Forms.Label labelWindowtime;
        private System.Windows.Forms.Label labelExecutetime;
        public System.Windows.Forms.Button buttonCmdDel;
        public System.Windows.Forms.Button buttonCancel;
    }
}