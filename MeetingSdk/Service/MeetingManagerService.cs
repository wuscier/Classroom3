using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Common.Contract.MeetingSdk;
using Common.Helper;
using Common.Model.ViewLayout;
using Serilog;

namespace MeetingSdk.Service
{
    public class MeetingManagerService : IMeetingManager
    {
        public MeetingManagerService()
        {
            CurrentAttendee = new Attendee();
            MainSpeaker = new Attendee();

            TaskCallbacks = new Dictionary<string, ITaskCallback>();
        }

        private static readonly object SyncRoot = new object();
        private static PFunc_CallBack _pFuncCallBack;

        public int CurrentMeetingId { get; set; }

        public Attendee CurrentAttendee { get; set; }
        public Attendee MainSpeaker { get; set; }

        private bool _isServerStarted;

        public bool IsServierStarted
        {
            get
            {
                Log.Logger.Debug($"【IsServerStarted】：{_isServerStarted}");
                return _isServerStarted;
            }
            set { _isServerStarted = value; }
        }

        public Dictionary<string, ITaskCallback> TaskCallbacks { get; }

        public bool IsMainSpeaker => CurrentAttendee?.Id == MainSpeaker?.Id;

        public Task<ReturnMessage> Start(string imei)
        {
            if (TaskCallbacks.ContainsKey("Start"))
            {
                return Task.FromResult(MessageManager.ServerStartingMessage);
            }

            if (IsServierStarted)
            {
                return Task.FromResult(MessageManager.ServerAlreadyStarted);
            }
           // var serverIP = GlobalData.Instance.ConfigManager.ServerInfo.ServerIp;
            lock (SyncRoot)
            {
                var tcs = new TaskCallback<ReturnMessage>("Start");
                TaskCallbacks.Add(tcs.Name, tcs);
               // Log.Logger.Debug(serverIP);
                Log.Logger.Debug($"任务名称{tcs.Name}");
                if (_pFuncCallBack == null)
                {
                    _pFuncCallBack = CallbackHandler;
                }

                int result = MeetingAgent.Start2(_pFuncCallBack, imei, "114.112.74.10");

                if (result != 0)
                {
                    return Task.FromResult(MessageManager.FailedToStartServer);
                }

                return tcs.Task;
            }
        }

