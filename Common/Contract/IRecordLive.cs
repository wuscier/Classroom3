using MeetingSdk.NetAgent.Models;
using MeetingSdk.NetAgent;

namespace Common.Contract
{
    public interface IRecordLive
    {
        int RecordId { get; }
        string RecordDirectory { get; }
        PublishLiveStreamParameter RecordParam { get; }

        void ResetStatus();

        bool GetRecordParam();

        MeetingResult RefreshLiveStream(VideoStreamModel[] videoStreamModels, AudioStreamModel[] audioStreamModels);
        MeetingResult StartMp4Record(VideoStreamModel[] videoStreamModels, AudioStreamModel[] audioStreamModels);
        MeetingResult StopMp4Record();

    }
}
