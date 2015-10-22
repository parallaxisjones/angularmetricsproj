using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PlayVerse.Core;
using PlayVerse.Client.Service;

namespace Moniverse.Contract
{
    #region DTOs

    public class Authenticated
    {
        public bool isAuthenticated { get; set; }
        public object SessionToken { get; set; }
        public string username { get; set; }
        public string profileImageUrl { get; set; }
        public string loginTime { get; set; }
        public bool stayLoggedIn { get; set; }
        public string GameId { get; set; }
        public string localStorageID { get; set; }
        public bool isPlayverseAdmin { get; set; }
    }

    public class AuthenticatedUser
    {
        public UserSessionInfo UserSessionInfo { get; set; }
        public Authenticated UserInfo { get; set; }
    }

    public class RegionSessionTypeCounts
    {
        public string RegionName { get; set; }
        public string SessionType { get; set; }
        public int Count { get; set; }
    }

    public class UserSessionMeta : IEquatable<UserSessionMeta>, IComparable<UserSessionMeta>
    {
        public string UserId { get; set; }
        public string GameId { get; set; }
        public string UserSessionId { get; set; }
        public string Platform { get; set; }
        public DateTime LoginTimestamp { get; set; }
        public DateTime LogoffTimestamp { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public float Longitude { get; set; }
        public float Latitude { get; set; }
        public int LocationId { get; set; }
        public long SessionLength { get; set; }
        public int InstallDateRecord { get; set; }
        public RetentionCohortType RetentionCohortType { get; set; }
        public string RecordDate { get; set; }

        public bool Equals(UserSessionMeta login)
        {
            if ((this.UserSessionId == login.UserSessionId))
            {
                return true;
            }

            return false;
        }

        public int CompareTo(UserSessionMeta login)
        {
            if ((this.LoginTimestamp > login.LoginTimestamp))
            {
                return 1;
            }
            else if ((this.LoginTimestamp < login.LoginTimestamp))
            {
                return -1;
            }
            return 0;
        }

    }

    public class Install : UserSessionMeta
    {
        public bool Equals(Install user)
        {
            if ((this.LoginTimestamp == user.LoginTimestamp) && (this.UserId == user.UserId))
            {
                return true;
            }

            return false;
        }
    }

    public class Logoff : UserSessionMeta
    {
        public bool Equals(Logoff user)
        {
            if (base.Equals(user) == false)
            {
                if ((this.UserSessionId == user.UserSessionId))
                {
                    return true;
                }

            }
            return false;

        }
        public int CompareTo(UserSessionMeta login)
        {
            if ((this.LogoffTimestamp > login.LogoffTimestamp))
            {
                return 1;
            }
            else if ((this.LogoffTimestamp < login.LogoffTimestamp))
            {
                return -1;
            }
            return 0;
        }
    }

    public class Login : UserSessionMeta
    {
        public bool Equals(Login user)
        {
            if (base.Equals(user) == false)
            {
                if ((this.UserSessionId == user.UserSessionId))
                {
                    return true;
                }
            }
            return false;

        }

        public override string ToString()
        {
            return String.Format("Date: {0}\tUserId: {1}\t\tUserSessionId: {2}\tCohort: {3}", LoginTimestamp.ToString(),
                UserId, UserSessionId, RetentionCohortType);
        }
    }
    public class ExternalAccountSummary
    {
        public string Type { get; set; }
        public int Count { get; set; }
    }

    public class UserRegistrationTracking
    {
        public DateTime RecordTimestamp { get; set; }
        public int Count { get; set; }
    }

    public class EventListenerTypeSummary
    {
        public string Type { get; set; }
        public int Count { get; set; }
    }

