using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Playverse.Data;
using Moniverse.Service;
using Utilities;
using System.Diagnostics;
using NUnit.Framework;
using System.Security.Cryptography;
using Moniverse.Contract;
using System.Globalization;


namespace Moniverse.Test
{
    public static class BucketingSeedData
    {
        public static DateTime Date {
            get { return new DateTime(2015, 01, 29); }
        }
    }

    public class TestRetentionVals : ReturnerBuckets
    {
        public TestRetentionVals(DateTime date) : base(date)
        {
        }

        public override string USER_SESSION_META_TABLE
        {
            get
            {
                return "RetentionBucketValidation";
            }
        }

        public override string RETENTION_RETURNER_VIEW_TABLE
        {
            get
            {
                return "Mock_Retention";
            }
        }
    }

    public class TestReturners : ReturningRetention {
        protected override string USER_SESSION_META_TABLE
        {
            get
            {
                return "Mock_UserSessionMeta";
            }
        }
    }
    public class TestRetentionCalcs : ReturningRetention
    {
        protected override string USER_SESSION_META_TABLE
        {
            get
            {
                return "RetentionBucketValidation";
            }
        }
    }

    [TestFixture]
    public class Bucketing
    {
        #region configuration
        protected const string ValidationFile = @"c:\users\public\moniversetests\ncr\Validation.csv";
        protected const string TestData = @"c:\users\public\moniversetests\ncr\CohortsTestData.csv";
        protected const string RetentionValidation = @"c:\users\public\moniversetests\ncr\NCRRetentionValidation.csv";
        
        protected string USER_SESSION_META_TABLE { get { return "Mock_UserSessionMeta"; } }
        
        protected string VALIDATION_TABLE { get { return "RetentionBucketValidation"; } }
        protected string CALC_VALIDATION { get { return "CalcValidation"; } }

        public new static Bucketing Instance = new Bucketing();

        [SetUp]
        public void AddSampleRetentionData()
        {

            //Put into the mock login table
            CreateValidationTable();
            InsertValidationData(LoadValidationData());

            CreateTestUserSessionsTable();
            InsertTestUserSessions(LoadTestUserSessions());
            CreateCalculationTable();
            CreateRetentionTable();
        }

        public void CreateRetentionTable()
        {
            string query = string.Format(@"drop table if exists {0}; create table {0} like Retention;", "Mock_Retention");
            DBManager.Instance.Query(Datastore.Monitoring, query);
        }

        public void CreateCalculationTable()
        {
            string dropTable = String.Format("DROP TABLE  IF EXISTS {0};", CALC_VALIDATION);
            StringBuilder builder = new StringBuilder(String.Format("CREATE TABLE {0} (", CALC_VALIDATION));

            List<string[]> results = CSVReader.instance.GetData(RetentionValidation, '\n', ',');
            List<string> inserts = new List<string>();

            string[] cols = results.Last();
            StringBuilder insertBuilder = new StringBuilder(string.Format("INSERT INTO {0} (", CALC_VALIDATION));
            foreach (string str in cols)
            {
                if (str == "Date")
                {
                    builder.Append("Date datetime,");
                    insertBuilder.Append(str + ",");
                }
                else
                {
                    builder.AppendFormat("{0} DOUBLE(10, 2),", str);
                    insertBuilder.Append(str + ",");
                }
            }
            results.Remove(cols);
            builder.Length--;
            insertBuilder.Length--;
            builder.Append(");");
            insertBuilder.Append(") VALUES ");

            string InsertTemplate = insertBuilder.ToString();
            foreach (string[] arr in results)
            {   

                StringBuilder i = new StringBuilder("(");
                foreach (string s in arr)
                {
                    if (String.IsNullOrEmpty(s) || s.Contains("n/a"))
                    {
                        i.Append("NULL,");
                        continue;
                    }else if(s.Contains("."))
                    {
                        float value = float.Parse(s, CultureInfo.InvariantCulture.NumberFormat);
                        i.AppendFormat("{0} ,", value);
                        continue;
                    }
                    else if(s.Contains("-"))
                    {
                        i.AppendFormat("'{0}' ,", s);
                    }
                    else
                    {
                        i.AppendFormat("{0} ,", s);
                    }

                }
                i.Length--;
                i.Append(")");
                inserts.Add(i.ToString());
            }
            Console.WriteLine(builder.ToString());
            string q = dropTable + builder.ToString() + InsertTemplate + string.Join(",", inserts.ToArray());
            Console.WriteLine(q);
            DBManager.Instance.Query(Datastore.Monitoring, q);
        }

        [TearDown]
        public void RemoveSampleRetentionData()
        {
            CreateValidationTable();
            CreateTestUserSessionsTable();
        }
        #endregion

