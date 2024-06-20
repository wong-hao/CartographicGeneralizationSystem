using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using SMGI.Common;

namespace SMGI.Plugin.ThematicChart.ThematicChart.ThematicMinMap
{
    public partial class FrmMinMapSet : Form
    {
        public FrmMinMapSet(string [] annolyrName)
        {
            InitializeComponent();

            this.cbcbAimLayer.Items.AddRange(annolyrName);
            if (cbcbAimLayer.Items.Count > 0)
            {
                cbcbAimLayer.SelectedIndex = 0;
            }

            this.cbcbAimLayer.Enabled = true;
            this.cbcbAimLayer.SelectedIndex = 0;
        }
        //List<string> names = new List<string>();
        public  string minMapName;
        //private string path ;
        private void FrmMinMapSet_Load(object sender, EventArgs e)
        {
            txtTitle.Text = AtlasApplication.CurrentPage.Title;
            txtPageNum.Text = AtlasApplication.CurrentPage.PageNum.ToString();
        }

        private void btAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "(*.png)|*.png";
            openFile.Title = "添加小地图";
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                string pic = openFile.FileName;
                string pathNew=GApplication.Application.Template.Root+@"\附图";
                if (!Directory.Exists(pathNew))
                {
                    Directory.CreateDirectory(pathNew);
                }
                pathNew = pathNew + "\\" + System.Guid.NewGuid().ToString() + System.IO.Path.GetExtension(pic);
                File.Copy(pic, pathNew);
                minMapName = pathNew;
                txtMinFullName.Text = openFile.FileName;
            }
        }
        public string layerName;

        private void btOK_Click(object sender, EventArgs e)
        {
            layerName = this.cbcbAimLayer.SelectedItem.ToString();
            DialogResult = DialogResult.OK;
        }

        private void btCen_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cbcbAimLayer_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
