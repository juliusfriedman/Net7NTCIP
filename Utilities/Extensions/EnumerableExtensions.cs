using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.Extensions
{
    public static class EnumerableExtensions
    {        
        public static void ForEach<T>(this IEnumerable<T> @enum, Action<T> mapFunction)
        {
            if (null == @enum) throw new ArgumentNullException("@enum");
            if (mapFunction == null) throw new ArgumentNullException("mapFunction");
            //@enum.ToList().ForEach(mapFunction);
            foreach (T item in @enum) mapFunction(item);
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> iEnumerable)
        {
            return iEnumerable == null || !iEnumerable.Any();
        }
    }
}
