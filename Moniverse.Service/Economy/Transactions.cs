using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playverse.Data;
using Utilities;
using System.Diagnostics;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;
//using PlayverseMonitoring;
using Moniverse.Contract;
using Moniverse.Service;
namespace Moniverse.Service
{
    public class Transactions : ServiceClassBase
    {
        public static Transactions Instance = new Transactions(); 
        public virtual string TRANSACTION_TABLE {get {return "GameCredit_GameCreditTransactions";}}
        public virtual string ECONOMY_TABLE { get { return "Economy_PurchaseBreakdownRaw"; } }
        public virtual string ECONOMY_GAMECREDITTRANSACTIONS_TABLE { get { return "Economy_GameCreditTransactions"; }}
        public Transactions() : base(){}

        private List<PlayverseGameCreditTransaction> GetGameCreditTransactions(string gameId)
        {
            DateTime lastUpdated;
            string latestQ = String.Format(@"SELECT MAX(RecordTimestamp) as LatestRecord FROM {0}", ECONOMY_GAMECREDITTRANSACTIONS_TABLE);

            DataTable response = DBManager.Instance.Query(Datastore.Monitoring, latestQ);

            string latest;
            if (response.Rows.Count == 0)
            {
                latest = "2015-04-28 15:00";
            }
            else
            {
                latest = response.Rows[0]["LatestRecord"].ToString();
            }
            DateTime.TryParse(latest, out lastUpdated);

            List<PlayverseGameCreditTransaction> creditTransactionsTable = new List<PlayverseGameCreditTransaction>();
            string query = String.Format(
                @"SELECT TransactionId,
                    UserId,
                    GameId,
                    ExternalOnlineService,
                    ThirdPartyOrderId,
                    Credits,
                    PaymentProvider,
                    PaymentTransactionId,
                    TransactionType,
                    CreditPackId,
                    UserData,
                    Description,
                    CostAmount,
                    Status,
                    CreatedOn,
                    UpdatedOn,
                    Category,
                    ClientKey
                FROM {0}
                Where CreatedOn BETWEEN '{1}' AND '{2}'
                AND Status=1
                AND CreatedOn > '2015-04-28 15:00'
                AND UserId NOT IN (SELECT UserId FROM ACL_UserRoleAssignment)
                ORDER BY DATE(CreatedOn) DESC;", TRANSACTION_TABLE, lastUpdated.AddSeconds(1).ToString("yyyy-MM-dd HH:mm:ss"), DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

            DataTable data = DBManager.Instance.Query(Datastore.General, query);

            if (data.Rows.Count > 0)
            {
                foreach (DataRow row in data.Rows)
                {
                    PlayverseGameCreditTransaction item = new PlayverseGameCreditTransaction()
                    {
                        RecordTimestamp = DateTime.UtcNow,
                        TransactionId = row["TransactionId"].ToString(),
                        UserId = row["UserId"].ToString(),
                        GameId = row["GameId"].ToString(),
                        ExternalOnlineService = row["ExternalOnlineService"].ToString(),
                        ThirdPartyOrderId = row["ThirdPartyOrderId"].ToString(),
                        Credits = Int32.Parse(row["Credits"].ToString()),
                        PaymentProvider = row["PaymentProvider"].ToString(),
                        PaymentTransactionId = row["PaymentTransactionId"].ToString(),
                        TransactionType = row["TransactionType"].ToString(),
                        CreditPackId  = row["CreditPackId"].ToString(),
                        UserData = row["UserData"].ToString(),
                        Description = row["Description"].ToString(),
                        CostAmount = Decimal.Parse(row["CostAmount"].ToString()),
                        Status = row["Status"].ToString(),
                        CreatedOn = DateTime.Parse(row["CreatedOn"].ToString()),
                        UpdatedOn = DateTime.Parse(row["UpdatedOn"].ToString()),
                        Category = row["Category"].ToString(),
                        ClientKey = row["ClientKey"].ToString()
                    };
                    creditTransactionsTable.Add(item);
                    
                }
            }
            return creditTransactionsTable;

        }

        private List<PlayversePurchaseAggregation> GetPurchaseTransactions(string gameId, DateTime startDate, DateTime endDate) {

            List<PlayversePurchaseAggregation> PurchaseTable = new List<PlayversePurchaseAggregation>();

            if (endDate < startDate) {
                //throw new Exception("end date must be before start date");
            }
            //more validation

            string query = String.Format(@"SELECT CreatedOn AS 'RecordTimestamp', GameId, TransactionType, UserData, MAX(Credits) AS 'Cost', COUNT(*) AS 'TotalItems', SUM(Credits) AS 'TotalCredits'
            FROM GameCredit_GameCreditTransactions
            -- WHERE TransactionType = 1
            Where CreatedOn BETWEEN '{0}' AND '{1}'
            AND CreatedOn > '2015-04-28 15:00'
            AND UserId NOT IN (SELECT UserId FROM ACL_UserRoleAssignment)
            GROUP BY DATE(CreatedOn), UserData
            ORDER BY DATE(CreatedOn) DESC, SUM(Credits) DESC;", startDate.ToString("yyyy-MM-dd HH:mm:ss"), endDate.ToString("yyyy-MM-dd HH:mm:ss"));

            DataTable response = DBManager.Instance.Query(Datastore.General, query);

            if (response.Rows.Count > 0) {
                foreach (DataRow row in response.Rows) {
                    PlayversePurchaseAggregation PurchaseRow = new PlayversePurchaseAggregation() { 
                        Id = 0, //this will get assigned as an auto increment value by mysql so 0 is fine here
                        RecordTimestamp = DateTime.Parse(row["RecordTimestamp"].ToString()),
                        GameId = row["GameId"].ToString(),
                        TransactionType = Int32.Parse(row["TransactionType"].ToString()),
                        UserData = row["UserData"].ToString(),
                        TotalItems = Int32.Parse(row["TotalItems"].ToString()),
                        TotalCredits = Int32.Parse(row["TotalCredits"].ToString())
                    };
                    PurchaseTable.Add(PurchaseRow);
                }    
            }
            return PurchaseTable;
        }

        private List<PlayversePurchaseAggregation> GetLatestTransactions(string gameId)
        {
            DateTime lastUpdated;

            List<PlayversePurchaseAggregation> PurchaseTable = new List<PlayversePurchaseAggregation>();

            string latestQ = String.Format(@"SELECT MAX(RecordTimestamp) as LatestRecord FROM {0}", ECONOMY_TABLE);

            try
            {
                DataTable response = DBManager.Instance.Query(Datastore.Monitoring, latestQ);
                string latest = response.Rows[0]["LatestRecord"].ToString();
                DateTime.TryParse(latest, out lastUpdated);

                string query = String.Format(@"SELECT CreatedOn AS 'RecordTimestamp', GameId, TransactionType, UserData, MAX(Credits) AS 'Cost', COUNT(*) AS 'TotalItems', SUM(Credits) AS 'TotalCredits', Category
                FROM {2}
                Where CreatedOn BETWEEN '{0}' AND '{1}'
                AND CreatedOn > '2015-04-28 15:00'
                AND UserId NOT IN (SELECT UserId FROM ACL_UserRoleAssignment)
                GROUP BY DATE(CreatedOn), UserData
                ORDER BY DATE(CreatedOn) DESC, SUM(Credits) DESC;", lastUpdated.AddSeconds(1).ToString("yyyy-MM-dd HH:mm:ss"), DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), TRANSACTION_TABLE);
                
                DataTable data = DBManager.Instance.Query(Datastore.General, query);
                if (data.Rows.Count > 0)
                {
                    foreach (DataRow row in data.Rows)
                    {
                        PlayversePurchaseAggregation PurchaseRow = new PlayversePurchaseAggregation()
                        {
                            Id = 0, //this will get assigned as an auto increment value by mysql so 0 is fine here
                            RecordTimestamp = DateTime.Parse(row["RecordTimestamp"].ToString()),
                            GameId = row["GameId"].ToString(),
                            TransactionType = Int32.Parse(row["TransactionType"].ToString()),
                            UserData = row["UserData"].ToString(),
                            TotalItems = Int32.Parse(row["TotalItems"].ToString()),
                            TotalCredits = Int32.Parse(row["TotalCredits"].ToString()),
                            Category = row["Category"].ToString()
                        };
                        PurchaseTable.Add(PurchaseRow);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(String.Format(@"[Transactions] {0}", ex.Message), ex.StackTrace);
            }

            return PurchaseTable;



        }

        private void RecordGameCreditTransactions(List<PlayverseGameCreditTransaction> gameCreditTransactions)
        {
            foreach (List<PlayverseGameCreditTransaction> aggregationBatch in gameCreditTransactions.Batch<PlayverseGameCreditTransaction>(1000))
            {
                string InsertStatement = String.Format(

                  @"INSERT INTO {1} (`RecordTimestamp`,`TransactionId`,`UserId`, `GameId`,`ExternalOnlineService` , `ThirdPartyOrderId`, `Credits`, `PaymentProvider`, `PaymentTransactionId`, `TransactionType`, `CreditPackId`, 
                    `UserData`, `Description`, `CostAmount`, `Status`, `CreatedOn`, `UpdatedOn`, `Category`, `ClientKey`) VALUES {0}",
                DatabaseUtilities.instance.GenerateInsertValues<PlayverseGameCreditTransaction>(aggregationBatch), ECONOMY_GAMECREDITTRANSACTIONS_TABLE);

                try
                {
                    MoniverseResponse response = new MoniverseResponse()
                    {
                        Status = "unsent",
                        TimeStamp = DateTime.UtcNow
                    };

                    lock (MoniverseBase.ConsoleWriterLock)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("Beginning PlayverseGameCreditTransactionAggregation Batch");
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("");
                    }

                    Transactions.Service(service =>
                    {
                        response = service.Insert(new MoniverseRequest()
                        {
                            TaskName = "PlayverseGameCreditTransactionAggregation Insert",
                            Task = InsertStatement,
                            TimeStamp = DateTime.UtcNow
                        });
                    });
                    
                    lock (MoniverseBase.ConsoleWriterLock)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("PlayverseGameCreditTransactionAggregation Batch success");
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("");
                        Console.ResetColor();
                    }

                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Logger.Instance.Info(e.Message);
                    Console.ResetColor();
                }
            }
        }
   
        private void RecordPurchaseTransactionAggregation(List<PlayversePurchaseAggregation> aggregations) {

            if (aggregations.Count == 0) {
                return;
            }
            foreach (List<PlayversePurchaseAggregation> aggregationBatch in aggregations.Batch<PlayversePurchaseAggregation>(100)) {
                int listLength = aggregations.Count - 1;
                int RowIndex = 0;
                string InsertStatement = "INSERT INTO Economy_PurchaseBreakdownRaw (`ID`,`RecordTimestamp`, `GameId`,`Type` , `UserData`, `Cost`, `TotalBought`, `TotalCredits`) VALUES ";
                foreach (PlayversePurchaseAggregation RowEntry in aggregationBatch)
                {
                    string entry = "(";
                    int propertyCount = RowEntry.GetType().GetProperties().Length - 1;
                    var propertyIndex = 0;
                    foreach (PropertyInfo propertyInfo in RowEntry.GetType().GetProperties())
                    {
                        if (propertyInfo.CanRead)
                        {
                            object property = propertyInfo.GetValue(RowEntry);
                            if (property is DateTime)
                            {
                                entry += "'" + DateTime.Parse(propertyInfo.GetValue(RowEntry).ToString()).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                            }
                            else if (property is string)
                            {
                                entry += "'" + propertyInfo.GetValue(RowEntry).ToString().Replace("'", @"\'") + "'";
                            }
                            else
                            {
                                entry += "'" + propertyInfo.GetValue(RowEntry).ToString() + "'";
                            }


                            if (propertyIndex < propertyCount)
                            {
                                entry += ",";
                            }
                        }
                        propertyIndex++;
                    }
                    entry += ")";
                    InsertStatement += entry;

                    if (RowIndex < listLength)
                    {
                        InsertStatement += ",";
                    }
                    RowIndex++;
                }
                try
                {
                    MoniverseResponse response = new MoniverseResponse()
                    {
                        Status = "unsent",
                        TimeStamp = DateTime.UtcNow
                    };

                    lock (MoniverseBase.ConsoleWriterLock)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("Beginning RecordPurchaseTransactionAggregation Batch");
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("");
                    }
                    
                    Transactions.Service(service => {
                        response = service.Insert(new MoniverseRequest()
                        {
                        TaskName = "RecordPurchaseTransactionAggregation Insert",
                        Task = InsertStatement,
                        TimeStamp = DateTime.UtcNow
                        });
                    });
                    //int result = DBManager.Instance.Insert(Datastore.Monitoring, InsertStatement);
                    lock (MoniverseBase.ConsoleWriterLock)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("RecordPurchaseTransactionAggregation Batch success");
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("");
                        Console.ResetColor();
                    }

                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Logger.Instance.Info(e.Message);
                    Console.ResetColor();
                }
            
            }
            
        }

