using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ASTITransportation.Extensions
{
    public static class GenericExtensions
    {
        public static bool In<T>(this T source, params T[] list)
        {
            if (null == source) throw new ArgumentNullException("source");
            if (null == list) throw new ArgumentNullException("list");
            return list.Contains(source);
        }

        [DebuggerStepThrough()]
        public static bool Is<T>(this object item) where T : class
        {
            return item is T;
        }

        [DebuggerStepThrough()]
        public static bool IsNot<T>(this object item) where T : class
        {
            return !(item.Is<T>());
        }

        [DebuggerStepThrough()]
        public static T As<T>(this object item) where T : class
        {
            return item as T;
        }

        [DebuggerStepThrough]
        public static void Swap<T>(ref T a, ref T b)
        {
            T t = a; a = b; b = t;            
        }

        [DebuggerStepThrough]
        public static void Sort<T>(ref T a, ref T b)
        where T : IComparable<T>
        {
            if (a.CompareTo(b) > 0)
                Swap(ref a, ref b);
        }


    }
}
