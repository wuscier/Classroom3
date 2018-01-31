using System;
using System.IO;
using Common.Contract;
using Common.Helper;
using Common.Model;
using Serilog;
using MeetingSdk.NetAgent.Models;
using MeetingSdk.NetAgent;
using System.Threading;

namespace Service
{
    public class LocalRecordService : IRecordLive
    {
        private readonly IMeetingSdkAgent _meetingService;
        private readonly ILocalDataManager _localDataManager;
        private readonly object _syncRoot = new object();

        public LocalRecordService()
        {
            _meetingService = DependencyResolver.Current.GetService<IMeetingSdkAgent>();
            _localDataManager = DependencyResolver.Current.GetService<ILocalDataManager>();
        }

        public int RecordId { get; set; }
        public string RecordDirectory { get; private set; }
        public PublishLiveStreamParameter RecordParam { get; private set; }

        public void ResetStatus()
        {
            RecordDirectory = string.Empty;
            RecordId = 0;
        }

        public bool GetRecordParam()
        {
            ConfigManager configManager = _localDataManager.GetSettingConfigData();

            try
            {
                if (configManager?.RecordInfo == null) return false;
                RecordParam = new PublishLiveStreamParameter()
                {
                    LiveParameter = new LiveParameter()
                    {
                        AudioBitrate = 64,
                        BitsPerSample = 16,
                        Channels = 2,
                        SampleRate = configManager.AudioInfo.SampleRate,
                        VideoBitrate = configManager.RecordInfo.RecordBitRate,
                        Width = configManager.RecordInfo.RecordDisplayWidth,
                        Height = configManager.RecordInfo.RecordDisplayHeight,
                        IsRecord = true,
                        IsLive = false,
                        FilePath = configManager.RecordInfo.RecordDirectory,
                    },
                    MediaType = MediaType.StreamMedia,
                    StreamType = StreamType.Live,

                };

                RecordDirectory = configManager.RecordInfo.RecordDirectory;

                return true;
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"【get record param exception】：{ex}");
                return false;
            }
        }

        public MeetingResult RefreshLiveStream(VideoStreamModel[] videoStreamModels, AudioStreamModel[] audioStreamModels)
        {
            if (RecordId == 0)
            {
                return new MeetingResult()
                {
                    Message = MessageManager.NoLiveToRefresh,
                    StatusCode = -1,
                };
            }

            Monitor.Enter(_syncRoot);

            MeetingResult updateVideoResult = _meetingService.UpdateLiveStreamVideoInfo(RecordId, videoStreamModels);
            MeetingResult updateAudioResult = _meetingService.UpdateLiveStreamAudioInfo(RecordId, audioStreamModels);

            MeetingResult mergedResult = new MeetingResult()
            {
                Message = "更新录制成功！",
                StatusCode = 0
            };

            if (updateAudioResult.StatusCode != 0)
            {
                mergedResult.StatusCode = -1;
                mergedResult.Message = updateAudioResult.Message;
            }

            if (updateVideoResult.StatusCode != 0)
            {
                mergedResult.StatusCode = -1;
                mergedResult.Message += $" {updateVideoResult.Message}";
            }

            Monitor.Exit(_syncRoot);

            return mergedResult;
        }

        public MeetingResult StartMp4Record(VideoStreamModel[] videoStreamModels, AudioStreamModel[] audioStreamModels)
        {
            if (string.IsNullOrEmpty(RecordDirectory) || !Directory.Exists(RecordDirectory))
            {
                return new MeetingResult()
                {
                    Message = MessageManager.RecordDirectoryNotSet,
                    StatusCode = -1,
                };
            }

            MeetingResult<int> publishLiveResult = _meetingService.PublishLiveStream(RecordParam);

            if (publishLiveResult.StatusCode != 0)
            {
                return publishLiveResult;
            }

            RecordId = publishLiveResult.Result;

            MeetingResult updateVideoResult = _meetingService.UpdateLiveStreamVideoInfo(publishLiveResult.Result, videoStreamModels);
            MeetingResult updateAudioResult = _meetingService.UpdateLiveStreamAudioInfo(publishLiveResult.Result, audioStreamModels);


            string filename = Path.Combine(RecordDirectory, DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss") + ".mp4");

            MeetingResult startRecordResult = _meetingService.StartMp4Record(publishLiveResult.Result, filename);


            MeetingResult mergedResult = new MeetingResult()
            {
                Message = "录制成功！",
                StatusCode = 0
            };

            if (updateVideoResult.StatusCode != 0)
            {
                mergedResult.StatusCode = -1;
                mergedResult.Message = updateVideoResult.Message;
            }

            if (updateAudioResult.StatusCode != 0)
            {
                mergedResult.StatusCode = -1;
                mergedResult.Message += $" {updateAudioResult.Message}";
            }

            if (startRecordResult.StatusCode != 0)
            {
                mergedResult.StatusCode = -1;
                mergedResult.Message += $" {startRecordResult.Message}";
            }

            return mergedResult;
        }

        public MeetingResult StopMp4Record()
        {
            if (RecordId == 0)
            {
                return new MeetingResult()
                {
                    Message = MessageManager.NoLiveToStop,
                    StatusCode = -1,
                };
            }

            MeetingResult stopRecordResult = _meetingService.StopMp4Record(RecordId);
            MeetingResult unpublishLiveResult = _meetingService.UnpublishLiveStream(RecordId);
            RecordId = 0;


            MeetingResult mergedResult = new MeetingResult()
            {
                Message = "停止录制成功！",
                StatusCode = 0
            };

            if (stopRecordResult.StatusCode != 0)
            {
                mergedResult.StatusCode = -1;
                mergedResult.Message = stopRecordResult.Message;
            }

            if (unpublishLiveResult.StatusCode != 0)
            {
                mergedResult.StatusCode = -1;
                mergedResult.Message += $" {unpublishLiveResult.Message}";
            }

            return mergedResult;
        }
    }
}
