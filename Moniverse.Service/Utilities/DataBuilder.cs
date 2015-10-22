using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Moniverse.Service;
using Playverse.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using System.Globalization;
using Moniverse.Contract;
using Amib.Threading;
using System.Threading;
using System.Data;
using GenericParsing;

namespace Moniverse.Service
{
    public class DataBuilder : ServiceClassBase
    {
        public DataBuilder() : base() { }
        public static DataBuilder Instance = new DataBuilder();
        
        public void TryCreateMoniverseTables(string filePath) {

            string DBSetupScript;
            try
            {
                DBSetupScript = File.ReadAllText(filePath);

            }
            catch (Exception)
            {
                Console.WriteLine("File Not Found");
                throw;
            }

            DBManager.Instance.Insert(Datastore.Monitoring, DBSetupScript);
        }

        public void TryRebuildUserSessionMeta(string filePath) {
            TryRebuildLogin(filePath);
            TryRebuildLogoff(filePath);
        }

        //need to update this
        public void TryRebuildLogin(string filePath)
        {
            string collectionName = "Login";

            IEnumerable<DataRow> rows = LoadFromCSV(collectionName, filePath).Rows.Cast<DataRow>();
            foreach (List<DataRow> entryBatch in rows.Batch<DataRow>(1000))
            {
                List<Login> loginlistbatch = new List<Login>();
                foreach(DataRow entry in entryBatch){
                    DateTime LoginTimestamp;
                    Login u = new Login()
                    {
                        UserId = entry["userid"].ToString(),
                        GameId = entry["gameid"].ToString(),
                        UserSessionId = entry["sessionid"].ToString(),
                        Platform = entry["platform"].ToString(),
                        LoginTimestamp = (DateTime.TryParse(entry["keen.timestamp"].ToString(), out LoginTimestamp) != false) ? LoginTimestamp : DateTime.MinValue,
                        City = (entry["location.city"] == null || entry["location.city"] == null) ? "" : entry["location.city"].ToString(),
                        Country = (entry["location.country"] == null || entry["location.country"] == null) ? "" : entry["location.country"].ToString(),
                        Region = (entry["location.region"] == null || entry["location.region"] == null) ? "" : entry["location.region"].ToString(),
                        Longitude = (entry["location.longitude"].ToString() == "" || entry["location.longitude"].ToString() == null) ? 0 : float.Parse(entry["location.longitude"].ToString(), CultureInfo.InvariantCulture.NumberFormat),
                        Latitude = (entry["location.latitude"].ToString() == "" || entry["location.latitude"].ToString() == null) ? 0 : float.Parse(entry["location.latitude"].ToString(), CultureInfo.InvariantCulture.NumberFormat),
                        LocationId = (entry["location.id"].ToString() == "" || entry["location.id"].ToString() == null) ? 0 : Convert.ToInt32(entry["location.id"].ToString()),
                        InstallDateRecord = (entry["isnewuser"].ToString() == "" || entry["isnewuser"].ToString() == null) ? 0 : ((Convert.ToBoolean(entry["isnewuser"].ToString())) ? 1 : 0),
                        SessionLength = -1,
                    };

                    u.RetentionCohortType = UserSessions.Instance.GetRetentionType(u);
                    loginlistbatch.Add(u);
                }
                try
                {
                    UserSessions.Instance.processLogins(loginlistbatch);
                }
                catch (Exception ex)
                {
                    Logger.Instance.Info("New User Insert Fail");
                }               
            }

        }

        public void TryRebuildLogoff(string filePath)
        {
            string collectionName = "Logoff";
            List<Logoff> LogoffBatch = new List<Logoff>();
            foreach (DataRow entry in LoadFromCSV(collectionName, filePath).Rows)
            {
                DateTime LoginTimestamp;
                Logoff u = new Logoff()
                {
                    UserId = entry["userid"].ToString(),
                    GameId = entry["gameid"].ToString(),
                    UserSessionId = entry["sessionid"].ToString(),
                    Platform = entry["platform"].ToString(),
                    City = (entry["location.city"] == null || entry["location.city"] == null) ? "" : entry["location.city"].ToString(),
                    Country = (entry["location.country"] == null || entry["location.country"] == null) ? "" : entry["location.country"].ToString(),
                    Region = (entry["location.region"] == null || entry["location.region"] == null) ? "" : entry["location.region"].ToString(),
                    Longitude = (entry["location.longitude"].ToString() == "" || entry["location.longitude"].ToString() == null) ? 0 : float.Parse(entry["location.longitude"].ToString(), CultureInfo.InvariantCulture.NumberFormat),
                    Latitude = (entry["location.latitude"].ToString() == "" || entry["location.latitude"].ToString() == null) ? 0 : float.Parse(entry["location.latitude"].ToString(), CultureInfo.InvariantCulture.NumberFormat),
                    LocationId = (entry["location.id"].ToString() == "" || entry["location.id"].ToString() == null) ? 0 : Convert.ToInt32(entry["location.id"].ToString()),
                    InstallDateRecord = (entry["isnewuser"].ToString() == "" || entry["isnewuser"].ToString() == null) ? 0 : ((Convert.ToBoolean(entry["isnewuser"].ToString())) ? 1 : 0),
                    SessionLength = -1,
                    LogoffTimestamp = (DateTime.TryParse(entry["keen.timestamp"].ToString(), out LoginTimestamp) != false) ? LoginTimestamp : DateTime.MinValue
                };

                LogoffBatch.Add(u);
            }


            try
            {
                UserSessions.Instance.processLogoffs(LogoffBatch);
            }
            catch (Exception)
            {
                Logger.Instance.Info("New User Insert Fail");
            }
        }

