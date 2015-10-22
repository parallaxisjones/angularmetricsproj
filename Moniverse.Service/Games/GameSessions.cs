using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Playverse.Data;
using Playverse.Utilities;
using System.Reflection;
using System.Diagnostics;
using Utilities;
using Moniverse.Contract;
using Moniverse.Service;

namespace Moniverse.Service
{

    public class GameSessions : ServiceClassBase
    {
        #region Configuration
        public virtual string AVERAGE_SESSIONLENGTH_TABLE { get { return "AverageSessionLength"; } }
        public virtual string GAME_SESSION_META_TABLE { get { return "GameSessionMeta"; } }
        public virtual string GAME_SESSION_TABLE { get { return "GameSession"; } }
        public virtual string GAME_SESSION_TYPE_TABLE { get { return "GameSessionType"; } }

        public GameSessions() : base(){}

        public static GameSessions instance = new GameSessions();
        #endregion   


        #region Execution
        public void QueryAndBuildGameSessionMeta(DateTime ProcessDate) {
            Logger.Instance.Info("--------------------------------------");
            Logger.Instance.Info("Beginning Game Session Query");
            Logger.Instance.Info("--------------------------------------");
            Logger.Instance.Info("");
            List<GameSessionMeta> RunningGameSessions = QueryRunningGameSessions();
            Logger.Instance.Info(String.Format("Successfully retrieved {0} Total Live Game Sessions from Playverse", RunningGameSessions.Count));
                    
            UpdateGameSessionMeta(RunningGameSessions, ProcessDate);
            Logger.Instance.Info(String.Format("{0} Total Game Sessions After Update", RunningGameSessions.Count));
            InsertGameSessionMeta(RunningGameSessions);
            Logger.Instance.Info(String.Format("{0} Total New Game Sessions Inserted", RunningGameSessions.Count));
        }
        #endregion

