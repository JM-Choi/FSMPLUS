using FSMPlus.Ref;
using FSMPlus.Vehicles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Reflection;

namespace FSMPlus.Logic
{
    public class VehicleEventParser : Global
    {
        #region singleton VEP
        private static volatile VehicleEventParser instance;
        private static object syncVsp = new object();
        public static VehicleEventParser Init
        {
            get
            {
                if (instance == null)
                {
                    lock (syncVsp)
                    {
                        instance = new VehicleEventParser();
                    }
                }
                return instance;
            }
        }
        #endregion
        private Configuration _cfg = Configuration.Init;
        private DbHandler _db = DbHandler.Inst;
        private Logger _log = Logger.Inst;

        public VehicleEventParser()
        {
        }
        public void Vehicle_OnRecvMsg(object sender, RecvMsgArgs e)
        {
            if (sender is Robot vec)
            {
                // 해당 Robot을 DB에서 가져오기
                var targetVec = Db.Vechicles.Where(p => p.ID == vec.m_vecName).Single();

                // Cmd4Vehicle에 맞는 Case로 이동
                switch (e.Cmd)
                {
                    case Cmd4Vehicle.STATUS:
                        // 선정된 Robot의 Data Update
                        targetVec_Update(e, ref targetVec);

                        // 선정된 Robot의 상태가 변경되었는지 확인하는 함수
                        if (targetVec_Status_Change(e, ref targetVec, vec))
                        {
                            Logger.Inst.Write(vec.m_vecName, CmdLogType.Comm, $"Vehicle State Change Event[{vec.m_vecName}] : {e.Cmd}/{e.Status.state}");
                        }
                        Db.DbUpdate(TableType.VEHICLE);
                        // Mplus가 Auto 상태에서 Robot이 재 실행될 경우 자동으로 Resume을 보내기 위한 함수
                        Vehicle_Auto_Resume_Send(e, vec);
                        // Map에 Robot 위치 Update
                        _MapDraw.ChangeStatus(vec.m_vecName, new System.Drawing.Point(e.Status.posX, e.Status.posY), e.Status.angle, e.Status.charge);
                        break;
                    case Cmd4Vehicle.ERROR:
                        // Error 상태 확인
                        // 0이면 Error 해제
                        // 1이면 Error
                        switch (e.ErrState.state)
                        {
                            case 0: // Clear
                                // Alarm 상태 적용 함수
                                AlarmControl(false, vec.m_vecName, e.ErrState.ErrCode);
                                break;
                            case 1: // Occur
                                // Alarm 상태 적용 함수
                                AlarmControl(true, vec.m_vecName, e.ErrState.ErrCode);
                                break;
                            default: break;
                        }
                        Db.DbUpdate(TableType.ALARM);
                        break;
                    case Cmd4Vehicle.JOB:
                        {
                            // Message에서 보내온 BatchID로 DB에서 Job 가져오기
                            var targetCmd = Db.Peps.Where(p => p.BATCHID == e.JobState.batchID).FirstOrDefault();
                            // Job이 없으면
                            if (targetCmd == null)
                            {
                                Logger.Inst.Write(vec.m_vecName, CmdLogType.Comm, $"차량({vec.m_vecName})으로부터 알수 없는 작업 이름이 수신되었습니다. {e.JobState.batchID}");
                                return;

                            }

                            Logger.Inst.Write(vec.m_vecName, CmdLogType.Comm, $"Vehicle Job Event[{vec.m_vecName}] : {e.Cmd}/{e.JobState.state}");

                            // 해당 Robot의 BuffSlot Data를 DB에서 가져오기
                            List<vehicle_part> targetPart = new List<vehicle_part>();
                            targetPart = Db.VecParts.Where(p => p.VEHICLEID == targetVec.ID).ToList();


                            switch (e.JobState.state)
                            {
                                // Job Assign 성공
                                case VehicleCmdState.ASSIGN: 
                                    vec.jobResponse = true;
                                    {
                                        // Job에 SrcFinishTime이 있으면 Dst 설비를, 없으면 Src 설비 이름 저장
                                        string compareStr = (targetCmd.C_srcFinishTime != null) ? targetCmd.T_EQPID : targetCmd.S_EQPID;
                                        // DB Units에서 해당 설비 가져오기
                                        var targetPos = Db.Units_FS.Where(p => p.ID == compareStr).FirstOrDefault();
                                        _MapDraw.ChangeStatus(vec.m_vecName, targetPos.GOALNAME, targetCmd.BATCHID, $"{targetCmd.S_EQPID} => {targetCmd.T_EQPID}");
                                    }
                                    break;
                                case VehicleCmdState.GO_END: // 비클 설비 앞 도착 (alive 후 PIO 동작 전)
                                case VehicleCmdState.JOB_COMPLETE:
                                    // Message 대기 변수 설정
                                    vec.vecResponse = true;
                                    break;
                                default:
                                    break;
                            }

                            // JobState가 GO_END 또는 None이 아니면
                            if (!Jobstate_Go_End(e) && !Jobstate_None(e))
                            {
                                // Message 처리 함수
                                EventDbUpdate(JobProcList[vec.m_vecName].jobType, e.JobState, targetVec, targetCmd, targetPart);
                                if (Cfg.Data.UseRv)
                                {
                                    // 상위 Program에 상태 보고
                                    if (Jobstate_Trans_End(e))
                                    {
                                        string msg = JOB_msg_create(e);

                                        Global.Init.RvComu.MRSM_Send(vec.m_vecName, e.recvMsg.ToUpper().Split(';')[2], e.JobState.batchID, msg);
                                    }
                                    else if (Jobstate_Trans_Comp(e))
                                        Global.Init.RvComu.MRSM_Send(vec.m_vecName, e.recvMsg.ToUpper().Split(';')[2], e.JobState.batchID, e.JobState.all);
                                    else if (Jobstate_Mg_End(e))
                                        Global.Init.RvComu.MRSM_Send(vec.m_vecName, e.recvMsg.ToUpper().Split(';')[2], e.JobState.batchID, e.JobState.all);
                                    else if (Jobstate_Job_Complete(e))
                                        Global.Init.RvComu.MRSM_Send(vec.m_vecName, e.recvMsg.ToUpper().Split(';')[2], e.JobState.batchID, e.JobState.all);
                                    else if (Jobstate_MR_BP_MAGAZINE(e))
                                        Global.Init.RvComu.MRSM_Send(vec.m_vecName, e.recvMsg.ToUpper().Split(';')[2], e.JobState.batchID, e.JobState.all);
                                    else
                                        Global.Init.RvComu.MRSM_Send(vec.m_vecName, e.recvMsg.ToUpper().Split(';')[2], e.JobState.batchID);
                                }
                            }
                        }

                        break;
                    case Cmd4Vehicle.GOAL_LD: 
                    case Cmd4Vehicle.GOAL_UL: 
                        {
                            // Robot이 DB에 저장되어있는 설비의 Data를 Mplus로 요청
                            // DB에 저장되어있는 모든 설비명 가져오기
                            var goalList = Db.Units_FS.Select(p => p.GOALNAME).ToList();
                            var goalListStr = "";
                            foreach (var goals in goalList)
                            {
                                goalListStr += $"{goals};";
                            }
                            // 설비명 Robot으로 전송
                            var sendGoalList = $"GOAL_LD;{goalList.Count};{goalListStr}";
                            vec.SEND_MSG(sendGoalList);
                        }
                        break;
                    case Cmd4Vehicle.MGTYPE_DATA: 
                        {
                            // Robot이 DB에 저장되어있는 Magazine의 Data를 Mplus로 요청
                            // DB에 저장되어있는 모든 Magazine Data 가져오기
                            var MGList = Db.Mg_Types.ToList();
                            var MGListStr = "";
                            foreach (var mgs in MGList)
                            {
                                MGListStr += $"{mgs.MG_NAME};{mgs.MG_MAX};{mgs.FS_MAX};";
                            }
                            // Magazine Data Robot으로 전송
                            var sendMGList = $"MGTYPE_DATA;{MGList.Count};{MGListStr}";
                            vec.SEND_MSG(sendMGList);
                        }
                        break;
                    case Cmd4Vehicle.SCAN:
                        {
                            vehicle_part targetPart = new vehicle_part();
                            targetPart = Db.VecParts.Where(p => p.VEHICLEID == e.Scan.vehicleid).SingleOrDefault();
                            ScanProcess(targetVec, targetPart, e.Scan);
                        }
                        break;
                    case Cmd4Vehicle.GOAL_FAIL:
                        {
                            if (vec.goRetry_count < Cfg.Data.goRetryMax)
                            {
                                RobotList[vec.m_vecName].STOP();
                                System.Threading.Thread.Sleep(1000);
                                RobotList[vec.m_vecName].GO(RvMsgList[vec.m_vecName].CurJob.BATCHID);
                                Logger.Inst.Write(vec.m_vecName, CmdLogType.Rv, string.Format("GO;{0};", RvMsgList[vec.m_vecName].CurJob.BATCHID));
                                Logger.Inst.Write(vec.m_vecName, CmdLogType.Comm, $"차량[{vec.m_vecName}]에 명령[{JobProcList[vec.m_vecName].jobType};{RvMsgList[vec.m_vecName].CurJob.BATCHID};]을 재 전달했습니다.");
                                vec.goReSend = true;
                                vec.goRetry_count++;
                            }
                            else
                            {
                                Logger.Inst.Write(vec.m_vecName, CmdLogType.Comm, $"차량[{vec.m_vecName}]에 명령[{JobProcList[vec.m_vecName].jobType};{RvMsgList[vec.m_vecName].CurJob.BATCHID};] 재시도 횟수를 초과하였습니다. {vec.goRetry_count.ToString()}/{Cfg.Data.goRetryMax.ToString()}");
                            }
                        }
                        break;
                    case Cmd4Vehicle.RESP:
                        {
                            string[] msg = e.recvMsg.Split(';');
                            if (msg[1] == "JOB_START")
                                vec.vecResponse = true;
                        }
                        break;
                    case Cmd4Vehicle.None:
                    default: break;
                }

            }
        }
        /// <summary>
        /// alarm 을 db 에 설정하거나 삭제한다
        /// </summary>
        /// <param name="isSet">false이면 삭제, true이면 추가</param>
        /// <param name="vecId">EQPID OR VEHICLEID</param>
        /// <param name="errorCode">code</param>
        public void AlarmControl(bool isSet, string vecId, int errorCode)
        {
            // isSet이 true면 Error
            // isSet이 false이면 Error 해제
            if (isSet)
            {
                // DB에 정의된 알람 목록 가져오기
                var alrmDf = Db.AlarmsDef.Where(p => p.code == errorCode).SingleOrDefault();
                // 해당 Robot의 알람 목록 불러오기
                var alrmDb = Db.Alarms.Where(p => p.ID == vecId && p.code == errorCode && p.releaseTime == null).SingleOrDefault();
                if (alrmDf == null)
                {
                    Console.WriteLine("정의 되지 않은 알람 code");
                    // 정의되지않은알람으로 alarm 목록 가져오기
                    alrmDf = Db.AlarmsDef.Where(p => p.code == 999).SingleOrDefault();

                    // dbalarm_create으로 alarm Update
                    Logger.Inst.Write(vecId, CmdLogType.All, log_msg(dbalarm_create(alrmDf, vecId, errorCode)));
                }
                else if (alrmDb == null)
                {
                    Console.WriteLine("발생 기록이 확인되지 않는 alarm");

                    // dbalarm_create으로 alarm Update
                    Logger.Inst.Write(vecId, CmdLogType.All, log_msg(dbalarm_create(alrmDf, vecId, errorCode)));
                }
            }
            else
            {
                // 해당 Robot의 Alarm 가져오기
                var dbAlarm = Db.Alarms.Where(p => p.ID == vecId && p.releaseTime == null).ToList();

                if (dbAlarm != null)
                {
                    // 해당 Robot의 alarm이 여러개여도 알람 해제 Message는 1개만 오기때문에
                    // 모든 Alarm 해제
                    for (int i = dbAlarm.Count; i > 0; i--)
                    {
                        dbAlarm[i - 1].releaseTime = DateTime.Now;
                        Db.CopyAlarmToHistory(dbAlarm[i - 1].idx);
                        Db.Delete(dbAlarm[i - 1]);
                    }
                    Db.DbUpdate(true, new TableType[] { TableType.ALARM });
                    Logger.Inst.Write(vecId, CmdLogType.All, log_msg(dbAlarm[0]));
                }
            }
        }

