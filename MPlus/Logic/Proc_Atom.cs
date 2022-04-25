using FSMPlus.Ref;
using FSMPlus.Vehicles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

namespace FSMPlus.Logic
{
    public partial class Proc_Atom : Global
    {
        #region singleton POA
        private static volatile Proc_Atom instance;
        private static object syncVsp = new object();
        public static Proc_Atom Init
        {
            get
            {
                if (instance == null)
                {
                    lock (syncVsp)
                    {
                        instance = new Proc_Atom();
                    }
                }
                return instance;
            }
        }
        #endregion

        public bool PROC_ATOM(SendJobToVecArgs1 e, unit_fs srcUnit, unit_fs dstUnit, ProcStep eStep)
        {
            bool bret = false;
            string[] words = e.job.TRAYID.Split(',');   // TRAY COUNT 계산
            int tray_count = words.Count();
            string sendMsg = string.Empty;
            switch (eStep)
            {
                case ProcStep.Pre_Assign:
                    {
                        job_pre_assign(e);          // src job 에서만 발생시킬 것이다.
                    }
                    break;
                case ProcStep.Chk_EqStatus:
                    {                               // 하나의 루틴에서 설비 상태를 체크한다. Retry 시도를 포함하고 있는 이유
                        bret = Chk_EqStatus(e, srcUnit, dstUnit);
                    }
                    break;
                case ProcStep.FSLoadInfoSet:
                    {                               // FSI, MGI 작업의 경우 FSSTK 작업전 설비에 미리 LoadInfoSet을 전송한다.
                        bret = FSLoadInfoSet(dstUnit.goaltype, e);
                    }
                    break;
                case ProcStep.FSUnLoadMove:
                    {                               // FSO, MGO 작업 시 FSSTK와 설비 둘다 Unload Message 전송한다.
                                                    // FSSTK 전송유무를 판단하여 Unload Message 전송
                                                    // 해당 설비로 이동하겠다는 Message를 TC로 전송
                        if (e.fsstk_send)
                            bret = EQFSUnloadMove(dstUnit.goaltype, e);
                        else
                            bret = EQFSUnloadMove(srcUnit.goaltype, e);
                    }
                    break;
                case ProcStep.FSUnLoadStandby:
                    {                               // FSO, MGO 작업 시 FSSTK와 설비 둘다 Unload Message 전송한다.
                                                    // FSSTK 전송유무를 판단하여 Unload Message 전송
                                                    // 해당 설비에 작업 준비 요청 (PIO Ready)
                        if (e.fsstk_send)
                            bret = EQFSUnLoadStandby(dstUnit.goaltype, e);
                        else
                            bret = EQFSUnLoadStandby(srcUnit.goaltype, e);
                    }
                    break;
                case ProcStep.FSUnLoadInfo:
                    {                               // FSO, MGO 작업 시 FSSTK와 설비 둘다 Unload Message 전송한다.
                                                    // FSSTK 전송유무를 판단하여 Unload Message 전송
                                                    // 해당 설비에 작업 진행 알림
                        if (e.fsstk_send)
                            bret = EQFSUnLoadInfo(dstUnit.goaltype, e);
                        else
                            bret = EQFSUnLoadInfo(srcUnit.goaltype, e);
                    }
                    break;
                case ProcStep.FSLoadMove:
                    {                               // 설비 Load 작업
                                                    // 해당 설비로 이동하겠다는 Message를 TC로 전송 
                        bret = EQFSLoadMove(dstUnit.goaltype, e);
                    }
                    break;
                case ProcStep.FSLoadStandby:
                    {                               // 설비 Load 작업
                                                    // 해당 설비에 작업 준비 요청(PIO Ready)
                        bret = EQFSLoadStandby(dstUnit.goaltype, e);
                    }
                    break;
                case ProcStep.FSLoadInfo:
                    {                               // 설비 Load 작업
                                                    // 해당 설비에 작업 진행 알림
                        bret = EQFSLoadInfo(dstUnit.goaltype, e);
                    }
                    break;
                case ProcStep.Req_EqTempDown:
                    {                               // 설비 TempDown 요청
                        bret = CHAMBER_TempDownSingleRequest(e.job.BATCHID, srcUnit, dstUnit, e.vecID);
                    }
                    break;
                case ProcStep.FSJobComplete:
                    {                               // 현재 진행중인 모든 Job 완료시 전송
                        bret = EQFSJOBCOMPCHECK(e);
                    }
                    break;
                case ProcStep.FSMGReturnInfoReq:
                    {                               // 작업 완료된 MAGAZINE을 FSSTK로 돌려놓기위해 정보 전송
                        bret = EQFSMGReturnInfoReq(srcUnit.goaltype, e);
                    }
                    break;
                case ProcStep.FSMGReturnComp:
                    {                               // 작업 완료된 MAGAZINE을 FSSTK로 Load 완료
                        bret = EQFSMGReturnComp(srcUnit.goaltype, e);
                    }
                    break;
                case ProcStep.Wait_FSLoadInfoSet:
                    {                               // LoadInfoSet에 대한 Rep 대기
                        bret = WaitRvMessage(e.vecID);
                    }
                    break;
                case ProcStep.Wait_FSLoadStandby:
                    {                               // FSLoadStandby에 대한 Rep 대기
                        bret = WaitRvMessage(e.vecID);
                    }
                    break;
                case ProcStep.Wait_DoorOpened:
                    {                               // DoorOpened에 대한 Rep 대기
                        bret = WaitRvMessage(e.vecID);
                    }
                    break;
                case ProcStep.Wait_FSLoadInfo:
                    {                               // FSLoadInfo에 대한 Rep 대기
                        bret = WaitRvMessage(e.vecID);
                    }
                    break;
                case ProcStep.Wait_MGLoadComp:
                    {                               // FSLoadComp에 대한 Rep 대기
                        bret = WaitRvMessage(e.vecID);
                    }
                    break;
                case ProcStep.Wait_FSUnloadStandby:
                    {                               // FSUnloadStandby에 대한 Rep 대기
                        bret = WaitRvMessage(e.vecID);
                    }
                    break;
                case ProcStep.Wait_FSUnloadInfo:
                    {                               // FSUnloadInfo에 대한 Rep 대기
                        bret = WaitRvMessage(e.vecID);
                    }
                    break;
                case ProcStep.Wait_MGUnloadComp:
                    {                               // FSUnloadComp에 대한 Rep 대기
                        bret = WaitRvMessage(e.vecID);
                    }
                    break;
                case ProcStep.Wait_FSMGReturnInfo:
                    {                               // FSMGReturnInfo에 대한 Rep 대기
                        bret = WaitRvMessage(e.vecID);
                    }
                    break;
                case ProcStep.Wait_FSMGReturnComp:
                    {                               // FSMGReturnComp에 대한 Rep 대기
                        bret = WaitRvMessage(e.vecID);
                    }
                    break;
                case ProcStep.Wait_MRBPMagazine_Start:
                    {                               // MRBPMagazine에 대한 Rep 대기
                        bret = WaitToVehicle_MRBP_Start(RobotList[e.vecID]);
                    }
                    break;
                case ProcStep.Wait_MRBPMagazine_End:
                    {                               // MRBPMagazine에 대한 Rep 대기
                        bret = WaitToVehicle(RobotList[e.vecID]);
                    }
                    break;
                case ProcStep.Wait_Job_Start:
                    {                               // Job_Start에 대한 Rep 대기
                        bret = WaitToVehicle(RobotList[e.vecID]);
                    }
                    break;
                case ProcStep.Wait_TransComp:
                    bool fakebret = false;
                    bret = WaitVehicleJobCompMessage(ref fakebret, e.vecID);
                    break;
                case ProcStep.Job_Assign:
                    {                               // Mobile Robot에 진행할 Job의 Data 전송
                        bret = job_assign(e);
                    }
                    break;
                case ProcStep.Go_Mobile:
                    {                               // Mobile Robot에 진행할 Job의 설비로 이동 명령
                                                    // Mobile Robot이 도착 후 GO_End를 보낼때까지 대기
                        bret = Go_End(e);
                        if (!bret)
                            break;
                        bret = WaitToVehicle(RobotList[e.vecID]);
                    }
                    break;
                case ProcStep.FSSTK_Start:
                    {                               // Mobile Robot이 설비에 도착 후 Job 진행 명령
                        RobotList[e.vecID].FSSTK_Start();
                        bret = true;
                    }
                    break;
                case ProcStep.Job_Start:
                    {                               // Mobile Robot이 설비에 도착 후 Job 진행 명령
                        RobotList[e.vecID].JobStart();
                        bret = true;
                    }
                    break;
                case ProcStep.FS_Move_Start:
                    {                               // FS Unload/Load Job일 때 MAGAZINE 위치 변경 후 진행 명령                                                    
                        RobotList[e.vecID].FS_MOVE_START();
                        bret = true;
                    }
                    break;
                case ProcStep.Job_Cancel:
                    {                               // 설비, Mobile Robot에 이상문제로 Job을 진행할 수 없을때 또는 재진행이 필요할때
                                                    // 할당된 Job을 취소
                        bret = JOBCANCEL(e);
                        Job_Cancel(e, bret);
                    }
                    break;
                case ProcStep.Err_JobAssign:
                    {
                        e.job.C_VEHICLEID = null;
                        if (e.job.C_state < 8 || e.job.C_state == 28)
                            e.job.C_state = (int)CmdState.SRC_NOT_ASSIGN;    // 우선순위를 밀어서 ReOrdering 이 필요하겠다
                        else
                            e.job.C_state = (int)CmdState.DST_NOT_ASSIGN;
                        Db.DbUpdate(TableType.PEPSCHEDULE);
                        Db.DbUpdate(false);
                        Logger.Inst.Write(e.vecID, CmdLogType.Comm, $"차량[{e.vecID}]이 명령[{e.cmd};{e.job.BATCHID};]을 수신하지 못했습니다.");
                        bret = false;
                    }
                    break;
                case ProcStep.Err_EqStatus:
                    if (Cfg.Data.UseRv)
                        Global.Init.RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_EqStatus: MoveCheck Fail");
                    bret = false;
                    break;
                case ProcStep.Err_EqTempDown:
                    //Global.Init.RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_EqTempDown: TempDown Fail");
                    bret = false;
                    break;
                case ProcStep.Err_FSLoadInfoSet:
                    //Global.Init.RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_FSLoadInfoSet: FSLoadInfoSet Fail");
                    bret = false;
                    break;
                case ProcStep.Err_FSLoadMove:
                    //Global.Init.RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_FSLoadMove: FSLoadInfoMove Fail");
                    bret = false;
                    break;
                case ProcStep.Err_FSLoadStandby:
                    //Global.Init.RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_FSLoadStandby: FSLoadStandby Fail");
                    bret = false;
                    break;
                case ProcStep.Err_FSLoadInfo:
                    //Global.Init.RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_FSLoadInfo: FSLoadInfo Fail");
                    bret = false;
                    break;
                case ProcStep.Err_FSUnLoadMove:
                    //Global.Init.RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_FSUnLoadMove: FSUnLoadMove Fail");
                    bret = false;
                    break;
                case ProcStep.Err_FSUnLoadStandby:
                    //Global.Init.RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_FSUnLoadStandby: FSUnLoadStandby Fail");
                    bret = false;
                    break;
                case ProcStep.Err_FSUnLoadInfo:
                    //Global.Init.RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_FSUnLoadInfo: FSUnLoadInfo Fail");
                    bret = false;
                    break;
                case ProcStep.Err_FSMGReturnInfoReq:
                    //Global.Init.RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_FSMGReturnInfoReq: FSMGReturnInfoReq Fail");
                    bret = false;
                    break;
                case ProcStep.Err_FSMGReturnComp:
                    //Global.Init.RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_FSMGReturnComp: FSMGReturnComp Fail");
                    bret = false;
                    break;
                case ProcStep.Err_Go_Mobile:
                    if (Cfg.Data.UseRv)
                        Global.Init.RvComu.MRSM_Send(e.Fail_unit, "", "", "VEHICLE_GO_TIMEOUT");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_Go_Mobile: GO_END Timeout");
                    bret = false;
                    break;
                case ProcStep.Err_Job_Start:
                    if (Cfg.Data.UseRv)
                        Global.Init.RvComu.MRSM_Send(e.Fail_unit, "", "", "VEHICLE_JOBSTART_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_Job_Start: JobStart Fail");
                    bret = false;
                    break;
                case ProcStep.Err_Wait_Job_Start:
                    //Global.Init.RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_Wait_Job_Start: VEHICLE_JOBSTART_FAIL");
                    bret = false;
                    break;

                case ProcStep.Err_Wait_FSLoadInfoSet:
                    //RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_Wait_FSLoadInfoSet: FSLoadComplete_Rep Fail");
                    bret = false;
                    break;
                case ProcStep.Err_Wait_FSLoadStandby:
                    //RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_Wait_FSLoadStandby: FSLoadStandby_Rep Fail");
                    bret = false;
                    break;
                case ProcStep.Err_Wait_DoorOpened:
                    //RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_Wait_DoorOpened: DoorOpened Fail");
                    bret = false;
                    break;
                case ProcStep.Err_Wait_FSLoadInfo:
                    //RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_Wait_FSLoadInfo: FSLoadInfo_Rep Fail");
                    bret = false;
                    break;
                case ProcStep.Err_Wait_MGLoadComplete:
                    //RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_Wait_FSLoadComplete: FSLoadComplete Fail");
                    bret = false;
                    break;
                case ProcStep.Err_Wait_FSUnloadStandby:
                    //RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_Wait_FSUnloadStandby: FSUnLoadStandby_Rep Fail");
                    bret = false;
                    break;
                case ProcStep.Err_Wait_FSUnloadInfo:
                    //RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_Wait_FSUnloadInfo: FSUnLoadInfo_Rep Fail");
                    bret = false;
                    break;
                case ProcStep.Err_Wait_MGUnLoadComplete:
                    //RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_Wait_FSUnLoadComplete: FSUnLoadComplete Fail");
                    bret = false;
                    break;
                case ProcStep.Err_Wait_FSMGReturnInfo:
                    //RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_Wait_FSMGReturnInfo: FSMGReturnInfo_Rep Fail");
                    bret = false;
                    break;
                case ProcStep.Err_Wait_FSMGReturnComp:
                    //RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_Wait_FSMGReturnComp: FSMGReturnComp_Rep Fail");
                    bret = false;
                    break;
                case ProcStep.Err_Wait_MRBPMagazine:
                    //RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_Wait_MRBPMagazine: MRBPMagazine Fail");
                    bret = false;
                    break;
                case ProcStep.Err_Wait_TransComp:
                    //RvComu.MRSM_Send(e.Fail_unit, "", "", "EQPMOVECHECK_FAIL");
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ProcStep.Err_Wait_TransComp: TransComp Fail");
                    bret = false;
                    break;
            }
            return bret;
        }

