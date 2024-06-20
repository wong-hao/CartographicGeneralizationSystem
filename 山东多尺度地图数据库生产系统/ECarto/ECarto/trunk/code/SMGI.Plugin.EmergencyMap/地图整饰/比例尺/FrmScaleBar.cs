using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Display;
using System.IO;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmScaleBar : Form
    {
        GApplication app;
        public string ScalebarLocation
        {
            get
            {
                string scaleLocation = "";
                foreach (var ctrl in this.gbLocation.Controls)
                {
                    if (ctrl is RadioButton)
                    {
                        RadioButton rb = ctrl as RadioButton;
                        if (rb.Checked)
                        {
                            scaleLocation = rb.Text;
                            break;
                        }
                    }
                }
                return scaleLocation;
            }
            set
            {
                foreach (var ctrl in this.gbLocation.Controls)
                {
                    if (ctrl is RadioButton)
                    {
                        RadioButton rb = ctrl as RadioButton;
                        if (rb.Text==value)
                        {
                            rb.Checked = true;
                            break;
                        }
                    }
                }
            }
        }
        public string ScaleUnit
        {
            get
            {
                return cbUnit.SelectedItem.ToString().Split(' ')[0]; ;
            }
            set
            {
                cbUnit.SelectedIndex = cbUnit.Items.IndexOf(value);
            }
        }
        public string ScaleBarName
        {
            get
            {
                if (listView1.SelectedItems.Count == 0)
                    return listView1.Items[0].Text;
                return listView1.SelectedItems[0].Text;
            }
            set
            {
                foreach (ListViewItem lv in listView1.Items)
                {
                    lv.Selected = false;
                    lv.BackColor = SystemColors.Window;
                    lv.ForeColor = Color.Black;
                    if (lv.Text == value)
                    {
                        lv.Selected = true;
                        lv.BackColor = SystemColors.MenuHighlight;
                        lv.ForeColor = Color.White;
                    }
                }
                
            }
        }
        public List<string> Notebar
        {
            get
            {
                return rtbNotes.Lines.ToList<string>();
            }
            set
            {
                foreach (var v in value)
                {
                    rtbNotes.AppendText(v);
                }
            }
        }
        public double ScalebarFont
        {
            get
            {
                return double.Parse(txtFontsize.Text);
            }
            set
            {
                txtFontsize.Text = value.ToString();
            }
        }
        bool oneKey = false;
        public FrmScaleBar(bool oneKey_ = false)
        {
            InitializeComponent();
            app = SMGI.Common.GApplication.Application;
            oneKey = oneKey_;
            IntiParms();
          
        }
        private void IntiParms()
        {
            string mdbPath = app.Template.Root + "\\整饰\\比例尺";
            DirectoryInfo di = new DirectoryInfo(mdbPath);
            List<string> imgname = new List<string>();
            foreach (FileInfo fi in di.GetFiles())
            {
                if (fi.Name.ToUpper().EndsWith(".BMP") || fi.Name.ToUpper().EndsWith(".PNG"))
                {
                    imageList1.Images.Add(Image.FromFile(fi.FullName));
                    string[] temp = fi.Name.Split('.');
                    imgname.Add(temp[0]);
                }
            }

            for (int i = 0; i < imgname.Count; i++)
            {
                ListViewItem lvi = new ListViewItem(imgname[i]);
                lvi.ImageIndex = i;
                listView1.Items.Add(lvi);
            }
            listView1.Items[listView1.Items.Count - 1].Selected = true;
            //比例尺位置
            this.rbInnerLegend.Checked = true;
            if (app.Workspace != null)
            {
                var layer = app.Workspace.LayerManager.GetLayer(l => { return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == "LPOLY"); }).First();
                if (layer == null)
                {
                    MessageBox.Show("不存在LPOLY图层！");
                    return;
                }
                IFeatureClass polygonfcl = (layer as IFeatureLayer).FeatureClass;
                IQueryFilter qfle = new QueryFilterClass();
                qfle.WhereClause = "TYPE='图例内边线'";
                IFeatureCursor cursorle = polygonfcl.Search(qfle, false);
                IFeature linele = cursorle.NextFeature();
                if (linele != null)
                {
                    this.rbInnerLegend.Checked = true;
                }
                else
                {
                    this.rbRigthDown.Checked = true;
                }
            }
            //比例尺标注单位
            double scale = app.MapControl.ReferenceScale;
            if (scale >= 100000)
            {
                this.cbUnit.SelectedIndex = 1;
            }
            else
            {
                this.cbUnit.SelectedIndex = 0;
            }
        }
        private void btOk_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;
            if (oneKey)
            {
                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
                return;
            }
            app.EngineEditor.StartOperation();
            string compassName = listView1.SelectedItems[0].Text;
            string GDBname = "SCALE";
            string scaleLocation="";
            foreach (var ctrl in this.gbLocation.Controls)
            {
                if (ctrl is RadioButton)
                {
                    RadioButton rb = ctrl as RadioButton;
                    if (rb.Checked)
                    {
                        scaleLocation = rb.Text;
                        break;
                    }
                }
            }
            string scaleUnit = cbUnit.SelectedItem.ToString();
            MapLayoutHelperLH mh = new MapLayoutHelperLH(GDBname);
            mh.ScaleLoaction = scaleLocation;
            mh.ScaleUnit = scaleUnit;
            mh.CreateScaleBar(compassName, GDBname, rtbNotes.Lines.ToList<string>(), double.Parse(txtFontsize.Text));
            app.EngineEditor.StopOperation("比例尺设置");
            Close();
            GC.Collect();
        }

        private void FrmScaleBar_Load(object sender, EventArgs e)
        {
           
        }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem lv in listView1.Items)
            {
                lv.BackColor = SystemColors.Window;
                lv.ForeColor = Color.Black;
            }
            foreach (ListViewItem lv in listView1.SelectedItems)
            {
                lv.BackColor = SystemColors.MenuHighlight;
                lv.ForeColor = Color.White;
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
