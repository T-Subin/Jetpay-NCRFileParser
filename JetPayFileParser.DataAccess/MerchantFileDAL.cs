
using System.Data;
using JetPayFileParser.Model;
using System.Data.SqlClient;

namespace JetPayFileParser.DataAccess
{
    public class MerchantFileDAL
    {
        public void MoveDataToMerchantTable(long fileImportId)
        {
            SqlParameter[] parms ={
                                    new SqlParameter("@fileImportId",SqlDbType.BigInt)
            };
            parms[0].Value = fileImportId;
            SqlHelper.ExecuteNonQuery(ConfigInfo.DB_CONNECTION_STRING, CommandType.StoredProcedure, "MoveDataToAdjustmentReserves_JetPay", parms);
        }
    }
}
