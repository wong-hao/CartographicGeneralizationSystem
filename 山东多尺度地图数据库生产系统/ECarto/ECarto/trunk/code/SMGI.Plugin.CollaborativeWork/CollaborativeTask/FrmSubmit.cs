using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;

namespace SMGI.Plugin.CollaborativeWork
{
    public partial class FrmSubmit : Form
    {
        public string SubmitDesc
        {
            get;
            private set;
        }

        /// <summary>
        /// 需要将修改要素提交到服务器中的要素类集合
        /// </summary>
        public List<string> NeedSubmitFC
        {
            get;
            private set;
        }

        public bool NeedBackUpDB
        {
            get
            {
                return cbBackUp.Checked;
            }
        }


        public FrmSubmit(Dictionary<string, Dictionary<string, FeatureInfo>> editedFeatures)
        {
            InitializeComponent();

            foreach (var kv in editedFeatures)
            {
                int rowIndex = dgSubmitContent.Rows.Add();
                dgSubmitContent.Rows[rowIndex].HeaderCell.Value = dgSubmitContent.RowCount.ToString();

                ((DataGridViewCheckBoxCell)dgSubmitContent.Rows[rowIndex].Cells["SelectedState"]).Value = true;
                dgSubmitContent.Rows[rowIndex].Cells["FCName"].Value = kv.Key;
                dgSubmitContent.Rows[rowIndex].Cells["FeatureNum"].Value = kv.Value.Count;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (rtbSubmitDesc.Text.Length == 0)
            {
                MessageBox.Show("请输入提交内容的描述信息!");
                return;
            }
           
            SubmitDesc = "";
            NeedSubmitFC = new List<string>();
            var unSubmitFC = new List<string>();
            for (int i = 0; i < dgSubmitContent.RowCount; ++i)
            {
                DataGridViewCheckBoxCell checkboxCell = dgSubmitContent.Rows[i].Cells["SelectedState"] as DataGridViewCheckBoxCell;
                if(checkboxCell != null )
                {
                    if(bool.Parse(checkboxCell.Value.ToString()))
                    {
                        NeedSubmitFC.Add(dgSubmitContent.Rows[i].Cells["FCName"].Value.ToString());

                        if (SubmitDesc == "")
                        {
                            SubmitDesc = rtbSubmitDesc.Text + "【内容：";
                            SubmitDesc += string.Format("{0}|{1}", dgSubmitContent.Rows[i].Cells["FCName"].Value.ToString(), dgSubmitContent.Rows[i].Cells["FeatureNum"].Value.ToString());
                        }
                        else
                        {
                            SubmitDesc += string.Format("、{0}|{1}", dgSubmitContent.Rows[i].Cells["FCName"].Value.ToString(), dgSubmitContent.Rows[i].Cells["FeatureNum"].Value.ToString());
                        }
                    
                        
                    }
                    else
                    {
                        unSubmitFC.Add(dgSubmitContent.Rows[i].Cells["FCName"].Value.ToString());
                    }
                }
            }
            if (NeedSubmitFC.Count == 0)
            {
                MessageBox.Show("没有选择可提交的编辑内容!");
                return;
            }
            SubmitDesc += "】";

            if (unSubmitFC.Count > 0)
            {
                string fcNameStr = "";
                foreach (var fcName in unSubmitFC)
                {
                    if (fcNameStr == "")
                    {
                        fcNameStr = fcName;
                    }
                    else
                    {
                        fcNameStr += "," + fcName;
                    }
                }

                if (DialogResult.No == MessageBox.Show(string.Format("有{0}个要素类【{1}】的编辑内容未勾选,若直接提交，则这些要素类中的编辑内容将会丢失，是否继续？", unSubmitFC.Count, fcNameStr), "提示", MessageBoxButtons.YesNo))
                    return;
            }

            using (var wo = GApplication.Application.SetBusy())
            {
                wo.SetText("正在进行本地数据库中被编辑要素的否合法判断...");
                if (!FeatureValidity.EditFeatureIsValid(CollaborativeTask.Instance.LocalWorkspace, CollaborativeTask.Instance.LocalBaseVersion, NeedSubmitFC))
                    return;
            }
                
            DialogResult = DialogResult.OK;
            
        }


    }
}
