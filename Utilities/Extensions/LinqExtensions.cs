using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.Data.SqlClient;
using System.Data.Linq.Mapping;
using System.ComponentModel;

namespace ASTITransportation.Extensions
{
    /// <summary>
    /// LinQ  Extentensions
    /// </summary>
    public static class LinqExtensions
    {

        #region DataContext Extensions

        /// <summary>
        /// Calls table.Attach(item, true)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="item"></param>
        public static void UpdateOnSubmit<T>(this System.Data.Linq.Table<T> table, T item) where T : class, System.ComponentModel.INotifyPropertyChanged
        {
            table.Attach(item, true);
        }

        /// <summary>
        /// Extention method for a DataContext object that checks if the object exists in the DataContext
        /// </summary>
        /// <param name="user"></param>
        /// <returns>True or false</returns>   
        public static bool IsValid<T>(this System.Data.Linq.Table<T> table, T item) where T : class, System.ComponentModel.INotifyPropertyChanged
        {
            try
            {
                if (null == table) return false;
                if (null == item) return false;
                else return table.Where(i => i == item).FirstOrDefault() != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Extension method for DataContext objects that determine if the object is already contained in the DataContext
        /// </summary>
        /// <param name="item">The entity to check existance for</param>
        /// <returns>True if the Entity exists otherwise false</returns>
        public static bool IsValid<T>(this System.Data.Linq.DataContext dataContext, T item) where T : class, System.ComponentModel.INotifyPropertyChanged
        {
            if (null == item) return false;
            try
            {
                System.Data.Linq.Table<T> table = dataContext.GetTable<T>();
                if (table == null) return false;
                else return table.Contains(item);
            }
            catch
            {
                return false;
            }
        }

        #region Incomplete

        /// <summary>
        /// Gets the primary key of a Linq-to-SQL table; requires that the table has a PRIMARY KEY NOT NULL
        /// </summary>
        /// <typeparam name="T">Type that you wish to find the primary key of</typeparam>
        /// <returns>PropertyInfo of the Primary Key of the supplied Type</returns>
        internal static PropertyInfo GetPrimaryKey<T>() where T : class, INotifyPropertyChanged
        {
            PropertyInfo[] infos = typeof(T).GetProperties();
            PropertyInfo PKProperty = null;
            foreach (PropertyInfo info in infos)
            {
                var column = info.GetCustomAttributes(false)
                    .Where(x => x.GetType() == typeof(ColumnAttribute))
                    .FirstOrDefault(x => ((ColumnAttribute)x).IsPrimaryKey && ((ColumnAttribute)x).DbType.Contains("NOT NULL"));
                if (column != null)
                {
                    PKProperty = info;
                    break;
                }
            }
            if (PKProperty == null)
                throw new NotSupportedException(typeof(T).ToString() + " has no Primary Key");
            return PKProperty;
        }

        internal static PropertyInfo[] GetForeignKeys<T>() where T : class, INotifyPropertyChanged
        {
            List<PropertyInfo> temp = new List<PropertyInfo>();
            var pKey = GetPrimaryKey<T>();
            PropertyInfo[] infos = typeof(T).GetProperties();
            PropertyInfo PKProperty = null;
            foreach (PropertyInfo info in infos)
            {
                var column = info.GetCustomAttributes(false)
                    .Where(x => x.GetType() == typeof(AssociationAttribute))
                    .FirstOrDefault(x => ((AssociationAttribute)x).ThisKey == pKey.Name && ((ColumnAttribute)x).DbType.Contains("NOT NULL"));
                if (column != null)
                {
                    //PKProperty = info;
                    temp.Add(info);
                    //break;
                }
            }            
            return temp.ToArray();
        }

        internal static bool DeleteWithCascase<T>(System.Data.Linq.DataContext ctx, T entity) where T : class, INotifyPropertyChanged
        {
            //Get the primary key of the context
            var pkey = GetPrimaryKey<T>();
            //Get foreign keys
            var fkeys = GetForeignKeys<T>();
            //get the model for the ctx
            var model = new AttributeMappingSource().GetModel(ctx.GetType());
            //Get the table for the given entity
            var eTable = ctx.GetTable<T>();
            foreach (var modelTable in model.GetTables())
            {
                //Loop tables and delete
            }

            return true;
        }

        #endregion

        #endregion

        /// <summary>
        /// Converts the Linq data to a comma seperated string including header.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static string ToCSVString(this System.Linq.IOrderedQueryable data)
        {
            return ToCSVString(data, "; ");
        }

        /// <summary>
        /// Converts the Linq data to a commaseperated string including header.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <returns></returns>
        public static string ToCSVString(this System.Linq.IOrderedQueryable data, string delimiter)
        {
            return ToCSVString(data, "; ", null);
        }

        /// <summary>
        /// Converts the Linq data to a commaseperated string including header.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <param name="nullvalue">The nullvalue.</param>
        /// <returns></returns>
        public static string ToCSVString(this System.Linq.IOrderedQueryable data, string delimiter, string nullvalue)
        {
            StringBuilder csvdata = new StringBuilder();
            string replaceFrom = delimiter.Trim();
            string replaceDelimiter = ";";
            System.Reflection.PropertyInfo[] headers = data.ElementType.GetProperties();
            switch (replaceFrom)
            {
                case ";":
                    replaceDelimiter = ":";
                    break;
                case ",":
                    replaceDelimiter = "¸";
                    break;
                case "\t":
                    replaceDelimiter = "    ";
                    break;
                default:
                    break;
            }
            if (headers.Length > 0)
            {
                foreach (var head in headers)
                {
                    csvdata.Append(head.Name.Replace("_", " ") + delimiter);
                }
                csvdata.Append("\n");
            }
            foreach (var row in data)
            {
                var fields = row.GetType().GetProperties();
                int fieldsLength = fields.Length;
                for (int i = 0; i < fieldsLength; ++i)
                {
                    object value = null;
                    try
                    {
                        value = fields[i].GetValue(row, null);
                    }
                    catch { }
                    if (value != null)
                    {
                        csvdata.Append(value.ToString().Replace("\r", "\f").Replace("\n", " \f").Replace("_", " ").Replace(replaceFrom, replaceDelimiter) + delimiter);
                    }
                    else
                    {
                        csvdata.Append(nullvalue);
                        csvdata.Append(delimiter);
                    }
                }
                csvdata.Append("\n");
            }
            return csvdata.ToString();
        }

        public static DataTable LINQToDataTable<T>(IEnumerable<T> varlist)
        {
            DataTable dtReturn = new DataTable();

            // column names 
            PropertyInfo[] oProps = null;

            if (varlist == null) return dtReturn;

            foreach (T rec in varlist)
            {
                // Use reflection to get property names, to create table, Only first time, others will follow 
                if (oProps == null)
                {
                    oProps = ((Type)rec.GetType()).GetProperties();
                    foreach (PropertyInfo pi in oProps)
                    {
                        Type colType = pi.PropertyType;

                        if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition()
                        == typeof(Nullable<>)))
                        {
                            colType = colType.GetGenericArguments()[0];
                        }

                        dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
                    }
                }

                DataRow dr = dtReturn.NewRow();

                foreach (PropertyInfo pi in oProps)
                {
                    dr[pi.Name] = pi.GetValue(rec, null) == null ? DBNull.Value : pi.GetValue
                    (rec, null);
                }

                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }

        public static DataTable ToDataTable(System.Data.Linq.DataContext ctx, object query)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            IDbCommand cmd = ctx.GetCommand(query as IQueryable);
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.SelectCommand = (SqlCommand)cmd;
            DataTable dt = new DataTable("sd");

            try
            {
                cmd.Connection.Open();
                adapter.FillSchema(dt, SchemaType.Source);
                adapter.Fill(dt);
            }
            finally
            {
                cmd.Connection.Close();
            }
            return dt;
        }       
    }

}
