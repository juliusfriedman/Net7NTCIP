using System;
using System.Collections.Generic;
using ASTITransportation.Modbus;

namespace MorningStar
{
    namespace TS
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

            #region Enums

            public enum ControlMode
            {
                Charge, Load, Diversion
            };

            public enum ControlState
            {
                START,
                NIGHTCHECK,
                DISCONNECT,
                NIGHT,
                FAULT,
                BULK,
                PWM,
                FLOAT,
                EQUALIZE,
                LOADSTART,
                NORMAL,
                LVDWARN,
                LVD,
                LOADFAULT,
                LOADDISCONNECT,
                NORMALOFF,
                OVERRIDELVD
            };

            #endregion

            #region Classes

            public class AlarmReading
            {
                System.Collections.BitArray bits;

                public AlarmReading(Int16 word)
                {
                    bits = new System.Collections.BitArray(new int[] { word });
                }

                public bool RTSOpen
                {
                    get { return bits.Get(0); }
                }

                public bool RTSShorted
                {
                    get { return bits.Get(1); }
                }

                public bool RTSDisconnected
                {
                    get { return bits.Get(2); }
                }

                public bool ThsDisconnected
                {
                    get { return bits.Get(3); }
                }

                public bool ThsShorted
                {
                    get { return bits.Get(4); }
                }

                public bool TristarHot
                {
                    get { return bits.Get(5); }
                }

                public bool CurrentLimit
                {
                    get { return bits.Get(6); }
                }

                public bool CurrentOffset
                {
                    get { return bits.Get(7); }
                }

                public bool BatterySense
                {
                    get { return bits.Get(8); }
                }

                public bool BattSenseDisc
                {
                    get { return bits.Get(9); }
                }

                public bool Uncalibrated
                {
                    get { return bits.Get(10); }
                }

                public bool RTSMiswire
                {
                    get { return bits.Get(11); }
                }

                public bool HVD
                {
                    get { return bits.Get(12); }
                }

                public bool HighD
                {
                    get { return bits.Get(13); }
                }

                public bool Miswire
                {
                    get { return bits.Get(14); }
                }

                public bool FETOpen
                {
                    get { return bits.Get(15); }
                }

                public bool P12
                {
                    get { return bits.Get(16); }
                }

                public bool LoadDisconnect
                {
                    get { return bits.Get(17); }
                }

                public bool Alarm19
                {
                    get { return bits.Get(18); }
                }

                public bool Alarm20
                {
                    get { return bits.Get(19); }
                }

                public bool Alarm21
                {
                    get { return bits.Get(20); }
                }

                public bool Alarm22
                {
                    get { return bits.Get(21); }
                }

                public bool Alarm23
                {
                    get { return bits.Get(22); }
                }

                public bool Alarm24
                {
                    get { return bits.Get(23); }
                }

            }

            public class FaultReading
            {
                System.Collections.BitArray bits;

                public FaultReading(Int16 word)
                {
                    bits = new System.Collections.BitArray(new int[] { word });
                }

                public bool ExternalShort
                {
                    get { return bits.Get(0); }
                }

                public bool Overcurrent
                {
                    get { return bits.Get(1); }
                }

                public bool FETShort
                {
                    get { return bits.Get(2); }
                }

                public bool Software
                {
                    get { return bits.Get(3); }
                }

                public bool HVD
                {
                    get { return bits.Get(4); }
                }

                public bool TriStarHot
                {
                    get { return bits.Get(5); }
                }

                public bool DIPswChanged
                {
                    get { return bits.Get(6); }
                }

                public bool SettingsEdit
                {
                    get { return bits.Get(7); }
                }

                public bool Reset
                {
                    get { return bits.Get(8); }
                }

                public bool Miswire
                {
                    get { return bits.Get(9); }
                }

                public bool RTSShorted
                {
                    get { return bits.Get(10); }
                }

                public bool RTSDisconnected
                {
                    get { return bits.Get(11); }
                }

                public bool Fault12
                {
                    get { return bits.Get(12); }
                }

                public bool Fault13
                {
                    get { return bits.Get(13); }
                }

                public bool Fault14
                {
                    get { return bits.Get(14); }
                }

