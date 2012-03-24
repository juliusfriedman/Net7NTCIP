using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ASTITransportation.Development.BaseClasses;

namespace ASTITransportation.Development.Enumerations
{
    public sealed class DistanceScale : DoubleBase
    {

        public readonly byte None = 0;

        public readonly byte Millimeters = 1;

        public readonly byte Decimeters = 2;

        public readonly byte Meters = 3;

        public readonly byte Kilometers = 4;

        public readonly byte Feet = 5;

        public readonly byte Miles = 6;

        public readonly byte Yards = 7;

        public readonly double DecimetersInCentimeter = 10;
        public readonly double DecimetersInMillimeter = 100;

        public readonly double MillimetersInMeters = 1000;
        public readonly double MillimetersInFeet = 304.8;

        public readonly double FeetInMeter = 3.28;
        public readonly double FeetInKilometer = 1328;
        public readonly double FeetInYear = 3;
        public readonly double InchesInFeet = 12;
    }
}