        public void TryRebuild(string collectionName, string filePath)
        {
            try
            {
                switch (collectionName.ToLower())
                {
                    case "login":
                        TryRebuildLogin(filePath);
                        break;
                    case "logoff":
                        TryRebuildLogoff(filePath);
                        break;
                }
            }
            catch (Exception)
            {
                Logger.Instance.Info("");
            }

           
        }

        protected DataTable  LoadFromCSV(string CollectionName, string filePath)
        {
            return LoadWithGenericParser(CollectionName, filePath, "asc");
        }

        protected DataTable LoadWithGenericParser(string CollectionName, string filePath, string order)
        {

            DataTable dt = new DataTable();
            string wantedCSV = "";
            foreach (string fileName in Directory.GetFiles(filePath))
            {
                CultureInfo culture = new CultureInfo("en-US");
                if (culture.CompareInfo.IndexOf(fileName, CollectionName, CompareOptions.IgnoreCase) == -1)
                {
                    continue;
                }
                else if (culture.CompareInfo.IndexOf(fileName, CollectionName, CompareOptions.IgnoreCase) > 0)
                {
                    wantedCSV = fileName;
                    break;
                }
                else
                {
                    Logger.Instance.Info("File Not Found. Press Any Key");
                    Console.ReadKey();
                }
            }
            //return CSVReader.instance.GetData(wantedCSV, '\n', ',');
            using (GenericParserAdapter parser = new GenericParserAdapter(wantedCSV))
            {
                parser.FirstRowHasHeader = true;
                dt = parser.GetDataTable();
            }
            Dictionary<string, List<DataRow>> dict = dt.AsEnumerable()
                .OrderBy(x => x.Field<string>("keen.timestamp"))
                .GroupBy(x => x.Field<string>("sessionid"))
                .ToDictionary(x => x.Key, y => y.ToList());
            return dt;
        }


        #region publicMigrationMethods
        public void MigrateAllGameSessionUserStats(SmartThreadPool tp)
        {
            tp.QueueWorkItem(() =>
            {
                GameSessionUserStats5MinuteMigrate();
            });
            tp.QueueWorkItem(() =>
            {
                GameSessionUserStats15MinuteMigrate();
            });
            tp.QueueWorkItem(() =>
            {
                GameSessionUserStats30Migrate();
            });
            tp.QueueWorkItem(() =>
            {
                GameSessionUserStats60Migrate();
            });
            tp.QueueWorkItem(() =>
            {
                GameSessionUserStats360Migrate();
            });
            tp.QueueWorkItem(() =>
            {
                GameSessionUserStats720Migrate();
            });
            tp.QueueWorkItem(() =>
            {
                GameSessionUserStats1440Migrate();
            });
        }

        public void MigrateAllGameUserActivity(SmartThreadPool tp)
        {

            tp.QueueWorkItem(() =>
            {
                GameUserActivity5MinuteMigrate();
            });
            tp.QueueWorkItem(() =>
            {
                GameUserActivity15MinuteMigrate();
            });
            tp.QueueWorkItem(() =>
            {
                GameUserActivity30Migrate();
            });
            tp.QueueWorkItem(() =>
            {
                GameUserActivity60Migrate();
            });
            tp.QueueWorkItem(() =>
            {
                GameUserActivity360Migrate();
            });
            tp.QueueWorkItem(() =>
            {
                GameUserActivity720Migrate();
            });
            tp.QueueWorkItem(() =>
            {
                GameUserActivity1440Migrate();
            });
        }

        public void Migrate5Minute(SmartThreadPool tp)
        {
            tp.QueueWorkItem(() =>
            {
                GameSessionUserStats5MinuteMigrate();
            });
            tp.QueueWorkItem(() =>
            {
                GameUserActivity5MinuteMigrate();
            });
        }
        public void Migrate15Minute(SmartThreadPool tp)
        {
            tp.QueueWorkItem(() =>
            {
                GameSessionUserStats15MinuteMigrate();
            });
            tp.QueueWorkItem(() =>
            {
                GameUserActivity15MinuteMigrate();
            });
        }
        public void Migrate30Minute(SmartThreadPool tp)
        {
            tp.QueueWorkItem(() =>
            {
                GameSessionUserStats30Migrate();
            });
            tp.QueueWorkItem(() =>
            {
                GameUserActivity30Migrate();
            });
        }
        public void Migrate1Hour(SmartThreadPool tp)
        {
            tp.QueueWorkItem(() =>
            {
                GameSessionUserStats60Migrate();
            });
            tp.QueueWorkItem(() =>
            {
                GameUserActivity60Migrate();
            });
        }
        public void Migrate6Hour(SmartThreadPool tp)
        {
            tp.QueueWorkItem(() =>
            {
                GameSessionUserStats360Migrate();
            });
            tp.QueueWorkItem(() =>
            {
                GameUserActivity360Migrate();
            });
        }
        public void Migrate12Hour(SmartThreadPool tp)
        {
            tp.QueueWorkItem(() =>
            {
                GameSessionUserStats720Migrate();
            });
            tp.QueueWorkItem(() =>
            {
                GameUserActivity720Migrate();
            });
        }
        public void Migrate24Hour(SmartThreadPool tp)
        {
            tp.QueueWorkItem(() =>
            {
                GameSessionUserStats1440Migrate();
            });
            tp.QueueWorkItem(() =>
            {
                GameUserActivity1440Migrate();
            });
        }
        #endregion

