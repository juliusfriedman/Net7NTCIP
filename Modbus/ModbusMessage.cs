using System;
using System.Collections.Generic;

namespace ASTITransportation.Modbus
{

    #region Enums

    public enum FunctionCode : byte
    {
        Unknown = 0,
        IllegalFunctionCode,
        IllegalDataAddress,
        IllegalDataValue,
        ServerFailure,
        Acknowledge,
        ServerBusy,
        GatewayPathProblem = 0x0a,
        GatewayFailedToRespond = 0x0b
    };

    public enum MessageType : byte
    {
        ReadCoilsInputs,
        ReadHoldingInputRegisters,
        ReadWriteMultipleRegisters,
        SlaveException,
        WriteMultipleCoils,
        WriteMultipleRegisters,
        WriteSingleCoil,
        WriteSingleRegister
    };

    #endregion

    public class ModbusMessage
    {
        #region Statics

        static byte ASCIIHeader = System.Text.Encoding.ASCII.GetBytes(":")[0];

        static byte[] crc_table = new byte[]{
            0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,
            0x40,0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,0x00,0xC1,0x81,0x40,0x01,0xC0,
            0x80,0x41,0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,0x00,0xC1,0x81,0x40,0x01,
            0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,0x01,0xC0,0x80,0x41,
            0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,0x00,0xC1,0x81,
            0x40,0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,0x01,0xC0,
            0x80,0x41,0x00,0xC1,0x81,0x40,0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,0x01,
            0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,
            0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,
            0x40,0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,0x01,0xC0,
            0x80,0x41,0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,0x00,0xC1,0x81,0x40,0x01,
            0xC0,0x80,0x41,0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,
            0x00,0xC1,0x81,0x40,0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,
            0x40,0x01,0xC0,0x80,0x41,0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,0x01,0xC0,
            0x80,0x41,0x00,0xC1,0x81,0x40,0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,0x01,
            0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,
            0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,
            0x40,0x00,0xC0,0xC1,0x01,0xC3,0x03,0x02,0xC2,0xC6,0x06,0x07,0xC7,0x05,0xC5,
            0xC4,0x04,0xCC,0x0C,0x0D,0xCD,0x0F,0xCF,0xCE,0x0E,0x0A,0xCA,0xCB,0x0B,0xC9,
            0x09,0x08,0xC8,0xD8,0x18,0x19,0xD9,0x1B,0xDB,0xDA,0x1A,0x1E,0xDE,0xDF,0x1F,
            0xDD,0x1D,0x1C,0xDC,0x14,0xD4,0xD5,0x15,0xD7,0x17,0x16,0xD6,0xD2,0x12,0x13,
            0xD3,0x11,0xD1,0xD0,0x10,0xF0,0x30,0x31,0xF1,0x33,0xF3,0xF2,0x32,0x36,0xF6,
            0xF7,0x37,0xF5,0x35,0x34,0xF4,0x3C,0xFC,0xFD,0x3D,0xFF,0x3F,0x3E,0xFE,0xFA,
            0x3A,0x3B,0xFB,0x39,0xF9,0xF8,0x38,0x28,0xE8,0xE9,0x29,0xEB,0x2B,0x2A,0xEA,
            0xEE,0x2E,0x2F,0xEF,0x2D,0xED,0xEC,0x2C,0xE4,0x24,0x25,0xE5,0x27,0xE7,0xE6,
            0x26,0x22,0xE2,0xE3,0x23,0xE1,0x21,0x20,0xE0,0xA0,0x60,0x61,0xA1,0x63,0xA3,
            0xA2,0x62,0x66,0xA6,0xA7,0x67,0xA5,0x65,0x64,0xA4,0x6C,0xAC,0xAD,0x6D,0xAF,
            0x6F,0x6E,0xAE,0xAA,0x6A,0x6B,0xAB,0x69,0xA9,0xA8,0x68,0x78,0xB8,0xB9,0x79,
            0xBB,0x7B,0x7A,0xBA,0xBE,0x7E,0x7F,0xBF,0x7D,0xBD,0xBC,0x7C,0xB4,0x74,0x75,
            0xB5,0x77,0xB7,0xB6,0x76,0x72,0xB2,0xB3,0x73,0xB1,0x71,0x70,0xB0,0x50,0x90,
            0x91,0x51,0x93,0x53,0x52,0x92,0x96,0x56,0x57,0x97,0x55,0x95,0x94,0x54,0x9C,
            0x5C,0x5D,0x9D,0x5F,0x9F,0x9E,0x5E,0x5A,0x9A,0x9B,0x5B,0x99,0x59,0x58,0x98,
            0x88,0x48,0x49,0x89,0x4B,0x8B,0x8A,0x4A,0x4E,0x8E,0x8F,0x4F,0x8D,0x4D,0x4C,
            0x8C,0x44,0x84,0x85,0x45,0x87,0x47,0x46,0x86,0x82,0x42,0x43,0x83,0x41,0x81,
            0x80,0x40
        };

        /// <summary>
        /// Calculates the 16bit Cyclic Redundancy Check for a given payload
        /// </summary>
        /// <param name="payload">The data used in CRC</param>
        /// <returns>CRC value</returns>
        public static byte[] CalculateCrc16(byte[] payload)
        {
            byte[] r = BitConverter.GetBytes(crc16(payload, payload.Length));
            return new byte[] { r[1], r[0] };
        }