        private bool job_pre_assign(SendJobToVecArgs1 e)
        {
            Logger.Inst.Write(e.vecID, CmdLogType.Rv
                , $"job_pre_assign. Src:{e.job.S_EQPID}, Dst:{e.job.T_EQPID}");

            try
            {
                e.vec.C_BATCHID = e.job.BATCHID;
                e.job.C_state = (int)CmdState.PRE_ASSIGN;
                Db.DbUpdate(true, new TableType[] { TableType.PEPSCHEDULE, TableType.VEHICLE });
                Db.DbUpdate(false);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"Exception. job_pre_assign. {ex.Message}\r\n{ex.StackTrace}");
            }
            return false;
        }

        private bool job_assign(SendJobToVecArgs1 e)
        {
            int magazine_count = e.job.TRAYID.Split(',').Count();

            int FStype = e.job.WORKTYPE.Contains("MG") ? (int)EqpFSType.MAGAZINE : (int)EqpFSType.FLASH;
            string cmd_head = e.job.WORKTYPE.Contains("MG") ? "MG" : "FS";
            
            string sendMsg = string.Empty;

            (string eqpslot, string eqpport) = cmd_Check(e);
            string[] slot_Split = null;
            string[] port_Split = null;

            if (eqpslot != null)
                slot_Split = eqpslot.Split(',');
            if (eqpport != null)
                port_Split = eqpport.Split(',');
            eqpslot = string.Empty;
            string reword_GoalName = string.Empty;
            if ((e.eqp.goaltype == (int)EqpGoalType.FSSTK || e.eqp.goaltype == (int)EqpGoalType.SYSWIN) && e.cmd == "SRC")
            {
                FStype = (int)EqpFSType.MAGAZINE;
                
                cmd_head = "MG";
                if (Cfg.Data.UseRv)
                {
                    sendMsg = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};",
                                                cmd_head + e.cmd, e.job.BATCHID, e.eqp.GOALNAME, FStype, e.eqp.lds_dist, e.eqp.rf_id, e.eqp.rf_ch, magazine_count, e.job.TRAYID, e.bufslot, e.job.S_PORT);
                }
                else
                {
                    sendMsg = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};",
                                                cmd_head + e.cmd, e.job.BATCHID, e.eqp.GOALNAME, FStype, e.eqp.lds_dist, e.eqp.rf_id, e.eqp.rf_ch, magazine_count, e.job.TRAYID, e.bufslot, e.eqpslot);
                }
            }
            else if ((e.eqp.goaltype == (int)EqpGoalType.FSSTK || e.eqp.goaltype == (int)EqpGoalType.SYSWIN) && e.cmd == "DST")
            {
                FStype = (int)EqpFSType.MAGAZINE;
                               
                cmd_head = "MG";
                if (Cfg.Data.UseRv)
                {
                    sendMsg = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};",
                                                cmd_head + e.cmd, e.job.BATCHID, e.job.T_EQPID, FStype, e.eqp.lds_dist, e.eqp.rf_id, e.eqp.rf_ch, magazine_count, e.job.TRAYID, e.bufslot, e.job.T_PORT);
                }
                else
                {
                    sendMsg = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};",
                                                cmd_head + e.cmd, e.job.BATCHID, e.eqp.GOALNAME, FStype, e.eqp.lds_dist, e.eqp.rf_id, e.eqp.rf_ch, magazine_count, e.job.TRAYID, e.bufslot, e.eqpslot);
                }
            }
            else if ((e.eqp.goaltype == (int)EqpGoalType.RDT || e.eqp.goaltype == (int)EqpGoalType.AGING) && e.job.WORKTYPE.Contains("FS"))
            {
                if (Cfg.Data.UseRv)
                {
                    string batchid = string.Empty;
                    int mgtype = 0;
                    
                    batchid = string.Format($"C{e.job.WORKTYPE}_{e.job.BATCHID.Split('_')[1]}");
                    var peps = Db.Peps.Where(p => p.BATCHID.Contains(batchid)).ToList();

                    int fsallcount = 0;
                    string childmsg = string.Empty;
                    magazine_count = peps.Count();
                    string MGID = string.Empty;
                    string bufslot = string.Empty;
                    string unit_slot = string.Empty;

                    foreach (var x in peps)
                    {
                        if (MGID.Length > 0)
                            MGID += ",";
                        MGID += (x.WORKTYPE == "FSO") ? string.Format($"{x.T_EQPID}") : string.Format($"{x.S_EQPID}");

                        if (bufslot.Length > 0)
                            bufslot += ",";
                        bufslot += x.C_bufSlot;

                        if (unit_slot.Length > 0)
                            unit_slot += ",";
                        unit_slot += "0,000";

                        fsallcount += x.TRAYID.Split(',').Count();

                        if (x.C_mgtype == "M.2")
                            mgtype = (int)EqpMGType.M2;
                        else if (x.C_mgtype == "SATA")
                            mgtype = (int)EqpMGType.SATA;
                        else if (x.C_mgtype == "SAS")
                            mgtype = (int)EqpMGType.SAS;
                        else if (x.C_mgtype == "UFS")
                            mgtype = (int)EqpMGType.UFS;
                        else if (x.C_mgtype == "M.2_L")
                            mgtype = (int)EqpMGType.M2_L;
                        else if (x.C_mgtype == "M.2_HS")
                            mgtype = (int)EqpMGType.M2_HS;

                        childmsg += string.Format($"{x.BATCHID};{mgtype};{x.TRAYID.Split(',').Count()};{x.TRAYID};");

                        if (e.cmd == "SRC")
                            childmsg += string.Format($"{x.T_PORT};{x.S_PORT};");
                        else
                            childmsg += string.Format($"{x.S_PORT};{x.T_PORT};");
                    }

                    batchid = string.Format($"P{e.job.WORKTYPE}_{e.job.BATCHID.Split('_')[1]}");
                    var pep = Db.Peps.Where(p => p.BATCHID == batchid).SingleOrDefault();

                    sendMsg = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};",
                                                cmd_head + e.cmd, pep.BATCHID, e.eqp.GOALNAME, FStype, e.eqp.lds_dist, e.eqp.rf_id,
                                                e.eqp.rf_ch, magazine_count, MGID, bufslot, unit_slot);

                    sendMsg += string.Format($"{fsallcount.ToString()};{childmsg}");

                }
                else
                {
                    sendMsg = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};",
                                                cmd_head + e.cmd, e.job.BATCHID, e.eqp.GOALNAME, FStype, e.eqp.lds_dist, e.eqp.rf_id, e.eqp.rf_ch, magazine_count, e.job.TRAYID, e.bufslot, e.eqpslot);
                }
            }
            else if ((e.eqp.goaltype == (int)EqpGoalType.RDT || e.eqp.goaltype == (int)EqpGoalType.AGING) && e.job.WORKTYPE.Contains("MG"))
            {
                if (Cfg.Data.UseRv)
                {
                    sendMsg = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};",
                                                cmd_head + e.cmd, e.job.BATCHID, e.eqp.GOALNAME, FStype, e.eqp.lds_dist, e.eqp.rf_id,
                                                e.eqp.rf_ch, magazine_count, e.job.TRAYID, e.job.S_PORT, e.job.C_bufSlot);
                }
                else
                {
                    sendMsg = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};",
                                                cmd_head + e.cmd, e.job.BATCHID, e.eqp.GOALNAME, FStype, e.eqp.lds_dist, e.eqp.rf_id, e.eqp.rf_ch, magazine_count, e.job.TRAYID, e.bufslot, e.eqpslot);
                }
            }
            else
            {
                sendMsg = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};",
                                            cmd_head + e.cmd, e.job.BATCHID, e.eqp.GOALNAME, FStype, e.eqp.lds_dist, e.eqp.rf_id, e.eqp.rf_ch, magazine_count, e.job.TRAYID, e.bufslot, e.eqpslot);
            }
            Logger.Inst.Write(e.vecID, CmdLogType.Rv, sendMsg);
            return SendJobToVehicle(RobotList[e.vecID], sendMsg);
        }

        private bool Go_End(SendJobToVecArgs1 e)
        {
            string batchid = string.Empty;
            if (e.job.WORKTYPE.Contains("FS") && e.job.BATCHID.Contains("CFS"))
            {
                string subbatch = e.job.BATCHID.Split('_')[1];
                batchid = string.Format($"P{e.job.WORKTYPE}_{subbatch}");

                var pep = Db.Peps.Where(p => p.BATCHID == batchid).SingleOrDefault();

                if (pep == null)
                {
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, string.Format("해당 Job[{0}]이 없습니다.;", batchid));

                    return false;
                }
            }
            else
            {
                batchid = e.job.BATCHID;
            }

            RobotList[e.vecID].GO(batchid);
            Logger.Inst.Write(e.vecID, CmdLogType.Rv, string.Format("GO;{0};", batchid));
            Logger.Inst.Write(e.vecID, CmdLogType.Comm, $"차량[{e.vecID}]에 명령[{e.cmd};{batchid};]을 전달했습니다.");
            return true;
        }

        private void Job_Cancel(SendJobToVecArgs1 e, bool bret)
        {
            if (bret)
            {
                if (RvMsgList[e.vec.ID].IsMulti)
                {
                    pepschedule cancel_peps = RvMsgList[e.vec.ID].MultiList.Where(p => p.C_state != null).OrderBy(p => p.BATCHID).First();

                    if (cancel_peps.C_state < 8 || cancel_peps.C_state == 28)
                    {
                        foreach (var v in RvMsgList[e.vec.ID].MultiList)
                        {
                            if (v.C_state == null || v.C_state < 8 || v.C_state == 28)
                            {
                                v.C_state = null;
                                v.C_isChecked = null;
                                v.C_priority = null;
                                v.C_srcArrivingTime = null;
                                v.C_srcAssignTime = null;
                                v.C_srcStartTime = null;
                                v.C_srcFinishTime = null;
                                v.C_dstArrivingTime = null;
                                v.C_dstAssignTime = null;
                                v.C_dstStartTime = null;
                                v.C_dstFinishTime = null;
                                v.C_VEHICLEID = null;
                                v.C_bufSlot = null;
                                v.S_PORT = null;
                                v.T_PORT = null;
                                v.MULTIID = null;
                            }
                        }
                    }
                    else
                    {
                        foreach (var v in RvMsgList[e.vec.ID].MultiList)
                        {
                            v.C_state = 8;
                            v.C_isChecked = null;
                            v.C_dstArrivingTime = null;
                            v.C_dstAssignTime = null;
                            v.C_dstStartTime = null;
                            v.C_dstFinishTime = null;
                        }
                    }
                }
                else
                {
                    if (e.job.C_state == null || e.job.C_state < 8 || e.job.C_state == 28)
                    {
                        e.job.C_state = null;
                        e.job.C_isChecked = null;
                        e.job.C_priority = null;
                        e.job.C_srcArrivingTime = null;
                        e.job.C_srcAssignTime = null;
                        e.job.C_srcStartTime = null;
                        e.job.C_srcFinishTime = null;
                        e.job.C_dstArrivingTime = null;
                        e.job.C_dstAssignTime = null;
                        e.job.C_dstStartTime = null;
                        e.job.C_dstFinishTime = null;
                        e.job.C_VEHICLEID = null;
                        e.job.C_bufSlot = null;
                        e.job.S_PORT = null;
                        e.job.T_PORT = null;
                        e.job.MULTIID = null;
                    }
                    else
                    {
                        e.job.C_state = 8;
                        e.job.C_isChecked = null;
                        e.job.C_dstArrivingTime = null;
                        e.job.C_dstAssignTime = null;
                        e.job.C_dstStartTime = null;
                        e.job.C_dstFinishTime = null;
                    }
                }
                Db.DbUpdate(TableType.PEPSCHEDULE);

                RvMsgList[e.vec.ID].IsMulti = false;
                if (RvMsgList[e.vec.ID].MultiList != null)
                    RvMsgList[e.vec.ID].MultiList.Clear();
                RvMsgList[e.vec.ID].jobcancel_check = true;
                RvMsgList[e.vecID].ChamberFrist = false;
                RvMsgList[e.vecID].ChamberFinish = false;
            }
        }
    

        private (string , string) cmd_Check(SendJobToVecArgs1 e)
        {
            string eqpslot = string.Empty;
            string eqpport = string.Empty;
            if (e.cmd == "SRC")
            {
                eqpslot = e.job.S_PORT;
                eqpport = e.job.S_SLOT;
            }
            else
            {
                eqpslot = e.job.T_PORT;
                eqpport = e.job.T_SLOT;
            }
            return (eqpslot, eqpport);
        }
        private bool SendJobToVehicle(Robot vec, string jobMsg)
        {
            vec.jobResponse = false;
            RvMsgList[vec.m_vecName].jobcancel_check = false;
            vec.SEND_MSG(jobMsg);
            int cnt = 0;
            while (cnt++ < 3000)
            {
                if (vec.jobResponse)
                {
                    return true;
                }

                if (RvMsgList[vec.m_vecName].jobcancel_check)
                {
                    return false;
                }
                Thread.Sleep(20);
            }
            return false;
        }

        private bool WaitToVehicle(Robot vec)
        {
            vec.goReset();
            RvMsgList[vec.m_vecName].jobcancel_check = false;
            int cnt = 0;
            while (cnt++ < 90000)
            {
                if (vec.vecResponse)
                {
                    vec.goRetry_count = 0;
                    return true;
                }

                if (RvMsgList[vec.m_vecName].jobcancel_check)
                {
                    vec.goReset();
                    return false;
                }

                if (vec.goReSend)
                {
                    vec.goReSend = false;
                    cnt = 0;
                }

                if (vec.goRetry_Fail)
                {
                    vec.goRetry_count = 0;
                    vec.goRetry_Fail = false;
                    return false;
                }

                Thread.Sleep(20);
            }
            Logger.Inst.Write(vec.m_vecName, CmdLogType.Rv, $"Exception. GO_END Recv Time Out");
            return false;
        }


        private bool WaitToVehicle_MRBP_Start(Robot vec)
        {
            vec.vecMRBPStart = false;
            RvMsgList[vec.m_vecName].jobcancel_check = false;
            int cnt = 0;
            while (cnt++ < 90000)
            {
                if (vec.vecMRBPStart)
                {                    
                    return true;
                }

                if (RvMsgList[vec.m_vecName].jobcancel_check)
                {
                    return false;
                }

                Thread.Sleep(20);
            }
            Logger.Inst.Write(vec.m_vecName, CmdLogType.Rv, $"Exception. GO_END Recv Time Out");
            return false;
        }
    }
}
