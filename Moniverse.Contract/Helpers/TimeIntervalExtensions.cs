using System;

namespace Moniverse.Contract
{
    public static class TimeIntervalExtensions
    {
        public static string GetIntervalString(this TimeInterval interval)
        {
            string result = String.Empty;

            switch (interval)
            {
                case TimeInterval.Minute:
                case TimeInterval.ThreeMinutes:
                case TimeInterval.FiveMinutes:
                case TimeInterval.TenMinutes:
                case TimeInterval.FifteenMinutes:
                case TimeInterval.ThirtyMinutes:
                    result = String.Format("{0} Minute", (int)interval);
                    break;
                case TimeInterval.Hour:
                case TimeInterval.ThreeHours:
                case TimeInterval.SixHours:
                case TimeInterval.TwelveHours:
                    result = String.Format("{0} Hour", (int)interval / 60);
                    break;
                case TimeInterval.Day:
                    result = "Day";
                    break;
                case TimeInterval.Week:
                    result = "Week";
                    break;
                case TimeInterval.Month:
                    result = "Month";
                    break;
                case TimeInterval.QuarterYear:
                    result = "Quarter";
                    break;
                case TimeInterval.Biannual:
                    result = "Biannual";
                    break;
                case TimeInterval.Year:
                    result = "Year";
                    break;
            }

            return result;
        }

        public static string ToDbTableString(this TimeInterval interval)
        {

            string intervalString = "_hour";

            switch (interval)
            {
                case TimeInterval.FiveMinutes:
                    intervalString = "_5min";
                    break;
                case TimeInterval.FifteenMinutes:
                    intervalString = "_15min";
                    break;
                case TimeInterval.ThirtyMinutes:
                    intervalString = "_30min";
                    break;
                case TimeInterval.Hour:
                    intervalString = "_hour";
                    break;
                case TimeInterval.SixHours:
                    intervalString = "_6hour";
                    break;
                case TimeInterval.Day:
                    intervalString = "_24hour";
                    break;
                default:
                    break;

            }
            return intervalString;
        }

        public static bool IsSupportedInterval(this TimeInterval interval, TimeInterval minSupportedInterval, TimeInterval maxSupportedInterval)
        {
            return ((int)interval >= (int)minSupportedInterval) && ((int)interval <= (int)maxSupportedInterval);
        }

        public static string GetTimestampColumnQueryFormat(this TimeInterval interval, string columnName)
        {
            if (String.IsNullOrEmpty(columnName))
            {
                throw new Exception("ColumnName cannot be empty or null");
            }

            string result = String.Empty;

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
                    result = String.Format("DATE_SUB({0}, INTERVAL (IFNULL(HOUR({0}) % FLOOR({1} / 60), 0) * 60 * 60) + ((MINUTE({0}) % {1}) * 60) + SECOND({0}) SECOND)",
                        columnName, (int)interval);
                    break;
                case TimeInterval.Day:
                case TimeInterval.Week:
                    result = String.Format("DATE_SUB(DATE({0}), INTERVAL (DAYOFWEEK({0}) - 1) % FLOOR({1} / 1440) DAY)",
                        columnName, (int)interval);
                    break;
                case TimeInterval.Month:
                    result = String.Format("ADDDATE(LAST_DAY(SUBDATE({0}, INTERVAL 1 MONTH)), 1)",
                        columnName);
                    break;
                case TimeInterval.QuarterYear:
                    result = String.Format("MAKEDATE(YEAR({0}), 1) + INTERVAL (CEIL(MONTH({0}) / 3) - 1) QUARTER",
                        columnName);
                    break;
                case TimeInterval.Biannual:
                    result = String.Format("MAKEDATE(YEAR({0}), 1) + INTERVAL ((CEIL(MONTH({0}) / 6) * 2) - 2) QUARTER",
                        columnName);
                    break;
                case TimeInterval.Year:
                    result = String.Format("MAKEDATE(YEAR({0}), 1)",
                        columnName);
                    break;
            }

            return result;
        }

        public static string GetTimeIntervalString(this TimeInterval interval)
        {

            string intervalString = "_hour";

            switch (interval)
            {
                case TimeInterval.FiveMinutes:
                    intervalString = "_5min";
                    break;
                case TimeInterval.FifteenMinutes:
                    intervalString = "_15min";
                    break;
                case TimeInterval.ThirtyMinutes:
                    intervalString = "_30min";
                    break;
                case TimeInterval.Hour:
                    intervalString = "_hour";
                    break;
                case TimeInterval.SixHours:
                    intervalString = "_6hour";
                    break;
                case TimeInterval.Day:
                    intervalString = "_24hour";
                    break;
                default:
                    break;

            }
            return intervalString;
        }
    }
}