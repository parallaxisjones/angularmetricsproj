using System.Collections.Generic;
using System.Linq;

namespace Moniverse.Contract
{
    public static class ListExtensions
    {
        public static List<T> LastAsList<T>(this List<T> list)
        {
            if (list == null || !list.Any())
                return list;

            return list.GetRange(list.Count() - 1, 1);
        }
    }
}