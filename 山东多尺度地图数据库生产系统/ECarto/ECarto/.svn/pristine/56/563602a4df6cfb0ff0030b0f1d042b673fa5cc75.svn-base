using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmAnnoConflict : Form
    {
        ILayer[] lyrs;
        public FrmAnnoConflict(ILayer[] lyrs_)
        {
            InitializeComponent();
            lyrs = lyrs_;
            foreach (var lyr in lyrs)
            {
              int index=   this.checkedListBox1.Items.Add(lyr.Name);
              if (lyr.Name !="整饰注记")
              {
                  this.checkedListBox1.SetItemChecked(index, true);
              }
            }
            
        }
        public AnnoConflictType conflictType = AnnoConflictType.Envelop;
        private void FrmAnnoConflict_Load(object sender, EventArgs e)
        {
            string path = System.IO.Path.GetDirectoryName(GApplication.Application.Workspace.FullName) + "\\注记冲突结果.shp";
            this.txtShp.Text = path;
        }
        public  List<ILayer> ListLyrs = new List<ILayer>();
        public string ShpFile
        {
            get
            {
                return this.txtShp.Text;
            }
        }
        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (this.txtShp.Text.LastIndexOf(".shp") == -1)
            {
            
                MessageBox.Show("请设置保存文件");
                return;
            }
            ListLyrs.Clear();
            if (checkedListBox1.CheckedItems.Count == 0)
                return;
            for (int i = 0; i < checkedListBox1.CheckedItems.Count; i++)
            {
               string lyrName=  checkedListBox1.CheckedItems[i].ToString();
               var ll= lyrs.Where(t => t.Name == lyrName).FirstOrDefault();
               ListLyrs.Add(ll);
            }
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "保存文件|*.shp";
            sf.FileName = "注记冲突结果";
            sf.RestoreDirectory = true;
            DialogResult dr = sf.ShowDialog();
            if (dr != DialogResult.OK)
                return;
            this.txtShp.Text = sf.FileName;
        }

        private void btAdv_Click(object sender, EventArgs e)
        {
            FrmAnnoCfAd frm = new FrmAnnoCfAd();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                conflictType = frm.ConflictType;
            }
        }
    }
}
