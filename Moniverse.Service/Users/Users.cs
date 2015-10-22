using Moniverse.Contract;
using Moniverse.Service;
using Playverse.Data;
using Playverse.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Utilities;
namespace Moniverse.Service
{
    public class Users : ServiceClassBase
    {

        public Users() : base() { }
        public static Users Instance = new Users();

        #region Queries


        public Dictionary<string, float> checkLastRecordsForTable(string gameId, string Tablename, int records)
        {

            Dictionary<string, float> results = new Dictionary<string, float>();

            string query = string.Format(@"select * from playverseDB.{0} order by max(ID) limit {0};", Tablename, records);

            DataTable queryResults = DBManager.Instance.Query(Datastore.General, query);
            if (queryResults.HasRows())
            {
                return null;
            }

            foreach (DataRowCollection row in queryResults.Rows)
            {

            }

            return results;
        }

        private string CheckStatsRecordExistsQueryStr(string gameId, string type, string recordTimestamp)
        {
            if (String.IsNullOrEmpty(gameId))
            {
                //throw new Exception("GameId cannot be null or empty");
            }

            if (String.IsNullOrEmpty(type))
            {
                throw new Exception("Type cannot be null or empty");
            }

            if (String.IsNullOrEmpty(recordTimestamp))
            {
                throw new Exception("RecordTimestamp cannot be null or empty");
            }

            return String.Format(
                @"SELECT COUNT(*)
                FROM MonitoringStats
                WHERE GameId = '{0}'
                AND Type = '{1}'
                AND RecordTimestamp = '{2}';",
                gameId,
                type,
                recordTimestamp);
        }



        private string InsertStatsRecordQueryStr(string gameId, string type, int count, string recordTimestamp)
        {
            if (String.IsNullOrEmpty(gameId))
            {
                throw new Exception("GameId cannot be null or empty");
            }

            if (String.IsNullOrEmpty(type))
            {
                throw new Exception("Type cannot be null or empty");
            }

            if (String.IsNullOrEmpty(recordTimestamp))
            {
                throw new Exception("RecordTimestamp cannot be null or empty");
            }

            return String.Format(
                @"INSERT INTO MonitoringStats
                (GameId, Type, Count, RecordTimestamp)
                VALUES ('{0}', '{1}', {2}, '{3}');",
                gameId,
                type,
                count,
                recordTimestamp);
        }

        private string GetRecentPlayerCountQueryStr(string gameId, string startDate, string endDate)
        {
            if (String.IsNullOrEmpty(startDate))
            {
                throw new Exception("StartDate cannot be null or empty");
            }

            if (String.IsNullOrEmpty(endDate))
            {
                throw new Exception("EndDate cannot be null or empty");
            }

            if (String.IsNullOrEmpty(gameId))
            {
                return String.Format(
                    @"SELECT COUNT(DISTINCT UserId)
                    FROM RecentPlayer
                    WHERE PlayTime BETWEEN '{0}' AND '{1}';",
                    startDate,
                    endDate);
            }

            return String.Format(
                @"SELECT COUNT(DISTINCT UserId)
                FROM RecentPlayer
                WHERE GameId = '{0}'
                AND PlayTime BETWEEN '{1}' AND '{2}';",
                gameId,
                startDate,
                endDate);
        }

        private string GetSessionTypeUserCountByRegionQueryStr(string gameId, List<string> sessionTypes)
        {
            if (String.IsNullOrEmpty(gameId))
            {
                throw new Exception("GameId cannot be null or empty");
            }

            return String.Format(
                @"SELECT CASE HR.Name WHEN 'Default' THEN 'US East (Northern Virginia)' ELSE HR.Name END AS RegionName,
		                GST.FriendlyName AS SessionType,
		                COUNT(DISTINCT GSU.UserId) AS Count
                FROM GameSessionUser GSU
                INNER JOIN GameSession GSES
	                ON GSU.GameId = GSES.GameId
	                AND GSU.GameSessionId = GSES.Id
                INNER JOIN GameSessionType GST
	                ON  GSES.GameId = GST.GameId
	                AND GSES.SessionTypeId = GST.Id
                INNER JOIN GameServer GSER
	                ON GSES.Id = GSER.GameSessionId
                INNER JOIN HostingInstance HI
	                ON GSER.InstanceId = HI.Id
                INNER JOIN RegionHostingConfiguration RHC
	                ON HI.RegionConfigurationId = RHC.Id
                INNER JOIN HostingRegion HR
	                ON RHC.RegionId = HR.Id
                WHERE GSU.Status = 2
                AND GSU.GameId = '{0}'
                AND GST.FriendlyName IN ('{1}')
                GROUP BY HR.Name, GST.FriendlyName
                UNION
                SELECT	CASE HR.Name WHEN 'Default' THEN 'US East (Northern Virginia)' ELSE HR.Name END AS RegionName,
		                'Other' AS SessionType,
		                COUNT(DISTINCT GSU.UserId) AS Count
                FROM GameSessionUser GSU
                INNER JOIN GameSession GSES
	                ON GSU.GameId = GSES.GameId
	                AND GSU.GameSessionId = GSES.Id
                INNER JOIN GameSessionType GST
	                ON  GSES.GameId = GST.GameId
	                AND GSES.SessionTypeId = GST.Id
                INNER JOIN GameServer GSER
	                ON GSES.Id = GSER.GameSessionId
                INNER JOIN HostingInstance HI
	                ON GSER.InstanceId = HI.Id
                INNER JOIN RegionHostingConfiguration RHC
	                ON HI.RegionConfigurationId = RHC.Id
                INNER JOIN HostingRegion HR
	                ON RHC.RegionId = HR.Id
                WHERE GSU.Status = 2
                AND GSU.GameId = '{0}'
                AND GST.FriendlyName NOT IN ('{1}')
                GROUP BY HR.Name;",
                gameId,
                String.Join("','", sessionTypes));
        }

