using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Classroom.View;
using Common.Contract;
using Common.CustomControl;
using Common.Helper;
using Prism.Commands;
using Serilog;
using Common.Model;
using Service;
using MeetingSdk.NetAgent;
using MeetingSdk.NetAgent.Models;
using Common.UiMessage;
using Prism.Events;
using MeetingSdk.Wpf;
using Caliburn.Micro;
using System.Reflection;
using Classroom.Service;

namespace Classroom.ViewModel
{
    public class LoginViewModel
    {

        private readonly IBms _bmsService;


        private readonly IMeetingSdkAgent _meetingSdkAgent;
        private readonly IEventAggregator _eventAggregator;
        private readonly IMeetingWindowManager _windowManager;

        private readonly ILocalDataManager _localDataManager;
        private readonly INetCheckService _netCheckService;
        private string _imei;
        private readonly LoginView _loginView;

        public LoginViewModel(LoginView loginView)
        {
            _loginView = loginView;
            _netCheckService = DependencyResolver.Current.GetService<INetCheckService>();
            _localDataManager = DependencyResolver.Current.GetService<ILocalDataManager>();
            _bmsService = DependencyResolver.Current.GetService<IBms>();

            _meetingSdkAgent = DependencyResolver.Current.GetService<IMeetingSdkAgent>();
            _eventAggregator = DependencyResolver.Current.GetService<IEventAggregator>();
            _windowManager = DependencyResolver.Current.GetService<IMeetingWindowManager>();

            LoginingCommand = DelegateCommand.FromAsyncHandler(LoginingAsync);

            //RegisterEvents();
            GlobalData.Instance.ModeList = new List<Mode>
            {
                new Mode {Name = "自动", ReceiveCommand = true},
                 new Mode {Name = "键盘", ReceiveCommand = false}
            };
            GlobalData.Instance.CurrentMode = GlobalData.Instance.ModeList.FirstOrDefault();
        }

        private async Task LoginingAsync()
        {
            InvitationService.Instance.Initialize();
            _windowManager.Initialize();

            MeetingSdkEventsRegister.Instance.RegisterSdkEvents();

            var deviceNameAccessor = IoC.Get<IDeviceNameAccessor>();
            var providers = IoC.GetAll<IDeviceNameProvider>();
            foreach (var provider in providers)
            {
                await provider.Provider(deviceNameAccessor);
            }

            // read config and cache.
            LoadLocalConfiger();
            //与录播系统对接
            RecordingSystemService.Instance.Start();
            RecordingSystemService.Instance.SetControlComand(ControlComand.ResetWorkMode);
            bool getClassroomResult = await GetClassroomAsync();
            await _bmsService.GetClassroomsAsync();
            if (getClassroomResult)
            {
                bool startSdkResult = await StartSdkAsync();
                if (!startSdkResult)
                    startSdkResult = await StartSdkAsync();

                if (startSdkResult)
                {
                    var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    //path = Path.Combine(path, "sdk");

                    var startHostResult = await _meetingSdkAgent.StartHost("PCJM", path);
                    if (startHostResult.StatusCode != 0)
                    {
                        MessageQueueManager.Instance.AddInfo("Host服务器启动失败！");
                    }

                    var meetingResult = await _meetingSdkAgent.LoginViaImei(_imei);
                    LoginModel loginModel = meetingResult.Result;

                    if (meetingResult.StatusCode == 0)
                    {
                        MessageQueueManager.Instance.AddInfo("登录成功！");

                        _eventAggregator.GetEvent<UserLoginEvent>().Publish(new UserInfo()
                        {
                            UserId = loginModel.Account.AccountId,
                            UserName = GlobalData.Instance.Classroom.SchoolRoomName,
                        });

                        var connectVdnResult = await _meetingSdkAgent.ConnectMeetingVDN(loginModel.Account.AccountId, loginModel.Account.AccountName, loginModel.Token);

                        if (connectVdnResult.StatusCode != 0)
                        {
                            MessageQueueManager.Instance.AddInfo("连接Host服务器失败！");
                        }
                    }
                    else
                    {
                        MessageQueueManager.Instance.AddError("登录失败！");
                        Dialog errorDialog = new Dialog($"登录失败！{meetingResult.Message}");
                        errorDialog.ShowDialog();
                    }

                    //启动网络检测
                    _netCheckService.StartCheckNetConnect();

                    //var localSetting = _localDataManager.GetSettingConfig();
                    if (GlobalData.Instance.LocalSetting.UseWelcome)
                    {
                        var welcome = DependencyResolver.Current.GetService<WelcomeView>();
                        welcome.Show();
                    }
                    else
                    {
                        MainView mainView = new MainView(MessageManager.ServerStarted);
                        mainView.Show();
                    }
                    //DeleteClosedMeetingFromLocalData();
                    _loginView.Close();

                }
            }
        }


