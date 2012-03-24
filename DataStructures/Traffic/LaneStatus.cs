using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.Traffic
{
    public enum LaneStatus : byte
    {
        Unknown = 0,
        OK,
        Suspect,
        Failed
    }
}
