using MeetingSdk.NetAgent.Models;

namespace Common.Contract
{
    public interface IPushServerLive
    {
        void ResetStatus();

        bool HasPushLiveSuccessfully { get; set; }

        int LiveId { get; }

        PublishLiveStreamParameter LiveParam { get; }

        PublishLiveStreamParameter GetLiveParam();

        //Task<AsyncCallbackMsg> StartPushLiveStream(List<LiveVideoStream> liveVideoStreamInfos,
        //    string pushLiveUrl = "");

        //Task<AsyncCallbackMsg> RefreshLiveStream(List<LiveVideoStream> liveVideoStreamInfos);
        //Task<AsyncCallbackMsg> StopPushLiveStream();
    }
}
