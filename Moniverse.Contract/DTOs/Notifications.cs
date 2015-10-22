using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Amazon.SimpleNotificationService;


namespace Moniverse.Contract
{

    public interface INotifier
    {
        void SendTick(Func<bool> KeepGoing, int interval);
        void setNotification(MoniverseNotification message);
        bool ShouldSend();
    }

    public enum NotificationLevel
    {
        Info = 0,
        Warning = 1,
        Error = 2,
        PVSupport = 3
    }

    public class NotificationSettings
    {
        public bool Info { get; set; }
        public bool Warning { get; set; }
        public bool Error { get; set; }
        public bool PVSupport { get; set; }
    }

    public class MoniverseNotification 
    {
        public string ARN { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public MessageTopic Topic { get; set; }
        public NotificationLevel Level { get; set; }
        public string Id { get; set; }
        public bool ShouldSend { get; set; }
    }


    public enum MessageTopic
    {
        Error,
        Economy,
        Users,
        Game,
        Hosting,
        All
    }


    public static class NotificationTopics
    {
        public static string ServiceError
        {
            get
            {
                return "arn:aws:sns:us-east-1:646520301408:playverse-errors";
            }
        }
        public static string TestTopic
        {
            get
            {
                return "arn:aws:sns:us-east-1:646520301408:notifications-test-topic";
            }
        }
        public static string UserDrop
        {
            get
            {
                return "arn:aws:sns:us-east-1:646520301408:playverse-errors";
            }
        }
        public static string EconomyPositive
        {
            get
            {
                return "arn:aws:sns:us-east-1:646520301408:playverse-errors";
            }
        }
        public static string EconomyNegative
        {
            get
            {
                return "arn:aws:sns:us-east-1:646520301408:playverse-errors";
            }
        }
    }


}
