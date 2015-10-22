using Playverse.Data;
using Playverse.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using Moniverse.Contract;
using Utilities;

namespace PlayverseMetrics.Models
{

    public class RetentionModel : BaseModel
    {
        public static RetentionModel Instance = new RetentionModel();
        public virtual string USER_SESSION_META_TABLE { get { return "UserSessionMeta"; } }
        #region Returners Retention



        public List<ReturnerRow> GetReturnerRetention(DateTime date)
        {

            List<ReturnerRow> dataview = new List<ReturnerRow>();
            string query = String.Format("SELECT * from Retention as view order by view.RecordTimestamp desc limit 14", date.ToString());

            try
            {
                DataTable result = DBManager.Instance.Query(Datastore.Monitoring, query);

                foreach (DataRow row in result.Rows)
                {
                    ReturnerRow rr = new ReturnerRow()
                    {
                        CURR = row.Field<Decimal>("CURR"),
                        NURR = row.Field<Decimal>("NURR"),
                        RURR = row.Field<Decimal>("RURR"),
                        Date = row.Field<DateTime>("RecordTimestamp").ToString()
                    };
                    dataview.Add(rr);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }



            return dataview;
        }

        public List<string> GetLoginsFromDateRange(DateTime startDate, DateTime endDate)
        {
            List<string> countUsers = new List<string>();

            if (startDate == DateTime.MinValue || endDate == DateTime.MinValue || (startDate >= endDate))
            {
                throw new Exception("StartDate and EndDate cannot be null, and StartDate must come before EndDate");
            }

            string query = String.Format(
                  @"SELECT UserId 
                    FROM {0}
                    WHERE LoginTimestamp 
                    BETWEEN '{0}' AND '{1}';",
                    startDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    endDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    USER_SESSION_META_TABLE);

            try
            {

                DataTable result = DBManager.Instance.Query(Datastore.Monitoring, query);
                foreach (DataRow row in result.Rows)
                {
                    countUsers.Add(row.Field<string>("UserID"));
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex.Message);
            }

            return countUsers;
        }

        public List<string> GetloginsToEOTFromDate(DateTime startDate)
        {

            //number shouldn't ever be negative;
            List<string> countUsers = new List<string>();

            string query = String.Format(
                  @"SELECT UserId 
                    FROM UserSessionMeta
                    WHERE LoginTimestamp < '{0}'", startDate.ToString("yyyy-MM-dd HH:mm:ss"));

            try
            {
                DataTable result = DBManager.Instance.Query(Datastore.Monitoring, query);
                foreach (DataRow row in result.Rows)
                {
                    countUsers.Add(row.Field<string>("UserID"));
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex.Message);
            }
            return countUsers;
        }
        #endregion

        #region Two Week Retention

        public List<RetentionRow> RetentionReport(string sortOrder = "desc")
        {
            List<RetentionRow> RetentionRows = new List<RetentionRow>();
            string query = string.Format("select * from Retention ORDER BY Date {0};", sortOrder);

            try
            {
                DataTable row = DBManager.Instance.Query(Datastore.Monitoring, query);
                if (row.Rows.Count > 0)
                {
                    foreach (DataRow c in row.Rows)
                    {
                        RetentionRow thisRow;
                        thisRow = new RetentionRow();
                        thisRow.date = c.Field<DateTime>("Date").ToString();
                        thisRow.installsOnThisDay = c.Field<int>("NewUsers");
                        thisRow.loginsOnThisDay = c.Field<int>("Logins");
                        thisRow.days[0] = c.Field<float>("Day1");
                        thisRow.days[1] = c.Field<float>("Day2");
                        thisRow.days[2] = c.Field<float>("Day3");
                        thisRow.days[3] = c.Field<float>("Day4");
                        thisRow.days[4] = c.Field<float>("Day5");
                        thisRow.days[5] = c.Field<float>("Day6");
                        thisRow.days[6] = c.Field<float>("Day7");
                        thisRow.days[7] = c.Field<float>("Day8");
                        thisRow.days[8] = c.Field<float>("Day9");
                        thisRow.days[9] = c.Field<float>("Day10");
                        thisRow.days[10] = c.Field<float>("Day11");
                        thisRow.days[11] = c.Field<float>("Day12");
                        thisRow.days[12] = c.Field<float>("Day13");
                        thisRow.days[13] = c.Field<float>("Day14");
                        RetentionRows.Add(thisRow);
                    }
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex.Message);
            }
            return RetentionRows;
        }

        public List<PVTimeSeries> AverageRetention()
        {
            List<RetentionRow> retentionTable = RetentionReport();
            List<PVTimeSeries> seriesList = new List<PVTimeSeries>();
            RetentionRow firstRow = null;
            for (int i = 0; i < 14; i++)
            {
                seriesList.Add(new PVTimeSeries()
                {
                    name = string.Format("day {0}", i + 1),
                    pointInterval = 1440 * 60 * 1000 * 7,
                    data = new List<int>()
                });
            }

            RetentionRow lastRow = retentionTable.Last();
            foreach (List<RetentionRow> rowBatch in retentionTable.Batch<RetentionRow>(7))
            {
                bool canProcess = true;
                float[] dayPercents = new float[14];

                foreach (RetentionRow row in rowBatch)
                {
                    for (int i = 0; i < row.days.Length; i++)
                    {
                        if (row.days[i] == -1)
                        {
                            canProcess = false;
                            break;

                        }
                        dayPercents[i] += row.days[i];
                    }
                    if (canProcess == false)
                    {
                        break;
                    }
                }
                if (canProcess == true)
                {
                    if (firstRow == null)
                    {
                        firstRow = rowBatch.FirstOrDefault();
                    }
                    for (int i = 0; i < dayPercents.Length; i++)
                    {
                        DateTime startDate = DateTime.Parse(lastRow.date);
                        seriesList[i].pointStart = startDate.ToUnixTimestamp() * 1000;
                        int percentAvg = Convert.ToInt32(Math.Floor((dayPercents[i] / 7)));
                        seriesList[i].data.Add(percentAvg);
                    }
                }


            }
            return seriesList;
        }

        public List<PVTimeSeries> InstallsLoginsTimeSeries()
        {
            List<RetentionRow> report = RetentionReport("asc");
            RetentionRow firstRow = report.FirstOrDefault();
            List<PVTimeSeries> seriesList = new List<PVTimeSeries>();
            seriesList.Add(new PVTimeSeries()
            {
                name = "Returning Logins",
                pointInterval = (int)TimeSeriesPointInterval.day,
                data = new List<int>(),
                pointStart = DateTime.Parse(firstRow.date).ToUnixTimestamp() * 1000,
                type = "column",
                yAxis = 0
            });
            seriesList.Add(new PVTimeSeries()
            {
                name = "New Installs",
                pointInterval = (int)TimeSeriesPointInterval.day,
                data = new List<int>(),
                pointStart = DateTime.Parse(firstRow.date).ToUnixTimestamp() * 1000,
                type = "column",
                yAxis = 0
            });
            foreach (List<RetentionRow> rowBatch in report.Batch<RetentionRow>(7))
            {
                foreach (RetentionRow row in rowBatch)
                {
                    seriesList[1].data.Add(row.installsOnThisDay);
                    seriesList[0].data.Add(row.loginsOnThisDay);
                }
            }

            return seriesList;
        }

        public List<Dictionary<string, object>> ReturnerDataTable(GameMonitoringConfig game, TimeInterval interval, AWSRegion region, DateTime startDate, DateTime endDate)
        {
            List<PVTimeSeries> SeriesList = new List<PVTimeSeries>();
            string query = String.Format(@"SELECT 
    `Retention`.`Date`,
    `Retention`.`WAU`,
    `Retention`.`NewUserCohort`,
    `Retention`.`ContinuingUsersCohort`,
    `Retention`.`ReactivatedUsersCohort`,
    `Retention`.`NUR`,
    `Retention`.`CUR`,
    `Retention`.`RUR`,
    `Retention`.`NURR`,
    `Retention`.`CURR`,
    `Retention`.`RURR`
FROM `Moniverse`.`Retention` order by Date desc;", startDate.ToString("yyyy-MM-dd HH:mm:ss"), endDate.ToString("yyyy-MM-dd HH:mm:ss"));

            DataTable dt = DBManager.Instance.Query(Datastore.Monitoring, query);

            return JSONFriendifyDataTable(dt);
        }

        public List<PVTimeSeries> ReturnerTimeSeries(GameMonitoringConfig game, TimeInterval interval, AWSRegion region, DateTime startDate, DateTime endDate)
        {
            List<PVTimeSeries> SeriesList = new List<PVTimeSeries>();
            string query = String.Format("SELECT Date, NURR, CURR, RURR from Retention as view WHERE view.Date BETWEEN '{0}' AND '{1}' order by view.Date desc", startDate.ToString("yyyy-MM-dd HH:mm:ss"), endDate.ToString("yyyy-MM-dd HH:mm:ss"));
            
            int daysDifference = 0;

            DataTable result = DBManager.Instance.Query(Datastore.Monitoring, query);

            if (result.HasRows())
            {
                DateTime minDate = startDate.Date;
                daysDifference = (endDate.Date - startDate.Date).Days;
                foreach (DataRow row in result.Rows)
                {
                    DateTime currentDay = DateTime.Parse(row["Date"].ToString());
                    int daysBetween = (currentDay - minDate).Days;

                    PVTimeSeries NewReturnsSeries = SeriesList.FirstOrDefault(x => x.name == "New Returns");
                    PVTimeSeries ContinuingReturnsSeries = SeriesList.FirstOrDefault(x => x.name == "Continuing Returns");
                    PVTimeSeries ReactivatedReturnsSeries = SeriesList.FirstOrDefault(x => x.name == "Reactivated Returns");
                    if (NewReturnsSeries == default(PVTimeSeries))
                    {
                        NewReturnsSeries = new PVTimeSeries();
                        NewReturnsSeries.name = "New Returns";
                        NewReturnsSeries.data = new List<int>();
                        NewReturnsSeries.pointStart = result.Rows[0].Field<DateTime>("Date").ToUnixTimestamp() * 1000; //JS unix timestamp is in milliseconds
                        NewReturnsSeries.pointInterval = Convert.ToInt32(TimeSeriesPointInterval.day);
                        NewReturnsSeries.type = "line";
                        for (int z = 0; z < daysDifference; z++)
                        {
                            NewReturnsSeries.data.Add(0);
                        }

                        SeriesList.Add(NewReturnsSeries);
                    }
                    if (ContinuingReturnsSeries == default(PVTimeSeries))
                    {
                        ContinuingReturnsSeries = new PVTimeSeries();
                        ContinuingReturnsSeries.name = "Continuing Returns";
                        ContinuingReturnsSeries.data = new List<int>();
                        ContinuingReturnsSeries.pointStart = result.Rows[0].Field<DateTime>("Date").ToUnixTimestamp() * 1000; //JS unix timestamp is in milliseconds
                        ContinuingReturnsSeries.pointInterval = Convert.ToInt32(TimeSeriesPointInterval.day);
                        ContinuingReturnsSeries.type = "line";
                        for (int z = 0; z < daysDifference; z++)
                        {
                            ContinuingReturnsSeries.data.Add(0);
                        }

                        SeriesList.Add(ContinuingReturnsSeries);
                    }
                    if (ReactivatedReturnsSeries == default(PVTimeSeries))
                    {
                        ReactivatedReturnsSeries = new PVTimeSeries();
                        ReactivatedReturnsSeries.name = "Reactivated Returns";
                        ReactivatedReturnsSeries.data = new List<int>();
                        ReactivatedReturnsSeries.pointStart = result.Rows[0].Field<DateTime>("Date").ToUnixTimestamp() * 1000; //JS unix timestamp is in milliseconds
                        ReactivatedReturnsSeries.pointInterval = Convert.ToInt32(TimeSeriesPointInterval.day);
                        ReactivatedReturnsSeries.type = "line";
                        for (int z = 0; z < daysDifference; z++)
                        {
                            ReactivatedReturnsSeries.data.Add(0);
                        }
                        SeriesList.Add(ReactivatedReturnsSeries);
                    }

                    int index = daysBetween;
                    
                    ReactivatedReturnsSeries.data[index] = (int)row.Field<decimal?>("RURR").GetValueOrDefault(-1);
                    ContinuingReturnsSeries.data[index] = (int)row.Field<decimal?>("CURR").GetValueOrDefault(-1);
                    NewReturnsSeries.data[index] = (int)row.Field<decimal?>("NURR").GetValueOrDefault(-1);
                }
            }
            return SeriesList;
        }

        #endregion


    }


}