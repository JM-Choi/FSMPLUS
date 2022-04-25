namespace FSMPlus.Controller
{
    partial class CtrlVec
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

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelJobState = new System.Windows.Forms.Label();
            this.labelMode = new System.Windows.Forms.Label();
            this.labelID = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.labelJobID = new System.Windows.Forms.Label();
            this.labelDst = new System.Windows.Forms.Label();
            this.labelSrc = new System.Windows.Forms.Label();
            this.panelStatus = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.dataGridViewPartition = new System.Windows.Forms.DataGridView();
            this.timerColorRefresh = new System.Windows.Forms.Timer(this.components);
            this.buttonShowDetailInfo = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPartition)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.panel1.Controls.Add(this.labelJobState);
            this.panel1.Controls.Add(this.labelMode);
            this.panel1.Controls.Add(this.labelID);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(10, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(142, 51);
            this.panel1.TabIndex = 0;
            // 
            // labelJobState
            // 
            this.labelJobState.AutoSize = true;
            this.labelJobState.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelJobState.Location = new System.Drawing.Point(6, 35);
            this.labelJobState.Name = "labelJobState";
            this.labelJobState.Size = new System.Drawing.Size(85, 11);
            this.labelJobState.TabIndex = 2;
            this.labelJobState.Text = "Vec Job State";
            // 
            // labelMode
            // 
            this.labelMode.AutoSize = true;
            this.labelMode.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelMode.Location = new System.Drawing.Point(6, 19);
            this.labelMode.Name = "labelMode";
            this.labelMode.Size = new System.Drawing.Size(62, 11);
            this.labelMode.TabIndex = 1;
            this.labelMode.Text = "Vec Mode";
            // 
            // labelID
            // 
            this.labelID.AutoSize = true;
            this.labelID.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelID.Location = new System.Drawing.Point(6, 3);
            this.labelID.Name = "labelID";
            this.labelID.Size = new System.Drawing.Size(49, 11);
            this.labelID.TabIndex = 0;
            this.labelID.Text = "Vec ID";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panel2.Controls.Add(this.labelJobID);
            this.panel2.Controls.Add(this.labelDst);
            this.panel2.Controls.Add(this.labelSrc);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(10, 51);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(142, 52);
            this.panel2.TabIndex = 1;
            // 
            // labelJobID
            // 
            this.labelJobID.AutoSize = true;
            this.labelJobID.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelJobID.Location = new System.Drawing.Point(6, 3);
            this.labelJobID.Name = "labelJobID";
            this.labelJobID.Size = new System.Drawing.Size(44, 11);
            this.labelJobID.TabIndex = 4;
            this.labelJobID.Text = "Job id";
            // 
            // labelDst
            // 
            this.labelDst.AutoSize = true;
            this.labelDst.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelDst.Location = new System.Drawing.Point(6, 33);
            this.labelDst.Name = "labelDst";
            this.labelDst.Size = new System.Drawing.Size(69, 11);
            this.labelDst.TabIndex = 3;
            this.labelDst.Text = "Destination";
            // 
            // labelSrc
            // 
            this.labelSrc.AutoSize = true;
            this.labelSrc.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelSrc.Location = new System.Drawing.Point(6, 17);
            this.labelSrc.Name = "labelSrc";
            this.labelSrc.Size = new System.Drawing.Size(45, 11);
            this.labelSrc.TabIndex = 2;
            this.labelSrc.Text = "Source";
            // 
            // panelStatus
            // 
            this.panelStatus.BackColor = System.Drawing.Color.Black;
            this.panelStatus.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelStatus.Location = new System.Drawing.Point(0, 0);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Size = new System.Drawing.Size(10, 138);
            this.panelStatus.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.White;
            this.panel3.Controls.Add(this.dataGridViewPartition);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(10, 103);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(142, 35);
            this.panel3.TabIndex = 2;
            // 
            // dataGridViewPartition
            // 
            this.dataGridViewPartition.AllowUserToAddRows = false;
            this.dataGridViewPartition.AllowUserToDeleteRows = false;
            this.dataGridViewPartition.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewPartition.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewPartition.ColumnHeadersVisible = false;
            this.dataGridViewPartition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewPartition.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewPartition.Name = "dataGridViewPartition";
            this.dataGridViewPartition.ReadOnly = true;
            this.dataGridViewPartition.RowHeadersVisible = false;
            this.dataGridViewPartition.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dataGridViewPartition.RowTemplate.Height = 23;
            this.dataGridViewPartition.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.dataGridViewPartition.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridViewPartition.Size = new System.Drawing.Size(142, 35);
            this.dataGridViewPartition.TabIndex = 0;
            this.dataGridViewPartition.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dataGridViewPartition_CellFormatting);
            // 
            // timerColorRefresh
            // 
            this.timerColorRefresh.Enabled = true;
            this.timerColorRefresh.Interval = 500;
            this.timerColorRefresh.Tick += new System.EventHandler(this.timerColorRefresh_Tick);
            // 
            // buttonShowDetailInfo
            // 
            this.buttonShowDetailInfo.BackColor = System.Drawing.Color.LightSteelBlue;
            this.buttonShowDetailInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonShowDetailInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonShowDetailInfo.Location = new System.Drawing.Point(0, 138);
            this.buttonShowDetailInfo.Name = "buttonShowDetailInfo";
            this.buttonShowDetailInfo.Size = new System.Drawing.Size(152, 19);
            this.buttonShowDetailInfo.TabIndex = 3;
            this.buttonShowDetailInfo.Text = "▼";
            this.buttonShowDetailInfo.UseVisualStyleBackColor = false;
            this.buttonShowDetailInfo.Click += new System.EventHandler(this.buttonShowDetailInfo_Click);
            // 
            // CtrlVec
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelStatus);
            this.Controls.Add(this.buttonShowDetailInfo);
            this.Name = "CtrlVec";
            this.Size = new System.Drawing.Size(152, 157);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPartition)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelJobState;
        private System.Windows.Forms.Label labelMode;
        private System.Windows.Forms.Label labelID;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label labelJobID;
        private System.Windows.Forms.Label labelDst;
        private System.Windows.Forms.Label labelSrc;
        private System.Windows.Forms.Panel panelStatus;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Timer timerColorRefresh;
        private System.Windows.Forms.DataGridView dataGridViewPartition;
        private System.Windows.Forms.Button buttonShowDetailInfo;
    }
}