        public string GetGameSessionUserCountQueryStr(string gameId)
        {
            if (String.IsNullOrEmpty(gameId))
            {
                return "SELECT COUNT(DISTINCT UserId) FROM GameSessionUser;";
            }

            return String.Format(
                @"SELECT COUNT(DISTINCT UserId)
                FROM GameSessionUser
                WHERE GameId = '{0}'", gameId);
        }

        private string GetGameSessionUserCountByRegionQueryStr(string gameId)
        {
            if (String.IsNullOrEmpty(gameId))
            {
                return @"SELECT CASE HR.Name WHEN 'Default' THEN 'US East (Northern Virginia)' ELSE HR.Name END AS RegionName,
		                        COUNT(DISTINCT UserId) AS Count
                        FROM GameSessionUser GSU
                        INNER JOIN GameSession GSES
	                        ON GSU.GameSessionId = GSES.Id
                        INNER JOIN GameServer GSER
	                        ON GSES.Id = GSER.GameSessionId
                        INNER JOIN HostingInstance HI
	                        ON GSER.InstanceId = HI.Id
                        INNER JOIN RegionHostingConfiguration RHC
	                        ON HI.RegionConfigurationId = RHC.Id
                        INNER JOIN HostingRegion HR
	                        ON RHC.RegionId = HR.Id
                        WHERE GSU.Status = 2
                        GROUP BY HR.Name;";
            }

            return String.Format(
                @"SELECT CASE HR.Name WHEN 'Default' THEN 'US East (Northern Virginia)' ELSE HR.Name END AS RegionName,
		                COUNT(DISTINCT UserId) AS Count
                FROM GameSessionUser GSU
                INNER JOIN GameSession GSES
	                ON GSU.GameSessionId = GSES.Id
                INNER JOIN GameServer GSER
	                ON GSES.Id = GSER.GameSessionId
                INNER JOIN HostingInstance HI
	                ON GSER.InstanceId = HI.Id
                INNER JOIN RegionHostingConfiguration RHC
	                ON HI.RegionConfigurationId = RHC.Id
                INNER JOIN HostingRegion HR
	                ON RHC.RegionId = HR.Id
                WHERE GSU.GameId = '{0}'
                AND GSU.Status = 2
                GROUP BY HR.Name;", gameId);
        }

        private string GetGameSessionUserStatsQueryStr(string gameId, List<string> publicTypes, List<string> privateTypes)
        {
            if (String.IsNullOrEmpty(gameId))
            {
                throw new Exception("GameId cannot be null or empty");
            }

            if (publicTypes == null || publicTypes.Where(x => String.IsNullOrEmpty(x)).Any() || !publicTypes.Any())
            {
                throw new Exception("PublicTypes cannot be null, empty, or contain any invalid strings");
            }

            if (privateTypes == null || privateTypes.Where(x => String.IsNullOrEmpty(x)).Any() || !privateTypes.Any())
            {
                throw new Exception("PrivateTypes cannot be null, empty, or contain any invalid strings");
            }

            return String.Format(
                @"SELECT OPEN.GameId AS GameId
		                ,OPEN.FriendlyName AS SessionType
		                ,OPEN.MaxNumPlayers AS MaxNumPlayers
		                ,OPEN.AvgPlayers AS AvgPlayers
		                ,OPEN.SessionCount AS Sessions
		                ,PRIVATE.AvgPlayers AS PrivateAvgPlayers
		                ,PRIVATE.SessionCount AS PrivateSessions
		                ,IFNULL(((OPEN.AvgPlayers * OPEN.SessionCount) + (PRIVATE.AvgPlayers * PRIVATE.SessionCount))/(OPEN.SessionCount + PRIVATE.SessionCount), 0) AS TotalAvgPlayers
		                ,OPEN.SessionCount + PRIVATE.SessionCount AS TotalSessions
                FROM (
	                SELECT	S.GameId
			                ,S.FriendlyName
			                ,S.MaxNumPlayers
			                ,AVG(S.Players) AS AvgPlayers
			                ,COUNT(S.GameSessionId) AS SessionCount
	                FROM (
		                SELECT	GST.GameId
				                ,GST.FriendlyName
				                ,MAX(GST.MaxNumPlayers) AS MaxNumPlayers
				                ,GS.Id AS GameSessionId
				                ,COUNT(GSU.UserSessionId) AS Players
		                FROM GameSessionType GST
		                LEFT JOIN GameSession GS
			                ON GST.GameId = GS.GameId
			                AND GST.Id = GS.SessionTypeId
			                AND GS.IsPrivateSession = 0
		                LEFT JOIN GameSessionUser GSU
			                ON GS.GameId = GSU.GameId
			                AND GS.Id = GSU.GameSessionId
			                AND GSU.Status = 2
		                WHERE GST.GameId = '{0}'
		                AND GST.FriendlyName IN ('{1}')
		                GROUP BY GS.GameId
				                ,GS.Id
	                ) S
	                GROUP BY S.GameId
			                ,S.MaxNumPlayers
                ) OPEN
                INNER JOIN (
	                SELECT	S.GameId
			                ,S.MaxNumPlayers
			                ,AVG(S.Players) AS AvgPlayers
			                ,COUNT(S.GameSessionId) AS SessionCount
	                FROM (
		                SELECT	GST.GameId
				                ,MAX(GST.MaxNumPlayers) AS MaxNumPlayers
				                ,GS.Id AS GameSessionId
				                ,COUNT(GSU.UserSessionId) AS Players
		                FROM GameSessionType GST
		                LEFT JOIN GameSession GS
			                ON GST.GameId = GS.GameId
			                AND GST.Id = GS.SessionTypeId
			                AND GS.IsPrivateSession = 1
		                LEFT JOIN GameSessionUser GSU
			                ON GS.GameId = GSU.GameId
			                AND GS.Id = GSU.GameSessionId
			                AND GSU.Status = 2
		                WHERE GST.GameId = '{0}'
		                AND GST.FriendlyName IN ('{2}')
		                GROUP BY GS.GameId
				                ,GS.Id
	                ) S
	                GROUP BY S.GameId
			                ,S.MaxNumPlayers
                ) PRIVATE
	                ON OPEN.GameId = PRIVATE.GameId;",
                gameId,
                String.Join("','", publicTypes),
                String.Join("','", privateTypes));
        }

        private string GetEventListenersCountQueryStr(string gameId)
        {
            if (String.IsNullOrEmpty(gameId))
            {
                return "SELECT COUNT(*) FROM Events_EventListener WHERE PlatformName NOT IN ('PCServer', 'LinuxServer', 'Web');";
            }

            return String.Format(
                @"SELECT COUNT(*)
                FROM Events_EventListener
                WHERE PlatformName NOT IN ('PCServer', 'LinuxServer', 'Web')
                AND GameId = '{0}';",
                gameId);
        }
        public static string GetIntervalGameSessionUserStats(int modVal)
        {
            string IntervalTable;
            switch (modVal)
            {
                case 5:
                    IntervalTable = "GameSessionUserStats_5min";
                    break;
                case 15:
                    IntervalTable = "GameSessionUserStats_15min";
                    break;
                case 30:
                    IntervalTable = "GameSessionUserStats_30min";
                    break;
                case 60:
                    IntervalTable = "GameSessionUserStats_hour";
                    break;
                case 360:
                    IntervalTable = "GameSessionUserStats_6hour";
                    break;
                case 720:
                    IntervalTable = "GameSessionUserStats_12hour";
                    break;
                case 1440:
                    IntervalTable = "GameSessionUserStats_24hour";
                    break;
                default:
                    IntervalTable = "GameSessionUserStats";
                    break;
            }
            return IntervalTable;
        }
        #endregion


        #region Daily Active Users

        private const string TYPE_DAU = "DailyActiveUsers";
        private static DateTime dau_nextRecordTime = DateTime.UtcNow.Date;

        public void RecordDailyActiveUsers()
        {
            try
            {
                if (dau_nextRecordTime <= DateTime.UtcNow)
                {
                    // Check if global DAU record exists for yesterday
                    string query = CheckStatsRecordExistsQueryStr(Games.EMPTYGAMEID, TYPE_DAU, dau_nextRecordTime.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"));
                    int count = DBManager.Instance.QueryForCount(Datastore.Monitoring, query);

                    if (count == 0)
                    {
                        // Get count of global DAU from RecentPlayers
                        query = GetRecentPlayerCountQueryStr(null, dau_nextRecordTime.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"), dau_nextRecordTime.AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss"));
                        count = DBManager.Instance.QueryForCount(Datastore.General, query);

                        // Insert global DAU record into Monitoring datastore
                        query = InsertStatsRecordQueryStr(Games.EMPTYGAMEID, TYPE_DAU, count, dau_nextRecordTime.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"));
                        try
                        {
                            MoniverseResponse response = new MoniverseResponse()
                            {
                                Status = "unsent",
                                TimeStamp = DateTime.UtcNow
                            };

                            lock (MoniverseBase.ConsoleWriterLock)
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Logger.Instance.Info("--------------------------------------");
                                Logger.Instance.Info("Beginning DAU INsert Batch");
                                Logger.Instance.Info("--------------------------------------");
                                Logger.Instance.Info("");
                            }
                            Users.Service(Service =>
                            {
                                response = Service.Insert(new MoniverseRequest()
                                {
                                    TaskName = "Insert RecordDailyActiveUsers",
                                    Task = query,
                                    TimeStamp = DateTime.UtcNow
                                });
                            });

                            lock (MoniverseBase.ConsoleWriterLock)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Logger.Instance.Info("--------------------------------------");
                                Logger.Instance.Info("DAU Batch Success");
                                Logger.Instance.Info("--------------------------------------");
                                Logger.Instance.Info("");
                                Logger.Instance.Info(String.Format("success!! {0}", response.Status));
                                Logger.Instance.Info("");
                                Console.ResetColor();
                            }

                        }
                        catch (Exception e)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Logger.Instance.Info(e.Message);
                            Console.ResetColor();
                        }
                    }

                    // Increment by a day so the record is only collected once a day after midnight
                    dau_nextRecordTime = dau_nextRecordTime.AddDays(1);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }
        }

        private static ConcurrentDictionary<string, DateTime> dau_nextGameRecordTime = new ConcurrentDictionary<string, DateTime>();

        public void RecordDailyActiveUsersByGame(GameMonitoringConfig game)
        {
            try
            {
                if (game == null)
                {
                    throw new Exception("GameInfo cannot be null or empty");
                }

                if (!dau_nextGameRecordTime.ContainsKey(game.Id))
                {
                    dau_nextGameRecordTime.TryAdd(game.Id, DateTime.UtcNow.Date);
                }

                DateTime nextRecordTime = dau_nextGameRecordTime[game.Id];

                if (nextRecordTime <= DateTime.UtcNow)
                {
                    // Check if game DAU record exists for yesterday
                    string query = CheckStatsRecordExistsQueryStr(game.Id, TYPE_DAU, nextRecordTime.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"));
                    int count = DBManager.Instance.QueryForCount(Datastore.Monitoring, query);

                    if (count == 0)
                    {
                        // Get count of game DAU from RecentPlayers
                        query = GetRecentPlayerCountQueryStr(game.Id, nextRecordTime.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"), nextRecordTime.AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss"));
                        count = DBManager.Instance.QueryForCount(Datastore.General, query);

                        // Insert game DAU record into Monitoring datastore
                        query = InsertStatsRecordQueryStr(game.Id, TYPE_DAU, count, nextRecordTime.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"));
                        try
                        {
                            MoniverseResponse response = new MoniverseResponse()
                            {
                                Status = "unsent",
                                TimeStamp = DateTime.UtcNow
                            };

                            lock (MoniverseBase.ConsoleWriterLock)
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Logger.Instance.Info("--------------------------------------");
                                Logger.Instance.Info("Beginning Insert of DAU By Game");
                                Logger.Instance.Info("--------------------------------------");
                                Logger.Instance.Info("");
                            }
                            Users.Service(Service =>
                            {
                                response = Service.Insert(new MoniverseRequest()
                                {
                                    TaskName = game.ShortTitle + "DAU Insert",
                                    Task = query,
                                    TimeStamp = DateTime.UtcNow
                                });
                            });
                            //int result = DBManager.Instance.Insert(Datastore.Monitoring, query);

                            lock (MoniverseBase.ConsoleWriterLock)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Logger.Instance.Info("--------------------------------------");
                                Logger.Instance.Info("DAU By Game Success");
                                Logger.Instance.Info("--------------------------------------");
                                Logger.Instance.Info("");
                                Logger.Instance.Info(String.Format("success!! {0}", response.Status));
                                Console.ResetColor();
                            }
                        }
                        catch (Exception e)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Logger.Instance.Info(e.Message);
                            Console.ResetColor();
                        }
                    }

                    // Increment by a day so the record is only collected once a day after midnight
                    dau_nextGameRecordTime[game.Id] = nextRecordTime.AddDays(1);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(String.Format("Game: {0} - {1} | Message: {2}", game.Id, game.Title, ex.Message), ex.StackTrace);
            }
        }

