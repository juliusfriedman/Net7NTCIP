using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.Snmp
{
    /// <summary>
    /// A structure which contains the Point to Multi Point Protocol Addressing Information
    /// </summary>
    public struct PmppEndPoint
    {

        public static readonly PmppEndPoint NTCIP = new PmppEndPoint(0x05, 0x03, 0xC1);

        #region Properties

        /// <summary>
        /// The Address of the EndPoint
        /// </summary>
        public Byte[] Address;
        /// <summary>
        /// The Control Byte of the EndPoint
        /// </summary>
        public Byte Control;
        /// <summary>
        /// The Protocol Identifier of the EndPoint
        /// </summary>
        public Byte[] ProtocolIdentifier;

        #endregion

        #region Methods

        /// <summary>
        /// Shows the Details of the Endpoint as a String
        /// </summary>
        /// <returns>A String contaiing the details of the PmppEndpoint</returns>
        public override string ToString()
        {
            return "{" + BitConverter.ToString(Address) + "," + BitConverter.ToString(new byte[] { Control }) + "," + BitConverter.ToString(ProtocolIdentifier) + "}";
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Construcuts a PmppEndPoint
        /// </summary>
        /// <param name="Address">The Address of the EndPoint</param>
        /// <param name="Control">The Control Byte of the EndPoint</param>
        /// <param name="ProtocolIdentifier">The Protocol Identifier of the EndPoint</param>
        public PmppEndPoint(Byte[] Address, Byte Control, Byte[] ProtocolIdentifier)
        {
            if (Address.Length > 2) throw new ArgumentException("Field Length cannot be greater then 2 bytes", "Address");
            if (ProtocolIdentifier.Length > 2) throw new ArgumentException("Field Length cannot be greater then 2 bytes", "ProtocolIdentifier");
            this.Address = Address;
            this.Control = Control;
            this.ProtocolIdentifier = ProtocolIdentifier;
        }

        /// <summary>
        /// Construcuts a PmppEndPoint
        /// </summary>
        /// <param name="Address">The Address of the EndPoint</param>
        /// <param name="Control">The Control Byte of the EndPoint</param>
        /// <param name="ProtocolIdentifier">The Protocol Identifier of the EndPoint</param>
        public PmppEndPoint(Byte Address, Byte Control, Byte ProtocolIdentifier)
        {
            this.Address = new byte[] { Address };
            this.Control = Control;
            this.ProtocolIdentifier = new byte[] { ProtocolIdentifier };
        }

        #endregion
    }
}
