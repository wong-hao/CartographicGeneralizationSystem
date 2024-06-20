using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using stdole;
using System.IO;
using ESRI.ArcGIS.Framework;
using System.Xml.Linq;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    public partial class FrmSingleBarChartsSet : Form
    {
        int hWnd = 0;
        public ICmykColor CMYKColors = null;
     
        public double MarkerSize = 20;//统计图尺寸
        public string ChartTitle = "";//统计图标题
        public double ColumnStep = 50;//条形间距
        //系列统计数据
        public Dictionary<string, Dictionary<string, double>> ChartDatas = new Dictionary<string, Dictionary<string, double>>();
      

        public string ChartName;//统计图名称
        public string ChartType;//统计图类别
        public IPoint BasePoint;//专题图表基点
       
        public bool GeoRelated = false;
        public string GeoLayer = "";//地理关联图层
        public bool ApplyPreview = false;
        public FrmSingleBarChartsSet(IPoint anchorPoint,string chartName_, string tableType = "条形图图")
        {
            InitializeComponent();
            BasePoint = anchorPoint;
            ChartType = tableType;
            ChartName = chartName_;
             
        }
       
        private void FrmChartsSet_Load(object sender, EventArgs e)
        {
          
        }


        private void btnApply_Click(object sender, EventArgs e)
        {
            try
            {
                if (ChartDatas.Count == 0)
                {
                    MessageBox.Show("请选择数据源");
                    return;
                }
                if (cbGeo.Checked)
                {
                    if (cmbGeo.SelectedItem != null)
                    {
                        GeoLayer = cmbGeo.SelectedItem.ToString();
                    }
                }
                GeoRelated = cbGeo.Checked;
                ColumnStep = Convert.ToDouble(cstep.Text);
                GetChartsData();
                IActiveView pAc =GApplication.Application.ActiveView; 
                double mapScale=GApplication.Application.MapControl.ReferenceScale;
                GApplication.Application.EngineEditor.StartOperation();
                DrawBarSingle bar = new DrawBarSingle(pAc, mapScale);
                bar.CreateSingleBar(this,BasePoint);
                GApplication.Application.EngineEditor.StopOperation("专题图生成");
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数设置错误：" + ex.Message);
            }
        }

        private void btok_Click(object sender, EventArgs e)
        {
            try
            {
                if (ChartDatas.Count == 0)
                {
                    MessageBox.Show("请选择数据源");
                    return;
                }
                if (cbGeo.Checked)
                {
                    if (cmbGeo.SelectedItem != null)
                    {
                        GeoLayer = cmbGeo.SelectedItem.ToString();
                    }
                }
                GeoRelated = cbGeo.Checked;
                ColumnStep = Convert.ToDouble(cstep.Text);
                GetChartsData();
                IActiveView pAc = GApplication.Application.ActiveView;
                double mapScale = GApplication.Application.MapControl.ReferenceScale;
                GApplication.Application.EngineEditor.StartOperation();
                DrawBarSingle bar = new DrawBarSingle(pAc, mapScale);
                bar.CreateSingleBar(this, BasePoint);
                GApplication.Application.EngineEditor.StopOperation("专题图生成");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数设置错误：" + ex.Message);
            }
        }
        private void GetChartsData()
        {

            
            ChartTitle = txtBarTitle.Text.Trim();
            MarkerSize = double.Parse(txtPieSize.Text);
            CMYKColors =ColorHelper.ConvertColorToCMYK(btBgColor.BackColor) as ICmykColor;
               
         
        }
        
        private void btcancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
       
        
        //浏览数据
        private void btPieOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog dl = new OpenFileDialog();
            dl.Title = "选择源数据";
            dl.Filter = "所有文件|*.*|2007Excel|*.xlsx|2003Excel|*.xls";
            dl.RestoreDirectory = true;
            dl.FilterIndex = 0;
            if (dl.ShowDialog() == DialogResult.OK)
            {

             
                ChartDatas.Clear();         
                string dataSource = dl.FileName;                                
                ChartDatas = ChartsDataSource.ObtainExcelData(dataSource);
                
                if (ChartDatas.Count > 0)
                {
                    this.txtDataSource.Text = dl.FileName;
                   
                }
                txtBarTitle.Text = System.IO.Path.GetFileNameWithoutExtension(dl.FileName);
            }
        }

        private void btBgColor_Click(object sender, EventArgs e)
        {
            IColorPalette colorPalette;
            colorPalette = new ColorPalette();
            System.Windows.Forms.Button btn = (System.Windows.Forms.Button)sender;
            IColor color = ConvertColorToIColor(btn.BackColor);
            tagRECT tagRect = new tagRECT();
            tagRect.left = (this.Left * 2 + this.Width) / 2 - 100;
            //tagRect.right = (this.Left*2+this.Width)/2;
            tagRect.bottom = (this.Top * 2 + this.Height) / 2 - 100;
            //tagRect.top = this.Top;

            if (colorPalette.TrackPopupMenu(ref tagRect, color, false, this.hWnd))
            {
                this.btBgColor.BackColor = ConvertIColorToColor(colorPalette.Color);

            }
        }
        /// <summary>
        /// IColor转Color
        /// </summary>
        /// <param name="pRgbColor"></param>
        /// <returns></returns>
        private Color ConvertIColorToColor(IColor pRgbColor)
        {
            return ColorTranslator.FromOle(pRgbColor.RGB);
        }
        /// <summary>
        /// Color转IColor
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private IColor ConvertColorToIColor(Color color)
        {
            IColor pColor = new RgbColorClass();
            pColor.RGB = color.B * 65536 + color.G * 256 + color.R;
            return pColor;
        }
        private void cbGeo_CheckedChanged(object sender, EventArgs e)
        {
            lbGeo.Enabled = cbGeo.Checked;
            cmbGeo.Enabled = cbGeo.Checked;
            if (cbGeo.Checked)
            {
                IntialCmbGeo();
            }
        }
        private void IntialCmbGeo()
        {

            cmbGeo.Items.Clear();
            List<string> geoFcl = new List<string>();
            IMap pMap = GApplication.Application.ActiveView as IMap;
            if (pMap == null)
                return;
            var bouaLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper().Contains("BOUA"))).ToArray();
            for (int i = 0; i < bouaLayer.Length; i++)
            {
                geoFcl.Add(bouaLayer[i].Name);
            }
            //for (int i = 0; i < pMap.LayerCount; i++)
            //{
            //    ILayer player = pMap.get_Layer(i);
            //    if (player is IGeoFeatureLayer)
            //    {
            //        if ((player as IFeatureLayer).FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon)
            //        {
            //            geoFcl.Add(player.Name);
            //        }
            //    }
            //}
            cmbGeo.Items.AddRange(geoFcl.ToArray());
        }



 
    }
}
