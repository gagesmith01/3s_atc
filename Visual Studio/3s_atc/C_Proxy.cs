using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3s_atc
{
    [Serializable()]
    public class C_Proxy
    {
        public string address { get; set; }
        public bool auth { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public bool passed { get; set; }
        public string hmac { get; set; }
        public string sitekey { get; set; }
        public string clientid { get; set; }
        public string duplicate { get; set; }
        public bool refresh { get; set; }
        public List<C_Cookie> cookies;
        public DateTime? hmac_expiration { get; set; }
        public OpenQA.Selenium.IWebDriver driver { get; set; }


        public C_Proxy()
        {
            this.passed = false;
            this.cookies = new List<C_Cookie>();
        }
    }
}
