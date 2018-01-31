using Common.Contract;
using System;
using Common.Helper;
using Serilog;
using Newtonsoft.Json;

namespace Service
{
    public class RemoteRecordService : IRemoteRecord
    {
        #region field

        private readonly ILocalDataManager _localDataManager;
       
        #endregion

        #region property

        public bool IsRemoteRecord
        {
            get; set;
        }

        #endregion
        #region ctor
        public RemoteRecordService()
        {
            _localDataManager = DependencyResolver.Current.GetService<ILocalDataManager>();
        }
        #endregion

        #region method

        public bool AddRecord()
        {
            try
            {
                var course = GlobalData.Instance.Course;
                if (course == null) return false;
                var config = _localDataManager.GetSettingConfigData();
                var ip = $"{config.ServerInfo.ServerIp}:{config.ServerInfo.BmsServerPort}";
                var fileOutId = $"{course.MeetingId}_{DateTime.Now:yyyymmddhhmmss}";
                var data =
                    $"{{\"classroomid\":\"{GlobalData.Instance.Classroom.Id}\",\"curriculumId\":\"{course.Id}\",\"classno\":\"{course.MeetingId}\",\"recordfilecid\":\"{fileOutId}\"}}";
                var url = $"http://{ip}/SupperSchool/startrecord";
                Log.Logger.Debug($"调用接口startrecord: url={url}");
                var response = HttpManager.HttpPostData(url, data);
                Log.Logger.Debug($"调用接口startrecord返回数据:{url},参数:{data}");
                if (string.IsNullOrEmpty(response)) return false;
                var temp = JsonConvert.DeserializeObject(response, typeof(ReturnMessage)) as ReturnMessage;
                return temp != null && temp.Status == "0";

            }
            catch (Exception ex)
            {
                Log.Logger.Error($"调用接口startrecord异常:{ex.Message}");
                return false;
            }
        }


        public bool StopRecord()
        {
            try
            {
                var config = _localDataManager.GetSettingConfigData();
                var ip = $"{config.ServerInfo.ServerIp}:{config.ServerInfo.BmsServerPort}";
                if (GlobalData.Instance.Classroom == null) return false;
                var data = $"{{\"classroomid\":\"{GlobalData.Instance.Classroom.Id}\"}}";
                var url = $"http://{ip}/SupperSchool/stoprecord";
                Log.Logger.Debug($"调用接口startrecord返回数据:{url},参数:{data}");
                var response = HttpManager.HttpPostData(url, data);
                Log.Logger.Debug($"调用接口stoprecord返回数据:{response}");
                if (string.IsNullOrEmpty(response)) return false;
                var temp = JsonConvert.DeserializeObject(response, typeof(ReturnMessage)) as ReturnMessage;
                return temp != null && temp.Status == "0";
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"调用接口stoprecord异常:{ex.Message}");
                return false;
            }

        }



        #endregion

        #region MyRegion

        #endregion







    }
}
