using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Data;
using JetPayFileParser.Model;
using JetPayFileParser.Utility;

namespace JetPayFileParser.DataAccess
{
    public class ProcessorAuthDAL
    {
        public void MoveDataToTransactionTables()
        {
            SqlHelper.ExecuteNonQuery(ConfigInfo.DB_CONNECTION_STRING, CommandType.StoredProcedure, "MoveDataToACQAuth_JetPay");
        }
        public void MoveDataToTransactionTables(long fileImportId)
        {
            SqlParameter[] parms ={
                                    new SqlParameter("@fileImportId",SqlDbType.BigInt)
            };
            parms[0].Value = fileImportId;
            SqlHelper.ExecuteNonQuery(ConfigInfo.DB_CONNECTION_STRING, CommandType.StoredProcedure, "MoveDataToACQAuth_JetPay", parms);
        }
    }
}
