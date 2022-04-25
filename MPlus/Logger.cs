using FSMPlus.Ref;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tech_library.Tools;

namespace FSMPlus
{
    public enum LogType
    {
        Debug,
        Info,
        Warn,
        Error,
        Fatal,
    }

    public enum CmdLogType
    {
        All
        , Comm
        , Db
        , Rv
    }

    public class WriteLogArgs : EventArgs
    {
        public LOGCMD cmd;
        public CmdLogType type;
        public string vecID;
        public int nID;
        public string msg;
    }

    public class Logger
    {
        public event EventHandler<WriteLogArgs> OnWriteLog;

        // MPlus db 에 드러난 설정 이외의 프로그램 자체 설정 생성
        private Configuration _cfg = Configuration.Init;

        private static volatile Logger instance;
        private static object syncRoot = new object();

        private string[] _VecID;
        private static ExEzLogger[] Vehiclelog ;

        /*유일한 Logger, instance 를 생성
         * */
        public static Logger Inst
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Logger();
                    }
                }
                return instance;
            }
        }

        private Logger()
        {
        }

        public void MakeVecHdl()
        {
            int idx = 0;

            Debug.Assert(Global.Init.Db.Vechicles.Count() > 0);

            _VecID = new string[Global.Init.Db.Vechicles.Where(p => p.isUse == 1).Count()];
            Vehiclelog = new ExEzLogger[Global.Init.Db.Vechicles.Where(p => p.isUse == 1).Count()];

            foreach (var item in Global.Init.Db.Vechicles.Where(p => p.isUse == 1).ToList())
            {
                _VecID[idx] = item.ID;
                if (_cfg.Data.LogPath?.Length > 0)
                {
                    Vehiclelog[idx] = new ExEzLogger(_cfg.Data.LogPath, item.ID);
                }
                else
                {
                    Vehiclelog[idx] = new ExEzLogger(EnvMgr.Root, item.ID);
                }

                Debug.Assert(Vehiclelog[idx] != null);

                if (Vehiclelog[idx] != null)
                {
                    Vehiclelog[idx].LoggerStart();
                }

                idx++;
            }            
        }

        public void LoggerClose()
        {
            foreach (var item in Vehiclelog)
            {
                item.LoggerStop();
            }
        }

        private int GetVecID( string VecID )
        {
            int idx = -1, chk = 0;
            if (null == _VecID)
                return -1;
            string id = _VecID.Where(p => p == VecID).SingleOrDefault();
            if (null == id)
                return -1;

            if ( VecID == id )
            {
                foreach (var item in _VecID)
                {
                    if ( item == VecID )
                    {
                        idx = chk;
                    }
                    if (-1 != idx) break;
                    chk++;
                }
            }
            return idx ;
        }

        public void Write( CmdLogType type, string msg )
        {
            int nID = GetVecID(Global.Init.First_Vehicle);
            switch (nID)
            {
                case -1: break;
                default:
#if true
                    if (null == Vehiclelog[nID]) return;
                    if (type != CmdLogType.All)
                    {
                        Vehiclelog[nID].StrAdd(CmdLogType.All, msg);
                    }
                    
                    Vehiclelog[nID].StrAdd(type, msg);
                    
                    
#else
                    for (int i = 0; i < 2; i++)
                    {
                        if (null == Vehiclelog[nID]) return;
                        if (i == 0)
                        {
                            Vehiclelog[nID].StrAdd(CmdLogType.All, msg);
                        }
                        else
                        {
                            Vehiclelog[nID].StrAdd(type, msg);
                        }
                    }
#endif


                    OnWriteLog?.Invoke(this, new WriteLogArgs() { cmd = LOGCMD.Job, type = type, nID = nID, vecID = "", msg = msg });
                    break;
            }

        }
        
        public void Write( string VecID, CmdLogType type, string msg)
        {
            if (VecID == "PROGRAM")
                VecID = Global.Init.First_Vehicle;
            int nID = GetVecID(VecID);
            switch (nID)
            {
                case -1: break;
                default:
#if true
                    if (null == Vehiclelog[nID]) return;
                    if (type != CmdLogType.All)
                    {
                        if (msg.Contains("STATUS"))
                        {
                            string test = msg.Substring(5, 6);
                            if (test != "STATUS")
                            {
                                Vehiclelog[nID].StrAdd(CmdLogType.All, msg);
                            }
                        }
                        else
                        {
                            Vehiclelog[nID].StrAdd(CmdLogType.All, msg);
                        }
                    }

                    Vehiclelog[nID].StrAdd(type, msg);
#else
                    for (int i = 0; i < 2; i++)
                    {
                        if (null == Vehiclelog[nID]) return;
                        if (i == 0)
                        {
                            Vehiclelog[nID].StrAdd(CmdLogType.All, msg);
                        }
                        else
                        {
                            Vehiclelog[nID].StrAdd(type, msg);
                        }                        
                    }
#endif                    
                    Debug.WriteLine(msg);
                    //OnWriteLog?.Invoke(this, new WriteLogArgs() { cmd = LOGCMD.Vehicle, type = type, nID = nID + 1, vecID = VecID, msg = msg });'
                    OnWriteLog?.Invoke(this, new WriteLogArgs() { cmd = LOGCMD.Vehicle, type = type, nID = nID, vecID = VecID, msg = msg });

                    break;
            }
        }
    }

    public class ExEzLogger
    {
        struct QueueMsg
        {
            public CmdLogType type;
            public string msg;
        }
        private ConcurrentQueue<QueueMsg> messageQueue = new ConcurrentQueue<QueueMsg>();
        private string _PathFodler;

        public ExEzLogger( string sPath, string sName  )
        {
            _PathFodler = $"{sPath}\\{sName}";
            DirectoryInfo dtif = new DirectoryInfo(_PathFodler);
            if (!dtif.Exists) {
                dtif.Create();
            }
        }

        private void MakeDir(CmdLogType type)
        {
            DateTime Date = DateTime.Now;
            string strY, strM ;
            DirectoryInfo dtif = new DirectoryInfo(_PathFodler);
            strY = string.Format("{0:yyyy}", Date);
            strM = string.Format("{0:MM}", Date);
            dtif = new DirectoryInfo(string.Format("{0}\\{1}\\{2}", _PathFodler, type.ToString(), strY));
            if (!dtif.Exists) {
                dtif.Create();
            }
            dtif = new DirectoryInfo(string.Format("{0}\\{1}\\{2}\\{3}", _PathFodler, type.ToString(), strY, strM));

            strM = string.Format("{0:MM}", DateTime.Now.AddMonths(-3));
            var delDir = string.Format("{0}\\{1}\\{2}\\{3}", _PathFodler, type.ToString(), strY, strM);
            if (!dtif.Exists)
            {
                dtif.Create();
                EmptyDirectory(delDir);
            }
        }

        public void EmptyDirectory(string directory)
        {
            // First delete all the files, making sure they are not readonly
            var stackA = new Stack<DirectoryInfo>();
            stackA.Push(new DirectoryInfo(directory));

            var stackB = new Stack<DirectoryInfo>();
            while (stackA.Any())
            {
                var dir = stackA.Pop();
                foreach (var file in dir.GetFiles())
                {
                    file.IsReadOnly = false;
                    file.Delete();
                }
                foreach (var subDir in dir.GetDirectories())
                {
                    stackA.Push(subDir);
                    stackB.Push(subDir);
                }
            }

            // Then delete the sub directories depth first
            while (stackB.Any())
            {
                stackB.Pop().Delete();
            }
        }

        private string GetFullPath(CmdLogType type)
        {
            DateTime Date = DateTime.Now;
            string strY, strM, strFileName, strFullPath = "";
            strY = string.Format("{0:yyyy}", Date);
            strM = string.Format("{0:MM}", Date);
            strFileName = string.Format("{0}_{1:dd}.txt", type.ToString(), Date);
            strFullPath = string.Format("{0}\\{1}\\{2}\\{3}\\{4}", _PathFodler, type.ToString(), strY, strM, strFileName);
            return string.Format("{0}\\{1}\\{2}\\{3}\\{4}", _PathFodler, type.ToString(), strY, strM, strFileName);
        }

        private string GetTime()
        {
            DateTime NowTime = DateTime.Now;
            return NowTime.ToString("HH:mm:ss.") + NowTime.Millisecond.ToString("000");
        }

        public void Write(CmdLogType type, string LogMsg)
        {            
            MakeDir(type);
            string sFullPath = GetFullPath(type);
            string log = $"{GetTime()} : {LogMsg}";
            FileInfo file = new FileInfo(sFullPath);
            try
            {
                if (!file.Exists)
                {
                    using (StreamWriter sw = new StreamWriter(sFullPath))
                    {
                        sw.WriteLine(log);
                        sw.Close();
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(sFullPath))
                    {                      
                        sw.WriteLine(log); 
                        sw.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Assert(false, "Event Log file write Error.\r\n" + e.ToString());
            }
        }

        public void StrAdd(CmdLogType type, string msg)
        {
            messageQueue.Enqueue(new QueueMsg() { type = type, msg = msg });
        }

        private bool loggerRun = true;
        public void LoggerStart()
        {
            MessageWriter();
        }
        public async void LoggerStop()
        {
            loggerRun = false;
            await Task.Delay(100);
        }
        private async void MessageWriter()
        {
            while (loggerRun)
            {
                try
                {
                    if (messageQueue.Count > 0)
                    {
                        if (messageQueue.TryDequeue(out QueueMsg res))
                        {
                            Write(res.type, res.msg);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Logger Exception : {e.ToString()}");
                }
                await Task.Delay(1);
            }
        }
    }
}
