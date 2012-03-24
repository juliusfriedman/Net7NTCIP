using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Communication.Interfaces
{
    public interface IIPClient : IDisposable
    {

        #region Properties

        /// <summary>
        /// Gets a value indicating whether a default remote host has been established. 
        /// </summary>
        bool Active { get; }

        /// <summary>
        /// Gets the amount of data received from the network that is available to read. 
        /// </summary>
        int Available { get; }

        /// <summary>
        /// Gets the amount of bytes sent with this IPClient
        /// </summary>
        int BytesSent { get; }

        /// <summary>
        /// Gets the amount of bytes recieved with this IPClient
        /// </summary>
        int BytesRecieved { get; }

        #endregion

        #region Methods

        #region [BeginConnect]

        /// <summary>
        /// Begins an asynchronous request for a remote host connection. The remote host is specified by an IPAddress and a port number (Int32).
        /// </summary>
        /// <param name="address">The IPAddress of the remote host.</param>
        /// <param name="port">The port number of the remote host.</param>
        /// <param name="requestCallback">An AsyncCallback delegate that references the method to invoke when the operation is complete.</param>
        /// <param name="state">A user-defined object that contains information about the connect operation. This object is passed to the requestCallback delegate when the operation is complete.</param>
        /// <returns>An IAsyncResult object that references the asynchronous connection.</returns>
        IAsyncResult BeginConnect(IPAddress address, int port, AsyncCallback requestCallback, object state);

        /// <summary>
        /// Begins an asynchronous request for a remote host connection. The remote host is specified by an IPAddress array and a port number (Int32).
        /// </summary>
        /// <param name="addresses">At least one IPAddress that designates the remote hosts.</param>
        /// <param name="port">The port number of the remote hosts.</param>
        /// <param name="requestCallback">An AsyncCallback delegate that references the method to invoke when the operation is complete.</param>
        /// <param name="state"></param>
        /// <returns>A user-defined object that contains information about the connect operation. This object is passed to the requestCallback delegate when the operation is complete.</returns>
        IAsyncResult BeginConnect(IPAddress[] addresses, int port, AsyncCallback requestCallback, object state);

        /// <summary>
        /// Begins an asynchronous request for a remote host connection. The remote host is specified by a host name (String) and a port number (Int32).
        /// </summary>
        /// <param name="host">The name of the remote host.</param>
        /// <param name="port">The port number of the remote host.</param>
        /// <param name="requestCallback">An AsyncCallback delegate that references the method to invoke when the operation is complete.</param>
        /// <param name="state">A user-defined object that contains information about the connect operation. This object is passed to the requestCallback delegate when the operation is complete.</param>
        /// <returns>An IAsyncResult object that references the asynchronous connection.</returns>
        IAsyncResult BeginConnect(string hostname, int port, AsyncCallback requestCallback, object state);

        /// <summary>
        /// Asynchronously accepts an incoming connection attempt.
        /// </summary>
        /// <param name="asyncResult">An IAsyncResult object returned by a call to BeginConnect.</param>
        void EndConnect(IAsyncResult asyncResult);

        #endregion

        #region [Connect]

        /// <summary>
        /// Establishes a default remote host using the specified network endpoint. 
        /// </summary>
        /// <param name="remote">The remote host to connect to</param>
        void Connect(IPEndPoint remote);

        /// <summary>
        /// Establishes a default remote host using the specified IP address and port number. 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        void Connect(IPAddress ipAddress, int port);

        /// <summary>
        /// Establishes a default remote host using the specified host name and port number. 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        void Connect(string host, int port);

        #endregion

        #region ConnectAsync

        /// <summary>
        /// Begins an asynchronous request for a remote host connection.
        /// </summary>
        /// <param name="e">The System.Net.Sockets.SocketAsyncEventArgs object to use for this asynchronous socket operation.</param>
        void ConnectAsync(SocketAsyncEventArgs e);

        /// <summary>
        /// Begins an asynchronous request for a remote host connection.
        /// </summary>
        /// <param name="socketType">One of the SocketType values.</param>
        /// <param name="protocolType">One of the ProtocolType values.</param>
        /// <param name="e">The System.Net.Sockets.SocketAsyncEventArgs object to use for this asynchronous socket operation.</param>
        void ConnectAsync(SocketType socketType, ProtocolType protocolType, SocketAsyncEventArgs e);

        #endregion

        #region [BeginDisconnect]

        /// <summary>
        /// Begins an asynchronous request to disconnect from a remote endpoint.
        /// </summary>
        /// <param name="reuseSocket">true if this socket can be reused after the connection is closed; otherwise, false.</param>
        /// <param name="callback">The AsyncCallback delegate.</param>
        /// <param name="state">An object that contains state information for this request.</param>
        /// <returns>An IAsyncResult object that references the asynchronous operation.</returns>
        IAsyncResult BeginDisconnect(bool reuseSocket, AsyncCallback callback, Object state);

        #endregion

        #region [DisconnectAsync]

        /// <summary>
        /// Begins an asynchronous request to disconnect from a remote endpoint.
        /// </summary>
        /// <param name="e">The System.Net.Sockets.SocketAsyncEventArgs object to use for this asynchronous socket operation.</param>
        /// <returns>
        /// Returns true if the I/O operation is pending. The SocketAsyncEventArgs.Completed event on the e parameter will be raised upon completion of the operation. 
        /// Returns false if the I/O operation completed synchronously. In this case, The SocketAsyncEventArgs.Completed event on the e parameter will not be raised and the e object passed as a parameter may be examined immediately after the method call returns to retrieve the result of the operation.
        /// </returns>
        bool DisconnectAsync(SocketAsyncEventArgs e);

        #endregion

        #region Disconnect

        /// <summary>
        /// Closes the socket connection and allows reuse of the socket.
        /// </summary>
        /// <param name="reuseSocket">true if this socket can be reused after the current connection is closed; otherwise, false.</param>
        void Disconnect(bool reuseSocket);

        #endregion

        #region [Recieve]

        /// <summary>
        /// Receives a datagram from a remote host asynchronously.
        /// </summary>
        /// <param name="requestCallback">An AsyncCallback delegate that references the method to invoke when the operation is complete.</param>
        /// <param name="state">A user-defined object that contains information about the receive operation. This object is passed to the requestCallback delegate when the operation is complete</param>
        /// <returns>An IAsyncResult object that references the asynchronous receive.</returns>
        IAsyncResult BeginReceive(AsyncCallback requestCallback, Object state);

        /// <summary>
        /// Ends a pending asynchronous receive.
        /// </summary>
        /// <param name="asyncResult">An IAsyncResult object returned by a call to BeginReceive.</param>
        /// <param name="remoteEP">The specified remote endpoint.</param>
        /// <returns>If successful, the number of bytes received. If unsuccessful, this method returns 0.</returns>
        byte[] EndRecieve(IAsyncResult asyncResult, ref IPEndPoint remoteEP);

        /// <summary>
        /// Ends a pending asynchronous receive.
        /// </summary>
        /// <param name="remoteEP">The specified remote endpoint.</param>
        /// <returns>If successful, the number of bytes received. If unsuccessful, this method returns 0.</returns>
        byte[] Recieve(ref IPEndPoint remoteEP);

        /// <summary>
        /// Begins an asynchronous request to receive data from a connected Socket object.
        /// </summary>
        /// <param name="e">The System.Net.Sockets.SocketAsyncEventArgs object to use for this asynchronous socket operation</param>
        /// <returns>
        /// Returns true if the I/O operation is pending. The SocketAsyncEventArgs.Completed event on the e parameter will be raised upon completion of the operation. 
        /// Returns false if the I/O operation completed synchronously. In this case, The SocketAsyncEventArgs.Completed event on the e parameter will not be raised and the e object passed as a parameter may be examined immediately after the method call returns to retrieve the result of the operation.
        /// </returns>
        bool RecieveAsync(SocketAsyncEventArgs e);

        /// <summary>
        /// Begins to asynchronously receive data from a specified network device.
        /// </summary>
        /// <param name="e">The System.Net.Sockets.SocketAsyncEventArgs object to use for this asynchronous socket operation.</param>
        /// <returns>
        /// Returns true if the I/O operation is pending. The SocketAsyncEventArgs.Completed event on the e parameter will be raised upon completion of the operation. 
        /// Returns false if the I/O operation completed synchronously. In this case, The SocketAsyncEventArgs.Completed event on the e parameter will not be raised and the e object passed as a parameter may be examined immediately after the method call returns to retrieve the result of the operation.
        /// </returns>
        bool ReceiveFromAsync(SocketAsyncEventArgs e);

        /// <summary>
        /// Receives the specified number of bytes of data into the specified location of the data buffer, using the specified SocketFlags, and stores the endpoint and packet information.
        /// </summary>
        /// <param name="buffer">An array of type Byte that is the storage location for received data.</param>
        /// <param name="offset">The position in the buffer parameter to store the received data.</param>
        /// <param name="size">The number of bytes to receive.</param>
        /// <param name="flags">A bitwise combination of the SocketFlags values.</param>
        /// <param name="remoteEP">An EndPoint, passed by reference, that represents the remote server.</param>
        /// <param name="ipPacketInformation">An IPPacketInformation holding address and interface information.</param>
        /// <returns>The number of bytes received.</returns>
        int RecieveMessageFrom(byte[] buffer, int offset, int size, ref SocketFlags flags, ref EndPoint remoteEP, out IPPacketInformation ipPacketInformation);

        /// <summary>
        /// Begins to asynchronously receive the specified number of bytes of data into the specified location in the data buffer, using the specified SocketAsyncEventArgs.SocketFlags, and stores the endpoint and packet information.
        /// </summary>
        /// <param name="e">The System.Net.Sockets.SocketAsyncEventArgs object to use for this asynchronous socket operation.</param>
        /// <returns>
        /// Returns true if the I/O operation is pending. The SocketAsyncEventArgs.Completed event on the e parameter will be raised upon completion of the operation. 
        /// Returns false if the I/O operation completed synchronously. In this case, The SocketAsyncEventArgs.Completed event on the e parameter will not be raised and the e object passed as a parameter may be examined immediately after the method call returns to retrieve the result of the operation.
        /// </returns>
        bool RecieveMessageFromAsync(SocketAsyncEventArgs e);

        #endregion

        #region [Send]

        /// <summary>
        /// Sends a IP Packet to a remote host
        /// </summary>
        /// <param name="buffer">A Byte array that contains the data to be sent.</param>        
        /// <returns>The number of bytes sent.</returns>
        int Send(byte[] buffer);

        /// <summary>
        /// Sends a IP Packet to a remote host. 
        /// </summary>
        /// <param name="buffer">A Byte array that contains the data to be sent.</param>
        /// <param name="bytes">The number of bytes in the IP Packet. </param>
        /// <returns>The number of bytes sent.</returns>
        int Send(byte[] buffer, int bytes);

        /// <summary>
        /// Sends a IP Packet to a remote host. 
        /// </summary>
        /// <param name="buffer">A Byte array that contains the data to be sent.</param>
        /// <param name="bytes">The number of bytes in the IP Packet. </param>
        /// <param name="flags">A bitwise combination of the SocketFlags values.</param>
        /// <returns>The number of bytes sent.</returns>
        int Send(byte[] buffer, int bytes, SocketFlags flags);

        /// <summary>
        /// Sends a IP Packet to a remote host. 
        /// </summary>
        /// <param name="buffer">A Byte array that contains the data to be sent.</param>
        /// <param name="offset">The position in the data buffer at which to begin sending data. </param>
        /// <param name="size">The number of bytes in the datagram. </param>
        /// <param name="flags">A bitwise combination of the SocketFlags values.</param>
        /// <returns>The number of bytes sent.</returns>
        int Send(byte[] buffer, int offset, int size, SocketFlags flags, out SocketError error);

        #endregion

        #region [SendTo]

        /// <summary>
        /// Sends a IP Packet to a specified port on a specified remote host.
        /// </summary>
        /// <param name="datagram">An array of type Byte that specifies the IP Packet that you intend to send represented as an array of bytes. </param>
        /// <param name="bytes">The number of bytes in the datagram.</param>
        /// <param name="endpoint">The EndPoint that represents the destination for the data.</param>
        /// <returns>The number of bytes sent.</returns>
        int SendTo(byte[] buffer, int bytes, IPEndPoint endPoint);

        /// <summary>
        /// Sends a IP Packet to a specified port on a specified remote host.
        /// </summary>
        /// <param name="datagram">An array of type Byte that specifies the IP Packet that you intend to send represented as an array of bytes. </param>
        /// <param name="bytes">The number of bytes in the datagram.</param>
        /// <param name="flags">A bitwise combination of the SocketFlags values.</param>
        /// <param name="endpoint">The EndPoint that represents the destination for the data.</param>
        /// <returns>The number of bytes sent.</returns>        
        int SendTo(byte[] buffer, int bytes, SocketFlags flags, IPEndPoint endPoint);

        /// <summary>
        /// Sends a IP Packet to a specified port on a specified remote host.
        /// </summary>
        /// <param name="buffer">An array of type Byte that specifies the IP Packet that you intend to send represented as an array of bytes. </param>
        /// <param name="bytes">The number of bytes in the datagram.</param>
        /// <param name="hostname">The name of the remote host to which you intend to send the datagram.</param>
        /// <param name="port">The remote port number with which you intend to communicate.</param>        
        /// <returns>The number of bytes sent.</returns>
        int SendTo(byte[] buffer, int bytes, string hostname, int port);

        /// <summary>
        /// Sends a IP Packet to a specified port on a specified remote host.
        /// </summary>
        /// <param name="buffer">An array of type Byte that specifies the IP Packet that you intend to send represented as an array of bytes. </param>
        /// <param name="bytes">The number of bytes in the datagram.</param>
        /// <param name="flags">A bitwise combination of the SocketFlags values.</param>
        /// <param name="hostname">The name of the remote host to which you intend to send the datagram.</param>
        /// <param name="port">The remote port number with which you intend to communicate.</param>        
        /// <returns>The number of bytes sent.</returns>
        int SendTo(byte[] buffer, int bytes, SocketFlags flags, string hostname, int port);

        #endregion

        #region [BeginSend]

        /// <summary>
        /// Sends a datagram to a remote host asynchronously. The destination was specified previously by a call to Connect.
        /// </summary>
        /// <param name="datagram">A Byte array that contains the data to be sent.</param>
        /// <param name="bytes">The number of bytes to send.</param>
        /// <param name="requestCallback">An AsyncCallback delegate that references the method to invoke when the operation is complete.</param>
        /// <param name="state">A user-defined object that contains information about the send operation. This object is passed to the requestCallback delegate when the operation is complete.</param>
        /// <returns>An IAsyncResult object that references the asynchronous send.</returns>
        IAsyncResult BeginSend(byte[] datagram, int bytes, AsyncCallback requestCallback, object state);

        /// <summary>
        /// Sends a datagram to a destination asynchronously. The destination is specified by a EndPoint.
        /// </summary>
        /// <param name="datagram">A Byte array that contains the data to be sent.</param>
        /// <param name="bytes">The number of bytes to send.</param>
        /// <param name="endPoint">The EndPoint that represents the destination for the data.</param>
        /// <param name="requestCallback">An AsyncCallback delegate that references the method to invoke when the operation is complete.</param>
        /// <param name="state">A user-defined object that contains information about the send operation. This object is passed to the requestCallback delegate when the operation is complete.</param>
        /// <returns>An IAsyncResult object that references the asynchronous send.</returns>
        IAsyncResult BeginSend(byte[] datagram, int bytes, IPEndPoint endPoint, AsyncCallback requestCallback, object state);

        /// <summary>
        /// Sends a datagram to a destination asynchronously. The destination is specified by the host name and port number.
        /// </summary>
        /// <param name="datagram">A Byte array that contains the data to be sent.</param>
        /// <param name="bytes">The number of bytes to send.</param>
        /// <param name="host">The destination host.</param>
        /// <param name="port">The destination port number.</param>
        /// <param name="requestCallback">An AsyncCallback delegate that references the method to invoke when the operation is complete.</param>
        /// <param name="state">A user-defined object that contains information about the send operation. This object is passed to the requestCallback delegate when the operation is complete.</param>
        /// <returns>An IAsyncResult object that references the asynchronous send.</returns>
        IAsyncResult BeginSend(byte[] datagram, int bytes, string host, int port, AsyncCallback requestCallback, object state);

        /// <summary>
        /// Ends a pending asynchronous send.
        /// </summary>
        /// <param name="asyncResult">An IAsyncResult object returned by a call to BeginSend.</param>
        /// <returns>If successful, the number of bytes sent to the UdpClient.</returns>
        int EndSend(IAsyncResult asyncResult);

        #endregion

        #region [SendAsync]

        /// <summary>
        /// Sends data asynchronously to a connected Socket object.
        /// </summary>
        /// <param name="e">The System.Net.Sockets.SocketAsyncEventArgs object to use for this asynchronous socket operation.</param>
        /// <returns>
        /// Returns true if the I/O operation is pending. The SocketAsyncEventArgs.Completed event on the e parameter will be raised upon completion of the operation. 
        /// Returns false if the I/O operation completed synchronously. In this case, The SocketAsyncEventArgs.Completed event on the e parameter will not be raised and the e object passed as a parameter may be examined immediately after the method call returns to retrieve the result of the operation.
        /// </returns>
        bool SendAsync(SocketAsyncEventArgs e);

        #endregion

        /// <summary>
        /// Set the IP protection level on a socket.
        /// </summary>
        /// <param name="level">The IP protection level to set on this socket</param>
        /// <remarks>
        /// The SetIPProtectionLevel method enables restricting an a IPv6 or IP socket to listen on a specified scope, such as addresses with the same link local or site local prefix. This socket option enables applications to place access restrictions on IPv6 or IP sockets. Such restrictions enable an application running on a private LAN to simply and robustly harden itself against external attacks. This socket option can also be used to remove access restrictions if the level parameter is set to Unrestricted. This socket option widens or narrows the scope of a listening socket, enabling unrestricted access from public and private users when appropriate, or restricting access only to the same site, as required. 
        /// This socket option has defined protection levels specified in the IPProtectionLevel enumeration.
        /// The SetIPProtectionLevel method is used to enable or disable Network Address Traversal (NAT) for a Socket instance. NAT traversal may be provided using Teredo, 6to4, or an ISATAP tunnel. 
        /// When the level parameter is set to EdgeRestricted, or Restricted, this explicitly disables NAT traversal for a Socket instance. 
        /// When the level parameter is set to EdgeRestricted, this may allow NAT traversal for a Socket depending on firewall rules in place on the system. 
        /// </remarks>
        void SetIPProtectionLevel(IPProtectionLevel level);

        /// <summary>
        /// Closes the UDP connection. 
        /// </summary>
        void Close();

        /// <summary>
        /// Disables sends and receives on a Socket.
        /// </summary>
        /// <param name="how">One of the SocketShutdown values that specifies the operation that will no longer be allowed.</param>
        void Shutdown(SocketShutdown how);

        #endregion

    }
}
