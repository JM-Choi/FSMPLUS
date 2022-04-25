namespace FSMPlus.Forms
{
    partial class FormOperatorAction
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.buttonDelJob = new System.Windows.Forms.Button();
            this.buttonAddJob = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxAmrCarrierId = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxEqpCarrierId = new System.Windows.Forms.TextBox();
            this.comboBoxEqp = new System.Windows.Forms.ComboBox();
            this.comboBoxVecPart = new System.Windows.Forms.ComboBox();
            this.radioButtonEqp = new System.Windows.Forms.RadioButton();
            this.radioButtonAmr = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxDestEqp = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.btn_Exit = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.dataGridViewAddJobs = new System.Windows.Forms.DataGridView();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAddJobs)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonDelJob
            // 
            this.buttonDelJob.BackColor = System.Drawing.Color.Tomato;
            this.buttonDelJob.FlatAppearance.BorderSize = 0;
            this.buttonDelJob.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonDelJob.ForeColor = System.Drawing.Color.White;
            this.buttonDelJob.Location = new System.Drawing.Point(341, 17);
            this.buttonDelJob.Name = "buttonDelJob";
            this.buttonDelJob.Size = new System.Drawing.Size(61, 53);
            this.buttonDelJob.TabIndex = 8;
            this.buttonDelJob.Text = "Del";
            this.buttonDelJob.UseVisualStyleBackColor = false;
            this.buttonDelJob.Click += new System.EventHandler(this.buttonDelJob_Click);
            // 
            // buttonAddJob
            // 
            this.buttonAddJob.BackColor = System.Drawing.Color.ForestGreen;
            this.buttonAddJob.FlatAppearance.BorderSize = 0;
            this.buttonAddJob.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonAddJob.ForeColor = System.Drawing.Color.White;
            this.buttonAddJob.Location = new System.Drawing.Point(29, 304);
            this.buttonAddJob.Name = "buttonAddJob";
            this.buttonAddJob.Size = new System.Drawing.Size(306, 57);
            this.buttonAddJob.TabIndex = 7;
            this.buttonAddJob.Text = "New";
            this.buttonAddJob.UseVisualStyleBackColor = false;
            this.buttonAddJob.Click += new System.EventHandler(this.buttonAddJob_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.comboBoxAmrCarrierId);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBoxEqpCarrierId);
            this.groupBox1.Controls.Add(this.comboBoxEqp);
            this.groupBox1.Controls.Add(this.comboBoxVecPart);
            this.groupBox1.Controls.Add(this.radioButtonEqp);
            this.groupBox1.Controls.Add(this.radioButtonAmr);
            this.groupBox1.ForeColor = System.Drawing.Color.White;
            this.groupBox1.Location = new System.Drawing.Point(29, 45);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(306, 193);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "출발지 선택";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(35, 136);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 12);
            this.label3.TabIndex = 19;
            this.label3.Text = "Carrier ID";
            this.label3.Visible = false;
            // 
            // comboBoxAmrCarrierId
            // 
            this.comboBoxAmrCarrierId.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAmrCarrierId.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.comboBoxAmrCarrierId.FormattingEnabled = true;
            this.comboBoxAmrCarrierId.Location = new System.Drawing.Point(99, 130);
            this.comboBoxAmrCarrierId.Name = "comboBoxAmrCarrierId";
            this.comboBoxAmrCarrierId.Size = new System.Drawing.Size(196, 24);
            this.comboBoxAmrCarrierId.Sorted = true;
            this.comboBoxAmrCarrierId.TabIndex = 18;
            this.comboBoxAmrCarrierId.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(35, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 12);
            this.label2.TabIndex = 17;
            this.label2.Text = "Carrier ID";
            // 
            // textBoxEqpCarrierId
            // 
            this.textBoxEqpCarrierId.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.textBoxEqpCarrierId.Location = new System.Drawing.Point(99, 59);
            this.textBoxEqpCarrierId.Name = "textBoxEqpCarrierId";
            this.textBoxEqpCarrierId.Size = new System.Drawing.Size(196, 26);
            this.textBoxEqpCarrierId.TabIndex = 16;
            // 
            // comboBoxEqp
            // 
            this.comboBoxEqp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxEqp.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.comboBoxEqp.FormattingEnabled = true;
            this.comboBoxEqp.Location = new System.Drawing.Point(99, 32);
            this.comboBoxEqp.Name = "comboBoxEqp";
            this.comboBoxEqp.Size = new System.Drawing.Size(196, 24);
            this.comboBoxEqp.Sorted = true;
            this.comboBoxEqp.TabIndex = 15;
            // 
            // comboBoxVecPart
            // 
            this.comboBoxVecPart.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxVecPart.Enabled = false;
            this.comboBoxVecPart.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.comboBoxVecPart.FormattingEnabled = true;
            this.comboBoxVecPart.Location = new System.Drawing.Point(99, 102);
            this.comboBoxVecPart.Name = "comboBoxVecPart";
            this.comboBoxVecPart.Size = new System.Drawing.Size(196, 24);
            this.comboBoxVecPart.Sorted = true;
            this.comboBoxVecPart.TabIndex = 14;
            this.comboBoxVecPart.SelectedIndexChanged += new System.EventHandler(this.comboBoxVecPart_SelectedIndexChanged);
            // 
            // radioButtonEqp
            // 
            this.radioButtonEqp.AutoSize = true;
            this.radioButtonEqp.Checked = true;
            this.radioButtonEqp.Location = new System.Drawing.Point(6, 38);
            this.radioButtonEqp.Name = "radioButtonEqp";
            this.radioButtonEqp.Size = new System.Drawing.Size(87, 16);
            this.radioButtonEqp.TabIndex = 13;
            this.radioButtonEqp.TabStop = true;
            this.radioButtonEqp.Text = "자동화 설비";
            this.radioButtonEqp.UseVisualStyleBackColor = true;
            this.radioButtonEqp.CheckedChanged += new System.EventHandler(this.radioButtonEqp_CheckedChanged);
            // 
            // radioButtonAmr
            // 
            this.radioButtonAmr.AutoSize = true;
            this.radioButtonAmr.Location = new System.Drawing.Point(6, 107);
            this.radioButtonAmr.Name = "radioButtonAmr";
            this.radioButtonAmr.Size = new System.Drawing.Size(50, 16);
            this.radioButtonAmr.TabIndex = 12;
            this.radioButtonAmr.Text = "AMR";
            this.radioButtonAmr.UseVisualStyleBackColor = true;
            this.radioButtonAmr.CheckedChanged += new System.EventHandler(this.radioButtonAmr_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.comboBoxDestEqp);
            this.groupBox2.ForeColor = System.Drawing.Color.White;
            this.groupBox2.Location = new System.Drawing.Point(29, 244);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(306, 54);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "목적지 선택";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 12);
            this.label1.TabIndex = 15;
            this.label1.Text = "자동화 설비";
            // 
            // comboBoxDestEqp
            // 
            this.comboBoxDestEqp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDestEqp.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.comboBoxDestEqp.FormattingEnabled = true;
            this.comboBoxDestEqp.Location = new System.Drawing.Point(99, 17);
            this.comboBoxDestEqp.Name = "comboBoxDestEqp";
            this.comboBoxDestEqp.Size = new System.Drawing.Size(196, 24);
            this.comboBoxDestEqp.Sorted = true;
            this.comboBoxDestEqp.TabIndex = 14;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonAdd);
            this.panel1.Controls.Add(this.btn_Exit);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(755, 14);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(87, 370);
            this.panel1.TabIndex = 17;
            // 
            // buttonAdd
            // 
            this.buttonAdd.BackColor = System.Drawing.Color.GreenYellow;
            this.buttonAdd.FlatAppearance.BorderSize = 0;
            this.buttonAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonAdd.ForeColor = System.Drawing.Color.Black;
            this.buttonAdd.Location = new System.Drawing.Point(0, 0);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(87, 215);
            this.buttonAdd.TabIndex = 19;
            this.buttonAdd.Text = "ADD JOB";
            this.buttonAdd.UseVisualStyleBackColor = false;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // btn_Exit
            // 
            this.btn_Exit.BackColor = System.Drawing.Color.DarkRed;
            this.btn_Exit.FlatAppearance.BorderSize = 0;
            this.btn_Exit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Exit.ForeColor = System.Drawing.Color.White;
            this.btn_Exit.Location = new System.Drawing.Point(0, 215);
            this.btn_Exit.Name = "btn_Exit";
            this.btn_Exit.Size = new System.Drawing.Size(87, 155);
            this.btn_Exit.TabIndex = 18;
            this.btn_Exit.Text = "CANCEL";
            this.btn_Exit.UseVisualStyleBackColor = false;
            this.btn_Exit.Click += new System.EventHandler(this.btn_Exit_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.LightGray;
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel2.Location = new System.Drawing.Point(14, 384);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(842, 14);
            this.panel2.TabIndex = 19;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.LightGray;
            this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel3.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(14, 398);
            this.panel3.TabIndex = 20;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.LightGray;
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel4.Location = new System.Drawing.Point(14, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(842, 14);
            this.panel4.TabIndex = 21;
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.LightGray;
            this.panel5.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel5.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel5.Location = new System.Drawing.Point(842, 14);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(14, 370);
            this.panel5.TabIndex = 22;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.dataGridViewAddJobs);
            this.groupBox3.Controls.Add(this.buttonDelJob);
            this.groupBox3.ForeColor = System.Drawing.Color.White;
            this.groupBox3.Location = new System.Drawing.Point(341, 45);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(408, 316);
            this.groupBox3.TabIndex = 23;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "추가할 작업목록(Multi/Batch Job)";
            // 
            // dataGridViewAddJobs
            // 
            this.dataGridViewAddJobs.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridViewAddJobs.BackgroundColor = System.Drawing.Color.White;
            this.dataGridViewAddJobs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewAddJobs.Location = new System.Drawing.Point(6, 17);
            this.dataGridViewAddJobs.MultiSelect = false;
            this.dataGridViewAddJobs.Name = "dataGridViewAddJobs";
            this.dataGridViewAddJobs.ReadOnly = true;
            this.dataGridViewAddJobs.RowHeadersVisible = false;
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
            this.dataGridViewAddJobs.RowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewAddJobs.RowTemplate.Height = 23;
            this.dataGridViewAddJobs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewAddJobs.Size = new System.Drawing.Size(329, 293);
            this.dataGridViewAddJobs.TabIndex = 20;
            this.dataGridViewAddJobs.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewAddJobs_CellClick);
            // 
            // FormOperatorAction
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(856, 398);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.buttonAddJob);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormOperatorAction";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormOperatorAction";
            this.Load += new System.EventHandler(this.FormOperatorAction_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAddJobs)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonDelJob;
        private System.Windows.Forms.Button buttonAddJob;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox comboBoxEqp;
        private System.Windows.Forms.ComboBox comboBoxVecPart;
        private System.Windows.Forms.RadioButton radioButtonEqp;
        private System.Windows.Forms.RadioButton radioButtonAmr;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxDestEqp;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button btn_Exit;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxAmrCarrierId;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxEqpCarrierId;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.DataGridView dataGridViewAddJobs;
    }
}