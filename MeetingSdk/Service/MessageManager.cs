using Common.Helper;

namespace MeetingSdk.Service
{
    public class MessageManager
    {
        private static string Error = "-1";

        public static ReturnMessage SetDefaultCameraErrorMessage => new ReturnMessage()
        {
            Message = "设置默认摄像头失败！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage ServerNotStartedMessage => new ReturnMessage()
        {
            Message = "服务未启动！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage ServerStartingMessage => new ReturnMessage()
        {
            Message = "服务正在启动中！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage ServerAlreadyStarted => new ReturnMessage()
        {
            Message = "服务已经启动！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage FailedToStartServer => new ReturnMessage()
        {
            Message = "服务启动失败！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage FailedToCreateMeeting => new ReturnMessage()
        {
            Message = "创建课堂失败！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage FailedToJoinMeeting => new ReturnMessage()
        {
            Message = "加入课堂失败！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage FailedToExitMeeting => new ReturnMessage()
        {
            Message = "退出课堂失败！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage FailedToSendUiMessage => new ReturnMessage()
        {
            Message = "发送透传消息失败！",
            HasError = true,
            Status = Error
        };


        public static ReturnMessage FailedToQueryMeeting => new ReturnMessage()
        {
            Message = "查询课堂失败！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage FailedToSpeak => new ReturnMessage()
        {
            Message = "申请发言失败！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage FailedToStopSpeak => new ReturnMessage()
        {
            Message = "取消发言失败！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage FailedToInviteParticipants => new ReturnMessage()
        {
            Message = "邀请参与者失败！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage FailedToStartShareDoc => new ReturnMessage()
        {
            Message = "打开课件失败！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage FailedToStopShareDoc => new ReturnMessage()
        {
            Message = "关闭课件失败！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage FailedToStartLiveStream => new ReturnMessage()
        {
            Message = "开启推流失败！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage FailedToStopLiveStream => new ReturnMessage()
        {
            Message = "停止推流失败！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage FailedToRefreshLiveStream => new ReturnMessage()
        {
            Message = "更新流失败！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage FailedToStartRecord => new ReturnMessage()
        {
            Message = "开启录制失败！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage FailedToStopRecord => new ReturnMessage()
        {
            Message = "停止录制失败！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage FailedToSetRecordParameter => new ReturnMessage()
        {
            Message = "设置录制参数失败！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage FailedToSetRecordDirectory => new ReturnMessage()
        {
            Message = "设置录制路径失败！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage DocVideoNotSet => new ReturnMessage()
        {
            Message = "课件视频采集源未设置！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage DocAudioNotSet => new ReturnMessage()
        {
            Message = "课件音频采集源未设置！",
            HasError = true,
            Status = Error
        };

        //public static ReturnMessage FailedToGetMeetingRecords => new ReturnMessage()
        //{
        //    Message = "获取可参加的课堂列表失败！",
        //    HasError = true,
        //    Status = Error
        //};

        public static ReturnMessage DiskSpaceNotEnough => new ReturnMessage()
        {
            Message = "磁盘空间不足！",
            HasError = true,
            Status = Error
        };



        public static ReturnMessage SpeakerNotSet => new ReturnMessage()
        {
            Message = "音频播放未设置！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage MicrophoneNotSet => new ReturnMessage()
        {
            Message = "音频采集未设置！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage CameraNotSet => new ReturnMessage()
        {
            Message = "视频采集未设置！",
            HasError = true,
            Status = Error
        };


        public static ReturnMessage OpenQosFailure => new ReturnMessage()
        {
            Message = "打开Qos工具失败！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage CloseQosFailure => new ReturnMessage()
        {
            Message = "关闭Qos工具失败！",
            HasError = true,
            Status = Error
        };

        public static ReturnMessage SetDoubleScreen => new ReturnMessage()
        {
            Message = "双屏渲染失败！",
            HasError = true,
            Status = Error
        };
    }
}
