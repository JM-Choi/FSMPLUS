using FSMPlus.Ref;
using FSMPlus.Vehicles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FSMPlus.Logic
{
    public class MainHandler : Global
    {
        public event EventHandler<TableUpdateArgs> OnChangeTableData;
        public event EventHandler<EventArgRvtate> OnChangeRvConnection;

        public MapDrawer mapDrawer
        {
            get { return _MapDraw; }
        }

        private string jobType = string.Empty;

        public MainHandler()
        {
            _MainHandler = this;
            var vecs = Db.Vechicles.Where(p => p.isUse == 1 && p.TRANSFERTYPE == "FS").ToList();
            ControllerSetup();
            VehicleSetup(vecs);
            RendezvousSetup(vecs);
            JobProcessSetup(vecs);
            VspSetup(vecs);
        }

        public void ControllerSetup()
        {
            var con = Db.Controllers.SingleOrDefault();
            if (con == null)
            {
                Debug.WriteLine("ControllerSetup : control data is null! error");
                return;
            }
            con.C_state = (int)ControllerState.INIT;
            con.C_onlineState = (int)ControllerOnlineState.OFFLINE;
            Db.DbUpdate(true, new TableType[] { TableType.CONTROLLER });
        }

        public async void VehicleSetup(List<vehicle> vecs)
        {
            await Task.Delay(1);

            RobotList.Clear();
            _MapDraw.ClearRobot();

            setRobots = new SetRobot[vecs.Count];
            int count = 0;

            foreach (var v in vecs)
            {
                if (v.isUse == 0)
                    continue;

                _MapDraw.AddRobot(new DrawRobotInfo() { ipAddress = v.IP, name = v.ID, status = VehicleMode.AUTO });

                setRobots[count] = new makeRobot();
                RobotList[v.ID] = (setRobots[count].CreateRobot(v.ID, v.IP, v.port, v.remoteIP, v.remotePort));
                RobotList[v.ID].SocketInit();
                count++;

            }
            //if (Cfg.Data.UseRvSocket)
            //{
            //    var vehicle2 = new RvSocket();

            //    vehicle2.RvSetInfo("127.0.0.1", 8002);
            //    vehicle2.RvSetupTimer();
            //    RvSocketList["PROGRAM"] = vehicle2;
            //}

        }


        private void JobProcessSetup(List<vehicle> vecs)
        {
            void CreateJobprocess(vehicle v)
            {
                JobProcList[v.ID] = new JobProccess(v);
                JobProcList[v.ID].OnCallExecuteTime += Vsp._Vsp_OnCallExecuteTime;
                RobotList[v.ID].OnRecvMsg += VEP.Vehicle_OnRecvMsg;
                RobotList[v.ID].OnChageConnected += JobProcList[v.ID].Vehicle_OnChageConnected;
            }

            //var vecs = Db.Vechicles.Where(p => p.isUse == 1).ToList();
            foreach (var v in vecs)
            {
                CreateJobprocess(v);
            }

            vehicle vec = new vehicle();
            vec.ID = "PROGRAM";
            JobProcList["PROGRAM"] = new JobProccess(vec);
            //Vsp.OnSendPreTempDown += RvSenderList["PROGRAM"]._Vsp_OnSendPreTempDown;
        }

        private void VspSetup(List<vehicle> vecs)
        {
#pragma warning disable CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.
            Vsp.StartVsp();
#pragma warning restore CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.
            //var vecs = Db.Vechicles.Where(p => p.isUse == 1).ToList();
            foreach (var v in vecs)
            {
                Vsp.OnCallCmdProcedure += JobProcList[v.ID].OnCallCmdProcedure;
            }

        }

        public void RvLsnerOnChangeRvConnection(object sender, EventArgRvtate e)
        {
            OnChangeRvConnection?.Invoke(this, e);
        }

        public void ClosePorcess()
        {
            Vsp.IsStop = true;
            var cont = Db.Controllers.SingleOrDefault();
            if (cont != null)
            {
                cont.C_onlineState = (int)ControllerOnlineState.OFFLINE;
                cont.C_state = (int)ControllerState.INIT;
                Db.DbUpdate(TableType.CONTROLLER);
            }
            var vec = Db.Vechicles.Where(p => p.isUse == 1 && p.TRANSFERTYPE == "FS").ToList();
            foreach (var item in vec)
            {
                RobotList[item.ID].Dispose();                
            }
            foreach (var v in vec)
            {
                JobProcList[v.ID].IsStop = true;
                RvMsgList[v.ID].jobcancel_check = true;
            }
            JobProcList["PROGRAM"].IsStop = true;
            System.Threading.Thread.Sleep(1500);
        }


        public void CreateDistanceTable()
        {
            if (_MapDraw.CurrentMapFile != null)
            {
                Db.DeleteAll(typeof(distance));
                Db.DbUpdate(false);

                var list = Db.Units_FS.Select(p => p.GOALNAME).ToList();

                for (int i = 0; i < list.Count; i++)
                {
                    for (int j = i + 1; j < list.Count; j++)
                    {
                        Db.Add(new distance() { UNITID_start = list[i], UNITID_end = list[j], distance1 = -1 });
                    }
                }

                Db.DbUpdate(false);

                GetDistanceFromVehicle(Db.Distances.ToList());
                Db.DbUpdate(false);
            }
        }

        public void DistanceCalcByMinus()
        {
            GetDistanceFromVehicle(Db.Distances.Where(p => p.distance1 < 0).ToList());
            Db.DbUpdate(false);
        }

        public void CreateDistanceTableByNewGoal()
        {
            if (_MapDraw.CurrentMapFile != null)
            {
                var usedGoal = Db.Distances.Select(p => p.UNITID_start).Distinct().ToList();
                //var currentGoal = _MapDraw.GoalList.Select(p => p.goalName).ToList();
                var currentGoal = Db.Units_FS.Select(p => p.GOALNAME).ToList();
                //사용중인 골에 없는것만 찾아낸다.
                var compareGoal = currentGoal.Except(usedGoal).ToList();

                for (int i = 0; i < compareGoal.Count; i++)
                {
                    for (int j = 0; j < currentGoal.Count; j++)
                    {
                        if (compareGoal[i].Equals(currentGoal[j]))
                        {
                            continue;
                        }
                        Db.Add(new distance() { UNITID_start = compareGoal[i], UNITID_end = currentGoal[j], distance1 = -1 });
                    }
                }
                Db.DbUpdate(false);
                DistanceCalcByMinus();
                Db.DbUpdate(false);
            }
        }

        public async void GetDistanceFromVehicle(List<distance> list)
        {

            string msg = "차량으로부터 데이터를 얻어오는 중 입니다.";
            AlphaMessageForm.FormMessage form = new AlphaMessageForm.FormMessage(msg, AlphaMessageForm.MsgType.Info, AlphaMessageForm.BtnType.Cancel);
            form.Show();

            Task<int>[] vecTaskList = new Task<int>[RobotList.Count];
            int[] vecTaskTarget = new int[RobotList.Count];

            var vecList = RobotList.Values.ToList();

            int listIndex = 0;
            while (listIndex < list.Count)
            {
                if (!form.Visible)
                {
                    break;
                }
                for (int i = 0; i < vecList.Count; i++)
                {
                    if (vecTaskList[i] == null)
                    {
                        vecTaskList[i] = vecList[i].CALCDISTANCE_BTW(list[listIndex].UNITID_start, list[listIndex].UNITID_end);
                        vecTaskTarget[i] = listIndex;
                    }
                    else
                    {
                        if (vecTaskList[i].IsCompleted)
                        {
                            list[vecTaskTarget[i]].distance1 = vecTaskList[i].Result;
                            vecTaskList[i] = null;
                            listIndex++;
                            form.ChangeMainText($"{msg}\r\n{listIndex + 1}/{list.Count}");
                        }
                    }
                }
                await Task.Delay(1);
            }

            form.Close();
        }

        public void GetGoalInfoFromFile()
        {
            if (_MapDraw.CurrentMapFile != null)
            {
                //Db.DeleteAll(typeof(unit));
                //Db.DeleteAll(typeof(standby));
                //Db.DbUpdate(false);

                var list = _MapDraw.GoalList.ToList();
                foreach (var item in list)
                {
                    unit_fs unt = Db.Units_FS.Where(p => p.GOALNAME == item.goalName).FirstOrDefault();
                    if (unt == null)
                        continue;

                    unt.loc_x = item.pos.X;
                    unt.loc_y = item.pos.Y;
                    unt.direction = item.angle.ToString();
                }

                Db.DbUpdate(false);
            }
        }

        public void GetGoalInfoCheckFromFile()
        {
            if (_MapDraw.CurrentMapFile != null)
            {
                var list = _MapDraw.GoalList.ToList();
                foreach (var item in list)
                {
                    if (Db.Units_FS.Where(p => p.GOALNAME == item.goalName).Any())
                    {
                        continue;
                    }
                    //Db.DbContext.Add(new unit() { GOALNAME = item.goalName, ID = item.goalName, loc_x = item.pos.X, loc_y = item.pos.Y, direction = item.angle.ToString(), goaltype = (int)EqpGoalType.K5_Default });
                    Db.Add(new unit() { GOALNAME = item.goalName, ID = item.goalName, loc_x = item.pos.X, loc_y = item.pos.Y, direction = item.angle.ToString(), goaltype = (int)EqpGoalType.rozze_Default });
                    Db.Add(new standby() { GOALNAME = item.goalName, VEHICLEID = null, loc_x = item.pos.X, loc_y = item.pos.Y });
                }

                Db.DbUpdate(false);
            }
        }

        public void SetUnitNamesInDrawer()
        {
            var dict = Db.Units_FS.ToDictionary(p => p.GOALNAME, o => o.ID);
            _MapDraw.SetUnitList(dict);
        }

        public void SetUnitVehicleInDrawer()
        {
            var dict = Db.Zones.GroupBy(p => p.UNITID);
            Dictionary<string, string[]> data = new Dictionary<string, string[]>();
            foreach (var item in dict)
            {
                List<string> vecList = new List<string>();
                foreach (var sub in item)
                {
                    vecList.Add(sub.VEHICLEID);
                }
                data.Add(item.Key, vecList.ToArray());
            }

            _MapDraw.SetUnitVehcileNameList(data);
        }

        public System.Drawing.Point GetVecPointInDrawer(string id)
        {
            if (_MapDraw.dictDrawRobot.ContainsKey(id))
            {
                var pt = _MapDraw.dictDrawRobot[id];
                return pt;
            }
            else
            {
                return new System.Drawing.Point();
            }
        }

        #region 확인완료

        /// <summary>
        /// pepschedule 을 삭제한다
        /// </summary>
        /// <param name="cmdId">pepschedule idx</param>
        public void DeleteCmd(string cmdId)
        {
            var cmd = Db.Peps.Where(p => p.BATCHID == cmdId).Single();
            if (cmd != null)
            {
                Db.CopyCmdToHistory(cmd.ID.ToString());
                Db.Delete(cmd);
                Db.DbUpdate(true, new TableType[] { TableType.PEPSCHEDULE });
            }
        }
        /// <summary>
        /// jm.choi - 190326
        /// VEHICLE 을 초기화한다
        /// </summary>
        /// <param name="cmdId">pepschedule idx</param>
        public void ResetCmd(string cmdId)
        {
            var robot_count = Db.Vechicles.Where(p => p.C_BATCHID == cmdId).Count();

            if (robot_count > 0)
            {
                var robot = Db.Vechicles.Where(p => p.C_BATCHID == cmdId).Single();

                ((vehicle)robot).C_BATCHID = null;
                ((vehicle)robot).C_lastArrivedUnit = null;
                ((vehicle)robot).isAssigned = null;
                Db.DbUpdate(TableType.VEHICLE);    // FormMonitor update
            }
        }
        /// <summary>
        /// DbHandler 에서 DbUpdate(~~) 수행시 이벤트를 수신하여 FormMain에 전송
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void _Db_OnChangeTableData(object sender, TableUpdateArgs e)
        {
            OnChangeTableData?.Invoke(sender, e);
        }
        /// <summary>
        /// FormMain 에서 Auto,Pause 누를 때 vehicle 에 전송할 것이다.
        /// </summary>
        /// <param name="toState"></param>
        public bool ChangeTscStatus(object obj)
        {
            // 초기화. 특별의미없음
            int onlineState = 0, state = 0;
            int old_state = 0;
            var control = Db.Controllers.SingleOrDefault();
            if (control == null)
            {
                AlphaMessageForm.FormMessage form = new AlphaMessageForm.FormMessage("", AlphaMessageForm.MsgType.Info, AlphaMessageForm.BtnType.Cancel);
                form.SetMsg($"ChangeTscStatus, Controller is null");
                form.ShowDialog();
                return false;
            }

            // 과거 상태
            old_state = control.C_state;

            // 버튼 상태(auto,pause 버튼)
            if (obj is ControllerState)
            {
                state = (int)obj;
                if (old_state == state)
                    return false;

                var list = Db.Vechicles.Where(p => p.isUse == 1).ToList();
                foreach (var item in list)
                {
                    if ((VehicleMode)item.C_mode == VehicleMode.INIT || (VehicleMode)item.C_mode == VehicleMode.MANUAL
                        || (VehicleMode)item.C_mode == VehicleMode.AUTO || (VehicleMode)item.C_mode == VehicleMode.TEACHING)
                    {
                        // auto 이면
                        if (state == (int)ControllerState.AUTO)
                        {
                            onlineState = (int)ControllerOnlineState.ONLINE;
                            if ((VehicleMode)item.C_mode != VehicleMode.TEACHING)
                                RobotList[item.ID].RESUME();
                            else
                                RobotList[item.ID].controllerState = ControllerState.AUTO;

                        }
                        else // pause
                        if (state == (int)ControllerState.PAUSED)
                        {
                            onlineState = (int)ControllerOnlineState.OFFLINE;
                            if ((VehicleMode)item.C_mode != VehicleMode.TEACHING)
                                RobotList[item.ID].PAUSE();
                            else
                                RobotList[item.ID].controllerState = ControllerState.PAUSED;

                        }
                        // stop 버튼 추가 jm.choi - 190306
                        else
                        if (state == (int)ControllerState.STOP)
                        {
                            onlineState = (int)ControllerOnlineState.OFFLINE;
                            RobotList[item.ID].controllerState = ControllerState.STOP;
                        }
                    }
                }
            }

            // db update
            control.C_state = state;
            control.C_onlineState = onlineState;
            Db.DbUpdate(TableType.CONTROLLER);
            Console.WriteLine($"버튼 상태. state = {state},{((ControllerState)state).ToString()}, onlinestate = {onlineState},{((ControllerOnlineState)onlineState).ToString()}");

            return true;
        }

        #endregion//확인완료
    }
    public class OperMakeJob
    {
        [DisplayName("출발지")]
        public string srcPort { get; set; }
        [DisplayName("도착지")]
        public string dstPort { get; set; }
        [DisplayName("케리어ID")]
        public string carrierId { get; set; }
    }
}
