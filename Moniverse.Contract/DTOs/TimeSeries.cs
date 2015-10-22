using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moniverse.Contract
{
    public class PlaytricsPoint
    {
        public string RecordTimestamp { get; set; }
        public object Count { get; set; }
    }
    public class PlaytricsPair
    {
        public string name { get; set; }
        public object value { get; set; }
    }

    public class TimeSeriesData
    {
        public List<string> CategoryEntries { get; set; }
        public Dictionary<string, List<object>> SeriesData { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class TimeSeriesDataNew
    {
        public List<string> CategoryEntries { get; set; }
        public Dictionary<string, List<PlaytricsPoint>> SeriesData { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    public class PVTimeSeries
    {
        public string name { get; set; }
        public List<int> data { get; set; }
        public int pointInterval { get; set; }
        public double pointStart { get; set; }
        public string type { get; set; }
        public int yAxis { get; set; }
    }

    public class PVTimeSeries<T> where T : struct
    {
        public string name { get; set; }
        public List<T> data { get; set; }
        public int pointInterval { get; set; }
        public double pointStart { get; set; }
        public string type { get; set; }
        public int yAxis { get; set; }
    }

    public class PVTableRow
    {
        public string index { get; set; } //this is the thing that the table is indexed by, in most cases it will be a date or a datetime
        public List<PlaytricsPair> data { get; set; } // this could be some generic tuple or something but whatever
    }

    public class PVPieChartCategories : PlaytricsPair
    {
        public Dictionary<string, string> metadata { get; set; }
        public string drilldown { get; set; }

        public PVPieChartCategories()
        {
            metadata = new Dictionary<string, string>();
        }
    }

    public class PVPieChartDrillDown
    {
        public string name { get; set; }
        public string id { get; set; }
        public List<PlaytricsPair> data { get; set; }
    }

    public class PVPPieChart
    {
        public string title { get; set; }
        public string subtitle { get; set; }
        public List<PVPieChartCategories> categoryData { get; set; }
        public List<PVPieChartDrillDown> drilldown { get; set; }
    }
}
