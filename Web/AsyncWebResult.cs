namespace ChipsWeb
{
    using System;
    using System.Web;

    public class AsyncWebResult : IAsyncResult
    {
        #region Fields

        volatile WebServer server;
        volatile AsyncCallback asyncCallback;
        volatile HttpContext httpContext;
        volatile bool isCompleted = false;

        #endregion

        #region Constructor

        public AsyncWebResult(WebServer server, AsyncCallback callback, HttpContext context)
        {
            asyncCallback = callback;
            httpContext = context;  
            Server = server;                                 
        }

        #endregion

        #region IAsyncResult Members

        public HttpContext HttpContext
        {
            get { return httpContext; }
        }

        public object AsyncState { get; set; }

        System.Threading.WaitHandle IAsyncResult.AsyncWaitHandle
        {
            get { throw new NotImplementedException(); }
        }

        bool IAsyncResult.CompletedSynchronously
        {
            get { return false; }
        }

        public WebServer Server
        {
            get { return server; }
            set
            {
                server = value;
                server.WorkerFunction(httpContext);
                IsCompleted = true;
            }
        }

        public bool IsCompleted
        {
            get { return isCompleted; }
            set
            {
                if (!value) return;
                this.isCompleted = true;
                asyncCallback(this);
            }
        }

        #endregion
    }
}