        private void CallbackHandler(int cmdId, IntPtr pData, int dataLen, long ctx)
        {
            lock (SyncRoot)
            {
                var callbackType = (ENUM_CALLBACK_CMD_TYPE)cmdId;
                switch (callbackType)
                {
                    case ENUM_CALLBACK_CMD_TYPE.enum_startup_result:
                        StartCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_createMeeting_result:
                        CreateMeetingCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_invite_result:
                        InviteCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_joinmeeting_result:
                        JoinMeetingCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_applyspeak_result:
                        ApplyToSpeakCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_exitmeeting_result:
                        ExitMeetingCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_getmeetinglist_result:
                        GetMeetingListCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_queryname_result:
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_modify_name_result:
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_senduimessage_result:
                        SendUiMessageCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_recive_invitation:
                        ReceiveInvitationCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_view_created:
                        ViewCreateCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_view_closed:
                        ViewCloseCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_startspeak:
                        //when participants start speaking:
                        StartSpeakCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_stopspeak:
                        StopSpeakCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_other_startspeak:
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_other_stopspeak:
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_other_joinmeeting:
                        OtherJoinMeetingCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_other_exitmeeting:
                        OtherExitMeetingCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_other_message:
                        UiMessageReceivedCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_set_double_screen_render:
                        SetDoulbeScreenRenderCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_error_message:
                        ErrorMsgReceivedCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_verifymeetexist_result:
                        QueryMeetingExistCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_open_doc_result:
                        StartShareDocCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_close_doc_result:
                        StopShareDocCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_start_broadcast_result:
                        StartLiveStreamCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_stop_broadcast_result:
                        StopLiveStreamCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_create_datemeeting_result:
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_host_kick_user_result:
                        //HostKickoutUserCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_kicked_by_host:
                        //KickedByHostCallback(pData);
                        break;

                    case ENUM_CALLBACK_CMD_TYPE.enum_close_camera_result:
                        //CloseCameraCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_close_data_camera_result:
                        //CloseDataCameraCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_open_camera_result:
                        OpenCameraCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_open_data_camera_result:
                        //OpenDataCameraCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_set_default_camera_result:
                        //SetDefaultCameraCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_set_mute_state_result:
                        //SetMicMuteStateCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_start_screensharing_result:
                        //StartScreenSharingCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_stop_screensharing_result:
                        //StopScreenSharingCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_req_user_speak_result:
                        //RequireUserSpeakCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_req_user_stopspeak_result:
                        //RequireUserStopSpeakCallback(pData);
                        break;

                    case ENUM_CALLBACK_CMD_TYPE.enum_start_record_result:
                        StartRecordCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_stop_record_result:
                        StopRecordCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_set_record_param_result:
                        SetRecordParamCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_update_live_layout_result:
                        UpdateLiveVideoStreamsCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_start_monitor_result:
                        //StartMonitorBroadcastCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_stop_monitor_result:
                        //StopMonitorBroadcastCallback(pData);
                        break;
                    case ENUM_CALLBACK_CMD_TYPE.enum_get_video_param_result:
                        //GetVideoParamCallback(pData);
                        break;

                    case ENUM_CALLBACK_CMD_TYPE.enum_check_disk_space:
                        DiskSpaceNotEnoughNotify(pData);
                        break;

                    //case ENUM_CALLBACK_CMD_TYPE.enum_device_update_result:
                    //    DeviceUpdateCallback(pData);
                    //    break;
                    default:
                        break;
                }
            }
        }

        private void OpenCameraCallback(IntPtr pData)
        {
            AsynCallResult openCameraResult = Marshal.PtrToStructure<AsynCallResult>(pData);

            ReturnMessage openCameraMessage;

            if (openCameraResult.m_rc == 0)
            {
                openCameraMessage = ReturnMessage.GenerateData(openCameraResult.m_message);
            }
            else
            {
                openCameraMessage = ReturnMessage.GenerateError(openCameraResult.m_message,
                    openCameraResult.m_rc.ToString());
            }


            OpenCameraEvent?.Invoke(openCameraMessage);
        }

        private void SetDoulbeScreenRenderCallback(IntPtr pData)
        {
            try
            {
                AsynCallResult asynCallRulst = (AsynCallResult)Marshal.PtrToStructure(pData, typeof(AsynCallResult));
                Log.Logger.Debug($"SetDoulbeScreenRenderCallback回调结果{asynCallRulst.m_rc}--{asynCallRulst.m_message}");
                ReturnMessage setDoubleScreenMessage;

                if (asynCallRulst.m_rc == 0)
                {
                    setDoubleScreenMessage = ReturnMessage.GenerateData(asynCallRulst.m_message);
                }
                else
                {
                    setDoubleScreenMessage = ReturnMessage.GenerateError(asynCallRulst.m_message,
                        asynCallRulst.m_rc.ToString());
                }

                SetResult("SetDoubleScreenRender", setDoubleScreenMessage);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"SetDoulbeScreenRenderCallback回调异常：{ex.Message}");
            }
        }

        private void UpdateLiveVideoStreamsCallback(IntPtr pData)
        {
            var updateLiveVideoStreamsResult =
                (AsynCallResult)Marshal.PtrToStructure(pData, typeof(AsynCallResult));


            ReturnMessage updateLiveMessage;

            if (updateLiveVideoStreamsResult.m_rc == 0)
            {
                updateLiveMessage = ReturnMessage.GenerateData(updateLiveVideoStreamsResult.m_message);
            }
            else
            {
                updateLiveMessage = ReturnMessage.GenerateError(updateLiveVideoStreamsResult.m_message,
                    updateLiveVideoStreamsResult.m_rc.ToString());
            }

            SetResult("UpdateLiveVideoStreams", updateLiveMessage);
        }

