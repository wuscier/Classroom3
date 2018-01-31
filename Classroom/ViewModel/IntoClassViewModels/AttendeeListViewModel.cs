using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using WindowsInput.Native;
using Classroom.Model;
using Common.Helper;
using Prism.Commands;
using MeetingSdk.NetAgent;

namespace Classroom.ViewModel
{
    public class AttendeeListViewModel
    {
        private readonly IMeetingSdkAgent _meetingService;

        public AttendeeListViewModel()
        {
            _meetingService = DependencyResolver.Current.GetService<IMeetingSdkAgent>();

            AttendeeItems = new ObservableCollection<AttendeeItem>();
            GetAttendeesCommand = new DelegateCommand(GetAttendeesAsync);
        }

        private void GetAttendeesAsync()
        {
            var attendees = _meetingService.GetParticipants();

            attendees.Result.ToList().ForEach(attendee =>
            {
                var trueAttendee = GlobalData.Instance.Classrooms.FirstOrDefault(o => o.SchoolRoomNum == attendee.AccountId.ToString());
                AttendeeItems.Add(new AttendeeItem()
                {
                    AttendeeName = $"{trueAttendee?.SchoolRoomName}  [{attendee.AccountId}]"
                });
            });

            InputSimulatorManager.Instance.InputSimu.Keyboard.KeyPress(VirtualKeyCode.TAB);
        }

        public ObservableCollection<AttendeeItem> AttendeeItems { get; set; }
        public ICommand GetAttendeesCommand { get; set; }

        

    }
}
