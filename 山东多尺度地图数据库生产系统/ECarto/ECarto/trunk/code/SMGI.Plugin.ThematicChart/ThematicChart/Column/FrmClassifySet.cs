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

namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    public partial class FrmClassifySet : Form
    {
        public FrmClassifySet(IPoint anchorPoint,string chartName_, string tableType = "分类图")
        {
            InitializeComponent();
            BasePoint = anchorPoint;
            ChartType = tableType;
            ChartName = chartName_;
        }
        public List<ICmykColor> CMYKColors = new List<ICmykColor>();
        List<Panel> panels = new List<Panel>();
        public int Type;
        public IPoint BasePoint;//专题图表基点
        public double MarkerSize = 20;//统计图尺寸
        public string ChartTitle = "";//统计图标题
        public bool GeoRelated = false;
        public bool GeoLengend = true;
        public bool GeoNum = true;
        public bool IsTransparent = true;
        double maxNum = 0;
        public double TxtAngle = 0;
        public double ColumnStep = 200;
        //系列统计数据
        public Dictionary<string, Dictionary<string, double>> ChartDatas = null;


        public string ChartName;//统计图名称
        public string ChartType;//统计图类别

        private string dataSource = "";//数据源
        public string GeoLayer = "";//地理关联图层

        private void panel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            foreach (var p in panels)
            {
                p.BorderStyle = BorderStyle.None;
            }
            Panel panel = sender as Panel;
            panel.BorderStyle = BorderStyle.FixedSingle;
            //弹出颜色窗体
            FrmColor frm = new FrmColor(panel.BackColor);
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                panel.BackColor = ColorHelper.ConvertIColorToColor(frm.SelectColor);
                ColorInfo info = panel.Tag as ColorInfo;
                info.BgColor = frm.SelectColor;
                panel.Tag = info;
            }

        }


        private void btPieOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog dl = new OpenFileDialog();
            dl.Title = "选择源数据";
            dl.Filter = "所有文件|*.*|2007Excel|*.xlsx|2003Excel|*.xls|TXT格式|*.txt";
            dl.RestoreDirectory = true;
            dl.FilterIndex = 0;
            if (dl.ShowDialog() == DialogResult.OK)
            {


                dataSource = dl.FileName;
                 
                
                ChartDatas = ChartsDataSource.ObtainExcelData(dataSource);
                 
                if (ChartDatas.Count > 0)
                {
                    this.txtSource.Text = dl.FileName;
                    this.txtPieTitle.Text = System.IO.Path.GetFileNameWithoutExtension(dl.FileName);
                    Type = ChartDatas.First().Value.Count;
                    CreateColors(Type);
                   
                }
            }
        }


        private string[] CreateStaticDatasClass(Dictionary<string, Dictionary<string, double>> datas)
        {
            List<string> types = new List<string>();
            var data = datas.First().Value;
            foreach (var k in data)
            {
                types.Add(k.Key);
            }

            foreach (var kv in datas)
            {
                double max = 0;
                foreach (var k in kv.Value)
                {
                    max += k.Value;
                }
                if (max > maxNum)
                {
                    maxNum = max;
                }
            }
            return types.ToArray();
        }
        private void LoadColorHsv()
        {
            string rulegdb = GApplication.Application.Template.Content.Element("ThematicRule").Value;
            string template = GApplication.Application.Template.Root + "\\" + rulegdb;

            IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();
            IWorkspace ws = wsFactory.OpenFromFile(template, 0);
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
        private List<HsvColorClass> HsvColors = new List<HsvColorClass>();
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
        public IColor ConvertColorToIColor(Color p_Color)
        {
            IColor pColor = new RgbColorClass { RGB = p_Color.B * 65536 + p_Color.G * 256 + p_Color.R };
            return pColor;
        }
        private Color ConvertIColorToColor(IColor pRgbColor)
        {
            return ColorTranslator.FromOle(pRgbColor.RGB);
        }
        int step = 10;
        private void DrawColor(int i, IColor co)
        {
            Panel panel = new Panel();
            if (i == 0)
            {
                step = 10;
            }
            //卸载
            panel.MouseDoubleClick -= new MouseEventHandler(panel_MouseDoubleClick);
            //添加事件
            panel.MouseDoubleClick += new MouseEventHandler(panel_MouseDoubleClick);
            panel.Width = 25;
            panel.Height = 100;


            panel.BackColor = ColorHelper.ConvertIColorToColor(co);
            ColorPan.Controls.Add(panel);
            if (panels.Count < Type)
            {
                panels.Add(panel);
            }
            //添加lable
            string colortype = ChartDatas.First().Value.Keys.ToArray()[i];
            Label lb = new Label(); lb.AutoSize = true;
            lb.Location = new System.Drawing.Point(step, 125);
            lb.Text = colortype;
            panel.Tag = new ColorInfo { TextStr = lb.Text, BgColor = co };
            ColorPan.Controls.Add(lb);
            panel.Location = new System.Drawing.Point(step + lb.Size.Width / 2 - panel.Width / 2, 15);
            step += 10 + lb.Size.Width;
        }
        private void Clearlb()
        {
            ColorPan.Controls.Clear();
             
        }
        private void CreateColors(int num)
        {
            panels.Clear();
            Clearlb();
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
                            Color pcolor = ColorHelper.ConvertIColorToColor(HsvColors[color]);
                            DrawColor(i, HsvColors[color]);
                        }
                        break;
                    }
                }
                if (i < num)
                {
                    temps.Add(color);
                    Color pcolor = ColorHelper.ConvertIColorToColor(HsvColors[color]);
                    DrawColor(i, HsvColors[color]);
                }
            }

        }
        private void GetChartsData()
        {
            ChartTitle = txtPieTitle.Text.Trim();
            MarkerSize = double.Parse(txtPieSize.Text);
        }
        private void btrandom_Click(object sender, EventArgs e)
        {
            CreateColors(Type);
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

                    MarkerSize = Convert.ToDouble(txtPieSize.Text);
                    double.TryParse(AngleTxt.Text, out TxtAngle);
                    double.TryParse(cstep.Text, out ColumnStep);
                    IsTransparent = cbTransparent.Checked;
                    GetChartsData();
                    GetJsonText();
                    IActiveView pAc = GApplication.Application.ActiveView;
                    double mapScale = GApplication.Application.MapControl.ReferenceScale;
                    GApplication.Application.EngineEditor.StartOperation();
                    switch(ChartName)
                    {
                        case "二维分类柱状图":
                            DrawClassify column1 = new DrawClassify(pAc, mapScale);
                            column1.CreateClassifyColumns(this,BasePoint);
                            break;
                        case "三维分类柱状图": ;
                            DrawClassify3D column2 = new DrawClassify3D(pAc, mapScale);
                            column2.CreateClassifyColumns(this,BasePoint);
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

                    MarkerSize = Convert.ToDouble(txtPieSize.Text);
                    double.TryParse(AngleTxt.Text, out TxtAngle);
                    double.TryParse(cstep.Text, out ColumnStep);
                    IsTransparent = cbTransparent.Checked;
                    GetChartsData();
                    GetJsonText();
                    IActiveView pAc = GApplication.Application.ActiveView;
                    double mapScale = GApplication.Application.MapControl.ReferenceScale;
                    GApplication.Application.EngineEditor.StartOperation();
                    DrawClassify3D column = new DrawClassify3D(pAc, mapScale);
                    column.CreateClassifyColumns(this, BasePoint);
                    GApplication.Application.EngineEditor.StopOperation("专题图生成");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数设置错误：" + ex.Message);
            }
        }
        //序列化Json字符
        public ColumnJson ColomInfo = null;
        private void GetJsonText()
        {
            //序列化
            ColomInfo = new ColumnJson();
            //颜色
            var listcolors = new List<ThematicColor>();
            for (int i = 0; i < CMYKColors.Count; i++)
            {
                var cmyk = CMYKColors[i];

                ThematicColor tc = new ThematicColor { C = cmyk.Cyan, Y = cmyk.Yellow, M = cmyk.Magenta,K=cmyk.Black};
                tc.ColorName = panels[i].Tag.ToString();
                listcolors.Add(tc);
            }
            ColomInfo.Colors = listcolors;
            ColomInfo.Title = ChartTitle;
            ColomInfo.DataSource = JsonHelper.JsonChartData(ChartDatas);
            ColomInfo.ThematicType = "分类柱状图";
            ColomInfo.Size = double.Parse(txtPieSize.Text);
            ColomInfo.LengendShow = cbLengend.Checked;
            ColomInfo.GeoNum = cbNum.Checked;
            ColomInfo.TxtAngel = TxtAngle;
            ColomInfo.IsTransparent = cbTransparent.Checked;
        }
        private void btcancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Frmclass_Load(object sender, EventArgs e)
        {
            LoadColorHsv();
        }
        private void IntialCmbGeo()
        {

            //cmbGeo.Items.Clear();
            List<string> geoFcl = new List<string>();
            IMap pMap = GApplication.Application.ActiveView as IMap;
            if (pMap == null)
                return;
            for (int i = 0; i < pMap.LayerCount; i++)
            {
                ILayer player = pMap.get_Layer(i);
                if (player is IGeoFeatureLayer)
                {
                    if ((player as IFeatureLayer).FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon)
                    {
                        geoFcl.Add(player.Name);
                    }
                }
            }
            //cmbGeo.Items.AddRange(geoFcl.ToArray());
        }

        private void cbGeo_CheckedChanged(object sender, EventArgs e)
        {
        //    lbGeo.Enabled = cbGeo.Checked;
        //    cmbGeo.Enabled = cbGeo.Checked;
        //    if (cbGeo.Checked)
        //    {
        //        IntialCmbGeo();
        //    }
        }

        private void cbLengend_CheckedChanged(object sender, EventArgs e)
        {
            GeoLengend = cbLengend.Checked;
        }

        private void cbNum_CheckedChanged(object sender, EventArgs e)
        {
            GeoNum = cbNum.Checked;
        }


    
    
    
    }
}
