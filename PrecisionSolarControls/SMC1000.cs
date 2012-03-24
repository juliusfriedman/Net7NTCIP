using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PrecisionSolarControls
{
    public class SMC1000Messenger
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
                set {
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
            public static byte MinimumOnTime = 0x1e;
            public static byte MaximumOnTime = 0xc8;
            public static int MaxLines = 3;
            List<Line> lines = new List<Line>();
            float onTime;
            BlinkCode blinkCode;
            short sequenceNumber;

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
                    if (value == BlinkCode.Illegal) throw new Exception("Cannot use BlinkCode.Illegal");
                    blinkCode = value;
                }
            }

            public float OnTime
            {
                get { return onTime; }
                set
                {
                    if (value == 0) { onTime = value; return; }                    
                    if (value != 0 && value < (float)MinimumOnTime / 100) throw new Exception("OnTime must be grather than '" + MinimumOnTime + "'");
                    if (value > (float)MaximumOnTime / 100) throw new Exception("OnTime must be less than '" + MaximumOnTime + "'");
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
                    value.ForEach(ln =>
                    {
                        if (ln.Text.Length > Line.MaxLength)
                            throw new Exception("Line text exceeds '" + Line.MaxLength + "'");
                    });
                    lines = value;
                }
            }

            public string MessageText
            {
                get
                {
                    StringBuilder sb = new StringBuilder();
                    Lines.ForEach(l => sb.Append(l.Text + Environment.NewLine));
                    return sb.ToString();
                }
            }     

            public Sequence(List<Line> lines, BlinkCode blinkCode, int onTime)
            {
                Lines = lines;
                BlinkCode = blinkCode;
                this.onTime = onTime;
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
                    result.AddRange(l.ToBytes());                    
                });
                result.AddRange(Encoding.ASCII.GetBytes(((int)blinkCode).ToString()));
                result.AddRange(Encoding.ASCII.GetBytes(((int)onTime).ToString("X2")));
                return result;
            }
        }

        public class Line
        {
            public static int MaxLength = 8;
            string line;

            public string Text
            {
                get { return line; }
                set
                {
                    if (value.Length > MaxLength) throw new Exception("Line length cannot be grather than '" + MaxLength + "'");
                    while(value.Length < MaxLength)
                    {
                        value += ' ';
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

            public static byte CommandStart = 0x0a;
            public static byte CommandEnd = 0x0d;

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
            public DisplayCommand() : base()
            {
                base.CommandCharacters = new char[] { 'D' };
                base.NormalResponse = new byte[] { 0x44, 0x00 };
                base.ErrorResponse = new byte[] { 0x44, 0x3f };
            }

            public DisplayCommand(Message msg)
                :this()
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
                while (msg.Frames.Count != Message.MaxFrames)
                {
                    msg.Frames.Add(new Sequence(new List<Line>() { new Line(string.Empty), new Line(string.Empty), new Line(string.Empty) }, BlinkCode.NoLinesBlink, 0));
                }
                List<byte> result = new List<byte>();
                result.Add(CommandStart);
                result.AddRange(Encoding.ASCII.GetBytes(CommandCharacters));
                msg.Frames.ForEach(f => result.AddRange(f.ToBytes()));
                result.Add(CommandEnd);
                return result;
            }
        }

        public class MessgeGetCommand : CommandBase
        {

            public MessgeGetCommand()
                : base()
            {
                base.CommandCharacters = new char[] { 'M', 'G' };
                base.ErrorResponse = new byte[] {0x4d, 0x47, 0x3f };
            }

            int messageNumber;

            public int MessageNumber
            {
                get { return messageNumber; }
                set { messageNumber = value; }
            }

            public override List<byte> ToBytes()
            {
                List<byte> result = new List<byte>();
                result.Add(CommandStart);
                result.AddRange(Encoding.ASCII.GetBytes(CommandCharacters));
                result.AddRange(Encoding.ASCII.GetBytes(messageNumber.ToString("X3")));                
                /*string msgstr = messageNumber.ToString();
                while (msgstr.Length != 3)
                {
                    msgstr = '0' + msgstr;
                }
                result.AddRange(Encoding.ASCII.GetBytes(msgstr));
                */
                result.Add(CommandEnd);
                return result;
            }

        }

        public class MessagePutCommand : CommandBase
        {
            public MessagePutCommand()
                : base()
            {
                base.CommandCharacters = new char[] { 'M','P' };
                base.NormalResponse = new byte[] { 0x4d, 0x50, 0x30 };
                base.ErrorResponse = new byte[] { 0x4d, 0x50, 0x3f };
            }
           
            int messageNumber;

            List<string> lines = new List<string>();

            public int MessageNumber
            {
                get { return messageNumber; }
                set { messageNumber = value; }
            }

            public void AddLine(string line)
            {
                if (lines.Count >= Sequence.MaxLines) throw new Exception("Cannot have more than '" + Sequence.MaxLines + "' per message, remove a line first");
                lines.Add(new Line(line).Text);
            }

            public void ClearLines()
            {
                lines.Clear();
            }

            public void RemoveLine(int lineNumber)
            {
                if (lineNumber > 0) lineNumber--;
                lines.RemoveAt(lineNumber);
            }

            public override List<byte> ToBytes()
            {
                List<byte> result = new List<byte>();

                result.Add(CommandStart);
                result.AddRange(Encoding.ASCII.GetBytes(CommandCharacters));
                result.AddRange(Encoding.ASCII.GetBytes(messageNumber.ToString("X3")));
                /*string msgstr = messageNumber.ToString();
                while (msgstr.Length != 3)
                {
                    msgstr = '0' + msgstr;
                }
                result.AddRange(Encoding.ASCII.GetBytes(msgstr));*/
                lines.ForEach(l => result.AddRange(Encoding.ASCII.GetBytes(l)));
                result.Add(CommandEnd);
                return result;
            }
        }

        public class QueryCommand : CommandBase
        {
            public QueryCommand()
                : base()
            {
                base.CommandCharacters = new char[] { 'Q' };                                
            }            
        }

        public class RunCommand : CommandBase
        {
            public RunCommand()
                : base()
            {
                base.CommandCharacters = new char[] { 'R' };
                base.NormalResponse = new byte[] { 0x52 };
                base.ErrorResponse = new byte[] { 0x52, 0x3f };
            }

            Int16 sequenceNumber;

            public Int16 SequenceNumber
            {
                get { return sequenceNumber; }
                set
                {
                    sequenceNumber = value;
                    base.NormalResponse = Encoding.ASCII.GetBytes("R" + sequenceNumber.ToString());
                }
            }

            public override List<byte> ToBytes()
            {
                List<byte> result = new List<byte>();
                result.Add(CommandStart);
                result.AddRange(Encoding.ASCII.GetBytes(CommandCharacters));
                result.AddRange(Encoding.ASCII.GetBytes(sequenceNumber.ToString("X2")));

                /*
                string seqStr = sequenceNumber.ToString();
                while (seqStr.Length < 3) seqStr = '0' + seqStr;
                result.AddRange(Encoding.ASCII.GetBytes(seqStr));
                */

                result.Add(CommandEnd);
                return result;
            }
        }

        public class StopCommand : RunCommand
        {
            public StopCommand()
                : base()
            {
                base.SequenceNumber = 0xff;
            }
        }

        public class RunCalendarCommand : RunCommand
        {
            public RunCalendarCommand()
                : base()
            {
                base.SequenceNumber = 0xfe;
            }
        }

        public class SequenceGetCommand : CommandBase
        {
            public SequenceGetCommand()
                : base()
            {
                base.CommandCharacters = new char[] { 'S','G' };
                base.NormalResponse = new byte[] { 0x53, 0x47 };
                base.ErrorResponse = new byte[] { 0x53, 0x47, 0x3f };
            }

            Int16 sequenceNumber;

            public Int16 SequenceNumber
            {
                get { return sequenceNumber; }
                set
                {
                    sequenceNumber = value;
                }
            }

            public override List<byte> ToBytes()
            {
                List<byte> result = new List<byte>();
                result.Add(CommandStart);
                result.AddRange(Encoding.ASCII.GetBytes(CommandCharacters));
                //string seqNum = sequenceNumber.ToString();
                //while (seqNum.Length < 2) seqNum = ' ' + seqNum;
                //result.AddRange(Encoding.ASCII.GetBytes(seqNum));
                result.AddRange(Encoding.ASCII.GetBytes(sequenceNumber.ToString("X2")));
                result.Add(CommandEnd);
                return result;
            }
        }

        public class SequencePutCommand : CommandBase
        {
            public SequencePutCommand()
                : base()
            {
                base.CommandCharacters = new char[] { 'S','P' };
                base.NormalResponse = new byte[] { 0x53, 0x50, 0x30 };
                base.ErrorResponse = new byte[] { 0x53, 0x50, 0x3f };
            }

            public List<int[]> commandParts = new List<int[]>();

            Int16 sequenceNumber;

            public Int16 SequenceNumber
            {
                get { return sequenceNumber; }
                set
                {
                    sequenceNumber = value;
                }
            }

            public void AddSequence(int messageNumber, BlinkCode blinkCode, int onTime)
            {
                if (commandParts.Count >= Message.MaxFrames) throw new Exception("Maximum Page Count reached, remove a page before adding another");
                if (onTime != 0)
                {
                    if (onTime < Sequence.MinimumOnTime) throw new Exception("onTime must be greater than '" + Sequence.MinimumOnTime + "'");
                    if (onTime > Sequence.MaximumOnTime) throw new Exception("onTime must be less than '" + Sequence.MaximumOnTime + "'");
                }
                if (blinkCode == BlinkCode.Illegal) throw new Exception("You cannot use BlinkCode Illegal");                
                int[] prts = new int[] {messageNumber, (int)blinkCode, onTime };
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
                result.AddRange(Encoding.ASCII.GetBytes(sequenceNumber.ToString("X2")));
                commandParts.ForEach(cp => {

                    //string msgNum = cp[0].ToString();
                    //while (msgNum.Length < 3) msgNum = '0' + msgNum;                    
                    //result.AddRange(Encoding.ASCII.GetBytes(msgNum));
                    result.AddRange(Encoding.ASCII.GetBytes(cp[0].ToString("X3")));

                    //string msgCode = cp[1].ToString();
                    //if (msgCode.Length > 1) msgCode = msgCode[0].ToString();
                    //result.AddRange(Encoding.ASCII.GetBytes(msgCode));
                    result.AddRange(Encoding.ASCII.GetBytes(cp[1].ToString("X1")));

                    //string onTime = cp[2].ToString();
                    //while (onTime.Length > 2) onTime = '0' + onTime;
                    //result.AddRange(Encoding.ASCII.GetBytes(onTime));
                    result.AddRange(Encoding.ASCII.GetBytes(cp[2].ToString("X2")));

                });
                result.Add(CommandEnd);
                return result;
            }            
        }

        #endregion

        #region Enums

        public enum BlinkCode : int
        {
            NoLinesBlink = 0,
            Line1Blink,
            Line2Blink,
            Line1and2Blink,
            Line3Blink,
            Line1and3Blink,
            Line2and3Blink,
            AllLinesBlink,
            Illegal = 8,
            Line1Sequencing,
            Line2Sequencing,
            Line1and2Sequencing,
            Line3Sequencing,
            Line1and3Sequencing,
            Line2and3Sequencing,
            AllLinesSequencing,
        }

        #endregion       

        #region Fields

        ProtocolType transportProtocol = ProtocolType.Udp;
        IPEndPoint ipEndPoint;

        int sendRecieveTimeout = 10000;

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

        #region Methods

        void Initialize()
        {
            if (transportProtocol == ProtocolType.Tcp)
            {
                if (udpClient != null)
                {
                    udpClient.Close();
                    udpClient = null;
                }
                if (tcpClient == null) tcpClient = new TcpClient();
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
                if (udpClient == null) udpClient = new UdpClient();
                udpClient.Connect(ipEndPoint);
            }

            Socket.SendTimeout = Socket.ReceiveTimeout = sendRecieveTimeout;
        }        

        byte[] SendRecieve(byte[] bytes, int retry)
        {
            if (retry > maxRetries) throw new Exception("Maximum retries reached");
            try
            {
                Send(bytes);
                return Recieve();
            }
            catch
            {
                return SendRecieve(bytes, retry++);
            }
        }

        void Send(byte[] bytes)
        {
            sendLog.Add(bytes);

            if (transportProtocol == ProtocolType.Tcp) tcpSent += bytes.Length;
            else udpSent += bytes.Length;

            if (transportProtocol == ProtocolType.Tcp) netStream.Write(bytes, 0, bytes.Length);
            else Socket.Send(bytes);

            return;
        }

        byte[] Recieve()
        {
            byte[] buffer = new byte[512];

            int rcv = Socket.Receive(buffer);

            byte[] result = new byte[rcv];
            
            Array.Copy(buffer, 0, result, 0, rcv);

            if (transportProtocol == ProtocolType.Tcp) tcpRecieved += rcv;
            else udpRecieved += rcv;

            return result;
        }

        public bool DisplayMessage(Message msg)
        {
            DisplayCommand cmd = new DisplayCommand(msg);            

            byte[] outBytes = cmd.ToBytes().ToArray();

            List<byte> result = new List<byte>(SendRecieve(outBytes, 0));

            while (result[result.Count - 1] != CommandBase.CommandEnd)
            {
                result.AddRange(Recieve());
            }

            result.RemoveAt(result.Count - 1);

            if (BitConverter.ToString(result.ToArray()) == BitConverter.ToString(cmd.ErrorResponse)) return false;
            return true;
        }

        public Message GetMessage(int messageNumber)
        {
            if (messageNumber < 0) throw new ArgumentOutOfRangeException("messageNumber", "Cannot be less than 0");
            if (messageNumber > 500) throw new ArgumentOutOfRangeException("messageNumber", "Cannot be greater than 500");

            Message resultMsg = new Message();

            MessgeGetCommand cmd = new MessgeGetCommand()
            {
                MessageNumber = messageNumber
            };

            byte[] outBytes = cmd.ToBytes().ToArray();

            List<byte> result = new List<byte>(SendRecieve(outBytes, 0));

            while (result[result.Count - 1] != CommandBase.CommandEnd)
            {
                result.AddRange(Recieve());
            }

            result.RemoveAt(result.Count - 1);

            if (BitConverter.ToString(result.ToArray()) == BitConverter.ToString(cmd.ErrorResponse)) return resultMsg;
            result.RemoveRange(0, cmd.CommandCharacters.Length);

            if (int.Parse(Encoding.ASCII.GetString(result.GetRange(0, 3).ToArray()), System.Globalization.NumberStyles.HexNumber) != messageNumber) return resultMsg;
            result.RemoveRange(0, 3);

            resultMsg.MessageNumber = (short)messageNumber;

            while (result.Count > 0)
            {
                string line1 = Encoding.ASCII.GetString(result.GetRange(0, 8).ToArray());
                string line2 = Encoding.ASCII.GetString(result.GetRange(8, 8).ToArray());
                string line3 = Encoding.ASCII.GetString(result.GetRange(16, 8).ToArray());
                resultMsg.AddPage(new string[] { line1, line2, line3 }, BlinkCode.NoLinesBlink, Sequence.MinimumOnTime);
                result.RemoveRange(0, 24);
            }

            //return Encoding.ASCII.GetString(result.ToArray());

            return resultMsg;
        }

        public bool PutMessage(int messageNumber, string[] lines)
        {

            if (messageNumber < 251 || messageNumber > 500) throw new Exception("Only messages number 251 - 500 can be changed");

            MessagePutCommand cmd = new MessagePutCommand()
            {
                MessageNumber = messageNumber
            };

            foreach (string line in lines)
                cmd.AddLine(line);

            byte[] outBytes = cmd.ToBytes().ToArray();

            List<byte> result = new List<byte>(SendRecieve(outBytes, 0));

            while (result[result.Count - 1] != CommandBase.CommandEnd)
            {
                result.AddRange(Recieve());
            }

            result.RemoveAt(result.Count - 1);

            if (BitConverter.ToString(result.ToArray()) == BitConverter.ToString(cmd.ErrorResponse)) return false;
            return true;
        }

        public Message QueryMessage()
        {
            Message response = new Message();
            QueryCommand cmd = new QueryCommand();


            byte[] outBytes = cmd.ToBytes().ToArray();

            List<byte> result = new List<byte>(SendRecieve(outBytes, 0));

            while (result[result.Count - 1] != CommandBase.CommandEnd)
            {
                result.AddRange(Recieve());
            }

            result.RemoveAt(result.Count - 1);

            if (Encoding.ASCII.GetString(result.ToArray()) == "QFF") 
            {
                response.MessageNumber = 0xff;                
                return response;
            }
            else
            {
                result.RemoveRange(0, cmd.CommandCharacters.Length);

                response.MessageNumber = Int16.Parse(Encoding.ASCII.GetString(result.GetRange(0, 2).ToArray()), System.Globalization.NumberStyles.HexNumber);//Convert.ToInt16(System.Text.Encoding.ASCII.GetString(result.GetRange(0, 2).ToArray()));

                result.RemoveRange(0, 2);

                while (result.Count > 0)
                {
                    byte[] workingFrame = result.GetRange(0, 27).ToArray();
                    BlinkCode code;
                    int onTime;
                    string[] lines = new string[Sequence.MaxLines];
                    lines[0] = Encoding.ASCII.GetString(workingFrame, 0, 8);
                    lines[1] = Encoding.ASCII.GetString(workingFrame, 8, 8);
                    lines[2] = Encoding.ASCII.GetString(workingFrame, 16, 8);
                    code = (BlinkCode)Convert.ToInt16(System.Text.Encoding.ASCII.GetString(workingFrame, 24, 1));
                    onTime = int.Parse(System.Text.Encoding.ASCII.GetString(workingFrame, 25, 2), System.Globalization.NumberStyles.HexNumber);//workingFrame[25] | workingFrame[26];
                    response.AddPage(lines, code, onTime);
                    result.RemoveRange(0, 27);
                }


                return response;
            }

        }

        public bool RunSequence(int sequenceNumber)
        {
            if (sequenceNumber < 0) throw new Exception("sequenceNumber must be greather than 0");
            if (sequenceNumber > 100) throw new Exception("sequenceNumebr must be less than 100");//107
            RunCommand cmd = new RunCommand()
            {
                SequenceNumber = (short)sequenceNumber
            };

            byte[] outBytes = cmd.ToBytes().ToArray();

            List<byte> result = new List<byte>(SendRecieve(outBytes, 0));

            while (result[result.Count - 1] != CommandBase.CommandEnd)
            {
                result.AddRange(Recieve());
            }

            result.RemoveAt(result.Count - 1);

            if (BitConverter.ToString(result.ToArray()) == BitConverter.ToString(cmd.ErrorResponse)) return false;
            return true;
        }

        public Message GetSequence(short sequenceNumber)
        {
            if(sequenceNumber < 0) throw new Exception("sequenceNumber must be greather than 0");
            if (sequenceNumber > 100) throw new Exception("sequenceNumebr must be less than 100");//107
            SequenceGetCommand cmd = new SequenceGetCommand()
            {
                SequenceNumber = (short)sequenceNumber
            };                        

            Message result = new Message();
            result.MessageNumber = -1;            

            byte[] outBytes = cmd.ToBytes().ToArray();

            List<byte> response = new List<byte>(SendRecieve(outBytes, 0));

            while (response[response.Count - 1] != CommandBase.CommandEnd)
            {
                response.AddRange(Recieve());
            }

            response.RemoveAt(response.Count - 1);

            if (BitConverter.ToString(response.ToArray()) == BitConverter.ToString(cmd.ErrorResponse)) return result;

            response.RemoveRange(0, cmd.CommandCharacters.Length);

            result.MessageNumber = Int16.Parse(Encoding.ASCII.GetString(response.GetRange(0, 2).ToArray()), System.Globalization.NumberStyles.HexNumber);//Convert.ToInt16( Encoding.ASCII.GetString(response.GetRange(0, 2).ToArray()) );

            response.RemoveRange(0, 2);

            while (response.Count > 0)
            {
                byte[] workingFrame = response.GetRange(0, 6).ToArray();
                int msgNumber;
                msgNumber = int.Parse(Encoding.ASCII.GetString(response.GetRange(0, 3).ToArray()), System.Globalization.NumberStyles.HexNumber);//Convert.ToInt32(Encoding.ASCII.GetString(response.GetRange(0, 3).ToArray()));
                response.RemoveRange(0, 3);
                BlinkCode code;
                code = ((BlinkCode)Convert.ToInt32(Encoding.ASCII.GetString(response.GetRange(0, 1).ToArray())));
                response.RemoveAt(0);
                int onTime;
                onTime = int.Parse(Encoding.ASCII.GetString(response.GetRange(0, 2).ToArray()), System.Globalization.NumberStyles.HexNumber); //(Convert.ToInt32(Encoding.ASCII.GetString(response.GetRange(0, 2).ToArray())));
                response.RemoveRange(0, 2);
                result.Frames.Add(new Sequence(msgNumber, code, onTime));
                if (msgNumber > 0)
                {
                    Message msgText = GetMessage(msgNumber);                    
                    result.Frames[result.Frames.Count - 1].Lines = msgText.Frames[0].Lines;
                }
                else
                {
                    result.Frames[result.Frames.Count - 1].Lines = new List<Line>() { new Line(string.Empty), new Line(string.Empty), new Line(string.Empty) };
                }
            }

            return result;

        }

        public bool PutSequence(SequencePutCommand cmd)
        {


            //for each msg in the sequence get the message by number
            //if the BlinkCode for the Line is Sequencing then check for '<', '-', '>'
            //if both '<' and '>' appear then use blinking
            //if any other character is present than '<', '-', '>' then use blinking

            cmd.commandParts.ForEach(cp =>
            {
                BlinkCode bc = (BlinkCode)cp[1];
                if (bc == BlinkCode.Line1Sequencing || bc == BlinkCode.Line2Sequencing || bc == BlinkCode.Line3Sequencing
                    || bc == BlinkCode.Line1and2Sequencing || bc == BlinkCode.Line1and3Sequencing || bc == BlinkCode.Line2and3Sequencing
                    || bc == BlinkCode.AllLinesSequencing)
                {
                    Message text = GetMessage(cp[0]);
                    string t;
                    if (bc == BlinkCode.Line1Sequencing)
                    {
                        t = text.Frames[0].Lines[0].Text;
                        if (t.Contains("<") && t.Contains(">"))
                        {
                            cp[1] = (int)BlinkCode.Line1Blink;
                        }
                        else
                        {
                            foreach (char c in t)
                            {
                                if (c != '<' && c != '>' && c != '-' && c != ' ')
                                {
                                    cp[1] = (int)BlinkCode.Line1Blink;
                                }
                            }
                        }
                    }
                    else if (bc == BlinkCode.Line2Sequencing)
                    {
                        t = text.Frames[0].Lines[1].Text;
                        if (t.Contains("<") && t.Contains(">"))
                        {
                            cp[1] = (int)BlinkCode.Line2Blink;
                        }
                        else
                        {
                            foreach (char c in t)
                            {
                                if (c != '<' && c != '>' && c != '-' && c != ' ')
                                {
                                    cp[1] = (int)BlinkCode.Line2Blink;
                                }
                            }
                        }
                    }
                    else if (bc == BlinkCode.Line3Sequencing)
                    {
                        t = text.Frames[0].Lines[2].Text;
                        if (t.Contains("<") && t.Contains(">"))
                        {
                            cp[1] = (int)BlinkCode.Line3Blink;
                        }
                        else
                        {
                            foreach (char c in t)
                            {
                                if (c != '<' && c != '>' && c != '-' && c != ' ')
                                {
                                    cp[1] = (int)BlinkCode.Line3Blink;
                                }
                            }
                        }
                    }
                    else if (bc == BlinkCode.Line1and3Sequencing)
                    {
                        t = text.Frames[0].Lines[0].Text; //+ text.Frames[0].Lines[2].Text;
                        if (t.Contains("<") && t.Contains(">"))
                        {
                            cp[1] = (int)BlinkCode.Line1and3Blink;
                        }
                        else
                        {
                            foreach (char c in t)
                            {
                                if (c != '<' && c != '>' && c != '-' && c != ' ')
                                {
                                    cp[1] = (int)BlinkCode.Line1and3Blink;
                                }
                            }
                        }

                        t = text.Frames[0].Lines[2].Text;
                        if (t.Contains("<") && t.Contains(">"))
                        {
                            cp[1] = (int)BlinkCode.Line1and3Blink;
                        }
                        else
                        {
                            foreach (char c in t)
                            {
                                if (c != '<' && c != '>' && c != '-' && c != ' ')
                                {
                                    cp[1] = (int)BlinkCode.Line1and3Blink;
                                }
                            }
                        }
                    }
                    else if (bc == BlinkCode.Line1and2Sequencing)
                    {
                        t = text.Frames[0].Lines[0].Text;//text.Frames[0].Lines[1].Text;
                        if (t.Contains("<") && t.Contains(">"))
                        {
                            cp[1] = (int)BlinkCode.Line1and2Blink;
                        }
                        else
                        {
                            foreach (char c in t)
                            {
                                if (c != '<' && c != '>' && c != '-' && c != ' ')
                                {
                                    cp[1] = (int)BlinkCode.Line1and2Blink;
                                }
                            }
                        }

                        t = text.Frames[0].Lines[1].Text;
                        if (t.Contains("<") && t.Contains(">"))
                        {
                            cp[1] = (int)BlinkCode.Line1and2Blink;
                        }
                        else
                        {
                            foreach (char c in t)
                            {
                                if (c != '<' && c != '>' && c != '-' && c != ' ')
                                {
                                    cp[1] = (int)BlinkCode.Line1and2Blink;
                                }
                            }
                        }
                    }
                    else if (bc == BlinkCode.Line2and3Sequencing)
                    {
                        t = text.Frames[0].Lines[1].Text;//text.Frames[0].Lines[1].Text;
                        if (t.Contains("<") && t.Contains(">"))
                        {
                            cp[1] = (int)BlinkCode.Line2and3Blink;
                        }
                        else
                        {
                            foreach (char c in t)
                            {
                                if (c != '<' && c != '>' && c != '-' && c != ' ')
                                {
                                    cp[1] = (int)BlinkCode.Line2and3Blink;
                                }
                            }
                        }

                        t = text.Frames[0].Lines[2].Text;
                        if (t.Contains("<") && t.Contains(">"))
                        {
                            cp[1] = (int)BlinkCode.Line2and3Blink;
                        }
                        else
                        {
                            foreach (char c in t)
                            {
                                if (c != '<' && c != '>' && c != '-' && c != ' ')
                                {
                                    cp[1] = (int)BlinkCode.Line2and3Blink;
                                }
                            }
                        }
                    }
                    else if (bc == BlinkCode.AllLinesSequencing)
                    {
                        t = text.Frames[0].Lines[0].Text;//text.Frames[0].Lines[1].Text;
                        if (t.Contains("<") && t.Contains(">"))
                        {
                            cp[1] = (int)BlinkCode.AllLinesBlink;
                        }
                        else
                        {
                            foreach (char c in t)
                            {
                                if (c != '<' && c != '>' && c != '-' && c != ' ')
                                {
                                    cp[1] = (int)BlinkCode.AllLinesBlink;
                                }
                            }
                        }

                        t = text.Frames[0].Lines[1].Text;
                        if (t.Contains("<") && t.Contains(">"))
                        {
                            cp[1] = (int)BlinkCode.AllLinesBlink;
                        }
                        else
                        {
                            foreach (char c in t)
                            {
                                if (c != '<' && c != '>' && c != '-' && c != ' ')
                                {
                                    cp[1] = (int)BlinkCode.AllLinesBlink;
                                }
                            }
                        }

                        t = text.Frames[0].Lines[2].Text;
                        if (t.Contains("<") && t.Contains(">"))
                        {
                            cp[1] = (int)BlinkCode.AllLinesBlink;
                        }
                        else
                        {
                            foreach (char c in t)
                            {
                                if (c != '<' && c != '>' && c != '-' && c != ' ')
                                {
                                    cp[1] = (int)BlinkCode.AllLinesBlink;
                                }
                            }
                        }
                    }
                }
            });


            while (true)
            {
                try
                {
                    cmd.AddSequence(0, 0, 0);
                }
                catch
                {
                    break;
                }
            }

            byte[] outBytes = cmd.ToBytes().ToArray();

            List<byte> result = new List<byte>(SendRecieve(outBytes, 0));

            while (result[result.Count - 1] != CommandBase.CommandEnd)
            {
                result.AddRange(Recieve());
            }

            result.RemoveAt(result.Count - 1);

            if (BitConverter.ToString(result.ToArray()) == BitConverter.ToString(cmd.ErrorResponse)) return false;
            return true;
        }

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

        #endregion

        #region Constructor

        public SMC1000Messenger(IPEndPoint ipEndPoint, ProtocolType transportProtocol)
        {
            this.ipEndPoint = ipEndPoint;
            this.transportProtocol = transportProtocol;
            Initialize();
        }

        #endregion

        #region Destructor

        ~SMC1000Messenger()
        {
            Close();
        }

        #endregion
    }
}
