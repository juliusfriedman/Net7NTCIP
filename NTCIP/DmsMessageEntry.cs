using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.NTCIP
{
    public enum DmsMessageEntry //SEQUENCE
    {
        dmsMessageMemoryType = 1,//INTEGER
        dmsMessageNumber,//INTEGER
        dmsMessageMultiString,//OCTET STRING
        dmsMessageOwner,//OwnerString
        dmsMessageCRC,//INTEGER
        dmsMessageBeacon,//INTEGER
        dmsMessagePixelService,//INTEGER
        dmsMessageRunTimePriority,//INTEGER
        dmsMessageStatus,//INTEGER
    }
}
