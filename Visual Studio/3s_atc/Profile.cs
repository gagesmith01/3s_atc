using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3s_atc
{
    [Serializable()]
    public class Profile
    {
        public string Email { set; get; }
        public string Password { set; get; }
        public string ProductID { set; get; }
        public List<double> Sizes { set; get; }
        public string Sitekey { set; get; }
        public string ClientID { set; get; }
        public string Duplicate { set; get; }
        public Dictionary<string, string> ExtraCookies { set; get; }
        public List<C_Cookie> Cookies { set; get; }
        public string SplashUrl { set; get; }
        public bool captcha { set; get; }
        public bool clientid { set; get; }
        public bool duplicate { set; get; }
        public int splashmode { set; get; }
        public bool loggedin { set; get; }
        public int index { set; get; }
        public bool running { set; get; }

        public Profile()
        {
            Sizes = new List<double>();
            ExtraCookies = new Dictionary<string, string>();
            Cookies = new List<C_Cookie>();
        }
    }
}
