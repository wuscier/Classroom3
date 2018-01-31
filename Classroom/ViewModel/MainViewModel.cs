using System.Windows.Controls;
using System.Windows.Input;
using Classroom.Model;
using Classroom.View;
using Common.Contract;
using Common.Helper;
using Common.UiMessage;
using Prism.Commands;

namespace Classroom.ViewModel
{
    public class MainViewModel
    {
        private readonly MainView _mainView;
        private readonly IBms _classroomBms;
        private readonly string _msg;

        public MainViewModel(MainView mainView, string msg = "")
        {
            _msg = msg;

            _mainView = mainView;
            _classroomBms = DependencyResolver.Current.GetService<IBms>();

            GotoCreateClassCommand = new DelegateCommand(GotoCreateClass);
            GotoSettingCommand = new DelegateCommand(GotoSetting);
            GotoTimetableCommand = new DelegateCommand(GotoTimetable);
            GotoClassroomCommand = new DelegateCommand(GotoClassroom);
            GotoJoinClassCommand = new DelegateCommand(GotoJoinClass);

            MainViewLoadedCommand = new DelegateCommand(MainViewLoadedAsync);
            InitMenus();
        }

        private void MainViewLoadedAsync()
        {
            switch (GlobalData.Instance.CurrentHomeMenu)
            {
                case MainMenuNames.Setting:
                    var setting = _mainView.Setting.Content as Button;
                    setting?.Focus();
                    break;
                case MainMenuNames.Classrooms:
                    var classrooms = _mainView.Classrooms.Content as Button;
                    classrooms?.Focus();
                    break;
                case MainMenuNames.JoinClass:
                    var joinclass = _mainView.JoinClass.Content as Button;
                    joinclass?.Focus();
                    break;
                case MainMenuNames.CreateClass:
                    var createclass = _mainView.CreateClass.Content as Button;
                    createclass?.Focus();
                    break;
                case MainMenuNames.Timetable:
                    var timetable = _mainView.Timetable.Content as Button;
                    timetable?.Focus();
                    break;
                default:
                    var joinclass2 = _mainView.JoinClass.Content as Button;
                    joinclass2?.Focus();
                    break;
            }
            if (!string.IsNullOrEmpty(_msg))
            {
                MessageQueueManager.Instance.AddInfo(_msg);
            }

            //await _classroomBms.GetClassroomsAsync();
        }

        private void GotoCreateClass()
        {
            GlobalData.Instance.CurrentHomeMenu = MainMenuNames.CreateClass;

            CreateClassView createClassView = new CreateClassView();
            createClassView.Show();

            _mainView.Close();
        }

        private void GotoSetting()
        {
            GlobalData.Instance.CurrentHomeMenu = MainMenuNames.Setting;
            var view = new SettingNavView();
            view.Show();
            _mainView.Close();
        }

        private async void GotoTimetable()
        {
            GlobalData.Instance.CurrentHomeMenu = MainMenuNames.Timetable;

            var classTable = await _classroomBms.GetClassTableInfoAsync(GlobalData.Instance.Classroom?.Id);

            ClassScheduleModel.DoUpdateCurriculumMeetingN0(classTable);

            var view = new ClassScheduleView();
            view.Show();
            _mainView.Close();
        }

        private void GotoClassroom()
        {
            GlobalData.Instance.CurrentHomeMenu = MainMenuNames.Classrooms;

            var view = new ClassListView();
            view.Show();
            _mainView.Close();
        }

        private void GotoJoinClass()
        {
            GlobalData.Instance.CurrentHomeMenu = MainMenuNames.JoinClass;

            var view = new JoinClassView();
            view.Show();
            _mainView.Close();
        }

        public ICommand GotoCreateClassCommand { get; set; }
        public ICommand GotoSettingCommand { get; set; }

        public ICommand GotoTimetableCommand { get; set; }

        public ICommand GotoClassroomCommand { get; set; }

        public ICommand GotoJoinClassCommand { get; set; }



        public ICommand MainViewLoadedCommand { get; set; }



        private void InitMenus()
        {
            TimetableMenu = new MainMenu()
            {
                FocusedImageUrl = "/Common;Component/Image/index_btn_1_bg.png",
                GotoCommand = GotoTimetableCommand,
                ImageUrl = "/Common;Component/Image/index_btn_1.png",
                MenuName = "课表"
            };

            CreateClassMenu = new MainMenu()
            {
                FocusedImageUrl = "/Common;Component/Image/index_btn_2_bg.png",
                GotoCommand = GotoCreateClassCommand,
                ImageUrl = "/Common;Component/Image/index_btn_2.png",
                MenuName = "创建课堂"
            };

            JoinClassMenu = new MainMenu()
            {
                FocusedImageUrl = "/Common;Component/Image/index_btn_3_bg.png",
                GotoCommand = GotoJoinClassCommand,
                ImageUrl = "/Common;Component/Image/index_btn_3.png",
                MenuName = "加入课堂"
            };

            ClassroomsMenu = new MainMenu()
            {
                FocusedImageUrl = "/Common;Component/Image/index_btn_4_bg.png",
                GotoCommand = GotoClassroomCommand,
                ImageUrl = "/Common;Component/Image/index_btn_4.png",
                MenuName = "教室列表"
            };

            SettingMenu = new MainMenu()
            {
                FocusedImageUrl = "/Common;Component/Image/index_btn_5_bg.png",
                GotoCommand = GotoSettingCommand,
                ImageUrl = "/Common;Component/Image/index_btn_5.png",
                MenuName = "设置"
            };

        }

        public MainMenu TimetableMenu { get; set; }
        public MainMenu CreateClassMenu { get; set; }
        public MainMenu JoinClassMenu { get; set; }
        public MainMenu ClassroomsMenu { get; set; }
        public MainMenu SettingMenu { get; set; }
    }
}
