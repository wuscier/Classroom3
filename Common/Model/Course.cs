using Newtonsoft.Json;
using System.Windows.Controls;
using Prism.Mvvm;

namespace Common.Model
{
    public class Course:BindableBase
    {
        /// <summary>
        /// 课程名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// 课时id
        /// </summary>
        [JsonProperty("courseid")]
        public int CourseId { get; set; }

        /// <summary>
        /// 学科名称
        /// </summary>
        [JsonProperty("coursename")]
        public string CourseName { get; set; }

        /// <summary>
        /// 课时开始时间
        /// </summary>
        [JsonProperty("coursestarttime")]
        public string CourseStartTime { get; set; }

        /// <summary>
        /// 主讲教室名称
        /// </summary>
        [JsonProperty("mainclassroomname")]
        public string MainClassRoomName { get; set; }

        /// <summary>
        /// 课时结束时间
        /// </summary>
        [JsonProperty("courseendtime")]
        public string CoursEendTime { get; set; }

        /// <summary>
        /// 课时序号id
        /// </summary>
        [JsonProperty("curriculumnumber")]
        public int CurriculumNumber { get; set; }

        /// <summary>
        /// 课时序号名称
        /// </summary>
        [JsonProperty("curriculumname")]
        public string CurriculumName { get; set; }

        /// <summary>
        /// 星期id
        /// </summary>
        [JsonProperty("weekid")]
        public int WeekId { get; set; }

        /// <summary>
        /// 星期名称
        /// </summary>
        [JsonProperty("weekname")]
        public string WeekName { get; set; }

        /// <summary>
        /// 年级Id
        /// </summary>
        [JsonProperty("Gradeid")]
        public int Gradeid { get; set; }

        /// <summary>
        /// 年级名称
        /// </summary>
        [JsonProperty("gradename")]
        public string GradeName { get; set; }

        /// <summary>
        /// 是否本地推流
        /// </summary>
        [JsonProperty("ispush")]
        public bool IsPush { get; set; }
        /// <summary>
        /// 是否本地录制
        /// </summary>
        [JsonProperty("isrecord")]
        public bool IsRecord { get; set; }

        /// <summary>
        /// 是否公网推流
        /// </summary>
        [JsonProperty("ispushremote")]
        public bool IsPushRemote { get; set; }

        /// <summary>
        /// 是否公网录制
        /// </summary>
        [JsonProperty("isrecordremote")]
        public bool IsRecordRemote { get; set; }

        [JsonProperty("islocalrecord")]
        public bool IsLocalRecord { get; set; }

        /// <summary>
        /// 主讲教室id
        /// </summary>
        [JsonProperty("mainclassroomid")]
        public int MainClassroomId { get; set; }

        /// <summary>
        /// 听讲教室id集合。多个以逗号分隔
        /// </summary>
        [JsonProperty("listenclassroomids")]
        public string ListenClassroomIds { get; set; }

        /// <summary>
        /// 听讲教室名称集合。多个以逗号分隔
        /// </summary>
        [JsonProperty("listenclassroomnames")]
        public string ListenClassroomNames { get; set; }

        /// <summary>
        /// 会议id
        /// </summary>
        [JsonProperty("mettingid")]
        public int MeetingId { get; set; }

        /// <summary>
        /// 数据库主键
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// 预约时间戳
        /// </summary>
        [JsonProperty("mettingbegindatetime")]
        public int MeetingBeginTime { get; set; }

        public ToolTip ToolTip { get; set; }

        private bool _isProcessing;

        public bool IsProcessing
        {
            get { return _isProcessing; }
            set
            {
                SetProperty(ref _isProcessing,value);
            }
        }
    }
}
