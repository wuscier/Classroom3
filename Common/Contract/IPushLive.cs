using MeetingSdk.NetAgent.Models;
using MeetingSdk.NetAgent;

namespace Common.Contract
{
    public interface IPushLive
    {
        void ResetStatus();

        bool HasPushLiveSuccessfully { get; set; }

        int LiveId { get; }

        PublishLiveStreamParameter LiveParam { get; }

        bool GetLiveParam();

        MeetingResult StartPushLiveStream(VideoStreamModel[] videoStreamModels, AudioStreamModel[] audioStreamModels,
            string pushLiveUrl = "");

        MeetingResult RefreshLiveStream(VideoStreamModel[] videoStreamModels, AudioStreamModel[] audioStreamModels);
        MeetingResult StopPushLiveStream();

    }
}
