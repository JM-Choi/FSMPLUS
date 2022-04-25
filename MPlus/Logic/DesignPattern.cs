using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using FSMPlus.Vehicles;
using FSMPlus.Ref;
using TIBCO.Rendezvous;
using tech_library.Tools;
using System.Text.RegularExpressions;
using System.Reflection;

namespace FSMPlus.Logic
{
    /// <summary>
    /// VSP Pattern은 Facade Pattern으로 작성
    /// Facade Pattern은 하나의 작업을 처리할때 여러개의 클래스의 상호 동작이 필요할때 묶어서 처리하는 Pattern
    /// 외부에서는 간단한 인터페이스로 노출됨
    /// </summary>
    #region VSP Pattern

    // 실제 외부에서 사용하는 부분은 start_chk Class
    public class start_chk
    {        
        Vehicle_chk vec_chk = new Vehicle_chk();
        ExecuteTime_chk ET_chk = new ExecuteTime_chk();

        // FS 관련 Job만을 Selete 하기 위한 함수
        // Tray/Stack DB와 같은 DB를 사용하기때문에 분별이 필요
        // 현재 FS만 있으나 다른 Type의 Vehicle이 추가 될 경우 외부에서 전달하는 Trans에 해당 TransferType으로 전달
        public void JobtypeSel(ref List<pepschedule> FSJobs, string trans)
        {
            FSJobs = FSJobs.Where(p => p.TRANSFERTYPE == trans && p.C_isChecked != 1).OrderBy(p => p.REAL_TIME).ThenBy(p => p.BATCHID).ToList();
        }

        // Vehicle, Job 시작시간을 확인 후 시작할 Job을 선정하는 함수
        // Vehicle, Job 시작시간이 true 일 경우 해당 시작 시간에 해당하는 JobList Selete
        // 현재 FS만 있으나 다른 Type의 Vehicle이 추가 될 경우 else 부분에 추가
        public bool isstart_chk(List<vehicle> vec, string exetime, ref List<pepschedule> FSJobs)
        {
            // 사용 가능한 Vehicle 확인
            if (!ET_chk.isEnoughExeTime(exetime))
            {
                return false;
            }
            // Job이 시작해야하는 시간이 현재 시간에 도달했는지 확인
            else if (!vec_chk.isEnoughVehicle(vec))
            {
                return false;
            }
            // 해당 시작 시간에 해당하는 JobList Selete
            else
            {
                Job_Selete job_sel = new FS_Job();
                List<transtype> trans = new List<transtype>();
                trans.Add(job_sel.seleteJob(FSJobs, exetime));

                // Magazine Type에 상관없이 Multi Job 생성시 사용
                //FSJobs = trans[0].searchJob(exetime, FSJobs[0].WORKTYPE);

                // 하나의 Parents Job만 사용
                FSJobs = trans[0].searchJob(exetime, FSJobs[0].C_mgtype);
                return true;
            }
        }

    }

    // 사용 가능한 Vehicle 확인
    // 사용 가능한 Vehicle이 없으면 false
    // 사용 가능한 Vehicle이 있으면 true
    class Vehicle_chk
    {
        public bool isEnoughVehicle(List<vehicle> vec)
        {
            if (vec != null && vec.Count() > 0)
                return true;
            else
                return false;
        }
    }

    // Job이 시작해야하는 시간이 현재 시간에 도달했는지 확인
    // exeTime > ut : 시작 시간이 현재 시간에 도달하지 못 함 return false;
    // exeTime <= ut : 시작 시간이 현재 시간에 도달 함 return true;
    class ExecuteTime_chk
    {
        public bool isEnoughExeTime(string exetime)
        {
            if (exetime == null || exetime == "")
                return false;

            int ut = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            if (Convert.ToInt32(exetime) < ut)
                return true;
            else
                return false;
        }
    }

    // JobList와 Job 시작시간에 대한 초기 값 저장
    public abstract class Job_Selete
    {
        public abstract transtype seleteJob(List<pepschedule> pep, string scheduleTime);
    }

    // Type에 따른 JobList와 Job 시작시간에 대한 초기 값 저장
    // Type이 추가될 경우 Class 이름(FS_Job)이 다른 Class를 만들어주면 적용
    public class FS_Job : Job_Selete
    {
        public override transtype seleteJob(List<pepschedule> pep, string scheduleTime)
        {
            return new FS(pep, scheduleTime);
        }
    }

    // JobList와 Job 시작시간에 대한 초기 값 저장
    public abstract class transtype
    {
        public List<pepschedule> m_pep;
        public string m_scheduleTime;

        // MultiJob시 사용
        //public abstract List<pepschedule> searchJob(string exetime, string worktype);
        public abstract List<pepschedule> searchJob(string exetime, string mgtype);
    }

    // start_chk의 isstart_chk의 해당 시작 시간에 해당하는 JobList Selete를 실행하는 Class
    public class FS : transtype
    {
        public FS(List<pepschedule> pep, string scheduledTime)
        {
            this.m_pep = pep;
            this.m_scheduleTime = scheduledTime;
        }

        // MultiJob시 사용
        //public override List<pepschedule> searchJob(string exetime, string worktype)
        //{
        //    Global.Init.Db.Update_PepsPriority(exetime);    // priority 가 할당되지 않은 항목에 대해 update
        //    // Parents Job만 Selete
        //    // Magazine Type에 상관없이 Multi Job 생성 시 사용
        //    List<pepschedule> factor_job = this.m_pep.Where(p => p.REAL_TIME == exetime && p.WORKTYPE == worktype && p.C_isChecked != 1 && p.BATCHID.Contains("PFS")).OrderBy(p => p.C_priority)
        //                                                    .ThenBy(p => p.C_mgtype).ToList();

        //    // Magazine Type이 같은 Job으로만 Multi Job 생성 시 사용
        //    //List<pepschedule> factor_job = this.m_pep.Where(p => p.REAL_TIME == exetime && p.WORKTYPE == worktype && p.C_isChecked != 1 && p.BATCHID.Contains("PFS") && p.C_mgtype == mgtype)
        //    //                                                .OrderBy(p => p.C_mgtype).ToList();
        //    string over_batchid = string.Empty;
        //    string[] over_batchids = null;
        //    int traycount = 0;
        //    int basecount = 12;

        //    // Selete된 Parents Job의 TrayID의 합이 최대수량을 넘어가면 Job List에서 제외
        //    foreach (var x in factor_job)
        //    {
        //        // UFS 최대 수량은 12 그 외 FS의 최대 수량은 6
        //        // 12를 기준으로 잡고 UFS 외의 FS 일때는 Magazine의 수에 * 2를 하여 계산
        //        traycount += (x.C_mgtype != "UFS") ? (x.TRAYID.Split(',').Count() * 2) : (x.TRAYID.Split(',').Count());
        //        //traycount += (x.TRAYID.Split(',').Count() * 2);

        //        if (traycount > basecount)
        //        {
        //            if (over_batchid.Length > 0)
        //                over_batchid += ",";

        //            over_batchid += x.BATCHID;
        //        }
        //    }
        //    over_batchids = over_batchid.Split(',');

        //    if (over_batchid != "")
        //    {
        //        //factor_job.Clear();
        //        //factor_job = this.m_pep;
        //        foreach (var k in over_batchids)
        //        {
        //            factor_job = factor_job.Where(p => p.REAL_TIME == exetime && p.WORKTYPE == worktype && p.C_isChecked != 1 && !p.BATCHID.Contains(k)).ToList();
        //        }
        //    }

        //    List<pepschedule> factor_job2 = new List<pepschedule>();

        //    foreach (var x in factor_job)
        //    {
        //        List<pepschedule> count = this.m_pep.Where(p => p.REAL_TIME == exetime && p.WORKTYPE == worktype && p.C_isChecked != 1 && p.BATCHID.Contains(x.BATCHID.Split('_')[1])).ToList();
        //        for (int i = 0; i < count.Count(); i++)
        //        {
        //            factor_job2.Add(count[i]);
        //        }
        //    }
        //    return factor_job2;
        //}

