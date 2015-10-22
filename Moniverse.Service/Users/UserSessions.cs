using Keen.Core.Query;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Playverse.Data;
using Playverse.Utilities;
using System.Reflection;
using System.Data;
using Utilities;
using Moniverse.Contract;
using Moniverse.Service;
using System.ServiceModel;
using System.Threading;
using System.Globalization;
namespace Moniverse.Service
{
    public class UserSessions : ServiceClassBase
    {
        #region Configuration
        protected IAnalyticsProvider provider = KeenIO.Instance;
        protected virtual string USER_SESSION_META_TABLE { get { return "UserSessionMeta"; } }

        public UserSessions() : base() { }
        public static UserSessions Instance = new UserSessions();
        #endregion

        #region ResourceGathering

        public HashSet<Login> ExtractLatestLogins(DateTime LastRecord)
        {
            HashSet<Login> Logins = new HashSet<Login>();

            DateTime end = DateTime.UtcNow;
            DateTime start = LastRecord;
            var result = provider.GetResource("Login", start, end);

            if (result.Any())
            {
                foreach (JObject entry in result)
                {
                    DateTime loginTime = DateTime.Parse(entry["keen"]["timestamp"].ToString());
                    bool isNewUser;
                    var isNewUserProperty = entry.Property("isnewuser");
                    if (isNewUserProperty != null)
                    {
                        try
                        {
                            isNewUser = Convert.ToBoolean(entry["isnewuser"].ToString());
                        }
                        catch (Exception ex)
                        {
                            isNewUser = false;
                        }
                        Login u = new Login()
                        {
                            UserId = entry["userid"].ToString(),
                            GameId = entry["gameid"].ToString(),
                            UserSessionId = entry["sessionid"].ToString(),
                            Platform = entry["platform"].ToString(),
                            LoginTimestamp = loginTime,
                            City = entry["location"]["city"].ToString(),
                            Country = entry["location"]["country"].ToString(),
                            Region = entry["location"]["region"].ToString(),
                            Longitude = float.Parse(entry["location"]["longitude"].ToString(), CultureInfo.InvariantCulture.NumberFormat),
                            Latitude = float.Parse(entry["location"]["latitude"].ToString(), CultureInfo.InvariantCulture.NumberFormat),
                            LocationId = Convert.ToInt32(entry["location"]["id"].ToString()),
                            InstallDateRecord = (isNewUser) ? 1 : 0,
                            SessionLength = -1,
                            RecordDate = loginTime.ToString("yyyy-MM-dd")
                        };

                        u.RetentionCohortType = GetRetentionType(u);

                        if (Logins.Add(u) == false)
                        {
                            Logger.Instance.Info(String.Format("{0} is a duplicate Login from keen", u.UserId));
                            //do something about duplicates from keen call becuse something is wrong.
                            // "badness detected" -- PJ

                        }
                    }
                    else
                    {
                        Login u = new Login()
                        {
                            UserId = entry["userid"].ToString(),
                            GameId = entry["gameid"].ToString(),
                            UserSessionId = entry["sessionid"].ToString(),
                            Platform = entry["platform"].ToString(),
                            LoginTimestamp = loginTime,
                            LogoffTimestamp = DateTime.MinValue,
                            InstallDateRecord = 0,
                            RetentionCohortType = RetentionCohortType.Unprocessed,
                            RecordDate = loginTime.Date.ToString(),
                            SessionLength = -1
                        };

                        if (Logins.Add(u) == false)
                        {
                            Logger.Instance.Info(String.Format("{0} is a duplicate Login from keen", u.UserId));
                            //do something about duplicates from keen call becuse something is wrong.
                            // "badness detected" -- PJ

                        }
                    }


                }
            }


            return Logins;
        }
        public HashSet<Logoff> ExtractLatestLogoffs(DateTime LastRecord)
        {
            HashSet<Logoff> Logoffs = new HashSet<Logoff>();

            DateTime end = DateTime.UtcNow;
            DateTime start = LastRecord;
            var result = provider.GetResource("Logoff", start, end);

            if (result.Any())
            {
                foreach (JObject entry in result)
                {
                    DateTime logoffTime = DateTime.Parse(entry["keen"]["timestamp"].ToString());
                    Logoff u = new Logoff()
                    {
                        UserId = entry["userid"].ToString(),
                        GameId = entry["gameid"].ToString(),
                        UserSessionId = entry["sessionid"].ToString(),
                        Platform = entry["platform"].ToString(),
                        LogoffTimestamp = logoffTime
                    };

                    if (Logoffs.Add(u) == false)
                    {
                        Logger.Instance.Info(String.Format("{0} is a duplicate Login from keen", u.UserId));
                        //do something about duplicates from keen call becuse something is wrong.
                        // "badness detected" -- PJ

                    }

                }
            }


            return Logoffs;
        }

