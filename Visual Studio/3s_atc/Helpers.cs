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
using System.IO.Pipes;

namespace _3s_atc
{
    public class Helpers
    {
        public List<Profile> profiles;
        public List<C_Captcha> captchas;
        public List<C_Proxy> proxylist;
        public List<CefSharp.Cookie> gmail_cookies;
        private Dictionary<string, string> marketsList;
        private bool proxy_running, sessions_running;
        public bool gmail_loggedin;
        public List<string> loggingin_emails;
        public List<C_Session> sessionlist;
        private Form1 form1;

        public Helpers(Form1 form)
        {
            form1 = form;
            proxy_running = false;
            profiles = new List<Profile>(); 
            captchas = new List<C_Captcha>(); 
            proxylist = new List<C_Proxy>();
            loggingin_emails = new List<string>();
            sessionlist = new List<C_Session>();
            gmail_loggedin = false;

            marketsList = new Dictionary<string, string>(); marketsList["AE"] = "en_AE"; marketsList["AR"] = "es_AR"; marketsList["AT"] = "de_AT"; marketsList["AU"] = "en_AU"; marketsList["BE"] = "fr_BE"; marketsList["BH"] = "en_BH"; marketsList["BR"] = "pt_BR"; marketsList["CA"] = "en_CA"; marketsList["CF"] = "fr_CA"; marketsList["CH"] = "de_CH"; marketsList["CL"] = "es_CL"; marketsList["CN"] = "zh_CN"; marketsList["CO"] = "es_CO"; marketsList["CZ"] = "cz_CZ"; marketsList["DE"] = "de_DE"; marketsList["DK"] = "da_DK"; marketsList["EE"] = "et_EE"; marketsList["ES"] = "es_ES"; marketsList["FI"] = "fi_FI"; marketsList["FR"] = "fr_FR"; marketsList["GB"] = "en_GB"; marketsList["GR"] = "en_GR"; marketsList["HK"] = "zh_HK"; marketsList["HU"] = "hu_HU"; marketsList["ID"] = "id_ID"; marketsList["IE"] = "en_IE"; marketsList["IN"] = "en_IN"; marketsList["IT"] = "it_IT"; marketsList["JP"] = "ja_JP"; marketsList["KR"] = "ko_KR"; marketsList["KW"] = "ar_KW"; marketsList["MX"] = "es_MX"; marketsList["MY"] = "en_MY"; marketsList["NG"] = "en_NG"; marketsList["NL"] = "nl_NL"; marketsList["NO"] = "no_NO"; marketsList["NZ"] = "en_NZ"; marketsList["OM"] = "en_OM"; marketsList["PE"] = "es_PE"; marketsList["PH"] = "en_PH"; marketsList["PL"] = "pl_PL"; marketsList["PT"] = "en_PT"; marketsList["QA"] = "en_QA"; marketsList["RU"] = "ru_RU"; marketsList["SA"] = "en_SA"; marketsList["SE"] = "sv_SE"; marketsList["SG"] = "en_SG"; marketsList["SK"] = "sk_SK"; marketsList["TH"] = "th_TH"; marketsList["TR"] = "tr_TR"; marketsList["TW"] = "zh_TW"; marketsList["US"] = "en_US"; marketsList["VE"] = "es_VE"; marketsList["VN"] = "vi_VN"; marketsList["ZA"] = "en_ZA";
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

        private void runProxyList(Profile profile,DataGridViewRowCollection rows)
        {
            if (proxylist.Count == 0){
                MessageBox.Show(String.Format("{0} - {1} : splash page mode needs at least one proxy.", profile.Email, profile.ProductID), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;}

            proxy_running = true;

            for (int i = 0; i < proxylist.Count; i++)
            {
                int index = i;
                Task.Run(() =>
                {
                    runProxy(profile, index, rows[index]);
                });
                System.Threading.Thread.Sleep(1000);
            }
            
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
                Task.Run(() =>
                {
                    runSession(profile, index, rows[index]);
                });
                System.Threading.Thread.Sleep(1000);
            }
        }

        public C_Session DeserializeSession(string sessionData)
        {
            try
            {
                C_Session result;

                System.Xml.Serialization.XmlSerializer xsSubmit = new System.Xml.Serialization.XmlSerializer(typeof(C_Session));

                using (TextReader reader = new StringReader(sessionData))
                {
                    result = (C_Session)xsSubmit.Deserialize(reader);
                }

                return result;
            }
            catch (Exception c)
            {
                System.Windows.Forms.MessageBox.Show(c.Message);
                return null;
            }
        }

        private string SerializeSession(C_Session data)
        {
            System.Xml.Serialization.XmlSerializer xsSubmit = new System.Xml.Serialization.XmlSerializer(typeof(C_Session));
            var xml = "";

            using (var sww = new StringWriter())
            {
                using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, data);
                    xml = sww.ToString();
                }
            }

