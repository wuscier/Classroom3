using System;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Input;
using WindowsInput.Native;
using Common.Contract;
using Common.Helper;
using Common.UiMessage;
using Classroom.Model;
using MeetingSdk.Wpf;
using MeetingSdk.NetAgent;
using System.Collections.Generic;
using System.Linq;
using MeetingSdk.NetAgent.Models;
using Common.Model;
using System.Threading;

namespace Classroom.ViewModel
{
    public class ClassModeViewModel : BindableBase
    {
        private readonly IMeetingSdkAgent _meetingService;
        private readonly IRemoteRecord _remoteRecord;
        private readonly ILocalDataManager _localDataManager;
        private readonly IMeetingWindowManager _windowManager;
        private readonly IDeviceNameAccessor _deviceNameAccessor;

        public ClassModeViewModel()
        {
            _localDataManager = DependencyResolver.Current.GetService<ILocalDataManager>();
            _meetingService = DependencyResolver.Current.GetService<IMeetingSdkAgent>();
            _windowManager = DependencyResolver.Current.GetService<IMeetingWindowManager>();
            _remoteRecord = DependencyResolver.Current.GetService<IRemoteRecord>();
            _deviceNameAccessor = DependencyResolver.Current.GetService<IDeviceNameAccessor>();

            CheckClassModeCommand = new DelegateCommand<object>(CheckClassModeAsync);
            ShareCommand = new DelegateCommand(ShareAsync);
            RecordCommand = DelegateCommand.FromAsyncHandler(RecordAsync);
            InitClassModeItems();
            InitToggleItems();
        }

        private void CheckClassModeAsync(object args)
        {
            if (args is KeyEventArgs)
            {
                KeyEventArgs keyEventArgs = (KeyEventArgs)args;

                switch (keyEventArgs.Key)
                {
                    case Key.Enter:
                        InputSimulatorManager.Instance.InputSimu.Keyboard.KeyPress(VirtualKeyCode.SPACE);
                        keyEventArgs.Handled = true;
                        break;
                }

            }

            if (args is ModeDisplayerType)
            {
                ModeDisplayerType classMode = (ModeDisplayerType)args;


                try
                {
                    if (_windowManager.ModeChange(classMode))
                    {
                        //_windowManager.LayoutRendererStore.CurrentLayoutRenderType = LayoutRenderType.AutoLayout;
                    }
                    else
                    {
                        CheckClassMode();
                    }
                }
                catch (Exception ex)
                {
                    MessageQueueManager.Instance.AddError(ex.Message);
                }
            }
        }

        private void CheckClassMode()
        {
            switch (_windowManager.ModeDisplayerStore.CurrentModeDisplayerType)
            {
                case ModeDisplayerType.SpeakerMode:
                    SpeakerModeItem.Checked = true;
                    break;
                case ModeDisplayerType.ShareMode:
                    ShareModeItem.Checked = true;
                    break;
                case ModeDisplayerType.InteractionMode:
                    InteractionModeItem.Checked = true;
                    break;
            }
        }

