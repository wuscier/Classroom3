using Autofac;
using Common.Contract;
using Serilog;
using MeetingSdk.NetAgent;
using MeetingSdk.Wpf;
using Prism.Events;

namespace Service
{
    public class ServiceModule : Module
    {
        public ServiceModule()
        {
            //Log.Logger.Debug("construct ServiceModule");
        }

        protected override void Load(ContainerBuilder builder)
        {
            //Log.Logger.Debug("Register ServiceModule Components");
            builder.RegisterType<ClassroomLogManager>().As<ILogManager>().SingleInstance();

            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();

            //builder.RegisterType<SerilogLogger>().As<IMeetingLogger>().SingleInstance();

            builder.RegisterInstance(DefaultMeetingSdkAgent.Instance).As<IMeetingSdkAgent>().SingleInstance();
            builder.RegisterType<MeetingWindowManager>().As<IMeetingWindowManager>().SingleInstance();
            builder.RegisterType<DeviceConfigLoader>().As<IDeviceConfigLoader>().SingleInstance();
            builder.RegisterType<DeviceNameAccessor>().As<IDeviceNameAccessor>().SingleInstance();
            builder.RegisterType<DeviceNameProvider>().As<IDeviceNameProvider>().SingleInstance();
            builder.RegisterType<VideoBoxManager>().As<IVideoBoxManager>();
            builder.RegisterType<ExtendedWindowManager>().As<IExtendedWindowManager>().SingleInstance();

            //builder.RegisterType<AutoLayoutRenderer>().Named<ILayoutRenderrer>("AutoLayout");
            builder.RegisterType<AverageLayoutRenderer>().Named<ILayoutRenderer>("AverageLayout");
            builder.RegisterType<BigSmallsLayoutRenderer>().Named<ILayoutRenderer>("BigSmallsLayout");
            builder.RegisterType<CloseupLayoutRenderer>().Named<ILayoutRenderer>("CloseupLayout");

            builder.RegisterType<SpeakerModeDisplayer>().Named<IModeDisplayer>("SpeakerMode");
            builder.RegisterType<ShareModeDisplayer>().Named<IModeDisplayer>("ShareMode");
            builder.RegisterType<InteractionModeDisplayer>().Named<IModeDisplayer>("InteractionMode");

            builder.RegisterType<PublishMicStreamParameterProvider>().As<IStreamParameterProvider<PublishMicStreamParameter>>().SingleInstance();
            builder.RegisterType<PublishCameraStreamParameterProvider>().As<IStreamParameterProvider<PublishCameraStreamParameter>>().SingleInstance();
            builder.RegisterType<PublishDataCardStreamParameterProvider>().As<IStreamParameterProvider<PublishDataCardStreamParameter>>().SingleInstance();
            builder.RegisterType<PublishWinCaptureStreamParameterProvider>().As<IStreamParameterProvider<PublishWinCaptureStreamParameter>>().SingleInstance();
            builder.RegisterType<SubscribeMicStreamParameterProvider>().As<IStreamParameterProvider<SubscribeMicStreamParameter>>().SingleInstance();
            builder.RegisterType<SubscribeCameraStreamParameterProvider>().As<IStreamParameterProvider<SubscribeCameraStreamParameter>>().SingleInstance();
            builder.RegisterType<SubscribeDataCardStreamParameterProvider>().As<IStreamParameterProvider<SubscribeDataCardStreamParameter>>().SingleInstance();
            builder.RegisterType<SubscribeWinCaptureStreamParameterProvider>().As<IStreamParameterProvider<SubscribeWinCaptureStreamParameter>>().SingleInstance();

            builder.RegisterType<ClassroomBms>().As<IBms>().SingleInstance();
            builder.RegisterType<LocalDataManager>().As<ILocalDataManager>().SingleInstance();

            builder.RegisterType<LocalRecordService>().As<IRecordLive>().SingleInstance();

            builder.RegisterType<ManualPushLiveService>().Named<IPushLive>("ManualPushLive").SingleInstance();
            builder.RegisterType<NetCheckService>().As<INetCheckService>().SingleInstance();
            builder.RegisterType<RemoteRecordService>().As<IRemoteRecord>().SingleInstance();

            builder.RegisterType<ClassScheduleService>().As<IClassScheduleService>().SingleInstance();
        }
    }
}
