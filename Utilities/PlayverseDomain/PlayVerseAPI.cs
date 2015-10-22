using System;
using System.Web;
using PlayVerse.Client.Service;
using PlayVerse.Core;
using Moniverse.Contract;
/// <summary>
/// Summary description for Base
/// </summary>
namespace Utilities {
    public class PlayVerseAPI
    {
        private Uuid MoniverseClientKey = Uuid.FromString("0-b3b65581f9974a81be73e2f7dac1cb05");
        private PlayVerseConnection PVConnection { get; set; }

        private PlayVerseAPI(PlayverseEnvironment environment)
        {
            string serviceUrl = EnvironmentManager.GetInternalServiceUrl(environment, false);
            string authUrl = EnvironmentManager.GetAuthenticationServiceUrl(environment, false);
            string logUrl = EnvironmentManager.GetLoggingServiceUrl(environment, false);

            PVConnection = new PlayVerseConnection(new Uri(serviceUrl), new Uri(authUrl), new Uri(logUrl), MoniverseClientKey, Uuid.Empty, Uuid.Empty, "PVCLIENT_MONIVERSE");
        }

        public static PlayVerseAPI Instance = new PlayVerseAPI(PlayverseEnvironment.Local);

        //public static PlayVerseAPI GetInstance(PlayverseEnvironment env) {
        //    Instance = new PlayVerseAPI(env);
        //    return Instance;
        //}

        //Stub
        public AuthenticatedUser Login(string email, string password)
        {

            AuthenticatedUser info = new AuthenticatedUser()
            {
                UserInfo = new Authenticated() { 
                    isAuthenticated = false
                },
            };

        try
        {
            info.UserSessionInfo = PVConnection.Get<UserAuth>().LoginWithPlayverse(email, password);
            info.UserInfo.isAuthenticated = true;
        }
        catch (Exception ex)
        {
            Logger.Instance.Exception(ex.Message, ex.StackTrace);
            PVConnection.Get<Logging>().AddLog(ErrorType.Exception, "MoniVerse PlayVerseAPI", "Login", Uuid.Empty, ex.Message, ex.StackTrace, ErrorLevel.Low);
        }


        return info;
    }

        public void Logoff()
        {

            try
            {
                PVConnection.Get<UserAuth>().LogOff();
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
                PVConnection.Get<Logging>().AddLog(ErrorType.Exception, "MoniVerse PlayVerseAPI", "Logoff", Uuid.Empty, ex.Message, ex.StackTrace, ErrorLevel.Low);
            }

        }

    }
}