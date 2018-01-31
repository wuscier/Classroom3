using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Common.Contract;
using Common.Helper;
using Common.Model.ViewLayout;
using Common.Model;
using Common.UiMessage;
using GM.Utilities;
using MeetingSdk.SdkWrapper;
using MeetingSdk.SdkWrapper.MeetingDataModel;
using Serilog;

namespace Service
{
    public class ViewLayoutService : IViewLayout
    {
        private readonly IMeeting _meetingService;
        private readonly IPushLive _manualPushLive;
        private readonly IPushServerLive _serverPushLive;
        private readonly IRecordLive _localRecordLive;



        private static readonly double Columns = 30;
        private static readonly double Rows = 10;


        public ViewLayoutService()
        {
            _meetingService = DependencyResolver.Current.GetService<IMeeting>();

            _manualPushLive = DependencyResolver.Current.Container.ResolveNamed<IPushLive>("ManualPushLive");
            _serverPushLive = DependencyResolver.Current.Container.ResolveNamed<IPushServerLive>("ServerPushLive");
            _localRecordLive = DependencyResolver.Current.GetService<IRecordLive>();

            InitializeStatus();


        }

        public List<ViewFrame> ViewFrameList { get; set; }
        public List<ViewFrame> ExtenedViewFrameList { get; set; }
        public event ClassModeChanged ClassModeChangedEvent;
        public event PictureModeChanged PictureModeChangedEvent;
        public event Action LayoutChangedEvent;

        private ClassMode _classMode;

        public ClassMode ClassMode
        {
            get { return _classMode; }
            private set
            {
                _classMode = value;
                ClassModeChangedEvent?.Invoke(value);
            }
        }

        public List<ScreenModel> Screens { get; set; }
        public bool IsDoubleScreenOn { get; set; }
        public bool IsSpearking { get; set; }
        public int ExtendScreenPosition { get; set; }

        public int ExtendScreenWidth { get; set; }

        public int ExtendScreenHeight { get; set; }

        private PictureMode _pictureMode;

        public PictureMode PictureMode
        {
            get { return _pictureMode; }
            private set
            {
                _pictureMode = value;
                PictureModeChangedEvent?.Invoke(value);
            }
        }

        public ViewFrame FullScreenView { get; private set; }


        public List<LiveVideoStream> GetStreamLayout(int resolutionWidth, int resolutionHeight)
        {
            var viewFramesVisible =
                ViewFrameList.Where(viewFrame => viewFrame.IsOpened && viewFrame.Visibility == Visibility.Visible);

            var viewFramesByDesending = viewFramesVisible.OrderBy(viewFrame => viewFrame.ViewOrder);

            var orderViewFrames = viewFramesByDesending.ToList();

            List<LiveVideoStream> liveVideoStreamInfos = new List<LiveVideoStream>();


            foreach (var orderViewFrame in orderViewFrames)
            {
                LiveVideoStream newLiveVideoStreamInfo = new LiveVideoStream();
                RefreshLiveLayout(ref newLiveVideoStreamInfo, orderViewFrame, resolutionWidth, resolutionHeight);
                liveVideoStreamInfos.Add(newLiveVideoStreamInfo);
            }

            return liveVideoStreamInfos;
        }

        private void RefreshLiveLayout(ref LiveVideoStream liveVideoStreamInfo, ViewFrame viewFrame,
            int resolutionWidth, int resolutionHeight)
        {
            liveVideoStreamInfo.Handle = (uint)viewFrame.Hwnd;

            liveVideoStreamInfo.X = (int)((viewFrame.Column / Columns) * resolutionWidth);
            liveVideoStreamInfo.Width = (int)((viewFrame.ColumnSpan / Columns) * resolutionWidth);

            liveVideoStreamInfo.Y = (int)((viewFrame.Row / Rows) * resolutionHeight);
            liveVideoStreamInfo.Height = (int)((viewFrame.RowSpan / Rows) * resolutionHeight);
        }

        public async Task LaunchLayout()
        {
            Log.Logger.Debug($"ViewLayout=>pictureMode={PictureMode}, classMode={ClassMode}");

            switch (PictureMode)
            {
                //case ViewMode.Auto:
                default:
                    switch (ClassMode)
                    {
                        //模式优先级 高于 画面布局，选择一个模式将会重置布局为自动
                        //在某种模式下，用户可以随意更改布局
                        case ClassMode.InteractionMode:
                            await LaunchAverageLayout();
                            break;
                        case ClassMode.SpeakerMode:
                            await GotoSpeakerMode();
                            break;
                        case ClassMode.ShareMode:
                            await GotoSharingMode();
                            break;
                    }
                    break;
                case PictureMode.AverageMode:
                    await LaunchAverageLayout();
                    break;
                case PictureMode.BigSmallsMode:
                    await LaunchBigSmallLayout();
                    break;
                case PictureMode.CloseupMode:
                    await LaunchCloseUpLayout();
                    break;
            }

            LayoutChangedEvent?.Invoke();
            await StartOrRefreshLiveAsync();
            //刷新扩展屏布局
            if (IsDoubleScreenOn)
                ExtentdedLaunchLayout();
        }

        public void SetSpecialView(ViewFrame view, SpecialViewType type)
        {
            switch (type)
            {
                case SpecialViewType.Big:
                    SetBigView(view);
                    break;
                case SpecialViewType.FullScreen:
                    SetFullScreenView(view);
                    break;
            }
        }

        public void ChangeClassMode(ClassMode targetClassMode)
        {
            if (ClassMode != targetClassMode)
            {
                ClassMode = targetClassMode;
            }
        }

