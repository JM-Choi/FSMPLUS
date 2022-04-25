using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FSMPlus.Ref;
using System.Diagnostics;
using FSMPlus.Forms;

namespace FSMPlus.Controller
{
    public partial class CtrlVec : UserControl
    {
        private string _VecID = string.Empty;
        private ColorStatus _Status = ColorStatus.Disconnected;
        /*private PartitionStatus p_status = PartitionStatus.Empty;  // global partition status buffer*/
        public CtrlVec()
        {
            InitializeComponent();
        }

        public void SetInitData(string vecId, int maxParti)
        {
            _VecID = vecId;
            labelID.Text = _VecID + " : AUTO";
            int row_val=0;

            maxParti = 4;
            row_val = 10;

            for (int i = 0; i < maxParti; i++)
            {
                dataGridViewPartition.Columns.Add($"{i}", $"{i}");
            }
            dataGridViewPartition.Rows.Add((int)(row_val));

            foreach (DataGridViewRow item in dataGridViewPartition.Rows)
            {
                if (maxParti > 4)
                {
                    item.Height = dataGridViewPartition.Height/2;
                }
                else
                {
                    item.Height = dataGridViewPartition.Height;
                }
            }

            for (int i = 0; i < maxParti; i++)
            {
                partInfo.Add(new PartData() { index = i, p = PartitionStatus.NotInUse });
            }
        }

        private List<PartData> partInfo = new List<PartData>();


        public void Partition_Color_Change(PartData partitionStatus)
        {
            var target = partInfo.Where(p => p.index == partitionStatus.index).FirstOrDefault();
            if (target != null)
            {
                target.p = partitionStatus.p;
            }
            dataGridViewPartition.Refresh();
        }

        /*
        public void Partition_Color_Change(List<PartData> partition_data)
        {
            // This function will update global partition information that is further used
            // in event handler for color update
            foreach(PartData i in partition_data)
            {
                if (i.p == PartitionStatus.Empty)
                {
                    p_status = PartitionStatus.Empty;
                }
                else if (i.p == PartitionStatus.Full)
                {
                    p_status = PartitionStatus.Full;

                }
                else if (i.p == PartitionStatus.NotInUse)
                {
                    p_status = PartitionStatus.NotInUse;
                }
            }
        }
        */

        private void UpdateItem()
        {
            var vecState = Global.Init.Db.Vechicles.Where(p => p.ID == _VecID && p.isUse == 1).FirstOrDefault();
            if (vecState == null)
            {
                return;
            }
            labelJobID.Text = vecState.C_BATCHID;

            if (vecState.C_BATCHID != null && vecState.C_BATCHID.Length > 1)
            {
                var job = Global.Init.Db.Peps.Where(p => p.BATCHID == vecState.C_BATCHID).FirstOrDefault();
                if (job != null)
                {
                    labelSrc.Text = job.S_EQPID;
                    labelDst.Text = job.T_EQPID;
                    labelJobState.Text = job.C_state.ToString();
                }
            }
            else
            {
                labelSrc.Text = string.Empty;
                labelDst.Text = string.Empty;
                labelJobState.Text = string.Empty;
            }

            labelMode.Text = vecState.C_mode.ToString();
            UpdatcStatus(new StatusData() { installState = (VehicleInstallState)vecState.installState, mode = (VehicleMode)vecState.C_mode, state = (VehicleState)vecState.C_state });
            UpdatePartitionStatus();
        }

        private void UpdatcStatus(StatusData data)
        {
            if (data.installState == VehicleInstallState.REMOVED)
            {
                _Status = ColorStatus.Disconnected;
            }
            else if (data.mode == VehicleMode.MANUAL)
            {
                _Status = ColorStatus.Manual;
            }
            else if (data.mode == VehicleMode.ERROR)
            {
                _Status = ColorStatus.Error;
            }
            else if (data.state == VehicleState.NOT_ASSIGN)
            {
                _Status = ColorStatus.WaitJob;
            }
            else if (data.state == VehicleState.DEPOSITING || data.state == VehicleState.ACQUIRING)
            {
                _Status = ColorStatus.URWorking;                
            }
            else
            {
                _Status = ColorStatus.WorkingJob;
            }
        }

