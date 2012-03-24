using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;

namespace EIS
{
    #region X3 Protocol

    public class X3Header
    {
        #region Fields

        static int HeaderLength = 3;
        static byte HeaderMark = 255;
        byte qualifier;
        int payloadSize;

        #endregion

        #region Properties

        public byte Qualifier
        {
            get { return qualifier; }
            set { qualifier = value; }
        }

        public int PayloadSize
        {
            get { return payloadSize; }
            set { payloadSize = value; }
        }

        public int Length
        {
            get { return HeaderLength; }
        }

        #endregion

        #region Methods

        public byte[] ToBytes()
        {
            return new byte[] { HeaderMark, Qualifier, (byte)PayloadSize };
        }

        #endregion

        #region Constructor

        public X3Header()
        {
        }

        public X3Header(byte[] data)
        {
            if (data[0] != HeaderMark) throw new Exception("Invalid Packet");
            qualifier = data[1];
            payloadSize = data[2];
        }

        #endregion
    }

    public class X3Payload
    {
        #region Fields

        byte[] data = new byte[0];

        #endregion

        #region Properties

        public int Length
        {
            get { return data.Length; }
            set { Array.Resize<byte>(ref data, value); }
        }

        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

        #endregion

        #region Constructor

        public X3Payload(int payloadSize)
        {
            this.Length = payloadSize;
        }

        public X3Payload(byte[] bytes)
        {
            data = bytes;
        }

        #endregion
    }

    public class X3Packet
    {
        #region Fields

        X3Header header;
        X3Payload payload;
        bool hasResponse;
        bool hasMultipleResponse;
        int addedTimeout;
        bool valid = true;
        //                                                  H     Q     L  P     C
        protected static byte[] FlushCommand = new byte[] { 0xff, 0x9f, 1, 0xff, 0xff };

        #endregion

        #region Properties

        public X3Header Header
        {
            get { return header; }
            set { header = value; }
        }

        public X3Payload Payload
        {
            get { return payload; }
            set
            {
                payload = value;
                header.PayloadSize = payload.Length;
            }
        }

        public bool HasResponse
        {
            get { return hasResponse; }
            set
            {
                if (value == false)
                {
                    hasResponse = hasMultipleResponse = false;
                }
                else hasResponse = true;
            }
        }

        public bool HasMultipleResponse
        {
            get { return hasMultipleResponse; }
            set
            {
                if (value == true)
                {
                    hasResponse = hasMultipleResponse = true;
                }
                else hasMultipleResponse = false;
            }

        }

        public int AddedTimeout
        {
            get { return addedTimeout; }
            set { addedTimeout = value; }
        }

        public int Length
        {
            get
            {
                if (valid) return header.Length + payload.Length + 1;
                return header.Length + payload.Length - 1;
            }
        }

        public bool Valid
        {
            get { return valid; }
        }

        #endregion

        #region Methods

        public virtual byte[] ToBytes()
        {
            byte[] result = new byte[this.Length];
            header.PayloadSize = payload.Length;
            header.ToBytes().CopyTo(result, 0);
            payload.Data.CopyTo(result, header.Length);
            result[result.Length - 1] = ComputeChecksum();
            return result;
        }

        protected virtual byte ComputeChecksum()
        {
            byte crcValue = 0;
            foreach (byte b in payload.Data) crcValue += b;
            return crcValue;
        }

        #endregion

        #region Constructor

        public X3Packet()
        {
            this.header = new X3Header();
            this.payload = new X3Payload(0);
        }

        public X3Packet(byte[] data)
        {
            if (null == data) return;
            //try catch call and if catch remove while not 0xff
            this.header = new X3Header(data);
            this.payload = new X3Payload(header.PayloadSize);
            try
            {
                Array.Copy(data, header.Length, payload.Data, 0, header.PayloadSize);
                if (ComputeChecksum() != data[this.header.Length + this.header.PayloadSize]) valid = false;
                else valid = true;
            }
            catch (ArgumentException)
            {
                valid = false;
                header.PayloadSize = data.Length - header.Length;
                payload.Length = header.PayloadSize;
                Array.Copy(data, header.Length, payload.Data, 0, header.PayloadSize);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public X3Packet(X3Packet other)
        {
            this.header = other.header;
            this.payload = other.payload;
            this.hasResponse = other.hasResponse;
            this.hasMultipleResponse = other.hasMultipleResponse;
            this.addedTimeout = other.addedTimeout;
        }

        #endregion
    }

    #endregion

    #region PC To RTMS Messages

    /// <summary>
    /// Data poll
    /// </summary>
    public class DataRequest : X3Packet
    {

        static byte qualifier = 0x8f;

        public byte Qualifier
        {
            get { return DataRequest.qualifier; }
        }

        public DataRequest(int sensorId)
            : base()
        {
            base.Header.Qualifier = qualifier;
            base.Payload.Data = new byte[] { (byte)sensorId };
            base.HasMultipleResponse = true;
        }

        public DataRequest(List<X3Packet> packets)
            : base(packets.Where(p => p.Header.Qualifier == qualifier).FirstOrDefault()) { }

        public override byte[] ToBytes()
        {
            byte[] cmd = base.ToBytes();
            byte[] result = new byte[cmd.Length + FlushCommand.Length];
            cmd.CopyTo(result, 0);
            FlushCommand.CopyTo(result, cmd.Length);
            return result;
        }
    }

    /// <summary>
    /// Clears the RTMS buffer
    /// </summary>
    public class BufferFlush : X3Packet
    {
        static byte qualifier = 0x1f;

        public byte Qualifier
        {
            get { return BufferFlush.qualifier; }
        }

        public BufferFlush(int sensorId)
            : base()
        {
            base.Header.Qualifier = qualifier;
            base.Payload.Data = new byte[] { (byte)sensorId };
            base.HasResponse = false;
        }
    }

    /// <summary>
    /// Self-Test request in Polled Mode
    /// </summary>
    public class PolledBitRequest : X3Packet
    {

        static byte qualifier = 0x90;

        public byte Qualifier
        {
            get { return PolledBitRequest.qualifier; }
        }

        public PolledBitRequest(int sensorId)
            : base()
        {
            base.Header.Qualifier = qualifier;
            base.Payload.Data = new byte[] { (byte)sensorId };
            base.HasResponse = true;
        }

        public override byte[] ToBytes()
        {
            byte[] cmd = base.ToBytes();
            byte[] result = new byte[cmd.Length + FlushCommand.Length];
            cmd.CopyTo(result, 0);
            FlushCommand.CopyTo(result, cmd.Length);
            return result;
        }
    }

    /// <summary>
    /// Requests current setup of power management
    /// </summary>
    public class PowerMangementVectorRequest : X3Packet
    {

        static byte qualifier = 0x96;

        public byte Qualifier
        {
            get { return PowerMangementVectorRequest.qualifier; }
        }

        public PowerMangementVectorRequest(int sensorId)
            : base()
        {
            base.Header.Qualifier = qualifier;
            base.Payload.Data = new byte[] { (byte)sensorId };
            base.HasResponse = true;
        }

        public override byte[] ToBytes()
        {
            byte[] cmd = base.ToBytes();
            byte[] result = new byte[cmd.Length + FlushCommand.Length];
            cmd.CopyTo(result, 0);
            FlushCommand.CopyTo(result, cmd.Length);
            return result;
        }
    }

    /// <summary>
    /// Requests the Speed Bins Configuration from the sensor in Polled Mode
    /// </summary>
    public class PolledBinsRequest : X3Packet
    {

        static byte qualifier = 0x8b;

        public byte Qualifier
        {
            get { return PolledBinsRequest.qualifier; }
        }

        public PolledBinsRequest(int sensorId)
            : base()
        {
            base.HasResponse = true;
            base.Header.Qualifier = qualifier;
            base.Payload.Data = new byte[] { (byte)sensorId };
        }

        public override byte[] ToBytes()
        {
            byte[] cmd = base.ToBytes();
            byte[] result = new byte[cmd.Length + FlushCommand.Length];
            cmd.CopyTo(result, 0);
            FlushCommand.CopyTo(result, cmd.Length);
            return result;
        }
    }

    /// <summary>
    /// Requests the Sensor to change its baud rate
    /// </summary>
    public class SetBaudRateRequest : X3Packet
    {

        static byte qualifier = 0xac;

        public byte Qualifier
        {
            get { return SetBaudRateRequest.qualifier; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sensorID">The SensorId who should recieve this message</param>
        /// <param name="baudRate">
        /// The new baudrate indication:
        ///     1 2400bps
        ///     2 4800bps
        ///     3 9600bps
        ///     4 14400bps
        ///     5 19200bps
        ///     6 38400bps
        ///     7 57600bps
        ///     8 115200bps
        /// </param>
        public SetBaudRateRequest(int sensorId, int baudRate)
            : base()
        {
            base.Header.Qualifier = qualifier;
            base.Payload.Data = new byte[] { (byte)sensorId, (byte)baudRate };
            base.HasResponse = false;
        }
    }

    /// <summary>
    /// Requests the Sensor to save a configuration block to flash
    /// </summary>
    public class SaveBlockRequest : X3Packet
    {

        static byte qualifier = 0x85;

        public byte Qualifier
        {
            get { return SaveBlockRequest.qualifier; }
        }


        public SaveBlockRequest(int sensorId, int blockNumber, int blockValue)
            : base()
        {
            base.Header.Qualifier = qualifier;
            base.Payload.Data = new byte[] { (byte)sensorId, (byte)blockNumber, (byte)blockValue };
            base.HasResponse = true;
        }
    }
    /// <summary>
    /// Requests the software information from a sensor in Polled Mode
    /// </summary>
    public class PolledSoftwareInformationRequest : X3Packet
    {

        static byte qualifier = 0x8c;

        public byte Qualifier
        {
            get { return PolledSoftwareInformationRequest.qualifier; }
        }

        public PolledSoftwareInformationRequest(int sensorId)
            : base()
        {
            base.HasResponse = true;
            base.Header.Qualifier = qualifier;
            base.Payload.Data = new byte[] { (byte)sensorId };
        }

        public override byte[] ToBytes()
        {
            byte[] cmd = base.ToBytes();
            byte[] result = new byte[cmd.Length + FlushCommand.Length];
            cmd.CopyTo(result, 0);
            FlushCommand.CopyTo(result, cmd.Length);
            return result;
        }
    }

    /// <summary>
    /// Requests the interval information from the sensor in Polled Mode
    /// </summary>
    public class PolledIntervalInformationRequest : X3Packet
    {

        static byte qualifier = 0x8e;

        public byte Qualifier
        {
            get { return PolledIntervalInformationRequest.qualifier; }
        }

        public PolledIntervalInformationRequest(int sensorId)
            : base()
        {
            base.HasResponse = true;
            base.Header.Qualifier = qualifier;
            base.Payload.Data = new byte[] { (byte)sensorId };
        }

        public override byte[] ToBytes()
        {
            byte[] cmd = base.ToBytes();
            byte[] result = new byte[cmd.Length + FlushCommand.Length];
            cmd.CopyTo(result, 0);
            FlushCommand.CopyTo(result, cmd.Length);
            return result;
        }
    }

    #endregion

    #region RTMS To PC Messages

    public class LongVehicleVolume : X3Packet
    {

        static byte qualifier = 0x1b;

        public byte Qualifier
        {
            get { return LongVehicleVolume.qualifier; }
        }

        public LongVehicleVolume(byte[] data)
            : base(data)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Long Vehicle Volume Packet");
        }

        public LongVehicleVolume(X3Packet packet)
            : base(packet)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Long Vehicle Volume Packet");
        }

        public LongVehicleVolume(List<X3Packet> packets)
            : base(packets.Where(p => p.Header.Qualifier == qualifier).FirstOrDefault()) { }


        private int LaneVolume(int laneNumber)
        {
            if (laneNumber > 8) throw new Exception("Only 8 Lanes are Supported");
            if (laneNumber < 0) throw new Exception("Invalid LaneNumber");
            if (laneNumber > 0) laneNumber--;
            return base.Payload.Data[laneNumber];
        }

        public int Lane_1_Volume
        {
            get { return LaneVolume(1); }
        }

        public int Lane_2_Volume
        {
            get { return LaneVolume(2); }
        }

        public int Lane_3_Volume
        {
            get { return LaneVolume(3); }
        }

        public int Lane_4_Volume
        {
            get { return LaneVolume(4); }
        }

        public int Lane_5_Volume
        {
            get { return LaneVolume(5); }
        }

        public int Lane_6_Volume
        {
            get { return LaneVolume(6); }
        }

        public int Lane_7_Volume
        {
            get { return LaneVolume(7); }
        }

        public int Lane_8_Volume
        {
            get { return LaneVolume(8); }
        }

        public int Voltage
        {
            get
            {
                return base.Payload.Data[8];
            }
        }

        public float Voltage_F
        {
            get
            {
                return (float)(Voltage * 100) / 1000;
            }
        }
    }

    public class VehicleVolume : X3Packet
    {

        static byte qualifier = 0x10;

        public byte Qualifier
        {
            get { return VehicleVolume.qualifier; }
        }

        public VehicleVolume(byte[] data)
            : base(data)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Vehicle Volume Packet");
        }

        public VehicleVolume(X3Packet packet)
            : base(packet)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Vehicle Volume Packet");
        }

