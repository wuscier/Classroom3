using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using WindowsInput;
using Classroom.View;
using Common.Contract;
using Common.Helper;
using Common.Model;
using Common.UiMessage;
using Prism.Commands;
using Prism.Mvvm;
using Serilog;
using MeetingSdk.Wpf;
using MeetingSdk.NetAgent;

namespace Classroom.ViewModel
{
    public class AudioSettingModel : BindableBase
    {

        #region field

        private string _audioSource;
        private string _docAudioSource;
        private int _sampleRate;
        private int _aac;
        private string _audioOutPutDevice;

        //private readonly IMeeting _meetingService;
        private readonly IMeetingSdkAgent _meetingSdkAgent;

        private readonly ILocalDataManager _localDataManager;
        private readonly IDeviceNameAccessor _deviceNameAccessor;
        private readonly IDeviceConfigLoader _deviceConfigLoader;

        private readonly AudioSettingView _view;
        private ConfigManager _configManager;
        private readonly InputSimulator _s;

        private const int AudioSourceType = 3;
        private const int DocSourceType = 5;
        private const int AudioOutPutDeviceType = 4;

        #endregion

        #region property


        public ObservableCollection<string> AudioSource { get; set; }
        public ObservableCollection<string> DocAudioSource { get; set; }
        public ObservableCollection<string> AudioOutPutDevice { get; set; }
        public ObservableCollection<int> SampleRate { get; set; }
        public ObservableCollection<int> Aac { get; set; }


        public string SelectedAudioSource
        {
            get { return _audioSource; }
            set
            {
                if (SetProperty(ref _audioSource, value))
                {
                    _deviceNameAccessor.SetName(DeviceName.Microphone, "");
                    _deviceNameAccessor.SetName(DeviceName.Microphone, value, "first");
                    if (!string.IsNullOrEmpty(SelectedDocAudioSource))
                    {
                        _deviceNameAccessor.SetName(DeviceName.Microphone, SelectedDocAudioSource, "second");
                    }
                }
            }
        }

        public string SelectedDocAudioSource
        {
            get { return _docAudioSource; }
            set
            {
                if (SetProperty(ref _docAudioSource, value))
                {
                    _deviceNameAccessor.SetName(DeviceName.Microphone, "");
                    _deviceNameAccessor.SetName(DeviceName.Microphone, value, "second");
                    if (!string.IsNullOrEmpty(SelectedAudioSource))
                    {
                        _deviceNameAccessor.SetName(DeviceName.Microphone, SelectedAudioSource, "first");
                    }
                }
            }
        }

        public int SelectedSampleRate
        {
            get { return _sampleRate; }
            set { SetProperty(ref _sampleRate, value); }
        }

        public int SelectedAac
        {
            get { return _aac; }
            set { SetProperty(ref _aac, value); }
        }

        public string SelectedAudioOutPutDevice
        {
            get { return _audioOutPutDevice; }
            set
            {
                if (SetProperty(ref _audioOutPutDevice, value))
                {
                    _deviceNameAccessor.SetName(DeviceName.Speaker, value);
                }
            }
        }

        #endregion

        #region ctor

        public AudioSettingModel(AudioSettingView view)
        {
            _view = view;
            Aac = new ObservableCollection<int>();
            SampleRate = new ObservableCollection<int>();
            AudioSource = new ObservableCollection<string>();
            DocAudioSource = new ObservableCollection<string>();
            AudioOutPutDevice = new ObservableCollection<string>();

            _s = InputSimulatorManager.Instance.InputSimu;
            _configManager = new ConfigManager();
            _meetingSdkAgent = DependencyResolver.Current.GetService<IMeetingSdkAgent>();
            _localDataManager = DependencyResolver.Current.GetService<ILocalDataManager>();

            _deviceNameAccessor = DependencyResolver.Current.GetService<IDeviceNameAccessor>();
            _deviceConfigLoader = DependencyResolver.Current.GetService<IDeviceConfigLoader>();

            LoadCommand = new DelegateCommand(Loading);
            WindowKeyDownCommand = new DelegateCommand<object>(WindowKeyDownHandler);
            GoBackCommand = new DelegateCommand(GoBack);
            CheckPeopleSourceDeviceCommand = new DelegateCommand(CheckPeopleSourceDevice);
            CheckDocSourceDeviceCommand = new DelegateCommand(CheckDocSourceDevice);
        }

        #endregion

        #region method

        private void CheckPeopleSourceDevice()
        {
            if (SelectedAudioSource == SelectedDocAudioSource)
                SelectedDocAudioSource = string.Empty;
        }

        private void CheckDocSourceDevice()
        {
            if (SelectedAudioSource == SelectedDocAudioSource)
                SelectedAudioSource = string.Empty;
        }