        private void SetRecordParamCallback(IntPtr pData)
        {
            var setRecordParamResult = (AsynCallResult)Marshal.PtrToStructure(pData, typeof(AsynCallResult));

            ReturnMessage setRecordMessage;

            if (setRecordParamResult.m_rc == 0)
            {
                setRecordMessage = ReturnMessage.GenerateData(setRecordParamResult.m_message);
            }
            else
            {
                setRecordMessage = ReturnMessage.GenerateError(setRecordParamResult.m_message,
                    setRecordParamResult.m_rc.ToString());
            }


            SetResult("SetRecordParameter", setRecordMessage);
        }

        private void StopRecordCallback(IntPtr pData)
        {
            var stopRecordkResult = (AsynCallResult)Marshal.PtrToStructure(pData, typeof(AsynCallResult));

            ReturnMessage stopRecordMessage;

            if (stopRecordkResult.m_rc == 0)
            {
                stopRecordMessage = ReturnMessage.GenerateData(stopRecordkResult.m_message);
            }
            else
            {
                stopRecordMessage = ReturnMessage.GenerateError(stopRecordkResult.m_message,
                    stopRecordkResult.m_rc.ToString());
            }

            SetResult("StopRecord", stopRecordMessage);
        }

        private void StartRecordCallback(IntPtr pData)
        {
            var startRecrodResult =
                (LocalRecordResult)Marshal.PtrToStructure(pData, typeof(LocalRecordResult));

            ReturnMessage startRecordMessage;

            if (startRecrodResult.m_result.m_rc == 0)
            {
                startRecordMessage = ReturnMessage.GenerateData(startRecrodResult.m_liveId);
            }
            else
            {
                startRecordMessage = ReturnMessage.GenerateError(startRecrodResult.m_result.m_message,
                    startRecrodResult.m_result.m_rc.ToString());
            }


            SetResult("StartRecord", startRecordMessage);
        }

        private void StopLiveStreamCallback(IntPtr pData)
        {
            var stopLiveStreamResult = (AsynCallResult)Marshal.PtrToStructure(pData, typeof(AsynCallResult));

            ReturnMessage stopLiveMessage;

            if (stopLiveStreamResult.m_rc == 0)
            {
                stopLiveMessage = ReturnMessage.GenerateData(stopLiveStreamResult.m_message);
            }
            else
            {
                stopLiveMessage = ReturnMessage.GenerateError(stopLiveStreamResult.m_message,
                    stopLiveStreamResult.m_rc.ToString());
            }

            SetResult("StopLiveStream", stopLiveMessage);
        }

        private void StartLiveStreamCallback(IntPtr pData)
        {
            var startLiveStreamResult =
                (StartLiveStreamResult)Marshal.PtrToStructure(pData, typeof(StartLiveStreamResult));

            ReturnMessage startLiveMessage;

            if (startLiveStreamResult.m_result.m_rc == 0)
            {
                startLiveMessage = ReturnMessage.GenerateData(startLiveStreamResult.m_liveId);
            }
            else
            {
                startLiveMessage = ReturnMessage.GenerateError(startLiveStreamResult.m_result.m_message,
                    startLiveStreamResult.m_result.m_rc.ToString());
            }

            SetResult("StartLiveStream", startLiveMessage);
        }

