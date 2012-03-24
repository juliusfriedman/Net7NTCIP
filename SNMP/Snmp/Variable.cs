using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.Snmp
{
    /// <summary>
    /// A Manged Representation of a Snmp Variable
    /// </summary>
    [Serializable()]
    public class Variable
    {
        #region Properties

        /// <summary>
        /// The Identifier of this Variable
        /// </summary>
        public String Identifier { get; set; }

        /// <summary>
        /// The SnmpType of this VariableBinding
        /// </summary>
        public SnmpType TypeCode { get; set; }

        /// <summary>
        /// The Length of the List of Bytes of this Value
        /// </summary>
        public Int32 Length
        {
            get
            {
                Value.TrimExcess();
                return Value.Count;
            }
            set
            {
                Value = new List<byte>(value);
            }
        }

        /// <summary>
        /// The BasicEncodingRules BaseType of the Variable
        /// </summary>
        public BasicEncodingRules.BaseTypes BaseType
        {
            get
            {
                int v = (int)TypeCode & (int)BasicEncodingRules.BaseTypes.Universal;
                v &= (int)BasicEncodingRules.BaseTypes.Application;
                return (BasicEncodingRules.BaseTypes)v;
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// The decoded bytes of the value of this variable binding
        /// </summary>
        public List<byte> Value;

        #endregion

        #region Methods

        /// <summary>
        /// Creates a List of Nodes in the Identifier
        /// </summary>
        /// <returns>A List of Int32 with all nodes contained in the identifier</returns>
        public List<Int32> GetIdentifierNodes()
        {
            try
            {
                return new List<int>(Array.ConvertAll<string, int>(Identifier.Split('.'), Convert.ToInt32));
            }
            catch
            {
                return new List<int>();
            }
        }

        /// <summary>Hash value for Identifier value
        /// </summary>
        /// <returns> The hash code for the object.</returns>
        public override int GetHashCode()
        {
            int hash = base.GetHashCode();
            GetIdentifierNodes().ForEach(b => hash ^= b);
            return hash;
        }

        /// <summary>
        /// Creates a Type, Length, Value (TLV) from this Variable instance
        /// </summary>
        /// <returns>The bytes which represent this Variable as a TLV</returns>
        public List<byte> ToBytes()
        {
            List<byte> TLV = new List<byte>();

            TLV.Add((byte)TypeCode);
            TLV.AddRange(BasicEncodingRules.EncodeLength(Length));
            TLV.AddRange(Value);

            return TLV;
        }
        
        /// <summary>
        /// Creates a Byte Sequence From this Variable
        /// </summary>
        /// <returns>The bytes which represent this variable as a Sequence</returns>
        public List<byte> ToSequence()
        {
            return ToSequence(null);
        }

        /// <summary>
        /// Creates a Byte Sequence From this Variable
        /// </summary>
        /// <param name="SequenceType">The Type of the Sequence</param>
        /// <returns>The bytes which represent this variable as a Sequence</returns>
        public List<byte> ToSequence(SnmpType? SequenceType)
        {
            SequenceType = SequenceType ?? SnmpType.Sequence;
            List<byte> _Sequence = new List<byte>();
            _Sequence.AddRange(BasicEncodingRules.EncodeObjectId(Identifier));
            _Sequence.AddRange(this.ToBytes());
            List<byte> Sequence = BasicEncodingRules.EncodeSequence(SequenceType.Value, ref _Sequence, true);
            _Sequence = null;
            return Sequence;
        }

        /// <summary>
        /// Converts a ASN.1 Encoded Variable to a System Integer
        /// </summary>
        /// <returns>A System Integer from the Encoded Value</returns>
        public Int32 ToInt32()
        {
            List<byte> raw = ToBytes();
            int position = 0;
            return BasicEncodingRules.DecodeInteger32(ref raw, ref position);
        }

        /// <summary>
        /// Converts a ASN.1 Encoded Variable to a System UInt32
        /// </summary>
        /// <returns>A System UInt32 from the Encoded Value</returns>
        public UInt32 ToUInt32()
        {
            List<byte> raw = ToBytes();
            int position = 0;
            return BasicEncodingRules.DecodeUInteger32(ref raw, ref position);
        }

        /// <summary>
        /// Converts a ASN.1 Encoded Variable to a System String
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            List<byte> raw = ToBytes();
            int position = 0;
            return BasicEncodingRules.DecodeOctetString(BasicEncodingRules.DecodeOctetString(ref raw, ref position));
        }

        /// <summary>
        /// Converts a ASN.1 Encoded DateTime to a System DateTime
        /// </summary>
        /// <returns></returns>
        public DateTime ToDateTime()
        {
            try
            {
                List<byte> raw = ToBytes();
                int position = 0;
                uint value = BasicEncodingRules.DecodeUInteger32(ref raw, ref position, TypeCode);
                //use 1, 1, 0 until when we figure out why im 2 hrs off.
                return (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddSeconds(value);

            }
            catch
            {
                //Maybe return Not a Date Time somehow?
                throw;
            }
        }

        /// <summary>
        /// Converts a ASN.1 Encoded TimeTicks to a System TimeSpan
        /// </summary>
        /// <returns></returns>
        public TimeSpan ToTimeSpan()
        {
            return new TimeSpan(ToUInt32());
        }

        /// <summary>
        /// Converts a ASN.1 Encoded IpAddress to a System.Net.IPAddress
        /// </summary>
        /// <returns></returns>
        public System.Net.IPAddress ToIPAddress()
        {
            return System.Net.IPAddress.Parse(string.Join(".", Array.ConvertAll<int, string>(Array.ConvertAll<byte, int>(Value.ToArray(), Convert.ToInt32), Convert.ToString)));
        }

        /// <summary>
        /// Converts a ASN.1 ObjectIdentifier to a System String
        /// </summary>
        /// <returns></returns>
        public string ToObjectIdentifier()
        {
            int pos = 0;
            List<byte> tlv = ToBytes();
            return BasicEncodingRules.DecodeObjectId(ref tlv, ref pos);
        }

        /// <summary>
        /// Converts a ASN.1 Encoded Variable to a System Boolean
        /// </summary>
        /// <returns></returns>
        public Boolean ToBoolean()
        {
            try
            {
                return Convert.ToBoolean(ToInt32());
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Creates a Varaible from a Array of bytes
        /// </summary>
        /// <param name="bytes">The Array of bytes</param>
        /// <returns>A Variable constructs from the given bytes</returns>
        public static Variable FromBytes(byte[] bytes)
        {
            return Variable.FromBytes(new List<byte>(bytes));
        }

        /// <summary>
        /// Creates a Varaible from a List of bytes
        /// </summary>
        /// <param name="bytes">The List of bytes</param>
        /// <returns>A Variable constructs from the given bytes</returns>
        public static Variable FromBytes(List<byte> bytes)
        {
            int position = 0;
            Variable result = new Variable();
            if (bytes[position].ToSnmpType() == SnmpType.ObjectIdentifier)
            {
                result.Identifier = BasicEncodingRules.DecodeObjectId(ref bytes, ref position);
            }
            result.TypeCode = bytes[position].ToSnmpType();
            int Length = BasicEncodingRules.DecodeLength(ref bytes, ref position, result.TypeCode);
            result.Value = bytes.GetRange(position, Length);
            return result;
        }

        /// <summary>
        /// Creates a Integer 32 Variable
        /// </summary>
        /// <param name="value">The System.Integer32 Value of the Variable</param>
        /// <returns>A Integer Variable with the given value</returns>
        public static Variable CreateInteger32(int value)
        {
            return Variable.FromBytes(BasicEncodingRules.EncodeInteger32(value));
        }

        /// <summary>
        /// Creates a UInteger 32 Variable
        /// </summary>
        /// <param name="value">The System.UInteger32 Value of the Variable</param>
        /// <returns>A UInteger 32 Variable with the given value</returns>
        public static Variable CreateUInteger32(uint value)
        {
            return Variable.FromBytes(BasicEncodingRules.EncodeUInteger32(value));
        }

        /// <summary>
        /// Creates a OctetString Variable
        /// </summary>
        /// <param name="value">The System.string Value of the Variable</param>
        /// <returns>A OctetString Variable with the given value</returns>
        public static Variable CreateOctetString(string value)
        {
            return Variable.FromBytes(BasicEncodingRules.EncodeOctetString(value));
        }

        /// <summary>
        /// Creates a Null Variable
        /// </summary>
        /// <returns>A Null Variable</returns>
        public static Variable CreateNull()
        {
            return Variable.FromBytes(BasicEncodingRules.EncodeNull(true));
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Variable()
        {
            Value = new List<byte>();
        }

        /// <summary>
        /// Construct a Variable Binding with a Known Identifier
        /// </summary>
        /// <param name="Identifier">The Object Identifier of the Variable Binding</param>
        public Variable(String Identifier)
            : this()
        {
            this.Identifier = Identifier;
            this.TypeCode = SnmpType.Null;
        }

        /// <summary>
        /// Constructs a Variable Binding from a known SnmpType
        /// </summary>
        /// <param name="TypeCode">The SnmpType of this Variable Binding</param>
        public Variable(SnmpType TypeCode)
            : this()
        {
            this.TypeCode = TypeCode;
        }

        /// <summary>
        /// Constructs a Variable Binding from a known SnmpType with a Value
        /// </summary>
        /// <param name="Identifier">The Object Identifier of the Variable Binding</param>
        /// <param name="TypeCode">The SnmpType of the Variable Binding</param>
        /// <param name="Value">The byte Value of the Variable Binding</param>
        public Variable(String Identifier, SnmpType TypeCode, byte[] Value)
            : this(Identifier, TypeCode, new List<byte>(Value))
        {            
        }

        /// <summary>
        /// Constructs a Variable Binding from a known SnmpType with a Value
        /// </summary>
        /// <param name="Identifier">The Object Identifier of the Variable Binding</param>
        /// <param name="TypeCode">The SnmpType of the Variable Binding</param>
        /// <param name="Value">The byte Value of the Variable Binding</param>
        public Variable(String Identifier, SnmpType TypeCode, List<byte> Value)
            : this()
        {
            this.Identifier = Identifier;
            this.TypeCode = TypeCode;
            this.Value = Value;
        }

        /// <summary>
        /// Creates a Variable from the given values
        /// </summary>
        /// <param name="values">The raw data of a variable</param>
        public Variable(byte[] values)
        {
            try
            {
                Variable created = Variable.FromBytes(values);
                this.Identifier = created.Identifier;
                this.TypeCode = created.TypeCode;
                this.Value = created.Value;
                created = null;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Creates a Variable from the given values
        /// </summary>
        /// <param name="values">The raw data of a variable</param>
        public Variable(List<byte> values)
            :this(values.ToArray())
        {            
        }

        /// <summary>
        /// Constructs a new Variable from a Varible
        /// </summary>
        /// <param name="v">A Variable to copy</param>
        public Variable(Variable v)
        {
            this.Identifier = v.Identifier;
            this.TypeCode = v.TypeCode;
            this.Value = v.Value;
        }

        #endregion

        #region Destructor

        /// <summary>
        /// Destructor
        /// </summary>
        ~Variable()
        {
            this.Value.Clear();
            this.Value = null;
            this.Identifier = null;
        }

        #endregion

    }
}
