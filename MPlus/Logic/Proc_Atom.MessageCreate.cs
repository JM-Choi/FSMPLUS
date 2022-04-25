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
using TIBCO.Rendezvous;

namespace FSMPlus.Logic
{
    public partial class Proc_Atom
    {
        // Global start
        #region Global
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

        public bool WaitRvMessage(string vecID, bool b = true)
        {
            if (b) RvMsgList[vecID].Rvmm.ResetFlag();

            //40분 대기 (TempDown 의 결과 40분가지도 갈 수 있다는 command)
            //40분 * 60초 * 1000 msec, 루프안에 delay 100 이 있으니 1000 mesc ==> 10
            int limit = 40 * 60 * 10, count = 0;
            while (count < limit)
            {
                if (RvMsgList[vecID].Rvmm._bWait)
                {
                    RvMsgList[vecID].Rvmm._bWait = false;
                    break;
                }

                if (RvMsgList[vecID].Rvmm._bSucc)
                    return true;
                if (RvMsgList[vecID].Rvmm._berror)
                    return false;

                bool ret = IsControllerStop(vecID);
                if (ret)
                    return !ret;
                Thread.Sleep(100);
                count++;
            }
            Logger.Inst.Write(vecID, CmdLogType.All, "Unload/Load Info RV Message Time Out");
            return false;
        }
        private bool WaitTempDownRvMessage(string vecID)
        {
            //40분 대기 (TempDown 의 결과 40분가지도 갈 수 있다는 command)
            //40분 * 60초 * 1000 msec, 루프안에 delay 100 이 있으니 1000 mesc ==> 10
            int limit = 40 * 60 * 10, count = 0;
            while (count < limit)
            {
                if (RvMsgList[vecID].Rvmm._TempDownbWait)
                {
                    RvMsgList[vecID].Rvmm._TempDownbWait = false;
                    count++;
                    continue;
                }

                if (RvMsgList[vecID].Rvmm._TempDownbSucc)
                {
                    RvMsgList[vecID].Rvmm._TempDownbSucc = false;
                    return true;
                }
                if (RvMsgList[vecID].Rvmm._TempDownberror)
                {
                    RvMsgList[vecID].Rvmm._TempDownberror = false;
                    return false;
                }

                bool ret = IsControllerStop(vecID);
                if (ret)
                    return !ret;
                Thread.Sleep(100);
                count++;
            }
            Logger.Inst.Write(vecID, CmdLogType.All, "Temp Down RV Message Time Out");
            return false;
        }
        private bool WaitVehicleJobCompMessage(ref bool bret, string vecID)
        {
            RvMsgList[vecID].jobcancel_check = false;
            int limit = 18000, count = 0;    //30분 대기
            while (count < limit)
            {
                if (RvMsgList[vecID].Rvmm._bVehicleWait)
                {
                    RvMsgList[vecID].Rvmm._bVehicleWait = false;
                    count++;
                    continue;
                }

                if (RvMsgList[vecID].Rvmm._bVehicleSucc)
                {
                    RvMsgList[vecID].Rvmm._bVehicleSucc = false;
                    bret = true;
                    return true;
                }
                if (RvMsgList[vecID].jobcancel_check)
                    bret = false;

                bool ret = IsControllerStop(vecID);
                if (ret)
                    return !ret;
                Thread.Sleep(100);
                count++;
            }
            Logger.Inst.Write(vecID, CmdLogType.All, "Vehicle Trans Complete Time Out");
            return false;
        }
        #endregion
        // Global End

        // Chk_EqStatus Method Start
        #region Chk_EqStatus

        public bool Chk_EqStatus(SendJobToVecArgs1 e, unit_fs srcUnit, unit_fs dstUnit)
        {
            Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"Src Job. job_assign. B:{e.job.BATCHID},S:{e.job.S_EQPID},D:{e.job.T_EQPID}");

            RvMsgList[e.vecID].Rvmm.Alloc(e);

            int errordelaytime = Cfg.Data.RvErrDelayTime
                       , limit = Cfg.Data.RvMainRetrylimit
                       , count = 0;

            bool err = false;
            string fail_unit = string.Empty;
            RvMsgList[e.vec.ID].jobcancel_check = false;
            while (count < limit)
            {
                bool ret = IsControllerStop(e.vecID);
                if (ret)
                    return !ret;
                if (err)
                {
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"main error cnt:({count}/{limit})");
                    Thread.Sleep(errordelaytime * 1000);
                }
                if (RvMsgList[e.vec.ID].jobcancel_check)
                    return false;

                if (RvMsgList[e.vec.ID].JobType == "UNLOAD")
                {
                    if (!CHK_EQorSTK(e, srcUnit, RvStatusChk.eChkStepFrom, ref count))
                    {
                        fail_unit = srcUnit.GOALNAME;
                        continue;
                    }
                }

                if (e.job.WORKTYPE == "FSO" && (dstUnit.goaltype == (int)EqpGoalType.RDT || dstUnit.goaltype == (int)EqpGoalType.AGING))
                {
                    if (!CHK_EQorSTK(e, dstUnit, RvStatusChk.eChkStepFrom, ref count))
                    {
                        fail_unit = dstUnit.GOALNAME;
                        continue;
                    }
                }
                else
                {
                    if (!CHK_EQorSTK(e, dstUnit, RvStatusChk.eChkStepTo, ref count))
                    {
                        fail_unit = dstUnit.GOALNAME;
                        continue;
                    }
                }

                if (RvMsgList[e.vec.ID].jobcancel_check)
                    return false;

