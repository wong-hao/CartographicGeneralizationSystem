using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Common
{
    public partial class TemplateSelectForm : Form
    {
        TemplateManager _templateMgr;
        public TemplateManager TemplateManager
        {
            get { return _templateMgr; }
            set { _templateMgr = value; }
        }

        public List<GTemplate> CurrentTemplates
        {
            get;
            private set;
        }

        internal TemplateSelectForm(PlatformInfo platformInfo)
        {
            CurrentTemplates = new List<GTemplate>();
            InitializeComponent();

            for (int i = 0; i < platformInfo.ProList.Count; i++)
            {
                //控件创建
                Button btn = new Button();
                this.Controls.Add(btn);
                btn.Location = new System.Drawing.Point(platformInfo.ProList[i].LocationX, platformInfo.ProList[i].LocationY); ;
                btn.Size = new System.Drawing.Size(platformInfo.ProList[i].Width,platformInfo.ProList[i].Height);
                btn.Name = string.Format("btn{0}", i + 1);
                btn.DialogResult = System.Windows.Forms.DialogResult.OK;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.TabIndex = i;
                btn.UseVisualStyleBackColor = true;
                btn.Enabled = false;

                //控件初始化
                btn.Tag = platformInfo.ProList[i].Name;
                btn.Image = null;
                btn.BackgroundImage = new Bitmap(GApplication.ResourcePath + "\\" + platformInfo.ProList[i].ImagePath);
                btn.BackgroundImageLayout = ImageLayout.Stretch;
                btn.Enabled = platformInfo.ProList[i].Usable;
                btn.Click += btnClick;
            }

            string imageFile = GApplication.ResourcePath + "\\" + platformInfo.BGImage;
            if(System.IO.File.Exists(imageFile))
                this.BackgroundImage = new Bitmap(imageFile);
            this.BackgroundImageLayout = ImageLayout.Stretch;
        }

        void btnClick(object sender, EventArgs e)
        {
            foreach (var item in _templateMgr.Templates)
            {
                if (item.ClassName == (sender as Button).Tag.ToString())
                {
                    CurrentTemplates.Add(item);
                }
            }
        }

        private void TemplateSelectForm_Load(object sender, EventArgs e)
        {
            
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}