        public void ChangePictureMode(PictureMode targetPictureMode)
        {
            if (PictureMode != targetPictureMode)
            {
                PictureMode = targetPictureMode;
            }
        }

        public void ResetAsAutoLayout()
        {
            ViewFrameList.ForEach(viewFrame => { viewFrame.IsBigView = false; });

            FullScreenView = null;

            PictureMode = PictureMode.AutoMode;
        }

        public void ResetAsInitialStatus()
        {
            MakeAllViewsInvisible();
            InitializeStatus();
        }

        public async Task ShowViewAsync(ParticipantView view)
        {
            Log.Logger.Debug($"ViewLayout=>phoneId={view.Participant.PhoneId}, name={view.Participant.Name}, type={view.ViewType}, hwnd={view.Hwnd}");
            var viewFrameVisible = ViewFrameList.FirstOrDefault(viewFrame => viewFrame.Hwnd == view.Hwnd);

            if (viewFrameVisible != null)
            {
                viewFrameVisible.IsOpened = true;
                viewFrameVisible.Visibility = Visibility.Visible;
                viewFrameVisible.PhoneId = view.Participant.PhoneId;


                var attendee =
                    GlobalData.Instance.Classrooms.FirstOrDefault(
                        classroom => classroom.SchoolRoomNum == view.Participant.PhoneId);

                string displayName = string.Empty;
                if (!string.IsNullOrEmpty(attendee?.SchoolRoomName))
                {
                    displayName = attendee.SchoolRoomName;
                }

                viewFrameVisible.ViewName = view.ViewType == 1
                    ? displayName
                    : $"(课件){displayName}";

                viewFrameVisible.ViewType = view.ViewType;
                viewFrameVisible.ViewOrder = ViewFrameList.Max(viewFrame => viewFrame.ViewOrder) + 1;
            }
            await LaunchLayout();
        }


        public async Task HideViewAsync(ParticipantView view)
        {
            ResetFullScreenView(view);

            var viewFrameInvisible = ViewFrameList.FirstOrDefault(viewFrame => viewFrame.Hwnd == view.Hwnd);

            if (viewFrameInvisible != null)
            {
                // LOG return a handle which can not be found in handle list.

                viewFrameInvisible.IsOpened = false;
                viewFrameInvisible.Visibility = Visibility.Collapsed;
            }

            await LaunchLayout();

        }



        private void InitializeStatus()
        {
            ViewFrameList = new List<ViewFrame>();

            ClassMode = ClassMode.InteractionMode;

            PictureMode = PictureMode.AutoMode;
            FullScreenView = null;
            IsDoubleScreenOn = false;
            var count = 0;
            var participants = _meetingService.GetParticipants();
            if (participants != null)
                count = participants.Count(p => p.PhoneId == _meetingService.SelfPhoneId);

            if (count == 0)
                ViewFrameList.Clear();

        }



        private void MakeAllViewsInvisible()
        {
            ViewFrameList.ForEach(viewFrame => { viewFrame.Visibility = Visibility.Collapsed; });
        }


