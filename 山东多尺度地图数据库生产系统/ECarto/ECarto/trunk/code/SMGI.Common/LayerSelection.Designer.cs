namespace SMGI.Common{
    partial class LayerSelection
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
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.groupBox_target = new System.Windows.Forms.GroupBox();
            this.linkLabel_switch = new System.Windows.Forms.LinkLabel();
            this.linkLabel_No = new System.Windows.Forms.LinkLabel();
            this.linkLabel_all = new System.Windows.Forms.LinkLabel();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.checkBox_visible = new System.Windows.Forms.CheckBox();
            this.groupBox_constrained = new System.Windows.Forms.GroupBox();
            this.treeView2 = new System.Windows.Forms.TreeView();
            this.checkBox_separately = new System.Windows.Forms.CheckBox();
            this.checkBox_constraint = new System.Windows.Forms.CheckBox();
            this.checkBox_showfeature = new System.Windows.Forms.CheckBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.groupBox_target.SuspendLayout();
            this.groupBox_constrained.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // groupBox_target
            // 
            this.groupBox_target.Controls.Add(this.linkLabel_switch);
            this.groupBox_target.Controls.Add(this.linkLabel_No);
            this.groupBox_target.Controls.Add(this.linkLabel_all);
            this.groupBox_target.Controls.Add(this.treeView1);
            this.groupBox_target.Controls.Add(this.checkBox_visible);
            this.groupBox_target.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.groupBox_target.Location = new System.Drawing.Point(12, 40);
            this.groupBox_target.Name = "groupBox_target";
            this.groupBox_target.Size = new System.Drawing.Size(310, 468);
            this.groupBox_target.TabIndex = 1;
            this.groupBox_target.TabStop = false;
            this.groupBox_target.Text = "目标图层";
            // 
            // linkLabel_switch
            // 
            this.linkLabel_switch.AutoSize = true;
            this.linkLabel_switch.LinkColor = System.Drawing.Color.Black;
            this.linkLabel_switch.Location = new System.Drawing.Point(270, 21);
            this.linkLabel_switch.Name = "linkLabel_switch";
            this.linkLabel_switch.Size = new System.Drawing.Size(29, 12);
            this.linkLabel_switch.TabIndex = 5;
            this.linkLabel_switch.TabStop = true;
            this.linkLabel_switch.Text = "反选";
            this.linkLabel_switch.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_switch_LinkClicked);
            // 
            // linkLabel_No
            // 
            this.linkLabel_No.AutoSize = true;
            this.linkLabel_No.LinkColor = System.Drawing.Color.Black;
            this.linkLabel_No.Location = new System.Drawing.Point(223, 21);
            this.linkLabel_No.Name = "linkLabel_No";
            this.linkLabel_No.Size = new System.Drawing.Size(41, 12);
            this.linkLabel_No.TabIndex = 5;
            this.linkLabel_No.TabStop = true;
            this.linkLabel_No.Text = "全不选";
            this.linkLabel_No.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_No_LinkClicked);
            // 
            // linkLabel_all
            // 
            this.linkLabel_all.AutoSize = true;
            this.linkLabel_all.LinkColor = System.Drawing.Color.Black;
            this.linkLabel_all.Location = new System.Drawing.Point(188, 21);
            this.linkLabel_all.Name = "linkLabel_all";
            this.linkLabel_all.Size = new System.Drawing.Size(29, 12);
            this.linkLabel_all.TabIndex = 5;
            this.linkLabel_all.TabStop = true;
            this.linkLabel_all.Text = "全选";
            this.linkLabel_all.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_all_LinkClicked);
            // 
            // treeView1
            // 
            this.treeView1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.treeView1.FullRowSelect = true;
            this.treeView1.Indent = 10;
            this.treeView1.ItemHeight = 25;
            this.treeView1.LineColor = System.Drawing.Color.Maroon;
            this.treeView1.Location = new System.Drawing.Point(7, 42);
            this.treeView1.Name = "treeView1";
            this.treeView1.ShowNodeToolTips = true;
            this.treeView1.Size = new System.Drawing.Size(297, 416);
            this.treeView1.TabIndex = 1;
            // 
            // checkBox_visible
            // 
            this.checkBox_visible.AutoSize = true;
            this.checkBox_visible.Location = new System.Drawing.Point(7, 20);
            this.checkBox_visible.Name = "checkBox_visible";
            this.checkBox_visible.Size = new System.Drawing.Size(108, 16);
            this.checkBox_visible.TabIndex = 4;
            this.checkBox_visible.Text = "关联图层可见性";
            this.checkBox_visible.UseVisualStyleBackColor = true;
            // 
            // groupBox_constrained
            // 
            this.groupBox_constrained.Controls.Add(this.treeView2);
            this.groupBox_constrained.Controls.Add(this.checkBox_separately);
            this.groupBox_constrained.Enabled = false;
            this.groupBox_constrained.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.groupBox_constrained.Location = new System.Drawing.Point(342, 40);
            this.groupBox_constrained.Name = "groupBox_constrained";
            this.groupBox_constrained.Size = new System.Drawing.Size(310, 468);
            this.groupBox_constrained.TabIndex = 2;
            this.groupBox_constrained.TabStop = false;
            this.groupBox_constrained.Text = "约束图层";
            // 
            // treeView2
            // 
            this.treeView2.CheckBoxes = true;
            this.treeView2.ItemHeight = 25;
            this.treeView2.Location = new System.Drawing.Point(7, 42);
            this.treeView2.Name = "treeView2";
            this.treeView2.ShowNodeToolTips = true;
            this.treeView2.Size = new System.Drawing.Size(297, 416);
            this.treeView2.TabIndex = 2;
            // 
            // checkBox_separately
            // 
            this.checkBox_separately.AutoSize = true;
            this.checkBox_separately.Location = new System.Drawing.Point(7, 20);
            this.checkBox_separately.Name = "checkBox_separately";
            this.checkBox_separately.Size = new System.Drawing.Size(180, 16);
            this.checkBox_separately.TabIndex = 1;
            this.checkBox_separately.Text = "为目标图层分别定制约束图层";
            this.checkBox_separately.UseVisualStyleBackColor = true;
            this.checkBox_separately.CheckedChanged += new System.EventHandler(this.checkBox_separately_CheckedChanged);
            // 
            // checkBox_constraint
            // 
            this.checkBox_constraint.AutoSize = true;
            this.checkBox_constraint.Location = new System.Drawing.Point(342, 12);
            this.checkBox_constraint.Name = "checkBox_constraint";
            this.checkBox_constraint.Size = new System.Drawing.Size(96, 16);
            this.checkBox_constraint.TabIndex = 3;
            this.checkBox_constraint.Text = "启用约束图层";
            this.checkBox_constraint.UseVisualStyleBackColor = true;
            this.checkBox_constraint.CheckedChanged += new System.EventHandler(this.checkBox_constraint_CheckedChanged);
            // 
            // checkBox_showfeature
            // 
            this.checkBox_showfeature.AutoSize = true;
            this.checkBox_showfeature.Location = new System.Drawing.Point(19, 12);
            this.checkBox_showfeature.Name = "checkBox_showfeature";
            this.checkBox_showfeature.Size = new System.Drawing.Size(108, 16);
            this.checkBox_showfeature.TabIndex = 2;
            this.checkBox_showfeature.Text = "显示图层内地物";
            this.checkBox_showfeature.UseVisualStyleBackColor = true;
            this.checkBox_showfeature.CheckedChanged += new System.EventHandler(this.checkBox_showfeature_CheckedChanged);
            // 
            // button_OK
            // 
            this.button_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_OK.Location = new System.Drawing.Point(496, 519);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确认";
            this.button_OK.UseVisualStyleBackColor = true;
            // 
            // button_Cancel
            // 
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(577, 519);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 28);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            // 
            // LayerSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(668, 559);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.checkBox_showfeature);
            this.Controls.Add(this.checkBox_constraint);
            this.Controls.Add(this.groupBox_constrained);
            this.Controls.Add(this.groupBox_target);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LayerSelection";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "图层选择";
            this.Load += new System.EventHandler(this.LayerSelection_Load);
            this.groupBox_target.ResumeLayout(false);
            this.groupBox_target.PerformLayout();
            this.groupBox_constrained.ResumeLayout(false);
            this.groupBox_constrained.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.GroupBox groupBox_target;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.CheckBox checkBox_showfeature;
        private System.Windows.Forms.GroupBox groupBox_constrained;
        private System.Windows.Forms.CheckBox checkBox_constraint;
        private System.Windows.Forms.CheckBox checkBox_visible;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.CheckBox checkBox_separately;
        private System.Windows.Forms.TreeView treeView2;
        private System.Windows.Forms.LinkLabel linkLabel_switch;
        private System.Windows.Forms.LinkLabel linkLabel_No;
        private System.Windows.Forms.LinkLabel linkLabel_all;   
    }
}