        private void GoBack()
        {
            SaveSetting();
            var nav = new SettingNavView();
            nav.Show();
            _view.Close();
        }


        private void Loading()
        {
            try
            {
                //获取本地保存的配置
                _configManager = _localDataManager.GetSettingConfigData() ??
                                     new ConfigManager { AudioInfo = new AudioInfo(),ServerInfo = new ServerInfo() { BmsServerPort = GlobalData.Instance.LocalSetting.BmsServerPort, ServerIp = GlobalData.Instance.LocalSetting.ServerIp } };
                if (_configManager.AudioInfo == null)
                {
                    _configManager.AudioInfo = new AudioInfo();
                }
                var parameterData = _localDataManager.GetSettingParameter();
                //设备
                var audioSourceList = _meetingSdkAgent.GetMicrophones();
                var docSourceList = _meetingSdkAgent.GetMicrophones();
                var audioOutPutList = _meetingSdkAgent.GetLoudSpeakers();

                var sampleRateList = parameterData.AudioParameterSampleRates;
                var aac = parameterData.AudioParameterAACs;
                //装载数据源
                audioSourceList.Result.ToList().ForEach(a => { AudioSource.Add(a); });
                docSourceList.Result.ToList().ForEach(d => { DocAudioSource.Add(d); });
                audioOutPutList.Result.ToList().ForEach(o => { AudioOutPutDevice.Add(o); });
                aac.ForEach(o => { Aac.Add(o.AAC); });
                sampleRateList.ForEach(o => { SampleRate.Add(o.SampleRate); });
                AudioSource.Add(string.Empty);
                DocAudioSource.Add(string.Empty);

                //设置默认选项
                SetDefaultSetting();

                if (audioSourceList.Result.All(o => o != SelectedAudioSource))
                    SelectedAudioSource = string.Empty;
                if (docSourceList.Result.All(o => o != SelectedDocAudioSource))
                    SelectedDocAudioSource = string.Empty;
                if (audioOutPutList.Result.All(o => o != SelectedAudioOutPutDevice))
                    SelectedAudioOutPutDevice = string.Empty;

            }
            catch (Exception ex)
            {
                Log.Logger.Error($"音频设置加载信息发生异常 exception：{ex}");
                MessageQueueManager.Instance.AddError(MessageManager.LoadingError);
            }

        }


        private void SetDefaultSetting()
        {
            SelectedAudioSource = _configManager.AudioInfo.AudioSammpleDevice;
            SelectedAac = _configManager.AudioInfo.AAC;
            SelectedAudioOutPutDevice = _configManager.AudioInfo.AudioOutPutDevice;
            SelectedDocAudioSource = _configManager.AudioInfo.DocAudioSammpleDevice;
            SelectedSampleRate = _configManager.AudioInfo.SampleRate;
        }


        private void WindowKeyDownHandler(object obj)
        {
            var keyEventArgs = obj as KeyEventArgs;
            switch (keyEventArgs?.Key.ToString().ToLower())
            {
                case "home":
                case "escape":
                    //保存设置
                    SaveSetting();
                    break;
            }
        }

        private void SaveSetting()
        {
            _deviceConfigLoader.SaveConfig(_deviceNameAccessor);

            //if (_meetingService == null)
            //{
            //    _configManager.AudioInfo = new AudioInfo();
            //}
            //if (_meetingService == null) return;
            //_meetingService.SetDefaultDevice(AudioSourceType, SelectedAudioSource);
            //_meetingService.SetDefaultDevice(DocSourceType, SelectedDocAudioSource);
            //_meetingService.SetDefaultDevice(AudioOutPutDeviceType, SelectedAudioOutPutDevice);
            //_meetingService.SetAudioSampleRate(SelectedSampleRate);
            //_meetingService.SetAudioBitRate(SelectedAac);
            //保存设置到本地配置文件
            _configManager.AudioInfo.AAC = SelectedAac;
            _configManager.AudioInfo.AudioOutPutDevice = SelectedAudioOutPutDevice;
            _configManager.AudioInfo.AudioSammpleDevice = SelectedAudioSource;
            _configManager.AudioInfo.SampleRate = SelectedSampleRate;
            _configManager.AudioInfo.DocAudioSammpleDevice = SelectedDocAudioSource;
            _localDataManager.SaveSettingConfigData(_configManager);
        }

        #endregion

        #region command

        public ICommand LoadCommand { get; set; }

        public ICommand WindowKeyDownCommand { get; set; }

        public ICommand GoBackCommand { get; set; }

        public ICommand CheckPeopleSourceDeviceCommand { get; set; }

        public ICommand CheckDocSourceDeviceCommand { get; set; }

        #endregion

    }
}
