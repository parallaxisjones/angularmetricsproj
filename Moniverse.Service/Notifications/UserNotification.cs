using Moniverse.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playverse.Utilities;
using System.Data;
using Playverse.Data;
using Utilities;
using System.Collections.Concurrent;
using Moniverse.Service.Notifications;

namespace Moniverse.Service
{
    public class UserNotification : BaseNotification
    {
        protected virtual MessageTopic Topic { get { return MessageTopic.Users; } }

        public static UserNotification Instance = new UserNotification();

        public static ConcurrentDictionary<string, List<INotifier>> RunningNotifications = new ConcurrentDictionary<string, List<INotifier>>();

        public static MoniverseNotification ActiveUserDeltaThreshold(GameMonitoringConfig game)
        {
            MoniverseNotification notification = new MoniverseNotification()
            {
                Id = "UserDelta",
                ShouldSend = false
            };

            string subject = "{0} Users Online Dropped {1}";
            string message = "[{0} Alert {1}] | Game: {2} - Number of Users Online Dropped by {3} over past {4} minutes which is over the threshold of {5}";
            Logger.Instance.Info(String.Format("Check User Delta for {0}", game.Id));

            // Get last three user count snapshots
            string query = String.Format(
                @"SELECT RecordTimestamp,
		                SUM(GameSessionUsers) AS GameSessionUsers
                FROM (
                SELECT	DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(3 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 3) * 60) + SECOND(RecordTimestamp) SECOND) AS RecordTimestamp,
		                ROUND(AVG(GameSessionUsers)) AS GameSessionUsers,
		                RegionName
                FROM GameUserActivity
                WHERE GameId = '{0}'
                AND DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(3 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 3) * 60) + SECOND(RecordTimestamp) SECOND) > UTC_TIMESTAMP() - INTERVAL 10 MINUTE
                GROUP BY DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(3 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 3) * 60) + SECOND(RecordTimestamp) SECOND),
		                RegionName
                ) USERS
                GROUP BY RecordTimestamp
                ORDER BY RecordTimestamp DESC
                LIMIT 0, 3;",
                game.Id);

            DataTable queryResults = DBManager.Instance.Query(Datastore.Monitoring, query);
            if (!queryResults.HasRows() || queryResults.Rows.Count != 3)
            {
                if (game.IsNotificationSettingEnabled(NotificationLevel.PVSupport))
                {

                    new Notifier(Games.EMPTYGAMEID, MessageTopic.Error, new MoniverseNotification()
                    {
                        Id = "DBERROR",
                        Message = "Not Enough User Rows returned in Online User Check",
                        Subject = "Online User Check Failed"
                    }).SendOnce();

                    Logger.Instance.Info(String.Format("Not Enough User Rows Returned", game.Id));
                }
            }

            int currentUsers = (int)queryResults.Rows[0].Field<decimal>("GameSessionUsers");
            int users_3MinsAgo = (int)queryResults.Rows[1].Field<decimal>("GameSessionUsers");
            int users_6MinsAgo = (int)queryResults.Rows[2].Field<decimal>("GameSessionUsers");
            string recordTimestamp = queryResults.Rows[0].Field<DateTime>("RecordTimestamp").ToString();

            // Check notification thresholds and send notification of delta warning or error is necessary
            float sixMinDropPercent = 1.0f - ((float)currentUsers / (float)users_6MinsAgo);
            float threeMinDropPercent = 1.0f - ((float)currentUsers / (float)users_3MinsAgo);

            string sixMinuteDropF = Math.Abs((decimal)(sixMinDropPercent * 100)).ToString("#.##\\%");


            //
            if (sixMinDropPercent > game.ActiveUserDeltaThresholdPct_6Min)
            {
                notification.ShouldSend = true;
                notification.Message = String.Format(message, NotificationLevel.Error.ToString(), recordTimestamp, game.Title, sixMinuteDropF, 6, game.ActiveUserDeltaThresholdPct_6Min);
                notification.Subject = String.Format(subject, game.ShortTitle, sixMinuteDropF);
            }
            else if (threeMinDropPercent > game.ActiveUserDeltaThresholdPct_6Min)
            {
                notification.ShouldSend = true;
                notification.Message = String.Format(message, NotificationLevel.Error.ToString(), recordTimestamp, game.Title, sixMinuteDropF, 6, game.ActiveUserDeltaThresholdPct_6Min);
                notification.Subject = String.Format(subject, game.ShortTitle, sixMinuteDropF);
            }
            else if (threeMinDropPercent > game.ActiveUserDeltaThresholdPct_3Min)
            {
                notification.ShouldSend = true;
                notification.Message = String.Format(message, NotificationLevel.Error.ToString(), recordTimestamp, game.Title, sixMinuteDropF, 6, game.ActiveUserDeltaThresholdPct_6Min);
                notification.Subject = String.Format(subject, game.ShortTitle, sixMinuteDropF);
            }

            return notification;
        }

        public static MoniverseNotification TestNotification(GameMonitoringConfig game)
        {
            MoniverseNotification notification = new MoniverseNotification()
            {
                Id = "TestNotification",
                Message = "TESTMESSAGE",
                Subject = "TEST TEST TEST"
            };

            return notification;
        }

        public static MoniverseNotification CheckOnlineUserCount(GameMonitoringConfig game, int counter, int milliseconds)
        {
            int count = DBManager.Instance.QueryForCount(Datastore.General, Users.Instance.GetGameSessionUserCountQueryStr(game.Id));
            string formatString = "";

            if (counter == 0)
            {
                formatString = "{1} > !ALERT! currently online: {0} notification interval set at {2}";
            }
            else
            {
                formatString = "{1} > !ALERT! currently online: {0} notification interval set at {2} ({3} Notifications pending)";
            }

            string alert = String.Format(formatString, count, game.ShortTitle, TimeSpan.FromMilliseconds(milliseconds).TotalMinutes, counter);

            return new MoniverseNotification()
            {
                Message = alert,
                Subject = alert,
                Id = "CheckOnlineUserCount",
                ShouldSend = true
            };
        }

    }


    public class UserNotificationException : Exception
    {
        public UserNotificationException()
        {
        }

        public UserNotificationException(string message)
            : base(message)
        {
        }

        public UserNotificationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