        #region CRUD
        public List<GameSessionMeta> QueryRunningGameSessions()
        {

            List<GameSessionMeta> SessionList = new List<GameSessionMeta>();
            DataTable GameSessionsTable = DBManager.Instance.Query(Datastore.General, GetGameSessionsQueryStr());
            if (GameSessionsTable.Rows.Count > 0)
            {

                foreach (DataRow row in GameSessionsTable.Rows)
                {
                    GameSessionMeta GameSessionMetaRow = new GameSessionMeta()
                    {
                        RecordCreated = DateTime.UtcNow,
                        RecordLastUpdateTime = DateTime.UtcNow,
                        GameSessionId = row["Id"].ToString(),
                        IP = row["IP"].ToString(),
                        Port = Convert.ToInt32(row["Port"].ToString()),
                        CreationTime = DateTime.Parse(row["CreationTime"].ToString()),
                        SessionStarted = Convert.ToBoolean(row["SessionStarted"]),
                        GameId = row["GameId"].ToString(),
                        IsLocallyEmulated = Convert.ToBoolean(row["IsLocallyEmulated"]),
                        LastUpdateTime = DateTime.Parse(row["LastUpdateTime"].ToString()),
                        Major = Convert.ToInt32(row["Major"]),
                        Minor = Convert.ToInt32(row["Minor"]),
                        UsersCount = Convert.ToInt32(row["UsersCount"]),
                        SessionTypeId = row["SessionTypeId"].ToString(),
                        SessionTypeFriendly = row["SessionTypeFriendly"].ToString(),
                        Status = Convert.ToInt32(row["Status"]),
                        CurrentRanking = Convert.ToInt32(row["CurrentRanking"]),
                        IsPartySession = Convert.ToBoolean(row["IsPartySession"]),
                        InitiatorUserId = row["InitiatorUserId"].ToString(),
                        IsHosted = Convert.ToBoolean(row["IsHosted"]),
                        SessionMetadata = row["SessionMetadata"].ToString()
                    };

                    SessionList.Add(GameSessionMetaRow);
                }
            }
            return SessionList;
        }
        public List<GameSessionMeta> QueryGameSessionMeta(DateTime ProcessDate)
        {
            string query = String.Format("SELECT * FROM {0} WHERE recordCreated = '{1}';", GAME_SESSION_META_TABLE, ProcessDate.ToString("yyyy-MM-dd HH:mm:ss"));
            List<GameSessionMeta> GameSessionList = new List<GameSessionMeta>();
            DataTable GameSessionMetaTable = new DataTable();
            try
            {
                GameSessionMetaTable = DBManager.Instance.Query(Datastore.Monitoring, query);
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
                //throw;
            }


            if (GameSessionMetaTable.Rows.Count > 0)
            {
                foreach (DataRow row in GameSessionMetaTable.Rows)
                {
                    GameSessionMeta GameSessionMetaRow = new GameSessionMeta()
                    {
                        RecordCreated = DateTime.Parse(row["RecordCreated"].ToString()),
                        RecordLastUpdateTime = DateTime.Parse(row["RecordLastUpdateTime"].ToString()),
                        GameSessionId = row["Id"].ToString(),
                        IP = row["IP"].ToString(),
                        Port = Convert.ToInt32(row["Port"]),
                        CreationTime = DateTime.Parse(row["CreationTime"].ToString()),
                        SessionStarted = (bool)row["SessionStarted"],
                        GameId = row["GameId"].ToString(),
                        IsLocallyEmulated = (bool)row["IsLocallyEmulated"],
                        LastUpdateTime = DateTime.Parse(row["LastUpdateTime"].ToString()),
                        Major = Convert.ToInt32(row["Major"]),
                        Minor = Convert.ToInt32(row["Minor"]),
                        UsersCount = Convert.ToInt32(row["UsersCount"]),
                        SessionTypeId = row["SessionTypeId"].ToString(),
                        SessionTypeFriendly = row["SessionTypeFriendly"].ToString(),
                        Status = Convert.ToInt32(row["Status"]),
                        CurrentRanking = Convert.ToInt32(row["CurrentRanking"]),
                        IsPartySession = (bool)row["IsPartySession"],
                        InitiatorUserId = row["InitiatorUserId"].ToString(),
                        IsHosted = (bool)row["IsHosted"],
                        SessionMetadata = row["SessionMetadata"].ToString()
                    };

                    GameSessionList.Add(GameSessionMetaRow);
                }
            }
            return GameSessionList;

        }
        public void InsertGameSessionMeta(List<GameSessionMeta> CurrentGameSessions)
        {
            List<string> InsertStatements = new List<string>();
            foreach (List<GameSessionMeta> Batch in CurrentGameSessions.Batch<GameSessionMeta>(500))
            {
                MoniverseResponse response = new MoniverseResponse() { 
                    Status = "unsent",
                    TimeStamp = DateTime.UtcNow
                };

                string InsertStatement = String.Format(@"INSERT INTO {0}
                                                                (`RecordCreated`,
                                                                `RecordLastUpdateTime`,
                                                                `GameSessionId`,
                                                                `IP`,
                                                                `Port`,
                                                                `CreationTime`,
                                                                `SessionStarted`,
                                                                `GameId`,
                                                                `IsLocallyEmulated`,
                                                                `LastUpdateTime`,
                                                                `Major`,
                                                                `Minor`,
                                                                `UsersCount`,
                                                                `SessionTypeId`,
                                                                `SessionTypeFriendly`,
                                                                `Status`,
                                                                `CurrentRanking`,
                                                                `IsPartySession`,
                                                                `GameSessionRankRangeMin`,
                                                                `GameSessionRankRangeMax`,
                                                                `StartLaunchCountDownTime`,
                                                                `IsPrivateSession`,
                                                                `InitiatorUserId`,
                                                                `IsHosted`,
                                                                `SessionMetadata`) VALUES {1} ON DUPLICATE KEY UPDATE RecordLastUpdateTime = '{2}'", GAME_SESSION_META_TABLE, DatabaseUtilities.instance.GenerateInsertValues<GameSessionMeta>(Batch), DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                try
                {
                    lock (MoniverseBase.ConsoleWriterLock)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("Beginning Insert of Game Sessions Batch");
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("");
                    }
                    GameSessions.Service(Service => {
                        response = Service.Insert(new MoniverseRequest()
                        {
                            TaskName = "Game Sessions Batch Insert",
                            Task = InsertStatement,
                            TimeStamp = DateTime.UtcNow
                        });                    
                    });
                    //int result = DBManager.Instance.Insert(Datastore.Monitoring, InsertStatement);



                    lock (MoniverseBase.ConsoleWriterLock)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("Game Sessions Batch Success");
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("");
                        Logger.Instance.Info(String.Format("success!! {0} inserted", Batch.Count()));
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
        public void UpdateGameSessionMeta(List<GameSessionMeta> CurrentGameSessions, DateTime ProcessDate)
        {
            List<GameMonitoringConfig> games = Games.Instance.GetMonitoredGames();

                //Logger.Instance.Info(String.Format("Beginning {0}", game.Title));
                int RunningGameCounter = 0;
                //List<GameSessionUpdateInfo> CurrentRecords = QueryGameSessionMeta(ProcessDate);
                string query = String.Format("SELECT * FROM {0} WHERE DATE(recordCreated) = '{1}' AND Status <> 2;", GAME_SESSION_META_TABLE, ProcessDate.ToString("yyyy-MM-dd"));
                Logger.Instance.Info(query);
//#if DEBUG
//                Debugger.Launch();
//#endif
                List<GameSessionMeta> GameSessionList = new List<GameSessionMeta>();

                DataTable GameSessionMetaTable = DBManager.Instance.Query(Datastore.Monitoring, query);

                if (GameSessionMetaTable.Rows.Count > 0)
                {
                    List<string> statements = new List<string>();
                    foreach (DataRow row in GameSessionMetaTable.Rows)
                    {
                        GameSessionMeta GameSessionMetaRow = new GameSessionMeta()
                        {
                            RecordCreated = DateTime.Parse(row["RecordCreated"].ToString()),
                            RecordLastUpdateTime = DateTime.Parse(row["RecordLastUpdateTime"].ToString()),
                            GameSessionId = row["GameSessionId"].ToString(),
                            IP = row["IP"].ToString(),
                            Port = Convert.ToInt32(row["Port"]),
                            CreationTime = DateTime.Parse(row["CreationTime"].ToString()),
                            SessionStarted = Convert.ToBoolean(row["SessionStarted"]),
                            GameId = row["GameId"].ToString(),
                            IsLocallyEmulated = Convert.ToBoolean(row["IsLocallyEmulated"]),
                            LastUpdateTime = DateTime.Parse(row["LastUpdateTime"].ToString()),
                            Major = Convert.ToInt32(row["Major"]),
                            Minor = Convert.ToInt32(row["Minor"]),
                            UsersCount = Convert.ToInt32(row["UsersCount"]),
                            SessionTypeId = row["SessionTypeId"].ToString(),
                            SessionTypeFriendly = row["SessionTypeFriendly"].ToString(),
                            Status = Convert.ToInt32(row["Status"]),
                            CurrentRanking = Convert.ToInt32(row["CurrentRanking"]),
                            IsPartySession = Convert.ToBoolean(row["IsPartySession"]),
                            InitiatorUserId = row["InitiatorUserId"].ToString(),
                            IsHosted = Convert.ToBoolean(row["IsHosted"]),
                            SessionMetadata = row["SessionMetadata"].ToString()
                        };

                        string UpdateStatement = String.Format("UPDATE {0} SET RecordLastUpdateTime = '{1}', Status = {2}, IP = '{4}', Port = {5}, LastUpdateTime = '{6}', SessionStarted = {7} WHERE GameSessionId = '{3}'; ", 
                            GAME_SESSION_META_TABLE, 
                            DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), 
                            GameSessionMetaRow.Status, 
                            GameSessionMetaRow.GameSessionId, 
                            GameSessionMetaRow.IP, 
                            GameSessionMetaRow.Port,
                            GameSessionMetaRow.LastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss"), 
                            (GameSessionMetaRow.SessionStarted == true) ? 1 : 0, 
                            GameSessionMetaRow.GameId);
                        //Logger.Instance.Info(UpdateStatement);


                        statements.Add(UpdateStatement);
                        
                        CurrentGameSessions.Remove(GameSessionMetaRow);
                        RunningGameCounter++;
                    }
                    try
                    {
                        Logger.Instance.Info(String.Format("updating {0} statements in {1} batches", statements.Count, statements.Count / 100));
                        foreach (List<string> updateBatch in statements.Batch<string>(1000)) {
                            MoniverseResponse response = new MoniverseResponse()
                            {
                                Status = "unsent",
                                TimeStamp = DateTime.UtcNow
                            };

                            lock (MoniverseBase.ConsoleWriterLock)
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Logger.Instance.Info("-----------------------------------------------");
                                Logger.Instance.Info("Beginning Update of Active Sessions (All Games)");
                                Logger.Instance.Info("-----------------------------------------------");
                                Logger.Instance.Info("");
                            }


                            GameSessions.Service(Service =>
                            {
                                Service.Update(new UpdateRequest()
                                {
                                    TaskName = "UpdateGameSessionMeta Update",
                                    Task = updateBatch,
                                    TimeStamp = DateTime.UtcNow
                                });
                            });

                            //int result = DBManager.Instance.Update(Datastore.Monitoring, statements);
                            lock (MoniverseBase.ConsoleWriterLock)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Logger.Instance.Info("--------------------------------------");
                                Logger.Instance.Info("Active Game Session Update Batch Success");
                                Logger.Instance.Info("--------------------------------------");
                                Console.ResetColor();
                            }                        
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
        #endregion

        #region GetQueryStrings
        private string GetGameSessionsQueryStr()
        {

            return String.Format(
            @"SELECT GS.`Id`,
            GS.`IP`,
            GS.`Port`,
            GS.`CreationTime`,
            GS.`SessionStarted`,
            GS.`GameId`,
            GS.`IsLocallyEmulated`,
            GS.`LastUpdateTime`,
            GS.`Major`,
            GS.`Minor`,
            GS.`UsersCount`,
            GS.`SessionTypeId`,
            GST.`FriendlyName` as SessionTypeFriendly,
            GS.`Status`,
            GS.`CurrentRanking`,
            GS.`IsPartySession`,
            GS.`GameSessionRankRangeMin`,
            GS.`GameSessionRankRangeMax`,
            GS.`StartLaunchCountDownTime`,
            GS.`IsPrivateSession`,
            GS.`InitiatorUserId`,
            GS.`IsHosted`,
            GS.`SessionMetadata`
		FROM {1} as GS
        LEFT JOIN {0} as GST
        ON GS.`SessionTypeId` = GST.`Id` 
        AND GS.`GameId` = GST.`GameId`;", GAME_SESSION_TYPE_TABLE, GAME_SESSION_TABLE);
        }
        #endregion
    }
}
