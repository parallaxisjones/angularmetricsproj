using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moniverse.Contract
{
    public class GameMonitoringConfig
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string ShortTitle { get; set; }
        public DateTime LaunchTime { get; set; }
        public List<string> ActiveUserSessionTypes { get; set; }
        public float ActiveUserDeltaThresholdPct_3Min { get; set; }
        public float ActiveUserDeltaThresholdPct_6Min { get; set; }
        public List<PrivacyCompareSessionTypes> PrivacyCompareSessionTypes { get; set; }
        public int MaxRunningHostingInstances { get; set; }
        public NotificationSettings NotificationSettings { get; set; }

        public bool IsNotificationSettingEnabled(NotificationLevel currentNotificationLevel)
        {
            bool shouldContinue = true;
            switch (currentNotificationLevel)
            {
                case NotificationLevel.Info:
                    shouldContinue = NotificationSettings.Info;
                    break;
                case NotificationLevel.Warning:
                    shouldContinue = NotificationSettings.Warning;
                    break;
                case NotificationLevel.Error:
                    shouldContinue = NotificationSettings.Error;
                    break;
                case NotificationLevel.PVSupport:
                    shouldContinue = NotificationSettings.PVSupport;
                    break;
            }
            return shouldContinue;
        }
    }

    public class PrivacyCompareSessionTypes
    {
        public List<string> PublicTypes { get; set; }
        public List<string> PrivateTypes { get; set; }
    }
}
