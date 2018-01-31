using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Common.Contract.MeetingSdk;
using Common.Helper;
using Serilog;

namespace MeetingSdk.Service
{
    public class MeetingControllerService : IMeetingController
    {
        private readonly IMeetingManager _meetingManager;

        public MeetingControllerService()
        {
            _meetingManager = DependencyResolver.Current.GetService<IMeetingManager>();
        }

        public Task<ReturnMessage> CreateMeeting(Attendee[] attendees)
        {
            Task<ReturnMessage> checkTask = _meetingManager.CheckMeetingServerStatus();
            if (checkTask != null)
            {
                return checkTask;
            }

            var tcs = new TaskCallback<ReturnMessage>("CreateMeeting");

            if (_meetingManager.TaskCallbacks.ContainsKey(tcs.Name))
            {
                _meetingManager.TaskCallbacks.Remove(tcs.Name);
            }

            _meetingManager.TaskCallbacks.Add(tcs.Name, tcs);

            List<ContactInfo> contactInfos = new List<ContactInfo>();

            foreach (var attendee in attendees)
            {
                ContactInfo newContactInfo = new ContactInfo()
                {
                    m_szDisplayName = attendee.Name,
                    m_szPhoneId = attendee.Id
                };

                contactInfos.Add(newContactInfo);
            }

            int result = MeetingAgent.CreateMeeting(contactInfos.ToArray(), contactInfos.Count);

            if (result != 0)
            {
                Task<ReturnMessage> resulTask;
                switch (result)
                {
                    case 13:
                        resulTask = Task.FromResult(MessageManager.CameraNotSet);
                        break;
                    case 14:
                        resulTask = Task.FromResult(MessageManager.MicrophoneNotSet);
                        break;
                    case 15:
                    case -1009:
                        resulTask = Task.FromResult(MessageManager.SpeakerNotSet);
                        break;
                    default:
                        resulTask = Task.FromResult(MessageManager.FailedToCreateMeeting);
                        break;
                }
                return resulTask;
            }

            return tcs.Task;
        }

        public Task<ReturnMessage> JoinMeeting(int meetingId, uint[] hwnds, int count, uint[] docHwnds, int docCount)
        {
            Task<ReturnMessage> checkTask = _meetingManager.CheckMeetingServerStatus();
            if (checkTask != null)
            {
                return checkTask;
            }

            var tcs = new TaskCallback<ReturnMessage>("JoinMeeting");

            if (_meetingManager.TaskCallbacks.ContainsKey(tcs.Name))
            {
                _meetingManager.TaskCallbacks.Remove(tcs.Name);
            }

            _meetingManager.TaskCallbacks.Add(tcs.Name, tcs);

            int result = MeetingAgent.JoinMeeting(meetingId, hwnds, count, docHwnds, docCount);

            if (result != 0)
            {
                _meetingManager.CurrentMeetingId = 0;
                return Task.FromResult(MessageManager.FailedToJoinMeeting);
            }

            return tcs.Task;
        }

        public Task<ReturnMessage> ExitMeeting()
        {
            Task<ReturnMessage> checkTask = _meetingManager.CheckMeetingServerStatus();
            if (checkTask != null)
            {
                return checkTask;
            }

            var tcs = new TaskCallback<ReturnMessage>("ExitMeeting");

            if (_meetingManager.TaskCallbacks.ContainsKey(tcs.Name))
                _meetingManager.TaskCallbacks.Remove(tcs.Name);

            _meetingManager.TaskCallbacks.Add(tcs.Name, tcs);

            int result = MeetingAgent.ExitMeeting();

            if (result != 0)
            {
                return Task.FromResult(MessageManager.FailedToExitMeeting);
            }

            return tcs.Task;
        }

