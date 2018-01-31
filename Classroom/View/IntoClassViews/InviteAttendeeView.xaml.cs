using System.Windows;
using System.Windows.Input;
using Classroom.ViewModel;

namespace Classroom.View
{
    /// <summary>
    /// InviteAttendeeView.xaml 的交互逻辑
    /// </summary>
    public partial class InviteAttendeeView : Window
    {
        public InviteAttendeeView()
        {
            InitializeComponent();
            DataContext = new InviteAttendeeViewModel();
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
