using Microsoft.Office.Tools.Ribbon;
using System.Windows.Forms;

namespace MS_Project_Import_Export
{
    public partial class Ribbon
    {
        private void btn_projectToAras_Click(object sender, RibbonControlEventArgs e)
        {
            string message = string.Empty;
            ProjectLoader loader = new ProjectLoader();
            MessageBoxIcon icon = MessageBoxIcon.Error;

            using (new WaitingCursor())
            {
                if (loader.UploadProject(Globals.ThisAddIn.Application.ActiveProject, out message))
                {
                    icon = MessageBoxIcon.Information;
                    message = Properties.Resources.PROJECT_IMPORTED;
                }
            }

            MessageBox.Show(message, Properties.Resources.TITLE, MessageBoxButtons.OK, icon);
        }

        private void btn_arasToProject_Click(object sender, RibbonControlEventArgs e)
        {
            if (dd_projects.Items.Count == 0 || string.IsNullOrEmpty(dd_projects?.SelectedItem?.Tag?.ToString()))
            {
                return;
            }

            using (new WaitingCursor())
            {
                ProjectLoader loader = new ProjectLoader();
                loader.DownloadProject(Globals.ThisAddIn.Application.ActiveProject, dd_projects.SelectedItem.Tag.ToString());
            }
        }

        private void btn_loginToAras_Click(object sender, RibbonControlEventArgs e)
        {
            if (InnovatorManager.Instance.IsLoggedIn)
            {
                return;
            }

            var loginForm = new LoginForm(Configuration.InnovatorURL, Configuration.InnovatorDatabaseName,
                InnovatorManager.Instance.GetDataBases(Configuration.InnovatorURL), Configuration.InnovatorUserName);
            DialogResult dialogResult;

            do
            {
                dialogResult = loginForm.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                    var result = InnovatorManager.Instance.LoginToInnovator(loginForm.InnovatorUrl, loginForm.DataBase, loginForm.UserName, loginForm.Password);
                    if (!string.IsNullOrEmpty(result))
                    {
                        MessageBox.Show(result, Properties.Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        continue;
                    }

                    Configuration.InnovatorDatabaseName = loginForm.DataBase;
                    Configuration.InnovatorURL = loginForm.InnovatorUrl;
                    Configuration.InnovatorUserName = loginForm.UserName;

                    setProjectsFromInnovator();
                    changeButtonsState(true);
                }
            } while (!InnovatorManager.Instance.IsLoggedIn && dialogResult == DialogResult.OK);
        }

        private void changeButtonsState(bool state)
        {
            btn_arasToProject.Enabled = state;
            btn_projectToAras.Enabled = state;
            dd_projects.Enabled = state;
        }

        private void setProjectsFromInnovator()
        {
            var projects = InnovatorManager.Instance.GetProjects();

            foreach (var project in projects)
            {
                RibbonDropDownItem item = Globals.Factory.GetRibbonFactory().CreateRibbonDropDownItem();
                item.Label = project.Key;
                item.Tag = project.Value;
                dd_projects.Items.Add(item);
            }
        }
    }
}
