using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Web;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.Events;
using OpenQA.Selenium.Support.UI;

namespace _3s_atc
{
    public class Helpers
    {
        public List<Profile> profiles;
        public List<C_Captcha> captchas;
        public List<C_Proxy> proxylist;
        private Dictionary<string, string> marketsList;
        private bool proxy_running, sessions_running;
        public List<string> loggingin_emails;
        public List<C_Session> sessionlist;

        public Helpers()
        {
            proxy_running = false;
            profiles = new List<Profile>(); 
            captchas = new List<C_Captcha>(); 
            proxylist = new List<C_Proxy>();
            loggingin_emails = new List<string>();
            sessionlist = new List<C_Session>();

            marketsList = new Dictionary<string, string>(); marketsList["AE"] = "en_AE"; marketsList["AR"] = "es_AR"; marketsList["AT"] = "de_AT"; marketsList["AU"] = "en_AU"; marketsList["BE"] = "fr_BE"; marketsList["BH"] = "en_BH"; marketsList["BR"] = "pt_BR"; marketsList["CA"] = "en_CA"; marketsList["CF"] = "fr_CA"; marketsList["CH"] = "de_CH"; marketsList["CL"] = "es_CL"; marketsList["CN"] = "zh_CN"; marketsList["CO"] = "es_CO"; marketsList["CZ"] = "cz_CZ"; marketsList["DE"] = "de_DE"; marketsList["DK"] = "da_DK"; marketsList["EE"] = "et_EE"; marketsList["ES"] = "es_ES"; marketsList["FI"] = "fi_FI"; marketsList["FR"] = "fr_FR"; marketsList["GB"] = "en_GB"; marketsList["GR"] = "en_GR"; marketsList["HK"] = "zh_HK"; marketsList["HU"] = "hu_HU"; marketsList["ID"] = "id_ID"; marketsList["IE"] = "en_IE"; marketsList["IN"] = "en_IN"; marketsList["IT"] = "it_IT"; marketsList["JP"] = "ja_JP"; marketsList["KR"] = "ko_KR"; marketsList["KW"] = "ar_KW"; marketsList["MX"] = "es_MX"; marketsList["MY"] = "en_MY"; marketsList["NG"] = "en_NG"; marketsList["NL"] = "nl_NL"; marketsList["NO"] = "no_NO"; marketsList["NZ"] = "en_NZ"; marketsList["OM"] = "en_OM"; marketsList["PE"] = "es_PE"; marketsList["PH"] = "en_PH"; marketsList["PL"] = "pl_PL"; marketsList["PT"] = "en_PT"; marketsList["QA"] = "en_QA"; marketsList["RU"] = "ru_RU"; marketsList["SA"] = "en_SA"; marketsList["SE"] = "sv_SE"; marketsList["SG"] = "en_SG"; marketsList["SK"] = "sk_SK"; marketsList["TH"] = "th_TH"; marketsList["TR"] = "tr_TR"; marketsList["TW"] = "zh_TW"; marketsList["US"] = "en_US"; marketsList["VE"] = "es_VE"; marketsList["VN"] = "vi_VN"; marketsList["ZA"] = "en_ZA";
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            foreach (var process in System.Diagnostics.Process.GetProcessesByName("phantomjs"))
                process.Kill();
        }

        private bool ReadCookie(string hostName, string cookieName, ref string value)
        {
            string path = Path.Combine(Properties.Settings.Default.www_path, "recaptcha_response.txt");

            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                File.Delete(path);

                if (lines.Length > 0)
                {
                    int expire_time = Convert.ToInt32(lines[0]);
                    int time = Convert.ToInt32(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);

                    if (expire_time > time && captchas.FirstOrDefault(s => s.response == lines[1]) == null)
                    {
                        value = lines[1];
                        return true;
                    }
                }
            }

            return false;
        }

