namespace _3s_atc
{
    partial class Form_ProxyEdit
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_ProxyEdit));
            this.label_Address = new System.Windows.Forms.Label();
            this.textBox_Address = new System.Windows.Forms.TextBox();
            this.label_Username = new System.Windows.Forms.Label();
            this.textBox_Username = new System.Windows.Forms.TextBox();
            this.textBox_Password = new System.Windows.Forms.TextBox();
            this.label_Password = new System.Windows.Forms.Label();
            this.checkBox_Auth = new System.Windows.Forms.CheckBox();
            this.checkBox_Bypass = new System.Windows.Forms.CheckBox();
            this.button_Update = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label_Address
            // 
            this.label_Address.AutoSize = true;
            this.label_Address.Location = new System.Drawing.Point(7, 14);
            this.label_Address.Name = "label_Address";
            this.label_Address.Size = new System.Drawing.Size(54, 13);
            this.label_Address.TabIndex = 0;
            this.label_Address.Text = "Address : ";
            // 
            // textBox_Address
            // 
            this.textBox_Address.Location = new System.Drawing.Point(58, 11);
            this.textBox_Address.Name = "textBox_Address";
            this.textBox_Address.Size = new System.Drawing.Size(148, 20);
            this.textBox_Address.TabIndex = 1;
            // 
            // label_Username
            // 
            this.label_Username.AutoSize = true;
            this.label_Username.Location = new System.Drawing.Point(218, 14);
            this.label_Username.Name = "label_Username";
            this.label_Username.Size = new System.Drawing.Size(64, 13);
            this.label_Username.TabIndex = 2;
            this.label_Username.Text = "Username : ";
            // 
            // textBox_Username
            // 
            this.textBox_Username.Location = new System.Drawing.Point(282, 11);
            this.textBox_Username.Name = "textBox_Username";
            this.textBox_Username.Size = new System.Drawing.Size(121, 20);
            this.textBox_Username.TabIndex = 3;
            // 
            // textBox_Password
            // 
            this.textBox_Password.Location = new System.Drawing.Point(485, 11);
            this.textBox_Password.Name = "textBox_Password";
            this.textBox_Password.Size = new System.Drawing.Size(121, 20);
            this.textBox_Password.TabIndex = 5;
            // 
            // label_Password
            // 
            this.label_Password.AutoSize = true;
            this.label_Password.Location = new System.Drawing.Point(421, 14);
            this.label_Password.Name = "label_Password";
            this.label_Password.Size = new System.Drawing.Size(62, 13);
            this.label_Password.TabIndex = 4;
            this.label_Password.Text = "Password : ";
            // 
            // checkBox_Auth
            // 
            this.checkBox_Auth.AutoSize = true;
            this.checkBox_Auth.Location = new System.Drawing.Point(622, 13);
            this.checkBox_Auth.Name = "checkBox_Auth";
            this.checkBox_Auth.Size = new System.Drawing.Size(94, 17);
            this.checkBox_Auth.TabIndex = 6;
            this.checkBox_Auth.Text = "Authentication";
            this.checkBox_Auth.UseVisualStyleBackColor = true;
            // 
            // checkBox_Bypass
            // 
            this.checkBox_Bypass.AutoSize = true;
            this.checkBox_Bypass.Location = new System.Drawing.Point(728, 13);
            this.checkBox_Bypass.Name = "checkBox_Bypass";
            this.checkBox_Bypass.Size = new System.Drawing.Size(99, 17);
            this.checkBox_Bypass.TabIndex = 7;
            this.checkBox_Bypass.Text = "Refresh bypass";
            this.checkBox_Bypass.UseVisualStyleBackColor = true;
            // 
            // button_Update
            // 
            this.button_Update.Location = new System.Drawing.Point(334, 48);
            this.button_Update.Name = "button_Update";
            this.button_Update.Size = new System.Drawing.Size(175, 23);
            this.button_Update.TabIndex = 8;
            this.button_Update.Text = "Update";
            this.button_Update.UseVisualStyleBackColor = true;
            this.button_Update.Click += new System.EventHandler(this.button_Update_Click);
            // 
            // Form_ProxyEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(831, 79);
            this.Controls.Add(this.button_Update);
            this.Controls.Add(this.checkBox_Bypass);
            this.Controls.Add(this.checkBox_Auth);
            this.Controls.Add(this.textBox_Password);
            this.Controls.Add(this.label_Password);
            this.Controls.Add(this.textBox_Username);
            this.Controls.Add(this.label_Username);
            this.Controls.Add(this.textBox_Address);
            this.Controls.Add(this.label_Address);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form_ProxyEdit";
            this.Text = "Edit Proxy";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_Address;
        private System.Windows.Forms.TextBox textBox_Address;
        private System.Windows.Forms.Label label_Username;
        private System.Windows.Forms.TextBox textBox_Username;
        private System.Windows.Forms.TextBox textBox_Password;
        private System.Windows.Forms.Label label_Password;
        private System.Windows.Forms.CheckBox checkBox_Auth;
        private System.Windows.Forms.CheckBox checkBox_Bypass;
        private System.Windows.Forms.Button button_Update;
    }
}