        private async Task LaunchAverageLayout()
        {
            await Task.Run(() =>
             {
                 var viewFramesVisible = ViewFrameList.Where(viewFrame => viewFrame.IsOpened);

                 var viewFramesByDesending = viewFramesVisible.OrderBy(viewFrame => viewFrame.ViewOrder);

                 var orderViewFrames = viewFramesByDesending.ToList();
                 switch (orderViewFrames.Count)
                 {
                     case 0:
                         //displays a picture
                         break;
                     case 1:
                         var viewFrameFull = orderViewFrames[0];
                         viewFrameFull.Visibility = Visibility.Visible;
                         viewFrameFull.Row = 0;
                         viewFrameFull.RowSpan = 10;
                         viewFrameFull.Column = 0;
                         viewFrameFull.ColumnSpan = 30;

                         viewFrameFull.Width = GlobalData.Instance.ViewArea.Width;
                         viewFrameFull.Height = GlobalData.Instance.ViewArea.Height;
                         viewFrameFull.VerticalAlignment = VerticalAlignment.Center;
                         Log.Logger.Debug($"LaunchAverageLayout=> phoneId={viewFrameFull.PhoneId}, name={viewFrameFull.ViewName}, hwnd={viewFrameFull.Hwnd}, width={viewFrameFull.Width}, height={viewFrameFull.Height}");

                         break;

                     case 2:
                         var viewFrameLeft2 = orderViewFrames[0];
                         var viewFrameRight2 = orderViewFrames[1];

                         viewFrameLeft2.Visibility = Visibility.Visible;
                         viewFrameLeft2.Row = 0;
                         viewFrameLeft2.RowSpan = 10;
                         viewFrameLeft2.Column = 0;
                         viewFrameLeft2.ColumnSpan = 15;
                         viewFrameLeft2.Width = GlobalData.Instance.ViewArea.Width / 2;
                         viewFrameLeft2.Height = GlobalData.Instance.ViewArea.Height / 2;
                         viewFrameLeft2.VerticalAlignment = VerticalAlignment.Center;

                         viewFrameRight2.Visibility = Visibility.Visible;
                         viewFrameRight2.Row = 0;
                         viewFrameRight2.RowSpan = 10;
                         viewFrameRight2.Column = 15;
                         viewFrameRight2.ColumnSpan = 15;
                         viewFrameRight2.Width = GlobalData.Instance.ViewArea.Width / 2;
                         viewFrameRight2.Height = GlobalData.Instance.ViewArea.Height / 2;
                         viewFrameRight2.VerticalAlignment = VerticalAlignment.Center;

                         Log.Logger.Debug($"LaunchAverageLayout=>left => phoneId={viewFrameLeft2.PhoneId}, name={viewFrameLeft2.ViewName}, hwnd={viewFrameLeft2.Hwnd}, width={viewFrameLeft2.Width}, height={viewFrameLeft2.Height}");
                         Log.Logger.Debug($"LaunchAverageLayout=>right => phoneId={viewFrameRight2.PhoneId}, name={viewFrameRight2.ViewName}, hwnd={viewFrameRight2.Hwnd}, width={viewFrameRight2.Width}, height={viewFrameRight2.Height}");

                         break;
                     case 3:

                         var viewFrameLeft3 = orderViewFrames[0];
                         var viewFrameRight3 = orderViewFrames[1];
                         var viewFrameBottom3 = orderViewFrames[2];


                         viewFrameLeft3.Visibility = Visibility.Visible;
                         viewFrameLeft3.Row = 0;
                         viewFrameLeft3.RowSpan = 5;
                         viewFrameLeft3.Column = 0;
                         viewFrameLeft3.ColumnSpan = 15;
                         viewFrameLeft3.Width = GlobalData.Instance.ViewArea.Width / 2;
                         viewFrameLeft3.Height = GlobalData.Instance.ViewArea.Height / 2;
                         viewFrameLeft3.VerticalAlignment = VerticalAlignment.Center;

                         viewFrameRight3.Visibility = Visibility.Visible;
                         viewFrameRight3.Row = 0;
                         viewFrameRight3.RowSpan = 5;
                         viewFrameRight3.Column = 15;
                         viewFrameRight3.ColumnSpan = 15;
                         viewFrameRight3.Width = GlobalData.Instance.ViewArea.Width / 2;
                         viewFrameRight3.Height = GlobalData.Instance.ViewArea.Height / 2;
                         viewFrameRight3.VerticalAlignment = VerticalAlignment.Center;

                         viewFrameBottom3.Visibility = Visibility.Visible;
                         viewFrameBottom3.Row = 5;
                         viewFrameBottom3.RowSpan = 5;
                         viewFrameBottom3.Column = 0;
                         viewFrameBottom3.ColumnSpan = 15;
                         viewFrameBottom3.Width = GlobalData.Instance.ViewArea.Width / 2;
                         viewFrameBottom3.Height = GlobalData.Instance.ViewArea.Height / 2;
                         viewFrameBottom3.VerticalAlignment = VerticalAlignment.Center;

                         break;
                     case 4:
                         var viewFrameLeftTop4 = orderViewFrames[0];
                         var viewFrameRightTop4 = orderViewFrames[1];
                         var viewFrameLeftBottom4 = orderViewFrames[2];
                         var viewFrameRightBottom4 = orderViewFrames[3];

                         viewFrameLeftTop4.Visibility = Visibility.Visible;
                         viewFrameLeftTop4.Row = 0;
                         viewFrameLeftTop4.RowSpan = 5;
                         viewFrameLeftTop4.Column = 0;
                         viewFrameLeftTop4.ColumnSpan = 15;
                         viewFrameLeftTop4.Width = GlobalData.Instance.ViewArea.Width / 2;
                         viewFrameLeftTop4.Height = GlobalData.Instance.ViewArea.Height / 2;
                         viewFrameLeftTop4.VerticalAlignment = VerticalAlignment.Center;

                         viewFrameRightTop4.Visibility = Visibility.Visible;
                         viewFrameRightTop4.Row = 0;
                         viewFrameRightTop4.RowSpan = 5;
                         viewFrameRightTop4.Column = 15;
                         viewFrameRightTop4.ColumnSpan = 15;
                         viewFrameRightTop4.Width = GlobalData.Instance.ViewArea.Width / 2;
                         viewFrameRightTop4.Height = GlobalData.Instance.ViewArea.Height / 2;
                         viewFrameRightTop4.VerticalAlignment = VerticalAlignment.Center;

                         viewFrameLeftBottom4.Visibility = Visibility.Visible;
                         viewFrameLeftBottom4.Row = 5;
                         viewFrameLeftBottom4.RowSpan = 5;
                         viewFrameLeftBottom4.Column = 0;
                         viewFrameLeftBottom4.ColumnSpan = 15;
                         viewFrameLeftBottom4.Width = GlobalData.Instance.ViewArea.Width / 2;
                         viewFrameLeftBottom4.Height = GlobalData.Instance.ViewArea.Height / 2;
                         viewFrameLeftBottom4.VerticalAlignment = VerticalAlignment.Center;

                         viewFrameRightBottom4.Visibility = Visibility.Visible;
                         viewFrameRightBottom4.Row = 5;
                         viewFrameRightBottom4.RowSpan = 5;
                         viewFrameRightBottom4.Column = 15;
                         viewFrameRightBottom4.ColumnSpan = 15;
                         viewFrameRightBottom4.Width = GlobalData.Instance.ViewArea.Width / 2;
                         viewFrameRightBottom4.Height = GlobalData.Instance.ViewArea.Height / 2;
                         viewFrameRightBottom4.VerticalAlignment = VerticalAlignment.Center;

                         break;
                     case 5:

                         #region 三托二


                         #endregion

                         #region 平均排列，两行三列

                         var viewFrameLeftTop5 = orderViewFrames[0];
                         var viewFrameMiddleTop5 = orderViewFrames[1];
                         var viewFrameRightTop5 = orderViewFrames[2];
                         var viewFrameLeftBottom5 = orderViewFrames[3];
                         var viewFrameRightBottom5 = orderViewFrames[4];

                         viewFrameLeftTop5.Visibility = Visibility.Visible;
                         viewFrameLeftTop5.Row = 0;
                         viewFrameLeftTop5.RowSpan = 5;
                         viewFrameLeftTop5.Column = 0;
                         viewFrameLeftTop5.ColumnSpan = 10;
                         viewFrameLeftTop5.Width = GlobalData.Instance.ViewArea.Width * 0.3333;
                         viewFrameLeftTop5.Height = GlobalData.Instance.ViewArea.Width * 0.1875;
                         viewFrameLeftTop5.VerticalAlignment = VerticalAlignment.Bottom;

                         viewFrameMiddleTop5.Visibility = Visibility.Visible;
                         viewFrameMiddleTop5.Row = 0;
                         viewFrameMiddleTop5.RowSpan = 5;
                         viewFrameMiddleTop5.Column = 10;
                         viewFrameMiddleTop5.ColumnSpan = 10;
                         viewFrameMiddleTop5.Width = GlobalData.Instance.ViewArea.Width * 0.3333;
                         viewFrameMiddleTop5.Height = GlobalData.Instance.ViewArea.Width * 0.1875;
                         viewFrameMiddleTop5.VerticalAlignment = VerticalAlignment.Bottom;


                         viewFrameRightTop5.Visibility = Visibility.Visible;
                         viewFrameRightTop5.Row = 0;
                         viewFrameRightTop5.RowSpan = 5;
                         viewFrameRightTop5.Column = 20;
                         viewFrameRightTop5.ColumnSpan = 10;
                         viewFrameRightTop5.Width = GlobalData.Instance.ViewArea.Width * 0.3333;
                         viewFrameRightTop5.Height = GlobalData.Instance.ViewArea.Width * 0.1875;
                         viewFrameRightTop5.VerticalAlignment = VerticalAlignment.Bottom;

                         viewFrameLeftBottom5.Visibility = Visibility.Visible;
                         viewFrameLeftBottom5.Row = 5;
                         viewFrameLeftBottom5.RowSpan = 5;
                         viewFrameLeftBottom5.Column = 0;
                         viewFrameLeftBottom5.ColumnSpan = 10;
                         viewFrameLeftBottom5.Width = GlobalData.Instance.ViewArea.Width * 0.3333;
                         viewFrameLeftBottom5.Height = GlobalData.Instance.ViewArea.Width * 0.1875;
                         viewFrameLeftBottom5.VerticalAlignment = VerticalAlignment.Top;

                         viewFrameRightBottom5.Visibility = Visibility.Visible;
                         viewFrameRightBottom5.Row = 5;
                         viewFrameRightBottom5.RowSpan = 5;
                         viewFrameRightBottom5.Column = 10;
                         viewFrameRightBottom5.ColumnSpan = 10;
                         viewFrameRightBottom5.Width = GlobalData.Instance.ViewArea.Width * 0.3333;
                         viewFrameRightBottom5.Height = GlobalData.Instance.ViewArea.Width * 0.1875;
                         viewFrameRightBottom5.VerticalAlignment = VerticalAlignment.Top;

                         #endregion

                         break;
                     default:

                         // LOG count of view frames is not between 0 and 5 
                         break;
                 }
             });
        }



