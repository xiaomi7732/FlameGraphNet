using System.Collections.Generic;
using System.Linq;

namespace FlameGraphNet.Core
{
    static class Extensions
    {
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }

        public static IEnumerable<T> NullAsEmpty<T>(this IEnumerable<T> items)
        {
            if (items == null)
            {
                return Enumerable.Empty<T>();
            }
            return items;
        }
    }
}