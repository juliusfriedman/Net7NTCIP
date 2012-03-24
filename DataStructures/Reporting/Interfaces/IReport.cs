using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataStructures.Reporting
{
    /// <summary>
    /// IReport defines a methods and properties which are related to all types of reports.
    /// </summary>
    /// <typeparam name="T">The type of elements in the report.</typeparam>
    public interface IReport<T>
    {
        /// <summary>
        /// Totals all elements for all entities
        /// </summary>
        /// <returns>The calculated total</returns>
        T Total();
        /// <summary>
        /// Total all elements for the given entity
        /// </summary>
        /// <param name="entity">The entity to obtain a total for</param>
        /// <returns>The calculated total for the  given entity</returns>
        T Total(Object entity);
        /// <summary>
        /// Total all elements for the given entities
        /// </summary>
        /// <param name="entities">The entities to obtain a total for</param>
        /// <returns>The calculated total for the  given entities</returns>
        T Total(Object[] entities);
        /// <summary>
        /// Total all elements for the given entities and date range
        /// </summary>
        /// <param name="entities">The entities to obtain a total for</param>
        /// <param name="start">The earliest time to factor</param>
        /// <param name="end">The latest time to factor</param>
        /// <returns>The calculated total for the given entities and date range</returns>
        T Total(Object[] entities, DateTime start, DateTime end);
        /// <summary>
        /// Averages all elements for all entities
        /// </summary>
        /// <returns>The calculated average</returns>
        T Average();
        /// <summary>
        /// Averages all elements for the given entity
        /// </summary>
        /// <param name="entity">The entity to average for</param>
        /// <returns>The calculated average for the given entity</returns>
        T Average(Object entity);
        /// <summary>
        /// Averages all elements for the given entities
        /// </summary>
        /// <param name="entities">The entities to obtain a average for</param>
        /// <returns>The calulcated average for the given entities</returns>
        T Average(Object[] entities);
        /// <summary>
        /// Average all elements for the given entities and date range
        /// </summary>
        /// <param name="entities">The entities to obtain a average for</param>
        /// <param name="start">The earliest time to factor</param>
        /// <param name="end">The latest time to factor</param>
        /// <returns>The calculated average for the given entities and date range</returns>
        T Average(Object[] entities, DateTime start, DateTime end);
        /// <summary>
        /// The TimeSpan which represents the TotalDuration of the Report
        /// </summary>
        TimeSpan TotalDuration { get; }
        /// <summary>
        /// The Entities contained in the report
        /// </summary>
        Object[] ReportEntities { get; }       
        /// <summary>
        /// Provides the ReportInformation of the Report
        /// </summary>
        ReportInformation ReportInformation { get; }        
        /// <summary>
        /// The type of elements int the Report
        /// </summary>
        Type ReportType { get; }
        /// <summary>
        /// The earliest DateTime in the report
        /// </summary>
        DateTime StartDate { get; }
        /// <summary>
        /// The lastest DateTime in the report
        /// </summary>
        DateTime EndDate { get; }
        /// <summary>
        /// Rertieves the BaseReportData for the given entity and timeReference
        /// </summary>
        /// <param name="entity">The entity to get data for</param>
        /// <param name="timeRefrence">The DateTime to get data for</param>
        /// <returns>The data related to the given entity and timeReference</returns>
        BaseReportData GetReportData(Object entity, DateTime timeRefrence);
        /// <summary>
        /// Determines if a entity is contained within the report
        /// </summary>
        /// <param name="entity">The entity to check for</param>
        /// <returns>True if the entity is contained otherwise false</returns>
        Boolean Contains(Object entity);
        /// <summary>
        /// Determines if the DaterTime given is contained within the report
        /// </summary>
        /// <param name="timeReference">The DateTime to check for</param>
        /// <returns>True if the DateTime is contained otherwise false</returns>
        Boolean Contains(DateTime timeReference);        
    }
}
