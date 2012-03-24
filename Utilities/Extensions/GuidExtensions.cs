using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.Extensions
{
    public static class GuidExtensions
    {
        public static string ToShortGuidString(this Guid value)
        {
            return Convert.ToBase64String(value.ToByteArray()).Replace("/", "_").Replace("+", "-").Substring(0, 22);
        }

        public static Guid FromShortGuidString(this string value)
        {
            return new Guid(Convert.FromBase64String(value.Replace("_", "/").Replace("-", "+") + "=="));
        } 
    }
}
