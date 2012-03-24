using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DataStructures.Reporting
{
    [Serializable]
    public abstract class BaseReport : ISerializable
    {
        #region Fields        
        ReportInformation reportInformation;
        Type reportType;
        Type baseReportType;
        BaseReportData[] reportData;
        Guid reportId = Guid.Empty;
        string friendlyName;
        System.Data.SqlClient.SqlParameter[] storedProcedureParameters;        
        #endregion

        #region Constructor

        public BaseReport(ReportInformation reportInformation, Type reportType, BaseReportData reportData = null)            
        {
            this.reportInformation = reportInformation;
            this.reportType = reportType;
            //this.reportData = reportData;
            this.baseReportType = GetType();
            if (baseReportType.IsGenericType) baseReportType = baseReportType.GetGenericTypeDefinition();
            InitializeReport();
        }

        protected BaseReport(Byte[] binary)
        {

            BaseReport pointer = Deserialize(binary);

            this.reportInformation = pointer.reportInformation;
            this.reportType = pointer.reportType;
            this.reportId = pointer.reportId;
            //this.reportData = pointer.reportData;
            //Already Initialized from binary
            //InitializeBase();
        }

        public BaseReport(SerializationInfo info, StreamingContext context)
        {
            this.reportInformation = (ReportInformation)info.GetValue("reportInformation", typeof(ReportInformation));
            this.reportType = (Type)info.GetValue("reportType", typeof(Type));
            this.baseReportType = (Type)info.GetValue("baseReportType", typeof(Type));
            this.reportId = (Guid)info.GetValue("reportId", typeof(Guid));
            this.reportData = (BaseReportData[])info.GetValue("reportData", typeof(BaseReportData[]));
            this.friendlyName = info.GetString("friendlyName");
            //this.DateTimeField = info.GetString("DateTimeField");
            //this.DataField = info.GetString("DataField");
            //this.StoredProcedureName = info.GetString("StoredProcedureName");
            //this.storedProcedureParameters = (System.Data.SqlClient.SqlParameter[])info.GetValue("storedProcedureParameters", typeof(Array));
        }

        #endregion

        #region Properties

        protected string ReportTypeString { get { return ReportProvider.AvailableReports.SingleOrDefault(pair => pair.Value.Equals(baseReportType)).Key; } }

        public DateTime StartDate { get { return ReportInformation.StartDate; } }

        public DateTime EndDate { get { return ReportInformation.EndDate; } }

        public TimeSpan TotalDuration { get { return ReportInformation.TotalDuration; } }

        public Object[] ReportEntities { get { return ReportInformation.ReportEntities.ToArray(); } }

        public String FriendlyName { get { if(string.IsNullOrEmpty(friendlyName)) return string.Empty; return friendlyName; } protected set { friendlyName = value; } }

        public Guid ReportId { get { return reportId; } }

        public virtual BaseReportData[] ReportData { get { return reportData; } protected set { reportData = value; } }

        [System.Web.Script.Serialization.ScriptIgnore]
        public ReportInformation ReportInformation { get { return reportInformation; } }

        [System.Web.Script.Serialization.ScriptIgnore]
        public Type ReportType { get { return reportType; } }

        protected System.Data.SqlClient.SqlParameter[] StoredProcedureParameters { get { return storedProcedureParameters; } set { storedProcedureParameters = value; } }
        
        protected String DataField { get; set; }

        protected String DateTimeField { get; set; }

        protected String StoredProcedureName { get; set; }

        #endregion

        #region Methods

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("reportInformation", this.reportInformation);
            info.AddValue("reportType", this.reportType);
            info.AddValue("baseReportType", this.baseReportType);
            info.AddValue("reportId", this.reportId);
            info.AddValue("repportData", this.reportData);
            info.AddValue("friendlyName", this.friendlyName);
            //Array arr = this.storedProcedureParameters;
            //info.AddValue("storedProcedureParameters", arr);
            //info.AddValue("DateTimeField", this.DateTimeField);
            //info.AddValue("DataField", this.DataField);
            //info.AddValue("StoredProcedureName", this.StoredProcedureName);
        }

        public virtual string ToJson()
        {
            return ReportProvider.Serializer.Serialize(this);
        }

        public byte[] Serialize()
        {
            using(System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                ReportProvider.BinaryFormatter.Serialize(ms, this);
                ms.Position = 0;
                return ms.ToArray();
            }            
        }

        public void StoreToDatabase()
        {
            using (System.Data.SqlClient.SqlConnection connection = ReportProvider.DatabaseConnection)
            {

                connection.Open();

                System.Data.SqlClient.SqlCommand command;

                //If the report has a Guid which is not Empty then it has been serialized before
                if (ReportId != Guid.Empty)
                {
                    //update

                    //should also remove old entities from the data set before serializing

                    command = new System.Data.SqlClient.SqlCommand("UPDATE ReportStore SET reportBinary = @reportBinary, friendlyName = @friendlyName", connection);
                    command.Parameters.AddWithValue("reportBinary", Serialize());
                    command.Parameters.AddWithValue("friendlyName", FriendlyName);
                    
                    int affected = command.ExecuteNonQuery();

                    command.Dispose();
                    command = null;

                    command = new System.Data.SqlClient.SqlCommand("INSERT INTO ReportMap (reportId, entityId) Values (@reportId, @entityId)", connection);
                    command.Parameters.AddWithValue("reportId", reportId);
                    command.Parameters.AddWithValue("entityId", null);

                    //foreach Entity add the supporting entry to the ReportMap
                    foreach (object entity in ReportEntities)
                    {
                        try
                        {
                            command.Parameters["entityId"].Value = entity;
                            affected = command.ExecuteNonQuery();
                        }
                        catch
                        {
                            //already there see above notes...
                        }
                    }
                    

                }
                else
                {
                    //insert
                    command = new System.Data.SqlClient.SqlCommand("INSERT INTO ReportStore (reportId, reportType, reportBinary, startDate, endDate, friendlyName) Values (@reportId, @reportType, @reportBinary, @startDate, @endDate, @friendlyName)", connection);
                    reportId = Guid.NewGuid();
                    command.Parameters.AddWithValue("reportId", reportId);
                    command.Parameters.AddWithValue("reportType", ReportTypeString);
                    command.Parameters.AddWithValue("reportBinary", Serialize());
                    command.Parameters.AddWithValue("startDate", StartDate);
                    command.Parameters.AddWithValue("endDate", EndDate);
                    command.Parameters.AddWithValue("friendlyName", FriendlyName);
                    int affected = command.ExecuteNonQuery();


                    command.Dispose();
                    command = null;


                    command = new System.Data.SqlClient.SqlCommand("INSERT INTO ReportMap (reportId, entityId) Values (@reportId, @entityId)", connection);
                    command.Parameters.AddWithValue("reportId", reportId);                    
                    command.Parameters.AddWithValue("entityId", null);

                    //foreach Entity add the supporting entry to the ReportMap
                    foreach (object entity in ReportEntities)
                    {
                        command.Parameters["entityId"].Value = entity;
                        affected = command.ExecuteNonQuery();
                    }
                }

                command.Dispose();
                command = null;
            }
            
        }       

        internal static BaseReport Deserialize(byte[] binary)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(binary))
            {
                try
                {
                    return ReportProvider.BinaryFormatter.Deserialize(ms) as BaseReport;
                }
                catch { return null; }                
            }
        }

        internal static BaseReport FromReportId(Guid ReportId)
        {
            //if there is a reportId in the ReportStore get the report data Deserialize and return
            using (System.Data.SqlClient.SqlConnection connection = ReportProvider.DatabaseConnection)
            {
                connection.Open();
                using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand("Select reportBinary from ReportStore Where reportId = @reportId", connection))
                {
                    command.Parameters.AddWithValue("reportId", ReportId);
                    using (System.Data.SqlClient.SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) return Deserialize(reader.GetSqlBinary(0).Value);
                        else return null;
                    }
                }
            }
        }

        #endregion        

        #region Abstraction

        protected abstract void InitializeReport();

        #endregion
    }
}
