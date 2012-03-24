using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Linq;

namespace Wavetronix
{
    namespace SS125
    {
        #region Classes

        public class RadarTuningParams
        {
            // Fields
            private float _defaultLoopSizeFactor;
            private float _durationMultiplier;
            private float _minGapMultiplier;
            private float _speedMultiplier;
            private float _thresholdMultiplier;
            private float _volumeMultiplier;

            // Methods
            public RadarTuningParams()
            {
                this._thresholdMultiplier = 0f;
                this._speedMultiplier = 100f;
                this._defaultLoopSizeFactor = 0f;
                this._durationMultiplier = 1f;
                this._volumeMultiplier = 100f;
                this._minGapMultiplier = 100f;
            }

            public RadarTuningParams(RadarTuningParams tuningParams)
            {
                if (tuningParams == null)
                {
                    this._thresholdMultiplier = 0f;
                    this._speedMultiplier = 100f;
                    this._defaultLoopSizeFactor = 0f;
                    this._durationMultiplier = 1f;
                    this._volumeMultiplier = 100f;
                    this._minGapMultiplier = 100f;
                }
                else
                {
                    this._thresholdMultiplier = tuningParams.ThresholdMultiplier;
                    this._speedMultiplier = tuningParams.SpeedMultiplier;
                    this._defaultLoopSizeFactor = tuningParams.DefaultLoopSizeFactor;
                    this._durationMultiplier = tuningParams.DurationMultiplier;
                    this._volumeMultiplier = tuningParams.VolumeMultiplier;
                    this._minGapMultiplier = tuningParams.MinGapMultiplier;
                }
            }

            // Properties
            public float DefaultLoopSizeFactor
            {
                get
                {
                    return this._defaultLoopSizeFactor;
                }
                set
                {
                    this._defaultLoopSizeFactor = value;
                }
            }

            public float DurationMultiplier
            {
                get
                {
                    return this._durationMultiplier;
                }
                set
                {
                    this._durationMultiplier = value;
                }
            }

            public float MinGapMultiplier
            {
                get
                {
                    return this._minGapMultiplier;
                }
                set
                {
                    this._minGapMultiplier = value;
                }
            }

            public float SpeedMultiplier
            {
                get
                {
                    return this._speedMultiplier;
                }
                set
                {
                    this._speedMultiplier = value;
                }
            }

            public float ThresholdMultiplier
            {
                get
                {
                    return this._thresholdMultiplier;
                }
                set
                {
                    this._thresholdMultiplier = value;
                }
            }

            public float VolumeMultiplier
            {
                get
                {
                    return this._volumeMultiplier;
                }
                set
                {
                    this._volumeMultiplier = value;
                }
            }
        }

        public class ActiveLaneInformation
        {
            // Fields
            private string _description;
            private char _direction;

            // Properties
            public string Description
            {
                get
                {
                    return this._description;
                }
                set
                {
                    this._description = value;
                }
            }

            public char Direction
            {
                get
                {
                    return this._direction;
                }
                set
                {
                    this._direction = value;
                }
            }
        }

        public class LaneInformation
        {
            // Fields
            private string _description;
            private char _direction;
            private int _state;

            // Properties
            public string Description
            {
                get
                {                    
                    return this._description;
                }
                set
                {
                    this._description = value;
                }
            }

            public char Direction
            {
                get
                {
                    return this._direction;
                }
                set
                {
                    this._direction = value;
                }
            }

            public int State
            {
                get
                {
                    return this._state;
                }
                set
                {
                    this._state = value;
                }
            }
        }

        public class GroupInformation
        {
            // Fields
            private string _description;
            private char _direction;
            private int[] _lanes = null;

            // Properties
            public string Description
            {
                get
                {
                    return this._description.Substring(0, this._description.IndexOf('\0'));
                }
                set
                {
                    this._description = value;
                }
            }

            public char Direction
            {
                get
                {
                    return this._direction;
                }
                set
                {
                    this._direction = value;
                }
            }

            public int[] Lanes
            {
                get
                {
                    return this._lanes;
                }
                set
                {
                    this._lanes = value;
                }
            }
        }

        public class BinData
        {
            // Fields
            private int[] _bins;
            private BinByType _type;

            // Methods
            public BinData(BinByType type, int bins)
            {
                this._type = type;
                this._bins = new int[bins];
            }

            // Properties
            public int[] Bins
            {
                get
                {
                    return this._bins;
                }
                set
                {
                    if (((value != null) && (this._bins != null)) && (value.Length == this._bins.Length))
                    {
                        value.CopyTo(this._bins, 0);
                    }
                }
            }

            public BinByType Type
            {
                get
                {
                    return this._type;
                }
                set
                {
                    this._type = value;
                }
            }
        }

        public class Timestamp
        {
            // Fields
            private DateTime _dt;
            private bool _invalidTime;
            private long _ticks;

            // Methods
            public Timestamp()
            {
                this._invalidTime = false;                
                this.SetDateTime(DateTime.Now);
            }

            public Timestamp(DateTime dt)
            {
                this._invalidTime = false;
                this.SetDateTime(dt);
            }

            public Timestamp(byte[] data)
            {
                this._invalidTime = false;
                this.SetDateTime(this.LoadDateTime(data));
            }

            public Timestamp(long ticksSince2000)
            {
                this._invalidTime = false;
                DateTime dt = new DateTime(this.BaseTicks + ticksSince2000);
                this.SetDateTime(dt);
            }

            public Timestamp(byte[] data, bool bInGMT)
            {
                this._invalidTime = false;
                DateTime dt = this.LoadDateTime(data);
                if (bInGMT)
                {
                    dt = dt.ToLocalTime();
                }
                this.SetDateTime(dt);
            }

            public Timestamp(DateTime dt, bool bInGMT)
            {
                this._invalidTime = false;
                if (bInGMT)
                {
                    dt = dt.ToLocalTime();
                }
                this.SetDateTime(dt);
            }

            public Timestamp(int y, int mo, int d, int h, int m, int s, int ms)
            {
                this._invalidTime = false;
                try
                {
                    DateTime dt = new DateTime(y, mo, d, h, m, s, ms);
                    this.SetDateTime(dt);
                }
                catch
                {
                    this._invalidTime = true;
                }
            }

            public Timestamp(int y, int mo, int d, int h, int m, int s, int ms, bool bInGMT)
            {
                this._invalidTime = false;
                try
                {
                    DateTime dt = new DateTime(y, mo, d, h, m, s, ms);
                    if (bInGMT)
                    {
                        dt = dt.ToLocalTime();
                    }
                    this.SetDateTime(dt);
                }
                catch
                {
                    this._invalidTime = true;
                }
            }

            public int CompareTimestamp(Timestamp timestamp)
            {
                if (timestamp.Ticks < this._ticks)
                {
                    return -1;
                }
                if (timestamp.Ticks > this._ticks)
                {
                    return 1;
                }
                return 0;
            }

            public string DayMoYearString()
            {
                string str = this.Day.ToString() + " ";
                switch (this.Month)
                {
                    case 1:
                        str = str + "Jan";
                        break;

                    case 2:
                        str = str + "Feb";
                        break;

                    case 3:
                        str = str + "Mar";
                        break;

                    case 4:
                        str = str + "Apr";
                        break;

                    case 5:
                        str = str + "May";
                        break;

                    case 6:
                        str = str + "Jun";
                        break;

                    case 7:
                        str = str + "Jul";
                        break;

                    case 8:
                        str = str + "Aug";
                        break;

                    case 9:
                        str = str + "Sep";
                        break;

                    case 10:
                        str = str + "Oct";
                        break;

                    case 11:
                        str = str + "Nov";
                        break;

                    case 12:
                        str = str + "Dec";
                        break;
                }
                return (str + " " + this.Year.ToString());
            }

            private int DaysToMonthConstant(int month)
            {
                if ((month >= 1) && (month <= 12))
                {
                    switch (month)
                    {
                        case 1:
                            return 0;

                        case 2:
                            return 0x1f;

                        case 3:
                            return 0x3b;

                        case 4:
                            return 90;

                        case 5:
                            return 120;

                        case 6:
                            return 0x97;

                        case 7:
                            return 0xb5;

                        case 8:
                            return 0xd4;

                        case 9:
                            return 0xf3;

                        case 10:
                            return 0x111;

                        case 11:
                            return 0x130;

                        case 12:
                            return 0x14e;
                    }
                }
                return -1;
            }

            private DateTime LoadDateTime(byte[] data)
            {
                if (data.Length >= 8)
                {
                    long num = data[0];
                    num = (num << 8) + data[1];
                    num = (num << 8) + data[2];
                    num = (num << 8) + data[3];
                    num = (num << 8) + data[4];
                    num = (num << 8) + data[5];
                    num = (num << 8) + data[6];
                    num = (num << 8) + data[7];
                    long num2 = 0x1ffe0000000000L;
                    long num3 = 0x1e000000000L;
                    long num4 = 0x1f00000000L;
                    long num5 = 0x7c00000L;
                    long num6 = 0x3f0000L;
                    long num7 = 0xfc00L;
                    long num8 = 0x3ffL;
                    int year = (int)((num & num2) >> 0x29);
                    int month = (int)((num & num3) >> 0x25);
                    int day = (int)((num & num4) >> 0x20);
                    int hour = (int)((num & num5) >> 0x16);
                    int minute = (int)((num & num6) >> 0x10);
                    int second = (int)((num & num7) >> 10);
                    int millisecond = (int)(num & num8);
                    float num16 = millisecond;
                    millisecond = (int)((num16 / 2048f) * 1000f);
                    if (((year > 0) && (month > 0)) && (day > 0))
                    {
                        return new DateTime(year, month, day, hour, minute, second, millisecond);
                    }
                    return new DateTime(0x7d5, 1, 2, 0, 0, 0, 0);
                }
                return DateTime.Now;
            }

            public string MMDDYYYY(char delimeter)
            {
                return string.Format("{1:00}{0}{2:00}{0}{3:0000}", new object[] { delimeter, this.Month, this.Day, this.Year });
            }

            public string MonthDayYearString()
            {
                string str = "";
                switch (this.Month)
                {
                    case 1:
                        str = "January";
                        break;

                    case 2:
                        str = "February";
                        break;

                    case 3:
                        str = "March";
                        break;

                    case 4:
                        str = "April";
                        break;

                    case 5:
                        str = "May";
                        break;

                    case 6:
                        str = "June";
                        break;

                    case 7:
                        str = "July";
                        break;

                    case 8:
                        str = "August";
                        break;

                    case 9:
                        str = "September";
                        break;

                    case 10:
                        str = "October";
                        break;

                    case 11:
                        str = "November";
                        break;

                    case 12:
                        str = "December";
                        break;
                }
                return ((str + " " + this.Day.ToString()) + ", " + this.Year.ToString());
            }

            public void SetDateTime(DateTime dt)
            {
                this._dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
                this._ticks = this._dt.Ticks - this.BaseTicks;
            }

            public string TimestampString()
            {
                return string.Format("{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}:{6:000}", new object[] { this.Year, this.Month, this.Day, this.Hour, this.Minute, this.Second, this.Millisecond });
            }

            public string TimestampString(Fields[] fields)
            {
                string str = "";
                for (int i = 0; i < fields.Length; ++i)
                {
                    string str2 = "";
                    Fields undefined = Fields.undefined;
                    Fields fields3 = Fields.undefined;
                    undefined = fields[i];
                    if (i < (fields.Length - 1))
                    {
                        fields3 = fields[i + 1];
                    }
                    if (((fields3 == Fields.day) || (fields3 == Fields.mo)) || (fields3 == Fields.yr))
                    {
                        str2 = "-";
                    }
                    else if ((((undefined == Fields.day) || (undefined == Fields.mo)) || (undefined == Fields.yr)) && (((fields3 == Fields.hr) || (fields3 == Fields.min)) || ((fields3 == Fields.sec) || (fields3 == Fields.ms))))
                    {
                        str2 = " ";
                    }
                    else if (((fields3 == Fields.hr) || (fields3 == Fields.min)) || ((fields3 == Fields.sec) || (fields3 == Fields.ms)))
                    {
                        str2 = ":";
                    }
                    switch (fields[i])
                    {
                        case Fields.yr:
                            str = str + string.Format("{0:0000}", this.Year);
                            break;

                        case Fields.mo:
                            str = str + string.Format("{0:00}", this.Month);
                            break;

                        case Fields.day:
                            str = str + string.Format("{0:00}", this.Day);
                            break;

                        case Fields.hr:
                            str = str + string.Format("{0:00}", this.Hour);
                            break;

                        case Fields.min:
                            str = str + string.Format("{0:00}", this.Minute);
                            break;

                        case Fields.sec:
                            str = str + string.Format("{0:00}", this.Second);
                            break;

                        case Fields.ms:
                            str = str + string.Format("{0:000}", this.Millisecond);
                            break;
                    }
                    str = str + str2;
                }
                return str;
            }

            public string TimeString(bool military, bool showMilliseconds)
            {
                string str;
                int hour = this.Hour;
                if (!military && (this.Hour > 12))
                {
                    hour = this.Hour - 12;
                }
                str = str = string.Format("{0:00}:{1:00}:{2:00}", hour, this.Minute, this.Second);
                if (showMilliseconds)
                {
                    str = str + string.Format(":{0:000}", this.Millisecond);
                }
                return str;
            }

            public Timestamp ToUniversalTime()
            {
                DateTime time = new DateTime(this.Year, this.Month, this.Day, this.Hour, this.Minute, this.Second, this.Millisecond);
                return new Timestamp(time.ToUniversalTime());
            }

            public string YYYYMMDD(char delimeter)
            {
                return string.Format("{1:0000}{0}{2:00}{0}{3:00}", new object[] { delimeter, this.Year, this.Day, this.Month });
            }

            // Properties
            public long BaseTicks
            {
                get
                {
                    return DateTime.Parse("1/1/2000 00:00:00").Ticks;
                }
            }

            public int Day
            {
                get
                {
                    return this._dt.Day;
                }
            }

            public int DaysSince2000
            {
                get
                {
                    return (((((this._dt.Year - 0x7d0) * 0x16d) - ((this._dt.Year - 0x7d0) / 4)) + this.DaysToMonthConstant(this._dt.Month)) + this._dt.Day);
                }
            }

            public int Hour
            {
                get
                {
                    return this._dt.Hour;
                }
            }

            public bool IsTimeValid
            {
                get
                {
                    return !this._invalidTime;
                }
            }

            public long Long
            {
                get
                {
                    long year = this._dt.Year;
                    year = (year << 4) + (15 & this._dt.Month);
                    year = (year << 5) + (0x1f & this._dt.Day);
                    year = year << 5;
                    year = (year << 5) + (0x1f & this._dt.Hour);
                    year = (year << 6) + (0x3f & this._dt.Minute);
                    year = (year << 6) + (0x3f & this._dt.Second);
                    return ((year << 10) + (0x3ff & this._dt.Millisecond));
                }
                set
                {
                    long num = value;
                    int year = (int)((num & 0x1ffe0000000000L) >> 0x29);
                    int month = (int)((num & 0x1e000000000L) >> 0x25);
                    int day = (int)((num & 0x1f00000000L) >> 0x20);
                    int hour = (int)((num & 0x7c00000L) >> 0x16);
                    int minute = (int)((num & 0x3f0000L) >> 0x10);
                    int second = (int)((num & 0xfc00L) >> 10);
                    int millisecond = (int)(num & 0x3ffL);
                    DateTime dt = new DateTime(year, month, day, hour, minute, second, millisecond);
                    this.SetDateTime(dt);
                }
            }

            public int Millisecond
            {
                get
                {
                    return this._dt.Millisecond;
                }
            }

            public int MillisecondsToday
            {
                get
                {
                    return ((((this._dt.Hour * 0x36ee80) + (this._dt.Minute * 0xea60)) + (this._dt.Second * 0x3e8)) + this._dt.Millisecond);
                }
            }

            public int Minute
            {
                get
                {
                    return this._dt.Minute;
                }
            }

            public int Month
            {
                get
                {
                    return this._dt.Month;
                }
            }

            public int Second
            {
                get
                {
                    return this._dt.Second;
                }
                set
                {
                    int year = this._dt.Year;
                    int month = this._dt.Month;
                    int day = this._dt.Day;
                    int hour = this._dt.Hour;
                    int minute = this._dt.Minute;
                    int second = value;
                    int millisecond = this.Millisecond;
                    this._dt = new DateTime(year, month, day, hour, minute, second, millisecond);
                }
            }

            public long Ticks
            {
                get
                {
                    return this._ticks;
                }
            }

            public long TicksAsMilliseconds
            {
                get
                {
                    return (this._ticks / 0x2710L);
                }
            }

            public int Year
            {
                get
                {
                    return this._dt.Year;
                }
            }

            public DateTime UniversalDateTime
            {
                get
                {
                    return _dt.ToUniversalTime();
                }
            }

            // Nested Types
            public enum Fields
            {
                yr,
                mo,
                day,
                hr,
                min,
                sec,
                ms,
                undefined
            }

            public static implicit operator Timestamp(DateTime dt)
            {
                Timestamp t = new Timestamp(dt);                
                return t;
            }
        }

        public class SensorId
        {
            // Fields
            private int _id = 0x102;
            private byte _subnet = 0;

            // Properties
            public byte[] DataPush
            {
                get
                {
                    return new byte[] { ((byte)(this._subnet & 0xff)), ((byte)((this._id & 0xff00) >> 8)), ((byte)(this._id & 0xff)) };
                }
                set
                {
                    if (value.Length == 3)
                    {
                        this._subnet = value[0];
                        this._id = (value[1] << 8) + value[2];
                    }
                }
            }

            public int Id
            {
                get
                {
                    return this._id;
                }
                set
                {
                    this._id = value & 0xffff;
                }
            }

            public byte Subnet
            {
                get
                {
                    return this._subnet;
                }
                set
                {
                    this._subnet = value;
                }
            }
        }

        public class DataPushConfiguration
        {
            // Fields
            private DataFormat _dataFormat = DataFormat.Z1;
            private DataPort _outputPort = DataPort.rs232;
            private bool _pushEnabled = false;
            private SensorId _sensorId = new SensorId();

            // Properties
            public DataFormat DataFormat
            {
                get
                {
                    return this._dataFormat;
                }
                set
                {
                    this._dataFormat = value;
                }
            }

            public DataPort OutputPort
            {
                get
                {
                    return this._outputPort;
                }
                set
                {
                    this._outputPort = value;
                }
            }

            public byte[] PushConfig
            {
                get
                {
                    byte[] buffer = new byte[6];
                    byte num = 0;
                    if (this._outputPort == DataPort.rs232)
                    {
                        num = 1;
                    }
                    byte num2 = 0;
                    if (this._dataFormat == DataFormat.Z0_Simple)
                    {
                        num2 = 1;
                    }
                    else if (this._dataFormat == DataFormat.Z0_Multidrop)
                    {
                        num2 = 2;
                    }
                    else if (this._dataFormat == DataFormat.RTMS)
                    {
                        num2 = 3;
                    }
                    byte num3 = 0;
                    if (this._pushEnabled)
                    {
                        num3 = 1;
                    }
                    buffer[0] = num;
                    buffer[1] = num2;
                    buffer[2] = num3;
                    byte[] dataPush = this._sensorId.DataPush;
                    buffer[3] = dataPush[0];
                    buffer[4] = dataPush[1];
                    buffer[5] = dataPush[2];
                    return buffer;
                }
                set
                {
                    byte num = 0;
                    byte num3 = 0;
                    num = value[0];
                    if (num == 1)
                    {
                        this._outputPort = DataPort.rs232;
                    }
                    else
                    {
                        this._outputPort = DataPort.rs485;
                    }
                    switch (value[1])
                    {
                        case 1:
                            this._dataFormat = DataFormat.Z0_Simple;
                            break;

                        case 2:
                            this._dataFormat = DataFormat.Z0_Multidrop;
                            break;

                        case 3:
                            this._dataFormat = DataFormat.RTMS;
                            break;

                        default:
                            this._dataFormat = DataFormat.Z1;
                            break;
                    }
                    num3 = value[2];
                    if (num3 == 1)
                    {
                        this._pushEnabled = true;
                    }
                    else
                    {
                        this._pushEnabled = false;
                    }
                    this._sensorId.DataPush = new byte[] { value[3], value[4], value[5] };
                }
            }

            public bool PushEnabled
            {
                get
                {
                    return this._pushEnabled;
                }
                set
                {
                    this._pushEnabled = value;
                }
            }

            public SensorId PushReturnId
            {
                get
                {
                    return this._sensorId;
                }
                set
                {
                    this._sensorId = value;
                }
            }
        }

        #endregion

        #region Enums

        public enum DataFormat
        {
            Z1,
            Z0_Simple,
            Z0_Multidrop,
            RTMS
        }

        public enum DataPort
        {
            rs485,
            rs232
        }

        public enum IntervalModeValue
        {
            Off,
            Loop,
            FillOnce
        }

        public enum AccessType
        {
            Read,
            Write,
            Listen
        }

        public enum DataType
        {
            PushData = 7,
            SavedAll = 6,
            SavedApproach = 5,
            SavedLane = 4,
            VolatileAll = 3,
            VolatileApproach = 2,
            VolatileLane = 1
        }        

        public enum BinType
        {
            undefined,
            aLane,
            sLane,
            boundary,
            excluded,
            inactive
        }

        public enum BinByType
        {
            direction = 3,
            length = 1,
            speed = 2
        }

        public enum LaneDirection
        {
            undefined,
            left,
            right
        }

        public enum LaneState
        {
            undefined,
            active,
            inactive,
            excluded,
            auto
        }
        
        public enum BroadcastType
        {
            broadcast,
            tempId,
            push
        }
        
        public enum BroadcastState
        {
            off,
            on
        }

        #endregion

        #region Structures

        [StructLayout(LayoutKind.Sequential)]
        public struct LaneStruct
        {
            public LaneState _state;
            public LaneDirection _direction;
            public double _nearRange;
            public double _farRange;
            public LaneStruct(LaneState state, LaneDirection dir, double near, double far)
            {
                this._state = state;
                this._direction = dir;
                this._nearRange = near;
                this._farRange = far;
            }
        }

        #endregion

        #region Z1Protocol

        /// <summary>
        /// A Managed Implementation of the CRC8
        /// </summary>
        public class Z1CRC
        {
            #region Fields

            #region Static

            private static byte[] crcTable = new byte[]{
                0x00,0x1C,0x38,0x24,0x70,0x6C,0x48,0x54,0xE0,0xFC,0xD8,0xC4,0x90,0x8C,0xA8,0xB4,
                0xDC,0xC0,0xE4,0xF8,0xAC,0xB0,0x94,0x88,0x3C,0x20,0x04,0x18,0x4C,0x50,0x74,0x68,
                0xA4,0xB8,0x9C,0x80,0xD4,0xC8,0xEC,0xF0,0x44,0x58,0x7C,0x60,0x34,0x28,0x0C,0x10,
                0x78,0x64,0x40,0x5C,0x08,0x14,0x30,0x2C,0x98,0x84,0xA0,0xBC,0xE8,0xF4,0xD0,0xCC,
                0x54,0x48,0x6C,0x70,0x24,0x38,0x1C,0x00,0xB4,0xA8,0x8C,0x90,0xC4,0xD8,0xFC,0xE0,
                0x88,0x94,0xB0,0xAC,0xF8,0xE4,0xC0,0xDC,0x68,0x74,0x50,0x4C,0x18,0x04,0x20,0x3C,
                0xF0,0xEC,0xC8,0xD4,0x80,0x9C,0xB8,0xA4,0x10,0x0C,0x28,0x34,0x60,0x7C,0x58,0x44, 
                0x2C,0x30,0x14,0x08,0x5C,0x40,0x64,0x78,0xCC,0xD0,0xF4,0xE8,0xBC,0xA0,0x84,0x98,
                0xA8,0xB4,0x90,0x8C,0xD8,0xC4,0xE0,0xFC,0x48,0x54,0x70,0x6C,0x38,0x24,0x00,0x1C,
                0x74,0x68,0x4C,0x50,0x04,0x18,0x3C,0x20,0x94,0x88,0xAC,0xB0,0xE4,0xF8,0xDC,0xC0,
                0x0C,0x10,0x34,0x28,0x7C,0x60,0x44,0x58,0xEC,0xF0,0xD4,0xC8,0x9C,0x80,0xA4,0xB8,
                0xD0,0xCC,0xE8,0xF4,0xA0,0xBC,0x98,0x84,0x30,0x2C,0x08,0x14,0x40,0x5C,0x78,0x64,
                0xFC,0xE0,0xC4,0xD8,0x8C,0x90,0xB4,0xA8,0x1C,0x00,0x24,0x38,0x6C,0x70,0x54,0x48,
                0x20,0x3C,0x18,0x04,0x50,0x4C,0x68,0x74,0xC0,0xDC,0xF8,0xE4,0xB0,0xAC,0x88,0x94,
                0x58,0x44,0x60,0x7C,0x28,0x34,0x10,0x0C,0xB8,0xA4,0x80,0x9C,0xC8,0xD4,0xF0,0xEC,
                0x84,0x98,0xBC,0xA0,0xF4,0xE8,0xCC,0xD0,0x64,0x78,0x5C,0x40,0x14,0x08,0x2C,0x30
            };

            #endregion

            private byte _value = 0;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructs a new Z1CRC from the given data
            /// </summary>
            /// <param name="data">The data to calculate the CRC8 of</param>
            public Z1CRC(byte[] data)
            {
                this.CalculateCRC(data);
            }

            #endregion

            #region Methods
            
            /// <summary>
            /// Calculates a CRC8 from the given data
            /// </summary>
            /// <param name="arr">The data to calculate the CRC8 of</param>
            public void CalculateCRC(byte[] arr)
            {
                byte crc = 0;
                for (int i = 0, e = arr.Length; i < e; ++i)
                {
                    crc = this.CRC8(arr[i], crc);
                }
                this._value = crc;
            }

            /// <summary>
            /// Returns the CRC8 of the given byte
            /// </summary>
            /// <param name="data">The data to lookup the CRC key for</param>
            /// <param name="crc">The CRC value for the byte</param>
            /// <returns>The CRC from the CRC Table which corresponds to the given byte</returns>
            private byte CRC8(byte data, byte crc)
            {
                int index = data ^ crc;
                crc = Z1CRC.crcTable[index];
                return crc;
            }

            #endregion

            #region Properties

            /// <summary>
            /// The byte value of the calculated CRC8
            /// </summary>
            public byte Value
            {
                get
                {
                    return this._value;
                }
            }            

            #endregion
        }

        /// <summary>
        /// A Managed Implementation of the Wavetronix Z1Header
        /// </summary>
        public class Z1Header
        {
            #region Fields

            private byte[] _buffer;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructs a new Z1Header
            /// </summary>
            public Z1Header()
            {
                this._buffer = new byte[11];
                this._buffer[0] = 90;
                this._buffer[1] = 0x31;
                this._buffer[2] = 0;
                this._buffer[3] = 0;
                this._buffer[4] = 0;
                this._buffer[5] = 0;
                this._buffer[6] = 0;
                this._buffer[7] = 0;
                this._buffer[8] = 0;
                this._buffer[9] = 0;
                byte[] data = new byte[10];
                for (int i = 0; i < 10; ++i) data[i] = this._buffer[i];
                Z1CRC zcrc = new Z1CRC(data);
                this._buffer[10] = zcrc.Value;
            }

            /// <summary>
            /// Constructs a new Z1Header from an Array of Byte
            /// </summary>
            /// <param name="header">The Byte Array containing the raw Z1Header</param>
            public Z1Header(byte[] header)
            {
                this._buffer = new byte[11];
                this._buffer[0] = 90;
                this._buffer[1] = 0x31;
                for (int i = 2; i < 11; ++i)
                {
                    this._buffer[i] = 0;
                    if (i < header.Length)
                    {
                        this._buffer[i] = header[i];
                    }
                }
            }