        public void UpdatPartitionStatus(PartData data)
        {
            partInfo.Where(p => p.index == data.index).FirstOrDefault().p = data.p;
            dataGridViewPartition.Refresh();
        }

        private void UpdatePartitionStatus()
        {
            var parts = Global.Init.Db.VecParts.Where(p => p.VEHICLEID == _VecID).ToList();
            int cnt = 0;
            foreach (var item in parts)
            {
                var part = partInfo.Where(p => p.index == cnt).FirstOrDefault();

                if (item.state == (int)VehiclePartState.DISABLE)
                {
                    part.p = PartitionStatus.NotInUse;
                }
                //else if (item.C_spareCnt == 0)
                //{
                //    part.p = PartitionStatus.Empty;
                //}
                //else
                //{
                //    part.p = PartitionStatus.Full;
                //}
                cnt++;
            }
            dataGridViewPartition.Refresh();
        }

        private bool Blink = false;
        private void timerColorRefresh_Tick(object sender, EventArgs e)
        {
            Blink = !Blink;
            switch (_Status)
            {
                case ColorStatus.Disconnected:
                    if (Blink)
                    {
                        panelStatus.BackColor = Color.Black;
                    }
                    else
                    {
                        panelStatus.BackColor = Color.DimGray;
                    }
                    break;
                case ColorStatus.Error:
                    if (Blink)
                    {
                        panelStatus.BackColor = Color.Red;
                    }
                    else
                    {
                        panelStatus.BackColor = Color.OrangeRed;
                    }
                    break;
                case ColorStatus.WaitJob:
                    if (Blink)
                    {
                        panelStatus.BackColor = Color.Blue;
                    }
                    else
                    {
                        panelStatus.BackColor = Color.SkyBlue;
                    }
                    break;
                case ColorStatus.Manual:
                    if (Blink)
                    {
                        panelStatus.BackColor = Color.LightGoldenrodYellow;
                    }
                    else
                    {
                        panelStatus.BackColor = Color.Yellow;
                    }
                    break;
                case ColorStatus.WorkingJob:
                    if (Blink)
                    {
                        panelStatus.BackColor = Color.Green;
                    }
                    else
                    {
                        panelStatus.BackColor = Color.Lime;
                    }
                    break;
                case ColorStatus.URWorking:
                    if (Blink)
                    {
                        panelStatus.BackColor = Color.Purple;
                    }
                    else
                    {
                        panelStatus.BackColor = Color.MediumPurple;
                    }
                    break;
                default:
                    break;
            }
            dataGridViewPartition.ClearSelection();
            UpdateItem();
        }

        private void dataGridViewPartition_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                var index = e.ColumnIndex + e.RowIndex * dataGridViewPartition.Columns.Count;
                var target = partInfo.Where(p => p.index == index).FirstOrDefault();

                if (target != null)
                {
                    switch (target.p)
                    {
                        case PartitionStatus.Empty:
                            e.CellStyle.BackColor = Color.White;
                            break;

                        case PartitionStatus.Full:
                            e.CellStyle.BackColor = Color.Lime;
                            break;

                        case PartitionStatus.NotInUse:
                            e.CellStyle.BackColor = Color.Black;
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine($"dataGridViewCmd_CellFormatting:{err.ToString()}");
            }
        }

