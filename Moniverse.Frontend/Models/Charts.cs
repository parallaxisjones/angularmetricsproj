using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moniverse.Contract;

namespace PlayverseMetrics.Models
{
    public class Charts
    {
        public static Charts Instance = new Charts();

        #region Helper Methods
        public TimeSeriesDataNew GetTimeSeriesNewData(DataTable data, TimeInterval interval, DateTime startDate, DateTime endDate, string timestampColumnName)
        {
            // This method expects the dates to be rounded to their respected intervals

            #region Validation

            if (String.IsNullOrEmpty(timestampColumnName))
            {
                throw new Exception("Timestamp column name supplied cannot be null or empty");
            }

            if (!(data.Columns[0].ColumnName == timestampColumnName))
            {
                throw new Exception("Invalid data set. First column must be of type DateTime with same name as the passed in column name");
            }

            if (data.Columns.Count <= 1 || ((data.Columns.Count - 1) % 2) != 0)
            {
                throw new Exception("Invalid column sequence. Sequence of columns after first (record timestamp) should be String, Object, String, Object, etc.");
            }

            if (startDate == DateTime.MinValue || endDate == DateTime.MinValue || (startDate >= endDate))
            {
                throw new Exception("StartDate and EndDate cannot be null, and StartDate must come before EndDate");
            }

            #endregion Validation


            // Init return object
            TimeSeriesDataNew result = new TimeSeriesDataNew()
            {
                CategoryEntries = new List<string>(),
                SeriesData = new Dictionary<string, List<PlaytricsPoint>>(),
                StartDate = startDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                EndDate = endDate.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            // Capture the record timestamp category entries
            switch (interval)
            {
                case TimeInterval.Minute:
                case TimeInterval.ThreeMinutes:
                case TimeInterval.FiveMinutes:
                case TimeInterval.TenMinutes:
                case TimeInterval.FifteenMinutes:
                case TimeInterval.ThirtyMinutes:
                case TimeInterval.Hour:
                case TimeInterval.ThreeHours:
                case TimeInterval.SixHours:
                case TimeInterval.TwelveHours:
                case TimeInterval.Day:
                    TimeSpan timeInterval = TimeSpan.FromMinutes((int)interval);
                    for (long i = 0; startDate.AddTicks(i).Ticks <= endDate.Ticks; i += timeInterval.Ticks)
                    {
                        result.CategoryEntries.Add(startDate.AddTicks(i).ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    break;
                case TimeInterval.Week:
                    for (int i = 0; startDate.AddDays(i) <= endDate; i += 7)
                    {
                        result.CategoryEntries.Add(startDate.AddDays(i).ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    break;
                case TimeInterval.Month:
                    for (int i = 0; startDate.AddMonths(i) <= endDate; i++)
                    {
                        result.CategoryEntries.Add(startDate.AddMonths(i).ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    break;
                case TimeInterval.QuarterYear:
                    for (int i = 0; startDate.AddMonths(i) <= endDate; i += 3)
                    {
                        result.CategoryEntries.Add(startDate.AddMonths(i).ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    break;
                case TimeInterval.Biannual:
                    for (int i = 0; startDate.AddMonths(i) <= endDate; i += 6)
                    {
                        result.CategoryEntries.Add(startDate.AddMonths(i).ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    break;
                case TimeInterval.Year:
                    for (int i = 0; startDate.AddYears(i) <= endDate; i++)
                    {
                        result.CategoryEntries.Add(startDate.AddYears(i).ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    break;
            }

            // Capture the series data that matches the record timestamp
            if (data.HasRows())
            {
                for (int i = 0; i < result.CategoryEntries.Count; i++)
                {
                    foreach (DataRow row in data.Select(String.Format("{0} = '{1}'", timestampColumnName, result.CategoryEntries[i])))
                    {
                        // Iterate through the series keypairs
                        for (int x = 1; x < data.Columns.Count; x += 2)
                        {
                            //if (row[x] == null || String.IsNullOrEmpty(row[x].ToString()))
                            //{
                            //    break;
                            //}

                            string seriesName = row[x].ToString();
                            string seriesValue = row[x + 1].ToString();

                            if (!result.SeriesData.ContainsKey(seriesName))
                            {
                                // If the series was not in the data set, add it
                                List<PlaytricsPoint> PointList = new List<PlaytricsPoint>();
                                for (int y = 0; y < result.CategoryEntries.Count; y++)
                                {
                                    PlaytricsPoint point = new PlaytricsPoint()
                                    {
                                        RecordTimestamp = result.CategoryEntries[y],
                                        Count = 0
                                    };
                                    PointList.Add(point);
                                }
                                PointList[i].Count = seriesValue;
                                if (seriesName != "")
                                {
                                    result.SeriesData.Add(seriesName, PointList);
                                }
                                //if (!PointList.All(p => p.Count.Equals(0)))
                                //{
                                //    result.SeriesData.Add(seriesName, PointList);
                                //}
                            }
                            else
                            {
                                // Add to the existing series data set
                                result.SeriesData[seriesName][i].Count = seriesValue;
                            }
                        }
                    }
                }
            }

            return result;
        }

        public List<PVTimeSeries> ProcessedTimeSeries(DataTable data, TimeInterval interval, DateTime startDate, DateTime endDate, string timestampColumnName)
        {
            // This method expects the dates to be rounded to their respected intervals
            List<PVTimeSeries> response = new List<PVTimeSeries>();

            // Capture the series data that matches the record timestamp
            if (data.HasRows())
            {
                int daysDifference = (endDate.Date - startDate.Date).Days + 1;

                DateTime minDate = startDate.Date;

                foreach (DataRow row in data.Rows)
                {
                    DateTime currentDay = DateTime.Parse(row["RecordTimestamp"].ToString()).Date;

                    // Iterate through the series keypairs
                    for (int x = 1; x < data.Columns.Count; x += 2)
                    {
                        if (row[x].ToString() == "")
                        {
                            continue;
                        }
                        string seriesName = row[x].ToString();
                        string seriesValue = row[x + 1].ToString();
                        PVTimeSeries series = response.FirstOrDefault(pvts => pvts.name == seriesName);

                        int daysBetween = (currentDay - minDate).Days;
                        if (series == default(PVTimeSeries))
                        {
                            series = new PVTimeSeries()
                            {
                                name = seriesName,
                                data = new List<int>(),
                                pointStart = ToMillisecondsUnixTime(startDate),
                                pointInterval = 86400000
                            };

                            for (int z = 0; z < daysDifference; z++)
                            {
                                series.data.Add(0);
                            }

                            series.data[0] = Int32.Parse(seriesValue);
                            response.Add(series);
                        }
                        else if (series.name != "" && series.name == seriesName)
                        {
                            int value = Int32.Parse(seriesValue);
                            int index = daysBetween;
                            series.data[index] = value;
                        }

                    }
                }
            }

            return response;
        }

        public List<PVTableRow> ProcessedDataTable(DataTable data, TimeInterval interval, DateTime startDate, DateTime endDate, string timestampColumnName)
        {
            // This method expects the dates to be rounded to their respected intervals


            //init return list
            List<PVTableRow> response = new List<PVTableRow>();

            // Capture the series data that matches the record timestamp
            if (data.HasRows())
            {
                foreach (DataRow row in data.Rows)
                {
                    List<PlaytricsPair> rowdata = new List<PlaytricsPair>();
                    // Iterate through the series keypairs
                    for (int x = 1; x < data.Columns.Count; x += 2)
                    {
                        if (row[x].ToString() == "")
                        {
                            continue;
                        }
                        string seriesName = row[x].ToString();
                        string seriesValue = row[x + 1].ToString();
                        PlaytricsPair datapoint = new PlaytricsPair()
                        {
                            name = seriesName,
                            value = (String.Format("{0} seconds", Int32.Parse(seriesValue) / 1000))
                        };
                        rowdata.Add(datapoint);
                    }
                    PVTableRow tablerow = new PVTableRow()
                    {
                        data = rowdata,
                        index = row["RecordTimestamp"].ToString()
                    };
                    response.Add(tablerow);
                }
            }

            return response;
        }

        public List<PVTableRow> ProcessedSessionLengthData(DataTable data, TimeInterval interval, DateTime startDate, DateTime endDate, string timestampColumnName)
        {
            // This method expects the dates to be rounded to their respected intervals


            //init return list
            List<PVTableRow> response = new List<PVTableRow>()
            {
            };

            // Capture the series data that matches the record timestamp
            if (data.HasRows())
            {
                List<object> SeriesNames = data.AsEnumerable().Select(r => r["SeriesName"]).Distinct().ToList();
                foreach (DataRow row in data.Rows)
                {
                    PVTableRow tablerow = new PVTableRow()
                    {
                        data = new List<PlaytricsPair>(),
                        index = row["RecordTimestamp"].ToString()
                    };
                    foreach (string sessionName in SeriesNames)
                    {
                        PlaytricsPair sessionLength = new PlaytricsPair()
                        {
                            name = sessionName,
                            value = 0
                        };
                        tablerow.data.Add(sessionLength);
                    }

                    // Iterate through the series keypairs
                    response.Add(tablerow);
                }
            }

            return response;
        }
        
        public TimeSeriesData GetTimeSeriesData(DataTable data, TimeInterval interval, DateTime startDate, DateTime endDate, string timestampColumnName)
        {
            // This method expects the dates to be rounded to their respected intervals

            #region Validation

            if (String.IsNullOrEmpty(timestampColumnName))
            {
                throw new Exception("Timestamp column name supplied cannot be null or empty");
            }

            if (!(data.Columns[0].ColumnName == timestampColumnName))
            {
                throw new Exception("Invalid data set. First column must be of type DateTime with same name as the passed in column name");
            }

            if (data.Columns.Count <= 1 || ((data.Columns.Count - 1) % 2) != 0)
            {
                throw new Exception("Invalid column sequence. Sequence of columns after first (record timestamp) should be String, Object, String, Object, etc.");
            }

            if (startDate == DateTime.MinValue || endDate == DateTime.MinValue || (startDate >= endDate))
            {
                throw new Exception("StartDate and EndDate cannot be null, and StartDate must come before EndDate");
            }

            #endregion Validation


            // Init return object
            TimeSeriesData result = new TimeSeriesData()
            {
                CategoryEntries = new List<string>(),
                SeriesData = new Dictionary<string, List<object>>(),
                StartDate = startDate,
                EndDate = endDate
            };

            // Capture the record timestamp category entries
            switch (interval)
            {
                case TimeInterval.Minute:
                case TimeInterval.ThreeMinutes:
                case TimeInterval.FiveMinutes:
                case TimeInterval.TenMinutes:
                case TimeInterval.FifteenMinutes:
                case TimeInterval.ThirtyMinutes:
                case TimeInterval.Hour:
                case TimeInterval.ThreeHours:
                case TimeInterval.SixHours:
                case TimeInterval.TwelveHours:
                case TimeInterval.Day:
                    TimeSpan timeInterval = TimeSpan.FromMinutes((int)interval);
                    for (long i = 0; startDate.AddTicks(i).Ticks <= endDate.Ticks; i += timeInterval.Ticks)
                    {
                        result.CategoryEntries.Add(startDate.AddTicks(i).ToString());
                    }
                    break;
                case TimeInterval.Week:
                    for (int i = 0; startDate.AddDays(i) <= endDate; i += 7)
                    {
                        result.CategoryEntries.Add(startDate.AddDays(i).ToString());
                    }
                    break;
                case TimeInterval.Month:
                    for (int i = 0; startDate.AddMonths(i) <= endDate; i++)
                    {
                        result.CategoryEntries.Add(startDate.AddMonths(i).ToString());
                    }
                    break;
                case TimeInterval.QuarterYear:
                    for (int i = 0; startDate.AddMonths(i) <= endDate; i += 3)
                    {
                        result.CategoryEntries.Add(startDate.AddMonths(i).ToString());
                    }
                    break;
                case TimeInterval.Biannual:
                    for (int i = 0; startDate.AddMonths(i) <= endDate; i += 6)
                    {
                        result.CategoryEntries.Add(startDate.AddMonths(i).ToString());
                    }
                    break;
                case TimeInterval.Year:
                    for (int i = 0; startDate.AddYears(i) <= endDate; i++)
                    {
                        result.CategoryEntries.Add(startDate.AddYears(i).ToString());
                    }
                    break;
            }

            // Capture the series data that matches the record timestamp
            if (data.HasRows())
            {
                for (int i = 0; i < result.CategoryEntries.Count; i++)
                {
                    foreach (DataRow row in data.Select(String.Format("{0} = '{1}'", timestampColumnName, result.CategoryEntries[i])))
                    {
                        // Iterate through the series keypairs
                        for (int x = 1; x < data.Columns.Count; x += 2)
                        {
                            if (row[x] == null || String.IsNullOrEmpty(row[x].ToString()))
                            {
                                break;
                            }

                            string seriesName = row[x].ToString();
                            string seriesValue = row[x + 1].ToString();

                            if (!result.SeriesData.ContainsKey(seriesName))
                            {
                                // If the series was not in the data set, add it
                                List<object> objectList = new List<object>();
                                for (int y = 0; y < result.CategoryEntries.Count; y++)
                                {
                                    objectList.Add(0);
                                }
                                objectList[i] = seriesValue;
                                result.SeriesData.Add(seriesName, objectList);
                            }
                            else
                            {
                                // Add to the existing series data set
                                result.SeriesData[seriesName][i] = seriesValue;
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static double ToMillisecondsUnixTime(DateTime dt)
        {
            return (dt - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }
        #endregion
    }

}