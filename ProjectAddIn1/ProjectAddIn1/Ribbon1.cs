using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;
using Aras.IOM;
using Microsoft.Office.Interop.MSProject;
using System.Windows.Forms;
using System.Globalization;

namespace ProjectAddIn1
{
    public partial class Ribbon1
    {

        public static bool logged_in;
        public static Innovator innov;
        public static I18NSessionContext cntx;
        private static RibbonDropDown dd_projects;
        private static Microsoft.Office.Core.DocumentProperties properties;
        private static string title;
        private static Microsoft.Office.Interop.MSProject.Project ap;
        private static int task_ct;
        private static string[] row_id;
        private static string[] prev_id;
        private static string wbs_id;
        private static string innovProjNum;
        private static readonly Dictionary<string, short> pj_prec
        = new Dictionary<string, short>
            {
                { "Finish to Finish",0},
                { "Finish to Start" ,1},
                { "Start to Finish" ,2},
                { "Start to Start"  ,3}
            };
        private void Ribbon1_Load(object sender, RibbonUIEventArgs e)
        {
            dd_projects = dropDown1;
        }

        private void logon_launcher(object sender, RibbonControlEventArgs e)
        {
            Login dlg = new Login();
            dlg.ShowDialog();
        }

        public static void set_login(bool v)
        {
            logged_in = v;
            cntx = innov.getI18NSessionContext();
            Item qry1 = innov.newItem("Project");
            qry1.setAttribute("select", "project_number");
            Item res1 = qry1.apply("get");
            if (res1.isError()) MessageBox.Show(res1.getErrorString());
            for (int i=0;i<res1.getItemCount();i++)
            {
                Item this_project = res1.getItemByIndex(i);
                RibbonDropDownItem item = Globals.Factory.GetRibbonFactory().CreateRibbonDropDownItem();
                item.Label = this_project.getProperty("project_number");
                item.Tag = this_project.getID();
                dd_projects.Items.Add(item);

            }
        }

