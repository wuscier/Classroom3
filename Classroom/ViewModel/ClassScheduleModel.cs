using Classroom.View;
using Common.Contract;
using Common.Helper;
using Common.Model;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WindowsInput;
using Prism.Mvvm;
using WindowsInput.Native;
using Common.UiMessage;
using Serilog;
using Classroom.Service;

namespace Classroom.ViewModel
{
    public class ClassScheduleModel : BindableBase
    {
        #region field

        private readonly ClassScheduleView _scheduleView;
        private readonly IBms _bmsService;
        private ToolTip _toolTip;
        private readonly InputSimulator _s;
        //private readonly IClassScheduleService _classScheduleService;

        #endregion

        #region property

        public ToolTip ToolTip
        {
            get { return _toolTip; }
            set { SetProperty(ref _toolTip, value); }
        }

        public List<Course> CourseList { get; set; }

        public ObservableCollection<CourseViewModel> CourseViewList { get; set; }

        #endregion


        #region ctor

        public ClassScheduleModel(ClassScheduleView view)
        {
            _s = new InputSimulator();
            //_classScheduleService = DependencyResolver.Current.GetService<IClassScheduleService>();
            CourseViewList = new ObservableCollection<CourseViewModel>();
            CourseList = new List<Course>();
            _bmsService = DependencyResolver.Current.GetService<IBms>();
            _scheduleView = view;
            LoadCommand = DelegateCommand.FromAsyncHandler(LoadingAsync);
            GoBackCommand = new DelegateCommand(GoBack);


        }

        #endregion

        #region method

        private void GoBack()
        {
            var mainView = new MainView();
            mainView.Show();
            _scheduleView.Close();
        }

        private async Task LoadingAsync()
        {
            try
            {
                //1.查询课表
                //2.检查是否有临近的课，如果有去预约会议号
                //3.获取列表数据
                var classroomId = GlobalData.Instance.Classroom.Id;
                var classTable = await _bmsService.GetClassTableInfoAsync(classroomId);
                //DoUpdateCurriculumMeetingN0(classTable);
                GetWeekCourse(classTable);
                if (!CourseList.Any(o => o.IsProcessing))
                {
                    _s.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    _s.Keyboard.KeyPress(VirtualKeyCode.TAB);
                }
                DeviceSettingsChecker.Instance.IsVideoAudioSettingsValid(_scheduleView);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"课表加载信息发生异常 exception：{ex}");
                MessageQueueManager.Instance.AddError(MessageManager.LoadingError);
            }

        }

        /// <summary>
        /// 获取一周课表
        /// </summary>
        /// <param name="classTable"></param>
        private void GetWeekCourse(ClassTable classTable)
        {
            if (classTable == null || classTable.ClassTableItems == null)
            {
                return;
            }

            CourseViewList.Clear();
            CourseList.Clear();
            for (var i = 1; i < 6; i++)
            {
                for (var j = 1; j < 9; j++)
                {
                    var course = new Course()
                    {
                        CurriculumNumber = j,
                        WeekId = i,
                        CourseName = "-"
                    };
                    var exsitCourse =
                        classTable.ClassTableItems.FirstOrDefault(o => o.CurriculumNumber == j && o.WeekId == i);
                    CourseList.Add(exsitCourse ?? course);
                }
            }

            for (var i = 1; i < 9; i++)
            {
                var cvm = GetCourseView(i);
                CourseViewList.Add(cvm);
            }
        }


        private CourseViewModel GetCourseView(int curriculumNumber)
        {
            var cvm = new CourseViewModel();
            CourseList.Where(o => o.CurriculumNumber == curriculumNumber).ToList().ForEach(c =>
            {
                var newcourse = new Course() { CourseName = c.CourseName, CourseId = c.CourseId, MeetingId = c.MeetingId };
                var isInclass = IsInClass(c);
                var course = c;
                var courseTipView = new CourseTipView(_scheduleView, course);
                newcourse.IsProcessing = isInclass;
                if (isInclass)
                {
                    newcourse.CourseName = c.CourseName + "\r\n(正在上课)";
                    courseTipView.intoBtn.Visibility = Visibility.Visible;

                }

                switch (c.WeekId)
                {
                    case 1:
                        cvm.MondayCourse = newcourse;
                        if (course?.CourseId > 0)
                            cvm.MondayCourse.ToolTip = new ToolTip() { Content = courseTipView, Background = null, BorderBrush = null };
                        break;
                    case 2:
                        cvm.TuesdayCourse = newcourse;
                        if (course?.CourseId > 0)
                            cvm.TuesdayCourse.ToolTip = new ToolTip() { Content = courseTipView, Background = null, BorderBrush = null };
                        break;
                    case 3:
                        cvm.WednesdayCourse = newcourse;
                        if (course?.CourseId > 0)
                            cvm.WednesdayCourse.ToolTip = new ToolTip() { Content = courseTipView, Background = null, BorderBrush = null };
                        break;
                    case 4:
                        cvm.ThursdayCourse = newcourse;
                        if (course?.CourseId > 0)
                            cvm.ThursdayCourse.ToolTip = new ToolTip() { Content = courseTipView, Background = null, BorderBrush = null };
                        break;
                    case 5:
                        cvm.FridayCourse = newcourse;
                        if (course?.CourseId > 0)
                            cvm.FridayCourse.ToolTip = new ToolTip() { Content = courseTipView, Background = null, BorderBrush = null };
                        break;
                }

            });
            return cvm;
        }

        public static void DoUpdateCurriculumMeetingN0(ClassTable classTable)
        {
            if (classTable.ClassTableItems == null || !classTable.ClassTableItems.Any()) return;
            var dayCourseList =
                classTable.ClassTableItems.Where(
                    o => o.WeekId.ToString() == DateTime.Now.DayOfWeek.ToString("d")).ToList();
            if (!dayCourseList.Any()) return;

            IClassScheduleService _classScheduleService = DependencyResolver.Current.GetService<IClassScheduleService>();

            dayCourseList.ForEach(dc =>
            {
                var beginMin = _classScheduleService.ReserveClass(dc);
                if (beginMin < 0) return;
                //此处弹出通知
                var message = $"{dc.CourseName}将在{beginMin}分钟后开始";
                ////弹出通知信息
                MessageQueueManager.Instance.AddInfo(message);
            });
        }

        private bool IsInClass(Course course)
        {
            var weekName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(DateTime.Now.DayOfWeek);
            if (weekName != course.WeekName) return false;
            var courseStartTime = Convert.ToDateTime(course.CourseStartTime).AddMinutes(-10).TimeOfDay;
            var coursEendTime = Convert.ToDateTime(course.CoursEendTime).TimeOfDay;
            var startTimeSpan = DateTime.Now.TimeOfDay - courseStartTime;
            var endTimeSpan = coursEendTime - courseStartTime;
            return course.MeetingId != 0 && startTimeSpan.TotalMinutes > 0 && startTimeSpan.TotalMinutes <= endTimeSpan.TotalMinutes;
        }

        #endregion

        #region command

        public ICommand LoadCommand { get; set; }

        public ICommand WindowKeyDownCommand { get; set; }

        public ICommand GoBackCommand { get; set; }

        #endregion





    }
}
