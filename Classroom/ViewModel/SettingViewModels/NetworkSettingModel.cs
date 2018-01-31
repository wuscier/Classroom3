using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Input;
using WindowsInput;
using Classroom.View;
using Common.Contract;
using Common.Helper;
using Common.Model;
using Prism.Mvvm;
using ConfigManager = Common.Model.ConfigManager;
using Prism.Commands;
using Serilog;
using System.Management;
using Common.UiMessage;

namespace Classroom.ViewModel
{
    public class NetworkSettingModel : BindableBase
    {
        #region field

        private int _ip1;
        private int _ip2;
        private int _ip3;
        private int _ip4;

        private int _mask1;
        private int _mask2;
        private int _mask3;
        private int _mask4;

        private int _gateWay1;
        private int _gateWay2;
        private int _gateWay3;
        private int _gateWay4;

        private int _dns1;
        private int _dns2;
        private int _dns3;
        private int _dns4;

        private string _selectNetWorkCard;
        private bool _autoGet;
        private bool _manualGet;

        private ConfigManager _configManager;
        private readonly ILocalDataManager _localDataManager;
        private readonly InputSimulator _s;
        private readonly NetworkSettingView _view;
        private List<NetInfo> _netInfos;
        private readonly INetCheckService _netCheckService;


        #endregion

        #region property

        public ObservableCollection<string> NetWorkCardSource { get; set; }



        public bool ManualGet
        {
            get { return _manualGet; }
            set { SetProperty(ref _manualGet, value); }
        }

        public bool AutoGet
        {
            get { return _autoGet; }
            set { SetProperty(ref _autoGet, value); }
        }

        public string SelectNetWorkCard
        {
            get { return _selectNetWorkCard; }
            set { SetProperty(ref _selectNetWorkCard, value); }
        }

        public int Ip1
        {
            get { return _ip1; }
            set { SetProperty(ref _ip1, value); }
        }

        public int Ip2
        {
            get { return _ip2; }
            set { SetProperty(ref _ip2, value); }
        }

        public int Ip3
        {
            get { return _ip3; }
            set { SetProperty(ref _ip3, value); }
        }

        public int Ip4
        {
            get { return _ip4; }
            set { SetProperty(ref _ip4, value); }
        }

        public int Mask1
        {
            get { return _mask1; }
            set { SetProperty(ref _mask1, value); }
        }

        public int Mask2
        {
            get { return _mask2; }
            set { SetProperty(ref _mask2, value); }
        }

        public int Mask3
        {
            get { return _mask3; }
            set { SetProperty(ref _mask3, value); }
        }

        public int Mask4
        {
            get { return _mask4; }
            set { SetProperty(ref _mask4, value); }
        }

        public int GateWay1
        {
            get { return _gateWay1; }
            set { SetProperty(ref _gateWay1, value); }
        }

        public int GateWay2
        {
            get { return _gateWay2; }
            set { SetProperty(ref _gateWay2, value); }
        }

        public int GateWay3
        {
            get { return _gateWay3; }
            set { SetProperty(ref _gateWay3, value); }
        }

        public int GateWay4
        {
            get { return _gateWay4; }
            set { SetProperty(ref _gateWay4, value); }
        }

        public int Dns1
        {
            get { return _dns1; }
            set { SetProperty(ref _dns1, value); }
        }

        public int Dns2
        {
            get { return _dns2; }
            set { SetProperty(ref _dns2, value); }
        }

        public int Dns3
        {
            get { return _dns3; }
            set { SetProperty(ref _dns3, value); }
        }

        public int Dns4
        {
            get { return _dns4; }
            set { SetProperty(ref _dns4, value); }
        }


        #endregion

        #region ctor

        public NetworkSettingModel(NetworkSettingView view)
        {
            _s = new InputSimulator();
            _view = view;
            _netInfos = new List<NetInfo>();
            _localDataManager = DependencyResolver.Current.GetService<ILocalDataManager>();
            _netCheckService = DependencyResolver.Current.GetService<INetCheckService>();
            NetWorkCardSource = new ObservableCollection<string>();
            LoadCommand = DelegateCommand.FromAsyncHandler(Loading);
            WindowKeyDownCommand = new DelegateCommand<object>(WindowKeyDownHandler);
            AutoGetDnsCommand = DelegateCommand.FromAsyncHandler(AutoGetDns);
            ManualGetDnsCommand = DelegateCommand.FromAsyncHandler(ManualGetDns);
            NetCardChangeCommand = DelegateCommand.FromAsyncHandler(ResetAsync);
            CheckNetCommand = DelegateCommand.FromAsyncHandler(CheckNetAsync);
            GoBackCommand = new DelegateCommand(GoBack);
        }

