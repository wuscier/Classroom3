using System.Windows;
using System.Windows.Controls;
using Classroom.ViewModel;
using Common.Helper;

namespace Classroom.View
{
    /// <summary>
    /// LiveSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class LiveSettingView
    {
        public LiveSettingView()
        {
            InitializeComponent();
            DataContext = new LiveSettingModel(this);
            CbbDisplaySource.Focus();
        }

        public override void EscapeKeyDownHandler()
        {
            var settingNavView = new SettingNavView();
            settingNavView.Show();
            Close();
        }

        private void combox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is ComboBox)
            {
                InputSimulatorManager.PreviewKeyDown(sender, e);
            }

            if (sender is TextBox)
            {
                InputSimulatorManager.Instance.TextBoxPreviewKeyDown(sender, e);
            }
        }

        private void stack_GotFocus(object sender, RoutedEventArgs e)
        {
            InputSimulatorManager.GotFocus(sender);
        }

        private void Stack_OnLostFocus(object sender, RoutedEventArgs e)
        {
            InputSimulatorManager.LostFocus(sender);
        }

        private void btnBack_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            InputSimulatorManager.VirtualPreviewKeyDown(sender, e);
        }
    }
}
