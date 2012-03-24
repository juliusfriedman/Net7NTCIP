namespace ChipsWeb
{
    using System;
    public class WebMessage
    {
        #region Constants

#if DEBUG
        const string formatJSON =
@"{{
    'UTCTimestamp':{0},
    'Command':'{1}',
    'Payload':{2},
    'AcknowledgeRequired':{3},
    'MessageId':'{4}'
}}";

#else
        const string formatJSON = "{{'UTCTimestamp':{0},'Command':'{1}','Payload':{2},'AcknowledgeRequired':{3},'MessageId':'{4}'}}";
#endif

        #endregion

        #region Fields

        DateTime m_UTCTimestamp = DateTime.UtcNow;
        string m_Command;
        string m_Payload;
        Guid m_MessageId = Guid.NewGuid();
        bool m_AcknowledgeRequired;

        #endregion

        #region Properties

        public Guid MessageId
        {
            get { return MessageId; }
        }

        public DateTime UTCTimestamp
        {
            get { return m_UTCTimestamp; }
            set { m_UTCTimestamp = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime(); }
        }

        public string Command
        {
            get { return m_Command; }
            set { m_Command = value; }
        }

        public string Payload
        {
            get { return m_Payload; }
            set { m_Payload = value; }
        }

        public bool AcknowledgeRequired
        {
            get { return m_AcknowledgeRequired; }
            set { m_AcknowledgeRequired = value; }
        }

        #endregion

        #region Constructor

        internal WebMessage() { }

        public WebMessage(string command, string payload)
        {
            Command = command;
            Payload = payload;
        }

        #endregion

        #region Methods

        public string ToJSON()
        {
            return string.Format(formatJSON, m_UTCTimestamp.ToFileTimeUtc(), m_Command, m_Payload, m_AcknowledgeRequired.ToString().ToLowerInvariant(), MessageId.ToString());
        }

        //MAYBE These methods will eventually use a JavaScript Serializer to decode and get keys and values from the JSONObject encoded in the BodyData if any.

        public Object GetValue(string key)
        {
            return null;
        }

        public T GetValue<T>(string key)
        {
            return default(T);
        }

        public bool TryGetValue(string key, out Object value)
        {
            value = null;
            return false;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            value = default(T);
            return false;
        }


        #endregion
    }
}