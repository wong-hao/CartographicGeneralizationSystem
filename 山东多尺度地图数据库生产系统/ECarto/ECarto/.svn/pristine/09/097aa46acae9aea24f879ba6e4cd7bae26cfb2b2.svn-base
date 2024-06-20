using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.GeneralEdit
{
    public partial class AttributesForm : Form
    {
        private AttributesCommand attributesCommand;
        private GApplication m_Application;
        private IFeature currentFeature;

        private List<string> layernameList = new List<string>();//存储选取要素所在图层名
        private List<IFeature> featureList = new List<IFeature>();//存储选取要素

        public AttributesForm(AttributesCommand ac,GApplication application)
        {
            m_Application = application;
            attributesCommand = ac;
            attributesCommand.AttributesRefreshEvent += new EventHandler(attributesCommand_AttributesRefreshEvent);
            InitializeComponent();
            timer1.Interval = 300;
            timer1.Start();
        }

        void attributesCommand_AttributesRefreshEvent(object sender, EventArgs e)
        {
            //FillTreeView();
            //if (treeView1.Nodes[0].Nodes[0].Text != null)
            //{
            //    string fid = treeView1.Nodes[0].Nodes[0].Text.ToString();
            //    FillGridView(fid);
            //}
            AttributesRefresh();
        }

        private void AttributesRefresh()
        {
            //清空原有数据
            if (layernameList.Count > 0)
            {
                layernameList.Clear();
            }
            if (featureList.Count > 0)
            {
                featureList.Clear();
            }
            if (treeView1.Nodes.Count > 0)
            {
                treeView1.Nodes.Clear();
            }
            if (dataGridView1.RowCount > 0 || dataGridView1.ColumnCount > 0)
            {
                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Clear();
            }
            //获得图上选中数据
            IEnumFeature enumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
            (enumFeature as IEnumFeatureSetup).AllFields = true;
            (enumFeature as IEnumFeatureSetup).Recycling = false;

            if (enumFeature != null)
            {
                IFeature enumFeatureItem = null;
                while ((enumFeatureItem = enumFeature.Next()) != null)
                {
                    string layerName = (enumFeatureItem.Class as IFeatureClass).AliasName;
                    //enumFeatureItem.Class.ToString();//!!获得要素所在的图层名
                    if (!layernameList.Contains(layerName))
                    {
                        layernameList.Add(layerName);
                    }

                    featureList.Add(enumFeatureItem);
                }

                if (layernameList.Count > 0 && featureList.Count > 0)
                {
                    //有数据就填充
                    FillTreeView();
                    if (treeView1.Nodes[0].Nodes[0].Text != null)
                    {
                        string fid = treeView1.Nodes[0].Nodes[0].Text.ToString();
                        FillGridView(fid);
                    }
                }
            }
        }

        private void FillTreeView()
        {
            foreach (string layername in layernameList)
            {
                TreeNode parentNode = new TreeNode();
                parentNode.Text = layername;

                foreach (IFeature fea in featureList)
                {
                    TreeNode childNode = new TreeNode();
                    if ((fea.Class as IFeatureClass).AliasName == layername)
                    {
                        childNode.Text = fea.OID.ToString();
                        childNode.Name = "featureid";
                        parentNode.Nodes.Add(childNode);
                    }

                }
                treeView1.Nodes.Add(parentNode);
            }
            treeView1.Nodes[0].Expand();
            //treeView1.Nodes[0].Nodes[0].BackColor = Color.SteelBlue;
        }

        private void FillGridView(string featureID)
        {
            if (dataGridView1.RowCount > 0 || dataGridView1.ColumnCount > 0)
            {
                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Clear();
            }

            int cnIndex = dataGridView1.Columns.Add("attributename", "属性名称");
            int cvIndex = dataGridView1.Columns.Add("attributevalue", "属性值");
            dataGridView1.Columns[cnIndex].ReadOnly = true;//第一列为只读
            dataGridView1.Columns[cvIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            foreach (IFeature fea in featureList)
            {
                if (fea.OID.ToString() == featureID)
                {
                    currentFeature = fea;//存储需操作属性的要素                  

                    try//填充
                    {
                        for (int i = 0; i < fea.Fields.FieldCount; i++)
                        {
                            IField field = fea.Fields.get_Field(i);
                            esriFieldType fieldType = field.Type;
                            if (fieldType == esriFieldType.esriFieldTypeGeometry)
                                continue;

                            string attributeName = field.Name;
                            object attributeValue = fea.get_Value(i);
                            int index = dataGridView1.Rows.Add();
                            dataGridView1.Rows[index].Cells[0].Value = attributeName;
                            dataGridView1.Rows[index].Cells[1].Value = attributeValue;
                            if (!field.Editable || field.Type == esriFieldType.esriFieldTypeBlob)
                            {
                                dataGridView1.Rows[index].Cells[1].ReadOnly = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }                    
                }
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            //闪烁对应要素  刷新gridview
            if (e.Node.Name == "featureid")
            {
                string featureID = e.Node.Text.ToString();

                FillGridView(featureID);

                m_Application.MapControl.FlashShape(currentFeature.Shape as IGeometry);//闪烁
            }
        }
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            //单元格数据发生改变 修改要素的相应属性
            if (currentFeature != null)
            {
                int rowIndex = e.RowIndex;
                string attributeName = dataGridView1.Rows[rowIndex].Cells[0].Value.ToString();                
                object attributeValue = dataGridView1.Rows[rowIndex].Cells[1].Value;
                int fieldIndex = currentFeature.Fields.FindField(attributeName);
                
                try
                {
                    if (m_Application.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing)
                        m_Application.EngineEditor.StartOperation();
                    currentFeature.set_Value(fieldIndex, attributeValue);
                    currentFeature.Store();
                    if (m_Application.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing)
                        m_Application.EngineEditor.StopOperation("修改属性");
                }
                catch (Exception ex)
                {
                    if (m_Application.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing)
                        m_Application.EngineEditor.AbortOperation();
                    MessageBox.Show("输入属性值类型错误！");
                    dataGridView1.Rows[rowIndex].Cells[1].Value = "";
                }
            }
        }

        private void AttributesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing
                || e.CloseReason == CloseReason.None)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!this.Visible)
                return;
            if (currentFeature == null)
                return;
            if (this.Focused)
                return;            
        }

        private void AttributesForm_Load(object sender, EventArgs e)
        {

        }
                
    }
}
