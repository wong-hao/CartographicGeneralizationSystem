namespace SMGI.Plugin.EmergencyMap.DataSource
{
    partial class DEMData
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DEMData));
            this.gbDEM = new System.Windows.Forms.GroupBox();
            this.demLayer = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.demDb = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.demIP = new System.Windows.Forms.TextBox();
            this.demUser = new System.Windows.Forms.TextBox();
            this.demPassword = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.btServerDEM = new DevExpress.XtraEditors.SimpleButton();
            this.ribbonImageCollection = new DevExpress.Utils.ImageCollection(this.components);
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.gbDEM.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonImageCollection)).BeginInit();
            this.SuspendLayout();
            // 
            // gbDEM
            // 
            this.gbDEM.Controls.Add(this.demLayer);
            this.gbDEM.Controls.Add(this.label17);
            this.gbDEM.Controls.Add(this.demDb);
            this.gbDEM.Controls.Add(this.label9);
            this.gbDEM.Controls.Add(this.demIP);
            this.gbDEM.Controls.Add(this.demUser);
            this.gbDEM.Controls.Add(this.demPassword);
            this.gbDEM.Controls.Add(this.label14);
            this.gbDEM.Controls.Add(this.label15);
            this.gbDEM.Controls.Add(this.label16);
            this.gbDEM.Location = new System.Drawing.Point(6, 4);
            this.gbDEM.Name = "gbDEM";
            this.gbDEM.Size = new System.Drawing.Size(573, 103);
            this.gbDEM.TabIndex = 4;
            this.gbDEM.TabStop = false;
            this.gbDEM.Text = "DEM数据库";
            // 
            // demLayer
            // 
            this.demLayer.Location = new System.Drawing.Point(289, 66);
            this.demLayer.Name = "demLayer";
            this.demLayer.Size = new System.Drawing.Size(85, 21);
            this.demLayer.TabIndex = 64;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(234, 72);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(59, 12);
            this.label17.TabIndex = 63;
            this.label17.Text = "DEM图层：";
            // 
            // demDb
            // 
            this.demDb.Location = new System.Drawing.Point(56, 69);
            this.demDb.Name = "demDb";
            this.demDb.Size = new System.Drawing.Size(110, 21);
            this.demDb.TabIndex = 62;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(11, 72);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 12);
            this.label9.TabIndex = 61;
            this.label9.Text = "数据库：";
            // 
            // demIP
            // 
            this.demIP.Location = new System.Drawing.Point(62, 28);
            this.demIP.Name = "demIP";
            this.demIP.Size = new System.Drawing.Size(113, 21);
            this.demIP.TabIndex = 57;
            // 
            // demUser
            // 
            this.demUser.Location = new System.Drawing.Point(289, 28);
            this.demUser.Name = "demUser";
            this.demUser.Size = new System.Drawing.Size(85, 21);
            this.demUser.TabIndex = 58;
            // 
            // demPassword
            // 
            this.demPassword.Location = new System.Drawing.Point(463, 28);
            this.demPassword.Name = "demPassword";
            this.demPassword.Size = new System.Drawing.Size(85, 21);
            this.demPassword.TabIndex = 60;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(429, 32);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(41, 12);
            this.label14.TabIndex = 59;
            this.label14.Text = "密码：";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(240, 31);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(53, 12);
            this.label15.TabIndex = 56;
            this.label15.Text = "用户名：";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(9, 31);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(53, 12);
            this.label16.TabIndex = 55;
            this.label16.Text = "IP地址：";
            // 
            // btServerDEM
            // 
            this.btServerDEM.ImageIndex = 8;
            this.btServerDEM.ImageList = this.ribbonImageCollection;
            this.btServerDEM.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleLeft;
            this.btServerDEM.Location = new System.Drawing.Point(6, 113);
            this.btServerDEM.Name = "btServerDEM";
            this.btServerDEM.Size = new System.Drawing.Size(88, 33);
            this.btServerDEM.TabIndex = 52;
            this.btServerDEM.Text = "连接测试";
            this.btServerDEM.Click += new System.EventHandler(this.btnValidServer);
            // 
            // ribbonImageCollection
            // 
            this.ribbonImageCollection.ImageStream = ((DevExpress.Utils.ImageCollectionStreamer)(resources.GetObject("ribbonImageCollection.ImageStream")));
            this.ribbonImageCollection.Images.SetKeyName(0, "Ribbon_New_16x16.png");
            this.ribbonImageCollection.Images.SetKeyName(1, "Ribbon_Close_16x16.png");
            this.ribbonImageCollection.Images.SetKeyName(2, "Ribbon_Find_16x16.png");
            this.ribbonImageCollection.Images.SetKeyName(3, "Ribbon_Save_16x16.png");
            this.ribbonImageCollection.Images.SetKeyName(4, "Ribbon_Exit_16x16.png");
            this.ribbonImageCollection.Images.SetKeyName(5, "back.gif");
            this.ribbonImageCollection.Images.SetKeyName(6, "favicon.gif");
            this.ribbonImageCollection.Images.SetKeyName(7, "EditingCreateFeaturesWindowShow16.png");
            this.ribbonImageCollection.Images.SetKeyName(8, "服务器初始化.png");
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(446, 113);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(57, 36);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(520, 113);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(59, 36);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // DEMData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 149);
            this.Controls.Add(this.btServerDEM);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.gbDEM);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DEMData";
            this.Text = "DEM数据设置";
            this.Load += new System.EventHandler(this.DEMData_Load);
            this.gbDEM.ResumeLayout(false);
            this.gbDEM.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonImageCollection)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbDEM;
        private DevExpress.XtraEditors.SimpleButton btServerDEM;
        private System.Windows.Forms.TextBox demLayer;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox demDb;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox demIP;
        private System.Windows.Forms.TextBox demUser;
        private System.Windows.Forms.TextBox demPassword;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Button btnOK;
        private DevExpress.Utils.ImageCollection ribbonImageCollection;
        private System.Windows.Forms.Button btnCancel;
    }
}