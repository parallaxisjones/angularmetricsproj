//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Data;
//using Playverse.Data;
//using Moniverse.Service;
//using Utilities;
//using System.Diagnostics;
//using NUnit.Framework;
//using System.Security.Cryptography;
//using Moniverse.Contract;


//namespace Moniverse.Test
//{
    
//    [TestFixture]
//    public class MockReturningRetention : ReturningRetention
//    {
//        #region configuration
//        protected override string USER_SESSION_META_TABLE { get { return "Mock_UserSessionMeta"; } }

//        public new static MockReturningRetention Instance = new MockReturningRetention();

//        [SetUp]
//        public void AddSampleRetentionData()
//        {
//            //Put into the mock login table
//            CreateSampleRetentionLoginTable();
//            InsertMockNewUsers(MockGetInstalls());

//            //Put into the mock install table

//            InsertMockLogins(MockGetLogins());
//        }
//        [TearDown]
//        public void RemoveSampleRetentionData()
//        {
//            //CreateSampleRetentionFirstLoginTable();
//            CreateSampleRetentionLoginTable();
//        }        
//        #endregion

//        #region tests
//        //public void 
//        [TestCase]
//        public void TestRURRCalculation()
//        {
//            //what date to run
//            DateTime toRun = new DateTime(2015, 1, 29);

//            //Get expected values
//            decimal expectedCurr = 46.00M;
//            int ContinuingUsersLastWeekExpected = 13;
//            int UsersThisWeekWhoWereContinuingLastWeek = 6;

//            ReturnerRetentionDataPoints CURRData = CalculateCURR(toRun);

//            //assert that actual is same as expected
//            Debug.Assert(ContinuingUsersLastWeekExpected == CURRData.CountPreviousWeek, string.Format("Install - A:{0} => E:{1}", CURRData.CountPreviousWeek, ContinuingUsersLastWeekExpected));
//            Debug.Assert(UsersThisWeekWhoWereContinuingLastWeek == CURRData.ReturningContinuing, string.Format("Played - A:{0} => E:{1}", CURRData.ReturningContinuing, UsersThisWeekWhoWereContinuingLastWeek));
//            Debug.Assert(expectedCurr == CURRData.Percentage, string.Format("CURR - A:{0} => E:{1}", CURRData.Percentage, expectedCurr));

//            ////what date to run
//            //DateTime toRun = new DateTime(2015, 1, 29);

//            ////Get expected values
//            //int installedMoreThan15DaysAgoExpected = 20;
//            //decimal expectedCurr = 46.00M;
//            //int played8to14InstalledMoreThan15Expected = 10;

//            ////add in sample data to mock retention login
//            //AddSampleRetentionData();

//            ////Run
//            //int installedMoreThan15DaysAgoActual = GetInstalledMoreThan15DaysAgo(toRun);
//            //int played8to14InstalledMoreThan15Actual = GetPlayed8to14InstalledMoreThan15(toRun);
//            //decimal actualCurr = CalculateCurrFromDate(toRun);

//            ////assert that actual is same as expected
//            //Debug.Assert(installedMoreThan15DaysAgoExpected == installedMoreThan15DaysAgoActual, string.Format("Install - A:{0} => E:{1}", installedMoreThan15DaysAgoActual, installedMoreThan15DaysAgoExpected));
//            //Debug.Assert(played8to14InstalledMoreThan15Expected == played8to14InstalledMoreThan15Actual, string.Format("Played - A:{0} => E:{1}", played8to14InstalledMoreThan15Actual, played8to14InstalledMoreThan15Expected));
//            //Debug.Assert(expectedCurr == actualCurr, string.Format("CURR - A:{0} => E:{1}", actualCurr, expectedCurr));

//            ////Teardown

//            ////Clean up any sample data
//            //RemoveSampleRetentionData();
//        }
//        [TestCase]
//        public void TestNURRCalculation()
//        {

//            //what date to run
//            DateTime toRun = new DateTime(2015, 1, 29);

//            //Get expected values
//            decimal expectedCurr = 46.00M;
//            int ContinuingUsersLastWeekExpected = 13;
//            int UsersThisWeekWhoWereContinuingLastWeek = 6;

//            ReturnerRetentionDataPoints CURRData = CalculateCURR(toRun);

