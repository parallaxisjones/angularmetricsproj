using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Moniverse.Contract;

namespace PlayverseMetrics.Models
{
    public class BaseModel
    {
        protected List<Dictionary<string, object>> JSONFriendifyDataTable(DataTable dt)
        {
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            string[] columnNames = dt.Columns.Cast<DataColumn>()
                                 .Where(x => x.ColumnName.ToUpper() != "ID")
                                 .Select(x => x.ColumnName)
                                 .ToArray();

            rows.Add(new Dictionary<string, object>() { 
            {"columns", columnNames }
            });

            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    if (col.ColumnName == "ID")
                    {
                        continue;
                    }

                    if (dr[col] is DateTime)
                    {
                        row.Add(col.ColumnName, DateTime.Parse(dr[col].ToString()).ToUnixTimestamp() * 1000);
                        continue;
                    }
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }
            return rows;
        }
    }
}