        #endregion


        #region Online Users Delta

        public void CheckActiveUserDelta(GameMonitoringConfig game)
        {
            //MoniverseNotification notification = UserNotification.ActiveUserDeltaThreshold(game);
            MoniverseNotification notification = UserNotification.ActiveUserDeltaThreshold(game);

            List<INotifier> NotifierList;

            if (!UserNotification.RunningNotifications.TryGetValue(game.ShortTitle, out NotifierList))
            {
                NotifierList = new List<INotifier>();
                UserNotification.RunningNotifications.TryAdd(game.ShortTitle, NotifierList);
            }

            if (notification.ShouldSend)
            {
                if (UserNotification.RunningNotifications.TryGetValue(game.ShortTitle, out NotifierList))
                {
                    if (NotifierList.Exists(x => {
                        Notifier notifier = (Notifier)x;
                        return notifier.NotificationId == notification.Id;
                    }))
                    {
                        return;
                    }
                    else
                    {
                        INotifier OnlineUserCount = new Notifier(game.Id, MessageTopic.Users, notification);
                        NotifierList.Add(OnlineUserCount);
                        
                        int counter = 0;
                        int interval = 1000 * 60 * 5;
                        int notifyCount = 3;

                        OnlineUserCount.SendTick(() =>
                         {

                             MoniverseNotification ThresholdCheck = UserNotification.CheckOnlineUserCount(game, notifyCount - counter - 1, interval);
                             OnlineUserCount.setNotification(ThresholdCheck);

                             if (counter >= notifyCount) 
                                 ThresholdCheck.ShouldSend = false;

                             if (!ThresholdCheck.ShouldSend) {
                                 UserNotification.RunningNotifications.TryGetValue(game.ShortTitle, out NotifierList);
                                 NotifierList.Remove(OnlineUserCount);                             
                             }
                             counter++;
                             return ThresholdCheck.ShouldSend;

                         }, interval);
                    }

                }
            }
        }

