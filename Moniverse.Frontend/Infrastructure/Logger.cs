using Playverse.Utilities;
using System;
using System.Diagnostics;

namespace PlayverseMetrics
{
    //public class Logger
    //{
    //    private EventLogger _eventLog = new EventLogger("PlaytricsSource", "PlaytricsLog");
    //    //private DBLogger _dbLogger = new DBLogger("Playtrics");

    //    public static Logger Instance = new Logger();

    //    public EventLog EventLog { get { return _eventLog.EventLog; } }

    //    public void Info(string message)
    //    {
    //        //_dbLogger.Log(message);
    //        _eventLog.Log(message);
    //    }

    //    public void Warning(string message)
    //    {
    //        //_dbLogger.Log(message, DBLogger.LogLevel.Warning);
    //        _eventLog.Log(String.Format("Warning: {0}", message), EventLogEntryType.Warning);
    //    }

    //    public void Error(string message)
    //    {
    //        //_dbLogger.Log(message, DBLogger.LogLevel.Error);
    //        _eventLog.Log(String.Format("Error: {0}", message), EventLogEntryType.Error);
    //    }

    //    public void Exception(string message, string callStack)
    //    {
    //        //_dbLogger.Log(message, DBLogger.LogLevel.Exception, callStack);
    //        _eventLog.Log(String.Format("Exception: {0} - StackTrace: {1}", message, callStack.Trim()), EventLogEntryType.Error);
    //    }
    //}
}
