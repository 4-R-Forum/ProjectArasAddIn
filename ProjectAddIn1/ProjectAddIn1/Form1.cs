using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aras.IOM;
using System.Xml;
using System.Net;


namespace ProjectAddIn1
{
    public partial class Login : Form
    {

        public Login()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string url = tb_url.Text + "Server/InnovatorServer.aspx";
            string db = cb_db.Text;
            string user = tb_user.Text;
            string pw = tb_pw.Text;

            HttpServerConnection conn = IomFactory.CreateHttpServerConnection(url, db, user, pw);
            Item res = conn.Login();
            if (res.isError())
            {
                //hrow new ArgumentException("Login failed", res.getErrorString());
                ErrorMsg.Text = res.getErrorString();
            }
            else
            {
                Ribbon1.innov = IomFactory.CreateInnovator(conn);
                if (Ribbon1.innov == null)
                {
                    ErrorMsg.Text = res.getErrorString();
                }
                else
                {
                    Ribbon1.set_login(true);
                   // Ribbon1.populate_queries();
                    this.Close();
                }
            }
        }

 
        private void tb_url_Leave(object sender, EventArgs e)
        {
            cb_db.Items.Clear();

            WebRequest request = WebRequest.Create(tb_url.Text + "/Server/DBList.aspx");
            request.Method = "POST";
            request.ContentLength = 0;
            WebResponse response = request.GetResponse() as WebResponse;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(response.GetResponseStream());
            XmlNodeList res = xmlDoc.DocumentElement.SelectNodes("DB/@id");
            for (int i = 0; i < res.Count; i++)
            {
                cb_db.Items.Add(res[i].Value);
            }

        }
        private void Login_Load(object sender, EventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("Config.xml");
            tb_url.Text = doc.SelectSingleNode("/Innovator/url").InnerText;
            cb_db.Text = doc.SelectSingleNode("/Innovator/db").InnerText;
            tb_user.Text = doc.SelectSingleNode("/Innovator/user").InnerText;
            tb_pw.Text = doc.SelectSingleNode("/Innovator/pw").InnerText;
        }

        private void Login_Leave(object sender, EventArgs e)
        {
            string config_xml = @"
<Innovator>
  <url>{0}</url>
  <db>{1}</db>
  <user>{2}</user>
  <pw>{3}</pw>  
</Innovator>";
            config_xml = string.Format(config_xml, tb_url.Text, cb_db.Text, tb_user.Text, tb_pw.Text);
            System.IO.File.WriteAllText("config.xml", config_xml);
        }

        private void Login_Deactivate(object sender, EventArgs e)
        {
            string config_xml = @"
<Innovator>
  <url>{0}</url>
  <db>{1}</db>
  <user>{2}</user>
  <pw>{3}</pw>  
</Innovator>";
            config_xml = string.Format(config_xml, tb_url.Text, cb_db.Text, tb_user.Text, tb_pw.Text);
            System.IO.File.WriteAllText("config.xml", config_xml);
        }

    }

}