        /// <summary>
        /// Calculate Longitudinal Redundancy Check for a given payload.
        /// </summary>
        /// <param name="data">The data used in LRC</param>
        /// <returns>LRC value</returns>
        public static byte CalculateLrc(byte[] payload)
        {
            if (payload == null)
                throw new ArgumentNullException("data");

            byte lrc = 0;
            foreach (byte b in payload)
                lrc += b;

            lrc = (byte)((lrc ^ 0xFF) + 1);

            return lrc;
        }

        static int crc16(byte[] modbusframe, int Length)
        {
            int i;
            int index;
            int crc_Low = 0xFF;
            int crc_High = 0xFF;

            for (i = 0; i < Length; ++i)
            {
                index = crc_High ^ (char)modbusframe[i];
                crc_High = crc_Low ^ crc_table[index];
                crc_Low = (byte)crc_table[index + 256];
            }

            return crc_High * 256 + crc_Low;
        } 

        #endregion

        #region Fields

        short? transactionId;

        short? protocolId;        

        byte? unitId = 0x00;//0xff

        byte functionCode;

        short? startingAddress;

        short? registerQuantity;

        List<byte> dataPoints = new List<byte>();

        bool hasResponse = true;

        #endregion

        #region Properties

        public Int16? TransactionId
        {
            get { return transactionId; }
            set { transactionId = value; }
        }

        public Int16? ProtocolId
        {
            get { return protocolId; }
            set { protocolId = value; }
        }

        public byte? UnitId
        {
            get { return unitId; }
            set { unitId = value; }
        }

        public byte FunctionCode
        {
            get { return functionCode; }
            set { functionCode = value; }
        }
      
        public Int16? StartingAddress
        {
            get { return startingAddress; }
            set { startingAddress = value; }
        }

        public Int16? RegisterQuantity
        {
            get { return registerQuantity; }
            set { registerQuantity = value; }
        }

        public List<byte> DataPoints
        {
            get { return dataPoints; }
            set { dataPoints = value; }
        }

        public Boolean HasResponse { 
            get { return hasResponse; } 
            set { hasResponse = value; } 
        }

        #endregion

        #region Methods

        public List<byte> ToAdu()
        {
            return ToAdu(false);
        }

        /// <summary>
        /// Builds a Modbus ApplicationDataUnit from this ModbusMessage
        /// </summary>
        /// <returns>A list of byte which represets the ApplicationDataUnit</returns>
        public List<byte> ToAdu(bool ascii)
        {
            List<byte> result = new List<byte>();

            List<byte> pdu = ToPdu(ascii);

            if(unitId.HasValue)
                pdu.Insert(0, unitId.Value);

            if(transactionId.HasValue)
                result.AddRange(BitConverter.GetBytes(transactionId.Value));

            if(protocolId.HasValue)
                result.AddRange(BitConverter.GetBytes(protocolId.Value));
            
            result.AddRange(BitConverter.GetBytes(((short)(pdu.Count))));            
                       
            result.AddRange(pdu);            

            return result;
        }

        public List<byte> ToPdu()
        {
            return ToPdu(false);
        }

        /// <summary>
        /// Builds a Modbus ProtocolDataUnit from this ModbusMessage
        /// </summary>
        /// <returns>A list of byte which represets the ProtcolDataUnit</returns>
        public List<byte> ToPdu(bool ascii)
        {
            List<byte> result = new List<byte>();

            if (ascii)
                result.Insert(0, ASCIIHeader);

            result.Add(functionCode);

            if(startingAddress.HasValue)
            result.AddRange(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(startingAddress.Value)));

            if(registerQuantity.HasValue)
            result.AddRange(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(registerQuantity.Value)));

            if (dataPoints.Count > 0) result.AddRange(dataPoints);

            return result;
        }        

        #endregion

        #region Constructor

        public ModbusMessage(byte[] data)
        {
            if (data.Length >= 14 && BitConverter.ToInt16(data, 2) == 0)
            {
                transactionId = BitConverter.ToInt16(data, 0);
                protocolId = BitConverter.ToInt16(data, 2);               
                Int16 pduSize = ((short)(BitConverter.ToInt16(data, 4) - 1));
                unitId = data[6];
                functionCode = data[7];
                int byteCount = data[8];
                int pos = 9;
                while (pos < data.Length - 2)
                {
                    dataPoints.AddRange(new byte[] { data[pos], data[pos + 1] });
                    byteCount -= 2;
                    pos += 2;
                }
            }
            else
            {                  
                unitId = data[0];
                functionCode = data[1];
                int byteCount;
                int pos;
                if (functionCode != 43)
                {
                    byteCount = data[2];
                    pos = 3;
                    //use existing while
                }
                else
                {
                    byteCount = data[9];
                    pos = 10;

                    //while pos < byteCount
                    //decode length and value of dataPoint

                }

                
                while (pos < data.Length - 2)
                {
                    dataPoints.AddRange(new byte[] { data[pos], data[pos + 1] });
                    byteCount -= 2;
                    pos += 2;
                }
            }
        }

        public ModbusMessage()
        {
        }

        #endregion

    }
}
