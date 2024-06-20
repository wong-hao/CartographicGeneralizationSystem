using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.DataSourcesGDB;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmQBOUA : Form
    {
        public double BufferDis;
        public string SQL;
        public string LyrName = "";
        string type = "SC"; 
        public FrmQBOUA()
        {
            InitializeComponent();
            sql2Level = new Dictionary<string, string>();
            sql2Level.Add("GB = 630100", "按省级行政区进行普色");
            sql2Level.Add("GB = 640100", "按地级行政区进行普色");
            sql2Level.Add("GB = 650100", "按县级行政区进行普色");
            sql2Level.Add("GB = 660100", "按乡级行政区进行普色");
        }
        private Dictionary<string, string> sql2Level;
        public FrmQBOUA(string type)
        {
            InitializeComponent();
            this.type = type;
            if (type == "XJ")
            {
                sql2Level = new Dictionary<string, string>();

                #region 重置新疆骑界普色窗体格局
                cbLevel.Visible = false;
                tbSQL.Visible = false;
                label1.Visible = false;
                label2.Visible = false;
                groupBox1.Size = new Size(469,118);
                btOk.Top = 140;
                btOk.Left = 295;
                btCancel.Top = 140;
                btCancel.Left = 402;
                this.Size = new Size(483,225);
                #endregion
            }
        }
        private void FrmQBOUL_Load(object sender, EventArgs e)
        {
            var bouaLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper().Contains("BOUA"))).ToArray();
            for (int i = 0; i < bouaLayer.Length; i++)
            {
                if (!bouaLayer[i].Name.ToUpper().Contains("_ATTACH"))
                cmbLyrs.Items.Add(bouaLayer[i].Name);
            }

            if (this.type != "XJ")
            {
                foreach (var item in sql2Level)
                {
                    cbLevel.Items.Add(item);
                }
                cbLevel.DisplayMember = "VALUE";
                cbLevel.SelectedIndex = 2;
            }
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            if (cmbLyrs.SelectedItem == null)
            {
                MessageBox.Show("请选择骑界的图层！");
                return;

            }
            double.TryParse(txtDis.Text,out BufferDis);
            if (BufferDis > 0)
            {
                LyrName = cmbLyrs.SelectedItem.ToString();
                SQL = tbSQL.Text;
                //是否存在附区
                Dictionary<string, string> envString = GApplication.Application.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
                if (envString == null)
                {
                    envString = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
                }
                bool attachMap = false;
                if (envString.ContainsKey("AttachMap"))
                {
                    attachMap = bool.Parse(envString["AttachMap"]);
                    if (attachMap)
                    {
                        if (SQL != "")
                        {
                            SQL += " and ATTACH is NULL";
                        }
                        else
                        {
                            SQL = "ATTACH is NULL";
                        }
                    }
                }
                IQueryFilter qf = null;
                if (SQL != "")
                {
                    qf = new QueryFilterClass();
                    qf.WhereClause = SQL;
                }
                ILayer boualayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && (l.Name.ToUpper() == (LyrName))).FirstOrDefault();
                int ct = (boualayer as IFeatureLayer).FeatureClass.FeatureCount(qf);
                if (ct == 0)
                {
                    MessageBox.Show("选择的行政区面要素为空！，请重新选择！");
                    return;
                }



                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("骑界宽度输入正确数字");
            }
            
        }

        private void cbLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            var kv = (KeyValuePair<string, string>)cbLevel.SelectedItem;
            tbSQL.Text = kv.Key;
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void txtDis_TextChanged(object sender, EventArgs e)
        {

        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
