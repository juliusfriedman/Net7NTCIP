using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Vaisala
{
    public class Messenger
    {
        public enum CommandMode
        {
            Local,
            Network
        }

        #region Statics

        static string OpenCmd = "OPEN";        
        static string OpenAll = "OPEN DSC *";
        static string CloseAll = "CLOSE";
        static string Message = "MES";
        static string Baud232 = "BAUD 232";
        static string Baud485 = "BAUD 485";
        static string Conf = "CONF";
        /*
         * INIT
        The INIT command restores the parameters to their default values.
        INIT {group} <ENTER>
        Options for the group parameter are the following:
        Parameter Option Description
        ALL Restores all configuration parameters displayed by
        the CONF command, as well as dry signals and
        the ID string, and clears the history memory
        CONF Restores all configuration parameters displayed by
        the CONF command
        HIST Clears the history memory of data messages
         */
        static string Init = "INIT";//ALL, CONF, OR HIST
        static string Par = "PAR";
        static string Id = "ID";
        static string Name = "NAME";
        static string Ver = "VER";
        /*
         * AMES
        The AMES command defines the message number and time interval
        for an automatically sent message.
        AMES {n} {t} <ENTER>
        where
        n = The message number, for example 16
        t = Time interval in minutes. If the value is set to "0", the message
        is sent as soon as possible, approximately every 30 seconds.
        The default setting is the following:
        AMES -1 0
        which disables the sending of automatic messages.
         */
        static string Ames = "AMES";
        static string Hist = "HIST";
        static string Clean = "CLEAN";
        static string Dry = "DRY";
        static string Moist = "MOIST";
        static string Status = "STATUS";
        static string Date = "DATE";//yyyy mm dd<enter>
        static string Help = "HELP";
        static string Link = "LINK";
        static string CloseLink = "<ESC><ESC><ESC>"; // 3x esc char
        static string ResetCmd = "RESET";
        static string Time = "TIME";//hh mm[ss]<enter>
        /*
        Table 9 STA Command Parameters
        Parameter Explanation Limits and Comments
        Grip Level of grip 0.00 (slippery road) ... 1.00 (good
        grip)
        Water Water amount in mm 0.00 ... 10.00 mm
        Ice Ice amount in mm 0.00 ... 10.00 mm
        Snow Snow amount in mm 0.00 ... 10.00 mm
        Optical Signals Reflected signals on which the
        analysis is based
        2 ... 20000. Dry road normally 9 ...
        600.
        Light powers I1, I2, I3 = Currents of the
        transmitter lasers
        Normal value:
        I(Tlaser) = I(25) + 0.3 x (Tlaser - 25 °C)
        mA/°C
        I1(25) = 25 ± 5 mA
        I2(25) = 32 ± 5 mA
        I3(25) = 32 ± 5 mA
        If any of the currents exceeds the
        normal value by more than 15 mA,
        the laser may be damaged and the
        transmitter unit needs to be
        replaced.
        Rapid variations in the laser currents
        are normal.
        Offset Offset of the optical signals -5 ... +5
        Ambient Light Intensity of the ambient light
        measured by the receiver of
        DSC111
        0.00 (dark) ... 0.95 (very bright
        sunshine reflecting from wet surface)
        Box
        Temperatures
        Tin = Temperature inside
        DSC111,
        Tlaser = Temperature of the
        transmitter lasers
        Tin = Tair ± 7°C,
        Tlaser = Tair +(5....30)°C
        Rapid variation of T2 is normal.
        Contamination Contamination state of the
        receiver window
        Refer to section Cleaning on page
        68.
        RX Current value of the
        contamination measurement
        3 ... 700
        Relative Current value divided by the
        contamination reference
        0.1 ... 1.1
        1.0 = clean
        0.1 = dirty
        Calibration status Not in use
        VB Input voltage in volts, the same
        as parameter number 14 in
        MES 14 and MES 16
        9 ... 30 V
        AD Reference Value of the internal VREF
        voltage
        1.2 ... 1.3 V
         */
        static string Sta = "STA";
        
        #endregion

        #region Fields

        int sendRecieveTimeout = 10000;

        int maxRetries = 5;

        IPEndPoint ipEndPoint;

        ProtocolType transportProtocol;

        UdpClient udpClient;
        int udpRecived;
        int udpSent;

        TcpClient tcpClient;
        NetworkStream netStream;
        int tcpRecieved;
        int tcpSent;

        CommandMode currentCommandMode;

        string unitId;
        string stationName;
        string serialNumber;
        List<double> whiteSignals;
        List<double> drySignals;
        List<double> opticalSignals;
        List<double> lightPowers;
        double offset;
        double ambientLight;
        double boxTemp;
        double cleanRx;
        double voltage;
        bool automaticMessageEnabled;
        int automaticMessageDuration;
        string softwareVersion;
        int currentBaud;

        bool opened = false;

        #endregion

        #region Properties

        public Socket Socket
        {
            get
            {
                if (transportProtocol == ProtocolType.Tcp) return tcpClient.Client;
                return udpClient.Client;
            }
        }

        public int TcpSentBytes
        {
            get { return tcpSent; }
        }

        public int TcpRecievedBytes
        {
            get { return tcpRecieved; }
        }

        public int UdpSentBytes
        {
            get { return udpSent; }
        }

        public int UdpRecievedBytes
        {
            get { return udpRecived; }
        }

        public int TotalSentBytes{
            get {return udpSent + tcpSent;}
        }

        public int TotalRecievedBytes{
            get{ return tcpRecieved+ udpRecived;}
        }

        public int SendRecieveTimeout{
            get{ return sendRecieveTimeout;}
            set{ sendRecieveTimeout = value;}
        }

        public ProtocolType TransportProtcol{
            get{ return transportProtocol;}
            set{
                if(value == ProtocolType.Tcp || value == ProtocolType.Udp){
                    transportProtocol = value;                
                }else throw new Exception("Only Tcp and Udp Transport Protocols are supported");
            }
        }

        public int MaxRetries{
            get{ return maxRetries;}
            set{ maxRetries = value;}
        }

        #endregion

        #region Methods

        private void Initialize()
        {
            if (transportProtocol == ProtocolType.Tcp)
            {
                if (udpClient != null)
                {
                    udpClient.Close();
                    udpClient = null;
                }
                if (tcpClient == null)
                {
                    tcpClient = new TcpClient();
                }
                tcpClient.Connect(ipEndPoint);
                netStream = tcpClient.GetStream();
            }
            else
            {
                if (netStream != null)
                {
                    netStream.Close();
                    netStream.Dispose();
                }
                if (tcpClient != null)
                {
                    tcpClient.Close();
                }
                if (udpClient == null)
                {
                    udpClient = new UdpClient();
                }
                udpClient.Connect(ipEndPoint);
            }
        }

        public void Close()
        {
            try
            {
                Socket.Send(Encoding.ASCII.GetBytes(CloseAll + (char)10 + (char)13));

                if (udpClient != null)
                {
                    udpClient.Close();
                    udpClient = null;
                }
                if (netStream != null)
                {
                    netStream.Close();
                    netStream.Dispose();
                    netStream = null;
                }
                if (tcpClient != null)
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
            }
        }

        public void Open()
        {
            string command;
            if (string.IsNullOrEmpty(unitId)) command = "\r\n" + OpenAll + "\r\n";
            else command = "\r\n" + OpenCmd + " " + unitId + " " + "\r\n";

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            opened = Encoding.ASCII.GetString(inbytes).ToUpperInvariant().Contains("DSC OPENED FOR OPERATOR COMMANDS");
            
        }

        public void SetBaud232(int newBaud)
        {
            switch (newBaud)
            {
                case 300:
                case 1200:
                case 2400:
                case 4800:
                case 9600:
                    break;
                default:
                    throw new Exception("Only Baudrate's 300, 1200, 2400, 4800, or 9600 BPS are supported");
            }

            if (!opened) Open();

            string command = (char)10 + (char)13 + Baud232 + " " + unitId + " " + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;


        }

        public void SetBaud485(int newBaud)
        {
            switch (newBaud)
            {
                case 300:
                case 1200:
                case 2400:
                case 4800:
                case 9600:
                    break;
                default:
                    throw new Exception("Only Baudrate's 300, 1200, 2400, 4800, or 9600 BPS are supported");
            }

            if (!opened) Open();

            string command;
            if (string.IsNullOrEmpty(unitId)) command = OpenAll;
            else command = (char)10 + (char)13 + Baud485 + " " + unitId + " " + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;


        }

        public string GetConfiguration()
        {
            if (!opened) Open();

            string command;
            if (string.IsNullOrEmpty(unitId)) command = OpenAll;
            else command = (char)10 + (char)13 + Conf + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string GetConfiguration(string name)
        {
            if (string.IsNullOrEmpty(name)) return GetConfiguration();
            if (!opened) Open();

            string command;
            if (string.IsNullOrEmpty(unitId)) command = OpenAll;
            else command = (char)10 + (char)13 + Conf + " " + name + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string SetConfiguration(string name, string value)
        {
            if(string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
            {
                throw new Exception("name or value cannot be null or empty");
            }

            if (!opened) Open();

            string command;
            if (string.IsNullOrEmpty(unitId)) command = OpenAll;
            else command = (char)10 + (char)13 + Conf + " " + name + " " + value + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            return Encoding.ASCII.GetString(inbytes);

        }

        public string InitAll()
        {
            if (!opened) Open();

            string command;
            if (string.IsNullOrEmpty(unitId)) command = OpenAll;
            else command = (char)10 + (char)13 + Init + " ALL " + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string InitConf()
        {
            if (!opened) Open();

            string command;
            if (string.IsNullOrEmpty(unitId)) command = OpenAll;
            else command = (char)10 + (char)13 + Init + " CONF " + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string InitHist()
        {
            if (!opened) Open();

            string command;
            if (string.IsNullOrEmpty(unitId)) command = OpenAll;
            else command = (char)10 + (char)13 + Init + " HIST " + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string GetPar()
        {
            if (!opened) Open();

            string command;
            if (string.IsNullOrEmpty(unitId)) command = OpenAll;
            else command = (char)10 + (char)13 + Par + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string GetId()
        {
            if (!opened) Open();

            string command;
            if (string.IsNullOrEmpty(unitId)) command = OpenAll;
            else command = (char)10 + (char)13 + Id + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string SetId(string Id)
        {
            if(string.IsNullOrEmpty(Id))
            {
                throw new Exception("Id cannot be null or empty");
            }
            if (Id.Length > 2)
            {
                throw new Exception("Id must be less than 2 characters");
            }

            if (!opened) Open();

            string command;
            if (string.IsNullOrEmpty(unitId)) command = OpenAll;
            else command = (char)10 + (char)13 + Messenger.Id + " " + Id + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            this.unitId = Id;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string GetName()
        {
            if (!opened) Open();

            string command;
            if (string.IsNullOrEmpty(unitId)) command = OpenAll;
            else command = Name + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            this.unitId = Id;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string SetName(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new Exception("name cannot be null or empty");
            if (name.Length > 19) throw new Exception("name cannot be greater than 19 characters long");

            if (!opened) Open();

            string command;
            if (string.IsNullOrEmpty(unitId)) command = OpenAll;
            else command = Name +  " " + name + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            this.unitId = Id;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string GetVersion()
        {
            if (!opened) Open();

            string command = Ver + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            this.unitId = Id;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string GetAmes()
        {
            if (!opened) Open();

            string command = Ames + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            this.unitId = Id;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string SetAmes(int messageNumber, int interval)
        {
            if (interval < 0) interval = 0;

            if (!opened) Open();

            string command = Ames + " " + messageNumber + " " + interval +(char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            this.unitId = Id;

            return Encoding.ASCII.GetString(inbytes);

        }

        public string DisableAmes()
        {
            if (!opened) Open();

            string command = Ames + " " + -1 + " " + 0 + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            this.unitId = Id;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string GetLastHistoryMessage()
        {
            if (!opened) Open();

            string command = Hist + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            this.unitId = Id;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string GetAllHistoryMessage()
        {
            if (!opened) Open();

            string command = Hist + " A" + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            this.unitId = Id;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string GetHistoryMemory(int number)
        {
            if (!opened) Open();

            string command = Hist + " " + number + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            this.unitId = Id;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string GetContaminationReference()
        {
            if (!opened) Open();

            string command = Clean + " " + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string UpdateContaminationReference()
        {
            if (!opened) Open();

            string command = Clean + " ON" + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string GetDrySignals()
        {
            if (!opened) Open();

            string command = Dry + " " + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string SetDrySignals()
        {
            if (!opened) Open();

            string command = Dry + " ON" + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string SetDrySignalsToHighSignals()
        {
            if (!opened) Open();

            string command = Dry + " AUTO" + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string SetMoistSignals()
        {
            if (!opened) Open();

            string command = Moist + " 0.08" + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            return Encoding.ASCII.GetString(inbytes);
        }

        public string GetStatus()
        {
            if (!opened) Open();

            string command = Sta + " " + (char)10 + (char)13;

            byte[] outBytes = Encoding.ASCII.GetBytes(command);
            int sent = Socket.Send(outBytes);
            if (transportProtocol == ProtocolType.Tcp) tcpSent += sent;
            else udpSent += sent;

            byte[] buffer = new byte[1024];
            int recieved = Socket.Receive(buffer);

            byte[] inbytes = new byte[recieved];

            Array.Copy(buffer, inbytes, recieved);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += recieved;
            else udpRecived += recieved;

            return Encoding.ASCII.GetString(inbytes);
        }

        public void SetDate(DateTime date)
        {
        }

        public void SetDate()
        {
        }

        public DateTime GetDate()
        {
            return DateTime.Now;
        }

        public DateTime GetTime()
        {
            return DateTime.Now;
        }

        public void SetTime()
        {
        }

        public void SetTime(DateTime time)
        {
        }

        public void Reset()
        {
            //issue reset cmd
        }

        public void Poll()
        {
            //check currentCommandMode if local just send
            //else @[unitId]M16            
        }

        #endregion

        #region Constructor

        public Messenger(CommandMode commandMode, ProtocolType transportProtocol, IPEndPoint ipEndPoint)
        {
            if (transportProtocol != ProtocolType.Tcp &&
                transportProtocol != ProtocolType.Udp)
            {
                throw new Exception("Only Tcp or Udp Transport Protocols are supported");
            }
            this.ipEndPoint = ipEndPoint;
            this.currentCommandMode = commandMode;
            this.transportProtocol = transportProtocol;
            Initialize();
        }

        #endregion

        #region Destructor

        ~Messenger()
        {
            Close();
        }

        #endregion
    }
}
