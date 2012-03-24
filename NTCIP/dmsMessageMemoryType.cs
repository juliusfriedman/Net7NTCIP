using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.NTCIP
{
    /*
        dmsMessageMemoryType OBJECT-TYPE
        SYNTAX INTEGER {
        other (1),
        permanent (2),
        changeable (3),
        volatile (4),
        currentBuffer (5),
        schedule (6),
        blank (7)
        }
        */
    public enum dmsMessageMemoryType : int
    {
        Other = 1,
        Permanent,
        Changeable,
        Volatile,
        CurrentBuffer,
        Schedule,
        Blank
    }
}
