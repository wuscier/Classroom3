using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
using ConfigManager = Common.Model.ConfigManager;
using MeetingSdk.Wpf;
using MeetingSdk.NetAgent;
using MeetingSdk.NetAgent.Models;
using System.Windows;

namespace Classroom.ViewModel
{
    public class VideoSettingModel : BindableBase
    {

        #region field

        //private readonly IMeeting _meetingService;
        private readonly IMeetingSdkAgent _meetingSdkAgent;
        private readonly ILocalDataManager _localDataManager;
        private readonly IDeviceNameAccessor _deviceNameAccessor;
        private readonly IDeviceConfigLoader _deviceConfigLoader;


        private readonly VideoSettingView _view;

        private string _selectedCameraDevice;
        private string _selectedDocDevice;
        private string _selectedVedioVGA;
        private int _selectedVedioRate;
        private string _selectedDocVGA;
        private int _selectedDocRate;
        private const int CameraDeviceType = 1;
        private const int DocDeviceType = 2;
        private ConfigManager _configManager;
        private readonly InputSimulator _s;
        private List<VideoDeviceModel> _cameraDeviceList;
        private List<VideoDeviceModel> _docDeviceList;

        #endregion

        #region property

        public ObservableCollection<string> CameraDeviceList { get; set; }
        public ObservableCollection<string> DocDeviceList { get; set; }

        public ObservableCollection<VideoFormatModel> CameraColorSpaces { get; set; }
        public ObservableCollection<VideoFormatModel> DocColorSpaces { get; set; }

        public ObservableCollection<string> VedioParameterVgaList { get; set; }
        public ObservableCollection<string> DocParameterVgaList { get; set; }
        public ObservableCollection<int> VedioParameterRatesList { get; set; }


        public string SelectedCameraDevice
        {
            get { return _selectedCameraDevice; }
            set
            {

                if (SetProperty(ref _selectedCameraDevice, value))
                {
                    _deviceNameAccessor.SetName(DeviceName.Camera, "");
                    _deviceNameAccessor.SetName(DeviceName.Camera, value, "first");
                    if (!string.IsNullOrEmpty(SelectedDocDevice))
                    {
                        _deviceNameAccessor.SetName(DeviceName.Camera, SelectedDocDevice, "second");
                    }

                    UpdateCameraColorSpace();
                }
            }
        }

        private void UpdateCameraColorSpace()
        {
            VideoDeviceModel videoDeviceModel = _cameraDeviceList.FirstOrDefault(camera => camera.DeviceName == SelectedCameraDevice);

            if (videoDeviceModel != null)
            {
                CameraColorSpaces.Clear();

                videoDeviceModel.VideoFormatModels.ForEach(vfm =>
                {
                    CameraColorSpaces.Add(vfm);
                });

                SelectedCameraColorSpace = videoDeviceModel.VideoFormatModels.FirstOrDefault();
            }
            else
            {
                SelectedCameraColorSpace = null;
            }
        }

        public string SelectedDocDevice
        {
            get { return _selectedDocDevice; }
            set
            {
                if (SetProperty(ref _selectedDocDevice, value))
                {
                    _deviceNameAccessor.SetName(DeviceName.Camera, "");
                    _deviceNameAccessor.SetName(DeviceName.Camera, value, "second");
                    if (!string.IsNullOrEmpty(SelectedCameraDevice))
                    {
                        _deviceNameAccessor.SetName(DeviceName.Camera, SelectedCameraDevice, "first");
                    }

                    UpdateDocColorSpace();
                }

            }
        }

        private void UpdateDocColorSpace()
        {
            VideoDeviceModel videoDeviceModel = _docDeviceList.FirstOrDefault(camera => camera.DeviceName == SelectedDocDevice);
            if (videoDeviceModel != null)
            {
                DocColorSpaces.Clear();

                videoDeviceModel.VideoFormatModels.ForEach(vfm =>
                {
                    DocColorSpaces.Add(vfm);
                });

                SelectedDocColorSpace = videoDeviceModel.VideoFormatModels.FirstOrDefault();
            }
            else
            {
                SelectedDocColorSpace = null;
            }
        }

        private VideoFormatModel _selectedCameraColorSpace;
        public VideoFormatModel SelectedCameraColorSpace
        {
            get { return _selectedCameraColorSpace; }
            set
            {
                if (SetProperty(ref _selectedCameraColorSpace, value))
                {
                    UpdateCameraVgaSource();
                }
            }
        }


