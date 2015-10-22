//using Moniverse.Reader;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading;
//using System.Threading.Tasks;
//using Utilities;

//namespace Moniverse.Service
//{


//    public enum MoniverseCommands
//    {
//        WakeUp = 128,
//        Sleep = 129,
//        ToggleLogging = 130
//    }

//    public class MoniverseServer : HttpServer
//    {


//        public MoniverseServer(int port)
//            : base(port)
//        {
//        }

//        public static string bUrlContainsBlah(string blah, string keywords)
//        {
//            Match regexMatch = Regex.Match(blah, @"\b" + keywords + @"\b", RegexOptions.Singleline | RegexOptions.IgnoreCase);

//            return regexMatch.Value;
//        }

//        public override void handleGETRequest(HttpProcessor p)
//        {
//            Logger.Instance.Info(String.Format("request: {0}", p.http_url));
//            p.writeSuccess();

//            switch (p.http_url)
//            {
//                default:
//                case "/command":
//                    //p.outputStream.WriteLing(p.http_url)
//                    p.outputStream.WriteLine("<img src='http://thankyou.dungeondefenders2.com/pimgpsh_fullsize_distr.jpg'/>");
//                    break;

//                case "/process":
//                    p.outputStream.WriteLine("processing retention");
//                    Retention.Instance.CalculateRetention(14);
//                    break;
//            }


//        }

//        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
//        {
//            //Logger.Instance.Info("POST request: {0}", p.http_url);
//            string data = inputData.ReadToEnd();

//            p.outputStream.WriteLine("<html><body><h1>test server</h1>");
//            p.outputStream.WriteLine("<a href=/test>return</a><p>");
//            p.outputStream.WriteLine("postbody: <pre>{0}</pre>", data);
//        }
//    }
//}
