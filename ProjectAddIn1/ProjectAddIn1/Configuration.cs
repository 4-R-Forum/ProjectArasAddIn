using MS_Project_Import_Export.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS_Project_Import_Export
{
    internal class Configuration
    {
        public static string InnovatorURL
        {
            get
            {
                return Settings.Default.InnovatorServerURL;
            }
            set
            {
                Settings.Default.InnovatorServerURL = value.Trim();
                Settings.Default.Save();
            }
        }
        public static string InnovatorDatabaseName
        {
            get
            {
                return Settings.Default.InnovatorDatabaseName;
            }
            set
            {
                Settings.Default.InnovatorDatabaseName = value;
                Settings.Default.Save();
            }
        }
        public static string InnovatorUserName
        {
            get
            {
                return Settings.Default.InnovatorUserName;
            }
            set
            {
                Settings.Default.InnovatorUserName = value;
                Settings.Default.Save();
            }
        }
    }
}