            return xml;
        }

        private void runSession(Profile profile, int index, DataGridViewRow row)
        {
            C_Session session = sessionlist[index];
            session.index = index;

            row.Cells[8].Value = "Setting up...";

            string pipename = Process.GetCurrentProcess().Id.ToString() + "_session_" + index.ToString();

            var pipe = new NamedPipeServerStream(pipename, PipeDirection.InOut, 1);
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo = new ProcessStartInfo();
            process.StartInfo.FileName = "3s_atc - browser.exe";
            process.StartInfo.Arguments = profile.SplashUrl + " " + pipename + " " + Properties.Settings.Default.splashidentifier + " " + Properties.Settings.Default.productpageidentifier + " " + Properties.Settings.Default.refresh_interval.ToString() + " " + gmail_loggedin.ToString();
            process.Start();

            session.pid = process.Id;

            pipe.WaitForConnection();

            session.hideShow();

            try
            {
                StreamWriter writer = new StreamWriter(pipe);

                if (gmail_loggedin)
                {
                    string gmailCookiesData = SerializeCefCookies(gmail_cookies);
                    writer.WriteLine(gmailCookiesData);
                    writer.Flush();
                }

                string sessionData = SerializeSession(session);

                writer.WriteLine(sessionData);
                writer.Flush();

                StreamReader reader = new StreamReader(pipe);

                while (true)
                {
                    string str = reader.ReadLine();
                    if (!String.IsNullOrEmpty(str))
                        parseMessage(index, str, row);
                }

            }

            catch (IOException exception)
            {
                MessageBox.Show(String.Format("Session {0} error: {1}\n", index.ToString(), exception.Message));
            }
        }

        public C_Proxy DeserializeProxy(string proxyData)
        {
            try
            {
                C_Proxy result;

                System.Xml.Serialization.XmlSerializer xsSubmit = new System.Xml.Serialization.XmlSerializer(typeof(C_Proxy));

                using (TextReader reader = new StringReader(proxyData))
                {
                    result = (C_Proxy)xsSubmit.Deserialize(reader);
                }

                return result;
            }
            catch (Exception c)
            {
                System.Windows.Forms.MessageBox.Show(c.Message);
                return null;
            }
        }

