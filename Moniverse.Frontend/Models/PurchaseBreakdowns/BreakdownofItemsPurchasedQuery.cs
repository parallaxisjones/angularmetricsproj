using System;
using System.Collections.Generic;
using Moniverse.Contract;
using Playverse.Data;
using PlayverseMetrics.Infrastructure;

namespace PlayverseMetrics.Models.PurchaseBreakdowns
{
    public class ItemBreakdownRow
    {
        public string UserData { get; set; }
        public decimal Cost { get; set; }
        public int TotalBought { get; set; }
        public int TotalCredits { get; set; }
    }

    public class BreakdownofItemsPurchasedQuery : IQueryExecutor<IEnumerable<ItemBreakdownRow>>
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Platform { get; set; }

        public IEnumerable<ItemBreakdownRow> Execute()
        {
            string sql =
@"
-- BREAKDOWN OF ITEMS 
SELECT UserData, MAX(Credits) AS 'Cost', COUNT(*) AS 'TotalBought', SUM(Credits) AS 'TotalCredits'
FROM Economy_GameCreditTransactions
WHERE TransactionType = 1
";
            if (!string.IsNullOrEmpty(Platform))
            {
                sql +=
@"
    AND ExternalOnlineService = @platform
";
            }

            sql +=
@"
AND CreatedOn BETWEEN @start AND @end
GROUP BY UserData
ORDER BY SUM(Credits) DESC;
";
            dynamic filter = new { start = Start.ToStartOfDay(), end = End.ToEndOfDay(), platform = Platform };

            IEnumerable<ItemBreakdownRow> rows = DBManager.Instance.Query<ItemBreakdownRow>(Datastore.Monitoring, sql, filter);
            
            return rows;
        }
    }
}