        #region DataPartitioning GameUserActivity
        protected void GameUserActivity5MinuteMigrate() {

            string statement = @"SET SQL_SAFE_UPDATES = 0;
-- GameSessionUserStats - 5 MIN INTERVAL

truncate GameUserActivity_5min;
INSERT INTO GameUserActivity_5min
SELECT 0,
  GameId,
  RegionName,
  DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(5 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 5) * 60) + SECOND(RecordTimestamp) SECOND) AS RecordTimestamp,
  ROUND(AVG(GameSessionUsers)) as GameSessionUsers,
        ROUND(AVG(EventListeners)) as EventListeners,
        ROUND(AVG(TitleScreenUsers)) as TitleScreenUsers,
		SessionTypeName_0,
		ROUND(AVG(SessionTypeUsers_0)) as SessionTypeUsers_0,
		SessionTypeName_1,
		ROUND(AVG(SessionTypeUsers_1)) as SessionTypeUsers_1,
		SessionTypeName_2,
		ROUND(AVG(SessionTypeUsers_2)) as SessionTypeUsers_2,
		SessionTypeName_3,
		ROUND(AVG(SessionTypeUsers_3)) as SessionTypeUsers_3,
		SessionTypeName_4,
		ROUND(AVG(SessionTypeUsers_4)) as SessionTypeUsers_4,
		SessionTypeName_5,
		ROUND(AVG(SessionTypeUsers_5)) as SessionTypeUsers_5,
		SessionTypeName_6,
		ROUND(AVG(SessionTypeUsers_6)) as SessionTypeUsers_6,
		SessionTypeName_7,
		ROUND(AVG(SessionTypeUsers_7)) SessionTypeUsers_7,
		ROUND(AVG(SessionTypeUsers_Other)) as SessionTypeUsers_Other
FROM GameUserActivity
WHERE RecordTimestamp < NOW()
GROUP BY GameId,
		 RegionName,
         DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(5 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 5) * 60) + SECOND(RecordTimestamp) SECOND),
		SessionTypeName_0,
		SessionTypeName_1,
		SessionTypeName_2,
		SessionTypeName_3,
		SessionTypeName_4,
		SessionTypeName_5,
		SessionTypeName_6,
		SessionTypeName_7
ORDER BY DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(5 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 5) * 60) + SECOND(RecordTimestamp) SECOND) desc,
   GameId;";
            MoniverseResponse response = new MoniverseResponse(){
                TaskName = "5 Minute GameUserActivity Migration",
                Status = "Fail",
                TimeStamp = DateTime.UtcNow
            };
            try 
	        {
                DataBuilder.Service(Service => {
                    response = Service.Insert(new MoniverseRequest()
                    {
                        TaskName = "5 Minute GameUserActivity Migration",
                        Task = statement,
                        TimeStamp = DateTime.UtcNow

                    });	                
                });	
	        }
	        catch (Exception ex)
	        {

                Logger.Instance.Info(ex.Message);
	        }


            Logger.Instance.Info(response.Status);
        }

        protected void GameUserActivity15MinuteMigrate()
        {

            string statement = @"SET SQL_SAFE_UPDATES = 0;   
-- select * from GameSessionUserStats_15min;   
-- GameSessionUserStats - 15 MIN INTERVAL
truncate GameUserActivity_15min;
-- select * from GameUserActivity_24hour;
INSERT INTO GameUserActivity_15min
SELECT 0,
  GameId,
  RegionName,
  DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(15 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 15) * 60) + SECOND(RecordTimestamp) SECOND) AS RecordTimestamp,
  ROUND(AVG(GameSessionUsers)) as GameSessionUsers,
        ROUND(AVG(EventListeners)) as EventListeners,
        ROUND(AVG(TitleScreenUsers)) as TitleScreenUsers,
		SessionTypeName_0,
		ROUND(AVG(SessionTypeUsers_0)) as SessionTypeUsers_0,
		SessionTypeName_1,
		ROUND(AVG(SessionTypeUsers_1)) as SessionTypeUsers_1,
		SessionTypeName_2,
		ROUND(AVG(SessionTypeUsers_2)) as SessionTypeUsers_2,
		SessionTypeName_3,
		ROUND(AVG(SessionTypeUsers_3)) as SessionTypeUsers_3,
		SessionTypeName_4,
		ROUND(AVG(SessionTypeUsers_4)) as SessionTypeUsers_4,
		SessionTypeName_5,
		ROUND(AVG(SessionTypeUsers_5)) as SessionTypeUsers_5,
		SessionTypeName_6,
		ROUND(AVG(SessionTypeUsers_6)) as SessionTypeUsers_6,
		SessionTypeName_7,
		ROUND(AVG(SessionTypeUsers_7)) SessionTypeUsers_7,
		ROUND(AVG(SessionTypeUsers_Other)) as SessionTypeUsers_Other
FROM GameUserActivity
WHERE RecordTimestamp < NOW()
GROUP BY GameId,
		 RegionName,
         DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(15 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 15) * 60) + SECOND(RecordTimestamp) SECOND),
		SessionTypeName_0,
		SessionTypeName_1,
		SessionTypeName_2,
		SessionTypeName_3,
		SessionTypeName_4,
		SessionTypeName_5,
		SessionTypeName_6,
		SessionTypeName_7
