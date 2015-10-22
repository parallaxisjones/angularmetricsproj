using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.AutoScaling.Model;
using Playverse.Data;
using System.Data;
using Moniverse.Contract;
using System.Text.RegularExpressions;
namespace Utilities.DataTools
{
    public class DataValidator
    {
        private const string PROD_ID = "_PROD";
        private const string STAGE_ID = "_STAGING";
        
        private List<IDatabaseManager> MonitoringEnvironments = new List<IDatabaseManager>();
        private IDatabaseManager ValidationDatabase = new LocalDatabase(Datastore.Validation);
        
        public DataValidator()
        {
            MonitoringEnvironments.Add(new ProductionDatabase(Datastore.Monitoring));
            MonitoringEnvironments.Add(new StagingDatabase(Datastore.Monitoring));
        }

        public static  DataValidator Instance = new DataValidator();

        public void Capture(string tableName, string DateColumn, DateTime startDate, DateTime endDate)
        {

            string CreateStatement = GetCreateStatement(tableName, MonitoringEnvironments.FirstOrDefault(x => x.GetEnvironment() == DBEnvironment.Production));
            string queryString = String.Format(@"SELECT * from {0} WHERE {1} BETWEEN '{2}' and '{3}'", tableName, DateColumn, startDate.ToString("yyyy-MM-dd hh:MM:ss"), endDate.ToString("yyyy-MM-dd hh:MM:ss"));
            foreach (IDatabaseManager manager in MonitoringEnvironments)
            {
                DataTable result = manager.Query(queryString);
                KeyValuePair<string, string> Createinfo = CreateReplicationStatement(manager.GetEnvironment(), CreateStatement);
                string validationTableCreate = Createinfo.Value;
                string validationTableName = Createinfo.Key;

                ValidationDatabase.Query(String.Format(@"DROP TABLE IF EXISTS {0}", validationTableName));
                ValidationDatabase.Query(String.Format(@"{0}", validationTableCreate));

                if(result.HasRows())
                {
                    if (result.Rows.Count > 1000)
                    {
                        List<DataTable> tableList = SplitTable(result, 1000);
                        foreach (DataTable table in tableList)
                        {
                            InsertDataTable(ValidationDatabase, table, validationTableName);
                        }
                    }
                    else
                    {
                        InsertDataTable(ValidationDatabase, result, validationTableName);
                    }


                }

            }
        }

        public void Diff(string tableName, string DateColumn, DateTime startDate, DateTime endDate)
        {
            string DiffTable = tableName + "_ENV_DIFF";

            Capture(tableName, DateColumn, startDate, endDate);

            string prodTable = tableName + PROD_ID;
            string stagingTable = tableName + STAGE_ID;

            DataTable PROD = ValidationDatabase.Query(SelectAll(prodTable));
            DataTable STAGE = ValidationDatabase.Query(SelectAll(stagingTable));
            DataTable result = Compare(PROD, STAGE);
           
            if (result.Rows.Count > 0)
            {
                ValidationDatabase.Query(GetDiffCreateTableTemplate(DiffTable));             
                InsertDataTable(ValidationDatabase, result, DiffTable);
            }
            else
            {
                Console.WriteLine("{0} contained no differences", tableName);
            }


            ValidationDatabase.Query(
                String.Format("DROP TABLE IF EXISTS {0}; DROP TABLE IF EXISTS {1}", 
                prodTable,
                stagingTable));

            Console.WriteLine("{0} analyzed", tableName);
        }

        private string GetCreateStatement(string TableName, IDatabaseManager db)
        {
            string queryString = String.Format(@"SHOW CREATE TABLE {0}", TableName);
            return db.Query(queryString).Rows[0]["Create Table"].ToString();

        }

