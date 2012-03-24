using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.Snmp
{
    /// <summary>
    /// Used for ErrorStatus of the ProtocolDataUnit
    /// </summary>
    public enum ErrorStatus : int
    {

        NoError = 0,

        TooBig,

        NoSuchName,

        BadValue,

        ReadOnly,

        GenErr,

        EnterpriseSpecific,

        NoAccess,

        WrongType,

        WrongLength,

        WrongEncoding,

        WrongValue,

        NoCreation,

        InconsistentValue,

        ResourceUnavailable,

        CommitFailed,

        UndoFailed,

        AuthorizationError,

        NotWritable,

        InconsistentName
    }
}
