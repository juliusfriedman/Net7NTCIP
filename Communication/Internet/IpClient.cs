using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Communication.Interfaces;

namespace Communication.Internet
{
    public class IpClient
    {

        #region Fields        

        /// <summary>
        /// The underlying Socket used by the IPClient
        /// </summary>
        protected Socket m_ClientSocket;

        ///<summary>
        /// The AllowNatTraversal method is used to enable or disable NAT traversal for a UdpClient instance. NAT traversal may be provided using Teredo, 6to4, or an ISATAP tunnel. 
        /// When the allowed parameter is false, the IPProtectionLevel option on the associated socket is set to EdgeRestricted. This explicitly disables NAT traversal for a UdpClient instance. 
        /// When the allowed parameter is true, the IPProtectionLevel option on the associated socket is set to Unrestricted. This may allow NAT traversal for a UdpClient depending on firewall rules in place on the system. 
        /// A Teredo address is an IPv6 address with the prefix of 2001::/32. Teredo addresses can be returned through normal DNS name resolution or enumerated as an IPv6 address assigned to a local interface.
        ///</summary>
        protected bool m_allowedNatTraversal,

        ///<summary>
        /// Determines if the IPClient is Broadcast capable
        ///</summary>
        m_IsBroadcast;

        /// <summary>
        /// The amount of bytes this IPClient has sent on it's client socket
        /// </summary>
        protected int m_BytesSent,

        /// <summary>
        /// The amount of bytes this IPClient has recieved on it's client socket
        /// </summary>
        m_BytesRecieved;

        /// <summary>
        /// The NetworkStream relevant to TcpConnections
        /// </summary>
        protected NetworkStream m_Stream;

        #endregion

        #region Constructor

        public IpClient(SocketInformation socketInformation) 
        { 
            m_ClientSocket = new Socket(socketInformation); 
        }

        public IpClient(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        { 
            m_ClientSocket = new Socket(addressFamily, socketType, protocolType); 
        }

        #endregion  

        #region Properties

        /// <summary>
        /// Gets the underlying network Socket. 
        /// </summary>
        public Socket Client
        {
            get { return m_ClientSocket; }
        }

        public bool NoDelay
        {
            get
            {
                return !(((int)m_ClientSocket.GetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug)) == 0);
            }
            set
            {
                m_ClientSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug, value ? 1 : 0);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if this IP Client is agnostic to IPV6
        /// </summary>
        public bool DualMode
        {
            get { return (bool)m_ClientSocket.GetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only); }
            protected set
            {
                if (m_ClientSocket.AddressFamily == AddressFamily.InterNetworkV6 && value != DualMode)
                    m_ClientSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, value);
            }
        }

        /// <summary>
        /// The AllowNatTraversal method is used to enable or disable NAT traversal for a UdpClient instance. NAT traversal may be provided using Teredo, 6to4, or an ISATAP tunnel. 
        /// When the allowed parameter is false, the IPProtectionLevel option on the associated socket is set to EdgeRestricted. This explicitly disables NAT traversal for a UdpClient instance. 
        /// When the allowed parameter is true, the IPProtectionLevel option on the associated socket is set to Unrestricted. This may allow NAT traversal for a UdpClient depending on firewall rules in place on the system. 
        /// A Teredo address is an IPv6 address with the prefix of 2001::/32. Teredo addresses can be returned through normal DNS name resolution or enumerated as an IPv6 address assigned to a local interface.
        /// </summary>
        public bool AllowedNatTraversal
        {
            get { return m_allowedNatTraversal; }
            protected set { AllowNatTraversal(value); }
        }
        
        /// <summary>
        /// Indicates if the underlying Socket is connected
        /// </summary>
        public bool Connected
        {
            get { return m_ClientSocket.Connected; }
        }

        /// <summary>
        /// The amount of bytes ready to be recieved at the Client Socket
        /// </summary>
        public int Available
        {
            get { return m_ClientSocket.Available; }
        }

