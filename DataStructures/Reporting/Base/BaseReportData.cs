using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using ASTITransportation.DataStructures;
using System.Threading;

namespace DataStructures.Reporting
{
    [Serializable]
    public abstract class BaseReportData
    {
        #region Fields
        /// <summary>
        /// The report to which this data corresponds
        /// </summary>        
        BaseReport report;
        /// <summary>
        /// The dictionary we use to store corresponding data
        /// </summary>
        Dictionary<Object, Array> dataDictionary = new Dictionary<Object, Array>();
        #endregion

        #region Constructor
        /// <summary>
        /// Instantiates a BaseReportData
        /// </summary>
        /// <param name="report">The BaseReport to which this data corresponds</param>
        public BaseReportData(BaseReport report)
        {
            //Assign the report to which this data corresponds
            this.report = report;
            //Allocate memory for the data
            //Initialize();            
        }
        #endregion

        #region Properties
        /// <summary>
        /// The Dictionary which is used to store the Data
        /// </summary>
        public Dictionary<Object, Array> DataDictionary { get { lock (SynchRoot) return dataDictionary; } set { lock (SynchRoot) dataDictionary = value; } }
        
        /// <summary>
        /// Provides a Synchronization mechanism inside the DataDictionary
        /// </summary>     
        [System.Web.Script.Serialization.ScriptIgnore]
        protected Object SynchRoot { get { return ((IDictionary)dataDictionary).SyncRoot; } }
        
        /// <summary>
        /// Returns the Report to which this Data corresponds
        /// </summary>     
        [System.Web.Script.Serialization.ScriptIgnore]
        protected BaseReport Report { get { return report; } set { report = value; } }
        #endregion
        
        #region Indexers
        /// <summary>
        /// Retrieves the data for an individual entity
        /// </summary>
        /// <param name="entity">The entity to retirve the data for</param>
        /// <returns>The Array of data representing the given entity</returns>
        protected virtual Array this[Object entity] { get { lock (SynchRoot) { return dataDictionary[entity]; } } }
        #endregion
        
        #region Methods

        public string ToJson()
        {
            return ReportProvider.Serializer.Serialize(this);
        }

        /// <summary>
        /// Allocates a multidimensional array (Matrix) of the given geometry.         
        /// </summary>
        /// <param name="type">The type of elements in the matrix</param>
        /// <param name="hoursInDay">The number of hours in a day</param>
        /// <param name="minutesInHour">The number of minutes in a hour</param>
        /// <param name="secondsInMinute">The number of the seconds in a minute</param>
        /// <returns>The Matrix allocated</returns>
        static internal Array AllocateDataEntry<T>(int hoursInDay, int minutesInHour, int secondsInMinute)
        {

            T[][][] dataEntry = new T[hoursInDay][][];
            for (int hour = 0; hour < hoursInDay; ++hour)
            {
                dataEntry[hour] = new T[minutesInHour][];
                for (int minute = 0; minute < minutesInHour; ++minute) dataEntry[hour][minute] = new T[secondsInMinute];
            }

            return dataEntry as Array;
        }

        /// <summary>
        /// Allocates a multidimensional array (Matrix) of the given geometry.         
        /// </summary>
        /// <param name="type">The type of elements in the matrix</param>
        /// <param name="daysInMonth">The number of days in the month</param>
        /// <param name="hoursInDay">The number of hours in a day</param>
        /// <param name="minutesInHour">The number of minutes in a hour</param>
        /// <param name="secondsInMinute">The number of the seconds in a minute</param>
        /// <returns>The Matrix allocated</returns>
        static internal Array AllocateDataEntry<T>(int daysInMonth, int hoursInDay, int minutesInHour, int secondsInMinute)
        {
            //int[] lengths = new int[] { daysInMonth, hoursInDay, minutesInHour, secondsInMinute };

            //Array dataEntry = Array.CreateInstance(type, lengths);            
            /*
            int size = typeof(T) == typeof(int) ? 4 : 8;
            int pressure = size * daysInMonth * minutesInHour * secondsInMinute;
            pressure /= 2;
            GC.AddMemoryPressure(pressure);
            */
            T[][][][] dataEntry = new T[daysInMonth][][][];
            for (int day = 0; day < daysInMonth; ++day)
            {
                dataEntry[day] = new T[hoursInDay][][];
                for (int hour = 0; hour < hoursInDay; ++hour)
                {
                    dataEntry[day][hour] = new T[minutesInHour][];
                    for (int minute = 0; minute < minutesInHour; ++minute)
                    {
                        dataEntry[day][hour][minute] = new T[secondsInMinute];
                        /*for (int second = 0; second < secondsInMinute; ++second)
                        {
                            dataEntry[day][hour][minute][second] = default(T);
                        }*/
                    }
                }
            }

            //GC.RemoveMemoryPressure(pressure);

            return dataEntry as Array;
        }

        /// <summary>
        /// Allocates a multidimensional array (Matrix) of the given geometry. 
        /// </summary>
        /// <param name="type">The type of elements in the matrix</param>
        /// <param name="year">The year to which the entry is contained</param>
        /// <param name="month">The month to which the entry is contained</param>
        /// <returns></returns>
        static internal Array AllocateDataEntry<T>(int year, int month)
        {
            return AllocateDataEntry<T>(DateTime.DaysInMonth(year, month), DateUtility.HoursInDay, DateUtility.MinutesInHour, DateUtility.SecondsInMinute);
        }

        /// <summary>
        /// Determines if a entity is contained by this data
        /// </summary>
        /// <param name="entity">The entity to check for</param>
        /// <returns>True if the entity is contained otherwise false</returns>
        public virtual bool Contains(Object entity)
        {
            return DataDictionary.ContainsKey(entity);
        }

        #endregion

        #region Abstraction
        /// <summary>
        /// Loads the BaseReportData for the given entity if it is not already loaded
        /// </summary>
        /// <param name="entity">The entity to load data for</param>
        protected abstract void LoadEntityData(Object entity);
        /// <summary>
        /// Loads the BaseReportData for all entities in the ReportInformation if not already loaded
        /// </summary>
        protected abstract void LoadEntityData();

        protected abstract System.Data.DataSet GetEntityDataSet(object entity);

        protected abstract void InitializeData();

        #endregion
    }
}
