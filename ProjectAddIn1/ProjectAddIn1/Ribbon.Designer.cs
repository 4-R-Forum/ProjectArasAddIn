namespace MS_Project_Import_Export
{
    partial class Ribbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public Ribbon()
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
            this.Login = this.Factory.CreateRibbonGroup();
            this.btn_loginToAras = this.Factory.CreateRibbonButton();
            this.btn_projectToAras = this.Factory.CreateRibbonButton();
            this.btn_arasToProject = this.Factory.CreateRibbonButton();
            this.dd_projects = this.Factory.CreateRibbonDropDown();
            this.upload = this.Factory.CreateRibbonGroup();
            this.download = this.Factory.CreateRibbonGroup();
            this.tab1.SuspendLayout();
            this.Login.SuspendLayout();
            this.upload.SuspendLayout();
            this.download.SuspendLayout();
            this.SuspendLayout();
            // 
            // tab1
            // 
            this.tab1.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.tab1.Groups.Add(this.Login);
            this.tab1.Groups.Add(this.upload);
            this.tab1.Groups.Add(this.download);
            this.tab1.Label = "TabAddIns";
            this.tab1.Name = "tab1";
            // 
            // Login
            // 
            ribbonDialogLauncherImpl1.Enabled = false;
            this.Login.DialogLauncher = ribbonDialogLauncherImpl1;
            this.Login.Items.Add(this.btn_loginToAras);
            this.Login.Label = "Login";
            this.Login.Name = "Login";
            // 
            // btn_loginToAras
            // 
            this.btn_loginToAras.Label = "Login to Aras";
            this.btn_loginToAras.Name = "btn_loginToAras";
            this.btn_loginToAras.ScreenTip = "Click for Login Dialog";
            this.btn_loginToAras.ShowImage = true;
            this.btn_loginToAras.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btn_loginToAras_Click);
            // 
            // btn_projectToAras
            // 
            this.btn_projectToAras.Enabled = false;
            this.btn_projectToAras.Label = "MSProject to Aras";
            this.btn_projectToAras.Name = "btn_projectToAras";
            this.btn_projectToAras.ScreenTip = "Click to Upload";
            this.btn_projectToAras.ShowImage = true;
            this.btn_projectToAras.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btn_projectToAras_Click);
            // 
            // btn_arasToProject
            // 
            this.btn_arasToProject.Enabled = false;
            this.btn_arasToProject.Label = "Aras to MSProject";
            this.btn_arasToProject.Name = "btn_arasToProject";
            this.btn_arasToProject.ScreenTip = "Click to Download project";
            this.btn_arasToProject.ShowImage = true;
            this.btn_arasToProject.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btn_arasToProject_Click);
            // 
            // dd_projects
            // 
            this.dd_projects.Enabled = false;
            this.dd_projects.Label = "Select";
            this.dd_projects.Name = "dd_projects";
            this.dd_projects.ScreenTip = "Select Project number";
            // 
            // upload
            // 
            this.upload.Items.Add(this.btn_projectToAras);
            this.upload.Label = "Upload";
            this.upload.Name = "upload";
            // 
            // download
            // 
            this.download.Items.Add(this.dd_projects);
            this.download.Items.Add(this.btn_arasToProject);
            this.download.Label = "Download";
            this.download.Name = "download";
            // 
            // Ribbon
            // 
            this.Name = "Ribbon";
            this.RibbonType = "Microsoft.Project.Project";
            this.Tabs.Add(this.tab1);
            this.tab1.ResumeLayout(false);
            this.tab1.PerformLayout();
            this.Login.ResumeLayout(false);
            this.Login.PerformLayout();
            this.upload.ResumeLayout(false);
            this.upload.PerformLayout();
            this.download.ResumeLayout(false);
            this.download.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup Login;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btn_projectToAras;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btn_arasToProject;
        internal Microsoft.Office.Tools.Ribbon.RibbonDropDown dd_projects;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btn_loginToAras;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup upload;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup download;
    }

    partial class ThisRibbonCollection
    {
        internal Ribbon Ribbon1
        {
            get { return this.GetRibbon<Ribbon>(); }
        }
    }
}
