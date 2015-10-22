using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;
using Utilities;
using Moniverse.Contract;
using Moniverse.Service;
using Moniverse.Reader;
using Moniverse.Test;
using Playverse.Utilities;
using NUnit.Framework;
using Playverse.Data;
using System.Data;
using System.Diagnostics;
using Amib.Threading;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Globalization;
using System.Threading.Tasks;
//using Moniverse = PlayverseMonitoring.Moniverse;

namespace Moniverse.ConsoleRunner
{

    public class MockTimerState : ITimerState
    {
        public MockTimerState(int state)
        {
            _counter = state;
        }

        private int _counter;

        public int counter
        {
            get { return _counter; }
        }
    }

    class Program : MoniverseBase
    {
        #region Configurations
#if DEBUG
         public const string env = "debug";
#elif STAGING
        public const string env = "STAGING";
#elif Release
        public const string env = Release"";
#else 
        public const string env = "Release";
#endif
        public static SmartThreadPool ConsoleThreads;
         public const string KeenExtractionPath = @"D:\documents\KeenExtractions\";
         public static IAnalyticsProvider AnalyticsProvider = KeenIO.Instance;
         protected static void Setup()
         {
             ConsoleThreads = new SmartThreadPool();
             if (!isWriterRunning())
             {
                 StartWriterServer();
             }


         }

         protected static void Shutdown()
         {
             ConsoleThreads.Shutdown();
         }
        #endregion

        public static WorkQueue queue = new WorkQueue();

        public static int lastSecondLogged = 0;

        static void SandBox(string[] args)
         {
             GameMonitoringConfig game = Games.Instance.GetMonitoredGames().FirstOrDefault(x => x.ShortTitle == "DD2");
             MoniverseNotification notification = UserNotification.TestNotification(game);
             List<Notifier> NotifierList;

             foreach (MoniverseNotification note in UserNotification.Instance.GetNotificationsForGame(Games.EMPTYGAMEID, MessageTopic.All)) {
                 Console.WriteLine(note.Message);
             }
             Console.ReadKey();


             //if(!UserNotification.RunningNotifications.TryGetValue(game.ShortTitle, out NotifierList)){
             //    NotifierList = new List<Notifier>();
             //    UserNotification.RunningNotifications.TryAdd(game.ShortTitle, NotifierList);
             //}
             //if (!string.IsNullOrEmpty(notification.Subject))
             //{

             //        if (NotifierList.Exists(x => x.NotificationId == notification.Id))
             //        {
             //            return;
             //        }
             //        else
             //        {

             //           if (UserNotification.RunningNotifications.TryGetValue(game.ShortTitle, out NotifierList))
             //           {
             //               Notifier test = new Notifier(Games.EMPTYGAMEID, MessageTopic.Error, notification);
             //               test.SendTick(() =>
             //               {
             //                   if (!string.IsNullOrEmpty(UserNotification.TestNotification(game).Message))
             //                   {
             //                       return true;
             //                   }

             //                   UserNotification.RunningNotifications.TryGetValue(game.ShortTitle, out NotifierList);
             //                   NotifierList.Remove(test);
             //                   return false;

             //               }, 1000 * 15);
             //           }
             //    }
             //}
         }

