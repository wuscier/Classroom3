using System;
using System.IO;
using Common.Contract;
using Common.Helper;
using Common.Model;
using GM.Utilities;
using ConfigManager = Common.Model.ConfigManager;
using Serilog;
using System.Linq;

namespace Service
{
    public class LocalDataManager : ILocalDataManager
    {
        private readonly string _dataFilePath = AppDomain.CurrentDomain.BaseDirectory + "/Data/";
        public SettingParameter GetSettingParameter()
        {
            try
            {
                var filePath = _dataFilePath + "/Parameter.xml";
                if (!File.Exists(filePath)) return null;
                object obj;
                SerializeHelper.Deserialize(filePath, typeof(SettingParameter), out obj);
                var parameter = obj as SettingParameter;
                return parameter;
            }
            catch (Exception ex)
            {
                // Logger.WriteErrorFmt("配置文件", ex, "配置文件处理异常：{0}", ex.Message);
            }
            return null;
        }

        public ConfigManager GetSettingConfigData()
        {
            try
            {
                var filePath = _dataFilePath + "/Config.xml";
                if (!File.Exists(filePath)) return null;
                object obj;
                SerializeHelper.Deserialize(filePath, typeof(ConfigManager), out obj);
                var config = obj as ConfigManager;
                return config;
            }
            catch (Exception ex)
            {
                //Logger.WriteErrorFmt("配置文件", ex, "配置文件处理异常：{0}", ex.Message);
                Log.Logger.Debug($"配置文件处理异常：{ex.Message}");
            }
            return null;
        }

        public void SaveSettingConfigData(ConfigManager config)
        {
            try
            {
                var filePath = _dataFilePath + "/Config.xml";
                SerializeHelper.Serialize(config, typeof(ConfigManager), filePath);
                GlobalData.Instance.ConfigManager = config;
            }
            catch (Exception ex)
            {
                Log.Logger.Debug($"配置文件处理异常：{ex.Message}");
            }
        }

        public MeetingList GetMeetingList()
        {
            try
            {
                var filePath = _dataFilePath + "/MeetingInvitationData.xml";
                if (!File.Exists(filePath)) return null;
                object obj;
                SerializeHelper.Deserialize(filePath, typeof(MeetingList), out obj);
                var config = obj as MeetingList;

                if (config != null)
                {
                    config.MeetingInfos = config.MeetingInfos.OrderByDescending(meetingInfo => meetingInfo.LastActivityTime).Take(6).ToList();
                }

                return config;
            }
            catch (Exception ex)
            {
                Log.Logger.Debug($"课堂列表文件获取异常：{ex.Message}");
            }
            return null;
        }


        public void SaveMeetingList(MeetingList meeting)
        {
            try
            {
                if (meeting == null)
                {
                    throw new ArgumentNullException(nameof(meeting));
                }

                meeting.MeetingInfos = meeting.MeetingInfos.OrderByDescending(meetingInfo => meetingInfo.LastActivityTime).Take(6).ToList();

                var filePath = _dataFilePath + "/MeetingInvitationData.xml";
                SerializeHelper.Serialize(meeting, typeof(MeetingList), filePath);
            }
            catch (Exception ex)
            {
                Logger.WriteErrorFmt("课堂列表文件", ex, "课堂列表文件保存异常：{0}", ex.Message);
            }
        }


        public LocalSetting GetSettingConfig()
        {
            try
            {
                var filePath = _dataFilePath + "/SettingConfig.xml";
                if (!File.Exists(filePath)) return null;
                object obj;
                SerializeHelper.Deserialize(filePath, typeof(LocalSetting), out obj);
                return obj as LocalSetting;
            }
            catch (Exception ex)
            {
                Log.Logger.Debug($"配置文件处理异常：{ex.Message}");
            }
            return new LocalSetting();
        }
    }
}