ORDER BY DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(15 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 15) * 60) + SECOND(RecordTimestamp) SECOND) desc,
   GameId;";
            MoniverseResponse response = new MoniverseResponse()
            {
                TaskName = "15 Minute GameUserActivity Migration",
                Status = "Fail",
                TimeStamp = DateTime.UtcNow
            };
            try
            {
                DataBuilder.Service(Service => {
                    response = Service.Insert(new MoniverseRequest()
                    {
                        TaskName = "15 Minute GameUserActivity Migration",
                        Task = statement,
                        TimeStamp = DateTime.UtcNow

                    });                
                });
            }
            catch (Exception ex)
            {

                Logger.Instance.Info(ex.Message);
            }


            Logger.Instance.Info(response.Status);
        }

        protected void GameUserActivity30Migrate()
        {

            string statement = @"SET SQL_SAFE_UPDATES = 0;
-- GameSessionUserStats - 30 MIN INTERVAL
truncate GameUserActivity_30min;
-- select * from GameUserActivity_24hour;
INSERT INTO GameUserActivity_30min
SELECT 0,
  GameId,
  RegionName,
  DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(30 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 30) * 60) + SECOND(RecordTimestamp) SECOND) AS RecordTimestamp,
  ROUND(AVG(GameSessionUsers)) as GameSessionUsers,
        ROUND(AVG(EventListeners)) as EventListeners,
        ROUND(AVG(TitleScreenUsers)) as TitleScreenUsers,
		SessionTypeName_0,
		ROUND(AVG(SessionTypeUsers_0)) as SessionTypeUsers_0,
		SessionTypeName_1,
		ROUND(AVG(SessionTypeUsers_1)) as SessionTypeUsers_1,
		SessionTypeName_2,
		ROUND(AVG(SessionTypeUsers_2)) as SessionTypeUsers_2,
		SessionTypeName_3,
		ROUND(AVG(SessionTypeUsers_3)) as SessionTypeUsers_3,
		SessionTypeName_4,
		ROUND(AVG(SessionTypeUsers_4)) as SessionTypeUsers_4,
		SessionTypeName_5,
		ROUND(AVG(SessionTypeUsers_5)) as SessionTypeUsers_5,
		SessionTypeName_6,
		ROUND(AVG(SessionTypeUsers_6)) as SessionTypeUsers_6,
		SessionTypeName_7,
		ROUND(AVG(SessionTypeUsers_7)) SessionTypeUsers_7,
		ROUND(AVG(SessionTypeUsers_Other)) as SessionTypeUsers_Other
FROM GameUserActivity
WHERE RecordTimestamp < NOW()
GROUP BY GameId,
		 RegionName,
         DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(30 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 30) * 60) + SECOND(RecordTimestamp) SECOND),
		SessionTypeName_0,
		SessionTypeName_1,
		SessionTypeName_2,
		SessionTypeName_3,
		SessionTypeName_4,
		SessionTypeName_5,
		SessionTypeName_6,
		SessionTypeName_7
ORDER BY DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(30 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 30) * 60) + SECOND(RecordTimestamp) SECOND) desc,
   GameId;";
            MoniverseResponse response = new MoniverseResponse()
            {
                TaskName = "30 Minute GameUserActivity Migration",
                Status = "Fail",
                TimeStamp = DateTime.UtcNow
            };
            try
            {
                DataBuilder.Service(Service => {
                    response = Service.Insert(new MoniverseRequest()
                    {
                        TaskName = "30 Minute GameUserActivity Migration",
                        Task = statement,
                        TimeStamp = DateTime.UtcNow

                    });                
                });
            }
            catch (Exception ex)
            {

                Logger.Instance.Info(ex.Message);
            }


            Logger.Instance.Info(response.Status);
        }

        protected void GameUserActivity60Migrate()
        {

            string statement = @"SET SQL_SAFE_UPDATES = 0;
     -- GameSessionUserStats - 60 MIN INTERVAL
truncate GameUserActivity_hour;
-- select * from GameUserActivity_24hour;
INSERT INTO GameUserActivity_hour
SELECT 0,
  GameId,
  RegionName,
  DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(60 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 60) * 60) + SECOND(RecordTimestamp) SECOND) AS RecordTimestamp,
  ROUND(AVG(GameSessionUsers)) as GameSessionUsers,
        ROUND(AVG(EventListeners)) as EventListeners,
        ROUND(AVG(TitleScreenUsers)) as TitleScreenUsers,
		SessionTypeName_0,
		ROUND(AVG(SessionTypeUsers_0)) as SessionTypeUsers_0,
		SessionTypeName_1,
		ROUND(AVG(SessionTypeUsers_1)) as SessionTypeUsers_1,
		SessionTypeName_2,
		ROUND(AVG(SessionTypeUsers_2)) as SessionTypeUsers_2,
		SessionTypeName_3,
		ROUND(AVG(SessionTypeUsers_3)) as SessionTypeUsers_3,
		SessionTypeName_4,
		ROUND(AVG(SessionTypeUsers_4)) as SessionTypeUsers_4,
		SessionTypeName_5,
		ROUND(AVG(SessionTypeUsers_5)) as SessionTypeUsers_5,
		SessionTypeName_6,
		ROUND(AVG(SessionTypeUsers_6)) as SessionTypeUsers_6,
		SessionTypeName_7,
		ROUND(AVG(SessionTypeUsers_7)) SessionTypeUsers_7,
		ROUND(AVG(SessionTypeUsers_Other)) as SessionTypeUsers_Other
FROM GameUserActivity
WHERE RecordTimestamp < NOW()
GROUP BY GameId,
		 RegionName,
         DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(60 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 60) * 60) + SECOND(RecordTimestamp) SECOND),
		SessionTypeName_0,
		SessionTypeName_1,
		SessionTypeName_2,
		SessionTypeName_3,
		SessionTypeName_4,
		SessionTypeName_5,
		SessionTypeName_6,
		SessionTypeName_7
