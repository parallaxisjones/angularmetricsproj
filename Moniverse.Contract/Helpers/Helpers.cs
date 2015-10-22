using System;
using System.Collections.Generic;

namespace Moniverse.Contract
{
    public static class GlobalHelperMethods
    {
        public static List<AWSRegion> GetRegions()
        {
            List<AWSRegion> result = new List<AWSRegion>();
            foreach (AWSRegion region in (AWSRegion[])Enum.GetValues(typeof(AWSRegion)))
            {
                result.Add(region);
            }

            return result;
        }

        public static List<TimeInterval> GetTimeIntervals()
        {
            List<TimeInterval> result = new List<TimeInterval>();
            foreach (TimeInterval interval in (TimeInterval[])Enum.GetValues(typeof(TimeInterval)))
            {
                result.Add(interval);
            }

            return result;
        }

        public static List<TimeInterval> GetTimeIntervalsByRange(TimeInterval minInterval, TimeInterval maxInterval)
        {
            List<TimeInterval> result = new List<TimeInterval>();
            foreach (TimeInterval interval in (TimeInterval[])Enum.GetValues(typeof(TimeInterval)))
            {
                if (interval < minInterval || interval > maxInterval)
                {
                    continue;
                }
                result.Add(interval);
            }

            return result;
        }
    }
}