        public HashSet<Login> ExtractLoginsOnDay(DateTime date)
        {
            HashSet<Login> Logins = new HashSet<Login>();

            DateTime end = date.Subtract(date.TimeOfDay);
            DateTime start = date.AddDays(-1).Subtract(date.TimeOfDay);
            var result = provider.GetResource("Login", start, end);

            if (result.Any())
            {
                foreach (JObject entry in result)
                {
                    DateTime loginTime = DateTime.Parse(entry["keen"]["timestamp"].ToString());
                    bool isNewUser;
                    var isNewUserProperty = entry.Property("isnewuser");
                    if (isNewUserProperty != null)
                    {
                        try
                        {
                            isNewUser = Convert.ToBoolean(entry["isnewuser"].ToString());
                        }
                        catch (Exception ex)
                        {
                            isNewUser = false;
                        }
                        Login u = new Login()
                        {
                            UserId = entry["userid"].ToString(),
                            GameId = entry["gameid"].ToString(),
                            UserSessionId = entry["sessionid"].ToString(),
                            Platform = entry["platform"].ToString(),
                            LoginTimestamp = loginTime,
                            City = entry["location"]["city"].ToString(),
                            Country = entry["location"]["country"].ToString(),
                            Region = entry["location"]["region"].ToString(),
                            Longitude = float.Parse(entry["location"]["longitude"].ToString(), CultureInfo.InvariantCulture.NumberFormat),
                            Latitude = float.Parse(entry["location"]["latitude"].ToString(), CultureInfo.InvariantCulture.NumberFormat),
                            LocationId = Convert.ToInt32(entry["location"]["id"].ToString()),
                            InstallDateRecord = (isNewUser) ? 1 : 0,
                            SessionLength = -1,
                            RecordDate = loginTime.ToString("yyyy-MM-dd")
                        };

                        u.RetentionCohortType = GetRetentionType(u);

                        if (Logins.Add(u) == false)
                        {
                            Logger.Instance.Info(String.Format("{0} is a duplicate Login from keen", u.UserId));
                            //do something about duplicates from keen call becuse something is wrong.
                            // "badness detected" -- PJ

                        }
                    }
                    else {
                        Login u = new Login()
                        {
                            UserId = entry["userid"].ToString(),
                            GameId = entry["gameid"].ToString(),
                            UserSessionId = entry["sessionid"].ToString(),
                            Platform = entry["platform"].ToString(),
                            LoginTimestamp = loginTime,
                            LogoffTimestamp = DateTime.MinValue,
                            InstallDateRecord = 0,
                            RetentionCohortType = RetentionCohortType.Unprocessed,
                            RecordDate = loginTime.Date.ToString(),
                            SessionLength = -1
                        };

                        if (Logins.Add(u) == false)
                        {
                            Logger.Instance.Info(String.Format("{0} is a duplicate Login from keen", u.UserId));
                            //do something about duplicates from keen call becuse something is wrong.
                            // "badness detected" -- PJ

                        }                 
                    }


                }
            }


            return Logins;
        }

