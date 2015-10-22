using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moniverse.Contract
{
    public class HostingInstanceMeta
    {
        public DateTime RecordTimestamp { get; set; }
        public string Id { get; set; }
        public string MachineID { get; set; }
        public string IP { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string GameId { get; set; }
        public string RegionHostingConfigurationId { get; set; }
        public int MinimumNumInstances { get; set; }
        public int MaximumFreeInstances { get; set; }
        public string HostingConfigurationName { get; set; }
        public string HostingRegionName { get; set; }
        public string GameVersionId { get; set; }
        public int GameVersionMajor { get; set; }
        public int GameVersionMinor { get; set; }
        public int GameVersionStatus { get; set; }
        public int GameVersionLabel { get; set; }
        public int Status { get; set; }
        public int Health { get; set; }
        public int MaximumComputeUnits { get; set; }
        public int TotalComputeUnits { get; set; }
        public int CalcTotalComputeUnits { get; set; }
        public int ServersCount { get; set; }
        public int CalcServersCount { get; set; }
        public int AvgComputeUnitsAcrossServers { get; set; }
        public int MaxUserCount { get; set; }
        public int UserCount { get; set; }
    }
}
