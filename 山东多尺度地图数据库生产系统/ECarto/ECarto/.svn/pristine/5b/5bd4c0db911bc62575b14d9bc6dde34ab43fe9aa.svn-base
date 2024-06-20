using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmAnnoRuleSet : Form
    {
        DataTable sourceDt = null;
        public DataTable targetDt = null;
        public bool Reserve = false;
        private DataRow currentRule = null;
        public DataRow targetAnnoRule = null;
        public FrmAnnoRuleSet(DataRow currentRule_)
        {
            InitializeComponent();
            currentRule = currentRule_;
           
        }

        private void cbLayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateSelectTable(cbLayers.SelectedItem.ToString());
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
                var drs = sourceDt.Select("图层='" + fcName + "'");
                DataTable dt = drs.CopyToDataTable();
                dgSelectRule.DataSource = dt;
            }
            //设置选中状态
            for(int i=0;i<dgSelectRule.Rows.Count;i++)
            {
                ((DataGridViewCheckBoxCell)dgSelectRule.Rows[i].Cells["Selected"]).Value = false;
            }
            if (currentRule != null)
            {
                for (int i = 0; i < dgSelectRule.Rows.Count; i++)
                {
                    if (dgSelectRule.Rows[i].Cells["ID"].Value.ToString() == currentRule["ID"].ToString())
                    {
                        DataGridViewCheckBoxCell checkboxCell = dgSelectRule.Rows[i].Cells["Selected"] as DataGridViewCheckBoxCell;
                        checkboxCell.Value = true;
                        break;
                    }
                }
            }
        }

        private void FrmAnnoLyr_Load(object sender, EventArgs e)
        {
            string ruleMatchFileName = EnvironmentSettings.getAnnoRuleDBFileName(GApplication.Application);
            sourceDt = CommonMethods.ReadToDataTable(ruleMatchFileName, "注记规则");
          
            cbLayers.Items.Add("所有图层");
            System.Data.DataTable dtAnnoLayers = sourceDt.AsDataView().ToTable(true, new string[] { "图层" });//distinct
            for (int i = 0; i < dtAnnoLayers.Rows.Count; ++i)
            {
                //图层名
                string LayerName = dtAnnoLayers.Rows[i]["图层"].ToString().Trim();
                cbLayers.Items.Add(LayerName);
            }
            cbLayers.SelectedIndex = 0;
            updateSelectTable(cbLayers.Text);
            
        }
        DataGridViewCheckBoxCell currentCell = null;
        private void dgSelectRule_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            DataGridViewCheckBoxCell checkboxCell = dgSelectRule.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewCheckBoxCell;
            if (checkboxCell != null)
            {
                if (currentCell != null)
                {
                    if (bool.Parse(currentCell.Value.ToString()))
                    {
                        currentCell.Value = false;
                    }
                }
                currentCell = checkboxCell;
                 
            }
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
                    break;
                }
            }

            if (targetDt.Rows.Count == 0)
            {
                MessageBox.Show("请选择规则！");
                return;
            }

            targetAnnoRule = targetDt.Rows[0];
            DialogResult = DialogResult.OK;
        }
       
    }
}
