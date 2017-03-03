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
    public partial class Form_Edit : Form
    {
        private Profile profile;
        private List<double> sizes;
        private Helpers helpers;

        public Form_Edit(Profile profile, Helpers helpers)
        {
            InitializeComponent();

            this.profile = profile;
            this.helpers = helpers;

            sizes = profile.Sizes;

            textBox_ProductID.Text = profile.ProductID; textBox_Sitekey.Text = profile.Sitekey; textBox_ClientID.Text = profile.ClientID; textBox_Duplicate.Text = profile.Duplicate;
            textBox_Email.Text = profile.Email; textBox_Password.Text = profile.Password; richTextBox_Cookies.Text = string.Join(";", profile.ExtraCookies.Select(m => m.Key + "=" + m.Value).ToArray()); textBox_SplashUrl.Text = profile.SplashUrl;
            checkBox_Captcha.Checked = profile.captcha; checkBox_ClientID.Checked = profile.clientid; checkBox_Duplicate.Checked = profile.duplicate; comboBox_SplashMode.SelectedIndex = profile.splashmode;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form_Sizes form_sizes = new Form_Sizes(sizes);
            form_sizes.StartPosition = FormStartPosition.CenterParent;
            form_sizes.ShowDialog();

            this.sizes = form_sizes.sizes;
        }

        private void button_Update_Click(object sender, EventArgs e)
        {
            string sitekey, clientid, duplicate, splash_url;
            sitekey = (checkBox_Captcha.Checked) ? textBox_Sitekey.Text : null;
            clientid = (checkBox_ClientID.Checked) ? textBox_ClientID.Text : null;
            duplicate = (checkBox_Duplicate.Checked) ? textBox_Duplicate.Text : null;
            if (comboBox_SplashMode.SelectedIndex > 0) splash_url = textBox_SplashUrl.Text; else splash_url = null;

            profile.ProductID = textBox_ProductID.Text; profile.Sitekey = sitekey; profile.ClientID = clientid; profile.Duplicate = duplicate;
            profile.Email = textBox_Email.Text; profile.Password = textBox_Password.Text; profile.Sizes = sizes; profile.ExtraCookies = helpers.splitCookies(richTextBox_Cookies.Text); profile.SplashUrl = splash_url;
            profile.captcha = checkBox_Captcha.Checked; profile.clientid = checkBox_ClientID.Checked; profile.duplicate = checkBox_Duplicate.Checked; profile.splashmode = comboBox_SplashMode.SelectedIndex;
            
            helpers.SaveProfiles();

            this.Close();
        }
    }
}
