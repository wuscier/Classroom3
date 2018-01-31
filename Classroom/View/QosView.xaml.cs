using System.Windows;
using System.Windows.Forms;

namespace Classroom.View
{
    /// <summary>
    /// QosView.xaml 的交互逻辑
    /// </summary>
    public partial class QosView : Window
    {
        public QosView(string qosInfo)
        {
            InitializeComponent();
            Height = SystemInformation.WorkingArea.Height;
            tbQosInfo.Text = qosInfo;
        }
    }
}
