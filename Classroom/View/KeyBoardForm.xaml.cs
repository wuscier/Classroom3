using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Common.Helper;

namespace Classroom.View
{
    public delegate void BtnCallBack();
    /// <summary>
    /// KeyBoardForm.xaml 的交互逻辑
    /// </summary>
    public partial class KeyBoardForm : Window
    {
        //绑定的textbox
        private TextBox textBox;
        //点击确认按钮时的回调
        private BtnCallBack btnCallBack;

        private KeyBoardForm()
        {
            InitializeComponent();
        }
        private void Bind(TextBox textBox, string content, BtnCallBack btnCallBack)
        {
            try
            {
                //设置确认按钮的字符串
                this.okBtn.Content = content;
                //设置绑定的输入框
                this.textBox = textBox;
                //绑定点击确定按钮的回调函数
                this.btnCallBack = btnCallBack;
            }
            catch (Exception e)
            {
                // Logger.WriteError(Log.KeyBoardForm, "键盘绑定失败", e);
            }
        }
        public static KeyBoardForm GetKeyBoardForm(TextBox textBox, string content, BtnCallBack btnCallBack)
        {
            try
            {
                var form = new KeyBoardForm();
                //获得textBox控件显示的真实宽和高
                var window = Window.GetWindow(textBox);
                var windowWidth = SystemParameters.PrimaryScreenWidth;
                var txtWidth = textBox.ActualWidth;
                var design_Width = 1920;
                var width = windowWidth * txtWidth / design_Width;
                width = width < 500 ? 500 : width;
                var windowHeight = SystemParameters.PrimaryScreenHeight;
                var txtHeight = textBox.ActualHeight;
                var design_Height = 1080;
                var height = windowHeight * txtHeight / design_Height;

                var point = textBox.TransformToAncestor(window).Transform(new Point(0, 0));
                SetPosition(form, point.X, point.Y + height, width, 180);
                form.Show();
                form.Activate();
                form.Bind(textBox, content, btnCallBack);
                form.okBtn.Focus();
                var hwnd = new System.Windows.Interop.WindowInteropHelper(form).Handle;
                WindowPosMananger.Instance.SetKeyBoardIntptr(hwnd);
                WindowPosMananger.Instance.SetIntPrt(hwnd, 1);
                return form;
            }
            catch (Exception ex)
            {
                //  Logger.WriteError(Log.KeyBoardForm, "获取键盘发生异常", ex);
            }
            return null;
        }
        public static KeyBoardForm GetKeyBoardForm(TextBox textBox, string content, BtnCallBack btnCallBack, double left, double top, double width, double height)
        {
            try
            {
                KeyBoardForm form = new KeyBoardForm();
                SetPosition(form, left, top, width, height);
                form.Show();
                form.Activate();
                form.Bind(textBox, content, btnCallBack);
                form.okBtn.Focus();
                IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(form).Handle;
                WindowPosMananger.Instance.SetKeyBoardIntptr(hwnd);
                return form;
            }
            catch (Exception ex)
            {
                // Logger.WriteError(Log.KeyBoardForm, "获得键盘发生异常", ex);
            }
            return null;
        }
        private static void SetPosition(KeyBoardForm form, double left, double top, double width, double height)
        {
            try
            {
                form.Left = left;
                form.Top = top;
                form.Width = width;
                form.Height = height;
            }
            catch (Exception e)
            {
                // Logger.WriteError(Log.KeyBoardForm, "设置键盘位置发生异常", e);
            }
        }
        public static void CloseFrom(KeyBoardForm form)
        {
            try
            {
                if (form != null)
                {
                    form.Close();
                    WindowPosMananger.Instance.SetKeyBoardIntptr(IntPtr.Zero);
                }
            }
            catch (Exception e)
            {
                // Logger.WriteError(Log.KeyBoardForm, "设置关闭键盘窗口发生异常", e);
            }
        }

        //数字按钮
        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button noBtn = (Button)sender;
                textBox.Text = textBox.Text + noBtn.Content;
            }
            catch (Exception ex)
            {
                // Logger.WriteError(Log.KeyBoardForm, "点击数字按钮发生异常", ex);
            }
        }
        //删除按钮
        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (textBox.Text.Length > 0)
                {
                    textBox.Text = textBox.Text.Substring(0, textBox.Text.Length - 1);
                }
            }
            catch (Exception ex)
            {
                // Logger.WriteError(Log.KeyBoardForm, "点击删除按钮发生异常", ex);
            }
        }
        //确认按钮
        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Visibility = Visibility.Collapsed;
                if (btnCallBack != null)
                {
                    btnCallBack();
                }
                this.Close();
            }
            catch (Exception ex)
            {
                // Logger.WriteError(Log.KeyBoardForm, "点击确定按钮发生异常", ex);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {

                if (e.Key == Key.Escape)
                {
                    WindowPosMananger.Instance.SetKeyBoardIntptr(IntPtr.Zero);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                // Logger.WriteError(Log.KeyBoardForm, "键盘监听事假发生异常", ex);
            }
        }
    }
}
