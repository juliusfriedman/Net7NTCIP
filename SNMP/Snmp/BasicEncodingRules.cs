using System;
using System.Collections.Generic;
using System.Linq;

namespace ASTITransportation.Snmp
{
    public sealed class BasicEncodingRules
    {

        public enum BaseTypes : byte
        {
            Universal = 0x0,
            Application = 0x40,
            Context = 0x80,
            Public = 0xC0,
            Primitive = Universal,
            Constructor = 0x20
        };

        #region Constructor
        
        /// <summary>
        /// Prevents a sealed class with all static memebers from being instantiated
        /// </summary>
        private BasicEncodingRules()
        {
        }
        
        #endregion

        #region Methods

        #region ASN.1 Length

        /// <summary>
        /// Encodes the given Integer length into a binary ASN.1 Length
        /// </summary>
        /// <param name="length">The Length to encode</param>
        /// <returns>A byte[] which represents the given length</returns>
        public static List<byte> EncodeLength(int length)
        {
            ////List<byte> bytes = new List<byte>();
            ////List<byte> buf = new List<byte>();
            ////byte[] len = BitConverter.GetBytes(length);
            ////for (int i = 3; i >= 0; i--)
            ////{
            ////    if (len[i] != 0 || buf.Count > 0)
            ////        buf.Add(len[i]);
            ////}
            ////if (buf.Count == 0)
            ////{
            ////    // we are encoding a 0 value. Can't have a 0 byte length encoding
            ////    buf.Add(0);
            ////}
            ////// check for short form encoding
            ////if (buf.Count == 1 && (buf[0] & 0x80) == 0)
            ////    bytes.AddRange(buf); // done
            ////else
            ////{
            ////    // long form encoding
            ////    byte encHeader = (byte)buf.Count;
            ////    encHeader = (byte)(encHeader | 0x80);
            ////    bytes.Add(encHeader);
            ////    bytes.AddRange(buf);
            ////}
            ////return bytes;

            List<byte> bytes = new List<byte>();
            if (length < 0) throw new System.ArgumentException("length cannot be negative", "length");
            else if (length < 127)
            {
                bytes.Add((byte)length);
                return bytes;
            }
            else while (length != 0)
                {
                    bytes.Add((byte)(length & 255));
                    length >>= 8;
                }
            bytes.Insert(0, (byte)(bytes.Count | 128));
            return bytes;

        }

        /// <summary>
        /// Decodes the ASN.1 Length from the given packet at the given position, checking for the given tag before decoding.
        /// </summary>
        /// <param name="packet">The packet which contains the ASN.1 data</param>        
        /// <param name="tag">The SnmpType to Ensure is Preceeding the data</param>
        /// <returns>The decoded Integer32 Length of the ASN.1 data</returns>
        public static int DecodeLength(List<byte> packet, SnmpType? tag)
        {
            int position = 0;
            if (tag.HasValue && packet[position++] != (byte)tag)
            {
                throw new System.Exception("ASN.1 " + System.String.Format(System.Globalization.CultureInfo.InvariantCulture, "0x{0:x}", tag) + " expected.");
            } else return DecodeLength(ref packet, ref position);
        }

        /// <summary>
        /// Decodes the ASN.1 Length from the given packet at the given position, checking for the given tag before decoding.
        /// </summary>
        /// <param name="packet">The packet which contains the ASN.1 data</param>
        /// <param name="position">The current position in the packet</param>              
        /// <param name="tag">The SnmpType to Ensure is Preceeding the data</param>
        /// <returns>The decoded Integer32 Length of the ASN.1 data</returns>
        public static int DecodeLength(ref List<byte> packet, ref int position, SnmpType tag)
        {
            //if (tag == SnmpType.Null) return 1;
            if (packet[position++] != (byte)tag)
            {
                throw new System.Exception("ASN.1 " + System.String.Format(System.Globalization.CultureInfo.InvariantCulture, "0x{0:x}", tag) + " expected.");
            } else return DecodeLength(ref packet, ref position);
        }