        private alarm dbalarm_create(alarm_define alrmDf, string vecId, int errorCode)
        {
            // alarm Update
            alarm dbalarm = new alarm
            {
                ID = vecId,
                code = errorCode,
                msg = alrmDf.msg,
                level = alrmDf.level,
                eventTime = DateTime.Now,
                releaseTime = null,
            };
            Db.Add(dbalarm);
            return dbalarm;
        }
        private string log_msg(alarm dbalarm)
        {
            return string.Format($"VEHICLE_ID={dbalarm.ID} CODE={dbalarm.code} MSG={dbalarm.msg} LEVEL={dbalarm.level} EVENT_TIME={dbalarm.eventTime} RELAEASE_TIME={dbalarm.releaseTime}");
        }

        private void targetVec_Update(RecvMsgArgs e, ref vehicle targetVec)
        {
            // 선정된 Robot의 DB Data Update
            targetVec.loc_x = e.Status.posX;
            targetVec.loc_y = e.Status.posY;
            targetVec.C_loc_th = e.Status.angle;
            targetVec.C_mode = (int)e.Status.mode;
            targetVec.C_chargeRate = e.Status.charge;
        }
        private bool targetVec_Status_Change(RecvMsgArgs e, ref vehicle targetVec, Robot vec)
        {
            // DB에 저장되어있는 State와 Message의 State를 비교
            if (targetVec.C_state != (int)e.Status.state)
            {
                // State가 변경되었으면 Update
                targetVec.C_state = (int)e.Status.state;
                // Message의 State가 Not_Assign이면
                if (e.Status.state == VehicleState.NOT_ASSIGN)
                {
                    // BatchID, LastArriveUnit 초기화
                    targetVec.C_BATCHID = "";
                    targetVec.C_lastArrivedUnit = string.Empty;
                    _MapDraw.ChangeStatus(vec.m_vecName, string.Empty);
                }

                // 상위 Program에 Robot 상태 변경 보고를 위한 Message 작성 함수
                string msg = Status_msg_create(e);
                // 상위 Program에 Robot 상태 변경 보고
                if (Cfg.Data.UseRv)
                    Global.Init.RvComu.MRSM_Send(vec.m_vecName, e.recvMsg.ToUpper().Split(';')[4], "", msg);
                return true;

            }
            return false;
        }
        private void Vehicle_Auto_Resume_Send(RecvMsgArgs e, Robot vec)
        {
            // Mplus의 상태가 Auto이고 Robot의 mode가 Manual 이면 Resume 전송
            if (RobotList[vec.m_vecName].controllerState == ControllerState.AUTO && e.Status.mode == VehicleMode.MANUAL)
                RobotList[vec.m_vecName].RESUME();
        }
        private bool Jobstate_Go_End(RecvMsgArgs e)
        {
            return e.JobState.state == VehicleCmdState.GO_END;
        }
        private bool Jobstate_Trans_End(RecvMsgArgs e)
        {
            return e.JobState.state == VehicleCmdState.TRANS_END;
        }
        private bool Jobstate_Trans_Comp(RecvMsgArgs e)
        {
            return e.JobState.state == VehicleCmdState.TRANS_COMPLETE;
        }
        private bool Jobstate_Mg_End(RecvMsgArgs e)
        {
            return e.JobState.state == VehicleCmdState.MG_END;
        }
        private bool Jobstate_Job_Complete(RecvMsgArgs e)
        {
            return e.JobState.state == VehicleCmdState.JOB_COMPLETE;
        }
        private bool Jobstate_MR_BP_MAGAZINE(RecvMsgArgs e)
        {
            return e.JobState.state == VehicleCmdState.MR_BP_MAGAZINE;
        }
        private bool Jobstate_None(RecvMsgArgs e)
        {
            return e.JobState.state == VehicleCmdState.None;
        }
        private string Status_msg_create(RecvMsgArgs e)
        {
            return e.recvMsg.ToUpper().Split(';')[5] + ";" + e.recvMsg.ToUpper().Split(';')[6]; ;
        }
        private string JOB_msg_create(RecvMsgArgs e)
        {
            string result = string.Empty;

            for (int i = 3; i < 6; i++)
            {
                if (i != 3)
                    result += ";";

                result += e.recvMsg.ToUpper().Split(';')[i];
            }

            return result;
        }
        private void EventParser_OnDeleteCmd(string e)
        {
            _MainHandler.DeleteCmd(e);
        }

