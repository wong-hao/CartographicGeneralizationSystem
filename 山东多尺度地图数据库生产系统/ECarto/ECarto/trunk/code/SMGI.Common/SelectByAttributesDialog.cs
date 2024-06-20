using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices; 
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;

namespace SMGI.Common
{
    public partial class SelectByAttributesDialog : Form
    {
        const int MF_REMOVE = 0x1000;
        const int SC_CLOSE = 0xF060; //关闭

        //记录上次查询条件--by YJ
        private static string _sqlCondtion;
        //记录当前选择图层
        private static string _curLayerName;

        private bool IsControl = false;
        private IMap Map;
        public string SQLCondition
        {
            get { return tbSQL.Text; }
            set { tbSQL.Text=value; }
        }

        public ILayer LayerSelected { get; set; }
        List<ILayer> list;
        public bool PointLayer {
            get { return CBPoint.Checked; }
            set { CBPoint.Checked = value; IsControl = true; }
        }

        public bool LineLayer
        {
            get { return CBLine.Checked; }
            set { CBLine.Checked = value; IsControl = true; }
        }

        public bool AreaLayer {
            get { return CBArea.Checked; }
            set { CBArea.Checked = value; IsControl = true; }
        }

        public bool OnlyOneLayer
        {
            get
            {
                return !(panelLayer.Visible);
            }
            set
            {
                panelLayer.Visible = !value;
            }
        }

        private Dictionary<string, ILayer> dclayers;

        public event EventHandler ApplyButtomClicked;

        public SelectByAttributesDialog(IMap map)
        {
            InitializeComponent();
            Map = map;
            dclayers = new Dictionary<string, ILayer>();
            list=new List<ILayer>();
        }

        private void SelectedLayerIndexChanged(object sender, EventArgs e)
        {
            LayerSelected = cbLayers.SelectedValue as ILayer;

            if (LayerSelected != null)
            {
                lbFields.Items.Clear();
                if (LayerSelected is IFeatureLayer)
                {
                    IFeatureClass lyrfeatureclass = (LayerSelected as IFeatureLayer).FeatureClass;
                    IFields lyrfields = lyrfeatureclass.Fields;
                    for (int i = 0; i < lyrfields.FieldCount; i++)
                    {
                        IField field = lyrfields.get_Field(i);
                        string name_str = field.Name;
                        lbFields.Items.Add(name_str);
                        lbFields.SelectedIndex = 0;
                    }
                }
            }

            if (lbFields.Items.Count > 0)
            {
                btOnlyValue.Enabled = true;
            }
            else
            {
                btOnlyValue.Enabled = false;
            }
        }

