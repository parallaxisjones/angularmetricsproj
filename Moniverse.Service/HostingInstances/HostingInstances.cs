using Playverse.Data;
using Playverse.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using Utilities;
using Moniverse.Contract;

namespace Moniverse.Service
{    
    public class HostingInstances : ServiceClassBase
    {
        #region Configuration

        public HostingInstances() : base() { }
        public static HostingInstances Instance = new HostingInstances();

        // Playverse Monitoring Tables
        public virtual string HOSTING_INSTANCE_META_TABLE { get { return "HostingInstanceMeta"; } }

        //Playverse General Datastore Tables
        public virtual string HOSTING_INSTANCE_TABLE { get { return "HostingInstance"; } }
        public virtual string REGION_HOSTING_CONFIGURATION_TABLE { get { return "RegionHostingConfiguration"; } }
        public virtual string HOSTING_CONFIGURATION_TABLE { get { return "HostingConfiguration"; } }
        public virtual string HOSTING_REGION_TABLE { get { return "HostingRegion"; } }
        public virtual string GAME_VERSION_TABLE { get { return "GameVersion"; } }
        public virtual string GAME_SERVER_TABLE { get { return "GameServer"; } }
        public virtual string GAME_SESSION_TABLE { get { return "GameSession"; } }
        public virtual string GAME_SESSION_TYPE_TABLE { get { return "GameSessionType"; } }
        public virtual string GAME_SESSION_USER_TABLE { get { return "GameSessionUser"; } }

        private static ConcurrentDictionary<string, DateTime> nhi_LastGameNotificationTime = new ConcurrentDictionary<string, DateTime>();
        #endregion

