using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using WindowsInput.Native;
using Classroom.View;
using Common.Contract;
using Common.Helper;
using Common.Model;
using Common.UiMessage;
using Prism.Commands;
using Prism.Mvvm;
using Serilog;
using MeetingSdk.NetAgent.Models;
using MeetingSdk.NetAgent;

namespace Classroom.ViewModel
{
    public class JoinClassControlViewModel : BindableBase
    {

        #region field

        private string _content;
        private bool _btnEnable;
        private string _btnContent;
        private readonly IMeetingSdkAgent _meetingService;
        private readonly int _meetingId;
        private readonly JoinClassView _view;

        private readonly ILocalDataManager _localDataManager;
        #endregion

        #region property

        public string Content
        {
            get { return _content; }
            set { SetProperty(ref _content, value); }
        }

        public bool BtnEnable
        {
            get { return _btnEnable; }
            set { SetProperty(ref _btnEnable, value); }
        }

        public string BtnContent
        {
            get { return _btnContent; }
            set { SetProperty(ref _btnContent, value); }
        }

        #endregion

        #region ctor

        public JoinClassControlViewModel(JoinClassView view, JoinClassControl jccView, MeetingItem item)
        {
            _view = view;
            _meetingId = item.MeetingId;
            _meetingService = DependencyResolver.Current.GetService<IMeetingSdkAgent>();
            Load(item);
            WindowKeyDownCommand = new DelegateCommand<object>(WindowKeyDownHandler);
            _localDataManager = DependencyResolver.Current.GetService<ILocalDataManager>();
        }

        #endregion

        #region method

        private void Load(MeetingItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Content = $"进入:{item.LastActivityTime} 创建者:{item.CreatorName} 课堂号:{item.MeetingId} 创建:{item.CreateTime}";

            BtnContent = "进入课堂";
            BtnEnable = true;
        }


        private async void WindowKeyDownHandler(object obj)
        {
            try
            {
                var keyEventArgs = obj as KeyEventArgs;
                switch (keyEventArgs?.Key)
                {
                    case Key.Return:
                        if (BtnEnable)
                        {
                            var reuslt = await _meetingService.IsMeetingExist(_meetingId);
                            if (reuslt.StatusCode!=0)
                            {
                                MessageQueueManager.Instance.AddError(MessageManager.MeetingNoExistError);
                                return;
                            }

                            var meetingList = _localDataManager.GetMeetingList() ??
                       new MeetingList() { MeetingInfos = new List<MeetingItem>() };
                            var localMeeting =
                                meetingList.MeetingInfos.FirstOrDefault(o => o.MeetingId == _meetingId);
                            var meeting = GlobalData.Instance.MeetingList?.FirstOrDefault(o => o.MeetingId == _meetingId);
                            if (localMeeting == null && meeting != null)
                            {
                                meetingList.MeetingInfos.Add(new MeetingItem()
                                {
                                    LastActivityTime = DateTime.Now,
                                    MeetingId = _meetingId,
                                    IsClose = false,
                                    CreatorId = meeting.Account.AccountId.ToString(),
                                    CreateTime = DateTime.Parse(meeting.StartTime),
                                    CreatorName = GlobalData.Instance.Classrooms.FirstOrDefault(cls => cls.SchoolRoomNum == meeting.HostId.ToString())?.SchoolRoomName,
                                });

                                AppCache.AddOrUpdate(CacheKey.HostId, meeting.Account.AccountId);
                            }
                            else
                            {
                                if (localMeeting != null)
                                {
                                    localMeeting.LastActivityTime = DateTime.Now;
                                    AppCache.AddOrUpdate(CacheKey.HostId, localMeeting?.CreatorId);
                                }
                            }
                            _localDataManager.SaveMeetingList(meetingList);
                            //此处跳转到课堂窗口
                            AppCache.AddOrUpdate(CacheKey.MeetingId, _meetingId);
                            GlobalData.Instance.Course = new Course();
                            var intoClassView = new IntoClassView(IntoClassType.Join);
                            intoClassView.Show();
                            _view.Close();

                        }
                        keyEventArgs.Handled = true;
                        break;
                    case Key.Left:
                        keyEventArgs.Handled = true;
                        break;
                    case Key.PageUp:
                        InputSimulatorManager.Instance.InputSimu.Keyboard.ModifiedKeyStroke(VirtualKeyCode.SHIFT, VirtualKeyCode.TAB);
                        break;
                    case Key.PageDown:
                        InputSimulatorManager.Instance.InputSimu.Keyboard.KeyPress(VirtualKeyCode.TAB);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"加入课堂监听键盘事件发生异常 exception：{ex}");
            }

        }

        #endregion

        #region command

        public ICommand WindowKeyDownCommand { get; set; }

        #endregion
    }
}
