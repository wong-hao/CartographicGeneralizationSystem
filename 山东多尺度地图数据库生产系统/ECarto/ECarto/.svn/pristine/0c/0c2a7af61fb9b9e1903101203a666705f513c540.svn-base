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
    public partial class FrmBOUASet : Form
    {
        public FrmBOUASet()
        {
            InitializeComponent();
            LoadBOUAColors();
            foreach (var kv in listsColorDic)
            {
                cmbColorBOUA.Items.Add(kv.Key);
            }
            cmbColorBOUA.SelectedIndex = 0;
            //附区  gap
            {
                BOUAAtColor = new CmykColorClass();
                (BOUAAtColor as ICmykColor).Cyan = 0;
                (BOUAAtColor as ICmykColor).Magenta = 0;
                (BOUAAtColor as ICmykColor).Yellow = 0;
                (BOUAAtColor as ICmykColor).Black = 0;
            }
            cbLevel.Items.Add("省级行政区普色");
            cbLevel.Items.Add("地市级行政区普色");
            cbLevel.Items.Add("区县级行政区普色");
            cbLevel.Items.Add("乡镇级行政区普色");
            cbLevel.SelectedIndex = 3;
            AutoSet();
        }
        #region 境界普色设置
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
            var mainPanel = (sender as Panel).Parent as Panel;
            foreach (Control c in mainPanel.Controls)
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
            var mainPanel = (sender as Panel).Parent as Panel;
            string tableName = "色带填色";
            var mdbpath = GApplication.Application.Template.Root + @"\专家库\Colors.mdb";
            foreach (Control c in mainPanel.Controls)
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
      
        private void cmbColorBOUA_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadBOUAColors();
            String type = this.cmbColorBOUA.SelectedItem.ToString();
            DrawColorBt(type, this.ColorPanelBOUA);
        }
        private void LoadBOUAColors()
        {

            string mdbpath = GApplication.Application.Template.Root + @"\专家库\Colors.mdb"; ;
            string tableName = "境界填色";
            ClearControls(ColorPanelBOUA);
            listsColorDic.Clear();
            DataTable dt = CommonMethods.ReadToDataTable(mdbpath, tableName);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];

                string planName = "方案" + dr["填色方案"].ToString();
                BOUAColor b = new BOUAColor();
                b.FID = int.Parse(dr["ID"].ToString());
                b.ColorPlan = planName;
                b.ColorIndex = dr["优先顺序"].ToString();

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
        #endregion

        public IColor BOUAAtColor = null;
        public List<ICmykColor> BOUAColors = new List<ICmykColor>();
        public string BouaLyr = "BOUA6";
        private void FrmBOUASet_Load(object sender, EventArgs e)
        {
          
        }
        public void AutoSet()
        {
            foreach (Control control in this.ColorPanelBOUA.Controls)
            {
                if (control is Panel)
                {
                    ICmykColor color = ColorHelper.ConvertColorToCMYK(control.BackColor);
                    BOUAColors.Add(color);
                }


            }
            BOUAAtColor = ColorHelper.ConvertColorToIColor(AttachColor.BackColor);
        }
        private void btOK_Click(object sender, EventArgs e)
        {
            #region 境界普色
            BOUAColors.Clear();
            foreach (Control control in this.ColorPanelBOUA.Controls)
            {
                if (control is Panel)
                {
                    ICmykColor color = ColorHelper.ConvertColorToCMYK(control.BackColor);
                    BOUAColors.Add(color);
                }


            }
            BOUAAtColor = ColorHelper.ConvertColorToIColor(AttachColor.BackColor);
            //设置普色级别
            switch (cbLevel.SelectedIndex)
            {
                case 0:
                    BouaLyr = "BOUA2";
                    break;
                case 1:
                    BouaLyr = "BOUA4";
                    break;
                case 2:
                    BouaLyr = "BOUA5";
                    break;
                case 3:
                    BouaLyr = "BOUA6";
                    break;
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
