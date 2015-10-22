using System;

namespace Moniverse.Contract
{
    public static class DateTimeExtensions
    {
        public static string ToStartOfDay(this DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("yyyy-MM-dd 00:00:00");
        }

        public static string ToEndOfDay(this DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("yyyy-MM-dd 23:59:59");
        }

        public static long ToUnixTimestamp(this DateTime target)
        {
            var date = new DateTime(1970, 1, 1, 0, 0, 0, target.Kind);
            var unixTimestamp = System.Convert.ToInt64((target - date).TotalSeconds);

            return unixTimestamp;
        }

        public static DateTime ToDateTime(this long timestamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return dateTime.AddSeconds(timestamp);
        }

        public static DateTime RoundUp(this DateTime dt, TimeSpan timeInterval)
        {
            return new DateTime(((dt.Ticks + timeInterval.Ticks) / timeInterval.Ticks) * timeInterval.Ticks);
        }

        public static DateTime RoundDown(this DateTime dt, TimeInterval interval)
        {
            DateTime result = DateTime.MinValue;

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
                    result = new DateTime((dt.Ticks / timeInterval.Ticks) * timeInterval.Ticks);
                    break;
                case TimeInterval.Week:
                    int diff = dt.DayOfWeek - DayOfWeek.Sunday;
                    if (diff < 0)
                    {
                        diff += 7;
                    }
                    result = dt.AddDays(-1 * diff).Date;
                    break;
                case TimeInterval.Month:
                    result = new DateTime(dt.Year, dt.Month, 1);
                    break;
                case TimeInterval.QuarterYear:
                    int qtr = (dt.Month - 1) / 3 + 1;
                    result = new DateTime(dt.Year, (qtr - 1) * 3 + 1, 1);
                    break;
                case TimeInterval.Biannual:
                    int half = (dt.Month - 1) / 6 + 1;
                    result = new DateTime(dt.Year, (half - 1) * 6 + 1, 1);
                    break;
                case TimeInterval.Year:
                    result = new DateTime(dt.Year, 1, 1);
                    break;
            }

            return result;
        }
    }
}