using System;
using System.Web.Mvc;
using Moniverse.Contract;
using PlayverseMetrics.Models.PurchaseBreakdowns;

namespace PlayverseMetrics.Controllers.Reports
{
    public class PurchaseBreakdownController : BaseController
    {
        public JsonResult CreditSummaryReport(string game, DateTime start, DateTime end, string platform)
        {
            GameMonitoringConfig gameMonitoringConfig = Games.Instance.GetMoniteredGame(game);

            CreditSummaryReportQuery creditSummaryReportQuery = new CreditSummaryReportQuery()
            {
                GameMonitoringConfig = gameMonitoringConfig,
                Start = start,
                End = end,
                Platform = platform
            };

            CreditSummaryReport creditSummaryReport = creditSummaryReportQuery.Execute();

            return JsonResult( creditSummaryReport );
        }

        public JsonResult BreakdownOfCreditPackagesRedeemed(DateTime start, DateTime end, string platform)
        {
            BreakdownOfCreditPackagesRedeemedQuery breakdownOfCreditPackagesRedeemed = new BreakdownOfCreditPackagesRedeemedQuery()
            {
                Start = start,
                End = end,
                Platform = platform
            };
            var results = breakdownOfCreditPackagesRedeemed.Execute();

            return JsonResult( results );
        }

        public JsonResult BreakdownofItemsPurchased(DateTime start, DateTime end, string platform)
        {
            BreakdownofItemsPurchasedQuery breakdownofItemsPurchased = new BreakdownofItemsPurchasedQuery();
            var results = breakdownofItemsPurchased.Execute();

            return JsonResult( results );
        }

        public JsonResult BreakdownofItemsPurchasedByDate(DateTime start, DateTime end, string platform)
        {
            BreakdownOfItemsPurchasedByDateQuery breakdownofItemsPurchased = new BreakdownOfItemsPurchasedByDateQuery();
            var results = breakdownofItemsPurchased.Execute();

            return JsonResult(results);
        }

        public JsonResult BreakdownofTotalUniqueUsersRedeemedvsSpentCreditsByDate(DateTime start, DateTime end, string platform)
        {
            BreakdownofTotalUniqueUsersRedeemedvsSpentCreditsByDateQuery breakdownQuery = new BreakdownofTotalUniqueUsersRedeemedvsSpentCreditsByDateQuery();
            var results = breakdownQuery.Execute();

            return JsonResult( results );
        }

        public JsonResult CreditSalesSteamCrossReference(DateTime start, DateTime end, string platform)
        {
            CreditSalesSteamCrossReferenceQuery creditSalesSteamCrossReference = new CreditSalesSteamCrossReferenceQuery()
            {
                Start = start,
                End = end,
                Platform = platform
            };
            var results = creditSalesSteamCrossReference.Execute();

            return JsonResult( results );
        }
    }
}