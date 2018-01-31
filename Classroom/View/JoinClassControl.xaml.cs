using System.Windows;
using System.Windows.Media;
using Classroom.ViewModel;
using MeetingSdk.NetAgent.Models;
using Common.Model;

namespace Classroom.View
{
    /// <summary>
    /// JoinClassControl.xaml 的交互逻辑
    /// </summary>
    public partial class JoinClassControl
    {
        public JoinClassControl(JoinClassView view, MeetingItem item)
        {
            InitializeComponent();
            DataContext = new JoinClassControlViewModel(view, this, item);
        }

        private void StackPanel_GotFocus(object sender, RoutedEventArgs e)
        {
            lab_Content.Foreground = new SolidColorBrush(Colors.Red);
            btn_into.Foreground = new SolidColorBrush(Colors.Red);

            this.grid_name.Background = (Brush)new BrushConverter().ConvertFromString("#e1ede9");
            this.btn_into.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#e1ede9");
        }

        private void StackPanel_LostFocus(object sender, RoutedEventArgs e)
        {
            lab_Content.Foreground = (Brush)new BrushConverter().ConvertFromString("#999999");
            btn_into.Foreground = (Brush)new BrushConverter().ConvertFromString("#999999");

            this.grid_name.Background = (Brush)new BrushConverter().ConvertFromString("#ffffff");
            this.btn_into.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#ffffff");
        }
    }
}
