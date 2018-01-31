using System.Linq;
using System.Windows.Input;
using Classroom.ViewModel;
using Common.CustomControl;
using Common.Helper;

namespace Classroom.View
{
    /// <summary>
    /// WelcomeView.xaml 的交互逻辑
    /// </summary>
    public partial class WelcomeView
    {
        public WelcomeView()
        {
            InitializeComponent();
            DataContext = new WelcomeViewModel(this);
        }

        private void GotoMainView(object sender, ExecutedRoutedEventArgs e)
        {
            //获取当前操作模式，弹出切换模式窗口
            var modelList = GlobalData.Instance.ModeList;
            var currentModel = GlobalData.Instance.CurrentMode;
            var modelDialog = new Dialog($"您当前在{currentModel.Name}模式，是否要切换到{modelList.FirstOrDefault(o => o.Name != currentModel.Name)?.Name}模式？", "是", "否");
            var result = modelDialog.ShowDialog();
            if (!result.HasValue || !result.Value) return;
            //更新模式
            SwichModeViewModel.SwitchMode();

        }
    }
}
