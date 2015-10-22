using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Moniverse.Contract;
using PlayverseMetrics.Models;

namespace PlayverseMetrics.Controllers
{
    public class MapController : BaseController
    {
        // GET: Map
        [HttpGet]
        public JsonResult LoginMap(string game, string region, string interval, string start, string end)
        {
            GameMonitoringConfig title = Games.Instance.GetMonitoredGames().FirstOrDefault(x => x.ShortTitle == game);
            //need to validate these
            TimeInterval i = (TimeInterval)Enum.Parse(typeof(TimeInterval), interval);
            DateTime st = Convert.ToDateTime(start);
            DateTime et = Convert.ToDateTime(end);
            AWSRegion r = AWSRegion.All;
            return Json(UsersModel.Instance.GetLoginDenistyMap(i, r, st, et, title), "application/json", System.Text.Encoding.UTF8, JsonRequestBehavior.AllowGet);

        }
    }
}