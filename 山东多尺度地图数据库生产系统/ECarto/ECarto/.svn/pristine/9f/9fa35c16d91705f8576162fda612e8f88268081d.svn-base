using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using System.Xml.Linq;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmBridgeSize : Form
    {
     
        public ILayer LyrBridge=null;
        public double Tolerance;
        Dictionary<string, ILayer> mLyrsDic = new Dictionary<string, ILayer>();
        public FrmBridgeSize()
        {
            InitializeComponent();
           
        }

        

        

        private void JustifyCulvertForm_Load(object sender, EventArgs e)
        {
            string name = string.Empty;
            cb_layer.Items.Clear();
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(x =>
            {
                return x is IFeatureLayer  && ((x as IFeatureLayer).FeatureClass).AliasName.ToLower() == "lfcl";

            })).ToArray();
            foreach (var lyr in lyrs)
            {
                cb_layer.Items.Add(lyr.Name);
                mLyrsDic[lyr.Name] = lyr;
                cb_layer.SelectedIndex = 0;
            }
            //设置比例关系
            double tempScale = getTemplateScale();
            double val = tempScale/GApplication.Application.ActiveView.FocusMap.ReferenceScale;
            this.numericUpDown1.Value =(decimal)Math.Round(val, 2);
        }
        private  double getTemplateScale()
        {
            double tempScale = 1;
            GApplication app = GApplication.Application;
            var envFileName = app.Template.Content.Element("EnvironmentSettings").Value;
            string fileName = app.Template.Root + @"\" + envFileName;
            XDocument doc = XDocument.Load(fileName);
            var content= doc.Element("Template").Element("Content");
            var mapTemplate = content.Element("MapTemplate");
            string fclname = "";
            switch (mapTemplate.Value)
            {
                case "10000":
                    tempScale = 10000;
                    break;

                case "25000":
                    tempScale = 25000;
                    break;
                case "5万":
                    tempScale = 50000;
                    break;
                case "10万":
                    tempScale = 100000;
                    break;
                case "25万":
                    tempScale = 250000;
                    break;
                case "50万":
                    tempScale = 500000;
                    break;
                case "100万":
                    tempScale = 1000000;
                    break;
                default:
                    tempScale = 1000000;
                    break;
            }
            return tempScale;

        }
        private void btn_ok_Click(object sender, EventArgs e)
        {
            try
            {
                Tolerance = Convert.ToDouble(this.numericUpDown1.Value);
                LyrBridge = mLyrsDic[cb_layer.SelectedItem.ToString()];
                DialogResult = DialogResult.OK;
            }
            catch(Exception ex)
            {
                MessageBox.Show("参数错误:"+ex.Message);
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        
    }
}
