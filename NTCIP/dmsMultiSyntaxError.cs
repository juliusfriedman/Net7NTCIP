using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.NTCIP
{
    //dmsMultiSyntaxError 
    public enum dmsMultiSyntaxError : int
    {
        Other = 1,
        None,
        UnsupportedTag,
        UnsupportedTagValue,
        TextTooBig,
        FontNotDefined,
        CharacterNotDefines,
        FieldDeviceNotExist,
        FieldDeviceError,
        FlashRegionError,
        TagConflict,
        TooManyPages
    }
}
