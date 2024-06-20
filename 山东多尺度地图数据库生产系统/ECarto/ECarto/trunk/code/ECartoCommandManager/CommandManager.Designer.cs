using System;

namespace ECartoCommandManager
{
    partial class CommandManager
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.treeViewCommand = new System.Windows.Forms.TreeView();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxClassName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxCaption = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxToolTip = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxHotKey = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxQOrder = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxImage = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.buttonOpenXML = new System.Windows.Forms.Button();
            this.textBoxXMLpath = new System.Windows.Forms.TextBox();
            this.button_up = new System.Windows.Forms.Button();
            this.button_down = new System.Windows.Forms.Button();
            this.button_SetHotKey = new System.Windows.Forms.Button();
            this.textBoxHotKey2 = new System.Windows.Forms.TextBox();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // treeViewCommand
            // 
            this.treeViewCommand.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treeViewCommand.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.treeViewCommand.Location = new System.Drawing.Point(22, 43);
            this.treeViewCommand.Name = "treeViewCommand";
            this.treeViewCommand.Size = new System.Drawing.Size(301, 514);
            this.treeViewCommand.TabIndex = 0;
            this.treeViewCommand.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewCommand_AfterSelect);
            this.treeViewCommand.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewCommand_NodeMouseClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 12F);
            this.label1.Location = new System.Drawing.Point(391, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "ClassName";
            // 
            // textBoxClassName
            // 
            this.textBoxClassName.Location = new System.Drawing.Point(477, 52);
            this.textBoxClassName.Name = "textBoxClassName";
            this.textBoxClassName.Size = new System.Drawing.Size(323, 21);
            this.textBoxClassName.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 12F);
            this.label2.Location = new System.Drawing.Point(396, 92);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "Caption";
            // 
            // textBoxCaption
            // 
            this.textBoxCaption.Enabled = false;
            this.textBoxCaption.Location = new System.Drawing.Point(477, 87);
            this.textBoxCaption.Name = "textBoxCaption";
            this.textBoxCaption.ReadOnly = true;
            this.textBoxCaption.Size = new System.Drawing.Size(323, 21);
            this.textBoxCaption.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 12F);
            this.label3.Location = new System.Drawing.Point(400, 133);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 16);
            this.label3.TabIndex = 5;
            this.label3.Text = "ToolTip";
            // 
            // textBoxToolTip
            // 
            this.textBoxToolTip.Location = new System.Drawing.Point(477, 128);
            this.textBoxToolTip.Multiline = true;
            this.textBoxToolTip.Name = "textBoxToolTip";
            this.textBoxToolTip.Size = new System.Drawing.Size(323, 54);
            this.textBoxToolTip.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 12F);
            this.label4.Location = new System.Drawing.Point(401, 197);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 16);
            this.label4.TabIndex = 7;
            this.label4.Text = "HotKey";
            // 
            // textBoxHotKey
            // 
            this.textBoxHotKey.Enabled = false;
            this.textBoxHotKey.Location = new System.Drawing.Point(477, 198);
            this.textBoxHotKey.Name = "textBoxHotKey";
            this.textBoxHotKey.ReadOnly = true;
            this.textBoxHotKey.Size = new System.Drawing.Size(120, 21);
            this.textBoxHotKey.TabIndex = 8;
            this.textBoxHotKey.Text = "Ctrl+Shift+A";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("宋体", 12F);
            this.label5.Location = new System.Drawing.Point(409, 269);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 16);
            this.label5.TabIndex = 11;
            this.label5.Text = "Image";
            // 
            // textBoxQOrder
            // 
            this.textBoxQOrder.Location = new System.Drawing.Point(477, 231);
            this.textBoxQOrder.Name = "textBoxQOrder";
            this.textBoxQOrder.Size = new System.Drawing.Size(100, 21);
            this.textBoxQOrder.TabIndex = 10;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("宋体", 12F);
            this.label6.Location = new System.Drawing.Point(404, 230);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 16);
            this.label6.TabIndex = 9;
            this.label6.Text = "QOrder";
            // 
            // textBoxImage
            // 
            this.textBoxImage.Location = new System.Drawing.Point(477, 270);
            this.textBoxImage.Name = "textBoxImage";
            this.textBoxImage.Size = new System.Drawing.Size(100, 21);
            this.textBoxImage.TabIndex = 13;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(627, 525);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(63, 32);
            this.button1.TabIndex = 12;
            this.button1.Text = "保存";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2,
            this.toolStripMenuItem3});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(101, 70);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(100, 22);
            this.toolStripMenuItem1.Text = "删除";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(100, 22);
            this.toolStripMenuItem2.Text = "修改";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(100, 22);
            this.toolStripMenuItem3.Text = "插入";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(497, 308);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(96, 96);
            this.pictureBox1.TabIndex = 14;
            this.pictureBox1.TabStop = false;
            // 
            // buttonOpenXML
            // 
            this.buttonOpenXML.Font = new System.Drawing.Font("宋体", 9F);
            this.buttonOpenXML.Location = new System.Drawing.Point(22, 13);
            this.buttonOpenXML.Name = "buttonOpenXML";
            this.buttonOpenXML.Size = new System.Drawing.Size(88, 23);
            this.buttonOpenXML.TabIndex = 15;
            this.buttonOpenXML.Text = "选择XML文件";
            this.buttonOpenXML.UseVisualStyleBackColor = true;
            this.buttonOpenXML.Click += new System.EventHandler(this.buttonOpenXML_Click);
            // 
            // textBoxXMLpath
            // 
            this.textBoxXMLpath.Font = new System.Drawing.Font("宋体", 9F);
            this.textBoxXMLpath.Location = new System.Drawing.Point(126, 14);
            this.textBoxXMLpath.Name = "textBoxXMLpath";
            this.textBoxXMLpath.Size = new System.Drawing.Size(610, 21);
            this.textBoxXMLpath.TabIndex = 16;
            // 
            // button_up
            // 
            this.button_up.Location = new System.Drawing.Point(329, 242);
            this.button_up.Name = "button_up";
            this.button_up.Size = new System.Drawing.Size(24, 40);
            this.button_up.TabIndex = 17;
            this.button_up.Text = "↑";
            this.button_up.UseVisualStyleBackColor = true;
            this.button_up.Click += new System.EventHandler(this.button_up_Click);
            // 
            // button_down
            // 
            this.button_down.Location = new System.Drawing.Point(329, 297);
            this.button_down.Name = "button_down";
            this.button_down.Size = new System.Drawing.Size(24, 40);
            this.button_down.TabIndex = 18;
            this.button_down.Text = "↓";
            this.button_down.UseVisualStyleBackColor = true;
            this.button_down.Click += new System.EventHandler(this.button_down_Click);
            // 
            // button_SetHotKey
            // 
            this.button_SetHotKey.Location = new System.Drawing.Point(612, 197);
            this.button_SetHotKey.Name = "button_SetHotKey";
            this.button_SetHotKey.Size = new System.Drawing.Size(53, 23);
            this.button_SetHotKey.TabIndex = 19;
            this.button_SetHotKey.Text = "←分配";
            this.button_SetHotKey.UseVisualStyleBackColor = true;
            // 
            // textBoxHotKey2
            // 
            this.textBoxHotKey2.Location = new System.Drawing.Point(680, 198);
            this.textBoxHotKey2.Name = "textBoxHotKey2";
            this.textBoxHotKey2.Size = new System.Drawing.Size(120, 21);
            this.textBoxHotKey2.TabIndex = 20;
            this.textBoxHotKey2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxHotKey2_KeyPress);
            this.textBoxHotKey2.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxHotKey2_KeyUp);
            // 
            // CommandManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(814, 578);
            this.Controls.Add(this.textBoxHotKey2);
            this.Controls.Add(this.button_SetHotKey);
            this.Controls.Add(this.button_down);
            this.Controls.Add(this.button_up);
            this.Controls.Add(this.textBoxXMLpath);
            this.Controls.Add(this.buttonOpenXML);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBoxImage);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBoxQOrder);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxHotKey);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxToolTip);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxCaption);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxClassName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.treeViewCommand);
            this.Font = new System.Drawing.Font("宋体", 9F);
            this.Name = "CommandManager";
            this.Text = "工具命令管理";
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        
        #endregion

        private System.Windows.Forms.TreeView treeViewCommand;
        private System.Windows.Forms.TextBox textBoxClassName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxCaption;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxToolTip;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxHotKey;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxQOrder;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.TextBox textBoxImage;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button buttonOpenXML;
        private System.Windows.Forms.TextBox textBoxXMLpath;
        private System.Windows.Forms.Button button_up;
        private System.Windows.Forms.Button button_down;
        private System.Windows.Forms.Button button_SetHotKey;
        private System.Windows.Forms.TextBox textBoxHotKey2;
    }
}