            /// <summary>
            /// Creates a new Z1Header from the given values
            /// </summary>
            /// <param name="destSubID">The “Destination Sub ID” is used like a network subnet. By changing this value on a sensor you can create logical subgroups of sensors. A Destination Sub ID of 255 (0xFF) will result in the message going to every subnet (i.e. a broadcast).</param>
            /// <param name="destID"> The “Destination ID” is the identification of the intended target device. An ID of “65535” (0xFFFF) is interpreted as a broadcast to all devices. Broadcast commands can be muted for each individual sensor and can be enabled on all sensors simultaneously. An ID of “0” is reserved for host devices, such as a data collector, and is not allowed on a sensor.</param>
            /// <param name="sourceSubID">The “Source Sub ID” is the subnet on which the sending device is located. A Source Sub ID should never be a Broadcast Sub ID (0xFF).</param>
            /// <param name="sourceID">The “Source ID” is the identification of the device sending the message. A source ID should never be a broadcast ID (0xFFFF).</param>
            /// <param name="seqNum">The Sequence Number of the Z1Header</param>
            /// <param name="payloadSize">The size of the Z1Payload which this Z1Header will preceed</param>
            public Z1Header(byte destSubID, ushort destID, byte sourceSubID, ushort sourceID, byte seqNum, byte payloadSize)
            {
                this._buffer = new byte[11];
                this._buffer[0] = 90;
                this._buffer[1] = 0x31;
                this._buffer[2] = destSubID;
                this._buffer[3] = (byte)(destID >> 8);
                this._buffer[4] = (byte)(destID & 0xff);
                this._buffer[5] = sourceSubID;
                this._buffer[6] = (byte)(sourceID >> 8);
                this._buffer[7] = (byte)(sourceID & 0xff);
                this._buffer[8] = seqNum;
                this._buffer[9] = payloadSize;
                byte[] data = new byte[10];
                for (int i = 0; i < 10; ++i) data[i] = this._buffer[i];
                Z1CRC zcrc = new Z1CRC(data);
                this._buffer[10] = zcrc.Value;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Calculates a CRC for this Z1Header and stores it in the Buffer
            /// </summary>
            public void GenerateCRC()
            {
                int bufferLength = this._buffer.Length - 1;
                byte[] data = new byte[bufferLength];
                for (int i = 0; i < bufferLength; ++i) data[i] = this._buffer[i];
                Z1CRC zcrc = new Z1CRC(data);
                this._buffer[this._buffer.Length - 1] = zcrc.Value;
            }

            /// <summary>
            /// Determines if the Generated CRC of this Z1Header is valid
            /// </summary>
            /// <returns>true if the header is valid, otherwise false</returns>
            public bool ValidateCRC()
            {
                if (this._buffer.Length >= 11)
                {
                    byte[] data = new byte[10];
                    for (int i = 0; i < 10; ++i) data[i] = this._buffer[i];
                    Z1CRC zcrc = new Z1CRC(data);
                    if (zcrc.Value == this.CRC)
                    {
                        return true;
                    }
                }
                return false;
            }

            #endregion

            #region Properties

            /// <summary>
            /// The bytes which make up this Z1Header
            /// </summary>
            public byte[] Buffer
            {
                get
                {
                    return this._buffer;
                }
                set
                {
                    this._buffer = new byte[11];
                    for (int i = 0; i < 11; ++i)
                    {
                        this._buffer[i] = 0;
                        if (i < value.Length) this._buffer[i] = value[i];
                    }
                }
            }

            /// <summary>
            /// The CRC8 Checksum for this Z1Header
            /// </summary>
            public byte CRC
            {
                get
                {
                    return this._buffer[10];
                }
                set
                {
                    this._buffer[10] = value;
                }
            }

            /// <summary>
            /// The “Destination ID” is the identification of the intended target device. An ID of “65535” (0xFFFF) is interpreted as a broadcast to all devices. Broadcast commands can be muted for each individual sensor and can be enabled on all sensors simultaneously. An ID of “0” is reserved for host devices, such as a data collector, and is not allowed on a sensor.
            /// </summary>
            public ushort DestinationID
            {
                get
                {
                    ushort num = this._buffer[3];
                    num = (ushort)(num << 8);
                    return (num = (ushort)(num + this._buffer[4]));
                }
                set
                {
                    this._buffer[3] = (byte)(value >> 8);
                    this._buffer[4] = (byte)(value & 0xff);
                }
            }
            /// <summary>
            /// The “Destination Sub ID” is used like a network subnet. By changing this value on a sensor you can create logical subgroups of sensors. A Destination Sub ID of 255 (0xFF) will result in the message going to every subnet (i.e. a broadcast).
            /// </summary>
            public byte DestinationSubID
            {
                get
                {
                    return this._buffer[2];
                }
                set
                {
                    this._buffer[2] = value;
                }
            }

            /// <summary>
            /// The length in bytes of this Z1Header (Always 11)
            /// </summary>
            public int Length
            {
                get
                {
                    return 11;
                }
            }

            /// <summary>
            /// The size of the Z1Payload to which this header belongs
            /// </summary>
            public byte PayloadSize
            {
                get
                {
                    return (byte)(this._buffer[9] + 1);
                }
                set
                {
                    this._buffer[9] = value;
                }
            }

            /// <summary>
            /// The Sequence number of Z1 Header which is incremented every request
            /// </summary>
            public byte SequenceNumber
            {
                get
                {
                    return this._buffer[8];
                }
                set
                {
                    this._buffer[8] = value;
                }
            }

            /// <summary>
            /// The “Source ID” is the identification of the device sending the message. A source ID should never be a broadcast ID (0xFFFF). 
            /// </summary>
            public ushort SourceID
            {
                get
                {
                    ushort num = this._buffer[6];
                    num = (ushort)(num << 8);
                    return (num = (ushort)(num + this._buffer[7]));
                }
                set
                {
                    this._buffer[6] = (byte)(value >> 8);
                    this._buffer[7] = (byte)(value & 0xff);
                }
            }

            /// <summary>
            /// The “Source Sub ID” is the subnet on which the sending device is located. A Source Sub ID should never be a Broadcast Sub ID (0xFF).
            /// </summary>
            public byte SourceSubID
            {
                get
                {
                    return this._buffer[5];
                }
                set
                {
                    this._buffer[5] = value;
                }
            }

            /// <summary>
            /// The Version of the Wavetronix Protocol This Z1Header conforms to (Always "Z1" or 0x5a39)
            /// </summary>
            public ushort Version
            {
                get
                {
                    return 0x5a39;
                }
            }

            #endregion
        }

        /// <summary>
        /// A Managed Implementation of the Wavetronix Z1Payload
        /// </summary>
        public class Z1Payload
        {
            #region Fields

            private byte[] _buffer;
            private byte[] _data;            

            #endregion

            #region Constructor

            /// <summary>
            /// Creates a new Z1Payload
            /// </summary>
            public Z1Payload()
            {
                this._buffer = new byte[4];
            }           

            /// <summary>
            /// Creates a new Z1Payload from a Array of Byte
            /// </summary>
            /// <param name="payload">The Byte Array which contains the Z1Payload</param>
            public Z1Payload(byte[] payload)
            {
                if ((payload != null) && (payload.Length >= 4))
                {
                    this._buffer = new byte[payload.Length];
                    payload.CopyTo(this._buffer, 0);
                    if (payload.Length > 4)
                    {
                        this._data = new byte[payload.Length - 4];
                        for (int i = 0; i < this._data.Length; ++i) this._data[i] = payload[i + 3];
                    }
                }
                else
                {
                    this._buffer = new byte[4];
                }
            }

            /// <summary>
            /// Creates a new Z1Payload from the given values
            /// </summary>
            /// <param name="id">The Id of the Message</param>
            /// <param name="subid">The SubID of the Message</param>
            /// <param name="access">The accessType of the Message</param>
            /// <param name="data">The raw data of the Message</param>
            public Z1Payload(byte id, byte subid, byte access, byte[] data)
            {
                if (data != null)
                {
                    this._buffer = new byte[data.Length + 4];
                    this._buffer[0] = id;
                    this._buffer[1] = subid;
                    this._buffer[2] = access;
                    for (int i = 0; i < data.Length; ++i) this._buffer[i + 3] = data[i];
                }
                else
                {
                    byte[] buffer = new byte[4];
                    buffer[0] = id;
                    buffer[1] = subid;
                    buffer[2] = access;
                    this._buffer = buffer;
                }
            }

            #endregion

            #region Methods

            /// <summary>
            /// Generates a CRC8 for this Z1Payload
            /// </summary>
            public void GenerateCRC()
            {
                int bufferLength = this._buffer.Length - 1;
                byte[] data = new byte[bufferLength];
                for (int i = 0; i < bufferLength; ++i) data[i] = this._buffer[i];
                Z1CRC zcrc = new Z1CRC(data);
                this._buffer[this._buffer.Length - 1] = zcrc.Value;
            }

            /// <summary>
            /// Validates the CRC8 in this Z1Payload
            /// </summary>
            /// <returns>true if the Z1Payload CRC is valid, otherwise false</returns>
            public bool ValidateCRC()
            {
                int bufferLength = this._buffer.Length - 1;
                byte[] data = new byte[bufferLength];
                for (int i = 0; i < bufferLength; ++i) data[i] = this._buffer[i];
                Z1CRC zcrc = new Z1CRC(data);
                return (this.CRC == zcrc.Value);
            }

            #endregion

            #region Properties

            /// <summary>
            /// The Access Type of this Z1Payload
            /// </summary>
            public AccessType Access
            {
                get
                {
                    if ((this._buffer != null) && (this._buffer.Length >= 3))
                    {
                        if (this._buffer[2] == 1)
                        {
                            return AccessType.Write;
                        }
                        if (this._buffer[2] == 2)
                        {
                            return AccessType.Listen;
                        }
                    }
                    return AccessType.Read;
                }
                set
                {
                    if ((this._buffer != null) && (this._buffer.Length >= 3))
                    {
                        this._buffer[2] = 0;
                        if (value == AccessType.Write)
                        {
                            this._buffer[2] = 1;
                        }
                        if (value == AccessType.Listen)
                        {
                            this._buffer[2] = 2;
                        }
                    }
                }
            }

            /// <summary>
            /// The Binary data of this Z1Payload
            /// </summary>
            public byte[] Buffer
            {
                get
                {
                    if (this._data != null)
                    {
                        if (this._buffer.Length < (this._data.Length + 4))
                        {
                            byte num = this._buffer[0];
                            byte num2 = this._buffer[1];
                            byte num3 = this._buffer[2];
                            this._buffer = new byte[this._data.Length + 4];
                            this._buffer[0] = num;
                            this._buffer[1] = num2;
                            this._buffer[2] = num3;
                        }
                        this._data.CopyTo(this._buffer, 3);
                    }
                    return this._buffer;
                }
            }

            /// <summary>
            /// The CRC8 of this Z1Payload
            /// </summary>
            public byte CRC
            {
                get
                {
                    if ((this._buffer != null) && (this._buffer.Length >= 4))
                    {
                        return this._buffer[this._buffer.Length - 1];
                    }
                    return 0;
                }
                set
                {
                    if ((this._buffer != null) && (this._buffer.Length >= 4))
                    {
                        this._buffer[this._buffer.Length - 1] = value;
                    }
                }
            }

            /// <summary>
            /// The Binary Data of this Z1Payload
            /// </summary>
            public byte[] Data
            {
                get
                {
                    if (this._data != null)
                    {
                        return this._data;
                    }
                    return null;
                }
                set
                {
                    this._data = value;
                }
            }

            /// <summary>
            /// The Length of Binary Data of this Z1Payload
            /// </summary>
            public int Length
            {
                get
                {
                    if (this.Buffer != null)
                    {
                        return this.Buffer.Length;
                    }
                    return 0;
                }
            }

            /// <summary>
            /// The Message ID of this Z1Payload
            /// </summary>
            public byte MessageID
            {
                get
                {
                    if (this._buffer != null)
                    {
                        return this._buffer[0];
                    }
                    return 0;
                }
                set
                {
                    this._buffer[0] = value;
                }
            }

            /// <summary>
            /// The Message Sub ID of this Z1Payload
            /// </summary>
            public byte MessageSubID
            {
                get
                {
                    if ((this._buffer != null) && (this._buffer.Length >= 2))
                    {
                        return this._buffer[1];
                    }
                    return 0;
                }
                set
                {
                    if ((this._buffer != null) && (this._buffer.Length >= 2))
                    {
                        this._buffer[1] = value;
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// A Managed Implementation of the Wavetronix Z1Packet
        /// </summary>
        public class Z1Packet
        {
            #region Fields

            private Z1Header _header;
            private Z1Payload _payload;
            private int _addedTimeout;
            private bool _hasResponse = true;
            private bool _hasMultiResponse;

            #endregion

            #region Properties

            public int MessageResult
            {
                get
                {
                    if ((this.Payload == null) || (this.Payload.Data == null))
                    {
                        return 0x270f;
                    }
                    if (this.Payload.Data.Length == 2)
                    {
                        int num = this.Payload.Data[0];
                        num = num << 8;
                        return (num + this.Payload.Data[1]);
                    }
                    return -1;
                }
            }

            public int AddedTimeout
            {
                get
                {
                    return _addedTimeout;
                }
                set
                {
                    _addedTimeout = value;
                }
            }

            public bool HasResponse
            {
                get
                {
                    return _hasResponse;
                }
                set
                {
                    _hasResponse = value;
                    if (value == false) _hasMultiResponse = value;
                }
            }

            public bool HasMultipleResponses
            {
                get
                {
                    return _hasMultiResponse;
                }
                set
                {
                    if (value == true) _hasResponse = value;
                    _hasMultiResponse = value;
                }
            }

            #endregion

            #region Constructor

            /// <summary>
            /// Constructs a new Z1Packet
            /// </summary>
            public Z1Packet()
            {
                this._header = new Z1Header()
                {
                    DestinationSubID = 0xff,
                    DestinationID = 0xffff                    
                };
                this._payload = new Z1Payload();
            }

            public Z1Packet(Z1Packet packet)
            {
                if (null == packet) return;
                this.Header = packet.Header;
                this.Payload = packet.Payload;
            }

            public Z1Packet(List<Z1Packet> packets)
                : this(packets.FirstOrDefault()) { }

            #endregion

            #region Properties

            /// <summary>
            /// Returns the bytes of this Z1Packet
            /// </summary>
            public byte[] Buffer
            {
                get
                {
                    if (this.Length > 0)
                    {
                        byte[] array = new byte[this.Length];
                        this._header.Buffer.CopyTo(array, 0);
                        this._payload.Buffer.CopyTo(array, this._header.Buffer.Length);
                        return array;
                    }
                    return null;
                }
            }

            /// <summary>
            /// The Z1Header of the Z1Packet
            /// </summary>
            public Z1Header Header
            {
                get
                {
                    return this._header;
                }
                set
                {
                    if ((value != null) && (value.Buffer.Length >= 4))
                    {
                        this._header = new Z1Header(value.Buffer);
                        if ((this._payload != null) && (this._payload.Buffer.Length >= 4))
                        {
                            this._header.PayloadSize = (byte)(this._payload.Buffer.Length - 1);
                        }
                    }
                }
            }

            /// <summary>
            /// The Length of the Z1Packet
            /// </summary>
            public int Length
            {
                get
                {
                    int num = 0;
                    if ((this._header == null) || (this._header.Length != 11))
                    {
                        return num;
                    }
                    num = 11;
                    if ((this._payload != null) && (this._payload.Length >= 4))
                    {
                        return (num + this._payload.Length);
                    }
                    return 0;
                }
            }

            /// <summary>
            /// The Z1Payload of the Z1Packet
            /// </summary>
            public Z1Payload Payload
            {
                get
                {
                    return this._payload;
                }
                set
                {
                    if ((value != null) && (value.Buffer.Length >= 4))
                    {
                        this._payload = new Z1Payload(value.Buffer);
                        if ((this._header != null) && (this._payload.Buffer.Length >= 4))
                        {
                            this._header.PayloadSize = (byte)(this._payload.Buffer.Length - 1);
                        }
                    }
                }
            }

            #endregion

            #region Methods
            
            protected string CreateString(byte[] buffer, int bufferOffset, int length)
            {
                if ((bufferOffset + length) > buffer.Length)
                {
                    return string.Empty;
                }
                string str = string.Empty;
                int end = bufferOffset + length;
                for (int i = bufferOffset; i < end; ++i) str = str + ((char)buffer[i]);
                return str;
            }

            protected float CreateFloat(byte[] buffer, int bufferOffset, int length)
            {
                if ((bufferOffset + length) > buffer.Length)
                {
                    return -1f;
                }
                float num = 0f;
                if (length == 2)
                {
                    num = buffer[bufferOffset];
                    return (num + (((float)buffer[bufferOffset + 1]) / 256f));
                }
                if (length != 3)
                {
                    throw new Exception("Unsupported conversion to float for SS125 Driver");
                }
                int num2 = buffer[bufferOffset] << 8;
                num2 += buffer[bufferOffset + 1];
                num2 &= 0x7fff;
                int num3 = 0x4000;
                if ((num2 & num3) != 0)
                {
                    num2 ^= 0x3fff;
                    num2++;
                    num2 &= 0x3fff;
                }
                return (num2 + (((float)buffer[bufferOffset + 2]) / 256f));
            }

            protected float CreateFloat(byte[] buffer, int bufferOffset, int length, ref bool valid, ref bool negative)
            {
                if ((bufferOffset + length) > buffer.Length)
                {
                    return -1f;
                }
                if (length != 3)
                {
                    throw new Exception("Unsupported conversion to float for SS125 Driver");
                }
                int num2 = buffer[bufferOffset] << 8;
                num2 += buffer[bufferOffset + 1];
                int num3 = 0x8000;
                valid = false;
                if ((num2 & num3) != 0)
                {
                    valid = true;
                }
                num2 &= 0x7fff;
                int num4 = 0x4000;
                negative = false;
                if ((num2 & num4) != 0)
                {
                    num2 ^= 0x3fff;
                    num2++;
                    num2 &= 0x3fff;
                    negative = true;
                }
                return (num2 + (((float)buffer[bufferOffset + 2]) / 256f));
            }

            protected int CreateInt(byte[] buffer, int bufferOffset, int length)
            {
                if ((bufferOffset + length) > buffer.Length)
                {
                    return -1;
                }
                if (length > 4)
                {
                    throw new Exception("You cannot shift an integer by more than 4 bytes");
                }
                int num = 0;
                int num2 = length - 1;
                int end = bufferOffset + length;
                for (int i = bufferOffset; i < end; ++i)
                {
                    num |= buffer[i] << (num2 * 8);
                    num2--;
                }
                return num;
            }

            protected long CreateLong(byte[] buffer, int bufferOffset, int length)
            {
                if (length > 8)
                {
                    throw new Exception("You cannot shift a long by more than 8 bytes");
                }
                long num = 0L;
                int num2 = length - 1;
                int end = bufferOffset + length;
                for (int i = bufferOffset; i < end; ++i)
                {
                    num |= (uint)buffer[i] << (num2 * 8);//Without cast this has a warning
                    num2--;
                }
                return num;
            }

            protected byte[] StoreBytesToBuffer(byte[] buffer, int bufferOffset, int length, int val)
            {
                if ((bufferOffset + length) <= buffer.Length)
                {
                    if (length > 4)
                    {
                        throw new Exception("Cannot shift Integer more than 4 bytes");
                    }
                    int num = length - 1;
                    int end = bufferOffset + length;
                    for (int i = bufferOffset; i < end; ++i)
                    {
                        buffer[i] = (byte)(val >> (num * 8));
                        num--;
                    }
                }
                return buffer;
            }

            protected byte[] StoreBytesToBuffer(byte[] buffer, int bufferOffset, int length, long val)
            {
                if ((bufferOffset + length) <= buffer.Length)
                {
                    if (length > 8)
                    {
                        throw new Exception("Cannot shift Long more than 8 bytes");
                    }
                    int num = length - 1;
                    int end = bufferOffset + length;
                    for (int i = bufferOffset; i < end; ++i)
                    {
                        buffer[i] = (byte)(val >> (num * 8));
                        num--;
                    }
                }
                return buffer;
            }

            protected byte[] StoreBytesToBuffer(byte[] buffer, int bufferOffset, int length, float val)
            {
                if (length == 2)
                {
                    buffer[bufferOffset] = (byte)val;
                    buffer[bufferOffset + 1] = (byte)(((val - buffer[bufferOffset]) * 256f) + 0.5f);
                    return buffer;
                }
                if (length != 3)
                {
                    throw new Exception("Unsupported conversion from float for SS125 Driver");
                }
                return buffer;
            }

            protected byte[] StoreBytesToBuffer(byte[] buffer, int bufferOffset, int length, string val)
            {
                if ((length + bufferOffset) > buffer.Length)
                {
                    length = buffer.Length - bufferOffset;
                }
                for (int i = bufferOffset; i < (bufferOffset + length); ++i)
                {
                    if ((i - bufferOffset) < val.Length)
                    {
                        buffer[i] = (byte)val[i - bufferOffset];
                    }
                    else
                    {
                        buffer[i] = 0;
                    }
                }
                return buffer;
            }

            protected byte[] StoreBytesToBuffer(byte[] buffer, int bufferOffset, int length, string val, int stringOffset)
            {
                if ((length + bufferOffset) > buffer.Length)
                {
                    length = buffer.Length - bufferOffset;
                }
                for (int i = bufferOffset; i < (bufferOffset + length); ++i)
                {
                    if ((i + stringOffset) < val.Length)
                    {
                        buffer[i] = (byte)val[i + stringOffset];
                    }
                    else
                    {
                        buffer[i] = 0;
                    }
                }
                return buffer;
            }

            #endregion
        }

        #endregion

        #region Messages

        public class SaveToFlashMessage : Z1Packet
        {
            #region Constructor

            public SaveToFlashMessage()
                : base()
            {
                base.Payload.MessageID = 8;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = AccessType.Write;
                base.AddedTimeout = 0x1388;
                base.HasResponse = false;
            }

            #endregion
        }

        public class RebootSensorMessage : Z1Packet
        {
            public RebootSensorMessage()
                : base()
            {
                base.Payload.MessageID = 20;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = AccessType.Write;
                base.HasResponse = false;
            }
        }

        public class ClearStoredIntervalDataMessage : Z1Packet
        {
            public ClearStoredIntervalDataMessage()
                : base()
            {
                base.Payload.MessageID = 100;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = AccessType.Write;
                base.HasResponse = false;
            }
        }

        public class ClearEventBufferMessage : Z1Packet
        {
            public ClearEventBufferMessage()
                : base()
            {
                base.Payload.MessageID = 0x6d;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = AccessType.Write;
                base.HasResponse = false;
            }
        }

        public class GeneralConfigurationMessage : Z1Packet
        {
            #region Constructor

            public GeneralConfigurationMessage(AccessType accessType)
                : base()
            {
                base.Payload.MessageID = 0;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = accessType;
            }

            public GeneralConfigurationMessage(Z1Packet packet)
                : base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;                
            }

            #endregion

            #region Properties

            public string Description
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateString(base.Payload.Data, 0x22, 0x20);
                    }
                    return null;
                }
                set
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        base.StoreBytesToBuffer(base.Payload.Data, 0x22, 0x20, value);
                    }
                }
            }

