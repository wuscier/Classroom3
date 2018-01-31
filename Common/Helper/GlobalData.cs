using System.Collections.Generic;
using Common.Model;
using Common.Model.ViewLayout;
using Prism.Mvvm;
using MeetingSdk.NetAgent.Models;

namespace Common.Helper
{
    public sealed class GlobalData : BindableBase
    {
        private GlobalData()
        {
        }

        public static GlobalData Instance { get; } = new GlobalData();

        public string CurrentHomeMenu { get; set; }
        public string CurrentSettingMenu { get; set; }

        public ConfigManager ConfigManager { get; set; }

        public Classroom Classroom { get; set; }
        public List<Classroom> Classrooms { get; set; }
        public ViewArea ViewArea { get; set; }
        public ViewArea ExtendedViewArea { get; set; }

        public Course Course { get; set; }

        /// <summary>
        /// 一周课程表
        /// </summary>
        public ClassTable Courses { get; set; }

        private Mode _currentMode;

        public Mode CurrentMode
        {
            get { return _currentMode; }
            set { SetProperty(ref _currentMode, value); }
        }

        public List<Mode> ModeList { get; set; }

        public List<MeetingModel> MeetingList { get; set; }

        public LocalSetting LocalSetting { get; set; }

    }
}