        public List<Attendee> GetAttendees()
        {
            List<Attendee> attendees = new List<Attendee>();

            var participantsPtr = IntPtr.Zero;

            if (!_meetingManager.IsServierStarted)
                return attendees;

            try
            {
                int maxParticipantCount = 100, getParticipantCount = 0;
                var participantByte = Marshal.SizeOf(typeof(ParticipantInfo));
                var maxParticipantBytes = participantByte * maxParticipantCount;

                participantsPtr = Marshal.AllocHGlobal(maxParticipantBytes);
                var result = MeetingAgent.GetParticipantsEx(participantsPtr, ref getParticipantCount,
                    maxParticipantCount);
                if (result != 0)
                {
                }

                var participants = new List<ParticipantInfo>();
                for (var i = 0; i < getParticipantCount; i++)
                {
                    var pointer = new IntPtr(participantsPtr.ToInt64() + participantByte * i);
                    var participant =
                        (ParticipantInfo)Marshal.PtrToStructure(pointer, typeof(ParticipantInfo));
                    participants.Add(participant);
                }

                participants.ForEach(p =>
                {
                    attendees.Add(new Attendee()
                    {
                        Id = p.m_contactInfo.m_szPhoneId,
                        Name = p.m_contactInfo.m_szDisplayName,
                        IsSpeaking = p.m_bIsSpeaking == 1
                    });
                });

                return attendees;
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"GetAttendees() excpetion：{ex}");
                return attendees;
            }
            finally
            {
                if (participantsPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(participantsPtr);
            }
        }

        public Task<ReturnMessage> SendUiMessage(int messageId, string pData, int dataLength, string targetPhoneId)
        {
            Task<ReturnMessage> checkTask = _meetingManager.CheckMeetingServerStatus();
            if (checkTask != null)
            {
                return checkTask;
            }

            var tcs = new TaskCallback<ReturnMessage>("SendUIMessage");

            if (_meetingManager.TaskCallbacks.ContainsKey(tcs.Name))
                _meetingManager.TaskCallbacks.Remove(tcs.Name);

            _meetingManager.TaskCallbacks.Add(tcs.Name, tcs);

            int result = MeetingAgent.SendUIMessage(messageId, pData, dataLength, targetPhoneId);

            if (result != 0)
                return Task.FromResult(MessageManager.FailedToSendUiMessage);

            return tcs.Task;
        }

        public Task<List<MeetingRecord>> GetMeetingRecords()
        {
            List<MeetingRecord> meetingRecords = new List<MeetingRecord>();

            if (!_meetingManager.IsServierStarted)
                return Task.FromResult(meetingRecords);

            var tcs = new TaskCallback<List<MeetingRecord>>("GetMeetingRecords");

            if (_meetingManager.TaskCallbacks.ContainsKey(tcs.Name))
                _meetingManager.TaskCallbacks.Remove(tcs.Name);

            _meetingManager.TaskCallbacks.Add(tcs.Name, tcs);

            int result = MeetingAgent.GetMeetingList();

            if (result != 0)
                return Task.FromResult(meetingRecords);

            return tcs.Task;
        }

        public Task<ReturnMessage> QueryMeetingExist(int meetingId)
        {
            Task<ReturnMessage> checkTask = _meetingManager.CheckMeetingServerStatus();
            if (checkTask != null)
            {
                return checkTask;
            }

            var tcs = new TaskCallback<ReturnMessage>("QueryMeetingExist");

            if (_meetingManager.TaskCallbacks.ContainsKey(tcs.Name))
                _meetingManager.TaskCallbacks.Remove(tcs.Name);

            _meetingManager.TaskCallbacks.Add(tcs.Name, tcs);

            var result = MeetingAgent.QueryMeetingExist(meetingId);

            if (result != 0)
            {
                return Task.FromResult(MessageManager.FailedToQueryMeeting);
            }

            return tcs.Task;
        }

        public Task<ReturnMessage> ApplyToSpeak()
        {
            Task<ReturnMessage> checkTask = _meetingManager.CheckMeetingServerStatus();
            if (checkTask != null)
            {
                return checkTask;
            }

            var tcs = new TaskCallback<ReturnMessage>("ApplyToSpeak");

            if (_meetingManager.TaskCallbacks.ContainsKey(tcs.Name))
                _meetingManager.TaskCallbacks.Remove(tcs.Name);

            _meetingManager.TaskCallbacks.Add(tcs.Name, tcs);

            var result = MeetingAgent.ApplyToSpeak();

            if (result != 0)
            {
                return Task.FromResult(MessageManager.FailedToSpeak);
            }

            return tcs.Task;

        }

