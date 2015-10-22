//using System.Diagnostics;

//namespace Playverse.Utilities
//{
//    public class EventLogger
//    {
//        private EventLog _eventLog;

//        public EventLogger(string source, string log)
//        {
//            _eventLog = new EventLog();
//            if (!EventLog.SourceExists(source))
//            {
//                EventLog.CreateEventSource(source, log);
//            }
//            _eventLog.Source = source;
//            _eventLog.Log = log;
//        }

//        public EventLog EventLog { get { return _eventLog; } }

//        public void Log(string message)
//        {
//            EventLog.WriteEntry(message);
//        }

//        public void Log(string message, EventLogEntryType logType)
//        {
//            EventLog.WriteEntry(message, logType);
//        }
//    }
//}