        #region tests
        //public void 
        [TestCase]
        public void TestContinuingBucketing()
        {
            int ContCount = 0;  
            //what date to run
            //RetentionReport report = new RetentionReport();
            DataTable ValidationTable = DBManager.Instance.Query(Datastore.Monitoring,
                String.Format(@"SELECT * FROM {0};", VALIDATION_TABLE));

            DataTable TestData = DBManager.Instance.Query(Datastore.Monitoring,
                String.Format(@"SELECT * FROM {0};", USER_SESSION_META_TABLE));

            int continuing = ValidationTable.AsEnumerable()
                .Where(y => Convert.ToInt32(y["RetentionCohortType"].ToString()) == 1).Count();

            if (ValidationTable.Rows.Count > 0 && TestData.Rows.Count > 0)
            {
                List<TrackedUserOccurance> list = new List<TrackedUserOccurance>();
                foreach (DataRow row in TestData.Rows)
                {
                    Login login = new Login()
                    {
                        UserId = row["UserId"].ToString(),
                        LoginTimestamp = Convert.ToDateTime(row["LoginTimestamp"]),
                        InstallDateRecord = Convert.ToInt32(row["InstallDateRecord"].ToString()),
                        UserSessionId = row["UserSessionId"].ToString()
                    };

                    TestReturners tester = new TestReturners();

                    TrackedUserOccurance occurance = tester.DetermineUserType(login);
                     if (occurance.CohortType == RetentionCohortType.ContinuingUser)
                    {
                        ContCount++;
                    }
                }
                Debug.Assert(continuing == ContCount, String.Format("cont <> a: {0} => e: {1}", ContCount, continuing));
            }
        }

        [TestCase]
        public void TestReactsBucketing()
        {
            int ReactCount = 0;
            //what date to run
            //RetentionReport report = new RetentionReport();
            DataTable ValidationTable = DBManager.Instance.Query(Datastore.Monitoring,
                String.Format(@"SELECT * FROM {0};", VALIDATION_TABLE));

            DataTable TestData = DBManager.Instance.Query(Datastore.Monitoring,
                String.Format(@"SELECT * FROM {0};", USER_SESSION_META_TABLE));

            int reacts = ValidationTable.AsEnumerable()
                .Where(y => Convert.ToInt32(y["RetentionCohortType"].ToString()) == 2).Count();

            if (ValidationTable.Rows.Count > 0 && TestData.Rows.Count > 0)
            {
                List<TrackedUserOccurance> list = new List<TrackedUserOccurance>();
                foreach (DataRow row in TestData.Rows)
                {
                    Login login = new Login()
                    {
                        UserId = row["UserId"].ToString(),
                        LoginTimestamp = Convert.ToDateTime(row["LoginTimestamp"]),
                        InstallDateRecord = Convert.ToInt32(row["InstallDateRecord"].ToString()),
                        UserSessionId = row["UserSessionId"].ToString()
                    };

                    TestReturners tester = new TestReturners();

                    TrackedUserOccurance occurance = tester.DetermineUserType(login);
                    if (occurance.CohortType == RetentionCohortType.ReactivatedUser)
                    {
                        ReactCount++;
                        list.Add(occurance);
                        //Console.WriteLine("react: {0} : {1}" , occurance.UserId, occurance.Date);
                    }
                }
                Debug.Assert(reacts == ReactCount, String.Format("reacts <> a: {0} => e: {1}", ReactCount, reacts));
            }
        }
        [TestCase]
        public void TestNewUsersBucketing()
        {

            int userCount = 0;
            //what date to run
            //RetentionReport report = new RetentionReport();
            DataTable ValidationTable = DBManager.Instance.Query(Datastore.Monitoring,
                String.Format(@"SELECT * FROM {0};", VALIDATION_TABLE));

            DataTable TestData = DBManager.Instance.Query(Datastore.Monitoring,
                String.Format(@"SELECT * FROM {0};", USER_SESSION_META_TABLE));

            int newUsers = ValidationTable.AsEnumerable()
                .Where(y => Convert.ToInt32(y["RetentionCohortType"].ToString()) == 0).Count();

            if (ValidationTable.Rows.Count > 0 && TestData.Rows.Count > 0)
            {
                List<TrackedUserOccurance> list = new List<TrackedUserOccurance>();
                foreach (DataRow row in TestData.Rows)
                {
                    Login login = new Login()
                    {
                        UserId = row["UserId"].ToString(),
                        LoginTimestamp = Convert.ToDateTime(row["LoginTimestamp"]),
                        InstallDateRecord = Convert.ToInt32(row["InstallDateRecord"].ToString()),
                        UserSessionId = row["UserSessionId"].ToString()
                    };

                    TestReturners tester = new TestReturners();

                    TrackedUserOccurance occurance = tester.DetermineUserType(login);

                    if (occurance.CohortType == RetentionCohortType.NewUser)
                    {
                        userCount++;
                    }
                }
                Debug.Assert(newUsers == userCount, String.Format("new users <> a: {0} => e: {1}", userCount, newUsers));
            }
        }
        [TestCase]
        public void TestOneRCalc()
        {

            //what date to run;
            Bucketing testBucket = new Bucketing();
            TestRetentionCalcs tester = new TestRetentionCalcs();
            int ReactCount = 0;

            DataTable ValidationTable = DBManager.Instance.Query(Datastore.Monitoring,
                String.Format(@"SELECT * FROM {0};", VALIDATION_TABLE));

            DataTable computedValues = DBManager.Instance.Query(Datastore.Monitoring,
                String.Format(@"SELECT * FROM {0} order by Date asc;", CALC_VALIDATION));

            foreach (DataRow row in computedValues.AsEnumerable())
            {
                DateTime process = DateTime.Parse(row["Date"].ToString());
                
                ReturnerBuckets buckets = new TestRetentionVals(process).Get();
                ReturnerRetentionDataPoints curr = buckets.CURR;
                //Console.ReadKey();

            }

            //assert that actual is same as expected
            //Debug.Assert(ContinuingUsersLastWeekExpected == CURRData.CountPreviousWeek, string.Format("Install - A:{0} => E:{1}", CURRData.CountPreviousWeek, ContinuingUsersLastWeekExpected));
            //Debug.Assert(UsersThisWeekWhoWereContinuingLastWeek == CURRData.ReturningContinuing, string.Format("Played - A:{0} => E:{1}", CURRData.ReturningContinuing, UsersThisWeekWhoWereContinuingLastWeek));
            //Debug.Assert(expectedCurr == CURRData.Percentage, string.Format("CURR - A:{0} => E:{1}", CURRData.Percentage, expectedCurr));

        }
        [TestCase]
        public void TestTwoRCalc()
        {

            //what date to run
            DateTime toRun = new DateTime(2015, 1, 29);
            //RetentionReport report = new RetentionReport();
            Bucketing testBucket = new Bucketing();
            
            
            
            //assert that actual is same as expected
            //Debug.Assert(ContinuingUsersLastWeekExpected == CURRData.CountPreviousWeek, string.Format("Install - A:{0} => E:{1}", CURRData.CountPreviousWeek, ContinuingUsersLastWeekExpected));
            //Debug.Assert(UsersThisWeekWhoWereContinuingLastWeek == CURRData.ReturningContinuing, string.Format("Played - A:{0} => E:{1}", CURRData.ReturningContinuing, UsersThisWeekWhoWereContinuingLastWeek));
            //Debug.Assert(expectedCurr == CURRData.Percentage, string.Format("CURR - A:{0} => E:{1}", CURRData.Percentage, expectedCurr));

        }

