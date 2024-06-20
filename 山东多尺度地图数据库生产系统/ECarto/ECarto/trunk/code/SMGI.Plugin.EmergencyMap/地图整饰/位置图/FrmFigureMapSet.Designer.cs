namespace SMGI.Plugin.EmergencyMap
{
    partial class FrmFigureMapSet
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmFigureMapSet));
            this.btnDownLeft = new System.Windows.Forms.Button();
            this.btnTopLeft = new System.Windows.Forms.Button();
            this.btnTopRight = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.gbPosition = new System.Windows.Forms.GroupBox();
            this.btnDownRight = new System.Windows.Forms.Button();
            this.btOk = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.txtsize = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.gbPosition.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnDownLeft
            // 
            this.btnDownLeft.Location = new System.Drawing.Point(2, 158);
            this.btnDownLeft.Margin = new System.Windows.Forms.Padding(2);
            this.btnDownLeft.Name = "btnDownLeft";
            this.btnDownLeft.Size = new System.Drawing.Size(27, 28);
            this.btnDownLeft.TabIndex = 31;
            this.btnDownLeft.UseVisualStyleBackColor = true;
            // 
            // btnTopLeft
            // 
            this.btnTopLeft.Location = new System.Drawing.Point(3, 7);
            this.btnTopLeft.Margin = new System.Windows.Forms.Padding(2);
            this.btnTopLeft.Name = "btnTopLeft";
            this.btnTopLeft.Size = new System.Drawing.Size(27, 28);
            this.btnTopLeft.TabIndex = 28;
            this.btnTopLeft.UseVisualStyleBackColor = true;
            // 
            // btnTopRight
            // 
            this.btnTopRight.BackColor = System.Drawing.Color.Transparent;
            this.btnTopRight.Location = new System.Drawing.Point(187, 7);
            this.btnTopRight.Margin = new System.Windows.Forms.Padding(2);
            this.btnTopRight.Name = "btnTopRight";
            this.btnTopRight.Size = new System.Drawing.Size(28, 28);
            this.btnTopRight.TabIndex = 25;
            this.btnTopRight.UseVisualStyleBackColor = false;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(16, 17);
            this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(107, 12);
            this.label13.TabIndex = 23;
            this.label13.Text = "位置图尺寸（mm)：";
            // 
            // gbPosition
            // 
            this.gbPosition.Controls.Add(this.btnDownRight);
            this.gbPosition.Controls.Add(this.btnTopRight);
            this.gbPosition.Controls.Add(this.btnTopLeft);
            this.gbPosition.Controls.Add(this.btnDownLeft);
            this.gbPosition.Location = new System.Drawing.Point(114, 40);
            this.gbPosition.Name = "gbPosition";
            this.gbPosition.Size = new System.Drawing.Size(217, 188);
            this.gbPosition.TabIndex = 34;
            this.gbPosition.TabStop = false;
            // 
            // btnDownRight
            // 
            this.btnDownRight.Location = new System.Drawing.Point(188, 158);
            this.btnDownRight.Margin = new System.Windows.Forms.Padding(2);
            this.btnDownRight.Name = "btnDownRight";
            this.btnDownRight.Size = new System.Drawing.Size(27, 28);
            this.btnDownRight.TabIndex = 39;
            this.btnDownRight.UseVisualStyleBackColor = true;
            // 
            // btOk
            // 
            this.btOk.Location = new System.Drawing.Point(137, 247);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(75, 32);
            this.btOk.TabIndex = 35;
            this.btOk.Text = "确定";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(256, 247);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 32);
            this.btCancel.TabIndex = 36;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // txtsize
            // 
            this.txtsize.Location = new System.Drawing.Point(116, 13);
            this.txtsize.Name = "txtsize";
            this.txtsize.Size = new System.Drawing.Size(215, 21);
            this.txtsize.TabIndex = 37;
            this.txtsize.Text = "50";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 47);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 24);
            this.label1.TabIndex = 38;
            this.label1.Text = "位置图相对\r\n内图廓方位：";
            // 
            // FrmFigureMapSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(360, 284);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtsize);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.gbPosition);
            this.Controls.Add(this.label13);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FrmFigureMapSet";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "位置图设置";
            this.Load += new System.EventHandler(this.FrmFigureMapSet_Load);
            this.gbPosition.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnDownLeft;
        private System.Windows.Forms.Button btnTopLeft;
        private System.Windows.Forms.Button btnTopRight;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.GroupBox gbPosition;
        private System.Windows.Forms.Button btnDownRight;
        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.TextBox txtsize;
        private System.Windows.Forms.Label label1;
    }
}