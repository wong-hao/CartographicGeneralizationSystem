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
    public partial class FrmCompassSet : Form
    {
        GApplication app;
        string compassName;

        public string CompassName
        {
            get { return compassName; }
            set {
                compassName = value; 
                foreach (ListViewItem lv in listView1.Items)
                {
                    lv.BackColor = SystemColors.Window;
                    lv.ForeColor = Color.Black;
                    if(lv.Text==compassName)
                    {
                        lv.Selected = true;
                    }
                }
                foreach (ListViewItem lv in listView1.SelectedItems)
                {
                    lv.BackColor = SystemColors.MenuHighlight;
                    lv.ForeColor = Color.White;
                }
            }
        }
        double compassSize;

        public double CompassSize
        {
            get { return compassSize; }
            set { 
                compassSize = value;
                tbSize.Text = compassSize.ToString();   
            }
        }
        string anchorLocation = "左上";

        public string AnchorLocation
        {
            get { return anchorLocation; }
            set { 
                anchorLocation = value;
                foreach (Control c in gbLocation.Controls)
                {
                    if (c is RadioButton)
                    {
                        RadioButton rb = c as RadioButton;
                        if (rb.Text == anchorLocation)
                        {
                            rb.Checked = true;
                        }
                    }
                }
            }
        }
        bool oneKey = false;
        public FrmCompassSet(bool oneKey_ = false)
        {
            InitializeComponent();
            app = SMGI.Common.GApplication.Application;
            radioButton3.Checked = true;
            oneKey = oneKey_;
            IntiParms();
        }
        private void IntiParms()
        {
            string mdbPath = app.Template.Root + "\\整饰\\指北针";
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
            listView1.Items[0].Selected = true;


            var paramContent = EnvironmentSettings.getContentElement(app);
            var pagesize = paramContent.Element("PageSize");//页面大小
            double w = double.Parse(pagesize.Element("Width").Value);
            double h = double.Parse(pagesize.Element("Height").Value);
            double size = w < h ? w : h;
            size = (int)(size * 0.01) * 5.0;//默认纸张最小边长的20分之一
            tbSize.Text = size.ToString();
            compassName = listView1.Items[0].Text;
            compassSize = double.Parse(tbSize.Text);
        }
        private void FrmCompassSet_Load(object sender, EventArgs e)
        {
            
        }
      
        private void btOk_Click(object sender, EventArgs e)
        {
           #region 输入验证
            ValidateUtil valid = new ValidateUtil();
            if (!valid.TraversalTextBox(this.Controls))
            {
                return;
            }
            #endregion

            if (!oneKey && !isClipGeoOut())
            {
                MessageBox.Show("请先生成图廓线！");
                return;
            }

            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择指北针类型");
                return;
            }
            compassName = listView1.SelectedItems[0].Text;
            compassSize = double.Parse(tbSize.Text);
            DialogResult = System.Windows.Forms.DialogResult.OK;
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
            this.Close();
        }

        private bool isClipGeoOut()
        {
            if (app.Workspace == null)
            {
                return false;
            }
            var lyr = app.Workspace.LayerManager.GetLayer(l => (l is IFeatureLayer) && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == "LLINE").FirstOrDefault();

            IQueryFilter qf = new QueryFilterClass();

            IFeature fe;
            IFeatureCursor cursor = null;
            //图名
            qf.WhereClause = "TYPE = '内图廓'";
            cursor = (lyr as IFeatureLayer).Search(qf, false);
            fe = cursor.NextFeature();
            Marshal.ReleaseComObject(cursor);
            if (fe != null)
            {
                Marshal.ReleaseComObject(fe);
                return true;
            }
            return false;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb.Checked)
            {
                anchorLocation = rb.Text;
            }
        }
        
    }
}
