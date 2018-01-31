using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Service
{
    public class RecordingSystemService
    {
        private static RecordingSystemService _instance = null;
        private Thread _currirThread = null;
        private ControlComand _controlCommand;
        private int port = 0;
        private Socket _udpSocket = null;
        private UdpClient _udpClient = null;
        private static IPEndPoint _point = new IPEndPoint(IPAddress.Any, 0);

        public static RecordingSystemService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RecordingSystemService();
                }
                return _instance;
            }
        }

        #region 服务端
        public void Start()
        {
            try
            {
                try
                {
                    IPEndPoint point = new IPEndPoint(IPAddress.Any, 0);
                    point.Port = MeetingControlSetting.Instance.ReceiveRecordSystemCmdPort;
                    //Ip和Port绑定到Socket
                    uint SIO_UDP_CONNRESET = 0x80000000 | 0x18000000 | 12;
                    Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    udpSocket.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
                    udpSocket.Bind(point);

                    Thread udpMonitor = new Thread(ReceivePerformance);
                    udpMonitor.Start(udpSocket);
                    // Logger.WriteInfo(Log.ControlRecordSystem, "同步教室应答监听启动成功");

                }
                catch (Exception ex)
                {
                    // Logger.WriteErrorFmt(Log.ControlRecordSystem, ex, "同步教室应答监听启动失败{0}", ex.Message);
                }
            }
            catch (Exception ex)
            {
                // Logger.WriteErrorFmt(Log.ControlRecordSystem, ex, "同步教室应答监听启动失败{0}", ex.Message);
            }
        }

        private void ReceivePerformance(object data)
        {
            Socket udpSocket = (Socket)data;
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, 0);
            EndPoint ep = (EndPoint)iep;
            byte[] buffer = new byte[1024];
            while (true)
            {
                try
                {
                    int recv = udpSocket.ReceiveFrom(buffer, ref ep);
                    // Logger.WriteInfoFmt(Log.ControlRecordSystem, "接收到{0}发送的应答数据", ep);
                    //数据解析
                    string message = Encoding.UTF8.GetString(buffer, 0, recv);
                    if (message.Contains('|'))
                    {
                        string[] ch = message.Trim().Split('|');
                        if (ch.Length >= 3 && ch[0] == "redcdn")
                        {
                            int cmd = (int)_controlCommand;
                            if (ch[1] == cmd.ToString())
                            {
                                if (ch[2] == "0")
                                {
                                    // Logger.WriteInfoFmt(Log.ControlRecordSystem, "指令：{0}执行成功", message);
                                }
                                else
                                {
                                    //执行失败重发
                                    //Logger.WriteInfoFmt(Log.ControlRecordSystem, "指令：{0}执行失败", message);
                                    Thread.Sleep(2000);
                                    SetControlComand(_controlCommand);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //  Logger.WriteErrorFmt(Log.ControlRecordSystem, ex, "接收{0}应答数据异常{1}", ep, ex.Message);
                }
            }
        }

        public void Stop()
        {
            try
            {
                if (_currirThread == null) return;
                _currirThread.Abort();
                _currirThread = null;
            }
            catch (Exception ex)
            {
                // Logger.WriteErrorFmt(Log.NetCheckProcessor, ex, "课表管理停止异常：{0}", ex.Message);
            }
        }
        #endregion

        #region 客户端
        public void SetControlComand(ControlComand cmd)
        {
            lock (this)
            {
                _controlCommand = cmd;
                string message = string.Format("redcdn|{0}", (int)cmd);
                Send(message);
            }
        }

        #region 异步方式传输
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="hitInfo"></param>
        private void Send(string Message)
        {
            IPEndPoint iep = new IPEndPoint(IPAddress.Parse(MeetingControlSetting.Instance.RecordSystemIpAddress), MeetingControlSetting.Instance.RecordSystemPort);
            try
            {
                if (_udpClient == null)
                {
                    _udpClient = new UdpClient();
                }
                byte[] sendData = Encoding.Default.GetBytes(Message);
                _udpClient.BeginSend(sendData, sendData.Length, iep, new AsyncCallback(SendCallback), _udpClient);
                // Logger.WriteDebugingFmt(Log.ControlRecordSystem, "向录课系统" + iep + "发送控制指令" + Message + "成功");
            }
            catch (Exception ex)
            {
                // Logger.WriteErrorFmt(Log.ControlRecordSystem, ex, "向录课系统" + iep + "发送控制指令" + Message + "异常");
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            UdpClient u = (UdpClient)ar.AsyncState;
        }
        #endregion
        #endregion
    }

    public enum ControlComand
    {
        DefulteWorkMode = 0,
        MainClassRoomMode = 1,
        ListenClassRoomMode = 2,
        ResetWorkMode = 3
    }
}
