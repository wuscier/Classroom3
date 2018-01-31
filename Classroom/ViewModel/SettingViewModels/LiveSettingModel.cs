using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Classroom.View;
using Common.Contract;
using Common.Helper;
using Common.Model;
using Common.UiMessage;
using Prism.Commands;
using Prism.Mvvm;
using Serilog;
using ConfigManager = Common.Model.ConfigManager;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Classroom.ViewModel
{
    public class LiveSettingModel : BindableBase
    {

        #region field

        private string _selectedLiveDisplay;
        private string _selectedRemoteDisplay;
        private int _selectedLiveRate;
        private int _selectedRemoteRate;
        private string _selectedLocalResolution;
        private int _selectedLocalBitrate;
        private string _selectedLocalPath;

        private ConfigManager _configManager;
        private readonly ILocalDataManager _localDataManager;
        private readonly LiveSettingView _view;

        #endregion

        #region property

        public ObservableCollection<string> LiveDisplaySource { get; set; }
        public ObservableCollection<int> LiveRateSource { get; set; }

        public string SelectedLiveDisplay
        {
            get { return _selectedLiveDisplay; }
            set { SetProperty(ref _selectedLiveDisplay, value); }
        }

        public string SelectedRemoteDisplay
        {
            get { return _selectedRemoteDisplay; }
            set { SetProperty(ref _selectedRemoteDisplay, value); }
        }

        public int SelectedLiveRate
        {
            get { return _selectedLiveRate; }
            set { SetProperty(ref _selectedLiveRate, value); }
        }

        public int SelectedRemoteRate
        {
            get { return _selectedRemoteRate; }
            set { SetProperty(ref _selectedRemoteRate, value); }
        }

        public string SelectedLocalResolution
        {
            get { return _selectedLocalResolution; }
            set { SetProperty(ref _selectedLocalResolution, value); }
        }

        public int SelectedLocalBitrate
        {
            get { return _selectedLocalBitrate; }
            set { SetProperty(ref _selectedLocalBitrate, value); }
        }

        public string SelectedLocalPath
        {
            get { return _selectedLocalPath; }
            set { SetProperty(ref _selectedLocalPath, value); }
        }


        #endregion

        #region ctor

        public LiveSettingModel(LiveSettingView view)
        {
            _view = view;
            _localDataManager = DependencyResolver.Current.GetService<ILocalDataManager>();
            LiveDisplaySource = new ObservableCollection<string>();
            LiveRateSource = new ObservableCollection<int>();
            LoadCommand = DelegateCommand.FromAsyncHandler(Loading);
            WindowKeyDownCommand = new DelegateCommand<object>(WindowKeyDownHandler);
            GoBackCommand = new DelegateCommand(GoBack);
            SelectRecordPathCommand = new DelegateCommand(SelectRecordPath);

        }

        #endregion

        #region method

        private void SelectRecordPath()
        {
            var fbd = new FolderBrowserDialog { SelectedPath = Environment.CurrentDirectory };
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                SelectedLocalPath = fbd.SelectedPath;
            }
        }

        private void GoBack()
        {
            SaveSetting();
            var nav = new SettingNavView();
            nav.Show();
            _view.Close();
        }

        private async Task Loading()
        {
            try
            {
                //数据源
                await GetDataSource();
                //设置默认选项
                await SetDefaultSetting();
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"直播设置加载信息发生异常 exception：{ex}");
                MessageQueueManager.Instance.AddError(MessageManager.LoadingError);
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
                if (_configManager == null)
                {
                    _configManager = new ConfigManager
                    {
                        LocalLiveStreamInfo = new LiveStreamInfo(),
                        RemoteLiveStreamInfo = new LiveStreamInfo(),
                        RecordInfo = new RecordInfo()
                    };

                }

                _configManager.LocalLiveStreamInfo.LiveStreamBitRate = SelectedLiveRate;
                _configManager.LocalLiveStreamInfo.LiveStreamDisplayHeight = int.Parse(SelectedLiveDisplay.Split('*')[1]);
                _configManager.LocalLiveStreamInfo.LiveStreamDisplayWidth = int.Parse(SelectedLiveDisplay.Split('*')[0]);

                _configManager.RemoteLiveStreamInfo.LiveStreamBitRate = SelectedRemoteRate;
                _configManager.RemoteLiveStreamInfo.LiveStreamDisplayHeight =
                    int.Parse(SelectedRemoteDisplay.Split('*')[1]);
                _configManager.RemoteLiveStreamInfo.LiveStreamDisplayWidth = int.Parse(SelectedRemoteDisplay.Split('*')[0]);


                _configManager.RecordInfo.RecordBitRate = SelectedLocalBitrate;
                _configManager.RecordInfo.RecordDirectory = SelectedLocalPath;
                _configManager.RecordInfo.RecordDisplayWidth = int.Parse(SelectedLocalResolution.Split('*')[0]);
                _configManager.RecordInfo.RecordDisplayHeight = int.Parse(SelectedLocalResolution.Split('*')[1]);

                _localDataManager.SaveSettingConfigData(_configManager);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"直播设置SaveSetting() exception：{ex}");
                MessageQueueManager.Instance.AddError(MessageManager.SaveError);
            }
        }


        private async Task SetDefaultSetting()
        {
            await _view.Dispatcher.BeginInvoke(new Action(() =>
            {
                //本地保存的配置
                _configManager = _localDataManager.GetSettingConfigData() ?? new ConfigManager() { ServerInfo = new ServerInfo() { BmsServerPort = GlobalData.Instance.LocalSetting.BmsServerPort, ServerIp = GlobalData.Instance.LocalSetting.ServerIp } };
                if (_configManager.LocalLiveStreamInfo == null) _configManager.LocalLiveStreamInfo = new LiveStreamInfo();
                if (_configManager.RecordInfo == null) _configManager.RecordInfo = new RecordInfo();
                if (_configManager.RemoteLiveStreamInfo == null) _configManager.RemoteLiveStreamInfo = new LiveStreamInfo();

                SelectedLiveDisplay =
                    $"{_configManager.LocalLiveStreamInfo.LiveStreamDisplayWidth}*{_configManager.LocalLiveStreamInfo.LiveStreamDisplayHeight}";
                SelectedLiveRate = _configManager.LocalLiveStreamInfo.LiveStreamBitRate;
                SelectedRemoteDisplay =
                    $"{_configManager.RemoteLiveStreamInfo.LiveStreamDisplayWidth}*{_configManager.RemoteLiveStreamInfo.LiveStreamDisplayHeight}";
                SelectedRemoteRate = _configManager.RemoteLiveStreamInfo.LiveStreamBitRate;
                SelectedLocalResolution =
                    $"{_configManager.RecordInfo.RecordDisplayWidth}*{_configManager.RecordInfo.RecordDisplayHeight}";
                SelectedLocalBitrate = _configManager.RecordInfo.RecordBitRate;
                SelectedLocalPath = _configManager.RecordInfo.RecordDirectory;
            }));
        }


        private async Task GetDataSource()
        {
            await _view.Dispatcher.BeginInvoke(new Action(() =>
            {
                //配置数据源
                var settingLocalData = _localDataManager.GetSettingParameter();
                settingLocalData.LiveParameterVGAs.ForEach(v => { LiveDisplaySource.Add(v.LiveDisplayWidth); });
                settingLocalData.LiveParameterRates.ForEach(r => { LiveRateSource.Add(r.LiveBitRate); });
            }));
        }

        #endregion

        #region command

        public ICommand LoadCommand { get; set; }

        public ICommand WindowKeyDownCommand { get; set; }
        public ICommand GoBackCommand { get; set; }

        public ICommand SelectRecordPathCommand { get; set; }

        #endregion


    }
}
