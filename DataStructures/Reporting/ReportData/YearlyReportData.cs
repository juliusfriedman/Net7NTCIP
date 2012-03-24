using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using ASTITransportation.DataStructures;
using System.Threading;

namespace DataStructures.Reporting
{
    /// <summary>
    /// Yearly Report Data is a Generic Class.
    /// It represents data from an entire year.
    /// </summary>
    /// <typeparam name="T">The type of data being represented</typeparam>
    [Serializable]
    public class YearlyReportData<T> : BaseReportData
    {
        #region Fields
        /// <summary>
        /// The year to which this data corresponds
        /// </summary>
        int year;                
        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a YearlyReportData Class for the Given report and Year
        /// </summary>
        /// <param name="report">The BaseReport to which the data corresponds</param>
        /// <param name="year">The year to which the data corresponds</param>
        public YearlyReportData(BaseReport report, int year)
            :base(report)
        {
            //Assign the report to which this data corresponds
            this.Report = report;
            //Assign the year to which this data corresponds
            this.year = year;
            //Sanity check... ensure we are dealing with a year which is valid
            DateUtility.ValidateYear(year);
            InitializeData();
        }

        /// <summary>
        /// Instantiates a YearlyReportData Class for the Given report and Year
        /// </summary>
        /// <param name="report">The BaseReport to which the data corresponds</param>
        /// <param name="timeReference">The DateTime which  is set to the Year the data corresponds</param>
        public YearlyReportData(BaseReport report, DateTime timeReference)
            : this(report, timeReference.Year) { DateUtility.ValidateYear(report, timeReference); }

        #endregion

        #region Properties

        public new MonthlyReportData<T>[] this[object entity]
        {
            get
            {
                return (MonthlyReportData<T>[])DataDictionary[entity];
            }
            set
            {
                DataDictionary[entity] = (Array)value;
            }
        }

        /// <summary>
        /// Provides access to the Matrix which represents the data for the given entity and month
        /// </summary>
        /// <param name="entity">The entity to which the data corresponds</param>
        /// <param name="month">The month to which the data corresponds</param>
        /// <returns>The Matrix which corresponds to the data</returns>
        public MonthlyReportData<T> this[object entity, int month]
        {
            get
            {

                return this[entity][month - 1];
            }
            set
            {
                this[entity][month - 1] = value;
            }
        }

        /// <summary>
        /// The Year to which the Data corresponds
        /// </summary>
        public int Year { get { return year; } }
       
        #endregion        

        #region Methods

        /// <summary>
        /// Determines if the given DateTime is included in the yearly data
        /// </summary>
        /// <param name="date">The DateTime to check</param>
        /// <returns>True if the date is contained otherwise false</returns>
        public virtual bool Contains(DateTime date)
        {
            DateUtility.ValidateYear(Report, date);
            return date.Year == Year;
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

        /// <summary>
        /// Allocates the MonthlyReportData for each of the Entities in the ReportInformation of the Report
        /// </summary>
        protected override void InitializeData()
        {
            foreach (Object entity in Report.ReportInformation.ReportEntities)
            {
                MonthlyReportData<T>[] monthData = new MonthlyReportData<T>[12];
                //Console.WriteLine("YearlyReportData(" + entity + " Year=" + year + ") Wait...");
                for (int month = 0, realMonth = 1; realMonth < 13; ++month, ++realMonth) monthData[month] = new MonthlyReportData<T>(Report, realMonth, year);
                DataDictionary[entity] = monthData;
                //Console.WriteLine("YearlyReportData(" + entity + " Year=" + year + ") Complete!");
            }                         
        }
    }
}
