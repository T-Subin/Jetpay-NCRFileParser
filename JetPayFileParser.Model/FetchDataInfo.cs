using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetPayFileParser.Model
{
    public class FetchDataInfo
    {
        public DataTable dataTable { get; set; }
        public List<string> lstIgnoreRow { get; set; }
        public List<string> lstErrorMessage { get; set; }
    }
}
