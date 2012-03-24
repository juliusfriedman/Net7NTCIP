using System;

namespace ASTITransportation.Extensions
{
    public static class ArrayExtensions
    {
        ///<summary>
        ///	Check if the array is null or empty
        ///</summary>
        ///<param name = "source"></param>
        ///<returns></returns>
        public static bool IsNullOrEmpty(this Array source)
        {
            return source != null ? source.Length <= 0 : false;
        }

        ///<summary>
        ///	Check if the index is within the array
        ///</summary>
        ///<param name = "source"></param>
        ///<param name = "index"></param>
        ///<returns></returns>
        public static bool WithinIndex(this Array source, int index, int? dimension)
        {
            if (!dimension.HasValue) dimension = 0;
            return source != null && index >= source.GetLowerBound(dimension.Value) && index < source.GetUpperBound(dimension.Value);
        }
    }
}