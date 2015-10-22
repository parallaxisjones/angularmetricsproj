using System;
using System.Collections.Generic;
using Moniverse.Contract;
using Playverse.Data;
using PlayverseMetrics.Infrastructure;

namespace PlayverseMetrics.Models.PurchaseBreakdowns
{
    public class CreditSalesSteamCrossReferenceRow
    {
        public DateTime Date { get; set; }
        public string Platform { get; set; }
        public int Credits { get; set; }
        public string UserData { get; set; }
        public string Description { get; set; }
        public decimal CostAmount { get; set; }
        public int TotalBought { get; set; }
    }

    public class CreditSalesSteamCrossReferenceQuery : IQueryExecutor<IEnumerable<CreditSalesSteamCrossReferenceRow>>
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Platform { get; set; }

        public IEnumerable<CreditSalesSteamCrossReferenceRow> Execute()
        {
            string sql =
@"
            -- PURCHASED CREDITS
            -- TIMES IN PST BECAUSE OF STEAM
            SELECT DATE(CONVERT_TZ(CreatedOn, 'UTC','US/Pacific')) AS 'Date', ExternalOnlineService AS 'Platform', Credits, UserData, Description, CostAmount, COUNT(*) AS 'TotalBought'
            FROM Economy_GameCreditTransactions
            WHERE ThirdPartyOrderId <> ''
            AND Status = 1";
            if (!string.IsNullOrEmpty(Platform))
            {
                sql +=
@"
    AND ExternalOnlineService = @platform
";
            }
            // CONVERT_TZ(CreatedOn, 'UTC','US/Pacific') will only work if mysql timezone dbs are installed
            sql +=
@"
            # AND ExternalOnlineService = 'Steam'
            AND CONVERT_TZ(CreatedOn, 'UTC','US/Pacific') BETWEEN @start AND @end
            GROUP BY DATE(CONVERT_TZ(CreatedOn, 'UTC','US/Pacific')), ExternalOnlineService, Credits, UserData, Description, CostAmount
            ORDER BY DATE(CONVERT_TZ(CreatedOn, 'UTC','US/Pacific')) DESC, CostAmount DESC;
";

            dynamic filter = new { start = Start.ToStartOfDay(), end = End.ToEndOfDay(), platform = Platform };

            return DBManager.Instance.Query<CreditSalesSteamCrossReferenceRow>(Datastore.Monitoring, sql, filter);
        }
    }
}