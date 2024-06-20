using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.DataSourcesGDB;
using SMGI.Plugin.EmergencyMap.Side;
namespace SMGI.Plugin.EmergencyMap.GX
{
    public partial class FrmSideSDM : Form
    {
        public FrmSideSDM()
        {
            InitializeComponent();
        }
        public FrmSideSDM(string type)
        {
            InitializeComponent();
            tooltype = type;
        }
        string tooltype = "";
        public string LyrName = "";
        public double SMDWidth;
        public int SMDNum;
        public Dictionary<string, ICmykColor> CMYKColors = new Dictionary<string, ICmykColor>();
        string mdbpath = "";
        string tableName = "色带填色";
        Dictionary<string, List<BOUAColor>> listsColorDic = new Dictionary<string, List<BOUAColor>>();
        public string BufferType= "LEFT";
        private void btOk_Click(object sender, EventArgs e)
        {
            if (rbLeft.Checked)
            {
                BufferType = "LEFT";
            }
            if (rbRight.Checked)
            {
                BufferType = "RIGHT";
            }
            if (rbDouble.Checked)
            {
                BufferType = "DOUBLE";
            }
            try
            {
                foreach (Control control in ColorPanel.Controls)
                {
                    if (control is Panel)
                    {
                        ICmykColor color = ColorHelper.ConvertColorToCMYK(control.BackColor);
                        string type = ((control as Panel).Tag as BOUAColor).ColorIndex;
                        //CMYKColors[type]=color;
                    }
                        

                }

                if (!double.TryParse(SDMWidth.Text, out SMDWidth) || SMDWidth==0)
                {
                    MessageBox.Show("请输入正确的数值!");
                    return;
                }
                if (tooltype != "")
                {
                    if (cmbLyrs.SelectedItem == null)
                    {
                        MessageBox.Show("请设置色带交互图层!");
                        return;
                    }
                    LyrName = cmbLyrs.SelectedItem.ToString();
                }
                SMDNum =int.Parse(txtNum.Value.ToString());
                DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
                return;
            }
          
        }

        private void colorDetail_Click(object sender, EventArgs e)
        {
            String type = "";
            if (cmbColor.SelectedItem != null)
            {
                type = cmbColor.SelectedItem.ToString();
            }
           
            FrmSDMColor frm = new FrmSDMColor(type);
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;
            LoadColors();
            cmbColor.Items.Clear();
            foreach (var kv in listsColorDic)
            {
                cmbColor.Items.Add(kv.Key);
            }
            cmbColor.SelectedIndex = 0;
        }