ORDER BY DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(60 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 60) * 60) + SECOND(RecordTimestamp) SECOND) desc,
   GameId;";
            MoniverseResponse response = new MoniverseResponse()
            {
                TaskName = "1 hour GameUserActivity Migration",
                Status = "Fail",
                TimeStamp = DateTime.UtcNow
            };
            try
            {
                DataBuilder.Service(Service => {
                    response = Service.Insert(new MoniverseRequest()
                    {
                        TaskName = "1 hour GameUserActivity Migration",
                        Task = statement,
                        TimeStamp = DateTime.UtcNow

                    });                
                });
            }
            catch (Exception ex)
            {

                Logger.Instance.Info(ex.Message);
            }


            Logger.Instance.Info(response.Status);
        }

        protected void GameUserActivity360Migrate()
        {

            string statement = @"SET SQL_SAFE_UPDATES = 0;
-- GameSessionUserStats - 6hour INTERVAL

truncate GameUserActivity_6hour;
-- select * from GameUserActivity_6hour;
INSERT INTO GameUserActivity_6hour
SELECT 0,
  GameId,
  RegionName,
  DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(360 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 360) * 60) + SECOND(RecordTimestamp) SECOND) AS RecordTimestamp,
  ROUND(AVG(GameSessionUsers)) as GameSessionUsers,
        ROUND(AVG(EventListeners)) as EventListeners,
        ROUND(AVG(TitleScreenUsers)) as TitleScreenUsers,
		SessionTypeName_0,
		ROUND(AVG(SessionTypeUsers_0)) as SessionTypeUsers_0,
		SessionTypeName_1,
		ROUND(AVG(SessionTypeUsers_1)) as SessionTypeUsers_1,
		SessionTypeName_2,
		ROUND(AVG(SessionTypeUsers_2)) as SessionTypeUsers_2,
		SessionTypeName_3,
		ROUND(AVG(SessionTypeUsers_3)) as SessionTypeUsers_3,
		SessionTypeName_4,
		ROUND(AVG(SessionTypeUsers_4)) as SessionTypeUsers_4,
		SessionTypeName_5,
		ROUND(AVG(SessionTypeUsers_5)) as SessionTypeUsers_5,
		SessionTypeName_6,
		ROUND(AVG(SessionTypeUsers_6)) as SessionTypeUsers_6,
		SessionTypeName_7,
		ROUND(AVG(SessionTypeUsers_7)) SessionTypeUsers_7,
		ROUND(AVG(SessionTypeUsers_Other)) as SessionTypeUsers_Other
FROM GameUserActivity
WHERE RecordTimestamp < NOW()
GROUP BY GameId,
		 RegionName,
         DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(360 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 360) * 60) + SECOND(RecordTimestamp) SECOND),
		SessionTypeName_0,
		SessionTypeName_1,
		SessionTypeName_2,
		SessionTypeName_3,
		SessionTypeName_4,
		SessionTypeName_5,
		SessionTypeName_6,
		SessionTypeName_7
ORDER BY DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(360 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 360) * 60) + SECOND(RecordTimestamp) SECOND) desc,
   GameId;";
            MoniverseResponse response = new MoniverseResponse()
            {
                TaskName = "6 hour GameUserActivity Migration",
                Status = "Fail",
                TimeStamp = DateTime.UtcNow
            };
            try
            {
                DataBuilder.Service(Service => {
                    response = Service.Insert(new MoniverseRequest()
                    {
                        TaskName = "6 hour GameUserActivity Migration",
                        Task = statement,
                        TimeStamp = DateTime.UtcNow

                    });                
                });
            }
            catch (Exception ex)
            {

                Logger.Instance.Info(ex.Message);
            }


            Logger.Instance.Info(response.Status);
        }

        protected void GameUserActivity720Migrate()
        {

            string statement = @"SET SQL_SAFE_UPDATES = 0;
-- GameSessionUserStats - 12 hour INTERVAL
truncate GameUserActivity_12hour;
-- select * from GameUserActivity_12hour;
INSERT INTO GameUserActivity_12hour
SELECT 0,
  GameId,
  RegionName,
  DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(720 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 720) * 60) + SECOND(RecordTimestamp) SECOND) AS RecordTimestamp,
  ROUND(AVG(GameSessionUsers)) as GameSessionUsers,
        ROUND(AVG(EventListeners)) as EventListeners,
        ROUND(AVG(TitleScreenUsers)) as TitleScreenUsers,
		SessionTypeName_0,
		ROUND(AVG(SessionTypeUsers_0)) as SessionTypeUsers_0,
		SessionTypeName_1,
		ROUND(AVG(SessionTypeUsers_1)) as SessionTypeUsers_1,
		SessionTypeName_2,
		ROUND(AVG(SessionTypeUsers_2)) as SessionTypeUsers_2,
		SessionTypeName_3,
		ROUND(AVG(SessionTypeUsers_3)) as SessionTypeUsers_3,
		SessionTypeName_4,
		ROUND(AVG(SessionTypeUsers_4)) as SessionTypeUsers_4,
		SessionTypeName_5,
		ROUND(AVG(SessionTypeUsers_5)) as SessionTypeUsers_5,
		SessionTypeName_6,
		ROUND(AVG(SessionTypeUsers_6)) as SessionTypeUsers_6,
		SessionTypeName_7,
		ROUND(AVG(SessionTypeUsers_7)) SessionTypeUsers_7,
		ROUND(AVG(SessionTypeUsers_Other)) as SessionTypeUsers_Other
FROM GameUserActivity
WHERE RecordTimestamp < NOW()
GROUP BY GameId,
		 RegionName,
         DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(720 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 720) * 60) + SECOND(RecordTimestamp) SECOND),
		SessionTypeName_0,
		SessionTypeName_1,
		SessionTypeName_2,
		SessionTypeName_3,
		SessionTypeName_4,
		SessionTypeName_5,
		SessionTypeName_6,
		SessionTypeName_7
