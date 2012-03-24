namespace ChipsWeb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Drawing;
    using System.Collections.ObjectModel;

    public class WebMouse : WebObject
    {

        #region Nested Types

        [Flags]
        public enum WebMouseButtons
        {
            None = 0,
            Left = 1,
            Right = 2,
            Middle = 4,
        }

        internal sealed class WebMouseEvent
        {
            #region Constants

            internal const string FormatJSONEvent =
    @"{{    
    //'X':{0},
    //'Y':{1},
    //'Z':{2},  
    'MouseButtons':{3},  
    'ScrollDelta':{4},
    'PulseMS':{12},
    'Styles': {
        'top':{1},
        'left':{0},
        'zIndex':{2},
        'backgroundImage':'{5}',
        'backgroundColor':'{6}',
        'backgroundRepeat':'{7}',
        'backgroundPosition':'{8}',
        'backgroundAttachment':'{9}',
        'opacity':{10},
        'border':'{11}'
    }
}}";
            #endregion

            internal int X, Y, Z;
            internal WebMouseButtons MouseButtons;
            internal double Opacity, ScrollDelta;
            internal bool CSSChangedEvent;
            internal bool JITEvent;
            internal Guid FromUserKey;
            internal DateTime RecievedOn = DateTime.Now;            
            internal uint PulseMS;
            internal string BackgroundImage, BackgroundColor, BackgroundRepeat, BackgroundPosition, BackgroundAttachment, BorderCssString, PulseColor;         

            public string ToJSON()
            {
                return string.Format(FormatJSONEvent,
                X.ToString(),
                Y.ToString(),
                Z.ToString(),
                MouseButtons.ToString(),
                ScrollDelta.ToString(),
                BackgroundImage, BackgroundColor, BackgroundRepeat, BackgroundPosition, BackgroundAttachment,
                Opacity.ToString(),
                BorderCssString,
                PulseMS);
            }
        }

        #endregion

        #region Fields

        internal Image m_CursorImage;
        internal bool m_SendClicks, m_SendRightClick, m_SendScrollWheel, m_AllowExternalControl, m_AllowExternalClicks, m_AllowExternalRightClick, m_AllowExternalScrollDelta, m_InJIT;
        internal DateTime m_LastSignalTime, m_LastJitTime, m_LastSendTime;
        internal TimeSpan m_UpdateThreshold = TimeSpan.FromSeconds(2.9);
        internal volatile List<WebMessage> m_Updates = new List<WebMessage>();
        internal volatile List<WebMouseEvent> m_Events = new List<WebMouseEvent>();
        internal volatile WebMouseEvent m_CurrentEvent;

        #endregion

        #region Properties

        public Image CursorImage
        {
            get { return m_CursorImage; }
            protected set
            {
                m_CursorImage = value; 
                Signal();
            }
        }

        internal WebMouseEvent CurrentEvent
        {
            get { return m_CurrentEvent; }
        }

        public bool UpdateEligible
        {
            get { return !m_InJIT && DateTime.Now - m_LastSignalTime >= m_UpdateThreshold && m_Updates.Count > 0; }
        }

        public bool AllowExternalClicks
        {
            get { return m_AllowExternalClicks; }
        }

        public bool AllowExternalRightClick
        {
            get { return m_AllowExternalRightClick; }
        }

        public bool AllowExternalScroll
        {
            get { return m_AllowExternalScrollDelta; }
        }

        internal ReadOnlyCollection<WebMouseEvent> Events
        {
            get { return m_Events.AsReadOnly(); }
        }

        #endregion

        #region Constructor

        public WebMouse(WebShare share) : base(share, share.Owner.PublicKey + " Cursor") { }

        public WebMouse(WebShare share, string cursorName) : base(share, cursorName) { }

        #endregion

        #region Methods

        internal void HandleEvent(WebClient sender, WebMouseEvent e)
        {
            //Synchronize
            lock (m_Events)
            {                
                m_Events.Add(e);
            }

            //If we need to update
            if (m_Updates.Count <= 12 && (m_SendClicks && e.MouseButtons != 0 && m_SendRightClick && e.MouseButtons.HasFlag(WebMouseButtons.Right)) || (m_SendScrollWheel && e.ScrollDelta != 0)) Signal(/*true*/);
            else if (m_Updates.Count > 13) JITEvents();            
        }

        internal void JITEvents() { JITEvents(false); }

        /// <summary>
        /// Mice move a lot, this algorithm helps to eliminate events in the history which make unnecessary movement or that save bandwidth
        /// </summary>
        internal void JITEvents(bool force)        
        {
            //Do not JIT Too Soon or when not needed
            if (m_InJIT && !force && m_Events.Count <= 12 || (DateTime.Now - m_LastJitTime) <= m_UpdateThreshold - m_Share.Owner.Server.Wake) return;

            WebMouseEvent currentEvent = m_Events.FirstOrDefault();

            if (currentEvent == null) return;

            m_InJIT = true;
            m_LastJitTime = DateTime.Now;

            var events = Events.OrderBy((e) => e.RecievedOn);
            Clear();

            int X = 0, Y = 0, Z = 0;
            
            var temp = default(IOrderedEnumerable<WebMouseEvent>);

            List<WebMouseEvent> newEvents = new List<WebMouseEvent>();

            //Compress based on pulse
            if (currentEvent.PulseMS > 0)
            {
                //Take the MouseEvents which were within the pulseTime
                var compress = events.TakeWhile((e) => (e.RecievedOn - currentEvent.RecievedOn).TotalMilliseconds <= currentEvent.PulseMS);

                if (compress.Count() > 0)
                {
                    //make compressed frames consisting of all the events which can be compressed
                    List<WebMouseEvent> compressed = new List<WebMouseEvent>();
                    //compress them
                    compress.Where((c)=>!c.JITEvent).ToList().ForEach((c) =>
                    {
                        //Ensure we get all clicks and deltas
                        if (c.MouseButtons != 0 || c.ScrollDelta != 0)
                        {
                            compressed.Add(c);
                            c.JITEvent = true;
                            return;
                        }

                        List<WebMouseEvent> movements = new List<WebMouseEvent>();

                        //combined x,y,z movements within a few of the last
                        if ((X - c.X) + (Y - c.Y) > 5 || c.Z != 0 || compressed.Count == 0)
                        {
                            X = c.X;
                            Y = c.Y;
                            Z = c.Z;
                            c.JITEvent = true;
                            movements.Add(c);
                            return;
                        }

                        compressed.Zip(movements, (a, b) =>
                        {
                            return a;
                        });

                    });
                }
            }

            //Other compression
            bool significantCompression = false;

            //Compress the remaining events with the main Events            
            temp = Events.OrderBy((e) => e.RecievedOn);
            Clear();
            events.Zip(temp, (a, b) =>
            {
                if (!b.JITEvent) return b = a;
                else return a = b;
                
            });

            //Add the compressed events back into the stack
            lock (m_Events)
            {
                m_Events.AddRange(events);
            }

            //If we made any real progress time to Signal
            if (significantCompression) Signal();

            m_InJIT = false;

        }

        internal void Signal()
        {
            Signal(false); 
            JITEvents();
        }

        internal void Signal(bool force)
        {
            //Do not signal within wake if not forced
            if (!force && DateTime.Now - m_LastSignalTime <= m_Share.Owner.Server.Wake) return;
            m_LastSignalTime = DateTime.Now;
            m_Share.Refresh();
        }

        internal void Clear()
        {
            lock (m_Events)
            {
                m_Events.Clear();
            }
        }

        //not sure yet
        public string Purge()
        {
            return string.Empty;
        }

        public override string ToJSON()
        {
            m_Value = m_CurrentEvent.ToJSON();
            return base.ToJSON();
        }

        #endregion

    }
}
