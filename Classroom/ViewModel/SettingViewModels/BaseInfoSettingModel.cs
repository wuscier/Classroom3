using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using WindowsInput;
using WindowsInput.Native;
using Classroom.View;
using Common.Contract;
using Common.Helper;
using Common.Model;
using Common.UiMessage;
using Prism.Commands;
using Prism.Mvvm;
using Serilog;
using ConfigManager = Common.Model.ConfigManager;

namespace Classroom.ViewModel
{
    public class BaseInfoSettingModel : BindableBase
    {
        #region field

        private ConfigManager _configManager;
        private readonly ILocalDataManager _localDataManager;
        private readonly BaseInfoSettingView _view;
        private readonly InputSimulator _s;
        private readonly IBms _bms;

        private int _serverIp1;
        private int _serverIp2;
        private int _serverIp3;
        private int _serverIp4;

        private int _serverPort;

        private ImageBrush _stackPanelFocusBackgroud;

        #endregion

        #region property

        public ImageBrush StackPanelFocusBackgroud
        {
            get { return _stackPanelFocusBackgroud; }
            set { SetProperty(ref _stackPanelFocusBackgroud, value); }
        }

        public int ServerIp1
        {
            get { return _serverIp1; }
            set { SetProperty(ref _serverIp1, value); }
        }

        public int ServerIp2
        {
            get { return _serverIp2; }
            set { SetProperty(ref _serverIp2, value); }
        }

        public int ServerIp3
        {
            get { return _serverIp3; }
            set { SetProperty(ref _serverIp3, value); }
        }

        public int ServerIp4
        {
            get { return _serverIp4; }
            set { SetProperty(ref _serverIp4, value); }
        }

        public int ServerPort
        {
            get { return _serverPort; }
            set { SetProperty(ref _serverPort, value); }
        }

        #endregion

        #region ctor

        public BaseInfoSettingModel(BaseInfoSettingView view)
        {
            _s = new InputSimulator();
            _view = view;
            _bms = DependencyResolver.Current.GetService<IBms>();
            _localDataManager = DependencyResolver.Current.GetService<ILocalDataManager>();
            LoadCommand = DelegateCommand.FromAsyncHandler(Loading);
            WindowKeyDownCommand = new DelegateCommand<object>(WindowKeyDownHandler);
            CheckConnectCommand = DelegateCommand.FromAsyncHandler(CheckConnectAsync);
            GoBackCommand = new DelegateCommand(GoBack);
        }

        #endregion

        #region method

        private void GoBack()
        {
            SaveSetting();
            var nav = new SettingNavView();
            nav.Show();
            _view.Close();
        }

        private async Task Loading()
        {
            await _view.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    //本地保存的配置
                    _configManager = _localDataManager.GetSettingConfigData() ??
                                     new ConfigManager { ServerInfo = new ServerInfo() {BmsServerPort = GlobalData.Instance.LocalSetting.BmsServerPort,ServerIp = GlobalData.Instance.LocalSetting.ServerIp } };
                    if (_configManager.ServerInfo == null)
                    {
                        _configManager.ServerInfo = new ServerInfo();
                    }
                    if (string.IsNullOrEmpty(_configManager.ServerInfo.ServerIp)) return;
                    ServerIp1 = int.Parse(_configManager.ServerInfo.ServerIp.Split('.')[0]);
                    ServerIp2 = int.Parse(_configManager.ServerInfo.ServerIp.Split('.')[1]);
                    ServerIp3 = int.Parse(_configManager.ServerInfo.ServerIp.Split('.')[2]);
                    ServerIp4 = int.Parse(_configManager.ServerInfo.ServerIp.Split('.')[3]);
                    ServerPort = _configManager.ServerInfo.BmsServerPort;
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"基础设置加载信息发生异常 exception：{ex}");
                    MessageQueueManager.Instance.AddError(MessageManager.LoadingError);
                }

            }));
        }

        private void WindowKeyDownHandler(object obj)
        {
            var keyEventArgs = obj as KeyEventArgs;
            switch (keyEventArgs?.Key)
            {
                case Key.Return:
                    break;
                case Key.Right:
                case Key.Down:
                case Key.PageDown:
                case Key.End:
                    _s.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    keyEventArgs.Handled = true;
                    break;

                case Key.Left:
                case Key.Up:
                case Key.PageUp:
                case Key.Home:
                    _s.Keyboard.ModifiedKeyStroke(VirtualKeyCode.SHIFT, VirtualKeyCode.TAB);
                    keyEventArgs.Handled = true;
                    break;

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
                var newIp = $"{ServerIp1}.{ServerIp2}.{ServerIp3}.{ServerIp4}";
                IPAddress ip;
                if (!IPAddress.TryParse(newIp, out ip) || ServerPort < 0 || ServerPort > 65535)
                {
                    //不合法
                }
                else
                {
                    if (_configManager == null)
                    {
                        _configManager = new ConfigManager { ServerInfo = new ServerInfo() };
                    }
                    _configManager.ServerInfo.BmsServerPort = ServerPort;
                    _configManager.ServerInfo.ServerIp = newIp;
                    _localDataManager.SaveSettingConfigData(_configManager);
                    //新IP地址与配置中保存的不一致们需要重启

                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"基础设置SaveSetting() exception：{ex}");
                MessageQueueManager.Instance.AddError(MessageManager.SaveError);

            }

        }

        private async Task CheckConnectAsync()
        {
            var baseUrl = $"http://{ServerIp1}.{ServerIp2}.{ServerIp3}.{ServerIp4}:{ServerPort}";
            var result = await _bms.CheckConnection(baseUrl);
            if (!result)
                MessageQueueManager.Instance.AddWarning(MessageManager.CheckServerError);
            else
                MessageQueueManager.Instance.AddInfo(MessageManager.CheckServer);
        }

        #endregion

        #region command

        public ICommand LoadCommand { get; set; }
        public ICommand WindowKeyDownCommand { get; set; }

        public ICommand CheckConnectCommand { get; set; }

        public ICommand GotFocusCommand { get; set; }

        public ICommand LostFocusCommand { get; set; }

        public ICommand GoBackCommand { get; set; }

        #endregion


    }
}
