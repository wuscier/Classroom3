using MeetingSdk.Wpf;

namespace Common.Model.ViewLayout
{
    public enum SpecialViewType
    {
        Big,
        FullScreen
    }

    public delegate void ClassModeChanged(ModeDisplayerType classMode);
    public delegate void PictureModeChanged(LayoutRenderType pictureMode);

}
