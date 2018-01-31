using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Common.Helper;
using Common.UiMessage;
using MahApps.Metro.Controls;

namespace Common.CustomControl
{
    public class BaseWindow : MetroWindow
    {
        public BaseWindow()
        {
            PreviewKeyDown += BaseWindow_PreviewKeyDown;
            //UseNoneWindowStyle = true;
            //WindowStyle = WindowStyle.None;
            //IgnoreTaskbarOnMaximize = true;

            //NonActiveGlowBrush = Brushes.Transparent;
            //NonActiveBorderBrush = Brushes.Transparent;

            //ResizeMode = ResizeMode.NoResize;
            //IsWindowDraggable = false;
            //Topmost = true;
            //Width = SystemParameters.PrimaryScreenWidth;//得到屏幕整体宽度
            //Height = SystemParameters.PrimaryScreenHeight;//得到屏幕整体高度
            //this.Left = 0.0;
            //this.Top = 0.0;

            //WindowState = WindowState.Maximized;
        }


        public virtual void EscapeKeyDownHandler()
        {
            Close();
        }

        private void BaseWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    EscapeKeyDownHandler();
                    break;
            }

            if ((Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) && Keyboard.IsKeyDown(Key.T))
            {
                //只要当下同时按下的键中包含LeftCtrl、H和C，就会进入
                var current = this.Topmost;
                this.Topmost = !current;
                MessageQueueManager.Instance.AddInfo(Topmost ? MessageManager.IsTopMost : MessageManager.NotTopMost);
            }
        }
    }
}