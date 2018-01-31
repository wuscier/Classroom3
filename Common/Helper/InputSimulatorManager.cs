using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WindowsInput.Native;


namespace Common.Helper
{
    public class InputSimulatorManager
    {
        public WindowsInput.InputSimulator InputSimu;
        private InputSimulatorManager()
        {
            InputSimu = new WindowsInput.InputSimulator();
        }

        public static InputSimulatorManager Instance { get; } = new InputSimulatorManager();

        public delegate void TextBoxEnterPress(TextBox textbox);
        public event TextBoxEnterPress TextBoxEnterPressEvent;

        public static void PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                ComboBox comboBox = sender as ComboBox;
                bool isOpen = comboBox.IsDropDownOpen;

                //combox下拉框未下拉,下拉框已打开则使用wpf的默认处理
                if (!isOpen)
                {
                    switch (e.Key)
                    {
                        //未下拉时，enter键，打开下拉框,返回
                        case Key.Return:
                            if (comboBox.Items.Count > 0)
                            {
                                comboBox.IsDropDownOpen = true;
                                e.Handled = true;
                            }
                            break;
                        //未下拉时，输入的上下键时，combox的下拉选项index不变
                        case Key.Up:
                        case Key.Down:
                        case Key.PageUp:
                        case Key.PageDown:
                            VirtualPreviewKeyDown(sender, e);
                            e.Handled = true;
                            break;
                            //为下拉时，输入左右键默认wpf的行为
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageInfoForm.ShowErrorMessage(MessageInformation.SetAudioError);
                //Logger.WriteErrorFmt(Log.SettingLog, ex, "设置音频信息异常：{0}", ex.Message);
            }
        }

        public static void VirtualPreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Key.Right:
                    case Key.Down:
                    case Key.PageDown:
                    case Key.End:
                        Instance.InputSimu.Keyboard.KeyPress(VirtualKeyCode.TAB);
                        e.Handled = true;
                        break;
                    case Key.Up:
                    case Key.PageUp:
                    case Key.Left:
                    case Key.Home:
                        Instance.InputSimu.Keyboard.ModifiedKeyStroke(VirtualKeyCode.SHIFT, VirtualKeyCode.TAB);
                        e.Handled = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                // Logger.WriteErrorFmt(Log.KeyBoardHelper, ex, "处理combox按键发生异常{0}", ex.Message);
            }
        }

        public void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Key.Right:
                    case Key.Down:
                    case Key.PageDown:
                    case Key.End:
                        Instance.InputSimu.Keyboard.KeyPress(VirtualKeyCode.TAB);
                        break;
                    case Key.Left:
                    case Key.Up:
                    case Key.PageUp:
                    case Key.Home:
                        Instance.InputSimu.Keyboard.ModifiedKeyStroke(VirtualKeyCode.SHIFT, VirtualKeyCode.TAB);
                        break;
                    case Key.Return:
                        e.Handled = true;
                        var tx = sender as TextBox;
                        if (TextBoxEnterPressEvent != null && TextBoxEnterPressEvent.GetInvocationList().Any())
                        {
                            TextBoxEnterPressEvent(tx);
                        }
                        break;

                }
            }
            catch (Exception ex)
            {
                // Logger.WriteErrorFmt(Log.KeyBoardHelper, ex, "处理combox按键发生异常{0}", ex.Message);
            }
        }


        public static void GotFocus(object sender)
        {
            var sp = sender as StackPanel;
            var imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Common;component/Image/light_flag.png", UriKind.Absolute));
            imageBrush.Stretch = Stretch.Fill;//设置图像的显示格式           
            sp.Background = imageBrush;
        }

        public static void LostFocus(object sender)
        {
            var sp = sender as StackPanel;
            sp.Background = null;
        }


    }
}
