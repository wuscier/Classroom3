namespace Common.Model
{
    public class Classroom
    {
        public static Classroom NullClassroom => new Classroom();
        /// <summary>
        /// 教室号
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 视讯号 65000635
        /// </summary>
        public string SchoolRoomNum { get; set; }

        /// <summary>
        /// 设备号 BOX923T89OX
        /// </summary>
        public string SchoolRoomImei { get; set; }
        public string SchoolRoomName { get; set; }
        public string SchoolRoomAddress { get; set; }
        public string PhysicalChannelOuterId { get; set; }
        public string PlayStreamUrl { get; set; }
        public string PushStreamUrl { get; set; }
        public string RemotePlayStreamUrl { get; set; }
        public string RemotePushStreamUrl { get; set; }
        public string Token { get; set; }
        public string Remark { get; set; }
        public string CreateTime { get; set; }
    }
}