        public override List<pepschedule> searchJob(string exetime, string mgtype)
        {
            Global.Init.Db.Update_PepsPriority(exetime);    // priority 가 할당되지 않은 항목에 대해 update

            // Worktype 이 FSI, FSO 일 경우 Magazine 최대 수량에 맞추어 Job Selete

            // 하나의 Parents Job만 사용
            pepschedule factor_job = this.m_pep.Where(p => p.REAL_TIME == exetime && p.C_isChecked != 1 && !(p.BATCHID.Contains("CFS")))
                                                            .OrderBy(p => p.C_priority).ThenBy(p => p.BATCHID).First();

            List<pepschedule> factor_job2 = new List<pepschedule>();

            List<pepschedule> count = this.m_pep.Where(p => p.REAL_TIME == exetime && p.WORKTYPE == factor_job.WORKTYPE && p.C_isChecked != 1 && p.BATCHID.Contains(factor_job.BATCHID.Split('_')[1]))
                                                       .ToList();
            for (int i = 0; i < count.Count(); i++)
            {
                factor_job2.Add(count[i]);
            }

            return factor_job2;

        }
    }
    #endregion

    /// <summary>
    /// Vehicle Pattern은 Facade + Factory Pattern으로 작성
    /// Facade Pattern은 하나의 작업을 처리할때 여러개의 클래스의 상호 동작이 필요할때 묶어서 처리하는 Pattern
    /// 외부에서는 간단한 인터페이스로 노출됨
    /// 실제 외부에서 사용하는 부분은 start_chk Class
    /// Factory Pattern은 Class의 인스턴스를 만드는 일을 Sub Class에서 실행하는 Pattern
    /// Main Class가 여러개이나 실행되는 인스턴스가 같을때 사용
    /// </summary>
    #region Vehicle Pattern
    // 실제 외부에서 사용하는 Robot Class
    public abstract class Robot
    {
        // Vehicle의 Recv를 받는 Event
        public abstract event EventHandler<RecvMsgArgs> OnRecvMsg;
        // Vehicle과의 Socket 연결 상태를 기준으로 Vehicle의 상태를 변경하는 Event
        public abstract event EventHandler<ChangeConnectedArgs> OnChageConnected;

        // 현재 Class를 사용하는 Vehicle 호기
        public string m_vecName;
        // Vehicle과 연결하기 위한 Data
        public string m_ip;
        public int m_port;
        public string m_remoteip;
        public Nullable<int> m_remoteport;

        public bool isStop;
        // Vehicle의 연결 상태
        public bool IsConnected = false;

        // Vehicle의 상태 값
        public ControllerState controllerState = ControllerState.INIT;

        // Vehicle 자동충전 기능을 위한 변수
        public ChargeStandby _csby;
        public ChargeStandby Csby
        {
            get { return _csby; }
            set { _csby = value; }
        }

        // Job 완료 후 충전소로 이동하기 위한 변수
        private bool _jobassign;
        public bool JobAssign
        {
            get { return _jobassign; }
            set { _jobassign = value; }
        }

        // Vehicle에 GO 명령 후 Recv를 기다리는 변수
        bool _vecResponse = false;
        public bool vecResponse
        {
            get { return _vecResponse; }
            set { _vecResponse = value; }
        }
        // Vehicle에 GO 명령 후 Recv를 기다리는 변수
        bool _vecmrbpstart = false;
        public bool vecMRBPStart
        {
            get { return _vecmrbpstart; }
            set { _vecmrbpstart = value; }
        }

        // Vehicle에 Assign 명령 후 Assign 완료 Recv를 확인하는 변수
        bool _jobResponse = false;
        public bool jobResponse
        {
            get { return _jobResponse; }
            set { _jobResponse = value; }
        }

        public bool goReSend = false;
        public bool goRetry_Fail = false;
        public int goRetry_count = 0;

        // Vehicle과 Socket 연결 함수
        public abstract void SocketInit();
        // Vehicle에 대한 모든 행동 중지
        public abstract void Dispose();
        // Vehicle과 Socket 재연결 함수
        public abstract void Reconnected();
        // Vehicle에 실행할 Job Assign 명령 전달 함수        
        public abstract void Assign(List<pepschedule> peps);
        // Vehicle에 현재 작업을 진행할 설비로 이동하는 GO 명령 전달 함수
        public abstract void GO(string BatchID);
        // Vehicle과 설비가 준비가 되었음을 확인하고 실제 Unload/Load 작업을 시작하는 JobStart 명령 전달 함수
        public abstract void JobStart();
        // FS 단위 반송 시 MAGAZINE이 여러개이면 MAGAZINE 교체 후 Unload/Load 작업을 시작하게하는 FS_MOVE_START 명령 전달 함수
        public abstract void FS_MOVE_START();
        // Vehicle과 설비가 준비가 되었음을 확인하고 실제 Unload/Load 작업을 시작하는 JobStart 명령 전달 함수
        public abstract void FSSTK_Start();
        // Vehicle에 Pause 명령 전달 함수
        public abstract void PAUSE();
        // Vehicle를 Auto Mode로 변경 시키기 위한 Resume 명령 전달 함수
        public abstract void RESUME();
        // Vehicle에 Job 취소를 하기위한 Cancel 명령 전달 함수
        public abstract void CANCEL();
        // Vehicle에 현재 가지고 있는 Job을 재 실행하기 위한 Job_Resume 명령 전달 함수
        public abstract void JOB_RESUME();
        // Vehicle에 LD 행동 정지를 하기 위한 Stop 명령 전달 함수
        public abstract void STOP();
        // Vehicle을 충전소로 보내기 위한 Charge 명령 전달 함수
        public abstract void CHARGE();
        // Vehicle에 해당 변수(_msg)를 보내는 함수
        public abstract void SEND_MSG(string _msg);
        public abstract Task<int> CALCDISTANCE_BTW(string goalName1, string goalName2);

        public abstract void goReset();
    }

    // Robot Class를 실제로 구동하는 Class
    public class Type_FS : Robot
    {
        // Vehicle의 Recv를 받는 Event
        public override event EventHandler<RecvMsgArgs> OnRecvMsg;
        // Vehicle과의 Socket 연결 상태를 기준으로 Vehicle의 상태를 변경하는 Event
        public override event EventHandler<ChangeConnectedArgs> OnChageConnected;
        // Vehicle과 Socket 연결이 끊겼을 때 재연결을 시도하는 Timer
        private Timer tmrConnect = new Timer();
        // Vehicle과 Socket 연결은 되어있으나 Recv Message가 30초동안 없을 시 Socket 연결을 끊는 Timer
        private Timer tmrState = new Timer();
        
        // Vehicle과의 Socket 연결을 위한 변수
        private AsyncClintSock sock;
        // Vehicle의 Recv Data를 저장할 버퍼
        string _MsgBuf = "";
        // 메시지 끝을 찾을 구분자
        private readonly char _Token = '\n';
        // 그 다음 구분자
        private readonly char _SubToken = '\r'; 
        
        // Vehicle의 Status Message의 상태를 저장
        private VehicleMode _curVecMode;
        public VehicleMode CurVecMode
        {
            get { return _curVecMode; }
            set { _curVecMode = value; }
        }

        // 기본 값 저장
        public Type_FS(string _vecName, string _ip, int _port, string _remoteip, Nullable<int> _remoteport)
        {
            this.m_vecName = _vecName;
            this.m_ip = _ip;
            this.m_port = _port;
            this.m_remoteip = _remoteip;
            this.m_remoteport = _remoteport;

            this._csby = new ChargeStandby(this);
        }

        // Vehicle과 Socket 연결 및 Timer 실행
        public override void SocketInit()
        {
            sock = new AsyncClintSock(true);
            sock.OnRcvMsg += Sock_OnRcvMsg;
            sock.OnChangeConnected += Sock_OnChangeConnected;


            tmrConnect.Interval = 5000;
            tmrConnect.Elapsed += tmrConnect_Elapsed;
            tmrConnect.Enabled = true;
           
            //tmrState.Interval = 1000;
            //tmrState.Elapsed += tmrState_Elapsed;
            //tmrState.Enabled = true;
        }
        // Vehicle의 Recv Data 처리하는 함수
        private void Sock_OnRcvMsg(object sender, string rcvStr)
        {
            _MsgBuf += rcvStr;
            // Robot과 연결확인 Flag 초기화
            sock.Connected_chk = false;
            while (_MsgBuf.IndexOf(_Token) != -1)
            {
                // '\n'으로 Split
                var split = _MsgBuf.Split(_Token);
                foreach (var item in split)
                {
                    try
                    {
                        if (item.IndexOf(_SubToken) != -1)
                        {
                            //'\r'을 삭제
                            string tempStr = item.Remove(item.Length - 1);
                            // Message 처리 함수
                            MsgPars(tempStr);
                        }
                    }
                    catch
                    {
                        Logger.Inst.Write(CmdLogType.Comm, $"차량으로부터 받은 메세지를 분석 할 수 없습니다. [{item}]");
                        continue;
                    }
                }
                // Message 처리 후 Message Buf에서 삭제
                _MsgBuf = _MsgBuf.Remove(0, _MsgBuf.LastIndexOf(_Token) + 1);
            }
            // Robot과 연결확인 Flag 설정
            sock.Connected_chk = true;
        }