        #region Execution
        public void QueryAndBuildHostingInstanceMeta(DateTime ProcessDate) {
            Logger.Instance.Info("--------------------------------------");
            Logger.Instance.Info("Beginning Hosting Instance Minute Event");
            Logger.Instance.Info("--------------------------------------");
            Logger.Instance.Info("");
            List<HostingInstanceMeta> RunningGameSessions = QueryRunningHostingInstances(ProcessDate);

            //UpdateHostingInstanceMeta(RunningGameSessions, ProcessDate);

            InsertHostingInstanceMeta(RunningGameSessions, ProcessDate);
            Logger.Instance.Info("--------------------------------------");
            Logger.Instance.Info("Finished Hosting Instance Minute Event");
            Logger.Instance.Info("--------------------------------------");
            Logger.Instance.Info("");
        }
        public void CheckComputeHostingInstances(GameMonitoringConfig game)
        {
            DataTable Result = new DataTable();
            string query = String.Format(@"SELECT 0 as 'ID'
                ,'{0}'  AS RecordTimestamp
                ,HI.Id AS 'HostingInstance_Id'
                ,HI.MachineId AS 'HostingInstance_MachineId'
                ,HI.IP AS 'HostingInstance_IP'
                ,HI.CreationTime AS 'HostingInstance_CreationTime'
                ,HI.StartTime AS 'HostingInstance_StartTime'
                ,HI.LastUpdateTime AS 'HostingInstance_LastUpdateTime'
                ,HI.GameId AS 'HostingInstance_GameId'
                ,RHC.Id AS 'RegionHostingConfiguration_Id'
                ,RHC.MinimumNumInstances AS 'RegionHostingConfiguration_MinimumNumInstances'
                ,RHC.MaximumFreeInstances AS 'RegionHostingConfiguration_MaximumFreeInstances'
                ,HC.Name AS 'HostingConfiguration_Name'
                ,HR.Name AS 'HostingRegion_Name'
                ,HI.GameVersionId 'GameVersion_Id'
                ,GV.Major AS 'GameVersion_Major'
                ,GV.Minor AS 'GameVersion_Minor'
                ,GV.Status AS 'GameVersion_Status'
                ,GV.Label 'GameVersion_Label'
                ,HI.Status AS 'HostingInstance_Status'
                ,HI.Health AS 'HostingInstance_Health'
                ,HI.MaximumComputeUnits AS 'HostingInstance_MaximumComputeUnits'
                ,HI.TotalComputeUnits AS 'HostingInstance_TotalComputeUnits'
                ,IFNULL(CALC.TotalComputeUnits, 0) AS 'HostingInstance_CalcTotalComputeUnits'
                ,HI.ServersCount AS 'HostingInstance_ServersCount'
                ,IFNULL(CALC.ServersCount, 0) AS 'HostingInstance_CalcServersCount'
                ,IFNULL(ROUND(CALC.TotalComputeUnits/CALC.ServersCount, 2), 0) AS 'HostingInstance_AvgComputeUnitsAcrossServers'
                ,IFNULL(CALC.MaxUserCount, 0) AS 'HostingInstance_MaxUserCount'
                ,IFNULL(CALC.UserCount, 0) AS 'HostingInstance_UserCount'
                FROM HostingInstance HI
                INNER JOIN RegionHostingConfiguration RHC
	                ON HI.RegionConfigurationId = RHC.Id
                INNER JOIN HostingConfiguration  HC
	                ON RHC.HostingConfigurationId = HC.Id
                INNER JOIN HostingRegion HR
	                ON RHC.RegionId = HR.Id
                LEFT JOIN GameVersion GV
	                ON HI.GameVersionId = GV.Id
                LEFT JOIN (
	                SELECT GSr.InstanceId, SUM(GSr.ComputeUnits) AS 'TotalComputeUnits', COUNT(*) AS 'ServersCount', SUM(GS.MaxNumPlayers) AS 'MaxUserCount', SUM(GS.UserCount) AS 'UserCount'
	                FROM GameServer GSr
	                INNER JOIN 
	                (
		                SELECT GSn.Id, COUNT(*) AS 'UserCount', GST.MaxNumPlayers
		                FROM GameSession GSn
                        INNER JOIN GameSessionType GST
			                ON GSn.SessionTypeId = GST.Id
		                LEFT JOIN GameSessionUser GSU
			                ON GSn.Id = GSU.GameSessionId
		                WHERE GSU.Status = 2
		                GROUP BY GSn.Id
	                ) GS
	                ON GSr.GameSessionId = GS.Id
	                GROUP BY GSr.InstanceId
                ) CALC
	                ON HI.Id = CALC.InstanceId
                WHERE HI.IsLocallyEmulated = 0
                AND HI.IsPhysicallyHosted = 0
                AND HI.GameID = '{1}'
                ORDER BY HI.GameId, HR.Name, HI.Status, HI.Id;",
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), game.Id);

