using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PlayverseMetrics.Models;
using Moniverse.Contract;

namespace PlayverseMetrics.Controllers
{

    public class RetentionController : BaseController
    {

        [HttpGet]
        [AllowAnonymous]
        public JsonResult Report()
        {
            return JsonResult( RetentionModel.Instance.RetentionReport() );
        }


        [HttpGet]
        [AllowAnonymous]
        public JsonResult ReportAverage()
        {
            return JsonResult( RetentionModel.Instance.AverageRetention() );
        }

        [HttpGet]
        [AllowAnonymous]
        public JsonResult InstallsLogins()
        {
            return JsonResult( RetentionModel.Instance.InstallsLoginsTimeSeries() );
        }


        [HttpGet]
        [AllowAnonymous]
        public JsonResult ReturnersReport()
        {
            //generating mock data for the graph

            //get the date
            DateTime today = DateTime.UtcNow;
            //init the table

            //init the lists of HighCharts configured points for
            //New Users Returns
            //Continuing User returns
            //Returning user returns (lol)
            List<PlaytricsPoint> NURRList = new List<PlaytricsPoint>();
            List<PlaytricsPoint> CURRList = new List<PlaytricsPoint>();
            List<PlaytricsPoint> RURRList = new List<PlaytricsPoint>();

            //this object has a contract with the Series javascript object.
            //they friends
            TimeSeriesDataNew tsd = new TimeSeriesDataNew();

            // for each row returned fill out the return
            List<ReturnerRow> ReturnerTable = RetentionModel.Instance.GetReturnerRetention(today);
            tsd.EndDate = ReturnerTable.OrderByDescending(x => x.Date).First().Date;
            tsd.StartDate = ReturnerTable.OrderByDescending(x => x.Date).Last().Date;

            foreach (ReturnerRow row in ReturnerTable.ToList())
            {


                //CreateActionInvoker high Charts configured points
                // [x,y] (s)datetime, (Count)]
                PlaytricsPoint CURR = new PlaytricsPoint();
                PlaytricsPoint NURR = new PlaytricsPoint();
                PlaytricsPoint RURR = new PlaytricsPoint();

                //need to make the date format in the correct timezone encoded JSON friendly format
                // .toJSON() in javascriptland

                CURR.RecordTimestamp = row.Date;
                CURR.Count = row.CURR;
                CURRList.Add(CURR);

                NURR.RecordTimestamp = row.Date;
                NURR.Count = row.NURR;
                NURRList.Add(NURR);

                RURR.RecordTimestamp = row.Date;
                RURR.Count = row.RURR;
                RURRList.Add(RURR);

            }


            Dictionary<string, List<PlaytricsPoint>> serieses = new Dictionary<string, List<PlaytricsPoint>>() {
            {"NURR", NURRList},
            {"CURR", CURRList},
            {"RURR", RURRList}
            };
            tsd.SeriesData = serieses;

            ReturningRetentionResponse RRresponse = new ReturningRetentionResponse();
            RRresponse.Chart = tsd;
            RRresponse.Table = ReturnerTable;

            return JsonResult( RRresponse );
        }

        [HttpGet]
        [AllowAnonymous]
        public JsonResult ReturnersSeries(string game, AWSRegion region, string interval, string start, string end)
        {
            TimeInterval i = (TimeInterval)Enum.Parse(typeof(TimeInterval), interval);
            DateTime st = Convert.ToDateTime(start).ToUniversalTime();
            DateTime et = Convert.ToDateTime(end).ToUniversalTime();

            GameMonitoringConfig gameMonitoringConfig = Games.Instance.GetMoniteredGame(game);

            return JsonResult( RetentionModel.Instance.ReturnerTimeSeries(gameMonitoringConfig, i, region, st, et) );
        }

        [HttpGet]
        [AllowAnonymous]
        public JsonResult ReturnersDataTable(string game, AWSRegion region, string interval, string start, string end)
        {
            TimeInterval i = (TimeInterval)Enum.Parse(typeof(TimeInterval), interval);
            DateTime st = Convert.ToDateTime(start);
            DateTime et = Convert.ToDateTime(end);

            GameMonitoringConfig gameMonitoringConfig = Games.Instance.GetMoniteredGame(game);

            return JsonResult( RetentionModel.Instance.ReturnerDataTable(gameMonitoringConfig, i, region, st, et) );
        }
    }
}