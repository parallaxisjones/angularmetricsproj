using System;
using System.ServiceModel;
using Topshelf;
using ServiceModelEx;
using Moniverse.Service;
using System.Diagnostics;
using System.Threading;
using NLog;
using Playverse.Data;

namespace Moniverse.Writer
{
    internal class Host
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private ServiceHost<WriterService> _service;

        internal Host()
        {
            logger.Info("Setting up services...");
            
            _service = new ServiceHost<WriterService>(new Uri[] { });
        }

        public void Start()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            logger.Info("wide opened queue that will do work...");
            _service.Open();
            logger.Info("Started!");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void Stop()
        {
            logger.Info("Stopping Writer services...");
            try
            {
                if (_service != null)
                {
                    if (_service.State == CommunicationState.Opened)
                    {
                        _service.Close();
                    }
                }
                logger.Info("Stopped!");
            }
            catch (Exception ex)
            {
                logger.Info("Could not stop: " + ex.Message);
            }
        }
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            logger.Info("Moniverse Writer Service - {0}", DBManager.Instance.GetEnvironment().ToString());
            Console.ResetColor();
            try
            {
                const string name = "MoniverseWriter";
                const string description = "Moniverse Writer Service open for biz";
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
                    configuration.BeforeInstall(() =>
                    {
                        //do some stuff here that is relevant
                        
                    });
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

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Writer Service fatal exception. ");
                
                foreach (Thread _thread in Process.GetCurrentProcess().Threads) {
                    _thread.Join();
                }
                Environment.FailFast("DIE");
            }
            Console.ReadKey();
        }
    }
}
