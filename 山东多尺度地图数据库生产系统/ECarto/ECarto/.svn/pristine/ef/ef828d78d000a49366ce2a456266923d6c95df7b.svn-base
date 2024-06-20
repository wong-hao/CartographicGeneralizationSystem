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
    public partial class FrmLengendSet : Form
    {
        public string LgRuleXML = "";
        public FrmLengendSet()
        {
            InitializeComponent();
        }

        private void btView_Click(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.Filter = "图例规则文件|*.xml";
            of.FileName = "LengendRule";
            of.InitialDirectory = GApplication.Application.Template.Root + @"\整饰\图例专家库";
            of.RestoreDirectory = true;
            DialogResult dr = of.ShowDialog();
            if (dr == DialogResult.OK)
            {
                txtXml.Text = of.FileName;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (File.Exists(txtXml.Text))
            {
                LgRuleXML = txtXml.Text;
                this.DialogResult = DialogResult.OK;
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
