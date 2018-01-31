using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Classroom.ViewModel;
using Common.Helper;

namespace Classroom.View
{
    /// <summary>
    /// NetworkSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class NetworkSettingView
    {
        private InputSimulatorManager _ism;
        public NetworkSettingView()
        {
            InitializeComponent();
            DataContext = new NetworkSettingModel(this);
            btn_netCheck.Focus();
            _ism = InputSimulatorManager.Instance;

        }

        public override void EscapeKeyDownHandler()
        {
            var settingNavView = new SettingNavView();
            settingNavView.Show();
            Close();
        }

        private void combox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            InputSimulatorManager.PreviewKeyDown(sender, e);
        }

        private void stack_GotFocus(object sender, RoutedEventArgs e)
        {
            InputSimulatorManager.GotFocus(sender);
        }

        private void Stack_OnLostFocus(object sender, RoutedEventArgs e)
        {
            InputSimulatorManager.LostFocus(sender);
        }

        private void stack_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var s = sender as StackPanel;
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.PageUp || e.Key == Key.PageDown)
            {
                if (s == null) return;
                switch (s.Name)
                {
                    case "stack_ip":
                        if (e.Key == Key.Down || e.Key == Key.PageDown)
                        {
                            txt_Mask_1.Focus();
                        }
                        else if (e.Key == Key.Up || e.Key == Key.PageUp)
                        {
                            cbb_netInfo.Focus();
                        }
                        e.Handled = true;
                        break;
                    case "stack_mask":
                        if (e.Key == Key.Down || e.Key == Key.PageDown)
                        {
                            txt_GateWay_1.Focus();
                        }
                        else if (e.Key == Key.Up || e.Key == Key.PageUp)
                        {
                            txt_IpAddress_1.Focus();
                        }
                        e.Handled = true;
                        break;
                    case "stack_GateWay":
                        if (e.Key == Key.Down || e.Key == Key.PageDown)
                        {
                            txt_Dns_1.Focus();
                        }
                        else if (e.Key == Key.Up || e.Key == Key.PageUp)
                        {
                            txt_Mask_1.Focus();
                        }
                        e.Handled = true;
                        break;
                    case "stack_Dns":
                        if (e.Key == Key.Down || e.Key == Key.PageDown)
                        {
                            btn_netCheck.Focus();
                        }
                        else if (e.Key == Key.Up || e.Key == Key.PageUp)
                        {
                            txt_GateWay_1.Focus();
                        }
                        e.Handled = true;
                        break;
                }
            }

        }

        private void textBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            _ism.TextBoxPreviewKeyDown(sender, e);
        }

        private void Rb_OnKeyDown(object sender, KeyEventArgs e)
        {
            var chk = sender as RadioButton;
            if (e.Key != Key.Return) return;
            if (chk != null) chk.IsChecked = true;
        }

        private void btnBack_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            InputSimulatorManager.VirtualPreviewKeyDown(sender, e);
        }
    }
}
