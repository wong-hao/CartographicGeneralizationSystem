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
    public partial class FrmLineChartsSet : Form
    {
        public List<ICmykColor> CMYKColors = new List<ICmykColor>();
        List<Panel> panels = new List<Panel>();
        public int Type;
        public bool XYAxis = true;
        public double MarkerSize = 20;//统计图尺寸
        public string ChartTitle = "";//统计图标题
        public bool IsTransparent = true;
        public bool GeoLengend = true;
        //系列统计数据
        public Dictionary<string, Dictionary<string, double>> ChartDatas = null;
      
        public string ChartName;//统计图名称
        public string ChartType;//统计图类别

        public bool GeoDataTag = false;
        public string GeoDataTagName = "";//数据标记图层

        double maxNum = 0;
        public double KeDu = 0;
        public double TxtAngle = 0;

        public FrmLineChartsSet()
        {
            InitializeComponent();
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
                            GeoDataTagName = cmbGeo.SelectedItem.ToString();
                        }
                    }
                    IsTransparent = cbTransparent.Checked;
                    GeoDataTag = cbGeo.Checked;
                    GetChartsData();
                    DialogResult = DialogResult.OK;
                    this.Close();
                    double k=0;
                    double.TryParse(txtKD.Text, out k);
                    if (k >= KeDu)
                    {
                        double.TryParse(txtKD.Text, out KeDu);
                    }
                    double.TryParse(AngleTxt.Text, out TxtAngle);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("参数设置错误："+ex.Message);
            }
        }

        private void GetChartsData()
        {
            XYAxis = checkBoxXY.Checked;
            ChartTitle = txtPieTitle.Text.Trim();
            MarkerSize = double.Parse(txtPieSize.Text);
        }
       
        private void btcancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #region 设置颜色相关
        
        int step = 10;
        private void DrawColor(int i, Color co)
        {
            Panel panel = new Panel();
            //卸载
            string colortype = ChartDatas.Keys.ToArray()[i];
            

            panel.MouseDoubleClick -= new MouseEventHandler(panel_MouseDoubleClick);
            //添加事件
            panel.MouseDoubleClick += new MouseEventHandler(panel_MouseDoubleClick);
            panel.Width = 26;
            panel.Height = 100;

         
            panel.BackColor = co;
            ColorPan.Controls.Add(panel);
            if (panels.Count < Type)
            {
                panels.Add(panel);
            }
            //label：间隔10
            
            Label lb = new Label();
            lb.AutoSize = true;
            lb.Text = colortype;
          
            lb.Location = new System.Drawing.Point(step, 125);
            panel.Tag = lb.Text;
            ColorPan.Controls.Add(lb);
            //动态设置panel的位置
            panel.Location = new Point( step + lb.Size.Width / 2 - panel.Width/2, 15);
            step += 10 + lb.Size.Width;
        }
        private void Clearlb()
        {
            ColorPan.Controls.Clear();
           
             
        }
        private void CreateColors(int num)
        {
            step = 10;
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
                            Color pcolor = ColorHelper.ConvertIColorToColor(HsvColors[color]);
                            DrawColor(i, pcolor);
                        }
                        break;
                    }
                }
                if (i < num)
                {
                    temps.Add(color);
                    Color pcolor = ColorHelper.ConvertIColorToColor(HsvColors[color]);
                    DrawColor(i, pcolor);
                }
            }

        }
        private void GetBgColor()
        {
            CMYKColors = new List<ICmykColor>();
            foreach (var panel in panels)
            {
                IColor pc = ConvertColorToIColor(panel.BackColor);
                ICmykColor pcolor = ConvertRGBToCMYK(pc as IRgbColor);
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

        private void btrandom_Click(object sender, EventArgs e)
        {
            CreateColors(Type);
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
                string  dataSource = dl.FileName;
                 ChartDatas = ChartsDataSource.ObtainExcelData(dataSource);
                 txtPieTitle.Text = System.IO.Path.GetFileNameWithoutExtension(dl.FileName);
                if (ChartDatas.Count > 0)
                {
                    this.txtSource.Text = dl.FileName;
                    Type = ChartDatas.Count;
                    CreateColors(Type);
                    CreateStaticDatas(ChartDatas);
                    CreateStaticDatasmin(ChartDatas);
                    //double kedu = (maxNum-min) / 9.5;
                    double kedu = maxNum / 9.5;
                    //min = Math.Floor(min) - 10;
                    kedu = Math.Ceiling(kedu);
                    //kedu += 1;
                    txtKD.Text = kedu.ToString();
                    KeDu = kedu;
                }
            }
        }

        private void cbGeo_CheckedChanged(object sender, EventArgs e)
        {
            lbGeo.Enabled = cbGeo.Checked;
            cmbGeo.Enabled = cbGeo.Checked;
        }
        
        private void cbLengend_CheckedChanged(object sender, EventArgs e)
        {
            GeoLengend = cbLengend.Checked;
        }

        private void checkBoxXY_CheckedChanged(object sender, EventArgs e)
        {
            XYAxis = checkBoxXY.Checked;
        }

        private string[] CreateStaticDatas(Dictionary<string, Dictionary<string, double>> datas)
        {

            double max = 0;
            List<string> types = new List<string>();
            var data = datas.First().Value;
            foreach (var k in data)
            {
                types.Add(k.Key);
            }
            foreach (var kv in datas)
            {
                var vals = kv.Value.OrderByDescending(r => r.Value);

                max = vals.First().Value > max ? vals.First().Value : max;

            }
            maxNum = max;
            return types.ToArray();
        }
       public  double min = double.MaxValue;
        private void CreateStaticDatasmin(Dictionary<string, Dictionary<string, double>> datas)
        {
            double m = 0;
            foreach (var kv in datas)
            {
                foreach (var k in kv.Value)
                {
                    m = k.Value;
                    if (m < min)
                    {
                        min = m;
                    }
                }
            }
        }

        private void txtKD_TextChanged(object sender, EventArgs e)
        {
       
        }

        private void txtKD_Validated(object sender, EventArgs e)
        {
        }
    }
}
