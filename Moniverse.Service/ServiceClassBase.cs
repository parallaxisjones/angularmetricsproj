using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Moniverse;
using Moniverse.Contract;
using Moniverse.Service;
using System.ServiceModel;
namespace Moniverse.Service
{
    public abstract class ServiceClassBase
    {
        public static Action<UseServiceDelegate<IWriterService>> Service = Service<IWriterService>.Use;
        public ServiceClassBase(){
            //Writer.Channel.Faulted += Channel_Faulted;
            //Service<IWriterService>.Use(service => {
            //    Insert = service.Insert;
            //    Update = service.Update;
            //});
        }

        //void Channel_Faulted(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        Writer.Refresh();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("error: " + ex.Message + " : could Not Refresh Writer Channel");                
        //    }

        //}
    }
}
