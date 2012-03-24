using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace ASTITransportation.Modbus
{

    public class ModbusMessenger
    {

        #region Enums

        public enum TransportEncoding
        {
            Binary,
            ASCII,
            IpWithCrc,
            IpWithOutCrc
        }

        #endregion

        #region Fields

        TransportEncoding transportEncoding = TransportEncoding.IpWithCrc;

        ProtocolType transportProtocol;

        IPEndPoint ipEndPoint;

        int sendRecieveTimeout = 15000;
        int maxRetries = 5;

        TcpClient tcpClient;
        NetworkStream netStream;
        int tcpSent;
        int tcpRcvd;
        
        UdpClient udpClient;
        int udpSent;
        int udpRcvd;

        short currentTransactionId;

        byte unitId = 0x00;

        List<ModbusMessage> requestLog = new List<ModbusMessage>();
        List<ModbusMessage> recieveLog = new List<ModbusMessage>();

        int crcErrors;

        #endregion

        #region Properties

        public ProtocolType TransportProtocol
        {
            get { return transportProtocol; }
            set
            {
                if (value == ProtocolType.Udp)
                {
                    netStream.Close();
                    tcpClient.Close();
                }
                else
                {
                    udpClient.Close();
                }
                transportProtocol = value;
                System.Threading.Thread.Sleep(0);
                Initialize();
            }
        }

        public Socket Socket
        {
            get { if (transportProtocol == ProtocolType.Tcp) return tcpClient.Client; return udpClient.Client; }
        }

        public TransportEncoding TransportEndoing
        {
            get { return transportEncoding; }
            set { transportEncoding = value; }
        }

        Int16 CurrentTransactionId
        {
            get { return currentTransactionId; }
        }

        Int16 NextTransactionId
        {
            get { return ++currentTransactionId; }
        }

        Int16 LastTransactionId
        {
            get { return ((short)(currentTransactionId - 1)); }
        }

        public int TcpSentBytes
        {
            get { return tcpSent; }
        }

        public int TcpRecievedBytes
        {
            get { return tcpRcvd; }
        }
       
        public int UdpSentBytes
        {
            get { return udpSent; }
        }

        public int UdpRecievedBytes
        {
            get { return udpRcvd; }
        }

        public int TotalBytesSent
        {
            get { return TcpSentBytes + UdpSentBytes; }
        }

        public int TotalBytesRecieved
        {
            get { return TcpRecievedBytes + UdpRecievedBytes; }
        }

        public int SendRecieveTimeout
        {
            get { return sendRecieveTimeout; }
            set { sendRecieveTimeout = value; }
        }

        public int MaximumRetries
        {
            get { return maxRetries; }
            set { maxRetries = value; }
        }

        public int CrcErrors { get { return crcErrors; } }

        public byte UnitId { get { return unitId; } set { unitId = value; } }

        #endregion

        #region Methods

        private void Initialize()
        {
            if (transportProtocol == ProtocolType.Tcp)
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(ipEndPoint);
                netStream = tcpClient.GetStream();
            }
            else
            {
                udpClient = new UdpClient();
                udpClient.Connect(ipEndPoint);
            }
        }

        private ModbusMessage SendMessage(ModbusMessage msg, int retry)
        {

            if (retry > maxRetries)
            {
                throw new Exception("Not Responding");
            }

            if (!Socket.Connected)
            {
                Socket.Connect(ipEndPoint);
            }

            Socket.SendTimeout = Socket.ReceiveTimeout = sendRecieveTimeout;

            requestLog.Add(msg);            

            byte[] outBytes = ToBytes(msg);
            
            if (Socket.Available > 0)
            {
                int r = Socket.Receive(new byte[Socket.Available]);
                if (transportProtocol == ProtocolType.Tcp) tcpRcvd += r;
                else udpRcvd += r;
            }

            if (transportProtocol == ProtocolType.Udp)
                Socket.Send(outBytes);
            else netStream.Write(outBytes, 0, outBytes.Length);

            if (tcpRcvd == 0 && udpRcvd == 0)
            {
                if (transportProtocol == ProtocolType.Udp)
                    Socket.Send(outBytes);
                else netStream.Write(outBytes, 0, outBytes.Length);
            }

            if (transportProtocol == ProtocolType.Tcp) tcpSent += outBytes.Length;
            else udpSent += outBytes.Length;

            byte[] buffer = new byte[256];

            while (Socket.Available < 6) System.Threading.Thread.Sleep(0);

            int rcv = Socket.Receive(buffer);

            byte[] inBytes = new byte[rcv];
            
            Array.Copy(buffer, 0, inBytes, 0, rcv);

            if (transportProtocol == ProtocolType.Tcp) tcpRcvd += inBytes.Length;
            else udpRcvd += inBytes.Length;

            ModbusMessage result = new ModbusMessage(inBytes);

            recieveLog.Add(result);

            return result;
        }

        private byte[] ToBytes(ModbusMessage msg)
        {
            List<byte> result = new List<byte>();
            switch (transportEncoding)
            {
                case TransportEncoding.ASCII:
                    result.AddRange(msg.ToAdu(true));
                    result.AddRange(System.Text.Encoding.ASCII.GetBytes(ModbusMessage.CalculateLrc(result.ToArray()).ToString("X4")));
                    result.AddRange(System.Text.Encoding.ASCII.GetBytes("\r\n"));
                    break;
                case TransportEncoding.Binary:
                    result.Add(UnitId);
                    result.AddRange(msg.ToPdu());
                    result.AddRange(ModbusMessage.CalculateCrc16(result.ToArray()));
                    //result.Insert(0, UnitId);
                    break;
                case TransportEncoding.IpWithOutCrc:
                    msg.TransactionId = NextTransactionId;
                    msg.UnitId = UnitId;
                    result.AddRange(msg.ToAdu());
                    break;
                default:
                case TransportEncoding.IpWithCrc:
                    msg.TransactionId = NextTransactionId;
                    msg.UnitId = UnitId;
                    result.AddRange(msg.ToAdu());
                    result.AddRange(ModbusMessage.CalculateCrc16(result.ToArray()));
                    break;                
            }
            return result.ToArray();
        }

        public void ClearRecieveLog()
        {
            recieveLog.Clear();
        }

        public void ClearRequestLog()
        {
            requestLog.Clear();
        }

        public void ResetTransactionId()
        {
            currentTransactionId = 0;
        }

        public void RandomizeTransactionId()
        {
            currentTransactionId = ((short)(new Random(currentTransactionId).Next((int)ushort.MaxValue)));
        }

        public void ResetMessenger()
        {
            ClearRecieveLog();
            ClearRequestLog();
            tcpRcvd = tcpSent = udpSent = udpRcvd = currentTransactionId = 0;
        }

        public void Close()
        {
            try
            {
                ResetMessenger();
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

        public ModbusMessage SendMessage(ModbusMessage msg)
        {
            return SendMessage(msg, 0);
        }

        public ModbusMessage ReadCoils(short startingAddress, short coilQuantity)
        {
            if (coilQuantity > 0x7d0) throw new ArgumentOutOfRangeException("coilQuantity", "Cannot be grater than 2000");

            return SendMessage(new ModbusMessage()
            {
                FunctionCode = 0x01,
                StartingAddress = startingAddress,
                RegisterQuantity = coilQuantity
            });
        }

        public ModbusMessage ReadDiscreteInputs(short startingAddress, short inputQuantity)
        {
            if (inputQuantity > 0x7d0) throw new ArgumentOutOfRangeException("inputQuantity", "Cannot be grater than 2000");

            return SendMessage(new ModbusMessage()
            {
                FunctionCode = 0x02,
                StartingAddress = startingAddress,
                RegisterQuantity = inputQuantity
            });
        }

        public ModbusMessage ReadHoldingRegisters(ushort startingAddress, short registerQuantity)
        {
            if (registerQuantity > 0x7d) throw new ArgumentOutOfRangeException("registerQuantity", "Cannot be grater than 125");

            return SendMessage(new ModbusMessage()
            {
                FunctionCode = 0x03,
                StartingAddress = (short)startingAddress,
                RegisterQuantity = registerQuantity
            });
        }

        public ModbusMessage ReadInputRegisters(short startingAddress, short registerQuantity)
        {
            if (registerQuantity > 0x7d) throw new ArgumentOutOfRangeException("registerQuantity", "Cannot be grater than 125");

            return SendMessage(new ModbusMessage()
            {
                FunctionCode = 0x04,
                StartingAddress = startingAddress,
                RegisterQuantity = registerQuantity
            });
        }

        public ModbusMessage WriteSingleCoil(short outputAddress, bool coilOn)
        {            
            return SendMessage(new ModbusMessage()
            {
                FunctionCode = 0x05,
                StartingAddress = outputAddress,
                RegisterQuantity = ((short)(coilOn == true ? 0xff00 : 0x0000))
            });
        }

        public ModbusMessage WriteSingleRegister(short registerAddress, short registerValue)
        {
            return SendMessage(new ModbusMessage()
            {
                FunctionCode = 0x06,
                StartingAddress = registerAddress,
                RegisterQuantity = registerValue
            });
        }

        public ModbusMessage ReadExceptionStatus()
        {
            //functionCode 0x07
            throw new NotImplementedException();
        }

        public ModbusMessage Diagnostics()
        {
            //functionCode 0x08
            throw new NotImplementedException();
        }

        public ModbusMessage WriteMultipleCoils(short startingAddress, short coilQuantity, short coilStatus)
        {
            if (coilQuantity > 0x7b0) throw new ArgumentOutOfRangeException("coilQuantity", "Cannot be grater than 0x7b0");

            return SendMessage(new ModbusMessage()
            {
                FunctionCode = 0x0f,
                StartingAddress = startingAddress,
                RegisterQuantity = coilQuantity,
                DataPoints = new List<byte>(BitConverter.GetBytes(coilStatus))
            });
        }

        public ModbusMessage WriteMultipleRegisters(short startingAddress, short registerQuantity, short[] registerValues)
        {
            if (registerQuantity > 0x7b) throw new ArgumentOutOfRangeException("registerQuantity", "Cannot be grater than 123");

            ModbusMessage result = new ModbusMessage()
            {
                FunctionCode = 0x10,
                StartingAddress = startingAddress,
                RegisterQuantity = registerQuantity                
            };

            foreach (short register in registerValues)
            {
                result.DataPoints.AddRange(BitConverter.GetBytes(register));
            }

            return SendMessage(result);
        }

        public ModbusMessage ReadFileRecord()
        {
            //functionCode 0x14
            throw new NotImplementedException();
        }

        public ModbusMessage WriteFileRecord()
        {
            //functionCode 0x15
            throw new NotImplementedException();
        }

        public ModbusMessage MaskWriteRegister()
        {
            //functionCode 0x16
            throw new NotImplementedException();
        }

        public ModbusMessage ReadWriteMultipleRegisters(short readStartingAddress, short readRegisterQuantity, short writeStartingAddress, short writeRegisterQuantity, short[] writeRegisterValues)
        {
            if (readRegisterQuantity > 0x7d) throw new ArgumentOutOfRangeException("readRegisterQuantity", "Cannot be grater than 125");
            if (writeRegisterQuantity > 0x79) throw new ArgumentOutOfRangeException("readRegisterQuantity", "Cannot be grater than 121");

            ModbusMessage result = new ModbusMessage()
            {
                FunctionCode = 0x17,
                StartingAddress = readStartingAddress,
                RegisterQuantity = readRegisterQuantity
            };

            result.DataPoints.AddRange(BitConverter.GetBytes(writeStartingAddress));

            result.DataPoints.AddRange(BitConverter.GetBytes(writeRegisterQuantity));

            foreach (short register in writeRegisterValues)
            {
                result.DataPoints.AddRange(BitConverter.GetBytes(register));
            }

            return SendMessage(result);
        }

        public ModbusMessage ReadFifoQueue(short fifoPointerAddress)
        {
            return SendMessage(new ModbusMessage()
            {
                FunctionCode = 0x18,
                StartingAddress = fifoPointerAddress                
            });
        }

        public ModbusMessage ReadDeviceInformation(int objectId, short accessType)
        {
            return SendMessage(new ModbusMessage()
            {
                FunctionCode = 0x2b,
                StartingAddress = ((short)(0x0e00 | accessType)),
                DataPoints = new List<byte>(){ (byte)objectId }
            });
        }

        #endregion

        #region Constructor

        public ModbusMessenger(IPEndPoint ipEndPoint, ProtocolType transportProtocol)
        {
            this.ipEndPoint = ipEndPoint;
            this.transportProtocol = transportProtocol;
            Initialize();
        }

        public ModbusMessenger(IPEndPoint ipEndPoint, ProtocolType transportProtocol, TransportEncoding transportEncoding)
            : this(ipEndPoint, transportProtocol)
        {
            this.transportEncoding = transportEncoding;
        }

        #endregion

        #region Destructor

        ~ModbusMessenger()
        {
            Close();
        }

        #endregion
    }
}
