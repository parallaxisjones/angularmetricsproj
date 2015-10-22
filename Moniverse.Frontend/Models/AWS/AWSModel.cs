using System;
using System.Collections.Generic;
using System.Net;
using Moniverse.Contract;
using Newtonsoft.Json;

namespace PlayverseMetrics.Models.AWS
{
    public class AWSModel
    {
        public AWSSummary FetchNetflixIceData(DateTime start, DateTime end)
        {
            WebClient webClient = new WebClient();

            string netflixIceUrl = string.Format("http://ec2-52-20-140-182.compute-1.amazonaws.com:8081/api/daily?start={0}&end={1}", start.ToString("yyyy-MM-ddT00:00:00.000Z"), end.Date.AddDays(1).ToString("yyyy-MM-ddT00:00:00.000Z"));
            string json = webClient.DownloadString(netflixIceUrl);

            var awsSummary = JsonConvert.DeserializeObject<AWSSummary>(json);

            return awsSummary;
        }
    }
}