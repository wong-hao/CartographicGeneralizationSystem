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
using SMGI.Common;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    public partial class FrmPOIBcg : Form
    {
        //返回结果1
        public IColor CMYKColors = null;
        int hWnd = 0;
      
        
      
        //返回结果3
        public string GeoLayer="";
        private Dictionary<string, double> chartsData=new Dictionary<string,double>();
        //返回结果2:分级:name->1~2
        public Dictionary<string, string> gradeData=new Dictionary<string,string>();
        private List<double> breaksval = new List<double>();
        //每个类别对应的大小 ：1~2:12
        public Dictionary<string, double> MarkersDic = new Dictionary<string, double>();
        public string GeoTitle = "";

        public double LengendSize;
        private double MarkerMin;
        private double MarkerMax;
        public FrmPOIBcg()
        {
            InitializeComponent();
        }

        private void btDsOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog dl = new OpenFileDialog();
            dl.Title = "选择源数据";
            dl.Filter = "2007Excel|*.xlsx|2003Excel|*.xls";
            dl.RestoreDirectory = true;
            dl.FilterIndex = 0;
            if (dl.ShowDialog() == DialogResult.OK)
            {
                string ds=dl.FileName;
                txtChartDs.Text = ds;
                txtPieTitle.Text = Path.GetFileNameWithoutExtension(ds);
                ChartsDataSource.ObtainGrade(ds, ref chartsData, ref gradeData, ref breaksval);
                 
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

        private void cmbGeo_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btok_Click(object sender, EventArgs e)
        {
            try
            {
                    if (txtPieTitle.Text.Trim() == "")
                    {
                        MessageBox.Show("标题不能为空");
                        return;
                    }
                    if (gradeData.Count == 0 )
                    {
                        MessageBox.Show("请先对数据进行标准分类");
                        return;
                    }
                    CMYKColors = ConvertColorToIColor(this.btBgColor.BackColor);
                    if (cmbGeo.SelectedItem == null)
                    {
                        MessageBox.Show("请选择渲染底图");
                        return;
                    }
                    if (cmbGeo.SelectedItem != null)
                    {
                        GeoLayer = cmbGeo.SelectedItem.ToString();
                    }
                    GeoTitle = txtPieTitle.Text;
                    //尺寸
                  
                    MarkerMin = double.Parse(txtSizeMin.Text);
                    MarkerMax = double.Parse(txtSizeMax.Text);
                    GetMarkerSize();
                    DialogResult = DialogResult.OK;
                    this.Close();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数设置错误：" + ex.Message);
            }
        }


        private void GetMarkerSize()
        {
            int m = breaksval.Count - 1;
            double d = (MarkerMax - MarkerMin) / (m - 1);
            for(int i=0;i<breaksval.Count-1;i++)
            {
                double r1 = (breaksval[i]);
                double r2 = (breaksval[i + 1]);
                string key = r1.ToString() + "~" + r2.ToString();
                MarkersDic[key] = MarkerMin + i * d;

            }
        }
        private void FrmThematicBcg_Load(object sender, EventArgs e)
        {
            IntialCmbGeo();
        }

        private void btcancel_Click(object sender, EventArgs e)
        {
            Close();
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
    }
}