        /// <summary>
        /// Decodes the ASN.1 Length from the given packet at the given position
        /// </summary>
        /// <param name="packet">The packet which contains the ASN.1 data</param>
        /// <param name="position">The current position in the packet</param>
        /// <returns>The decoded Integer32 Length of the ASN.1 data</returns>
        public static int DecodeLength(ref List<byte> packet, ref int position)
        {
            ////if (position == packet.Count)
            ////    throw new OverflowException("Buffer is too short.");
            ////int dataLen = 0;
            ////if ((packet[position] & 0x80) == 0)
            ////{
            ////    // short form encoding
            ////    dataLen = packet[position++];
            ////    return dataLen; // we are done
            ////}
            ////else
            ////{
            ////    dataLen = packet[position++] & ~0x80; // store byte length of the encoded length value
            ////    int value = 0;
            ////    for (int i = 0; i < dataLen; ++i)
            ////    {
            ////        value <<= 8;
            ////        value |= packet[position++];
            ////        if (position > packet.Count || (i < (dataLen - 1) && position == packet.Count))
            ////            throw new OverflowException("Buffer is too short.");
            ////    }
            ////    return value;
            ////}        

            int length = packet[position++];
            if (length >= 128)
            {
                length = 0;
                for (int end = length & 127; position < end; ) length = (length << 8) | packet[position++];
            }
            return length;
        }

        #endregion

        #region ASN.1 Integer32

        /// <summary>
        /// Encodes the given Integer as a ASN.1 Integer32
        /// </summary>
        /// <param name="value">The system Integer32 to encode as a ASN.1 Integer32</param>
        /// <returns>The ASN.1 Encoded Integer representation of the given value</returns>
        public static List<byte> EncodeInteger32(int value)
        {
            return EncodeInteger32(value, true, true);
        }

        /// <summary>
        /// Encodes the given Integccer as a ASN.1 Integer32
        /// </summary>
        /// <param name="value">The system Integer32 to encode as a ASN.1 Integer32</param>
        /// <param name="includeType">A value to indicate if the ASN.1 Type should be included in the ASN.1 Encoded Response</param>
        /// <param name="includeLength">A value to indicate if the ASN.1 Encoded Length should be included in the ASN.1 Encoded Response</param>
        /// <returns>The ASN.1 Encoded Integer representation of the given value</returns>
        public static List<byte> EncodeInteger32(int value, bool includeType, bool includeLength)
        {
            return EncodeInteger32(value, includeType, includeLength, null);
        }

        /// <summary>
        /// Encodes the given Integer as a ASN.1 Integer32
        /// </summary>
        /// <param name="value">The system Integer32 to encode as a ASN.1 Integer32</param>
        /// <param name="includeType">A value to indicate if the ASN.1 Type should be included in the ASN.1 Encoded Response</param>
        /// <param name="includeLength">A value to indicate if the ASN.1 Encoded Length should be included in the ASN.1 Encoded Response</param>
        /// <param name="asType">The SnmpType to represent as a ASN.1 Integer32</param>
        /// <returns>The ASN.1 Encoded Integer representation of the given value</returns>
        public static List<byte> EncodeInteger32(int value, bool includeType, bool includeLength, SnmpType? asType)
        {
            int val = (int)value;

            byte[] integerBytes = BitConverter.GetBytes(value);

            List<byte> encoded = new List<byte>();
            // if value is negative
            if (val < 0)
            {

                for (int i = 3; i >= 0; i--)
                {
                    if (encoded.Count > 0 || integerBytes[i] != 0xff)
                    {
                        encoded.Add(integerBytes[i]);
                    }
                }

                if (encoded.Count == 0)
                {
                    // if the value is -1 then all bytes in an integer are 0xff and will be skipped above
                    encoded.Insert(0, 0xff);
                }
                // make sure value is negative
                if ((encoded[0] & 0x80) == 0)
                    encoded.Insert(0, 0xff);
            }
            else if (val == 0)
            {
                // this is just a shortcut to save processing time
                encoded.Insert(0, 0);
            }
            else
            {
                for (int i = 3; i >= 0; i--)
                {
                    if (integerBytes[i] != 0 || encoded.Count > 0)
                        encoded.Add(integerBytes[i]);
                }
                // if buffer length is 0 then value is 0 and we have to add it to the buffer
                if (encoded.Count == 0)
                    encoded.Insert(0, 0);
                else
                {
                    if ((encoded[0] & 0x80) != 0)
                    {
                        // first bit of the first byte has to be 0 otherwise value is negative.
                        encoded.Insert(0, 0);
                    }
                }
            }
            // check for 9 1s at the beginning of the encoded value
            if ((encoded.Count > 1 && encoded[0] == 0xff && (encoded[1] & 0x80) != 0))
            {

                encoded.Insert(0, 0);
            }
            if (includeLength) encoded.InsertRange(0, EncodeLength(encoded.Count));
            if (includeType) encoded.Insert(0, (byte)(asType ?? SnmpType.Integer32));
            return encoded;
        }

