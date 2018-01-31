using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WindowsInput.Native;
using Common.Contract;
using Common.Helper;
using Common.UiMessage;
using Prism.Commands;
using Serilog;
using Classroom.Model;
using MeetingSdk.NetAgent;

namespace Classroom.ViewModel
{
    public class InviteAttendeeViewModel
    {
        private readonly IBms _bmsService;
        private readonly IMeetingSdkAgent _meetingService;

        public InviteAttendeeViewModel()
        {
            _bmsService = DependencyResolver.Current.GetService<IBms>();
            _meetingService = DependencyResolver.Current.GetService<IMeetingSdkAgent>();
            SendInvitationItems = new ObservableCollection<TextWithButtonItem>();

            GetInviteesCommand = DelegateCommand.FromAsyncHandler(GetInviteesAsync);
            SendInvitationCommand = DelegateCommand<TextWithButtonItem>.FromAsyncHandler(SendInvitationAsync);
        }

        private async Task SendInvitationAsync(TextWithButtonItem arg)
        {
            try
            {
                var invitName = GlobalData.Instance.Classrooms.FirstOrDefault(classroom => classroom.SchoolRoomNum == arg.Id)?.SchoolRoomName;
                //执行邀请方法

                object meetingIdObj = AppCache.TryGet(CacheKey.MeetingId);

                int meetingId;
                if (meetingIdObj != null && int.TryParse(meetingIdObj.ToString(), out meetingId))
                {
                    int[] invitees = new int[] { int.Parse(arg.Id) };
                    var result = await _meetingService.MeetingInvite(meetingId, invitees);

                    if (result.StatusCode != 0)
                    {
                        Log.Logger.Error($"邀请失败信息：{result.Message}");
                        MessageQueueManager.Instance.AddError(string.IsNullOrEmpty(result.Message) ? "邀请失败！" : result.Message);
                    }
                    else
                    {
                        arg.Content = "已邀请";
                        arg.ButtonBackground = new SolidColorBrush(Colors.White);
                        arg.ButtonForeground = (SolidColorBrush)Application.Current.Resources["ThemeBrush"];
                        arg.ButtonVisibility = Visibility.Visible;

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"邀请发生异常 exception：{ex}");
                MessageQueueManager.Instance.AddError("邀请失败！");
            }

        }

        private async Task GetInviteesAsync()
        {
            List<Common.Model.Classroom> classrooms = await _bmsService.GetClassroomsAsync();

            var invitees = from invitee in classrooms
                           select new TextWithButtonItem()
                           {
                               Id = invitee.SchoolRoomNum,
                               Text = invitee.SchoolRoomName + $" [{invitee.SchoolRoomNum}]",
                               ButtonCommand = SendInvitationCommand,
                               Content = "邀 请",
                               ButtonForeground = new SolidColorBrush(Colors.White),
                               ButtonBackground = (SolidColorBrush)Application.Current.Resources["ThemeBrush"],
                               ButtonVisibility = Visibility.Collapsed
                           };

            invitees.ToList().ForEach(invitee =>
            {
                SendInvitationItems.Add(invitee);
            });

            InputSimulatorManager.Instance.InputSimu.Keyboard.KeyPress(VirtualKeyCode.TAB);
        }

        public ObservableCollection<TextWithButtonItem> SendInvitationItems { get; set; }
        public ICommand GetInviteesCommand { get; set; }
        public ICommand SendInvitationCommand { get; set; }
    }
}
