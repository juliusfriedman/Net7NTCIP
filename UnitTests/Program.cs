using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using ASTITransportation.Utilities;

namespace UnitTests
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {

            UnitTests();

            if (args.Length == 0) return;

            string program = args[0].ToLowerInvariant();

            bool Tcp = false;
            bool pmpp = false;
            ASTITransportation.Snmp.PmppEndPoint pmppep = new ASTITransportation.Snmp.PmppEndPoint();
            
            //check for params
            for(int i = 0, e = args.Length; i < e ; i++)
            {
                string arg = args[i];
                //check for tcp
                if (arg.ToLowerInvariant() == "tcp") Tcp = true;
                //check for pmpp address
                if (arg.ToLowerInvariant() == "pmpp")
                {
                    pmpp = true;
                    try
                    {
                        pmppep.Address = new byte[]{Convert.ToByte(args[i + 1])};
                    }
                    catch 
                    {
                        pmppep.Address = new byte[]{0x05};
                    }

                    try
                    {
                        pmppep.Control = Convert.ToByte(args[i + 2]);
                    }
                    catch 
                    {
                        pmppep.Control = 0x13;
                    }

                    try
                    {
                        pmppep.ProtocolIdentifier = new byte[]{Convert.ToByte(args[i + 3])};
                    }
                    catch 
                    {
                        pmppep.ProtocolIdentifier = new byte[]{0xc1};
                    }
                }
            }

            if (program.Contains("next"))
            {
                #region SnmpGetNext

                ASTITransportation.Snmp.Transport.SnmpMessenger testSession = new ASTITransportation.Snmp.Transport.SnmpMessenger(
                    new System.Net.IPEndPoint(System.Net.IPAddress.Parse(args[1]), Convert.ToInt32(args[2])),//The IPEndPoint of the Target
                    (program.Contains("ntcip") ? 
                        pmpp ?
                            pmppep : new ASTITransportation.Snmp.PmppEndPoint(0x05, 0x13, 0xc1) : (ASTITransportation.Snmp.PmppEndPoint?)null)
                );

                testSession.CommunityName = args[3];
                testSession.SnmpVersion = Convert.ToInt32(args[4]);
                
                if (Tcp) testSession.TrasnportProtocol = System.Net.Sockets.ProtocolType.Tcp;
                else testSession.TrasnportProtocol = System.Net.Sockets.ProtocolType.Udp;
                

                ASTITransportation.Snmp.Variable result = testSession.GetNext(args[5]);

                Console.WriteLine("Identifier: " + result.Identifier);
                Console.WriteLine("Type: " + result.TypeCode);
                Console.WriteLine("Length: " + result.Length);
                Console.WriteLine("Value: ");                                
                Console.WriteLine(BitConverter.ToString(result.Value.ToArray()));

                testSession = null;

                #endregion
            }
            else if (program.Contains("get"))
            {
                #region SnmpGet

                ASTITransportation.Snmp.Transport.SnmpMessenger testSession = new ASTITransportation.Snmp.Transport.SnmpMessenger(
                    new System.Net.IPEndPoint(System.Net.IPAddress.Parse(args[1]), Convert.ToInt32(args[2])),//The IPEndPoint of the Target
                    (program.Contains("ntcip") ? 
                        pmpp ?
                            pmppep : new ASTITransportation.Snmp.PmppEndPoint(0x05, 0x13, 0xc1) : (ASTITransportation.Snmp.PmppEndPoint?)null)
                );

                testSession.CommunityName = args[3];
                testSession.SnmpVersion = Convert.ToInt32(args[4]);

                if (Tcp) testSession.TrasnportProtocol = System.Net.Sockets.ProtocolType.Tcp;
                else testSession.TrasnportProtocol = System.Net.Sockets.ProtocolType.Udp;

                ASTITransportation.Snmp.ProtocolDataUnit result = testSession.Get(args[5]);

                Console.WriteLine(result.ToString());

                Console.WriteLine(); Console.WriteLine();

                Console.WriteLine("Value: ");                
                Console.WriteLine(BitConverter.ToString(result.Bindings[0].Value.ToArray()));

                testSession = null;

                #endregion
            }
            else if (program.Contains("walk"))
            {
                #region SnmpWalk

                ASTITransportation.Snmp.Transport.SnmpMessenger testSession = new ASTITransportation.Snmp.Transport.SnmpMessenger(
                    new System.Net.IPEndPoint(System.Net.IPAddress.Parse(args[1]), Convert.ToInt32(args[2])),//The IPEndPoint of the Target
                    (program.Contains("ntcip") ? 
                        pmpp ?
                            pmppep : new ASTITransportation.Snmp.PmppEndPoint(0x05, 0x13, 0xc1) : (ASTITransportation.Snmp.PmppEndPoint?)null)
                );

                testSession.CommunityName = args[3];
                testSession.SnmpVersion = Convert.ToInt32(args[4]);
                
                if (Tcp) testSession.TrasnportProtocol = System.Net.Sockets.ProtocolType.Tcp;
                else testSession.TrasnportProtocol = System.Net.Sockets.ProtocolType.Udp;

                List<ASTITransportation.Snmp.Variable> tree = testSession.Walk(args[5]);

                foreach (ASTITransportation.Snmp.Variable v in tree)
                {
                    string format = "{0} = {1}: {2}";

                    switch (v.TypeCode)
                    {
                        case ASTITransportation.Snmp.SnmpType.OctetString:
                            Console.WriteLine(string.Format(format, new object[] { v.Identifier, v.TypeCode, '"' + v.ToString() + '"' }));
                            break;
                        case ASTITransportation.Snmp.SnmpType.Integer32:
                            Console.WriteLine(string.Format(format, new object[] { v.Identifier, v.TypeCode, v.ToInt32() }));
                            break;
                        case ASTITransportation.Snmp.SnmpType.Counter32:
                        case ASTITransportation.Snmp.SnmpType.UInteger32:
                            Console.WriteLine(string.Format(format, new object[] { v.Identifier, v.TypeCode, v.ToUInt32() }));
                            break;
                        case ASTITransportation.Snmp.SnmpType.ObjectIdentifier:
                            Console.WriteLine(string.Format(format, new object[] { v.Identifier, v.TypeCode, v.ToObjectIdentifier() }));
                            break;
                        case ASTITransportation.Snmp.SnmpType.TimeTicks:
                            Console.WriteLine(string.Format(format, new object[] { v.Identifier, v.TypeCode, v.ToTimeSpan().ToString() }));
                            break;
                        case ASTITransportation.Snmp.SnmpType.IPAddress:
                            Console.WriteLine(string.Format(format, new object[] { v.Identifier, v.TypeCode, v.ToIPAddress().ToString() }));
                            break;
                        case ASTITransportation.Snmp.SnmpType.Boolean:
                            Console.WriteLine(string.Format(format, new object[] { v.Identifier, v.TypeCode, v.ToBoolean() }));
                            break;
                        default:
                            Console.WriteLine(string.Format(format, new object[] { v.Identifier, v.TypeCode, BitConverter.ToString(v.Value.ToArray()) }));
                            break;
                    }
                }

                testSession = null;

                #endregion
            }
            else if (program.Contains("set"))
            {
                #region SnmpSet

                ASTITransportation.Snmp.Transport.SnmpMessenger testSession = new ASTITransportation.Snmp.Transport.SnmpMessenger(
                    new System.Net.IPEndPoint(System.Net.IPAddress.Parse(args[1]), Convert.ToInt32(args[2])),//The IPEndPoint of the Target
                    (program.Contains("ntcip") ? 
                        pmpp ?
                            pmppep : new ASTITransportation.Snmp.PmppEndPoint(0x05, 0x13, 0xc1) : (ASTITransportation.Snmp.PmppEndPoint?)null)
                );

                testSession.CommunityName = args[3];
                testSession.SnmpVersion = Convert.ToInt32(args[4]);

                if (Tcp) testSession.TrasnportProtocol = System.Net.Sockets.ProtocolType.Tcp;
                else testSession.TrasnportProtocol = System.Net.Sockets.ProtocolType.Udp;

                ASTITransportation.Snmp.Variable setter = new ASTITransportation.Snmp.Variable();
                setter.Identifier = args[5];
                setter.TypeCode = (ASTITransportation.Snmp.SnmpType)Convert.ToInt32(args[6]);

                if (setter.TypeCode == ASTITransportation.Snmp.SnmpType.OctetString)
                {
                    setter.Value = ASTITransportation.Snmp.BasicEncodingRules.EncodeOctetString(args[7], false, false, ASTITransportation.Snmp.SnmpType.OctetString);
                }
                else if (setter.TypeCode == ASTITransportation.Snmp.SnmpType.Integer32)
                {
                    setter.Value = ASTITransportation.Snmp.BasicEncodingRules.EncodeInteger32(Convert.ToInt32(args[7]), false, false, ASTITransportation.Snmp.SnmpType.Integer32);
                }
                else if (setter.TypeCode == ASTITransportation.Snmp.SnmpType.UInteger32)
                {
                    setter.Value = ASTITransportation.Snmp.BasicEncodingRules.EncodeUInteger32(Convert.ToUInt32(args[7]), false, false, ASTITransportation.Snmp.SnmpType.UInteger32);
                }
                else
                {
                    Console.WriteLine("Only Settings OctetString, Integer32 and UInteger32 are supported");
                    return;
                }

                ASTITransportation.Snmp.ProtocolDataUnit result = testSession.Set(setter);

                Console.WriteLine(result.ToString());

                Console.WriteLine(); Console.WriteLine();

                Console.WriteLine("Value Bytes (Decimal)");
                string value = string.Empty;
                foreach (byte b in result.Bindings[0].Value) value += b.ToString();
                Console.WriteLine(value);

                testSession = null;

                #endregion
            }
            else
            {
                Console.WriteLine("No recognized command");
                return;
            }

        }
      
        public static void UnitTests()
        {

            //byte[] me = BitConverter.GetBytes(500);

            //System.Diagnostics.Debug.WriteLine(Convert.ToString(me));

            //WakeTest();

            //ReportingTest();

            //SS125UpdateTest();

            #region Font Serialization
            /*
            foreach (string fontFile in new string[] { "Large (8x10)", "Double (7x7)", "Single (5x7)", "Narrow (4x7)" })
            {
                GenerateNETMFFontCodeFiles(fontFile, null);
            }
            
            return;
            */
            #endregion

            //XAMLTest();
            
            //PDFTest();

            //OtherTests();            

            #region PSCTests

            //SMC1000Test();
            //SMC2000Test();

            #endregion

            #region DateUtilityTest

            //DateUtilityTest();            

            #endregion

            #region DateMatrixTest

            //DateMatrixTest();

            //DateMatrixTest2();

            #endregion

            #region MoringStarTests

            //MorningStarTests();

            #endregion

            #region X3Tests

            //EISX3MessengerTest();

            #endregion

            #region SS125Tests

            //SS125TestLocal();
            //SS125Test273();
            //SS125UpdateTest();

            #endregion

            #region SS105 Tests

            //SS105LocalTest();

            #endregion

            #region SnmpTests


            ASTITransportation.Snmp.Transport.SnmpMessenger testSession = new ASTITransportation.Snmp.Transport.SnmpMessenger(
                new System.Net.IPEndPoint(System.Net.IPAddress.Parse("68.25.136.78"), 12345),//The IPEndPoint of the Target
                null,//new ASTITransportation.Snmp.PmppEndPoint(0x05, 0x13, 0xc1),//The optional PmppEndPoint information                
                "administrator"
            );

            testSession.SnmpVersion = 0;            
            testSession.CommunityName = "public";
            testSession.TrasnportProtocol = System.Net.Sockets.ProtocolType.Udp;

            //WalkTest(testSession);

            //DownloadFonts(testSession, 1, 8);
            
            //ResetTest(testSession);

            //TestVariableEncoding();

            //TestModem(testSession);

            //ESSTest(testSession);

            //TestGlobals(testSession);

            //BlankTest(testSession);

            PollTest(testSession);

            //ResetTest(testSession);

            MessageTest(null, testSession);

            //CameraTestGet(testSession);

            //CameraTestMove(testSession);

            //CameraTestPreset(testSession);

            //CameraTestConfig(testSession);

            //WalkTest(testSession);

            //DownloadChangeableMessages(testSession);

            //DownloadPermanentMessages(testSession);

            //GetBulkTest(testSession);

            //PixelTest(testSession);

            RegTest();

            ControllerGetTest(testSession);

            testSession = null;

            #endregion
        }

        private static void WakeTest()
        {
            var ep = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("166.161.217.205"), 10136);
            ASTITransportation.Snmp.Transport.SnmpMessenger messenger = new ASTITransportation.Snmp.Transport.SnmpMessenger(ep, new ASTITransportation.Snmp.PmppEndPoint(0x05, 0x13, 0xc1), "administrator");
            messenger.TrasnportProtocol = System.Net.Sockets.ProtocolType.Tcp;
            //var gps  = Airlink.Dialogs.GetGPSInformation(messenger);
            ASTITransportation.Snmp.Variable result = ASTITransportation.NTCIP.Dialogs.DmsReset(messenger);
            Console.WriteLine(result.ToInt32());            
        }

        static void WaitForInput()
        {
            Console.Write(Console.ReadLine());
        }

        private static void ReportingTest()
        {
            ////Show that I can dynamically get types
            //foreach (KeyValuePair<string, Type> report in DataStructures.Reporting.ReportProvider.AvailableReports) Console.WriteLine("Key:= " + report.Key + ", Value:= " + report.Value.FullName);
            
            ////This can be done in the EarliestDate and LatestDate properties... this way the result is always in UTC
            ////show earliest report for type
            //DateTime e = DateTime.SpecifyKind( DataStructures.Reporting.ReportProvider.EarliestDate("VolumeReport", "8A03BA9C-1D40-4F01-B2DE-31DF9941E988"), DateTimeKind.Utc);

            //DateTime l = DateTime.SpecifyKind( DataStructures.Reporting.ReportProvider.LatestDate("VolumeReport", "8A03BA9C-1D40-4F01-B2DE-31DF9941E988"), DateTimeKind.Utc);
            

            ////show that I can create report information
            //DataStructures.Reporting.ReportInformation ri = new DataStructures.Reporting.ReportInformation(DateTime.Now.AddMonths(-6), DateTime.Now.AddMonths(6));
            ////Add a entity 
            //ri.Add("8A03BA9C-1D40-4F01-B2DE-31DF9941E988");
            ////ri.Add("9A03BA9C-1D40-4F01-B2DE-31DF9941E989");     
            ////Make a volume Report
            //DataStructures.Reporting.VolumeReport<int> vrInt = new DataStructures.Reporting.VolumeReport<int>(ri);
            ////make some yearly data belonging to that VolumeReport
            ////DataStructures.Reporting.YearlyReportData<int> yrdInt = new DataStructures.Reporting.YearlyReportData<int>(vrInt, DateTime.Now);
            ////WaitForInput();            

            
            ////Problems with serialization
            /////Maybe redo design...

            ////test cast to a base type
            //DataStructures.Reporting.BaseReportData brd = vrInt.GetReportData("8A03BA9C-1D40-4F01-B2DE-31DF9941E988", vrInt.StartDate);
            
            //System.IO.File.WriteAllText("DataJson.txt", brd.ToJson());

            //System.IO.File.WriteAllText("ReportJson.txt", vrInt.ToJson());

            //            //Get the binary data
            //byte[] test = vrInt.Serialize();
            ////make a report from the binary
            //DataStructures.Reporting.BaseReport br = DataStructures.Reporting.ReportProvider.FromBinary(test);            
            ////get the exact report via a cast
            //DataStructures.Reporting.VolumeReport<int> fromStream = (DataStructures.Reporting.VolumeReport<int>)br;
            ////get the exact report via constructor
            ////DataStructures.Reporting.VolumeReport<int> fromClr = new DataStructures.Reporting.VolumeReport<int>(test);

            ////Write the data to the db
            ////WaitForInput();
            
            ////show insert
            ////fromClr.StoreToDatabase();

            ////show update
            ////fromClr.StoreToDatabase();
        }

        private static void ControllerGetTest(ASTITransportation.Snmp.Transport.SnmpMessenger testSession)
        {
            DateTime start = DateTime.Now;
            int error = 0;
            int counter = 0;
            ///simulate 10 udp clients sending a message
            for (int i = 0; i < 1000000; ++i, ++counter)
            {

                try
                {
                    //Sleep for 5 seconds if this is a even number
                    //if ((i % 2) == 0) System.Threading.Thread.Sleep(5000);
                 
                    ASTITransportation.Snmp.ProtocolDataUnit pduResponse = testSession.Get("1.2.3.4");
                    
                    if (pduResponse.ErrorStatus != ASTITransportation.Snmp.ErrorStatus.NoError ||
                        pduResponse.PduType != ASTITransportation.Snmp.SnmpType.GetResponsePdu)
                    {
                        i--;
                        error++;
                        Console.WriteLine("Error Encounted: totalErrors = " + error + " totalSuccess = " + i);
                    }
                    else Console.WriteLine("Successful Request: totalErrors = " + error + " totalSuccess = " + i);
                    pduResponse = null;

                }

                catch
                {
                    Console.WriteLine("Exception Encounted: totalErrors = " + error + " totalSuccess = " + i);
                    i--;
                    error++;
                }

              /*  new System.Threading.Thread(new System.Threading.ThreadStart(delegate
                {
                    try
                    {
                        ASTITransportation.Snmp.ProtocolDataUnit pduResponse = testSession.Get("1.2.3.4");
                        if (pduResponse.ErrorStatus != ASTITransportation.Snmp.ErrorStatus.NoError ||
                            pduResponse.PduType != ASTITransportation.Snmp.SnmpType.GetResponsePdu)
                        {
                            i--;
                            error++;
                        }
                        pduResponse = null;
                    }

                    catch
                    {
                        i--;
                        error++;
                    }
                })).Start();*/

            }


            TimeSpan total = DateTime.Now - start;

            double averageGetTime = total.TotalSeconds / 1000000;

        }

        private static void SS125UpdateTest()
        {

            var test = new Wavetronix.SS125.SensorLoginMessage(Wavetronix.SS125.AccessType.Write);
            string p = test.DecodePassword("^9HY?`ch+w/'");
            string tp = test.EncodePassword("SS125 U100000896" + " 2010-08-05");
            string tp2 = test.EncodePassword("SS125 U100000896");
            string tp3 = test.EncodePassword("ss125 2010-08-05");            

            Wavetronix.SS125.SS125Messenger messenger = new Wavetronix.SS125.SS125Messenger(
                System.Net.Sockets.ProtocolType.Tcp,
                new System.Net.IPEndPoint(System.Net.IPAddress.Parse("166.141.173.177"), 10478));
            /*
            var result = messenger.SendMessage(new Wavetronix.SS125.IsPasswordEnabledMessage());

            var pwd = new Wavetronix.SS125.IsPasswordEnabledMessage(result[0]);

            result = messenger.SendMessage(new Wavetronix.SS125.RetrieveTempPasswordMessage());

            var tpwd = new Wavetronix.SS125.RetrieveTempPasswordMessage(result[0]);

            var disable = new Wavetronix.SS125.PasswordEnableDisableMessage(Wavetronix.SS125.AccessType.Write, false);

            result = messenger.SendMessage(new Wavetronix.SS125.IsPasswordEnabledMessage());

            pwd = new Wavetronix.SS125.IsPasswordEnabledMessage(result[0]);
            */

            messenger.UpgradeFirmware(true);

            messenger.Close();
        }

        private static void VaisalaTest()
        {
            string testString = @"
2005-06-08 14:05,01,M14,DSC
01 -1.5;02 20;03 -3.0;14 23.8;30 -2.0;36 206;42 0.35;61 00;
66 206;68 0.15;71 00;72 0.00;73 0.05;74 0.35;
=";

            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(testString);

            Vaisala.Message test = new Vaisala.Message(bytes);

            Vaisala.Messenger mesenger = new Vaisala.Messenger(Vaisala.Messenger.CommandMode.Network, 
                System.Net.Sockets.ProtocolType.Udp,
                new System.Net.IPEndPoint(System.Net.IPAddress.Parse("166.154.75.248"), 2001));

            string status = mesenger.GetStatus();

            string ver = mesenger.GetVersion();

            string par = mesenger.GetPar();

            int overhead = mesenger.TotalRecievedBytes + mesenger.TotalSentBytes;

            mesenger.Close();

        }

        private static void SMC1000Test()
        {
            PrecisionSolarControls.SMC1000Messenger messenger =
                new PrecisionSolarControls.SMC1000Messenger(
                    new System.Net.IPEndPoint(System.Net.IPAddress.Parse("68.25.131.10"), 12345), 
                    System.Net.Sockets.ProtocolType.Udp);

            #region Run Sequence

            bool ranSequence = messenger.RunSequence(1);

            #endregion

            #region Get Current Message

            PrecisionSolarControls.SMC1000Messenger.Message currentMessage = messenger.QueryMessage();

            #endregion

            #region Get Message From SMC By Message number

            PrecisionSolarControls.SMC1000Messenger.Message aMessage = messenger.GetMessage(67);
            PrecisionSolarControls.SMC1000Messenger.Message aMessage2 = messenger.GetMessage(64);
            PrecisionSolarControls.SMC1000Messenger.Message aMessage3 = messenger.GetMessage(251);

            #endregion

            #region Display New Message

            PrecisionSolarControls.SMC1000Messenger.Message newMsg = new PrecisionSolarControls.SMC1000Messenger.Message();

            newMsg.AddPage(new string[] { "LINE1", "LINE2", "LINE3" }, PrecisionSolarControls.SMC1000Messenger.BlinkCode.Line1and3Blink, 2);
            newMsg.AddPage(new string[] { "LINE1", "LINE2", "LINE3" }, PrecisionSolarControls.SMC1000Messenger.BlinkCode.Line2Blink, 2);
            newMsg.AddPage(new string[] { "LINE1", "------->", "LINE3" }, PrecisionSolarControls.SMC1000Messenger.BlinkCode.Line2Sequencing, 2);

            bool displayedMessage = messenger.DisplayMessage(newMsg);

            #endregion

            #region Get Sequence

            aMessage = messenger.GetSequence(1);
            aMessage = messenger.GetSequence(10);
            aMessage = messenger.GetSequence(95);
            aMessage = messenger.GetSequence(100);
            //aMessage = messenger.GetSequence(101);
            //aMessage = messenger.GetSequence(102);

            #endregion

            currentMessage = messenger.QueryMessage();

            #region Put Message

            bool putMessage = messenger.PutMessage(251, new string[] { "<-------", "<-------", "LINE3" });
            putMessage = messenger.PutMessage(252, new string[] { "LINE1", "------->", "LINE3" });
            putMessage = messenger.PutMessage(253, new string[] { "LINE1", "------->", "------->" });

            #endregion         

            #region Put, Query and Run Sequence

            bool putSequence = messenger.PutSequence(new PrecisionSolarControls.SMC1000Messenger.SequencePutCommand()
            {
                SequenceNumber = 100,
                commandParts = new List<int[]>() { 
                    //new int[]{63, (int)PrecisionSolarControls.SMC1000Messenger.BlinkCode.AllLinesBlink, 20 },
                    //new int[]{64, (int)PrecisionSolarControls.SMC1000Messenger.BlinkCode.Line2Blink, 20 },
                    //new int[]{65, (int)PrecisionSolarControls.SMC1000Messenger.BlinkCode.NoLinesBlink, 20 },
                    new int[]{251, (int)PrecisionSolarControls.SMC1000Messenger.BlinkCode.Line1and2Sequencing, 20 },
                    new int[]{252, (int)PrecisionSolarControls.SMC1000Messenger.BlinkCode.Line2Sequencing, 20 },
                    new int[]{253, (int)PrecisionSolarControls.SMC1000Messenger.BlinkCode.Line2and3Sequencing, 20 }
                }
            });

            aMessage = messenger.GetSequence(100);

            ranSequence = messenger.RunSequence(100);

            #endregion

            currentMessage = messenger.QueryMessage();

            messenger.Close();

            messenger = null;
        }

        private static void SMC2000Test()
        {
            PrecisionSolarControls.SMC2000Messenger messenger =
                new PrecisionSolarControls.SMC2000Messenger(
                    new System.Net.IPEndPoint(System.Net.IPAddress.Parse("1.2.3.4"), 12345),
                    System.Net.Sockets.ProtocolType.Udp);


            PrecisionSolarControls.SMC2000Messenger.Message newMessage = new PrecisionSolarControls.SMC2000Messenger.Message();
            newMessage.AddPage(new string[] { "LINE1", "LINE2", "LINE3" }, PrecisionSolarControls.SMC2000Messenger.BlinkCode.Blinking, 100);                        

            PrecisionSolarControls.SMC2000Messenger.Message currentMessage = messenger.QueryMessage();

            bool ranSequence = messenger.RunSequence(0);

            currentMessage = messenger.QueryMessage();

            messenger.PutMessage(0, new string[] { "LINE1", "LINE2", "LINE3" });

            messenger.Close();

            messenger = null;
        }
        
        private static void DateUtilityTest()
        {
            ////int occurs = ASTITransportation.DataStructures.DateUtility.OccurancesOfDayInMonth(DayOfWeek.Friday);
            ////int occursly = ASTITransportation.DataStructures.DateUtility.OccurancesOfDayInMonth(DayOfWeek.Friday, DateTime.Now.Month, 2009);
            ////int occursybly = ASTITransportation.DataStructures.DateUtility.OccurancesOfDayInMonth(DayOfWeek.Friday, DateTime.Now.Month, 2008); 
        }

        private static void MorningStarTests()
        {            
            //ModbusMessenger used to transport requests from a computer to any Modbus Device
            // Instantiate with IPEndpoint(IP, Port), 
            // TransportProtocol {Tcp, Udp}, 
            // TransportEncoding{Binary, ASCII, IpWithCrc, IpWithoutCrc}
            ASTITransportation.Modbus.ModbusMessenger messenger = new ASTITransportation.Modbus.ModbusMessenger(
                new System.Net.IPEndPoint(System.Net.IPAddress.Parse("68.25.244.129"), 502), 
                System.Net.Sockets.ProtocolType.Tcp, 
                ASTITransportation.Modbus.ModbusMessenger.TransportEncoding.Binary);
            
            //Tell the messenger which unit it should address packets to
            messenger.UnitId = 1;

            //use a managed dialog to retrieve the batteryVoltage
            double batteryVoltage = MorningStar.TS.Dialogs.GetBatteryVoltage(messenger);
            
            //use a managed dialog to retrieve the systemVoltage
            double systemVoltage = MorningStar.TS.Dialogs.GetSystemVoltage(messenger);

            //use a managed dialog to retrieve the PowerSourceVoltage
            double powerSourceVoltage = MorningStar.TS.Dialogs.GetPowerSourceVoltage(messenger);

            //use a managed dialog to retrieve the TotalAmpHours
            double tah = MorningStar.TS.Dialogs.GetTotalAmpHours(messenger);

            //use a managed dialog to retrieve the ResetableAmpHours
            double rah = MorningStar.TS.Dialogs.GetResetableAmpHours(messenger);

            //use a managed dialog to retrieve the KillowattHours
            double kwh = MorningStar.TS.Dialogs.GetKilowattHours(messenger);

            //use a managed dialog to retrieve the minimumBatteryVoltage
            double minbv = MorningStar.TS.Dialogs.GetMinimumBatteryVoltage(messenger);

            //use a managed dialog to retrieve the maximumBatteryVoltage
            double maxbv = MorningStar.TS.Dialogs.GetMaximumBatteryVoltage(messenger);

            //use a managed dialog to retrieve the chargingCurrent
            double chargeCurrent = MorningStar.TS.Dialogs.GetChargingCurrent(messenger);

            //use a managed dialog to retrieve the loadCurrent
            double loadCurrent = MorningStar.TS.Dialogs.GetLoadCurrent(messenger);

            //use a managed dialog to retrieve SerialNumber
            string serialNumber = MorningStar.TS.Dialogs.GetSerialNumber(messenger);

            //use a managed dialog to retrieve the hardwareVersion
            string hardwareVersion = MorningStar.TS.Dialogs.GetHardwareVersion(messenger);

            //use a managed dialog to retrieve the daysSinceBatteryService
            int daysSinceService = MorningStar.TS.Dialogs.GetDaysSinceLastBatteryService(messenger);

            //use a managed dialog to retrieve the daysBetweenBatteryServices
            int daysBetweenService = MorningStar.TS.Dialogs.GetDaysBetweenBatteryServiceIntervals(messenger);

            //use a managed dialog to retrieve the vendorName
            string vendor = MorningStar.TS.Dialogs.GetVendorName(messenger);

            //use a managed dialog to retrieve the productCode
            string prodCode = MorningStar.TS.Dialogs.GetProductCode(messenger);

            //use a managed dialog to retrieve the softwareVersion
            string swVer = MorningStar.TS.Dialogs.GetSoftwareVersion(messenger);

            //use a managed dialog to reset the chargeController
            //bool reset = MorningStar.Dialogs.ResetController(messenger);

            //use a managed dialog to retrieve the faultReading
            MorningStar.TS.Dialogs.FaultReading faults = MorningStar.TS.Dialogs.GetFaults(messenger);            

            //use a managed dialog to retrieve the alarmReading
            MorningStar.TS.Dialogs.AlarmReading alarms = MorningStar.TS.Dialogs.GetAlarms(messenger);

            //use a managed dialog to retrieve the controlmode
            MorningStar.TS.Dialogs.ControlMode controlMode = MorningStar.TS.Dialogs.GetControlMode(messenger);

            //you need to know the controlMode to describe the ControlState
            MorningStar.TS.Dialogs.ControlState controlState = MorningStar.TS.Dialogs.GetControlState(messenger, controlMode);

            //alternatively if you think your 1337 and you dont need to know about the control mode
            int ctrlState = MorningStar.TS.Dialogs.GetControlState(messenger);

            //if the reminder was reset
            if (MorningStar.TS.Dialogs.ResetBatteryServiceReminder(messenger))
            {
                ///get the values to ensure it was reset
                int daysBetweena = MorningStar.TS.Dialogs.GetDaysBetweenBatteryServiceIntervals(messenger);
                int daysSinceServicea = MorningStar.TS.Dialogs.GetDaysSinceLastBatteryService(messenger);
            }            

            //if the counter was reset
            if (MorningStar.TS.Dialogs.ClearResetableAmpHours(messenger))
            {
                //get the values to ensure it was reset
                double atah = MorningStar.TS.Dialogs.GetTotalAmpHours(messenger);
            }

            //I think im done with the messenger for now
            messenger.Close();

        }

        private static void GetBulkTest(ASTITransportation.Snmp.Transport.SnmpMessenger testSession)
        {
            //performs a Snmp GetBulk Request for the given Oid, nonRepeaters, and maxRepetions
            ASTITransportation.Snmp.ProtocolDataUnit result = testSession.GetBulk("1.3.6.1.4.1.1206.4.2.3.5.8.1.3.3", 1, 10);
        }

        private static void PixelTest(ASTITransportation.Snmp.Transport.SnmpMessenger testSession)
        {
            //Get a list of the bad pixels on the board by performing a pixel test
            List<ASTITransportation.Snmp.Variable> results = ASTITransportation.NTCIP.Dialogs.GetBadPixelLocations(testSession);
            //Use the managed dialogs to obtain a text report which can be shown on screen or in a email
            string report = ASTITransportation.NTCIP.Dialogs.PixelReport(results, testSession);
            Console.Write(report);
        }

        public static void EISX3MessengerTest()
        {       
            //Instantiate a messenger which will communicate with the X3/K3 Sensor
            //Instantiate with IPEndPoint(IPAddres,Port)
            //TransportProtocol{Tcp, Udp}
            EIS.X3Messenger messenger = new EIS.X3Messenger(
                new System.Net.IPEndPoint(System.Net.IPAddress.Parse("68.25.213.25"), 12345),
                System.Net.Sockets.ProtocolType.Udp);
            
            //Here the messenger already has the GeneralConfiguration, SoftwareInformation, and BinInformation

            //Do a dataRequest to retrieve the latest lane information
            messenger.DataRequest();

            //Switch to Tcp Transport for whatever reason
            messenger.TransportProtocol = System.Net.Sockets.ProtocolType.Tcp;

            //Here the messenger re-obtains has the GeneralConfiguration, SoftwareInformation, and BinInformation

            //Do a dataRequest to retrieve the latest lane information over Tcp
            messenger.DataRequest();

            //Close the ports used by the messenger, and the logs the messenger was keeping
            messenger.Close();

            //get rid of the messengerInstance
            messenger = null;
        }
       
        public static void SS125TestLocal()
        {
            //Instantiate a messenger to communicate with the SS125
            //TranportProtocol{Tcp, Udp}
            //IPEndPoint(IPAddress, Port)
            Wavetronix.SS125.SS125Messenger messenger = new Wavetronix.SS125.SS125Messenger(
                System.Net.Sockets.ProtocolType.Tcp,
                new System.Net.IPEndPoint(System.Net.IPAddress.Parse("68.25.213.25"), 12345));

            //Here the messenger already has the GeneralConfiguraion, BinInformation, SoftwareInformation, MemoryInformation, and the Single Sample from the most recent index

            //Tell the messenger to download data between two dates
            //messenger.DownloadData(new Wavetronix.SS125.Timestamp(2010, 2, 1, 0, 0, 0, 0), new Wavetronix.SS125.Timestamp(2010, 2, 1, 0, 10, 0, 0));
            
            //Tell the messenger to download data between two dates
            //messenger.DownloadData(new Wavetronix.SS125.Timestamp(2010, 2, 10, 0, 0, 0, 0), new Wavetronix.SS125.Timestamp(2010, 2, 10, 0, 10, 0, 0));  

            //Tell the messenger to download data between two dates
            messenger.DownloadData(new Wavetronix.SS125.Timestamp(2010, 2, 10, 0, 0, 0, 0), new Wavetronix.SS125.Timestamp(2010, 2, 10, 0, 5, 0, 0));

            //Tell the messenger to download data between two dates
            messenger.DownloadData(new Wavetronix.SS125.Timestamp(2010, 2, 10, 0, 5, 0, 0), new Wavetronix.SS125.Timestamp(2010, 2, 10, 0, 10, 0, 0));

            //Tell the messenger to download data between two dates
            //messenger.DownloadData(new Wavetronix.SS125.Timestamp(2010, 6, 1, 0, 0, 0, 0), new Wavetronix.SS125.Timestamp(2010, 6, 1, 1, 0, 0, 0));

            //writes a WaveTronix Compatible Logfile to the file @ path given
            messenger.WriteToLog("test.txt");
        }

        public static void SS125Test273()
        {
            //Instantiate a messenger
            Wavetronix.SS125.SS125Messenger messenger = new Wavetronix.SS125.SS125Messenger(System.Net.Sockets.ProtocolType.Tcp,
                new System.Net.IPEndPoint(System.Net.IPAddress.Parse("166.141.173.177"), 10478));

            //Tell the messenger to update the dataInterval to 60 seconds
            //messenger2.UpdateDataInterval(60);     

            //Tell the messenger to download data between two dates
            //messenger.DownloadData(new Wavetronix.SS125.Timestamp(2010, 5, 28, 0, 0, 0, 0), new Wavetronix.SS125.Timestamp(2010, 5, 28, 0, 10, 0, 0));
            
            //Tell the messenger to download data between two dates
            messenger.DownloadData(DateTime.Parse("5/28/2010 12:00 AM"), DateTime.Parse("5/28/2010 12:10 AM"));

            //Tell the messenger to download data between two dates
            messenger.DownloadData(new Wavetronix.SS125.Timestamp(2010, 6, 10, 0, 0, 0, 0), new Wavetronix.SS125.Timestamp(2010, 6, 10, 0, 10, 0, 0));

            //Tell the write the dataDictionary to the Wavetronix style logfile @ name given 
            messenger.WriteToLog("test");
        }

        public static void SS105LocalTest()
        {
            //Instantiate a SS105 Messenger
            Wavetronix.SS105.SS105Messenger messenger = new Wavetronix.SS105.SS105Messenger(
                new System.Net.IPEndPoint(System.Net.IPAddress.Parse("68.25.213.25"), 12345), 
                System.Net.Sockets.ProtocolType.Udp);
            
            //Tell the messenger to download data between two indexes
            //messenger.DownloadData(10, 0);

            //Tell the messenger to download 2234 index's from flash memory
            messenger.DownloadFlashData();

            //Tell the messenger to download 246 index's from volatile memory
            messenger.DownloadVolatileData();

            //Tell the messenger to download all data available from the ss105
            messenger.DownloadAllData();

            //close the ports in use by the messenger and clear the dataDictionary and counters used by the messenger
            messenger.Close();

        }       

        static void TestVariableEncoding()
        {
            //Pdu Checksum 7E test
            List<byte> encoded = new ASTITransportation.Snmp.ProtocolDataUnit()
            {
                Version = 1,
                RequestId = 8,
                CommunityName = "public",
                PduType = ASTITransportation.Snmp.SnmpType.GetRequestPdu,
                Bindings = { new ASTITransportation.Snmp.Variable("1.3.6.1.4.1.1206.4.2.3.5.8.1.9.3.1") }
            }.ToPacket(new ASTITransportation.Snmp.PmppEndPoint(0x05, 0x13, 0xc1));

            ASTITransportation.Snmp.SnmpExtensions.PmppDecode(encoded);

            //Tests how a Integer is encoded with the value 1
            ASTITransportation.Snmp.Variable testi = new ASTITransportation.Snmp.Variable(ASTITransportation.Snmp.BasicEncodingRules.EncodeInteger32(1));

            //Tests how a Octet string is encoded with the value "test"
            ASTITransportation.Snmp.Variable tests = new ASTITransportation.Snmp.Variable(ASTITransportation.Snmp.BasicEncodingRules.EncodeOctetString("test"));

            //Tests how a ProtocolDataUnit is constructed inline
            ASTITransportation.Snmp.ProtocolDataUnit testPdu = new ASTITransportation.Snmp.ProtocolDataUnit()
            {
                CommunityName = "administrator",
                RequestId = 1,
                PduType = ASTITransportation.Snmp.SnmpType.GetRequestPdu,
                Bindings = { new ASTITransportation.Snmp.Variable("1.3.6.1.4.1.1206.4.2.7.2.4"), new ASTITransportation.Snmp.Variable("1.3.6.1.4.1.1206.4.2.7.2.4", ASTITransportation.Snmp.SnmpType.OctetString, ASTITransportation.Snmp.BasicEncodingRules.EncodeOctetString("test", false, false)) }
            };

            //Tests how a ProtocolDataUnit is serialized to a byte array
            byte[] bytes = testPdu.ToPacket().ToArray();

            //Tests how a ProtocolDataUnit is contstructed from binary Data 
            ASTITransportation.Snmp.ProtocolDataUnit fromTest = new ASTITransportation.Snmp.ProtocolDataUnit(bytes);

            //Get a string which shows the ProtcolDataUnit and it's values in a JSON style markup
            string test = fromTest.ToString();

            //Write the test string to the console
            Console.WriteLine(test);

        }

        static void TestGlobals(ASTITransportation.Snmp.Transport.SnmpMessenger messengerSession)
        {
            //retrieves a few variables from the GlobalModuleTable of a NTCIP Compliant Device
            //"1.3.6.1.4.1.1206.4.2.6.1.3.1.3.1", //global.globalConfiguration.globalModuleTable.moduleTableEntry.moduleMake.1
            //"1.3.6.1.4.1.1206.4.2.6.1.3.1.4.1", //global.globalConfiguration.globalModuleTable.moduleTableEntry.moduleModel.1
            //"1.3.6.1.4.1.1206.4.2.6.1.3.1.5.1", //global.globalConfiguration.globalModuleTable.moduleTableEntry.moduleVersion.1
            //"1.3.6.1.4.1.1206.4.2.6.1.3.1.6.1", //global.globalConfiguration.globalModuleTable.moduleTableEntry.moduleType.1
            List<ASTITransportation.Snmp.Variable> results = ASTITransportation.NTCIP.Dialogs.GetGlobals(messengerSession);
            return;
        }

        static void TestModem(ASTITransportation.Snmp.Transport.SnmpMessenger messengerSession)
        {

            //These should all be self explanitory            
            //If the modem does not support the feature being queried a NoSuchInstance
            //will appear as the result of the query through the Dialog
            ASTITransportation.Snmp.Variable dateTime = Airlink.Dialogs.GetModemNetworkTime(messengerSession);

            DateTime modemTime = DateTime.SpecifyKind(DateTime.Parse(dateTime.ToString()), DateTimeKind.Utc).ToLocalTime();

            ASTITransportation.Snmp.Variable networkType = Airlink.Dialogs.GetModemNetworkType(messengerSession);

            if (networkType.TypeCode != ASTITransportation.Snmp.SnmpType.NoSuchInstance)
            {
                string typeOfnetwork = networkType.ToString();
            }

            ASTITransportation.Snmp.Variable networkProvider = Airlink.Dialogs.GetModemNetworkProvider(messengerSession);

            if (networkProvider.TypeCode != ASTITransportation.Snmp.SnmpType.NoSuchInstance)
            {
                string provider = networkProvider.ToString();
            }

            ASTITransportation.Snmp.Variable modemModel = Airlink.Dialogs.GetModemModel(messengerSession);

            string modem = modemModel.ToString();

            ASTITransportation.Snmp.Variable modemSoftwareVersion = Airlink.Dialogs.GetModemSoftwareVersion(messengerSession);

            string swInfo = modemSoftwareVersion.ToString();

            ASTITransportation.Snmp.Variable signalStrength = Airlink.Dialogs.GetRSSI(messengerSession);

            int RSSI = signalStrength.ToInt32();

            List<ASTITransportation.Snmp.Variable> GPSInfo = Airlink.Dialogs.GetGPSInformation(messengerSession);

            bool hasSatelliteFix = GPSInfo[0].ToBoolean();

            int satelliteCount = GPSInfo[1].ToInt32();

            List<ASTITransportation.Snmp.Variable> locationInfo = Airlink.Dialogs.GetLatitudeLongitude(messengerSession);

            float lat = (float)locationInfo[0].ToInt32() / 100000;

            float lng = (float)locationInfo[1].ToInt32() / 100000;

            ASTITransportation.Snmp.Variable voltageInfo = Airlink.Dialogs.GetVoltage(messengerSession);

            if (voltageInfo.TypeCode != ASTITransportation.Snmp.SnmpType.NoSuchInstance)
            {
                float voltage = float.Parse(voltageInfo.ToString());
            }

            List<ASTITransportation.Snmp.Variable> serialInfo = Airlink.Dialogs.GetSerialSettings(messengerSession);

            string[] tokens = serialInfo[0].ToString().Split(',');

            int baudRate = int.Parse(tokens[0]);

            int dataBits = int.Parse(tokens[1][0].ToString());

            char parity = tokens[1][1];

            int stopBits = int.Parse(tokens[1][2].ToString());

            bool flowControlenabled = serialInfo[1].ToBoolean();
            
        }

        static void CameraTestGet(ASTITransportation.Snmp.Transport.SnmpMessenger messengerSession)
        {
            //Retrieves some obects which a 1205 camera should support
            List<String> objects = new List<string>();
            objects.Add("1.3.6.1.4.1.1206.4.2.7.2.4");//CCTV-MIB1::timeoutFocus
            objects.Add("1.3.6.1.4.1.1206.4.2.7.2.5");//CCTV-MIB1::timeoutIris
            objects.Add("1.3.6.1.4.1.1206.4.2.7.2.6");//CCTV-MIB1::cctvTimeout.6 (Zoom)

            ASTITransportation.Snmp.ProtocolDataUnit result = messengerSession.Get(objects);

        }

        static void CameraTestConfig(ASTITransportation.Snmp.Transport.SnmpMessenger messengerSession)
        {
            //Gets the CCtv Range Values from a 1205 Camera
            List<ASTITransportation.Snmp.Variable> ranges = ASTITransportation.NTCIP.Dialogs.GetCctvRanges(messengerSession);
            //Gets the CCtv Timeouts Values from a 1205 Camera
            List<ASTITransportation.Snmp.Variable> timeouts = ASTITransportation.NTCIP.Dialogs.GetCctvTimeouts(messengerSession);
        }

        static void CameraTestMove(ASTITransportation.Snmp.Transport.SnmpMessenger messengerSession)
        {

            ASTITransportation.Snmp.ProtocolDataUnit result;

            #region Delta Pan

            ASTITransportation.Snmp.Variable commandLeft = ASTITransportation.NTCIP.Dialogs.EncodePanCommand(128, ASTITransportation.NTCIP.PositionRefrence.Delta, 10000);

            ASTITransportation.Snmp.Variable commandRight = ASTITransportation.NTCIP.Dialogs.EncodePanCommand(127, ASTITransportation.NTCIP.PositionRefrence.Delta, 10000);

            result = messengerSession.Set(commandLeft);

            result = messengerSession.Set(commandRight);

            #endregion

            #region Delta Tilt

            ASTITransportation.Snmp.Variable commandUp = ASTITransportation.NTCIP.Dialogs.EncodeTiltCommand(127, ASTITransportation.NTCIP.PositionRefrence.Delta, 1000);

            ASTITransportation.Snmp.Variable commandDown = ASTITransportation.NTCIP.Dialogs.EncodeTiltCommand(128, ASTITransportation.NTCIP.PositionRefrence.Delta, 1000);

            result = messengerSession.Set(commandUp);

            result = messengerSession.Set(commandDown);

            #endregion

            #region Delta Zoom

            ASTITransportation.Snmp.Variable commandIn = ASTITransportation.NTCIP.Dialogs.EncodeZoomCommand(127, ASTITransportation.NTCIP.PositionRefrence.Delta, 10);

            ASTITransportation.Snmp.Variable commandout = ASTITransportation.NTCIP.Dialogs.EncodeZoomCommand(128, ASTITransportation.NTCIP.PositionRefrence.Delta, 1000);

            result = messengerSession.Set(commandIn);

            result = messengerSession.Set(commandout);

            #endregion

            #region Continious Pan

            ASTITransportation.Snmp.Variable commandLeftC = ASTITransportation.NTCIP.Dialogs.EncodePanCommand(128, ASTITransportation.NTCIP.PositionRefrence.Continious, 0);

            result = messengerSession.Set(commandLeftC);
            result = ASTITransportation.NTCIP.Dialogs.StopPan(messengerSession);

            ASTITransportation.Snmp.Variable commandRightC = ASTITransportation.NTCIP.Dialogs.EncodePanCommand(127, ASTITransportation.NTCIP.PositionRefrence.Continious, 0);

            result = messengerSession.Set(commandRightC);
            result = ASTITransportation.NTCIP.Dialogs.StopPan(messengerSession);

            #endregion

            #region Continious Tilt

            ASTITransportation.Snmp.Variable commandUpC = ASTITransportation.NTCIP.Dialogs.EncodeTiltCommand(127, ASTITransportation.NTCIP.PositionRefrence.Delta, 1000);

            result = messengerSession.Set(commandUpC);
            result = ASTITransportation.NTCIP.Dialogs.StopTilt(messengerSession);

            ASTITransportation.Snmp.Variable commandDownC = ASTITransportation.NTCIP.Dialogs.EncodeTiltCommand(128, ASTITransportation.NTCIP.PositionRefrence.Delta, 1000);

            result = messengerSession.Set(commandDownC);
            result = ASTITransportation.NTCIP.Dialogs.StopTilt(messengerSession);

            #endregion

            #region Continious Zoom

            ASTITransportation.Snmp.Variable commandInC = ASTITransportation.NTCIP.Dialogs.EncodeZoomCommand(127, ASTITransportation.NTCIP.PositionRefrence.Continious, 1000);

            result = messengerSession.Set(commandInC);
            result = ASTITransportation.NTCIP.Dialogs.StopZoom(messengerSession);

            ASTITransportation.Snmp.Variable commandOutC = ASTITransportation.NTCIP.Dialogs.EncodeZoomCommand(128, ASTITransportation.NTCIP.PositionRefrence.Continious, 1000);

            result = messengerSession.Set(commandOutC);
            result = ASTITransportation.NTCIP.Dialogs.StopZoom(messengerSession);

            #endregion

        }

        static void CameraTestPreset(ASTITransportation.Snmp.Transport.SnmpMessenger messengerSession)
        {
            ASTITransportation.Snmp.ProtocolDataUnit result = ASTITransportation.NTCIP.Dialogs.StorePresetPosition(1, messengerSession);

            result = ASTITransportation.NTCIP.Dialogs.GotoPresetPosition(3, messengerSession);

            result = ASTITransportation.NTCIP.Dialogs.GotoPresetPosition(1, messengerSession);
        }

        static void MessageTest(string msg, ASTITransportation.Snmp.Transport.SnmpMessenger messengerSession)
        {
            //if (string.IsNullOrEmpty(msg)) msg = "[flt10o5]lineJ[/fl][nl][flt10o5]line2[/fl][nl][flt10o5]line3[/fl][np][flt10o5]line4[/fl][nl][flt10o5]line5[/fl][nl][flt10o5]line6[/fl]";            
            //msg = "[pt20o]EX 23-26[nl]3 MILES[nl]4 MINS[np]EX 26-34[nl]8 MILES[nl]11 MIN";
            msg = "**";
            ASTITransportation.NTCIP.dmsActivateMsgError result = ASTITransportation.NTCIP.Dialogs.RunMessage(msg, messengerSession);
            //ASTITransportation.NTCIP.dmsActivateMsgError result = ASTITransportation.NTCIP.Dialogs.RunMessage(msg,null,null, ASTITransportation.NTCIP.dmsMessageMemoryType.CurrentBuffer, messengerSession);
        }

        static void BlankTest(ASTITransportation.Snmp.Transport.SnmpMessenger messengerSession)
        {
            ASTITransportation.NTCIP.dmsActivateMsgError result = ASTITransportation.NTCIP.Dialogs.Blank(255, messengerSession);
        }

        static void PollTest(ASTITransportation.Snmp.Transport.SnmpMessenger messengerSession)
        {
            List<string> objects = new List<string>();
            objects.Add("1.3.6.1.4.1.1206.4.2.3.5.8.1.3.5.1");//currentBuffer
            objects.Add("1.3.6.1.4.1.1206.4.2.3.5.8.1.3.3.1");//changeableMemory
            objects.Add("1.3.6.1.4.1.1206.4.2.3.9.8.1.0");//signVolts
            objects.Add("1.3.6.1.4.1.1206.4.2.6.3.1.0");//globalTime            

            ASTITransportation.Snmp.ProtocolDataUnit test = new ASTITransportation.Snmp.ProtocolDataUnit();
            test.Version = 0;
            test.CommunityName = "public";
            ASTITransportation.Snmp.SnmpExtensions.AddRange(test.Bindings, objects);
            //Equivelant
            //objects.ForEach(o => test.Bindings.Add(new ASTITransportation.Snmp.Variable(o)));

            List<byte> packet = test.ToPacket(ASTITransportation.Snmp.PmppEndPoint.NTCIP);

            List<byte> decoded =  ASTITransportation.Snmp.SnmpExtensions.PmppDecode(packet);

            ASTITransportation.Snmp.ProtocolDataUnit from = new ASTITransportation.Snmp.ProtocolDataUnit(decoded);

            ASTITransportation.Snmp.ProtocolDataUnit result = messengerSession.Get(objects);

            //List<string> testObjects = new List<string>();
            //testObjects.Add("1.3.6.1.4.1.1206.4.2.3.9.8.5");//lineVolts            
            //NTCIP.powerSource ps = NTCIP.Dialogs.GetPowerSource(messengerSession);
        }

        static void ResetTest(ASTITransportation.Snmp.Transport.SnmpMessenger messengerSession)
        {
            //Resets the controller of a NTCIP DMS Device
            ASTITransportation.Snmp.Variable result = ASTITransportation.NTCIP.Dialogs.DmsReset(messengerSession);
        }

        static void WalkTest(ASTITransportation.Snmp.Transport.SnmpMessenger messengerSession)
        {
            //walks a oid tree starting from the given node and outputs the results to the console

            List<ASTITransportation.Snmp.Variable> tree = messengerSession.Walk("1.3.6.1.4.1.1206.4.2.3.3.4.1.1.1.0");

            //PscPrivate
            //List<ASTITransportation.Snmp.Variable> tree = messengerSession.Walk("1.3.6.1.4.1.1206.3.34");
            //Event log
            //List<Variable> tree = messengerSession.Walk("1.3.6.1.4.1.1206.4.2.6.4.6.1.4.1.0");
            //Whole tree
            //messengerSession.Walk("1");
            //Permanent messages
            //messengerSession.Walk("1.3.6.1.4.1.1206.4.2.3.5.8.1.3.2.0");
            //Changable message
            //messengerSession.Walk("1.3.6.1.4.1.1206.4.2.3.5.8.1.3.3.0");

            foreach (ASTITransportation.Snmp.Variable v in tree)
            {
                string format = "{0} = {1}: {2}";

                switch (v.TypeCode)
                {
                    case ASTITransportation.Snmp.SnmpType.OctetString:
                        Console.WriteLine(string.Format(format, new object[] { v.Identifier, v.TypeCode, '"' + v.ToString() + '"' }));
                        break;
                    case ASTITransportation.Snmp.SnmpType.Integer32:
                        Console.WriteLine(string.Format(format, new object[] { v.Identifier, v.TypeCode, v.ToInt32() }));
                        break;
                    case ASTITransportation.Snmp.SnmpType.Counter32:
                    case ASTITransportation.Snmp.SnmpType.UInteger32:
                        Console.WriteLine(string.Format(format, new object[] { v.Identifier, v.TypeCode, v.ToUInt32() }));
                        break;
                    case ASTITransportation.Snmp.SnmpType.ObjectIdentifier:
                        Console.WriteLine(string.Format(format, new object[] { v.Identifier, v.TypeCode, v.ToObjectIdentifier() }));
                        break;
                    case ASTITransportation.Snmp.SnmpType.TimeTicks:
                        Console.WriteLine(string.Format(format, new object[] { v.Identifier, v.TypeCode, v.ToTimeSpan().ToString() }));
                        break;
                    case ASTITransportation.Snmp.SnmpType.IPAddress:
                        Console.WriteLine(string.Format(format, new object[] { v.Identifier, v.TypeCode, v.ToIPAddress().ToString() }));
                        break;
                    case ASTITransportation.Snmp.SnmpType.Boolean:
                        Console.WriteLine(string.Format(format, new object[] { v.Identifier, v.TypeCode, v.ToBoolean() }));
                        break;
                    default:
                        Console.WriteLine(string.Format(format, new object[] { v.Identifier, v.TypeCode, BitConverter.ToString(v.Value.ToArray()) }));
                        break;
                }
            }
        }

        static void PDFTest()
        {            
            ASTITransportation.Utilities.Utility.CreatePDF("test.pdf", @"
            <div id='pdfContainer'>
                <DIV id=HnDDL>
	                <A style='COLOR: white; CURSOR: pointer'><B>Close[X]</B></A>
	                <A style='COLOR: white; CURSOR: pointer'>&nbsp;&nbsp;<B>Print Reports</B></A>
                </DIV>
                <DIV id=wsCnt></DIV>
                <DIV class='something'><B>New Hampshire Department of Transportation Reports for I-93 Salem / Manchester Exit 1 - 5 - q01</B></DIV>
                <DIV style='PAGE-BREAK-AFTER: always; POSITION: relative' id=0-72010 class=printWindow><IMG class=goog-serverchart-image src='http://chart.apis.google.com/chart?cht=lc&amp;chs=700x250&amp;chtt=Total%20Volume%20%3A%20Sunday%2C%207%20%2F%202010&amp;chxt=x%2Cy&amp;chxl=0%3A%7C24%3A00%7C1%3A00%7C2%3A00%7C3%3A00%7C4%3A00%7C5%3A00%7C6%3A00%7C7%3A00%7C8%3A00%7C9%3A00%7C10%3A00%7C11%3A00%7C12%3A00%7C13%3A00%7C14%3A00%7C15%3A00%7C16%3A00%7C17%3A00%7C18%3A00%7C19%3A00%7C20%3A00%7C21%3A00%7C22%3A00%7C23%3A00&amp;chdlp=r&amp;chdl=q04(NB)%7Cq01(NB)&amp;chco=3399CC%2C80C65A&amp;chxr=1%2C222%2C6629&amp;chd=e%3AFZDBBLADAABTEUIgMeQaVcYEYKWOU1UbSyT8SKO1MZJrH2Fm%2CJGFHCfAiAtEdKEOJYSjfze9P..995h0f2b13y0rMk7dHWRMn&amp;is3D=true' width=700 height=250></DIV>
                <DIV style='PAGE-BREAK-AFTER: always; POSITION: relative' id=3-72010 class=printWindow><IMG class=goog-serverchart-image src='http://chart.apis.google.com/chart?cht=lc&amp;chs=700x250&amp;chtt=Total%20Volume%20%3A%20Wednesday%2C%207%20%2F%202010&amp;chxt=x%2Cy&amp;chxl=0%3A%7C24%3A00%7C1%3A00%7C2%3A00%7C3%3A00%7C4%3A00%7C5%3A00%7C6%3A00%7C7%3A00%7C8%3A00%7C9%3A00%7C10%3A00%7C11%3A00%7C12%3A00%7C13%3A00%7C14%3A00%7C15%3A00%7C16%3A00%7C17%3A00%7C18%3A00%7C19%3A00%7C20%3A00%7C21%3A00%7C22%3A00%7C23%3A00&amp;chdlp=r&amp;chdl=q04(NB)%7Cq01(NB)&amp;chco=3399CC%2C80C65A&amp;chxr=1%2C243%2C7269&amp;chd=e%3AC4A5AAABArDVJxNuM1NRPIQUPkRTU5aEbMcnYoR7MqLQH9Fh%2CEvBkA8BhFza8uC402pr8piuNtmruwCyl3L..zApvdpZRTXKo&amp;is3D=true' width=700 height=250 ></DIV>
                <DIV style='PAGE-BREAK-AFTER: always; POSITION: relative' id=1-72010 class=printWindow><IMG class=goog-serverchart-image src='http://chart.apis.google.com/chart?cht=lc&amp;chs=700x250&amp;chtt=Total%20Volume%20%3A%20Monday%2C%207%20%2F%202010&amp;chxt=x%2Cy&amp;chxl=0%3A%7C24%3A00%7C1%3A00%7C2%3A00%7C3%3A00%7C4%3A00%7C5%3A00%7C6%3A00%7C7%3A00%7C8%3A00%7C9%3A00%7C10%3A00%7C11%3A00%7C12%3A00%7C13%3A00%7C14%3A00%7C15%3A00%7C16%3A00%7C17%3A00%7C18%3A00%7C19%3A00%7C20%3A00%7C21%3A00%7C22%3A00%7C23%3A00&amp;chdlp=r&amp;chdl=q04(NB)%7Cq01(NB)&amp;chco=3399CC%2C80C65A&amp;chxr=1%2C190%2C6036&amp;chd=e%3AD0BPAOAABYE.LPN9OfQdSMVQTjTtW9aYcdbIZ1SPOGMyIHFn%2CHMDNBfBvGkZ1wF4R0WtryZ2Y2b2936-j..9Y3qq-jbcCTgLR&amp;is3D=true' width=700 height=250></DIV>
            </div>");

        }

        static void XAMLTest()
        {            
            Utility.CreateRemappedRasterFromVector(
                "svgTest4.xaml", //path to xamlFile
                System.Windows.Media.Colors.LimeGreen, //color to remap elements to
                new System.IO.FileStream("out.png", System.IO.FileMode.Create), //the stream to save the result to
                500, //the height
                500,//the width
                null // dynamic text could also be "M01" or "Something Here"
                );
        }

        #region Move To NTCIP Library

        [Serializable()]
        internal struct FontInfo
        {
            public int fontNumber;
            public int fontIndex;
            public string fontName;
            public int fontHeight;
            public int fontCharSpacing;
            public int fontLineSpacing;
            public int fontVersionID;
            public int fontStatus;
            public int maxFontCharacters;
            public List<CharacterInformation> CharacterInfomation;            
        }

        [Serializable()]
        internal struct CharacterInformation
        {
            public ASTITransportation.Snmp.Variable characterBitmap;
            public int characterWidth;
            public int characterNumber;
        }

        static void DownloadFonts(ASTITransportation.Snmp.Transport.SnmpMessenger messengerSession, int startFont, int endFont)
        {
            string fontNumberOID = "1.3.6.1.4.1.1206.4.2.3.3.2.1.2";
            string fontNameOID = "1.3.6.1.4.1.1206.4.2.3.3.2.1.3";
            string fontHeightOID = "1.3.6.1.4.1.1206.4.2.3.3.2.1.4";
            string fontCharSpacingOID = "1.3.6.1.4.1.1206.4.2.3.3.2.1.5";
            string fontLineSpacingOID = "1.3.6.1.4.1.1206.4.2.3.3.2.1.6";
            string fontVersionIDOID = "1.3.6.1.4.1.1206.4.2.3.3.2.1.7";
            //string fontStatusOID = "1.3.6.1.4.1.1206.4.2.3.3.2.1.8";

            string characterWidthOID = "1.3.6.1.4.1.1206.4.2.3.3.4.1.2";
            string characterBitmapOID = "1.3.6.1.4.1.1206.4.2.3.3.4.1.3";

            for (int i = startFont, e = endFont; i <= e; i++ )
            {                
            ReadFont:                
                FontInfo fontInfo = new FontInfo();
                fontInfo.fontIndex = i;

                fontInfo.CharacterInfomation = new List<CharacterInformation>();

                int characterNumber = 0;
                try
                {
                    ASTITransportation.Snmp.ProtocolDataUnit pdu = messengerSession.Get(new List<string>()
                    {
                        fontNumberOID + "." + fontInfo.fontIndex,
                        fontNameOID + "." + fontInfo.fontIndex,
                        fontHeightOID + "." + fontInfo.fontIndex,
                        fontCharSpacingOID + "." + fontInfo.fontIndex,
                        fontLineSpacingOID + "." + fontInfo.fontIndex,
                        fontVersionIDOID + "." + fontInfo.fontIndex,
                        "1.3.6.1.4.1.1206.4.2.3.3.3"
                    });

                    fontInfo.fontNumber = pdu.Bindings[0].ToInt32();
                    fontInfo.fontName = pdu.Bindings[1].ToString();
                    fontInfo.fontHeight = pdu.Bindings[2].ToInt32();
                    fontInfo.fontCharSpacing = pdu.Bindings[3].ToInt32();
                    fontInfo.fontLineSpacing = pdu.Bindings[4].ToInt32();
                    fontInfo.fontVersionID = pdu.Bindings[5].ToInt32();
                    fontInfo.maxFontCharacters = pdu.Bindings[6].ToInt32();
                    fontInfo.fontStatus = 11;

                }
                catch
                {
                    goto ReadFont;
                }               

                Console.WriteLine("Downloading Font: " + fontInfo.fontName);

                if (fontInfo.maxFontCharacters < 0 || fontInfo.maxFontCharacters < 10 || fontInfo.maxFontCharacters > 512) goto ReadFont;                

                while (characterNumber <= fontInfo.maxFontCharacters)
                {
                FindCharacterWidth:                    
                    int characterWidth;
                    ASTITransportation.Snmp.Variable characterBitmap;
                    
                    try
                    {
                        ASTITransportation.Snmp.ProtocolDataUnit pdu = messengerSession.Get(new List<string>()
                        {
                            characterWidthOID + "." + fontInfo.fontIndex + "." + characterNumber,
                            characterBitmapOID + "." + fontInfo.fontIndex + "." + characterNumber
                        });

                        characterWidth = pdu.Bindings[0].ToInt32();
                        characterBitmap = pdu.Bindings[1];

                        if (characterWidth > 10) goto FindCharacterWidth;

                    }
                    catch
                    {
                        goto FindCharacterWidth;
                    }                                       

                    fontInfo.CharacterInfomation.Add(new CharacterInformation()
                    {
                        characterNumber = characterNumber,
                        characterBitmap = characterBitmap,
                        characterWidth = characterWidth
                    });

                    Console.WriteLine("Downloaded Font Character Number: " + characterNumber + " - '" + (char) characterNumber + "'");
                    Console.WriteLine("Width: " + characterWidth);
                    Console.WriteLine("Bytes: " + BitConverter.ToString(characterBitmap.Value.ToArray()));

                    characterNumber++;
                }

                string fName = fontInfo.fontName;

                System.IO.Stream stream = System.IO.File.Open(fontInfo.fontName, System.IO.FileMode.Create);
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bFormatter.Serialize(stream, fontInfo);
                stream.Close();

                fontInfo = new FontInfo();
                stream = System.IO.File.Open(fName, System.IO.FileMode.Open);
                fontInfo = (FontInfo)bFormatter.Deserialize(stream);
                stream.Close();

                Console.WriteLine("Finshed Downloading Font: " + fName);

            }
        }

        static void GenerateNETMFFontCodeFiles(string fontFileName, int ? fontNumber)
        {
            #region Deserialize Font

            if (string.IsNullOrEmpty(fontFileName)) fontFileName = "Single (5x7)";
            FontInfo fontInfo;
            try
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                System.IO.Stream stream = System.IO.File.Open(fontFileName, System.IO.FileMode.Open);
                fontInfo = (FontInfo)bFormatter.Deserialize(stream);
                stream.Close();
            }
            catch { throw; }

            #endregion
            
            #region Generate netMF Font Class Code
            string className = fontInfo.fontName.Trim().Replace("(", string.Empty).Replace(")", string.Empty).Trim().Replace(" ", string.Empty).ToString();

            string codeHeader = @"
namespace FontDefinitions
{{
    class {0} : AbstractClasses.FontDefinition
    {{       
        protected override void Initialize()
        {{         
            this.fontName = {1};
            this.fontNumber = {2};
            this.fontHeight = {3};
            this.fontCharSpacing = {4};
            this.fontLineSpacing = {5};
            this.fontVersionID = {6};                    
            this.fontStatus = {7};
";            
            string codeFooter = @"        
        }
    }
}";
            string initializeCodeFormat = @"
            #region Character '{9}'   
            //CharacterWidth                     
            Objects.Add(new MFSnmp.Variable()
            {{  
                Description = {0},
                Identifier = {1},
                Value = {2},
                ReadAccess = 1,
                WriteAccess = 2
            }});
            //CharacterBitmap
            Objects.Add(new MFSnmp.Variable()
            {{  
                Description = {3},
                Identifier = {4},
                Value = {5},
                ReadAccess = 1,
                WriteAccess = 2
            }});
            //CharacterNumber
            Objects.Add(new MFSnmp.Variable()
            {{  
                Description = {6},
                Identifier = {7},
                Value = {8},
                ReadAccess = 1,
                WriteAccess = 0
            }});            
            #endregion
";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            int file = 0;
            sb.Append(string.Format(codeHeader, new object[]{
                className + "_Sector_" + file,
                "\"" + fontInfo.fontName + "\"",
                fontNumber??fontInfo.fontNumber,
                fontInfo.fontHeight,
                fontInfo.fontCharSpacing,
                fontInfo.fontLineSpacing,
                fontInfo.fontVersionID,
                fontInfo.fontStatus,                
            }));

            int generatedCharacters = 0;
            foreach (CharacterInformation ci in fontInfo.CharacterInfomation)
            {
                if (ci.characterWidth == 0) continue;
                if (ci.characterBitmap.Value.Count == 0) continue;

                string result;
                if (ci.characterWidth > 0)
                {
                    result = string.Format(initializeCodeFormat, new object[] { 
                    "string.Empty",  
                    "\"1.3.6.1.4.1.1206.4.2.3.3.4.1.2." + fontInfo.fontIndex + "." + ci.characterNumber+"\"",
                    "new byte[]{ " +  ci .characterWidth + " }",
                    "string.Empty",
                    "\"1.3.6.1.4.1.1206.4.2.3.3.4.1.3." + fontInfo.fontIndex + "." + ci.characterNumber+"\"",
                    "new byte[]{0x"+ BitConverter.ToString(ci.characterBitmap.Value.ToArray()).Replace("-",",0x").ToString() +"}",
                    "string.Empty",  
                    "\"1.3.6.1.4.1.1206.4.2.3.3.4.1.1." + fontInfo.fontIndex + "." + ci.characterNumber+"\"",
                    "new byte[]{ " +  ci .characterNumber + " }",
                    ci .characterNumber
                });
                }
                else
                {
                    result = string.Format(initializeCodeFormat, new object[] { 
                    "string.Empty",  
                    "\"1.3.6.1.4.1.1206.4.2.3.3.4.1.2." + fontInfo.fontIndex + "." + ci.characterNumber+"\"",
                    "new byte[]{ 0x00 }",
                    "string.Empty",
                    "\"1.3.6.1.4.1.1206.4.2.3.3.4.1.3." + fontInfo.fontIndex + "." + ci.characterNumber+"\"",
                    "new byte[]{}",
                    "string.Empty",  
                    "\"1.3.6.1.4.1.1206.4.2.3.3.4.1.1." + fontInfo.fontIndex + "." + ci.characterNumber+"\"",
                    "new byte[]{ " +  ci .characterNumber + " }",
                    ci .characterNumber
                });
                }

                

                sb.Append(result);
                if (generatedCharacters > 32)
                {
                    sb.Append(codeFooter);
                    System.IO.File.WriteAllText(className + "_Sector_" + file + ".cs", sb.ToString());
                    generatedCharacters = 0;
                    file++;

                    sb = new System.Text.StringBuilder();
                    sb.Append(string.Format(codeHeader, new object[]{
                        className + "_Sector_" + file,
                        "\"" + fontInfo.fontName + "\"",
                        fontInfo.fontNumber,
                        fontInfo.fontHeight,
                        fontInfo.fontCharSpacing,
                        fontInfo.fontLineSpacing,
                        fontInfo.fontVersionID,
                        fontInfo.fontStatus                        
                    }));

                }

                generatedCharacters++;
            }
            #endregion

            #region Generate netMF Font Definition Code
            string definitionHeader = @"
namespace FontDefinitions
{{
    public sealed class {0}
    {{       

        private {0}(){{}}

        public static void Deploy(int fontIndex)
        {{         
            {1}
";
            string definitionFooter = @"        
        }
    }
}";
            sb = new System.Text.StringBuilder();            
            string foreachCode = string.Empty;

            for(int cb = 0; cb < file; cb++)
            {
                foreachCode += Environment.NewLine + string.Format(@"
            using({0}_Sector_{1} sector = new {0}_Sector_{1}())
            {{
                sector.Deploy(fontIndex);
            }}

", new object[] { className, cb });
            }

            sb.Append(string.Format(definitionHeader, new object[]{
                        className + "Definition",
                        foreachCode                 
                    }));

            sb.Append(definitionFooter);
            System.IO.File.WriteAllText(className + "Definition.cs", sb.ToString());
            #endregion            
        }

        static void DownloadChangeableMessages(ASTITransportation.Snmp.Transport.SnmpMessenger testSession)
        {
            //use a GetRequest to retrieve the number of rows in the table
            ASTITransportation.Snmp.Variable numRows = testSession.Get("1.3.6.1.4.1.1206.4.2.3.5.2").Bindings[0];

            //instantiate a list to hold the retirved variables
            List<ASTITransportation.Snmp.Variable> messages = new List<ASTITransportation.Snmp.Variable>();

            //declare the OID base to increment off of
            string OIDbase = "1.3.6.1.4.1.1206.4.2.3.5.8.1.3.3.";

            //loop from 0 until the numberOfRows + 1 to retrieve all values
            for (int index = 0, end = (numRows.ToInt32() + 1); index < end; index++)
            {
                try
                {
                    //add the retrieved value to the list we declared to hold the values being retrieved
                    messages.Add(testSession.Get(OIDbase + index.ToString()).Bindings[0]);
                }
                catch
                {
                    //decrement and try again
                    index--;
                    continue;
                }

            }

        }

        static void DownloadPermanentMessages(ASTITransportation.Snmp.Transport.SnmpMessenger testSession)
        {
            ASTITransportation.Snmp.Variable numRows = testSession.Get("1.3.6.1.4.1.1206.4.2.3.5.1").Bindings[0];

            List<ASTITransportation.Snmp.Variable> messages = new List<ASTITransportation.Snmp.Variable>();

            string OIDbase = "1.3.6.1.4.1.1206.4.2.3.5.8.1.3.2.";

            for (int i = 0, e = (numRows.ToInt32() + 1); i < e; i++)
            {
                try
                {
                    messages.Add(testSession.Get(OIDbase + i.ToString()).Bindings[0]);
                }
                catch
                {
                    i--;
                    continue;
                }

            }
        }

        #endregion

        static void OtherTests()
        {

            string teststr = "ABCDefg";
            teststr.Reverse();

            int a = 0;

            int b = 99;

            ASTITransportation.Extensions.IntExtensions.Swap(ref a, ref b);

            ASTITransportation.Extensions.IntExtensions.Swap(ref b, ref a);

            Guid test = Guid.NewGuid();

            //System.Diagnostics.Debug.Assert(false);

            return;
        }

        static void RegTest()
        {
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex("http://[\\.\\w?/~_@&=%]+");
            string result = r.Replace("visit us: http://www.apache.org!", "1234<a href=\"$0\">$0</a>");                        
            var m = r.Match("1234<a href=\"$0\">$0</a>");      
      
            //r.p
        }

    }
}
