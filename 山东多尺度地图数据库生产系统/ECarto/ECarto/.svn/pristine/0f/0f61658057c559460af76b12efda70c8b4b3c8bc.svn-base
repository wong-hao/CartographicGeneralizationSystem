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
namespace SMGI.Plugin.EmergencyMap
{
    public partial class SDMCreateFrom : Form
    {
        public string LayerName
        {
            set;
            get;
        }

        public Dictionary<string, ICmykColor> SDMColors
        {
            set;
            get;
        }

        /// <summary>
        /// 色带面的总宽度（mm）
        /// </summary>
        public double SDMTotalWidth
        {
            set;
            get;
        }

        /// <summary>
        /// 色带面的层数
        /// </summary>
        public int SDMLayerNum
        {
            set;
            get;
        }

        private string _toolType;
        private string _colorRuleMDB;
        private string _tableName;
        private Dictionary<string, List<BOUAColor>> _listsColorDic;
        public SDMCreateFrom(string type = "")
        {
            InitializeComponent();

            _toolType = type;
            _tableName = "色带填色";
            _listsColorDic = new Dictionary<string, List<BOUAColor>>();

            SDMColors = new Dictionary<string, ICmykColor>();
            IntiParms();
        }

        private void IntiParms()
        {
            _colorRuleMDB = GApplication.Application.Template.Root + @"\专家库\Colors.mdb";

            LoadColors();
            foreach (var kv in _listsColorDic)
            {
                cmbColor.Items.Add(kv.Key);
            }
            cmbColor.SelectedIndex = 0;

            if (_toolType == "色带面交互创建")
            {
                var bouaLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper().Contains("BOUA"))).ToArray();
                for (int i = 0; i < bouaLayer.Length; i++)
                {
                    if (!bouaLayer[i].Name.ToUpper().Contains("_ATTACH"))
                        cmbLyrs.Items.Add(bouaLayer[i].Name);
                }
                lbManual.Visible = true;
                cmbLyrs.Visible = true;
                if (cmbLyrs.Items.Count > 0)
                    cmbLyrs.SelectedIndex = 0;
            }
            //默认参数设置
            foreach (Control control in ColorPanel.Controls)
            {
                if (control is Panel)
                {
                    ICmykColor color = ColorHelper.ConvertColorToCMYK(control.BackColor);
                    string type = ((control as Panel).Tag as BOUAColor).ColorIndex;
                    SDMColors[type] = color;
                }
            }
            double width = 0;
            if (!double.TryParse(txtSDMTotalWidth.Text, out width) || width == 0)
            {
                return;
            }
            SDMTotalWidth = width;
            if (_toolType == "色带面交互创建")
            {
                if (cmbLyrs.SelectedItem == null)
                {
                    MessageBox.Show("请设置色带交互图层!");
                    return;
                }
                LayerName = cmbLyrs.SelectedItem.ToString();
            }
            SDMLayerNum = (int)numSDM.Value;
        }

        private void FrmSDM_Load(object sender, EventArgs e)
        {
           

        }

        private void cmbColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadColors();
            String type = cmbColor.SelectedItem.ToString();
            DrawColorBt(type);
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
            foreach (var kv in _listsColorDic)
            {
                cmbColor.Items.Add(kv.Key);
            }
            cmbColor.SelectedIndex = 0;
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Control control in ColorPanel.Controls)
                {
                    if (control is Panel)
                    {
                        ICmykColor color = ColorHelper.ConvertColorToCMYK(control.BackColor);
                        string type = ((control as Panel).Tag as BOUAColor).ColorIndex;
                        SDMColors[type] = color;
                    }
                }

                double width = 0;
                if (!double.TryParse(txtSDMTotalWidth.Text, out width) || width == 0)
                {
                    MessageBox.Show("请输入一个合法的色带面总宽度!");
                    return;
                }
                SDMTotalWidth = width;

                if (_toolType == "色带面交互创建")
                {
                    if (cmbLyrs.SelectedItem == null)
                    {
                        MessageBox.Show("请设置色带交互图层!");
                        return;
                    }
                    LayerName = cmbLyrs.SelectedItem.ToString();
                    var bouaLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l.Name.ToUpper() == LayerName)).FirstOrDefault();
                    if (bouaLayer == null)
                    {
                        MessageBox.Show("色带交互图层未空");
                        return;
                    }
                    int ct= (bouaLayer as IFeatureLayer).FeatureClass.FeatureCount(null);
                    if (ct==0)
                    {
                        MessageBox.Show("色带交互图层没有要素，请重新选择");
                        return;
                    }
                }

                SDMLayerNum = (int)numSDM.Value;

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
            _listsColorDic.Clear();
            DataTable dt = CommonMethods.ReadToDataTable(_colorRuleMDB, _tableName);

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
                if (_listsColorDic.ContainsKey(planName))
                {
                    List<BOUAColor> colorlist = _listsColorDic[planName];
                    colorlist.Add(b);
                    _listsColorDic[planName] = colorlist;
                }
                else
                {
                    List<BOUAColor> colorlist = new List<BOUAColor>();
                    colorlist.Add(b);
                    _listsColorDic[planName] = colorlist;
                }

            }

        }
        private void DrawColorBt(string type)
        {
            int row = 0;
            var list = _listsColorDic[type];

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
            IColor color = ColorHelper.ConvertColorToIColor((sender as Panel).BackColor);
            ESRI.ArcGIS.esriSystem.tagRECT tagRect = new ESRI.ArcGIS.esriSystem.tagRECT();
            tagRect.left = (this.Left * 2 + this.Width) / 2 - 100;
            tagRect.bottom = (this.Top * 2 + this.Height) / 2 - 100;
            if (colorPalette.TrackPopupMenu(ref tagRect, color, false, 0))
            {

                (sender as Panel).BackColor = ColorHelper.ConvertIColorToColor(colorPalette.Color);
                ICmykColor cmyk = ColorHelper.ConvertColorToCMYK((sender as Panel).BackColor);
                string id = ((sender as Panel).Tag as BOUAColor).FID.ToString();
                var sql = "UPDATE " + _tableName + " SET " +
                      "C = '" + cmyk.Cyan.ToString() + "'" +
                     ",Y = '" + cmyk.Yellow.ToString() + "'" +
                     ",M = '" + cmyk.Magenta.ToString() + "'" +
                     ",K = '" + cmyk.Black.ToString() + "' where ID=" + id;

                IWorkspaceFactory awf = new AccessWorkspaceFactory();
                var ws = awf.OpenFromFile(_colorRuleMDB, 0);
                ws.ExecuteSQL(sql);
                Marshal.ReleaseComObject(ws);
                Marshal.ReleaseComObject(awf);
            }

        }

        #endregion

        private void cmbLyrs_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        
    }
}
