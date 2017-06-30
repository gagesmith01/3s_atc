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
            textBox_Email.Text = profile.Email; textBox_Password.Text = profile.Password;
            checkBox_isSplash.Checked = profile.issplash;
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
            bool sitekey = (!String.IsNullOrEmpty(textBox_Sitekey.Text)) ? true : false;
            bool clientid = (!String.IsNullOrEmpty(textBox_ClientID.Text)) ? true : false;
            bool duplicate = (!String.IsNullOrEmpty(textBox_Duplicate.Text)) ? true : false;

            profile.ProductID = textBox_ProductID.Text; profile.Sitekey = textBox_Sitekey.Text; profile.ClientID = textBox_ClientID.Text; profile.Duplicate = textBox_Duplicate.Text;
            profile.Email = textBox_Email.Text; profile.Password = textBox_Password.Text; profile.Sizes = sizes;
            profile.captcha = sitekey; profile.clientid = clientid; profile.duplicate = duplicate; profile.issplash = checkBox_isSplash.Checked;
            
            helpers.SaveProfiles();

            this.Close();
        }
    }
}
