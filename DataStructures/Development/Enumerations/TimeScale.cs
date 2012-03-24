using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ASTITransportation.Development.BaseClasses;

namespace ASTITransportation.Development.Enumerations
{
    public sealed class TimeScale : DoubleBase
    {

        public readonly byte None = 0;

        public readonly byte Milliseconds = 1;

        public readonly byte Seconds = 2;

        public readonly byte Minutes = 3;

        public readonly byte Hours = 4;

        public readonly byte Days = 5;

        public readonly byte Months = 6;

        public readonly byte Years = 7;

        public const double SecondsInMinute = 60;
        public const double MillisecondsInSecond = 1000;
        public const double MillisecondsInMinute = 60000;
        public const double MillisecondsInHour = 3600000;
        public const double MillisecondsInDay = 86400000;
        
        public const double DaysInMonth = 30.5;
        public const double MillisecondsInMonth = 2635200000;
    }
}
