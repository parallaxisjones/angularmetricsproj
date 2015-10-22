using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Moniverse.Contract
{
    public class MoniverseResponse
    {
        [DataMember]
        public string TaskName { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public DateTime TimeStamp { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}, my status is {2}", TaskName, TimeStamp.ToString("yyyy/MM/dd HH:mm:ss"), Status);
        }
    }
}
