using Playverse.Data;
using System;
using System.Data;
using System.Linq;
using System.ServiceProcess;

namespace PlayverseMetrics.Models
{
    public class SystemsModel
    {

        public static SystemsModel instance = new SystemsModel();

        public string CheckWindowsServiceStatus()
        {
            string result = String.Empty;
            string status = String.Empty;
            string lastUpdate = String.Empty;
            ServiceController sc = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == "MoniverseWriter");

            if (sc != null)
            {
                switch (sc.Status)
                {
                    case ServiceControllerStatus.Running:
                        status = "Running";
                        break;
                    case ServiceControllerStatus.Stopped:
                        status = "Stopped";
                        break;
                    case ServiceControllerStatus.Paused:
                        status = "Paused";
                        break;
                    case ServiceControllerStatus.StopPending:
                        status = "Stopping";
                        break;
                    case ServiceControllerStatus.StartPending:
                        status = "Starting";
                        break;
                    default:
                        status = "Status Changing";
                        break;
                }
            }
            else
            {
                status = "Not Installed";
            }

            result = String.Format("{0}", status);

            return result;
        }
    }
}