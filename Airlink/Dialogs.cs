using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ASTITransportation.Snmp;
using ASTITransportation.Snmp.Transport;

namespace Airlink
{
    public sealed class Dialogs
    {
        #region Constructor

        /// <summary>
        /// A private constructor in a sealed class with all static memebers has special perf. opts.
        /// </summary>
        private Dialogs()
        {
        }

        #endregion

        #region Airlink Modem Methods

        /// <summary>
        /// Retrieves the Model Information from a Airlink Modem
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction on</param>
        /// <returns>A Variable indicating the Model of the modem</returns>
        public static Variable GetModemModel(SnmpMessenger messengerSession)
        {
            PmppEndPoint? old = null;
            int oldv = messengerSession.SnmpVersion;
            System.Net.Sockets.ProtocolType oldType = messengerSession.TrasnportProtocol;
            if (messengerSession.PmppEndPoint.HasValue)
            {
                old = messengerSession.PmppEndPoint;
                messengerSession.PmppEndPoint = null;
            }
            messengerSession.TrasnportProtocol = System.Net.Sockets.ProtocolType.Udp;
            messengerSession.SnmpVersion = 2;
            //modemModel
            Variable result = messengerSession.Get("1.3.6.1.4.1.20542.1.4.0").Bindings[0];

            if (old.HasValue)
            {
                messengerSession.PmppEndPoint = old;
            }
            messengerSession.TrasnportProtocol = oldType;
            messengerSession.SnmpVersion = oldv;
            //voltage
            return result;
        }

        /// <summary>
        /// Retrieves the Software Version Information from a Airlink Modem
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction on</param>
        /// <returns>A Variable indicating the Software Version of the modem</returns>
        public static Variable GetModemSoftwareVersion(SnmpMessenger messengerSession)
        {
            PmppEndPoint? old = null;
            int oldv = messengerSession.SnmpVersion;
            System.Net.Sockets.ProtocolType oldType = messengerSession.TrasnportProtocol;
            if (messengerSession.PmppEndPoint.HasValue)
            {
                old = messengerSession.PmppEndPoint;
                messengerSession.PmppEndPoint = null;
            }
            messengerSession.TrasnportProtocol = System.Net.Sockets.ProtocolType.Udp;
            messengerSession.SnmpVersion = 2;
            //aleos software version
            Variable result = messengerSession.Get("1.3.6.1.4.1.20542.1.5.0").Bindings[0];

            if (old.HasValue)
            {
                messengerSession.PmppEndPoint = old;
            }
            messengerSession.TrasnportProtocol = oldType;
            messengerSession.SnmpVersion = oldv;
            //voltage
            return result;
        }

        /// <summary>
        /// Retrieves the Network Provider Information from a Airlink Modem
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction on</param>
        /// <returns>A Variable indicating the Network Provider Information of the modem</returns>
        public static Variable GetModemNetworkProvider(SnmpMessenger messengerSession)
        {
            PmppEndPoint? old = null;
            int oldv = messengerSession.SnmpVersion;
            System.Net.Sockets.ProtocolType oldType = messengerSession.TrasnportProtocol;
            if (messengerSession.PmppEndPoint.HasValue)
            {
                old = messengerSession.PmppEndPoint;
                messengerSession.PmppEndPoint = null;
            }
            messengerSession.TrasnportProtocol = System.Net.Sockets.ProtocolType.Udp;
            messengerSession.SnmpVersion = 2;
            //voltage
            Variable result = messengerSession.Get("1.3.6.1.4.1.20542.3.10.0").Bindings[0];

            if (old.HasValue)
            {
                messengerSession.PmppEndPoint = old;
            }
            messengerSession.TrasnportProtocol = oldType;
            messengerSession.SnmpVersion = oldv;
            //voltage
            return result;
        }

