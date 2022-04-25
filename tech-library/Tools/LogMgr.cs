using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tech_library.Tools
{
    public enum LogType { Daily, Monthly }
    public class LogMgr
    {
        private string _path;

        #region Constructor
        public LogMgr(string path, LogType logType, string prefix, string postfix)
        {
            _path = path;
            _SetLogPath(logType, prefix, postfix);
        }

        public LogMgr(string prefix, string postfix)
            : this(Path.Combine(EnvMgr.Root, "Log"), LogType.Daily, prefix, postfix)
        {
        }

        public LogMgr()
            : this(Path.Combine(EnvMgr.Root, "Log"), LogType.Daily, null, null)
        {
        }
        #endregion

        private void _SetLogPath(LogType logType, string prefix, string postfix)
        {
            string path = string.Empty;
            string name = string.Empty;

            switch(logType)
            {
                case LogType.Daily:
                    path = String.Format(@"\{0}\{1}\", DateTime.Now.Year, DateTime.Now.ToString("MM"));
                    name = DateTime.Now.ToString("yyyyMMdd");
                    break;
                case LogType.Monthly:
                    path = String.Format(@"\{0}\", DateTime.Now.Year);
                    name = DateTime.Now.ToString("yyyyMM");
                    break;
            }

            _path = Path.Combine(_path + path);
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);

            if (!String.IsNullOrEmpty(prefix))
                name = prefix + name;

            if (!String.IsNullOrEmpty(postfix))
                name = name + postfix;

            name += ".txt";

            _path = Path.Combine(_path, name);
        }

        public void Write(string data)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(_path, true))
                {
                    writer.Write(data);
                }
            }
            catch (Exception)
            { }
        }

        public void WriteLine(string data)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(_path, true))
                {
                    writer.WriteLine(DateTime.Now.ToString("yyyyMMdd HH:mm:ss\t") + data);
                }
            }
            catch(Exception)
            { }
        }
    }
}