        private KeyValuePair<string, string> CreateReplicationStatement(DBEnvironment env, string CreateStatement)
        {
            Regex rgx = new Regex("`(.*?)`");
            Match tableName = rgx.Match(CreateStatement);
            string ReplacementTable = tableName.Groups[1].ToString();
            switch (env)
            {
                case DBEnvironment.Production:
                    ReplacementTable = "`" + ReplacementTable + PROD_ID + "`";
                    break;
                case DBEnvironment.Staging:
                    ReplacementTable = "`" + ReplacementTable + STAGE_ID + "`";
                    break;
                default:
                    throw new Exception("Invalid Env");
            }

            return new KeyValuePair<string, string>(ReplacementTable, rgx.Replace(CreateStatement, ReplacementTable, 1));
        }


        public int InsertDataTable(IDatabaseManager db, DataTable data, string TableName)
        {
            string InsertString = String.Format(@"INSERT INTO {0} (", TableName);
            StringBuilder sb = new StringBuilder(InsertString);

            foreach (DataColumn column in data.Columns)
            {
                sb.Append(column.ColumnName + ",");
            }

            sb.Length--;
            sb.Append(") VALUES ");

            foreach (DataRow row in data.Rows)
            {
                sb.Append("(");
                foreach (DataColumn column in data.Columns)
                {
                    string value = string.Empty;
                    if (column.DataType.Name == "DateTime")
                    {
                        value = row.Field<DateTime>(column.ColumnName).ToString("yyyy-MM-dd hh:MM:ss");
                    }
                    else
                    {
                        value = row[column.ColumnName].ToString().Replace("'", @"\'");
                    }
                    sb.Append("'" + value + "'");
                    sb.Append(",");
                }
                sb.Length--;
                sb.Append("),");
            }
            sb.Length--;
            sb.Length--;
            sb.Append(")");
            InsertString = sb.ToString() + ";";
            return db.Insert(InsertString);
        }

        public string SelectAll(string tablename)
        {
            return String.Format("SELECT * From {0}", tablename);
        }
        public DataTable Union(DataTable First, DataTable Second)
        {
            //Result table
            DataTable table = new DataTable("Union");
            //Build new columns
            DataColumn[] newcolumns = new DataColumn[First.Columns.Count];
            for (int i = 0; i < First.Columns.Count; i++)
            {
                newcolumns[i] = new DataColumn(First.Columns[i].ColumnName,
                First.Columns[i].DataType);
            }
            //add new columns to result table
            table.Columns.AddRange(newcolumns);
            table.BeginLoadData();
            //Load data from first table
            foreach (DataRow row in First.Rows)
            {
                table.LoadDataRow(row.ItemArray, true);
            }
            //Load data from second table
            foreach (DataRow row in Second.Rows)
            {
                table.LoadDataRow(row.ItemArray, true);
            }
            table.EndLoadData();
            return table;
        }

