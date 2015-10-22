using System;
using System.Collections.Generic;
using Moniverse.Contract;
using Playverse.Data;
using PlayverseMetrics.Infrastructure;

namespace PlayverseMetrics.Models.PurchaseBreakdowns
{
    public class BreakdownOfItemsPurchasedByDateRow
    {
        public DateTime Date { get; set; }
        public string UserData { get; set; }
        public int Cost { get; set; }
        public int TotalBought { get; set; }
        public int TotalCredits { get; set; }
    }

    public class BreakdownOfItemsPurchasedByDateQuery : IQueryExecutor<IEnumerable<BreakdownOfItemsPurchasedByDateRow>>
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Platform { get; set; }

        public IEnumerable<BreakdownOfItemsPurchasedByDateRow> Execute()
        {
            string sql =
@"
SELECT DATE(CreatedOn) AS 'Date', UserData, MAX(Credits) AS 'Cost', COUNT(*) AS 'TotalBought', SUM(Credits) AS 'TotalCredits'
FROM Economy_GameCreditTransactions
WHERE TransactionType = 1";
            if (!string.IsNullOrEmpty(Platform))
            {
                sql +=
@"
    AND ExternalOnlineService = @platform
";
            }

            sql +=
@"	
#AND ExternalOnlineService = 'Steam'
AND CreatedOn BETWEEN @start AND @end
AND UserId NOT IN (SELECT UserId FROM ACL_UserRoleAssignment)
GROUP BY DATE(CreatedOn), UserData
ORDER BY DATE(CreatedOn) DESC, SUM(Credits) DESC;
";
            dynamic filter = new { start = Start.ToStartOfDay(), end = End.ToEndOfDay(), platform = Platform };

            var results = DBManager.Instance.Query<BreakdownOfItemsPurchasedByDateRow>(Datastore.Monitoring, sql, filter);

            return results;
        }
    }
}