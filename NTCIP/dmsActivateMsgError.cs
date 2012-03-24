using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.NTCIP
{
    //dmsActivateMsgError
    public enum dmsActivateMsgError : int //OBJECT-TYPE
    {
        Other = 1,
        None,
        Priority,
        MessageStatus,
        MemoryType,
        MessageNumber,
        MessageCRC,
        SyntaxMULTI,
        LocalMode,
    }
}