                public bool Fault15
                {
                    get { return bits.Get(15); }
                }

            }

            #endregion

            #region Scalars
            /* TS Modbus registers have scalars used to calculate their equivalent double value */
            static double stdScalar1 = 0.002950042724609375;
            static double stdScalar2 = 0.00424652099609375;
            static double stdScalar3 = 0.002034515380859375;
            static double stdScalar4 = 0.00966400146484375;
            static double stdScalar5 = 0.000031;
            static double stdScalar6 = 0.1;
            #endregion

            #region Methods

            /// <summary>
            /// Retrieves the Battery Voltage From a Morning Star Charge Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The Battery Voltage reported by the Charge Controller</returns>
            public static double GetBatteryVoltage(ModbusMessenger messenger)
            {
                //remember the old transport encoding
                ModbusMessenger.TransportEncoding te = messenger.TransportEndoing;
                //set the new transport encoding
                messenger.TransportEndoing = ModbusMessenger.TransportEncoding.Binary;
                //obtain batteryVoltage - scale via stdScalar1
                ModbusMessage registers = messenger.ReadHoldingRegisters(0x0008, 0x0001);
                //get registers.DataPoints into a short v
                short v = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(registers.DataPoints.ToArray(), 0));
                //return v * stdScalar1
                messenger.TransportEndoing = te;
                return (double)v * stdScalar1;
            }

            /// <summary>
            /// Retrieves the System Voltage From a Morning Star Charge Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The System Voltage reported by the Charge Controller</returns>
            public static double GetSystemVoltage(ModbusMessenger messenger)
            {
                //remember the old transport encoding
                ModbusMessenger.TransportEncoding te = messenger.TransportEndoing;
                //set the new transport encoding
                messenger.TransportEndoing = ModbusMessenger.TransportEncoding.Binary;
                //obtain systemVoltage - scale via stdScalar5
                ModbusMessage registers = messenger.ReadHoldingRegisters(0xf005, 0x0001);
                //get registers.DataPoints into a short v
                short v = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(registers.DataPoints.ToArray(), 0));
                //return v * stdScalar5
                messenger.TransportEndoing = te;
                return (double)v * stdScalar5;
            }

            /// <summary>
            /// Retrieves the Power Source Voltage From a Morning Star Charge Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The Power Source Voltage reported by the Charge Controller</returns>
            public static double GetPowerSourceVoltage(ModbusMessenger messenger)
            {
                //remember the old transport encoding
                ModbusMessenger.TransportEncoding te = messenger.TransportEndoing;
                //set the new transport encoding
                messenger.TransportEndoing = ModbusMessenger.TransportEncoding.Binary;
                //obtain powerSource - scale via stdScalar2
                ModbusMessage registers = messenger.ReadHoldingRegisters(0x000a, 0x0001);
                //get registers.DataPoints into a short v
                short v = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(registers.DataPoints.ToArray(), 0));
                //return v * stdScalar2
                messenger.TransportEndoing = te;
                return (double)v * stdScalar2;
            }

            /// <summary>
            /// Retrieves the Total Amp Hours Counter From a Morning Star Charge Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The Total Amp Hours Counter reported by the Charge Controller</returns>
            public static double GetTotalAmpHours(ModbusMessenger messenger)
            {
                //remember the old transport encoding
                ModbusMessenger.TransportEncoding te = messenger.TransportEndoing;
                //set the new transport encoding
                messenger.TransportEndoing = ModbusMessenger.TransportEncoding.Binary;
                //obtain totalAmpHours - scale via stdScalar6
                ModbusMessage registers = messenger.ReadHoldingRegisters(0xe028, 0x0002);
                //get registers.DataPoints into a short v
                byte[] data = registers.DataPoints.ToArray();
                short v = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, 0));
                if (data.Length > 2)
                {
                    v += System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, 2));
                }
                //return v * stdScalar6
                messenger.TransportEndoing = te;
                return (double)v * stdScalar6;
            }

            /// <summary>
            /// Retrieves the Resetable Amp Hours Counter From a Morning Star Charge Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The Resetable Amp Hours Counter reported by the Charge Controller</returns>
            public static double GetResetableAmpHours(ModbusMessenger messenger)
            {
                //remember the old transport encoding
                ModbusMessenger.TransportEncoding te = messenger.TransportEndoing;
                //set the new transport encoding
                messenger.TransportEndoing = ModbusMessenger.TransportEncoding.Binary;
                //obtain totalAmpHours - scale via stdScalar6
                ModbusMessage registers = messenger.ReadHoldingRegisters(0xe026, 0x0002);
                //get registers.DataPoints into a short v
                byte[] data = registers.DataPoints.ToArray();
                short v = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, 0));
                if (data.Length > 2)
                {
                    v += System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, 2));
                }
                //return v * stdScalar6
                messenger.TransportEndoing = te;
                return (double)v * stdScalar6;
            }

            /// <summary>
            /// Retrieves the Killowatt Hours Counter From a Morning Star Charge Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The Killowatt Hours Counter reported by the Charge Controller</returns>
            public static int GetKilowattHours(ModbusMessenger messenger)
            {
                //remember the old transport encoding
                ModbusMessenger.TransportEncoding te = messenger.TransportEndoing;
                //set the new transport encoding
                messenger.TransportEndoing = ModbusMessenger.TransportEncoding.Binary;
                //obtain totalAmpHours - scale via stdScalar6
                ModbusMessage registers = messenger.ReadHoldingRegisters(0xe02a, 0x0001);
                //get registers.DataPoints into a short v
                short v = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(registers.DataPoints.ToArray(), 0));
                //return v * stdScalar6
                messenger.TransportEndoing = te;
                return v;
            }

            /// <summary>
            /// Retrieves the Control Mode from a Morning Star Charge Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The ControlMode the Charge controller is operating under</returns>
            public static ControlMode GetControlMode(ModbusMessenger messenger)
            {
                //remember the old transport encoding
                ModbusMessenger.TransportEncoding te = messenger.TransportEndoing;
                //set the new transport encoding
                messenger.TransportEndoing = ModbusMessenger.TransportEncoding.Binary;
                ModbusMessage registers = messenger.ReadHoldingRegisters(0x001a, 0x0001);
                //get registers.DataPoints into a short v
                short v = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(registers.DataPoints.ToArray(), 0));
                messenger.TransportEndoing = te;
                return (ControlMode)v;
            }

            /// <summary>
            /// Retrieves the Control State from a Morning Star Charge Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The ControlState the Charge controller is operating under</returns>
            public static ControlState GetControlState(ModbusMessenger messenger, ControlMode controlMode)
            {
                //remember the old transport encoding
                ModbusMessenger.TransportEncoding te = messenger.TransportEndoing;
                //set the new transport encoding
                messenger.TransportEndoing = ModbusMessenger.TransportEncoding.Binary;
                ModbusMessage registers = messenger.ReadHoldingRegisters(0x001a, 0x0001);
                //get registers.DataPoints into a short v
                short v = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(registers.DataPoints.ToArray(), 0));
                messenger.TransportEndoing = te;
                if (controlMode == ControlMode.Charge || controlMode == ControlMode.Diversion)
                    return (ControlState)v;
                else return ((ControlState)(8 + v));
            }

            /// <summary>
            /// Retrieves the Control State from a Morning Star Charge Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The ControlState the Charge controller is operating under</returns>
            public static int GetControlState(ModbusMessenger messenger)
            {
                //remember the old transport encoding
                ModbusMessenger.TransportEncoding te = messenger.TransportEndoing;
                //set the new transport encoding
                messenger.TransportEndoing = ModbusMessenger.TransportEncoding.Binary;
                ModbusMessage registers = messenger.ReadHoldingRegisters(0x001a, 0x0001);
                //get registers.DataPoints into a short v
                short v = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(registers.DataPoints.ToArray(), 0));
                messenger.TransportEndoing = te;
                return v;
            }


            public static double GetBatteryTemperature(ModbusMessenger messenger)
            {
                throw new NotImplementedException();
            }

            public static double GetHeatSinkTemperature(ModbusMessenger messenger)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Retrieves the Charging Current from a Morning Star Charge Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The Charging Current reported by the charge controller</returns>
            public static double GetChargingCurrent(ModbusMessenger messenger)
            {
                //remember the old transport encoding
                ModbusMessenger.TransportEncoding te = messenger.TransportEndoing;
                //set the new transport encoding
                messenger.TransportEndoing = ModbusMessenger.TransportEncoding.Binary;
                //obtain chargeCurrent - scale via stdScalar3
                ModbusMessage registers = messenger.ReadHoldingRegisters(0x000b, 0x001);
                //get registers.DataPoints into a short v
                short v = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(registers.DataPoints.ToArray(), 0));
                //return v * stdScalar3
                messenger.TransportEndoing = te;
                return (double)v * stdScalar3;
            }

            /// <summary>
            /// Retrieves the Load Current from a Morning Star Charge Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The Load Current reported by the charge controller</returns>
            public static double GetLoadCurrent(ModbusMessenger messenger)
            {
                //remember the old transport encoding
                ModbusMessenger.TransportEncoding te = messenger.TransportEndoing;
                //set the new transport encoding
                messenger.TransportEndoing = ModbusMessenger.TransportEncoding.Binary;
                //obtain loadCurrent - scale via stdScalar4
                ModbusMessage registers = messenger.ReadHoldingRegisters(0x000c, 0x001);
                //get registers.DataPoints into a short v
                short v = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(registers.DataPoints.ToArray(), 0));
                //return v * stdScalar4
                messenger.TransportEndoing = te;
                return (double)v * stdScalar4;
            }

            /// <summary>
            /// Retrieves the Alarms from a Morning Star Charge Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The Alarm Readings reported by the charge controller</returns>
            public static AlarmReading GetAlarms(ModbusMessenger messenger)
            {
                //remember the old transport encoding
                ModbusMessenger.TransportEncoding te = messenger.TransportEndoing;
                //set the new transport encoding
                messenger.TransportEndoing = ModbusMessenger.TransportEncoding.Binary;
                ModbusMessage registersHi = messenger.ReadHoldingRegisters(0x0017, 0x0001);
                short v = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(registersHi.DataPoints.ToArray(), 0));
                ModbusMessage registersLo = messenger.ReadHoldingRegisters(0x001d, 0x0001);
                v += System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(registersLo.DataPoints.ToArray(), 0));
                //get registers.DataPoints into a short v
                messenger.TransportEndoing = te;
                return new AlarmReading(v);
            }

            /// <summary>
            /// Retrieves the Faults from a Morning Star Charge Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The Fault Readings reported by the charge controller</returns>
            public static FaultReading GetFaults(ModbusMessenger messenger)
            {
                //remember the old transport encoding
                ModbusMessenger.TransportEncoding te = messenger.TransportEndoing;
                //set the new transport encoding
                messenger.TransportEndoing = ModbusMessenger.TransportEncoding.Binary;
                ModbusMessage registers = messenger.ReadHoldingRegisters(0x001a, 0x0001);
                //get registers.DataPoints into a short v
                short v = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(registers.DataPoints.ToArray(), 0));
                messenger.TransportEndoing = te;
                return new FaultReading(v);
            }

            /// <summary>
            /// Retrieves the amount of days inbetween battery services from a Morning Star Change Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The amount of days inbetween the last day the batteries were serviced and today as reported by the charge controller</returns>
            public static int GetDaysBetweenBatteryServiceIntervals(ModbusMessenger messenger)
            {
                //remember the old transport encoding
                ModbusMessenger.TransportEncoding te = messenger.TransportEndoing;
                //set the new transport encoding
                messenger.TransportEndoing = ModbusMessenger.TransportEncoding.Binary;
                ModbusMessage registers = messenger.ReadHoldingRegisters(0xe022, 0x0001);
                short v = BitConverter.ToInt16(registers.DataPoints.ToArray(), 0);
                messenger.TransportEndoing = te;
                return v;
            }

            /// <summary>
            /// Retrieves the amount of days since the last battery service from a Morning Star Change Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The amount of days since the last day the batteries were serviced as reported by the charge controller</returns>
            public static int GetDaysSinceLastBatteryService(ModbusMessenger messenger)
            {
                //remember the old transport encoding
                ModbusMessenger.TransportEncoding te = messenger.TransportEndoing;
                //set the new transport encoding
                messenger.TransportEndoing = ModbusMessenger.TransportEncoding.Binary;
                ModbusMessage registers = messenger.ReadHoldingRegisters(0xe023, 0x0001);
                short v = BitConverter.ToInt16(registers.DataPoints.ToArray(), 0);
                messenger.TransportEndoing = te;
                return v;
            }

            /// <summary>
            /// Retirves the minimum battery voltage from a Morning Star Charge Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The minimum battery voltage as reported by the charge controller</returns>
            public static double GetMinimumBatteryVoltage(ModbusMessenger messenger)
            {
                //remember the old transport encoding
                ModbusMessenger.TransportEncoding te = messenger.TransportEndoing;
                //set the new transport encoding
                messenger.TransportEndoing = ModbusMessenger.TransportEncoding.Binary;
                //obtain minBatteryVoltage - scale via stdScalar3
                ModbusMessage registers = messenger.ReadHoldingRegisters(0xe02b, 0x0001);
                //get registers.DataPoints into a short v
                short v = BitConverter.ToInt16(registers.DataPoints.ToArray(), 0);
                //return v * stdScalar3
                messenger.TransportEndoing = te;
                return (double)v * stdScalar3;
            }

            /// <summary>
            /// Retirves the maximium battery voltage from a Morning Star Charge Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The maximum battery voltage as reported by the charge controller</returns>
            public static double GetMaximumBatteryVoltage(ModbusMessenger messenger)
            {
                //remember the old transport encoding
                ModbusMessenger.TransportEncoding te = messenger.TransportEndoing;
                //set the new transport encoding
                messenger.TransportEndoing = ModbusMessenger.TransportEncoding.Binary;
                //obtain minBatteryVoltage - scale via stdScalar3
                ModbusMessage registers = messenger.ReadHoldingRegisters(0xe02c, 0x0001);
                //get registers.DataPoints into a short v
                short v = BitConverter.ToInt16(registers.DataPoints.ToArray(), 0);
                //return v * stdScalar3
                messenger.TransportEndoing = te;
                return (double)v * stdScalar3;
            }

            /// <summary>
            /// Retrieves the Hardware Version from a Morning Star Charge Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The hardware version as reported by the charge controller</returns>
            public static string GetHardwareVersion(ModbusMessenger messenger)
            {
                ModbusMessenger.TransportEncoding te = messenger.TransportEndoing;
                messenger.TransportEndoing = ModbusMessenger.TransportEncoding.Binary;
                ModbusMessage registers = messenger.ReadHoldingRegisters(0xf00a, 0x0001);
                messenger.TransportEndoing = te;
                return registers.DataPoints[0] + "." + registers.DataPoints[1];
            }

            /// <summary>
            /// Retrieves the Serial Number from a Morning Star Charge Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The serial number of the charge controller</returns>
            public static string GetSerialNumber(ModbusMessenger messenger)
            {
                //remember the old transport encoding
                ModbusMessenger.TransportEncoding te = messenger.TransportEndoing;
                //set the new transport encoding
                messenger.TransportEndoing = ModbusMessenger.TransportEncoding.Binary;
                ModbusMessage registers = messenger.ReadHoldingRegisters(0xf000, 0x0004);//was 0x0008
                messenger.TransportEndoing = te;
                return System.Text.Encoding.ASCII.GetString(registers.DataPoints.ToArray());//needed substring 0, 8
            }

            /// <summary>
            /// Retrieves the Vendor Name from a Morning Star Charge Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The Vendor Name of the charge controller</returns>
            public static string GetVendorName(ModbusMessenger messenger)
            {
                ModbusMessage vn = messenger.ReadDeviceInformation(0, 1);
                return System.Text.Encoding.ASCII.GetString(vn.DataPoints.ToArray());
            }

            /// <summary>
            /// Retrieves the Product Code from a Morning Star Charge Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The Product Code of the charge controller</returns>
            public static string GetProductCode(ModbusMessenger messenger)
            {
                ModbusMessage pc = messenger.ReadDeviceInformation(1, 1);
                return System.Text.Encoding.ASCII.GetString(pc.DataPoints.ToArray());
            }

            /// <summary>
            /// Retrieves the Software Version from a Morning Star Charge Controller
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>The Software Version of the charge controller</returns>
            public static string GetSoftwareVersion(ModbusMessenger messenger)
            {
                ModbusMessage mmr = messenger.ReadDeviceInformation(2, 1);
                return System.Text.Encoding.ASCII.GetString(mmr.DataPoints.ToArray());
            }

            /// <summary>
            /// Instructs the Morning Star Charge Controller to reset the Battery Service Reminder Alarm
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>True if the controller has reset the Battery Service Reminder</returns>
            public static bool ResetBatteryServiceReminder(ModbusMessenger messenger)
            {
                try
                {
                    ModbusMessage result = messenger.WriteSingleCoil(0x0013, true);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Instructs the Morning Star Charge Controller to Reset itself
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>True if the controller has reset</returns>
            public static bool ResetController(ModbusMessenger messenger)
            {
                try
                {
                    ModbusMessage result = messenger.WriteSingleCoil(0x00ff, true);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Instructs the Morning Star Charge Controller to reset the Fault Table
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>True if the controller has reset the Fault Table</returns>
            public static bool ClearFaults(ModbusMessenger messenger)
            {
                try
                {
                    ModbusMessage result = messenger.WriteSingleCoil(0x0014, true);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Instructs the Morning Star Charge Controller to reset the Alarm Table
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>True if the controller has reset the Alarm Table</returns>
            public static bool ClearAlarms(ModbusMessenger messenger)
            {
                try
                {
                    ModbusMessage result = messenger.WriteSingleCoil(0x0015, true);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Instructs the Morning Star Charge Controller to force it's software into DISCONNECT state. Turns off MOSFETs and waits.
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>True if the controller has been disconnected</returns>
            public static bool DisconnectController(ModbusMessenger messenger)
            {
                try
                {
                    ModbusMessage result = messenger.WriteSingleCoil(0x0001, false);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Instructs the Morning Star Charge Controller to force it's software into CONNECT state. Turns off MOSFETs and waits.
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>True if the controller has been connected</returns>
            public static bool ConnectController(ModbusMessenger messenger)
            {
                try
                {
                    ModbusMessage result = messenger.WriteSingleCoil(0x0001, true);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Instructs the Morning Star Charge Controller to reset the Resetable Amp Hours Counter
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>True if the controller has reset the Resetable Amp Hours Counter</returns>
            public static bool ClearResetableAmpHours(ModbusMessenger messenger)
            {
                try
                {
                    ModbusMessage result = messenger.WriteSingleCoil(0x0010, true);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Instructs the Morning Star Charge Controller to reset the Total Amp Hours Counter
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>True if the controller has reset the Total Amp Hours Counter</returns>
            public static bool ClearTotalAmpHours(ModbusMessenger messenger)
            {
                try
                {
                    ModbusMessage result = messenger.WriteSingleCoil(0x0011, true);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Instructs the Morning Star Charge Controller to reset the Killowatt Hours Counter
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>True if the controller has reset the Killowatt Hours Counter</returns>
            public static bool ClearKillowattHours(ModbusMessenger messenger)
            {
                try
                {
                    ModbusMessage result = messenger.WriteSingleCoil(0x0012, true);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Instructs the Morning Star Charge Controller to forcefully update it's EEPROM
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>True if the controller has updated EEPROM</returns>
            public static bool ForceEEPROMUpdate(ModbusMessenger messenger)
            {
                try
                {
                    ModbusMessage result = messenger.WriteSingleCoil(0x0016, true);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Instructs the Morning Star Charge Controller to enter Low Voltage Disconnect for 1 Cycle
            /// </summary>
            /// <param name="messenger">The ModbusMessenger to perform the transaction on</param>
            /// <returns>True if the controller has entered Low Voltage Disconnect for 1 Cycle</returns>
            public static bool LVDOverrideOneCycle(ModbusMessenger messenger)
            {
                try
                {
                    ModbusMessage result = messenger.WriteSingleCoil(0x0017, true);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            #endregion
        }
    }
}
