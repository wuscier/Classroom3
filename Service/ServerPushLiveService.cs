using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Contract;
using Common.Helper;
using Common.Model;
using MeetingSdk.SdkWrapper;
using MeetingSdk.SdkWrapper.MeetingDataModel;
using Serilog;
using Newtonsoft.Json;
using MeetingSdk.NetAgent;
using MeetingSdk.NetAgent.Models;

namespace Service
{
    public class ServerPushLiveService : IPushServerLive
    {
        private readonly IMeetingSdkAgent _meetingService;
        private readonly ILocalDataManager _localDataManager;

        public ServerPushLiveService()
        {
            _localDataManager = DependencyResolver.Current.GetService<ILocalDataManager>();
            _meetingService = DependencyResolver.Current.GetService<IMeetingSdkAgent>();
        }

        public void ResetStatus()
        {
            LiveId = 0;
            HasPushLiveSuccessfully = false;
        }

        public bool HasPushLiveSuccessfully { get; set; }
        public int LiveId { get; set; }
        public PublishLiveStreamParameter LiveParam { get; private set; }

        public PublishLiveStreamParameter GetLiveParam()
        {
            ConfigManager configManager = _localDataManager.GetSettingConfigData();

            try
            {
                LiveParam = new PublishLiveStreamParameter
                {
                    LiveParameter=new LiveParameter()
                    {
                        AudioBitrate = 64,
                        BitsPerSample = 16,
                        Channels = 1,
                        IsLive = true,
                        IsRecord = false,
                        SampleRate = 8000,
                        VideoBitrate = configManager.RemoteLiveStreamInfo.LiveStreamBitRate,
                        Width = configManager.RemoteLiveStreamInfo.LiveStreamDisplayWidth,
                        Height = configManager.RemoteLiveStreamInfo.LiveStreamDisplayHeight
                    }
                };
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"【get server push live param exception】：{ex}");

                return null;
            }

            return LiveParam;
        }

        //public async Task<AsyncCallbackMsg> StartPushLiveStream(List<LiveVideoStream> liveVideoStreamInfos,
        //    string pushLiveUrl = "")
        //{
        //    if (string.IsNullOrEmpty(pushLiveUrl))
        //    {
        //        return AsyncCallbackMsg.GenerateMsg(MessageManager.NoPushLiveUrl);
        //    }
        //    LiveParam.Url1 = pushLiveUrl;

        //    if (LiveParam.Width == 0 || LiveParam.Height == 0 || LiveParam.VideoBitrate == 0)
        //    {
        //        return AsyncCallbackMsg.GenerateMsg(MessageManager.PushLiveResolutionNotSet);
        //    }

        //    Log.Logger.Debug(
        //        $"【server push live begins】：width={LiveParam.Width}, height={LiveParam.Height}, bitrate={LiveParam.VideoBitrate}, url={LiveParam.Url1}, videos={liveVideoStreamInfos.Count}");

        //    for (int i = 0; i < liveVideoStreamInfos.Count; i++)
        //    {
        //        Log.Logger.Debug(
        //            $"video{i + 1}：x={liveVideoStreamInfos[i].X}, y={liveVideoStreamInfos[i].Y}, width={liveVideoStreamInfos[i].Width}, height={liveVideoStreamInfos[i].Height}");
        //    }

        //    AsyncCallbackMsg startLiveStreamResult =
        //        await _meetingService.StartLiveStream(LiveParam, liveVideoStreamInfos.ToArray(),
        //            liveVideoStreamInfos.Count);

        //    if (startLiveStreamResult.Data != null)
        //        LiveId = int.Parse(startLiveStreamResult.Data.ToString());

        //    if (startLiveStreamResult.HasError)
        //    {
        //        HasPushLiveSuccessfully = false;
        //        Log.Logger.Error($"【server push live failed】：{startLiveStreamResult.Message}");

