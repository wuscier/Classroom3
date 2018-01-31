using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Classroom.View;
using Common.Contract;
using Common.Helper;
using Prism.Commands;
using Prism.Mvvm;
using ClassroomModel = Common.Model.Classroom;

namespace Classroom.ViewModel
{
    public class ClassListViewModel : BindableBase
    {
        #region field

        private readonly ClassListView _listView;
        private int _totalClassRoom;
        private string _listInfo;

        #endregion

        #region property

        public string ListInfo
        {
            get { return _listInfo; }
            set { SetProperty(ref _listInfo, value); }
        }

        public string NumOrName { get; set; }

        public ObservableCollection<ClassroomModel> ClassroomList { get; set; }

        #endregion

        #region ctor
        public ClassListViewModel(ClassListView view)
        {
            _listView = view;
            ClassroomList = new ObservableCollection<ClassroomModel>();
            LoadCommand = DelegateCommand.FromAsyncHandler(LoadingAsync);
            SearchCommand = DelegateCommand.FromAsyncHandler(SearchAsync);
            GridDownKeyPressCommand = new DelegateCommand(GridDownKeyPress);
            GridUpKeyPressCommand = new DelegateCommand(GridUpKeyPress);
            GridLeftOrRightKeyPressCommand = new DelegateCommand(GridLeftOrRightKeyPress);
            GoBackCommand = new DelegateCommand(GoBack);
        }

        #endregion

        #region method


        private void GoBack()
        {
            var mainView = new MainView();
            mainView.Show();
            _listView.Close();
        }

        private async Task LoadingAsync()
        {
            await GetClassroomList();
            GridRowForcus(0);
        }


        private async Task SearchAsync()
        {
            await GetClassroomList(NumOrName);
        }

        private void GridDownKeyPress()
        {
            var grid = _listView.ClassesDataGrid;
            var nextIndex = grid.SelectedIndex == _totalClassRoom - 1 ? 0 : grid.SelectedIndex + 1;
            GridRowForcus(nextIndex);
        }

        private void GridUpKeyPress()
        {
            var grid = _listView.ClassesDataGrid;
            var nextIndex = grid.SelectedIndex == 0 ? _totalClassRoom - 1 : grid.SelectedIndex - 1;
            GridRowForcus(nextIndex);
        }

        private void GridLeftOrRightKeyPress()
        {

        }

        private void GridRowForcus(int index = 0)
        {
            if (_totalClassRoom <= 0) return;
            var grid = _listView.ClassesDataGrid;
            grid.SelectedIndex = index;
            grid.ScrollIntoView(grid.SelectedItem);
            grid.Focusable = true;
            grid.FocusVisualStyle = null;
            grid.Focus();
            ListInfo = $"当前第{index + 1}个共{_totalClassRoom}个";
        }


        private async Task GetClassroomList(string numOrName = "")
        {
            var selfClassRoomNum = "";
            var bmsService = DependencyResolver.Current.GetService<IBms>();
            var roomListAll = await bmsService.GetClassroomsAsync();
            var roomList = roomListAll.Where(o => o.SchoolRoomNum != selfClassRoomNum).ToList();

            if (!string.IsNullOrEmpty(numOrName))
            {
                numOrName = numOrName.ToLower();
                roomList = roomList.Where(o => o.SchoolRoomNum.ToLower().StartsWith(numOrName) || o.SchoolRoomName.ToLower().StartsWith(numOrName)).ToList();
            }
            ClassroomList.Clear();
            roomList.ForEach(item =>
            {
                var classromm = new ClassroomModel()
                {
                    SchoolRoomName = item.SchoolRoomName,
                    CreateTime = item.CreateTime,
                    SchoolRoomNum = item.SchoolRoomNum
                };
                ClassroomList.Add(classromm);
            }
            );
            _totalClassRoom = ClassroomList.Count;

        }
        #endregion

        #region command

        public ICommand LoadCommand { get; set; }

        public ICommand SearchCommand { get; set; }

        public ICommand LostFocusCommand { get; set; }
        public ICommand PreviewKeyDownCommand { get; set; }
        public ICommand TextChangedCommand { get; set; }

        public ICommand GridDownKeyPressCommand { get; set; }
        public ICommand GridUpKeyPressCommand { get; set; }
        public ICommand GridLeftOrRightKeyPressCommand { get; set; }

        public ICommand GoBackCommand { get; set; }

        #endregion

    }
}
