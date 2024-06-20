using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using SMGI.Common;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
namespace SMGI.Plugin.EmergencyMap.GX
{
    public partial class FrmSDMColor : Form
    {
        public Color SelectColor;
        private Panel PanelSe = null;
        string mdbpath = "";
        string tableName = "色带填色";
        private Dictionary<string, HsvColorClass> HsvColors = new Dictionary<string,HsvColorClass>();
     
        public FrmSDMColor(string type)
        {
            InitializeComponent();
            txtColorPlan.Text = type;
           
        }
        private void ClearControls()
        {
            foreach (Control control in ColorPanel.Controls)
            {

                ColorPanel.Controls.Remove(control);
                
            }
            if (ColorPanel.Controls.Count > 0)
                ClearControls();
        }
         
        private void FrmColor_Load(object sender, EventArgs e)
        {
            mdbpath = GApplication.Application.Template.Root + @"\专家库\Colors.mdb";
          
            LoadColors();
            
        }
        private void LoadColors()
        {
            ClearControls();
            DataTable dt = CommonMethods.ReadToDataTable(mdbpath, tableName);
            Dictionary<string, List<BOUAColor>> listsColorDic = new Dictionary<string, List<BOUAColor>>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                if (dr["色带方案"].ToString() == "")
                    continue;
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
            int row = 0;
            foreach (var list in listsColorDic)
            {
                int colomn = 0;

                Label lbinfo = new Label();
                lbinfo.Top = 25 + 55 * (row);
                lbinfo.Left = 5;
                lbinfo.AutoSize = true;
                lbinfo.Text = list.Key;
                ColorPanel.Controls.Add(lbinfo);
              //  var listOrder = list.Value.OrderBy(t => int.Parse(t.ColorIndex));
                foreach (var l in list.Value)
                {

                    int x = colomn++;
                    int y = row;

                    Color pcolor = ColorHelper.ConvertICMYKColorToColor(l.CmykColor);
                    Panel panel = new Panel();
                    panel.Width = 35;
                    panel.Tag = l;
                    panel.Height = 30;
                    panel.Location = new Point(45 + x * 55, 15 + 55 * y);
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
                row++;
            }
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
            PanelSe = (sender as Panel);
        }

       
        public IColor ConvertColorToIColor(Color p_Color)
        {
            IColor pColor = new RgbColorClass { RGB = p_Color.B * 65536 + p_Color.G * 256 + p_Color.R };
            return pColor;
        }
        private Color ConvertIColorToColor(IColor pRgbColor)
        {
            return ColorTranslator.FromOle(pRgbColor.RGB);
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            
            DialogResult = DialogResult.OK;
        }

        private void btAdd_Click(object sender, EventArgs e)
        {
            FrmSDMColorAdd frm = new FrmSDMColorAdd();
            frm.StartPosition = FormStartPosition.CenterParent;
            DialogResult dr= frm.ShowDialog();
            if (dr == DialogResult.OK)
            {
                LoadColors();
            }
        }

        private void btDelete_Click(object sender, EventArgs e)
        {
            IWorkspaceFactory pWorkspaceFactory = new AccessWorkspaceFactory();
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(mdbpath, 0);

            ITable ptable = (pWorkspace as IFeatureWorkspace).OpenTable(tableName);
            IQueryFilter qf=new QueryFilterClass();

            qf.WhereClause = "ID=" + (PanelSe.Tag as BOUAColor).FID.ToString();
            ptable.DeleteSearchedRows(qf);
            LoadColors();
        }

    }
    
   
}
