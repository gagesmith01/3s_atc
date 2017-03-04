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

        public Form1()
        {
            InitializeComponent();

            addLog("Welcome to 3s_atc!", Color.Empty);

            helpers = new Helpers();
            Sizes = new List<double>();
            warningDisplayed = false;

            if (Properties.Settings.Default.profiles.Length > 0)
                helpers.profiles = helpers.LoadProfiles();

            if (Properties.Settings.Default.proxylist.Length > 0)
                helpers.proxylist = helpers.LoadProxyList();

            foreach (Profile profile in helpers.profiles)
            {
                dataGridView1.Rows.Add(new string[] { profile.Email, profile.ProductID, string.Join("/ ", profile.Sizes.Select(x => x.ToString()).ToArray()), profile.Sitekey, profile.ClientID, profile.Duplicate, string.Join(";", profile.ExtraCookies.Select(kv => kv.Key + "=" + kv.Value).ToArray()), profile.SplashUrl, "" });
                profile.loggedin = false;
            }

            foreach (C_Proxy proxy in helpers.proxylist)
                dataGridView2.Rows.Add(new string[] { proxy.address, proxy.refresh.ToString(), proxy.auth.ToString(), proxy.username, null, null, null, null });

            for (int i = 0; i < comboBox_3_Website.Items.Count; i++)
            {
                if (comboBox_3_Website.GetItemText(comboBox_3_Website.Items[i]) == string.Format("{0} - {1}", Properties.Settings.Default.code, Properties.Settings.Default.locale))
                {
                    comboBox_3_Website.SelectedItem = comboBox_3_Website.Items[i]; break;
                }
            }

            comboBox_1_SplashMode.SelectedIndex = 0;
            numericUpDown_Sessions.Value = Properties.Settings.Default.sessions_count; numericUpDown_RSessions.Value = Properties.Settings.Default.r_sessions_count;
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

            if (String.IsNullOrWhiteSpace(Properties.Settings.Default.locale) || String.IsNullOrWhiteSpace(Properties.Settings.Default.code))
            {
                MessageBox.Show("Please choose a locale in the 'Settings' tab.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (String.IsNullOrWhiteSpace(textBox_1_Email.Text) || String.IsNullOrWhiteSpace(textBox_1_Password.Text) || String.IsNullOrWhiteSpace(textBox_1_PID.Text))
            {
                MessageBox.Show("Email, password or product ID empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (String.IsNullOrWhiteSpace(textBox_1_Sitekey.Text) && checkBox_1_Captcha.Checked || String.IsNullOrWhiteSpace(textBox_1_ClientID.Text) && checkBox_1_ClientID.Checked || String.IsNullOrWhiteSpace(textBox_1_Duplicate.Text) && checkBox_1_Duplicate.Checked || String.IsNullOrWhiteSpace(textBox_1_Splashurl.Text) && comboBox_1_SplashMode.SelectedIndex > 0)
            {
                MessageBox.Show("Captcha/Duplicate/Splash page checked but fields are empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (Sizes.Count <= 0)
            {
                MessageBox.Show("Select at least one size to continue.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (helpers.profiles.FindIndex(x => x.Email == textBox_1_Email.Text && x.ProductID == textBox_1_PID.Text) != -1)
            {
                MessageBox.Show(String.Format("E-mail already in use for product ID '{0}'.", textBox_1_PID.Text), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (comboBox_1_SplashMode.SelectedIndex > 0 && (Properties.Settings.Default.sessions_count == 0 || Properties.Settings.Default.r_sessions_count == 0))
            {
                MessageBox.Show("Multi-sessions method needs at least 1 session, please update your settings." , "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if(comboBox_1_SplashMode.SelectedIndex > 0 && !warningDisplayed)
            {
                MessageBox.Show("Please note that multi session method opens by definition multiple sessions with your IP and so can get you banned.That's why we recommend you to use this method only if you have a dynamic IP or are using a VPN.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                warningDisplayed = true;
            }

            sitekey = (checkBox_1_Captcha.Checked) ? textBox_1_Sitekey.Text : null;
            clientid = (checkBox_1_ClientID.Checked) ? textBox_1_ClientID.Text : null;
            duplicate = (checkBox_1_Duplicate.Checked) ? textBox_1_Duplicate.Text : null;
            if (comboBox_1_SplashMode.SelectedIndex > 0) splash_url = textBox_1_Splashurl.Text; else splash_url = null;


            string[] row = new string[] { textBox_1_Email.Text, textBox_1_PID.Text, string.Join("/ ", Sizes.Select(x => x.ToString()).ToArray()), sitekey, clientid, duplicate, richTextBox_1_Cookies.Text, splash_url, "" };
            int rowindex = dataGridView1.Rows.Add(row);
            helpers.profiles.Add(new Profile { Email = textBox_1_Email.Text, Password = textBox_1_Password.Text, ProductID = textBox_1_PID.Text, Sizes = new List<double>(this.Sizes), Sitekey = sitekey, ClientID = clientid, Duplicate = duplicate, ExtraCookies = helpers.splitCookies(richTextBox_1_Cookies.Text), SplashUrl = splash_url, captcha = checkBox_1_Captcha.Checked, clientid = checkBox_1_ClientID.Checked, duplicate = checkBox_1_Duplicate.Checked, splashmode = comboBox_1_SplashMode.SelectedIndex, loggedin = false, running = false, index = rowindex });

            helpers.SaveProfiles();

            Sizes.Clear();
        }

        private void updateRows(Profile profile)
        {
            DataGridViewRow row = dataGridView1.Rows[profile.index];
            row.Cells[0].Value = profile.Email;
            row.Cells[1].Value = profile.ProductID;
            row.Cells[2].Value = string.Join("/ ", profile.Sizes.Select(x => x.ToString()).ToArray());
            row.Cells[3].Value = profile.Sitekey;
            row.Cells[4].Value = profile.ClientID;
            row.Cells[5].Value = profile.Duplicate;
            row.Cells[6].Value = string.Join(";", profile.ExtraCookies.Select(m => m.Key + "=" + m.Value).ToArray());
            row.Cells[7].Value = profile.SplashUrl;
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
            int index = helpers.profiles.FindIndex(x => x.Email == e.Row.Cells[0].Value.ToString() && x.ProductID == e.Row.Cells[1].Value.ToString());
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
        private void captcha_Click(Object sender, System.EventArgs e)
        {
            Task.Run(() => helpers.getCaptcha(helpers.profiles[currentMouseOverRow]));
        }
        private async Task cart(Profile profile, DataGridViewCell cell, DataGridViewRowCollection rows = null)
        {
            addLog(String.Format("{0} : started process for product '{1}'", profile.Email, profile.ProductID), Color.Empty);
            addLog(String.Format("{0} : logging in...", profile.Email, profile.ProductID), Color.Empty);

            cell.Style = new DataGridViewCellStyle { ForeColor = Color.Empty };

            string result = await Task.Run(() => helpers.cart(profile, cell, rows));
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
            for (int i = 0; i < helpers.profiles.Count; i++)
            {
                Profile profile = helpers.profiles[i];
                DataGridViewRow row = dataGridView1.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[0].Value.ToString() == profile.Email && r.Cells[1].Value.ToString() == profile.ProductID).First();
                if (String.IsNullOrWhiteSpace(row.Cells[8].Value.ToString()) || row.Cells[8].Value.ToString().Contains("Error") || row.Cells[8].Style.ForeColor == Color.Red || row.Cells[8].Value == "Logged in!" && profile.loggedin)
                {
                    if (!profile.running)
                    {
                        if (profile.splashmode > 0)
                            cart(profile, row.Cells[8], dataGridView2.Rows);
                        else
                            cart(profile, row.Cells[8]);
                    }
                }

                System.Threading.Thread.Sleep(250);
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

        private async Task getInventory()
        {
            listBox_2_Inventory.Items.Add(String.Format("Getting inventory for product '{0}' ...", Properties.Settings.Default.pid));
            Dictionary<string, Dictionary<string, string>> products = await Task.Run(() => helpers.getInventory(Properties.Settings.Default.pid, null));
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

            Properties.Settings.Default.sessions_count = Convert.ToInt32(numericUpDown_Sessions.Value);
            Properties.Settings.Default.r_sessions_count = Convert.ToInt32(numericUpDown_RSessions.Value);

            Properties.Settings.Default.Save();
        }

        private void button_3_Add_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textBox_3_Address.Text))
            {
                MessageBox.Show("Proxy address is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (helpers.proxylist.FindIndex(x => x.address == textBox_3_Address.Text && x.auth == checkBox_3_Auth.Checked) != -1)
            {
                MessageBox.Show("Proxy already in list.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            helpers.proxylist.Add(new C_Proxy { address = textBox_3_Address.Text, username = textBox_3_Username.Text, password = textBox_3_Password.Text, hmac = null, sitekey = null, auth = checkBox_3_Auth.Checked, refresh = checkBox_3_Bypass.Checked });

            dataGridView2.Rows.Add(new string[] { textBox_3_Address.Text, checkBox_3_Bypass.Checked.ToString(), checkBox_3_Auth.Checked.ToString(), textBox_3_Username.Text, null, null, null, null });

            helpers.SaveProxyList();
        }

        private void dataGridView2_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            int index = helpers.proxylist.FindIndex(x => x.address.Contains(e.Row.Cells[0].Value.ToString()) && x.auth.ToString().Contains(e.Row.Cells[2].Value.ToString()));
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

                    if (dataGridView2.Rows[currentMouseOverRow2].Cells[0].Value != "session")
                        m.MenuItems.Add(new MenuItem("Edit proxy", editProxy_Click));
                    else if (dataGridView2.Rows[currentMouseOverRow2].Cells[0].Value == "session" && (dataGridView2.Rows[currentMouseOverRow2].Cells[8].Value == "Splash page passed!" || dataGridView2.Rows[currentMouseOverRow2].Cells[5].Value != null))
                        m.MenuItems.Add(new MenuItem("Transfer session", transferSession_Click));

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

        private void transferSession_Click(Object sender, System.EventArgs e)
        {
            int index = currentMouseOverRow2;
            C_Session session = helpers.sessionlist[index];
            helpers.transferSession(session);
        }

        private void updateProxyRows(C_Proxy proxy, int index)
        {
            DataGridViewRow row = dataGridView2.Rows[index];
            row.Cells[0].Value = proxy.address;
            row.Cells[1].Value = proxy.refresh.ToString();
            row.Cells[2].Value = proxy.auth.ToString();
            row.Cells[3].Value = proxy.username;
        }

        private void button_1_Login_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                for (int i = 0; i < helpers.profiles.Count; i++)
                {
                    int index = i;
                    Profile profile = helpers.profiles[index];
                    DataGridViewRow row = dataGridView1.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[0].Value.ToString() == profile.Email && r.Cells[1].Value.ToString() == profile.ProductID).First();

                    Task.Run(() => helpers.login(profile, row.Cells[8], null));
                    System.Threading.Thread.Sleep(500);
                }
            });
        }
    }
}
