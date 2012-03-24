namespace ChipsWeb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class WebChat
    {
        #region Constants

        const string formatJSONMessage = @"{{'UserKey':{0},'RoomId':'{1}','Message':'{2}'}}";

        const string formatJSONInfo = @"{{'RoomId':'{0}','RoomName':'{1}','OwnerKey':{2},'InviteOnly':{3},'PresentKeys':[{4}],'InvitedKeys':[{5}],'BannedKeys':[{6}]}}";

        const string formatJSONEnterExit = @"{{'UserKey':{0},'ChatInfo':{1}}}";

        #endregion

        #region Fields

        Guid m_RoomId = Guid.NewGuid();
        volatile List<WebClient> m_Present = new List<WebClient>();
        volatile List<WebClient> m_Invited = new List<WebClient>();
        volatile List<WebClient> m_Banned = new List<WebClient>();
        bool m_InviteOnly;
        volatile string m_RoomName;
        WebClient m_Owner;
        //Could have banner and sounds etc and allow them to be shared in chat? maybe each chat should have a WebShare....
        //WebShare m_Share;

        #endregion

        #region Properties

        public Guid RoomId
        {
            get { return m_RoomId; }
        }

        public bool InviteOnly
        {
            get { return m_InviteOnly; }
            set
            {
                m_InviteOnly = value; 
                SendChatInfo();
            }
        }

        public string RoomName
        {
            get { return m_RoomName; }
            set
            {
                m_RoomName = value; 
                SendChatInfo();
            }
        }

        public WebClient Owner
        {
            get { return m_Owner; }
        }

        public WebServer Server
        {
            get { return m_Owner.Server; }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<WebClient> Present
        {
            get { return m_Present.AsReadOnly(); }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<WebClient> Invited
        {
            get { return m_Invited.AsReadOnly(); }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<WebClient> Banned
        {
            get { return m_Banned.AsReadOnly(); }
        }

        #endregion

        #region Constructor

        internal WebChat(WebClient owner, string roomName)
        {
            m_Owner = owner;
            m_RoomName = roomName;
        }

        #endregion

        #region Methods

        public void Empty()
        {
            lock (this)
            {
                //Tell the chat user is leaving                
                foreach (WebClient client in Present) LeaveChat(client);
            }
        }

        public void Say(WebClient client, string what)
        {
            //Create the chatMessage
            WebMessage message = new WebMessage("chatMessage", string.Format(formatJSONMessage, client.ToJSON(), m_RoomId.ToString(), what));

            //Use the server to send the message to the present users
            Server.SendTo(Present.Select(c => c.LoginToken).ToArray(), message);
        }

        public void JoinChat(WebClient client)
        {
            List<WebClient> clients;

            //If the chat is InviteOnly and the client is not invited and he is not the owner
            if (InviteOnly && client != m_Owner)
            {
                lock (m_Invited)
                {
                    //If he was not invited return
                    if (!m_Invited.Contains(client) && client != m_Owner) return;
                }
            }

            //Ensure only 1 thread is in m_Present
            lock (m_Present)
            {
                //Ensure 1 thread in m_Banned
                lock (m_Banned)
                {
                    //If the client has already joined or he is banned we don't need to do anything else
                    if (m_Present.Contains(client) || m_Banned.Contains(client)) return;
                }
                //Get a list of clients to alert
                clients = m_Present.ToList();
                //Add the user to the present clients
                m_Present.Add(client);
            }

            //Create the message
            WebMessage message = CreateEnterExitMessage(client, false);

            //Indicate to all OTHER users that someone entered the chat                        
            clients.ForEach(c => c.Que(message));

            //Send the updated userList
            SendChatInfo();
        }

        public void LeaveChat(WebClient client)
        {
            bool someoneLeft = false;
            List<WebClient> clients;

            //Ensure only 1 thread is adding or removing
            lock (m_Present)
            {
                //Determine if someone really left
                someoneLeft = m_Present.Remove(client);
                clients = m_Present.ToList();
            }

            //If no-one left then return
            if (!someoneLeft) return;

            WebMessage message = CreateEnterExitMessage(client, true);

            //Indicate to all OTHER users that someone entered the chat            
            clients.ForEach(c => c.Que(message));

            //If there is no one left then remove the chat, otherwise send the ChatInfo
            if (m_Present.Count == 0) Server.RemoveChat(RoomId);
            else SendChatInfo();
        }

        public void SendChatInfo()
        {
            //Create the information message
            WebMessage message = new WebMessage("chatInfo", ToJSON());
            //Use the serer to send the message to the present users
            Server.SendTo(Present.Select(c => c.LoginToken).ToArray(), message);
        }

        public string ToJSON()
        {
            return string.Format(formatJSONInfo,
                m_RoomId.ToString(),
                m_RoomName,
                m_Owner.PublicKey.ToString(),
                m_InviteOnly.ToString().ToLowerInvariant(),
                string.Join(",", Present.Select(p => p.PublicKey.ToString()).ToArray()),
                string.Join(",", Invited.Select(i => i.PublicKey.ToString()).ToArray()),
                string.Join(",", Banned.Select(b => b.PublicKey.ToString()).ToArray()));
        }

        public void Ban(WebClient client)
        {
            if (Banned.Contains(client)) return;

            //Ensure only 1 thread in m_Banned
            lock (m_Banned)
            {
                m_Banned.Add(client);
            }

            //If the user was present then make him leave
            if (Present.Contains(client))
            {
                //Tell the client he was banned
                client.Que(new WebMessage("chatBan", ToJSON()));
                LeaveChat(client);
            }
        }

        public void UnBan(WebClient client, bool invite)
        {
            m_Banned.Remove(client);
            if (invite) Invite(client);
        }

        public void InviteAll()
        {
            Invite(Invited.Where(c=>!Present.Contains(c)));
        }

        internal void Invite(WebClient client)
        {
            if (!m_Invited.Contains(client)) m_Invited.Add(client);
            client.Send(new WebMessage("chatInvite", ToJSON()));
        }

        internal void Invite(IEnumerable<WebClient> clients)
        {           
            foreach (WebClient client in clients) Invite(client);
        }

        internal WebMessage CreateEnterExitMessage(WebClient client, bool exit)
        {
            return new WebMessage(exit ? "leaveChat" : "enterChat", string.Format(formatJSONEnterExit, client.PublicKey.ToString(), ToJSON()));
        }

        #endregion
    }
}