            public string Location
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateString(base.Payload.Data, 2, 0x20);
                    }
                    return null;
                }
                set
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        base.StoreBytesToBuffer(base.Payload.Data, 2, 0x20, value);
                    }
                }
            }

            public string Orientation
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateString(base.Payload.Data, 0, 2);
                    }
                    return null;
                }
                set
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        base.StoreBytesToBuffer(base.Payload.Data, 0, 2, value);
                    }
                }
            }

            public string SerialNumber
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateString(base.Payload.Data, 0x42, 0x10);
                    }
                    return null;
                }
                set
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        base.StoreBytesToBuffer(base.Payload.Data, 0x42, 0x10, value);
                    }
                }
            }

            public byte Units
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[0x52];
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        base.Payload.Data[0x52] = value;
                    }
                }
            }

            #endregion
            
        }

        public class VersionInformationMessage : Z1Packet
        {
            #region Constructor

            public VersionInformationMessage()
                : base()
            {
                base.Payload.MessageID = 4;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = AccessType.Read;
                base.HasResponse = true;
            }

            public VersionInformationMessage(Z1Packet packet)
                : base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            #region Fields

            private static char[] hexDigits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

            #endregion

            #region Methods

            private static string ToHexString(byte[] bytes)
            {
                return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", new object[] { bytes[0], bytes[1], bytes[2], bytes[3] });
            }

            // Properties
            public string AlgorithmRev
            {
                get
                {
                    byte[] bytes = new byte[4];
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 8))
                    {
                        for (int i = 0; i < 4; ++i)
                        {
                            bytes[i] = base.Payload.Data[i + 4];
                        }
                        return ToHexString(bytes);
                    }
                    return "N/A";
                }
            }

            public string DSPCodeRev
            {
                get
                {
                    byte[] bytes = new byte[4];
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 4))
                    {
                        for (int i = 0; i < 4; ++i)
                        {
                            bytes[i] = base.Payload.Data[i];
                        }
                        return ToHexString(bytes);
                    }
                    return "N/A";
                }
            }

            public string FPAAVersion
            {
                get
                {
                    byte[] bytes = new byte[4];
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 0x10))
                    {
                        for (int i = 0; i < 4; ++i)
                        {
                            bytes[i] = base.Payload.Data[i + 12];
                        }
                        return ToHexString(bytes);
                    }
                    return "N/A";
                }
            }

            public string FPGAVersion
            {
                get
                {
                    byte[] bytes = new byte[4];
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 12))
                    {
                        for (int i = 0; i < 4; ++i)
                        {
                            bytes[i] = base.Payload.Data[i + 8];
                        }
                        return ToHexString(bytes);
                    }
                    return "N/A";
                }
            }

            #endregion

        }

        public class AutoConfigurationMessage : Z1Packet
        {

            #region Constructor

            public AutoConfigurationMessage(AccessType accessType)
                : base()
            {
                base.Payload.MessageID = 12;
                base.Payload.MessageSubID = 1;
                base.Payload.Access = accessType;
                //addedTimeout = 0x3e8
            }

            public AutoConfigurationMessage(Z1Packet packet)
                : base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public BinType[] CfgArr
            {
                get
                {
                    BinType[] typeArray = new BinType[250];
                    typeArray.Initialize();
                    int numLanes = this.NumLanes;
                    if (numLanes > 0)
                    {
                        for (int i = 0; i < numLanes; ++i)
                        {
                            int index = (int)(this.LanesArr[i]._nearRange + 0.5);
                            int num4 = (int)(this.LanesArr[i]._farRange + 0.5);
                            BinType undefined = BinType.undefined;
                            if (this.LanesArr[i]._state == LaneState.auto)
                            {
                                undefined = BinType.aLane;
                            }
                            if (((index > 0) && (num4 < 250)) && (index < num4))
                            {
                                for (int j = index; j <= num4; j++)
                                {
                                    typeArray[j] = undefined;
                                }
                                if (undefined != BinType.excluded)
                                {
                                    typeArray[index] = BinType.boundary;
                                    typeArray[num4] = BinType.boundary;
                                }
                            }
                        }
                    }
                    return typeArray;
                }
                set
                {
                    byte[] buffer = new byte[180];
                    BinType undefined = BinType.undefined;
                    BinType type2 = BinType.undefined;
                    BinType type3 = BinType.undefined;
                    int num = -1;
                    int num2 = 0;
                    int index = 0;
                    int num4 = 0;
                    int num5 = value.Length - 1;
                    for (index = 0; index <= num5; index++)
                    {
                        undefined = type2;
                        type2 = value[index];
                        type3 = BinType.undefined;
                        if (index < num5)
                        {
                            type3 = value[index + 1];
                        }
                        if ((type2 == BinType.boundary) || ((type2 == BinType.excluded) && (undefined != BinType.excluded)))
                        {
                            if (num == -1)
                            {
                                num = index;
                            }
                            else
                            {
                                num2++;
                                buffer[num4++] = (byte)num;
                                buffer[num4++] = 0;
                                buffer[num4++] = (byte)index;
                                buffer[num4++] = 0;
                                if (undefined == BinType.inactive)
                                {
                                    buffer[num4++] = 1;
                                }
                                else
                                {
                                    buffer[num4++] = 0;
                                }
                                buffer[num4++] = 0;
                                num = -1;
                                if ((type2 == BinType.excluded) && (((undefined == BinType.sLane) || (undefined == BinType.inactive)) || ((undefined == BinType.aLane) || (undefined == BinType.boundary))))
                                {
                                    num = ++index;
                                }
                                else
                                {
                                    switch (type3)
                                    {
                                        case BinType.sLane:
                                        case BinType.aLane:
                                        case BinType.inactive:
                                            num = index;
                                            break;
                                    }
                                }
                            }
                        }
                        else if ((type2 == BinType.excluded) && (type3 != BinType.excluded))
                        {
                            if (num == -1)
                            {
                                num = index;
                            }
                            else
                            {
                                num2++;
                                buffer[num4++] = (byte)num;
                                buffer[num4++] = 0;
                                buffer[num4++] = (byte)index;
                                buffer[num4++] = 0;
                                buffer[num4++] = 2;
                                buffer[num4++] = 0;
                                num = -1;
                                switch (type3)
                                {
                                    case BinType.sLane:
                                    case BinType.aLane:
                                    case BinType.inactive:
                                    case BinType.excluded:
                                        num = index;
                                        break;
                                }
                            }
                        }
                    }
                    base.Payload.Data = new byte[(num2 * 6) + 1];
                    base.Payload.Data[0] = (byte)num2;
                    for (index = 1; index < (num2 * 6); index++)
                    {
                        base.Payload.Data[index] = buffer[index];
                    }
                }
            }

            public LaneStruct[] LanesArr
            {
                get
                {
                    if (base.Payload == null)
                    {
                        return null;
                    }
                    if (this.NumLanes <= 0)
                    {
                        return null;
                    }
                    LaneStruct[] structArray = new LaneStruct[this.NumLanes];
                    int num = 1;
                    int numLanes = this.NumLanes;
                    for (int i = 0; i < numLanes; ++i)
                    {
                        double near = base.Payload.Data[num++];
                        near += ((double)base.Payload.Data[num++]) / 256.0;
                        double far = base.Payload.Data[num++];
                        far += ((double)base.Payload.Data[num++]) / 256.0;
                        LaneState undefined = LaneState.undefined;
                        switch (base.Payload.Data[num++])
                        {
                            case 0:
                                undefined = LaneState.auto;
                                break;

                            case 1:
                                undefined = LaneState.auto;
                                break;

                            case 2:
                                undefined = LaneState.auto;
                                break;
                        }
                        LaneDirection dir = LaneDirection.undefined;
                        switch (base.Payload.Data[num++])
                        {
                            case 0:
                                dir = LaneDirection.right;
                                break;

                            case 1:
                                dir = LaneDirection.left;
                                break;
                        }
                        structArray[i] = new LaneStruct(undefined, dir, near, far);
                    }
                    return structArray;
                }
                set
                {
                    if (value != null)
                    {
                        int valueLength = value.Length;
                        base.Payload.Data = new byte[(valueLength * 6) + 1];
                        base.Payload.Data[0] = (byte)valueLength;
                        int num = 1;
                        for (int i = 0; i < valueLength; ++i)
                        {
                            base.Payload.Data[num++] = (byte)value[i]._nearRange;
                            int num3 = (int)value[i]._nearRange;
                            num3 = (int)((value[i]._nearRange - num3) * 256.0);
                            base.Payload.Data[num++] = (byte)num3;
                            base.Payload.Data[num++] = (byte)value[i]._farRange;
                            num3 = (int)value[i]._farRange;
                            num3 = (int)((value[i]._farRange - num3) * 256.0);
                            base.Payload.Data[num++] = (byte)num3;
                            byte num4 = 0;
                            switch (value[i]._state)
                            {
                                case LaneState.active:
                                    num4 = 0;
                                    break;

                                case LaneState.inactive:
                                    num4 = 1;
                                    break;

                                case LaneState.excluded:
                                    num4 = 2;
                                    break;
                            }
                            base.Payload.Data[num++] = num4;
                            byte num5 = 0;
                            switch (value[i]._direction)
                            {
                                case LaneDirection.left:
                                    num5 = 1;
                                    break;

                                case LaneDirection.right:
                                    num5 = 0;
                                    break;
                            }
                            base.Payload.Data[num++] = num5;
                        }
                    }
                }
            }

            public int NumLanes
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[0];
                    }
                    return 0;
                }
            }

        }

        public class AutoConfigurationResetMessage : Z1Packet
        {
            public AutoConfigurationResetMessage()
                : base()
            {
                base.Payload.MessageID = 15;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = AccessType.Write;
            }
        }        

        public class UARTIDMessage : Z1Packet
        {

            #region Constructor

            public UARTIDMessage()
                :base()
            {
                base.Payload.MessageID = 0x15;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = AccessType.Read;
            }

            public UARTIDMessage(Z1Packet packet)               
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public string CurrentID
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        switch (base.Payload.Data[0])
                        {
                            case 0:
                                return "RS485";

                            case 1:
                                return "RS232";

                            case 3:
                                return "Exp1";

                            case 4:
                                return "Exp2";
                        }
                    }
                    return "Unknown";
                }
            }

        }

        public class UARTConfigurationMessage : Z1Packet
        {
            #region Constructor

            public UARTConfigurationMessage(AccessType accessType)
                :base()
            {
                base.Payload.MessageID = 0x15;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = accessType;
            }

            public UARTConfigurationMessage(Z1Packet packet)               
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            #region Properties

            public int ExpPort0HWHandshake
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[13];
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 14))
                    {
                        base.Payload.Data[13] = (byte)value;
                    }
                }
            }

            public int ExpPort0Termination
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[0x10];
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 0x11))
                    {
                        base.Payload.Data[0x10] = (byte)value;
                    }
                }
            }

            public int ExpPort1HWHandshake
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[14];
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 15))
                    {
                        base.Payload.Data[14] = (byte)value;
                    }
                }
            }

            public int ExpPort1Termination
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[0x11];
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 0x12))
                    {
                        base.Payload.Data[0x11] = (byte)value;
                    }
                }
            }

            public int RS232HWHandshake
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[12];
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 13))
                    {
                        base.Payload.Data[12] = (byte)value;
                    }
                }
            }

            public int RS485Termination
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[15];
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 0x10))
                    {
                        base.Payload.Data[15] = (byte)value;
                    }
                }
            }

            public int UART0BaudRate
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[0];
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 1))
                    {
                        base.Payload.Data[0] = (byte)value;
                    }
                }
            }

            public int UART0TurnAroundTime
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 4, 2);
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 6))
                    {
                        base.StoreBytesToBuffer(base.Payload.Data, 4, 2, value);
                    }
                }
            }

            public int UART1BaudRate
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[1];
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 2))
                    {
                        base.Payload.Data[1] = (byte)value;
                    }
                }
            }

            public int UART1TurnAroundTime
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 6, 2);
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 8))
                    {
                        base.StoreBytesToBuffer(base.Payload.Data, 6, 2, value);
                    }
                }
            }

            public int UART2BaudRate
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[2];
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 3))
                    {
                        base.Payload.Data[2] = (byte)value;
                    }
                }
            }

            public int UART2TurnAroundTime
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 8, 2);
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 10))
                    {
                        base.StoreBytesToBuffer(base.Payload.Data, 8, 2, value);
                    }
                }
            }

            public int UART3BaudRate
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[3];
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 4))
                    {
                        base.Payload.Data[3] = (byte)value;
                    }
                }
            }

            public int UART3TurnAroundTime
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 10, 2);
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 12))
                    {
                        base.StoreBytesToBuffer(base.Payload.Data, 10, 2, value);
                    }
                }
            }

            #endregion

        }

        public class SystemTimeMessage : Z1Packet
        {
            #region Constructor

            public SystemTimeMessage(AccessType accessType)
                :base()
            {
                base.Payload.MessageID = 14;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = accessType;
            }

            public SystemTimeMessage(Z1Packet packet)               
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public Timestamp Timestamp
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    byte[] data = new byte[8];
                    int dataLength = data.Length;
                    for (int i = 0; i < dataLength; ++i) data[i] = base.Payload.Data[i];
                    return new Timestamp(data, true);
                }
                set
                {
                    Timestamp timestamp = value.ToUniversalTime();
                    base.Payload.Data = base.StoreBytesToBuffer(base.Payload.Data, 0, 8, timestamp.Long);
                }
            }

        }

        public class StoredIndexByTimestampMessage : Z1Packet
        {
            #region Constructor

            public StoredIndexByTimestampMessage()
                :base()
            {
                base.Payload.MessageID = 0x6f;
                base.Payload.MessageSubID = 0;
                base.Payload.Access = AccessType.Read;
                base.Payload.Data = new byte[8];
            }

            public StoredIndexByTimestampMessage(Z1Packet packet)               
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public int IntervalIndex
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0, 3);
                    }
                    return 0;
                }
            }

            public Timestamp Timestamp
            {
                set
                {
                    if (base.Payload != null)
                    {
                        Timestamp timestamp = value.ToUniversalTime();
                        base.Payload.Data = base.StoreBytesToBuffer(base.Payload.Data, 0, 8, timestamp.Long);
                    }
                }
            }
        }

        public class SpeedClassificationConfigurationMessage : Z1Packet
        {
            #region Constructor

            public SpeedClassificationConfigurationMessage(AccessType accessType)
                :base()
            {
                base.Payload.MessageID = 0x1d;
                base.Payload.Access = accessType;
            }

            public SpeedClassificationConfigurationMessage(Z1Packet packet)               
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion
            
            // Properties
            public int BoundaryCount
            {
                get
                {
                    return base.Payload.MessageSubID;
                }
                set
                {
                    base.Payload.MessageSubID = (byte)value;
                    base.Payload.Data = new byte[value * 2];
                }
            }

            public float[] SpeedBoundaries
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return new float[0];
                    }
                    //System.Collections.ArrayList list = new System.Collections.ArrayList();
                    List<float> list = new List<float>();
                    int length = 2;
                    int boundryCount = this.BoundaryCount;
                    for (int i = 0; i < boundryCount; ++i)
                    {
                        if (((i * length) + 1) < base.Payload.Data.Length)
                        {
                            float num3 = base.CreateFloat(base.Payload.Data, i * length, length);
                            if (num3 > 0f)
                            {
                                list.Add(num3);
                            }
                        }
                    }
                    return list.ToArray();
                    /*
                    float[] numArray = new float[list.Count];
                    for (int j = 0; j < list.Count; j++)
                    {
                        numArray[j] = (float)list[j];
                    }
                    return numArray;
                    */
                }
                set
                {
                    int length = 2;
                    int valueLength = value.Length;
                    int payloadDataLength = base.Payload.Data.Length;
                    for (int i = 0; ((i < valueLength) && (base.Payload.Data != null)) && (((i * length) + length) <= payloadDataLength); ++i)
                    {
                        base.StoreBytesToBuffer(base.Payload.Data, i * length, length, value[i]);
                    }
                }
            }
        }

        public class SensorAlignmentStatusMessage : Z1Packet
        {
              #region Constructor

            public SensorAlignmentStatusMessage(Z1Packet packet)               
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public int Direction
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[0];
                    }
                    return 0;
                }
            }

            public uint Magnitude
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return (uint)base.CreateInt(base.Payload.Data, 1, 2);
                    }
                    return 0;
                }
            }
        }

        public class SavedConfigurationMessage : Z1Packet
        {
            #region Constructor

            public SavedConfigurationMessage(AccessType accessType)
                :base()
            {
                base.Payload.MessageID = 12;
                base.Payload.MessageSubID = 0;
                base.Payload.Access = accessType;
            }

            public SavedConfigurationMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public BinType[] CfgArr
            {
                get
                {
                    BinType[] typeArray = new BinType[250];
                    typeArray.Initialize();
                    int numLanes = this.NumLanes;
                    if (numLanes > 0)
                    {
                        for (int i = 0; i < numLanes; ++i)
                        {
                            int index = (int)(this.LanesArr[i]._nearRange + 0.5);
                            int num4 = (int)(this.LanesArr[i]._farRange + 0.5);
                            BinType undefined = BinType.undefined;
                            switch (this.LanesArr[i]._state)
                            {
                                case LaneState.active:
                                    undefined = BinType.sLane;
                                    break;

                                case LaneState.inactive:
                                    undefined = BinType.inactive;
                                    break;

                                case LaneState.excluded:
                                    undefined = BinType.excluded;
                                    break;
                            }
                            if (((index >= 0) && (num4 < 250)) && (index <= num4))
                            {
                                for (int j = index; j <= num4; j++)
                                {
                                    typeArray[j] = undefined;
                                }
                                if (undefined != BinType.excluded)
                                {
                                    typeArray[index] = BinType.boundary;
                                    typeArray[num4] = BinType.boundary;
                                }
                            }
                        }
                    }
                    return typeArray;
                }
                set
                {
                    byte[] buffer = new byte[180];
                    BinType undefined = BinType.undefined;
                    BinType type2 = BinType.undefined;
                    BinType type3 = BinType.undefined;
                    int num = -1;
                    int num2 = 0;
                    int index = 0;
                    int num4 = 0;
                    int num5 = value.Length - 1;
                    for (index = 0; index <= num5; ++index)
                    {
                        undefined = type2;
                        type2 = value[index];
                        type3 = BinType.undefined;
                        if (index < num5)
                        {
                            type3 = value[index + 1];
                        }
                        if ((type2 == BinType.boundary) || ((type2 == BinType.excluded) && (undefined != BinType.excluded)))
                        {
                            if (num == -1)
                            {
                                num = index;
                            }
                            else
                            {
                                num2++;
                                buffer[num4++] = (byte)num;
                                buffer[num4++] = 0;
                                buffer[num4++] = (byte)index;
                                buffer[num4++] = 0;
                                if (undefined == BinType.inactive)
                                {
                                    buffer[num4++] = 1;
                                }
                                else
                                {
                                    buffer[num4++] = 0;
                                }
                                buffer[num4++] = 0;
                                num = -1;
                                if ((type2 == BinType.excluded) && (((undefined == BinType.sLane) || (undefined == BinType.inactive)) || ((undefined == BinType.aLane) || (undefined == BinType.boundary))))
                                {
                                    num = ++index;
                                }
                                else
                                {
                                    switch (type3)
                                    {
                                        case BinType.sLane:
                                        case BinType.aLane:
                                        case BinType.inactive:
                                            num = index;
                                            break;
                                    }
                                }
                            }
                        }
                        else if ((type2 == BinType.excluded) && (type3 != BinType.excluded))
                        {
                            if (num == -1)
                            {
                                num = index;
                            }
                            else
                            {
                                num2++;
                                buffer[num4++] = (byte)num;
                                buffer[num4++] = 0;
                                buffer[num4++] = (byte)index;
                                buffer[num4++] = 0;
                                buffer[num4++] = 2;
                                buffer[num4++] = 0;
                                num = -1;
                                switch (type3)
                                {
                                    case BinType.sLane:
                                    case BinType.aLane:
                                    case BinType.inactive:
                                    case BinType.excluded:
                                        num = index;
                                        break;
                                }
                            }
                        }
                    }
                    base.Payload.Data = new byte[(num2 * 6) + 1];
                    byte[] buffer2 = new byte[(num2 * 6) + 1];
                    int buffer2Length = buffer2.Length;
                    for (index = 1; index < buffer2Length; ++index) buffer2[index] = buffer[index - 1];
                    base.Payload.Data = buffer2;
                    base.Payload.Data[0] = (byte)num2;
                }
            }

            public LaneStruct[] LanesArr
            {
                get
                {
                    int numLanes = this.NumLanes;
                    if (base.Payload == null)
                    {
                        return null;
                    }
                    if (numLanes <= 0)
                    {
                        return null;
                    }
                    LaneStruct[] structArray = new LaneStruct[numLanes];
                    int num = 1;                    
                    for (int i = 0; i < numLanes; ++i)
                    {
                        double near = base.Payload.Data[num++];
                        near += ((double)base.Payload.Data[num++]) / 256.0;
                        double far = base.Payload.Data[num++];
                        far += ((double)base.Payload.Data[num++]) / 256.0;
                        LaneState undefined = LaneState.undefined;
                        switch (base.Payload.Data[num++])
                        {
                            case 0:
                                undefined = LaneState.active;
                                break;

                            case 1:
                                undefined = LaneState.inactive;
                                break;

                            case 2:
                                undefined = LaneState.excluded;
                                break;
                        }
                        LaneDirection dir = LaneDirection.undefined;
                        switch (base.Payload.Data[num++])
                        {
                            case 0:
                                dir = LaneDirection.right;
                                break;

                            case 1:
                                dir = LaneDirection.left;
                                break;
                        }
                        structArray[i] = new LaneStruct(undefined, dir, near, far);
                    }
                    return structArray;
                }
                set
                {
                    if (value != null)
                    {
                        int valueLength = value.Length;
                        base.Payload.Data = new byte[(valueLength * 6) + 1];
                        base.Payload.Data[0] = (byte)valueLength;
                        int num = 1;
                        for (int i = 0; i < valueLength; ++i)
                        {
                            double num3 = value[i]._nearRange;
                            double num4 = value[i]._farRange;
                            base.Payload.Data[num++] = (byte)num3;
                            base.Payload.Data[num++] = (byte)((num3 - ((byte)num3)) * 256.0);
                            base.Payload.Data[num++] = (byte)num4;
                            base.Payload.Data[num++] = (byte)((num4 - ((byte)num4)) * 256.0);
                            byte num5 = 0;
                            switch (value[i]._state)
                            {
                                case LaneState.active:
                                    num5 = 0;
                                    break;

                                case LaneState.inactive:
                                    num5 = 1;
                                    break;

                                case LaneState.excluded:
                                    num5 = 2;
                                    break;
                            }
                            base.Payload.Data[num++] = num5;
                            byte num6 = 0;
                            switch (value[i]._direction)
                            {
                                case LaneDirection.left:
                                    num6 = 1;
                                    break;

                                case LaneDirection.right:
                                    num6 = 0;
                                    break;
                            }
                            base.Payload.Data[num++] = num6;
                        }
                    }
                }

            }

            public int NumLanes
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[0];
                    }
                    return 0;
                }
            }
        }

        public class RadarTuningMessage : Z1Packet
        {
            #region Constructor

            public RadarTuningMessage(AccessType accessType)
                :base()
            {
                base.Payload.MessageID = 0x19;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = accessType;
            }

            public RadarTuningMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            private float ConvertFromDB(int val)
            {
                switch (val)
                {
                    case -20:
                        return 0.0078125f;

                    case -19:
                        return 0.01171875f;

                    case -18:
                        return 0.015625f;

                    case -17:
                        return 0.01953125f;

                    case -16:
                        return 0.0234375f;

                    case -15:
                        return 0.03125f;

                    case -14:
                        return 0.0390625f;

                    case -13:
                        return 0.046875f;

                    case -12:
                        return 0.0625f;

                    case -11:
                        return 0.078125f;

                    case -10:
                        return 0.09765625f;

                    case -9:
                        return 0.125f;

                    case -8:
                        return 0.15625f;

                    case -7:
                        return 0.1992188f;

                    case -6:
                        return 0.25f;

                    case -5:
                        return 0.3125f;

                    case -4:
                        return 0.3945313f;

                    case -3:
                        return 0.5f;

                    case -2:
                        return 0.6289063f;

                    case -1:
                        return 0.7929688f;

                    case 0:
                        return 1f;

                    case 1:
                        return 1.257813f;

                    case 2:
                        return 1.582031f;

                    case 3:
                        return 1.992188f;

                    case 4:
                        return 2.511719f;

                    case 5:
                        return 3.160156f;

                    case 6:
                        return 3.980469f;

                    case 7:
                        return 5.011719f;

                    case 8:
                        return 6.308594f;

                    case 9:
                        return 7.941406f;

                    case 10:
                        return 10f;

                    case 11:
                        return 12.58594f;

                    case 12:
                        return 15.84766f;

                    case 13:
                        return 19.94922f;

                    case 14:
                        return 25.11719f;

                    case 15:
                        return 31.62109f;

                    case 0x10:
                        return 39.80859f;

                    case 0x11:
                        return 50.11719f;

                    case 0x12:
                        return 63.09375f;

                    case 0x13:
                        return 79.42969f;

                    case 20:
                        return 100f;
                }
                return 1f;
            }

            private float ConvertToDB(float fval)
            {
                //http://bytes.com/topic/c-sharp/answers/255776-switch-expression-does-not-work-float-types
                //Cannot switch on floats
                int num = 0;
                if (fval == 0.0078125f)
                {
                    num = -20;
                }
                else if (fval == 0.01171875f)
                {
                    num = -19;
                }
                else if (fval == 0.015625f)
                {
                    num = -18;
                }
                else if (fval == 0.01953125f)
                {
                    num = -17;
                }
                else if (fval == 0.0234375f)
                {
                    num = -16;
                }
                else if (fval == 0.03125f)
                {
                    num = -15;
                }
                else if (fval == 0.0390625f)
                {
                    num = -14;
                }
                else if (fval == 0.046875f)
                {
                    num = -13;
                }
                else if (fval == 0.0625f)
                {
                    num = -12;
                }
                else if (fval == 0.078125f)
                {
                    num = -11;
                }
                else if (fval == 0.09765625f)
                {
                    num = -10;
                }
                else if (fval == 0.125f)
                {
                    num = -9;
                }
                else if (fval == 0.15625f)
                {
                    num = -8;
                }
                else if (fval == 0.1992188f)
                {
                    num = -7;
                }
                else if (fval == 0.25f)
                {
                    num = -6;
                }
                else if (fval == 0.3125)
                {
                    num = -5;
                }
                else if (fval == 0.39453125)
                {
                    num = -4;
                }
                else if (fval == 0.5)
                {
                    num = -3;
                }
                else if (fval == 0.62890625)
                {
                    num = -2;
                }
                else if (fval == 0.79296875)
                {
                    num = -1;
                }
                else if (fval == 1f)
                {
                    num = 0;
                }
                else if (fval == 1.257813f)
                {
                    num = 1;
                }
                else if (fval == 1.582031f)
                {
                    num = 2;
                }
                else if (fval == 1.992188f)
                {
                    num = 3;
                }
                else if (fval == 2.511719f)
                {
                    num = 4;
                }
                else if (fval == 3.160156f)
                {
                    num = 5;
                }
                else if (fval == 3.980469f)
                {
                    num = 6;
                }
                else if (fval == 5.011719f)
                {
                    num = 7;
                }
                else if (fval == 6.308594f)
                {
                    num = 8;
                }
                else if (fval == 7.941406f)
                {
                    num = 9;
                }
                else if (fval == 10f)
                {
                    num = 10;
                }
                else if (fval == 12.58594f)
                {
                    num = 11;
                }
                else if (fval == 15.84766f)
                {
                    num = 12;
                }
                else if (fval == 19.94922f)
                {
                    num = 13;
                }
                else if (fval == 25.11719f)
                {
                    num = 14;
                }
                else if (fval == 31.62109f)
                {
                    num = 15;
                }
                else if (fval == 39.80859f)
                {
                    num = 0x10;
                }
                else if (fval == 50.11719f)
                {
                    num = 0x11;
                }
                else if (fval == 63.09375f)
                {
                    num = 0x12;
                }
                else if (fval == 79.42969f)
                {
                    num = 0x13;
                }
                else if (fval == 100f)
                {
                    num = 20;
                }
                return (float)num;
            }

            // Properties
            public int ConfiguredLanes
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[0];
                    }
                    return 0;
                }
                set
                {
                    if (base.Payload.Data == null)
                    {
                        base.Payload.Data = new byte[(value * 10) + 1];
                    }
                    base.Payload.Data[0] = (byte)value;
                }
            }

            public RadarTuningParams[] TuningParams
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    RadarTuningParams[] paramsArray = new RadarTuningParams[this.ConfiguredLanes];
                    int pararmsArrayLength = paramsArray.Length;
                    for (int i = 0; i < pararmsArrayLength; ++i)
                    {
                        int num = i * 10;
                        paramsArray[i] = new RadarTuningParams();
                        if (base.Payload.Data[num + 1] > 0x7f)
                        {
                            paramsArray[i].ThresholdMultiplier = (0x100 - base.Payload.Data[num + 1]) * -1;
                        }
                        else
                        {
                            paramsArray[i].ThresholdMultiplier = base.CreateFloat(base.Payload.Data, num + 1, 2);
                        }
                        paramsArray[i].SpeedMultiplier = base.CreateFloat(base.Payload.Data, num + 3, 2);
                        paramsArray[i].VolumeMultiplier = base.CreateFloat(base.Payload.Data, num + 5, 2);
                        paramsArray[i].MinGapMultiplier = base.CreateFloat(base.Payload.Data, num + 7, 2);
                        paramsArray[i].DefaultLoopSizeFactor = base.CreateFloat(base.Payload.Data, num + 9, 2);
                        if (paramsArray[i].DefaultLoopSizeFactor > 128f)
                        {
                            paramsArray[i].DefaultLoopSizeFactor = (paramsArray[i].DefaultLoopSizeFactor - 128f) * -1f;
                        }
                    }
                    return paramsArray;
                }
                set
                {
                    if (value != null)
                    {
                        int valueLength = value.Length;
                        base.Payload.Data = new byte[(valueLength * 10) + 1];
                        base.Payload.Data[0] = (byte)valueLength;
                        for (int i = 0; i < valueLength; ++i)
                        {
                            if (value[i] == null)
                            {
                                value[i] = new RadarTuningParams();
                            }
                            int num = i * 10;
                            if (value[i].ThresholdMultiplier < 0f)
                            {
                                base.Payload.Data[num + 1] = (byte)(0x100 + ((int)value[i].ThresholdMultiplier));
                                base.Payload.Data[num + 2] = 0;
                            }
                            else
                            {
                                base.StoreBytesToBuffer(base.Payload.Data, num + 1, 2, (float)((int)value[i].ThresholdMultiplier));
                            }
                            base.StoreBytesToBuffer(base.Payload.Data, num + 3, 2, value[i].SpeedMultiplier);
                            base.StoreBytesToBuffer(base.Payload.Data, num + 5, 2, value[i].VolumeMultiplier);
                            base.StoreBytesToBuffer(base.Payload.Data, num + 7, 2, value[i].MinGapMultiplier);
                            if (value[i].DefaultLoopSizeFactor < 0f)
                            {
                                RadarTuningParams params1 = value[i];
                                params1.DefaultLoopSizeFactor *= -1f;
                                value[i].DefaultLoopSizeFactor += 128f;
                            }
                            base.StoreBytesToBuffer(base.Payload.Data, num + 9, 2, value[i].DefaultLoopSizeFactor);
                        }
                    }
                }
            }

        }

        public class RadarConfigurationMessage : Z1Packet
        {
            #region Constructor

            public RadarConfigurationMessage(AccessType accessType)
                :base()
            {
                base.Payload.MessageID = 6;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = accessType;
            }

            public RadarConfigurationMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public int ConfiguredLanes
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[0];
                    }
                    return 0;
                }
                set
                {
                    base.Payload.Data[0] = (byte)value;
                }
            }
        }

        public class RadarChannelMessage : Z1Packet
        {
            #region Constructor

            public RadarChannelMessage(AccessType accessType)
                :base()
            {
                base.Payload.MessageID = 0x16;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = accessType;
                base.Payload.Data = new byte[1];
            }

            public RadarChannelMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public int Channel
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[0];
                    }
                    return 0;
                }
                set
                {
                    if (base.Payload.Data == null)
                    {
                        base.Payload.Data = new byte[1];
                    }
                    base.Payload.Data[0] = (byte)value;
                }
            }
        }

        public class PushEnableDisableMessage : Z1Packet
        {
            #region Constructor

            public PushEnableDisableMessage(AccessType accessType)
                :base()
            {
                base.Payload.MessageID = 13;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = accessType;
                base.Payload.Data = new byte[1];
            }

            public PushEnableDisableMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public bool Enabled
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return false;
                    }
                    return ((base.Payload.Data[0] == 1) || ((base.Payload.Data[0] == 0) && false));
                }
                set
                {
                    if (base.Payload.Data == null)
                    {
                        base.Payload.Data = new byte[1];
                    }
                    base.Payload.Data[0] = 0;
                    if (value)
                    {
                        base.Payload.Data[0] = 1;
                    }
                }
            }
        }

        public class PlayPauseAutoConfigurationMessage : Z1Packet
        {
            #region Constructor

            public PlayPauseAutoConfigurationMessage(AccessType accessType)
                :base()
            {
                base.Payload.MessageID = 0x12;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = accessType;
                base.Payload.Data = new byte[1];
            }

            public PlayPauseAutoConfigurationMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public bool Pause
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return false;
                    }
                    return ((base.Payload.Data[0] == 0) || ((base.Payload.Data[0] == 1) && false));
                }
                set
                {
                    if (base.Payload.Data == null)
                    {
                        base.Payload.Data = new byte[1];
                    }
                    base.Payload.Data[0] = 1;
                    if (value)
                    {
                        base.Payload.Data[0] = 0;
                    }
                }
            }

            public bool Play
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return false;
                    }
                    return ((base.Payload.Data[0] == 1) || ((base.Payload.Data[0] == 0) && false));
                }
                set
                {
                    if (base.Payload.Data == null)
                    {
                        base.Payload.Data = new byte[1];
                    }
                    base.Payload.Data[0] = 0;
                    if (value)
                    {
                        base.Payload.Data[0] = 1;
                    }
                }
            }
        }

        public class LaneInformationMessage : Z1Packet
        {
            #region Constructor

            public LaneInformationMessage(AccessType accessType)
                :base()
            {
                base.Payload.MessageID = 2;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = accessType;                
            }

            public LaneInformationMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Fields
            private int _laneCount = -1;

            // Properties
            public byte ActiveLanes
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[1];
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data == null) && (value > 0))
                    {
                        base.Payload.Data = new byte[(1 + (value * 8)) + value];
                    }
                    base.Payload.Data[1] = value;
                }
            }

            public byte ConfiguredLanes
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[0];
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data == null) && (value > 0))
                    {
                        base.Payload.Data = new byte[(1 + (value * 8)) + value];
                    }
                    base.Payload.Data[0] = value;
                }
            }

            public byte LaneCount
            {
                get
                {
                    if ((base.Payload != null) || ((this._laneCount >= 0) && (this._laneCount <= 0x16)))
                    {
                        if ((this._laneCount >= 0) && (this._laneCount <= 0x16))
                        {
                            return (byte)this._laneCount;
                        }
                        if ((base.Payload.MessageSubID > 0) && (base.Payload.MessageSubID <= 0x16))
                        {
                            this._laneCount = base.Payload.MessageSubID;
                            return (byte)this._laneCount;
                        }
                    }
                    return 0;
                }
                set
                {
                    if ((value < 0) || (value > 0x16))
                    {
                        value = 0x16;
                    }
                    if ((base.Payload.Data == null) && (value > 0))
                    {
                        base.Payload.Data = new byte[(1 + (value * 8)) + value];
                    }
                    this._laneCount = base.Payload.MessageSubID = value;
                }
            }

            public LaneInformation[] Lanes
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    int laneCount = this.LaneCount;
                    LaneInformation[] informationArray = new LaneInformation[laneCount];
                    for (int i = 0; i < laneCount; ++i) informationArray[i] = new LaneInformation();
                    for (int j = 0; j < laneCount; ++j)
                    {
                        int index = 2 + (j * 10);
                        int bufferOffset = 3 + (j * 10);
                        int num5 = 11 + (j * 10);
                        if (((index >= base.Payload.Data.Length) || (bufferOffset >= base.Payload.Data.Length)) || (num5 >= base.Payload.Data.Length))
                        {
                            return informationArray;
                        }
                        informationArray[j].State = base.Payload.Data[index];
                        informationArray[j].Description = base.CreateString(base.Payload.Data, bufferOffset, 8);
                        if (base.Payload.Data[num5] == 0)
                        {
                            informationArray[j].Direction = 'R';
                        }
                        else
                        {
                            informationArray[j].Direction = 'L';
                        }
                    }
                    return informationArray;
                }
                set
                {
                    int valueLength = value.Length;
                    int laneCount = this.LaneCount;
                    this.LaneCount = (byte)valueLength;
                    base.Payload.Data = new byte[2 + (laneCount * 10)];
                    base.Payload.Data[0] = (byte)laneCount;
                    int num = 0;
                    for (int i = 0; i < laneCount; ++i)
                    {
                        int bufferOffset = 2 + (i * 10);
                        int num4 = 3 + (i * 10);
                        int index = 11 + (i * 10);
                        if (value[i].State == 0)
                        {
                            num++;
                        }
                        if (((bufferOffset >= base.Payload.Data.Length) || (num4 >= base.Payload.Data.Length)) || (index >= base.Payload.Data.Length))
                        {
                            break;
                        }
                        base.StoreBytesToBuffer(base.Payload.Data, bufferOffset, 1, value[i].State);
                        base.StoreBytesToBuffer(base.Payload.Data, num4, 8, value[i].Description);
                        base.Payload.Data[index] = 0;
                        if (value[i].Direction == 'L')
                        {
                            base.Payload.Data[index] = 1;
                        }
                    }
                    base.Payload.Data[1] = (byte)num;
                }
            }
        }

        public class IntervalDataMessage : Z1Packet
        {
            #region Constructor

            public IntervalDataMessage(DataType dataType)
                :base()
            {
                base.Payload.MessageID = 0x72;
                base.Payload.MessageSubID = (byte)dataType;
                base.Payload.Access = AccessType.Read;
                base.Payload.Data = new byte[4];
            }

            public IntervalDataMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Fields
            private bool _negative = false;
            private bool _valid = false;

            // Properties
            public int AveOccupancy
            {
                get
                {
                    float num = this.AveOccupancy_F + 0.5f;
                    return (int)num;
                }
            }

            public float AveOccupancy_F
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateFloat(base.Payload.Data, 0x16, 2);
                    }
                    return 0f;
                }
            }

            public int AveSpeed
            {
                get
                {
                    float num = this.AveSpeed_F;
                    if (num > 0f)
                    {
                        num += 0.5f;
                    }
                    else
                    {
                        num -= 0.5f;
                    }
                    return (int)num;
                }
            }

            public float AveSpeed_F
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return 0f;
                    }
                    float num = base.CreateFloat(base.Payload.Data, 0x10, 3, ref this._valid, ref this._negative);
                    if (this._negative)
                    {
                        num *= -1f;
                    }
                    return num;
                }
            }

            public BinData[] BinsOfData
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    int index = 0x21;
                    int length = 3;
                    List<BinData> list = new List<BinData>();
                    int payloadDataLength = base.Payload.Data.Length;
                    while ((index < payloadDataLength) && (base.Payload.Data[index] > 0))
                    {
                        int bins = base.Payload.Data[index + 1];
                        BinData data = new BinData((BinByType)base.Payload.Data[index], bins);
                        int[] numArray = new int[bins];
                        for (int i = 0; i < bins; ++i) numArray[i] = base.CreateInt(base.Payload.Data, (index + (i * length)) + 2, length);
                        data.Bins = numArray;
                        list.Add(data);
                        index += (bins * length) + 2;
                    }
                    return list.ToArray();
                }
            }

            public float Gap
            {
                get
                {
                    return (((float)this.Gap_M) / 1000f);
                }
            }

            public int Gap_M
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 30, 3);
                    }
                    return 0;
                }
            }

            public float Headway
            {
                get
                {
                    return (((float)this.Headway_M) / 1000f);
                }
            }

            public int Headway_M
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x1b, 3);
                    }
                    return 0;
                }
            }

            public int IndexNumber
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0, 3);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 0, 3, value);
                }
            }

            public int IntervalDuration
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 12, 2);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 12, 2, value);
                }
            }

            public int LaneOrApproach
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 3, 1);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 3, 1, value);
                }
            }

            public int NumberOfApproaches
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 15, 1);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 15, 1, value);
                }
            }

            public int NumberOfLanes
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 14, 1);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 14, 1, value);
                }
            }

            public int Speed85p
            {
                get
                {
                    float num = this.Speed85p_F;
                    if (num > 0f)
                    {
                        num += 0.5f;
                    }
                    else
                    {
                        num -= 0.5f;
                    }
                    return (int)num;
                }
            }

            public float Speed85p_F
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return 0f;
                    }
                    float num = base.CreateFloat(base.Payload.Data, 0x18, 3, ref this._valid, ref this._negative);
                    if (this._negative)
                    {
                        num *= -1f;
                    }
                    return num;
                }
            }

            public Timestamp Timestamp
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    int num = 4;
                    byte[] data = new byte[8];
                    int dataLength = data.Length;
                    for (int i = 0; i < dataLength; ++i)
                    {
                        if ((i + num) < base.Payload.Data.Length)
                        {
                            data[i] = base.Payload.Data[i + num];
                        }
                    }
                    return new Timestamp(data, true);
                }
            }

            public int Volume
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x13, 3);
                    }
                    return 0;
                }
            }                      
        }

        public class IndexedLaneGroupDataMessage : Z1Packet
        {
            #region Constructor

            public IndexedLaneGroupDataMessage()
                :base()
            {
                base.Payload.MessageID = 0x70;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = AccessType.Read;
                base.Payload.Data = new byte[3];
            }

            public IndexedLaneGroupDataMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Fields
            private bool _negative = false;
            private bool _valid = false;

            // Properties
            public int AveOccupancy
            {
                get
                {
                    float num = this.AveOccupancy_F + 0.5f;
                    return (int)num;
                }
            }

            public float AveOccupancy_F
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateFloat(base.Payload.Data, 0x11, 2);
                    }
                    return 0f;
                }
            }

            public int AveSpeed
            {
                get
                {
                    float num = this.AveSpeed_F;
                    if (num > 0f)
                    {
                        num += 0.5f;
                    }
                    else
                    {
                        num -= 0.5f;
                    }
                    return (int)num;
                }
            }

            public float AveSpeed_F
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return 0f;
                    }
                    float num = base.CreateFloat(base.Payload.Data, 11, 3, ref this._valid, ref this._negative);
                    if (this._negative)
                    {
                        num *= -1f;
                    }
                    return num;
                }
                set
                {
                    base.Payload.Data = base.StoreBytesToBuffer(base.Payload.Data, 11, 3, value);
                }
            }

            public int[] Classification
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    int num = 0x13;
                    int num2 = 4;
                    int length = 3;
                    int[] numArray = new int[num2];
                    for (int i = 0; i < num2; ++i)
                    {
                        int bufferOffset = num + (i * length);
                        numArray[i] = base.CreateInt(base.Payload.Data, bufferOffset, length);
                    }
                    return numArray;
                }
            }

            public float Gap
            {
                get
                {
                    return (((float)this.Gap_M) / 1000f);
                }
            }

            public int Gap_M
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x25, 3);
                    }
                    return 0;
                }
            }

            public float Headway
            {
                get
                {
                    return (((float)this.Headway_M) / 1000f);
                }
            }

            public int Headway_M
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x22, 3);
                    }
                    return 0;
                }
            }

            public byte Index
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.MessageSubID;
                    }
                    return 0;
                }
                set
                {
                    base.Payload.MessageSubID = value;
                }
            }

            public int IndexNumber
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0, 3);
                    }
                    return 0;
                }
                set
                {
                    base.Payload.Data = base.StoreBytesToBuffer(base.Payload.Data, 0, 3, value);
                }
            }

            public int NumberOfGroups
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x29, 1);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 0x29, 1, value);
                }
            }

            public int NumberOfLanes
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 40, 1);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 40, 1, value);
                }
            }

            public int Speed85p
            {
                get
                {
                    float num = this.Speed85p_F;
                    if (num > 0f)
                    {
                        num += 0.5f;
                    }
                    else
                    {
                        num -= 0.5f;
                    }
                    return (int)num;
                }
            }

            public float Speed85p_F
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return 0f;
                    }
                    float num = base.CreateFloat(base.Payload.Data, 0x1f, 3, ref this._valid, ref this._negative);
                    if (this._negative)
                    {
                        num *= -1f;
                    }
                    return num;
                }
            }

            public Timestamp Timestamp
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    byte[] data = new byte[8];
                    int dataLength = data.Length;
                    for (int i = 0; i < dataLength; ++i)
                    {
                        data[i] = base.Payload.Data[i + 3];
                    }
                    return new Timestamp(data, true);
                }
                set
                {
                    Timestamp timestamp = value.ToUniversalTime();
                    base.Payload.Data = base.StoreBytesToBuffer(base.Payload.Data, 3, 8, timestamp.Long);
                }
            }

            public int Volume
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 14, 3);
                    }
                    return 0;
                }
            }
        }

        public class IndexedLaneDataMessage : Z1Packet
        {
            #region Constructor

            public IndexedLaneDataMessage()
                :base()
            {
                base.Payload.MessageID = 0x60;
                base.Payload.MessageSubID = 0;
                base.Payload.Access = AccessType.Read;
                base.Payload.Data = new byte[3];
            }

            public IndexedLaneDataMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion
            // Fields
            private bool _negative = false;
            private bool _valid = false;

            // Properties
            public int AveOccupancy
            {
                get
                {
                    float num = this.AveOccupancy_F + 0.5f;
                    return (int)num;
                }
            }

            public float AveOccupancy_F
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateFloat(base.Payload.Data, 0x11, 2);
                    }
                    return 0f;
                }
            }

            public int AveSpeed
            {
                get
                {
                    float num = this.AveSpeed_F;
                    if (num > 0f)
                    {
                        num += 0.5f;
                    }
                    else
                    {
                        num -= 0.5f;
                    }
                    return (int)num;
                }
            }

            public float AveSpeed_F
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return 0f;
                    }
                    float num = base.CreateFloat(base.Payload.Data, 11, 3, ref this._valid, ref this._negative);
                    if (this._negative)
                    {
                        num *= -1f;
                    }
                    return num;
                }
                set
                {
                    base.Payload.Data = base.StoreBytesToBuffer(base.Payload.Data, 11, 3, value);
                }
            }

            public int[] Classification
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    int num = 0x13;
                    int num2 = 4;
                    int length = 3;
                    int[] numArray = new int[num2];
                    for (int i = 0; i < num2; ++i)
                    {
                        int bufferOffset = num + (i * length);
                        numArray[i] = base.CreateInt(base.Payload.Data, bufferOffset, length);
                    }
                    return numArray;
                }
            }

            public float Gap
            {
                get
                {
                    return (((float)this.Gap_M) / 1000f);
                }
            }

            public int Gap_M
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x25, 3);
                    }
                    return 0;
                }
            }

            public float Headway
            {
                get
                {
                    return (((float)this.Headway_M) / 1000f);
                }
            }

            public int Headway_M
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x22, 3);
                    }
                    return 0;
                }
            }

            public byte Index
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.MessageSubID;
                    }
                    return 0;
                }
                set
                {
                    base.Payload.MessageSubID = value;
                }
            }

            public int IndexNumber
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0, 3);
                    }
                    return 0;
                }
                set
                {
                    base.Payload.Data = base.StoreBytesToBuffer(base.Payload.Data, 0, 3, value);
                }
            }

            public int Speed85p
            {
                get
                {
                    float num = this.Speed85p_F;
                    if (num > 0f)
                    {
                        num += 0.5f;
                    }
                    else
                    {
                        num -= 0.5f;
                    }
                    return (int)num;
                }
            }

            public float Speed85p_F
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return 0f;
                    }
                    float num = base.CreateFloat(base.Payload.Data, 0x1f, 3, ref this._valid, ref this._negative);
                    if (this._negative)
                    {
                        num *= -1f;
                    }
                    return num;
                }
            }

            public Timestamp Timestamp
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    byte[] data = new byte[8];
                    int dataLength = data.Length;
                    for (int i = 0; i < dataLength; ++i)
                    {
                        data[i] = base.Payload.Data[i + 3];
                    }
                    return new Timestamp(data, true);
                }
                set
                {
                    Timestamp timestamp = value.ToUniversalTime();
                    base.Payload.Data = base.StoreBytesToBuffer(base.Payload.Data, 3, 8, timestamp.Long);
                }
            }

            public int Volume
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 14, 3);
                    }
                    return 0;
                }
            }
        }

        public class IndexedGroupDataMessage : IndexedLaneDataMessage
        {
            #region Constructor

            public IndexedGroupDataMessage()
                :base()
            {
                base.Payload.MessageID = 0x61;
                base.Payload.MessageSubID = 0;
                base.Payload.Access = AccessType.Read;
                base.Payload.Data = new byte[3];
            }

            public IndexedGroupDataMessage(Z1Packet packet)
                : base(packet)
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion
        }

        public class IDConfigurationMessage : Z1Packet
        {
            #region Constructor

            public IDConfigurationMessage(AccessType accessType)
                :base()
            {
                base.Payload.MessageID = 1;
                base.Payload.MessageSubID = 0;
                base.Payload.Access = accessType;
                if (accessType == AccessType.Write)
                {
                    base.Payload.Data = new byte[4];
                }
                if (base.Header.DestinationID == 0xffff)//Broadcasting
                {
                    //addedTimeout = 0x1388
                }             
            }

            public IDConfigurationMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public int RTMSID
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[0];
                    }
                    return 0;
                }
                set
                {
                    base.Payload.Data[0] = (byte)value;
                }
            }

            public int SensorID
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 2, 2);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 2, 2, value);
                }
            }

            public int SlotSelect
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.MessageSubID;
                    }
                    return 0;
                }
                set
                {
                    base.Payload.MessageSubID = (byte)value;
                }
            }

            public int SubnetID
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[1];
                    }
                    return 0;
                }
                set
                {
                    base.Payload.Data[1] = (byte)value;
                }
            }
        }

        public class GroupInformationMessage : Z1Packet
        {
            #region Constructor

            public GroupInformationMessage(AccessType accessType)
                :base()
            {
                base.Payload.MessageID = 0x11;
                base.Payload.MessageSubID = 4;
                base.Payload.Access = accessType;
            }

            public GroupInformationMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            private int[] GetLanesInGroup(int group)
            {
                int msgGroupCount = this.MsgGroupCount;
                int num = ((1 + (msgGroupCount * 8)) + msgGroupCount) + msgGroupCount;
                byte[] lanesPerGroup = this.LanesPerGroup;
                int lanesPerGroupLenth = lanesPerGroup.Length;
                for (int i = 0; (i < lanesPerGroupLenth) && (i < group); ++i) num += lanesPerGroup[i];
                int[] numArray = new int[lanesPerGroup[group]];
                if (base.Payload.Data != null)
                {
                    for (int j = 0; (j < lanesPerGroup[group]) && ((j + num) < base.Payload.Data.Length); j++)
                    {
                        numArray[j] = base.Payload.Data[num + j];
                    }
                }
                return numArray;
            }

            private void SetLanesInGroup(int group, byte[] lanes, int count, byte[] lanesPerGroup)
            {
                int num = ((1 + (count * 8)) + count) + count;
                int lanesPerGroupLength = lanesPerGroup.Length;
                for (int i = 0; (i < lanesPerGroupLength) && (i < group); ++i)
                {
                    num += lanesPerGroup[i];
                }
                if (base.Payload.Data != null)
                {
                    int lanesLength = lanes.Length;
                    int payloadDataLength = base.Payload.Data.Length;
                    for (int j = 0; (j < lanesLength) && ((j + num) < payloadDataLength); ++j) base.Payload.Data[num + j] = lanes[j];
                }
            }

            // Properties
            public byte ConfiguredGroups
            {
                get
                {
                    if (((base.Payload != null) && (base.Payload.Data != null)) && (base.Payload.Data.Length > 0))
                    {
                        return base.Payload.Data[0];
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length > 0))
                    {
                        base.Payload.Data[0] = value;
                    }
                }
            }

            public string[] GroupDescription
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    string[] strArray = new string[this.MsgGroupCount];
                    int bufferOffset = 1;
                    int length = 8;
                    int strArrayLength = strArray.Length;
                    for (int i = 0; i < strArrayLength; ++i)
                    {
                        strArray[i] = base.CreateString(base.Payload.Data, bufferOffset, length);
                        bufferOffset += length;
                    }
                    return strArray;
                }
                set
                {
                    int bufferOffset = 1;
                    int length = 8;
                    int valueLength = value.Length;
                    for (int i = 0; i < valueLength; ++i)
                    {
                        base.Payload.Data = base.StoreBytesToBuffer(base.Payload.Data, bufferOffset, length, value[i]);
                        bufferOffset += length;
                    }
                }
            }

            private byte[] GroupDirection
            {
                get
                {
                    int msgGroupCount = this.MsgGroupCount;
                    byte[] buffer = new byte[msgGroupCount];
                    int num = (msgGroupCount * 8) + 1;
                    int bufferLength = buffer.Length;
                    for (int i = 0; i < bufferLength; ++i)
                    {
                        if ((base.Payload.Data != null) && (base.Payload.Data.Length > (i + num)))
                        {
                            buffer[i] = base.Payload.Data[i + num];
                        }
                        else
                        {
                            buffer[i] = 0x52;
                        }
                    }
                    return buffer;
                }
                set
                {
                    int num = (base.Payload.MessageSubID * 8) + 1;
                    int valueLength = value.Length;
                    int payloadDataLength = base.Payload.Data.Length;
                    if (payloadDataLength < (valueLength + num))
                    {
                        byte[] data = base.Payload.Data;
                        int dataLength = data.Length;
                        base.Payload.Data = new byte[valueLength + num];
                        payloadDataLength = base.Payload.Data.Length;
                        for (int j = 0; (j < dataLength) && (j < payloadDataLength); ++j) base.Payload.Data[j] = data[j];
                    }
                    for (int i = 0; (i < valueLength) && ((i + num) < payloadDataLength); ++i) base.Payload.Data[i + num] = value[i];
                }
            }

            public GroupInformation[] Groups
            {
                get
                {
                    int msgGroupCount = this.MsgGroupCount;
                    GroupInformation[] informationArray = new GroupInformation[msgGroupCount];
                    string[] groupDescription = this.GroupDescription;
                    byte[] groupDirection = this.GroupDirection;
                    for (int i = 0; i < msgGroupCount; ++i)
                    {
                        informationArray[i] = new GroupInformation();
                        informationArray[i].Description = groupDescription[i];
                        informationArray[i].Direction = (char)groupDirection[i];
                        informationArray[i].Lanes = this.GetLanesInGroup(i);
                    }
                    return informationArray;
                }
                set
                {
                    int length = value.Length;
                    int num2 = (((1 + (length * 8)) + length) + length) + 1;
                    byte[] lanesPerGroup = new byte[length];
                    for (int i = 0; i < length; ++i)
                    {
                        num2 += value[i].Lanes.Length;
                        lanesPerGroup[i] = (byte)value[i].Lanes.Length;
                    }
                    base.Payload.Data = new byte[num2];
                    this.MsgGroupCount = (byte)length;
                    this.LanesPerGroup = lanesPerGroup;
                    string[] strArray = new string[length];
                    byte[] buffer2 = new byte[length];
                    for (int j = 0; j < length; ++j)
                    {
                        strArray[j] = value[j].Description;
                        buffer2[j] = (byte)value[j].Direction;
                        byte[] lanes = new byte[value[j].Lanes.Length];
                        int lanesLength = lanes.Length;
                        for (int k = 0; k < lanesLength; ++k) lanes[k] = (byte)value[j].Lanes[k];
                        this.SetLanesInGroup(j, lanes, length, lanesPerGroup);
                    }
                    this.GroupDescription = strArray;
                    this.GroupDirection = buffer2;
                }
            }

            private byte[] LanesPerGroup
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    int msgGroupCount = this.MsgGroupCount;
                    byte[] buffer = new byte[msgGroupCount];
                    int num = (1 + (msgGroupCount * 8)) + msgGroupCount;
                    int bufferLength = buffer.Length;
                    int payloadDataLength = base.Payload.Data.Length;
                    for (int i = 0; i < bufferLength; ++i)
                    {
                        if ((base.Payload.Data != null) && (payloadDataLength > (num + i)))
                        {
                            buffer[i] = base.Payload.Data[num + i];
                        }
                        else
                        {
                            buffer[i] = 0;
                        }
                    }
                    return buffer;
                }
                set
                {
                    int valueLength = value.Length;
                    int num = (1 + (valueLength * 8)) + valueLength;
                    int payloadDataLength = base.Payload.Data.Length;
                    if (base.Payload.Data != null)
                    {
                        for (int i = 0; (i < valueLength) && ((i + num) < payloadDataLength); ++i)
                        {
                            base.Payload.Data[num + i] = value[i];
                        }
                    }
                }
            }

            public byte MsgGroupCount
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Length > 0))
                    {
                        return base.Payload.MessageSubID;
                    }
                    return 0;
                }
                set
                {
                    if (base.Payload.Length > 0)
                    {
                        base.Payload.MessageSubID = value;
                    }
                }
            }
        }

        public class GlobalPushEnableDisableMessage : Z1Packet
        {

            #region Constructor

            public GlobalPushEnableDisableMessage(AccessType accessType)
                :base()
            {
                base.Payload.MessageID = 13;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = accessType;
                if (accessType == AccessType.Write)
                {
                    //this.GlobalPushState = false;
                }
            }

            public GlobalPushEnableDisableMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public bool GlobalPushState
            {
                get
                {
                    return (((base.Payload != null) && (base.Payload.Data != null)) && ((base.Payload.Data.Length == 1) && (base.Payload.Data[0] == 1)));
                }
                set
                {
                    if (base.Payload.Data == null)
                    {
                        base.Payload.Data = new byte[1];
                    }
                    base.Payload.Data[0] = 0;
                    if (value)
                    {
                        base.Payload.Data[0] = 1;
                    }
                }
            }

        }

        public class PresenceMessage : Z1Packet
        {

            #region Constructor

            public PresenceMessage()
                :base()
            {
                base.Payload.MessageID = 0x68;
                base.Payload.MessageSubID = 0;
                base.Payload.Access = AccessType.Read;
            }

            public PresenceMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public byte[] PresenceArr
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data;
                    }
                    return null;
                }
            }

        }

        public class IntervalStorageMessage : Z1Packet
        {
            #region Constructor

            public IntervalStorageMessage()
                :base()
            {
                base.Payload.MessageID = 0x6a;
                base.Payload.MessageSubID = 0;
                base.Payload.Access = AccessType.Read;
            }

            public IntervalStorageMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public int Available
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x13, 3);
                    }
                    return 0;
                }
            }

            public int Capacity
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x10, 3);
                    }
                    return 0;
                }
            }

            public Timestamp FirstEntryTimestamp
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    byte[] data = new byte[8];
                    int dataLength = data.Length;
                    for (int i = 0; i < dataLength; ++i)
                    {
                        data[i] = base.Payload.Data[i];
                    }
                    return new Timestamp(data, true);
                }
            }

            public Timestamp LastEntryTimestamp
            {
                get
                {
                    if (base.Payload == null)
                    {
                        return null;
                    }
                    byte[] data = new byte[8];
                    int dataLength = data.Length;
                    for (int i = 0; i < dataLength; ++i)
                    {
                        data[i] = base.Payload.Data[i + 8];
                    }
                    return new Timestamp(data, true);
                }
            }

            public int PercentUsed
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x16, 1);
                    }
                    return 0;
                }
            }

        }

        public class HistogramDataMessage : Z1Packet
        {
            #region Constructor

            public HistogramDataMessage()
                :base()
            {
                base.Payload.MessageID = 110;
                base.Payload.MessageSubID = 0;
                base.Payload.Access = AccessType.Read;
            }

            public HistogramDataMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;

                this._peaksBinArr = new int[0x20];
                this._peaksSizeArr = new int[0x20];
                for (int i = 0; i < 0x20; ++i) this._peaksBinArr[i] = this._peaksSizeArr[i] = 0;

            }

            #endregion

            // Fields
            private int[] _peaksBinArr;
            private int[] _peaksSizeArr;

            // Properties
            public int[] PeaksBinArr
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        int payloadDataLength = base.Payload.Data.Length;
                        int peakBinArrLength = this._peaksBinArr.Length;
                        int index = 0;
                        for (int i = 0; (i < payloadDataLength) && (index < peakBinArrLength); i += 2)
                        {
                            this._peaksBinArr[index] = base.Payload.Data[i];
                            index++;
                        }
                    }
                    return this._peaksBinArr;
                }
            }

            public int[] PeaksSizeArr
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        int payloadDataLength = base.Payload.Data.Length;
                        int index = 0;
                        int peakSizeArrLength = this._peaksSizeArr.Length;
                        for (int i = 1; (i < payloadDataLength) && (index < peakSizeArrLength); i += 2)
                        {
                            this._peaksSizeArr[index] = base.Payload.Data[i];
                            index++;
                        }
                    }
                    return this._peaksSizeArr;
                }
            }

        }

        public class EventsMessage : Z1Packet
        {
            #region Constructor

            public EventsMessage()
                :base()
            {
                base.Payload.MessageID = 0x66;
                base.Payload.MessageSubID = 0;
                base.Payload.Access = AccessType.Read;
            }

            public EventsMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Fields
            private bool _negDirection;
            private bool _validSpeed;

            // Properties
            public int Classification
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x11, 1);
                    }
                    return 0;
                }
            }

            public int Duration
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 11, 3);
                    }
                    return 0;
                }
            }

            public bool IsDirectionNegative
            {
                get
                {
                    float single1 = this.Velocity_F;
                    return this._negDirection;
                }
            }

            public byte Lane
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[8];
                    }
                    return 0;
                }
            }

            public int EventLength
            {
                get
                {
                    if (((base.Payload == null) || (base.Payload.Data == null)) || (base.Payload.Data.Length < 20))
                    {
                        return 0;
                    }
                    float num = base.CreateFloat(base.Payload.Data, 0x12, 2);                    
                    return (int)num;
                }
            }

            public float MetricLength
            {
                get
                {
                    return Length_F * 3.28084f;
                }
            }

            public float Length_F
            {
                get
                {
                    if (((base.Payload != null) && (base.Payload.Data != null)) && (base.Payload.Data.Length >= 20))
                    {
                        return base.CreateFloat(base.Payload.Data, 0x12, 2);
                    }
                    return 0f;
                }
            }

            public int Range
            {
                get
                {
                    float num = this.Range_F + 0.5f;
                    return (int)num;
                }
            }

            public float Range_F
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return 0f;
                    }
                    float num = base.CreateFloat(base.Payload.Data, 9, 2);                    
                    return num;
                }
            }

            public float MetricRange
            {
                get
                {
                    return Range_F * 3.28084f;
                }
            }

            public Timestamp Timestamp
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    byte[] data = new byte[8];
                    int dataLength = data.Length;
                    for (int i = 0; i < dataLength; ++i) data[i] = base.Payload.Data[i];
                    return new Timestamp(data, true);
                }
            }

            public bool ValidEvent
            {
                get
                {
                    return ((base.Payload != null) && (base.Payload.MessageSubID == 1));
                }
            }

            public bool ValidSpeed
            {
                get
                {
                    float single1 = this.Velocity_F;
                    return this._validSpeed;
                }
            }

            public int Velocity
            {
                get
                {
                    float num = this.Velocity_F + 0.5f;
                    return (int)num;
                }
            }

            public float Velocity_F
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateFloat(base.Payload.Data, 14, 3, ref this._validSpeed, ref this._negDirection);
                    }
                    return 0f;
                }
            }
        }

        public class ActiveEventsMessage : Z1Packet
        {
            #region Constructor

            public ActiveEventsMessage()
                :base()
            {
                base.Payload.MessageID = 0x67;
                base.Payload.MessageSubID = 0;
                base.Payload.Access = AccessType.Read;
            }

            public ActiveEventsMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Fields
            private bool _negDirection = true;
            private bool _validSpeed = true;

            // Properties
            public float AveSpeed_d
            {
                get
                {
                    float range = this.Range;
                    range += 0.5f;
                    return (float)((int)range);
                }
            }

            public int Classification
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x11, 1);
                    }
                    return 0;
                }
            }

            public int Duration
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 11, 3);
                    }
                    return 0;
                }
            }

            public bool IsDirectionNegative
            {
                get
                {
                    float single1 = this.Velocity_F;
                    return this._negDirection;
                }
            }

            public byte Lane
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[8];
                    }
                    return 0;
                }
            }

            public int EventLength
            {
                get
                {
                    if (((base.Payload == null) || (base.Payload.Data == null)) || (base.Payload.Data.Length < 20))
                    {
                        return 0;
                    }
                    float num = base.CreateFloat(base.Payload.Data, 0x12, 2);
                    return (int)num;
                }
            }

            public float MetricLength
            {
                get
                {
                    return Length_F * 3.28084f;
                }
            }

            public float Length_F
            {
                get
                {
                    if (((base.Payload != null) && (base.Payload.Data != null)) && (base.Payload.Data.Length >= 20))
                    {
                        return base.CreateFloat(base.Payload.Data, 0x12, 2);
                    }
                    return 0f;
                }
            }

            public int Range
            {
                get
                {
                    float num = this.Range_F + 0.5f;
                    return (int)num;
                }
            }

            public float MetricRange
            {
                get
                {
                    return Range_F * 3.28084f;
                }
            }

            public float Range_F
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return 0f;
                    }
                    float num = base.CreateFloat(base.Payload.Data, 9, 2);                    
                    return num;
                }
            }

            public Timestamp Timestamp
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    byte[] data = new byte[8];
                    int dataLength = data.Length;
                    for (int i = 0; i < dataLength; ++i) data[i] = base.Payload.Data[i];
                    return new Timestamp(data, true);
                }
            }

            public bool ValidEvent
            {
                get
                {
                    return ((base.Payload != null) && (base.Payload.MessageSubID == 1));
                }
            }

            public bool ValidSpeed
            {
                get
                {
                    float single1 = this.Velocity_F;
                    return this._validSpeed;
                }
            }

            public int Velocity
            {
                get
                {
                    float num = this.Velocity_F + 0.5f;
                    return (int)num;
                }
            }

            public float Velocity_F
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateFloat(base.Payload.Data, 14, 3, ref this._validSpeed, ref this._negDirection);
                    }
                    return 0f;
                }
            }

        }

        public class ExpansionIDMessage : Z1Packet
        {
            #region Constructor

            public ExpansionIDMessage()
                :base()
            {
                base.Payload.MessageID = 0x1b;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = AccessType.Read;
            }

            public ExpansionIDMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public int ExpModule0
            {
                get
                {
                    if (((base.Payload != null) && (base.Payload.Data != null)) && (base.Payload.Data.Length >= 2))
                    {
                        return base.Payload.Data[0];
                    }
                    return 0xff;
                }
            }

            public int ExpModule1
            {
                get
                {
                    if (((base.Payload != null) && (base.Payload.Data != null)) && (base.Payload.Data.Length >= 2))
                    {
                        return base.Payload.Data[1];
                    }
                    return 0xff;
                }
            }
        }

        public class EventDataPushMessage : Z1Packet
        {

            #region Constructor

            public EventDataPushMessage()
                :base()
            {
                base.Payload.MessageID = 0x65;
                base.Payload.MessageSubID = 0;
                base.Payload.Access = AccessType.Read;
            }

            public EventDataPushMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public int Classification
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x11, 1);
                    }
                    return 0;
                }
            }

            public int Duration
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 11, 3);
                    }
                    return 0;
                }
            }

            public byte Lane
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[8];
                    }
                    return 0;
                }
            }

            public int Range
            {
                get
                {
                    float num = this.Range_F + 0.5f;
                    return (int)num;
                }
            }

            public float Range_F
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateFloat(base.Payload.Data, 9, 2);
                    }
                    return 0f;
                }
            }

            public Timestamp Timestamp
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    byte[] data = new byte[8];
                    int dataLength = data.Length;
                    for (int i = 0; i < dataLength; ++i) data[i] = base.Payload.Data[i];
                    return new Timestamp(data, true);
                }
            }

            public bool ValidEvent
            {
                get
                {
                    return ((base.Payload != null) && (base.Payload.MessageSubID == 1));
                }
            }

            public int Velocity
            {
                get
                {
                    float num = this.Velocity_F + 0.5f;
                    return (int)num;
                }
            }

            public float Velocity_F
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateFloat(base.Payload.Data, 14, 3);
                    }
                    return 0f;
                }
            }

        }

        public class DirectionClassificationConfigurationMessage : Z1Packet
        {
            #region Constructor

            public DirectionClassificationConfigurationMessage(AccessType accessType)
                :base()
            {
                base.Payload.MessageID = 30;
                base.Payload.Access = accessType;
                if (accessType == AccessType.Write)
                {
                    base.Payload.Data = new byte[1];
                }
                //addedTimeout = 0x3e8
            }

            public DirectionClassificationConfigurationMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public bool Enabled
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return false;
                    }
                    return (base.Payload.Data[0] == 1);
                }
                set
                {
                    base.Payload.Data = new byte[1];
                    if (value)
                    {
                        base.Payload.Data[0] = 1;
                    }
                    else
                    {
                        base.Payload.Data[0] = 0;
                    }
                }
            }

        }

        public class DataConfigurationMessage : Z1Packet
        {

            #region Constructor

            public DataConfigurationMessage(AccessType accessType)
                :base()
            {
                base.Payload.MessageID = 3;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = accessType; 
            }

            public DataConfigurationMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public int DataInterval
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0, 2);
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 2))
                    {
                        base.StoreBytesToBuffer(base.Payload.Data, 0, 2, value);
                    }
                }
            }

            public DataPushConfiguration EventPushCFG
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    byte[] buffer = new byte[6];
                    int bufferLength = buffer.Length;
                    for (int i = 0; i < bufferLength; ++i)
                    {
                        buffer[i] = base.Payload.Data[i + 3];
                    }
                    return new DataPushConfiguration { PushConfig = buffer };
                }
                set
                {
                    int pushConfigLength = value.PushConfig.Length;
                    int payloadDataLength = base.Payload.Data.Length;
                    for (int i = 0; ((i < pushConfigLength) && (i < 6)) && ((base.Payload.Data != null) && (payloadDataLength >= (i + 3))); ++i)
                    {
                        base.Payload.Data[i + 3] = value.PushConfig[i];
                    }
                }
            }

            public IntervalModeValue IntervalMode
            {
                get
                {
                    if (((base.Payload != null) && (base.Payload.Data != null)) && (base.Payload.Data.Length > 2))
                    {
                        switch (base.Payload.Data[2])
                        {
                            case 0:
                                return IntervalModeValue.Off;

                            case 1:
                                return IntervalModeValue.Loop;

                            case 2:
                                return IntervalModeValue.FillOnce;
                        }
                    }
                    return IntervalModeValue.Off;
                }
                set
                {
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 3))
                    {
                        switch (value)
                        {
                            case IntervalModeValue.Off:
                                base.Payload.Data[2] = 0;
                                return;

                            case IntervalModeValue.Loop:
                                base.Payload.Data[2] = 1;
                                return;

                            case IntervalModeValue.FillOnce:
                                base.Payload.Data[2] = 2;
                                return;

                            default:
                                return;
                        }
                    }
                }
            }

            public DataPushConfiguration IntervalPushCFG
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    byte[] buffer = new byte[6];
                    int bufferLength = buffer.Length;
                    for (int i = 0; i < bufferLength; ++i)
                    {
                        buffer[i] = base.Payload.Data[i + 9];
                    }
                    return new DataPushConfiguration { PushConfig = buffer };
                }
                set
                {
                    int pushConfigLength = value.PushConfig.Length;
                    int payloadDataLength = base.Payload.Data.Length;
                    for (int i = 0; ((i < pushConfigLength) && (i < 6)) && ((base.Payload.Data != null) && (payloadDataLength >= (i + 9))); ++i)
                    {
                        base.Payload.Data[i + 9] = value.PushConfig[i];
                    }
                }
            }

            public int LoopSeparation
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return 0;
                    }
                    float num = base.CreateFloat(base.Payload.Data, 0x15, 2);
                    int num2 = ((int)num) << 8;
                    return (num2 + ((int)((num - ((int)num)) * 256f)));
                }
                set
                {
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 0x17))
                    {
                        base.StoreBytesToBuffer(base.Payload.Data, 0x15, 2, value);
                    }
                }
            }

            public int LoopSize
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return 0;
                    }
                    float num = base.CreateFloat(base.Payload.Data, 0x17, 2);
                    int num2 = ((int)num) << 8;
                    return (num2 + ((int)((num - ((int)num)) * 256f)));
                }
                set
                {
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 0x19))
                    {
                        base.StoreBytesToBuffer(base.Payload.Data, 0x17, 2, value);
                    }
                }
            }

            public DataPushConfiguration PresencePushCFG
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    byte[] buffer = new byte[6];
                    int bufferLength = buffer.Length;
                    for (int i = 0; i < bufferLength; ++i)
                    {
                        buffer[i] = base.Payload.Data[i + 15];
                    }
                    return new DataPushConfiguration { PushConfig = buffer };
                }
                set
                {
                    int pushConfigLength = value.PushConfig.Length;
                    int payloadDataLength = base.Payload.Data.Length;
                    for (int i = 0; ((i < pushConfigLength) && (i < 6)) && ((base.Payload.Data != null) && (payloadDataLength >= (i + 15))); ++i)
                    {
                        base.Payload.Data[i + 15] = value.PushConfig[i];
                    }
                }
            }

        }

        public class DAQConfigurationMessage : Z1Packet
        {
            #region Constructor

            public DAQConfigurationMessage(AccessType accessType)
                :base()
            {
                base.Payload.MessageID = 0xc4;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = accessType;                               
            }

            public DAQConfigurationMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public int ConfigMode
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0, 2);
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data != null) && (base.Payload.Data.Length >= 2))
                    {
                        base.StoreBytesToBuffer(base.Payload.Data, 0, 2, value);
                    }
                }
            }

        }

        public class CurrentLaneGroupMessage : Z1Packet
        {

            #region Constructor

            public CurrentLaneGroupMessage()
                :base()
            {
                base.Payload.MessageID = 0x71;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = AccessType.Read;
                base.Payload.Data = new byte[3];
                //addedTimeout = 0x3e8
            }

            public CurrentLaneGroupMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Fields
            private bool _negative = false;
            private bool _valid = false;

            // Properties
            public int AveOccupancy
            {
                get
                {
                    float num = this.AveOccupancy_F + 0.5f;
                    return (int)num;
                }
            }

            public float AveOccupancy_F
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateFloat(base.Payload.Data, 0x11, 2);
                    }
                    return 0f;
                }
            }

            public int AveSpeed
            {
                get
                {
                    float num = this.AveSpeed_F;
                    if (num > 0f)
                    {
                        num += 0.5f;
                    }
                    else
                    {
                        num -= 0.5f;
                    }
                    return (int)num;
                }
            }

            public float AveSpeed_F
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return 0f;
                    }
                    float num = base.CreateFloat(base.Payload.Data, 11, 3, ref this._valid, ref this._negative);
                    if (this._negative)
                    {
                        num *= -1f;
                    }
                    return num;
                }
            }

            public int[] Classification
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    int num = 0x13;
                    int num2 = 4;
                    int length = 3;
                    int[] numArray = new int[num2];
                    for (int i = 0; i < num2; ++i)
                    {
                        int bufferOffset = num + (i * length);
                        numArray[i] = base.CreateInt(base.Payload.Data, bufferOffset, length);
                    }
                    return numArray;
                }
            }

            public float Gap
            {
                get
                {
                    return (((float)this.Gap_M) / 1000f);
                }
            }

            public int Gap_M
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x25, 3);
                    }
                    return 0;
                }
            }

            public float Headway
            {
                get
                {
                    return (((float)this.Headway_M) / 1000f);
                }
            }

            public int Headway_M
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x22, 3);
                    }
                    return 0;
                }
            }

            public byte Index
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.MessageSubID;
                    }
                    return 0;
                }
                set
                {
                    base.Payload.MessageSubID = value;
                }
            }

            public int IndexNumber
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0, 3);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 0, 3, value);
                }
            }

            public int NumberOfGroups
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x29, 1);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 0x29, 1, value);
                }
            }

            public int NumberOfLanes
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 40, 1);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 40, 1, value);
                }
            }

            public int Speed85p
            {
                get
                {
                    float num = this.Speed85p_F;
                    if (num > 0f)
                    {
                        num += 0.5f;
                    }
                    else
                    {
                        num -= 0.5f;
                    }
                    return (int)num;
                }
            }

            public float Speed85p_F
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return 0f;
                    }
                    float num = base.CreateFloat(base.Payload.Data, 0x1f, 3, ref this._valid, ref this._negative);
                    if (this._negative)
                    {
                        num *= -1f;
                    }
                    return num;
                }
            }

            public Timestamp Timestamp
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    int num = 3;
                    byte[] data = new byte[8];
                    int dataLength = data.Length;
                    for (int i = 0; i < dataLength; ++i) data[i] = base.Payload.Data[i + num];
                    return new Timestamp(data, true);
                }
            }

            public int Volume
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 14, 3);
                    }
                    return 0;
                }
            }


        }

        public class CurrentLaneDataMessage : Z1Packet
        {
            #region Constructor

            public CurrentLaneDataMessage()
                :base()
            {
                base.Payload.MessageID = 0x6b;
                base.Payload.MessageSubID = 0;
                base.Payload.Access = AccessType.Read;
                base.Payload.Data = new byte[3];
                //addedTimeout = 0x3e8
            }

            public CurrentLaneDataMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            private bool _negative = false;
            private bool _valid = false;

            // Properties
            public int AveOccupancy
            {
                get
                {
                    float num = this.AveOccupancy_F + 0.5f;
                    return (int)num;
                }
            }

            public float AveOccupancy_F
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateFloat(base.Payload.Data, 0x11, 2);
                    }
                    return 0f;
                }
            }

            public int AveSpeed
            {
                get
                {
                    float num = this.AveSpeed_F;
                    if (num > 0f)
                    {
                        num += 0.5f;
                    }
                    else
                    {
                        num -= 0.5f;
                    }
                    return (int)num;
                }
            }

            public float AveSpeed_F
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return 0f;
                    }
                    float num = base.CreateFloat(base.Payload.Data, 11, 3, ref this._valid, ref this._negative);
                    if (this._negative)
                    {
                        num *= -1f;
                    }
                    return num;
                }
            }

            public int[] Classification
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    int num = 0x13;
                    int num2 = 4;
                    int length = 3;
                    int[] numArray = new int[num2];
                    for (int i = 0; i < num2; ++i)
                    {
                        int bufferOffset = num + (i * length);
                        numArray[i] = base.CreateInt(base.Payload.Data, bufferOffset, length);
                    }
                    return numArray;
                }
            }

            public float Gap
            {
                get
                {
                    return (((float)this.Gap_M) / 1000f);
                }
            }

            public int Gap_M
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x25, 3);
                    }
                    return 0;
                }
            }

            public float Headway
            {
                get
                {
                    return (((float)this.Headway_M) / 1000f);
                }
            }

            public int Headway_M
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x22, 3);
                    }
                    return 0;
                }
            }

            public byte Index
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.MessageSubID;
                    }
                    return 0;
                }
                set
                {
                    base.Payload.MessageSubID = value;
                }
            }

            public int IndexNumber
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0, 3);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 0, 3, value);
                }
            }

            public int Speed85p
            {
                get
                {
                    float num = this.Speed85p_F;
                    if (num > 0f)
                    {
                        num += 0.5f;
                    }
                    else
                    {
                        num -= 0.5f;
                    }
                    return (int)num;
                }
            }

            public float Speed85p_F
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return 0f;
                    }
                    float num = base.CreateFloat(base.Payload.Data, 0x1f, 3, ref this._valid, ref this._negative);
                    if (this._negative)
                    {
                        num *= -1f;
                    }
                    return num;
                }
            }

            public Timestamp Timestamp
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    int num = 3;
                    byte[] data = new byte[8];
                    int dataLength = data.Length;
                    for (int i = 0; i < dataLength; ++i)
                    {
                        data[i] = base.Payload.Data[i + num];
                    }
                    return new Timestamp(data, true);
                }
            }

            public int Volume
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 14, 3);
                    }
                    return 0;
                }
            }
        }

        public class CurrentGroupDataMessage : CurrentLaneDataMessage
        {
            #region Constructor

            public CurrentGroupDataMessage()
                :base()
            {
                base.Payload.MessageID = 0x6c;
                base.Payload.MessageSubID = 0;
                base.Payload.Access = AccessType.Read;
                base.Payload.Data = new byte[3];
                //addedTimeout = 0x3e8
            }

            public CurrentGroupDataMessage(CurrentLaneDataMessage packet)
                : base(packet)
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion
        }

        public class ConfigurationUploadMessage : Z1Packet
        {
            #region Constructor

            public ConfigurationUploadMessage(AccessType accessType)
                :base()
            {
                base.Payload.MessageID = 0x16;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = accessType;
                if (accessType == AccessType.Write)
                {
                    base.Payload.Data = new byte[1];
                }
                //addedTimeout = 0x3e8
            }

            public ConfigurationUploadMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public int Channel
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[0];
                    }
                    return 0;
                }
                set
                {
                    if (base.Payload.Data == null)
                    {
                        base.Payload.Data = new byte[1];
                    }
                    base.Payload.Data[0] = (byte)value;
                }
            }

        }

        public class CommunicationProtocolMessage : Z1Packet
        {
            #region Constructor

            public CommunicationProtocolMessage(AccessType accessType)
                :base()
            {
                base.Payload.MessageID = 0x10;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = accessType;
                if (accessType == AccessType.Write)
                {
                    base.Payload.Data = new byte[2];
                }                
            }

            public CommunicationProtocolMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public bool RTMSProtocolEnabled
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return false;
                    }
                    return ((base.Payload.Data[1] == 1) || ((base.Payload.Data[1] == 0) && false));
                }
                set
                {
                    if (base.Payload.Data == null)
                    {
                        base.Payload.Data = new byte[2];
                    }
                    base.Payload.Data[1] = 0;
                    if (value)
                    {
                        base.Payload.Data[1] = 1;
                    }
                }
            }

            public bool SS105ProtocolEnabled
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return false;
                    }
                    return ((base.Payload.Data[0] == 1) || ((base.Payload.Data[0] == 0) && false));
                }
                set
                {
                    if (base.Payload.Data == null)
                    {
                        base.Payload.Data = new byte[2];
                    }
                    base.Payload.Data[0] = 0;
                    if (value)
                    {
                        base.Payload.Data[0] = 1;
                    }
                }
            }

        }

        public class ClassificationConfigurationMessage : Z1Packet
        {
            #region Constructor

            public ClassificationConfigurationMessage(AccessType accessType)
                :base()
            {
                base.Payload.MessageID = 0x13;
                base.Payload.Access = accessType;
            }

            public ClassificationConfigurationMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public int BoundaryCount
            {
                get
                {
                    return base.Payload.MessageSubID;
                }
                set
                {
                    base.Payload.MessageSubID = (byte)value;
                    base.Payload.Data = new byte[value * 2];
                }
            }

            public float[] ClassificationBoundaries
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return new float[0];
                    }
                    //System.Collections.ArrayList list = new System.Collections.ArrayList();
                    List<float> list = new List<float>();
                    int length = 2;
                    int boundyCount = this.BoundaryCount;
                    for (int i = 0; i < boundyCount; ++i)
                    {
                        float num3 = base.CreateFloat(base.Payload.Data, i * length, length);
                        if (num3 > 0f)
                        {
                            list.Add(num3);
                        }
                    }
                    return list.ToArray();
                    /*
                    float[] numArray = new float[list.Count];
                    for (int j = 0; j < list.Count; j++)
                    {
                        numArray[j] = (float)list[j];
                    }
                    return numArray;
                     */
                }
                set
                {
                    int length = 2;
                    int valueLength = value.Length;
                    int payloadDataLenth = base.Payload.Data.Length;
                    for (int i = 0; ((i < valueLength) && (base.Payload.Data != null)) && (((i * length) + length) <= payloadDataLenth); ++i)
                    {
                        base.StoreBytesToBuffer(base.Payload.Data, i * length, length, value[i]);
                    }
                }
            }

        }

        public class BroadcastOnOffMessage : Z1Packet
        {
            #region Constructor

            public BroadcastOnOffMessage(AccessType accessType)
                :base()
            {                
                base.Payload.MessageID = 10;
                base.Payload.MessageSubID = 0;
                base.Payload.Access = accessType;
                base.Header.DestinationID = 0xffff;
                base.Header.DestinationSubID = 0xff;
                if (accessType == AccessType.Write)
                {
                    base.Payload.Data = new byte[1];
                }
                //ignoreTimeout
            }

            public BroadcastOnOffMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public BroadcastState MsgState
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return (BroadcastState)base.Payload.Data[0];
                    }
                    return BroadcastState.off;
                }
                set
                {
                    if (value == BroadcastState.on)
                    {
                        base.Payload.Data[0] = 1;
                    }
                    else
                    {
                        base.Payload.Data[0] = 0;
                    }
                }
            }

            public BroadcastType MsgType
            {
                set
                {
                    base.Payload.MessageSubID = (byte)value;
                }
            }
        }

        public class ActiveLaneInformationMessage : Z1Packet
        {
            #region Constructor

            public ActiveLaneInformationMessage(AccessType accessType)
                :base()
            {
                base.Payload.MessageID = 0x17;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = AccessType.Read;
                //addedTimeout = 0x3e8
            }

            public ActiveLaneInformationMessage(Z1Packet packet)
                :base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            #endregion

            // Properties
            public byte ActiveLanes
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[0];
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data == null) && (value > 0))
                    {
                        base.Payload.Data = new byte[(1 + (value * 8)) + value];
                    }
                    base.Payload.Data[0] = value;
                }
            }

            public ActiveLaneInformation[] Lanes
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    int num = base.Payload.Data[0];
                    ActiveLaneInformation[] informationArray = new ActiveLaneInformation[num];
                    for (int i = 0; i < num; ++i)
                    {
                        informationArray[i] = new ActiveLaneInformation();
                    }
                    for (int j = 0; j < num; ++j)
                    {
                        int bufferOffset = 1 + (j * 9);
                        int index = 9 + (j * 9);
                        int payloadDataLength = base.Payload.Data.Length;
                        if ((bufferOffset >= payloadDataLength) || (index >= payloadDataLength))
                        {
                            return informationArray;
                        }
                        informationArray[j].Description = base.CreateString(base.Payload.Data, bufferOffset, 8);
                        if (base.Payload.Data[index] == 0)
                        {
                            informationArray[j].Direction = 'R';
                        }
                        else
                        {
                            informationArray[j].Direction = 'L';
                        }
                    }
                    return informationArray;
                }
                set
                {
                    int valueLength = value.Length;
                    base.Payload.Data = new byte[1 + (valueLength * 9)];
                    for (int i = 0; i < valueLength; ++i)
                    {
                        int bufferOffset = 1 + (i * 9);
                        int index = 9 + (i * 9);
                        if ((bufferOffset >= base.Payload.Data.Length) || (index >= base.Payload.Data.Length))
                        {
                            break;
                        }
                        base.StoreBytesToBuffer(base.Payload.Data, bufferOffset, 8, value[i].Description);
                        base.Payload.Data[index] = 0;
                        if (value[i].Direction == 'L')
                        {
                            base.Payload.Data[index] = 1;
                        }
                    }
                    base.Payload.Data[0] = (byte)value.Length;
                    base.Payload.MessageSubID = (byte)value.Length;
                }
            }

        }

        public class BaseUpgradeMessage : Z1Packet
        {
            // Fields
            private uint[] _crcTable;
            protected byte[] _dataArr;
            protected bool _dataReady;
            protected bool _cancelUpload;
            public static uint CRC_32_INITIAL_CRC_VAL = uint.MaxValue;
            public static int MAX_BYTES_PER_MSG = 240;

            // Events
            public event ResetEvent SensorReset;

            public event UploadEvent UploadProgress;

            // Methods
            public BaseUpgradeMessage()
                :base()
            {
                this.Payload.MessageSubID = 0;         
                this._crcTable = new uint[0x100];
                this.GenerateCrc32Table();
            }

            public uint CalculateCRC32()
            {
                uint num = CRC_32_INITIAL_CRC_VAL;
                int num1 = (this._dataArr.Length - 4) / 4;
                uint num2 = 0;
                int dataArrLength = this._dataArr.Length;
                for (int i = 0; i < (dataArrLength - 4); i += 4)
                {
                    num2 = (uint)(this._dataArr[i] << 0x18);
                    uint num4 = num2 & 0xff000000;
                    num2 = (uint)(this._dataArr[i + 1] << 0x10);
                    num4 += num2 & 0xff0000;
                    num2 = (uint)(this._dataArr[i + 2] << 8);
                    num4 += num2 & 0xff00;
                    num2 = this._dataArr[i + 3];
                    num4 += num2 & 0xff;
                    int index = (int)(num ^ num4);
                    index &= 0xff;
                    num = (num >> 8) & 0xffffff;
                    num ^= this._crcTable[index];
                    num4 = (num4 >> 8) & 0xffffff;
                    index = (int)(num ^ num4);
                    index &= 0xff;
                    num = (num >> 8) & 0xffffff;
                    num ^= this._crcTable[index];
                    num4 = (num4 >> 8) & 0xffffff;
                    index = (int)(num ^ num4);
                    index &= 0xff;
                    num = (num >> 8) & 0xffffff;
                    num ^= this._crcTable[index];
                    num4 = (num4 >> 8) & 0xffffff;
                    index = (int)(num ^ num4);
                    index &= 0xff;
                    num = (num >> 8) & 0xffffff;
                    num ^= this._crcTable[index];
                }
                return num;
            }

            private void GenerateCrc32Table()
            {
                uint num2 = 0xedb88320;
                for (int i = 0; i < 0x100; ++i)
                {
                    uint num = (uint)i;
                    for (int j = 8; j > 0; --j)
                    {
                        if ((num & 1) != 0)
                        {
                            num = num >> 1;
                            num &= 0x7fffffff;
                            num ^= num2;
                        }
                        else
                        {
                            num = num >> 1;
                            num &= 0x7fffffff;
                        }
                    }
                    this._crcTable[i] = num;
                }
            }

            public bool StoreData(Stream stream)
            {
                if ((stream == null) || (stream.Length < 4L))
                {
                    return false;
                }
                this._dataArr = new byte[stream.Length];
                int num = 0;
                int dataArrLength = this._dataArr.Length;
                for (int i = 0; i < dataArrLength; ++i)
                {
                    num = stream.ReadByte();
                    if (num != -1)
                    {
                        this._dataArr[i] = (byte)num;
                    }
                }
                return true;
            }

            public bool Upload(SS125Messenger messenger)
            {
                bool flag = false;
                if ((this._dataArr == null) || !this.IsDataArrCRCValid)
                {
                    return false;
                }
                int num = 0;
                //new GlobalPushEnableDisableMsg(AccessType.Write).Communicate();
                messenger.SendMessage(new GlobalPushEnableDisableMessage(AccessType.Write));
                int num2 = (this._dataArr.Length / MAX_BYTES_PER_MSG) + 1;
                int num3 = 0;
                int progress = 0;
                base.Payload.MessageSubID = 0;
                for (int i = 0; i < num2; ++i)
                {
                    base.Header.SequenceNumber = (byte)i;
                    if (i == (num2 - 1))
                    {
                        base.Payload.MessageSubID = 2;
                        if (this.SensorReset != null)
                        {
                            this.SensorReset(0x4e20);
                        }
                        base.AddedTimeout = 0x4e20;
                    }
                    else if (i > 0)
                    {
                        base.Payload.MessageSubID = 1;
                    }
                    byte[] array = null;
                    if (num2 == 1)
                    {
                        base.Payload.MessageSubID = 3;
                        base.AddedTimeout = 0x4e20;
                        array = new byte[this._dataArr.Length];
                        this._dataArr.CopyTo(array, 0);
                    }
                    else
                    {
                        array = new byte[Math.Min(this._dataArr.Length - num3, MAX_BYTES_PER_MSG)];
                        int arrayLength = array.Length;
                        for (int j = 0; j < arrayLength; ++j) array[j] = this._dataArr[j + num3];
                        num3 += array.Length;
                    }                    
                    this.Payload = new Z1Payload(base.Payload.MessageID, base.Payload.MessageSubID, (byte)base.Payload.Access, array);
                    int num8 = 0;
                    flag = false;
                    float num9 = (((float)i) / ((float)num2)) * 100f;
                    if (num9 > (progress + 1))
                    {
                        progress = (int)num9;
                        if (this.UploadProgress != null)
                        {
                            this.UploadProgress(progress);
                        }
                    }
                    while ((num8 < 10) && !flag)
                    {
                        //flag = this.Communicate();
                        flag = !(messenger.SendMessage(this).Count < 0);
                        num8++;
                        if (!flag && (num8 < 10))
                        {
                            if (num8 > num)
                            {
                                num = num8;
                            }
                            base.Header.SequenceNumber = (byte)(base.Header.SequenceNumber - 1);
                            //CommJob._ssmceRouter.SequenceNumber = (byte)(base.RequestPacket.Header.Sequence - 1);
                            System.Threading.Thread.Sleep(300);
                        }
                        if (this._cancelUpload)
                        {
                            break;
                        }
                    }
                    if (!flag)
                    {
                        return flag;
                    }
                    if (this._cancelUpload)
                    {
                        return false;
                    }
                }
                return flag;
            }

            // Properties
            public bool IsDataArrCRCValid
            {
                get
                {
                    uint num = 0;
                    if (this._dataArr.Length > 4)
                    {
                        num = (uint)(this._dataArr[this._dataArr.Length - 4] << 0x18);
                        num += (uint)(this._dataArr[this._dataArr.Length - 3] << 0x10);
                        num += (uint)(this._dataArr[this._dataArr.Length - 2] << 8);
                        num += this._dataArr[this._dataArr.Length - 1];
                    }
                    uint num2 = this.CalculateCRC32();
                    return (num == num2);
                }
            }

            public bool IsDataReady
            {
                get
                {
                    return this._dataReady;
                }
            }

            // Nested Types
            public delegate void ResetEvent(int delay);

            public delegate void UploadEvent(int progress);
        }

        public class FPAAUploadMessage : BaseUpgradeMessage
        {
            // Methods
            public FPAAUploadMessage(Stream stream)
                :base()
            {
                base.Payload.MessageID = 0x92;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = AccessType.Write;
                base._dataReady = false;
                if ((base.StoreData(stream) && base.IsDataArrCRCValid) && this.IsDataArrValidLength)
                {
                    base._dataReady = true;
                }
            }

            // Properties
            public bool IsDataArrValidLength
            {
                get
                {
                    bool flag = false;
                    bool flag2 = false;
                    if ((base._dataArr.Length % 4) == 0)
                    {
                        flag = true;
                    }
                    if ((base._dataArr.Length / 4) <= 0x3fe)
                    {
                        flag2 = true;
                    }
                    return (flag && flag2);
                }
            }
        }

        public class FPGAUploadMessage : BaseUpgradeMessage
        {
            // Fields
            public static int FPAA_NUM_CHARS_PER_BYTE = 4;
            public static int FPGA_NUM_BYTES_PER_LINE = 8;
            public static int FPGA_NUM_CHARS_IN_HEADER = 0xc5;
            public static int FPGA_NUM_CHARS_PER_LINE = 10;
            public static int FPGA_NUM_LINES_IN_HEADER = 7;

            // Methods
            public FPGAUploadMessage(Stream stream)
            {
                base.Payload.MessageID = 0x91;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = AccessType.Write;
                base._dataReady = false;
                if ((base.StoreData(stream) && base.IsDataArrCRCValid) && this.IsDataArrValidLength)
                {
                    base._dataReady = true;
                }
            }

            // Properties
            public bool IsDataArrValidLength
            {
                get
                {
                    bool flag = false;
                    bool flag2 = false;
                    if ((base._dataArr.Length % 4) == 0)
                    {
                        flag = true;
                    }
                    if ((base._dataArr.Length / 4) <= 0xec00)
                    {
                        flag2 = true;
                    }
                    return (flag && flag2);
                }
            }
        }

        public class DSPCodeUploadMesssage : BaseUpgradeMessage
        {
            // Methods
            public DSPCodeUploadMesssage(Stream stream, long length)
            {
                base.Payload.MessageID = 0x90;
                base.Payload.MessageSubID = 0;
                base.Payload.Access = AccessType.Write;
                base._dataReady = false;
                if ((base.StoreData(stream) && base.IsDataArrCRCValid) && this.IsDataArrValidLength)
                {
                    base._dataReady = true;
                }
            }

            // Properties
            public bool IsDataArrValidLength
            {
                get
                {
                    bool flag = false;
                    bool flag2 = false;
                    if ((base._dataArr.Length % 4) == 0)
                    {
                        flag = true;
                    }
                    if ((base._dataArr.Length / 4) <= 0x20000)
                    {
                        flag2 = true;
                    }
                    return (flag && flag2);
                }
            }
        }

        #region 1.41 Firmware

        public class BulkIntervalDataMessage : Z1Packet
        {
            #region Constructor

            public BulkIntervalDataMessage()
                : base()
            {
                base.Payload.MessageID = 0x73;
                base.Payload.MessageSubID = 1;
                base.Payload.Access = AccessType.Read;
                base.Payload.Data = new byte[8];
                base.AddedTimeout = 0x3e8;
                base.HasMultipleResponses = true;
            }

            public BulkIntervalDataMessage(Z1Packet packet) : base(packet) { }

            #endregion

            // Fields
            private bool _negative = false;
            private bool _valid = false;

            // Properties
            public int AveOccupancy
            {
                get
                {
                    float num = this.AveOccupancy_F + 0.5f;
                    return (int)num;
                }
            }

            public float AveOccupancy_F
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateFloat(base.Payload.Data, 0x17, 2);
                    }
                    return 0f;
                }
            }

            public int AveSpeed
            {
                get
                {
                    float num = this.AveSpeed_F;
                    if (num > 0f)
                    {
                        num += 0.5f;
                    }
                    else
                    {
                        num -= 0.5f;
                    }
                    return (int)num;
                }
            }

            public float AveSpeed_F
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return 0f;
                    }
                    float num = base.CreateFloat(base.Payload.Data, 0x11, 3, ref this._valid, ref this._negative);
                    if (this._negative)
                    {
                        num *= -1f;
                    }
                    return num;
                }
            }

            public BinData[] BinsOfData
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    int index = 0x22;
                    int length = 3;
                    List<BinData> list = new List<BinData>();
                    int payloadDataLength = base.Payload.Data.Length;
                    while ((index < payloadDataLength) && (base.Payload.Data[index] > 0))
                    {
                        int bins = base.Payload.Data[index + 1];
                        BinData data = new BinData((BinByType)base.Payload.Data[index], bins);
                        int[] numArray = new int[bins];
                        for (int i = 0; i < bins; ++i) numArray[i] = base.CreateInt(base.Payload.Data, (index + (i * length)) + 2, length);
                        data.Bins = numArray;
                        list.Add(data);
                        index += (bins * length) + 2;
                    }
                    return list.ToArray();
                }
            }

            public float Gap
            {
                get
                {
                    return (((float)this.Gap_M) / 1000f);
                }
            }

            public int Gap_M
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x1f, 3);
                    }
                    return 0;
                }
            }

            public float Headway
            {
                get
                {
                    return (((float)this.Headway_M) / 1000f);
                }
            }

            public int Headway_M
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x1c, 3);
                    }
                    return 0;
                }
            }

            public int IntervalDuration
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 13, 2);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 13, 2, value);
                }
            }

            public int IntervalIndex
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0, 4);
                    }
                    return 0;
                }
            }

            public int LaneIndex
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 4, 1);
                    }
                    return -1;
                }
            }

            public int LaneOrApproach
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 4, 1);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 4, 1, value);
                }
            }

            public int NewestIndexNumber
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0, 4);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 0, 4, value);
                }
            }

            public int NumberOfApproaches
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x10, 1);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 0x10, 1, value);
                }
            }

            public int NumberOfLanes
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 15, 1);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 15, 1, value);
                }
            }

            public int OldestIndexNumber
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 4, 4);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 4, 4, value);
                }
            }

            public bool QueryByStorageIndex
            {
                set
                {
                    if (value)
                    {
                        base.Payload.MessageSubID = 1;
                    }
                    else
                    {
                        base.Payload.MessageSubID = 0;
                    }
                }
            }

            public int Speed85p
            {
                get
                {
                    float num = this.Speed85p_F;
                    if (num > 0f)
                    {
                        num += 0.5f;
                    }
                    else
                    {
                        num -= 0.5f;
                    }
                    return (int)num;
                }
            }

            public float Speed85p_F
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return 0f;
                    }
                    float num = base.CreateFloat(base.Payload.Data, 0x19, 3, ref this._valid, ref this._negative);
                    if (this._negative)
                    {
                        num *= -1f;
                    }
                    return num;
                }
            }

            public Timestamp Timestamp
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    int num = 5;
                    byte[] data = new byte[8];
                    int dataLength = data.Length;
                    int payloadDataLength = base.Payload.Data.Length;
                    for (int i = 0; i < dataLength; ++i) if ((i + num) < payloadDataLength) data[i] = base.Payload.Data[i + num];
                    return new Timestamp(data, true);
                }
            }

            public int Volume
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 20, 3);
                    }
                    return 0;
                }
            }

        }

        public class IsPasswordEnabledMessage : Z1Packet
        {
            // Methods
            public IsPasswordEnabledMessage():
                base()
            {
                base.Payload.MessageID = 0x1f;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = AccessType.Read;
            }

            public IsPasswordEnabledMessage(Z1Packet packet):base(packet){}

            // Properties
            public bool PasswordEnabled
            {
                get
                {
                    if ((base.Payload.Data == null) || (base.Payload.Data.Length != 1))
                    {
                        return false;
                    }
                    if (base.Payload.Data[0] == 0)
                    {
                        return false;
                    }
                    return true;
                }
            }
        }

        public class ChangePasswordMessage : Z1Packet
        {
            // Methods
            public ChangePasswordMessage()
                :base()
            {
                base.Payload.MessageID = 0x21;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = AccessType.Write;
                base.AddedTimeout = 0x7d0;
            }

            public ChangePasswordMessage(Z1Packet packet) : base(packet) { }

            public void SetPassword(string oldPassword, string newPassword)
            {
                SensorLoginMessage msg = new SensorLoginMessage(AccessType.Read);
                string str = msg.EncodePassword(oldPassword);
                string str2 = msg.EncodePassword(newPassword);
                base.Payload.Data = new byte[(str.Length + str2.Length) + 2];
                base.Payload.Data[0] = (byte)str.Length;
                base.Payload.Data[str.Length + 1] = (byte)str2.Length;
                int num = 1;
                int strLength = str.Length;
                for (int i = 0; i < strLength; ++i)
                {
                    base.Payload.Data[i + num] = (byte)str[i];
                }
                num = str.Length + 2;
                int str2Length = str2.Length;
                for (int j = 0; j < str2Length; ++j) base.Payload.Data[j + num] = (byte)str2[j];
            }

            // Properties
            public string NewPassword
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    int num = base.Payload.Data[0] + 1;
                    string str = "";
                    //Cannot optomize because string is modified in loop
                    for (int i = 0; i < str.Length; i++)
                    {
                        str = str + base.Payload.Data[(num + 1) + i];
                    }
                    return str;
                }
            }

            public string OldPassword
            {
                get
                {
                    string str = "";
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        //Cannot optomize because string is modified in loop
                        for (int i = 0; i < base.Payload.Data[0]; i++)
                        {
                            str = str + base.Payload.Data[i + 1];
                        }
                    }
                    return str;
                }
            }
        }

        public class SensorLoginMessage : Z1Packet
        {
            // Fields
            private const string _passwordKey = "jE7G*-yzV'7Bm\"%1]e]KT=f3a2C[j)wi";
            private const int PASSWORD_KEY_SIZE = 0x20;

            // Methods
            public SensorLoginMessage(AccessType access)
                : base()
            {
                base.Payload.MessageID = 0x20;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = access;
            }

            public SensorLoginMessage(AccessType access, bool logIn, string password)
                :base()
            {
                base.Payload.MessageID = 0x20;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = access;
                if (access == AccessType.Write)
                {
                    byte num = 0;
                    if (logIn)
                    {
                        num = 1;
                    }
                    base.Payload.MessageSubID = num;
                }
                this.Password = password;
            }

            public SensorLoginMessage(Z1Packet packet)
                : base(packet)
            {
            }

            public string DecodePassword(string strToDecode)
            {
                string str = "";
                int num = 0;
                int strToDecodeLength = strToDecode.Length;
                for (int i = 0; i < strToDecodeLength; ++i)
                {
                    int num3 = strToDecode[i] - _passwordKey[num];
                    if (num3 < 0)
                    {
                        num3 += 0x5f;
                    }
                    num3 += PASSWORD_KEY_SIZE;
                    str = str + ((char)num3);
                    num++;
                    if (num >= PASSWORD_KEY_SIZE)
                    {
                        num = 0;
                    }
                }
                return str;
            }

            public string EncodePassword(string strToEncode)
            {
                string str = "";
                int num = 0;
                int strToEncodeLength = strToEncode.Length;
                for (int i = 0; i < strToEncodeLength; ++i)
                {
                    int num3 = strToEncode[i] + _passwordKey[num];
                    num3 -= 0x40;
                    if (num3 > 0x5e)
                    {
                        num3 -= 0x5f;
                    }
                    num3 += PASSWORD_KEY_SIZE;
                    str = str + ((char)num3);
                    num++;
                    if (num >= PASSWORD_KEY_SIZE)
                    {
                        num = 0;
                    }
                }
                return str;
            }

            // Properties
            public bool LoggedIn
            {
                get
                {
                    return (((base.Payload.Data != null) && (base.Payload.Data.Length == 1)) && (base.Payload.Data[0] == 1));
                }
            }

            public string Password
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    string strToDecode = "";
                    int payloadDataLength = base.Payload.Data.Length;
                    for (int i = 0, end = payloadDataLength; i < end; ++i)
                    {
                        strToDecode = strToDecode + ((char)base.Payload.Data[i]);
                    }
                    return this.DecodePassword(strToDecode);
                }
                set
                {
                    int valueLength = value.Length;
                    string str = this.EncodePassword(value);
                    base.Payload.Data = new byte[valueLength];
                    for (int i = 0; i < valueLength; ++i) base.Payload.Data[i] = (byte)str[i];
                }
            }
        }

        public class PasswordEnableDisableMessage : Z1Packet
        {
            // Methods
            public PasswordEnableDisableMessage(AccessType access, bool enabled)
                : base()
            {
                base.Payload.MessageID = 0x1f;
                base.Payload.Access = access;
                if (access == AccessType.Write)
                {
                    if (enabled)
                    {
                        base.Payload.MessageSubID = 1;
                    }
                    else
                    {
                        base.Payload.MessageSubID = 0;
                    }
                    base.AddedTimeout = 0x7d0;
                }
            }

            public PasswordEnableDisableMessage(Z1Packet packet)
                : base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            // Properties
            public string Password
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    string strToDecode = "";
                    int payloadDataLength = base.Payload.Data.Length;
                    for (int i = 0; i < payloadDataLength; ++i) strToDecode = strToDecode + base.Payload.Data[i];
                    new SensorLoginMessage(AccessType.Read).DecodePassword(strToDecode);
                    return strToDecode;
                }
                set
                {
                    string str = new SensorLoginMessage(AccessType.Read).EncodePassword(value);
                    int strLength = str.Length;
                    base.Payload.Data = new byte[strLength];
                    for (int i = 0; i < strLength; ++i) base.Payload.Data[i] = (byte)str[i];
                }
            }

            public bool PasswordEnabled
            {
                get
                {
                    if (base.Payload.Access == AccessType.Read)
                    {
                        if (base.Payload.Data[0] == 0)
                        {
                            return false;
                        }
                        return true;
                    }
                    if (base.Payload.MessageSubID == 0)
                    {
                        return false;
                    }
                    return true;
                }
                set
                {
                    if (AccessType.Write == base.Payload.Access)
                    {
                        if (!value)
                        {
                            base.Payload.MessageSubID = 0;
                        }
                        else
                        {
                            base.Payload.MessageSubID = 1;
                        }
                    }
                }
            }
        }

        public class PasswordInactivityTimeoutMessage : Z1Packet
        {
            // Methods
            public PasswordInactivityTimeoutMessage(AccessType access)
                : base()
            {
                base.Payload.MessageID = 0x22;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = access;
            }

            // Properties
            public int InactivityTimeout
            {
                get
                {
                    if (base.Payload.Access == AccessType.Read)
                    {
                        return base.CreateInt(base.Payload.Data, 0, 2);
                    }
                    return base.CreateInt(base.Payload.Data, 0, 2);
                }
                set
                {
                    if (AccessType.Write == base.Payload.Access)
                    {
                        base.Payload.Data = new byte[2];
                        base.StoreBytesToBuffer(base.Payload.Data, 0, 2, value);
                    }
                }
            }
        }

        public class RetrieveTempPasswordMessage : Z1Packet
        {
            // Methods
            public RetrieveTempPasswordMessage()
                : base()
            {
                base.Payload.MessageID = 0x23;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = AccessType.Read;
            }

            public RetrieveTempPasswordMessage(Z1Packet packet)
                : base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            // Properties
            public string TempPassword
            {
                get
                {
                    if (base.Payload.Data.Length >= 1)
                    {
                        int length = base.Payload.Data[0];
                        if (base.Payload.Data.Length >= length)
                        {
                            return base.CreateString(base.Payload.Data, 1, length);
                        }
                    }
                    return "";
                }
            }
        }

        public class RadarTuningMessage2 : Z1Packet
        {
            // Methods
            public RadarTuningMessage2(AccessType access)
                : base()
            {
                base.Payload.MessageID = 0x1a;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = AccessType.Read;
                if (access == AccessType.Write)
                {
                    base.Payload.Access = AccessType.Write;
                }
                base.AddedTimeout = 0x3e8;
            }

            public RadarTuningMessage2(Z1Packet packet)
                : base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            // Properties
            public int ConfiguredLanes
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.Payload.Data[0];
                    }
                    return 0;
                }
                set
                {
                    if (base.Payload.Data == null)
                    {
                        base.Payload.Data = new byte[value + 1];
                    }
                    base.Payload.Data[0] = (byte)value;
                }
            }

            public bool[] TuningParams
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    int length = this.ConfiguredLanes;
                    bool[] flagArray = new bool[length];

                    for (int i = 0; i < length; ++i)
                    {
                        if (base.Payload.Data[1 + i] == 0)
                        {
                            flagArray[i] = false;
                        }
                        else
                        {
                            flagArray[i] = true;
                        }
                    }
                    return flagArray;
                }
                set
                {
                    if (value != null)
                    {
                        int valueLength = value.Length;
                        base.Payload.Data = new byte[valueLength + 1];
                        base.Payload.Data[0] = (byte)valueLength;
                        for (int i = 0; i < valueLength; ++i)
                        {
                            if (!value[i])
                            {
                                base.Payload.Data[1 + i] = 0;
                            }
                            else
                            {
                                base.Payload.Data[1 + i] = 1;
                            }
                        }
                    }
                }
            }
        }

        public class NonVolatileIntervalIndexesMessage : Z1Packet
        {
            // Methods
            public NonVolatileIntervalIndexesMessage()
                :base()
            {
                base.Payload.MessageID = 0x75;
                base.Payload.MessageSubID = 0;
                base.Payload.Access = AccessType.Read;
                base.AddedTimeout = 400;
            }

            public NonVolatileIntervalIndexesMessage(Z1Packet packet)
                : base()
            {
                base.Header = packet.Header;
                base.Payload = packet.Payload;
            }

            // Properties
            public int NewestIndex
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0, 4);
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data == null) || (base.Payload.Data.Length < 8))
                    {
                        base.Payload.Data = new byte[8];
                    }
                    base.StoreBytesToBuffer(base.Payload.Data, 0, 4, value);
                }
            }

            public int OldestIndex
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 4, 4);
                    }
                    return 0;
                }
                set
                {
                    if ((base.Payload.Data == null) || (base.Payload.Data.Length < 8))
                    {
                        base.Payload.Data = new byte[8];
                    }
                    base.StoreBytesToBuffer(base.Payload.Data, 4, 4, value);
                }
            }

            public Timestamp Timestamp
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    int num = 0;
                    byte[] data = new byte[8];
                    int dataLength = data.Length;
                    for (int i = 0; i < dataLength; ++i)
                    {
                        if ((i + num) < base.Payload.Data.Length)
                        {
                            data[i] = base.Payload.Data[i + num];
                        }
                    }
                    return new Timestamp(data, true);
                }
                set
                {
                    if ((base.Payload.Data == null) || (base.Payload.Data.Length < 8))
                    {
                        base.Payload.Data = new byte[8];
                    }
                    Timestamp timestamp = value.ToUniversalTime();
                    base.StoreBytesToBuffer(base.Payload.Data, 0, 8, timestamp.Long);
                }
            }
        }

        public class LoadDefaultConfigurationMessage : Z1Packet
        {            
            public LoadDefaultConfigurationMessage()
                :base()
            {
                base.Payload.MessageID = 9;
                base.Payload.MessageSubID = 0xff;
                base.Payload.Access = AccessType.Write;
                base.AddedTimeout += 0x7d0;
            }            

        }

        public class IntervalDataByTimestampMessage : Z1Packet
        {
            // Fields
            private bool _negative;
            private bool _valid;

            // Methods
            public IntervalDataByTimestampMessage(DataType type)
                :base()
            {
                base.Payload.MessageID = 0x74;
                base.Payload.MessageSubID = (byte)type;
                base.Payload.Access = AccessType.Read;
                base.Payload.Data = new byte[9];
                base.AddedTimeout = 0x3e8;
            }

            public IntervalDataByTimestampMessage(Z1Packet packet): base(packet){}

            // Properties
            public Timestamp AtTimestamp
            {
                set
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        base.Payload.Data = base.StoreBytesToBuffer(base.Payload.Data, 0, 8, value.Long);
                    }
                }
            }

            public int AveOccupancy
            {
                get
                {
                    float num = this.AveOccupancy_F + 0.5f;
                    return (int)num;
                }
            }

            public float AveOccupancy_F
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateFloat(base.Payload.Data, 0x16, 2);
                    }
                    return 0f;
                }
            }

            public int AveSpeed
            {
                get
                {
                    float num = this.AveSpeed_F;
                    if (num > 0f)
                    {
                        num += 0.5f;
                    }
                    else
                    {
                        num -= 0.5f;
                    }
                    return (int)num;
                }
            }

            public float AveSpeed_F
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return 0f;
                    }
                    float num = base.CreateFloat(base.Payload.Data, 0x10, 3, ref this._valid, ref this._negative);
                    if (this._negative)
                    {
                        num *= -1f;
                    }
                    return num;
                }
            }

            public BinData[] BinsOfData
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    int index = 0x21;
                    int length = 3;
                    List<BinData> list = new List<BinData>();
                    int payloadDataLength = base.Payload.Data.Length;
                    while ((index < payloadDataLength) && (base.Payload.Data[index] > 0))
                    {
                        int bins = base.Payload.Data[index + 1];
                        BinData data = new BinData((BinByType)base.Payload.Data[index], bins);
                        int[] numArray = new int[bins];
                        for (int i = 0; i < bins; ++i) numArray[i] = base.CreateInt(base.Payload.Data, (index + (i * length)) + 2, length);
                        data.Bins = numArray;
                        list.Add(data);
                        index += (bins * length) + 2;
                    }
                    return list.ToArray();
                }
            }

            public float Gap
            {
                get
                {
                    return (((float)this.Gap_M) / 1000f);
                }
            }

            public int Gap_M
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 30, 3);
                    }
                    return 0;
                }
            }

            public float Headway
            {
                get
                {
                    return (((float)this.Headway_M) / 1000f);
                }
            }

            public int Headway_M
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x1b, 3);
                    }
                    return 0;
                }
            }

            public int IntervalDuration
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 12, 2);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 12, 2, value);
                }
            }

            public int LaneOrApproach
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 8, 1);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 8, 1, value);
                }
            }

            public int NumberOfApproaches
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 15, 1);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 15, 1, value);
                }
            }

            public int NumberOfLanes
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 14, 1);
                    }
                    return 0;
                }
                set
                {
                    base.StoreBytesToBuffer(base.Payload.Data, 14, 1, value);
                }
            }

            public int Speed85p
            {
                get
                {
                    float num = this.Speed85p_F;
                    if (num > 0f)
                    {
                        num += 0.5f;
                    }
                    else
                    {
                        num -= 0.5f;
                    }
                    return (int)num;
                }
            }

            public float Speed85p_F
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return 0f;
                    }
                    float num = base.CreateFloat(base.Payload.Data, 0x18, 3, ref this._valid, ref this._negative);
                    if (this._negative)
                    {
                        num *= -1f;
                    }
                    return num;
                }
            }

            public Timestamp Timestamp
            {
                get
                {
                    if ((base.Payload == null) || (base.Payload.Data == null))
                    {
                        return null;
                    }
                    int num = 4;
                    byte[] data = new byte[8];
                    int dataLength = data.Length;
                    int payloadDataLength = base.Payload.Data.Length;
                    for (int i = 0; i < dataLength; ++i) if ((i + num) < payloadDataLength) data[i] = base.Payload.Data[i + num];
                    return new Timestamp(data, true);
                }
            }

            public int Volume
            {
                get
                {
                    if ((base.Payload != null) && (base.Payload.Data != null))
                    {
                        return base.CreateInt(base.Payload.Data, 0x13, 3);
                    }
                    return 0;
                }
            }
        }

        #endregion

        #endregion

        #region Transport
        /// <summary>
        /// A Class for communication with the Wavetronix SS125 HD Sensor
        /// </summary>
        public class SS125Messenger
        {

            #region Fields

            protected int currentSequenceNumber = 0;

            protected int udpBytesSent = 0;

            protected int udpBytesRecieved = 0;

            protected int tcpBytesSent = 0;

            protected int tcpBytesRecieved = 0;

            protected int sendRecieveTimeout = 10000;

            protected TcpClient tcpClient = null;

            protected NetworkStream netStream = null;

            protected UdpClient udpClient = null;

            protected IPEndPoint ipEndPoint = null;

            protected ProtocolType transportProtocol = ProtocolType.Tcp;

            protected byte destinationSubID = 0xff;

            protected ushort destinationID = 0xffff;

            protected bool busy = false;

            protected bool stop = false;

            protected List<Z1Packet> requestLog = new List<Z1Packet>();

            protected List<Z1Packet> recieveLog = new List<Z1Packet>();

            protected string description = string.Empty;

            protected string location = string.Empty;

            protected string orientation = string.Empty;

            protected string serialNumber = string.Empty;

            protected byte units = 0x00;

            protected Timestamp firstIntervalTime = null;

            protected Timestamp lastIntervalTime = null;

            protected int highestInerval = 0;

            protected DataConfigurationMessage dataConfiguration = null;

            protected ClassificationConfigurationMessage clasificationConfiguration = null;

            protected SpeedClassificationConfigurationMessage speedClassificationConfiguration = null;

            protected DirectionClassificationConfigurationMessage directionClassificationConfiguration = null;

            protected GroupInformationMessage groupInformation = null;

            protected LaneInformationMessage laneInformation = null;

            protected VersionInformationMessage versionInformation = null;

            protected SortedDictionary<DateTime, List<IntervalDataMessage>> dataDictionary = new SortedDictionary<DateTime, List<IntervalDataMessage>>();

            protected IntervalDataMessage[] latestIntervalData = null;

            protected Timestamp sensorTime = null;

            protected IDConfigurationMessage idConfiguration = null;

            #endregion

            #region Properties

            /// <summary>
            /// The current SequenceNumber which will be used in the next SendMessage
            /// </summary>
            public int SequenceNumber 
            { 
                get 
                {                 
                    return currentSequenceNumber; 
                } 
            }

            /// <summary>
            /// The last SequenceNumber used to send a Message
            /// </summary>
            public int LastSequenceNumber
            {
                get
                {
                    return currentSequenceNumber - 1;
                }
            }

            /// <summary>
            /// Advanced SequenceNumber by 1 and if higher then the allowed threshold resets to 0
            /// </summary>
            public int NextSequenceNumber
            {
                get
                {
                    if (currentSequenceNumber >= 255) currentSequenceNumber = 0;
                    return ++currentSequenceNumber;
                }
            }

            /// <summary>
            /// The amount of time our client will wait when sending and recieving messages
            /// </summary>
            public int SendRecieveTimeout
            {
                get
                {
                    return sendRecieveTimeout;
                }
                set
                {
                    sendRecieveTimeout = value;
                }
            }

            /// <summary>
            /// The total amount of sent bytes over the TCP protocol
            /// </summary>
            public int TcpSentBytes
            {
                get
                {
                    return tcpBytesSent;
                }
            }

            /// <summary>
            /// The total amount of recieved bytes over the TCP protocol
            /// </summary>
            public int TcpRecivedBytes
            {
                get
                {
                    return tcpBytesRecieved;
                }
            }

            /// <summary>
            /// The total amount of sent bytes over the UDP protocol
            /// </summary>
            public int UdpSentBytes
            {
                get
                {
                    return udpBytesSent;
                }                
            }

            /// <summary>
            /// The total amount of recieved bytes over the UDP protocol
            /// </summary>
            public int UdpRecievedBytes
            {
                get
                {
                    return udpBytesRecieved;
                }
            }

            /// <summary>
            /// The total amount of sent bytes over all protocols
            /// </summary>
            public int TotalSentBytes
            {
                get
                {
                    return tcpBytesSent + udpBytesSent;
                }
            }

            /// <summary>
            /// The total amount of recieved bytes over all protocols
            /// </summary>
            public int TotalRecievedBytes
            {
                get
                {
                    return tcpBytesRecieved + udpBytesRecieved;
                }
            }

            /// <summary>
            /// The DestinationID of the Sensor to which SendMessage will address Messages
            /// </summary>
            public ushort DestinationID
            {
                get
                {
                    return destinationID;
                }
                set
                {
                    destinationID = value;
                }
            }

            /// <summary>
            /// The DestinationSubID of the Sensor to which SendMessage will address Messages
            /// </summary>
            public byte DestinationSubID
            {
                get
                {
                    return destinationSubID;
                }
                set
                {
                    destinationSubID = value;
                }
            }

            /// <summary>
            /// The transport protocol the messenger will use to communicate with the sensor
            /// </summary>
            public ProtocolType TransportProtcol
            {
                get
                {
                    return transportProtocol;
                }
                set
                {
                    if (value != transportProtocol)
                    {
                        if (value == ProtocolType.Tcp || value == ProtocolType.Udp)
                        {
                            transportProtocol = value;
                            Initialize();
                        }
                        else throw new Exception("Only Tcp and Udp Transport is supported");
                    }
                }
            }

            /// <summary>
            /// The socket used by the messenger
            /// </summary>
            public Socket Socket
            {
                get
                {
                    if (this.transportProtocol == ProtocolType.Tcp)
                    {
                        return tcpClient.Client;
                    }
                    else return udpClient.Client;
                }
            }

            /// <summary>
            /// Indicates the messenger is busy or not with job
            /// </summary>
            public bool IsBusy
            {
                get
                {
                    return busy;
                }
            }

            /// <summary>
            /// The sensors LoopSize
            /// </summary>
            public int LoopSize
            {
                get
                {
                    return this.dataConfiguration.LoopSize;
                }
            }

            /// <summary>
            /// The sensors LoopSeperation
            /// </summary>
            public int LoopSeperation
            {
                get
                {
                    return this.dataConfiguration.LoopSeparation;
                }
            }

            /// <summary>
            /// True if the Sensor uses Metric Measurement
            /// </summary>
            public bool IsMetric
            {
                get
                {
                    return units == 0x01;
                }
            }

            /// <summary>
            /// List of packets sent to the Sensor
            /// </summary>
            public List<Z1Packet> RequestLog
            {
                get
                {
                    return requestLog;
                }
            }

            /// <summary>
            /// List of packets recieved from the Sensor
            /// </summary>
            public List<Z1Packet> RecieveLog
            {
                get
                {
                    return recieveLog;
                }
            }

            /// <summary>
            /// The amount of lanes the sensor is watching
            /// </summary>
            public int LaneCount
            {
                get
                {
                    return laneInformation.LaneCount;
                }
            }

            /// <summary>
            /// The amount of lanes the sensor is configured to watch
            /// </summary>
            public int ConfiguredLanes
            {
                get
                {
                    return laneInformation.ConfiguredLanes;
                }
            }

            /// <summary>
            /// The amount of ActiveLanes in the Sensor
            /// </summary>
            public int ActiveLanes
            {
                get
                {
                    return laneInformation.ActiveLanes;
                }
            }

            /// <summary>
            /// The LaneInformation of the Sensor
            /// </summary>
            public LaneInformation[] LaneInformation
            {
                get
                {
                    return laneInformation.Lanes;
                }
            }

            /// <summary>
            /// The GroupInformation of the Sensor
            /// </summary>
            public GroupInformation[] GroupInformation
            {
                get
                {
                    return this.groupInformation.Groups;
                }
            }

            /// <summary>
            /// The DSP Code Revision of the Sensor
            /// </summary>
            public string DSPCodeRevision
            {
                get
                {
                    return this.versionInformation.DSPCodeRev;
                }
            }

            /// <summary>
            /// The Algorithm Revision of the Sensor
            /// </summary>
            public string AlgorithmRevision
            {
                get
                {
                    return this.versionInformation.AlgorithmRev;
                }
            }

            /// <summary>
            /// The FPGA Version of the Sensor
            /// </summary>
            public string FPGAVersion
            {
                get
                {
                    return this.versionInformation.FPGAVersion;
                }
            }

            /// <summary>
            /// The FPAA Version of the Sensor
            /// </summary>
            public string FPAAVersion
            {
                get
                {
                    return this.versionInformation.FPAAVersion;                
                }
            }

            /// <summary>
            /// The SerialNumber of the Sensor
            /// </summary>
            public string SerialNumber
            {
                get
                {
                    return serialNumber;
                }
            }

            /// <summary>
            /// The Description of the Sensor
            /// </summary>
            public string Description
            {
                get
                {
                    return description.Substring(0, description.IndexOf('\0'));
                }
            }

            /// <summary>
            /// The Orientation of the Sensor
            /// </summary>
            public string Orientation
            {
                get
                {
                    return orientation;
                }
            }

            /// <summary>
            /// The Location of the Sensor
            /// </summary>
            public string Location
            {
                get
                {
                    return location.Substring(0, location.IndexOf('\0'));
                }
            }

            /// <summary>
            /// The Units the Sensor is Measuring in
            /// </summary>
            public byte Units
            {
                get
                {
                    return units;
                }
            }

            /// <summary>
            /// The DataInterval of the Sensor in Seconds
            /// </summary>
            public int DataInterval
            {
                get
                {
                    return this.dataConfiguration.DataInterval;
                }
            }

            /// <summary>
            /// The Speed Classification of the Sensor
            /// </summary>
            public float[] SpeedClassifications
            {
                get
                {
                    return this.speedClassificationConfiguration.SpeedBoundaries;
                }
            }

            /// <summary>
            /// Indicator weather the sensor is configured for Direction Classifications
            /// </summary>
            public bool DirectionClassificationsEnabled
            {
                get
                {
                    return this.directionClassificationConfiguration.Enabled;
                }
            }

            /// <summary>
            /// The Length Classifications the sensor is configured for
            /// </summary>
            public float[] LengthClassifications
            {
                get
                {
                    return this.clasificationConfiguration.ClassificationBoundaries;
                }
            }

            /// <summary>
            /// The FirstIntevalTime the Sensor has
            /// </summary>
            public DateTime FirstIntervalTime
            {
                get
                {
                    return firstIntervalTime.UniversalDateTime;
                }
            }

            /// <summary>
            /// The LastSensorTime the Sensor has
            /// </summary>
            public DateTime LastIntervalTime
            {
                get
                {
                    return lastIntervalTime.UniversalDateTime;
                }
            }

            /// <summary>
            /// The Current Time of the Sensor
            /// </summary>
            public DateTime SensorTime
            {

                get
                {
                    return this.sensorTime.UniversalDateTime;
                }
            }

            /// <summary>
            /// A DataDictionary containing any IntervalDataMessages downlaoded with the Messenger
            /// </summary>
            public SortedDictionary<DateTime, List<IntervalDataMessage>> DataDictionary
            {
                get
                {
                    return dataDictionary;
                }
            }

            /// <summary>
            /// A Array of the Latest Interval Data for each Lane
            /// </summary>
            public IntervalDataMessage[] LatestIntervalData
            {
                get
                {
                    return this.latestIntervalData;
                }
            }

            /// <summary>
            /// The ID Configuration of the Sensor
            /// </summary>
            public IDConfigurationMessage IDConfiguration
            {
                get
                {
                    return this.idConfiguration;
                }
            }

            #endregion

            #region Constructor

            public SS125Messenger(ProtocolType transportProtocol, IPEndPoint ipEndPoint)
            {
                this.transportProtocol = transportProtocol;
                this.ipEndPoint = ipEndPoint;
                this.Initialize();
            }

            #endregion

            #region Destructor

            ~SS125Messenger()
            {
                Close();   
            }

            #endregion

            #region Methods

            /// <summary>
            /// Closes all sockets and NetworkStrams and resets the Session
            /// </summary>
            public void Close()
            {
                try
                {
                    ResetSession();
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
                    udpBytesSent = tcpBytesSent = udpBytesRecieved = tcpBytesRecieved = currentSequenceNumber = 0;
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

            /// <summary>
            /// Resets the SequenceNumber to 0
            /// </summary>
            public void ResetSequenceNumber()
            {
                currentSequenceNumber = 0;
            }

            /// <summary>
            /// Clears the Request Log
            /// </summary>
            public void ClearRequestLog()
            {
                requestLog.Clear();
            }

            /// <summary>
            /// Clears the RecieveLog
            /// </summary>
            public void ClearRecieveLog()
            {
                recieveLog.Clear();
            }

            /// <summary>
            /// Resets the session by clearing the Request and Recieve Log and resetting the SequenceNumber
            /// </summary>
            public void ResetSession() 
            {
                ResetSequenceNumber();
                ClearRequestLog();
                ClearRecieveLog();
            }

            /// <summary>
            /// Send a Flag to stop the current job
            /// </summary>
            public void StopCurrentJob()
            {
                if (busy)
                {
                    stop = true;
                }
            }

            /// <summary>
            /// Downloads common information for the Sensor
            /// </summary>
            public void Initialize()
            {
                try
                {
                    if (this.transportProtocol == ProtocolType.Tcp)
                    {
                        this.tcpClient = new TcpClient();
                        this.tcpClient.Connect(this.ipEndPoint);
                        this.netStream = this.tcpClient.GetStream();
                    }
                    else
                    {
                        this.udpClient = new UdpClient();
                        this.udpClient.Connect(this.ipEndPoint);
                    }
                }
                catch
                {
                    throw;
                }
                FetchGeneralConfiguration();
                FetchVersionInformation();
                FetchIDConfiguration();
                FetchStorageInfomation();
                FetchIntervalConfiguration();
                FetchClassificationConfiguration();
                FetchSpeedClassificationConfiguration();
                FetchDirectionClassificationConfiguration();
                FetchGroupInformation();
                FetchLaneInformation();
                UpdateLaneGroupIntervalData();
                FetchSensorTime();
            }

            /// <summary>
            /// Sends a Z1Packet and Returns any responses from the transaction
            /// </summary>
            /// <param name="requestPacket">The Z1packet to Send</param>
            /// <returns>A List of Packets from the Transaction</returns>
            public List<Z1Packet> SendMessage(Z1Packet requestPacket)
            {
                //Build packet
                requestPacket.Header.SequenceNumber = (byte)NextSequenceNumber;
                requestPacket.Header.DestinationID = this.DestinationID;
                requestPacket.Header.DestinationSubID = this.DestinationSubID;
                requestPacket.Header.PayloadSize = (byte)(requestPacket.Payload.Buffer.Length - 1);
                requestPacket.Header.GenerateCRC();
                requestPacket.Payload.GenerateCRC();

                //Add to request log
                requestLog.Add(requestPacket);

                //Get bytes to send
                byte[] outBytes = requestPacket.Buffer;

                //declare responses
                List<Z1Packet> responses = new List<Z1Packet>();

                if (this.transportProtocol == ProtocolType.Tcp)
                {

                    if (!tcpClient.Connected) tcpClient.Connect(this.ipEndPoint);

                    //Set Timeouts
                    tcpClient.SendTimeout = tcpClient.ReceiveTimeout = this.SendRecieveTimeout + requestPacket.AddedTimeout;
                    
                    //Use the NetworkStream
                    netStream.Write(outBytes, 0, outBytes.Length);
                    tcpBytesSent += outBytes.Length;

                    if (!requestPacket.HasResponse)
                    {
                        return responses;
                    }
                    else if (requestPacket.HasMultipleResponses)
                    {
                        //Declare a buffer
                        List<byte> inBuffer = new List<byte>();
                        //Declare a cursor to the buffer
                        byte inByte;
                        //While the cursor has a valid value
                        while ((int)(inByte = (byte)netStream.ReadByte()) != -1)
                        {
                            //Add the curor byte to the buffer
                            inBuffer.Add(inByte);
                            //if (Socket.Available == 0) System.Threading.Thread.Sleep(requestPacket.AddedTimeout);
                            if (Socket.Available == 0) break;
                        }

                        //keep track of recieved bytes
                        tcpBytesRecieved += inBuffer.Count;

                        //set the start position
                        int position = 0;

                        //iterate until the position is less then or equal to the buffer count
                        while (position < inBuffer.Count)
                        {
                            Z1Packet response = new Z1Packet();

                            //byte[] inBytes = inBuffer.GetRange(position, response.Header.Length).ToArray();

                            byte[] inBytes = null;

                            try
                            {
                                inBytes = inBuffer.GetRange(position, response.Header.Length).ToArray();
                            }
                            catch
                            {
                                System.Threading.Thread.Sleep(requestPacket.AddedTimeout);
                                if (Socket.Available > 0)
                                {
                                    byte[] tmp = new byte[Socket.Available];
                                    tcpBytesRecieved += netStream.Read(tmp, 0, Socket.Available);
                                    inBuffer.AddRange(tmp);
                                    tmp = null;
                                    inBytes = inBuffer.GetRange(position, response.Header.Length).ToArray();
                                }
                            }

                            position += inBytes.Length;

                            response.Header.Buffer = inBytes;
                            if (!response.Header.ValidateCRC())
                            {
                                return SendMessage(requestPacket);
                            }

                            try
                            {
                                inBytes = inBuffer.GetRange(position, response.Header.PayloadSize).ToArray();
                            }
                            catch
                            {
                                System.Threading.Thread.Sleep(requestPacket.AddedTimeout);
                                if (Socket.Available > 0)
                                {
                                    byte[] tmp = new byte[Socket.Available];
                                    tcpBytesRecieved += netStream.Read(tmp, 0, Socket.Available);
                                    inBuffer.AddRange(tmp);
                                    tmp = null;
                                    inBytes = inBuffer.GetRange(position, response.Header.PayloadSize).ToArray();
                                }
                            }

                            
                            position += inBytes.Length;

                            response.Payload = new Z1Payload(inBytes);

                            if (!response.Payload.ValidateCRC())
                            {
                                return SendMessage(requestPacket);
                            }

                            responses.Add(response);
                        }
                    }
                    else
                    {
                        Z1Packet response = new Z1Packet();
                        byte[] inBytes = new byte[response.Header.Length];
                        tcpBytesRecieved += netStream.Read(inBytes, 0, inBytes.Length);
                        response.Header.Buffer = inBytes;
                        if (!response.Header.ValidateCRC())
                        {
                            return SendMessage(requestPacket);
                        }

                        inBytes = new byte[response.Header.PayloadSize];

                        tcpBytesRecieved += netStream.Read(inBytes, 0, inBytes.Length);

                        response.Payload = new Z1Payload(inBytes);

                        if (!response.Payload.ValidateCRC())
                        {
                            return SendMessage(requestPacket);
                        }

                        responses.Add(response);
                    }
                }
                else
                {

                    if (udpClient.Client.Connected) udpClient.Connect(this.ipEndPoint);

                    //Set Timeouts
                    udpClient.Client.SendTimeout = udpClient.Client.ReceiveTimeout = this.SendRecieveTimeout + requestPacket.AddedTimeout;

                    //send bytes
                    udpBytesSent += udpClient.Send(outBytes, outBytes.Length);

                    //if no response return the empty list
                    if (!requestPacket.HasResponse)
                    {
                        return responses;
                    }
                    else if (requestPacket.HasMultipleResponses)
                    {
                        //recieve the multi packets
                        List<byte> inBuffer = new List<byte>(udpClient.Receive(ref ipEndPoint));

                        //incrment the amount of bytes recieved
                        udpBytesRecieved += inBuffer.Count;

                        //set the start position
                        int position = 0;

                        //iterate until the position is less then or equal to the buffer count
                        while (position < inBuffer.Count)
                        {
                            Z1Packet response = new Z1Packet();
                            try
                            {
                                inBuffer.GetRange(position, response.Header.Length).ToArray().CopyTo(response.Header.Buffer, 0);
                            }
                            catch
                            {
                                if (Socket.Available > 0)
                                {
                                    byte[] tmp = udpClient.Receive(ref this.ipEndPoint);
                                    udpBytesRecieved += tmp.Length;
                                    inBuffer.AddRange(tmp);
                                    tmp = null;
                                    inBuffer.GetRange(position, response.Header.Length).ToArray().CopyTo(response.Header.Buffer, 0);
                                }
                            }

                            position += response.Header.Length;

                            if (!response.Header.ValidateCRC())
                            {
                                return SendMessage(requestPacket);
                                //Invalid Header Response CRC
                            }
                            
                            byte[] payload = new byte[response.Header.PayloadSize];
                            try
                            {

                                inBuffer.GetRange(position, response.Header.PayloadSize).ToArray().CopyTo(payload, 0);
                            }
                            catch
                            {
                                if (Socket.Available > 0)
                                {                                    
                                    byte[] tmp = udpClient.Receive(ref this.ipEndPoint);
                                    udpBytesRecieved += tmp.Length;
                                    inBuffer.AddRange(tmp);
                                    tmp = null;
                                    inBuffer.GetRange(position, response.Header.PayloadSize).ToArray().CopyTo(payload, 0);
                                }
                            }

                            response.Payload = new Z1Payload(payload);

                            position += response.Header.PayloadSize;

                            response.Payload = new Z1Payload(payload);

                            if (!response.Payload.ValidateCRC())
                            {
                                //System.Diagnostics.Debugger.Break();
                                return SendMessage(requestPacket);
                            }

                            responses.Add(response);
                        }
                    }
                    else
                    {
                        Z1Packet response = new Z1Packet();
                        //recv inBytes Length
                        byte[] inBytes = udpClient.Receive(ref ipEndPoint);

                        udpBytesRecieved += inBytes.Length;

                        Array.Copy(inBytes, response.Header.Buffer, response.Header.Length);

                        if (!response.Header.ValidateCRC())
                        {
                            //System.Diagnostics.Debugger.Break();
                            return SendMessage(requestPacket);
                        }

                        byte[] payload = new byte[response.Header.PayloadSize];

                        Array.Copy(inBytes, response.Header.Length, payload, 0, response.Header.PayloadSize);

                        response.Payload = new Z1Payload(payload);

                        if (!response.Payload.ValidateCRC())
                        {
                            //System.Diagnostics.Debugger.Break();
                            return SendMessage(requestPacket);
                        }

                        responses.Add(response);
                    }
                }

                //Add the recieved responses
                recieveLog.AddRange(responses);

                //return the responses
                return responses;
            }

            /// <summary>
            /// Downloads the GeneralConfiguration Information from the Sensor
            /// </summary>
            private void FetchGeneralConfiguration()
            {
                GeneralConfigurationMessage generalConfig = new GeneralConfigurationMessage(SendMessage(new GeneralConfigurationMessage(AccessType.Read))[0]);
                this.serialNumber = generalConfig.SerialNumber;
                this.units = generalConfig.Units;
                this.description = generalConfig.Description;
                this.location = generalConfig.Location;
                this.orientation = generalConfig.Orientation;
                generalConfig = null;
            }

            /// <summary>
            /// Downloads the StorageInformation from the Sensor
            /// </summary>
            private void FetchStorageInfomation()
            {
                IntervalStorageMessage intervalStorage = new IntervalStorageMessage(SendMessage(new IntervalStorageMessage())[0]);
                this.firstIntervalTime = intervalStorage.FirstEntryTimestamp;
                this.lastIntervalTime = intervalStorage.LastEntryTimestamp;
                this.highestInerval = intervalStorage.Capacity - intervalStorage.Available;
                intervalStorage = null;
            }

            /// <summary>
            /// Downloads the IntervalConfiguration from the Sensor
            /// </summary>
            private void FetchIntervalConfiguration()
            {
                this.dataConfiguration = new DataConfigurationMessage(SendMessage(new DataConfigurationMessage(AccessType.Read))[0]);                                
            }

            /// <summary>
            /// Downloads the ClassificationConfiguration from the Sensor
            /// </summary>
            private void FetchClassificationConfiguration()
            {
                this.clasificationConfiguration = new ClassificationConfigurationMessage(SendMessage(new ClassificationConfigurationMessage(AccessType.Read))[0]);
            }

            /// <summary>
            /// Downloads the SpeedClassificationConfiguration from the Sensor
            /// </summary>
            private void FetchSpeedClassificationConfiguration()
            {
                this.speedClassificationConfiguration = new SpeedClassificationConfigurationMessage(SendMessage(new SpeedClassificationConfigurationMessage(AccessType.Read))[0]);
            }

            /// <summary>
            /// Downloads the DirectionClassificationConfiguration from the Sensor
            /// </summary>
            private void FetchDirectionClassificationConfiguration()
            {
                this.directionClassificationConfiguration = new DirectionClassificationConfigurationMessage(SendMessage(new DirectionClassificationConfigurationMessage(AccessType.Read))[0]);
            }

            /// <summary>
            /// Downloads the GroupInformation from the Sensor
            /// </summary>
            private void FetchGroupInformation()
            {
                this.groupInformation = new GroupInformationMessage(SendMessage(new GroupInformationMessage(AccessType.Read))[0]);
            }

            /// <summary>
            /// Downloads the LaneInformation from the Sensor
            /// </summary>
            private void FetchLaneInformation()
            {
                this.laneInformation = new LaneInformationMessage(SendMessage(new LaneInformationMessage(AccessType.Read))[0]);
            }

            /// <summary>
            /// Downloads the CurrentTime from the Sensor
            /// </summary>
            private void FetchSensorTime()
            {
                this.sensorTime = new SystemTimeMessage(SendMessage(new SystemTimeMessage(AccessType.Read))[0]).Timestamp;
            }

            /// <summary>
            /// Downloads the Version Information from the Sensor
            /// </summary>
            private void FetchVersionInformation()
            {
                this.versionInformation = new VersionInformationMessage(SendMessage(new VersionInformationMessage())[0]);
            }

            /// <summary>
            /// Fetched the SensorID Configuration
            /// </summary>
            private void FetchIDConfiguration()
            {
                this.idConfiguration = new IDConfigurationMessage(SendMessage(new IDConfigurationMessage(AccessType.Read))[0]);
            }

            /// <summary>
            /// Downloads a index from the Sensor given a Timestamp
            /// </summary>
            /// <param name="time">The time to find the IntervalIndex for</param>
            /// <returns>The index number if found or -1 if not found</returns>
            private int FetchIndex(Timestamp time)
            {
                StoredIndexByTimestampMessage index = new StoredIndexByTimestampMessage(SendMessage(new StoredIndexByTimestampMessage()
                {
                    Timestamp = time
                })[0]);

                if (index.MessageResult > 0) return -1;

                return index.IntervalIndex;
            }

            /// <summary>
            /// Downloads a LaneGroupIntervalData for the given index
            /// </summary>
            /// <param name="index">The IntervalIndex to download</param>
            /// <returns>A List of IntervalDataMessages (1 for each lane)</returns>
            private List<IntervalDataMessage> FetchLaneGroupIntervalData(int index)
            {                

                List<Z1Packet> responses = SendMessage(new IntervalDataMessage(DataType.SavedAll)
                {
                    LaneOrApproach = 0xff,
                    IndexNumber = index,                    
                    HasMultipleResponses = true,
                    AddedTimeout = 0x3e8
                });

                List<IntervalDataMessage> convertedResponses = new List<IntervalDataMessage>();

                responses.ForEach(z1Packet =>
                {
                    convertedResponses.Add(new IntervalDataMessage(z1Packet));
                });

                if (dataDictionary.ContainsKey(convertedResponses[0].Timestamp.UniversalDateTime)) return dataDictionary[convertedResponses[0].Timestamp.UniversalDateTime];

                dataDictionary.Add(convertedResponses[0].Timestamp.UniversalDateTime, convertedResponses);
                
                responses = null;

                return dataDictionary[convertedResponses[0].Timestamp.UniversalDateTime];
            }

            /// <summary>
            /// Starts a Download job and places downloaded data in the DataDictionary
            /// </summary>
            /// <param name="start">The start time</param>
            /// <param name="end">the end time</param>
            public void DownloadData(Timestamp start, Timestamp end)
            {
                
                if (start.CompareTimestamp(end) == -1) return;

                if (start.CompareTimestamp(lastIntervalTime) != 1) return;

                if (end.CompareTimestamp(firstIntervalTime) != -1) return;

                int startIndex = FetchIndex(start);

                int endIndex = FetchIndex(end);

                int index = startIndex;
                
                if (startIndex == -1) return;
                if (endIndex == -1) return;
                if (startIndex > highestInerval) return;
                if (endIndex > highestInerval) return;                

                busy = true;

                while (!stop)
                {

                    FetchLaneGroupIntervalData(index);

                    int endCheck = FetchIndex(end);

                    if (endCheck == endIndex) index--;
                    else
                    {
                        //Determine if the index shifted more then 1
                        int difference = Math.Abs(endCheck - endIndex);
                        //If it did then increment the difference
                        if (difference > 1)
                        {
                            index += difference;
                        }
                        //maintain the new end
                        endIndex = endCheck;
                    }
                    if (index <= endIndex) break;

                }

                busy = false;
            }

            /// <summary>
            /// Downloads the most recent IntervalIndex
            /// </summary>
            public void UpdateLaneGroupIntervalData()
            {
                List<Z1Packet> responses = SendMessage(new IntervalDataMessage(DataType.SavedAll)
                {
                    LaneOrApproach = 0xff,
                    IndexNumber = 0,
                    HasMultipleResponses = true,
                    AddedTimeout = 0x3e8
                });

                List<IntervalDataMessage> convertedResponses = new List<IntervalDataMessage>();

                responses.ForEach(z1Packet =>
                {
                    convertedResponses.Add(new IntervalDataMessage(z1Packet));
                });

                if (!dataDictionary.ContainsKey(convertedResponses[0].Timestamp.UniversalDateTime))
                {
                    dataDictionary.Add(convertedResponses[0].Timestamp.UniversalDateTime, convertedResponses);
                }

                this.latestIntervalData = convertedResponses.ToArray();

                convertedResponses = null;
            }

            /// <summary>
            /// Updates the sensor to the given intervalDuration
            /// </summary>
            /// <param name="intervalSeconds">The new IntervalDuration in seconds</param>
            public void UpdateDataInterval(int intervalSeconds)
            {
                DataConfigurationMessage config = new DataConfigurationMessage(AccessType.Write)
                {
                    HasResponse = false,
                    LoopSeparation = dataConfiguration.LoopSeparation,
                    LoopSize = dataConfiguration.LoopSize,
                    IntervalPushCFG = dataConfiguration.IntervalPushCFG,
                    EventPushCFG = dataConfiguration.EventPushCFG,
                    PresencePushCFG = dataConfiguration.PresencePushCFG,
                    IntervalMode = dataConfiguration.IntervalMode,
                    DataInterval = intervalSeconds
                };
                SendMessage(config);
                SendMessage(new SaveToFlashMessage()
                {
                    HasResponse = false
                });
                FetchIntervalConfiguration();
            }

            /// <summary>
            /// Updates the Sensors Clock to the given Timestamp
            /// </summary>
            /// <param name="newTime"></param>
            public void UpdateSensorTime(Timestamp newTime)
            {                

                SendMessage(new SystemTimeMessage(AccessType.Write)
                {
                    HasResponse = false,
                    Timestamp = newTime
                });

                SendMessage(new SaveToFlashMessage()
                {
                    HasResponse = false
                });

                FetchSensorTime();
            }

            /// <summary>
            /// Updates the Sensor time to the time of the current time on this computer
            /// </summary>
            public void SynchSensorTime()
            {
                UpdateSensorTime(new Timestamp(DateTime.Now));
            }

            /// <summary>
            /// Updates the Orientation of the Sensor
            /// </summary>
            /// <param name="newOrientation">The new Orientation</param>
            public void UpdateOrientation(string newOrientation)
            {
                SendMessage(new GeneralConfigurationMessage(AccessType.Write)
                {
                    Units = this.units,
                    Location = this.location,
                    Description = this.description,
                    Orientation = newOrientation,
                });

                SendMessage(new SaveToFlashMessage()
                {
                    HasResponse = false
                });

                FetchGeneralConfiguration();
            }

            /// <summary>
            /// Updates the Description of the Sensor
            /// </summary>
            /// <param name="newDescription">The new Description</param>
            public void UpdateDescription(string newDescription)
            {
                SendMessage(new GeneralConfigurationMessage(AccessType.Write)
                {
                    Units = this.units,
                    Location = this.location,
                    Description = newDescription,
                    Orientation = this.orientation,
                });

                SendMessage(new SaveToFlashMessage()
                {
                    HasResponse = false
                });

                FetchGeneralConfiguration();

            }

            /// <summary>
            /// Updtes the Location of the Sensor
            /// </summary>
            /// <param name="newLocation">The new Location</param>
            public void UpdateLocation(string newLocation)
            {
                SendMessage(new GeneralConfigurationMessage(AccessType.Write)
                {
                    Units = this.units,
                    Location = newLocation,
                    Description = this.description,
                    Orientation = this.orientation,
                });

                SendMessage(new SaveToFlashMessage()
                {
                    HasResponse = false
                });

                FetchGeneralConfiguration();

            }

            /// <summary>
            /// Updates the Units the Sensor is measuring in
            /// </summary>
            /// <param name="unitType">The type of units the sensor should measure in (0 for Standard, 1 for Metric)</param>
            public void UpdateSensorUnits(int unitType)
            {
                if (unitType != 0 || unitType != 1) throw new ArgumentOutOfRangeException("unitType", "Cannot be less than 0 or grater than 1");
                
                SendMessage(new GeneralConfigurationMessage(AccessType.Write)
                {
                    Units = (byte)unitType,
                    Location = this.location,
                    Description = this.description,
                    Orientation = this.orientation,
                });

                SendMessage(new SaveToFlashMessage()
                {
                    HasResponse = false
                });

                FetchGeneralConfiguration();
            }

            /// <summary>
            /// Updates the SpeedBins of the Sensor
            /// </summary>
            /// <param name="newBins">the new speed classes</param>
            public void UpdateSpeedBins(float[] newBins)
            {
                SendMessage(new SpeedClassificationConfigurationMessage(AccessType.Write)
                {
                    HasResponse = true,
                    BoundaryCount = newBins.Length,
                    SpeedBoundaries = newBins
                });

                FetchGeneralConfiguration();
            }

            /// <summary>
            /// Updates the ClassificationBins of the Sensor
            /// </summary>
            /// <param name="newBins">the new classifcations</param>
            public void UpdateClassificationBins(float[] newBins)
            {
                SendMessage(new ClassificationConfigurationMessage(AccessType.Write)
                {
                    HasResponse = true,
                    BoundaryCount = newBins.Length,
                    ClassificationBoundaries = newBins
                });

                FetchGeneralConfiguration();
            }

            /// <summary>
            /// Updates the RTMS ID of the Sensor
            /// </summary>
            /// <param name="id">the new RTMS id</param>
            public void UpdateSensorRTMSID(int id)
            {
                SendMessage(new IDConfigurationMessage(AccessType.Write)
                {
                    RTMSID = id,
                    SensorID = this.idConfiguration.SensorID,
                    SlotSelect = this.idConfiguration.SlotSelect,
                    SubnetID = this.idConfiguration.SubnetID,                   
                });

                SendMessage(new SavedConfigurationMessage(AccessType.Write)
                {
                    HasResponse = false
                });

                FetchIDConfiguration();

            }

            /// <summary>
            /// Updates the SensorID of the Sensor
            /// </summary>
            /// <param name="id">the new ID of the Sensor</param>
            public void UpdateSensorID(int id)
            {
                SendMessage(new IDConfigurationMessage(AccessType.Write)
                {
                    RTMSID = this.idConfiguration.RTMSID,
                    SensorID = id,
                    SlotSelect = this.idConfiguration.SlotSelect,
                    SubnetID = this.idConfiguration.SubnetID,
                });

                SendMessage(new SavedConfigurationMessage(AccessType.Write)
                {
                    HasResponse = false
                });

                FetchIDConfiguration();
            }

            /// <summary>
            /// Updates the Sensor SubNetID
            /// </summary>
            /// <param name="id">The new Sensor SubnetID</param>
            public void UpdateSensorSubnetID(int id)
            {
                SendMessage(new IDConfigurationMessage(AccessType.Write)
                {
                    RTMSID = this.idConfiguration.RTMSID,
                    SensorID = this.idConfiguration.SubnetID,
                    SlotSelect = this.idConfiguration.SlotSelect,
                    SubnetID = id
                });

                SendMessage(new SavedConfigurationMessage(AccessType.Write)
                {
                    HasResponse = false
                });

                FetchIDConfiguration();
            }

            /// <summary>
            /// Instructs the sensor to erase all stored data
            /// </summary>
            public void EraseSensorMemory()
            {
                SendMessage(new ClearStoredIntervalDataMessage());
                FetchStorageInfomation();
            }

            /// <summary>
            /// Updates the sensor to the latest embedded firmware version and restores the configuration
            /// </summary>
            /// <param name="force">Specifies weather or not the upgrade should be forced</param>
            public void UpgradeFirmware(bool force)
            {
                this.busy = true;

                string algRev = "00009A28", dspRev = "10010304", fpaaRev = "A0006302", fpgaRev = "00000650";

                if (this.AlgorithmRevision == algRev
                    &&
                   this.DSPCodeRevision == dspRev
                    &&
                   this.FPAAVersion == fpaaRev
                    &&
                   this.FPGAVersion == fpgaRev && !force) return;

                bool upgradeDsp = force, upgradeFpga = force, upgradeFpaa = force;

                if (!force)
                {

                    if (int.Parse(this.DSPCodeRevision, System.Globalization.NumberStyles.HexNumber)
                        <
                       int.Parse(dspRev, System.Globalization.NumberStyles.HexNumber)) upgradeDsp = true;

                    if (int.Parse(this.FPAAVersion, System.Globalization.NumberStyles.HexNumber)
                        <
                       int.Parse(fpaaRev, System.Globalization.NumberStyles.HexNumber)) upgradeFpaa = true;

                    if (int.Parse(this.FPGAVersion, System.Globalization.NumberStyles.HexNumber)
                        <
                       int.Parse(fpgaRev, System.Globalization.NumberStyles.HexNumber)) upgradeFpga = true;
                }

                Stream s;

                bool flag = false;

                if (upgradeDsp)
                {
                    s = new FileStream("dspFile.bin", FileMode.Open);
                    DSPCodeUploadMesssage dspUpload = new DSPCodeUploadMesssage(s, s.Length);
                    if (!dspUpload.IsDataReady)
                    {
                        throw new Exception("DspFile Compatibility Error");
                    }
                    else
                    {
                        flag = dspUpload.Upload(this);//Shoudl probably happen after the event is registered
                        dspUpload.SensorReset += new BaseUpgradeMessage.ResetEvent(delegate(int t)
                        {
                            this.SendMessage(new RebootSensorMessage()
                            {
                                AddedTimeout = t
                            });
                        });
                    }
                    s.Close();
                    s.Dispose();
                }

                if (upgradeFpga && flag)
                {
                    s = new FileStream("fpgaFile.bin", FileMode.Open);
                    FPGAUploadMessage fpgaUpload = new FPGAUploadMessage(s);
                    if (!fpgaUpload.IsDataReady)
                    {
                        throw new Exception("FpgaFile Compatibility Error");

                    }
                    else
                    {
                        flag = fpgaUpload.Upload(this);
                        fpgaUpload.SensorReset += new BaseUpgradeMessage.ResetEvent(delegate(int t)
                        {
                            this.SendMessage(new RebootSensorMessage()
                            {
                                AddedTimeout = t
                            });
                        });
                    }
                    s.Close();
                    s.Dispose();
                }

                if (upgradeFpaa && flag)
                {
                    s = new FileStream("fpaaFile.bin", FileMode.Open);
                    FPAAUploadMessage fpaaUpload = new FPAAUploadMessage(s);
                    if (!fpaaUpload.IsDataReady)
                    {
                        throw new Exception("FpaaFile Compatibility Error");
                    }
                    else
                    {
                        flag = fpaaUpload.Upload(this);
                        fpaaUpload.SensorReset += new BaseUpgradeMessage.ResetEvent(delegate(int t)
                        {
                            this.SendMessage(new RebootSensorMessage()
                            {
                                AddedTimeout = t
                            });
                        });
                    }
                    s.Close();
                    s.Dispose();
                }

                this.UpdateDataInterval(this.DataInterval);
                this.UpdateDescription(this.Description);
                this.UpdateLocation(this.Location);
                this.UpdateOrientation(this.Orientation);
                this.UpdateSensorID(this.idConfiguration.SensorID);
                this.UpdateSensorRTMSID(this.idConfiguration.RTMSID);
                this.UpdateSensorSubnetID(this.idConfiguration.SubnetID);
                this.SynchSensorTime();
                this.UpdateSpeedBins(this.speedClassificationConfiguration.SpeedBoundaries);
                this.UpdateClassificationBins(this.clasificationConfiguration.ClassificationBoundaries);

                this.busy = false;

            }

            #region Log Stuff (NEEDS OPTOMIZATION)

            private List<string> _logBuffer = new List<string>();

            private System.Text.StringBuilder WriteHeaderToLog(string fileName)
            {                
                List<string> logBuffer = new List<string>();

                logBuffer.Add("");
                logBuffer.Add("");
                logBuffer.Add("###############################################################################################################################################################################################################################################");
                logBuffer.Add(string.Format("{0}{0,238}", "#"));
                logBuffer.Add(string.Format("{0,-27}{1,-13}: {2,-80}   {0,114}", "#", "Date", DateTime.Now.ToString("MMMM dd, yyyy")));
                logBuffer.Add(string.Format("{0,-27}{1,-13}: {2,-80}   {0,114}", "#", "SerialNumer", this.SerialNumber));
                logBuffer.Add(string.Format("{0,-27}{1,-13}: {2,-80}   {0,114}", "#", "Description", this.Description));
                logBuffer.Add(string.Format("{0,-27}{1,-13}: {2,-80}   {0,114}", "#", "Location", this.Location));
                logBuffer.Add(string.Format("{0,-27}{1,-13}: {2,-80}   {0,114}", "#", "Orientation", this.Orientation));
                logBuffer.Add("#_____________________________________________________________________________________________________________________________________________________________________________________________________________________________________________#");
                string[] strArray = new string[8];
                for (int i = 0; i < strArray.Length; i++)
                {
                    if (i < this.clasificationConfiguration.ClassificationBoundaries.Length)
                    {
                        if ((this.clasificationConfiguration.ClassificationBoundaries[i] == 255f) || (this.units != 1))
                        {
                            strArray[i] = string.Format("{0,4:0}", this.clasificationConfiguration.ClassificationBoundaries[i]);
                            if (this.clasificationConfiguration.ClassificationBoundaries[i] < 100f)
                            {
                                strArray[i] = string.Format(" {0,-3:0}", this.clasificationConfiguration.ClassificationBoundaries[i]);
                            }
                        }
                        else if (this.clasificationConfiguration.ClassificationBoundaries[i] >= 10f)
                        {
                            strArray[i] = string.Format("{0,4:0.0}", this.clasificationConfiguration.ClassificationBoundaries[i]);
                        }
                        else
                        {
                            strArray[i] = string.Format("{0,4:0.0}", this.clasificationConfiguration.ClassificationBoundaries[i]);
                        }
                    }
                    else
                    {
                        strArray[i] = "    ";
                    }
                }
                string[] strArray2 = new string[15];
                bool flag = false;
                for (int j = 0; j < strArray2.Length; j++)
                {
                    if ((j < this.speedClassificationConfiguration.SpeedBoundaries.Length) && !flag)
                    {
                        if ((this.speedClassificationConfiguration.SpeedBoundaries[j] == 255f) || (this.units != 1))
                        {
                            strArray2[j] = string.Format(" {0,-4:0}", this.speedClassificationConfiguration.SpeedBoundaries[j]);
                            if (this.speedClassificationConfiguration.SpeedBoundaries[j] < 100f)
                            {
                                strArray2[j] = string.Format("  {0,-3:0}", this.speedClassificationConfiguration.SpeedBoundaries[j]);
                            }
                            if (this.speedClassificationConfiguration.SpeedBoundaries[j] == 255f)
                            {
                                flag = true;
                            }
                        }
                        else if (this.speedClassificationConfiguration.SpeedBoundaries[j] >= 100f)
                        {
                            strArray2[j] = string.Format("{0,4:0.0}", this.speedClassificationConfiguration.SpeedBoundaries[j]);
                        }
                        else if (this.speedClassificationConfiguration.SpeedBoundaries[j] >= 10f)
                        {
                            strArray2[j] = string.Format("{0,4:0.0} ", this.speedClassificationConfiguration.SpeedBoundaries[j]);
                        }
                        else
                        {
                            strArray2[j] = string.Format("{0,4:0.0} ", this.speedClassificationConfiguration.SpeedBoundaries[j]);
                        }
                    }
                    else
                    {
                        strArray2[j] = "     ";
                    }
                }
                string str = "(" + "MPH" + ")";
                if ((this.units == 1))
                {
                    str = "(" + "KPH" + ")";
                }
                logBuffer.Add(string.Format("#{0,11}{0,9} {2,-6}{0}{0,8}  {1,-5}{0}              {3,-25}{0}{0,10}  {0,6}{0,22}  {5,49} {0, 44} {6}  {4}", new object[] { "|", "85%", "Occu", "ClassCount", "#", "SpeedBins", "DirectionBins" }));
                logBuffer.Add(string.Format("#{0,11}{0,9} {2,-6}{0} {1,-6}{0} {1,-6}{0}{3}{0}{0,10}  {0,6}    {4,-16} {0}{5}{0}{6}#", new object[] { "|", "Speed", "pancy", "_______________________________________", "SensorTime", "_______________________________________________________________________________________________", "_________________" }));
                logBuffer.Add(string.Format("# {2,-9}{0} {3,-7}{0}  {6,-5}{0} {4,-6}{0} {5,-6}{0} {7,-3}{0} {8,-3}{0} {9,-3}{0} {10,-3}{0} {11,-3}{0} {12,-3}{0} {13,-3}{0} {14,-3}{0} {15,-8}{0}  {16,-4} {0} {1} {0} {17,-3} {0} {18,-3} {0} {19,-3} {0} {20,-3} {0} {21,-3} {0} {22,-3} {0} {23,-3} {0} {24,-3} {0} {25,-3} {0} {26,-3} {0} {27,-3} {0} {28,-3} {0} {29,-3} {0} {30,-3} {0} {31,-3} {0} {32,-3} {0} {33,-3} #", new object[] { 
        "|", "YYYY-MM-DD HH:MM:SS", "NAME", "VOLUME", str, str, "(%)", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "HEADWAY", "GAP", "SP1", "SP2", "SP3", "SP4", "SP5", "SP6", "SP7", "SP8", "SP9", "SP10", "SP11", "SP12", "SP13", "SP14", "SP15", 
        "Correct", "Wrong"
     }));
                logBuffer.Add(string.Format("#          |        |       |       |       |{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|         |       |                     |{8}|{9}|{10}|{11}|{12}|{13}|{14}|{15}|{16}| {17}| {18}| {19}| {20}| {21}| {22}|         |       #", new object[] { 
        strArray[0], strArray[1], strArray[2], strArray[3], strArray[4], strArray[5], strArray[6], strArray[7], strArray2[0], strArray2[1], strArray2[2], strArray2[3], strArray2[4], strArray2[5], strArray2[6], strArray2[7], 
        strArray2[8], strArray2[9], strArray2[10], strArray2[11], strArray2[12], strArray2[13], strArray2[14]
     }));
                logBuffer.Add("###############################################################################################################################################################################################################################################");

                // Testing
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                logBuffer.ForEach(s => sb.AppendLine(s));

                string test = sb.ToString();

                //End Testing

                return sb;

            }

            private void LogInterval(List<IntervalDataMessage> msgs)
            {

                List<string[]> list = new List<string[]>();
                List<string> list2 = new List<string>();
                int num = 0;
                int num2 = 0;

                foreach (IntervalDataMessage msg in msgs)
                {                    
                    if (num < msg.NumberOfLanes)
                    {                        
                        if (this.laneInformation.Lanes != null)
                        {                            
                            if (num2 < this.laneInformation.Lanes.Length)
                            {
                                if (this.laneInformation.Lanes[num2].Description.IndexOf("\0") > -1)
                                {
                                    list2.Add(this.laneInformation.Lanes[num2].Description.Substring(0, this.laneInformation.Lanes[num2].Description.IndexOf("\0")));
                                }
                                else
                                {
                                    list2.Add(this.laneInformation.Lanes[num2].Description);
                                }
                            }
                            else
                            {
                                list2.Add("Unknown(" + num2 + ")");
                            }
                        }
                        else
                        {
                            list2.Add("Lane " + (msg.LaneOrApproach + 1));
                        }
                        num2++;
                    }
                    else if ((num - msg.NumberOfLanes) < msg.NumberOfApproaches)
                    {
                        int num6 = msg.LaneOrApproach - msg.NumberOfLanes;
                        if (this.groupInformation.GroupDescription != null)
                        {
                            if (num6 < this.groupInformation.GroupDescription.Length)
                            {
                                if (this.groupInformation.GroupDescription[num6].IndexOf("\0") > -1)
                                {
                                    list2.Add(this.groupInformation.GroupDescription[num6].Substring(0, this.groupInformation.GroupDescription[num6].IndexOf("\0")));
                                }
                                else
                                {
                                    list2.Add(this.groupInformation.GroupDescription[num6]);
                                }
                            }
                            else
                            {
                                list2.Add("Unknown(" + num6 + ")");
                            }
                        }
                        else
                        {
                            list2.Add("Appr. " + (num6 + 1));
                        }
                    }
                    num++;
                    if (msg.NumberOfLanes == 0)
                    {
                        list2.Add("No Lanes");
                        list2.Add("Defined");
                        list2.Add("-");
                        list2.Add("-");
                        list2.Add("-");
                        list2.Add("-");
                        list2.Add("-");
                    }
                    else
                    {
                        list2.Add("" + msg.Volume);
                        list2.Add(string.Format("{0:F1}", msg.AveOccupancy_F));
                        list2.Add(msg.AveSpeed_F.ToString("0.0"));
                        list2.Add(msg.Speed85p_F.ToString("0.0"));
                        list2.Add(msg.Headway.ToString("0.0"));
                        list2.Add(msg.Gap.ToString("0.0"));
                        BinData[] binsOfData = msg.BinsOfData;
                        if (binsOfData != null)
                        {
                            for (int j = 0; j < binsOfData.Length; j++)
                            {
                                if (binsOfData[j].Type == BinByType.length)
                                {
                                    int[] bins = binsOfData[j].Bins;
                                    for (int k = 0; k < 8; k++)
                                    {
                                        if (k < bins.Length)
                                        {
                                            list2.Add("" + bins[k]);
                                        }
                                        else
                                        {
                                            list2.Add("-");
                                        }
                                    }
                                }
                                if (binsOfData[j].Type == BinByType.speed)
                                {
                                    int[] numArray2 = binsOfData[j].Bins;
                                    for (int m = 0; m < 15; m++)
                                    {
                                        if (m < numArray2.Length)
                                        {
                                            list2.Add("" + numArray2[m]);
                                        }
                                        else
                                        {
                                            list2.Add("-");
                                        }
                                    }
                                }
                                if (binsOfData[j].Type == BinByType.direction)
                                {
                                    int[] numArray3 = binsOfData[j].Bins;
                                    for (int n = 0; n < 2; n++)
                                    {
                                        if (n < numArray3.Length)
                                        {
                                            list2.Add("" + numArray3[n]);
                                        }
                                        else
                                        {
                                            list2.Add("-");
                                        }
                                    }
                                }
                            }
                        }
                    }

                }

                string[] strArray = new string[list2.Count];
                for (int i = 0; i < strArray.Length; i++)
                {
                    strArray[i] = (string)list2[i];
                }
                list.Add(strArray);


                string format = "  {1,-8}  {2,6}    {3,5}   {4,5}   {5,5}   {6,2}   {7,2}   {8,2}   {9,2}   {10,2}   {11,2}   {12,2}   {13,2}  {14,7}  {15,7}   {16}  {17,4}  {18,4}  {19,4}  {20,4}  {21,4}  {22,4}  {23,4}  {24,4}  {25,4}   {26,4}   {27,4}   {28,4}   {29,4}   {30,4}   {31,4}     {32,4}     {33,4}";
                for (int i = 0; i < list.Count; i++)
                {
                    string[] astrArray = list[i];
                    string[] strArray2 = new string[strArray.Length];
                    for (int k = 0; k < strArray2.Length; k++)
                    {
                        strArray2[k] = strArray[k];
                    }
                    this._logBuffer.Add(string.Format(format, new object[] { 
            0, (strArray2.Length > 1) ? strArray2[1] : "-", (strArray2.Length > 2) ? strArray2[2] : "-", (strArray2.Length > 3) ? strArray2[3] : "-", (strArray2.Length > 4) ? strArray2[4] : "-", (strArray2.Length > 5) ? strArray2[5] : "-", (strArray2.Length > 8) ? strArray2[8] : "-", (strArray2.Length > 9) ? strArray2[9] : "-", (strArray2.Length > 10) ? strArray2[10] : "-", (strArray2.Length > 11) ? strArray2[11] : "-", (strArray2.Length > 12) ? strArray2[12] : "-", (strArray2.Length > 13) ? strArray2[13] : "-", (strArray2.Length > 14) ? strArray2[14] : "-", (strArray2.Length > 15) ? strArray2[15] : "-", (strArray2.Length > 6) ? strArray2[6] : "-", (strArray2.Length > 7) ? strArray2[7] : "-", 
            (strArray2.Length > 0) ? strArray2[0] : "-", (strArray2.Length > 0x11) ? strArray2[0x10] : "-", (strArray2.Length > 0x12) ? strArray2[0x11] : "-", (strArray2.Length > 0x13) ? strArray2[0x12] : "-", (strArray2.Length > 20) ? strArray2[0x13] : "-", (strArray2.Length > 0x15) ? strArray2[20] : "-", (strArray2.Length > 0x16) ? strArray2[0x15] : "-", (strArray2.Length > 0x17) ? strArray2[0x16] : "-", (strArray2.Length > 0x18) ? strArray2[0x17] : "-", (strArray2.Length > 0x19) ? strArray2[0x18] : "-", (strArray2.Length > 0x1a) ? strArray2[0x19] : "-", (strArray2.Length > 0x1b) ? strArray2[0x1a] : "-", (strArray2.Length > 0x1c) ? strArray2[0x1b] : "-", (strArray2.Length > 0x1d) ? strArray2[0x1c] : "-", (strArray2.Length > 30) ? strArray2[0x1d] : "-", (strArray2.Length > 0x1f) ? strArray2[30] : "-", 
            (strArray2.Length > 0x20) ? strArray2[0x1f] : "-", (strArray2.Length == 0x21) ? strArray2[0x20] : "-"
         }));
                }                
                this._logBuffer.Add("");

                //Testing

                //EndTesting

            }

            public void WriteToLog(string fileName)
            {
                System.Text.StringBuilder output = WriteHeaderToLog(fileName);

                foreach (List<IntervalDataMessage> msgss in this.dataDictionary.Values)
                {
                    LogInterval(msgss);
                }

                foreach (string s in this._logBuffer)
                {
                    output.AppendLine(s);
                }

                return;
            }

            #endregion

            #endregion
        }

        #endregion        
    }
}
