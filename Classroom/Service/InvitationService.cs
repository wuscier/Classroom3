using Classroom.View;
using Common.CustomControl;
using Common.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Common.Contract;
using Common.Model;
using Common.UiMessage;
using Autofac;
using Prism.Events;
using MeetingSdk.Wpf;
using MeetingSdk.NetAgent.Models;
using MeetingSdk.NetAgent;

namespace Classroom.Service
{
    public class InvitationService
    {
        private readonly IMeetingSdkAgent _meetingSdkAgent;
        private readonly IMeetingWindowManager _windowManager;
        private readonly ILocalDataManager _localDataManager;
        private readonly IPushLive _manualPushLive;
        private readonly IRecordLive _localRecordLive;
        private readonly IEventAggregator _eventAggregator;
        const int WM_CLOSE = 0x0010;

        private InvitationService()
        {
            _meetingSdkAgent = DependencyResolver.Current.GetService<IMeetingSdkAgent>();
            _windowManager = DependencyResolver.Current.GetService<IMeetingWindowManager>();
            _localDataManager = DependencyResolver.Current.GetService<ILocalDataManager>();
            _manualPushLive = DependencyResolver.Current.Container.ResolveNamed<IPushLive>("ManualPushLive");
            _localRecordLive = DependencyResolver.Current.GetService<IRecordLive>();
            _eventAggregator = DependencyResolver.Current.GetService<IEventAggregator>();

            _eventAggregator.GetEvent<UiTransparentMsgReceivedEvent>().Subscribe(OnUiMsgReceivedEventHandler);
            _eventAggregator.GetEvent<MeetingInvitationEvent>().Subscribe(_meetingManagerService_InvitationReceivedEvent);
        }

        private void OnUiMsgReceivedEventHandler(UiTransparentMsg obj)
        {
            
        }

        public void Initialize()
        {

        }

        public static readonly InvitationService Instance = new InvitationService();
        private Dialog _exitDialog = null;

