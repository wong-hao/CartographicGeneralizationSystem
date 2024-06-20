using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Plugin.CollaborativeWork
{
    public partial class PropertyConflictProcessForm : Form
    {

        public Dictionary<string, object> NeedUpdatePropertyList
        {
            protected set;
            get;
        }

        private Dictionary<string, KeyValuePair<object, object>> _conflictPropertys;
        public PropertyConflictProcessForm(Dictionary<string, KeyValuePair<object, object>> conflictPropertys)
        {
            InitializeComponent();

            _conflictPropertys = conflictPropertys;
            
        }

        private void PropertyConflictProcessForm_Load(object sender, EventArgs e)
        {
            foreach (var kv in _conflictPropertys)
            {
                int rowIndex = dgConflictResult.Rows.Add();

                dgConflictResult.Rows[rowIndex].Cells["FieldName"].Value = kv.Key;
                if (kv.Value.Key == null)
                {
                    dgConflictResult.Rows[rowIndex].Cells["localFeatureVaule"].Value = "<空>";
                }
                else
                {
                    dgConflictResult.Rows[rowIndex].Cells["localFeatureVaule"].Value = kv.Value.Key.ToString();
                }
                if (kv.Value.Value == null)
                {
                    dgConflictResult.Rows[rowIndex].Cells["ServerFeatureValue"].Value = "<空>";
                }
                else
                {
                    dgConflictResult.Rows[rowIndex].Cells["ServerFeatureValue"].Value = kv.Value.Value.ToString();
                }


            }

            btOK.Enabled = _conflictPropertys.Count > 0;
        }

        private void dgConflictResult_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btOK_Click(object sender, EventArgs e)
        {
            NeedUpdatePropertyList = new Dictionary<string, object>();
            for (int i = 0; i < dgConflictResult.Rows.Count; ++i)
            {
                string fn = dgConflictResult.Rows[i].Cells["FieldName"].Value.ToString();
                DataGridViewCheckBoxCell checkboxCell = dgConflictResult.Rows[i].Cells["Replace"] as DataGridViewCheckBoxCell;

                if (Convert.ToBoolean(checkboxCell.EditedFormattedValue))
                {
                    NeedUpdatePropertyList.Add(fn, _conflictPropertys[fn].Value);
                }
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
