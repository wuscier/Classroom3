using System;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Common.Contract;
using Common.Helper;
using Common.UiMessage;
using Serilog;

namespace Service
{
    /// <summary>
    /// 网络状态检测服务
    /// </summary>
    public class NetCheckService : INetCheckService
    {

        #region field

        private static NetCheckService _instance = null;

        private Thread _netThread = null;

        private NetStatus NetStatus = NetStatus.Normal;

        private int disconnectCount = 0;

        private bool _isRestart = false;

        private static object _localObject = new object();
        private readonly ILocalDataManager _localDataManager;

        #endregion

        #region ctor

        public NetCheckService()
        {
            _localDataManager = DependencyResolver.Current.GetService<ILocalDataManager>();
        }

        #endregion



        #region method

        public void StartCheckNetConnect()
        {
            try
            {
                if (_netThread == null)
                {
                    Task.Factory.StartNew(DoNetCheckProcess);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"【网络启动异常 exception】：{ex}");
            }
        }

        public bool PingServer(string host, string serverIp)
        {
            try
            {
                if (string.IsNullOrEmpty(host))
                {
                    host = serverIp;
                }
                //远程服务器IP         
                //构造Ping实例  
                var pingSender = new Ping();
                ////Ping 选项设置  
                var options = new PingOptions();
                //options.DontFragment = true;
                ////测试数据  
                //string data = "1";
                var buffer = new byte[32];
                //设置超时时间  
                var timeout = 1000;
                //调用同步 send 方法发送数据,将返回结果保存至PingReply实例  
                var reply = pingSender.Send(host, timeout, buffer, options);
                if (reply != null && reply.Status == IPStatus.Success)
                {
                    // Logger.WriteDebugingFmt(Log.NetCheckProcessor, "ping接入服务器正常");
                    return true;
                }
                disconnectCount++;
                // Logger.WriteDebugingFmt(Log.NetCheckProcessor, "ping接入服务器失败");
                return false;
            }
            catch (Exception ex)
            {
                // Logger.WriteErrorFmt(Log.IndexLog, ex, "检查服务器连接异常:{0}", ex.Message);
                return false;
            }
        }


        private void DoNetCheckProcess()
        {
            while (true)
            {
                try
                {

                    if (IsConnectedToInternet())
                    {
                        var serverAddress =
                            $"{GlobalData.Instance.ConfigManager.ServerInfo.ServerIp}";
                        NetStatus = PingServer(string.Empty, serverAddress) ? NetStatus.Normal : NetStatus.ConnectFail;
                    }
                    else
                    {
                        NetStatus = NetStatus.ConnectFail;
                    }

                }
                catch (Exception ex)
                {
                    NetStatus = NetStatus.Disconnect;
                    Log.Logger.Error($"【欢迎页检测网络状态 exception】：{ex}");
                    MessageQueueManager.Instance.AddError("网络异常");
                }
                Thread.Sleep(10000);
            }
        }


        public NetStatus GetNetStatus()
        {
            return NetStatus;
        }

        //检测网卡是否正常
        private bool IsConnectedToInternet()
        {
            return InternetGetConnectedState(0, 0);
        }

        private bool CheckConnectToSever()
        {
            //获取服务器ip、端口
            var configData = _localDataManager.GetSettingConfigData();
            return configData?.ServerInfo != null && PingServer(string.Empty, $"{configData.ServerInfo.ServerIp}:{configData.ServerInfo.BmsServerPort}");
        }

        [DllImport("wininet.dll")]
        private static extern bool InternetGetConnectedState(int description, int reservedValue);

        #endregion
    }
}