        /// <summary>
        /// Retrieves the Network Type Information from a Airlink Modem
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction on</param>
        /// <returns>A Variable indicating the Network Type Information of the modem</returns>
        public static Variable GetModemNetworkType(SnmpMessenger messengerSession)
        {
            PmppEndPoint? old = null;
            int oldv = messengerSession.SnmpVersion;
            System.Net.Sockets.ProtocolType oldType = messengerSession.TrasnportProtocol;
            if (messengerSession.PmppEndPoint.HasValue)
            {
                old = messengerSession.PmppEndPoint;
                messengerSession.PmppEndPoint = null;
            }
            messengerSession.TrasnportProtocol = System.Net.Sockets.ProtocolType.Udp;
            messengerSession.SnmpVersion = 2;
            //provider
            Variable result = messengerSession.Get("1.3.6.1.4.1.20542.3.11.0").Bindings[0];

            if (old.HasValue)
            {
                messengerSession.PmppEndPoint = old;
            }
            messengerSession.TrasnportProtocol = oldType;
            messengerSession.SnmpVersion = oldv;
            //voltage
            return result;
        }

        /// <summary>
        /// Retrieves the Network Time Information from a Airlink Modem
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction on</param>
        /// <returns>A Variable indicating the Network Time Information of the modem</returns>
        public static Variable GetModemNetworkTime(SnmpMessenger messengerSession)
        {
            PmppEndPoint? old = null;
            int oldv = messengerSession.SnmpVersion;
            System.Net.Sockets.ProtocolType oldType = messengerSession.TrasnportProtocol;
            if (messengerSession.PmppEndPoint.HasValue)
            {
                old = messengerSession.PmppEndPoint;
                messengerSession.PmppEndPoint = null;
            }
            messengerSession.TrasnportProtocol = System.Net.Sockets.ProtocolType.Udp;
            messengerSession.SnmpVersion = 2;
            //Current network Date Time
            Variable result = messengerSession.Get("1.3.6.1.4.1.20542.2.1.0").Bindings[0];

            if (old.HasValue)
            {
                messengerSession.PmppEndPoint = old;
            }
            messengerSession.TrasnportProtocol = oldType;
            messengerSession.SnmpVersion = oldv;
            //voltage
            return result;
        }

        /// <summary>
        /// Retrieves the GPS Information from a Airlink Modem
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction on</param>
        /// <returns>A Variable indicating the GPS Information of the modem</returns>
        public static List<Variable> GetGPSInformation(SnmpMessenger messengerSession)
        {
            PmppEndPoint? old = null;
            int oldV = messengerSession.SnmpVersion;
            System.Net.Sockets.ProtocolType oldType = messengerSession.TrasnportProtocol;
            if (messengerSession.PmppEndPoint.HasValue)
            {
                old = messengerSession.PmppEndPoint;
                messengerSession.PmppEndPoint = null;
            }

            List<string> objects = new List<string>(){
                "1.3.6.1.4.1.20542.4.1.0",//GPSFix
                "1.3.6.1.4.1.20542.4.2.0"//SatelliteCount
            };
            messengerSession.TrasnportProtocol = System.Net.Sockets.ProtocolType.Udp;
            messengerSession.SnmpVersion = 2;

            List<Variable> results = messengerSession.Get(objects).Bindings;

            if (old.HasValue)
            {
                messengerSession.PmppEndPoint = old;
            }
            messengerSession.TrasnportProtocol = oldType;
            messengerSession.SnmpVersion = oldV;

            return results;
        }

        /// <summary>
        /// Retrieves the Latitude and Longitude Information from a Airlink Modem
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction on</param>
        /// <returns>A Variable indicating the Latitude and Longitude of the modem</returns>
        public static List<Variable> GetLatitudeLongitude(SnmpMessenger messengerSession)
        {
            PmppEndPoint? old = null;
            int oldV = messengerSession.SnmpVersion;
            System.Net.Sockets.ProtocolType oldType = messengerSession.TrasnportProtocol;
            if (messengerSession.PmppEndPoint.HasValue)
            {
                old = messengerSession.PmppEndPoint;
                messengerSession.PmppEndPoint = null;
            }

            List<string> objects = new List<string>(){
                "1.3.6.1.4.1.20542.4.3.0",//latitude
                "1.3.6.1.4.1.20542.4.4.0"//longitude
            };
            messengerSession.TrasnportProtocol = System.Net.Sockets.ProtocolType.Udp;
            messengerSession.SnmpVersion = 2;

            List<Variable> results = messengerSession.Get(objects).Bindings;

            if (old.HasValue)
            {
                messengerSession.PmppEndPoint = old;
            }
            messengerSession.TrasnportProtocol = oldType;
            messengerSession.SnmpVersion = oldV;

            return results;
        }