        // Vehicle의 Recv Data Parsing
        private async void MsgPars(string msg)
        {
            await Task.Delay(1);
            Logger.Inst.Write(this.m_vecName, CmdLogType.Comm, $"R = [{msg}]");
            RecvMsgArgs sendArg = new RecvMsgArgs() { recvMsg = msg };
            // Robot에서 Message가 왔는지 확인하는 Flag
            tmrState.Enabled = true;
            State_wait_count = 0;

            // Message Head를 Cmd4Vehicle로 변경하는 함수
            sendArg.Cmd = GetVehicleCmd(msg);
            switch (sendArg.Cmd)
            {
                // Message Head가 Status 일 때
                case Cmd4Vehicle.STATUS:
                    VecStatus CurrStatus = VecStatus.Parse(sendArg.recvMsg);
                    // Message 처리에 필요한 Data 가공
                    sendArg.Status = VecStatus.Parse(sendArg.recvMsg);
                    sendArg.reportEvent = VehicleEvent.ReportStatus;
                    {
                        // Auto Charge 기능에 사용되는 변수 저장
                        Csby.propertyCurrVecStatus = new VecStatus(CurrStatus);
                        // Robot에 Message 전송 시 상태 확인을 위한 변수
                        CurVecMode = CurrStatus.mode;
                    }
                    break;
                // Message Head가 Error 일 때
                case Cmd4Vehicle.ERROR:
                    // Message 처리에 필요한 Data 가공
                    sendArg.ErrState = VecErrStatus.Parse(sendArg.recvMsg);
                    break;
                // Message Head가 Job 일 때
                case Cmd4Vehicle.JOB:
                    // Message 처리에 필요한 Data 가공
                    sendArg.JobState = VecJobStatus.Parse(sendArg.recvMsg);
                    break;
                // Message Head가 GOAL_LD 또는 GOAL_UL 일 때
                case Cmd4Vehicle.GOAL_LD:
                case Cmd4Vehicle.GOAL_UL:
                case Cmd4Vehicle.MGTYPE_DATA:
                    // Message 처리에 필요한 Data 가공
                    sendArg.Goal = VecGoal.Parse(sendArg.recvMsg);
                    sendArg.reportEvent = VehicleEvent.RequestGoalList;
                    break;
                // Message Head가 SCAN 일 때
                case Cmd4Vehicle.SCAN:
                    // Message 처리에 필요한 Data 가공
                    sendArg.Scan = VecScanArgs.Parse(sendArg.recvMsg);
                    break;
                // Message Head가 RESP 일 때
                case Cmd4Vehicle.RESP:                    
                    Logger.Inst.Write(this.m_vecName, CmdLogType.Comm, $"메세지 응답 수신 [{msg}]");
                    break;
                // Message Head가 GOAL_FAIL 일 때
                case Cmd4Vehicle.GOAL_FAIL:
                    Logger.Inst.Write(this.m_vecName, CmdLogType.Comm, $"[{msg}]");
                    break;
                // Message Head가 위 Case에 없을 때
                default:
                    Logger.Inst.Write(this.m_vecName, CmdLogType.Comm, $"잘못된 명령을 수신 [{msg}]");
                    return;
            }

            // 가공된 Data를 사용하여 내부 처리하는 함수
            OnRecvMsg?.Invoke(this, sendArg);
        }

        // Vehicle의 Recv Data 중 Message Header를 Cmd4Vehicle로 변경하는  함수
        private Cmd4Vehicle GetVehicleCmd(string msg)
        {
            Cmd4Vehicle rtn = Cmd4Vehicle.None;
            char tok = ';';
            var split = msg.Split(tok);

            try
            {
                rtn = (Cmd4Vehicle)Enum.Parse(typeof(Cmd4Vehicle), split[0]);
            }
            catch (Exception)
            {
                Logger.Inst.Write(CmdLogType.Comm, $"String to Enum(Cmd4Vehicle) 변환에 실패하였습니다.{split[0]}");
            }
            return rtn;
        }
        // Vehicle과 Socket 연결 상태 변경
        private void Sock_OnChangeConnected(object sender, ChangeConnectedArgs e)
        {
            if (e.connected)
            {
                IsConnected = true;
                Logger.Inst.Write(this.m_vecName, CmdLogType.Comm, $"{this.m_ip} : O (Client 접속 성공 이벤트)");
                StillConnected();
            }
            else
            {
                IsConnected = false;
                Logger.Inst.Write(this.m_vecName, CmdLogType.Comm, $"{this.m_ip} : X (Client 접속 종료 이벤트)");
            }
        }
        // 1초의 delay 를 주고 "CONNECT" 메시지를 Vehicle에 전송한다.
        private async void StillConnected()
        {
            int connectDelay = 0;
            while (sock.Connected)
            {
                await Task.Delay(100);
                connectDelay++;

                if (connectDelay >= 10)
                {
                    SendMessageToVehicle("CONNECT");
                    break;
                }
            }
        }
        // Vehicle과 Socket 연결 모니터링
        private void tmrConnect_Elapsed(object sender, ElapsedEventArgs e)
        {
            tmrConnect.Enabled = false;
            if (IsConnected)
            {
                if (sock.Connected)
                {
                    OnChageConnected?.Invoke(this, new ChangeConnectedArgs() { connected = true });
                    Debug.WriteLine("connected");
                }
                else
                {
                    sock.StopClient();
                    IsConnected = false;
                    OnChageConnected?.Invoke(this, new ChangeConnectedArgs() { connected = false });
                    Debug.WriteLine("disconnected");
                }
            }
            else
            {
                if (!sock.Connected)
                {
                    OnChageConnected?.Invoke(this, new ChangeConnectedArgs() { connected = false });
                    Debug.WriteLine("disconnected");
                }

                sock.ConnectToServer(this.m_ip, (ushort)this.m_port, this.m_remoteip, (ushort)this.m_remoteport, this.m_vecName);
            }

            try
            {
                tmrConnect.Enabled = true;
            }
            catch (Exception) { }
        }

        // Vehicle의 Recv Message가 30초 동안 오지 않을 경우 Socket 재 연결
        int State_wait_count = 0;
        private void tmrState_Elapsed(object sender, ElapsedEventArgs e)
        {
            tmrState.Enabled = false;
            if (IsConnected)
            {
                if (State_wait_count >= 30)
                {
                    Reconnected();
                    State_wait_count = 0;
                }
                else
                {
                    State_wait_count++;
                }
            }
            tmrState.Enabled = true;
        }
        // tmrState에서 Socket 재 연결 시 사용되는 함수
        public override void Reconnected()
        {
            sock.StopClient();
            IsConnected = false;
            OnChageConnected?.Invoke(this, new ChangeConnectedArgs() { connected = false });
            Logger.Inst.Write(this.m_vecName, CmdLogType.All, $"VEHICLE Socket Close");

        }
        // Vehicle에 대한 모든 행동 중지
        public override void Dispose()
        {
            Csby.propertyStop = true;
            tmrConnect.Stop();
            tmrConnect.Dispose();
            tmrState.Stop();
            tmrState.Dispose();
            sock.StopClient();
            sock = null;
        }
        // Vehicle에 실행할 Job Assign 명령 전달 함수        
        public override void Assign(List<pepschedule> peps)
        {
            
        }

        // Vehicle에 현재 작업을 진행할 설비로 이동하는 GO 명령 전달 함수
        public override void GO(string BatchID)
        {
            SendMessageToVehicle(string.Format("GO;{0};", BatchID));
        }

        // Vehicle과 설비가 준비가 되었음을 확인하고 실제 Unload/Load 작업을 시작하는 JobStart 명령 전달 함수
        public override void JobStart()
        {
            SendMessageToVehicle(string.Format("JOB_START;"));
        }

        // FS 단위 반송 시 MAGAZINE이 여러개이면 MAGAZINE 교체 후 Unload/Load 작업을 시작하도록 하는 FS_MOVE_START 명령 전달 함수
        public override void FS_MOVE_START()
        {
            SendMessageToVehicle(string.Format("FS_MOVE_START;"));
        }
        // Vehicle과 설비가 준비가 되었음을 확인하고 실제 Unload/Load 작업을 시작하는 JobStart 명령 전달 함수
        public override void FSSTK_Start()
        {
            SendMessageToVehicle(string.Format("FSSTK_START;"));
        }