        private void StopShareDocCallback(IntPtr pData)
        {
            AsynCallResult asynCallRulst = (AsynCallResult)Marshal.PtrToStructure(pData, typeof(AsynCallResult));

            ReturnMessage stopDocMessage;

            if (asynCallRulst.m_rc == 0)
            {
                stopDocMessage = ReturnMessage.GenerateMessage(asynCallRulst.m_message);
            }
            else
            {
                stopDocMessage = ReturnMessage.GenerateError(asynCallRulst.m_message, asynCallRulst.m_rc.ToString());
            }

            SetResult("StopShareDoc", stopDocMessage);
        }

        private void StartShareDocCallback(IntPtr pData)
        {
            AsynCallResult asynCallRulst = (AsynCallResult)Marshal.PtrToStructure(pData, typeof(AsynCallResult));

            ReturnMessage startDocMessage;

            if (asynCallRulst.m_rc == 0)
            {
                startDocMessage = ReturnMessage.GenerateMessage(asynCallRulst.m_message);
            }
            else
            {
                startDocMessage = ReturnMessage.GenerateError(asynCallRulst.m_message, asynCallRulst.m_rc.ToString());
            }

            SetResult("StartShareDoc", startDocMessage);
        }

        private void ReceiveInvitationCallback(IntPtr pData)
        {
            MeetingInvitation meetingInvitation =
                (MeetingInvitation)Marshal.PtrToStructure(pData, typeof(MeetingInvitation));

            Invitation invitation = new Invitation()
            {
                Invitor = new Attendee()
                {
                    Name = meetingInvitation.m_invitor.m_szDisplayName,
                    Id = meetingInvitation.m_invitor.m_szPhoneId
                },
                MeetingId = meetingInvitation.m_meetingId
            };

            InvitationReceivedEvent?.Invoke(invitation);
        }

        private void InviteCallback(IntPtr pData)
        {
            AsynCallResult result = (AsynCallResult)Marshal.PtrToStructure(pData, typeof(AsynCallResult));

            ReturnMessage inviteReturnMessage;
            if (result.m_rc == 0)
            {
                inviteReturnMessage = ReturnMessage.GenerateMessage(result.m_message);
            }
            else
            {
                inviteReturnMessage = ReturnMessage.GenerateError(result.m_message, result.ToString());
            }

            SetResult("InviteParticipants", inviteReturnMessage);
        }

        private void ApplyToSpeakCallback(IntPtr pData)
        {
            AsynCallResult applyResult = (AsynCallResult)Marshal.PtrToStructure(pData, typeof(AsynCallResult));

            ReturnMessage applyMessage;

            if (applyResult.m_rc == 0)
            {
                applyMessage = ReturnMessage.GenerateMessage(applyResult.m_message);
            }
            else
            {
                applyMessage = ReturnMessage.GenerateError(applyResult.m_message, applyResult.m_rc.ToString());
            }

            SetResult("ApplyToSpeak", applyMessage);
        }

        private void QueryMeetingExistCallback(IntPtr pData)
        {
            var queryMeetingExistResult =
                (AsynCallResult)Marshal.PtrToStructure(pData, typeof(AsynCallResult));

            ReturnMessage queryMeetingMsg;
            if (queryMeetingExistResult.m_rc == 0)
            {
                queryMeetingMsg = ReturnMessage.GenerateMessage(queryMeetingExistResult.m_message);
            }
            else
            {
                queryMeetingMsg = ReturnMessage.GenerateError(queryMeetingExistResult.m_message,
                    queryMeetingExistResult.m_rc.ToString());
            }

            SetResult("QueryMeetingExist", queryMeetingMsg);
        }

