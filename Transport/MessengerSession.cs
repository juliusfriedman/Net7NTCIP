using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System;


namespace jfriedman.Transport
{
    /// <summary>
    /// A Structure which defines a Configuration for a Messenger
    /// </summary>
    public class MessengerSession 
    {
        #region Fields

        /// <summary>
        /// The next RequestId the Messenger should use
        /// </summary>
        public int NextRequestId
        {
            get
            {
                ++CurrentRequestId;
                if (CurrentRequestId >= 255) CurrentRequestId = 1;
                return CurrentRequestId;
            }
        }

        /// <summary>
        /// The last RequestId the Messenger used
        /// </summary>
        public int LastRequestId
        {
            get
            {
                return CurrentRequestId - 1;
            }
        }

        /// <summary>
        /// The Last ProtocolDataUnit sent in this session
        /// </summary>
        public ProtocolDataUnit LastSentPdu
        {
            get
            {
                try
                {
                    return SentLog[SentLog.Count - 1];
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// The Last ProtocolDataUnit revieved in this session
        /// </summary>
        public ProtocolDataUnit LastRecievedPdu
        {
            get
            {
                try
                {
                    return RecieveLog[RecieveLog.Count - 1];
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// The Last ErrorStatus of the Last PDU sent or recieved
        /// </summary>
        public ErrorStatus LastSendError
        {
            get
            {                
                try
                {
                    return LastSentPdu.ErrorStatus;
                }
                catch
                {
                    return ErrorStatus.NoError;
                }
            }
        }

        /// <summary>
        /// The Last ErrorStatus of the Last PDU sent or recieved
        /// </summary>
        public ErrorStatus LastRecieveError
        {
            get
            {
                try
                {
                    return LastRecievedPdu.ErrorStatus;
                }
                catch
                {
                    return ErrorStatus.NoError;
                }
            }
        }

        /// <summary>
        /// The Socket of the Tcp Transport
        /// </summary>
        public Socket TcpSocket
        {
            get
            {
                return TcpTransport.Socket;
            }
        }

        /// <summary>
        /// The Socket of the Udp Transport
        /// </summary>
        public Socket UdpSocket
        {
            get
            {
                return UdpTransport.Socket;
            }
        }

        /// <summary>
        /// The amount of Bytes sent in this session
        /// </summary>
        public int SentBytes
        {
            get
            {
                return UdpTransport.SentBytes + TcpTransport.SentBytes;
            }
        }
        
        /// <summary>
        /// The amount of Bytes recieved in this session
        /// </summary>
        public int RecievedBytes
        {
            get
            {
                return UdpTransport.RecievedBytes + TcpTransport.RecievedBytes;
            }
        }

        #endregion

        #region Properties
        /// <summary>
        /// The current RequestId of the Messenger Configuration
        /// </summary>
        public int CurrentRequestId;
        /// <summary>
        /// The Amount of General Errors The Messenger has encountered with this configuration
        /// </summary>
        public int GeneralErrors;
        /// <summary>
        /// The Amount of Point to Multi Point Protocol Checksum Errors The Messenger has encountered with this configuration
        /// </summary>
        public int PmppCrcErrors;
        /// <summary>
        /// The Transport Protocol Type The Messenger will use when transporting with this configuration
        /// </summary>
        public ProtocolType TrasnportProtocol;
        /// <summary>
        /// The IPEndPoint The Messenger will use when transporting with this configuration
        /// </summary>
        public IPEndPoint IPEndPoint;
        /// <summary>
        /// The Pmpp EndPoint information the Messenger will ue when transporting with this configuration
        /// </summary>
        public PmppEndPoint? PmppEndPoint;
        /// <summary>
        /// The max amount of times the Messenger will attempt to send or recieving with this configuration
        /// </summary>
        public int MaxRetries;
        /// <summary>
        /// The SendRecieveTimeout of the Socket the Messenger will enforce when sending or recieving with this configuration
        /// </summary>
        public int SendRecieveTimeout;        
        /// <summary>
        /// The SnmpVersion the Messenger will enforce when sending with this configuration
        /// </summary>
        public int SnmpVersion;
        /// <summary>
        /// The CommunityName the Messenger will use when transporting with this configuration
        /// </summary>
        public string CommunityName;
        /// <summary>
        /// Contains a log of all sent Pdu's in this configuration
        /// </summary>
        public List<ProtocolDataUnit> SentLog;
        /// <summary>
        /// Contains a log of all recieved Pdu's in this configuration
        /// </summary>
        public List<ProtocolDataUnit> RecieveLog;

        /// <summary>
        /// Udp Transport Manager
        /// </summary>
        internal UdpTransport UdpTransport;
        /// <summary>
        /// Tcp Transport Manager
        /// </summary>
        internal TcpTransport TcpTransport;

        protected bool disposed;

        #endregion

        #region Methods

        /// <summary>
        /// Randomizes the Request Id of this configuration
        /// </summary>
        public void RandomizeRequestId()
        {
            System.Random rand = new System.Random(this.CurrentRequestId);
            this.CurrentRequestId = rand.Next(1, 255);
            rand = null;
            return;
        }

        //Clears the list of sent Pdu's
        public void ClearSentLog()
        {
            SentLog.Clear();
        }

        /// <summary>
        /// Clears the list of recieved Pdu's
        /// </summary>
        public void ClearRecieveLog()
        {
            RecieveLog.Clear();
        }

        /// <summary>
        /// Clearns the Sent and Recived log and resets the Current Request Id to 0
        /// </summary>
        public void ResetSession()
        {
            ClearRecieveLog();
            ClearSentLog();
            CurrentRequestId = 0;
        }

        /// <summary>
        /// Closes the Transport Sockets and clears the session information
        /// </summary>
        public void Close()
        {
            try
            {
                ResetSession();
                SentLog = null;
                RecieveLog = null;
                this.IPEndPoint = null;
                this.PmppEndPoint = null;
                this.TcpTransport.Close();
                this.TcpTransport = null;
                this.UdpTransport.Close();
                this.UdpTransport = null;
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
        /// Directs the Messenger to perform a Set with this Session
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ProtocolDataUnit Set(Variable value)
        {
            ProtocolDataUnit outBound, inBound;
            byte[] outBytes, recieved;
            List<byte> inBytes;

            if (TrasnportProtocol == ProtocolType.Udp)
            {
                outBound = new ProtocolDataUnit()
                {
                    CommunityName = this.CommunityName,
                    RequestId = this.NextRequestId,
                    PduType = SnmpType.SetRequestPdu                     
                };
                outBound.Bindings.Add(value);

                SentLog.Add(outBound);

                outBytes = outBound.ToPacket(PmppEndPoint).ToArray();

                recieved = UdpTransport.Request(IPEndPoint.Address, IPEndPoint.Port, outBytes, outBytes.Length, SendRecieveTimeout, MaxRetries);

                inBytes = new List<byte>(recieved);

                if (PmppEndPoint.HasValue)
                {
                    inBytes.PmppDecode(false, this);
                }

                inBound = new ProtocolDataUnit(inBytes);

                if (inBound.ErrorStatus == ErrorStatus.GenErr)
                {
                    GeneralErrors++;
                }

                RecieveLog.Add(inBound);

            }
            else if (TrasnportProtocol == ProtocolType.Tcp)
            {
                outBound = new ProtocolDataUnit()
                {
                    CommunityName = this.CommunityName,
                    RequestId = this.NextRequestId,
                    PduType = SnmpType.SetRequestPdu
                };
                outBound.Bindings.Add(value);

                SentLog.Add(outBound);

                outBytes = outBound.ToPacket(PmppEndPoint).ToArray();

                recieved = TcpTransport.Request(IPEndPoint.Address, IPEndPoint.Port, outBytes, outBytes.Length, SendRecieveTimeout, MaxRetries);

                inBytes = new List<byte>(recieved);

                if (PmppEndPoint.HasValue)
                {
                    inBytes.PmppDecode(false, this);
                }

                inBound = new ProtocolDataUnit(inBytes);

                if (inBound.ErrorStatus == ErrorStatus.GenErr)
                {
                    GeneralErrors++;
                }

                RecieveLog.Add(inBound);                
            }
            else throw new System.NotImplementedException("Only Tcp and Udp Protocols are Supported");

            outBound = null;
            outBytes = null;
            recieved = null;
            inBytes = null;

            return inBound;

        }

        /// <summary>
        /// Directs the Messenger to perform a Set with this Session Information
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public ProtocolDataUnit Set(List<Variable> values)
        {
            ProtocolDataUnit outBound, inBound;
            byte[] outBytes, recieved;
            List<byte> inBytes;

            if (TrasnportProtocol == ProtocolType.Udp)
            {
                outBound = new ProtocolDataUnit()
                {
                    CommunityName = this.CommunityName,
                    RequestId = this.NextRequestId,
                    PduType = SnmpType.SetRequestPdu
                };
                outBound.Bindings.AddRange(values);

                SentLog.Add(outBound);

                outBytes = outBound.ToPacket(PmppEndPoint).ToArray();

                recieved = UdpTransport.Request(IPEndPoint.Address, IPEndPoint.Port, outBytes, outBytes.Length, SendRecieveTimeout, MaxRetries);

                inBytes = new List<byte>(recieved);

                if (PmppEndPoint.HasValue)
                {
                    inBytes.PmppDecode(false, this);
                }

                inBound = new ProtocolDataUnit(inBytes);

                if (inBound.ErrorStatus == ErrorStatus.GenErr)
                {
                    GeneralErrors++;
                }

                RecieveLog.Add(inBound);

            }
            else if (TrasnportProtocol == ProtocolType.Tcp)
            {
                outBound = new ProtocolDataUnit()
                {
                    CommunityName = this.CommunityName,
                    RequestId = this.NextRequestId,
                    PduType = SnmpType.SetRequestPdu
                };
                outBound.Bindings.AddRange(values);

                SentLog.Add(outBound);

                outBytes = outBound.ToPacket(PmppEndPoint).ToArray();

                recieved = TcpTransport.Request(IPEndPoint.Address, IPEndPoint.Port, outBytes, outBytes.Length, SendRecieveTimeout, MaxRetries);

                inBytes = new List<byte>(recieved);

                if (PmppEndPoint.HasValue)
                {
                    inBytes.PmppDecode(false, this);
                }

                inBound = new ProtocolDataUnit(inBytes);

                if (inBound.ErrorStatus == ErrorStatus.GenErr)
                {
                    GeneralErrors++;
                }

                RecieveLog.Add(inBound);
            }
            else throw new System.NotImplementedException("Only Tcp and Udp Protocols are Supported");

            outBound = null;
            outBytes = null;
            recieved = null;
            inBytes = null;

            return inBound;
        }

        /// <summary>
        /// Directs the Messenger to perform a Set with this Session
        /// </summary>
        /// <param name="value">The Variable to Get from this Session</param>
        /// <returns></returns>
        public ProtocolDataUnit Get(string value)
        {
            ProtocolDataUnit outBound, inBound;
            byte[] outBytes, recieved;
            List<byte> inBytes;

            if (TrasnportProtocol == ProtocolType.Udp)
            {
                outBound = new ProtocolDataUnit()
                {
                    CommunityName = this.CommunityName,
                    RequestId = this.NextRequestId,
                    PduType = SnmpType.GetRequestPdu
                };

                outBound.Bindings.Add(value);

                SentLog.Add(outBound);

                outBytes = outBound.ToPacket(PmppEndPoint).ToArray();

                recieved = UdpTransport.Request(IPEndPoint.Address, IPEndPoint.Port, outBytes, outBytes.Length, SendRecieveTimeout, MaxRetries);

                inBytes = new List<byte>(recieved);

                if (PmppEndPoint.HasValue)
                {
                    inBytes.PmppDecode(false, this);
                }

                inBound = new ProtocolDataUnit(inBytes);

                if (inBound.ErrorStatus == ErrorStatus.GenErr)
                {
                    GeneralErrors++;
                }

                RecieveLog.Add(inBound);

            }
            else if (TrasnportProtocol == ProtocolType.Tcp)
            {
                outBound = new ProtocolDataUnit()
                {
                    CommunityName = this.CommunityName,
                    RequestId = this.NextRequestId,
                    PduType = SnmpType.GetRequestPdu
                };
                outBound.Bindings.Add(value);

                SentLog.Add(outBound);

                outBytes = outBound.ToPacket(PmppEndPoint).ToArray();

                recieved = TcpTransport.Request(IPEndPoint.Address, IPEndPoint.Port, outBytes, outBytes.Length, SendRecieveTimeout, MaxRetries);

                inBytes = new List<byte>(recieved);

                if (PmppEndPoint.HasValue)
                {
                    inBytes.PmppDecode(false, this);
                }

                inBound = new ProtocolDataUnit(inBytes);

                if (inBound.ErrorStatus == ErrorStatus.GenErr)
                {
                    GeneralErrors++;
                }

                RecieveLog.Add(inBound);
            }
            else throw new System.NotImplementedException("Only Tcp and Udp Protocols are Supported");

            outBound = null;
            outBytes = null;
            recieved = null;
            inBytes = null;

            return inBound;
        }

        /// <summary>
        /// Directs the Messenger to perform a Set with this Session Information
        /// </summary>
        /// <param name="valuse">The Variable to Get from this Session</param>
        /// <returns></returns>
        public ProtocolDataUnit Get(List<string> values)
        {
            ProtocolDataUnit outBound, inBound;
            byte[] outBytes, recieved;
            List<byte> inBytes;

            if (TrasnportProtocol == ProtocolType.Udp)
            {
                outBound = new ProtocolDataUnit()
                {
                    CommunityName = this.CommunityName,
                    RequestId = this.NextRequestId,
                    PduType = SnmpType.GetRequestPdu
                };
                outBound.Bindings.AddRange(values);

                SentLog.Add(outBound);

                outBytes = outBound.ToPacket(PmppEndPoint).ToArray();

                recieved = UdpTransport.Request(IPEndPoint.Address, IPEndPoint.Port, outBytes, outBytes.Length, SendRecieveTimeout, MaxRetries);

                inBytes = new List<byte>(recieved);

                if (PmppEndPoint.HasValue)
                {
                    inBytes.PmppDecode(false, this);
                }

                inBound = new ProtocolDataUnit(inBytes);

                if (inBound.ErrorStatus == ErrorStatus.GenErr)
                {
                    GeneralErrors++;
                }

                RecieveLog.Add(inBound);

            }
            else if (TrasnportProtocol == ProtocolType.Tcp)
            {
                outBound = new ProtocolDataUnit()
                {
                    CommunityName = this.CommunityName,
                    RequestId = this.NextRequestId,
                    PduType = SnmpType.GetRequestPdu
                };
                outBound.Bindings.AddRange(values);

                SentLog.Add(outBound);

                outBytes = outBound.ToPacket(PmppEndPoint).ToArray();

                recieved = TcpTransport.Request(IPEndPoint.Address, IPEndPoint.Port, outBytes, outBytes.Length, SendRecieveTimeout, MaxRetries);

                inBytes = new List<byte>(recieved);

                if (PmppEndPoint.HasValue)
                {
                    inBytes.PmppDecode(false, this);
                }

                inBound = new ProtocolDataUnit(inBytes);

                if (inBound.ErrorStatus == ErrorStatus.GenErr)
                {
                    GeneralErrors++;
                }

                RecieveLog.Add(inBound);
            }
            else throw new System.NotImplementedException("Only Tcp and Udp Protocols are Supported");

            outBound = null;
            outBytes = null;
            recieved = null;
            inBytes = null;

            return inBound;
        }

        #endregion

        #region Constructor

        public MessengerSession()
        {
            this.CurrentRequestId = 0;
            SentLog = new List<ProtocolDataUnit>();
            RecieveLog = new List<ProtocolDataUnit>();
            this.CommunityName = "public";
            this.SnmpVersion = 1;
            SendRecieveTimeout = 5000;
            this.MaxRetries = 5;
            this.TcpTransport = new TcpTransport(this);
            this.UdpTransport = new UdpTransport(this);
        }

        /// <summary>
        /// Creates a Messenger Configuration to use with the Messenger
        /// </summary>        
        public MessengerSession(IPEndPoint IPEndPoint, PmppEndPoint? PmppEndPoint)
            :this()
        {            
            this.IPEndPoint = IPEndPoint;
            this.PmppEndPoint = PmppEndPoint;
            this.TrasnportProtocol = ProtocolType.Udp;            
        }

        /// <summary>
        /// Creates a Messenger Configuration to use with the Messenger
        /// </summary>       
        public MessengerSession(IPEndPoint IPEndPoint, PmppEndPoint? PmppEndPoint, string CommunityName)
            :this(IPEndPoint, PmppEndPoint)
        {
            this.CommunityName = CommunityName;
        }

        /// <summary>
        /// Creates a Messenger Configuration to use with the Messenger
        /// </summary>       
        public MessengerSession(IPEndPoint IPEndPoint, PmppEndPoint? PmppEndPoint, string CommunityName, ProtocolType transportProtocol)
            : this(IPEndPoint, PmppEndPoint, CommunityName)
        {
            this.TrasnportProtocol = transportProtocol;
        }

        #endregion
       
    }
}
