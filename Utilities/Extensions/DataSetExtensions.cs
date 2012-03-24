using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

/*

 * Client Example
function outputDT(dataTable)
{
    var headers = [];
    var rows = [];

    headers.push("<tr>");
    for (var name in dataTable[0])
        headers.push("<td><b>"+name+"</b></td>");
    headers.push("</tr>");

    for (var row in dataTable)
    {
        rows.push("<tr>");
        for (var name in dataTable[row])
        {
            rows.push("<td>");
            rows.push(dataTable[row][name]);
            rows.push("</td>");
        }
        rows.push("</tr>");
    }            

    var top = "<table border='1'>";
    var bottom = "</table>";  

    return top + headers.join("") + rows.join("") + bottom;
} 

 
 */

namespace ASTITransportation.Extensions
{
    public static class DataSetExtensions
    {
        public static List<Dictionary<string, object>> ToListDictionary(this DataTable table)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            foreach (DataRow dr in table.Rows)
            {
                Dictionary<string, object> drow = new Dictionary<string, object>();
                for (int i = 0, e = table.Columns.Count; i < e; ++i) drow.Add(table.Columns[i].ColumnName, dr[i]);                
                result.Add(drow);
            }
            return result;
        }

        public static Dictionary<string, object> ToDictionary(this DataTable table)
        {            
            return new Dictionary<string, object>()
            {
                { table.TableName, ToListDictionary(table) }
            };
        }

        public static Dictionary<string, object> ToDictionary(this DataSet data)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (DataTable table in data.Tables) result.Add(table.TableName, ToListDictionary(table));
            return result;
        }
    }
}
