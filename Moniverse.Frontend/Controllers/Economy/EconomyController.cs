using System;
using System.Web.Mvc;
using Moniverse.Contract;

namespace PlayverseMetrics.Controllers
{
    public class EconomyController : BaseController
    {
        [HttpGet]
        public JsonResult GetCoinFlowMacro(string game, AWSRegion region, string start, string end)
        {
            DateTime st = Convert.ToDateTime(start).ToUniversalTime();
            DateTime et = Convert.ToDateTime(end).ToUniversalTime();

            GameMonitoringConfig gameMonitoringConfig = Games.Instance.GetMoniteredGame(game);

            return JsonResult( EconomyModel.instance.GetCoinFlow(gameMonitoringConfig.Id, region, st, et) );
        }

        [HttpGet]
        public JsonResult GetCoinFlowMacroByCategory(string game, AWSRegion region, string start, string end)
        {
            DateTime st = Convert.ToDateTime(start).ToUniversalTime();
            DateTime et = Convert.ToDateTime(end).ToUniversalTime();

            GameMonitoringConfig gameMonitoringConfig = Games.Instance.GetMoniteredGame(game);

            return JsonResult( EconomyModel.instance.GetCoinFlowByCat(gameMonitoringConfig, region, st, et) );
        }


        [HttpGet]
        public JsonResult GetBuyWhales(string game, AWSRegion region, string start, string end, int cohort)
        {
            DateTime st = Convert.ToDateTime(start).ToUniversalTime();
            DateTime et = Convert.ToDateTime(end).ToUniversalTime();
            
            GameMonitoringConfig gameMonitoringConfig = Games.Instance.GetMoniteredGame(game);

            return JsonResult( EconomyModel.instance.GetBuyWhaleReport(gameMonitoringConfig, region, st, et, cohort) );
        }

        [HttpGet]
        public JsonResult GetSpendWhales(string game, AWSRegion region, string start, string end, int cohort)
        {
            DateTime st = Convert.ToDateTime(start).ToUniversalTime();
            DateTime et = Convert.ToDateTime(end).ToUniversalTime();

            GameMonitoringConfig gameMonitoringConfig = Games.Instance.GetMoniteredGame(game);

            return JsonResult( EconomyModel.instance.GetSpendWhaleReport(gameMonitoringConfig, region, st, et, cohort) );
        }
    }
}