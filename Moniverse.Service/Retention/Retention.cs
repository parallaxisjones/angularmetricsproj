using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Playverse.Data;
using Utilities;
using System.Diagnostics;
using Moniverse.Contract;
using Moniverse.Service;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Moniverse.Service
{
    public static class RetentionRowExtensions
    {
        public static string GenerateDayString(this RetentionRow row)
        {
            List<string> stringlist = new List<string>();
            for (int i = 1; i < row.days.Length; i++)
            {
                string dayString = "Day" + i;
                stringlist.Add(dayString);
            }
            return string.Join(",", stringlist.ToArray());
        }
    }

    public class RetentionRow : IRetentionRow
    {
        public DateTime date { get; set; }
        public int installsOnThisDay { get; set; }
        public int loginsOnThisDay { get; set; }
        public float[] days { get; set; }


        public RetentionRow()
        {
            days = new float[61];
            // added = to 0 comparison to test if that fixes the 60th day not calculating
            for (int i = days.Length - 1; i >= 0; i--)
            {
                days[i] = -1;
            }
        }

        public void SetDayPercent(int i, float percent)
        {
            days[i] = percent;
        }

        public int GetDayNRetentionCount(int n)
        {
            //loginsTodayThatInstalledNDaysAgo
            int count = 0;
            string query = String.Format(@"Select COUNT(DISTINCT(logins.UserId)) as count
                from UserSessionMeta as logins
                inner join (
                    select * 
                    from UserSessionMeta as installssubquery
                    WHERE date(installssubquery.LoginTimestamp) = date('{1}')
                    AND InstallDateRecord = 1
                ) installs
                on logins.UserId = installs.UserId
                where DATE(logins.LoginTimestamp) = adddate('{1}', INTERVAL {0} DAY);", n, date.Date.ToString("yyyy/MM/dd"));
            try
            {
                DataTable result = DBManager.Instance.Query(Datastore.Monitoring, query);
                if (result.Rows.Count > 0)
                {
                    object c = result.Rows[0][0];
                    count = Convert.ToInt32(c);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }
            return count;
        }

        public int GetDayNRetentionCount(DateTime today, int n)
        {
            return 0;
        }
        public int GetDayNInstallsCount(DateTime today, int n)
        {
            return 0;
        }

        public static RetentionRow Get(DateTime datetime)
        {
            RetentionRow RetentionRow = new RetentionRow()
            {
                date = datetime.Date
            };
            string query = String.Format(@"SELECT * FROM {1} where DATE(Date) = DATE('{0}')", datetime.ToString("yyyy/MM/dd HH:mm:ss"), "Retention");
            try
            {
                DataTable singleRetentionRow = DBManager.Instance.Query(Datastore.Monitoring, query);
                if (singleRetentionRow.Rows.Count > 0)
                {

                    DataRow c = singleRetentionRow.Rows[0];
                    foreach (DataColumn col in singleRetentionRow.Columns)
                    {
                        if (col.ColumnName.Contains("Day"))
                        {
                            int colIndex = Convert.ToInt32(Regex.Split(col.ColumnName, @"\D+")[1]);
                            float perc = c.Field<float>(col.ColumnName);
                            RetentionRow.SetDayPercent(colIndex, perc);
                        }
                    }
                }
                else
                {
                    RetentionRow = new RetentionRow()
                    {
                        date = datetime.Date
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Retention Get Problems" + datetime.ToString());
            }

            return RetentionRow;
        }

        public void Process()
        {
            try
            {
                lock (MoniverseBase.ConsoleWriterLock)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info(String.Format("--== {0}  : Updating Retention  ==--", date.Date));
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info("");
                }

                updatePercents();

                Logger.Instance.Info(String.Format("{0} : installs {1}", date.ToString(), installsOnThisDay));
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }
        }

        private void updatePercents()
        {
            for (int i = 1; i < days.Length; i++)
            {
                float dayPercent = 0;
                if (canHavePercent(i))
                {
                    int loginsTodayThatInstalledNDaysAgo = GetDayNRetentionCount(i);
                    Logger.Instance.Info(String.Format("divisor (bottom) {0} for date: {1} (installs on {1})", installsOnThisDay, date.ToString()));

                    if (installsOnThisDay != 0 || days[i] == -1)
                    {
                        dayPercent = (loginsTodayThatInstalledNDaysAgo / (float)installsOnThisDay) * 100;
                        if (double.IsNaN(dayPercent) || double.IsInfinity(dayPercent))
                        {
                            dayPercent = 0;
                        }

                        Logger.Instance.Info(String.Format("{0} : {1} = ({2} / {3}) * 100", date.ToString("yyyy/MM/dd"), dayPercent, loginsTodayThatInstalledNDaysAgo, (float)installsOnThisDay));
                    }

                    Logger.Instance.Info(String.Format("{0} : Day {2} : {3} : {1} % : Processed", date.ToString("yyyy/MM/dd"), dayPercent, i, date.AddDays(-i).ToString("yyyy/MM/dd")));
                    Logger.Instance.Info("--------------------  \r\n");

                    SetDayPercent(i, dayPercent);
                }

                //days[i] = 0;
            }
        }
        public bool canHavePercent(int i)
        {
            bool bCanHas = !(date.AddDays(i).Date >= DateTime.UtcNow.Date);
            Reasoning reason = new Reasoning()
            {
                Actual = bCanHas.ToString(),
                Expected = date.AddDays(i).Date.ToString(),
                Condition = "Expected is less than or equal to actual",
                OriginMethod = "canHavePercent"
            };

            Console.WriteLine(reason.GetReasoningString());
            return bCanHas;
        }

    }

    public class ReturnerBuckets
    {
        public virtual string USER_SESSION_META_TABLE { get { return "UserSessionMeta"; } }
        public virtual string RETENTION_RETURNER_VIEW_TABLE { get { return "Retention"; } }

        public DateTime Date { get; set; }
        public List<string> AllUserIdsLast7Days { get; set; }
        public ReturnerRetentionDataPoints NURR { get; set; }
        public ReturnerRetentionDataPoints CURR { get; set; }
        public ReturnerRetentionDataPoints RURR { get; set; }

        public ReturnerBuckets(DateTime date)
        {
            Date = date.Date;
        }

        public int GetWAU(){
            return GetWAUForDate(Date).Count;
        }

        public ReturnerBuckets Get()
        {

            Get7DayTotalCountUsers();
            string query = String.Format(@"SELECT * FROM {1} where DATE(Date) = DATE('{0}')", Date.ToString("yyyy/MM/dd"), RETENTION_RETURNER_VIEW_TABLE);
            try
            {
                DataTable singleRetentionRow = DBManager.Instance.Query(Datastore.Monitoring, query);

                NURR = new ReturnerRetentionDataPoints()
                {
                    RecordDate = Date,
                    startRange = Date.AddDays(-6),
                    Last7DaysTotalCount = AllUserIdsLast7Days.Count,
                    Type = RetentionCohortType.NewUser,
                };
                CURR = new ReturnerRetentionDataPoints()
                {
                    RecordDate = Date,
                    startRange = Date.AddDays(-6),
                    Last7DaysTotalCount = AllUserIdsLast7Days.Count,
                    Type = RetentionCohortType.ContinuingUser
                };
                RURR = new ReturnerRetentionDataPoints()
                {
                    RecordDate = Date,
                    startRange = Date.AddDays(-6),
                    Last7DaysTotalCount = AllUserIdsLast7Days.Count,
                    Type = RetentionCohortType.ReactivatedUser
                };

                if (singleRetentionRow.Rows.Count > 0)
                {

                    DataRow c = singleRetentionRow.Rows[0];
                    if (c["NewUserCohort"] != DBNull.Value)
                    {
                        NURR.RelevantMetricProcessedCount = c.Field<int>("NewUserCohort");
                    }
                    if (c["ContinuingUsersCohort"] != DBNull.Value)
                    {
                        CURR.RelevantMetricProcessedCount = c.Field<int>("ContinuingUsersCohort");
                    }

                    if (c["ReactivatedUsersCohort"] != DBNull.Value)
                    {
                        RURR.RelevantMetricProcessedCount = c.Field<int>("ReactivatedUsersCohort");
                    }

                    if (c["NURR"] != DBNull.Value)
                    {
                        NURR.Percentage = c.Field<decimal>("NURR");
                    }

                    if (c["CURR"] != DBNull.Value)
                    {
                        CURR.Percentage = c.Field<decimal>("CURR");
                    }

                    if (c["RURR"] != DBNull.Value)
                    {
                        RURR.Percentage = c.Field<decimal>("RURR");
                    }
                }
                else
                {
                    CalculateNURR();
                    CalculateCURR();
                    CalculateRURR();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Returners Get Problems" + Date.ToString());
            }

            return this;
        }

        #region Continuing
        protected bool IsContinuingUser(Login User)
        {
            string query = String.Format(@"SELECT COUNT(UserID)
            FROM {1} as hasBeenSeen
            WHERE hasBeenSeen.UserId = '{0}'
			AND hasBeenSeen.LoginTimestamp > subdate('{2}', interval 7 day)    
            AND hasBeenSeen.UserId IN 
            ( 
                SELECT UserID
                    FROM {1} as thisWeek
                    WHERE thisWeek.UserId = '{0}'
					AND thisWeek.LoginTimestamp > subdate('{2}', interval 14 day) #2015-01-08
					AND thisWeek.LoginTimestamp < subdate('{2}', interval 6 day) #2015-01-15
            );", User.UserId, USER_SESSION_META_TABLE, User.LoginTimestamp.ToString("yyyy-MM-dd"));

            return (DBManager.Instance.QueryForCount(Datastore.Monitoring, query) > 0);
        }
        public void CalculateCURR()
        {

            List<string> cont = GetContinuingUserIdsfrom8to14DaysAgoRange(Date);
            List<string> returningCont = GetWAUForDate(Date);
            List<string> intersection = cont.Intersect(returningCont).ToList();
            CURR.RelevantMetricProcessedCount = intersection.Count;
            if (intersection.Count > 0)
            {
                decimal percentage = Decimal.Divide(intersection.Count, cont.Count);
                percentage = Decimal.Multiply(percentage, 100);
                percentage = Decimal.Round(percentage, 2);
                percentage = Decimal.Floor(percentage);
                CURR.RecordDate = Date.Date;
                CURR.CountPreviousWeek = cont.Count;
                CURR.ReturningContinuing = intersection.Count;
                CURR.Percentage = percentage;
            }
        }


        public List<string> GetContinuingUserIdsfrom8to14DaysAgoRange(DateTime ProcessDate)
        {
            List<TrackedUserOccurance> ContinuingUsersfrom8to14DaysAgoRange = GetRetentionCohorts(ProcessDate);
            
            List<string> users = ContinuingUsersfrom8to14DaysAgoRange
                .Where(grp => grp.CohortType == RetentionCohortType.ContinuingUser)
                .Select(x => x.UserId)
                .ToList();

            return users;
        }
        #endregion

        #region Reacts
        protected bool IsReactivatedUser(Login User)
        {
            bool result = false;
            string query = String.Format(@"select COUNT(*)
                                        from {1} as PlayedMorethan6DaysAgo 
                                        WHERE PlayedMorethan6DaysAgo.UserId = '{2}'
                                        AND LoginTimestamp < subdate('{0}', INTERVAL 7 DAY)
                                        AND PlayedMorethan6DaysAgo.UserId NOT IN (
	                                        SELECT rbv.UserId FROM {1} as rbv
	                                        WHERE DATE(rbv.LoginTimestamp) >= subdate('{0}', INTERVAL 7 DAY) 
	                                        AND DATE(rbv.LoginTimestamp) < DATE('{0}')
	                                        AND rbv.UserId = '{2}'
                                        ); ",
                                        User.LoginTimestamp.ToString("yyyy-MM-dd"),
                                        USER_SESSION_META_TABLE,
                                        User.UserId);

            return (DBManager.Instance.QueryForCount(Datastore.Monitoring, query) > 0);
        }
        public void CalculateRURR()
        {
            //ReturnerRetentionDataPoints retentionDate = new ReturnerRetentionDataPoints();

            List<string> reacts = GetReactsUsersIdsfrom8to14DaysAgoRange(Date);
            List<string> lastWeek = GetWAUForDate(Date);
            List<string> intersection = reacts.Intersect(lastWeek).ToList();
            RURR.RelevantMetricProcessedCount = intersection.Count;
            if (intersection.Count > 0)
            {
                decimal percentage = Decimal.Divide(intersection.Count, reacts.Count);
                percentage = Decimal.Multiply(percentage, 100);
                percentage = Decimal.Round(percentage, 2);
                percentage = Decimal.Floor(percentage);
                RURR.RecordDate = Date.Date;
                RURR.CountPreviousWeek = reacts.Count;
                RURR.ReturningContinuing = intersection.Count;
                RURR.Percentage = percentage;
            }

        }
        public List<TrackedUserOccurance> GetReactsfrom8to14DaysAgoRange(DateTime ProcessDate)
        {
            List<TrackedUserOccurance> userList = new List<TrackedUserOccurance>();

            //have to add a day to the end of the range we're looking at if we're using the 00:00:00 midnight beginning of the range dates
            //so we can get the whole date, either we can use INTERVAL 9 DAY in the SQL statement or ADD DAY in C# DateTime land. -- PJ
            string query = String.Format(@"SELECT DISTINCT(UserId) as UserId, LoginTimestamp 
                                           FROM {0} 
                                           WHERE LoginTimestamp >= SUBDATE('{1}', INTERVAL 13 DAY) 
                                           AND LoginTimestamp <= SUBDATE('{1}', INTERVAL 7 DAY) 
                                           AND RetentionCohortType = 2  
                                           ORDER BY LoginTimestamp asc;",
                                           USER_SESSION_META_TABLE,
                                           ProcessDate.ToString("yyyy-MM-dd 00:00:00"));

            DataTable UsersTable = DBManager.Instance.Query(Datastore.Monitoring, query);
            if (UsersTable.Rows.Count > 0)
            {
                foreach (DataRow UserRecord in UsersTable.Rows)
                {
                    TrackedUserOccurance user = new TrackedUserOccurance()
                    {
                        Date = DateTime.Parse(UserRecord["LoginTimestamp"].ToString()),
                        UserId = UserRecord["UserId"].ToString(),
                        CohortType = RetentionCohortType.ReactivatedUser
                    };
                    userList.Add(user);
                }
            }
            return userList;
        }
        public List<string> GetReactsUsersIdsfrom8to14DaysAgoRange(DateTime ProcessDate)
        {
            List<TrackedUserOccurance> Reactsfrom8to14DaysAgoRange = GetReactsfrom8to14DaysAgoRange(ProcessDate);
            List<string> users = Reactsfrom8to14DaysAgoRange
                .GroupBy(occurance => occurance.UserId)
                .Select(grp => grp.First())
                .Select(x => x.UserId)
                .ToList();
            return users;
        }

        #endregion

        #region NewUsers
        protected bool IsNewUser(Login User)
        {
            string query = String.Format(@"SELECT COUNT(UserId)
                                            FROM {0} 
                                            WHERE DATE(LoginTimestamp) > subdate('{1}', interval 7 day)
                                            AND InstallDateRecord = 1                                          
                                            AND UserId = '{2}';", USER_SESSION_META_TABLE, User.LoginTimestamp.ToString("yyyy-MM-dd"), User.UserId);

            return (DBManager.Instance.QueryForCount(Datastore.Monitoring, query) > 0);
        }
        public void CalculateNURR()
        {
            List<string> lastWeek = GetWAUForDate(Date);
            List<string> newUsers = GetNewUsersUserIdsFrom8to14DaysAgoRange(Date);
            List<string> intersection = lastWeek.Intersect(newUsers).ToList();
            NURR.RelevantMetricProcessedCount = intersection.Count;
            if (intersection.Count > 0)
            {
                decimal percentage = Decimal.Divide(intersection.Count, newUsers.Count);
                percentage = Decimal.Multiply(percentage, 100);
                percentage = Decimal.Round(percentage, 2);
                percentage = Decimal.Floor(percentage);

                NURR.RecordDate = Date.Date;
                NURR.CountPreviousWeek = newUsers.Count;
                NURR.ReturningContinuing = intersection.Count;
                NURR.Percentage = percentage;
            }

        }
        public List<TrackedUserOccurance> GetNewUsersfrom8to14DaysAgoRange(DateTime ProcessDate)
        {
            List<TrackedUserOccurance> userList = new List<TrackedUserOccurance>();

            //have to add a day to the end of the range we're looking at if we're using the 00:00:00 midnight beginning of the range dates
            //so we can get the whole date, either we can use INTERVAL 9 DAY in the SQL statement or ADD DAY in C# DateTime land. -- PJ
            string query = String.Format(@"SELECT DISTINCT(UserId) as UserId, LoginTimestamp 
                                           FROM {0} 
                                           WHERE LoginTimestamp >= SUBDATE('{1}', INTERVAL 13 DAY) 
                                           AND LoginTimestamp <= SUBDATE('{1}', INTERVAL 7 DAY) 
                                           AND RetentionCohortType = 0  
                                           ORDER BY LoginTimestamp desc;",
                                           USER_SESSION_META_TABLE,
                                           ProcessDate.ToString("yyyy-MM-dd 00:00:00"));

            DataTable UsersTable = DBManager.Instance.Query(Datastore.Monitoring, query);
            if (UsersTable.Rows.Count > 0)
            {
                foreach (DataRow UserRecord in UsersTable.Rows)
                {
                    TrackedUserOccurance user = new TrackedUserOccurance()
                    {
                        Date = DateTime.Parse(UserRecord["LoginTimestamp"].ToString()),
                        UserId = UserRecord["UserId"].ToString(),
                        CohortType = RetentionCohortType.NewUser
                    };
                    userList.Add(user);
                }
            }
            return userList;
        }
        public List<string> GetNewUsersUserIdsFrom8to14DaysAgoRange(DateTime ProcessDate)
        {
            List<TrackedUserOccurance> NewUsersfrom8to14DaysAgoRange = GetNewUsersfrom8to14DaysAgoRange(ProcessDate);
            return NewUsersfrom8to14DaysAgoRange.Select(x => x.UserId).ToList();
        }

        #endregion

        protected List<string> Get7DayTotalCountUsers()
        {
            string query = String.Format(@"SELECT DISTINCT(UserId) 
                                           FROM {0} 
                                           WHERE LoginTimestamp BETWEEN SUBDATE('{1}', INTERVAL 6 DAY) AND '{1}'
                                           ORDER BY LoginTimestamp desc;",
                               USER_SESSION_META_TABLE,
                               Date.ToString("yyyy-MM-dd 00:00:00"));

            DataTable AllUsersFromTheLast7DaysIncludingThisDay = DBManager.Instance.Query(Datastore.Monitoring, query);

            AllUserIdsLast7Days = AllUsersFromTheLast7DaysIncludingThisDay.AsEnumerable().Select(x => x.Field<string>("UserId").ToString()).ToList();
            return this.AllUserIdsLast7Days;
        }
        public List<TrackedUserOccurance> GetRetentionCohorts(DateTime ProcessDate)
        {
            List<TrackedUserOccurance> userList = new List<TrackedUserOccurance>();

            //have to add a day to the end of the range we're looking at if we're using the 00:00:00 midnight beginning of the range dates
            //so we can get the whole date, either we can use INTERVAL 9 DAY in the SQL statement or ADD DAY in C# DateTime land. -- PJ
            string query = String.Format(@"	SELECT UserId, min(LoginTimestamp) as LoginTimestamp, RetentionCohortType
	                                        FROM {0} 
	                                        WHERE LoginTimestamp >= SUBDATE('{1}', INTERVAL 13 DAY) 
	                                        AND LoginTimestamp <= SUBDATE('{1}', INTERVAL 7 DAY) 
	                                        GROUP BY UserId  
	                                        ORDER BY RetentionCohortType, UserId;",
   USER_SESSION_META_TABLE,
   ProcessDate.ToString("yyyy-MM-dd 00:00:00"));

            DataTable UsersTable = DBManager.Instance.Query(Datastore.Monitoring, query);
            if (UsersTable.Rows.Count > 0)
            {
                foreach (DataRow UserRecord in UsersTable.Rows)
                {
                    TrackedUserOccurance user = new TrackedUserOccurance()
                    {
                        Date = DateTime.Parse(UserRecord["LoginTimestamp"].ToString()),
                        UserId = UserRecord["UserId"].ToString(),
                        CohortType = (RetentionCohortType)Convert.ToInt32(UserRecord["RetentionCohortType"].ToString())
                    };
                    userList.Add(user);
                }
            }
            return userList;
        }
        public List<string> GetWAUForDate(DateTime ProcessDate)
        {
            string query = String.Format(@"SELECT DISTINCT(UserId) 
                                           FROM {0} 
                                           WHERE LoginTimestamp >= SUBDATE('{1}', INTERVAL 6 DAY) 
                                           AND LoginTimestamp <= '{1}'
                                           ORDER BY LoginTimestamp desc;",
                                           USER_SESSION_META_TABLE,
                                           ProcessDate.ToString("yyyy-MM-dd 00:00:00"));

            DataTable Continuingfrom8to14ThatReturned1to7 = DBManager.Instance.Query(Datastore.Monitoring, query);
            return Continuingfrom8to14ThatReturned1to7.AsEnumerable().Select(x => x.Field<string>("UserId").ToString()).ToList();
        }
    }

    public class Retention : ServiceClassBase
    {
        public virtual string USER_SESSION_META_TABLE { get { return "UserSessionMeta"; } }

        public Retention() : base() { }
        public static Retention Instance = new Retention();
        public void CalculateRetention(int ProcessXNumberOfDays)
        {
            DateTime today = DateTime.UtcNow;

            try
            {
                lock (MoniverseBase.ConsoleWriterLock)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info(String.Format("-= Beginning Retention Calculation =- \r\n -= {0} days: span {1} - {2} =-", (today - today.AddDays(-ProcessXNumberOfDays)).Days, today, today.AddDays(-ProcessXNumberOfDays)));
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info("");
                }


                ProcessXRetentionDays((today - today.AddDays(-ProcessXNumberOfDays)).Days);

                lock (MoniverseBase.ConsoleWriterLock)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info(String.Format("-= Retention Calculation Success =- \r\n -= {0} days: span {1} - {2} =-", (today - today.AddDays(-ProcessXNumberOfDays)).Days, today, today.AddDays(-ProcessXNumberOfDays)));
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info("");
                    Console.ResetColor();
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Logger.Instance.Info(e.GetBaseException().ToString());
                Logger.Instance.Info(e.Message);
                Console.ResetColor();
            }



        }

        public void ProcessRetentionForDate(DateTime processDate)
        {

            lock (MoniverseBase.ConsoleWriterLock)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Logger.Instance.Info("--------------------------------------");
                Logger.Instance.Info(String.Format("-= {0} - RetentionInstallCheckDate - Getting RetentionRow for date =- \r\n", processDate));
                Logger.Instance.Info("--------------------------------------");
                Logger.Instance.Info("");
            }

            RetentionRow YesterdayRow = GetRetentionRow(processDate);
            if (YesterdayRow == null || YesterdayRow.installsOnThisDay.Equals(0))
            {
                lock (MoniverseBase.ConsoleWriterLock)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info(String.Format("-= {0} - Success Getting RetentionRow for date =- \r\n", processDate));
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info("");
                }

                try
                {
                    YesterdayRow = new RetentionRow();
                    YesterdayRow.date = processDate;
                    lock (MoniverseBase.ConsoleWriterLock)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info(String.Format("--== {1} New Users Added ==--", processDate.Date.AddDays(-1)));
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("");
                    }

                    YesterdayRow.installsOnThisDay = GetNewUsersCount(processDate);
                    YesterdayRow.loginsOnThisDay = GetLoginsCount(processDate);
                    Logger.Instance.Info(String.Format("{0} : installs {1}", processDate.ToString(), YesterdayRow.installsOnThisDay));
                }
                catch (Exception ex)
                {
                    Logger.Instance.Info(ex.Message);
                }

            }
            if (ShouldProcess(YesterdayRow))
            {
                UpdateDayPercents(YesterdayRow);
            }

            UpdateRetentionRow(YesterdayRow);
        }

        public void ProcessXRetentionDays(int I)
        {
            //#if DEBUG
            //            Debugger.Launch();
            //#endif
            DateTime RealToday = DateTime.UtcNow;
            int DaysLeft = I;
            for (int i = 1; i <= I; i++)
            {
                int DayToCheck = (RealToday - RealToday.AddDays(-DaysLeft)).Days;
                DateTime RetentionInstallCheckDate = RealToday.AddDays(-DaysLeft);
                lock (MoniverseBase.ConsoleWriterLock)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info(String.Format("-= {0} - RetentionInstallCheckDate - Getting RetentionRow for date =- \r\n", RetentionInstallCheckDate));
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info("");
                }

                RetentionRow YesterdayRow = GetRetentionRow(RetentionInstallCheckDate);
                if (YesterdayRow == null || YesterdayRow.installsOnThisDay.Equals(0))
                {
                    lock (MoniverseBase.ConsoleWriterLock)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info(String.Format("-= {0} - Success Getting RetentionRow for date =- \r\n", RetentionInstallCheckDate));
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("");
                    }

                    try
                    {
                        YesterdayRow = new RetentionRow();
                        YesterdayRow.date = RetentionInstallCheckDate;
                        lock (MoniverseBase.ConsoleWriterLock)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Logger.Instance.Info("--------------------------------------");
                            Logger.Instance.Info(String.Format("--== {0} iteration : {1} New Users Added ==--", i, RetentionInstallCheckDate.Date.AddDays(-1)));
                            Logger.Instance.Info("--------------------------------------");
                            Logger.Instance.Info("");
                        }

                        YesterdayRow.installsOnThisDay = GetNewUsersCount(RetentionInstallCheckDate);
                        YesterdayRow.loginsOnThisDay = GetLoginsCount(RetentionInstallCheckDate);
                        Logger.Instance.Info(String.Format("{0} : installs {1}", RetentionInstallCheckDate.ToString(), YesterdayRow.installsOnThisDay));
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Info(ex.Message);
                    }

                }
                if (ShouldProcess(YesterdayRow))
                {
                    UpdateDayPercents(YesterdayRow);
                }

                UpdateRetentionRow(YesterdayRow);
                DaysLeft--;
                Logger.Instance.Info(String.Format("---- {0} Days Left ---- \r\n\r\n", DaysLeft));
            }
        }

        public bool ShouldProcess(RetentionRow row)
        {
            return true;
        }

        public bool canHavePercent(DateTime Nday, int i)
        {
            bool bCanHazPercent = true;

            DateTime DaysSinceDayN = Nday.AddDays(i);
            //Logger.Instance.Info("-= Row Date: {0} =- \r\n\r\n -= Percent Process Date: {1} : Should Process: {2}=-", Nday, DaysSinceDayN, bCanHazPercent);
            if (DaysSinceDayN.Date >= DateTime.UtcNow.Date)
            {
                bCanHazPercent = false;
            }

            Logger.Instance.Info(String.Format("-= Day {3} Percent Process Date: {1} : retention for New Installs Row Date: {0} =- \r\n -= Possible to Process?: {2} =-", Nday, DaysSinceDayN, bCanHazPercent, i));
            Logger.Instance.Info("\r\n");
            return bCanHazPercent;
        }

        public bool isTodayNDayZero(DateTime Nday, int i)
        {
            bool bCanHazPercent = false;

            DateTime DaysSinceDayN = Nday.AddDays(i);

            if (DaysSinceDayN.Date == DateTime.UtcNow.Date)
            {
                bCanHazPercent = true;
            }


            return bCanHazPercent;
        }

        public void UpdateDayPercents(IRetentionRow row)
        {

            //divide by zero exception check
            //also dont insert if there is nothing

            for (int i = row.days.Length - 1; i > 0; i--)
            {
                float dayPercent = 0;
                if (canHavePercent(row.date, i))
                {

                    int loginsTodayThatInstalledNDaysAgo = row.GetDayNRetentionCount(row.date, i);
                    //int installsNDaysAgo = row.GetDayNInstallsCount(row.date, i);
                    Logger.Instance.Info(String.Format("divisor (bottom) {0} for date: {1} (installs on {1})", row.installsOnThisDay, row.date.ToString()));
                    if (row.installsOnThisDay != 0 || row.days[i] == -1)
                    {
                        dayPercent = (loginsTodayThatInstalledNDaysAgo / (float)row.installsOnThisDay) * 100;
                        if (double.IsNaN(dayPercent) || double.IsInfinity(dayPercent))
                        {
                            dayPercent = 0;
                        }
                        Logger.Instance.Info(String.Format("{0} : {1} = ({2} / {3}) * 100", row.date.ToString("yyyy/MM/dd"), dayPercent, loginsTodayThatInstalledNDaysAgo, (float)row.installsOnThisDay));
                    }
                    Logger.Instance.Info(String.Format("{0} : Day {2} : {3} : {1} % : Processed", row.date.ToString("yyyy/MM/dd"), dayPercent, i, row.date.AddDays(-i).ToString("yyyy/MM/dd")));
                    Logger.Instance.Info("--------------------  \r\n");
                    row.SetDayPercent(i, dayPercent);
                }

                //days[i] = 0;
            }
        }
        private string GenerateDayString(RetentionRow row)
        {
            List<string> stringlist = new List<string>();
            for (int i = 1; i < row.days.Length; i++)
            {
                string dayString = "Day" + i;
                stringlist.Add(dayString);
            }
            return string.Join(",", stringlist.ToArray());
        }
        public void InsertRetentionRow(RetentionRow newday)
        {
            StringBuilder query = new StringBuilder();
            query.Append("INSERT INTO Retention");
            query.AppendFormat("(Date, NewUsers, Logins, " + GenerateDayString(newday) + ") VALUES ('{0}','{1}', '{2}'", newday.date.ToString("yyyy/MM/dd 00:00:00"), newday.installsOnThisDay, newday.loginsOnThisDay);

            for (int i = 1; i <= newday.days.Length - 1; i++)
            {
                query.AppendFormat(",'{0}'", newday.days[i]);
            }
            query.Append(");");
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
                    Logger.Instance.Info("Beginning Insert of Retention Row Batch");
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info("");
                }
                Retention.Service(Service => Service.Insert(new MoniverseRequest()
                {
                    TaskName = "Insert Retention Row Batch",
                    Task = query.ToString(),
                    TimeStamp = DateTime.UtcNow
                }));
                //int result = DBManager.Instance.Insert(Datastore.Monitoring, query.ToString());

                lock (MoniverseBase.ConsoleWriterLock)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info("Insert Retention Row Batch success");
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info("");
                    Logger.Instance.Info("Done inserting Retention Row Data.");
                    Logger.Instance.Info(String.Format("success!! {0}", response.Status));
                    Logger.Instance.Info("");
                    Console.ResetColor();
                }

            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
                //Logger.Instance.Error(ex.Message);
            }
        }

        public void UpdateRetentionRow(RetentionRow thatDay)
        {
            StringBuilder query = new StringBuilder();
            query.Append("UPDATE Retention SET ");
            for (int i = thatDay.days.Length - 1; i > 0; i--)
            {
                query.AppendFormat("Day{0} = '{1}'", i, thatDay.days[i]);
                if (i != 1)
                {
                    query.Append(",");
                }
                else
                {
                    query.AppendFormat(" WHERE DATE(Date) = DATE('{0}');", thatDay.date.ToString("yyyy/MM/dd 00:00:00"));
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
                    Logger.Instance.Info("Beginning Update of Retention Row Batch");
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info("");
                }
                Retention.Service(Service =>
                    response = Service.Insert(new MoniverseRequest()
                    {
                        TaskName = "UpdateRetentionRow Row Batch",
                        Task = query.ToString(),
                        TimeStamp = DateTime.UtcNow
                    }));
                if (response.Status != "404" || response.Status != "500" || response.Status != "fail")
                {
                    Retention.Instance.InsertRetentionRow(thatDay);
                    lock (MoniverseBase.ConsoleWriterLock)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("Retention Row Update Success");
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("");
                        Logger.Instance.Info(String.Format("--== Updated Retention Row: {0} ==--", thatDay.date.ToString()));
                        Logger.Instance.Info("");
                        Console.ResetColor();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex.Message);
                Logger.Instance.Info(ex.Message);
            }
        }

        public int GetNewUsersCount(int i)
        {
            return GetNewUsersCount(DateTime.UtcNow.AddDays(-i));
        }

        public int GetNewUsersCount(DateTime datetime)
        {

            //if (datetime > DateTime.UtcNow)
            //{
            //    Exception ex = new Exception("datetime must be before today");
            //    //throw ex;
            //}

            int count = 0;
            string query = String.Format("select count(distinct(UserId)) from UserSessionMeta as fl where DATE(fl.LoginTimestamp) like DATE('{0}') AND InstallDateRecord = 1", datetime.ToString("yyyy/MM/dd HH:mm:ss"));

            try
            {
                DataTable result = DBManager.Instance.Query(Datastore.Monitoring, query);
                object r = result.Rows[0][0];
                count = Convert.ToInt32(r);
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }

            return count;
        }
        public int GetLoginsCount(DateTime datetime)
        {


            int count = 0;
            string query = String.Format("select distinct(count(UserId)) from UserSessionMeta as l where DATE(l.LoginTimestamp) = DATE('{0}') and InstallDateRecord = 0;", datetime.ToString("yyyy/MM/dd HH:mm:ss"));

            try
            {
                DataTable result = DBManager.Instance.Query(Datastore.Monitoring, query);
                object r = result.Rows[0][0];
                count = Convert.ToInt32(r);
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }

            return count;
        }

        //we're in a for loop when this is called, so we only want one row at a time
        public RetentionRow GetRetentionRow(DateTime datetime)
        {
            RetentionRow RetentionRow = new RetentionRow();
            string query = String.Format(@"SELECT * FROM Retention where DATE(Date) = DATE('{0}')", datetime.ToString("yyyy/MM/dd HH:mm:ss"));
            try
            {
                DataTable singleRetentionRow = DBManager.Instance.Query(Datastore.Monitoring, query);
                if (singleRetentionRow.Rows.Count > 0)
                {

                    DataRow c = singleRetentionRow.Rows[0];
                    RetentionRow.date = DateTime.Parse(c["Date"].ToString());
                    RetentionRow.installsOnThisDay = Convert.ToInt32(c["NewUsers"].ToString());
                    RetentionRow.loginsOnThisDay = Convert.ToInt32(c["Logins"].ToString());

                    int index = 0;
                    foreach (object o in c.ItemArray)
                    {
                        if (o is float)
                        {
                            index++;
                            RetentionRow.SetDayPercent(index, (float)o);
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Do not have Retention Entry for date" + datetime.ToString());
            }

            return RetentionRow;
        }



        public int GetDayNRetentionCount(DateTime today, int n)
        {
            //loginsTodayThatInstalledNDaysAgo
            int count = 0;

            string query = String.Format(@"SELECT COUNT(DISTINCT(logins.UserId)) AS count
                                            FROM  {2} AS logins
                                            WHERE logins.UserId IN (
                                                SELECT COUNT(DISTINCT(installs.UserId))
                                                FROM {2} AS installs
                                                WHERE installs.InstallDateRecord = 1
                                                AND DATE(installs.LoginTimestamp) = SUBDATE('{1}', INTERVAL {0} DAY)
                                            )
                                            AND DATE(logins.LoginTimestamp) = '{1}';",
                                            n,
                                            today.Date.ToString("yyyy/MM/dd"),
                                            USER_SESSION_META_TABLE);
            try
            {
                DataTable result = DBManager.Instance.Query(Datastore.Monitoring, query);
                if (result.Rows.Count > 0)
                {
                    object c = result.Rows[0][0];
                    count = Convert.ToInt32(c);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }
            return count;
        }

        public int GetDayNInstallsCount(DateTime today, int n)
        {
            int count = 0;

            string query = String.Format(@"SELECT COUNT(DISTINCT(installs.UserId))
                                            FROM {2} AS installs
                                            WHERE installs.InstallDateRecord = 1
                                            AND DATE(installs.LoginTimestamp) = SUBDATE('{1}', INTERVAL {0} DAY)",
                                            n,
                                            today.Date.ToString("yyyy/MM/dd"),
                                            USER_SESSION_META_TABLE);

            try
            {
                DataTable result = DBManager.Instance.Query(Datastore.Monitoring, query);
                if (result.Rows.Count > 0)
                {
                    object c = result.Rows[0][0];
                    count = Convert.ToInt32(c);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }
            return count;
        }
        public void RecordLatestLogins()
        {

            DateTime LatestRecord = UserSessions.Instance.GetLastLoginRecordTimestamp();
            if (LatestRecord == DateTime.MinValue)
            {
                LatestRecord = DateTime.UtcNow.AddDays(-1).Date;
            }
            HashSet<Login> logins = UserSessions.Instance.ExtractLatestLogins(LatestRecord);

            if (logins.Count > 0)
            {
                List<string> queries = new List<string>();
                List<string> processedUsers = new List<string>();

                foreach (List<Login> userBatch in logins.Batch<Login>(500))
                {
                    string q = String.Format(@"INSERT INTO {1}
                            (`UserId`,
                            `GameId`,
                            `UserSessionId`,
                            `Platform`,
                            `LoginTimestamp`,
                            `LogoffTimestamp`,
                            `SessionLength`,
                            `InstallDateRecord`,
                            `RetentionCohortType`,
                            `RecordDate`)
                        VALUES {0};", DatabaseUtilities.instance.GenerateInsertValues<Login>(userBatch), USER_SESSION_META_TABLE);
                    try
                    {
                        int result = -1;
                        MoniverseResponse response = new MoniverseResponse()
                        {
                            Status = "unsent",
                            TimeStamp = DateTime.UtcNow
                        };

                        lock (MoniverseBase.ConsoleWriterLock)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Logger.Instance.Info("--------------------------------------");
                            Logger.Instance.Info(@"   Beginning Insert of Login Batch   ");
                            Logger.Instance.Info("--------------------------------------");
                            Logger.Instance.Info("");
                        }
                        Retention.Service(Service =>
                        {
                            response = Service.Insert(new MoniverseRequest()
                            {
                                TaskName = "Insert of Login Batch ",
                                Task = q,
                                TimeStamp = DateTime.UtcNow
                            });
                        });
                        //esult = DBManager.Instance.Insert(Datastore.Monitoring, q);
                        lock (MoniverseBase.ConsoleWriterLock)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Logger.Instance.Info("--------------------------------------");
                            Logger.Instance.Info(@"       Login Batch success");
                            Logger.Instance.Info("--------------------------------------");
                            Logger.Instance.Info("");
                            Logger.Instance.Info("Done inserting Retention Row Data.");
                            Logger.Instance.Info(String.Format("success!! {0}", result));
                            Logger.Instance.Info("");
                            Console.ResetColor();
                        }
                        if (result > 0)
                        {
                            foreach (Login user in userBatch)
                                processedUsers.Add(user.UserId);
                        }
                        else
                        {
                            Logger.Instance.Info(String.Format("There was a problem with adding {0} users for {1} - {0}", userBatch.Count, LatestRecord.ToString(), DateTime.UtcNow.ToString()));
                        }

                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Logger.Instance.Info(e.Message);
                        Console.ResetColor();
                    }

                }

                Logger.Instance.Info(String.Format("{0} Added to UserSessionMeta since {1}", logins.Count(), LatestRecord.ToString()));
            }
            else
            {
                Logger.Instance.Info("No Logins retrieved from Keen");
            }
        }

        public void RecordLoginsFromDay(DateTime datePassedIn)
        {
            DateTime datetime = datePassedIn.AddDays(-1);
            string countsQ = String.Format("Select COUNT(LoginTimestamp) from {1} where RecordDate = DATE('{0}')", datetime.ToString("yyyy/MM/dd HH:mm:ss"), USER_SESSION_META_TABLE);
            int r = DBManager.Instance.QueryForCount(Datastore.Monitoring, countsQ);
            if (r == 0)
            {
                HashSet<Login> logins = UserSessions.Instance.ExtractLoginsOnDay(datePassedIn);
                Logger.Instance.Info(String.Format("{1} : {0} Raw Keen User Logins", datetime.Date.ToString(), logins.Count));
                if (logins.Count > 0)
                {

                }
                else
                {
                    Logger.Instance.Info("No Logins retrieved from Keen");
                }
            }
            else
            {
                Logger.Instance.Info(String.Format("{1} Already Processed: {0} logins in DB", r, datetime.ToString()));
            }
        }

    }

    public class RetentionReport
    {
        protected virtual string USER_SESSION_META_TABLE { get { return "UserSessionMeta"; } }
        protected virtual string RETENTION_RETURNER_VIEW_TABLE { get { return "Retention"; } }

        public DateTime Date { get; set; }
        public int Logins { get; set; }
        public int Installs { get; set; }
        public RetentionRow GeneralRetention { get; set; }
        public ReturnerBuckets CohortRetention { get; set; }
        public int WAU { get; set; }
        public int reacts { get; set; }
        public int newUsers { get; set; }
        public int continues { get; set; }
        public RetentionReport(DateTime ReportDate)
        {
            Date = ReportDate;
        }

        public void Process()
        {
            Installs = GetInstalls();
            Logins = GetLogins();
            GeneralRetention = RetentionRow.Get(Date);
            GeneralRetention.installsOnThisDay = Installs;
            GeneralRetention.loginsOnThisDay = Logins;
            GeneralRetention.Process();

            CohortRetention = new ReturnerBuckets(Date).Get();
            WAU = CohortRetention.GetWAU();
            reacts = CohortRetention.RURR.CountPreviousWeek;
            newUsers = CohortRetention.NURR.CountPreviousWeek;
            continues = CohortRetention.CURR.CountPreviousWeek;
            Save();    
        }

        protected void Save()
        {
            StringBuilder query = new StringBuilder();
            query.AppendFormat("INSERT INTO {0}", RETENTION_RETURNER_VIEW_TABLE);
            query.AppendFormat(@"(Date, NewUsers, Logins, WAU, NUR, CUR, RUR, NewUserCohort, ContinuingUsersCohort, ReactivatedUsersCohort, NURR, CURR, RURR, "
                + GeneralRetention.GenerateDayString() +
                ") VALUES ('{0}','{1}', '{2}','{3}','{4}', '{5}', '{6}', '{7}', '{8}','{9}', '{10}', '{11}', '{12}'",
                Date.ToString("yyyy-MM-dd 00:00:00"), 
                Installs, 
                Logins,
                WAU,
                CohortRetention.NURR.RelevantMetricProcessedCount,
                CohortRetention.CURR.RelevantMetricProcessedCount,
                CohortRetention.RURR.RelevantMetricProcessedCount,
                newUsers,
                continues,
                reacts,
                CohortRetention.NURR.Percentage,
                CohortRetention.CURR.Percentage,
                CohortRetention.RURR.Percentage
                );

            for (int i = 1; i < GeneralRetention.days.Length; i++)
            {
                query.AppendFormat(",'{0}'", GeneralRetention.days[i]);
            }
            query.Append(") ON DUPLICATE KEY UPDATE ");
            for (int idx = 1; idx < GeneralRetention.days.Length - 1; idx++)
            {
                GeneralRetention.canHavePercent(idx);
                {
                    query.AppendFormat("Day{0} = {1},", idx, GeneralRetention.days[idx]);
                }
            }
            query.Length--;

            try
            {
                string q = query.ToString();
                MoniverseResponse response = new MoniverseResponse()
                {
                    Status = "unsent",
                    TimeStamp = DateTime.UtcNow
                };

                lock (MoniverseBase.ConsoleWriterLock)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info("Beginning Insert of Retention Row Batch");
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info("");
                }
                //Retention.Service(Service => Service.Insert(new MoniverseRequest()
                //{
                //    TaskName = "Insert Retention Row Batch",
                //    Task = q,
                //    TimeStamp = DateTime.UtcNow
                //}));
                int result = DBManager.Instance.Insert(Datastore.Monitoring, query.ToString());

                lock (MoniverseBase.ConsoleWriterLock)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info("Insert Retention Row Batch success");
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info("");
                    Logger.Instance.Info("Done inserting Retention Row Data.");
                    Logger.Instance.Info(String.Format("success!! {0}", response.Status));
                    Logger.Instance.Info("");
                    Console.ResetColor();
                }

            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
                //Logger.Instance.Error(ex.Message);
            }

        }


        protected bool RecordExists()
        {
            string query = String.Format(@"SELECT * FROM {0} WHERE Date = '{1}'", "Retention", this.Date);

            int count = DBManager.Instance.QueryForCount(Datastore.Monitoring, query);

            return (count > 0);
        }
        public int GetInstalls()
        {
            int count = 0;
            string query = String.Format("select count(distinct(UserId)) from {1} as fl where DATE(fl.LoginTimestamp) like DATE('{0}') AND InstallDateRecord = 1", Date.ToString("yyyy/MM/dd HH:mm:ss"), USER_SESSION_META_TABLE);

            try
            {
                DataTable result = DBManager.Instance.Query(Datastore.Monitoring, query);
                object r = result.Rows[0][0];
                count = Convert.ToInt32(r);
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }

            return count;
        }
        public int GetLogins()
        {
            int count = 0;
            string query = String.Format("select count(distinct(UserId)) from {1} as l where DATE(l.LoginTimestamp) = DATE('{0}') and InstallDateRecord = 0;", Date.ToString("yyyy/MM/dd HH:mm:ss"), USER_SESSION_META_TABLE);

            try
            {
                DataTable result = DBManager.Instance.Query(Datastore.Monitoring, query);
                object r = result.Rows[0][0];
                count = Convert.ToInt32(r);
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }
            return count;
        }



    }
}
