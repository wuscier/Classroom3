using System;
using System.Collections.Generic;
using Prism.Mvvm;

namespace Common.Model
{
    public class ConfigManager
    {
        public ServerInfo ServerInfo { get; set; }
        public VideoInfo MainVideoInfo { get; set; }
        public VideoInfo DocVideoInfo { get; set; }

        public AudioInfo AudioInfo { get; set; }

        public NetInfo NetInfo { get; set; }

        public LiveStreamInfo LocalLiveStreamInfo { get; set; }

        public LiveStreamInfo RemoteLiveStreamInfo { get; set; }

        public RecordInfo RecordInfo { get; set; }
    }

    public class ServerInfo
    {
        public string NameInfo { get; set; }

        public string ServerIp { get; set; }

        public int BmsServerPort { get; set; }
    }

    public class VideoInfo : BindableBase
    {
        private string _videoDevice;
        public string VideoDevice
        {
            get { return _videoDevice; }
            set { SetProperty(ref _videoDevice, value); }
        }


        private int _displayWidth;
        public int DisplayWidth
        {
            get { return _displayWidth; }
            set { SetProperty(ref _displayWidth, value); }
        }

        private int _displayHeight;
        public int DisplayHeight
        {
            get { return _displayHeight; }
            set { SetProperty(ref _displayHeight, value); }
        }

        private int _videoBitRate;
        public int VideoBitRate
        {
            get { return _videoBitRate; }
            set { SetProperty(ref _videoBitRate, value); }
        }

        private int _colorSpace;
        public int ColorSpace { get
            {
                return _colorSpace;
            }
            set
            {
                SetProperty(ref _colorSpace, value);
            }
        }

        //private string _docDevice;
        //public string DocDevice
        //{
        //    get { return _docDevice; }
        //    set { SetProperty(ref _docDevice, value); }
        //}


        //private int _docDisplayWidth;
        //public int DocDisplayWidth
        //{
        //    get { return _docDisplayWidth; }
        //    set { SetProperty(ref _docDisplayWidth, value); }
        //}

        //private int _docDisplayHeight;
        //public int DocDisplayHeight
        //{
        //    get { return _docDisplayHeight; }
        //    set { SetProperty(ref _docDisplayHeight, value); }
        //}

        //private int _docBitRate;
        //public int DocBitRate
        //{
        //    get { return _docBitRate; }
        //    set { SetProperty(ref _docBitRate, value); }
        //}

    }

    public class AudioInfo
    {
        public string AudioSammpleDevice { get; set; }

        public string DocAudioSammpleDevice { get; set; }
        /// <summary>
        /// 采样率
        /// </summary>
        public int SampleRate { get; set; }
        /// <summary>
        /// 音频码率
        /// </summary>
        public int AAC { get; set; }
        /// <summary>
        /// 放音设备
        /// </summary>
        public string AudioOutPutDevice { get; set; }

    }

    public class NetInfo
    {
        public string AdapterName { get; set; }
        /// <summary>
        /// TRUE:自动配置 false：手动配置
        /// </summary>
        public bool IsAutoSet { get; set; }

        public string IpAddress { get; set; }

        public string Mark { get; set; }

        public string Dns { get; set; }

        public string GatWay { get; set; }

        //备用DNS
        public string lvsDns { get; set; }
    }

    public class LiveStreamInfo
    {
        public int LiveStreamDisplayWidth { get; set; }

        public int LiveStreamDisplayHeight { get; set; }

        public int LiveStreamBitRate { get; set; }
    }

    public class RecordInfo
    {
        public int RecordDisplayWidth { get; set; }

        public int RecordDisplayHeight { get; set; }

        public int RecordBitRate { get; set; }

        public string RecordDirectory { get; set; }
    }

    public class MeetingList
    {
        public List<MeetingItem> MeetingInfos { get; set; }
    }

    public class MeetingItem
    {
        public int MeetingId { get; set; }

        public int MeetingType { get; set; }

        public string CreatorId { get; set; }

        public string CreatorName { get; set; }

        public DateTime LastActivityTime { get; set; }

        public DateTime CreateTime { get; set; }

        public bool IsClose { get; set; }
    }
}
