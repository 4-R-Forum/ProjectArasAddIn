namespace MS_Project_Import_Export
{
    partial class LoginForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            this.urlTextBox = new System.Windows.Forms.TextBox();
            this.urllabel = new System.Windows.Forms.Label();
            this.dataBaseName = new System.Windows.Forms.Label();
            this.databaseComboBox = new System.Windows.Forms.ComboBox();
            this.userLabel = new System.Windows.Forms.Label();
            this.userTextBox = new System.Windows.Forms.TextBox();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.passwodTextBox = new System.Windows.Forms.TextBox();
            this.loginButton = new System.Windows.Forms.Button();
            this.btn_getDatabases = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // urlTextBox
            // 
            resources.ApplyResources(this.urlTextBox, "urlTextBox");
            this.urlTextBox.Name = "urlTextBox";
            this.urlTextBox.Enter += new System.EventHandler(this.urlTextBox_Enter);
            // 
            // urllabel
            // 
            resources.ApplyResources(this.urllabel, "urllabel");
            this.urllabel.Name = "urllabel";
            // 
            // dataBaseName
            // 
            resources.ApplyResources(this.dataBaseName, "dataBaseName");
            this.dataBaseName.Name = "dataBaseName";
            // 
            // databaseComboBox
            // 
            resources.ApplyResources(this.databaseComboBox, "databaseComboBox");
            this.databaseComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.databaseComboBox.FormattingEnabled = true;
            this.databaseComboBox.Name = "databaseComboBox";
            this.databaseComboBox.SelectedIndexChanged += new System.EventHandler(this.databaseComboBox_SelectedIndexChanged);
            // 
            // userLabel
            // 
            resources.ApplyResources(this.userLabel, "userLabel");
            this.userLabel.Name = "userLabel";
            // 
            // userTextBox
            // 
            resources.ApplyResources(this.userTextBox, "userTextBox");
            this.userTextBox.Name = "userTextBox";
            // 
            // passwordLabel
            // 
            resources.ApplyResources(this.passwordLabel, "passwordLabel");
            this.passwordLabel.Name = "passwordLabel";
            // 
            // passwodTextBox
            // 
            resources.ApplyResources(this.passwodTextBox, "passwodTextBox");
            this.passwodTextBox.Name = "passwodTextBox";
            this.passwodTextBox.UseSystemPasswordChar = true;
            // 
            // loginButton
            // 
            resources.ApplyResources(this.loginButton, "loginButton");
            this.loginButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.loginButton.Name = "loginButton";
            this.loginButton.UseVisualStyleBackColor = true;
            this.loginButton.Click += new System.EventHandler(this.loginButton_Click);
            // 
            // btn_getDatabases
            // 
            resources.ApplyResources(this.btn_getDatabases, "btn_getDatabases");
            this.btn_getDatabases.Name = "btn_getDatabases";
            this.btn_getDatabases.UseVisualStyleBackColor = true;
            this.btn_getDatabases.Click += new System.EventHandler(this.btn_getDatabases_Click);
            // 
            // LoginForm
            // 
            this.AcceptButton = this.loginButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btn_getDatabases);
            this.Controls.Add(this.loginButton);
            this.Controls.Add(this.passwodTextBox);
            this.Controls.Add(this.passwordLabel);
            this.Controls.Add(this.userTextBox);
            this.Controls.Add(this.userLabel);
            this.Controls.Add(this.databaseComboBox);
            this.Controls.Add(this.dataBaseName);
            this.Controls.Add(this.urllabel);
            this.Controls.Add(this.urlTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox urlTextBox;
        private System.Windows.Forms.Label urllabel;
        private System.Windows.Forms.Label dataBaseName;
        private System.Windows.Forms.ComboBox databaseComboBox;
        private System.Windows.Forms.Label userLabel;
        private System.Windows.Forms.TextBox userTextBox;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.TextBox passwodTextBox;
        private System.Windows.Forms.Button loginButton;
        private System.Windows.Forms.Button btn_getDatabases;
    }
}