        public Task<ReturnMessage> StopSpeak()
        {
            Task<ReturnMessage> checkTask = _meetingManager.CheckMeetingServerStatus();
            if (checkTask != null)
            {
                return checkTask;
            }

            var tcs = new TaskCallback<ReturnMessage>("StopSpeak");

            if (_meetingManager.TaskCallbacks.ContainsKey(tcs.Name))
                _meetingManager.TaskCallbacks.Remove(tcs.Name);

            _meetingManager.TaskCallbacks.Add(tcs.Name, tcs);

            var result = MeetingAgent.StopSpeak();

            if (result != 0)
            {
                return Task.FromResult(MessageManager.FailedToStopSpeak);
            }

            return tcs.Task;
        }

        public Task<ReturnMessage> InviteParticipants(Attendee[] attendees)
        {
            Task<ReturnMessage> checkTask = _meetingManager.CheckMeetingServerStatus();
            if (checkTask != null)
            {
                return checkTask;
            }

            var tcs = new TaskCallback<ReturnMessage>("InviteParticipants");

            if (_meetingManager.TaskCallbacks.ContainsKey(tcs.Name))
                _meetingManager.TaskCallbacks.Remove(tcs.Name);

            _meetingManager.TaskCallbacks.Add(tcs.Name, tcs);

            List<ContactInfo> contactInfos = new List<ContactInfo>();
            foreach (var attendee in attendees)
            {
                contactInfos.Add(new ContactInfo()
                {
                    m_szDisplayName = attendee.Name,
                    m_szPhoneId = attendee.Id
                });
            }

            var result = MeetingAgent.InviteParticipants(_meetingManager.CurrentMeetingId, contactInfos.ToArray(),
                contactInfos.Count);

            if (result != 0)
            {
                return Task.FromResult(MessageManager.FailedToInviteParticipants);
            }

            return tcs.Task;
        }

        public Task<ReturnMessage> StartShareDoc()
        {
            Task<ReturnMessage> checkTask = _meetingManager.CheckMeetingServerStatus();
            if (checkTask != null)
            {
                return checkTask;
            }

            var tcs = new TaskCallback<ReturnMessage>("StartShareDoc");

            if (_meetingManager.TaskCallbacks.ContainsKey(tcs.Name))
                _meetingManager.TaskCallbacks.Remove(tcs.Name);

            _meetingManager.TaskCallbacks.Add(tcs.Name, tcs);

            var result = MeetingAgent.StartShareDoc();

            if (result != 0)
            {
                ReturnMessage returnMessage;
                switch (result)
                {
                    case -1006:
                        returnMessage = MessageManager.DocVideoNotSet;
                        break;
                    case -1007:
                        returnMessage = MessageManager.DocAudioNotSet;
                        break;
                    default:
                        returnMessage = MessageManager.FailedToStartShareDoc;
                        break;
                }
                return Task.FromResult(returnMessage);
            }

            return tcs.Task;
        }

        public Task<ReturnMessage> StopShareDoc()
        {
            Task<ReturnMessage> checkTask = _meetingManager.CheckMeetingServerStatus();
            if (checkTask != null)
            {
                return checkTask;
            }

            var tcs = new TaskCallback<ReturnMessage>("StopShareDoc");

            if (_meetingManager.TaskCallbacks.ContainsKey(tcs.Name))
                _meetingManager.TaskCallbacks.Remove(tcs.Name);

            _meetingManager.TaskCallbacks.Add(tcs.Name, tcs);

            var result = MeetingAgent.StopShareDoc();

            if (result != 0)
            {
                return Task.FromResult(MessageManager.FailedToStopShareDoc);
            }

            return tcs.Task;
        }

        public bool IsSharedDocOpened()
        {
            Task<ReturnMessage> checkTask = _meetingManager.CheckMeetingServerStatus();
            if (checkTask != null)
            {
                return false;
            }

            int result = MeetingAgent.IsShareDocOpened();

            return result != 0;
        }

