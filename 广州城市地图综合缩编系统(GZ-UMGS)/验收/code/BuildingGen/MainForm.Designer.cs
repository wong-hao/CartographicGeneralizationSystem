namespace BuildingGen
{
    partial class MainForm
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
            //Ensures that any ESRI libraries that have been used are unloaded in the correct order. 
            //Failure to do this may result in random crashes on exit due to the operating system unloading 
            //the libraries in the incorrect order. 
            ESRI.ArcGIS.ADF.COMSupport.AOUninitialize.Shutdown();

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
          System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
          this.axMapControl1 = new ESRI.ArcGIS.Controls.AxMapControl();
          this.axToolbarControl1 = new ESRI.ArcGIS.Controls.AxToolbarControl();
          this.axTOCControl1 = new ESRI.ArcGIS.Controls.AxTOCControl();
          this.statusStrip1 = new System.Windows.Forms.StatusStrip();
          this.statusBarXY = new System.Windows.Forms.ToolStripStatusLabel();
          this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
          this.statusbarScale = new System.Windows.Forms.ToolStripStatusLabel();
          this.axToolbarControl2 = new ESRI.ArcGIS.Controls.AxToolbarControl();
          this.editToolBar = new ESRI.ArcGIS.Controls.AxToolbarControl();
          this.axLicenseControl1 = new ESRI.ArcGIS.Controls.AxLicenseControl();
          this.axToolbarControl3 = new ESRI.ArcGIS.Controls.AxToolbarControl();
          this.menuStrip1 = new System.Windows.Forms.MenuStrip();
          this.menuDoc = new System.Windows.Forms.ToolStripMenuItem();
          this.menuNewDoc = new System.Windows.Forms.ToolStripMenuItem();
          this.menuOpenDoc = new System.Windows.Forms.ToolStripMenuItem();
          this.menuSaveDoc = new System.Windows.Forms.ToolStripMenuItem();
          this.menuCloseDoc = new System.Windows.Forms.ToolStripMenuItem();
          this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
          this.miWorkspaceInfo = new System.Windows.Forms.ToolStripMenuItem();
          this.menuView = new System.Windows.Forms.ToolStripMenuItem();
          this.menuViewLayer = new System.Windows.Forms.ToolStripMenuItem();
          this.menuViewShowGen = new System.Windows.Forms.ToolStripMenuItem();
          this.menuViewShowOrg = new System.Windows.Forms.ToolStripMenuItem();
          this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
          this.menuHighBuilding = new System.Windows.Forms.ToolStripMenuItem();
          this.menuHightRoad = new System.Windows.Forms.ToolStripMenuItem();
          this.menuHighWater = new System.Windows.Forms.ToolStripMenuItem();
          this.MenuHighPlant = new System.Windows.Forms.ToolStripMenuItem();
          this.Ҫ����ʾToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.menuViewBuildingGroup = new System.Windows.Forms.ToolStripMenuItem();
          this.menuViewBuildingStruct = new System.Windows.Forms.ToolStripMenuItem();
          this.menuViewRoadRank = new System.Windows.Forms.ToolStripMenuItem();
          this.������ʾToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
          this.menuScaleOrg = new System.Windows.Forms.ToolStripMenuItem();
          this.menuScaleGen = new System.Windows.Forms.ToolStripMenuItem();
          this.menuGen = new System.Windows.Forms.ToolStripMenuItem();
          this.menuGenPara = new System.Windows.Forms.ToolStripMenuItem();
          this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
          this.��·�ۺ�ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.�������ۺ�ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.ˮϵ�ۺ�ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.ֲ���ۺ�ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.menuData = new System.Windows.Forms.ToolStripMenuItem();
          this.menuHelp = new System.Windows.Forms.ToolStripMenuItem();
          this.����ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.outlookBar1 = new OutlookBar.OutlookBar();
          ((System.ComponentModel.ISupportInitialize)(this.axMapControl1)).BeginInit();
          ((System.ComponentModel.ISupportInitialize)(this.axToolbarControl1)).BeginInit();
          ((System.ComponentModel.ISupportInitialize)(this.axTOCControl1)).BeginInit();
          this.statusStrip1.SuspendLayout();
          ((System.ComponentModel.ISupportInitialize)(this.axToolbarControl2)).BeginInit();
          ((System.ComponentModel.ISupportInitialize)(this.editToolBar)).BeginInit();
          ((System.ComponentModel.ISupportInitialize)(this.axLicenseControl1)).BeginInit();
          ((System.ComponentModel.ISupportInitialize)(this.axToolbarControl3)).BeginInit();
          this.menuStrip1.SuspendLayout();
          this.SuspendLayout();
          // 
          // axMapControl1
          // 
          this.axMapControl1.Dock = System.Windows.Forms.DockStyle.Fill;
          this.axMapControl1.Location = new System.Drawing.Point(229, 81);
          this.axMapControl1.Name = "axMapControl1";
          this.axMapControl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axMapControl1.OcxState")));
          this.axMapControl1.Size = new System.Drawing.Size(602, 438);
          this.axMapControl1.TabIndex = 2;
          this.axMapControl1.OnMapReplaced += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnMapReplacedEventHandler(this.axMapControl1_OnMapReplaced);
          this.axMapControl1.OnMouseMove += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnMouseMoveEventHandler(this.axMapControl1_OnMouseMove);
          // 
          // axToolbarControl1
          // 
          this.axToolbarControl1.Dock = System.Windows.Forms.DockStyle.Right;
          this.axToolbarControl1.Location = new System.Drawing.Point(831, 81);
          this.axToolbarControl1.Name = "axToolbarControl1";
          this.axToolbarControl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axToolbarControl1.OcxState")));
          this.axToolbarControl1.Size = new System.Drawing.Size(28, 438);
          this.axToolbarControl1.TabIndex = 3;
          // 
          // axTOCControl1
          // 
          this.axTOCControl1.Dock = System.Windows.Forms.DockStyle.Left;
          this.axTOCControl1.Location = new System.Drawing.Point(0, 81);
          this.axTOCControl1.Name = "axTOCControl1";
          this.axTOCControl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axTOCControl1.OcxState")));
          this.axTOCControl1.Size = new System.Drawing.Size(29, 460);
          this.axTOCControl1.TabIndex = 4;
          this.axTOCControl1.Visible = false;
          // 
          // statusStrip1
          // 
          this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusBarXY,
            this.toolStripStatusLabel1,
            this.statusbarScale});
          this.statusStrip1.Location = new System.Drawing.Point(229, 519);
          this.statusStrip1.Name = "statusStrip1";
          this.statusStrip1.Size = new System.Drawing.Size(630, 22);
          this.statusStrip1.Stretch = false;
          this.statusStrip1.TabIndex = 7;
          this.statusStrip1.Text = "statusBar1";
          // 
          // statusBarXY
          // 
          this.statusBarXY.AutoSize = false;
          this.statusBarXY.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
          this.statusBarXY.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
          this.statusBarXY.Name = "statusBarXY";
          this.statusBarXY.Size = new System.Drawing.Size(150, 17);
          this.statusBarXY.Text = "Test 123";
          // 
          // toolStripStatusLabel1
          // 
          this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
          this.toolStripStatusLabel1.Size = new System.Drawing.Size(11, 17);
          this.toolStripStatusLabel1.Text = "|";
          // 
          // statusbarScale
          // 
          this.statusbarScale.Name = "statusbarScale";
          this.statusbarScale.Size = new System.Drawing.Size(24, 17);
          this.statusbarScale.Text = "1:?";
          // 
          // axToolbarControl2
          // 
          this.axToolbarControl2.Dock = System.Windows.Forms.DockStyle.Top;
          this.axToolbarControl2.Location = new System.Drawing.Point(0, 53);
          this.axToolbarControl2.Name = "axToolbarControl2";
          this.axToolbarControl2.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axToolbarControl2.OcxState")));
          this.axToolbarControl2.Size = new System.Drawing.Size(859, 28);
          this.axToolbarControl2.TabIndex = 8;
          this.axToolbarControl2.Visible = false;
          // 
          // editToolBar
          // 
          this.editToolBar.Location = new System.Drawing.Point(393, 206);
          this.editToolBar.Name = "editToolBar";
          this.editToolBar.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("editToolBar.OcxState")));
          this.editToolBar.Size = new System.Drawing.Size(265, 28);
          this.editToolBar.TabIndex = 9;
          this.editToolBar.Visible = false;
          // 
          // axLicenseControl1
          // 
          this.axLicenseControl1.Enabled = true;
          this.axLicenseControl1.Location = new System.Drawing.Point(307, 360);
          this.axLicenseControl1.Name = "axLicenseControl1";
          this.axLicenseControl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axLicenseControl1.OcxState")));
          this.axLicenseControl1.Size = new System.Drawing.Size(32, 32);
          this.axLicenseControl1.TabIndex = 10;
          // 
          // axToolbarControl3
          // 
          this.axToolbarControl3.Dock = System.Windows.Forms.DockStyle.Top;
          this.axToolbarControl3.Location = new System.Drawing.Point(0, 25);
          this.axToolbarControl3.Name = "axToolbarControl3";
          this.axToolbarControl3.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axToolbarControl3.OcxState")));
          this.axToolbarControl3.Size = new System.Drawing.Size(859, 28);
          this.axToolbarControl3.TabIndex = 11;
          this.axToolbarControl3.Visible = false;
          // 
          // menuStrip1
          // 
          this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuDoc,
            this.menuView,
            this.menuGen,
            this.menuData,
            this.menuHelp});
          this.menuStrip1.Location = new System.Drawing.Point(0, 0);
          this.menuStrip1.Name = "menuStrip1";
          this.menuStrip1.Size = new System.Drawing.Size(859, 25);
          this.menuStrip1.TabIndex = 12;
          this.menuStrip1.Text = "menuStrip1";
          // 
          // menuDoc
          // 
          this.menuDoc.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuNewDoc,
            this.menuOpenDoc,
            this.menuSaveDoc,
            this.menuCloseDoc,
            this.toolStripMenuItem1,
            this.miWorkspaceInfo});
          this.menuDoc.Name = "menuDoc";
          this.menuDoc.Size = new System.Drawing.Size(68, 21);
          this.menuDoc.Text = "����׼��";
          // 
          // menuNewDoc
          // 
          this.menuNewDoc.Name = "menuNewDoc";
          this.menuNewDoc.Size = new System.Drawing.Size(136, 22);
          this.menuNewDoc.Text = "����������";
          // 
          // menuOpenDoc
          // 
          this.menuOpenDoc.Name = "menuOpenDoc";
          this.menuOpenDoc.Size = new System.Drawing.Size(136, 22);
          this.menuOpenDoc.Text = "�򿪹�����";
          // 
          // menuSaveDoc
          // 
          this.menuSaveDoc.Name = "menuSaveDoc";
          this.menuSaveDoc.Size = new System.Drawing.Size(136, 22);
          this.menuSaveDoc.Text = "���湤����";
          // 
          // menuCloseDoc
          // 
          this.menuCloseDoc.Name = "menuCloseDoc";
          this.menuCloseDoc.Size = new System.Drawing.Size(136, 22);
          this.menuCloseDoc.Text = "�رչ�����";
          // 
          // toolStripMenuItem1
          // 
          this.toolStripMenuItem1.Name = "toolStripMenuItem1";
          this.toolStripMenuItem1.Size = new System.Drawing.Size(133, 6);
          // 
          // miWorkspaceInfo
          // 
          this.miWorkspaceInfo.Name = "miWorkspaceInfo";
          this.miWorkspaceInfo.Size = new System.Drawing.Size(136, 22);
          this.miWorkspaceInfo.Text = "����������";
          this.miWorkspaceInfo.Click += new System.EventHandler(this.miWorkspaceInfo_Click);
          // 
          // menuView
          // 
          this.menuView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuViewLayer,
            this.Ҫ����ʾToolStripMenuItem,
            this.������ʾToolStripMenuItem,
            this.toolStripMenuItem4,
            this.menuScaleOrg,
            this.menuScaleGen});
          this.menuView.Name = "menuView";
          this.menuView.Size = new System.Drawing.Size(68, 21);
          this.menuView.Text = "��ʾЧ��";
          // 
          // menuViewLayer
          // 
          this.menuViewLayer.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuViewShowGen,
            this.menuViewShowOrg,
            this.toolStripMenuItem3,
            this.menuHighBuilding,
            this.menuHightRoad,
            this.menuHighWater,
            this.MenuHighPlant});
          this.menuViewLayer.Name = "menuViewLayer";
          this.menuViewLayer.Size = new System.Drawing.Size(136, 22);
          this.menuViewLayer.Text = "ͼ����ʾ";
          // 
          // menuViewShowGen
          // 
          this.menuViewShowGen.Checked = true;
          this.menuViewShowGen.CheckOnClick = true;
          this.menuViewShowGen.CheckState = System.Windows.Forms.CheckState.Checked;
          this.menuViewShowGen.Name = "menuViewShowGen";
          this.menuViewShowGen.Size = new System.Drawing.Size(136, 22);
          this.menuViewShowGen.Text = "��ʾ�ۺϲ�";
          this.menuViewShowGen.Click += new System.EventHandler(this.ShowMap);
          // 
          // menuViewShowOrg
          // 
          this.menuViewShowOrg.CheckOnClick = true;
          this.menuViewShowOrg.Name = "menuViewShowOrg";
          this.menuViewShowOrg.Size = new System.Drawing.Size(136, 22);
          this.menuViewShowOrg.Text = "��ʾ��ͼ��";
          this.menuViewShowOrg.Click += new System.EventHandler(this.ShowMap);
          // 
          // toolStripMenuItem3
          // 
          this.toolStripMenuItem3.Name = "toolStripMenuItem3";
          this.toolStripMenuItem3.Size = new System.Drawing.Size(133, 6);
          // 
          // menuHighBuilding
          // 
          this.menuHighBuilding.CheckOnClick = true;
          this.menuHighBuilding.Name = "menuHighBuilding";
          this.menuHighBuilding.Size = new System.Drawing.Size(136, 22);
          this.menuHighBuilding.Text = "ͻ��������";
          this.menuHighBuilding.Click += new System.EventHandler(this.ShowMap);
          // 
          // menuHightRoad
          // 
          this.menuHightRoad.CheckOnClick = true;
          this.menuHightRoad.Name = "menuHightRoad";
          this.menuHightRoad.Size = new System.Drawing.Size(136, 22);
          this.menuHightRoad.Text = "ͻ����·";
          this.menuHightRoad.Click += new System.EventHandler(this.ShowMap);
          // 
          // menuHighWater
          // 
          this.menuHighWater.CheckOnClick = true;
          this.menuHighWater.Name = "menuHighWater";
          this.menuHighWater.Size = new System.Drawing.Size(136, 22);
          this.menuHighWater.Text = "ͻ��ˮϵ";
          this.menuHighWater.Click += new System.EventHandler(this.ShowMap);
          // 
          // MenuHighPlant
          // 
          this.MenuHighPlant.CheckOnClick = true;
          this.MenuHighPlant.Name = "MenuHighPlant";
          this.MenuHighPlant.Size = new System.Drawing.Size(136, 22);
          this.MenuHighPlant.Text = "ͻ��ֲ��";
          this.MenuHighPlant.Click += new System.EventHandler(this.ShowMap);
          // 
          // Ҫ����ʾToolStripMenuItem
          // 
          this.Ҫ����ʾToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuViewBuildingGroup,
            this.menuViewBuildingStruct,
            this.menuViewRoadRank});
          this.Ҫ����ʾToolStripMenuItem.Name = "Ҫ����ʾToolStripMenuItem";
          this.Ҫ����ʾToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
          this.Ҫ����ʾToolStripMenuItem.Text = "Ҫ����ʾ";
          // 
          // menuViewBuildingGroup
          // 
          this.menuViewBuildingGroup.CheckOnClick = true;
          this.menuViewBuildingGroup.Name = "menuViewBuildingGroup";
          this.menuViewBuildingGroup.Size = new System.Drawing.Size(160, 22);
          this.menuViewBuildingGroup.Text = "ǿ�����������";
          this.menuViewBuildingGroup.Click += new System.EventHandler(this.ShowMap);
          // 
          // menuViewBuildingStruct
          // 
          this.menuViewBuildingStruct.CheckOnClick = true;
          this.menuViewBuildingStruct.Name = "menuViewBuildingStruct";
          this.menuViewBuildingStruct.Size = new System.Drawing.Size(160, 22);
          this.menuViewBuildingStruct.Text = "ǿ�������ṹ";
          this.menuViewBuildingStruct.Click += new System.EventHandler(this.ShowMap);
          // 
          // menuViewRoadRank
          // 
          this.menuViewRoadRank.CheckOnClick = true;
          this.menuViewRoadRank.Name = "menuViewRoadRank";
          this.menuViewRoadRank.Size = new System.Drawing.Size(160, 22);
          this.menuViewRoadRank.Text = "ǿ����·�ּ�";
          this.menuViewRoadRank.Click += new System.EventHandler(this.ShowMap);
          // 
          // ������ʾToolStripMenuItem
          // 
          this.������ʾToolStripMenuItem.Name = "������ʾToolStripMenuItem";
          this.������ʾToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
          this.������ʾToolStripMenuItem.Text = "������ʾ";
          // 
          // toolStripMenuItem4
          // 
          this.toolStripMenuItem4.Name = "toolStripMenuItem4";
          this.toolStripMenuItem4.Size = new System.Drawing.Size(133, 6);
          // 
          // menuScaleOrg
          // 
          this.menuScaleOrg.Name = "menuScaleOrg";
          this.menuScaleOrg.Size = new System.Drawing.Size(136, 22);
          this.menuScaleOrg.Text = "ԭʼ������";
          // 
          // menuScaleGen
          // 
          this.menuScaleGen.Name = "menuScaleGen";
          this.menuScaleGen.Size = new System.Drawing.Size(136, 22);
          this.menuScaleGen.Text = "�ۺϱ�����";
          // 
          // menuGen
          // 
          this.menuGen.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuGenPara,
            this.toolStripMenuItem2,
            this.��·�ۺ�ToolStripMenuItem,
            this.�������ۺ�ToolStripMenuItem,
            this.ˮϵ�ۺ�ToolStripMenuItem,
            this.ֲ���ۺ�ToolStripMenuItem});
          this.menuGen.Name = "menuGen";
          this.menuGen.Size = new System.Drawing.Size(68, 21);
          this.menuGen.Text = "�ۺϲ���";
          // 
          // menuGenPara
          // 
          this.menuGenPara.Name = "menuGenPara";
          this.menuGenPara.Size = new System.Drawing.Size(136, 22);
          this.menuGenPara.Text = "�ۺϲ���";
          this.menuGenPara.Click += new System.EventHandler(this.GenParaToolStripMenuItem_Click);
          // 
          // toolStripMenuItem2
          // 
          this.toolStripMenuItem2.Name = "toolStripMenuItem2";
          this.toolStripMenuItem2.Size = new System.Drawing.Size(133, 6);
          // 
          // ��·�ۺ�ToolStripMenuItem
          // 
          this.��·�ۺ�ToolStripMenuItem.Name = "��·�ۺ�ToolStripMenuItem";
          this.��·�ۺ�ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
          this.��·�ۺ�ToolStripMenuItem.Text = "��·�ۺ�";
          // 
          // �������ۺ�ToolStripMenuItem
          // 
          this.�������ۺ�ToolStripMenuItem.Name = "�������ۺ�ToolStripMenuItem";
          this.�������ۺ�ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
          this.�������ۺ�ToolStripMenuItem.Text = "�������ۺ�";
          // 
          // ˮϵ�ۺ�ToolStripMenuItem
          // 
          this.ˮϵ�ۺ�ToolStripMenuItem.Name = "ˮϵ�ۺ�ToolStripMenuItem";
          this.ˮϵ�ۺ�ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
          this.ˮϵ�ۺ�ToolStripMenuItem.Text = "ˮϵ�ۺ�";
          // 
          // ֲ���ۺ�ToolStripMenuItem
          // 
          this.ֲ���ۺ�ToolStripMenuItem.Name = "ֲ���ۺ�ToolStripMenuItem";
          this.ֲ���ۺ�ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
          this.ֲ���ۺ�ToolStripMenuItem.Text = "ֲ���ۺ�";
          // 
          // menuData
          // 
          this.menuData.Name = "menuData";
          this.menuData.Size = new System.Drawing.Size(68, 21);
          this.menuData.Text = "���ݼ��";
          // 
          // menuHelp
          // 
          this.menuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.����ToolStripMenuItem});
          this.menuHelp.Name = "menuHelp";
          this.menuHelp.Size = new System.Drawing.Size(44, 21);
          this.menuHelp.Text = "����";
          // 
          // ����ToolStripMenuItem
          // 
          this.����ToolStripMenuItem.Name = "����ToolStripMenuItem";
          this.����ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
          this.����ToolStripMenuItem.Text = "����";
          this.����ToolStripMenuItem.Click += new System.EventHandler(this.����ToolStripMenuItem_Click);
          // 
          // outlookBar1
          // 
          this.outlookBar1.ButtonHeight = 25;
          this.outlookBar1.Dock = System.Windows.Forms.DockStyle.Left;
          this.outlookBar1.Location = new System.Drawing.Point(29, 81);
          this.outlookBar1.Name = "outlookBar1";
          this.outlookBar1.SelectedBand = 0;
          this.outlookBar1.Size = new System.Drawing.Size(200, 460);
          this.outlookBar1.TabIndex = 11;
          // 
          // MainForm
          // 
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.ClientSize = new System.Drawing.Size(859, 541);
          this.Controls.Add(this.axLicenseControl1);
          this.Controls.Add(this.axMapControl1);
          this.Controls.Add(this.editToolBar);
          this.Controls.Add(this.axToolbarControl1);
          this.Controls.Add(this.statusStrip1);
          this.Controls.Add(this.outlookBar1);
          this.Controls.Add(this.axTOCControl1);
          this.Controls.Add(this.axToolbarControl2);
          this.Controls.Add(this.axToolbarControl3);
          this.Controls.Add(this.menuStrip1);
          this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
          this.Name = "MainForm";
          this.Text = "���ݳ��е�ͼ�ۺ�����ϵͳ(GZ-UMGS)";
          this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
          this.Load += new System.EventHandler(this.MainForm_Load);
          ((System.ComponentModel.ISupportInitialize)(this.axMapControl1)).EndInit();
          ((System.ComponentModel.ISupportInitialize)(this.axToolbarControl1)).EndInit();
          ((System.ComponentModel.ISupportInitialize)(this.axTOCControl1)).EndInit();
          this.statusStrip1.ResumeLayout(false);
          this.statusStrip1.PerformLayout();
          ((System.ComponentModel.ISupportInitialize)(this.axToolbarControl2)).EndInit();
          ((System.ComponentModel.ISupportInitialize)(this.editToolBar)).EndInit();
          ((System.ComponentModel.ISupportInitialize)(this.axLicenseControl1)).EndInit();
          ((System.ComponentModel.ISupportInitialize)(this.axToolbarControl3)).EndInit();
          this.menuStrip1.ResumeLayout(false);
          this.menuStrip1.PerformLayout();
          this.ResumeLayout(false);
          this.PerformLayout();

        }

        #endregion

        private ESRI.ArcGIS.Controls.AxMapControl axMapControl1;
        private ESRI.ArcGIS.Controls.AxToolbarControl axToolbarControl1;
        private ESRI.ArcGIS.Controls.AxTOCControl axTOCControl1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusBarXY;
        private ESRI.ArcGIS.Controls.AxToolbarControl axToolbarControl2;
        private ESRI.ArcGIS.Controls.AxToolbarControl editToolBar;
        private ESRI.ArcGIS.Controls.AxLicenseControl axLicenseControl1;
        private ESRI.ArcGIS.Controls.AxToolbarControl axToolbarControl3;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem menuDoc;
        private System.Windows.Forms.ToolStripMenuItem menuNewDoc;
        private System.Windows.Forms.ToolStripMenuItem menuSaveDoc;
        private System.Windows.Forms.ToolStripMenuItem menuCloseDoc;
        private System.Windows.Forms.ToolStripMenuItem menuView;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem menuGen;
        private System.Windows.Forms.ToolStripMenuItem menuData;
        private System.Windows.Forms.ToolStripMenuItem menuOpenDoc;
        private System.Windows.Forms.ToolStripMenuItem menuGenPara;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem ��·�ۺ�ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem �������ۺ�ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ˮϵ�ۺ�ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ֲ���ۺ�ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuHelp;
        private System.Windows.Forms.ToolStripMenuItem ����ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuViewLayer;
        private System.Windows.Forms.ToolStripMenuItem menuViewShowGen;
        private System.Windows.Forms.ToolStripMenuItem menuViewShowOrg;
        private System.Windows.Forms.ToolStripMenuItem Ҫ����ʾToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ������ʾToolStripMenuItem;
        private OutlookBar.OutlookBar outlookBar1;
        private System.Windows.Forms.ToolStripMenuItem menuHighBuilding;
        private System.Windows.Forms.ToolStripMenuItem menuHightRoad;
        private System.Windows.Forms.ToolStripMenuItem menuHighWater;
        private System.Windows.Forms.ToolStripMenuItem MenuHighPlant;
        private System.Windows.Forms.ToolStripMenuItem menuViewBuildingGroup;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem menuScaleOrg;
        private System.Windows.Forms.ToolStripMenuItem menuScaleGen;
        private System.Windows.Forms.ToolStripMenuItem menuViewBuildingStruct;
        private System.Windows.Forms.ToolStripMenuItem menuViewRoadRank;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel statusbarScale;
        private System.Windows.Forms.ToolStripMenuItem miWorkspaceInfo;
    }
}