        private void button1_Click(object sender, RibbonControlEventArgs e)
        {
            ap = Globals.ThisAddIn.Application.ActiveProject;
            task_ct = ap.Tasks.Count;
            I18NSessionContext cntx = innov.getI18NSessionContext();
            string locale = cntx.GetLocale();
            string msg = "";
            string msg1 = "";
            string msg2 = "";



            // check there are no empty rows or dependencies to rollup task
            for (int i = 1; i < task_ct + 1; i++)
            {
                if (ap.Tasks[i] == null)
                {
                     ap.Tasks[i].Delete();
                }
                else
                {
                    for (int j = 1; j < ap.Tasks[i].TaskDependencies.Count + 1; j++)
                    {
                        var td = ap.Tasks[i].TaskDependencies[j];
                        if (td.From.Rollup)
                        {
                            if (msg2 != "")
                            {
                                msg2 += ", ";
                            }
                            msg2 += td.To.ID;
                        }
                    }
                }
            }
            if (msg1 != "" || msg2 != "")
            {
                if (msg1 != "")
                {
                    msg += "Empty Tasks at rows " + msg1 + ", ";
                }
                if (msg2 != "")
                {
                    msg += "Dependency to Rollup Task at rows " + msg2 + ", ";
                }
                msg += "not allowed in Import. Please edit Project and try again";
                MessageBox.Show(msg);
                return;
            }
            // first lets create ids for project/template and wbs
            string project_id = innov.getNewID();
            wbs_id = innov.getNewID();
            // and array with an id for each row in MSProject, used for relationship source_id and to calculate prev_item
            // using base 1 to make it easier to match row numbers
            row_id = new string[task_ct + 1];
            for (int i = 1; i < task_ct + 1; i++)
            {
                row_id[i] = innov.getNewID();
            }

            // and array for each row in MSProject for Innovator prev_item property
            // used to maintain order of rows in Project Tree
            prev_id = new string[task_ct + 1];
            for (int i = 2; i < task_ct + 1; i++)
            {
                var prevRow = ap.Tasks[i - 1];
                if (ap.Tasks[i].OutlineParent.ID == prevRow.ID)
                {
                    prev_id[i] = ""; // if this row has parent = previous row then prev_id is empty
                }
                else
                {
                    if (ap.Tasks[i].OutlineParent.ID == prevRow.OutlineParent.ID)
                    {
                        prev_id[i] = row_id[i - 1]; // if this row has same parent as previour row then prev_id is id of previous row
                    }
                    else
                    {
                        // the row of the parent is
                        int parent = ap.Tasks[i].OutlineParent.ID;
                        // and this row is
                        int r = ap.Tasks[i].ID;
                        // now find previous row with same parent     
                        do
                        {
                            r = r - 1;
                        } while (!((ap.Tasks[r].OutlineParent.ID == parent) || (r == 0)));
                        if (r > 0)
                        {
                            // prev_item is source id of the row we found
                            prev_id[i] = row_id[r];
                        }
                        else
                        {
                            // there is no such row so this must be the first and prev_id will be empty
                            prev_id[i] = "";
                        }
                    }
                }
            }

            // create Top WBS
            Item iTopWBS = innov.newItem();

            iTopWBS.setID(wbs_id);
            iTopWBS.setType("WBS Element");
            iTopWBS.setProperty("name", ap.Name);
            iTopWBS.setProperty("is_top", "1");
            iTopWBS.setAction("add");
            commit(iTopWBS, "Root WBS", null);

            // create Project/Template
            Item iProject = innov.newItem();
            string project_type = "Project";
            iProject.setID(project_id);

            // get new Project number
            innovProjNum = innov.getNextSequence("Project Number");
            iProject.setType("Project");
            iProject.setProperty("date_start_target", cd(ap.Start));
            iProject.setProperty("date_due_target", cd(ap.Finish));
            iProject.setProperty("project_number", innovProjNum);
            iProject.setProperty("scheduling_type", "Forward");
            //iProject.setProperty("scheduling_method", "7DC85B0668134E949B9212D7CE199265");
            //iProject.setProperty("update_method", "6E1133AB87A44D529DF5F9D1FD740100");
            //iProject.setProperty("scheduling_mode", "1");
            //iProject.setProperty("project_update_mode", "1");

            iProject.setProperty("name", ap.Name);
            iProject.setProperty("wbs_id", wbs_id);
            iProject.setAction("add");
            commit(iProject, "Project", null);
            processRows();
            addPredecessors();
            var sProject = innov.newItem("Project");
            sProject.setID(project_id);
            var res = sProject.apply("Schedule Project");
            if (res.isError())
            {
                MessageBox.Show(res.getErrorString());
            }
            else
            {
                MessageBox.Show("Project imported successfully");
            }
        }


            private void processRows() {

            for (int i = 1; i < task_ct + 1; i++)
            {
                //Determine row type
                var rowType = "";
                if (ap.Tasks[i].Summary)
                {
                    rowType = "WBS";
                }
                else
                {
                    rowType = "Activity2";
                }
                switch (rowType)
                {
                    case "WBS":
                        addWBS(i);
                        break;
                    case "Activity2":
                        addActivity(i);
                        break;
                    default:
                        MessageBox.Show("Unable to determine row type in row " + i);
                        break;
                }
            }
        }

        private void addWBS(int r) {
            // add WBS Element
            var this_row = ap.Tasks[r];
            var iTemp = innov.newItem("WBS Element", "add");
            iTemp.setID(row_id[r]);
            iTemp.setProperty("name", this_row.Name);
            iTemp.setProperty("prev_item", prev_id[r]);
            //iTemp.setProperty ("wbs_index", ap.Tasks(r).WBS);
            commit(iTemp,"Row"+r.ToString(),null);
            // add subWBS
            var iRel = innov.newItem("Sub WBS", "add");
            //iRel.setID(relationship_id[r]);
            if (ap.Tasks[r].OutlineParent.ID == 0)
            {
                iRel.setProperty("source_id", wbs_id);
            }
            else
            {
                iRel.setProperty("source_id", row_id[this_row.OutlineParent.ID]);
            }
            iRel.setProperty("related_id", row_id[r]);
            commit(iRel, r + " Rel",null);

        }