        /// <summary>
        /// Decodes a ASN.1 Integer32 from the given packet at the given position
        /// </summary>
        /// <param name="packet">The packet which contains the ASN.1 data</param>        
        /// <param name="position">The current position in the packet</param>              
        /// <returns>A System Integer32 decoded from the ASN.1 data</returns>
        public static int DecodeInteger32(ref List<byte> packet, ref int position)
        {
            return DecodeInteger32(ref packet, ref position, null);
        }

        /// <summary>
        /// Decodes a ASN.1 Integer32 from the given packet at the given position
        /// </summary>
        /// <param name="packet">The packet which contains the ASN.1 data</param>        
        /// <param name="position">The current position in the packet</param>              
        /// <param name="tag">The optional SnmpType to Ensure is Preceeding the data</param>
        /// <returns>A System Integer32 decoded from the ASN.1 data</returns>
        public static int DecodeInteger32(ref List<byte> packet, ref int position, SnmpType? tag)
        {
            if (!tag.HasValue) tag = packet[position].ToSnmpType();
           
            int Len = DecodeLength(ref packet, ref position, tag.Value);

            int Val = (packet[position] & 0x80) != 0 ? -1 : 0;

            if (packet[position] == 0x80 && Len > 2 && (packet[position + 1] == 0xff && (packet[position + 2] & 0x80) != 0))
            {
                // this is a filler byte to comply with no 9 x consecutive 1s
                position += 1;
                // we've used one byte of the encoded length
                Len -= 1;
            }

            for (int i = 0; i < Len; ++i)
            {
                Val <<= 8;
                Val = Val | packet[position++];
            }

            return Val;
        }

        #endregion

        #region ASN.1 UInteger32

        /// <summary>
        /// Encodes the given Integer as a ASN.1 UInteger32
        /// </summary>
        /// <param name="value">The system UInt32 to encode as a ASN.1 Integer32</param>
        /// <returns>The ASN.1 Encoded UInt32 representation of the given value</returns>
        public static List<byte> EncodeUInteger32(UInt32 value)
        {
            return EncodeUInteger32(value, true, true);
        }

        /// <summary>
        /// Encodes the given UInt32 as a ASN.1 UInteger32
        /// </summary>
        /// <param name="value">The system UInt32 to encode as a ASN.1 UInteger32</param>
        /// <param name="includeType">A value to indicate if the ASN.1 Type should be included in the ASN.1 Encoded Response</param>
        /// <param name="includeLength">A value to indicate if the ASN.1 Encoded Length should be included in the ASN.1 Encoded Response</param>
        /// <returns>The ASN.1 Encoded UInteger32 representation of the given value</returns>
        public static List<byte> EncodeUInteger32(UInt32 value, bool includeType, bool includeLength)
        {
            return EncodeUInteger32(value, includeType, includeLength, null);
        }

