using System.Linq;
using System.Windows.Input;
using Common.Model;
using Prism.Commands;
using Prism.Mvvm;

namespace Classroom.ViewModel
{
    public class ClassScheduleTipViewModel : BindableBase
    {
        #region field

        private string _couseInfo;
        private string _speakerClassroom;
        private string _lecturesClassroom;
        private Course _course;

        #endregion

        #region ctor

        public ClassScheduleTipViewModel(Course course)
        {
            //course.CourseName, course.MainClassRoomName, course.ListenClassroomNames
            CourseD = course;
            var lecturesClassroom = course.ListenClassroomNames;
            var lectureClassroomList = lecturesClassroom.Split(',').ToList();
            CouseInfo = $"({course.CourseName})主讲教室：{course.MainClassRoomName}";

            switch (lectureClassroomList.Count)
            {
                case 0:
                    LecturesClassroom = $"听课教室：";
                    break;
                case 1:
                    LecturesClassroom = $"听课教室：{lecturesClassroom}";
                    break;
                case 2:
                    LecturesClassroom = $"听课教室：{lecturesClassroom[0]},{lecturesClassroom[1]}";
                    break;
                default:
                    LecturesClassroom = $"听课教室：{lecturesClassroom[0]},{lecturesClassroom[1]}等…";
                    break;
            }
            EnterClassCommand = new DelegateCommand(EnterClass);
        }

        #endregion

        #region property

        public Course CourseD
        {
            get { return _course; }
            set { SetProperty(ref _course, value); }
        }

        public string CouseInfo
        {
            get { return _couseInfo; }
            set { SetProperty(ref _couseInfo, value); }
        }

        public string SpeakerClassroom
        {
            get { return _speakerClassroom; }
            set { SetProperty(ref _speakerClassroom, value); }
        }

        public string LecturesClassroom
        {
            get { return _lecturesClassroom; }
            set { SetProperty(ref _lecturesClassroom, value); }
        }

        #endregion

        #region method

        private void EnterClass()
        {
           
        }

        #endregion

        #region command

        public ICommand EnterClassCommand { get; set; }

        #endregion
    }
}
