using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Classroom.Model;
using Common.Helper;
using Prism.Commands;
using Prism.Mvvm;
using MeetingSdk.Wpf;
using Common.UiMessage;

namespace Classroom.ViewModel.IntoClassViewModels
{
    public class SetSpecialViewModel : BindableBase
    {
        private readonly IMeetingWindowManager _windowManager;

        private readonly LayoutRenderType _targetPictureMode;

        public SetSpecialViewModel(LayoutRenderType pictureMode)
        {
            _windowManager = DependencyResolver.Current.GetService<IMeetingWindowManager>();

            _targetPictureMode = pictureMode;

            SetSpecialViewTip(pictureMode);

            AttendeeViews = new ObservableCollection<TextWithButtonItem>();

            GetAttendeeViewsCommand = new DelegateCommand(GetAttendeeViewsAsync);
            SetSpecialViewCommand = new DelegateCommand<TextWithButtonItem>(SetSpecialViewAsync);
        }

        private void SetSpecialViewTip(LayoutRenderType pictureMode)
        {
            if (pictureMode == LayoutRenderType.BigSmallsLayout)
            {
                SpecialViewTip = "请选择作为大画面的教室";
            }

            if (pictureMode == LayoutRenderType.CloseupLayout)
            {
                SpecialViewTip = "请选择作为特写画面的教室";
            }
       }

        private void SetSpecialViewAsync(TextWithButtonItem attendeeViewItem)
        {
            var specialView = _windowManager.VideoBoxManager.Items.FirstOrDefault(v => v.AccountResource != null && v.AccountResource.AccountModel.AccountId.ToString() == attendeeViewItem.Id && v.Handle == attendeeViewItem.Hwnd);

            if (specialView == null)
            {
                MessageQueueManager.Instance.AddWarning("找不到该视图！");
                return;
            }

            _windowManager.VideoBoxManager.SetProperty(_targetPictureMode.ToString(), specialView.Name);

            try
            {
                if (!_windowManager.LayoutChange(WindowNames.MainWindow, _targetPictureMode))
                {
                    MessageQueueManager.Instance.AddError("无法设置一大一小画面模式！");
                }
                //if (_windowManager.LayoutChange(WindowNames.ExtendedWindow, _targetPictureMode))
                //{
                //}
            }
            catch (Exception ex)
            {
                MessageQueueManager.Instance.AddError(ex.Message);
            }
        }

        private void GetAttendeeViewsAsync()
        {
            var openedViews = _windowManager.VideoBoxManager.Items.Where(v => v.Visible);

            var invitees = from openedView in openedViews
                select new TextWithButtonItem()
                {
                    Id = openedView.AccountResource.AccountModel.AccountId.ToString(),
                    Hwnd = openedView.Handle,
                    Text = openedView.AccountResource.AccountModel.AccountId +  ((openedView.AccountResource.AccountModel.AccountId == _windowManager.HostId && openedView.AccountResource.MediaType == MeetingSdk.NetAgent.Models.MediaType.VideoDoc )? $" [{openedView.AccountResource.AccountModel.AccountName}（课件）]":    $" [{openedView.AccountResource.AccountModel.AccountName}]"),
                    ButtonCommand = SetSpecialViewCommand,
                    Content = "设 置",
                    ButtonForeground = new SolidColorBrush(Colors.White),
                    ButtonBackground = (SolidColorBrush) Application.Current.Resources["ThemeBrush"],
                    ButtonVisibility = Visibility.Collapsed
                };

            invitees.ToList().ForEach(invitee =>
            {
                AttendeeViews.Add(invitee);
            });

        }

        private string _specialViewTip;
        public string SpecialViewTip
        {
            get { return _specialViewTip; }
            set { SetProperty(ref _specialViewTip, value); }
        }


        public ObservableCollection<TextWithButtonItem> AttendeeViews { get; set; }
        public ICommand GetAttendeeViewsCommand { get; set; }
        public ICommand SetSpecialViewCommand { get; set; }
    }
}