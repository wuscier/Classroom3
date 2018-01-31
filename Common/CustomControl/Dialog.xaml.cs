using System.Windows;
using System.Windows.Input;

namespace Common.CustomControl
{
    /// <summary>
    /// Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog
    {
        public Dialog()
        {
            InitializeComponent();
        }


        private void ClassModeView_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    e.Handled = true;
                    Close();
                    break;
            }
        }

        public Dialog(string message, string positiveMsg, string negativeMsg) : this()
        {
            MessageTextBlock.Text = message;
            YesButton.Content = positiveMsg;
            NoButton.Content = negativeMsg;

            YesButton.Focus();
        }

        public Dialog(string message) : this()
        {
            MessageTextBlock.Text = message;

            YesButton.Visibility = Visibility.Collapsed;
            NoButton.Visibility = Visibility.Collapsed;
        }

        private void ChoiceButton_OnClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement feSource = e.Source as FrameworkElement;
            switch (feSource?.Name)
            {
                case "YesButton":
                    DialogResult = true;
                    break;
                case "NoButton":
                    DialogResult = false;
                    break;
            }
            e.Handled = true;
        }
    }
}
