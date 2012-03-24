using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.Traffic.S
{
    public class Segment
    {
        double mileMarkerStart = 0.0D;
        public double MileMarkerStart
        {
            get { return mileMarkerStart; }
            set { mileMarkerStart = value; }
        }

        double mileMarkerEnd = 0.0D;
        public double MileMarkerEnd
        {
            get { return mileMarkerEnd; }
            set { mileMarkerEnd = value; }
        }

        public double DistanceFactor
        {
            get
            {
                return Math.Abs(mileMarkerStart - MileMarkerEnd);
            }
        }

        double maxSpeed = 0.0D;
        public double MaxSpeed
        {
            get { return maxSpeed; }
            set { maxSpeed = value; }
        }

        double freeFlowSpeed = 0.0D;
        public double FreeFlowSpeed
        {
            get
            {
                return freeFlowSpeed;
            }
            set
            {
                freeFlowSpeed = value;
            }
        }

        LaneCollection segmentLanes = new LaneCollection();
        public LaneCollection Lanes
        {
            get { return segmentLanes; }
            set { segmentLanes = value; }
        }
    }
}
