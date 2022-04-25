namespace FSMPlus.Forms
{
    partial class GoalEditor
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
            this.cmb_GoalName = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.listBoxVehicleList = new System.Windows.Forms.ListBox();
            this.buttonDelVehicle = new System.Windows.Forms.Button();
            this.grd_Zone = new System.Windows.Forms.PropertyGrid();
            this.panel3 = new System.Windows.Forms.Panel();
            this.comboBoxAddVec = new System.Windows.Forms.ComboBox();
            this.buttonAddVehicle = new System.Windows.Forms.Button();
            this.btn_Exit = new System.Windows.Forms.Button();
            this.propertyGridGoalParam = new System.Windows.Forms.PropertyGrid();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cmb_GoalName);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Font = new System.Drawing.Font("굴림", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(365, 36);
            this.panel1.TabIndex = 10;
            // 
            // cmb_GoalName
            // 
            this.cmb_GoalName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_GoalName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_GoalName.FormattingEnabled = true;
            this.cmb_GoalName.Location = new System.Drawing.Point(100, 0);
            this.cmb_GoalName.Name = "cmb_GoalName";
            this.cmb_GoalName.Size = new System.Drawing.Size(265, 35);
            this.cmb_GoalName.TabIndex = 11;
            this.cmb_GoalName.SelectedIndexChanged += new System.EventHandler(this.cmb_GoalName_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Font = new System.Drawing.Font("굴림", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 36);
            this.label1.TabIndex = 10;
            this.label1.Text = "GoalName";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.listBoxVehicleList);
            this.panel2.Controls.Add(this.buttonDelVehicle);
            this.panel2.Controls.Add(this.grd_Zone);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 172);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(365, 165);
            this.panel2.TabIndex = 11;
            // 
            // listBoxVehicleList
            // 
            this.listBoxVehicleList.Dock = System.Windows.Forms.DockStyle.Top;
            this.listBoxVehicleList.FormattingEnabled = true;
            this.listBoxVehicleList.ItemHeight = 12;
            this.listBoxVehicleList.Location = new System.Drawing.Point(0, 0);
            this.listBoxVehicleList.Name = "listBoxVehicleList";
            this.listBoxVehicleList.Size = new System.Drawing.Size(146, 112);
            this.listBoxVehicleList.TabIndex = 9;
            this.listBoxVehicleList.Click += new System.EventHandler(this.listBoxVehicleList_Click);
            // 
            // buttonDelVehicle
            // 
            this.buttonDelVehicle.BackColor = System.Drawing.Color.Tomato;
            this.buttonDelVehicle.FlatAppearance.BorderSize = 0;
            this.buttonDelVehicle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonDelVehicle.ForeColor = System.Drawing.Color.White;
            this.buttonDelVehicle.Location = new System.Drawing.Point(12, 122);
            this.buttonDelVehicle.Name = "buttonDelVehicle";
            this.buttonDelVehicle.Size = new System.Drawing.Size(40, 37);
            this.buttonDelVehicle.TabIndex = 11;
            this.buttonDelVehicle.Text = "Del";
            this.buttonDelVehicle.UseVisualStyleBackColor = false;
            this.buttonDelVehicle.Click += new System.EventHandler(this.buttonDelVehicle_Click);
            // 
            // grd_Zone
            // 
            this.grd_Zone.Dock = System.Windows.Forms.DockStyle.Right;
            this.grd_Zone.Enabled = false;
            this.grd_Zone.HelpVisible = false;
            this.grd_Zone.Location = new System.Drawing.Point(146, 0);
            this.grd_Zone.Name = "grd_Zone";
            this.grd_Zone.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.grd_Zone.Size = new System.Drawing.Size(219, 165);
            this.grd_Zone.TabIndex = 8;
            this.grd_Zone.ToolbarVisible = false;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.comboBoxAddVec);
            this.panel3.Controls.Add(this.buttonAddVehicle);
            this.panel3.Controls.Add(this.btn_Exit);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 337);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(365, 58);
            this.panel3.TabIndex = 12;
            // 
            // comboBoxAddVec
            // 
            this.comboBoxAddVec.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAddVec.Font = new System.Drawing.Font("굴림", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.comboBoxAddVec.FormattingEnabled = true;
            this.comboBoxAddVec.Location = new System.Drawing.Point(58, 11);
            this.comboBoxAddVec.Name = "comboBoxAddVec";
            this.comboBoxAddVec.Size = new System.Drawing.Size(228, 29);
            this.comboBoxAddVec.TabIndex = 12;
            // 
            // buttonAddVehicle
            // 
            this.buttonAddVehicle.BackColor = System.Drawing.Color.Green;
            this.buttonAddVehicle.FlatAppearance.BorderSize = 0;
            this.buttonAddVehicle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonAddVehicle.ForeColor = System.Drawing.Color.White;
            this.buttonAddVehicle.Location = new System.Drawing.Point(12, 6);
            this.buttonAddVehicle.Name = "buttonAddVehicle";
            this.buttonAddVehicle.Size = new System.Drawing.Size(40, 37);
            this.buttonAddVehicle.TabIndex = 10;
            this.buttonAddVehicle.Text = "Add";
            this.buttonAddVehicle.UseVisualStyleBackColor = false;
            this.buttonAddVehicle.Click += new System.EventHandler(this.buttonAddVehicle_Click);
            // 
            // btn_Exit
            // 
            this.btn_Exit.BackColor = System.Drawing.Color.YellowGreen;
            this.btn_Exit.Dock = System.Windows.Forms.DockStyle.Right;
            this.btn_Exit.FlatAppearance.BorderSize = 0;
            this.btn_Exit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Exit.ForeColor = System.Drawing.Color.Black;
            this.btn_Exit.Location = new System.Drawing.Point(292, 0);
            this.btn_Exit.Name = "btn_Exit";
            this.btn_Exit.Size = new System.Drawing.Size(73, 58);
            this.btn_Exit.TabIndex = 9;
            this.btn_Exit.Text = "EXIT";
            this.btn_Exit.UseVisualStyleBackColor = false;
            this.btn_Exit.Click += new System.EventHandler(this.btn_Exit_Click);
            // 
            // propertyGridGoalParam
            // 
            this.propertyGridGoalParam.Dock = System.Windows.Forms.DockStyle.Top;
            this.propertyGridGoalParam.HelpVisible = false;
            this.propertyGridGoalParam.Location = new System.Drawing.Point(0, 48);
            this.propertyGridGoalParam.Name = "propertyGridGoalParam";
            this.propertyGridGoalParam.Size = new System.Drawing.Size(365, 109);
            this.propertyGridGoalParam.TabIndex = 13;
            this.propertyGridGoalParam.ToolbarVisible = false;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Location = new System.Drawing.Point(0, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(365, 12);
            this.label2.TabIndex = 14;
            this.label2.Text = "Goal Informatin";
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Top;
            this.label3.Location = new System.Drawing.Point(0, 157);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(365, 15);
            this.label3.TabIndex = 15;
            this.label3.Text = "Permit-to-Work AMR list";
            // 
            // GoalEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(365, 395);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.propertyGridGoalParam);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Name = "GoalEditor";
            this.Text = "GoalEditor";
            this.Load += new System.EventHandler(this.GoalEditor_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox cmb_GoalName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PropertyGrid grd_Zone;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btn_Exit;
        private System.Windows.Forms.ListBox listBoxVehicleList;
        private System.Windows.Forms.Button buttonDelVehicle;
        private System.Windows.Forms.Button buttonAddVehicle;
        private System.Windows.Forms.ComboBox comboBoxAddVec;
        private System.Windows.Forms.PropertyGrid propertyGridGoalParam;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}