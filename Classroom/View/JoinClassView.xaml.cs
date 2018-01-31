using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WindowsInput.Native;
using Classroom.ViewModel;
using Common.Contract;
using Common.Helper;
using Common.Model;
using Serilog;
using Common.UiMessage;
using MeetingSdk.NetAgent;
using MeetingSdk.NetAgent.Models;

namespace Classroom.View
{
    /// <summary>
    /// JoinClassView.xaml 的交互逻辑
    /// </summary>
    public partial class JoinClassView
    {

        #region filed

        private KeyBoardForm _form;
        private readonly JoinClassView _view;
        private readonly IMeetingSdkAgent _meetingService;
        private const string DefaultTip = "按ok键弹出键盘";
        private ILocalDataManager _localDataManager;

        #endregion

        #region ctor

        public JoinClassView()
        {
            _view = this;
            InitializeComponent();
            _localDataManager = DependencyResolver.Current.GetService<ILocalDataManager>();
            _meetingService = DependencyResolver.Current.GetService<IMeetingSdkAgent>();
            DataContext = new JoinClassViewModel(this);
            txt_classNo.Focus();
            EventBinding();
        }

        #endregion

        #region event

        private void EventBinding()
        {
            InputSimulatorManager.Instance.TextBoxEnterPressEvent += GetKeyBoardForm;
        }

        private void txt_classNo_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txt_classNo.Text.Length > 8)
            {
                txt_classNo.Text = txt_classNo.Text.Substring(0, 8);
            }
        }

        private void txt_classNo_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.PageUp:
                    InputSimulatorManager.Instance.InputSimu.Keyboard.ModifiedKeyStroke(VirtualKeyCode.SHIFT,
                        VirtualKeyCode.TAB);
                    e.Handled = true;
                    break;
                case Key.Left:
                case Key.Home:
                    InputSimulatorManager.Instance.TextBoxPreviewKeyDown(sender, e);
                    e.Handled = true;
                    break;
                case Key.Right:
                case Key.End:
                    InputSimulatorManager.Instance.TextBoxPreviewKeyDown(sender, e);
                    e.Handled = true;
                    break;
                case Key.Down:
                case Key.PageDown:
                    StackPanelFirstRowFocus();
                    e.Handled = true;
                    break;
                case Key.Enter:
                    InputSimulatorManager.Instance.TextBoxPreviewKeyDown(sender, e);
                    break;
            }


        }

        private void txt_classNo_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (txt_classNo.Text.Contains(DefaultTip) && txt_classNo.Text != DefaultTip)
            //    txt_classNo.Text = txt_classNo.Text.Replace(DefaultTip, "");
        }



        public override void EscapeKeyDownHandler()
        {
            InputSimulatorManager.Instance.TextBoxEnterPressEvent -= GetKeyBoardForm;
            var mainView = new MainView();
            mainView.Show();
            Close();
        }

        #endregion

        #region method

        private void StackPanelFirstRowFocus()
        {
            try
            {
                if (stack_join.Children.Count <= 0) return;
                var control = (JoinClassControl)stack_join.Children[0];
                control.stackPanel.Focus();
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"加入课堂加载列表发生异常 exception：{ex}");
            }
        }

        private void GetKeyBoardForm(TextBox textbox)
        {
            try
            {
                _form = KeyBoardForm.GetKeyBoardForm(textbox, "确定", OkBtnClickCallBack);

            }
            catch (Exception ex)
            {
                Log.Logger.Error($"加载键盘发生异常 exception：{ex}");
            }
        }

        private void OkBtnClickCallBack()
        {
            JoinClass(txt_classNo.Text);
        }

        #endregion

        private void btnBack_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            InputSimulatorManager.VirtualPreviewKeyDown(sender, e);
        }

        public async void JoinClass(string classNo)
        {
            //1.判断课堂号是否存在
            try
            {
                if (string.IsNullOrEmpty(classNo))
                {
                    MessageQueueManager.Instance.AddError("请输入课堂号！");
                    return;
                }
                int meetingId;
                var isNumber = int.TryParse(classNo, out meetingId);
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
                    var reuslt = await _meetingService.IsMeetingExist(meetingId);
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
                                localMeeting.LastActivityTime=DateTime.Now;
                                AppCache.AddOrUpdate(CacheKey.HostId, localMeeting?.CreatorId);
                            }
                            else
                            {
                                MeetingResult<MeetingModel> meetingModelResult = await _meetingService.GetMeetingInfo(meetingId);
                                meeting = meetingModelResult.Result;

                                meetingList.MeetingInfos.Add(new MeetingItem()
                                {
                                    LastActivityTime = DateTime.Now,
                                    MeetingId = meetingId,
                                    IsClose = false,
                                    CreatorId = meeting.Account.AccountId.ToString(),
                                    CreateTime = DateTime.Parse(meeting.StartTime),
                                    CreatorName = GlobalData.Instance.Classrooms.FirstOrDefault(cls => cls.SchoolRoomNum == meeting.HostId.ToString())?.SchoolRoomName,
                                });


                                AppCache.AddOrUpdate(CacheKey.HostId, meetingModelResult.Result.HostId);
                            }
                        }
                        _localDataManager.SaveMeetingList(meetingList);
                        //进入课堂

                        AppCache.AddOrUpdate(CacheKey.MeetingId, meetingId);

                        GlobalData.Instance.Course = new Course();
                        var intoClassView = new IntoClassView(IntoClassType.Join);
                        intoClassView.Show();
                        txt_classNo.Text = string.Empty;
                        _view.Close();

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"加入课堂发生异常 exception：{ex}");
                MessageQueueManager.Instance.AddError(MessageManager.JoinClassError);
            }
        }

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            InputSimulatorManager.Instance.TextBoxEnterPressEvent -= GetKeyBoardForm;
            var mainView = new MainView();
            _form?.Close();
            mainView.Show();
            _view.Close();
        }

        private void JoinClassView_OnClosed(object sender, EventArgs e)
        {
            _form?.Close();
            InputSimulatorManager.Instance.TextBoxEnterPressEvent -= GetKeyBoardForm;
        }
    }
}
