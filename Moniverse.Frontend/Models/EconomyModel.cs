using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moniverse.Contract;
using Playverse.Data;

namespace PlayverseMetrics
{
    public enum TransactionType { 
        AddCredits = 0,
        Purchase = 1
    }
    
    public class CoinFlowVector
    {
        public List<double> itemData;
        public List<double> coinData;
        public long StartDate;
        public long interval;
        public string Source;
        public string Category;
        public string Type;
    }
   
    public class FlowDataView {
        public List<CoinFlowVector> Inflows;
        public List<CoinFlowVector> Outflows;
        public long StartDate;
        public long interval;
    }

    public class EconomyModel
    {
        public static EconomyModel instance = new EconomyModel();

        public List<CoinFlowVector> GetFlowData(string gameId, TransactionType type, DateTime startDate, DateTime endDate)
        {
            //GameMonitoringConfig game = 
            List<CoinFlowVector> ReturnList = new List<CoinFlowVector>();
            string query = String.Format(@"SELECT DATE(RecordTimestamp) as RecordTimestamp, Type, sum(TotalBought) as ItemCount, sum(TotalCredits) as CreditAmount FROM Economy_PurchaseBreakdownRaw WHERE GameId = '{3}' AND DATE(RecordTimestamp) BETWEEN '{0}' and '{1}' AND Type = {2} group by Type, DATE(RecordTimestamp) order by DATE(RecordTimestamp);", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), (int)type, gameId);

            DataTable SourceTable = DBManager.Instance.Query(Datastore.Monitoring, query);

            if (SourceTable.Rows.Count > 0) {
                
                //category and group by

                CoinFlowVector flowSource = new CoinFlowVector();
                flowSource.coinData = new List<double>();
                flowSource.itemData = new List<double>();
                flowSource.StartDate = startDate.Date.ToUnixTimestamp() * 1000;
                flowSource.interval = 1 * 24 * 60 * 60 * 1000; //1 day in ms
                flowSource.Category = "Everything";
                flowSource.Type = type.ToString();

                DateTime lastDay = startDate.Date;
                foreach (DataRow source in SourceTable.Rows)
                {
                    DateTime currentDay = DateTime.Parse(source["RecordTimestamp"].ToString());

                    int daysBetween = (currentDay - lastDay).Days;
                    if (daysBetween > 1)
                    {
                        while(--daysBetween != 0)
                        {
                            flowSource.coinData.Add(0);
                            flowSource.itemData.Add(0);
                        }
                        
                    }
                    flowSource.coinData.Add(Double.Parse(source["CreditAmount"].ToString()));
                    flowSource.itemData.Add(Double.Parse(source["ItemCount"].ToString()));

                    lastDay = currentDay;

                }
                ReturnList.Add(flowSource);
            }
            return ReturnList;
        }

        public FlowDataView GetCoinFlow(string game, AWSRegion region, DateTime start, DateTime end)
        {
            GameMonitoringConfig GameToGet = Games.Instance.GetMonitoredGames().Where(x => x.ShortTitle == game).FirstOrDefault();
            FlowDataView chart = new FlowDataView() {
                Inflows = GetFlowData(GameToGet.Id, TransactionType.AddCredits, start, end),
                Outflows = GetFlowData(GameToGet.Id, TransactionType.Purchase, start, end),
                StartDate = start.ToUnixTimestamp() * 1000
            };

            return chart;
        }
        public FlowDataView GetCoinFlowByCat(GameMonitoringConfig game, AWSRegion region, DateTime start, DateTime end)
        {
            FlowDataView chart = new FlowDataView()
            {
                Inflows = GetFlowDataByCategory(game.Id, TransactionType.AddCredits, start, end),
                Outflows = GetFlowDataByCategory(game.Id, TransactionType.Purchase, start, end),
                StartDate = start.ToUnixTimestamp() * 1000
            };

            return chart;
        }

