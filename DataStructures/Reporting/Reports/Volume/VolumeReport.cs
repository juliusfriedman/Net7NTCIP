using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ASTITransportation.DataStructures;

namespace DataStructures.Reporting
{
    [Serializable]
    public class VolumeReport<T> : BaseReport, IReport<T>
    {
        #region Fields        
        Dictionary<int, YearlyReportData<T>> yearlyData;
        #endregion

        #region Constructor

        //Will probably be internal or go away
        public VolumeReport(byte[] binary)
            : base(binary)
        {
            //now I have evyerthing or atleast i should            
        }

        //Going away??? or internal access
        public VolumeReport(ReportInformation reportInfo, YearlyReportData<T> data)
            : this(reportInfo)
        {
            yearlyData.Add(data.Year, data);            
        }

        public VolumeReport(ReportInformation reportInfo)
            : base(reportInfo, typeof(T))
        {                       
            //Instantiate the yearlyData Dictionary
            yearlyData = new Dictionary<int, YearlyReportData<T>>();
            //Allocate the memory for each year in the report
            InitializeData();                  
        }

        #endregion

        #region Properties

        public override BaseReportData[] ReportData
        {
            get
            {
                return base.ReportData = this.yearlyData.Values.ToArray();
            }
            protected set
            {
                base.ReportData = value;
                foreach (BaseReportData brd in value)
                {
                    if (brd is YearlyReportData<T>)
                    {
                        YearlyReportData<T> data = brd as YearlyReportData<T>;
                        yearlyData.Remove(data.Year);
                        yearlyData.Add(data.Year, data);
                    }
                    if (brd is MonthlyReportData<T>)
                    {
                        MonthlyReportData<T> data = brd as MonthlyReportData<T>;
                        YearlyReportData<T> yearData = yearlyData[data.Year];
                        foreach (Object entity in ReportEntities)
                        {
                            MonthlyReportData<T> eData = yearData[entity, data.Month];
                            if (eData == null) continue;
                            else yearData[entity, data.Month] = data;
                        }                        
                    }
                }
            }
        }

        #endregion

        #region Methods

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

        void InitializeData()
        {
            //from StartDate.Year -> EndDate.Year
            //Add a YearlyReportData for the corresponding year
            for (int year = StartDate.Year, yearEnd = EndDate.Year; year <= yearEnd; ++year) 
                yearlyData.Add(year, new YearlyReportData<T>(this, year));
        }

        #endregion

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

        public BaseReportData GetReportData(object entity, DateTime timeRefrence)
        {
            return yearlyData[timeRefrence.Year][entity, timeRefrence.Month - 1];
        }

        public bool Contains(object entity)
        {
            return Contains(entity);
        }

        public bool Contains(DateTime timeReference)
        {
            return Contains(timeReference);
        }

        #endregion          
    }
}