        private void EventParser_OnRvMessageSend(EventArgsTransEnd e)
        {
            unit_fs un = MainHandler.Init.Db.Units_FS.Where(p => p.GOALNAME == e.eqpId).SingleOrDefault();
            string eqpid = (un.goaltype == (int)EqpGoalType.RDT) ? e.eqpId.Substring(0, 8) : e.eqpId.Split('-')[0];
            Global.Init.RvComu.Send(eqpid, e.rvMsg, MethodBase.GetCurrentMethod().Name, e.vecId);
        }
        public async Task EventDbUpdate(string jobtype, VecJobStatus jobst, vehicle vec, pepschedule cmd, List<vehicle_part> parts)
        {
            switch (jobst.state)
            {
                case VehicleCmdState.None:
                    break;
                // State가 Assign 일 때
                case VehicleCmdState.ASSIGN:
                    await AssignProcess(vec, cmd);
                    break;
                // State가 Enroute 일 때
                case VehicleCmdState.ENROUTE:
                    await EnrouteProcess(vec, cmd);
                    break;
                // State가 Arrived 일 때
                case VehicleCmdState.ARRIVED:
                    await ArriveProcess(vec, cmd);
                    break;
                // State가 PIO_Start 일 때
                case VehicleCmdState.PIO_START:
                    await PIOStartPorcess(jobtype, vec, cmd);
                    break;
                // State가 Trans_Start 일 때
                case VehicleCmdState.TRANS_START:
                    await TransferStartPorcess(jobtype, vec, cmd);
                    break;
                // State가 Trans_Start 일 때
                case VehicleCmdState.TRANS_ERROR:
                    await TransferErrorPorcess(jobtype, vec, cmd);
                    break;
                // State가 Trans_Begin 일 때
                case VehicleCmdState.TRANS_BEGIN:
                    await TransferBeginProcess(jobtype, vec, cmd);
                    break;
                // State가 Trans_End 일 때
                case VehicleCmdState.TRANS_END:
                    await TransferEndProcess(jobtype, vec, cmd, parts, jobst);
                    break;
                // State가 Trans_Complete 일 때
                case VehicleCmdState.TRANS_COMPLETE:
                    await TransferCompleteProcess(jobtype, vec, cmd, parts, jobst);
                   break;
                // State가 MG_End 일 때
                case VehicleCmdState.MG_END:
                    await MGEndProcess(jobtype, vec, cmd, parts, jobst);
                    break;
                // State가 Job_Complete 일 때
                case VehicleCmdState.JOB_COMPLETE:
                    await JobCompleteProcess(jobtype, vec, cmd, parts, jobst);
                    break;
                // State가 MR_BP_Magazine 일 때
                case VehicleCmdState.MR_BP_MAGAZINE:
                    await MRBPMagazineProcess(jobtype, vec, cmd, parts, jobst);
                    break;
                default:
                    break;
            }
        }

