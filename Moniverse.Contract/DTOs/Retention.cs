using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moniverse.Contract
{
    public class RetentionRow
    {
        public string date = DateTime.UtcNow.ToString();
        public int installsOnThisDay = 0;
        public int loginsOnThisDay = 0;
        public float[] days = new float[14];

        public RetentionRow()
        {
            for (int i = 0; i < days.Length; i++)
            {
                days[i] = 0;
            }
        }
        public void SetDayPercent(int i, float percent)
        {
            days[i - 1] = percent;
        }
    }

    public class ReturningRetentionResponse
    {
        public List<ReturnerRow> Table;
        public TimeSeriesDataNew Chart;
    }

    public class ReturnerRow
    {
        public string Date = DateTime.UtcNow.ToString();
        public Decimal NURR { get; set; }
        public Decimal CURR { get; set; }
        public Decimal RURR { get; set; }
        public TimeSeriesDataNew chartFormat { get; set; }
    }

    public class TrackedUserOccurance
    {
        public DateTime Date { get; set; }
        public string UserId { get; set; }
        public RetentionCohortType CohortType { get; set; }
    }
    public class ReturnerRetentionDataPoints
    {
        public DateTime RecordDate { get; set; }
        public DateTime startRange { get; set; }
        public RetentionCohortType Type { get; set; }
        public int CountPreviousWeek { get; set; }
        public int ReturningContinuing { get; set; }
        public int Last7DaysTotalCount { get; set; }
        public int RelevantMetricProcessedCount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class SingleDayReturnerBuckets
    {
        public DateTime Date { get; set; }
        public int NewUsersReturns { get; set; }
        public int ReactReturns { get; set; }
        public int ContinuingReturns { get; set; }

        public int NewUsers { get; set; }
        public int Reacts { get; set; }
        public int Continuing { get; set; }
    }
}
