using Classroom.ViewModel;

namespace Classroom.View
{
    /// <summary>
    /// UpdatingView.xaml 的交互逻辑
    /// </summary>
    public partial class UpdatingView
    {
        public UpdatingView()
        {
            InitializeComponent();
            DataContext = new UpdatingViewModel(this);
        }
    }
}