//            //assert that actual is same as expected
//            Debug.Assert(ContinuingUsersLastWeekExpected == CURRData.CountPreviousWeek, string.Format("Install - A:{0} => E:{1}", CURRData.CountPreviousWeek, ContinuingUsersLastWeekExpected));
//            Debug.Assert(UsersThisWeekWhoWereContinuingLastWeek == CURRData.ReturningContinuing, string.Format("Played - A:{0} => E:{1}", CURRData.ReturningContinuing, UsersThisWeekWhoWereContinuingLastWeek));
//            Debug.Assert(expectedCurr == CURRData.Percentage, string.Format("CURR - A:{0} => E:{1}", CURRData.Percentage, expectedCurr));

//            ////what date to run
//            //DateTime toRun = new DateTime(2015, 1, 29);

//            ////Get expected values
//            //int installedMoreThan15DaysAgoExpected = 20;
//            //decimal expectedCurr = 46.00M;
//            //int played8to14InstalledMoreThan15Expected = 10;

//            ////add in sample data to mock retention login
//            //AddSampleRetentionData();

//            ////Run
//            //int installedMoreThan15DaysAgoActual = GetInstalledMoreThan15DaysAgo(toRun);
//            //int played8to14InstalledMoreThan15Actual = GetPlayed8to14InstalledMoreThan15(toRun);
//            //decimal actualCurr = CalculateCurrFromDate(toRun);

//            ////assert that actual is same as expected
//            //Debug.Assert(installedMoreThan15DaysAgoExpected == installedMoreThan15DaysAgoActual, string.Format("Install - A:{0} => E:{1}", installedMoreThan15DaysAgoActual, installedMoreThan15DaysAgoExpected));
//            //Debug.Assert(played8to14InstalledMoreThan15Expected == played8to14InstalledMoreThan15Actual, string.Format("Played - A:{0} => E:{1}", played8to14InstalledMoreThan15Actual, played8to14InstalledMoreThan15Expected));
//            //Debug.Assert(expectedCurr == actualCurr, string.Format("CURR - A:{0} => E:{1}", actualCurr, expectedCurr));

//            ////Teardown

//            ////Clean up any sample data
//            //RemoveSampleRetentionData();
//        }
//        [TestCase]
//        public void TestCurrCalculation()
//        {

//            //what date to run
//            DateTime toRun = new DateTime(2015, 1, 29);

//            //Get expected values
//            decimal expectedCurr = 46.00M;
//            int ContinuingUsersLastWeekExpected = 13;
//            int UsersThisWeekWhoWereContinuingLastWeek = 6;

//            ReturnerRetentionDataPoints CURRData = CalculateCURR(toRun);

//            //assert that actual is same as expected
//            Debug.Assert(ContinuingUsersLastWeekExpected == CURRData.CountPreviousWeek, string.Format("Install - A:{0} => E:{1}", CURRData.CountPreviousWeek, ContinuingUsersLastWeekExpected));
//            Debug.Assert(UsersThisWeekWhoWereContinuingLastWeek == CURRData.ReturningContinuing, string.Format("Played - A:{0} => E:{1}", CURRData.ReturningContinuing, UsersThisWeekWhoWereContinuingLastWeek));
//            Debug.Assert(expectedCurr == CURRData.Percentage, string.Format("CURR - A:{0} => E:{1}", CURRData.Percentage, expectedCurr));

//        }

//        #endregion

//        #region helpers
//        protected void InsertMockNewUsers(HashSet<Install> newUsers)
//        {
//            StringBuilder query = new StringBuilder();
//            string MockGameId = "0-00000000000000000000000000000000";

//            string mockPlatform = "Windows";
//            query.AppendFormat(@"insert into {0} 
//                                (`ID`,
//                                `UserId`,
//                                `GameId`,
//                                `UserSessionId`,
//                                `Platform`,
//                                `LoginTimestamp`,
//                                `LogoffTimestamp`,
//                                `SessionLength`,
//                                `InstallDateRecord`,
//                                `RetentionCohortType`,
//                                `RecordDate`) values ", USER_SESSION_META_TABLE);
//            query.Append(string.Join(",", newUsers.Select(x =>
//            {
//                return string.Format("(null, '{0}', '{1}','{2}','{3}','{4}','{5}',{6},{7},{8},'{9}')", x.UserId, MockGameId, GetUniqueKey(MockGameId.Length), mockPlatform, x.LoginTimestamp.ToString("yyyy-MM-dd"), x.LoginTimestamp.ToString("yyyy-MM-dd"), -1, 1, (int)x.RetentionCohortType, x.LoginTimestamp.ToString("yyyy-MM-dd"));

//            })));

//            DBManager.Instance.Insert(Datastore.Monitoring, query.ToString());
//        }

//        protected void InsertMockLogins(HashSet<Login> loginUsers)
//        {
//            StringBuilder query = new StringBuilder();
//            string MockGameId = "0-00000000000000000000000000000000";

