using JetPayFileParser.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetPayFileParser.DataAccess
{
    public class CommonDAL
    {
        public DataTable GetTableSchema(string tableName)
        {
            return SqlHelper.GetTableSchema(ConfigInfo.DB_CONNECTION_STRING, tableName);
        }
        public DataTable GetParseInfo()
        {
            DataSet  dsParseInfo = SqlHelper.ExecuteDataset(ConfigInfo.DB_CONNECTION_STRING, CommandType.StoredProcedure, "GetParseInfo");
            return dsParseInfo.Tables[0];
        }
        public DataTable GetColumnMappingsByFileTypeId(int FileTypeId)
        {
            SqlParameter param = new SqlParameter("@FileTypeId", SqlDbType.Int);
            param.Value = FileTypeId;
            DataSet dsColumnMapping = SqlHelper.ExecuteDataset(ConfigInfo.DB_CONNECTION_STRING, CommandType.StoredProcedure, "GetColumnMappingsByFileTypeId", param);
            return dsColumnMapping.Tables[0];
        }
        public void AddErrorLog(long fileImportLogId, List<string> lstErrors, SqlConnection connection)
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("ErrorMessage", typeof(string));
            foreach (string error in lstErrors)
            {
                DataRow row = dt.NewRow();
                row["ErrorMessage"] = error;
                dt.Rows.Add(row);
            }
            SqlParameter[] parms = new SqlParameter[2] 
            {
                new SqlParameter("@List", SqlDbType.Structured),
                new SqlParameter("@FileImportLogId", SqlDbType.Int)
            };
            //foreach (var item in lstErrors)
            {

                parms[0].Value = dt;
                parms[1].Value = fileImportLogId;
                SqlHelper.ExecuteNonQuery(ConfigInfo.DB_CONNECTION_STRING, CommandType.StoredProcedure, "AddErrorLog", parms);
            }


        }
    }
}
