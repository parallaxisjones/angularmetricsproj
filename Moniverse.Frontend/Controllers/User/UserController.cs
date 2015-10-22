using System;
using System.Collections.Generic;
using System.Web.Mvc;
using PlayverseMetrics.Models;
using Moniverse.Contract;

namespace PlayverseMetrics.Controllers
{

    public class UserController : BaseController
    {
        #region Counts

        [HttpGet]
        public JsonResult TotalUserCount()
        {
            var Users = new Dictionary<string, int>();

            Users.Add("Count", UsersModel.Instance.GetRegisteredUsersCount());

            return JsonResult( Users );
        }

        [HttpGet]
        public JsonResult ExternalUserCount()
        {
            var Users = new Dictionary<string, int>();

            Users.Add("Count", UsersModel.Instance.GetRegisteredExternalUsersCount());

            return JsonResult( Users );
        }

        [HttpGet]
        public JsonResult CurrentActivePlayersByGame(string title)
        {
            var Users = new Dictionary<string, int>();
            GameMonitoringConfig gameMonitoringConfig = Games.Instance.GetMoniteredGame(title);
            Users.Add("Count", UsersModel.Instance.GetGameSessionUserCountByGameID(gameMonitoringConfig.Id));

            return JsonResult( Users );
        }

        [HttpGet]
        public JsonResult EventListenerCountByGame(string title)
        {
            var Users = new Dictionary<string, int>();
            GameMonitoringConfig gameMonitoringConfig = Games.Instance.GetMoniteredGame(title);
            Users.Add("Count", UsersModel.Instance.GetGameSessionUserCountByGameID(gameMonitoringConfig.Id));

            return JsonResult( Users );
        }

        [HttpGet]
        public JsonResult PartiesCount()
        {
            var Users = new Dictionary<string, int>();

            Users.Add("Count", UsersModel.Instance.GetPartiesCount());

            return JsonResult (Users );
        }

        [HttpGet]
        public JsonResult PartiesCountByGame(string title)
        {
            var Users = new Dictionary<string, int>();

            GameMonitoringConfig gameMonitoringConfig = Games.Instance.GetMoniteredGame(title);
            Users.Add("Count", UsersModel.Instance.GetPartiesCountByGameID(gameMonitoringConfig.Id));

            return JsonResult( Users );
        }
        #endregion

        [HttpGet]
        public JsonResult DailyActiveUser(AWSRegion region, string interval, string start, string end)
        {
            //need to validate these
            TimeInterval i = (TimeInterval)Enum.Parse(typeof(TimeInterval), interval);
            DateTime st = Convert.ToDateTime(start);
            DateTime et = Convert.ToDateTime(end);

            Dictionary<string, List<DailyActiveUserSummary>> counts = new Dictionary<string, List<DailyActiveUserSummary>>();
            List<GameMonitoringConfig> games = Games.Instance.GetMonitoredGames();
            
            foreach (GameMonitoringConfig game in games) {
                counts.Add(game.ShortTitle, UsersModel.Instance.GetDailyActiveUserSummaryById(game.Id, i, region, st, et));
            }
            return JsonResult( counts );
        }

        [HttpGet]
        public JsonResult DailyActiveUserByGame(string game, AWSRegion region, string interval, string start, string end)
        {
            //need to validate these
            TimeInterval i = (TimeInterval)Enum.Parse(typeof(TimeInterval), interval);
            DateTime st = Convert.ToDateTime(start);
            DateTime et = Convert.ToDateTime(end);

            GameMonitoringConfig gameMonitoringConfig = Games.Instance.GetMoniteredGame(game);

            return JsonResult(UsersModel.Instance.GetDailyActiveUsersByGame(gameMonitoringConfig, i, region, st, et));
        }

        [HttpGet]
        [AllowAnonymous]
        public JsonResult getCurrentOn(string game, string region, string interval, string start, string end)
        {
            GameMonitoringConfig gameMonitoringConfig = Games.Instance.GetMoniteredGame(game);

            TimeInterval i = (TimeInterval)Enum.Parse(typeof(TimeInterval), interval);
            DateTime st = Convert.ToDateTime(start);
            DateTime et = Convert.ToDateTime(end);

            return JsonResult( UsersModel.Instance.GetCurrentOnline(i, st, et, gameMonitoringConfig) );
        }

        [HttpGet]
        [AllowAnonymous]
        public JsonResult GetUsersByRegion(string game, AWSRegion region, TimeInterval interval, string start, string end)
        {
            DateTime st = Convert.ToDateTime(start);
            DateTime et = Convert.ToDateTime(end);

            GameMonitoringConfig gameMonitoringConfig = Games.Instance.GetMoniteredGame(game);
            return JsonResult( UsersModel.Instance.GetUsersByRegion(interval, region, st, et, gameMonitoringConfig) );
        }

    }

}