                Thread.Sleep(500);
                return true;
            }
            e.Fail_unit = fail_unit;
            return false;
        }

        private bool CHK_EQorSTK(SendJobToVecArgs1 e, unit_fs Unit, RvStatusChk rvstatusval, ref int count, string val = null)
        {
            RvMsgList[e.vecID].Rvmm.ResetFlag();

            unit_fs unt = null;
            bool err = CHK_EQorSTK_Sts(e, Unit, rvstatusval, val);
            if (!err)
            {
                unt = Unit;

                Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"{RvMsgList[e.vecID].JobType}:{unt}. Status ERROR - {unt}");
                count++;
                return err;
            }

            Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"{RvMsgList[e.vecID].JobType}:{unt}. Status OK - To {unt}");

            return err;
        }
        private bool CHK_EQorSTK_Sts(SendJobToVecArgs1 e, unit_fs Unit, RvStatusChk eDirect, string type = null)
        {
            RvMsgList[e.vecID].Rvmm.ResetFlag();
            int count = 0, noresponse_time = 0;
            bool bSend = false, bNoResp = false;

            pepschedule send = e.job;
            DateTime dStart = DateTime.Now;
            while (count < Cfg.Data.RvSubRetrylimit)
            {
                bool ret = IsControllerStop(e.vecID);
                if (ret)
                    return !ret;

                if (RvMsgList[e.vecID].Rvmm._bWait)
                {
                    CHK_EQorSTK_Sts_rv_wait(e, ref count, ref bNoResp, ref bSend, ref dStart, ref noresponse_time);
                    continue;
                }

                if (!bSend)
                {
                    if (eDirect == RvStatusChk.eChkStepTo)
                    {
                        if (!SendLOADData(e, eDirect, type, dStart, ref bSend, count, send))
                            return false;
                    }
                    else
                    {
                        if (!SendUNLOADData(e, Unit, eDirect, type, dStart, ref bSend, count))
                            return false;
                    }
                }

                if (RvMsgList[e.vecID].Rvmm._bSucc)
                {
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"CHK_EQorSTK_Sts. {RvMsgList[e.vecID].Rvmm._bSucc}");
                    RvMsgList[e.vecID].Rvmm._bSucc = false;
                    return true;
                }

                if (RvMsgList[e.vec.ID].jobcancel_check)
                    return false;

                if (noresponse_time == Cfg.Data.RvErrDelayTime * 10)
                {
                    RvMsgList[e.vecID].Rvmm._bWait = true;
                    bNoResp = true;
                }
                noresponse_time++;
                Thread.Sleep(100);
            }

            Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"CHK_EQorSTK_Sts. limit over");
            return false;
        }
        private void CHK_EQorSTK_Sts_rv_wait(SendJobToVecArgs1 e, ref int count, ref bool bNoResp, ref bool bSend, ref DateTime dStart, ref int noresponse_time)
        {
            count++;
            if (bNoResp)
            {
                bNoResp = false;
                Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"NoResponse, Retry({count}/{Cfg.Data.RvSubRetrylimit})");
            }
            else
            {
                Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"ResponseErr, Retry({count}/{Cfg.Data.RvSubRetrylimit})");
                Thread.Sleep(Cfg.Data.RvErrDelayTime * 1000);
            }


            // unset wait 
            RvMsgList[e.vecID].Rvmm.ResetFlag();
            bSend = false;
            dStart = DateTime.Now;
            noresponse_time = 0;
        }
        private bool SendLOADData(SendJobToVecArgs1 e, RvStatusChk eDirect, string type, DateTime dStart, ref bool bSend, int count, pepschedule send)
        {
            string eqpid = string.Empty;
            string[] eqpid_count = e.job.T_EQPID.Split(',');

            for (int i = 0; i < eqpid_count.Count(); i++)
            {
                if (i == 0)
                    eqpid = eqpid_count[i];
                unit_fs split_dstUnit = Db.Units_FS.Where(p => p.GOALNAME == eqpid_count[i]).Single();
                string eqp = string.Empty;
                if (eqpid_count.Count() > 1)
                {
                    e.job = Db.Peps.Where(p => p.EXECUTE_TIME == e.job.EXECUTE_TIME && p.T_EQPID == eqpid_count[i]
                                            && p.MULTIID == e.job.MULTIID).FirstOrDefault();
                }
                string sndMsg = SndMsg_Decision(e, split_dstUnit, eDirect, out eqp);
                try
                {
                    if (!Global.Init.RvComu.Send(eqp, sndMsg, MethodBase.GetCurrentMethod().Name, e.vecID))
                        return false;
                    
                    dStart = DateTime.Now;
                    bSend = true;
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"({count}/{Cfg.Data.RvSubRetrylimit})");
                }
                catch (RendezvousException exception)
                {
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"CHK_EQorSTK_Sts. RendezvousException Occur. retry exit.\r\n{exception.Message}\r\n{exception.StackTrace}");
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"CHK_EQorSTK_Sts. Exception Occur. retry exit.\r\n{ex.Message}\r\n{ex.StackTrace}");
                    return false;
                }

                if (i == eqpid_count.Count() - 1)
                {
                    break;
                }

                while (!RvMsgList[e.vecID].Rvmm._bSucc)
                {
                }
                if (i != eqpid_count.Count() - 1)
                    RvMsgList[e.vecID].Rvmm._bSucc = false;
            }

            e.job = send;
            e.job.T_EQPID = eqpid;
            return true;
        }
        private bool SendUNLOADData(SendJobToVecArgs1 e, unit_fs Unit, RvStatusChk eDirect, string type, DateTime dStart, ref bool bSend, int count)
        {
            string eqp = string.Empty;
            string sndMsg = SndMsg_Decision(e, Unit, eDirect, out eqp);
            try
            {
                if (!Global.Init.RvComu.Send(eqp, sndMsg, MethodBase.GetCurrentMethod().Name, e.vecID))
                    return false;

                dStart = DateTime.Now;
                bSend = true;
                Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"({count}/{Cfg.Data.RvSubRetrylimit})");
                return true;
            }
            catch (RendezvousException exception)
            {
                Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"CHK_EQorSTK_Sts. RendezvousException Occur. retry exit.\r\n{exception.Message}\r\n{exception.StackTrace}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"CHK_EQorSTK_Sts. Exception Occur. retry exit.\r\n{ex.Message}\r\n{ex.StackTrace}");
                return false;
            }
        }
        private string SndMsg_Decision(SendJobToVecArgs1 e, unit_fs Unit, RvStatusChk eDirect, out string eqpId)
        {
            eqpId = SndMsg_Decision_eqpId_Create(Unit);
            return SndMsg_Decision_sndMsg_Create(e, eDirect, Unit);
        }
        private string SndMsg_Decision_eqpId_Create(unit_fs sendunit)
        {
            string eqp = string.Empty;
            if (sendunit.goaltype == (int)EqpGoalType.FSSTK)
                eqp = "FSSTK";
            else if (sendunit.goaltype == (int)EqpGoalType.RDT)
                eqp = sendunit.ID.Substring(0, 8);
            else
                eqp = sendunit.ID.Split('-')[0];

            return eqp;
        }
        private string SndMsg_Decision_sndMsg_Create(SendJobToVecArgs1 e, RvStatusChk eDirect, unit_fs sendunit)
        {
            string sndMsg = string.Empty;
            string jobtype = string.Empty;
            string stepid = string.Empty;

            jobtype = (eDirect == RvStatusChk.eChkStepFrom) ? "UNLOAD" : "LOAD";
            stepid = (eDirect == RvStatusChk.eChkStepFrom) ? e.job.S_STEPID : e.job.T_STEPID;

            if ((EqpGoalType)sendunit.goaltype == EqpGoalType.FSSTK)
            {
                sndMsg = string.Format($"EQFSMOVECHECK HDR=(KDS1.LH.{sendunit.ID.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={sendunit.ID.Split('-')[0]} MGID=({e.job.TRAYID}) ");
                sndMsg += string.Format($"JOBTYPE={jobtype} MRNO={e.vecID}");
            }
            else if ((EqpGoalType)sendunit.goaltype == EqpGoalType.RDT)
            {
                sndMsg = string.Format($"EQFSMOVECHECK HDR=({sendunit.ID.Substring(0,8)},LH.MPLUS,GARA,TEMP) EQPID={sendunit.ID.Substring(0, 8)} SUBEQPID={sendunit.ID} MGID=({e.job.TRAYID}) ");
                sndMsg += string.Format($"JOBTYPE={jobtype} MRNO={e.vecID}");
            }
            else if ((EqpGoalType)sendunit.goaltype == EqpGoalType.AGING)
            {
                sndMsg = string.Format($"EQFSMOVECHECK HDR=({sendunit.ID.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={sendunit.ID.Split('-')[0]} SUBEQPID={sendunit.ID} MGID=({e.job.TRAYID}) ");
                sndMsg += string.Format($"JOBTYPE={jobtype} MRNO={e.vecID}");
            }
            else
            {
                sndMsg = string.Format($"EQFSMOVECHECK HDR=({sendunit.ID.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={sendunit.ID.Split('-')[0]} SUBEQPID={sendunit.ID} MGID=({e.job.TRAYID}) ");
                sndMsg += string.Format($"JOBTYPE={jobtype} STEPID={stepid} MRNO={e.vecID}");
            }
            return sndMsg;
        }
        #endregion
        // Chk_EqStatus Method End

        // FSLoadInfoSet Method Start
        #region FSLoadInfoSet
        public bool FSLoadInfoSet(int goaltype, SendJobToVecArgs1 e)
        {
            bool bret = false;
            int forcount = 0;
            string[] teqpid = e.job.T_EQPID.Split(',');
            (string sndMsg, string devTyp) = FSLoadInfoSet_sndMsg_Create(goaltype, e, MethodBase.GetCurrentMethod().Name, teqpid);

            string[] sendMsgs = sndMsg.Split(';');

            foreach (var x in sendMsgs)
            {
                unit_fs un = MainHandler.Init.Db.Units_FS.Where(p => p.GOALNAME == teqpid[forcount]).SingleOrDefault();
                string eqpid = (un.goaltype == (int)EqpGoalType.RDT) ? teqpid[forcount].Substring(0,8) : teqpid[forcount].Split('-')[0];
                bret = Global.Init.RvComu.Send(eqpid, x, MethodBase.GetCurrentMethod().Name, e.vecID);
                forcount++;
                if (!bret)
                    return bret;
            }
            return bret;
        }

        private (string, string) FSLoadInfoSet_sndMsg_Create(int goaltype, SendJobToVecArgs1 e, string RvMsgName, string[] teqpid)
        {
            string sndMsg = string.Empty;
            string devTyp = string.Empty;
            string mgid = string.Empty;
            string fsid = string.Empty;
            string stepid = string.Empty;
            Logger.Inst.Write(e.vecID, CmdLogType.Rv, "LOAD GOALTYPE = " + ((EqpGoalType)goaltype).ToString());

            foreach (var k in teqpid)
            {
                switch ((EqpGoalType)goaltype)
                {
                    case EqpGoalType.FSSTK:
                        break;
                    default:
                        if (e.job.WORKTYPE == "FSI")
                        {
                            string batchid = string.Format($"C{e.job.WORKTYPE}");

                            List<pepschedule> child_jobs = e.joblist.Where(p => p.WORKTYPE == e.job.WORKTYPE && p.REAL_TIME == e.job.REAL_TIME && p.BATCHID.Contains(batchid)
                                                                            && p.T_EQPID == k).OrderBy(p => p.BATCHID).ToList();
                            string[] mgids = e.job.TRAYID.Split(',');
                            foreach (var x in child_jobs)
                            {
                                foreach (var m in mgids)
                                {
                                    if (x.S_EQPID == m)
                                    {
                                        string[] childtrayids = x.TRAYID.Split(',');
                                        for (int i = 0; i < childtrayids.Count(); i++)
                                        {
                                            if (fsid != null && fsid != "")
                                                fsid += ",";

                                            fsid += string.Format($"{m}:{childtrayids[i]}");
                                        }
                                        if (mgid != null && mgid != "")
                                            mgid += ",";

                                        mgid += string.Format($"{m}");
                                        stepid = x.STEPID;
                                        break;
                                    }
                                }
                            }

                            if (mgid.Split(',').Count() > 1)
                                mgid = string.Format($"({mgid})");

                            if (fsid.Split(',').Count() > 1)
                                fsid = string.Format($"({fsid})");


                            if (sndMsg != "")
                                sndMsg += ";";
                            if (goaltype == (int)EqpGoalType.RDT)
                                sndMsg += string.Format($"{RvMsgName.ToUpper()} HDR=({k.Substring(0, 8)},LH.MPLUS,GARA,TEMP) EQPID={k.Substring(0,8)} SUBEQPID={k} MGID={mgid} FSID={fsid} BATCHJOBID={e.job.BATCHID} STEPID={stepid} MRNO={e.vecID}");
                            else
                                sndMsg += string.Format($"{RvMsgName.ToUpper()} HDR=({k.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={k.Split('-')[0]} SUBEQPID={k} MGID={mgid} FSID={fsid} BATCHJOBID={e.job.BATCHID} STEPID={stepid} MRNO={e.vecID}");

                            devTyp = "CHAMBER";
                        }
                        else if (e.job.WORKTYPE == "MGI")
                        {
                            if (sndMsg != "")
                                sndMsg += ";";

                            mgid = (e.job.TRAYID.Split(',').Count() > 1) ? string.Format($"({e.job.TRAYID})") : e.job.TRAYID;

                            if (goaltype == (int)EqpGoalType.RDT)
                                sndMsg += string.Format($"{RvMsgName.ToUpper()} HDR=({k.Substring(0, 8)},LH.MPLUS,GARA,TEMP) EQPID={k.Substring(0, 8)} SUBEQPID={k} MGID={mgid} BATCHJOBID={e.job.BATCHID} STEPID={e.job.STEPID} MRNO={e.vecID}");
                            else
                                sndMsg += string.Format($"{RvMsgName.ToUpper()} HDR=({k.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={k.Split('-')[0]} SUBEQPID={k} MGID={mgid} BATCHJOBID={e.job.BATCHID} STEPID={e.job.STEPID} MRNO={e.vecID}");

                            devTyp = "CHAMBER";

                        }
                        break;
                }
            }
            return (sndMsg, devTyp);
        }
        #endregion
        // FSLoadInfoSet Method End

        // FSUnLoadInfo Method Start
        #region FSUnLoadInfo
        public bool EQFSUnloadMove(int goaltype, SendJobToVecArgs1 e)
        {
            string sndMsg = string.Empty;
            string devTyp = string.Empty;

#if true
            (sndMsg, devTyp) = EQFSUnloadInfo_sndMsg_Create(goaltype, e, MethodBase.GetCurrentMethod().Name);
#else
            if (goaltype == (int)EqpGoalType.RDT)
            {
                (sndMsg, devTyp) = FSLoadMove_sndMsg_Create(goaltype, e, MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                (sndMsg, devTyp) = EQFSUnloadInfo_sndMsg_Create(goaltype, e, MethodBase.GetCurrentMethod().Name);
            }
#endif

            unit_fs un = MainHandler.Init.Db.Units_FS.Where(p => p.GOALNAME == e.job.S_EQPID).SingleOrDefault();
            string eqpid = (un.goaltype == (int)EqpGoalType.RDT) ? e.job.S_EQPID.Substring(0, 8) : e.job.S_EQPID.Split('-')[0];
            return Global.Init.RvComu.Send(eqpid, sndMsg, MethodBase.GetCurrentMethod().Name, e.vecID);

        }

        public bool EQFSUnLoadStandby(int goaltype, SendJobToVecArgs1 e)
        {
            Logger.Inst.Write(e.vecID, CmdLogType.Rv, "unload start");

            (string sndMsg, string devTyp) = EQFSUnloadInfo_sndMsg_Create(goaltype, e, MethodBase.GetCurrentMethod().Name);
            
            unit_fs un = MainHandler.Init.Db.Units_FS.Where(p => p.GOALNAME == e.job.S_EQPID).SingleOrDefault();
            string eqpid = (un.goaltype == (int)EqpGoalType.RDT) ? e.job.S_EQPID.Substring(0, 8) : e.job.S_EQPID.Split('-')[0];
            return Global.Init.RvComu.Send(eqpid, sndMsg, MethodBase.GetCurrentMethod().Name, e.vecID);
        }

        public bool EQFSUnLoadInfo(int goaltype, SendJobToVecArgs1 e)
        {
            (string sndMsg, string devTyp) = EQFSUnloadInfo_sndMsg_Create(goaltype, e, MethodBase.GetCurrentMethod().Name);

            unit_fs un = MainHandler.Init.Db.Units_FS.Where(p => p.GOALNAME == e.job.S_EQPID).SingleOrDefault();
            string eqpid = (un.goaltype == (int)EqpGoalType.RDT) ? e.job.S_EQPID.Substring(0, 8) : e.job.S_EQPID.Split('-')[0];
            return Global.Init.RvComu.Send(eqpid, sndMsg, MethodBase.GetCurrentMethod().Name, e.vecID);
        }
        private (string, string) EQFSUnloadInfo_sndMsg_Create(int goaltype, SendJobToVecArgs1 e, string RvMsgName)
        {
            string sndMsg = string.Empty;
            string devTyp = string.Empty;
            Logger.Inst.Write(e.vecID, CmdLogType.Rv, "UNLOAD GOALTYPE = " + ((EqpGoalType)goaltype).ToString());
            string mgid = string.Empty;
            string trayid = string.Empty;
            string s_slotno = string.Empty;
            string t_slotno = string.Empty;
            
            s_slotno = (e.job.S_SLOT.Split(',').Count() > 1) ? string.Format($"({e.job.S_SLOT})") : e.job.S_SLOT;
            t_slotno = (e.job.T_SLOT.Split(',').Count() > 1) ? string.Format($"({e.job.T_SLOT})") : e.job.T_SLOT;
            trayid = (e.job.TRAYID.Split(',').Count() > 1) ? string.Format($"({e.job.TRAYID})") : e.job.TRAYID;

            switch ((EqpGoalType)goaltype)
            {
                case EqpGoalType.FSSTK:                 

                    sndMsg = string.Format($"{RvMsgName.ToUpper()} HDR=(KDS1.LH.{e.job.S_EQPID.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={e.job.S_EQPID.Split('-')[0]} MGID={trayid} " +
                        $"BATCHJOBID={e.job.BATCHID} MULTIJOBID= SOURCEPORT={e.job.S_EQPID.Split('-')[0]} SOURCESLOTNO={e.job.S_SLOT} DESTPORT={e.job.T_EQPID} DESTSLOTNO={e.job.T_SLOT} " +
                        $"EXECUTETIME={e.job.EXECUTE_TIME} MRNO={e.vecID}");

                    devTyp = "FSSTK";
                    break;
                case EqpGoalType.SYSWIN:
                    sndMsg = string.Format($"{RvMsgName.ToUpper()} HDR=({e.job.S_EQPID.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={e.job.S_EQPID.Split('-')[0]} SUBEQPID={e.job.S_EQPID} " +
                    $"MGID={trayid} LOTID={e.job.LOT_NO} SLOTID={s_slotno} EXECUTETIME={e.job.EXECUTE_TIME} MRNO={e.vecID} STEPID={e.job.S_STEPID} " +
                    $"JOBTYPE=UNLOAD");
                    break;
                default:   
                    if (e.job.WORKTYPE == "FSO")
                    {
                        mgid = (e.job.T_EQPID.Split(',').Count() > 1) ? string.Format($"({e.job.T_EQPID})") : e.job.T_EQPID;

                        if (goaltype == (int)EqpGoalType.RDT)
                            sndMsg = string.Format($"{RvMsgName.ToUpper()} HDR=({e.job.S_EQPID.Substring(0,8)},LH.MPLUS,GARA,TEMP) EQPID={e.job.S_EQPID.Substring(0, 8)} SUBEQPID={e.job.S_EQPID} " +
                            $"MGID={e.job.T_EQPID} MSID={t_slotno} FSID={trayid} SOURCEPORT={e.job.S_EQPID} SOURCESLOTNO={s_slotno} DESTPORT=MRBP " +
                            $"DESTSLOTNO={t_slotno} STEPID={e.job.STEPID} EXECUTETIME={e.job.EXECUTE_TIME} MRNO={e.vecID}");
                        else
                            sndMsg = string.Format($"{RvMsgName.ToUpper()} HDR=({e.job.S_EQPID.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={e.job.S_EQPID.Split('-')[0]} SUBEQPID={e.job.S_EQPID} " +
                            $"MGID={e.job.T_EQPID} MSID={t_slotno} FSID={trayid} SOURCEPORT={e.job.S_EQPID} SOURCESLOTNO={s_slotno} DESTPORT=MRBP " +
                            $"DESTSLOTNO={t_slotno} STEPID={e.job.STEPID} EXECUTETIME={e.job.EXECUTE_TIME} MRNO={e.vecID}");
                    }
                    else
                    {
                        if (goaltype == (int)EqpGoalType.RDT)
                            sndMsg = string.Format($"{RvMsgName.ToUpper()} HDR=({e.job.S_EQPID.Substring(0, 8)},LH.MPLUS,GARA,TEMP) EQPID={e.job.S_EQPID.Substring(0, 8)} SUBEQPID={e.job.S_EQPID} " +
                            $"MGID={trayid} SOURCEPORT={e.job.S_EQPID} SOURCESLOTNO={s_slotno} DESTPORT={e.job.T_EQPID} " +
                            $"DESTSLOTNO={t_slotno} STEPID={e.job.STEPID} EXECUTETIME={e.job.EXECUTE_TIME} MRNO={e.vecID}");
                        else
                            sndMsg = string.Format($"{RvMsgName.ToUpper()} HDR=({e.job.S_EQPID.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={e.job.S_EQPID.Split('-')[0]} SUBEQPID={e.job.S_EQPID} " +
                            $"MGID={trayid} SOURCEPORT={e.job.S_EQPID} SOURCESLOTNO={s_slotno} DESTPORT={e.job.T_EQPID} " +
                            $"DESTSLOTNO={t_slotno} STEPID={e.job.STEPID} EXECUTETIME={e.job.EXECUTE_TIME} MRNO={e.vecID}");

                    }
                    devTyp = "CHAMBER";

                    break;
            }
            return (sndMsg, devTyp);
        }
#endregion
        // FSUnLoadInfo Method End


        // FSLoadInfo Method Start
#region FSLoadInfo
        public bool EQFSLoadMove(int goaltype, SendJobToVecArgs1 e)
        {
            string sndMsg = string.Empty;
            string devTyp = string.Empty;

#if true
            (sndMsg, devTyp) = FSLoadInfo_sndMsg_Create(goaltype, e, MethodBase.GetCurrentMethod().Name);
#else
            if (goaltype == (int)EqpGoalType.RDT)
            {
                (sndMsg, devTyp) = FSLoadMove_sndMsg_Create(goaltype, e, MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                (sndMsg, devTyp) = FSLoadInfo_sndMsg_Create(goaltype, e, MethodBase.GetCurrentMethod().Name);
            }
#endif
            unit_fs un = MainHandler.Init.Db.Units_FS.Where(p => p.GOALNAME == e.job.T_EQPID).SingleOrDefault();
            string eqpid = (un.goaltype == (int)EqpGoalType.RDT) ? e.job.T_EQPID.Substring(0, 8) : e.job.T_EQPID.Split('-')[0];
            return Global.Init.RvComu.Send(eqpid, sndMsg, MethodBase.GetCurrentMethod().Name, e.vecID);
        }
        private (string, string) FSLoadMove_sndMsg_Create(int goaltype, SendJobToVecArgs1 e, string RvMsgName)
        {
            string sndMsg = string.Empty;
            string devTyp = string.Empty;
            string batchid = string.Empty;
            string eqpid = string.Empty;
            string mgid = string.Empty;
            string s_port = string.Empty;
            string s_slotno = string.Empty;
            string t_port = string.Empty;
            string t_slotno = string.Empty;

            if (e.job.WORKTYPE.Contains("FS"))
                batchid = string.Format($"P{e.job.WORKTYPE}_{e.job.BATCHID.Split('_')[1]}");
            else
                batchid = e.job.BATCHID;
            var pep = Db.Peps.Where(p => p.BATCHID == batchid).Single();

            eqpid = (pep.WORKTYPE == "MGO") ? pep.S_EQPID : pep.T_EQPID;

            mgid = (pep.TRAYID.Split(',').Count() > 1) ? string.Format($"({pep.TRAYID})") : pep.TRAYID;

            s_port = (pep.WORKTYPE == "MGO") ? pep.T_EQPID : pep.S_EQPID;
            s_slotno = (pep.WORKTYPE == "MGO") ? pep.T_SLOT : pep.S_SLOT;
            s_slotno = (s_slotno.Split(',').Count() > 1) ? string.Format($"({s_slotno})") : s_slotno;

            t_port = (pep.WORKTYPE == "MGO") ? pep.S_EQPID : pep.T_EQPID;
            t_slotno = (pep.WORKTYPE == "MGO") ? pep.S_SLOT : pep.T_SLOT;
            t_slotno = (t_slotno.Split(',').Count() > 1) ? string.Format($"({t_slotno})") : t_slotno;

            switch ((EqpGoalType)goaltype)
            {
                case EqpGoalType.FSSTK:
                    sndMsg = string.Format($"{RvMsgName.ToUpper()} HDR=(KDS1.LH.{eqpid.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={eqpid.Split('-')[0]} MGID={mgid} " +
                        $"BATCHJOBID={e.job.BATCHID} MULTIJOBID= SOURCEPORT={e.job.S_EQPID} SOURCESLOTNO={e.job.S_SLOT} DESTPORT={eqpid.Split('-')[0]} DESTSLOTNO={e.job.T_SLOT} " +
                        $"EXECUTETIME={e.job.EXECUTE_TIME} MRNO={e.vecID}");

                    devTyp = "FSSTK";
                    break;
                case EqpGoalType.RDT:
                    sndMsg = string.Format($"{RvMsgName.ToUpper()} HDR=({eqpid.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={eqpid.Split('-')[0]} SUBEQPID={eqpid} " +
                        $"MGID={mgid} BATCHJOBID={pep.BATCHID} MULTIJOBID= SOURCEPORT={s_port} SOURCESLOTNO={s_slotno} DESTPORT={t_port} DESTSLOTNO={t_slotno} " +
                        $"EXECUTETIME={pep.EXECUTE_TIME} MRNO={e.vecID}");


                    devTyp = "CHAMBER";
                    break;
            }

            return (sndMsg, devTyp);
        }
        public bool EQFSLoadStandby(int goaltype, SendJobToVecArgs1 e)
        {
            (string sndMsg, string devTyp) = FSLoadInfo_sndMsg_Create(goaltype, e, MethodBase.GetCurrentMethod().Name);

            unit_fs un = MainHandler.Init.Db.Units_FS.Where(p => p.GOALNAME == e.job.T_EQPID).SingleOrDefault();
            string eqpid = (un.goaltype == (int)EqpGoalType.RDT) ? e.job.T_EQPID.Substring(0, 8) : e.job.T_EQPID.Split('-')[0];
            return Global.Init.RvComu.Send(eqpid, sndMsg, MethodBase.GetCurrentMethod().Name, e.vecID);
        }
        public bool EQFSLoadInfo(int goaltype, SendJobToVecArgs1 e)
        {
            (string sndMsg, string devTyp) = FSLoadInfo_sndMsg_Create(goaltype, e, MethodBase.GetCurrentMethod().Name);

            unit_fs un = MainHandler.Init.Db.Units_FS.Where(p => p.GOALNAME == e.job.T_EQPID).SingleOrDefault();
            string eqpid = (un.goaltype == (int)EqpGoalType.RDT) ? e.job.T_EQPID.Substring(0, 8) : e.job.T_EQPID.Split('-')[0];
            return Global.Init.RvComu.Send(eqpid, sndMsg, MethodBase.GetCurrentMethod().Name, e.vecID);
        }
        private (string, string) FSLoadInfo_sndMsg_Create(int goaltype, SendJobToVecArgs1 e, string RvMsgName)
        {
            string sndMsg = string.Empty;
            string devTyp = string.Empty;
            string mgid = string.Empty;
            string msid = string.Empty;
            string trayid = string.Empty;
            string s_slotno = string.Empty;
            string t_slotno = string.Empty;

            s_slotno = (e.job.S_SLOT.Split(',').Count() > 1) ? string.Format($"({e.job.S_SLOT})") : e.job.S_SLOT;
            t_slotno = (e.job.T_SLOT.Split(',').Count() > 1) ? string.Format($"({e.job.T_SLOT})") : e.job.T_SLOT;
            trayid = (e.job.TRAYID.Split(',').Count() > 1) ? string.Format($"({e.job.TRAYID})") : e.job.TRAYID;
            
            switch ((EqpGoalType)goaltype)
            {
                case EqpGoalType.FSSTK:
                    sndMsg = string.Format($"{RvMsgName.ToUpper()} HDR=(KDS1.LH.{e.job.T_EQPID.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={e.job.T_EQPID.Split('-')[0]} MGID={trayid} " +
                        $"BATCHJOBID={e.job.BATCHID} MULTIJOBID= SOURCEPORT={e.job.S_EQPID} SOURCESLOTNO={e.job.S_SLOT} DESTPORT={e.job.T_EQPID.Split('-')[0]} DESTSLOTNO={e.job.T_SLOT} " +
                        $"EXECUTETIME={e.job.EXECUTE_TIME} MRNO={e.vecID}");

                    devTyp = "FSSTK";
                    break;
                case EqpGoalType.SYSWIN:
                    sndMsg = string.Format($"{RvMsgName.ToUpper()} HDR=({e.job.T_EQPID.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={e.job.T_EQPID.Split('-')[0]} SUBEQPID={e.job.T_EQPID} " +
                    $"MGID={trayid} LOTID={e.job.LOT_NO} SLOTID={t_slotno} EXECUTETIME={e.job.EXECUTE_TIME} MRNO={e.vecID} STEPID={e.job.T_STEPID} " +
                    $"JOBTYPE=LOAD");
                    break;
                default:
                    if (e.job.WORKTYPE == "FSI")
                    {
                        mgid = (e.job.S_EQPID.Split(',').Count() > 1) ? string.Format($"({e.job.S_EQPID})") : e.job.S_EQPID;

                        if (goaltype == (int)EqpGoalType.RDT)
                            sndMsg = string.Format($"{RvMsgName.ToUpper()} HDR=({e.job.T_EQPID.Substring(0,8)},LH.MPLUS,GARA,TEMP) EQPID={e.job.T_EQPID.Substring(0, 8)} SUBEQPID={e.job.T_EQPID} " +
                            $"MGID={mgid} MSID={s_slotno} FSID={trayid} SOURCEPORT=MRBP SOURCESLOTNO={s_slotno} DESTPORT={e.job.T_EQPID} " +
                            $"DESTSLOTNO={t_slotno} STEPID={e.job.T_STEPID} EXECUTETIME={e.job.EXECUTE_TIME} MRNO={e.vecID}");
                        else
                            sndMsg = string.Format($"{RvMsgName.ToUpper()} HDR=({e.job.T_EQPID.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={e.job.T_EQPID.Split('-')[0]} SUBEQPID={e.job.T_EQPID} " +
                            $"MGID={mgid} MSID={s_slotno} FSID={trayid} SOURCEPORT=MRBP SOURCESLOTNO={s_slotno} DESTPORT={e.job.T_EQPID} " +
                            $"DESTSLOTNO={t_slotno} STEPID={e.job.T_STEPID} EXECUTETIME={e.job.EXECUTE_TIME} MRNO={e.vecID}");
                    }
                    else
                    {
                        if (goaltype == (int)EqpGoalType.RDT)
                            sndMsg = string.Format($"{RvMsgName.ToUpper()} HDR=({e.job.T_EQPID.Substring(0, 8)},LH.MPLUS,GARA,TEMP) EQPID={e.job.T_EQPID.Substring(0, 8)} SUBEQPID={e.job.T_EQPID} " +
                            $"MGID={trayid} SOURCEPORT={e.job.S_EQPID} SOURCESLOTNO={s_slotno} DESTPORT={e.job.T_EQPID} " +
                            $"DESTSLOTNO={t_slotno} STEPID={e.job.T_STEPID} EXECUTETIME={e.job.EXECUTE_TIME} MRNO={e.vecID}");
                        else
                            sndMsg = string.Format($"{RvMsgName.ToUpper()} HDR=({e.job.T_EQPID.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={e.job.T_EQPID.Split('-')[0]} SUBEQPID={e.job.T_EQPID} " +
                            $"MGID={trayid} SOURCEPORT={e.job.S_EQPID} SOURCESLOTNO={s_slotno} DESTPORT={e.job.T_EQPID} " +
                            $"DESTSLOTNO={t_slotno} STEPID={e.job.T_STEPID} EXECUTETIME={e.job.EXECUTE_TIME} MRNO={e.vecID}");

                    }
                    devTyp = "CHAMBER";
                    break;
            }

            return (sndMsg, devTyp);
        }
#endregion
        // FSLoadInfo Method End


        // CHAMBER_TempDownSingleRequest Method Start
#region CHAMBER_TempDownSingleRequest

        /// <summary>
        /// EQPRETEMPDOWNREQ 수행후에도 실제 장비 작업전까지 온도증가예상된다. 보험으로 EQTEMPDOWNREQ 다시 때린다
        /// - for 문 2회에서 조건에 해당하는 경우 메시지 보내고 return, 실제 1회
        /// </summary>
        /// <param name="e"></param>
        /// <param name="srcUnit"></param>
        /// <param name="dstUnit"></param>
        /// <returns></returns>
        public bool CHAMBER_TempDownSingleRequest(string batchId, unit_fs srcUnit, unit_fs dstUnit, string vecID)
        {
            Task.Delay(1);

            CHAMBER_TempDownSingleRequest_lstTempDown_Add(srcUnit, dstUnit, vecID);

            if (RvMsgList[vecID].Rvmm.lstTempDown.Count() == 0)
            {
                return false;
            }

            lock (RvMsgList[vecID].Rvmm.syncTempDown)
            {
                for (int i = 0; i < RvMsgList[vecID].Rvmm.lstTempDown.Count(); i++)
                {
                    if (!CHAMBER_TempDownSingleRequest_sndMsg_Send(i, vecID))
                        return false;
                }
            }
            return WaitTempDownRvMessage(vecID);
        }
        private void CHAMBER_TempDownSingleRequest_lstTempDown_Add(unit_fs srcUnit, unit_fs dstUnit, string vecID)
        {
            if (RvMsgList[vecID].Rvmm.lstTempDown.Count() > 0)
            {
                RvMsgList[vecID].Rvmm.lstTempDown.Clear();
            }
            unit_fs TD_unit = (RvMsgList[vecID].JobType == "UNLOAD") ? srcUnit : dstUnit;

            if (TD_unit != null && TD_unit.goaltype != (int)EqpGoalType.FSSTK)
                RvMsgList[vecID].Rvmm.lstTempDown.Add(TD_unit.ID);
        }
        private bool CHAMBER_TempDownSingleRequest_sndMsg_Send(int i, string vecID)
        {
            unit_fs Target_unit = Db.Units_FS.Where(p => p.GOALNAME == RvMsgList[vecID].Rvmm.lstTempDown[i]).SingleOrDefault();
            if (Target_unit == null)
                return false;
            string eqpId = (Target_unit.goaltype == (int)EqpGoalType.RDT) ? RvMsgList[vecID].Rvmm.lstTempDown[i].Substring(0,8) : RvMsgList[vecID].Rvmm.lstTempDown[i].Split('-')[0];
            string sndMsg = string.Format($"EQTEMPDOWNREQ HDR=({eqpId},LH.MPLUS,GARA,TEMP) EQPID={eqpId} SUBEQPID={RvMsgList[vecID].Rvmm.lstTempDown[i]} JOBTYPE={RvMsgList[vecID].JobType} MRNO={vecID}");


            Logger.Inst.Write(vecID, CmdLogType.Rv, $"SYSWIN_TempDownRequest. List TempDown'Count is {RvMsgList[vecID].Rvmm.lstTempDown.Count()}, {RvMsgList[vecID].Rvmm.lstTempDown[i]}");

            if (!SendRvMessageNoWait(eqpId, sndMsg, vecID))
            {
                RvMsgList[vecID].Rvmm.lstTempDown.Clear();
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"Error. SYSWIN_TempDownRequest. {eqpId}'s msg sending fail.");
                return false;
            }
            RvMsgList[vecID].Rvmm.tempdown_check = false;
            return true;
        }
        private bool SendRvMessageNoWait(string eqpId, string sndMsg, string vecID)
        {
            RvMsgList[vecID].Rvmm.ResetFlag();

            return Global.Init.RvComu.Send(eqpId, sndMsg, MethodBase.GetCurrentMethod().Name, vecID);
        }
#endregion
        // CHAMBER_TempDownSingleRequest Method End

        // EQFSJOBCOMPCHECK Method Start
#region EQFSJOBCOMPCHECK
        public bool EQFSJOBCOMPCHECK(SendJobToVecArgs1 e)
        {
            try
            {
                bool bret = false;
                RvMsgList[e.vecID].Rvmm.ResetvehicleFlag();
                string subeqpid = EQFSJOBCOMPCHECK_subeqpid_Check(e);

                string status = (WaitVehicleJobCompMessage(ref bret, e.vecID)) ? "PASS" : "FAIL";
                if (!bret)
                    return bret;

                string isComp = (Db.IsJobCompCheck(RvMsgList[e.vecID].CurJob.EXECUTE_TIME, subeqpid, RvMsgList[e.vecID].CurJob.BATCHID) > 0) ? "BUSY" : "COMP";

                Thread.Sleep(2000);
                EQFSJOBCOMPCHECK_sndMsg_Send(e, subeqpid, status, isComp);
                return true;
            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(e.vecID, CmdLogType.Rv, $"error. EQTRAYJOBCOMPCHECK. {ex.Message}\r\n{ex.StackTrace}");
                RvMsgList[e.vecID].Rvmm._berror = false;
                return false;
            }
        }
        const string ERR_EQFSJOBCOMPCHECK = "error. EQFSJOBCOMPCHECK. RUNMODE is EMPTY.";

        private string EQFSJOBCOMPCHECK_subeqpid_Check(SendJobToVecArgs1 e)
        {
            string subeqpid = string.Empty;

            if (e.job.WORKTYPE.Contains("FS"))
            {
                    subeqpid = e.job.S_EQPID;
            }
            else
            {
                if (RvMsgList[e.vecID].JobType == "LOAD")
                    subeqpid = e.job.T_EQPID;
                else if (RvMsgList[e.vecID].JobType == "UNLOAD")
                    subeqpid = e.job.S_EQPID;
            }


            if (subeqpid == "")
                throw new UtilMgrCustomException("error. EQTRAYMOVEREQ. EQPID or SUBEQPID is no data.");

            return subeqpid;
        }
        private void EQFSJOBCOMPCHECK_sndMsg_Send(SendJobToVecArgs1 e, string subeqpid, string status, string isComp)
        {
            string batchid = string.Empty;
            string mgid = string.Empty;
            pepschedule pep = new pepschedule();

            unit_fs Target_Unit = Db.Units_FS.Where(p => p.GOALNAME == subeqpid).SingleOrDefault();
            if (Target_Unit == null)
                return;
            if (e.job.WORKTYPE.Contains("FS"))
            {
                batchid = string.Format($"P{e.job.WORKTYPE}_{e.job.BATCHID.Split('_')[1]}");
                pep = Db.Peps.Where(p => p.BATCHID.Contains(batchid)).FirstOrDefault();
                mgid = pep.TRAYID;
            }
            else
            {
                mgid = e.job.TRAYID;
            }


            if (mgid.Split(',').Count() > 1)
                mgid = string.Format($"({mgid})");

            string sndMsg = string.Empty;
            string jobtype_head = (e.job.WORKTYPE.Contains("FS")) ? "FS" : "MG";

            if (Target_Unit.goaltype == (int)EqpGoalType.SYSWIN)
            {
                sndMsg = string.Format($"EQFSJOBCOMPCHECK_REP HDR=({subeqpid.Split('-')[0]},LH.MPLUS,GARA,TEMP) STATUS={status} EQPID={subeqpid.Split('-')[0]} SUBEQPID={subeqpid} ");
                sndMsg += string.Format($"JOBCOMPCHECK={isComp} JOBTYPE={RvMsgList[e.vecID].JobType} ERRORCODE= ERRORMSG=");
            }
            else
            {
                if (Target_Unit.goaltype == (int)EqpGoalType.RDT)
                    sndMsg = string.Format($"EQFSJOBCOMPCHECK_REP HDR=({subeqpid.Substring(0, 8)},LH.MPLUS,GARA,TEMP) STATUS={status} EQPID={subeqpid.Substring(0, 8)} SUBEQPID={subeqpid} ");
                else
                    sndMsg = string.Format($"EQFSJOBCOMPCHECK_REP HDR=({subeqpid.Split('-')[0]},LH.MPLUS,GARA,TEMP) STATUS={status} EQPID={subeqpid.Split('-')[0]} SUBEQPID={subeqpid} ");
                sndMsg += string.Format($"MGID={mgid} JOBCOMPCHECK={isComp} JOBTYPE={jobtype_head}{RvMsgList[e.vecID].JobType} ERRORCODE= ERRORMSG=");
            }

            string eqpid = (Target_Unit.goaltype == (int)EqpGoalType.RDT) ? subeqpid.Substring(0, 8) : subeqpid.Split('-')[0];
            Global.Init.RvComu.Send(eqpid, sndMsg, MethodBase.GetCurrentMethod().Name, e.vecID);

        }
#endregion
        // EQFSJOBCOMPCHECK Method End

        // FSMGReturnInfoReq Method Start
#region FSMGReturnInfoReq
        public bool EQFSMGReturnInfoReq(int goaltype, SendJobToVecArgs1 e)
        {
            (string sndMsg, string devTyp) = FSMGReturnInfoReq_sndMsg_Create(goaltype, e, MethodBase.GetCurrentMethod().Name);

            unit_fs un = MainHandler.Init.Db.Units_FS.Where(p => p.GOALNAME == e.job.S_EQPID).SingleOrDefault();
            string eqpid = (un.goaltype == (int)EqpGoalType.RDT) ? e.job.S_EQPID.Substring(0, 8) : e.job.S_EQPID.Split('-')[0];
            return Global.Init.RvComu.Send(eqpid, sndMsg, MethodBase.GetCurrentMethod().Name, e.vecID);
        }
        private (string, string) FSMGReturnInfoReq_sndMsg_Create(int goaltype, SendJobToVecArgs1 e, string RvMsgName)
        {
            string sndMsg = string.Empty;
            string devTyp = string.Empty;
            string mgstatus = string.Empty;
            string mgid = string.Empty;

            if (e.job.TRAYID.Split(',').Count() > 1)
                mgid = string.Format($"({e.job.TRAYID})");
            else
                mgid = e.job.TRAYID;

            switch ((EqpGoalType)goaltype)
            {
                case EqpGoalType.AGING:
                    // JobCancel 후 진행 시 Load/Unload 판단이 불가하므로 Worktype으로 판별
                    if (e.job.WORKTYPE == "FSI")
                        mgstatus = "EMPTY";
                    else
                        mgstatus = "INPROCESS";

                    sndMsg = string.Format($"{RvMsgName.ToUpper()} HDR=({e.job.S_EQPID.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={e.job.S_EQPID.Split('-')[0]} SUBEQPID={e.job.S_EQPID} " +
                        $"MGID={mgid} MGSTATUS={mgstatus} MRNO={e.vecID}");

                    devTyp = "CHAMBER";
                    break;
                case EqpGoalType.RDT:
                    // JobCancel 후 진행 시 Load/Unload 판단이 불가하므로 Worktype으로 판별
                    if (e.job.WORKTYPE == "FSI")
                        mgstatus = "EMPTY";
                    else 
                        mgstatus = "INPROCESS";

                    sndMsg = string.Format($"{RvMsgName.ToUpper()} HDR=({e.job.S_EQPID.Substring(0,8)},LH.MPLUS,GARA,TEMP) EQPID={e.job.S_EQPID.Substring(0, 8)} SUBEQPID={e.job.S_EQPID} " +
                        $"MGID={mgid} MGSTATUS={mgstatus} MRNO={e.vecID}");

                    devTyp = "CHAMBER";
                    break;
                case EqpGoalType.SYSWIN:

                    sndMsg = string.Format($"{RvMsgName.ToUpper()} HDR=({e.job.S_EQPID.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={e.job.S_EQPID.Split('-')[0]} SUBEQPID={e.job.S_EQPID} " +
                        $"MGID={mgid} MGSTATUS=INPROCESS MRNO={e.vecID}");

                    devTyp = "SYSWIN";
                    break;
            }

            return (sndMsg, devTyp);
        }
#endregion
        // FSMGReturnInfoReq Method End

        // FSMGReturnComp Method Start
#region FSMGReturnComp
        public bool EQFSMGReturnComp(int goaltype, SendJobToVecArgs1 e)
        {
            (string sndMsg, string devTyp) = FSMGReturnComp_sndMsg_Create(goaltype, e, MethodBase.GetCurrentMethod().Name);

            return Global.Init.RvComu.Send(e.job.T_EQPID.Split('-')[0], sndMsg, MethodBase.GetCurrentMethod().Name, e.vecID);
        }
        private (string, string) FSMGReturnComp_sndMsg_Create(int goaltype, SendJobToVecArgs1 e, string RvMsgName)
        {
            string sndMsg = string.Empty;
            string devTyp = string.Empty;
            string mgstatus = string.Empty;
            string mgid = string.Empty;
            string sfid = string.Empty;

            if (e.job.TRAYID.Split(',').Count() > 1)
                mgid = string.Format($"({e.job.TRAYID})");
            else
                mgid = e.job.TRAYID;

            if (e.job.T_SLOT.Split(',').Count() > 1)
                sfid = string.Format($"({e.job.T_SLOT})");
            else
                sfid = e.job.T_SLOT;

            if (e.job.WORKTYPE == "FSI")
                mgstatus = "EMPTY";
            else
                mgstatus = "INPROCESS";

            sndMsg = string.Format($"{RvMsgName.ToUpper()} HDR=(KDS1.LH.{e.job.T_EQPID.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={e.job.T_EQPID.Split('-')[0]} " +
                $"MGID={mgid} SFID={sfid} MGSTATUS={mgstatus} MRNO={e.vecID}");

            devTyp = "FSSTK";

            return (sndMsg, devTyp);
        }
#endregion
        // FSMGReturnComp Method End

        // JOBCANCEL Method Start
#region JOBCANCEL
        public bool JOBCANCEL(SendJobToVecArgs1 e)
        {
            Task.Delay(1);

            string jobtype_msg = string.Empty;
            string eqpid = string.Empty;
            unit_fs cancel_unit = null;
            if (e.vecID == "PROGRAM")
                e.vecID = "VEHICLE01";

            if ((e.job.C_state < (int)CmdState.SRC_COMPLETE || e.job.C_state == null) || (e.job.WORKTYPE == "FSO" && e.job.BATCHID.Contains("CFS")))
            {
                jobtype_msg = "UNLOAD";
                eqpid = e.job.S_EQPID;
                cancel_unit = Db.Units_FS.Where(p => p.GOALNAME == eqpid).Single();

                if (cancel_unit.goaltype == (int)EqpGoalType.FSSTK)
                {
                    return JOBCANCEL_UnLoad_STK(e, jobtype_msg, eqpid, cancel_unit);
                }
                else
                {
                    return JOBCANCEL_UnLoad_Load_another(e, jobtype_msg, eqpid, cancel_unit);
                }
            }
            else
            {
                jobtype_msg = "LOAD";
                eqpid = e.job.T_EQPID.Split(',')[0];
                cancel_unit = Db.Units_FS.Where(p => p.GOALNAME == eqpid).Single();

                {
                    return JOBCANCEL_UnLoad_Load_another(e, jobtype_msg, eqpid, cancel_unit);
                }
            }
            return true;
        }
        private bool JOBCANCEL_UnLoad_STK(SendJobToVecArgs1 e, string jobtype_msg, string eqpid, unit_fs cancel_unit)
        {
            string sndMsg = JobCancel_Send_Message(e, jobtype_msg, ref eqpid, cancel_unit);

            if (!Global.Init.RvComu.Send(eqpid.Split('-')[0], sndMsg, MethodBase.GetCurrentMethod().Name, e.vecID))
                return false;

            return true;
        }
        public string JobCancel_Send_Message(SendJobToVecArgs1 e, string jobtype_msg, ref string eqpid, unit_fs cancel_unit)
        {
            string sndMsg = string.Empty;
                        
            if (cancel_unit.goaltype == (int)EqpGoalType.FSSTK)
            {
                sndMsg = string.Format($"EQFSJOBCANCEL HDR=({eqpid.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={eqpid.Split('-')[0]} MGID=({e.job.TRAYID})");
            }
            else if (cancel_unit.goaltype == (int)EqpGoalType.RDT)
            {
                sndMsg = string.Format($"EQFSJOBCANCEL HDR=({eqpid.Substring(0,8)},LH.MPLUS,GARA,TEMP) EQPID={eqpid.Substring(0, 8)} SUBEQPID={eqpid} MGID=({e.job.TRAYID})");
                eqpid = eqpid.Substring(0, 8);
            }
            else
            {
                sndMsg = string.Format($"EQFSJOBCANCEL HDR=({eqpid.Split('-')[0]},LH.MPLUS,GARA,TEMP) EQPID={eqpid.Split('-')[0]} SUBEQPID={eqpid} MGID=({e.job.TRAYID})");
                eqpid = eqpid.Split('-')[0];
            }

            sndMsg += string.Format($" JOBTYPE={jobtype_msg} BATCHJOBID={e.job.BATCHID} MULTIJOBID= STEPID={e.job.STEPID}");

            return sndMsg;
        }
        private bool JOBCANCEL_UnLoad_Load_another(SendJobToVecArgs1 e, string jobtype_msg, string eqpid, unit_fs cancel_unit)
        {
            string sndMsg = JobCancel_Send_Message(e, jobtype_msg, ref eqpid, cancel_unit);

            return Global.Init.RvComu.Send(eqpid, sndMsg, MethodBase.GetCurrentMethod().Name, e.vecID);
        }
#endregion
        // JOBCANCEL Method End
    }
}