        public void getCaptcha(Profile profile)
        {
            if (String.IsNullOrEmpty(Properties.Settings.Default.www_path))
            {
                MessageBox.Show("www/htdocs folder not found, update your settings.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Process.Start("http://dev.adidas.com/sitekey.php?key=" + profile.Sitekey);


            string captcha_response = null;

            while (!ReadCookie("dev.adidas.com", "g-recaptcha-response", ref captcha_response))
                System.Threading.Thread.Sleep(1000);

            captchas.Add(new C_Captcha { sitekey = profile.Sitekey, response = captcha_response, profileID = profile.index });
        }

        private string webRequestPost(Profile profile, string url, Dictionary<string, string> post, C_Proxy proxy=null, C_Session session=null)
        {
            string postData = "";

            foreach (string key in post.Keys)
            {
                postData += HttpUtility.UrlEncode(key) + "="
                      + HttpUtility.UrlEncode(post[key]) + "&";
            }

            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";

            if (profile.splashmode > 0)
                webRequest.Referer = profile.SplashUrl;
            else
                webRequest.Referer = String.Format("http://www.{0}/", Properties.Settings.Default.locale);

            if (proxy != null)
            {
                WebProxy webproxy = new WebProxy(proxy.address);

                if(proxy.auth)
                    webproxy.Credentials = new NetworkCredential(proxy.username, proxy.password);

                webRequest.Proxy = webproxy;
            }

            byte[] data = Encoding.ASCII.GetBytes(postData);

            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = data.Length;

            CookieContainer cookies = new CookieContainer();

            List<C_Cookie> c_cookies;

            if (profiles.FirstOrDefault(x => x.Email == profile.Email && x.loggedin) != null)
                c_cookies = profiles.FirstOrDefault(x => x.Email == profile.Email).Cookies;
            else
                c_cookies = profile.Cookies;

            foreach (C_Cookie cookie in c_cookies)
                cookies.Add(new System.Net.Cookie(cookie.name, cookie.value) { Domain = cookie.domain });

            if(proxy != null)
            {
                foreach (C_Cookie cookie in proxy.cookies)
                    cookies.Add(new System.Net.Cookie(cookie.name, cookie.value) { Domain = cookie.domain });
            } else if(session != null)
            {
                foreach (C_Cookie cookie in session.cookies)
                    cookies.Add(new System.Net.Cookie(cookie.name, cookie.value) { Domain = cookie.domain });
            }

            webRequest.CookieContainer = cookies;

            Stream requestStream = webRequest.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();

            HttpWebResponse myHttpWebResponse = (HttpWebResponse)webRequest.GetResponse();
            
            Stream responseStream = myHttpWebResponse.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);

            string pageContent = myStreamReader.ReadToEnd();

            myStreamReader.Close();
            responseStream.Close();
            myHttpWebResponse.Close();

            return pageContent;
        }

        private string getDuplicate(string source, string ogurl)
        {
            string[] lines = source.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string script_line = lines.FirstOrDefault(s => s.Contains("application.js"));

            if (script_line != null)
            {
                script_line = script_line.Split('"')[1];

                if (!script_line.Contains("adidas."))
                    script_line = ogurl + "/" + script_line;
                else if (script_line.StartsWith("//"))
                    script_line = "http://" + script_line.Remove(0, 2);

                string js = webRequest(script_line);

                if (js != null)
                {
                    string[] js_source = js.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

                    js = js_source.FirstOrDefault(s => s.Contains("$('#flashproductform').append"));

                    string duplicate = js.Split(new string[] { "name=\"" }, StringSplitOptions.None)[1].Split('"')[0];
                    return duplicate;
                }
            }

            return null;

        }

        public void WaitForPageLoad(IWebDriver _driver, int timeout) 
        {
            string state = string.Empty;
            try 
            {
                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeout));

                wait.Until(d => {
                    try { state = ((IJavaScriptExecutor) _driver).ExecuteScript(@"return document.readyState").ToString(); }
                    catch (InvalidOperationException) { } 
                    catch (NoSuchWindowException) { _driver.SwitchTo().Window(_driver.WindowHandles.Last()); }
                    return (state.Equals("complete", StringComparison.InvariantCultureIgnoreCase));		
                });
            } 
            catch 
            {
                throw;
            }
        }
        private void refreshDriver(IWebDriver _driver)
        {
            System.Threading.Thread.Sleep(10000);
            if (_driver.FindElements(By.ClassName("g-recaptcha")).Count == 0)
            {
                _driver.Manage().Cookies.DeleteAllCookies();
                _driver.Navigate().Refresh();
                WaitForPageLoad(_driver, 60);
            }
        }
        private void proxyRun(Profile profile, C_Proxy proxy, int i, DataGridViewRowCollection rows)
        {
            rows[i].Cells[8].Value = "Setting up...";

            IWebDriver _driver;
            _driver = createNewJSDriver(proxy);
            _driver.Navigate().GoToUrl(profile.SplashUrl);

            if (ElementDisplayed(_driver, /*By.CssSelector(".message.message-1.hidden")*/By.ClassName("sk-fading-circle"), 120))
            {
                rows[i].Cells[8].Value = "On splash page...";

                while (_driver.FindElements(By.ClassName("g-recaptcha")).Count == 0)
                {
                    if (proxy.refresh)
                        refreshDriver(_driver);
                    else
                        System.Threading.Thread.Sleep(1000);
                }

                rows[i].Cells[8].Value = "Splash page passed!";
                rows[i].Cells[8].Style = new DataGridViewCellStyle { ForeColor = System.Drawing.Color.Green };

                string cookie_name = _driver.Manage().Cookies.AllCookies.FirstOrDefault(s => s.Value.Contains("hmac")).Name;
                if (cookie_name != null)
                {
                    proxy.hmac = _driver.Manage().Cookies.AllCookies.FirstOrDefault(s => s.Value.Contains("hmac")).Value;
                    proxy.hmac_expiration = _driver.Manage().Cookies.AllCookies.FirstOrDefault(s => s.Value.Contains("hmac")).Expiry;
                    rows[i].Cells[4].Value = proxy.hmac;
                } if (_driver.FindElements(By.ClassName("g-recaptcha")).Count > 0)
                {
                    proxy.sitekey = _driver.FindElement(By.ClassName("g-recaptcha")).GetAttribute("data-sitekey");
                    rows[i].Cells[5].Value = proxy.sitekey;
                } if (_driver.FindElements(By.Id("flashproductform")).Count > 0)
                {
                    proxy.clientid = _driver.FindElement(By.Id("flashproductform")).GetAttribute("action").Split(new string[] { "clientId=" }, StringSplitOptions.None)[1];
                    rows[i].Cells[6].Value = proxy.clientid;
                } if (_driver.FindElements(By.XPath("//link[@rel='canonical']")).Count > 0)
                {
                    proxy.duplicate = getDuplicate(_driver.PageSource, _driver.FindElement(By.XPath("//link[@rel='canonical']")).GetAttribute("href"));
                    rows[i].Cells[7].Value = proxy.duplicate;
                }

                rows[i].Cells[8].Value = "HMAC and Sitekey retrieved!";
                foreach (OpenQA.Selenium.Cookie cookie in _driver.Manage().Cookies.AllCookies)
                {
                    if (cookie.Domain.Contains("adidas"))
                        proxy.cookies.Add(new C_Cookie { name = cookie.Name, value = cookie.Value, domain = cookie.Domain, expiry = cookie.Expiry });
                }

                proxy.passed = true;

                if(proxy.auth)
                    File.AppendAllText("logs.txt", String.Format("Proxy --- Address : {0} --- Username: {1} --- Password: {2} / Cookie --- Name: {3} --- Value : {4} / Sitekey: {5} / Client ID : {6} / Duplicate : {7}", proxy.address, proxy.username, proxy.password, cookie_name, proxy.hmac, proxy.sitekey, proxy.clientid, proxy.duplicate) + Environment.NewLine);
                else
                    File.AppendAllText("logs.txt", String.Format("Proxy --- Address : {0} / Cookie --- Name: {1} --- Value : {2} / Sitekey: {3} / Client ID : {4} / Duplicate : {5}", proxy.address, cookie_name, proxy.hmac, proxy.sitekey, proxy.clientid, proxy.duplicate) + Environment.NewLine);
                
                File.WriteAllText(String.Format("{0}\\{1}_productpage_source.txt", AppDomain.CurrentDomain.BaseDirectory, profile.ProductID), _driver.PageSource);
                _driver.Quit();
            }
            else
            {
                rows[i].Cells[8].Value = "Error!";
                rows[i].Cells[8].Style = new DataGridViewCellStyle { ForeColor = System.Drawing.Color.Red };

                _driver.Quit();
            }
        }
        private void runProxyList(Profile profile,DataGridViewRowCollection rows)
        {
            if (proxylist.Count == 0){
                MessageBox.Show(String.Format("{0} - {1} : splash page mode needs at least one proxy.", profile.Email, profile.ProductID), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;}

            proxy_running = true;

            for (int i = 0; i < proxylist.Count; i++)
            {
                int index = i;
                Task.Run(() => proxyRun(profile, proxylist[index], index, rows));
            }
            
        }

        public void transferSession(C_Session session)
        {
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

            IWebDriver _driver = new ChromeDriver(driverService);
            _driver.Navigate().GoToUrl("https://www.google.com/"); // navigate to google so captcha solving isn't slow when passed the splash
            WaitForPageLoad(_driver, 15);

            Profile profile = profiles.FirstOrDefault(x => !String.IsNullOrEmpty(x.SplashUrl));
            _driver.Navigate().GoToUrl(profile.SplashUrl);
            WaitForPageLoad(_driver, 30);

            foreach(C_Cookie cookie in session.cookies)
                _driver.Manage().Cookies.AddCookie(new OpenQA.Selenium.Cookie(cookie.name, cookie.value, cookie.domain, "/", cookie.expiry));

            IJavaScriptExecutor js = _driver as IJavaScriptExecutor;
            js.ExecuteScript(String.Format("var source = '{0}';document.write(source);document.close();", session.source.Replace(System.Environment.NewLine, "").Replace("'", "\"").Replace("<script>", "<scr' + 'ipt>").Replace("<script ", "<scr' + 'ipt ").Replace("</script>", "</scr' + 'ipt>")));
        }

        private void runSessionList(Profile profile, DataGridViewRowCollection rows)
        {
            if (sessionlist.Count == 0)
            {
                MessageBox.Show(String.Format("{0} - {1} : splash page mode needs at least one session.", profile.Email, profile.ProductID), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            sessions_running = true;

            for (int i = 0; i < sessionlist.Count; i++)
            {
                int index = i;
                Task.Run(() => runSession(profile, sessionlist[index], rows[index]));
            }

        }

        private void runSession(Profile profile, C_Session session, DataGridViewRow row)
        {
            row.Cells[8].Value = "Setting up...";
            
            IWebDriver _driver = createNewJSDriver();
            _driver.Navigate().GoToUrl(profile.SplashUrl);

            if (ElementDisplayed(_driver, /*By.CssSelector(".message.message-1.hidden")*/By.ClassName("sk-fading-circle"), 120))
            {
                row.Cells[8].Value = "On splash page...";

                while (_driver.FindElements(By.ClassName("g-recaptcha")).Count == 0)
                {
                    if (session.refresh)
                        refreshDriver(_driver);
                    else
                        System.Threading.Thread.Sleep(2000);
                }

                row.Cells[8].Value = "Splash page passed!";
                row.Cells[8].Style = new DataGridViewCellStyle { ForeColor = System.Drawing.Color.Green };

                session.source = _driver.PageSource;

                string cookie_name = null;
                if (_driver.Manage().Cookies.AllCookies.FirstOrDefault(s => s.Value.Contains("hmac")) != null)
                {
                    cookie_name = _driver.Manage().Cookies.AllCookies.FirstOrDefault(s => s.Value.Contains("hmac")).Name;
                    session.hmac = _driver.Manage().Cookies.AllCookies.FirstOrDefault(s => s.Value.Contains("hmac")).Value;
                    session.hmac_expiration = _driver.Manage().Cookies.AllCookies.FirstOrDefault(s => s.Value.Contains("hmac")).Expiry;
                    row.Cells[4].Value = session.hmac;
                } if (_driver.FindElements(By.ClassName("g-recaptcha")).Count > 0)
                {
                    session.sitekey = _driver.FindElement(By.ClassName("g-recaptcha")).GetAttribute("data-sitekey");
                    row.Cells[5].Value = session.sitekey;
                } if (_driver.FindElements(By.Id("flashproductform")).Count > 0)
                {
                    session.clientid = _driver.FindElement(By.Id("flashproductform")).GetAttribute("action").Split(new string[] { "clientId=" }, StringSplitOptions.None)[1];
                    row.Cells[6].Value = session.clientid;
                } if (_driver.FindElements(By.XPath("//link[@rel='canonical']")).Count > 0)
                {
                    session.duplicate = getDuplicate(_driver.PageSource, _driver.FindElement(By.XPath("//link[@rel='canonical']")).GetAttribute("href"));
                    row.Cells[7].Value = session.duplicate;
                }

                row.Cells[8].Value = "HMAC and Sitekey retrieved!";
                foreach (OpenQA.Selenium.Cookie cookie in _driver.Manage().Cookies.AllCookies)
                {
                    if (cookie.Domain.Contains("adidas"))
                        session.cookies.Add(new C_Cookie { name = cookie.Name, value = cookie.Value, domain = cookie.Domain, expiry = cookie.Expiry });
                }

                session.passed = true;

                File.AppendAllText("logs.txt", String.Format("Session / Cookie --- Name: {0} --- Value : {1} / Sitekey: {2} / Client ID : {3} / Duplicate : {4}", cookie_name, session.hmac, session.sitekey, session.clientid, session.duplicate) + Environment.NewLine);

                File.WriteAllText(String.Format("{0}\\{1}_productpage_source.txt", AppDomain.CurrentDomain.BaseDirectory, profile.ProductID), _driver.PageSource);
                _driver.Quit();
            }
            else
            {
                row.Cells[8].Value = "Error!";
                row.Cells[8].Style = new DataGridViewCellStyle { ForeColor = System.Drawing.Color.Red };

                _driver.Quit();
            }
        }
        private string cartSplash(Profile profile, DataGridViewCell cell, DataGridViewRowCollection rows)
        {
            cell.Value = "Waiting for HMAC...(check settings page)";
            C_Proxy proxy = null;
            C_Session session = null;

            if (!proxy_running && profile.splashmode == 1)
            {
                Task.Factory.StartNew(() => runProxyList(profile, rows));

                while (proxylist.FirstOrDefault(s => proxy.passed) == null)
                    System.Threading.Thread.Sleep(1000);

                cell.Value = "GOT HMAC!";

                proxy = proxylist.FirstOrDefault(s => proxy.passed && s.hmac_expiration > DateTime.Now);
            }
            else if(!sessions_running && profile.splashmode == 2)
            {
                for (int i = 0; i < Properties.Settings.Default.sessions_count; i++)
                    sessionlist.Add(new C_Session { refresh = false });
                for (int i = 0; i < Properties.Settings.Default.r_sessions_count; i++)
                    sessionlist.Add(new C_Session { refresh = true });

                rows.Clear();

                foreach(C_Session s in this.sessionlist)
                    rows.Add( new string[] { "session", s.refresh.ToString(), "False", null, null, null, null, null, null });

                Task.Factory.StartNew(() => runSessionList(profile, rows));

                while (sessionlist.FirstOrDefault(x => x.passed) == null)
                    System.Threading.Thread.Sleep(1000);

                cell.Value = "GOT HMAC!";

                session = sessionlist.FirstOrDefault(s => s.passed && s.hmac_expiration > DateTime.Now);
            }

            return cartNoSplash(profile, cell, proxy, session);
        }

        public bool LoggedIn(IWebDriver _driver, int timeout)
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeout));
                wait.Until(x => x.Manage().Cookies.GetCookieNamed("username") != null && x.Url.ToLower().Contains("myaccount-show"));
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool ElementDisplayed(IWebDriver _driver, By by, int timeout)
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeout));
                var myElement = wait.Until(x => x.FindElement(by));
                return myElement.Displayed;
            }
            catch
            {
                return false;
            }
        }

        public IWebDriver createNewJSDriver(C_Proxy proxy=null)
        {
            IWebDriver _driver;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            driverService.LoadImages = false; //reduce ram usage

            if (proxy != null)
            {
                if (proxy.auth)
                {
                    driverService.ProxyType = "socks5";
                    driverService.ProxyAuthentication = String.Format("{0}:{1}", proxy.username, proxy.password);
                }
                else
                    driverService.ProxyType = "http";

                driverService.Proxy = proxy.address;
                driverService.IgnoreSslErrors = true;
            }

            var driverOptions = new PhantomJSOptions();
            driverOptions.AddAdditionalCapability("phantomjs.page.settings.userAgent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");

            _driver = new PhantomJSDriver(driverService, driverOptions);
            return _driver;
        }
        
        public bool login(Profile profile, DataGridViewCell cell, C_Proxy proxy)
        {
            if (!cell.Value.ToString().ToLower().Contains("login") && !profile.loggedin)
            {
                cell.Value = "Logging in...";

                while (loggingin_emails.Find(x => x == profile.Email) != null && profiles.FirstOrDefault(x => x.Email == profile.Email && x.loggedin) == null)
                    System.Threading.Thread.Sleep(1000);

                return Login(profile, cell, proxy);
            }

            return false;
        }
        private bool Login(Profile profile, DataGridViewCell cell, C_Proxy proxy)
        {
            if (profiles.FirstOrDefault(x => x.Email == profile.Email && x.loggedin) != null)
            {
                System.Threading.Thread.Sleep(100);
                cell.Value = "Logged in!";
                profile.loggedin = true;
                return true;
            }
            else
                loggingin_emails.Add(profile.Email);

            cell.Value = "Connecting to login page...";

            IWebDriver _driver = createNewJSDriver(proxy);
            _driver.Navigate().GoToUrl("https://cp." + Properties.Settings.Default.locale + "/web/eCom/" + marketsList[Properties.Settings.Default.code] + "/loadsignin?target=account");

            if (ElementDisplayed(_driver, By.Id("username"), 120))
            {
                //executing javascript is much faster than sending keys
                ((IJavaScriptExecutor)_driver).ExecuteScript(String.Format("document.getElementById('username').value='{0}'", profile.Email));
                ((IJavaScriptExecutor)_driver).ExecuteScript(String.Format("document.getElementById('password').value='{0}'", profile.Password));
                ((IJavaScriptExecutor)_driver).ExecuteScript("document.getElementById('rememberme').click()");
                ((IJavaScriptExecutor)_driver).ExecuteScript("document.getElementById('signinSubmit').click()");
                cell.Value = "Logging in...";
            }
            else
            {
                cell.Value = "Error while connecting to login page";
                cell.Style = new DataGridViewCellStyle { ForeColor = System.Drawing.Color.Red };
                _driver.Quit();
                return false;
            }


            if (LoggedIn(_driver, 90))
            {
                cell.Value = "Logged in!";
                System.Threading.Thread.Sleep(500);

                profile.loggedin = true;
                foreach (OpenQA.Selenium.Cookie cookie in _driver.Manage().Cookies.AllCookies)
                {
                    if (cookie.Domain.Contains("adidas"))
                        profile.Cookies.Add(new C_Cookie { name = cookie.Name, value = cookie.Value, domain = cookie.Domain, expiry = cookie.Expiry });
                }

                if(profiles.FirstOrDefault(x => x.Email == profile.Email && !x.loggedin) == null)
                    loggingin_emails.Remove(profile.Email);

                _driver.Quit();
                return true;
            }
            else
            {
                profile.loggedin = false;
                cell.Value = _driver.FindElement(By.CssSelector(".errorcommon.errorcommonshow")).Text;
                cell.Style = new DataGridViewCellStyle { ForeColor = System.Drawing.Color.Red };
                _driver.Quit();
                return false;
            }
        }

        private string getEUSize(double size)
        {
            Dictionary<double, string> eu_sizes = new Dictionary<double,string>();
            eu_sizes[4] = "36"; eu_sizes[4.5] = "36 2/3"; eu_sizes[5] = "37 1/3"; eu_sizes[5.5] = "38"; eu_sizes[6] = "38 2/3"; eu_sizes[6.5] = "39 1/3"; eu_sizes[7] = "40"; eu_sizes[7.5] = "40 2/3"; eu_sizes[8] = "41 1/3"; eu_sizes[8.5] = "42"; eu_sizes[9] = "42 2/3"; eu_sizes[9.5] = "43 1/3"; eu_sizes[10] = "44"; eu_sizes[10.5] = "44 2/3"; eu_sizes[11] = "45 1/3"; eu_sizes[11.5] = "46"; eu_sizes[12] = "46 2/3"; eu_sizes[12.5] = "47 1/3"; eu_sizes[13] = "48"; eu_sizes[13.5] = "48 2/3"; eu_sizes[14] = "49 1/3"; eu_sizes[14.5] = "50";
            return eu_sizes[size];
        }

        private string getUKSize(double size)
        {
            return (size - 0.5).ToString("0.#").Replace(',', '.');
        }
        private string getFirstAvailableSize(List<double> sizes, string pid, string cid, int forceCID)
        {
            Dictionary<string, Dictionary<string, string>> inventory;

            if(forceCID > 0)
                inventory = getClientInventory(pid, cid);
            else
                inventory = getInventory(pid, cid);


            if (inventory == null) return null;

            for (int i = 0; i < sizes.Count; i++)
            {
                int index = i;
                    
                if(Properties.Settings.Default.code == "GB")
                {
                    KeyValuePair<string, Dictionary<string, string>> entry_uk = inventory.FirstOrDefault(s => inventory[s.Key]["size"] == getUKSize(sizes[index]) && Convert.ToInt32(inventory[s.Key]["stockcount"]) > 0);
                    if(entry_uk.Key != null)
                        return entry_uk.Key;
                }
                else
                {
                    KeyValuePair<string, Dictionary<string, string>> entry_us = inventory.FirstOrDefault(s => inventory[s.Key]["size"] == sizes[index].ToString("0.#").Replace(',', '.') && Convert.ToInt32(inventory[s.Key]["stockcount"]) > 0);
                    KeyValuePair<string, Dictionary<string, string>> entry_eu = inventory.FirstOrDefault(s => inventory[s.Key]["size"] == getEUSize(sizes[index]) && Convert.ToInt32(inventory[s.Key]["stockcount"]) > 0);

                    if(entry_us.Key != null)
                        return entry_us.Key;
                    else if(entry_eu.Key != null)
                        return entry_eu.Key;
                }

                System.Threading.Thread.Sleep(100);
            }

            return null;
        }

        private string cartNoSplash(Profile profile, DataGridViewCell cell, C_Proxy proxy=null, C_Session session=null)
        {
            Dictionary<string, string> post = new Dictionary<string, string>();

            string atcURL;
            if(profile.clientid && !String.IsNullOrWhiteSpace(profile.ClientID))
                atcURL = "http://www." + Properties.Settings.Default.locale + "/on/demandware.store/Sites-adidas-" + Properties.Settings.Default.code + "-Site/" + marketsList[Properties.Settings.Default.code] + "/Cart-MiniAddProduct?clientId=" + profile.ClientID;
            else
                atcURL = "http://www." + Properties.Settings.Default.locale + "/on/demandware.store/Sites-adidas-" + Properties.Settings.Default.code + "-Site/" + marketsList[Properties.Settings.Default.code] + "/Cart-MiniAddProduct";

            string result = null;

            if (proxy != null)
            {
                profile.Sitekey = proxy.sitekey; profile.captcha = true;
                profile.ClientID = proxy.clientid; profile.clientid = true;
                profile.Duplicate = proxy.duplicate; profile.duplicate = true;
            }
            else if(session != null)
            {
                profile.Sitekey = session.sitekey; profile.captcha = true;
                profile.ClientID = session.clientid; profile.clientid = true;
                profile.Duplicate = session.duplicate; profile.duplicate = true;
            }

            if (!profile.loggedin)
                Task.Run(() => login(profile, cell, proxy));

            if (profile.loggedin)
            {
                if (profile.captcha && !String.IsNullOrWhiteSpace(profile.Sitekey))
                {
                    cell.Value = "SOLVE CAPTCHA!";
                    cell.Style = new DataGridViewCellStyle { ForeColor = Color.Red };

                    while (captchas.FirstOrDefault(s => s.sitekey.Contains(profile.Sitekey) && s.expiration > DateTime.Now) == null)
                            System.Threading.Thread.Sleep(1000);

                    C_Captcha captcha = captchas.FirstOrDefault(s => s.sitekey.Contains(profile.Sitekey) && s.expiration > DateTime.Now && s.profileID == profile.index && !String.IsNullOrEmpty(s.response));
                    post.Add("g-recaptcha-response", captcha.response);

                    if (profile.duplicate && !String.IsNullOrWhiteSpace(profile.Duplicate))
                        post.Add(profile.Duplicate, captcha.response);

                    captchas.Remove(captcha);
                }

                cell.Value = "Checking sizes...";
                cell.Style = new DataGridViewCellStyle { ForeColor = Color.Empty };

                string size = getFirstAvailableSize(profile.Sizes, profile.ProductID, profile.ClientID, profile.splashmode);

                if (size == null)
                    return "No sizes available";

                cell.Value = "Adding to cart...";
                post.Add("pid", size); post.Add("masterPid", profile.ProductID); post.Add("Quantity", "1"); post.Add("request", "ajax"); post.Add("responseformat", "json"); post.Add("sessionSelectedStoreID", "null"); post.Add("layer", "Add To Bag overlay");
                result = webRequestPost(profile, atcURL, post, proxy, session);
            }

            return result;
        }

        public string cart(Profile profile, DataGridViewCell cell, DataGridViewRowCollection rows = null)
        {
            if (profile.splashmode > 0 && !String.IsNullOrWhiteSpace(profile.SplashUrl))
                return cartSplash(profile, cell, rows);
            else
                return cartNoSplash(profile, cell);
        }

        public static string webRequest(string url)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "GET";
            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
            webRequest.ContentType = "application/json; charset=utf-8";
            webRequest.Timeout = 30000;

            string pageContent = string.Empty;

            using (var response = (HttpWebResponse)webRequest.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var streamReader = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        pageContent = streamReader.ReadToEnd();
                    }
                }
            }

            return pageContent;
        }

        public static Dictionary<string, dynamic> json_decode(string source)
        {
            var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            var dict = jss.Deserialize<Dictionary<string, dynamic>>(source);

            return dict;
        }

        private Dictionary<string, Dictionary<string, string>> getClientInventory(string pid, string clientID)
        {
            Dictionary<string, Dictionary<string, string>> products = new Dictionary<string, Dictionary<string, string>>();
            string responseString = string.Empty;
            string locale = Properties.Settings.Default.code;
            string url;

            if (locale == "US" || locale == "CA" || locale == "MX")
                url = String.Format("http://production-us-adidasgroup.demandware.net/s/adidas-{0}/dw/shop/v15_6/products/({1})?client_id={2}&expand=availability,variations,prices", locale, pid, clientID);
            else if (locale == "PT")
                url = String.Format("http://production-store-adidasgroup.demandware.net/s/adidas-MLT/dw/shop/v15_6/products/({0})?client_id={1}&expand=availability,variations,prices", pid, clientID);
            else
                url = String.Format("http://production.store.adidasgroup.demandware.net/s/adidas-{0}/dw/shop/v15_6/products/({1})?client_id={2}&expand=availability,variations,prices", locale, pid, clientID);
            
            using(var client = new TimedWebClient())
            {
                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
                responseString = client.DownloadString(url);
            }

            Dictionary<string, dynamic> json = json_decode(responseString);
            for(int i = 0; i < json["data"][0]["variants"].Count; i++)
            {
                int index = i;
                string id = json["data"][0]["variants"][index]["product_id"];

                products[id] = new Dictionary<string, string>();

                using (var client = new TimedWebClient())
                {
                    client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
                    responseString = client.DownloadString(url.Replace(pid, id));
                }

                json = json_decode(responseString);

                products[id]["size"] = json["data"][0]["c_sizeSearchValue"];
                products[id]["stockcount"] = Convert.ToInt32(json["data"][0]["inventory"]["ats"]).ToString();
            }

            return products;
        }
        public Dictionary<string, Dictionary<string, string>> getInventory(string pid, string clientID, string splash_url=null)
        {
            string url = String.Format("http://www.{0}/on/demandware.store/Sites-adidas-{1}-Site/{2}/Product-GetVariants?pid={3}", Properties.Settings.Default.locale, Properties.Settings.Default.code, marketsList[Properties.Settings.Default.code], pid);

            string responseString = string.Empty;

            using (var client = new TimedWebClient())
            {
                try
                {
                    client.Headers[HttpRequestHeader.Accept] = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                    client.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";

                    if (!String.IsNullOrEmpty(splash_url))
                        client.Headers[HttpRequestHeader.Referer] = splash_url;
                    else
                        client.Headers[HttpRequestHeader.Referer] = String.Format("http://www.{0}/", Properties.Settings.Default.locale);

                    responseString = client.DownloadString(url);
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null || ex.Status == WebExceptionStatus.Timeout)
                    {
                        var resp = (HttpWebResponse)ex.Response;
                        if (String.IsNullOrEmpty(clientID))
                        {
                            MessageBox.Show(String.Format("WebRequest error({0}): Could not get variant stock for product {1}, please specify a valid client ID to get client stock and try again.", resp.StatusCode, pid), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }
                        else
                            return getClientInventory(pid, clientID);
                    }
                }
            }

            if(responseString.Contains("<html"))
            {
                if (String.IsNullOrEmpty(clientID))
                {
                    MessageBox.Show("Could not get product variant stock, please specify a valid client ID to get client stock and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                else
                    return getClientInventory(pid, clientID);
            }

            Dictionary<string, Dictionary<string, string>> products = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, dynamic> json = json_decode(responseString);

            for (int i = 0; i < json["variations"]["variants"].Count; i++)
            {
                int index = i;
                string stylecode = json["variations"]["variants"][index]["id"];
                products[stylecode] = new Dictionary<string, string>();

                products[stylecode]["size"] = json["variations"]["variants"][index]["attributes"]["size"];
                products[stylecode]["stockcount"] = Convert.ToInt32(json["variations"]["variants"][index]["ATS"]).ToString();
            }

            return products;
        }

        public string getCookie(string cookie_name, string[] cookies)
        {
            foreach (string cookie in cookies)
            {
                string name = cookie.Split('=')[0];
                if (name.Contains(cookie_name))
                    return cookie.Substring(name.Length + 1);
            }

            return null;
        }

        public double getUSSize(string size)
        {
            string[] tokens = size.Split(new char[0]);
            double US_Size = double.Parse(tokens[1], System.Globalization.CultureInfo.InvariantCulture);

            return US_Size;
        }

        public int getAdidasSize(string size)
        {
            double US_Size = getUSSize(size);

            double Size = ((US_Size - 6.5) * 20) + 580;
            int rawSize = Convert.ToInt32(Size);

            return rawSize;
        }

        public Dictionary<string,string> splitCookies(string cookie)
        {
            Dictionary<string, string> cookies = new Dictionary<string, string>();

            if (!String.IsNullOrWhiteSpace(cookie))
            {
                string[] tokens = cookie.Split(';');

                foreach (string token in tokens)
                {
                    string[] tok = token.Split('=');
                    if(!String.IsNullOrEmpty(tok[0]))
                        cookies[tok[0]] = tok[1];
                }
            }

            return cookies;
        }

        public void SaveProfiles()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bf.Serialize(ms, profiles);
                ms.Position = 0;
                byte[] buffer = new byte[(int)ms.Length];
                ms.Read(buffer, 0, buffer.Length);
                Properties.Settings.Default.profiles = Convert.ToBase64String(buffer);
                Properties.Settings.Default.Save();
            }
        }

        public void SaveProxyList()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bf.Serialize(ms, proxylist);
                ms.Position = 0;
                byte[] buffer = new byte[(int)ms.Length];
                ms.Read(buffer, 0, buffer.Length);
                Properties.Settings.Default.proxylist = Convert.ToBase64String(buffer);
                Properties.Settings.Default.Save();
            }
        }

        public List<Profile> LoadProfiles()
        {
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(Properties.Settings.Default.profiles)))
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (List<Profile>)bf.Deserialize(ms);
            }
        }

        public List<C_Proxy> LoadProxyList()
        {
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(Properties.Settings.Default.proxylist)))
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (List<C_Proxy>)bf.Deserialize(ms);
            }
        }
    }
}
