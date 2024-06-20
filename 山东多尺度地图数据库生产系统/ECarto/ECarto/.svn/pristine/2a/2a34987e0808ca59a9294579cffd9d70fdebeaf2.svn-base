using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.SystemUI;
using System.Security.Cryptography;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
namespace SMGI.Plugin.EmergencyMap.LabelSym
{
    public partial class FrmLbExpress : Form
    {

        public FrmLbExpress(string ex="")
        {
            InitializeComponent();
            Expression.Text = ex;
        }
       
        private void FrmAnnoMap_Load(object sender, EventArgs e)
        {
           
           
        }
        public string ExpressionStr
        {
            get
            {
                return Expression.Text;
            }
        }
        private void btOk_Click(object sender, EventArgs e)
        {
            if (!Expression.Text.ToUpper().Contains("FUNCTION"))
            {
                MessageBox.Show("请添加注记表达式！");
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
             
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
