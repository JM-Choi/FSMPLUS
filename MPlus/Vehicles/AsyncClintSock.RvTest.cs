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
    public partial class AsyncClintSock
    {
        public event RecvStrEvent OnRvRcvMsg;
        public event RecvDataEvent OnRvRcvData;
        public event EventHandler<ChangeConnectedArgs> OnRvChangeConnected;
        
        private bool g_rvConnected;
        public bool rvConnected { get { return g_rvConnected; } }

        private Socket m_RvClientSocket = null;

        private AsyncCallback m_RvfnReceiveHandler;
        private AsyncCallback m_RvfnSendHandler;

        private bool RvBind(string bindIp = "", ushort bindPort = 0)
        {
            try
            {
                // TCP 통신을 위한 소켓을 생성합니다.
                m_RvClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

                IPAddress myIP = IPAddress.Parse(bindIp);
                IPEndPoint ipLocal = new IPEndPoint(myIP, bindPort);
                if (ipLocal.Port != 0)
                {
                    m_RvClientSocket.Bind(ipLocal);
                    Logger.Inst.Write(CmdLogType.Rv, $"Bind success. localip:{bindIp}, localport:{bindPort}");
                }
            }
            catch (Exception ex)
            {
                Logger.Inst.Write(CmdLogType.Rv, $"Exception. Bind.\r\n{ex.Message}\r\n{ex.StackTrace}");
                return false;
            }
            return true;
        }
        public void RvConnectToServer(string hostName, ushort hostPort, string remoteIp, ushort remotePort)
        {
            bool isConnected = false;
            try
            {
                if (RvBind(hostName, hostPort))
                {
                    // 연결 시도
                    bool success = m_RvClientSocket.BeginConnect(remoteIp, Convert.ToInt32(remotePort), null, null).AsyncWaitHandle.WaitOne(1000, true);
                    m_RvClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);
                    m_RvClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);

                    Logger.Inst.Write(CmdLogType.Rv, $"connecting...remoteip:{remoteIp},remoteport:{remotePort}");

                    if (success)
                    {
                        // 연결 성공
                        isConnected = true;
                        Console.WriteLine($"{hostName} : 연결 성공1");
                    }
                }
                else
                {
                    Logger.Inst.Write(CmdLogType.Rv, "local socket bind fail");
                }
            }
            catch
            {
                // 연결 실패 (연결 도중 오류가 발생함)
                isConnected = false;
                Console.WriteLine($"{hostName} : 연결 실패1");
            }
            g_rvConnected = isConnected;

            if (isConnected)
            {
                try
                {
                    // 4096 바이트의 크기를 갖는 바이트 배열을 가진 AsyncObject 클래스 생성
                    AsyncObject ao = new AsyncObject(4096)
                    {
                        // 작업 중인 소켓을 저장하기 위해 sockClient 할당
                        rvWorkingSocket = m_RvClientSocket
                    };

                    // 비동기적으로 들어오는 자료를 수신하기 위해 BeginReceive 메서드 사용!
                    m_RvClientSocket.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_RvfnReceiveHandler, ao);

                    //Logger.Inst.Write(CmdLogType.prdt, $"{hostName} : 연결 성공");
                    Console.WriteLine($"{hostName} : 연결 성공2");
                    OnChangeConnected?.Invoke(this, new ChangeConnectedArgs() { connected = true });
                }
                catch (Exception ex)
                {
                    Logger.Inst.Write(CmdLogType.Rv, $"{ex.Message}\r\n{ex.StackTrace}");
                    Console.WriteLine($"{hostName} : 연결 중 오류발생");
                    OnChangeConnected?.Invoke(this, new ChangeConnectedArgs() { connected = false });
                    rvStopClient();
                }
            }
            else
            {
                //Debug.WriteLine("연결 실패!");
            }
        }

        public void RvSendMessage(string message)
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
                rvWorkingSocket = m_RvClientSocket
            };

            // 전송 시작!
            try
            {
                m_RvClientSocket.BeginSend(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_RvfnSendHandler, ao);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"전송 중 오류 발생!\n메세지: {ex.Message}");
                rvStopClient();
                g_rvConnected = false;
                OnRvChangeConnected?.Invoke(this, new ChangeConnectedArgs() { connected = false });
            }
        }

        private void RvhandleDataSend(IAsyncResult ar)
        {
            // 넘겨진 추가 정보를 가져옵니다.
            AsyncObject ao = (AsyncObject)ar.AsyncState;

            // 보낸 바이트 수를 저장할 변수 선언
            int sentBytes;
            try
            {
                // 자료를 전송하고, 전송한 바이트를 가져옵니다.
                sentBytes = ao.rvWorkingSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                // 예외가 발생하면 예외 정보 출력 후 함수를 종료한다.
                Logger.Inst.Write(CmdLogType.Comm, $"Exception. handleDataSend.자료 송신 도중 오류 발생!\r\n{ex.Message}\r\n{ex.StackTrace}");
                g_rvConnected = false;
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
        private void RvhandleDataReceive(IAsyncResult ar)
        {
            // 넘겨진 추가 정보를 가져옵니다.
            // AsyncState 속성의 자료형은 Object 형식이기 때문에 형 변환이 필요합니다~!
            AsyncObject ao = (AsyncObject)ar.AsyncState;

            // 받은 바이트 수 저장할 변수 선언
            Int32 recvBytes;
            try
            {
                // 자료를 수신하고, 수신받은 바이트를 가져옵니다.
                recvBytes = ao.rvWorkingSocket.EndReceive(ar);
            }
            catch (Exception ex)
            {
                // 예외가 발생하면 함수 종료!
                Logger.Inst.Write(CmdLogType.Comm, $"Exception. handleDataReceive.자료 수신 도중 오류 발생!\r\n{ex.Message}\r\n{ex.StackTrace}");
                rvStopClient();
                g_rvConnected = false;
                return;
            }

            // 수신받은 자료의 크기가 1 이상일 때에만 자료 처리
            if (recvBytes > 0)
            {
                // 공백 문자들이 많이 발생할 수 있으므로, 받은 바이트 수 만큼 배열을 선언하고 복사한다.
                byte[] msgByte = new byte[recvBytes];
                Array.Copy(ao.Buffer, msgByte, recvBytes);

                OnRvRcvData?.Invoke(this, msgByte);
                OnRvRcvMsg?.Invoke(this, Encoding.ASCII.GetString(msgByte));

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
                ao.rvWorkingSocket.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_RvfnReceiveHandler, ao);
            }
            catch (Exception ex)
            {
                // 예외가 발생하면 예외 정보 출력 후 함수를 종료한다.
                Logger.Inst.Write(CmdLogType.Comm, $"Exception. handleDataReceive.자료 송신 도중 오류 발생!\r\n{ex.Message}\r\n{ex.StackTrace}");
                rvStopClient();
                g_rvConnected = false;
                return;
            }
        }

        public void rvStopClient()
        {
            // 가차없이 클라이언트 소켓을 닫습니다.
            if (m_RvClientSocket != null)
            {
                tmrPing.Enabled = false;
                m_RvClientSocket.Close();
                g_rvConnected = false;
                m_RvClientSocket.Dispose();
                m_RvClientSocket = null;
                Console.WriteLine($"클라이언트소켓 삭제");
            }
        }
    }
}