            try
            {

                Logger.Instance.Info("--------------------------------------");
                Logger.Instance.Info("Beginning Hosting Instance Query");
                Logger.Instance.Info("--------------------------------------");
                Logger.Instance.Info("");
                Result = DBManager.Instance.Query(Datastore.General, query);
                Logger.Instance.Info(String.Format("Succesfully retrieved Hosting Instances  : {0}", Result.Rows.Count));
                if (Result.Rows.Count > 0)
                {
                    List<string> InsertStatements = new List<string>();
                    foreach (DataRow row in Result.Rows)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(@"INSERT INTO `HostingInstance_ComputeRaw`
                    (`ID`,
                    `RecordTimestamp`,
                    `HostingInstance_Id`,
                    `HostingInstance_MachineId`,
                    `HostingInstance_IP`,
                    `HostingInstance_CreationTime`,
                    `HostingInstance_StartTime`,
                    `HostingInstance_LastUpdateTime`,
                    `HostingInstance_GameId`,
                    `RegionHostingConfiguration_Id`,
                    `RegionHostingConfiguration_MinimumNumInstances`,
                    `RegionHostingConfiguration_MaximumFreeInstances`,
                    `HostingConfiguration_Name`,
                    `HostingRegion_Name`,
                    `GameVersion_Id`,
                    `GameVersion_Major`,
                    `GameVersion_Minor`,
                    `GameVersion_Status`,
                    `GameVersion_Label`,
                    `HostingInstance_Status`,
                    `HostingInstance_Health`,
                    `HostingInstance_MaximumComputeUnits`,
                    `HostingInstance_TotalComputeUnits`,
                    `HostingInstance_CalcTotalComputeUnits`,
                    `HostingInstance_ServersCount`,
                    `HostingInstance_CalcServersCount`,
                    `HostingInstance_AvgComputeUnitsAcrossServers`,
                    `HostingInstance_MaxUserCount`,
                    `HostingInstance_UserCount`) VALUES (");
                        foreach (var item in row.ItemArray)
                        {
                            if (item is DateTime)
                            {
                                DateTime itm = (DateTime)item;
                                sb.Append("'" + itm.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                                sb.Append(",");
                            }
                            else
                            {
                                sb.Append("'" + item.ToString() + "'");
                                sb.Append(",");
                            }

                        }
                        sb.Length--;
                        sb.Append(");");
                        string Insert = sb.ToString();
                        InsertStatements.Add(Insert);

                    }
                    try
                    {
                        MoniverseResponse response = new MoniverseResponse()
                        {
                            Status = "unsent",
                            TimeStamp = DateTime.UtcNow
                        };
                        
                        string shittyWay = string.Join("", InsertStatements);
                        lock (MoniverseBase.ConsoleWriterLock)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Logger.Instance.Info("--------------------------------------");
                            Logger.Instance.Info("Beginning Insert of Retention Row Batch");
                            Logger.Instance.Info("--------------------------------------");
                            Logger.Instance.Info("");
                        }
                        HostingInstances.Service(Service => {
                            Service.Insert(new MoniverseRequest()
                            {
                                TaskName = "Insert",
                                Task = shittyWay,
                                TimeStamp = DateTime.UtcNow
                            });                        
                        });
                        //int result = DBManager.Instance.Insert(Datastore.Monitoring, shittyWay);
                        lock (MoniverseBase.ConsoleWriterLock)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Logger.Instance.Info("--------------------------------------");
                            Logger.Instance.Info("Beginning Insert of Retention Row Batch");
                            Logger.Instance.Info("--------------------------------------");
                            Logger.Instance.Info("");
                            Logger.Instance.Info(String.Format("success!! {0} inserted", InsertStatements.Count));
                            Console.ResetColor();
                        }


                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Logger.Instance.Info(e.Message);
                        Console.ResetColor();
                    }
                }


            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }
        }
        #endregion

