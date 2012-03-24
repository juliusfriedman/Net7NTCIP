using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataStructures.Reporting
{
    public class DensityReport<T> : BaseReport, IReport<T>
    {

        public DensityReport(ReportInformation reportInfo, DateTime start, DateTime end)
            : base(reportInfo, typeof(T))
        {
        }

        #region IReport<T> Members

        public T Total()
        {
            throw new NotImplementedException();
        }

        public T Total(object entity)
        {
            throw new NotImplementedException();
        }

        public T Total(object[] entities)
        {
            throw new NotImplementedException();
        }

        public T Total(object[] entities, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public T Average()
        {
            throw new NotImplementedException();
        }

        public T Average(object entity)
        {
            throw new NotImplementedException();
        }

        public T Average(object[] entities)
        {
            throw new NotImplementedException();
        }

        public T Average(object[] entities, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public bool Contains(object entity)
        {
            throw new NotImplementedException();
        }

        public bool Contains(DateTime timeReference)
        {
            throw new NotImplementedException();
        }

        public BaseReportData GetReportData(object entity, DateTime timeRefrence)
        {
            throw new NotImplementedException();
        }

        #endregion   

        protected override void InitializeReport()
        {
            this.DataField = "Volume";
            this.DateTimeField = "CheckInTimeLocal";
            this.StoredProcedureName = "spGetDeviceVolume";
            this.StoredProcedureParameters = new System.Data.SqlClient.SqlParameter[]
            {
                new System.Data.SqlClient.SqlParameter("DeviceID", System.Data.SqlDbType.UniqueIdentifier),
                new System.Data.SqlClient.SqlParameter("LaneGroup", System.Data.SqlDbType.Char),
                new System.Data.SqlClient.SqlParameter("TimeZone", System.Data.SqlDbType.Int),
                new System.Data.SqlClient.SqlParameter("FromTimeUTC", System.Data.SqlDbType.DateTime),
                new System.Data.SqlClient.SqlParameter("ToUTC", System.Data.SqlDbType.DateTime)
            };
        }
           
    }
}