        private VideoFormatModel _selectedDocColorSpace;
        public VideoFormatModel SelectedDocColorSpace
        {
            get { return _selectedDocColorSpace; }
            set
            {
                if (SetProperty(ref _selectedDocColorSpace, value))
                {
                    UpdateDocVgaSource();
                }
            }
        }


        public string SelectedVedioVGA
        {
            get { return _selectedVedioVGA; }
            set { SetProperty(ref _selectedVedioVGA, value); }
        }

        public int SelectedVedioRate
        {
            get { return _selectedVedioRate; }
            set { SetProperty(ref _selectedVedioRate, value); }
        }

        public string SelectedDocVGA
        {
            get { return _selectedDocVGA; }
            set { SetProperty(ref _selectedDocVGA, value); }
        }

        public int SelectedDocRate
        {
            get { return _selectedDocRate; }
            set { SetProperty(ref _selectedDocRate, value); }
        }


        #endregion

        #region ctor

        public VideoSettingModel(VideoSettingView view)
        {
            _view = view;
            _cameraDeviceList = new List<VideoDeviceModel>();
            _docDeviceList = new List<VideoDeviceModel>();
            _s = new InputSimulator();

            _configManager = new ConfigManager();
            _meetingSdkAgent = DependencyResolver.Current.GetService<IMeetingSdkAgent>();
            _localDataManager = DependencyResolver.Current.GetService<ILocalDataManager>();

            _deviceNameAccessor = DependencyResolver.Current.GetService<IDeviceNameAccessor>();
            _deviceConfigLoader = DependencyResolver.Current.GetService<IDeviceConfigLoader>();

            CameraDeviceList = new ObservableCollection<string>();
            DocDeviceList = new ObservableCollection<string>();

            CameraColorSpaces = new ObservableCollection<VideoFormatModel>();
            DocColorSpaces = new ObservableCollection<VideoFormatModel>();

            VedioParameterVgaList = new ObservableCollection<string>();
            DocParameterVgaList = new ObservableCollection<string>();
            VedioParameterRatesList = new ObservableCollection<int>();
            LoadCommand = new DelegateCommand(Loading);
            WindowKeyDownCommand = new DelegateCommand<object>(WindowKeyDownHandler);
            CheckCameraDeviceCommand = DelegateCommand.FromAsyncHandler(CheckCameraDeviceAsync);
            CheckDocDeviceCommand = DelegateCommand.FromAsyncHandler(CheckDocDeviceAsync);

            //CheckCameraColorSpaceCommand = new DelegateCommand(CheckCameraColorSpace);
            //CheckDocColorSpaceCommand = new DelegateCommand(CheckDocColorSpace);

            GoBackCommand = new DelegateCommand(GoBack);

        }

        //private void CheckDocColorSpace()
        //{
        //    throw new NotImplementedException();
        //}

        //private void CheckCameraColorSpace()
        //{
        //    throw new NotImplementedException();
        //}

        #endregion

        #region method

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
                _configManager = _localDataManager.GetSettingConfigData() ?? new ConfigManager() { ServerInfo = new ServerInfo() { BmsServerPort = GlobalData.Instance.LocalSetting.BmsServerPort, ServerIp = GlobalData.Instance.LocalSetting.ServerIp } };

                if (_configManager == null)
                {
                    MessageQueueManager.Instance.AddError("读取配置文件时出错！");
                    return;
                }

                if (_configManager.MainVideoInfo == null)
                {
                    _configManager.MainVideoInfo = new VideoInfo();
                }
                if (_configManager.DocVideoInfo == null)
                {
                    _configManager.DocVideoInfo = new VideoInfo();
                }

                CameraDeviceList.Clear();
                DocDeviceList.Clear();

                CameraColorSpaces.Clear();
                DocColorSpaces.Clear();

                VedioParameterVgaList.Clear();
                DocParameterVgaList.Clear();
                VedioParameterRatesList.Clear();


                //摄像头设备
                var cameraList = _meetingSdkAgent.GetVideoDevices();

                if (cameraList.Result == null)
                {
                    MessageQueueManager.Instance.AddError("无法获取本机设备信息！");
                    return;
                }

