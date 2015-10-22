using Moniverse.Contract;
using Playverse.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moniverse.Service.Notifications
{
    public class BaseNotification
    {

        protected virtual MessageTopic Topic { get { return MessageTopic.All; } }

        protected virtual NotificationLevel Level { get { return NotificationLevel.PVSupport; } }

        protected virtual string NotificationTable { get { return "Notification"; } }

        public MoniverseNotification GetNotificationById(int NotificationId)
        {

            string query = String.Format(@"select * from {0} 
                                            WHERE Id = {1};",
                                       NotificationTable,
                                       NotificationId
                                       );

            return DBManager.Instance.Query<MoniverseNotification>(Datastore.Monitoring, query).ToList<MoniverseNotification>().FirstOrDefault();


        }

        public List<MoniverseNotification> GetNotificationSinceId(int NotificationId)
        {
            List<MoniverseNotification> notificationList = new List<MoniverseNotification>();

            string query = String.Format(@"select * from {0} 
                                            WHERE Id >= {1}
                                            ORDER BY CreatedAt DESC;",
                                       NotificationTable,
                                       NotificationId
                                       );

            notificationList = DBManager.Instance.Query<MoniverseNotification>(Datastore.Monitoring, query).ToList<MoniverseNotification>();

            return notificationList;

        }

        public List<MoniverseNotification> GetNotificationsForGame(string gameId, MessageTopic Topic, NotificationLevel Level, DateTime start, DateTime end)
        {
            List<MoniverseNotification> notificationList = new List<MoniverseNotification>();
            string topic;
            string level = "Info";

            switch (Topic) { 
                case MessageTopic.All:
                    topic = "Error";
                    break;
                default:
                    topic = Topic.ToString();
                    break;
            }
            

            string query = String.Format(@"select * from {5} 
                                            WHERE GameID = '{0}'
                                            -- AND Topic = '{1}' 
                                            -- AND Level = '{2}' 
                                            AND CreatedAt BETWEEN '{3}' AND '{4}'
                                            ORDER BY CreatedAt DESC;",
                                       gameId, 
                                       topic, 
                                       level, 
                                       start.ToString("yyyy-MM-dd HH:mm:ss"), 
                                       end.ToString("yyyy-MM-dd HH:mm:ss"),
                                       NotificationTable
                                       );

            notificationList = DBManager.Instance.Query<MoniverseNotification>(Datastore.Monitoring, query).ToList<MoniverseNotification>();


            return notificationList;

        }

        public List<MoniverseNotification> GetNotificationsForGame(string gameId, MessageTopic topic, NotificationLevel Level)
        {
            DateTime start = new DateTime(2015, 01, 01);
            return GetNotificationsForGame(gameId, topic, Level, start, DateTime.UtcNow);
        }
        public List<MoniverseNotification> GetNotificationsForGame(string gameId, MessageTopic topic)
        {
            DateTime start = new DateTime(2015,01,01);

            return GetNotificationsForGame(gameId, topic, Level, start, DateTime.UtcNow);
        }
        public List<MoniverseNotification> GetNotificationsForGame(string gameId, DateTime start, DateTime end)
        {

            return GetNotificationsForGame(gameId, Topic, Level, start, end);
        }
        public List<MoniverseNotification> GetNotificationsForGame(string gameId)
        {
            DateTime start = new DateTime(2015, 01, 01);

            return GetNotificationsForGame(gameId, Topic, Level, start, DateTime.UtcNow);
        }
    }
}
