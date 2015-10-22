using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Utilities;
using Moniverse.Contract;
using Microsoft.AspNet.Identity;
using Playverse.Data;
using PlayVerse.Core;
using PlayVerse.Client.Service;
using MySql.Data.MySqlClient;
namespace PlayverseMetrics
{
    public class MoniverseUser {
        public string Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string LastNotificationSeen { get; set; }
        public string DashContent { get; set; }
    }
    public class PVAuth
    {
        public virtual string UserTable { 
            get {
                return "User";
            } 
        }

        public AuthenticatedUser Login(string email, string password)
        {
            AuthenticatedUser user = PlayVerseAPI.Instance.Login(email, password);

             if (!String.IsNullOrEmpty(user.UserSessionInfo.AuthenticationToken) && (user.UserSessionInfo.IsPlayVerseAdmin || user.UserSessionInfo.IsGameAdmin))
             {

                 string hash = HashPassword(password);
                 user.UserInfo.isAuthenticated = true;
                 user.UserInfo.SessionToken = user.UserSessionInfo.AuthenticationToken;
                 user.UserInfo.localStorageID = user.UserSessionInfo.UserIdentityId;
                 user.UserInfo.isPlayverseAdmin = user.UserSessionInfo.IsPlayVerseAdmin;

                 try
                 {
                     string get = String.Format("select * from {0} where Id = '{1}';", UserTable, user.UserSessionInfo.UserIdentityId);
                     List<MoniverseUser> getResult = DBManager.Instance.Query<MoniverseUser>(Datastore.Monitoring, get).ToList();
                     if (getResult.Count > 0 && IsValidPassword(password, getResult.FirstOrDefault().PasswordHash)) {
                         return user;
                     }
                     else
                     {
                         string record = String.Format(@"INSERT INTO {0} (Id, Username, PasswordHash, Email) VALUES ('{1}', '{2}', '{3}', '{4}') ON DUPLICATE KEY UPDATE Username = '{2}', PasswordHash= '{3}', Email = '{4}';", UserTable, user.UserSessionInfo.UserIdentityId, user.UserSessionInfo.GamerName, hash, email);
                         try
                         {
                             DBManager.Instance.Insert(Datastore.Monitoring, record);
                         }
                         catch (Exception ex) { }
                     }
                 }
                 catch (Exception ex)
                 {
                     Logger.Instance.Exception(String.Format("Login Failure: {0}", ex.Message), ex.StackTrace);
                 }
                 
             }
             else {
                 user.UserInfo.isAuthenticated = false;
             }

             return user;
        }

        public void LogOff() {
            PlayVerseAPI.Instance.Logoff();   
        }

        public string HashPassword(string password) {
            PasswordHasher hasher = new PasswordHasher();
            return hasher.HashPassword(password);
        }

        public bool IsValidPassword(string password, string hash)
        {
            PasswordHasher hasher = new PasswordHasher();
            PasswordVerificationResult result = hasher.VerifyHashedPassword(hash, password);
            if (result == PasswordVerificationResult.Success)
                return true;

            return false;
        }
    }
}