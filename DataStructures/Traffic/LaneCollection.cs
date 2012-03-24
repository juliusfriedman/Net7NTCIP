using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.Traffic
{
    public class LaneCollection : ICollection<Lane>
    {
        #region Fields

        const bool ReadOnly = false;

        Dictionary<Guid, Lane> lanes = new Dictionary<Guid, Lane>();

        #endregion

        #region Properties

        public List<Lane> Lanes
        {
            get
            {
                List<Lane> localCopy = null;
                lock (lanes) localCopy = lanes.Values.ToList();
                return localCopy;
            }
        }

        public List<ushort> LaneGroups
        {
            get
            {
                List<ushort> result = new List<ushort>();
                Lanes.ForEach(lane => { if (!result.Contains(lane.LaneGroup)) result.Add(lane.LaneGroup); });
                return result;
            }
        }

        public List<char> LegacyLaneGroups
        {
            get
            {
                List<char> result = new List<char>();
                Lanes.ForEach(lane => { if (!result.Contains(lane.LegacyLaneGroup)) result.Add(lane.LegacyLaneGroup); });                
                return result;
            }
        }        

        #endregion

        #region Methods

        #region Speed

        public double TotalSpeed(ushort laneGroupMask)
        {
            return TotalSpeed(laneGroupMask, LaneStatus.OK);
        }

        public double TotalSpeed(char laneGroup)
        {
            return TotalSpeed(laneGroup, LaneStatus.OK);
        }

        public double TotalSpeed(ushort laneGroupMask, LaneStatus status)
        {
            double total = 0.0F;
            GetLanesByLaneMask(laneGroupMask, status).ForEach(lane => { total += lane.Speed; });
            return total;
        }

        public double TotalSpeed(char laneGroup, LaneStatus status)
        {
            double total = 0.0F;
            GetLanesByLegacyLaneGroup(laneGroup, status).ForEach(lane => { total += lane.Speed; });
            return total;
        }

        public double AverageSpeed(ushort laneGroupMask)
        {
            return AverageSpeed(laneGroupMask, LaneStatus.OK);
        }

        public double AverageSpeed(char laneGroup)
        {
            return AverageSpeed(laneGroup, LaneStatus.OK);
        }

        public double AverageSpeed(ushort laneGroupMask, LaneStatus status)
        {
            double average = 0.0F;
            int count = 0;
            GetLanesByLaneMask(laneGroupMask, status).ForEach(lane => { average += lane.Speed; ++count; });
            return average / count;
        }

        public double AverageSpeed(char laneGroup, LaneStatus status)
        {
            double average = 0.0F;
            int count = 0;
            GetLanesByLegacyLaneGroup(laneGroup, status).ForEach(lane => { average += lane.Speed; ++count; });
            return average / count;
        }

        public double HighestSpeed(ushort laneGroupMask)
        {
            return HighestSpeed(laneGroupMask, LaneStatus.OK);
        }

        public double HighestSpeed(char laneGroup)
        {
            return HighestSpeed(laneGroup, LaneStatus.OK);
        }

        public double HighestSpeed(ushort laneGroupMask, LaneStatus status)
        {
            double highest = 0.0F;
            GetLanesByLaneMask(laneGroupMask, status).ForEach(lane => { if (lane.Density > highest) highest = lane.Speed; });
            return highest;
        }

        public double HighestSpeed(char laneGroup, LaneStatus status)
        {
            double highest = 0.0F;
            GetLanesByLegacyLaneGroup(laneGroup, status).ForEach(lane => { if (lane.Density > highest) highest = lane.Speed; });
            return highest;
        }

        public double LowestSpeed(ushort laneGroupMask)
        {
            return LowestSpeed(laneGroupMask, LaneStatus.OK);
        }

        public double LowestSpeed(char laneGroup)
        {
            return LowestSpeed(laneGroup, LaneStatus.OK);
        }

        public double LowestSpeed(ushort laneGroupMask, LaneStatus status)
        {
            double lowest = 0.0F;
            GetLanesByLaneMask(laneGroupMask, status).ForEach(lane => { if (lane.Density < lowest) lowest = lane.Speed; });
            return lowest;
        }

        public double LowestSpeed(char laneGroup, LaneStatus status)
        {
            double lowest = 0.0F;
            GetLanesByLegacyLaneGroup(laneGroup, status).ForEach(lane => { if (lane.Density < lowest) lowest = lane.Speed; });
            return lowest;
        }

        #endregion

        #region Volume

        public double TotalVolume(ushort laneGroupMask)
        {
            return TotalVolume(laneGroupMask, LaneStatus.OK);
        }

        public double TotalVolume(char laneGroup)
        {
            return TotalVolume(laneGroup, LaneStatus.OK);
        }

        public double AverageVolume(ushort laneGroupMask)
        {
            return AverageVolume(laneGroupMask, LaneStatus.OK);
        }

        public double AverageVolume(char laneGroup)
        {
            return AverageVolume(laneGroup, LaneStatus.OK);
        }

        public double HighestVolume(ushort laneGroupMask)
        {
            return HighestVolume(laneGroupMask, LaneStatus.OK);
        }

        public double HighestVolume(char laneGroup)
        {
            return HighestVolume(laneGroup, LaneStatus.OK);
        }

        public double LowestVolume(ushort laneGroupMask)
        {
            return LowestVolume(laneGroupMask, LaneStatus.OK);
        }

        public double LowestVolume(char laneGroup)
        {
            return LowestVolume(laneGroup, LaneStatus.OK);
        }

        public double TotalVolume(ushort laneGroupMask, LaneStatus status)
        {
            double total = 0.0F;
            GetLanesByLaneMask(laneGroupMask, status).ForEach(lane => { total += lane.Volume; });
            return total;
        }

        public double TotalVolume(char laneGroup, LaneStatus status)
        {
            double total = 0.0F;
            GetLanesByLegacyLaneGroup(laneGroup, status).ForEach(lane => { total += lane.Volume; });
            return total;
        }

        public double AverageVolume(ushort laneGroupMask, LaneStatus status)
        {
            double average = 0.0F;
            int count = 0;
            GetLanesByLaneMask(laneGroupMask, status).ForEach(lane => { average += lane.Volume; ++count; });
            return average / count;
        }

        public double AverageVolume(char laneGroup, LaneStatus status)
        {
            double average = 0.0F;
            int count = 0;
            GetLanesByLegacyLaneGroup(laneGroup, status).ForEach(lane => { average += lane.Volume; ++count; });
            return average / count;
        }

        public double HighestVolume(ushort laneGroupMask, LaneStatus status)
        {
            double highest = 0.0F;
            GetLanesByLaneMask(laneGroupMask, status).ForEach(lane => { if (lane.Density > highest) highest = lane.Volume; });
            return highest;
        }

        public double HighestVolume(char laneGroup, LaneStatus status)
        {
            double highest = 0.0F;
            GetLanesByLegacyLaneGroup(laneGroup, status).ForEach(lane => { if (lane.Density > highest) highest = lane.Volume; });
            return highest;
        }

        public double LowestVolume(ushort laneGroupMask, LaneStatus status)
        {
            double lowest = 0.0F;
            GetLanesByLaneMask(laneGroupMask, status).ForEach(lane => { if (lane.Density < lowest) lowest = lane.Volume; });
            return lowest;
        }

        public double LowestVolume(char laneGroup, LaneStatus status)
        {
            double lowest = 0.0F;
            GetLanesByLegacyLaneGroup(laneGroup, status).ForEach(lane => { if (lane.Density < lowest) lowest = lane.Volume; });
            return lowest;
        }

        #endregion

        #region Density

        public double TotalDensity(ushort laneGroupMask)
        {
            return TotalDensity(laneGroupMask, LaneStatus.OK);
        }

        public double TotalDensity(char laneGroup)
        {
            return TotalDensity(laneGroup, LaneStatus.OK);
        }

        public double AverageDensity(ushort laneGroupMask)
        {
            return AverageDensity(laneGroupMask, LaneStatus.OK);
        }

        public double AverageDensity(char laneGroup)
        {
            return AverageDensity(laneGroup, LaneStatus.OK);
        }

        public double HighestDensity(ushort laneGroupMask)
        {
            return HighestDensity(laneGroupMask, LaneStatus.OK);
        }

        public double HighestDensity(char laneGroup)
        {
            return HighestDensity(laneGroup, LaneStatus.OK);
        }

        public double LowestDensity(ushort laneGroupMask)
        {
            return LowestDensity(laneGroupMask, LaneStatus.OK);
        }

        public double LowestDensity(char laneGroup)
        {
            return LowestDensity(laneGroup, LaneStatus.OK);
        }

        public double TotalDensity(ushort laneGroupMask, LaneStatus status)
        {
            double total = 0.0F;
            GetLanesByLaneMask(laneGroupMask, status).ForEach(lane => { total += lane.Density; });
            return total;
        }

        public double TotalDensity(char laneGroup, LaneStatus status)
        {
            double total = 0.0F;
            GetLanesByLegacyLaneGroup(laneGroup, status).ForEach(lane => { total += lane.Density; });
            return total;
        }

        public double AverageDensity(ushort laneGroupMask, LaneStatus status)
        {
            double average = 0.0F;
            int count = 0;
            GetLanesByLaneMask(laneGroupMask, status).ForEach(lane => { average += lane.Density; ++count; });
            return average / count;
        }

        public double AverageDensity(char laneGroup, LaneStatus status)
        {
            double average = 0.0F;
            int count = 0;
            GetLanesByLegacyLaneGroup(laneGroup, status).ForEach(lane => { average += lane.Density; ++count; });
            return average / count;
        }

        public double HighestDensity(ushort laneGroupMask, LaneStatus status)
        {
            double highest = 0.0F;
            GetLanesByLaneMask(laneGroupMask, status).ForEach(lane => { if (lane.Density > highest) highest = lane.Density; });
            return highest;
        }

        public double HighestDensity(char laneGroup, LaneStatus status)
        {
            double highest = 0.0F;
            GetLanesByLegacyLaneGroup(laneGroup, status).ForEach(lane => { if (lane.Density > highest) highest = lane.Density; });
            return highest;
        }

        public double LowestDensity(ushort laneGroupMask, LaneStatus status)
        {
            double lowest = 0.0F;
            GetLanesByLaneMask(laneGroupMask, status).ForEach(lane => { if (lane.Density < lowest) lowest = lane.Density; });
            return lowest;
        }

        public double LowestDensity(char laneGroup, LaneStatus status)
        {
            double lowest = 0.0F;
            GetLanesByLegacyLaneGroup(laneGroup, status).ForEach(lane => { if (lane.Density < lowest) lowest = lane.Density; });
            return lowest;
        }

        #endregion

        #region DataBin Methods

        public double TotalDataBin(char legacyLaneGroup, DataBinType binType)
        {
            return TotalDataBin(legacyLaneGroup, LaneStatus.OK, binType);
        }

        public double TotalDataBin(char legacyLaneGroup, LaneStatus status, DataBinType binType)
        {
            double total = 0;
            GetLanesByLegacyLaneGroup(legacyLaneGroup, status).ForEach(lane =>
            {
                foreach(DataBin db in lane.DataBins.Values.Where(db => db.BinType.Equals(binType)))
                {
                    total += db.BinValue;
                };
            });
            return total;
        }

        public double AverageDataBin(char legacyLaneGroup, DataBinType binType)
        {
            return AverageDataBin(legacyLaneGroup, LaneStatus.OK, binType);
        }

        public double AverageDataBin(char legacyLaneGroup, LaneStatus status, DataBinType binType)
        {
            double total = 0;
            double count = 0;
            GetLanesByLegacyLaneGroup(legacyLaneGroup, status).ForEach(lane =>
            {
                foreach (DataBin db in lane.DataBins.Values.Where(db => db.BinType.Equals(binType)))
                {
                    total += db.BinValue;                    
                };
                ++count;
            });
            if (total == 0 || count == 0) return total;
            return total / count;
        }

        #endregion

        /// <summary>
        /// The Count of all Lane Objects in the collection
        /// </summary>
        /// <param name="legacyLaneGroup"></param>
        /// <returns>The count of lanes which match the given legacyLaneGroup</returns>
        public int LaneCount(char legacyLaneGroup, LaneStatus status)
        {
            return GetLanesByLegacyLaneGroup(legacyLaneGroup, status).Count;
        }

        /// <summary>
        /// The Count of all Lane Objects in the collection
        /// </summary>
        /// <param name="laneMask"></param>
        /// <returns>The count of lanes which match the given laneMask</returns>
        public int LaneCount(ushort laneMask, LaneStatus status)
        {
            return GetLanesByLaneMask(laneMask, status).Count;
        }

        /// <summary>
        /// The Count of all Lane Objects in the collection
        /// </summary>
        /// <param name="legacyLaneGroup"></param>
        /// <returns>The count of lanes which match the given legacyLaneGroup</returns>
        public int LaneCount(char legacyLaneGroup)
        {
            return LaneCount(legacyLaneGroup, LaneStatus.OK);
        }

        /// <summary>
        /// The Count of all Lane Objects in the collection
        /// </summary>
        /// <param name="laneMask"></param>
        /// <returns>The count of lanes which match the given laneMask</returns>
        public int LaneCount(ushort laneMask)
        {
            return GetLanesByLaneMask(laneMask, LaneStatus.OK).Count;
        }

        /// <summary>
        /// Removes a Lane from this LaneCollection
        /// </summary>
        /// <param name="itemId">The LaneId to Remove</param>
        /// <returns>True if the lane is removed, else false</returns>
        public bool Remove(Guid itemId)
        {
            lock (lanes)
            {
                return lanes.Remove(itemId);
            }
        }

        public bool Contains(Guid itemId)
        {
            return lanes.ContainsKey(itemId);
        }

        public IEnumerator<Lane> GetEnumerator(char legacyLaneGroup, LaneStatus status)
        {
            return GetLanesByLegacyLaneGroup(legacyLaneGroup, status).GetEnumerator();
        }

        public IEnumerator<Lane> GetEnumerator(ushort laneGroup, LaneStatus status)
        {
            return GetLanesByLaneMask(laneGroup, status).GetEnumerator();
        }

        public IEnumerator<Lane> GetEnumerator(char legacyLaneGroup)
        {
            return GetEnumerator(legacyLaneGroup, LaneStatus.OK);
        }

        public IEnumerator<Lane> GetEnumerator(ushort laneGroup)
        {
            return GetEnumerator(laneGroup, LaneStatus.OK);
        }

        public List<Lane> GetLanesByLaneMask(ushort laneMask)
        {
            return Lanes.Where(lane => (lane.LaneGroup & laneMask) > 0).ToList();
        }

        public List<Lane> GetLanesByLaneMask(ushort laneMask, LaneStatus laneStatus)
        {
            return Lanes.Where(lane => lane.LaneStatus.Equals(laneStatus) && (lane.LaneGroup & laneMask) > 0).ToList();
        }

        public List<Lane> GetLanesByLegacyLaneGroup(char legacyLaneGroup)
        {
            return Lanes.Where(lane => (lane.LegacyLaneGroup.Equals(legacyLaneGroup))).ToList();
        }

        public List<Lane> GetLanesByLegacyLaneGroup(char legacyLaneGroup, LaneStatus laneStatus)
        {
            return Lanes.Where(lane => lane.LaneStatus.Equals(laneStatus) && (lane.LegacyLaneGroup.Equals(legacyLaneGroup))).ToList();
        }

        #endregion

        #region Operators

        #endregion

        #region ICollection Implementation

        public void Add(Lane item)
        {
            lock (lanes)
            {
                lanes.Add(item.Id, item);
            }
        }

        public void Clear()
        {
            lock (lanes)
            {
                lanes.Clear();
            }
        }

        public bool Contains(Lane item)
        {
            return lanes.ContainsValue(item);
        }

        public void CopyTo(Lane[] array, int arrayIndex)
        {
            Lanes.CopyTo(array, arrayIndex);            
        }

        /// <summary>
        /// The count of all Lane Objects in the LaneCollection
        /// </summary>
        public int Count
        {
            get { return lanes.Count; }
        }

        public bool IsReadOnly
        {
            get { return ReadOnly; }
        }

        /// <summary>
        /// Removes a Lane from this LaneCollection
        /// </summary>
        /// <param name="item">The Lane to Remove</param>
        /// <returns>True if the lane is removed, else false</returns>
        public bool Remove(Lane item)
        {
            return Remove(item.Id);
        }

        public IEnumerator<Lane> GetEnumerator()
        {
            return lanes.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return lanes.GetEnumerator();
        }

        #endregion
    }
}