        // Vehicle에 Pause 명령 전달 함수
        public override void PAUSE()
        {
            controllerState = ControllerState.PAUSED;
            SendMessageToVehicle("REMOTE;PAUSE;");
        }

        // Vehicle를 Auto Mode로 변경 시키기 위한 Resume 명령 전달 함수
        public override void RESUME()
        {
            controllerState = ControllerState.AUTO;
            SendMessageToVehicle("REMOTE;RESUME;");
        }

        // Vehicle에 LD 행동 정지를 하기 위한 Stop 명령 전달 함수
        public override void STOP()
        {
            controllerState = ControllerState.STOP;
            SendMessageToVehicle("STOP;");
        }

        // Vehicle에 Job 취소를 하기위한 Cancel 명령 전달 함수
        public override void CANCEL()
        {
            SendMessageToVehicle("REMOTE;CANCEL;");
        }

        // Vehicle에 현재 가지고 있는 Job을 재 실행하기 위한 Job_Resume 명령 전달 함수
        public override void JOB_RESUME()
        {
            SendMessageToVehicle("JOB_RESUME;");
        }

        // Vehicle을 충전소로 보내기 위한 Charge 명령 전달 함수
        public override void CHARGE()
        {
            SendMessageToVehicle("CHARGE;");
        }

        // Vehicle에 해당 변수(_msg)를 보내는 함수
        public override void SEND_MSG(string _msg)
        {
            SendMessageToVehicle(_msg);
        }

        private bool _IsEndDistanceCalc = false;
        private int _CalcDistResult = -1;
        public override async Task<int> CALCDISTANCE_BTW(string goalName1, string goalName2)
        {
            _IsEndDistanceCalc = false;
            SendMessageToVehicle($"DISTANCEBTW;{goalName1};{goalName2};");
            int cnt = 0;
            while (!_IsEndDistanceCalc)
            {
                System.Threading.Thread.Sleep(10);
                if (cnt++ >= 500)
                {
                    _CalcDistResult = -1;
                    break;
                }
            }
            return _CalcDistResult;
        }

        // Vehicle에 msg 변수를 전달하는 함수()
        // CONNECT, GOAL_LD, MGTYPE_DATA는 Vehicle과 연결 시에 바로 전송하는 Message
        // 위 세가지 Message는 Vehicle의 controllerState에 상관없이 전송
        // 나머지 Message는 Vehicle의 controllerState가 Auto 일 때에만 전송
        public async void SendMessageToVehicle(string msg)
        {
            // Connect, Goal_LD, MGType_Data는 Robot의 상태에 상관없이 전송
            if (msg.Contains("CONNECT") || msg.Contains("GOAL_LD") || msg.Contains("MGTYPE_DATA"))
            {
                if (sock.Connected)
                {
                    try
                    {
                        await Task.Delay(1);
                        sock.SendMessage(msg + "\r\n");
                        Logger.Inst.Write(this.m_vecName, CmdLogType.Comm, $"S = [{msg}]");
                    }
                    catch (Exception e)
                    {
                        //spoolResp.Enqueue(msg);
                        Logger.Inst.Write(this.m_vecName, CmdLogType.Comm, $"{this.m_vecName}:예외 {e.ToString()} : S = [{msg}]");
                    }
                }
                else
                {
                    //spoolResp.Enqueue(msg);
                    Logger.Inst.Write(this.m_vecName, CmdLogType.Comm, $"{this.m_vecName}:연결이 되지 않아 보내지 못했습니다. : S = [{msg}]");
                }
            }
            // 그 외의 Data는 Auto 상태일 때에만 전송
            else
            {
                if (controllerState == ControllerState.AUTO)
                {
                    if (CurVecMode == VehicleMode.TEACHING && CurVecMode == VehicleMode.ERROR
                        || CurVecMode == VehicleMode.INIT && CurVecMode == VehicleMode.AUTO)
                    {
                        Logger.Inst.Write(this.m_vecName, CmdLogType.Comm, $"Ignore~~~~~~ VehicleMode is {CurVecMode.ToString()}");
                        return;
                    }
                    else
                    {
                        if (sock.Connected)
                        {
                            try
                            {
                                await Task.Delay(1);
                                sock.SendMessage(msg + "\r\n");
                                Logger.Inst.Write(this.m_vecName, CmdLogType.Comm, $"S = [{msg}]");
                            }
                            catch (Exception e)
                            {
                                Logger.Inst.Write(this.m_vecName, CmdLogType.Comm, $"{this.m_vecName}:예외 {e.ToString()} : S = [{msg}]");
                            }
                        }
                        else
                        {
                            Logger.Inst.Write(this.m_vecName, CmdLogType.Comm, $"{this.m_vecName}:연결이 되지 않아 보내지 못했습니다. : S = [{msg}]");
                        }
                    }
                }
            }
        }


        public override void goReset()
        {
            vecResponse = false;
            goReSend = false;
            goRetry_Fail = false;
            goRetry_count = 0;
        }
    }
    // Robot Class 의 기본 Data를 저장하기 위한 Class
    public abstract class SetRobot
    {
        public abstract Robot CreateRobot(string _vecName, string _ip, int _port, string _remoteip, Nullable<int> _remoteport);
    }
    // Robot Class 의 기본 Data를 저장하기 위한 Class
    // 입력 받는 변수를 다르게 가져갈 시 Class 명이 다른 Class 생성
    public class makeRobot : SetRobot
    {
        public override Robot CreateRobot(string _vecName, string _ip, int _port, string _remoteip, Nullable<int> _remoteport)
        {
            return new Type_FS(_vecName, _ip, _port, _remoteip, _remoteport);
        }
    }
    #endregion

    /// <summary>
    /// Rv Pattern은 Observer + Factory Pattern으로 작성
    /// Observer Pattern은 한 객체의 상태가 변경되면 그 객체에 의존하는 모든 객체가 변경되는 Pattern
    /// Factory Pattern은 Class의 인스턴스를 만드는 일을 Sub Class에서 실행하는 Pattern
    /// Main Class가 여러개이나 실행되는 인스턴스가 같을때 사용
    /// </summary>
    #region Rv Pattern
    // 실제 외부에서 사용하는 RvMsg Class
    // Rv Message를 Send하거나 Recv된 Message를 처리하는 Class
    public abstract class RvMsg
    {
        public RvMsgmm Rvmm = new RvMsgmm();
        public bool jobcancel_check = false;
        private bool _ismulti;
        public bool IsMulti
        {
            get { return _ismulti; }
            set { _ismulti = value; }
        }
        private List<pepschedule> _multilist;
        public List<pepschedule> MultiList
        {
            get
            {
                lock (this)
                {
                    return _multilist;
                }
            }
            set
            {
                lock (this)
                {
                    _multilist = value;
                }
            }
        }
        private pepschedule _curJob;
        public pepschedule CurJob
        {
            get { return _curJob; }
            set { _curJob = value; }
        }

        private string _jobtype;
        public string JobType
        {
            get { return _jobtype; }
            set { _jobtype = value; }
        }
        private bool _chamberfrist;
        public bool ChamberFrist
        {
            get { return _chamberfrist; }
            set { _chamberfrist = value; }
        }
        private bool _chamberfinish;
        public bool ChamberFinish
        {
            get { return _chamberfinish; }
            set { _chamberfinish = value; }
        }
        public abstract void Recv(string msg, string vecID);
    }

    // RvMsg Class를 실제로 구동하는 Class
    public class Rv_Type_FS : RvMsg
    {
        // Rv Message Recv 처리하는 함수
        public override void Recv(string msg, string vecID)
        {
            if (!Global.Init.Cfg.Data.UseRv)
                return;

            string[] words = msg.Split(' ');

            if (vecID.Contains("VEHICLE"))
            {
                switch (words[0])
                {
                    case "EQFSMOVECHECK_REP": EQFSMOVECHECK_REP(msg, words, vecID); break;
                    case "EQTEMPDOWNREQ_REP": EQTEMPDOWNREQ_REP(words, vecID); break;
                    case "EQDOOROPENED": EQDOOROPENED(words, vecID); break;
                    case "EQFSUNLOADSTANDBY_REP": EQFSUNLOADSTANDBY_REP(words, vecID); break;
                    case "EQFSUNLOADINFO_REP": EQFSUNLOADINFO_REP(words, vecID); break;
                    case "EQMGUNLOADCOMPLETE": EQMGUNLOADCOMPLETE(words, vecID); break;
                    case "EQFSLOADSTANDBY_REP": EQFSLOADSTANDBY_REP(words, vecID); break;
                    case "EQFSLOADINFO_REP": EQFSLOADINFO_REP(words, vecID); break;
                    case "EQMGLOADCOMPLETE":
                    case "EQMGLOADCOMPLETE_REP": EQMGLOADCOMPLETE(msg, words, vecID); break;
                    case "EQFSMGRETURNINFOREQ_REP": EQFSMGRETURNINFOREQ_REP(words, vecID); break;
                    case "EQFSMGRETURNCOMP_REP": EQFSMGRETURNCOMP_REP(words, vecID); break;
                    case "FSLOADINFOSET_REP": EQFSLOADINFOSET_REP(words, vecID); break;
                    default: break;
                }
            }
            else
            {
                if ((words[0] == "EQFSMOVEREQ"))
                {
                    EQFSMOVEREQ(words, Global.Init.First_Vehicle);
                }
                return;
            }
        }