                _cameraDeviceList = cameraList.Result.ToList();

                string log = "↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓\r\n";
                foreach (var cameraDevice in _cameraDeviceList)
                {
                    log += $"name：{cameraDevice.DeviceName}, ";

                    foreach (var format in cameraDevice.VideoFormatModels)
                    {
                        log += $"{format.ColorspaceName}\r\n";

                        foreach (var size in format.SizeModels)
                        {
                            log += $"size：{size.Width}*{size.Height}\r\n";
                        }
                    }
                }

                log += "↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑\r\n";

                Log.Logger.Information(log);

                var docCameraList = _meetingSdkAgent.GetVideoDevices();

                _docDeviceList = docCameraList.Result.ToList();

                //码率
                var settingLocalData = _localDataManager.GetSettingParameter();
                if (settingLocalData != null)
                {
                    var rateList = settingLocalData.VedioParameterRates;
                    rateList.ForEach(v => { VedioParameterRatesList.Add(v.VideoBitRate); });
                }
                _cameraDeviceList.ForEach(c => { CameraDeviceList.Add(c.DeviceName); });
                _docDeviceList.ForEach(d => { DocDeviceList.Add(d.DeviceName); });
                CameraDeviceList.Add("");
                DocDeviceList.Add("");
                SetDefaultSetting();

