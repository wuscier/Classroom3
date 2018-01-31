using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Autofac;
using Classroom.Model;
using Classroom.View;

using Common.Contract;
using Common.CustomControl;
using Common.Helper;
using Common.Model;
using Common.Model.ViewLayout;
using Common.UiMessage;
using MahApps.Metro.Controls;

using Prism.Commands;
using Prism.Mvvm;
using Serilog;
using Service;
using MeetingSdk.NetAgent;
using MeetingSdk.Wpf;
using Prism.Events;
using MeetingSdk.NetAgent.Models;
using Timer = System.Threading.Timer;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Label = System.Windows.Forms.Label;
using MeetingSdk.Wpf.Interfaces;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.Integration;

namespace Classroom.ViewModel
{
    public class IntoClassViewModel : BindableBase
    {
        private readonly IntoClassView _intoClassView;
        private UIElement _downFocusedUiElement;
        private UIElement _upFocusedUiElement;
        private int _pressedUpKeyCount = 0;
        private int _pressedDownKeyCount = 0;
        private int _pressedLeftKeyCount = 0;
        private int _pressedRightKeyCount = 0;

        private const string IsSpeaking = "取消发言";
        private const string IsNotSpeaking = "申请发言";

        private readonly IMeetingSdkAgent _meetingSdkAgent;
        private readonly IMeetingWindowManager _windowManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILocalDataManager _localDataManager;
        private readonly IExtendedWindowManager _extendedWindowManager;

        private readonly IPushLive _manualPushLive;
        private readonly IRecordLive _localRecordLive;
        private readonly IRemoteRecord _remoteRecord;
        private ExtendedScreenView _extendedScreenView;
        private Timer _AutoHideMenuTimer;
        private bool _IsWindowActive;
        private DateTime _autoHideInitialTime = DateTime.Now;

        private bool isRecordRemote;
        private bool isRecordLocal;

        public IntoClassViewModel(IntoClassView intoClassView)
        {

            _intoClassView = intoClassView;

            _localDataManager = DependencyResolver.Current.GetService<ILocalDataManager>();
            _extendedWindowManager = DependencyResolver.Current.GetService<IExtendedWindowManager>();

            _meetingSdkAgent = DependencyResolver.Current.GetService<IMeetingSdkAgent>();
            _windowManager = DependencyResolver.Current.GetService<IMeetingWindowManager>();

            _eventAggregator = DependencyResolver.Current.GetService<IEventAggregator>();

                        _manualPushLive = DependencyResolver.Current.Container.ResolveNamed<IPushLive>("ManualPushLive");
            _manualPushLive.ResetStatus();
            _localRecordLive = DependencyResolver.Current.GetService<IRecordLive>();
            _remoteRecord = DependencyResolver.Current.GetService<IRemoteRecord>();
            _localRecordLive.ResetStatus();
            _remoteRecord.IsRemoteRecord = false;

            RegisterMeetingEvents();

            PrepareDataContext();
        }

        private void RegisterMeetingEvents()
        {
            _intoClassView.Closing += _intoClassView_Closing;

            _eventAggregator.GetEvent<StartSpeakEvent>().Subscribe(StartSpeakEventHandler);
            _eventAggregator.GetEvent<StopSpeakEvent>().Subscribe(StopSpeakEventHandler);
            _eventAggregator.GetEvent<UserJoinEvent>().Subscribe(OtherJoinMeetingEventHandler);
            _eventAggregator.GetEvent<UserLeaveEvent>().Subscribe(OtherExitMeetingEventHandler);
            _eventAggregator.GetEvent<TransparentMsgReceivedEvent>().Subscribe(UIMessageReceivedEventHandler);
            _eventAggregator.GetEvent<HostKickoutUserEvent>().Subscribe(KickedByHostEventHandler);

            _eventAggregator.GetEvent<DeviceLostNoticeEvent>().Subscribe(DeviceLostNoticeEventHandler);
            _eventAggregator.GetEvent<DeviceStatusChangedEvent>().Subscribe(DeviceStatusChangedEventHandler);
            _eventAggregator.GetEvent<LockStatusChangedEvent>().Subscribe(LockStatusChangedEventHandler);
            _eventAggregator.GetEvent<MeetingManageExceptionEvent>().Subscribe(MeetingManageExceptionEventHandler);
            _eventAggregator.GetEvent<SdkCallbackEvent>().Subscribe(SdkCallbackEventHandler);
            _eventAggregator.GetEvent<UiTransparentMsgReceivedEvent>().Subscribe(UiTransparentMsgReceivedEventHandler);

            _eventAggregator.GetEvent<ModeDisplayerTypeChangedEvent>().Subscribe(ClassModeChangedEventHandler);
            _eventAggregator.GetEvent<LayoutChangedEvent>().Subscribe(LayoutChangedEventHandler);
            _eventAggregator.GetEvent<RefreshCanvasEvent>().Subscribe(RefreshViewContainerBackground);

            _eventAggregator.GetEvent<ExtendedViewsClosedEvent>().Subscribe(ExtendedViewsClosedEventHandler);
            _eventAggregator.GetEvent<ExtendedViewsShowedEvent>().Subscribe(SyncExtendedViews);

            _eventAggregator.GetEvent<ParticipantCollectionChangeEvent>().Subscribe(ParticipantCollectionChangeEventHandler);

        }

        private void ParticipantCollectionChangeEventHandler(IEnumerable<Participant> obj)
        {
            foreach (var participant in _windowManager.Participants)
            {
                var classroom = GlobalData.Instance.Classrooms.FirstOrDefault(cls => cls.SchoolRoomNum == participant.Account.AccountId.ToString());
                if (classroom != null)
                {
                    participant.Account.AccountName = classroom.SchoolRoomName;
                }
            }
        }

        private async void LayoutChangedEventHandler(LayoutRenderType obj)
        {
            RefreshLocalRecordLive();
            RefreshPushLive();
        }

        private async void ClassModeChangedEventHandler(ModeDisplayerType modeDisplayerType)
        {
            if (IsCreator)
            {
                await SyncClassMode();
            }

            RefreshLocalRecordLive();
            RefreshPushLive();

            ClassModeStatus.ClassModeName = EnumHelper.GetDescription(typeof(ModeDisplayerType), modeDisplayerType);
            ClassModeStatus.ClassModeImage = ClassModeHelper.GetImageUrl(modeDisplayerType);
        }

