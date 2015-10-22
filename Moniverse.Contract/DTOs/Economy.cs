using System;

namespace Moniverse.Contract
{
    public class PlayversePurchaseAggregation
    {
        public int Id { get; set; }
        public DateTime RecordTimestamp { get; set; }
        public string GameId { get; set; }
        public int TransactionType { get; set; }
        public string UserData { get; set; }
        public string Category { get; set; }
        public int Cost { get; set; }
        public int TotalItems { get; set; }
        public int TotalCredits { get; set; }
    }

    public class PlayverseGameCreditTransaction
    {
        public DateTime RecordTimestamp { get; set; }
        public string TransactionId { get; set; }
        public string UserId { get; set; }
        public string GameId { get; set; }
        public string ExternalOnlineService { get; set; }
        public string ThirdPartyOrderId { get; set; }
        public int Credits { get; set; }
        public string PaymentProvider { get; set; }
        public string PaymentTransactionId { get; set; }
        public string TransactionType { get; set; }
        public string CreditPackId { get; set; }
        public string UserData { get; set; }
        public string Description { get; set; }
        public decimal CostAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string Category { get; set; }
        public string ClientKey { get; set; }
    }
}