        public void RecordActiveUsers(GameMonitoringConfig game)
        {
            try
            {
                if (game == null)
                {
                    throw new Exception("GameInfo cannot be null or empty");
                }

                // Init the session type specific collections
                string[] sessionTypes = game.ActiveUserSessionTypes.ToArray();
                string[] sessionTypeNames = new string[8];
                for (int i = 0; i < 8; i++)
                {
                    if (i < sessionTypes.Count())
                    {
                        sessionTypeNames[i] = sessionTypes[i];
                    }
                    else
                    {
                        sessionTypeNames[i] = String.Empty;
                    }
                }

                // Grab regional counts
                DataTable queryResults = DBManager.Instance.Query(Datastore.General, GetSessionTypeUserCountByRegionQueryStr(game.Id, sessionTypes.ToList()));
                if (queryResults.HasRows())
                {
                    List<RegionSessionTypeCounts> gameSessionUserResults = new List<RegionSessionTypeCounts>();
                    foreach (DataRow row in queryResults.Rows)
                    {
                        gameSessionUserResults.Add(new RegionSessionTypeCounts()
                        {
                            RegionName = row.Field<string>("RegionName"),
                            SessionType = row.Field<string>("SessionType"),
                            Count = (int)row.Field<long>("Count")
                        });
                    }

                    // Grab global playverse game counts
                    int eventListeners = DBManager.Instance.QueryForCount(Datastore.General, GetEventListenersCountQueryStr(game.Id));
                    int titleScreenUsers = eventListeners - gameSessionUserResults.Sum(x => x.Count);
                    int otherSessionTypeUsers = 0;
                    DateTime timestamp = DateTime.UtcNow;

                    // Grab the region counts and insert the region specific metric
                    foreach (IGrouping<string, RegionSessionTypeCounts> region in gameSessionUserResults.GroupBy(x => x.RegionName))
                    {
                        int gameSessionUsers = region.Sum(x => x.Count);
                        int[] sessionTypeUserCounts = new int[8];
                        for (int i = 0; i < 8; i++)
                        {
                            if (sessionTypeNames[i] != String.Empty)
                            {
                                RegionSessionTypeCounts regionSessionType = region.Where(x => x.SessionType == sessionTypeNames[i]).FirstOrDefault();
                                if (regionSessionType != null)
                                {
                                    sessionTypeUserCounts[i] = regionSessionType.Count;
                                }
                            }
                        }

                        // Grab the other session type region count
                        RegionSessionTypeCounts otherRegionSessionType = region.Where(x => x.SessionType == "Other").FirstOrDefault();
                        if (otherRegionSessionType != null)
                        {
                            otherSessionTypeUsers = otherRegionSessionType.Count;
                        }



                        // Store new game user activity record
                        string insertQuery = String.Format(
                            @"INSERT INTO GameUserActivity
                            (GameId, RegionName, RecordTimestamp, GameSessionUsers, EventListeners, TitleScreenUsers, SessionTypeName_0, SessionTypeUsers_0,
                            SessionTypeName_1, SessionTypeUsers_1, SessionTypeName_2, SessionTypeUsers_2, SessionTypeName_3, SessionTypeUsers_3,
                            SessionTypeName_4, SessionTypeUsers_4, SessionTypeName_5, SessionTypeUsers_5, SessionTypeName_6, SessionTypeUsers_6,
                            SessionTypeName_7, SessionTypeUsers_7, SessionTypeUsers_Other) 
                            VALUES('{0}','{1}','{2}',{3},{4},{5},'{6}',{7},'{8}',{9},'{10}',{11},'{12}',{13},'{14}',{15},'{16}',{17},'{18}',{19},'{20}',{21}, {22})",
                                    game.Id,
                                    region.Key,
                            //truncated seconds
                                    timestamp.ToString("yyyy-MM-dd HH:mm"),
                                    gameSessionUsers,
                                    eventListeners,
                                    titleScreenUsers,
                                    sessionTypeNames[0],
                                    sessionTypeUserCounts[0],
                                    sessionTypeNames[1],
                                    sessionTypeUserCounts[1],
                                    sessionTypeNames[2],
                                    sessionTypeUserCounts[2],
                                    sessionTypeNames[3],
                                    sessionTypeUserCounts[3],
                                    sessionTypeNames[4],
                                    sessionTypeUserCounts[4],
                                    sessionTypeNames[5],
                                    sessionTypeUserCounts[5],
                                    sessionTypeNames[6],
                                    sessionTypeUserCounts[6],
                                    sessionTypeNames[7],
                                    sessionTypeUserCounts[7],
                                    otherSessionTypeUsers);
                        try
                        {
                            MoniverseResponse response = new MoniverseResponse()
                            {
                                Status = "unsent",
                                TimeStamp = DateTime.UtcNow
                            };

                            lock (MoniverseBase.ConsoleWriterLock)
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Logger.Instance.Info("--------------------------------------");
                                Logger.Instance.Info("Beginning Insert of Active Users");
                                Logger.Instance.Info("--------------------------------------");
                                Logger.Instance.Info("");
                            }
                            Users.Service(Service =>
                            {
                                response = Service.Insert(new MoniverseRequest()
                                {
                                    TaskName = "Insert RecordActiveUsers",
                                    Task = insertQuery,
                                    TimeStamp = DateTime.UtcNow
                                });
                            });
                            //int result = DBManager.Instance.Insert(Datastore.Monitoring, insertQuery);

                            lock (MoniverseBase.ConsoleWriterLock)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Logger.Instance.Info("--------------------------------------");
                                Logger.Instance.Info("Active User Success");
                                Logger.Instance.Info("--------------------------------------");
                                Logger.Instance.Info("");
                                Logger.Instance.Info("Done inserting Retention Row Data.");
                                Logger.Instance.Info(String.Format("success!! {0}", response.Status));
                                Logger.Instance.Info("");
                                Console.ResetColor();
                            }

                        }
                        catch (Exception e)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Logger.Instance.Info(e.Message);
                            Console.ResetColor();
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(String.Format("Game: {0} - {1} | Message: {2}", game.Id, game.Title, ex.Message), ex.StackTrace);
            }
        }