        private void addActivity(int r) {
            var this_row = ap.Tasks[r];
            var iTemp = innov.newItem("Activity2", "add");

            iTemp.setID(row_id[r]);
            iTemp.setProperty("name", this_row.Name);
            iTemp.setProperty("prev_item", prev_id[r]);
            iTemp.setProperty("proj_num", innovProjNum);
            if (this_row.Notes != "")
            {
                iTemp.setProperty("description", this_row.Notes);
            }
            iTemp.setProperty("work_est", (this_row.Work / 60).ToString()); //Work is in minutes
            iTemp.setProperty("date_start_target", cd(this_row.Start));
            iTemp.setProperty("date_due_target", cd(this_row.Finish));
            if (this_row.Milestone)
            {
                // this is a milestone
                iTemp.setProperty("is_milestone", "1");
                iTemp.setProperty("expected_duration", "0");
            }
            else
            {
                // not a milestone
                iTemp.setProperty("expected_duration", (this_row.Duration / 60 / 8).ToString()); //Duration is in minutes
            }
            commit(iTemp, r.ToString()+ " Item",null);

            //Add relationship
            Item iRel =innov.newItem("WBS Activity2", "add");
            //iRel.setID(relationship_id[r]);
            if (ap.Tasks[r].OutlineParent.ID == 0)
            {
                iRel.setProperty("source_id", wbs_id);
            }
            else
            {
                iRel.setProperty("source_id", row_id[this_row.OutlineParent.ID]);
            }
            iRel.setProperty("related_id", row_id[r]);
            commit(iRel, r.ToString() + " Rel",null);

            //Add Assigments
            addAssignments(r);
            // Promote Activity Complete NB Assignments must be added first because
            // adding an assignment makes a complete activity active again
            if (ap.Tasks[r].PercentComplete == 100)
            {
                Item prom = innov.newItem("Activity2", "promoteItem");
                prom.setID(row_id[r]);
                prom.setProperty("state", "Complete");
                Item result = prom.apply();
                if (result.isError())
                {
                    MessageBox.Show("Promotion Error at row " + r.ToString() + ". " + result.getErrorDetail());
                    return;
                }
            }
        }

        private void addAssignments(int r) {

            /*
                This function tries to use MS Project Resource names.
                The result will be most meaningful if Resource names match Alias Identities in Innovator,
                or values in the 'Project Team' List
            */

            // variable this_row is in scope from calling function
            Task this_row = ap.Tasks[r];
            int asst_ct = this_row.Assignments.Count;
            for (int j = 1; j < asst_ct + 1; j++)
            {
                string idAsst = "";
                Assignment this_asst = this_row.Assignments[j];
                // if this is a Project and not a Template we try to find a User
                // whose name matches the MSProject Resource name

                    Item iUsr = innov.newItem("User", "get");
                    Item rAlias = innov.newItem("Alias", "get");
                    iUsr.setProperty("keyed_name", this_asst.ResourceName);
                    iUsr.setAttribute("select", "id");
                    rAlias.setAttribute("select", "id,related_id");
                    iUsr.addRelationship(rAlias);
                    Item result = iUsr.apply();
                    if (!result.isError())
                    {
                        idAsst = result.getRelationships("Alias").getItemByIndex(0).getProperty("related_id");
                    }

                // create an assignment Item
                Item rAsst = innov.newItem("Activity2 Assignment", "add");
                rAsst.setProperty("source_id", row_id[this_row.ID]);
                rAsst.setProperty("percent_load", (this_asst.Units * 100).ToString());
                rAsst.setProperty("work_est", (this_asst.Work / 60).ToString());
                // if the id of an Identity is known use it, otherwise use the Resource name as a role
                if (idAsst!="")
                {
                    rAsst.setProperty("related_id", idAsst);
                }
                else
                {
                    rAsst.setProperty("role", this_asst.ResourceName);
                }
                // apply the assignment Item, and promote it to complete if the MSProject row is complete
                if (ap.Tasks[r].PercentComplete == 100)
                {
                    commit(rAsst, r.ToString() + " Asst", "Complete");
                }
                else
                {
                    commit(rAsst, r.ToString() + " Asst", null);
                }
            }
        }