        public VehicleVolume(List<X3Packet> packets)
            : base(packets.Where(p => p.Header.Qualifier == qualifier).FirstOrDefault()) { }

        private int LaneVolume(int laneNumber)
        {
            if (laneNumber > 8) throw new Exception("Only 8 Lanes are Supported");
            if (laneNumber < 0) throw new Exception("Invalid LaneNumber");
            if (laneNumber > 0) laneNumber--;
            return base.Payload.Data[laneNumber];
        }

        public int Lane_1_Volume
        {
            get { return LaneVolume(1); }
        }

        public int Lane_2_Volume
        {
            get { return LaneVolume(2); }
        }

        public int Lane_3_Volume
        {
            get { return LaneVolume(3); }
        }

        public int Lane_4_Volume
        {
            get { return LaneVolume(4); }
        }

        public int Lane_5_Volume
        {
            get { return LaneVolume(5); }
        }

        public int Lane_6_Volume
        {
            get { return LaneVolume(6); }
        }

        public int Lane_7_Volume
        {
            get { return LaneVolume(7); }
        }

        public int Lane_8_Volume
        {
            get { return LaneVolume(8); }
        }

        public int MessageNumber
        {
            get
            {
                return base.Payload.Data[8];
            }
        }
    }

    public class VehicleOccupancy : X3Packet
    {

        static byte qualifier = 0x11;

        public byte Qualifier
        {
            get { return VehicleOccupancy.qualifier; }
        }

        public VehicleOccupancy(byte[] data)
            : base(data)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Vehicle Occupancy Packet");
        }

