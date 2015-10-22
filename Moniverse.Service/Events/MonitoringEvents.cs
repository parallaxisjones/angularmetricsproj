using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Amib.Threading;
using Moniverse.Contract;

namespace Moniverse.Service
{
    public class MonitoringEvents
    {
        public static MonitoringEvents instance = new MonitoringEvents();
        public static SmartThreadPool ReaderThreads = new SmartThreadPool();
        public event IntervalFinishedHandler OnIntervalComplete;
        public void SetEventCallbacks(IntervalHandler timer, List<GameMonitoringConfig> games, QueueManager WorkQueue)
        {
            //interval timer events
            //+= to add a new event delegate to fire on the appropriate timer

            //one
            timer.OneMinute += new IntervalHandler.IntervalHandlerDelegate(UserActivitycheck);
            timer.OneMinute += new IntervalHandler.IntervalHandlerDelegate(HostingInstanceCheck);
            timer.OneMinute += new IntervalHandler.IntervalHandlerDelegate(GameSessionActivityCheck);
            timer.OneMinute += new IntervalHandler.IntervalHandlerDelegate(RunTransactionReport);
            timer.OneMinute += new IntervalHandler.IntervalHandlerDelegate(ScrapeUserData);
            //five
            timer.FiveMinutes += new IntervalHandler.IntervalHandlerDelegate(OnInterval);

            //fifteen
            timer.FifteenMinutes += new IntervalHandler.IntervalHandlerDelegate(OnInterval);
            //timer.FifteenMinutes += new IntervalHandler.IntervalHandlerDelegate(ProcessRetention);

            //thirty
            timer.ThirtyMinutes += new IntervalHandler.IntervalHandlerDelegate(OnInterval);

            //one hour
            timer.SixtyMinutes += new IntervalHandler.IntervalHandlerDelegate(OnInterval);

            //six hours
            timer.SixHours += new IntervalHandler.IntervalHandlerDelegate(OnInterval);

            //twelve
            timer.TwelveHours += new IntervalHandler.IntervalHandlerDelegate(OnInterval);
            timer.TwelveHours += new IntervalHandler.IntervalHandlerDelegate(DailyStats);

            //daily
            timer.TwentyFourHours += new IntervalHandler.IntervalHandlerDelegate(OnInterval);
            timer.TwentyFourHours += new IntervalHandler.IntervalHandlerDelegate(ProcessRetention);

            Logger.Instance.Info("set event callbacks");
        }


        public static void DailyStats(List<GameMonitoringConfig> games, int timeInfo)
        {
            Parallel.ForEach(games, game =>
            {
                Users.Instance.RecordDailyActiveUsersByGame(game);                  
            });
        }

        public static void GameSessionActivityCheck(List<GameMonitoringConfig> games, int timeInfo)
        {
            Logger.Instance.Info("Fire Game Session Tasks");
            GameSessions.instance.QueryAndBuildGameSessionMeta(DateTime.UtcNow);
        }
        
        public static void UserSessionActivityCheck(List<GameMonitoringConfig> games, int timeInfo)
        {
            Logger.Instance.Info("User Session Tasks");
            UserSessions.Instance.QueryAndBuildUserSessionMeta();
        }

        public static void UserActivitycheck(List<GameMonitoringConfig> games, int timeInfo)
        {

            try
            {
                lock (MoniverseBase.ConsoleWriterLock) {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info(@"     User Activity Tasks Started     ");
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info("");                
                }

                // Process non game specific tasks
                Users.Instance.RecordDailyActiveUsers();

                // Process game specific tasks
                Parallel.ForEach(games, game =>
                {
                    Users.Instance.RecordActiveUsers(game);
                    Users.Instance.RecordGameSessionUserStats(game);
                    if(game.ShortTitle != "DDE")
                        Users.Instance.CheckActiveUserDelta(game);
                });
                //IntervalCounter += 1;
                lock (MoniverseBase.ConsoleWriterLock) {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info(@"     User Activity Tasks Complete     ");
                    Logger.Instance.Info("--------------------------------------");
                    Console.ResetColor();                
                }

            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }
        }

        public static void HostingInstanceCheck(List<GameMonitoringConfig> games, int timeInfo)
        {
            try
            {
                lock (MoniverseBase.ConsoleWriterLock) {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info(@"   Hosting Instance Tasks Started    ");
                    Logger.Instance.Info("--------------------------------------");
                    Console.ResetColor();                
                }

                // Process non game specific tasks
                HostingInstances.Instance.QueryAndBuildHostingInstanceMeta(DateTime.UtcNow);
                lock (MoniverseBase.ConsoleWriterLock) {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info(@"   Hosting Instance Tasks Complete   ");
                    Logger.Instance.Info("--------------------------------------");
                    Console.ResetColor();                
                }

            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }
        }

        public static void OnInterval(List<GameMonitoringConfig> games, int timeInfo)
        {
            try
            {
                if (timeInfo > 0 && timeInfo != 1)
                {
                    lock (MoniverseBase.ConsoleWriterLock) {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info(String.Format(@"   {0} Tasks Started    ", timeInfo));
                        Logger.Instance.Info("--------------------------------------");
                        Console.ResetColor();                    
                    }
                    ReaderThreads.QueueWorkItem(() => {
                        Users.Instance.RecordGameSessionUserStatsInterval(timeInfo);
                    });
                    ReaderThreads.QueueWorkItem(() =>
                    {
                        Users.Instance.RecordActiveUsersInterval(timeInfo);
                    });

                    lock (MoniverseBase.ConsoleWriterLock) {
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info(String.Format(@"   {0} Tasks Complete    ", timeInfo));
                        Logger.Instance.Info("--------------------------------------");
                        Console.ResetColor();                    
                    }


                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }
        }

        public static void ScrapeUserData(List<GameMonitoringConfig> games, int timeInfo) {
            //Retention.Instance.RecordLatestInstalls();
            //Retention.Instance.RecordLatestLogins();
            UserSessions.Instance.QueryAndBuildUserSessionMeta();   
        }

        public static void ProcessRetention(List<GameMonitoringConfig> games, int timeInfo)
        {
            try
            {
                for (int i = -15; i < 0; i++)
                {
                    var reportDate = DateTime.UtcNow.Date.AddDays(i);

                    new RetentionReport(reportDate).Process();
                }

            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }
        }

        public static void RunTransactionReport(List<GameMonitoringConfig> games, int timeInfo)
        {

            try
            {
                Transactions.Instance.CaptureLatest(Games.Instance.GetMonitoredGames().FirstOrDefault(x => x.ShortTitle == "DD2"));
                Transactions.Instance.CaptureGameCreditTransactions(Games.Instance.GetMonitoredGames().FirstOrDefault(x => x.ShortTitle == "DD2"));
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

        }

    }
}