        private void addPredecessors() {
            for (int r = 1; r < task_ct + 1; r++)
            {
                Task this_row = ap.Tasks[r];
                if (!this_row.Rollup)
                {
                    var pred_ct = this_row.TaskDependencies.Count;
                    for (int i = 1; i < pred_ct + 1; i++)
                    {
                        var this_pred = this_row.TaskDependencies[i];
                        if (this_pred.To.ID == this_row.ID)
                        {
                            //look only at Predecessors not Successors
                            string predType="";
                            Item iPred = innov.newItem("Predecessor", "add");
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
                            iPred.setProperty("precedence_type", predType);
                            iPred.setProperty("lead_lag", (this_pred.Lag / 60 / 8).ToString()); //Lag is minutes in 8 hour day
                            iPred.setProperty("source_id", row_id[r]);
                            iPred.setProperty("related_id", row_id[this_pred.From.ID]);
                            commit(iPred, this_row.ID + " Predecessor " + i.ToString(),null);
                        }
                    }
                }
            }


        }
        private void commit(Item item, string x, string state)
        {

            var result = item.apply();
            if (result.isError())
            {
                MessageBox.Show("Error at row " + x.ToString() + ". " + result.getErrorString());
                return;
            }
            if (state!=null)
            {
                if (result.getProperty("state") != state)
                {
                    var prom = innov.newItem(item.getType(), "promoteItem");
                    prom.setID(item.getID());
                    prom.setProperty("state", state);
                    result = prom.apply();
                    if (result.isError())
                    {
                        MessageBox.Show("Promotion error at row " + x.ToString() + ". " + result.getErrorDetail());
                        return;
                    }
                }
            }
        }

        private string cd( DateTime d)
        {
            string d_string = d.Date.ToString("u");
            // we need to pass a string to ConvertToNeutral
            DateTimeFormatInfo dtfi =
               new CultureInfo(cntx.GetLocale(), false).DateTimeFormat;
            //get a DateTimeFormat
            string pattern = dtfi.UniversalSortableDateTimePattern;
            // identify the format we are using
            d_string = cntx.ConvertToNeutral(d_string, "date", pattern);
            // return the new launch date in neutral format
            return d_string;
        }
        private DateTime cs(string d)
        {
            return DateTime.Parse(cntx.ConvertFromNeutral(d, "date", "short_date"));
        }




