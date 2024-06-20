using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
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
using System.Runtime.Serialization.Json; 
namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    public partial class FrmPieChartsSet : Form
    {
        public List<ICmykColor> CMYKColors = new List<ICmykColor>();
        List<Panel> panels = new List<Panel>();
        public int Type;
        public double RingRate = 0.4;//椭圆环比
        public double EllipseRate = 2;//椭圆率
        public double MarkerSize = 20;//统计图尺寸
        public double MarkerSizeMax = 20;//统计图尺寸
        public string ChartTitle = "";//统计图标题
        public bool IsTransparent = true;
        
        //统计数据
        public Dictionary<string, Dictionary<string, double>> ChartDatas = new Dictionary<string, Dictionary<string, double>>();
        public string LableType="";//饼图标注方式

        public string ChartName;//统计图名称
        public string ChartType;//统计图类别

        private string dataSource = "";//数据源

        public string GeoLayer = "";//地理关联图层
        public bool EnablePieVals = false;
        public IPoint BasePoint;//专题图表基点
        //序列化Json字符
        public PieJson PieInfo = null;
        public FrmPieChartsSet(IPoint anchorPoint,string chartName_, string tableType = "饼图")
        {
            InitializeComponent();
            BasePoint = anchorPoint;
            ChartType = tableType;
            ChartName = chartName_;
             
        }
       
        private void FrmChartsSet_Load(object sender, EventArgs e)
        {
           
            LoadColorHsv();
            
        }
        private List<HsvColorClass> HsvColors = new List<HsvColorClass>();
        private void LoadColorHsv()
        {
            try
            {
                string rulegdb = GApplication.Application.Template.Content.Element("ThematicRule").Value;
                string template = GApplication.Application.Template.Root + "\\" + rulegdb;

                Type factoryType = System.Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
                IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance
                    (factoryType);
                IWorkspace ws = workspaceFactory.OpenFromFile(template, 0);

                //IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();
                //IWorkspace ws = wsFactory.OpenFromFile(template, 0);
                
                ITable pTable = (ws as IFeatureWorkspace).OpenTable("ThematicColor");
                ICursor cursor = pTable.Search(null, false);
                IRow prow = null;
                int hindex = pTable.FindField("H");
                int sindex = pTable.FindField("S");
                int vindex = pTable.FindField("V");
                while ((prow = cursor.NextRow()) != null)
                {
                    string h = prow.get_Value(hindex).ToString().Trim();
                    string s = prow.get_Value(sindex).ToString().Trim();
                    string v = prow.get_Value(vindex).ToString().Trim();
                    HsvColorClass pcolor = new HsvColorClass();
                    pcolor.Hue = int.Parse(h);
                    pcolor.Saturation = int.Parse(s);
                    pcolor.Value = int.Parse(v);
                    HsvColors.Add(pcolor);
                }
                Marshal.ReleaseComObject(cursor);
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载颜色库报错："+ex.Message);
            }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            try
            {
                if (panels.Count == Type)
                {

                    GetBgColor();
                    if (CMYKColors.Count == 0)
                    {
                        MessageBox.Show("请选择颜色");
                        return;
                    }
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
                    MarkerSize = double.Parse(txtPieSizeMin.Text);
                    MarkerSizeMax = double.Parse(txtPieSizeMax.Text);
                    EllipseRate = double.Parse(txtEllipseRate.Text);
                    RingRate = double.Parse(txtRingRate.Text);
                    if (RingRate <= 0 || RingRate >= 1)
                    {
                        MessageBox.Show("环率设置错误：设置范围0~1");
                        return;
                    }
                    if (MarkerSizeMax < MarkerSize)
                    {
                        MessageBox.Show("统计图尺寸设置错误");
                        return;
                    }
                    EnablePieVals = cbStaticVals.Checked;
                    IsTransparent = cbTransparent.Checked;
                    GetChartsData();
                    GetJsonText();
                    IActiveView pAc = GApplication.Application.ActiveView;
                    double mapScale = GApplication.Application.MapControl.ReferenceScale;
                    GApplication.Application.EngineEditor.StartOperation();
                    
                    switch (ChartName)
                    {
                        case "三维饼图":
                            Draw3DPie pie = new Draw3DPie(pAc, mapScale);
                            pie.Draw3DPieCharts(this, BasePoint);
                            break;
                        case "三维圆饼图":
                            Draw3DCircelPie2 pie1 = new Draw3DCircelPie2(pAc, mapScale);
                            pie1.Draw3DCirclePie(this, BasePoint);
                            break;
                        case "三维环饼图":
                            Draw3DRingPie2 pie2 = new Draw3DRingPie2(pAc, mapScale);
                            pie2.DrawRingPieCharts(this, BasePoint);
                            break;
                    }
                    GApplication.Application.EngineEditor.StopOperation("专题图生成");
                }
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
                if (panels.Count == Type)
                {
                  
                    GetBgColor();
                    if (CMYKColors.Count == 0)
                    {
                        MessageBox.Show("请选择颜色");
                        return;
                    }
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
                    MarkerSize = double.Parse(txtPieSizeMin.Text);
                    MarkerSizeMax = double.Parse(txtPieSizeMax.Text);
                    EllipseRate = double.Parse(txtEllipseRate.Text);
                    RingRate = double.Parse(txtRingRate.Text);
                    if (RingRate <= 0 || RingRate >= 1)
                    {
                        MessageBox.Show("环率设置错误：设置范围0~1");
                        return;
                    }
                    if (MarkerSizeMax < MarkerSize)
                    {
                        MessageBox.Show("统计图尺寸设置错误");
                        return;
                    }
                    EnablePieVals = cbStaticVals.Checked;
                    IsTransparent = cbTransparent.Checked;
                    GetChartsData();
                    GetJsonText();
                    IActiveView pAc = GApplication.Application.ActiveView;
                    double mapScale = GApplication.Application.MapControl.ReferenceScale;
                    GApplication.Application.EngineEditor.StartOperation();
                    switch (ChartName)
                    { 
                        case "三维饼图":
                            Draw3DPie pie = new Draw3DPie(pAc, mapScale);
                            pie.Draw3DPieCharts(this,BasePoint);
                            break;
                        case "三维圆饼图":
                            Draw3DCircelPie2 pie1 = new Draw3DCircelPie2(pAc, mapScale);
                            pie1.Draw3DCirclePie(this, BasePoint);
                            break;
                        case "三维环饼图":
                            Draw3DRingPie2 pie2 = new Draw3DRingPie2(pAc, mapScale);
                            pie2.DrawRingPieCharts(this,BasePoint);
                            break;
                    }
                    GApplication.Application.EngineEditor.StopOperation("专题图生成");
                    this.Close();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("参数设置错误："+ex.Message);
            }
        }
        private void GetJsonText()
        {
           
            //序列化
            PieInfo = new PieJson();
            //颜色
            var listcolors = new List<ThematicColor>();
            for(int i=0;i<CMYKColors.Count;i++)
            {
                 var cmyk=CMYKColors[i];

                 ThematicColor tc = new ThematicColor { C = cmyk.Cyan, Y = cmyk.Yellow, M = cmyk.Magenta, K = cmyk.Black };
                tc.ColorName = panels[i].Tag.ToString();
                listcolors.Add(tc);
            }
            PieInfo.Colors = listcolors;

            PieInfo.Title = ChartTitle;
            PieInfo.RingRate = RingRate;
            PieInfo.EllipseRate = EllipseRate;
            PieInfo.DataSource = JsonHelper.JsonChartData(ChartDatas);

            PieInfo.GeoRalated = cbGeo.Checked;
            PieInfo.LayerName = GeoLayer;
            PieInfo.LabelInfo = LableType;
            PieInfo.TotalLable = EnablePieVals;
            PieInfo.ThematicType = "饼图";

            
            
        }
        private void GetChartsData()
        {
            
          
            ChartTitle = txtPieTitle.Text.Trim();
            
           
            if (ChartName.Contains("环"))
            {
                RingRate = double.Parse(txtRingRate.Text);
               
            }
           
            
               
         
        }
     
        private void btcancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #region 设置颜色相关
        int step = 10;
        private void DrawColor(int i, IColor co)
        {
            if (i == 0)
                step = 10;
          
            Panel panel = new Panel();
           
            //卸载
            panel.MouseDoubleClick -= new MouseEventHandler(panel_MouseDoubleClick);
            //添加事件
            panel.MouseDoubleClick += new MouseEventHandler(panel_MouseDoubleClick);
            panel.Width = 25;
            panel.Height = 100;
            panel.BackColor =ColorHelper.ConvertIColorToColor(co);
            ColorPan.Controls.Add(panel);
            if (panels.Count < Type)
            {
                panels.Add(panel);
            }
            string colortype = ChartDatas.First().Value.Keys.ToArray()[i];
            Label lb = new Label();
            lb.AutoSize = true;
            lb.Location = new System.Drawing.Point(step, 125);
            lb.Text = colortype;
            panel.Tag = new ColorInfo{ TextStr = lb.Text, BgColor = co };
            ColorPan.Controls.Add(lb);
            //动态设置panel的位置
            panel.Location = new System.Drawing.Point(step + lb.Size.Width / 2 - panel.Width / 2, 15);
            step += 10 + lb.Size.Width;

        }
        private void Clearlb()
        {
            ColorPan.Controls.Clear();
        }
        private void CreateColors(int num)
        {
            Clearlb();
            panels.Clear();
            List<int> temps = new List<int>();
            Random r = new Random();
            for (int i = 0; i < num; i++)
            {
                int color = r.Next(0, HsvColors.Count);
                while (temps.Contains(color))
                {
                    color = r.Next(0, HsvColors.Count);
                    if (temps.Count * 2 >= HsvColors.Count)
                    {
                        for (i = temps.Count; i < num; i++)
                        {
                            color = r.Next(0, HsvColors.Count);
                            temps.Add(color);
                           // Color pcolor = ColorHelper.ConvertIColorToColor(HsvColors[color]);
                           DrawColor(i, HsvColors[color]);
                        }
                        break;
                    }
                }
                if (i < num)
                {
                    temps.Add(color);
                   // Color pcolor = ColorHelper.ConvertIColorToColor(HsvColors[color]);
                    DrawColor(i, HsvColors[color]);
                }
            }
        }
        private void GetBgColor()
        {
            CMYKColors = new List<ICmykColor>();
            foreach (var panel in panels)
            {
                IColor pc = (panel.Tag as ColorInfo).BgColor;
                ICmykColor pcolor = new CmykColorClass { CMYK = pc.CMYK };
                CMYKColors.Add(pcolor);

            }
        }
        private ICmykColor ConvertRGBToCMYK(IRgbColor rgb)
        {
            ICmykColor pcolor = new CmykColorClass();
            double c = (double)(255 - rgb.Red) / 255;
            double m = (double)(255 - rgb.Green) / 255;
            double y = (double)(255 - rgb.Blue) / 255;
            double k = (double)Math.Min(c, Math.Min(m, y));
            if (k == 1.0)
            {
                c = m = y = 0;
                k = 0.6;
            }
            else
            {
                c = (c - k) / (1 - k);
                m = (m - k) / (1 - k);
                y = (y - k) / (1 - k);
            }
            c *= 100;
            m *= 100;
            y *= 100;
            k *= 100;
            pcolor.Cyan = (int)c;
            pcolor.Magenta = (int)m;
            pcolor.Yellow = (int)y;
            pcolor.Black = (int)k;

            return pcolor;
        }
        //.net color转为Icolor
        private IColor ConvertColorToIColor(Color p_Color)
        {
            IColor pColor = new RgbColorClass { RGB = p_Color.B * 65536 + p_Color.G * 256 + p_Color.R };
            return pColor;
        }

        private Color ConvertIColorToColor(IColor pRgbColor)
        {
            return ColorTranslator.FromOle(pRgbColor.RGB);
        }

        private void panel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            foreach (var p in panels)
            {
                p.BorderStyle = BorderStyle.None;
            }
            Panel panel = sender as Panel;
            panel.BorderStyle = BorderStyle.FixedSingle;
           
            //弹出颜色窗体
            FrmColor frm = new FrmColor(panel.Tag as IColor);
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                panel.BackColor =ColorHelper.ConvertIColorToColor( frm.SelectColor);
                ColorInfo info = panel.Tag as ColorInfo;
                info.BgColor = frm.SelectColor;
                panel.Tag = info;
            }
            
          
        }
        
        #endregion

        private void btrandom_Click(object sender, EventArgs e)
        {
            CreateColors(Type);
        }
        //饼图浏览数据
        private void btPieOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog dl = new OpenFileDialog();
            dl.Title = "选择源数据";
            dl.Filter = "2007Excel|*.xlsx|2003Excel|*.xls";
            dl.RestoreDirectory = true;
            dl.FilterIndex = 0;
            if (dl.ShowDialog() == DialogResult.OK)
            {

                dataSource = dl.FileName;
                txtPieTitle.Text = System.IO.Path.GetFileNameWithoutExtension(dataSource);
                if (System.IO.Path.GetExtension(dl.FileName) == ".txt")
                {
                    ChartDatas = ChartsDataSource.ObtainTxtData(dataSource);
                }
                else
                {
                    ChartDatas = ChartsDataSource.ObtainExcelData(dataSource);
                }
                if (ChartDatas.Count>0)
                {
                    this.txtPieDs.Text = dl.FileName;
                    Type = ChartDatas.First().Value.Count;
                    CreateColors(Type);
                }
            }
        }

        private void rb_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb.Checked)
            {
                LableType = rb.Text;
            }
        }

        private void cbGeo_CheckedChanged(object sender, EventArgs e)
        {
           lbGeo.Enabled=cbGeo.Checked;
           cmbGeo.Enabled=cbGeo.Checked;
           if (cbGeo.Checked)
           {
               IntialCmbGeo();
           }
        }
        private void IntialCmbGeo()
        {
            
            cmbGeo.Items.Clear();
            List<string> geoFcl = new List<string>();
            IMap pMap=GApplication.Application.ActiveView as IMap;
            if(pMap ==null)
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
            //        if ((player as IFeatureLayer).FeatureClass.ShapeType ==ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon)
            //        {
            //            geoFcl.Add(player.Name);
            //        }
            //    }
            //}
            cmbGeo.Items.AddRange(geoFcl.ToArray());
        }

        

         

        
    }
}
