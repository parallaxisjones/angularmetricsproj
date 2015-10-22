using System;
using System.Collections.Generic;
using System.ServiceModel;
using Topshelf;
using ServiceModelEx;
using Moniverse.Contract;
using Moniverse.Service;
using Utilities;
using Amib.Threading;

namespace Moniverse.Reader
{
    internal class Host
    {
        private ServiceHost<ReaderService> _service;
        private List<GameMonitoringConfig> games;
        private IntervalHandler timer;
        public static QueueManager GameWorkQueue = new QueueManager("MoniverseTaskQueue");       
        public static SmartThreadPool ReaderThreads;

        internal Host()
        {
            Logger.Instance.Info("Setting up all the services...");
            Logger.Instance.Info("Setting up DB Writer Proxy service...");

            ReaderThreads = new SmartThreadPool();
            Logger.Instance.Info("Setting up Reader services...");
            TimerState ts = new TimerState();

            timer = new IntervalHandler(ts, ReaderThreads);
            Logger.Instance.Info("initialized timer");
            
            GC.KeepAlive(timer);
            Logger.Instance.Info("told that GC to ignore timer object");
            
            _service = new ServiceHost<ReaderService>(new Uri[] { });
            games = Games.Instance.GetMonitoredGames();
        }

        public void Start()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            _service.Open();
            Logger.Instance.Info("Started!");
            Console.ForegroundColor = ConsoleColor.White;
            MonitoringEvents.instance.SetEventCallbacks(timer, games, GameWorkQueue);

            int WaitTillWholeMin = (60 - DateTime.UtcNow.Second) * 1000; //this is telling this guy to wait until the next whoe minute (:00 seconds)
            timer.run((long)60000, WaitTillWholeMin, games);
        }

        public void Stop()
        {
            Logger.Instance.Info("Stopping services...");
            ReaderThreads.Shutdown();
            try
            {
                if (_service != null)
                {
                    if (_service.State == CommunicationState.Opened)
                    {
                        _service.Close();
                    }
                }
                Logger.Instance.Info("Stopped!");
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception("Could not stop: " + ex.Message, ex.StackTrace);
            }
        }

        static void Main(string[] args)
        {
            Logger.Instance.Info("Interval Reader Service Started");
            try
            {
                const string name = "PlayverseReader";
                const string description = "Playverse Reader Service ";
                var host = HostFactory.New(configuration =>
                {
                    configuration.Service<Host>(callback =>
                    {
                        callback.ConstructUsing(s => new Host());
                        callback.WhenStarted(service => service.Start());
                        callback.WhenStopped(service => service.Stop());
                    });
                    configuration.SetDisplayName(name);
                    configuration.SetServiceName(name);
                    configuration.SetDescription(description);
                    configuration.StartAutomatically();
                    configuration.AfterInstall(() =>
                    {
                        ServiceSetup installer = new ServiceSetup(name);
                        installer.StartAfterInstall();
                    });
                    configuration.EnableServiceRecovery(rc =>
                    {
                        rc.RestartService(0); // restart the service after 1 minute
                        rc.RestartService(0); // restart the service after 1 minute
                        rc.RestartService(0); // restart the service after 1 minute
                    });
                });
                host.Run();
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception("Reader Service fatal exception. " + ex.Message, ex.StackTrace);

                ReaderThreads.Shutdown();
                Environment.FailFast("READER DIE");
            }
        }
    }

}