        public DataTable Compare(DataTable First, DataTable Second)
        {
            //Result table
            DataTable table = new DataTable("Diff");
            //Build new columns
            List<DataColumn> newcolumns = new List<DataColumn>();
            
            newcolumns.Add(new DataColumn("Date", typeof(DateTime)));
            newcolumns.Add(new DataColumn("Field", typeof(String)));
            newcolumns.Add(new DataColumn("Production", typeof(String)));
            newcolumns.Add(new DataColumn("Staging", typeof(String)));
            newcolumns.Add(new DataColumn("Diff", typeof(String)));
            //add new columns to result table
            table.Columns.AddRange(newcolumns.ToArray());
            table.BeginLoadData();
            //Load data from first table
            for (int i = 0; i <= First.Rows.Count - 1; i++)
            {
                foreach (DataColumn col in First.Columns)
                {
                    bool bIsConflict = false;
                    if (col.ColumnName == "ID")
                    {
                        continue;
                    }
                    if (col.DataType == typeof(DateTime))
                    {
                        bIsConflict = !(First.Rows[i].Field<DateTime>(col.ColumnName) == Second.Rows[i].Field<DateTime>(col.ColumnName));
                    }
                    if (col.DataType == typeof(String))
                    {
                        bIsConflict = !(First.Rows[i].Field<String>(col.ColumnName) == Second.Rows[i].Field<String>(col.ColumnName));
                    }
                    if (col.DataType == typeof(Int32))
                    {
                        bIsConflict = !(First.Rows[i].Field<Int32>(col.ColumnName) == Second.Rows[i].Field<Int32>(col.ColumnName));
                    }
                    if (bIsConflict)
                    {
                        DataRow newRow = table.NewRow();
                        newRow["Date"] = First.Rows[i]["RecordTimestamp"];
                        newRow["Field"] = col.ColumnName;
                        newRow["Production"] = First.Rows[i][col.ColumnName].ToString();
                        newRow["Staging"] = Second.Rows[i][col.ColumnName].ToString();
                        if (col.DataType == typeof(DateTime))
                        {
                            newRow["Diff"] = First.Rows[i].Field<int>("Count") - Second.Rows[i].Field<int>("Count");   
                        }




                        if (col.DataType == typeof(Int32))
                        {
                            bIsConflict = !(First.Rows[i].Field<Int32>(col.ColumnName) == Second.Rows[i].Field<Int32>(col.ColumnName));
                        }

                        newRow["Diff"] = First.Rows[i].Field<int>("Count") - Second.Rows[i].Field<int>("Count");   
                        table.Rows.Add(newRow);
                    }
                }
            }
            table.EndLoadData();
            return table;
        }

        private string GetDiffCreateTableTemplate(string tableName)
        {
            return String.Format(@"CREATE TABLE `{0}` (
              `ID` int(11) NOT NULL AUTO_INCREMENT,
              `Date` timestamp NOT NULL,
              `Field` varchar(255) NOT NULL,
              `Production` varchar(255) NOT NULL,
              `Staging` varchar(255) NOT NULL,
              `Diff` varchar(255) NOT NULL,  
              PRIMARY KEY (`ID`)
            ) ENGINE=InnoDB AUTO_INCREMENT=1182 DEFAULT CHARSET=utf8;
            ", tableName);
        }

        public List<string> getActiveTables()
        {
            List<string> ProdTables = new List<string>();
            List<string> StaingTables = new List<string>();
            List<string> ActiveTables = new List<string>();

            string queryId = "T";
            string TablesAs = String.Format(@"SELECT 
                 table_name AS {0} 
                FROM 
                 information_schema.tables
                WHERE 
                 table_schema = DATABASE()", queryId);

            DataTable result = MonitoringEnvironments.FirstOrDefault(x => x.GetEnvironment() == DBEnvironment.Staging).Query(TablesAs);

            if (result.Rows.Count > 0)
            {
                foreach (DataRow row in result.Rows)
                {
                    StaingTables.Add(row.Field<string>(queryId));
                }

                DataTable compare = MonitoringEnvironments.FirstOrDefault(x => x.GetEnvironment() == DBEnvironment.Production).Query(TablesAs);

                if (compare.Rows.Count > 0)
                {
                    foreach (DataRow row in compare.Rows)
                    {
                        ProdTables.Add(row.Field<string>(queryId));
                    }
                }
            }

            return ProdTables.Intersect(StaingTables).ToList();
        }

        private static List<DataTable> SplitTable(DataTable originalTable, int batchSize)
        {
            List<DataTable> tables = new List<DataTable>();
            int i = 0;
            int j = 1;
            DataTable newDt = originalTable.Clone();
            newDt.TableName = "Table_" + j;
            newDt.Clear();
            foreach (DataRow row in originalTable.Rows)
            {
                DataRow newRow = newDt.NewRow();
                newRow.ItemArray = row.ItemArray;
                newDt.Rows.Add(newRow);
                i++;
                if (i == batchSize)
                {
                    tables.Add(newDt);
                    j++;
                    newDt = originalTable.Clone();
                    newDt.TableName = "Table_" + j;
                    newDt.Clear();
                    i = 0;
                }
            }
            return tables;
        }

    }
}
