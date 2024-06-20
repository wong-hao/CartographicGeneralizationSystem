using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using SMGI.Common;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmSDMColorAdd : Form
    {
        string mdbpath = "";
        string tableName = "色带填色";
        public FrmSDMColorAdd()
        {
            InitializeComponent();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btOK_Click(object sender, EventArgs e)
        {

            //添加到数据库
            IWorkspaceFactory pWorkspaceFactory = new AccessWorkspaceFactory();
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(mdbpath, 0);

            ITable ptable = (pWorkspace as IFeatureWorkspace).OpenTable(tableName);

            try
            {

                if (C.Text == "" || M.Text == "" || Y.Text == "" || K.Text == "")
                {

                    MessageBox.Show("颜色文本框不能为空！");
                    return;
                }
                if (Plan.Value == 0)
                {
                    MessageBox.Show("数字不能为0！");
                    return;
                }
                if (cmbType.SelectedItem == null)
                {
                    MessageBox.Show("内外层为空！");
                    return;
                }
                IRow r = ptable.CreateRow();
                int c;
                int.TryParse(C.Text, out c);
                int m;
                int.TryParse(M.Text, out m);
                int y = int.Parse(Y.Text);
                int.TryParse(Y.Text, out y);
                int k;
                int.TryParse(K.Text, out k);

                r.set_Value(ptable.FindField("色带方案"), Plan.Value.ToString());
                r.set_Value(ptable.FindField("C"), c.ToString());
                r.set_Value(ptable.FindField("M"), m.ToString());
                r.set_Value(ptable.FindField("Y"), y.ToString());
                r.set_Value(ptable.FindField("K"), k.ToString());
                r.set_Value(ptable.FindField("类型"), cmbType.SelectedItem.ToString());
                r.Store();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show("添加方案错误："+ex.Message);
            }
            MessageBox.Show("添加成功！");
            DialogResult = DialogResult.OK;
        }

        private void FrmColorAdd_Load(object sender, EventArgs e)
        {

            mdbpath = GApplication.Application.Template.Root + @"\专家库\Colors.mdb";
          
            cmbType.SelectedIndex = 0;
        }
    }
}