        private FormDetailInfo fm = null;// = new FormDetailInfo(vec, parts);
        private void buttonShowDetailInfo_Click(object sender, EventArgs e)
        {
            if (fm != null)
            {
                if (fm.Visible)
                {
                    fm.Close();
                    fm = null;
                    return;
                }
            }
            var vec = Global.Init.Db.Vechicles.Where(p => p.ID == _VecID && p.isUse == 1).ToArray();
            var parts = Global.Init.Db.VecParts.Where(p => p.VEHICLEID == _VecID).ToArray();

            bool isstop = Global.Init.RobotList[_VecID].isStop;
            fm = new FormDetailInfo(vec, parts, isstop);
            fm.OnChangePartInfo += Fm_OnChangePartInfo;
            fm.OnResume += _DetailInfo_OnResume;
            fm.OnStop += _DetailInfo_OnStop;
            fm.OnReconnect += _DetailInfo_OnReconnect;
            var pos = (sender as Control).PointToScreen(buttonShowDetailInfo.Location);
            fm.Show();
            //fm.Location = new Point(pos.X, pos.Y + Size.Height - Location.Y);
            fm.Location = new Point(pos.X, pos.Y - panelStatus.Size.Height + buttonShowDetailInfo.Size.Height);
            fm.TopMost = true;            
        }
        private void _DetailInfo_OnResume(object sender, buttonEventArgs e)
        {
            Global.Init.RobotList[e.vecID].JOB_RESUME();
        }
        private void _DetailInfo_OnStop(object sender, buttonEventArgs e)
        {
            if (e.stop)
            {
                labelID.Text = _VecID + " : STOP";
            }
            else
            {
                labelID.Text = _VecID + " : AUTO";

            }
            Global.Init.RobotList[e.vecID].isStop = e.stop;
        }
        private void _DetailInfo_OnReconnect(object sender, ReconnectedEventArgs e)
        {
            Global.Init.RobotList[e.vecID].Reconnected();
        }


        public void _DetailInfo_OnResumeEnable(bool check, string vec_id)
        {
            if (fm.VecID == vec_id)
                fm.ResumeEnable(check);
        }

        private void Fm_OnChangePartInfo(object sender, ChangePartEventArgs e)
        {
            // jm.choi - 190619
            // Tray Insert/Delete
            var parts = Global.Init.Db.VecParts.Where(p => p.VEHICLEID == _VecID).OrderBy(p => p.VEHICLEID).ToList();
            //part[e.PartIndex].Status = e.DstStatus;
            if (e.DstStatus == VehiclePartStatus.FULL)
            {
                string id = string.Empty;
                if (InputBox("Tray 추가", "새로 추가 할 Tray의 ID를 넣으세요.", ref id) == DialogResult.OK)
                {
                    var part = parts.Where(p => p.idx == e.PartIndex).First();
                    part.C_trayId = id;
                    Global.Init.Db.DbUpdate(TableType.VEHICLE_PART);
                    Logger.Inst.Write(_VecID, CmdLogType.Db, $"Tray Insert : idx[{e.PartIndex}]/TrayID[{id}]/part[{part.VEHICLEID}]");
                }
            }
            else if (e.DstStatus == VehiclePartStatus.EMPTY)
            {
                var part = parts.Where(p => p.idx == e.PartIndex).First();
                part.C_trayId = null;
                Global.Init.Db.DbUpdate(TableType.VEHICLE_PART);
                Logger.Inst.Write(_VecID, CmdLogType.Db, $"Tray Delete : idx[{e.PartIndex}]/part[{part.VEHICLEID}]");
            }
            else
            {
                Logger.Inst.Write(_VecID, CmdLogType.Db, $"Tray Insert/Delete Error : idx[{e.PartIndex}]");
            }

        }

        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;
            form.TopMost = true;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }
        //public void Dispose()
        //{
        //    this.dis
        //}
    }

    public class JobData
    {
        public string JobName = "";
        public string SrcUnit = "";
        public string DstUnit = "";
    }

    public class StatusData
    {
        public VehicleMode mode = VehicleMode.MANUAL;
        public VehicleState state = VehicleState.NOT_ASSIGN;
        public VehicleInstallState installState = VehicleInstallState.REMOVED;
    }

    public class PartData
    {
        public PartitionStatus p;
        public int index;

    }

    public enum ColorStatus
    {
        Disconnected,   //black
        Error,          //Red
        WaitJob,        //Blue
        Manual,         //Yellow
        WorkingJob,     //Green
        URWorking,      //Purple
    }

    public enum PartitionStatus
    {
        Empty,          // Black
        Full,           // Green 
        NotInUse        // Yellow
    }

}