        private void GetMeetingListCallback(IntPtr pData)
        {
            var getMeetingListResult =
                (GetMeetingListResult)Marshal.PtrToStructure(pData, typeof(GetMeetingListResult));

            List<MeetingRecord> meetingRecords = new List<MeetingRecord>();

            if (getMeetingListResult.m_result.m_rc == 0)
            {
                int meetingInfoByte = Marshal.SizeOf(typeof(MeetingInfo));

                MeetingInfo[] meetingInfos = new MeetingInfo[getMeetingListResult.m_meetingList.m_count];

                for (int i = 0; i < getMeetingListResult.m_meetingList.m_count; i++)
                {
                    IntPtr missPtr =
                        (IntPtr)(getMeetingListResult.m_meetingList.m_pMeetings.ToInt64() + i * meetingInfoByte);
                    meetingInfos[i] = (MeetingInfo)Marshal.PtrToStructure(missPtr, typeof(MeetingInfo));
                }

                meetingInfos.ToList().ForEach((meetingInfo =>
                {
                    DateTime baseDateTime = new DateTime(1970, 1, 1);
                    baseDateTime = baseDateTime.AddSeconds(double.Parse(meetingInfo.m_szStartTime));
                    string formattedStartTime = baseDateTime.ToString("yyyy-MM-dd HH:mm:ss");

                    meetingRecords.Add(new MeetingRecord()
                    {
                        MeetingType = meetingInfo.m_meetingType,
                        CreatorPhoneId = meetingInfo.m_szCreatorId,
                        CreatorName = meetingInfo.m_szCreatorName,
                        MeetingNo = meetingInfo.m_meetingId.ToString(),
                        StartTime = formattedStartTime,
                    });
                }));
            }

            SetResult("GetMeetingRecords", meetingRecords);
            //GetMeetingListEvent?.Invoke(meetingRecords);
        }

        private void DiskSpaceNotEnoughNotify(IntPtr pData)
        {
            var diskSpaceNotEnough = (AsynCallResult)Marshal.PtrToStructure(pData, typeof(AsynCallResult));

            ReturnMessage message = MessageManager.DiskSpaceNotEnough;

            DiskSpaceNotEnough?.Invoke(message);
        }

        private void ErrorMsgReceivedCallback(IntPtr pData)
        {
            var errorMsgResult = (AsynCallResult)Marshal.PtrToStructure(pData, typeof(AsynCallResult));

            ReturnMessage message = ReturnMessage.GenerateError(errorMsgResult.m_message, errorMsgResult.m_rc.ToString());

            ErrorMsgReceivedEvent?.Invoke(message);
        }

        private void UiMessageReceivedCallback(IntPtr pData)
        {
            var message = (UIMessage)Marshal.PtrToStructure(pData, typeof(UIMessage));

            TransparentMessage transparentMessage = new TransparentMessage()
            {
                Data = message.m_szData,
                Id = message.m_messageId,
                Sender = new Attendee()
                {
                    Id = message.m_sender.m_szPhoneId,
                    Name = message.m_sender.m_szDisplayName
                }
            };

            UiMessageReceivedEvent?.Invoke(transparentMessage);
        }

        private void OtherExitMeetingCallback(IntPtr pData)
        {
            var contactInfo = (ContactInfo)Marshal.PtrToStructure(pData, typeof(ContactInfo));

            Attendee attendee = new Attendee()
            {
                Id = contactInfo.m_szPhoneId,
                Name = contactInfo.m_szDisplayName
            };

            OtherExitMeetingEvent?.Invoke(attendee);
        }

        private void OtherJoinMeetingCallback(IntPtr pData)
        {
            var contactInfo = (ContactInfo)Marshal.PtrToStructure(pData, typeof(ContactInfo));

            Attendee attendee = new Attendee()
            {
                Id = contactInfo.m_szPhoneId,
                Name = contactInfo.m_szDisplayName
            };

            OtherJoinMeetingEvent?.Invoke(attendee);
        }

        private void StopSpeakCallback(IntPtr pData)
        {
            // NO Callback Message!!!!!!

            ReturnMessage stopSpeakMessage = ReturnMessage.GenerateMessage(string.Empty);

            SetResult("StopSpeak", stopSpeakMessage);
            StopSpeakEvent?.Invoke();
        }

        private void StartSpeakCallback(IntPtr pData)
        {
            StartSpeakEvent?.Invoke();
        }