        public Task<ReturnMessage> StartLiveStream(LiveParameter liveParameter, VideoStreamInfo[] videoStreamInfos, int count)
        {
            var checkTask = _meetingManager.CheckMeetingServerStatus();
            if (checkTask != null)
            {
                return checkTask;
            }

            var tcs = new TaskCallback<ReturnMessage>("StartLiveStream");

            if (_meetingManager.TaskCallbacks.ContainsKey(tcs.Name))
                _meetingManager.TaskCallbacks.Remove(tcs.Name);

            _meetingManager.TaskCallbacks.Add(tcs.Name, tcs);


            var liveParam = new LiveParam()
            {
                m_nAudioBitrate = 64,
                m_nBitsPerSample = 16,
                m_nChannels = 1,
                m_nHeight = liveParameter.Height,
                m_nIsLive = 1,
                m_nIsRecord = 0,
                m_nSampleRate = 8000,
                m_nVideoBitrate = liveParameter.VideoBitrate,
                m_nWidth = liveParameter.Width,
                m_sRecordFilePath = liveParameter.RecordFilePath,
               // m_url1 = "http://gslb.butel.com/live/live.butel.com/3a9d",
                m_url1 = liveParameter.Url1,
                m_url2 = liveParameter.Url2
            };

            var liveVideoStreamInfos = new List<LiveVideoStreamInfo>();

            foreach (var videoStreamInfo in videoStreamInfos)
            {
                liveVideoStreamInfos.Add(new LiveVideoStreamInfo()
                {
                    Handle = videoStreamInfo.Handle,
                    Height = videoStreamInfo.Height,
                    Width = videoStreamInfo.Width,
                    XLocation = videoStreamInfo.X,
                    YLocation = videoStreamInfo.Y
                });
            }

            var result = MeetingAgent.StartLiveStream(liveParam, liveVideoStreamInfos.ToArray(),
                liveVideoStreamInfos.Count);

            return result != 0 ? Task.FromResult(MessageManager.FailedToStartLiveStream) : tcs.Task;
        }

        public Task<ReturnMessage> StopLiveStream(int liveId)
        {
            Task<ReturnMessage> checkTask = _meetingManager.CheckMeetingServerStatus();
            if (checkTask != null)
            {
                return checkTask;
            }

            var tcs = new TaskCallback<ReturnMessage>("StopLiveStream");

            if (_meetingManager.TaskCallbacks.ContainsKey(tcs.Name))
                _meetingManager.TaskCallbacks.Remove(tcs.Name);

            _meetingManager.TaskCallbacks.Add(tcs.Name, tcs);

            var result = MeetingAgent.StopLiveStream(liveId);

            if (result != 0)
            {
                return Task.FromResult(MessageManager.FailedToStopLiveStream);
            }

            return tcs.Task;
        }

        public Task<ReturnMessage> UpdateLiveVideoStreams(int liveId, VideoStreamInfo[] videoStreamInfos, int count)
        {
            Task<ReturnMessage> checkTask = _meetingManager.CheckMeetingServerStatus();
            if (checkTask != null)
            {
                return checkTask;
            }

            var tcs = new TaskCallback<ReturnMessage>("UpdateLiveVideoStreams");

            if (_meetingManager.TaskCallbacks.ContainsKey(tcs.Name))
                _meetingManager.TaskCallbacks.Remove(tcs.Name);

            _meetingManager.TaskCallbacks.Add(tcs.Name, tcs);


            List<LiveVideoStreamInfo> liveVideoStreamInfos = new List<LiveVideoStreamInfo>();

            foreach (var videoStreamInfo in videoStreamInfos)
            {
                liveVideoStreamInfos.Add(new LiveVideoStreamInfo()
                {
                    Handle = videoStreamInfo.Handle,
                    Height = videoStreamInfo.Height,
                    Width = videoStreamInfo.Width,
                    XLocation = videoStreamInfo.X,
                    YLocation = videoStreamInfo.Y
                });
            }

            var result = MeetingAgent.UpdateLiveVideoStreams(liveId, liveVideoStreamInfos.ToArray(),
                liveVideoStreamInfos.Count);

            if (result != 0)
            {
                return Task.FromResult(MessageManager.FailedToRefreshLiveStream);
            }

            return tcs.Task;
        }