        // EQTRAYMOVECHECK_REP Method Start
        #region EQTRAYMOVECHECK_REP
        public void EQFSMOVECHECK_REP(string msg, string[] words, string vecID)
        {
            unit_fs gtype = MOVECHECK_GoalName_Unit_Check(msg, words);

            if ((int)EqpGoalType.FSSTK == gtype.goaltype)
            {
                Global.Init.RvMsgList[vecID].Rvmm._bSucc = true;
            }
            else if ((int)EqpGoalType.AGING == gtype.goaltype || (int)EqpGoalType.RDT == gtype.goaltype)
            {
                EQFSMOVECHECK_REP_CHAMBER(words, vecID);
            }
            else
            {
                EQFSMOVECHECK_REP_SYSWIN(words, vecID);
            }
        }

        private unit_fs MOVECHECK_GoalName_Unit_Check(string msg, string[] words)
        {
            string goalname = string.Empty;
            if (msg.Contains("SUBEQPID"))
            {
                goalname = UtilMgr.FindKeyStringToValue(words, "SUBEQPID");
            }
            else
            {
                goalname = UtilMgr.FindKeyStringToValue(words, "EQPID");
                if (!goalname.Contains('-'))
                {
                    goalname += "-0";
                }
            }

            return Global.Init.Db.Units_FS.Where(p => p.GOALNAME == goalname).Single();
        }
        #endregion
        // EQTRAYMOVECHECK_REP Method End

        // EQTRAYMOVECHECK_REP_CHAMBER Method Start
        #region EQTRAYMOVECHECK_REP_CHAMBER
        private void EQFSMOVECHECK_REP_CHAMBER(string[] words, string vecID)
        {
            try
            {
                CHAMBER_EQSTATUS_Check(words);

                CHAMBER_STATUS_Check(words);

                //CHAMBER_ESINFO_Check(words, vecID);

                //CHAMBER_ESSTATUS_Check(words, vecID);

                Global.Init.RvMsgList[vecID].Rvmm._bSucc = true;
                return;
            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQFSMOVECHECK_REP_CHAMBER. {ex.Message}\r\n{ex.StackTrace}");
                Global.Init.RvMsgList[vecID].Rvmm._berror = false;
            }
            Global.Init.RvMsgList[vecID].Rvmm._bWait = true;
        }

        const string ERR_MOVECHK_REP_CHAMBER_EQSTATUS = "error. EQFSMOVECHECK_REP_CHAMBER. EQSTATUS is not RUN and IDLE";
        const string ERR_MOVECHK_REP_CHAMBER_STATUS = "error. EQFSMOVECHECK_REP_CHAMBER. STATUS is PASS";
        const string ERR_MOVECHK_REP_CHAMBER_ESINFO = "error. EQFSMOVECHECK_REP_CHAMBER. ESINSO is Not EMPTY";
        const string ERR_MOVECHK_REP_CHAMBER_ESSTATUS = "error. EQFSMOVECHECK_REP_CHAMBER. ESSTATUS is Not IDLE and Not Run";

        private void CHAMBER_EQSTATUS_Check(string[] words)
        {
            string eqstatus = UtilMgr.FindKeyStringToValue(words, "TESTSTATUS");
            if (eqstatus != "RUN" && eqstatus != "IDLE")
            {
                throw new UtilMgrCustomException(ERR_MOVECHK_REP_CHAMBER_EQSTATUS);
            }
        }
        private void CHAMBER_STATUS_Check(string[] words)
        {
            if (UtilMgr.FindKeyStringToValue(words, "STATUS") != "PASS")
            {
                throw new UtilMgrCustomException(ERR_MOVECHK_REP_CHAMBER_STATUS);
            }
        }
        private void CHAMBER_ESINFO_Check(string[] words, string vecID)
        {
            string esinfo = UtilMgr.FindKeyStringToValue(words, "ESINFO");
            esinfo = Regex.Replace(esinfo, "[()]", "");
            string[] es_data = esinfo.Split(',');

            string[] t_slotinfo = Global.Init.RvMsgList[vecID].CurJob.T_SLOT.Split(',');

            for (int i = 0; i < t_slotinfo.Count(); i++)
            {
                int slotnum = Convert.ToInt32(t_slotinfo[i].Substring(2, t_slotinfo[i].Length - 2));
                if (es_data[slotnum - 1] != "EMPTY")
                {
                    throw new UtilMgrCustomException(ERR_MOVECHK_REP_CHAMBER_ESINFO);
                }
            }
        }
        private void CHAMBER_ESSTATUS_Check(string[] words, string vecID)
        {
            string testdata = string.Empty;
            if (Global.Init.RvMsgList[vecID].JobType == "LOAD")
                testdata = "IDLE";
            else if (Global.Init.RvMsgList[vecID].JobType == "LOAD")
                testdata = "RUN";

            string esstatus = UtilMgr.FindKeyStringToValue(words, "ESSTATUS");
            esstatus = Regex.Replace(esstatus, "[()]", "");
            string[] esstauts_data = esstatus.Split(',');

            string[] t_slotinfo = Global.Init.RvMsgList[vecID].CurJob.T_SLOT.Split(',');
            for (int i = 0; i < t_slotinfo.Count(); i++)
            {
                int slotnum = Convert.ToInt32(t_slotinfo[i].Substring(2, t_slotinfo[i].Length - 2));

                if (esstauts_data[slotnum - 1] != testdata)
                {
                    throw new UtilMgrCustomException(ERR_MOVECHK_REP_CHAMBER_ESSTATUS);
                }
            }
        }
        #endregion
        // EQTRAYMOVECHECK_REP_SYSWIN Method End

        // EQTRAYMOVECHECK_REP_SYSWIN Method Start
        #region EQTRAYMOVECHECK_REP_SYSWIN
        private void EQFSMOVECHECK_REP_SYSWIN(string[] words, string vecID)
        {
            try
            {
                SYSWIN_EQSTATUS_Check(words);

                SYSWIN_STATUS_Check(words);

                Global.Init.RvMsgList[vecID].Rvmm._bSucc = true;
                return;
            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQFSMOVECHECK_REP_CHAMBER. {ex.Message}\r\n{ex.StackTrace}");
                Global.Init.RvMsgList[vecID].Rvmm._berror = false;
            }
            Global.Init.RvMsgList[vecID].Rvmm._bWait = true;
        }

        const string ERR_MOVECHK_REP_SYSWIN_EQSTATUS = "error. EQFSMOVECHECK_REP_SYSWIN. EQSTATUS is not RUN and IDLE";
        const string ERR_MOVECHK_REP_SYSWIN_STATUS = "error. EQFSMOVECHECK_REP_SYSWIN. STATUS is PASS";

        private void SYSWIN_EQSTATUS_Check(string[] words)
        {
            string eqstatus = UtilMgr.FindKeyStringToValue(words, "EQSTATUS");
            if (eqstatus != "RUN" && eqstatus != "IDLE")
            {
                throw new UtilMgrCustomException(ERR_MOVECHK_REP_SYSWIN_EQSTATUS);
            }
        }
        private void SYSWIN_STATUS_Check(string[] words)
        {
            if (UtilMgr.FindKeyStringToValue(words, "STATUS") != "PASS")
            {
                throw new UtilMgrCustomException(ERR_MOVECHK_REP_SYSWIN_STATUS);
            }
        }
        #endregion
        // EQTRAYMOVECHECK_REP_CHAMBER Method End

