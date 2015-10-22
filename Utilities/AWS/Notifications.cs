using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System;
using System.IO;
using Utilities;
using Moniverse.Contract;
using System.Threading.Tasks;
using System.Threading;
using Playverse.Data;

namespace Playverse.Utilities
{

    public class Notifier : INotifier
    {
        private MoniverseNotification message { get; set; }
        public string NotificationId { get; set; }
        public DateTime LastSent { get; set; }
        public bool isTick { get; set; }
        public string GameId { get; set; }
        public MessageTopic Topic { get; set; }
        public NotificationLevel Level { get; set; }

        public Notifier(string gameId, MessageTopic topic, MoniverseNotification notification, NotificationLevel level = NotificationLevel.Info)
        {
            GameId = gameId;
            message = notification;
            NotificationId = notification.Id;
            Topic = topic;
            Level = level;

            switch (topic)
            {
                case MessageTopic.Error:
                    message.ARN = NotificationTopics.ServiceError;
                    if (level == NotificationLevel.Error)
                    {
                        message.ARN = NotificationTopics.ServiceError;
                    }
                    break;
                case MessageTopic.Users:
                    message.ARN = NotificationTopics.ServiceError;
                    break;
                case MessageTopic.Game:
                    break;
                case MessageTopic.Hosting:
                    break;

            }
        }
        public void SendTick(Func<bool> stopCondition, int intervalMS)
        {
            Task.Factory.StartNew(() =>
            {
                Send();
                Func<bool> condition = stopCondition;
                while (condition())
                {
                    Thread.Sleep(intervalMS);
                    Send();

                }
            });
        }
        public void SendOnce()
        {
            Send();
        }

        protected void Send()
        {

            try
            {

                using (AmazonSimpleNotificationServiceClient snsClient = new AmazonSimpleNotificationServiceClient("AKIAJHGOA5MAPPB3JCTA", "/EqP+uv+q+T7b3TkfBA/Xn7f4finoQEZgwSKOZ1K", RegionEndpoint.USEast1))
                {

                    snsClient.Publish(new PublishRequest()
                    {
                        Subject = message.Subject,
                        Message = message.Message,
                        TopicArn = message.ARN
                    });
                    LastSent = DateTime.UtcNow;
                    string insert = String.Format("INSERT INTO Notification (Id, GameId, Subject, Message, Topic, Level, ARN, CreatedAt) VALUES ({0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}');", 0, GameId, message.Subject, message.Message, Topic.ToString(), Level.ToString(), message.ARN, LastSent.ToString("yyyy-MM-dd HH:mm:ss"));
                    DBManager.Instance.Insert(Datastore.Monitoring, insert);
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(String.Format("Notification Error <> {0}", ex.Message));
                Logger.Instance.Exception(String.Format("Notification Error <> {0}", ex.Message), ex.StackTrace);
            }
        }
        public bool ShouldSend()
        {
            return message.ShouldSend;
        }
        public void setNotification(MoniverseNotification notification)
        {
            notification.ARN = this.message.ARN;
            this.message = notification;
        }

    }
}
