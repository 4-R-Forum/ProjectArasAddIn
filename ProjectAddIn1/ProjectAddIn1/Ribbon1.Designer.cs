namespace MS_Project_Import_Export
{
    partial class Ribbon1 : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public Ribbon1()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Microsoft.Office.Tools.Ribbon.RibbonDialogLauncher ribbonDialogLauncherImpl1 = this.Factory.CreateRibbonDialogLauncher();
            this.tab1 = this.Factory.CreateRibbonTab();
            this.ArasMSProject = this.Factory.CreateRibbonGroup();
            this.btn_loginToAras = this.Factory.CreateRibbonButton();
            this.btn_projectToAras = this.Factory.CreateRibbonButton();
            this.btn_arasToProject = this.Factory.CreateRibbonButton();
            this.dd_projects = this.Factory.CreateRibbonDropDown();
            this.tab1.SuspendLayout();
            this.ArasMSProject.SuspendLayout();
            this.SuspendLayout();
            // 
            // tab1
            // 
            this.tab1.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.tab1.Groups.Add(this.ArasMSProject);
            this.tab1.Label = "TabAddIns";
            this.tab1.Name = "tab1";
            // 
            // ArasMSProject
            // 
            ribbonDialogLauncherImpl1.Enabled = false;
            this.ArasMSProject.DialogLauncher = ribbonDialogLauncherImpl1;
            this.ArasMSProject.Items.Add(this.btn_loginToAras);
            this.ArasMSProject.Items.Add(this.btn_projectToAras);
            this.ArasMSProject.Items.Add(this.btn_arasToProject);
            this.ArasMSProject.Items.Add(this.dd_projects);
            this.ArasMSProject.Label = "Login to Aras";
            this.ArasMSProject.Name = "ArasMSProject";
            // 
            // btn_loginToAras
            // 
            this.btn_loginToAras.Label = "Login to Aras";
            this.btn_loginToAras.Name = "btn_loginToAras";
            this.btn_loginToAras.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btn_loginToAras_Click);
            // 
            // btn_projectToAras
            // 
            this.btn_projectToAras.Label = "MSProject to Aras";
            this.btn_projectToAras.Name = "btn_projectToAras";
            this.btn_projectToAras.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btn_projectToAras_Click);
            // 
            // btn_arasToProject
            // 
            this.btn_arasToProject.Label = "Aras to MSProject";
            this.btn_arasToProject.Name = "btn_arasToProject";
            this.btn_arasToProject.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btn_arasToProject_Click);
            // 
            // dd_projects
            // 
            this.dd_projects.Label = "Select";
            this.dd_projects.Name = "dd_projects";
            // 
            // Ribbon1
            // 
            this.Name = "Ribbon1";
            this.RibbonType = "Microsoft.Project.Project";
            this.Tabs.Add(this.tab1);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.Ribbon1_Load);
            this.tab1.ResumeLayout(false);
            this.tab1.PerformLayout();
            this.ArasMSProject.ResumeLayout(false);
            this.ArasMSProject.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup ArasMSProject;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btn_projectToAras;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btn_arasToProject;
        internal Microsoft.Office.Tools.Ribbon.RibbonDropDown dd_projects;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btn_loginToAras;
    }

    partial class ThisRibbonCollection
    {
        internal Ribbon1 Ribbon1
        {
            get { return this.GetRibbon<Ribbon1>(); }
        }
    }
}
