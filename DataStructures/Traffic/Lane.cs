using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.Traffic
{
    public class Lane
    {
        #region Fields

        const string IdProperty = "Id";
        const string SpeedProperty = "Speed";
        const string VolumeProperty = "Volume";
        const string DensityProperty = "Density";
        const string BinsProperty = "Bins";
        const string PresenceProperty = "Presense";
        const string LaneNumberProperty = "Presense";
        const string LaneGroupProperty = "Group";
        const string LaneStatusProperty = "Status";//could be masked into group 
        const string DescriptionProperty = "Description";

        Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                {IdProperty, Guid.NewGuid()},
                {DescriptionProperty, string.Empty},
                {LaneGroupProperty, 0x0},
                {SpeedProperty, 0.0F},
                {VolumeProperty, 0.0F},
                {DensityProperty, 0.0F},
                {PresenceProperty, false},
                {LaneStatusProperty, LaneStatus.Unknown},
                {BinsProperty, new Dictionary<Guid, DataBin>()}
            };
        
        #endregion

        #region Properties

        public Guid Id
        {
            get
            {
                return (Guid)properties[IdProperty];
            }
            set
            {
                lock (properties)
                {
                    properties[IdProperty] = value;
                }
            }
        }

        public int LaneNumber
        {
            get
            {
                return (int)properties[LaneNumberProperty];
            }
            set
            {
                lock (properties)
                {
                    properties[LaneNumberProperty] = value;
                }
            }
        }

        public LaneStatus LaneStatus
        {
            get
            {
                return (LaneStatus)properties[LaneStatusProperty];
            }
            set
            {
                lock (properties)
                {
                    properties[LaneStatusProperty] = value;
                }
            }
        }

        public double Speed
        {
            get
            {
                return (double)properties[SpeedProperty];
            }
            set
            {
                lock (properties)
                {
                    properties[SpeedProperty] = value;
                }
            }
        }

        public double Volume
        {
            get
            {
                return (double)properties[VolumeProperty];
            }
            set
            {
                lock (properties)
                {
                    properties[VolumeProperty] = value;
                }
            }
        }

        public double Density
        {
            get
            {
                return (double)properties[DensityProperty];
            }
            set
            {
                lock (properties)
                {
                    properties[DensityProperty] = value;
                }
            }
        }

        public bool Presense
        {
            get
            {
                return (bool)properties[PresenceProperty];
            }
            set
            {
                lock (properties)
                {
                    properties[PresenceProperty] = value;
                }
            }
        }

        public string Description
        {
            get { return this[DescriptionProperty].ToString(); }
            set { this[DescriptionProperty] = value; }
        }

        public ushort LaneGroup
        {
            get { return (ushort)properties[LaneGroupProperty]; }
            private set
            {
                lock (properties)
                {
                    properties[LaneGroupProperty] = value;
                }
            }
        }

        public char LegacyLaneGroup
        {
            get
            {
                return ((char)(LaneGroup << 8));
            }
        }

        public Dictionary<Guid, DataBin> DataBins
        {
            get
            {
                return (Dictionary<Guid, DataBin>)properties[BinsProperty];
            }
            private set
            {
                lock (properties)
                {
                    properties[BinsProperty] = value;
                }
            }
        }

        public Object this[string key]
        {
            get
            {
                object value = null;
                if (!string.IsNullOrEmpty(key)) properties.TryGetValue(key, out value);
                return value;
            }
            private set
            {
                if (string.IsNullOrEmpty(key)) return;
                lock (properties)
                {
                    if (properties.ContainsKey(key)) properties[key] = value;
                    else properties.Add(key, value);
                }
            }
        }

        #endregion

        #region Methods

        void AddDataBin(DataBin bin)
        {
            DataBins.Add(bin.Id, bin);
        }

        bool RemoveDataBin(DataBin bin)
        {
            return DataBins.Remove(bin.Id);
        }

        bool RemoveDataBin(Guid binId)
        {
            return DataBins.Remove(binId);
        }

        #endregion
    }
}
