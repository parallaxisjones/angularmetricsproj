using Moniverse.Contract;
using Playverse.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PlayverseMetrics.Models.AWS;
using Utilities;

namespace PlayverseMetrics.Models
{
    public class UsersModel
    {
        public static UsersModel Instance = new UsersModel();

        #region Counts

        public int GetRegisteredUsersCount()
        {
            int result = 0;

            string query =
                @"SELECT COUNT(*)
                FROM UserIdentity
                WHERE RegistrationTime > '2014-07-22 16:00:00';";

            try
            {
                result = DBManager.Instance.QueryForCount(Datastore.General, query);
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return result;
        }

        public int GetRegisteredExternalUsersCount()
        {
            int result = 0;

            string query =
                @"SELECT COUNT(*)
                FROM ExternalAccount
                WHERE UseridentityId IN (
	                SELECT Id
	                FROM UserIdentity
	                WHERE RegistrationTime > '2014-07-22 16:00:00'
                );";

            try
            {
                result = DBManager.Instance.QueryForCount(Datastore.General, query);
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return result;
        }

        public int GetGameSessionUserCount()
        {
            int result = 0;

            string query =
                @"SELECT COUNT(*)
                FROM GameSessionUser
                WHERE Status = 2;";

            try
            {
                result = DBManager.Instance.QueryForCount(Datastore.General, query);
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return result;
        }

        public int GetGameSessionUserCount(GameMonitoringConfig game)
        {
            int result = 0;

            string query = String.Format(
                @"SELECT COUNT(*)
                FROM GameSessionUser
                WHERE GameId = '{0}'
                AND Status = 2;",
                game.Id);

            try
            {
                result = DBManager.Instance.QueryForCount(Datastore.General, query);
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return result;
        }

        public int GetGameSessionUserCountByGameID(string gameID)
        {
            int result = 0;

            string query = String.Format(
                @"SELECT COUNT(*)
                FROM GameSessionUser
                WHERE GameId = '{0}'
                AND Status = 2;",
                gameID);

            try
            {
                result = DBManager.Instance.QueryForCount(Datastore.General, query);
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return result;
        }

        public int GetPartiesCount()
        {
            int result = 0;

            string query =
                @"SELECT COUNT(*) FROM Party;";

            try
            {
                result = DBManager.Instance.QueryForCount(Datastore.General, query);
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return result;
        }

        public int GetPartiesCount(GameMonitoringConfig game)
        {
            int result = 0;

            string query = String.Format(
                @"SELECT COUNT(*)
                FROM Party
                WHERE GameId = '{0}';",
                game.Id);

            try
            {
                result = DBManager.Instance.QueryForCount(Datastore.General, query);
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return result;
        }

        public int GetPartiesCountByGameID(string gameId)
        {
            int result = 0;

            string query = String.Format(
                @"SELECT COUNT(*)
                FROM Party
                WHERE GameId = '{0}';",
                gameId);

            try
            {
                result = DBManager.Instance.QueryForCount(Datastore.General, query);
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return result;
        }

        #endregion


        #region Custom Queries

        public List<ExternalAccountSummary> GetRegisteredExternalUserSummary()
        {
            List<ExternalAccountSummary> result = new List<ExternalAccountSummary>();

            string query =
                @"SELECT	CASE Type
			                WHEN 0 THEN 'None'
			                WHEN 1 THEN 'FaceBook'
			                WHEN 2 THEN 'Steam'
			                WHEN 3 THEN 'GameCenter'
			                ELSE 'PlayStore'
		                END AS 'Type'
		                ,COUNT(*) As 'Count'
                FROM ExternalAccount
                WHERE UseridentityId IN (
	                SELECT Id
	                FROM UserIdentity
	                WHERE RegistrationTime > '2014-07-22 16:00:00'
                )
                GROUP BY Type;";

            try
            {
                DataTable queryResult = DBManager.Instance.Query(Datastore.General, query);

                if (queryResult.HasRows())
                {
                    foreach (DataRow row in queryResult.Rows)
                    {
                        result.Add(new ExternalAccountSummary()
                        {
                            Type = row.Field<string>("Type"),
                            Count = (int)row.Field<long>("Count")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return result;
        }

        public List<UserRegistrationTracking> GetRegisteredUsersCountByHour()
        {
            List<UserRegistrationTracking> result = new List<UserRegistrationTracking>();

            string query =
                @"SELECT DATE_SUB(RegistrationTime, INTERVAL ((MINUTE(RegistrationTime) % 60) * 60) + SECOND(RegistrationTime) SECOND) AS RecordTimestamp
		                ,COUNT(*) AS Count
                FROM UserIdentity
                WHERE RegistrationTime > '2014-07-22 16:00:00'
                GROUP BY DATE_SUB(RegistrationTime, INTERVAL ((MINUTE(RegistrationTime) % 60) * 60) + SECOND(RegistrationTime) SECOND)
                ORDER BY DATE_SUB(RegistrationTime, INTERVAL ((MINUTE(RegistrationTime) % 60) * 60) + SECOND(RegistrationTime) SECOND);";

            try
            {
                DataTable queryResult = DBManager.Instance.Query(Datastore.General, query);

                if (queryResult.HasRows())
                {
                    foreach (DataRow row in queryResult.Rows)
                    {
                        result.Add(new UserRegistrationTracking()
                        {
                            RecordTimestamp = row.Field<DateTime>("RecordTimestamp"),
                            Count = row.Field<int>("Count")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return result;
        }

        public List<DailyActiveUserSummary> GetDailyActiveUserSummary()
        {
            List<DailyActiveUserSummary> result = new List<DailyActiveUserSummary>();

            string query =
                @"SELECT RecordTimestamp
		                ,Count
                FROM MonitoringStats
                WHERE Type = 'DailyActiveUsers'
                AND GameId = '0-00000000000000000000000000000000'
                ORDER BY RecordTimestamp;";

            try
            {
                DataTable queryResult = DBManager.Instance.Query(Datastore.Monitoring, query);

                if (queryResult.HasRows())
                {
                    foreach (DataRow row in queryResult.Rows)
                    {
                        result.Add(new DailyActiveUserSummary()
                        {
                            RecordTimestamp = row.Field<DateTime>("RecordTimestamp"),
                            Count = row.Field<int>("Count")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return result;
        }

        public List<DailyActiveUserSummary> GetDailyActiveUserSummary(GameMonitoringConfig game)
        {
            List<DailyActiveUserSummary> result = new List<DailyActiveUserSummary>();

            string query = String.Format(
                @"SELECT RecordTimestamp
		                ,Count
                FROM MonitoringStats
                WHERE Type = 'DailyActiveUsers'
                AND GameId = '{0}'
                ORDER BY RecordTimestamp;",
                game.Id);

            try
            {
                DataTable queryResult = DBManager.Instance.Query(Datastore.Monitoring, query);

                if (queryResult.HasRows())
                {
                    foreach (DataRow row in queryResult.Rows)
                    {
                        result.Add(new DailyActiveUserSummary()
                        {
                            RecordTimestamp = row.Field<DateTime>("RecordTimestamp"),
                            Count = row.Field<int>("Count")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return result;
        }
        
        public List<DailyActiveUserSummary> GetDailyActiveUserSummaryById(string gameId, TimeInterval interval, AWSRegion region, DateTime startdate, DateTime enddate)
        {
            List<DailyActiveUserSummary> result = new List<DailyActiveUserSummary>();

            string query = String.Format(
                @"SELECT RecordTimestamp
		                ,Count
                FROM MonitoringStats
                WHERE Type = 'DailyActiveUsers'
                AND GameId = '{0}'
                AND DATE(RecordTimestamp) BETWEEN DATE('{1}') and DATE('{2}')
                ORDER BY RecordTimestamp;",
                gameId,
                startdate.ToString("yyyy-MM-dd HH:mm:ss"),
                enddate.ToString("yyyy-MM-dd HH:mm:ss"));

            try
            {
                DataTable queryResult = DBManager.Instance.Query(Datastore.Monitoring, query);

                if (queryResult.HasRows())
                {
                    foreach (DataRow row in queryResult.Rows)
                    {
                        result.Add(new DailyActiveUserSummary()
                        {
                            RecordTimestamp = row.Field<DateTime>("RecordTimestamp"),
                            Count = row.Field<int>("Count")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return result;
        }

        public List<GameUserActivity> GetGameUserActivity(GameMonitoringConfig game, int days)
        {
            List<GameUserActivity> result = new List<GameUserActivity>();

            string query = String.Format(
                @"SELECT *
                FROM GameUserActivity
                WHERE GameId = '{0}'
                AND RecordTimestamp BETWEEN DATE_SUB(UTC_TIMESTAMP(), INTERVAL {1} DAY) AND UTC_TIMESTAMP()
                ORDER BY RecordTimestamp ASC;",
                game.Id,
                days);

            try
            {
                DataTable queryResult = DBManager.Instance.Query(Datastore.Monitoring, query);

                if (queryResult.HasRows())
                {
                    foreach (DataRow row in queryResult.Rows)
                    {
                        result.Add(new GameUserActivity()
                        {
                            GameId = row.Field<string>("GameId"),
                            RegionName = row.Field<string>("RegionName"),
                            RecordTimestamp = row.Field<DateTime>("RecordTimestamp"),
                            GameSessionUsers = row.Field<int>("GameSessionUsers"),
                            EventListeners = row.Field<int>("EventListeners"),
                            TitleScreenUsers = row.Field<int>("TitleScreenUsers"),
                            SessionTypeName_0 = row.Field<string>("SessionTypeName_0"),
                            SessionTypeUsers_0 = row.Field<int>("SessionTypeUsers_0"),
                            SessionTypeName_1 = row.Field<string>("SessionTypeName_1"),
                            SessionTypeUsers_1 = row.Field<int>("SessionTypeUsers_1"),
                            SessionTypeName_2 = row.Field<string>("SessionTypeName_2"),
                            SessionTypeUsers_2 = row.Field<int>("SessionTypeUsers_2"),
                            SessionTypeName_3 = row.Field<string>("SessionTypeName_3"),
                            SessionTypeUsers_3 = row.Field<int>("SessionTypeUsers_3"),
                            SessionTypeName_4 = row.Field<string>("SessionTypeName_4"),
                            SessionTypeUsers_4 = row.Field<int>("SessionTypeUsers_4"),
                            SessionTypeName_5 = row.Field<string>("SessionTypeName_5"),
                            SessionTypeUsers_5 = row.Field<int>("SessionTypeUsers_5"),
                            SessionTypeName_6 = row.Field<string>("SessionTypeName_6"),
                            SessionTypeUsers_6 = row.Field<int>("SessionTypeUsers_6"),
                            SessionTypeName_7 = row.Field<string>("SessionTypeName_7"),
                            SessionTypeUsers_7 = row.Field<int>("SessionTypeUsers_7"),
                            SessionTypeUsers_Other = row.Field<int>("SessionTypeUsers_Other")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return result;
        }

        public List<GameSessionUserStats> GetGameSessionUserStats(GameMonitoringConfig game, int days)
        {
            List<GameSessionUserStats> result = new List<GameSessionUserStats>();

            string query = String.Format(
                @"SELECT *
                FROM GameSessionUserStats
                WHERE GameId = '{0}'
                AND RecordTimestamp BETWEEN DATE_SUB(UTC_TIMESTAMP(), INTERVAL {1} DAY) AND UTC_TIMESTAMP()
                ORDER BY RecordTimestamp ASC;",
                game.Id,
                days);

            try
            {
                DataTable queryResult = DBManager.Instance.Query(Datastore.Monitoring, query);

                if (queryResult.HasRows())
                {
                    foreach (DataRow row in queryResult.Rows)
                    {
                        result.Add(new GameSessionUserStats()
                        {
                            GameId = row.Field<string>("GameId"),
                            SessionType = row.Field<string>("SessionType"),
                            MaxNumPlayers = row.Field<int>("MaxNumPlayers"),
                            AvgPlayers = row.Field<decimal>("AvgPlayers"),
                            Sessions = row.Field<int>("Sessions"),
                            PrivateAvgPlayers = row.Field<decimal>("PrivateAvgPlayers"),
                            PrivateSessions = row.Field<int>("PrivateSessions"),
                            TotalAvgPlayers = row.Field<decimal>("TotalAvgPlayers"),
                            TotalSessions = row.Field<int>("TotalSessions"),
                            RecordTimestamp = row.Field<DateTime>("RecordTimestamp")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return result;
        }

        public List<string> GetPrivateSessionCompareTypes(GameMonitoringConfig game)
        {
            List<string> result = new List<string>();

            string query = String.Format(
                @"SELECT SessionType
                FROM GameSessionUserStats
                WHERE GameId = '{0}'
                GROUP BY SessionType
                HAVING MAX(Sessions) > 1
                AND MAX(PrivateSessions) > 1;"
                , game.Id);

            try
            {
                DataTable dt = DBManager.Instance.Query(Datastore.Monitoring, query);
                if (dt.HasRows())
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string sessionType = row.Field<string>("SessionType");
                        if (sessionType != null && !String.IsNullOrEmpty(sessionType))
                        {
                            result.Add(sessionType);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return result;
        }

        #endregion

        #region jsonreturns

        public List<PVTimeSeries> GetDailyActiveUsersByGame(GameMonitoringConfig game, TimeInterval interval, AWSRegion region, DateTime startdate, DateTime enddate)
        {
            DateTime minDate = startdate.Date;
            int daysDifference = (enddate.Date - startdate.Date).Days + 1;

            List<PVTimeSeries> result = new List<PVTimeSeries>();


            string query = String.Format(
                @"SELECT RecordTimestamp
		                ,Count
                FROM MonitoringStats
                WHERE Type = 'DailyActiveUsers'
                AND GameId = '{0}'
                AND DATE(RecordTimestamp) BETWEEN DATE('{1}') and DATE('{2}')
                ORDER BY RecordTimestamp;",
                game.Id,
                startdate.ToString("yyyy-MM-dd HH:mm:ss"),
                enddate.ToString("yyyy-MM-dd HH:mm:ss"));

            try
            {
                DataTable queryResult = DBManager.Instance.Query(Datastore.Monitoring, query);

                if (queryResult.HasRows())
                {
                    PVTimeSeries series = result.FirstOrDefault(x => x.name == "DailyActiveUsers");
                    if (series == default(PVTimeSeries))
                    {
                        series = new PVTimeSeries();
                        series.name = "Users";
                        series.data = new List<int>();
                        series.pointStart = queryResult.Rows[0].Field<DateTime>("RecordTimestamp").ToUnixTimestamp() * 1000; //JS unix timestamp is in milliseconds
                        series.pointInterval = 1440 * 60 * 1000; //JS unix timestamp is in milliseconds
                        series.type = "column";

                        result.Add(series);

                        for (int i = 0; i < daysDifference; i++)
                        {
                            series.data.Add(0);
                        }
                    }
                    foreach (DataRow row in queryResult.Rows)
                    {
                        int index = (row.Field<DateTime>("RecordTimestamp").Date - minDate).Days;
                        series.data[index] = Convert.ToInt32(row["Count"].ToString());
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return result;
        }

        public List<PVTimeSeries> AverageSessionLength(string gameShort, TimeInterval interval, AWSRegion region, DateTime startDate, DateTime endDate)
        {
            GameMonitoringConfig game = Games.Instance.GetMonitoredGames().Where(x => x.ShortTitle == gameShort).FirstOrDefault();

            List<PVTimeSeries> timeSeriesData = new List<PVTimeSeries>();
            DataTable queryResults = new DataTable();

            startDate = startDate.RoundDown(interval);
            endDate = endDate.RoundDown(interval);

            string query = String.Format(
                @"SELECT   RecordTimestamp,
                        SessionTypeName_0, AverageSessionTypeLength_0,
                        SessionTypeName_1, AverageSessionTypeLength_1,
                        SessionTypeName_2, AverageSessionTypeLength_2,
                        SessionTypeName_3, AverageSessionTypeLength_3,
                        SessionTypeName_4, AverageSessionTypeLength_4,
                        SessionTypeName_5, AverageSessionTypeLength_5,
                        SessionTypeName_6, AverageSessionTypeLength_6,
                        SessionTypeName_7, AverageSessionTypeLength_7
	            FROM GameAverageSessionLength
	            WHERE GameId = '{1}'
	            GROUP BY RecordTimestamp,
			            SessionTypeName_0,
			            SessionTypeName_1,
			            SessionTypeName_2,
			            SessionTypeName_3,
			            SessionTypeName_4,
			            SessionTypeName_5,
			            SessionTypeName_6,
			            SessionTypeName_7
                ORDER BY RecordTimestamp ASC;",
                "GameAverageSessionLength",
                game.Id);

            try
            {
                queryResults = DBManager.Instance.Query(Datastore.Monitoring, query);
                timeSeriesData = Charts.Instance.ProcessedTimeSeries(queryResults, interval, startDate, endDate, "RecordTimestamp");

            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return timeSeriesData;

        }

        public List<PVTableRow> AverageSessionLengthChart(string gameShort, TimeInterval interval, AWSRegion region, DateTime startDate, DateTime endDate)
        {
            GameMonitoringConfig game = Games.Instance.GetMonitoredGames().Where(x => x.ShortTitle == gameShort).FirstOrDefault();
            List<PVTableRow> DataTableInfo = GetAverageSessionLengthTable(interval, region, startDate, endDate, game);

            return DataTableInfo;
        }

        public List<PVTableRow> GetAverageSessionLengthTable(TimeInterval interval, AWSRegion region, DateTime startDate, DateTime endDate, GameMonitoringConfig game)
        {
            #region Validation


            if (!interval.IsSupportedInterval(TimeInterval.Minute, TimeInterval.Year))
            {
                throw new Exception(String.Format("Chart data only supports an interval between {0} and {1}", TimeInterval.Day, TimeInterval.Year));
            }

            if (startDate == DateTime.MinValue || endDate == DateTime.MinValue || (startDate >= endDate))
            {
                throw new Exception("StartDate and EndDate cannot be null, and StartDate must come before EndDate");
            }

            if (String.IsNullOrEmpty(game.Id))
            {
                throw new Exception("GameID cannot be empty or null");
            }

            #endregion

            List<PVTableRow> dataTableData = new List<PVTableRow>();
            DataTable queryResults = new DataTable();

            startDate = startDate.RoundDown(interval);
            endDate = endDate.RoundDown(interval);

            string query = String.Format(
                @"SELECT   RecordTimestamp,
                        SessionTypeName_0, AverageSessionTypeLength_0,
                        SessionTypeName_1, AverageSessionTypeLength_1,
                        SessionTypeName_2, AverageSessionTypeLength_2,
                        SessionTypeName_3, AverageSessionTypeLength_3,
                        SessionTypeName_4, AverageSessionTypeLength_4,
                        SessionTypeName_5, AverageSessionTypeLength_5,
                        SessionTypeName_6, AverageSessionTypeLength_6,
                        SessionTypeName_7, AverageSessionTypeLength_7
	            FROM GameAverageSessionLength
	            WHERE GameId = '{1}'
	            GROUP BY RecordTimestamp,
			            SessionTypeName_0,
			            SessionTypeName_1,
			            SessionTypeName_2,
			            SessionTypeName_3,
			            SessionTypeName_4,
			            SessionTypeName_5,
			            SessionTypeName_6,
			            SessionTypeName_7
                ORDER BY RecordTimestamp DESC;",
                "GameAverageSessionLength",
                game.Id);

            try
            {
                queryResults = DBManager.Instance.Query(Datastore.Monitoring, query);
                dataTableData = Charts.Instance.ProcessedDataTable(queryResults, interval, startDate, endDate, "RecordTimestamp");
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return dataTableData;
        }

        public List<PVTimeSeries> GetCurrentOnline(TimeInterval interval, DateTime start, DateTime end, GameMonitoringConfig game)
        {
            #region Validation

            if (!interval.IsSupportedInterval(TimeInterval.Minute, TimeInterval.Year))
            {
                throw new Exception(String.Format("Chart data only supports an interval between {0} and {1}", TimeInterval.Day, TimeInterval.Year));
            }

            if (start == DateTime.MinValue || end == DateTime.MinValue || (start >= end))
            {
                throw new Exception("StartDate and EndDate cannot be null, and StartDate must come before EndDate");
            }

            start = start.RoundDown(interval).ToUniversalTime();
            end = end.RoundDown(interval).ToUniversalTime();

            #endregion

            string query = String.Format(
                @"SELECT RecordTimestamp,
		                Title,
		                sum(GameSessionUsers) as GameSessionUsers
                FROM (
	                SELECT	RecordTimestamp,
			                GMC.Title,
                            GUA.RegionName,
			                round(avg(GameSessionUsers)) as GameSessionUsers
	                FROM {0} GUA
	                INNER JOIN GameMonitoringConfig GMC
		                ON GUA.GameId = GMC.GameId
	                WHERE RecordTimestamp BETWEEN '{1}' AND '{2}'
                    AND GMC.GameId = '{3}'
	                GROUP BY RecordTimestamp, GMC.Title, GUA.RegionName
                ) USERS
                GROUP BY RecordTimestamp, Title
                ORDER BY RecordTimestamp ASC, Title ASC;",
                String.Format("GameUserActivity{0}", interval.ToDbTableString()),
                start.ToString("yyyy-MM-dd HH:mm:ss"),
                end.ToString("yyyy-MM-dd HH:mm:ss"),
                game.Id);
            List<PVTimeSeries> SeriesList = new List<PVTimeSeries>();
            try
            {
                // Get time series data
                DataTable queryResults = DBManager.Instance.Query(Datastore.Monitoring, query);
                foreach (DataRow row in queryResults.Rows)
                {
                    PVTimeSeries series = SeriesList.FirstOrDefault(x => x.name == row["Title"].ToString());
                    if (series == default(PVTimeSeries))
                    {
                        series = new PVTimeSeries();
                        series.name = row["Title"].ToString();
                        series.data = new List<int>();
                        series.pointStart = queryResults.Rows[0].Field<DateTime>("RecordTimestamp").ToUnixTimestamp() * 1000; //JS unix timestamp is in milliseconds
                        series.pointInterval = (int)interval * 60 * 1000; //JS unix timestamp is in milliseconds
                        series.type = "area";

                        int zerosCount = ((int)(end - start).TotalMinutes / (int)interval) + 1;
                        for (int i = 0; i < zerosCount; i++)
                        {
                            series.data.Add(0);
                        }

                        SeriesList.Add(series);
                    }

                    DateTime timeStamp = row.Field<DateTime>("RecordTimestamp");
                    int index = (int)(timeStamp - start).TotalMinutes / (int)interval;

                    series.data[index] = Convert.ToInt32(row["GameSessionUsers"].ToString());

                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return SeriesList;
        }

        public List<PVTimeSeries> GetConCurrencyByGame(GameMonitoringConfig game, TimeInterval interval, DateTime start, DateTime end)
        {
            #region Validation

            if (!interval.IsSupportedInterval(TimeInterval.Minute, TimeInterval.Year))
            {
                throw new Exception(String.Format("Chart data only supports an interval between {0} and {1}", TimeInterval.Day, TimeInterval.Year));
            }

            if (start == DateTime.MinValue || end == DateTime.MinValue || (start >= end))
            {
                throw new Exception("StartDate and EndDate cannot be null, and StartDate must come before EndDate");
            }


            #endregion

            string query = String.Format(
                @"SELECT RecordTimestamp,
		                Title,
		                sum(GameSessionUsers) as GameSessionUsers
                FROM (
	                SELECT	RecordTimestamp,
			                GMC.Title,
                            GUA.RegionName,
			                round(avg(GameSessionUsers)) as GameSessionUsers
	                FROM {0} GUA
	                INNER JOIN GameMonitoringConfig GMC
		                ON GUA.GameId = GMC.GameId
	                WHERE RecordTimestamp BETWEEN '{1}' AND '{2}'
	                GROUP BY RecordTimestamp, GMC.Title, GUA.RegionName
                ) USERS
                GROUP BY RecordTimestamp, Title
                ORDER BY RecordTimestamp ASC, Title ASC;",
                String.Format("GameUserActivity{0}", interval.ToDbTableString()),
                start.ToString("yyyy-MM-dd HH:mm:ss"),
                end.ToString("yyyy-MM-dd HH:mm:ss"));
            List<PVTimeSeries> SeriesList = new List<PVTimeSeries>();
            try
            {
                // Get time series data
                DataTable queryResults = DBManager.Instance.Query(Datastore.Monitoring, query);
                foreach (DataRow row in queryResults.Rows)
                {
                    PVTimeSeries series = SeriesList.FirstOrDefault(x => x.name == row["Title"].ToString());
                    if (series == default(PVTimeSeries))
                    {
                        series = new PVTimeSeries();
                        series.name = row["Title"].ToString();
                        series.data = new List<int>();
                        series.pointStart = queryResults.Rows[0].Field<DateTime>("RecordTimestamp").ToUnixTimestamp() * 1000; //JS unix timestamp is in milliseconds
                        series.pointInterval = (int)interval * 60 * 1000; //JS unix timestamp is in milliseconds
                        series.type = "area";

                        SeriesList.Add(series);
                    }
                    series.data.Add(Convert.ToInt32(row["GameSessionUsers"].ToString()));
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return SeriesList;
        }
        
        public List<PVTimeSeries> GetUsersByRegion(TimeInterval interval, AWSRegion region, DateTime startDate, DateTime endDate, GameMonitoringConfig game)
        {
            #region Validation

            //if (!interval.IsSupportedInterval(TimeInterval.Minute, TimeInterval.Year))
            //{
            //    throw new Exception(String.Format("Chart data only supports an interval between {0} and {1}", TimeInterval.Day, TimeInterval.Year));
            //}

            //if (startDate == DateTime.MinValue || endDate == DateTime.MinValue || (startDate >= endDate))
            //{
            //    throw new Exception("StartDate and EndDate cannot be null, and StartDate must come before EndDate");
            //}

            //if (String.IsNullOrEmpty(game.Id))
            //{
            //    throw new Exception("GameID cannot be empty or null");
            //}

            #endregion

            List<PVTimeSeries> timeSeriesData = new List<PVTimeSeries>();

            // Init dates
            startDate = startDate.RoundDown(interval).ToUniversalTime();
            endDate = endDate.RoundDown(interval).ToUniversalTime();

            string query = String.Format(
                @"SELECT RecordTimestamp,
		                RegionName,
                        GameSessionUsers
                FROM {0}
                WHERE GameId = '{1}'
                AND RecordTimestamp BETWEEN '{2}' AND '{3}'
                ORDER BY RecordTimestamp, RegionName ASC;",
                String.Format("GameUserActivity{0}", interval.ToDbTableString()),
                game.Id,
                startDate.ToString("yyyy-MM-dd HH:mm:ss"),
                endDate.ToString("yyyy-MM-dd HH:mm:ss"));

            try
            {
                // Get time series data
                DataTable queryResults = DBManager.Instance.Query(Datastore.Monitoring, query);
                foreach (DataRow row in queryResults.Rows)
                {
                    PVTimeSeries series = timeSeriesData.FirstOrDefault(x => x.name == row["RegionName"].ToString());
                    if (series == default(PVTimeSeries))
                    {
                        series = new PVTimeSeries
                        {
                            name = row["RegionName"].ToString(),
                            data = new List<int>(),
                            pointStart = queryResults.Rows[0].Field<DateTime>("RecordTimestamp").ToUnixTimestamp() * 1000, //JS unix timestamp is in milliseconds
                            pointInterval = (int)interval * 60 * 1000,
                            type = "area"
                        };

                        int zerosCount = ((int)(endDate - startDate).TotalMinutes / (int)interval) + 1;
                        for (int i = 0; i < zerosCount; i++)
                        {
                            series.data.Add(0);
                        }

                        timeSeriesData.Add(series);
                    }

                    DateTime timeStamp = row.Field<DateTime>("RecordTimestamp");
                    int index = (int)(timeStamp - startDate).TotalMinutes / (int)interval;

                    series.data[index] = Convert.ToInt32(row["GameSessionUsers"].ToString());
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return timeSeriesData;
        }

        public List<PVTimeSeries<double>> GetDollarCostAveragePerDAU(GameMonitoringConfig gameMonitoringConfig, AWSRegion awsRegion, DateTime start, DateTime end)
        {
            int daysDifference = (end.Date - start.Date).Days + 1;

            DateTime minDate = start.Date;

            var summary = (new AWSModel()).FetchNetflixIceData(start, end);
            var retention = this.GetDailyActiveUsersByGame(gameMonitoringConfig, TimeInterval.Day, AWSRegion.All, start, end);

            List<PVTimeSeries<double>> allSeries = new List<PVTimeSeries<double>>();

            var pvDAU = new PVTimeSeries<double>()
            {
                data = new List<double>(),
                name = "Daily Active Users",
                pointInterval = 24 * 60 * 60 * 1000,
                pointStart = minDate.ToUnixTimestamp() * 1000,
                type = "column"
            };
            for (int i = 0; i < daysDifference; i++)
            {
                int dailyActiveUsers = retention[0].data[i];

                pvDAU.data.Add(dailyActiveUsers);
            }
            allSeries.Add(pvDAU);

            var pvTimeSeries = new PVTimeSeries<double>()
            {
                data = new List<double>(),
                name = "Dollar Cost per DAU",
                pointInterval = 24 * 60 * 60 * 1000,
                pointStart = minDate.ToUnixTimestamp() * 1000,
                type = "line",
                yAxis = 1
            };
            for (int i = 0; i < daysDifference; i++)
            {
                double cost = summary.data.aggregated[i];
                int dailyActiveUsers = retention[0].data[i];

                double dollarCostPerDAU = dailyActiveUsers == 0 ? 0 : cost / dailyActiveUsers;

                pvTimeSeries.data.Add(dollarCostPerDAU);
            }
            allSeries.Add(pvTimeSeries);

            return allSeries;
        }
        #endregion
        public GeoJsonData GetLoginDenistyMap(TimeInterval interval, AWSRegion region, DateTime startDate, DateTime endDate, GameMonitoringConfig game)
        {
            GeoJsonData returnValue = new GeoJsonData()
            {
                type = "FeatureCollection",
                features = new List<GeoJsonFeature>()
            };
            string query = string.Format(@"select Country,
	           Region,
               City,
	           Latitude, 
	           Longitude, 
               DATE_SUB(LoginTimestamp, INTERVAL (IFNULL(HOUR(LoginTimestamp) % FLOOR(60 / 60), 0) * 60 * 60) + ((MINUTE(LoginTimestamp) % 60) * 60) + SECOND(LoginTimestamp) SECOND) AS Date, 
               COUNT(UserId) as c
                from UserSessionMeta 
                where DATE(LoginTimestamp) > '2015-07-12'
                AND LoginTimestamp BETWEEN '{0}' and '{1}'
                AND Latitude IS NOT NULL
                AND Longitude IS NOT NULL
                group by Date, Latitude, Longitude
                order by c desc, LoginTimestamp desc;", startDate.ToString("yyyy-MM-dd HH:mm:ss"), endDate.ToString("yyyy-MM-dd HH:mm:ss"));

            DataTable dt = DBManager.Instance.Query(Datastore.Monitoring, query);

            foreach (DataRow data in dt.Rows)
            {
                List<float> floatList = new List<float>(); 

                //cooridinates
                floatList.Add((data["Longitude"].ToString() == null || data["Longitude"].ToString() == "") ? 0 : float.Parse(data["Longitude"].ToString()));
                floatList.Add((data["Latitude"].ToString() == null || data["Latitude"].ToString() == "") ? 0 : float.Parse(data["Latitude"].ToString()));

                //description
                string desc = string.Format(@" {0}, {1}, {2} - {3}",
                    (data["City"].ToString() == null || data["City"].ToString() == "") ? "" : data["City"].ToString(),
                    (data["Region"].ToString() == null || data["Region"].ToString() == "") ? "" : data["Region"].ToString(),
                    (data["Country"].ToString() == null || data["Country"].ToString() == "") ? "" : data["Country"].ToString(),
                    (data["c"].ToString() == null || data["c"].ToString() == "") ? 0 : Convert.ToInt32(data["c"].ToString()));
                returnValue.features.Add(new GeoJsonFeature()
                {
                    type = "Feature",
                    geometry = new GeoJsonGeometry()
                    {
                        type = "Point",
                        coordinates = floatList.ToArray()
                    },
                    properties = new GeoJsonProperties()
                    {
                        count = (data["c"].ToString() == null || data["c"].ToString() == "") ? 0 : Convert.ToInt32(data["c"].ToString()),
                        timestamp = (data["Country"].ToString() == null || data["Country"].ToString() == "") ? 0 : DateTime.Parse(data["Date"].ToString()).ToUnixTimestamp() * 1000
                    }
                });
            }
            return returnValue;
        }
    }

}