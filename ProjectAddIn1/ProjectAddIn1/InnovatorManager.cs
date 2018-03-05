using Aras.IOM;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MS_Project_Import_Export
{
    public class InnovatorManager
    {
        private static InnovatorManager instance;
        private Innovator innovatorInstance;
        private TimeZoneInfo innTimeZoneInfo;

        private InnovatorManager() { }

        public static InnovatorManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new InnovatorManager();
                }
                return instance;
            }
        }

        public bool IsLoggedIn { get; set; }

        public string LoginToInnovator(string innovatorUrl, string database, string userName, string password)
        {
            HttpServerConnection serverConnection = null;

            using (new WaitingCursor())
            {
                try
                {
                    serverConnection = IomFactory.CreateHttpServerConnection(innovatorUrl, database, userName, Innovator.ScalcMD5(password));

                    var result = serverConnection.Login();
                    if (result.isError())
                    {
                        serverConnection = null;
                        return result.getErrorString();
                    }
                }
                catch (Exception ex)
                {
                    serverConnection = null;
                    return ex.Message;
                }
            }

            innovatorInstance = new Innovator(serverConnection);
            IsLoggedIn = true;
            return string.Empty;
        }

        public List<string> GetDataBases(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return new List<string>();
            }

            using (new WaitingCursor())
            {
                try
                {
                    var serverConnection = IomFactory.CreateHttpServerConnection(url);
                    return serverConnection.GetDatabases().ToList();
                }
                catch
                {
                    return new List<string>();
                }
            }
        }

        public Item CreateNewItem(string type = null, string action = null)
        {
            return innovatorInstance.newItem(type, action);
        }

        public Item CreatePromotionItem(string type, string action, string id, string state)
        {
            Item item = innovatorInstance.newItem(type, action);
            item.setID(id);
            item.setProperty("state", state);
            return item;
        }

        public Item CreateProjectItem(string name, string WBSId, DateTime startDate, DateTime finishDate)
        {
            Item item = innovatorInstance.newItem("Project", "add");

            item.setProperty("date_start_target", LocalDateToInnovatorDate(startDate));
            item.setProperty("date_due_target", LocalDateToInnovatorDate(finishDate));
            item.setProperty("project_number", innovatorInstance.getNextSequence("Project Number"));
            item.setProperty("scheduling_type", "Forward");
            item.setProperty("scheduling_method", "7DC85B0668134E949B9212D7CE199265");
            item.setProperty("update_method", "6E1133AB87A44D529DF5F9D1FD740100");
            item.setProperty("scheduling_mode", "1");
            item.setProperty("project_update_mode", "1");
            item.setProperty("name", name);
            item.setProperty("wbs_id", WBSId);

            return item;
        }

        public Dictionary<string, string> GetProjects()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            Item item = innovatorInstance.newItem("Project", "get");
            item = item.apply();

            if (item.isError())
            {
                return result;
            }

            var itemsCount = item.getItemCount();
            for (int i = 0; i < itemsCount; i++)
            {
                var project = item.getItemByIndex(i);
                result.Add(project.getProperty("project_number"), project.getID());
            }

            return result;
        }

        public Item ApplyAML(string aml)
        {
            return innovatorInstance.applyAML(aml);
        }

        public string LocalDateToInnovatorDate(DateTime date)
        {
            string convertedDate = date.ToString("u");
            // we need to pass a string to ConvertToNeutral
            DateTimeFormatInfo dtFormatInfo = new CultureInfo(innovatorInstance.getI18NSessionContext().GetLocale(), false).DateTimeFormat;
            // identify the format we are using
            convertedDate = innovatorInstance.getI18NSessionContext().ConvertToNeutral(convertedDate, "date", dtFormatInfo.UniversalSortableDateTimePattern);
            // return the new launch date in neutral format
            return convertedDate;
        }

        public DateTime InnovatorDateToLocalDate(string date)
        {
            if (innTimeZoneInfo == null)
            {
                innTimeZoneInfo = getInnovatorTimeZone();
            }

            return TimeZoneInfo.ConvertTime(DateTime.Parse(date), innTimeZoneInfo, TimeZoneInfo.Local);
        }

        private TimeZoneInfo getInnovatorTimeZone()
        {
            var item = innovatorInstance.newItem("Variable", "get");
            item.setProperty("name", "CorporateTimeZone");
            item = item.apply();
            if (item.isError())
            {
                return TimeZoneInfo.Local;
            }

            if (!string.IsNullOrWhiteSpace(item.getProperty("value", string.Empty)))
            {
                return TimeZoneInfo.FindSystemTimeZoneById(item.getProperty("value"));
            }
            if (!string.IsNullOrWhiteSpace(item.getProperty("default_value", string.Empty)))
            {
                return TimeZoneInfo.FindSystemTimeZoneById(item.getProperty("default_value"));
            }

            return TimeZoneInfo.Local;
        }
    }
}