        private async Task LaunchBigSmallLayout()
        {
            var viewFramesVisible = ViewFrameList.Where(viewFrame => viewFrame.IsOpened);
            var framesVisible = viewFramesVisible as ViewFrame[] ?? viewFramesVisible.ToArray();
            if (framesVisible.Length <= 1)
            {
                await GotoDefaultMode();
                return;
            }

            var bigViewFrame = framesVisible.FirstOrDefault(viewFrame => viewFrame.IsBigView);
            if (bigViewFrame == null)
            {
                await GotoDefaultMode();
                return;
            }

            bigViewFrame.Visibility = Visibility.Visible;
            bigViewFrame.Row = 1;
            bigViewFrame.RowSpan = 8;
            bigViewFrame.Column = 0;
            bigViewFrame.ColumnSpan = 24;
            bigViewFrame.Width = GlobalData.Instance.ViewArea.Width * 0.8;
            bigViewFrame.Height = GlobalData.Instance.ViewArea.Width * 0.45;
            bigViewFrame.VerticalAlignment = VerticalAlignment.Center;

            Log.Logger.Debug($"LaunchBigSmallLayout=>big => phoneId={bigViewFrame.PhoneId}, name={bigViewFrame.ViewName}, hwnd={bigViewFrame.Hwnd}, width={bigViewFrame.Width}, height={bigViewFrame.Height}");


            var smallViewFrames = framesVisible.Where(viewFrame => !viewFrame.IsBigView);
            var row = 1;
            foreach (var frame in smallViewFrames.OrderBy(viewFrame => viewFrame.ViewOrder))
            {
                if (row > 7)
                    break;

                frame.Visibility = Visibility.Visible;
                frame.Row = row;
                frame.RowSpan = 2;
                frame.Column = 24;
                frame.ColumnSpan = 6;
                frame.Width = GlobalData.Instance.ViewArea.Width * 0.2;
                frame.Height = GlobalData.Instance.ViewArea.Width * 0.1125;
                frame.VerticalAlignment = VerticalAlignment.Center;

                Log.Logger.Debug($"LaunchBigSmallLayout=>small => phoneId={frame.PhoneId}, name={frame.ViewName}, hwnd={frame.Hwnd}, width={frame.Width}, height={frame.Height}");


                row += 2;
            }
        }

        #region 扩展双屏相关方法(扩展屏定位窗口通过视讯号确定)


