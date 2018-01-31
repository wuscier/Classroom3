using Classroom.ViewModel;

namespace Classroom.View
{
    /// <summary>
    /// ClassListView.xaml 的交互逻辑
    /// </summary>
    public partial class ClassListView
    {
        public ClassListView()
        {
            InitializeComponent();
            DataContext = new ClassListViewModel(this);
        }

        public override void EscapeKeyDownHandler()
        {
            var mainView = new MainView();
            mainView.Show();
            Close();


        }
    }
}
