using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Classroom.View;
using Common.Contract;
using Common.Helper;
using Common.Model;
using Common.UiMessage;
using Prism.Commands;
using Prism.Mvvm;
using Serilog;
using MeetingSdk.NetAgent;
using MeetingSdk.NetAgent.Models;
using Classroom.Service;

namespace Classroom.ViewModel
{
    public class JoinClassViewModel : BindableBase
    {

        #region field

        private readonly JoinClassView _view;
        private string _classNo;
        private readonly ILocalDataManager _localDataManager;
        private readonly IMeetingSdkAgent _meetingSdkAgent;
        //private KeyBoardForm form;
        #endregion

        #region property

        public ObservableCollection<MeetingList> MeetingInvitationInfo { get; set; }

        public string ClassNo
        {
            get { return _classNo; }
            set { SetProperty(ref _classNo, value); }
        }

        #endregion

        #region ctor

        public JoinClassViewModel(JoinClassView view)
        {
            _view = view;
            _localDataManager = DependencyResolver.Current.GetService<ILocalDataManager>();
            _meetingSdkAgent = DependencyResolver.Current.GetService<IMeetingSdkAgent>();
            MeetingInvitationInfo = new ObservableCollection<MeetingList>();
            LoadCommand = DelegateCommand.FromAsyncHandler(LoadAsync);
            JoinClassCommand = new DelegateCommand(JoinClass);
        }

        #endregion

        #region method

        private async Task LoadAsync()
        {
            try
            {
                await GetMeetingList();
                DeviceSettingsChecker.Instance.IsVideoAudioSettingsValid(_view);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"加入课堂加载课堂信息发生异常 exception：{ex}");
                MessageQueueManager.Instance.AddError(MessageManager.LoadingError);
            }
        }

        private async Task GetMeetingList()
        {
            try
            {
                var list = await _meetingSdkAgent.GetMeetingList();

                GlobalData.Instance.MeetingList = list.Result.ToList();

                BindData();
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"加载课堂列表发生异常 exception：{ex}");
                MessageQueueManager.Instance.AddError(MessageManager.LoadingError);
            }
        }

        private void BindData()
        {
            var meetinglist = _localDataManager.GetMeetingList();

            var currentClassNo = GlobalData.Instance.Classroom.SchoolRoomNum;

            if (meetinglist != null && meetinglist.MeetingInfos.Count > 0)
            {
                meetinglist.MeetingInfos.ForEach(m =>
                {
                    //如果该课堂创建者是班级创建者，传true
                    //m.MeetingNo应该是creatorId
                    var joinControl = new JoinClassControl(_view, m);
                    _view.stack_join.Children.Add(joinControl);
                });
            }
        }


        public async void JoinClass()
        {
            //1.判断课堂号是否存在
            try
            {
                if (string.IsNullOrEmpty(ClassNo))
                {
                    MessageQueueManager.Instance.AddError("请输入课堂号！");
                    return;
                }
                int meetingId;
                var isNumber = int.TryParse(ClassNo, out meetingId);
                if (!isNumber)
                {
                    MessageQueueManager.Instance.AddError(MessageManager.MeetingNoExistError);
                }
                else
                {
                    if (meetingId <= 0)
                    {
                        MessageQueueManager.Instance.AddError(MessageManager.MeetingNoExistError);
                        return;
                    }
                    //判断课堂是否存在
                    var reuslt = await _meetingSdkAgent.IsMeetingExist(meetingId);
                    if (reuslt.StatusCode != 0)
                    {
                        MessageQueueManager.Instance.AddError(MessageManager.MeetingNoExistError);
                    }
                    else
                    {
                        var meetingList = _localDataManager.GetMeetingList() ??
                         new MeetingList() { MeetingInfos = new List<MeetingItem>() };
                        var localMeeting =
                               meetingList.MeetingInfos.FirstOrDefault(o => o.MeetingId == meetingId);
                        var meeting = GlobalData.Instance.MeetingList?.FirstOrDefault(o => o.MeetingId == meetingId);
                        if (localMeeting == null && meeting != null)
                        {
                            meetingList.MeetingInfos.Add(new MeetingItem()
                            {
                                LastActivityTime = DateTime.Now,
                                MeetingId = meetingId,
                                IsClose = false,
                                CreatorId = meeting.Account.AccountId.ToString()
                            });

                            AppCache.AddOrUpdate(CacheKey.HostId, meeting.Account.AccountId);
                            //_meetingService.CreatorPhoneId = meeting.CreatorId;
                        }
                        else
                        {
                            if (localMeeting != null)
                            {
                                localMeeting.LastActivityTime = DateTime.Now;
                                AppCache.AddOrUpdate(CacheKey.HostId, localMeeting.CreatorId);

                                //_meetingService.CreatorPhoneId = localMeeting.CreatorId;
                            }
                            else
                            {
                                MeetingResult<MeetingModel> meetingInfo = await _meetingSdkAgent.GetMeetingInfo(meetingId);

                                if (meetingInfo.StatusCode != 0)
                                {
                                    MessageQueueManager.Instance.AddError("获取会议信息时出错！");
                                    return;
                                }

                                AppCache.AddOrUpdate(CacheKey.HostId, meetingInfo.Result.HostId);

                                meetingList.MeetingInfos.Add(new MeetingItem()
                                {
                                    LastActivityTime = DateTime.Now,
                                    MeetingId = meetingId,
                                    IsClose = false,
                                    CreatorId = meetingInfo.Result.HostId.ToString(),
                                    CreateTime = DateTime.Parse(meetingInfo.Result.StartTime),
                                    CreatorName = GlobalData.Instance.Classrooms.FirstOrDefault(cls => cls.SchoolRoomNum == meetingInfo.Result.HostId.ToString())?.SchoolRoomName,
                                });
                            }
                        }
                        _localDataManager.SaveMeetingList(meetingList);
                        //进入课堂

                        AppCache.AddOrUpdate(CacheKey.MeetingId, meetingId);
                        //_meetingService.MeetingId = meetingId;
                        GlobalData.Instance.Course = new Course();
                        var intoClassView = new IntoClassView(IntoClassType.Join);
                        intoClassView.Show();

                        _view.Close();
                        ClassNo = string.Empty;

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"加入课堂发生异常 exception：{ex}");
                MessageQueueManager.Instance.AddError(MessageManager.JoinClassError);
            }

        }

        #endregion

        #region command

        public ICommand LoadCommand { get; set; }

        public ICommand JoinClassCommand { get; set; }

        #endregion

    }
}