        /// <summary>
        /// Encodes the given UInt32 as a ASN.1 UInteger32
        /// </summary>
        /// <param name="value">The system UInt32 to encode as a ASN.1 UInteger32</param>
        /// <param name="includeType">A value to indicate if the ASN.1 Type should be included in the ASN.1 Encoded Response</param>
        /// <param name="includeLength">A value to indicate if the ASN.1 Encoded Length should be included in the ASN.1 Encoded Response</param>
        /// <param name="asType">The SnmpType to represent as a ASN.1 UInteger32</param>
        /// <returns>The ASN.1 Encoded UInteger32 representation of the given value</returns>
        public static List<byte> EncodeUInteger32(UInt32 value, bool includeType, bool includeLength, SnmpType? asType)
        {
            List<byte> encoded = new List<byte>();
            byte[] b = BitConverter.GetBytes(value);

            for (int i = 3; i >= 0; i--)
            {
                if (b[i] != 0 || encoded.Count > 0)
                    encoded.Add(b[i]);
            }
            // if (tmp.Length > 1 && tmp[0] == 0xff && (tmp[1] & 0x80) != 0)
            if (encoded.Count > 0 && (encoded[0] & 0x80) != 0)
            {
                encoded.Insert(0, 0);
            }
            else if (encoded.Count == 0)
                encoded.Add(0);
            if (includeLength) encoded.InsertRange(0, EncodeLength(encoded.Count));
            if (includeType) encoded.Insert(0, (byte)(asType ?? SnmpType.UInteger32));
            return encoded;
        }

        /// <summary>
        /// Decodes a ASN.1 UInteger32 from the given packet at the given position
        /// </summary>
        /// <param name="packet">The packet which contains the ASN.1 data</param>        
        /// <param name="position">The current position in the packet</param>              
        /// <returns>A System UInt32 decoded from the ASN.1 data</returns>
        public static UInt32 DecodeUInteger32(ref List<byte> packet, ref int position)
        {
            return DecodeUInteger32(ref packet, ref position, null);
        }

        /// <summary>
        /// Decodes a ASN.1 UInteger32 from the given packet at the given position
        /// </summary>
        /// <param name="packet">The packet which contains the ASN.1 data</param>        
        /// <param name="position">The current position in the packet</param>              
        /// <param name="tag">The optional SnmpType to Ensure is Preceeding the data</param>
        /// <returns>A System UInt32 decoded from the ASN.1 data</returns>
        public static UInt32 DecodeUInteger32(ref List<byte> packet, ref int position, SnmpType? tag)
        {
            if (!tag.HasValue) tag = packet[position].ToSnmpType();

            int Len = DecodeLength(ref packet, ref position, tag.Value);

            uint Val = 0;
            
            for (int i = 0; i < Len; ++i)
            {
                Val <<= 8;
                Val = Val | packet[position++];
            }

            return Val;
        }

        #endregion

        #region ASN.1 OctetString

        /// <summary>
        /// Encodes a ASN.1 OctetString
        /// </summary>
        /// <param name="value">The data to encode</param>
        /// <returns>A ASN.1 encoded OctetString</returns>
        public static List<byte> EncodeOctetString(byte[] value)
        {
            return EncodeOctetString(System.Text.Encoding.ASCII.GetString(value), true, true);
        }

        /// <summary>
        /// Encodes a ASN.1 OctetString
        /// </summary>
        /// <param name="value">The data to encode</param>
        /// <returns>A ASN.1 encoded OctetString</returns>
        public static List<byte> EncodeOctetString(IEnumerable<byte> value)
        {
            return EncodeOctetString(System.Text.Encoding.ASCII.GetString(value.ToArray()), true, true);
        }

        /// <summary>
        /// Encodes a ASN.1 OctetString
        /// </summary>
        /// <param name="value">The data to encode</param>
        /// <returns>A ASN.1 encoded OctetString</returns>
        public static List<byte> EncodeOctetString(List<byte> value)
        {
            return EncodeOctetString(System.Text.Encoding.ASCII.GetString(value.ToArray()), true, true);
        }

        /// <summary>
        /// Encodes a ASN.1 OctetString
        /// </summary>
        /// <param name="value">The data to encode</param>
        /// <returns>A ASN.1 encoded OctetString</returns>
        public static List<byte> EncodeOctetString(string value)
        {
            return EncodeOctetString(value, true, true);
        }