        // EQTEMPDOWNREQ_REP Method Start
        #region EQTEMPDOWNREQ_REP
        public void EQTEMPDOWNREQ_REP(string[] words, string vecID)
        {
            try
            {
                EQTEMPDOWNREQ_Status_Check(words);

                EQTEMPDOWNREQ_Send_List_Check(words, vecID);
                return;
            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQTEMPDOWNREQ_REP. {ex.Message}\r\n{ex.StackTrace}");
                Global.Init.RvMsgList[vecID].Rvmm._TempDownberror = true;
            }
            Global.Init.RvMsgList[vecID].Rvmm._TempDownbWait = true;

        }

        const string ERR_EQTEMPDOWNREQ_REP_STATUS = "error. EQTEMPDOWNREQ_REP. STATUS is not pass";
        private void EQTEMPDOWNREQ_Status_Check(string[] words)
        {
            if (UtilMgr.FindKeyStringToValue(words, "STATUS") != "PASS")
            {
                throw new UtilMgrCustomException(ERR_EQTEMPDOWNREQ_REP_STATUS);
            }
        }
        private void EQTEMPDOWNREQ_Send_List_Check(string[] words, string vecID)
        {
            lock (Global.Init.RvMsgList[vecID].Rvmm.syncTempDown)
            {
                int v = Global.Init.RvMsgList[vecID].Rvmm.lstTempDown.IndexOf(UtilMgr.FindKeyStringToValue(words, "SUBEQPID"));
                if (v >= 0)
                    Global.Init.RvMsgList[vecID].Rvmm.lstTempDown.RemoveAt(v);
            }

            if (Global.Init.RvMsgList[vecID].Rvmm.lstTempDown.Count() == 0)
            {
                Global.Init.RvMsgList[vecID].Rvmm._TempDownbSucc = true;
            }
        }

        #endregion
        // EQTEMPDOWNREQ_REP Method End

        public void EQDOOROPENED(string[] words, string vecID)
        {
            try
            {
                Global.Init.RvMsgList[vecID].Rvmm._bSucc = true;
                return;
            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQDOOROPENED. {ex.Message}\r\n{ex.StackTrace}");
                Global.Init.RvMsgList[vecID].Rvmm._berror = true;
            }
            Global.Init.RvMsgList[vecID].Rvmm._bWait = true;
        }
        public void EQFSLOADSTANDBY_REP(string[] words, string vecID)
        {
            try
            {
                Global.Init.RvMsgList[vecID].Rvmm._bSucc = true;
                return;
            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQFSLOADSTANDBY_REP. {ex.Message}\r\n{ex.StackTrace}");
                Global.Init.RvMsgList[vecID].Rvmm._berror = true;
            }
            Global.Init.RvMsgList[vecID].Rvmm._bWait = true;

        }
        public void EQFSUNLOADSTANDBY_REP(string[] words, string vecID)
        {
            try
            {
                Global.Init.RvMsgList[vecID].Rvmm._bSucc = true;
                return;
            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQFSUNLOADSTANDBY_REP. {ex.Message}\r\n{ex.StackTrace}");
                Global.Init.RvMsgList[vecID].Rvmm._berror = true;
            }
            Global.Init.RvMsgList[vecID].Rvmm._bWait = true;

        }

        // EQFSLOADINFO_REP/EQTRAYUNLOADINFO_REP Method Start
        #region EQFSLOADINFO_REP/EQTRAYUNLOADINFO_REP
        public void EQFSLOADINFO_REP(string[] words, string vecID)
        {
            try
            {
                EQFSLOAD_UNLOADINFO_REP_Status_Check(words, vecID);
                return;
            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQFSLOADINFO_REP. {ex.Message}\r\n{ex.StackTrace}");
                Global.Init.RvMsgList[vecID].Rvmm._berror = true;
            }
            Global.Init.RvMsgList[vecID].Rvmm._bWait = true;

        }
        public void EQFSLOADINFOSET_REP(string[] words, string vecID)
        {
            try
            {
                Global.Init.RvMsgList[vecID].Rvmm._bSucc = true;
                return;
            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQFSLOADINFOSET_REP. {ex.Message}\r\n{ex.StackTrace}");
                Global.Init.RvMsgList[vecID].Rvmm._berror = true;
            }
            Global.Init.RvMsgList[vecID].Rvmm._bWait = true;

        }

        public void EQFSUNLOADINFO_REP(string[] words, string vecID)
        {
            try
            {
                EQFSLOAD_UNLOADINFO_REP_Status_Check(words, vecID);
                return;
            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQFSUNLOADINFO_REP. {ex.Message}\r\n{ex.StackTrace}");
                Global.Init.RvMsgList[vecID].Rvmm._berror = true;
            }
            Global.Init.RvMsgList[vecID].Rvmm._bWait = true;

        }

        const string ERR_EQFSLOADINFO_REP_STATUS = "error. EQFSLOADINFO_REP. STATUS is not pass";
        const string ERR_EQFSUNLOADINFO_REP_STATUS = "error. EQFSUNLOADINFO_REP. STATUS is not pass";
        private void EQFSLOAD_UNLOADINFO_REP_Status_Check(string[] words, string vecID)
        {
            if (UtilMgr.FindKeyStringToValue(words, "STATUS") != "PASS")
            {
                LOAD_UNLOADINFO_Error(words[0]);
            }
            Global.Init.RvMsgList[vecID].Rvmm._bSucc = true;
        }
        private void LOAD_UNLOADINFO_Error(string word)
        {
            switch (word)
            {
                case "EQFSLOADINFO_REP":
                    throw new UtilMgrCustomException(ERR_EQFSLOADINFO_REP_STATUS);
                case "EQFSUNLOADINFO_REP":
                    throw new UtilMgrCustomException(ERR_EQFSUNLOADINFO_REP_STATUS);
                default:
                    break;
            }
        }

        #endregion
        // EQTRAYLOADINFO_REP/EQTRAYUNLOADINFO_REP Method End

        public void EQMGUNLOADCOMPLETE(string[] words, string vecID)
        {
            try
            {
                Global.Init.RvMsgList[vecID].Rvmm._bSucc = true;
                return;

            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQMGUNLOADCOMPLETE. {ex.Message}\r\n{ex.StackTrace}");
                Global.Init.RvMsgList[vecID].Rvmm._berror = false;
            }
            Global.Init.RvMsgList[vecID].Rvmm._bWait = true;

        }
        public void EQMGLOADCOMPLETE(string msg, string[] words, string vecID)
        {
            try
            {
                Global.Init.RvMsgList[vecID].Rvmm._bSucc = true;
                return;

            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQMGLOADCOMPLETE. {ex.Message}\r\n{ex.StackTrace}");
                Global.Init.RvMsgList[vecID].Rvmm._berror = false;
            }
            Global.Init.RvMsgList[vecID].Rvmm._bWait = true;
        }

        // EQTRAYMOVEREQ Method Start
        #region EQTRAYMOVEREQ
        public void EQFSMOVEREQ(string[] words, string vecID)
        {
            try
            {
                EQFSMOVEREQ_sndMsg_Send(words, vecID);
            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQFSMOVEREQ. {ex.Message}\r\n{ex.StackTrace}");
                Global.Init.RvMsgList[vecID].Rvmm._berror = false;
            }
        }

        private void EQFSMOVEREQ_sndMsg_Send(string[] words, string vecID)
        {
            string sndMsg = EQFSMOVEREQ_sndMsg_Create(words);

            Global.Init.RvComu.Send(UtilMgr.FindKeyStringToValue(words, "EQPID"), sndMsg, MethodBase.GetCurrentMethod().Name, vecID);
        }
        private string EQFSMOVEREQ_sndMsg_Create(string[] words)
        {
            string sndMsg = string.Empty;
            string hdr = string.Format($"HDR=({UtilMgr.FindKeyStringToValue(words, "EQPID")},LH.MPLUS,GARA,TEMP)");
            for (int i = 0; i < words.Count(); i++)
            {
                if (words[i].Contains("HDR="))
                {
                    sndMsg += string.Format($" {hdr}");
                    sndMsg += " STATUS=PASS";
                }
                else if (words[i].Contains("EQFSMOVEREQ"))
                {
                    sndMsg += string.Format($" {words[i]}_REP");
                }
                else
                {
                    sndMsg += string.Format($" {words[i]}");
                }
            }
            return sndMsg;
        }

        #endregion
        // EQTRAYMOVEREQ Method End

