﻿using Aras.IOM;
using Microsoft.Office.Interop.MSProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MS_Project_Import_Export
{
    public class ProjectLoader
    {
        private List<Row> rows;
        private Item thisItem;

        #region Dictionaries

        private static readonly Dictionary<string, short> predecessorTypes = new Dictionary<string, short>
            {
                { "Finish to Finish",0},
                { "Finish to Start" ,1},
                { "Start to Finish" ,2},
                { "Start to Start"  ,3}
            };

        private static readonly Dictionary<string, string> dependencyTypes = new Dictionary<string, string>
        {
            {"pjFinishToFinish", "Finish to Finish"},
            {"pjFinishToStart", "Finish to Start"},
            {"pjStartToFinsih", "Start to Finish" },
            {"pjStartToStart", "Start to Start" }
        };

        #endregion

            #region Public methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activeProject"></param>
        /// <param name="errorString">Empty string if a prject successfully uploaded</param>
        /// <returns></returns>
        public bool UploadProject(Project activeProject, out string errorString)
        {
            rows = new List<Row>();
            thisItem = InnovatorManager.Instance.CreateNewItem();
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
                errorString = string.Format(Properties.Resources.ERROR_ROLLUP_NOT_ALLOWED, errorIds);
                return false;
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
            topWBSItem.setProperty("name", activeProject.Title);
            topWBSItem.setProperty("is_top", "1");
            applyThisItem(topWBSItem);

            Item projectItem = InnovatorManager.Instance.CreateProjectItem(activeProject.Name, topWBSItem.getID(), activeProject.Start, activeProject.Finish);
            applyThisItem(projectItem);

            processRows(rows, topWBSItem.getID(), projectItem.getProperty("project_number"));
            addPredecessors();

            var sProject = InnovatorManager.Instance.CreateNewItem("Project");
            sProject.setID(projectItem.getID());
            sProject = sProject.apply("Schedule Project");

            if (sProject.isError())
            {
                errorString = sProject.getErrorString(); /// TODO where is errorString declared?
                return false;
            }

            errorString = string.Empty;
            return true;
        }

        public void DownloadProject(Project activeProject, string projectId)
        {
            var ap = activeProject;

            // first call server method to get an ordered list of project rows
            Item z = InnovatorManager.Instance.CreateNewItem("Project", "select_project_tree");
            //Item z = innov.newItem("Project", "select_project_tree");
            //string this_project_id = dd_projects.SelectedItem.Tag.ToString(); //"5266E1D280F84861A1BBCF5E50A9C65B";
            z.setID(projectId);
            Item tree = z.apply();
            if (tree.isError())
            {
                MessageBox.Show("Error getting Project Tree");
                return;
            }
            // create a dictionary object to convert ids to row numbers
            // using key=id, value= row number
            Dictionary<string, string> d = new Dictionary<string, string>();
 
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
                        Item wbs = InnovatorManager.Instance.CreateNewItem("WBS Element", "get");
                        wbs.setID(task.getProperty("id"));
                        wbs = wbs.apply();
                        break;
                    case "A":
                        Item act = InnovatorManager.Instance.CreateNewItem("Activity2", "get");
                        act.setID(task.getProperty("id"));
                        var assts = InnovatorManager.Instance.CreateNewItem("Activity2 Assignment", "get");
                        assts.setAttribute("select", "role,related_id,percent_load,work_est");
                        assts.setAttribute("related_expand", "0");
                        act.addRelationship(assts);
                        act = act.apply();
                        at[x].Duration = act.getProperty("expected_duration");
                        at[x].Start = InnovatorManager.Instance.InnovatorDateToLocalDate(act.getProperty("date_due_sched"));
                        at[x].Finish = InnovatorManager.Instance.InnovatorDateToLocalDate(act.getProperty("date_due_sched"));
                        at[x].Estimated = false;
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
            }

            // traverse tasks again, the first time there could have been predecessors not created yet
            for (int x = 1; x < tree.getItemCount(); x++)
            {
                Item task = tree.getItemByIndex(x);

                if (task.getType() == "A")
                {
                    Item act = InnovatorManager.Instance.CreateNewItem("Activity2", "get");
                    act.setID(task.getProperty("id"));
                    var preds = InnovatorManager.Instance.CreateNewItem("Predecessor", "get");
                    preds.setAttribute("select", "related_id,precedence_type,lead_lag");
                    preds.setAttribute("related_expand", "0");
                    act.addRelationship(preds);
                    act = act.apply();
                    var pred = act.getItemsByXPath("//Item[@type='Predecessor']");
                    for (int y = 0; y < pred.getItemCount(); y++)
                    {
                        string PredID = d[pred.getItemByIndex(y).getProperty("related_id")];
                        PjTaskLinkType precType = (PjTaskLinkType) predecessorTypes[pred.getItemByIndex(y).getProperty("precedence_type")];
                        var lead = pred.getItemByIndex(y).getProperty("lead_lag");
                        at[x].TaskDependencies.Add(at[short.Parse(PredID)], precType, lead);
                    }
                }
            }
        }

        #endregion

        #region Private methods

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
            applyThisItem(row.Item);
            // add subWBS
            var relationship = InnovatorManager.Instance.CreateNewItem("Sub WBS", "add");
            var sourceId = (row.Task.OutlineParent.ID == 0) ? wbsId : rows.FirstOrDefault(r => r.Task.ID == row.Task.OutlineParent.ID)?.Item.getID();
            relationship.setProperty("source_id", sourceId);
            relationship.setProperty("related_id", row.Item.getID());
            applyThisItem(relationship);
        }

        private void addActivity(Row row, string wbsId, string projectNumber)
        {
            row.Item.setProperty("name", row.Task.Name);
            row.Item.setProperty("proj_num", projectNumber);

            if (!string.IsNullOrEmpty(row.Task.Notes))
            {
                row.Item.setProperty("description", row.Task.Notes);
            }

            row.Item.setProperty("work_est", Math.Round((double)row.Task.Work / 60).ToString()); //Work is in minutes
            row.Item.setProperty("date_start_target", InnovatorManager.Instance.LocalDateToInnovatorDate((DateTime)row.Task.Start));
            row.Item.setProperty("date_due_target", InnovatorManager.Instance.LocalDateToInnovatorDate((DateTime)row.Task.Finish));

            if (row.Task.Milestone) // this is a milestone
            {
                row.Item.setProperty("is_milestone", "1");
                row.Item.setProperty("expected_duration", "0");
            }
            else // not a milestone
            {
                row.Item.setProperty("expected_duration", Math.Round((double)row.Task.Duration / 60 / 8).ToString()); //Duration is in minutes
            }
            applyThisItem(row.Item);

            Item relationship = InnovatorManager.Instance.CreateNewItem("WBS Activity2", "add");
            var sourceId = (row.Task.OutlineParent.ID == 0) ? wbsId : rows.FirstOrDefault(r => r.Task.ID == row.Task.OutlineParent.ID)?.Item.getID();
            relationship.setProperty("source_id", sourceId);
            relationship.setProperty("related_id", row.Item.getID());
            applyThisItem(relationship);

            //Add Assigments
            addAssignments(row);

            // Promote Activity Complete NB Assignments must be added first because
            // adding an assignment makes a complete activity active again
            if (row.Task.PercentComplete == 100)
            {
                Item promItem = InnovatorManager.Instance.CreatePromotionItem("Activity2", "promoteItem", row.Item.getID(), "Complete");
                applyThisItem(promItem);
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
                string assignmentId = string.Empty;
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
                    assignmentId = result.getRelationships("Alias").getItemByIndex(0).getProperty("related_id");
                }

                // create an assignment Item
                Item assignmentRelation = InnovatorManager.Instance.CreateNewItem("Activity2 Assignment", "add");
                assignmentRelation.setProperty("source_id", row.Item.getID());
                assignmentRelation.setProperty("percent_load", (assignment.Units * 100).ToString());
                assignmentRelation.setProperty("work_est", Math.Round((double)assignment.Work / 60).ToString());

                // if the id of an Identity is known use it, otherwise use the Resource name as a role
                if (!string.IsNullOrEmpty(assignmentId))
                {
                    assignmentRelation.setProperty("related_id", assignmentId);
                }
                else
                {
                    assignmentRelation.setProperty("role", assignment.ResourceName);
                }

                // apply the assignment Item, and promote it to complete if the MSProject row is complete
                applyThisItem(assignmentRelation);
                if (row.Task.PercentComplete == 100)
                {
                    var promItem = InnovatorManager.Instance.CreatePromotionItem(assignmentRelation.getType(), "promoteItem",
                        assignmentRelation.getID(), "Complete");
                    applyThisItem(promItem);
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

                foreach (TaskDependency dependency in row.Task.TaskDependencies)
                {
                    if (dependency.To.ID == row.Task.ID)
                    {
                        //look only at Predecessors not Successors
                        string predType = string.Empty;
                        dependencyTypes.TryGetValue(dependency.Type.ToString(), out predType);

                        Item predItem = InnovatorManager.Instance.CreateNewItem("Predecessor", "add");
                        predItem.setProperty("precedence_type", predType);
                        predItem.setProperty("lead_lag", Math.Round((double)dependency.Lag / 60 / 8).ToString()); //Lag is minutes in 8 hour day
                        predItem.setProperty("source_id", row.Item.getID());
                        predItem.setProperty("related_id", rows.FirstOrDefault(r => r.Task.ID == dependency.From.ID)?.Item.getID());
                        applyThisItem(predItem);
                    }
                }
            }
        }

        private void setTaskFromActivity(Task task, Item item, Resources resources, ref int uasCount)
        {
            task.Duration = item.getProperty("expected_duration");
            task.Start = InnovatorManager.Instance.InnovatorDateToLocalDate(item.getProperty("date_start_target", item.getProperty("date_start_sched")));
            task.Finish = InnovatorManager.Instance.InnovatorDateToLocalDate(item.getProperty("date_due_target", item.getProperty("date_due_sched")));
            task.Estimated = false;
            var assignments = item.getRelationships("Activity2 Assignment");
            for (var i = 0; i < assignments.getItemCount(); i++)
            {
                string name = assignments.getItemByIndex(i).getPropertyAttribute("related_id", "keyed_name");
                if (string.IsNullOrEmpty(name))
                {
                    name = assignments.getItemByIndex(i).getProperty("role", Properties.Resources.UNKNOWN_ASSIGNMENT);
                    name = (name == Properties.Resources.UNKNOWN_ASSIGNMENT) ? name + uasCount++ : name;
                }

                try
                {
                    var temp = resources[name];
                }
                catch
                {
                    resources.Add(name);
                }
                task.Assignments.Add(task.ID, resources[name].ID);
            }
        }

        private void setPredecessors(Item rootWBS, Tasks tasks, Dictionary<string, string> rowsIds)
        {
            var activities = rootWBS.getItemsByXPath("//Item[@type='Activity2']");
            for (int i = 0; i < activities.getItemCount(); i++)
            {
                var activity = activities.getItemByIndex(i);
                var predecessors = activity.getRelationships("Predecessor");
                for (int y = 0; y < predecessors.getItemCount(); y++)
                {
                    int taskIndex = 0;
                    if (!int.TryParse(activity.getProperty("inumber", string.Empty), out taskIndex))
                    {
                        continue;
                    }

                    if (rowsIds.ContainsKey(predecessors.getItemByIndex(y).getProperty("related_id")))
                    {
                        string predID = rowsIds[predecessors.getItemByIndex(y).getProperty("related_id")];
                        PjTaskLinkType precType = (PjTaskLinkType)predecessorTypes[predecessors.getItemByIndex(y).getProperty("precedence_type")];
                        var lead = predecessors.getItemByIndex(y).getProperty("lead_lag");
                        tasks[taskIndex].TaskDependencies.Add(tasks[short.Parse(predID)], precType, lead);
                    }
                }
            }
        }

        private void applyThisItem(Item this_item)
        {
            this_item = this_item.apply();
            if (this_item.isError())
            {
                MessageBox.Show(this_item.getErrorString(), Properties.Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        #endregion
    }
}