        /// <summary>
        /// Encodes a ASN.1 OctetString
        /// </summary>
        /// <param name="value">The data to encode</param>
        /// <param name="includeType">A value to indicate if the ASN.1 Type should be included in the ASN.1 Encoded Response</param>
        /// <param name="includeLength">A value to indicate if the ASN.1 Encoded Length should be included in the ASN.1 Encoded Response</param>
        /// <returns>The ASN.1 Encoded OctetString representation of the given value</returns>
        public static List<byte> EncodeOctetString(string value, bool includeType, bool includeLength)
        {
            return EncodeOctetString(value, includeType, includeLength, null);
        }

        /// <summary>
        /// Encodes a ASN.1 OctetString
        /// </summary>
        /// <param name="value">The data to encode</param>
        /// <param name="includeType">A value to indicate if the ASN.1 Type should be included in the ASN.1 Encoded Response</param>
        /// <param name="includeLength">A value to indicate if the ASN.1 Encoded Length should be included in the ASN.1 Encoded Response</param>
        /// <param name="tag">The SnmpType to represent as a ASN.1 OctetString</param>
        /// <returns>The ASN.1 Encoded OctetString representation of the given value</returns>
        public static List<byte> EncodeOctetString(string value, bool includeType, bool includeLength, SnmpType? tag)
        {
            List<byte> B = new List<byte>();
            if (includeType) B.Add((byte)(tag ?? SnmpType.OctetString));//Type
            if (includeLength) B.AddRange(EncodeLength(value.Length));//Length
            B.AddRange(System.Text.Encoding.ASCII.GetBytes(value));//Value
            return B;
        }

        /// <summary>
        /// Decodes a ASN.1 OctetString from the given packet at the given position
        /// </summary>
        /// <param name="packet">The packet which contains the ASN.1 data</param>        
        /// <param name="position">The current position in the packet</param>              
        /// <returns>The decoded ASN.1 data</returns>
        public static List<byte> DecodeOctetString(ref List<byte> packet, ref int position)
        {
            //int Len = DecodeLength(ref packet, ref position, (SnmpType)packet[position]);
            int Len = DecodeLength(ref packet, ref position, SnmpType.OctetString);
            List<byte> bytes = packet.GetRange(position, Len);
            position += Len;
            return bytes;
        }

        /// <summary>
        /// Decodes a ASN.1 OctetString from the given packet at the given position
        /// </summary>
        /// <param name="packet">The packet which contains the ASN.1 data</param>        
        /// <param name="position">The current position in the packet</param>      
        /// <param name="tag">The optional SnmpType to Ensure is Preceeding the data</param>
        /// <returns>The decoded ASN.1 data</returns>
        public static List<byte> DecodeOctetString(ref List<byte> packet, ref int position, SnmpType? tag)
        {
            int Len = DecodeLength(ref packet, ref position, tag ?? SnmpType.OctetString);
            List<byte> bytes = packet.GetRange(position, Len);
            position += Len;
            return bytes;
        }

        /// <summary>
        /// Converts a ASN.1 OctetString to a ASCII Encoded System.String
        /// </summary>
        /// <param name="OctetString">The ASN.1 OctetString value to decode</param>
        /// <returns>A ASCII encoded System.string</returns>
        public static string DecodeOctetString(List<byte> OctetString)
        {
            return System.Text.Encoding.ASCII.GetString(OctetString.ToArray());
        }

        #endregion

        #region ASN.1 ObjectIdentifier

        /// <summary>
        /// Encodes a Object Identifier TLV
        /// </summary>
        /// <param name="identifier">The String Identifier to Encode</param>
        /// <returns>A TLV of Object Identifier</returns>
        public static List<byte> EncodeObjectId(string identifier)
        {
            return EncodeObjectId(identifier, true, true);
        }

