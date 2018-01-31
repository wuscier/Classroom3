using System.Windows;
using System.Windows.Input;
using WindowsInput.Native;
using Classroom.ViewModel;
using Common.Helper;

namespace Classroom.View
{
    /// <summary>
    /// PictureModeView.xaml 的交互逻辑
    /// </summary>
    public partial class PictureModeView : Window
    {
        public PictureModeView()
        {
            InitializeComponent();
            DataContext = new PictureModeViewModel();
            Loaded += PictureModeView_Loaded;
        }

        private void PictureModeView_Loaded(object sender, RoutedEventArgs e)
        {
            InputSimulatorManager.Instance.InputSimu.Keyboard.KeyPress(VirtualKeyCode.TAB);
        }

        private void ClassModeView_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    e.Handled = true;
                    Close();
                    break;
            }
        }

    }
}
