using FSMPlus.Forms;
using FSMPlus.Logic;
using FSMPlus.Ref;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace FSMPlus
{
    public partial class FormMain : Form
    {
        private Configuration _cfg = Configuration.Init;
        private MainHandler _MainHandler = new MainHandler();
        private Logger _log = Logger.Inst;
        private FormMonitor _TableFm;
        private Frm_Log _LogFm;

        private Timer tmrMapDraw = new Timer();
        private Timer tmrCurTime = new Timer();

        private bool _FormLoaded = false;

        public FormMain(string version)
        {
            InitializeComponent();
            #region 확인완료
            labelVersion.Text = version;

            _MainHandler.OnChangeTableData += _MainHandler_OnChangeTableData;

            tmrMapDraw.Interval = 300;
            tmrMapDraw.Tick += TmrMapDraw_Tick;
            tmrMapDraw.Enabled = true;

            tmrCurTime.Interval = 500;
            tmrCurTime.Tick += TmrCurTime_Tick;
            tmrCurTime.Enabled = true;
            #endregion//확인완료

            _TableFm = new FormMonitor();
            _TableFm.OnDeleteCmd += _TableFm_OnDeleteCmd;
            _TableFm.OnCancelAbortCmd += _TableFm_OnCancelAbortCmd;
            _TableFm.OnAlarmClrCmd += _TableFm_OnAlarmClrCmd;

            _MainHandler.mapDrawer.OnDrawCompleteEvent += _MapDrawer_OnDrawCompleteEvent;
            _MainHandler.mapDrawer.OnClickItem += _MapDrawer_OnClickItem;
            _MainHandler.OnChangeRvConnection += OnChangeRvConnection;

            if (File.Exists(LastMapFileName))
            {
                LoadMapFile(File.ReadAllText(LastMapFileName));
                _MainHandler.SetUnitVehicleInDrawer();
            }
            
            _LogFm = new Frm_Log();
            _log.OnWriteLog += _Log_OnMsg;
            _log.MakeVecHdl();
            InitVec();
            Logger.Inst.Write("VEHICLE01", CmdLogType.All, $"MPlus Start. Ver.{version}");
        }

        private void OnExecuteTime(object sender, string e)
        {
            _TableFm.ExecuteTime = (string)e;
        }

        private void OnChangeRvConnection(object sender, EventArgRvtate e)
        {
            if (_FormLoaded)
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    labelOnlineState.ImageIndex = (e.state == RvState.disconnected) ? 0 : 1;
                    if (e.state == RvState.connected)
                    {
                        btnRvConnection.Text = "LIVE";
                        btnRvConnection.BackColor = Color.Chartreuse;
                    }
                    else
                    {
                        btnRvConnection.Text = "DIE";
                        btnRvConnection.BackColor = Color.Red;
                    }
                }));
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            if (panelMainViewer.Controls.Count > 0)
            {
                panelMainViewer.Controls.RemoveAt(0);
            }

            _TableFm.TopLevel = false;
            _TableFm.Dock = DockStyle.Fill;
            panelMainViewer.Controls.Add(_TableFm);
            _TableFm.Show();

            _LogFm.TopLevel = false;
            _LogFm.Dock = DockStyle.Fill;
            splitContainer2.Panel2.Controls.Add(_LogFm);
            _LogFm.Show();

            _FormLoaded = true;
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("정말로 종료 하시겠습니까?", "확인", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            else
            {
                _FormLoaded = false;
                _MainHandler.ClosePorcess();

                System.Threading.Thread.Sleep(1000);
                Logger.Inst.LoggerClose();
            }
        }

        

        private void buttonConfig_Click(object sender, EventArgs e)
        {
            FormConfiguration fm = new FormConfiguration();
            fm.OnRecalculateDist += Fm_OnRecalculateDist;
            fm.OnRecalculateDistZero += Fm_OnRecalculateDistZero;
            fm.OnRecalculateDistAdd += Fm_OnRecalculateDistAdd;
            fm.OnRefreshGoalInfo += Fm_OnRefreshGoalInfo;
            fm.OnCheckGoalInfo += Fm_OnCheckGoalInfo;

            fm.ShowDialog();
            _MainHandler.SetUnitNamesInDrawer();

            _TableFm.UpdateTable(TableType.VEHICLE);
            _TableFm.UpdateTable(TableType.VEHICLE_PART);
        }

        private void Fm_OnCheckGoalInfo(object sender, EventArgs e)
        {
            _MainHandler.GetGoalInfoCheckFromFile();
        }

        private void Fm_OnRefreshGoalInfo(object sender, EventArgs e)
        {
            _MainHandler.GetGoalInfoFromFile();

        }

        private void Fm_OnRecalculateDistAdd(object sender, EventArgs e)
        {
            _MainHandler.CreateDistanceTableByNewGoal();
        }

        private void Fm_OnRecalculateDistZero(object sender, EventArgs e)
        {
            _MainHandler.DistanceCalcByMinus();
        }

        private void Fm_OnRecalculateDist(object sender, EventArgs e)
        {
            _MainHandler.CreateDistanceTable();
        }

        private void buttonZoomIn_Click(object sender, EventArgs e)
        {
            _MainHandler.mapDrawer.Scale += 0.01;
        }

        private void buttonZoomOut_Click(object sender, EventArgs e)
        {
            if (_MainHandler.mapDrawer.Scale <= 0)
            {
                return;
            }
            _MainHandler.mapDrawer.Scale -= 0.01;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _MainHandler.mapDrawer.Scale = 0.01f;
        }

        private void buttonMapFileSel_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog()
            {
                DefaultExt = "map",
                Filter = "Map Files(*.map)|*.map",
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                LoadMapFile(dlg.FileName);
            }
        }

        private bool _MapLoaded = false;
        private readonly string LastMapFileName = "LastMapFile.txt";
        public void LoadMapFile(string filePath)
        {
            _MapLoaded = _MainHandler.mapDrawer.SetMapFile(filePath);
            if (_MapLoaded)
            {
                textBoxMapFileName.Text = filePath;
                File.WriteAllText(LastMapFileName, filePath);
                _MainHandler.SetUnitNamesInDrawer();
            }
        }

        private void checkBoxViewItems_Click(object sender, EventArgs e)
        {
            VehicleItems items = new VehicleItems();
            if (checkBoxShowDest.Checked) items |= VehicleItems.Dest;
            if (checkBoxShowJob.Checked) items |= VehicleItems.Job;
            if (checkBoxShowStatus.Checked) items |= VehicleItems.Status;
            if (checkBoxShowCharge.Checked) items |= VehicleItems.Charge;
            if (checkBoxShowTrace.Checked) items |= VehicleItems.Trace;

            _MainHandler.mapDrawer.SetShowVehicleItems(items);
        }

        private void _Log_OnMsg(object sender, WriteLogArgs e)
        {
            switch (e.cmd)
            {
                case LOGCMD.Job:
                    _LogFm.SetLogMsg(e.cmd, e.nID, e.type, e.msg);
                    break;
                case LOGCMD.Vehicle:
                    _LogFm.SetLogMsg(e.cmd, e.nID, e.type, e.msg);
                    break;
                case LOGCMD.Etc:
                default:
                    break;
            }
        }



        private void pictureBoxMap_DoubleClick(object sender, EventArgs e)
        {
            var mouse = e as MouseEventArgs;
            _MainHandler.mapDrawer.DoubleClickImage(mouse.Location);
        }

        
        private ToolTip goalViewInMap = new ToolTip();
        private void pictureBoxMap_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseButtons == MouseButtons.Right)
            {
                Point changePoint = new Point(e.Location.X - MouseDownPos.X, e.Location.Y - MouseDownPos.Y);
                panel4.AutoScrollPosition = new Point(-panel4.AutoScrollPosition.X - changePoint.X, -panel4.AutoScrollPosition.Y - changePoint.Y);
                
                return;
            }
            //Debug.WriteLine($"{e.Location.X}");
            var data = _MainHandler.mapDrawer.MouseMoveImage(e.Location);

            if (data != null && data.Count() > 0)
            {
                if (goalViewInMap != null)
                {
                    return;
                }

                string showMsg = string.Empty;
                foreach (var item in data)
                {
                    var info = DbHandler.Inst.Units_FS.Where(p => p.GOALNAME.ToUpper() == item.ItemName.ToUpper()).FirstOrDefault();
                    if (info != null)
                    {
                        showMsg += $"ID:{info.ID}\r\nGoal:{info.GOALNAME}\r\n";

                        var ableVecs = DbHandler.Inst.Zones.Where(p => p.UNITID.ToUpper() == info.GOALNAME.ToUpper()).ToList();

                        string vecList = "Vec:";
                        if (ableVecs.Count > 0)
                        {
                            foreach (var subItem in ableVecs)
                            {
                                vecList += $"{subItem.VEHICLEID}, ";
                            }
                        }
                        else
                        {
                            vecList += "All";
                        }
                        vecList += "\r\n\r\n";
                        showMsg += vecList;
                    }
                }

                goalViewInMap = new ToolTip();
                goalViewInMap.ShowAlways = true;
                goalViewInMap.Show(showMsg, pictureBoxMap, e.Location);
            }
            else
            {
                if (goalViewInMap != null)
                {
                    goalViewInMap.Dispose();
                    goalViewInMap = null;
                }
            }
        }

        private void comboBoxVecFind_SelectedIndexChanged(object sender, EventArgs e)
        {
            var pt = _MainHandler.GetVecPointInDrawer(comboBoxVecFind.SelectedItem.ToString());
            pt.X -= panel4.Width / 2;
            pt.Y -= panel4.Height / 2;
            panel4.AutoScrollPosition = pt;
        }

        private void comboBoxVecFind_DropDown(object sender, EventArgs e)
        {
            comboBoxVecFind.Items.Clear();
            var vecNames = DbHandler.Inst.Vechicles.Where(p => p.isUse == 1).Select(p => p.ID).ToList();
            comboBoxVecFind.Items.Add("----------");
            foreach (var item in vecNames)
            {
                comboBoxVecFind.Items.Add(item);
            }
        }

        private void DragMoveImage(int offsetX, int offsetY)
        {
            var past = panel4.AutoScrollPosition;
            past.X *= -1;
            past.Y *= -1;
            past.Offset(offsetX, offsetY);
            panel4.AutoScrollPosition = past;
        }

        private Point MouseDownPos;
        private void pictureBoxMap_MouseDown(object sender, MouseEventArgs e)
        {
            MouseDownPos = e.Location;
        }

        FormLegendHint LegendFm = null;

        private void buttonLegend_Click(object sender, EventArgs e)
        {
            if (LegendFm == null)
            {
                LegendFm = new FormLegendHint();
                LegendFm.FormClosed += LegendFm_FormClosed;
            }
            LegendFm.Show();
        }

        private void LegendFm_FormClosed(object sender, FormClosedEventArgs e)
        {
            LegendFm = null;
        }

        #region 확인완료
        private void _MapDrawer_OnClickItem(object sender, ClickedItemArgs e)
        {
            GoalEditor zone = new GoalEditor(e.name);
            zone.StartPosition = FormStartPosition.Manual;
            zone.Location = Cursor.Position;
            zone.ShowDialog();
            _MainHandler.SetUnitVehicleInDrawer();
        }
        private void _MapDrawer_OnDrawCompleteEvent(Image sender)
        {
            try
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    pictureBoxMap.Image = sender;

                    //try
                    //{
                    //    var pos = _MainHandler._MapDraw.dictDrawRobot.First().Value;
                    //    pos.X -= panel1.Width / 2;
                    //    pos.Y -= panel1.Height / 2;
                    //    panel1.AutoScrollPosition = pos;
                    //}
                    //catch
                    //{

                    //}
                }));
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// 실제 작업 시작전일 때만 취소 가능하다.(QUQUQ,PRE-ASSIGN,ASSIGN,SRC_ENROUTE,SRC_ARRIVED)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void _TableFm_OnCancelAbortCmd(object sender, CancelAbortEventArgs e)
        {
            var job = DbHandler.Inst.Peps.Where(p => p.BATCHID == e.cmdID).SingleOrDefault();
            if (job == null)
            {
                return;
            }
            Logger.Inst.Write(job.C_VEHICLEID, CmdLogType.Db, $"작업 취소/중단 : {e.cmdID}");
            //oper init action

            vehicle vec = null;
            if (job.C_VEHICLEID != null && job.C_VEHICLEID != "")
            {
                Global.Init.RobotList[job.C_VEHICLEID].STOP();
                await Task.Delay(1000);
                Global.Init.RobotList[job.C_VEHICLEID].CANCEL();
                vec = DbHandler.Inst.Vechicles.Where(p => p.ID == job.C_VEHICLEID).Single();
            }
            else
            {
                vec = new vehicle();
                vec.ID = "PROGRAM";
            }
                        
            //vec = _MainHandler.Db.Vechicles.Where(p => p.ID == job.C_VEHICLEID).ToList();
            var args = new SendJobToVecArgs1() { vecID = vec.ID, job = job, vec = vec };

            List<unit_fs> units = new List<unit_fs>();
            string dst_eqpid = string.Empty;

            if ((job.BATCHID.Contains("CFS") && job.WORKTYPE == "FSO") || !(job.BATCHID.Contains("CFS")))
                units = DbHandler.Inst.Units_FS.Where(p => p.ID == job.S_EQPID).ToList();

            if ((job.BATCHID.Contains("CFS") && job.WORKTYPE == "FSI") || !(job.BATCHID.Contains("CFS")))
            {
                dst_eqpid = job.T_EQPID.Split(',')[0];
                units = DbHandler.Inst.Units_FS.Where(p => p.ID == dst_eqpid).ToList();
            }

            if (_cfg.Data.UseRv)
            {
                Proc_Atom.Init.PROC_ATOM(args, units[0], units[0], ProcStep.Job_Cancel);       
            }

            if (vec.ID != "PROGRAM")
            {
                vec.C_BATCHID = null;
                vec.isAssigned = 0;                
                //_MainHandler.Db.DbUpdate(TableType.VEHICLE);

                var vecparts = DbHandler.Inst.VecParts.Where(p => p.VEHICLEID == vec.ID).ToList();
                string[] trays = job.TRAYID.Split(',');
                if (job.C_state < 8 || job.C_state == 28)
                {
                    foreach (var v in vecparts)
                    {
                        for (int i = 0; i < trays.Count(); i++)
                        {
                            if (v.C_trayId == trays[i])
                                v.C_trayId = null;
                        }
                    }
                    //_MainHandler.Db.DbUpdate(TableType.VEHICLE_PART);
                    DbHandler.Inst.DbUpdate(true, new TableType[] { TableType.VEHICLE, TableType.VEHICLE_PART });
                }
            }
        }
        /// <summary>
        /// alarm 삭제 버튼 눌렀을 때
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _TableFm_OnAlarmClrCmd(object sender, CancelAbortEventArgs e)
        {
            if (MessageBox.Show("알람 강제 클리어하시겠습니까?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Logger.Inst.Write(CmdLogType.Db, $"알람 강제 클리어: ID={e.cmdID}");

                var val = DbHandler.Inst.Alarms.Where(p => p.ID.ToString() == e.cmdID && p.msg == e.msg && p.eventTime.ToString() == e.event_time).SingleOrDefault();
                if (val != null)
                {
                    _MainHandler.VEP.AlarmControl(false, val.ID, val.code);
                }
            }
        }
        /// <summary>
        /// pepschedule 삭제 버튼 눌렀을 때
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">첫번째 열은 idx, 변수 이름 및 int 값인데 DeleteEventArgs 변경없이 사용</param>
        private void _TableFm_OnDeleteCmd(object sender, FSMPlus.Forms.DeleteEventArgs e)
        {
            Logger.Inst.Write(CmdLogType.Db, $"작업 삭제 : {e.cmdID}");
            _MainHandler.DeleteCmd(e.cmdID);
            // jm.choi - 190326
            // job 삭제 시 vehicle data 초기화
            _MainHandler.ResetCmd(e.cmdID);
            _TableFm.buttonCancel.Enabled = false;
            _TableFm.buttonCmdDel.Enabled = false;
        }
        /// <summary>
        /// 0.5 초 주기로 시간 변경을 화면에 표시하는 타이머
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TmrCurTime_Tick(object sender, EventArgs e)
        {
            labelCurTime.Text = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");

            _TableFm.ExecuteTime = VSP.Init.ExecuteTime;
        }
        /// <summary>
        /// 0.3 초 주기로 맵을 다시 그려주는 함수를 호출하는 타이머
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TmrMapDraw_Tick(object sender, EventArgs e)
        {
            if (_MapLoaded)
            {
                _MainHandler.mapDrawer.RenderingImage();
            }
        }
        /// <summary>
        /// DbHandler 에서 db 수정에 따른 결과를 FormMonitor에 반영한다
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _MainHandler_OnChangeTableData(object sender, TableUpdateArgs e)
        {
            if (this.Visible)
            {
                foreach (var item in e.target)
                {
                    switch (item)
                    {
                        case TableType.ALARM:
                            _TableFm.UpdateTable(TableType.ALARM);
                            break;
                        case TableType.ALARM_DEFINE:
                            break;
                        case TableType.PEPSCHEDULE:
                            _TableFm.UpdateTable(TableType.PEPSCHEDULE);
                            break;
                        case TableType.CONTROLLER:
                            _TableFm.UpdateTable(TableType.CONTROLLER);
                            break;
                        case TableType.UNIT:
                            break;
                        case TableType.VEHICLE:
                            _TableFm.UpdateTable(TableType.VEHICLE);
                            break;
                        case TableType.VEHICLE_PART:
                            _TableFm.UpdateTable(TableType.VEHICLE_PART);
                            break;
                        case TableType.ZONE:
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// auto, pause 의 상태를 시각적으로 잘 전달하기 위해 bar 를 toggle
        /// </summary>
        private bool toggle = false;
        private Color statusColor = Color.White;
        private Color statusColorSub = Color.Gray;

        private void timerStatusBar_Tick(object sender, EventArgs e)
        {
            if (toggle)
            {
                toggle = false;
                panelStatusColorBar.BackColor = statusColor;
            }
            else
            {
                toggle = true;
                panelStatusColorBar.BackColor = statusColorSub;
            }

        }

        /// <summary>
        /// auto 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAuto_Click(object sender, EventArgs e)
        {
            if (_MainHandler.ChangeTscStatus(ControllerState.AUTO))
            {
                OnChangeTscState(ControllerState.AUTO);
            }
        }

        /// <summary>
        /// pause 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonPause_Click(object sender, EventArgs e)
        {
            if (buttonPause.Text == "Pause")
            {
                if (_MainHandler.ChangeTscStatus(ControllerState.PAUSED))
                {
                    OnChangeTscState(ControllerState.PAUSED);
                }
            }
            else
            {
                if (_MainHandler.ChangeTscStatus(ControllerState.AUTO))
                {
                    OnChangeTscState(ControllerState.RESUME);
                }
            }
        }

        /// <summary>
        /// Stop 버튼 추가 jm.choi - 190306
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonStop_Click(object sender, EventArgs e)
        {
            if (_MainHandler.ChangeTscStatus(ControllerState.STOP))
            {
                OnChangeTscState(ControllerState.STOP);
            }
        }

        /// <summary>
        /// auto,pause 버튼 클릭에 따른 ui 업데이트
        /// </summary>
        /// <param name="state"></param>
        private void OnChangeTscState(ControllerState state)
        {
            if (_FormLoaded)
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    switch (state)
                    {
                        case ControllerState.PAUSED:
                            statusColor = Color.Orange;
                            statusColorSub = Color.OrangeRed;
                            StsInit.Visible = false;
                            StsAuto.Visible = false;
                            StsPause.Visible = true;
                            StsStop.Visible = false;
                            buttonAuto.Enabled = false;
                            buttonPause.Enabled = true;
                            buttonStop.Enabled = true;
                            buttonPause.Text = "Resume";
                            _TableFm.buttonCancel.Enabled = false;
                            _TableFm.buttonCmdDel.Enabled = true;
                            break;
                        case ControllerState.AUTO:
                            statusColor = Color.Lime;
                            statusColorSub = Color.Green;
                            StsInit.Visible = false;
                            StsAuto.Visible = true;
                            StsPause.Visible = false;
                            StsStop.Visible = false;
                            buttonAuto.Enabled = false;
                            buttonPause.Enabled = true;
                            buttonStop.Enabled = true;
                            _TableFm.buttonCancel.Enabled = false;
                            _TableFm.buttonCmdDel.Enabled = false;
                            break;
                        case ControllerState.RESUME:
                            statusColor = Color.Lime;
                            statusColorSub = Color.Green;
                            StsInit.Visible = false;
                            StsAuto.Visible = true;
                            StsPause.Visible = false;
                            StsStop.Visible = false;
                            buttonAuto.Enabled = false;
                            buttonPause.Enabled = true;
                            buttonStop.Enabled = true;
                            buttonPause.Text = "Pause";
                            _TableFm.buttonCancel.Enabled = false;
                            _TableFm.buttonCmdDel.Enabled = false;
                            break;
                        // Stop 버튼 추가 jm.choi - 190306
                        case ControllerState.STOP:
                            statusColor = Color.Red;
                            statusColorSub = Color.OrangeRed;
                            StsInit.Visible = false;
                            StsAuto.Visible = false;
                            StsPause.Visible = false;
                            StsStop.Visible = true;
                            buttonAuto.Enabled = true;
                            buttonPause.Enabled = false;
                            buttonStop.Enabled = false;
                            _TableFm.buttonCancel.Enabled = true;
                            _TableFm.buttonCmdDel.Enabled = true;
                            break;
                        default:
                            StsInit.Visible = true;
                            StsAuto.Visible = false;
                            StsPause.Visible = false;
                            StsStop.Visible = false;
                            buttonAuto.Enabled = true;
                            buttonPause.Enabled = false;
                            buttonStop.Enabled = false;
                            _TableFm.buttonCancel.Enabled = false;
                            _TableFm.buttonCmdDel.Enabled = false;
                            break;
                    }
                }));
            }
        }

        private void InitVec()
        {
            var vec = DbHandler.Inst.Vechicles.Where(p => p.ID.Contains("VEHICLE")).ToList();

            foreach (var x in vec)
            {
                x.C_BATCHID = null;
                x.isAssigned = 0;
            }
            DbHandler.Inst.DbUpdate(TableType.VEHICLE);
        }

        #endregion//확인완료
    }

}