        /// <summary>
        /// Encodes a ASN.1 ObjectIdentifier
        /// </summary>
        /// <param name="identifier">The string representation of the ASN.1 ObjectIdentifier to Encode</param>
        /// <param name="includeType">A value to indicate if the ASN.1 Type should be included in the ASN.1 Encoded Response</param>
        /// <param name="includeLength">A value to indicate if the ASN.1 Encoded Length should be included in the ASN.1 Encoded Response</param>
        /// <returns>The ASN.1 Encoded ObjectIdentifier</returns>
        public static List<byte> EncodeObjectId(string identifier, bool includeType, bool includeLength)
        {

            if (identifier.StartsWith("1.3", StringComparison.OrdinalIgnoreCase)) identifier = "43" + identifier.Substring(3);
            if (identifier.EndsWith(".", StringComparison.OrdinalIgnoreCase)) identifier = identifier.Remove(identifier.Length - 1);
            List<byte> Bytes = new List<byte>();
            char[] sep = { '.' };
            string[] nodes = identifier.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            int Length = 0;
            foreach (string node in nodes)
            {
                int V = System.Convert.ToInt32(node, System.Globalization.CultureInfo.InvariantCulture);
                Bytes.Insert(Length, (byte)(V & 0x7F));// V - 127
                V >>= 7;
                while (V != 0)
                {
                    Bytes.Insert(Length, (byte)(V & 0x7F | 0x80)); //( V - 127 ) + 128
                    V >>= 7;
                }
                Length = Bytes.Count;
            }
            if (includeLength)
            {                
                Bytes.InsertRange(0, EncodeLength(Bytes.Count));
            }
            if (includeType) Bytes.Insert(0, (byte)SnmpType.ObjectIdentifier);
            return Bytes;
        }

        /// <summary>
        /// Decodes a ASN.1 ObjectIdentifier from a list of bytes
        /// </summary>
        /// <param name="packet">The Packet or Raw ProtocolDataUnit</param>
        /// <param name="position">The current position in the bytes</param>
        /// <returns>A Decoded ASN.1 Object Identifier as a String</returns>
        public static string DecodeObjectId(ref List<byte> packet, ref int position)
        {            
            int Len = DecodeLength(ref packet, ref position, SnmpType.ObjectIdentifier);
            String Result = string.Empty;
            try
            {
                int End = position + Len;
                for (int i = position; i < End; ++i)
                {
                    int Node = 0;
                    int N;
                    do
                    {
                        N = packet[i];
                        Node = (Node << 7) + (N & 0x7F); //N - 127
                        if (N > 127) i++; else break;
                    } while (i < End);
                    if (Result.ToString() != string.Empty) Result += ".";
                    Result += Node;
                }
                if (Result.ToString().StartsWith("43", StringComparison.OrdinalIgnoreCase)) Result = ("1.3" + Result.ToString().Substring(2));
                if (Result.ToString().EndsWith(".", StringComparison.OrdinalIgnoreCase)) Result = (Result.ToString().Remove(Result.Length - 1));
                position += Len;
                return Result;
            }
            catch
            {
                throw;
            }
            finally
            {
                Result = null;
            }
        }

        #endregion

        #region ASN.1 Null

        /// <summary>
        /// Encodes a ASN.1 Null Value
        /// </summary>
        /// <returns>The Bytes representing a ASN.1 Null Value</returns>
        public static byte[] EncodeNull()
        {
            return EncodeNull(false);
        }

        /// <summary>
        /// Encodes a ASN.1 Null Value
        /// </summary>
        /// <param name="includeType">A value indicating that SnmpType byte should preceed the Null value</param>
        /// <returns>The Bytes representing a ASN.1 Null Value</returns>
        public static byte[] EncodeNull(Boolean includeType)
        {
            byte[] Result;
            if (includeType) Result = new byte[] { (byte)SnmpType.Null, 0x00 };
            else Result = new byte[] { 0x00 };
            return Result;
        }

