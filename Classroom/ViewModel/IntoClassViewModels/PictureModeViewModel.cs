using System.Windows.Input;
using WindowsInput.Native;
using Classroom.Model;
using Classroom.View.IntoClassViews;
using Common.Helper;
using Prism.Commands;
using MeetingSdk.Wpf;

namespace Classroom.ViewModel
{
    public class PictureModeViewModel
    {
        private readonly IMeetingWindowManager _windowManager;

        public PictureModeViewModel()
        {
            _windowManager = DependencyResolver.Current.GetService<IMeetingWindowManager>();

            CheckPictureModeCommand = new DelegateCommand<object>(CheckPictureModeAsync);
            InitPictureModes();
        }

        private void CheckPictureModeAsync(object args)
        {
            if (args is KeyEventArgs)
            {
                KeyEventArgs keyEventArgs = (KeyEventArgs) args;

                switch (keyEventArgs.Key)
                {
                    case Key.Enter:
                        InputSimulatorManager.Instance.InputSimu.Keyboard.KeyPress(VirtualKeyCode.SPACE);
                        keyEventArgs.Handled = true;
                        break;
                }

            }


            if (args is LayoutRenderType)
            {
                LayoutRenderType pictureMode = (LayoutRenderType) args;

                switch (pictureMode)
                {
                    case LayoutRenderType.AutoLayout:
                    case LayoutRenderType.AverageLayout:

                        if (_windowManager.LayoutChange(WindowNames.MainWindow, pictureMode))
                        {
                        }
                        //if (_windowManager.LayoutChange(WindowNames.ExtendedWindow, pictureMode))
                        //{
                        //}

                        break;
                    case LayoutRenderType.CloseupLayout:
                    case LayoutRenderType.BigSmallsLayout:

                        SetSpecialView setSpecialView = new SetSpecialView(pictureMode);
                        setSpecialView.ShowDialog();
                        setSpecialView.Focus();
                        break;
                }

                if (pictureMode != _windowManager.LayoutRendererStore.CurrentLayoutRenderType)
                {
                    CheckPictureMode();
                }
            }
        }


        private void CheckPictureMode()
        {
            switch (_windowManager.LayoutRendererStore.CurrentLayoutRenderType)
            {
                case LayoutRenderType.AutoLayout:
                    AutoPictureModeItem.Checked = true;
                    break;
                case LayoutRenderType.AverageLayout:
                    AveragePictureModeItem.Checked = true;
                    break;
                case LayoutRenderType.CloseupLayout:
                    CloseupPictureModeItem.Checked = true;
                    break;
                case LayoutRenderType.BigSmallsLayout:
                    BigSmallsPictureModeItem.Checked = true;
                    break;
            }
        }

        private void InitPictureModes()
        {
            AutoPictureModeItem = new RadioPictureButtonItem()
            {
                CheckCommand = CheckPictureModeCommand,
                Type = LayoutRenderType.AutoLayout,
                Image = "/Common;Component/Image/kt_zdpj.png",
                Name = "自动布局",
                GroupName = "PictureMode"
            };
            AveragePictureModeItem = new RadioPictureButtonItem()
            {
                CheckCommand = CheckPictureModeCommand,
                Type = LayoutRenderType.AverageLayout,
                Image = "/Common;Component/Image/kt_pjpl.png",
                Name = "平均排列",
                GroupName = "PictureMode"

            };
            CloseupPictureModeItem = new RadioPictureButtonItem()
            {
                CheckCommand = CheckPictureModeCommand,
                Type = LayoutRenderType.CloseupLayout,
                Image = "/Common;Component/Image/kt_ydyx.png",
                Name = "特写模式",
                GroupName = "PictureMode"

            };
            BigSmallsPictureModeItem = new RadioPictureButtonItem()
            {
                CheckCommand = CheckPictureModeCommand,
                Type = LayoutRenderType.BigSmallsLayout,
                Image = "/Common;Component/Image/kt_hzh.png",
                Name = "一大多小",
                GroupName = "PictureMode"

            };

            CheckPictureMode();
        }

        public ICommand CheckPictureModeCommand { get; set; }

        public RadioPictureButtonItem AutoPictureModeItem { get; set; }
        public RadioPictureButtonItem AveragePictureModeItem { get; set; }
        public RadioPictureButtonItem CloseupPictureModeItem { get; set; }
        public RadioPictureButtonItem BigSmallsPictureModeItem { get; set; }
    }
}
