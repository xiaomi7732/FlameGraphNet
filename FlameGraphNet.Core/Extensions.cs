using System.Collections.Generic;

namespace FlameGraphNet.Core
{
    static class Extensions
    {
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }
    }
}