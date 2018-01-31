using MeetingSdk.NetAgent;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class SerilogLogger : IMeetingLogger
    {
        public void LogError(Exception e, string message)
        {
            Log.Logger.Error(e, message);
        }

        public void LogMessage(string message)
        {
            Log.Logger.Information(message);
        }
    }
}
