using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;

namespace SMGI.Common
{
    public partial class SelectFeatureSetControl : UserControl
    {

        private SelectByAttributesDialog sba;

        public List<SelectFeatureSet> FeatureSets
        {
            get;
            private set;
        }

        public bool PointLayer { 
            get { return sba.PointLayer; }
            set { sba.PointLayer = value; }
        }
        public bool LineLayer
        {
            get { return sba.LineLayer; }
            set { sba.LineLayer = value; }
        }
        public bool AreaLayer
        {
            get { return sba.AreaLayer; }
            set { sba.AreaLayer = value; }
        }

        /*
        public List<string> GetFeatureSets()
        {
            List<string> featuresets = new List<string>();
            if (this.listViewdata.Items.Count > 0)
            {
                foreach (ListViewItem lvi in this.listViewdata.Items)
                {
                    featuresets.Add(lvi.Text);
                }
            }
            return featuresets;
        }
         * */

        public SelectFeatureSetControl(GApplication app)
        {
            InitializeComponent();

            sba = new SelectByAttributesDialog(app);
            sba.ApplyButtomClicked += new EventHandler(sba_ApplyButtomClicked);
            FeatureSets = new List<SelectFeatureSet>();

            ColumnHeader ch = new ColumnHeader();
            ch.Text = "featureinfo";
            ch.Width = this.listViewdata.Width - 4;
            ch.TextAlign = HorizontalAlignment.Left;
            this.listViewdata.Columns.Add(ch);
        }

        private void addfeature_Click(object sender, EventArgs e)
        {
            sba.ShowDialog();
            if (sba.DialogResult == DialogResult.OK)
            {
                SelectFeatureSet sfs = new SelectFeatureSet(sba.LayerSelected, sba.SQLCondition);
                bool isexist = false;
                foreach (var one in FeatureSets)
                {
                    if (sfs.ToString() == one.ToString())
                    {
                        isexist = true;
                        break;
                    }
                }

                if (!isexist)
                {
                    FeatureSets.Add(sfs);

                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = sfs.ToString();
                    this.listViewdata.BeginUpdate();
                    this.listViewdata.Items.Add(lvi);
                    this.listViewdata.EndUpdate();
                }
            }
            return;
        }

        void sba_ApplyButtomClicked(object sender, EventArgs e)
        {
            SelectFeatureSet sfs = new SelectFeatureSet(sba.LayerSelected, sba.SQLCondition);
            bool isexist = false;
            foreach (var one in FeatureSets)
            {
                if (sfs.ToString() == one.ToString())
                {
                    isexist = true;
                    break;
                }
            }
            if (!isexist)
            {
                FeatureSets.Add(sfs);

                ListViewItem lvi = new ListViewItem();
                lvi.Text = sfs.ToString();
                this.listViewdata.BeginUpdate();
                this.listViewdata.Items.Add(lvi);
                this.listViewdata.EndUpdate();
            }
            /*
            sfs.ToString();
            ListViewItem lvi = new ListViewItem();
            lvi.Text = sba.LayerSelected.LayerName + "(" + sba.SQLCondition + ")";
            bool isexist = false;
            foreach (ListViewItem str in this.listViewdata.Items)
            {
                if (str.Text == lvi.Text)
                {
                    isexist = true;
                    break;
                }
            }
            if (!isexist)
            {
                this.listViewdata.BeginUpdate();
                this.listViewdata.Items.Add(lvi);
                this.listViewdata.EndUpdate();
            }
             * */
        }

        private void delfeature_Click(object sender, EventArgs e)
        {
            if (this.listViewdata.SelectedItems.Count > 0)
            {
                int index = 0;
                for (int i = this.listViewdata.SelectedItems.Count - 1; i >= 0; i--)
                {
                    ListViewItem item = this.listViewdata.SelectedItems[i];
                    if (i == 0)
                    {
                        index = item.Index;
                    }
                    FeatureSets.RemoveAt(item.Index);
                    this.listViewdata.Items.Remove(item);
                }
                if (this.listViewdata.Items.Count > 0)
                {
                    if (index > 0)
                    {
                        this.listViewdata.Items[index - 1].Selected = true;
                    }
                    else
                    {
                        this.listViewdata.Items[0].Selected = true;
                    }
                }
                this.listViewdata.Focus();
            }
        }

        private void btClear_Click(object sender, EventArgs e)
        {
            this.listViewdata.Items.Clear();
            this.FeatureSets.Clear();
        }
    }

    public class SelectFeatureSet
    {
        public ILayer Layer { get; set; }
        public string SQLCondition { get; set; }
        public SelectFeatureSet(ILayer li, string sql)
        {
            Layer = li;
            SQLCondition = sql;
        }

        public override string ToString()
        {
            return string.Format("{0}({1})", Layer.Name, SQLCondition); 
        }
    }
}
