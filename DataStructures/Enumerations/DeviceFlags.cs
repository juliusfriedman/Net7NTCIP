using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ASTITransportation.Enumerations
{
    /// <summary>
    /// An Enumeration Describing the Capabilities of a Device
    /// </summary>
    [Flags]
    public enum DeviceFlags : ulong
    {
        //Basic Flags
        [EnumMember(Value = "None")]
        None = 0,
        [EnumMember(Value = "GenericDevice")]
        GenericDevice = 1,
        [EnumMember(Value = "GenericSignalDevice")]
        GenericSignalDevice = 3,
        [EnumMember(Value = "GenericMeasurementDevice")]
        GenericMeasurementDevice = 5,
        [EnumMember(Value = "GenericVideoDevice")]
        GenericVideoDevice = 9,
        [EnumMember(Value = "GenericDisplayDevice")]
        GenericDisplayDevice = 17,
        [EnumMember(Value = "Beacon")]
        Beacon = 65539,
        [EnumMember(Value = "TrafficSafetySensor")]
        TrafficSafetySensor = 65541,
        [EnumMember(Value = "Camera")]
        Camera = 65545,
        [EnumMember(Value = "DynamicMessageBoard")]
        DynamicMessageBoard = 65553,
        [EnumMember(Value = "Traffic")]
        Traffic = 131075,
        [EnumMember(Value = "VideoSuite")]
        VideoSuite = 131081,
        [EnumMember(Value = "SpeedAwarenessMonitor")]
        SpeedAwarenessMonitor = 131089,
        [EnumMember(Value = "Railroad")]
        Railroad = 262147,
        [EnumMember(Value = "Audio")]
        Audio = 262161,
        [EnumMember(Value = "Ramp")]
        Ramp = 524291,
        [EnumMember(Value = "RFSensor")]
        RFSensor = 524293,    
        [EnumMember(Value = "WavetronixSS105")]
        WavetronixSS105 = 4295032837,
        [EnumMember(Value = "WavetronixSS125")]
        WavetronixSS125 = 8590000133, 
        //Masks
        DeviceTypeMask = 4294901760,
        GenericDeviceMask = 65535


    }
}
