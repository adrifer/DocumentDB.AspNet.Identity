using System;
using System.Collections.Generic;
using System.Linq;

namespace DocumentDB.AspNet.Identity
{
    internal static class Extensions
    {
        internal static IList<T> ToIList<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.ToList();
        }

        public static IList<T> Remove<T>(this IList<T> collection, Func<T, bool> predicate)
        {
            var item = collection.FirstOrDefault(predicate);
            if (item != null)
            {
                collection.Remove(item);
            }

            return collection;
        }
    }
}