        public List<CoinFlowVector> GetFlowDataByCategory(string gameId, TransactionType type, DateTime startDate, DateTime endDate)
        {
            //GameMonitoringConfig game = 
            List<CoinFlowVector> ReturnList = new List<CoinFlowVector>();
            string query = String.Format(
                @"SELECT DATE(RecordTimestamp) as RecordTimestamp, 
            Type, 
            sum(TotalBought) as ItemCount, 
            sum(TotalCredits) as CreditAmount,
            Category        
            FROM Economy_PurchaseBreakdownRaw 
            WHERE GameId = '{3}' 
            AND DATE(RecordTimestamp) BETWEEN '{0}' and '{1}' 
            AND Type = {2} 
            group by Category, Type, 
            DATE(RecordTimestamp) 
            order by DATE(RecordTimestamp);", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), (int)type, gameId);

            DataTable SourceTable = DBManager.Instance.Query(Datastore.Monitoring, query);

            string EMPTYCAT = "No Category";
            if (SourceTable.Rows.Count > 0)
            {
                CoinFlowVector flowSource = new CoinFlowVector();
                //category and group by
                List<string> categories = SourceTable.AsEnumerable().Select(s =>
                {
                    string catname = EMPTYCAT;
                    if (!String.IsNullOrEmpty(s.Field<string>("Category")))
                    {
                        catname = s.Field<string>("Category");
                    }
                    return catname;
                }).Distinct().ToList();


                foreach (string cat in categories)
                {
                    DateTime firstRecordTimestamp = SourceTable.AsEnumerable()
                        .FirstOrDefault(y =>
                        {
                            string catname = y.Field<string>("Category");
                            if (string.IsNullOrEmpty(catname))
                            {
                                catname = EMPTYCAT;
                            }
                            return cat == catname;
                        })
                        .Field<DateTime>("RecordTimestamp");


                    ReturnList.Add(new CoinFlowVector()
                    {
                        Category = cat,
                        coinData = new List<double>(),
                        itemData = new List<double>(),
                        StartDate = firstRecordTimestamp.Date.ToUnixTimestamp() * 1000,
                        interval = 1 * 24 * 60 * 60 * 1000,
                        Type = type.ToString()
                    });
                }


                DateTime lastDay = startDate.Date;
                foreach (DataRow source in SourceTable.Rows)
                {

                    DateTime currentDay = DateTime.Parse(source["RecordTimestamp"].ToString());

                    int daysBetween = (currentDay - lastDay).Days;
                    if (daysBetween > 1)
                    {
                        while (--daysBetween != 0)
                        {
                            foreach (CoinFlowVector series in ReturnList)
                            {
                                series.coinData.Add(0);
                                series.itemData.Add(0);
                            }
                        }

                    }
                    lastDay = currentDay;

                    string cat = EMPTYCAT;

                    if (!string.IsNullOrEmpty(source["Category"].ToString()))
                    {
                        cat = source["Category"].ToString();
                    }

                    ReturnList.FirstOrDefault(x => x.Category == cat).coinData.Add(Double.Parse(source["CreditAmount"].ToString()));
                    ReturnList.FirstOrDefault(x => x.Category == cat).itemData.Add(Double.Parse(source["ItemCount"].ToString()));

                }
            }
            return ReturnList;
        }

        public PVPPieChart GetBuyWhaleReport(GameMonitoringConfig game, AWSRegion region, DateTime start, DateTime end, int cohort)
        {
            string gameId = game.Id;

            string thresholdQuery = String.Format(
@"
# Get threshold for top x% spenders in dollars
SET @startDate = '{0}';
SET @endDate = '{1}';
SET @rownum = 0, @prev_val = NULL, @top_percent= {2};

# Calculate threshold for top x% of spenders
SELECT IFNULL(min(score),0.0) as threshold, IFNULL(max(row),0) as numUsers FROM
	(
	SELECT @rownum := @rownum + 1 AS row,
		@prev_val := score AS score,
		UserId
		FROM
		(
			select sum(CostAmount) as score, UserId from Moniverse.Economy_GameCreditTransactions
				where GameId = '{3}'     
                and TransactionType = 0   # addCredits
                and CostAmount > 0          # money was actually spent
				and Status = 1 			    # finalized transaction
				and CreatedOn between @startDate and @endDate
				group by UserId
				order by score desc
		) as spending
	ORDER BY score DESC
	) as rankedSpending
WHERE row <= ceil((@top_percent/100 * @rownum)); # ceil helps return at least one row for small datasets

", start.ToString("yyyy-MM-dd HH:mm:ss"), end.ToString("yyyy-MM-dd HH:mm:ss"), cohort, gameId);

            DataTable thresholdDatable = DBManager.Instance.Query(Datastore.Monitoring, thresholdQuery);
            decimal threshold = thresholdDatable.Rows[0].Field<decimal>("threshold");
            long numUsers = thresholdDatable.Rows[0].Field<long>("numUsers");

            string query = String.Format(
@"
# Get people that spent more than threshold for a given date range
SET @startDate = '{0}';
SET @endDate = '{1}';

# Grab users that have spent >= threshold
DROP TEMPORARY TABLE IF EXISTS topSpenders;
CREATE TEMPORARY TABLE IF NOT EXISTS topSpenders (PRIMARY KEY (UserId))
(
	select sum(CostAmount) as score, UserId from Moniverse.Economy_GameCreditTransactions
		where GameId = '{2}' 
        and TransactionType = 0     # addCredits
        and CostAmount > 0          # money was actually spent
		and Status = 1 			    # finalized transaction
	    and CreatedOn between @startDate and @endDate        
	    group by UserId
        having score >= {3}
	    order by score desc
);

# Get items bought by the top spenders
SELECT UserData as category, Sum(CostAmount) as total, count(*) as count
FROM Economy_GameCreditTransactions AS egct
INNER JOIN topSpenders AS topSpenders
ON egct.UserId = topSpenders.UserId
where GameId = '{2}' 
    and TransactionType = 0     # addCredits
    and CostAmount > 0          # money was actually spent
    and Status = 1 			    # finalized transaction
    and CreatedOn between @startDate and @endDate  
GROUP BY UserData
order by total desc;
", start.ToString("yyyy-MM-dd HH:mm:ss"), end.ToString("yyyy-MM-dd HH:mm:ss"), gameId, threshold);

            DataTable result = DBManager.Instance.Query(Datastore.Monitoring, query);

            var categoryData = new List<PVPieChartCategories>();
            foreach (DataRow row in result.Rows)
            {
                var category = new PVPieChartCategories()
                {
                    name = row.Field<String>("category"),
                    value = row.Field<Decimal>("total")
                };
                category.metadata.Add("count", row["count"].ToString());
                categoryData.Add(category);
            }

            var pieChart = new PVPPieChart()
            {
                title = cohort + "%",
                subtitle = (categoryData.Any() ? string.Format("Users: ~{0}<br/>Threshold: >= ${1}", numUsers, threshold) : "No data"),
                categoryData = categoryData
            };

            return pieChart;
        }

        public PVPPieChart GetSpendWhaleReport(GameMonitoringConfig game, AWSRegion region, DateTime start, DateTime end, int cohort)
        {
            string gameId = game.Id;

            string thresholdQuery = String.Format(
@"
# Get threshold for top x% spenders of gems
SET @startDate = '{0}';
SET @endDate = '{1}';
SET @rownum = 0, @prev_val = NULL, @top_percent= {2};

# Calculate threshold for top x% of spenders
SELECT IFNULL(min(score),0) as threshold, IFNULL(max(row),0) as numUsers FROM
	(
	SELECT @rownum := @rownum + 1 AS row,
		@prev_val := score AS score,
		UserId
		FROM
		(
			select sum(Credits) as score, UserId from Moniverse.Economy_GameCreditTransactions
				WHERE GameId = '{3}' 
                AND TransactionType = 1   # removeCredits
				AND Status = 1 			    # finalized transaction
				AND CreatedOn between @startDate and @endDate
				group by UserId
				order by score desc
		) as spending
	ORDER BY score DESC
	) as rankedSpending
WHERE row <= ceil((@top_percent/100 * @rownum)); # ceil helps return at least one row for small datasets
", start.ToString("yyyy-MM-dd HH:mm:ss"), end.ToString("yyyy-MM-dd HH:mm:ss"), cohort, gameId);

            DataTable thresholdDatable = DBManager.Instance.Query(Datastore.Monitoring, thresholdQuery);
            decimal threshold = thresholdDatable.Rows[0].Field<decimal>("threshold");
            long numUsers = thresholdDatable.Rows[0].Field<long>("numUsers");

            string query = String.Format(
@"
# Get people that spent more than threshold for a given date range
SET @startDate = '{0}';
SET @endDate = '{1}';

# Grab users that have spent >= threshold
DROP TEMPORARY TABLE IF EXISTS topSpenders;
CREATE TEMPORARY TABLE IF NOT EXISTS topSpenders (PRIMARY KEY (UserId))
(
	select sum(Credits) as score, UserId from Moniverse.Economy_GameCreditTransactions
		WHERE GameId = '{2}' 
        AND TransactionType = 1     # removeCredits
		and Status = 1 			    # finalized transaction
	    and CreatedOn between @startDate and @endDate        
	    group by UserId
        having score >= {3}
	    order by score desc
);

# Get gems spent by the top spenders
SELECT UserData as category, sum(Credits) as total, count(*) as count
FROM Moniverse.Economy_GameCreditTransactions AS egct
INNER JOIN topSpenders AS topSpenders
ON egct.UserId = topSpenders.UserId
    WHERE GameId = '{2}'
    AND egct.TransactionType = 1        # removeCredits
    AND egct.Status = 1 			    # finalized transaction
    AND CreatedOn between @startDate and @endDate 
GROUP BY UserData
order by total desc;
", start.ToString("yyyy-MM-dd HH:mm:ss"), end.ToString("yyyy-MM-dd HH:mm:ss"), gameId, threshold);

            DataTable result = DBManager.Instance.Query(Datastore.Monitoring, query);

            var categoryData = new List<PVPieChartCategories>();
            foreach (DataRow row in result.Rows)
            {
                var category = new PVPieChartCategories()
                {
                    name = row.Field<String>("category"),
                    value = row.Field<Decimal>("total")
                };
                category.metadata.Add("count", row["count"].ToString());
                categoryData.Add(category);
            }

            var pieChart = new PVPPieChart()
            {
                title = cohort + "%",
                subtitle = (categoryData.Any() ? string.Format("Users: ~{0}<br/>Threshold: >= {1} credits", numUsers, threshold ) : "No data"),
                categoryData = categoryData
            };

            return pieChart;
        }

    }
}