        #endregion

        #region helpers
        protected void InsertTestUserSessions(HashSet<Login> newUsers)
        {
            StringBuilder query = new StringBuilder();
            string MockGameId = "0-00000000000000000000000000000000";

            string mockPlatform = "Windows";
            query.AppendFormat(@"insert into {0} 
                                (`ID`,
                                `UserId`,
                                `GameId`,
                                `UserSessionId`,
                                `Platform`,
                                `LoginTimestamp`,
                                `LogoffTimestamp`,
                                `SessionLength`,
                                `InstallDateRecord`,
                                `RetentionCohortType`,
                                `RecordDate`) values ", USER_SESSION_META_TABLE);
            query.Append(string.Join(",", newUsers.Select(x =>
            {
                return string.Format("(null, '{0}', '{1}','{2}','{3}','{4}','{5}',{6},{7},{8},'{9}')", x.UserId, MockGameId, x.UserSessionId, mockPlatform, x.LoginTimestamp.ToString("yyyy-MM-dd"), x.LoginTimestamp.ToString("yyyy-MM-dd"), -1, x.InstallDateRecord, -1, x.LoginTimestamp.ToString("yyyy-MM-dd"));

            })));

            DBManager.Instance.Insert(Datastore.Monitoring, query.ToString());
        }


        protected void InsertValidationData(HashSet<Login> loginUsers)
        {
            StringBuilder query = new StringBuilder();
            string MockGameId = "0-00000000000000000000000000000000";
            string mockPlatform = "Windows";

            query.AppendFormat(@"insert into {0} 
                                (`ID`,
                                `UserId`,
                                `GameId`,
                                `UserSessionId`,
                                `Platform`,
                                `LoginTimestamp`,
                                `LogoffTimestamp`,
                                `SessionLength`,
                                `InstallDateRecord`,
                                `RetentionCohortType`,
                                `RecordDate`) values ", VALIDATION_TABLE);
            
            query.Append(string.Join(",", loginUsers.Select(x =>
            {
                return string.Format("(null, '{0}', '{1}','{2}','{3}','{4}','{5}',{6},{7},{8},'{9}')", x.UserId, MockGameId, x.UserSessionId, mockPlatform, x.LoginTimestamp.ToString("yyyy-MM-dd"), x.LoginTimestamp.ToString("yyyy-MM-dd"), -1, x.InstallDateRecord, (int)x.RetentionCohortType, x.LoginTimestamp.ToString("yyyy-MM-dd"));

            })));
            DBManager.Instance.Insert(Datastore.Monitoring, query.ToString());
        }

        protected HashSet<Login> LoadTestUserSessions()
        {
            List<string[]> results = CSVReader.instance.GetData(ValidationFile, '\n', ',');
            HashSet<Login> NewUsers = new HashSet<Login>();
            foreach (string[] line in results)
            {
                if (line[0] == "ID" || line[1] == "")
                {
                    continue;
                }
                Login rawline = new Login()
                {
                    UserId = line[1],
                    GameId = line[2],
                    UserSessionId = line[3],
                    Platform = line[4],
                    LoginTimestamp = Convert.ToDateTime(line[5]).ToUniversalTime(),
                    LogoffTimestamp = Convert.ToDateTime(line[5]).ToUniversalTime(),
                    SessionLength = 0,
                    InstallDateRecord = Convert.ToInt32(line[8]),
                    RetentionCohortType = RetentionCohortType.Unprocessed,
                    RecordDate = Convert.ToDateTime(line[5]).Date.ToString("yyyy-MM-dd")

                };
                if (NewUsers.Add(rawline) == false)
                {
                    Console.WriteLine("{0} user not added", rawline.UserSessionId);
                }
            }
            Console.WriteLine("user count {0}", NewUsers.Count);
            return NewUsers;
        }

        //public static string GetUniqueKey(int maxSize)
        //{
        //    char[] chars = new char[62];
        //    chars =
        //    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        //    byte[] data = new byte[1];
        //    using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
        //    {
        //        crypto.GetNonZeroBytes(data);
        //        data = new byte[maxSize];
        //        crypto.GetNonZeroBytes(data);
        //    }
        //    StringBuilder result = new StringBuilder(maxSize);
        //    foreach (byte b in data)
        //    {
        //        result.Append(chars[b % (chars.Length)]);
        //    }
        //    return result.ToString();
        //}
        protected HashSet<Login> LoadValidationData()
        {
            List<string[]> results = CSVReader.instance.GetData(ValidationFile, '\n', ',');
            HashSet<Login> Logins = new HashSet<Login>();
            foreach (string[] line in results)
            {
                if (line[0] == "ID" || line[1] == "")
                {
                    continue;
                }
                RetentionCohortType type = RetentionCohortType.Unprocessed;
                if (!String.IsNullOrEmpty(line[9]))
                {
                    string cohort = line[9];
                    switch (cohort)
                    {
                        case "n":
                            type = RetentionCohortType.NewUser;
                            break;
                        case "r":
                            type = RetentionCohortType.ReactivatedUser;
                            break;
                        case "c":
                        default:
                            type = RetentionCohortType.ContinuingUser;
                            break;


                    }
                }

                Login rawline = new Login()
                {
                    UserId = line[1],
                    GameId = line[2],
                    UserSessionId = line[3],
                    Platform = line[4],
                    LoginTimestamp = Convert.ToDateTime(line[5]).ToUniversalTime(),
                    LogoffTimestamp = Convert.ToDateTime(line[5]).ToUniversalTime(),
                    SessionLength = 0,
                    InstallDateRecord = Convert.ToInt32(line[8]),
                    RetentionCohortType = type,
                    RecordDate = Convert.ToDateTime(line[5]).Date.ToString("yyyy-MM-dd")

                };
                if (Logins.Add(rawline) == false)
                {
                    Console.WriteLine(String.Format("{0} was not added", rawline.UserId));
                    //do something about duplicates from keen call becuse something is wrong.
                    // "badness detected" -- PJ

                }

            }
            return Logins;
        }

        protected void CreateValidationTable()
        {
            string query = string.Format(@"drop table if exists {0}; create table {0} like UserSessionMeta;", VALIDATION_TABLE);
            DBManager.Instance.Query(Datastore.Monitoring, query);
        }

        protected void CreateTestUserSessionsTable()
        {
            string query = string.Format(@"drop table if exists {0}; create table {0} like UserSessionMeta;", USER_SESSION_META_TABLE);
            DBManager.Instance.Query(Datastore.Monitoring, query);
        }
        #endregion

    }
}