        public List<HostingInstanceMeta> QueryRunningHostingInstances(DateTime ProcessDate) {
            List<HostingInstanceMeta> HostingInstances = new List<HostingInstanceMeta>();
            DataTable Result = new DataTable();
            string query = String.Format(@"SELECT 0 as 'ID'
                ,'{0}'  AS RecordTimestamp
                ,HI.Id AS 'Id'
                ,HI.MachineId AS 'MachineID'
                ,HI.IP AS 'IP'
                ,HI.CreationTime AS 'CreationTime'
                ,HI.StartTime AS 'StartTime'
                ,HI.LastUpdateTime AS 'LastUpdateTime'
                ,HI.GameId AS 'GameId'
                ,RHC.Id AS 'RegionHostingConfigurationId'
                ,RHC.MinimumNumInstances AS 'MinimumNumInstances'
                ,RHC.MaximumFreeInstances AS 'MaximumFreeInstances'
                ,HC.Name AS 'HostingConfigurationName'
                ,HR.Name AS 'HostingRegionName'
                ,HI.GameVersionId 'GameVersionId'
                ,GV.Major AS 'GameVersionMajor'
                ,GV.Minor AS 'GameVersionMinor'
                ,GV.Status AS 'GameVersionStatus'
                ,GV.Label 'GameVersionLabel'
                ,HI.Status AS 'Status'
                ,HI.Health AS 'Health'
                ,HI.MaximumComputeUnits AS 'MaximumComputeUnits'
                ,HI.TotalComputeUnits AS 'TotalComputeUnits'
                ,IFNULL(CALC.TotalComputeUnits, 0) AS 'CalcTotalComputeUnits'
                ,HI.ServersCount AS 'ServersCount'
                ,IFNULL(CALC.ServersCount, 0) AS 'CalcServersCount'
                ,IFNULL(ROUND(CALC.TotalComputeUnits/CALC.ServersCount, 2), 0) AS 'AvgComputeUnitsAcrossServers'
                ,IFNULL(CALC.MaxUserCount, 0) AS 'MaxUserCount'
                ,IFNULL(CALC.UserCount, 0) AS 'UserCount'
                FROM {1} HI
                INNER JOIN {2} RHC
	                ON HI.RegionConfigurationId = RHC.Id
                INNER JOIN {3}  HC
	                ON RHC.HostingConfigurationId = HC.Id
                INNER JOIN {4} HR
	                ON RHC.RegionId = HR.Id
                LEFT JOIN {5} GV
	                ON HI.GameVersionId = GV.Id
                LEFT JOIN (
	                SELECT GSr.InstanceId, SUM(GSr.ComputeUnits) AS 'TotalComputeUnits', COUNT(*) AS 'ServersCount', SUM(GS.MaxNumPlayers) AS 'MaxUserCount', SUM(GS.UserCount) AS 'UserCount'
	                FROM {6} GSr
	                INNER JOIN 
	                (
		                SELECT GSn.Id, COUNT(*) AS 'UserCount', GST.MaxNumPlayers
		                FROM {7} GSn
                        INNER JOIN {8} GST
			                ON GSn.SessionTypeId = GST.Id
		                LEFT JOIN {9} GSU
			                ON GSn.Id = GSU.GameSessionId
		                WHERE GSU.Status = 2
		                GROUP BY GSn.Id
	                ) GS
	                ON GSr.GameSessionId = GS.Id
	                GROUP BY GSr.InstanceId
                ) CALC
	                ON HI.Id = CALC.InstanceId
                WHERE HI.IsLocallyEmulated = 0
                AND HI.IsPhysicallyHosted = 0
                ORDER BY HI.GameId, HR.Name, HI.Status, HI.Id;",
                ProcessDate.ToString("yyyy-MM-dd HH:mm:ss"), 
                HOSTING_INSTANCE_TABLE, 
                REGION_HOSTING_CONFIGURATION_TABLE, 
                HOSTING_CONFIGURATION_TABLE, 
                HOSTING_REGION_TABLE, 
                GAME_VERSION_TABLE, 
                GAME_SERVER_TABLE, 
                GAME_SESSION_TABLE, 
                GAME_SESSION_TYPE_TABLE, 
                GAME_SESSION_USER_TABLE);

            try
            {

                Logger.Instance.Info("--------------------------------------");
                Logger.Instance.Info("Beginning Hosting Instance Query");
                Logger.Instance.Info("--------------------------------------");
                Logger.Instance.Info("");
                Result = DBManager.Instance.Query(Datastore.General, query);
                Logger.Instance.Info("");

            }catch (Exception ex) {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }
            if (Result.Rows.Count > 0) {
                foreach (DataRow Row in Result.Rows)
                {
                    HostingInstanceMeta HostingInstanceMetaRow = new HostingInstanceMeta() {
                        RecordTimestamp = DateTime.UtcNow,
                        Id = Row["Id"].ToString(),
                        MachineID = Row["MachineId"].ToString(),
                        IP = Row["IP"].ToString(),
                        CreationTime = DateTime.Parse(Row["CreationTime"].ToString()),
                        StartTime = DateTime.Parse(Row["StartTime"].ToString()),
                        LastUpdateTime = DateTime.Parse(Row["LastUpdateTime"].ToString()),
                        GameId = Row["GameId"].ToString(),
                        RegionHostingConfigurationId = Row["RegionHostingConfigurationId"].ToString(),
                        MinimumNumInstances = Convert.ToInt32(Row["MinimumNumInstances"]),
                        MaximumFreeInstances = Convert.ToInt32(Row["MaximumFreeInstances"]),
                        HostingConfigurationName = Row["HostingConfigurationName"].ToString(),
                        HostingRegionName = Row["HostingRegionName"].ToString(),
                        GameVersionId = Row["GameVersionId"].ToString(),
                        GameVersionMajor = Convert.ToInt32(Row["GameVersionMajor"]),
                        GameVersionMinor = Convert.ToInt32(Row["GameVersionMinor"]),
                        GameVersionStatus = Convert.ToInt32(Row["GameVersionStatus"]),
                        GameVersionLabel = Convert.ToInt32(Row["GameVersionLabel"]),
                        Status = Convert.ToInt32(Row["Status"]),
                        Health = Convert.ToInt32(Row["Health"]),
                        MaximumComputeUnits = Convert.ToInt32(Row["MaximumComputeUnits"]),
                        TotalComputeUnits = Convert.ToInt32(Row["TotalComputeUnits"]),
                        CalcTotalComputeUnits = Convert.ToInt32(Row["CalcTotalComputeUnits"]),
                        ServersCount = Convert.ToInt32(Row["ServersCount"]),
                        CalcServersCount = Convert.ToInt32(Row["CalcServersCount"]),
                        AvgComputeUnitsAcrossServers = Convert.ToInt32(Row["AvgComputeUnitsAcrossServers"]),
                        MaxUserCount = Convert.ToInt32(Row["MaxUserCount"]),
                        UserCount = Convert.ToInt32(Row["UserCount"]),
                    };
                    HostingInstances.Add(HostingInstanceMetaRow);
                }            
            }

            Logger.Instance.Info(String.Format("Succesfully retrieved Hosting Instances  : {0}", HostingInstances.Count));
            Logger.Instance.Info("");

            return HostingInstances;
        }

