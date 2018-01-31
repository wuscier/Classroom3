using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Helper;
using Common.Model;

namespace Common.Contract
{
    public interface IBms
    {
        Task<ReturnMessage> GetLogoAsync();
        Task<List<Common.Model.Classroom>> GetClassroomsAsync();
        Task<ReturnMessage> GetClassroomAsync(string imei);
        Task<ClassTable> GetClassTableInfoAsync(string classroomId);
        Task<ReturnMessage> UpdateMeetingIdOfCourseAsync(Course course);
        Task<ReturnMessage> StartServerRecordAsync(Course course);
        Task<ReturnMessage> StopServerRecordAsync(Course course);
        Task<ReturnMessage> RegisterLiveStreamAsync(CourseLiveStream courseLiveStream);
        Task<ReturnMessage> SendLiveStreamStatusAsync();
        Task<bool> CheckConnection(string baseUrl);
    }
}