        // EQFSMGRETURNINFOREQ_REP Method Start
        #region EQFSMGRETURNINFOREQ_REP
        public void EQFSMGRETURNINFOREQ_REP(string[] words, string vecID)
        {
            try
            {
                EQFSMGRETURNINFOREQ_REP_STATUS_Check(words);
                EQFSMGRETURNINFOREQ_REP_Slot_Check(words, vecID);
                Global.Init.RvMsgList[vecID].Rvmm._bSucc = true;
            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQFSMGRETURNINFOREQ. {ex.Message}\r\n{ex.StackTrace}");
                Global.Init.RvMsgList[vecID].Rvmm._berror = false;
            }
        }

        const string ERR_MOVECHK_REP_FSMGRETURNINFOREQ_REP_STATUS = "error. EQFSMGRETURNINFOREQ_REP. STATUS is Not PASS";
        const string ERR_MOVECHK_REP_FSMGRETURNINFOREQ_REP_SLOT = "error. EQFSMGRETURNINFOREQ_REP. SLOT is Null";
        const string ERR_MOVECHK_REP_FSMGRETURNINFOREQ_REP_FSSTKEQPID = "error. EQFSMGRETURNINFOREQ_REP. FSSTKEQPID is Null";
        private void EQFSMGRETURNINFOREQ_REP_STATUS_Check(string[] words)
        {
            if (UtilMgr.FindKeyStringToValue(words, "STATUS") != "PASS")
            {
                throw new UtilMgrCustomException(ERR_MOVECHK_REP_FSMGRETURNINFOREQ_REP_STATUS);
            }
        }
        private void EQFSMGRETURNINFOREQ_REP_Slot_Check(string[] words, string vecID)
        {
            string slot = UtilMgr.FindKeyStringToValue(words, "SFID");
            string eqpid = UtilMgr.FindKeyStringToValue(words, "FSSTKEQPID");

            if (slot == null || slot == "")
            {
                throw new UtilMgrCustomException(ERR_MOVECHK_REP_FSMGRETURNINFOREQ_REP_SLOT);
            }
            
            //Global.Init.RvMsgList[vecID].CurJob.T_SLOT = slot;
            var pep = Global.Init.Db.Peps.Where(p => p.BATCHID == Global.Init.RvMsgList[vecID].CurJob.BATCHID).SingleOrDefault();

            slot = Regex.Replace(slot, "[()]", "");
            string[] slot_split = slot.Split(',');
            string slot_make = string.Empty;

            foreach (var x in slot_split)
            {
                if (slot_make != "")
                    slot_make += ",";

                slot_make += "0,";
                slot_make += (Convert.ToInt32(x.Substring(2, x.Length - 2))).ToString("D3");
            }


            if (pep != null)
            {
                pep.T_SLOT = slot;
                pep.T_PORT = slot_make;
                    
                if (!string.IsNullOrEmpty(eqpid))
                    pep.T_EQPID = eqpid;
                Global.Init.Db.DbUpdate(TableType.PEPSCHEDULE);
            }
        }
        #endregion
        // EQFSMGRETURNINFOREQ_REP Method End

        // EQFSMGRETURNCOMP_REP Method Start
        #region EQFSMGRETURNCOMP_REP
        public void EQFSMGRETURNCOMP_REP(string[] words, string vecID)
        {
            try
            {
                EQFSMGRETURNCOMP_REP_STATUS_Check(words);
                Global.Init.RvMsgList[vecID].Rvmm._bSucc = true;
            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQFSMGRETURNCOMP. {ex.Message}\r\n{ex.StackTrace}");
                Global.Init.RvMsgList[vecID].Rvmm._berror = false;
            }
        }

        const string ERR_MOVECHK_REP_EQFSMGRETURNCOMP_REP_STATUS = "error. EQFSMGRETURNCOMP_REP. STATUS is PASS";
        private void EQFSMGRETURNCOMP_REP_STATUS_Check(string[] words)
        {
            if (UtilMgr.FindKeyStringToValue(words, "STATUS") != "PASS")
            {
                throw new UtilMgrCustomException(ERR_MOVECHK_REP_EQFSMGRETURNCOMP_REP_STATUS);
            }
        }
        #endregion
        // EQFSMGRETURNINFOREQ_REP Method End

    }

    public class RvMsgmm
    {
        public bool _berror;     // error 발생, _bwait 플래그가 의미 없어질, 없을 것이다
        public bool _bSucc;
        public bool _bWait;
        public bool _TempDownberror;     // error 발생, _bwait 플래그가 의미 없어질, 없을 것이다
        public bool _TempDownbSucc;
        public bool _TempDownbWait;
        public pepschedule job;
        public unit_fs eqp;
        public bool _bVehicleSucc;
        public bool _bVehicleWait;


        public bool tempdown_check = true;

        public bool lp_tray = false;
        public void Reset()
        {
            _berror = false;
            _bWait = false;
            _bSucc = false;
            job = null;
            eqp = null;
        }
        public void ResetFlag()
        {
            _berror = false;
            _bWait = false;
            _bSucc = false;
        }
        public void ResetvehicleFlag()
        {
            _bVehicleWait = false;
            _bVehicleSucc = false;
        }
        public void Alloc(SendJobToVecArgs1 e)
        {
            Reset();
            job = e.job;
            eqp = e.eqp;
        }
        public List<string> lstTempDown = new List<string>();
        public object syncTempDown = new object();
    }


    // Rv 통신 연결 Class
    public abstract class RvComu
    {
        // Rv Message Send
        Transport transport;
        public Transport Transport
        {
            get { return transport; }
            set { transport = value; }
        }
        // Rv Message Recv
        public Listener listener;

        // Rv 통신을 위한 Data
        public string m_service;
        public string m_network;
        public string m_daemon;
        public string m_listenerTopics;
        public string m_senderSubject;

        // Rv 통신연결을 위한 함수
        public abstract void RvStart();
        // Rv 재연결을 위한 타이머 함수
        public abstract void RvReconnectTimer();
        // RV Message를 보내는 함수 Mplus -> TC
        // eqpid는 Subject에 들어가는 변수 ex) RO1504, RR4201, MRSM
        public abstract bool Send(string eqpid, string sndMsg, string devTyp, string VehicleId);
        // PreTempDown Message를 보내는 함수
        public abstract void PreTempDown(SendPreTempDown e);
        // MRSM에 현재 상태를 RV Message로 보내는 함수
        public abstract bool MRSM_Send(string vecid, string alarm = "", string subalarm = "", string message_data = "");

    }
        
    public class RvProcess : RvComu
    {
        bool RvConnect = false;
        private Timer tmrConnect = new Timer();
        // Rv 통신을 위한 기본 값
        public RvProcess(string _service, string _network, string _daemon, string _listenerTopics, string _senderSubject)
        {
            this.m_service = _service;
            this.m_network = _network;
            this.m_daemon = _daemon;
            this.m_listenerTopics = _listenerTopics;
            this.m_senderSubject = _senderSubject;
        }

        public override void RvReconnectTimer()
        {
            tmrConnect.Interval = 5000;
            tmrConnect.Elapsed += tmrConnect_Elapsed;
            tmrConnect.Enabled = true;
        }

        private void tmrConnect_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!RvConnect)
            {
                var vecs = Global.Init.Db.Vechicles.Where(p => p.isUse == 1 && p.TRANSFERTYPE == "FS").ToList();
                Global.Init.RendezvousSetup(vecs);
            }

