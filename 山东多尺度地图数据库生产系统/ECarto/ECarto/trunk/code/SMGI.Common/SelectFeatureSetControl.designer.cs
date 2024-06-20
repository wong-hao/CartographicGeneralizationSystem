namespace SMGI.Common
{
    partial class SelectFeatureSetControl
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
            this.delfeature = new System.Windows.Forms.Button();
            this.listViewdata = new System.Windows.Forms.ListView();
            this.addfeature = new System.Windows.Forms.Button();
            this.OnCancel = new System.Windows.Forms.Button();
            this.OnOK = new System.Windows.Forms.Button();
            this.btClear = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // delfeature
            // 
            this.delfeature.Location = new System.Drawing.Point(75, 3);
            this.delfeature.Name = "delfeature";
            this.delfeature.Size = new System.Drawing.Size(58, 27);
            this.delfeature.TabIndex = 13;
            this.delfeature.Text = "删除";
            this.delfeature.UseVisualStyleBackColor = true;
            this.delfeature.Click += new System.EventHandler(this.delfeature_Click);
            // 
            // listViewdata
            // 
            this.listViewdata.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewdata.Font = new System.Drawing.Font("宋体", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listViewdata.GridLines = true;
            this.listViewdata.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewdata.HideSelection = false;
            this.listViewdata.Location = new System.Drawing.Point(12, 36);
            this.listViewdata.Name = "listViewdata";
            this.listViewdata.Size = new System.Drawing.Size(400, 212);
            this.listViewdata.TabIndex = 12;
            this.listViewdata.UseCompatibleStateImageBehavior = false;
            this.listViewdata.View = System.Windows.Forms.View.Details;
            // 
            // addfeature
            // 
            this.addfeature.Location = new System.Drawing.Point(12, 3);
            this.addfeature.Name = "addfeature";
            this.addfeature.Size = new System.Drawing.Size(58, 27);
            this.addfeature.TabIndex = 11;
            this.addfeature.Text = "添加";
            this.addfeature.UseVisualStyleBackColor = true;
            this.addfeature.Click += new System.EventHandler(this.addfeature_Click);
            // 
            // OnCancel
            // 
            this.OnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.OnCancel.Location = new System.Drawing.Point(354, 254);
            this.OnCancel.Name = "OnCancel";
            this.OnCancel.Size = new System.Drawing.Size(58, 27);
            this.OnCancel.TabIndex = 15;
            this.OnCancel.Text = "取消";
            this.OnCancel.UseVisualStyleBackColor = true;
            // 
            // OnOK
            // 
            this.OnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OnOK.Location = new System.Drawing.Point(284, 254);
            this.OnOK.Name = "OnOK";
            this.OnOK.Size = new System.Drawing.Size(58, 27);
            this.OnOK.TabIndex = 14;
            this.OnOK.Text = "确定";
            this.OnOK.UseVisualStyleBackColor = true;
            // 
            // btClear
            // 
            this.btClear.Location = new System.Drawing.Point(138, 3);
            this.btClear.Name = "btClear";
            this.btClear.Size = new System.Drawing.Size(58, 27);
            this.btClear.TabIndex = 16;
            this.btClear.Text = "清除";
            this.btClear.UseVisualStyleBackColor = true;
            this.btClear.Click += new System.EventHandler(this.btClear_Click);
            // 
            // SelectFeatureSetControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btClear);
            this.Controls.Add(this.OnCancel);
            this.Controls.Add(this.OnOK);
            this.Controls.Add(this.delfeature);
            this.Controls.Add(this.listViewdata);
            this.Controls.Add(this.addfeature);
            this.Name = "SelectFeatureSetControl";
            this.Size = new System.Drawing.Size(420, 291);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button delfeature;
        private System.Windows.Forms.ListView listViewdata;
        private System.Windows.Forms.Button addfeature;
        private System.Windows.Forms.Button OnCancel;
        private System.Windows.Forms.Button OnOK;
        private System.Windows.Forms.Button btClear;
    }
}