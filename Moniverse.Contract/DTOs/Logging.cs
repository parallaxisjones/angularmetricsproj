using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moniverse.Contract
{
    public class Log
    {
        public string Message { get; set; }
        public string LogTime { get; set; }
        public string LogDate { get; set; }

        public Log(string message)
        {
            Message = message;
            LogDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            LogTime = DateTime.UtcNow.ToString("hh:mm:ss.fff tt");
        }

    }
    public class Reasoning
    {
        public string OriginMethod { get; set; }
        public string Condition { get; set; }
        public string Actual { get; set; }
        public string Expected { get; set; }

        public string GetReasoningString() {
            return String.Format("{0} checked for {1}\r\nResult: {2}\r\nActual: {2}\r\nExpected:{3}", OriginMethod, Condition, Actual, Expected);
        }
    }

    public class NotificationLog : Log{

        public string Arn { get; set; }
        public string GameId { get; set; }
        public string Subject { get; set; }
        public string Reason { get; set; }

        public NotificationLog(Reasoning reason, string subject, string message) : base(message)
        {
            Subject = subject;
            Reason = reason.GetReasoningString();
        }

    }
}
