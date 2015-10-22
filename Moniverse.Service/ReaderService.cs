using Moniverse.Contract;
using System.ServiceModel;

namespace Moniverse.Service
{
    public abstract class MoniverseBase
    {
        public static object ConsoleWriterLock = new object();
    }

    [ServiceBehavior]
    public class ReaderService : MoniverseBase, IReaderService
    {
        [OperationBehavior]
        public MoniverseResponse RunRentention(MoniverseRequest request){

            return new MoniverseResponse()
            {
                TaskName = "Retention",
                Status = "Success",
                TimeStamp = request.TimeStamp
            };
        }
    }

}
