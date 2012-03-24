using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace ASTITransportation.Traffic.D
{

    //Enumerations
    public enum IOCapabilities : long
    {
        None = 0,
        RS232 = 1,
        RS485 = 2,
        Ethernet = 4,
        Other
    }

    //Interface

    public interface IDevice
    {
        
    }

    //Abstraction

    public abstract class BaseDevice : IDevice
    {
    }

    //Implmentation

    public class Device : BaseDevice
    {

        public void test()
        {
            List<SocketAddress> sa = new List<SocketAddress>();
            //sa[0].
        }

    }

}
