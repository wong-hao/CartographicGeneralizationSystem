namespace SMGI.Plugin.EmergencyMap
{
    partial class FrmFootBorder
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmFootBorder));
            this.inWidth = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btOK = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.txtInterval = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.listView1 = new System.Windows.Forms.ListView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.cmb_Ang = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lbMsg = new System.Windows.Forms.Label();
            this.grp_laceLib = new System.Windows.Forms.GroupBox();
            this.lbl_tips = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.chk_Filp = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtinLen = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.btn_del = new System.Windows.Forms.Button();
            this.btn_add = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.picCornerLace = new System.Windows.Forms.PictureBox();
            this.pnl_lace = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btn_Preview = new System.Windows.Forms.Button();
            this.btn_LastParas = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            this.grp_laceLib.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCornerLace)).BeginInit();
            this.pnl_lace.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // inWidth
            // 
            this.inWidth.Location = new System.Drawing.Point(123, 22);
            this.inWidth.Name = "inWidth";
            this.inWidth.Size = new System.Drawing.Size(114, 21);
            this.inWidth.TabIndex = 5;
            this.inWidth.Text = "11";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "花边线宽度(mm)：";
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(418, 399);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(50, 32);
            this.btOK.TabIndex = 6;
            this.btOK.Text = "确定";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(478, 399);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(45, 32);
            this.btCancel.TabIndex = 7;
            this.btCancel.Text = "关闭";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // txtInterval
            // 
            this.txtInterval.Location = new System.Drawing.Point(422, 22);
            this.txtInterval.Name = "txtInterval";
            this.txtInterval.Size = new System.Drawing.Size(90, 21);
            this.txtInterval.TabIndex = 9;
            this.txtInterval.Text = "2";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(279, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 12);
            this.label1.TabIndex = 8;
            this.label1.Text = "花边与外图廓间距(mm)：";
            // 
            // listView1
            // 
            this.listView1.Dock = System.Windows.Forms.DockStyle.Top;
            this.listView1.LargeImageList = this.imageList1;
            this.listView1.Location = new System.Drawing.Point(3, 17);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(491, 138);
            this.listView1.TabIndex = 10;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(52, 52);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(15, 87);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(53, 12);
            this.label10.TabIndex = 4;
            this.label10.Text = "花边符号";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(442, 87);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(65, 12);
            this.label11.TabIndex = 4;
            this.label11.Text = "花边角符号";
            // 
            // cmb_Ang
            // 
            this.cmb_Ang.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmb_Ang.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmb_Ang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_Ang.FormattingEnabled = true;
            this.cmb_Ang.Items.AddRange(new object[] {
            "0",
            "90",
            "180",
            "270"});
            this.cmb_Ang.Location = new System.Drawing.Point(253, 163);
            this.cmb_Ang.Name = "cmb_Ang";
            this.cmb_Ang.Size = new System.Drawing.Size(82, 20);
            this.cmb_Ang.TabIndex = 15;
            this.cmb_Ang.SelectedIndexChanged += new System.EventHandler(this.cmb_Ang_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.lbMsg);
            this.groupBox2.Controls.Add(this.grp_laceLib);
            this.groupBox2.Controls.Add(this.btn_del);
            this.groupBox2.Controls.Add(this.btn_add);
            this.groupBox2.Controls.Add(this.panel2);
            this.groupBox2.Controls.Add(this.pnl_lace);
            this.groupBox2.Controls.Add(this.txtInterval);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.inWidth);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(11, 9);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(526, 384);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "花边设置";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(148, 55);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(251, 12);
            this.label3.TabIndex = 22;
            this.label3.Text = "花边线宽度+花边与外图廓间距<=内外图廓间距";
            // 
            // lbMsg
            // 
            this.lbMsg.AutoSize = true;
            this.lbMsg.Location = new System.Drawing.Point(16, 54);
            this.lbMsg.Name = "lbMsg";
            this.lbMsg.Size = new System.Drawing.Size(89, 12);
            this.lbMsg.TabIndex = 21;
            this.lbMsg.Text = "内外图廓间距：";
            // 
            // grp_laceLib
            // 
            this.grp_laceLib.Controls.Add(this.label4);
            this.grp_laceLib.Controls.Add(this.lbl_tips);
            this.grp_laceLib.Controls.Add(this.listView1);
            this.grp_laceLib.Controls.Add(this.label9);
            this.grp_laceLib.Controls.Add(this.chk_Filp);
            this.grp_laceLib.Controls.Add(this.label8);
            this.grp_laceLib.Controls.Add(this.cmb_Ang);
            this.grp_laceLib.Controls.Add(this.txtinLen);
            this.grp_laceLib.Controls.Add(this.label13);
            this.grp_laceLib.Controls.Add(this.label12);
            this.grp_laceLib.Location = new System.Drawing.Point(16, 169);
            this.grp_laceLib.Name = "grp_laceLib";
            this.grp_laceLib.Size = new System.Drawing.Size(497, 211);
            this.grp_laceLib.TabIndex = 20;
            this.grp_laceLib.TabStop = false;
            this.grp_laceLib.Text = "符号设置";
            // 
            // lbl_tips
            // 
            this.lbl_tips.AutoSize = true;
            this.lbl_tips.BackColor = System.Drawing.Color.White;
            this.lbl_tips.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_tips.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lbl_tips.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.lbl_tips.Location = new System.Drawing.Point(166, 145);
            this.lbl_tips.Name = "lbl_tips";
            this.lbl_tips.Padding = new System.Windows.Forms.Padding(4);
            this.lbl_tips.Size = new System.Drawing.Size(297, 22);
            this.lbl_tips.TabIndex = 17;
            this.lbl_tips.Text = "如果为0或空，表示按符号在规则库中的长宽比例设置";
            this.lbl_tips.Visible = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(200, 167);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(47, 12);
            this.label9.TabIndex = 4;
            this.label9.Text = "旋转角:";
            // 
            // chk_Filp
            // 
            this.chk_Filp.AutoSize = true;
            this.chk_Filp.Location = new System.Drawing.Point(406, 165);
            this.chk_Filp.Name = "chk_Filp";
            this.chk_Filp.Size = new System.Drawing.Size(48, 16);
            this.chk_Filp.TabIndex = 16;
            this.chk_Filp.Text = "翻转";
            this.chk_Filp.UseVisualStyleBackColor = true;
            this.chk_Filp.CheckedChanged += new System.EventHandler(this.chk_Filp_CheckedChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(341, 167);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(29, 12);
            this.label8.TabIndex = 4;
            this.label8.Text = "(度)";
            // 
            // txtinLen
            // 
            this.txtinLen.Location = new System.Drawing.Point(106, 163);
            this.txtinLen.Name = "txtinLen";
            this.txtinLen.Size = new System.Drawing.Size(59, 21);
            this.txtinLen.TabIndex = 5;
            this.txtinLen.Text = "0";
            this.txtinLen.Visible = false;
            this.txtinLen.TextChanged += new System.EventHandler(this.txtinLen_TextChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label13.ForeColor = System.Drawing.Color.Brown;
            this.label13.Location = new System.Drawing.Point(164, 168);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(12, 12);
            this.label13.TabIndex = 4;
            this.label13.Text = "?";
            this.label13.Visible = false;
            this.label13.MouseEnter += new System.EventHandler(this.label13_MouseEnter);
            this.label13.MouseLeave += new System.EventHandler(this.label13_MouseLeave);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(21, 167);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(89, 12);
            this.label12.TabIndex = 4;
            this.label12.Text = "花边长度(mm)：";
            this.label12.Visible = false;
            // 
            // btn_del
            // 
            this.btn_del.Location = new System.Drawing.Point(196, 82);
            this.btn_del.Name = "btn_del";
            this.btn_del.Size = new System.Drawing.Size(39, 23);
            this.btn_del.TabIndex = 19;
            this.btn_del.Text = "删除";
            this.btn_del.UseVisualStyleBackColor = true;
            this.btn_del.Click += new System.EventHandler(this.button2_Click);
            // 
            // btn_add
            // 
            this.btn_add.Location = new System.Drawing.Point(157, 82);
            this.btn_add.Name = "btn_add";
            this.btn_add.Size = new System.Drawing.Size(39, 23);
            this.btn_add.TabIndex = 19;
            this.btn_add.Text = "添加";
            this.btn_add.UseVisualStyleBackColor = true;
            this.btn_add.Click += new System.EventHandler(this.button1_Click);
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.picCornerLace);
            this.panel2.Location = new System.Drawing.Point(442, 105);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(62, 62);
            this.panel2.TabIndex = 18;
            // 
            // picCornerLace
            // 
            this.picCornerLace.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picCornerLace.Location = new System.Drawing.Point(4, 4);
            this.picCornerLace.Name = "picCornerLace";
            this.picCornerLace.Size = new System.Drawing.Size(52, 52);
            this.picCornerLace.TabIndex = 0;
            this.picCornerLace.TabStop = false;
            // 
            // pnl_lace
            // 
            this.pnl_lace.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_lace.Controls.Add(this.panel1);
            this.pnl_lace.Location = new System.Drawing.Point(16, 105);
            this.pnl_lace.Name = "pnl_lace";
            this.pnl_lace.Size = new System.Drawing.Size(400, 62);
            this.pnl_lace.TabIndex = 17;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Highlight;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Location = new System.Drawing.Point(3, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(52, 62);
            this.panel1.TabIndex = 1;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.SystemColors.Control;
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox1.Location = new System.Drawing.Point(0, 5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(52, 52);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // btn_Preview
            // 
            this.btn_Preview.Location = new System.Drawing.Point(360, 399);
            this.btn_Preview.Name = "btn_Preview";
            this.btn_Preview.Size = new System.Drawing.Size(49, 32);
            this.btn_Preview.TabIndex = 6;
            this.btn_Preview.Text = "预览";
            this.btn_Preview.UseVisualStyleBackColor = true;
            this.btn_Preview.Click += new System.EventHandler(this.btn_Preview_Click);
            // 
            // btn_LastParas
            // 
            this.btn_LastParas.Image = ((System.Drawing.Image)(resources.GetObject("btn_LastParas.Image")));
            this.btn_LastParas.Location = new System.Drawing.Point(11, 401);
            this.btn_LastParas.Name = "btn_LastParas";
            this.btn_LastParas.Size = new System.Drawing.Size(31, 30);
            this.btn_LastParas.TabIndex = 17;
            this.btn_LastParas.UseVisualStyleBackColor = true;
            this.btn_LastParas.Click += new System.EventHandler(this.btn_LastParas_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(200, 189);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(245, 12);
            this.label4.TabIndex = 23;
            this.label4.Text = "注：旋转角和翻转是针对长宽一致的花边元素";
            // 
            // FrmFootBorder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 438);
            this.Controls.Add(this.btn_LastParas);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btn_Preview);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.btCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FrmFootBorder";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "花边设置";
            this.Load += new System.EventHandler(this.FrmFootBorder_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.grp_laceLib.ResumeLayout(false);
            this.grp_laceLib.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picCornerLace)).EndInit();
            this.pnl_lace.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox inWidth;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.TextBox txtInterval;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox cmb_Ang;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox chk_Filp;
        private System.Windows.Forms.Panel pnl_lace;
        private System.Windows.Forms.Button btn_del;
        private System.Windows.Forms.Button btn_add;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.GroupBox grp_laceLib;
        private System.Windows.Forms.PictureBox picCornerLace;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox txtinLen;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label lbl_tips;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btn_Preview;
        private System.Windows.Forms.Button btn_LastParas;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbMsg;
        private System.Windows.Forms.Label label4;
    }
}