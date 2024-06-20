using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;

namespace SMGI.Plugin.ThematicChart.ThematicChart.Table
{
    public partial class FrmThematicTableSet : Form
    {
        GApplication app;
        public string dataSource;
        public double markerSize;
        public double outBorderSpacing;//外边框间距
        public string outBorderStyle;//外边框样式
        public IColor outBorderColor = new RgbColorClass();//外边框颜色
        public IColor innerLineColor = new RgbColorClass();//内部分割线颜色
        public double innerLineWidth;                      //内部分割线间距
        public bool isCorner = false;//表格是否在四个角落；
        public string tableLocation;//表格位置，默认锚点
        public bool cusBorder = false;//自定义边框
        public TableBorder tableBorder;//边框结构

        public struct TableBorder
        {
            public bool topBorder;
            public bool rightBorder;
            public bool bottomBorder;
            public bool leftBorder;
        }
        public FrmThematicTableSet()
        {
            app = SMGI.Common.GApplication.Application;
            InitializeComponent();
            cb_borderStyle.SelectedIndex = 0;
            tableLocation = "左上";
            num_outSpacing.Enabled = false;
        }

        private void btok_Click(object sender, EventArgs e)
        {
            if (!isClipGeoOut())
            {
                MessageBox.Show("请先生成图廓线！","提示");
                return;
            }
            if (dataSource == string.Empty || dataSource == null)
            {
                MessageBox.Show("请选择Excel源数据！","提示");
                return;
            }
            markerSize = double.Parse(txtSize.Text);
            if (cb_borderStyle.SelectedItem.ToString().Trim() == "双实线")
            {
                outBorderSpacing = Convert.ToDouble(num_outSpacing.Value);
            }
            else
            {
                outBorderSpacing = 0;
            }
            outBorderStyle = cb_borderStyle.SelectedItem.ToString().Trim();
            innerLineWidth = Convert.ToDouble(num_innerSpacing.Value);
            if (cb_talbelocation.Checked)
            {
                isCorner = true;
            }
            if (panel_border.Enabled)
            {
                cusBorder = true;
                tableBorder = new TableBorder();
                tableBorder.topBorder = cb_top.Checked;
                tableBorder.rightBorder = cb_right.Checked;
                tableBorder.bottomBorder = cb_bottom.Checked;
                tableBorder.leftBorder = cb_left.Checked;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        public string sheetName;
        List <string > sh = new List<string> ();
        private void btOpen_Click(object sender, EventArgs e)
        {
            sh.Clear();
            OpenFileDialog dl = new OpenFileDialog();
            dl.Title = "选择源数据";
            dl.Filter = "所有文件|*.*|2007Excel|*.xlsx|2003Excel|*.xls";
            dl.RestoreDirectory = true;
            dl.FilterIndex = 0;
            if (dl.ShowDialog() == DialogResult.OK)
            {
                txtExcel.Text = dl.FileName;
                dataSource = dl.FileName;
                ExcelGenerator eg = new ExcelGenerator();

                sh = eg.getSheetName(dataSource);
                eg.CloseExcelEx();
                for (int i = 0; i < sh.Count;i++ )
                {
                    cbCount.Items.Add(sh[i]);
                }
                if (cbCount.Items.Count > 0)
                {
                    cbCount.SelectedIndex = 0;
                }
                //sheetName = cbCount.SelectedItem.ToString().Trim();
            }
        }

        private void cbCount_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                sheetName = cbCount.SelectedItem.ToString().Trim();
            }
            catch
            {
                return;
            }
        }

        private void btn_borderColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                btn_borderColor.BackColor = colorDialog1.Color;
                outBorderColor.RGB = colorDialog1.Color.B * 65536 + colorDialog1.Color.G * 256 + colorDialog1.Color.R;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                btn_innerLineColor.BackColor = colorDialog1.Color;
                innerLineColor.RGB = colorDialog1.Color.B * 65536 + colorDialog1.Color.G * 256 + colorDialog1.Color.R;
            }
        }

        private void cb_borderStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            num_outSpacing.Enabled = false;
            panel_border.Enabled = false;
            if (cb_borderStyle.SelectedItem.ToString().Trim() == "双实线")
            {
                num_outSpacing.Enabled = true;
            }
            if(cb_borderStyle.SelectedItem.ToString().Trim() == "双实线" && !cb_talbelocation.Checked)
            {
                panel_border.Enabled = true;
            }
        }

        private void cb_talbelocation_CheckedChanged(object sender, EventArgs e)
        {
            panel_border.Enabled = false;
            if (cb_talbelocation.Checked == true)
            {
                gb_tablelocation.Enabled = true;
            }
            else
            {
                gb_tablelocation.Enabled = false;
            }
            if (cb_borderStyle.SelectedItem.ToString().Trim() == "双实线" && !cb_talbelocation.Checked)
            {
                panel_border.Enabled = true;
            }
        }

        private void rbtn_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = (sender as RadioButton);
            if (rb.Checked)
            {
                tableLocation = rb.Text;
            }
        }


        private bool isClipGeoOut()
        {
            var lyr = app.Workspace.LayerManager.GetLayer(l => (l is IFeatureLayer) &&(l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == "LLINE").FirstOrDefault();
          
            IQueryFilter qf = new QueryFilterClass();

            IFeature fe;
            IFeatureCursor cursor = null;
            //图名
            qf.WhereClause = "TYPE = '内图廓'";
            cursor = (lyr as IFeatureLayer).Search(qf, false);

            fe = cursor.NextFeature();
            if (fe != null)
            {
                return true;
            }
            return false;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            borderHelp borderhelp = new borderHelp();
            borderhelp.Show();
        }
    }
}
