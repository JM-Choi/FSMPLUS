using FSMPlus.Forms;
using FSMPlus.Ref;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMPlus.Logic
{
    public class TableUpdateArgs : EventArgs
    {
        public TableType[] target;
    }

    public class TransferCmdArgs : EventArgs
    {
        public TransferRcmdType Rcmd;

        public string CommandId;
        public string CommandType;
        public int Priority;
        public int Replace;
        public string CarrierId;
        public string SrcPort;
        public string DstPort;
        public int CarrierType;
        public string BatchId;
        public string BatchSeq;

        /// <summary>
        /// 해당 메세지에 대한 응답을 위한 변수
        /// </summary>
        public long SysByte;
        public long Stream;
        public long Function;

    }

    public enum TransferRcmdType
    {
        Transfer,
        Update,
        BatchTrans,
        StageCmd,
    }
    public class DbHandler
    {
        #region DbHandler.Inst 싱글톤 객체 생성
        private static volatile DbHandler instance;
        private static object syncObj = new object();
        public static DbHandler Inst
        {
            get
            {
                if (instance == null)
                {
                    lock (syncObj)
                    {
                        if (instance == null)
                            instance = new DbHandler();
                    }
                }
                return instance;
            }
        }
        #endregion

        public event EventHandler<TableUpdateArgs> OnChangeTableData;

        private MPlusEntities           RealDb = null;
        public List<alarm>              Alarms;
        public List<alarm_define>       AlarmsDef;
        public List<alarm_history>      AlarmHisto;
        public List<charge>             Charges;
        public List<controller>         Controllers;
        public List<cost>               Costs;
        public List<distance>           Distances;
        public List<pepschedule>        Peps;
        public List<pepschedule_history> PepsHisto;
        public List<vehicle>            Vechicles;
        public List<vehicle_part>       VecParts;
        public List<unit>               Units;
        public List<standby>            Standby;
        public List<zone>               Zones;
        public List<mg_type>            Mg_Types;
        public List<unit_fs>            Units_FS;

        private object _syncObjListDelete = new object();
        private object _syncObjListDeleteAll = new object();
        private object _syncObjListAdd = new object();

        private List<object> _objListDelete = new List<object>();
        private List<object> _objListDeleteAll = new List<object>();
        private List<object> _objListAdd = new List<object>();

        public int GetCount_ObjListDelete() { return _objListDelete.Count(); }
        public int GetCount_ObjListDeleteAll() { return _objListDeleteAll.Count(); }
        public int GetCount_ObjListAll() { return _objListAdd.Count(); }

        public DbHandler()
        {
            RealDb = new MPlusEntities();
            LocalDb();
        }

        private void LocalDb()
        {
            Alarms = RealDb.alarm.ToList();
            AlarmsDef = RealDb.alarm_define.ToList();
            AlarmHisto = RealDb.alarm_history.ToList();
            Charges = RealDb.charge.ToList();
            Controllers = RealDb.controller.ToList();
            Costs = RealDb.cost.ToList();
            Distances = RealDb.distance.ToList();
            // Tray/Stack 과 같은 DB를 사용하므로
            // FS 반송에 필요한 Job 만 가져오기
            Peps = RealDb.pepschedule.Where(p=>p.TRANSFERTYPE == "FS").ToList();
            PepsHisto = RealDb.pepschedule_history.Where(p => p.TRANSFERTYPE == "FS").ToList();
            Vechicles = RealDb.vehicle.Where(p => p.isUse == 1 && p.TRANSFERTYPE == "FS").ToList();
            //
            VecParts = RealDb.vehicle_part.ToList();
            Units = RealDb.unit.ToList();
            Standby = RealDb.standby.ToList();
            Zones = RealDb.zone.ToList();
            Mg_Types = RealDb.mg_type.ToList();
            Units_FS = RealDb.unit_fs.ToList();

            // M6X 에 AUTO 제어를 내리기 위해 Vehicles 상태를 확인할거다. 해서 기존 정보는 삭제하자
            foreach (var vec in Vechicles)
            {
                vec.loc_x = 0;
                vec.loc_y = 0;
                vec.C_loc_th = 0;
                vec.C_mode = (int)VehicleMode.INIT;
                vec.C_state = (int)VehicleState.NOT_ASSIGN;
                vec.C_chargeRate = 100;
            }
        }

        public string FindVehicleforUnit(string unitname)
        {
            try
            {
                var unts = Units_FS.Where(p => p.VEHICLEID == unitname).Single();
                return unts.VEHICLEID;
            }
            catch(Exception ex)
            {
                Logger.Inst.Write(CmdLogType.Db, $"{ex.Message}\r\n{ex.StackTrace}");
                return string.Empty;
            }
        }

        public void DbUpdate(TableType type)
        {
            OnChangeTableData?.Invoke(this, new TableUpdateArgs() { target = new TableType[] { type } });
        }
        public void DbUpdate(bool viewUpdate, TableType[] types = null)
        {
            if (viewUpdate)
            {
                OnChangeTableData?.Invoke(this, new TableUpdateArgs() { target = types });
            }
        }
        public void Update()   //ss.kim-20190102
        {
            try
            {
                RealDb.SaveChangesAsync();
            }
            catch (InvalidOperationException e)
            {
                Logger.Inst.Write(CmdLogType.Db, $"Db Commit 실패 : {e.ToString()}");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                Logger.Inst.Write(CmdLogType.Db, $"Db Commit 실패 : {e.ToString()}");
                foreach (var err in e.EntityValidationErrors)
                {
                    Logger.Inst.Write(CmdLogType.Db, err.ToString());
                }
            }
            catch (Exception e)
            {
                Logger.Inst.Write(CmdLogType.Db, $"Db Commit 실패 : {e.ToString()}");
            }
        }

        public void Add(object obj)
        {
            lock (_syncObjListAdd)
            {
                _objListAdd.Add(obj);
            }
        }
        public void CommitAdd()
        {
            lock (_syncObjListAdd)
            {
                foreach (var v in _objListAdd)
                {
                    if (v is alarm)
                    {
                        Alarms.Add((alarm)v);
                        RealDb.alarm.Add((alarm)v);
                    }
                    else if (v is alarm_define)
                    {
                        AlarmsDef.Add((alarm_define)v);
                        RealDb.alarm_define.Add((alarm_define)v);
                    }
                    else if (v is alarm_history)
                    {
                        AlarmHisto.Add((alarm_history)v);
                        RealDb.alarm_history.Add((alarm_history)v);
                    }
                    else if (v is charge)
                    {
                        Charges.Add((charge)v);
                        RealDb.charge.Add((charge)v);
                    }
                    else if (v is controller)
                    {
                        Controllers.Add((controller)v);
                        RealDb.controller.Add((controller)v);
                    }
                    else if (v is cost)
                    {
                        Costs.Add((cost)v);
                        RealDb.cost.Add((cost)v);
                    }
                    else if (v is distance)
                    {
                        Distances.Add((distance)v);
                        RealDb.distance.Add((distance)v);
                    }
                    else if (v is pepschedule)
                    {
                        Peps.Add((pepschedule)v);
                        RealDb.pepschedule.Add((pepschedule)v);
                    }
                    else if (v is pepschedule_history)
                    {
                        PepsHisto.Add((pepschedule_history)v);
                        RealDb.pepschedule_history.Add((pepschedule_history)v);
                    }
                    else if (v is standby)
                    {
                        Standby.Add((standby)v);
                        RealDb.standby.Add((standby)v);
                    }
                    else if (v is unit)
                    {
                        Units.Add((unit)v);
                        RealDb.unit.Add((unit)v);
                    }
                    else if (v is vehicle)
                    {
                        Vechicles.Add((vehicle)v);
                        RealDb.vehicle.Add((vehicle)v);
                    }
                    else if (v is vehicle_part)
                    {
                        VecParts.Add((vehicle_part)v);
                        RealDb.vehicle_part.Add((vehicle_part)v);
                    }
                    else if (v is zone)
                    {
                        Zones.Add((zone)v);
                        RealDb.zone.Add((zone)v);
                    }
                    else if (v is unit_fs)
                    {
                        Units_FS.Add((unit_fs)v);
                        RealDb.unit_fs.Add((unit_fs)v);
                    }
                }
                _objListAdd.Clear();
            }
        }
        public void Delete(object obj)
        {
            lock (_syncObjListDelete)
            {
                _objListDelete.Add(obj);
                //Console.WriteLine($"Delete input : {((pepschedule)obj).BATCHID}");
            }
        }
        public void CommitDelete()
        {
            lock (_syncObjListDelete)
            {
                foreach (var v in _objListDelete)
                {
                    if (v is alarm)
                    {
                        RealDb.alarm.Remove((alarm)v);
                        Alarms.Remove((alarm)v);
                    }
                    else if (v is alarm_define)
                    {
                        RealDb.alarm_define.Remove((alarm_define)v);
                        AlarmsDef.Remove((alarm_define)v);
                    }
                    else if (v is alarm_history)
                    {
                        RealDb.alarm_history.Remove((alarm_history)v);
                        AlarmHisto.Remove((alarm_history)v);
                    }
                    else if (v is charge)
                    {
                        RealDb.charge.Remove((charge)v);
                        Charges.Remove((charge)v);
                    }
                    else if (v is controller)
                    {
                        RealDb.controller.Remove((controller)v);
                        Controllers.Remove((controller)v);
                    }
                    else if (v is cost)
                    {
                        RealDb.cost.Remove((cost)v);
                        Costs.Remove((cost)v);
                    }
                    else if (v is distance)
                    {
                        RealDb.distance.Remove((distance)v);
                        Distances.Remove((distance)v);
                    }
                    else if (v is pepschedule)
                    {
                        RealDb.pepschedule.Remove((pepschedule)v);
                        Peps.Remove((pepschedule)v);
                    }
                    else if (v is pepschedule_history)
                    {
                        RealDb.pepschedule_history.Remove((pepschedule_history)v);
                        PepsHisto.Remove((pepschedule_history)v);
                    }
                    else if (v is standby)
                    {
                        RealDb.standby.Remove((standby)v);
                        Standby.Remove((standby)v);
                    }   
                    else if (v is unit)
                    {
                        RealDb.unit.Remove((unit)v);
                        Units.Remove((unit)v);
                    }
                    else if (v is vehicle)
                    {
                        RealDb.vehicle.Remove((vehicle)v);
                        Vechicles.Remove((vehicle)v);
                    }
                    else if (v is vehicle_part)
                    {
                        RealDb.vehicle_part.Remove((vehicle_part)v);
                        VecParts.Remove((vehicle_part)v);
                    }
                    else if (v is zone)
                    {
                        RealDb.zone.Remove((zone)v);
                        Zones.Remove((zone)v);
                    }
                    else if (v is unit_fs)
                    {
                        RealDb.unit_fs.Remove((unit_fs)v);
                        Units_FS.Remove((unit_fs)v);
                    }
                }

                _objListDelete.Clear();
            }
        }
        public void DeleteAll(Type type)
        {
            lock (_syncObjListDeleteAll)
            {
                _objListDeleteAll.Add(type);
            }
        }
        public void CommitDeleteAll()
        {
            lock (_syncObjListDeleteAll)
            {
                foreach (var v in _objListDeleteAll)
                {
                    if ((Type)v == typeof(alarm))
                    {
                        Alarms.Clear();
                        RealDb.alarm.RemoveRange(RealDb.alarm);
                    }
                    else if ((Type)v == typeof(alarm_define))
                    {
                        AlarmsDef.Clear();
                        RealDb.alarm_define.RemoveRange(RealDb.alarm_define);
                    }
                    else if ((Type)v == typeof(alarm_history))
                    {
                        AlarmHisto.Clear();
                        RealDb.alarm_history.RemoveRange(RealDb.alarm_history);
                    }
                    else if ((Type)v == typeof(charge))
                    {
                        Charges.Clear();
                        RealDb.charge.RemoveRange(RealDb.charge);
                    }
                    else if ((Type)v == typeof(controller))
                    {
                        Controllers.Clear();
                        RealDb.controller.RemoveRange(RealDb.controller);
                    }
                    else if ((Type)v == typeof(cost))
                    {
                        Costs.Clear();
                        RealDb.cost.RemoveRange(RealDb.cost);
                    }
                    else if ((Type)v == typeof(distance))
                    {
                        Distances.Clear();
                        RealDb.distance.RemoveRange(RealDb.distance);
                    }
                    else if ((Type)v == typeof(pepschedule))
                    {
                        Peps.Clear();
                        RealDb.pepschedule.RemoveRange(RealDb.pepschedule);
                    }
                    else if ((Type)v == typeof(pepschedule_history))
                    {
                        PepsHisto.Clear();
                        RealDb.pepschedule_history.RemoveRange(RealDb.pepschedule_history);
                    }
                    else if ((Type)v == typeof(standby))
                    {
                        Standby.Clear();
                        RealDb.standby.RemoveRange(RealDb.standby);
                    }
                    else if ((Type)v == typeof(unit))
                    {
                        Units.Clear();
                        RealDb.unit.RemoveRange(RealDb.unit);
                    }
                    else if ((Type)v == typeof(vehicle))
                    {
                        Vechicles.Clear();
                        RealDb.vehicle.RemoveRange(RealDb.vehicle);
                    }
                    else if ((Type)v == typeof(vehicle_part))
                    {
                        VecParts.Clear();
                        RealDb.vehicle_part.RemoveRange(RealDb.vehicle_part);
                    }
                    else if ((Type)v == typeof(zone))
                    {
                        Zones.Clear();
                        RealDb.zone.RemoveRange(RealDb.zone);
                    }
                    else if ((Type)v == typeof(unit_fs))
                    {
                        Units_FS.Clear();
                        RealDb.unit_fs.RemoveRange(RealDb.unit_fs);
                    }
                }
                _objListDeleteAll.Clear();
            }
        }

        /// <summary>
        /// comp 가 jobs 리스트에 포함되어 있는지를 비교하는데 키로 id(auto increment) 와 batchid(unique) 를 비교한다.
        /// 있으면 해당 jobs[x] == pepschedule 를 리턴하여 jobs 리스트에서 찾은 pepschedule 을 삭제하는데 사용한다.
        /// 없으면 comp 가 jobs 리스트에 없으면 db 단에서 comp가 삭제되었다 판단한다.
        /// </summary>
        /// <param name="jobs"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        private pepschedule ExistEqualsPepschedule(List<pepschedule> jobs, pepschedule comp)
        {
            for(int x = 0; x < jobs.Count; x++)
            {
                if (jobs[x].ID == comp.ID && jobs[x].BATCHID == comp.BATCHID)
                {
                    return jobs[x]; // equal
                }
            }
            return null;
        }

        private alarm ExistEqualsAlarm(List<alarm> alarms, alarm comp)
        {
            for (int x = 0; x < alarms.Count; x++)
            {
                if (alarms[x].ID == comp.ID && alarms[x].code == comp.code)
                {
                    return alarms[x]; // equal
                }
            }
            return null;
        }

        /// <summary>
        /// RealDb 에서 신규 요청 schedule 을 확인하고 Append - TODO:Cancel처리 관련 완성 필요
        /// </summary>
        public void UpdateSchedule()
        {
            // Tray/Stack 과 같은 DB를 사용하므로
            // FS 반송에 필요한 Job 만 가져오기
            List<pepschedule> jobs = RealDb.pepschedule.Where(p => p.TRANSFERTYPE == "FS").ToList();
            if (jobs.Count() > 0)
            {   for(int x = 0; x < Peps.Count; x++) // Peps와 jobs가 동일한지 비교
                {   if (jobs.Count > 0)
                    {   pepschedule job = ExistEqualsPepschedule(jobs, Peps[x]);  //id, batch 가지고 비교
                        if (job != null)
                            jobs.Remove(job);
                        else
                        {   // Peps 에는 있는데 jobs 에 없다? db 에서 삭제되었다.
                            // Peps Cancel 처리
                            Peps.Remove(Peps[x]);
                            DbUpdate(TableType.PEPSCHEDULE);
                        }
                    }
                    else
                    {   // Peps 에는 있는데 jobs 에 없다? db 에서 삭제되었다.
                        // Peps Cancel 처리
                        Peps.Remove(Peps[x]);
                        DbUpdate(TableType.PEPSCHEDULE);
                    }
                }

                // jobs 에 남아 있는게 있으면 Peps 에 추가한다.
                for (int y = 0; y < jobs.Count; y++)
                {
                    Peps.Add(jobs[y]);
                    if (y == jobs.Count - 1)
                        DbUpdate(TableType.PEPSCHEDULE);
                }
                jobs.Clear();
            }
            else
            {   if (Peps.Count > 0)
                {   // 1. db 단에서 삭제 되었다.
                    // 모두 cancel 처리
                    Peps.Clear();
                    DbUpdate(TableType.PEPSCHEDULE);
                }
                else
                {   // 아무 action 없다
                }
            }
        }
        public void UpdateError()
        {
            List<alarm> alarms = RealDb.alarm.ToList();
            if (alarms.Count() > 0)
            {
                for (int x = 0; x < Alarms.Count; x++) // Alarms와 alarms가 동일한지 비교
                {
                    if (alarms.Count > 0)
                    {
                        alarm alarm = ExistEqualsAlarm(alarms, Alarms[x]);  //id, batch 가지고 비교
                        if (alarm != null)
                            alarms.Remove(alarm);
                        else
                        {   // Alarms 에는 있는데 alarms 에 없다? db 에서 삭제되었다.
                            // Alarms Cancel 처리
                            Alarms.Remove(Alarms[x]);
                            DbUpdate(TableType.ALARM);
                        }
                    }
                    else
                    {   // Alarms 에는 있는데 alarms 에 없다? db 에서 삭제되었다.
                        // Alarms Cancel 처리
                        Alarms.Remove(Alarms[x]);
                        DbUpdate(TableType.ALARM);
                    }
                }

                // alarms 에 남아 있는게 있으면 Alarms 에 추가한다.
                for (int y = 0; y < alarms.Count; y++)
                {
                    Alarms.Add(alarms[y]);
                    if (y == alarms.Count - 1)
                        DbUpdate(TableType.ALARM);
                }
                alarms.Clear();
            }
            else
            {
                if (Alarms.Count > 0)
                {   // 1. db 단에서 삭제 되었다.
                    // 모두 cancel 처리
                    Alarms.Clear();
                    DbUpdate(TableType.ALARM);
                }
                else
                {   // 아무 action 없다
                    DbUpdate(TableType.ALARM);
                }
            }
        }


        /// <summary>
        /// 로컬 Peps 리스트에서 선택된 EXECUTE_TIME 을 선택한다.(특별한 지정 없으면 시간값 오래된 순으로 정렬된다)
        /// - 가장 오래된 EXECUTE_TIME 을 선택 : 0 번째
        /// - 검색 결과 있으면 가장 오래된 EXECUTE_TIME 중 하나를 선택하면 되서 굳이 GROUP BY는 하지 않음
        /// 
        /// ps. 시간값은 UNIX TIME 이고 해석하려면 tech-library>Tools>UtilMgr>UnixtimeStamptoDateTime 이용하여 datetime 으로 변환가능
        /// </summary>
        /// 
        /// <returns>
        ///     검색결과 없으면 string.Empty
        ///     검색결과 있으면 (string)EXECUTE_TIME
        /// </returns>
        public string Select_ExcuteTime(ref List<vehicle> veclist, int early_time, List<pepschedule> FSJobs)
        {
            bool fail = false;
            // FSJobs가 있으면
            if (FSJobs.Count() > 0)
            {
                // DB에 있는 Job 중 해당 조건에 만족하는 Job을 RealTime으로 Ordering 하여 저장
                var all = FSJobs.Where(p => ((p.C_srcAssignTime == null && (p.C_state == null || p.C_state == (int)CmdState.SRC_NOT_ASSIGN)) ||
                                          (p.C_dstAssignTime == null && (p.C_state == (int)CmdState.SRC_COMPLETE))) && p.WORKTYPE != "TEMP_DOWN"
                                          && p.C_isChecked != 1)
                              .OrderBy(p => p.REAL_TIME).ThenBy(p => p.BATCHID)
                              .ToList();

                // 저장된 Job이 없으면 return
                if (all == null || all.Count() == 0)
                    return string.Empty;
                
                List<vehicle> vec_chk = null;
                foreach (var v in all)
                {
                    // 현재 Job의 TransferType과 Robot의 TransferType이 동일한 Robot 가져오기
                    vec_chk = veclist.Where(p => p.TRANSFERTYPE == v.TRANSFERTYPE).ToList();

                    // 가져온 Robot Data가 있으면
                    if (vec_chk != null && vec_chk.Count() > 0)
                    {
                        // 저장한 Job 중 TransferType이 같은 Job을 RealTime으로 Ordering 하여 저장
                        all = all.Where(p => p.TRANSFERTYPE == v.TRANSFERTYPE).OrderBy(p => p.REAL_TIME).ToList();
                        // veclist 초기화
                        veclist.Clear();

                        // 선정된 Robot을 veclist에 저장
                        foreach (var x in vec_chk)
                        {
                            if (!(VSP.Init.RobotList[x.ID].isStop))
                                veclist.Add(x);
                        }
                        if (veclist.Count > 0)
                            fail = true;
                        else
                            fail = false;
                        break;
                    }
                }

                // 이상없이 진행하였으면
                if (fail)
                    // 선정된 Job 중 맨위의 Job의 RealTime을 return
                    return all[0].REAL_TIME;
                // 이상이 있었으면
                else
                    // Empty를 return
                    return string.Empty;
            }

            // FSJobs가 없으면 Empty를 return
            return string.Empty;
        }

        public bool Select_vehicle(string ExecuteTime, ref List<vehicle> veclist)
        {
            if (Peps.Count() > 0)
            {
                var all = Peps.Where(p => ((p.C_srcAssignTime == null && (p.C_state == null || p.C_state == (int)CmdState.SRC_NOT_ASSIGN)) ||
                                          (p.C_dstAssignTime == null && (p.C_state == (int)CmdState.SRC_COMPLETE))) && p.WORKTYPE != "TEMP_DOWN"
                                          && p.C_isChecked != 1 && p.REAL_TIME == ExecuteTime)
                              .OrderBy(p => p.REAL_TIME)
                              .ToList();

                if (all == null || all.Count() == 0)
                    return false;

                List<vehicle> vec_chk = null;
                foreach (var v in all)
                {
                    if (v.TRANSFERTYPE == null)
                        v.TRANSFERTYPE = "STACK";

                    vec_chk = veclist.Where(p => p.TRANSFERTYPE == v.TRANSFERTYPE && p.C_mode == (int)VehicleMode.AUTO).ToList();

                    if (vec_chk != null && vec_chk.Count() > 0)
                    {
                        all = all.Where(p => p.TRANSFERTYPE == v.TRANSFERTYPE).OrderBy(p => p.REAL_TIME).ToList();
                        veclist.Clear();

                        foreach (var x in vec_chk)
                        {
                            if (!(VSP.Init.RobotList[x.ID].isStop))
                                veclist.Add(x);
                        }
                        if (veclist.Count > 0)
                            return true;
                        else
                            return false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// 선정된 ExecuteTime 보다 과거된 회귀된 executeTime을 가지는 신규잡은 처리하지 않게 Filtering
        /// </summary>
        /// <param name="executeTime">선정된 ExecuteTime</param>
        /// <returns></returns>
        public List<pepschedule> PassedTimeJob(string executeTime, int sec)
        {
            try
            {
                Int32 udtTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds - sec;
                var all = Peps.Where(p => (Convert.ToInt32(executeTime) < udtTime && p.C_srcAssignTime == null))
                              .OrderBy(p => p.C_priority).ToList();
                return all;
            }
            catch (Exception ex)
            {
                Logger.Inst.Write(CmdLogType.Db, $"{ex.Message}\r\n{ex.StackTrace}");
            }
            return null;
        }


        public PepsWorkType GetWorkTypeStringToEnum(string wORKTYPE)
        {
            switch (wORKTYPE)
            {   case "TEMP_DOWN": return PepsWorkType.TEMP_DOWN;      
                case "FSI":       return PepsWorkType.FSI;      
                case "FSO":       return PepsWorkType.FSO;      
                case "MGI":       return PepsWorkType.MGI;      
                case "MGO":       return PepsWorkType.MGO;      
            }
            return PepsWorkType.NONE;
        }

        /// <summary>
        /// Peps 테이블에서 SRC JOB 마친 DST JOB 이 있는지 검색
        /// - 조건 : CmdState.SRC_COMPLETE && p.C_srcFinishTime != null && C_dstFinishTime == null
        /// </summary>
        /// <param name="executeTime">선정된 EXECUTE_TIME</param>
        /// <returns> DST JOB 의 List<peoschedule> </returns>
        public List<pepschedule> Select_DstJob(string executeTime)
        {
            try
            {
                var all = Peps.Where(p => (p.EXECUTE_TIME == executeTime
                                        && p.C_state == (int)CmdState.SRC_COMPLETE
                                        && p.C_srcFinishTime != null
                                        && p.C_dstFinishTime == null))
                              .OrderBy(p => p.C_priority).ToList();
                return all;
            }
            catch (Exception ex)
            {
                Logger.Inst.Write(CmdLogType.Db, $"{ex.Message}\r\n{ex.StackTrace}");
            }
            return null;
        }

        /// <summary>
        /// 가용 가능한 Vehicle 이 있는지 검색
        /// - 통신이 연결되고(VehicleInstallState.INSTALLED), Vehicles 에 BATCHID 가 할당되지 않은 것
        /// </summary>
        /// <returns>IDLE VEHICLE LIST<vehicle></returns>
        /// jm.choi - 190411
        /// 현재 배정된 Job의 Worktype에 따라 Vehicle 정보 조회
        public List<vehicle> Select_IdleVehicle(PepsWorkType worktype)
        {
            string vecid = string.Empty;
            // jm.choi - 190411
            // 사용가능한 Vehicle의 데이터 Load
            // 사용가능한 Vehicle이 2대 이상이면 각각 맡은 Worktype 진행
            List<vehicle> vecs = Vechicles.Where(p=>p.isUse == 1).ToList();

            if (vecs != null)
            {
                if (vecs.Count() > 1)
                {
                    if ((int)worktype == 2)
                    {
                        vecid = "VEHICLE01";
                    }
                    else if ((int)worktype == 3)
                    {
                        vecid = "VEHICLE02";
                    }
                    else
                    {
                        for (int i = 0; i < vecs.Count(); i++)
                        {
                            if (vecs[i].isAssigned == 0)
                                vecid = vecs[i].ID;
                        }
                    }
                }
                else
                {
                    vecid = vecs[0].ID;
                }
            }

            return Vechicles.Where(p => ((p.C_BATCHID == "" || p.C_BATCHID == null) ||
                                         (p.isAssigned == 0 || p.isAssigned == null))
                                        && p.isUse == 1 && p.ID == vecid
                                        && p.installState == (int)VehicleInstallState.INSTALLED).ToList();
        }

        /// <summary>
        /// 선정된 EXECUTE_TIME 내에서 처리되지 않은 NEW JOB 을 검색
        /// 우선 순위를 적용하며 검색된 worktype 을 out 한다.
        /// 검색된 jobs 을 return한다
        /// </summary>
        /// <param name="executeTime">선정된 EXECUTE_TIME</param>
        /// <param name="e"></param>
        /// <returns></returns>
        public pepschedule Select_NewJob(string executeTime, PepsWorkType eCur, out PepsWorkType eRet)
        {
            var jobs = Peps.Where(p => p.C_srcAssignTime == null && p.EXECUTE_TIME == executeTime).ToList();
            if (jobs == null || jobs.Count == 0)
            {
                eRet = PepsWorkType.NONE;
                return null;
            }

            List<pepschedule> Newjobs = new List<pepschedule>();
            for (int i = (int)eCur; i < (int)PepsWorkType.LAST; i++)
            {
                Newjobs = jobs.Where(p => p.WORKTYPE == ((PepsWorkType)i).ToString()).ToList();
                if (Newjobs.Count() <= 0)
                    continue;

                if (i == 1 || i == 2 || i == 3 || i == 4)
                {
                    for (int j = 0; j < Newjobs.Count(); j++)
                    {
                        eRet = (PepsWorkType)i;
                        return Newjobs[j];
                    }
                }
                else
                {
                    eRet = (PepsWorkType)i;
                    return Newjobs[0];
                }

            }

            eRet = PepsWorkType.NONE;
            return null;
        }

        /// <summary>
        /// 파라메터 executeTime 과 동일한 시간값을 가지는 dbRecord 를 검색한다.(src job 조건, dst job 조건 모두)
        /// 검색결과는 priority 로 sorting 한다
        /// </summary>
#if true
        public (PepsWorkType, List<pepschedule>) SelectAllPepsInExecuteTime(string executeTime, List<vehicle> veclist)
        {
            PepsWorkType worktype = PepsWorkType.NONE;
            var jobs = Peps.Where(p => p.REAL_TIME == executeTime
                                    && p.C_isChecked != 1  
                                    && ((p.C_srcAssignTime == null && (p.C_state == null || p.C_state == (int)CmdState.SRC_NOT_ASSIGN)) ||
                                       (p.C_dstAssignTime == null && (p.C_state == (int)CmdState.SRC_COMPLETE))))
                           .OrderBy(p => p.C_priority)
                           .ToList();

            if (jobs == null || jobs.Count == 0)
            {   worktype = PepsWorkType.NONE;
                return (worktype, null);
            }

            worktype = GetWorkTypeStringToEnum(jobs[0].WORKTYPE);
            jobs = Peps.Where(p => p.REAL_TIME == executeTime
                                    && p.C_isChecked != 1 && p.WORKTYPE == jobs[0].WORKTYPE
                                    && ((p.C_srcAssignTime == null && (p.C_state == null || p.C_state == (int)CmdState.SRC_NOT_ASSIGN)) ||
                                       (p.C_dstAssignTime == null && (p.C_state == (int)CmdState.SRC_COMPLETE))))
                           .OrderBy(p => p.C_priority)
                           .ToList();
            return (worktype, jobs);
        }
#else
        public List<pepschedule> SelectAllPepsInExecuteTime(string executeTime, ref PepsWorkType worktype)
        {
            var jobs = Peps.Where(p => p.EXECUTE_TIME == executeTime && p.C_isChecked != 1 &&
                                      ( p.C_srcAssignTime == null && (p.C_state == null || p.C_state == (int)CmdState.QUEUE)) ||
                                      ( p.C_dstAssignTime == null && (p.C_state == (int)CmdState.SRC_COMPLETE)))
                           .OrderBy(p => p.C_priority)
                           .ToList();

            if (jobs == null || jobs.Count == 0)
                return null;

            worktype = GetWorkTypeStringToEnum(jobs[0].WORKTYPE);
            return jobs;
        }
#endif
        /// <summary>
        /// WORKTYPE alphabet 문자를 priority 를 표시할 수 있는 숫자로 변환한다
        /// db 데이터 획득시 priority 로 정렬하여 얻는다
        /// db 에 priority 적용은 src job 획득시에만 부과한다
        /// </summary>
        public void Update_PepsPriority(string executeTime)
        {
            var jobs = Peps.Where(p => p.REAL_TIME == executeTime && 
                                       p.C_srcAssignTime == null)
                           .ToList();

            if (jobs == null || jobs.Count == 0)
                return;

            foreach (var job in jobs)
            {
                job.C_priority = (int)GetWorkTypeStringToEnum(job.WORKTYPE);
            }
        }
        
        /*선정된 EXECUTE_TIME 내에서 최초 1회 WORKTYPE 을 해석하여 작업 우선순위를 부여 (삼성 기준제시)
         * - OUT > OUT/IN > IN > Empty Out > Empty In > Tester Out > Tester In (문서상)
         * - O   > OI     > I  > EO        > EI       > TO         > TI        (실제 DB 입력)
         * - WORKTYPE 의 STRING 을 보고 _priority 필드에 우선순위를 marking 한다
         * - 동일한 우선순위를 가지는 건이 다수개 발생할 수 있다 - 이 때 multi job 여부를 확인이 필요하다
         * - 이 후부터는 선정된 executeTime 내에서 _priority 값으로 ordering 해야 한다.
         * */
        /// <param name="executeTime">선정된 EXECUTE_TIME</param>


        // jm.choi 추가 - 190304
        // Execute Time 오름차순 이후 priority 오름차순 시행
        public List<pepschedule> Sort_Peps(string executeTime)
        {
            var jobs = Peps.Where(p => ((p.C_srcAssignTime == null && (p.C_state == null || p.C_state == (int)CmdState.SRC_NOT_ASSIGN))
                                    || (p.C_dstAssignTime == null && (p.C_state == (int)CmdState.SRC_COMPLETE))) && p.EXECUTE_TIME == executeTime)
                            .OrderBy(p => p.EXECUTE_TIME).OrderBy(p => p.C_priority).ToList();
            if (jobs == null || jobs.Count == 0)
                return null;

            return jobs;
        }

		// jm.choi - 190403
		// Multi Job으로 합쳐진 Job의 Tray와 StepID를 매치 시키기 위한 함수
		// Worktype이 I 일 경우 pepschdule에서 찾아야하고
		// Worktype이 O 일 경우 pepschdule_history에서 찾아야한다
        ///string sql = string.Format($"SELECT COUNT(*) FROM mplus.pepschedule WHERE BATCHID = '{batchid}'");
        public List<pepschedule_history> FindMultiId(string multiid, string jobtype, string worktype, ref List<pepschedule> pep)
        {
            List<pepschedule_history> pep_his = null;
            int pephis_count = 0;
            int pep_count = 0;

            pephis_count = PepsHisto.Where(p => p.MULTIID == multiid).OrderBy(p => p.ID).Count();
            pep_count = Peps.Where(p => p.MULTIID == multiid).OrderBy(p => p.ID).Count();
            try
            {
#if true
                if (pephis_count > 0)
                    pep_his = PepsHisto.Where(p => p.MULTIID == multiid).OrderBy(p => p.ID).ToList();
                if (pep_count > 0)
                    pep = Peps.Where(p => p.MULTIID == multiid).OrderBy(p => p.ID).ToList();
#else
                if (jobtype.CompareTo("MSRC") == 0 && worktype == "I")
                {
                    if (pephis_count > 0)
                        pep_his = PepsHisto.Where(p => p.MULTIID == multiid).OrderBy(p => p.ID).ToList();
                    if (pep_count > 0)
                        pep = Peps.Where(p => p.MULTIID == multiid).OrderBy(p=>p.ID).ToList();
                }
                else if (jobtype.CompareTo("MSRC") == 0 && worktype == "O")
                {
                    pep_his = PepsHisto.Where(p => p.MULTIID == multiid).OrderBy(p => p.ID).ToList();
                }
                else if (jobtype.CompareTo("MDST") == 0 && worktype == "I")
                {
                    if (pephis_count > 0)
                        pep_his = PepsHisto.Where(p => p.MULTIID == multiid).OrderBy(p => p.ID).ToList();
                    if (pep_count > 0)
                        pep = Peps.Where(p => p.MULTIID == multiid).OrderBy(p => p.ID).ToList();
                }
                else if (jobtype.CompareTo("MDST") == 0 && worktype == "O")
                {
                    pep_his = PepsHisto.Where(p => p.MULTIID == multiid).OrderBy(p => p.ID).ToList();
                }
#endif
            }
            catch (Exception ex)
            {
                Logger.Inst.Write(CmdLogType.Db, $"Exception. FindBatchId({multiid}).\r\n{ex.Message}\r\n{ex.StackTrace}");
            }
            return pep_his;
        }
        public int FindBatchId(string batchid)
        {
            int COUNT = 0;
            try
            {
                COUNT = Peps.Where(p => p.BATCHID == batchid).Count();
            }
            catch (Exception ex)
            {
                Logger.Inst.Write(CmdLogType.Db, $"Exception. FindBatchId({batchid}).\r\n{ex.Message}\r\n{ex.StackTrace}");
            }
            return COUNT;
        }
        // sql = SELECT * FROM mplus.pepschedule WHERE EXECUTE_TIME = '1555555555' && (S_EQPID = 'RO1018-1' || T_EQPID = 'RO1018-1') && (WORKTYPE = 'O' || WORKTYPE = 'OI' || WORKTYPE = 'I') && (_state = NULL || _state = 0) && (_srcAssignTime = NULL);
        // jm.choi - 190409
        // Dst 작업이 Multi로 진행될경우에도 Busy가 발생하여야하므로
        // p.C_state == 8, p.C_dstAssignTime == null 조건 추가
        // 현재 Job은 제외하여야하므로 (p.BATCHID != batchid) 조건 추가
        public int IsJobCompCheck(string exetime, string eqpId, string batchid)
        {
            int peps_count = 0;

#if true
            peps_count += Peps.Where(p => (p.EXECUTE_TIME == exetime) && (p.T_EQPID == eqpId)
                        && (p.C_state == null || p.C_state < 10) && (p.BATCHID != batchid) && !(p.BATCHID.Contains("PFS"))).Count();

            peps_count += Peps.Where(p => (p.EXECUTE_TIME == exetime) && (p.S_EQPID == eqpId)
                        && (p.C_state == null || p.C_state == 0) && (p.BATCHID != batchid) && !(p.BATCHID.Contains("PFS"))).Count();
#else
            if (worktype == "FSI")
            {
                foreach (var x in all_worktype)
                {
                    peps_count += Peps.Where(p => (p.EXECUTE_TIME == exetime) && (p.T_EQPID == eqpId) && (p.WORKTYPE == x)
                                && (p.C_state == null || p.C_state < 10) && (p.BATCHID != batchid)).Count();
                }
            }
            else if (worktype == "FSO")
            {
                peps_count = Peps.Where(p => (p.EXECUTE_TIME == exetime) && (p.S_EQPID == eqpId) && (p.WORKTYPE == worktype)
                            && (p.C_state == null || p.C_state < 10) && (p.BATCHID != batchid)).Count();
            }
            else if (worktype == "MGI")
            {
                peps_count = Peps.Where(p => (p.EXECUTE_TIME == exetime) && (p.T_EQPID == eqpId) && (p.WORKTYPE == worktype)
                            && (p.C_state == null || p.C_state < 10) && (p.BATCHID != batchid)).Count();
            }
            else if (worktype == "MGO")
            {
                peps_count = Peps.Where(p => (p.EXECUTE_TIME == exetime) && (p.S_EQPID == eqpId) && (p.WORKTYPE == worktype)
                            && (p.C_state == null || p.C_state < 10) && (p.BATCHID != batchid)).Count();
            }
#endif
            return peps_count;
        }

        public void CopyCmdToHistory(pepschedule src)
        {
            try
            {
                pepschedule_history dst = new pepschedule_history
                {
                    ID                  = src.ID,
                    MULTIID             = src.MULTIID,
                    BATCHID             = src.BATCHID,
                    S_EQPID             = src.S_EQPID,
                    S_PORT              = src.S_PORT,
                    S_SLOT              = src.S_SLOT,
                    T_EQPID             = src.T_EQPID,
                    T_PORT              = src.T_PORT,
                    T_SLOT              = src.T_SLOT,
                    C_mgtype            = src.C_mgtype,
                    TRAYID              = src.TRAYID,
                    WORKTYPE            = src.WORKTYPE,
                    TRANSFERTYPE        = src.TRANSFERTYPE,
                    WINDOW_TIME         = src.WINDOW_TIME,
                    EXECUTE_TIME        = src.EXECUTE_TIME,
                    REAL_TIME           = src.REAL_TIME,
                    STATUS              = src.STATUS,
                    LOT_NO              = src.LOT_NO,
                    QTY                 = src.QTY,
                    STEPID              = src.STEPID,
                    S_STEPID            = src.S_STEPID,
                    T_STEPID            = src.T_STEPID,
                    URGENCY             = src.URGENCY,
                    FLOW_STATUS         = src.FLOW_STATUS,
                    C_VEHICLEID         = src.C_VEHICLEID,
                    C_bufSlot           = src.C_bufSlot,
                    C_state             = src.C_state,
                    C_srcAssignTime     = src.C_srcAssignTime,
                    C_srcArrivingTime   = src.C_srcArrivingTime,
                    C_srcStartTime      = src.C_srcStartTime,
                    C_srcFinishTime     = src.C_srcFinishTime,
                    C_dstAssignTime     = src.C_dstAssignTime,
                    C_dstArrivingTime   = src.C_dstArrivingTime,
                    C_dstStartTime      = src.C_dstStartTime,
                    C_dstFinishTime     = src.C_dstFinishTime,
                    C_isChecked         = src.C_isChecked,
                    C_priority          = src.C_priority,
                    DOWNTEMP            = src.DOWNTEMP,
                    EVENT_DATE          = src.EVENT_DATE,
                    ORDER               = src.ORDER,
                };
                Add(dst);
            }
            catch(Exception ex)
            {
                Logger.Inst.Write(CmdLogType.Db, $"Exception. CopyCmdToHistory. {ex.Message}\r\n{ex.StackTrace}");
                return;
            }
        }
        /// <summary>
        /// Cmd수행완료시 pepschedule 테이블 ==> pepschedule_history로 이관
        /// </summary>
        /// <param name="cmdId">pepschedule 테이블의 id(자동증가),key값</param>
        /// <remarks>TODO: 테이블을 1달 또는 12달 있다가 삭제하는것 대신 로컬 파일로 떨궈 보관하는 방법에 대해 고민</remarks>
        public void CopyCmdToHistory(string cmdId)
        {
            try
            {
                var src = Peps.Where(p => p.ID.ToString() == cmdId).Single();
                pepschedule_history dst = new pepschedule_history
                {
                    ID                  = src.ID,
                    MULTIID             = src.MULTIID,
                    BATCHID             = src.BATCHID,
                    S_EQPID             = src.S_EQPID,
                    S_PORT              = src.S_PORT,
                    S_SLOT              = src.S_SLOT,
                    T_EQPID             = src.T_EQPID,
                    T_PORT              = src.T_PORT,
                    T_SLOT              = src.T_SLOT,
                    C_mgtype            = src.C_mgtype,
                    TRAYID              = src.TRAYID,
                    WORKTYPE            = src.WORKTYPE,
                    TRANSFERTYPE        = src.TRANSFERTYPE,
                    WINDOW_TIME         = src.WINDOW_TIME,
                    EXECUTE_TIME        = src.EXECUTE_TIME,
                    REAL_TIME           = src.REAL_TIME,
                    STATUS              = src.STATUS,
                    LOT_NO              = src.LOT_NO,
                    QTY                 = src.QTY,
                    STEPID              = src.STEPID,
                    S_STEPID            = src.S_STEPID,
                    T_STEPID            = src.T_STEPID,
                    URGENCY             = src.URGENCY,
                    FLOW_STATUS         = src.FLOW_STATUS,
                    C_VEHICLEID         = src.C_VEHICLEID,
                    C_bufSlot           = src.C_bufSlot,
                    C_state             = src.C_state,
                    C_srcAssignTime     = src.C_srcAssignTime,
                    C_srcArrivingTime   = src.C_srcArrivingTime,
                    C_srcStartTime      = src.C_srcStartTime,
                    C_srcFinishTime     = src.C_srcFinishTime,
                    C_dstAssignTime     = src.C_dstAssignTime,
                    C_dstArrivingTime   = src.C_dstArrivingTime,
                    C_dstStartTime      = src.C_dstStartTime,
                    C_dstFinishTime     = src.C_dstFinishTime,
                    C_isChecked         = src.C_isChecked,
                    C_priority          = src.C_priority,
                    DOWNTEMP            = src.DOWNTEMP,
                    EVENT_DATE          = src.EVENT_DATE,
                    ORDER               = src.ORDER,
                };
                Add(dst);
            }
            catch (Exception ex)
            {
                Logger.Inst.Write(CmdLogType.Db, $"Exception. CopyCmdToHistory. {ex.Message}\r\n{ex.StackTrace}");
                return;
            }

            //var delCmd = PepsHisto.Where(x => x.EXECUTE_TIME.CompareTo(DateTime.Now.AddMonths(-12)) < 0).OrderBy(p => p.EXECUTE_TIME).ToList();
            //if (delCmd != null)
            //{
            //    Delete(delCmd);
            //}
        }

        /// <summary>
        /// 알람해제시 alarm 테이블 ==> alarm_history 테이블로 이관
        /// </summary>
        /// <param name="alarmDbId">알람 테이블 idx</param>
        /// <remarks>TODO: 테이블을 1달 또는 12달 있다가 삭제하는것 대신 로컬 파일로 떨궈 보관하는 방법에 대해 고민</remarks>
        public void CopyAlarmToHistory(int alarmDbId)
        {
            try
            {
                var src = Alarms.Where(p => p.idx == alarmDbId).Single();
                alarm_history dst = new alarm_history()
                {
                    idx = src.idx,
                    ID = src.ID,
                    code = src.code,
                    msg = src.msg,
                    level = src.level,
                    eventTime = src.eventTime,
                    releaseTime = DateTime.Now,
                };
                Add(dst);
            }
            catch (Exception ex)
            {
                Logger.Inst.Write(CmdLogType.Db, $"Exception. CopyAlarmToHistory. {ex.Message}\r\n{ex.StackTrace}");
                return;
            }

            var delCmd = AlarmHisto.Where(x => x.eventTime.CompareTo(DateTime.Now.AddMonths(-12)) < 0).OrderBy(p => p.eventTime).ToList();
            if (delCmd != null)
            {
                Delete(delCmd);
            }
        }
    }
}
