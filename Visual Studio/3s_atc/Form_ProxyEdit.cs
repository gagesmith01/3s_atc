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
    public partial class Form_ProxyEdit : Form
    {
        private C_Proxy proxy;
        private Helpers helpers;

        public Form_ProxyEdit(C_Proxy proxy, Helpers helpers)
        {
            InitializeComponent();

            this.proxy = proxy;
            this.helpers = helpers;

            textBox_Address.Text = proxy.address;

            if(proxy.auth)
            {
                textBox_Username.Text = proxy.username;
                textBox_Password.Text = proxy.password;
            }

            checkBox_Auth.Checked = proxy.auth;
            checkBox_Bypass.Checked = proxy.refresh;
        }

        private void button_Update_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textBox_Address.Text))
            {
                MessageBox.Show("Proxy address is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (helpers.proxylist.FindIndex(x => x.address == textBox_Address.Text && x.auth == checkBox_Auth.Checked && x.username == textBox_Username.Text && x.password == textBox_Password.Text) != -1)
            {
                MessageBox.Show("Proxy already in list.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            proxy.address = textBox_Address.Text;
            proxy.auth = checkBox_Auth.Checked;

            if(checkBox_Auth.Checked)
            {
                proxy.username = textBox_Username.Text;
                proxy.password = textBox_Password.Text;
            }
            else 
            {
                proxy.username = null;
                proxy.password = null;
            }

            proxy.refresh = checkBox_Bypass.Checked;

            helpers.SaveProxyList();
            this.Close();
        }
    }
}
