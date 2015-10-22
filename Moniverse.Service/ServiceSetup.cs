using System.ServiceProcess;

namespace Moniverse.Service
{
    public class ServiceSetup
    {
        private readonly string _servicename;

        public ServiceSetup(string ServiceName)
        {
            _servicename = ServiceName;
        }

        public void StartAfterInstall(bool shouldStart = true)
        {
            if (shouldStart)
            {
                using (ServiceController sc = new ServiceController(_servicename))
                {
                    sc.Start();
                }
            }
        }
    }
}