        private void ViewCloseCallback(IntPtr pData)
        {
            var speakerView = (SpeakerView)Marshal.PtrToStructure(pData, typeof(SpeakerView));

            AttendeeView attendeeView = new AttendeeView()
            {
                Attendee = new Attendee()
                {
                    Id = speakerView.m_speaker.m_szPhoneId,
                    Name = speakerView.m_speaker.m_szDisplayName,
                },
                Hwnd = speakerView.m_viewHwnd,
                ViewType = speakerView.m_viewType
            };

            ViewCloseEvent?.Invoke(attendeeView);
        }

        private void ViewCreateCallback(IntPtr pData)
        {
            var speakerView = (SpeakerView)Marshal.PtrToStructure(pData, typeof(SpeakerView));
            AttendeeView attendeeView = new AttendeeView()
            {
                Attendee = new Attendee()
                {
                    Id = speakerView.m_speaker.m_szPhoneId,
                    Name = speakerView.m_speaker.m_szDisplayName,
                },
                Hwnd = speakerView.m_viewHwnd,
                ViewType = speakerView.m_viewType
            };
            ViewCreateEvent?.Invoke(attendeeView);

        }

        private void SendUiMessageCallback(IntPtr pData)
        {
            var sendUiMsgResult = (AsynCallResult)Marshal.PtrToStructure(pData, typeof(AsynCallResult));

            ReturnMessage sendUiReturnMessage;

            if (sendUiMsgResult.m_rc == 0)
            {
                sendUiReturnMessage = ReturnMessage.GenerateMessage(sendUiMsgResult.m_message);
            }
            else
            {
                sendUiReturnMessage = ReturnMessage.GenerateError(sendUiMsgResult.m_message,
                    sendUiMsgResult.m_rc.ToString());
            }

            SetResult("SendUIMessage", sendUiReturnMessage);
        }

        private void ExitMeetingCallback(IntPtr pData)
        {
            var exitMeetingResult = (AsynCallResult)Marshal.PtrToStructure(pData, typeof(AsynCallResult));

            ReturnMessage exitMessage;

            if (exitMeetingResult.m_rc == 0)
            {
                exitMessage = ReturnMessage.GenerateMessage(exitMeetingResult.m_message);
            }
            else
            {
                exitMessage = ReturnMessage.GenerateError(exitMeetingResult.m_message, exitMeetingResult.m_rc.ToString());
            }

            ExitMeetingEvent?.Invoke();

            SetResult("ExitMeeting", exitMessage);
        }

        private void JoinMeetingCallback(IntPtr pData)
        {
            JoinMeetingResult joinMeetingResult =
                (JoinMeetingResult)Marshal.PtrToStructure(pData, typeof(JoinMeetingResult));

            ReturnMessage joinMeetingMessage;
            if (joinMeetingResult.m_result.m_rc == 0)
            {
                if (!string.IsNullOrEmpty(joinMeetingResult.m_meetingInfo.m_szTeacherPhoneId))
                {
                    MainSpeaker.Id = joinMeetingResult.m_meetingInfo.m_szTeacherPhoneId;
                }

                joinMeetingMessage = ReturnMessage.GenerateData(joinMeetingResult.m_meetingInfo.m_szTeacherPhoneId);
            }
            else
            {
                CurrentMeetingId = 0;
                switch (joinMeetingResult.m_result.m_rc)
                {
                    case 13:
                        joinMeetingMessage = MessageManager.CameraNotSet;
                        break;
                    case 14:
                        joinMeetingMessage = MessageManager.MicrophoneNotSet;
                        break;
                    case 15:
                    case -1009:
                        joinMeetingMessage = MessageManager.SpeakerNotSet;
                        break;
                    default:
                        joinMeetingMessage = MessageManager.FailedToJoinMeeting;
                        break;
                }
            }

            SetResult("JoinMeeting", joinMeetingMessage);
        }