ORDER BY DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(720 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 720) * 60) + SECOND(RecordTimestamp) SECOND) desc,
   GameId;";
            MoniverseResponse response = new MoniverseResponse()
            {
                TaskName = "12 hour GameUserActivity Migration",
                Status = "Fail",
                TimeStamp = DateTime.UtcNow
            };
            try
            {
                DataBuilder.Service(Service => {
                    response = Service.Insert(new MoniverseRequest()
                    {
                        TaskName = "12 hour GameUserActivity Migration",
                        Task = statement,
                        TimeStamp = DateTime.UtcNow

                    });                
                });
            }
            catch (Exception ex)
            {

                Logger.Instance.Info(ex.Message);
            }


            Logger.Instance.Info(response.Status);
        }

        protected void GameUserActivity1440Migrate()
        {

            string statement = @"SET SQL_SAFE_UPDATES = 0;
-- GameSessionUserStats - 24 hour INTERVAL
truncate GameUserActivity_24hour;
-- select * from GameUserActivity_24hour;
INSERT INTO GameUserActivity_24hour
SELECT 0,
  GameId,
  RegionName,
  DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(1440 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 1440) * 60) + SECOND(RecordTimestamp) SECOND) AS RecordTimestamp,
  ROUND(AVG(GameSessionUsers)) as GameSessionUsers,
        ROUND(AVG(EventListeners)) as EventListeners,
        ROUND(AVG(TitleScreenUsers)) as TitleScreenUsers,
		SessionTypeName_0,
		ROUND(AVG(SessionTypeUsers_0)) as SessionTypeUsers_0,
		SessionTypeName_1,
		ROUND(AVG(SessionTypeUsers_1)) as SessionTypeUsers_1,
		SessionTypeName_2,
		ROUND(AVG(SessionTypeUsers_2)) as SessionTypeUsers_2,
		SessionTypeName_3,
		ROUND(AVG(SessionTypeUsers_3)) as SessionTypeUsers_3,
		SessionTypeName_4,
		ROUND(AVG(SessionTypeUsers_4)) as SessionTypeUsers_4,
		SessionTypeName_5,
		ROUND(AVG(SessionTypeUsers_5)) as SessionTypeUsers_5,
		SessionTypeName_6,
		ROUND(AVG(SessionTypeUsers_6)) as SessionTypeUsers_6,
		SessionTypeName_7,
		ROUND(AVG(SessionTypeUsers_7)) SessionTypeUsers_7,
		ROUND(AVG(SessionTypeUsers_Other)) as SessionTypeUsers_Other
FROM GameUserActivity
WHERE RecordTimestamp < NOW()
GROUP BY GameId,
		 RegionName,
         DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(1440 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 1440) * 60) + SECOND(RecordTimestamp) SECOND),
		SessionTypeName_0,
		SessionTypeName_1,
		SessionTypeName_2,
		SessionTypeName_3,
		SessionTypeName_4,
		SessionTypeName_5,
		SessionTypeName_6,
		SessionTypeName_7
ORDER BY DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(1440 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 1440) * 60) + SECOND(RecordTimestamp) SECOND) desc,
   GameId;";
            MoniverseResponse response = new MoniverseResponse()
            {
                TaskName = "24 hour GameUserActivity Migration",
                Status = "Fail",
                TimeStamp = DateTime.UtcNow
            };
            try
            {
                DataBuilder.Service(Service => {
                    response = Service.Insert(new MoniverseRequest()
                    {
                        TaskName = "24 hour GameUserActivity Migration",
                        Task = statement,
                        TimeStamp = DateTime.UtcNow

                    });                
                });
            }
            catch (Exception ex)
            {

                Logger.Instance.Info(ex.Message);
            }


            Logger.Instance.Info(response.Status);
        }
        #endregion

        #region DataPartitioning GameSessionUserStats
        protected void GameSessionUserStats5MinuteMigrate()
        {

            string statement = @"SET SQL_SAFE_UPDATES = 0;
-- GameSessionUserStats - 5 MIN INTERVAL

truncate GameSessionUserStats_5min;
INSERT INTO GameSessionUserStats_5min
SELECT 0,
  GameId,
  SessionType,
  ROUND(AVG(MaxNumPlayers)),
        AVG(AvgPlayers),
        ROUND(AVG(Sessions)),
        AVG(PrivateAvgPlayers),
        ROUND(AVG(PrivateSessions)),
        AVG(TotalAvgPlayers),
        ROUND(AVG(TotalSessions)),
DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(5 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 5) * 60) + SECOND(RecordTimestamp) SECOND) AS RecordTimestamp
FROM GameSessionUserStats
-- WHERE '2015-3-19 00:00:00' > DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(5 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 5) * 60) + SECOND(RecordTimestamp) SECOND)
GROUP BY 
	GameId,
    SessionType,
	DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(5 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 5) * 60) + SECOND(RecordTimestamp) SECOND)
ORDER BY 
	DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(5 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 5) * 60) + SECOND(RecordTimestamp) SECOND),
    GameId,
    SessionType;