        static void SandBoxBatch(int take, int skip)
        {
            DBManager.Instance.Stream(Datastore.Monitoring, string.Format(@"select * from UserSessionMeta where RetentionCohortType = -1 order by LoginTimestamp asc LIMIT {1};", skip, take), (DataRow row) =>
            {
                double startMemory = GC.GetTotalMemory(false) / 1024D / 1024D;

                try
                {

                    Login LoginToProcess = new Login()
                    {
                        UserSessionId = row["UserSessionId"].ToString(),
                        InstallDateRecord = Int32.Parse(row["InstallDateRecord"].ToString()),
                        UserId = row["UserId"].ToString(),
                        LoginTimestamp = DateTime.Parse(row["LoginTimestamp"].ToString()),
                        RetentionCohortType = (RetentionCohortType)Int32.Parse(row["RetentionCohortType"].ToString())
                    };

                    double afterLogin = GC.GetTotalMemory(false) / 1024D / 1024D;

                    TrackedUserOccurance occ = ReturningRetention.Instance.DetermineUserType(LoginToProcess);

                    double afterTrack = GC.GetTotalMemory(false) / 1024D / 1024D;

                    string update = String.Format(
                        @"UPDATE UserSessionMeta SET RetentionCohortType = '{0}' where UserSessionId = '{1}';",
                        (int)occ.CohortType, LoginToProcess.UserSessionId);
                    //queue.Execute(() =>
                    {
                        try
                        {
                            DBManager.Instance.Insert(Datastore.Monitoring, update);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    //);

                    double afterQuery = GC.GetTotalMemory(false) / 1024D / 1024D;

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }

        static void Main(string[] args)
        {
            Setup();


            int userInput = -1;
            while (userInput != 0)
            {
                userInput = DisplayMenu();
                switch (userInput)
                {
                    case 1:
                        RetentionMenu();
                        break;
                    case 2:
                        RebuildDataMenu();
                        break;
                    case 3:
                        IntervalMenu();
                        break;
                    case 4:
                        TestingMenu();
                        break;
                    case 5:
                        SandBox(args);
                        DisplayMenu();
                        break;
                    default:
                        break;

                }
                if (userInput == 0)
                {
                    Shutdown();
                    Environment.Exit(userInput);
                }

            }

            Console.WriteLine("DOOOONNNEEE");
            Console.ReadLine();
        }

        #region Menus
        static public int DisplayMenu()
        {
            Console.Clear();
            Console.WriteLine(String.Format("Moniverse Manager : {0} : What What What, What would you do?", env));
            Console.WriteLine("");
            Console.WriteLine("1. Retention");
            Console.WriteLine("2. Rebuild Data");
            Console.WriteLine("3. Run Moniverse Interval");
            Console.WriteLine("4. Testing");
            Console.WriteLine("5. Execute Sandbox Code");
            Console.WriteLine("0. Exit");
            string result = Console.ReadLine();
            int r;
            try
            {
                if (result != "" || result != "\n" || result != "\n")
                {
                    r = Convert.ToInt32(result);
                }
                else {
                    r = 0;
                }
                
            }
            catch (Exception)
            {

                Console.WriteLine("Invalid Input");
                r = -1;
            }
            return r;
        }
        static public int RetentionMenu()
        {
            int DaysPastToCalculate = 60;
            bool bRunMainRetention = true;
            bool bRunReturnerRetention = false;

            Console.Clear();
            Console.WriteLine(String.Format("Moniverse Manager : {0} : What What What, What would you do?", env));
            Console.WriteLine("");
            Console.WriteLine(String.Format("1. Run General 14 day Retention from {0} - {1}", DateTime.UtcNow.AddDays(-14).Date, DateTime.UtcNow.AddDays(-1).Date));
            Console.WriteLine(String.Format("2. Run Returner (NURR CURR RURR) Retention from {0} - {1}", DateTime.UtcNow.AddDays(-14).Date, DateTime.UtcNow.AddDays(-1).Date));
            Console.WriteLine(String.Format("2. Run Both", DateTime.UtcNow.AddDays(-14).Date, DateTime.UtcNow.AddDays(-1).Date));
            Console.WriteLine("3. Run it all");
            Console.WriteLine("3. Run for date");
            Console.WriteLine("0. Exit" );
            string result = Console.ReadLine();
            int resultSwitch = Convert.ToInt32(result);
            switch(resultSwitch){
                case 1:
                    bRunMainRetention = true;
                    bRunReturnerRetention = false;
                    //RunRetention(DaysPastToCalculate, bRunMainRetention, bRunReturnerRetention);
                    break;
                case 2:
                    bRunMainRetention = false;
                    bRunReturnerRetention = true;
                    //RunRetention(DaysPastToCalculate, bRunMainRetention, bRunReturnerRetention);
                    break;
                case 3:
                    bRunMainRetention = true;
                    bRunReturnerRetention = true;
                    //RunRetention(DaysPastToCalculate, bRunMainRetention, bRunReturnerRetention);
                    break;
                case 4:
                    DateTime processDate = AskForADate("What Day?");
                    ProcessRetention(processDate);
                    
                    break;
                default:
                    break;

            }
            return Convert.ToInt32(result);
        }
        static public void IntervalMenu()
        {

            Console.Clear();
            Console.WriteLine(String.Format("Moniverse Manager : {0} : What What What, What would you do?", env));
            Console.WriteLine(String.Format("Run a timer Interval?", env));
            Console.WriteLine(String.Format(@"These intervals correspond to the intervals that fire in the playverse reader service"));
            Console.WriteLine("");
            Console.WriteLine(String.Format("1. One Minute"));
            Console.WriteLine(String.Format("5. Five Minute"));
            Console.WriteLine(String.Format("15. Fifteen Minute"));
            Console.WriteLine(String.Format("30. Thirty Minute"));
            Console.WriteLine(String.Format("60. Sixty Minute"));
            Console.WriteLine(String.Format("360. Three Hundred Sixty (6 hours)"));
            Console.WriteLine(String.Format("720. Seven Hundred Twenty (12 hours)"));
            Console.WriteLine(String.Format("1440.Fourteen Hundered Fourty  (24 hour)"));
            Console.WriteLine("0. Back");
            string result = Console.ReadLine();
            try
            {
                int r = 0;
                if(result != "" || result != "/r" || result != "/n"){
                    r = Convert.ToInt32(result);
                }
                int resultSwitch = r;

                try
                {
                    MockTimerState ts = new MockTimerState(r);
                    RunInterval(ts);
                    DisplayMenu();
                }
                catch (Exception)
                {
                    Console.WriteLine("Incorrect Input");
                    IntervalMenu();
                }
                IntervalMenu();
            }
            catch (Exception)
            {
                Console.WriteLine("INvalid Input");
            }          
        }
        static public int RebuildDataMenu()
        {

            Console.Clear();
            Console.WriteLine(String.Format("Moniverse Manager : {0} : What What What, What would you do?", env));
            Console.WriteLine("");
            Console.WriteLine(String.Format("1. Rebuild Keen Data From File"));
            Console.WriteLine(String.Format("2. Query Keen For Latest From Collection"));
            Console.WriteLine(String.Format("3. Activity Interval Migration"));
            Console.WriteLine(String.Format("4. List Schemas from Analytics Provider"));
            Console.WriteLine(String.Format("5. Request Data Extraction from Analytics Provider"));
            Console.WriteLine(String.Format("6. by day UserSessionMeta scrape"));
            Console.WriteLine("0. Exit");
            var result = Console.ReadLine();

            try
            {
                int r = Convert.ToInt32(result);
                switch (r)
                {
                    case 1:
                        RebuildFromFileMenu();
                        break;
                    case 2:
                        QueryLastestKeenMenu();
                        break;
                    case 3:
                        IntervalMigrationMenu();
                        break;
                    case 4:
                        foreach (JObject x in AnalyticsProvider.GetProviderSchemas()) {
                            Console.WriteLine((string)x["name"]);
                        }
                        Console.ReadKey();
                        break;
                    case 5:
                        try
                        {
                            Console.WriteLine("Email Address to send the extraction?");
                           string email = Console.ReadLine();
                           if (!RegexUtilities.instance.IsValidEmail(email)) {
                               do
                               {
                                   Console.Clear();
                                   Console.WriteLine("Not a valid address");
                                   Thread.Sleep(TimeSpan.FromSeconds(1));
                                   Console.Clear();
                                   Console.WriteLine("Email Address to send the extraction?");
                                   email = Console.ReadLine();
                               } while (!RegexUtilities.instance.IsValidEmail(email));                           
                           }
                            
                            Console.WriteLine("collection?");
                            string collection = Console.ReadLine();

                            Console.WriteLine("startdate?");
                            string startDate = Console.ReadLine();
                            DateTime start;
                            if (!DateTime.TryParse(startDate, out start)) {
                                do
                                {
                                    Console.Clear();
                                    Console.WriteLine("Not a valid date time");
                                    Thread.Sleep(TimeSpan.FromSeconds(1));
                                    Console.Clear();
                                    Console.WriteLine("startdate?");
                                    startDate = Console.ReadLine();
                                    try
                                    {
                                        start = DateTime.Parse(startDate);
                                    }
                                    catch (Exception)
                                    {
                                    }
                                } while (!DateTime.TryParse(startDate, out start));                            
                            }


                            Console.WriteLine("enddate?");
                            string endDate = Console.ReadLine();
                            DateTime end;
                            if (!DateTime.TryParse(endDate, out end)) {
                                do
                                {
                                    Console.Clear();
                                    Console.WriteLine("Not a valid date time");
                                    Thread.Sleep(TimeSpan.FromSeconds(1));
                                    Console.Clear();
                                    Console.WriteLine("enddate?");
                                    endDate = Console.ReadLine();
                                    try
                                    {
                                        end = DateTime.Parse(endDate.ToString());
                                    }
                                    catch (Exception)
                                    {
                                    }
                                } while (!DateTime.TryParse(endDate, out end));                            
                            }

                            bool success = KeenIO.Instance.RequestEmailExtraction(email, collection, start, end);
                            Console.WriteLine(success.ToString());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        break;
                    case 6:
                        int daysToCalc = 2;
                        RunUserSessionScrape(daysToCalc);
                        Console.ReadKey();
                        break;
                        
                }
               
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid Input");
            }


            return 0;
        }
        static public int RebuildFromFileMenu()
        {

            Console.Clear();
            Console.WriteLine(String.Format("Moniverse Manager : {0} :  What would you do?", env));
            Console.WriteLine("");
            Console.WriteLine(String.Format("1. Rebuild User Session Meta (All)"));
            Console.WriteLine(String.Format("2. Login"));
            Console.WriteLine(String.Format("3. Logoff"));
            Console.WriteLine(String.Format("4. Request Data Extraction from Analytics Provider"));
            Console.WriteLine("0. Exit");
            var result = Console.ReadLine();

            try
            {
                int r = Convert.ToInt32(result);
                switch (r)
                {
                    case 1:
                        DataBuilder.Instance.TryRebuildUserSessionMeta(KeenExtractionPath);
                        break;
                    case 2:
                        DataBuilder.Instance.TryRebuild("Login", KeenExtractionPath);
                        break;
                    case 3:
                        DataBuilder.Instance.TryRebuild("Logoff", KeenExtractionPath);
                        break;
                    case 4:
                        foreach (JObject x in AnalyticsProvider.GetProviderSchemas())
                        {
                            Console.WriteLine((string)x["name"]);
                        }
                        Console.ReadKey();
                        break;

                }

            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid Input");
            }


            return 0;
        }
        static public int QueryLastestKeenMenu()
        {

            Console.Clear();
            Console.WriteLine(String.Format("Moniverse Manager : {0} : What What What, What would you do?", env));
            Console.WriteLine("");
            Console.WriteLine(String.Format("1. Process Logins"));
            Console.WriteLine(String.Format("3. Process Logoffs"));
            Console.WriteLine(String.Format("4. Process All User Meta"));
            Console.WriteLine(String.Format("5. List Schemas from Analytics Provider"));
            Console.WriteLine("0. Exit");
            var result = Console.ReadLine();

            try
            {
                int r = Convert.ToInt32(result);
                switch (r)
                {
                    case 1:
                        UserSessions.Instance.processLogins();
                        break;
                    case 3:
                        UserSessions.Instance.processLogoffs();
                        break;
                    case 4:
                        UserSessions.Instance.QueryAndBuildUserSessionMeta();
                        break;
                    case 5:
                        IntervalMigrationMenu();
                        break;
                    case 6:
                        foreach (JObject x in AnalyticsProvider.GetProviderSchemas())
                        {
                            Console.WriteLine((string)x["name"]);
                        }
                        Console.ReadKey();
                        break;

                }

            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid Input");
            }


            return 0;
        }
        static public int IntervalMigrationMenu()
        {

            Console.Clear();
            Console.WriteLine(String.Format("Moniverse Manager : {0} : What What What, What would you do?", env));
            Console.WriteLine("------");
            Console.WriteLine(String.Format("Playtrics View Interval Migrations"));
            Console.WriteLine("------");
            Console.WriteLine(String.Format("1. 5 minutes"));
            Console.WriteLine(String.Format("2. 15 minutes"));
            Console.WriteLine(String.Format("3. 30 minutes"));
            Console.WriteLine(String.Format("4. 1 hour"));
            Console.WriteLine(String.Format("5. 6 hour"));
            Console.WriteLine(String.Format("6. 12 hour"));
            Console.WriteLine(String.Format("7. 24 hour"));
            Console.WriteLine(String.Format("8. Do All Session Intervals"));
            Console.WriteLine(String.Format("9. Do All User Activity Intervals"));
            Console.WriteLine(String.Format("10. Do Everything"));
            Console.WriteLine("0. Exit");
            var result = Console.ReadLine();

            try
            {
                int r = Convert.ToInt32(result);
                switch (r)
                {
                    case 1:
                        DataBuilder.Instance.Migrate5Minute(ConsoleThreads);
                        break;
                    case 2:
                        DataBuilder.Instance.Migrate15Minute(ConsoleThreads);
                        break;
                    case 3:
                        DataBuilder.Instance.Migrate30Minute(ConsoleThreads);
                        break;
                    case 4:
                        DataBuilder.Instance.Migrate1Hour(ConsoleThreads);
                        break;
                    case 5:
                        DataBuilder.Instance.Migrate6Hour(ConsoleThreads);
                        break;
                    case 6:
                        DataBuilder.Instance.Migrate12Hour(ConsoleThreads);
                        break;
                    case 7:
                        DataBuilder.Instance.Migrate24Hour(ConsoleThreads);
                        break;
                    case 8:
                        DataBuilder.Instance.MigrateAllGameSessionUserStats(ConsoleThreads);
                        break;
                    case 9:
                        DataBuilder.Instance.MigrateAllGameUserActivity(ConsoleThreads);
                        break;
                    case 10:
                        DataBuilder.Instance.MigrateAllGameSessionUserStats(ConsoleThreads);
                        DataBuilder.Instance.MigrateAllGameUserActivity(ConsoleThreads);
                        break;
                }

            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid Input");
            }


            return 0;
        }
        
        public static int TestingMenu()
        {

            Console.Clear();
            Console.WriteLine(String.Format("Moniverse Manager : {0} : What What What, What would you do?", env));
            Console.WriteLine("");
            Console.WriteLine(String.Format("Testing Menu"));
            Console.WriteLine(String.Format("1. Load All Testing Data", DateTime.UtcNow.AddDays(-14).Date, DateTime.UtcNow.AddDays(-1).Date));
            Console.WriteLine(String.Format("2. Run Nurr Curr Rurr Tests", DateTime.UtcNow.AddDays(-14).Date, DateTime.UtcNow.AddDays(-1).Date));
            Console.WriteLine(String.Format("3. Clear Data", DateTime.UtcNow.AddDays(-14).Date, DateTime.UtcNow.AddDays(-1).Date));
            Console.WriteLine("3. Run it all");
            Console.WriteLine("0. Exit");
            string result = Console.ReadLine();
            int resultSwitch = Convert.ToInt32(result);
            switch (resultSwitch)
            {
                case 1:
                    ExecuteSetUpMethodsOnClass("Moniverse.Test.MockReturningRetention");
                    break;
                case 2:
                    ExecuteTestMethodsOnClass("Moniverse.Test.MockReturningRetention");
                    break;
                case 3:
                    ExecuteTearDownMethodsOnClass("Moniverse.Test.MockReturningRetention");
                    break;
                default:
                    break;

            }
            return Convert.ToInt32(result);       
        }
        #endregion

        #region taskExecution
        static void RunInterval(MockTimerState ts)
        {
            IntervalHandler timer = new IntervalHandler(ts, ConsoleThreads);

            Console.WriteLine("initialized timer");
            GC.KeepAlive(timer);
            Console.WriteLine("tell GC to ignore timer object");
            List<GameMonitoringConfig> games = Games.Instance.GetMonitoredGames();

            //one
            timer.OneMinute += new IntervalHandler.IntervalHandlerDelegate(MonitoringEvents.UserActivitycheck);
            timer.OneMinute += new IntervalHandler.IntervalHandlerDelegate(MonitoringEvents.HostingInstanceCheck);
            timer.OneMinute += new IntervalHandler.IntervalHandlerDelegate(MonitoringEvents.GameSessionActivityCheck);
            timer.OneMinute += new IntervalHandler.IntervalHandlerDelegate(MonitoringEvents.RunTransactionReport);
            //five
            timer.FiveMinutes += new IntervalHandler.IntervalHandlerDelegate(MonitoringEvents.OnInterval);
            //fifteen
            timer.FifteenMinutes += new IntervalHandler.IntervalHandlerDelegate(MonitoringEvents.OnInterval);
            //timer.FifteenMinutes += new IntervalHandler.IntervalHandlerDelegate(ProcessRetention);
            //thirty
            timer.ThirtyMinutes += new IntervalHandler.IntervalHandlerDelegate(MonitoringEvents.OnInterval);

            //one hour
            timer.SixtyMinutes += new IntervalHandler.IntervalHandlerDelegate(MonitoringEvents.OnInterval);
            timer.SixtyMinutes += new IntervalHandler.IntervalHandlerDelegate(MonitoringEvents.ScrapeUserData);

            //six hours
            timer.SixHours += new IntervalHandler.IntervalHandlerDelegate(MonitoringEvents.OnInterval);


            //twelve
            timer.TwelveHours += new IntervalHandler.IntervalHandlerDelegate(MonitoringEvents.OnInterval);

            //daily
            timer.TwentyFourHours += new IntervalHandler.IntervalHandlerDelegate(MonitoringEvents.OnInterval);
            timer.TwentyFourHours += new IntervalHandler.IntervalHandlerDelegate(MonitoringEvents.RunTransactionReport);
            timer.TwentyFourHours += new IntervalHandler.IntervalHandlerDelegate(MonitoringEvents.ProcessRetention);
            Console.WriteLine("set event callbacks");

            timer.OnCheck += new IntervalHandler.OnCheckDelegate(StopInterval);

            //long timeToWait = 60000;
            long timeToWait = 1000;

            long WaitTillWholeMin = 0; //this is telling this guy to wait until the next whoe minute (:00 seconds)
            Console.WriteLine("Waiting till the next whole minute to start the Timer");
            //run the time at what interval
            timer.run((long)timeToWait, WaitTillWholeMin, games); ///60 seconds // 1 minute == 60000
            ///
            //Thread.Sleep(30000);
        }

        static void StopInterval(IntervalHandler handler, int timeInfo)
        {
            //timesExecuted++;
            //if (Moniverse.hasError || timesExecuted > 0)
            //{
            handler.stop();
            Console.ForegroundColor = System.ConsoleColor.Yellow;
            Console.WriteLine("===============Done==============");
            Console.ForegroundColor = System.ConsoleColor.Gray;
            //}

        }

        static void RunGameSessionScrape(int DaysToCalc)
        {
            //Console.SetOut(new MasterLogger(Console.Out));
            int DaysToCalculate = DaysToCalc;
            //for (int i = 0; i <= DaysToCalculate; i++)
            //{
            //    DateTime thisday = DateTime.UtcNow.AddDays(-i);
            //    GameSessions.instance.QueryAndBuildGameSessionMeta(thisday);

            //}
            int i = 0;
            TimeSpan RefreshRate = TimeSpan.FromSeconds(30);

            while (true)
            {
                DateTime thisday = DateTime.UtcNow;
                GameSessionMetaScrape(thisday);
                Thread.Sleep(RefreshRate);
                i++;
            }
        }

        static void GameSessionMetaScrape(DateTime today)
        {

            GameSessions.instance.QueryAndBuildGameSessionMeta(today);
        }

        static void UserSessionMetaScrape(DateTime today)
        {
            UserSessions.Instance.processLogins(today);
            UserSessions.Instance.processLogoffs(today);
        }

        static void RunUserSessionScrape(int daysToCalc) {
            for (int i = daysToCalc; i >= 0; i--)
            {
                DateTime thisday = DateTime.UtcNow.AddDays(-i);

                UserSessionMetaScrape(thisday);
            }
        }

        static void ProcessRetention(DateTime date) {
            Retention.Instance.ProcessRetentionForDate(date);
        }
        //static void RunRetention(int dayToCalc, bool bRun14Day, bool bRunReturner)
        //{
        //    //Console.SetOut(new MasterLogger(Console.Out));
        //    //UserSessions.Instance.QueryAndBuildUserSessionMeta();
        //    int DaysToCalculate = dayToCalc;
        //    for (int i = DaysToCalculate; i >= 0; i--)
        //    {
        //        DateTime thisday = DateTime.UtcNow.AddDays(-i);

        //        //get the logins, installs and logoffs for every day we are calculating
        //        //UserSessions.Instance.QueryAndBuildUserSessionMeta(thisday);
        //        if (bRunReturner)
        //        {
        //            ReturningRetention.Instance.Calculate(thisday.AddDays(-1));
        //        }
        //    }

        //    if (bRun14Day)
        //    {

        //        Console.WriteLine("--== Calculating Retention ==--");
        //        Retention.Instance.CalculateRetention(DaysToCalculate);
        //    }



        //    //    Console.WriteLine("--== Retention Done ==--");


        //    //    Console.WriteLine(@"-------------------------------------");
        //    //}

        //    Console.WriteLine("Done");

        //}

        static void RunTransactionReport(DateTime start, DateTime end)
        {
            GameMonitoringConfig dd2 = Games.Instance.GetMonitoredGames().Where(x => x.ShortTitle == "DD2").FirstOrDefault();
            //Transactions.Instance.CaptureTransactionsForLastFullDay(dd2);
            Transactions.Instance.RecordTransactionsFromRange(dd2.Id, start, end);
        }

        private static void ParseUserSessionMetas()
        {
            string query = "SELECT * FROM UserSessionMeta where RetentionCohortType = -1;";

            DBManager.Instance.Stream(Datastore.Monitoring, query, (row) =>
            {
                Login user = new Login()
                {
                    UserId = row["UserId"].ToString(),
                    City = row["City"].ToString(),
                    Country = row["Country"].ToString(),
                    GameId = row["GameId"].ToString(),
                    LoginTimestamp = DateTime.Parse(row["LoginTimestamp"].ToString()),
                    LogoffTimestamp = DateTime.Parse(row["LogoffTimestamp"].ToString()),
                    RecordDate = row["RecordDate"].ToString(),
                    InstallDateRecord = Int32.Parse(row["InstallDateRecord"].ToString()),
                    Region = row["Region"].ToString(),
                    RetentionCohortType = (RetentionCohortType)Int32.Parse(row["RetentionCohortType"].ToString()),
                    UserSessionId = row["UserSessionId"].ToString()
                };

                TrackedUserOccurance occurance = ReturningRetention.Instance.DetermineUserType(user);

                user.RetentionCohortType = occurance.CohortType;
                DBManager.Instance.Insert(Datastore.Monitoring,
                    String.Format(
                        @"UPDATE UserSessionMeta SET RetentionCohortType = '{0}' Where UserSessionId = '{1}'",
                        (int)user.RetentionCohortType, user.UserSessionId));
                Console.WriteLine("Session {0} Processed as {1}", user.UserSessionId, occurance.CohortType.ToString());
            });
        }

        #endregion


        #region misc helpers
        private static bool isWriterRunning() {
            Process[] pname = Process.GetProcessesByName("Moniverse.Writer");
            if (pname.Length > 0){
                return true;
            }
            return false;
        }

        private static void StartWriterServer() {
            string thisAssembly = System.Reflection.Assembly.GetEntryAssembly().Location;
            Console.WriteLine(thisAssembly);
            string dirPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(thisAssembly).ToString()).ToString()).ToString()).ToString();
            string MoniverseReaderServer = dirPath + @"\Moniverse.Writer\bin\" + env +@"\Moniverse.Writer.exe";

            ConsoleThreads.QueueWorkItem(() =>
            {
                try
                {
                    StartProcess(MoniverseReaderServer);
                }
                catch (Exception ex)
                {

                    Console.WriteLine("Failed to start server: " + ex.Message);
                }


            });        
        }

        private static void StartProcess(string ExeName, string arguments = "")
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = arguments;
            start.FileName = ExeName;
            start.WindowStyle = ProcessWindowStyle.Normal;
            start.CreateNoWindow = false;
            int exitCode;
            // Run the external process & wait for it to finish
            using (Process proc = Process.Start(start))
            {
                proc.WaitForExit();
                exitCode = proc.ExitCode;
            }

        }

        private static DateTime AskForADate(string messageText){
                Console.WriteLine(messageText);
                string startDate = Console.ReadLine();
                DateTime start;
                if (!DateTime.TryParse(startDate, out start)) {
                    do
                    {
                        Console.Clear();
                        Console.WriteLine("Not a valid date time");
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                        Console.Clear();
                        Console.WriteLine(messageText);
                        startDate = Console.ReadLine();
                        try
                        {
                            start = DateTime.Parse(startDate);
                        }
                        catch (Exception)
                        {
                        }
                    } while (!DateTime.TryParse(startDate, out start));                            
                }
                return start;
        }

        private static void ExecuteTestMethodsOnClass(string ClassName) {
            foreach (MethodInfo info in Type.GetType(ClassName).GetMethods())
            {
                System.Action action = (System.Action)Delegate.CreateDelegate(typeof(System.Action), info);
                if (isTestMethod(() => action()))
                {
                    action();
                }
            }        
        }
        private static void ExecuteSetUpMethodsOnClass(string ClassName)
        {
            foreach (MethodInfo info in Type.GetType(ClassName).GetMethods())
            {
                System.Action action = (System.Action)Delegate.CreateDelegate(typeof(System.Action), info);
                if (isSetUpMethod(() => action()))
                {
                    action();
                }
            }
        }
        private static void ExecuteTearDownMethodsOnClass(string ClassName)
        {
            foreach (MethodInfo info in Type.GetType(ClassName).GetMethods())
            {
                System.Action action = (System.Action)Delegate.CreateDelegate(typeof(System.Action), info);
                if (isTearDownMethod(() => action()))
                {
                    action();
                }
            }
        }

        public static MethodInfo MethodOf( Expression<System.Action> expression )
        {
            MethodCallExpression body = (MethodCallExpression)expression.Body;
            return body.Method;
        }
        private static bool isTestMethod(Expression<System.Action> expression) {

            MemberInfo member = MethodOf(expression);
            return MemberHasTestCaseAttribute(member);
        }
        private static bool isSetUpMethod(Expression<System.Action> expression)
        {

            MemberInfo member = MethodOf(expression);
            return MemberHasTestCaseAttribute(member);
        }
        private static bool isTearDownMethod(Expression<System.Action> expression)
        {

            MemberInfo member = MethodOf(expression);
            return MemberHasTestCaseAttribute(member);
        }

        private static bool MemberHasTestCaseAttribute(MemberInfo member)
        {
            const bool includeInherited = false;
            return member.GetCustomAttributes(typeof(TestCaseAttribute), includeInherited).Any();
        }
        private static bool MemberHasSetUpAttribute(MemberInfo member)
        {
            const bool includeInherited = false;
            return member.GetCustomAttributes(typeof(SetUpAttribute), includeInherited).Any();
        }
        private static bool MemberHasTearDownAttribute(MemberInfo member)
        {
            const bool includeInherited = false;
            return member.GetCustomAttributes(typeof(TearDownAttribute), includeInherited).Any();
        }
        //private static string GetRightPartOfPath(string path, string startAfterPart)
        //{
        //    // use the correct seperator for the environment
        //    var pathParts = path.Split(Path.DirectorySeparatorChar);

        //    // this assumes a case sensitive check. If you don't want this, you may want to loop through the pathParts looking
        //    // for your "startAfterPath" with a StringComparison.OrdinalIgnoreCase check instead
        //    int startAfter = Array.IndexOf(pathParts, startAfterPart);

        //    if (startAfter == -1)
        //    {
        //        // path path not found
        //        return null;
        //    }

        //    // try and work out if last part was a directory - if not, drop the last part as we don't want the filename
        //    var lastPartWasDirectory = pathParts[pathParts.Length - 1].EndsWith(Path.DirectorySeparatorChar.ToString());
        //    return string.Join(
        //        Path.DirectorySeparatorChar.ToString(),
        //        pathParts, startAfter,
        //        pathParts.Length - startAfter - (lastPartWasDirectory ? 0 : 1));
        //}
        #endregion

    }

}
