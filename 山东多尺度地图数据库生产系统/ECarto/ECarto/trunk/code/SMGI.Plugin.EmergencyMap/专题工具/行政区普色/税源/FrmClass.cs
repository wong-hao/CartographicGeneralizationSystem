using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.ADF.Connection.Local;
using NetDesign.Com;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.DataSourcesGDB;
using System.IO;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
namespace NetDesign
{
    public partial class FrmClass : Form
    {
        ColorCombox ColorCmb = null;
        List<ColorRamp> colorRamps = new List<ColorRamp>();
       
        Dictionary<string, double> regionGDB = new Dictionary<string, double>();
        //
        Dictionary<string, string> regionBreaks = new Dictionary<string, string>();
        string field = string.Empty;
        string nameField = "NAME";
        string pacField = "PAC";

        #region 外部调用参数
        public Dictionary<int, ICmykColor> ColorsDic = new Dictionary<int, ICmykColor>();
        public string BOUAName
        {
            get
            {
                return this.cmbBOUA.SelectedItem.ToString();
            }
        }
        public Dictionary<string, int> RegionColors = new Dictionary<string, int>();
        #endregion
        public FrmClass(string lyrTax, string field_)
        {
            InitializeComponent();
            ColorCmb = new ColorCombox();
            ColorCmb.Width = cmbDemo.Width;
            ColorCmb.Items.Clear();
            ColorCmb.Location = cmbDemo.Location;
           
            this.Controls.Add(ColorCmb);
            ColorCmb.SelectedIndexChanged += new EventHandler(ColorCmb_SelectedIndexChanged);
            field = field_;
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper().StartsWith("BOUA");
            }));
            foreach (var lyr in lyrs)
            {
                if ((lyr as IFeatureLayer).FeatureClass.FeatureCount(null) > 0)
                    cmbBOUA.Items.Add(lyr.Name);
            }
            lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper().StartsWith(lyrTax.ToUpper());
            }));

            foreach (var lyr in lyrs)
            {
                cmbTax.Items.Add(lyr.Name);
                cmbTax.SelectedIndex = 0;
            }
            if(cmbBOUA.Items.Count>0)
               cmbBOUA.SelectedIndex = 0;
           
        }
        
        private void ColorCmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            //对panel重新绘制颜色
            try
            {
                UpdateGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private ICmykColor GetColorByString(string cmyk)
        {
            char[] D = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            StringBuilder sb = new StringBuilder();
            //新建一个CMYK颜色，然后各项值付为0
            ICmykColor CMYK_Color = new CmykColorClass();
            CMYK_Color.Cyan = 0;
            CMYK_Color.Magenta = 0;
            CMYK_Color.Yellow = 0;
            CMYK_Color.Black = 0;
            try
            {
                for (int i = 0; i <= cmyk.Length; i++)
                {
                    if (i == cmyk.Length)
                    {
                        string sbs = sb.ToString();
                        if (sbs.Contains('C'))
                        {
                            CMYK_Color.Cyan = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('M'))
                        {
                            CMYK_Color.Magenta = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('Y'))
                        {
                            CMYK_Color.Yellow = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('K'))
                        {
                            CMYK_Color.Black = int.Parse(sbs.Substring(1));
                        }
                        break;
                    }
                    else
                    {
                        char C = cmyk[i];
                        if (D.Contains(C))
                        {
                            sb.Append(C);
                        }
                        else
                        {
                            string sbs = sb.ToString();
                            if (sbs.Contains('C'))
                            {
                                CMYK_Color.Cyan = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('M'))
                            {
                                CMYK_Color.Magenta = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('Y'))
                            {
                                CMYK_Color.Yellow = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('K'))
                            {
                                CMYK_Color.Black = int.Parse(sbs.Substring(1));
                            }
                            sb.Clear();
                            sb.Append(C);
                        }
                    }
                }
                return CMYK_Color;
            }
            catch
            {
                return null;
            }

        }
        //加载模板颜色
        private void LoadColorRamp()
        {

            string template = GApplication.Application.Template.Root + "\\RuleFoot.gdb";

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

                var cmykfrom = GetColorByString(fcolor);
                var cmykto = GetColorByString(tcolor);
                ColorRamp cr = new ColorRamp() { fromColor = cmykfrom, toColor = cmykto };
                colorRamps.Add(cr);
            }
            Marshal.ReleaseComObject(cursor);
            ColorCmb.Items.Clear();
            foreach (var cr in colorRamps)
            {
                cr.ColorNum = 5;
                ColorCmb.Items.Add(cr);
            }
            if (ColorCmb.Items.Count > 0)
                ColorCmb.SelectedIndex = 0;
        }

        private void LoadGrid()
        {
            this.dataGridView1.Rows.Clear();
            Dictionary<string,string> dic=BreakClass.Application.BreakValues(regionGDB, (int)numBreak.Value);
            var gdbType = dic.GroupBy(t => t.Value).ToList();
            Dictionary<string, double> dd = new Dictionary<string, double>();
            foreach (var kv in gdbType)
            {
                string[] strs=kv.Key.Split(new char[] { '—', '-' },StringSplitOptions.RemoveEmptyEntries);
                if (strs.Length == 1)
                {
                    dd[kv.Key] = double.Parse(strs[0]);
                }
                else
                {
                    dd[kv.Key] = (double.Parse(strs[0]) + double.Parse(strs[1])) * 0.5;
                }
            }
            var ddOrder=  dd.OrderBy(t => t.Value);
            foreach (var kv in ddOrder)
            {
                int ct = this.dataGridView1.Rows.Count;
                
                this.dataGridView1.Rows.Add(new string[] { "", kv.Key, kv.Key});

                this.dataGridView1.Rows[ct].HeaderCell.Value = (ct + 1).ToString();
            }
            regionBreaks = dic;
        }
        private void LoadGridChange()
        {
            this.dataGridView1.Rows.Clear();
            Dictionary<string, string> dic = regionBreaks;
            var gdbType = dic.GroupBy(t => t.Value).ToList();
            Dictionary<string, double> dd = new Dictionary<string, double>();
            foreach (var kv in gdbType)
            {
                string[] strs = kv.Key.Split(new char[] { '—', '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (strs.Length == 1)
                {
                    dd[kv.Key] = double.Parse(strs[0]);
                }
                else
                {
                    dd[kv.Key] = (double.Parse(strs[0]) + double.Parse(strs[1])) * 0.5;
                }
            }
            var ddOrder = dd.OrderBy(t => t.Value);
            foreach (var kv in ddOrder)
            {
                int ct = this.dataGridView1.Rows.Count;

                this.dataGridView1.Rows.Add(new string[] { "", kv.Key, kv.Key });

                this.dataGridView1.Rows[ct].HeaderCell.Value = (ct + 1).ToString();
            }
           
        }

        private string TransformPAC(string pac)
        {
            switch (cmbBOUA.SelectedItem.ToString())
            {
                case "省级行政区面":
                    pac = pac.Substring(0, 3);
                    break;
                case "地级行政区面":
                    pac = pac.Substring(0,4);
                    break;
                case "县级行政区面":
                    pac = pac.Substring(0, 6);
                    break;
                default:
                    break;
            }
            return pac;

        }

        private void FrmClass_Load(object sender, EventArgs e)
        {
           
            //列标题居中
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //单元格内容居中
            foreach (DataGridViewColumn item in this.dataGridView1.Columns)
            {
                item.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                item.SortMode = DataGridViewColumnSortMode.NotSortable;//列标题右边有预留一个排序小箭头的位置，所以整个列标题就向左边多一点，而当把SortMode属性设置为NotSortable时，不使用排序，也就没有那个预留的位置，所有完全居中了
            }
            LoadColorRamp();
        }

        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == 0&&e.RowIndex>-1)//索引-symbol
            {
                Console.WriteLine(e.RowIndex);
                e.Handled = true;
                ICmykColor pColor = ColorsDic[e.RowIndex];
                Color color = Converter.FromRGBColor(new RgbColorClass { RGB = pColor.RGB });
                using (SolidBrush brush = new SolidBrush(color))
                {
                    e.Graphics.FillRectangle(brush, e.CellBounds);
                }
               
                ControlPaint.DrawBorder(e.Graphics, e.CellBounds, color, ButtonBorderStyle.Outset);
            }

        }

        private void cBReverse_CheckedChanged(object sender, EventArgs e)
        {
            Dictionary<int, ICmykColor> copy = new Dictionary<int, ICmykColor>(ColorsDic);
            foreach (var kv in copy)
            {
                ColorsDic[copy.Count - kv.Key-1] = kv.Value;
            }

            LoadGrid();
        }
        private void UpdateGridChange()
        {
            ColorRamp colorRamp = ColorCmb.SelectedItem as ColorRamp;
            colorRamp.ColorNum =(int) (numBreak.Value);
            colorRamp.Reverse = cBReverse.Checked;
            if (ColorsDic.Count != numBreak.Value)
                ColorsDic = colorRamp.ColorItems;
            LoadGridChange();
        }
        private void UpdateGrid()
        {
            ColorRamp colorRamp = ColorCmb.SelectedItem as ColorRamp;
            if (colorRamp == null)
                return;
            colorRamp.ColorNum = (int)(numBreak.Value);
            colorRamp.Reverse = cBReverse.Checked;
            ColorsDic = colorRamp.ColorItems;
            LoadGrid();
        }
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (!change)
            {
                UpdateGrid();
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex > -1)//索引-symbol
            {
                IColorPalette colorPalette;
                colorPalette = new ColorPalette();
            
                IColor color = ColorsDic[e.RowIndex];
                tagRECT tagRect = new tagRECT();
                tagRect.left = (this.Left * 2 + this.Width) / 2 - 100;

                tagRect.bottom = (this.Top * 2 + this.Height) / 2 - 100;


                if (colorPalette.TrackPopupMenu(ref tagRect, color, false, 0))
                {
                    DataGridViewRow r = this.dataGridView1.Rows[e.RowIndex];
                    ColorsDic[e.RowIndex] = new CmykColorClass { CMYK = colorPalette.Color.CMYK };
                    this.dataGridView1.InvalidateCell(r.Cells[e.ColumnIndex]);
                }

            }
        }

        bool change = false;
        private void btDetail_Click(object sender, EventArgs e)
        {
            FrmClassDetail frm = new FrmClassDetail(regionGDB, regionBreaks);
            if (DialogResult.OK != frm.ShowDialog())
                return;
            change = true;
            numBreak.Value = frm.NumClass;
            regionBreaks = frm.regionBreaks;
            UpdateGridChange();
            change = false;
        }

        private void cmbBOUA_SelectedIndexChanged(object sender, EventArgs e)
        {
            IFeatureLayer fclLyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper().StartsWith(this.cmbTax.SelectedItem.ToString().ToUpper());
            })).FirstOrDefault() as IFeatureLayer;

            IFeatureCursor cursor = fclLyr.Search(null, false);
            IFeature fe;
            Dictionary<string, double> pacDic = new Dictionary<string, double>();
            while ((fe = cursor.NextFeature()) != null)
            {
                string gdp = fe.get_Value(fe.Class.FindField(field)).ToString();
                double val = 0;
                double.TryParse(gdp, out val);
                if (val == 0)
                    continue;
                string pac = fe.get_Value(fe.Class.FindField(pacField)).ToString();
                if (pac.Trim() == "")
                {
                    continue;
                }
                pac = TransformPAC(pac);
                if (pacDic.ContainsKey(pac))
                {
                    pacDic[pac] += val;
                }
                else
                {
                    pacDic[pac] = val;
                }

            }
            IFeatureLayer bouaLyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && (l as IFeatureLayer).Name == cmbBOUA.SelectedItem.ToString();
            })).FirstOrDefault() as IFeatureLayer;
            cursor = bouaLyr.Search(null, false);
            while ((fe = cursor.NextFeature()) != null)
            {
                string name = fe.get_Value(fe.Class.FindField(nameField)).ToString();
                string pac = fe.get_Value(fe.Class.FindField(pacField)).ToString();
                pac = TransformPAC(pac);
                if (pacDic.ContainsKey(pac))
                {
                    regionGDB[name] = pacDic[pac];
                }
            }
            UpdateGrid();
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
            {
                dic[this.dataGridView1.Rows[i].Cells[1].Value.ToString()] = i;
            }
            RegionColors.Clear();
            foreach (var kv in regionBreaks)
            {
                RegionColors[kv.Key] = dic[kv.Value];
            }
            DialogResult = DialogResult.OK;
            this.Close();

        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
