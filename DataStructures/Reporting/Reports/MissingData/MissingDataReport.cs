using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataStructures.Reporting
{
    /// <summary>
    /// MissingDataReport is a Generic Class.
    /// It is constructed using any other BaseReport
    /// </summary>
    /// <typeparam name="T">The type of elements in the MissingDataReport</typeparam>
    public class MissingDataReport<T> : BaseReport, IReport<T>
    {
        /// <summary>
        /// The report to which this report was calulcated from
        /// </summary>
        BaseReport report;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="report"></param>
        public MissingDataReport(BaseReport report)
            :base(report.ReportInformation, typeof(T))
        {
            this.report = report;
        }

        /// <summary>
        /// The Report from which this MissingDataReport was generated
        /// </summary>
        public BaseReport Report { get { return report; } }

        #region IReport<T> Members

        T IReport<T>.Total()
        {
            throw new NotImplementedException();
        }

        T IReport<T>.Total(object entity)
        {
            throw new NotImplementedException();
        }

        T IReport<T>.Total(object[] entities)
        {
            throw new NotImplementedException();
        }

        T IReport<T>.Total(object[] entities, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        T IReport<T>.Average()
        {
            throw new NotImplementedException();
        }

        T IReport<T>.Average(object entity)
        {
            throw new NotImplementedException();
        }

        T IReport<T>.Average(object[] entities)
        {
            throw new NotImplementedException();
        }

        T IReport<T>.Average(object[] entities, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        bool IReport<T>.Contains(object entity)
        {
            return Report.ReportInformation.Contains(entity);
        }

        bool IReport<T>.Contains(DateTime timeReference)
        {
            return Report.ReportInformation.Contains(timeReference);
        }

        Type IReport<T>.ReportType
        {
            get { return Report.ReportType; }
        }

        DateTime IReport<T>.StartDate
        {
            get { return Report.StartDate; }
        }

        DateTime IReport<T>.EndDate
        {
            get { return Report.EndDate; }
        }

        BaseReportData IReport<T>.GetReportData(object entity, DateTime timeRefrence)
        {
            throw new NotImplementedException();
        }

        #endregion   
    
        protected override void InitializeReport()
        {
            /*No Sql Initialization for MissingDataReport*/
        }
         
    }
}
