using System.Windows;
using Classroom.ViewModel;
using Common.Helper;
using System.Windows.Controls;

namespace Classroom.View
{
    /// <summary>
    /// BaseInfoSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class BaseInfoSettingView
    {
        public BaseInfoSettingView()
        {
            InitializeComponent();
            DataContext = new BaseInfoSettingModel(this);
        }
        public override void EscapeKeyDownHandler()
        {
            var settingNavView = new SettingNavView();
            settingNavView.Show();
            Close();
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

        private void TxtOnGotFocus(object sender, RoutedEventArgs e)
        {
            var textbox = (TextBox)sender;
            if (!string.IsNullOrEmpty(textbox?.Text))
                textbox.SelectionStart = textbox.Text.Length;
        }
    }
}
