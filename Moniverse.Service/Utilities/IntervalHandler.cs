using Moniverse.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Utilities;
using Amib.Threading;
using Moniverse.Contract;

namespace Moniverse.Service
{

    public class TimerState : ITimerState
    {
        public int counter { get { return ((int)DateTime.UtcNow.Hour * 60) + DateTime.UtcNow.Minute; } }
    }
    public enum MinuteTicks
    {
        FiveMinutes = 5,
        FifteenMinutes = 15,
        ThirtryMinutes = 30,
        OneHour = 60,
        SixHours = 360,
        TwelveHours = 720,
        TwentyFourHours = 1440
    }

    public interface ITimerState
    {
        int counter { get; }
    }
    //onDataFinished events
 
    public class IntervalFinishedEventArgs : EventArgs
    {
        private string _FiredInterval;

        public IntervalFinishedEventArgs(string IntervalName){
            _FiredInterval = IntervalName;
        }
        public string GetInterval(){
            return _FiredInterval;
        }
    
    }
    public delegate void IntervalFinishedHandler(object source, IntervalFinishedEventArgs e);

    public class IntervalHandler
    {
        public ITimerState s { get; set; }
        public List<GameMonitoringConfig> games { get; set; }
        private SmartThreadPool ReaderThreads;
        public IntervalHandler(ITimerState ts, SmartThreadPool ReaderThreadPool)
        {
            s = ts;
            ReaderThreads = ReaderThreadPool;
        }
        
        private static Timer timer;
        //register event delegates
        public delegate void IntervalHandlerDelegate(List<GameMonitoringConfig> games, int timeInfo);
        public delegate void OnCheckDelegate(IntervalHandler handler, int timeInfo);
        public event OnCheckDelegate OnCheck;



        //IntervalHandlerDelegate is the event that is fired
        //new functions that match the signature of the delegate can be added to the event with += in Moniverse.cs
        public event IntervalHandlerDelegate OneMinute;

        protected void OnOneMinute(List<GameMonitoringConfig> games, int timeInfo)
        {
            if (OneMinute != null)
            {
                ReaderThreads.QueueWorkItem(() => {
                    try
                    {
                        Logger.Instance.Info("one minutes thread fired");
                        OneMinute(games, timeInfo);
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Exception("1 minute fail:" + ex.Message, ex.StackTrace);
                    }                    
                });
            }
        }

        public event IntervalHandlerDelegate FiveMinutes;

        protected void OnFiveMinutes(List<GameMonitoringConfig> games, int timeInfo)
        {
            if (FiveMinutes != null && ReaderThreads != null)
            {
                ReaderThreads.QueueWorkItem(() =>
                {
                    try
                    {
                        Logger.Instance.Info("5 minutes thread fired");
                        FiveMinutes(games, timeInfo);
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Exception("5 minute fail:" + ex.Message, ex.StackTrace);
                    }
                });
            }
        }
        public event IntervalHandlerDelegate FifteenMinutes;

        protected void OnFifteenMinutes(List<GameMonitoringConfig> games, int timeInfo)
        {
            if (FifteenMinutes != null)
            {
                ReaderThreads.QueueWorkItem(() =>
                {
                    try
                    {
                        Logger.Instance.Info("15 minutes thread fired");
                        FifteenMinutes(games, timeInfo);
                    }
                    catch (Exception ex)
                    {

                        Logger.Instance.Exception("15 minute fail:" + ex.Message, ex.StackTrace);
                    }
                });
            }
        }
        public event IntervalHandlerDelegate ThirtyMinutes;

        protected void OnThirtyMinutes(List<GameMonitoringConfig> games, int timeInfo)
        {
            if (ThirtyMinutes != null)
            {
                ReaderThreads.QueueWorkItem(() =>
                {
                    try
                    {
                        Logger.Instance.Info("30 minutes thread fired");
                        ThirtyMinutes(games, timeInfo);
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Exception("30 minute fail: " + ex.Message, ex.StackTrace);
                    }

                });

            }
        }
        public event IntervalHandlerDelegate SixtyMinutes;

        protected void OnSixtyMinutes(List<GameMonitoringConfig> games, int timeInfo)
        {
            if (SixtyMinutes != null)
            {

                ReaderThreads.QueueWorkItem(() =>
                {
                    try
                    {
                        Logger.Instance.Info("60 minutes thread fired");
                        SixtyMinutes(games, timeInfo);
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Exception("60 minute fail:" + ex.Message, ex.StackTrace);
                    }

                });

            }
        }
        public event IntervalHandlerDelegate SixHours;

        protected void OnSixHours(List<GameMonitoringConfig> games, int timeInfo)
        {
            if (SixHours != null)
            {
                ReaderThreads.QueueWorkItem(() =>
                {
                    try
                    {
                        Logger.Instance.Info("360 minutes thread fired");
                        SixHours(games, timeInfo);
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Exception("360 minute fail:" + ex.Message, ex.StackTrace);
                    }

                });

            }
        }
        public event IntervalHandlerDelegate TwelveHours;

        protected void OnTwelveHours(List<GameMonitoringConfig> games, int timeInfo)
        {
            if (TwelveHours != null)
            {
                SixHours(games, timeInfo); ReaderThreads.QueueWorkItem(() =>
                {
                    try
                    {
                        Logger.Instance.Info("720 minutes thread fired");
                        TwelveHours(games, timeInfo);
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Exception("720 minute fail:" + ex.Message, ex.StackTrace);
                    }

                });
            }
        }
        public event IntervalHandlerDelegate TwentyFourHours;

