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
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmAnnoLyr : Form
    {
        DataTable sourceDt = null;
        public DataTable targetDt = null;
        public bool Reserve = false;
        public FrmAnnoLyr( DataTable sourceDt_,string mdbFilePath = "")
        {
            InitializeComponent();
            sourceDt = sourceDt_;

            tbMDBFilePath.Text = mdbFilePath;
        }

        private void cbLayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateSelectTable(cbLayers.Text);
        }
        private void updateSelectTable(string fcName)
        {
            if (sourceDt == null)
            {
                dgSelectRule.DataSource = null;
                return;
            }

            //dgSelectRule.Rows.Clear();
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
                ((DataGridViewCheckBoxCell)dgSelectRule.Rows[i].Cells["Selected"]).Value = true;
            }

        }

        private void FrmAnnoLyr_Load(object sender, EventArgs e)
        {
            cbLayers.Items.Add("所有图层");
            if (sourceDt == null)
                return;

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

        private void dgSelectRule_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgSelectRule_ColumnHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {

        }

        private void dgSelectRule_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnImportConfig_Click(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.Filter = "注记选择项配置|*.xml";
            of.InitialDirectory = GApplication.Application.Template.Root + @"\专家库";
            DialogResult dr = of.ShowDialog();
            if (dr != DialogResult.OK)
                return;

            LoadSelectConfig(of.FileName);
        }

        private void btnExportConfig_Click(object sender, EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "注记选择项配置|*.xml";
            sf.InitialDirectory = GApplication.Application.Template.Root + @"\专家库";
            DialogResult dr = sf.ShowDialog();
            if (dr != DialogResult.OK)
                return;


            bool res = ExportXML(sf.FileName);
            if (res)
            {
                MessageBox.Show("导出配置文件成功!", "提示");
            }
            else
            {
                MessageBox.Show("导出配置文件失败!", "提示");
            }
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
            if (dgSelectRule.DataSource != null)
            {
                targetDt = (dgSelectRule.DataSource as DataTable).Clone();
                var dt = (dgSelectRule.DataSource as DataTable);
                //for (int i = 0; i < dgSelectRule.Rows.Count; i++)
                //{
                //    if (bool.Parse(dgSelectRule.Rows[i]["Selected"].ToString()))
                //   {
                //       targetDt.Rows.Add(dt.Rows[i].ItemArray);

                //   }

                //}
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
                // targetDt = dgSelectRule.DataSource as DataTable;
            }
            else
            {
                MessageBox.Show("请选择规则！");
                return;
            }

            Reserve = this.cb_reserve.Checked;
            DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// 加载外部选择项配置文件
        /// </summary>
        /// <param name="fileName">XML文件全路径</param>
        public void LoadSelectConfig(string fileName)
        {
            try
            {
                List<KeyValuePair<string, string>> fcName2AnnoDesc = new List<KeyValuePair<string, string>>();

                XDocument doc = XDocument.Load(fileName);
                var content = doc.Element("Root").Element("AnnoSelectItem");
                var items = content.Elements("SelectItem");
                foreach (XElement ele in items)
                {
                    string LayerName = ele.Attribute("图层").Value;
                    string desc = ele.Attribute("注记说明").Value;

                    fcName2AnnoDesc.Add(new KeyValuePair<string, string>(LayerName, desc));
                }

                //更新控件
                int index = cbLayers.Items.IndexOf("所有图层");
                if (index != -1)//显示所有项
                {
                    cbLayers.SelectedIndex = index;
                }
                foreach (DataGridViewRow row in dgSelectRule.Rows)//先清空所有选项
                {
                    row.Cells["Selected"].Value = false;
                }
                foreach (DataGridViewRow row in dgSelectRule.Rows)//设置选择项
                {
                    string LayerName = row.Cells["图层"].Value.ToString();
                    string desc = row.Cells["注记说明"].Value.ToString();

                    KeyValuePair<string, string> kv = new KeyValuePair<string, string>(LayerName, desc);
                    if(fcName2AnnoDesc.Contains(kv))
                        row.Cells["Selected"].Value = true;
                }

                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.ToString());
            }
        }

        private bool ExportXML(string fileName)
        {
            try
            {
                FileInfo f = new FileInfo(fileName);
                XDocument doc = new XDocument();
                doc.Declaration = new XDeclaration("1.0", "utf-8", "");
                var root = new XElement("Root");
                doc.Add(root);


                var content = new XElement("AnnoSelectItem");
                root.Add(content);
                foreach (DataGridViewRow row in dgSelectRule.Rows)
                {
                    bool bSelect = bool.Parse(row.Cells["Selected"].Value.ToString());
                    if (bSelect)//将选中需生成注记的规则项导出为xml
                    {

                        var lyr = new XElement("SelectItem");
                        lyr.SetAttributeValue("图层", row.Cells["图层"].Value.ToString());
                        lyr.SetAttributeValue("注记说明", row.Cells["注记说明"].Value.ToString());
                        content.Add(lyr);
                    }

                }

                doc.Save(fileName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// 验证SQL合法性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnVerify_Click(object sender, EventArgs e)
        {
            btnVerify.Enabled = false;
            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;

            try
            {
                Dictionary<string, IFeatureClass> fcName2FC = new Dictionary<string, IFeatureClass>();

                IQueryFilter qf = new QueryFilterClass();
                for (int i = 0; i < dgSelectRule.Rows.Count; i++)
                {
                    if (dgSelectRule.Rows[i].Cells["Selected"] == null)
                        continue;
                    if (!bool.Parse(dgSelectRule.Rows[i].Cells["Selected"].Value.ToString()))
                        continue;

                    string id = dgSelectRule.Rows[i].Cells["ID"].Value.ToString().ToUpper();
                    string fcName = dgSelectRule.Rows[i].Cells["图层"].Value.ToString().ToUpper();
                    string sqlText = dgSelectRule.Rows[i].Cells["查询条件"].Value.ToString();



                    IFeatureClass fc = null;
                    if (fcName2FC.ContainsKey(fcName))
                    {
                        fc = fcName2FC[fcName];
                    }
                    else
                    {
                        if ((GApplication.Application.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, fcName))
                        {
                            fc = (GApplication.Application.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(fcName);
                            fcName2FC.Add(fcName, fc);
                        }
                        else
                        {
                            IFeatureLayer layer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName)).FirstOrDefault() as IFeatureLayer;
                            if (layer != null)
                            {
                                fc = layer.FeatureClass;
                                fcName2FC.Add(fcName, fc);
                            }
                        }
                    }
                    if (fc == null)
                    {
                        MessageBox.Show(string.Format("没有找到要素类【{0}】!", fcName));
                        return;
                    }

                    //验证SQL合法性
                    qf.WhereClause = sqlText;
                    try
                    {
                        int count = fc.FeatureCount(qf);
                    }
                    catch (Exception ex1)
                    {
                        throw new Exception(string.Format("第【{0}】行的SQL表达式错误:{1}!", id, ex1.Message));
                    }
                }

                MessageBox.Show("验证通过");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                btnVerify.Enabled = true;
                this.Cursor = System.Windows.Forms.Cursors.Arrow;
            }
        }
    }
}
