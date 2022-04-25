using FSMPlus.Ref;
using FSMPlus.Vehicles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FSMPlus.Logic
{
    public enum ProcStep
    {
        None,
        Pre_Assign,
        Chk_EqStatus,
        Req_EqTempDown,
        Job_Assign,
        
        Job_Cancel,

        Go_Mobile,     
        Job_Start,
        FS_Move_Start,
        FSSTK_Start,

        FSLoadInfoSet,
        FSUnLoadMove,
        FSUnLoadInfo,
        FSLoadMove,
        FSLoadInfo,
        FSJobComplete,
        FSMGReturnInfoReq,
        FSMGReturnComp,
        FSUnLoadStandby,
        FSLoadStandby,

        Wait_FSLoadInfoSet,
        Wait_FSLoadStandby,
        Wait_DoorOpened,
        Wait_FSLoadInfo,
        Wait_MGLoadComp,
        Wait_FSUnloadStandby,
        Wait_FSUnloadInfo,
        Wait_MGUnloadComp,
        Wait_FSMGReturnInfo,
        Wait_FSMGReturnComp,
        Wait_MRBPMagazine_Start,
        Wait_MRBPMagazine_End,
        Wait_TransComp,

        Wait_Job_Start,


        Err_EqStatus,
        Err_EqTempDown,
        Err_JobAssign,
        Err_Go_Mobile,
        Err_Job_Start,
        Err_FS_Move_Start,
        Err_FSSTK_Start,
        Err_FSLoadInfoSet,
        Err_FSUnLoadMove,
        Err_FSUnLoadInfo,
        Err_FSLoadMove,
        Err_FSLoadInfo,
        Err_FSJobComplete,
        Err_FSMGReturnInfoReq,
        Err_FSMGReturnComp,
        Err_FSUnLoadStandby,
        Err_FSLoadStandby,

        Err_Wait_FSLoadInfoSet,
        Err_Wait_FSLoadStandby,
        Err_Wait_DoorOpened,
        Err_Wait_FSLoadInfo,
        Err_Wait_MGLoadComplete,
        Err_Wait_FSUnloadStandby,
        Err_Wait_FSUnloadInfo,
        Err_Wait_MGUnLoadComplete,
        Err_Wait_FSMGReturnInfo,
        Err_Wait_FSMGReturnComp,
        Err_Wait_MRBPMagazine,
        Err_Wait_TransComp,

        Err_Wait_Job_Start,
    }

    public partial class JobProcess_member : Global
    {
        private vehicle _vec;
        public vehicle Vec
        { get { return _vec; }
            set { _vec = value; }
        }

        private string _vecId;
        protected string VecId
        { get { return _vecId; }
            set { _vecId = value; }
        }

        public bool IsStop { get; set; }
        private List<CallCmdProcArgs1> _callCmdProcArgslist = new List<CallCmdProcArgs1>();
        private object _syncCallCmdProcArgslist = new object();

        protected int CntCallCmdProcArgslist()
        {
            return _callCmdProcArgslist.Count();
        }

        protected void AddCallCmdProcArgslist(CallCmdProcArgs1 v)
        {
            lock (_syncCallCmdProcArgslist)
            {
                _callCmdProcArgslist.Add(v);
            }
        }

        protected CallCmdProcArgs1 RemoveCallCmdProcArgslist()
        {
            CallCmdProcArgs1 v = new CallCmdProcArgs1();
            if (CntCallCmdProcArgslist() <= 0)
                return null;

            lock(_syncCallCmdProcArgslist)
            {
                v = _callCmdProcArgslist.First();
                _callCmdProcArgslist.RemoveAt(0);
            }
            return v;
        }

    }

    public partial class JobProccess : JobProcess_member
    {
        public event EventHandler<CallExecuteTime> OnCallExecuteTime;

        public string jobType = string.Empty;
        public bool bFront = false;
        public CallCmdProcArgs1 CmdProcList;

        private struct TempNearestGoalStruct
        {
            public string goalName;
            public float goalDist;
        }
        public JobProccess()
        {
            Thread t1 = new Thread(new ThreadStart(Run));
            t1.Start();
        }

        public JobProccess(vehicle vec) : this()
        {
            Vec = vec;
            VecId = vec.ID;
        }

        private VertualPep _virtualPep = new VertualPep();

        private async void Run()
        {
            CallCmdProcArgs1 e = null;
            DateTime dtold = DateTime.Now;
            bool bret = false;
            while(!IsStop)
            {
                Thread.Sleep(500);

                if (Vec.isAssigned == 0)
                    continue;

                if (!bFront)
                    bFront = true;

                if (CntCallCmdProcArgslist() > 0)
                {
                    e = RemoveCallCmdProcArgslist();
                    if (bFront)
                    {
                        CmdProcList = e;
                    }
                    bret = CallCmdProcedure(e);
                    
                    dtold = DateTime.Now;
                    CallExecuteTime execute = new CallExecuteTime();
                    execute.executeTime = "";
                    OnCallExecuteTime?.Invoke(this, execute);
                }
            }
        }
        public void Vehicle_OnChageConnected(object sender, ChangeConnectedArgs e)
        {
            if (sender is Robot vec)
            {
                var targetVec = Db.Vechicles.Where(p => p.ID == vec.m_vecName).Single();
                if (targetVec.installState != Convert.ToInt32(e.connected))
                {
                    if (e.connected)
                    {
                        targetVec.installState = (int)VehicleInstallState.INSTALLED;
                    }
                    else
                    {
                        targetVec.installState = (int)VehicleInstallState.REMOVED;
                        targetVec.C_lastArrivedUnit = string.Empty;
                    }
                    Db.DbUpdate(true, new TableType[] { TableType.VEHICLE });
                }
            }
        }

        /// <summary>
        /// 설비별 타입을 보고 SRC JOB, DST JOB 에 사용될 함수를 구분하고, RV 인터페이스한다
        /// S_EQPID 가 STK 이면, STK 는 UNLOAD, EQ 는 LOAD
        /// T_EQPID 가 STK 이면, STK 는 LOAD, EQ 는 UNLOAD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public bool OnSendJobToVehicle(SendJobToVecArgs1 e)
        {
            if (!string.Equals(e.vecID, VecId))
                return false;

            Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"EVENT:_Vsp_OnSendJobToVehicle. JOBTYPE:{e.cmd},S:{e.job.S_EQPID},D:{e.job.T_EQPID}");

            string S_eqpid = string.Empty;
            // 나중에 Multi Job을 사용하게되면 EQPID가 여러개가 생기는 경우가 생김
            // 그 때 맨 앞에 있는 Data만 사용하기 위한 Split
            string T_eqpid = e.job.T_EQPID.Split(',')[0];

            // 현재 Job이 DST 이고 Worktype이 FSI 또는 FSO 이고 Return_Check가 false 일때
            if (e.cmd == "DST" && (e.job.WORKTYPE == "FSI" || e.job.WORKTYPE == "FSO") && !e.return_check)
                S_eqpid = "FSSTK-0";
            else
                S_eqpid = e.job.S_EQPID;

            // 현재 Job이 SRC 이고 Worktype이 FSO 일 때            
            if (e.cmd == "SRC" && e.job.WORKTYPE == "FSO")
                T_eqpid = "FSSTK-0";

            var src_units = Db.Units_FS.Where(p => p.ID == S_eqpid).ToList();
            var dst_units = Db.Units_FS.Where(p => p.ID == T_eqpid).ToList();

            if (e.cmd == "SRC")            
                Debug.Assert(src_units.Count() == 1);

            Debug.Assert(dst_units.Count() == 1);

            bool bret = false;
            if (e.cmd == "SRC")
            {
                // FSSTK에서 UNLOAD 작업
                if (src_units[0].goaltype == (int)EqpGoalType.FSSTK)
                {
                    bret = From_FSSTK(e, src_units[0], dst_units[0]);
                }
                // Chamber에서 UNLOAD 작업
                else if (src_units[0].goaltype == (int)EqpGoalType.RDT || src_units[0].goaltype == (int)EqpGoalType.AGING)
                {
                    bret = From_CHAMBER(e, src_units[0], dst_units[0]);
                }
                // Chamber에서 UNLOAD 작업
                else
                {
                    bret = From_SYSWIN(e, src_units[0], dst_units[0]);
                }
            }
            else if (e.cmd == "DST")
            {
                // FSSTK에서 LOAD 작업
                if (dst_units[0].goaltype == (int)EqpGoalType.FSSTK)
                {
                    bret = to_FSSTK(e, src_units[0], dst_units[0]);
                }
                // Chamber에서 LOAD 작업
                else if (dst_units[0].goaltype == (int)EqpGoalType.RDT || dst_units[0].goaltype == (int)EqpGoalType.AGING)
                {
                    bret = to_CHAMBER(e, src_units[0], dst_units[0]);
                }
                // Chamber에서 LOAD 작업
                else
                {
                    bret = to_SYSWIN(e, src_units[0], dst_units[0]);
                }
            }

            if (!bret)
                return bret;
            else
            {                
                return bret;
            }

        }

        // Unload Magazine
        // FSI, FSO, MGI, MGO 작업은 무조건 FSSTK에서 Magazine을 Unload 한다.
        private bool From_FSSTK(SendJobToVecArgs1 e, unit_fs srcUnit, unit_fs dstUnit)
        {
            Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"From_FSSTK. B:[{e.job.BATCHID}],S:[{e.job.S_EQPID}],D:[{e.job.T_EQPID}]");
            e.rvAssign = RvAssignProc.From_FSSTK;

            RvMsgList[e.vecID].JobType = "UNLOAD";

            bool bret = false;
            
            // job pre assign
            bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Pre_Assign);
            if (Cfg.Data.UseRv)
            {
                // 설비 상태 체크
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Chk_EqStatus);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_EqStatus);

                // FSI, MGI 작업의 경우 FSSTK 작업전 설비에 미리 LoadInfoSet을 전송한다.
                if (e.job.WORKTYPE == "FSI")
                {
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSLoadInfoSet);
                    if (!bret)
                        return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSLoadInfoSet);

                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_FSLoadInfoSet);
                    if (!bret)
                        return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_FSLoadInfoSet);
                }

                // 해당 설비로 이동하겠다는 Message를 TC로 전송
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSUnLoadMove);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSUnLoadMove);
            }

            // Mobile Robot에 진행할 Job의 Data 전송
            bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Job_Assign);
            if (!bret)
                return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_JobAssign);

            // Mobile Robot에 진행할 Job의 설비로 이동 명령
            bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Go_Mobile);
            if (!bret)
                return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Go_Mobile);

            Logger.Inst.Write(e.vecID, CmdLogType.Comm, $"차량[{e.vecID}]이 명령[{e.cmd};{e.job.BATCHID};] 위치에 도착했습니다.");

            if (Cfg.Data.UseRv)
            {
                // 해당 설비에 작업 진행 알림
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSUnLoadInfo);
                if (!bret)                                               
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSUnLoadInfo);

                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_FSUnloadInfo);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_FSUnloadInfo);
            }

            // Mobile Robot이 설비에 도착 후 Job 진행 명령
            bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSSTK_Start);
            if (!bret)
                return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSSTK_Start);

            if (Cfg.Data.UseRv)
            {
                // MGLoadComplete Message 대기 함수 생성?
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_MGUnloadComp);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_MGUnLoadComplete);
            }
            // Robot만 연결하여 Test 시 사용, 실제 양산적용시 삭제해도 무관
            else
            {
                // Mobile Robot Job Complete를 확인하여 종료
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_TransComp);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_TransComp);
            }

            Logger.Inst.Write(e.vecID, CmdLogType.Comm, $"From_FSSTK. B:[{e.job.BATCHID}],S:[{e.job.S_EQPID}] 작업을 마칩니다");
            e.fsstk_send = true;
            return true;
        }

        // Return Magazine
        // FSI, FSO 작업은 무조건 빈 Magazine을 FSSTK에 Load 한다.
        // MGO 작업 시 CHAMBER에서 FSSTK로 Magazine을 Load 한다.
        private bool to_FSSTK(SendJobToVecArgs1 e, unit_fs srcUnit, unit_fs dstUnit)
        {
            bool bret = false;
            if (e.job.WORKTYPE.Contains("FS"))
            {
                if (Cfg.Data.UseRv)
                {
                    // Return 작업 중 JobCancel 시 원본 PFS Job은 History로 넘어가 있는 상태이므로
                    // DB에서 원본 PFS Job이 없으면 JobComplete는 Skip 하도록한다
                    string batchid = string.Format($"P{e.job.WORKTYPE}_{e.job.BATCHID.Split('_')[1]}");
                    var pep = Db.Peps.Where(p => p.BATCHID == batchid).SingleOrDefault();
                    if (pep != null)
                    {
                        // FSI, FSO 작업 완료 후 JobComplete Message 전송
                        bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSJobComplete);
                        if (!bret)
                            return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSJobComplete);
                    }
                }
                else
                {
                    // Return 작업 중 JobCancel 시 원본 PFS Job은 History로 넘어가 있는 상태이므로
                    // DB에서 원본 PFS Job이 없으면 JobComplete는 Skip 하도록한다
                    string batchid = string.Format($"P{e.job.WORKTYPE}_{e.job.BATCHID.Split('_')[1]}");
                    var pep = Db.Peps.Where(p => p.BATCHID == batchid).SingleOrDefault();
                    if (pep != null)
                    {
                        // Mobile Robot Job Complete를 확인하여 종료
                        bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_TransComp);
                        if (!bret)
                            return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_TransComp);
                    }
                }
            }

            Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"to_FSSTK. B:[{e.job.BATCHID}],S:[{e.job.S_EQPID}],D:[{e.job.T_EQPID}]");
            e.rvAssign = RvAssignProc.to_FSSTK;

            RvMsgList[e.vecID].JobType = "LOAD";

            if (Cfg.Data.UseRv)
            {
                // 작업 완료된 MAGAZINE을 FSSTK로 돌려놓기위해 정보 전송 
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSMGReturnInfoReq);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSMGReturnInfoReq);

                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_FSMGReturnInfo);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_FSMGReturnInfo);

                // 설비 상태 체크
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Chk_EqStatus);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_EqStatus);

                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSLoadMove);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSLoadMove);
            }



            // Mobile Robot에 진행할 Job의 Data 전송
            bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Job_Assign);
            if (!bret)
                return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_JobAssign);

            // Mobile Robot에 진행할 Job의 설비로 이동 명령
            bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Go_Mobile);
            if (!bret)
                return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Go_Mobile);

            Logger.Inst.Write(e.vecID, CmdLogType.Comm, $"차량[{e.vecID}]이 명령[{e.cmd};{e.job.BATCHID};] 위치에 도착했습니다.");

            if (Cfg.Data.UseRv)
            {
                // 해당 설비에 작업 진행 알림
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSLoadInfo);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSLoadInfo);

                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_FSLoadInfo);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_FSLoadInfo);
            }

            // Mobile Robot이 설비에 도착 후 Job 진행 명령
            bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSSTK_Start);
            if (!bret)
                return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSSTK_Start);

            if (Cfg.Data.UseRv)
            {
                // MGLoadComplete Message 대기 함수 생성?
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_MGLoadComp);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_MGLoadComplete);

                // 작업 완료된 MAGAZINE을 FSSTK로 Load 완료
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSMGReturnComp);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSMGReturnComp);
            }
            // Robot만 연결하여 Test 시 사용, 실제 양산적용시 삭제해도 무관
            else
            {
                // Mobile Robot Job Complete를 확인하여 종료
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_TransComp);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_TransComp);
            }


            Logger.Inst.Write(e.vecID, CmdLogType.Comm, $"to_FSSTK. B:[{e.job.BATCHID}],D:[{e.job.T_EQPID}] 작업을 마칩니다");
            return true;
        }

        // FSO, MGO 작업 시 설비 UnLoad
        // FSO 일 때는 Child Job 만큼 From_CHAMBER를 반복
        // 반복 시에는 UnLoadInfo Message 부터 반복
        // MGO 일 때는 Mobile Robot에 JOB_START 명령
        // FSO 일 때는 Mobile Robot에 FS_MOVE_START 명령
        private bool From_CHAMBER(SendJobToVecArgs1 e, unit_fs srcUnit, unit_fs dstUnit)
        {
            bool bret = false;

            // 첫 Child Job 일 때만 동작 또는 MGO, MGI 일때 동작
            if (!RvMsgList[e.vecID].ChamberFrist || e.job.WORKTYPE.Contains("MG"))
            {
                Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"From_CHAMBER. B:[{e.job.BATCHID}],S:[{e.job.S_EQPID}],D:[{e.job.T_EQPID}]");
                e.rvAssign = RvAssignProc.From_CHAMBER;

                RvMsgList[e.vecID].JobType = "UNLOAD";

                if (Cfg.Data.UseRv && e.job.WORKTYPE.Contains("MG"))
                {
                    // 설비 상태 체크
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Chk_EqStatus);
                    if (!bret)
                        return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_EqStatus);
                }

                if (Cfg.Data.UseRv)
                {
                    // 해당 설비로 이동하겠다는 Message를 TC로 전송
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSUnLoadMove);
                    if (!bret)
                        return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSUnLoadMove);
                }

                // Mobile Robot에 진행할 Job의 Data 전송
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Job_Assign);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_JobAssign);

                if (e.job.WORKTYPE == "FSO")
                    JobProcList[e.vec.ID].jobType = "DST";

                // Mobile Robot에 진행할 Job의 설비로 이동 명령
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Go_Mobile);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Go_Mobile);

                Logger.Inst.Write(e.vecID, CmdLogType.Comm, $"차량[{e.vecID}]이 명령[{e.cmd};{e.job.BATCHID};] 위치에 도착했습니다.");

                if (Cfg.Data.UseRv)
                {
                    // 해당 설비에 작업 준비 요청(PIO Ready)
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSUnLoadStandby);
                    if (!bret)
                        return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSUnLoadStandby);

                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_FSUnloadStandby);
                    if (!bret)
                        return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_FSUnloadStandby);
                }

                if (e.job.WORKTYPE.Contains("FS") )
                {
                    // Magazine MRBF -> MRBP 이동 완료 대기
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_MRBPMagazine_End);
                    if (!bret)
                        return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_MRBPMagazine);
                }
                if (Cfg.Data.UseRv)
                {
                    // 설비 TempDown 요청 
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Req_EqTempDown);
                    if (!bret)
                        return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_EqTempDown);
                }
                // Mobile Robot이 설비에 도착 후 PIO 연결 명령
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Job_Start);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Job_Start);

                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_Job_Start);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_Job_Start);

                if (Cfg.Data.UseRv)
                {
                    // DoorOpened에 대한 Rep 대기
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_DoorOpened);
                    if (!bret)
                        return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_DoorOpened);
                }
                RvMsgList[e.vecID].ChamberFrist = true;
            }

            if (e.job.WORKTYPE == "FSO")
                JobProcList[e.vec.ID].jobType = "DST";

            if (Cfg.Data.UseRv)
            {
                // 해당 설비에 작업 진행 알림
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSUnLoadInfo);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSUnLoadInfo);

                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_FSUnloadInfo);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_FSUnloadInfo);
            }

            // Mobile Robot이 설비에 도착 후 Job 진행 명령 - FSO
            bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FS_Move_Start);
            if (!bret)
                return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FS_Move_Start);

            if (e.job.WORKTYPE.Contains("MG"))
            {
                if (Cfg.Data.UseRv)
                {
                    // FSLoadComp에 대한 Rep 대기
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_MGUnloadComp);
                    if (!bret)
                        return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_MGUnLoadComplete);

                    // FSJobComplete 전송
                    // FSLoadComp와 M6X의 완료 메세지를 확인하여 전송
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSJobComplete);
                    if (!bret)
                        return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSJobComplete);
                }
                // Robot만 연결하여 Test 시 사용, 실제 양산적용시 삭제해도 무관
                else
                {
                    // Vehicle의 TransComp Message 대기                
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_TransComp);
                    if (!bret)
                        return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_TransComp);
                }
            }
            else
            {
                if (Cfg.Data.UseRv)
                {
                    // FSLoadComp에 대한 Rep 대기
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_MGUnloadComp);
                    if (!bret)
                        return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_MGUnLoadComplete);
                }
                // Robot만 연결하여 Test 시 사용, 실제 양산적용시 삭제해도 무관
                else
                {
                    // Vehicle의 TransComp Message 대기                
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_TransComp);
                    if (!bret)
                        return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_TransComp);
                }

                if (RvMsgList[e.vecID].ChamberFinish)
                {
                    // Magazine MRBP -> MRBF, MRBF -> MRBP 이동 완료 대기
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_MRBPMagazine_Start);
                }
                else
                {
                    // Magazine MRBP -> MRBF, MRBF -> MRBP 이동 완료 대기
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_MRBPMagazine_End);
                }
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_MRBPMagazine);
            }

            Logger.Inst.Write(e.vecID, CmdLogType.Comm, $"to_SYSWIN. B:[{e.job.BATCHID}],D:[{e.job.T_EQPID}] 작업을 마칩니다");
            return true;

        }

        // FSI, MGI 작업 시 설비 Load
        // FSI 일 때는 Child Job 만큼 to_CHAMBER를 반복
        // 반복 시에는 LoadInfo Message 부터 반복
        // MGI 일 때는 Mobile Robot에 JOB_START 명령
        // FSI 일 때는 Mobile Robot에 FS_MOVE_START 명령
        private bool to_CHAMBER(SendJobToVecArgs1 e, unit_fs srcUnit, unit_fs dstUnit)
        {
            bool bret = false;
            
            if (!RvMsgList[e.vecID].ChamberFrist)
            {
                Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"to_CHAMBER. B:[{e.job.BATCHID}],S:[{e.job.S_EQPID}],D:[{e.job.T_EQPID}]");
                e.rvAssign = RvAssignProc.to_CHAMBER;

                RvMsgList[e.vecID].JobType = "LOAD";
                if (Cfg.Data.UseRv)
                {
                    // 해당 설비로 이동하겠다는 Message를 TC로 전송
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSLoadMove);
                    if (!bret)
                        return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSLoadMove);
                }

                // Mobile Robot에 진행할 Job의 Data 전송
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Job_Assign);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_JobAssign);

                // Mobile Robot에 진행할 Job의 설비로 이동 명령
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Go_Mobile);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Go_Mobile);

                Logger.Inst.Write(e.vecID, CmdLogType.Comm, $"차량[{e.vecID}]이 명령[{e.cmd};{e.job.BATCHID};] 위치에 도착했습니다.");

                if (e.job.WORKTYPE.Contains("FS"))
                {
                    if (Cfg.Data.UseRv)
                    {
                        // 해당 설비에 작업 준비 요청(PIO Ready)
                        bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSLoadStandby);
                        if (!bret)
                            return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSLoadStandby);

                        bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_FSLoadStandby);
                        if (!bret)
                            return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_FSLoadStandby);
                    }

                    if (e.job.WORKTYPE.Contains("FS"))
                    {
                        // Magazine MRBF -> MRBP 이동 완료 대기
                        bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_MRBPMagazine_End);
                        if (!bret)
                            return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_MRBPMagazine);
                    }
                }
                if (Cfg.Data.UseRv)
                {
                    // 설비 TempDown 요청 
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Req_EqTempDown);
                    if (!bret)
                        return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_EqTempDown);
                }

                // Mobile Robot이 설비에 도착 후 PIO 연결 명령

                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Job_Start);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Job_Start);

                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_Job_Start);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_Job_Start);

                if (Cfg.Data.UseRv)
                {
                    // DoorOpened에 대한 Rep 대기
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_DoorOpened);
                    if (!bret)
                        return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_DoorOpened);
                }
                RvMsgList[e.vecID].ChamberFrist = true;
            }

            if (Cfg.Data.UseRv)
            {
                // 해당 설비에 작업 진행 알림
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSLoadInfo);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSLoadInfo);

                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_FSLoadInfo);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_FSLoadInfo);
            }

            // Mobile Robot이 설비에 도착 후 Job 진행 명령 - FSO
            bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FS_Move_Start);
            if (!bret)
                return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FS_Move_Start);

            

            if (e.job.WORKTYPE.Contains("MG"))
            {
                if (Cfg.Data.UseRv)
                {
                    // MGLoadComplete Message 대기 함수 생성?
                    if (Cfg.Data.UseRv)
                    {
                        bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_MGLoadComp);
                        if (!bret)
                            return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_MGLoadComplete);
                    }

                    // FSJobComplete 전송
                    // FSLoadComp와 M6X의 완료 메세지를 확인하여 전송
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSJobComplete);
                    if (!bret)
                        return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSJobComplete);
                }
                // Robot만 연결하여 Test 시 사용, 실제 양산적용시 삭제해도 무관
                else
                {
                    // Vehicle의 TransComp Message 대기                
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_TransComp);
                    if (!bret)
                        return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_TransComp);
                }
            }
            else
            {
                if (Cfg.Data.UseRv)
                {
                    // MGLoadComplete Message 대기 함수 생성?
                    if (Cfg.Data.UseRv)
                    {
                        bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_MGLoadComp);
                        if (!bret)
                            return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_MGLoadComplete);
                    }
                }
                // Robot만 연결하여 Test 시 사용, 실제 양산적용시 삭제해도 무관
                else
                {
                    // Vehicle의 TransComp Message 대기                
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_TransComp);
                    if (!bret)
                        return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_TransComp);
                }

                if (RvMsgList[e.vecID].ChamberFinish)
                {
                    // Magazine MRBP -> MRBF, MRBF -> MRBP 이동 완료 대기
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_MRBPMagazine_Start);
                }
                else
                {
                    // Magazine MRBP -> MRBF, MRBF -> MRBP 이동 완료 대기
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_MRBPMagazine_End);
                }
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_MRBPMagazine);

                
            }

            Logger.Inst.Write(e.vecID, CmdLogType.Comm, $"to_SYSWIN. B:[{e.job.BATCHID}],D:[{e.job.T_EQPID}] 작업을 마칩니다");
            return true;

        }



        // FSO, MGO 작업 시 설비 UnLoad
        // FSO 일 때는 Child Job 만큼 From_CHAMBER를 반복
        // 반복 시에는 UnLoadInfo Message 부터 반복
        // MGO 일 때는 Mobile Robot에 JOB_START 명령
        // FSO 일 때는 Mobile Robot에 FS_MOVE_START 명령
        private bool From_SYSWIN(SendJobToVecArgs1 e, unit_fs srcUnit, unit_fs dstUnit)
        {
            bool bret = false;

            Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"From_SYSWIN. B:[{e.job.BATCHID}],S:[{e.job.S_EQPID}],D:[{e.job.T_EQPID}]");
            e.rvAssign = RvAssignProc.From_SYSWIN;

            RvMsgList[e.vecID].JobType = "UNLOAD";

            if (Cfg.Data.UseRv)
            {
                // 설비 상태 체크
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Chk_EqStatus);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_EqStatus);


                // 해당 설비로 이동하겠다는 Message를 TC로 전송
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSUnLoadMove);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSUnLoadMove);
            }

            // Mobile Robot에 진행할 Job의 Data 전송
            bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Job_Assign);
            if (!bret)
                return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_JobAssign);

            if (e.job.WORKTYPE == "FSO")
                JobProcList[e.vec.ID].jobType = "DST";

            // Mobile Robot에 진행할 Job의 설비로 이동 명령
            bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Go_Mobile);
            if (!bret)
                return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Go_Mobile);

            Logger.Inst.Write(e.vecID, CmdLogType.Comm, $"차량[{e.vecID}]이 명령[{e.cmd};{e.job.BATCHID};] 위치에 도착했습니다.");


            if (Cfg.Data.UseRv)
            {
                // 해당 설비에 작업 진행 알림
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSUnLoadInfo);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSUnLoadInfo);

                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_FSUnloadInfo);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_FSUnloadInfo);
            }

            if (Cfg.Data.UseRv)
            {
                // 설비 TempDown 요청 
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Req_EqTempDown);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_EqTempDown);
            }
            // Mobile Robot이 설비에 도착 후 PIO 연결 명령
            bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Job_Start);
            if (!bret)
                return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Job_Start);

            bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_Job_Start);
            if (!bret)
                return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_Job_Start);

            if (Cfg.Data.UseRv)
            {
                // DoorOpened에 대한 Rep 대기
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_DoorOpened);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_DoorOpened);
            }

            // Mobile Robot이 설비에 도착 후 Job 진행 명령 - FSO
            bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FS_Move_Start);
            if (!bret)
                return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FS_Move_Start);

            if (Cfg.Data.UseRv)
            {
                // FSLoadComp에 대한 Rep 대기
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_MGUnloadComp);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_MGUnLoadComplete);

                // FSJobComplete 전송
                // FSLoadComp와 M6X의 완료 메세지를 확인하여 전송
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSJobComplete);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSJobComplete);
            }
            // Robot만 연결하여 Test 시 사용, 실제 양산적용시 삭제해도 무관
            else
            {
                // Vehicle의 TransComp Message 대기                
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_TransComp);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_TransComp);
            }


            Logger.Inst.Write(e.vecID, CmdLogType.Comm, $"to_SYSWIN. B:[{e.job.BATCHID}],D:[{e.job.T_EQPID}] 작업을 마칩니다");
            return true;

        }

        // FSI, MGI 작업 시 설비 Load
        // FSI 일 때는 Child Job 만큼 to_CHAMBER를 반복
        // 반복 시에는 LoadInfo Message 부터 반복
        // MGI 일 때는 Mobile Robot에 JOB_START 명령
        // FSI 일 때는 Mobile Robot에 FS_MOVE_START 명령
        private bool to_SYSWIN(SendJobToVecArgs1 e, unit_fs srcUnit, unit_fs dstUnit)
        {
            bool bret = false;

            Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"to_SYSWIN. B:[{e.job.BATCHID}],S:[{e.job.S_EQPID}],D:[{e.job.T_EQPID}]");
            e.rvAssign = RvAssignProc.to_CHAMBER;

            RvMsgList[e.vecID].JobType = "LOAD";
            if (Cfg.Data.UseRv)
            {
                // 해당 설비로 이동하겠다는 Message를 TC로 전송
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSLoadMove);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSLoadMove);
            }

            // Mobile Robot에 진행할 Job의 Data 전송
            bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Job_Assign);
            if (!bret)
                return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_JobAssign);

            // Mobile Robot에 진행할 Job의 설비로 이동 명령
            bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Go_Mobile);
            if (!bret)
                return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Go_Mobile);

            Logger.Inst.Write(e.vecID, CmdLogType.Comm, $"차량[{e.vecID}]이 명령[{e.cmd};{e.job.BATCHID};] 위치에 도착했습니다.");


            if (Cfg.Data.UseRv)
            {
                // 해당 설비에 작업 진행 알림
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSLoadInfo);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSLoadInfo);

                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_FSLoadInfo);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_FSLoadInfo);
            }

            if (Cfg.Data.UseRv)
            {
                // 설비 TempDown 요청 
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Req_EqTempDown);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_EqTempDown);
            }

            // Mobile Robot이 설비에 도착 후 PIO 연결 명령

            bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Job_Start);
            if (!bret)
                return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Job_Start);

            bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_Job_Start);
            if (!bret)
                return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_Job_Start);

            if (Cfg.Data.UseRv)
            {
                // DoorOpened에 대한 Rep 대기
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_DoorOpened);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_DoorOpened);
            }

            // Mobile Robot이 설비에 도착 후 Job 진행 명령 - FSO
            bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FS_Move_Start);
            if (!bret)
                return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FS_Move_Start);


            if (Cfg.Data.UseRv)
            {
                // MGLoadComplete Message 대기 함수 생성?
                if (Cfg.Data.UseRv)
                {
                    bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_MGLoadComp);
                    if (!bret)
                        return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_MGLoadComplete);
                }

                // FSJobComplete 전송
                // FSLoadComp와 M6X의 완료 메세지를 확인하여 전송
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.FSJobComplete);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_FSJobComplete);
            }
            // Robot만 연결하여 Test 시 사용, 실제 양산적용시 삭제해도 무관
            else
            {
                // Vehicle의 TransComp Message 대기                
                bret = Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Wait_TransComp);
                if (!bret)
                    return Proc_Atom.Init.PROC_ATOM(e, srcUnit, dstUnit, ProcStep.Err_Wait_TransComp);
            }


            Logger.Inst.Write(e.vecID, CmdLogType.Comm, $"to_SYSWIN. B:[{e.job.BATCHID}],D:[{e.job.T_EQPID}] 작업을 마칩니다");
            return true;

        }

        /// <summary>
        /// SRC 또는 DST 양쪽에서 MULTIJOB 을 수행할 수 있는 모듈
        /// </summary>
        /// <param name="ismulti"></param>
        /// <param name="direct"></param>
        /// <param name="jobs"></param>
        /// <param name="vec"></param>
        /// <returns></returns>
        //public bool CallCmdProcedure(object sender, bool ismulti, eMultiJobWhere where, List<pepschedule> jobs, vehicle vec)
        public async void OnCallCmdProcedure(object sender, CallCmdProcArgs1 e)
        {
            if (e.vec.ID != VecId)
                return;

            await Task.Delay(1);
            AddCallCmdProcArgslist(e);
            return;
        }

        public bool CallCmdProcedure(CallCmdProcArgs1 e)
        {
            try
            {
                List<pepschedule> Jobs = e.peps;
                bool IsMultiJob = false;
                bool bret = false;
                int remakecount = 0;
                int remakeJobcount = 0;
                int foreachcount = 0;
                int I_type_remakcount = 0;
                int I_type_remakeJobcount = 0;
                int I_type_foreachcount = 0;

                Job_Select(Jobs, ref IsMultiJob);

                JobProcList[e.vec.ID].jobType = "SRC";
                if (Jobs[0].C_srcFinishTime == null)
                {
                    bret = ProcessSrcJob(IsMultiJob, ref Jobs, e, ref remakecount, ref I_type_remakcount);
                    if (!bret)
                        return bret;
                    //Jobs.Reverse();
                    //Job_ordering(ref Jobs, I_type_remakcount, e);
                    
                    // MultiJob일 때 사용되는 변수 
                    remakeJobcount = remakecount;
                    I_type_remakeJobcount = I_type_remakcount;
                }
                else
                {
                    bret = true;
                }

                // FS 작업 시 Parents Job만을 진행하기 위해 SrcJobs로 따로 빼내어 진행한다.
                List<pepschedule> SrcJobs = new List<pepschedule>();

                // FS Job은 Multi가 아니더라도 Child Job을 가지고 가야하므로
                // RvMsgList[e.vec.ID].IsMulti를 true로 가져간다.
                // 이후 JobCancel 시 Child Job과 Parents Job을 초기화 할 때 사용
                if (Jobs[0].WORKTYPE == "FSI" || Jobs[0].WORKTYPE == "FSO")
                {
                    RvMsgList[e.vec.ID].IsMulti = true;
                    SrcJobs = Jobs.Where(p => p.BATCHID.Contains("PFS")).ToList();
                }
                else if (Jobs[0].WORKTYPE == "MGI" || Jobs[0].WORKTYPE == "MGO")
                {
                    RvMsgList[e.vec.ID].IsMulti = IsMultiJob;
                    SrcJobs = Jobs;
                }


                RvMsgList[e.vec.ID].MultiList = Jobs;
                foreach (var xxx in SrcJobs)
                {
                    Thread.Sleep(1);
                    isControl_Pause();
                    if (xxx.C_srcFinishTime != null)
                        continue;

                    unit_fs unit_chk = Db.Units_FS.Where(p => p.GOALNAME == xxx.S_EQPID).SingleOrDefault();
                    UpdateVehicleByID(VecId, xxx.BATCHID);
                    RvMsgList[e.vec.ID].CurJob = xxx;

                    bret = OrderToVehicle(JobProcList[e.vec.ID].jobType, xxx, e.peps, e.vec);
                    if (!bret)
                        break;

                    while (true)
                    {
                        bret = Proc_Atom.Init.IsControllerStop(e.vec.ID);
                        if (bret)
                        {
                            bret = !bret;
                            break;                                
                        }
                        else
                        {
                            bret = true;
                        }
                        if (xxx.C_srcFinishTime != null)
                            break;
                        Thread.Sleep(10);
                    }

                    // Multi Job 일 때 사용
                    if (e != null && Jobs.Count > 0 && Jobs[foreachcount].C_dstFinishTime == null && Jobs[foreachcount].MULTIID != null)
                    {
                        if (Jobs[0].MULTIID.Split('_')[0] == "MFSI")
                        {
                            if (SrcMultiJobDeleteCheck_MI(ref Jobs, e, ref I_type_remakeJobcount, ref I_type_foreachcount, ref I_type_remakcount))
                                break;

                        }
                    }

                    // 현재 Job의 BatchID가 PFS를 포함하고 IsMultiJob이 False 일 때
                    if (xxx.BATCHID.Contains("PFS") && !IsMultiJob)
                    {
                        // 현재 Parent Job의 Data를 해당되는 Child Job에 공유 및 저장
                        SrcChildJobUpdate(ref Jobs, e);
                        break;
                    }
                }


                if (!bret)
                    return bret;

                // FSO 작업은 FSSTK에서 Magazine을 Unload하는 SRC 작업 이후
                // Chamber에서 FS를 Magazine으로 Unload하는 SRC 작업을 진행하기에
                // JobProcList[e.vec.ID].jobType를 SRC로 가져간다.
                if (Jobs[0].WORKTYPE == "FSO")
                    JobProcList[e.vec.ID].jobType = "SRC";
                else
                    JobProcList[e.vec.ID].jobType = "DST";

                bret = ProcessDstJob(IsMultiJob, ref Jobs, e);

                if (!bret)
                    return bret;

                // FS 작업 시 Child Job만을 진행하기 위해 DstJobs로 따로 빼내어 진행한다.
                List<pepschedule> DstJobs = new List<pepschedule>();

                if (Jobs[0].WORKTYPE == "FSI" || Jobs[0].WORKTYPE == "FSO")
                {
                    DstJobs = Jobs.Where(p => p.BATCHID.Contains("CFS")).ToList();
                }
                else if (Jobs[0].WORKTYPE == "MGI" || Jobs[0].WORKTYPE == "MGO")
                {
                    DstJobs = Jobs;
                }
                
                int jobcount = 0;
                foreach (var xxx in DstJobs)
                {
                    // 마지막 Child Job을 판단하기 위한 변수
                    jobcount++;
                    if (jobcount == DstJobs.Count())
                        RvMsgList[e.vec.ID].ChamberFinish = true;

                    Thread.Sleep(1);
                    isControl_Pause();

                    if (xxx.C_dstFinishTime != null)
                        continue;
                    // FSO에서 JobAssign까지는 JobProcList[e.vec.ID].jobType를 SRC로 가져가나
                    // From_Chamber, To_Chamber에서 JobProcList[e.vec.ID].jobType를 JobAssign 이후 DST로 변경
                    // 다음 Child Job은 SRC로 시작해야하므로 값 변경
                    if (xxx.WORKTYPE == "FSO")
                        JobProcList[e.vec.ID].jobType = "SRC";

                    UpdateVehicleByID(VecId, xxx.BATCHID);

                    RvMsgList[e.vec.ID].CurJob = xxx;

                    bret = OrderToVehicle(JobProcList[e.vec.ID].jobType, xxx, e.peps, e.vec);
                    if (!bret)
                        break;

                    while (true)
                    {
                        bret = Proc_Atom.Init.IsControllerStop(e.vec.ID);
                        if (bret)
                        {
                            bret = !bret;
                            break;
                        }
                        else
                        {
                            bret = true;
                        }
                        if (xxx.C_dstFinishTime != null)
                            break;

                        Thread.Sleep(10);
                    }

                }
                if (!bret)
                    return bret;

                // FSSTK Return Magazine Job 생성 및 진행부분 작성
                if (Jobs[0].WORKTYPE == "FSI" || Jobs[0].WORKTYPE == "FSO")
                {
                    JobProcList[e.vec.ID].jobType = "DST";

                    // Magazine Return Job을 생성하는 함수
                    bret = ProcessReturnJob(IsMultiJob, ref Jobs, e);

                    foreach (var xxx in Jobs)
                    {
                        Thread.Sleep(1);
                        isControl_Pause();
                        if (xxx.C_dstFinishTime != null)
                            continue;

                        UpdateVehicleByID(VecId, xxx.BATCHID);

                        RvMsgList[e.vec.ID].CurJob = xxx;

                        bret = OrderToVehicle(JobProcList[e.vec.ID].jobType, xxx, e.peps, e.vec, true);
                        if (!bret)
                            break;
                        while (true)
                        {
                            bret = Proc_Atom.Init.IsControllerStop(e.vec.ID);
                            if (bret)
                            {
                                bret = !bret;
                                break;
                            }
                            else
                            {
                                bret = true;
                            }
                            if (xxx.C_dstFinishTime != null)
                                break;

                            Thread.Sleep(10);
                        }
                    }

                    if (!bret)
                        return bret;
                } 

                // 모든 Job 완료 후 초기화
                RobotList[e.vec.ID].JobAssign = true;
                RvMsgList[e.vec.ID].ChamberFrist = false;
                RvMsgList[e.vec.ID].ChamberFinish = false;
                RvMsgList[e.vec.ID].IsMulti = false;
                RvMsgList[e.vec.ID].MultiList = null;
                RvMsgList[e.vec.ID].CurJob = null;
                e.vec.isAssigned = 0;
                
                Db.DbUpdate(TableType.VEHICLE);
                return bret;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void OnMultiSrcFinish(MultiSrcFinishArgs e, List<pepschedule> Jobs, int I_type_remakcount = 0)
        {
            _virtualPep.ZeroMem();
            _virtualPep.Copy(e.pep);

            multiJob_Delete(ref Jobs, I_type_remakcount);            
            
            string[] job_tray_num = stack_tray_check(Jobs);

            foreach (var v in Jobs)
            {
                if (_virtualPep.S_EQPID == v.S_EQPID && !(v.BATCHID.Contains("MSRC")))
                {
                    v.C_VEHICLEID = _virtualPep.C_VEHICLEID;
                    v.C_state = _virtualPep.C_state;
                    v.C_srcStartTime = _virtualPep.C_srcStartTime;
                    v.C_srcArrivingTime = _virtualPep.C_srcArrivingTime;
                    v.C_srcAssignTime = _virtualPep.C_srcAssignTime;
                    v.C_srcFinishTime = _virtualPep.C_srcFinishTime;

                    if (v.C_bufSlot == null || v.C_bufSlot == "")
                    {
                        if (_virtualPep.TRANSFERTYPE == "FS")
                        {
                            v.C_bufSlot = batchJob_tray_data(_virtualPep.TRAYID.Split(','), v.TRAYID.Split(','), _virtualPep.C_bufSlot.Split(','));                            
                        }
                        else if(_virtualPep.TRANSFERTYPE == "STACK")
                        {
                            v.C_bufSlot = batchJob_stack_data(_virtualPep.TRAYID.Split(','), v.TRAYID.Split(','), _virtualPep.C_bufSlot.Split(','), job_tray_num);                            
                        }
                    }
                    else
                    {
                        int emptyslotchk = tray_empty_check(v);

                        if (emptyslotchk != 0)
                        {
                            v.C_bufSlot = batchJob_tray_empty_select(v, v.C_bufSlot.Split(','), emptyslotchk);
                        }
                    }
                }
            }
        }
        public List<pepschedule> SelectMultiList(bool ismulti, eMultiJobWhere eMulti, int worktype, string grplst_EQPID, string ExecuteTime)
        {
            List<pepschedule> Jobs = null;
            if (eMulti == eMultiJobWhere.SRC)
            {
                Jobs = Db.Peps.Where(p => p.REAL_TIME == ExecuteTime &&
                                            p.C_priority == (int)worktype &&
                                            p.C_dstFinishTime == null &&
                                            p.BATCHID.Split('_')[0] != "MSRC" &&
                                            p.C_isChecked == 1 &&
                                            p.S_EQPID == grplst_EQPID).ToList();
            }
            else
            {
                Jobs = Db.Peps.Where(p => p.REAL_TIME == ExecuteTime &&
                                            p.C_priority == (int)worktype &&
                                            p.C_dstFinishTime == null &&
                                            p.C_isChecked == 1 &&
                                            p.T_EQPID == grplst_EQPID).ToList();
            }

            return Jobs;

        }

        public List<pepschedule> SelectMultiList_OI(bool ismulti, eMultiJobWhere eMulti, int worktype, string ExecuteTime)
        {
            List<pepschedule> Jobs = null;
            if (eMulti == eMultiJobWhere.SRC)
            {
                Jobs = Db.Peps.Where(p => p.REAL_TIME == ExecuteTime &&
                                            p.C_priority == (int)worktype &&
                                            p.C_dstFinishTime == null &&
                                            p.BATCHID.Split('_')[0] != "MSRC").ToList();
            }
            else
            {
                Jobs = Db.Peps.Where(p => p.REAL_TIME == ExecuteTime &&
                                            p.C_priority == (int)worktype &&
                                            p.C_dstFinishTime == null).ToList();
            }
            return Jobs;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        private bool ProcessSrcJob(bool IsMultiJob, ref List<pepschedule> Jobs, CallCmdProcArgs1 e, ref int remakecount, ref int I_type_remakcount)
        {
            Thread.Sleep(1);
            try
            {
                List<MulGrp> mulgrplist = new List<MulGrp>();

                // Multi Job 일 때
                if (IsMultiJob)
                {
                    string multiId = string.Format("M{0}_{1}", Jobs[0].WORKTYPE.ToString(), DateTime.Now.ToString("ddHHmmssfff"));
                    foreach (var v in Jobs)
                    {
                        v.MULTIID = multiId;
                    }

                    if (!RobotList[e.vec.ID].IsConnected)
                        return false;

                    if (Jobs[0].WORKTYPE.Contains("FS"))
                    {
                        bool bret = false;
                        List<pepschedule> peps = Jobs.Where(p => p.BATCHID.Contains("PFS")).ToList();

                        mulgrplist = (e.where == eMultiJobWhere.SRC) ? FindSubMultiJob(peps, eMultiJobWhere.DST) :
                                                                                  FindSubMultiJob(peps, eMultiJobWhere.SRC);
                        if (e.where == eMultiJobWhere.SRC)
                        {
                            bret = SRCJobwhereSRCProcces(ref peps, mulgrplist, e, ref remakecount, ref I_type_remakcount);
                        }
                        else
                        {
                            bret = SRCJobwhereDSTProcces(ref peps, mulgrplist, e, ref remakecount, ref I_type_remakcount);
                        }

                        if (bret)
                        {
                            for (int i = 0; i < peps.Count(); i++)
                            {
                                if (Jobs.Where(p => p.BATCHID == peps[i].BATCHID).Count() > 0)
                                {
                                    Jobs.Remove(peps[i]);
                                    Jobs.Add(peps[i]);
                                }
                                else
                                {
                                    Jobs.Add(peps[i]);
                                }
                            }
                        }

                        return bret;
                    }
                    else
                    {
                        mulgrplist = (e.where == eMultiJobWhere.SRC) ? FindSubMultiJob(Jobs, eMultiJobWhere.DST) :
                                                                                  FindSubMultiJob(Jobs, eMultiJobWhere.SRC);

                        if (e.where == eMultiJobWhere.SRC)
                        {
                            return SRCJobwhereSRCProcces(ref Jobs, mulgrplist, e, ref remakecount, ref I_type_remakcount);
                        }
                        else
                        {
                            return SRCJobwhereDSTProcces(ref Jobs, mulgrplist, e, ref remakecount, ref I_type_remakcount);
                        }
                    }
                }         
                // Single Job 일 때
                else
                {
                    SortMultiJobList(ref mulgrplist, e.vec.ID);
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        private bool ProcessDstJob(bool IsMultiJob, ref List<pepschedule> Jobs, CallCmdProcArgs1 e)
        {
            Thread.Sleep(1);
            try
            {
                if (e.vec == null)
                    return false;

                if (!RobotList[e.vec.ID].IsConnected)
                    return false;

                List<MulGrp> mulgrplist = new List<MulGrp>();                

                if (IsMultiJob)
                {
                    int mulgrplist_count = 0;

                    if (e.where == eMultiJobWhere.DST)
                    {
                        return DSTJobwhereSRCProcces(ref Jobs, mulgrplist, e, ref mulgrplist_count);
                    }
                    else
                    {
                        return DSTJobwhereDSTProcces(ref Jobs, mulgrplist, e, ref mulgrplist_count);
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool ProcessReturnJob(bool IsMultiJob, ref List<pepschedule> Jobs, CallCmdProcArgs1 e)
        {
            Thread.Sleep(1);
            try
            {
                if (e.vec == null)
                    return false;

                if (!RobotList[e.vec.ID].IsConnected)
                    return false;

                List<MulGrp> mulgrplist = new List<MulGrp>();

                // ReturnJob 생성 후 JobCancel를 하여 재 진행시
                // ReturnJob으로 ReturnJob을 만드는것을 방지
                pepschedule pep = Jobs.Where(p => p.BATCHID.Contains("PFS")).FirstOrDefault();
                if (pep.BATCHID.Split('_').Count() < 3)
                    MagazineReturnPeps(pep, ref Jobs);
                
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 상대 방향에 Sub 멀티그룹이 있는지 조사
        /// 만약 MultiJobs 이 Src 에서 그룹핑되어 있다고 가정하면, Dst 쪽에서 SubMultiGrp 조사
        /// </summary>
        /// <param name="multijobs">Src 또는 Dst 의 동일 설비명으로 그룹핑된 최상위 MultiJobs 리스트</param>
        /// <param name="where">eMultiJobWhere.SRC 또는 eMultiJobWhere.DST, 그룹핑 대상이 source 설비인지 target 설비인지</param>
        /// <returns> List<MulGrp> </returns>
        private List<MulGrp> FindSubMultiJob(List<pepschedule> multijobs, eMultiJobWhere where)
        {
            List<MulGrp> itemlist
                = multijobs.GroupBy(p => new { PRIORITY = p.C_priority, EQPID = (where == eMultiJobWhere.SRC ? p.S_EQPID : p.T_EQPID) })
                           .Select(group => new MulGrp() { PRIORITY = group.Key.PRIORITY.Value, EQPID = group.Key.EQPID, COUNT = group.Count() })
                           .OrderBy(x => x.PRIORITY).ThenByDescending(x => x.COUNT)
                           .ToList();
            return itemlist;
        }

        private List<pepschedule> FindSubMultiJobList(List<pepschedule> multijobs, MulGrp obj, eMultiJobWhere where, string ExecuteTime)
        {
            if (where == eMultiJobWhere.SRC)
            {
                List<pepschedule> itemlist
                = multijobs.Where(p => p.EXECUTE_TIME == ExecuteTime &&
                                       p.C_priority == obj.PRIORITY &&
                                       obj.EQPID == p.S_EQPID)
                           .OrderBy(p => p.C_priority)
                           .ToList();
                return itemlist;
            }
            else
            {
                List<pepschedule> itemlist
                = multijobs.Where(p => p.EXECUTE_TIME == ExecuteTime &&
                                       p.C_priority == obj.PRIORITY &&
                                       obj.EQPID == p.T_EQPID)
                           .OrderBy(p => p.C_priority)
                           .ToList();
                return itemlist;
            }
        }

        private void SortMultiJobList(ref List<MulGrp> itemlist, string vecid)
        {
            // 현시점 x,y 에서 각 장비들의 위치를 따져서 path를 구하고 eqpGrp 를 ordering 한다
            List<unit_fs> unitlist = new List<unit_fs>();
            foreach (var w in itemlist)
            {
                try
                {
                    unit_fs unt = Db.Units_FS.Where(p => p.GOALNAME == w.EQPID).FirstOrDefault();
                    if (unt == null)
                        continue;
                    unitlist.Add(unt);
                }
                catch (Exception ex)
                {
                    Logger.Inst.Write(vecid, CmdLogType.Rv, $"Exception. ProcessDstJob. {ex.Message}\r\n{ex.StackTrace}");
                }
            }

            List<TempNearestGoalStruct> distlist = GetGoalNameNearestInUnitList(unitlist, RobotList[vecid].Csby.propertyCurrVecStatus.posX, RobotList[vecid].Csby.propertyCurrVecStatus.posY);


            var query = from A in distlist
                        join B in itemlist on A.goalName equals B.EQPID
                        select new MulGrp() { PRIORITY = B.PRIORITY, EQPID = B.EQPID, COUNT = B.COUNT };
            itemlist = query.ToList();
        }

        private void SortMultiJobList1(ref List<MulGrp> itemlist, string eqpid)
        {
            // 현시점 x,y 에서 각 장비들의 위치를 따져서 path를 구하고 eqpGrp 를 ordering 한다
            List<unit_fs> unitlist = new List<unit_fs>();
            foreach (var w in itemlist)
            {
                try
                {
                    unit_fs unt = Db.Units_FS.Where(p => p.GOALNAME == w.EQPID).FirstOrDefault();
                    if (unt == null)
                        continue;
                    unitlist.Add(unt);
                }
                catch (Exception ex)
                {
                    Logger.Inst.Write(CmdLogType.Rv, $"Exception. ProcessDstJob. {ex.Message}\r\n{ex.StackTrace}");
                }
            }

            unit_fs unt1 = Db.Units_FS.Where(p => p.GOALNAME == eqpid).FirstOrDefault();
            if (unt1 == null)
                return;

            List<TempNearestGoalStruct> distlist = GetGoalNameNearestInUnitList(unitlist, (float)unt1.loc_x, (float)unt1.loc_y);


            var query = from A in distlist
                        join B in itemlist on A.goalName equals B.EQPID
                        select new MulGrp() { PRIORITY = B.PRIORITY, EQPID = B.EQPID, COUNT = B.COUNT };
            itemlist = query.ToList();
        }

        private List<TempNearestGoalStruct> GetGoalNameNearestInUnitList(List<unit_fs> goalList, float x, float y)
        {
            var resArry = new TempNearestGoalStruct[goalList.Count];
            Parallel.For(0, goalList.Count, i =>
            {
                resArry[i] = new TempNearestGoalStruct() { goalDist = GetDistanceBetweenPoints(new PointF(x, y), new PointF((float)goalList[i].loc_x, (float)goalList[i].loc_y)), goalName = goalList[i].GOALNAME };
            });

            return resArry.OrderBy(p => p.goalDist).ToList();
        }

        private float GetDistanceBetweenPoints(PointF a, PointF b)
        {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }


        /// <summary>
        /// 택 타임을 고려하여 최대 10개씩 모아서 처리할 수 있도록 JOB 을 통합, 재생성한다.
        /// </summary>
        /// <param name="multijobs"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        /// <note>입력 받은 job schedule 과 새롭게 생성한 스케쥴을 어떻게 처리할 것인가???</note>
        /// <note>1. 기존 job schedule 은 시간 update 하고 history 테이블로 옮긴다.</note>
        /// <note>1.1. 새로 생성한 스케쥴은 리스트</note>
        private bool RemakeMultiJobs(ref List<pepschedule> multijobs, int where, ref int mulgrplist_count, bool bDelete = false, bool tray_over = false)
        {
            pepschedule pep = new pepschedule();
            PepsDeepCopy(pep, multijobs[0]);

            List<pepschedule> newmultijobs = new List<pepschedule>();
            JobRemakeWords jobrewords = new JobRemakeWords();

            multijobs = multijobs.OrderBy(p => p.ORDER).ToList();

            if (multijobs.Count > 1 || tray_over == true)
            {                
                multijobs = Job_remake(where, ref mulgrplist_count, tray_over, pep, ref multijobs, bDelete, jobrewords);
            }
            Thread.Sleep(2000);
            return true;
        }
        private bool OrderToVehicle(string cmd, pepschedule job, List<pepschedule> peps, vehicle vec, bool return_check = false)
        {
            bool bret = false;
            vehicle _vec = vec;
            if (cmd == "DST")
            {
                List<vehicle> list = Db.Vechicles.Where(p => p.ID == job.C_VEHICLEID && p.isUse == 1).ToList();
                Debug.Assert(list.Count() != 0);
                // SRC job 에 사용되었던 vehicle 을 재할당
                _vec = list[0];
            }

            Thread.Sleep(1);

            int traycount = 0;
            string eqp_portslot = string.Empty;
            string buf_portslot = string.Empty;
            List<unit_fs> units = new List<unit_fs>();

            // 설비 PortSlot을 Robot에 전송될 Data로 가공
            if (!MakePortSlot(cmd, job, _vec, ref units, ref eqp_portslot, ref traycount)) return false;
            // Robot에 사용될 BuffSlot이 있는지 확인 및 Data 가공
            if (!MakeBuffSlot(cmd, job, _vec, ref buf_portslot, traycount)) return false;

            var args = new SendJobToVecArgs1() { vecID = _vec.ID, job = job, joblist = peps, vec = _vec, eqp = units[0] };
            args.cmd = cmd;
            args.eqpslot = eqp_portslot;
            args.bufslot = buf_portslot;
            args.return_check = return_check;
            args.fsstk_send = false;
            bret = OnSendJobToVehicle(args);

            if (!bret)
                return bret;
            int cnt = 0;
            DateTime dtold = DateTime.Now;
            return bret;
        }

        private void isControl_Pause()
        {
            var controller = Db.Controllers.SingleOrDefault();
            while (controller.C_state == (int)ControllerState.PAUSED)
            {
                continue;
            }
        }
        private void PepsDeepCopy(pepschedule dst, pepschedule src)
        {
            dst.ID = src.ID;
            dst.MULTIID = src.MULTIID;
            dst.BATCHID = src.BATCHID;
            dst.S_EQPID = src.S_EQPID;
            dst.S_PORT = src.S_PORT;
            dst.S_SLOT = src.S_SLOT;
            dst.T_EQPID = src.T_EQPID;
            dst.T_PORT = src.T_PORT;
            dst.T_SLOT = src.T_SLOT;
            dst.C_mgtype = src.C_mgtype;
            dst.TRAYID = src.TRAYID;
            dst.WORKTYPE = src.WORKTYPE;
            dst.TRANSFERTYPE = src.TRANSFERTYPE;
            dst.WINDOW_TIME = src.WINDOW_TIME;
            dst.EXECUTE_TIME = src.EXECUTE_TIME;
            dst.REAL_TIME = src.REAL_TIME;
            dst.STATUS = src.STATUS;
            dst.LOT_NO = src.LOT_NO;
            dst.QTY = src.QTY;
            dst.STEPID = src.STEPID;
            dst.S_STEPID = src.S_STEPID;
            dst.T_STEPID = src.T_STEPID;
            dst.URGENCY = src.URGENCY;
            dst.FLOW_STATUS = src.FLOW_STATUS;
            dst.C_VEHICLEID = src.C_VEHICLEID;
            dst.C_bufSlot = src.C_bufSlot;
            dst.C_state = src.C_state;
            dst.C_srcAssignTime = src.C_srcAssignTime;
            dst.C_srcArrivingTime = src.C_srcArrivingTime;
            dst.C_srcStartTime = src.C_srcStartTime;
            dst.C_srcFinishTime = src.C_srcFinishTime;
            dst.C_dstAssignTime = src.C_dstAssignTime;
            dst.C_dstArrivingTime = src.C_dstArrivingTime;
            dst.C_dstStartTime = src.C_dstStartTime;
            dst.C_dstFinishTime = src.C_dstFinishTime;
            dst.C_isChecked = src.C_isChecked;
            dst.C_priority = src.C_priority;
            dst.DOWNTEMP = src.DOWNTEMP;
            dst.EVENT_DATE = src.EVENT_DATE;
            dst.ORDER = src.ORDER;
        }

        private void StringAdd(ref string total, string trayids, char delimeter)
        {
            if (total.Length == 0)
            {
                if (!string.IsNullOrEmpty(trayids))
                    total = trayids;
            }
            else
            {
                if (total[total.Length - 1] == delimeter)
                {
                    if (!string.IsNullOrEmpty(trayids))
                    {
                        total += trayids;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(trayids))
                        total += string.Format("{0}{1}", delimeter, trayids);
                    else
                        total += string.Format("{0}{1}", delimeter, "");
                }
            }
        }

        private string makeportslot(string slot, ref int traycount, int goaltype)
        {
            string eqp_portslot = string.Empty;

            // slot에 있는 설비 PortSlot 이름을 ','로 Split
            string[] words = slot.Split(',');
            traycount = words.Count();
            int portNo = -1;
            int slotNo = -1;
            for (int n = 0; n < words.Count(); n++)
            {
                // 설비 PortSlot 이름을 PortNo, SlotNo으로 가공
                (portNo, slotNo) = makeportslotNo(words[n], goaltype);

                if (portNo < 0 || slotNo < 0)
                    return string.Empty;

                // 가공된 Data를 Robot에 사용하는 포멧으로 변경
                make_eqpportslot(ref eqp_portslot, portNo, slotNo);
            }

            return eqp_portslot;
        }
        private string makeportslot_chamber_fs(string cmd, pepschedule job, ref int traycount, int goaltype)
        {
            string childs_slot = string.Empty;
            string eqp_portslot = string.Empty;
            string all_eqp_portslot = string.Empty;

            // FS 작업 시 Child Job에 있는 Data를 사용해야하므로 DB에서 해당 Parent Job에 연관된 Child Job을 가져온다.
            string batchid = string.Format($"C{job.WORKTYPE}_{job.BATCHID.Split('_')[1]}");
            var peps = Db.Peps.Where(p => p.BATCHID.Contains(batchid) && p.REAL_TIME == job.REAL_TIME).ToList();

            foreach (var x in peps)
            {
                // 설비 PortSlot Data를 저장
                if (cmd == "SRC")
                    childs_slot = x.S_SLOT;
                else
                    childs_slot = x.T_SLOT;

                // 설비 PortSlot Data를 ','로 Split
                string[] words = childs_slot.Split(',');
                traycount = words.Count();

                int portNo = -1;
                int slotNo = -1;
                for (int n = 0; n < words.Count(); n++)
                {
                    // 설비 PortSlot 이름을 PortNo, SlotNo으로 가공
                    (portNo, slotNo) = makeportslotNo(words[n], goaltype);

                    if (portNo < 0 || slotNo < 0)
                        return string.Empty;

                    // 가공된 Data를 Robot에 사용하는 포멧으로 변경
                    make_eqpportslot(ref eqp_portslot, portNo, slotNo);
                }

                // 만들어진 Data를 DB에 저장
                if (cmd == "SRC")
                    x.S_PORT = eqp_portslot;
                else
                    x.T_PORT = eqp_portslot;

                // Child Job에서 만들어진 PortSlot Data를 누적
                if (all_eqp_portslot.Length > 0)
                    all_eqp_portslot += ",";
                all_eqp_portslot += eqp_portslot;
                eqp_portslot = string.Empty;
            }
            return all_eqp_portslot;
        }

        private string makeportslot_SYSWIN(string slot, ref int traycount, int goaltype, string eqp)
        {
            string eqp_portslot = string.Empty;

            // slot에 있는 설비 PortSlot 이름을 ','로 Split
            string[] words = slot.Split(',');
            traycount = words.Count();
            int portNo = -1;
            int slotNo = -1;
            for (int n = 0; n < words.Count(); n++)
            {
                // 설비 PortSlot 이름을 PortNo, SlotNo으로 가공
                (portNo, slotNo) = makeportslotNo_SYSWIN(words[n], goaltype, eqp);

                if (portNo < 0 || slotNo < 0)
                    return string.Empty;

                // 가공된 Data를 Robot에 사용하는 포멧으로 변경
                make_eqpportslot(ref eqp_portslot, portNo, slotNo);
            }

            return eqp_portslot;
        }

        private string makeportslot_magazine(string slot)
        {
            string eqp_portslot = string.Empty;

            // FS 위치 Data를 ','로 Split
            string[] words = slot.Split(',');
            // PortNo은 1로 고정
            int portNo = 1;
            int slotNo = -1;
            for (int n = 0; n < words.Count(); n++)
            {
                // SlotNo은 상위에서 내려준 FS 위치 이름에서 숫자만 사용
                slotNo = Convert.ToInt32(words[n].Substring(2, words[n].Length - 2));

                if (slotNo < 0)
                    return string.Empty;

                // PortNo은 한자리로 SlotNo은 세자리로 하여 가공
                if (eqp_portslot.Length > 0)
                    eqp_portslot += ",";
                eqp_portslot += portNo.ToString();
                eqp_portslot += ",";
                eqp_portslot += slotNo.ToString("D3");
            }

            return eqp_portslot;
        }
        private void makeportslot_cancel(pepschedule job, string eqp, string msg, CmdState states)
        {
            Logger.Inst.Write(CmdLogType.Db, msg);
            job.C_srcAssignTime = DateTime.Now;
            job.C_dstAssignTime = DateTime.Now;
            job.C_state = (int)states;
            Db.DbUpdate(TableType.PEPSCHEDULE);
            Db.CopyCmdToHistory(job.ID.ToString());
            Db.Delete(job);
        }
        private bool MakePortSlot(string cmd, pepschedule job, vehicle vec, ref List<unit_fs> units, ref string eqp_portslot, ref int traycount)
        {
            string eqp = (cmd == "DST") ? job.T_EQPID : job.S_EQPID;
            string slot = (cmd == "DST") ? job.T_SLOT : job.S_SLOT;

            try
            {
                // 해당 설비가 DB에 등록되어있는지 확인
                units = Db.Units_FS.Where(p => p.ID == eqp).ToList();
                if (units == null || units.Count() == 0)
                {   throw new UtilMgrCustomException($"SendMissionToVehicle. Invalid EQNAME = {eqp}");
                }
                
                // 설비가 FSSTK 또는 Chamber이고 Worktype이 MGI 또는 MGO 일 때
                if ((units[0].goaltype == (int)EqpGoalType.FSSTK) || (units[0].goaltype == (int)EqpGoalType.RDT && (job.WORKTYPE == "MGI" || job.WORKTYPE == "MGO")))
                {
                    // 설비 PortSlot을 Robot에 보낼 Data로 가공
                    eqp_portslot = makeportslot(slot, ref traycount, units[0].goaltype);
                    // PortSlot이 제대로 작성되지않으면 Error
                    portslot_error_msg(eqp_portslot, units[0], slot);
                    // 만들어진 설비의 PortSlot Data를 DB에 저장
                    if (cmd == "SRC")
                        job.S_PORT = eqp_portslot;
                    else if (cmd == "DST")
                        job.T_PORT = eqp_portslot;
                }
                // 설비가 Chamber이고 Worktype이 FSI 또는 FSO 일 때
                else if ((units[0].goaltype == (int)EqpGoalType.RDT || units[0].goaltype == (int)EqpGoalType.AGING)&& (job.WORKTYPE == "FSI" || job.WORKTYPE == "FSO"))
                {
                    // 설비 PortSlot을 Robot에 보낼 Data로 가공
                    eqp_portslot = makeportslot_chamber_fs(cmd, job, ref traycount, units[0].goaltype);
                    // PortSlot이 제대로 작성되지않으면 Error
                    portslot_error_msg(eqp_portslot, units[0], slot);
                }
                else if (units[0].goaltype == (int)EqpGoalType.SYSWIN)
                {
                    // 설비 PortSlot을 Robot에 보낼 Data로 가공
                    eqp_portslot = makeportslot_SYSWIN(slot, ref traycount, units[0].goaltype, eqp);
                    // PortSlot이 제대로 작성되지않으면 Error
                    portslot_error_msg(eqp_portslot, units[0], slot);
                    // 만들어진 설비의 PortSlot Data를 DB에 저장
                    if (cmd == "SRC")
                        job.S_PORT = eqp_portslot;
                    else if (cmd == "DST")
                        job.T_PORT = eqp_portslot;
                }
                else
                {
                    eqp_portslot = string.Empty;
                    throw new UtilMgrCustomException($"makeportslot. Invalid goaltype = {units[0].goaltype}");
                }
            }
            catch (UtilMgrCustomException ex)
            {
                // 작성시 Exception 발생하면 Error 처리
                Logger.Inst.Write(vec.ID, CmdLogType.Rv, $"Exception. MakePortSlot. {ex.Message}\r\n{ex.StackTrace}");
                makeportslot_cancel(job, eqp, ex.Message, CmdState.INVALID_SLOT);
                return false;
            }
            return true;
        }
        private bool MakeBuffSlot(string cmd, pepschedule job, vehicle vec, ref string buf_portslot, int traycount)
        {
            // 현재 진행중인 Job이 DST 이거나 SRC이면서 CFSO 인 Job이면 Job에 있는 Data를 사용
            if (cmd == "DST" || (cmd == "SRC" && job.BATCHID.Contains("CFSO")))
            {
                buf_portslot = job.C_bufSlot;
            }
            // 그 외에는 Data 가공
            else
            {
                int mg_traycount = 0;
                // Magazine Type이 UFS면 Traycount는 그대로 사용
                // Magazine Type이 UFS가 아니면 Traycount는 2배로하여 사용
                // UFS Type은 Buffslot을 1칸만 사용하나 그 외 Type은 2칸을 사용
                if (job.C_mgtype != "UFS")
                    mg_traycount = traycount * 2;
                else
                    mg_traycount = traycount;

                // Robot의 비어있는 BuffSlot을 Port 3 -> 2 -> 1 순으로 확인하며 mg_traycount 수 만큼 있는지 확인하여 vecParts에 저장
                var vecParts = Db.VecParts.Where(p => p.state == (int)VehiclePartState.ENABLE && (p.C_trayId == null || p.C_trayId == "") && p.VEHICLEID == vec.ID)
                                               .OrderBy(p => p.portNo == 1)
                                               .OrderBy(p => p.portNo == 2)
                                               .OrderBy(p => p.portNo == 3)
                                               .Take(mg_traycount);

                if (vecParts == null)
                {
                    Logger.Inst.Write(vec.ID, CmdLogType.Db, "차량 Buffer 에 여유공간이 없습니다.");
                    return false;
                }

                // 해당된 BuffSlot의 Data를 Robot에 보낼 포멧으로 가공
                job.C_bufSlot = bufslot_data(ref buf_portslot, vecParts, job);

                // FS 작업 시 Child Job의 Magazine 내부의 FS 위치 Data를 미리 가공
                if (job.WORKTYPE == "FSI" || job.WORKTYPE == "FSO")
                {
                    // Parent Job의 Magazine을 Split
                    string[] magazineids = job.TRAYID.Split(',');
                    // 위에서 가공한 BuffSlot Data를 Split
                    string[] bufslot_split = buf_portslot.Split(',');
                    // BuffSlot을 개별로 저장하기 위한 변수
                    int count = 0;
                    
                    foreach (var x in magazineids)
                    {
                        // Parent Job에 해당되는 Child Job을 찾기위한 BatchID 선정
                        string batchid = string.Format($"C{job.WORKTYPE}_{job.BATCHID.Split('_')[1]}");
                        pepschedule pep = new pepschedule();

                        // Child Job BatchID와 해당 Magazine ID를 가진 Child Job을 가져온다
                        if (job.WORKTYPE == "FSI")
                            pep = Db.Peps.Where(p => p.BATCHID.Contains(batchid) && p.S_EQPID == x && p.REAL_TIME == job.REAL_TIME).SingleOrDefault();
                        else if (job.WORKTYPE == "FSO")
                            pep = Db.Peps.Where(p => p.BATCHID.Contains(batchid) && p.T_EQPID == x && p.REAL_TIME == job.REAL_TIME).SingleOrDefault();

                        if (pep != null)
                        {
                            // Worktype에 맞추어 FS 위치 Data 저장
                            string slot = (job.WORKTYPE == "FSI") ? pep.S_SLOT : pep.T_SLOT;

                            // Magazine Type이 DB에 저장된 Type인지 확인
                            mg_type mgtype = Db.Mg_Types.Where(p => p.ID == pep.C_mgtype).SingleOrDefault();
                            if (mgtype != null)
                            {
                                // FS 위치 Data 를 Robot에 보낼 포멧으로 가공
                                string bufslot = makeportslot_magazine(slot);

                                // 가공된 FS 위치 Data를 DB에 저장
                                if (job.WORKTYPE == "FSI")
                                    pep.S_PORT = bufslot;
                                else
                                    pep.T_PORT = bufslot;

                                // Magazine의 Robot Buffslot 위치를 Parent Job에서 가공하여 Child Job에 저장 
                                pep.C_bufSlot = string.Format($"{bufslot_split[count++]},{bufslot_split[count++]}");
                            }
                        }
                        
                    }
                }
                Logger.Inst.Write(vec.ID, CmdLogType.All, $"buf_portslot:{buf_portslot}");
            }
            return true;
        }

    }

    public class JobRemakeWords : EventArgs
    {
        public string[] wordTrayIds = null;
        public string[] wordLotNos = null;
        public string[] wordQtys = null;
        public string[] wordSSlot = null;
        public string[] wordTSlot = null;
        public string[] wordStepIds = null;
        public string[] wordSStepIds = null;
        public string[] wordTStepIds = null;
        public string[] wordbuffslot = null;
        public string[] wordteqpid = null;
        public string[] wordmgtype = null;

        public string submultiId = string.Empty;
        public string lotNos = string.Empty;
        public string trayIds = string.Empty;
        public string Qtys = string.Empty;
        public string sslot = string.Empty;
        public string tslot = string.Empty;
        public string stepids = string.Empty;
        public string sstepids = string.Empty;
        public string tstepids = string.Empty;
        public string teqpid = string.Empty;
        public string bufslot = string.Empty;
        public string mgtype = string.Empty;

        public unit_fs chkunit_src = null;
        public unit_fs chkunit_dst = null;
    }

    public class VertualPep : pepschedule
    {
        public void Copy(pepschedule v)
        {
            this.ID = v.ID;
            this.MULTIID = v.MULTIID;
            this.BATCHID = v.BATCHID;
            this.S_EQPID = v.S_EQPID;
            this.S_PORT = v.S_PORT;
            this.S_SLOT = v.S_SLOT;
            this.T_EQPID = v.T_EQPID;
            this.T_PORT = v.T_PORT;
            this.T_SLOT = v.T_SLOT;
            this.C_mgtype = v.C_mgtype;
            this.TRAYID = v.TRAYID;
            this.WORKTYPE = v.WORKTYPE;
            this.TRANSFERTYPE = v.TRANSFERTYPE;
            this.WINDOW_TIME = v.WINDOW_TIME;
            this.EXECUTE_TIME = v.EXECUTE_TIME;
            this.REAL_TIME = v.REAL_TIME;
            this.STATUS = v.STATUS;
            this.LOT_NO = v.LOT_NO;
            this.QTY = v.QTY;
            this.STEPID = v.STEPID;
            this.S_STEPID = v.S_STEPID;
            this.T_STEPID = v.T_STEPID;
            this.URGENCY = v.URGENCY;
            this.FLOW_STATUS = v.FLOW_STATUS;
            this.C_VEHICLEID = v.C_VEHICLEID;
            this.C_bufSlot = v.C_bufSlot;
            this.C_state = v.C_state;
            this.C_srcAssignTime = v.C_srcAssignTime;
            this.C_srcArrivingTime = v.C_srcArrivingTime;
            this.C_srcStartTime = v.C_srcStartTime;
            this.C_srcFinishTime = v.C_srcFinishTime;
            this.C_dstAssignTime = v.C_dstAssignTime;
            this.C_dstArrivingTime = v.C_dstArrivingTime;
            this.C_dstStartTime = v.C_dstStartTime;
            this.C_dstFinishTime = v.C_dstFinishTime;
            this.C_isChecked = v.C_isChecked;
            this.C_priority = v.C_priority;
            this.DOWNTEMP = v.DOWNTEMP;
            this.EVENT_DATE = v.EVENT_DATE;
            this.ORDER = v.ORDER;
        }
        public void ZeroMem()
        {
            Copy(new pepschedule());
        }
    }
}
