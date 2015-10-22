using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Keen.Core;
using Keen.Core.Query;
using Utilities;
using System.IO;

namespace Moniverse.Service
{

    public interface IAnalyticsProvider
    {
        IEnumerable<dynamic> GetResource(string CollectionName, DateTime CollectionStart, DateTime CollectionEnd);
        JArray GetProviderSchemas();
    }

    public class KeenIO : KeenClient, IAnalyticsProvider
    {
        private static ProjectSettingsProvider settings = new ProjectSettingsProvider("54d4f1d7d2eaaa109ff065c7", "23BCE6ECA87D76B7033A0FF7A6CBE859");

        private KeenIO(ProjectSettingsProvider settings) : base(settings) { }

        public static KeenIO Instance = new KeenIO(settings);


        public IEnumerable<dynamic> GetResource(string CollectionName, DateTime CollectionStart, DateTime CollectionEnd)
        {
            DateTime end = CollectionEnd;
            DateTime start = CollectionStart;
            QueryAbsoluteTimeframe range = new QueryAbsoluteTimeframe(start, end);
            string KeenLogMessage = String.Format("Querying Analytics Provider: {0} for Collection: {1} for Timeframe {2} - {3}", "Keen", CollectionName, CollectionStart.ToString(), CollectionEnd.ToString());
            string KEENCALLLOG = String.Format("KEEN CALL : {0} : {1} - {2}", CollectionName, CollectionStart.ToString(), CollectionEnd.ToString());
            Logger.Instance.Info(KeenLogMessage);
            IEnumerable<dynamic> result = new List<dynamic>();
            try
            {
                result = QueryExtractResource(CollectionName, range);
                KeenLog specialLog = new KeenLog(KEENCALLLOG);
                WriteKeenLog(specialLog);
            }
            catch (Exception ex)
            {
                try
                {
                    int retryCount = 0;
                    Retry.Do(() =>
                    {
                        retryCount++;
                        KeenLog retryLog = new KeenLog(String.Format("retry {0} : {1}", retryCount, KEENCALLLOG));
                        WriteKeenLog(retryLog);
                        result = QueryExtractResource(CollectionName, range);
                    }, TimeSpan.FromSeconds(20));
                }
                catch (AggregateException xxx)
                {
                    foreach (Exception x in xxx.InnerExceptions)
                    {
                        Logger.Instance.Exception(x.Message, x.StackTrace);
                    }
                }
                //throw ex;
            }


            return result;
        }
        public JArray GetProviderSchemas()
        {
            return GetSchemas();
        }
        public bool RequestEmailExtraction(string email, string CollectionName, DateTime startDate, DateTime endDate)
        {
            //DateTime end = endDate;
            //DateTime start = startDate;
            bool success = false;
            QueryAbsoluteTimeframe range = new QueryAbsoluteTimeframe(startDate, endDate);
            try
            {
                QueryExtractResource(CollectionName, range, null, 0, email);
                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return success;
        }

        public void WriteKeenLog(KeenLog entry)
        {
            string logDir = "C:/Users/Public/MoniverseLog/KeenLogs/";
            string logPath = logDir + "KeenCalls_" + entry.LogDate;
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            // This could be optimised to prevent opening and closing the file for each write
            try
            {
                using (FileStream fs = File.Open(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (StreamWriter log = new StreamWriter(fs))
                    {
                        log.WriteLine(string.Format("{0}\t{1}", entry.LogTime, entry.Message));
                    }
                }
            }
            catch (Exception ex)
            {
                Retry.Do(() =>
                {
                    using (FileStream fs = File.Open(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (StreamWriter log = new StreamWriter(fs))
                        {
                            log.WriteLine(string.Format("{0}\t{1}", entry.LogTime, entry.Message));
                        }
                    }
                }, TimeSpan.FromSeconds(1));

            }
        }
    }

    public class KeenLog
    {
        public string Message { get; set; }
        public string LogTime { get; set; }
        public string LogDate { get; set; }

        public KeenLog(string message)
        {
            Message = message;
            LogDate = DateTime.Now.ToString("yyyy-MM-dd");
            LogTime = DateTime.Now.ToString("hh:mm:ss.fff tt");
        }
    }
}
