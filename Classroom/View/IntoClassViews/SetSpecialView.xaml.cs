using System.Windows;
using System.Windows.Input;
using Classroom.ViewModel.IntoClassViewModels;
using Common.Helper;
using MeetingSdk.Wpf;

namespace Classroom.View.IntoClassViews
{
    /// <summary>
    /// SetSpecialView.xaml 的交互逻辑
    /// </summary>
    public partial class SetSpecialView : Window
    {
        public SetSpecialView(LayoutRenderType pictureMode)
        {
            InitializeComponent();
            DataContext = new SetSpecialViewModel(pictureMode);
        }

        private void ClassModeView_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    e.Handled = true;
                    Close();
                    break;
                case Key.Down:
                case Key.PageDown:
                    InputSimulatorManager.VirtualPreviewKeyDown(sender, e);
                    break;
                case Key.Up:
                case Key.PageUp:
                    InputSimulatorManager.VirtualPreviewKeyDown(sender, e);
                    break;

            }
        }
    }
}
