using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFinderApi.Models.RequestModels
{
    public class CodeVerify
    {
        public string smsCode { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
    }
}
