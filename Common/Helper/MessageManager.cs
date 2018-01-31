namespace Common.Helper
{
    public class MessageManager
    {
        public const string ErrorGetClassroom = "获取教室信息时异常：";
        public const string CheckServer = "检查服务器设置成功";
        public const string CheckServerError = "检查服务器设置失败";
        public const string CheckDns = "检查成功";
        public const string CheckDnsError = "检查设置失败";
        public const string LoadingError = "加载数据失败";
        public const string SaveError = "保存数据失败";

        public const string WarningBigSmallLayoutNeedsTwoAboveViews = "一大多小布局至少需要两个视图！";
        public const string ExitMeetingError = "退出课堂失败！";


        public const string NoPushLiveUrl = "推流地址为空！";
        public const string PushLiveResolutionNotSet = "推流分辨率或码率未设置！";
        public const string NoLiveToRefresh = "没有可更新的流！";
        public const string NoLiveToStop = "没有可停止的流！";
        public const string RecordDirectoryNotSet = "录制路径未设置！";
        public const string RecordResolutionNotSet = "录制分辨率或码率未设置！";

        public const string NoVideoDevice = "视频设置，人像采集源未设置！";
        public const string NoAudioDevice = "音频设置，人声采集源未设置！";


        public const string ServerStarted = "服务启动成功！";

        #region 加入课堂
        public const string JoinClassError = "加入课堂失败";
        public const string JoinClassNoError = "请输入要加入的课堂号";

        public const string MeetingNoExistError = "该课堂不存在";
        public const string MeetingEndError = "该课堂已结束";
        public const string MeetingIsNotBegin = "该课堂尚未开始";
        public const string MeetingNoError = "无效的课堂号";
        public const string MeetingUserCount = "无效的听课账号";
        public const string MeetingMax = "听课数量达到限制";
        #endregion


        public const string IsTopMost = "启用窗口置顶模式";
        public const string NotTopMost = "取消当前窗口置顶";
    }
}
