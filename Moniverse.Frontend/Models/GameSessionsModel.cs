using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moniverse.Contract;
using Playverse.Data;
using PlayverseMetrics.Models;
using Utilities;

namespace PlayverseMetrics
{
    public class GameSessionsModel
    {
        public static GameSessionsModel Instance = new GameSessionsModel();

        public List<PVTimeSeries> AverageSessionLength(string gameShort, TimeInterval interval, AWSRegion region, DateTime startDate, DateTime endDate)
        {

            //List<OnlineBySessionTypeSeries> retVal = new List<OnlineBySessionTypeSeries>();

            //need to format this data in the appropriate way as to feed it directly into high charts
            List<PVTimeSeries> TimeSeriesChart = GetAverageSessionLength(interval, region, startDate, endDate, gameShort);

            //this is the datetime format with TimeZone encoded in thatjavascript understands
            // the Z means UTC
            // and also JS Date has a method to JSON that turns the datetime into this string.
            // fuck yeah
            string IEFTFormatForJSON = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            return TimeSeriesChart;

        }

        public List<PVTableRow> AverageSessionLengthChart(string gameShort, TimeInterval interval, AWSRegion region, DateTime startDate, DateTime endDate)
        {
            GameMonitoringConfig game = Games.Instance.GetMonitoredGames().Where(x => x.ShortTitle == gameShort).FirstOrDefault();
            List<PVTableRow> DataTableInfo = GetAverageSessionLengthTable(interval, region, startDate, endDate, game);

            return DataTableInfo;
        }

        public List<PVTimeSeries> GetAverageSessionLength(TimeInterval interval, AWSRegion region, DateTime startDate, DateTime endDate, string gameShortName)
        {
            List<PVTimeSeries> timeSeriesData = new List<PVTimeSeries>();
            DataTable queryResults = new DataTable();

            GameMonitoringConfig game = Games.Instance.GetMonitoredGames().Where(x => x.ShortTitle == gameShortName).FirstOrDefault();
            string query = String.Format(
                        @"select DATE(RecordCreated) as RecordTimeStamp, 
                         SessionTypeFriendly as SeriesName, 
                         round(avg(minute(timediff(RecordLastUpdateTime, RecordCreated)))) * 60 * 1000 as AverageSessionLength 
                            from {0}
                            WHERE GameId = '{1}'
                            AND DATE(RecordCreated) BETWEEN '{2}' and '{3}'
                            AND minute(timediff(RecordLastUpdateTime, RecordCreated)) > 1 
                            group by DATE(RecordCreated), SessionTypeFriendly
                            order by RecordCreated asc;",
                        "GameSessionMeta", game.Id, startDate.ToString("yyyy-MM-dd HH:mm:ss"), endDate.ToString("yyyy-MM-dd HH:mm:ss"));

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
                        @"select DATE(RecordCreated) as RecordTimeStamp, 
                         SessionTypeFriendly as SeriesName, 
                         round(avg(minute(timediff(RecordLastUpdateTime, RecordCreated)))) as AverageSessionLength 
                            from {0}
                            WHERE GameId = '{1}'
                            AND DATE(RecordCreated) BETWEEN '{2}' and '{3}'
                            AND minute(timediff(RecordLastUpdateTime, RecordCreated)) > 1 
                            group by DATE(RecordCreated), SessionTypeFriendly
                            order by RecordCreated asc;",
                        "GameSessionMeta", game.Id, startDate.ToString("yyyy-MM-dd HH:mm:ss"), endDate.ToString("yyyy-MM-dd HH:mm:ss"));

            try
            {
                queryResults = DBManager.Instance.Query(Datastore.Monitoring, query);
                dataTableData = Charts.Instance.ProcessedSessionLengthData(queryResults, interval, startDate, endDate, "RecordTimestamp");

            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return dataTableData;
        }

