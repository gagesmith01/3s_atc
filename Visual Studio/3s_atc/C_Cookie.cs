using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3s_atc
{
    [Serializable()]
    public class C_Cookie
    {
        public string name { get; set; }
        public string value { get; set; }
        public string domain { get; set; }
        public DateTime? expiry { get; set; }
    }
}