        private async Task RecordAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                if (!_remoteRecord.IsRemoteRecord)
                {
                    if (GlobalData.Instance.Course == null) return;
                    var result = _remoteRecord.AddRecord();
                    if (result)
                    {
                        _remoteRecord.IsRemoteRecord = true;
                        RecordToggleItem.IsChecked = true;
                    }
                    else
                    {
                        _remoteRecord.IsRemoteRecord = false;
                        RecordToggleItem.IsChecked = false;
                        MessageQueueManager.Instance.AddError("启动录制失败。");
                    }

                }
                else
                {
                    if (GlobalData.Instance.Course == null) return;
                    var result = _remoteRecord.StopRecord();
                    if (result)
                    {
                        _remoteRecord.IsRemoteRecord = false;
                        RecordToggleItem.IsChecked = false;
                    }
                    else
                    {
                        _remoteRecord.IsRemoteRecord = true;
                        RecordToggleItem.IsChecked = true;

                        MessageQueueManager.Instance.AddError("停止录制失败。");
                    }
                }
            });
        }

        private bool _isProcessingShareOperation = false;

        private async void ShareAsync()
        {
            if (_isProcessingShareOperation)
            {
                ShareToggleItem.IsChecked = !ShareToggleItem.IsChecked;
                return;
            }

            if (!_isProcessingShareOperation)
            {
                _isProcessingShareOperation = true;
            }

            try
            {
                MeetingResult<IList<VideoDeviceModel>> videoDeviceResult = _meetingService.GetVideoDevices();

                MeetingResult<IList<string>> micResult = _meetingService.GetMicrophones();

                ConfigManager configManager = _localDataManager.GetSettingConfigData();

                IEnumerable<string> docCameras;
                if (!_deviceNameAccessor.TryGetName(DeviceName.Camera, (devName) => { return devName.Option == "second"; }, out docCameras) || !videoDeviceResult.Result.Any(vdm => vdm.DeviceName == docCameras.FirstOrDefault()))
                {
                    ShareToggleItem.IsChecked = false;
                    MessageQueueManager.Instance.AddError("课件摄像头未配置！");
                    return;
                }


                if (configManager.DocVideoInfo?.DisplayWidth == 0 || configManager.DocVideoInfo?.DisplayHeight == 0 || configManager.DocVideoInfo?.VideoBitRate == 0)
                {
                    ShareToggleItem.IsChecked = false;
                    MessageQueueManager.Instance.AddError("课件采集参数未设置！");
                    return;
                }


                IEnumerable<string> docMics;
                if (!_deviceNameAccessor.TryGetName(DeviceName.Microphone, (devName) => { return devName.Option == "second"; }, out docMics) || !micResult.Result.Any(mic => mic == docMics.FirstOrDefault()))
                {
                    ShareToggleItem.IsChecked = false;
                    MessageQueueManager.Instance.AddError("课件麦克风未配置！");
                    return;
                }

                if (!_windowManager.Participant.IsSpeaking)
                {
                    ShareToggleItem.IsChecked = false;
                    MessageQueueManager.Instance.AddError("发言状态才可以进行课件分享！");
                    return;
                }

                if (ShareToggleItem.IsChecked)
                {
                    MeetingResult<int> publishDocCameraResult = await _windowManager.Publish(MeetingSdk.NetAgent.Models.MediaType.VideoDoc, docCameras.FirstOrDefault());
                    MeetingResult<int> publishDocMicResult = await _windowManager.Publish(MeetingSdk.NetAgent.Models.MediaType.AudioDoc, docMics.FirstOrDefault());

                    if (publishDocCameraResult.StatusCode != 0 || publishDocMicResult.StatusCode != 0)
                    {
                        ShareToggleItem.IsChecked = false;
                        MessageQueueManager.Instance.AddError("打开课件失败！");
                        return;
                    }

                    AppCache.AddOrUpdate(CacheKey.DocVideoResourceId, publishDocCameraResult.Result);
                    AppCache.AddOrUpdate(CacheKey.DocAudioResourceId, publishDocMicResult.Result);
                    AppCache.AddOrUpdate(CacheKey.IsDocOpen, true);
                }
                else
                {

                    object docCameraResourceIdObj = AppCache.TryGet(CacheKey.DocVideoResourceId);
                    int docCameraResourceId;
                    if (docCameraResourceIdObj == null || !int.TryParse(docCameraResourceIdObj.ToString(), out docCameraResourceId))
                    {
                        return;
                    }

                    object docAudioResourceIdObj = AppCache.TryGet(CacheKey.DocAudioResourceId);
                    int docAudioResourceId;
                    if (docAudioResourceIdObj == null || !int.TryParse(docAudioResourceIdObj.ToString(), out docAudioResourceId))
                    {
                        return;
                    }

                    MeetingResult stopShareCameraResult = await _windowManager.Unpublish(MeetingSdk.NetAgent.Models.MediaType.VideoDoc, docCameraResourceId);
                    MeetingResult stopShareMicResult = await _windowManager.Unpublish(MeetingSdk.NetAgent.Models.MediaType.AudioDoc, docAudioResourceId);

                    if (stopShareCameraResult.StatusCode != 0 || stopShareMicResult.StatusCode != 0)
                    {
                        ShareToggleItem.IsChecked = true;
                        MessageQueueManager.Instance.AddError("关闭课件失败！");
                        return;
                    }

                    //CheckClassModeAsync(ModeDisplayerType.InteractionMode);

                    AppCache.AddOrUpdate(CacheKey.IsDocOpen, false);
                }

                CheckClassMode();
            }
            catch (Exception)
            {
            }
            finally
            {
                _isProcessingShareOperation = false;
            }
        }

        public ICommand ShareCommand { get; set; }
        public ICommand RecordCommand { get; set; }
        public ICommand CheckClassModeCommand { get; set; }

        private void InitClassModeItems()
        {
            SpeakerModeItem = new RadioPictureButtonItem()
            {
                CheckCommand = CheckClassModeCommand,
                Type = ModeDisplayerType.SpeakerMode,
                Image = "/Common;Component/Image/kt_zj.png",
                Name = "主讲模式",
                GroupName = "ClassMode"
            };
            ShareModeItem = new RadioPictureButtonItem()
            {
                CheckCommand = CheckClassModeCommand,
                Type = ModeDisplayerType.ShareMode,
                Image = "/Common;Component/Image/kt_kj.png",
                Name = "课件模式",
                GroupName = "ClassMode"

            };
            InteractionModeItem = new RadioPictureButtonItem()
            {
                CheckCommand = CheckClassModeCommand,
                Type = ModeDisplayerType.InteractionMode,
                Image = "/Common;Component/Image/kt_hd.png",
                Name = "互动模式",
                GroupName = "ClassMode"

            };

            CheckClassMode();
        }

        private void InitToggleItems()
        {
            object isDocOpenObj = AppCache.TryGet(CacheKey.IsDocOpen);

            bool isDocOpen = false;
            if (isDocOpenObj != null && bool.TryParse(isDocOpenObj.ToString(), out isDocOpen))
            {

            }
            ShareToggleItem = new ToggleButtonItem()
            {
                Name = "课件分享",
                ToggleCommand = ShareCommand,
                IsChecked = isDocOpen
            };

            RecordToggleItem = new ToggleButtonItem()
            {
                Name = "课堂录制",
                ToggleCommand = RecordCommand,
                IsChecked = _remoteRecord.IsRemoteRecord
            };
        }

        public RadioPictureButtonItem SpeakerModeItem { get; set; }
        public RadioPictureButtonItem ShareModeItem { get; set; }
        public RadioPictureButtonItem InteractionModeItem { get; set; }

        public ToggleButtonItem ShareToggleItem { get; set; }
        public ToggleButtonItem RecordToggleItem { get; set; }


    }
}
