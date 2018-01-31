using Classroom.View;
using Common.Helper;
using System.Linq;
using System.Windows;

namespace Classroom.ViewModel
{
    public class SwichModeViewModel
    {
        // public string Name { get; set; }

        public static void SwitchMode()
        {
            //如果是在课堂中，则切换模式，不进行跳转
            var currentWindow = Application.Current.Windows;
            var window = currentWindow[0];
            var mainView = new MainView();
            mainView.Show();
            window?.Close();
            if (GlobalData.Instance.CurrentMode?.Name == "键盘")
            {
                GlobalData.Instance.CurrentMode = GlobalData.Instance.ModeList.FirstOrDefault(o => o.Name == "自动");
            }
            if (GlobalData.Instance.CurrentMode?.Name == "自动")
            {
                GlobalData.Instance.CurrentMode = GlobalData.Instance.ModeList.FirstOrDefault(o => o.Name == "键盘");
            }
        }
    }
}
