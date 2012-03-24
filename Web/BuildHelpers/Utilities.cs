using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace ChipsWeb
{
    class Utilities
    {
        internal static ChipsDataContext GetDataContext()
        {
            return new ChipsDataContext();
        }

        internal static string GetJSON<T1>(User user)
        {
            throw new NotImplementedException();
        }

        internal static JavaScriptSerializer Serializer = new JavaScriptSerializer();

        internal static WebServer WebServer = new WebServer(8099, false);
    }
}
