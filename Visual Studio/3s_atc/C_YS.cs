using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3s_atc
{
    public class C_YS
    {
        public int index { get; set; }
        public string size { get; set; }
        public int pid { get; set; }
        public bool browser_visible { get; set; }

        public C_YS()
        {
            this.browser_visible = true;
        }

        public void hideShow()
        {
            System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(this.pid);

            if (!process.HasExited)
            {
                if (!this.browser_visible)
                {
                    IntPtr HWND = FindWindow(null, "3s_atc browser - " + System.Diagnostics.Process.GetCurrentProcess().Id.ToString() + "_ys_" + index);
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
