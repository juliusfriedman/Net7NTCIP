// This file is part of SNMP#NET.
// 
// SNMP#NET is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// SNMP#NET is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with SNMP#NET.  If not, see <http://www.gnu.org/licenses/>.
// 
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ASTITransportation.Snmp.Transport
{
    /// <summary>
    /// IP/TCP transport class.
    /// </summary>
    class TcpTransport
    {
        static EndPoint AnyRemote = (EndPoint)new IPEndPoint(IPAddress.Any, 0);

        #region Properties

        protected TcpClient _client;

        protected NetworkStream _stream;

        protected int _sentBytes;

        protected int _recvBytes;

        protected SnmpMessenger _session;

        protected Object padLock = new object();

        #endregion

        #region Fields

        public Socket Socket
        {
            get
            {
                return _client.Client;
            }
        }

        public int SentBytes
        {
            get
            {
                return _sentBytes;
            }
        }

        public int RecievedBytes
        {
            get
            {
                return _recvBytes;
            }
        }

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Constructor. Initializes and binds the Socket class
        /// </summary>
        public TcpTransport(SnmpMessenger messengerSession)
        {
            this._session = messengerSession;
        }

        public TcpTransport(SnmpMessenger messengerSession, TcpClient existingClient)
        {
            this._session = messengerSession;
            this._client = existingClient;
        }

        #endregion

        #region Destructor

        /// <summary>
        /// Destructor
        /// </summary>
        ~TcpTransport()
        {
            Close();
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
                if (_client == null) _client = new TcpClient();
                IPEndPoint netPeer = new IPEndPoint(peer, port);
                _client.SendTimeout = timeout;
                _client.ReceiveTimeout = timeout;
                int recv = 0;
                int retry = 0;
                int total = 0;
                byte[] partialbuffer = new byte[0];
                byte[] inbuffer = new byte[64 * 1024];
                EndPoint remote = (EndPoint)AnyRemote;
                if (_client.Connected)
                {
                    if (!_client.Client.RemoteEndPoint.Equals(netPeer))
                    {
                        Close();
                        return Request(peer, port, buffer, bufferLength, timeout, retries);
                    }
                }
                else
                {
                    try
                    {
                        _client.Connect(netPeer);
                    }
                    catch
                    {                        
                        throw;
                    }
                }

                _stream = _client.GetStream();

                while (true)
                {
                    try
                    {
                        if (_client.Client.Available > 0)
                        {
                            _recvBytes += recv = _client.Client.ReceiveFrom(inbuffer, ref remote);
                            total = recv + partialbuffer.Length;
                        }
                        else
                        {
                            if (total == 0)
                            {
                                //_sentBytes += _socket.SendTo(buffer, bufferLength, SocketFlags.None, (EndPoint)netPeer);
                                _stream.Write(buffer, 0, bufferLength);
                                _sentBytes += bufferLength;
                            }
                            if (_client.Client.Poll(timeout * 1000, SelectMode.SelectRead))
                            {
                                _recvBytes += recv = _stream.Read(inbuffer, 0, inbuffer.Length);
                                total = recv + partialbuffer.Length;
                            }

                            //_recvBytes += recv = _socket.ReceiveFrom(inbuffer, ref remote);
                            //total = recv + partialbuffer.Length;
                        }
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
                            if (_client.Client.Available > 0)
                            {
                                _recvBytes += recv = _client.Client.ReceiveFrom(inbuffer, ref remote);
                                total = recv + partialbuffer.Length;
                            }
                            else recv = 0; // Connection attempt timed out. Fall through to retry
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
                            if (_client.Client.Available > 0)
                            {
                                _recvBytes += recv = _client.Client.ReceiveFrom(inbuffer, ref remote);
                                total = recv + partialbuffer.Length;
                            }
                        }
                    }                    
                    if (recv > 0)
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
            if (null != _stream)
            {
                _stream.Dispose();
            }
            if (null != _client)
            {
                _client.Close();
                _client = null;
            }
        }

        #endregion
    }
}
