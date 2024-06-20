using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
namespace SMGI.Common 
{
    public partial class BookMarkDialog : Form 
    {
        GApplication app;
        public BookMarkDialog(GApplication app) 
        {
            this.app = app;
            InitializeComponent();
            if (app.Workspace == null)
                return;
            listItem();
        }
        void listItem() 
        {
            BookMarkInfo[] info = app.Workspace.MapConfig["BookMarksEx"] as BookMarkInfo[];
            if (info != null) 
            {
                this.listBox1.Items.AddRange(info);
            }
            
            if (this.listBox1.Items.Count > 0) 
            {
                this.listBox1.SelectedIndex = 0;
            }
        }
        private void btNew_Click(object sender, EventArgs e) 
        {
            BookMarkInfo info = new BookMarkInfo(app.MapControl.ActiveView);
            this.listBox1.Items.Add(info);            
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) 
        {
            
            BookMarkInfo info = listBox1.SelectedItem as BookMarkInfo;
            if (info == null) 
            {
                pictureBox1.Image = null;
                return;
            }
            pictureBox1.Image = info.bitmap;
            pictureBox1.Refresh();
        }

        private void BookMarkDlg_FormClosed(object sender, FormClosedEventArgs e) 
        {
            pictureBox1.Image = null;
            BookMarkInfo[] infos = new BookMarkInfo[listBox1.Items.Count];
            listBox1.Items.CopyTo(infos, 0);
            app.Workspace.MapConfig["BookMarksEx"] = infos;
            foreach (var item in listBox1.Items)
            {
                (item as BookMarkInfo).bitmap.Dispose();
            }
        }

        private void btDelete_Click(object sender, EventArgs e) 
        {
            if (this.listBox1.SelectedIndex == -1) 
            {
                return;
            }            
            List<int> selectIdxs = new List<int>();
            foreach (var item in this.listBox1.SelectedIndices)            
                selectIdxs.Add((int)item);            
            selectIdxs.Sort((x, y) => -x.CompareTo(y));
            foreach (var idx in selectIdxs)            
                this.listBox1.Items.RemoveAt(idx); 
        }

        private void btGoto_Click(object sender, EventArgs e) 
        {
            if (this.listBox1.SelectedIndex == -1)
            {
                return;
            }
            BookMarkInfo info = this.listBox1.SelectedItem as BookMarkInfo;
            app.MapControl.Extent = info.Envelope;
        }

        private void listBox1_DoubleClick(object sender, EventArgs e) 
        {
            btGoto_Click(sender, e);
        }

        private void btRename_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndex == -1)
            {
                return;
            }
            int idx = this.listBox1.SelectedIndex;
            BookMarkInfo info = this.listBox1.SelectedItem as BookMarkInfo;
            InputStringForm f = new InputStringForm();
            f.Value = info.name;
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
            {
                info.name = f.Value;
                this.listBox1.Items.RemoveAt(idx);
                this.listBox1.Items.Insert(idx, info);
                this.listBox1.SelectedIndex = idx;
            }
        }
    }
}

