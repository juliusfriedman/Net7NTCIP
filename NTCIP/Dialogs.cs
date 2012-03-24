using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ASTITransportation.Snmp;
using ASTITransportation.Snmp.Transport;
using ASTITransportation.Utilities;

namespace ASTITransportation.NTCIP
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

        #region Global Methods

        /// <summary>
        /// Downloads the global.globalConfiguration Variables
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction</param>
        /// <returns>The List of Variables retireved in the transaction</returns>
        public static List<Variable> GetGlobals(SnmpMessenger messengerSession)
        {
            List<string> objects = new List<string>()
            {
                "1.3.6.1.4.1.1206.4.2.6.1.3.1.3.1", //global.globalConfiguration.globalModuleTable.moduleTableEntry.moduleMake.1
                "1.3.6.1.4.1.1206.4.2.6.1.3.1.4.1", //global.globalConfiguration.globalModuleTable.moduleTableEntry.moduleModel.1
                "1.3.6.1.4.1.1206.4.2.6.1.3.1.5.1", //global.globalConfiguration.globalModuleTable.moduleTableEntry.moduleVersion.1
                "1.3.6.1.4.1.1206.4.2.6.1.3.1.6.1", //global.globalConfiguration.globalModuleTable.moduleTableEntry.moduleType.1
            };

            List<Variable> results = messengerSession.Get(objects).Bindings;
            return results;
        }

        #endregion

        #region DMS Methods

        #region Message Functions

        /// <summary>
        /// Run and Activate a message on a DMS
        /// </summary>
        /// <param name="multiString">The multiString to activate</param>
        /// <param name="messengerSession">The messengerSession in which to perform the transaction</param>
        /// <returns>The dmsActivateMsgError of the Transaction</returns>
        public static dmsActivateMsgError RunMessage(String multiString, SnmpMessenger messengerSession)
        {
            UInt32 crc = ModifyMessage(multiString, 255, 1, dmsMessageMemoryType.Changeable, messengerSession);

            return ActivateMesssage(crc, 65535, 255, 1, dmsMessageMemoryType.Changeable, messengerSession);            
        }

        /// <summary>
        /// Run and Activate a message on a DMS with the IP of the owner set to the IP of the machine which runs this method
        /// </summary>
        /// <param name="multiString">The multiString to activate</param>
        /// <param name="owner">The owner of the message</param>
        /// <param name="messengerSession">The messengerSession in which to perform the transaction</param>
        /// <returns>The dmsActivateMsgError of the Transaction</returns>
        public static dmsActivateMsgError RunMessage(String multiString, String owner, SnmpMessenger messengerSession)
        {
            UInt32 crc = ModifyMessage(multiString, owner, 255, 1, dmsMessageMemoryType.Changeable, messengerSession);

            return ActivateMesssage(crc, 65535, 255, 1, dmsMessageMemoryType.Changeable, messengerSession);
        }

        /// <summary>
        /// Run and Activate a message on a DMS using the Changeable memory
        /// </summary>
        /// <param name="multiString">The multiString to activate</param>        
        /// <param name="owner">The owner of the message</param>
        /// <param name="ownerIp">The IPAddress of the owner of the message</param>
        /// <param name="messengerSession">The messengerSession in which to perform the transaction</param>
        /// <returns>The dmsActivateMsgError of the Transaction</returns>
        public static dmsActivateMsgError RunMessage(String multiString, String owner, System.Net.IPAddress ownerIp, SnmpMessenger messengerSession)
        {
            UInt32 crc = ModifyMessage(multiString, owner, 255, 1, dmsMessageMemoryType.Changeable, messengerSession);

            return ActivateMesssage(crc, ownerIp, 65535, 255, 1, dmsMessageMemoryType.Changeable, messengerSession);
        }

        /// <summary>
        /// Run and Activate a message on a DMS using the given memory type
        /// </summary>
        /// <param name="multiString">The multiString to activate</param>        
        /// <param name="owner">The owner of the message</param>
        /// <param name="ownerIp">The IPAddress of the owner of the message</param>
        /// <param name="messageMemoryType">The dmsMessageMemoryType of the message</param>
        /// <param name="messengerSession">The messengerSession in which to perform the transaction</param>
        /// <returns>The dmsActivateMsgError of the Transaction</returns>
        public static dmsActivateMsgError RunMessage(String multiString, String owner, System.Net.IPAddress ownerIp, dmsMessageMemoryType messageMemoryType, SnmpMessenger messengerSession)
        {

            UInt32 crc = ModifyMessage(multiString, owner, 255, 1, messageMemoryType, messengerSession);

            return ActivateMesssage(crc, ownerIp, 65535, 255, 1, messageMemoryType, messengerSession);
        }

        /// <summary>
        /// Active a Message already on a DMS
        /// </summary>
        /// <param name="messageCrc">The Crc16 of the message</param>
        /// <param name="ownerIp">The IPAddress of the owner of the message</param>
        /// <param name="duration">the duration to display the message in minutes</param>
        /// <param name="priority">the priority of the message</param>
        /// <param name="messageNumber">the message number to display</param>
        /// <param name="messageMemoryType">the memoryType from which to load the message</param>
        /// <param name="messengerSession">the messengerSession to perform the transaction on</param>
        /// <returns>The dmsActivateMsgError of the Transaction</returns>
        public static dmsActivateMsgError ActivateMesssage(UInt32 messageCrc, System.Net.IPAddress ownerIp, ushort? duration, byte? priority, Int32? messageNumber, dmsMessageMemoryType? messageMemoryType, SnmpMessenger messengerSession)
        {
            messageMemoryType = messageMemoryType.HasValue ? messageMemoryType.Value : dmsMessageMemoryType.Changeable;
            messageNumber = messageNumber.HasValue && messageNumber > 0 ? messageNumber.Value : 1;
            priority = priority.HasValue ? priority.Value : (byte)255;
            duration = duration.HasValue ? duration.Value : (ushort)65535;
            
            //Make a standard function GetMessageActivationCode
            byte[] activate = new byte[12];
            BitConverter.GetBytes(duration.Value).CopyTo(activate, 0);
            //activate[0] = (byte)(duration / 256); //Duration
            //activate[1] = (byte)(duration & 0xFF); // lsb of duration
            activate[2] = (byte) priority; //priority
            activate[3] = (byte) messageMemoryType; // memory type
            activate[4] = (byte)(messageNumber / 256); //messageNumber 
            activate[5] = (byte)(messageNumber & 0xFF); // lsb of messageNumber
            activate[6] = (byte)(messageCrc / 256);// crc
            activate[7] = (byte)(messageCrc & 0xFF); // lsb of crc

            if (null == ownerIp || ownerIp.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            {
                ownerIp = Utility.GetV4IPAddress();
            }

            Array.ConvertAll<int, byte>(Array.ConvertAll<string, int>(ownerIp.ToString().Split('.'), Convert.ToInt32), Convert.ToByte).CopyTo(activate, 8);

            Variable dmsActivateMessage = new Variable();
            dmsActivateMessage.Identifier = "1.3.6.1.4.1.1206.4.2.3.6.3.0";
            dmsActivateMessage.TypeCode = SnmpType.OctetString;
            dmsActivateMessage.Value.AddRange(activate);

            dmsActivateMessage = messengerSession.Set(dmsActivateMessage).Bindings[0];

            Variable dmsActivateMsgError = new Variable();
            dmsActivateMsgError.Identifier = "1.3.6.1.4.1.1206.4.2.3.6.17.0";

            dmsActivateMsgError = messengerSession.Get(dmsActivateMsgError.Identifier).Bindings[0];

            return (dmsActivateMsgError)dmsActivateMsgError.ToInt32();
        }

        /// <summary>
        /// Active a Message already on a DMS with the owner Ip set to the IP of the machine calling the method
        /// </summary>
        /// <param name="messageCrc">The Crc16 of the message</param>
        /// <param name="duration">the duration to display the message</param>
        /// <param name="priority">the priority of the message</param>
        /// <param name="messageNumber">the message number to display</param>
        /// <param name="messageMemoryType">the memoryType from which to load the message</param>
        /// <param name="messengerSession">the messengerSession to perform the transaction on</param>
        /// <returns>The dmsActivateMsgError of the Transaction</returns>
        public static dmsActivateMsgError ActivateMesssage(UInt32 messageCrc, ushort? duration, byte? priority, Int32? messageNumber, dmsMessageMemoryType? messageMemoryType, SnmpMessenger messengerSession)
        {
            return ActivateMesssage(messageCrc, null, duration, priority, messageNumber, messageMemoryType, messengerSession);
        }

        /// <summary>
        /// Modifies a Message in the DMS
        /// </summary>
        /// <param name="multiString">The new value for the multiString</param>
        /// <param name="priority">The new priority for the multiString</param>
        /// <param name="messageNumber">The messageNumber to modify</param>
        /// <param name="messageMemoryType">The type of memory to store multiString in</param>
        /// <param name="messengerSession">The messengerSession to perform the request on</param>
        /// <returns>The Crc16 of the Modified Message</returns>
        public static UInt32 ModifyMessage(String multiString, String owner, byte? priority, byte? messageNumber, dmsMessageMemoryType? messageMemoryType, SnmpMessenger messengerSession)
        {

            messageMemoryType = messageMemoryType.HasValue ? messageMemoryType.Value : dmsMessageMemoryType.Changeable;
            messageNumber = messageNumber.HasValue ? messageNumber.Value : (byte)1;
            priority = priority.HasValue ? priority.Value : (byte)255;
            if (string.IsNullOrEmpty(owner)) owner = Environment.MachineName;

            string oidPrefix = "1.3.6.1.4.1.1206.4.2.3.5.8.1.";
            string oidPostfix = "." + (int)messageMemoryType + "." + messageNumber.ToString();
            
            //Variable dmsMessageStatus = messengerSession.Get("1.3.6.1.4.1.1206.4.2.3.5.8.1.5.3.1").Bindings[0];
            Variable dmsMessageStatus = messengerSession.Get(oidPrefix + (int)DmsMessageEntry.dmsMessageStatus + oidPostfix).Bindings[0];

            if ((NTCIP.dmsMessageStatus)dmsMessageStatus.ToInt32() != NTCIP.dmsMessageStatus.NotUsed
                 &&
                (NTCIP.dmsMessageStatus)dmsMessageStatus.ToInt32() != NTCIP.dmsMessageStatus.NotUsedReq)
            {
                dmsMessageStatus.Value = BasicEncodingRules.EncodeInteger32((int)NTCIP.dmsMessageStatus.NotUsedReq, false, false);
                dmsMessageStatus = messengerSession.Set(dmsMessageStatus).Bindings[0];
            }

            dmsMessageStatus.Value = BasicEncodingRules.EncodeInteger32((int)NTCIP.dmsMessageStatus.ModifyReq, false, false);
            dmsMessageStatus = messengerSession.Set(dmsMessageStatus).Bindings[0];

            int attempt = 0;

            //standards....
            while ((NTCIP.dmsMessageStatus)dmsMessageStatus.ToInt32() != NTCIP.dmsMessageStatus.Modifying
                &&
                   (NTCIP.dmsMessageStatus)dmsMessageStatus.ToInt32() != NTCIP.dmsMessageStatus.ModifyReq
                && attempt <= messengerSession.MaxRetries)
            {
                dmsMessageStatus = messengerSession.Get(dmsMessageStatus.Identifier).Bindings[0];
                attempt++;
            }

            Variable dmsMessageMultiString = new Variable();
            dmsMessageMultiString.Identifier = oidPrefix + (int)DmsMessageEntry.dmsMessageMultiString + oidPostfix;
            dmsMessageMultiString.TypeCode = SnmpType.OctetString;
            dmsMessageMultiString.Value = BasicEncodingRules.EncodeOctetString(multiString, false, false);

            Variable dmsMessageOwner = new Variable();
            dmsMessageOwner.Identifier = oidPrefix + (int)DmsMessageEntry.dmsMessageOwner + oidPostfix;
            dmsMessageOwner.TypeCode = SnmpType.OctetString;
            dmsMessageOwner.Value = BasicEncodingRules.EncodeOctetString(owner, false, false);

            Variable dmsMessageRunTimePriority = new Variable();
            dmsMessageRunTimePriority.Identifier = oidPrefix + (int)DmsMessageEntry.dmsMessageRunTimePriority + oidPostfix;
            dmsMessageRunTimePriority.TypeCode = SnmpType.Integer32;
            dmsMessageRunTimePriority.Value = BasicEncodingRules.EncodeInteger32(priority.Value, false, false);

            dmsMessageMultiString = messengerSession.Set(dmsMessageMultiString).Bindings[0];
            dmsMessageOwner = messengerSession.Set(dmsMessageOwner).Bindings[0];
            dmsMessageRunTimePriority = messengerSession.Set(dmsMessageRunTimePriority).Bindings[0];

            dmsMessageStatus.Value = BasicEncodingRules.EncodeInteger32((int)NTCIP.dmsMessageStatus.ValidateReq, false, false);

            dmsMessageStatus = messengerSession.Set(dmsMessageStatus).Bindings[0];

            while ((NTCIP.dmsMessageStatus)dmsMessageStatus.ToInt32() == NTCIP.dmsMessageStatus.Validating                   
                   ||
                   (NTCIP.dmsMessageStatus)dmsMessageStatus.ToInt32() == NTCIP.dmsMessageStatus.ValidateReq
                   ||dmsMessageStatus.ToInt32() == 255)
            {
                dmsMessageStatus = messengerSession.Get(dmsMessageStatus.Identifier).Bindings[0];                
            }

            //Should check if valid here and if not valid get the reason why its not valid and report.
            if ((NTCIP.dmsMessageStatus)dmsMessageStatus.ToInt32() != NTCIP.dmsMessageStatus.Valid)
            {
                //needs steps here to determine error and return
                System.Threading.Thread.Sleep(0);
            }

            Variable dmsMessageCRC = new Variable();
            dmsMessageCRC.Identifier = oidPrefix + (int)DmsMessageEntry.dmsMessageCRC + oidPostfix;
            dmsMessageCRC = messengerSession.Get(dmsMessageCRC.Identifier).Bindings[0];

            return dmsMessageCRC.ToUInt32();
            
        }

        /// <summary>
        /// Modifies a Message in the DMS With the Owner set to the Machine Name of the computer which runs the method
        /// </summary>
        /// <param name="multiString">The new value for the multiString</param>
        /// <param name="priority">The new priority for the multiString</param>
        /// <param name="messageNumber">The messageNumber to modify</param>
        /// <param name="messageMemoryType">The type of memory to store multiString in</param>
        /// <param name="messengerSession">The messengerSession to perform the request on</param>
        /// <returns>The Crc16 of the Modified Message</returns>
        public static UInt32 ModifyMessage(String multiString, byte? priority, byte? messageNumber, dmsMessageMemoryType? messageMemoryType, SnmpMessenger messengerSession)
        {

            return ModifyMessage(multiString, Environment.MachineName, priority, messageNumber, messageMemoryType, messengerSession);

        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="multiString"></param>
        /// <param name="duration"></param>
        /// <param name="priority"></param>
        /// <param name="messageNumber"></param>
        /// <param name="messageMemoryType"></param>
        /// <param name="ownerIp"></param>
        /// <param name="messengerSession"></param>
        /// <returns></returns>
        private static byte[] GenerateActivationCode(String multiString, ushort? duration, byte? priority, byte? messageNumber, dmsMessageMemoryType? messageMemoryType, System.Net.IPAddress ownerIp, SnmpMessenger messengerSession)
        {
            duration = duration ?? 255;
            priority = priority ?? 255;
            messageMemoryType = messageMemoryType ?? dmsMessageMemoryType.Changeable;
            if (null == ownerIp) ownerIp = Utility.GetV4IPAddress();
            byte[] crc = (new Crc16()).ComputeHash(Encoding.ASCII.GetBytes(multiString));

            byte[] activate = new byte[12];
            BitConverter.GetBytes(duration.Value).CopyTo(activate, 0);
            //activate[0] = (byte)(duration / 256); //Duration
            //activate[1] = (byte)(duration & 0xFF); // lsb of duration
            activate[2] = (byte)255;//priority
            activate[3] = (byte)dmsMessageMemoryType.Blank; // memory type
            activate[4] = (byte)(255 / 256);
            activate[5] = (byte)(255 & 0xFF); // message number
            activate[6] = (byte)(0);
            activate[7] = (byte)(0); // lsb of crc

            crc.CopyTo(activate, 6);

            byte[] ipBytes = Array.ConvertAll<int, byte>(Array.ConvertAll<string, int>(ownerIp.ToString().Split('.'), Convert.ToInt32), Convert.ToByte);

            ipBytes.CopyTo(activate, 8);

            ipBytes = null;

            crc = null;

            return activate;
        }

        /// <summary>
        /// Blanks a DMS for the specified duration
        /// </summary>
        /// <param name="duration">The duartion to blank the DMS for</param>
        /// <param name="messengerSession">The messengerSession in which to perform this transaction</param>
        /// <returns>The dmsActivateMsgError of the transaction</returns>
        public static dmsActivateMsgError Blank(ushort? duration, SnmpMessenger messengerSession)
        {
            byte[] activate = new byte[12];
            BitConverter.GetBytes(duration.Value).CopyTo(activate, 0);
            //activate[0] = (byte)(duration / 256); //Duration
            //activate[1] = (byte)(duration & 0xFF); // lsb of duration
            activate[2] = (byte)255;//priority
            activate[3] = (byte)dmsMessageMemoryType.Blank; // memory type
            activate[4] = (byte)(255 / 256);
            activate[5] = (byte)(255 & 0xFF); // message number
            activate[6] = (byte)(0);
            activate[7] = (byte)(0); // lsb of crc

            byte[] ipBytes = Array.ConvertAll<int, byte>(Array.ConvertAll<string, int>(Utility.GetV4IPAddress().ToString().Split('.'), Convert.ToInt32), Convert.ToByte);

            ipBytes.CopyTo(activate, 8);

            Variable dmsActivateMessage = new Variable();
            dmsActivateMessage.Identifier = "1.3.6.1.4.1.1206.4.2.3.6.3.0";
            dmsActivateMessage.TypeCode = SnmpType.OctetString;
            dmsActivateMessage.Value.AddRange(activate);

            dmsActivateMessage = messengerSession.Set(dmsActivateMessage).Bindings[0];

            Variable dmsActivateMsgError = new Variable();
            dmsActivateMsgError.Identifier = "1.3.6.1.4.1.1206.4.2.3.6.17.0";

            dmsActivateMsgError = messengerSession.Get(dmsActivateMsgError.Identifier).Bindings[0];

            return (dmsActivateMsgError)dmsActivateMsgError.ToInt32();
        }

        #endregion

        #region Diagnostic Functions

        /// <summary>
        /// Resets a DMS
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction on</param>
        /// <returns>The Variable which represents the result of the transaction</returns>
        public static Variable DmsReset(SnmpMessenger messengerSession)
        {
            Variable dmsSWReset = new Variable("1.3.6.1.4.1.1206.4.2.3.6.2");
            dmsSWReset.TypeCode = SnmpType.Integer32;
            dmsSWReset.Value = BasicEncodingRules.EncodeInteger32(1, false, false);
            dmsSWReset = messengerSession.Set(dmsSWReset).Bindings[0];
            return dmsSWReset;
        }

        /// <summary>
        /// Directs the DMS to perform a pixel test
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction</param>
        /// <returns>A Variable indicating the amount of rows in in the BadPixel table</returns>
        public static Variable ActivatePixelTest(SnmpMessenger messengerSession)
        {
            Variable pixelTestActivation = new Variable("1.3.6.1.4.1.1206.4.2.3.9.7.4.0");
            pixelTestActivation.TypeCode = SnmpType.Integer32;            
            pixelTestActivation.Value = BasicEncodingRules.EncodeInteger32((int)NTCIP.pixelFailureDetectionType.MessageDisplay, false, false);
            pixelTestActivation = messengerSession.Set(pixelTestActivation).Bindings[0];

            while ((NTCIP.pixelFailureDetectionType)pixelTestActivation.ToInt32() == NTCIP.pixelFailureDetectionType.MessageDisplay)
            {
                System.Threading.Thread.Sleep(1000);
                pixelTestActivation = messengerSession.Get(pixelTestActivation.Identifier).Bindings[0];
            }

            Variable pixelFailureTableNumRows = new Variable("1.3.6.1.4.1.1206.4.2.3.9.7.2.0");
            pixelFailureTableNumRows = messengerSession.Get(pixelFailureTableNumRows.Identifier).Bindings[0];
            return pixelFailureTableNumRows;
        }

        /// <summary>
        /// Directs the DMS to perform a pixel test and retrieves the list of bad pixels from the bad pixel table
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction</param>
        /// <returns>A list of variables which represet the X and Y locations of the bad pixels</returns>
        public static List<Variable> GetBadPixelLocations(SnmpMessenger messengerSession)
        {
            Variable pixelFailureTableNumRows = ActivatePixelTest(messengerSession);

            List<Variable> badPixels = new List<Variable>();

            string OIDbase = "1.3.6.1.4.1.1206.4.2.3.9.7.3.1.";

            int exCount = 0;

            for (int i = 1, e = (pixelFailureTableNumRows.ToInt32() + 1); i < e; ++i)
            {
                Variable pixelFailureXLocation = new Variable(OIDbase + "3.2." + i.ToString() + ".0");
                Variable pixelFailureYLocation = new Variable(OIDbase + "4.2." + i.ToString() + ".0");

                try
                {
                    pixelFailureXLocation = messengerSession.Get(pixelFailureXLocation.Identifier).Bindings[0];
                    pixelFailureYLocation = messengerSession.Get(pixelFailureYLocation.Identifier).Bindings[0];
                    exCount = 0;
                }
                catch (SnmpException ex)
                {
                    if (ex.Message == "Request has reached maximum retries." && exCount < messengerSession.MaxRetries)
                    {                        
                        i--;
                        exCount++;
                        continue;
                    }
                    else throw;
                }
                catch (Exception)
                {
                    throw;
                }

                if (pixelFailureXLocation.TypeCode == SnmpType.Null || pixelFailureYLocation.TypeCode == SnmpType.Null)
                {
                    break;
                }

                badPixels.Add(pixelFailureXLocation);
                badPixels.Add(pixelFailureYLocation);
            }

            return badPixels;
        }

        /// <summary>
        /// Creats a string which shows what pixels of the DMS need replacing on a list of already known bad pixels
        /// </summary>
        /// <param name="badPixels">A List of bad pixels from the GetBadPixelLocationsFunction</param>
        /// <param name="messengerSession">The messengerSession to perform the transaction</param>
        /// <returns>A string which can be read to determine which pixels are in need of replacement</returns>
        public static String PixelReport(List<Variable> badPixels, SnmpMessenger messengerSession)
        {
            List<string> cfg = new List<string>();
            cfg.Add("1.3.6.1.4.1.1206.4.2.3.2.1"); //vmsCharacterHeightPixels -- charBoard
            cfg.Add("1.3.6.1.4.1.1206.4.2.3.2.2"); //vmsCharacterWidthPixels -- charBoard
            cfg.Add("1.3.6.1.4.1.1206.4.2.3.2.3"); //vmsSignHeightPixels -- (used to get amt of charboards or rows high)
            cfg.Add("1.3.6.1.4.1.1206.4.2.3.2.4"); //vmsSignWidthPixels -- (used to get amt of charboards or rows wide)
            List<Variable> downloadedConfiguration = messengerSession.Get(cfg).Bindings;
            Variable vmsCharacterHeightPixels = downloadedConfiguration[0];
            Variable vmsCharacterWidthPixels = downloadedConfiguration[1];
            Variable vmsSignHeightPixels = downloadedConfiguration[2];
            Variable vmsSignWidthPixels = downloadedConfiguration[3];

            Func<int, int, int, int> LogicalIndex = delegate(int vmsSignHeightPixelsORvmsSignWidthPixels, int vmsCharacterHeightPixelsORvmsCharacterWidthPixels, int OutagePixelLocationXorY)
            {
                if (vmsSignHeightPixelsORvmsSignWidthPixels <= 0 || vmsCharacterHeightPixelsORvmsCharacterWidthPixels <= 0 || OutagePixelLocationXorY <= 0) return -1;
                return (vmsSignHeightPixelsORvmsSignWidthPixels / vmsCharacterHeightPixelsORvmsCharacterWidthPixels - (int)Math.Floor((double)(vmsSignHeightPixelsORvmsSignWidthPixels - OutagePixelLocationXorY) / vmsCharacterHeightPixelsORvmsCharacterWidthPixels));                                
            };

            StringBuilder output = new StringBuilder();

            for (int bpi = 0; bpi < badPixels.Count; bpi += 2)
            {
                int Horizontal = LogicalIndex(vmsSignWidthPixels.ToInt32(), vmsCharacterWidthPixels.ToInt32(), badPixels[bpi].ToInt32());
                int Vertical = LogicalIndex(vmsSignHeightPixels.ToInt32(), vmsCharacterHeightPixels.ToInt32(), badPixels[bpi + 1].ToInt32());
                output.AppendLine("-------");
                output.AppendLine("Outage On Horizontal Character Board: " + Horizontal + "  From the Left");
                output.AppendLine("Outage On Horizontal Character Board: " + Vertical + "  From the Top");
                output.AppendLine("-------");
            }

            return output.ToString();
        }

        /// <summary>
        /// Creats a string which shows what pixels of the DMS need replacing
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction</param>
        /// <returns>A string which can be read to determine which pixels are in need of replacement</returns>
        public static String PixelReport(SnmpMessenger messengerSession)
        {
            List<Variable> badPixels = GetBadPixelLocations(messengerSession);
            return PixelReport(badPixels, messengerSession);
        }

        /// <summary>
        /// Downloads the dms.multiCfg Variables
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction</param>
        /// <returns>The List of Variables retireved in the transaction</returns>
        public static List<Variable> GetMultiCfg(SnmpMessenger messengerSession)
        {
            List<string> objects = new List<string>()
            {
                "1.3.6.1.4.1.1206.4.2.3.4.1", //dms.multiCfg.defaultBackgroundColor
                "1.3.6.1.4.1.1206.4.2.3.4.2", //dms.multiCfg.defaultForegroundColor
                "1.3.6.1.4.1.1206.4.2.3.4.3", //dms.multiCfg.defaultFlashOn
                "1.3.6.1.4.1.1206.4.2.3.4.4", //dms.multiCfg.defaultFlashOff
                "1.3.6.1.4.1.1206.4.2.3.4.5", //dms.multiCfg.defaultFont
            };

            List<Variable> results = messengerSession.Get(objects).Bindings;

            objects = new List<string>()
            {
                "1.3.6.1.4.1.1206.4.2.3.4.6", //dms.multiCfg.defaultJustificationLine
                "1.3.6.1.4.1.1206.4.2.3.4.7", //dms.multiCfg.defaultJustificationPage
                "1.3.6.1.4.1.1206.4.2.3.4.8", //dms.multiCfg.defaultPageOnTime
                "1.3.6.1.4.1.1206.4.2.3.4.9", //dms.multiCfg.defaultPageOffTime
                "1.3.6.1.4.1.1206.4.2.3.4.10", //dms.multiCfg.defaultCharacterSet
            };

            results.AddRange(messengerSession.Get(objects).Bindings);

            return results;
        }

        /// <summary>
        /// Downloads the dms.vmsCfg Variables
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction</param>
        /// <returns>The List of Variables retireved in the transaction</returns>
        public static List<Variable> GetVmsCfg(SnmpMessenger messengerSession)
        {
            List<string> objects = new List<string>()
            {
                "1.3.6.1.4.1.1206.4.2.3.2.1", //dms.vmsCfg.vmsCharacterHeightPixels (characterBoard)
                "1.3.6.1.4.1.1206.4.2.3.2.2", //dms.vmsCfg.vmsCharacterWidthPixels (characterBoard)
                "1.3.6.1.4.1.1206.4.2.3.2.3", //dms.vmsCfg.vmsSignHeightPixels (amt of charboards / rows high)
                "1.3.6.1.4.1.1206.4.2.3.2.4", //dms.vmsCfg.vmsSignWidthPixels (amt of charboards / rows wide)
                //"1.3.6.1.4.1.1206.4.2.3.2.5", //dms.vmsCfg.vmsHorizontalPitch
                //"1.3.6.1.4.1.1206.4.2.3.2.6", //dms.vmsCfg.vmsVerticalPitch
            };

            List<Variable> results = messengerSession.Get(objects).Bindings;

            return results;
        }

        /// <summary>
        /// Downloads the dms.dmsSignCfg Variables
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction</param>
        /// <returns>The List of Variables retireved in the transaction</returns>
        public static List<Variable> GetDmsSignCfg(SnmpMessenger messengerSession)
        {
            List<string> objects = new List<string>()
            {
                "1.3.6.1.4.1.1206.4.2.3.1.1", //dms.dmsSignCfg.dmsSignAccess
                "1.3.6.1.4.1.1206.4.2.3.1.2", //dms.dmsSignCfg.dmsSignType
                "1.3.6.1.4.1.1206.4.2.3.1.3", //dms.dmsSignCfg.dmsSignHeight
                "1.3.6.1.4.1.1206.4.2.3.1.4", //dms.dmsSignCfg.dmsSignWidth
            };

            List<Variable> results = messengerSession.Get(objects).Bindings;

            objects = new List<string>()
            {
                "1.3.6.1.4.1.1206.4.2.3.1.5", //dms.dmsSignCfg.dmsHorizontalBorder
                "1.3.6.1.4.1.1206.4.2.3.1.6", //dms.dmsSignCfg.dmsVerticalBorder
                "1.3.6.1.4.1.1206.4.2.3.1.7", //dms.dmsSignCfg.dmsLegend
                "1.3.6.1.4.1.1206.4.2.3.1.7", //dms.dmsSignCfg.dmsLegend
                "1.3.6.1.4.1.1206.4.2.3.1.8", //dms.dmsSignCfg.dmsBeaconType
                "1.3.6.1.4.1.1206.4.2.3.1.9", //dms.dmsSignCfg.dmsSignTechnology
            };

            results.AddRange(messengerSession.Get(objects).Bindings);

            return results;
        }

        /// <summary>
        /// Retrieves the powerSource information from a Message Board
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction</param>
        /// <returns>A powerSource value indicating the powerSource of the DMS</returns>
        public static powerSource GetPowerSource(SnmpMessenger messengerSession)
        {
            return (powerSource)messengerSession.Get("1.3.6.1.4.1.1206.4.2.3.9.8.6").Bindings[0].ToInt32();
        }
        
        /// <summary>
        /// Determines the maximum number of pages allowed in the dmsMessageMultiString
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction</param>
        /// <returns>The maximum amount of pages the Mesage Board Supports in a MultiString Message</returns>
        public static int GetMaxPageLength(SnmpMessenger messengerSession)
        {
            //dmsMaxNumberPages
            return messengerSession.Get("1.3.6.1.4.1.1206.4.2.3.4.15").Bindings[0].ToInt32();
        }

        /// <summary>
        /// Determines the maximum number of bytes allowed within the dmsMessageMultiString.
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction</param>
        /// <returns>The Maximum Length of a MultiString Message the Message Board Support in a MutlString Message</returns>
        public static int GetMaxMultiStringLength(SnmpMessenger messengerSession)
        {
            //dmsMaxMultiStringLength
            return messengerSession.Get("1.3.6.1.4.1.1206.4.2.3.4.16").Bindings[0].ToInt32();
        }

        /// <summary>
        /// An indication of the MULTI Tags supported by the device. This
        ///object is a bitmap representation of tag support. When a bit is set (=1),
        ///the device supports the corresponding tag. When a bit is cleared (=0), the
        ///device does not support the corresponding tag.
        ///<Format>
        ///Bit 0 : color background[cbx] / [cbr,g,b]
        ///Bit 1 : color foreground[cfx] / [cfr,g,b]
        ///Bit 2 : flashing[fltxoy] / [floytx]
        ///Bit 3 : font[fox] / [fox,cccc]
        ///Bit 4 : graphic [gn] / [gn,x,y] / [gn,x,y,cccc]
        ///Bit 5 : hexadecimal character[hcx]
        ///Bit 6 : justification line[jlx]
        ///Bit 7 : justification page[jpx]
        ///Bit 8 : manufacturer specific[msx,y]
        ///Bit 9 : moving text[mvtdw,s,r,text]
        ///Bit 10 : new line[nlx]
        ///Bit 11 : new page[np]
        ///Bit 12 : page time[ptxoy]
        ///Bit 13 : spacing character[scx]
        ///Bit 14 : field local time 12 hour[f1]
        ///Bit 15 : field local time 24 hour[f2]
        ///Bit 16 : ambient temperature Celsius[f3]
        ///Bit 17 : ambient temperature Fahrenheit[f4]
        ///Bit 18 : speed km/h[f5]
        ///Bit 19 : speed m/h[f6]
        ///Bit 20 : day of week[f7]
        ///Bit 21 : date of month[f8]
        ///Bit 22 : year 2 digits[f9]
        ///Bit 23 : year 4 digits[f10]
        ///Bit 24 : local time 12 hour AM/PM[f11]
        ///Bit 25 : local time 12 hour am/pm[f12]
        ///Bit 26 : text rectangle [trx,y,w,h]
        ///Bit 27 : color rectangle [crx,y,w,h,z] / [crx,y,w,h,r,g,b]
        ///Bit 28 : Page background [pbz] / [pbr,g,b]
        ///</Format>
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction</param>
        /// <returns>A Variable Which can be BitShifted to Determine if a Variable Supports certain MultiTags</returns>
        public static Variable GetSupportedMultiTags(SnmpMessenger messengerSession)
        {
            //dmsSupportedMultiTags
            return messengerSession.Get("1.3.6.1.4.1.1206.4.2.3.4.14").Bindings[0];
        }

        #endregion

        #endregion

        #region TSS Methods

        #region Diagnostic Functions

        #endregion

        #endregion

        #region ESS Methods



        #region Diagnostic Functions

        #endregion

        #endregion

        #region Camera Methods

        #region Static Commands

        /// <summary>
        /// A Command which directs a NTCIP Camera to stop panning
        /// </summary>
        public static readonly Variable StopPanCommand = new Variable("1.3.6.1.4.1.1206.4.2.7.4.1")
        {
            TypeCode = SnmpType.OctetString,
            Value = { 0x00, 0x00, 0x00, 0x00 }
        };

        /// <summary>
        /// A Command which directs a NTCIP Camera to stop tilting
        /// </summary>
        public static readonly Variable StopTiltCommand = new Variable("1.3.6.1.4.1.1206.4.2.7.4.2")
        {
            TypeCode = SnmpType.OctetString,
            Value = { 0x00, 0x00, 0x00, 0x00 }
        };

        /// <summary>
        /// A Command which directs a NTCIP Camera to stop zooming
        /// </summary>
        public static readonly Variable StopZoomCommand = new Variable("1.3.6.1.4.1.1206.4.2.7.4.3")
        {
            TypeCode = SnmpType.OctetString,
            Value = { 0x00, 0x00, 0x00, 0x00 }
        };

        /// <summary>
        /// A List of Commands which direct a NTCIP Camera to stop all movement (pan, tilt and zoom)
        /// </summary>
        public static List<Variable> StopAllMovementCommand = new List<Variable>()
        {
            StopPanCommand,
            StopTiltCommand,
            StopZoomCommand
        };

        #endregion

        #region Command Encoding Functions

        /// <summary>
        /// Encodes a Camera Command from the given Values
        /// </summary>
        /// <param name="ObjectIdentifier">The Object to bind the PositionRefrence to</param>
        /// <param name="movementSpeed">The speed of movement requested</param>
        /// <param name="movementMode">The mode of movement requested</param>
        /// <param name="postionValue">The position amount requested</param>
        /// <returns>A Variable which contains the encoded command</returns>
        public static Variable EncodeCameraCommand(String ObjectIdentifier, byte? movementSpeed, PositionRefrence movementMode, Int32 positionValue)
        {
            //Build Object
            if (string.IsNullOrEmpty(ObjectIdentifier)) ObjectIdentifier = "1.3.6.1.4.1.1206.4.2.7.4.1";
            Variable result = new Variable();
            result.Identifier = ObjectIdentifier;
            result.TypeCode = SnmpType.OctetString;
            result.Value.Add((byte)movementMode);
            result.Value.Add((byte)movementSpeed);
            //result.Value.Add((byte)Postion);
            //result.Value.AddRange(BasicEncodingRules.EncodeInteger32(postionValue, false, false));
            //while (result.Value.Count < 4) result.Value.Add(0x00);

            if (movementMode == PositionRefrence.StopMovement || movementMode == PositionRefrence.Continious)
            {
                while (result.Value.Count < 4) result.Value.Add(0x00);
            }
            else
            {
                //result.Value.AddRange(BitConverter.GetBytes(Convert.ToInt16(postionValue)));
                result.Value.AddRange(BasicEncodingRules.EncodeInteger32(positionValue, false, false));
            }
            return result;
        }

        /// <summary>
        /// Encodes a Camera Tilt Command from the given Values
        /// </summary>
        /// <param name="movementSpeed">The speed of movement requested</param>
        /// <param name="movementMode">The mode of movement requested</param>
        /// <param name="postionValue">The position amount requested</param>
        /// <returns>A Variable which contains the encoded command</returns>
        public static Variable EncodeTiltCommand(byte? movementSpeed, PositionRefrence movementMode, Int32 positionValue)
        {
            return EncodeCameraCommand("1.3.6.1.4.1.1206.4.2.7.4.2", movementSpeed, movementMode, positionValue);            
        }

        /// <summary>
        /// Encodes a Camera Pan Command from the given Values
        /// </summary>
        /// <param name="movementSpeed">The speed of movement requested</param>
        /// <param name="movementMode">The mode of movement requested</param>
        /// <param name="postionValue">The position amount requested</param>
        /// <returns>A Variable which contains the encoded command</returns>
        public static Variable EncodePanCommand(byte? movementSpeed, PositionRefrence movementMode, Int32 positionValue)
        {
            return EncodeCameraCommand("1.3.6.1.4.1.1206.4.2.7.4.1", movementSpeed, movementMode, positionValue);            
        }

        /// <summary>
        /// Encodes a Camera Zoom Command from the given Values
        /// </summary>
        /// <param name="movementSpeed">The speed of movement requested</param>
        /// <param name="movementMode">The mode of movement requested</param>
        /// <param name="postionValue">The position amount requested</param>
        /// <returns>A Variable which contains the encoded command</returns>
        public static Variable EncodeZoomCommand(byte? movementSpeed, PositionRefrence movementMode, Int32 positionValue)
        {            
            return EncodeCameraCommand("1.3.6.1.4.1.1206.4.2.7.4.3", movementSpeed, movementMode, positionValue);
        }

        #endregion

        #region Camera Movement Functions

        /// <summary>
        /// Instructs the Camera to Stop Panning
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction on</param>
        /// <returns>The ProtocolDataUnit as a result of the transaction</returns>
        public static ProtocolDataUnit StopPan(SnmpMessenger messengerSession)
        {
            return messengerSession.Set(StopPanCommand);
        }

        /// <summary>
        /// Instructs the Camera to Stop Tilting
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction on</param>
        /// <returns>The ProtocolDataUnit as a result of the transaction</returns>
        public static ProtocolDataUnit StopTilt(SnmpMessenger messengerSession)
        {
            return messengerSession.Set(StopTiltCommand);
        }

        /// <summary>
        /// Instructs the Camera to Stop Zooming
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction on</param>
        /// <returns>The ProtocolDataUnit as a result of the transaction</returns>
        public static ProtocolDataUnit StopZoom(SnmpMessenger messengerSession)
        {
            return messengerSession.Set(StopZoomCommand);
        }

        /// <summary>
        /// Instructs the Camera to Stop All Movement (pan, tilt and zoom)
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction on</param>
        /// <returns>The ProtocolDataUnit as a result of the transaction</returns>
        public static ProtocolDataUnit StopAllMovement(SnmpMessenger messengerSession)
        {
            return messengerSession.Set(StopAllMovementCommand);
        }
        
        /// <summary>
        /// Instructs the camera to move to the given presetNumber
        /// </summary>
        /// <param name="presetNumber">The presetNumber the camera should move to</param>
        /// <param name="messengerSession">The MessengerSession to perform this transaction on</param>
        /// <returns>The ProtocolDataUnit returned from the transaction</returns>
        public static ProtocolDataUnit GotoPresetPosition(int presetNumber, SnmpMessenger messengerSession)
        {
           Variable result = new Variable("1.3.6.1.4.1.1206.4.2.7.3.1")
            {
                TypeCode = SnmpType.Integer32,
                Value = BasicEncodingRules.EncodeInteger32(presetNumber, false, false)
            };
           return messengerSession.Set(result);            
        }

        /// <summary>
        /// Stores the current position as the given preset number in the camera
        /// </summary>
        /// <param name="presetNumber">The presetNumber to set in the camera</param>
        /// <param name="messengerSession">The managerSession to perform the transaction on</param>
        /// <returns>The ProtocolDataUnit returned from the transaction</returns>
        public static ProtocolDataUnit StorePresetPosition(int presetNumber, SnmpMessenger messengerSession)
        {
            Variable result = new Variable("1.3.6.1.4.1.1206.4.2.7.3.2")
            {
                TypeCode = SnmpType.Integer32,
                Value = BasicEncodingRules.EncodeInteger32(presetNumber, false, false)
            };
            return messengerSession.Set(result);
        }

        #endregion

        #region Diagnostic Functions

        /// <summary>
        /// Gets the cctvTimeout values from a cctv Camera
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction on</param>
        /// <returns>The variables returned from the transaction</returns>
        public static List<Variable> GetCctvTimeouts(SnmpMessenger messengerSession)
        {
            List<string> objects = new List<string>()
            {
                "1.3.6.1.4.1.1206.4.2.7.2.1", //cctvTimeout.timeoutPan
                "1.3.6.1.4.1.1206.4.2.7.2.2", //cctvTimeout.timeoutTilt
                "1.3.6.1.4.1.1206.4.2.7.2.3", //cctvTimeout.timeoutZoom
                "1.3.6.1.4.1.1206.4.2.7.2.4", //cctvTimeout.timeoutFocus
                "1.3.6.1.4.1.1206.4.2.7.2.5", //cctvTimeout.timeoutIris
            };

            List<Variable> results = messengerSession.Get(objects).Bindings;

            return results;
        }

        /// <summary>
        /// Gets the cctvRange values from a cctv Camera
        /// </summary>
        /// <param name="messengerSession">The messengerSession to perform the transaction on</param>
        /// <returns>The variables returned from the transaction</returns>
        public static List<Variable> GetCctvRanges(SnmpMessenger messengerSession)
        {
            List<string> objects = new List<string>()
            {
                "1.3.6.1.4.1.1206.4.2.7.1.1", //cctvRange.rangeMaximumPreset
                "1.3.6.1.4.1.1206.4.2.7.1.2", //cctvRange.rangePanLeftLimit
                "1.3.6.1.4.1.1206.4.2.7.1.3", //cctvRange.rangePanRightLimit
                "1.3.6.1.4.1.1206.4.2.7.1.4", //cctvRange.rangePanHomePosition
                "1.3.6.1.4.1.1206.4.2.7.1.5", //cctvRange.rangeTrueNorthOffset
            };

            List<Variable> results = messengerSession.Get(objects).Bindings;

            objects = new List<string>()
            {
                "1.3.6.1.4.1.1206.4.2.7.1.6", //cctvRange.rangeTiltUpLimit
                "1.3.6.1.4.1.1206.4.2.7.1.7", //cctvRange.rangeTiltDownLimit
                "1.3.6.1.4.1.1206.4.2.7.1.8", //cctvRange.rangeZoomLimit
                "1.3.6.1.4.1.1206.4.2.7.1.9", //cctvRange.rangeFocusLimit
                "1.3.6.1.4.1.1206.4.2.7.1.10", //cctvRange.rangeIrisLimit
            };

            results.AddRange(messengerSession.Get(objects).Bindings);

            objects = new List<string>()
            {
                "1.3.6.1.4.1.1206.4.2.7.1.11", //cctvRange.rangeMinimumPanStepAngle
                "1.3.6.1.4.1.1206.4.2.7.1.12", //cctvRange.rangeMinimumTiltStepAngle
            };

            results.AddRange(messengerSession.Get(objects).Bindings);

            return results;
        }

        /// <summary>
        /// Determines the cctvRange.rangeMaximumPreset for the given messengerSession
        /// </summary>
        /// <param name="messengerSession">The Snmp Messenger to perform the transaction</param>
        /// <returns>The maximum number of presets supported by the device</returns>
        public static int GetMaxPresets(SnmpMessenger messengerSession)
        {
            //"1.3.6.1.4.1.1206.4.2.7.1.1", //cctvRange.rangeMaximumPreset
            return messengerSession.Get("1.3.6.1.4.1.1206.4.2.7.1.1").Bindings[0].ToInt32();
        }

        #endregion

        #endregion

    }
}
