using FSMPlus.Ref;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FSMPlus.Logic
{
    public class UtilMgrCustomException : Exception
    {
        public UtilMgrCustomException(string msg) : base(msg)
        {
        }
    }

    public class VSP : Global
    {
        #region singleton VSP
        private static volatile VSP instance;
        private static object syncVsp = new object();
        public static VSP Init
        { get
            { if (instance == null)
                { lock (syncVsp)
                    { instance = new VSP();
                    }
                }
                return instance;
            }
        }
        #endregion
        #region property
        public bool IsStop { get; set; }                    /* Schedule 스레드를 종료하기 위한 플래그           */

        public string ExecuteTime { get; set; }             /* 실행 선택된 executeTime                         */

        public List<pepschedule> FSJobs { get; set; }         /* 선택된 executeTime에 해당하는 모든 pep_job       */        

        public List<MulGrp> GroupList { get; set; }

        public eMultiJobWhere MultiJobWhere { get; set; }   /* MultiJob Grouping 위치 src, dst - 1 : src grouping, 2 : dst grouping */

        public DateTime DtOld { get; set; }                 /* AllDbUpdate 수행한 과거 시간                    */

        public DateTime DtCur { get; set; }                 /* 현재시간                                        */

        private VertualPep _virtualPep = new VertualPep();
        #endregion property

        #region event
        public event EventHandler<CallCmdProcArgs1> OnCallCmdProcedure;
        public event EventHandler<SendPreTempDown> OnSendPreTempDown;
        #endregion event
        // Schedule() 루틴이 비정상 탈출될 때, 재시작하기 위한 flag
        public VSP()
        {
            IsStop = false;
        }

        public async Task StartVsp()
        {
            while (!IsStop)
            {
                await Schedule();
                await Task.Delay(5000);
            }
        }

        public List<vehicle> VehicleIsAssigned()
        {
            // Robot 중 Assign이 되지않고 사용가능한 Robot이며 Installed 상태이고 Mode가 AUTO이고 TransferType이 FS인 Robot만 가져온다.
            List<vehicle> vecs = Db.Vechicles.Where(p => p.isAssigned == 0 && p.isUse == 1 && p.installState == (int)VehicleInstallState.INSTALLED 
                                                        //&& p.C_mode == (int)VehicleMode.AUTO 
                                                        && p.TRANSFERTYPE == "FS" && p.C_chargeRate > Cfg.Data.ChargeStart)
                                             .ToList();
            List<vehicle> stop_vec = new List<vehicle>();

            stop_vec.Clear();
            // 가져온 Robot 중 개별 Stop이 되어있는 Robot이 있는지 확인
            foreach (var x in vecs)
            {
                // 개별 Stop이 아니면 Add
                if (!RobotList[x.ID].isStop)
                    stop_vec.Add(x);
            }

            // 개별 Stop이 아닌 Robot이 있으면
            if (stop_vec != null)
            {
                // vecs에 Add
                vecs.Clear();
                foreach(var z in stop_vec)
                {
                    vecs.Add(z);   
                }
            }

            return vecs;
        }

        public async Task Schedule()
        {
            Logger.Inst.Write(CmdLogType.Db, $"작업 스케쥴링 시작");

            List<vehicle> vecs = new List<vehicle>();
            PepsWorkType curWorkType = PepsWorkType.NONE;

            int scheduled_delay = Cfg.Data.ScheduledDelay;  // msec
            bool is_scheduled_delay = (scheduled_delay == 0) ? false : true;

            try
            {
                while (!IsStop)
                {
                    await Task.Delay(100);

                    // 프로그램 내부에서 Delete, Update, Add 된 DB를 적용
                    if (DbRefresh()) continue;
                    // 외부에서 DB에 적용한 Pepschedule 내용을 프로그램과 동기화
                    Db.UpdateSchedule();
                    // 외부에서 DB에 적용한 Error 내용을 프로그램과 동기화
                    Db.UpdateError();
                    // Mplus의 상태가 Auto 인지 확인
                    // Pause/Stop의 경우 continue
                    if (!IsControllerAuto()) continue;

                    // PreTempDown 실행하는 함수
                    if (!PreProcess()) continue;

                    // 사용할 수 있는 Robot 확인, Job이 실행될 시간이 되었는지 확인, Job 선정을 하는 함수
                    (bool bret, bool bdelay) = CyclicProcessing(ref curWorkType, ref vecs, ref scheduled_delay, ref is_scheduled_delay);
                    if (!(bret && bdelay))
                    {
                        continue;
                    }

                    // 선정된 Job의 Worktype을 기준으로 MultiJob 기준이 Src인지 Dst인지 검사
                    SelectMultiJob(curWorkType);

                    // 진행될 작업 설비와 가까운 Robot 선정, JobProccess로 이벤트 전달 함수
                    AssingGrplst(vecs);
                }
            }
            catch (Exception ex)
            {

            }
        }
               
        void FSVehicle_GoDock_Check()
        {
            // 현재시간을 Unix Time으로 변경
            Int32 udtTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            // DB의 Job 중 RealTime이 현재시간 * Config에서 지정된 시간 보다 작고 TransferType이 FS이고 isChecked가 1이 아닌 Job을 RealTime으로 Ordering하여 저장
            List<pepschedule> FS_job = Db.Peps.Where(p => Convert.ToInt32(p.REAL_TIME) < (udtTime + Cfg.Data.PassedTime) && p.TRANSFERTYPE == "FS" && p.C_isChecked != 1)
                .OrderBy(p => p.REAL_TIME).ToList();

            // 저장된 Job중 필요한 Job만 저장
            (List<pepschedule> collect_job, int count) = FS_Job_Collect(FS_job);

            // Robot 중 TransferType이 FS인 Robot만 DB에서 가져오기
            List<vehicle> FS_vec = Db.Vechicles.Where(p => p.TRANSFERTYPE == "FS").ToList();

            // 저장된 Robot 중 필요한 Robot만 저장
            List<vehicle> collect_vec = FS_Vec_Collect(FS_vec);

            // 해당 Job과 해당 Robot을 충전소로 보내는 함수
            FS_Vec_GoDock_Check(collect_job, collect_vec, count);
        }

        (List<pepschedule>, int) FS_Job_Collect(List<pepschedule> tray_job)
        {
            List<pepschedule> collect_job = new List<pepschedule>();
            int count = 0;
            string real = string.Empty;
            string work = string.Empty;
            // 선정된 Job 중 RealTime과 Worktype을 한가지씩 collect_job에 저장
            foreach (var x in tray_job)
            {
                if (x.REAL_TIME != real || x.WORKTYPE != work)
                {
                    // 저장된 Job의 RealTime과 Worktype을 저장하여 동일하면 저장하지않도록하는 변수
                    real = x.REAL_TIME;
                    work = x.WORKTYPE;
                    count++;
                    collect_job.Add(x);
                }
            }
            return (collect_job, count);
        }

        List<vehicle> FS_Vec_Collect(List<vehicle> tray_vec)
        {
            List<vehicle> collect_vec = new List<vehicle>();
            foreach (var z in tray_vec)
            {
                // 선정된 Robot 중 Job을 완료하고 충전소로 이동하지않은 Robot만 저장
                if (RobotList[z.ID].JobAssign)
                {
                    collect_vec.Add(z);
                }
            }

            return collect_vec;
        }

        void FS_Vec_GoDock_Check(List<pepschedule> collect_job, List<vehicle> collect_vec, int count)
        {            
            if (collect_vec.Count() > 0 && count == 0)
            {
                // Robot에게 충전소 이동 명령을 내리는 함수
                VehicleGoDock("FS");
            }
        }

        (bool bret, bool bdelay) CyclicProcessing(ref PepsWorkType worktype, ref List<vehicle> veclist, ref int delay, ref bool isDelay)
        {
            List<pepschedule> peps = Db.Peps.ToList();
            start_chk stc = new start_chk();
            // 사용가능한 Robot List 저장
            veclist = VehicleIsAssigned();
                        
            // ExecuteTime이 Null 또는 Empty이면
            if (string.IsNullOrEmpty(ExecuteTime))
            {
                // 현재 DB에 저장되어있는 Job 중 TransferType이 FS인 Job만 Class에 저장
                stc.JobtypeSel(ref peps, "FS");
                // TransferType이 FS인 Job중 가장 빠른 ExecuteTime을 선정
                ExecuteTime = Db.Select_ExcuteTime(ref veclist, Cfg.Data.early_Time, peps);
                // 사용가능한 Robot이 있는지
                // ExecuteTime이 현재 시간에 도달해 있는지
                // 확인하여 Job을 선정
                if (!stc.isstart_chk(veclist, ExecuteTime, ref peps))
                {
                    // 1가지라도 안 맞는다면 초기화
                    ExecuteTime = default;
                    // Robot을 충전소로 이동시키는 함수
                    FSVehicle_GoDock_Check();
                    return (false, false);
                }
                else
                {
                    // Job이 선정되었다면 FSJobs 변수에 저장
                    if (FSJobs != null)
                        FSJobs.Clear();
                    FSJobs = peps;
                    // Worktype을 문자에서 숫자로 변경
                    worktype = Global.Init.Db.GetWorkTypeStringToEnum(FSJobs[0].WORKTYPE);
                }
            }
            else
            {
            }

            // 선정된 Job의 설비, 설비 Slot 매칭 검사
            if (!FetchJob()) return (false, false);

            return (true, true);
        }

        private void AssingGrplst(List<vehicle> vecs)
        {
            // 작업이 진행될 설비와 가장 가까운 Robot 선정하는 함수
            TempNearestGoalAndVehicleStruct v = SelectVehicleNearbyToUnitS(FSJobs, vecs);
            // MultiJob의 수 저장
            int group_count = GroupList.Count();

            // 선정된 Job이 사용중이라는 Flag를 DB에 저장
            foreach (var j in FSJobs)
            {
                j.C_isChecked = 1;
            }
            Db.DbUpdate(TableType.PEPSCHEDULE);
            // FS 일 경우 Parents Job 기준으로 진행하기 위한 변수
            List<pepschedule> eventJobs = new List<pepschedule>();                        
            foreach (var x in FSJobs)
            {
                eventJobs.Add(Db.Peps.Where(p => p.BATCHID == x.BATCHID).SingleOrDefault());
            }

            foreach (var g in FSJobs)
            {
                // Job의 Src 설비 명과 가까운 Robot 선정시 Src 설비 명이 같으면 진행
                if (string.Compare(g.S_EQPID, v.goalName) == 0)
                {
                    RobotList[v.vec.ID].JobAssign = false;
                    vecs.Remove(v.vec);
                    // Jobproccess로 Data 전달
                    OnCallCmdProcedure.Invoke(this, new CallCmdProcArgs1
                    {
                        where = MultiJobWhere,
                        vec = v.vec,
                        grp = GroupList[0],
                        executeTime = g.EXECUTE_TIME,
                        realTime = ExecuteTime,
                        grp_count = group_count,
                        peps = eventJobs
                    });                    
                    GroupList.Remove(GroupList[0]);
                    break;
                }
            }
            Thread.Sleep(1000);

            FSJobs.Clear();
            v.vec.isAssigned = 1;
            Db.DbUpdate(TableType.VEHICLE);
        }



        // jm.choi - 190418
        // Assign된 Job이 완료가 되고 다음 Job이 없거나 5분안에 없으면 Vehicle을 Dock으로 보냄
        // JobAssign은 해당 Vehicle이 JobAssign이 되었던 적이 있는지를 확인하는 Flag이며
        // Assign된 Job을 완료하면 Vehicle의 isAssigned는 0으로 변경됨
        // JobAssign == true고 isAssigned가 1이면 Job이 Assign 되었으나 완료된 Vehicle이 되며
        // 그 Vehicle을 Dock으로 보냄
        private bool VehicleGoDock(string transfer = null)
        {
            // 전달된 Transfer에 따라 Robot Data를 DB에서 가져오기
            var vec_dock_count = Db.Vechicles.Where(p => p.isUse == 1 && (transfer != null) ? (p.TRANSFERTYPE == transfer) : (p.TRANSFERTYPE != transfer)).ToList();

            // 가져온 Robot 이 없으면 return
            if (vec_dock_count == null && vec_dock_count.Count() == 0)
            {
                return false;
            }
            // 가져온 Robot의 수만큼 반복
            for (int i = 0; i < vec_dock_count.Count(); i++)
            {
                // Robot이 Job 완료후 충전으로 이동하지않았으면
                if (RobotList[vec_dock_count[i].ID].JobAssign)
                {
                    // Robot의 상태가 NotAssign 이면
                    if (vec_dock_count[i].C_state == 0)
                    {
                        // Robot에 CHARGE 명령 전송
                        RobotList[vec_dock_count[i].ID].CHARGE();
                        // Robot이 Job 완료 Flag 초기화
                        RobotList[vec_dock_count[i].ID].JobAssign = false;
                        // Robot의 JobAssign 초기화
                        vec_dock_count[i].isAssigned = 0;
                        Db.DbUpdate(TableType.VEHICLE);
                    }
                }
            }
            return true;
        }
        #region db refresh

        private bool DbRefresh()
        {
            try
            {
                AllDbUpdate();              // update
                if (AllDbDeleteInsert())    // delete, insert
                    return true;            // refresh ok - continue
            }
            catch (Exception ex)
            {
                Logger.Inst.Write(CmdLogType.Rv, $"Exception. DbRefresh. {ex.Message}\r\n{ex.StackTrace}");
                return true;                // - continue
            }
            return false;                   // 수행 내역 없음 - next
        }

        /// <summary>
        /// 시작하자마다 _Db.DbContext.Update(); 수행되면 오류가 발생했다. 시간 딜레이를 주기 위해 한 번 스킵(Skip)한다
        /// </summary>
        /// <param name="dtCur">현재 DateTime. Caller 에 시간값을 저장</param>
        /// <param name="dtOld">이전 DateTime. 이전에 _Db.DbContext.Update 수행했던 시간값을 저장</param>
        /// <param name="bfirst">최초실행flag. </param>
        private void AllDbUpdate()
        {
            // 호출시 즉각 db 반영될 수 있게 시간 딜레이 제거
            DtCur = DateTime.Now;

            if (DtOld == default(DateTime))
                DtOld = DtCur;
            else
            {
                TimeSpan timeDiff = DtCur - DtOld;
                if (timeDiff.TotalSeconds > 1)
                {
                    Db.Update();
                    Console.WriteLine($"dbrefresh.....dtcur = {DtCur}, dtold = {DtOld}, timediff = {timeDiff.TotalSeconds}");
                    DtOld = DtCur;
                }
            }
        }

        private bool AllDbDeleteInsert()
        {
            bool brefresh = false;
            if (Db.GetCount_ObjListDeleteAll() > 0)
            {
                Db.CommitDeleteAll();
                brefresh = true;
            }
            if (Db.GetCount_ObjListDelete() > 0)
            {
                Db.CommitDelete();
                brefresh = true;
            }
            if (Db.GetCount_ObjListAll() > 0)
            {
                Db.CommitAdd();
                brefresh = true;
            }

            if (brefresh)
            {
                Db.DbUpdate(true, new TableType[] { TableType.PEPSCHEDULE });
                brefresh = false;
                return true;
            }
            return false;
        }

        #endregion db refresh

        /// <summary>
        /// ExecuteTime 가 IsNullOrEmpty 일 때 ExecuteTime 을 가져오기 위해 Db에서 pepschedule 테이블을 로딩한다
        /// </summary>
        /// <returns></returns>
        private bool FetchJob()
        {
            if (string.IsNullOrEmpty(ExecuteTime)) return false;
            
            // FSJobs이 존재하는지 확인
            if (FSJobs == null || FSJobs.Count == 0)
            {
                ExecuteTime = string.Empty;
                return false;
            }

            // 현재 Job의 지정된 설비가 DB에 저장되어있는지 확인
            if (!VerifyEquipExist())
            { ExecuteTime = string.Empty;
                return false;
            }

            // 현재 Job의 지정된 설비의 PortSlot에 상위에서 내려준 Number가 DB에 저장된 범위 안에 있는지 확인
            CheckPortSlotInfo();
            // FSJobs이 존재하는지 확인
            if (FSJobs == null || FSJobs.Count == 0)
            { ExecuteTime = string.Empty;
                return false;
            }

            return true;
        }

        private bool VerifyEquipExist()
        {
            foreach (var v in FSJobs)
            {
                // 현재 Job이 Child Job이면 Skip
                if (v.BATCHID.Contains("CFS"))
                    continue;

                // 현재 Job의 Src 설비를 DB에서 확인
                var isSrcUnit = Db.Units_FS.Where(p => p.ID == v.S_EQPID).ToList();
                // 현재 Job의 Dst 설비를 DB에서 확인
                var isDstUnit = Db.Units_FS.Where(p => p.ID == v.T_EQPID).ToList();
                // Src/Dst 설비가 존재하지않으면 Error
                if (isSrcUnit == null || isDstUnit == null || isSrcUnit.Count() == 0 || isDstUnit.Count() == 0)
                {
                    v.C_srcAssignTime = DateTime.Now;
                    v.C_state = (int)CmdState.EQP_NOTEXIST;

                    Db.CopyCmdToHistory(v.ID.ToString());
                    Db.Delete(v);
                    Db.DbUpdate(TableType.PEPSCHEDULE);
                    Logger.Inst.Write(CmdLogType.Db, $"[VSP] EQP_NOTEXIST. srcEqp = {v.S_EQPID}, dstEqp = {v.T_EQPID}");
                }
            }

            return (FSJobs.Count() > 0) ? true : false;
        }

        private void CheckPortSlotInfo()
        {
            string eqp_portslot = string.Empty;
            string buf_portslot = string.Empty;
            string eqp = string.Empty;
            string slot = string.Empty;
            List<unit_fs> units = new List<unit_fs>();
            bool b = false;
            foreach (var v in FSJobs)
            {                
                // 현재 Job이 Child Job이면 Skip
                if (v.BATCHID.Contains("CFS"))
                {
                    continue;
                }
                b = false;
                for (int i = 0; i < 2 && !b; i++)
                {
                    // 반복 횟수에 따라 Src와 Dst Data 저장
                    eqp = (i == 0) ? ((pepschedule)v).S_EQPID : ((pepschedule)v).T_EQPID;
                    slot = (i == 0) ? ((pepschedule)v).S_SLOT : ((pepschedule)v).T_SLOT;

                    // 현재 저장된 설비가 DB에 있는지 확인
                    units = Db.Units_FS.Where(p => p.ID == eqp).ToList();
                    if (units.Count() == 0)
                    { b = true; continue;
                    }

                    // 저장된 설비가 FSSTK이면
                    if (units[0].goaltype == (int)EqpGoalType.FSSTK)
                    {
                        string[] slots = slot.Split(',');

                        foreach (var x in slots)
                        {
                            // PortSlot에 SF가 포함되어있고 길이가 5이면 통과
                            if (!(x.ToUpper().Contains("SF") && x.Length == 5))
                            {
                                b = true; continue;
                            }
                        }
                    }
                    // 저장된 설비가 CHAMBER이면
                    else if (units[0].goaltype == (int)EqpGoalType.RDT || units[0].goaltype == (int)EqpGoalType.AGING)
                    {
                        // 상위에서 내려준 PortSlotNo을 ','로 Split
                        string[] words = slot.Split(',');
                        for (int n = 0; n < words.Count() && !b; n++)
                        {
                            int allslot = -1, slotNo = -1;

                            try
                            {
                                // PortSlotNo을 숫자만 추출                                
                                slotNo = Convert.ToInt32(words[n].Substring(1, words[n].Length - 1));
                                // DB에 저장된 CHAMBER의 Slot 개수를 계산
                                allslot = (int)units[0].max_row * (int)units[0].max_col;
                                
                            }
                            catch (Exception ex)
                            {
                                Logger.Inst.Write(CmdLogType.Db, $"Exception. CheckPortSlotInfo. {ex.Message}\r\n{ex.StackTrace}");
                            }

                            // 상위에서 내려준 PortSlotNo이 DB의 저장된 Slot 개수보다 크면 Error
                            if ((slotNo > allslot))
                            {
                                b = true; continue;
                            }

                        }
                    }
                    else if (units[0].goaltype == (int)EqpGoalType.SYSWIN)
                    {
                        // 상위에서 내려준 PortSlotNo을 ','로 Split
                        string[] words = slot.Split(',');
                        for (int n = 0; n < words.Count() && !b; n++)
                        {
                            byte[] ascPort = Encoding.ASCII.GetBytes(words[n].Substring(0, 1));// - 'A';
                            byte[] ascBase = Encoding.ASCII.GetBytes("A");

                            int allslot = -1, slotNo = -1;
                            
                            try
                            {
                                int row = Convert.ToInt32(words[n].Substring(1, words.Length - 1));

                                slotNo = ascPort[0] - ascBase[0] + 1;   // portNo 는 1 Base 다

                                if (row > 1)
                                {
                                    slotNo += (int)units[0].max_col * (row - 1);
                                }

                                // DB에 저장된 CHAMBER의 Slot 개수를 계산
                                allslot = (int)units[0].max_row * (int)units[0].max_col;

                            }
                            catch (Exception ex)
                            {
                                Logger.Inst.Write(CmdLogType.Db, $"Exception. CheckPortSlotInfo. {ex.Message}\r\n{ex.StackTrace}");
                            }

                            // 상위에서 내려준 PortSlotNo이 DB의 저장된 Slot 개수보다 크면 Error
                            if ((slotNo > allslot))
                            {
                                b = true; continue;
                            }

                        }
                    }
                    else
                    {
                        b = true;
                    }
                }
                // Error 처리
                if (b)
                {
                    Logger.Inst.Write(CmdLogType.Db, $"CheckPortSlotInfo. Invalid slot info");
                    try
                    {
                        ((pepschedule)v).C_srcAssignTime = DateTime.Now;
                        ((pepschedule)v).C_dstAssignTime = DateTime.Now;
                        ((pepschedule)v).C_state = (int)CmdState.INVALID_SLOT;
                        Db.DbUpdate(TableType.PEPSCHEDULE);    // FormMonitor update
                        Db.CopyCmdToHistory(((pepschedule)v).ID.ToString());
                        Db.Delete(((pepschedule)v));
                    }
                    catch (Exception ex)
                    {
                        Logger.Inst.Write(CmdLogType.Db, $"exception CheckPortSlotInfo: {ex.Message}\r\n{ex.StackTrace}");
                    }
                }
            }
        }



        #region job_preprocess

        /// <summary>
        /// scheduled 시간이 되었는지 IF_FLAG 업데이트, 실행가능한 PreTempDown에 대해 수행, 수행가능잡이 있는지, Idle Vehicle이 있는지 체크
        /// </summary>
        /// <param name="bb"></param>
        /// <param name="vec"></param>
        /// <returns></returns>
        private bool PreProcess()
        {
            // 상위에서 내려준 Job 중에 Worktype이 TEMP_DOWN 이고 C_state가 null 인 Job을 REAL_TIME 순으로 Ordering하여 가져오기
            var tempdownjob = Db.Peps.Where(p => p.WORKTYPE == "TEMP_DOWN" && p.C_state == null).OrderBy(p => p.REAL_TIME).ToList();
            // 가져온 Job이 없으면 return
            if (tempdownjob == null || tempdownjob.Count() == 0)
                return true;
            Thread.Sleep(100);

            // 현재 시간을 unix Time으로 변경
            Int32 udtTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            
            // 가져온 Tempdown Job의 Realtime이 현재 Unix Time 보다 늦으면 return
            if ((Convert.ToInt32(tempdownjob[0].REAL_TIME)) > udtTime)
                return true;

            // 실제 진행될 Job List 변수로 다시 가져오기
            FSJobs = Db.Peps.Where(p => p.WORKTYPE == "TEMP_DOWN" && p.C_state == null).OrderBy(p => p.REAL_TIME).ToList();
            
            // State에 완료 Data 저장
            ((pepschedule)FSJobs[0]).C_state = (int)CmdState.DST_COMPLETE;

            if (Cfg.Data.UseRv)
            {
                // 현재 Job에서 DB에 저장된 Src 설비 불러오기
                var isSrcUnit = Db.Units_FS.Where(p => p.ID == ((pepschedule)FSJobs[0]).S_EQPID).Single();
                // 현재 Job에서 DB에 저장된 Dst 설비 불러오기
                var isDstUnit = Db.Units_FS.Where(p => p.ID == ((pepschedule)FSJobs[0]).T_EQPID).Single();

                var args = new SendPreTempDown()
                {
                    vec = JobProcList["PROGRAM"].Vec,
                    job = (pepschedule)FSJobs[0],
                    srcUnit = isSrcUnit,
                    dstUnit = isDstUnit,
                    downtemp = ((pepschedule)FSJobs[0]).DOWNTEMP
                };
                // PreTempDown 전송 이벤트
                Global.Init.RvComu.PreTempDown(args);
            }       

            // 체크 수행 후, 실제 명령을 수행하던 삭제하던 레코드의 데이터를 삭제하던 레코드 시간 데이터를 업데이트
            ((pepschedule)FSJobs[0]).C_srcAssignTime = DateTime.Now;
            ((pepschedule)FSJobs[0]).C_srcFinishTime = DateTime.Now;
            ((pepschedule)FSJobs[0]).C_dstAssignTime = DateTime.Now;
            ((pepschedule)FSJobs[0]).C_dstFinishTime = DateTime.Now;
            Db.DbUpdate(TableType.PEPSCHEDULE);    // FormMonitor update
            Db.CopyCmdToHistory(((pepschedule)FSJobs[0]).ID.ToString());
            Db.Delete(FSJobs[0]);

            // 완료 후 FSJob에서 해당 PreTempDown Job 삭제
            FSJobs.Remove(FSJobs[0]);
            return false;

        }

        #endregion job_preprocess

        #region selectmulti

        /// <summary>
        /// MultiJobWhere, IsMultiJob, MultiJobs, Jobs
        /// </summary>
        /// <param name="worktype"></param>
        private void SelectMultiJob(PepsWorkType worktype)
        {
            MultiJobWhere = (int)eMultiJobWhere.NONE;
            if (FSJobs.Count() <= 0)
                return;

            List<MulGrp> grplst = new List<MulGrp>();
            // worktype에 따라 MultiJob 기준 설비 지정
            switch(worktype)
            {
                case PepsWorkType.FSI:
                case PepsWorkType.MGI: MultiJobWhere = eMultiJobWhere.SRC; break;
                case PepsWorkType.FSO:
                case PepsWorkType.MGO: MultiJobWhere = eMultiJobWhere.DST; break;
                default: return;
            }
            // Worktype과 MultiJobwhere을 기준으로 동일한 설비가 있는지 확인
            grplst = TryGrouping(MultiJobWhere, worktype);

            //if (grplst.Count > 0)   // 배열첨자 인덱싱 오류 예방 체크
            //{   if (grplst[0].COUNT < 2)
            //    {
            //        if (worktype == PepsWorkType.OI)
            //        {   MultiJobWhere = eMultiJobWhere.DST;
            //            grplst = TryGrouping(MultiJobWhere, worktype);
            //        }
            //    }
            //}

            GroupList = grplst;
        }

        /// <summary>
        /// EXECUTE_TIME 이 WORKTYPE 에 따라 JOB 을 그룹핑하여, 여러건의 잡(JOB)을 단건으로의 조합으로 통합
        /// . 통합시 
        /// </summary>
        /// <param name="where">그룹핑 할 장비 선택 기준</param>
        /// <param name="worktype">worktype 선택</param>
        /// <returns>그룹핑된 </returns>
        //  (priority, eqpid, count)
        //  3	RO1501-1	2
        //  3	RO2114-1	3
        //  3	RR4201-1	3
        private List<MulGrp> TryGrouping(eMultiJobWhere where, PepsWorkType worktype)
        {
            // Worktype과 MultiJobwhere을 기준으로 동일한 설비가 있는지 확인
            List<MulGrp> itemlist =
                   FSJobs.Where(p => p.REAL_TIME == ExecuteTime && p.C_priority == (int)worktype)
                       .GroupBy(p => new { PRIORITY = p.C_priority, EQPID = ((where == eMultiJobWhere.SRC) ? p.S_EQPID : p.T_EQPID) })
                       .Select(group => new MulGrp()
                       {
                           PRIORITY = group.Key.PRIORITY.Value,
                           EQPID = group.Key.EQPID,
                           COUNT = group.Count()
                       })
                       .OrderBy(x => x.PRIORITY).ThenByDescending(x => x.COUNT)
                       .ToList();
            return itemlist;
        }

#endregion selectmulti


        private struct TempNearestGoalAndVehicleStruct
        {
            public string goalName;
            public float goalDist;
            public vehicle vec;
        }

        /// <summary>
        /// Groupby된 작업목록,itemlist와 가용 가능한 vehicles 간의 거리값을 구하여, 최종 vehicle 을 선정, 배정할 것이다
        /// </summary>
        /// <param name="itemlist"></param>
        /// <param name="vecs"></param>
        /// <returns></returns>
        private TempNearestGoalAndVehicleStruct SelectVehicleNearbyToUnitS(List<pepschedule> itemlist, List<vehicle> vecs)
        {   // itemlist 의 eqpid 로 unitlist 생성
            List<unit_fs> unitlist = new List<unit_fs>();
            foreach (var v in itemlist)
            {
                unit_fs unt = SelectUnitByID(v.S_EQPID);
                if (unt == null)
                    continue;
                unitlist.Add(unt);
            }

            if (unitlist.Count == 0 || vecs.Count == 0)
            {
                TempNearestGoalAndVehicleStruct ret = new TempNearestGoalAndVehicleStruct();
                return ret;
            }

            // job 목록에 해당하는 unitlist 와 가용 가능한 vecs 들의 절대 거리값을 구한다
            var resArry = new TempNearestGoalAndVehicleStruct[unitlist.Count*vecs.Count];
            Parallel.For(0, vecs.Count, i =>
            {   for (int j = 0; j < unitlist.Count; j++)
                {   resArry[i * unitlist.Count + j] = new TempNearestGoalAndVehicleStruct()
                    {   goalDist = GetDistanceBetweenPoints(new PointF((float)unitlist[j].loc_x, (float)unitlist[j].loc_y), 
                                                            new PointF((float)vecs[i].loc_x, (float)vecs[i].loc_y)),
                        goalName = unitlist[j].ID,
                        vec = vecs[i]
                    };
                }
            });

            // 최소 거리를 가지는 vehicle 을 선정, 배정할 것이다
            var min = resArry.OrderBy(p => p.goalDist).First();
            // 현재 Job에 VEHICLEID가 존재하고 SrcFinishTime이 Null이 아니면
            if (itemlist[0].C_VEHICLEID != null && itemlist[0].C_VEHICLEID != "" && itemlist[0].C_srcFinishTime != null)
            {
                // 현재 Job에 지정된 VEHICLEID를 min.vec에 덮어씌운다.
                var AssignVec = vecs.Where(p => p.ID == itemlist[0].C_VEHICLEID).SingleOrDefault();
                if (AssignVec != null)
                    min.vec = AssignVec;
            }
            return min;
        }

        private unit_fs SelectUnitByID(string id)
        {
            unit_fs unt = new unit_fs();
            try
            {   unt = Db.Units_FS.Where(p => p.ID == id).FirstOrDefault();
            }
            catch(Exception ex)
            {   Logger.Inst.Write(CmdLogType.Rv, $"Exception. SelectUnitByID. {ex.Message}\r\n{ex.StackTrace}");
                return (unit_fs)null;
            }

            if (unt == null)
                return (unit_fs)null;
            return unt;
        }

        private float GetDistanceBetweenPoints(PointF a, PointF b)
        {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        public async void _Vsp_OnCallExecuteTime(object sender, CallExecuteTime e)
        {
            ExecuteTime = e.executeTime;
        }
    }
	
    public class SendJobToVecArgs1 : EventArgs
    {
        public string cmd;
        public string vecID = "";
        public bool fsstk_send= false;
        public bool return_check;
        public pepschedule job;
        public List<pepschedule> joblist;
        public vehicle vec;
        public unit_fs eqp;
        public string eqpslot;
        public string bufslot;
        public string sndMsg;
        public RvAssignProc rvAssign;
        public string Fail_unit = "";
    }

    public class SendPreTempDown : EventArgs
    {
        public vehicle vec;
        public pepschedule job;
        public unit_fs srcUnit;
        public unit_fs dstUnit;
        public Nullable<int> downtemp;
    }

    public class MulGrp
    {
        public int PRIORITY;
        public string EQPID;
        public int COUNT;
    }

    public class CallCmdProcArgs1 : EventArgs
    {
        public eMultiJobWhere where;
        public List<pepschedule> peps;
        public vehicle vec;
        public MulGrp grp;
        public string executeTime;
        public string realTime;
        public int grp_count;
    }

    public class MultiSrcFinishArgs : EventArgs
    {
        public pepschedule pep;
        public vehicle vec;
    }
    public class CallExecuteTime : EventArgs
    {
        public string executeTime;
    }
}
