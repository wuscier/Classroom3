using Common.Helper;

namespace Common.Contract
{
    public interface INetCheckService
    {
        bool PingServer(string host, string serverIp);
        NetStatus GetNetStatus();
        void StartCheckNetConnect();
    }
}