        public double AverageValueIntArray(int[] valuesToAverage)
        {
            return valuesToAverage.Average();
        }

        public void RecordActiveUsersInterval(object modVal)
        {
            //#if DEBUG
            //            Debugger.Launch();
            //#endif
            try
            {
                string IntervalTable = "";
                // TODO: insert into correct table based on time interval
                //TODO: based on the modval create an interval range
                // select 

                DateTime endDate = DateTime.UtcNow;
                DateTime startDate = endDate.AddMinutes(-(int)modVal);

                switch ((int)modVal)
                {
                    case 5:
                        IntervalTable = "GameUserActivity_5min";
                        break;
                    case 15:
                        IntervalTable = "GameUserActivity_15min";
                        break;
                    case 30:
                        IntervalTable = "GameUserActivity_30min";
                        break;
                    case 60:
                        IntervalTable = "GameUserActivity_hour";
                        break;
                    case 360:
                        IntervalTable = "GameUserActivity_6hour";
                        break;
                    case 720:
                        IntervalTable = "GameUserActivity_12hour";
                        break;
                    case 1440:
                        IntervalTable = "GameUserActivity_24hour";
                        break;
                    default:
                        startDate = endDate;
                        IntervalTable = "GameUserActivity";
                        break;
                }

                string insertQuery = String.Format(
                    @"INSERT INTO {0}
                            (GameId, RegionName, RecordTimestamp, GameSessionUsers, EventListeners, TitleScreenUsers, SessionTypeName_0, SessionTypeUsers_0,
                            SessionTypeName_1, SessionTypeUsers_1, SessionTypeName_2, SessionTypeUsers_2, SessionTypeName_3, SessionTypeUsers_3,
                            SessionTypeName_4, SessionTypeUsers_4, SessionTypeName_5, SessionTypeUsers_5, SessionTypeName_6, SessionTypeUsers_6,
                            SessionTypeName_7, SessionTypeUsers_7, SessionTypeUsers_Other)  
                              SELECT GameId,
                              RegionName,
                              '{2}',
                                    ROUND(AVG(GameSessionUsers)),
                                    ROUND(AVG(EventListeners)),
                                    ROUND(AVG(TitleScreenUsers)),
                                    SessionTypeName_0,
                                    ROUND(AVG(SessionTypeUsers_0)),
                                    SessionTypeName_1,
                                    ROUND(AVG(SessionTypeUsers_1)),
                                    SessionTypeName_2,
                                    ROUND(AVG(SessionTypeUsers_2)),
                                    SessionTypeName_3,
                                    ROUND(AVG(SessionTypeUsers_3)),
                                    SessionTypeName_4,
                                    ROUND(AVG(SessionTypeUsers_4)),
                                    SessionTypeName_5,
                                    ROUND(AVG(SessionTypeUsers_5)),
                                    SessionTypeName_6,
                                    ROUND(AVG(SessionTypeUsers_6)),
                              SessionTypeName_7, 
                              ROUND(AVG(SessionTypeUsers_7)),
                              ROUND(AVG(SessionTypeUsers_Other))
                            FROM GameUserActivity
                            WHERE RecordTimestamp BETWEEN '{1}' AND '{2}'
                            GROUP BY GameId,
                                     RegionName,
                                    SessionTypeName_0,
                                    SessionTypeName_1,
                                    SessionTypeName_2,
                                    SessionTypeName_3,
                                    SessionTypeName_4,
                                    SessionTypeName_5,
                                    SessionTypeName_6,
                                    SessionTypeName_7;",
                            IntervalTable,
                            startDate.ToString("yyyy-MM-dd HH:mm"),
                            endDate.ToString("yyyy-MM-dd HH:mm"));
#if DEBUG
                //Debugger.Launch();
#endif
                try
                {
                    MoniverseResponse response = new MoniverseResponse()
                    {
                        Status = "unsent",
                        TimeStamp = DateTime.UtcNow
                    };

                    lock (MoniverseBase.ConsoleWriterLock)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info(String.Format("Beginning insert Game User Activity {0} minute", modVal));
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("");
                    }
                    Users.Service(Service =>
                    {
                        response = Service.Insert(new MoniverseRequest()
                        {
                            TaskName = "Insert RecordActiveUsersInterval",
                            Task = insertQuery,
                            TimeStamp = DateTime.UtcNow
                        });
                    });
                    //int result = DBManager.Instance.Insert(Datastore.Monitoring, insertQuery);

                    lock (MoniverseBase.ConsoleWriterLock)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info(String.Format("Game User Activity {0} minute success", modVal));
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("");
                        Logger.Instance.Info(String.Format("success!! {0}", response.Status));
                        Logger.Instance.Info("");
                        Console.ResetColor();
                    }

                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Logger.Instance.Info(e.Message);
                    Console.ResetColor();
                }

            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(String.Format("Message: {0}", ex.Message), ex.StackTrace);
            }
        }


        #endregion


        #region Game Session User Stats

        public void RecordGameSessionUserStats(GameMonitoringConfig game)
        {
            try
            {
                if (game == null)
                {
                    throw new Exception("GameInfo cannot be null or empty");
                }

                foreach (PrivacyCompareSessionTypes privacyCompare in game.PrivacyCompareSessionTypes)
                {
                    DataTable result = DBManager.Instance.Query(Datastore.General, GetGameSessionUserStatsQueryStr(game.Id, privacyCompare.PublicTypes, privacyCompare.PrivateTypes));

                    if (result.HasRows())
                    {
                        string query = String.Format(
                            @"INSERT INTO GameSessionUserStats
                            (GameId, SessionType, MaxNumPlayers, AvgPlayers, Sessions, PrivateAvgPlayers
                            ,PrivateSessions, TotalAvgPlayers, TotalSessions, RecordTimestamp)
                            VALUES ('{0}', '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, '{9}');",
                            result.Rows[0].Field<string>("GameId"),
                            result.Rows[0].Field<string>("SessionType"),
                            result.Rows[0].Field<int>("MaxNumPlayers"),
                            result.Rows[0].Field<decimal>("AvgPlayers"),
                            result.Rows[0].Field<long>("Sessions"),
                            result.Rows[0].Field<decimal>("PrivateAvgPlayers"),
                            result.Rows[0].Field<long>("PrivateSessions"),
                            result.Rows[0].Field<decimal>("TotalAvgPlayers"),
                            result.Rows[0].Field<long>("TotalSessions"),
                            DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                        try
                        {
                            MoniverseResponse response = new MoniverseResponse()
                            {
                                Status = "unsent",
                                TimeStamp = DateTime.UtcNow
                            };

                            lock (MoniverseBase.ConsoleWriterLock)
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Logger.Instance.Info("--------------------------------------");
                                Logger.Instance.Info("Beginning Insert of Game Session User Stats");
                                Logger.Instance.Info("--------------------------------------");
                                Logger.Instance.Info("");
                            }
                            Users.Service(Service =>
                            {
                                response = Service.Insert(new MoniverseRequest()
                                {
                                    TaskName = "Insert",
                                    Task = query,
                                    TimeStamp = DateTime.UtcNow
                                });
                            });
                            //int res = DBManager.Instance.Insert(Datastore.Monitoring, query);

                            lock (MoniverseBase.ConsoleWriterLock)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Logger.Instance.Info("--------------------------------------");
                                Logger.Instance.Info("Game Session User Stats success");
                                Logger.Instance.Info("--------------------------------------");
                                Logger.Instance.Info("");
                                Logger.Instance.Info(String.Format("success!! {0}", response.Status));
                                Console.ResetColor();
                            }

                        }
                        catch (Exception e)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Logger.Instance.Info(e.Message);
                            Console.ResetColor();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(String.Format("Game: {0} - {1} | Message: {2}", game.Id, game.Title, ex.Message), ex.StackTrace);
            }
        }
        public void RecordGameSessionUserStatsInterval(object modVal)
        {
            //#if DEBUG
            //            Debugger.Launch();
            //#endif
            List<GameMonitoringConfig> games = Games.Instance.GetMonitoredGames();
            try
            {
                string IntervalTable = "";
                DateTime endDate = DateTime.UtcNow;
                DateTime startDate = endDate.AddMinutes(-(int)modVal);
                switch ((int)modVal)
                {
                    case 5:
                        {
                            endDate = endDate.AddSeconds(-endDate.Second);
                            startDate = startDate.AddSeconds(-startDate.Second);
                            IntervalTable = "GameSessionUserStats_5min";
                        }
                        break;
                    case 15:
                        {
                            endDate = endDate.AddSeconds(-endDate.Second);
                            startDate = startDate.AddSeconds(-startDate.Second);
                            IntervalTable = "GameSessionUserStats_15min";
                        }
                        break;
                    case 30:
                        {
                            endDate = endDate.AddSeconds(-endDate.Second);
                            startDate = startDate.AddSeconds(-startDate.Second);
                            IntervalTable = "GameSessionUserStats_30min";
                        }
                        break;
                    case 60:
                        {
                            //endDate = endDate.AddMinutes(-endDate.Minute);
                            //startDate = startDate.AddMinutes(-startDate.Minute);
                            endDate = endDate.AddSeconds(-endDate.Second);
                            startDate = startDate.AddSeconds(-startDate.Second);
                            IntervalTable = "GameSessionUserStats_hour";
                        }
                        break;
                    case 360:
                        {
                            //endDate = endDate.AddMinutes(-endDate.Minute);
                            //startDate = startDate.AddMinutes(-startDate.Minute);
                            //endDate = endDate.AddSeconds(-endDate.Second);
                            //startDate = startDate.AddSeconds(-startDate.Second);
                            IntervalTable = "GameSessionUserStats_6hour";
                        }
                        break;
                    case 720:
                        {
                            //endDate = endDate.AddMinutes(-endDate.Minute);
                            //startDate = startDate.AddMinutes(-startDate.Minute);
                            //endDate = endDate.AddSeconds(-endDate.Second);
                            //startDate = startDate.AddSeconds(-startDate.Second);
                            IntervalTable = "GameSessionUserStats_12hour";
                        }
                        break;
                    case 1440:
                        {
                            endDate = endDate.Subtract(endDate.TimeOfDay);
                            startDate = startDate.Subtract(startDate.TimeOfDay);
                            IntervalTable = "GameSessionUserStats_24hour";
                        }
                        break;
                    default:
                        IntervalTable = "GameSessionUserStats";
                        startDate = endDate;
                        break;
                }

                List<string> queries = new List<string>();

                foreach (GameMonitoringConfig game in games)
                {
                    string query = String.Format(
                     @"INSERT INTO {0}
                            (GameId, SessionType, MaxNumPlayers, AvgPlayers, Sessions, PrivateAvgPlayers
                            ,PrivateSessions, TotalAvgPlayers, TotalSessions, RecordTimestamp)
                            SELECT
                            GameId, SessionType, ROUND(AVG(MaxNumPlayers)), ROUND(AVG(AvgPlayers),2), ROUND(AVG(Sessions)), ROUND(AVG(PrivateAvgPlayers),2)
                            ,ROUND(AVG(PrivateSessions)), ROUND(AVG(TotalAvgPlayers),2), ROUND(AVG(TotalSessions)), '{2}'
                            FROM GameSessionUserStats
                            WHERE RecordTimestamp BETWEEN '{1}' AND '{2}'
                            AND GameId = '{3}'
                            GROUP BY GameId,
                                     SessionType;",
                     IntervalTable,
                     startDate.ToString("yyyy-MM-dd HH:mm"),
                     endDate.ToString("yyyy-MM-dd HH:mm"),
                     game.Id);

                    queries.Add(query);
                    //Logger.Instance.Info(String.Format("query for {0}: \n {1}", game.Id, query));
                }


#if DEBUG
                //Debugger.Launch();
#endif
                //Logger.Instance.Info(query);
                try
                {
                    try
                    {
                        MoniverseResponse response = new MoniverseResponse()
                        {
                            Status = "unsent",
                            TimeStamp = DateTime.UtcNow
                        };

                        string shitty = string.Join("", queries);
                        lock (MoniverseBase.ConsoleWriterLock)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Logger.Instance.Info("--------------------------------------");
                            Logger.Instance.Info(String.Format("Beginning Insert of {0}", IntervalTable));
                            Logger.Instance.Info("--------------------------------------");
                            Logger.Instance.Info("");
                        }
                        Users.Service(Service =>
                        {
                            response = Service.Insert(new MoniverseRequest()
                            {
                                TaskName = "Insert RecordGameSessionUserStatsInterval",
                                Task = shitty,
                                TimeStamp = DateTime.UtcNow
                            });
                        });
                        //int result = DBManager.Instance.Insert(Datastore.Monitoring, shitty);

                        lock (MoniverseBase.ConsoleWriterLock)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Logger.Instance.Info("--------------------------------------");
                            Logger.Instance.Info(String.Format("{0} success", IntervalTable));
                            Logger.Instance.Info("--------------------------------------");
                            Logger.Instance.Info("");
                            Logger.Instance.Info(String.Format("success!! {0}", response.Status));
                            Console.ResetColor();
                        }

                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Logger.Instance.Info(e.Message);
                        Console.ResetColor();
                    }

                }
                catch (Exception e)
                {
                    //Moniverse.hasError = true;
                    Logger.Instance.Exception(e.Message, e.StackTrace);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(String.Format("Message: {0}", ex.Message), ex.StackTrace);
            }
        }
        #endregion
    }
}
