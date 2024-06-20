namespace SMGI.Common
{
    partial class TableForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.tablePanel = new System.Windows.Forms.Panel();
            this.selectPanel = new System.Windows.Forms.Panel();
            this.toolsPanel = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.tablePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tablePanel);
            this.panel1.Controls.Add(this.toolsPanel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(817, 262);
            this.panel1.TabIndex = 0;
            // 
            // tablePanel
            // 
            this.tablePanel.Controls.Add(this.selectPanel);
            this.tablePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablePanel.Location = new System.Drawing.Point(0, 0);
            this.tablePanel.Name = "tablePanel";
            this.tablePanel.Size = new System.Drawing.Size(817, 262);
            this.tablePanel.TabIndex = 0;
            this.tablePanel.Resize += new System.EventHandler(this.OnResize);
            // 
            // selectPanel
            // 
            this.selectPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectPanel.Location = new System.Drawing.Point(0, 0);
            this.selectPanel.Name = "selectPanel";
            this.selectPanel.Size = new System.Drawing.Size(817, 262);
            this.selectPanel.TabIndex = 0;
            this.selectPanel.Visible = false;
            // 
            // toolsPanel
            // 
            this.toolsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolsPanel.Location = new System.Drawing.Point(0, 262);
            this.toolsPanel.Name = "toolsPanel";
            this.toolsPanel.Size = new System.Drawing.Size(817, 0);
            this.toolsPanel.TabIndex = 1;
            this.toolsPanel.Visible = false;
            // 
            // TableForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(817, 262);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "TableForm";
            this.Text = "TableForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TableForm_FormClosed);
            this.Load += new System.EventHandler(this.TableForm_Load);
            this.panel1.ResumeLayout(false);
            this.tablePanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel tablePanel;
        private System.Windows.Forms.Panel toolsPanel;
        private System.Windows.Forms.Panel selectPanel;
    }
}