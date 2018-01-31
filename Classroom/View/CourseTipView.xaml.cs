using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Classroom.ViewModel;
using Common.Helper;
using Common.Model;
using System.Linq;
using Common.Contract;
using MeetingSdk.NetAgent;

namespace Classroom.View
{
    /// <summary>
    /// CourseTipView.xaml 的交互逻辑
    /// </summary>
    public partial class CourseTipView : UserControl
    {
        private readonly IMeetingSdkAgent _meetingService;
        private readonly ClassScheduleView _view;
        private readonly ILocalDataManager _localDataManager;
        public CourseTipView(ClassScheduleView view, Course course)
        {
            _view = view;
            _localDataManager = DependencyResolver.Current.GetService<ILocalDataManager>();
            InitializeComponent();
            _meetingService = DependencyResolver.Current.GetService<IMeetingSdkAgent>();
            if (course.MainClassroomId <= 0) return;
            DataContext = new ClassScheduleTipViewModel(course);
        }

        private async void IntoBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var course = (Course)btn.Tag;

            AppCache.AddOrUpdate(CacheKey.MeetingId, course.MeetingId);

            GlobalData.Instance.Course = course;
            //判断该课堂视讯号与当前登录人的视讯号是否相同，如果相同，设置为主讲
            var mainclassroom = GlobalData.Instance.Classrooms.ToList().FirstOrDefault(o => o.Id == course.MainClassroomId.ToString());
            if (mainclassroom != null)
            {
                AppCache.AddOrUpdate(CacheKey.HostId, mainclassroom.SchoolRoomNum);
            }
            var intoClassView = new IntoClassView(IntoClassType.Join);
            intoClassView.Show();


            var meetingInfoResult = await _meetingService.GetMeetingInfo(course.MeetingId);

            var meetingList = _localDataManager.GetMeetingList() ??
                             new MeetingList() { MeetingInfos = new List<MeetingItem>() };

            var cachedMeeting = meetingList.MeetingInfos.FirstOrDefault(meeting => meeting.MeetingId == course.MeetingId);

            if (cachedMeeting != null)
            {
                cachedMeeting.LastActivityTime = DateTime.Now;
            }
            else
            {
                meetingList.MeetingInfos.Add(new MeetingItem()
                {
                    LastActivityTime = DateTime.Now,
                    MeetingId = course.MeetingId,
                    CreatorName = mainclassroom?.SchoolRoomName,
                    IsClose = false,
                    CreatorId = mainclassroom?.SchoolRoomNum,
                    CreateTime = DateTime.Parse(meetingInfoResult.Result.StartTime),
                });
            }

            _localDataManager.SaveMeetingList(meetingList);
            _view.Close();

        }
    }
}
