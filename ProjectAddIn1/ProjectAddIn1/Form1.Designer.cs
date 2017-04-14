

namespace ProjectAddIn1
{
    partial class Login
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
            this.Load += new System.EventHandler(this.Login_Load);
            this.Leave += new System.EventHandler(this.Login_Leave);
            this.Deactivate += new System.EventHandler(this.Login_Deactivate);

            this.label1 = new System.Windows.Forms.Label();
            this.tb_url = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.cb_db = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tb_user = new System.Windows.Forms.TextBox();
            this.tb_pw = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.ErrorMsg = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 11);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Url";
            // 
            // tb_url
            // 
            this.tb_url.Location = new System.Drawing.Point(62, 11);
            this.tb_url.Margin = new System.Windows.Forms.Padding(2);
            this.tb_url.Name = "tb_url";
            this.tb_url.Size = new System.Drawing.Size(268, 20);
            this.tb_url.TabIndex = 1;
            this.tb_url.Text = "http://jhodge06/Innovator11SP6/";
            this.tb_url.Leave += new System.EventHandler(this.tb_url_Leave);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(127, 136);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(56, 19);
            this.button1.TabIndex = 2;
            this.button1.Text = "Login";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // cb_db
            // 
            this.cb_db.FormattingEnabled = true;
            this.cb_db.Location = new System.Drawing.Point(62, 36);
            this.cb_db.Name = "cb_db";
            this.cb_db.Size = new System.Drawing.Size(121, 21);
            this.cb_db.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(22, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "DB";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 73);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Login";
            // 
            // tb_user
            // 
            this.tb_user.Location = new System.Drawing.Point(62, 73);
            this.tb_user.Name = "tb_user";
            this.tb_user.Size = new System.Drawing.Size(121, 20);
            this.tb_user.TabIndex = 6;
            // 
            // tb_pw
            // 
            this.tb_pw.Location = new System.Drawing.Point(62, 111);
            this.tb_pw.Name = "tb_pw";
            this.tb_pw.PasswordChar = '*';
            this.tb_pw.Size = new System.Drawing.Size(121, 20);
            this.tb_pw.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 117);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(25, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "PW";
            // 
            // ErrorMsg
            // 
            this.ErrorMsg.AutoSize = true;
            this.ErrorMsg.Location = new System.Drawing.Point(36, 179);
            this.ErrorMsg.MaximumSize = new System.Drawing.Size(400, 50);
            this.ErrorMsg.Name = "ErrorMsg";
            this.ErrorMsg.Size = new System.Drawing.Size(0, 13);
            this.ErrorMsg.TabIndex = 9;
            this.ErrorMsg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(389, 258);
            this.Controls.Add(this.ErrorMsg);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tb_pw);
            this.Controls.Add(this.tb_user);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cb_db);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tb_url);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Login";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tb_url;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox cb_db;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tb_user;
        private System.Windows.Forms.TextBox tb_pw;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label ErrorMsg;
    }
}