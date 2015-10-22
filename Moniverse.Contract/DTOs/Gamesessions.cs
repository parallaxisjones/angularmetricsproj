using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moniverse.Contract
{
    public class GameSessionMeta
    {
        public DateTime RecordCreated { get; set; }
        public DateTime RecordLastUpdateTime { get; set; }
        public string GameSessionId { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public DateTime CreationTime { get; set; }
        public bool SessionStarted { get; set; }
        public string GameId { get; set; }
        public bool IsLocallyEmulated { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public int Major { get; set; }
        public int Minor { get; set; }
        public int UsersCount { get; set; }
        public string SessionTypeId { get; set; }
        public string SessionTypeFriendly { get; set; }
        public int Status { get; set; }
        public int CurrentRanking { get; set; }
        public bool IsPartySession { get; set; }
        public int GameSessionRankRangeMin { get; set; }
        public int GameSessionRankRangeMax { get; set; }
        public DateTime StartLaunchCountDownTime { get; set; }
        public bool IsPrivateSession { get; set; }
        public string InitiatorUserId { get; set; }
        public bool IsHosted { get; set; }
        public string SessionMetadata { get; set; }
    }
}