            try
            {
                tmrConnect.Enabled = true;
            }
            catch (Exception) { }
        }

        // Rv 통신 시작
        public override async void RvStart()
        {
            await Run();
        }

        // Rv Transport/listener 설정
        public async Task Run()
        {
            Logger.Inst.Write(CmdLogType.Rv, $"RVHandler 시작");

            EventArgRvtate rvState = new EventArgRvtate() { state = RvState.disconnected };
            try
            {
                TIBCO.Rendezvous.Environment.Open();
                
                this.Transport = new NetTransport(this.m_service, this.m_network, this.m_daemon);
                this.listener = new Listener(Queue.Default, this.Transport, this.m_listenerTopics, new object());
                listener.MessageReceived += OnMessageReceived;
                RvConnect = true;
                while (true)
                {
                    await Task.Delay(1);
                    try
                    {
                        Queue.Default.Dispatch();
                    }
                    catch (RendezvousException ex)
                    {
                        Logger.Inst.Write(CmdLogType.Rv, $"TIBCO.RendezvousException.\r\n{ex.Message}\r\n{ex.StackTrace}");

                        rvState.state = RvState.disconnected;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Inst.Write(CmdLogType.Rv, $"TIBCO.Exception.\r\n{ex.Message}\r\n{ex.StackTrace}");

                rvState.state = RvState.disconnected;
            }
            finally
            {
                Logger.Inst.Write(CmdLogType.Rv, $"RVHandler 종료");

                TIBCO.Rendezvous.Environment.Close();

                rvState.state = RvState.disconnected;
                RvConnect = false;
            }
        }

        // Rv Message Recv 함수
        public void OnMessageReceived(object listener, MessageReceivedEventArgs messageReceivedEventArgs)
        {
            if (!Global.Init.Cfg.Data.UseRv)
                return;

            Message message = messageReceivedEventArgs.Message;
            string msg = message.GetField("DATA").Value.ToString();
            string[] words = msg.Split(' ');
            string vecID = string.Empty;
            if (msg.Contains("MRNO"))
            {
                vecID = UtilMgr.FindKeyStringToValue(words, "MRNO");
                vehicle vec_chk = MainHandler.Init.Db.Vechicles.Where(p => p.ID == vecID).SingleOrDefault();
                if (vec_chk != null && vec_chk.TRANSFERTYPE == "FS")
                {
                    if (vecID != "")
                    {
                        Logger.Inst.Write(vecID, CmdLogType.Rv, "R <= " + msg);
                        Global.Init.RvMsgList[vecID].Recv(msg, vecID);
                    }
                    else
                    {
                        Logger.Inst.Write(CmdLogType.Rv, "R <= " + msg);
                    }
                }
            }
            else
            {
                Logger.Inst.Write(CmdLogType.Rv, "R <= " + msg);
                Global.Init.RvMsgList["PROGRAM"].Recv(msg, "PROGRAM");
            }
        }

        // Rv Message Send 함수
        // SendSubject과 Message를 받아 전송
        public override bool Send(string eqpid, string sndMsg, string devTyp, string VehicleId)
        {
            try
            {
                if (!sndMsg.Contains("JOBCANCEL") && VehicleId != "PROGRAM")
                {
                    bool ret = IsControllerStop(VehicleId);
                    if (ret)
                        return !ret;
                }
                else if (VehicleId == "PROGRAM")
                {
                    VehicleId = "VEHICLE05";
                }
                string newSubject = string.Format("{0}.{1}", m_senderSubject, eqpid);
                Message message = new Message { SendSubject = newSubject };
                message.AddField("DATA", sndMsg);
                Transport.Send(message);

                Logger.Inst.Write(VehicleId, CmdLogType.Rv, $"S=>{sndMsg}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Inst.Write(VehicleId, CmdLogType.Rv, $"exception {devTyp}, {ex.Message}\r\n{ex.StackTrace}");
                return false;
            }
        }

        public override async void PreTempDown(SendPreTempDown e)
        {
            Logger.Inst.Write(CmdLogType.Rv, $"EVENT:_Vsp_OnSendTempDown. S:{e.srcUnit},D:{e.dstUnit}");
            if (Global.Init.Cfg.Data.UseRv)
            {
                // PreTempDown 시작 시간 저장
                e.job.C_srcAssignTime = DateTime.Now;
                // PreTempDown 전송 함수
                bool b = await SYSWIN_TempPreDownRequest(e.srcUnit, e.dstUnit, e.downtemp, e.vec.ID);
                // PreTempDown 종료 시간 저장
                e.job.C_dstFinishTime = DateTime.Now;

                // 전송 성공/실패에 따른 State Data 저장
                e.job.C_state = (b == true) ? (int)CmdState.PRETEMPDOWN_SUCC : (int)CmdState.PRETEMPDOWN_FAIL;

                Global.Init.Db.DbUpdate(TableType.PEPSCHEDULE);    // FormMonitor update				
            }
        }

        public override bool MRSM_Send(string vecid, string alarm = "", string subalarm = "", string message_data = "")
        {
            string now_time = DateTime.Now.ToString();
            string sndMsg = string.Format($"MRSTATUSMONITORING HDR=(KDS1.LH.MRSM,LH.MPLUS,GARA,TEMP) EVENT_TIME={now_time} LINEID=DSR7F EQPID={vecid} ALARMTYPE={alarm} SUBALARMTYPE={subalarm} MESSAGE={message_data}");
            //return true;
            return Send("MRSM", sndMsg, MethodBase.GetCurrentMethod().Name, vecid);
        }

        public bool IsControllerStop(string vecid)
        {
            var controller = Global.Init.Db.Controllers.SingleOrDefault();
            if (controller == null)
            {
                return false;
            }

            if (Global.Init.RobotList[vecid].isStop)
                return true;

            if (controller.C_state != (int)ControllerState.STOP)
                return false;

            return true;
        }

        // SYSWIN_TempPreDownRequest Method Start
        #region SYSWIN_TempPreDownRequest
        public async Task<bool> SYSWIN_TempPreDownRequest(unit_fs srcUnit, unit_fs dstUnit, Nullable<int> downtemp, string vecID)
        {
            await Task.Delay(1);
            string sndMsg = string.Empty;
            string eqp = string.Empty;
            bool bRet = true;
            for (int i = 0; i < 2; i++)
            {
                // PreTempDown Send Message 생성 함수
                if (!SYSWIN_TempPreDownRequest_sndMsg_Create(i, srcUnit, dstUnit, downtemp, ref eqp, ref sndMsg))
                    continue;

                try
                {
                    //PreTempDown은 TempDown과 마찬가지로 설비가 2대일때 동시에 데이터를 내려보내야하므로 NoWait로 변경
                    bRet = SendRvMessageNoWait(eqp, sndMsg, vecID);
                }
                catch (Exception ex)
                {
                    Logger.Inst.Write(CmdLogType.Rv, $"SYSWIN_TempPreDownRequest. Exception Occur. exit.\r\n{ex.Message}\r\n{ex.StackTrace}");
                    return false;
                }
            }
            return bRet;
        }
        private bool SYSWIN_TempPreDownRequest_sndMsg_Create(int i, unit_fs srcUnit, unit_fs dstUnit, Nullable<int> downtemp, ref string eqp, ref string sndMsg)
        {
            string eqpid = string.Empty;
            // i가 0 이고 Src 설비가 null이 아니고 Src 설비 이름이 RO를 가지고 있고 Job에서 설정된 downtemp가 1 또는 3이면
            if (i == 0 && srcUnit != null && srcUnit.goaltype != (int)EqpGoalType.FSSTK && (downtemp == 1 || downtemp == 3))
            {
                eqp = srcUnit.ID.Split('-')[0];
                if (srcUnit.goaltype == (int)EqpGoalType.RDT)
                    eqpid = srcUnit.ID.Substring(0, 8);
                else
                    eqpid = srcUnit.ID.Split('-')[0];
                sndMsg = string.Format($"EQPRETEMPDOWNREQ HDR=({srcUnit.ID.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={eqpid} SUBEQPID={srcUnit.ID}");
            }
            // i가 1이고 Dst 설비가 null이 아니고 Dst 설비 이름이 RO를 가지고 있고 Job에서 설정된 downtemp가 2 또는 3이면
            else if (i == 1 && dstUnit != null && dstUnit.goaltype != (int)EqpGoalType.FSSTK && (downtemp == 2 || downtemp == 3))
            {
                eqp = dstUnit.ID.Split('-')[0];
                if (dstUnit.goaltype == (int)EqpGoalType.RDT)
                    eqpid = dstUnit.ID.Substring(0, 8);
                else
                    eqpid = dstUnit.ID.Split('-')[0];
                sndMsg = string.Format($"EQPRETEMPDOWNREQ HDR=({dstUnit.ID.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={eqpid} SUBEQPID={dstUnit.ID}");
            }
            else
                return false;
            return true;
        }
        private bool SendRvMessageNoWait(string eqpId, string sndMsg, string vecID)
        {
            Global.Init.RvMsgList[vecID].Rvmm.ResetFlag();

            return Send(eqpId, sndMsg, MethodBase.GetCurrentMethod().Name, vecID);
        }
        #endregion
        // SYSWIN_TempPreDownRequest Method End
    }
    // Robot Class 의 기본 Data를 저장하기 위한 Class
    public abstract class SetRv
    {
        public abstract RvComu CreateRV(string _service, string _network, string _daemon, string _listenerTopics, string _senderSubject);
    }

    // Robot Class 의 기본 Data를 저장하기 위한 Class
    public class makeRv : SetRv
    {
        public override RvComu CreateRV(string _service, string _network, string _daemon, string _listenerTopics, string _senderSubject)
        {
            return new RvProcess(_service, _network, _daemon, _listenerTopics, _senderSubject);           
        }
    }
    #endregion
}
