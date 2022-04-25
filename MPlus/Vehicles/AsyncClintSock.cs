using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.Timers;
using System.Net.NetworkInformation;

namespace FSMPlus.Vehicles
{
    public class ChangeConnectedArgs : EventArgs
    {
        public bool connected;
    }
    public partial class AsyncClintSock
    {
        public class AsyncObject
        {
            public byte[] Buffer;
            public Socket WorkingSocket;
            public Socket rvWorkingSocket;
            public AsyncObject(int bufferSize)
            {
                Buffer = new byte[bufferSize];
            }
        }

        public delegate void RecvStrEvent(object sender, string rcvStr);
        public event RecvStrEvent OnRcvMsg;
        public delegate void RecvDataEvent(object sender, byte[] rcvStr);
        public event RecvDataEvent OnRcvData;
        public event EventHandler<ChangeConnectedArgs> OnChangeConnected;

        private bool g_Connected;
        public bool Connected { get { return g_Connected; } }

        private bool g_Connected_chk;
        public bool Connected_chk
        {
            get { return g_Connected_chk; }
            set { g_Connected_chk = value; }
        }

        private Socket m_ClientSocket = null;

        private AsyncCallback m_fnReceiveHandler;
        private AsyncCallback m_fnSendHandler;

        private System.Timers.Timer tmrPing = new System.Timers.Timer();

        public AsyncClintSock(bool val)
        {
            if (val)
            {
                // 비동기 작업에 사용될 대리자를 초기화합니다.
                m_fnReceiveHandler = new AsyncCallback(handleDataReceive);
                m_fnSendHandler = new AsyncCallback(handleDataSend);

                tmrPing.Interval = 1;
                tmrPing.Enabled = false;
                tmrPing.Elapsed += tmrPing_Elapsed1;
            }
            else
            {
                // 비동기 작업에 사용될 대리자를 초기화합니다.
                m_RvfnReceiveHandler = new AsyncCallback(RvhandleDataReceive);
                m_RvfnSendHandler = new AsyncCallback(RvhandleDataSend);

            }
        }

        /*삼성경우 보안으로 방화벽 해제가 불가피하다
         * 따라서 외부 연결을 하기 위해 IP 및 Port까지 방화벽 해제 요청을 해야하고 Client 소켓에 ip, port 지정이 필요하다
         * 또한 통신 상태 점검시 Netstat -na 로 ip:port 의 연결상태를 점검 및 확인할 수 있다.
         * 
         * 다만 해당 소켓에 대해 close 처리가 제대로 안될 때 소켓 binding 오류를 발생시킬 가능성을 가진다.
         * 
         * 방화벽 포트 해제가 필요한 상황에서 분명한 소켓 처리가 요구된다.
         * */

        private bool Bind(string vecid, string bindIp = "", ushort bindPort = 0)
        {
            try
            {
                // TCP 통신을 위한 소켓을 생성합니다.
                m_ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

                IPAddress myIP = IPAddress.Parse(bindIp);
                IPEndPoint ipLocal = new IPEndPoint(myIP, bindPort);
                if (ipLocal.Port != 0)
                {
                    m_ClientSocket.Bind(ipLocal);
                    Logger.Inst.Write(vecid, CmdLogType.Comm, $"Bind success. localip:{bindIp}, localport:{bindPort}");
                }
            }
            catch (Exception ex)
            {
                Logger.Inst.Write(vecid, CmdLogType.Comm, $"Exception. Bind.\r\n{ex.Message}\r\n{ex.StackTrace}");
                return false;
            }
            return true;
        }


        public void ConnectToServer(string hostName, ushort hostPort, string remoteIp, ushort remotePort, string vecid)
        {
            bool isConnected = false;
            try
            {
                if (Bind(vecid, hostName, hostPort))
                {
                    // 연결 시도
                    bool success = m_ClientSocket.BeginConnect(remoteIp, Convert.ToInt32(remotePort), null, null).AsyncWaitHandle.WaitOne(1000, true);
                    m_ClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);
                    m_ClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);

                    Logger.Inst.Write(vecid, CmdLogType.Comm, $"connecting...remoteip:{remoteIp},remoteport:{remotePort}");

                    if (success)
                    {
                        // 연결 성공
                        isConnected = true;
                        Console.WriteLine($"{hostName} : 연결 성공1");
                    }
                }
                else
                {
                    Logger.Inst.Write(vecid, CmdLogType.Comm, "local socket bind fail");
                }
            }
            catch
            {
                // 연결 실패 (연결 도중 오류가 발생함)
                isConnected = false;
                Console.WriteLine($"{hostName} : 연결 실패1");
            }
            g_Connected = isConnected;

