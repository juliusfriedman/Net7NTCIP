using System;
using System.Collections.Generic;
using System.Linq;
using ASTITransportation.Utilities;

namespace ASTITransportation.Snmp
{
    public static class SnmpExtensions
    {

        #region List<Variable> Extensions

        /// <summary>
        /// Adds the Identifier to the Bindings by Creating a Variable
        /// </summary>
        /// <param name="Bindings">The Bindings to Add a Variable to</param>
        /// <param name="Identifier">The Identifier to Create a Variable for</param>
        public static void Add(this List<Variable> Bindings, String Identifier)
        {            
            Bindings.Add(new Variable(Identifier));
        }

        /// <summary>
        /// Adds a range of Identifiers to a List of Variables
        /// </summary>
        /// <param name="Bindings">The Bindings to Add a Variable to</param>
        /// <param name="Identifiers">The Identifiers to Create a Variable for</param>
        public static void AddRange(this List<Variable> Bindings, List<string> Identifiers)
        {
            Identifiers.ForEach(oid => Bindings.Add(oid));            
        }

        #endregion

        #region PMPP (Point to Multi Point Protocol) Extensions List<byte>

        internal static byte PMPP = 0x7e;
        internal static byte PMPP_ESCAPE = 0x7d;
        internal static byte[] ESCAPE_PMPP = new byte[] { 0x7d, 0x5e };
        internal static byte[] ESCAPE_PMPPESCAPE = new byte[] { 0x7d, 0x5d };
        //internal static byte[][] Escapes = new byte[][]
        //{
        //    new byte[] { 0x7d, 0x5d },
        //    new byte[] { 0x7d, 0x5d }
        //};

        /// <summary>
        /// Encodes a List of Byte with the Hdlc Specialization PMPP
        /// </summary>
        /// <param name="InformationPayload">The bytes to be made transparent</param>
        /// <param name="Address">The Hdlc Address field</param>
        /// <param name="Control">The Hdlc Control field</param>
        /// <param name="ProtocolIdentifier">The Hdlc Initial Protocol Identifier</param>
        /// <returns>A List of Byte which has undergone Hdlc Transparency</returns>
        public static List<byte> PmppEncode(this List<byte> InformationPayload, Byte[] Address, Byte Control, Byte[] ProtocolIdentifier)
        {
            List<byte> Encoded = new List<byte>();

            InformationPayload.InsertRange(0, ProtocolIdentifier);
            InformationPayload.Insert(0, Control);
            InformationPayload.InsertRange(0, Address);
            InformationPayload.AddRange((new ASTITransportation.Utilities.Crc16()).ComputeHash(InformationPayload.ToArray()).Reverse());

            if (InformationPayload.Contains(PMPP) || InformationPayload.Contains(PMPP_ESCAPE))
            {
                InformationPayload.ForEach(b=>
                {
                    if (b == PMPP)
                    {
                        Encoded.AddRange(ESCAPE_PMPP);
                        //Encoded.Add(0x7d);
                        //Encoded.Add(0x5e);
                    }
                    else if (b == PMPP_ESCAPE)
                    {
                        Encoded.AddRange(ESCAPE_PMPPESCAPE);
                        //Encoded.Add(0x7d);
                        //Encoded.Add(0x5d);
                    }
                    else
                        Encoded.Add(b);
                });
            }
            else
                Encoded.AddRange(InformationPayload);                        
            
            //Pmpp Frame Flags
            Encoded.Insert(0, PMPP);
            Encoded.Add(PMPP);
            return Encoded;
        }        

        /// <summary>
        /// Encodes the ProtocolDataUnit with the PMPP Hdlc specialization
        /// </summary>
        /// <param name="Pdu">The ProtocolDataUnit to encode</param>
        /// <param name="HdlcAddress">The Hdlc Address Field</param>
        /// <param name="HdlcControlField">The Hdlc Control Field</param>
        /// <param name="HdlcProtocolIdentifier">The Hdlc InitalProtocolIdentifier</param>
        /// <returns>A PMPP Encoded ProtocolDataUnit</returns>
        public static List<byte> PmppEncode(this ProtocolDataUnit Pdu, Byte[] Address, Byte Control, Byte[] ProtocolIdentifier)
        {
            return Pdu.ToPacket(new PmppEndPoint(Address, Control, ProtocolIdentifier));
        }

        public static List<byte> PmppEncode(this ProtocolDataUnit Pdu, PmppEndPoint AddressStructure)
        {
            return PmppEncode(Pdu, AddressStructure.Address, AddressStructure.Control, AddressStructure.ProtocolIdentifier); 
        }

        public static List<byte> PmppEncode(this List<byte> InformationPayload, Transport.SnmpMessenger messengerSession)
        {
            return PmppEncode(InformationPayload, messengerSession.PmppEndPoint.Value.Address, messengerSession.PmppEndPoint.Value.Control, messengerSession.PmppEndPoint.Value.ProtocolIdentifier);
        }

        /// <summary>
        /// Removes the Hdlc Transparency from a List of Byte
        /// </summary>
        /// <param name="Bytes">The List of Byte to remove the Hdlc Transparency from</param>
        /// <returns>A List of Bytes which have been decoded from a Transparent state</returns>
        public static List<byte> PmppDecode(this List<byte> Bytes)
        {
            return PmppDecode(Bytes, false, null);
        }