        //    }
        //    else
        //    {
        //        HasPushLiveSuccessfully = true;
        //        Log.Logger.Debug($"【server push live succeeded】：liveId={startLiveStreamResult.Data}");
        //        var classroom = GlobalData.Instance.Classroom;
        //        CourseLiveStream live = new CourseLiveStream()
        //        {
        //            ClassRoomImie = classroom.SchoolRoomImei,
        //            ClassRoomName = classroom.SchoolRoomName,
        //            LiveStreamBeginTime = DateTime.Now.ToString(),
        //            LiveStreamUrl = classroom.PlayStreamUrl

        //        };
        //        if (GlobalData.Instance.Course != null && GlobalData.Instance.Course.Id > 0)
        //        {
        //            var course = GlobalData.Instance.Course;
        //            live.CurriculumName = course.CurriculumName;
        //            live.LiveStreamBeginTime = course.CourseStartTime;
        //            live.GradeName = course.GradeName;
        //        }
        //        else
        //        {
        //            live.GradeName = string.Empty;
        //            live.CurriculumName = string.Empty;
        //        }
        //        //注册直播
        //        RegisterLive(live);
        //    }

        //    return startLiveStreamResult;
        //}

        //public async Task<AsyncCallbackMsg> RefreshLiveStream(List<LiveVideoStream> openedStreamInfos)
        //{
        //    if (LiveId != 0)
        //    {
        //        Log.Logger.Debug($"【server refresh live begins】：liveId={LiveId}, videos={openedStreamInfos.Count}");
        //        for (int i = 0; i < openedStreamInfos.Count; i++)
        //        {
        //            Log.Logger.Debug(
        //                $"video{i + 1}：x={openedStreamInfos[i].X}, y={openedStreamInfos[i].Y}, width={openedStreamInfos[i].Width}, height={openedStreamInfos[i].Height}");
        //        }

        //        AsyncCallbackMsg updateAsynCallResult =
                    
        //                _meetingService.UpdateLiveVideoStreams(LiveId, openedStreamInfos.ToArray(),
        //                    openedStreamInfos.Count);
        //        Log.Logger.Debug(
        //            $"【server refresh live result】：hasError={updateAsynCallResult.HasError}, msg={updateAsynCallResult.Message}");
        //        return updateAsynCallResult;
        //    }
        //    return AsyncCallbackMsg.GenerateMsg(MessageManager.NoLiveToRefresh);
        //}

        //public async Task<AsyncCallbackMsg> StopPushLiveStream()
        //{
        //    if (LiveId != 0)
        //    {
        //        Log.Logger.Debug($"【server push live stop begins】：liveId={LiveId}");
        //        AsyncCallbackMsg stopAsynCallResult = await _meetingService.StopLiveStream(LiveId);
        //        LiveId = 0;

        //        Log.Logger.Debug(
        //            $"【server push live stop result】：hasError={stopAsynCallResult.HasError}, msg={stopAsynCallResult.Message}");

        //        return stopAsynCallResult;
        //    }
        //    return AsyncCallbackMsg.GenerateMsg(MessageManager.NoLiveToStop);
        //}


        private void RegisterLive(CourseLiveStream live)
        {
            try
            {
                var _config = _localDataManager.GetSettingConfigData();
                string ip = string.Format("{0}:{1}", _config.ServerInfo.ServerIp, _config.ServerInfo.BmsServerPort);
                if (_config == null) return;
                string data = JsonConvert.SerializeObject(live);
                string url = string.Format("http://{0}/{1}", ip, "SupperSchool/RegisterLive");
                Log.Logger.Debug($"【调用接口RegisterLive】：: url={url},参数:{data}");
                string response = HttpManager.HttpPostData(url, data);
                Log.Logger.Debug($"【调用接口RegisterLive返回数据:{response}】");
                if (string.IsNullOrEmpty(response)) return;
                var temp = JsonConvert.DeserializeObject(response, typeof(ReturnMessage)) as ReturnMessage;
                if (temp.Status == "0") return;
                else return;
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"【调用接口RegisterLive异常：{ex.Message}】");
            }
        }

    }
}
