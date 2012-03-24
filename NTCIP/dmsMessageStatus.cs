using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.NTCIP
{
    public enum dmsMessageStatus : int //OBJECT-TYPE
    {
        NotUsed = 1,
        Modifying,
        Validating,
        Valid,
        Error,
        ModifyReq,
        ValidateReq,
        NotUsedReq
    }
}
