using System.Windows.Input;
using WindowsInput.Native;
using Classroom.ViewModel;
using Common.Helper;

namespace Classroom.View
{
    /// <summary>
    /// CreateClassView.xaml 的交互逻辑
    /// </summary>
    public partial class CreateClassView
    {
        public CreateClassView()
        {
            InitializeComponent();
            DataContext = new CreateClassViewModel(this);
        }

        public override void EscapeKeyDownHandler()
        {
            MainView mainView = new MainView();
            mainView.Show();
            Close();
        }

        private void CreateClassView_OnPreviewKeyDown(object sender, KeyEventArgs e)
            {
            switch (e.Key)
            {
                case Key.Left:
                    InputSimulatorManager.Instance.InputSimu.Keyboard.ModifiedKeyStroke(VirtualKeyCode.SHIFT,
                        VirtualKeyCode.TAB);
                    e.Handled = true;
                    break;
                case Key.Right:
                    CreateClassButton.Focus();
                    e.Handled = true;
                    break;
            }
        }
    }
}
