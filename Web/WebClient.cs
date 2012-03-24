namespace ChipsWeb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Net;
    using System.Web;
    /// <summary>
    /// TODO Performance Counters
    /// </summary>
    public class WebClient
    {
        #region Constants

#if DEBUG

        const string formatJSON =
@"{{
    'ListenId':{0},
    'LastListen':{1},
    'UserData':'{2}',
    'PublicKey':'{3}'
}}";

#else
        const string formatJSON = @"{{'ListenId':{0},'LastListen':{1},'UserData':'{2}','PublicKey':'{3}'}}";
#endif

        #endregion

        #region Nested Types

        public enum WebClientStatus
        {
            NotAvailable,
            Connected,
            Invisible = NotAvailable,
            Busy
        }

        #endregion

        #region Fields

        internal volatile HttpContextAdapter HttpContext;
        volatile List<WebMessage> Messages = new List<WebMessage>();
        internal DateTime LastListen;
        internal int ListenId = -1;

        #endregion

        #region Properties

        public bool Banned
        {
            get { return Server.Banned.Contains(HttpContext.RemoteIPAddress); }
            set { if (value) Ban(); else UnBan(); }
        }

        public Object UserData { get; set; }

        public Guid LoginToken { get; protected set; }

        public Guid PublicKey { get; protected set; }

        public WebServer Server { get; protected set; }

        public IPAddress RemoteIP
        {
            get { return HttpContext.RemoteIPAddress; }
        }

        public TimeSpan ListenTime
        {
            get { return DateTime.Now - LastListen; }
        }

        public int MessageCount
        {
            get { return Messages.Count; }
        }

        public bool DisconnectEligible
        {
            get { return !Server.Clients.Contains(this) || Server.DisconnectTime >= ListenTime && Server.DisconnectTime - ListenTime > Server.Wake; }
        }

        #endregion

        #region Constructor

        public WebClient(Guid loginToken, WebServer server)
        {
            if (loginToken.Equals(Guid.Empty)) throw new ArgumentException("loginToken");
            this.LoginToken = loginToken;
            this.PublicKey = new Guid();
            this.Server = server;
        }

        #endregion

        #region Internal Methods

        internal void BeginListenRequest(HttpContextAdapter context)
        {
            //Disconnect the current client
            EndListenRequest();

            //Increment state
            ListenId = Interlocked.Increment(ref ListenId);

            //Store the new listen
            HttpContext = context;
            HttpContext.BufferOutput = true;
            HttpContext.ResponseContentType = "text/json";
            LastListen = DateTime.Now;
        }

        /// <summary>
        /// Ends all ListenRequests
        /// </summary>
        internal void EndListenRequest()
        {
            lock (this)
            {
                EndRequest(HttpContext);
                HttpContext = null;
            }
        }

        internal void EndRequest(HttpContextAdapter adapter)
        {
            if (adapter == null) return;
            //Ensure there is only one thread working on this adapter
            lock (adapter)
            {
                //If there are messages to send, send them
                if (Messages.Count > 0)
                {
                    //Empty the volatile messages list into an array
                    WebMessage[] messages = Messages.ToArray();
                    Messages.Clear();

                    //Get the server encoding
                    Encoding encoding = Server.Encoding;

                    try
                    {
                        //FrameStart
                        adapter.OutputStream.WriteByte(encoding.GetBytes("[")[0]);
                    }
                    catch
                    {
                        //Reque the messages
                        Messages.AddRange(messages);
                        goto EndRequest;
                    }

                    byte[] binary;

                    //Get all of the messages except the last
                    if (messages.Length > 1)
                    {
                        //Iterate the array appending the JSON Object to the buffer
                        foreach (WebMessage message in messages.Take(Math.Max(1, messages.Length - 2)))
                        {
                            try
                            {
                                //Attempt to write the message
                                binary = encoding.GetBytes(message.ToJSON() + ',');
                                adapter.OutputStream.Write(binary, 0, binary.Length);
                            }
                            catch
                            {
                                //Reque the messages
                                Messages.Add(message);
                                continue;
                            }
                        }
                    }

                    //Get the last Message
                    WebMessage lastMessage = messages.Skip(messages.Length - 1).Take(1).Single();

                    try
                    {
                        //Attempt to write the lastMessage and FrameEnd
                        binary = encoding.GetBytes(lastMessage.ToJSON() + ']');
                        adapter.OutputStream.Write(binary, 0, binary.Length);
                    }
                    catch
                    {
                        //Reque the message
                        Messages.Add(lastMessage);
                        goto EndRequest;
                    }
                }

            EndRequest:
                //End the current listen
                try
                {
                    adapter.OutputStream.Flush();
                    adapter.OutputStream.Close();
                    adapter.Close();
                    adapter.End();
                }
                catch
                {
                    return;
                }
            }
        }

        #endregion

        #region Public Methods

        public void Release()
        {
            //Disconnect the current client
            EndListenRequest();
        }

        public void Que(WebMessage message)
        {
            Messages.Add(message);
        }

        public void Send(WebMessage message)
        {
            Que(message);
            Release();
        }

        public void Refresh()
        {
            PublicKey = new Guid();
            Server.SendAll(Server.CreateUserList());
        }

        public string ToJSON()
        {
            return string.Format(formatJSON, ListenId, LastListen.ToFileTimeUtc(), UserData.ToString(), PublicKey.ToString());
        }

        public void Ban()
        {
            Server.Ban(HttpContext.RemoteIPAddress);
        }

        public void UnBan()
        {
            Server.UnBan(HttpContext.RemoteIPAddress);
        }

        #endregion
    }
}