        public Task<ReturnMessage> StartRecord(string fileName, VideoStreamInfo[] videoStreamInfos, int count)
        {
            Task<ReturnMessage> checkTask = _meetingManager.CheckMeetingServerStatus();
            if (checkTask != null)
            {
                return checkTask;
            }

            var tcs = new TaskCallback<ReturnMessage>("StartRecord");

            if (_meetingManager.TaskCallbacks.ContainsKey(tcs.Name))
                _meetingManager.TaskCallbacks.Remove(tcs.Name);

            _meetingManager.TaskCallbacks.Add(tcs.Name, tcs);


            List<LiveVideoStreamInfo> liveVideoStreamInfos = new List<LiveVideoStreamInfo>();

            foreach (var videoStreamInfo in videoStreamInfos)
            {
                liveVideoStreamInfos.Add(new LiveVideoStreamInfo()
                {
                    Handle = videoStreamInfo.Handle,
                    Height = videoStreamInfo.Height,
                    Width = videoStreamInfo.Width,
                    XLocation = videoStreamInfo.X,
                    YLocation = videoStreamInfo.Y
                });
            }

            var result = MeetingAgent.StartRecord(fileName, liveVideoStreamInfos.ToArray(),
                liveVideoStreamInfos.Count);

            if (result != 0)
            {
                return Task.FromResult(MessageManager.FailedToStartRecord);
            }

            return tcs.Task;
        }

        public Task<ReturnMessage> StopRecord()
        {
            Task<ReturnMessage> checkTask = _meetingManager.CheckMeetingServerStatus();
            if (checkTask != null)
            {
                return checkTask;
            }

            var tcs = new TaskCallback<ReturnMessage>("StopRecord");

            if (_meetingManager.TaskCallbacks.ContainsKey(tcs.Name))
                _meetingManager.TaskCallbacks.Remove(tcs.Name);

            _meetingManager.TaskCallbacks.Add(tcs.Name, tcs);

            var result = MeetingAgent.StopRecord();

            if (result != 0)
            {
                return Task.FromResult(MessageManager.FailedToStopRecord);
            }

            return tcs.Task;
        }

        public Task<ReturnMessage> SetRecordDirectory(string recordDir)
        {
            int result = MeetingAgent.SetRecordDirectory(recordDir);

            if (result != 0)
            {
                return Task.FromResult(MessageManager.FailedToSetRecordDirectory);
            }
            return Task.FromResult(ReturnMessage.GenerateMessage(""));
        }

        public Task<ReturnMessage> SetRecordParameter(RecordParameter recordParameter)
        {
            Task<ReturnMessage> checkTask = _meetingManager.CheckMeetingServerStatus();
            if (checkTask != null)
            {
                return checkTask;
            }

            var tcs = new TaskCallback<ReturnMessage>("SetRecordParameter");

            if (_meetingManager.TaskCallbacks.ContainsKey(tcs.Name))
                _meetingManager.TaskCallbacks.Remove(tcs.Name);

            _meetingManager.TaskCallbacks.Add(tcs.Name, tcs);


            RecordParam recordParam = new RecordParam()
            {
                AudioBitrate = recordParameter.AudioBitrate,
                BitsPerSample = recordParameter.BitsPerSample,
                Channels = recordParameter.Channels,
                Height = recordParameter.Height,
                SampleRate = recordParameter.SampleRate,
                VideoBitrate = recordParameter.VideoBitrate,
                Width = recordParameter.Width
            };

            var result = MeetingAgent.SetRecordParam(recordParam);

            if (result != 0)
            {
                return Task.FromResult(MessageManager.FailedToSetRecordParameter);
            }

            return tcs.Task;
        }


        public Task<ReturnMessage> SetDoubleScreenRender(string phoneId, int mediaType, int isRenderOnDoubleScreen,
            IntPtr displayWindowIntPtr)
        {
            Task<ReturnMessage> checkTask = _meetingManager.CheckMeetingServerStatus();
            if (checkTask != null)
            {
                return checkTask;
            }
            var tcs = new TaskCallback<ReturnMessage>("SetDoubleScreenRender");

            if (_meetingManager.TaskCallbacks.ContainsKey(tcs.Name))
                _meetingManager.TaskCallbacks.Remove(tcs.Name);

            _meetingManager.TaskCallbacks.Add(tcs.Name, tcs);
            Log.Logger.Debug($"双屏回调参数{phoneId}-{mediaType}-{isRenderOnDoubleScreen}-{displayWindowIntPtr}");
            int result = MeetingAgent.SetDoubleScreenRender(phoneId, mediaType, isRenderOnDoubleScreen,
               displayWindowIntPtr);

            if (result != 0)
            {
                return Task.FromResult(MessageManager.SetDoubleScreen);
            }

            return tcs.Task;
        }
    }
}