        public void _meetingManagerService_InvitationReceivedEvent(MeetingInvitationModel invitation)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
            {
                string creatorName;
                var currentWindows = Application.Current.Windows;
                Window window = currentWindows[0];
                if (_exitDialog != null) return;
                var trueInvitor =
                    GlobalData.Instance.Classrooms.FirstOrDefault(o => o.SchoolRoomNum == invitation.SenderId.ToString());
                var inClass = false;
                //判断是否再课堂中，如果在课堂中，需要判断新邀请与当前课堂是否是同一个，如果是，则不需要处理，否则退出当前课堂，进入新课堂

                int curMeetingId;
                object curMeetingIdObj = AppCache.TryGet(CacheKey.MeetingId);
                if (curMeetingIdObj != null && int.TryParse(curMeetingIdObj.ToString(), out curMeetingId))
                {
                    if (curMeetingId == invitation.MeetingId) return;
                    //在课堂中
                    _exitDialog = new Dialog($"您当前正在课堂中，是否要结束当前课堂，加入到新课堂？", "加入新课堂", "留在当前课堂");
                    inClass = true;
                }
                else
                {
                    _exitDialog = new Dialog($"{trueInvitor?.SchoolRoomName}，邀请您听课", "去听课", "稍后");
                }

                var result = _exitDialog.ShowDialog();
                if (!result.HasValue || !result.Value) return;

                bool isSettingsValid = DeviceSettingsChecker.Instance.IsVideoAudioSettingsValid(window);

                if (!isSettingsValid)
                {
                    return;
                }

                //判断课堂是否存在
                var reuslt = await _meetingSdkAgent.IsMeetingExist(invitation.MeetingId);
                if (reuslt.StatusCode != 0)
                {
                    MessageQueueManager.Instance.AddError(MessageManager.MeetingNoExistError);
                    return;
                }
                if (inClass)
                {
                    //退出当前课堂
                    //var clearResult = await ClearMeeting();
                    //if (!clearResult)
                    //{
                    //    MessageQueueManager.Instance.AddError(MessageManager.ExitMeetingError);
                    //    return;
                    //}
                    var exitMessage = await _meetingSdkAgent.LeaveMeeting();
                    await _windowManager.Leave();
                    if (exitMessage.StatusCode != 0)
                    {
                        MessageQueueManager.Instance.AddError(MessageManager.ExitMeetingError);
                        return;
                    }
                }
                var meetingList = _localDataManager.GetMeetingList() ??
                                  new MeetingList() { MeetingInfos = new List<MeetingItem>() };
                var localMeetingInfo = meetingList.MeetingInfos.FirstOrDefault(o => o.MeetingId == invitation.MeetingId);

                string createTime = string.Empty;

                if (localMeetingInfo != null && localMeetingInfo.CreatorId == GlobalData.Instance.Classroom.SchoolRoomNum)
                {
                    createTime = localMeetingInfo.CreateTime.ToString();
                    AppCache.AddOrUpdate(CacheKey.HostId, GlobalData.Instance.Classroom.SchoolRoomNum);
                    creatorName = GlobalData.Instance.Classroom.SchoolRoomName;
                }
                else
                {
                    MeetingResult<MeetingModel> meetingInfo = await _meetingSdkAgent.GetMeetingInfo(invitation.MeetingId);

                    if (meetingInfo.StatusCode != 0)
                    {
                        MessageQueueManager.Instance.AddError("获取会议信息时出错！");
                        return;
                    }

                    if (meetingInfo.Result.MeetingType == MeetingType.DatedMeeting && meetingInfo.Result.HostId == 0)
                    {


                    }

                    AppCache.AddOrUpdate(CacheKey.HostId, meetingInfo.Result.HostId);

                    createTime = meetingInfo.Result.StartTime;

                    creatorName = GlobalData.Instance.Classrooms.FirstOrDefault(cls => cls.SchoolRoomNum == meetingInfo.Result.HostId.ToString())?.SchoolRoomName;
                }

                AppCache.AddOrUpdate(CacheKey.MeetingId, invitation.MeetingId);

                GlobalData.Instance.Course = new Course();
                if (inClass)
                {
                    //var mainView = new MainView();
                    //mainView.Show();
                    var wd = SetWindowsTop.FindWindow(null, "IntoClassView");
                    await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //if (_viewLayoutService.IsDoubleScreenOn)
                        //{
                        //    _viewLayoutService.IsDoubleScreenOn = false;
                        //    _viewLayoutService.StopExtendedViewAsync();
                        //    _viewLayoutService.ExtenedViewFrameList?.Clear();
                        //    _viewLayoutService.ResetAsInitialStatus();
                        //    Thread.Sleep(20);
                        //    foreach (Window item in Application.Current.Windows)
                        //    {
                        //        if (item is ExtendedScreenView)
                        //        {
                        //            item.Close();
                        //        }
                        //    }
                        //    SetWindowsTop.SetWindowPos(wd, 0, int.MaxValue, int.MaxValue, 0, 0, 0);
                        //}
                        var intoClassView = new IntoClassView(IntoClassType.Join);
                        intoClassView.Show();
                        //foreach (Window item in Application.Current.Windows)
                        //{
                        //    if (item is MainView)
                        //    {
                        //        item.Close();
                        //    }
                        //}
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            SetWindowsTop.SendMessage(wd, WM_CLOSE, 0, 0);
                        }));
                    }));
                }
                else
                {
                    var intoClassView = new IntoClassView(IntoClassType.Join);
                    intoClassView.Show();
                    foreach (Window item in Application.Current.Windows)
                    {
                        if (!(item is IntoClassView || item is ExtendedScreenView)) item.Close();
                    }
                }
                UpdateMeeting(meetingList, creatorName,createTime);
            }));
            _exitDialog = null;
        }

        private bool ClearMeeting()
        {
            var reuslt = StopAllLives();
            if (!reuslt) return false;

            if (_windowManager.ModeChange(ModeDisplayerType.InteractionMode))
            {
            }

            //if (_windowManager.LayoutChange(WindowNames.MainWindow, LayoutRenderType.AutoLayout))
            //{
            //}
            //if (_windowManager.LayoutChange(WindowNames.ExtendedWindow, LayoutRenderType.AutoLayout))
            //{
            //}

            return true;
        }

        private bool StopAllLives()
        {
             _manualPushLive.StopPushLiveStream();
            _localRecordLive.StopMp4Record();
            //_viewLayoutService.ExtenedViewFrameList?.Clear();
            return true;
        }

        private void UpdateMeeting(MeetingList meetingList, string creatorName, string createTime)
        {
            var currentMeeting =
                    meetingList.MeetingInfos.FirstOrDefault(o => o.MeetingId == (int)AppCache.TryGet(CacheKey.MeetingId));
            if (currentMeeting == null)
            {
                MeetingItem meetingItem = new MeetingItem()
                {
                    LastActivityTime = DateTime.Now,
                    MeetingId = (int)AppCache.TryGet(CacheKey.MeetingId),
                    CreatorName = creatorName,
                    IsClose = false,
                    CreatorId = AppCache.TryGet(CacheKey.HostId).ToString(),
                };

                if (!string.IsNullOrEmpty(createTime))
                {
                    meetingItem.CreateTime = DateTime.Parse(createTime);
                }

                meetingList.MeetingInfos.Add(meetingItem);
            }
            else
            {
                currentMeeting.LastActivityTime = DateTime.Now;
            }
            _localDataManager.SaveMeetingList(meetingList);
        }
    }
}