        public VehicleOccupancy(X3Packet packet)
            : base(packet)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Vehicle Occupancy Packet");
        }

        public VehicleOccupancy(List<X3Packet> packets)
            : base(packets.Where(p => p.Header.Qualifier == qualifier).FirstOrDefault()) { }

        private int LaneOccupancy(int laneNumber)
        {
            if (laneNumber > 8) throw new Exception("Only 8 Lanes are Supported");
            if (laneNumber < 0) throw new Exception("Invalid Lane Number");
            if (laneNumber > 0) laneNumber--;
            if (base.Payload.Length == 9) return base.Payload.Data[laneNumber];
            else return base.Payload.Data[laneNumber] + base.Payload.Data[laneNumber + 1];
        }

        public int Lane_1_Occupancy
        {
            get { return LaneOccupancy(1); }
        }

        public int Lane_2_Occupancy
        {
            get { return LaneOccupancy(2); }
        }

        public int Lane_3_Occupancy
        {
            get { return LaneOccupancy(3); }
        }

        public int Lane_4_Occupancy
        {
            get { return LaneOccupancy(4); }
        }

        public int Lane_5_Occupancy
        {
            get { return LaneOccupancy(5); }
        }

        public int Lane_6_Occupancy
        {
            get { return LaneOccupancy(6); }
        }

        public int Lane_7_Occupancy
        {
            get { return LaneOccupancy(7); }
        }

        public int Lane_8_Occupancy
        {
            get { return LaneOccupancy(8); }
        }

        public int SensorID
        {
            get
            {
                if (this.Payload.Length == 9) return base.Payload.Data[8];
                else return base.Payload.Data[16];
            }
        }
    }

    public class VehicleSpeed : X3Packet
    {

        static byte qualifier = 0x12;

        public byte Qualifier
        {
            get { return VehicleSpeed.qualifier; }
        }

        public VehicleSpeed(byte[] data)
            : base(data)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Vehicle Speed Packet");
        }

        public VehicleSpeed(X3Packet packet)
            : base(packet)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Vehicle Speed Packet");
        }

        public VehicleSpeed(List<X3Packet> packets)
            : base(packets.Where(p => p.Header.Qualifier == qualifier).FirstOrDefault()) { }

        private int LaneSpeedKPH(int laneNumber)
        {
            if (laneNumber < 0) throw new Exception("Invalid LaneNumber");
            if (laneNumber > 8) throw new Exception("Only 8 Lanes are Supported");
            if (laneNumber > 0) laneNumber--;
            int speed = base.Payload.Data[laneNumber];
            if (speed == 240) speed = -1;
            return speed;
        }

        public int Lane_1_Speed_KPH
        {
            get { return LaneSpeedKPH(1); }
        }

        public int Lane_1_Speed_MPH
        {
            get { return ((int)(LaneSpeedKPH(1) * 0.6214)); }
        }

        public int Lane_2_Speed_KPH
        {
            get { return LaneSpeedKPH(2); }
        }

        public int Lane_2_Speed_MPH
        {
            get { return ((int)(LaneSpeedKPH(2) * 0.6214)); }
        }

        public int Lane_3_Speed_KPH
        {
            get { return LaneSpeedKPH(3); }
        }

        public int Lane_3_Speed_MPH
        {
            get { return ((int)(LaneSpeedKPH(3) * 0.6214)); }
        }

        public int Lane_4_Speed_KPH
        {
            get { return LaneSpeedKPH(4); }
        }

        public int Lane_4_Speed_MPH
        {
            get { return ((int)(LaneSpeedKPH(4) * 0.6214)); }
        }

        public int Lane_5_Speed_KPH
        {
            get { return LaneSpeedKPH(5); }
        }

        public int Lane_5_Speed_MPH
        {
            get { return ((int)(LaneSpeedKPH(5) * 0.6214)); }
        }

        public int Lane_6_Speed_KPH
        {
            get { return LaneSpeedKPH(6); }
        }

        public int Lane_6_Speed_MPH
        {
            get { return ((int)(LaneSpeedKPH(6) * 0.6214)); }
        }

        public int Lane_7_Speed_KPH
        {
            get { return LaneSpeedKPH(7); }
        }

        public int Lane_7_Speed_MPH
        {
            get { return ((int)(LaneSpeedKPH(7) * 0.6214)); }
        }

        public int Lane_8_Speed_KPH
        {
            get { return LaneSpeedKPH(8); }
        }

        public int Lane_8_Speed_MPH
        {
            get { return ((int)(LaneSpeedKPH(8) * 0.6214)); }
        }

        public int ForwardLookingModeAverageSpeed
        {
            get
            {
                int speed = base.Payload.Data[9];
                if (speed == 240) speed = -1;
                return speed;
            }
        }

        public byte Direction
        {
            get { return base.Payload.Data[8]; }
        }

        public string TrafficDirection
        {
            get
            {
                if (Direction == 0x00) return "IN";
                else return "OUT";
            }
        }

        byte HealthStatus
        {
            get { return base.Payload.Data[10]; }
        }

    }

    public class MidSizeVehicleVolume : X3Packet
    {

        static byte qualifier = 0x20;

        public byte Qualifier
        {
            get { return MidSizeVehicleVolume.qualifier; }
        }

        public MidSizeVehicleVolume(byte[] data)
            : base(data)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Mid Size Vehicle Volume Packet");
        }

        public MidSizeVehicleVolume(X3Packet packet)
            : base(packet)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Mid Size Vehicle Volume Packet");
        }

        public MidSizeVehicleVolume(List<X3Packet> packets)
            : base(packets.Where(p => p.Header.Qualifier == qualifier).FirstOrDefault()) { }

        private int LaneVolume(int laneNumber)
        {
            if (laneNumber > 8) throw new Exception("Only 8 Lanes are Supported");
            if (laneNumber < 0) throw new Exception("Invalid LaneNumber");
            if (laneNumber > 0) laneNumber--;
            return base.Payload.Data[laneNumber];
        }

        public int Lane_1_Volume
        {
            get { return LaneVolume(1); }
        }

        public int Lane_2_Volume
        {
            get { return LaneVolume(2); }
        }

        public int Lane_3_Volume
        {
            get { return LaneVolume(3); }
        }

        public int Lane_4_Volume
        {
            get { return LaneVolume(4); }
        }

        public int Lane_5_Volume
        {
            get { return LaneVolume(5); }
        }

        public int Lane_6_Volume
        {
            get { return LaneVolume(6); }
        }

        public int Lane_7_Volume
        {
            get { return LaneVolume(7); }
        }

        public int Lane_8_Volume
        {
            get { return LaneVolume(8); }
        }
    }

    public class XLSizeVehicleVolume : X3Packet
    {

        static byte qualifier = 0x36;

        public byte Qualifier
        {
            get { return XLSizeVehicleVolume.qualifier; }
        }

        public XLSizeVehicleVolume(byte[] data)
            : base(data)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid XL Size Vehicle Volume Packet");
        }

        public XLSizeVehicleVolume(X3Packet packet)
            : base(packet)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid XL Size Vehicle Volume Packet");
        }

        public XLSizeVehicleVolume(List<X3Packet> packets)
            : base(packets.Where(p => p.Header.Qualifier == qualifier).FirstOrDefault()) { }

        private int LaneVolume(int laneNumber)
        {
            if (laneNumber > 8) throw new Exception("Only 8 Lanes are Supported");
            if (laneNumber < 0) throw new Exception("Invalid LaneNumber");
            if (laneNumber > 0) laneNumber--;
            return base.Payload.Data[laneNumber];
        }

        public int Lane_1_Volume
        {
            get { return LaneVolume(1); }
        }

        public int Lane_2_Volume
        {
            get { return LaneVolume(2); }
        }

        public int Lane_3_Volume
        {
            get { return LaneVolume(3); }
        }

        public int Lane_4_Volume
        {
            get { return LaneVolume(4); }
        }

        public int Lane_5_Volume
        {
            get { return LaneVolume(5); }
        }

        public int Lane_6_Volume
        {
            get { return LaneVolume(6); }
        }

        public int Lane_7_Volume
        {
            get { return LaneVolume(7); }
        }

        public int Lane_8_Volume
        {
            get { return LaneVolume(8); }
        }
    }

    public class RealTimeClock : X3Packet
    {

        static byte qualifier = 0x16;

        public byte Qualifier
        {
            get { return RealTimeClock.qualifier; }
        }

        public RealTimeClock(byte[] data)
            : base(data)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Real Time Clock Packet");
        }

        public RealTimeClock(X3Packet packet)
            : base(packet)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Real Time Clock Packet");
        }

        public RealTimeClock(List<X3Packet> packets)
            : base(packets.Where(p => p.Header.Qualifier == qualifier).FirstOrDefault()) { }

        public int Second
        {
            get
            {
                return int.Parse(base.Payload.Data[0].ToString("X"));
            }
        }

        public int Minute
        {
            get
            {
                return int.Parse(base.Payload.Data[1].ToString("X"));
            }
        }

        public int Hour
        {
            get
            {
                return int.Parse(base.Payload.Data[2].ToString("X"));
            }
        }

        public int DayOfWeek
        {
            get
            {
                return int.Parse(base.Payload.Data[3].ToString("X"));
            }
        }

        public int DayOfMonth
        {
            get
            {
                return int.Parse(base.Payload.Data[4].ToString("X"));
            }
        }

        public int Month
        {
            get
            {
                return int.Parse(base.Payload.Data[5].ToString("X"));
            }
        }

        public int Year
        {
            get
            {
                return 2000 + int.Parse(base.Payload.Data[6].ToString("X"));
            }
        }

        public DateTime UniversalDateTime
        {
            get { return new DateTime(Year, Month, DayOfMonth, Hour, Minute, Second, DateTimeKind.Utc); }
        }
    }

    /// <summary>
    /// IMPLEMENT ME
    /// GOTCHA IS the 2 BYTE crc
    /// </summary>
    public class TicksPacket : X3Packet
    {
        protected override byte ComputeChecksum()
        {
            return base.ComputeChecksum();
        }

        public override byte[] ToBytes()
        {
            return base.ToBytes();
        }
    }

    public class InfoMessage : X3Packet
    {

        static byte qualifier = 0x39;

        public byte Qualifier
        {
            get { return InfoMessage.qualifier; }
        }

        public InfoMessage(byte[] data)
            : base(data)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Info Packet");
        }

        public InfoMessage(X3Packet packet)
            : base(packet)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Info Packet");
        }

        public InfoMessage(List<X3Packet> packets)
            : base(packets.Where(p => p.Header.Qualifier == qualifier).FirstOrDefault()) { }

        public int SensorId
        {
            get { return base.Payload.Data[0]; }
        }

        public int MessageNumber
        {
            get { return base.Payload.Data[1]; }
        }

        public int Voltage
        {
            get { return base.Payload.Data[2]; }
        }

        public float Voltage_F
        {
            get
            {
                return (float)(Voltage * 100) / 1000;
            }
        }

        public byte Health
        {
            get { return base.Payload.Data[3]; }
        }
    }

    /// <summary>
    /// Sent every 100 ms in NORMAL mode only. Indicates presence in lane
    /// </summary>
    public class Targets : X3Packet
    {

        static byte qualifier = 0x18;

        public byte Qualifier
        {
            get { return Targets.qualifier; }
        }

        public Targets(byte[] data)
            : base(data)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Targets Packet");
        }

        public Targets(X3Packet packet)
            : base(packet)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Targets Packet");
        }

        public Targets(List<X3Packet> packets)
            : base(packets.Where(p => p.Header.Qualifier == qualifier).FirstOrDefault()) { }

        public bool LanePresence(int laneNumber)
        {
            if (laneNumber > 8) throw new Exception("Only 8 Lanes are Supported");
            if (laneNumber < 0) throw new Exception("Invalid LaneNumber");
            if (laneNumber > 0) laneNumber--;
            if (laneNumber > 4) laneNumber -= 4;

            return (base.Payload.Data[laneNumber] << 8) == 1;
        }
    }

    /// <summary>
    /// IMPLEMENT ME
    /// GOTCHA IS NO DATA FORMAT FOR IMPLEMENTATION
    /// </summary>
    public class RTMSStat : X3Packet
    {
    }

    /// <summary>
    /// Sent in Normal mode in Forward looking modes
    /// </summary>
    public class RealTimeSpeed : X3Packet
    {

        static byte qualifier = 0x1b;

        public byte Qualifier
        {
            get { return RealTimeSpeed.qualifier; }
        }

        public RealTimeSpeed(byte[] data)
            : base(data)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Real Time Speed Packet");
        }

        public RealTimeSpeed(X3Packet packet)
            : base(packet)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Real Time Speed Packet");
        }

        public RealTimeSpeed(List<X3Packet> packets)
            : base(packets.Where(p => p.Header.Qualifier == qualifier).FirstOrDefault()) { }

        public int SensorId
        {
            get { return base.Payload.Data[0]; }
        }

        public int ActualSpeed_KPH
        {
            get { return base.Payload.Data[1]; }
        }

        public int ActualSpeed_MPH
        {
            get { return ((int)(ActualSpeed_KPH * 0.6214)); }
        }

        public int VehicleLength_DCM
        {
            get { return base.Payload.Data[2]; }
        }

        public byte Direction
        {
            get { return base.Payload.Data[3]; }
        }

        public string TrafficDirection
        {
            get
            {
                if (Direction == 0x00) return "IN";
                else return "OUT";
            }
        }

        public int TrapSpeed_KPH
        {
            get { return base.Payload.Data[4]; }
        }

        public int TrapSpeed_MPH
        {
            get { return ((int)(TrapSpeed_KPH * 0.6214)); }
        }

    }

    /// <summary>
    /// Sent 20 Seconds aftter A Self-Test Request (BitRequest)
    /// </summary>
    public class BitResult : X3Packet
    {

        static byte qualifier = 0x08;

        public byte Qualifier
        {
            get { return BitResult.qualifier; }
        }

        System.Collections.BitArray _bits;

        public BitResult(byte[] data)
            : base(data)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Bit Result Packet");
            _bits = new System.Collections.BitArray(new int[] { base.Payload.Data[0] });
        }

        public BitResult(X3Packet packet)
            : base(packet)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Bit Result Packet");
            _bits = new System.Collections.BitArray(new int[] { base.Payload.Data[0] });
        }

        public BitResult(List<X3Packet> packets)
            : base(packets.Where(p => p.Header.Qualifier == qualifier).FirstOrDefault()) { }

        public bool PowerSupplyFault
        {
            get
            {
                return _bits.Get(1);
            }
        }

        public bool ModulatorSignalFault
        {
            get
            {
                return _bits.Get(2);
            }
        }

        public bool MicrowaveModuleFault
        {
            get
            {
                return _bits.Get(3);
            }
        }

        public bool TempratureCalibratorFault
        {
            get
            {
                return _bits.Get(4);
            }
        }

        public bool ModulatorMemoryFault
        {
            get
            {
                return _bits.Get(5);
            }
        }

        public bool DSPFault
        {
            get
            {
                return _bits.Get(6);
            }
        }

        public bool ProgramMemoryFault
        {
            get
            {
                return _bits.Get(7);
            }
        }

        public bool ADCFault
        {
            get
            {
                return _bits.Get(8);
            }
        }

        public byte FirwareMajorVersion
        {
            get { return base.Payload.Data[1]; }
        }

        public byte FirwareMinorVersion
        {
            get { return base.Payload.Data[2]; }
        }

    }

    /// <summary>
    /// Provides the results of a PolledBitRequest
    /// </summary>
    public class PolledBitResult : X3Packet
    {
        static byte qualifier = 0x1d;

        public byte Qualifier
        {
            get { return PolledBitResult.qualifier; }
        }

        System.Collections.BitArray _bits;

        public PolledBitResult(byte[] data)
            : base(data)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Polled Bit Result Packet");
            _bits = new System.Collections.BitArray(new int[] { base.Payload.Data[1] });
        }

        public PolledBitResult(X3Packet packet)
            : base(packet)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Polled Bit Result Packet");
            _bits = new System.Collections.BitArray(new int[] { base.Payload.Data[1] });
        }

        public PolledBitResult(List<X3Packet> packets)
            : base(packets.Where(p => p.Header.Qualifier == qualifier).FirstOrDefault()) { }

        public int SensorId
        {
            get { return base.Payload.Data[0]; }
        }

        public bool PowerSupplyFault
        {
            get
            {
                return _bits.Get(1);
            }
        }

        public bool ModulatorSignalFault
        {
            get
            {
                return _bits.Get(2);
            }
        }

        public bool MicrowaveModuleFault
        {
            get
            {
                return _bits.Get(3);
            }
        }

        public bool TempratureCalibratorFault
        {
            get
            {
                return _bits.Get(4);
            }
        }

        public bool ModulatorMemoryFault
        {
            get
            {
                return _bits.Get(5);
            }
        }

        public bool DSPFault
        {
            get
            {
                return _bits.Get(6);
            }
        }

        public bool ProgramMemoryFault
        {
            get
            {
                return _bits.Get(7);
            }
        }

        public bool ADCFault
        {
            get
            {
                return _bits.Get(8);
            }
        }

        public byte FirwareMajorVersion
        {
            get { return base.Payload.Data[2]; }
        }

        public byte FirwareMinorVersion
        {
            get { return base.Payload.Data[3]; }
        }
    }

    /// <summary>
    /// Sent in response to a PowerMangementVectorRequest
    /// </summary>
    public class PowerManagementVector : X3Packet
    {

        static byte qualifier = 0xaa;

        public byte Qualifier
        {
            get { return PowerManagementVector.qualifier; }
        }

        public PowerManagementVector(byte[] data)
            : base(data)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Power Mangement Vector Result Packet");
        }

        public PowerManagementVector(X3Packet packet)
            : base(packet)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Power Mangement Vector Result Packet");
        }

        public PowerManagementVector(List<X3Packet> packets)
            : base(packets.Where(p => p.Header.Qualifier == qualifier).FirstOrDefault()) { }

        int SensorId
        {
            get { return base.Payload.Data[0]; }
        }

        int NumberOfMessagePeriodsInONMode
        {
            get { return base.Payload.Data[1]; }
        }

        int NumberOfMinutesInOFFMode
        {
            get { return base.Payload.Data[2]; }
        }

    }

    /// <summary>
    /// Sent in response to a SoftwareInformationRequest
    /// </summary>
    public class SoftwareInformation : X3Packet
    {

        static byte qualifier = 0x0c;

        public byte Qualifier
        {
            get { return SoftwareInformation.qualifier; }
        }

        public SoftwareInformation(byte[] data)
            : base(data)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Software Information Packet");
        }

        public SoftwareInformation(X3Packet packet)
            : base(packet)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Software Information Packet");
        }

        public SoftwareInformation(List<X3Packet> packets)
            : base(packets.Where(p => p.Header.Qualifier == qualifier).FirstOrDefault()) { }

        public int SerialNumber
        {
            get
            {
                return 65535;
            }
        }

        public string SoftwareVersion
        {
            get
            {
                string format = "{0}.{1}/{2}/{3}";
                char[] versions = base.Payload.Data[4].ToString("X").ToCharArray();
                string major = versions[0].ToString();
                string minor = versions[1].ToString();
                return string.Format(format, major, minor, base.Payload.Data[5].ToString("X"), base.Payload.Data[8].ToString("X"));
            }
        }


    }

    /// <summary>
    /// Sent in response to a IntervalInformationRequest
    /// </summary>
    public class IntervalInformation : X3Packet
    {

        static byte qualifier = 0x0e;

        public byte Qualifier
        {
            get { return IntervalInformation.qualifier; }
        }

        public IntervalInformation(byte[] data)
            : base(data)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Interval Information Packet");
        }

        public IntervalInformation(X3Packet packet)
            : base(packet)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Interval Information Packet");
        }

        public IntervalInformation(List<X3Packet> packets)
            : base(packets.Where(p => p.Header.Qualifier == qualifier).FirstOrDefault()) { }

        public int ZoneCount
        {
            get
            {
                return int.Parse(base.Payload.Data[4].ToString("X"), System.Globalization.NumberStyles.HexNumber);
            }
        }

        public int Sensitivity
        {
            get
            {
                switch (base.Payload.Data[19].ToString("X"))
                {
                    case "84":
                        return 1;
                    case "8C":
                        return 2;
                    case "94":
                        return 3;
                    case "9C":
                        return 4;
                    case "A4":
                        return 5;
                    case "AC":
                        return 6;
                    case "B4":
                        return 7;
                    case "BC":
                        return 8;
                    case "C4":
                        return 9;
                    case "CC":
                        return 10;
                    case "D4":
                        return 11;
                    case "DC":
                        return 12;
                    case "E4":
                        return 13;
                    case "EC":
                        return 14;
                    case "F4":
                        return 15;
                    default:
                        return -1;
                }
            }
        }

        public int DataIntervalDurationSeconds
        {
            get
            {
                return int.Parse(base.Payload.Data[15].ToString("X"), System.Globalization.NumberStyles.HexNumber);
            }
        }
    }

    /// <summary>
    /// Sent in response to a BinInformationRequest
    /// </summary>
    public class BinInformation : X3Packet
    {

        static byte qualifier = 0x0b;

        public byte Qualifier
        {
            get { return BinInformation.qualifier; }
        }

        public BinInformation(byte[] data)
            : base(data)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Bin Information Packet");
        }

        public BinInformation(X3Packet packet)
            : base(packet)
        {
            if (Header.Qualifier != qualifier) throw new Exception("Invalid Bin Information Packet");
        }

        public BinInformation(List<X3Packet> packets)
            : base(packets.Where(p => p.Header.Qualifier == qualifier).FirstOrDefault()) { }

        private int GetBin(int binNumber)
        {
            int speed = int.Parse(base.Payload.Data[binNumber].ToString("X"), System.Globalization.NumberStyles.HexNumber);
            if (speed == 0xf0 || speed == 0xff || speed < 0) speed = 20;
            return speed;
        }

        public int this[int binIndex]
        {
            get
            {
                return GetBin(binIndex);
            }
        }

        public int Bin1
        {
            get
            {
                return GetBin(0);
            }
        }

        public int Bin2
        {
            get
            {
                return GetBin(1);
            }
        }

        public int Bin3
        {
            get
            {
                return GetBin(2);
            }
        }

        public int Bin4
        {
            get
            {
                return GetBin(3);
            }
        }

        public int Bin5
        {
            get
            {
                return GetBin(4);
            }
        }

        public int Bin6
        {
            get
            {
                return GetBin(5);
            }
        }

        public int Bin7
        {
            get
            {
                return GetBin(6);
            }
        }

    }

    #endregion

    #region Transport

    public class X3Messenger
    {
        #region Fields

        IPEndPoint ipEndPoint;
        ProtocolType transportProtocol;

        TcpClient tcpClient;
        NetworkStream netStream;
        UdpClient udpClient;

        int tcpSentBytes;
        int udpSentBytes;

        int tcpRecvBytes;
        int udpRecvBytes;

        int sendRecvTimeout = 10000;

        bool busy;

        List<X3Packet> requestLog = new List<X3Packet>();
        List<X3Packet> recieveLog = new List<X3Packet>();

        int sensorId = 1;

        SoftwareInformation softwareInfo;
        IntervalInformation intervalInfo;
        PowerManagementVector powerInfo;
        BinInformation binInfo;

        #endregion

        #region Properties

        /// <summary>
        /// The transport protocol this messenger is communicating with
        /// </summary>
        public ProtocolType TransportProtocol
        {
            get { return transportProtocol; }
            set
            {
                if (value == ProtocolType.Udp)
                {
                    netStream.Close();
                    tcpClient.Close();
                }
                else
                {
                    udpClient.Close();
                }
                transportProtocol = value;
                System.Threading.Thread.Sleep(0);
                Initialize();
            }
        }

        /// <summary>
        /// The IPEndPoint the messenger is communicating with
        /// </summary>
        public IPEndPoint IPEndPoint
        {
            get { return ipEndPoint; }
        }

        /// <summary>
        /// The Socket the messenger is communicating with
        /// </summary>
        public Socket Socket
        {
            get
            {
                if (transportProtocol == ProtocolType.Tcp) return tcpClient.Client;
                else return udpClient.Client;
            }
        }

        /// <summary>
        /// The Sensor Id to address packets to
        /// </summary>
        public int SensorId
        {
            get { return sensorId; }
            set { if (value <= 0) return; if (value > 255) return; sensorId = value; }
        }

        /// <summary>
        /// The amount of time the messenger will wait before timeout during sending and recieving
        /// </summary>
        public int SendRecieveTimeout
        {
            get { return sendRecvTimeout; }
            set { if (value < 0) return; sendRecvTimeout = value; }
        }

        /// <summary>
        /// The amount of bytes sent over TCP
        /// </summary>
        public int TcpSentBytes
        {
            get { return tcpSentBytes; }
        }

        /// <summary>
        /// The amount of bytes recieved over TCP
        /// </summary>
        public int TcpRecievedBytes
        {
            get { return tcpRecvBytes; }
        }

        /// <summary>
        /// The amount of bytes sent over UDP
        /// </summary>
        public int UdpSentBytes
        {
            get { return udpSentBytes; }
        }

        /// <summary>
        /// The amount of bytes recieved over UDP
        /// </summary>
        public int UdpRecievedBytes
        {
            get { return udpRecvBytes; }
        }

        /// <summary>
        /// The total amount of bytes sent with this messenger
        /// </summary>
        public int TotalSentBytes
        {
            get { return tcpSentBytes + udpSentBytes; }
        }

        /// <summary>
        /// The total amount of bytes recieved with this messenger
        /// </summary>
        public int TotalRecievedBytes
        {
            get { return tcpRecvBytes + udpRecvBytes; }
        }

        /// <summary>
        /// Indicates if this messenger is busy
        /// </summary>
        public bool IsBusy
        {
            get { return busy; }
        }

        /// <summary>
        /// A Log of all packets sent with this messenger
        /// </summary>
        public List<X3Packet> RequestLog
        {
            get { return requestLog; }
        }

        /// <summary>
        /// A Log of all packets recieved with this messenger
        /// </summary>
        public List<X3Packet> RecieveLog
        {
            get { return recieveLog; }
        }

        /// <summary>
        /// The software information retrieved by the messenger
        /// </summary>
        public SoftwareInformation SoftwareInformation
        {
            get { return softwareInfo; }
        }

        /// <summary>
        /// The interval information retrieved by the messenger
        /// </summary>
        public IntervalInformation IntervalInformation
        {
            get { return intervalInfo; }
        }

        /// <summary>
        /// The Power management information retrieved by the messenger
        /// </summary>
        public PowerManagementVector PowerMangementInformation
        {
            get { return powerInfo; }
        }

        /// <summary>
        /// The Bin information retrieved by the messenger
        /// </summary>
        public BinInformation BinInformation
        {
            get { return binInfo; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets up the socket for the messenger
        /// </summary>
        void Initialize()
        {
            try
            {
                if (transportProtocol == ProtocolType.Tcp)
                {
                    if (null == tcpClient)
                    {
                        tcpClient = new TcpClient();
                        tcpClient.Connect(ipEndPoint);
                    }
                    netStream = tcpClient.GetStream();
                }
                else
                {
                    if (null == udpClient)
                    {
                        udpClient = new UdpClient();
                        udpClient.Connect(ipEndPoint);
                    }
                }

                FetchConfiguration();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Clears the socket for any messages available and adds them to the recive log
        /// </summary>
        void ClearSocket()
        {
            if (Socket.Available > 0)
            {
                //Frame packets
                if (transportProtocol == ProtocolType.Tcp)
                {
                    byte[] inBuffers = new byte[2048];
                    EndPoint endPoint = ipEndPoint;
                    int rcv = tcpClient.Client.ReceiveFrom(inBuffers, ref endPoint);
                    byte[] inBytes = new byte[rcv];
                    Array.Copy(inBuffers, 0, inBytes, 0, rcv);
                    tcpRecvBytes += rcv;
                    List<byte> inBuffer = new List<byte>(inBytes);
                    while (inBuffer.Count > 0 && inBuffer[0] != 0xff) inBuffer.RemoveAt(0);
                    while (inBuffer.Count > 1)
                    {
                        recieveLog.Add(new X3Packet(inBuffer.ToArray()));
                        inBuffer.RemoveRange(0, recieveLog[recieveLog.Count - 1].Length);
                        while (inBuffer.Count > 0 && inBuffer[0] != 0xff) inBuffer.RemoveAt(0);
                    }
                    return;
                }
                else
                {
                    byte[] inBytes = udpClient.Receive(ref ipEndPoint);
                    udpRecvBytes += inBytes.Length;
                    List<byte> inBuffer = new List<byte>(inBytes);
                    while (inBuffer.Count > 0 && inBuffer[0] != 0xff) inBuffer.RemoveAt(0);
                    while (inBuffer.Count > 1)
                    {
                        recieveLog.Add(new X3Packet(inBuffer.ToArray()));
                        inBuffer.RemoveRange(0, recieveLog[recieveLog.Count - 1].Length);
                        while (inBuffer.Count > 0 && inBuffer[0] != 0xff) inBuffer.RemoveAt(0);
                    }
                    return;
                }
            }
        }

        /// <summary>
        /// Downloads the SoftwareInformation from the sensor
        /// </summary>
        void FetchSoftwareInformation()
        {
            /*
            try
            {
                softwareInfo = new SoftwareInformation(SendMessage(new SoftwareInformationRequest())[0]);
            }
            catch
            {
                softwareInfo = new SoftwareInformation(SendMessage(new PolledSoftwareInformationRequest(sensorId))[0]);
            }*/
            List<X3Packet> responses = SendMessage(new PolledSoftwareInformationRequest(sensorId));

            if (responses[0].Header.Qualifier == 26 && responses.Count == 1)
            {
                FetchSoftwareInformation();
                return;
            }

            if (responses.Count > 1)
            {
                X3Packet infoPacket = responses.Find(delegate(X3Packet packet)
                {
                    return packet.Header.Qualifier == 0x0c;
                });
                if (infoPacket != null)
                {
                    softwareInfo = new SoftwareInformation(infoPacket);
                }
                else
                {
                    FetchSoftwareInformation();
                    return;
                }
            }
            else
            {
                softwareInfo = new SoftwareInformation(responses[0]);
            }
            responses = null;
        }

        /// <summary>
        /// Downloads the IntervalInformation from the sensor
        /// </summary>
        void FetchIntervalInformation()
        {
            /*
            try
            {
                intervalInfo = new IntervalInformation(SendMessage(new IntervalInformationRequest())[0]);
            }
            catch
            {
                intervalInfo = new IntervalInformation(SendMessage(new PolledIntervalInformationRequest(sensorId))[0]);
            }*/
            List<X3Packet> responses = SendMessage(new PolledIntervalInformationRequest(sensorId));

            if (responses[0].Header.Qualifier == 26 && responses.Count == 1)
            {
                FetchIntervalInformation();
                return;
            }

            if (responses.Count > 1)
            {
                X3Packet infoPacket = responses.Find(delegate(X3Packet packet)
                {
                    return packet.Header.Qualifier == 0x0e;
                });
                if (infoPacket != null)
                {
                    intervalInfo = new IntervalInformation(infoPacket);
                }
                else
                {
                    FetchIntervalInformation();
                    return;
                }
            }
            else
            {
                intervalInfo = new IntervalInformation(responses[0]);
            }
            responses = null;
        }

        /// <summary>
        /// Downloads the PowerMangementInformation from the sensor
        /// </summary>
        void FetchPowerMangementVector()
        {
            /*
            try
            {
                powerInfo = new PowerManagementVector(SendMessage(new PowerMangementVectorRequest(0xff))[0]);
            }
            catch
            {
                powerInfo = new PowerManagementVector(SendMessage(new PowerMangementVectorRequest(sensorId))[0]);
            }
            */

            List<X3Packet> responses = SendMessage(new PowerMangementVectorRequest(sensorId));
            if (responses[0].Header.Qualifier == 26 && responses.Count == 1)
            {
                FetchPowerMangementVector();
                return;
            }
            if (responses.Count > 1)
            {
                X3Packet infoPacket = responses.Find(delegate(X3Packet packet)
                {
                    return packet.Header.Qualifier == 0xaa;
                });
                if (infoPacket != null)
                {
                    powerInfo = new PowerManagementVector(infoPacket);
                }
                else
                {
                    FetchPowerMangementVector();
                    return;
                }
            }
            else
            {
                powerInfo = new PowerManagementVector(responses[0]);
            }
            responses = null;

        }

        /// <summary>
        /// Downloads the BinInformation from the sensor
        /// </summary>
        void FetchBinInformation()
        {
            /*
            try
            {
                binInfo = new BinInformation(SendMessage(new BinsRequest())[0]);
            }
            catch
            {
                binInfo = new BinInformation(SendMessage(new PolledBinsRequest(sensorId))[0]);
            }
             */
            List<X3Packet> responses = SendMessage(new PolledBinsRequest(sensorId));
            if (responses[0].Header.Qualifier == 26 && responses.Count == 1)
            {
                FetchBinInformation();
                return;
            }
            if (responses.Count > 1)
            {
                X3Packet infoPacket = responses.Find(delegate(X3Packet packet)
                {
                    return packet.Header.Qualifier == 0x0b;
                });
                if (infoPacket != null)
                {
                    binInfo = new BinInformation(infoPacket);
                }
                else
                {
                    FetchBinInformation();
                    return;
                }
            }
            else
            {
                binInfo = new BinInformation(responses[0]);
            }
            responses = null;
        }

        /// <summary>
        /// Downloads several configuration packets from the sensor
        /// </summary>
        void FetchConfiguration()
        {
            FetchSoftwareInformation();
            FetchIntervalInformation();
            FetchPowerMangementVector();
            FetchBinInformation();
        }

        /// <summary>
        /// Instructs the sensor to save its settings to flash
        /// </summary>
        public void SendSaveToFlash()
        {
            SaveBlockRequest block1 = new SaveBlockRequest(sensorId, 0x01, DateTime.Now.Day);
            SaveBlockRequest block2 = new SaveBlockRequest(sensorId, 0x02, 0x06);
            SaveBlockRequest block3 = new SaveBlockRequest(sensorId, 0x03, 0x0a);

            block1.HasResponse = false;
            block2.HasResponse = false;
            block3.HasResponse = false;

            SendMessage(block1);
            SendMessage(block2);
            SendMessage(block3);
        }

        /// <summary>
        /// Instrucsts the sensor to enter Stat mode
        /// </summary>
        public void EnterStatMode()
        {

            SendSaveToFlash();

            X3Packet enterStatPacket1 = new X3Packet();
            enterStatPacket1.Header.Qualifier = 0x7f;
            enterStatPacket1.Payload.Data = new byte[] { 0xbf, 0xfd, 0xfc, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            X3Packet enterStatPacket2 = new X3Packet();
            enterStatPacket2.Header.Qualifier = 0xfc;
            enterStatPacket2.Payload.Data = new byte[] { 0x41 };

            X3Packet enterStatPacket3 = new X3Packet();
            enterStatPacket3.Header.Qualifier = 0xe8;
            enterStatPacket3.Payload.Data = new byte[] { (byte)sensorId, 0x41 };
            enterStatPacket3.HasResponse = true;

            SendMessage(enterStatPacket1);

            SendMessage(enterStatPacket2);

            SendMessage(enterStatPacket3);
        }

        /// <summary>
        /// Instrucsts the sensor to enter Polled mode
        /// </summary>
        public void EnterPolledMode()
        {
            SendSaveToFlash();

            X3Packet enterPolledPacket1 = new X3Packet();
            enterPolledPacket1.Header.Qualifier = 0x7f;
            enterPolledPacket1.Payload.Data = new byte[] { 0xbf, 0xfd, 0xfc, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            X3Packet enterPolledPacket2 = new X3Packet();
            enterPolledPacket2.Header.Qualifier = 0xfc;
            enterPolledPacket2.Payload.Data = new byte[] { 0x51 };

            X3Packet enterPolledPacket3 = new X3Packet();
            enterPolledPacket3.Header.Qualifier = 0xe8;
            enterPolledPacket3.Payload.Data = new byte[] { (byte)sensorId, 0x51 };
            enterPolledPacket3.HasResponse = true;

            SendMessage(enterPolledPacket1);

            SendMessage(enterPolledPacket2);

            SendMessage(enterPolledPacket3);

        }

        /// <summary>
        /// Instrucsts the sensor to enter Normal mode
        /// </summary>
        public void EnterNormalMode()
        {
            X3Packet enterNormalPacket1 = new X3Packet();
            enterNormalPacket1.Header.Qualifier = 0x7f;
            enterNormalPacket1.Payload.Data = new byte[] { 0xbf, 0xfd, 0xfc, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            X3Packet enterNormalPacket2 = new X3Packet();
            enterNormalPacket2.Header.Qualifier = 0xfc;
            enterNormalPacket2.Payload.Data = new byte[] { 0xc1 };

            X3Packet enterNormalPacket3 = new X3Packet();
            enterNormalPacket3.Header.Qualifier = 0xe8;
            enterNormalPacket3.Payload.Data = new byte[] { (byte)sensorId, 0xc1 };

            SendMessage(enterNormalPacket1);

            SendMessage(enterNormalPacket2);

            SendMessage(enterNormalPacket3);

            SendSaveToFlash();

        }

        /// <summary>
        /// Performs a DataRequest on the sensor
        /// </summary>
        public List<X3Packet> DataRequest()
        {
            try
            {
                int lastSampleCount = GetSamples().Count;
                List<X3Packet> results;

                if (Socket.Available > 0 && Socket.Available >= 100)
                {
                    ClearSocket();
                    results = GetLatestSample();
                    if (results.Count > lastSampleCount) return results;
                }

                results = SendMessage(new DataRequest(sensorId));

                if (results[0].Header.Qualifier == 26 && results.Count == 1)
                {
                    return DataRequest();
                }

                if (results.Count > 0 && lastSampleCount < GetSamples().Count)
                {
                    FlushRTMSBuffer();
                    return results;
                }
                else
                {
                    if (results.Count == 0) return results;
                    return DataRequest();
                }
            }
            catch
            {
                return GetLatestSample();
            }
        }

        /// <summary>
        /// Filters the recieve log for packets which pertain to a sample
        /// </summary>
        public List<List<X3Packet>> GetSamples()
        {
            List<List<X3Packet>> results = new List<List<X3Packet>>();
            List<X3Packet> sample = new List<X3Packet>();

            foreach (X3Packet packet in RecieveLog)
            {
                if (packet.Header.Qualifier == 0x1b)
                {
                    if (sample.Count > 0)
                    {
                        results.Add(sample);
                        sample = new List<X3Packet>();
                    }
                    sample.Add(new LongVehicleVolume(packet));
                }
                else if (packet.Header.Qualifier == 0x10) sample.Add(new VehicleVolume(packet));
                else if (packet.Header.Qualifier == 0x11) sample.Add(new VehicleOccupancy(packet));
                else if (packet.Header.Qualifier == 0x20) sample.Add(new MidSizeVehicleVolume(packet));
                else if (packet.Header.Qualifier == 0x36) sample.Add(new XLSizeVehicleVolume(packet));
                else if (packet.Header.Qualifier == 0x12) sample.Add(new VehicleSpeed(packet));
                else if (packet.Header.Qualifier == 0x16) sample.Add(new RealTimeClock(packet));

            }

            if (sample.Count > 0) results.Add(sample);

            return results;
        }

        /// <summary>
        /// Retrieves the last item from the Samples List
        /// </summary>
        public List<X3Packet> GetLatestSample()
        {
            List<List<X3Packet>> samples = GetSamples();
            if (samples.Count == 0) return new List<X3Packet>();
            else return samples[samples.Count - 1];
        }

        /// <summary>
        /// Sends a X3Packet
        /// </summary>
        /// <param name="msg">The X3Packet to communicate</param>
        /// <returns>The responses in a list or a empty list if no responses</returns>
        public List<X3Packet> SendMessage(X3Packet msg)
        {
            return _SendMessage(msg, 0);
        }

        /// <summary>
        /// Attempts to send the message up to 5 times
        /// </summary>
        /// <param name="msg">The message to send</param>
        /// <param name="retry">the retry counter</param>
        /// <returns>The list of responses or a empty list of no responses in the alloted amount of retires</returns>
        private List<X3Packet> _SendMessage(X3Packet msg, int retry)
        {
            Socket.SendTimeout = Socket.ReceiveTimeout = sendRecvTimeout;

            if (!Socket.Connected) Socket.Connect(ipEndPoint);

            ClearSocket();

            List<X3Packet> responses = new List<X3Packet>();

            if (retry == 5) return responses;

            requestLog.Add(msg);

            byte[] outBytes = msg.ToBytes();

            if (transportProtocol == ProtocolType.Tcp)
            {
                netStream.Write(outBytes, 0, outBytes.Length);
                tcpSentBytes += outBytes.Length;
            }
            else
            {
                udpSentBytes += udpClient.Send(outBytes, outBytes.Length);
            }

            if (!msg.HasResponse)
            {
                ClearSocket();
                return responses;
            }

            if (transportProtocol == ProtocolType.Tcp)
            {
                byte[] inBuffers = new byte[2048];
                EndPoint endPoint = ipEndPoint;
                DateTime start = DateTime.Now;
                while (Socket.Available <= 0)
                {
                    System.Threading.Thread.Sleep(500);
                    if ((DateTime.Now - start).Seconds > (sendRecvTimeout / 1000))
                    {
                        return _SendMessage(msg, retry + 1);
                    }
                }

                int rcv = tcpClient.Client.ReceiveFrom(inBuffers, ref endPoint);
                byte[] inBytes = new byte[rcv];
                Array.Copy(inBuffers, 0, inBytes, 0, rcv);
                tcpRecvBytes += rcv;
                List<byte> inBuffer = new List<byte>(inBytes);
                while (inBuffer.Count > 0 && inBuffer[0] != 0xff) inBuffer.RemoveAt(0);
                while (inBuffer.Count > 1)
                {
                    responses.Add(new X3Packet(inBuffer.ToArray()));
                    inBuffer.RemoveRange(0, responses[responses.Count - 1].Length);
                    while (inBuffer.Count > 0 && inBuffer[0] != 0xff) inBuffer.RemoveAt(0);
                }
                recieveLog.AddRange(responses);
                return responses;
            }
            else
            {
                DateTime start = DateTime.Now;
                while (Socket.Available <= 0)
                {
                    System.Threading.Thread.Sleep(500);
                    if ((DateTime.Now - start).Seconds > (sendRecvTimeout / 1000))
                    {
                        return _SendMessage(msg, retry + 1);
                    }
                }
                byte[] inBytes = udpClient.Receive(ref ipEndPoint);
                udpRecvBytes += inBytes.Length;
                List<byte> inBuffer = new List<byte>(inBytes);
                while (inBuffer.Count > 0 && inBuffer[0] != 0xff) inBuffer.RemoveAt(0);
                while (inBuffer.Count > 1)
                {
                    responses.Add(new X3Packet(inBuffer.ToArray()));
                    inBuffer.RemoveRange(0, responses[responses.Count - 1].Length);
                    while (inBuffer.Count > 0 && inBuffer[0] != 0xff) inBuffer.RemoveAt(0);
                }
                recieveLog.AddRange(responses);
                return responses;
            }
        }

        /// <summary>
        /// Instructs the sensor to update its sensitivity
        /// </summary>
        /// <param name="newSensitivity">The new sensitivity parameter</param>
        public void UpdateSensitivity(int newSensitivity)
        {
            if (newSensitivity < 1) throw new ArgumentOutOfRangeException("newSensitivity", "Cannot be less than 1");
            if (newSensitivity > 15) throw new ArgumentOutOfRangeException("newSensitivity", "Cannot be grater than 15");

            //byte[] msg = null;

            SaveBlockRequest updateSensitivityPacket = null;

            switch (newSensitivity)
            {
                case 1:
                    //msg = new byte[] { 0xff, 0x85, 0x03, 0x01, 0x13, 0x84, 0x98 };
                    updateSensitivityPacket = new SaveBlockRequest(sensorId, 0x13, 0x84);
                    break;
                case 2:
                    //msg = new byte[] { 0xff, 0x85, 0x03, 0x01, 0x13, 0x8C, 0xa0 };
                    updateSensitivityPacket = new SaveBlockRequest(sensorId, 0x13, 0x8c);
                    break;
                case 3:
                    //msg = new byte[] { 0xff, 0x85, 0x03, 0x01, 0x13, 0x94, 0xa8 };
                    updateSensitivityPacket = new SaveBlockRequest(sensorId, 0x13, 0x94);
                    break;
                case 4:
                    //msg = new byte[] { 0xff, 0x85, 0x03, 0x01, 0x13, 0x9c, 0xb0 };
                    updateSensitivityPacket = new SaveBlockRequest(sensorId, 0x13, 0x9c);
                    break;
                case 5:
                    //msg = new byte[] { 0xff, 0x85, 0x03, 0x01, 0x13, 0xa4, 0xb8 };
                    updateSensitivityPacket = new SaveBlockRequest(sensorId, 0x13, 0xa4);
                    break;
                case 6:
                    //msg = new byte[] { 0xff, 0x85, 0x03, 0x01, 0x13, 0xac, 0xc0 };
                    updateSensitivityPacket = new SaveBlockRequest(sensorId, 0x13, 0xac);
                    break;
                case 7:
                    //msg = new byte[] { 0xff, 0x85, 0x03, 0x01, 0x13, 0xb4, 0xc8 };
                    updateSensitivityPacket = new SaveBlockRequest(sensorId, 0x13, 0xb4);
                    break;
                case 8:
                    //msg = new byte[] { 0xff, 0x85, 0x03, 0x01, 0x13, 0xbc, 0xd0 };
                    updateSensitivityPacket = new SaveBlockRequest(sensorId, 0x13, 0xbc);
                    break;
                case 9:
                    //msg = new byte[] { 0xff, 0x85, 0x03, 0x01, 0x13, 0xc4, 0xd8 };
                    updateSensitivityPacket = new SaveBlockRequest(sensorId, 0x13, 0xc4);
                    break;
                case 10:
                    //msg = new byte[] { 0xff, 0x85, 0x03, 0x01, 0x13, 0xcc, 0xe0 };
                    updateSensitivityPacket = new SaveBlockRequest(sensorId, 0x13, 0xcc);
                    break;
                case 11:
                    //msg = new byte[] { 0xff, 0x85, 0x03, 0x01, 0x13, 0xd4, 0xe8 };
                    updateSensitivityPacket = new SaveBlockRequest(sensorId, 0x13, 0xd4);
                    break;
                case 12:
                    //msg = new byte[] { 0xff, 0x85, 0x03, 0x01, 0x13, 0xdc, 0xf0 };
                    updateSensitivityPacket = new SaveBlockRequest(sensorId, 0x13, 0xdc);
                    break;
                case 13:
                    //msg = new byte[] { 0xff, 0x85, 0x03, 0x01, 0x13, 0xe4, 0xf8 };
                    updateSensitivityPacket = new SaveBlockRequest(sensorId, 0x13, 0xe4);
                    break;
                case 14:
                    //msg = new byte[] { 0xff, 0x85, 0x03, 0x01, 0x13, 0xec, 0x00 };
                    updateSensitivityPacket = new SaveBlockRequest(sensorId, 0x13, 0xec);
                    break;
                case 15:
                    //msg = new byte[] { 0xff, 0x85, 0x03, 0x01, 0x13, 0xf4, 0x08 };
                    updateSensitivityPacket = new SaveBlockRequest(sensorId, 0x13, 0xf4);
                    break;
            }

            updateSensitivityPacket.HasResponse = true;

            //RTMSPacket setSensitivityPacket = new RTMSPacket(msg);
            //setSensitivityPacket.HasResponse = false;

            X3Packet setSensitivityConfirmpacket = new X3Packet(new byte[] { 0xff, 0xd0, 0x01, 0x20, 0x20 });
            setSensitivityConfirmpacket.HasResponse = true;

            SendMessage(updateSensitivityPacket);

            X3Packet confirmResonse = SendMessage(setSensitivityConfirmpacket)[0];

            SendSaveToFlash();

            FetchIntervalInformation();

        }

        /// <summary>
        /// Instructs the sensor to change its zone count
        /// </summary>
        /// <param name="numberOfZones">The new number of zones</param>
        public void UpdateZoneCount(int numberOfZones)
        {
            SendMessage(new SaveBlockRequest(sensorId, 4, numberOfZones));
            FetchIntervalInformation();
        }

        /// <summary>
        /// Updates the SpeedBins on the sensor
        /// </summary>
        /// <param name="newBins">The array describing the new bins</param>
        public void UpdateSpeedBins(int[] newBins)
        {
            if (newBins.Length > 7) throw new ArgumentOutOfRangeException("newBins", "Cannot have more then 8 bins");
            int[] updatedBins = new int[7];

            if (newBins.Length < updatedBins.Length)
            {
                newBins.CopyTo(updatedBins, 0);
                for (int i = updatedBins.Length - newBins.Length; i < 8; ++i)
                {
                    updatedBins[i] = newBins[newBins.Length - 1];
                }
            }
            else updatedBins = newBins;

            X3Packet updateBinsPacket = new X3Packet();
            updateBinsPacket.Header.Qualifier = 0xfb;
            updateBinsPacket.Payload.Length = 0x20;
            updateBinsPacket.HasResponse = true;

            //updatedBins.CopyTo(updateBinsPacket.Payload.Data, 0);

            byte[] binsAsBytes = Array.ConvertAll<int, byte>(updatedBins, Convert.ToByte);

            binsAsBytes.CopyTo(updateBinsPacket.Payload.Data, 0);

            byte[] PayloadBody = new byte[]{
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x04,
                0x00, 0x77, 0x40, 0x04, 0x80, 0x08,
                0x18, 0x05, 0x28, 0x0a, 0x00, 0x20,
                0x40
            };

            PayloadBody.CopyTo(updateBinsPacket.Payload.Data, 7);

            X3Packet response = SendMessage(updateBinsPacket)[0];

            SendSaveToFlash();

            FetchBinInformation();
        }

        /// <summary>
        /// Set the time on the sensor
        /// </summary>
        /// <param name="time">The time to set on the sensor</param>
        public void SetSensorTime(DateTime time)
        {
            X3Packet setTimePacket = new X3Packet();
            setTimePacket.Header.Qualifier = 0x23;
            setTimePacket.Payload.Length = 7;

            setTimePacket.Payload.Data[0] = (byte)time.Second;
            setTimePacket.Payload.Data[1] = (byte)time.Minute;
            setTimePacket.Payload.Data[2] = (byte)time.Hour;
            setTimePacket.Payload.Data[3] = (byte)TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Negate().Hours;
            //setTimePacket.Payload.Data[3] = 0x04;//delimiter? or offset
            //TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Negate().Hours
            setTimePacket.Payload.Data[4] = (byte)time.Day;
            setTimePacket.Payload.Data[5] = (byte)time.Month;
            setTimePacket.Payload.Data[2] = (byte)(time.Year - 2000);
            SendMessage(setTimePacket);
        }

        /// <summary>
        /// Synchronizes the time on the sensor with the time on this computer
        /// </summary>
        public void SynchSensorTime()
        {
            SetSensorTime(DateTime.Now);
        }

        /// <summary>
        /// Instructs the sensor to sample at a new duration
        /// </summary>
        /// <param name="newDurationSeconds">The new duration in seconds </param>
        public void UpdateDataInterval(int newDurationSeconds)
        {
            if (newDurationSeconds < 10) throw new ArgumentOutOfRangeException("newDurationSeconds", "Cannot be less than 10");
            if (newDurationSeconds > 900) throw new ArgumentOutOfRangeException("newDurationSeconds", "Cannot be greater than 900");

            SendMessage(new SaveBlockRequest(sensorId, 0x0f, newDurationSeconds) { HasResponse = false });

            FetchIntervalInformation();
        }

        /// <summary>
        /// Instructs the X3 to use the given baudRate on its serial port
        /// </summary>
        /// <param name="newRate">
        /// The new baudRate indication:
        ///     1 2400Bps
        ///     2 4800Bps
        ///     3 9600Bps
        ///     4 14400Bps
        ///     5 19200Bps
        ///     6 38400Bps
        ///     7 57600Bps
        ///     8 115200Bps
        /// </param>
        public void UpdateBaudRate(int newRate)
        {
            SendMessage(new SetBaudRateRequest(sensorId, newRate));
        }

        /// <summary>
        /// Sends the RTMS BufferFlush Command
        /// </summary>
        public void FlushRTMSBuffer()
        {
            SendMessage(new BufferFlush(sensorId));
        }

        /// <summary>
        /// Instructs the sensor to perform a self test
        /// </summary>
        /// <returns>The PolledBitResult of the transaction</returns>
        public PolledBitResult SelfTest()
        {
            try
            {
                return new PolledBitResult(SendMessage(new PolledBitRequest(sensorId))[0]);
            }
            catch (Exception e)
            {
                if (e.Message == "Invalid Software Information Packet") return SelfTest();
                else throw;
            }
        }

        /// <summary>
        /// Empties the log of request packets
        /// </summary>
        public void ClearRequestLog()
        {
            requestLog.Clear();
        }

        /// <summary>
        /// Empties the log of recieved packets
        /// </summary>
        public void ClearRecieveLog()
        {
            recieveLog.Clear();
        }

        /// <summary>
        /// Closes the messenger
        /// </summary>
        public void Close()
        {
            try
            {
                if (null != udpClient)
                {
                    udpClient.Close();
                    udpClient = null;
                }
                if (null != netStream)
                {
                    this.netStream.Dispose();
                    this.netStream = null;
                }
                if (null != tcpClient)
                {
                    tcpClient.Close();
                    tcpClient = null;
                }
                udpSentBytes = tcpSentBytes = udpRecvBytes = tcpRecvBytes = 0;
                ClearRecieveLog();
                ClearRequestLog();
            }
            catch
            {
                throw;
            }
            finally
            {
                GC.SuppressFinalize(this);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        #endregion

        #region Constructor

        public X3Messenger(IPEndPoint ipEndPoint, ProtocolType transportProtocol)
        {
            this.ipEndPoint = ipEndPoint;
            this.transportProtocol = transportProtocol;
            Initialize();
        }

        public X3Messenger(TcpClient existingClient)
        {
            this.transportProtocol = ProtocolType.Tcp;
            this.tcpClient = existingClient;
            Initialize();
        }

        public X3Messenger(UdpClient existingClient)
        {
            this.transportProtocol = ProtocolType.Udp;
            this.udpClient = existingClient;
            Initialize();
        }

        #endregion

        #region Destructor

        ~X3Messenger()
        {
            Close();
        }

        #endregion
    }

    #endregion
}