        //public void UpdateHostingInstanceMeta(List<HostingInstanceMeta> CurrentHostingInstances, DateTime ProcessDate)
        //{
        //    List<GameMonitoringConfig> games = Games.Instance.GetMonitoredGames();
        //    Logger.Instance.Info("--------------------------------------");
        //    Logger.Instance.Info("Beginning Update of Active Sessions (All Games)");
        //    Logger.Instance.Info("--------------------------------------");
        //    Logger.Instance.Info("");
        //    //Logger.Instance.Info(String.Format("Beginning {0}", game.Title));
        //    int RunningGameCounter = 0;
        //    //List<GameSessionUpdateInfo> CurrentRecords = QueryGameSessionMeta(game.Id, ProcessDate);
        //    string query = String.Format("SELECT * FROM {0} WHERE DATE(recordCreated) = '{1}'", GAME_SESSION_META_TABLE, ProcessDate.ToString("yyyy-MM-dd"));
        //    List<GameSessionUpdateInfo> GameSessionList = new List<GameSessionUpdateInfo>();

        //    DataTable GameSessionMetaTable = DBManager.Instance.Query(Datastore.Monitoring, query);

        //    if (GameSessionMetaTable.Rows.Count > 0)
        //    {
        //        List<string> statements = new List<string>();
        //        foreach (DataRow row in GameSessionMetaTable.Rows)
        //        {
        //            GameSessionUpdateInfo GameSessionMetaRow = new GameSessionUpdateInfo()
        //            {
        //                RecordCreated = DateTime.Parse(row["RecordCreated"].ToString()),
        //                RecordLastUpdateTime = DateTime.Parse(row["RecordLastUpdateTime"].ToString()),
        //                GameSessionId = row["Id"].ToString(),
        //                IP = row["IP"].ToString(),
        //                Port = Convert.ToInt32(row["Port"]),
        //                CreationTime = DateTime.Parse(row["CreationTime"].ToString()),
        //                SessionStarted = Convert.ToBoolean(row["SessionStarted"]),
        //                GameId = row["GameId"].ToString(),
        //                IsLocallyEmulated = Convert.ToBoolean(row["IsLocallyEmulated"]),
        //                LastUpdateTime = DateTime.Parse(row["LastUpdateTime"].ToString()),
        //                Major = Convert.ToInt32(row["Major"]),
        //                Minor = Convert.ToInt32(row["Minor"]),
        //                UsersCount = Convert.ToInt32(row["UsersCount"]),
        //                SessionTypeId = row["SessionTypeId"].ToString(),
        //                SessionTypeFriendly = row["SessionTypeFriendly"].ToString(),
        //                Status = Convert.ToInt32(row["Status"]),
        //                CurrentRanking = Convert.ToInt32(row["CurrentRanking"]),
        //                IsPartySession = Convert.ToBoolean(row["IsPartySession"]),
        //                InitiatorUserId = row["InitiatorUserId"].ToString(),
        //                IsHosted = Convert.ToBoolean(row["IsHosted"]),
        //                SessionMetadata = row["SessionMetadata"].ToString()
        //            };