    public class DailyActiveUserSummary
    {
        public DateTime RecordTimestamp { get; set; }
        public int Count { get; set; }
    }
    public class PlaytricsResult
    {
        public int status { get; set; }
        public double interval { get; set; }
        public double start { get; set; }
        public Dictionary<string, double> stats { get; set; }
        public string groupBy { get; set; }
        public string region { get; set; }
        public Dictionary<string, double[]> data { get; set; }
        public double[] time { get; set; }
    }

    public class GameUserActivity
    {
        public string GameId { get; set; }
        public string RegionName { get; set; }
        public DateTime RecordTimestamp { get; set; }
        public int GameSessionUsers { get; set; }
        public int EventListeners { get; set; }
        public int TitleScreenUsers { get; set; }
        public string SessionTypeName_0 { get; set; }
        public int SessionTypeUsers_0 { get; set; }
        public string SessionTypeName_1 { get; set; }
        public int SessionTypeUsers_1 { get; set; }
        public string SessionTypeName_2 { get; set; }
        public int SessionTypeUsers_2 { get; set; }
        public string SessionTypeName_3 { get; set; }
        public int SessionTypeUsers_3 { get; set; }
        public string SessionTypeName_4 { get; set; }
        public int SessionTypeUsers_4 { get; set; }
        public string SessionTypeName_5 { get; set; }
        public int SessionTypeUsers_5 { get; set; }
        public string SessionTypeName_6 { get; set; }
        public int SessionTypeUsers_6 { get; set; }
        public string SessionTypeName_7 { get; set; }
        public int SessionTypeUsers_7 { get; set; }
        public int SessionTypeUsers_Other { get; set; }
    }

    public class GameSessionUserStats
    {
        public string GameId { get; set; }
        public string SessionType { get; set; }
        public int MaxNumPlayers { get; set; }
        public decimal AvgPlayers { get; set; }
        public int Sessions { get; set; }
        public decimal PrivateAvgPlayers { get; set; }
        public int PrivateSessions { get; set; }
        public decimal TotalAvgPlayers { get; set; }
        public int TotalSessions { get; set; }
        public DateTime RecordTimestamp { get; set; }
    }
    public class OnlineBySessionTypePoint
    {
        public DateTime timestamp;
        public string sessionType;
        public string count;
    }

    public class OnlineBySessionTypeSeries
    {
        public DateTime start;
        public DateTime end;
        public TimeInterval interval;
        public List<OnlineBySessionTypePoint> Series;
    }


    public class AvgData
    {
        public string sessionType;
        public int maxNumPlayers;
        public string avgPlayers;
        public string privateAvgPlayers;
    }

    public class GeoJsonGeometry
    {
        public string type { get; set; }
        public float[] coordinates { get; set; }
    }
    public class GeoJsonProperties
    {
        public string title { get; set; }
        public int count { get; set; }
        public long timestamp { get; set; }
        public string description { get; set; }
        public string markerColor { get; set; }
        public string markerSymbol { get; set; }
        public string markerSize { get; set; }
    }
    public class GeoJsonFeature
    {
        public string type { get; set; }
        public GeoJsonProperties properties { get; set; }
        public GeoJsonGeometry geometry { get; set; }
    }
    public class GeoJsonData
    {
        public string type { get; set; }
        public List<GeoJsonFeature> features { get; set; }

    }

    public class USA48Ranges
    {
        public float NorthBound
        {
            get
            {
                return 48.987386F;
            }
        }
        public float SouthBound
        {
            get
            {
                return 18.005611F;
            }
        }
        public float EastBound
        {
            get
            {
                return -62.361014F;
            }
        }
        public float WestBound
        {
            get
            {
                return -124.626080F;
            }
        }
    }
    public class AmericasRanges
    {
        public float NorthBound
        {
            get
            {
                return 48.987386F;
            }
        }
        public float SouthBound
        {
            get
            {
                return 18.005611F;
            }
        }
        public float EastBound
        {
            get
            {
                return -62.361014F;
            }
        }
        public float WestBound
        {
            get
            {
                return -124.626080F;
            }
        }
    }
    #endregion
}
