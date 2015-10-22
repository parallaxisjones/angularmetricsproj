using System;
using System.Collections.Generic;
using Moniverse.Contract;
using Playverse.Data;
using PlayverseMetrics.Infrastructure;

namespace PlayverseMetrics.Models.PurchaseBreakdowns
{
    public class BreakdownOfCreditPackagesRedeemedRow
    {
        public int Credits { get; set; }
        public int TotalRedemptions { get; set; }
    }

    public class BreakdownOfCreditPackagesRedeemedQuery : IQueryExecutor<IEnumerable<BreakdownOfCreditPackagesRedeemedRow>>
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Platform { get; set; }

        public IEnumerable<BreakdownOfCreditPackagesRedeemedRow> Execute()
        {
            string sql = @"
-- BREAKDOWN OF CREDITS REDEEMED
SELECT Credits, COUNT(*) 'TotalRedemptions'
FROM Economy_GameCreditTransactions
WHERE TransactionType = 0";
            if (!string.IsNullOrEmpty(Platform))
            {
                sql +=
@"
    AND ExternalOnlineService = @platform
";
            }

            sql +=
@"	
# AND ExternalOnlineService = 'Steam'
AND (ThirdPartyOrderId = '' || (ThirdPartyOrderId != '' && Status = 1))
AND CreatedOn BETWEEN @start AND @end
# AND CreatedOn BETWEEN '2015-07-28 00:00:00' AND '2015-09-20 23:59:59'
# AND UserId NOT IN (SELECT UserId FROM ACL_UserRoleAssignment)
GROUP BY Credits
ORDER BY Credits DESC;
";

            dynamic filter = new { start = Start.ToStartOfDay(), end = End.ToEndOfDay(), platform = Platform };

            IEnumerable<BreakdownOfCreditPackagesRedeemedRow> rows = DBManager.Instance.Query<BreakdownOfCreditPackagesRedeemedRow>(Datastore.Monitoring, sql, filter);

            return rows;
        }
    }
}