using Playverse.Data;
using Playverse.Utilities;
using Moniverse.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Utilities;
using Moniverse.Contract;

namespace Moniverse.Service
{

    public class Games
    {
        public static Games Instance = new Games();

        public const string EMPTYGAMEID = "0-00000000000000000000000000000000";

        public List<GameMonitoringConfig> GetMonitoredGames()
        {
            List<GameMonitoringConfig> games = new List<GameMonitoringConfig>();

            // Get game info configuration
            string query =
                @"SELECT *
                FROM GameMonitoringConfig;";
            try {
                DataTable queryResults = DBManager.Instance.Query(Datastore.Monitoring, query);


                // Collect game info configuration from result
                if (queryResults.HasRows())
                {

                    foreach (DataRow row in queryResults.Rows)
                    {
                        GameMonitoringConfig game = new GameMonitoringConfig();
                        game.Id = row.Field<string>("GameId");
                        game.Title = row.Field<string>("Title");
                        game.ShortTitle = row.Field<string>("ShortTitle");
                        game.LaunchTime = row.Field<DateTime>("LaunchTime");
                        game.ActiveUserSessionTypes = row.Field<string>("ActiveUserSessionTypes").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        game.ActiveUserDeltaThresholdPct_3Min = (float)row.Field<decimal>("ActiveUserDeltaThresholdPct_3Min");
                        game.ActiveUserDeltaThresholdPct_6Min = (float)row.Field<decimal>("ActiveUserDeltaThresholdPct_6Min");
                        List<PrivacyCompareSessionTypes> privacyCompares = new List<PrivacyCompareSessionTypes>();
                        foreach (string privacyCompare in row.Field<string>("PrivacyCompareSessionTypes").Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries).ToList())
                        {
                            string[] compares = privacyCompare.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                            if (compares.Count() == 2)
                            {
                                List<string> publicTypes = compares[0].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                List<string> privateTypes = compares[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                if (publicTypes.Any() && privateTypes.Any())
                                {
                                    privacyCompares.Add(new PrivacyCompareSessionTypes()
                                    {
                                        PublicTypes = publicTypes,
                                        PrivateTypes = privateTypes
                                    });
                                }
                            }
                        }
                        game.PrivacyCompareSessionTypes = privacyCompares;
                        game.MaxRunningHostingInstances = row.Field<int>("MaxRunningHostingInstances");
                        game.NotificationSettings = new NotificationSettings()
                        {
                            Info = Convert.ToBoolean(row.Field<ulong>("Notifications_Info")),
                            Warning = Convert.ToBoolean(row.Field<ulong>("Notifications_Warning")),
                            Error = Convert.ToBoolean(row.Field<ulong>("Notifications_Error")),
                            PVSupport = Convert.ToBoolean(row.Field<ulong>("Notifications_PVSupport"))
                        };
                        games.Add(game);
                    }
                }
            }
            catch (Exception e) {
                Logger.Instance.Exception(e.Message, e.StackTrace);
            }
            
            return games;
        }
    }
}
