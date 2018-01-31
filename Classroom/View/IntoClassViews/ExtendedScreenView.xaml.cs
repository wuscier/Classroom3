using MeetingSdk.Wpf.Interfaces;
using System.Collections.Generic;
using System.Windows.Forms.Integration;


namespace Classroom.View
{
    /// <summary>
    /// ExtendedScreenView.xaml 的交互逻辑
    /// </summary>
    public partial class ExtendedScreenView : IHost
    {
        public ExtendedScreenView()
        {
            InitializeComponent();
            InitializeHosts();
        }

        private void InitializeHosts()
        {
            Hosts = new List<WindowsFormsHost>();
            Hosts.Add(VideoBox1);
            Hosts.Add(VideoBox2);
            Hosts.Add(VideoBox3);
            Hosts.Add(VideoBox4);
            Hosts.Add(VideoBox5);
            Hosts.Add(VideoBox6);
        }

        public IList<WindowsFormsHost> Hosts { get; set; }
    }
}
