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
        private int currentMouseOverRow2;
        private bool warningDisplayed;
        public bool guestmode;
        public int splashmode;
        public string SplashUrl;

        public Form1()
        {
            InitializeComponent();

            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;

            helpers = new Helpers(this);
            Sizes = new List<double>();
            warningDisplayed = false;
            guestmode = false;
            splashmode = 0;

            if (Properties.Settings.Default.profiles.Length > 0)
                helpers.profiles = helpers.LoadProfiles();

            if (Properties.Settings.Default.proxylist.Length > 0)
                helpers.proxylist = helpers.LoadProxyList();

            foreach (Profile profile in helpers.profiles)
            {
                dataGridView1.Rows.Add(new string[] { profile.name, profile.ProductID, string.Join("/ ", profile.Sizes.Select(x => x.ToString()).ToArray()), "" });
                //profile.loggedin = false;
            }

            foreach (C_Proxy proxy in helpers.proxylist)
            {
                if(!String.IsNullOrEmpty(proxy.username))
                    dataGridView2.Rows.Add(new string[] { proxy.address + "@" + proxy.username, proxy.refresh.ToString(), "" });
                else
                    dataGridView2.Rows.Add(new string[] { proxy.address, proxy.refresh.ToString(), "" });
            }

            for (int i = 0; i < comboBox_3_Website.Items.Count; i++)
            {
                if (comboBox_3_Website.GetItemText(comboBox_3_Website.Items[i]) == string.Format("{0} - {1}", Properties.Settings.Default.code, Properties.Settings.Default.locale))
                {
                    comboBox_3_Website.SelectedItem = comboBox_3_Website.Items[i]; break;
                }
            }

            for (int i = 0; i < comboBox_2_CartBrowser.Items.Count; i++)
            {
                if (comboBox_2_CartBrowser.GetItemText(comboBox_2_CartBrowser.Items[i]) == Properties.Settings.Default.cartbrowser)
                {
                    comboBox_2_CartBrowser.SelectedItem = comboBox_2_CartBrowser.Items[i]; 
                    break;
                }
            }

            comboBox_1_SplashMode.SelectedIndex = 0;
            numericUpDown_Sessions.Value = Properties.Settings.Default.sessions_count; numericUpDown_RSessions.Value = Properties.Settings.Default.r_sessions_count; numericUpDown_3_RefreshInterval.Value = Properties.Settings.Default.refresh_interval; textBox_3_SplashIdentifier.Text = Properties.Settings.Default.splashidentifier; textBox_3_ProductPageIdentifier.Text = Properties.Settings.Default.productpageidentifier;
            label_3_SCount.Visible = false; label_3_SRCount.Visible = false;
            numericUpDown_Sessions.Visible = false; numericUpDown_RSessions.Visible = false;
            label_1_ProxyAddress.Visible = false; label_1_Username.Visible = false; label_1_ProxyPw.Visible = false;
            textBox_1_Address.Visible = false; textBox_1_Username.Visible = false; textBox_1_ProxyPw.Visible = false; checkBox_1_Refresh.Visible = false; button_1_AddProxy.Visible = false;
            comboBox_1_YSSize.Visible = false; button_1_YSTask.Visible = false;
        }

        private void button_1_AddProfile_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(Properties.Settings.Default.locale) || String.IsNullOrWhiteSpace(Properties.Settings.Default.code))
            {
                MessageBox.Show("Please choose a locale in the 'Settings' tab.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (String.IsNullOrWhiteSpace(textBox_1_PID.Text))
            {
                MessageBox.Show("Product ID is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (Sizes.Count <= 0)
            {
                MessageBox.Show("Select at least one size to continue.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (comboBox_1_SplashMode.SelectedIndex == 1 && (Properties.Settings.Default.sessions_count + Properties.Settings.Default.r_sessions_count) == 0)
            {
                MessageBox.Show("Proxy method needs at least 1 proxy, please update your settings.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (comboBox_1_SplashMode.SelectedIndex == 2 && (Properties.Settings.Default.sessions_count + Properties.Settings.Default.r_sessions_count) == 0)
            {
                MessageBox.Show("Multi-sessions method needs at least 1 session, please update your settings." , "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if(comboBox_1_SplashMode.SelectedIndex == 2 && !warningDisplayed)
            {
                MessageBox.Show("Please note that multi session method opens by definition multiple sessions with your IP and so can get you banned.That's why we recommend you to use this method only if you have a dynamic IP or are using a VPN.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                warningDisplayed = true;
            }

            bool sitekey = (!String.IsNullOrEmpty(textBox_1_Sitekey.Text)) ? true : false;
            bool clientid = (!String.IsNullOrEmpty(textBox_1_ClientID.Text)) ? true : false;
            bool duplicate = (!String.IsNullOrEmpty(textBox_1_Duplicate.Text)) ? true : false;

            string[] row = new string[] { textBox_1_ProfileName.Text, textBox_1_PID.Text, string.Join("/ ", Sizes.Select(x => x.ToString()).ToArray()), "" };
            int rowindex = dataGridView1.Rows.Add(row);
            helpers.profiles.Add(new Profile { name = textBox_1_ProfileName.Text, ProductID = textBox_1_PID.Text, Sizes = new List<double>(this.Sizes), Sitekey = textBox_1_Sitekey.Text, ClientID = textBox_1_ClientID.Text, Duplicate = textBox_1_Duplicate.Text , captcha = sitekey, clientid = clientid, duplicate = duplicate, /*loggedin = false,*/ running = false, index = rowindex, issplash = checkBox_1_isSplash.Checked });

            helpers.SaveProfiles();

            Sizes.Clear();
        }

        private void updateRows(Profile profile)
        {
            DataGridViewRow row = dataGridView1.Rows[profile.index];
            row.Cells[0].Value = profile.name;
            row.Cells[1].Value = profile.ProductID;
            row.Cells[2].Value = string.Join("/ ", profile.Sizes.Select(x => x.ToString()).ToArray());
        }

        private void button_1_SelectSizes_Click(object sender, EventArgs e)
        {
            Form_Sizes form_sizes = new Form_Sizes(Sizes);
            form_sizes.StartPosition = FormStartPosition.CenterParent;
            form_sizes.ShowDialog();

            Sizes = new List<double>(form_sizes.sizes);
        }

        private void dataGridView1_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            int index = helpers.profiles.FindIndex(x => x.name == e.Row.Cells[0].Value.ToString() && x.ProductID == e.Row.Cells[1].Value.ToString());
            helpers.profiles.RemoveAt(index);
            helpers.SaveProfiles();
        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                currentMouseOverRow = dataGridView1.HitTest(e.X, e.Y).RowIndex;

                if (e.Button == MouseButtons.Right && currentMouseOverRow >= 0 && currentMouseOverRow <= dataGridView1.Rows.Count)
                {
                    ContextMenu m = new ContextMenu();
                    m.MenuItems.Add(new MenuItem("Show", show_Click));
                    m.MenuItems.Add(new MenuItem("Edit profile", edit_Click));
                    m.MenuItems.Add(new MenuItem("Duplicate profile", duplicate_Click));

                    if (!String.IsNullOrWhiteSpace(helpers.profiles[currentMouseOverRow].Sitekey) && helpers.profiles[currentMouseOverRow].captcha)
                        m.MenuItems.Add(new MenuItem("Solve captcha", captcha_Click));

                    m.Show(dataGridView1, new Point(e.X, e.Y));
                }
            }
            catch (Exception ex)
            {

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
                        str = str + field.Name + ": " + string.Join("/", v.Select(d => d.ToString())) + " | ";
                    }
                    else
                        str = str + field.Name + ": " + value + " | ";
                }
            }

            MessageBox.Show(str);
        }

        private void edit_Click(Object sender, System.EventArgs e)
        {
            Profile profile = helpers.profiles[currentMouseOverRow];
            Form_Edit form_edit = new Form_Edit(profile, this.helpers);
            form_edit.StartPosition = FormStartPosition.CenterParent;
            form_edit.ShowDialog();

            updateRows(profile);
        }

        private void duplicate_Click(Object sender, System.EventArgs e)
        {
            Profile profile = helpers.profiles[currentMouseOverRow];
            textBox_1_PID.Text = profile.ProductID;
            textBox_1_Sitekey.Text = profile.Sitekey;
            textBox_1_ClientID.Text = profile.ClientID;
            textBox_1_Duplicate.Text = profile.Duplicate;
            this.Sizes = new List<double>(profile.Sizes);
            checkBox_1_isSplash.Checked = profile.issplash;
        }

        private void captcha_Click(Object sender, System.EventArgs e)
        {
            Task.Run(() => helpers.getCaptcha(helpers.profiles[currentMouseOverRow]));
        }

        private async Task cart(Profile profile, DataGridViewCell cell)
        {
            DataGridViewRowCollection rows = null;

            cell.Style = new DataGridViewCellStyle { ForeColor = Color.Empty };

            if (splashmode > 0)
                rows = dataGridView2.Rows;

            string result = null;

            helpers.startSizeChecking(profile);
            result = await Task.Run(() => helpers.cart(profile, cell, rows));

            if (result.Contains("SUCCESS"))
            {
                cell.Value = "In cart!";
                cell.Style = new DataGridViewCellStyle { ForeColor = Color.Green };
            }
            else
            {
                MessageBox.Show(profile.ProductID + ": " + result, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cell.Value = "Error!";
                cell.Style = new DataGridViewCellStyle { ForeColor = Color.Red };
            }
        }

        private void button_1_Run_Click(object sender, EventArgs e)
        {
            /*if (helpers.profiles.FirstOrDefault(p => p.loggedin == true) == null)
            {
                MessageBox.Show("You must be logged-in to continue otherwise use the 'guest' mode.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }*/

            if (splashmode == 0 && helpers.profiles.FirstOrDefault(p => p.issplash) != null)
            {
                MessageBox.Show("Please select a splash mode.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (splashmode > 0 && String.IsNullOrEmpty(textBox_1_Splashurl.Text) && helpers.profiles.FirstOrDefault(p => p.issplash) != null)
            {
                MessageBox.Show("Please enter a valid splash url.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            for (int i = 0; i < helpers.profiles.Count; i++)
            {
                Profile profile = helpers.profiles[i];
                DataGridViewRow row = dataGridView1.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[0].Value.ToString() == profile.name && r.Cells[1].Value.ToString() == profile.ProductID).First();
                if (String.IsNullOrWhiteSpace(row.Cells[3].Value.ToString()) || row.Cells[3].Value.ToString().Contains("Error") || row.Cells[3].Style.ForeColor == Color.Red /*|| row.Cells[3].Value == "Logged in!" && profile.loggedin*/)
                {
                    if (!profile.running)
                    {
                        this.SplashUrl = textBox_1_Splashurl.Text;
                        cart(profile, row.Cells[3]);
                    }
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        private void homeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel_Home.Visible = true; panel_Settings.Visible = false;
        }

        private void proxyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel_Home.Visible = false; panel_Settings.Visible = true;
        }

        private async Task getInventory()
        {
            listBox_2_Inventory.Items.Add(String.Format("Getting inventory for product '{0}' ...", Properties.Settings.Default.pid));

            string url = String.Format("http://www.{0}/on/demandware.store/Sites-adidas-{1}-Site/{2}/Product-GetVariants?pid={3}", Properties.Settings.Default.locale, Properties.Settings.Default.code, helpers.marketsList[Properties.Settings.Default.code], Properties.Settings.Default.pid);
            string pipename = System.Diagnostics.Process.GetCurrentProcess().Id.ToString() + "_sizechecking_1337";

            var pipe = new System.IO.Pipes.NamedPipeServerStream(pipename, System.IO.Pipes.PipeDirection.InOut, 1);
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo();
            process.StartInfo.FileName = "3s_atc - browser.exe";
            process.StartInfo.Arguments = url + " " + pipename;
            process.Start();

            Dictionary<string, Dictionary<string, string>> products = await Task.Run(() => helpers.getInventory(pipe));
            listBox_2_Inventory.Items.Clear();

            foreach (KeyValuePair<string, Dictionary<string, string>> entry in products)
                listBox_2_Inventory.Items.Add(String.Format("{0} - Size: {1} - Quantity: {2}", entry.Key, products[entry.Key]["size"], products[entry.Key]["stockcount"]));

            if (listBox_2_Inventory.Items.Count > 0 && listBox_2_Inventory.Items[1].ToString().Contains("Quantity"))
                MessageBox.Show("Done!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                listBox_2_Inventory.Items.Add("Error!");
                MessageBox.Show("Error while getting inventory!", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button_2_Check_Click(object sender, EventArgs e)
        {
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

            Properties.Settings.Default.sessions_count = Convert.ToInt32(numericUpDown_Sessions.Value);
            Properties.Settings.Default.r_sessions_count = Convert.ToInt32(numericUpDown_RSessions.Value);
            Properties.Settings.Default.refresh_interval = Convert.ToInt32(numericUpDown_3_RefreshInterval.Value);
            Properties.Settings.Default.splashidentifier = textBox_3_SplashIdentifier.Text;
            Properties.Settings.Default.productpageidentifier = textBox_3_ProductPageIdentifier.Text;
            Properties.Settings.Default.cartbrowser = comboBox_2_CartBrowser.Text;
            Properties.Settings.Default.Save();
        }

        private void button_1_AddProxy_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textBox_1_Address.Text))
            {
                MessageBox.Show("Proxy address is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (helpers.proxylist.FindIndex(x => x.address == textBox_1_Address.Text) != -1)
            {
                MessageBox.Show("Proxy already in list.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool auth = (!String.IsNullOrEmpty(textBox_1_Username.Text) && !String.IsNullOrEmpty(textBox_1_ProxyPw.Text)) ? true : false;

            helpers.proxylist.Add(new C_Proxy { address = textBox_1_Address.Text, username = textBox_1_Username.Text, password = textBox_1_ProxyPw.Text, auth = auth, refresh = checkBox_1_Refresh.Checked });

            if(auth)
                dataGridView2.Rows.Add(new string[] { textBox_1_Address.Text + "@" + textBox_1_Username.Text, checkBox_1_Refresh.Checked.ToString(), "" });
            else
                dataGridView2.Rows.Add(new string[] { textBox_1_Address.Text, checkBox_1_Refresh.Checked.ToString(), "" });

            helpers.SaveProxyList();
        }

        private void dataGridView2_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            int index = 0;

            if(e.Row.Cells[0].Value.ToString().Contains("@"))
                index = helpers.proxylist.FindIndex(x => x.address.Contains(e.Row.Cells[0].Value.ToString().Split('@')[0]) && x.username.ToString().Contains(e.Row.Cells[0].Value.ToString().Split('@')[1]));
            else
                index = helpers.proxylist.FindIndex(x => x.address.Contains(e.Row.Cells[0].Value.ToString()) && String.IsNullOrEmpty(x.username));
            
            helpers.proxylist.RemoveAt(index);
            helpers.SaveProxyList();
        }

        private void button_3_Browse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (!String.IsNullOrEmpty(Properties.Settings.Default.www_path))
                dialog.SelectedPath = Properties.Settings.Default.www_path;
            else
                dialog.SelectedPath = "c:\\";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.www_path = dialog.SelectedPath;
                Properties.Settings.Default.Save();
            }
        }

        private void dataGridView2_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                currentMouseOverRow2 = dataGridView2.HitTest(e.X, e.Y).RowIndex;

                if (e.Button == MouseButtons.Right && currentMouseOverRow2 >= 0 && currentMouseOverRow2 <= dataGridView2.Rows.Count)
                {
                    ContextMenu m = new ContextMenu();

                    if (!dataGridView2.Rows[currentMouseOverRow2].Cells[0].Value.ToString().Contains("session") && !dataGridView2.Rows[currentMouseOverRow2].Cells[0].Value.ToString().Contains("YS"))
                        m.MenuItems.Add(new MenuItem("Edit proxy", editProxy_Click));
                    if (dataGridView2.Rows[currentMouseOverRow2].Cells[2].Value.ToString() == "PRODUCT PAGE - EXTRACTED SESSION")
                        m.MenuItems.Add(new MenuItem("Session info", sessionInfo_Click));
                    if (dataGridView2.Rows[currentMouseOverRow2].Cells[2].Value.ToString() != "Setting up..." || !String.IsNullOrEmpty(dataGridView2.Rows[currentMouseOverRow2].Cells[2].Value.ToString()))
                    {
                        if ((helpers.sessionlist.Count > 0 && !helpers.sessionlist[currentMouseOverRow2].browser_visible) || (dataGridView2.Rows[currentMouseOverRow2].Cells[0].Value.ToString().Contains("YS") && !helpers.ys_tasks[currentMouseOverRow2].browser_visible))
                            m.MenuItems.Add(new MenuItem("Show browser", showHideBrowser_Click));
                        else
                            m.MenuItems.Add(new MenuItem("Hide browser", showHideBrowser_Click));
                    }

                    m.Show(dataGridView2, new Point(e.X, e.Y));
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void editProxy_Click(Object sender, System.EventArgs e)
        {
            int index = currentMouseOverRow2;
            C_Proxy proxy = helpers.proxylist[index];
            Form_ProxyEdit form_proxyedit = new Form_ProxyEdit(proxy, this.helpers);
            form_proxyedit.StartPosition = FormStartPosition.CenterParent;
            form_proxyedit.ShowDialog();

            updateProxyRows(proxy, index);
        }

        private void showHideBrowser_Click(Object sender, System.EventArgs e)
        {
            int index = currentMouseOverRow2;

            if (dataGridView2.Rows[index].Cells[0].Value.ToString().Contains("YS"))
            {
                if (helpers.ys_tasks.Count > 0)
                    helpers.ys_tasks[index].hideShow();
            }
            else
            {
                if (helpers.sessionlist.Count > 0)
                    helpers.sessionlist[index].hideShow();
                else
                    helpers.proxylist[index].hideShow();
            }
        }

        private void sessionInfo_Click(Object sender, System.EventArgs e)
        {
            int index = currentMouseOverRow2;

            C_Session session = helpers.sessionlist[index];
            string infos = "Press CTRL+C to copy :\n\nHMAC Cookie : Name=" + session.hmac_cookie.name + "       Value=" + session.hmac_cookie.value + "\nSitekey: " + session.sitekey + "\nClient ID: " + session.clientid + "\nDuplicate: " + session.duplicate;
            MessageBox.Show(infos, "Session info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void updateProxyRows(C_Proxy proxy, int index)
        {
            DataGridViewRow row = dataGridView2.Rows[index];
            row.Cells[0].Value = proxy.address;
            row.Cells[1].Value = proxy.refresh.ToString();
            row.Cells[2].Value = proxy.auth.ToString();
            row.Cells[3].Value = proxy.username;
        }

        /*private void button_1_Login_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                for (int i = 0; i < helpers.profiles.Count; i++)
                {
                    int index = i;
                    Profile profile = helpers.profiles[index];
                    DataGridViewRow row = dataGridView1.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[0].Value.ToString() == profile.Email && r.Cells[1].Value.ToString() == profile.ProductID).First();


                    Task.Run(() => helpers.login(profile, row.Cells[3], null));
                    System.Threading.Thread.Sleep(1000);
                }
            });
        }*/

        private void guestMode_cart(Profile profile)
        {
            this.SplashUrl = textBox_1_Splashurl.Text;
            Task.Run(() => helpers.guestMode_Cart(profile, dataGridView2.Rows));
        }

        private void button_1_RunGuest_Click(object sender, EventArgs e)
        {
            if (guestmode)
            {
                MessageBox.Show("Guest mode already running!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (String.IsNullOrEmpty(textBox_1_Splashurl.Text) && comboBox_1_SplashMode.SelectedIndex < 3)
            {
                MessageBox.Show("Please enter a splash url.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (comboBox_1_SplashMode.SelectedIndex == 0)
            {
                MessageBox.Show("Please select a splash mode.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (comboBox_1_SplashMode.SelectedIndex == 1 && (Properties.Settings.Default.sessions_count + Properties.Settings.Default.r_sessions_count) == 0)
            {
                MessageBox.Show("Proxy method needs at least 1 proxy, please update your settings.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (comboBox_1_SplashMode.SelectedIndex == 2 && (Properties.Settings.Default.sessions_count + Properties.Settings.Default.r_sessions_count) == 0)
            {
                MessageBox.Show("Multi-sessions method needs at least 1 session, please update your settings.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (comboBox_1_SplashMode.SelectedIndex == 3 && helpers.ys_tasks.Count == 0)
            {
                MessageBox.Show("Add at leat 1 size to conitnue.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            

            guestmode = true;

            if (comboBox_1_SplashMode.SelectedIndex == 3)
            {
                foreach(C_YS ys in helpers.ys_tasks)
                    Task.Run(() => helpers.yeezySupply_Cart(ys, dataGridView2.Rows[ys.index]));
            }
            else
            {
                Profile profile = new Profile();
                guestMode_cart(profile);
            }
        }

        private void button_1_LoginGmail_Click(object sender, EventArgs e)
        {
            if(helpers.gmail_loggedin)
            {
                MessageBox.Show("Already logged in!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Task.Run(() => helpers.LoginGmail());
        }

        private void comboBox_1_SplashMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.splashmode = comboBox_1_SplashMode.SelectedIndex;
            switch (splashmode)
            {
                case 0:
                    label_3_SCount.Visible = false; label_3_SRCount.Visible = false;
                    numericUpDown_Sessions.Visible = false; numericUpDown_RSessions.Visible = false;
                    label_1_ProxyAddress.Visible = false; label_1_Username.Visible = false; label_1_ProxyPw.Visible = false;
                    textBox_1_Address.Visible = false; textBox_1_Username.Visible = false; textBox_1_ProxyPw.Visible = false; checkBox_1_Refresh.Visible = false; button_1_AddProxy.Visible = false;
                    comboBox_1_YSSize.Visible = false; button_1_YSTask.Visible = false; label_1_size.Visible = false;
                    break;
                case 1:
                    label_3_SCount.Visible = false; label_3_SRCount.Visible = false;
                    numericUpDown_Sessions.Visible = false; numericUpDown_RSessions.Visible = false;
                    label_1_ProxyAddress.Visible = true; label_1_Username.Visible = true; label_1_ProxyPw.Visible = true;
                    textBox_1_Address.Visible = true; textBox_1_Username.Visible = true; textBox_1_ProxyPw.Visible = true; checkBox_1_Refresh.Visible = true; button_1_AddProxy.Visible = true;
                    comboBox_1_YSSize.Visible = false; button_1_YSTask.Visible = false; label_1_size.Visible = false;                
                    break;
                case 2:
                    label_3_SCount.Visible = true; label_3_SRCount.Visible = true;
                    numericUpDown_Sessions.Visible = true; numericUpDown_RSessions.Visible = true;
                    label_1_ProxyAddress.Visible = false; label_1_Username.Visible = false; label_1_ProxyPw.Visible = false;
                    textBox_1_Address.Visible = false; textBox_1_Username.Visible = false; textBox_1_ProxyPw.Visible = false; checkBox_1_Refresh.Visible = false; button_1_AddProxy.Visible = false;
                    comboBox_1_YSSize.Visible = false; button_1_YSTask.Visible = false; label_1_size.Visible = false;
                    break;
                case 3:
                    label_3_SCount.Visible = false; label_3_SRCount.Visible = false;
                    numericUpDown_Sessions.Visible = false; numericUpDown_RSessions.Visible = false;
                    label_1_ProxyAddress.Visible = false; label_1_Username.Visible = false; label_1_ProxyPw.Visible = false;
                    textBox_1_Address.Visible = false; textBox_1_Username.Visible = false; textBox_1_ProxyPw.Visible = false; checkBox_1_Refresh.Visible = false; button_1_AddProxy.Visible = false;
                    comboBox_1_YSSize.Visible = true; button_1_YSTask.Visible = true; label_1_size.Visible = true;
                    break;
            }
        }

        private void numericUpDown_RSessions_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.r_sessions_count = Convert.ToInt32(numericUpDown_RSessions.Value);
            Properties.Settings.Default.Save();
        }

        private void numericUpDown_Sessions_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.sessions_count = Convert.ToInt32(numericUpDown_Sessions.Value);
            Properties.Settings.Default.Save();
        }

        private void button_1_YSTask_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(comboBox_1_YSSize.SelectedItem.ToString()))
            {
                MessageBox.Show("Select a size to continue.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string[] row = new string[] { "YS - " + comboBox_1_YSSize.SelectedItem.ToString(), null, null };
            int rowindex = dataGridView2.Rows.Add(row);
            helpers.ys_tasks.Add(new C_YS { index = rowindex, size = comboBox_1_YSSize.SelectedItem.ToString().Split(null)[1] });
        }
    }
}
