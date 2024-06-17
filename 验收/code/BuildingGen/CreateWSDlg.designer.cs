﻿namespace BuildingGen
{
    partial class CreateWSDlg
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateWSDlg));
            this.layerList = new System.Windows.Forms.ListView();
            this.dirImages = new System.Windows.Forms.ImageList(this.components);
            this.SourceTypeImages = new System.Windows.Forms.ImageList(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.LayerNameTextbox = new System.Windows.Forms.TextBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.dirTree = new System.Windows.Forms.TreeView();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.upButton = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // layerList
            // 
            this.layerList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.layerList.HideSelection = false;
            this.layerList.LargeImageList = this.dirImages;
            this.layerList.Location = new System.Drawing.Point(12, 39);
            this.layerList.MultiSelect = false;
            this.layerList.Name = "layerList";
            this.layerList.Size = new System.Drawing.Size(578, 301);
            this.layerList.SmallImageList = this.dirImages;
            this.layerList.TabIndex = 1;
            this.layerList.UseCompatibleStateImageBehavior = false;
            this.layerList.View = System.Windows.Forms.View.List;
            this.layerList.DoubleClick += new System.EventHandler(this.layerList_DoubleClick);
            this.layerList.Click += new System.EventHandler(this.layerList_Click);
            // 
            // dirImages
            // 
            this.dirImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("dirImages.ImageStream")));
            this.dirImages.TransparentColor = System.Drawing.Color.Transparent;
            this.dirImages.Images.SetKeyName(0, "gdb16.bmp");
            this.dirImages.Images.SetKeyName(1, "ds16.bmp");
            this.dirImages.Images.SetKeyName(2, "Layer.bmp");
            // 
            // SourceTypeImages
            // 
            this.SourceTypeImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("SourceTypeImages.ImageStream")));
            this.SourceTypeImages.TransparentColor = System.Drawing.Color.Transparent;
            this.SourceTypeImages.Images.SetKeyName(0, "Points.bmp");
            this.SourceTypeImages.Images.SetKeyName(1, "Polygon.bmp");
            this.SourceTypeImages.Images.SetKeyName(2, "Polyline.bmp");
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 352);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "工作区名称：";
            // 
            // LayerNameTextbox
            // 
            this.LayerNameTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.LayerNameTextbox.Location = new System.Drawing.Point(83, 348);
            this.LayerNameTextbox.Name = "LayerNameTextbox";
            this.LayerNameTextbox.Size = new System.Drawing.Size(345, 21);
            this.LayerNameTextbox.TabIndex = 5;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(434, 346);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 9;
            this.okButton.Text = "创建";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(515, 346);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 10;
            this.cancelButton.Text = "取消";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.DropDownHeight = 1;
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.DropDownWidth = 1;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.IntegralHeight = false;
            this.comboBox1.Location = new System.Drawing.Point(12, 10);
            this.comboBox1.MaxDropDownItems = 1;
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(551, 20);
            this.comboBox1.TabIndex = 12;
            this.comboBox1.Click += new System.EventHandler(this.comboBox1_Click);
            // 
            // dirTree
            // 
            this.dirTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dirTree.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dirTree.HotTracking = true;
            this.dirTree.ImageIndex = 0;
            this.dirTree.ImageList = this.dirImages;
            this.dirTree.Location = new System.Drawing.Point(12, 36);
            this.dirTree.Name = "dirTree";
            this.dirTree.SelectedImageIndex = 0;
            this.dirTree.ShowLines = false;
            this.dirTree.ShowPlusMinus = false;
            this.dirTree.ShowRootLines = false;
            this.dirTree.Size = new System.Drawing.Size(551, 229);
            this.dirTree.TabIndex = 13;
            this.dirTree.TabStop = false;
            this.dirTree.Visible = false;
            this.dirTree.Leave += new System.EventHandler(this.dirTree_Leave);
            this.dirTree.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.dirTree_NodeMouseClick);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.upButton});
            this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.toolStrip1.Location = new System.Drawing.Point(569, 10);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(24, 23);
            this.toolStrip1.TabIndex = 14;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // upButton
            // 
            this.upButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.upButton.Image = ((System.Drawing.Image)(resources.GetObject("upButton.Image")));
            this.upButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.upButton.Name = "upButton";
            this.upButton.Size = new System.Drawing.Size(23, 20);
            this.upButton.Text = "向上";
            this.upButton.Click += new System.EventHandler(this.upButton_Click);
            // 
            // CreateWSDlg
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(602, 381);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.dirTree);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.LayerNameTextbox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.layerList);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateWSDlg";
            this.ShowIcon = false;
            this.Text = "新建工作区";
            this.Load += new System.EventHandler(this.AddDataDlg_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView layerList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox LayerNameTextbox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.ImageList SourceTypeImages;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ImageList dirImages;
        private System.Windows.Forms.TreeView dirTree;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton upButton;
    }
}