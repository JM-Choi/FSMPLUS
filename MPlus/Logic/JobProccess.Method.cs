using FSMPlus.Ref;
using FSMPlus.Vehicles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FSMPlus.Logic
{
    public partial class JobProccess
    {
        // CallCmdProcedure Method Start  
        #region CallCmdProcedure Method
        private void Job_Select(List<pepschedule> Jobs, ref bool IsMultiJob)
        {
            // 맨위에 Job의 WorkType이 FS 가 포함되어있을 때
            if (Jobs[0].WORKTYPE.Contains("FS"))
            {
                // BatchID에 PFS가 포함되어있는 Job을 가져온다.
                List<pepschedule> peps = Jobs.Where(p => p.BATCHID.Contains("PFS")).ToList();

                // 가져온 Job의 개수가 1개 이상이면 MultiJob 아니면 SingleJob
                if (peps.Count() > 1)
                    IsMultiJob = true;
                else
                    IsMultiJob = false;
            }
        }

        private void Job_ordering(ref List<pepschedule> Jobs, int I_type_remakcount, CallCmdProcArgs1 e)
        {
            if (Jobs[0].WORKTYPE == "FSI" || Jobs[0].WORKTYPE == "FSO")
            {
                int pfs_count = Jobs.Where(p => p.BATCHID.Contains("PFS")).Count();
                if (pfs_count > 1)
                    Jobs.Reverse();
            }
            else if (Jobs[0].WORKTYPE == "MGI" || Jobs[0].WORKTYPE == "MGO")
            {
                if (Jobs.Count() > 1)
                    Jobs.Reverse();
            }
        }
        // MultiJob 진행 시 사용 예정
        private bool SrcMultiJobDeleteCheck_MI(ref List<pepschedule> Jobs, CallCmdProcArgs1 e, ref int I_type_remakeJobcount, ref int I_type_foreachcount, ref int I_type_remakcount)
        {
            MultiSrcFinishArgs arglist = new MultiSrcFinishArgs();
            if (I_type_remakeJobcount > 1)
                arglist.pep = Jobs[I_type_foreachcount];
            else
                arglist.pep = Jobs[0];
            arglist.vec = e.vec;
            OnMultiSrcFinish(arglist, Jobs, I_type_remakcount);
            SrcChildJobUpdate(ref Jobs, e);
            I_type_remakcount--;
            I_type_foreachcount++;
            if (I_type_remakcount == 0)
            {
                for (int j = 0; j < Jobs.Count(); j++)
                {
                    if (Jobs[I_type_remakeJobcount - 1].BATCHID.Contains("MSRC"))
                    {
                        Db.CopyCmdToHistory(Jobs[I_type_remakeJobcount - 1]);
                        Db.Delete(Jobs[I_type_remakeJobcount - 1]);
                        Jobs.Remove(Jobs[I_type_remakeJobcount - 1]);
                        I_type_remakeJobcount--;
                        if (I_type_remakeJobcount == 0)
                            break;
                    }
                }
                return true;
            }
            return false;
        }

        // Parent Job에서 FSSTK에서 Magazine을 UNLOAD하고
        // 해당되는 Child Job에 Data를 공유 및 저장
        private bool SrcChildJobUpdate(ref List<pepschedule> Jobs, CallCmdProcArgs1 e)
        {
            List<pepschedule> peps = Jobs.Where(p=>p.BATCHID.Contains("PFS")).ToList();

            foreach (var pep in peps)
            {
                // Parent Job에 해당되는 Child Job 가져오기
                string batchid = string.Format($"C{pep.WORKTYPE}_{pep.BATCHID.Split('_')[1]}");
                var childjobs = Jobs.Where(p => p.BATCHID.Contains(batchid) && p.WORKTYPE == pep.WORKTYPE && p.REAL_TIME == pep.REAL_TIME).ToList();

                // Child Job에 필요 Data 저장
                foreach (var x in childjobs)
                {
                    x.C_VEHICLEID = pep.C_VEHICLEID;
                    x.C_srcArrivingTime = pep.C_srcArrivingTime;
                    x.C_srcAssignTime = pep.C_srcAssignTime;
                    x.C_srcStartTime = pep.C_srcStartTime;
                    x.C_srcFinishTime = pep.C_srcFinishTime;
                    x.C_state = pep.C_state;
                }
            }
            Db.DbUpdate(TableType.PEPSCHEDULE);

            return false;
        }

        // MultiJob 진행 시 사용 예정
        private void SrcMultiJobDeleteCheck_MOI_MEO_MTO(ref List<pepschedule> Jobs, CallCmdProcArgs1 e)
        {
            MultiSrcFinishArgs arglist = new MultiSrcFinishArgs();
            arglist.pep = Jobs[0];
            arglist.vec = e.vec;
            OnMultiSrcFinish(arglist, Jobs);
        }

        // MultiJob 진행 시 사용 예정
        private bool SrcMultiJobDeleteCheck_MOI(ref List<pepschedule> Jobs, CallCmdProcArgs1 e, ref int remakeJobcount, ref int foreachcount, ref int remakecount)
        {
            MultiSrcFinishArgs arglist = new MultiSrcFinishArgs();
            arglist.pep = Jobs[foreachcount];
            arglist.vec = e.vec;
            OnMultiSrcFinish(arglist, Jobs);
            remakecount--;
            foreachcount++;
            if (remakecount == 0)
            {
                for (int j = 0; j < Jobs.Count(); j++)
                {
                    if (Jobs[remakeJobcount - 1].BATCHID.Contains("MDST"))
                    {
                        Db.CopyCmdToHistory(Jobs[remakeJobcount - 1]);
                        Db.Delete(Jobs[remakeJobcount - 1]);
                        Jobs.Remove(Jobs[remakeJobcount - 1]);
                        remakeJobcount--;
                        if (remakeJobcount == 0)
                            break;
                    }
                }
                return true;
            }
            return false;
        }
#endregion
        // CallCmdProcedure Method End

        // OnMultiSrcFinish Method Start
#region OnMultiSrcFinish
        // MultiJob 진행 시 사용 예정
        private void multiJob_Delete(ref List<pepschedule> Jobs, int I_type_remakcount)
        {
            if (Jobs != null)
            {
                for (int i = 0; i < Jobs.Count(); i++)
                {
                    if (Jobs[i].WORKTYPE != "O" && Jobs[i].BATCHID.Contains("MSRC") && I_type_remakcount == 0)
                    {
                        Db.CopyCmdToHistory(Jobs[i]);
                        Db.Delete(Jobs[i]);
                        Jobs.Remove(Jobs[i]);
                    }
                }
            }
        }
        private string[] stack_tray_check(List<pepschedule> Jobs)
        {
            string[] job_tray_num = new string[Jobs.Count()];
            for (int i = 0; i < Jobs.Count(); i++)
            {
                job_tray_num[i] = Jobs[i].TRAYID;
            }
            return job_tray_num;
        }
        private string batchJob_tray_data(string[] _virPep, string[] trayid, string[] word)
        {
            string bufslot = string.Empty;
            for (int i = 0; i < trayid.Count(); i++)
            {
                if (i != 0 && bufslot != null && bufslot != "")
                    bufslot += ",";

                for (int j = 0; j < _virPep.Length; j++)
                {
                    if (trayid[i] == _virPep[j])
                    {
                        bufslot += word[(j * 2)] + ",";
                        bufslot += word[(j * 2) + 1];
                        break;
                    }
                }
            }
            return bufslot;
        }
        private string batchJob_stack_data(string[] _virPep, string[] trayid, string[] word, string[] job_tray_num)
        {
            string bufslot = string.Empty;
            int portarray = 0;
            for (int i = 0; i < trayid.Count(); i++)
            {
                if (bufslot.Length > 0)
                    break;

                for (int j = 0; j < _virPep.Length; j++)
                {
                    if (trayid[i] == _virPep[j])
                    {
                        for (int k = 0; k < job_tray_num.Count(); k++)
                        {
                            if (job_tray_num[k].Contains(trayid[i]))
                            {
                                portarray = k;
                                break;
                            }
                        }
                        bufslot += word[(portarray * 2)] + ",";
                        bufslot += word[(portarray * 2) + 1];
                        break;
                    }
                }
            }
            return bufslot;
        }
        private int tray_empty_check(pepschedule v)
        {
            int result = 0;
            for (int i = 0; i < v.C_bufSlot.Split(',').Count(); i++)
            {
                if (v.C_bufSlot.Split(',')[i] == "")
                {
                    result = i;
                    break;
                }
            }
            return result;
        }
        private string batchJob_tray_empty_select(pepschedule v, string[] c_bufslot, int emptyslotchk)
        {
            string bufslot = string.Empty;
            for (int i = 0; i < v.TRAYID.Split(',').Count(); i++)
            {
                for (int j = 0; j < _virtualPep.TRAYID.Split(',').Length; j++)
                {
                    if (v.TRAYID.Split(',')[i] == _virtualPep.TRAYID.Split(',')[j])
                    {
                        c_bufslot[emptyslotchk] += _virtualPep.C_bufSlot.Split(',')[(j * 2)] + ",";
                        c_bufslot[emptyslotchk] += _virtualPep.C_bufSlot.Split(',')[(j * 2) + 1];
                        emptyslotchk++;
                        break;
                    }
                }
            }

            for (int i = 0; i < c_bufslot.Count(); i++)
            {
                if (i != 0)
                    bufslot += ",";
                bufslot += c_bufslot[i];
            }
            return bufslot;
        }
#endregion
        // OnMultiSrcFinish Method End

        // ProcessSrcJob Method Start
#region ProcessSrcJob
        private bool SRCJobwhereSRCProcces(ref List<pepschedule> Jobs, List<MulGrp> mulgrplist, CallCmdProcArgs1 e, ref int remakecount, ref int I_type_remakcount)
        {
            SortMultiJobList1(ref mulgrplist, Jobs[0].S_EQPID);

            List<pepschedule> tempJobs = new List<pepschedule>();
            foreach (var w in mulgrplist)
            {
                if (!SRCremakejob_SRC_other(ref Jobs, ref tempJobs, e, Jobs[0].MULTIID, ref remakecount, ref I_type_remakcount, true, w))
                    return false;
            }

            if (tempJobs != null && tempJobs.Count() != 0)
            {
                Jobs.Clear();
                foreach (var z in tempJobs)
                {
                    Jobs.Add(z);
                }
            }

            if (!SRCremakejob_SRC_other(ref Jobs, ref tempJobs, e, Jobs[0].MULTIID, ref remakecount, ref I_type_remakcount, false))
                return false;

            return true;
        }

        private bool SRCJobwhereDSTProcces(ref List<pepschedule> Jobs, List<MulGrp> mulgrplist, CallCmdProcArgs1 e, ref int remakecount, ref int I_type_remakcount)
        {
            SortMultiJobList(ref mulgrplist, e.vec.ID);

            List<pepschedule> tempJobs = new List<pepschedule>();
            foreach (var w in mulgrplist)
            {
                SRCremakejob_DST_other(ref Jobs, ref tempJobs, e, Jobs[0].MULTIID, ref remakecount, w);
            }
            Jobs.Clear();
            foreach (var z in tempJobs)
            {
                Jobs.Add(z);
            }

            return true;
        }

        private bool SRCremakejob_SRC_other(ref List<pepschedule> Jobs, ref List<pepschedule> tempJobs, CallCmdProcArgs1 e, string multiId, ref int mulgrplist_count,
            ref int I_type_remakcount, bool foreach_chk = true, MulGrp w = null)
        {
            if (foreach_chk)
            {
                List<pepschedule> jobs = null;

                jobs = FindSubMultiJobList(Jobs, w, eMultiJobWhere.DST, e.executeTime);

                if (w.COUNT > 1)
                {
                    if (!RemakeMultiJobs(ref jobs, (int)eMultiJobWhere.DST, ref mulgrplist_count, true))
                        return false;

                    mulgrplist_count++;
                }

                foreach (var z in jobs)
                {
                    tempJobs.Add(z);
                }
            }
            else
            {
                int i_tray_over = Jobs[0].TRAYID.Split(',').Count();
                List<pepschedule> itemlist = Jobs;
                if (Jobs.Count() > 1)
                {
                    if (!RemakeMultiJobs(ref itemlist, (int)eMultiJobWhere.SRC, ref mulgrplist_count, false))
                        return false;

                    for (int i = 0; i < itemlist.Count(); i++)
                    {
                        itemlist[i].C_isChecked = 1;
                    }
                    foreach (var z in itemlist)
                    {
                        Jobs.Add(z);
                    }
                    I_type_remakcount = itemlist.Count();
                }
            }

            return true;
        }

        private bool SRCremakejob_DST_EO_TO(ref List<pepschedule> Jobs, CallCmdProcArgs1 e, string multiId, ref int mulgrplist_count)
        {
            List<pepschedule> itemlist = Jobs;
            unit_fs unit_chk = Db.Units_FS.Where(p => p.GOALNAME == itemlist[0].S_EQPID).SingleOrDefault();

            //if (Jobs.Count() > 1 && unit_chk.goaltype == (int)EqpGoalType.HANDLER_STACK)
            //{
            //    if (!RemakeMultiJobs(ref itemlist, (int)eMultiJobWhere.SRC, ref mulgrplist_count, false))
            //        return false;

            //    for (int i = 0; i < itemlist.Count(); i++)
            //    {
            //        itemlist[i].C_isChecked = 1;
            //    }
            //    foreach (var z in itemlist)
            //    {
            //        Jobs.Add(z);
            //    }
            //}


            return true;
        }

        private bool SRCremakejob_DST_other(ref List<pepschedule> Jobs, ref List<pepschedule> tempJobs, CallCmdProcArgs1 e, string multiId, ref int mulgrplist_count, MulGrp w = null)
        {
            List<pepschedule> jobs = null;

            jobs = FindSubMultiJobList(Jobs, w, eMultiJobWhere.SRC, e.executeTime);

            if (w.COUNT > 1)
            {
                if (!RemakeMultiJobs(ref jobs, (int)eMultiJobWhere.SRC, ref mulgrplist_count, true))
                    return false;

                mulgrplist_count++;
            }

            foreach (var z in jobs)
            {
                tempJobs.Add(z);
            }

            return true;
        }
#endregion
        // ProcessSrcJob Method End

        // ProcessDstJob Method Start
#region ProcessDstJob
        private bool DSTJobwhereSRCProcces(ref List<pepschedule> Jobs, List<MulGrp> mulgrplist, CallCmdProcArgs1 e, ref int mulgrplist_count)
        {
            List<pepschedule> itemlist = Jobs;
            if (Jobs.Count() > 1)
            {
                DSTremakejob_DST_other(ref Jobs, e, ref mulgrplist_count);
            }
            else
            {
                DSTremakejob_DST_O(ref Jobs, e, ref mulgrplist_count);
            }
            return true;
        }
        private bool DSTJobwhereDSTProcces(ref List<pepschedule> Jobs, List<MulGrp> mulgrplist, CallCmdProcArgs1 e, ref int mulgrplist_count)
        {
            List<MulGrp> itemlist = FindSubMultiJob(Jobs, eMultiJobWhere.DST);

            SortMultiJobList(ref itemlist, e.vec.ID);
            List<pepschedule> tempJobs = new List<pepschedule>();
            foreach (var w in itemlist)
            {
                Thread.Sleep(10);
                List<pepschedule> jobs = FindSubMultiJobList(Jobs, w, eMultiJobWhere.DST, e.executeTime);
                foreach (var v in jobs)
                {
                    tempJobs.Add(v);
                }
            }

            //unit unit_chk = Db.Units.Where(p => p.GOALNAME == tempJobs[0].T_EQPID).SingleOrDefault();
            //if (Jobs.Count() > 1 && (Jobs[0].WORKTYPE == "EI" || Jobs[0].WORKTYPE == "TI") && e.grp_count == 1 && unit_chk.goaltype == (int)EqpGoalType.HANDLER_STACK)
            //{
            //    Jobs.Clear();
            //    foreach (var z in tempJobs)
            //    {
            //        Jobs.Add(z);
            //    }

            //    tempJobs.Reverse();
            //    if (!RemakeMultiJobs(ref tempJobs, (int)eMultiJobWhere.SRC, ref mulgrplist_count, true))
            //        return false;
            //    for (int i = 0; i < tempJobs.Count(); i++)
            //    {
            //        tempJobs[i].C_isChecked = 1;
            //    }
            //    Jobs.Clear();
            //    foreach (var z in tempJobs)
            //    {
            //        Jobs.Add(z);
            //    }

            //}
            return true;
        }

        private bool DSTremakejob_DST_OI(ref List<pepschedule> Jobs, CallCmdProcArgs1 e, ref int mulgrplist_count, List<MulGrp> mulgrplist = null)
        {
            List<pepschedule> tempJobs = new List<pepschedule>();
            if (e.grp_count > 1)
            {
                // 현시점 x,y 에서 각 장비들의 위치를 따져서 path를 구하고 eqpGrp 를 ordering 한다
                SortMultiJobList1(ref mulgrplist, Jobs[0].S_EQPID);

                string multiId = string.Format("M{0}_{1}", Jobs[0].WORKTYPE.ToString(), DateTime.Now.ToString("ddHHmmssfff"));

                foreach (var w in mulgrplist)
                {
                    List<pepschedule> jobs = null;
                    jobs = FindSubMultiJobList(Jobs, w, eMultiJobWhere.DST, e.executeTime);

                    if (w.COUNT > 1)
                    {
                        if (!RemakeMultiJobs(ref jobs, (int)eMultiJobWhere.DST, ref mulgrplist_count, true))
                            return false;

                        mulgrplist_count++;
                    }

                    foreach (var v in jobs)
                    {
                        tempJobs.Add(v);
                    }
                }

                Jobs.Clear();
                foreach (var z in tempJobs)
                {
                    Jobs.Add(z);
                }
            }
            else
            {
                tempJobs = Jobs;
                if (!RemakeMultiJobs(ref tempJobs, (int)eMultiJobWhere.DST, ref mulgrplist_count, true))
                    return false;

                Jobs.Clear();
                foreach (var z in tempJobs)
                {
                    Jobs.Add(z);
                }
            }

            return true;
        }

        private bool DSTremakejob_DST_O(ref List<pepschedule> Jobs, CallCmdProcArgs1 e, ref int mulgrplist_count)
        {
            List<pepschedule> tempJobs = new List<pepschedule>();
            tempJobs = Jobs;
            int tray_count = Jobs[0].TRAYID.Split(',').Count();
            if (tray_count > 10)
            {
                if (!RemakeMultiJobs(ref tempJobs, (int)eMultiJobWhere.DST, ref mulgrplist_count, true, true))
                    return false;

                Jobs.Clear();
                foreach (var z in tempJobs)
                {
                    Jobs.Add(z);
                }
            }

            return true;
        }

        private bool DSTremakejob_DST_other(ref List<pepschedule> Jobs, CallCmdProcArgs1 e, ref int mulgrplist_count)
        {
            List<pepschedule> tempJobs = new List<pepschedule>();
            tempJobs = Jobs;
            if (!RemakeMultiJobs(ref tempJobs, (int)eMultiJobWhere.DST, ref mulgrplist_count, true))
                return false;

            Jobs.Clear();
            foreach (var z in tempJobs)
            {
                Jobs.Add(z);
            }

            return true;
        }
#endregion
        // ProcessDstJob Method End

        // MakePortSlot Method Start
#region MakePortSlot
        private void portslot_error_msg(string eqp_portslot, unit_fs units, string slot)
        {
            if (string.IsNullOrEmpty(eqp_portslot))
            {
                throw new UtilMgrCustomException($"{((EqpGoalType)units.goaltype).ToString()}. Invalid reflow Slot = {slot}");
            }

        }
#endregion
        // MakePortSlot Method End

        // makeportslot Method Start
#region makeportslot
        private (int, int) makeportslotNo(string words, int goaltype)
        {
            // 설비의 PortNo은 0으로 고정, SlotNo은 상위에서 내려준 PortSlot 이름에서 숫자만 사용
            int portNo = 0;
            int slotNo = 0;

            if (goaltype == (int)EqpGoalType.FSSTK)
                slotNo = Convert.ToInt32(words.Substring(2, words.Length - 2));
            else
                slotNo = Convert.ToInt32(words.Substring(1, words.Length - 1));

            return (portNo, slotNo);
        }
        private void make_eqpportslot(ref string eqp_portslot, int portNo, int slotNo)
        {
            // PortNo은 한자리로 SlotNo은 세자리로 하여 가공
            if (eqp_portslot != "")
                eqp_portslot += ",";
            eqp_portslot += portNo.ToString();
            eqp_portslot += ",";
            
            eqp_portslot += slotNo.ToString("D3");
        }
        private (int, int) makeportslotNo_SYSWIN(string words, int goaltype, string eqp)
        {
            // 설비의 PortNo은 0으로 고정, SlotNo은 상위에서 내려준 PortSlot 이름에서 숫자만 사용
            int portNo = 0;
            int slotNo = 0;

            unit_fs unit = Db.Units_FS.Where(p => p.goaltype == goaltype && p.GOALNAME == eqp).FirstOrDefault();

            if (unit != null)
            {
                byte[] ascPort = Encoding.ASCII.GetBytes(words.Substring(0, 1));// - 'A';
                byte[] ascBase = Encoding.ASCII.GetBytes("A");


                int row = Convert.ToInt32(words.Substring(1, words.Length - 1));

                slotNo = ascPort[0] - ascBase[0] + 1;   // portNo 는 1 Base 다

                if (row > 1)
                {
                    slotNo += (int)unit.max_col * (row - 1);
                }
            }


            return (portNo, slotNo);
        }
        #endregion
        // makeportslot Method End

        // MakeBuffSlot Method Start
        #region MakeBuffSlot
        private string bufslot_data(ref string buf_portslot, IEnumerable<vehicle_part> vecParts, pepschedule Jobs)
        {
            foreach (var part in vecParts)
            {
                // Magazine Type이 UFS 일 경우 그대로 사용
                if (Jobs.C_mgtype == "UFS")
                {
                    if (buf_portslot.Length > 0)
                        buf_portslot += ",";
                    buf_portslot += part.portNo.ToString();
                    buf_portslot += ",";
                    buf_portslot += ((int)part.slotNo).ToString("D3");
                }
                // Magazine Type이 UFS가 아닌 그 외 일 경우 한칸 건너뛰어 사용
                else
                {
                    if ((int)part.slotNo % 2 == 0)
                    {
                        if (buf_portslot.Length > 0)
                            buf_portslot += ",";
                        buf_portslot += part.portNo.ToString();
                        buf_portslot += ",";
                        buf_portslot += ((int)part.slotNo).ToString("D3");
                    }
                }
            }
            return buf_portslot;
        }
#endregion
        // MakeBuffSlot Method End

        // RemakeMultiJobs Method Start
#region RemakeMultiJobs
        private List<pepschedule> Job_remake(int where, ref int mulgrplist_count, bool tray_over, pepschedule pep, ref List<pepschedule> multijobs, bool bDelete, JobRemakeWords jobrewords)
        {
            int order_num = (int)multijobs[0].ORDER;
            List<pepschedule> newmultijobs = new List<pepschedule>();
            int buflimit, for_count = 0;

            StringDataSum(ref jobrewords, multijobs, bDelete);

            if (jobType != "DST")
                jobrewords.bufslot = pep.C_bufSlot;

            unit_fs unt = Db.Units_FS.Where(p => p.ID == ((where == (int)eMultiJobWhere.SRC) ? pep.S_EQPID : pep.T_EQPID)).Single();

            (buflimit, for_count) = lotation_check(where, pep, jobrewords.wordTrayIds);

            for (int i = 0; i < for_count; i++)
            {
                jobrewords.trayIds = string.Empty;
                jobrewords.lotNos = string.Empty;
                jobrewords.Qtys = string.Empty;
                jobrewords.teqpid = string.Empty;

                mulgrplist_count_check(ref mulgrplist_count, i, where, tray_over, pep);

                remakejob_wording(i, buflimit, where, pep, ref jobrewords, mulgrplist_count);

                rewordStepID_I_OandtrayoverX(ref jobrewords, multijobs);        


                if (tray_over)
                {
                    if (jobType == "DST")
                    {
                        DSTJobrewordStepID(ref jobrewords, multijobs);                        
                    }
                }
                remakePeps(pep, jobrewords, order_num, ref newmultijobs);                
            }

            if (bDelete)
            {
                foreach (var v in multijobs)
                {
                    v.MULTIID = pep.MULTIID;
                    v.C_dstArrivingTime = Vsp.DtCur;
                    Db.CopyCmdToHistory(v);
                    Db.Delete(v);
                    Logger.Inst.Write(VecId, CmdLogType.Rv, $"RemakeMultiJobs. OnDeleteCmd({pep.MULTIID})");
                }
            }
            Thread.Sleep(1000);
            return newmultijobs;
        }
        private void StringDataSum(ref JobRemakeWords jobrewords, List<pepschedule> multijobs, bool bDelete)
        {
            string totalTrayIds = string.Empty;
            string totalLotNos = string.Empty;
            string totalQtys = string.Empty;
            string totalSSlot = string.Empty;
            string totalTSlot = string.Empty;
            string totalStepIds = string.Empty;
            string totalSStepIds = string.Empty;
            string totalTStepIds = string.Empty;
            string totalbuffslot = string.Empty;
            string totalTEQPID = string.Empty;
            string totalmgtype = string.Empty;

            foreach (var v in multijobs)
            {
                StringAdd(ref totalTrayIds, v.TRAYID, ',');
                StringAdd(ref totalLotNos, v.LOT_NO, ',');
                StringAdd(ref totalQtys, v.QTY, ',');
                StringAdd(ref totalStepIds, v.STEPID, ',');
                StringAdd(ref totalSStepIds, v.S_STEPID, ',');
                StringAdd(ref totalTStepIds, v.T_STEPID, ',');

                if (bDelete == false)
                {
                    StringAdd(ref totalTEQPID, v.T_EQPID, ',');
                }

                jobrewords.chkunit_src = Db.Units_FS.Where(p => p.GOALNAME == ((pepschedule)v).S_EQPID).Single();
                StringAdd(ref totalSSlot, v.S_SLOT, ',');

                jobrewords.chkunit_dst = Db.Units_FS.Where(p => p.GOALNAME == ((pepschedule)v).T_EQPID).Single();
                if (!v.BATCHID.Contains("PFS"))
                    StringAdd(ref totalTSlot, v.T_SLOT, ',');
                

                if (jobType == "DST")
                {
                    StringAdd(ref totalbuffslot, v.C_bufSlot, ',');
                }

                StringAdd(ref totalmgtype, v.C_mgtype, ',');
            }

            if (totalSSlot == null || totalSSlot == "")
                totalSSlot = multijobs[0].S_SLOT;

            if (totalTSlot == null || totalTSlot == "")
                totalTSlot = multijobs[0].T_SLOT;

            if (totalTEQPID == null || totalTEQPID == "")
                totalTEQPID = multijobs[0].T_EQPID;

            jobrewords.wordTrayIds = totalTrayIds.Split(',');
            jobrewords.wordLotNos = totalLotNos.Split(',');
            jobrewords.wordQtys = totalQtys.Split(',');
            jobrewords.wordSSlot = totalSSlot.Split(',');
            jobrewords.wordTSlot = totalTSlot.Split(',');
            jobrewords.wordStepIds = totalStepIds.Split(',');
            jobrewords.wordSStepIds = totalSStepIds.Split(',');
            jobrewords.wordTStepIds = totalTStepIds.Split(',');
            jobrewords.wordbuffslot = totalbuffslot.Split(',');
            jobrewords.wordteqpid = totalTEQPID.Split(',');
            jobrewords.wordmgtype = totalmgtype.Split(',');
        }
        private (int, int) lotation_check(int where, pepschedule pep, string[] wordTrayIds)
        {
            unit_fs unt = Db.Units_FS.Where(p => p.ID == ((where == (int)eMultiJobWhere.SRC) ? pep.S_EQPID : pep.T_EQPID)).Single();
            int buflimit = (unt.goaltype == 1) ? (int)unt.max_row : (int)unt.max_row * (int)unt.max_col;

            int for_count = wordTrayIds.Count() / buflimit;
            int count_chk = wordTrayIds.Count() % buflimit;
            if (count_chk != 0)
                for_count += 1;

            return (buflimit, for_count);
        }
        private void mulgrplist_count_check(ref int mulgrplist_count, int i, int where, bool tray_over, pepschedule pep)
        {
            if (where == (int)eMultiJobWhere.SRC && i != 0)
                mulgrplist_count++;
            if (tray_over && i != 0)
                mulgrplist_count++;
            if (where == (int)eMultiJobWhere.DST && pep.WORKTYPE == "O" && i != 0)
                mulgrplist_count++;
        }
        private void remakejob_wording(int i, int buflimit, int where, pepschedule pep, ref JobRemakeWords jobrewords, int mulgrplist_count)
        {
            for (int j = i * buflimit; ((j < buflimit + (i * buflimit)) && (j < jobrewords.wordTrayIds.Count())); j++)
            {
                if (j == i * buflimit)
                {
                    remakeFirstStep(j, ref jobrewords, where, pep, mulgrplist_count);
                }
                else
                {
                    remakeNextStep(j, ref jobrewords, where, pep, mulgrplist_count);
                }
            }
        }
        private void remakeFirstStep(int j, ref JobRemakeWords jobrewords, int where, pepschedule pep, int mulgrplist_count)
        {
            const char delimeter = ',';
            string submultiId = string.Empty;

            if (pep.MULTIID != null)
                submultiId = pep.MULTIID.Split('_')[1];
            else
                submultiId = DateTime.Now.ToString("ddHHmmssfff");


            jobrewords.submultiId = string.Format("M{0}_{1}_{2}", (where == (int)eMultiJobWhere.SRC) ? "SRC" : "DST", submultiId, mulgrplist_count);

            jobrewords.trayIds = (jobrewords.wordTrayIds.Count() > j) ? jobrewords.wordTrayIds[j] : "";
            jobrewords.lotNos = (jobrewords.wordLotNos.Count() > j) ? jobrewords.wordLotNos[j] : "";
            jobrewords.Qtys = (jobrewords.wordQtys.Count() > j) ? jobrewords.wordQtys[j] : "";
            jobrewords.sslot = (jobrewords.wordSSlot.Count() > 0) ? jobrewords.wordSSlot[0] : "";
            jobrewords.tslot = (jobrewords.wordTSlot.Count() > 0) ? jobrewords.wordTSlot[0] : "";
            jobrewords.stepids = (jobrewords.wordStepIds.Count() > j) ? jobrewords.wordStepIds[j] : "";
            jobrewords.sstepids = (jobrewords.wordSStepIds.Count() > j) ? jobrewords.wordSStepIds[j] : ""; 
            jobrewords.tstepids = (jobrewords.wordTStepIds.Count() > j) ? jobrewords.wordTStepIds[j] : ""; 
            jobrewords.teqpid = (jobrewords.wordteqpid.Count() > j) ? jobrewords.wordteqpid[j] : "";

            jobrewords.mgtype = (jobrewords.wordmgtype.Count() > j) ? jobrewords.wordmgtype[j] : "";

            if (jobType == "DST")
            {
                jobrewords.bufslot = (jobrewords.wordbuffslot.Count() > (j * 2)) ? jobrewords.wordbuffslot[(j * 2)] : "";
                jobrewords.bufslot += string.Format("{0}{1}", delimeter, (jobrewords.wordbuffslot.Count() > (j * 2) + 1) ? jobrewords.wordbuffslot[(j * 2) + 1] : "");
            }
        }

        private void remakeNextStep(int j, ref JobRemakeWords jobrewords, int where, pepschedule pep, int mulgrplist_count)
        {
            const char delimeter = ',';
            jobrewords.trayIds += string.Format("{0}{1}", delimeter, (jobrewords.wordTrayIds.Count() > j) ? jobrewords.wordTrayIds[j] : "");
            jobrewords.lotNos += string.Format("{0}{1}", delimeter, (jobrewords.wordLotNos.Count() > j) ? jobrewords.wordLotNos[j] : "");
            jobrewords.Qtys += string.Format("{0}{1}", delimeter, (jobrewords.wordQtys.Count() > j) ? jobrewords.wordQtys[j] : "");
            if (jobrewords.chkunit_src.goaltype == (int)EqpGoalType.FSSTK)
                jobrewords.sslot += string.Format("{0}{1}", delimeter, (jobrewords.wordSSlot.Count() > j) ? jobrewords.wordSSlot[j] : "");

            if (jobrewords.chkunit_dst.goaltype == (int)EqpGoalType.RDT)
            {
                if (jobrewords.wordTSlot.Count() > j)
                    jobrewords.tslot += string.Format("{0}{1}", delimeter, (jobrewords.wordTSlot.Count() > j) ? jobrewords.wordTSlot[j] : "");
            }

            if (jobrewords.wordStepIds.Count() > j)
                jobrewords.stepids += string.Format("{0}{1}", delimeter, (jobrewords.wordStepIds.Count() > j) ? jobrewords.wordStepIds[j] : "");

            if (jobrewords.wordSStepIds.Count() > j)
                jobrewords.sstepids += string.Format("{0}{1}", delimeter, (jobrewords.wordSStepIds.Count() > j) ? jobrewords.wordSStepIds[j] : "");
            if (jobrewords.wordTStepIds.Count() > j)
                jobrewords.tstepids += string.Format("{0}{1}", delimeter, (jobrewords.wordTStepIds.Count() > j) ? jobrewords.wordTStepIds[j] : "");
           
            if (where == (int)eMultiJobWhere.DST)
            {
                if (jobrewords.wordteqpid.Count() > j)
                    jobrewords.teqpid += string.Format("{0}{1}", delimeter, (jobrewords.wordteqpid.Count() > j) ? jobrewords.wordteqpid[j] : "");
            }

            if (jobType == "DST")
            {
                jobrewords.bufslot += string.Format("{0}{1}", delimeter, (jobrewords.wordbuffslot.Count() > (j * 2)) ? jobrewords.wordbuffslot[(j * 2)] : "");
                jobrewords.bufslot += string.Format("{0}{1}", delimeter, (jobrewords.wordbuffslot.Count() > (j * 2) + 1) ? jobrewords.wordbuffslot[(j * 2) + 1] : "");
            }

            if (jobrewords.wordmgtype.Count() > j)
                jobrewords.mgtype += string.Format("{0}{1}", delimeter, (jobrewords.wordmgtype.Count() > j) ? jobrewords.wordmgtype[j] : "");
        }
        private void rewordStepID_I_OandtrayoverX(ref JobRemakeWords jobrewords, List<pepschedule> multijobs)
        {
            jobrewords.teqpid = string.Empty;
            jobrewords.stepids = string.Empty;
            jobrewords.sstepids = string.Empty;
            jobrewords.tstepids = string.Empty;
            string[] trayIds_count = jobrewords.trayIds.Split(',');
            bool teqpid_add = false;
            foreach (var l in multijobs)
            {
                string[] org_tray = l.TRAYID.Split(',');
                teqpid_add = false;
                foreach (var k in org_tray)
                {
                    foreach (var j in trayIds_count)
                    {
                        if (k == j)
                        {
                            if (jobrewords.teqpid != "")
                            {
                                jobrewords.teqpid += ",";
                                jobrewords.stepids += ",";
                                jobrewords.sstepids += ",";
                                jobrewords.tstepids += ",";
                            }
                            jobrewords.teqpid += l.T_EQPID;
                            jobrewords.stepids += l.STEPID;
                            jobrewords.sstepids += l.S_STEPID;
                            jobrewords.tstepids += l.T_STEPID;
                            teqpid_add = true;
                            break;
                        }
                    }
                    if (teqpid_add)
                        break;
                }
            }
        }
        private void DSTJobrewordStepID(ref JobRemakeWords jobrewords, List<pepschedule> multijobs)
        {
            jobrewords.stepids = string.Empty;
            jobrewords.sstepids = string.Empty;
            jobrewords.tstepids = string.Empty;
            string[] trayIds_count = jobrewords.trayIds.Split(',');
            bool stepids_add = false;

            foreach (var l in multijobs)
            {
                List<pepschedule_history> pepshis = Db.PepsHisto.Where(p => p.EXECUTE_TIME == l.EXECUTE_TIME && p.WORKTYPE == l.WORKTYPE).ToList();
                foreach (var u in pepshis)
                {
                    string[] org_tray = u.TRAYID.Split(',');
                    stepids_add = false;
                    foreach (var k in org_tray)
                    {
                        foreach (var j in trayIds_count)
                        {
                            if (k == j)
                            {
                                if (jobrewords.stepids != "")
                                {
                                    jobrewords.stepids += ",";
                                    jobrewords.sstepids += ",";
                                    jobrewords.tstepids += ",";
                                }
                                jobrewords.stepids += u.STEPID;
                                jobrewords.sstepids += u.S_STEPID;
                                jobrewords.tstepids += u.T_STEPID;
                                stepids_add = true;
                                break;
                            }
                        }
                        if (stepids_add)
                            break;
                    }
                }
            }
        }

        private void remakePeps(pepschedule pep, JobRemakeWords jobrewords, int order_num, ref List<pepschedule> newmultijobs)
        {
            pepschedule addjob = new pepschedule()
            {
                MULTIID = pep.MULTIID,
                BATCHID = jobrewords.submultiId,
                S_EQPID = pep.S_EQPID,
                S_PORT = pep.S_PORT,
                S_SLOT = jobrewords.sslot,
                T_EQPID = jobrewords.teqpid,
                T_PORT = pep.T_PORT,
                T_SLOT = jobrewords.tslot,
                C_mgtype = pep.C_mgtype,
                TRAYID = jobrewords.trayIds,
                WORKTYPE = pep.WORKTYPE,
                TRANSFERTYPE = pep.TRANSFERTYPE,
                WINDOW_TIME = pep.WINDOW_TIME,
                EXECUTE_TIME = pep.EXECUTE_TIME,
                REAL_TIME = pep.REAL_TIME,
                STATUS = pep.STATUS,
                LOT_NO = jobrewords.lotNos,
                QTY = jobrewords.Qtys,
                STEPID = jobrewords.stepids,
                S_STEPID = jobrewords.sstepids,
                T_STEPID = jobrewords.tstepids,
                URGENCY = pep.URGENCY,
                FLOW_STATUS = pep.FLOW_STATUS,
                C_VEHICLEID = pep.C_VEHICLEID,
                C_bufSlot = jobrewords.bufslot,
                C_state = pep.C_state,
                C_srcAssignTime = pep.C_srcAssignTime,
                C_srcArrivingTime = pep.C_srcArrivingTime,
                C_srcStartTime = pep.C_srcStartTime,
                C_srcFinishTime = pep.C_srcFinishTime,
                C_dstAssignTime = pep.C_dstAssignTime,
                C_dstArrivingTime = pep.C_dstArrivingTime,
                C_dstStartTime = pep.C_dstStartTime,
                C_dstFinishTime = pep.C_dstFinishTime,
                C_isChecked = pep.C_isChecked,
                C_priority = pep.C_priority,
                DOWNTEMP = pep.DOWNTEMP,
                EVENT_DATE = pep.EVENT_DATE,
                ORDER = order_num
            };
            Db.Add(addjob);
            Logger.Inst.Write(VecId, CmdLogType.Rv, $"RemakeMultiJobs. AddCmd({pep.MULTIID})");
            newmultijobs.Add(addjob);
        }
        #endregion
        // RemakeMultiJobs Method End

        private void MagazineReturnPeps(pepschedule pep, ref List<pepschedule> newmultijobs)
        {
            pepschedule addjob = new pepschedule()
            {
                MULTIID = pep.MULTIID,
                BATCHID = pep.BATCHID + "_0",
                S_EQPID = pep.T_EQPID,
                S_PORT = pep.T_PORT,
                S_SLOT = pep.T_SLOT,
                T_EQPID = pep.S_EQPID,
                T_PORT = pep.S_PORT,
                T_SLOT = pep.S_SLOT,
                C_mgtype = pep.C_mgtype,
                TRAYID = pep.TRAYID,
                WORKTYPE = pep.WORKTYPE,
                TRANSFERTYPE = pep.TRANSFERTYPE,
                WINDOW_TIME = pep.WINDOW_TIME,
                EXECUTE_TIME = pep.EXECUTE_TIME,
                REAL_TIME = pep.REAL_TIME,
                STATUS = pep.STATUS,
                LOT_NO = pep.LOT_NO,
                QTY = pep.QTY,
                STEPID = pep.STEPID,
                S_STEPID = pep.S_STEPID,
                T_STEPID = pep.T_STEPID,
                URGENCY = pep.URGENCY,
                FLOW_STATUS = pep.FLOW_STATUS,
                C_VEHICLEID = pep.C_VEHICLEID,
                C_bufSlot = pep.C_bufSlot,
                C_state = pep.C_state,
                C_srcAssignTime = pep.C_srcAssignTime,
                C_srcArrivingTime = pep.C_srcArrivingTime,
                C_srcStartTime = pep.C_srcStartTime,
                C_srcFinishTime = pep.C_srcFinishTime,
                C_dstAssignTime = pep.C_dstAssignTime,
                C_dstArrivingTime = pep.C_dstArrivingTime,
                C_dstStartTime = pep.C_dstStartTime,
                C_dstFinishTime = pep.C_dstFinishTime,
                C_isChecked = pep.C_isChecked,
                C_priority = pep.C_priority,
                DOWNTEMP = pep.DOWNTEMP,
                EVENT_DATE = pep.EVENT_DATE,
                ORDER = pep.ORDER
            };
            Db.Add(addjob);
            Logger.Inst.Write(VecId, CmdLogType.Rv, $"RemakeMultiJobs. AddCmd({pep.MULTIID})");
            newmultijobs.Clear();
            newmultijobs.Add(addjob);

            //Db.CopyCmdToHistory(pep);
            //Db.Delete(pep);
            
            Thread.Sleep(1000);
        }
    }
}
