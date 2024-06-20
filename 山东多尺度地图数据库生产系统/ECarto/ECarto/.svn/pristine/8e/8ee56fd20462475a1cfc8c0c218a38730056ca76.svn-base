using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using ESRI.ArcGIS.Controls;

namespace SMGI.Common.AttributeTable
{
    public partial class AttributeProcessForm : Form
    {
        GApplication _app;
        IMap _map;
        IFeatureLayer _layer;
        DataGridView _dataView;
        public AttributeProcessForm(GApplication app,DataGridView dataView,IMap map,IFeatureLayer layer)
        {
            InitializeComponent();
            _map = map;
            _layer = layer;
            _dataView = dataView;
            _app = app;
        }

        private void checkButton_Click(object sender, EventArgs e)
        {
            string modifyFieldName = cmbFieldNames.SelectedItem.ToString();
            string modifyFieldValue = tbModifyFieldValue.Text;

            IFeatureClass fc = _layer.FeatureClass;
            int fIndex = fc.Fields.FindField(modifyFieldName);

            progressBar1.Maximum = _dataView.Rows.GetRowCount(DataGridViewElementStates.Selected);
            progressBar1.Step = 1;
            IFeatureSelection featureSelection = _layer as IFeatureSelection;
            bool isEditing = _app.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing ? true : false;

            if (isEditing)
            {
                _app.EngineEditor.StartOperation();
            }

            for (int i = 0; i < _dataView.Rows.GetRowCount(DataGridViewElementStates.Selected); i++)
            {
                int OID = Convert.ToInt32(_dataView.SelectedRows[i].Cells[fc.OIDFieldName].Value);
                IFeature f = fc.GetFeature(OID);
                if (fIndex != -1)
                {

                    if (modifyFieldValue.ToUpper() == "NULL")
                    {
                        f.set_Value(fIndex, DBNull.Value);
                        f.Store();
                    }
                    else
                    {
                        f.set_Value(fIndex, modifyFieldValue);
                        f.Store();
                    }
                }
                progressBar1.PerformStep();
                System.Windows.Forms.Application.DoEvents();
            }

            if (isEditing)
            {
                _app.EngineEditor.StopOperation("属性统改");
            }

            this.Close();
        }

        private void btnAttributeProcess_Load(object sender, EventArgs e)
        {
            IFeatureClass fc = _layer.FeatureClass;
            var fields = fc.Fields;
            for (int i = 0; i < fields.FieldCount; i++)
            {
                var field = fields.get_Field(i);
                if (field.Type!= esriFieldType.esriFieldTypeOID && field.Type!= esriFieldType.esriFieldTypeGeometry && !field.Name.ToUpper().StartsWith("SMGI")
                    && field.Name.ToUpper()!="SHAPE_LENGTH" && field.Name.ToUpper()!="STACOD" && field.Name.ToUpper()!="FEAID" && field.Name.ToUpper()!="VERS")
                {
                    cmbFieldNames.Items.Add(field.Name);
                }
            }

            if (cmbFieldNames.Items.Count>0)
            {
                cmbFieldNames.SelectedIndex = 0;
            } 
        }
    }
}
