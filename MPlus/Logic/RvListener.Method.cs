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
    public partial class RvListener
    {
        // Global Start
        #region Global
        public bool IsControllerStop(string log_vecid)
        {
            var controller = Db.Controllers.SingleOrDefault();
            if (controller == null)
            {
                return false;
            }

            if (RobotList[log_vecid].isStop)
                return true;

            if (controller.C_state != (int)ControllerState.STOP)
                return false;

            return true;
        }

        #endregion
        //Global End


        // EQTRAYMOVECHECK_REP Method Start
        #region EQTRAYMOVECHECK_REP
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
                if (goalname == "FSSTK")
                {
                    goalname = "FSSTK-0";
                }
                else if (!goalname.Contains('-'))
                {
                    goalname += "-1";
                }
            }

            return Db.Units_FS.Where(p => p.GOALNAME == goalname).Single();
        }
        #endregion
        // EQTRAYMOVECHECK_REP Method End

        // EQTRAYMOVECHECK_REP_CHAMBER Method Start
        #region EQTRAYMOVECHECK_REP_CHAMBER
        const string ERR_MOVECHK_REP_CHAMBER_EQSTATUS = "error. EQFSMOVECHECK_REP_CHAMBER. EQSTATUS is not RUN and IDLE";
        const string ERR_MOVECHK_REP_CHAMBER_STATUS = "error. EQFSMOVECHECK_REP_CHAMBER. STATUS is PASS";
        const string ERR_MOVECHK_REP_CHAMBER_ESINFO = "error. EQFSMOVECHECK_REP_CHAMBER. ESINSO is Not EMPTY";
        const string ERR_MOVECHK_REP_CHAMBER_ESINFO_Empty = "error. EQFSMOVECHECK_REP_CHAMBER. ESINSO is EMPTY";
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

            string[] t_slotinfo = null;

            if (RvMsgList[vecID].CurJob.WORKTYPE == "MGO")
                t_slotinfo = RvMsgList[vecID].CurJob.S_SLOT.Split(',');
            else if (RvMsgList[vecID].CurJob.WORKTYPE == "MGI")
                t_slotinfo = RvMsgList[vecID].CurJob.T_SLOT.Split(',');
            else
            {
                string batchid = string.Format($"C{RvMsgList[vecID].CurJob.WORKTYPE}_{RvMsgList[vecID].CurJob.BATCHID.Split('_')[1]}");
                var peps = Db.Peps.Where(p => p.BATCHID.Contains(batchid)).ToList();

                string child_slotinfo = string.Empty;

                foreach (var x in peps)
                {
                    if (child_slotinfo.Length > 0)
                        child_slotinfo += ",";

                    if (x.WORKTYPE == "FSI")
                        child_slotinfo += x.T_SLOT;
                    else
                        child_slotinfo += x.S_SLOT;
                }

                t_slotinfo = child_slotinfo.Split(',');
            }


            for (int i = 0; i < t_slotinfo.Count(); i++)
            {
                int slotnum = Convert.ToInt32(t_slotinfo[i].Substring(2, t_slotinfo[i].Length - 2));

                if (RvMsgList[vecID].CurJob.WORKTYPE == "FSI" || RvMsgList[vecID].CurJob.WORKTYPE == "MGI")
                {
                    if (es_data[slotnum - 1] != "EMPTY")
                    {
                        throw new UtilMgrCustomException(ERR_MOVECHK_REP_CHAMBER_ESINFO);
                    }
                }
                else
                {
                    if (es_data[slotnum - 1] == "EMPTY")
                    {
                        throw new UtilMgrCustomException(ERR_MOVECHK_REP_CHAMBER_ESINFO_Empty);
                    }
                }
            }
        }
        private void CHAMBER_ESSTATUS_Check(string[] words, string vecID)
        {
            string testdata = string.Empty;
            if (RvMsgList[VehicleID].JobType == "LOAD")
                testdata = "IDLE";
            else if (RvMsgList[VehicleID].JobType == "UNLOAD")
                testdata = "RUN";

            string[] t_slotinfo = null;

            if (RvMsgList[vecID].CurJob.WORKTYPE == "MGO")
                t_slotinfo = RvMsgList[vecID].CurJob.S_SLOT.Split(',');
            else if (RvMsgList[vecID].CurJob.WORKTYPE == "MGI")
                t_slotinfo = RvMsgList[vecID].CurJob.T_SLOT.Split(',');
            else
            {
                string batchid = string.Format($"C{RvMsgList[vecID].CurJob.WORKTYPE}_{RvMsgList[vecID].CurJob.BATCHID.Split('_')[1]}");
                var peps = Db.Peps.Where(p => p.BATCHID.Contains(batchid)).ToList();

                string child_slotinfo = string.Empty;

                foreach (var x in peps)
                {
                    if (child_slotinfo.Length > 0)
                        child_slotinfo += ",";

                    if (x.WORKTYPE == "FSI")
                        child_slotinfo += x.T_SLOT;
                    else
                        child_slotinfo += x.S_SLOT;
                }

                t_slotinfo = child_slotinfo.Split(',');
            }


            string esstatus = UtilMgr.FindKeyStringToValue(words, "ESSTATUS");
            esstatus = Regex.Replace(esstatus, "[()]", "");
            string[] esstauts_data = esstatus.Split(',');

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
        // EQTRAYMOVECHECK_REP_CHAMBER Method End

        // EQTEMPDOWNREQ_REP Method Start
        #region EQTEMPDOWNREQ_REP
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
            lock (RvMsgList[vecID].Rvmm.syncTempDown)
            {
                int v = RvMsgList[vecID].Rvmm.lstTempDown.IndexOf(UtilMgr.FindKeyStringToValue(words, "SUBEQPID"));
                if (v >= 0)
                    RvMsgList[vecID].Rvmm.lstTempDown.RemoveAt(v);
            }

            if (RvMsgList[vecID].Rvmm.lstTempDown.Count() == 0)
            {
                RvMsgList[vecID].Rvmm._TempDownbSucc = true;
            }
        }

        #endregion
        // EQTEMPDOWNREQ_REP Method End

        // EQFSLOADINFO_REP/EQTRAYUNLOADINFO_REP Method Start
        #region EQFSLOADINFO_REP/EQTRAYUNLOADINFO_REP
        const string ERR_EQFSLOADINFO_REP_STATUS = "error. EQFSLOADINFO_REP. STATUS is not pass";
        const string ERR_EQFSUNLOADINFO_REP_STATUS = "error. EQFSUNLOADINFO_REP. STATUS is not pass";
        private void EQFSLOAD_UNLOADINFO_REP_Status_Check(string[] words, string vecID)
        {
            if (UtilMgr.FindKeyStringToValue(words, "STATUS") != "PASS")
            {
                LOAD_UNLOADINFO_Error(words[0]);
            }
            RvMsgList[vecID].Rvmm._bSucc = true;
        }
        private void LOAD_UNLOADINFO_Error(string word)
        {
            switch (word)
            {
                case "EQTRAYLOADINFO_REP":
                    throw new UtilMgrCustomException(ERR_EQFSLOADINFO_REP_STATUS);
                case "EQTRAYUNLOADINFO_REP":
                    throw new UtilMgrCustomException(ERR_EQFSUNLOADINFO_REP_STATUS);
                default:
                    break;
            }
        }

        #endregion
        // EQTRAYLOADINFO_REP/EQTRAYUNLOADINFO_REP Method End
        
        // EQTRAYMOVEREQ Method Start
        #region EQTRAYMOVEREQ
        private void EQTRAYMOVEREQ_sndMsg_Send(string[] words, string vecID)
        {
            string sndMsg = EQTRAYMOVEREQ_sndMsg_Create(words);

            Global.Init.RvComu.Send(UtilMgr.FindKeyStringToValue(words, "EQPID"), sndMsg, MethodBase.GetCurrentMethod().Name, vecID);
        }
        private string EQTRAYMOVEREQ_sndMsg_Create(string[] words)
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
                else if (words[i].Contains("EQTRAYMOVEREQ"))
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
        const string ERR_MOVECHK_REP_FSMGRETURNINFOREQ_REP_STATUS = "error. EQFSMGRETURNINFOREQ_REP. STATUS is PASS";
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
            if (slot == null || slot == "")
            {
                //throw new UtilMgrCustomException(ERR_MOVECHK_REP_FSMGRETURNINFOREQ_REP_SLOT);
            }
            //Global.Init.RvMsgList[vecID].CurJob.T_SLOT = slot;
            var pep = Db.Peps.Where(p => p.BATCHID == Global.Init.RvMsgList[vecID].CurJob.BATCHID).SingleOrDefault();

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
                Db.DbUpdate(TableType.PEPSCHEDULE);
            }
        }
        #endregion
        // EQFSMGRETURNINFOREQ_REP Method End

        // EQFSMGRETURNCOMP_REP Method Start
        #region EQFSMGRETURNCOMP_REP
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
}