        //            string UpdateStatement = String.Format("UPDATE {0} SET RecordLastUpdateTime = '{1}', Status = {2}, IP = '{4}', Port = {5}, LastUpdateTime = '{6}', SessionStarted = {7} WHERE GameSessionId = '{3}' and GameId = '{7}'; ",
        //                HOSTING_INSTANCE_META_TABLE, DateTime.UtcNow, GameSessionMetaRow.Status, GameSessionMetaRow.GameSessionId, GameSessionMetaRow.IP, GameSessionMetaRow.Port, GameSessionMetaRow.LastUpdateTime, GameSessionMetaRow.SessionStarted, GameSessionMetaRow.GameId);

        //            statements.Add(UpdateStatement);

        //            CurrentGameSessions.Remove(GameSessionMetaRow);
        //            RunningGameCounter++;
        //        }
        //        try
        //        {
        //            Logger.Instance.Info(String.Format("updating {0} statements", statements.Count));

        //            DBManager.Instance.Update(Datastore.Monitoring, statements);
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.Instance.Exception(ex.Message, ex.StackTrace);
        //            Retry.Do(() =>
        //            {
        //                Logger.Instance.Info(String.Format("retry {0} statements", statements.Count));

        //                DBManager.Instance.Update(Datastore.Monitoring, statements);
        //            }, TimeSpan.FromSeconds(10), 5);
        //        }

        //    }
        //    Logger.Instance.Info(String.Format("Updated {1} running sessions", RunningGameCounter));
        //}

