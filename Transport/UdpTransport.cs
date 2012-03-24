using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace jfriedman.Transport
{
    /// <summary>
    /// IP/UDP transport class.
    /// </summary>
    class UdpTransport
    {
        #region Properties

        protected Socket _socket;

        protected int _sentBytes;

        protected int _recvBytes;

        protected MessengerSession _session;
        
        protected Object padLock = new object();

        #endregion

        #region Fields

        /// <summary>
        /// Udp Socket
        /// </summary>
        public Socket Socket
        {
            get
            {
                return _socket;
            }
        }

        /// <summary>
        /// Bytes Sent in this Transport instance
        /// </summary>
        public int SentBytes
        {
            get
            {
                return _sentBytes;
            }
        }

        /// <summary>
        /// Bytes Recieved in this Transport instance
        /// </summary>
        public int RecievedBytes
        {
            get
            {
                return _recvBytes;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor. Initializes and binds the Socket class
        /// </summary>
        public UdpTransport(jfriedman.Snmp.Transport.MessengerSession messengerSession)
        {
            this._session = messengerSession;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
            EndPoint ep = (EndPoint)ipEndPoint;
            _socket.Bind(ep);
        }

        #endregion

        #region Destructor

        /// <summary>
        /// Destructor
        /// </summary>
        ~UdpTransport()
        {
            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Make sync request using IP/UDP with request timeouts and retries.
        /// </summary>
        /// <param name="peer">SNMP agent IP address</param>
        /// <param name="port">SNMP agent port number</param>
        /// <param name="buffer">Data to send to the agent</param>
        /// <param name="bufferLength">Data length in the buffer</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <param name="retries">Maximum number of retries. 0 = make a single request with no retry attempts</param>
        /// <returns>Byte array returned by the agent. Null on error</returns>
        /// <exception cref="SnmpException">Thrown on request timed out. SnmpException.ErrorCode is set to
        /// SnmpException.RequestTimedOut constant.</exception>
        public byte[] Request(IPAddress peer, int port, byte[] buffer, int bufferLength, int timeout, int retries)
        {
            lock (padLock)
            {
                if (_socket == null)
                {
                    return null; // socket has been closed. no new operations are possible.
                }
                IPEndPoint netPeer = new IPEndPoint(peer, port);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, timeout);
                int total = 0;
                int recv = 0;
                int retry = 0;
                byte[] partialbuffer = new byte[0];
                byte[] inbuffer = new byte[64 * 1024];
                EndPoint remote = (EndPoint)new IPEndPoint(IPAddress.Any, 0);
                while (true)
                {
                    try
                    {
                        if (total == 0)
                        {
                            _sentBytes += _socket.SendTo(buffer, bufferLength, SocketFlags.None, (EndPoint)netPeer);
                        }
                        _recvBytes += recv = _socket.ReceiveFrom(inbuffer, ref remote);
                        total = recv + partialbuffer.Length;
                    }
                    catch (SocketException ex)
                    {
                        if (ex.ErrorCode == 10040)
                        {
                            recv = 0; // Packet too large
                        }
                        else if (ex.ErrorCode == 10064)
                        {
                            throw new SnmpNetworkException(ex, "Network error: remote host is down.");
                        }
                        else if (ex.ErrorCode == 10065)
                        {
                            throw new SnmpNetworkException(ex, "Network error: remote host is unreachable.");
                        }
                        else if (ex.ErrorCode == 10061)
                        {
                            throw new SnmpNetworkException(ex, "Network error: connection refused.");
                        }
                        else if (ex.ErrorCode == 10060)
                        {
                            recv = 0; // Connection attempt timed out. Fall through to retry
                        }
                        else if (ex.ErrorCode == 10050)
                        {
                            throw new SnmpNetworkException(ex, "Network error: Destination network is down.");
                        }
                        else if (ex.ErrorCode == 10051)
                        {
                            throw new SnmpNetworkException(ex, "Network error: destination network is unreachable.");
                        }
                        else
                        {
                            // Assume it is a timeout
                        }
                    }
                    if (recv > 0)
                    {
                        if (remote.ToString() != netPeer.ToString())
                        {
                            /* Not good, we got a response from somebody other then who we requested a response from */
                            retry++;
                            if (retry > retries)
                            {
                                throw new SnmpException(SnmpException.RequestTimedOut, "Request has reached maximum retries.");
                                // return null;
                            }
                        }
                        else
                        {
                            //Acccount for partial responses
                            if (total >= 20)
                            {

                                if (partialbuffer.Length >= 1)
                                {
                                    byte[] t = new byte[recv + partialbuffer.Length];
                                    Buffer.BlockCopy(partialbuffer, 0, t, 0, partialbuffer.Length);
                                    Buffer.BlockCopy(inbuffer, 0, t, partialbuffer.Length, recv);
                                    return t;
                                }
                                else
                                {
                                    byte[] t = new byte[recv];
                                    Buffer.BlockCopy(inbuffer, 0, t, 0, recv);
                                    return t;
                                }


                            }
                            else
                            {
                                partialbuffer = new byte[recv];
                                Buffer.BlockCopy(inbuffer, 0, partialbuffer, 0, recv);
                                inbuffer = new byte[64 * 1024];
                                continue;
                            }
                        }
                    }
                    else
                    {
                        retry++;
                        total = 0;
                        partialbuffer = new byte[0];
                        if (retry > retries)
                        {
                            throw new SnmpException(SnmpException.RequestTimedOut, "Request has reached maximum retries.");
                        }
                    }
                }
            }            
        }

        /// <summary>
        /// Make sync request using IP/UDP with request timeouts and retries.
        /// </summary>
        /// <param name="peer">SNMP agent IP address</param>
        /// <param name="port">SNMP agent port number</param>
        /// <param name="buffer">Data to send to the agent</param>
        /// <param name="bufferLength">Data length in the buffer</param>        
        /// <returns>Byte array returned by the agent. Null on error</returns>
        /// <exception cref="SnmpException">Thrown on request timed out. SnmpException.ErrorCode is set to
        /// SnmpException.RequestTimedOut constant.</exception>
        public byte[] Request(IPAddress peer, int port, byte[] buffer, int bufferLength)
        {
            return Request(peer, port, buffer, bufferLength, _session.SendRecieveTimeout, _session.MaxRetries);
        }
        
        /// <summary>
        /// Dispose of the class. Call this function to close the socket.
        /// </summary>
        public void Close()
        {
            // removed based on MSDN recommendation not to use Shutdown with connectionless socket
            // _socket.Shutdown(SocketShutdown.Both);
            if (null != _socket)
            {
                _socket.Close();
                _socket = null;
            }
        }

        #endregion
    }
}
