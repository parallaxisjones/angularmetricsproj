using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Moniverse.Contract
{

    [DataContract]
    public class MoniverseRequest
    {
        [DataMember]
        public string TaskName { get; set; }

        [DataMember]
        public string Task { get; set; }

        [DataMember]
        public DateTime TimeStamp { get; set; }

        [DataMember]
        public string ServiceName { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}, my name is {2}", Task, TimeStamp.ToString("yyyy/MM/dd HH:mm:ss"), ServiceName);
        }
    }
    [DataContract]
    public class UpdateRequest
    {
        [DataMember]
        public string TaskName { get; set; }

        [DataMember]
        public List<string> Task { get; set; }

        [DataMember]
        public DateTime TimeStamp { get; set; }

        [DataMember]
        public string ServiceName { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}, my name is {2}", Task, TimeStamp.ToString("yyyy/MM/dd HH:mm:ss"), ServiceName);
        }
    }
}
