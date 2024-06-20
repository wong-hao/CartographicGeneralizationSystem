using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;

namespace SMGI.Plugin.GeneralEdit
{
    public partial class LayerFeatureTopoForm : Form
    {

        private DataTable result;

        private GApplication app;
        public string tolerance { get; set; }
        public Dictionary<string, int> layerlevel { get; set; }
        public LayerFeatureTopoForm(DataTable _dt, GApplication _app)
        {
            InitializeComponent(); result = _dt; app = _app;
        }

        public void LayerFeatureTopoForm_Load(object sender, EventArgs e)
        {
            IntiColums();
            loadData();
        }
        public void IntiColums()
        {
            DataGridViewCheckBoxColumn checkColumn = new DataGridViewCheckBoxColumn();

            checkColumn.HeaderText = "选择";
            checkColumn.Width = 20;
            checkColumn.MinimumWidth = 20;
            this.dataGV_reuslt.Columns.Add(checkColumn);

            DataGridViewTextBoxColumn txtColumn = new DataGridViewTextBoxColumn();
            txtColumn.HeaderText = "图层";
            this.dataGV_reuslt.Columns.Add(txtColumn);

            DataGridViewTextBoxColumn txtlevelColumn = new DataGridViewTextBoxColumn();
            txtlevelColumn.HeaderText = "级别";
            this.dataGV_reuslt.Columns.Add(txtlevelColumn);
        }
        public void loadData()
        {
            int i = 0;
            foreach (DataRow dr in result.Rows)
            {
                object[] dataObj = new object[result.Columns.Count];
                dataObj[0] = false;
                for (int cindex = 1; cindex < result.Columns.Count; cindex++)
                {
                    dataObj[cindex] = dr[cindex].ToString();
                }
                dataGV_reuslt.Rows.Insert(i, dataObj);
                dataGV_reuslt.Rows[i].HeaderCell.Value = (i + 1).ToString();
                i++;
            }
        }

        private void btnSELALL_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow r in this.dataGV_reuslt.Rows)
            {
                if (r.Cells[0].Value == null)
                    continue;
                //勾选上               
                string test = r.Cells[0].EditedFormattedValue.ToString();
                r.Cells[0].Value = true;
                this.dataGV_reuslt.CommitEdit((DataGridViewDataErrorContexts)123);
                this.dataGV_reuslt.EndEdit();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow r in this.dataGV_reuslt.Rows)
            {
                if (r.Cells[0].Value == null)
                    continue;
                //勾选上               
                string test = r.Cells[0].EditedFormattedValue.ToString();
                r.Cells[0].Value = false;
                this.dataGV_reuslt.CommitEdit((DataGridViewDataErrorContexts)123);
                this.dataGV_reuslt.EndEdit();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            tolerance = _tolerance.Text.ToString().Trim();
            layerlevel = new Dictionary<string, int>();
            foreach (DataGridViewRow r in this.dataGV_reuslt.Rows)
            {
                if (r.Cells[0].Value == null)
                    continue;
                //勾选上               
                string test = r.Cells[0].EditedFormattedValue.ToString();
                if (test.ToLower() == "true")
                {
                    layerlevel.Add(r.Cells[1].Value.ToString(), int.Parse(r.Cells[2].Value.ToString()));
                }
            }
            if (tolerance == null || layerlevel.Count == 0) { MessageBox.Show("请设置好参数"); }
            this.Close();
        }
    }
}
