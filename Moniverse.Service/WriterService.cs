using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Moniverse.Contract;
using Moniverse.Service.Attributes;
using Utilities;
using Playverse.Data;
using System.ServiceProcess;
namespace Moniverse.Service
{
    [ServiceBehavior]
    [SvcErrorHandlerBehaviourAttribute]
    public class WriterService : IWriterService
    {
        public static WorkQueue _workStack;

        public WriterService() {
            _workStack = new WorkQueue();
        }
        
        [OperationBehavior]
        public MoniverseResponse Insert(MoniverseRequest request) {
            MoniverseResponse response = new MoniverseResponse()
            {
                TaskName = request.TaskName,
                TimeStamp = request.TimeStamp,
                Status = "Fail"
            };
            try
            {
                Logger.Instance.Info(String.Format("Insert Recieved - {0} - {1}", request.TaskName, request.TimeStamp));
                try
                {
                    DBManager.Instance.Insert(Datastore.Monitoring, request.Task);
                    Logger.Instance.Info(request.TaskName + " Success");
                }
                catch (Exception ex)
                {
                    _workStack.Execute(() => {
                        DBManager.Instance.Insert(Datastore.Monitoring, request.Task);
                        Logger.Instance.Info(request.TaskName + " Success");                    
                    });
                    Logger.Instance.Exception("Message: " + ex.Message + request.TaskName + " Error", ex.StackTrace);
                }
            }
            catch (Exception ex)
            {
                OperationContext.Current.Channel.Close();
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            response.Status = "Success";
            Logger.Instance.Info(response.ToString());
            return response;
        }

        [OperationBehavior]
        public MoniverseResponse Update(UpdateRequest request)
        {
            MoniverseResponse response = new MoniverseResponse()
            {
                TaskName = request.TaskName,
                TimeStamp = request.TimeStamp,
                Status = "Fail"
            };
            try
            {
                _workStack.Execute(() =>
                {
                    Logger.Instance.Info(String.Format("Update Recieved - {0} - {1}", request.TaskName, request.TimeStamp));
                    try
                    {
                        DBManager.Instance.Update(Datastore.Monitoring, request.Task);
                        Logger.Instance.Info(request.TaskName + " Success");
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Exception("Message: " + ex.Message + request.TaskName + " Error", ex.StackTrace);
                    }
                });
            }
            catch (Exception ex)
            {
                OperationContext.Current.Channel.Close();
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            response.Status = "Success";
            Logger.Instance.Info(response.ToString());
            return response;

        }
        [OperationBehavior]
        public MoniverseResponse QueueSize(MoniverseRequest request)
        {
            //_workStack.Execute(() =>
            //{
            //    Logger.Instance.Info(String.Format("Queue Size Request Recieved - {0} - {1}", request.TaskName, request.TimeStamp));
            //});
            MoniverseResponse response = new MoniverseResponse()
            {
                TaskName = "QUEUE SIZE",
                TimeStamp = request.TimeStamp,
                Status = "Success"
            };
            Logger.Instance.Info(response.ToString());
            return response;
        }
    }


}
