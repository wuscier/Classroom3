using System;
using System.Windows;
using System.Windows.Controls;
using Classroom.ViewModel;
using Common.Helper;

namespace Classroom.View
{
    /// <summary>
    /// SettingNavView.xaml 的交互逻辑
    /// </summary>
    public partial class SettingNavView
    {
        private readonly SettingNavView _view;

        public SettingNavView()
        {
            Loaded += SettingNavView_Loaded;

            InitializeComponent();
            DataContext = new SettingNavViewModel(this);
            _view = this;
        }

        private void SettingNavView_Loaded(object sender, RoutedEventArgs e)
        {
            switch (GlobalData.Instance.CurrentSettingMenu)
            {
                case MainMenuNames.Basic:
                    var basic = Basic.Content as Button;
                    basic?.Focus();
                    break;
                case MainMenuNames.Video:
                    var video = Video.Content as Button;
                    video?.Focus();
                    break;
                case MainMenuNames.Audio:
                    var audio = Audio.Content as Button;
                    audio?.Focus();
                    break;
                case MainMenuNames.Network:
                    var network = Network.Content as Button;
                    network?.Focus();
                    break;
                case MainMenuNames.Live:
                    var live = Live.Content as Button;
                    live?.Focus();
                    break;
                default:
                    var audio2 = Audio.Content as Button;
                    audio2?.Focus();
                    break;
            }
        }

        public override void EscapeKeyDownHandler()
        {
            try
            {
                MainView mainView = new MainView();
                mainView.Show();
                _view.Close();
            }
            catch (Exception ex)
            {
                var exception = ex;
                throw;
            }
         
        }
    }
}