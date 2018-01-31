using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Model;
using Common.Model.ViewLayout;
using MeetingSdk.SdkWrapper.MeetingDataModel;

namespace Common.Contract
{
    public interface IViewLayout
    {
        List<ViewFrame> ViewFrameList { get; set; }
        List<ViewFrame> ExtenedViewFrameList { get; set; }
        List<ScreenModel> Screens { get; set; }

        event ClassModeChanged ClassModeChangedEvent;
        event PictureModeChanged PictureModeChangedEvent;
        event Action LayoutChangedEvent;

        ClassMode ClassMode { get; }
        PictureMode PictureMode { get; }
        ViewFrame FullScreenView { get; }
        bool IsDoubleScreenOn { get; set; }
        bool IsSpearking { get; set; }

        int ExtendScreenPosition { get; set; }
        int ExtendScreenWidth { get; set; }
        int ExtendScreenHeight { get; set; }

        List<LiveVideoStream> GetStreamLayout(int resolutionWidth, int resolutionHeight);

        Task LaunchLayout();
        void SetSpecialView(ViewFrame view, SpecialViewType type);
        void ChangeClassMode(ClassMode targetClassMode);
        void ChangePictureMode(PictureMode targetPictureMode);
        void ResetAsAutoLayout();
        void ResetAsInitialStatus();

        Task ShowViewAsync(ParticipantView view);
        Task HideViewAsync(ParticipantView view);
        bool IsExtendedMode();

        void ShowExtendedViewAsync(List<ParticipantView> view);
        void HideExtendedViewAsync(ParticipantView view);
        void StopExtendedViewAsync();



    }
}