        public List<PVTimeSeries> GetPrivateSessionTimeSeries(string gameShort, TimeInterval interval, DateTime startDate, DateTime endDate)
        {
            #region Validation

            GameMonitoringConfig game = Games.Instance.GetMonitoredGames().Where(x => x.ShortTitle == gameShort).FirstOrDefault();

            #endregion
            List<PVTimeSeries> timeSeriesData = new List<PVTimeSeries>();

            startDate = startDate.RoundDown(interval);
            endDate = endDate.RoundDown(interval);

            // Create a chart for each privacy comparison session type
            foreach (string sessionType in GetPrivateSessionCompareTypes(game))
            {
                string query = String.Format(
                    @"SELECT RecordTimestamp,
		                    'Private', PrivateSessions,
                            'Non-Private', Sessions
                    FROM {0}
                    WHERE GameId = '{1}'
                    AND SessionType = '{2}'
                    AND RecordTimestamp BETWEEN '{3}' AND '{4}'
                    GROUP BY RecordTimestamp
                    ORDER BY RecordTimestamp;",
                    String.Format("GameSessionUserStats{0}", interval.GetTimeIntervalString()),
                    game.Id,
                    sessionType,
                    startDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    endDate.ToString("yyyy-MM-dd HH:mm:ss"));

                try
                {
                    // Get time series data
                    DataTable queryResults = DBManager.Instance.Query(Datastore.Monitoring, query);
                    if (queryResults.HasRows())
                    {
                        foreach (DataRow row in queryResults.Rows)
                        {
                            PVTimeSeries series = timeSeriesData.FirstOrDefault(x => x.name == row["Private"].ToString());
                            if (series == default(PVTimeSeries))
                            {
                                series = new PVTimeSeries();
                                series.name = row["Private"].ToString();
                                series.data = new List<int>();
                                series.pointStart = queryResults.Rows[0].Field<DateTime>("RecordTimestamp").ToUnixTimestamp() * 1000; //JS unix timestamp is in milliseconds
                                series.pointInterval = (int)interval * 60 * 1000; //JS unix timestamp is in milliseconds
                                series.type = "area";

                                timeSeriesData.Add(series);
                            }

                            PVTimeSeries nonPrivateSeries = timeSeriesData.FirstOrDefault(x => x.name == row["Non-Private"].ToString());
                            if (nonPrivateSeries == default(PVTimeSeries))
                            {
                                nonPrivateSeries = new PVTimeSeries();
                                nonPrivateSeries.name = row["Non-Private"].ToString();
                                nonPrivateSeries.data = new List<int>();
                                nonPrivateSeries.pointStart = queryResults.Rows[0].Field<DateTime>("RecordTimestamp").ToUnixTimestamp() * 1000; //JS unix timestamp is in milliseconds
                                nonPrivateSeries.pointInterval = (int)interval * 60 * 1000; //JS unix timestamp is in milliseconds
                                nonPrivateSeries.type = "area";
                                timeSeriesData.Add(nonPrivateSeries);
                            }
                            series.data.Add(Convert.ToInt32(row["PrivateSessions"].ToString()));
                            nonPrivateSeries.data.Add(Convert.ToInt32(row["Sessions"].ToString()));
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logger.Instance.Exception(ex.Message, ex.StackTrace);
                }
            }

            return timeSeriesData;
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

        public List<PVTimeSeries> UsersOnlineBySessionType(string gameShort, TimeInterval interval, AWSRegion region, DateTime startDate, DateTime endDate)
        {
            GameMonitoringConfig game = Games.Instance.GetMonitoredGames().Where(x => x.ShortTitle == gameShort).FirstOrDefault();

            List<PVTimeSeries> timeSeriesData = new List<PVTimeSeries>();
            DataTable queryResults = new DataTable();

            startDate = startDate.RoundDown(interval);
            endDate = endDate.RoundDown(interval);

            string query = String.Format(
                 @"SELECT   RecordTimestamp,
                            SessionTypeName_0, SUM(SessionTypeUsers_0) AS SessionTypeUsers_0,
                            SessionTypeName_1, SUM(SessionTypeUsers_1) AS SessionTypeUsers_1,
                            SessionTypeName_2, SUM(SessionTypeUsers_2) AS SessionTypeUsers_2,
                            SessionTypeName_3, SUM(SessionTypeUsers_3) AS SessionTypeUsers_3,
                            SessionTypeName_4, SUM(SessionTypeUsers_4) AS SessionTypeUsers_4,
                            SessionTypeName_5, SUM(SessionTypeUsers_5) AS SessionTypeUsers_5,
                            SessionTypeName_6, SUM(SessionTypeUsers_6) AS SessionTypeUsers_6,
                            SessionTypeName_7, SUM(SessionTypeUsers_7) AS SessionTypeUsers_7,
                            Other, SUM(SessionTypeUsers_Other) AS SessionTypeUsers_Other
                FROM (
	                SELECT	RecordTimestamp,
			                RegionName,
			                SessionTypeName_0, ROUND(AVG(SessionTypeUsers_0)) AS SessionTypeUsers_0,
			                SessionTypeName_1, ROUND(AVG(SessionTypeUsers_1)) AS SessionTypeUsers_1,
			                SessionTypeName_2, ROUND(AVG(SessionTypeUsers_2)) AS SessionTypeUsers_2,
			                SessionTypeName_3, ROUND(AVG(SessionTypeUsers_3)) AS SessionTypeUsers_3,
			                SessionTypeName_4, ROUND(AVG(SessionTypeUsers_4)) AS SessionTypeUsers_4,
			                SessionTypeName_5, ROUND(AVG(SessionTypeUsers_5)) AS SessionTypeUsers_5,
			                SessionTypeName_6, ROUND(AVG(SessionTypeUsers_6)) AS SessionTypeUsers_6,
			                SessionTypeName_7, ROUND(AVG(SessionTypeUsers_7)) AS SessionTypeUsers_7,
			                'Other', ROUND(AVG(SessionTypeUsers_Other)) AS SessionTypeUsers_Other
	                FROM {0}
	                WHERE GameId = '{1}'
	                AND RecordTimestamp BETWEEN '{2}' AND '{3}'
                    AND RegionName like '{4}'
	                GROUP BY RecordTimestamp,
			                RegionName,
			                SessionTypeName_0,
			                SessionTypeName_1,
			                SessionTypeName_2,
			                SessionTypeName_3,
			                SessionTypeName_4,
			                SessionTypeName_5,
			                SessionTypeName_6,
			                SessionTypeName_7,
			                'Other'
                ) AGGSESSIONS
                GROUP BY RecordTimestamp,
		                SessionTypeName_0,
		                SessionTypeName_1,
		                SessionTypeName_2,
		                SessionTypeName_3,
		                SessionTypeName_4,
		                SessionTypeName_5,
		                SessionTypeName_6,
		                SessionTypeName_7,
		                Other
                ORDER BY RecordTimestamp ASC;",
                String.Format("GameUserActivity{0}", interval.ToDbTableString()),
                game.Id,
                startDate.ToString("yyyy-MM-dd HH:mm:ss"),
                endDate.ToString("yyyy-MM-dd HH:mm:ss"),
                region.GetDatabaseString());

            try
            {
                // Get time series data
                queryResults = DBManager.Instance.Query(Datastore.Monitoring, query);
                if (queryResults.HasRows())
                {                    
                    foreach (DataRow row in queryResults.Rows)
                    {
                        foreach (DataColumn col in queryResults.Columns)
                        {
                            if ((col.ColumnName.Contains("SessionTypeName") || col.ColumnName == "Other") && !String.IsNullOrEmpty(row[col.ColumnName].ToString()))
                            {
                                int count = Convert.ToInt32(row[col.Ordinal + 1].ToString());
                                
                                PVTimeSeries series = timeSeriesData.FirstOrDefault(x => x.name == row[col.ColumnName].ToString());

                                if (series == default(PVTimeSeries))
                                {
                                    series = new PVTimeSeries
                                    {
                                        name = row[col.ColumnName].ToString(),
                                        data = new List<int>(),
                                        pointStart = row.Field<DateTime>("RecordTimestamp").ToUnixTimestamp() * 1000, //JS unix timestamp is in milliseconds
                                        pointInterval = (int)interval * 60 * 1000, //JS unix timestamp is in milliseconds
                                        type = "area"
                                    };

                                    int zerosCount = ((int)(endDate - startDate).TotalMinutes / (int)interval) + 1;
                                    for (int i = 0; i < zerosCount; i++)
                                    {
                                        series.data.Add(0);
                                    }

                                    timeSeriesData.Add(series);
                                }
                                else
                                {
                                    DateTime timeStamp = row.Field<DateTime>("RecordTimestamp");
                                    int index = (int)(timeStamp - startDate).TotalMinutes / (int)interval;

                                    series.data[index] = count;
                                }
                                
                            }
                        }
                        
                    }
                }                

            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return timeSeriesData;
        }

        public TimeSeriesDataNew GetConcurrentUsersSessionType(TimeInterval interval, AWSRegion region, DateTime startDate, DateTime endDate, GameMonitoringConfig game)
        {
            #region Validation


            if (!interval.IsSupportedInterval(TimeInterval.Minute, TimeInterval.Year))
            {
                throw new Exception(String.Format("Chart data only supports an interval between {0} and {1}", TimeInterval.Day, TimeInterval.Year));
            }

            //if (region != 0) {
            //    throw new Exception("write check for valid region");
            //}


            if (startDate == DateTime.MinValue || endDate == DateTime.MinValue || (startDate >= endDate))
            {
                throw new Exception("StartDate and EndDate cannot be null, and StartDate must come before EndDate");
            }

            if (String.IsNullOrEmpty(game.Id))
            {
                throw new Exception("GameID cannot be empty or null");
            }

            #endregion
            TimeSeriesDataNew timeSeriesData = new TimeSeriesDataNew();
            DataTable queryResults = new DataTable();

            startDate = startDate.RoundDown(interval);
            endDate = endDate.RoundDown(interval);

            string query = String.Format(
                 @"SELECT   RecordTimestamp,
                            SessionTypeName_0, SUM(SessionTypeUsers_0) AS SessionTypeUsers_0,
                            SessionTypeName_1, SUM(SessionTypeUsers_1) AS SessionTypeUsers_1,
                            SessionTypeName_2, SUM(SessionTypeUsers_2) AS SessionTypeUsers_2,
                            SessionTypeName_3, SUM(SessionTypeUsers_3) AS SessionTypeUsers_3,
                            SessionTypeName_4, SUM(SessionTypeUsers_4) AS SessionTypeUsers_4,
                            SessionTypeName_5, SUM(SessionTypeUsers_5) AS SessionTypeUsers_5,
                            SessionTypeName_6, SUM(SessionTypeUsers_6) AS SessionTypeUsers_6,
                            SessionTypeName_7, SUM(SessionTypeUsers_7) AS SessionTypeUsers_7,
                            Other, SUM(SessionTypeUsers_Other) AS SessionTypeUsers_Other
                FROM (
	                SELECT	RecordTimestamp,
			                RegionName,
			                SessionTypeName_0, ROUND(AVG(SessionTypeUsers_0)) AS SessionTypeUsers_0,
			                SessionTypeName_1, ROUND(AVG(SessionTypeUsers_1)) AS SessionTypeUsers_1,
			                SessionTypeName_2, ROUND(AVG(SessionTypeUsers_2)) AS SessionTypeUsers_2,
			                SessionTypeName_3, ROUND(AVG(SessionTypeUsers_3)) AS SessionTypeUsers_3,
			                SessionTypeName_4, ROUND(AVG(SessionTypeUsers_4)) AS SessionTypeUsers_4,
			                SessionTypeName_5, ROUND(AVG(SessionTypeUsers_5)) AS SessionTypeUsers_5,
			                SessionTypeName_6, ROUND(AVG(SessionTypeUsers_6)) AS SessionTypeUsers_6,
			                SessionTypeName_7, ROUND(AVG(SessionTypeUsers_7)) AS SessionTypeUsers_7,
			                'Other', ROUND(AVG(SessionTypeUsers_Other)) AS SessionTypeUsers_Other
	                FROM {0}
	                WHERE GameId = '{1}'
	                AND RecordTimestamp BETWEEN '{2}' AND '{3}'
                    AND RegionName like '{4}'
	                GROUP BY RecordTimestamp,
			                RegionName,
			                SessionTypeName_0,
			                SessionTypeName_1,
			                SessionTypeName_2,
			                SessionTypeName_3,
			                SessionTypeName_4,
			                SessionTypeName_5,
			                SessionTypeName_6,
			                SessionTypeName_7,
			                'Other'
                ) AGGSESSIONS
                GROUP BY RecordTimestamp,
		                SessionTypeName_0,
		                SessionTypeName_1,
		                SessionTypeName_2,
		                SessionTypeName_3,
		                SessionTypeName_4,
		                SessionTypeName_5,
		                SessionTypeName_6,
		                SessionTypeName_7,
		                Other
                ORDER BY RecordTimestamp ASC;",
                String.Format("GameUserActivity{0}", interval.ToDbTableString()),
                game.Id,
                startDate.ToString("yyyy-MM-dd HH:mm:ss"),
                endDate.ToString("yyyy-MM-dd HH:mm:ss"),
                region.GetDatabaseString());

            try
            {
                // Get time series data
                queryResults = DBManager.Instance.Query(Datastore.Monitoring, query);
                timeSeriesData = Charts.Instance.GetTimeSeriesNewData(queryResults, interval, startDate, endDate, "RecordTimestamp");

            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return timeSeriesData;
        }
    }
}