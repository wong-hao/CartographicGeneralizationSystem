namespace SMGI.Common
{
    partial class SelectBySQLDialog
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
            this.panelLayer = new System.Windows.Forms.Panel();
            this.CBArea = new System.Windows.Forms.CheckBox();
            this.CBLine = new System.Windows.Forms.CheckBox();
            this.CBPoint = new System.Windows.Forms.CheckBox();
            this.cbLayers = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panelCondition = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.cbSelectionMethod = new System.Windows.Forms.ComboBox();
            this.btOnlyValue = new System.Windows.Forms.Button();
            this.lbValues = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button14 = new System.Windows.Forms.Button();
            this.button13 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.lbFields = new System.Windows.Forms.ListBox();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.btOK = new System.Windows.Forms.Button();
            this.btClear = new System.Windows.Forms.Button();
            this.panelSQL = new System.Windows.Forms.Panel();
            this.tbSQL = new System.Windows.Forms.TextBox();
            this.btApply = new System.Windows.Forms.Button();
            this.panelLayer.SuspendLayout();
            this.panelCondition.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.panelSQL.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelLayer
            // 
            this.panelLayer.Controls.Add(this.CBArea);
            this.panelLayer.Controls.Add(this.CBLine);
            this.panelLayer.Controls.Add(this.CBPoint);
            this.panelLayer.Controls.Add(this.cbLayers);
            this.panelLayer.Controls.Add(this.label1);
            this.panelLayer.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelLayer.Location = new System.Drawing.Point(0, 0);
            this.panelLayer.Margin = new System.Windows.Forms.Padding(2);
            this.panelLayer.Name = "panelLayer";
            this.panelLayer.Size = new System.Drawing.Size(379, 52);
            this.panelLayer.TabIndex = 35;
            // 
            // CBArea
            // 
            this.CBArea.AutoSize = true;
            this.CBArea.Checked = true;
            this.CBArea.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBArea.Location = new System.Drawing.Point(102, 30);
            this.CBArea.Name = "CBArea";
            this.CBArea.Size = new System.Drawing.Size(36, 16);
            this.CBArea.TabIndex = 38;
            this.CBArea.Text = "面";
            this.CBArea.UseVisualStyleBackColor = true;
            this.CBArea.CheckedChanged += new System.EventHandler(this.CBArea_CheckedChanged);
            // 
            // CBLine
            // 
            this.CBLine.AutoSize = true;
            this.CBLine.Checked = true;
            this.CBLine.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBLine.Location = new System.Drawing.Point(58, 30);
            this.CBLine.Name = "CBLine";
            this.CBLine.Size = new System.Drawing.Size(36, 16);
            this.CBLine.TabIndex = 37;
            this.CBLine.Text = "线";
            this.CBLine.UseVisualStyleBackColor = true;
            this.CBLine.CheckedChanged += new System.EventHandler(this.CBLine_CheckedChanged);
            // 
            // CBPoint
            // 
            this.CBPoint.AutoSize = true;
            this.CBPoint.Checked = true;
            this.CBPoint.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBPoint.Location = new System.Drawing.Point(16, 30);
            this.CBPoint.Name = "CBPoint";
            this.CBPoint.Size = new System.Drawing.Size(36, 16);
            this.CBPoint.TabIndex = 36;
            this.CBPoint.Text = "点";
            this.CBPoint.UseVisualStyleBackColor = true;
            this.CBPoint.CheckedChanged += new System.EventHandler(this.CBPoint_CheckedChanged);
            // 
            // cbLayers
            // 
            this.cbLayers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cbLayers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbLayers.DropDownWidth = 300;
            this.cbLayers.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.cbLayers.FormattingEnabled = true;
            this.cbLayers.Location = new System.Drawing.Point(144, 26);
            this.cbLayers.Name = "cbLayers";
            this.cbLayers.Size = new System.Drawing.Size(222, 27);
            this.cbLayers.TabIndex = 35;
            this.cbLayers.SelectedIndexChanged += new System.EventHandler(this.SelectedLayerIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(14, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 20);
            this.label1.TabIndex = 34;
            this.label1.Text = "图层选择:";
            // 
            // panelCondition
            // 
            this.panelCondition.Controls.Add(this.label3);
            this.panelCondition.Controls.Add(this.cbSelectionMethod);
            this.panelCondition.Controls.Add(this.btOnlyValue);
            this.panelCondition.Controls.Add(this.lbValues);
            this.panelCondition.Controls.Add(this.label2);
            this.panelCondition.Controls.Add(this.button14);
            this.panelCondition.Controls.Add(this.button13);
            this.panelCondition.Controls.Add(this.button12);
            this.panelCondition.Controls.Add(this.button11);
            this.panelCondition.Controls.Add(this.button10);
            this.panelCondition.Controls.Add(this.button9);
            this.panelCondition.Controls.Add(this.button8);
            this.panelCondition.Controls.Add(this.button7);
            this.panelCondition.Controls.Add(this.button6);
            this.panelCondition.Controls.Add(this.button5);
            this.panelCondition.Controls.Add(this.button4);
            this.panelCondition.Controls.Add(this.button3);
            this.panelCondition.Controls.Add(this.button2);
            this.panelCondition.Controls.Add(this.button1);
            this.panelCondition.Controls.Add(this.lbFields);
            this.panelCondition.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelCondition.Location = new System.Drawing.Point(0, 52);
            this.panelCondition.Margin = new System.Windows.Forms.Padding(2);
            this.panelCondition.Name = "panelCondition";
            this.panelCondition.Size = new System.Drawing.Size(379, 302);
            this.panelCondition.TabIndex = 36;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(15, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 20);
            this.label3.TabIndex = 50;
            this.label3.Text = "选择方法:";
            // 
            // cbSelectionMethod
            // 
            this.cbSelectionMethod.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cbSelectionMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSelectionMethod.DropDownWidth = 300;
            this.cbSelectionMethod.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.cbSelectionMethod.FormattingEnabled = true;
            this.cbSelectionMethod.Items.AddRange(new object[] {
            "创建新选择内容",
            "添加到当前选择内容",
            "从当前选择内容中移除",
            "从当前选择内容中选择"});
            this.cbSelectionMethod.Location = new System.Drawing.Point(144, 8);
            this.cbSelectionMethod.Name = "cbSelectionMethod";
            this.cbSelectionMethod.Size = new System.Drawing.Size(222, 27);
            this.cbSelectionMethod.TabIndex = 49;
            // 
            // btOnlyValue
            // 
            this.btOnlyValue.Enabled = false;
            this.btOnlyValue.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btOnlyValue.Location = new System.Drawing.Point(159, 253);
            this.btOnlyValue.Name = "btOnlyValue";
            this.btOnlyValue.Size = new System.Drawing.Size(139, 25);
            this.btOnlyValue.TabIndex = 48;
            this.btOnlyValue.Text = "获取唯一值";
            this.btOnlyValue.UseVisualStyleBackColor = true;
            this.btOnlyValue.Click += new System.EventHandler(this.OnlyValue_Click);
            // 
            // lbValues
            // 
            this.lbValues.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lbValues.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lbValues.ItemHeight = 17;
            this.lbValues.Location = new System.Drawing.Point(159, 140);
            this.lbValues.Name = "lbValues";
            this.lbValues.Size = new System.Drawing.Size(206, 89);
            this.lbValues.TabIndex = 47;
            this.lbValues.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lbValues_MouseDoubleClick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(11, 281);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 20);
            this.label2.TabIndex = 46;
            this.label2.Text = "SQL查询语句:";
            // 
            // button14
            // 
            this.button14.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button14.Location = new System.Drawing.Point(18, 255);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(40, 23);
            this.button14.TabIndex = 45;
            this.button14.Text = "Is";
            this.button14.UseVisualStyleBackColor = true;
            this.button14.Click += new System.EventHandler(this.bt_Click);
            // 
            // button13
            // 
            this.button13.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button13.Location = new System.Drawing.Point(112, 226);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(40, 23);
            this.button13.TabIndex = 44;
            this.button13.Text = "Not";
            this.button13.UseVisualStyleBackColor = true;
            this.button13.Click += new System.EventHandler(this.bt_Click);
            // 
            // button12
            // 
            this.button12.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button12.Location = new System.Drawing.Point(64, 226);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(40, 23);
            this.button12.TabIndex = 43;
            this.button12.Text = "()";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.bt_Click);
            // 
            // button11
            // 
            this.button11.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button11.Location = new System.Drawing.Point(39, 226);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(18, 23);
            this.button11.TabIndex = 42;
            this.button11.Text = "%";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.bt_Click);
            // 
            // button10
            // 
            this.button10.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button10.Location = new System.Drawing.Point(18, 226);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(18, 23);
            this.button10.TabIndex = 41;
            this.button10.Text = "_";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.bt_Click);
            // 
            // button9
            // 
            this.button9.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button9.Location = new System.Drawing.Point(112, 196);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(40, 23);
            this.button9.TabIndex = 40;
            this.button9.Text = "Or";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.bt_Click);
            // 
            // button8
            // 
            this.button8.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button8.Location = new System.Drawing.Point(64, 196);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(40, 23);
            this.button8.TabIndex = 39;
            this.button8.Text = "<=";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.bt_Click);
            // 
            // button7
            // 
            this.button7.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button7.Location = new System.Drawing.Point(18, 196);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(40, 23);
            this.button7.TabIndex = 38;
            this.button7.Text = "<";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.bt_Click);
            // 
            // button6
            // 
            this.button6.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button6.Location = new System.Drawing.Point(112, 166);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(40, 23);
            this.button6.TabIndex = 37;
            this.button6.Text = "And";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.bt_Click);
            // 
            // button5
            // 
            this.button5.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button5.Location = new System.Drawing.Point(64, 166);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(40, 23);
            this.button5.TabIndex = 36;
            this.button5.Text = ">=";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.bt_Click);
            // 
            // button4
            // 
            this.button4.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button4.Location = new System.Drawing.Point(18, 166);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(40, 23);
            this.button4.TabIndex = 35;
            this.button4.Text = ">";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.bt_Click);
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button3.Location = new System.Drawing.Point(112, 136);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(40, 23);
            this.button3.TabIndex = 34;
            this.button3.Text = "Like";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.bt_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button2.Location = new System.Drawing.Point(63, 136);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(40, 23);
            this.button2.TabIndex = 33;
            this.button2.Text = "<>";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.bt_Click);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button1.Location = new System.Drawing.Point(18, 136);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(40, 23);
            this.button1.TabIndex = 32;
            this.button1.Text = "=";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.bt_Click);
            // 
            // lbFields
            // 
            this.lbFields.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lbFields.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lbFields.FormattingEnabled = true;
            this.lbFields.ItemHeight = 17;
            this.lbFields.Location = new System.Drawing.Point(15, 38);
            this.lbFields.Name = "lbFields";
            this.lbFields.Size = new System.Drawing.Size(350, 72);
            this.lbFields.TabIndex = 31;
            this.lbFields.SelectedIndexChanged += new System.EventHandler(this.SelectedFieldChanged);
            this.lbFields.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lbFields_MouseDoubleClick);
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.btApply);
            this.panelButtons.Controls.Add(this.btCancel);
            this.panelButtons.Controls.Add(this.btOK);
            this.panelButtons.Controls.Add(this.btClear);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(0, 427);
            this.panelButtons.Margin = new System.Windows.Forms.Padding(2);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(379, 47);
            this.panelButtons.TabIndex = 37;
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btCancel.Location = new System.Drawing.Point(306, 10);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 27);
            this.btCancel.TabIndex = 21;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOK
            // 
            this.btOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btOK.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btOK.Location = new System.Drawing.Point(236, 10);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(64, 27);
            this.btOK.TabIndex = 20;
            this.btOK.Text = "确定";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // btClear
            // 
            this.btClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btClear.Enabled = false;
            this.btClear.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.btClear.Location = new System.Drawing.Point(102, 10);
            this.btClear.Name = "btClear";
            this.btClear.Size = new System.Drawing.Size(64, 27);
            this.btClear.TabIndex = 22;
            this.btClear.Text = "清除";
            this.btClear.UseVisualStyleBackColor = true;
            this.btClear.Click += new System.EventHandler(this.ClearClick);
            // 
            // panelSQL
            // 
            this.panelSQL.Controls.Add(this.tbSQL);
            this.panelSQL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSQL.Location = new System.Drawing.Point(0, 354);
            this.panelSQL.Margin = new System.Windows.Forms.Padding(2);
            this.panelSQL.Name = "panelSQL";
            this.panelSQL.Size = new System.Drawing.Size(379, 73);
            this.panelSQL.TabIndex = 38;
            // 
            // tbSQL
            // 
            this.tbSQL.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSQL.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.tbSQL.Location = new System.Drawing.Point(15, 10);
            this.tbSQL.Multiline = true;
            this.tbSQL.Name = "tbSQL";
            this.tbSQL.Size = new System.Drawing.Size(350, 55);
            this.tbSQL.TabIndex = 50;
            this.tbSQL.TextChanged += new System.EventHandler(this.tbSQL_TextChanged);
            // 
            // btApply
            // 
            this.btApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btApply.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.btApply.Location = new System.Drawing.Point(166, 10);
            this.btApply.Name = "btApply";
            this.btApply.Size = new System.Drawing.Size(64, 27);
            this.btApply.TabIndex = 24;
            this.btApply.Text = "应用";
            this.btApply.UseVisualStyleBackColor = true;
            this.btApply.Click += new System.EventHandler(this.ApllyBottomClick);
            // 
            // SelectBySQLDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(379, 474);
            this.Controls.Add(this.panelSQL);
            this.Controls.Add(this.panelCondition);
            this.Controls.Add(this.panelButtons);
            this.Controls.Add(this.panelLayer);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(380, 499);
            this.Name = "SelectBySQLDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "按属性选择";
            this.Load += new System.EventHandler(this.Select_By_Attributes_Load);
            this.panelLayer.ResumeLayout(false);
            this.panelLayer.PerformLayout();
            this.panelCondition.ResumeLayout(false);
            this.panelCondition.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            this.panelSQL.ResumeLayout(false);
            this.panelSQL.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelLayer;
        private System.Windows.Forms.CheckBox CBArea;
        private System.Windows.Forms.CheckBox CBLine;
        private System.Windows.Forms.CheckBox CBPoint;
        private System.Windows.Forms.ComboBox cbLayers;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelCondition;
        private System.Windows.Forms.Button btOnlyValue;
        private System.Windows.Forms.ListBox lbValues;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListBox lbFields;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Button btClear;
        private System.Windows.Forms.Panel panelSQL;
        private System.Windows.Forms.TextBox tbSQL;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbSelectionMethod;
        private System.Windows.Forms.Button btApply;


    }
}