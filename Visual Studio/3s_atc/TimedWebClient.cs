using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace _3s_atc
{
    public class TimedWebClient : WebClient
    {
        public int Timeout { get; set; }

        public TimedWebClient()
        {
            this.Timeout = 30000;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var objWebRequest = base.GetWebRequest(address);
            objWebRequest.Timeout = this.Timeout;
            return objWebRequest;
        }
    }
}
