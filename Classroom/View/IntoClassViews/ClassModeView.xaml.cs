using Classroom.ViewModel;
using System.Windows.Input;
using WindowsInput.Native;
using Common.Helper;

namespace Classroom.View
{
    /// <summary>
    /// ClassModeView.xaml 的交互逻辑
    /// </summary>
    public partial class ClassModeView
    {
        public ClassModeView()
        {
            InitializeComponent();
            DataContext = new ClassModeViewModel();
            Loaded += ClassModeView_Loaded;
        }

        private void ClassModeView_Loaded(object sender, System.Windows.RoutedEventArgs e)
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