        /// <summary>
        /// Retrieves the Voltage Information from a Airlink Modem
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction on</param>
        /// <returns>A Variable indicating the RSSI Signal Strength of the modem</returns>
        public static Variable GetVoltage(SnmpMessenger messengerSession)
        {
            PmppEndPoint? old = null;
            int oldv = messengerSession.SnmpVersion;
            System.Net.Sockets.ProtocolType oldType = messengerSession.TrasnportProtocol;
            if (messengerSession.PmppEndPoint.HasValue)
            {
                old = messengerSession.PmppEndPoint;
                messengerSession.PmppEndPoint = null;
            }
            messengerSession.TrasnportProtocol = System.Net.Sockets.ProtocolType.Udp;
            messengerSession.SnmpVersion = 2;
            //voltage
            Variable result = messengerSession.Get("1.3.6.1.4.1.20542.3.13.0").Bindings[0];

            if (old.HasValue)
            {
                messengerSession.PmppEndPoint = old;
            }
            messengerSession.TrasnportProtocol = oldType;
            messengerSession.SnmpVersion = oldv;
            //voltage
            return result;
        }

        /// <summary>
        /// Retrieves the RSSI Signal Strength from a Airlink Modem
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction on</param>
        /// <returns>A Variable indicating the RSSI Signal Strength of the modem</returns>
        public static Variable GetRSSI(SnmpMessenger messengerSession)
        {
            PmppEndPoint? old = null;
            int oldv = messengerSession.SnmpVersion;
            System.Net.Sockets.ProtocolType oldType = messengerSession.TrasnportProtocol;
            if (messengerSession.PmppEndPoint.HasValue)
            {
                old = messengerSession.PmppEndPoint;
                messengerSession.PmppEndPoint = null;
            }
            messengerSession.TrasnportProtocol = System.Net.Sockets.ProtocolType.Udp;
            messengerSession.SnmpVersion = 2;
            //rssi
            Variable result = messengerSession.Get("1.3.6.1.4.1.20542.3.4.0").Bindings[0];

            if (old.HasValue)
            {
                messengerSession.PmppEndPoint = old;
            }
            messengerSession.SnmpVersion = oldv;
            messengerSession.TrasnportProtocol = oldType;
            return result;
        }

        /// <summary>
        /// Retrieves the Serial Settings from a Airlink Modem
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction on</param>
        /// <returns>A List of variables containing the serial settings of the modem</returns>
        public static List<Variable> GetSerialSettings(SnmpMessenger messengerSession)
        {

            PmppEndPoint? old = null;
            int olv = messengerSession.SnmpVersion;
            System.Net.Sockets.ProtocolType oldType = messengerSession.TrasnportProtocol;
            if (messengerSession.PmppEndPoint.HasValue)
            {
                old = messengerSession.PmppEndPoint;
                messengerSession.PmppEndPoint = null;
            }

            List<string> objects = new List<string>()
            {
                "1.3.6.1.4.1.20542.2.9.0",//serialPortSettings
                "1.3.6.1.4.1.20542.2.10.0"//serialPortFlowControl
            };
            messengerSession.TrasnportProtocol = System.Net.Sockets.ProtocolType.Udp;
            messengerSession.SnmpVersion = 2;
            List<Variable> results = messengerSession.Get(objects).Bindings;

            if (old.HasValue)
            {
                messengerSession.PmppEndPoint = old;
            }
            messengerSession.TrasnportProtocol = oldType;
            messengerSession.SnmpVersion = olv;

            return results;
        }

        #endregion
    }
}
