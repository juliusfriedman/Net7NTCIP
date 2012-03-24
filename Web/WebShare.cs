namespace ChipsWeb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Collections.ObjectModel;

    public class WebShare
    {
        #region Constants

        internal const string FormatJSON =
@"{{
    'ShareInfo':{0},
    'AccessControl':{1},
    'Resources':{2}
}}";

        internal const string FormatInfo =
@"{{
    'ShareId':{0},
    'OwnerKey':'{1}',
    'ShareName':{2}',
    'Public':{3},    
    'InvitedKeys':[{4}],
    'PresentKeys':[{5}]
}}";

        internal const string FormatACL =
@"{{
    'ShareId':'{1}',
    'AccessControl':{2}    
}}";

        internal const string FormatUpdate =
@"{{
    'ResourceId':'{1}',
    'Update':{2}    
}}";

        internal const string FormatResource =
@"{{
    {0}:{1}    
}}";

        #endregion

        #region Nested Types

        [Flags]
        public enum WebPermission
        {
            None = 0,
            Create = 1,
            Read = 2,
            Update = 4,
            Delete = 8,
            All = Create | Read | Update | Delete
        }

        #endregion

        #region Fields
        
        internal bool m_Public = true;
        internal WebClient m_Owner;
        internal Guid m_ShareId;
        internal string m_ShareName;
        internal volatile Dictionary<Guid, WebObject> m_Resources = new Dictionary<Guid, WebObject>();
        internal volatile Dictionary<WebClient, WebPermission> m_AccessControlDictionary = new Dictionary<WebClient, WebPermission>();
        internal volatile List<WebClient> m_Invited = new List<WebClient>();
        internal volatile List<WebClient> m_Present = new List<WebClient>();

        #endregion

        #region Properties

        public Dictionary<Guid, WebObject>.ValueCollection Resources
        {
            get { return m_Resources.Values; }
        }

        public bool Public
        {
            get { return m_Public; }
        }

        public ReadOnlyCollection<WebClient> Invited
        {
            get { return m_Invited.AsReadOnly(); }
        }

        public ReadOnlyCollection<WebClient> Present
        {
            get { return m_Invited.AsReadOnly(); }
        }

        public WebClient Owner
        {
            get { return m_Owner; }
            set
            {
                m_Owner = value;
                Refresh();
            }
        }

        public Guid ShareId
        {
            get { return m_ShareId; }
        }

        public string ShareName
        {
            get { return m_ShareName; }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                m_ShareName = value;
                Refresh();
            }
        }

        #endregion

        #region Constructor

        public WebShare(WebClient owner, string shareName)
        {
            if (string.IsNullOrEmpty(shareName)) throw new ArgumentNullException("shareName");
            m_Owner = owner;
            m_ShareName = shareName;
            m_ShareId = new Guid();
        }

        #endregion

        #region Methods

        public void InviteAll()
        {
            Invite(Invited.Where(c => !Present.Contains(c)));
        }

        internal void Invite(WebClient client)
        {
            if (!Invited.Contains(client)) m_Invited.Add(client);
            client.Send(new WebMessage("webShareInvite", ToJSON()));
        }

        internal void Invite(IEnumerable<WebClient> clients)
        {
            foreach (WebClient client in clients) Invite(client);
        }

        internal void Refresh()
        {
            Refresh(Present);
        }

        internal void Refresh(IEnumerable<WebClient> clients)
        {
            foreach (WebClient client in clients) Refresh(client);
        }

        internal void Refresh(WebClient client)
        {
            client.Send(new WebMessage("webShareRefresh", ToJSON()));
        }

        public void SetPermissions(WebClient client, WebPermission permission)
        {
            if (!Invited.Contains(client)) return;

            WebPermission lastPermission;

            if (m_AccessControlDictionary.TryGetValue(client, out lastPermission))
            {
                m_AccessControlDictionary[client] = permission;
                if (permission == WebPermission.None) LeaveShare(client);
            }
            else
            {
                lock (m_AccessControlDictionary)
                {
                    m_AccessControlDictionary.Add(client, permission);
                }
            }

        }

        public void JoinShare(WebClient client)
        {
            List<WebClient> clients;
            //Ensure only 1 thread is in m_Present
            lock (m_Present)
            {
                //Get a list of clients to alert
                clients = m_Present.ToList();
                //Add the user to the present clients
                m_Present.Add(client);
            }

            //Send the updated userList
            Refresh();
        }

        public void LeaveShare(WebClient client)
        {
            bool someoneLeft = false;

            //Ensure only 1 thread is adding or removing
            lock (m_Present)
            {
                //Determine if someone really left
                someoneLeft = m_Present.Remove(client);
            }

            //If no-one left then return
            if (!someoneLeft) return;

            //If there is no one left then remove the chat, otherwise send the ChatInfo
            if (m_Present.Count == 0) m_Owner.Server.RemoveWebObject(m_ShareId);
            else Refresh();
        }

        public void Ban(WebClient client)
        {
            if (Invited.Contains(client)) return;

            //Ensure only 1 thread in m_Invited
            lock (m_Invited)
            {
                m_Invited.Add(client);
            }

            //If the user was present then make him leave
            if (Present.Contains(client))
            {
                //Tell the client he was banned
                client.Que(new WebMessage("shareBan", ToJSON()));
                LeaveShare(client);
            }
        }

        public void UnBan(WebClient client, bool invite)
        {
            LeaveShare(client);
            m_Invited.Remove(client);
            if (invite) Invite(client);
        }

        public WebObject GetObject(Guid objectId)
        {
            WebObject result;
            if (m_Resources.TryGetValue(objectId, out result))
            {
                return result;
            }
            return null;
        }

        public WebObject[] GetObjects(string name)
        {
            return Resources.Where(r => r.ResourceName.Equals(name)).ToArray();
        }

        public string ToJSON()
        {
            return string.Format(FormatJSON, CreateInfoMessage().ToJSON(), CreateAccessControlMessage().ToJSON(), CreateResourcesMessage().ToJSON());
        }

        public WebMessage CreateResourcesMessage()
        {
            return CreateResourcesMessage(false, null);
        }

        internal WebMessage CreateResourcesMessage(bool asArray, Guid[] objectsToInclude)
        {
            string[] resources;

            if (null != objectsToInclude)
            {
                lock (m_Resources)
                {
                    resources = m_Resources.Select((r) => string.Format(FormatResource, r.Key, r.Value.ToJSON())).ToArray();
                }
            }
            else
            {
                lock (m_Resources)
                {
                    resources = m_Resources.Where((r) => objectsToInclude.Contains(r.Key)).Select((r) => string.Format(FormatResource, r.Key, r.Value.ToJSON())).ToArray();
                }
            }

            string payload;

            if (asArray) payload = string.Concat('[', string.Join(",", resources), ']');
            else payload = string.Join(",", resources);

            return new WebMessage("webShareResouces", payload);
        }

        public WebMessage CreateAccessControlMessage()
        {

            string[] acls;

            lock (m_Resources)
            {
                acls = m_AccessControlDictionary.Select((r) => string.Format(FormatResource, r.Key, r.Value.ToString())).ToArray();
            }

            string payload = string.Join(",", acls);

            payload = string.Format(FormatACL, m_ShareId.ToString(), payload);

            return new WebMessage("webShareResouces", payload);

        }

        public WebMessage CreateInfoMessage()
        {
            return new WebMessage("webShareInfo", string.Format(FormatInfo,
                m_ShareId.ToString(),
                m_Owner.PublicKey.ToString(),
                m_ShareName,
                m_Public.ToString().ToLowerInvariant(),
                string.Join(",", m_Invited.Select((i) => i.PublicKey.ToString()).ToArray()),
                string.Join(",", m_Present.Select((p) => p.PublicKey.ToString()).ToArray())));
        }

        #endregion
    }
}
