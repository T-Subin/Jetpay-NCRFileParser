using JetPayFileParser.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetPayFileParser.Business
{
    public class ParseReportManager
    {
        CommonDAL commonDAL = new CommonDAL();
        public DataTable GetParseInfo()
        {
            return commonDAL.GetParseInfo();
        }
    }
}
