using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

namespace _3s_atc
{
    public partial class Form_Browser : Form
    {
        ChromiumWebBrowser _browser;

        public Form_Browser(string url, string title, C_Proxy proxy)
        {
            if (!Cef.IsInitialized)
                Cef.Initialize(new CefSettings() { UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36" });

            _browser = new ChromiumWebBrowser(url);
            this.Controls.Add(_browser);
            _browser.Dock = DockStyle.Fill;

            InitializeComponent();

            this.Text = title;

            this.Show();
            this.Hide();
            this.ShowInTaskbar = false;
        }

        public bool DocumentReady(TimeSpan timeSpan)
        {
            Task<bool> task = Task<bool>.Factory.StartNew(() =>
                 {
                     bool success = false;
                     int elapsed = 0;

                     while ((!success) && (elapsed < timeSpan.TotalMilliseconds))
                     {
                         System.Threading.Thread.Sleep(1000);
                         elapsed += 1000;

                         this.Invoke((MethodInvoker)delegate
                         {
                             try
                             {
                                 string readyState = EvaluateScriptString("(function() {return document.readyState; })();", TimeSpan.FromSeconds(60)).GetAwaiter().GetResult();
                                 if (!String.IsNullOrEmpty(readyState) && readyState == "complete")
                                     success = true;
                                 else
                                     success = false;
                             }
                             catch(Exception ex)
                             {
                                 success = false;
                             }
                         });
                     }

                     return success;
                 });

            return task.Result;
        }

        public bool LoggedIn()
        {
            bool success = false;
            int elapsed = 0;
            while ((!success) && (elapsed < TimeSpan.FromSeconds(60).TotalMilliseconds))
            {
                System.Threading.Thread.Sleep(1000);
                elapsed += 1000;

                if (Convert.ToBoolean(getElementByClassName("accountwelcome")) == true && _browser.Address.ToLower().Contains("myaccount-show"))
                    success = true;
            }

            return success;
        }

        public object getElementById(string id, string attribute=null, string value = null, bool click = false, bool checkifready = false)
        {
        Task<object> task = Task<object>.Factory.StartNew(() =>
         {
             object element = null;
             int elapsed = 0;
             while ((element == null) && (elapsed < TimeSpan.FromSeconds(30).TotalMilliseconds))
             {
                 if (checkifready)
                 {
                     if (!DocumentReady(TimeSpan.FromSeconds(30)))
                         continue;
                 }

                 System.Threading.Thread.Sleep(1000);
                 elapsed += 1000;

                 this.Invoke((MethodInvoker)delegate
                 {
                     if (attribute == null)
                         element = EvaluateScriptBool("(function() {var style = window.getComputedStyle(document.getElementById('" + id + "')); return (style.display !== 'none') })();", TimeSpan.FromSeconds(60)).GetAwaiter().GetResult();
                     else
                         element = EvaluateScriptString("(function() { return document.getElementById('" + id + "').getAttribute(" + attribute + ");})();", TimeSpan.FromSeconds(60)).GetAwaiter().GetResult();


                     if (value != null)
                         _browser.ExecuteScriptAsync("document.getElementById('" + id + "').value = '" + value + "'");

                     if (click)
                         _browser.ExecuteScriptAsync("document.getElementById('" + id + "').click()");
                 });
             }

             return element;
         });

            return task.Result;
        }

        public object getElementByClassName(string classname, bool html=false)
        {
            Task<object> task = Task<object>.Factory.StartNew(() =>
            {
                object element = null;
                int elapsed = 0;

                while ((element == null) && (elapsed < TimeSpan.FromSeconds(30).TotalMilliseconds))
                {
                        System.Threading.Thread.Sleep(1000);
                        elapsed += 1000;
                        this.Invoke((MethodInvoker)delegate
                        {   if (html)
                                element = EvaluateScriptString("(function() { return document.getElementsByClassName('" + classname + "')[0].innerHTML;})();", TimeSpan.FromSeconds(60)).GetAwaiter().GetResult();
                            else
                                element = EvaluateScriptBool("(function() {var style = window.getComputedStyle(document.getElementsByClassName('" + classname + "')[0]); return (style.display !== 'none') })();", TimeSpan.FromSeconds(60)).GetAwaiter().GetResult();
                        });
                }

                return element;
            });

            return task.Result;
        }
        private async Task<bool> EvaluateScriptBool(string script, TimeSpan timeout)
        {
            bool result = false;
            if (_browser.IsBrowserInitialized && !_browser.IsDisposed && !_browser.Disposing)
            {
                try
                {
                    var task = _browser.EvaluateScriptAsync(script, timeout);
                    await task.ContinueWith(res =>
                    {
                        if (!res.IsFaulted)
                        {
                            result = Convert.ToBoolean(res.Result.Result);
                        }
                    }).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.InnerException.Message);
                }
            }
            return result;
        }

        private async Task<string> EvaluateScriptString(string script, TimeSpan timeout)
        {
            string result = null;
            if (_browser.IsBrowserInitialized && !_browser.IsDisposed && !_browser.Disposing)
            {
                try
                {
                    var task = _browser.EvaluateScriptAsync(script, timeout);
                    await task.ContinueWith(res =>
                    {
                        if (!res.IsFaulted)
                        {
                            result = res.Result.Result.ToString();
                        }
                    }).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.InnerException.Message);
                }
            }
            return result;
        }

        public void Dspose()
        {
            this.Invoke((MethodInvoker)delegate
            {
                this.Dispose();
            });
        }

        public List<Cookie> getCookies()
        {
            List<Cookie> cookies = new List<Cookie>();

            var visitor = new CookieMonster();

            if (Cef.GetGlobalCookieManager().VisitAllCookies(visitor))
                visitor.WaitForAllCookies();

            foreach (Cookie c in visitor.Cookies)
                cookies.Add(c);

            return cookies;
        }

        class CookieMonster : ICookieVisitor
        {
            readonly List<Cookie> cookies = new List<Cookie>();
            readonly System.Threading.ManualResetEvent gotAllCookies = new System.Threading.ManualResetEvent(false);

            public bool Visit(Cookie cookie, int count, int total, ref bool deleteCookie)
            {
                cookies.Add(cookie);

                if (count == total - 1)
                    gotAllCookies.Set();

                return true;
            }

            public void WaitForAllCookies()
            {
                gotAllCookies.WaitOne();
            }

            public IEnumerable<Cookie> Cookies
            {
                get { return cookies; }
            }

            public void Dispose() { }
        }
    }
}
