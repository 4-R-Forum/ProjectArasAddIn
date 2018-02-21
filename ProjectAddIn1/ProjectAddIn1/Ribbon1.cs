using Aras.IOM;
using Microsoft.Office.Interop.MSProject;
using Microsoft.Office.Tools.Ribbon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MS_Project_Import_Export
{
    public partial class Ribbon1
    {
        private List<Row> rows;
        private Item uploadItems;

        private static readonly Dictionary<string, short> pj_prec
        = new Dictionary<string, short>
            {
                { "Finish to Finish",0},
                { "Finish to Start" ,1},
                { "Start to Finish" ,2},
                { "Start to Start"  ,3}
            };

        //public static bool logged_in;        
        //private static RibbonDropDown dd_projects;

        //private static Project activeProject;        

        // private static string wbs_id;
        //private static string innovProjNum;

        private void Ribbon1_Load(object sender, RibbonUIEventArgs e)
        {
        }

        private void btn_projectToAras_Click(object sender, RibbonControlEventArgs e)
        {
            uploadProject(Globals.ThisAddIn.Application.ActiveProject);
        }

        private void uploadProject(Project activeProject)
        {
            rows = new List<Row>();
            uploadItems = InnovatorManager.Instance.CreateNewItem();
            string errorIds = string.Empty;

            // check there are no empty rows or dependencies to rollup task
            foreach (Task task in activeProject.Tasks)
            {
                if (task == null)
                {
                    continue;
                }

                foreach (TaskDependency dependency in task.TaskDependencies)
                {
                    if (dependency.From.Rollup)
                    {
                        if (errorIds != string.Empty)
                        {
                            errorIds += ", ";
                        }
                        errorIds += dependency.To.ID;
                    }
                }
                rows.Add(new Row() { Task = task });
            }

            if (!string.IsNullOrEmpty(errorIds))
            {
                MessageBox.Show(string.Format(Properties.Resources.ERROR_ROLLUP_NOT_ALLOWED, errorIds), Properties.Resources.TITLE,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Row prevRow = null;
            List<Row> processedRows = new List<Row>();

            foreach (var row in rows)
            {
                var prevId = string.Empty;

                if (prevRow != null && row.Task.OutlineParent.ID != prevRow.Task.ID)
                {
                    if (row.Task.OutlineParent.ID == prevRow.Task.OutlineParent.ID)
                    {
                        prevId = prevRow.Item?.getID();
                    }
                    else
                    {
                        var pRow = processedRows.LastOrDefault(r => r.Task.OutlineParent.ID == row.Task.OutlineParent.ID);
                        prevId = pRow?.Item.getID();
                    }
                }

                var type = row.Task.Summary ? "WBS Element" : "Activity2";
                var item = InnovatorManager.Instance.CreateNewItem(type, "add");
                item.setProperty("prev_item", prevId);
                row.Item = item;

                prevRow = row;
                processedRows.Add(row);
            }

            Item topWBSItem = InnovatorManager.Instance.CreateNewItem("WBS Element", "add"); // create Top WBS      
            topWBSItem.setProperty("name", activeProject.Name);
            topWBSItem.setProperty("is_top", "1");
            uploadItems.appendItem(topWBSItem);

            Item projectItem = InnovatorManager.Instance.CreateProjectItem(activeProject.Name, topWBSItem.getID(), activeProject.Start, activeProject.Finish);
            uploadItems.appendItem(projectItem);

            processRows(rows, topWBSItem.getID(), projectItem.getProperty("project_number"));
            addPredecessors();

            if (uploadItems.isCollection())
            {
                uploadItems.removeItem(uploadItems.getItemByIndex(0));
            }

            var response = InnovatorManager.Instance.ApplyAML(uploadItems.dom.OuterXml);
            if (response.isError())
            {
                MessageBox.Show(response.getErrorString(), Properties.Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var sProject = InnovatorManager.Instance.CreateNewItem("Project");
            sProject.setID(projectItem.getID());
            response = sProject.apply("Schedule Project");

            if (response.isError())
            {
                MessageBox.Show(response.getErrorString());
            }
            else
            {
                MessageBox.Show(Properties.Resources.PROJECT_IMPORTED);
            }
        }

        private void processRows(List<Row> rows, string wbsId, string projectNumber)
        {
            foreach (var item in rows)
            {
                switch (item.Item.getType())
                {
                    case "WBS Element":
                        addWBS(item, wbsId);
                        break;
                    case "Activity2":
                        addActivity(item, wbsId, projectNumber);
                        break;
                    default:
                        MessageBox.Show("Unable to determine row type in row " + rows.IndexOf(item));
                        break;
                }
            }
        }

        private void addWBS(Row row, string wbsId)
        {
            row.Item.setProperty("name", row.Task.Name);
            uploadItems.appendItem(row.Item);
            
            // add subWBS
            var relationship = InnovatorManager.Instance.CreateNewItem("Sub WBS", "add");
            var sourceId = (row.Task.OutlineParent.ID == 0) ? wbsId : rows.FirstOrDefault(r => r.Task.ID == row.Task.OutlineParent.ID)?.Item.getID();
            relationship.setProperty("source_id", sourceId);
            relationship.setProperty("related_id", row.Item.getID());
            uploadItems.appendItem(relationship);
        }

        private void addActivity(Row row, string wbsId, string projectNumber)
        {
            row.Item.setProperty("name", row.Task.Name);
            row.Item.setProperty("proj_num", projectNumber);

            if (row.Task.Notes != "")
            {
                row.Item.setProperty("description", row.Task.Notes);
            }

            row.Item.setProperty("work_est", (row.Task.Work / 60).ToString()); //Work is in minutes
            row.Item.setProperty("date_start_target", InnovatorManager.Instance.LocalDateToInnovatorDate((DateTime)row.Task.Start));
            row.Item.setProperty("date_due_target", InnovatorManager.Instance.LocalDateToInnovatorDate((DateTime)row.Task.Finish));
            if (row.Task.Milestone) // this is a milestone
            {
                row.Item.setProperty("is_milestone", "1");
                row.Item.setProperty("expected_duration", "0");
            }
            else // not a milestone
            {
                row.Item.setProperty("expected_duration", (row.Task.Duration / 60 / 8).ToString()); //Duration is in minutes
            }
            uploadItems.appendItem(row.Item);

            Item relationship = InnovatorManager.Instance.CreateNewItem("WBS Activity2", "add");
            var sourceId = (row.Task.OutlineParent.ID == 0) ? wbsId : rows.FirstOrDefault(r => r.Task.ID == row.Task.OutlineParent.ID)?.Item.getID();
            relationship.setProperty("source_id", sourceId);
            relationship.setProperty("related_id", row.Item.getID());
            uploadItems.appendItem(relationship);

            //Add Assigments
            addAssignments(row);

            // Promote Activity Complete NB Assignments must be added first because
            // adding an assignment makes a complete activity active again
            if (row.Task.PercentComplete == 100)
            {
                Item promItem = InnovatorManager.Instance.CreatePromotionItem("Activity2", "promoteItem", row.Item.getID(), "Complete");
                uploadItems.appendItem(promItem);
            }
        }

        /// <summary>
        ///  This function tries to use MS Project Resource names.  
        ///  The result will be most meaningful if Resource names match Alias Identities in Innovator,
        ///  or values in the 'Project Team' List
        /// </summary>
        /// <param name="row"></param>
        private void addAssignments(Row row)
        {
            for (int i = 1; i < row.Task.Assignments.Count + 1; i++)
            {
                string asstId = string.Empty;
                Assignment assignment = row.Task.Assignments[i];

                // if this is a Project and not a Template we try to find a User
                // whose name matches the MSProject Resource name
                Item userItem = InnovatorManager.Instance.CreateNewItem("User", "get");
                Item aliasItem = InnovatorManager.Instance.CreateNewItem("Alias", "get");
                userItem.setProperty("keyed_name", assignment.ResourceName);
                userItem.setAttribute("select", "id");
                aliasItem.setAttribute("select", "id,related_id");
                userItem.addRelationship(aliasItem);

                Item result = userItem.apply();
                if (!result.isError())
                {
                    asstId = result.getRelationships("Alias").getItemByIndex(0).getProperty("related_id");
                }

                // create an assignment Item
                Item asstRelation = InnovatorManager.Instance.CreateNewItem("Activity2 Assignment", "add");
                asstRelation.setProperty("source_id", row.Item.getID());
                asstRelation.setProperty("percent_load", (assignment.Units * 100).ToString());
                asstRelation.setProperty("work_est", (assignment.Work / 60).ToString());

                // if the id of an Identity is known use it, otherwise use the Resource name as a role
                if (asstId != string.Empty)
                {
                    asstRelation.setProperty("related_id", asstId);
                }
                else
                {
                    asstRelation.setProperty("role", assignment.ResourceName);
                }

                // apply the assignment Item, and promote it to complete if the MSProject row is complete
                uploadItems.appendItem(asstRelation);
                if (row.Task.PercentComplete == 100)
                {
                    var promItem = InnovatorManager.Instance.CreatePromotionItem(asstRelation.getType(), "promoteItem", asstRelation.getID(), "Complete");
                    uploadItems.appendItem(promItem);
                }
            }
        }

        private void addPredecessors()
        {
            foreach (var row in rows)
            {
                if (row.Task.Rollup)
                {
                    continue;
                }

                var pred_ct = row.Task.TaskDependencies.Count;
                for (int i = 1; i < pred_ct + 1; i++)
                {
                    var this_pred = row.Task.TaskDependencies[i];
                    if (this_pred.To.ID == row.Task.ID)
                    {
                        //look only at Predecessors not Successors
                        string predType = string.Empty;
                        Item predecessor = InnovatorManager.Instance.CreateNewItem("Predecessor", "add");
                        switch (this_pred.Type.ToString())
                        {
                            case "pjFinishToFinish":
                                //MSProject constant pjFinishToFinish
                                predType = "Finish to Finish";
                                break;
                            case "pjFinishToStart":
                                //MSProject constant pjFinishToStart
                                predType = "Finish to Start";
                                break;
                            case "pjStartToFinsih":
                                //MSProject constant pjStartToFinish
                                predType = "Start to Finish";
                                break;
                            case "pjStartToStart":
                                //MSProject constant pjStartToStart
                                predType = "Start to Start";
                                break;
                        }
                        predecessor.setProperty("precedence_type", predType);
                        predecessor.setProperty("lead_lag", (this_pred.Lag / 60 / 8).ToString()); //Lag is minutes in 8 hour day
                        predecessor.setProperty("source_id", row.Item.getID());
                        predecessor.setProperty("related_id", rows.FirstOrDefault(r => r.Task.ID == this_pred.From.ID)?.Item.getID());
                        uploadItems.appendItem(predecessor);
                    }
                }
            }
        }

        private void btn_arasToProject_Click(object sender, RibbonControlEventArgs e)
        {
            var activeProject = Globals.ThisAddIn.Application.ActiveProject;
            
            Item rootWBS = InnovatorManager.Instance.CreateNewItem("WBS Element", "GetProjectTree");
            rootWBS.setProperty("project_id", dd_projects.SelectedItem.Tag.ToString());
            rootWBS = rootWBS.apply();
            if (rootWBS.isError())
            {
                MessageBox.Show(rootWBS.getErrorString(), Properties.Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            // // first call server method to get an ordered list of project rows
            Item z = InnovatorManager.Instance.CreateNewItem("Project", "select_project_tree");
            string this_project_id = dd_projects.SelectedItem.Tag.ToString(); //"5266E1D280F84861A1BBCF5E50A9C65B";
            z.setID(this_project_id);
            Item tree = z.apply();
            if (tree.isError())
            {
                MessageBox.Show("Error getting Project Tree");
                return;
            }
            // // create a dictionary object to convert ids to row numbers
            // // using key=id, value= row number
            Dictionary<string, string> d = new Dictionary<string, string>();
            // // create a dictionary object to convert converting scheduling_type values to MSProject constants


            //ap = Globals.ThisAddIn.Application.ActiveProject = new Project();


            string rootId = rootWBS.getID();


            //string root = tree.getItemByIndex(0).getProperty("id");
            var tasks = activeProject.Tasks;
            var resources = activeProject.Resources;
            int uasCount = 1; // counter for unknown assigments

            int itemNumber = 1;
            var currentItem = rootWBS.getItemsByXPath("//Item[n='1']");

            while (currentItem.node != null)
            {
                d.Add(currentItem.getProperty("id"), (Int16.Parse(currentItem.getProperty("n"))).ToString());
                var currentTask = tasks.Add(currentItem.getProperty("name"));

                var t = tasks[tasks.Count];
                var t2 = tasks[tasks.Count];
                if (t == t2)
                {

                }

                int lev = Int16.Parse(currentItem.getProperty("l"));
                if (lev > currentTask.OutlineLevel)
                {
                    currentTask.OutlineIndent();
                }
                else
                {
                    while (currentTask.OutlineLevel > lev)
                    {
                        currentTask.OutlineOutdent();
                    }
                }

                switch (currentItem.getType())
                {
                    case "WBS Element":
                        break;
                    case "Activity2":
                        currentTask.Duration = currentItem.getProperty("expected_duration");
                        currentTask.Start = InnovatorManager.Instance.InnovatorDateToLocalDate(currentItem.getProperty("date_start_sched"));
                        currentTask.Finish = InnovatorManager.Instance.InnovatorDateToLocalDate(currentItem.getProperty("date_due_sched"));

                        //currentTask.Start = InnovatorManager.Instance.InnovatorDateToLocalDate(currentItem.getProperty("date_start_target"));
                        //currentTask.Finish = InnovatorManager.Instance.InnovatorDateToLocalDate(currentItem.getProperty("date_due_target"));
                       
                        currentTask.Estimated = false;
                        var res = currentItem.getRelationships("Activity2 Assignment");
                        for (var y = 0; y < res.getItemCount(); y++)
                        {
                            string name = res.getItemByIndex(y).getPropertyAttribute("related_id", "keyed_name");
                            if (string.IsNullOrEmpty(name))
                            {
                                name = res.getItemByIndex(y).getProperty("role", "Unknown");
                                if (name == "Unknown") { name += uasCount; uasCount += 1; }
                            }
                            var units = res.getItemByIndex(y).getProperty("percent_load");
                            try { var n = resources[name]; } catch (System.Exception ex) { resources.Add(name); }
                            currentTask.Assignments.Add(currentTask.ID, resources[name].ID);
                        }

                        break;
                }

                currentItem = rootWBS.getItemsByXPath("//Item[n='" + (++itemNumber).ToString() + "']");
            }

            var activities = rootWBS.getItemsByXPath("//Item[@type='Activity2']");
            for (int i = 0; i < activities.getItemCount(); i++)
            {
                var activity = activities.getItemByIndex(i);
                var pred = activity.getRelationships("Predecessor");
                for (int y = 0; y < pred.getItemCount(); y++)
                {
                    int taskIndex = 0;
                    if (!int.TryParse(activity.getProperty("n", string.Empty), out taskIndex))
                    {
                        continue;
                    }

                    string PredID = d[pred.getItemByIndex(y).getProperty("related_id")];
                    PjTaskLinkType precType = (PjTaskLinkType)pj_prec[pred.getItemByIndex(y).getProperty("precedence_type")];
                    var lead = pred.getItemByIndex(y).getProperty("lead_lag");
                    tasks[taskIndex].TaskDependencies.Add(tasks[short.Parse(PredID)], precType, lead);
                }
            }

            // populate the MSProject and dictionary object
            //for (var x = 1; x < tree.getItemCount(); x++)
            //{
            //    Item task = tree.getItemByIndex(x);
            //    d.Add(task.getProperty("id"), (Int16.Parse(task.getProperty("n")) - 1).ToString());
            //    tasks.Add(task.getProperty("name"));
            //    int lev = Int16.Parse(task.getProperty("l"));
            //    if (lev > tasks[x].OutlineLevel)
            //    {
            //        tasks[x].OutlineIndent();
            //    }
            //    else
            //    {
            //        while (tasks[x].OutlineLevel > lev)
            //        {
            //            tasks[x].OutlineOutdent();
            //        }
            //    }
            //    switch (task.getType())
            //    {
            //        case "W":
            //            Item wbs = InnovatorManager.Instance.CreateNewItem("WBS Element", "get");
            //            wbs.setID(task.getProperty("id"));
            //            wbs = wbs.apply();
            //            break;
            //        case "A":
            //            Item act = InnovatorManager.Instance.CreateNewItem("Activity2", "get");
            //            act.setID(task.getProperty("id"));
            //            var assts = InnovatorManager.Instance.CreateNewItem("Activity2 Assignment", "get");
            //            assts.setAttribute("select", "role,related_id,percent_load,work_est");
            //            assts.setAttribute("related_expand", "0");
            //            act.addRelationship(assts);
            //            act = act.apply();
            //            tasks[x].Duration = act.getProperty("expected_duration");
            //            //if (!is_template)
            //            //{
            //            tasks[x].Start = InnovatorManager.Instance.InnovatorDateToLocalDate(act.getProperty("date_start_sched"));
            //            tasks[x].Finish = InnovatorManager.Instance.InnovatorDateToLocalDate(act.getProperty("date_due_sched"));
            //            //}
            //            tasks[x].Estimated = (System.Boolean)false;
            //            var res = act.getItemsByXPath("//Item[@type='Activity2 Assignment']");
            //            for (var y = 0; y < res.getItemCount(); y++)
            //            {
            //                string name = res.getItemByIndex(y).getPropertyAttribute("related_id", "keyed_name");
            //                if (string.IsNullOrEmpty(name))
            //                {
            //                    name = res.getItemByIndex(y).getProperty("role", "Unknown");
            //                    if (name == "Unknown") { name += uasCount; uasCount += 1; }
            //                }
            //                var units = res.getItemByIndex(y).getProperty("percent_load");
            //                try { var n = resources[name]; } catch (System.Exception ex) { resources.Add(name); }
            //                tasks[x].Assignments.Add(tasks[x].ID, resources[name].ID);
            //            }
            //            break;
            //    }
        //}

            // traverse tasks again, the first time there could have been predecessors not created yet
//            for (int x = 1; x<tree.getItemCount(); x++)
//            {
//                Item task = tree.getItemByIndex(x);

//                if (task.getType() == "A")
//                {
//                    Item act = InnovatorManager.Instance.CreateNewItem("Activity2", "get");
//        act.setID(task.getProperty("id"));
//                    var preds = InnovatorManager.Instance.CreateNewItem("Predecessor", "get");
//        preds.setAttribute("select", "related_id,precedence_type,lead_lag");
//                    preds.setAttribute("related_expand", "0");
//                    act.addRelationship(preds);
//                    act = act.apply();
//                    var pred = act.getItemsByXPath("//Item[@type='Predecessor']");
//                    for (int y = 0; y<pred.getItemCount(); y++)
//                    {
//                        string PredID = d[pred.getItemByIndex(y).getProperty("related_id")];
//        PjTaskLinkType precType = (PjTaskLinkType)pj_prec[pred.getItemByIndex(y).getProperty("precedence_type")];
//        var lead = pred.getItemByIndex(y).getProperty("lead_lag");
//        tasks[x].TaskDependencies.Add(tasks[short.Parse(PredID)], precType, lead);
//                    }
//}
//            }
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
        }
    } while (!InnovatorManager.Instance.IsLoggedIn && dialogResult == DialogResult.OK);
}

private void setProjectsFromInnovator()
{
    List<Item> projects = InnovatorManager.Instance.GetProjects();

    foreach (var project in projects)
    {
        RibbonDropDownItem item = Globals.Factory.GetRibbonFactory().CreateRibbonDropDownItem();
        item.Label = project.getProperty("project_number");
        item.Tag = project.getID();
        dd_projects.Items.Add(item);
    }
}
    }
}