        public HashSet<Logoff> ExtractLogoffsFromDay(DateTime date)
        {
            DateTime end = date.Subtract(date.TimeOfDay);
            DateTime start = date.AddDays(-1).Subtract(date.TimeOfDay);
            HashSet<Logoff> logoffEvents = new HashSet<Logoff>();
            var result = provider.GetResource("Logoff", start, end);

            if (result.Any())
            {
                foreach (JObject entry in result)
                {
                    DateTime logoffTime = DateTime.Parse(entry["keen"]["timestamp"].ToString());
                    Logoff logoff = new Logoff()
                    {
                        UserId = entry["userid"].ToString(),
                        GameId = entry["gameid"].ToString(),
                        UserSessionId = entry["sessionid"].ToString(),
                        Platform = entry["platform"].ToString(),
                        LoginTimestamp = logoffTime
                    };
                    if (logoffEvents.Add(logoff) == false)
                    {
                        Logger.Instance.Info(String.Format("{0} is a duplicate sessionId detected", logoff.UserSessionId));
                        //do something about duplicates from keen call becuse something is wrong.
                        // "badness detected" -- PJ

                    }

                }
            }
            return logoffEvents;
        }
        #endregion

        #region ExecutionContext
        public void QueryAndBuildUserSessionMeta()
        {
            processLogins();
            processLogoffs();
        }
        public void processLogins()
        {

            DateTime lastProcessRecord = GetLastLoginRecordTimestamp();
            //string countsUnProcessedNewUserLogins = String.Format("Select COUNT(*) from UserSessionMeta where DATE(LoginTimestamp) = DATE('{0}') AND UserSessionId IS null", ProcessDate.AddDays(-1).ToString("yyyy/MM/dd HH:mm:ss"));
            //int UnProcessedNewUsers = DBManager.Instance.QueryForCount(Datastore.Monitoring, countsUnProcessedNewUserLogins);

            if (true)
            {
                Logger.Instance.Info("------------------------------------------------");
                Logger.Instance.Info(String.Format("Processing Logins for {0} - {1}", lastProcessRecord.ToString(), DateTime.UtcNow));
                Logger.Instance.Info("------------------------------------------------");


                HashSet<Login> logins = ExtractLatestLogins(lastProcessRecord);
                Logger.Instance.Info(String.Format("{0} Logins Retrieved Successfully for {1}", logins.Count, lastProcessRecord.ToString()));

                List<UserSessionMeta> sessionsToday = logins.Cast<UserSessionMeta>().ToList();

                InsertLoginSessionMeta(sessionsToday);


            }


        }
        public void processLogins(List<Login> logins)
        {

            //DateTime lastProcessRecord = GetLastLoginRecordTimestamp();
            logins.Sort();
            Login FirstLoginInBatch = logins.FirstOrDefault();
            Login LastLoginINBatch = logins.LastOrDefault();
            string countsUnProcessedNewUserLogins = String.Format(@"Select COUNT(*) from {2} 
                                                                        where UserSessionID IN ('{0}','{1}');",
                                                                        FirstLoginInBatch.UserSessionId,
                                                                        LastLoginINBatch.UserSessionId,
                                                                        USER_SESSION_META_TABLE);
            int ProcessedNewUsers = DBManager.Instance.QueryForCount(Datastore.Monitoring, countsUnProcessedNewUserLogins);

            if (ProcessedNewUsers < 2)
            {
                Logger.Instance.Info("------------------------------------------------");
                Logger.Instance.Info(String.Format("Processing Logins for {0} - {1}", LastLoginINBatch.LoginTimestamp.ToString("yyyy/MM/dd HH:mm:ss"), FirstLoginInBatch.LoginTimestamp.ToString("yyyy/MM/dd HH:mm:ss")));
                Logger.Instance.Info("------------------------------------------------");

                Logger.Instance.Info(String.Format("{0} Logins Retrieved Successfully", logins.Count));
                List<UserSessionMeta> sessionsToday = logins.Cast<UserSessionMeta>().ToList();

                InsertLoginSessionMeta(sessionsToday);
                //Thread.Sleep(2 * 1000);
            }
            else
            {
                Console.WriteLine("Batch Already Processed");
            }


        }
        public void processLogins(DateTime ProcessDate)
        {

            //DateTime lastProcessRecord = GetLastLoginRecordTimestamp();
            string countsUnProcessedNewUserLogins = String.Format("Select COUNT(*) from UserSessionMeta where LoginTimestamp between '{0}' and '{1}' and InstallDateRecord = 0", ProcessDate.AddDays(-1).Date.ToString("yyyy/MM/dd HH:mm:ss"), ProcessDate.Date.ToString("yyyy/MM/dd HH:mm:ss"));
            List<UserSessionMeta> RangeOfSessions = UserSessions.Instance.GetSessions(ProcessDate.Date, ProcessDate.AddDays(1).Date);

            if (RangeOfSessions.Count == 0)
            {
                Logger.Instance.Info("------------------------------------------------");
                Logger.Instance.Info(String.Format("Processing Logins for {0} - {1}", ProcessDate.Date.ToString("yyyy/MM/dd HH:mm:ss"), ProcessDate.Date.ToString("yyyy/MM/dd HH:mm:ss")));
                Logger.Instance.Info("------------------------------------------------");


                HashSet<Login> logins = ExtractLoginsOnDay(ProcessDate);
                Logger.Instance.Info(String.Format("{0} Logins Retrieved Successfully", logins.Count));

                List<UserSessionMeta> sessionsToday = logins.Cast<UserSessionMeta>().ToList();
                //insert remaining
                InsertLoginSessionMeta(sessionsToday);
            }
            else
            {
                Console.WriteLine("Batch Already Processed");
            }


        }

