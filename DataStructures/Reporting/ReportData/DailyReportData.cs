using System;
using System.Collections.Generic;
using System.Linq;
using ASTITransportation.DataStructures;
using System.Threading;

namespace DataStructures.Reporting
{
    public class DailyReportData<T> : BaseReportData
    {

        #region Fields
        /// <summary>
        /// The month and year to which the data correspond
        /// </summary>
        int month, year, day;
        #endregion

        #region Constructor

        public DailyReportData(BaseReport report, int month, int year, int day)
            : base(report)
        {
            //Sanity check... ensure we are dealing with valid values
            DateUtility.ValidateDay(day, year, month);
            //Assign the month in review
            this.month = month;
            //Assign the year in review
            this.year = year;
            //Assign the day in review
            this.day = day;
            InitializeData();
        }



        public DailyReportData(BaseReport report, DateTime timeReference)
            : this(report, timeReference.Month, timeReference.Year, timeReference.Day)
        {
            //Sanity check... ensure the time refrence given is valid in the context of the report
            DateUtility.ValidateYear(report, timeReference); //need day overload -JAY
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
        /// The day of the year to which the data corresponds
        /// </summary>
        public int Day { get { return day; } }

        /// <summary>
        /// The Matrix which represents the data for the given entity
        /// </summary>
        /// <param name="entity">The entity to check for</param>
        /// <returns>The matrix of data for the given entity of the entire day</returns>
        public new T[][][] this[Object entity]
        {
            get
            {
                lock (SynchRoot)
                {
                    return (T[][][])base.DataDictionary[entity];
                }
            }
        }

        /// <summary>
        /// The Matrix which represents the data for the given entity on the given hour
        /// </summary>
        /// <param name="entity">The entity to check for</param>
        /// <param name="day">The day to which the data corresponds</param>
        /// <returns>The matrix of data for the given entity and hour</returns>
        public T[][] this[Object entity, int hour]
        {
            get
            {
                DateUtility.ValidateHour(hour);
                lock (SynchRoot)
                {
                    return this[entity][hour];
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
        public T[] this[Object entity, int hour, int minute]
        {
            get
            {
                DateUtility.ValidateDay(day, year, month);
                DateUtility.ValidateHour(hour);
                DateUtility.ValidateMinuteOrSecond(minute);
                lock (SynchRoot)
                {
                    return this[entity][hour][minute];
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
        public T this[Object entity, int hour, int minute, int second]
        {
            get
            {
                DateUtility.ValidateHour(hour);
                DateUtility.ValidateMinuteOrSecond(minute);
                DateUtility.ValidateMinuteOrSecond(second);
                lock (SynchRoot)
                {
                    return this[entity][hour][minute][second];
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
                    return this[entity][timeReference.Hour][timeReference.Minute][timeReference.Second];
                }
            }
        }

        #endregion

        #region Methods

        public virtual bool Contains(DateTime date)
        {
            return date.Year == Year && date.Month == Month && date.Day == day;
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
            foreach (Object entity in Report.ReportInformation.ReportEntities)
            {
                AllocateAndStartWorker(() =>
                {
                    Console.WriteLine("DailyReportData(" + entity + " Month = " + month + " Day = " + day + ") Wait...");
                    for (int hour = 0; hour < DateUtility.HoursInDay; ++hour) 
                        DataDictionary[entity] = BaseReportData.AllocateDataEntry<T>(DateUtility.HoursInDay, DateUtility.MinutesInHour, DateUtility.SecondsInMinute);
                    Console.WriteLine("DailyReportData(" + entity + " Month = " + month + " Day = " + day + ") Complete!");
                });
            }
        }
    }

}
