using FSMPlus.Ref;
using FSMPlus.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FSMPlus.Vehicles
{
    public class ChargeStandby
    {
        #region 프로퍼티
        private Type_FS _parent;

        private Configuration _cfg = Configuration.Init;
        protected Configuration propertyCfg
        {   get { return _cfg; }
        }
        /// <summary>
        /// VehicleID. Charge & Standby 는 Vehicle 별로 되어야 한다
        /// </summary>
        private string _vehicleId;
        public string propertyVehicleId
        {   get { return _vehicleId; }
            set { _vehicleId = value; }
        }
        /// <summary>
        /// 스레드 종료 Flag
        /// </summary>
        private bool _stop;
        public bool propertyStop
        {   get { return _stop; }
            set { _stop = value; }
        }

        /// <summary>
        /// Vehicle's VecStatus메시지 저장. 상태변경여부 판단하기 위해 _oldVecStatus 에 백업저장
        /// </summary>
        private VecStatus _oldVecStatus = new VecStatus();
        private VecStatus _currVecStatus = new VecStatus();
        public VecStatus propertyCurrVecStatus
        {   get
            {   lock (this)
                {   return _currVecStatus;
                }
            }
            set
            {   lock (this)
                {   _oldVecStatus = _currVecStatus;
                    _currVecStatus = value;
                }
            }
        }

        /// <summary>
        /// Standby 특징적으로 체크시작시간. 상태 지속 체크를 위한 기준시점
        /// </summary>
        private DateTime _dtStartStandbyCheck;
        public DateTime propertyDtStartStandbyCheck
        {
            get { return _dtStartStandbyCheck; }
            set { _dtStartStandbyCheck = value; }
        }

        /// <summary>
        /// Charge 특징적으로 체크시작시간. 상태 지속 체크를 위한 기준시점
        /// </summary>
        private DateTime _dtStartChargeCheck;
        public DateTime propertyDtStartChargeCheck
        {
            get { return _dtStartChargeCheck; }
            set { _dtStartChargeCheck = value; }
        }

        /// <summary>
        /// 충전 중 인지 FLAG
        /// </summary>
        private bool _isStateCharge;
        public bool propertyIsStateCharge
        {
            get { return _isStateCharge; }
            set { _isStateCharge = value; }
        }

        /// <summary>
        /// Charge 와 관련하여 프로그램 저장한 상태와 Db 에 업데이트된 상태가 같은지 FLAG
        /// </summary>
        private bool _isStateChargeSyncronized;
        public bool PROPERTY_IsStateChargeSyncronized
        {
            get { return _isStateChargeSyncronized; }
            set { _isStateChargeSyncronized = value; }
        }

        /// <summary>
        /// 스탠바이 인지 FLAG
        /// </summary>
        private bool _isStateStandby;
        public bool PROPERTY_IsStateStandby
        {
            get { return _isStateStandby; }
            set { _isStateStandby = value; }
        }

        /// <summary>
        /// 스탠바이 상태가 db 와 같은지 flag - db update 실행을 위한 flag
        /// </summary>
        private bool _isStateStandbySyncronized;
        public bool PROPERTY_IsStateStandbySyncronized
        {
            get { return _isStateStandbySyncronized; }
            set { _isStateStandbySyncronized = value; }
        }
        #endregion

        public ChargeStandby(Type_FS vec)
        {
            _parent = vec;
            propertyVehicleId = vec.m_vecName;

            Thread t1 = new Thread(new ThreadStart(Run));
            t1.Start();
        }

        private void Run()
        {
            #region ChargeStandby 의 Thread 수행 가능여부 체크
            #endregion
            if (!CheckStartCondition())
                return;

            DateTime dtCur = DateTime.Now;

            string stsStr = string.Empty;              // ControlStatus 중복 메시지 방지용
            bool bErrChargeMinus = false;             // 메시지중복방지용 : "VecStatus's ChargeRatio is minus"
            bool bErrNotFoundStandbyGoal = false;     // 메시지중복방지용 : Not found standby goal
            while (!propertyStop)
            {
                Thread.Sleep(100);
                dtCur = DateTime.Now;

                /*
                 * AutoCharge, Standby 기능 실행 조건 확인
                 * */
                if (!CheckActMode(ref stsStr))
                {
                    Initialize();

                    bErrChargeMinus = false;
                    bErrNotFoundStandbyGoal = false;

                    Thread.Sleep(1000);    // 10 초 delay
                    continue;
                }

                if (propertyCfg.Data.UseAutoCharge)
                {
                    CheckCharge(dtCur, ref bErrChargeMinus);
                }

                /*
                 * AutoCharge 상태에서는 Standby 수행 Skip, CheckCharge 내부에서 설정
                 * */
                if (propertyIsStateCharge)
                    continue;

                if (propertyCfg.Data.UseStandby)
                {
                    CheckStandby(dtCur, ref bErrNotFoundStandbyGoal);
                }
            }
        }

        private void Initialize()
        {
            propertyDtStartChargeCheck = DateTime.MinValue;
            propertyDtStartStandbyCheck = DateTime.MinValue;
            propertyIsStateCharge = false;
            PROPERTY_IsStateChargeSyncronized = false;
            PROPERTY_IsStateStandby = false;
            PROPERTY_IsStateStandbySyncronized = false;
        }

        /// <summary>
        /// ChargeStandby 의 Thread 수행 가능여부 체크
        /// AutoCharge & MoveToStandby 체크 스레드의 수행 가능여부체크
        /// </summary>
        /// <returns>false - thread exit, true - thread continue</returns>
        private bool CheckStartCondition()
        {
            if (!(propertyCfg.Data.UseAutoCharge || propertyCfg.Data.UseStandby))
            {
                AlphaMessageForm.FormMessage form
                    = new AlphaMessageForm.FormMessage("", AlphaMessageForm.MsgType.Info, AlphaMessageForm.BtnType.Cancel);
                form.SetMsg(@"AutoCharge&&MovetoStandby function is disabled!!! Thread Exit!!!");
                form.ShowDialog();
                return false;
            }

            var control = Global.Init.Db.Controllers.SingleOrDefault();
            if (control == null)
            {
                AlphaMessageForm.FormMessage form
                    = new AlphaMessageForm.FormMessage("Controller", AlphaMessageForm.MsgType.Info, AlphaMessageForm.BtnType.Cancel);
                form.SetMsg(@"Controller Table's Record is Empty!!! Thread Exit!!");
                form.ShowDialog();
                return false;
            }

            if (propertyCfg.Data.UseAutoCharge)
            {
                if (Global.Init.Db.Charges.ToList().Count <= 0)
                {
                    AlphaMessageForm.FormMessage form
                        = new AlphaMessageForm.FormMessage("", AlphaMessageForm.MsgType.Info, AlphaMessageForm.BtnType.Cancel);
                    form.SetMsg(@"Charge Table's Record is Empty!!! Not Use AutoCharge Function!!!");
                    form.ShowDialog();
                    return true;
                }
            }

            if (propertyCfg.Data.UseStandby)
            {
                if (Global.Init.Db.Standby.ToList().Count <= 0)
                {
                    AlphaMessageForm.FormMessage form
                        = new AlphaMessageForm.FormMessage("", AlphaMessageForm.MsgType.Info, AlphaMessageForm.BtnType.Cancel);
                    form.SetMsg(@"Standby Table's Record is Empty!!! Not Use MoveToStandby Function!!!");
                    form.ShowDialog();
                    return true;
                }
            }

            return true;
        }

        /// <summary>
        /// AutoCharge, Standby 를 체크하기 위한 Controller 및 Vehicle 의 상태 체크
        /// </summary>
        /// <param name="sErrMsg"></param>
        /// <returns>bool : true - check start, false - check stop</returns>
        private bool CheckActMode(ref string stsStr)
        {
            var control = Global.Init.Db.Controllers.SingleOrDefault();
            if (control == null)
            {
                AlphaMessageForm.FormMessage form
                    = new AlphaMessageForm.FormMessage("Controller", AlphaMessageForm.MsgType.Info, AlphaMessageForm.BtnType.Cancel);
                form.SetMsg($"ChangeTscStatus, Controller is null");
                form.ShowDialog();
                return false;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("ControlStatus is current status : ");
            sb.AppendFormat($"ControllerOnlineState.{((ControllerOnlineState)control.C_onlineState).ToString()}");
            sb.AppendFormat($", ControllerState.{((ControllerState)control.C_state).ToString()}");
            if (propertyCurrVecStatus != null)
                sb.AppendFormat($", VehicleMode.{propertyCurrVecStatus.mode.ToString()}");
            string s = sb.ToString();

            if (propertyCurrVecStatus == null)
                return false;

            if ((ControllerOnlineState.ONLINE != (ControllerOnlineState)control.C_onlineState)
                || (ControllerState.AUTO != (ControllerState)control.C_state)
                || (VehicleMode.AUTO != propertyCurrVecStatus.mode))
            //||(VehicleState.NOT_ASSIGN != Prob_CurrVecStatus.state))
            /*
             * Charge 의 상태 체크를 위해서 NOT_ASSING 은 추가적으로 개별 함수에서 체크
             * */
            {
                if (s == stsStr)
                    return false;

                stsStr = s;
                Logger.Inst.Write(CmdLogType.All, $"{s}");
                return false;
            }

            if (s == stsStr)
                return true;

            stsStr = s;
            Logger.Inst.Write(CmdLogType.All, $"{s}");
            return true;
        }

        /// <summary>
        /// AutoCharge 의 Dock, UnDock 에 대한 조건처리
        /// </summary>
        /// <param name="dt">현재시간</param>
        /// <param name="bErrChargeMinus">충전값 minus 값 오류메시지 출력 방지용</param>
        /// <returns>void</returns>
        /// <remarks>연속된 제어 발생을 방지하기 위해 DOCK,UNDOCK 제어 후 수행시 Prob_DtStartChargeCheck = DateTime.MinValue 초기화</remarks>
        /// <remarks>cfg 에서 가져오는 시간 연산 설정치는 모두 sec 단위</remarks>
        /// <remarks>충전상태의 상태 변이를 체크하기 위한 최소 간격은 1초</remarks>
        /* 
         * 기본적으로 Dock, UnDock 을 체크하기 위해 10초 지연을 가진다.
         * 이 말은 최초 시행시 10초 후 Dock, UnDock 을 제어조건을 체크한다. 
         * Dock, UnDock 제어 후 _dtStartChargeCheck = DateTime.MinValue 초기화 하는 과정을 통해 
         * 다음 Dock, UnDock 제어를 위한 체크를 10초 지연시킨다.
         * 지연된 체크에 Dock, UnDock 조건이 만족되지 않아 제어가 발생되지 않을 때 
         * 1초 지연을 발생시키고 다시 Dock, UnDock 을 제어조건을 체크한다. 
         * */
        private void CheckCharge(DateTime dt, ref bool bErrChargeMinus)
        {
            /*
             * AutoCharge 가 수행되어야 할 미래 시간을 생성
             * Prob_DtStartChargeCheck 가 초기값 할당이 없으면 현재시간 + _cfg.Data.IdleDuration 값
             * */
            if (propertyDtStartChargeCheck == DateTime.MinValue)
            {
                if (VehicleState.NOT_ASSIGN != propertyCurrVecStatus.state)
                    return;

                propertyDtStartChargeCheck = dt;

                int delay = (propertyCfg.Data.IdleDuration < 3) ? 3 : propertyCfg.Data.IdleDuration;
                System.TimeSpan tsp = new System.TimeSpan(0, 0, 0, delay);
                propertyDtStartChargeCheck = propertyDtStartChargeCheck.Add(tsp);

                Logger.Inst.Write(propertyVehicleId, CmdLogType.All, "AutoCharge Check Init!!!");
                return;
            }

            /*
             * Charge 가능한 상태가 지속됨을 체크. 미래시간에서 현재시간 뺄셈 연산을 통해 확인
             * */
            TimeSpan tDiff = propertyDtStartChargeCheck - dt;
            if (tDiff.TotalMilliseconds > 0)
            {
                return;
            }

            if (propertyCurrVecStatus.charge > 0)
            {
                bErrChargeMinus = false;        /*bErrChargeMinus 상태반전*/

                #region Prob_IsStateCharge 상태에서 M+ 은 Job을 할당하지 않음을 보증해라
                // Prob_IsStateCharge 상태를 설정된 상태에서 M+ 은 Job 을 할당하지 않는다
                // 그 동안 Job 상태가 Charging 으로 상태가 올라오는지 보고 flag 을 반전한다
                #endregion
                #region propertyIsStateCharge 가 true 면 충전상태다 로컬 셋팅된 상태, 이미 "Charge;nearDockGoal" 명령어는 전송했고 실행여부는 확인 안된 상태
                #endregion
                if (propertyIsStateCharge)
                {
                    #region PROPERTY_IsStateChargeSyncronized 가 false 는 아직 동기화에 대한 체크가 되지 않은 상태로 이제 체크하러 간다
                    #endregion

                    if (!PROPERTY_IsStateChargeSyncronized)
                    {
                        #region PROPERTY_IsStateChargeSyncronized_Sequence
                        /*
                        0. M /R에서 수신된 상태가 CHARGING 이면 propertyIsStateCharge 가 true 로 설정된 현재 상태와 동일하다.
                        1. MPLUS : propertyIsStateCharge - true, M/RemoteCommandType : CHARGING 상태가 동일하다 확인되었다
                        2. CHARGE DB 에 VehicleID 를 기록해서 ChargeDock 점유를 알리자
                        3. PROPERTY_IsStateChargeSyncronized 플래그를 true 로 설정하여 동기화 상태 표시한다
                        4. 다음 체크를 위해 propertyDtStartChargeCheck 는 DateTime.MinValue 초기값으로 설정한다
                        */
                        #endregion // PROPERTY_IsStateChargeSyncronized_Sequence

                        if (VehicleState.CHARGING == propertyCurrVecStatus.state)
                        {
                            #region 양쪽 상태 일치
                            /* 
                             * TODO : Prob_VehicleId 에 해당하는 Charge 레코드를 찾아 db update 한다
                             * TODO : 충전기 여러개 DOCK 이라는 명령 날리면 어떻게 찾아가나?
                             * */
                            #endregion  // 양쪽 상태 일치

                            Logger.Inst.Write(propertyVehicleId, CmdLogType.All, @"AutoCharge ""DOCK"" Status Match!!! - Db' Charge table Status Update");
                            Global.Init.Db.Charges[0].VEHICLEID = propertyVehicleId;
                            propertyDtStartChargeCheck = DateTime.MinValue;
                            PROPERTY_IsStateChargeSyncronized = true;
                        }
                        else
                        {
                            Logger.Inst.Write(propertyVehicleId, CmdLogType.All, @"AutoCharge ""DOCK"" Status Un-Match!!! - Retry ""DOCK"" Command Sending Step Move!!!");
                            propertyIsStateCharge = false;
                            return;
                        }
                    }
                    else
                    {
                        if (VehicleState.PARKED/*JOB을 할당 받으면 PARKED 상태로 변경*/ == propertyCurrVecStatus.state
                           || VehicleState.ENROUTE/*이동중*/ == propertyCurrVecStatus.state
                           || VehicleState.ACQUIRING/*자재로딩중*/ == propertyCurrVecStatus.state
                           || VehicleState.DEPOSITING/*자재언로딩중*/ == propertyCurrVecStatus.state
                           || propertyCfg.Data.ChargeEnd <= propertyCurrVecStatus.charge)
                        {
                            ChargeCommand("CHARGE");
                            //_parent.SendMessageToVehicle($"UNDOCK;");
                            //Logger.Inst.Write(propertyVehicleId, CmdLogType.All, @"AutoCharge ""UNDOCK"" Command Send!!! - Status Initialize");

                            //propertyIsStateCharge = false;
                            //propertyDtStartChargeCheck = DateTime.MinValue;
                            return;
                        }
                    }
                }
                else
                {
                    if (PROPERTY_IsStateChargeSyncronized)
                    {
                        if (VehicleState.CHARGING == propertyCurrVecStatus.state)
                        {
                            Logger.Inst.Write(propertyVehicleId, CmdLogType.All, @"AutoCharge ""UNDOCK"" Status Un-Match!!! - Retry ""UNDOCK"" Command Sending Step Move!!!");
                            propertyIsStateCharge = true;
                            return;
                        }
                        else
                        {
                            Logger.Inst.Write(propertyVehicleId, CmdLogType.All, @"AutoCharge ""UNDOCK"" Status Match!!! - Status Initialize");
                            Global.Init.Db.Charges[0].VEHICLEID = null;
                            PROPERTY_IsStateChargeSyncronized = false;
                            propertyDtStartChargeCheck = DateTime.MinValue;
                            return;
                        }
                    }
                    else
                    {
                        PROPERTY_IsStateChargeSyncronized = false;
                        if (propertyCfg.Data.ChargeStart >= propertyCurrVecStatus.charge && VehicleState.NOT_ASSIGN == propertyCurrVecStatus.state)
                        {
                            ChargeCommand("DISCHARGE");
                            return;
                        }
                    }
                }
            }
            else
            {
                if (!bErrChargeMinus)
                    Logger.Inst.Write(CmdLogType.Comm, @"AutoCharge VecStatus's ChargeRatio is minus");
                bErrChargeMinus = true;         /*bErrChargeMinus Set*/
            }

            // 제어가 없는 상태, 다음 상태 변경을 체크할 미래 시간을 1초 뒤로 지연
            {
                System.TimeSpan tsp = new System.TimeSpan(0, 0, 0, 1);
                propertyDtStartChargeCheck = propertyDtStartChargeCheck.Add(tsp);
            }
        }

        private void ChargeCommand(string v)
        {
            if (v.Equals("DISCHARGE"))
            {
                _parent.SendMessageToVehicle("CHARGE;");
                Logger.Inst.Write(propertyVehicleId, CmdLogType.All, @"AutoCharge ""CHARGE;"" Command Send(UNDOCK)!!! - Status Initialize");
                propertyIsStateCharge = true;
                propertyDtStartChargeCheck = DateTime.MinValue;
                return;
            }
            if (v.Equals("CHARGE"))
            {
                string nearChargeDock = FindChargeGoal();
                if (!string.IsNullOrEmpty(nearChargeDock))
                {

                }

            }
        }

        private void CheckStandby(DateTime dt, ref bool bErrNotFoundStandbyGoal)
        {
            /*
             * MoveToStandby 가 수행되어야 할 미래 시간을 생성
             * Prob_DtStartStandbyCheck 가 초기값 할당이 없으면 현재시간 + _cfg.Data.IdleDuration 값
             * */
            if (propertyDtStartStandbyCheck == DateTime.MinValue)
            {
                if (VehicleState.NOT_ASSIGN != propertyCurrVecStatus.state)
                    return;

                propertyDtStartStandbyCheck = dt;

                int delay = (propertyCfg.Data.IdleDuration < 3) ? 3 : propertyCfg.Data.IdleDuration;
                System.TimeSpan tsp = new System.TimeSpan(0, 0, 0, delay);
                propertyDtStartStandbyCheck = propertyDtStartStandbyCheck.Add(tsp);

                Logger.Inst.Write(propertyVehicleId, CmdLogType.All, "Standby Check Init!!!");
                return;
            }

            /*
             * Charge 가능한 상태가 지속됨을 체크. 미래시간에서 현재시간 뺄셈 연산을 통해 확인
             * */
            TimeSpan tDiff = propertyDtStartStandbyCheck - dt;
            if (tDiff.TotalMilliseconds > 0)
            {
                return;
            }

            if (VehicleState.NOT_ASSIGN == propertyCurrVecStatus.state)
            {
                var nearStandby = FindStandbyGoal();
                if (!string.IsNullOrEmpty(nearStandby))
                {
                    bErrNotFoundStandbyGoal = false;
                    Logger.Inst.Write(propertyVehicleId, CmdLogType.All, "Standby Run");

                    _parent.SendMessageToVehicle($"STANDBY;{nearStandby};");
                    SetStandbyGoal(nearStandby, propertyVehicleId);

                    PROPERTY_IsStateStandby = true;
                }
                else
                {
                    if (!bErrNotFoundStandbyGoal)
                        Logger.Inst.Write(propertyVehicleId, CmdLogType.All, "Standby Goal을 찾을 수 없습니다.");
                    bErrNotFoundStandbyGoal = true;
                }
            }
            else
            {
                if (_oldVecStatus != null && _oldVecStatus.state != propertyCurrVecStatus.state)
                {
                    _parent.SendMessageToVehicle($"STANDBY;;");
                    SetStandbyGoal(string.Empty, propertyVehicleId);

                    propertyDtStartStandbyCheck = DateTime.MinValue;
                    PROPERTY_IsStateStandby = false;
                    return;
                }
            }

            // 상태 변경을 Retry 할 미래 시간을 _cfg.Data.ReChkPeriodChargeStatus 초 후로 설정(1보다 작을 때 1초로 설정)
            {
                System.TimeSpan tsp = new System.TimeSpan(0, 0, 0, 1);
                propertyDtStartStandbyCheck = propertyDtStartStandbyCheck.Add(tsp);
            }
        }

        private string FindChargeGoal()
        {

            return string.Empty;
        }

        public string FindStandbyGoal()
        {
            var emptyList = Global.Init.Db.Standby.Where(p => string.IsNullOrEmpty(p.VEHICLEID)).ToList();
            float goalDist = float.MaxValue;
            string goalName = string.Empty;
            foreach (var item in emptyList)
            {
                // 각각 전체(All), 단 하나라도 존재(Any), 정확한 값 존재(Contains)를 의미합니다.
                if (Global.Init.Db.Zones.Where(p => p.UNITID == item.GOALNAME && p.VEHICLEID == propertyVehicleId).Any())
                {
                    var dist = GetDistanceBetweenPoints(new PointF(propertyCurrVecStatus.posX, propertyCurrVecStatus.posY), new PointF(item.loc_x, item.loc_y));
                    if (dist < goalDist)
                    {
                        goalDist = dist;
                        goalName = item.GOALNAME;
                    }
                }
            }
            return goalName;
        }

        // 파타고라스 정리에 의한 두 점간의 거리
        private float GetDistanceBetweenPoints(PointF a, PointF b)
        {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        /// <summary>
        /// 대기위치 골에 대한 정보 변경 메소드
        /// </summary>
        /// <param name="goalName"></param>
        /// <param name="vecId"></param>
        protected void SetStandbyGoal(string goalName, string vecId)
        {
            var goal = Global.Init.Db.Standby.Where(p => p.GOALNAME == goalName).FirstOrDefault();
            if (goal != null)
            {
                goal.VEHICLEID = vecId;
            }
            else
            {
                var secondGoal = Global.Init.Db.Standby.Where(p => p.VEHICLEID == vecId).FirstOrDefault();
                if (secondGoal != null)
                {
                    secondGoal.VEHICLEID = null;
                }
            }
        }
    }
}
