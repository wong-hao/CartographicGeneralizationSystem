using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using System.IO;
using System.Xml.Linq;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmLabelLyr : Form
    {
        DataTable sourceDt = null;
        public DataTable targetDt = null;
        public bool Reserve = false;
        public FrmLabelLyr()
        {
            InitializeComponent();
           
        }

        private void cbLayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateSelectTable(cbLayers.Text);
        }
        private void updateSelectTable(string fcName)
        {
            
            if (fcName == "所有图层")
            {
                DataTable dt = sourceDt.Copy();
                dgSelectRule.DataSource = dt;
            }
            else 
            {
                var drs = sourceDt.Select("地图类型='" + fcName + "'");
                DataTable dt = drs.CopyToDataTable();
                dgSelectRule.DataSource = dt;
            }
            //设置选中状态
            for(int i=0;i<dgSelectRule.Rows.Count;i++)
            {
                ((DataGridViewCheckBoxCell)dgSelectRule.Rows[i].Cells["Selected"]).Value = true;
            }

        }

        private void FrmAnnoLyr_Load(object sender, EventArgs e)
        {
            string mdbPath = GApplication.Application.Template.Root + @"\专家库\标注引线\引线规则.mdb";
            sourceDt = Helper.ReadToDataTable(mdbPath, "属性标注");
            

            // cbLayers.Items.Add("所有类型");
            DataTable dtfcl = sourceDt.DefaultView.ToTable(true,  new string[] { "地图类型" }); 
           // System.Data.DataTable dtAnnoLayers = sourceDt.AsDataView().ToTable(true, new string[] { "图层" });//distinct
            for (int i = dtfcl.Rows.Count-1; i >=0; i--)
            {
                //图层名
                string LayerName = dtfcl.Rows[i]["地图类型"].ToString().Trim();
                cbLayers.Items.Add(LayerName);
            }
           
            //获取专题图类型
            Dictionary<string, string> envString = GApplication.Application.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
            if (envString == null || !envString.ContainsKey("ThemDataBase"))
            {
                envString = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
            }
            if (envString != null)
            {
                if (envString.ContainsKey("ThemDataBase"))
                {
                    string thematic = envString["ThemDataBase"];
                    cbLayers.SelectedIndex = cbLayers.Items.IndexOf(thematic);
                  
                }
            }
            if(cbLayers.SelectedIndex==-1)
               cbLayers.SelectedIndex = 0;
            updateSelectTable(cbLayers.Text);
        }

        private void dgSelectRule_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgSelectRule_ColumnHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {

        }

        private void dgSelectRule_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

        }

        

        private void btAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgSelectRule.Rows)
            {
                row.Cells["Selected"].Value = true;
                
            }
        }

        private void btClear_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgSelectRule.Rows)
            {
                row.Cells["Selected"].Value = false;

            }
        }


        private void btOk_Click(object sender, EventArgs e)
        {
            targetDt = (dgSelectRule.DataSource as DataTable).Clone();
            var dt=(dgSelectRule.DataSource as DataTable);
          
            for (int i = 0; i < dgSelectRule.Rows.Count; i++)
            {
                if (dgSelectRule.Rows[i].Cells["Selected"] == null)
                    continue;
                if (bool.Parse(dgSelectRule.Rows[i].Cells["Selected"].Value.ToString()))
                {
                    targetDt.Rows.Add(dt.Rows[i].ItemArray);
                }
            }

            if (targetDt.Rows.Count == 0)
            {
                MessageBox.Show("请选择规则！");
                return;
            }
            DialogResult = DialogResult.OK;
        }
        //双击弹出对话框
        private void dgSelectRule_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1||e.ColumnIndex==-1)
                return;
            
            DataRow row = (dgSelectRule.DataSource as DataTable).Rows[e.RowIndex];
            FrmLabelInfo frm = new FrmLabelInfo(row);
            if (DialogResult.OK != frm.ShowDialog())
                return;
             


        }

       

     
    }
}
