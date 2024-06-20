using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Output;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using System.IO;
using System.Xml.Linq;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Framework;
namespace SMGI.Plugin.EmergencyMap.OneKey
{
    public partial class FrmSDMSet : Form
    {
        public FrmSDMSet()
        {
            InitializeComponent();
            #region 色带初始化
            var mdbpath = GApplication.Application.Template.Root + @"\专家库\Colors.mdb";
            string tableName = "色带填色";
            LoadSDMColors(mdbpath, tableName);
            foreach (var kv in listsColorDic)
            {
                cmbColor.Items.Add(kv.Key);
            }
            cmbColor.SelectedIndex = 0;
            #endregion
            AutoSet();
        }
        #region 色带颜色设置
        Dictionary<string, List<BOUAColor>> listsColorDic = new Dictionary<string, List<BOUAColor>>();
        private void ClearControls(Panel panel)
        {
            foreach (Control control in panel.Controls)
            {

                panel.Controls.Remove(control);

            }
            if (panel.Controls.Count > 0)
                ClearControls(panel);
        }
        private void LoadSDMColors(string mdbpath, string tableName)
        {
            ClearControls(ColorPanel);
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
        private void DrawColorBt(string type, Panel panelMain)
        {
            int row = 0;
            var list = listsColorDic[type];

            int colomn = 0;

            Label lbinfo = new Label();
            lbinfo.Top = 25 + 55 * (row);
            lbinfo.Left = 5;
            lbinfo.AutoSize = true;
            lbinfo.Text = type;
            panelMain.Controls.Add(lbinfo);
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
                panelMain.Controls.Add(panel);

                Label lb = new Label();
                lb.Top = 50 + 55 * y;
                lb.Left = 45 + x * 55;
                lb.AutoSize = true;
                lb.Text = ("" + l.ColorIndex).Trim();
                panelMain.Controls.Add(lb);

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
            string tableName = "色带填色";
            var mdbpath = GApplication.Application.Template.Root + @"\专家库\Colors.mdb";
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
                var sql = "UPDATE " + tableName + " SET " +
                      "C = '" + cmyk.Cyan.ToString() + "'" +
                     ",Y = '" + cmyk.Yellow.ToString() + "'" +
                     ",M = '" + cmyk.Magenta.ToString() + "'" +
                     ",K = '" + cmyk.Black.ToString() + "' where ID=" + id;

                //IWorkspaceFactory awf = new AccessWorkspaceFactory();
                //var ws = awf.OpenFromFile(mdbpath, 0);
                //ws.ExecuteSQL(sql);

            }

        }
        private void cmbColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            var mdbpath = GApplication.Application.Template.Root + @"\专家库\Colors.mdb";
            string tableName = "色带填色";
            LoadSDMColors(mdbpath, tableName);
            String type = cmbColor.SelectedItem.ToString();
            DrawColorBt(type, this.ColorPanel);
        }
        #endregion
        private void FrmSDMSet_Load(object sender, EventArgs e)
        {
           
        }
        public Dictionary<string, ICmykColor> SDMColors = new Dictionary<string, ICmykColor>();
        public double SDMDis = 2.5;
        public void AutoSet()
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

            if (!double.TryParse(SDMWidth.Text, out SDMDis) || SDMDis < 0)
            {
                MessageBox.Show("请输入正确的色带宽度数值!");
                return;
            }
        }
        private void btOK_Click(object sender, EventArgs e)
        {
            #region 色带
            foreach (Control control in ColorPanel.Controls)
            {
                if (control is Panel)
                {
                    ICmykColor color = ColorHelper.ConvertColorToCMYK(control.BackColor);
                    string type = ((control as Panel).Tag as BOUAColor).ColorIndex;
                    SDMColors[type] = color;
                }


            }

            if (!double.TryParse(SDMWidth.Text, out SDMDis) || SDMDis < 0)
            {
                MessageBox.Show("请输入正确的色带宽度数值!");
                return;
            }
            DialogResult = DialogResult.OK;
            #endregion
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
