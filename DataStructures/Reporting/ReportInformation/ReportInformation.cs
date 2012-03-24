using System;
using System.Collections;

namespace DataStructures.Reporting
{
    /// <summary>
    /// ReportInformation defines the entities and dates included in a report.
    /// </summary>
    [Serializable]
    public class ReportInformation
    {
        #region Fields
        DateTime startDate, endDate;

        TimeSpan totalDuration;

        ArrayList reportEntities;
        #endregion

        #region Constructor
        public ReportInformation(DateTime startDate, DateTime endDate)
        {

            if (endDate < startDate) throw new ArgumentException("Cannot be greater then startDate", "endDate");

            this.startDate = startDate;
            this.endDate = endDate;            

            totalDuration = endDate - startDate;

            reportEntities = new ArrayList();
        }
        #endregion

        #region Properties

        /// <summary>
        /// The earliest Date the information contains
        /// </summary>
        public DateTime StartDate
        {
            get
            {
                return startDate;
            }
        }

        /// <summary>
        /// The lasest Date the information contains
        /// </summary>
        public DateTime EndDate
        {
            get
            {
                return endDate;
            }
        }

        /// <summary>
        /// The total duration of the information
        /// </summary>
        public TimeSpan TotalDuration
        {
            get
            {                
                return totalDuration;
            }
        }

        /// <summary>
        /// The entities included in the information
        /// </summary>
        public Object[] ReportEntities
        {
            get
            {
                lock (reportEntities.SyncRoot)
                {
                    return reportEntities.ToArray();
                }
            }
        }

        #endregion

        #region Indexers

        /// <summary>
        /// Retrieves a entity from the information
        /// </summary>
        /// <param name="entity">The entity to retrieve</param>
        /// <returns>The entity if contained, otherwise null</returns>
        public Object this[Object entity]
        {
            get
            {
                lock (reportEntities.SyncRoot)
                {
                    int index = reportEntities.IndexOf(entity);
                    if (index == -1) return null;
                    return reportEntities[index];
                }
            }
        }

        /// <summary>
        /// Retrieves a entity from the information
        /// </summary>
        /// <param name="entityIndex">The known index of the entity</param>
        /// <returns>The entity at the given index</returns>
        public Object this[int entityIndex]
        {
            get
            {                
                lock (reportEntities.SyncRoot)
                {
                    if (entityIndex < 0 || entityIndex >= reportEntities.Count) throw new ArgumentOutOfRangeException("entityIndex");
                    return reportEntities[entityIndex];
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a entity to the information if not already added
        /// </summary>
        /// <param name="entity">The entity to add</param>
        public void Add(Object entity)
        {
            lock (reportEntities.SyncRoot)
            {
                if (!reportEntities.Contains(entity)) reportEntities.Add(entity);                
            }
        }

        /// <summary>
        /// Indicates weather or not the information contains a given entity
        /// </summary>
        /// <param name="entity">The entity to check for</param>
        /// <returns>True if the entity is contained, otherwise false</returns>
        public bool Contains(Object entity)
        {
            lock (reportEntities.SyncRoot)
            {
                return reportEntities.Contains(entity);
            }
        }

        /// <summary>
        /// Indicates weather or not the information contains the given date
        /// </summary>
        /// <param name="date">The date to check for</param>
        /// <returns>True if the date is contains, otherwise false</returns>
        public bool Contains(DateTime date)
        {
            return date <= endDate && date >= startDate;
        }

        /// <summary>
        /// Indicates weather or not the information contains the given month and year
        /// </summary>
        /// <param name="month">The month to check for</param>
        /// <param name="year">The year to check for</param>
        /// <returns>True if the month and year are contained, otherwise false</returns>
        public bool Contains(int month, int year)
        {
            return (month <= EndDate.Month && month >= StartDate.Month && year <= EndDate.Year && year >= StartDate.Year);                
        }

        /// <summary>
        /// Removes a given entity from the information
        /// </summary>
        /// <param name="entity">The entity to remove</param>
        public void Remove(Object entity)
        {
            lock (reportEntities.SyncRoot)
            {
                reportEntities.Remove(entity);
            }
        }

        #endregion
    }
}
