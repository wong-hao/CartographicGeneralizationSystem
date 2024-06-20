using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BuildingGen {
    public partial class BookMarkDlg : Form {
        GApplication app;
        public BookMarkDlg(GApplication app) {
            this.app = app;
            InitializeComponent();
            if (app.Workspace == null)
                return;
            listItem();
        }
        void listItem() {
            BookMarkInfo[] info = app.Workspace.MapConfig["BookMarks"] as BookMarkInfo[];
            if (info != null) {
                this.listBox1.Items.AddRange(info);
            }
            
            if (this.listBox1.Items.Count > 0) {
                this.listBox1.SelectedIndex = 0;
            }
        }
        private void btNew_Click(object sender, EventArgs e) {
            BookMarkInfo info = new BookMarkInfo(app.MapControl.ActiveView);
            this.listBox1.Items.Add(info);            
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            
            BookMarkInfo info = listBox1.SelectedItem as BookMarkInfo;
            if (info == null) {
                pictureBox1.Image = null;
                return;
            }
            pictureBox1.Image = info.bitmap;
            pictureBox1.Refresh();
        }

        private void BookMarkDlg_FormClosed(object sender, FormClosedEventArgs e) {
            pictureBox1.Image = null;
            BookMarkInfo[] infos = new BookMarkInfo[listBox1.Items.Count];
            listBox1.Items.CopyTo(infos, 0);
            app.Workspace.MapConfig["BookMarks"] = infos;
            foreach (var item in infos) {
                item.bitmap.Dispose();
            }
        }

        private void btDelete_Click(object sender, EventArgs e) {
            if (this.listBox1.SelectedIndex == -1) {
                return;
            }
            this.listBox1.Items.RemoveAt(this.listBox1.SelectedIndex);
        }

        private void btGoto_Click(object sender, EventArgs e) {
            if (this.listBox1.SelectedIndex == -1) {
                return;
            }
            BookMarkInfo info = this.listBox1.SelectedItem as BookMarkInfo;
            app.MapControl.Extent = info.Envelope;
        }

        private void listBox1_DoubleClick(object sender, EventArgs e) {
            btGoto_Click(sender, e);
        }
    }
}