        private async Task AssignProcess(vehicle vec, pepschedule cmd)
        {
            await Task.Delay(1);
            // Src 작업 Assign
            if (cmd.C_srcFinishTime == null)    
            {
                // Robot과 Job Data Update
                vec.C_BATCHID = cmd.BATCHID;
                vec.C_state = (int)VehicleState.PARKED;
                vec.C_lastArrivedUnit = cmd.S_EQPID;

                cmd.C_VEHICLEID = vec.ID;
                cmd.C_state = (int)CmdState.ASSIGN;
                cmd.C_srcAssignTime = DateTime.Now;
                _db.DbUpdate(true, new TableType[] { TableType.PEPSCHEDULE, TableType.VEHICLE });
                await Task.Delay(100);
                _log.Write(vec.ID, CmdLogType.Db, $"{CollectionEvent.VehicleAssigned.ToString()} : {cmd.BATCHID}");
            }
            // Dst 작업 Assign
            else
            {
                // Robot과 Job Data Update
                vec.C_BATCHID = cmd.BATCHID;
                vec.C_state = (int)VehicleState.PARKED;
                vec.C_lastArrivedUnit = cmd.T_EQPID;

                // Job에 저장된 Robot 이름과 현재 Robot의 이름이 다르면
                // 현재 Robot의 이름으로 Job Robot 이름 Update
                if(cmd.C_VEHICLEID != vec.ID)
                {
                    Debug.WriteLine($"Warning: oldVec={cmd.C_VEHICLEID}, curVec={vec.ID}");
                    cmd.C_VEHICLEID = vec.ID;
                }                
                cmd.C_state = (int)CmdState.DEPARTED;
                cmd.C_dstAssignTime = DateTime.Now;
                _db.DbUpdate(true, new TableType[] { TableType.PEPSCHEDULE, TableType.VEHICLE });

                _log.Write(vec.ID, CmdLogType.Db, $"{CollectionEvent.VehicleDeparted.ToString()} : {cmd.BATCHID}");
            }
        }

        private async Task EnrouteProcess(vehicle vec, pepschedule cmd)
        {
            // Job State Update
            await Task.Delay(1);
            if (cmd.C_srcFinishTime == null)
            {
                cmd.C_state = (int)CmdState.SRC_ENROUTE;
            }
            else
            {
                cmd.C_state = (int)CmdState.DST_ENROUTE;
            }
            _db.DbUpdate(TableType.PEPSCHEDULE);
        }

        private async Task ArriveProcess(vehicle vec, pepschedule cmd)
        {
            await Task.Delay(1);
            // Src 작업 일 때
            if (cmd.C_srcFinishTime == null)
            {
                // Job Data Update
                cmd.C_srcArrivingTime = DateTime.Now;
                cmd.C_state = (int)CmdState.SRC_ARRIVED;

                _db.DbUpdate(TableType.PEPSCHEDULE);

                // Log 작성 함수
                SrcUnitArriveEventReport(vec, cmd);
            }
            // Dst 작업 일 때
            else
            {
                // Job Data Update
                cmd.C_dstArrivingTime = DateTime.Now;
                cmd.C_state = (int)CmdState.DST_ARRIVED;

                _db.DbUpdate(TableType.PEPSCHEDULE);

                // Log 작성 함수
                DestUnitArriveEventReport(vec, cmd);
            }
        }

        private void DestUnitArriveEventReport(vehicle vec, pepschedule cmd)
        {
            _log.Write(vec.ID, CmdLogType.Db, $"{CollectionEvent.VehicleArrived_dst.ToString()} : {cmd.ID}");
        }

        private void SrcUnitArriveEventReport(vehicle vec, pepschedule cmd)
        {
            _log.Write(vec.ID, CmdLogType.Db, $"{CollectionEvent.VehicleArrived_src.ToString()} : {cmd.ID}");
        }

        private async Task PIOStartPorcess(string jobtype, vehicle vec, pepschedule cmd)
        {
            await Task.Delay(1);
            // Src 작업 일 때
            if (jobtype.CompareTo("SRC") == 0)
            {
                // Job Data Update
                cmd.C_srcStartTime = DateTime.Now;
            }
            // Dst 작업 일 때
            else
            {
                // Job Data Update
                cmd.C_dstStartTime = DateTime.Now;
            }
            _db.DbUpdate(TableType.PEPSCHEDULE);
        }

        private async Task TransferStartPorcess(string jobtype, vehicle vec, pepschedule cmd)
        {
            await Task.Delay(1);
            // Src 작업 일 때
            if (jobtype.CompareTo("SRC") == 0)
            {
                // Job Data Update
                cmd.C_state = (int)CmdState.SRC_START;
            }
            // Dst 작업 일 때
            else
            {
                // Job Data Update
                cmd.C_state = (int)CmdState.DST_START;
            }
            _db.DbUpdate(TableType.PEPSCHEDULE);
        }

