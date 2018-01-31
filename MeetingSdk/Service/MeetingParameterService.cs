using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Contract.MeetingSdk;
using Common.Helper;
using System.Runtime.InteropServices;
using Serilog;

namespace MeetingSdk.Service
{
    public class MeetingParameterService : IMeetingParameter
    {
        private readonly IMeetingManager _meetingManager;

        public MeetingParameterService()
        {
            _meetingManager = DependencyResolver.Current.GetService<IMeetingManager>();
        }

        public Device[] GetDevices(int deviceType)
        {
            if (!_meetingManager.IsServierStarted)
            {
                return Device.EmptyDevices;
            }

            var deviceListPointer = IntPtr.Zero;

            try
            {
                int maxDeviceCount = 10, getDeviceCount = 0;
                var deviceInfoByte = Marshal.SizeOf(typeof(DeviceInfo));

                var maxDeviceBytes = deviceInfoByte * maxDeviceCount;
                deviceListPointer = Marshal.AllocHGlobal(maxDeviceBytes);

                var result = MeetingAgent.GetDeviceList(deviceType, deviceListPointer, maxDeviceCount,
                    ref getDeviceCount);
                if (result != 0)
                    return Device.EmptyDevices;

                Device[] devices = new Device[getDeviceCount];
                for (var i = 0; i < getDeviceCount; i++)
                {
                    var pointer = (IntPtr) (deviceListPointer.ToInt64() + i * deviceInfoByte);
                    DeviceInfo deviceInfo = (DeviceInfo) Marshal.PtrToStructure(pointer, typeof(DeviceInfo));

                    devices[i] = new Device()
                    {
                        IsDefault = deviceInfo.m_isDefault == 1,
                        Name = deviceInfo.m_szDevName
                    };
                }

                return devices;
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"GetDevices({deviceType}) exception：{ex}");
                return Device.EmptyDevices;
            }
            finally
            {
                if (deviceListPointer != IntPtr.Zero)
                    Marshal.FreeHGlobal(deviceListPointer);
            }
        }

        public void SetAudioBitrate(int bitrate)
        {
            if (_meetingManager.IsServierStarted)
            {
                int result = MeetingAgent.SetAudioCapBitRate(bitrate);
                Log.Logger.Debug($"SetAudioBitrate({bitrate}) result：{result}");
            }
        }

        public void SetAudioSampleRate(int sampleRate)
        {
            if (_meetingManager.IsServierStarted)
            {
                int result = MeetingAgent.SetAudioCapSampleRate(sampleRate);
                Log.Logger.Debug($"SetAudioSampleRate({sampleRate}) result：{result}");
            }
        }

        public void SetCameraResolution(int cameraType, int width, int height)
        {
            if (_meetingManager.IsServierStarted)
            {
                int result = MeetingAgent.SetVideoCapResolution(cameraType, width, height);
                Log.Logger.Debug($"SetCameraResolution({cameraType},{width},{height}) result：{result}");
            }
        }

        public Task<ReturnMessage> SetDefaultCamera(int cameraType, string cameraName)
        {
            Task<ReturnMessage> checkTask = _meetingManager.CheckMeetingServerStatus();
            if (checkTask != null)
            {
                return checkTask;
            }

            var tcs = new TaskCallback<ReturnMessage>("SetDefaultCamera");

            if (_meetingManager.TaskCallbacks.ContainsKey(tcs.Name))
                _meetingManager.TaskCallbacks.Remove(tcs.Name);

            _meetingManager.TaskCallbacks.Add(tcs.Name, tcs);

            var result = MeetingAgent.SetDefaultCamera(cameraType, cameraName);

            if (result != 0)
            {
                return Task.FromResult(MessageManager.SetDefaultCameraErrorMessage);
            }

            return tcs.Task;
        }

        public void SetDefaultDevice(int deviceType, string deviceName)
        {
            if (_meetingManager.IsServierStarted)
            {
                int result = MeetingAgent.SetDefaultDevice(deviceType, deviceName);
                Log.Logger.Debug($"SetDefaultDevice({deviceType},{deviceName}) result：{result}");
            }
        }

        public void SetVideoBitrate(int cameraType, int bitrate)
        {
            if (_meetingManager.IsServierStarted)
            {
                int result = MeetingAgent.SetVideoCapBitRate(cameraType, bitrate);
                Log.Logger.Debug($"SetVideoBitrate({cameraType},{bitrate}) result：{result}");
            }
        }

        public Camera GetCameraParameters(string cameraName)
        {

            var cameraDeviceInfoPtr = IntPtr.Zero;
            Camera camera = new Camera()
            {
                CameraParameters = new List<CameraParameter>()
            };

            try
            {
                var cameraInfoByte = Marshal.SizeOf(typeof(VideoDeviceInfo));

                cameraDeviceInfoPtr = Marshal.AllocHGlobal(cameraInfoByte);

                var result = MeetingAgent.GetCameraInfo(cameraName, cameraDeviceInfoPtr);

                var pointer = new IntPtr(cameraDeviceInfoPtr.ToInt64());

                var cameraInfo = (VideoDeviceInfo) Marshal.PtrToStructure(pointer, typeof(VideoDeviceInfo));

                camera.Name = cameraInfo.Name;
                for (int i = 0; i < cameraInfo.FormatCount; i++)
                {
                    VideoFormat videoFormat = cameraInfo.Formats[i];

                    CameraParameter cameraParameter = new CameraParameter()
                    {
                        ColorSpace = videoFormat.ColorSpace.ToString(),
                        Fps = new List<int>(),
                        VideSizes = new List<Size>()
                    };
                    for (int j = 0; j < videoFormat.sizeCount; j++)
                    {
                        VideoSize videoSize = videoFormat.VideoSizes[j];
                        cameraParameter.VideSizes.Add(new Size()
                        {
                            Width = videoSize.Width,
                            Height = videoSize.Height
                        });
                    }

                    for (int k = 0; k < videoFormat.fpsCount; k++)
                    {
                        cameraParameter.Fps.Add(videoFormat.Fps[k]);
                    }

                    camera.CameraParameters.Add(cameraParameter);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"GetCameraParameters({cameraName}) exception：{ex}");
            }
            finally
            {
                if (cameraDeviceInfoPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(cameraDeviceInfoPtr);
                }
            }

            return camera;
        }

        public string Imei
        {
            get
            {
                IntPtr ptr = Marshal.AllocHGlobal(24);
                string imei = string.Empty;
                try
                {
                    int result = MeetingAgent.GetSerialNo(ptr);
                    imei = Marshal.PtrToStringAnsi(ptr, 15);
                    Log.Logger.Debug($"【GetImei()】：result={result}, imei={imei}");
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"GetImei() exception：{ex}");
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                }

                return imei;
            }
        }
    }
}