        protected void OnTwentyFourHours(List<GameMonitoringConfig> games, int timeInfo)
        {
            if (TwentyFourHours != null)
            {
                ReaderThreads.QueueWorkItem(() =>
                {
                    try
                    {
                        Logger.Instance.Info("1440 minutes thread fired");
                        TwentyFourHours(games, timeInfo);
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Exception("1440 minute fail:" + ex.Message, ex.StackTrace);
                    }

                });
            }
        }



        //takes an interval on which to run and a list of games to check
        public void run(long interval, long waitTime, List<GameMonitoringConfig> games)
        {
            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            //AutoResetEvent resetEvent = new AutoResetEvent(false);           
            Logger.Instance.Info("Waiting till the next whole minute to start the Timer");
            Logger.Instance.Info(String.Format("Hi {0}", userName));

            timer = new Timer((object state) => {
                Check(games);
            }, s, waitTime, interval);
            //need to check and add callback functions
        }


        public void stop()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }


        //check the intervals and fire event callbacks
        protected void Check(List<GameMonitoringConfig> games)
        {                        
#if DEBUG
            //Debugger.Launch();
#endif
            // Would have like a sleep for a minute, like the Stopwatch implementation - You only need to collect data at a maximum of 1 min intervals

            // Take DateTime.UtcNow -> 12:30:33 | 1 min = 12:30:00 | 5 min = 12:30:00 (12:25 - 12:30) | 10 min = 12:30 (12:20-12:30) | 30 min = 12:30 (12:00-12:30)
            // Take DateTime.UtcNow -> 12:35:33 | 1 min = 12:35:00 | 5 min = 12:35:00 (12:30 - 12:35)
            // For each of these, go to the raw table and grab "BETWEEN (12:30 - interval like 5 or 10 mins) AND 12:30" like "BETWEEN 12:25 AND 12:30"
            if (s.counter == 0)
            {
                OnTwentyFourHours(games, (int)MinuteTicks.TwentyFourHours);
                OnTwelveHours(games, (int)MinuteTicks.TwelveHours);
                OnSixHours(games, (int)MinuteTicks.SixHours);
                OnSixtyMinutes(games, (int)MinuteTicks.OneHour);
                OnThirtyMinutes(games, (int)MinuteTicks.ThirtryMinutes);
                OnFifteenMinutes(games, (int)MinuteTicks.FifteenMinutes);
                OnFiveMinutes(games, (int)MinuteTicks.FiveMinutes);
                OnOneMinute(games, 1);
                try
                {
                    if (OnCheck != null)
                    {
                        OnCheck(this, s.counter);
                    }
                }
                catch (Exception ex)
                {
                    /* Do Nothing as this is for the console app exclusively*/
                }
                //Logger.Instance.Info("done");
                return;
            }

            if (s.counter % 1 == 0)
            {
                lock (MoniverseBase.ConsoleWriterLock) {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info(@"           One Minute Event          ");
                    Logger.Instance.Info("--------------------------------------");
                    Console.ResetColor();                
                }

                OnOneMinute(games, 1);

            }
            if (s.counter % 5 == 0)
            {
                lock (MoniverseBase.ConsoleWriterLock) {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info(@"           Five Minute Event         ");
                    Logger.Instance.Info("--------------------------------------");
                    Console.ResetColor();                
                }

                OnFiveMinutes(games, (int)MinuteTicks.FiveMinutes);
            }
            if (s.counter % 15 == 0)
            {
                lock (MoniverseBase.ConsoleWriterLock) {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info(@"        Fifteen Minute Event         ");
                    Logger.Instance.Info("--------------------------------------");
                    Console.ResetColor();                
                }

                OnFifteenMinutes(games, (int)MinuteTicks.FifteenMinutes);
            }
            if (s.counter % 30 == 0)
            {
                lock (MoniverseBase.ConsoleWriterLock) {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info(@"        Thirty Minute Event          ");
                    Logger.Instance.Info("--------------------------------------");
                    Console.ResetColor();                
                }

                OnThirtyMinutes(games, (int)MinuteTicks.ThirtryMinutes);
            }
            if (s.counter % 60 == 0)
            {
                lock (MoniverseBase.ConsoleWriterLock) {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info(@"           One Hour Event            ");
                    Logger.Instance.Info("--------------------------------------");
                    Console.ResetColor();                
                }

                OnSixtyMinutes(games, (int)MinuteTicks.OneHour);
            }
            if (s.counter % 360 == 0)
            {
                lock (MoniverseBase.ConsoleWriterLock) {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info(@"           Six Hour Event           ");
                    Logger.Instance.Info("--------------------------------------");
                    Console.ResetColor();                
                }

                OnSixHours(games, (int)MinuteTicks.SixHours);
            }
            if (s.counter % 720 == 0)
            {
                lock (MoniverseBase.ConsoleWriterLock) {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Logger.Instance.Info("--------------------------------------");
                    Logger.Instance.Info(@"          Twelve Hour Event          ");
                    Logger.Instance.Info("--------------------------------------");
                    Console.ResetColor();                
                }

                OnTwelveHours(games, (int)MinuteTicks.TwelveHours);
            }
            try
            {
                if (OnCheck != null) {
                    OnCheck(this, s.counter);       
                }
            }
            catch (Exception ex)
            {
               /* Do Nothing as this is for the console app exclusively*/
            }

        }
    }




}
