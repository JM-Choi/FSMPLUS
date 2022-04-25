using FSMPlus.Ref;
using FSMPlus.Vehicles;
using FSMPlus.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMPlus
{
    public class DeleteEventArgs : EventArgs
    {
        public string cmdID = "";
    }

    public class Global
    {
        #region singleton Global
        private static volatile Global instance;
        private static object syncVsp = new object();
        public static Global Init
        {
            get
            {
                if (instance == null)
                {
                    lock (syncVsp)
                    {
                        instance = new Global();
                    }
                }
                return instance;
            }
        }
        #endregion
        public SetRobot[] setRobots;
        //public List<Robot> Robots = new List<Robot>();
        protected static MainHandler _MainHandler;
        public static MapDrawer _MapDraw = new MapDrawer();
        private static Configuration _cfg = Configuration.Init;
        private static DbHandler _db = DbHandler.Inst;
        private static VSP _vsp = VSP.Init;
        private static VehicleEventParser _vep = VehicleEventParser.Init;
        private static Dictionary<string/*VehicleName*/, JobProccess> _JobProcList = new Dictionary<string, JobProccess>();
        private static Dictionary<string/*VehicleName*/, Robot> _RobotList = new Dictionary<string, Robot>();
        private static Dictionary<string/*VehicleName*/, RvMsg> _RvMsgList = new Dictionary<string, RvMsg>();
        private static Proc_Atom _poa = Proc_Atom.Init;

        // Local Test 시 RV 통신 대신 Socket 통신으로 RV Test를 진행하기 위하여 RV용 Socket을 만들고 Global로 사용
        private static RvListener _rvListener = RvListener.Init;
        //

        public SetRv setRv = new makeRv();
        public RvComu _rvcomu;
        public RvComu RvComu
        {
            get { return _rvcomu; }
            set { _rvcomu = value; }
        }

        public Configuration Cfg
        {   get { return _cfg; }
            set { _cfg = value; }
        }
        public DbHandler Db
        {
            get { return _db; }
            set { _db = value; }
        }
        public VSP Vsp
        {
            get { return _vsp; }
            set { _vsp = value; }
        }
        public VehicleEventParser VEP
        {
            get { return _vep; }
            set { _vep = value; }
        }
        public RvListener RvLsner
        {
            get { return _rvListener; }
            set { _rvListener = value; }
        }
        public Dictionary<string, JobProccess> JobProcList
        {
            get { return _JobProcList; }
            set { _JobProcList = value; }
        }
        public Dictionary<string, Robot> RobotList
        {
            get { return _RobotList; }
            set { _RobotList = value; }
        }
        public Dictionary<string, RvMsg> RvMsgList
        {
            get { return _RvMsgList; }
            set { _RvMsgList = value; }
        }
        // controller 관련 중복 오류 메시지 발생 방지 Flag
        private bool _bAuto;
        public bool bAuto
        {   get { return _bAuto; }
            set { _bAuto = value; }
        }

        private bool _bPause;
        public bool bPause
        {
            get { return _bPause; }
            set { _bPause = value; }
        }

        public string _First_Vehicle;
        public string First_Vehicle
        {
            get { return _First_Vehicle; }
            set { _First_Vehicle = value; }
        }

        public Global()
        {
            Db.OnChangeTableData += _Db_OnChangeTableData;
        }

        virtual protected void _Db_OnChangeTableData(object sender, TableUpdateArgs e)
        {
        }

        public void RendezvousSetup(List<vehicle> vecs)
        {
            Global.Init.RvComu = setRv.CreateRV(Cfg.Data.Service, Cfg.Data.Network, Cfg.Data.Daemon, Cfg.Data.ListenerTopics, Cfg.Data.SenderSubject);
            Global.Init.RvComu.RvStart();

            RvMsgList.Clear();
            if (vecs.Count() > 0)
                First_Vehicle = vecs[0].ID;
            foreach (var v in vecs)
            {
                RvMsgList[v.ID] = new Rv_Type_FS();
            }
            RvMsgList["PROGRAM"] = new Rv_Type_FS();


        }
        #region controller status

        /// <summary>
        /// M+ controller 의 상태가 auto 이면 true, 아니면 false
        /// </summary>
        /// <param name="form"></param>
        /// <param name="b">동일상태의 중복메시지가 발생되지 않도록 flag 처리</param>
        /// <returns></returns>
        protected bool IsControllerAuto()
        {
            // DB에 저장된 Mplus 상태 가져오기
            var controller = Db.Controllers.SingleOrDefault();
            // 상태가 null 이나
            if (controller == null)
            {
                // 내부 상태가 true 이면 Error 처리
                if (bAuto == true)
                {
                    bAuto = false;
                    Logger.Inst.Write(CmdLogType.Db, $"Not found Controller Infomation");

                    AlphaMessageForm.FormMessage form = new AlphaMessageForm.FormMessage(string.Empty, AlphaMessageForm.MsgType.Error, AlphaMessageForm.BtnType.Ok);
                    form.SetMsg("Not found Controller Infomation");
                    form.ShowDialog();
                }
                return false;
            }

            if (!bAuto)
                bAuto = true;

            // DB에 저장된 상태가 Auto 가 아니면 false
            if (controller.C_state != (int)ControllerState.AUTO)
                return false;

            return true;
        }
        #endregion controller status

        #region db search
        protected vehicle SelectVehicleByID(string id)
        {
            vehicle vec = new vehicle();
            try
            {   vec = Db.Vechicles.Where(p => p.ID == id).FirstOrDefault();
            }
            catch(Exception ex)
            {   Logger.Inst.Write(CmdLogType.Rv, $"Exception. SelectVehicleByID. {ex.Message}\r\n{ex.StackTrace}");
                return (vehicle)null;
            }

            if (vec == null)
                return (vehicle)null;
            return vec;
        }

        protected void UpdateVehicleByID(string vecid, string batchid)
        {
            try
            {   vehicle vec = Db.Vechicles.Where(p => p.ID == vecid).FirstOrDefault();
                if (vec != null)
                {   vec.C_BATCHID = batchid;
                    Db.DbUpdate(TableType.VEHICLE);
                }
                pepschedule pep = Db.Peps.Where(p => p.BATCHID == batchid).FirstOrDefault();
                if (pep != null)
                {
                    pep.C_VEHICLEID = vecid;
                    Db.DbUpdate(TableType.PEPSCHEDULE);
                }
            }
            catch(Exception ex)
            {   Logger.Inst.Write(vecid, CmdLogType.Rv, $"Exception. SelectVehicleByID. {ex.Message}\r\n{ex.StackTrace}");
                return;
            }
        }

#endregion //db search
    }
}
