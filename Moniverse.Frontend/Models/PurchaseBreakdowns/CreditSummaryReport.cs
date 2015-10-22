using System;
using System.Collections.Generic;
using System.Data;
using Moniverse.Contract;
using Playverse.Data;
using PlayverseMetrics.Infrastructure;

namespace PlayverseMetrics.Models.PurchaseBreakdowns
{
    public class CreditSummaryReport
    {
        public long UsersRedeemedCredits { get; set; }
        public long UsersSpentCredits { get; set; }
        public decimal UsersSpentCreditsPercentage { get; set; }

        public decimal UsersAvgPercentofCreditsSpent { get; set; }
        public decimal UsersAvgCreditsRemaining { get; set; }

        public decimal TotalCreditsRedeemed { get; set; }
        public decimal TotalCreditsSpent { get; set; }
        public decimal TotalCreditsRemaining { get; set; }
        public decimal TotalCreditsSpentPercentage { get; set; }
    }

    public class CreditSummaryReportQuery : IQueryExecutor<CreditSummaryReport>
    {
        public GameMonitoringConfig GameMonitoringConfig { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Platform { get; set; }

        public CreditSummaryReport Execute()
        {
            Dictionary<string,string> filter = new Dictionary<string, string>()
            {
                {"gameId", GameMonitoringConfig.Id},
                {"start", Start.ToStartOfDay()},
                {"end", End.ToEndOfDay()},
                {"platform", Platform}
            };

            CreditSummaryReport creditSummaryReport = new CreditSummaryReport();

            string uniqueUsersSql =
                @"
-- UNIQUE USERS REDEEMED VS SPENT
SELECT REDEEMED.Total AS 'UsersRedeemedCredits', SPENT.Total AS 'UsersSpentCredits', IFNULL(ROUND((SPENT.Total / REDEEMED.Total) * 100, 2), 0) AS 'UsersSpentCreditsPercentage'
FROM (
	SELECT COUNT(DISTINCT UserId) AS 'Total'
	FROM Economy_GameCreditTransactions
	WHERE GameId = @gameId
    AND TransactionType = 0";
            if (!string.IsNullOrEmpty(Platform))
            {
                uniqueUsersSql +=
@"
    AND ExternalOnlineService = @platform
";
            }

            uniqueUsersSql +=
@"	
    AND (ThirdPartyOrderId = '' || (ThirdPartyOrderId != '' && Status = 1))
    AND CreatedOn BETWEEN @start AND @end
    # AND CreatedOn BETWEEN '2015-07-28 00:00:00' AND '2015-09-20 23:59:59'
    # AND UserId NOT IN (SELECT UserId FROM ACL_UserRoleAssignment)
) REDEEMED,
(
	SELECT COUNT(DISTINCT UserId) AS 'Total'
	FROM Economy_GameCreditTransactions
	WHERE GameId = @gameId
    AND TransactionType = 1";

            if (!string.IsNullOrEmpty(Platform))
            {
                uniqueUsersSql +=
@"
    AND ExternalOnlineService = @platform
";
            }

            uniqueUsersSql +=
@"
    AND CreatedOn BETWEEN @start AND @end
    # AND UserId NOT IN (SELECT UserId FROM ACL_UserRoleAssignment)
) SPENT;
";
            DataTable dataTable = DBManager.Instance.Query(Datastore.Monitoring, uniqueUsersSql, filter);

            creditSummaryReport.UsersRedeemedCredits = (long)dataTable.Rows[0]["UsersRedeemedCredits"];
            creditSummaryReport.UsersSpentCredits = (long)dataTable.Rows[0]["UsersSpentCredits"];
            creditSummaryReport.UsersSpentCreditsPercentage = (decimal)dataTable.Rows[0]["UsersSpentCreditsPercentage"];


            string avgSpentSql =
                @"
-- AVG USERS SPENT CREDITS AND REMAINING
SELECT IFNULL(AVG(USER.Spent_Pct), 0) as 'UsersAvgPercentofCreditsSpent', IFNULL(AVG(USER.Remaining), 0) as 'UsersAvgCreditsRemaining'
FROM (
	SELECT REDEEMED.UserId 'UserId', REDEEMED.Credits 'Redeemed', IFNULL(SPENT.Credits, 0) 'Spent', (REDEEMED.Credits - IFNULL(SPENT.Credits, 0)) 'Remaining', (ROUND(IFNULL(SPENT.Credits, 0) / REDEEMED.Credits, 2) * 100) 'Spent_Pct'
	FROM (
		SELECT UserId, SUM(Credits) AS 'Credits'
		FROM Economy_GameCreditTransactions
		WHERE TransactionType = 0";
            if (!string.IsNullOrEmpty(Platform))
            {
                avgSpentSql +=
@"
    AND ExternalOnlineService = @platform
";
            }

            avgSpentSql +=
@"
		AND (ThirdPartyOrderId = '' || (ThirdPartyOrderId != '' && Status = 1))
		AND CreatedOn BETWEEN @start AND @end
        # AND UserId NOT IN (SELECT UserId FROM ACL_UserRoleAssignment)
		GROUP BY UserId
	) REDEEMED
	INNER JOIN 
	(
		SELECT UserId, SUM(Credits) AS 'Credits'
		FROM Economy_GameCreditTransactions
		WHERE TransactionType = 1";
            if (!string.IsNullOrEmpty(Platform))
            {
                avgSpentSql +=
@"
    AND ExternalOnlineService = @platform
";
            }

            avgSpentSql +=
@"

		AND CreatedOn BETWEEN @start AND @end
        # AND UserId NOT IN (SELECT UserId FROM ACL_UserRoleAssignment)
		GROUP BY UserId
	) SPENT
	ON REDEEMED.UserId = SPENT.UserId
) USER;
";
            dataTable = DBManager.Instance.Query(Datastore.Monitoring, avgSpentSql, filter);

            creditSummaryReport.UsersAvgPercentofCreditsSpent = (decimal)dataTable.Rows[0]["UsersAvgPercentofCreditsSpent"];
            creditSummaryReport.UsersAvgCreditsRemaining = (decimal)dataTable.Rows[0]["UsersAvgCreditsRemaining"];

            string totalUserCreditsSql =
@"
            -- TOTAL USER CREDITS BREAKDOWN
            SELECT IFNULL(CREDIT.Total, 0) AS 'TotalCreditsRedeemed', IFNULL(DEBIT.Total, 0) AS 'TotalCreditsSpent', IFNULL((CREDIT.Total - DEBIT.Total), 0) AS 'TotalCreditsRemaining', IFNULL(ROUND((DEBIT.Total / CREDIT.Total) * 100, 2), 0) AS 'TotalCreditsSpentPercentage'
            FROM (
            	SELECT SUM(Credits) AS 'Total'
            	FROM Economy_GameCreditTransactions
            	WHERE TransactionType = 0";
            if (!string.IsNullOrEmpty(Platform))
            {
                totalUserCreditsSql +=
@"
    AND ExternalOnlineService = @platform
";
            }

            totalUserCreditsSql +=
@"
            	AND (ThirdPartyOrderId = '' || (ThirdPartyOrderId != '' && Status = 1))
            	AND CreatedOn BETWEEN @start AND @end
                # AND UserId NOT IN (SELECT UserId FROM ACL_UserRoleAssignment)
            ) CREDIT,
            (
            	SELECT SUM(Credits) AS 'Total'
            	FROM Economy_GameCreditTransactions
            	WHERE TransactionType = 1";
            if (!string.IsNullOrEmpty(Platform))
            {
                totalUserCreditsSql +=
@"
    AND ExternalOnlineService = @platform
";
            }

            totalUserCreditsSql +=
@"
            	AND CreatedOn BETWEEN @start AND @end
                # AND UserId NOT IN (SELECT UserId FROM ACL_UserRoleAssignment)
            ) DEBIT;
";

            dataTable = DBManager.Instance.Query(Datastore.Monitoring, totalUserCreditsSql, filter);

            creditSummaryReport.TotalCreditsRedeemed = (decimal)dataTable.Rows[0]["TotalCreditsRedeemed"];
            creditSummaryReport.TotalCreditsSpent = (decimal)dataTable.Rows[0]["TotalCreditsSpent"];
            creditSummaryReport.TotalCreditsRemaining = (decimal)dataTable.Rows[0]["TotalCreditsRemaining"];
            creditSummaryReport.TotalCreditsSpentPercentage = (decimal)dataTable.Rows[0]["TotalCreditsSpentPercentage"];

            return creditSummaryReport;
        }
    }
}