";
            MoniverseResponse response = new MoniverseResponse()
            {
                TaskName = "5 Minute GameSessionUserStats Migration",
                Status = "Fail",
                TimeStamp = DateTime.UtcNow
            };
            try
            {
                DataBuilder.Service(Service => {
                    response = Service.Insert(new MoniverseRequest()
                    {
                        TaskName = "5 Minute GameSessionUserStats Migration",
                        Task = statement,
                        TimeStamp = DateTime.UtcNow

                    });                
                });
            }
            catch (Exception ex)
            {

                Logger.Instance.Info(ex.Message);
            }


            Logger.Instance.Info(response.Status);
        }

        protected void GameSessionUserStats15MinuteMigrate()
        {

            string statement = @"select * from GameSessionUserStats_15min;   
-- GameSessionUserStats - 15 MIN INTERVAL
truncate GameSessionUserStats_15min;
INSERT INTO GameSessionUserStats_15min
SELECT 0,
  GameId,
  SessionType,
  ROUND(AVG(MaxNumPlayers)),
        AVG(AvgPlayers),
        ROUND(AVG(Sessions)),
        AVG(PrivateAvgPlayers),
        ROUND(AVG(PrivateSessions)),
        AVG(TotalAvgPlayers),
        ROUND(AVG(TotalSessions)),
		DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(15 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 15) * 60) + SECOND(RecordTimestamp) SECOND) AS RecordTimestamp
FROM GameSessionUserStats
-- WHERE '2015-3-19 00:00:00' > DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(15 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 15) * 60) + SECOND(RecordTimestamp) SECOND)
GROUP BY GameId,
         SessionType,
         DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(15 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 15) * 60) + SECOND(RecordTimestamp) SECOND)
ORDER BY DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(15 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 15) * 60) + SECOND(RecordTimestamp) SECOND),
         GameId,
         SessionType;";
            MoniverseResponse response = new MoniverseResponse()
            {
                TaskName = "15 Minute GameSessionUserStats Migration",
                Status = "Fail",
                TimeStamp = DateTime.UtcNow
            };
            try
            {
                DataBuilder.Service(Service => {
                    response = Service.Insert(new MoniverseRequest()
                    {
                        TaskName = "15 Minute GameSessionUserStats Migration",
                        Task = statement,
                        TimeStamp = DateTime.UtcNow

                    });                
                });
            }
            catch (Exception ex)
            {

                Logger.Instance.Info(ex.Message);
            }


            Logger.Instance.Info(response.Status);
        }

        protected void GameSessionUserStats30Migrate()
        {

            string statement = @"-- GameSessionUserStats - 30 MIN INTERVAL
truncate GameSessionUserStats_30min;
INSERT INTO GameSessionUserStats_30min
SELECT 0,
  GameId,
  SessionType,
  ROUND(AVG(MaxNumPlayers)),
        AVG(AvgPlayers),
        ROUND(AVG(Sessions)),
        AVG(PrivateAvgPlayers),
        ROUND(AVG(PrivateSessions)),
        AVG(TotalAvgPlayers),
        ROUND(AVG(TotalSessions)),
 DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(30 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 30) * 60) + SECOND(RecordTimestamp) SECOND) AS RecordTimestamp
FROM GameSessionUserStats
-- WHERE '2015-3-19 00:00:00' > DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(30 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 30) * 60) + SECOND(RecordTimestamp) SECOND)
GROUP BY 
   GameId,
   SessionType,
   DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(30 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 30) * 60) + SECOND(RecordTimestamp) SECOND)
ORDER BY 
   DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(30 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 30) * 60) + SECOND(RecordTimestamp) SECOND),
   GameId,
   SessionType;";
            MoniverseResponse response = new MoniverseResponse()
            {
                TaskName = "30 Minute GameSessionUserStats Migration",
                Status = "Fail",
                TimeStamp = DateTime.UtcNow
            };
            try
            {
                DataBuilder.Service(Service => {
                    response = Service.Insert(new MoniverseRequest()
                    {
                        TaskName = "30 Minute GameSessionUserStats Migration",
                        Task = statement,
                        TimeStamp = DateTime.UtcNow

                    });                
                });
            }
            catch (Exception ex)
            {

                Logger.Instance.Info(ex.Message);
            }


            Logger.Instance.Info(response.Status);
        }

        protected void GameSessionUserStats60Migrate()
        {

            string statement = @"     -- GameSessionUserStats - 60 MIN INTERVAL
truncate GameSessionUserStats_hour;
INSERT INTO GameSessionUserStats_hour
SELECT 0,
  GameId,
  SessionType,
  ROUND(AVG(MaxNumPlayers)),
        AVG(AvgPlayers),
        ROUND(AVG(Sessions)),
        AVG(PrivateAvgPlayers),
        ROUND(AVG(PrivateSessions)),
        AVG(TotalAvgPlayers),
        ROUND(AVG(TotalSessions)),
		DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(60 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 60) * 60) + SECOND(RecordTimestamp) SECOND) AS RecordTimestamp
FROM GameSessionUserStats
-- WHERE '2015-3-19 00:00:00' > DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(60 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 60) * 60) + SECOND(RecordTimestamp) SECOND)
GROUP BY GameId,
   SessionType,
            DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(60 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 60) * 60) + SECOND(RecordTimestamp) SECOND)
            
ORDER BY DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(60 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 60) * 60) + SECOND(RecordTimestamp) SECOND),
   GameId,
   SessionType;
