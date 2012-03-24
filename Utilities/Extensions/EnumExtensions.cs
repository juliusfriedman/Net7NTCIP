using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.Extensions
{
    public static class EnumExtensions
    {
        public static bool HasFlag(this Enum aEnum, Enum flag)
        {
            if (!aEnum.GetType().IsEquivalentTo(flag.GetType()))
            {
                throw new ArgumentException("Enum Types do not match");
            }
            ulong num = Convert.ToUInt64(flag);
            return ((Convert.ToUInt64(aEnum) & num) == num);
        }
    }
}
