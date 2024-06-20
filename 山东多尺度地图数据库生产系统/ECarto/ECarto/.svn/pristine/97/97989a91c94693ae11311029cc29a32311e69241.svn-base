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
using System.Runtime.Serialization.Json; 
namespace SMGI.Plugin.ThematicChart.ChartAttrs
{
    public partial class FrmPieAttribute : Form
    {

        string jsonText = "";
        public string Json = "";
        PieJson pieInfo = null;
        PieJson RePieInfo = null;
        List<Panel> panels = new List<Panel>();
        Dictionary<string, Dictionary<string, double>> chartDs = null;
        List<ICmykColor> CMYKColors = null;
        public FrmPieAttribute(string _jsontext)
        {
            InitializeComponent();
            jsonText = _jsontext;
            pieInfo= JsonHelper.GetPieInfo(jsonText);
        }

        private void FrmPieAttribute_Load(object sender, EventArgs e)
        {
            DataGridViewTextBoxColumn txtColumn = new DataGridViewTextBoxColumn();
            txtColumn.HeaderText = "类别";
            txtColumn.Name = "类别";
            this.dataGV_reuslt.Columns.Add(txtColumn);
            chartDs = JsonHelper.CHDataSource(pieInfo.DataSource);
            txtColumn = new DataGridViewTextBoxColumn();
            txtColumn.HeaderText = chartDs.First().Key;
            txtColumn.Name = chartDs.First().Key;
            this.dataGV_reuslt.Columns.Add(txtColumn);
            object[] dataObj = new object[2];
            Dictionary<string, double> dic = chartDs.First().Value;
            int i = 0;
            foreach (var kv in dic)
            {
                dataObj[0] = kv.Key;
                dataObj[1] = kv.Value;
                dataGV_reuslt.Rows.Insert(i, dataObj);
                dataGV_reuslt.Rows[i].HeaderCell.Value = (++i).ToString();

            }
            TxtSize.Text = pieInfo.Size.ToString();
            Title.Text = pieInfo.Title.ToString();

            EllipseRate.Text = pieInfo.EllipseRate.ToString();
            RingRate.Text = pieInfo.RingRate.ToString();
            TotalStatic.Checked =  pieInfo.TotalLable;
            for (int t = 0; t < lbCmb.Items.Count; t++)
            {
                if (lbCmb.Items[t].ToString() == pieInfo.LabelInfo)
                {
                    lbCmb.SelectedIndex = t;
                    break;
                }
            }
            if(pieInfo.LabelInfo==null)
                lbCmb.SelectedIndex = 3;
            if (pieInfo.LabelInfo == "")
                lbCmb.SelectedIndex = 3;
            CreateColors();
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
        int step = 10;
        private void DrawColor(int i, Color co)
        {
            if (i == 0)
            {
                step = 10;
            }
            Panel panel = new Panel();
          
            //卸载
            panel.MouseDoubleClick -= new MouseEventHandler(panel_MouseDoubleClick);
            //添加事件
            panel.MouseDoubleClick += new MouseEventHandler(panel_MouseDoubleClick);
            panel.Width = 25;
            panel.Height = 70;
           
            panel.BackColor = co;
            ColorPan.Controls.Add(panel);
            if (panels.Count < chartDs.First().Value.Count)
            {
                panels.Add(panel);
            }
            string colortype = chartDs.First().Value.Keys.ToArray()[i];
            Label lb = new Label();
            lb.Location = new System.Drawing.Point(step, 125);
            lb.Text = colortype;
            panel.Tag = lb.Text;
            ColorPan.Controls.Add(lb);
            
        }
        private void Clearlb()
        {
            ColorPan.Controls.Clear();
          
        }
        private void CreateColors()
        {
            Clearlb();
           
            List<ICmykColor> cmykColors = new List<ICmykColor>();
            int i = 0;
            foreach (var c in pieInfo.Colors)
            {
                var cmyk = new CmykColorClass();
                cmyk.Cyan = c.C;
                cmyk.Yellow = c.Y;
                cmyk.Magenta = c.M;
                cmyk.Black = c.K;
                cmykColors.Add(cmyk);

                Color pcolor = ColorHelper.ConvertIColorToColor(cmyk);
                DrawColor(i, pcolor);
                i++;
            }
                
            

        }
        private void GetBgColor()
        {
            CMYKColors = new List<ICmykColor>();
            foreach (var panel in panels)
            {
                IColor pc =ColorHelper.ConvertColorToIColor(panel.BackColor);
                ICmykColor pcolor = ColorHelper.ConvertRGBToCMYK(pc as IRgbColor);
                CMYKColors.Add(pcolor);

            }
        }
        
        private void txtEllipseRate_TextChanged(object sender, EventArgs e)
        {

        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
                
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            //颜色
            RePieInfo = new PieJson();
            //颜色
            GetBgColor();
            var listcolors = new List<ThematicColor>();
            for (int i = 0; i < CMYKColors.Count; i++)
            {
                var cmyk = CMYKColors[i];
                ThematicColor tc = new ThematicColor { C = cmyk.Cyan, Y = cmyk.Yellow, M = cmyk.Magenta,K=cmyk.Black };
                tc.ColorName = panels[i].Tag.ToString();
                listcolors.Add(tc);
            }
            RePieInfo.Colors = listcolors;
            RePieInfo.Title = Title.Text.Trim();
            RePieInfo.RingRate =double.Parse(RingRate.Text);
            RePieInfo.EllipseRate = double.Parse(EllipseRate.Text);
            var ChartDatas = new Dictionary<string, Dictionary<string, double>>();
            var dic = new Dictionary<string, double>();
            for (int i = 0; i < dataGV_reuslt.Rows.Count; i++)
            {
               string key=  dataGV_reuslt.Rows[i].Cells[0].Value.ToString();
               string value = dataGV_reuslt.Rows[i].Cells[1].Value.ToString();
               double re = 0;
               double.TryParse(value, out re);
               dic[key] = re;
            }
            ChartDatas[chartDs.First().Key] = dic;
            RePieInfo.ThematicType = pieInfo.ThematicType;
            RePieInfo.DataSource = JsonHelper.JsonChartData(ChartDatas);
            RePieInfo.LabelInfo = lbCmb.SelectedItem.ToString();
            RePieInfo.TotalLable = TotalStatic.Checked;
            RePieInfo.Size = double.Parse(TxtSize.Text);
            Json = JsonHelper.GetJsonText(RePieInfo);
            if (Json != jsonText)
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
            
        }

    }
}
