using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChipsWeb
{
    /// <summary>
    /// Summary description for UserWrapper
    /// </summary>
    public class UserWrapper
    {
        public UserWrapper() { }

        public UserWrapper(WebClient client) : this(client.LoginToken) { this.client = client; }

        public UserWrapper(Guid loginToken)
        {
            this.loginToken = loginToken;

            using (ChipsDataContext dc = Utilities.GetDataContext())
            {
                session = dc.GetUserSession(loginToken);
                user = session.User;
            }
        }

        WebClient client;
        Guid loginToken;
        User user;
        Session session;

        public WebClient Client
        {
            get { return client; }
        }

        public Guid PublicKey
        {
            get { return client.LoginToken; }
        }

        public Session Session
        {
            get { return session; }
        }

        public User User
        {
            get { return user; }
        }

        public Guid LoginToken
        {
            get { return loginToken; }
        }

        public override string ToString()
        {
            return Utilities.GetJSON<User>(user);
        }
    }
}

