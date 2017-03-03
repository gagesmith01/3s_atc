using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace _3s_atc
{
    public class C_Captcha
    {
        public string sitekey { get; set; }
        public string response { get; set; }
        public DateTime expiration { get; set; }
        public int profileID { get; set; }

        public C_Captcha()
        {
            this.expiration = DateTime.Now.AddMinutes(2);
        }
    }
}
