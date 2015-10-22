using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class DatabaseUtilities
    {
        public static DatabaseUtilities instance = new DatabaseUtilities();

        public string GenerateInsertValues<T>(List<T> EnumerableObjects)
        {

            string InsertStatement = "";
            int listLength = EnumerableObjects.Count - 1;
            int RowIndex = 0;
            foreach (object RowEntry in EnumerableObjects)
            {
                string entry = "(";
                int propertyCount = RowEntry.GetType().GetProperties().Length - 1;
                var propertyIndex = 0;
                foreach (PropertyInfo propertyInfo in RowEntry.GetType().GetProperties())
                {
                    if (propertyInfo.CanRead)
                    {
                        object property = propertyInfo.GetValue(RowEntry);
                        if (property == null)
                        {
                            entry += "NULL,";
                            continue;
                        }

                        if (property is DateTime)
                        {
                            DateTime propertyDate = DateTime.Parse(propertyInfo.GetValue(RowEntry).ToString());
                            if (propertyDate == DateTime.MinValue.ToUniversalTime() || propertyDate == DateTime.MinValue)
                            {
                                entry += "'" + propertyDate.ToString("1000-01-01 00:00:00") + "'";
                            }
                            else
                            {
                                entry += "'" + propertyDate.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                            }
                        }
                        else if (property is string)
                        {
                            entry += "'" + propertyInfo.GetValue(RowEntry).ToString().Replace("'", @"\'") + "'";
                        }
                        else if (property is bool)
                        {
                            int boolValue = Convert.ToInt32(propertyInfo.GetValue(RowEntry));
                            entry += "'" + boolValue.ToString() + "'";
                        }
                        else if (property.GetType().IsEnum)
                        {
                            entry += "'" + (int)property + "'";
                        }
                        else
                        {
                            string prpty = (propertyInfo.GetValue(RowEntry) != null) ? propertyInfo.GetValue(RowEntry).ToString() : "NULL";
                            entry += "'" + prpty + "'";
                        }


                        if (propertyIndex < propertyCount)
                        {
                            entry += ",";
                        }
                    }
                    propertyIndex++;
                }
                if (entry[entry.Length - 1] == ',')
                    entry = entry.TrimEnd(',');
                entry += ")";
                InsertStatement += entry;

                if (RowIndex < listLength)
                {
                    InsertStatement += ",";
                }
                if (InsertStatement[InsertStatement.Length - 1] == ',')
                    InsertStatement.TrimEnd(',');
                RowIndex++;
            }
            return InsertStatement;
        }
    }
}