        private void CreateMeetingCallback(IntPtr pData)
        {
            CreateMeetingResult createMeetingResult =
                (CreateMeetingResult)Marshal.PtrToStructure(pData, typeof(CreateMeetingResult));

            ReturnMessage createMeetingMessage;

            if (createMeetingResult.m_result.m_rc == 0)
            {
                MainSpeaker = CurrentAttendee;

                CurrentMeetingId = createMeetingResult.m_meetingId;

                createMeetingMessage = ReturnMessage.GenerateData(createMeetingResult.m_meetingId.ToString());
            }
            else
            {
                createMeetingMessage = ReturnMessage.GenerateError(createMeetingResult.m_result.m_message,
                    createMeetingResult.m_result.m_rc.ToString());
            }

            SetResult("CreateMeeting", createMeetingMessage);
        }

        private void SetResult(string name, object result)
        {
            if (TaskCallbacks.ContainsKey(name))
            {
                TaskCallbacks[name].SetResult(result);
                TaskCallbacks.Remove(name);
            }
        }

        private void StartCallback(IntPtr pData)
        {
            StartResult startResult = (StartResult)Marshal.PtrToStructure(pData, typeof(StartResult));

            IsServierStarted = startResult.m_result.m_rc == 0;

            ReturnMessage startReturnMessage;
            if (startResult.m_result.m_rc == 0)
            {
                CurrentAttendee.Id = GlobalData.Instance.Classroom.SchoolRoomNum;
                CurrentAttendee.Name = GlobalData.Instance.Classroom.SchoolRoomName;
                IsServierStarted = true;
                startReturnMessage = ReturnMessage.GenerateMessage(startResult.m_result.m_message);
            }
            else
            {
                IsServierStarted = false;
                startReturnMessage = ReturnMessage.GenerateError(startResult.m_result.m_message);
            }

            SetResult("Start", startReturnMessage);
        }

        public void Stop()
        {
            int result = MeetingAgent.Stop();
            IsServierStarted = result != 0;
        }

        public Task<ReturnMessage> CheckMeetingServerStatus()
        {
            return IsServierStarted ? null : Task.FromResult(MessageManager.ServerNotStartedMessage);
        }

        public Task<ReturnMessage> ShowQosView()
        {
            Task<ReturnMessage> checkTask = CheckMeetingServerStatus();
            if (checkTask != null)
            {
                return checkTask;
            }

            int result = MeetingAgent.ShowQosView();

            if (result != 0)
            {
                return Task.FromResult(MessageManager.OpenQosFailure);
            }

            return Task.FromResult(ReturnMessage.GenerateMessage(""));
        }

        public Task<ReturnMessage> CloseQosView()
        {
            Task<ReturnMessage> checkTask = CheckMeetingServerStatus();
            if (checkTask != null)
            {
                return checkTask;
            }

            int result = MeetingAgent.CloseQosView();

            if (result != 0)
            {
                return Task.FromResult(MessageManager.CloseQosFailure);
            }

            return Task.FromResult(ReturnMessage.GenerateMessage(""));
        }

        public event ViewChange ViewCreateEvent;
        public event ViewChange ViewCloseEvent;
        public event Action StartSpeakEvent;
        public event Action StopSpeakEvent;
        public event Action ExitMeetingEvent;
        public event GetMeetingListDelegate GetMeetingListEvent;
        public event AttendeeChange OtherJoinMeetingEvent;
        public event AttendeeChange OtherExitMeetingEvent;
        public event ReceiveUIMessage UiMessageReceivedEvent;
        public event ReceiveMsg ErrorMsgReceivedEvent;
        public event ReceiveInvitation InvitationReceivedEvent;
        public event ReceiveMsg KickedByHostEvent;
        public event ReceiveMsg DiskSpaceNotEnough;
        public event ReceiveMsg OpenCameraEvent;
    }
}
