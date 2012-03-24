using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PrecisionSolarControls
{
    public class SMC2000Messenger
    {
        #region Message Classes

        public class Message
        {
            public static int MaxFrames = 6;

            Int16 messageNumber;

            List<Sequence> frames = new List<Sequence>();

            public string MessageText
            {
                get
                {
                    StringBuilder sb = new StringBuilder();
                    frames.ForEach(f => f.Lines.ForEach(l => sb.Append(l.Text + Environment.NewLine)));
                    return sb.ToString();
                }
            }

            public Int16 MessageNumber
            {
                get { return messageNumber; }
                set { messageNumber = value; }
            }

            public List<Sequence> Frames
            {
                get { return frames; }
                set
                {
                    if (value.Count > MaxFrames) throw new Exception("Messages can only have '" + MaxFrames + "' pages");
                    frames = value;
                }
            }

            public void AddPage(string[] lines, BlinkCode blinkCode, int onTime)
            {
                if (frames.Count > MaxFrames) throw new Exception("Max Pages Reached, remove a page first");
                List<Line> linez = new List<Line>();
                foreach (string line in lines)
                    linez.Add(new Line(line));
                frames.Add(new Sequence(linez, blinkCode, onTime));
            }

            public void ClearPages()
            {
                frames.Clear();
            }

            public void RemovePage(int pageNumber)
            {
                if (pageNumber > 0) pageNumber--;
                try
                {
                    frames.RemoveAt(pageNumber);
                }
                catch
                {
                }
            }
        }

        public class Sequence
        {
            public static byte MinimumOnTime = 0x00;
            public static byte MaximumOnTime = 0xFF;
            public static int MaxLines = 3;
            List<Line> lines = new List<Line>();
            float onTime;
            BlinkCode blinkCode;
            short sequenceNumber;
            FontCode fontCode;

            public Int16 SequenceNumber
            {
                get { return sequenceNumber; }
                set { sequenceNumber = value; }
            }

            public BlinkCode BlinkCode
            {
                get { return blinkCode; }
                set
                {
                    blinkCode = value;
                }
            }

            public FontCode FontCode
            {
                get { return fontCode; }
                set { fontCode = value; }
            }

            public float OnTime
            {
                get { return onTime / 10; }
                set
                {                    
                    if (value < MinimumOnTime) throw new Exception("OnTime must be grather than '" + MinimumOnTime + "'");
                    if (value > MaximumOnTime) throw new Exception("OnTime must be less than '" + MaximumOnTime + "'");
                    onTime = value; return;
                }
            }

            public List<Line> Lines
            {
                get
                {
                    return lines;
                }
                set
                {
                    if (value.Count > MaxLines) throw new Exception("Lines Count cannot be grater than '" + MaxLines + "'");                    
                    lines = value;
                }
            }

            public Sequence(List<Line> lines, BlinkCode blinkCode, int onTime)
            {
                Lines = lines;
                BlinkCode = blinkCode;
                OnTime = onTime;
            }

            public Sequence(int messageNumber, BlinkCode blinkCode, int onTime)
            {
                this.sequenceNumber = (short)messageNumber;
                this.blinkCode = blinkCode;
                this.onTime = onTime;
            }

            public List<byte> ToBytes()
            {
                List<byte> result = new List<byte>();
                lines.ForEach(l => {
                    result.Add((byte)fontCode);
                    result.AddRange(l.ToBytes());
                });
                result.AddRange(Encoding.ASCII.GetBytes(((int)blinkCode).ToString()));
                result.AddRange(Encoding.ASCII.GetBytes(((int)onTime).ToString()));
                return result;
            }
        }

        public class Line
        {
            public static int MaxLength = 10;
            string line;

            public string Text
            {
                get { return line; }
                set
                {
                    if (value.Length > MaxLength) throw new Exception("Line length cannot be grather than '" + MaxLength + "'");
                    if (value.Length < MaxLength)
                    {
                        while (value.Length < MaxLength)
                            value += " ";
                    }
                    line = value;
                }
            }

            public Line(string line)
            {
                Text = line;
            }

            public List<byte> ToBytes()
            {
                return new List<byte>(Encoding.ASCII.GetBytes(line));
            }
        }

        #endregion

        #region Command Classes

        public abstract class CommandBase
        {
            #region Statics

            public static byte CommandStart = 0x0a; //<LF>

            public static byte CommandEnd = 0x0d; //<CR>

            public static byte Error = 0x3f;// '?'

            public static byte Success = 0x30;// '0'

            #endregion

            public virtual char[] CommandCharacters { get; set; }

            public virtual List<byte> ToBytes()
            {
                List<byte> result = new List<byte>();
                result.Add(CommandStart);
                result.AddRange(Encoding.ASCII.GetBytes(CommandCharacters));
                result.Add(CommandEnd);
                return result;
            }

            public virtual byte[] NormalResponse { get; set; }

            public virtual byte[] ErrorResponse { get; set; }
        }

        public class DisplayCommand : CommandBase
        {
            public DisplayCommand()
                : base()
            {
                base.CommandCharacters = new char[] { 'D' };
                base.NormalResponse = new byte[] {CommandStart, 0x44, CommandBase.Success, CommandEnd };
                base.ErrorResponse = new byte[] {CommandStart, 0x44, CommandBase.Error, CommandEnd };
            }

            public DisplayCommand(Message msg)
                : this()
            {
                this.msg = msg;
            }

            Message msg;

            public Message Message
            {
                get { return msg; }
                set { msg = value; }
            }

            public override List<byte> ToBytes()
            {
                List<byte> result = new List<byte>();
                result.Add(CommandStart);
                result.AddRange(Encoding.ASCII.GetBytes(CommandCharacters));
                result.AddRange(Encoding.ASCII.GetBytes(msg.Frames.Count.ToString()));
                result.Add(CommandEnd);
                msg.Frames.ForEach(f => result.AddRange(f.ToBytes()));                
                return result;
            }
        }

        public class FileGetCommand : CommandBase
        {
            public FileGetCommand()
                : base()
            {
                base.CommandCharacters = new char[] { 'F', 'G' };
                base.NormalResponse = new byte[] { CommandStart, 0x46, 0x47, CommandBase.Success, CommandEnd };
                base.ErrorResponse = new byte[] { CommandStart, 0x46, 0x47, CommandBase.Error, CommandEnd };
            }

            string fileName = string.Empty;

            public string FileName { get { return fileName; } set {
                if (value.Length > 15) throw new Exception("Filename cannot be longer than 15 characters");
                fileName = value; 
            } }

            public override List<byte> ToBytes()
            {
                List<byte> result = new List<byte>();
                result.Add(CommandStart);
                result.AddRange(Encoding.ASCII.GetBytes(CommandCharacters));
                result.AddRange(Encoding.ASCII.GetBytes(fileName));
                result.Add(CommandEnd);
                return result;
            }
            
        }

        public class FilePutCommand : CommandBase
        {
            public FilePutCommand()
                : base()
            {
                base.CommandCharacters = new char[] { 'F', 'P' };
                base.NormalResponse = new byte[] { CommandStart, 0x46, 0x47, CommandBase.Success, CommandEnd };
                base.ErrorResponse = new byte[] { CommandStart, 0x46, 0x47, CommandBase.Error, CommandEnd };
            }

            string fileName = string.Empty;

            public string FileName
            {
                get { return fileName; }
                set
                {
                    if (value.Length > 15) throw new Exception("Filename cannot be longer than 15 characters");
                    fileName = value;
                }
            }

            byte[] fileData;

            public byte[] FileData { get { return fileData; } set { fileData = value; } }

            public override List<byte> ToBytes()
            {
                List<byte> result = new List<byte>();
                result.Add(CommandStart);
                result.AddRange(Encoding.ASCII.GetBytes(CommandCharacters));
                result.AddRange(Encoding.ASCII.GetBytes(fileName));
                result.Add(CommandEnd);
                return result;
            }
        }

        public class GetTextMessageCommand : CommandBase
        {
            public GetTextMessageCommand(int textMessageNumber)
                : base()
            {
                base.CommandCharacters = new char[] { 'M', 'G', 'T' };
                base.NormalResponse = new byte[] { CommandStart, 0x4d, 0x47, 0x54, CommandBase.Success, CommandEnd };
                base.ErrorResponse = new byte[] { CommandStart, 0x4d, 0x47, 0x54, CommandBase.Error, CommandEnd };
            }

            int textMessageNumber;

            public int TextMessageNumber
            {
                get { return textMessageNumber; }
                set
                {
                    if (value < 0) throw new Exception("TextMessageNumber cannot be less than 0");
                    if (value > 500) throw new Exception("TextMessageNumber cannot be greater than 500");
                    textMessageNumber = value;                  
                }
            }

            public override List<byte> ToBytes()
            {
                List<byte> result = new List<byte>();
                result.Add(CommandStart);
                result.AddRange(Encoding.ASCII.GetBytes(CommandCharacters));
                result.AddRange(Encoding.ASCII.GetBytes(textMessageNumber.ToString("X3")));
                result.Add(CommandEnd);
                return result;
            }
        }

        public class GetGraphicMessageCommand : CommandBase
        {
            public GetGraphicMessageCommand()
                : base()
            {
                base.CommandCharacters = new char[] { 'M', 'G', 'G' };
                base.NormalResponse = new byte[] { 0x4d, 0x47, 0x47, CommandBase.Success, CommandEnd };
                base.ErrorResponse = new byte[] { 0x4d, 0x47, 0x47, CommandBase.Error, CommandEnd };
            }

            int graphicMessageNumber;

            public int GraphicMessageNumber
            {
                get { return graphicMessageNumber; }
                set
                {
                    if (value < 501) throw new Exception("GraphicMessageNumber cannot be less than 501");
                    if (value > 600) throw new Exception("GraphicMessageNumber cannot be greater than 600");
                    graphicMessageNumber = value;
                }
            }

            public override List<byte> ToBytes()
            {
                List<byte> result = new List<byte>();
                result.Add(CommandStart);
                result.AddRange(Encoding.ASCII.GetBytes(CommandCharacters));
                result.AddRange(Encoding.ASCII.GetBytes(graphicMessageNumber.ToString("X3")));
                result.Add(CommandEnd);
                return result;
            }
        }

        public class PutTextMessageCommand : CommandBase
        {
            public PutTextMessageCommand()
                : base()
            {
                base.CommandCharacters = new char[] { 'M', 'P', 'T' };
                base.NormalResponse = new byte[] { CommandStart, 0x4d, 0x50, 0x54, CommandBase.Success, CommandEnd };
                base.ErrorResponse = new byte[] { CommandStart, 0x4d, 0x50, 0x54, CommandBase.Error, CommandEnd };
            }

            int textMessageNumber;

            public int TextMessageNumber { get { return textMessageNumber; } set { textMessageNumber = value; } }

            Line line;

            public Line Line { get { return line; } set { line = value; } }

            public override List<byte> ToBytes()
            {
                List<byte> result = new List<byte>();
                result.Add(CommandStart);
                result.AddRange(Encoding.ASCII.GetBytes(CommandCharacters));
                result.AddRange(Encoding.ASCII.GetBytes(textMessageNumber.ToString("X3")));
                result.AddRange(line.ToBytes());
                result.Add(CommandEnd);
                return result;
            }
        }

        public class PutGraphicMessageCommand : CommandBase
        {

            public PutGraphicMessageCommand()
                : base()
            {
                base.CommandCharacters = new char[] { 'M', 'P', 'G' };
                base.NormalResponse = new byte[] { CommandStart, 0x4d, 0x50, 0x47, CommandBase.Success, CommandEnd };
                base.ErrorResponse = new byte[] { CommandStart, 0x4d, 0x50, 0x47, CommandBase.Error, CommandEnd };
            }

            int graphicMessageNumber;

            public int GraphicMessageNumber { get { return graphicMessageNumber; } set { graphicMessageNumber = value; } }

            byte[] graphicImageData;

            public byte[] GraphicImageData { get { return graphicImageData; } set { graphicImageData = value; } }

            public override List<byte> ToBytes()
            {
                List<byte> result = new List<byte>();
                result.Add(CommandStart);
                result.AddRange(Encoding.ASCII.GetBytes(CommandCharacters));
                result.AddRange(Encoding.ASCII.GetBytes(graphicMessageNumber.ToString("X3")));
                result.Add(CommandEnd);
                result.AddRange(graphicImageData);                
                return result;
            }
        }

        public class QueryCommand : CommandBase
        {
            public QueryCommand()
                : base()
            {
                base.CommandCharacters = new char[] { 'Q' };
                base.NormalResponse = new byte[] { CommandStart, 0x51, 0x00, 0x00, CommandEnd };
                base.ErrorResponse = new byte[] { CommandStart, 0x51, Error, CommandEnd };
            }
        }

        public class RunCommand : CommandBase
        {
            public RunCommand(short messageNumber)
                : base()
            {
                base.CommandCharacters = new char[] { 'R' };                
                base.ErrorResponse = new byte[] { CommandStart, 0x52, Error, CommandEnd };
            }

            Int16 messageNumber;

            public Int16 MessageNumber
            {
                get { return messageNumber; }
                set
                {
                    messageNumber = value;
                    base.NormalResponse = ToBytes().ToArray();
                }
            }

            public override List<byte> ToBytes()
            {
                List<byte> result = new List<byte>();
                result.Add(CommandStart);
                result.AddRange(Encoding.ASCII.GetBytes(CommandCharacters));
                result.AddRange(Encoding.ASCII.GetBytes(messageNumber.ToString()));
                result.Add(CommandEnd);
                return result;
            }
        }

        public class GetSequenceCommand : CommandBase
        {
            public GetSequenceCommand()
                : base()
            {
                base.CommandCharacters = new char[] { 'S', 'G' };
                base.NormalResponse = new byte[] { CommandStart,0x53, 0x47, CommandBase.Success, CommandEnd };
                base.ErrorResponse = new byte[] { CommandStart, 0x53, 0x47, CommandBase.Error, CommandEnd };
            }

            short sequenceNumber;

            public Int16 SequenceNumber { get { return sequenceNumber; } set { sequenceNumber = value; } }

            public override List<byte> ToBytes()
            {
                List<byte> result = new List<byte>();
                result.Add(CommandStart);
                result.AddRange(Encoding.ASCII.GetBytes(CommandCharacters));
                result.AddRange(Encoding.ASCII.GetBytes(sequenceNumber.ToString("X2")));
                result.Add(CommandEnd);                
                return result;
            }
        }

        public class PutSequenceCommand : CommandBase
        {
            public PutSequenceCommand()
                : base()
            {
                base.CommandCharacters = new char[] { 'S', 'P' };
                base.NormalResponse = new byte[] { CommandStart, 0x53, 0x50, CommandBase.Success, CommandEnd };
                base.ErrorResponse = new byte[] { CommandStart, 0x53, 0x50, CommandBase.Error, CommandEnd };
            }

            short sequenceNumber;

            public Int16 SequenceNumber { get { return sequenceNumber; } set { sequenceNumber = value; } }

            public List<int[]> commandParts = new List<int[]>();

            public void AddSequence(int messageNumber, BlinkCode[] blinkCodes, int onTime)
            {
                if (commandParts.Count >= Message.MaxFrames) throw new Exception("Maximum Page Count reached, remove a page before adding another");
                if (onTime != 0)
                {
                    if (onTime < Sequence.MinimumOnTime) throw new Exception("onTime must be greater than '" + Sequence.MinimumOnTime + "'");
                    if (onTime > Sequence.MaximumOnTime) throw new Exception("onTime must be less than '" + Sequence.MaximumOnTime + "'");
                }
                int[] codes = new int[3];
                for (int i = 0, end = 3; i < end; ++i) codes[i] = (int)blinkCodes[i];         
                int[] prts = new int[] { messageNumber, onTime, codes[0], codes[1], codes[2] };
                commandParts.Add(prts);
            }

            public void RemoveSequence(int sequenceNumber)
            {
                if (sequenceNumber > 0) sequenceNumber--;
                commandParts.RemoveAt(sequenceNumber);
            }

            public override List<byte> ToBytes()
            {
                List<byte> result = new List<byte>();
                result.Add(CommandStart);
                result.AddRange(Encoding.ASCII.GetBytes(CommandCharacters));
                result.AddRange(Encoding.ASCII.GetBytes(sequenceNumber.ToString()));
                commandParts.ForEach(cp => result.AddRange(Encoding.ASCII.GetBytes(cp[0].ToString() + cp[1].ToString() + cp[2].ToString() + cp[3].ToString() + cp[4].ToString())));
                result.Add(CommandEnd);
                return result;
            }       
        }

        #endregion

        #region Enums

        public enum BlinkCode : int
        {
            NoBlinking = 0,
            Blinking,
            SequencingRight,
            SequencingLeft
        }

        public enum FontCode : int
        {
            SingleStroke, // 7x6 3 lines
            DoubleStroke,// 7x7 3 lines
            SingleStokeMedium,// 11x7 2 lines
            SingleStrokeLarge// 14x9 1 line
        }

        public enum PageType : byte
        {
            Text = 0x54,
            Graphic = 0x47
        }

        #endregion

        #region Fields

        ProtocolType transportProtocol = ProtocolType.Udp;
        IPEndPoint ipEndPoint;

        int sendRecieveTimeout = 5000;

        int maxRetries = 5;

        UdpClient udpClient;
        TcpClient tcpClient;
        NetworkStream netStream;

        int tcpRecieved;
        int tcpSent;

        int udpRecieved;
        int udpSent;

        List<byte[]> sendLog = new List<byte[]>();

        List<byte[]> recieveLog = new List<byte[]>();

        #endregion

        #region Properties

        public int TotalSentBytes
        {
            get { return tcpSent + udpSent; }
        }

        public int TotalRecievedBytes
        {
            get { return tcpRecieved + udpRecieved; }
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
            get { return udpRecieved; }
        }

        public int SendRecieveTimeout
        {
            get { return sendRecieveTimeout; }
            set 
            {                 
                Socket.SendTimeout = Socket.ReceiveTimeout = sendRecieveTimeout = value;
            }
        }

        public int MaxRetries
        {
            get { return maxRetries; }
            set { maxRetries = value; }
        }

        public ProtocolType TransportProtocol
        {
            get { return transportProtocol; }
            set
            {
                if (value == ProtocolType.Udp || value == ProtocolType.Tcp)
                {
                    transportProtocol = value;
                    Initialize();
                }
                else throw new Exception("Only Tcp or Udp ProtocolTypes are supported");
            }
        }

        public Socket Socket
        {
            get
            {
                if (transportProtocol == ProtocolType.Tcp) return tcpClient.Client;
                else return udpClient.Client;
            }
        }

        public List<byte[]> RequestLog
        {
            get { return sendLog; }
        }

        public List<byte[]> RecieveLog
        {
            get { return recieveLog; }
        }

        #endregion
        
        #region Constructor

        public SMC2000Messenger(IPEndPoint ipEndPoint, ProtocolType transportProtocol)
        {
            this.ipEndPoint = ipEndPoint;
            this.transportProtocol = transportProtocol;
            Initialize();
        }

        #endregion

        #region Destructor

        ~SMC2000Messenger()
        {
            Close();
        }

        #endregion

        #region Methods

        public void ClearRequestLog()
        {
            RequestLog.Clear();
        }

        public void ClearRecieveLog()
        {
            recieveLog.Clear();
        }

        public void Close()
        {
            try
            {
                ClearRecieveLog();
                ClearRequestLog();
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

        private void Initialize()
        {
            if (transportProtocol == ProtocolType.Tcp)
            {
                if (udpClient != null)
                {
                    udpClient.Close();
                    udpClient = null;
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
                    tcpClient = null;
                }
                udpClient.Connect(ipEndPoint);
            }

            Socket.SendTimeout = Socket.ReceiveTimeout = sendRecieveTimeout;
        }

        #endregion

        public Message QueryMessage()
        {
            throw new NotImplementedException();
        }

        public void PutMessage(int p, string[] p_2)
        {
            throw new NotImplementedException();
        }

        public bool RunSequence(int p)
        {
            throw new NotImplementedException();
        }
    }
}
