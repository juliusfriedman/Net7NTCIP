namespace ChipsWeb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Net;
    using System.Threading;
    using System.Web;
    /// <summary>
    /// TODO Performance Counters
    /// </summary>
    public class WebServer
    {
        #region Nested Types

        /// <summary>
        /// Handler for custom requests on the WebServer, relays the given parameters to the Delegate's Method.
        /// </summary>
        /// <param name="loginToken">The loginToken determined from parsing the SessionCookieName</param>
        /// <param name="context">The HttpContextAdapter which contains the Request and Response</param>
        public delegate void Relay(Guid loginToken, HttpContextAdapter context);

        /// <summary>
        /// Handler for custom requests on the WebServer, not yet used.
        /// </summary>
        /// <param name="loginToken">The loginToken determined from parsing the SessionCookieName</param>
        /// <param name="context">The HttpContextAdapter which contains the Request and Response</param>
        /// <param name="incoming">The incoming Message From the loginToken</param>
        public delegate void MessageHandler(Guid loginToken, HttpContextAdapter context, WebMessage incoming);

        /// <summary>
        /// Handler for Client Events in Programming
        /// </summary>
        /// <param name="client">The client</param>
        /// <param name="data">Data describing the event</param>
        public delegate void ClientEventHandler(WebClient client, object data);

        #endregion

        #region Constants

        //All Prefix parts start with this, {0} = the CommServer Port
        const string PartsStart = @"http://+:{0}/";

        //All Prefix parts end with this
        const string PartsEnd = @"/";

        //The name of the anonymous function
        const string Anonymous = "anonymous";

        //The default name of xml root nodes
        const string Data = "data";

        //The string which represents a null value in Javascript
        const string Null = "null";

#if DEBUG
        //The String to Format for Javascript
        const string FormatJavascriptFuction = @"(function {0}() {{ if(confirm('Debug Message?'))debugger; return {1}; }})({2});";
#else
        //The String to Format for Javascript
        const string FormatJavascriptFuction = @"(function {0}() {{ return {1}; }})({2});";
#endif

        //The String which represents a Javascript Code Block
        const string FormatJavascript = @"<script type='text/javascript'> {0} </script>";

        /// <summary>
        /// The String which represents a CSS Code Block
        /// </summary>
        const string FormatCSS = @"<style type='text/css'> {0} </style>";

        /// <summary>
        /// The String which represents a Html Page
        /// 0 = HtmlHead,
        /// 1 = Title,
        /// 2 = Time,
        /// 3 = RemoteAddress
        /// 4 = HtmlBody
        /// </summary>
        const string FormatHtmlPage =
@"<html>
    <head>
        {0}
        <title>{1}</title>
    </head>
    <!-- ServerPush.ashx Start Body @ [Time {2}], [RemoteAddress {3}] -->
    <body>
        {4}
    </body>
</html>";

        /// <summary>
        /// The String which represents a Xml Page
        /// 0 = XmlVersion,
        /// 1 = XmlEncoding,
        /// 2 = Time,
        /// 3 = RemoteAddress,
        /// 4 = RootNode,
        /// 5 = XmlBody
        /// </summary>
        const string FormatXml =
@"<?xml version='{0}' encoding='{1}'?>
<!-- ServerPush.ashx Start Xml @ [Time {2}], [RemoteAddress {3}] -->
<{4}>
    {5}    
</{4}>";

        //The String which represents the JSON Info of the CommServer
        const string FormatInfo =
@"{{    
    'Banned':[{0}],
    'Clients':[{1}],
    'Encoding':'{2}',
    'DisconnectTime':{3},
    'Wake':{4},
    'Map':[{5}],
    'InstanceId':{6},
    'Port':{7},
    'HeaderName': '{8}'
}}";

        //The landingPages we recognize
        static string[] Parts = new string[] { "Listen", "Send", "Info", "Status", "Javascript" };

        //The count of instantiated WebServers
        static int InstanceCounter = -1;

        #endregion

        #region Fields

        volatile internal int m_instanceId;
        volatile internal HttpListener m_Listener;
        volatile internal Thread m_WorkerThread;
        volatile int m_Port = -1;
        volatile List<IPAddress> m_Banned = new List<IPAddress>();
        volatile Dictionary<Guid, WebClient> m_Clients = new Dictionary<Guid, WebClient>();
        volatile Dictionary<string, Delegate> m_Sites = new Dictionary<string, Delegate>();
        volatile Dictionary<Guid, WebChat> m_Chats = new Dictionary<Guid, WebChat>();
        volatile String m_Identifier = "login";
        volatile bool m_iisListening;

        #endregion

        #region Properties

        /// <summary>
        /// The name of the HttpHeader or HttpCookie which identifies a given request and must be present
        /// </summary>
        public string Identifier
        {
            get { return m_Identifier; }
            set { m_Identifier = value; }
        }

        public int Port
        {
            get { return m_Port; }
            set { m_Port = Math.Max(1, Math.Min(value, 65535)); if (m_iisListening) return; Stop(); Start(); }
        }

        public bool IsListening
        {
            get { return m_iisListening || m_Listener.IsListening; }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<IPAddress> Banned
        {
            get { return m_Banned.AsReadOnly(); }
        }

        public Dictionary<Guid, WebClient>.ValueCollection Clients
        {
            get { return m_Clients.Values; }
        }

        public Dictionary<Guid, WebChat>.ValueCollection Chats
        {
            get { return m_Chats.Values; }
        }

        public System.Text.Encoding Encoding { get; set; }

        public TimeSpan DisconnectTime = TimeSpan.FromSeconds(24.9);

        public TimeSpan Wake = TimeSpan.FromMilliseconds(900.9);

        #endregion

        #region Events

        public event ClientEventHandler ClientConnected;

        public event ClientEventHandler ClientSend;

        public event ClientEventHandler ClientDisconnected;

        public event ClientEventHandler ClientRelease;

        #endregion

        #region Constructor

        public WebServer() : this(80, true) { }

        public WebServer(int port, bool iisListening)
        {
            if (!(m_iisListening = iisListening))
            {
                if (!HttpListener.IsSupported)
                {
                    throw new Exception("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                }
            }

            if (port <= 0 && port > 65535)
            {
                throw new ArgumentOutOfRangeException("port");
            }
            else
            {
                m_instanceId = Interlocked.Increment(ref InstanceCounter);
                Encoding = new System.Text.UTF8Encoding();
                //If IIS is already listening then return;
                if (m_iisListening) return;
                m_Port = port;
                m_Listener = new HttpListener();
                foreach (string part in Parts) m_Listener.Prefixes.Add(string.Concat(string.Format(PartsStart, m_Port), part, PartsEnd));
            }
        }

        #endregion

        #region Private Methods

        void PollClients(bool worker)
        {
            //Poll workers and End the Listen Requests which have timed out
            int sleep = 1;
            //Ensure only one thread polls at a time
            lock (m_Clients)
            {
                //Determine which clients are nearing the max request time
                //Iterate the clients who are being disconnected
                foreach (WebClient client in Clients.Where(c => c.DisconnectEligible && c.ListenTime.TotalMilliseconds >= (DisconnectTime.TotalMilliseconds / 2) && c.MessageCount > 5))
                {
                    client.Release();
                    ++sleep;
                }
            }

            //If a user left then send the userList
            if (sleep > 1) SendAll(CreateUserList());

            //Sleep a little bit if we are not a worker
            if (!worker) System.Threading.Thread.Sleep(Math.Max(100, sleep * 50));
        }

        void MainLoop()
        {
            //While we are still listening
            while (IsListening)
            {
                try
                {
                    //If IIS is not Listening
                    if (!m_iisListening)
                    {
                        //Get a Http Context
                        HttpListenerContext context = m_Listener.GetContext();

                        //Que the worker from the ThreadPool
                        ThreadPool.QueueUserWorkItem(new WaitCallback(WorkerFunction), context);
                    }
                }
                catch
                {
                    if (!IsListening) Start();
                }

                //Poll workers and End the Listen Requests which have timed out from the MainLoop
                PollClients(false);
            }
        }

        void ProcessListen(Guid loginToken, HttpContextAdapter adapter)
        {
            WebClient client;
            //If we have a client
            if (m_Clients.TryGetValue(loginToken, out client))
            {
                //Being the ListenRequest
                client.BeginListenRequest(adapter);
            }
            else
            {

                //Create a client and being the ListenRequest
                client = new WebClient(loginToken, this);

                //Assign the application UserData
                client.UserData = new UserWrapper(client);

                //Ensure only one client is added in a listen
                lock (m_Clients)
                {
                    //Add the client o the m_Clients Dictionary
                    m_Clients.Add(client.LoginToken, client);
                }

                //Give the client the most up to date WebServerInfo
                client.Que(new WebMessage("webServerInfo", ToJSON()));

                //Instuct the client he is connected
                client.Que(CreateEnterExitMessage(client, false));

                //Begin his listenRequest
                client.BeginListenRequest(adapter);

                //Give the connected users to all users
                SendAll(CreateUserList());
            }
        }

        void ProcessSend(Guid loginToken, HttpContextAdapter adapter)
        {
            WebClient client;
            //If we have a client process the Send
            if (m_Clients.TryGetValue(loginToken, out client))
            {
                //Get the body data
                string body = adapter.GetRequestBody();

                //If there is body data decode and handle the incoming message
                if (!string.IsNullOrEmpty(body))
                {
                    try
                    {
                        //Use JavaScriptSerializer from System.Web.Extensions to Decode Message 
                        Dictionary<string, object> message = Utilities.Serializer.DeserializeObject(body) as Dictionary<string, object>;
                        if (message != null)
                        {
                            //If we have a message determine the command
                            object value;
                            if (message.TryGetValue((String)message["Command"], out value))
                            {
                                //If we have a command attempt to process
                                string command = (String)value;
                                if (!string.IsNullOrEmpty(command))
                                {
                                    //Get the payload
                                    object payload;
                                    Dictionary<string, object> Payload = null;
                                    //If there was a payload unbox it
                                    if (message.TryGetValue("Payload", out payload)) Payload = payload as Dictionary<string, object>;
                                    //Determine hook and process                        
                                    switch (command)
                                    {                                       
                                        case "sendAll":
                                            {
                                                SendAll(new WebMessage(command, (String)message["Payload"]));
                                                return;
                                            }
                                        case "queAll":
                                            {
                                                QueAll(new WebMessage(command, (String)message["Payload"]));
                                                return;
                                            }
                                        case "sendToKey":
                                            {
                                                SendMessage((Guid)Payload["PublicKey"], new WebMessage(command, (String)Payload["Message"]));
                                                return;
                                            }
                                        case "queToKey":
                                            {
                                                QueMessage((Guid)Payload["PublicKey"], new WebMessage(command, (String)Payload["Message"]));
                                                return;
                                            }
                                        case "sendToChat":
                                            {
                                                break;
                                            }
                                        case "api":
                                        case "javascript":
                                            {
                                                ProcessJavascript(loginToken, adapter);
                                                return;
                                            }
                                        case "info":
                                        case "refresh":
                                            {
                                                ProcessInfo(loginToken, adapter);
                                                return;
                                            }
                                        case "status":
                                            {
                                                ProcessStatus(loginToken, adapter);
                                                return;
                                            }                                        
                                        case "push":
                                        case "serverPush":
                                            {
                                                ProcessServerPush(loginToken, adapter);
                                                return;
                                            }
                                        default:
                                            {
                                                ProcessUnknown(loginToken, adapter);
                                                return;
                                            }
                                    }
                                }
                            }
                            else
                            {
                                ProcessOther(loginToken, adapter);
                                return;
                            }
                        }
                    }
                    catch
                    {
                        //Do nothing;
                    }
                }

                //Push any message data at this time on the Response
                client.EndRequest(adapter);
            }
            else
            {
                //Indicate the client is not connected
                ProcessNoSession(adapter);
            }
        }

        private void ProcessServerPush(Guid loginToken, HttpContextAdapter adapter)
        {
            throw new NotImplementedException();
        }

        void ProcessDisconnect(Guid loginToken, HttpContextAdapter adapter)
        {
            WebClient client;
            //If we have a client
            if (m_Clients.TryGetValue(loginToken, out client))
            {
                //Inform the client of the Disconnect
                client.Que(CreateEnterExitMessage(client, true));

                //Push any message data at this time on the Response
                client.EndRequest(adapter);

                //Remove the Client from the Server
                RemoveClient(ref client);
            }
            else
            {
                //Indicate the client is not connected
                ProcessNoSession(adapter);
            }
        }

        void ProcessInfo(Guid loginToken, HttpContextAdapter adapter)
        {

            //Construct a response.
            WebMessage message = new WebMessage("webServerInfo", ToJSON());

            //If we have a client then process the request as a message
            WebClient client;
            if (m_Clients.TryGetValue(loginToken, out client))
            {
                //Que the Information to the client
                client.Que(message);

                //End the request on the currentContext
                client.EndRequest(adapter);

            }
            else
            {
                //Write response
                WriteContextResponse(adapter, 200, "text/html", message.ToJSON(), Encoding);
                //WriteContextResponse(context, message.ToJSON(), "You have recieved the Server Information via JSON RPC, Please see the HEAD of this response!");
            }

        }

        void ProcessStatus(Guid loginToken, HttpContextAdapter context)
        {
            //TODO
        }

        void ProcessOther(Guid loginToken, HttpContextAdapter adapter)
        {
            if (null == adapter) return;
            //Determine if the Uri matches a page in the custom sites
            string page = m_Sites.Keys.SingleOrDefault(p => adapter.RequestUri.AbsoluteUri.Contains(p));
            //If the page is not null then invoke the handler
            if (page != null)
            {
                //Get the handler Delegate from the Sites Dictionary
                Delegate handler = m_Sites[page];
                //If there is handler
                if (handler != null)
                {
                    //If the Delegate is a Relay or MessageHandler then Invoke it
                    if (handler is Relay) ((Relay)handler).Invoke(loginToken, adapter);
                    else if (handler is MessageHandler) ((MessageHandler)handler).Invoke(loginToken, adapter, null);
                    else
                    {
                        try
                        {
                            //The handler is a Delegate, try to Invoke it
                            handler.DynamicInvoke(new object[] { loginToken, adapter });
                        }
                        catch
                        {
                            //Throw the exception
                            throw;
                        }
                    }
                }
            }

            //Otherwise indicate the process is unknown
            ProcessUnknown(loginToken, adapter);
        }

        void ProcessUnknown(Guid loginToken, HttpContextAdapter adapter)
        {
            //Write response with updated serverInfo
            WriteContextResponseHtml(adapter, 404, CreateJavascriptFunctionBlock(Anonymous, ToJSON(), null), "Error - 404", "The page you requested could not found, please try again!");//+ context.Request.Url.AbsoluteUri
        }

        void ProcessJavascript(Guid loginToken, HttpContextAdapter adapter)
        {
            //Construct a response.            
            string responseString = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\resources\javascript\Classes\WebServerAPI.js").Replace("'<ServerObject>'", ToJSON());

            //Write response
            WriteContextResponse(adapter, 200, "text/javascript", responseString, Encoding);
        }

        void ProcessBanned(HttpContextAdapter context)
        {
            //Write response
            WriteContextResponseHtml(context, 500, null, "RemoteAddress Banned", "Sorry your IPAddress have been banned by the Administrator.");
        }

        void ProcessNoSession(HttpContextAdapter context)
        {
            //Write response
            WriteContextResponseHtml(context, 500, null, "Identification Error", string.Format("Sorry, Neither a HttpHeader or Cookie named '{0}' could be found that contained valid data identifying the session, please ensure the necessary data was passed in the request and try again.", m_Identifier));
        }

        //All these methods are candidates to me be moved to the adapter


        void WriteContextResponseJson(HttpContextAdapter adapter, string functionName, string functionCode, string functionArguments)
        {
            //If there is no context then there is nothing we can do
            if (adapter == null) return;
            //Write response
            WriteContextResponse(adapter, 200, "text/javascript", CreateJavascriptFunction(functionName, functionCode, functionArguments), Encoding);
        }

        void WriteContextResponseXml(HttpContextAdapter adapter, double xmlVersion, string rootNode, string xmlBody)
        {
            if (adapter == null) return;
            //Write the response
            WriteContextResponse(adapter, 200, "text/xml", CreateXmlPage(adapter.RemoteIPAddress.ToString(), xmlVersion, Encoding, rootNode, xmlBody), Encoding);
        }

        void WriteContextResponse(HttpContextAdapter adapter, int statusCode, string contentType, string payload, System.Text.Encoding encoding)
        {
            try
            {
                if (null == adapter) return;
                if (null == encoding) encoding = Encoding;
                if (null == contentType) contentType = "text/plain";
                adapter.ResponseContentType = contentType;
                byte[] binary = encoding.GetBytes(payload);
                adapter.OutputStream.Write(binary, 0, binary.Length);
                adapter.OutputStream.Close();
                //adapter.End();
            }
            catch
            {
                return;
            }
        }

        string CreateXmlPage(string remoteAddress, double xmlVersion, System.Text.Encoding encoding, string rootNode, string xmlBody)
        {
            if (null == encoding) encoding = Encoding;
            if (null == rootNode) rootNode = Data;
            if (null == xmlBody) xmlBody = string.Empty;
            return string.Format(FormatXml, xmlVersion.ToString(), encoding.EncodingName, DateTime.Now.ToString(), remoteAddress, rootNode, xmlBody);
        }

        string CreateCSSBlock(string cssCode)
        {
            return string.Format(FormatCSS, cssCode);
        }

        string CreateJavascriptBlock(string javascriptCode)
        {
            if (string.IsNullOrEmpty(javascriptCode)) javascriptCode = Null;
            return string.Format(FormatJavascript, javascriptCode);
        }

        string CreateJavascriptFunction(string functionName, string functionCode, string functionArguments)
        {
            if (string.IsNullOrEmpty(functionName)) functionName = Anonymous;
            if (null == functionCode) functionCode = Null;
            if (null == functionArguments) functionArguments = Null;
            return string.Format(FormatJavascriptFuction, functionName, functionCode, functionArguments);
        }

        string CreateJavascriptFunctionBlock(string functionName, string functionCode, string functionArguments)
        {
            return string.Format(FormatJavascript, CreateJavascriptFunction(functionName, functionCode, functionArguments));
        }

        string CreateHtmlPage(string remoteAddress, string responseHead, string responseTitle, string responseBody)
        {
            return string.Format(FormatHtmlPage, responseHead, responseTitle, DateTime.UtcNow.ToString(), remoteAddress, responseBody);
        }

        void WriteContextResponseHtml(HttpContextAdapter adapter, int statusCode, string responseHead, string responseTitle, string responseBody)
        {
            adapter.ResponseStatusCode = statusCode;
            adapter.ResponseContentType = "text/html";
            byte[] binary = Encoding.GetBytes(CreateHtmlPage(adapter.RemoteIPAddress.ToString(), responseHead, responseTitle, responseBody));
            adapter.OutputStream.Write(binary, 0, binary.Length);
            adapter.OutputStream.Close();
            //adapter.End();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Accepts the Request
        /// </summary>
        /// <param name="state">HttpContext</param>
        internal void WorkerFunction(object state)
        {
            //If the state is not present then return
            if (null == state) return;
            //Create an HttpContext from the state
            HttpContextAdapter adapter = new HttpContextAdapter(state);
            //get the remoteIp
            IPAddress remoteIp = adapter.RemoteIPAddress;
            //If the RemoteEndPoint is Banned then Handle this scenario
            if (m_Banned.Contains(remoteIp))
            {
                ProcessBanned(adapter);
            }
            else
            {
                try
                {
                    //Get the loginToken from the request
                    Guid loginToken = adapter.GetLoginToken(m_Identifier);
                    if (loginToken.Equals(Guid.Empty)) throw new ArgumentException("loginToken");

                    //Determine the process
                    string process = Parts.SingleOrDefault(p => adapter.RequestUri.AbsoluteUri.Contains(p));

                    //Process the request according to the Determined ordinal
                    switch (process)
                    {
                        default: ProcessOther(loginToken, adapter); break;
                        case "Listen": ProcessListen(loginToken, adapter); break;
                        case "Send": ProcessSend(loginToken, adapter); break;
                        case "Info": ProcessInfo(loginToken, adapter); break;
                        case "Status": ProcessStatus(loginToken, adapter); break;
                        case "Javascript": ProcessJavascript(loginToken, adapter); break;
                    }
                }
                catch
                {
                    //There was an malformed loginToken
                    ProcessNoSession(adapter);
                }
            }

            //Poll workers and End the Listen Requests which have timed out from the Worker
            PollClients(true);
        }

        internal WebMessage CreateEnterExitMessage(WebClient client, bool exit)
        {
            return new WebMessage(exit ? "goodbyeClient" : "welcomeClient", client.ToJSON());
        }

        internal void MoveClientTo(WebClient client, WebServer other)
        {
            //Can't move a client to a server it is already on, or that is not listening
            if (client.Server.m_instanceId == other.m_instanceId || !other.IsListening) return;
            RemoveClient(client.LoginToken);
            //Ensure synchronization
            lock (other)
            {
                other.ProcessListen(client.LoginToken, client.HttpContext);
            }
        }

        internal void RemoveClient(Guid loginToken)
        {
            WebClient client;
            //If there is a session with the given loginToken
            if (m_Clients.TryGetValue(loginToken, out client))
            {
                //Empty all chats the client owned
                foreach (WebChat chat in Chats.Where(c => c.Owner.LoginToken.Equals(loginToken))) RemoveChat(chat.RoomId);
                //Remove the client
                RemoveClient(ref client);
            }
        }

        internal void RemoveClient(ref WebClient client)
        {
            lock (m_Clients)
            {
                client.Release();
                m_Clients.Remove(client.LoginToken);
                client = null;
            }
        }

        internal void RemoveChat(Guid roomId)
        {
            WebChat chat;
            //If there is a chat with the given roomId
            if (m_Chats.TryGetValue(roomId, out chat))
            {
                //Remove the chat
                RemoveChat(ref chat);
            }
        }

        internal void RemoveChat(ref WebChat chat)
        {
            lock (m_Chats)
            {
                //Empty the chat
                chat.Empty();
                //Remove the existing chat
                m_Chats.Remove(chat.RoomId);
                chat = null;
            }
        }

        internal void SendTo(Guid loginToken, WebMessage message)
        {
            WebClient client;
            if (m_Clients.TryGetValue(loginToken, out client))
            {
                client.Send(message);
            }
        }

        internal void SendTo(Guid[] loginTokens, WebMessage message)
        {
            foreach (Guid loginToken in loginTokens) SendTo(loginToken, message);
        }

        internal void QueTo(Guid loginToken, WebMessage message)
        {
            WebClient client;
            if (m_Clients.TryGetValue(loginToken, out client))
            {
                client.Que(message);
            }
        }

        internal void QueTo(Guid[] loginTokens, WebMessage message)
        {
            foreach (Guid loginToken in loginTokens) QueTo(loginToken, message);
        }

        internal void QueAllBut(Guid loginToken, WebMessage message)
        {
            foreach (WebClient client in Clients.Where(c => !c.LoginToken.Equals(loginToken)))
            {
                client.Que(message);
            }
        }

        internal void SendAllBut(Guid loginToken, WebMessage message)
        {
            foreach (WebClient client in Clients.Where(c => !c.LoginToken.Equals(loginToken)))
            {
                client.Send(message);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Send the given WebMessage to all WebClients immediately
        /// </summary>
        /// <param name="message">The message</param>
        public void SendAll(WebMessage message)
        {
            foreach (WebClient client in Clients)
            {
                client.Send(message);
            }
        }

        /// <summary>
        /// Que the given WebMessage to all WebClients
        /// </summary>
        /// <param name="message">The message</param>
        public void QueAll(WebMessage message)
        {
            foreach (WebClient client in Clients)
            {
                client.Que(message);
            }
        }

        /// <summary>
        /// Create a WebMessage which contains only the active users
        /// </summary>
        /// <returns>A WebMessage which contains the JSON Array of WebClients currently connected to the WebServer</returns>
        public WebMessage CreateUserList()
        {
            //Return the message
            return new WebMessage("userList", '[' + string.Join(",", Clients.Select(c => c.ToJSON()).ToArray()) + ']');
        }

        /// <summary>
        /// Prevent the given IPAddres from connecting to this WebServer
        /// </summary>
        /// <param name="ipAddress">The IPAddress</param>
        public void Ban(IPAddress ipAddress)
        {
            //Ensure only 1 thread in m_Banned
            lock (m_Banned)
            {
                //If the given IPAddress was not banned then we have nothing to do, else remove it from the ban list
                if (m_Banned.Contains(ipAddress)) return;
                else m_Banned.Add(ipAddress);
            }

            //Keep track is a user was effected by this ban
            bool someoneLeft = false;

            //Foreach client if the client is now banned
            Clients.ToList().ForEach(c =>
            {
                //If the banned list contains a connected client then Disconnect him
                if (m_Banned.Contains(c.RemoteIP))
                {
                    someoneLeft |= true;
                    //Disconnect the client
                    c.Release();
                    //Remove the client from the Server
                    m_Clients.Remove(c.LoginToken);
                }
            });

            //If someone was removed then update all users
            if (someoneLeft) SendAll(CreateUserList());
        }

        /// <summary>
        /// Allow the given IPAddres to connect to this WebServer if it was previously banned
        /// </summary>
        /// <param name="ipAddress">The IPAddress</param>
        public void UnBan(IPAddress ipAddress)
        {
            m_Banned.Remove(ipAddress);
        }

        /// <summary>
        /// Starts the HttpListener if required and runs the MainLoop on a MTA Thread with AboveNormal Priority
        /// </summary>
        public void Start()
        {
            try
            {
                if (!m_iisListening) m_Listener.Start();
                if (m_WorkerThread == null)
                {
                    m_WorkerThread = new Thread(MainLoop);
                    m_WorkerThread.SetApartmentState(ApartmentState.MTA);
                    m_WorkerThread.Name = "CommServer-" + (m_instanceId);
                    m_WorkerThread.Priority = ThreadPriority.AboveNormal;
                    m_WorkerThread.Start();
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Stops the HttpListener if required and Joins the MainLoop.        
        /// </summary>
        public void Stop()
        {
            if (!IsListening) return;
            try
            {
                //Disconnect all currently connected users
                DisconnectAll();
                //Stop the HttpListener if required
                if (!m_iisListening)
                {
                    Abort();
                    Close();
                    m_Listener.Stop();
                }
                //Join the worker thread
                m_WorkerThread.Join();
                m_WorkerThread = null;
            }
            catch
            {
                //Throw any exceptions which occur
                throw;
            }
        }

        /// <summary>
        /// Aborts the HttpListener if required 
        /// </summary>
        public void Abort()
        {
            try
            {
                if (!m_iisListening) m_Listener.Abort();
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// Closes the HttpListener if required
        /// </summary>
        public void Close()
        {
            if (!m_iisListening) m_Listener.Close();
        }

        /// <summary>
        /// Send a WebMessage to a single WebClient
        /// </summary>
        /// <param name="publicKey">The PublicKey of the WebClient</param>
        /// <param name="message">The Message</param>
        public void SendMessage(Guid publicKey, WebMessage message)
        {
            WebClient client = Clients.SingleOrDefault(c => c.PublicKey.Equals(publicKey));
            if (null != client) client.Send(message);
        }

        /// <summary>
        /// Que a WebMessage to a single WebClient 
        /// </summary>
        /// <param name="publicKey">The PublicKey of the WebClient</param>
        /// <param name="message">The Message</param>
        public void QueMessage(Guid publicKey, WebMessage message)
        {
            WebClient client = Clients.SingleOrDefault(c => c.PublicKey.Equals(publicKey));
            if (null != client) client.Que(message);
        }

        /// <summary>
        /// Send a WebMessage to the given users
        /// </summary>
        /// <param name="publicKey">The PublicKey's of the WebClients</param>
        /// <param name="message">The Message</param>
        public void SendMessage(Guid[] publicKeys, WebMessage message)
        {
            foreach (Guid publicKey in publicKeys)
            {
                WebClient client = Clients.SingleOrDefault(c => c.PublicKey.Equals(publicKey));
                if (null != client) client.Send(message);
            }
        }

        /// <summary>
        /// Que a WebMessage to the given users
        /// </summary>
        /// <param name="publicKey">The PublicKey's of the WebClients</param>
        /// <param name="message">The Message</param>
        public void QueMessage(Guid[] publicKeys, WebMessage message)
        {
            foreach (Guid publicKey in publicKeys)
            {
                WebClient client = Clients.SingleOrDefault(c => c.PublicKey.Equals(publicKey));
                if (null != client) client.Que(message);
            }
        }

        /// <summary>
        /// Add a MessageHandler to handle a request
        /// </summary>
        /// <param name="part">The string which identifies the request</param>
        /// <param name="handler">The MessageHandler to invoke</param>
        /// <param name="qualify">Indicates if the part should be turned into a fully qualified Uri</param>
        public void AddSite(string part, Delegate handler, bool qualify)
        {
            try
            {
                //Turn the landing into a Fully Qualified Uri if required
                if (qualify) part = string.Concat(string.Format(PartsStart, m_Port), part, PartsEnd);
                //If we need to augment the HttpListener then do so
                if (!m_iisListening)
                {
                    //Add it to the Listener's Prefix's
                    m_Listener.Prefixes.Add(part);
                }
                //Add the Relay 
                m_Sites.Add(part, handler);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Remove a regisered Prefix from the Listener and the custom sites Dictionary
        /// </summary>
        /// <param name="part">The part string which identified the custom site</param>
        /// <param name="qualify">Indicates if the part should be turned into a fully qualified uri.</param>
        public void RemoveSite(string part, bool qualify)
        {
            //Turn the landing into a Fully Qualified Uri if required
            if (qualify) part = string.Concat(string.Format(PartsStart, m_Port), part, PartsEnd);
            //Remove it from the Listener's Prefix's
            lock (m_Listener)
            {
                m_Listener.Prefixes.Remove(part);
            }
            //Remove it from the Sites Map
            lock (m_Sites)
            {
                m_Sites.Remove(part);
            }
        }

        public WebChat StartChat(WebClient owner, string roomName)
        {
            //Find out of the owner has already stated this chat
            WebChat chat = Chats.SingleOrDefault(c => c.Owner.Equals(owner) && c.RoomName.ToLowerInvariant().Equals(roomName.ToLowerInvariant()));
            //If there is already a chat with the same name
            if (null != chat)
            {
                //There is alredy a chat with the same name, return it
                return chat;
            }
            else
            {
                //Ensure on one thread is adding a chat
                lock (m_Chats)
                {
                    //Create a new chat
                    chat = new WebChat(owner, roomName);
                    //Add it to the Server
                    m_Chats.Add(chat.RoomId, chat);
                }
            }
            //Return the chat
            return chat;
        }

        /// <summary>
        /// Empties the WebChat with the given roomId by removing all users from the WebChat 
        /// and then subsequently removing the WebChat from the Server in which it was created.
        /// </summary>
        /// <param name="roomId">The roomId of the WebChat to Empty</param>
        public void EmptyChat(Guid roomId)
        {
            WebChat chat;
            if (m_Chats.TryGetValue(roomId, out chat))
            {
                chat.Empty();
            }
        }

        /// <summary>
        /// Disconnect all of the currently connected clients from the WebServer
        /// </summary>
        public void DisconnectAll()
        {
            Clients.ToList().ForEach(c => c.Release());
        }

        /// <summary>
        /// Provides the JSON Representation of the WebServer
        /// </summary>
        /// <returns></returns>
        public string ToJSON()
        {
            return string.Format(FormatInfo,
                string.Join(",", Banned.Select(ban => ban.ToString()).ToArray()),
                string.Join(",", Clients.Select(c => c.ToJSON()).ToArray()),
                Encoding.EncodingName,
                DisconnectTime.TotalMilliseconds.ToString(),
                Wake.TotalMilliseconds.ToString(),
                string.Join(",", Parts.Union(m_Sites.Keys).Select(p => '\'' + p + '\'').ToArray()),
                m_instanceId.ToString(),
                m_Port.ToString(),
                m_Identifier);
        }

        #endregion

        internal void RemoveSharedObject(Guid m_ResourceId)
        {
            throw new NotImplementedException();
        }

        internal void RemoveWebObject(Guid m_ResourceId)
        {
            throw new NotImplementedException();
        }
    }
}
