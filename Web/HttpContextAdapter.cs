namespace ChipsWeb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Net;
    using System.IO;
    using System.Collections.Specialized;

    public class HttpContextAdapter
    {
        #region Fields

        HttpContext httpContext;
        HttpListenerContext httpListenerContext;
        IPAddress remoteIp;
        string requestBody;

        #endregion

        #region Constructor

        internal HttpContextAdapter(object context)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (context is HttpContext) httpContext = (HttpContext)context;
            else if (context is HttpListenerContext) httpListenerContext = (HttpListenerContext)context;
            else throw new ArgumentException("context");
        }

        internal HttpContextAdapter(HttpContext context)
        {
            httpContext = context;
        }

        internal HttpContextAdapter(HttpListenerContext context)
        {
            httpListenerContext = context;
        }

        #endregion

        #region Properties

        public Stream InputStream
        {
            get { return httpListenerContext == null ? httpContext.Request.InputStream : httpListenerContext.Request.InputStream; }
        }

        public Stream OutputStream
        {
            get { return httpListenerContext == null ? httpContext.Response.OutputStream : httpListenerContext.Response.OutputStream; }
        }

        public Uri RequestUri
        {
            get { return httpListenerContext == null ? httpContext.Request.Url : httpListenerContext.Request.Url; }
        }

        public Encoding RequesteContentEncoding
        {
            get { return httpListenerContext == null ? httpContext.Request.ContentEncoding : httpListenerContext.Request.ContentEncoding; }
        }

        public Encoding ResponseContentEncoding
        {
            get { return httpListenerContext == null ? httpContext.Response.ContentEncoding : httpListenerContext.Response.ContentEncoding; }
        }

        public bool BufferOutput
        {
            get { return httpListenerContext == null ? httpContext.Response.BufferOutput : httpListenerContext.Response.SendChunked; }
            set { if (httpListenerContext == null) httpContext.Response.Buffer = httpContext.Response.BufferOutput = value; else httpListenerContext.Response.SendChunked = value; }

        }

        public NameValueCollection RequestHeaders
        {
            get { return httpListenerContext == null ? httpContext.Request.Headers : httpListenerContext.Request.Headers; }
        }

        public NameValueCollection ResponseHeaders
        {
            get { return httpListenerContext == null ? httpContext.Response.Headers : httpListenerContext.Response.Headers; }
        }

        public IPAddress RemoteIPAddress
        {
            get
            {
                if (remoteIp != null) return remoteIp;
                if (httpListenerContext == null)
                {
                    try
                    {
                        return remoteIp = IPAddress.Parse(httpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString());
                    }
                    catch
                    {
                        return remoteIp = IPAddress.Parse(httpContext.Request.UserHostAddress);
                    }
                } return remoteIp = httpListenerContext.Request.RemoteEndPoint.Address;
            }
        }

        public int ResponseStatusCode
        {
            get { return httpListenerContext == null ? httpContext.Response.StatusCode : httpListenerContext.Response.StatusCode; }
            set { if (httpListenerContext == null) httpContext.Response.StatusCode = value; else httpListenerContext.Response.StatusCode = value; }
        }

        public string RequestContentType
        {
            get { return httpListenerContext == null ? httpContext.Request.ContentType : httpListenerContext.Request.ContentType; }
        }

        public string ResponseContentType
        {
            get { return httpListenerContext == null ? httpContext.Response.ContentType : httpListenerContext.Response.ContentType; }
            set { if (httpListenerContext == null) httpContext.Response.ContentType = value; else httpListenerContext.Response.ContentType = value; }
        }

        public string RequestBody
        {
            get { if (null != requestBody) return requestBody; return requestBody = GetRequestBody(); }
        }

        #endregion

        #region Methods

        public string GetRequestCookie(string name)
        {
            try
            {
                if (httpListenerContext == null) return httpContext.Request.Cookies.Get(name).Value;
                else return httpListenerContext.Request.Cookies[name].Value;
            }
            catch
            {
                return null;
            }
        }

        public string GetResponseCookie(string name)
        {
            try
            {
                if (httpListenerContext == null) return httpContext.Response.Cookies.Get(name).Value;
                else return httpListenerContext.Response.Cookies[name].Value;
            }
            catch
            {
                return null;
            }
        }

        public string GetRequestCookie(int index)
        {
            try
            {
                if (httpListenerContext == null) return httpContext.Request.Cookies.Get(index).Value;
                else return httpListenerContext.Request.Cookies[index].Value;
            }
            catch
            {
                return null;
            }
        }

        public string GetResponseCookie(int index)
        {
            try
            {
                if (httpListenerContext == null) return httpContext.Response.Cookies.Get(index).Value;
                else return httpListenerContext.Response.Cookies[index].Value;
            }
            catch
            {
                return null;
            }
        }

        internal string GetRequestBody()
        {
            try
            {
                //If the request InputStream CanRead
                if (InputStream.CanRead)
                {
                    //Using a new StreamReader with the context Request Encoding, return the result of ReadToEnd
                    using (StreamReader sr = new StreamReader(InputStream, RequesteContentEncoding))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch
            {
                //Do Nothing
            }
            //Return EmptyString to stop property from being computed again
            return string.Empty;
        }

        internal Guid GetLoginToken(string tokenName)
        {
            //Determine if there is there is a loginToken identifying the request            
            try
            {
                //Try the headers
                return new Guid(RequestHeaders[tokenName]);
            }
            catch
            {
                try
                {
                    //Try the Cookies
                    return new Guid(GetRequestCookie(tokenName));
                }
                catch
                {
                    //Return Guid Empty
                    return Guid.Empty;
                }
            }
        }

        internal void End()
        {
            if (null == httpListenerContext)
            {
                httpContext.Response.End();
            }
        }

        internal void Close()
        {
            if (null == httpListenerContext)
            {
                httpContext.Response.Close();
            }
            else httpListenerContext.Response.Close();
        }

        #endregion
    }
}
