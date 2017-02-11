using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3s_atc
{
    public partial class Form1 : Form
    {
        public Helpers helpers;
        private List<double> Sizes;
        private int currentMouseOverRow;
        private int captchaRow;

        public Form1()
        {
            InitializeComponent();

            addLog("Welcome to 3s_atc!", Color.Empty);

            helpers = new Helpers();
            Sizes = new List<double>();

            if(Properties.Settings.Default.profiles.Length > 0)
                helpers.profiles = helpers.LoadProfiles();

            if (Properties.Settings.Default.proxylist.Length > 0)
                helpers.proxylist = helpers.LoadProxyList();

            foreach(Profile profile in helpers.profiles)
                dataGridView1.Rows.Add(new string[] { profile.Email, profile.ProductID, string.Join("/ ", profile.Sizes.Select(x => x.ToString()).ToArray()), profile.Sitekey, profile.ClientID, profile.Duplicate, string.Join(";", profile.ExtraCookies.Select(kv => kv.Key + "=" + kv.Value).ToArray()), profile.SplashUrl, "" });

            foreach(C_Proxy proxy in helpers.proxylist)
                dataGridView2.Rows.Add(new string[] { proxy.address, proxy.auth.ToString(), proxy.username, proxy.password, null, null, null });

            for (int i = 0; i < comboBox_3_Website.Items.Count; i++)
            {
                if (comboBox_3_Website.GetItemText(comboBox_3_Website.Items[i]) == string.Format("{0} - {1}", Properties.Settings.Default.code, Properties.Settings.Default.locale)){
                    comboBox_3_Website.SelectedItem = comboBox_3_Website.Items[i]; break;}
            }
        }

        public void addLog(string text, Color color)
        {
            RichTextBox box = this.richTextBox_1_Logs;

            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText("- " + text + Environment.NewLine);
            box.SelectionColor = box.ForeColor;
        }

        private void button_1_AddProfile_Click(object sender, EventArgs e)
        {
            string sitekey, clientid, duplicate, splash_url;

            if (String.IsNullOrWhiteSpace(Properties.Settings.Default.locale) || String.IsNullOrWhiteSpace(Properties.Settings.Default.code)) {
                MessageBox.Show("Please choose a locale in the 'Settings' tab.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;}

            if(String.IsNullOrWhiteSpace(textBox_1_Email.Text) || String.IsNullOrWhiteSpace(textBox_1_Password.Text) || String.IsNullOrWhiteSpace(textBox_1_PID.Text)) {
                MessageBox.Show("Email, password or product ID empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;}

            if (String.IsNullOrWhiteSpace(textBox_1_Sitekey.Text) && checkBox_1_Captcha.Checked || String.IsNullOrWhiteSpace(textBox_1_ClientID.Text) && checkBox_1_ClientID.Checked || String.IsNullOrWhiteSpace(textBox_1_Duplicate.Text) && checkBox_1_Duplicate.Checked || String.IsNullOrWhiteSpace(textBox_1_Splashurl.Text) && checkBox_1_Splashpage.Checked) {
                MessageBox.Show("Captcha/Duplicate/Splash page checked but fields are empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;}

            if(Sizes.Count <= 0) {
                MessageBox.Show("Select at least one size to continue.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;}

            if (helpers.profiles.FindIndex(x => x.Email == textBox_1_Email.Text && x.ProductID == textBox_1_PID.Text) != -1) {
                MessageBox.Show(String.Format("E-mail already in use for product ID '{0}'.", textBox_1_PID.Text), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; }

            sitekey = (checkBox_1_Captcha.Checked) ? textBox_1_Sitekey.Text : null;
            clientid = (checkBox_1_ClientID.Checked) ? textBox_1_ClientID.Text : null;
            duplicate = (checkBox_1_Duplicate.Checked) ? textBox_1_Duplicate.Text : null;
            splash_url = (checkBox_1_Splashpage.Checked) ? textBox_1_Splashurl.Text : null;

            helpers.profiles.Add(new Profile { Email = textBox_1_Email.Text, Password = textBox_1_Password.Text, ProductID = textBox_1_PID.Text, Sizes = this.Sizes, Sitekey = sitekey, ClientID = clientid, Duplicate = duplicate, ExtraCookies = helpers.splitCookies(richTextBox_1_Cookies.Text), SplashUrl = splash_url, captcha = checkBox_1_Captcha.Checked, clientid = checkBox_1_ClientID.Checked, duplicate = checkBox_1_Duplicate.Checked, splash = checkBox_1_Splashpage.Checked, loggedin = false, running = false });
            string[] row = new string[] { textBox_1_Email.Text, textBox_1_PID.Text, string.Join("/ ", Sizes.Select(x => x.ToString()).ToArray()), sitekey, clientid, duplicate, richTextBox_1_Cookies.Text, splash_url, "" };
            dataGridView1.Rows.Add(row);
            helpers.SaveProfiles();

            Sizes.Clear();
        }

        private void button_1_SelectSizes_Click(object sender, EventArgs e)
        {
            Form_Sizes form_sizes = new Form_Sizes(Sizes);
            form_sizes.ShowDialog();

            Sizes = new List<double>(form_sizes.sizes);
        }

        private void dataGridView1_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            int index = helpers.profiles.FindIndex(x => x.Email == e.Row.Cells[0].Value.ToString() && x.ProductID == e.Row.Cells[1].Value.ToString());
            helpers.profiles.RemoveAt(index);
            helpers.SaveProfiles();
        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            currentMouseOverRow = dataGridView1.HitTest(e.X, e.Y).RowIndex;

            if (e.Button == MouseButtons.Right && currentMouseOverRow >= 0)
            {
                ContextMenu m = new ContextMenu();
                m.MenuItems.Add(new MenuItem("Show", show_Click));

                if(!String.IsNullOrWhiteSpace(helpers.profiles[currentMouseOverRow].Sitekey) && helpers.profiles[currentMouseOverRow].captcha)
                    m.MenuItems.Add(new MenuItem("Solve captcha", captcha_Click));

                m.Show(dataGridView1, new Point(e.X, e.Y));
            }
        }

        private void show_Click(Object sender, System.EventArgs e)
        {
            string str = "";

            foreach (var field in helpers.profiles[currentMouseOverRow].GetType().GetProperties())
            {
               var value = field.GetValue(helpers.profiles[currentMouseOverRow]);
               if (value != null)
               {
                   if (value is List<C_Cookie>)
                       continue;

                   if (value is Dictionary<string, string>)
                   {
                       Dictionary<string, string> v = field.GetValue(helpers.profiles[currentMouseOverRow], null) as Dictionary<string, string>;
                       str = str + field.Name + ": " + string.Join(",", v) + " | ";
                   }
                   else if (value is List<double>)
                   {
                       List<double> v = field.GetValue(helpers.profiles[currentMouseOverRow], null) as List<double>;
                       str = str + field.Name + ": " + string.Join("/", v.Select(d => d.ToString()) ) + " | ";
                   }
                   else
                       str = str + field.Name + ": " + value + " | ";
               }
            }

            MessageBox.Show(str);
        }

        private void captcha_Click(Object sender, System.EventArgs e)
        {
            int captchaRow = currentMouseOverRow;
            if (Properties.Settings.Default.chrome_captcha)
                Task.Run(() => helpers.getCaptcha(helpers.profiles[currentMouseOverRow]));
            else
            {
                panel_Home.Visible = false; panel_Tools.Visible = true; panel_Settings.Visible = false;
                webBrowser_2.Url = new Uri(String.Format("http://dev.adidas.com/sitekey.php?key={0}", helpers.profiles[captchaRow].Sitekey));
            }
        }
        private async Task cart(Profile profile, DataGridViewCell cell, DataGridViewRowCollection rows = null)
        {
            addLog(String.Format("{0} : started process for product '{1}'", profile.Email, profile.ProductID), Color.Empty);
            addLog(String.Format("{0} : logging in...", profile.Email, profile.ProductID), Color.Empty);

            string result = await Task.Run(() =>  helpers.cart(profile, cell, rows));
            if (result.Contains("SUCCESS"))
            {
                addLog(String.Format("{0} : product '{1}' in your cart!", profile.Email, profile.ProductID), Color.Green);
                cell.Value = "In cart!";
                cell.Style = new DataGridViewCellStyle { ForeColor = Color.Green };
            }
            else
            {
                addLog(String.Format("{0} - {1} : {2}", profile.Email, profile.ProductID, result.Replace("\n", String.Empty)), Color.Red);
                cell.Value = "Error! Check logs";
                cell.Style = new DataGridViewCellStyle { ForeColor = Color.Red };
            }
        }
        private void button_1_Run_Click(object sender, EventArgs e)
        {
            for(int i = 0; i < helpers.profiles.Count; i++)
            {
                Profile profile = helpers.profiles[i];
                DataGridViewRow row = dataGridView1.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[0].Value.ToString() == profile.Email && r.Cells[1].Value.ToString() == profile.ProductID).First();
                if (String.IsNullOrWhiteSpace(row.Cells[8].Value.ToString()) || row.Cells[8].Value.ToString().Contains("Error") || row.Cells[8].Style.ForeColor == Color.Red)
                {
                    if (!profile.running)
                    {
                        if (profile.splash)
                            cart(profile, row.Cells[8], dataGridView2.Rows);
                        else
                            cart(profile, row.Cells[8]);
                    }
                }
            }
        }

        private void homeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel_Home.Visible = true; panel_Tools.Visible = false; panel_Settings.Visible = false;
        }

        private void inventoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel_Home.Visible = false; panel_Tools.Visible = true; panel_Settings.Visible = false;
        }

        private void proxyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel_Home.Visible = false; panel_Tools.Visible = false; panel_Settings.Visible = true;
        }

        private void webBrowser_2_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

            if (webBrowser_2.Url != new Uri("about:blank"))
            {

                string recaptcha_response = helpers.getCookie("g-recaptcha-response", webBrowser_2.Document.Cookie.Split(';'));

                if (!String.IsNullOrWhiteSpace(recaptcha_response))
                {
                    Profile profile = helpers.profiles[captchaRow];
                    helpers.captchas.Add(new C_Captcha { sitekey = profile.Sitekey, response = recaptcha_response });
                }
            }
        }

        private async Task getInventory()
        {
            listBox_2_Inventory.Items.Add(String.Format("Getting inventory for product '{0}' ...", Properties.Settings.Default.pid));
            Dictionary<string, Dictionary<string, string>> products = await Task.Run(() => helpers.getInventory(Properties.Settings.Default.pid));
            listBox_2_Inventory.Items.Clear();

            foreach (KeyValuePair<string, Dictionary<string, string>> entry in products)
                listBox_2_Inventory.Items.Add(String.Format("{0} - Size: {1} - Quantity: {2}", entry.Key, products[entry.Key]["size"], products[entry.Key]["stockcount"]));

            if (listBox_2_Inventory.Items.Count > 0 && listBox_2_Inventory.Items[1].ToString().Contains("Quantity"))
                addLog("Inventory checker : Done!", Color.Green);
            else
            {
                listBox_2_Inventory.Items.Add("Error!");
                addLog("Inventory checker : Error!", Color.Red);
            }
        }

        private void button_2_Check_Click(object sender, EventArgs e)
        {
            addLog(String.Format("Getting inventory for product: {0}", textBox_2_PID.Text), Color.Empty);
            Properties.Settings.Default.pid = textBox_2_PID.Text; Properties.Settings.Default.Save();
            listBox_2_Inventory.Items.Clear();
            getInventory();
        }
        private void button_3_Update_Click(object sender, EventArgs e)
        {
            if (comboBox_3_Website.Text != String.Empty)
            {
                string[] split = comboBox_3_Website.Text.Split(new string[] { " - " }, StringSplitOptions.None);
                Properties.Settings.Default.locale = split[1];
                Properties.Settings.Default.code = split[0];
            }

            Properties.Settings.Default.chrome_captcha = checkBox_3_captchachrome.Checked;

            Properties.Settings.Default.Save();
        }

        private void button_3_Add_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textBox_3_Address.Text)) {
                MessageBox.Show("Proxy address is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; }

            if(helpers.proxylist.FindIndex(x => x.address == textBox_3_Address.Text && x.auth == checkBox_3_Auth.Checked && x.username == textBox_3_Username.Text && x.password == textBox_3_Password.Text) != -1) {
                MessageBox.Show("Proxy already in list.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;}

            helpers.proxylist.Add(new C_Proxy { address = textBox_3_Address.Text, username = textBox_3_Username.Text, password = textBox_3_Password.Text, hmac = null, sitekey = null, auth = checkBox_3_Auth.Checked });

            dataGridView2.Rows.Add(new string[] { textBox_3_Address.Text, checkBox_3_Auth.Checked.ToString(), textBox_3_Username.Text, textBox_3_Password.Text, null, null, null});

            helpers.SaveProxyList();
        }

        private void dataGridView2_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            int index = helpers.proxylist.FindIndex(x => x.address.Contains(e.Row.Cells[0].Value.ToString()) && x.auth.ToString().Contains(e.Row.Cells[1].Value.ToString()));
            helpers.proxylist.RemoveAt(index);
            helpers.SaveProxyList();
        }
    }
}
