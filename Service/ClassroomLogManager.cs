using Common.Contract;
using Serilog;
using System;
using System.IO;
using System.Reflection;

namespace Service
{
    public class ClassroomLogManager : ILogManager
    {
        public void CreateLogFile()
        {
            string logPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "logs");

            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            logPath += "\\log.txt";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile(logPath)
                .CreateLogger();
        }
    }
}
