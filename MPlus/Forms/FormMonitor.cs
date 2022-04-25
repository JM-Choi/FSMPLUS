using FSMPlus.Controller;
using FSMPlus.Ref;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FSMPlus.Forms
{
    public partial class FormMonitor : Form
    {
        public event EventHandler<DeleteEventArgs> OnDeleteCmd;
        public event EventHandler<CancelAbortEventArgs> OnCancelAbortCmd;
        public event EventHandler<CancelAbortEventArgs> OnAlarmClrCmd;

        public string ExecuteTime { get; set; }

        public string vec_first = string.Empty;
        public FormMonitor()
        {
            InitializeComponent();
            GridViewSetDoubleBuffered(dataGridViewAlarm, true);
            GridViewSetDoubleBuffered(dataGridViewCmd, true);
        }

        private List<CtrlVec> ctrlVecs = new List<CtrlVec>();

        private void FormMonitor_Load(object sender, EventArgs e)
        {
            UpdateTable(TableType.ALARM);
            UpdateTable(TableType.PEPSCHEDULE);
            UpdateTable(TableType.VEHICLE);
            UpdateTable(TableType.VEHICLE_PART);
            RefreshVehicleView();
        }

        public void RefreshVehicleView()
        {
            foreach (var item in ctrlVecs)
            {
                item.Dispose();
            }
            ctrlVecs.Clear();
            panelVecList.Controls.Clear();       // This command will clear all the instances of vehicle information panels created so far
            var vecList = Global.Init.Db.Vechicles.Where(p => p.isUse == 1).ToList();

            int cnt = 0, max = 4;
            foreach (var item in vecList)
            {
                var vec = new CtrlVec();
                vec.SetInitData(item.ID, Global.Init.Db.VecParts.Where(p => p.VEHICLEID == item.ID).Count());
                vec.Parent = panelVecList;

                vec.Top = (int)(cnt/ max) * vec.Height;
                vec.Left = cnt% max * vec.Width;

                if (cnt == 0)
                    vec_first = item.ID;
                cnt++;
                ctrlVecs.Add(new CtrlVec());
            }
        }


        private void buttonForceRefr_Click(object sender, EventArgs e)
        {
            UpdateTable(TableType.ALARM);
            UpdateTable(TableType.PEPSCHEDULE);
            UpdateTable(TableType.VEHICLE_PART);
            UpdateTable(TableType.VEHICLE);
        }

        private bool[] refreshFlag = new bool[Enum.GetValues(typeof(TableType)).Length];

        public void UpdateTable(TableType type)
        {
            refreshFlag[(int)type] = true;
            
        }

        int selectedCmdRow = 0;
        private void dataGridViewCmd_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            selectedCmdRow = e.RowIndex;
            var control = Global.Init.Db.Controllers.SingleOrDefault();
            if (dataGridViewCmd.Rows[selectedCmdRow].Cells[9].Value != null && dataGridViewCmd.Rows[selectedCmdRow].Cells[9].Value.ToString() != "")
            { 
                string val = dataGridViewCmd.Rows[selectedCmdRow].Cells[9].Value.ToString();
                if (Global.Init.RobotList[val].isStop || control.C_state == (int)ControllerState.STOP)
                {
                    buttonCmdDel.Enabled = true;
                    buttonCancel.Enabled = true;
                }
                else
                {
                    buttonCmdDel.Enabled = false;
                    buttonCancel.Enabled = false;
                }
            }
            else
            {
                if (control.C_state == (int)ControllerState.STOP)
                {
                    buttonCmdDel.Enabled = true;
                    buttonCancel.Enabled = true;
                }
                else if (control.C_state == (int)ControllerState.PAUSED)
                {
                    buttonCmdDel.Enabled = true;
                }
                else
                {
                    buttonCmdDel.Enabled = false;
                    buttonCancel.Enabled = false;
                }
                Logger.Inst.Write(CmdLogType.Comm, $"No Vehicle ID");
            }
        }

        private void buttonCancelAbort_Click(object sender, EventArgs e)
        {
            if(selectedCmdRow >= 0 && selectedCmdRow < dataGridViewCmd.Rows.Count)
            {
                var val = dataGridViewCmd.Rows[selectedCmdRow].Cells[12].Value.ToString();
                Logger.Inst.Write(CmdLogType.Comm, $"Oper-CancelJob : {val}");
                if (val != null)
                {
                    OnCancelAbortCmd?.Invoke(this, new CancelAbortEventArgs() { cmdID = val });
                }
            }
        }

        string[] enumstateString = { "QUEUE", "PRE_ASSIGN", "ASSIGN", "SRC_ENROUTE", "SRC_ARRIVED", "SRC_START", "SRC_BEGIN", "SRC_END", "SRC_COMPLETE", "DEPARTED", "DST_ENROUTE", "DST_ARRIVED", "DST_START", "DST_BEGIN", "DST_END", "DST_COMPLETE" };
        private void dataGridViewCmd_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            //try
            //{
            //    var val = dataGridViewCmd.Rows[e.RowIndex].Cells["C_state"].Value;
            //    if (val == null)
            //        return;
            //    int nVal = Convert.ToInt32(val);
            //    if (0 < nVal && nVal < enumstateString.Count())
            //    {
            //        dataGridViewCmd.Rows[e.RowIndex].Cells[3].Value = enumstateString[nVal];
            //    }
            //}
            //catch (Exception err)
            //{
            //    Debug.WriteLine($"dataGridViewCmd_CellFormatting:{err.ToString()}");
            //}
            /*
            try
            {
                if (e.ColumnIndex % 3 == 0)
                {
                    var val = dataGridViewCmd.Rows[e.RowIndex].Cells["State"].Value;

                    string text = string.Empty;
                    if (val == null)
                    {
                        return;
                    }
                    else
                    {
                        text = val.ToString();
                    }

                    bool isMulti = (Convert.ToInt32(dataGridViewCmd.Rows[e.RowIndex].Cells["BatchCount"].Value) > 1) ? true : false;
                    if (text == CmdState.QUEUE.ToString())
                    {
                        if (isMulti)
                        {
                            dataGridViewCmd.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.AliceBlue;
                        }
                        else
                        {
                            dataGridViewCmd.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.SkyBlue;
                        }
                    }
                    else if (text == CmdState.SRC_COMPLETE.ToString())
                    {
                        if (isMulti)
                        {
                            dataGridViewCmd.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Yellow;
                        }
                        else
                        {
                            dataGridViewCmd.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.GreenYellow;
                        }
                    }
                    else if (text == CmdState.DST_COMPLETE.ToString())
                    {
                        if (isMulti)
                        {
                            dataGridViewCmd.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Gray;
                        }
                        else
                        {
                            dataGridViewCmd.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGray;
                        }
                    }
                    else
                    {
                        if (isMulti)
                        {
                            dataGridViewCmd.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Lime;
                        }
                        else
                        {
                            dataGridViewCmd.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LimeGreen;
                        }
                    }

                    bool isFirst = (Convert.ToInt32(dataGridViewCmd.Rows[e.RowIndex].Cells["BatchSeq"].Value) == 1) ? true : false;
                    //var preFont = dataGridViewCmd.Rows[e.RowIndex].DefaultCellStyle.Font;

                    if (isFirst)
                    {
                        //dataGridViewCmd.Rows[e.RowIndex].DefaultCellStyle.Font = new Font(dataGridViewCmd.Rows[e.RowIndex].DefaultCellStyle.Font, FontStyle.Bold);
                        dataGridViewCmd.Rows[e.RowIndex].DividerHeight = 2;
                    }
                    else
                    {
                        //dataGridViewCmd.Rows[e.RowIndex].DefaultCellStyle.Font = new Font(dataGridViewCmd.Rows[e.RowIndex].DefaultCellStyle.Font, FontStyle.Regular);
                        dataGridViewCmd.Rows[e.RowIndex].DividerHeight = 0;
                    }
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine($"dataGridViewCmd_CellFormatting:{err.ToString()}");

            }
            */
        }
                
        int selectedPartRow = 0;
        private void dataGridViewPartition_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            selectedPartRow = e.RowIndex;
        }

        private int selectedAlarmRow = 0;
        private void dataGridViewAlarm_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            selectedAlarmRow = e.RowIndex;
        }

        private void dataGridViewCmd_DataError(object sender, DataGridViewDataErrorEventArgs anError)
        {
            if ((anError.Exception) is ConstraintException)
            {
                DataGridView view = (DataGridView)sender;
                view.Rows[anError.RowIndex].ErrorText = "an error";
                view.Rows[anError.RowIndex].Cells[anError.ColumnIndex].ErrorText = "an error";

                anError.ThrowException = false;
            }
        }

        // Dynamic resizing of the vehicle status panel
        // Detail: Generates one row until the vehicle count is 4. As soon as the vehicle count
        // exceeds 4 it will create another row and move any number of vehicles greater than 4
        // to the new row.
        private void panelVecList_Resize(object sender, EventArgs e)
        {
            //var vec = new CtrlVec();
 
            //// Case: Upto 4 Vehicle info panel
            //if (cnt<=4)
            //{
            //    panelVecList.Height = vec.Height;
            //    panelVecList.Width = vec.Width * cnt;   // variable cnt is is updated in loader function. It counts instances of ctrlvec created on startup
            //}
          
            //// Case: More than 4 vehicle info panel
            //if(cnt>4)
            //{
            //    panelVecList.Height = 2 * vec.Height;
            //    panelVecList.Width = vec.Width * cnt;
            //}

        }

        #region 확인완료
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (refreshFlag[(int)TableType.ALARM])
                {
                    refreshFlag[(int)TableType.ALARM] = false;
                    var almData = Global.Init.Db.Alarms.Select(p => new { p.ID, p.code, p.msg, p.level, p.eventTime, p.releaseTime }).OrderBy(p => p.eventTime).ToList();
                    if (dataGridViewAlarm.Rows.Count != almData.Count())
                    {
                        dataGridViewAlarm.DataSource = null;
                    }

                    if (almData.Count() > 0)
                    {
                        dataGridViewAlarm.DataSource = almData;
                    }
                    else
                    {
                        dataGridViewAlarm.DataSource = new List<alarm>();//.Select(p => new { p.ID, p.code, p.msg, p.level, p.eventTime, p.releaseTime });
                    }
                    dataGridViewAlarm.Refresh();
                    dataGridViewAlarm.ClearSelection();
                }

                if (refreshFlag[(int)TableType.PEPSCHEDULE])
                {
                    refreshFlag[(int)TableType.PEPSCHEDULE] = false;
                    var cmdData = Global.Init.Db.Peps.Select(p => new { p.WORKTYPE, p.C_priority, p.REAL_TIME, p.EXECUTE_TIME, p.WINDOW_TIME, p.C_state, p.TRAYID, p.S_EQPID, p.T_EQPID, p.C_VEHICLEID, p.S_SLOT, p.T_SLOT, p.BATCHID }).OrderBy(p => p.EXECUTE_TIME).ThenBy(p => p.C_priority).ToList();
                    if (dataGridViewCmd.Rows.Count != cmdData.Count())
                    {
                        dataGridViewCmd.DataSource = null;
                    }

                    if (cmdData.Count() > 0)
                    {
                        dataGridViewCmd.DataSource = cmdData;
                    }
                    else
                    {
                        dataGridViewCmd.DataSource = new List<pepschedule>().Select(p => new { p.WORKTYPE, p.C_priority, p.REAL_TIME, p.EXECUTE_TIME, p.WINDOW_TIME, p.C_state, p.TRAYID, p.S_EQPID, p.T_EQPID, p.C_VEHICLEID, p.S_SLOT, p.T_SLOT, p.BATCHID });
                    }

                    dataGridViewCmd.Refresh();
                    dataGridViewCmd.ClearSelection();
                }

                labelExecutetime.Text = ExecuteTime;
                labelWindowtime.Text = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");

            }
            catch (Exception ex)
            {
                Logger.Inst.Write(CmdLogType.Db, $"화면 업데이트 중 오류가 발생하였습니다. {ex.ToString()}");
            }
        }
        /// <summary>
        /// 이렇게 하면 좀 덜 깜빡이나?
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="setting"></param>
        public void GridViewSetDoubleBuffered(DataGridView dgv, bool setting)
        {
            Type dgvType = dgv.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            pi.SetValue(dgv, setting, null);
        }
        /// <summary>
        /// AlarmList 윈도우에서 선택된 알람줄을 삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAlarmClear_Click(object sender, EventArgs e)
        {
            if (selectedAlarmRow >= 0 && selectedAlarmRow < dataGridViewAlarm.Rows.Count)
            {
                var id = dataGridViewAlarm.Rows[selectedAlarmRow].Cells[0].Value.ToString();
                var msg = dataGridViewAlarm.Rows[selectedAlarmRow].Cells[2].Value.ToString();
                var eventtime= dataGridViewAlarm.Rows[selectedAlarmRow].Cells[4].Value.ToString();
                OnAlarmClrCmd?.Invoke(this, new CancelAbortEventArgs() { cmdID = id, msg = msg, event_time = eventtime });
            }
        }
        /// <summary>
        /// Schedule 삭제 명령
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCmdDel_Click(object sender, EventArgs e)
        {            
            if (selectedCmdRow >= 0 && selectedCmdRow < dataGridViewCmd.Rows.Count)
            {
                var val = dataGridViewCmd.Rows[selectedCmdRow].Cells[12].Value.ToString();

                if (val != null)
                {
                    if (ControllerState.PAUSED == Global.Init.RobotList[vec_first].controllerState)
                    {                        
                        if (dataGridViewCmd.Rows[selectedCmdRow].Cells[5].Value == null)
                        {
                            Logger.Inst.Write(CmdLogType.All, $"Oper-DeleteJob : {val}");
                            OnDeleteCmd?.Invoke(this, new DeleteEventArgs() { cmdID = val });
                        }
                        else
                        {
                            Logger.Inst.Write(CmdLogType.All, $"Job in progress. Please Delete it after Stop.");
                        }

                    }
                    else
                    {
                        Logger.Inst.Write(CmdLogType.All, $"Oper-DeleteJob : {val}");
                        OnDeleteCmd?.Invoke(this, new DeleteEventArgs() { cmdID = val });
                    }
                }
            }
        }
        #endregion//확인완료
    }


    public class DeleteEventArgs : EventArgs
    {
        public string cmdID = "";
    }

    public enum JobCancelArgs
    {
        Cancel,
        Abort,
    }
    public class CancelAbortEventArgs : EventArgs
    {
        public string cmdID = "";
        public string msg = "";
        public string event_time = "";
    }
}
