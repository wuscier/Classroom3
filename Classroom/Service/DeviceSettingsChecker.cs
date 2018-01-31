using Classroom.View;
using Common.Contract;
using Common.Helper;
using Common.Model;
using Common.UiMessage;
using MeetingSdk.NetAgent;
using MeetingSdk.NetAgent.Models;
using MeetingSdk.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Classroom.Service
{
    public class DeviceSettingsChecker
    {
        private readonly IMeetingSdkAgent _meetingSdkAgent;
        private readonly ILocalDataManager _localDataManager;

        private DeviceSettingsChecker()
        {
            _meetingSdkAgent = DependencyResolver.Current.GetService<IMeetingSdkAgent>();
            _localDataManager = DependencyResolver.Current.GetService<ILocalDataManager>();

        }

        public readonly static DeviceSettingsChecker Instance = new DeviceSettingsChecker();

        public bool IsVideoAudioSettingsValid(Window sourceWindow)
        {
            string errorMsg = string.Empty;

            MeetingResult<IList<VideoDeviceModel>> videoDeviceResult = _meetingSdkAgent.GetVideoDevices();

            MeetingResult<IList<string>> micResult = _meetingSdkAgent.GetMicrophones();

            MeetingResult<IList<string>> speakerResult = _meetingSdkAgent.GetLoudSpeakers();

            ConfigManager configManager = _localDataManager.GetSettingConfigData();

            if (configManager == null)
            {
                errorMsg = "参数配置有误！";
                MessageQueueManager.Instance.AddInfo(errorMsg);

                SettingNavView settingNavView = new SettingNavView();
                settingNavView.Show();

                sourceWindow?.Close();
                return false;
            }

            IDeviceNameAccessor deviceNameAccessor = DependencyResolver.Current.GetService<IDeviceNameAccessor>();

            IEnumerable<string> cameraDeviceName;
            if (videoDeviceResult.Result.Count == 0 || string.IsNullOrEmpty(configManager.MainVideoInfo?.VideoDevice) || !deviceNameAccessor.TryGetName(DeviceName.Camera, new Func<DeviceName, bool>(d => { return d.Option == "first"; }), out cameraDeviceName) || !videoDeviceResult.Result.Any(vdm => vdm.DeviceName == cameraDeviceName.FirstOrDefault()))
            {
                errorMsg = "人像采集未设置！";
                MessageQueueManager.Instance.AddInfo(errorMsg);

                VideoSettingView videoSettingView = new VideoSettingView();
                videoSettingView.Show();

                sourceWindow?.Close();
                return false;
            }

            if (configManager.MainVideoInfo?.DisplayWidth == 0 || configManager.MainVideoInfo?.DisplayHeight == 0 || configManager.MainVideoInfo?.VideoBitRate == 0)
            {
                errorMsg = "人像采集参数未设置！";
                MessageQueueManager.Instance.AddInfo(errorMsg);

                VideoSettingView videoSettingView = new VideoSettingView();
                videoSettingView.Show();

                sourceWindow?.Close();

                return false;
            }

            IEnumerable<string> micDeviceName;
            if (micResult.Result.Count == 0 || string.IsNullOrEmpty(configManager.AudioInfo?.AudioSammpleDevice) || !deviceNameAccessor.TryGetName(DeviceName.Microphone, new Func<DeviceName, bool>(d => { return d.Option == "first"; }), out micDeviceName) || !micResult.Result.Any(mic => mic == micDeviceName.FirstOrDefault()))
            {
                errorMsg = "人声音源未设置！";
                MessageQueueManager.Instance.AddInfo(errorMsg);

                AudioSettingView audioSettingView = new AudioSettingView();
                audioSettingView.Show();

                sourceWindow?.Close();

                return false;
            }

            if (configManager.AudioInfo?.SampleRate == 0 || configManager.AudioInfo?.AAC == 0)
            {
                errorMsg = "人声音源参数未设置！";
                MessageQueueManager.Instance.AddInfo(errorMsg);

                AudioSettingView audioSettingView = new AudioSettingView();
                audioSettingView.Show();

                sourceWindow?.Close();

                return false;
            }

            string audioOutputDeviceName;
            if (speakerResult.Result.Count == 0 || string.IsNullOrEmpty(configManager.AudioInfo?.AudioOutPutDevice) || !deviceNameAccessor.TryGetSingleName(DeviceName.Speaker, out audioOutputDeviceName) || !speakerResult.Result.Any(speaker => speaker == audioOutputDeviceName))
            {
                errorMsg = "放音设备未设置！";
                MessageQueueManager.Instance.AddInfo(errorMsg);

                AudioSettingView audioSettingView = new AudioSettingView();
                audioSettingView.Show();

                sourceWindow?.Close();

                return false;
            }


            return true;
        }
    }
}