        public void processLogoffs()
        {
            DateTime lastProcessRecord = GetLastLoginRecordTimestamp();
            HashSet<Logoff> logoffs = ExtractLatestLogoffs(lastProcessRecord);
            Logger.Instance.Info(String.Format("{0} Logoffs Retrieved Successfully {1} - {2}", logoffs.Count, lastProcessRecord.ToString(), DateTime.UtcNow.ToString()));
            if (logoffs.Count > 0)
            {
                List<string> SessionIds = new List<string>();
                StringBuilder sb = new StringBuilder();
                sb.Append("SELECT * from UserSessionMeta where UserSessionId IN (");
                foreach (Logoff logoff in logoffs)
                {
                    sb.AppendFormat("'{0}'", logoff.UserSessionId);
                    sb.Append(",");
                }
                sb.Length--;
                sb.Append(");");
                string query = sb.ToString();
                DataTable LogoffResult = DBManager.Instance.Query(Datastore.Monitoring, query);
                List<string> UpdateStatements = new List<string>();
                if (LogoffResult.HasRows())
                {
                    foreach (DataRow row in LogoffResult.Rows)
                    {
                        long validatedSessionLength = -1;
                        Logoff logofftoUpdate = logoffs.Where(x => x.UserSessionId == row["UserSessionId"].ToString()).FirstOrDefault();
                        DateTime RowLoginTime = DateTime.Parse(row["LoginTimestamp"].ToString());
                        if (logofftoUpdate != null)
                        {
                            long sessionLength = (logofftoUpdate.LogoffTimestamp - RowLoginTime).Ticks / TimeSpan.TicksPerMillisecond;
                            validatedSessionLength = (sessionLength > 0) ? sessionLength : -1;
                        }

                        UpdateStatements.Add(string.Format("UPDATE {0} SET LogoffTimestamp = '{1}', SessionLength = {2} WHERE UserSessionId = '{3}';", USER_SESSION_META_TABLE, logofftoUpdate.LogoffTimestamp.ToString("yyyy/MM/dd HH:mm:ss"), validatedSessionLength, logofftoUpdate.UserSessionId));
                    }
                    MoniverseResponse response = new MoniverseResponse();
                    UserSessions.Service(Service =>

                        response = Service.Update(new UpdateRequest()
                        {
                            Task = UpdateStatements,
                            TaskName = "Update Logoff Batch",
                            TimeStamp = DateTime.UtcNow
                        }));
                }

            }
            else
            {
                Logger.Instance.Info("No Logoffs retrieved");
                return;
            }

        }
        public void processLogoffs(List<Logoff> logoffs)
        {

            logoffs.Sort();

            Logoff FirstLoginInBatch = logoffs.FirstOrDefault();
            Logoff LastLoginINBatch = logoffs.LastOrDefault();
            List<UserSessionMeta> RangeOfSessions = UserSessions.Instance.GetSessions(LastLoginINBatch.LoginTimestamp, FirstLoginInBatch.LoginTimestamp);
            long validatedSessionLength = -1;
            foreach (Logoff sessionLogoff in logoffs)
            {
                UserSessionMeta CurrentUserSessions = RangeOfSessions.Where(x => x.UserId == sessionLogoff.UserId && x.UserSessionId == sessionLogoff.UserSessionId).FirstOrDefault();
                if (sessionLogoff != null)
                {
                    CurrentUserSessions.LogoffTimestamp = sessionLogoff.LogoffTimestamp;

                    long sessionLength = (CurrentUserSessions.LoginTimestamp - CurrentUserSessions.LogoffTimestamp).Ticks / TimeSpan.TicksPerMillisecond;
                    validatedSessionLength = (sessionLength > 0) ? sessionLength : -1;

                    CurrentUserSessions.SessionLength = validatedSessionLength;
                }
            }

            //clean up orphaned open sessions
            UpdateExistingOpenSessionsWithLogoutInfo(logoffs);

        }
        public void processLogoffs(DateTime ProcessDate)
        {
            string countsUnProcessedNewUserLogins = String.Format("Select COUNT(*) from UserSessionMeta where LoginTimestamp between '{0}' and '{1}' and SessionLength = -1", ProcessDate.AddDays(-1).Date.ToString("yyyy/MM/dd HH:mm:ss"), ProcessDate.Date.ToString("yyyy/MM/dd HH:mm:ss"));
            List<UserSessionMeta> RangeOfSessions = UserSessions.Instance.GetSessions(ProcessDate.Date, ProcessDate.AddDays(1).Date);

            if (RangeOfSessions.Count < 0)
            {
                HashSet<Logoff> KeenLogoffs = UserSessions.Instance.ExtractLogoffsFromDay(ProcessDate);
                long validatedSessionLength = -1;
                foreach (Logoff sessionLogoff in KeenLogoffs)
                {
                    UserSessionMeta CurrentUserSessions = RangeOfSessions.Where(x => x.UserId == sessionLogoff.UserId && x.UserSessionId == sessionLogoff.UserSessionId).FirstOrDefault();
                    if (sessionLogoff != null)
                    {
                        CurrentUserSessions.LogoffTimestamp = sessionLogoff.LogoffTimestamp;

                        long sessionLength = (CurrentUserSessions.LoginTimestamp - CurrentUserSessions.LogoffTimestamp).Ticks / TimeSpan.TicksPerMillisecond;
                        validatedSessionLength = (sessionLength > 0) ? sessionLength : -1;

                        CurrentUserSessions.SessionLength = validatedSessionLength;
                    }
                }
                List<Logoff> processedList = RangeOfSessions.Cast<Logoff>().ToList();
                //clean up orphaned open sessions
                UpdateExistingOpenSessionsWithLogoutInfo(processedList);
            }
        }