        /// <summary>
        /// The endpoint from which this IpClient established his details
        /// </summary>
        public EndPoint LocalEndPoint
        {
            get { return m_ClientSocket.LocalEndPoint; }
        }

        /// <summary>
        /// The endpoint to which this IpClient is connected
        /// </summary>
        public EndPoint RemoteEndPoint
        {
            get { return m_ClientSocket.RemoteEndPoint; }
        }

        #endregion

        #region Methods

        #region Connect

        public void Connect(EndPoint remoteEP) { m_ClientSocket.Connect(remoteEP); }

        public void Connect(IPAddress[] addresses, int port) { m_ClientSocket.Connect(addresses, port); }

        public void Connect(IPAddress address, int port) { m_ClientSocket.Connect(address, port); }

        public void Connect(string host, int port) { m_ClientSocket.Connect(host, port); }

        public bool ConnectAsync(SocketAsyncEventArgs e) 
        { 
            e.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEventArg_Completed);
            if (!m_ClientSocket.ConnectAsync(e))
            {
                e.Completed -= SocketEventArg_Completed;
                SocketEventArg_Completed(this, e);
                return false;
            }
            else return true;
        }

        public IAsyncResult BeginConnect(EndPoint remoteEP, AsyncCallback callback, object state)
        {
            return m_ClientSocket.BeginConnect(remoteEP, callback, state);
        }

        public IAsyncResult BeginConnect(IPAddress address, int port, AsyncCallback requestCallback, object state)
        {
            return m_ClientSocket.BeginConnect(address, port, requestCallback, state);
        }

        public IAsyncResult BeginConnect(IPAddress[] addresses, int port, AsyncCallback requestCallback, object state)
        {
            return m_ClientSocket.BeginConnect(addresses, port, requestCallback, state);
        }

        public IAsyncResult BeginConnect(string host, int port, AsyncCallback requestCallback, object state)
        {
            return m_ClientSocket.BeginConnect(host, port, requestCallback, state);
        }

        public void EndConnect(IAsyncResult asyncResult)
        {
            m_ClientSocket.EndConnect(asyncResult);
        }

        #endregion

        #region Disconnect

        public void Disconnect(bool reuseSocket)
        {
            m_ClientSocket.Disconnect(reuseSocket);            
        }

        public bool DisconnectAsync(SocketAsyncEventArgs e)
        {
            e.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEventArg_Completed);
            if (!m_ClientSocket.DisconnectAsync(e))
            {
                e.Completed -= new EventHandler<SocketAsyncEventArgs>(SocketEventArg_Completed);
                SocketEventArg_Completed(this, e);
                return false;
            }
            else return true;
        }

        public IAsyncResult BeginDisconnect(bool reuseSocket, AsyncCallback callback, object state)
        {
            return m_ClientSocket.BeginDisconnect(reuseSocket, callback, state);
        }

        public void EndDisconnect(IAsyncResult asyncResult)
        {
            m_ClientSocket.EndDisconnect(asyncResult);
        }

        #endregion

        #region Accept

        public Socket Accept()
        {
            return m_ClientSocket.Accept();
        }

        public bool AcceptAsync(SocketAsyncEventArgs e)
        {
            e.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEventArg_Completed);
            if (!m_ClientSocket.AcceptAsync(e))
            {
                e.Completed -= SocketEventArg_Completed;
                SocketEventArg_Completed(this, e);
                return false;
            }
            return true;
        }

        public IAsyncResult BeginAccept(AsyncCallback callback, object state)
        {
            return m_ClientSocket.BeginAccept(callback, state);
        }

        public IAsyncResult BeginAccept(int receiveSize, AsyncCallback callback, object state)
        {
            return BeginAccept(null, receiveSize, callback, state);
        }

        public IAsyncResult BeginAccept(Socket acceptSocket, int receiveSize, AsyncCallback callback, object state)
        {
            return m_ClientSocket.BeginAccept(acceptSocket, receiveSize, callback, state);
        }

        public Socket EndAccept(IAsyncResult asyncResult)
        {
            //AcceptedSocket class with parent?
            return m_ClientSocket.EndAccept(asyncResult);
        }

        public Socket EndAccept(out byte[] buffer, IAsyncResult asyncResult)
        {
            int recieved;
            byte[] buffer2;
            Socket socket = EndAccept(out buffer2, out recieved, asyncResult);
            buffer = new byte[recieved];
            Array.Copy(buffer2, buffer, recieved);
            m_BytesRecieved += recieved;
            return socket;
        }

        public Socket EndAccept(out byte[] buffer, out int bytesTransferred, IAsyncResult asyncResult)
        {
            return m_ClientSocket.EndAccept(out buffer, out bytesTransferred, asyncResult);
        }

        #endregion

        #region Send

        /// <summary>
        /// Send a packet Asyncronusly
        /// </summary>
        /// <param name="e">The event args</param>
        /// <returns>True if the I/O Operation is pending otherwise false.</returns>
        public bool SendAsync(SocketAsyncEventArgs e)
        {
            e.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEventArg_Completed);
            if(!m_ClientSocket.SendAsync(e))
            {
                e.Completed -= SocketEventArg_Completed;
                SocketEventArg_Completed(this, e);
                return false;
            }else return true;
        }

        public int Send(byte[] buffer)
        {
            int sent = m_ClientSocket.Send(buffer);
            m_BytesSent += sent;
            return sent;
        }

        public int Send(byte[] buffer, SocketFlags socketFlags)
        {
            int sent = m_ClientSocket.Send(buffer, socketFlags);
            m_BytesSent += sent;
            return sent;
        }

        public int Send(byte[] buffer, int size, SocketFlags socketFlags)
        {
            int sent = m_ClientSocket.Send(buffer, 0, size, socketFlags);
            m_BytesSent += sent;
            return sent;
        }

        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            SocketError error;
            int sent = m_ClientSocket.Send(buffer, offset, size, socketFlags, out error);
            if (error != SocketError.Success)
            {
                throw new SocketException((int)error);
            }
            m_BytesSent += sent;
            return sent;
        }

        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode)
        {
            int sent = m_ClientSocket.Send(buffer, offset, size, socketFlags, out errorCode);
            m_BytesSent += sent;
            return sent;
        }

        public int SendTo(byte[] buffer, EndPoint remoteEP)
        {
            int sent = m_ClientSocket.SendTo(buffer, 0, (buffer != null) ? buffer.Length : 0, SocketFlags.None, remoteEP);
            m_BytesSent += sent;
            return sent;
        }

        public int SendTo(byte[] buffer, SocketFlags socketFlags, EndPoint remoteEP)
        {
            int sent = m_ClientSocket.SendTo(buffer, 0, (buffer != null) ? buffer.Length : 0, socketFlags, remoteEP);
            m_BytesSent += sent;
            return sent;
        }

        public int SendTo(byte[] buffer, int size, SocketFlags socketFlags, EndPoint remoteEP)
        {
            int sent = m_ClientSocket.SendTo(buffer, 0, size, socketFlags, remoteEP);
            m_BytesSent += sent;
            return sent;
        }

        public int SendTo(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP)
        {
            int sent = m_ClientSocket.SendTo(buffer, offset, size, socketFlags, remoteEP);
            m_BytesRecieved += sent;
            return sent;
        }

        public bool SendToAsync(SocketAsyncEventArgs e)
        {
            e.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEventArg_Completed);
            if (!m_ClientSocket.SendToAsync(e))
            {
                e.Completed -= SocketEventArg_Completed;
                SocketEventArg_Completed(this, e);
                return false;
            }
            else return true;
        }

        public bool SendPacketsAsync(SocketAsyncEventArgs e)
        {
            e.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEventArg_Completed);
            if (!m_ClientSocket.SendPacketsAsync(e))
            {
                e.Completed -= SocketEventArg_Completed;
                SocketEventArg_Completed(this, e);
                return false;
            }
            else return true;
        }

        public IAsyncResult BeginSend(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            SocketError error;
            IAsyncResult result = m_ClientSocket.BeginSend(buffer, offset, size, socketFlags, out error, callback, state);
            if ((error != SocketError.Success) && (error != SocketError.IOPending))
            {
                throw new SocketException((int)error);
            }
            return result;
        }

        public IAsyncResult BeginSend(byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
        {
            return m_ClientSocket.BeginSend(buffer, offset, size, socketFlags, out errorCode, callback, state);
        }

        //Right now does not increment send / recieve counters
        public IAsyncResult BeginSendFile(string fileName, AsyncCallback callback, object state)
        {
            return m_ClientSocket.BeginSendFile(fileName, callback, state);
        }

        //Right now does not increment send / recieve counters
        public IAsyncResult BeginSendFile(string fileName, byte[] preBuffer, byte[] postBuffer, TransmitFileOptions flags, AsyncCallback callback, object state)
        {
            return m_ClientSocket.BeginSendFile(fileName, preBuffer, postBuffer, flags, callback, state);
        }

        public IAsyncResult BeginSendTo(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP, AsyncCallback callback, object state)
        {
            return m_ClientSocket.BeginSendTo(buffer, offset, size, socketFlags, remoteEP, callback, state);
        }

        public int EndSend(IAsyncResult asyncResult)
        {
            SocketError error;
            int sent = m_ClientSocket.EndSend(asyncResult, out error);
            if (error != SocketError.Success)
            {
                throw new SocketException((int)error);
            }
            m_BytesSent += sent;
            return sent;
        }

        public int EndSend(IAsyncResult asyncResult, out SocketError errorCode)
        {
            int sent = m_ClientSocket.EndSend(asyncResult, out errorCode);
            m_BytesSent += sent;
            return sent;
        }

        public void EndSendFile(IAsyncResult asyncResult)
        {
            m_ClientSocket.EndSendFile(asyncResult);
        }

        public int EndSendTo(IAsyncResult asyncResult)
        {
            int sent = m_ClientSocket.EndSendTo(asyncResult);
            m_BytesSent += sent;
            return sent;
        }

        #endregion

        #region Recieve

        public bool ReceiveAsync(SocketAsyncEventArgs e)
        {
            e.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEventArg_Completed);
            if (!m_ClientSocket.ReceiveAsync(e))
            {
                e.Completed -= SocketEventArg_Completed;
                SocketEventArg_Completed(this, e);
                return false;
            }
            else return true;
        }

        public int Receive(byte[] buffer)
        {
            int recieved = m_ClientSocket.Receive(buffer, 0, (buffer != null) ? buffer.Length : 0, SocketFlags.None);
            m_BytesRecieved += recieved;
            return recieved;
        }

        public int Receive(byte[] buffer, SocketFlags socketFlags)
        {
            int recieved = m_ClientSocket.Receive(buffer, 0, (buffer != null) ? buffer.Length : 0, socketFlags);
            m_BytesRecieved += recieved;
            return recieved;
        }

        public int Receive(byte[] buffer, int size, SocketFlags socketFlags)
        {
            int recieved = m_ClientSocket.Receive(buffer, 0, size, socketFlags);
            m_BytesRecieved += recieved;
            return recieved;
        }

        public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            SocketError error;
            int recieved = m_ClientSocket.Receive(buffer, offset, size, socketFlags, out error);
            if (error != SocketError.Success)
            {
                throw new SocketException((int)error);
            }
            m_BytesRecieved += recieved;
            return recieved;
        }

        public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode)
        {
            int recieved = m_ClientSocket.Receive(buffer, offset, size, socketFlags, out errorCode);
            m_BytesRecieved += recieved;
            return recieved;
        }

        public int ReceiveFrom(byte[] buffer, ref EndPoint remoteEP)
        {
            int recieved = m_ClientSocket.ReceiveFrom(buffer, 0, (buffer != null) ? buffer.Length : 0, SocketFlags.None, ref remoteEP);
            m_BytesRecieved += recieved;
            return recieved;
        }

        public int ReceiveFrom(byte[] buffer, SocketFlags socketFlags, ref EndPoint remoteEP)
        {
            int recieved = m_ClientSocket.ReceiveFrom(buffer, 0, (buffer != null) ? buffer.Length : 0, socketFlags, ref remoteEP);
            m_BytesRecieved += recieved;
            return recieved;
        }

        public int ReceiveFrom(byte[] buffer, int size, SocketFlags socketFlags, ref EndPoint remoteEP)
        {
            int recieved = m_ClientSocket.ReceiveFrom(buffer, 0, size, socketFlags, ref remoteEP);
            m_BytesRecieved += recieved;
            return recieved;
        }

        public int ReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP)
        {
            int recieved = m_ClientSocket.ReceiveFrom(buffer, offset, size, socketFlags, ref remoteEP);
            m_BytesRecieved += recieved;
            return recieved;
        }

        public bool ReceiveFromAsync(SocketAsyncEventArgs e)
        {
            e.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEventArg_Completed);
            if (!m_ClientSocket.ReceiveFromAsync(e))
            {
                e.Completed -= SocketEventArg_Completed;
                SocketEventArg_Completed(this, e);
                return false;
            }
            return true;
        }

        public int ReceiveMessageFrom(byte[] buffer, int offset, int size, ref SocketFlags socketFlags, ref EndPoint remoteEP, out IPPacketInformation ipPacketInformation)
        {
            int recieved = m_ClientSocket.ReceiveMessageFrom(buffer, offset, size, ref socketFlags, ref remoteEP, out ipPacketInformation);
            m_BytesRecieved += recieved;
            return recieved;
        }

        public bool ReceiveMessageFromAsync(SocketAsyncEventArgs e)
        {
            e.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEventArg_Completed);
            if (!m_ClientSocket.ReceiveMessageFromAsync(e))
            {
                e.Completed -= SocketEventArg_Completed;
                SocketEventArg_Completed(this, e);
                return false;
            }
            return true;
        }

        public IAsyncResult BeginReceive(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            SocketError error;
            IAsyncResult result = m_ClientSocket.BeginReceive(buffer, offset, size, socketFlags, out error, callback, state);
            if ((error != SocketError.Success) && (error != SocketError.IOPending))
            {
                throw new SocketException((int)error);
            }
            return result;
        }
        
        public IAsyncResult BeginReceive(byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
        {
            return m_ClientSocket.BeginReceive(buffer, offset, size, socketFlags, out errorCode, callback, state);
        }
        
        public IAsyncResult BeginReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP, AsyncCallback callback, object state)
        {
            return m_ClientSocket.BeginReceiveFrom(buffer, offset, size, socketFlags, ref remoteEP, callback, state);
        }

        public IAsyncResult BeginReceiveMessageFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP, AsyncCallback callback, object state)
        {
            return m_ClientSocket.BeginReceiveMessageFrom(buffer, offset, size, socketFlags, ref remoteEP, callback, state);
        }

        public int EndReceive(IAsyncResult asyncResult)
        {
            SocketError error;
            int recieved = this.EndReceive(asyncResult, out error);
            if (error != SocketError.Success)
            {
                throw new SocketException((int)error);
            }
            m_BytesRecieved += recieved;
            return recieved;
        }

        public int EndReceive(IAsyncResult asyncResult, out SocketError errorCode)
        {
            int recieved = m_ClientSocket.EndReceive(asyncResult, out errorCode);
            m_BytesRecieved += recieved;
            return recieved;
        }

        public int EndReceiveFrom(IAsyncResult asyncResult, ref EndPoint endPoint)
        {
            int recieved = m_ClientSocket.EndReceiveFrom(asyncResult, ref endPoint);
            m_BytesRecieved += recieved;
            return recieved;
        }

        public int EndReceiveMessageFrom(IAsyncResult asyncResult, ref SocketFlags socketFlags, ref EndPoint endPoint, out IPPacketInformation ipPacketInformation)
        {
            int recieved = m_ClientSocket.EndReceiveMessageFrom(asyncResult, ref socketFlags, ref endPoint, out ipPacketInformation);
            m_BytesRecieved += recieved;
            return recieved;
        }

        #endregion

        #region Udp

        private void CheckForBroadcast(IPAddress ipAddress)
        {
            if (((this.m_ClientSocket != null) && !this.m_IsBroadcast) && ipAddress.IsBroadcast())
            {
                this.m_IsBroadcast = true;
                this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            }
        }

        public void DropMulticastGroup(IPAddress multicastAddr)
        {
            //if (this.m_CleanedUp)
            //{
            //    throw new ObjectDisposedException(base.GetType().FullName);
            //}
            if (multicastAddr == null)
            {
                throw new ArgumentNullException("multicastAddr");
            }
            if (multicastAddr.AddressFamily != m_ClientSocket.AddressFamily)
            {
                throw new ArgumentException("net_protocol_UDP invalid_multicast_family", "multicastAddr");
            }
            if (m_ClientSocket.AddressFamily == AddressFamily.InterNetwork)
            {
                MulticastOption optionValue = new MulticastOption(multicastAddr);
                this.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, optionValue);
            }
            else
            {
                IPv6MulticastOption option2 = new IPv6MulticastOption(multicastAddr);
                this.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.DropMembership, option2);
            }
        }

        public void DropMulticastGroup(IPAddress multicastAddr, int ifindex)
        {
            //if (this.m_CleanedUp)
            //{
            //    throw new ObjectDisposedException(base.GetType().FullName);
            //}
            if (multicastAddr == null)
            {
                throw new ArgumentNullException("multicastAddr");
            }
            if (ifindex < 0)
            {
                throw new ArgumentException("net_value_cannot_be_negative", "ifindex");
            }
            if (m_ClientSocket.AddressFamily != AddressFamily.InterNetworkV6)
            {
                throw new SocketException((int)SocketError.OperationNotSupported);
            }            
            m_ClientSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.DropMembership, new IPv6MulticastOption(multicastAddr, (long)ifindex));
        }

        public void JoinMulticastGroup(IPAddress multicastAddr)
        {
            //if (this.m_CleanedUp)
            //{
            //    throw new ObjectDisposedException(base.GetType().FullName);
            //}
            if (multicastAddr == null)
            {
                throw new ArgumentNullException("multicastAddr");
            }
            if (multicastAddr.AddressFamily != m_ClientSocket.AddressFamily)
            {
                throw new ArgumentException("net_protocol_invalid_UDP_multicast_family", "multicastAddr");
            }
            if (m_ClientSocket.AddressFamily == AddressFamily.InterNetwork)
            {                
                m_ClientSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(multicastAddr));
            }
            else
            {                
                m_ClientSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, new IPv6MulticastOption(multicastAddr));
            }
        }

        public void JoinMulticastGroup(int ifindex, IPAddress multicastAddr)
        {
            //if (this.m_CleanedUp)
            //{
            //    throw new ObjectDisposedException(base.GetType().FullName);
            //}
            if (multicastAddr == null)
            {
                throw new ArgumentNullException("multicastAddr");
            }
            if (ifindex < 0)
            {
                throw new ArgumentException("net_value_cannot_be_negative", "ifindex");
            }
            if (m_ClientSocket.AddressFamily != AddressFamily.InterNetworkV6)
            {
                throw new SocketException((int)SocketError.OperationNotSupported);
            }            
            this.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, new IPv6MulticastOption(multicastAddr, (long)ifindex));
        }

        public void JoinMulticastGroup(IPAddress multicastAddr, int timeToLive)
        {
            //if (this.m_CleanedUp)
            //{
            //    throw new ObjectDisposedException(base.GetType().FullName);
            //}
            if (multicastAddr == null)
            {
                throw new ArgumentNullException("multicastAddr");
            }
            //Validate Ttl Live value
            if (!(timeToLive >= 0xff) && (timeToLive <= 0))
            {
                throw new ArgumentOutOfRangeException("timeToLive");
            }
            this.JoinMulticastGroup(multicastAddr);
            this.Client.SetSocketOption((m_ClientSocket.AddressFamily == AddressFamily.InterNetwork) ? SocketOptionLevel.IP : SocketOptionLevel.IPv6, SocketOptionName.MulticastTimeToLive, timeToLive);
        }

        public void JoinMulticastGroup(IPAddress multicastAddr, IPAddress localAddress)
        {
            //if (this.m_CleanedUp)
            //{
            //    throw new ObjectDisposedException(base.GetType().FullName);
            //}
            if (m_ClientSocket.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new SocketException((int)SocketError.OperationNotSupported);
            }
            MulticastOption optionValue = new MulticastOption(multicastAddr, localAddress);
            this.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, optionValue);
        }

        #endregion

        #region Tcp

        public NetworkStream GetStream()
        {
            if (m_ClientSocket.Connected && m_Stream == null) m_Stream = new NetworkStream(m_ClientSocket);
            return m_Stream;
        }

        #endregion   

        #region Event Handlers

        // A single callback is used for all socket operations. 
        // This method forwards execution on to the correct handler 
        // based on the type of completed operation
        void SocketEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    ProcessAccept(e);
                    break;
                case SocketAsyncOperation.Connect:
                    ProcessConnect(e);
                    break;
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                case SocketAsyncOperation.Disconnect:
                    ProcessDisconnect(e);
                    break;
                default:
                    throw new Exception("Invalid operation completed");
            }
        }

        protected virtual void ProcessAccept(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                //AcceptedSocket Class with parent Member?
            }
            else throw new SocketException((int)e.SocketError);
        }

        protected virtual void ProcessConnect(SocketAsyncEventArgs e)
        {            
            if (e.SocketError == SocketError.Success)
            {
                //We are connected now
            }
            else throw new SocketException((int)e.SocketError);
        }

        protected virtual void ProcessDisconnect(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                //We are disconnected now
            }
            else throw new SocketException((int)e.SocketError);
        }

        protected virtual void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                this.m_BytesSent += e.BytesTransferred;
            }
            else throw new SocketException((int)e.SocketError);            
        }

        protected virtual void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                this.m_BytesRecieved += e.BytesTransferred;
            }
            else throw new SocketException((int)e.SocketError);            
        }

        #endregion

        ///<summary>
        /// The AllowNatTraversal method is used to enable or disable NAT traversal for a IpClient instance. NAT traversal may be provided using Teredo, 6to4, or an ISATAP tunnel. 
        /// When the allowed parameter is false, the IPProtectionLevel option on the associated socket is set to EdgeRestricted. This explicitly disables NAT traversal for a UdpClient instance. 
        /// When the allowed parameter is true, the IPProtectionLevel option on the associated socket is set to Unrestricted. This may allow NAT traversal for a UdpClient depending on firewall rules in place on the system. 
        /// A Teredo address is an IPv6 address with the prefix of 2001::/32. Teredo addresses can be returned through normal DNS name resolution or enumerated as an IPv6 address assigned to a local interface.
        ///</summary>
        /// <param name="allowed">A Boolean value that specifies whether to enable or disable NAT traversal.</param>
        public void AllowNatTraversal(bool allowed)
        {
            if (m_allowedNatTraversal = allowed) m_ClientSocket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
            m_ClientSocket.SetIPProtectionLevel(IPProtectionLevel.EdgeRestricted);            
        }

        #endregion

    }
}