//            string mockPlatform = "Windows";
//            query.AppendFormat(@"insert into {0} 
//                                (`ID`,
//                                `UserId`,
//                                `GameId`,
//                                `UserSessionId`,
//                                `Platform`,
//                                `LoginTimestamp`,
//                                `LogoffTimestamp`,
//                                `SessionLength`,
//                                `InstallDateRecord`,
//                                `RetentionCohortType`,
//                                `RecordDate`) values ", USER_SESSION_META_TABLE);
//            query.Append(string.Join(",", loginUsers.Select(x =>
//            {
//                return string.Format("(null, '{0}', '{1}','{2}','{3}','{4}','{5}',{6},{7},{8},'{9}')", x.UserId, MockGameId, GetUniqueKey(MockGameId.Length), mockPlatform, x.LoginTimestamp.ToString("yyyy-MM-dd"), x.LoginTimestamp.ToString("yyyy-MM-dd"), -1, 1, (int)x.RetentionCohortType, x.LoginTimestamp.ToString("yyyy-MM-dd"));

//            })));
//            DBManager.Instance.Insert(Datastore.Monitoring, query.ToString());
//        }

//        protected HashSet<Install> MockGetInstalls()
//        {
//            List<string[]> results = CSVReader.instance.GetData(@"c:\users\public\moniversetests\UserSessionMetaTestData.csv", '\n', ',');
//            HashSet<Install> NewUsers = new HashSet<Install>();
//            foreach (string[] line in results)
//            {
//                if (line[0] == "ID" || line[1] == "")
//                {
//                    continue;
//                }
//                Install rawline = new Install()
//                {
//                    UserId = line[1],
//                    GameId = line[2],
//                    UserSessionId = line[3],
//                    Platform = line[4],
//                    LoginTimestamp = Convert.ToDateTime(line[5]).ToUniversalTime(),
//                    LogoffTimestamp = Convert.ToDateTime(line[5]).ToUniversalTime(),
//                    SessionLength = 0,
//                    InstallDateRecord = Convert.ToInt32(line[8]),
//                    RetentionCohortType = (RetentionCohortType)Convert.ToInt32(line[9]),
//                    RecordDate = Convert.ToDateTime(line[5]).Date.ToString("yyyy-MM-dd")

//                };
//                if (rawline.InstallDateRecord == 1)
//                {
//                    NewUsers.Add(rawline);
//                }
//            }
//            return NewUsers;
//        }

//        public static string GetUniqueKey(int maxSize)
//        {
//            char[] chars = new char[62];
//            chars =
//            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
//            byte[] data = new byte[1];
//            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
//            {
//                crypto.GetNonZeroBytes(data);
//                data = new byte[maxSize];
//                crypto.GetNonZeroBytes(data);
//            }
//            StringBuilder result = new StringBuilder(maxSize);
//            foreach (byte b in data)
//            {
//                result.Append(chars[b % (chars.Length)]);
//            }
//            return result.ToString();
//        }

//        protected HashSet<Login> MockGetLogins()
//        {
//            List<string[]> results = CSVReader.instance.GetData(@"c:\users\public\moniversetests\UserSessionMetaTestData.csv", '\n', ',');
//            HashSet<Login> Logins = new HashSet<Login>();
//            foreach (string[] line in results)
//            {
//                if (line[0] == "ID" || line[1] == "")
//                {
//                    continue;
//                }
//                Login rawline = new Login()
//                {
//                    UserId = line[1],
//                    GameId = line[2],
//                    UserSessionId = line[3],
//                    Platform = line[4],
//                    LoginTimestamp = Convert.ToDateTime(line[5]).ToUniversalTime(),
//                    LogoffTimestamp = Convert.ToDateTime(line[5]).ToUniversalTime(),
//                    SessionLength = 0,
//                    InstallDateRecord = Convert.ToInt32(line[8]),
//                    RetentionCohortType = (RetentionCohortType)Convert.ToInt32(line[9]),
//                    RecordDate = Convert.ToDateTime(line[5]).Date.ToString("yyyy-MM-dd")

//                };
//                if (rawline.InstallDateRecord == 0)
//                {
//                    Logins.Add(rawline);
//                }

//            }
//            return Logins;
//        }

//        protected void CreateSampleRetentionLoginTable()
//        {
//            string query = string.Format(@"drop table if exists {0}; create table {0} like UserSessionMeta;", USER_SESSION_META_TABLE);
//            DBManager.Instance.Query(Datastore.Monitoring, query);
//        }
//        #endregion
  
//    }
//}

