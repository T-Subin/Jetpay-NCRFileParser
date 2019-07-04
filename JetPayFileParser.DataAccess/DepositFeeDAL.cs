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
    public class DepositFeeDAL
    {
        public void MoveDataToDepositFeeData()
        {
            SqlHelper.ExecuteNonQuery(ConfigInfo.DB_CONNECTION_STRING, CommandType.StoredProcedure, "MoveDataToDepositFee_JetPay");
            
        }        
    }
}
