using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3s_atc
{
    [Serializable()]
    public class C_Session
    {
        public int index { get; set; }
        public bool passed { get; set; }
        public C_Cookie hmac_cookie { get; set; }
        public string sitekey { get; set; }
        public string clientid { get; set; }
        public string duplicate { get; set; }
        public bool refresh { get; set; }
        public List<C_Cookie> cookies;
        public string source { get; set; }
        public int pid { get; set; }
        public bool browser_visible { get; set; }

        public C_Session()
        {
            this.passed = false;
            this.cookies = new List<C_Cookie>();
            this.browser_visible = true;
        }

        public void hideShow()
        {
            System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(this.pid);

            if (!process.HasExited)
            {
                if (!this.browser_visible)
                {
                    IntPtr HWND = FindWindow(null, "3s_atc browser - session_" + index);
                    this.browser_visible = true;
                    ShowWindow(HWND, 5);
                }
                else if (this.browser_visible)
                {
                    this.browser_visible = false;
                    ShowWindow(process.MainWindowHandle, 0);
                }
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }
}