        private void ExtendedViewsClosedEventHandler()
        {
            if (_extendedWindowManager.ExtendedView != null && _extendedWindowManager.ExtendedView.IsLoaded)
            {
                foreach (var host in _extendedWindowManager.Items)
                {
                    //VideoBox videoBox = host.GetValue(MeetingWindow.VideoBoxProperty) as VideoBox;
                    //WindowsFormsHost windowsFormsHost = host as WindowsFormsHost;
                    //PictureBox pictureBox = windowsFormsHost.Child as PictureBox;

                    //if (host.Visibility != Visibility.Visible && (host.Tag != null))
                    //{
                    //    if (videoBox.AccountResource != null)
                    //    {
                    //        _meetingSdkAgent.RemoveDisplayWindow(videoBox.AccountResource.AccountModel.AccountId, videoBox.AccountResource.ResourceId, pictureBox.Handle, 0, 0);
                    //    }
                    //    host.Tag = null;
                    //}
                }
            }
        }

        private void UiTransparentMsgReceivedEventHandler(UiTransparentMsg obj)
        {
            if (obj.MsgId < 3)
            {
                AppCache.AddOrUpdate(CacheKey.HostId, obj.TargetAccountId);

                var classMode = (ModeDisplayerType)obj.MsgId;


                if (_windowManager.ModeChange(classMode))
                {
                }
            }
        }


