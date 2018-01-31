using System;
using Common.Contract;
using Common.Helper;
using Common.Model;

namespace Service
{
    public class ClassScheduleService : IClassScheduleService
    {

        private readonly IBms _bmsService;

        public ClassScheduleService()
        {
            _bmsService = DependencyResolver.Current.GetService<IBms>();
        }

        public int ReserveClass(Course course)
        {

            var courseDateTime = Convert.ToDateTime(course.CourseStartTime).AddMinutes(-10);
            var now = DateTime.Now;
            //提前创建预约会议时间
            var dateTime = new DateTime(now.Year, now.Month, now.Day, courseDateTime.Hour, courseDateTime.Minute, courseDateTime.Second);
            var courseTimeSpan = courseDateTime.TimeOfDay;
            var tsStart = now.TimeOfDay - courseTimeSpan;
            var tsEnd = Convert.ToDateTime(course.CoursEendTime).TimeOfDay - courseTimeSpan;
            if (!(tsStart.TotalMinutes > 0) || !(tsStart.TotalMinutes <= tsEnd.TotalMinutes)) return -1;
            course.CourseStartTime = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            //更新课堂号
            _bmsService.UpdateMeetingIdOfCourseAsync(course);
            if (!(tsStart.TotalMinutes <= 10)) return -1;
            var beginMin = 10 - (int)tsStart.TotalMinutes;
            return beginMin;

        }

    }
}
