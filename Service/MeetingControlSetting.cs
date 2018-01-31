using GM.Utilities;

namespace Service
{
    public class MeetingControlSetting
    {
        static MeetingControlSetting _instance;

        public static MeetingControlSetting Instance
        {
            get
            {
                if (_instance == null)
                    _instance = SettingManager.Instance.GetSetting<MeetingControlSetting>(true);

                return _instance;
            }
        }

        [XmlSetting("RecordSystemIpAddress", "127.0.0.1")]
        public string RecordSystemIpAddress = "127.0.0.1";

        [XmlSetting("RecordSystemPort", "8642")]
        public int RecordSystemPort = 8642;

        [XmlSetting("ReceiveRecordSystemCmdPort", "8649")]
        public int ReceiveRecordSystemCmdPort = 8649;

        /// <summary>
        /// 是否频繁抢焦点
        /// </summary>
        [XmlSetting("IsGetFocusFrequent", "true")]
        public bool IsGetFocusFrequent = true;


        /// <summary>
        /// 是否改变主讲教室显示 
        /// false为不显主讲教师模式不改变，全为互动模式。
        /// True 主讲模式的课件模式生效
        /// </summary>
        [XmlSetting("IsChangeModel", "false")]
        public bool IsChangeModel = false;

        /// <summary>
        /// 是否改变主讲教室显示 
        /// false为不显主讲教师模式不改变，全为互动模式。
        /// True 主讲模式的课件模式生效
        /// </summary>
        [XmlSetting("IsLunchStart", "true")]
        public bool IsLunchStart = true;
    }
}
