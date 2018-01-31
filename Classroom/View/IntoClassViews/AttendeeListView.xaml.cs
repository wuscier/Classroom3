using System.Windows;
using System.Windows.Input;
using Classroom.ViewModel;

namespace Classroom.View
{
    /// <summary>
    /// AttendeeListView.xaml 的交互逻辑
    /// </summary>
    public partial class AttendeeListView : Window
    {
        public AttendeeListView()
        {
            InitializeComponent();
            DataContext = new AttendeeListViewModel();
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
