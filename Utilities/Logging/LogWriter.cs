using System;
using System.Management;
using System.Reflection;
using System.Text;
using NLog;

namespace Utilities
{
    public class LogWriter
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private static LogWriter instance;

        public string RunningVersion = "";
        public string RunningApplication = "Moniverse";
        public string RunningEnvironment = "";

        private LogWriter() { }

        public static LogWriter Instance
        {
            get
            {
                // If the instance is null then create one and init the Queue
                if (instance == null)
                {
                    instance = new LogWriter();
                }
                return instance;
            }
        }

        /// <summary>
        /// The single instance method that writes to the log file
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        public void WriteToLog(string message)
        {
            logger.Info(message);           
        }

        public string sweetASCIIArt()
        {
            StringBuilder sb = new StringBuilder();


                sb.Append(
                    @"
                 ^    ^    ^    ^    ^    ^    ^    ^    ^    ^    ^    ^    ^    ^    ^    ^  
                /M\  /O\  /N\  /I\  /V\  /E\  /R\  /S\  /E\  /_\  /L\  /O\  /G\  /G\  /E\  /R\ 
               <___><___><___><___><___><___><___><___><___><___><___><___><___><___><___><___>"
                    );
#if DEBUG 
                RunningEnvironment = "Debug";
#elif LOCAL
            RunningEnvironment = "LocalNodebug";
#elif STAGING
                RunningEnvironment = "Staging";
#else
                RunningEnvironment = "Production";
#endif

                Assembly ExecutingAssembly = Assembly.GetEntryAssembly();
                RunningVersion = AssemblyName.GetAssemblyName(ExecutingAssembly.Location).Version.ToString();
                sb.Append(String.Format(@"{0} : {1} version {2}", RunningEnvironment, RunningApplication, RunningVersion));
                foreach (var item in new ManagementObjectSearcher("Select * from Win32_ComputerSystem").Get())
                {
                    sb.Append(String.Format("Number Of Physical Processors: {0} \r\n", item["NumberOfProcessors"]));
                }

                int coreCount = 0;
                foreach (var item in new ManagementObjectSearcher("Select * from Win32_Processor").Get())
                {
                    coreCount += int.Parse(item["NumberOfCores"].ToString());
                }
                sb.Append(String.Format("Number Of Cores: {0} \r\n", coreCount));

                foreach (var item in new ManagementObjectSearcher("Select * from Win32_ComputerSystem").Get())
                {
                    sb.Append(String.Format("Number Of Logical Processors: {0} \r\n", item["NumberOfLogicalProcessors"]));
                }

            return sb.ToString();

        }
    }
}