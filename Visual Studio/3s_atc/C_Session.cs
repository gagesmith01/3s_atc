using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3s_atc
{
    public class C_Session
    {
        public bool passed { get; set; }
        public string hmac { get; set; }
        public string sitekey { get; set; }
        public string clientid { get; set; }
        public string duplicate { get; set; }
        public bool refresh { get; set; }
        public List<C_Cookie> cookies;
        public DateTime? hmac_expiration { get; set; }
        public string source { get; set; }

        public C_Session()
        {
            this.passed = false;
            this.cookies = new List<C_Cookie>();
        }
    }
}
