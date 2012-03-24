using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ASTITransportation.DataStructures;
using System.Threading;

namespace DataStructures.Reporting
{
    /// <summary>
    /// Monthly Report Data is a Generic Class.
    /// It provides data for 1 month out of a given year.
    /// </summary>
    /// <typeparam name="T">The type of Data being represented</typeparam>
    [Serializable]
    public class MonthlyReportData<T> : BaseReportData
    {
        #region Fields
        /// <summary>
        /// The month and year to which the data correspond
        /// </summary>
        int month, year;
        #endregion

        #region Constructor

        public MonthlyReportData(BaseReport report, int month, int year)
            :base(report)
        {
            //Sanity check... ensure we are dealing with valid values
            DateUtility.ValidateMonth(month);
            DateUtility.ValidateYear(year);
            //Assign the month in review
            this.month = month;
            //Assign the year in review
            this.year = year;
            InitializeData();
        }

        public MonthlyReportData(BaseReport report, DateTime timeReference)
            : this(report, timeReference.Month, timeReference.Year) 
        { 
            //Sanity check... ensure the time refrence given is valid in the context of the report
            DateUtility.ValidateYear(report, timeReference); 
        }

        #endregion

        #region Properties
        /// <summary>
        /// The Month to which the data corresponds
        /// </summary>
        public int Month { get { return month; } }

        /// <summary>
        /// The year to which the data corresponds
        /// </summary>
        public int Year { get { return year; } }

        /// <summary>
        /// The Matrix which represents the data for the given entity
        /// </summary>
        /// <param name="entity">The entity to check for</param>
        /// <returns>The matrix of data for the given entity</returns>
        public new T[][][][] this[Object entity]
        {
            get
            {                
                lock (SynchRoot)
                {
                    return (T[][][][])base.DataDictionary[entity];
                }
            }
        }

        /// <summary>
        /// The Matrix which represents the data for the given entity on the given day
        /// </summary>
        /// <param name="entity">The entity to check for</param>
        /// <param name="day">The day to which the data corresponds</param>
        /// <returns>The matrix of data for the given entity and day</returns>
        public T[][][] this[Object entity, int day]
        {
            get
            {
                DateUtility.ValidateDay(day, year, month); 
                lock (SynchRoot)
                {
                    return this[entity][day - 1];
                }
            }
        }

        /// <summary>
        /// The Matrix which represents the data for the given entity on the given day
        /// </summary>
        /// <param name="entity">The entity to check for</param>
        /// <param name="day">The day to which the data corresponds</param>
        /// <param name="hour">The hour to which the data corresponds</param>
        /// <returns>The matrix of data for the given entity and day and hour</returns>
        public T[][] this[Object entity, int day, int hour]
        {
            get
            {
                DateUtility.ValidateDay(day, year, month); DateUtility.ValidateHour(hour);
                lock (SynchRoot)
                {
                    return this[entity, day][hour];
                }
            }
        }

        /// <summary>
        /// The Matrix which represents the data for the given entity on the given day
        /// </summary>
        /// <param name="entity">The entity to check for</param>
        /// <param name="day">The day to which the data corresponds</param>
        /// <param name="hour">The hour to which the data corresponds</param>
        /// <param name="minute">The minute to which the data corresponds</param>
        /// <returns>The matrix of data for the given entity and day and hour and minute</returns>
        public T[] this[Object entity, int day, int hour, int minute]
        {
            get
            {
                DateUtility.ValidateDay(day, year, month); 
                DateUtility.ValidateHour(hour);
                DateUtility.ValidateMinuteOrSecond(minute);
                lock (SynchRoot)
                {
                    return this[entity, day][hour][minute];
                }
            }
        }

        /// <summary>
        /// The Matrix which represents the data for the given entity on the given day
        /// </summary>
        /// <param name="entity">The entity to check for</param>
        /// <param name="day">The day to which the data corresponds</param>
        /// <param name="hour">The hour to which the data corresponds</param>
        /// <param name="minute">The minute to which the data corresponds</param>
        /// <param name="second">The second to which data corresponds</param>
        /// <returns>The matrix of data for the given entity and day and hour and minute and second</returns>
        public T this[Object entity, int day, int hour, int minute, int second]
        {
            get
            {
                DateUtility.ValidateDay(day, year, month);
                DateUtility.ValidateHour(hour);
                DateUtility.ValidateMinuteOrSecond(minute);
                DateUtility.ValidateMinuteOrSecond(second);
                lock (SynchRoot)
                {
                    return this[entity, day][hour][minute][second];
                }
            }
        }

        /// <summary>
        /// The Matrix which represents the data for the given entity on the given day
        /// </summary>
        /// <param name="entity">The entity to check for</param>
        /// <param name="timeReference">The DateTime which is set to the date and time of interest</param>
        /// <returns>The matrix of data for the given entity and time reference</returns>
        public T this[Object entity, DateTime timeReference]
        {
            get
            {
                DateUtility.ValidateYear(base.Report, timeReference);
                lock (SynchRoot)
                {
                    return this[entity, timeReference.Day - 1][timeReference.Hour][timeReference.Minute][timeReference.Second];
                }
            }
        }

        #endregion

        #region Methods

        public virtual bool Contains(DateTime date)
        {
            return date.Year == Year && date.Month == Month;
        }

        #endregion

        protected override void LoadEntityData(object entity)
        {
            throw new NotImplementedException();
        }

        protected override void LoadEntityData()
        {
            throw new NotImplementedException();
        }

        protected override System.Data.DataSet GetEntityDataSet(object entity)
        {
            throw new NotImplementedException();
        }

        protected override void InitializeData()
        {
            //Monthly Data can contain the DailyData to reduce overhead of allocation
            foreach (Object entity in Report.ReportInformation.ReportEntities)
            {
                DataDictionary[entity] = AllocateDataEntry<T>(year, month);
            }

            /*foreach (Object entity in Report.ReportInformation.ReportEntities)
            {
                AllocateAndStartWorker(() =>
                {
                    //Here this should be using the AllocateDataEntry for each day in mon.
                    Console.WriteLine("MonthlyReportData(" + entity + " Month = " + month + ") Wait...");
                    int daysInMonth = DateUtility.DaysInMonth(month, year) - 1;
                    DailyReportData<T>[] data = new DailyReportData<T>[daysInMonth];
                    for (int day = 0, realDay = 1; day < daysInMonth; ++day, ++realDay) data[day] = new DailyReportData<T>(Report, Month, Year, realDay);
                    DataDictionary[entity] = data;
                    Console.WriteLine("MonthlyReportData(" + entity + " Month = " + month + ") Complete!");
                });
            }*/
        }
    }
}
