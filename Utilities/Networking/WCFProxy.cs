using System;
using System.ServiceModel;
using System.Threading;
using NLog;

namespace Utilities
{
    public delegate void UseServiceDelegate<TContract>(TContract proxy);

    public static class Service<TContract>
                 where TContract : class
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public static ChannelFactory<TContract> _channelFactory = new ChannelFactory<TContract>(typeof(TContract).Name + "_Endpoint");

        public static void Use(UseServiceDelegate<TContract> serviceTODoThing)
        {
            IClientChannel proxy = null;
            bool success = false;


            Exception mostRecentEx = null;
            int millsecondsToSleep = 1000;

            for (int i = 0; i < 5; i++)  // Attempt a maximum of 5 fucking times // shit will work eventually as we've seen
            {
                // Proxy literally can't even be reused
                proxy = (IClientChannel)_channelFactory.CreateChannel();
                //set default connection level configs here.
                // see below example: timeout and faulted event
                // in the callback that uses the delegate these can be ovverridden
                // this is by design and really fucking awesome -- PJ
                proxy.OperationTimeout = new TimeSpan(0, 10, 0);
                //proxy.Faulted += (object Sender, EventArgs args) =>
                //{
                //    Console.WriteLine("!!!! FAULTED CALLBACK WORKS!!!");
                //    proxy.Close();
                //    proxy.Open();
                //    proxy = (IClientChannel)_channelFactory.CreateChannel();
                //};                
                try
                {
                    serviceTODoThing((TContract)proxy);
                    proxy.Close();
                    success = true;

                    break;
                }
                catch (FaultException customFaultEx)
                {
                    logger.Error(customFaultEx, "WCFProxy-customFaultEx threw an exception");

                    mostRecentEx = customFaultEx;
                    proxy.Abort();

                    //  Custom resolution for this app-level exception
                    Thread.Sleep(millsecondsToSleep * (i + 1));
                }

                // ChannelTerminatedException is typically thrown on the client when a channel is terminated due to the server closing the connection.
                catch (ChannelTerminatedException cte)
                {
                    logger.Error(cte, "WCFProxy threw an exception");

                    mostRecentEx = cte;
                    proxy.Abort();
                    //  delay (backoff) and retry 
                    Thread.Sleep(millsecondsToSleep * (i + 1));
                }

                // EndpointNotFoundException is thrown when a remote endpoint could not be found or reached.  The endpoint may not be found or 
                // reachable because the remote endpoint is down, the remote endpoint is unreachable, or because the remote network is unreachable.
                catch (EndpointNotFoundException enfe)
                {
                    logger.Error(enfe, "WCFProxy threw an exception");

                    mostRecentEx = enfe;
                    proxy.Abort();
                    //  delay (backoff) and retry 
                    Thread.Sleep(millsecondsToSleep * (i + 1));
                }

                // ServerTooBusyException exception that is thrown when a server is too busy to accept a message.
                catch (ServerTooBusyException stbe)
                {
                    logger.Error(stbe, "WCFProxy threw an exception");

                    mostRecentEx = stbe;
                    proxy.Abort();

                    //  delay (backoff) and retry 
                    Thread.Sleep(millsecondsToSleep * (i + 1));
                }
                catch (TimeoutException timeoutEx)
                {
                    logger.Error(timeoutEx, "WCFProxy threw an exception");

                    mostRecentEx = timeoutEx;
                    proxy.Abort();

                    //  delay (backoff) and retry 
                    Thread.Sleep(millsecondsToSleep * (i + 1));
                }
                catch (CommunicationException comException)
                {
                    logger.Error(comException, "WCFProxy threw an exception");

                    mostRecentEx = comException;
                    proxy.Abort();

                    //  delay (backoff) and retry 
                    Thread.Sleep(millsecondsToSleep * (i + 1));
                }
                catch (Exception e)
                {
                    logger.Error(e, "WCFProxy threw an exception");

                    // rethrow any other exception not defined here
                    // in the future we can possibly define custom Exceptions to pass information such as failure count, and failure type, and other shit
                    proxy.Abort();
                    throw e;
                }
            }
            if (success == false && mostRecentEx != null)
            {
                proxy.Abort();

                logger.Error(mostRecentEx, "mostRecentEx");

                throw new Exception("WCF call failed after 5 retries.", mostRecentEx);
            }

        }

    }

}
