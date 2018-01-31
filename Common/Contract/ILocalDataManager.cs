using Common.Model;


namespace Common.Contract
{
    public interface ILocalDataManager
    {
        SettingParameter GetSettingParameter();
        ConfigManager GetSettingConfigData();
        void SaveSettingConfigData(ConfigManager config);
        /// <summary>
        /// 获取会议列表
        /// </summary>
        /// <returns></returns>
        MeetingList GetMeetingList();
        /// <summary>
        /// 保存会议列表
        /// </summary>
        /// <param name="data">列表数据</param>
        void SaveMeetingList(MeetingList data);

        LocalSetting GetSettingConfig();

    }
}