        #endregion

        #region method

        private async Task Loading()
        {
            try
            {
                _configManager = _localDataManager.GetSettingConfigData() ?? new ConfigManager() { ServerInfo = new ServerInfo() { BmsServerPort = GlobalData.Instance.LocalSetting.BmsServerPort, ServerIp = GlobalData.Instance.LocalSetting.ServerIp } };
                if (_configManager?.NetInfo == null)
                {
                    _configManager.NetInfo = new NetInfo();
                }
                if (_configManager?.ServerInfo == null)
                {
                    _configManager.ServerInfo = new ServerInfo();
                }
                //实时网络信息
                await GetNetInfos();
                await BindDataSource();

                var defaultNefInfo = _netInfos.FirstOrDefault(o => o.AdapterName == _configManager.NetInfo?.AdapterName);
                if (defaultNefInfo != null)
                {
                    await SetDefaultSetting(_configManager.NetInfo);
                }
                else
                {
                    await SetDefaultSetting(_netInfos.FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"网络设置加载信息发生异常 exception：{ex}");
                MessageQueueManager.Instance.AddError(MessageManager.LoadingError);
            }


        }

        private async Task ResetAsync()
        {
            await _view.Dispatcher.BeginInvoke(new Action(() =>
            {
                var netInfo = _netInfos.ToList().FirstOrDefault(o => o.AdapterName == SelectNetWorkCard);
                if (netInfo == null) return;
                SelectNetWorkCard = netInfo.AdapterName;
                BindIpAddress(netInfo);
                BindMask(netInfo);
                BindGateWay(netInfo);
                BindDns(netInfo);
            }));
        }

        private async Task GetNetInfos()
        {
            await _view.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    _netInfos.Clear();
                    var nics = NetworkInterface.GetAllNetworkInterfaces().ToList();
                    if (!nics.Any()) return;
                    foreach (var adapter in nics)
                    {
                        try
                        {
                            var pd1 = (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet); //判断是否是以太网连接
                            //Log.Logger.Debug($"网卡信息：{adapter.Name}----{adapter.NetworkInterfaceType}");
                            if (!pd1) continue;
                            var netinfo = new NetInfo();
                            var ip = adapter.GetIPProperties(); //IP配置信息
                            if (ip.UnicastAddresses.Count > 0)
                            {
                                var unicastIpAddressInformationCollection = ip.UnicastAddresses;
                                foreach (var unicastIpAddressInformation in unicastIpAddressInformationCollection)
                                {
                                    if (unicastIpAddressInformation.Address.AddressFamily != AddressFamily.InterNetwork)
                                        continue;
                                    netinfo.AdapterName = adapter.Description;
                                    netinfo.IpAddress = unicastIpAddressInformation.Address.ToString();
                                    netinfo.Mark = unicastIpAddressInformation.IPv4Mask.ToString();
                                    break;
                                }
                            }
                            if (ip.GatewayAddresses.Count > 0)
                            {
                                netinfo.GatWay = ip.GatewayAddresses[0].Address.ToString();
                            }
                            var dnsCount = ip.DnsAddresses.Count;
                            if (dnsCount > 0)
                            {
                                netinfo.Dns = ip.DnsAddresses[0].ToString();
                            }
                            _netInfos.Add(netinfo);
                        }
                        catch (Exception ex)
                        {
                            Log.Logger.Error($"获得网卡信息发生异常 exception：{ex}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"获得网卡信息发生异常 exception：{ex}");
                }
            }));
        }

