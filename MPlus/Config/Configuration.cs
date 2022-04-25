using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMPlus
{
    [DefaultPropertyAttribute("Common")]
    public class CfgData
    {
        private int _passedtime;
        private int _goretry;

        [Category("Local Network Settings"), DisplayName("MPlus 의 IP 주소"), Description("MPlus 의 IP 주소")]
        public string LocalIp { get; set; }

        [Category("Local Network Settings"), DisplayName("MPlus 의 Port 번호"), Description("MPlus 의 Port 번호")]
        public int LocalPort { get; set; }

        [Category("Rv Settings"), DisplayName("RV 통신 사용유무"), Description("RV 통신 사용유무")]
        public bool UseRv { get; set; }

        [Category("Rv Settings"), DisplayName("RV Service Port 번호"), Description("RV 의 Service Port 번호")]
        public string Service { get; set; }

        [Category("Rv Settings"), DisplayName("RV 서버의 Network 주소"), Description("RV 서버의 Network 주소")]
        public string Network { get; set; }

        [Category("Rv Settings"), DisplayName("RV Daemon Port 번호"), Description("RV Daemon Port 번호")]
        public string Daemon { get; set; }

        [Category("Rv Settings"), DisplayName("RV Listener 토픽 이름"), Description("RV Listener 토픽 이름")]
        public string ListenerTopics { get; set; }

        [Category("Rv Settings"), DisplayName("RV Sender 토픽 Prefix"), Description("RV Sender 토픽 Prefix")]
        public string SenderSubject { get; set; }

        [Category("Rv Settings"), DisplayName("RV 통신 타임아웃"), Description("RV 통신 타임아웃")]
        public int RvTimeOut { get; set; }

        [Category("Rv Settings"), DisplayName("RV 에러시 딜레이타임"), Description("RV 에러시 딜레이타임")]
        public int RvErrDelayTime { get; set; }

        [Category("Rv Settings"), DisplayName("RV 설비상태체크 Main 재시도 최대횟수"), Description("RV 설비상태체크 Main 재시도 최대횟수")]
        public int RvMainRetrylimit { get; set; }

        [Category("Rv Settings"), DisplayName("RV 설비상태체크 Sub 재시도 최대횟수"), Description("RV 설비상태체크 Sub 재시도 최대횟수")]
        public int RvSubRetrylimit { get; set; }
        
        [Category("Db Settings"), DisplayName("MPlus'DB 의 연결스트링"), Description("MPlus'DB 의 연결스트링")]
        public string ConnectionString{ get; set; }

        [Category("Log Settings"), DisplayName("Log 폴더 설정"), Description("Log 폴더 설정")]
        public string LogPath { get; set; }

        [Category("Log Settings"), DisplayName("Log 최대 보관일"), Description("Log 최대 보관일")]
        public int LogHoldDay { get;  set; }

        [Category("Log Settings"), DisplayName("Log 자동삭제 기능 사용유무"), Description("Log 자동삭제 기능 사용유무")]
        public bool UseLogAutoDel { get; set; }

        [Category("Charge"), DisplayName("Standby 기능 사용유무"), Description("Standby 기능 사용유무")]
        public bool UseStandby { get; set; }

        [Category("Charge"), DisplayName("AutoCharge 기능 사용유무"), Description("AutoCharge 기능 사용유무")]
        public bool UseAutoCharge { get; set; }

        [Category("Charge"), DisplayName("AutoCharge 시작 기준치(%)"), Description("AutoCharge 시작 기준치(%)")]
        public int ChargeStart { get; set; }

        [Category("Charge"), DisplayName("AutoCharge 완료 기준치(%)"), Description("AutoCharge 완료 기준치(%)")]
        public int ChargeEnd { get; set; }

        [Category("Charge"), DisplayName("대기위치 이동 전 대기시간(초)"), Description("대기위치 이동 전 대기시간(초)")]
        public int IdleDuration { get; set; }

        [Category("Charge"), DisplayName("상태 변화 체크 주기(초)"), Description("상태 변화 체크 주기(초)")]
        public int StatusCheckDuration { get; set; }

        [Category("Option"), DisplayName("스케쥴 확인시 대기시간(msec)"), Description("스케쥴 확인시 지정시간 대기 후 최종 스케쥴 재확인(밀리세컨드)")]
        public int ScheduledDelay { get; set; } = 1000;     // default 1초, 음수(-)는 막자

        [Category("Option"), DisplayName("작업 완료 후 다음 작업까지의 대기시간(초)"), Description("작업 완료 후 해당 시간 안에 다음 작업이 없을 경우 충전으로 이동")]
        public int PassedTime
        {
            get { return _passedtime; }
            set
            {
                if (value < 0)
                    _passedtime = 0;     // default 1초, 음수(-)는 막자
                else
                    _passedtime = value;
            }
        }

        [Category("Option"), DisplayName("I Type Tray 선배출시간(sec)"), Description("I Type Unload 시 STK에서 ExecuteTime 보다 선배출하는 시간(sec)")]
        public int early_Time { get; set; } = 120;     // default 1초, 음수(-)는 막자

        [Category("Option"), DisplayName("Go Fail 시 Retry 시도 횟수"), Description("Go Fail 시 Go 명령을 다시 시도하는 횟수")]
        public int goRetryMax
        {
            get { return _goretry; }
            set
            {
                if (value < 1)
                    _goretry = 1;     // default 1번, 음수(-)는 막자
                else
                    _goretry = value;
            }
        }
    }

    public class Configuration
    {
        private static volatile Configuration instance;
        private static object syncConfig = new object();

        public static Configuration Init
        {   get
            {   if (instance == null)
                {   lock(syncConfig)
                    {   instance = new Configuration();
                    }
                }
                return instance;
            }
        }
        public CfgData Data;

        private Configuration()
        {
            Data = new CfgData();
            if (XmlFileHandler.ConfigLoad($"LocalData.xml", ref Data))
            {
                Console.WriteLine("LocalData.xml 파일을 찾을 수 없습니다");
            }
        }
        
        public void SaveConfiguration(CfgData cfg)
        {
            XmlFileHandler.ConfigSave("LocalData.xml", ref Data);
        }
    }
}
