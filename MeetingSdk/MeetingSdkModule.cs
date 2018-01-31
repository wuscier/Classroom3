using System;
using Autofac;
using Common.Contract.MeetingSdk;
using MeetingSdk.Service;
using Serilog;

namespace MeetingSdk
{
    public class MeetingSdkModule:Module
    {

        public MeetingSdkModule()
        {
            Log.Logger.Debug("construct MeetingSdkModule");
        }

        protected override void Load(ContainerBuilder builder)
        {
            Log.Logger.Debug("Register ServiceModule Components");

            builder.RegisterType<MeetingManagerService>().As<IMeetingManager>().SingleInstance();

            builder.RegisterType<MeetingParameterService>().As<IMeetingParameter>().SingleInstance();
            builder.RegisterType<MeetingControllerService>().As<IMeetingController>().SingleInstance();
        }
    }
}
