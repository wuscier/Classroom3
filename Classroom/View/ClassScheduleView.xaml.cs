using Classroom.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WindowsInput.Native;
using Classroom.Service;
using Common.Helper;
using Common.Model;

namespace Classroom.View
{
    /// <summary>
    /// ClassScheduleView.xaml 的交互逻辑
    /// </summary>
    public partial class ClassScheduleView
    {
        private Button btn;
        public ClassScheduleView()
        {
            InitializeComponent();
            DataContext = new ClassScheduleModel(this);
        }

        public override void EscapeKeyDownHandler()
        {
            var mainView = new MainView();
            mainView.Show();
            Close();
        }

        private void Btn_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var focusedBtn = sender as Button;
            btn = focusedBtn;
            if (focusedBtn == null) return;
            var toolTip = (ToolTip)focusedBtn.ToolTip;
            var point = focusedBtn.PointToScreen(new Point());
            SetWindowsTop.SetCursorPos((int)point.X + 50, (int)point.Y + 30);
            if (toolTip == null) return;
            toolTip.IsOpen = true;
            toolTip.Background = null;
            toolTip.BorderBrush = null;
            var classToolTip = (CourseTipView)toolTip.Content;
            classToolTip.intoBtn.Focus();
            ToolTipService.SetShowDuration(toolTip, 3000);
        }

        private void Btn_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var focusedBtn = sender as Button;
            var toolTip = (ToolTip)focusedBtn?.ToolTip;
            if (toolTip == null) return;
            toolTip.IsOpen = false;
        }

        private void Windown_KeyDown(object sender, KeyEventArgs e)
        {
            var keyEventArgs = e as KeyEventArgs;
            switch (keyEventArgs?.Key)
            {
                case Key.Up:
                case Key.PageUp:
                    break;
                case Key.Down:
                case Key.PageDown:
                    break;
                case Key.Left:
                case Key.Home:
                    InputSimulatorManager.Instance.InputSimu.Keyboard.ModifiedKeyStroke(VirtualKeyCode.SHIFT, VirtualKeyCode.TAB);
                    keyEventArgs.Handled = true;
                    break;
                case Key.Right:
                case Key.End:
                    InputSimulatorManager.Instance.InputSimu.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    keyEventArgs.Handled = true;
                    break;
            }

        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn.Tag != null)
            {
                var course = (Course)btn.Tag;
                if (course.IsProcessing)
                {
                    btn.Focus();
                }
            }

        }
    }
}
