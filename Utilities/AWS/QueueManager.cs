using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
namespace Utilities
{
    public class QueueManager
    {
        private string _queueUrl = "";

        public QueueManager(string queueName) {
            _queueUrl = GetOrCreateQueue(queueName);    
        }

        private static AmazonSQSClient sqs = new AmazonSQSClient("AKIAJHGOA5MAPPB3JCTA", "/EqP+uv+q+T7b3TkfBA/Xn7f4finoQEZgwSKOZ1K", RegionEndpoint.USEast1);
        
        public string GetOrCreateQueue(string QueueName){
            string queueUrl = "";

            foreach (string queue in GetQueueList()) {
                string[] parts = queue.Split('/');
                foreach (string part in parts) {
                    if (part.Contains(QueueName)) {
                        queueUrl = part;
                    }
                }
            }
            if (queueUrl == "") {
                try
                {
                    //Creating a queue
                    Console.WriteLine("Creating a queue called {0}.\n", QueueName);
                    CreateQueueRequest sqsRequest = new CreateQueueRequest();
                    try
                    {
                        sqsRequest.QueueName = QueueName;
                        CreateQueueResponse createQueueResponse = sqs.CreateQueue(sqsRequest);
                        queueUrl = createQueueResponse.QueueUrl;
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Exception(ex.Message, ex.StackTrace);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Exception("error: " + ex.Message, ex.StackTrace);
                }            
            }

            return queueUrl;
        }

        public List<string> GetQueueList() {
            //Confirming the queue exists
            ListQueuesRequest listQueuesRequest = new ListQueuesRequest();
            ListQueuesResponse listQueuesResponse = new ListQueuesResponse()
            {
                ContentLength = 0
            };
            List<string> QueueList = new List<string>();
            try
            {
            //Console.WriteLine("Printing list of Amazon SQS queues.\n");
                listQueuesResponse = sqs.ListQueues(listQueuesRequest);
                if (listQueuesResponse.ContentLength == 0)
                {
                    Logger.Instance.Info("no queues in list");
                    return QueueList;
                }
                
                foreach (String Url in listQueuesResponse.QueueUrls)
                {
                    QueueList.Add(Url);
                }                

            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return QueueList;      
        }

        public string AddMessage(string message) {
            //Sending a message
            Console.WriteLine("Sending a message to MyQueue.\n");
            SendMessageRequest sendMessageRequest = new SendMessageRequest();
            SendMessageResponse response = new SendMessageResponse();
            try
            {
                sendMessageRequest.QueueUrl = _queueUrl;
                sendMessageRequest.MessageBody = message;
                response = sqs.SendMessage(sendMessageRequest);
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex.Message, ex.StackTrace);
            }

            return response.MessageId;
        }

        public string TakeMessage() {
            //Receiving a message
            ReceiveMessageRequest receiveMessageRequest = new ReceiveMessageRequest();
            receiveMessageRequest.QueueUrl = _queueUrl;
            ReceiveMessageResponse receiveMessageResponse = sqs.ReceiveMessage(receiveMessageRequest);
            string task = "";
            //Console.WriteLine("Printing received message.\n");
            foreach (Message message in receiveMessageResponse.Messages)
            {
                //Console.WriteLine("  Message");
                //Console.WriteLine("    MessageId: {0}", message.MessageId);
                //Console.WriteLine("    ReceiptHandle: {0}", message.ReceiptHandle);
                //Console.WriteLine("    MD5OfBody: {0}", message.MD5OfBody);
                //Console.WriteLine("    Body: {0}", message.Body);
                task = message.Body;

                //foreach (KeyValuePair<string, string> entry in message.Attributes)
                //{
                //    Console.WriteLine("  Attribute");
                //    Console.WriteLine("    Name: {0}", entry.Key);
                //    Console.WriteLine("    Value: {0}", entry.Value);
                //}
            }
            string messageRecieptHandle = receiveMessageResponse.Messages[0].ReceiptHandle;

            RemoveMessage(messageRecieptHandle);

            return task;
        }

        public void RemoveMessage(string messageReceiptHandle)
        {
            //Deleting a message
            Console.WriteLine("Deleting the message.\n");
            DeleteMessageRequest deleteRequest = new DeleteMessageRequest();
            deleteRequest.QueueUrl = _queueUrl;
            deleteRequest.ReceiptHandle = messageReceiptHandle;
            sqs.DeleteMessage(deleteRequest);        
        }
    }
}
