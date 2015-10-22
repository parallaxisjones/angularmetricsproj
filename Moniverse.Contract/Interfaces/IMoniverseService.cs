using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;


namespace Moniverse.Contract
{
    [ServiceContract]
    public interface IWriterService
    {
        [OperationContract]
        MoniverseResponse Insert(MoniverseRequest request);

        [OperationContract]
        MoniverseResponse Update(UpdateRequest request);

        [OperationContract]
        MoniverseResponse QueueSize(MoniverseRequest request);

    }
    [ServiceContract]
    public interface IReaderService
    {
        [OperationContract]
        MoniverseResponse RunRentention(MoniverseRequest request);
    }
}