        bool findLyr(ILayer l)
        {
            return l is IFeatureLayer &&
                ((CBPoint.Checked ? ((l as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint) : false) ||
                (CBLine.Checked ? ((l as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline) : false) ||
                (CBArea.Checked ? ((l as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon) : false));
        }

        private void Select_By_Attributes_Load(object sender, EventArgs e)
        {
            if (IsControl)
            {
                if (!CBPoint.Checked)
                    CBPoint.Enabled = false;
                if (!CBLine.Checked)
                    CBLine.Enabled = false;
                if (!CBArea.Checked)
                    CBArea.Enabled = false;
            }
            UpdataComboBox();
            cbSelectionMethod.SelectedIndex = 0;

            //目标图层变化了 _sqlCondtion设为空
            if (LayerSelected != null && LayerSelected.Name == _curLayerName)
            {
                tbSQL.Text = _sqlCondtion;
            }
            else
            {
                _curLayerName = LayerSelected.Name;
                _sqlCondtion = "";
            }
        }

        private void UpdataComboBox()
        {

            dclayers.Clear();
            list.Clear();
            if (OnlyOneLayer)
                list.Add(LayerSelected);
            else
            {
                var ls = Map.get_Layers();
                ls.Reset();
                ILayer l = null;
                while ((l = ls.Next()) != null)
                {
                    if (findLyr(l))
                    {
                        list.Add(l);
                    }
                }
            }

            BindingSource bs = new BindingSource();
            bs.DataSource = list;
            
            cbLayers.DataSource = bs;
            //comboBox1.DisplayMember = "Key";
            //comboBox1.ValueMember = "Value";

            if (cbLayers.Items.Count > 0 )
            {
                cbLayers.SelectedIndex = 0;
                SelectedLayerIndexChanged(cbLayers, new EventArgs());
            }

            
        }

        private void bt_Click(object sender, EventArgs e)
        {
            Button bt = sender as Button;

            if (bt.Text == "%" || bt.Text == "_")
            {
                testBoxAppendText(bt.Text, false);
            }
            else if (bt.Text == "()")
            {
                testBoxAppendText(bt.Text, true, true);
            }
            else
            {
                testBoxAppendText(bt.Text);
            }
        }

        private void testBoxAppendText(string str,bool needspace = true, bool unitmove = false)
        {
            if (needspace && !String.IsNullOrEmpty(tbSQL.Text))
            {
                str = " " + str;
            }

            int index = tbSQL.SelectionStart;
            tbSQL.Text = tbSQL.Text.Insert(index, str);

            if (!unitmove)
            {
                tbSQL.SelectionStart = index + str.Length;
            }
            else
            {
                tbSQL.SelectionStart = index + str.Length - 1;
            }
            tbSQL.Focus();
        }

        private void tbSQL_TextChanged(object sender, EventArgs e)
        {
            _sqlCondtion = SQLCondition;
            if (String.IsNullOrEmpty(SQLCondition))
            {
                btClear.Enabled = false;
            }
            else
            {
                btClear.Enabled = true;
            }
        }

        private void lbFields_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.lbFields.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                //do your stuff here
                string str_item = this.lbFields.Items[index].ToString();
                testBoxAppendText(str_item);
            }
        }

        private void OnlyValue_Click(object sender, EventArgs e)
        {
            int index = this.lbFields.SelectedIndex;
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                // get the selected item.
                string str_item = this.lbFields.Items[index].ToString();
                if (this.LayerSelected != null)
                {
                    if (this.LayerSelected is IFeatureLayer)
                    {
                        IFeatureClass featureclass = (this.LayerSelected as IFeatureLayer).FeatureClass;
                        IFeatureCursor cursor = featureclass.Search(null, false);
                        IDataStatistics datasta = new DataStatisticsClass();
                        datasta.Field = str_item;
                        datasta.Cursor = cursor as ICursor;
                        System.Collections.IEnumerator enumvartor = datasta.UniqueValues; //枚举
                        int RecordCount = datasta.UniqueValueCount;
                        string[] strvalue = new string[RecordCount];
                        enumvartor.Reset();
                        int i = 0;
                        while (enumvartor.MoveNext())
                        {
                            strvalue[i++] = enumvartor.Current.ToString();
                        }

                        int fieldindex = featureclass.Fields.FindField(datasta.Field);
                        IField field = featureclass.Fields.get_Field(fieldindex);
                        esriFieldType fieldtype = field.Type;

                        lbValues.Items.Clear();
                        foreach (var str in strvalue)
                        {
                            if (fieldtype == esriFieldType.esriFieldTypeString)
                            {
                                string str_ = "\'" + str + "\'";
                                lbValues.Items.Add(str_);
                                lbValues.SelectedIndex = 0;
                            }
                            else {
                            lbValues.Items.Add(str);
                            lbValues.SelectedIndex = 0;
                            }
                        }
                        if (lbValues.Items.Count > 0)
                        {
                            btOnlyValue.Enabled = false;
                        }
                    }
                    else { }
                }
            }
        }

        private void lbValues_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.lbValues.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                //do your stuff here
                string str_item = this.lbValues.Items[index].ToString();
                testBoxAppendText(str_item);
            }
        }

        private void SelectedFieldChanged(object sender, EventArgs e)
        {
            this.lbValues.Items.Clear();
            this.btOnlyValue.Enabled = true;
        }

        private void ClearClick(object sender, EventArgs e)
        {
            this.tbSQL.Text = "";
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            //System.Windows.Forms.MessageBox.Show(sqlcondition);
            if (LayerSelected != null)
            {
                IFeatureSelection Layerselection = (IFeatureSelection)(LayerSelected as IFeatureLayer);
                try
                {
                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = SQLCondition;

                    Layerselection.SelectFeatures(qf, (esriSelectionResultEnum)cbSelectionMethod.SelectedIndex, false);
                    Helper.RefreshAttributeWindow(LayerSelected);

                    this.DialogResult = DialogResult.OK;
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show("SQL语法错误！");
                    return;
                    // this.DialogResult = DialogResult.Cancel;
                }
            }
            else {
                System.Windows.Forms.MessageBox.Show("请选择控制图层！");
                return;
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void ApllyBottomClick(object sender, EventArgs e)
        {
            if (LayerSelected != null)
            {
                IFeatureSelection Layerselection = (IFeatureSelection)(LayerSelected as IFeatureLayer);
                try
                {
                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = SQLCondition;
                    Layerselection.SelectFeatures(qf, (esriSelectionResultEnum)cbSelectionMethod.SelectedIndex, false);
                    Helper.RefreshAttributeWindow(LayerSelected);
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show("SQL语法错误！");
                    return;
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("请选择控制图层！");
                return;
            }

            if (ApplyButtomClicked != null)
                ApplyButtomClicked(this, new EventArgs());
        }

        private void CBPoint_CheckedChanged(object sender, EventArgs e)
        {
            UpdataComboBox();
        }

        private void CBLine_CheckedChanged(object sender, EventArgs e)
        {
            UpdataComboBox();
        }

        private void CBArea_CheckedChanged(object sender, EventArgs e)
        {
            UpdataComboBox();
        }
    }
}
