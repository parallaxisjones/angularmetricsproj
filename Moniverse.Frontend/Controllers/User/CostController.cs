using System;
using System.Web.Mvc;
using Moniverse.Contract;
using PlayverseMetrics.Models;
using PlayverseMetrics.Models.AWS;

namespace PlayverseMetrics.Controllers
{
    public class CostController : BaseController
    {
        [HttpGet]
        public JsonResult Index()
        {
            AWSModel awsModel = new AWSModel();
            
            var awsSummary = awsModel.FetchNetflixIceData(DateTime.MinValue, DateTime.MaxValue);

            return new JsonResult() {Data = awsSummary, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

        [HttpGet]
        public JsonResult DollarCostAveragePerDAU(string game, AWSRegion region, string interval, string start, string end)
        {
            DateTime st = Convert.ToDateTime(start).ToUniversalTime();
            DateTime et = Convert.ToDateTime(end).ToUniversalTime();

            GameMonitoringConfig gameMonitoringConfig = Games.Instance.GetMoniteredGame(game);

            UsersModel userModel = new UsersModel();
            var timeSeries = userModel.GetDollarCostAveragePerDAU(gameMonitoringConfig, region, st, et);

            return JsonResult( timeSeries );
        }
    }
}