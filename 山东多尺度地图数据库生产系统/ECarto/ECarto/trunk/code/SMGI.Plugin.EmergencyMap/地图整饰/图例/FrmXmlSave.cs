using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using System.IO;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmXmlSave : Form
    {
        public FrmXmlSave()
        {
            InitializeComponent();
        }
        public string XmlPath = "";
        private void btView_Click(object sender, EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "图例配置文件|*.xml";
            sf.FileName = "LengendRule";
            sf.InitialDirectory = GApplication.Application.Template.Root + @"\整饰\图例专家库";
            DialogResult dr = sf.ShowDialog();
            if (dr != DialogResult.OK)
                return;
           
            txtFileName.Text = sf.FileName;
        }

        private void FrmXmlSave_Load(object sender, EventArgs e)
        {
            string path = GApplication.Application.Template.Root + @"\整饰\图例专家库";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                 
            }
            path += @"\LengendRule.xml";
            if (File.Exists(path))
            {
                txtFileName.Text = path;
            }


        }

        private void btOk_Click(object sender, EventArgs e)
        {
            if (txtFileName.Text.IndexOf(".xml") == -1)
            {
                MessageBox.Show("请选择保存路径");
                return;
            }
            XmlPath = txtFileName.Text;
            DialogResult = DialogResult.OK;
        }
    }
}