        public void InsertHostingInstanceMeta(List<HostingInstanceMeta> CurrentHostingInstances, DateTime ProcessDate)
        {
            //there aren't currently going to be this many instances running, but this at least forces an upper limit to a batch size by convention
            // only 500 will ever be processed at once, so that's a thing -- PJ
            foreach (List<HostingInstanceMeta> Batch in CurrentHostingInstances.Batch<HostingInstanceMeta>(500))
            {
                string InsertStatement = String.Format(@"INSERT INTO {0}
                                                                (`RecordTimestamp`,
                                                                `HostingInstance_Id`,
                                                                `HostingInstance_MachineId`,
                                                                `HostingInstance_IP`,
                                                                `HostingInstance_CreationTime`,
                                                                `HostingInstance_StartTime`,
                                                                `HostingInstance_LastUpdateTime`,
                                                                `HostingInstance_GameId`,
                                                                `RegionHostingConfiguration_Id`,
                                                                `RegionHostingConfiguration_MinimumNumInstances`,
                                                                `RegionHostingConfiguration_MaximumFreeInstances`,
                                                                `HostingConfiguration_Name`,
                                                                `HostingRegion_Name`,
                                                                `GameVersion_Id`,
                                                                `GameVersion_Major`,
                                                                `GameVersion_Minor`,
                                                                `GameVersion_Status`,
                                                                `GameVersion_Label`,
                                                                `HostingInstance_Status`,
                                                                `HostingInstance_Health`,
                                                                `HostingInstance_MaximumComputeUnits`,
                                                                `HostingInstance_TotalComputeUnits`,
                                                                `HostingInstance_CalcTotalComputeUnits`,
                                                                `HostingInstance_ServersCount`,
                                                                `HostingInstance_CalcServersCount`,
                                                                `HostingInstance_AvgComputeUnitsAcrossServers`,
                                                                `HostingInstance_MaxUserCount`,
                                                                `HostingInstance_UserCount`) VALUES {1}", HOSTING_INSTANCE_META_TABLE, DatabaseUtilities.instance.GenerateInsertValues<HostingInstanceMeta>(Batch));
                try
                {
                    MoniverseResponse response = new MoniverseResponse()
                    {
                        Status = "unsent",
                        TimeStamp = DateTime.UtcNow
                    };
                    
                    lock (MoniverseBase.ConsoleWriterLock)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("Beginning Insert of Hosting Instances Batch");
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("");
                    }

                    HostingInstances.Service(Service => Service.Insert(new MoniverseRequest()
                    {
                        TaskName = "Hosting Instance Insert",
                        Task = InsertStatement,
                        TimeStamp = DateTime.UtcNow

                    }));

                    lock (MoniverseBase.ConsoleWriterLock)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("Hosting Instance Batch Success");
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info(String.Format("Inserted {0} Hosting Instances", Batch.Count));
                        Logger.Instance.Info(String.Format("success!! {0}", response.Status));
                        Logger.Instance.Info("");
                        Console.ResetColor();
                    }

                }
                catch (Exception ex)
                {
                    Logger.Instance.Exception(ex.Message, ex.StackTrace);
                }
            }

        }

        public void CheckNumberHostingInstances(GameMonitoringConfig game)
        {
            Logger.Instance.Info("Checking number of hosting instances...");
            int result = 0;

            string query = String.Format(
                @"SELECT COUNT(*) AS Count
                FROM HostingInstance
                WHERE GameId = '{0}';",
                game.Id);

            try
            {
                result = DBManager.Instance.QueryForCount(Datastore.General, query);

                Logger.Instance.Info(String.Format("Got result... {0}", result));

                if (game.MaxRunningHostingInstances < result)
                {
                    // Set up notification tracking for game
                    if (!nhi_LastGameNotificationTime.ContainsKey(game.Id))
                    {
                        nhi_LastGameNotificationTime.TryAdd(game.Id, DateTime.UtcNow.AddDays(-1));
                    }

                    // Update internal notification info for game
                    DateTime lastNotificationTime = nhi_LastGameNotificationTime[game.Id];

                    nhi_LastGameNotificationTime[game.Id] = lastNotificationTime;

                    // Send notification
                    if (true)
                    {
                        string subject = String.Format("Game: {0} | Hosting Instance Check - # Instances {1}/{2}", game.ShortTitle, result, game.MaxRunningHostingInstances);
                        string message = "[{0} Alert {1}] - Game: {2} | The number of Hosting Instances is at {3}, which is above the configured max threshold of {4}.";

                        Reasoning reason = new Reasoning()
                        {
                            OriginMethod = "CheckNumberHostingInstances",
                            Condition = "if (game.MaxRunningHostingInstances < result)",
                            Expected = game.MaxRunningHostingInstances.ToString(),
                            Actual = result.ToString()
                        };                        
                        //Notifications.Instance.SendNotification(NotificationLevel.Error, subject, String.Format(message, NotificationLevel.Error.ToString(), DateTime.UtcNow, game.Title, result, game.MaxRunningHostingInstances));
                    }
                }

                Logger.Instance.Info("Done with checking hosting instances");
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }
        }
    }
}
