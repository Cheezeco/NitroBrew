using System;
using System.Collections.Generic;
using System.Text;

namespace NitroBrew.Extensions
{
    internal static class IEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
        {
            foreach (var item in values)
            {
                action.Invoke(item);
            }
        }
    }
}
