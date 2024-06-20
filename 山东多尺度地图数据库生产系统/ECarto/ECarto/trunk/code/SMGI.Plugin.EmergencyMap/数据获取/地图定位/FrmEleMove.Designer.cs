namespace SMGI.Plugin.EmergencyMap
{
    partial class FrmEleMove
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmEleMove));
            this.btUp = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.btDown = new System.Windows.Forms.Button();
            this.btLeft = new System.Windows.Forms.Button();
            this.btRight = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtStep = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btOK = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btUp
            // 
            this.btUp.ImageIndex = 3;
            this.btUp.ImageList = this.imageList1;
            this.btUp.Location = new System.Drawing.Point(125, 15);
            this.btUp.Name = "btUp";
            this.btUp.Size = new System.Drawing.Size(38, 44);
            this.btUp.TabIndex = 0;
            this.btUp.UseVisualStyleBackColor = true;
            this.btUp.Click += new System.EventHandler(this.btUp_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "GenericBlueDownArrowLongTail32.png");
            this.imageList1.Images.SetKeyName(1, "GenericBlueLeftArrowLongTail32.png");
            this.imageList1.Images.SetKeyName(2, "GenericBlueRightArrowLongTail32.png");
            this.imageList1.Images.SetKeyName(3, "GenericBlueUpArrowLongTail32.png");
            // 
            // btDown
            // 
            this.btDown.ImageIndex = 0;
            this.btDown.ImageList = this.imageList1;
            this.btDown.Location = new System.Drawing.Point(125, 143);
            this.btDown.Name = "btDown";
            this.btDown.Size = new System.Drawing.Size(38, 44);
            this.btDown.TabIndex = 1;
            this.btDown.UseVisualStyleBackColor = true;
            this.btDown.Click += new System.EventHandler(this.btDown_Click);
            // 
            // btLeft
            // 
            this.btLeft.ImageIndex = 1;
            this.btLeft.ImageList = this.imageList1;
            this.btLeft.Location = new System.Drawing.Point(36, 87);
            this.btLeft.Name = "btLeft";
            this.btLeft.Size = new System.Drawing.Size(51, 35);
            this.btLeft.TabIndex = 2;
            this.btLeft.UseVisualStyleBackColor = true;
            this.btLeft.Click += new System.EventHandler(this.btLeft_Click);
            // 
            // btRight
            // 
            this.btRight.ImageIndex = 2;
            this.btRight.ImageList = this.imageList1;
            this.btRight.Location = new System.Drawing.Point(204, 87);
            this.btRight.Name = "btRight";
            this.btRight.Size = new System.Drawing.Size(48, 35);
            this.btRight.TabIndex = 3;
            this.btRight.UseVisualStyleBackColor = true;
            this.btRight.Click += new System.EventHandler(this.btRight_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btUp);
            this.groupBox1.Controls.Add(this.btRight);
            this.groupBox1.Controls.Add(this.btDown);
            this.groupBox1.Controls.Add(this.btLeft);
            this.groupBox1.Location = new System.Drawing.Point(5, 55);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(291, 194);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "移动步长:";
            // 
            // txtStep
            // 
            this.txtStep.Location = new System.Drawing.Point(73, 17);
            this.txtStep.Name = "txtStep";
            this.txtStep.Size = new System.Drawing.Size(161, 21);
            this.txtStep.TabIndex = 6;
            this.txtStep.Text = "1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(256, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "毫米";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.txtStep);
            this.groupBox2.Location = new System.Drawing.Point(5, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(291, 50);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(177, 259);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(52, 28);
            this.btOK.TabIndex = 9;
            this.btOK.Text = "确定";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(238, 259);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(52, 28);
            this.btCancel.TabIndex = 10;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // FrmEleMove
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(304, 295);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FrmEleMove";
            this.Text = "出图纸张移动设置";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmEleMove_FormClosed);
            this.Load += new System.EventHandler(this.FrmEleMove_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btUp;
        private System.Windows.Forms.Button btDown;
        private System.Windows.Forms.Button btLeft;
        private System.Windows.Forms.Button btRight;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtStep;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Button btCancel;
    }
}