        private void RecordLatestTransactions(List<PlayversePurchaseAggregation> aggregations)
        {

            if (aggregations.Count == 0)
            {
                return;
            }
            foreach (List<PlayversePurchaseAggregation> aggregationBatch in aggregations.Batch<PlayversePurchaseAggregation>(1000))
            {
                int listLength = aggregations.Count - 1;
                string InsertStatement = String.Format(
                    @"INSERT INTO {1} (`ID`,`RecordTimestamp`, `GameId`,`Type` , `UserData`, `Category`, `Cost`, `TotalBought`, `TotalCredits`) VALUES {0}", 
                    DatabaseUtilities.instance.GenerateInsertValues<PlayversePurchaseAggregation>(aggregationBatch), ECONOMY_TABLE);                
                try
                {
                    MoniverseResponse response = new MoniverseResponse()
                    {
                        Status = "unsent",
                        TimeStamp = DateTime.UtcNow
                    };

                    lock (MoniverseBase.ConsoleWriterLock)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("Beginning RecordPurchaseTransactionAggregation Batch");
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("");
                    }

                    Transactions.Service(service =>
                    {
                        response = service.Insert(new MoniverseRequest()
                        {
                            TaskName = "RecordPurchaseTransactionAggregation Insert",
                            Task = InsertStatement,
                            TimeStamp = DateTime.UtcNow
                        });
                    });
                    //int result = DBManager.Instance.Insert(Datastore.Monitoring, InsertStatement);
                    lock (MoniverseBase.ConsoleWriterLock)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("RecordPurchaseTransactionAggregation Batch success");
                        Logger.Instance.Info("--------------------------------------");
                        Logger.Instance.Info("");
                        Console.ResetColor();
                    }

                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Logger.Instance.Info(e.Message);
                    Console.ResetColor();
                }

            }

        }

        public void RecordTransactionsFromRange(string gameId, DateTime start, DateTime end) {

            List<PlayversePurchaseAggregation> purchaseDays = GetPurchaseTransactions(gameId, start, end);
            RecordPurchaseTransactionAggregation(purchaseDays);

        }

        public List<PlayversePurchaseAggregation> getTransactionsForGameFromRange(GameMonitoringConfig Game, DateTime start, DateTime end)
        {
            //GameShortTitle = GameShortTitle.ToUpper();
            
            //GameMonitoringConfig game = Games.Instance.GetMonitoredGames().Where(x => x.ShortTitle == GameShortTitle).FirstOrDefault();
            //if (game == null) {
            //    throw new Exception("No Game by that name");
            //}

            return GetPurchaseTransactions(Game.Id, start, end);
        }

        public void CaptureGameCreditTransactions(GameMonitoringConfig game)
        {
            List<PlayverseGameCreditTransaction> data = GetGameCreditTransactions(game.Id);

            RecordGameCreditTransactions(data);
        }
        
        public void CaptureTransactionsForLastFullDay(GameMonitoringConfig game)
        {
            DateTime end = DateTime.Now.Date;
            DateTime start = end.AddDays(-1);
            List<PlayversePurchaseAggregation> purchaseDays = GetPurchaseTransactions(game.Id, start, end);

            RecordPurchaseTransactionAggregation(purchaseDays);
        }

        public void CaptureLatest(GameMonitoringConfig game)
        {
            List<PlayversePurchaseAggregation> purchaseDays = GetLatestTransactions(game.Id);

            RecordLatestTransactions(purchaseDays);

        }


    }
}