        /// <summary>
        /// Removes the Hdlc Transparency from a List of Byte
        /// </summary>
        /// <param name="Bytes">The List of Byte to remove the Hdlc Transparency from</param>
        /// <param name="IncludeHdlcFields">Specifies the Hdlc Fields retrieved should be included in the the returned Bytes</param>
        /// <returns>A List of Bytes which have been decoded from a Transparent state</returns>
        public static List<byte> PmppDecode(this List<byte> Bytes, bool IncludeHdlcFields, Transport.SnmpMessenger messengerSession)
        {
            if (Bytes.Count == 0) return Bytes;
            if (Bytes.First() != 0x7e && Bytes.Last() != 0x7e)
            {
                int firstByte = Bytes.IndexOf(PMPP);
                int lastByte = Bytes.LastIndexOf(PMPP);
                if (firstByte == lastByte)
                {
                    //offsets are the same....
                    return Bytes;
                }
                Bytes = Bytes.GetRange(firstByte, lastByte);                
            }
            
            //Removes PMPP Frameing Chars
            Bytes.RemoveAt(0);
            Bytes.RemoveAt(Bytes.Count - 1);

            //Remove Transparency
            Int32 Position = 0;
            
            List<byte> Decoded = new List<byte>();

            if (!Bytes.Any(b => b == PMPP || b == PMPP_ESCAPE))
            {
                Decoded.AddRange(Bytes);
            }
            else
            {
                for (int end = Bytes.Count; Position < end; ++Position)
                {
                    if (Bytes[Position] == PMPP_ESCAPE)
                    {
                        if (Bytes[Position + 1] == 0x5e)
                        {
                            Decoded.Add(PMPP);
                            ++Position;
                        }
                        else if (Bytes[Position + 1] == 0x5d)
                        {
                            Decoded.Add(PMPP_ESCAPE);
                            ++Position;
                        }
                    }
                    else
                    {
                        Decoded.Add(Bytes[Position]);                        
                    }          
                }
            }

            


            //Get Checksum            
            List<byte> Checksum = Decoded.GetRange(Decoded.Count - 2, 2);
        
            //Remove Checksum from the working range
            Decoded.RemoveRange(Decoded.Count - 2, 2);
            
            //Calculate a Crc16 on the InformationPayload
            Crc16 alg = new Crc16();
            alg.ComputeHash(Decoded.ToArray());
            
            int? packetChecksum = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToUInt16(Checksum.ToArray(), 0));
            int? pseudoChecksum = System.Net.IPAddress.HostToNetworkOrder((int)alg.CrcValue);

            alg = null;
            Checksum = null;

            //Compare Checksums
            if (packetChecksum != pseudoChecksum)
            {
                if (null != messengerSession)
                {
                    messengerSession.PmppCrcErrors++;
                }
            }
            packetChecksum = null;
            pseudoChecksum = null;

            //back at the beginning to decode the Hdlc fields
            Position = 0;

            List<byte> HdlcAddress = new List<byte>();
            Byte? HdlcControlField;
            List<byte> HdlcProtocolIdentifier = new List<byte>();
            HdlcAddress = GetHdlcField(ref Decoded, ref Position);
            HdlcControlField = Bytes[Position++];
            HdlcProtocolIdentifier = GetHdlcField(ref Decoded, ref Position);


            if (!IncludeHdlcFields)
            {
                //Remove the fields we retrieved
                Decoded.RemoveRange(0, Position);
            }

            HdlcAddress = null;
            HdlcControlField = null;
            HdlcProtocolIdentifier = null;

            Bytes.Clear();

            Bytes.AddRange(Decoded);

            return Bytes;
        }

        #endregion

        #region HDLC (High Level Data Link Control Protocol) Extensions List<byte>

        /// <summary>
        /// Returns a List of Byte which has been decoded using the Hdlc field retrival rules.
        /// </summary>
        /// <param name="Bytes">The List of Bytes to retrieve the Hdlc Field from</param>
        /// <param name="Postion">The Position to start the search for the Hdlc Field</param>
        /// <returns>A Single Hdlc Field Value</returns>
        public static List<byte> GetHdlcField(ref List<byte> Bytes, ref Int32 Postion)
        {
            List<byte> field = new List<byte>();
            do
            {
                field.Add(Bytes[Postion++]);
            } while ((field.Last() & 0x01) == 0 && field.Count < 2);
            return field;
        }

        #endregion

        #region Conveinience Methods

        /// <summary>
        /// Converts the Type to a byte
        /// </summary>
        /// <param name="Type">The SnmpType to Convert</param>
        /// <returns>The Type as a byte</returns>
        public static byte ToByte(this SnmpType Type)
        {
            return ((byte)Type);
        }

        /// <summary>
        /// Converts the Type to a byte
        /// </summary>
        /// <param name="Status">The SnmpType to Convert</param>
        /// <returns>The Type as a byte</returns>
        public static byte ToByte(this ErrorStatus Status)
        {
            return ((byte)Status);
        }

        /// <summary>
        /// Converts a Byte into a SnmpType through casting
        /// </summary>
        /// <param name="b">The Byte to Cast as a SnmpType</param>
        /// <returns>The SnmpType casted on b</returns>
        public static SnmpType ToSnmpType(this Byte b)
        {
            return ((SnmpType)b);
        }

        /// <summary>
        /// Converts a Byte into a ErrorStatus through casting
        /// </summary>
        /// <param name="b">THe byte to Cast as a ErrorStatus</param>
        /// <returns>The ErrorStatus casted on b</returns>
        public static ErrorStatus ToErrorStatus(this Byte b)
        {
            return ((ErrorStatus)b);
        }

        #endregion

    }
}