        private async Task TransferErrorPorcess(string jobtype, vehicle vec, pepschedule cmd)
        {
            await Task.Delay(1);
            if (jobtype.CompareTo("SRC") == 0)
                cmd.C_state = (int)CmdState.SRC_UR_ERROR;
            else
                cmd.C_state = (int)CmdState.DST_UR_ERROR;

            _db.DbUpdate(TableType.PEPSCHEDULE);
        }
        private async Task TransferBeginProcess(string jobtype, vehicle vec, pepschedule cmd)
        {
            await Task.Delay(1);
            // Src 작업 일 때
            if (jobtype.CompareTo("SRC") == 0)
            {
                // Job Data Update
                cmd.C_state = (int)CmdState.SRC_BEGIN;
            }
            // Dst 작업 일 때
            else
            {
                // Job Data Update
                cmd.C_state = (int)CmdState.DST_BEGIN;
            }
            _db.DbUpdate(TableType.PEPSCHEDULE);

            _log.Write(vec.ID, CmdLogType.Db, $"{CollectionEvent.VehicleDepositStarted.ToString()} : {cmd.ID}");
        }

        private async Task TransferEndProcess(string jobtype, vehicle vec, pepschedule cmd, List<vehicle_part> parts, VecJobStatus jobst)
        {
            bool FindTrayIdx(ref int idx)
            {
                //// Job에 있는 TrayID Split
                //string[] tray_split = cmd.TRAYID.Split(',');
                //for (int i = 0; i < tray_split.Length; i++)
                //{
                //    // Message에 있는 TrayID가 Job에 있는 TrayID 존재하면
                //    if (jobst.trayid == tray_split[i])
                //    {   idx = i;
                //        return true;
                //    }
                //}

                if (jobst.slot > 0)
                    idx = jobst.slot - 1;
                else
                    idx = 0;

                return true;
                // Message에 있는 TrayID가 Job에 있는 TrayID 존재하지 않으면
                //return false;
            }                       

            await Task.Delay(1);

            try
            {
                // TRANS_END 에 RV 쪽으로 FS LOAD,UNLOAD 를 보내달라 요청
                if (_cfg.Data.UseRv)
                {
                    int trayIdx = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? jobst.slot_dst : jobst.slot;

                    //// Message에 있는 TrayID와 Job에 있는 TrayID 비교 함수
                    //if (!FindTrayIdx(ref trayIdx))
                    //    return;                     
                    
                    string step_id = string.Empty;
                    string multiid = string.Empty;

                    // Robot의 Trans_End Message에 따라 TC로 EQFSUNLOADED/EQFSLOADED Message 전송
                    // EQFSUNLOADED/EQFSLOADED Message Data 가공
                    string slot  = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? cmd.S_SLOT  : cmd.T_SLOT;
                    string eqp   = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? cmd.S_EQPID : cmd.T_EQPID;
                    eqp = eqp.Split(',')[0];
                    string rvcmd = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? "EQFSUNLOADED" : "EQFSLOADED";
                    string stepid = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? cmd.S_STEPID : cmd.T_STEPID;
                    string[] slot_split = slot.Split(',');
                    
                    unit_fs _unit = Db.Units_FS.Where(p => p.GOALNAME == eqp).Single();
                    string msg_trayid   = jobst.trayid;
                    string msg_slotid   = "E" + trayIdx.ToString("D3");
                    string msg_stepid   = step_id;
                    string msg_execute  = cmd.EXECUTE_TIME;
                    string loaded = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? "UNLOAD" : "LOAD";
                    string rvMsg = string.Empty;

                    if ((_unit.goaltype == (int)EqpGoalType.RDT || _unit.goaltype == (int)EqpGoalType.AGING) && (cmd.WORKTYPE == "FSI" || cmd.WORKTYPE == "FSO"))
                    {
                        string eqpid = string.Empty;
                        if (_unit.goaltype == (int)EqpGoalType.RDT)
                            eqpid = eqp.Substring(0, 8);   
                        else
                            eqpid = eqp.Split('-')[0];

                        rvMsg = string.Format($"{rvcmd} HDR=({eqpid},LH.MPLUS,GARA,TEMP) EQPID={eqpid} SUBEQPID={eqp} FSID={msg_trayid} ESID={msg_slotid} JOBTYPE={loaded} " +
                            $"STEPID={stepid} EXECUTETIME ={msg_execute} MRNO={vec.ID}");
                    }

                    var args = new EventArgsTransEnd();
                    args.rvMsg = rvMsg;
                    args.eqpId = eqp;
                    args.vecId = vec.ID;
                    EventParser_OnRvMessageSend(args);
                }
            }
            catch(Exception ex)
            {
                Logger.Inst.Write(CmdLogType.All, $"{ex}");
            }
            /*
            // 현재 Job이 SRC 일 때
            if (jobtype.CompareTo("SRC") == 0)
            {
                // Job Data Update
                cmd.C_state = (int)CmdState.SRC_END;
                _db.DbUpdate(TableType.PEPSCHEDULE);

                // Robot BuffSlot Data Update
                var part = parts.Where(p => (p.VEHICLEID == vec.ID) && (p.portNo == jobst.port) && (p.slotNo == jobst.slot)).Single();
                if (part != null)
                {
                    part.C_trayId = jobst.trayid;
                    _db.DbUpdate(TableType.VEHICLE_PART);
                }

                _log.Write(vec.ID, CmdLogType.Db, $"{CollectionEvent.VehicleAcquireStarted.ToString()} : {cmd.ID}");
            }
            else
            {
                // Job Data Update
                cmd.C_state = (int)CmdState.DST_END;
                _db.DbUpdate(TableType.PEPSCHEDULE);

                // Robot BuffSlot Data Update
                var part = parts.Where(p => (p.VEHICLEID == vec.ID) && (p.portNo == jobst.port_dst) && (p.slotNo == jobst.slot_dst)).Single();
                if (part != null)
                {
                    part.C_trayId = "";
                    _db.DbUpdate(TableType.VEHICLE_PART);
                }

                _log.Write(vec.ID, CmdLogType.Db, $"{CollectionEvent.VehicleDepositStarted.ToString()} : {cmd.ID}");
            }
            */
        }