        private async Task SetDefaultSetting(NetInfo netInfo)
        {
            await _view.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (netInfo == null) return;
                SelectNetWorkCard = netInfo.AdapterName;
                BindIpAddress(netInfo);
                BindMask(netInfo);
                BindGateWay(netInfo);
                BindDns(netInfo);
                if (!netInfo.IsAutoSet)
                    ManualGet = true;
                else
                    AutoGet = true;
            }));
        }

        private async Task BindDataSource()
        {
            await _view.Dispatcher.BeginInvoke(new Action(() =>
            {
                //判断本地配置中是否存在该网卡信息，如果存在加载
                _netInfos = _netInfos.Distinct().ToList();
                NetWorkCardSource.Clear();
                _netInfos.ForEach(n => { NetWorkCardSource.Add(n.AdapterName); });
            }));
        }

        private async Task AutoGetDns()
        {
            try
            {
                await Task.Run(async () =>
                 {
                     if (_configManager.NetInfo.IsAutoSet) return;
                     AutoGet = true;
                     ManualGet = false;
                     var wmi = new ManagementClass("Win32_NetworkAdapterConfiguration");
                     var moc = wmi.GetInstances();
                     foreach (var o in moc)
                     {
                         var mo = (ManagementObject)o;
                         if (
                             mo.Properties.Cast<PropertyData>().Where(prop => prop.Name == "Description").All(prop => prop.Value.ToString() != _configManager.NetInfo.AdapterName)) continue;
                         mo.InvokeMethod("SetDNSServerSearchOrder", null);
                         mo.InvokeMethod("SetGateways", null, null);
                         //开启DHCP  
                         mo.InvokeMethod("EnableDHCP", null);
                         GetNetInfo();
                         var netInfo = _netInfos.FirstOrDefault();
                         if (netInfo != null)
                         {
                             netInfo.IsAutoSet = true;
                             await BindDataSource();
                             await SetDefaultSetting(netInfo);
                         }
                         _configManager.NetInfo.IsAutoSet = true;
                         return;
                     }
                 });
            }
            catch (Exception ex)
            {

                Log.Logger.Error($"自动获取网卡信息 exception：{ex}");
            }

        }

        private async Task ManualGetDns()
        {
            try
            {
                await _view.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (_configManager.NetInfo != null && _configManager.NetInfo.IsAutoSet) return;
                }));
            }
            catch (Exception ex)
            {

                Log.Logger.Error($"自动获取网卡信息 exception：{ex}");
            }
        }

        private void GetNetInfo()
        {
            try
            {
                _netInfos.Clear();
                var nics = NetworkInterface.GetAllNetworkInterfaces();
                if (nics.Length <= 0)
                    return;
                foreach (var adapter in nics)
                {
                    try
                    {
                        var pd1 = (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet || adapter.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet); //判断是否是以太网连接
                        Log.Logger.Error($"网卡{adapter.Name}{adapter.NetworkInterfaceType}");
                        if (!pd1) continue;
                        var netinfo = new NetInfo();
                        var ip = adapter.GetIPProperties();     //IP配置信息
                        if (ip.UnicastAddresses.Count > 0)
                        {
                            var unicastIpAddressInformationCollection = ip.UnicastAddresses;
                            foreach (var unicastIpAddressInformation in unicastIpAddressInformationCollection)
                            {
                                if (unicastIpAddressInformation.Address.AddressFamily != AddressFamily.InterNetwork &&
                                    unicastIpAddressInformation.Address.AddressFamily != AddressFamily.InterNetworkV6)
                                    continue;
                                netinfo.AdapterName = adapter.Description;
                                netinfo.IpAddress = unicastIpAddressInformation.Address.ToString();
                                netinfo.Mark = unicastIpAddressInformation.IPv4Mask.ToString();
                                break;
                            }
                        }
                        if (ip.GatewayAddresses.Count > 0)
                        {
                            netinfo.GatWay = ip.GatewayAddresses[0].Address.ToString();
                        }
                        var dnsCount = ip.DnsAddresses.Count;
                        if (dnsCount > 0)
                        {
                            netinfo.Dns = ip.DnsAddresses[0].ToString();
                        }
                        _netInfos.Add(netinfo);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error($"获取网卡信息发生异常 exception：{ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"获取网卡信息发生异常 exception：{ex}");
            }
        }

        private void BindIpAddress(NetInfo netInfo)
        {
            try
            {
                if (string.IsNullOrEmpty(netInfo?.IpAddress)) return;
                Ip1 = int.Parse(netInfo.IpAddress.Split('.')[0]);
                Ip2 = int.Parse(netInfo.IpAddress.Split('.')[1]);
                Ip3 = int.Parse(netInfo.IpAddress.Split('.')[2]);
                Ip4 = int.Parse(netInfo.IpAddress.Split('.')[3]);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"网络设置Ip绑定 exception：{ex}");
            }

        }

        private void BindMask(NetInfo netInfo)
        {
            try
            {
                if (string.IsNullOrEmpty(netInfo?.Mark)) return;
                Mask1 = int.Parse(netInfo.Mark.Split('.')[0]);
                Mask2 = int.Parse(netInfo.Mark.Split('.')[1]);
                Mask3 = int.Parse(netInfo.Mark.Split('.')[2]);
                Mask4 = int.Parse(netInfo.Mark.Split('.')[3]);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"网络设置掩码绑定 exception：{ex}");
            }
        }

        private void BindGateWay(NetInfo netInfo)
        {
            try
            {
                if (string.IsNullOrEmpty(netInfo?.GatWay)) return;
                GateWay1 = int.Parse(netInfo.GatWay.Split('.')[0]);
                GateWay2 = int.Parse(netInfo.GatWay.Split('.')[1]);
                GateWay3 = int.Parse(netInfo.GatWay.Split('.')[2]);
                GateWay4 = int.Parse(netInfo.GatWay.Split('.')[3]);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"网络设置网关绑定 exception：{ex}");
            }
        }

        private void BindDns(NetInfo netInfo)
        {
            try
            {
                if (string.IsNullOrEmpty(netInfo?.Dns)) return;
                Dns1 = int.Parse(netInfo.Dns.Split('.')[0]);
                Dns2 = int.Parse(netInfo.Dns.Split('.')[1]);
                Dns3 = int.Parse(netInfo.Dns.Split('.')[2]);
                Dns4 = int.Parse(netInfo.Dns.Split('.')[3]);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"网络设置DNS绑定 exception：{ex}");
            }
        }

        private void WindowKeyDownHandler(object obj)
        {
            var keyEventArgs = obj as KeyEventArgs;
            switch (keyEventArgs?.Key)
            {
                case Key.Home:
                case Key.Escape:
                    //保存设置
                    SaveSetting();
                    break;
            }
        }

        private void SaveSetting()
        {
            try
            {
                var ip = $"{Ip1}.{Ip2}.{Ip3}.{Ip4}";
                var mask = $"{Mask1}.{Mask2}.{Mask3}.{Mask4}";
                var gateWay = $"{GateWay1}.{GateWay2}.{GateWay3}.{GateWay4}";
                var dns = $"{Dns1}.{Dns2}.{Dns3}.{Dns4}";
                _configManager.NetInfo.IpAddress = ip;
                _configManager.NetInfo.GatWay = gateWay;
                _configManager.NetInfo.AdapterName = SelectNetWorkCard;
                _configManager.NetInfo.Dns = dns;
                _configManager.NetInfo.Mark = mask;
                _configManager.NetInfo.IsAutoSet = AutoGet;
                _localDataManager.SaveSettingConfigData(_configManager);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"网络设置保存 exception：{ex}");
                MessageQueueManager.Instance.AddError(MessageManager.SaveError);
            }

        }

        private async Task CheckNetAsync()
        {
            await _view.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    SaveSetting();
                    string gwAddress = $"{GateWay1}.{GateWay2}.{GateWay3}.{GateWay4}";
                    var isOk = _netCheckService.PingServer(gwAddress, _configManager.ServerInfo.ServerIp);
                    MessageQueueManager.Instance.AddInfo(isOk ? MessageManager.CheckDns : MessageManager.CheckDnsError);
                }
                catch (Exception ex)
                {
                    MessageQueueManager.Instance.AddError(MessageManager.CheckDnsError);
                }
            }));
        }

        private void GoBack()
        {
            SaveSetting();
            var nav = new SettingNavView();
            nav.Show();
            _view.Close();
        }

        #endregion

        #region command

        public ICommand LoadCommand { get; set; }

        public ICommand WindowKeyDownCommand { get; set; }

        public ICommand AutoGetDnsCommand { get; set; }

        public ICommand ManualGetDnsCommand { get; set; }

        public ICommand NetCardChangeCommand { get; set; }

        public ICommand CheckNetCommand { get; set; }

        public ICommand GoBackCommand { get; set; }

        #endregion
    }
}
