using System;
using System.Collections.Generic;
using Playverse.Data;
using System.Data;
using Utilities;
using System.Linq;
using Moniverse.Contract;
using Moniverse.Service;
using System.Text;
namespace Moniverse.Service
{
    public class ReturningRetention : ServiceClassBase
    {
        #region configuration
        protected virtual string USER_SESSION_META_TABLE { get { return "UserSessionMeta"; } }
        protected virtual string RETENTION_RETURNER_VIEW_TABLE { get { return "Retention_ReturnersView"; } }
        
        public ReturningRetention() : base() { }
        public static ReturningRetention Instance = new ReturningRetention();
        #endregion

        public TrackedUserOccurance DetermineUserType(Login User)
        {

            TrackedUserOccurance occurence = new TrackedUserOccurance();

            occurence.Date = User.LoginTimestamp;
            occurence.UserId = User.UserId;

            if (User.InstallDateRecord == 1)
            { // todo: if you install, quit and then play on the same day... should this be marked as n, and c?
                occurence.CohortType = RetentionCohortType.NewUser;
            }
            else if (IsReactivatedUser(User))
            {
                occurence.CohortType = RetentionCohortType.ReactivatedUser;
            }
            else
            {
                occurence.CohortType = RetentionCohortType.ContinuingUser;
            }

            return occurence;
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
        public ReturnerRetentionDataPoints CalculateCURR(DateTime ProcessDate)
        {

            ReturnerRetentionDataPoints retentionDate = new ReturnerRetentionDataPoints();
            List<string> cont = GetContinuingUserIdsfrom8to14DaysAgoRange(ProcessDate);
            List<string> returningCont = GetReturningContinuingUserIdsZeroToSixRange(ProcessDate);
            List<string> intersection = cont.Intersect(returningCont).ToList();

            if (intersection.Count > 0)
            {
                decimal percentage = Decimal.Divide(intersection.Count, cont.Count);
                percentage = Decimal.Multiply(percentage, 100);
                percentage = Decimal.Round(percentage, 2);
                percentage = Decimal.Floor(percentage);
                retentionDate.RecordDate = ProcessDate.Date;
                retentionDate.CountPreviousWeek = cont.Count;
                retentionDate.ReturningContinuing = intersection.Count;
                retentionDate.Percentage = percentage;
            }

            return retentionDate;
        }

        public List<TrackedUserOccurance> GetContinuingfrom8to14DaysAgoRange(DateTime ProcessDate)
        {
            List<TrackedUserOccurance> userList = new List<TrackedUserOccurance>();

            //have to add a day to the end of the range we're looking at if we're using the 00:00:00 midnight beginning of the range dates
            //so we can get the whole date, either we can use INTERVAL 9 DAY in the SQL statement or ADD DAY in C# DateTime land. -- PJ
            string query = String.Format(@"SELECT DISTINCT(UserId) as UserId, LoginTimestamp 
                                           FROM {0} 
                                           WHERE LoginTimestamp BETWEEN SUBDATE('{1}', INTERVAL 14 DAY) AND SUBDATE('{2}', INTERVAL 8 DAY)
                                           AND RetentionCohortType = 1  
                                           ORDER BY LoginTimestamp desc;",
                                           USER_SESSION_META_TABLE,
                                           ProcessDate.ToString("yyyy-MM-dd 00:00:00"),
                                           ProcessDate.AddDays(1).ToString("yyyy-MM-dd 00:00:00"));

            DataTable UsersTable = DBManager.Instance.Query(Datastore.Monitoring, query);
            if (UsersTable.Rows.Count > 0)
            {
                foreach (DataRow UserRecord in UsersTable.Rows)
                {
                    TrackedUserOccurance user = new TrackedUserOccurance()
                    {
                        Date = DateTime.Parse(UserRecord["LoginTimestamp"].ToString()),
                        UserId = UserRecord["UserId"].ToString(),
                        CohortType = RetentionCohortType.ContinuingUser
                    };
                    userList.Add(user);
                }
            }
            return userList;
        }
        public List<string> GetContinuingUserIdsfrom8to14DaysAgoRange(DateTime ProcessDate)
        {
            List<TrackedUserOccurance> ContinuingUsersfrom8to14DaysAgoRange = GetContinuingfrom8to14DaysAgoRange(ProcessDate);
            return ContinuingUsersfrom8to14DaysAgoRange.Select(x => x.UserId).ToList();
        }
        public List<string> GetReturningContinuingUserIdsZeroToSixRange(DateTime ProcessDate)
        {
            string query = String.Format(@"SELECT DISTINCT(UserId) 
                                           FROM {0} 
                                           WHERE LoginTimestamp BETWEEN SUBDATE('{1}', INTERVAL 6 DAY) AND '{2}'
                                           ORDER BY LoginTimestamp desc;",
                                           USER_SESSION_META_TABLE,
                                           ProcessDate.ToString("yyyy-MM-dd 00:00:00"),
                                           ProcessDate.AddDays(1).ToString("yyyy-MM-dd 00:00:00"));

            DataTable Continuingfrom8to14ThatReturned1to7 = DBManager.Instance.Query(Datastore.Monitoring, query);

            return Continuingfrom8to14ThatReturned1to7.AsEnumerable().Select(x => x.Field<string>("UserId").ToString()).ToList();
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
        public ReturnerRetentionDataPoints CalculateRURR(DateTime ProcessDate)
        {
            ReturnerRetentionDataPoints retentionDate = new ReturnerRetentionDataPoints();

            List<string> reacts = GetReactsUsersIdsfrom8to14DaysAgoRange(ProcessDate);
            List<string> lastWeek = GetReturningRurs(ProcessDate);
            List<string> intersection = reacts.Intersect(lastWeek).ToList();

            if (intersection.Count > 0)
            {
                decimal percentage = Decimal.Divide(intersection.Count, reacts.Count);
                percentage = Decimal.Multiply(percentage, 100);
                percentage = Decimal.Round(percentage, 2);
                percentage = Decimal.Floor(percentage);
                retentionDate.RecordDate = ProcessDate.Date;
                retentionDate.CountPreviousWeek = reacts.Count;
                retentionDate.ReturningContinuing = intersection.Count;
                retentionDate.Percentage = percentage;
            }

            return retentionDate;
        }
        public List<TrackedUserOccurance> GetReactsfrom8to14DaysAgoRange(DateTime ProcessDate)
        {
            List<TrackedUserOccurance> userList = new List<TrackedUserOccurance>();

            //have to add a day to the end of the range we're looking at if we're using the 00:00:00 midnight beginning of the range dates
            //so we can get the whole date, either we can use INTERVAL 9 DAY in the SQL statement or ADD DAY in C# DateTime land. -- PJ
            string query = String.Format(@"SELECT DISTINCT(UserId) as UserId, LoginTimestamp 
                                           FROM {0} 
                                           WHERE LoginTimestamp BETWEEN SUBDATE('{1}', INTERVAL 14 DAY) AND SUBDATE('{2}', INTERVAL 8 DAY)
                                           AND RetentionCohortType = 2  
                                           ORDER BY LoginTimestamp desc;",
                                           USER_SESSION_META_TABLE,
                                           ProcessDate.ToString("yyyy-MM-dd 00:00:00"),
                                           ProcessDate.AddDays(1).ToString("yyyy-MM-dd 00:00:00"));

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
            return Reactsfrom8to14DaysAgoRange.Select(x => x.UserId).ToList();
        }
        public List<string> GetReturningRurs(DateTime ProcessDate)
        {
            string query = String.Format(@"SELECT DISTINCT(UserId) 
                                           FROM {0} 
                                           WHERE LoginTimestamp BETWEEN SUBDATE('{1}', INTERVAL 6 DAY) AND '{2}'
                                           ORDER BY LoginTimestamp desc;",
                                           USER_SESSION_META_TABLE,
                                           ProcessDate.ToString("yyyy-MM-dd 00:00:00"),
                                           ProcessDate.AddDays(1).ToString("yyyy-MM-dd 00:00:00"));

            DataTable Continuingfrom8to14ThatReturned1to7 = DBManager.Instance.Query(Datastore.Monitoring, query);
            return Continuingfrom8to14ThatReturned1to7.AsEnumerable().Select(x => x.Field<string>("UserId").ToString()).ToList();
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
        public ReturnerRetentionDataPoints CalculateNURR(DateTime ProcessDate)
        {

            ReturnerRetentionDataPoints retentionDate = new ReturnerRetentionDataPoints();

            List<string> lastWeek = GetReturningNurs(ProcessDate);
            List<string> newUsers = GetNewUsersUserIdsFrom8to14DaysAgoRange(ProcessDate);
            List<string> intersection = newUsers.Intersect(lastWeek).ToList();

            if (intersection.Count > 0)
            {
                decimal percentage = Decimal.Divide(intersection.Count, newUsers.Count);
                percentage = Decimal.Multiply(percentage, 100);
                percentage = Decimal.Round(percentage, 2);
                percentage = Decimal.Floor(percentage);

                retentionDate.RecordDate = ProcessDate.Date;
                retentionDate.CountPreviousWeek = newUsers.Count;
                retentionDate.ReturningContinuing = intersection.Count;
                retentionDate.Percentage = percentage;
            }

            return retentionDate;
        }
        public List<TrackedUserOccurance> GetNewUsersfrom8to14DaysAgoRange(DateTime ProcessDate)
        {
            List<TrackedUserOccurance> userList = new List<TrackedUserOccurance>();

            //have to add a day to the end of the range we're looking at if we're using the 00:00:00 midnight beginning of the range dates
            //so we can get the whole date, either we can use INTERVAL 9 DAY in the SQL statement or ADD DAY in C# DateTime land. -- PJ
            string query = String.Format(@"SELECT DISTINCT(UserId) as UserId, LoginTimestamp 
                                           FROM {0} 
                                           WHERE LoginTimestamp BETWEEN SUBDATE('{1}', INTERVAL 14 DAY) AND SUBDATE('{2}', INTERVAL 8 DAY)
                                           AND RetentionCohortType = 0  
                                           ORDER BY LoginTimestamp desc;",
                                           USER_SESSION_META_TABLE,
                                           ProcessDate.ToString("yyyy-MM-dd 00:00:00"),
                                           ProcessDate.AddDays(1).ToString("yyyy-MM-dd 00:00:00"));

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

        public List<string> GetReturningNurs(DateTime ProcessDate)
        {
            string query = String.Format(@"SELECT DISTINCT(UserId) 
                                           FROM {0} 
                                           WHERE LoginTimestamp BETWEEN SUBDATE('{1}', INTERVAL 6 DAY) AND '{2}'
                                           ORDER BY LoginTimestamp desc;",
                                           USER_SESSION_META_TABLE,
                                           ProcessDate.ToString("yyyy-MM-dd 00:00:00"),
                                           ProcessDate.AddDays(1).ToString("yyyy-MM-dd 00:00:00"));

            DataTable Continuingfrom8to14ThatReturned1to7 = DBManager.Instance.Query(Datastore.Monitoring, query);
            return Continuingfrom8to14ThatReturned1to7.AsEnumerable().Select(x => x.Field<string>("UserId").ToString()).ToList();
        }
        #endregion
    }
}
