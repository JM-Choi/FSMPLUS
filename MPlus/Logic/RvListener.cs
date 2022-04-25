using FSMPlus.Ref;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TIBCO.Rendezvous;
using System.Diagnostics;
using System.Threading;
using tech_library.Tools;
using System.Text.RegularExpressions;
using System.Reflection;

namespace FSMPlus.Logic
{
    public class EventArgRvtate : EventArgs
    {
        public RvState state;
    }
    public class CallVehicleID : EventArgs
    {
        public string vehicleid;
    }

    public partial class RvListener : Global
    {
        #region singleton RvListener
        private static volatile RvListener instance;
        private static object syncRv = new object();
        public static RvListener Init
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRv)
                    {
                        if (instance == null)
                            instance = new RvListener();
                    }
                }
                return instance;
            }
        }
        #endregion
        
        public RvListener()
        {
        }
        public string VehicleID = string.Empty;
        public RvListener(string vecID) : this()
        {
            VehicleID = vecID;
        }

        /// <summary>
        /// EQTRAYMOVECHECK 의 REP 의 처리메인
        /// </summary>
        /// <param name="words"></param>
        public void EQFSMOVECHECK_REP(string msg, string[] words, string vecID)
        {
            unit_fs gtype = MOVECHECK_GoalName_Unit_Check(msg, words);

            if ((int)EqpGoalType.RDT == gtype.goaltype)
            {
                EQFSMOVECHECK_REP_CHAMBER(words, vecID);
            }
            else if ((int)EqpGoalType.FSSTK == gtype.goaltype)
            {
                RvMsgList[vecID].Rvmm._bSucc = true;
            }
        }
        private void EQFSMOVECHECK_REP_CHAMBER(string[] words, string vecID)
        {
            try
            {
                CHAMBER_EQSTATUS_Check(words);

                CHAMBER_STATUS_Check(words);

                //CHAMBER_ESINFO_Check(words, vecID);

                //CHAMBER_ESSTATUS_Check(words, vecID);

                RvMsgList[vecID].Rvmm._bSucc = true;
                
                return;
            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQFSMOVECHECK_REP_CHAMBER. {ex.Message}\r\n{ex.StackTrace}");
                RvMsgList[vecID].Rvmm._berror = false;
            }
            RvMsgList[vecID].Rvmm._bWait = true;
        }
        public void EQFSLOADInfoSet(string[] words, string vecID)
        {
            try
            {
                RvMsgList[vecID].Rvmm._bSucc = true;
                return;

            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQFSUNLOADCOMPLETE. {ex.Message}\r\n{ex.StackTrace}");
                RvMsgList[vecID].Rvmm._berror = false;
            }
            RvMsgList[vecID].Rvmm._bWait = true;

        }
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
                RvMsgList[vecID].Rvmm._TempDownberror = true;
            }
            RvMsgList[vecID].Rvmm._TempDownbWait = true;

        }
        public void EQDOOROPENED(string[] words, string vecID)
        {
            try
            {
                RvMsgList[vecID].Rvmm._bSucc = true;
                return;
            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQDOOROPENED. {ex.Message}\r\n{ex.StackTrace}");
                RvMsgList[vecID].Rvmm._berror = true;
            }
            RvMsgList[vecID].Rvmm._bWait = true;
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
                RvMsgList[vecID].Rvmm._berror = true;
            }
            RvMsgList[vecID].Rvmm._bWait = true;

        }
        public void EQFSUNLOADCOMPLETE(string[] words, string vecID)
        {
            try
            {
                RvMsgList[vecID].Rvmm._bSucc = true;
                return;

            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQFSUNLOADCOMPLETE. {ex.Message}\r\n{ex.StackTrace}");
                RvMsgList[vecID].Rvmm._berror = false;
            }
            RvMsgList[vecID].Rvmm._bWait = true;

        }
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
                RvMsgList[vecID].Rvmm._berror = true;
            }
            RvMsgList[vecID].Rvmm._bWait = true;

        }
        public void EQFSLOADCOMPLETE(string msg, string[] words, string vecID)
        {
            try
            {
                RvMsgList[vecID].Rvmm._bSucc = true;
                return;

            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQFSLOADCOMPLETE. {ex.Message}\r\n{ex.StackTrace}");
                RvMsgList[vecID].Rvmm._berror = false;
            }
            RvMsgList[vecID].Rvmm._bWait = true;
        }
        public void EQTRAYMOVEREQ(string[] words, string vecID)
        {
            try
            {
                EQTRAYMOVEREQ_sndMsg_Send(words, vecID);
            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQFSMOVEREQ. {ex.Message}\r\n{ex.StackTrace}");
                RvMsgList[vecID].Rvmm._berror = false;
            }
        }
        public void EQFSMGRETURNINFOREQ_REP(string[] words, string vecID)
        {
            try
            {
                EQFSMGRETURNINFOREQ_REP_STATUS_Check(words);
                EQFSMGRETURNINFOREQ_REP_Slot_Check(words, vecID);
                RvMsgList[vecID].Rvmm._bSucc = true;
            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQFSMGRETURNINFOREQ. {ex.Message}\r\n{ex.StackTrace}");
                RvMsgList[vecID].Rvmm._berror = false;
            }
        }
        public void EQFSMGRETURNCOMP_REP(string[] words, string vecID)
        {
            try
            {
                EQFSMGRETURNCOMP_REP_STATUS_Check(words);
                RvMsgList[vecID].Rvmm._bSucc = true;
            }
            catch (UtilMgrCustomException ex)
            {
                Logger.Inst.Write(vecID, CmdLogType.Rv, $"error. EQFSMGRETURNCOMP. {ex.Message}\r\n{ex.StackTrace}");
                RvMsgList[vecID].Rvmm._berror = false;
            }
        }
    }

}
