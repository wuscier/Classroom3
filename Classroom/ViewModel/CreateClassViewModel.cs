using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Classroom.View;
using Common.Contract;
using Common.Helper;
using Common.UiMessage;
using Prism.Commands;
using Prism.Mvvm;
using MeetingSdk.NetAgent;
using MeetingSdk.NetAgent.Models;
using MeetingSdk.Wpf;
using Classroom.Service;

namespace Classroom.ViewModel
{
    public class CreateClassViewModel : BindableBase
    {
        private readonly CreateClassView _createClassView;
        private readonly IBms _classroomBms;

        private readonly IMeetingSdkAgent _meetingSdkAgent;
        private readonly IMeetingWindowManager _windowManager;

        public ObservableCollection<ClassroomEx> Classrooms { get; set; }

        public CreateClassViewModel(CreateClassView createClassView)
        {
            _createClassView = createClassView;
            Classrooms = new ObservableCollection<ClassroomEx>();
            _classroomBms = DependencyResolver.Current.GetService<IBms>();
            _meetingSdkAgent = DependencyResolver.Current.GetService<IMeetingSdkAgent>();
            _windowManager = DependencyResolver.Current.GetService<IMeetingWindowManager>();

            GetClassroomsCommand = DelegateCommand.FromAsyncHandler(GetClassroomsAsync);
            CreateClassCommand = new DelegateCommand(CreateClass);
            GoBackCommand = new DelegateCommand(GoBack);
        }

        private void GoBack()
        {
            MainView mainView = new MainView();
            mainView.Show();

            _createClassView.Close();
        }

        private void CreateClass()
        {

            if (GlobalData.Instance.ConfigManager?.AudioInfo?.AudioSammpleDevice == null)
            {
                MessageQueueManager.Instance.AddError(MessageManager.NoAudioDevice);
                return;
            }
            if (string.IsNullOrEmpty(GlobalData.Instance.ConfigManager?.MainVideoInfo?.VideoDevice))
            {
                MessageQueueManager.Instance.AddError(MessageManager.NoVideoDevice);
                return;
            }

            var invitees = from classroom in Classrooms
                           where classroom.Selected
                           select new MeetingSdk.Wpf.Participant(new AccountModel(int.Parse(classroom.Classroom.SchoolRoomNum), classroom.Classroom.SchoolRoomName));

            AppCache.AddOrUpdate(CacheKey.Invitees, invitees);
            AppCache.AddOrUpdate(CacheKey.HostId, _windowManager.Participant.Account.AccountId);

            var intoClassView = new IntoClassView(IntoClassType.Create);
            intoClassView.Show();

            _createClassView.Close();
        }

        private void InviteeKeyDown(ClassroomEx classroomEx)
        {
            classroomEx.Selected = !classroomEx.Selected;
        }

        private async Task GetClassroomsAsync()
        {
            List<global::Common.Model.Classroom> classrooms = await _classroomBms.GetClassroomsAsync();
            classrooms.ForEach(classroom =>
            {
                Classrooms.Add(new ClassroomEx()
                {
                    Classroom = classroom,
                    Selected = false,
                    SelectInviteeCommand = new DelegateCommand<ClassroomEx>(InviteeKeyDown)
                });
            });

            DeviceSettingsChecker.Instance.IsVideoAudioSettingsValid(_createClassView);
        }

        public ICommand GetClassroomsCommand { get; set; }
        public ICommand InviteeKeyDownCommand { get; set; }
        public ICommand CreateClassCommand { get; set; }
        public ICommand GoBackCommand { get; set; }

        private ClassroomEx _selectedClassroomEx;
        public ClassroomEx SelectedClassroomEx
        {
            get { return _selectedClassroomEx; }
            set { SetProperty(ref _selectedClassroomEx, value); }
        }

    }

    public class ClassroomEx : BindableBase
    {
        private global::Common.Model.Classroom _classroom;
        public global::Common.Model.Classroom Classroom
        {
            get { return _classroom; }
            set { SetProperty(ref _classroom, value); }
        }


        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set { SetProperty(ref _selected, value); }
        }

        public ICommand SelectInviteeCommand { get; set; }
    }
}
