namespace DatabaseUpdate
{
  partial class MatchResultForm
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
      this.dataGridView1 = new System.Windows.Forms.DataGridView();
      this.新测要素DataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.原始要素DataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.匹配度DataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.matchResultBindingSource = new System.Windows.Forms.BindingSource(this.components);
      this.button1 = new System.Windows.Forms.Button();
      this.button2 = new System.Windows.Forms.Button();
      this.button3 = new System.Windows.Forms.Button();
      this.button4 = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.matchResultBindingSource)).BeginInit();
      this.SuspendLayout();
      // 
      // dataGridView1
      // 
      this.dataGridView1.AllowUserToAddRows = false;
      this.dataGridView1.AllowUserToDeleteRows = false;
      this.dataGridView1.AllowUserToResizeRows = false;
      this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.dataGridView1.AutoGenerateColumns = false;
      this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.新测要素DataGridViewTextBoxColumn,
            this.原始要素DataGridViewTextBoxColumn,
            this.匹配度DataGridViewTextBoxColumn});
      this.dataGridView1.DataSource = this.matchResultBindingSource;
      this.dataGridView1.Location = new System.Drawing.Point(0, 0);
      this.dataGridView1.Name = "dataGridView1";
      this.dataGridView1.ReadOnly = true;
      this.dataGridView1.RowHeadersVisible = false;
      this.dataGridView1.RowTemplate.Height = 23;
      this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridView1.Size = new System.Drawing.Size(360, 265);
      this.dataGridView1.TabIndex = 0;
      // 
      // 新测要素DataGridViewTextBoxColumn
      // 
      this.新测要素DataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.新测要素DataGridViewTextBoxColumn.DataPropertyName = "新测要素";
      this.新测要素DataGridViewTextBoxColumn.HeaderText = "新测要素";
      this.新测要素DataGridViewTextBoxColumn.Name = "新测要素DataGridViewTextBoxColumn";
      this.新测要素DataGridViewTextBoxColumn.ReadOnly = true;
      // 
      // 原始要素DataGridViewTextBoxColumn
      // 
      this.原始要素DataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.原始要素DataGridViewTextBoxColumn.DataPropertyName = "原始要素";
      this.原始要素DataGridViewTextBoxColumn.HeaderText = "原始要素";
      this.原始要素DataGridViewTextBoxColumn.Name = "原始要素DataGridViewTextBoxColumn";
      this.原始要素DataGridViewTextBoxColumn.ReadOnly = true;
      // 
      // 匹配度DataGridViewTextBoxColumn
      // 
      this.匹配度DataGridViewTextBoxColumn.DataPropertyName = "匹配度";
      this.匹配度DataGridViewTextBoxColumn.HeaderText = "匹配度";
      this.匹配度DataGridViewTextBoxColumn.Name = "匹配度DataGridViewTextBoxColumn";
      this.匹配度DataGridViewTextBoxColumn.ReadOnly = true;
      // 
      // matchResultBindingSource
      // 
      this.matchResultBindingSource.DataSource = typeof(DatabaseUpdate.MatchResult);
      // 
      // button1
      // 
      this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.button1.Location = new System.Drawing.Point(12, 278);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(56, 23);
      this.button1.TabIndex = 1;
      this.button1.Text = "全选";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button2
      // 
      this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.button2.Location = new System.Drawing.Point(74, 278);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(56, 23);
      this.button2.TabIndex = 2;
      this.button2.Text = "反选";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // button3
      // 
      this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.button3.Location = new System.Drawing.Point(258, 278);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(56, 23);
      this.button3.TabIndex = 3;
      this.button3.Text = "忽略";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // button4
      // 
      this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.button4.Location = new System.Drawing.Point(196, 278);
      this.button4.Name = "button4";
      this.button4.Size = new System.Drawing.Size(56, 23);
      this.button4.TabIndex = 4;
      this.button4.Text = "更新";
      this.button4.UseVisualStyleBackColor = true;
      // 
      // MatchResultForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(360, 313);
      this.Controls.Add(this.button4);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.dataGridView1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "MatchResultForm";
      this.Text = "匹配结果";
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.matchResultBindingSource)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.DataGridView dataGridView1;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Button button3;
    private System.Windows.Forms.Button button4;
    public System.Windows.Forms.BindingSource matchResultBindingSource;
    private System.Windows.Forms.DataGridViewTextBoxColumn 新测要素DataGridViewTextBoxColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn 原始要素DataGridViewTextBoxColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn 匹配度DataGridViewTextBoxColumn;
  }
}