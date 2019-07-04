using System.Data;
using JetPayFileParser.Model;
using System.Data.SqlClient;

namespace JetPayFileParser.DataAccess
{
    public class ReservesDAL
    {
        public void MoveDataToReserves(long fileImportId)
        {
            SqlParameter[] parms ={
                                    new SqlParameter("@fileImportId",SqlDbType.BigInt)
            };
            parms[0].Value = fileImportId;
            SqlHelper.ExecuteNonQuery(ConfigInfo.DB_CONNECTION_STRING, CommandType.StoredProcedure, "MoveDataToReserves_JetPay", parms);
        }       
    }
}