        private void LoadLocalConfiger()
        {
            GlobalData.Instance.LocalSetting = _localDataManager.GetSettingConfig() ?? new LocalSetting();

            GlobalData.Instance.ConfigManager = new ConfigManager() { AudioInfo = new AudioInfo(), LocalLiveStreamInfo = new LiveStreamInfo(), NetInfo = new NetInfo(), RecordInfo = new RecordInfo(), RemoteLiveStreamInfo = new LiveStreamInfo(), ServerInfo = new ServerInfo(), MainVideoInfo = new VideoInfo(), DocVideoInfo = new VideoInfo() };
            var localDataConfig = _localDataManager.GetSettingConfigData();
            if (localDataConfig != null)
            {
                if (localDataConfig.ServerInfo == null || localDataConfig.ServerInfo.BmsServerPort == 0)
                {
                    localDataConfig.ServerInfo = new ServerInfo() { ServerIp = GlobalData.Instance.LocalSetting.ServerIp, BmsServerPort = GlobalData.Instance.LocalSetting.BmsServerPort };
                    _localDataManager.SaveSettingConfigData(localDataConfig);
                }
                GlobalData.Instance.ConfigManager = localDataConfig;
            }
            else
            {
                localDataConfig = GlobalData.Instance.ConfigManager;
                localDataConfig.ServerInfo = new ServerInfo() { ServerIp = GlobalData.Instance.LocalSetting.ServerIp, BmsServerPort = GlobalData.Instance.LocalSetting.BmsServerPort };
                _localDataManager.SaveSettingConfigData(localDataConfig);
                GlobalData.Instance.ConfigManager = _localDataManager.GetSettingConfigData();
            }

        }

        private async Task<bool> GetClassroomAsync()
        {
            try
            {
                //_imei = "BOX708BCD567E45";
                _imei = _meetingSdkAgent.GetSerialNo()?.Result;

                ReturnMessage bmsMessage = await _bmsService.GetClassroomAsync(_imei);

                if (bmsMessage.HasError)
                {
                    string errorMsg = bmsMessage.Status == "-1" ? $"设备号：{_imei}未注册！" : bmsMessage.Message;
                    Dialog errorDialog = new Dialog(errorMsg);
                    errorDialog.ShowDialog();
                }
                else
                {
                    GlobalData.Instance.Classroom = bmsMessage.Data as Common.Model.Classroom;

                    var classTable = await _bmsService.GetClassTableInfoAsync(GlobalData.Instance.Classroom?.Id);

                    ClassScheduleModel.DoUpdateCurriculumMeetingN0(classTable);
                }
                return !bmsMessage.HasError;
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"GetClassroom exception：{ex}");
                string errInfo = MessageManager.ErrorGetClassroom + $"{ex.Message}";
                Dialog errorDialog = new Dialog(errInfo);
                errorDialog.ShowDialog();
                return false;
            }
        }

        private async Task<bool> StartSdkAsync()
        {
            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            //path = Path.Combine(path, "sdk");

            MeetingResult result = await _meetingSdkAgent.Start("PCJM", path);

            if (result.StatusCode != 0)
            {
                MessageQueueManager.Instance.AddError("启动失败！");
                Dialog errorDialog = new Dialog($"启动失败！{result.Message}");
                errorDialog.ShowDialog();
            }
            else
            {
                MessageQueueManager.Instance.AddInfo("启动成功！");
            }

            return result.StatusCode == 0;
        }

        //private void DeleteClosedMeetingFromLocalData()
        //{
        //    var meetingList = _localDataManager.GetMeetingList();
        //    if (meetingList != null && meetingList.MeetingInfos.Any())
        //    {
        //        var lastMonth = DateTime.Now.AddMonths(-1).Month;
        //        var delList = meetingList.MeetingInfos.Where(o => o.LastActivityTime.Month == lastMonth);
        //        var meetingItems = delList as MeetingItem[] ?? delList.ToArray();
        //        if (meetingItems.Any())
        //        {
        //            foreach (var meetingItem in meetingItems)
        //            {
        //                meetingList.MeetingInfos.Remove(meetingItem);
        //            }
        //        }
        //        _localDataManager.SaveMeetingList(meetingList);
        //    }
        //}

        public ICommand LoginingCommand { get; set; }


        //public void RegisterEvents()
        //{
        //    _meetingService.ReceiveInvitationEvent += InvitationService.Instance._meetingManagerService_InvitationReceivedEvent;
        //}
    }
}
