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
    public partial class FrmCloumnNumSet : Form
    {
        public List<ICmykColor> CMYKColors = new List<ICmykColor>();
        List<Panel> panels = new List<Panel>();
        List<Label> labels = new List<Label>();
        public int Type;

        public double RectRate = 1;
        public double MarkerSize = 20;//统计图尺寸
        public string ChartTitle = "";//统计图标题
        //统计数据  地名->产值
        public  Dictionary<string, Dictionary<string, double>> ChartDatas = new  Dictionary<string, Dictionary<string, double>>();

        public  Dictionary<string, double> dicInt = new Dictionary<string, double>();
        public  Dictionary<string, double> dicDecimal = new Dictionary<string, double>();
        public string ChartName;//统计图名称
        public string ChartType;//统计图类别

        private string dataSource = "";//数据源
        public string GeoLayer = "";//地理关联图层
        public bool GeoRalated = false;
        public FrmCloumnNumSet(string chartName_, string tableType = "条形图图")
        {
            InitializeComponent();
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

     
     

        private void btok_Click(object sender, EventArgs e)
        {
            try
            {
                if (panels.Count == Type)
                {
                  
                    GetBgColor();
                    GetChartsData();
                    DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("参数设置错误："+ex.Message);
            }
        }
        private void GetChartsData()
        {

            ChartTitle = txtTitle.Text.Trim();
            RectRate = double.Parse(txtRate.Text.Trim());
            MarkerSize = double.Parse(txtPieSize.Text);
            if (cbGeo.Checked)
            {
                if (cmbGeo.SelectedItem != null)
                {
                    GeoLayer = cmbGeo.SelectedItem.ToString();
                }
            }
          
               
         
        }
        
        private void btcancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #region 设置颜色相关
        private void DrawColor(int i, IColor co)
        {
            Panel panel = new Panel();
            //if (panels.Count == Type)
            //{
            //    panel = panels[i];
            //}
            //卸载
            panel.MouseDoubleClick -= new MouseEventHandler(panel_MouseDoubleClick);
            //添加事件
            panel.MouseDoubleClick += new MouseEventHandler(panel_MouseDoubleClick);
            panel.Width = 25;
            panel.Height = 100;

            panel.Location = new Point(5 + i * 40, 35);
            panel.BackColor = ColorHelper.ConvertIColorToColor(co);
            panel.Tag = new ColorInfo { TextStr = "", BgColor = co };
            ColorPan.Controls.Add(panel);
            if (panels.Count < Type)
            {
                panels.Add(panel);
            }
        }
        private void Clearlb()
        {
            foreach (Control control in ColorPan.Controls)
            {
                if (control is Panel)
                {
                    ColorPan.Controls.Remove(control);
                }
            }
            if (ColorPan.Controls.Count > 2)
                Clearlb();
        }
        private void CreateColors(int num)
        {

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
            //绘制lable
            //整数部分

            lbInt.Location = new Point(5 + (dicInt.Count-1) * 40, 138);
            lbInt.Text = "整数";
            lbDec.Text = "";
            if (dicDecimal.Count > 0)
            {
                lbDec.Location = new Point(5 + (dicInt.Count) * 40, 137);
                lbDec.Text = "小数";
               
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
        #endregion

        private void btrandom_Click(object sender, EventArgs e)
        {
            double num = maxNum;
            
            if(num!=0)
            {
              GetNumData(num);
              Type = dicInt.Count + dicDecimal.Count;
              CreateColors(Type);
            }
        }
        private void GetNumData(double val)
        {
            dicDecimal.Clear();
            dicInt.Clear();
            string txt = val.ToString();

            string[] txts = txt.Split('.');
            string str = txts[0];
            //整数
            char[] chars = str.ToCharArray();
            int length = chars.Length;
            foreach (char c in chars)
            {
                string label = "1";
                for (int i = 0; i < length - 1; i++)
                {
                    label += "0";
                }
                dicInt[label] = double.Parse(c.ToString());
                length--;
            }
            //小数
            if (txts.Length == 2)
            {
                chars = txts[1].ToCharArray();
                length = chars.Length;
                foreach (char c in chars)
                {
                    string label = "0.";
                    for (int i = 0; i < length - 1; i++)
                    {
                        label += "0";
                    }
                    label += "1";
                    dicDecimal[label] = double.Parse(c.ToString());
                    length--;
                }
            }
        }

        private double maxNum = 0;
        private void btOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog dl = new OpenFileDialog();
            dl.Title = "选择源数据";
            dl.Filter = "所有文件|*.*|2007Excel|*.xlsx|2003Excel|*.xls|TXT格式|*.txt";
            dl.RestoreDirectory = true;
            dl.FilterIndex = 0;
            if (dl.ShowDialog() == DialogResult.OK)
            {

                dataSource = dl.FileName;
                if (Path.GetExtension(dl.FileName) == ".txt")
                {
                    ChartDatas = ChartsDataSource.ObtainTxtData(dataSource);
                }
                else
                {
                    ChartDatas = ChartsDataSource.ObtainExcelData(dataSource);
                }


                if (ChartDatas.Count > 0)
                {
                    double max = double.MinValue;
                    foreach (var kv in ChartDatas)
                    {
                        if (kv.Value.First().Value > max)
                        {
                            max = kv.Value.First().Value;
                        }
                    }
                    txtTitle.Text = System.IO.Path.GetFileNameWithoutExtension(dl.FileName);
                    maxNum = max;
                    GetNumData(max);
                    Type = dicInt.Count + dicDecimal.Count;
                    CreateColors(Type);
                    this.txtSource.Text = dl.FileName;
                }

            }

        }
        private bool CreateStaticData()
        {
            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(dataSource, FileMode.Open))
                {
                    using (StreamReader sw = new StreamReader(fs, Encoding.UTF8))
                    {
                        string line;
                        while ((line = sw.ReadLine()) != null)
                        {
                            if (line.Trim() == "")
                                continue;
                            string[] infos = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                           // ChartData[infos[0]] = double.Parse(infos[1]);


                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {

                MessageBox.Show("数据源格式出错：" + ex.Message);
                return false;
            }
        }

        private void cbGeo_CheckedChanged(object sender, EventArgs e)
        {
            lbGeo.Enabled = cbGeo.Checked;
            cmbGeo.Enabled = cbGeo.Checked;
            if (cbGeo.Checked)
            {
                IntialCmbGeo();
            }
            GeoRalated = cbGeo.Checked;
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
