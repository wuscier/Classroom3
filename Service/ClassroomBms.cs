using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Contract;
using Common.Helper;
using Common.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using Serilog;

namespace Service
{
    public class ClassroomBms : IBms
    {
        public async Task<ReturnMessage> GetClassroomAsync(string imei)
        {
            string requestUrl = $@"SupperSchool/AuthenticationClassRoom?classRoomImei={imei}";

            ReturnMessage bmsMessage = await Request(requestUrl);

            if (!bmsMessage.HasError)
            {
                JObject jObject = JObject.Parse(bmsMessage.Data.ToString());
                string sClassroom = jObject.SelectToken("classroom").ToString();

                Classroom classroom = JsonConvert.DeserializeObject<Classroom>(sClassroom);

                bmsMessage.Data = classroom;
            }

            return bmsMessage;
        }

        public async Task<List<Classroom>> GetClassroomsAsync()
        {
            var url = "SupperSchool/GetClassList";
            var requestResult = await HttpPostData(url);
            List<Classroom> classrooms = requestResult.Status != "0"
                ? new List<Classroom>()
                : JsonConvert.DeserializeObject<List<Classroom>>(requestResult.Data.ToString());

            if (classrooms == null || classrooms.Count <= 0) return classrooms;
            classrooms.ForEach(o =>
            {
                o.CreateTime= Convert.ToDateTime(o.CreateTime).ToString("yyyy-MM-dd HH:mm:ss");
            });
            GlobalData.Instance.Classrooms = classrooms;
            return classrooms;
        }

        public async Task<ClassTable> GetClassTableInfoAsync(string classroomId)
        {
            const string url = "SupperSchool/getcourselist";
            var json = new JObject
            {
                {"classroomId", classroomId}
            };

            var content = new StringContent(json.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var result = await HttpPostData(url, content);
            return result.Status == "0" ? JsonConvert.DeserializeObject<ClassTable>(result.Data.ToString()) : new ClassTable();
        }

        public Task<ReturnMessage> GetLogoAsync()
        {
            return null;
        }

        public Task<ReturnMessage> RegisterLiveStreamAsync(CourseLiveStream courseLiveStream)
        {
            return null;
        }

        public Task<ReturnMessage> SendLiveStreamStatusAsync()
        {
            return null;
        }

        public Task<ReturnMessage> StartServerRecordAsync(Course course)
        {
            return null;
        }

        public Task<ReturnMessage> StopServerRecordAsync(Course course)
        {
            return null;
        }

        public async Task<ReturnMessage> UpdateMeetingIdOfCourseAsync(Course course)
        {
            const string url = "SupperSchool/UpdateCurriculumMeetingN0";
            var utcTime = CommonHelp.ConvertDateTimeInt(course.CourseStartTime).ToString();
            var json = new JObject
            {
                {"CurriculumId", course.Id},
                {"ClassRoomimie", course.MainClassroomId},
                {"MeetingId", course.MeetingId},
                {"beginDateTime", utcTime}
            };
            var content = new StringContent(json.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var result = await Request(url, content);
            return null;
        }


        public async Task<bool> CheckConnection(string baseUrl)
        {
            const string url = "SupperSchool/CheckConnect";
            var reuslt = await HttpPostData(url, null, baseUrl);
            return reuslt.Status == "0";
        }

        private async Task<ReturnMessage> Request(string requestUrl, HttpContent content = null)
        {
            using (var httpClient = new HttpClient())
            {
                var serverAddress =
                    $"http://{GlobalData.Instance.ConfigManager.ServerInfo?.ServerIp}:{GlobalData.Instance.ConfigManager.ServerInfo?.BmsServerPort}";
                //var serverAddress = "http://114.112.74.110:81";
                httpClient.BaseAddress = new Uri(serverAddress);

                HttpResponseMessage response;
                ReturnMessage bmsMessage;

                try
                {
                    response = content == null
                        ? await httpClient.GetAsync(requestUrl)
                        : await httpClient.PostAsync(requestUrl, content);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Request exception：{ex}");
                    bmsMessage = ReturnMessage.GenerateError(ex.Message, "-999");
                    return bmsMessage;
                }


                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();

                    Log.Logger.Debug($"HttpResponseMessage Content：{result}");


                    JObject jObject = JObject.Parse(result.ToLower());
                    string status = jObject.SelectToken("status").ToString();
                    string message = jObject.SelectToken("message").ToString();

                    if (status != "0")
                    {
                        bmsMessage = ReturnMessage.GenerateError(message, status);
                    }
                    else
                    {
                        bmsMessage = ReturnMessage.GenerateData(result);
                    }
                }
                else
                {

                    bmsMessage = ReturnMessage.GenerateError(response.ReasonPhrase,
                        response.StatusCode.ToString());
                }

                return bmsMessage;
            }
        }

        /// <summary>
        /// api返回结果即为结果，不是标准的带reusltcode,data,message
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private async Task<ReturnMessage> HttpPostData(string requestUrl, HttpContent content = null, string baseUrl = "")
        {
            using (var httpClient = new HttpClient())
            {
                if (string.IsNullOrEmpty(baseUrl)) baseUrl = $"http://{GlobalData.Instance.ConfigManager.ServerInfo.ServerIp}:{GlobalData.Instance.ConfigManager.ServerInfo.BmsServerPort}";
                httpClient.BaseAddress = new Uri(baseUrl);
                HttpResponseMessage response;
                ReturnMessage bmsMessage;
                try
                {
                    response = await httpClient.PostAsync(requestUrl, content);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"HttpPostData exception：{ex}");

                    bmsMessage = ReturnMessage.GenerateError(ex.Message);
                    return bmsMessage;
                }
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();

                    Log.Logger.Debug($"HttpResponseMessage Content：{result}");

                    bmsMessage = ReturnMessage.GenerateData(result);
                }
                else
                {
                    bmsMessage = ReturnMessage.GenerateError(response.ReasonPhrase,
                        response.StatusCode.ToString());
                }

                return bmsMessage;
            }
        }


    }
}