        public void ShowExtendedViewAsync(List<ParticipantView> views)
        {
            views.ForEach(x =>
            {
                if (ExtenedViewFrameList.Any(o => o.PhoneId == x.Participant.PhoneId)) return;

                var viewFrameVisible = ExtenedViewFrameList.ToList().FirstOrDefault(o => string.IsNullOrEmpty(o.PhoneId));
                if (viewFrameVisible != null)
                {
                    viewFrameVisible.IsOpened = true;
                    viewFrameVisible.Visibility = Visibility.Visible;
                    viewFrameVisible.PhoneId = x.Participant.PhoneId;

                    var attendee =
                        GlobalData.Instance.Classrooms.FirstOrDefault(
                            classroom => classroom.SchoolRoomNum == x.Participant.PhoneId);

                    var displayName = string.Empty;
                    if (!string.IsNullOrEmpty(attendee?.SchoolRoomName))
                    {
                        displayName = attendee.SchoolRoomName;
                    }

                    viewFrameVisible.ViewName = x.ViewType == 1
                        ? displayName
                        : $"(课件){displayName}";

                    viewFrameVisible.ViewType = x.ViewType;
                    viewFrameVisible.ViewOrder = ViewFrameList.Max(viewFrame => viewFrame.ViewOrder) + 1;
                }
            });
            ExtentdedLaunchLayout();

            foreach (ViewFrame orderedView in ExtenedViewFrameList.Where(o => o.IsOpened))
            {
                try
                {
                    Log.Logger.Debug($"渲染双屏 phoneId={orderedView.PhoneId}, hwnd={orderedView.Hwnd}");
                    var result = _meetingService.SetDoubleScreenRender(orderedView.PhoneId, orderedView.ViewType,
                            IsDoubleScreenOn ? 1 : 0, orderedView.Hwnd);
                    Log.Logger.Debug($"渲染结果{orderedView.PhoneId},结果{result.Status}");
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"{ex}");
                }
            }
        }

