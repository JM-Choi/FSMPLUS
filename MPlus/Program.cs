using FSMPlus.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FSMPlus
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            int cnt = 0;
            //System.Version assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            //DateTime bulidDate = new DateTime(2000, 1, 1).AddDays(assemblyVersion.Build).AddSeconds(assemblyVersion.Revision * 2);
            //string version = bulidDate.ToString("yyyy-MM-dd");
            string version = "20181117.0-1";
            string title = "FSMPlus";

            Process[] procs = Process.GetProcesses();
            foreach (Process p in procs)
            {
                if (p.ProcessName.Equals(title) == true)
                {
                    cnt++;
                }
            }

            if (cnt > 1)
            {
                Process[] processList = Process.GetProcessesByName("MPlus");
                while (processList.Length >= 1)
                {
                    processList[0].Kill();
                }
                MessageBox.Show("MPlus를 다시 실행해주십시오.");
                BringForgroundWindow.FocusProcess(title);
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                FormSplash.ShowSplashScreen();

                var mainForm = new FormMain(version);
                FormSplash.CloseForm();
                Application.Run(mainForm);
            }
        }

        /// <summary>
        /// 프로그램 타이틀을 이용. 윈도우 메시지 SW_RESTORE 수행
        /// </summary>
        public static class BringForgroundWindow
        {
            [DllImport("user32.dll")]
            public static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);
            [DllImport("user32.dll")]
            public static extern bool SetForegroundWindow(IntPtr WindowHandle);
            public const int SW_RESTORE = 9;

            public static void FocusProcess(string procName)
            {
                Process[] objProcesses = System.Diagnostics.Process.GetProcessesByName(procName);
                if (objProcesses.Length > 0)
                {
                    IntPtr hWnd = IntPtr.Zero;
                    hWnd = objProcesses[0].MainWindowHandle;
                    ShowWindowAsync(new HandleRef(null, hWnd), SW_RESTORE);
                    SetForegroundWindow(objProcesses[0].MainWindowHandle);
                }
            }
        }
    }
}
