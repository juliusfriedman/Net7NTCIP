using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.NTCIP
{
    /// <summary>
    /// powerSource:
    /// other: indicates that the sign is powered by a method not listed below (see device manual);
    /// powerShutdown: indicates that there is just enough power to perform shutdown activities.
    /// noSignPower: indicates that the sign controller has power but the sign display has no power;
    /// acLine: indicates that the controller and sign is powered by AC power
    /// generator: indicates that the sign and the controller are powered by a generator;
    /// solar: indicates that the sign and the controller are powered by solar equipment;
    /// batteryUPS: indicates that the sign and controller are powered by battery or UPS with no significant charging occurring.
    /// </summary>
    public enum powerSource : int //OBJECT-TYPE
    {
        other = 1,
        powerShutdown,
        noSignPower,
        acLine,
        generator,
        solar,
        batteryUPS
    }

    






}
