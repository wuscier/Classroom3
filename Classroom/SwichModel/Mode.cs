using Classroom.SwichModel.BaseClass;
using System;
using System.Data;
using System.Linq;
using Common.Contract;
using Common.Helper;
using Common.UiMessage;
using System.Windows;
using Classroom.View;
using Common.CustomControl;
using Common.Model;
using MeetingSdk.NetAgent;
using MeetingSdk.Wpf;

namespace Classroom.SwichModel
{
    public class Mode : PropertyChangedBase
    {

        #region field

        private readonly IMeetingSdkAgent _meetingSdkAgent;
        private readonly IMeetingWindowManager _windowManager;
        private readonly IClassScheduleService _classScheduleService;

        #endregion

        #region ctor

        public Mode()
        {
            _meetingSdkAgent = DependencyResolver.Current.GetService<IMeetingSdkAgent>();
            _windowManager = DependencyResolver.Current.GetService<IMeetingWindowManager>();
            _classScheduleService = DependencyResolver.Current.GetService<IClassScheduleService>();
        }
        #endregion

        /// <summary>
        /// 模式名称
        /// </summary>
        public string Name { get; set; }

        private bool _receiveCommand;
        /// <summary>
        /// 该模式下是否能够接受命令 
        /// </summary>
        public bool ReceiveCommand
        {
            get
            {
                return _receiveCommand;
            }
            set
            {
                if (_receiveCommand != value)
                {
                    _receiveCommand = value;
                    this.OnPropertyChanged(p => p.ReceiveCommand);
                }
            }
        }

        public void InitializationInAutoMode()
        {
            SerialPortCommunicator.Instance.InitDefaultSerialPort();

            CommandController.Instance.JoinMeeting += JoinMeeting;
            CommandController.Instance.ExitMeeting += ExitMeeting;

            CommandController.Instance.SetSpeakerMode += SetSpeakerMode;
            CommandController.Instance.SetCourseMode += SetCourseMode;
            CommandController.Instance.SetInteractionMode += SetInteractionMode;
            CommandController.Instance.OpenDocument += OpenDocument;
            CommandController.Instance.CloseDocument += CloseDocument;

            CommandController.Instance.SetAutoLayout += SetAutoLayout;
            CommandController.Instance.SetFlatLayout += SetFlatLayout;
            CommandController.Instance.SetPictureLayout += SetPictureLayout;
            CommandController.Instance.SetFeatureLayout += SetFeatureLayout;
        }

        private void SetFeatureLayout()
        {
            // BigSmalls 一大多小画面布局
            if (_windowManager.LayoutChange(WindowNames.MainWindow, LayoutRenderType.BigSmallsLayout))
            {
            }
            //if (_windowManager.LayoutChange(WindowNames.ExtendedWindow, LayoutRenderType.BigSmallsLayout))
            //{
            //}
        }

        private void SetPictureLayout()
        {
            // Closeup 特写画面布局
            if (_windowManager.LayoutChange(WindowNames.MainWindow, LayoutRenderType.CloseupLayout))
            {
            }
            //if (_windowManager.LayoutChange(WindowNames.ExtendedWindow, LayoutRenderType.CloseupLayout))
            //{
            //}

        }

        private void SetFlatLayout()
        {
            // Average 平均排列画面布局
            if (_windowManager.LayoutChange(WindowNames.MainWindow, LayoutRenderType.CloseupLayout))
            {
            }
            //if (_windowManager.LayoutChange(WindowNames.ExtendedWindow, LayoutRenderType.CloseupLayout))
            //{
            //}

        }

        private void SetAutoLayout()
        {
            // AutoLayout 自动
            if (_windowManager.LayoutChange(WindowNames.MainWindow, LayoutRenderType.AutoLayout))
            {
            }
            if (_windowManager.LayoutChange(WindowNames.MainWindow, LayoutRenderType.AutoLayout))
            {
            }

        }

        //关闭文档
        private async void CloseDocument()
        {
            //var message = await _meetingService.StopShareDoc();
            //if (message.HasError)
            //{
            //    MessageQueueManager.Instance.AddError(message.Message);
            //}
        }

        //打开文档
        private async void OpenDocument()
        {
            //var startShareMessage = await _meetingService.StartShareDoc();
            //if (startShareMessage.HasError)
            //{
            //    MessageQueueManager.Instance.AddError(startShareMessage.Message);
            //}
        }

        private void SetInteractionMode()
        {
            if (_windowManager.ModeChange(ModeDisplayerType.InteractionMode))
            {
            }
        }

        private void SetCourseMode()
        {
            // 共享模式
            if (_windowManager.ModeChange(ModeDisplayerType.ShareMode))
            {
            }
        }

        private void SetSpeakerMode()
        {
            //主讲模式
            if (_windowManager.ModeChange(ModeDisplayerType.SpeakerMode))
            {
            }
        }

        private void JoinMeeting()
        {
            var currentWindow = Application.Current.Windows;
            var window = currentWindow[0];
            //判断当前是否在课堂中

            if (window is IntoClassView)
            {
                MessageQueueManager.Instance.AddError("已经在课堂中");
                return;
            }

            //预约会议
            var course = DoUpdateCurriculumMeetingN0(GlobalData.Instance.Courses);
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
            window?.Close();
        }


        private Course DoUpdateCurriculumMeetingN0(ClassTable classTable)
        {
            if (!classTable.ClassTableItems.Any()) return null;
            var dayCourseList =
                classTable.ClassTableItems.Where(
                    o => o.WeekId.ToString() == DateTime.Now.DayOfWeek.ToString("d")).ToList();
            if (!dayCourseList.Any()) return null;

            var latestCourse = dayCourseList.OrderBy(o => o.CourseStartTime).FirstOrDefault();
            if (latestCourse == null) return null;
            var result = _classScheduleService.ReserveClass(latestCourse);
            return result > 0 ? latestCourse : null;
        }

        private async void ExitMeeting()
        {
            var currentWindow = Application.Current.Windows;
            var window = currentWindow[0];
            if (!(window is IntoClassView)) return;
            var exitDialog = new Dialog($"您当前正在课堂中，是否要退出当前课堂？", "是", "否");
            var result = exitDialog.ShowDialog();
            if (!result.HasValue || !result.Value) return;
            //退出当前课堂
            var exitMessage = await _meetingSdkAgent.LeaveMeeting();

           await _windowManager.Leave();

            if (exitMessage.StatusCode != 0)
            {
                MessageQueueManager.Instance.AddError(MessageManager.ExitMeetingError);
                return;
            }
            var mainView = new MainView();
            mainView.Show();
        }
    }
}
