using System.Data;

namespace Moniverse.Contract
{
    public static class DataTableExtensions
    {
        public static bool HasRows(this DataTable dt)
        {
            bool hasRows = false;
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                hasRows = true;
            }
            return hasRows;
        }
    }
}