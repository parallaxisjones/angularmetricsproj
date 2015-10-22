using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Moniverse.Contract;

namespace PlayverseMetrics.Controllers
{
    public class GameController : BaseController
    {
        [HttpGet]
        public JsonResult ExampleSessionLengthData(string game = "DD2", string region = "", string interval = "", string start = "", string end = "")
        {
            //need to validate these
            DateTime et = DateTime.UtcNow;
            DateTime st = new DateTime(et.Year, et.Month, 1);

            if (region == "")
            {
                region = "0";
            }
            if (interval == "")
            {
                interval = "15";
            }
            if (start != "")
            {
                st = Convert.ToDateTime(start).ToUniversalTime();
            }
            if (end != "")
            {
                et = Convert.ToDateTime(end).ToUniversalTime();
            }
            AWSRegion r = (AWSRegion)Enum.Parse(typeof(AWSRegion), region);
            TimeInterval i = (TimeInterval)Enum.Parse(typeof(TimeInterval), interval);

            List<PVTableRow> SeriesList = GameSessionsModel.Instance.AverageSessionLengthChart(game.ToUpper(), i, r, st, et);

            return JsonResult( SeriesList );

        }

        [HttpGet]
        public JsonResult UsersOnlineBySessionType(string game, AWSRegion region, TimeInterval interval, string start, string end)
        {
            DateTime st = Convert.ToDateTime(start);
            DateTime et = Convert.ToDateTime(end);

            return JsonResult(GameSessionsModel.Instance.UsersOnlineBySessionType(game, interval, region, st, et));
        }

        [HttpGet]
        public JsonResult SessionLengthChartData(string game, AWSRegion region, TimeInterval interval, string start, string end)
        {
            DateTime st = Convert.ToDateTime(start);
            DateTime et = Convert.ToDateTime(end);

            return JsonResult(GameSessionsModel.Instance.AverageSessionLengthChart(game, interval, region, st, et));
        }

        [HttpGet]
        public JsonResult SessionLengthGraphData(string game, AWSRegion region, TimeInterval interval, string start, string end)
        {
            DateTime st = Convert.ToDateTime(start);
            DateTime et = Convert.ToDateTime(end);

            return JsonResult(GameSessionsModel.Instance.GetAverageSessionLength(interval, region, st, et, game));
        }

        [HttpGet]
        [AllowCrossSiteJsonAttribute]
        public JsonResult PrivateVsPublic(string game, AWSRegion region, TimeInterval interval, string start, string end)
        {
            DateTime st = Convert.ToDateTime(start);
            DateTime et = Convert.ToDateTime(end);

            return JsonResult(GameSessionsModel.Instance.GetPrivateSessionTimeSeries(game, interval, st, et));
        }

    }
}