";
            MoniverseResponse response = new MoniverseResponse()
            {
                TaskName = "1 hour GameSessionUserStats Migration",
                Status = "Fail",
                TimeStamp = DateTime.UtcNow
            };
            try
            {
                DataBuilder.Service(Service => {
                    response = Service.Insert(new MoniverseRequest()
                    {
                        TaskName = "1 hour GameSessionUserStats Migration",
                        Task = statement,
                        TimeStamp = DateTime.UtcNow

                    });                
                });
            }
            catch (Exception ex)
            {

                Logger.Instance.Info(ex.Message);
            }


            Logger.Instance.Info(response.Status);
        }

        protected void GameSessionUserStats360Migrate()
        {

            string statement = @"-- GameSessionUserStats - 6hour INTERVAL
truncate GameSessionUserStats_6hour;
INSERT INTO GameSessionUserStats_6hour
SELECT 0,
  GameId,
  SessionType,
  ROUND(AVG(MaxNumPlayers)),
        AVG(AvgPlayers),
        ROUND(AVG(Sessions)),
        
        AVG(PrivateAvgPlayers),

        ROUND(AVG(PrivateSessions)),
        
        AVG(TotalAvgPlayers),
        
        ROUND(AVG(TotalSessions)),
  DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(360 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 360) * 60) + SECOND(RecordTimestamp) SECOND) AS RecordTimestamp
FROM GameSessionUserStats
-- WHERE '2015-3-19 00:00:00' > DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(360 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 360) * 60) + SECOND(RecordTimestamp) SECOND)
GROUP BY GameId,
   SessionType,
            DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(360 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 360) * 60) + SECOND(RecordTimestamp) SECOND)
            
ORDER BY DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(360 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 360) * 60) + SECOND(RecordTimestamp) SECOND),
   GameId,
   SessionType;";
            MoniverseResponse response = new MoniverseResponse()
            {
                TaskName = "6 hour GameSessionUserStats Migration",
                Status = "Fail",
                TimeStamp = DateTime.UtcNow
            };
            try
            {
                DataBuilder.Service(Service => {
                    response = Service.Insert(new MoniverseRequest()
                    {
                        TaskName = "6 hour GameSessionUserStats Migration",
                        Task = statement,
                        TimeStamp = DateTime.UtcNow

                    });                
                });
            }
            catch (Exception ex)
            {

                Logger.Instance.Info(ex.Message);
            }


            Logger.Instance.Info(response.Status);
        }
        protected void GameSessionUserStats720Migrate()
        {

            string statement = @"-- GameSessionUserStats - 12 hour INTERVAL
truncate GameSessionUserStats_12hour;
INSERT INTO GameSessionUserStats_12hour
SELECT 0,
  GameId,
  SessionType,
  ROUND(AVG(MaxNumPlayers)),
        AVG(AvgPlayers),
        ROUND(AVG(Sessions)),
        
        AVG(PrivateAvgPlayers),

        ROUND(AVG(PrivateSessions)),
        
        AVG(TotalAvgPlayers),
        
        ROUND(AVG(TotalSessions)),
 DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(720 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 720) * 60) + SECOND(RecordTimestamp) SECOND) AS RecordTimestamp
FROM GameSessionUserStats
-- WHERE '2015-3-19 00:00:00' > DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(720 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 720) * 60) + SECOND(RecordTimestamp) SECOND)
GROUP BY GameId,
   SessionType,
            DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(720 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 720) * 60) + SECOND(RecordTimestamp) SECOND)
ORDER BY DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(720 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 720) * 60) + SECOND(RecordTimestamp) SECOND),
   GameId,
   SessionType;";
            MoniverseResponse response = new MoniverseResponse()
            {
                TaskName = "12 hour GameSessionUserStats Migration",
                Status = "Fail",
                TimeStamp = DateTime.UtcNow
            };
            try
            {
                DataBuilder.Service(Service => {
                    response = Service.Insert(new MoniverseRequest()
                    {
                        TaskName = "12 hour GameSessionUserStats Migration",
                        Task = statement,
                        TimeStamp = DateTime.UtcNow

                    });                
                });
            }
            catch (Exception ex)
            {

                Logger.Instance.Info(ex.Message);
            }


            Logger.Instance.Info(response.Status);
        }
        protected void GameSessionUserStats1440Migrate()
        {

            string statement = @"   -- GameSessionUserStats - 24 hour INTERVAL
truncate GameSessionUserStats_24hour;
INSERT INTO GameSessionUserStats_24hour
SELECT 0,
  GameId,
  SessionType,
  
  ROUND(AVG(MaxNumPlayers)),
        AVG(AvgPlayers),
        ROUND(AVG(Sessions)),
        
        AVG(PrivateAvgPlayers),

        ROUND(AVG(PrivateSessions)),
        
        AVG(TotalAvgPlayers),
        
        ROUND(AVG(TotalSessions)),
DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(1440 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 1440) * 60) + SECOND(RecordTimestamp) SECOND) AS RecordTimestamp
FROM GameSessionUserStats
-- WHERE '2015-3-19 00:00:00' > DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(1440 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 1440) * 60) + SECOND(RecordTimestamp) SECOND)
GROUP BY GameId,
   SessionType,
            DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(1440 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 1440) * 60) + SECOND(RecordTimestamp) SECOND)
ORDER BY DATE_SUB(RecordTimestamp, INTERVAL (IFNULL(HOUR(RecordTimestamp) % FLOOR(1440 / 60), 0) * 60 * 60) + ((MINUTE(RecordTimestamp) % 1440) * 60) + SECOND(RecordTimestamp) SECOND),
   GameId,
   SessionType;";
            MoniverseResponse response = new MoniverseResponse()
            {
                TaskName = "24 hour GameSessionUserStats Migration",
                Status = "Fail",
                TimeStamp = DateTime.UtcNow
            };
            try
            {
                DataBuilder.Service(Service => {
                    response = Service.Insert(new MoniverseRequest()
                    {
                        TaskName = "24 hour GameSessionUserStats Migration",
                        Task = statement,
                        TimeStamp = DateTime.UtcNow

                    });                
                });
            }
            catch (Exception ex)
            {

                Logger.Instance.Info(ex.Message);
            }


            Logger.Instance.Info(response.Status);
        }
        #endregion
    }
}
