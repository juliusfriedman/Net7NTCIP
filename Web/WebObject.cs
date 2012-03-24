namespace ChipsWeb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Collections.ObjectModel;

    public class WebObject
    {
        #region Constant

        internal const string FormatJSON =
@"{{
    'ShareId''{0}',
    'Value':{1}
}}";

        #endregion

        #region Fields

        internal WebShare m_Share;
        internal Guid m_ResourceId;
        internal string m_ResourceName;
        internal Object m_Value = null;

        #endregion

        #region Properties

        public WebShare Share
        {
            get { return m_Share; }
        }

        public Guid ResourceId
        {
            get { return m_ResourceId; }
        }

        public string ResourceName
        {
            get { return m_ResourceName; }
            set
            {
                if (string.IsNullOrEmpty(value)) return; 
                m_ResourceName = value; 
                m_Share.Refresh();
            }
        }

        public Object Value
        {
            get { return m_Value; }
        }
       
        #endregion

        #region Constructor

        public WebObject(WebShare share, string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName)) throw new ArgumentNullException("resourceName");
            m_Share = share;
            m_ResourceName = resourceName;
            m_ResourceId = new Guid();
        }

        #endregion

        #region Methods

        public void Update(Object value)
        {            
            lock (this)
            {
                if (value == m_Value) return;
                m_Value = value;
            }

            m_Share.Refresh();
        }

        public virtual string ToJSON()
        {
            return string.Format(FormatJSON,
               m_Share.ShareId.ToString(),
                m_Value.ToString());
        }

        

        #endregion
    }
}
