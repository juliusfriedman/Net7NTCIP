using System;
using System.Linq;
using System.Web;
using System.Threading;

namespace ChipsWeb
{
    public class ListenHandler : IHttpAsyncHandler, IHttpHandler
    {
        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            Utilities.WebServer.WorkerFunction(context);
        }

        #endregion

        #region IHttpAsyncHandler Members

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            return new AsyncWebResult(Utilities.WebServer, cb, context);
        }

        public void EndProcessRequest(IAsyncResult result) { return; }

        #endregion
    }
}