                if (_cameraDeviceList.All(o => o.DeviceName != SelectedCameraDevice))
                    SelectedCameraDevice = string.Empty;
                if (_docDeviceList.All(o => o.DeviceName != SelectedDocDevice))
                    SelectedDocDevice = string.Empty;
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"视频设置加载信息发生异常 exception：{ex}");
                MessageQueueManager.Instance.AddError(MessageManager.LoadingError);
            }
        }

        private async Task CheckCameraDeviceAsync()
        {
            await _view.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (SelectedCameraDevice == SelectedDocDevice)
                    SelectedDocDevice = string.Empty;
            }));
        }

        private void UpdateCameraVgaSource()
        {
            VedioParameterVgaList.Clear();
            var cameraVgaList = VgaList(SelectedCameraColorSpace);
            cameraVgaList.ForEach(v => { VedioParameterVgaList.Add(v); });
        }

        private void UpdateDocVgaSource()
        {
            DocParameterVgaList.Clear();
            var docVgaList = VgaList(SelectedDocColorSpace);
            docVgaList.ForEach(v => { DocParameterVgaList.Add(v); });
        }

        private async Task CheckDocDeviceAsync()
        {
            await _view.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (SelectedCameraDevice == SelectedDocDevice)
                    SelectedCameraDevice = string.Empty;
            }));
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
                _deviceConfigLoader.SaveConfig(_deviceNameAccessor);

                var cameraDeviceName = SelectedCameraDevice;
                var docDeviceName = SelectedDocDevice;
                var videoVga = SelectedVedioVGA;
                var docVga = SelectedDocVGA;
                var videoRate = SelectedVedioRate;
                _configManager.MainVideoInfo = new VideoInfo();
                _configManager.DocVideoInfo = new VideoInfo();

                if (string.IsNullOrEmpty(SelectedCameraDevice) && string.IsNullOrEmpty(SelectedDocDevice))
                    throw new Exception("未选择任何视频采集源！");

                if (!string.IsNullOrEmpty(SelectedCameraDevice))
                {
                    if (!string.IsNullOrEmpty(videoVga))
                    {
                        var videoVgaWith = int.Parse(videoVga.Split('*')[0]);
                        var videoVgaHeight = int.Parse(videoVga.Split('*')[1]);

                        _configManager.MainVideoInfo.DisplayWidth = videoVgaWith;
                        _configManager.MainVideoInfo.DisplayHeight = videoVgaHeight;
                    }
                    //视频设置
                    //var result1 = _meetingService.SetDefaultDevice(CameraDeviceType, cameraDeviceName);
                    //if (result1.HasError)
                    //    throw new Exception(MessageManager.SaveError);
                    //var result2 = _meetingService.SetVideoBitRate(CameraDeviceType, videoRate);
                    //if (result2.HasError)
                    //    throw new Exception(MessageManager.SaveError);
                    //var result3 = _meetingService.SetVideoResolution(CameraDeviceType, videoVgaWith, videoVgaHeight);
                    //if (result3.HasError)
                    //    throw new Exception(MessageManager.SaveError);

                    _configManager.MainVideoInfo.VideoDevice = cameraDeviceName;
                    _configManager.MainVideoInfo.VideoBitRate = videoRate;
                    _configManager.MainVideoInfo.ColorSpace = SelectedCameraColorSpace.Colorsapce;
                }

                if (!string.IsNullOrEmpty(SelectedDocDevice))
                {
                    if (!string.IsNullOrEmpty(docVga))
                    {
                        var docVgaWith = int.Parse(docVga.Split('*')[0]);
                        var docVgaHeight = int.Parse(docVga.Split('*')[1]);

                        _configManager.DocVideoInfo.DisplayHeight = docVgaHeight;
                        _configManager.DocVideoInfo.DisplayWidth = docVgaWith;

                    }

                    var docRate = SelectedDocRate;

                    //课件设置
                    //var result4 = _meetingService.SetDefaultDevice(DocDeviceType, docDeviceName);
                    //if (result4.HasError)
                    //    throw new Exception(MessageManager.SaveError);
                    //var result5 = _meetingService.SetVideoBitRate(DocDeviceType, docRate);
                    //if (result5.HasError)
                    //    throw new Exception(MessageManager.SaveError);
                    //var result6 = _meetingService.SetVideoResolution(DocDeviceType, docVgaWith, docVgaHeight);
                    //if (result6.HasError)
                    //    throw new Exception(MessageManager.SaveError);
                    _configManager.DocVideoInfo.VideoBitRate = docRate;
                    _configManager.DocVideoInfo.VideoDevice = docDeviceName;
                    _configManager.DocVideoInfo.ColorSpace = SelectedDocColorSpace.Colorsapce;
                }
                if (!string.IsNullOrEmpty(SelectedDocDevice) || !string.IsNullOrEmpty(SelectedCameraDevice))
                    _localDataManager.SaveSettingConfigData(_configManager);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"视频设置SaveSetting() exception：{ex}");
                MessageQueueManager.Instance.AddError(ex.Message);
            }
        }

        private void SetDefaultSetting()
        {
            SelectedCameraDevice = _configManager.MainVideoInfo.VideoDevice;
            SelectedDocDevice = _configManager.DocVideoInfo.VideoDevice;

            SelectedCameraColorSpace = _cameraDeviceList.FirstOrDefault(vdm => vdm.DeviceName == SelectedCameraDevice)?.VideoFormatModels.FirstOrDefault(vfm => vfm.Colorsapce == _configManager.MainVideoInfo.ColorSpace);
            SelectedDocColorSpace = _docDeviceList.FirstOrDefault(vdm => vdm.DeviceName == SelectedDocDevice)?.VideoFormatModels.FirstOrDefault(vfm => vfm.Colorsapce == _configManager.DocVideoInfo.ColorSpace);

            SelectedVedioVGA = $"{_configManager.MainVideoInfo.DisplayWidth}*{_configManager.MainVideoInfo.DisplayHeight}";
            SelectedDocVGA = $"{_configManager.DocVideoInfo.DisplayWidth}*{_configManager.DocVideoInfo.DisplayHeight}";

            SelectedVedioRate = _configManager.MainVideoInfo.VideoBitRate;
            SelectedDocRate = _configManager.DocVideoInfo.VideoBitRate;
        }


        private List<string> VgaList(VideoFormatModel videoFormatModel)
        {
            var vgaList = new List<string>();

            if (videoFormatModel == null)
            {
                return vgaList;
            }

            if (videoFormatModel.SizeModels.Count == 0)
            {
                return vgaList;
            }

            videoFormatModel.SizeModels.ForEach(size =>
                    {
                        vgaList.Add($"{size.Width}*{size.Height}");
                    });

            return vgaList.Distinct().ToList();
        }

        #endregion

        #region command

        public ICommand LoadCommand { get; set; }

        public ICommand WindowKeyDownCommand { get; set; }

        public ICommand CheckCameraDeviceCommand { get; set; }

        public ICommand CheckDocDeviceCommand { get; set; }

        //public ICommand CheckCameraColorSpaceCommand { get; set; }
        //public ICommand CheckDocColorSpaceCommand { get; set; }

        public ICommand GoBackCommand { get; set; }

        #endregion

    }
}
