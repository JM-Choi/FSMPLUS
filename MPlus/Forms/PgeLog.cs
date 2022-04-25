using FSMPlus.Ref;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FSMPlus.Forms
{
    public partial class PgeLog : Form
    {
        private Thread thdMsgAdder;
        private ConcurrentQueue<LogMsg> CCQlogMsg = new ConcurrentQueue<LogMsg>();
        private System.Windows.Forms.Timer tmrlogAll = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer tmrlogComm = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer tmrlogDb = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer tmrlogRv = new System.Windows.Forms.Timer();
        public class LogMsg
        {
            public CmdLogType logType;
            public string msg;
        }

        public PgeLog()
        {
            InitializeComponent();

            thdMsgAdder = new Thread(msgAddProcess)
            {
                IsBackground = true
            };

            tabControl1.TabPages[0].Text = "PepsDb";
            tabControl1.TabPages[1].Text = "RV";
            tabControl1.TabPages[2].Text = "Comm";
            tabControl1.TabPages[3].Text = "Debug";
            tabControl1.SelectedIndex = 3;
            thdMsgAdder.Start();
        }

        public void SetLog(CmdLogType type, string msg)
        {
            CCQlogMsg.Enqueue(new LogMsg() { logType = type, msg = msg });
        }

        private const int maxLine = 500;
        private bool isThRun = true;

        public bool bLogAllPause { get; private set; }
        public bool bLogComm { get; private set; }
        public bool bLogDb { get; private set; }
        public bool bLogRv { get; private set; }

        private void msgAddProcess()
        {
            while (isThRun)
            {
                if (CCQlogMsg.Count > 0)
                {
                    LogMsg temp = new LogMsg();
                    if (!CCQlogMsg.TryDequeue(out temp))
                    {
                        Thread.Sleep(1);
                        continue;
                    }
                    if (temp == null) { return; }
                    string tempStr = string.Format("{0}{1}", DateTime.Now.ToString("HH:mm:ss.fff] "), temp.msg);
                    RichTextBox target = null;
                    
                    for (int i = 0; i < 2; i++)
                    {
                        if (i == 0)
                        {
                            if (temp.msg.Contains("STATUS"))
                            {
                                string status_chk = temp.msg.Substring(5, 6);
                                if (status_chk == "STATUS")
                                {
                                    continue;
                                }
                            }

                            target = logDebug;
                        }
                        else
                        {
                            switch (temp.logType)
                            {
                                case CmdLogType.Comm: target = logComm; break;
                                case CmdLogType.Db: target = logDb; break;
                                case CmdLogType.Rv: target = logRv; break;
                                default: continue;
                            }
                        }

                        try
                        {
                            Invoke(new Action(delegate () // this == Form 이다. Form이 아닌 컨트롤의 Invoke를 직접호출해도 무방하다.
                            {
                                if (tempStr.IndexOf("\r\n") < 0)
                                {
                                    tempStr += "\r\n";
                                }
                                
                                target.AppendText(tempStr);
                                
                                if (target.Lines.Count() > maxLine)
                                {
                                    target.SelectionStart = 0;
                                    target.SelectionLength = target.GetFirstCharIndexFromLine(1);
                                    target.SelectedText = "";
                                }
                                target.SelectionStart = target.Text.Length;
                                target.Focus();
                                target.ScrollToCaret();
                            }));
                        }
                        catch (Exception ex)
                        {
                           Logger.Inst.Write(CmdLogType.Rv, $"Exception. msgAddProcess. {ex.Message}\r\n{ex.StackTrace}");
                        }
                    }
                }
                Thread.Sleep(5);
            }
        }
    }
}