        private string SerializeProxy(C_Proxy data)
        {
            System.Xml.Serialization.XmlSerializer xsSubmit = new System.Xml.Serialization.XmlSerializer(typeof(C_Proxy));
            var xml = "";

            using (var sww = new StringWriter())
            {
                using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, data);
                    xml = sww.ToString();
                }
            }

            return xml;
        }

        private void runProxy(Profile profile, int index, DataGridViewRow row)
        {
            C_Proxy proxy = proxylist[index];
            proxy.index = index;

            row.Cells[8].Value = "Setting up...";

            string pipename = Process.GetCurrentProcess().Id.ToString() + "_proxy_" + index.ToString();

            var pipe = new NamedPipeServerStream(pipename, PipeDirection.InOut, 1);
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo = new ProcessStartInfo();
            process.StartInfo.FileName = "3s_atc - browser.exe";
            process.StartInfo.Arguments = profile.SplashUrl + " " + pipename + " " + Properties.Settings.Default.splashidentifier + " " + Properties.Settings.Default.productpageidentifier + " " + Properties.Settings.Default.refresh_interval.ToString();
            process.Start();

            pipe.WaitForConnection();

            proxy.pid = process.Id;
            proxy.hideShow();

            try
            {
                StreamWriter writer = new StreamWriter(pipe);

                if (gmail_loggedin)
                {
                    string gmailCookiesData = SerializeCefCookies(gmail_cookies);
                    writer.WriteLine(gmailCookiesData);
                    writer.Flush();
                }

                string proxyData = SerializeProxy(proxy);
                writer.WriteLine(proxyData);
                writer.Flush();

                StreamReader reader = new StreamReader(pipe);

                while (true)
                {
                    string str = reader.ReadLine();
                    if (!String.IsNullOrEmpty(str))
                        parseMessage(index, str, row, false, true);
                }

            }

            catch (IOException exception)
            {
                MessageBox.Show(String.Format("Session {0} error: {1}\n", index.ToString(), exception.Message));
            }
        }

        private void parseMessage(int index, string msg, DataGridViewRow row, bool session=true, bool proxy=false)
        {
            switch(msg)
            {
                case "splash":
                    row.Cells[8].Value = "On splash page...";
                    break;
                case "product":
                    row.Cells[8].Value = "ON PRODUCT PAGE!";
                    break;
                case "error":
                    row.Cells[8].Value = "ERROR COULD NOT REACH SPLASH PAGE!";
                    row.Cells[8].Style = new DataGridViewCellStyle { ForeColor = System.Drawing.Color.Red };
                    break;
                default:
                    if (msg.Contains("xml"))
                    {
                        if (session)
                        {
                            sessionlist[index] = DeserializeSession(msg);
                            row.Cells[8].Value = "PRODUCT PAGE - EXTRACTED SESSION";
                            row.Cells[4].Value = sessionlist[index].hmac_cookie.value;
                            row.Cells[5].Value = sessionlist[index].sitekey;
                            row.Cells[6].Value = sessionlist[index].clientid;
                            row.Cells[7].Value = sessionlist[index].duplicate;
                        }
                        else if (!session && proxy)
                        {
                            proxylist[index] = DeserializeProxy(msg);
                            row.Cells[8].Value = "PRODUCT PAGE - EXTRACTED SESSION";
                            row.Cells[4].Value = proxylist[index].hmac_cookie.value;
                            row.Cells[5].Value = proxylist[index].sitekey;
                            row.Cells[6].Value = proxylist[index].clientid;
                            row.Cells[7].Value = proxylist[index].duplicate;
                        }
                    }
                    break;
            }
        }

        private string cartSplash(Profile profile, DataGridViewCell cell, DataGridViewRowCollection rows)
        {
            cell.Value = "Waiting for HMAC...(check settings page)";
            C_Proxy proxy = null;
            C_Session session = null;

            if (!proxy_running && profile.splashmode == 1)
            {
                Task.Run(() => runProxyList(profile, rows));

                while (proxylist.FirstOrDefault(s => proxy.passed) == null)
                    System.Threading.Thread.Sleep(1000);

                cell.Value = "GOT HMAC!";

                proxy = proxylist.FirstOrDefault(s => proxy.passed && s.hmac_cookie.expiry > DateTime.Now);
            }
            else if(!sessions_running && profile.splashmode == 2)
            {
                    for (int i = 0; i < Properties.Settings.Default.sessions_count; i++)
                        sessionlist.Add(new C_Session { refresh = false });
                    for (int i = 0; i < Properties.Settings.Default.r_sessions_count; i++)
                        sessionlist.Add(new C_Session { refresh = true });

                rows.Clear();

                    foreach (C_Session s in sessionlist)
                        rows.Add(new string[] { "session", s.refresh.ToString(), "False", null, null, null, null, null, null });

                Task.Run(() => runSessionList(profile, rows));

                while (sessionlist.FirstOrDefault(x => x.passed == true) == null)
                    System.Threading.Thread.Sleep(1000);

                cell.Value = "GOT HMAC!";

                session = sessionlist.FirstOrDefault(s => s.passed && s.hmac_cookie.expiry > DateTime.Now);
            }

            return cartNoSplash(profile, cell, proxy, session);
        }

        public void guestMode_Cart(Profile profile, DataGridViewRowCollection rows)
        {
            C_Proxy proxy = null;
            C_Session session = null;

            if (!proxy_running && profile.splashmode == 1)
            {
                Task.Run(() => runProxyList(profile, rows));

                while (proxylist.FirstOrDefault(s => proxy.passed) == null)
                    System.Threading.Thread.Sleep(1000);

                proxy = proxylist.FirstOrDefault(s => proxy.passed && s.hmac_cookie.expiry > DateTime.Now);
                MessageBox.Show("proxy_" + proxy.index.ToString() + " : on product page! Right click on the session and show the window in order to purchase the shoe!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if(!sessions_running && profile.splashmode == 2)
            {
                    for (int i = 0; i < Properties.Settings.Default.sessions_count; i++)
                        sessionlist.Add(new C_Session { refresh = false });
                    for (int i = 0; i < Properties.Settings.Default.r_sessions_count; i++)
                        sessionlist.Add(new C_Session { refresh = true });

                rows.Clear();

                    foreach (C_Session s in sessionlist)
                        rows.Add(new string[] { "session_" + s.index.ToString(), s.refresh.ToString(), "False", null, null, null, null, null, null });

                Task.Run(() => runSessionList(profile, rows));

                while (sessionlist.FirstOrDefault(x => x.passed == true) == null)
                    System.Threading.Thread.Sleep(1000);

                session = sessionlist.FirstOrDefault(s => s.passed == true);
                MessageBox.Show("session_" + session.index.ToString() + " : on product page! Right click on the session and show the window in order to purchase the shoe!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
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

            string url = "https://cp." + Properties.Settings.Default.locale + "/web/eCom/" + marketsList[Properties.Settings.Default.code] + "/loadsignin?target=account";

            string pipename = Process.GetCurrentProcess().Id.ToString() + "_login_" + profile.index.ToString();

            var pipe = new NamedPipeServerStream(pipename, PipeDirection.InOut, 1);
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo = new ProcessStartInfo();
            process.StartInfo.FileName = "3s_atc - browser.exe";
            process.StartInfo.Arguments = url + " " + pipename + " " + profile.Email + " " + profile.Password;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();

            pipe.WaitForConnection();

            try
            {
                StreamReader reader = new StreamReader(pipe);

                while (true)
                {
                    string str = reader.ReadLine();
                    if (!String.IsNullOrEmpty(str))
                    {
                        switch(str)
                        {
                            case "loggingin":
                                cell.Value = "Logging in...";
                                break;
                            case "loggedin":
                                cell.Value = "Logged in!";
                                profile.loggedin = true;
                                loggingin_emails.Remove(profile.Email);
                                break;
                            case "error":
                                cell.Value = "Error while connecting to login page";
                                cell.Style = new DataGridViewCellStyle { ForeColor = System.Drawing.Color.Red };
                                process.Kill();
                                return false;
                            case "error_unknown":
                                cell.Value = "Unknown error while logging in.";
                                cell.Style = new DataGridViewCellStyle { ForeColor = System.Drawing.Color.Red };
                                process.Kill();
                                break;
                            default:
                                if(str.Contains("xml"))
                                {
                                    List<C_Cookie> cookies = DeserializeCookies(str);
                                    foreach (C_Cookie c in cookies)
                                        profile.Cookies.Add(c);
                                    process.Kill();
                                    return true;
                                }
                                else if(str.StartsWith("error:"))
                                {
                                    cell.Value = str.Split(new string[] { "error: " }, StringSplitOptions.None)[0];
                                    cell.Style = new DataGridViewCellStyle { ForeColor = System.Drawing.Color.Red };
                                    process.Kill();
                                    return false;
                                }
                                break;
                        }
                    }
                }
            }
            catch (IOException exception)
            {
                MessageBox.Show(String.Format("NamedPipe error: {0}\n", exception.Message));
            }

            return false;
        }

        public List<C_Cookie> DeserializeCookies(string cookiesData)
        {
            try
            {
                List<C_Cookie> result;

                System.Xml.Serialization.XmlSerializer xsSubmit = new System.Xml.Serialization.XmlSerializer(typeof(List<C_Cookie>));

                using (TextReader reader = new StringReader(cookiesData))
                {
                    result = (List<C_Cookie>)xsSubmit.Deserialize(reader);
                }

                return result;
            }
            catch (Exception c)
            {
                System.Windows.Forms.MessageBox.Show(c.Message);
                return null;
            }
        }

        private string SerializeCefCookies(List<CefSharp.Cookie> data)
        {
            System.Xml.Serialization.XmlSerializer xsSubmit = new System.Xml.Serialization.XmlSerializer(typeof(List<CefSharp.Cookie>));
            var xml = "";

            using (var sww = new StringWriter())
            {
                using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, data);
                    xml = sww.ToString();
                }
            }

            return xml;
        }

        public List<CefSharp.Cookie> DeserializeCefCookies(string cookiesData)
        {
            try
            {
                List<CefSharp.Cookie> result;
                System.Xml.Serialization.XmlSerializer xsSubmit = new System.Xml.Serialization.XmlSerializer(typeof(List<CefSharp.Cookie>));
                using (TextReader reader = new StringReader(cookiesData))
                {
                    result = (List<CefSharp.Cookie>)xsSubmit.Deserialize(reader);
                }

                return result;
            }
            catch (Exception c)
            {
                System.Windows.Forms.MessageBox.Show(c.Message);
                return null;
            }
        }

        public void LoginGmail()
        {
            string pipename = Process.GetCurrentProcess().Id.ToString() + "_login_gmail";

            var pipe = new NamedPipeServerStream(pipename, PipeDirection.InOut, 1);
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo = new ProcessStartInfo();
            process.StartInfo.FileName = "3s_atc - browser.exe";
            process.StartInfo.Arguments = "https://www.google.com/gmail" + " " + pipename;
            process.Start();

            pipe.WaitForConnection();

            StreamReader reader = new StreamReader(pipe);

            string str = reader.ReadLine();
            if (!String.IsNullOrEmpty(str))
            {
                gmail_cookies = DeserializeCefCookies(str);
                gmail_loggedin = true;
                MessageBox.Show("Logged in!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            process.Kill();
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

        public double getUSSize(string size)
        {
            string[] tokens = size.Split(new char[0]);
            double US_Size = double.Parse(tokens[1], System.Globalization.CultureInfo.InvariantCulture);

            return US_Size;
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
