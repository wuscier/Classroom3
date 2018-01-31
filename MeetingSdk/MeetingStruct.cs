using System;
using System.Runtime.InteropServices;

namespace MeetingSdk
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void PFunc_CallBack(int cmdId, IntPtr pData, int dataLen, long ctx);

    #region  结构体

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ContactInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string m_szPhoneId; /*视讯号*/
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string m_szDisplayName; /*名称*/
    }

    //窗口位置信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ViewRect
    {
        public int m_left;
        public int m_right;
        public int m_top;
        public int m_bottom;
    }

    //设备信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DeviceInfo
    {
        public int m_isDefault; /*是否是默认设备*/
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string m_szDevName; /*设备名称*/
    }

    //参会者信息结构体定义
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ParticipantInfo
    {
        public ContactInfo m_contactInfo; /*参会者视讯号 和 名称*/
        public int m_bIsSpeaking; /*是否正在发言 0 否  非0 是*/
    }

    //当前会议信息结构体定义
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct CurMeetingInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string m_szTeacherPhoneId; /*老师的视讯号*/
        //ParticipantInfo*	m_pParticipantsInfo;			/*参会人列表指针*/
        public IntPtr m_pParticipantsInfo; /*参会人列表指针*/
        public int m_iParticipantsCount; /*参会人列表中成员的个数*/
    }

    //异步调用结果
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class AsynCallResult
    {
        public int m_rc; /*错误码 0：成功  非0：失败*/
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string m_message; /*错误描述信息*/
    }

    //启动结果
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class StartResult
    {
        public AsynCallResult m_result; /*异步调用结果*/
        public ContactInfo m_selfInfo; /*自己的视讯号和名称，异步调用成功时有效*/
    }

    //创建会议结果
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class CreateMeetingResult
    {
        public AsynCallResult m_result; /*异步调用结果*/
        public int m_meetingId; /*创建的会议Id*/
    }

    //加入会议结果
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class JoinMeetingResult
    {
        public AsynCallResult m_result; /*异步调用结果*/
        public CurMeetingInfo m_meetingInfo; /*会议信息，调用成功时有效*/
    }

    //邀请参会通知
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct MeetingInvitation
    {
        public ContactInfo m_invitor; /*邀请者信息*/
        public int m_meetingId; /*邀请参加的会议Id*/
    }

    //发言者视频窗口信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SpeakerView
    {
        public ContactInfo m_speaker; /*发言者信息*/
        public ViewRect m_viewPosition; /*窗口默认位置*/
        //void *				m_viewHwnd;		/*窗口句柄*/
        public IntPtr m_viewHwnd; /*窗口句柄*/
        public int m_viewType; /*视图类型 1：摄像头  2：课件 */
        public int m_visible; /*是否可见 0：不可见  非0：可见*/
    }

    //参会者间的透传消息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct UIMessage
    {
        public ContactInfo m_sender; /*消息发送者*/
        public int m_messageId; /*消息Id*/
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 160)] public string m_szData; /*消息携带数据*/

        private readonly int m_DataLen; /*携带数据长度*/
    }

    //当前可参加的会议列表
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct MeetingInfo
    {
        public int m_meetingId; /*会议Id*/
        public int m_meetingType; /*会议类型 1即时会议 2 预约会议*/
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string m_szCreatorId; /*创建者视讯号*/
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string m_szCreatorName; /*创建者名称*/
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)] public string m_szStartTime; /*会议开始时间*/

    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct MeetingList
    {
        //MeetingInfo *			m_pMeetings;		/*会议信息指针*/
        public IntPtr m_pMeetings; /*会议信息指针*/
        public int m_count; /*会议数*/
    }

    #endregion

    /*取得会议列表结果*/

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct GetMeetingListResult
    {
        public AsynCallResult m_result; /*异步调用结果*/
        public MeetingList m_meetingList; /*会议列表*/
    };

    /*查询名称结果*/

    public struct QueryNameResult
    {
        public AsynCallResult m_result; /*异步调用结果*/
        public ContactInfo m_contactInfo; /*视讯号 和 名称*/
    };

    /// <summary>
    /// 推流参数
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct LiveParam
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string m_url1; //直播流地址1,直播流地址2

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string m_url2; //直播流地址1,直播流地址2

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string m_sRecordFilePath; //录制文件路径

        public int m_nWidth; //直播画面宽度
        public int m_nHeight; //直播画面高度
        public int m_nVideoBitrate; //直播视频码率(单位Kbps)	
        public int m_nSampleRate; //采样率
        public int m_nChannels; //声道数
        public int m_nBitsPerSample; //采样精度
        public int m_nAudioBitrate; //直播音频码率(单位Kbps)
        public int m_nIsLive; //是否直播
        public int m_nIsRecord; //是否录制
    }

    /// <summary>
    /// 开始直播返回结果
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class StartLiveStreamResult
    {
        /// <summary>
        /// 异步调用结果
        /// </summary>
        public AsynCallResult m_result;

        /// <summary>
        /// 创建的直播Id
        /// </summary>
        public int m_liveId;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class LocalRecordResult
    {
        /// <summary>
        /// 异步调用结果
        /// </summary>
        public AsynCallResult m_result;

        /// <summary>
        /// 创建的直播Id
        /// </summary>
        public int m_liveId;
    }

    /*监控推流录制*/

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class MonitorBroadcastResult
    {
        public AsynCallResult m_result; /*异步调用结果*/
        public int m_streamid; /*流id*/
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct VideoDeviceInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Name;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public VideoFormat[] Formats;
        public int FormatCount;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct VideoFormat
    {
        public VideoColorSpace ColorSpace;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public VideoSize[] VideoSizes;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public int[] Fps;
        public int sizeCount;
        public int fpsCount;
    }

    public enum VideoColorSpace
    {
        COLORSPACE_I420,
        COLORSPACE_YV12,
        COLORSPACE_NV12,
        COLORSPACE_YUY2,
        COLORSPACE_YUYV,
        COLORSPACE_RGB,
        COLORSPACE_MJPG
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct VideoSize
    {
        public int Width;
        public int Height;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct LiveVideoStreamInfo
    {
        public int XLocation;
        public int YLocation;
        public int Width;
        public int Height;
        public uint Handle;
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct RecordParam
    {
        public int Width; //直播画面宽度     
        public int Height; //直播画面高度
        public int VideoBitrate; //直播视频码率(单位Kbps)
        public int SampleRate; //采样率    
        public int Channels; //声道数
        public int BitsPerSample; //采样精度
        public int AudioBitrate; //直播音频码率(单位Kbps)
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct VideoStreamParam
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string m_sPhoneId; //视讯号
        public int m_nMediaType;
        public int m_nVideoWidth;
        public int m_nVideoHeight;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class GetvideoParamResult
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public VideoStreamParam[] m_VideoParams;
        public int m_count; //实际取到的数目
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DeviceUpdateInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string m_szDevName; /*设备名称*/
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DeviceUpdateResult
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public DeviceUpdateInfo[] m_Devices;
        public int m_count; //实际取到的数目
        public int m_nDeviceType; //0:视频设备1：音频设备2：音频播放设备

    }

    public enum ENUM_CALLBACK_CMD_TYPE
    {
        /*异步调用结果*/
        enum_startup_result = 100, /*启动结果					回调参数 StartResult				*/
        enum_createMeeting_result, /*创建会议结果				回调参数 CreateMeetingResult		*/
        enum_invite_result, /*邀请参会结果				回调参数 AsynCallResult				*/
        enum_joinmeeting_result, /*进入会议结果				回调参数 JoinMeetingResult			*/
        enum_applyspeak_result, /*申请发言结果				回调参数 AsynCallResult				*/
        enum_exitmeeting_result, /*退出会议结果				回调参数 AsynCallResult				*/
        enum_getmeetinglist_result, /*取得会议列表结果			回调参数 GetMeetingListResult		*/
        enum_queryname_result, /*查询名称结果				回调参数 QueryNameResult			*/
        enum_modify_name_result, /*修改显示名称结果			回调参数 AsynCallResult				*/
        enum_senduimessage_result, /*发送消息结果				回调参数 AsynCallResult				*/
        enum_verifymeetexist_result, /*查询会议是否存在结果		回调参数 AsynCallResult				*/
        enum_open_doc_result, /*打开课件结果				回调参数 AsynCallResult				*/
        enum_close_doc_result, /*关闭课件结果				回调参数 AsynCallResult				*/
        enum_start_broadcast_result, /*开始推流结果				回调参数 AsynCallResult				*/
        enum_stop_broadcast_result, /*停止推流结果				回调参数 AsynCallResult				*/
        enum_create_datemeeting_result, /*创建预约会议结果			回调参数 CreateMeetingResult		*/
        enum_req_user_speak_result, /*请求用户发言结果			回调参数 AsynCallResult				*/
        enum_req_user_stopspeak_result, /*请用户停止发言结果		回调参数 AsynCallResult				*/
        enum_openuserstream_result, /*打开用户流结果			回调参数 AsynCallResult				*/
        enum_closeuserstream_result, /*关闭用户流结果			回调参数 AsynCallResult				*/
        enum_start_record_result, /*开始录制结果				回调参数 AsynCallResult				*/
        enum_stop_record_result, /*停止录制结果				回调参数 AsynCallResult				*/
        enum_set_mute_state_result, /*设置麦克风静音结果		回调参数 AsynCallResult				*/
        enum_host_kick_user_result, /*设置麦克风静音结果		回调参数 AsynCallResult				*/
        enum_start_screensharing_result, /*开始屏幕分享结果			回调参数 AsynCallResult				*/
        enum_stop_screensharing_result, /*停止桌面分享结果			回调参数 AsynCallResult				*/
        enum_open_camera_result, /*打开摄像头				回调参数 AsynCallResult				*/
        enum_close_camera_result, /*关闭摄像头				回调参数 AsynCallResult				*/
        enum_open_data_camera_result, /*打开数据分享				回调参数 AsynCallResult				*/
        enum_close_data_camera_result, /*关闭数据分享				回调参数 AsynCallResult				*/
        enum_set_default_camera_result, /*设置默认摄像头			回调参数 AsynCallResult				*/
        enum_update_live_layout_result, /*更新直播视频流布局		回调参数 AsynCallResult				*/
        enum_set_record_param_result, /*设置录制参数				回调参数 AsynCallResult				*/
        enum_stop_monitor_result, /*停止监控推流				回调参数 AsynCallResult				*/
        enum_start_monitor_result, /*开始监控推流				回调参数 MonoitorBroadcastResult	*/
        enum_get_video_param_result, /*取得视频分辨率			回调参数 GetvideoParamResult		*/
        enum_device_update_result, /*设备更新					回调参数 GetvideoParamResult		*/
        enum_check_disk_space,          /*检查磁盘空间				回调参数 AsynCallResult		*/

        /*通知消息*/
        enum_recive_invitation = 200, /*收到参会邀请通知			回调参数 MeetingInvitation			*/
        enum_view_created, /*新视频窗口创建通知		回调参数 SpeakerView				*/
        enum_view_closed, /*视频窗口关闭通知			回调参数 SpeakerView				*/
        enum_startspeak, /*开始发言通知				无回调参数							*/
        enum_stopspeak, /*停止发言通知				无回调参数							*/
        enum_other_startspeak, /*其他人开始发言通知		回调参数 ContactInfo（发言人信息）	*/
        enum_other_stopspeak, /*其他人停止发言通知		回调参数 ContactInfo（发言人信息）	*/
        enum_other_joinmeeting, /*其他人进入会议通知		回调参数 ContactInfo（进入者信息）	*/
        enum_other_exitmeeting, /*其他人退出会议通知		回调参数 ContactInfo（退出者信息）	*/
        enum_other_message, /*收到来自其他参会者的消息	回调参数 UIMessage					*/
        enum_device_lost, /*设备丢失通知				回调参数 DeviceLost					*/
        enum_set_double_screen_render, /*设置双屏渲染模式			回调参数							*/

        enum_kicked_by_host,


        /*通用的底层回调的错误提示信息*/
        enum_error_message = 300 /*底层错误提示信息	回调参数 AsynCallResult						*/
    }

}
