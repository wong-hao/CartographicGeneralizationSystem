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
namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    public partial class FrmThematicBcg : Form
    {
        private List<ICmykColor> CMYKColors = new List<ICmykColor>();
        private List<Panel> panels = new List<Panel>();
        //返回结果1
        public Dictionary<string, ICmykColor> ColorsDic = new Dictionary<string, ICmykColor>();
        private int Type;
        //返回结果3
        public string GeoLayer="";
        private Dictionary<string, double> chartsData=new Dictionary<string,double>();
        //返回结果2
        public Dictionary<string, string> gradeData=new Dictionary<string,string>();
        private List<double> breaksval = new List<double>();
        public string GeoTitle = "";
        public FrmThematicBcg()
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
                txtPieTitle.Text = Path.GetFileNameWithoutExtension(ds);
                txtChartDs.Text = ds;
                chartsData.Clear(); gradeData.Clear(); breaksval.Clear();
                ChartsDataSource.ObtainGrade(ds, ref chartsData, ref gradeData, ref breaksval);
                Type = breaksval.Count - 1;
                CreateColors(Type);
                fillList(Type);
            }
        }
        #region 设置颜色相关
        private void Clearlb()
        {
            panels.Clear();
            ColorPan.Controls.Clear();
        }
        private void DrawColor(int i, Color co)
        {
            Panel panel = new Panel();
            if (panels.Count == Type)
            {
                panel = panels[i];
            }
            //卸载
            panel.MouseDoubleClick -= new MouseEventHandler(panel_MouseDoubleClick);
            //添加事件
            panel.MouseDoubleClick += new MouseEventHandler(panel_MouseDoubleClick);
            panel.Width = 25;
            panel.Height = 100;

            panel.Location = new Point(15 + i * 100, 35);
            panel.BackColor = co;
           
            double r1 =(breaksval[i]);
            double r2 =(breaksval[i+1]);
            Label lb = new Label();
            lb.Location = new Point(5 + i * 100, 138);
            lb.Text = r1.ToString() + "~" + r2.ToString();
            panel.Tag = lb.Text;
            ColorPan.Controls.Add(panel);
            ColorPan.Controls.Add(lb);
            if (panels.Count < Type)
            {
                panels.Add(panel);
            }
        }
        private void CreateColors(int num)
        {
            Clearlb();
            CMYKColors = new List<ICmykColor>();
            //Dictionary<int, Color> colorsDic = new Dictionary<int, Color>();
            //int ct = 1;
            //foreach (var c in (typeof(Color)).GetMembers())
            //{
            //    if (c.MemberType == System.Reflection.MemberTypes.Property)
            //    {
            //        Color item = Color.FromName(c.Name);
            //        if (ct > 141)//过滤白色
            //            continue;
            //        if (ct == 1)//过滤透明色
            //        {
            //            ct++;
            //            continue;
            //        }
            //        colorsDic[ct++] = item;

            //    }
            //}
            Random r = new Random();
            List<int> temps = new List<int>();
            for (int i = 0; i < num; i++)
            {
                //int color = r.Next(2, 141);
                //while (temps.Contains(color))
                //{
                //    color = r.Next(2, 141);
                //}
                //temps.Add(color);
                //IColor pc = ConvertColorToIColor(colorsDic[color]);
                //ICmykColor pcolor = ConvertRGBToCMYK(pc as IRgbColor);
                //CMYKColors.Add(pcolor);
                // DrawColor(i, colorsDic[color]);
            }

        }
        private void GetBgColor()
        {
            CMYKColors = new List<ICmykColor>();
            foreach (var panel in panels)
            {
                IColor pc = ConvertColorToIColor(panel.BackColor);
                ICmykColor pcolor = ConvertRGBToCMYK(pc as IRgbColor);
                ColorsDic[panel.Tag.ToString()] = pcolor;

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
            }
           
        }
        #endregion
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
                if (panels.Count == 0)
                {
                    MessageBox.Show("数据未进行标准化,或者标准化格式错误。");
                    return;
                }
                if (panels.Count == Type)
                {

                    GetBgColor();
                    
                    if (ColorsDic.Count == 0)
                    {
                        MessageBox.Show("请选择数据源");
                        return;
                    }
                    if ( gradeData.Count == 0 )
                    {
                        MessageBox.Show("请先对数据进行标准分类");
                        return;
                    }
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
                
                    DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数设置错误：" + ex.Message);
            }
        }

        private void btrandom_Click(object sender, EventArgs e)
        {
            CreateColors(Type);
        }
        private ColorCombox ColorCmb=null;
        private List<ColorRamp> colorRamps = new List<ColorRamp>();
        private void FrmThematicBcg_Load(object sender, EventArgs e)
        {
            IntialCmbGeo();
            ColorCmb = new ColorCombox();
            ColorCmb.Items.Clear();
            ColorCmb.Location = new Point(110, 53);
            
            splitContainer1.Panel2.Controls.Add(ColorCmb);
            ColorCmb.SelectedIndexChanged+=new EventHandler(ColorCmb_SelectedIndexChanged);
            LoadColorRamp();
        }
        private void ColorCmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            //对panel重新绘制颜色
            try
            {
                ColorRamp colorRamp = ColorCmb.SelectedItem as ColorRamp;
                int n = colorRamp.ColorNum;
                ICmykColor f = colorRamp.fromColor;
                ICmykColor t = colorRamp.toColor;
                int dc = (t.Cyan - f.Cyan) / (n - 1);
                int dm = (t.Magenta - f.Magenta) / (n - 1);
                int dy = (t.Yellow - f.Yellow) / (n - 1);

                int c, m, y;
                for (int x = 0; x < n; x++)
                {

                    c = f.Cyan + x * dc;
                    m = f.Magenta + x * dm;
                    y = f.Yellow + x * dy;
                    ICmykColor temp = new CmykColorClass();
                    temp.Cyan = c;
                    temp.Magenta = m;
                    temp.Yellow = y;
                    Color co = ColorHelper.ConvertICMYKColorToColor(temp);

                    DrawColor(x, co);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //加载模板颜色
        private void LoadColorRamp()
        {
            string rulegdb = GApplication.Application.Template.Content.Element("ThematicRule").Value;
            string template = GApplication.Application.Template.Root + "\\" + rulegdb;
            
            IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();
            IWorkspace ws = wsFactory.OpenFromFile(template, 0);
            ITable pTable = (ws as IFeatureWorkspace).OpenTable("ColorRamp");
            ICursor cursor = pTable.Search(null, false);
            IRow prow = null;
            int findex = pTable.FindField("起始值");
            int tindex = pTable.FindField("终止值");
            
            while ((prow = cursor.NextRow()) != null)
            {
                string fcolor = prow.get_Value(findex).ToString().Trim();
                string tcolor = prow.get_Value(tindex).ToString().Trim();

                var cmykfrom=  ColorHelper.GetColorByString(fcolor);
                var cmykto = ColorHelper.GetColorByString(tcolor);
                ColorRamp cr = new ColorRamp() { fromColor = cmykfrom, toColor = cmykto };
                colorRamps.Add(cr);
            }
            Marshal.ReleaseComObject(cursor);
        }
        private void fillList(int colortype)
        {
            ColorCmb.Items.Clear();
            foreach (var cr in colorRamps)
            {
                cr.ColorNum = colortype;
                ColorCmb.Items.Add(cr);
            }
            if(ColorCmb.Items.Count>0)
               ColorCmb.SelectedIndex = 0;
          
        }
        private void btcancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