        public void StopExtendedViewAsync()
        {
            foreach (ViewFrame orderedView in ExtenedViewFrameList.Where(o => !string.IsNullOrEmpty(o.PhoneId)))
            {
                try
                {
                    Log.Logger.Debug($"停止渲染双屏{orderedView.PhoneId}{orderedView.Hwnd}");
                    _meetingService.SetDoubleScreenRender(orderedView.PhoneId, orderedView.ViewType,
                        0, orderedView.Hwnd);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"{ex}");
                }
            }


        }

        public void HideExtendedViewAsync(ParticipantView view)
        {
            if (view?.Participant == null || ExtenedViewFrameList == null)
            {
                ExtentdedLaunchLayout();
                return;
            }
            var viewFrameInvisible =
                ExtenedViewFrameList.FirstOrDefault(viewFrame => viewFrame.PhoneId == view.Participant.PhoneId);

            if (viewFrameInvisible != null)
            {
                // LOG return a handle which can not be found in handle list.
                viewFrameInvisible.IsOpened = false;
                viewFrameInvisible.Visibility = Visibility.Collapsed;
                viewFrameInvisible.PhoneId = null;
                // ExtenedViewFrameList.Remove(viewFrameInvisible);
            }

        }




        public void ExtentdedLaunchLayout()
        {
            ExtendedLaunchAverageLayout();
            Log.Logger.Debug($"当前模式{ClassMode}");
            LayoutChangedEvent?.Invoke();
        }

        private void ExtendedLaunchAverageLayout()
        {

            if ((int)GlobalData.Instance.ExtendedViewArea.Width % 16 != 0 || (int)GlobalData.Instance.ExtendedViewArea.Height % 9 != 0)
            {
                GlobalData.Instance.ExtendedViewArea.Height = GlobalData.Instance.ExtendedViewArea.Width / 16 * 9;
            }

            Log.Logger.Debug($"扩展屏宽度高度---{GlobalData.Instance.ExtendedViewArea.Width}---{GlobalData.Instance.ExtendedViewArea.Height}");
            var viewFramesVisible = ExtenedViewFrameList.Where(o => !string.IsNullOrEmpty(o.PhoneId) && o.IsOpened);
            var viewFramesByDesending = viewFramesVisible.OrderBy(viewFrame => viewFrame.ViewOrder);

            var orderViewFrames = viewFramesByDesending.ToList();
            Log.Logger.Debug($"总数{orderViewFrames.Count}");
            viewFramesVisible.ToList().ForEach(x =>
            {
                Log.Logger.Debug($"视讯号{x.PhoneId};句柄{x.Hwnd}");
            });

            switch (orderViewFrames.Count)
            {
                case 0:
                    //displays a picture
                    break;
                case 1:
                    var viewFrameFull = orderViewFrames[0];
                    viewFrameFull.Visibility = Visibility.Visible;
                    viewFrameFull.Row = 0;
                    viewFrameFull.RowSpan = 10;
                    viewFrameFull.Column = 0;
                    viewFrameFull.ColumnSpan = 30;

                    viewFrameFull.Width = GlobalData.Instance.ExtendedViewArea.Width;
                    viewFrameFull.Height = GlobalData.Instance.ExtendedViewArea.Height;
                    viewFrameFull.VerticalAlignment = VerticalAlignment.Center;
                    Log.Logger.Debug($"ExtendedLaunchAverageLayout=> phoneId={viewFrameFull.PhoneId}, name={viewFrameFull.ViewName}, hwnd={viewFrameFull.Hwnd}, width={viewFrameFull.Width}, height={viewFrameFull.Height}");

                    break;


                case 2:
                    var viewFrameLeft2 = orderViewFrames[0];
                    var viewFrameRight2 = orderViewFrames[1];

                    viewFrameLeft2.Visibility = Visibility.Visible;
                    viewFrameLeft2.Row = 0;
                    viewFrameLeft2.RowSpan = 10;
                    viewFrameLeft2.Column = 0;
                    viewFrameLeft2.ColumnSpan = 15;
                    viewFrameLeft2.Width = GlobalData.Instance.ExtendedViewArea.Width / 2;
                    viewFrameLeft2.Height = GlobalData.Instance.ExtendedViewArea.Height / 2;
                    viewFrameLeft2.VerticalAlignment = VerticalAlignment.Center;

                    viewFrameRight2.Visibility = Visibility.Visible;
                    viewFrameRight2.Row = 0;
                    viewFrameRight2.RowSpan = 10;
                    viewFrameRight2.Column = 15;
                    viewFrameRight2.ColumnSpan = 15;
                    viewFrameRight2.Width = GlobalData.Instance.ExtendedViewArea.Width / 2;
                    viewFrameRight2.Height = GlobalData.Instance.ExtendedViewArea.Height / 2;
                    viewFrameRight2.VerticalAlignment = VerticalAlignment.Center;
                    Log.Logger.Debug($"ExtendedLaunchAverageLayout=>left => phoneId={viewFrameLeft2.PhoneId}, name={viewFrameLeft2.ViewName}, hwnd={viewFrameLeft2.Hwnd}, width={viewFrameLeft2.Width}, height={viewFrameLeft2.Height}");
                    Log.Logger.Debug($"ExtendedLaunchAverageLayout=>right => phoneId={viewFrameRight2.PhoneId}, name={viewFrameRight2.ViewName}, hwnd={viewFrameRight2.Hwnd}, width={viewFrameRight2.Width}, height={viewFrameRight2.Height}");

                    break;
                case 3:

                    var viewFrameLeft3 = orderViewFrames[0];
                    var viewFrameRight3 = orderViewFrames[1];
                    var viewFrameBottom3 = orderViewFrames[2];


                    viewFrameLeft3.Visibility = Visibility.Visible;
                    viewFrameLeft3.Row = 0;
                    viewFrameLeft3.RowSpan = 5;
                    viewFrameLeft3.Column = 0;
                    viewFrameLeft3.ColumnSpan = 15;
                    viewFrameLeft3.Width = GlobalData.Instance.ExtendedViewArea.Width / 2;
                    viewFrameLeft3.Height = GlobalData.Instance.ExtendedViewArea.Height / 2;
                    viewFrameLeft3.VerticalAlignment = VerticalAlignment.Bottom;

                    viewFrameRight3.Visibility = Visibility.Visible;
                    viewFrameRight3.Row = 0;
                    viewFrameRight3.RowSpan = 5;
                    viewFrameRight3.Column = 15;
                    viewFrameRight3.ColumnSpan = 15;
                    viewFrameRight3.Width = GlobalData.Instance.ExtendedViewArea.Width / 2;
                    viewFrameRight3.Height = GlobalData.Instance.ExtendedViewArea.Height / 2;
                    viewFrameRight3.VerticalAlignment = VerticalAlignment.Bottom;

                    viewFrameBottom3.Visibility = Visibility.Visible;
                    viewFrameBottom3.Row = 5;
                    viewFrameBottom3.RowSpan = 5;
                    viewFrameBottom3.Column = 0;
                    viewFrameBottom3.ColumnSpan = 15;
                    viewFrameBottom3.Width = GlobalData.Instance.ExtendedViewArea.Width / 2;
                    viewFrameBottom3.Height = GlobalData.Instance.ExtendedViewArea.Height / 2;
                    viewFrameBottom3.VerticalAlignment = VerticalAlignment.Top;

                    break;
                case 4:
                    var viewFrameLeftTop4 = orderViewFrames[0];
                    var viewFrameRightTop4 = orderViewFrames[1];
                    var viewFrameLeftBottom4 = orderViewFrames[2];
                    var viewFrameRightBottom4 = orderViewFrames[3];

                    viewFrameLeftTop4.Visibility = Visibility.Visible;
                    viewFrameLeftTop4.Row = 0;
                    viewFrameLeftTop4.RowSpan = 5;
                    viewFrameLeftTop4.Column = 0;
                    viewFrameLeftTop4.ColumnSpan = 15;
                    viewFrameLeftTop4.Width = GlobalData.Instance.ExtendedViewArea.Width / 2;
                    viewFrameLeftTop4.Height = GlobalData.Instance.ExtendedViewArea.Height / 2;
                    viewFrameLeftTop4.VerticalAlignment = VerticalAlignment.Bottom;

                    viewFrameRightTop4.Visibility = Visibility.Visible;
                    viewFrameRightTop4.Row = 0;
                    viewFrameRightTop4.RowSpan = 5;
                    viewFrameRightTop4.Column = 15;
                    viewFrameRightTop4.ColumnSpan = 15;
                    viewFrameRightTop4.Width = GlobalData.Instance.ExtendedViewArea.Width / 2;
                    viewFrameRightTop4.Height = GlobalData.Instance.ExtendedViewArea.Height / 2;
                    viewFrameRightTop4.VerticalAlignment = VerticalAlignment.Bottom;

                    viewFrameLeftBottom4.Visibility = Visibility.Visible;
                    viewFrameLeftBottom4.Row = 5;
                    viewFrameLeftBottom4.RowSpan = 5;
                    viewFrameLeftBottom4.Column = 0;
                    viewFrameLeftBottom4.ColumnSpan = 15;
                    viewFrameLeftBottom4.Width = GlobalData.Instance.ExtendedViewArea.Width / 2;
                    viewFrameLeftBottom4.Height = GlobalData.Instance.ExtendedViewArea.Height / 2;
                    viewFrameLeftBottom4.VerticalAlignment = VerticalAlignment.Top;

                    viewFrameRightBottom4.Visibility = Visibility.Visible;
                    viewFrameRightBottom4.Row = 5;
                    viewFrameRightBottom4.RowSpan = 5;
                    viewFrameRightBottom4.Column = 15;
                    viewFrameRightBottom4.ColumnSpan = 15;
                    viewFrameRightBottom4.Width = GlobalData.Instance.ExtendedViewArea.Width / 2;
                    viewFrameRightBottom4.Height = GlobalData.Instance.ExtendedViewArea.Height / 2;
                    viewFrameRightBottom4.VerticalAlignment = VerticalAlignment.Top;

                    break;
                case 5:

                    #region 三托二

                    #endregion

                    #region 平均排列，两行三列

                    var viewFrameLeftTop5 = orderViewFrames[0];
                    var viewFrameMiddleTop5 = orderViewFrames[1];
                    var viewFrameRightTop5 = orderViewFrames[2];
                    var viewFrameLeftBottom5 = orderViewFrames[3];
                    var viewFrameRightBottom5 = orderViewFrames[4];

                    viewFrameLeftTop5.Visibility = Visibility.Visible;
                    viewFrameLeftTop5.Row = 0;
                    viewFrameLeftTop5.RowSpan = 5;
                    viewFrameLeftTop5.Column = 0;
                    viewFrameLeftTop5.ColumnSpan = 10;
                    viewFrameLeftTop5.Width = GlobalData.Instance.ExtendedViewArea.Width * 0.3333;
                    viewFrameLeftTop5.Height = GlobalData.Instance.ExtendedViewArea.Width * 0.1875;
                    viewFrameLeftTop5.VerticalAlignment = VerticalAlignment.Bottom;

                    viewFrameMiddleTop5.Visibility = Visibility.Visible;
                    viewFrameMiddleTop5.Row = 0;
                    viewFrameMiddleTop5.RowSpan = 5;
                    viewFrameMiddleTop5.Column = 10;
                    viewFrameMiddleTop5.ColumnSpan = 10;
                    viewFrameMiddleTop5.Width = GlobalData.Instance.ExtendedViewArea.Width * 0.3333;
                    viewFrameMiddleTop5.Height = GlobalData.Instance.ExtendedViewArea.Width * 0.1875;
                    viewFrameMiddleTop5.VerticalAlignment = VerticalAlignment.Bottom;


                    viewFrameRightTop5.Visibility = Visibility.Visible;
                    viewFrameRightTop5.Row = 0;
                    viewFrameRightTop5.RowSpan = 5;
                    viewFrameRightTop5.Column = 20;
                    viewFrameRightTop5.ColumnSpan = 10;
                    viewFrameRightTop5.Width = GlobalData.Instance.ExtendedViewArea.Width * 0.3333;
                    viewFrameRightTop5.Height = GlobalData.Instance.ExtendedViewArea.Width * 0.1875;
                    viewFrameRightTop5.VerticalAlignment = VerticalAlignment.Bottom;

                    viewFrameLeftBottom5.Visibility = Visibility.Visible;
                    viewFrameLeftBottom5.Row = 5;
                    viewFrameLeftBottom5.RowSpan = 5;
                    viewFrameLeftBottom5.Column = 0;
                    viewFrameLeftBottom5.ColumnSpan = 10;
                    viewFrameLeftBottom5.Width = GlobalData.Instance.ExtendedViewArea.Width * 0.3333;
                    viewFrameLeftBottom5.Height = GlobalData.Instance.ExtendedViewArea.Width * 0.1875;
                    viewFrameLeftBottom5.VerticalAlignment = VerticalAlignment.Top;

                    viewFrameRightBottom5.Visibility = Visibility.Visible;
                    viewFrameRightBottom5.Row = 5;
                    viewFrameRightBottom5.RowSpan = 5;
                    viewFrameRightBottom5.Column = 10;
                    viewFrameRightBottom5.ColumnSpan = 10;
                    viewFrameRightBottom5.Width = GlobalData.Instance.ExtendedViewArea.Width * 0.3333;
                    viewFrameRightBottom5.Height = GlobalData.Instance.ExtendedViewArea.Width * 0.1875;
                    viewFrameRightBottom5.VerticalAlignment = VerticalAlignment.Top;

                    #endregion

                    break;
                default:

                    // LOG count of view frames is not between 0 and 5 
                    break;
            }
        }

        #endregion



        private async Task LaunchCloseUpLayout()
        {
            if (FullScreenView == null)
            {
                await GotoDefaultMode();
                return;
            }

            ViewFrameList.ForEach(viewFrame =>
            {
                if (viewFrame.Hwnd != FullScreenView.Hwnd)
                    viewFrame.Visibility = Visibility.Collapsed;
            });

            FullScreenView.Visibility = Visibility.Visible;
            FullScreenView.Row = 0;
            FullScreenView.RowSpan = 10;
            FullScreenView.Column = 0;
            FullScreenView.ColumnSpan = 30;
            FullScreenView.Width = GlobalData.Instance.ViewArea.Width;
            FullScreenView.Height = GlobalData.Instance.ViewArea.Height;
            FullScreenView.VerticalAlignment = VerticalAlignment.Center;
        }



        public async Task GotoSpeakerMode()
        {
            // 主讲模式下，不会显示听讲者视图
            //1. 有主讲者视图和共享视图，主讲者大，共享小
            //2. 有主讲者，没有共享，主讲者全屏
            //3. 无主讲者，无法设置主讲模式【选择主讲模式时会校验】

            var speakerView =
                ViewFrameList.FirstOrDefault(
                    v =>
                        (v.PhoneId == _meetingService.CreatorPhoneId) && v.IsOpened &&
                        (v.ViewType == 1));
            if (speakerView == null)
            {
                await GotoDefaultMode();
                return;
            }

            var sharingView =
                ViewFrameList.FirstOrDefault(
                    v =>
                        (v.PhoneId == _meetingService.CreatorPhoneId) && v.IsOpened &&
                        (v.ViewType == 2));
            if (sharingView == null)
            {
                FullScreenView = speakerView;
                await LaunchCloseUpLayout();
                return;
            }

            SetBigView(speakerView);
            await LaunchBigSmallLayout();
        }

        public async Task GotoSharingMode()
        {
            // 共享模式下，不会显示听讲者视图【设置完共享源，将自动开启共享模式】
            //1. 有主讲者视图和共享视图，主讲者小，共享大
            //2. 无主讲者，有共享，共享全屏
            //3. 没有共享，无法设置共享模式【选择共享模式时会校验】

            var sharingView =
                ViewFrameList.FirstOrDefault(
                    v => (v.PhoneId == _meetingService.CreatorPhoneId) && v.IsOpened && (v.ViewType == 2));
            if (sharingView == null)
            {
                await GotoDefaultMode();
                return;
            }

            var speakerView =
                ViewFrameList.FirstOrDefault(
                    v => (v.PhoneId == _meetingService.CreatorPhoneId) && v.IsOpened && (v.ViewType == 1));
            if (speakerView == null)
            {
                FullScreenView = sharingView;
                await LaunchCloseUpLayout();
                return;
            }

            SetBigView(sharingView);

            await LaunchBigSmallLayout();
        }

        private async Task GotoDefaultMode()
        {
            ClassMode = ClassMode.InteractionMode;
            ResetAsAutoLayout();

            await LaunchLayout();
        }


        public bool IsExtendedMode()
        {
            try
            {
                System.Windows.Forms.Screen[] screens = System.Windows.Forms.Screen.AllScreens;
                int screenCount = screens.Length;
                Log.Logger.Information($"扩展屏数量：{screenCount}");
                if (screenCount <= 1)
                {
                    return false;
                }
                else
                {
                    Screens = new List<ScreenModel>();
                    Screens.Add(new ScreenModel()
                    {
                        X = screens[0].Bounds.X,
                        Y = screens[0].Bounds.Y,
                        Width = screens[0].Bounds.Width,
                        Height = screens[0].Bounds.Height,
                        Type = 1
                    });
                    Screens.Add(new ScreenModel()
                    {
                        X = screens[1].Bounds.X,
                        Y = screens[1].Bounds.Y,
                        Width = screens[1].Bounds.Width,
                        Height = screens[1].Bounds.Height,
                        Type = 2
                    });
                    Log.Logger.Information($"扩展屏信息1：{screens[0].Bounds.X},{screens[0].Bounds.Width},{screens[1].Bounds.X},{screens[1].Bounds.Width}");
                    var mainScreen = Screens.FirstOrDefault(o => o.X == 0);
                    var extendedScreen = Screens.FirstOrDefault(o => o.X != 0);
                    if (mainScreen == null || extendedScreen == null) return false;
                    mainScreen.Type = 1;
                    extendedScreen.Type = 2;
                    var result = true;
                    ExtendScreenPosition = extendedScreen.X >= 0 ? mainScreen.Width : extendedScreen.X;
                    ExtendScreenWidth = extendedScreen.Width;
                    ExtendScreenHeight = extendedScreen.Height;

                    //ExtendScreenWidth = 1680;
                    //ExtendScreenHeight = 945;

                    Log.Logger.Information($"扩展屏信息宽度与高度：{ExtendScreenWidth}---{ExtendScreenHeight}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        private void SetBigView(ViewFrame view)
        {
            ViewFrameList.ForEach(viewFrame => { viewFrame.IsBigView = viewFrame.Hwnd == view.Hwnd; });
        }

        private void SetFullScreenView(ViewFrame view)
        {
            FullScreenView = view;
        }

        private void ResetFullScreenView(ParticipantView toBeClosedView)
        {
            if ((FullScreenView != null) && (FullScreenView.PhoneId == toBeClosedView.Participant.PhoneId) &&
                (FullScreenView.ViewType == toBeClosedView.ViewType))
                FullScreenView = null;
        }

        private async Task StartOrRefreshLiveAsync()
        {

            try
            {
                if (ViewFrameList.Count(viewFrame => viewFrame.IsOpened && viewFrame.Visibility == Visibility.Visible) > 0)
                {
                    if (_manualPushLive.LiveId != 0)
                    {
                        await
                            _manualPushLive.RefreshLiveStream(GetStreamLayout(
                                _manualPushLive.LiveParam.Width,
                                _manualPushLive.LiveParam.Height));
                    }

                    if (_serverPushLive.LiveId != 0 && _serverPushLive.LiveParam != null)
                    {
                        await
                        _serverPushLive.RefreshLiveStream(
                            GetStreamLayout(_serverPushLive.LiveParam.Width,
                                _serverPushLive.LiveParam.Height));
                    }

                    if (_localRecordLive.RecordId != 0)
                    {

                        var reuslt = await
                              _localRecordLive.RefreshLiveStream(GetStreamLayout(_localRecordLive.RecordParam.Width,
                                  _localRecordLive.RecordParam.Height));
                        if (ClassMode == ClassMode.ShareMode)
                        {
                            Log.Logger.Information($"文档共享模式刷新流结果：{reuslt.Status}--{reuslt.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error("邀请进入失败" + ex.Message);
            }
        }
    }
}