        #endregion

        #region Classification

        public RetentionCohortType GetRetentionType(Login user)
        {

            TrackedUserOccurance occurance = ReturningRetention.Instance.DetermineUserType(user);

            return occurance.CohortType;

        }

        #endregion

        #region CRUD
        public List<UserSessionMeta> GetSessions(DateTime StartDate, DateTime EndDate)
        {
            List<UserSessionMeta> UserSessions = new List<UserSessionMeta>();

            return UserSessions;
        }
        public void UpdateExistingOpenSessionsWithLogoutInfo(HashSet<Logoff> logoffs)
        {
            Logger.Instance.Info("Trying to update existing 'orphaned' user sessionsleft 'open'");
            int updateCount = 0;

            string query = String.Format(@"select * from {0} as GSM WHERE GSM.LogoffTimestamp = '0001-01-01 00:00:00'", USER_SESSION_META_TABLE);
            DataTable NullLogoffTimestampsSinceBOT = new DataTable();
            try
            {
                Logger.Instance.Info("UpdateExistingOpenSessionsWithLogoutInfo");
                NullLogoffTimestampsSinceBOT = DBManager.Instance.Query(Datastore.Monitoring, query);
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
                //throw;
            }


            Logger.Instance.Info(String.Format("{0} lonely orphans without logouts", NullLogoffTimestampsSinceBOT.Rows.Count));
            if (NullLogoffTimestampsSinceBOT.Rows.Count > 0)
            {
                List<string> updates = new List<string>();
                foreach (DataRow row in NullLogoffTimestampsSinceBOT.Rows)
                {

                    if (logoffs.Any(x =>
                    {
                        string thisSessionId = row["UserSessionId"].ToString();
                        //Logger.Instance.Info("Comparing {0} => {1}", x.UserSessionId, thisSessionId);
                        return x.UserSessionId == thisSessionId;
                    }))
                    {
                        Logoff match = logoffs.FirstOrDefault(x => x.UserSessionId == row["UserSessionId"].ToString());
                        string UserSessionId = row["UserSessionId"].ToString();
                        string validatedLogoffTimestamp = match.LoginTimestamp.ToString("yyyy-MM-dd HH:mm:ss");

                        long SessionLength = (match.LoginTimestamp - DateTime.Parse(row["LoginTimestamp"].ToString())).Ticks / TimeSpan.TicksPerMillisecond;

                        string updateQuery = String.Format(@"UPDATE {0} WHERE UserSessionId = '{1}' SET LogoffTimestamp = '{2}', SessionLength = {3};",
                            USER_SESSION_META_TABLE, UserSessionId, validatedLogoffTimestamp, SessionLength);
                        updates.Add(updateQuery);
                        updateCount++;
                    }
                }
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
                        Logger.Instance.Info("Beginning UpdateExistingOpenSessionsWithLogoutInfo");
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("");
                    }
                    UserSessions.Service(Service =>
                    {
                        response = Service.Update(new UpdateRequest()
                        {
                            TaskName = "UpdateExistingOpenSessionsWithLogoutInfo",
                            Task = updates,
                            TimeStamp = DateTime.UtcNow
                        });
                    });
                    //int result = DBManager.Instance.Update(Datastore.Monitoring, updates);

                    lock (MoniverseBase.ConsoleWriterLock)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("UpdateExistingOpenSessionsWithLogoutInfo success ");
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("");
                        Logger.Instance.Info(String.Format("Updated {0} orphaned Sessions with LogoutTimestamp and Session Length", updateCount));
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
        public void UpdateExistingOpenSessionsWithLogoutInfo(List<Logoff> logoffs)
        {

            List<string> updates = new List<string>();
            foreach (Logoff logoff in logoffs)
            {
                string updateQuery = String.Format(@"UPDATE {0} SET LogoffTimestamp = '{2}', SessionLength = {3} WHERE UserSessionId = '{1}';",
                    USER_SESSION_META_TABLE, logoff.UserSessionId, logoff.LogoffTimestamp.ToString("yyyy-MM-dd HH:mm:ss"), logoff.SessionLength);
                updates.Add(updateQuery);
            }
            try
            {
                lock (MoniverseBase.ConsoleWriterLock)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info("Beginning UpdateExistingOpenSessionsWithLogoutInfo");
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info("");
                }
                UserSessions.Service(service =>
                {
                    var specificCallConfig = service as IClientChannel;
                    specificCallConfig.OperationTimeout = new TimeSpan(0, 10, 0);
                    service.Update(new UpdateRequest()
                    {
                        TaskName = "UpdateExistingOpenSessionsWithLogoutInfo",
                        Task = updates,
                        TimeStamp = DateTime.UtcNow
                    });
                }
                );
                //int result = DBManager.Instance.Update(Datastore.Monitoring, updates);

                lock (MoniverseBase.ConsoleWriterLock)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info("UpdateExistingOpenSessionsWithLogoutInfo success ");
                    Logger.Instance.Info("--------------------------------------");
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
        public int InsertLoginSessionMeta(List<UserSessionMeta> sessions)
        {

            UserSessionMeta FirstLoginInBatch = sessions.FirstOrDefault();
            UserSessionMeta LastLoginINBatch = sessions.LastOrDefault();

            if (true)
            {
                foreach (List<UserSessionMeta> UserSessionBatch in sessions.Batch<UserSessionMeta>(500))
                {
                    string InsertStatement = string.Format("INSERT IGNORE INTO {1} (`UserId`, `GameId`,`UserSessionId` , `Platform`, `LoginTimestamp`, `LogoffTimestamp`, `City`, `Country`, `Region`,`Longitude`,`Latitude`, `LocationId`, `SessionLength`, `InstallDateRecord`, `RetentionCohortType`, `RecordDate`) VALUES {0};", DatabaseUtilities.instance.GenerateInsertValues<UserSessionMeta>(UserSessionBatch), USER_SESSION_META_TABLE);
                    int listLength = sessions.Count - 1;

                    try
                    {
                        lock (MoniverseBase.ConsoleWriterLock)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Logger.Instance.Info("--------------------------------------");
                            Logger.Instance.Info("Beginning Insert of Login Session Meta Batch");
                            Logger.Instance.Info("--------------------------------------");
                            Logger.Instance.Info("");
                        }

                        // int result = DBManager.Instance.Insert(Datastore.Monitoring, InsertStatement);
                        MoniverseResponse response = new MoniverseResponse()
                        {
                            Status = "unsent",
                            TimeStamp = DateTime.UtcNow
                        };
                        UserSessions.Service(Service =>
                        {
                            response = Service.Insert(new MoniverseRequest()
                            {
                                TaskName = "InsertLoginSessionMeta",
                                Task = InsertStatement,
                                TimeStamp = DateTime.UtcNow
                            });
                        });
                        lock (MoniverseBase.ConsoleWriterLock)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Logger.Instance.Info("--------------------------------------");
                            Logger.Instance.Info("Login Session Meta Batch Success");
                            Logger.Instance.Info("--------------------------------------");
                            Logger.Instance.Info("");
                            Logger.Instance.Info(String.Format("success!! {0}", response.Status));
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
            else
            {
                Console.WriteLine(String.Format("Already processed {0} - {1}",
                    FirstLoginInBatch.LoginTimestamp.ToString("yyyy/MM/dd HH:mm:ss"),
                    LastLoginINBatch.LoginTimestamp.ToString("yyyy/MM/dd HH:mm:ss")));
            }

            return 0;
        }

        #endregion

        #region Helpers
        public DateTime GetLastLoginRecordTimestamp()
        {
            string lastRecordGot = String.Format("select LoginTimestamp from {0} Where InstallDateRecord = 0 order by LoginTimestamp desc limit 1;", USER_SESSION_META_TABLE);
            DataTable singleRow = DBManager.Instance.Query(Datastore.Monitoring, lastRecordGot);
            DateTime result = DateTime.MinValue;
            try
            {
                result = DateTime.Parse(singleRow.Rows[0]["LoginTimestamp"].ToString());
            }
            catch (Exception)
            {

                Console.WriteLine("No Records");
            }
            return result;
        }
        public DateTime GetLastInstallRecordTimestamp()
        {
            string lastRecordGot = String.Format("select LoginTimestamp from {0} Where InstallDateRecord = 1 order by LoginTimestamp desc limit 1;", USER_SESSION_META_TABLE);
            DataTable singleRow = DBManager.Instance.Query(Datastore.Monitoring, lastRecordGot);

            return DateTime.Parse(singleRow.Rows[0]["LoginTimestamp"].ToString());
        }
        public DateTime GetLastFirstLoginRecordTimestamp()
        {
            string lastRecordGot = "select RecordTimestamp from retention_firstlogin order by RecordTimestamp desc limit 1;";
            DataTable singleRow = DBManager.Instance.Query(Datastore.Monitoring, lastRecordGot);

            return DateTime.Parse(singleRow.Rows[0]["RecordTimestamp"].ToString());
        }
        #endregion
    }
}
