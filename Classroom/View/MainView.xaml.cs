using Classroom.ViewModel;

namespace Classroom.View
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView
    {
        public MainView()
        {
            InitializeComponent();
            DataContext = new MainViewModel(this);
        }

        public MainView(string msg)
        {
            InitializeComponent();
            DataContext = new MainViewModel(this, msg);
        }
    }
}