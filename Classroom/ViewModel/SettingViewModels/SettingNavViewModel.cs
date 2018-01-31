using System.Windows.Input;
using Classroom.Model;
using Classroom.View;
using Common.Helper;
using Prism.Commands;
using Prism.Mvvm;

namespace Classroom.ViewModel
{
    public class SettingNavViewModel : BindableBase
    {

        #region field

        private readonly SettingNavView _navView;
        private readonly string _elementName;

        #endregion

        #region property

        public string ElementName
        {
            get { return _elementName; }
            set { SetProperty(ref value, _elementName); }
        }

        public MainMenu BasicSettingMenu { get; set; }
        public MainMenu VideoSettingMenu { get; set; }
        public MainMenu AudioSettingMenu { get; set; }
        public MainMenu NetSettingMenu { get; set; }
        public MainMenu LiveSettingMenu { get; set; }

        #endregion

        #region ctor

        public SettingNavViewModel(SettingNavView view)
        {
            ElementName = "AudioSetting";
            _navView = view;
            GotoVideoCommand = new DelegateCommand(GotoVideo);
            GotoAudioCommand = new DelegateCommand(GotoAudio);
            GotoLiveCommand = new DelegateCommand(GotoLive);
            GotoNetWorkCommand = new DelegateCommand(GotoNetWork);
            GotoBasicCommand = new DelegateCommand(GotoBasic);
            GoBackCommand = new DelegateCommand(GoBack);
            InitMenus();
        }

        #endregion

        #region method

        private void GoBack()
        {
            var mainview = new MainView();
            mainview.Show();
            _navView.Close();
        }


        private void GotoBasic()
        {
            GlobalData.Instance.CurrentSettingMenu = MainMenuNames.Basic;

            var view = new BaseInfoSettingView();
            view.Show();
            _navView.Close();
        }
        private void GotoVideo()
        {
            GlobalData.Instance.CurrentSettingMenu = MainMenuNames.Video;

            var view = new VideoSettingView();
            view.Show();
            _navView.Close();
        }
        private void GotoAudio()
        {
            GlobalData.Instance.CurrentSettingMenu = MainMenuNames.Audio;

            var view = new AudioSettingView();
            view.Show();
            _navView.Close();
        }
        private void GotoLive()
        {
            GlobalData.Instance.CurrentSettingMenu = MainMenuNames.Live;

            var view = new LiveSettingView();
            view.Show();
            _navView.Close();
        }
        private void GotoNetWork()
        {
            GlobalData.Instance.CurrentSettingMenu = MainMenuNames.Network;

            var view = new NetworkSettingView();
            view.Show();
            _navView.Close();
        }

        private void InitMenus()
        {
            BasicSettingMenu = new MainMenu()
            {
                FocusedImageUrl = "/Common;Component/Image/seting_1_1.png",
                GotoCommand = GotoBasicCommand,
                ImageUrl = "/Common;Component/Image/seting_1.png",
                MenuName = "基本设置"
            };
            VideoSettingMenu = new MainMenu()
            {
                FocusedImageUrl = "/Common;Component/Image/setting_2_1.png",
                GotoCommand = GotoVideoCommand,
                ImageUrl = "/Common;Component/Image/setting_2.png",
                MenuName = "视频设置"
            };
            AudioSettingMenu = new MainMenu()
            {
                FocusedImageUrl = "/Common;Component/Image/setting_3_1.png",
                GotoCommand = GotoAudioCommand,
                ImageUrl = "/Common;Component/Image/setting_3.png",
                MenuName = "音频设置"
            };
            NetSettingMenu = new MainMenu()
            {
                FocusedImageUrl = "/Common;Component/Image/setting_4_1.png",
                GotoCommand = GotoNetWorkCommand,
                ImageUrl = "/Common;Component/Image/setting_4.png",
                MenuName = "网络设置"
            };
            LiveSettingMenu = new MainMenu()
            {
                FocusedImageUrl = "/Common;Component/Image/setter_5_1.png",
                GotoCommand = GotoLiveCommand,
                ImageUrl = "/Common;Component/Image/setter_5.png",
                MenuName = "直播设置"
            };
        }


        #endregion

        #region command

        public ICommand GotoVideoCommand { get; set; }
        public ICommand GotoBasicCommand { get; set; }
        public ICommand GotoAudioCommand { get; set; }
        public ICommand GotoNetWorkCommand { get; set; }
        public ICommand GotoLiveCommand { get; set; }
        public ICommand GoBackCommand { get; set; }

        #endregion


    }
}
