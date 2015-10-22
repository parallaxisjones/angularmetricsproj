using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moniverse.Contract
{
    public enum TimeInterval
    {
        Minute = 1,
        ThreeMinutes = 3,
        FiveMinutes = 5,
        TenMinutes = 10,
        FifteenMinutes = 15,
        ThirtyMinutes = 30,
        Hour = 60,
        ThreeHours = 180,
        SixHours = 360,
        TwelveHours = 720,
        Day = 1440,
        Week = 10080,
        Month = 99999, // Not accurate value
        QuarterYear = 999999, // Not accurate value
        Biannual = 9999999, // Not accurate value
        Year = 99999999 // Not accurate value
    }

    public enum AWSRegion
    {
        All = 0,
        USEast_NorthVirg = 1,
        USWest_NorthCali = 2,
        USWest_Oregon = 3,
        EU_Ireland = 4,
        AsiaPac_Singapore = 5,
        AsiaPac_Sydney = 6,
        AsiaPac_Tokyo = 7,
        SouthAmer_SaoPaulo = 8
    }

    public enum RetentionCohortType
    {
        Unprocessed = -1,
        NewUser = 0,
        ContinuingUser = 1,
        ReactivatedUser = 2
    }

    public enum TimeSeriesPointInterval : uint
    {
        hour = 3600000,
        day = 86400000,
        week = 604800000,
        month = 2592000000
    }

}
