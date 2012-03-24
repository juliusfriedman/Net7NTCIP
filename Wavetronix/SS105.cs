using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Wavetronix
{
    namespace SS105
    {
        #region Classes

        /// <summary>
        /// A Managed Class which represents the LaneData of a SS105
        /// </summary>
        public class LaneData
        {
            public LaneData(byte[] rawData)
            {
                this.rawData = rawData;
            }
            /// <summary>
            /// The raw lane data from the response
            /// </summary>
            protected byte[] rawData;
            
            /// <summary>
            /// The position of the lane in the SmartSensor configuration.
            /// </summary>
            public int LaneID
            {
                get
                {
                    return int.Parse(Encoding.ASCII.GetString(rawData, 0, 1), System.Globalization.NumberStyles.HexNumber);
                }
            }
            
            /// <summary>
            /// The total number of vehicles detected in the lane, during the time interval.
            /// </summary>
            public int Volume
            {
                get
                {
                    return int.Parse(Encoding.ASCII.GetString(rawData, 1, 8), System.Globalization.NumberStyles.HexNumber);
                }
            }

            /// <summary>
            /// The average speed vehicles traveled in the lane, during the interval.
            /// </summary>
            public int AverageSpeed
            {
                get
                {
                    return int.Parse(Encoding.ASCII.GetString(rawData, 9, 4), System.Globalization.NumberStyles.HexNumber);
                }
            }

            /// <summary>
            /// The percentage of time lane was occupied during the interval
            /// </summary>
            public int Occupancy
            {
                get
                {
                    return int.Parse(Encoding.ASCII.GetString(rawData, 13, 4), System.Globalization.NumberStyles.HexNumber);
                }
            }

            /// <summary>
            /// The percentage of time lane was occupied during the interval as a float
            /// </summary>
            public float Occupancy_F
            {
                get
                {
                    float result = (Occupancy * 100) / 1024;
                    return result;
                }
            }

            /// <summary>
            /// The percentage of vehicles whose lengths were classified as small
            /// </summary>
            public int SmallClass
            {
                get
                {
                    return int.Parse(Encoding.ASCII.GetString(rawData, 17, 4), System.Globalization.NumberStyles.HexNumber);
                }
            }

            /// <summary>
            /// The percentage of vehicles whose lengths were classified as small as float
            /// </summary>
            public float SmallClass_F
            {
                get
                {
                    float result = (SmallClass * 100) / 1024;
                    return result;
                }
            }

            /// <summary>
            /// The percentage of vehicles whose lengths were classified as medium
            /// </summary>
            public int MediumClass
            {
                get
                {
                    return int.Parse(Encoding.ASCII.GetString(rawData, 21, 4), System.Globalization.NumberStyles.HexNumber);
                }
            }

            /// <summary>
            /// The percentage of vehicles whose lengths were classified as medium as float
            /// </summary>
            public float MediumClass_F
            {
                get
                {
                    float result = (MediumClass * 100) / 1024;
                    return result;
                }
            }

            /// <summary>
            /// The percentage of vehicles whose lengths were classified as large
            /// </summary>
            public int LargeClass
            {
                get
                {
                    return int.Parse(Encoding.ASCII.GetString(rawData, 25, 4), System.Globalization.NumberStyles.HexNumber);
                }
            }

            //The percentage of vehicles whose lengths were classified as large as float
            public float LargeClass_F
            {
                get
                {
                    float result = (LargeClass * 100) / 1024;
                    return result;
                }
            }
        }

        /// <summary>
        /// A Managed Class which represents the GeneralConfiguration of a SS105
        /// </summary>
        public class GeneralConfiguration
        {
            byte[] rawData;
            public GeneralConfiguration(byte[] rawData)
            {
                this.rawData = rawData;
            }

            public string SerialNumber
            {
                get
                {
                    return Encoding.ASCII.GetString(rawData, 0, 16);
                }
            }

            public string Location
            {
                get
                {
                    return Encoding.ASCII.GetString(rawData, 78, 32).Trim();
                }
                set
                {
                    if (value.Length > 32) throw new ArgumentException("Max length is 32 characters", "Location");
                    Encoding.ASCII.GetBytes(value).CopyTo(rawData, 78);
                }
            }

            public string Description
            {
                get
                {
                    return Encoding.ASCII.GetString(rawData, 110, 32).Trim();
                }
                set
                {
                    if (value.Length > 32) throw new ArgumentException("Max length is 32 characters", "Description");
                    Encoding.ASCII.GetBytes(value).CopyTo(rawData, 110);
                }
            }

            public string SensorID
            {
                get
                {
                    return Encoding.ASCII.GetString(rawData, 66, 4);
                }
            }

            public string RTMSID
            {
                get
                {
                    return Encoding.ASCII.GetString(rawData, 62, 4);
                }
            }

            public string Orentation
            {
                get { return Encoding.ASCII.GetString(rawData, 76, 2); }
                set
                {
                    if (value.Length > 2) throw new ArgumentException("Max length is 2 characters", "Orentation");
                    Encoding.ASCII.GetBytes(value).CopyTo(rawData, 76);
                }
            }
        }

        /// <summary>
        /// A Managed Class which represents the VersionInformation of a SS105
        /// </summary>
        public class VersionInformation
        {
            byte[] rawData;

            public VersionInformation(byte[] rawData)
            {
                this.rawData = rawData;
                string[] tokens = Encoding.ASCII.GetString(rawData).Split(':');
                DSPVersion = tokens[1].Replace(" FPGA",string.Empty).Trim();
                FPGAVersion = tokens[2].Trim();
            }
            public string DSPVersion
            {
                get;
                private set;

            }
            public string FPGAVersion
            {
                get;
                private set;
            }
        }
        
        public class EventData
        {
            byte[] rawData;

            public EventData(byte[] rawData)
            {
                this.rawData = rawData;
            }

            public DateTime TimeStamp
            {
                get
                {
                    int divisor = 2500; //2.5 ms
                    long value = BitConverter.ToInt64(rawData, 2);
                    DateTime result = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, DateTimeKind.Utc);
                    result.AddMilliseconds(value / divisor);
                    return result;
                }
            }

            public int LaneID
            {
                get
                {
                    return rawData[10];
                }
            }

            public int Duration
            {
                get
                {
                    return BitConverter.ToInt32(rawData, 11);
                }
            }

            public float Duration_F
            {
                get
                {
                    return ((float)(Duration / 2500));
                }
            }

            public int Speed
            {
                get
                {
                    return BitConverter.ToInt32(rawData, 15);
                }
            }

            public int ClassID
            {
                get
                {
                    return rawData[19];
                }
            }

        }

        public class PresenceData
        {
            public PresenceData(byte[] rawData)
            {
                this.rawData = rawData;
                this.binaryData = BitConverter.ToInt32(rawData, 0);
            }

            byte[] rawData;
            int binaryData;

            public bool PresenceInLane(int laneNumber)
            {
                return (binaryData << laneNumber) == 1;
            }
        }

        /// <summary>
        /// A Managed Class which represents the ClassificationInformation of a SS105
        /// </summary>
        public class ClassificationInformation
        {
            byte[] rawData;
            public ClassificationInformation(byte[] rawData)
            {
                this.rawData = rawData;
            }
            public int Class0_Min
            {
                get
                {
                    return int.Parse( Encoding.ASCII.GetString(rawData, 0, 4), System.Globalization.NumberStyles.HexNumber);
                }
                set
                {
                    if (value >= Class0_Max) return;
                    BitConverter.GetBytes(value).CopyTo(rawData, 0);
                }
            }
            public int Class0_Max
            {
                get
                {
                    return int.Parse(Encoding.ASCII.GetString(rawData, 4, 4), System.Globalization.NumberStyles.HexNumber);
                }
                set
                {
                    if (value >= Class1_Min) return;
                    BitConverter.GetBytes(value).CopyTo(rawData, 4);
                }
            }

            public int Class1_Min
            {
                get
                {
                    return int.Parse(Encoding.ASCII.GetString(rawData, 16, 4), System.Globalization.NumberStyles.HexNumber);
                }
                set
                {
                    if (value >= Class1_Max) return;
                    BitConverter.GetBytes(value).CopyTo(rawData, 16);
                }
            }
            public int Class1_Max
            {
                get
                {
                    return int.Parse(Encoding.ASCII.GetString(rawData, 20, 4), System.Globalization.NumberStyles.HexNumber);
                }
                set
                {
                    if (value >= Class2_Min) return;
                    BitConverter.GetBytes(value).CopyTo(rawData, 20);
                }
            }

            public int Class2_Min
            {
                get
                {
                    return int.Parse(Encoding.ASCII.GetString(rawData, 28, 4), System.Globalization.NumberStyles.HexNumber);
                }
                set
                {
                    if (value >= Class2_Max) return;
                    BitConverter.GetBytes(value).CopyTo(rawData, 28);
                }
            }
            public int Class2_Max
            {
                get
                {
                    return int.Parse(Encoding.ASCII.GetString(rawData, 32, 4), System.Globalization.NumberStyles.HexNumber);
                }
                set
                {
                    if (value <= Class2_Max) return;
                    BitConverter.GetBytes(value).CopyTo(rawData, 32);
                }
            }

            public byte[] ToBytes()
            {
                return rawData;
            }
        }

        /// <summary>
        /// Decribes the BaudRate on a Expansion Interface
        /// </summary>
        public enum BaudRate
        {
            Bps9600 = 0,
            Bps19200 = 1,
            Bps38400 = 2,
            Bps57600 = 3,
            Bps115200 = 4,
            Bps2230400 = 5,
            Bps460800 = 6,
            Bps921600 = 7
            //0x08 - 0x0F Reserved                
        }

        #endregion

        /// <summary>
        /// Defines a class which can communicate with a Wavetronix SS105 Sensor
        /// </summary>
        public class SS105Messenger
        {

            #region Constants

            /// <summary>
            /// The modem layer ocassionally returns the following bytes preceeding a response from a SS105.
            /// they need to be skipped to retireve the actualy payload
            /// </summary>
            static readonly byte[] ResponseSkip = new byte[] { 0x41, 0x54, 0x41, 0x7e, 0x0d, 0x0d };
            /// <summary>
            /// All MultiDrop Requests and Responses contain this header mark followed by the 4 byte ID of the device responding.
            /// </summary>
            static readonly byte[] MultiDropHeader = Encoding.ASCII.GetBytes("Z0");         
            /// <summary>
            /// The bytes to mark the end of a response from a SS105
            /// </summary>
            static readonly byte[] ResponseTerminator = Encoding.ASCII.GetBytes("~\r\r");
            /// <summary>
            /// The bytes to mark the end of a request to a SS105
            /// </summary>
            static readonly byte[] RequestTerminator = Encoding.ASCII.GetBytes("\r");
            /// <summary>
            /// The string present when a request results in a Invalid Response
            /// </summary>
            const string InvalidString = "Invalid";
            /// <summary>
            /// The string present when a request results in a Error Response
            /// </summary>
            const string ErrorString = "Error";
            /// <summary>
            /// The string present when a request results in a Failure Response
            /// </summary>
            const string FailureString = "Failure";
            /// <summary>
            /// The string present when a request results in a Empty Response
            /// </summary>
            const string EmptyString = "Empty";
            /// <summary>
            /// The string present when a request results in a No Lanes Response
            /// </summary>
            const string NoLanes = "No Lanes";
            /// <summary>
            /// The string present when a request results in a Success Response
            /// </summary>
            const string SuccessString = "Success";

            static readonly byte[] GetSensorBaudRateControl = Encoding.ASCII.GetBytes("SJS0000970004" /*+ RequestTerminator*/);//contains checksum in response (response is SJ and 4 bytes control and 4 bytes checksum + terminator)
            //static readonly byte[] SetSensorBaudRateControl = Encoding.ASCII.GetBytes("SK");//needs 4 bytes control appened 4 byte checksum and terminator 
            static readonly byte[] SetSensorBaudRateControl = Encoding.ASCII.GetBytes("SKS000097");//needs 4 bytes control appened 4 byte checksum and terminator             
            static readonly byte[] GetSensorTimeIntervalMessage = Encoding.ASCII.GetBytes("SJS00008E0008" /*+ RequestTerminator*/);//contains checksum in response
            static readonly byte[] SetSensorTimeIntervalMessage = Encoding.ASCII.GetBytes("SKS00008E0008" /*+ RequestTerminator*/);//contains checksum in request only needs 8 byte timestamp added to command + 4 byte checksum
            static readonly byte[] GetSystemTimeMessage = Encoding.ASCII.GetBytes("SB");//no checksum
            static readonly byte[] SetSystemTimeMessage = Encoding.ASCII.GetBytes("S4");//no checksum needs 8 byte timestamp added
            static readonly byte[] GetSensorLaneInformationMessage = Encoding.ASCII.GetBytes("S01");
            static readonly byte[] GeneralConfigurationMessage = Encoding.ASCII.GetBytes("S0");//no checksum
            static readonly byte[] ExpansionInterfaceMessage = Encoding.ASCII.GetBytes("SH");//no checksum
            static readonly byte[] SystemVersionMessage = Encoding.ASCII.GetBytes("S5");//no checksum
            static readonly byte[] SystemUnitsMessage = Encoding.ASCII.GetBytes("S6");//no checksum
            static readonly byte[] DataIntervalMessage = Encoding.ASCII.GetBytes("XD"/*+ RequestTerminator*/);//checksum in response only
            static readonly byte[] EventDataMessage = Encoding.ASCII.GetBytes("XA" /*+ RequestTerminator*/);//no checksum
            static readonly byte[] PresenceDataMessage = Encoding.ASCII.GetBytes("X1"/* + RequestTerminator*/);//no checksum
            static readonly byte[] GetLengthClassificationMessage = Encoding.ASCII.GetBytes("SJS0200000028" /*+ RequestTerminator*/);//response is 40 bytes + checksum and terminator
            static readonly byte[] SetLengthClassificationMessage = Encoding.ASCII.GetBytes("SK" /*+ RequestTerminator*/);//request needs 40 byte substring and checksum and terminator            

            static readonly DateTime baseTime = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 0, 0, 0, 0), DateTimeKind.Utc);

            static readonly int LaneDataBlockSize = 29;

            #endregion

            #region Fields

            IPEndPoint ipEndPoint;

            int sendRecieveTimeout = 10000;

            UdpClient udpClient;
            int udpBytesRecieved;
            int udpBytesSent;

            TcpClient tcpClient;
            NetworkStream netStream;
            int tcpBytesRecieved;
            int tcpBytesSent;

            ProtocolType transportProtocol;
            
            byte[] mutliDropAddress;

            GeneralConfiguration generalConfiguration = null;
            VersionInformation versionInformation = null;
            
            string interfaceInformation;
            string interfaceBaudRate;
            
            ClassificationInformation classificationInformation = null;

            List<LaneData> laneData;
            List<string> laneInformation;

            PresenceData presenceData;

            EventData eventData;

            int intervalDuration;
            DateTime sensorTime;

            List<byte[]> requestLog = new List<byte[]>();
            List<byte[]> recieveLog = new List<byte[]>();

            SortedDictionary<DateTime, List<LaneData>> dataDictionary = new SortedDictionary<DateTime, List<LaneData>>();

            char units;

            bool busy;

            #endregion

            #region Properties

            /// <summary>
            /// Specifies if the messenger is busy in a threaded operation
            /// </summary>
            public bool IsBusy
            {
                get { return busy; }
            }

            /// <summary>
            /// Specifies if the messenger is using MultiDrop addressing
            /// </summary>
            public bool IsMultiDrop
            {
                get { return mutliDropAddress != null; }
            }

            /// <summary>
            /// Specifies the MultiDropAddress to utilize to communicate with SS105's
            /// </summary>
            public byte[] MultiDropAddress
            {
                get
                {
                    return mutliDropAddress;
                }
                set
                {
                    mutliDropAddress = value;
                }
            }

            /// <summary>
            /// The Socket of the Messenger depending on the TransportProtocol of the Messenger
            /// </summary>
            public Socket Socket
            {
                get
                {
                    if (transportProtocol == ProtocolType.Tcp)
                    {
                        return tcpClient.Client;
                    }
                    else return udpClient.Client;
                }
            }

            /// <summary>
            /// The transportProtocol the Mesenger will use to communicate with a SS105
            /// </summary>
            public ProtocolType TransportProtocol
            {
                get
                {
                    return this.transportProtocol;
                }                
            }

            /// <summary>
            /// The most recent lane data available from the sensor
            /// </summary>
            public List<LaneData> LaneData
            {
                get
                {
                    return this.laneData;
                }
            }

            /// <summary>
            /// The information to describe the lane data
            /// </summary>
            public List<String> LaneInfo
            {
                get
                {
                    return laneInformation;
                }
            }

            /// <summary>
            /// The amount of lanes contained in the LaneData List
            /// </summary>
            public int LaneCount
            {
                get
                {
                    return LaneData.Count;
                }
            }

            /// <summary>
            /// The data dictionary containing data from the SS105
            /// </summary>
            public SortedDictionary<DateTime, List<LaneData>> DataDictionary
            {
                get { return this.dataDictionary; }
            }

            /// <summary>
            /// The General Configuration of the connected SS105
            /// </summary>
            public GeneralConfiguration GeneralConfiguration
            {
                get { return this.generalConfiguration; }
            }

            /// <summary>
            /// The VersionInformation of the connected SS105
            /// </summary>
            public VersionInformation VersionInformation
            {
                get
                {
                    return this.versionInformation;
                }
            }

            /// <summary>
            /// The ClassificationInformation of the connected SS105
            /// </summary>
            public ClassificationInformation ClassificationInformation
            {
                get { return this.classificationInformation; }
            }

            /// <summary>
            /// The baudRate of the Expansion B port
            /// </summary>
            public BaudRate BaudRateExpansionB
            {
                get
                {
                    return (BaudRate)int.Parse(this.interfaceBaudRate[0].ToString(), System.Globalization.NumberStyles.HexNumber);
                }
            }

            /// <summary>
            /// The baudRate of the RS232 Port
            /// </summary>
            public BaudRate BaudRateRS232
            {
                get
                {
                    return (BaudRate)int.Parse(this.interfaceBaudRate[1].ToString(), System.Globalization.NumberStyles.HexNumber);
                }
            }

            /// <summary>
            /// The baudRate of the Expansion A port
            /// </summary>
            public BaudRate BaudRateExpansionA
            {
                get
                {
                    return (BaudRate)int.Parse(this.interfaceBaudRate[2].ToString(), System.Globalization.NumberStyles.HexNumber);
                }
            }

            /// <summary>
            /// The baudRate of the RS285 port
            /// </summary>
            public BaudRate BaudRateRS485
            {
                get
                {
                    return (BaudRate)int.Parse(this.interfaceBaudRate[3].ToString(), System.Globalization.NumberStyles.HexNumber);
                }
            }

            /// <summary>
            /// The amount of bytes recieved over UDP with this messenger
            /// </summary>
            public int UdpRecievedBytes
            {
                get { return this.udpBytesRecieved; }
            }

            /// <summary>
            /// The amount of bytes recieved over TCP with this messenger
            /// </summary>
            public int TcpRecievedBytes
            {
                get { return this.tcpBytesRecieved; }
            }

            /// <summary>
            /// The amount of bytes sent over UDP with this messenger
            /// </summary>
            public int UdpSentBytes
            {
                get { return this.udpBytesSent; }
            }

            /// <summary>
            /// The amount of bytes sent over TCP with this messenger
            /// </summary>
            public int TcpSentBytes
            {
                get { return this.tcpBytesSent; }
            }

            /// <summary>
            /// The total amount of bytes sent with this messenger
            /// </summary>
            public int TotalSentBytes
            {
                get
                {
                    return this.tcpBytesSent + this.udpBytesSent;
                }
            }

            /// <summary>
            /// The total amount of bytes recieved with this messenger
            /// </summary>
            public int TotalRecievedBytes
            {
                get
                {
                    return this.udpBytesRecieved + this.tcpBytesRecieved;
                }
            }

            /// <summary>
            /// The time this messenge will wait when sending or recieving
            /// </summary>
            public int SendRecieveTimeout
            {
                get { return this.sendRecieveTimeout; }
                set { this.sendRecieveTimeout = value; }
            }

            /// <summary>
            /// Indicates weather or not the values of this sensor appear as metric or english
            /// </summary>
            public bool IsMetric
            {
                get { return this.units == 'M'; }
            }

            #endregion

            #region Constructor

            public SS105Messenger(IPEndPoint ipEndPoint, ProtocolType transportProtocol)
            {
                this.ipEndPoint = ipEndPoint;
                this.transportProtocol = transportProtocol;
                Initialize();
            }

            public SS105Messenger(TcpClient existingClient)
            {
                this.transportProtocol = ProtocolType.Tcp;
                this.tcpClient = existingClient;
                Initialize();
            }

            public SS105Messenger(UdpClient existingClient)
            {
                this.transportProtocol = ProtocolType.Udp;
                this.udpClient = existingClient;
                Initialize();
            }

            #endregion

            #region Destructor

            ~SS105Messenger()
            {
                Close();
            }

            #endregion

            #region Public Methods

            /// <summary>
            /// Closes all sockets and clears all logs
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
            /// Clears the request log
            /// </summary>
            public void ClearRequestLog()
            {
                requestLog.Clear();
            }

            /// <summary>
            /// Clears the recieve log
            /// </summary>
            public void ClearRecieveLog()
            {
                recieveLog.Clear();
            }

            /// <summary>
            /// Resets the request and recieve log and send and recieved byte counters
            /// </summary>
            public void ResetSession()
            {
                udpBytesRecieved = udpBytesSent = tcpBytesRecieved = tcpBytesSent = 0;
                ClearRequestLog();
                ClearRecieveLog();
            }

            /// <summary>
            /// Updates the LaneInformation from the most recent index available on the sensor
            /// </summary>
            public void UpdateLaneInformation()
            {
                byte[] response = SendMessage(GetSensorDataIntervalMessage());
                List<LaneData> results = ParseSensorTimeIntervalMessage(response);
                this.laneData = results;
            }

            /// <summary>
            /// Retrieves the specified index from the memory of the sensor
            /// </summary>
            /// <param name="index">The index to retirve</param>
            public void RetrieveLaneInformationByIndex(int index)
            {
                byte[] response = SendMessage(GetSensorDataIntervalMessage(index));
                List<LaneData> results = ParseSensorTimeIntervalMessage(response);
                this.laneData = results;
            }

            /// <summary>
            /// Updates the DataInterval on the sensor
            /// </summary>
            /// <param name="newDurationSeconds">The new duration in seconds</param>
            public void UpdateSensorTimeInterval(int newDurationSeconds)
            {
                if (IsValidResponse(SendMessage(SetTimeIntervalMessage(newDurationSeconds))))
                {
                    FetchTimeIntervalInformation();
                }
            }

            /// <summary>
            /// Updates the Sensors Clock with the Given DateTime
            /// </summary>
            /// <param name="newTime">The time to set the sensors clock to</param>
            public void UpdateSensorTime(DateTime newTime)
            {
                if (IsValidResponse(SendMessage(SetSensorTimeMessage(newTime))))
                {
                    FetchSensorTime();
                }
            }

            /// <summary>
            /// Synchronizes the Sensor Clock with this PC's Time
            /// </summary>
            public void SynchSensorTime()
            {
                UpdateSensorTime(DateTime.Now);
            }

            /// <summary>
            /// Updates the Sensors Classifications based on the Messengers Settings for the Classification Information
            /// </summary>
            public void UpdateClassificationInformation()
            {
                if (IsValidResponse(SendMessage(SetLengthClassificationInformationMessage(this.classificationInformation))))
                {
                    FetchLengthClassificationInformation();
                }
            }

            /// <summary>
            /// Updates the Sensors Classification based on the given ClassificationInformation
            /// </summary>
            /// <param name="classificationInformation">The Classificiation information to set on the sensor</param>
            public void UpdateClassificationInformation(ClassificationInformation classificationInformation)
            {
                if (IsValidResponse(SendMessage(SetLengthClassificationInformationMessage(classificationInformation))))
                {
                    FetchLengthClassificationInformation();
                }
            }

            /// <summary>
            /// Updates the Sensors baud rate on the expansion ports
            /// Each character of the substring represents the baud rate of one of the ports.
            /// 1'st Character is ExpansionB
            /// 2'nd Character is RS-232
            /// 3'rd Character is ExpansionA
            /// 4'th Character is RS-485
            /// Valid Values are 0 - 7 and correspond to the BaudRate Enum
            /// </summary>
            /// <param name="baudRateString">The BaudRateString to Update the Sensor with</param>
            public void UpdateSensorBaudRate(string baudRateString)
            {
                if (IsValidResponse(SendMessage(SetBaudRateMessage(baudRateString))))
                {
                    FetchSensorBaudRate();
                }
            }

            /// <summary>
            /// Downloads data from a SS105 given a start and end index
            /// </summary>
            /// <param name="indexStart">The index to start downloading at</param>
            /// <param name="indexEnd">The index to stop downloading at</param>
            public void DownloadData(int indexStart, int indexEnd)
            {
                busy = true;
                for (int index = indexStart, end = indexEnd; index >= end; index--)
                {
                    RetrieveLaneInformationByIndex(index);
                }
                busy = false;
            }

            /// <summary>
            /// Downloads all data from a SS105 Sensor
            /// </summary>
            public void DownloadAllData()
            {
                busy = true;
                for (int index = 2480, end = -1; index > end; index--)
                {
                    RetrieveLaneInformationByIndex(index);
                }
                busy = false;
            }

            /// <summary>
            /// Downloads the first 246 index's from a SS105
            /// </summary>
            public void DownloadVolatileData()
            {
                busy = true;
                for (int index = 246, end = -1; index > end; index--)
                {
                    RetrieveLaneInformationByIndex(index);
                }
                busy = false;
            }

            /// <summary>
            /// Downloads the 2234 index's available in the flash memory of the SS105
            /// </summary>
            public void DownloadFlashData()
            {
                busy = true;
                for (int index = 2480, end = 246; index > end; index--)
                {
                    RetrieveLaneInformationByIndex(index);
                }
                busy = false;
            }

            #endregion

            #region Private Methods

            /// <summary>
            /// Initializes the messenger
            /// </summary>
            private void Initialize()
            {
                if (transportProtocol == ProtocolType.Tcp)
                {
                    if (tcpClient == null)
                    {
                        tcpClient = new TcpClient();
                        tcpClient.Connect(ipEndPoint);
                    }
                    netStream = tcpClient.GetStream();
                }
                else
                {
                    if (udpClient == null)
                    {
                        udpClient = new UdpClient();
                        udpClient.Connect(ipEndPoint);
                    }
                }
                FetchGeneralConfiguration();
                FetchVersionInformation();
                FetchSensorTime();
                FetchLaneInformation();
                FetchTimeIntervalInformation();
                FetchExpansionInformation();
                FetchSensorBaudRate();
                FetchLengthClassificationInformation();
                UpdateLaneInformation();                
            }

            #region Fetch Methods

            private void FetchVersionInformation()
            {
                this.versionInformation = ParseVersionInformationMessage(SendMessage(GetVersionInformationMessage()));
            }

            private void FetchGeneralConfiguration()
            {
                this.generalConfiguration =  ParseGeneralConfigurationMessage(SendMessage(GetGeneralConfigurationMessage()));
            }

            private void FetchTimeIntervalInformation()
            {
                this.intervalDuration = ParseTimeIntervalMessage(SendMessage(GetTimeIntervalMessage()));
            }

            private void FetchSensorTime()
            {                
                this.sensorTime = ParseSensorTimeMessage(SendMessage(GetSensorTimeMessage()));
            }

            private void FetchExpansionInformation()
            {
                this.interfaceInformation = ParseExpansionInterfaceMessage(SendMessage(GetExpansionInterfaceMessage()));
            }

            private void FetchSensorBaudRate()
            {
                this.interfaceBaudRate = ParseBaudRateMessage(SendMessage(GetBaudRateMessage()));
            }

            private void FetchLengthClassificationInformation()
            {
                this.classificationInformation = ParseLengthClassificationInformationMessage(SendMessage(GetLengthClassificationInformationMessage()));
            }

            private void FetchLaneInformation()
            {
                this.laneInformation = ParseLaneInformationMessage(SendMessage(GetLaneInformationMessage()));
            }

            #endregion

            #region Protocol Helpers

            /// <summary>
            /// Generates the required checksum for the SS105 data protocol
            /// </summary>
            /// <param name="data"></param>
            /// <returns></returns>
            private byte[] GenerateChecksum(List<byte> data)
            {
                int crcvalue = 0;
                foreach (char c in Encoding.ASCII.GetString(data.GetRange(2, data.Count - 2).ToArray())) crcvalue += c;
                return BitConverter.GetBytes(crcvalue);
            }

            /// <summary>
            /// Validates a byte array to determine if a checksum is valid
            /// </summary>
            /// <param name="data">The data to validate</param>
            /// <param name="checkSum">The checksum found for the data</param>
            /// <returns>true if data's checksum is equal to the given checksum</returns>
            private bool ValidChecksum(byte[] data, int checkSum)
            {
                int crcvalue = 0;
                foreach (char c in Encoding.ASCII.GetString(data)) crcvalue += c;
                return crcvalue == checkSum;
            }

            /// <summary>
            /// Determines weather or not the response is valid from a SS105
            /// </summary>
            /// <param name="response">The binary data from the response</param>
            /// <returns>True if the data is valid otherwise false</returns>
            private bool IsValidResponse(byte[] response)
            {
                string check = Encoding.ASCII.GetString(response);
                if(check.Contains(SuccessString)) return true;
                bool result = check.Contains(ErrorString) || check.Contains(InvalidString) || check.Contains(FailureString) || check.Contains(EmptyString);
                return !result;
            }

            #endregion

            #region Lane Information

            private byte[] GetLaneInformationMessage()
            {
                List<byte> buffer = new List<byte>();

                if (IsMultiDrop)
                {
                    buffer.AddRange(MultiDropHeader);
                    buffer.AddRange(mutliDropAddress);
                }

                buffer.AddRange(GetSensorLaneInformationMessage);
                buffer.AddRange(RequestTerminator);

                return buffer.ToArray();
            }

            private List<string> ParseLaneInformationMessage(byte[] response)
            {

                List<string> results = new List<string>();

                if (!IsValidResponse(response)) return results;


                List<byte> buffer = new List<byte>(response);
                //if the buffer contains the ATA modem header remove it
                if (buffer[0] == ResponseSkip[0])
                {
                    buffer.RemoveRange(0, ResponseSkip.Length);
                }

                //Check for a valid header for a GetSensorLaneInformationMessage
                if (buffer[0] == GetSensorLaneInformationMessage[0] && buffer[1] == GetSensorLaneInformationMessage[1])
                {
                    //Remove the command header
                    buffer.RemoveRange(0, 3);
                }
                else return results;

                //If the buffer contains the ReponseTrailer then remove it
                if (buffer[buffer.Count - 3] == ResponseTerminator[0])
                {
                    buffer.RemoveRange(buffer.Count - 3, 3);
                }

                //Keep the Checksum
                int checkSum = int.Parse(Encoding.ASCII.GetString(buffer.GetRange(buffer.Count - 4, 4).ToArray()), System.Globalization.NumberStyles.HexNumber);

                //remove it from the response
                buffer.RemoveRange(buffer.Count - 4, 4);

                //If the cheksum is valid continue
                if (ValidChecksum(buffer.ToArray(), checkSum))
                {

                    //Get the units identifier
                    this.units = (char)buffer[0];
                    buffer.RemoveAt(0);
                    
                    //parse the lane info
                    for (int pos = 0, end = 8; pos < end; pos++)
                    {
                        byte[] raw = buffer.GetRange(0, 8).ToArray();
                        buffer.RemoveRange(0, 8);
                        results.Add(Encoding.ASCII.GetString(raw));
                    }

                    return results;

                }else return results;
                
            }

            #endregion

            #region Expansion Interface

            private byte[] GetExpansionInterfaceMessage()
            {
                List<byte> buffer = new List<byte>();

                if (IsMultiDrop)
                {
                    buffer.AddRange(MultiDropHeader);
                    buffer.AddRange(mutliDropAddress);
                }

                buffer.AddRange(ExpansionInterfaceMessage);
                buffer.AddRange(RequestTerminator);

                return buffer.ToArray();
            }

            private string ParseExpansionInterfaceMessage(byte[] response)
            {
                if (!IsValidResponse(response)) return null;

                List<byte> buffer = new List<byte>(response);

                //if the buffer contains the ATA modem header remove it
                if (buffer[0] == ResponseSkip[0])
                {
                    buffer.RemoveRange(0, ResponseSkip.Length);
                }

                //Check for a valid header for a TimeInterval
                if (buffer[0] == ExpansionInterfaceMessage[0] && buffer[1] == ExpansionInterfaceMessage[1])
                {
                    buffer.RemoveRange(0, 2);
                }

                //If the buffer contains the ReponseTrailer then remove it
                if (buffer[buffer.Count - 3] == ResponseTerminator[0])
                {
                    buffer.RemoveRange(buffer.Count - 3, 3);
                }

                return Encoding.ASCII.GetString(buffer.ToArray());
            }

            #endregion

            #region Time Interval

            private byte[] GetTimeIntervalMessage()
            {
                List<byte> buffer = new List<byte>();

                if (IsMultiDrop)
                {
                    buffer.AddRange(MultiDropHeader);
                    buffer.AddRange(mutliDropAddress);
                }

                buffer.AddRange(GetSensorTimeIntervalMessage);
                buffer.AddRange(RequestTerminator);

                return buffer.ToArray();
            }

            private byte[] SetTimeIntervalMessage(int newDurationSeconds)
            {
                List<byte> buffer = new List<byte>();

                if (IsMultiDrop)
                {
                    buffer.AddRange(MultiDropHeader);
                    buffer.AddRange(mutliDropAddress);
                }

                buffer.AddRange(SetSensorTimeIntervalMessage);                               

                buffer.AddRange(BitConverter.GetBytes(newDurationSeconds));

                buffer.AddRange(GenerateChecksum(buffer));
                
                buffer.AddRange(RequestTerminator);

                return buffer.ToArray();
            }

            private int ParseTimeIntervalMessage(byte[] response)
            {
                if (!IsValidResponse(response)) return -1;

                List<byte> buffer = new List<byte>(response);

                //if the buffer contains the ATA modem header remove it
                if (buffer[0] == ResponseSkip[0])
                {
                    buffer.RemoveRange(0, ResponseSkip.Length);
                }

                //Check for a valid header for a TimeInterval
                if (buffer[0] == GetSensorTimeIntervalMessage[0] && buffer[1] == GetSensorTimeIntervalMessage[1])
                {
                    buffer.RemoveRange(0, 2);
                }

                //If the buffer contains the ReponseTrailer then remove it
                if (buffer[buffer.Count - 3] == ResponseTerminator[0])
                {
                    buffer.RemoveRange(buffer.Count - 3, 3);
                }
                
                //Keep the Checksum
                int checkSum = int.Parse(Encoding.ASCII.GetString(buffer.GetRange(buffer.Count - 4, 4).ToArray()), System.Globalization.NumberStyles.HexNumber);
                
                //remove it from the response
                buffer.RemoveRange(buffer.Count - 4, 4);

                //If the cheksum is valid continue
                if (ValidChecksum(buffer.ToArray(), checkSum))
                {
                    int dataIntervalTime = int.Parse(Encoding.ASCII.GetString(buffer.ToArray()), System.Globalization.NumberStyles.HexNumber);

                    return dataIntervalTime;
                }
                else return -1;               
            }

            #endregion

            #region General Configuration

            private byte[] GetGeneralConfigurationMessage()
            {
                List<byte> buffer = new List<byte>();

                if (IsMultiDrop)
                {
                    buffer.AddRange(MultiDropHeader);
                    buffer.AddRange(mutliDropAddress);
                }

                buffer.AddRange(GeneralConfigurationMessage);
                buffer.AddRange(RequestTerminator);

                return buffer.ToArray();
            }

            private GeneralConfiguration ParseGeneralConfigurationMessage(byte[] response)
            {
                if (!IsValidResponse(response)) return null;

                List<byte> buffer = new List<byte>(response);

                //if the buffer contains the ATA modem header remove it
                if (buffer[0] == ResponseSkip[0])
                {
                    buffer.RemoveRange(0, ResponseSkip.Length);
                }

                //Could check for multiDrop headers here


                //Check for a valid header for a GeneralConfigurationMessage
                if (buffer[0] == GeneralConfigurationMessage[0] && buffer[1] == GeneralConfigurationMessage[1])
                {
                    buffer.RemoveRange(0, 2);
                }

                //If the buffer contains the ReponseTrailer then remove it
                if (buffer[buffer.Count - 3] == ResponseTerminator[0])
                {
                    buffer.RemoveRange(buffer.Count - 3, 3);
                }                

                //Keep the Checksum
                int checkSum = int.Parse(Encoding.ASCII.GetString(buffer.GetRange(buffer.Count - 4, 4).ToArray()), System.Globalization.NumberStyles.HexNumber);
                
                //remove it from the response
                buffer.RemoveRange(buffer.Count - 4, 4);

                return new GeneralConfiguration(buffer.ToArray());

                /*
                 * Needs to be implemented... for some reason checksum is off by 'A' - 65 
                if (ValidChecksum(buffer.ToArray(), checkSum))
                {
                    
                }
                else return null;
                */
                
            }

            #endregion

            #region Version Information

            private byte[] GetVersionInformationMessage()
            {
                List<byte> buffer = new List<byte>();

                if (IsMultiDrop)
                {
                    buffer.AddRange(MultiDropHeader);
                    buffer.AddRange(mutliDropAddress);
                }

                buffer.AddRange(SystemVersionMessage);
                buffer.AddRange(RequestTerminator);

                return buffer.ToArray();
            }

            private VersionInformation ParseVersionInformationMessage(byte[] response)
            {
                if (!IsValidResponse(response)) return null;

                List<byte> buffer = new List<byte>(response);

                //if the buffer contains the ATA modem header remove it
                if (buffer[0] == ResponseSkip[0])
                {
                    buffer.RemoveRange(0, ResponseSkip.Length);
                }

                //Could check for multiDrop headers here


                //Check for a valid header for a TimeInterval
                if (buffer[0] == GeneralConfigurationMessage[0] && buffer[1] == GeneralConfigurationMessage[1])
                {
                    buffer.RemoveRange(0, 2);
                }

                //If the buffer contains the ReponseTrailer then remove it
                if (buffer[buffer.Count - 3] == ResponseTerminator[0])
                {
                    buffer.RemoveRange(buffer.Count - 3, 3);
                }

                return new VersionInformation(buffer.ToArray());
            }

            #endregion

            #region Length Classification Information

            private byte[] SetLengthClassificationInformationMessage(ClassificationInformation classificationInformation)
            {
                List<byte> buffer = new List<byte>();
                
                if (IsMultiDrop)
                {
                    buffer.AddRange(MultiDropHeader);
                    buffer.AddRange(mutliDropAddress);
                }

                buffer.AddRange(SetLengthClassificationMessage);
                buffer.AddRange(classificationInformation.ToBytes());

                return buffer.ToArray();
            }

            private byte[] GetLengthClassificationInformationMessage()
            {
                List<byte> buffer = new List<byte>();

                if (IsMultiDrop)
                {
                    buffer.AddRange(MultiDropHeader);
                    buffer.AddRange(mutliDropAddress);
                }

                buffer.AddRange(GetLengthClassificationMessage);
                buffer.AddRange(RequestTerminator);

                return buffer.ToArray();
            }

            private ClassificationInformation ParseLengthClassificationInformationMessage(byte[] response)
            {
                if (!IsValidResponse(response)) return null;

                List<byte> buffer = new List<byte>(response);

                //if the buffer contains the ATA modem header remove it
                if (buffer[0] == ResponseSkip[0])
                {
                    buffer.RemoveRange(0, ResponseSkip.Length);
                }

                //Could check for multiDrop headers here


                //Check for a valid header for a TimeInterval
                if (buffer[0] == GetLengthClassificationMessage[0] && buffer[1] == GetLengthClassificationMessage[1])
                {
                    buffer.RemoveRange(0, 2);
                }

                //If the buffer contains the ReponseTrailer then remove it
                if (buffer[buffer.Count - 3] == ResponseTerminator[0])
                {
                    buffer.RemoveRange(buffer.Count - 3, 3);
                }

                //Keep the Checksum
                int checkSum = int.Parse(Encoding.ASCII.GetString(buffer.GetRange(buffer.Count - 4, 4).ToArray()), System.Globalization.NumberStyles.HexNumber);
                
                //remove it from the response
                buffer.RemoveRange(buffer.Count - 4, 4);

                //If the cheksum is valid continue
                if (ValidChecksum(buffer.ToArray(), checkSum))
                {
                    return new ClassificationInformation(buffer.ToArray());
                }
                else return null;                
            }

            #endregion

            #region Sensor Time

            private byte[] GetSensorTimeMessage()
            {
                List<byte> buffer = new List<byte>();
                
                if (IsMultiDrop)
                {
                    buffer.AddRange(MultiDropHeader);
                    buffer.AddRange(mutliDropAddress);
                }

                buffer.AddRange(GetSystemTimeMessage);
                buffer.AddRange(RequestTerminator);

                return buffer.ToArray();
            }

            private byte[] SetSensorTimeMessage(DateTime newDate)
            {
                List<byte> buffer = new List<byte>();

                if (IsMultiDrop)
                {
                    buffer.AddRange(MultiDropHeader);
                    buffer.AddRange(mutliDropAddress);
                }

                buffer.AddRange(SetSystemTimeMessage);

                TimeSpan seconds = newDate - baseTime;
                
                buffer.AddRange(Encoding.ASCII.GetBytes(((int)seconds.TotalSeconds).ToString("X")));

                buffer.AddRange(GenerateChecksum(buffer));

                buffer.AddRange(RequestTerminator);

                return buffer.ToArray();
            }

            private DateTime ParseSensorTimeMessage(byte[] response)
            {
                if (!IsValidResponse(response)) return DateTime.MinValue;

                DateTime result = baseTime;

                List<byte> buffer = new List<byte>(response);

                //if the buffer contains the ATA modem header remove it
                if (buffer[0] == ResponseSkip[0])
                {
                    buffer.RemoveRange(0, ResponseSkip.Length);
                }

                //Check for a valid header for a TimeInterval
                if (buffer[0] == GetSystemTimeMessage[0] && buffer[1] == GetSystemTimeMessage[1])
                {
                    buffer.RemoveRange(0, 2);
                }

                //If the buffer contains the ReponseTrailer then remove it
                if (buffer[buffer.Count - 3] == ResponseTerminator[0])
                {
                    buffer.RemoveRange(buffer.Count - 3, 3);
                }



                int secondsToadd = int.Parse(Encoding.ASCII.GetString(buffer.ToArray()), System.Globalization.NumberStyles.HexNumber);

                return result.AddSeconds(secondsToadd);

            }

            #endregion

            #region Interval Data

            private byte[] GetSensorDataIntervalMessage()
            {
                return GetSensorDataIntervalMessage(0);
            }

            private byte[] GetSensorDataIntervalMessage(int index)
            {
                List<byte> buffer = new List<byte>();

                if (IsMultiDrop)
                {
                    buffer.AddRange(MultiDropHeader);
                    buffer.AddRange(mutliDropAddress);
                }

                buffer.AddRange(DataIntervalMessage);

                if (index >= 1)
                {                    
                    buffer.AddRange(Encoding.ASCII.GetBytes(index.ToString("X4")));
                }

                buffer.AddRange(RequestTerminator);

                return buffer.ToArray();
            }

            private List<LaneData> ParseSensorTimeIntervalMessage(byte[] response)
            {

                List<LaneData> results = new List<LaneData>();

                if(!IsValidResponse(response)) return results;
                                

                List<byte> buffer = new List<byte>(response);
                //if the buffer contains the ATA modem header remove it
                if (buffer[0] == ResponseSkip[0])
                {
                    buffer.RemoveRange(0, ResponseSkip.Length);
                }

                //Check for a valid header for a TimeInterval
                if (buffer[0] == DataIntervalMessage[0] && buffer[1] == DataIntervalMessage[1])
                {
                    //Remove the command header
                    buffer.RemoveRange(0, 2);

                    //If the buffer contains the ReponseTrailer then remove it
                    if (buffer[buffer.Count - 3] == ResponseTerminator[0])
                    {
                        buffer.RemoveRange(buffer.Count - 3, 3);
                    }

                    //Keep the Checksum
                    int checkSum = int.Parse(Encoding.ASCII.GetString(buffer.GetRange(buffer.Count - 4, 4).ToArray()), System.Globalization.NumberStyles.HexNumber);
                    
                    //remove it from the response
                    buffer.RemoveRange(buffer.Count - 4, 4);

                    //If the cheksum is valid continue
                    if (ValidChecksum(buffer.ToArray(), checkSum))
                    {
                        //Determine the key 
                        DateTime key = baseTime;

                        int secondsToadd = int.Parse(Encoding.ASCII.GetString(buffer.GetRange(0, 8).ToArray()), System.Globalization.NumberStyles.HexNumber);                        

                        key = key.AddSeconds(secondsToadd);

                        buffer.RemoveRange(0, 8);

                        //Determine the amount of lanes being returned

                        int laneCount = buffer.Count / LaneDataBlockSize;

                        //parse the lanes
                        for (int pos = 0, end = laneCount; pos < end; pos++)
                        {
                            byte[] raw = buffer.GetRange(0, LaneDataBlockSize).ToArray();
                            buffer.RemoveRange(0, LaneDataBlockSize);                            
                            results.Add(new LaneData(raw));
                        }
                        
                        //try to add the data to the dictionary and return
                        try
                        {                            
                            dataDictionary.Add(key, results);
                            return dataDictionary[key];
                        }
                        catch
                        {
                            return dataDictionary[key];
                        }                        

                    }
                    else return results;
                }
                else return results;
            }

            #endregion

            #region Baud Rate

            private byte[] GetBaudRateMessage()
            {
                List<byte> buffer = new List<byte>();

                if (IsMultiDrop)
                {
                    buffer.AddRange(MultiDropHeader);
                    buffer.AddRange(mutliDropAddress);
                }

                buffer.AddRange(GetSensorBaudRateControl);
                buffer.AddRange(RequestTerminator);

                return buffer.ToArray();
            }

            private byte[] SetBaudRateMessage(string sensorBaudString)
            {
                List<byte> buffer = new List<byte>();

                if (IsMultiDrop)
                {
                    buffer.AddRange(MultiDropHeader);
                    buffer.AddRange(mutliDropAddress);
                }

                buffer.AddRange(SetSensorBaudRateControl);

                buffer.AddRange(BitConverter.GetBytes(int.Parse(sensorBaudString, System.Globalization.NumberStyles.HexNumber)));

                buffer.AddRange(GenerateChecksum(buffer));

                buffer.AddRange(RequestTerminator);

                return buffer.ToArray();
            }

            private string ParseBaudRateMessage(byte[] response)
            {
                if (!IsValidResponse(response)) return "0000";

                List<byte> buffer = new List<byte>(response);

                //if the buffer contains the ATA modem header remove it
                if (buffer[0] == ResponseSkip[0])
                {
                    buffer.RemoveRange(0, ResponseSkip.Length);
                }

                //Check for a valid header for a TimeInterval
                if (buffer[0] == GetSensorBaudRateControl[0] && buffer[1] == GetSensorBaudRateControl[1])
                {
                    buffer.RemoveRange(0, 2);
                }

                //If the buffer contains the ReponseTrailer then remove it
                if (buffer[buffer.Count - 3] == ResponseTerminator[0])
                {
                    buffer.RemoveRange(buffer.Count - 3, 3);
                }
                
                //Keep the Checksum
                int checkSum = int.Parse(Encoding.ASCII.GetString(buffer.GetRange(buffer.Count - 4, 4).ToArray()), System.Globalization.NumberStyles.HexNumber);
                
                //remove it from the response
                buffer.RemoveRange(buffer.Count - 4, 4);

                //If the cheksum is valid continue
                if (ValidChecksum(buffer.ToArray(), checkSum))
                {
                    return Encoding.ASCII.GetString(buffer.ToArray());
                }
                else return "0000";
            }

            #endregion

            #region Transport

            private byte[] SendMessage(byte[] msg)
            {
                try
                {
                    if (!Socket.Connected)
                    {
                        Socket.Connect(this.ipEndPoint);
                    }

                    Socket.SendTimeout = sendRecieveTimeout;
                    Socket.ReceiveTimeout = sendRecieveTimeout;

                    requestLog.Add(msg);

                    if (transportProtocol == ProtocolType.Tcp)
                    {
                        netStream.Write(msg, 0, msg.Length);
                        tcpBytesSent += msg.Length;

                        byte[] inbuff = new byte[1024];
                        EndPoint ep = this.ipEndPoint;

                        int recv = Socket.ReceiveFrom(inbuff, ref ep);

                        tcpBytesRecieved += recv;

                        if (recv == ResponseSkip.Length)
                        {
                            recv = Socket.ReceiveFrom(inbuff, ref ep);

                            tcpBytesRecieved += recv;
                        }

                        byte[] indata = new byte[recv];
                        Array.Copy(inbuff, 0, indata, 0, recv);
                        inbuff = null;

                        recieveLog.Add(indata);

                        if (indata[0] == MultiDropHeader[0] && indata[1] == MultiDropHeader[1])
                        {
                            byte[] realdata = new byte[6 - indata.Length];
                            Array.Copy(indata, 6, realdata, 0, realdata.Length);
                            return realdata;
                        }

                        return indata;

                    }
                    else
                    {
                        udpBytesSent += udpClient.Send(msg, msg.Length);
                        byte[] buff = udpClient.Receive(ref ipEndPoint);
                        udpBytesRecieved += buff.Length;
                        recieveLog.Add(buff);

                        if (buff[0] == MultiDropHeader[0] && buff[1] == MultiDropHeader[1])
                        {
                            byte[] realdata = new byte[6 - buff.Length];
                            Array.Copy(buff, 6, realdata, 0, realdata.Length);
                            return realdata;
                        }

                        return buff;
                    }
                }
                catch
                {
                    throw;
                }                
            }

            #endregion

            #endregion

        }

    }
}
