using System;
using System.Collections.Generic;
using Moniverse.Contract;
using Playverse.Data;
using PlayverseMetrics.Infrastructure;

namespace PlayverseMetrics.Models.PurchaseBreakdowns
{
    public class BreakdownRedeemedvsSpentCreditsRow
    {
        public DateTime Date { get; set; }
        public decimal UsersRedeemedCredits { get; set; }
        public decimal CumulativeUsersRedeemedCredits { get; set; }
        public decimal UsersSpentCredits { get; set; }
        public decimal UsersSpentCreditsPercentage { get; set; }
    }

    public class BreakdownofTotalUniqueUsersRedeemedvsSpentCreditsByDateQuery : IQueryExecutor<IEnumerable<BreakdownRedeemedvsSpentCreditsRow>>
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Platform { get; set; }

        public IEnumerable<BreakdownRedeemedvsSpentCreditsRow> Execute()
        {
            string sql =
@"
-- UNIQUE USERS REDDEMED VS SPENT BY DAY
SELECT DATE(SPENT.Date) AS 'Users - Redeemed Date', REDEEMED.Total AS 'Users - Redeemed Credits', REDEEMED.CTotal AS 'Cumulative Users - Redeemed Credits', SPENT.Total AS 'Users - Spent Credits', ROUND((SPENT.Total / REDEEMED.CTotal) * 100, 2) AS 'Users - Spent Credits %'
FROM (
	SELECT DATE(CreatedOn) AS 'Date', COUNT(DISTINCT UserId) AS 'Total', (@total := @total + COUNT(DISTINCT UserId)) AS CTotal
	FROM Economy_GameCreditTransactions, (SELECT @total:=0) T
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
    #AND UserId NOT IN (SELECT UserId FROM ACL_UserRoleAssignment)
    GROUP BY DATE(CreatedOn)
) REDEEMED
RIGHT JOIN
(
	SELECT DATE(CreatedOn) AS 'Date', COUNT(DISTINCT UserId) AS 'Total'
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
    #AND UserId NOT IN (SELECT UserId FROM ACL_UserRoleAssignment)
    GROUP BY DATE(CreatedOn)
) SPENT
ON REDEEMED.Date = SPENT.Date
,(SELECT @total:=0) AS T
GROUP BY SPENT.Date
ORDER BY SPENT.Date DESC;
";
            dynamic filter = new { start = Start.ToStartOfDay(), end = End.ToEndOfDay(), platform = Platform };

            return DBManager.Instance.Query<BreakdownRedeemedvsSpentCreditsRow>(Datastore.Monitoring, sql, filter);
        }
    }
}