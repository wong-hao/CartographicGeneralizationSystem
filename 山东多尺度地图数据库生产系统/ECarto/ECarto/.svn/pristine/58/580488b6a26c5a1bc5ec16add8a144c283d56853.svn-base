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
    public partial class FrmColumnClAttribute : Form
    {

        string jsonText = "";
        public string Json = "";
        ColumnJson  columnInfo = null;
        ColumnJson ReColumnInfo = null;
        List<Panel> panels = new List<Panel>();
        Dictionary<string, Dictionary<string, double>> chartDs = null;
        List<ICmykColor> CMYKColors = null;
        public FrmColumnClAttribute(string _jsontext)
        {
            InitializeComponent();
            jsonText = _jsontext;
            columnInfo = JsonHelper.GetColumnInfo(jsonText);
        }

        private void FrmPieAttribute_Load(object sender, EventArgs e)
        {
           
            chartDs = JsonHelper.CHDataSource(columnInfo.DataSource);
            DataGridViewTextBoxColumn txtColumn = new DataGridViewTextBoxColumn();
            txtColumn.HeaderText = "类别";
            txtColumn.Name = "类别";
            this.dataGV_reuslt.Columns.Add(txtColumn);
            foreach (var kv in chartDs)
            {
                txtColumn = new DataGridViewTextBoxColumn();
                txtColumn.HeaderText = kv.Key;
                txtColumn.Name = kv.Key;
                this.dataGV_reuslt.Columns.Add(txtColumn);
            }
          
          
        
           
            Dictionary<string, List<object>> griddata = new Dictionary<string, List<object>>();
            foreach (var kv in chartDs)
            {
                
                foreach (var sub in kv.Value)
                {
                    if (griddata.ContainsKey(sub.Key))
                    {
                        var objs = griddata[sub.Key];
                        objs.Add(sub.Value);
                        griddata[sub.Key] = objs;
                    }
                    else
                    {
                        var objs = new List<object>();
                        objs.Add(sub.Value);
                        griddata[sub.Key] = objs;
                    }
                     
                }
            }
            //加载数据到grid
            int i = 0;
            foreach (var kv in griddata)
            {
                var objs = kv.Value;
                object[] dataObj = new object[1 + objs.Count];
                dataObj[0] = kv.Key;
                for (int j = 0; j < objs.Count; j++)
                {
                    dataObj[j + 1] = objs[j];
                }
                dataGV_reuslt.Rows.Insert(i, dataObj);
                dataGV_reuslt.Rows[i].HeaderCell.Value = (++i).ToString();
            }
          
              

             
            TxtSize.Text = columnInfo.Size.ToString();
         
            cbLengend.Checked = columnInfo.LengendShow;
         
            
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
            Label lb = new Label(); lb.AutoSize = true;
            lb.Location = new System.Drawing.Point(step, 125);
            lb.Text = colortype;
            panel.Tag = lb.Text;
            ColorPan.Controls.Add(lb);
            panel.Location = new System.Drawing.Point(step + lb.Size.Width / 2 - panel.Width / 2, 15);
            step += 10 + lb.Size.Width;
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
            foreach (var c in columnInfo.Colors)
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
        
     
        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
                
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            //颜色
            ReColumnInfo = new ColumnJson();
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
      
            var ChartDatas = new Dictionary<string, Dictionary<string, double>>();
         
            for (int c = 1; c < dataGV_reuslt.Columns.Count; c++)
            {
               var dic = new Dictionary<string, double>();
               for (int i = 0; i < dataGV_reuslt.Rows.Count; i++)
               {
               
                    string key = dataGV_reuslt.Rows[i].Cells[0].Value.ToString();
                    string value = dataGV_reuslt.Rows[i].Cells[c].Value.ToString();
                    double re = 0;
                    double.TryParse(value, out re);
                    dic[key] = re;
                }
               string name = dataGV_reuslt.Columns[c].Name;
               ChartDatas[name] = dic;
            }

            ReColumnInfo.Colors = listcolors;
            ReColumnInfo.Size = double.Parse(TxtSize.Text);
            ReColumnInfo.LengendShow = cbLengend.Checked;
            ReColumnInfo.Title = columnInfo.Title;
            ReColumnInfo.GeoNum = columnInfo.GeoNum;
            ReColumnInfo.ThematicType = columnInfo.ThematicType;
            ReColumnInfo.DataSource = JsonHelper.JsonChartData(ChartDatas);
           
            Json = JsonHelper.GetJsonText(ReColumnInfo);
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
