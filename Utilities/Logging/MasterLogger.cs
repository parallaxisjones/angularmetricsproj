using System;
using Playverse.Utilities;
using System.Diagnostics;

namespace Utilities
{
    public class Logger
    {

       // private EventLogger _eventLog = new EventLogger("MoniverseSource", "MoniverseLog");
        //private DBLogger _dbLogger = new DBLogger("Moniverse");
        private string LOGPATH = "C:/Users/Public/";
        private string logfile = "";
#if DEBUG || LOCAL || STAGING
        public static bool bInfoOn = false;
        public static bool bWriteToConsole = true;
#else
        public static bool bInfoOn = true;
        public static bool bWriteToConsole = true;
#endif

        private static object locker = new Object();
        public static Logger Instance = new Logger();
        LogWriter writer = LogWriter.Instance;

        //public EventLog EventLog { get { return _eventLog.EventLog; } }

        public string RunningVersion = "";
        public string RunningApplication = "Moniverse";
        public string RunningEnvironment = "";

        public void Info(string message)
        {
            writer.WriteToLog(message);
        }

        public void Warning(string message)
        {
            writer.WriteToLog(message);
        }

        public void Error(string message)
        {
            writer.WriteToLog(message);
        }

        public void Exception(string message, string callStack)
        {
            writer.WriteToLog(message);
        }
    }
}
