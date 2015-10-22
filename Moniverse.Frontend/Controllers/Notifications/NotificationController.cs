using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Moniverse.Contract;
using Moniverse.Service;
using System.Linq;
namespace PlayverseMetrics.Controllers
{
    public class NotificationController : BaseController
    {
        [HttpGet]
        public JsonResult GetNotificationsByGame(string game, string start, string end) {
            List<MoniverseNotification> notifications = new List<MoniverseNotification>();
            DateTime st = Convert.ToDateTime(start);
            DateTime et = Convert.ToDateTime(end);
            GameMonitoringConfig g = Games.Instance.GetMonitoredGames().Where(x => x.ShortTitle == game).FirstOrDefault();

            notifications = UserNotification.Instance.GetNotificationsForGame(Games.EMPTYGAMEID, st, et);
            
            return JsonResult(notifications);
        }

        [HttpGet]
        public JsonResult GetNotifications(string game, string start, string end)
        {
            List<MoniverseNotification> notifications = new List<MoniverseNotification>();
            DateTime st = Convert.ToDateTime(start);
            DateTime et = Convert.ToDateTime(end);
            GameMonitoringConfig g = Games.Instance.GetMonitoredGames().Where(x => x.ShortTitle == game).FirstOrDefault();

            notifications = UserNotification.Instance.GetNotificationsForGame(g.Id, st, et);

            return JsonResult(notifications);
        }

        [HttpGet]
        public JsonResult GetNotification(int Id)
        {
            List<MoniverseNotification> notifications = new List<MoniverseNotification>();
            //DateTime st = Convert.ToDateTime(start);
            //DateTime et = Convert.ToDateTime(end);
            //GameMonitoringConfig g = Games.Instance.GetMonitoredGames().Where(x => x.ShortTitle == game).FirstOrDefault();

            notifications = UserNotification.Instance.GetNotificationsForGame(Games.EMPTYGAMEID, new DateTime(2015, 01, 01), DateTime.UtcNow);

            return JsonResult(notifications);
        }

    }
}