using MS_Project_Import_Export;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MS_Project_Import_Export
{
    public partial class LoginForm : Form
    {
        private string urlOnEnter = string.Empty;

        public string InnovatorUrl
        {
            get
            {
                return urlTextBox.Text.Trim();
            }
        }
        public string DataBase
        {
            get
            {
                return databaseComboBox.Text;
            }
        }
        public string UserName
        {
            get
            {
                return userTextBox.Text;
            }
        }
        public string Password
        {
            get
            {
                return passwodTextBox.Text;
            }
        }

        public LoginForm(string innovatorUrl, string database, List<string> databasesList, string username)
        {
            InitializeComponent();

            urlTextBox.Text = innovatorUrl;
            userTextBox.Text = username;

            if (!string.IsNullOrEmpty(innovatorUrl))
            {
                passwodTextBox.Select();
            }

            databaseComboBox.Items.AddRange(databasesList.ToArray());
            if (databasesList.Contains(database))
            {
                databaseComboBox.SelectedItem = database;
            }
        }

        private void urlTextBox_Enter(object sender, EventArgs e)
        {
            urlOnEnter = urlTextBox.Text;
        }

        private void databaseComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            userTextBox.Enabled = true;
            passwodTextBox.Enabled = true;
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btn_getDatabases_Click(object sender, EventArgs e)
        {
            var url = urlTextBox.Text.Trim();
            if (!urlOnEnter.Equals(url, StringComparison.CurrentCulture))
            {
                databaseComboBox.Items.Clear();
                databaseComboBox.Text = string.Empty;

                if (!url.StartsWith(@"http://"))
                {
                    url = @"http://" + url;
                    urlTextBox.Text = url;
                }

                List<string> dataBases = InnovatorManager.Instance.GetDataBases(url);
                if (dataBases != null)
                {
                    foreach (string database in dataBases)
                    {
                        databaseComboBox.Items.Add(database);
                    }
                }

                if (databaseComboBox.Items.Count > 0)
                {
                    databaseComboBox.SelectedIndex = 0;
                }
            }
        }
    }
}