            if (isConnected)
            {
                try
                {
                    // 4096 바이트의 크기를 갖는 바이트 배열을 가진 AsyncObject 클래스 생성
                    AsyncObject ao = new AsyncObject(4096)
                    {
                        // 작업 중인 소켓을 저장하기 위해 sockClient 할당
                        WorkingSocket = m_ClientSocket
                    };

                    // 비동기적으로 들어오는 자료를 수신하기 위해 BeginReceive 메서드 사용!
                    m_ClientSocket.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnReceiveHandler, ao);

                    //Logger.Inst.Write(CmdLogType.prdt, $"{hostName} : 연결 성공");
                    Console.WriteLine($"{hostName} : 연결 성공2");
                    OnChangeConnected?.Invoke(this, new ChangeConnectedArgs() { connected = true });
                }
                catch(Exception ex)
                {
                    Logger.Inst.Write(CmdLogType.Comm, $"{ex.Message}\r\n{ex.StackTrace}");
                    Console.WriteLine($"{hostName} : 연결 중 오류발생");
                    OnChangeConnected?.Invoke(this, new ChangeConnectedArgs() { connected = false });
                    StopClient();
                }
            }
            else
            {
                //Debug.WriteLine("연결 실패!");
            }
        }

        public void StopClient()
        {
            // 가차없이 클라이언트 소켓을 닫습니다.
            if (m_ClientSocket != null)
            {
                m_ClientSocket.Close();
                g_Connected = false;
                m_ClientSocket.Dispose();
                m_ClientSocket = null;
                Console.WriteLine($"클라이언트소켓 삭제");
            }
        }

        public void SendMessage(string message)
        {
            
            // 추가 정보를 넘기기 위한 변수 선언
            // 크기를 설정하는게 의미가 없습니다.
            // 왜냐하면 바로 밑의 코드에서 문자열을 유니코드 형으로 변환한 바이트 배열을 반환하기 때문에
            // 최소한의 크기르 배열을 초기화합니다.
            AsyncObject ao = new AsyncObject(1)
            {
                // 문자열을 바이트 배열으로 변환
                //ao.Buffer = Encoding.Unicode.GetBytes(message);
                Buffer = Encoding.UTF8.GetBytes(message),
                WorkingSocket = m_ClientSocket
            };

            // 전송 시작!
            try
            {
                m_ClientSocket.BeginSend(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnSendHandler, ao);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"전송 중 오류 발생!\n메세지: {ex.Message}");
                StopClient();
                g_Connected = false;
                OnChangeConnected?.Invoke(this, new ChangeConnectedArgs() { connected = false });
            }
        }

        public void SendData(byte[] dataArray)
        {
            // 추가 정보를 넘기기 위한 변수 선언
            // 크기를 설정하는게 의미가 없습니다.
            // 왜냐하면 바로 밑의 코드에서 문자열을 유니코드 형으로 변환한 바이트 배열을 반환하기 때문에
            // 최소한의 크기르 배열을 초기화합니다.
            AsyncObject ao = new AsyncObject(1)
            {
                Buffer = dataArray,
                WorkingSocket = m_ClientSocket
            };

            // 전송 시작!
            try
            {
                m_ClientSocket.BeginSend(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnSendHandler, ao);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"전송 중 오류 발생!\n메세지: {ex.Message}");
                StopClient();
                g_Connected = false;
                OnChangeConnected?.Invoke(this, new ChangeConnectedArgs() { connected = false });
            }
        }

        private void handleDataReceive(IAsyncResult ar)
        {
            // 넘겨진 추가 정보를 가져옵니다.
            // AsyncState 속성의 자료형은 Object 형식이기 때문에 형 변환이 필요합니다~!
            AsyncObject ao = (AsyncObject)ar.AsyncState;

            // 받은 바이트 수 저장할 변수 선언
            Int32 recvBytes;
            try
            {
                // 자료를 수신하고, 수신받은 바이트를 가져옵니다.
                recvBytes = ao.WorkingSocket.EndReceive(ar);
            }
            catch(Exception ex)
            {
                // 예외가 발생하면 함수 종료!
                Logger.Inst.Write(CmdLogType.Comm, $"Exception. handleDataReceive.자료 수신 도중 오류 발생!\r\n{ex.Message}\r\n{ex.StackTrace}");
                StopClient();
                g_Connected = false;
                return;
            }

            // 수신받은 자료의 크기가 1 이상일 때에만 자료 처리
            if (recvBytes > 0)
            {
                // 공백 문자들이 많이 발생할 수 있으므로, 받은 바이트 수 만큼 배열을 선언하고 복사한다.
                byte[] msgByte = new byte[recvBytes];
                Array.Copy(ao.Buffer, msgByte, recvBytes);

                OnRcvData?.Invoke(this, msgByte);
                OnRcvMsg?.Invoke(this, Encoding.ASCII.GetString(msgByte));

                // 받은 메세지를 출력
                //Debug.WriteLine("메세지 받음: {0}", Encoding.Unicode.GetString(msgByte));
                //Debug.WriteLine("메세지 받음: {0}", Encoding.ASCII.GetString(msgByte));
            }

            try
            {
                // 자료 처리가 끝났으면~
                // 이제 다시 데이터를 수신받기 위해서 수신 대기를 해야 합니다.
                // Begin~~ 메서드를 이용해 비동기적으로 작업을 대기했다면
                // 반드시 대리자 함수에서 End~~ 메서드를 이용해 비동기 작업이 끝났다고 알려줘야 합니다!
                ao.WorkingSocket.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnReceiveHandler, ao);
            }
            catch (Exception ex)
            {
                // 예외가 발생하면 예외 정보 출력 후 함수를 종료한다.
                Logger.Inst.Write(CmdLogType.Comm, $"Exception. handleDataReceive.자료 송신 도중 오류 발생!\r\n{ex.Message}\r\n{ex.StackTrace}");
                StopClient();
                g_Connected = false;
                return;
            }
        }

        private void handleDataSend(IAsyncResult ar)
        {
            // 넘겨진 추가 정보를 가져옵니다.
            AsyncObject ao = (AsyncObject)ar.AsyncState;

            // 보낸 바이트 수를 저장할 변수 선언
            int sentBytes;
            try
            {
                // 자료를 전송하고, 전송한 바이트를 가져옵니다.
                sentBytes = ao.WorkingSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                // 예외가 발생하면 예외 정보 출력 후 함수를 종료한다.
                Logger.Inst.Write(CmdLogType.Comm, $"Exception. handleDataSend.자료 송신 도중 오류 발생!\r\n{ex.Message}\r\n{ex.StackTrace}");
                g_Connected = false;
                return;
            }

            if (sentBytes > 0)
            {
                // 여기도 마찬가지로 보낸 바이트 수 만큼 배열 선언 후 복사한다.
                byte[] msgByte = new byte[sentBytes];
                Array.Copy(ao.Buffer, msgByte, sentBytes);

                //Debug.WriteLine("메세지 보냄: {0}", Encoding.ASCII.GetString(msgByte));
            }
        }

        /// <summary>
        /// connect 상태에서만 구동 - m6x 와의 통신연결상태를 체크
        /// 3초 주기 2초 이내 무응답이면 오류로 체크, 10회 오류를 만날 때 연결종료상태로 판단하고 g_Connected 상태를 바꾼다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private const int ping_err_limit = 10;
        private int ping_err_cnt = 0;
        private void tmrPing_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (g_Connected)
                {   IPEndPoint ip = m_ClientSocket.RemoteEndPoint as IPEndPoint;
                    IPAddress who = ip.Address;

                    int timeout = 2000;
                    string data = "aaaa";
                    byte[] buffer = Encoding.ASCII.GetBytes(data);

                    var reply = new Ping().Send(who, timeout, buffer, new PingOptions(64, true));
                    if (reply.Status != IPStatus.Success)
                    {   if (++ping_err_cnt < ping_err_limit)
                        {   Console.WriteLine($"ping err count : {ping_err_cnt}");
                        }
                        else
                        {   Logger.Inst.Write(CmdLogType.Comm, $"ping err limit 초과 : {ping_err_cnt}");
                            g_Connected = false;
                            OnChangeConnected?.Invoke(this, new ChangeConnectedArgs() { connected = false });
                        }
                    }
                    else
                    {   ping_err_cnt = 0;
                    }
                }
            }
            catch(Exception ex)
            {   Logger.Inst.Write(CmdLogType.Comm, $"Exception. tmrPing_Elapsed.\r\n{ex.Message}\r\n{ex.StackTrace}");
            }
        }
        int status_count = 0;
        private void tmrPing_Elapsed1(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (Connected_chk == true)
                {
                    status_count++;

                    if (status_count == 30000)
                    {
                        g_Connected = false;
                        OnChangeConnected?.Invoke(this, new ChangeConnectedArgs() { connected = false });
                    }
                }
                else
                {
                    status_count = 0;
                }
                    
            }
            catch (Exception ex)
            {
                Logger.Inst.Write(CmdLogType.Comm, $"Exception. tmrPing_Elapsed.\r\n{ex.Message}\r\n{ex.StackTrace}");
            }
        }
    }
}




