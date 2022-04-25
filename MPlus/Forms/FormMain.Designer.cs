namespace FSMPlus
{
    partial class FormMain
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonLegend = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.buttonMakeJob = new System.Windows.Forms.Button();
            this.buttonConfig = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonPause = new System.Windows.Forms.Button();
            this.buttonAuto = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.btnRvConnection = new System.Windows.Forms.Button();
            this.StsInit = new System.Windows.Forms.Button();
            this.StsStop = new System.Windows.Forms.Button();
            this.StsAuto = new System.Windows.Forms.Button();
            this.StsPause = new System.Windows.Forms.Button();
            this.labelCurTime = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelOnlineState = new System.Windows.Forms.Label();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panelMainViewer = new System.Windows.Forms.Panel();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.panel4 = new System.Windows.Forms.Panel();
            this.pictureBoxMap = new System.Windows.Forms.PictureBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.comboBoxVecFind = new System.Windows.Forms.ComboBox();
            this.buttonZoomOut = new System.Windows.Forms.Button();
            this.buttonZoomIn = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBoxShowTrace = new System.Windows.Forms.CheckBox();
            this.checkBoxShowCharge = new System.Windows.Forms.CheckBox();
            this.checkBoxShowStatus = new System.Windows.Forms.CheckBox();
            this.checkBoxShowJob = new System.Windows.Forms.CheckBox();
            this.checkBoxShowDest = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonMapFileSel = new System.Windows.Forms.Button();
            this.textBoxMapFileName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panelStatusColorBar = new System.Windows.Forms.Panel();
            this.timerStatusBar = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMap)).BeginInit();
            this.panel3.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.DimGray;
            this.panel1.Controls.Add(this.buttonLegend);
            this.panel1.Controls.Add(this.groupBox4);
            this.panel1.Controls.Add(this.buttonConfig);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 60);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(116, 541);
            this.panel1.TabIndex = 5;
            // 
            // buttonLegend
            // 
            this.buttonLegend.BackColor = System.Drawing.Color.Gold;
            this.buttonLegend.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonLegend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonLegend.Location = new System.Drawing.Point(0, 515);
            this.buttonLegend.Name = "buttonLegend";
            this.buttonLegend.Size = new System.Drawing.Size(116, 26);
            this.buttonLegend.TabIndex = 4;
            this.buttonLegend.Text = "Legend";
            this.buttonLegend.UseVisualStyleBackColor = false;
            this.buttonLegend.Visible = false;
            this.buttonLegend.Click += new System.EventHandler(this.buttonLegend_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.buttonMakeJob);
            this.groupBox4.ForeColor = System.Drawing.Color.White;
            this.groupBox4.Location = new System.Drawing.Point(10, 217);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(88, 101);
            this.groupBox4.TabIndex = 13;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Oper Action";
            this.groupBox4.Visible = false;
            // 
            // buttonMakeJob
            // 
            this.buttonMakeJob.BackColor = System.Drawing.Color.DarkViolet;
            this.buttonMakeJob.FlatAppearance.BorderSize = 0;
            this.buttonMakeJob.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonMakeJob.ForeColor = System.Drawing.Color.White;
            this.buttonMakeJob.Location = new System.Drawing.Point(11, 33);
            this.buttonMakeJob.Name = "buttonMakeJob";
            this.buttonMakeJob.Size = new System.Drawing.Size(68, 47);
            this.buttonMakeJob.TabIndex = 3;
            this.buttonMakeJob.Text = "Create Job";
            this.buttonMakeJob.UseVisualStyleBackColor = false;
            this.buttonMakeJob.Visible = false;
            // 
            // buttonConfig
            // 
            this.buttonConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonConfig.BackColor = System.Drawing.Color.Gainsboro;
            this.buttonConfig.FlatAppearance.BorderSize = 0;
            this.buttonConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonConfig.Location = new System.Drawing.Point(13, 439);
            this.buttonConfig.Name = "buttonConfig";
            this.buttonConfig.Size = new System.Drawing.Size(86, 53);
            this.buttonConfig.TabIndex = 12;
            this.buttonConfig.Text = "Config";
            this.buttonConfig.UseVisualStyleBackColor = false;
            this.buttonConfig.Click += new System.EventHandler(this.buttonConfig_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonStop);
            this.groupBox1.Controls.Add(this.buttonPause);
            this.groupBox1.Controls.Add(this.buttonAuto);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.ForeColor = System.Drawing.Color.White;
            this.groupBox1.Location = new System.Drawing.Point(10, 25);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(88, 186);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Actions";
            // 
            // buttonStop
            // 
            this.buttonStop.BackColor = System.Drawing.Color.Silver;
            this.buttonStop.Enabled = false;
            this.buttonStop.FlatAppearance.BorderSize = 0;
            this.buttonStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonStop.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Bold);
            this.buttonStop.ForeColor = System.Drawing.Color.Black;
            this.buttonStop.Location = new System.Drawing.Point(11, 124);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(68, 47);
            this.buttonStop.TabIndex = 4;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = false;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // buttonPause
            // 
            this.buttonPause.BackColor = System.Drawing.Color.Silver;
            this.buttonPause.Enabled = false;
            this.buttonPause.FlatAppearance.BorderSize = 0;
            this.buttonPause.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPause.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.buttonPause.ForeColor = System.Drawing.Color.Black;
            this.buttonPause.Location = new System.Drawing.Point(11, 71);
            this.buttonPause.Name = "buttonPause";
            this.buttonPause.Size = new System.Drawing.Size(68, 47);
            this.buttonPause.TabIndex = 3;
            this.buttonPause.Text = "Pause";
            this.buttonPause.UseVisualStyleBackColor = false;
            this.buttonPause.Click += new System.EventHandler(this.buttonPause_Click);
            // 
            // buttonAuto
            // 
            this.buttonAuto.BackColor = System.Drawing.Color.Silver;
            this.buttonAuto.FlatAppearance.BorderSize = 0;
            this.buttonAuto.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonAuto.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.buttonAuto.ForeColor = System.Drawing.Color.Black;
            this.buttonAuto.Location = new System.Drawing.Point(11, 18);
            this.buttonAuto.Name = "buttonAuto";
            this.buttonAuto.Size = new System.Drawing.Size(68, 47);
            this.buttonAuto.TabIndex = 2;
            this.buttonAuto.Text = "Auto";
            this.buttonAuto.UseVisualStyleBackColor = false;
            this.buttonAuto.Click += new System.EventHandler(this.buttonAuto_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.DarkBlue;
            this.panel2.Controls.Add(this.panel5);
            this.panel2.Controls.Add(this.labelVersion);
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.pictureBox1);
            this.panel2.Controls.Add(this.labelOnlineState);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1305, 55);
            this.panel2.TabIndex = 6;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.btnRvConnection);
            this.panel5.Controls.Add(this.StsInit);
            this.panel5.Controls.Add(this.StsStop);
            this.panel5.Controls.Add(this.StsAuto);
            this.panel5.Controls.Add(this.StsPause);
            this.panel5.Controls.Add(this.labelCurTime);
            this.panel5.Controls.Add(this.label4);
            this.panel5.Controls.Add(this.label1);
            this.panel5.Controls.Add(this.label3);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel5.Location = new System.Drawing.Point(743, 0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(562, 55);
            this.panel5.TabIndex = 24;
            // 
            // btnRvConnection
            // 
            this.btnRvConnection.BackColor = System.Drawing.Color.Chartreuse;
            this.btnRvConnection.Enabled = false;
            this.btnRvConnection.FlatAppearance.BorderSize = 0;
            this.btnRvConnection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRvConnection.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnRvConnection.ForeColor = System.Drawing.Color.Black;
            this.btnRvConnection.Location = new System.Drawing.Point(142, 11);
            this.btnRvConnection.Name = "btnRvConnection";
            this.btnRvConnection.Size = new System.Drawing.Size(68, 31);
            this.btnRvConnection.TabIndex = 32;
            this.btnRvConnection.Text = "LIVE";
            this.btnRvConnection.UseVisualStyleBackColor = false;
            // 
            // StsInit
            // 
            this.StsInit.BackColor = System.Drawing.Color.Silver;
            this.StsInit.Enabled = false;
            this.StsInit.FlatAppearance.BorderSize = 0;
            this.StsInit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StsInit.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.StsInit.ForeColor = System.Drawing.Color.Black;
            this.StsInit.Location = new System.Drawing.Point(273, 11);
            this.StsInit.Name = "StsInit";
            this.StsInit.Size = new System.Drawing.Size(68, 31);
            this.StsInit.TabIndex = 29;
            this.StsInit.Text = "INIT";
            this.StsInit.UseVisualStyleBackColor = false;
            // 
            // StsStop
            // 
            this.StsStop.BackColor = System.Drawing.Color.Red;
            this.StsStop.Enabled = false;
            this.StsStop.FlatAppearance.BorderSize = 0;
            this.StsStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StsStop.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.StsStop.ForeColor = System.Drawing.Color.Black;
            this.StsStop.Location = new System.Drawing.Point(273, 11);
            this.StsStop.Name = "StsStop";
            this.StsStop.Size = new System.Drawing.Size(68, 31);
            this.StsStop.TabIndex = 30;
            this.StsStop.Text = "Stop";
            this.StsStop.UseVisualStyleBackColor = false;
            // 
            // StsAuto
            // 
            this.StsAuto.BackColor = System.Drawing.Color.Chartreuse;
            this.StsAuto.Enabled = false;
            this.StsAuto.FlatAppearance.BorderSize = 0;
            this.StsAuto.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StsAuto.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.StsAuto.ForeColor = System.Drawing.Color.Black;
            this.StsAuto.Location = new System.Drawing.Point(273, 11);
            this.StsAuto.Name = "StsAuto";
            this.StsAuto.Size = new System.Drawing.Size(68, 31);
            this.StsAuto.TabIndex = 21;
            this.StsAuto.Text = "Auto";
            this.StsAuto.UseVisualStyleBackColor = false;
            // 
            // StsPause
            // 
            this.StsPause.BackColor = System.Drawing.Color.Coral;
            this.StsPause.Enabled = false;
            this.StsPause.FlatAppearance.BorderSize = 0;
            this.StsPause.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StsPause.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.StsPause.ForeColor = System.Drawing.Color.Black;
            this.StsPause.Location = new System.Drawing.Point(273, 11);
            this.StsPause.Name = "StsPause";
            this.StsPause.Size = new System.Drawing.Size(68, 31);
            this.StsPause.TabIndex = 22;
            this.StsPause.Text = "Pause";
            this.StsPause.UseVisualStyleBackColor = false;
            // 
            // labelCurTime
            // 
            this.labelCurTime.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelCurTime.AutoSize = true;
            this.labelCurTime.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelCurTime.ForeColor = System.Drawing.Color.SpringGreen;
            this.labelCurTime.ImageIndex = 0;
            this.labelCurTime.Location = new System.Drawing.Point(422, 21);
            this.labelCurTime.Name = "labelCurTime";
            this.labelCurTime.Size = new System.Drawing.Size(160, 13);
            this.labelCurTime.TabIndex = 28;
            this.labelCurTime.Text = "YYYY.MM.DD 00:00:00";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(381, 19);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 18);
            this.label4.TabIndex = 27;
            this.label4.Text = "Time";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(216, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 18);
            this.label1.TabIndex = 26;
            this.label1.Text = "Actions";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(34, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(108, 18);
            this.label3.TabIndex = 24;
            this.label3.Text = "RV Connection";
            // 
            // labelVersion
            // 
            this.labelVersion.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelVersion.AutoSize = true;
            this.labelVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVersion.ForeColor = System.Drawing.Color.White;
            this.labelVersion.Location = new System.Drawing.Point(175, 28);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(83, 15);
            this.labelVersion.TabIndex = 20;
            this.labelVersion.Text = "-------------------";
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(146, 28);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(28, 15);
            this.label6.TabIndex = 19;
            this.label6.Text = "Ver.";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(71, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(68, 39);
            this.label5.TabIndex = 18;
            this.label5.Text = "M+";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(66, 55);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 17;
            this.pictureBox1.TabStop = false;
            // 
            // labelOnlineState
            // 
            this.labelOnlineState.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelOnlineState.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelOnlineState.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelOnlineState.ImageIndex = 1;
            this.labelOnlineState.ImageList = this.imageList1;
            this.labelOnlineState.Location = new System.Drawing.Point(653, 11);
            this.labelOnlineState.Name = "labelOnlineState";
            this.labelOnlineState.Size = new System.Drawing.Size(47, 33);
            this.labelOnlineState.TabIndex = 25;
            this.labelOnlineState.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.labelOnlineState.Visible = false;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "broken-link.png");
            this.imageList1.Images.SetKeyName(1, "link.png");
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(116, 60);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panelMainViewer);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.AutoScroll = true;
            this.splitContainer1.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1189, 541);
            this.splitContainer1.SplitterDistance = 824;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 8;
            // 
            // panelMainViewer
            // 
            this.panelMainViewer.BackColor = System.Drawing.Color.Gray;
            this.panelMainViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMainViewer.Location = new System.Drawing.Point(0, 0);
            this.panelMainViewer.Name = "panelMainViewer";
            this.panelMainViewer.Size = new System.Drawing.Size(824, 541);
            this.panelMainViewer.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.BackColor = System.Drawing.Color.White;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.panel4);
            this.splitContainer2.Panel1.Controls.Add(this.panel3);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.BackColor = System.Drawing.Color.Black;
            this.splitContainer2.Size = new System.Drawing.Size(362, 541);
            this.splitContainer2.SplitterDistance = 312;
            this.splitContainer2.TabIndex = 4;
            // 
            // panel4
            // 
            this.panel4.AutoScroll = true;
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.panel4.Controls.Add(this.pictureBoxMap);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 75);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(362, 237);
            this.panel4.TabIndex = 3;
            // 
            // pictureBoxMap
            // 
            this.pictureBoxMap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.pictureBoxMap.Location = new System.Drawing.Point(4, 7);
            this.pictureBoxMap.Name = "pictureBoxMap";
            this.pictureBoxMap.Size = new System.Drawing.Size(321, 321);
            this.pictureBoxMap.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxMap.TabIndex = 1;
            this.pictureBoxMap.TabStop = false;
            this.pictureBoxMap.DoubleClick += new System.EventHandler(this.pictureBoxMap_DoubleClick);
            this.pictureBoxMap.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBoxMap_MouseDown);
            this.pictureBoxMap.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBoxMap_MouseMove);
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.DarkGray;
            this.panel3.Controls.Add(this.tableLayoutPanel2);
            this.panel3.Controls.Add(this.tableLayoutPanel1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(362, 75);
            this.panel3.TabIndex = 2;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 6;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 310F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel2.Controls.Add(this.groupBox2, 5, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonZoomOut, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonZoomIn, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.button1, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.groupBox3, 4, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 29);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(362, 46);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.comboBoxVecFind);
            this.groupBox2.Location = new System.Drawing.Point(215, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(144, 40);
            this.groupBox2.TabIndex = 18;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Find AMR";
            // 
            // comboBoxVecFind
            // 
            this.comboBoxVecFind.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxVecFind.FormattingEnabled = true;
            this.comboBoxVecFind.Location = new System.Drawing.Point(3, 14);
            this.comboBoxVecFind.Name = "comboBoxVecFind";
            this.comboBoxVecFind.Size = new System.Drawing.Size(139, 21);
            this.comboBoxVecFind.TabIndex = 16;
            this.comboBoxVecFind.SelectedIndexChanged += new System.EventHandler(this.comboBoxVecFind_SelectedIndexChanged);
            // 
            // buttonZoomOut
            // 
            this.buttonZoomOut.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonZoomOut.BackgroundImage")));
            this.buttonZoomOut.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonZoomOut.FlatAppearance.BorderSize = 0;
            this.buttonZoomOut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonZoomOut.Font = new System.Drawing.Font("굴림", 5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.buttonZoomOut.ForeColor = System.Drawing.Color.Transparent;
            this.buttonZoomOut.Location = new System.Drawing.Point(-239, 3);
            this.buttonZoomOut.Name = "buttonZoomOut";
            this.buttonZoomOut.Size = new System.Drawing.Size(42, 40);
            this.buttonZoomOut.TabIndex = 13;
            this.buttonZoomOut.Text = "ZOOM OUT";
            this.buttonZoomOut.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.buttonZoomOut.UseVisualStyleBackColor = true;
            this.buttonZoomOut.Click += new System.EventHandler(this.buttonZoomOut_Click);
            // 
            // buttonZoomIn
            // 
            this.buttonZoomIn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonZoomIn.BackgroundImage")));
            this.buttonZoomIn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonZoomIn.FlatAppearance.BorderSize = 0;
            this.buttonZoomIn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonZoomIn.Font = new System.Drawing.Font("굴림", 5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.buttonZoomIn.ForeColor = System.Drawing.Color.Transparent;
            this.buttonZoomIn.Location = new System.Drawing.Point(-191, 3);
            this.buttonZoomIn.Name = "buttonZoomIn";
            this.buttonZoomIn.Size = new System.Drawing.Size(42, 40);
            this.buttonZoomIn.TabIndex = 12;
            this.buttonZoomIn.Text = "ZOOM IN";
            this.buttonZoomIn.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.buttonZoomIn.UseVisualStyleBackColor = true;
            this.buttonZoomIn.Click += new System.EventHandler(this.buttonZoomIn_Click);
            // 
            // button1
            // 
            this.button1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("button1.BackgroundImage")));
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("굴림", 5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.button1.ForeColor = System.Drawing.Color.Transparent;
            this.button1.Location = new System.Drawing.Point(-143, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(42, 40);
            this.button1.TabIndex = 14;
            this.button1.Text = "ZOOM ORI";
            this.button1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.checkBoxShowTrace);
            this.groupBox3.Controls.Add(this.checkBoxShowCharge);
            this.groupBox3.Controls.Add(this.checkBoxShowStatus);
            this.groupBox3.Controls.Add(this.checkBoxShowJob);
            this.groupBox3.Controls.Add(this.checkBoxShowDest);
            this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox3.Location = new System.Drawing.Point(-95, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(304, 40);
            this.groupBox3.TabIndex = 17;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Vehicle Items";
            // 
            // checkBoxShowTrace
            // 
            this.checkBoxShowTrace.AutoSize = true;
            this.checkBoxShowTrace.Location = new System.Drawing.Point(243, 16);
            this.checkBoxShowTrace.Name = "checkBoxShowTrace";
            this.checkBoxShowTrace.Size = new System.Drawing.Size(53, 17);
            this.checkBoxShowTrace.TabIndex = 4;
            this.checkBoxShowTrace.Text = "Trace";
            this.checkBoxShowTrace.UseVisualStyleBackColor = true;
            this.checkBoxShowTrace.Click += new System.EventHandler(this.checkBoxViewItems_Click);
            // 
            // checkBoxShowCharge
            // 
            this.checkBoxShowCharge.AutoSize = true;
            this.checkBoxShowCharge.Location = new System.Drawing.Point(183, 16);
            this.checkBoxShowCharge.Name = "checkBoxShowCharge";
            this.checkBoxShowCharge.Size = new System.Drawing.Size(62, 17);
            this.checkBoxShowCharge.TabIndex = 3;
            this.checkBoxShowCharge.Text = "Charge";
            this.checkBoxShowCharge.UseVisualStyleBackColor = true;
            this.checkBoxShowCharge.Click += new System.EventHandler(this.checkBoxViewItems_Click);
            // 
            // checkBoxShowStatus
            // 
            this.checkBoxShowStatus.AutoSize = true;
            this.checkBoxShowStatus.Location = new System.Drawing.Point(127, 16);
            this.checkBoxShowStatus.Name = "checkBoxShowStatus";
            this.checkBoxShowStatus.Size = new System.Drawing.Size(57, 17);
            this.checkBoxShowStatus.TabIndex = 2;
            this.checkBoxShowStatus.Text = "Status";
            this.checkBoxShowStatus.UseVisualStyleBackColor = true;
            this.checkBoxShowStatus.Click += new System.EventHandler(this.checkBoxViewItems_Click);
            // 
            // checkBoxShowJob
            // 
            this.checkBoxShowJob.AutoSize = true;
            this.checkBoxShowJob.Location = new System.Drawing.Point(84, 16);
            this.checkBoxShowJob.Name = "checkBoxShowJob";
            this.checkBoxShowJob.Size = new System.Drawing.Size(44, 17);
            this.checkBoxShowJob.TabIndex = 1;
            this.checkBoxShowJob.Text = "Job";
            this.checkBoxShowJob.UseVisualStyleBackColor = true;
            this.checkBoxShowJob.Click += new System.EventHandler(this.checkBoxViewItems_Click);
            // 
            // checkBoxShowDest
            // 
            this.checkBoxShowDest.AutoSize = true;
            this.checkBoxShowDest.Location = new System.Drawing.Point(5, 16);
            this.checkBoxShowDest.Name = "checkBoxShowDest";
            this.checkBoxShowDest.Size = new System.Drawing.Size(84, 17);
            this.checkBoxShowDest.TabIndex = 0;
            this.checkBoxShowDest.Text = "Destination";
            this.checkBoxShowDest.UseVisualStyleBackColor = true;
            this.checkBoxShowDest.Click += new System.EventHandler(this.checkBoxViewItems_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15.35948F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 84.64053F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 47F));
            this.tableLayoutPanel1.Controls.Add(this.buttonMapFileSel, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxMapFileName, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(362, 29);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // buttonMapFileSel
            // 
            this.buttonMapFileSel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonMapFileSel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonMapFileSel.Location = new System.Drawing.Point(314, 7);
            this.buttonMapFileSel.Margin = new System.Windows.Forms.Padding(0);
            this.buttonMapFileSel.Name = "buttonMapFileSel";
            this.buttonMapFileSel.Size = new System.Drawing.Size(48, 22);
            this.buttonMapFileSel.TabIndex = 17;
            this.buttonMapFileSel.Text = "...";
            this.buttonMapFileSel.UseVisualStyleBackColor = true;
            this.buttonMapFileSel.Click += new System.EventHandler(this.buttonMapFileSel_Click);
            // 
            // textBoxMapFileName
            // 
            this.textBoxMapFileName.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBoxMapFileName.Location = new System.Drawing.Point(48, 7);
            this.textBoxMapFileName.Margin = new System.Windows.Forms.Padding(0);
            this.textBoxMapFileName.Name = "textBoxMapFileName";
            this.textBoxMapFileName.Size = new System.Drawing.Size(266, 22);
            this.textBoxMapFileName.TabIndex = 16;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 29);
            this.label2.TabIndex = 16;
            this.label2.Text = "Current Map File";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panelStatusColorBar
            // 
            this.panelStatusColorBar.BackColor = System.Drawing.Color.White;
            this.panelStatusColorBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelStatusColorBar.Location = new System.Drawing.Point(0, 55);
            this.panelStatusColorBar.Name = "panelStatusColorBar";
            this.panelStatusColorBar.Size = new System.Drawing.Size(1305, 5);
            this.panelStatusColorBar.TabIndex = 19;
            // 
            // timerStatusBar
            // 
            this.timerStatusBar.Enabled = true;
            this.timerStatusBar.Interval = 500;
            this.timerStatusBar.Tick += new System.EventHandler(this.timerStatusBar_Tick);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1305, 601);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelStatusColorBar);
            this.Controls.Add(this.panel2);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("맑은 고딕", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Name = "FormMain";
            this.Text = "VSP - Techfloor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.panel1.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMap)).EndInit();
            this.panel3.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonPause;
        private System.Windows.Forms.Button buttonAuto;
        private System.Windows.Forms.Button buttonConfig;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panelMainViewer;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.PictureBox pictureBoxMap;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panelStatusColorBar;
        private System.Windows.Forms.Timer timerStatusBar;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button buttonMakeJob;
        private System.Windows.Forms.Button buttonLegend;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Button StsAuto;
        private System.Windows.Forms.Button StsPause;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Button StsInit;
        private System.Windows.Forms.Label labelCurTime;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelOnlineState;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button buttonMapFileSel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxMapFileName;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button buttonZoomOut;
        private System.Windows.Forms.Button buttonZoomIn;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox checkBoxShowTrace;
        private System.Windows.Forms.CheckBox checkBoxShowCharge;
        private System.Windows.Forms.CheckBox checkBoxShowStatus;
        private System.Windows.Forms.CheckBox checkBoxShowJob;
        private System.Windows.Forms.CheckBox checkBoxShowDest;
        private System.Windows.Forms.ComboBox comboBoxVecFind;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button StsStop;
        private System.Windows.Forms.Button btnRvConnection;
    }
}

