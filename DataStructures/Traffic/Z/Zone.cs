using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.Traffic.Z
{
    //Zone Out
    //Seg in
    public class Zone
    {
        bool active = false;
        public bool Active
        {
            get
            {
                return active;
            }
            set
            {
                active = false;
            }
        }

        List<Traffic.S.Segment> segments = new List<S.Segment>();
        public List<Traffic.S.Segment> Segments
        {
            get { return segments; }
            set { segments = value; }
        }

        public void Calculate()
        {
            if (!active || Segments.Count <= 0) return;
            segments.OrderBy(segment => segment.MileMarkerStart);
            segments.ForEach(segment =>
            {
                //
            });
            return;
        }
    }
}
