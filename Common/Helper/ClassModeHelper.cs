using MeetingSdk.Wpf;

namespace Common.Helper
{
    public class ClassModeHelper
    {
        public static string GetImageUrl(ModeDisplayerType classMode)
        {
            switch (classMode)
            {
                case ModeDisplayerType.SpeakerMode:
                    return "/Common;Component/Image/kt_zj.png";
                case ModeDisplayerType.ShareMode:
                    return "/Common;Component/Image/kt_kj.png";
                case ModeDisplayerType.InteractionMode:
                    return "/Common;Component/Image/kt_hd.png";
                default:
                    return "/Common;Component/Image/kt_hd.png";
            }
        }
    }
}
