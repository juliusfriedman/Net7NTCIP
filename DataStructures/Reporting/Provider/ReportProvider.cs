using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataStructures.Reporting
{
    public static class ReportProvider
    {
        #region Fields

        internal static System.Web.Script.Serialization.JavaScriptSerializer Serializer = new System.Web.Script.Serialization.JavaScriptSerializer()
        {
            MaxJsonLength = int.MaxValue
        };

        internal static System.Runtime.Serialization.Formatters.Binary.BinaryFormatter BinaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

        static Dictionary<string, Type> typeDictionary = new Dictionary<string, Type>();        

        static string ConnectionString = "Server=172.16.100.220;DataBase=ChipsWeb;Integrated Security=SSPI";

        #endregion

        #region Properties

        public static Dictionary<string, Type> AvailableReports { get { return typeDictionary; } }
        
        internal static System.Data.SqlClient.SqlConnection DatabaseConnection
        {
            get{ return new System.Data.SqlClient.SqlConnection(ConnectionString);}
        }

        #endregion

        #region Methods

        public static DateTime EarliestDate(string reportType, object entity)
        {
            //find earliest date in data base from sql dataset using reportType to filter the rows
            //if there is a reportId in the ReportStore get the report data Deserialize and return
            //Type systemType = typeDictionary[reportType];
            DateTime result = DateTime.Now;
            using (System.Data.SqlClient.SqlConnection connection = DatabaseConnection)
            {
                connection.Open();
                using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand("Select startDate From ReportStore Where reportType Like '%"+reportType+"%'", connection))
                {
                    //command.Parameters.AddWithValue("reportType", reportType);
                    using (System.Data.SqlClient.SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime compare = reader.GetDateTime(0);
                            if (compare < result) result = compare;
                        }
                    }
                }
            }
            
            //Select startDate From ReportStore Where reportType = systemType.FullName
            return result;
        }

        public static DateTime LatestDate(string reportType, object entity)
        {
            //find earliest date in data base from sql dataset using reportType to filter the rows

            /*
            Select ReportMap.reportId, ReportMap.entityId, ReportMap.extraData, ReportStore.reportBinary, ReportStore.startDate, ReportStore.endDate from ReportMap
            Inner Join ReportStore
            On ReportMap.reportId = ReportStore.reportId
            */

            //find earliest date in data base from sql dataset using reportType to filter the rows
            //if there is a reportId in the ReportStore get the report data Deserialize and return
            Type systemType = typeDictionary[reportType];
            DateTime result = DateTime.Now;
            using (System.Data.SqlClient.SqlConnection connection = DatabaseConnection)
            {
                connection.Open();
                using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand("Select endDate From ReportStore Where reportType Like '%" + reportType + "%'", connection))
                {
                    using (System.Data.SqlClient.SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime compare = reader.GetDateTime(0);
                            if (compare > result) result = compare;
                        }
                    }
                }
            }

            //Select startDate From ReportStore Where reportType = systemType.FullName
            return result;
        }

        public static BaseReport FromReportId(Guid id)
        {
            return BaseReport.FromReportId(id);
        }

        public static BaseReport FromFriendlyName(string friendlyName)
        {
            byte[] data = null;
            //get data 
            //field Select * From ReportStore Where friendlyName = friendlyName
            //read data into data
            return BaseReport.Deserialize(data);
        }

        public static BaseReport FromBinary(byte[] data)
        {
            return BaseReport.Deserialize(data);
        }

      

        #endregion

        #region Constructor

        static ReportProvider()
        {
            Type baseReportType = typeof(BaseReport);
            char[] split = new char[]{'.'};
            foreach (Type type in baseReportType.Assembly.GetTypes().Where(type => type.BaseType == baseReportType))
            {
                string typeName = type.FullName;
                typeName = typeName.Split(split).Last();
                typeName = typeName.Substring(0, typeName.IndexOf('`'));
                typeDictionary.Add(typeName, type);
            }
        }

        #endregion
    }
}
