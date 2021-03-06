﻿using System.Windows;
using System.Windows.Input;
using Classroom.ViewModel;
using Common.Helper;

namespace Classroom.View
{
    /// <summary>
    /// VideoSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class VideoSettingView
    {
        private readonly VideoSettingView _view;
        public VideoSettingView()
        {
            InitializeComponent();
            DataContext = new VideoSettingModel(this);
            CbbDisplaySource.Focus();
            _view = this;

        }

        public override void EscapeKeyDownHandler()
        {
            var settingNavView = new SettingNavView();
            settingNavView.Show();
            // Close();
            _view.Close();
        }

        private void combox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            InputSimulatorManager.PreviewKeyDown(sender, e);
        }

        private void stack_GotFocus(object sender, RoutedEventArgs e)
        {
            InputSimulatorManager.GotFocus(sender);
        }

        private void Stack_OnLostFocus(object sender, RoutedEventArgs e)
        {
            InputSimulatorManager.LostFocus(sender);
        }

        private void btnBack_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            InputSimulatorManager.VirtualPreviewKeyDown(sender, e);

        }
    }
}
