using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.Snmp
{
    public class ProtocolDataUnit
    {
        #region Fields

        /// <summary>
        /// The Version of the Snmp Standard to which this Protocol Data Unit adheres
        /// </summary>
        public Int32 Version
        {
            get
            {
                if (version <= 1) return 1;
                else return version;
            }
            set
            {
                if (value <= 1) value = 0;
                else if (value > 3) value = 3;
                version = value;
            }
        }

        /// <summary>
        /// The Request Identifier of this Protocol Data Unit
        /// </summary>
        public Int32 RequestId
        {
            get
            {
                return requestId;
            }
            set
            {
                if (value < 0) value = 0;
                requestId = value;
            }
        }

        /// <summary>
        /// The Type of Protocol Data Unit in the Snmp Standard this ProtcolDataUnit represents
        /// </summary>
        public SnmpType PduType { get; set; }

        /// <summary>
        /// The OctetString which indicates the intended level of access in the Protocol Data Unit
        /// </summary>
        public String CommunityName { get; set; }

        /// <summary>
        /// A Representation of any Error which may occur in the Snmp Communication Process
        /// </summary>
        public ErrorStatus ErrorStatus { get; set; }

        /// <summary>
        /// The Position in the List of Bindings in which the ErrorStatus occured
        /// </summary>
        public Int32 ErrorIndex { get; set; }

        /// <summary>
        /// A List of Managed Snmp Variables
        /// </summary>
        public List<Variable> Bindings { get; set; }

        #endregion

        #region Properties
        
        Int32 version;

        Int32 requestId;

        #endregion

        #region Methods

        /// <summary>
        /// Constructs the byte representation of the ProtocolDataUnit
        /// </summary>
        /// <returns>A Byte List which represents the ProtocolDataUnit</returns>
        public List<byte> ToPdu()
        {
            List<byte> Pdu = new List<byte>();
            List<byte> _RequestId = BasicEncodingRules.EncodeInteger32(this.RequestId);
            List<byte> _ErrorStatus = BasicEncodingRules.EncodeInteger32((int)this.ErrorStatus);
            List<byte> _ErrorIndex = BasicEncodingRules.EncodeInteger32(this.ErrorIndex);
            List<byte> _Bindings = BasicEncodingRules.EncodeSequence(this.Bindings);
            Pdu.AddRange(_RequestId);
            Pdu.AddRange(_ErrorStatus);
            Pdu.AddRange(_ErrorIndex);
            Pdu.AddRange(_Bindings);
            Pdu.InsertRange(0, BasicEncodingRules.EncodeSequenceHeader(PduType, Pdu.Count, true));
            Pdu.TrimExcess();
            return Pdu;
        }

        /// <summary>
        /// Constructs the byte representation of the SnmpPacket
        /// </summary>
        /// <returns>A Byte List which represents this ProtocolDataUnit as a SnmpPacket</returns>
        public List<byte> ToPacket()
        {
            return ToPacket(null);
        }

        /// <summary>
        /// Constructs the byte representation of the SnmpPacket
        /// </summary>
        /// <returns>A Byte List which represents this ProtocolDataUnit as a SnmpPacket</returns>
        public List<byte> ToPacket(PmppEndPoint? EndPoint)
        {
            try
            {
                List<byte> Bytes = new List<byte>();
                
                Bytes.AddRange(BasicEncodingRules.EncodeInteger32(this.version));
                Bytes.AddRange(BasicEncodingRules.EncodeOctetString(this.CommunityName));
                Bytes.AddRange(ToPdu());
                Bytes.InsertRange(0, BasicEncodingRules.EncodeSequenceHeader(SnmpType.Sequence, Bytes.Count, true));

                Bytes.TrimExcess();
                if (EndPoint.HasValue)
                {
                    Bytes = Bytes.PmppEncode(EndPoint.Value.Address, EndPoint.Value.Control, EndPoint.Value.ProtocolIdentifier);
                }

                return Bytes;
            }
            catch
            {
                throw;
            }            
        }

        /// <summary>
        /// Creates a String representation of the ProtocolDataUnit in JSON Format
        /// </summary>
        /// <returns>The String representation of the ProtcolDataUnit</returns>
        public override string ToString()
        {

            string PacketTemplate = @"{{'SnmpPacket':{{                
                    'Version': {0},
                    'CommunityName': {1},
                    {2}                
                }}
            }}";

            string PduTemplate = @"'ProtocolDataUnit':{{
                        'RequestId': {0},
                        'ErrorStatus': {1},
                        'ErrorIndex': {2},
                        'PduType': {3},
                        'Bindings': [{4}]
                    }}";

            string BindingTemplate = @"{{ 'Identifier': {0}, 'Type': {1}, 'Length': {2}, 'Values': {3} }}";

            string bindings = string.Empty;

            for (int i = 0, end = Bindings.Count; i < end; ++i)
            {
                Variable v = Bindings[i];
                string valueString;
                if (v.TypeCode != SnmpType.Null)
                {
                    valueString = "[" + string.Join(",", Array.ConvertAll<int, string>(Array.ConvertAll<byte, int>(v.Value.ToArray(), Convert.ToInt32), Convert.ToString)) + "]";

                }else{

                    valueString = "[]";
                }

                bindings += String.Format(BindingTemplate, ('"' + v.Identifier + '"'), ('"' + v.TypeCode.ToString() + '"'), v.Length.ToString(), valueString);

                if (i == end - 1)
                {
                    break;
                }
                else
                {
                    bindings += "," + Environment.NewLine;
                }
            }

            string pdu = string.Format(PduTemplate, RequestId, ('"' + ErrorStatus.ToString() + '"'), ErrorIndex, ('"' + PduType.ToString() + '"'), bindings);

            string output = string.Format(PacketTemplate, Version, CommunityName, pdu);

            return output;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a Snmp Protocol Data Unit
        /// </summary>
        public ProtocolDataUnit()
        {
            Bindings = new List<Variable>();
        }

        /// <summary>
        /// Constructs a Snmp Protocol Data Unit With a Specific Version
        /// </summary>
        /// <param name="Version">The Version to indicate in the ProcolDataUnit</param>
        public ProtocolDataUnit(Int32 Version)
            : this()
        {
            this.Version = Version;
        }

        /// <summary>
        /// Constructs a Managed Snmp Protocol Data Unit from a Byte Array
        /// </summary>
        /// <param name="Pdu">The Byte Array to Create a Managed Protocol Data Unit from</param>
        public ProtocolDataUnit(Byte[] Pdu)
            : this(new List<byte>(Pdu)) { }

        /// <summary>
        /// Copies a Snmp Protocol Data Unit
        /// </summary>
        /// <param name="Pdu">The Protocol Data Unit to Copy</param>
        public ProtocolDataUnit(ProtocolDataUnit Pdu)
        {
            this.version = Pdu.version;
            this.requestId = Pdu.requestId;
            this.PduType = Pdu.PduType;
            this.ErrorStatus = Pdu.ErrorStatus;
            this.ErrorIndex = Pdu.ErrorIndex;
            this.CommunityName = Pdu.CommunityName;
            this.Bindings = Pdu.Bindings;
        }

        /// <summary>
        /// Constructs a Managed Protocol Data Unit from a List of Byte
        /// </summary>
        /// <param name="Pdu">The List of Byte to Construct a Protocol Data Unit from</param>
        public ProtocolDataUnit(List<byte> SnmpPacket)
        {
            int Pos = 0;
            int End = BasicEncodingRules.DecodeLength(ref SnmpPacket, ref Pos, SnmpType.Sequence);
            Version = BasicEncodingRules.DecodeInteger32(ref SnmpPacket, ref Pos);
            CommunityName = BasicEncodingRules.DecodeOctetString(BasicEncodingRules.DecodeOctetString(ref SnmpPacket, ref Pos));
            PduType = (SnmpType)SnmpPacket[Pos];

            BasicEncodingRules.DecodeLength(ref SnmpPacket, ref Pos, PduType);
            RequestId = (int)BasicEncodingRules.DecodeInteger32(ref SnmpPacket, ref Pos);
            ErrorStatus = (ErrorStatus)BasicEncodingRules.DecodeInteger32(ref SnmpPacket, ref Pos);
            ErrorIndex = (int)BasicEncodingRules.DecodeInteger32(ref SnmpPacket, ref Pos);

            //Gets PDU Length
            int pduLeng = BasicEncodingRules.DecodeLength(ref SnmpPacket, ref Pos, SnmpType.Sequence);
            if (Pos >= End) return;
            Bindings = new List<Variable>();
            while (Pos < End)
            {
                try
                {
                    BasicEncodingRules.DecodeLength(ref SnmpPacket, ref Pos, SnmpType.Sequence);
                    Bindings.Add(BasicEncodingRules.DecodeSequence(ref SnmpPacket, ref Pos));
                }
                catch
                {
                    throw;
                }
            }
        }

        #endregion        
        
        #region Destructor

        /// <summary>
        /// Deconstuctor
        /// </summary>
        ~ProtocolDataUnit()
        {
            if (this.Bindings != null)
            {
                this.Bindings.Clear();
                this.Bindings = null;
            }
        }     

        #endregion
    }
}