        private void button2_Click(object sender, RibbonControlEventArgs e)
        {
            I18NSessionContext cntx = innov.getI18NSessionContext();
            ap = Globals.ThisAddIn.Application.ActiveProject;

            // first call server method to get an ordered list of project rows
            Item z = innov.newItem("Project", "select_project_tree");
            string this_project_id = dd_projects.SelectedItem.Tag.ToString(); //"5266E1D280F84861A1BBCF5E50A9C65B";
            z.setID(this_project_id);
            Item tree = z.apply();
                    if (tree.isError())
                    {
                       MessageBox.Show ("Error getting Project Tree");
                       return;
                    }
            // create a dictionary object to convert ids to row numbers
            // using key=id, value= row number
            Dictionary<string, string> d = new Dictionary<string, string>();
            // create a dictionary object to convert converting scheduling_type values to MSProject constants

           
            //ap = Globals.ThisAddIn.Application.ActiveProject = new Project();
                   string root = tree.getItemByIndex(0).getProperty("id");
                    var at = ap.Tasks;
                    var r = ap.Resources;
                    var u = 1; // counter for unknown assigments
                               // populate the MSProject and dictionary object
                   for (var x = 1; x < tree.getItemCount(); x++)
                    {
                Item task = tree.getItemByIndex(x);
                d.Add(task.getProperty("id"), (Int16.Parse(task.getProperty("n")) - 1).ToString());
                at.Add(task.getProperty("name"));
                int lev = Int16.Parse(task.getProperty("l"));
                if (lev > at[x].OutlineLevel)
                {
                    at[x].OutlineIndent();
                }
                else
                {
                    while (at[x].OutlineLevel > lev)
                    {
                        at[x].OutlineOutdent();
                    }
                }
                switch (task.getType())
                {
                    case "W":
                        Item wbs = innov.newItem("WBS Element", "get");
                        wbs.setID(task.getProperty("id"));
                        wbs = wbs.apply();
                        break;
                    case "A":
                        Item act = innov.newItem("Activity2", "get");
                        act.setID(task.getProperty("id"));
                        var assts = innov.newItem("Activity2 Assignment", "get");
                        assts.setAttribute("select", "role,related_id,percent_load,work_est");
                        assts.setAttribute("related_expand", "0");
                        act.addRelationship(assts);
                        act = act.apply();
                        at[x].Duration = act.getProperty("expected_duration");
                        //if (!is_template)
                        //{
                            at[x].Start = cs(act.getProperty("date_start_sched"));
                            at[x].Finish =cs(act.getProperty("date_due_sched"));
                        //}
                        at[x].Estimated = (System.Boolean)false;
                        var res = act.getItemsByXPath("//Item[@type='Activity2 Assignment']");
                        for (var y = 0; y < res.getItemCount(); y++)
                        {
                            string name = res.getItemByIndex(y).getPropertyAttribute("related_id", "keyed_name");
                            if (string.IsNullOrEmpty(name))
                            {
                                name = res.getItemByIndex(y).getProperty("role", "Unknown");
                                if (name == "Unknown") { name += u; u += 1; }
                            }
                            var units = res.getItemByIndex(y).getProperty("percent_load");
                            try { var n = r[name]; } catch (System.Exception ex) { r.Add(name); }
                            at[x].Assignments.Add(at[x].ID, r[name].ID);
                        }
                        break;
                }
                //ap.ScreenUpdating=true; 
                //ap.SelectTaskField(1, "Name");
                //ap.ScreenUpdating=false;
            }


            // traverse tasks again, the first time there could have been predecessors not created yet
            for (int x = 1; x < tree.getItemCount(); x++)
            {
                Item task = tree.getItemByIndex(x);

                if (task.getType() == "A")
                {
                    Item act = innov.newItem("Activity2", "get");
                    act.setID(task.getProperty("id"));
                    var preds = innov.newItem("Predecessor", "get");
                    preds.setAttribute("select", "related_id,precedence_type,lead_lag");
                    preds.setAttribute("related_expand", "0");
                    act.addRelationship(preds);
                    act = act.apply();
                    var pred = act.getItemsByXPath("//Item[@type='Predecessor']");
                    for (int y = 0; y < pred.getItemCount(); y++)
                    {
                        string PredID = d[pred.getItemByIndex(y).getProperty("related_id")];
                        PjTaskLinkType precType = (PjTaskLinkType)pj_prec[pred.getItemByIndex(y).getProperty("precedence_type")];
                        var lead = pred.getItemByIndex(y).getProperty("lead_lag");
                        at[x].TaskDependencies.Add(at[short.Parse(PredID)], precType, lead);
                    }
                }
            }
            //ap.ScreenUpdating=true;
            //ap.Calculation = -1;

        }
    }
}