        private async Task TransferCompleteProcess(string jobtype, vehicle vec, pepschedule cmd, List<vehicle_part> parts, VecJobStatus jobst)
        {
            void UpdatePepschedule(ref pepschedule peps, string type)
            {
                // 현재 Job이 진행되는 부분이 Src 인지 Dst인지에 따라 Job Data Update
                switch (type)
                {
                    case "SRC":
                        cmd.C_srcFinishTime = DateTime.Now;
                        cmd.C_state = (int)CmdState.SRC_COMPLETE;
                        break;
                    case "DST":
                        cmd.C_dstFinishTime = DateTime.Now;
                        cmd.C_state = (int)CmdState.DST_COMPLETE;
                        break;
                }
            }

            await Task.Delay(1);
            //if (cmd.WORKTYPE == "MGI" || cmd.WORKTYPE == "MGO")
            {
                try
                {
                    UpdatePepschedule(ref cmd, jobtype);
                    // Trans_Complete 시 RV 쪽으로 MG LOAD,UNLOAD 를 보내달라 요청
                    if (_cfg.Data.UseRv)
                    {
                        // Robot의 Trans_complete Message에 따라 TC로 EQMGUNLOADED/EQMGLOADED Message 전송
                        // EQMGUNLOADED/EQMGLOADED Message Data 가공
                        string mgid = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? cmd.T_EQPID : cmd.S_EQPID;
                        string msid = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? cmd.T_SLOT : cmd.S_SLOT;
                        string eqpid = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? cmd.S_EQPID : cmd.T_EQPID;
                        string rvcmd = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? "EQMGUNLOADED" : "EQMGLOADED";
                        string stepid = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? cmd.S_STEPID : cmd.T_STEPID;

                        unit_fs _unit = Db.Units_FS.Where(p => p.GOALNAME == eqpid).Single();
                        string fsid = cmd.TRAYID;
                        string esid = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? cmd.S_SLOT : cmd.T_SLOT;
                        string msg_execute = cmd.EXECUTE_TIME;
                        string rvMsg = string.Empty;

                        if ((_unit.goaltype == (int)EqpGoalType.RDT || _unit.goaltype == (int)EqpGoalType.AGING) && (cmd.WORKTYPE == "FSI" || cmd.WORKTYPE == "FSO"))
                        {
                            string eqp = string.Empty;
                            if (_unit.goaltype == (int)EqpGoalType.RDT)
                                eqp = eqpid.Substring(0, 8);
                            else
                                eqp = eqpid.Split('-')[0];
                            rvMsg = string.Format($"{rvcmd} HDR=({eqp},LH.MPLUS,GARA,TEMP) EQPID={eqp} SUBEQPID={eqpid} MGID={mgid} ");

                            //msid가 1개면 변경없이, 2개 이상이면 괄호를 추가한다
                            if (msid.Split(',').Count() > 1)
                                rvMsg += string.Format($"MSID=({msid}) ");
                            else
                                rvMsg += string.Format($"MSID={msid} ");

                            //fsid 1개면 변경없이, 2개 이상이면 괄호를 추가한다
                            if (fsid.Split(',').Count() > 1)
                                rvMsg += string.Format($"FSID=({fsid}) ");
                            else
                                rvMsg += string.Format($"FSID={fsid} ");

                            //esid 1개면 변경없이, 2개 이상이면 괄호를 추가한다
                            if (esid.Split(',').Count() > 1)
                                rvMsg += string.Format($"ESID=({esid}) ");
                            else
                                rvMsg += string.Format($"ESID={esid} ");                             

                            rvMsg += string.Format($"JOBTYPE={RvMsgList[vec.ID].JobType} STEPID={stepid} EXECUTETIME={msg_execute} MRNO={vec.ID}");

                            var args = new EventArgsTransEnd();
                            args.rvMsg = rvMsg;
                            args.eqpId = eqpid;
                            args.vecId = vec.ID;
                            EventParser_OnRvMessageSend(args);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Inst.Write(CmdLogType.All, $"{ex}");
                }

                // Robot Message 대기 변수를 설정
                RvMsgList[vec.ID].Rvmm._bVehicleSucc = true;
            }
            _log.Write(vec.ID, CmdLogType.Db, $"TransferCompleteProcess : {jobtype}");
        }


        private async Task MGEndProcess(string jobtype, vehicle vec, pepschedule cmd, List<vehicle_part> parts, VecJobStatus jobst)
        {
            bool FindTrayIdx(ref int idx)
            {
                // Job에 있는 TrayID Split
                string[] tray_split = cmd.TRAYID.Split(',');
                for (int i = 0; i < tray_split.Length; i++)
                {
                    // Message에 있는 TrayID가 Job에 있는 TrayID 존재하면
                    if (jobst.trayid == tray_split[i])
                    {
                        idx = i;
                        return true;
                    }
                }
                // Message에 있는 TrayID가 Job에 있는 TrayID 존재하지 않으면
                return false;
            }

            await Task.Delay(1);

            try
            {
                // MG_END 에 RV 쪽으로 FS LOAD,UNLOAD 를 보내달라 요청
                string eqp = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? cmd.S_EQPID : cmd.T_EQPID;
                eqp = eqp.Split(',')[0];
                unit_fs _unit = Db.Units_FS.Where(p => p.GOALNAME == eqp).Single();
                
                // FSSTK의 경우 FSSTK 설비로 RV Message를 전송하는것이 아닌 EC에 직접 Message를 전송
                // FSSTK는 TC 가 존재하지 않기 때문
                if (_cfg.Data.UseRv && _unit.goaltype == (int)EqpGoalType.FSSTK)
                {
                    int trayIdx = 0;
                    // Message에 있는 TrayID와 Job에 있는 TrayID 비교 함수
                    if (!FindTrayIdx(ref trayIdx))
                        return;

                    string multiid = string.Empty;

                    // Robot의 MG_End Message에 따라 TC로 EQFSUNLOADED/EQFSLOADED Message 전송
                    // EQFSUNLOADED/EQFSLOADED Message Data 가공
                    string rvcmd = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? "EQMGUNLOADED" : "EQMGLOADED";
                    string msg_MGid = string.Empty;

                    msg_MGid = jobst.trayid;

                    string msg_execute = cmd.EXECUTE_TIME;
                    string loaded = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? "UNLOAD" : "LOAD";
                    string rvMsg = string.Empty;

                    rvMsg = string.Format($"{rvcmd} HDR=(KDS1.LH.{eqp.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={eqp.Split('-')[0]} MGID={msg_MGid} ");
                    rvMsg += string.Format($"JOBTYPE={loaded} EXECUTETIME={msg_execute} ");
                    if (jobtype == "DST")
                    {
                        if (cmd.WORKTYPE == "FSI")
                            rvMsg += string.Format($"MGSTATUS=EMPTY ");
                        else
                            rvMsg += string.Format($"MGSTATUS=INPROCESS ");
                    }
                    rvMsg += string.Format($"MRNO={vec.ID}");



                    var args = new EventArgsTransEnd();
                    args.rvMsg = rvMsg;
                    args.eqpId = eqp;
                    args.vecId = vec.ID;
                    EventParser_OnRvMessageSend(args);
                }
                else if (_cfg.Data.UseRv && _unit.goaltype == (int)EqpGoalType.SYSWIN)
                {
                    int trayIdx = 0;
                    // Message에 있는 TrayID와 Job에 있는 TrayID 비교 함수
                    if (!FindTrayIdx(ref trayIdx))
                        return;

                    string multiid = string.Empty;

                    // Robot의 MG_End Message에 따라 TC로 EQFSUNLOADED/EQFSLOADED Message 전송
                    // EQFSUNLOADED/EQFSLOADED Message Data 가공
                    string rvcmd = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? "EQMGUNLOADED" : "EQMGLOADED";
                    string msg_MGid = string.Empty;
                    string stepid = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? cmd.S_STEPID : cmd.T_STEPID;
                    string slot = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? cmd.S_SLOT : cmd.T_SLOT;
                    string[] slot_split = slot.Split(',');

                    msg_MGid = jobst.trayid;

                    string msg_execute = cmd.EXECUTE_TIME;
                    string loaded = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? "UNLOAD" : "LOAD";
                    string rvMsg = string.Empty;

                    rvMsg = string.Format($"{rvcmd} HDR=(KDS1.LH.{eqp.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={eqp.Split('-')[0]} SUBEQPID={eqp} MGID={msg_MGid} ");
                    rvMsg += string.Format($"SLOTID={slot_split[trayIdx]} JOBTYPE={loaded} STEPID={stepid} EXECUTETIME={msg_execute} MRNO={vec.ID}");



                    var args = new EventArgsTransEnd();
                    args.rvMsg = rvMsg;
                    args.eqpId = eqp;
                    args.vecId = vec.ID;
                    EventParser_OnRvMessageSend(args);
                }
                else
                //if (_cfg.Data.UseRv && (_unit.goaltype == (int)EqpGoalType.RDT || _unit.goaltype == (int)EqpGoalType.AGING))
                {
                    int trayIdx = 0;
                    // Message에 있는 TrayID와 Job에 있는 TrayID 비교 함수
                    if (!FindTrayIdx(ref trayIdx))
                        return;

                    string multiid = string.Empty;

                    // Robot의 MG_End Message에 따라 TC로 EQFSUNLOADED/EQFSLOADED Message 전송
                    // EQFSUNLOADED/EQFSLOADED Message Data 가공
                    string rvcmd = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? "EQFSUNLOADED" : "EQFSLOADED";
                    string stepid = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? cmd.S_STEPID : cmd.T_STEPID;
                    string msg_MGid = string.Empty;

                    msg_MGid = jobst.trayid;

                    string msg_ESid = string.Empty;
                    
                    msg_ESid = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? string.Format($"ES{jobst.slot_dst.ToString("D2")}") : string.Format($"ES{jobst.slot.ToString("D2")}");
                                       

                    string msg_execute = cmd.EXECUTE_TIME;
                    string loaded = (string.Compare(RvMsgList[vec.ID].JobType, "UNLOAD") == 0) ? "UNLOAD" : "LOAD";
                    string rvMsg = string.Empty;

                    string eqpid = string.Empty;

                    if (_unit.goaltype == (int)EqpGoalType.RDT)
                        eqpid = eqp.Substring(0, 8);
                    else
                        eqpid = eqp.Split('-')[0];

                    rvMsg = string.Format($"{rvcmd} HDR=({eqpid},LH.MPLUS,GARA,TEMP) EQPID={eqpid} SUBEQPID={eqp} MGID={msg_MGid} ESID={msg_ESid} ");
                    rvMsg += string.Format($"STEPID={stepid} JOBTYPE={loaded} EXECUTETIME={msg_execute} MRNO={vec.ID}");
                    

                    var args = new EventArgsTransEnd();
                    args.rvMsg = rvMsg;
                    args.eqpId = eqp;
                    args.vecId = vec.ID;
                    EventParser_OnRvMessageSend(args);
                }
            }
            catch (Exception ex)
            {
                Logger.Inst.Write(CmdLogType.All, $"{ex}");
            }

            // 현재 Job이 SRC 일 때
            if (jobtype.CompareTo("SRC") == 0)
            {
                // Job Data Update
                cmd.C_state = (int)CmdState.SRC_END;
                _db.DbUpdate(TableType.PEPSCHEDULE);

                // Robot BuffSlot Data Update
                var part = parts.Where(p => (p.VEHICLEID == vec.ID) && (p.portNo == jobst.port) && (p.slotNo == jobst.slot)).Single();
                if (part != null)
                {
                    part.C_trayId = jobst.trayid;
                    _db.DbUpdate(TableType.VEHICLE_PART);
                }

                // Magazine Type이 UFS가 아니면 2칸을 차지하므로 추가로 Data Update
                if (cmd.C_mgtype != "UFS")
                {
                    part = parts.Where(p => (p.VEHICLEID == vec.ID) && (p.portNo == jobst.port) && (p.slotNo == jobst.slot + 1)).Single();
                    if (part != null)
                    {
                        part.C_trayId = jobst.trayid;
                        _db.DbUpdate(TableType.VEHICLE_PART);
                    }
                }

                _log.Write(vec.ID, CmdLogType.Db, $"{CollectionEvent.VehicleAcquireStarted.ToString()} : {cmd.ID}");
            }
            else
            {
                // Job Data Update
                cmd.C_state = (int)CmdState.DST_END;
                _db.DbUpdate(TableType.PEPSCHEDULE);

                // Robot BuffSlot Data Update
                var part = parts.Where(p => (p.VEHICLEID == vec.ID) && (p.portNo == jobst.port_dst) && (p.slotNo == jobst.slot_dst)).Single();
                if (part != null)
                {
                    part.C_trayId = "";
                    _db.DbUpdate(TableType.VEHICLE_PART);
                }

                // Magazine Type이 UFS가 아니면 2칸을 차지하므로 추가로 Data Update
                if (cmd.C_mgtype != "UFS")
                {
                    part = parts.Where(p => (p.VEHICLEID == vec.ID) && (p.portNo == jobst.port_dst) && (p.slotNo == jobst.slot_dst + 1)).Single();
                    if (part != null)
                    {
                        part.C_trayId = "";
                        _db.DbUpdate(TableType.VEHICLE_PART);
                    }
                }

                _log.Write(vec.ID, CmdLogType.Db, $"{CollectionEvent.VehicleDepositStarted.ToString()} : {cmd.ID}");
            }
        }

        private async Task JobCompleteProcess(string jobtype, vehicle vec, pepschedule cmd, List<vehicle_part> parts, VecJobStatus jobst)
        {
            void UpdatePepschedule(ref pepschedule peps, string type)
            {
                // 현재 Job이 진행되는 부분이 Src 인지 Dst인지에 따라 Job Data Update
                switch (type)
                {
                    case "SRC":
                        cmd.C_srcFinishTime = DateTime.Now;
                        cmd.C_state = (int)CmdState.SRC_COMPLETE;
                        break;
                    case "DST":
                        
                        if (!cmd.S_EQPID.Contains("FSSTK") || cmd.WORKTYPE.Contains("MG"))
                        {
                            cmd.C_dstFinishTime = DateTime.Now;
                            cmd.C_state = (int)CmdState.DST_COMPLETE;
                        }
                        break;
                }
            }

            await Task.Delay(1);
            UpdatePepschedule(ref cmd, jobtype);
            
            // 현재 Job이 SRC 일 때
            if (jobtype.CompareTo("SRC") == 0)
            {
                _db.DbUpdate(true, new TableType[] { TableType.PEPSCHEDULE, TableType.VEHICLE_PART });
                _log.Write(vec.ID, CmdLogType.Db, $"JobCompleteProcess : SRC");
            }
            else
            {
                // Robot Data Updaate
                string batch = vec.C_BATCHID;

                vec.C_BATCHID = null;
                _db.DbUpdate(true, new TableType[] { TableType.PEPSCHEDULE, TableType.VEHICLE_PART, TableType.VEHICLE });
                _log.Write(vec.ID, CmdLogType.Db, $"JobCompleteProcess : DST");

                // 현재 진행 중인 Job의 DST 설비가 FSSTK 이거나 SRC 설비가 FSSTK이고 BatchID에 PFS를 포함하거나 BatchID에 MG를 포함할때
                // 현재 Job 삭제
                if (cmd.T_EQPID.Contains("FSSTK") || (cmd.S_EQPID.Contains("FSSTK") && (cmd.BATCHID.Contains("PFS") || cmd.BATCHID.Contains("MG"))))
                    EventParser_OnDeleteCmd(cmd.BATCHID);
            }
            RvMsgList[vec.ID].Rvmm.tempdown_check = true;
            RvMsgList[vec.ID].Rvmm._bVehicleSucc = true;
        }

        private async Task MRBPMagazineProcess(string jobtype, vehicle vec, pepschedule cmd, List<vehicle_part> parts, VecJobStatus jobst)
        {
            await Task.Delay(1);
            // MGBP_START는 Magazine을 FS 반송 Table(MRBP)에서 Robot Buff(MRBF)로 이동 완료에 대한 Message
            // MGBP_END는 Magazine을 Robot Buff(MRBF)에서 FS 반송 Table(MRBP)로 이동 완료에 대한 Message
            // MGBP_END 일경우
            if (jobst.mrbp_state == MRBPStatus.MGBP_END)
            {
                RobotList[vec.ID].vecResponse = true;
                // Robot에서 BatchID가 지금 Job으로 올 경우 주석해제
                //if (RvMsgList[vec.ID].ChamberFrist)
                //{
                //    _MainHandler.DeleteCmd(cmd.BATCHID);
                //}
                _log.Write(vec.ID, CmdLogType.Db, $"MRBPMagazineProcess : MGID={jobst.trayid} MRBF {jobst.port_dst}, {jobst.slot_dst} -> MRBP");
            }
            // MGBP_START 일경우
            else if (jobst.mrbp_state == MRBPStatus.MGBP_START)
            {
                // 현재 Job이 Child Job의 마지막 일 경우
                if (RvMsgList[vec.ID].ChamberFinish)
                {
                    RobotList[vec.ID].vecMRBPStart = true;
                }
                // 현재 Job 삭제
                _MainHandler.DeleteCmd(cmd.BATCHID);
                
                _log.Write(vec.ID, CmdLogType.Db, $"MRBPMagazineProcess : MGID={jobst.trayid} MRBP -> MRBF {jobst.port_dst}, {jobst.slot_dst}");
            }
        }
        public async void ScanProcess(vehicle vec, vehicle_part part, VecScanArgs scan)
        {
            await Task.Delay(1);
            int sparecnt = 10;
            for(int i = 0; i < 4; i++)
            {
                sparecnt = 10;
                // 해당 portNo 에 해당하는 열이 slot 갯수만큼 검색된다.
                var items = _db.VecParts.Where(p => p.VEHICLEID == vec.ID && p.portNo == i).OrderBy(p=> p.portNo).ThenBy(p=>p.slotNo).ToList();
                if (items == null)
                {
                    Debug.WriteLine($"portNo:{i} is not define!");
                    continue;
                }
                for(int j = 0; j < 10; j++)
                {
                    if(items[j] == null)
                        continue;

                    switch(j)
                    {
                        case 0: items[j].C_trayId = scan.trayid[i, j]; break;
                        case 1: items[j].C_trayId = scan.trayid[i, j]; break;
                        case 2: items[j].C_trayId = scan.trayid[i, j]; break;
                        case 3: items[j].C_trayId = scan.trayid[i, j]; break;
                        case 4: items[j].C_trayId = scan.trayid[i, j]; break;
                        case 5: items[j].C_trayId = scan.trayid[i, j]; break;
                        case 6: items[j].C_trayId = scan.trayid[i, j]; break;
                        case 7: items[j].C_trayId = scan.trayid[i, j]; break;
                        case 8: items[j].C_trayId = scan.trayid[i, j]; break;
                        case 9: items[j].C_trayId = scan.trayid[i, j]; break;
                    }

                    if (scan.trayid[i, j] != null)
                        sparecnt--;
                }
                _db.DbUpdate(TableType.VEHICLE_PART);
            }
        }
    }

    public class EventArgsTransEnd : EventArgs
    {
        public string rvMsg = string.Empty;
        public string eqpId = string.Empty;
        public string vecId = string.Empty; // jm.choi - 190410 OnTransEnd 시 사용
    }
}
