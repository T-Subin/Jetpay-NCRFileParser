using JetPayFileParser.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetPayFileParser.DataAccess
{
    public class DepFundingDAL
    {        
        public void MoveDataToTransactionTables()
        {
            SqlHelper.ExecuteNonQuery(ConfigInfo.DB_CONNECTION_STRING, CommandType.StoredProcedure, "MoveDataToFundingDepositSummary_JetPay");
        }
    }
}
