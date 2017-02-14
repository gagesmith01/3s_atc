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
        public Dictionary<string, string> marketsList;
        public List<C_Proxy> passedSplash;
        private bool proxy_running;

        public Helpers()
        {
            proxy_running = false;
            profiles = new List<Profile>(); 
            captchas = new List<C_Captcha>(); 
            proxylist = new List<C_Proxy>(); passedSplash = new List<C_Proxy>();
            marketsList = new Dictionary<string, string>(); marketsList["AE"] = "en_AE"; marketsList["AR"] = "es_AR"; marketsList["AT"] = "de_AT"; marketsList["AU"] = "en_AU"; marketsList["BE"] = "fr_BE"; marketsList["BH"] = "en_BH"; marketsList["BR"] = "pt_BR"; marketsList["CA"] = "en_CA"; marketsList["CF"] = "fr_CA"; marketsList["CH"] = "de_CH"; marketsList["CL"] = "es_CL"; marketsList["CN"] = "zh_CN"; marketsList["CO"] = "es_CO"; marketsList["CZ"] = "cz_CZ"; marketsList["DE"] = "de_DE"; marketsList["DK"] = "da_DK"; marketsList["EE"] = "et_EE"; marketsList["ES"] = "es_ES"; marketsList["FI"] = "fi_FI"; marketsList["FR"] = "fr_FR"; marketsList["GB"] = "en_GB"; marketsList["GR"] = "en_GR"; marketsList["HK"] = "zh_HK"; marketsList["HU"] = "hu_HU"; marketsList["ID"] = "id_ID"; marketsList["IE"] = "en_IE"; marketsList["IN"] = "en_IN"; marketsList["IT"] = "it_IT"; marketsList["JP"] = "ja_JP"; marketsList["KR"] = "ko_KR"; marketsList["KW"] = "ar_KW"; marketsList["MX"] = "es_MX"; marketsList["MY"] = "en_MY"; marketsList["NG"] = "en_NG"; marketsList["NL"] = "nl_NL"; marketsList["NO"] = "no_NO"; marketsList["NZ"] = "en_NZ"; marketsList["OM"] = "en_OM"; marketsList["PE"] = "es_PE"; marketsList["PH"] = "en_PH"; marketsList["PL"] = "pl_PL"; marketsList["PT"] = "en_PT"; marketsList["QA"] = "en_QA"; marketsList["RU"] = "ru_RU"; marketsList["SA"] = "en_SA"; marketsList["SE"] = "sv_SE"; marketsList["SG"] = "en_SG"; marketsList["SK"] = "sk_SK"; marketsList["TH"] = "th_TH"; marketsList["TR"] = "tr_TR"; marketsList["TW"] = "zh_TW"; marketsList["US"] = "en_US"; marketsList["VE"] = "es_VE"; marketsList["VN"] = "vi_VN"; marketsList["ZA"] = "en_ZA";
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            foreach (var process in System.Diagnostics.Process.GetProcessesByName("phantomjs"))
                process.Kill();
        }

        public void getCaptcha(Profile profile)
        {
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

            IWebDriver _driver;
            _driver = new ChromeDriver(driverService);
            Size browser_size = _driver.Manage().Window.Size;

            if (browser_size.Height == 708 && browser_size.Width == 1050)
                _driver.Manage().Window.Size = new Size(511, 708);

            _driver.Navigate().GoToUrl("http://dev.adidas.com/sitekey.php?key=" + profile.Sitekey);

            while (_driver.Manage().Cookies.AllCookies.FirstOrDefault(s => s.Name == "g-recaptcha-response") == null)
                System.Threading.Thread.Sleep(1000);

            OpenQA.Selenium.Cookie cookie = _driver.Manage().Cookies.AllCookies.FirstOrDefault(s => s.Name == "g-recaptcha-response");
            captchas.Add(new C_Captcha { sitekey = profile.Sitekey, response = cookie.Value });

            _driver.Quit();
        }

        private string webRequestPost(Profile profile, string url, Dictionary<string, string> post, C_Proxy proxy=null)
        {
            string postData = "";

            foreach (string key in post.Keys)
            {
                postData += HttpUtility.UrlEncode(key) + "="
                      + HttpUtility.UrlEncode(post[key]) + "&";
            }

            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
            
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

            if(proxy != null)
            {
                foreach (C_Cookie cookie in proxy.cookies)
                    cookies.Add(new System.Net.Cookie(cookie.name, cookie.value) { Domain = cookie.domain });
            }

            foreach (C_Cookie cookie in profile.Cookies)
                cookies.Add(new System.Net.Cookie(cookie.name, cookie.value) { Domain = cookie.domain });

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

        void proxyRun(Profile profile, C_Proxy proxy, int i, DataGridViewRowCollection rows)
        {
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

            if (proxy.auth)
                driverService.ProxyAuthentication = String.Format("{0}:{1}", proxy.username, proxy.password);

            driverService.Proxy = proxy.address;
            //driverService.ProxyType = "http";

            rows[i].Cells[6].Value = "Setting up...";

            IWebDriver _driver;
            _driver = new PhantomJSDriver(driverService);
            _driver.Navigate().GoToUrl(profile.SplashUrl);

            if (ElementDisplayed(_driver, By.CssSelector(".message.message-1.hidden"), 40))
            {
                rows[i].Cells[6].Value = "On splash page...";

                while (_driver.FindElements(By.Id("captcha")).Count <= 0)
                    System.Threading.Thread.Sleep(1000);

                rows[i].Cells[6].Value = "Splash page passed!";
                rows[i].Cells[6].Style = new DataGridViewCellStyle { ForeColor = System.Drawing.Color.Green };

                string cookie_name = _driver.Manage().Cookies.AllCookies.FirstOrDefault(s => s.Value.Contains("hmac")).Name;
                proxy.hmac = _driver.Manage().Cookies.AllCookies.FirstOrDefault(s => s.Value.Contains("hmac")).Value;
                proxy.hmac_expiration = _driver.Manage().Cookies.AllCookies.FirstOrDefault(s => s.Value.Contains("hmac")).Expiry;
                rows[i].Cells[4].Value = proxy.hmac;
                proxy.sitekey = _driver.FindElement(By.Id("captcha")).GetAttribute("data-sitekey");
                rows[i].Cells[5].Value = proxy.sitekey;

                rows[i].Cells[6].Value = "HMAC and Sitekey retrieved!";
                foreach (OpenQA.Selenium.Cookie cookie in _driver.Manage().Cookies.AllCookies)
                {
                    if (cookie.Domain.Contains("adidas"))
                        proxy.cookies.Add(new C_Cookie { name = cookie.Name, value = cookie.Value, domain = cookie.Domain, expiry = cookie.Expiry });
                }

                passedSplash.Add(proxy);

                if(proxy.auth)
                    File.AppendAllText("hmacs.txt", String.Format("Proxy --- Address : {0} --- Username: {1} --- Password: {2} / Cookie --- Name: {3} --- Value : {4} / Sitekey: {5}", proxy.address, proxy.username, proxy.password, cookie_name, proxy.hmac, proxy.sitekey) + Environment.NewLine);
                else
                    File.AppendAllText("hmacs.txt", String.Format("Proxy --- Address : {0} / Cookie --- Name: {1} --- Value : {2} / Sitekey: {3}", proxy.address, cookie_name, proxy.hmac, proxy.sitekey) + Environment.NewLine);
                
                File.WriteAllText(String.Format("{0}\\{1}_productpage_source.txt", AppDomain.CurrentDomain.BaseDirectory, profile.ProductID), _driver.PageSource);
                _driver.Quit();
            }
            else
            {
                rows[i].Cells[6].Value = "Error!";
                rows[i].Cells[6].Style = new DataGridViewCellStyle { ForeColor = System.Drawing.Color.Red };

                _driver.Quit();
            }
        }
        void runProxyList(Profile profile,DataGridViewRowCollection rows)
        {
            if (proxylist.Count == 0){
                MessageBox.Show(String.Format("{0} - {1} : splash page mode needs at least one proxy.", profile.Email, profile.ProductID), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;}

            proxy_running = true;

            for (int i = 0; i < proxylist.Count; i++)
            {
                int index = i;
                Task.Factory.StartNew(() => proxyRun(profile, proxylist[index], index, rows));
            }
            
        }
        private string cartSplash(Profile profile, DataGridViewCell cell, DataGridViewRowCollection rows)
        {
            if(!proxy_running)
                Task.Factory.StartNew(() => runProxyList(profile, rows));

            cell.Value = "Waiting for HMAC...(check settings page)";

            while (passedSplash.Count == 0)
                System.Threading.Thread.Sleep(1000);

            cell.Value = "GOT HMAC!";

            C_Proxy proxy = passedSplash.FirstOrDefault(s => s.hmac_expiration > DateTime.Now);
            return cartNoSplash(profile, cell, proxy);
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

            if (proxy != null)
            {
                if (proxy.auth)
                    driverService.ProxyAuthentication = String.Format("{0}:{1}", proxy.username, proxy.password);

                driverService.Proxy = proxy.address;
                driverService.ProxyType = "http";
                driverService.IgnoreSslErrors = true;
            }

            _driver = new PhantomJSDriver(driverService);

            return _driver;
        }

        public bool Login(Profile profile, DataGridViewCell cell, C_Proxy proxy)
        {
            cell.Value = "Connecting to login page...";
            IWebDriver _driver = profile._driver;

            _driver = createNewJSDriver(proxy);
            _driver.Navigate().GoToUrl("https://cp." + Properties.Settings.Default.locale + "/web/eCom/" + marketsList[Properties.Settings.Default.code] + "/loadsignin?target=account");

            if (ElementDisplayed(_driver, By.Id("username"), 40))
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


            if (LoggedIn(_driver, 60))
            {
                cell.Value = "Logged in!";
                System.Threading.Thread.Sleep(1500);

                profile.loggedin = true;
                foreach (OpenQA.Selenium.Cookie cookie in _driver.Manage().Cookies.AllCookies)
                {
                    if (cookie.Domain.Contains("adidas"))
                        profile.Cookies.Add(new C_Cookie { name = cookie.Name, value = cookie.Value, domain = cookie.Domain, expiry = cookie.Expiry });
                }
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
        private string getFirstAvailableSize(List<double> sizes, string pid)
        {
            Dictionary<string, Dictionary<string, string>> inventory = getInventory(pid);

            foreach (KeyValuePair<string, Dictionary<string, string>> entry in inventory)
            {
                foreach(double size in sizes)
                {
                    string s = inventory[entry.Key]["size"];
                    int stock = Convert.ToInt32(inventory[entry.Key]["stockcount"]);

                    if (Properties.Settings.Default.code == "GB")
                    {
                        if (s == getUKSize(size) && stock > 0)
                            return entry.Key;
                    }
                    else
                    {
                        if (getEUSize(size).Replace(" ", String.Empty) == s.Replace(" ", String.Empty) && stock > 0 || s == size.ToString("0.#").Replace(',', '.') && stock > 0)
                            return entry.Key;
                    }
                }
            }

            return null;
        }
        private string cartNoSplash(Profile profile, DataGridViewCell cell, C_Proxy proxy=null)
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
            }

            if (Login(profile, cell, proxy))
            {
                if (profile.captcha && !String.IsNullOrWhiteSpace(profile.Sitekey))
                {
                    cell.Value = "SOLVE CAPTCHA!";
                    cell.Style = new DataGridViewCellStyle { ForeColor = Color.Red };

                    while (captchas.FirstOrDefault(s => s.sitekey.Contains(profile.Sitekey) && s.expiration > DateTime.Now) == null)
                            System.Threading.Thread.Sleep(1000);

                    C_Captcha captcha = captchas.FirstOrDefault(s => s.sitekey.Contains(profile.Sitekey) && s.expiration > DateTime.Now);
                    post.Add("g-recaptcha-response", captcha.response);

                    if (profile.duplicate && !String.IsNullOrWhiteSpace(profile.Duplicate))
                        post.Add(profile.Duplicate, captcha.response);

                    captchas.Remove(captcha);
                }

                cell.Value = "Checking sizes...";
                cell.Style = new DataGridViewCellStyle { ForeColor = Color.Empty };

                string size = getFirstAvailableSize(profile.Sizes, profile.ProductID);
                if (size == null)
                    return "No sizes available";

                cell.Value = "Adding to cart...";
                post.Add("pid", size); post.Add("masterPid", profile.ProductID); post.Add("Quantity", "1"); post.Add("request", "ajax"); post.Add("responseformat", "json"); post.Add("sessionSelectedStoreID", "null"); post.Add("layer", "Add To Bag overlay");
                result = webRequestPost(profile, atcURL, post, proxy);
            }

            return result;
        }

        public string cart(Profile profile, DataGridViewCell cell, DataGridViewRowCollection rows = null)
        {
            if (profile.splash && !String.IsNullOrWhiteSpace(profile.SplashUrl))
                return cartSplash(profile, cell, rows);
            else
                return cartNoSplash(profile, cell);
        }

        public static string webRequest(string url)
        {
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            webRequest.Method = "GET";
            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";

            HttpWebResponse myHttpWebResponse = (HttpWebResponse)webRequest.GetResponse();

            Stream responseStream = myHttpWebResponse.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);

            string pageContent = myStreamReader.ReadToEnd();

            myStreamReader.Close();
            responseStream.Close();
            myHttpWebResponse.Close();

            return pageContent;
        }

        public static Dictionary<string, dynamic> json_decode(string source)
        {
            var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            var dict = jss.Deserialize<Dictionary<string, dynamic>>(source);

            return dict;
        }

        public Dictionary<string, Dictionary<string, string>> getInventory(string pid)
        {
            string url = String.Format("http://www.{0}/on/demandware.store/Sites-adidas-{1}-Site/{2}/Product-GetVariants?pid={3}", Properties.Settings.Default.locale, Properties.Settings.Default.code, marketsList[Properties.Settings.Default.code], pid);

            string responseString = webRequest(url);

            Dictionary<string, Dictionary<string, string>> products = new Dictionary<string, Dictionary<string, string>>();

            Dictionary<string, dynamic> json = json_decode(responseString);

            for (int i = 0; i < json["variations"]["variants"].Count; i++)
            {
                string stylecode = json["variations"]["variants"][i]["id"];
                products[stylecode] = new Dictionary<string, string>();

                products[stylecode]["size"] = json["variations"]["variants"][i]["attributes"]["size"];
                products[stylecode]["stockcount"] = Convert.ToInt32(json["variations"]["variants"][i]["ATS"]).ToString();
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