        /// <summary>
        /// Decodes a ASN.1 Encoded Null Value from a given byte list
        /// </summary>
        /// <param name="bytes">The Packet or Raw ProtocolDataUnit</param>
        /// <param name="position">The current position in the bytes</param>
        /// <returns>A Empty byte if the byte at the position is a ASN.1 Null Value</returns>
        public static byte[] DecodeNull(ref List<byte> bytes, ref int position)
        {
            if (bytes[position++] != (byte)SnmpType.Null) throw new System.Exception("ASN.1 Null expected.");
            position++;
            return new byte[1];
        }

        #endregion

        #region ASN.1 Sequence

        /// <summary>
        /// Create a Binary Sequence Header from the given parameters
        /// </summary>
        /// <param name="tag">The type of the sequence</param>
        /// <param name="length">The length of the sequence</param>
        /// <param name="includeType">Indicates to include the tag in the returned value</param>
        /// <returns>A ASN.1 Sequence Header for the given tag and length</returns>
        public static List<byte> EncodeSequenceHeader(SnmpType tag, int length, bool includeType)
        {
            List<byte> Sequence = new List<byte>();            
            if (includeType) Sequence.Add((byte)tag);
            Sequence.AddRange(EncodeLength(length));
            return Sequence;
        }

        /// <summary>
        /// Create a Binary Sequence from the given parameters
        /// </summary>
        /// <param name="tag">The type of the sequence</param>
        /// <param name="values">The bytes of the sequence</param>
        /// <param name="includeType">Indicates to include the tag in the returned value</param>
        /// <returns>A ASN.1 Encoded Sequence of the given tag and values</returns>
        public static List<byte> EncodeSequence(SnmpType tag, ref List<byte> values, bool includeType)
        {
            List<byte> Sequence = EncodeSequenceHeader(tag, values.Count, includeType);
            Sequence.AddRange(values);            
            return Sequence;
        }

        /// <summary>
        /// Encodes a Binary Sequence from a List of Variables
        /// </summary>
        /// <param name="Bindings">The Variable to create a Binary Sequence from</param>
        /// <returns>A Binary Sequence</returns>
        public static List<byte> EncodeSequence(List<Variable> Bindings)
        {
            List<byte> _Sequence = new List<byte>();
            Bindings.ForEach(vb => _Sequence.AddRange(vb.ToSequence()));
            List<byte> Sequence = new List<byte>();
            Sequence.AddRange(BasicEncodingRules.EncodeSequence(SnmpType.Sequence, ref _Sequence, true));            
            _Sequence = null;
            return Sequence;
        }

        /// <summary>
        /// Decodes a ASN.1 Sequence into Variable
        /// </summary>
        /// <param name="packet">The packet which contains the ASN.1 data</param>        
        /// <param name="position">The current position in the packet</param>      
        /// <returns>A Variable Constructed from the given packet at the given position</returns>
        public static Variable DecodeSequence(ref List<byte> packet, ref int position)
        {
            String Identifier = DecodeObjectId(ref packet, ref position);
            SnmpType TypeCode = (SnmpType)packet[position];
            int Length = DecodeLength(ref packet, ref position, TypeCode);
            List<byte> Value = packet.GetRange(position, Length);
            position += Length;
            return new Variable(Identifier, TypeCode, Value);
        }

        #endregion

        #region ASN.1 IpAddress

        public static List<byte> EncodeIpAddress(System.Net.IPAddress ipaddress,bool includeType, bool includeLength)
        {
            List<byte> encoded = new List<byte>();
            byte[] ipBytes = ipaddress.GetAddressBytes();
            encoded.AddRange(ipBytes);
            if (includeType) encoded.Add((byte)SnmpType.IPAddress);
            if (includeLength) encoded.Add((byte)ipBytes.Length);
            return encoded;
        }

        public static System.Net.IPAddress DecodeIpAddress(ref List<byte> packet, ref int position)
        {
            int Len = DecodeLength(ref packet, ref position, SnmpType.IPAddress);
            byte[] value = new byte[Len];
            for (int b = 0, e = value.Length; b < e; b++)
            {
                value[b] = packet[position++];
            }
            return new System.Net.IPAddress(value);
        }

        #endregion

        #endregion
    }
}