        private void FrmSDM_Load(object sender, EventArgs e)
        {
            mdbpath = GApplication.Application.Template.Root + @"\专家库\Colors.mdb";
          
            LoadColors();
            foreach (var kv in listsColorDic)
            {
                cmbColor.Items.Add(kv.Key);
            }
            cmbColor.SelectedIndex = 0;

            if (tooltype == "色带面交互创建")
            {
                var bouaLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper().Contains("BOUA"))).ToArray();
                for (int i = 0; i < bouaLayer.Length; i++)
                {
                    if (!bouaLayer[i].Name.ToUpper().Contains("_ATTACH"))
                        cmbLyrs.Items.Add(bouaLayer[i].Name);
                }
                lbManual.Visible = true;
                cmbLyrs.Visible = true;
                if(cmbLyrs.Items.Count>0)
                  cmbLyrs.SelectedIndex = 0;
            }
           
        }
        #region //颜色处理
        private void ClearControls()
        {
            foreach (Control control in ColorPanel.Controls)
            {

                ColorPanel.Controls.Remove(control);

            }
            if (ColorPanel.Controls.Count > 0)
                ClearControls();
        }
        private void LoadColors()
        {
            ClearControls();
            listsColorDic.Clear();
            DataTable dt = CommonMethods.ReadToDataTable(mdbpath, tableName);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];

                string planName = "方案" + dr["色带方案"].ToString();
                BOUAColor b = new BOUAColor();
                b.FID = int.Parse(dr["ID"].ToString());
                b.ColorPlan = planName;
                b.ColorIndex = dr["类型"].ToString();

                int c1 = int.Parse(dr["C"].ToString());
                int m = int.Parse(dr["M"].ToString());
                int y = int.Parse(dr["Y"].ToString());
                int k = int.Parse(dr["K"].ToString());
                CmykColorClass pcolor = new CmykColorClass();
                pcolor.Cyan = c1;
                pcolor.Magenta = m;
                pcolor.Yellow = y;
                pcolor.Black = k;
                b.CmykColor = pcolor;
                if (listsColorDic.ContainsKey(planName))
                {
                    List<BOUAColor> colorlist = listsColorDic[planName];
                    colorlist.Add(b);
                    listsColorDic[planName] = colorlist;
                }
                else
                {
                    List<BOUAColor> colorlist = new List<BOUAColor>();
                    colorlist.Add(b);
                    listsColorDic[planName] = colorlist;
                }

            }

        }
        private void DrawColorBt(string type)
        {
            CMYKColors.Clear();
            int row = 0;
            var list = listsColorDic[type];

            int colomn = 0;

            Label lbinfo = new Label();
            lbinfo.Top = 25 + 55 * (row);
            lbinfo.Left = 5;
            lbinfo.AutoSize = true;
            lbinfo.Text = type;
            ColorPanel.Controls.Add(lbinfo);
           // var listOrder = list.OrderBy(t => int.Parse(t.ColorIndex));
            foreach (var l in list)
            {

                int x = colomn++;
                int y = row;

                Color pcolor = ColorHelper.ConvertICMYKColorToColor(l.CmykColor);
                Panel panel = new Panel();
                panel.Width = 35;
                panel.Tag = l;
                panel.Height = 30;
                panel.Location = new System.Drawing.Point(45 + x * 55, 15 + 55 * y);
                panel.BackColor = pcolor;
                ColorPanel.Controls.Add(panel);

                Label lb = new Label();
                lb.Top = 50 + 55 * y;
                lb.Left = 45 + x * 55;
                lb.AutoSize = true;
                lb.Text = ("" + l.ColorIndex).Trim();
                ColorPanel.Controls.Add(lb);
                CMYKColors[l.ColorIndex] = l.CmykColor;
                panel.MouseClick += new MouseEventHandler(panel_MouseClick);
                panel.MouseDoubleClick += new MouseEventHandler(panel_MouseDoubleClick);

            }


        }
        private void panel_MouseClick(object sender, MouseEventArgs e)
        {
            foreach (Control c in ColorPanel.Controls)
            {
                if (c.GetType() == typeof(System.Windows.Forms.Panel))
                {
                    (c as Panel).BorderStyle = BorderStyle.None;
                }
            }
            (sender as Panel).BorderStyle = BorderStyle.Fixed3D;
            //PanelSe = (sender as Panel);
        }
        private void panel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            foreach (Control c in ColorPanel.Controls)
            {
                if (c.GetType() == typeof(System.Windows.Forms.Panel))
                {
                    (c as Panel).BorderStyle = BorderStyle.None;
                }
            }
            (sender as Panel).BorderStyle = BorderStyle.Fixed3D;

            //弹出颜色窗体
            IColorPalette colorPalette;
            colorPalette = new ColorPalette();
            IColor color = CMYKColors[((sender as Panel).Tag as BOUAColor).ColorIndex];
            ESRI.ArcGIS.esriSystem.tagRECT tagRect = new ESRI.ArcGIS.esriSystem.tagRECT();
            tagRect.left = (this.Left * 2 + this.Width) / 2 - 100;
            tagRect.bottom = (this.Top * 2 + this.Height) / 2 - 100;
            if (colorPalette.TrackPopupMenu(ref tagRect, color, false, 0))
            {
                string type = ((sender as Panel).Tag as BOUAColor).ColorIndex;
                ICmykColor cmyk = new CmykColorClass { CMYK = colorPalette.Color.CMYK };
                CMYKColors[type] = cmyk;
                
                (sender as Panel).BackColor = ColorHelper.ConvertIColorToColor(colorPalette.Color);
                
                string id = ((sender as Panel).Tag as BOUAColor).FID.ToString();
                var sql = "UPDATE " + tableName + " SET " +
                      "C = '" + cmyk.Cyan.ToString() + "'" +
                     ",Y = '" + cmyk.Yellow.ToString() + "'" +
                     ",M = '" + cmyk.Magenta.ToString() + "'" +
                     ",K = '" + cmyk.Black.ToString() + "' where ID=" + id;

                IWorkspaceFactory awf = new AccessWorkspaceFactory();
                var ws = awf.OpenFromFile(mdbpath, 0);
                ws.ExecuteSQL(sql);
                Marshal.ReleaseComObject(ws);
                Marshal.ReleaseComObject(awf);
            }

        }

        #endregion

        private void cmbColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadColors();
            String type = cmbColor.SelectedItem.ToString();
            DrawColorBt(type);
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (rbLeft.Checked)
            {
                BufferType = "LEFT";
            }
            if (rbRight.Checked)
            {
                BufferType = "RIGHT";
            }
            if (rbDouble.Checked)
            {
                BufferType = "DOUBLE";
            }
            if (!double.TryParse(SDMWidth.Text, out SMDWidth) || SMDWidth == 0)
            {
                MessageBox.Show("请输入正确的数值!");
                return;
            }

            var lys = GApplication.Application.Workspace.LayerManager.GetLayer(l => l.Visible && l is IGeoFeatureLayer && ((IGeoFeatureLayer)l).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline).ToList();
            var fes = new List<IFeature>();
            var fas = (IEnumFeature)GApplication.Application.ActiveView.FocusMap.FeatureSelection;
            IFeature fa;
            while ((fa = fas.Next()) != null)
            {
                if (fa.Shape is IPolyline)
                    fes.Add(fa);
            }
            if (fes.Count == 0)
            {
                MessageBox.Show("请选中线要素！");
                return;
            }
            SideHelper.InStance.CreateView(BufferType, fes, SMDWidth);
        }

        private void FrmSideSDM_FormClosed(object sender, FormClosedEventArgs e)
        {
            (GApplication.Application.ActiveView as IGraphicsContainer).DeleteAllElements();
            GApplication.Application.ActiveView.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewGraphics, null, null);
    
        }
    }
}
