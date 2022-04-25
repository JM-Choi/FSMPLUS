using FSMPlus.Ref;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FSMPlus.Forms
{
    public partial class Frm_Log : Form
    {
        PgeLog[] _Pgelog;
        public Frm_Log()
        {
            InitializeComponent();
            tab_Log.TabPages.Clear();
            tab_Log.ImageList = imageList1;

            int idx = 0;
            string sTitle = string.Empty;
            TabPage _Page;
            _Pgelog = new PgeLog[Global.Init.Db.Vechicles.Where(p=>p.isUse == 1).Count()];
            foreach (var item in Global.Init.Db.Vechicles.Where(p => p.isUse == 1).ToList())
            {
                _Pgelog[idx] = new PgeLog();
                _Pgelog[idx].TopLevel = false;
                _Pgelog[idx].Dock = DockStyle.Fill;

                sTitle = $"{item.ID}";
                _Page = new TabPage(sTitle);
                _Pgelog[idx].Parent = _Page;
                _Page.Controls.Add(_Pgelog[idx]);
                _Page.ImageIndex = 1;
                tab_Log.TabPages.Add(_Page);
                _Pgelog[idx].Show();
                idx++;
            }
        }

        public void SetLogMsg(LOGCMD nID, int nVecID, CmdLogType type, string msg )
        {
            switch (nID)
            {
                case LOGCMD.Job:
                    _Pgelog[nVecID].SetLog(type, msg);
                    break;
                case LOGCMD.Vehicle:
                    int idx = nVecID ;
                    _Pgelog[nVecID].SetLog(type, msg);
                    break;
                case LOGCMD.Etc:
                default: break;
            }
        }
    }
}
