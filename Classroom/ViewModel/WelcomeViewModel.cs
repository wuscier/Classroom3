using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Classroom.SwichModel;
using Classroom.View;
using Common.Contract;
using Common.Helper;
using Prism.Commands;
using Prism.Mvvm;
using Serilog;

namespace Classroom.ViewModel
{
    public class WelcomeViewModel : BindableBase
    {
        #region field

        private string _netStatusImgUrl;
        private string _version;
        private string _currentTime;
        private WelcomeView _view;
        private DispatcherTimer ShowTimer;
        private int TimerCounter = 0;
        private string _classInfo;
        private readonly INetCheckService _netCheckService;
        #endregion

        #region property

        public string ClassInfo
        {
            get { return _classInfo; }
            set { SetProperty(ref _classInfo, value); }
        }

        public string CurrentTime
        {
            get { return _currentTime; }
            set { SetProperty(ref _currentTime, value); }
        }

        public string NetStatusImgUrl
        {
            get { return _netStatusImgUrl; }
            set { SetProperty(ref _netStatusImgUrl, value); }
        }

        public string Version
        {
            get { return _version; }
            set { SetProperty(ref _version, value); }
        }

        #endregion

        #region ctor

        public WelcomeViewModel(WelcomeView view)
        {
            _view = view;
            _netCheckService = DependencyResolver.Current.GetService<INetCheckService>();
            LoadCommand = DelegateCommand.FromAsyncHandler(LoadingAsync);
        }
        #endregion

        #region method

        private async Task LoadingAsync()
        {

            await _view.Dispatcher.BeginInvoke(new Action(() =>
             {
                 //1.判断网络
                 //2.获取系统版本
                 //3.教室信息
                 GetNetStatus();
                 Version = GetVersionInfo();

                 if (ShowTimer == null)
                 {
                     ShowTimer = new DispatcherTimer();
                     ShowTimer.Tick += ShowCurTimer; //起个Timer一直获取当前时间
                     ShowTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
                 }
                 ShowTimer.Start();
                 ClassInfo = $"{GlobalData.Instance.Classroom.SchoolRoomName}（教室号：{GlobalData.Instance.Classroom.SchoolRoomNum}）";
                 var mode = new Mode();
                 mode.InitializationInAutoMode();
             }));
        }
        private delegate void TimerDispatcherDelegate();
        public void ShowCurTimer(object sender, EventArgs e)
        {
            try
            {
                //获得星期几
                CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                if (TimerCounter % 5 == 0)
                {
                    GetNetStatus();
                    TimerCounter = 0;
                }
                TimerCounter++;
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"获取时间异常 exception：{ex}");
            }
        }


        private void GetNetStatus()
        {
            var netStatus = _netCheckService.GetNetStatus();
            switch (netStatus)
            {
                case NetStatus.Normal:
                    NetStatusImgUrl = "/Common;Component/Image/index_net_connect.png";
                    break;
                case NetStatus.ConnectFail:
                    NetStatusImgUrl = "/Common;Component/Image/index_net_ConnectServer.png";
                    break;
                case NetStatus.Disconnect:
                    NetStatusImgUrl = "/Common;Component/Image/index_net_dismiss.png";
                    break;
            }
        }


        private string GetVersionInfo()
        {
            try
            {
                var sVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

                var arrVersion = sVersion.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                var versionInfo = "V" + arrVersion[0] + "." + arrVersion[1] + "." + arrVersion[2];
                return versionInfo;
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"【获取软件版本 exception】：{ex}");
                return string.Empty;
            }
        }

        #endregion

        #region command

        public ICommand LoadCommand { get; set; }

        public ICommand WindowKeyDownCommand { get; set; }

        #endregion
    }
}
