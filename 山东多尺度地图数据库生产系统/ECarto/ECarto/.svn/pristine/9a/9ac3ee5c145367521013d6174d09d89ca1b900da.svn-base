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
namespace SMGI.Plugin.EmergencyMap
{
    public partial class ColorAddForm : Form
    {
        string mdbpath = "";
        string tableName = "";
        public ColorAddForm(int num = 0, string name = "境界填色")
        {
            InitializeComponent();
            Plan.Value = num+1;
            tableName = name;
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btOK_Click(object sender, EventArgs e)
        {

            //添加到数据库
            IWorkspaceFactory pWorkspaceFactory = new AccessWorkspaceFactory();
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(mdbpath, 0);

            ITable ptable = (pWorkspace as IFeatureWorkspace).OpenTable(tableName);
            if (Plan.Value == 0)
            {
                MessageBox.Show("数字不能为0！");
                return;
            }
            int ct = GetColorsPanel();
            if (ct < 4&&tableName=="境界填色")
            {
                MessageBox.Show("方案颜色至少为4个！");
                return;
            }
            if (ct < 3 && tableName == "邻区境界填色")
            {
                MessageBox.Show("方案颜色至少为3个！");
                return;
            }
            int i=0;
            foreach (Control control in ColorPanel.Controls)
            {
                if (control is Panel)
                {
                    ICmykColor color = ColorHelper.ConvertColorToCMYK((control as Panel).BackColor);
                    IRow r = ptable.CreateRow();
                    int c = color.Cyan;
                    
                    int m=color.Magenta;

                    int y = color.Yellow;

                    int k = color.Black ;
                 
                    i++;
                    r.set_Value(ptable.FindField("填色方案"), Plan.Value);
                    r.set_Value(ptable.FindField("C"), c);
                    r.set_Value(ptable.FindField("M"), m);
                    r.set_Value(ptable.FindField("Y"), y);
                    r.set_Value(ptable.FindField("K"), k);
                    r.set_Value(ptable.FindField("优先顺序"), (i));
                    if (tableName == "邻区境界填色")
                    {
                        r.set_Value(ptable.FindField("TYPE"), (control.Tag as Label).Text);
                        
                    }
                    r.Store();
                }

            }
            MessageBox.Show("添加成功！");
            DialogResult = DialogResult.OK;
        }
        Dictionary<int, string> nameDic = new Dictionary<int, string>();
        private void FrmColorAdd_Load(object sender, EventArgs e)
        {
            mdbpath = GApplication.Application.Template.Root + @"\专家库\Colors.mdb";
            nameDic[0] = "省内";
            nameDic[1] = "省外";
            nameDic[2] = "国外";
            if (tableName == "境界填色")
            {
                lbInfo.Visible = false;
            }
        }
        private int  GetColorsPanel()
        {
            int i = 0;
            foreach (Control control in ColorPanel.Controls)
            {
                if (control is Panel)
                {
                    i++;
                } 

            }
            return i;
        }
        
        private void BtAdd_Click(object sender, EventArgs e)
        {
            
            int x = GetColorsPanel();
            if (x == 3 && tableName == "邻区境界填色")
                return;
            int c;
            int.TryParse(C.Text, out c);
            int m;
            int.TryParse(M.Text, out m);
            int y;
            int.TryParse(Y.Text, out y);
            int k;
            int.TryParse(K.Text, out k);
            CmykColorClass cmykColor = new CmykColorClass();
            cmykColor.Cyan = c;
            cmykColor.Magenta = m;
            cmykColor.Yellow = y;
            cmykColor.Black = k;
            Color pcolor = ColorHelper.ConvertICMYKColorToColor(cmykColor);
            Panel panel = new Panel();
            panel.Width = 35;
            panel.Height = 30;
            panel.Location = new Point(45 + x * 55, 25);
            panel.BackColor = pcolor;
          

            Label lb = new Label();
            lb.Top = 75 ;
            lb.Left = 45 + x * 55;
            lb.AutoSize = true;
            lb.Text = ("颜色:" + (x+1)).Trim();
            if (tableName == "邻区境界填色")
            {
                lb.Text = nameDic[x];
            }
            panel.Tag = lb;
            ColorPanel.Controls.Add(lb);
           
            ColorPanel.Controls.Add(panel);
        

            panel.MouseClick += new MouseEventHandler(panel_MouseClick);
            panel.MouseDoubleClick += new MouseEventHandler(panel_MouseDoubleClick);
         
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
            }

        }

        Panel PanelSe = null;
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

        private void btRemove_Click(object sender, EventArgs e)
        {
            if (PanelSe != null)
            {
               
                ColorPanel.Controls.Remove(PanelSe);
                ReLoadPanel();
                Label LB = PanelSe.Tag as Label;
                ColorPanel.Controls.Remove(LB);
              
            }

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
        private void ReLoadPanel()
        {
            List<Panel> panels = new List<Panel>();
            foreach (Control c in ColorPanel.Controls)
            {
                if (c.GetType() == typeof(System.Windows.Forms.Panel))
                {
                    panels.Add(c as Panel);
                }
            }
            ClearControls();
            for (int i = 0; i < panels.Count; i++)
            {
                Label lb = new Label();
                Panel panel = panels[i];               
                lb.Top = 75;
                lb.Left = 45 + i * 55;
                lb.AutoSize = true;
                lb.Text = (panel.Tag as Label).Text;
                ColorPanel.Controls.Add(lb);
                panel.Tag = lb;
                ColorPanel.Controls.Add(panel);
            }
        }
    }
}
