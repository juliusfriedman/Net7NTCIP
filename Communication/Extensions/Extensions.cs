using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Communication
{
    public static class Extensions
    {
        public static bool IsBroadcast(this IPAddress address)
        {
            if (address.AddressFamily == AddressFamily.InterNetworkV6) return false;
            return (address.AddressFamily == IPAddress.Broadcast.AddressFamily);
        }
    }
}