        private void SyncExtendedViews()
        {
            Console.WriteLine("SyncExtendedViews begins");
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_extendedWindowManager.ExtendedView != null && _extendedWindowManager.ExtendedView.IsLoaded)
                {
                    //foreach (var item in _extendedWindowManager.Items)
                    //{
                    //    if (item.AccountResource != null && item.Visible)
                    //    {
                    //        Console.WriteLine("AddDisplayWindow begins");

                    //        var result = _meetingSdkAgent.AddDisplayWindow(item.AccountResource.AccountModel.AccountId, item.AccountResource.ResourceId, item.Handle, 0, 0);

                    //        Console.WriteLine("AddDisplayWindow ends");
                    //    }

                    //    AddLabel4PictureBox(item);
                    //}

                    RefreshExntendedViewContainerBackground();
                }
            }));
            Console.WriteLine("SyncExtendedViews ends");
        }

        private void AddLabel4PictureBox(VideoBox videoBox)
        {
            var host = _extendedScreenView as IHost;

            if (host != null)
            {
                var winHost = host.Hosts.FirstOrDefault(h => h.Name == videoBox.Name);

                if (winHost != null)
                {
                    var picBox = winHost.Child as PictureBox;

                    Label label = new Label()
                    {
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = System.Drawing.Color.White,
                        ForeColor = System.Drawing.Color.Black,
                        Visible = false
                    };

                    picBox.Controls.Clear();

                    picBox.Controls.Add(label);
                }
            }
        }

        private void RefreshExntendedViewContainerBackground()
        {
            int openedViewsCount = _extendedWindowManager.Items.Count(v => v.Visible);

            try
            {
                Canvas canvas = _extendedScreenView?.Content as Canvas;

                if (canvas != null)
                {
                    if (openedViewsCount == 0)
                    {
                        canvas.Background = new ImageBrush()
                        {
                            ImageSource = new BitmapImage(new Uri("pack://application:,,,/Common;Component/Image/kt_bg.png", UriKind.RelativeOrAbsolute)),
                            Stretch = Stretch.Fill
                        };
                    }
                    else
                    {
                        canvas.Background = new SolidColorBrush(Colors.Black);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private async Task SyncClassMode()
        {
            MeetingResult sendUiMsgResult = await _meetingSdkAgent.AsynSendUIMsg((int)_windowManager.ModeDisplayerStore.CurrentModeDisplayerType, 0, "");
        }

        private void RefreshViewContainerBackground()
        {
            int openedViewsCount = _windowManager.VideoBoxManager.Items.Count(v => v.Visible);
            _intoClassView.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (openedViewsCount == 0)
                    {
                        BackgroundBrush = new ImageBrush()
                        {
                            ImageSource =
                                new BitmapImage(new Uri("pack://application:,,,/Common;Component/Image/kt_bg.png",
                                    UriKind.RelativeOrAbsolute)),
                            Stretch = Stretch.Fill
                        };
                    }
                    else
                    {
                        BackgroundBrush = new SolidColorBrush(Colors.Black);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }));
        }

        private void RefreshPushLive()
        {
            if (_manualPushLive.LiveId != 0)
            {
                IGetLiveVideoCoordinate liveVideoCoordinate = _windowManager as IGetLiveVideoCoordinate;
                System.Windows.Size size = new System.Windows.Size()
                {
                    Width = _manualPushLive.LiveParam.LiveParameter.Width,
                    Height = _manualPushLive.LiveParam.LiveParameter.Height,
                };

                VideoStreamModel[] videoStreamModels = liveVideoCoordinate.GetVideoStreamModels(size);

                _manualPushLive.RefreshLiveStream(videoStreamModels, GetAudioStreamModels());
            }
        }

        private void RefreshLocalRecordLive()
        {
            if (_localRecordLive.RecordId != 0)
            {
                IGetLiveVideoCoordinate liveVideoCoordinate = _windowManager as IGetLiveVideoCoordinate;
                System.Windows.Size size = new System.Windows.Size()
                {
                    Width = _localRecordLive.RecordParam.LiveParameter.Width,
                    Height = _localRecordLive.RecordParam.LiveParameter.Height,
                };

                VideoStreamModel[] videoStreamModels = liveVideoCoordinate.GetVideoStreamModels(size);

                _localRecordLive.RefreshLiveStream(videoStreamModels, GetAudioStreamModels());
            }
        }

        private void SdkCallbackEventHandler(SdkCallbackModel obj)
        {
            throw new NotImplementedException();
        }

        private void MeetingManageExceptionEventHandler(ExceptionModel obj)
        {
            throw new NotImplementedException();
        }

        private void LockStatusChangedEventHandler(MeetingResult obj)
        {
            throw new NotImplementedException();
        }

        private void DeviceStatusChangedEventHandler(DeviceStatusModel obj)
        {
            throw new NotImplementedException();
        }

        private void DeviceLostNoticeEventHandler(ResourceModel obj)
        {
            throw new NotImplementedException();
        }

        private void _intoClassView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _AutoHideMenuTimer?.Dispose();
            UnRegisterMeetingEvents();
            _extendedScreenView?.Close();
            //if (_viewLayoutService.ExtenedViewFrameList != null && _viewLayoutService.ExtenedViewFrameList.Any())
            //{
            //    _viewLayoutService.ExtenedViewFrameList?.Clear();
            //}
        }

        private void UnRegisterMeetingEvents()
        {
            _eventAggregator.GetEvent<StartSpeakEvent>().Unsubscribe(StartSpeakEventHandler);
            _eventAggregator.GetEvent<StopSpeakEvent>().Unsubscribe(StopSpeakEventHandler);

            //CurrentLayoutInfo.Instance.ClassModeChangedEvent -= MeetingModeChangedEventHandler;
            //CurrentLayoutInfo.Instance.PictureModeChangedEvent -= ViewModeChangedEventHandler;
            //CurrentLayoutInfo.Instance.LayoutChangedEvent -= _viewLayoutService_LayoutChangedEvent;


            _eventAggregator.GetEvent<UserJoinEvent>().Unsubscribe(OtherJoinMeetingEventHandler);
            _eventAggregator.GetEvent<UserLeaveEvent>().Unsubscribe(OtherExitMeetingEventHandler);
            _eventAggregator.GetEvent<TransparentMsgReceivedEvent>().Unsubscribe(UIMessageReceivedEventHandler);
            _eventAggregator.GetEvent<HostKickoutUserEvent>().Unsubscribe(KickedByHostEventHandler);

            _eventAggregator.GetEvent<DeviceLostNoticeEvent>().Unsubscribe(DeviceLostNoticeEventHandler);
            _eventAggregator.GetEvent<DeviceStatusChangedEvent>().Unsubscribe(DeviceStatusChangedEventHandler);
            _eventAggregator.GetEvent<LockStatusChangedEvent>().Unsubscribe(LockStatusChangedEventHandler);
            _eventAggregator.GetEvent<MeetingManageExceptionEvent>().Unsubscribe(MeetingManageExceptionEventHandler);
            _eventAggregator.GetEvent<SdkCallbackEvent>().Unsubscribe(SdkCallbackEventHandler);
            _eventAggregator.GetEvent<UiTransparentMsgReceivedEvent>().Unsubscribe(UiTransparentMsgReceivedEventHandler);
            _eventAggregator.GetEvent<ExtendedViewsClosedEvent>().Unsubscribe(ExtendedViewsClosedEventHandler);
            _eventAggregator.GetEvent<ExtendedViewsShowedEvent>().Unsubscribe(SyncExtendedViews);

            _eventAggregator.GetEvent<ModeDisplayerTypeChangedEvent>().Unsubscribe(ClassModeChangedEventHandler);
            _eventAggregator.GetEvent<LayoutChangedEvent>().Unsubscribe(LayoutChangedEventHandler);
            _eventAggregator.GetEvent<RefreshCanvasEvent>().Unsubscribe(RefreshViewContainerBackground);


            _eventAggregator.GetEvent<ParticipantCollectionChangeEvent>().Unsubscribe(ParticipantCollectionChangeEventHandler);

        }


        private async void PrepareDataContext()
        {
            isRecordRemote = GlobalData.Instance.Course?.IsRecordRemote ?? false;
            isRecordLocal = GlobalData.Instance.Course?.IsLocalRecord ?? false;


            UpButtonGotFocusCommand = new DelegateCommand<UIElement>(menuItem =>
            {
                //Console.WriteLine($"up button got focus, {menuItem}");
                _upFocusedUiElement = menuItem;
            });
            DownButtonGotFocusCommand = new DelegateCommand<UIElement>((menuItem) =>
            {
                //Console.WriteLine($"down button got focus, {menuItem}");
                _downFocusedUiElement = menuItem;
            });

            LoadedCommand = DelegateCommand.FromAsyncHandler(LoadedAsync);
            PushLiveCommand = new DelegateCommand(PushLiveAsync);
            LocalRecordCommand = new DelegateCommand(LocalRecordAsync);

            GotoExitMeetingCommand = DelegateCommand.FromAsyncHandler(GotoExitMeetingAsync);
            SetDoubleScreenOnOrOffCommand = DelegateCommand.FromAsyncHandler(SetDoubleScreenOnOrOff);

            KeyDownCommand = new DelegateCommand<object>(KeyDownHandler);

            ClassModeStatus = new ClassModeItem();

            _windowManager.ModeDisplayerStore.CurrentModeDisplayerType = IsCreator ? ModeDisplayerType.SpeakerMode : ModeDisplayerType.InteractionMode;
            _windowManager.LayoutRendererStore.CurrentLayoutRenderType = LayoutRenderType.AutoLayout;

            PushLiveTriggerItem = new ToggleButtonItem()
            {
                IsChecked = false,
                Name = "一键推流",
                ToggleCommand = PushLiveCommand
            };

            LocalRecordTriggerItem = new ToggleButtonItem()
            {
                IsChecked = isRecordLocal,
                Name = "本地录制",
                ToggleCommand = LocalRecordCommand
            };

            CourseNo = $"课堂号：{AppCache.TryGet(CacheKey.MeetingId)}";

            RefreshViewContainerBackground();
        }

        private async Task GotoExitMeetingAsync()
        {
            _autoHideInitialTime = DateTime.Now;

            _upFocusedUiElement = _intoClassView.ExitButton;

            Dialog exitDialog = new Dialog("您当前在课堂中，是否要结束该课堂？", "退出该课堂", "留在当前课堂");
            bool? result = exitDialog.ShowDialog();
            _autoHideInitialTime = DateTime.Now;

            if (result.HasValue && result.Value)
            {
                UnRegisterMeetingEvents();


                MeetingResult meetingResult = await _meetingSdkAgent.LeaveMeeting();

                await _windowManager.Leave();

                if (meetingResult.StatusCode != 0)
                {
                    MessageQueueManager.Instance.AddError(MessageManager.ExitMeetingError);
                    Log.Logger.Error(meetingResult.Message);
                }

                _extendedWindowManager.CloseExtendedView();                

                StopAllLives();

                MainView mainView = new MainView();
                mainView.Show();

                _intoClassView.Grid.Children.Remove(App.VideoControl);

                _intoClassView.Close();
                GlobalData.Instance.Course = new Course();

                AppCache.TryRemove(CacheKey.MeetingId);

                AppCache.AddOrUpdate(CacheKey.IsDocOpen, false);

                foreach (Window item in System.Windows.Application.Current.Windows)
                {
                    if (item is InviteAttendeeView)
                    {
                        item.Close();
                    }
                }
            }
            else
            {
                _upFocusedUiElement?.Focus();
            }
        }

        private void StopAllLives()
        {
            _manualPushLive.StopPushLiveStream();
             _localRecordLive.StopMp4Record();
        }

        private void CheckOnOpenQos()
        {
            var qosInfo = _meetingSdkAgent.GetMeetingQos();

            if (qosInfo.StatusCode != 0)
            {
                MessageQueueManager.Instance.AddError(qosInfo.Message);
            }

            QosView qosView = new QosView(qosInfo.Result);
            qosView.Show();
        }

        private void CheckOnCloseQos()
        {
            foreach (Window window in App.Current.Windows)
            {
                if (window is QosView)
                {
                    window.Close();
                }
            }
        }


        private void KeyDownHandler(object obj)
        {
            _autoHideInitialTime = DateTime.Now;

            KeyEventArgs keyEventArgs = obj as KeyEventArgs;

            switch (keyEventArgs.Key)
            {
                case Key.Escape:
                    _pressedUpKeyCount = 0;
                    _pressedDownKeyCount = 0;
                    _pressedLeftKeyCount = 0;
                    _pressedRightKeyCount = 0;

                    ShowTopBottomMenus = false;

                    break;
                case Key.Enter:
                case Key.Apps:
                    _pressedUpKeyCount = 0;
                    _pressedDownKeyCount = 0;
                    _pressedLeftKeyCount = 0;
                    _pressedRightKeyCount = 0;


                    if (!ShowTopBottomMenus)
                    {
                        ShowTopBottomMenus = true;
                        _downFocusedUiElement?.Focus();
                        keyEventArgs.Handled = true;
                        //InputSimulatorManager.Instance.InputSimu.Keyboard.KeyPress(VirtualKeyCode.DOWN);
                    }
                    break;
                case Key.Up:
                    _pressedDownKeyCount = 0;
                    _pressedLeftKeyCount = 0;
                    _pressedRightKeyCount = 0;

                    if (ShowTopBottomMenus)
                    {
                        if (_upFocusedUiElement == null)
                        {
                            _upFocusedUiElement = _intoClassView.ExitButton;
                        }

                        _intoClassView.ExitButton.Focus();
                    }

                    _pressedUpKeyCount++;

                    if (_pressedUpKeyCount == 4)
                    {
                        _pressedUpKeyCount = 0;
                        CheckOnOpenQos();
                    }

                    break;
                case Key.Down:

                    _pressedUpKeyCount = 0;
                    _pressedLeftKeyCount = 0;
                    _pressedRightKeyCount = 0;

                    _pressedDownKeyCount++;

                    if (ShowTopBottomMenus)
                    {
                        if (_downFocusedUiElement == null)
                        {
                            SetUpDefaultFocusedBottomButton();
                        }
                        else
                        {
                            _downFocusedUiElement.Focus();
                        }
                    }

                    if (_pressedDownKeyCount == 4)
                    {
                        _pressedDownKeyCount = 0;
                        CheckOnCloseQos();
                    }

                    break;
                default:
                    _pressedUpKeyCount = 0;
                    _pressedDownKeyCount = 0;
                    _pressedLeftKeyCount = 0;
                    _pressedRightKeyCount = 0;
                    break;
            }
        }


        private bool HasStreams()
        {
            return _windowManager.VideoBoxManager.Items.Any(v => v.Visible);
        }

        private  void LocalRecordAsync()
        {
            _autoHideInitialTime = DateTime.Now;

            _upFocusedUiElement = _intoClassView.LocalRecordTriggerButton;

            if (!HasStreams())
            {
                LocalRecordTriggerItem.IsChecked = !LocalRecordTriggerItem.IsChecked;
                MessageQueueManager.Instance.AddError("当前没有音视频流，无法录制！");
                return;
            }

            if (LocalRecordTriggerItem.IsChecked)
            {
                var recordRt = _localRecordLive.GetRecordParam();
                if (!recordRt)
                {
                    MessageQueueManager.Instance.AddError("录制参数未正确配置");
                    LocalRecordTriggerItem.IsChecked = false;
                    LocalRecordTriggerItem.Tips = null;
                    return;
                }

                IGetLiveVideoCoordinate liveVideoCoordinate = _windowManager as IGetLiveVideoCoordinate;

                System.Windows.Size size = new System.Windows.Size()
                {
                    Width = _localRecordLive.RecordParam.LiveParameter.Width,
                    Height = _localRecordLive.RecordParam.LiveParameter.Height,
                };

                VideoStreamModel[] videoStreamModels = liveVideoCoordinate.GetVideoStreamModels(size);

                AudioStreamModel[] audioStreamModels = GetAudioStreamModels();
                MeetingResult result = _localRecordLive.StartMp4Record(videoStreamModels, audioStreamModels);

                if (result.StatusCode != 0)
                {
                    MessageQueueManager.Instance.AddError(result.Message);
                    LocalRecordTriggerItem.IsChecked = false;
                    LocalRecordTriggerItem.Tips = null;
                }
                else
                {
                    LocalRecordTriggerItem.Tips =
                        string.Format(
                            $"分辨率：{_localRecordLive.RecordParam.LiveParameter.Width}*{_localRecordLive.RecordParam.LiveParameter.Height}\r\n" +
                            $"码率：{_localRecordLive.RecordParam.LiveParameter.VideoBitrate}\r\n" +
                            $"录制路径：{_localRecordLive.RecordDirectory}");
                }
            }
            else
            {
                MeetingResult result = _localRecordLive.StopMp4Record();
                LocalRecordTriggerItem.Tips = null;

                if (result.StatusCode != 0)
                {
                    MessageQueueManager.Instance.AddError(result.Message);
                    LocalRecordTriggerItem.IsChecked = true;
                }
            }

            _upFocusedUiElement?.Focus();
        }

        private AudioStreamModel[] GetAudioStreamModels()
        {
            List<AudioStreamModel> audioStreamModels = new List<AudioStreamModel>();

            var selfAudioResources = _windowManager.Participant.Resources.Where(res => res.IsUsed = true &&
            (res.MediaType == MediaType.AudioCaptureCard || res.MediaType == MediaType.AudioDoc || res.MediaType == MediaType.Microphone));

            foreach (var selfAudioRes in selfAudioResources)
            {
                AudioStreamModel audioStreamModel = new AudioStreamModel()
                {
                    AccountId = _windowManager.Participant.Account.AccountId.ToString(),
                    StreamId = selfAudioRes.ResourceId,
                    BitsPerSameple = 16,
                    Channels = 2,
                    SampleRate = 48000,
                };

                audioStreamModels.Add(audioStreamModel);
            }


            foreach (var participant in _windowManager.Participants)
            {
                var otherAudioResources = participant.Resources.Where(res => res.IsUsed &&
                (res.MediaType == MediaType.AudioCaptureCard || res.MediaType == MediaType.AudioDoc || res.MediaType == MediaType.Microphone));

                foreach (var otherAudioRes in otherAudioResources)
                {
                    AudioStreamModel audioStreamModel = new AudioStreamModel()
                    {
                        AccountId = participant.Account.AccountId.ToString(),
                        StreamId = otherAudioRes.ResourceId,
                        BitsPerSameple = 16,
                        Channels = 2,
                        SampleRate = 48000,
                    };

                    audioStreamModels.Add(audioStreamModel);
                }
            }

            return audioStreamModels.ToArray();
        }

        private void PushLiveAsync()
        {
            _autoHideInitialTime = DateTime.Now;

            _upFocusedUiElement = _intoClassView.PushLiveTriggerButton;

            if (!HasStreams())
            {
                PushLiveTriggerItem.IsChecked = !PushLiveTriggerItem.IsChecked;
                MessageQueueManager.Instance.AddError("当前没有音视频流，无法推流！");
                return;
            }

            if (PushLiveTriggerItem.IsChecked)
            {
                var pushrt = _manualPushLive.GetLiveParam();
                if (!pushrt)
                {
                    MessageQueueManager.Instance.AddError("推流参数未正确配置！");
                    PushLiveTriggerItem.IsChecked = false;
                    PushLiveTriggerItem.Tips = null;
                    return;
                }

                IGetLiveVideoCoordinate liveVideoCoordinate = _windowManager as IGetLiveVideoCoordinate;
                System.Windows.Size size = new System.Windows.Size()
                {
                    Width = _manualPushLive.LiveParam.LiveParameter.Width,
                    Height = _manualPushLive.LiveParam.LiveParameter.Height,
                };

                VideoStreamModel[] videoStreamModels = liveVideoCoordinate.GetVideoStreamModels(size);

                MeetingResult result =
                        _manualPushLive.StartPushLiveStream(videoStreamModels, GetAudioStreamModels(),
                        _manualPushLive.LiveParam.LiveParameter.Url1);

                if (result.StatusCode != 0)
                {
                    MessageQueueManager.Instance.AddError(result.Message);
                    PushLiveTriggerItem.IsChecked = false;
                    PushLiveTriggerItem.Tips = null;
                }
                else
                {
                    PushLiveTriggerItem.Tips =
                        string.Format(
                            $"分辨率：{_manualPushLive.LiveParam.LiveParameter.Width}*{_manualPushLive.LiveParam.LiveParameter.Height}\r\n" +
                            $"码率：{_manualPushLive.LiveParam.LiveParameter.VideoBitrate}\r\n" +
                            $"推流地址：{_manualPushLive.LiveParam.LiveParameter.Url1}");
                }
            }
            else
            {
                MeetingResult result = _manualPushLive.StopPushLiveStream();
                PushLiveTriggerItem.Tips = null;

                if (result.StatusCode != 0)
                {
                    MessageQueueManager.Instance.AddError(result.Message);
                    PushLiveTriggerItem.IsChecked = true;
                }
            }

            _upFocusedUiElement?.Focus();
        }

        public void SwitchCamera()
        {
            //var availableDevices = new List<string>();
            //try
            //{
            //    var cameraList = _meetingService.GetDevices(1).ToList();
            //    if (cameraList.Count <= 1)
            //    {
            //        MessageQueueManager.Instance.AddWarning("没有辅助摄像头，不能实现切换");
            //        return;
            //    }
            //    var docCameraList = _meetingService.GetDevices(2).ToList();
            //    var defaultDocCamera = docCameraList.FirstOrDefault(o => o.IsDefault)?.Name;
            //    var defaultCameraName = cameraList.FirstOrDefault(o => o.IsDefault)?.Name;
            //    foreach (var camera in cameraList)
            //    {
            //        if (!string.IsNullOrEmpty(camera.Name) && camera.Name != defaultDocCamera &&
            //            camera.Name != defaultCameraName)
            //        {
            //            availableDevices.Add(camera.Name);
            //        }
            //    }
            //    if (availableDevices.Any())
            //    {
            //        var targetDevice = availableDevices[0];
            //        var dialog = new Dialog($"当前摄像头：{defaultCameraName}，是否切换到摄像头：{targetDevice}", "是", "否");
            //        var result = dialog.ShowDialog();
            //        if (result.HasValue && result.Value)
            //        {
            //            _meetingService.SetDefaultDevice(1, targetDevice);
            //        }
            //    }
            //    else
            //    {
            //        MessageQueueManager.Instance.AddWarning("没有辅助摄像头，不能实现切换");
            //    }


            //}
            //catch (Exception ex)
            //{
            //    MessageQueueManager.Instance.AddWarning("切换摄像头异常，切换失败");
            //    Log.Logger.Error($"切换摄像头发生异常 exception：{ex}");
            //}
        }


        private async Task LoadedAsync()
        {
            if (App.VideoControl == null)
            {
                App.VideoControl = new VideoControl();
            }

            _intoClassView.Grid.Children.Add(App.VideoControl);

            await IntoClassAsync();

            InitAutoHideSettings();
            CalculateToggleButtonWidth();
            SetupUiBasedOnIsMainSpeaker();

        }

        private async Task IntoClassAsync()
        {
            int meetingId = 0;

            switch (_intoClassView.IntoClassType)
            {
                case IntoClassType.Create:
                    var createResult = await _meetingSdkAgent.CreateMeeting("");

                    if (createResult.StatusCode != 0)
                    {
                        MainView mainView = new MainView();
                        mainView.Show();

                        _intoClassView.Close();
                        MessageQueueManager.Instance.AddError($"创建失败！{createResult.StatusCode}");
                        return;
                    }

                    meetingId = createResult.Result.MeetingId;
                    AppCache.AddOrUpdate(CacheKey.MeetingId, meetingId);

                    object inviteesObj = AppCache.TryGet(CacheKey.Invitees);
                    if (inviteesObj != null)
                    {
                        var invitees = inviteesObj as IEnumerable<MeetingSdk.Wpf.Participant>;

                        if (invitees.Count() > 0)
                        {
                            List<int> inviteeIds = new List<int>();

                            foreach (var invitee in invitees)
                            {
                                inviteeIds.Add(invitee.Account.AccountId);
                            }

                            var inviteResult = await _meetingSdkAgent.MeetingInvite(meetingId, inviteeIds.ToArray());

                            if (inviteResult.StatusCode != 0)
                            {
                                MessageQueueManager.Instance.AddError("邀请听课教室失败！");
                            }
                        }
                    }

                    var meetingList = _localDataManager.GetMeetingList() ??
                  new MeetingList() { MeetingInfos = new List<MeetingItem>() };


                    var cachedMeeting = meetingList.MeetingInfos.FirstOrDefault(meeting => meeting.MeetingId == meetingId);

                    if (cachedMeeting!=null)
                    {
                        cachedMeeting.LastActivityTime = DateTime.Now;
                    }
                    else
                    {
                        meetingList.MeetingInfos.Add(new MeetingItem()
                        {
                            LastActivityTime = DateTime.Now,
                            MeetingId = meetingId,
                            CreatorName = GlobalData.Instance.Classroom.SchoolRoomName,
                            IsClose = false,
                            CreatorId = GlobalData.Instance.Classroom.SchoolRoomNum,
                            CreateTime = DateTime.Now,
                        });
                    }

                    _localDataManager.SaveMeetingList(meetingList);

                    break;
                case IntoClassType.Join:

                    object meetingIdObj = AppCache.TryGet(CacheKey.MeetingId);

                    if (meetingIdObj != null && int.TryParse(meetingIdObj.ToString(), out meetingId))
                    {
                        var joinResult = await _meetingSdkAgent.JoinMeeting(meetingId, true);

                        if (joinResult.StatusCode != 0)
                        {
                            if (joinResult.StatusCode == -2014)
                            {
                                MessageQueueManager.Instance.AddError("该课程已经结束！");
                            }
                            else
                            {
                                MessageQueueManager.Instance.AddError("加入课程失败！");
                            }
                        }
                    }
                    else
                    {
                        MainView mainView = new MainView();
                        mainView.Show();

                        _intoClassView.Close();
                        MessageQueueManager.Instance.AddError("课程号无效！");
                    }

                    break;
            }

            CourseNo = meetingId.ToString();

            SetScreenSize();

            await _windowManager.Join(meetingId, true);

            
            //判断是否启用双屏
            if (IsCreator && ExtendedScreenHelper.Instance.IsExtendedMode())
            {
                var mainScreen = ExtendedScreenHelper.Instance.Screens.FirstOrDefault(o => o.Type == 1);
                ExtendedScreenHelper.Instance.IsDoubleScreenOn = true;

                if (mainScreen?.Width != null)
                    _extendedScreenView = new ExtendedScreenView
                    {
                        Top = 0,
                        Left = ExtendedScreenHelper.Instance.ExtendScreenPosition,
                        Width = ExtendedScreenHelper.Instance.ExtendScreenWidth,
                        Height = ExtendedScreenHelper.Instance.ExtendScreenHeight

                    };

                _extendedScreenView.WindowState = WindowState.Normal;
                Log.Logger.Information($"扩展屏信息4：{ExtendedScreenHelper.Instance.ExtendScreenPosition}");
                _extendedScreenView.WindowStartupLocation = WindowStartupLocation.Manual;
                _extendedScreenView.Show();

                _extendedWindowManager.ShowExtendedView(_extendedScreenView);

                Log.Logger.Information($"扩展屏信息7：{_extendedScreenView.ActualWidth}{_extendedScreenView.ActualHeight}");
                GlobalData.Instance.ExtendedViewArea = new ViewArea()
                {
                    Width = ExtendedScreenHelper.Instance.ExtendScreenWidth,
                    Height = ExtendedScreenHelper.Instance.ExtendScreenHeight
                };
                _intoClassView.Left = 0;
                _intoClassView.Top = 0;
            }

            GlobalData.Instance.Course = new Course();

            //判断是否是从课表进入且是主讲，并且该课堂开启了录制功能
            if (GlobalData.Instance.Course != null && IsCreator && GlobalData.Instance.Course.IsRecord)
            {
                _remoteRecord.AddRecord();
                LocalRecordTriggerItem.IsChecked = true;
                _remoteRecord.IsRemoteRecord = true;
            }

            //如果是课堂创建者，改变课堂模式标记
            if (IsCreator)
            {
                RecordingSystemService.Instance.SetControlComand(ControlComand.MainClassRoomMode);
            }

            UpdateAttendeesCount();
        }

        private void SetScreenSize()
        {
            IScreen screen = _windowManager.VideoBoxManager as IScreen;
            screen.Size = new System.Windows.Size(_intoClassView.ActualWidth, _intoClassView.ActualHeight);
        }

        private void InitAutoHideSettings()
        {
            _AutoHideMenuTimer = new Timer((state) =>
            {
                _intoClassView.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _IsWindowActive = _intoClassView.IsActive;
                }));

                if (DateTime.Now > _autoHideInitialTime.AddSeconds(10) && _IsWindowActive)
                {
                    if (ShowTopBottomMenus)
                    {
                        ShowTopBottomMenus = false;
                    }
                }
            }, null, 0, 4000);
        }

        /// <summary>
        /// 加入会议，将主屏句柄传递给sdk
        /// 如果是从课表进入，并且是主讲开启自动录制
        /// </summary>
        /// <returns></returns>
        private void JoinMeetingAsync()
        {

            //await CreateAndJoinClassAsync();
            //try
            //{
            //    await _windowManager.Join(int.Parse(AppCache.TryGet(CacheKey.MeetingId).ToString()), IsCreator);

            //    //set user to speak automatically

            //}
            //catch (Exception ex)
            //{

            //}




            //GlobalData.Instance.ViewArea = new ViewArea()
            //{
            //    Width = _intoClassView.ActualWidth,
            //    Height = _intoClassView.ActualHeight
            //};

            //uint[] uint32SOfNonDataArray =
            //{
            //    (uint) _intoClassView.PictureBox1.Handle.ToInt32(),
            //    (uint) _intoClassView.PictureBox2.Handle.ToInt32(),
            //    (uint) _intoClassView.PictureBox3.Handle.ToInt32(),
            //    (uint) _intoClassView.PictureBox4.Handle.ToInt32(),
            //};

            //foreach (var hwnd in uint32SOfNonDataArray)
            //{
            //    Log.Logger.Debug($"【figure hwnd】：{hwnd}");
            //}

            //uint[] uint32SOfDataArray = { (uint)_intoClassView.PictureBox5.Handle.ToInt32() };

            //foreach (var hwnd in uint32SOfDataArray)
            //{
            //    Log.Logger.Debug($"【data hwnd】：{hwnd}");
            //}

            ////判断是否启用双屏
            //if (_meetingService.IsCreator && _viewLayoutService.IsExtendedMode())
            //{
            //    var mainScreen = _viewLayoutService.Screens.FirstOrDefault(o => o.Type == 1);
            //    _viewLayoutService.IsDoubleScreenOn = true;

            //    if (mainScreen?.Width != null)
            //        _extendedScreenView = new ExtendedScreenView
            //        {
            //            Top = 0,
            //            Left = _viewLayoutService.ExtendScreenPosition,
            //            Width = _viewLayoutService.ExtendScreenWidth,
            //            Height = _viewLayoutService.ExtendScreenHeight

            //        };
            //    _extendedScreenView.WindowState = WindowState.Normal;
            //    Log.Logger.Information($"扩展屏信息4：{_viewLayoutService.ExtendScreenPosition}");
            //    _extendedScreenView.WindowStartupLocation = WindowStartupLocation.Manual;
            //    _extendedScreenView.Show();

            //    Log.Logger.Information($"扩展屏信息7：{_extendedScreenView.ActualWidth}{_extendedScreenView.ActualHeight}");
            //    GlobalData.Instance.ExtendedViewArea = new ViewArea()
            //    {
            //        Width = _viewLayoutService.ExtendScreenWidth,
            //        Height = _viewLayoutService.ExtendScreenHeight
            //    };
            //    _intoClassView.Left = 0;
            //    _intoClassView.Top = 0;
            //}

            //var joinMeetingMessage =
            //    await _meetingService.JoinMeeting(_meetingService.MeetingId,
            //        uint32SOfNonDataArray, uint32SOfNonDataArray.Length,
            //        uint32SOfDataArray,
            //        uint32SOfDataArray.Length);

            //if (joinMeetingMessage.HasError)
            //{
            //    var mainView = new MainView();
            //    mainView.Show();
            //    _intoClassView.Close();
            //    _extendedScreenView?.Close();
            //    MessageQueueManager.Instance.AddError(joinMeetingMessage.Message);
            //}
            //ShowTopBottomMenus = true;
            ////判断是否是从课表进入且是主讲，并且该课堂开启了录制功能
            //if (GlobalData.Instance.Course != null && IsCreator && GlobalData.Instance.Course.IsRecord)
            //{
            //    _remoteRecord.AddRecord();
            //    LocalRecordTriggerItem.IsChecked = true;
            //    _remoteRecord.IsRemoteRecord = true;
            //}

            ////如果是课堂创建者，改变课堂模式标记
            //if (IsCreator)
            //{
            //    RecordingSystemService.Instance.SetControlComand(ControlComand.MainClassRoomMode);
            //}
        }

        private void CalculateToggleButtonWidth()
        {
            double totalAvailableWidth = _intoClassView.ActualWidth - 100;

            int toggleButtonsCount = IsCreator ? 5 : 4;

            CalculatedToggleButtonWidth = totalAvailableWidth / toggleButtonsCount - 20;
        }

        private void SetupUiBasedOnIsMainSpeaker()
        {
            MainSpeakerFunctionalityVisibility = IsCreator
                ? Visibility.Visible
                : Visibility.Collapsed;
            SetUpDefaultFocusedBottomButton();
        }

        private void SetUpDefaultFocusedBottomButton()
        {
            _downFocusedUiElement = IsCreator
                ? _intoClassView.ClassModeToggleButton
                : _intoClassView.StartStopSpeakToggleButton;

            _downFocusedUiElement.Focus();
        }


        private double _calculatedToggleButtonWidth;

        public double CalculatedToggleButtonWidth
        {
            get { return _calculatedToggleButtonWidth; }
            set { SetProperty(ref _calculatedToggleButtonWidth, value); }
        }

        public ICommand PushLiveCommand { get; set; }
        public ICommand LocalRecordCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand KeyDownCommand { get; set; }
        public ICommand GotoExitMeetingCommand { get; set; }
        public ICommand SetDoubleScreenOnOrOffCommand { get; set; }
        public ICommand DownButtonGotFocusCommand { get; set; }
        public ICommand UpButtonGotFocusCommand { get; set; }


        private string _pushLiveTips;

        public string PushLiveTips
        {
            get { return _pushLiveTips; }
            set { SetProperty(ref _pushLiveTips, value); }
        }

        private string _localRecordTips;

        public string LocalRecordTips
        {
            get { return _localRecordTips; }
            set { SetProperty(ref _localRecordTips, value); }
        }

        private bool _classModeChecked;

        public bool ClassModeChecked
        {
            get { return _classModeChecked; }
            set { SetProperty(ref _classModeChecked, value); }
        }

        public ICommand GotoClassModeCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _autoHideInitialTime = DateTime.Now;
                    _downFocusedUiElement = _intoClassView.ClassModeToggleButton;

                    ClassModeView classModeView = new ClassModeView();
                    classModeView.ShowDialog();
                    _autoHideInitialTime = DateTime.Now;

                    ClassModeChecked = false;
                    _downFocusedUiElement.Focus();

                });
            }
        }

        public ICommand StartStopSpeakCommand
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    _autoHideInitialTime = DateTime.Now;

                    _downFocusedUiElement = _intoClassView.StartStopSpeakToggleButton;

                    switch (SpeakingStatus)
                    {
                        case IsNotSpeaking:



                            var applyToSpeakMsg = await _meetingSdkAgent.AskForSpeak();
                            if (applyToSpeakMsg.StatusCode != 0)
                            {
                                MessageQueueManager.Instance.AddError(applyToSpeakMsg.Message);
                            }

                            break;
                        case IsSpeaking:
                            var stopSpeakMsg = await _meetingSdkAgent.AskForStopSpeak();
                            if (stopSpeakMsg.StatusCode != 0)
                            {
                                MessageQueueManager.Instance.AddError(stopSpeakMsg.Message);
                            }
                            //         var attendItem = _attendeeViews.FirstOrDefault(
                            //o => o.Participant.PhoneId == GlobalData.Instance.Classroom.SchoolRoomNum);
                            //         _attendeeViews.Remove(attendItem);
                            break;
                    }

                    _downFocusedUiElement.Focus();
                });
            }
        }

        private bool _pictureModeChecked;

        public bool PictureModeChecked
        {
            get { return _pictureModeChecked; }
            set { SetProperty(ref _pictureModeChecked, value); }
        }

        public ICommand GotoPictureModeCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _autoHideInitialTime = DateTime.Now;

                    _downFocusedUiElement = _intoClassView.PictureModeToggleButton;

                    PictureModeView pictureModeView = new PictureModeView();
                    pictureModeView.ShowDialog();
                    _autoHideInitialTime = DateTime.Now;

                    PictureModeChecked = false;
                    _downFocusedUiElement.Focus();
                });
            }
        }

        private bool _invitationChecked;

        public bool InvitationChecked
        {
            get { return _invitationChecked; }
            set { SetProperty(ref _invitationChecked, value); }
        }

        public ICommand GotoInvitationCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _autoHideInitialTime = DateTime.Now;

                    _downFocusedUiElement = _intoClassView.InvitationToggleButton;

                    InviteAttendeeView inviteAttendeeView = new InviteAttendeeView();
                    inviteAttendeeView.ShowDialog();
                    _autoHideInitialTime = DateTime.Now;

                    InvitationChecked = false;
                    _downFocusedUiElement.Focus();
                });
            }
        }

        private bool _attendeesChecked;

        public bool AttendeesChecked
        {
            get { return _attendeesChecked; }
            set { SetProperty(ref _attendeesChecked, value); }
        }

        public ICommand GotoAttendeesCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _autoHideInitialTime = DateTime.Now;

                    _downFocusedUiElement = _intoClassView.AttendeesToggleButton;

                    AttendeeListView attendeeListView = new AttendeeListView();
                    attendeeListView.ShowDialog();
                    _autoHideInitialTime = DateTime.Now;

                    AttendeesChecked = false;
                    _downFocusedUiElement.Focus();
                });
            }
        }

        private Visibility _mainSpeakerFunctionalityVisibility;

        public Visibility MainSpeakerFunctionalityVisibility
        {
            get { return _mainSpeakerFunctionalityVisibility; }
            set { SetProperty(ref _mainSpeakerFunctionalityVisibility, value); }
        }

        private bool _showTopBottomMenus;

        public bool ShowTopBottomMenus
        {
            get { return _showTopBottomMenus; }
            set
            {
                //if (value)
                //{
                //    _showTopBottomMenus = false;
                //}

                SetProperty(ref _showTopBottomMenus, value);
            }
        }

        public ClassModeItem ClassModeStatus { get; set; }
        public ToggleButtonItem PushLiveTriggerItem { get; set; }
        public ToggleButtonItem LocalRecordTriggerItem { get; set; }

        private string _courseNo;
        public string CourseNo
        {
            get
            {
                return _courseNo;
            }
            set
            {
                SetProperty(ref _courseNo, value);
            }
        }

        public bool IsCreator
        {
            get
            {
                return AppCache.TryGet(CacheKey.HostId).ToString() == _windowManager.Participant.Account.AccountId.ToString();
            }
        }

        private string _speakingStatus = IsNotSpeaking;

        public string SpeakingStatus
        {
            get { return _speakingStatus; }
            set { SetProperty(ref _speakingStatus, value); }
        }

        private System.Windows.Media.Brush _backgroundBrush;

        public System.Windows.Media.Brush BackgroundBrush
        {
            get { return _backgroundBrush; }
            set { SetProperty(ref _backgroundBrush, value); }
        }


        public ViewFrame ViewFrame1 { get; private set; }
        public ViewFrame ViewFrame2 { get; private set; }
        public ViewFrame ViewFrame3 { get; private set; }
        public ViewFrame ViewFrame4 { get; private set; }
        public ViewFrame ViewFrame5 { get; private set; }

        private void KickedByHostEventHandler(KickoutUserModel obj)
        {
            // close IntoClassView and handle related operations.
        }


        private void UIMessageReceivedEventHandler(TransparentMsg message)
        {
            //int msgId;
            //if (int.TryParse(message.Data, out msgId) && msgId < 3)
            //{
            //    AppCache.AddOrUpdate(CacheKey.HostId, message.SenderAccountId);

            //    var classMode = (ModeDisplayerType)msgId;

            //    ModeDisplayerFactory.Factory.CurrentModeDisplayerType = classMode;

            //}
        }

        private void UpdateAttendeesCount()
        {
            var attendees = _meetingSdkAgent.GetParticipants();
            _intoClassView.BeginInvoke(new Action(() =>
            {
                _intoClassView.AttendeesToggleButton.Content = $"教室列表({attendees.Result.Count})";
            }
            ));
        }

        private void OtherExitMeetingEventHandler(AccountModel contactInfo)
        {
            var attendee =
                GlobalData.Instance.Classrooms.FirstOrDefault(userInfo => userInfo.SchoolRoomNum == contactInfo.AccountId.ToString());

            var displayName = string.Empty;
            if (!string.IsNullOrEmpty(attendee?.SchoolRoomName))
            {
                displayName = attendee.SchoolRoomName + " - ";
            }

            string exitMsg = $"{displayName}{contactInfo.AccountId}退出会议！";

            MessageQueueManager.Instance.AddInfo(exitMsg);

            UpdateAttendeesCount();

            object hostIdObj = AppCache.TryGet(CacheKey.HostId);

            int hostId;
            if (hostIdObj != null && int.TryParse(hostIdObj.ToString(), out hostId))
            {
                if (hostId == contactInfo.AccountId)
                {
                    if (_windowManager.ModeChange(ModeDisplayerType.InteractionMode))
                    {

                    }
                }
            }
        }

        private void OtherJoinMeetingEventHandler(AccountModel contactInfo)
        {
            var attendee =
                GlobalData.Instance.Classrooms.FirstOrDefault(userInfo => userInfo.SchoolRoomNum == contactInfo.AccountId.ToString());

            string displayName = string.Empty;
            if (!string.IsNullOrEmpty(attendee?.SchoolRoomName))
            {
                displayName = attendee.SchoolRoomName + " - ";
            }

            string joinMsg = $"{displayName}{contactInfo.AccountId}加入会议！";

            MessageQueueManager.Instance.AddInfo(joinMsg);

            UpdateAttendeesCount();

            //speaker automatically sends a message(with creatorPhoneId) to nonspeakers
            //!!!CAREFUL!!! ONLY speaker will call this
            if (IsCreator)
            {
                _meetingSdkAgent.AsynSendUIMsg((int)_windowManager.ModeDisplayerStore.CurrentModeDisplayerType, contactInfo.AccountId, "");
            }
        }


        private void StopSpeakEventHandler(SpeakModel obj)
        {
            if (IsCreator)
            {
                if (_windowManager.ModeChange(ModeDisplayerType.InteractionMode))
                {
                }
            }

            //if (_windowManager.LayoutChange(WindowNames.MainWindow, LayoutRenderType.AutoLayout))
            //{
            //}
            //if (_windowManager.LayoutChange(WindowNames.ExtendedWindow, LayoutRenderType.AutoLayout))
            //{
            //}

            AppCache.AddOrUpdate(CacheKey.IsDocOpen, false);

            SpeakingStatus = IsNotSpeaking;
        }

        private void StartSpeakEventHandler(SpeakModel obj)
        {
            Console.WriteLine("StartSpeakEventHandler");
            SpeakingStatus = IsSpeaking;
        }

        private async Task SetDoubleScreenOnOrOff()
        {
            await App.Current.Dispatcher.BeginInvoke(new Action(() =>
             {
                 //只有主讲可以扩展屏幕
                 if (!IsCreator) return;
                 var message = ExtendedScreenHelper.Instance.IsDoubleScreenOn ? "双屏渲染已经开启，是否关闭？" : "双屏渲染已经关闭，是否开启？";
                 var confirm = new Dialog($"{message}", "是", "否");
                 var result = confirm.ShowDialog();
                 if (!result.HasValue || !result.Value) return;
                 //开始扩展模式
                 ExtendedScreenHelper.Instance.IsDoubleScreenOn = !ExtendedScreenHelper.Instance.IsDoubleScreenOn;
                 if (!ExtendedScreenHelper.Instance.IsDoubleScreenOn)
                 {
                     _extendedWindowManager.CloseExtendedView();
                 }
                 else
                 {
                     _extendedScreenView = new ExtendedScreenView
                     {
                         Top = 0,
                         Left = ExtendedScreenHelper.Instance.ExtendScreenPosition,
                         Width = ExtendedScreenHelper.Instance.ExtendScreenWidth,
                         Height = ExtendedScreenHelper.Instance.ExtendScreenHeight,
                         WindowState = WindowState.Normal
                     };
                     Log.Logger.Information($"扩展屏信息3：{ExtendedScreenHelper.Instance.ExtendScreenPosition}");
                     _extendedScreenView.Show();
                     _extendedWindowManager.ShowExtendedView(_extendedScreenView);
                 }
             }));

        }
    }
}
