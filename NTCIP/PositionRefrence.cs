using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.NTCIP
{
    /// <summary>
    /// PositionRefrence from NTCIP 1205
    /// </summary>
    public enum PositionRefrence : int //OBJECT-TYPE
    {
        StopMovement = 0,
        Delta,
        Absolute,
        Continious
    }
}
