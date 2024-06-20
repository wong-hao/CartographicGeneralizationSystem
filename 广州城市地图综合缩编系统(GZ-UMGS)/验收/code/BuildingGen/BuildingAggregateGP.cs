using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace BuildingGen
{
    public class BuildingAggregateGP : BaseGenCommand
    {
        public BuildingAggregateGP()
        {
            base.m_category = "GBuilding";
            base.m_caption = "建筑物融合(批量)";
            base.m_message = "对选定图层做建筑物融合";
            base.m_toolTip = "对选定图层做建筑物融合";
            base.m_name = "BuildingAggregateGP";
            base.m_usedParas = new GenDefaultPara[]
            {
                new GenDefaultPara("建筑物融合（批量）_融合距离",(double)5)
                ,new GenDefaultPara("建筑物融合（批量）_最小上图面积",(double)0)
                ,new GenDefaultPara("建筑物融合（批量）_最小保留洞面积",(double)0)
            };
        }
        public override bool Enabled
        {
            get
            {
                return m_application.Workspace != null
                    && m_application.EngineEditor.EditState != ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing
                    && m_application.Workspace.EditLayer != null;
            }
        }

        public override void OnClick()
        {
            IMap map = m_application.Workspace.Map;
            GLayerInfo info = m_application.Workspace.EditLayer;
            IFeatureLayer layer = (info.Layer as IFeatureLayer);
            if (layer == null)
            {
                return;
            }

            IFeatureClass fc = layer.FeatureClass;
            if (fc.ShapeType != esriGeometryType.esriGeometryPolygon)
            {
                System.Windows.Forms.MessageBox.Show("当前编辑图层不是面状图层");
                return;
            }

            AggregatePolygons ap = new AggregatePolygons();
            ap.in_features = fc;
            int i = 1;
            string featureName = layer.Name;
            while ((m_application.Workspace.Workspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass,featureName + i.ToString()))
            {
                i++;
            }
            featureName += i.ToString();
            string tableName = layer.Name + "_tb";
            i = 1;
            while ((m_application.Workspace.Workspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable, tableName + i.ToString()))
            {
                i++;
            }
            tableName += i;
            ap.out_feature_class = featureName;
            ap.aggregation_distance = m_application.GenPara["建筑物融合（批量）_融合距离"];
            ap.minimum_area = m_application.GenPara["建筑物融合（批量）_最小上图面积"];
            ap.minimum_hole_size = m_application.GenPara["建筑物融合（批量）_最小保留洞面积"];
            ap.orthogonality_option = "ORTHOGONAL";
            ap.out_table = tableName;
            
            m_application.SetBusy(true);
            try
            {
                object result = m_application.Geoprosessor.Execute(ap, null);
                if (result != null)
                {
                    IFeatureClass fcGen = (m_application.Workspace.Workspace as IFeatureWorkspace).OpenFeatureClass(featureName);

                    List<IField> fields = new List<IField>();
                    for (int j = 0; j < fc.Fields.FieldCount; j++)
                    {
                        IField field = (fc.Fields.get_Field(j) as ESRI.ArcGIS.esriSystem.IClone).Clone() as IField;
                        if (field.Editable 
                            && field.Type != esriFieldType.esriFieldTypeGeometry
                            && field.Type != esriFieldType.esriFieldTypeOID
                            )
                        {
                            if (field.Name == fcGen.OIDFieldName)
                            {
                                (field as IFieldEdit).Name_2 = fc.OIDFieldName;
                            }
                            fcGen.AddField(field);
                            fields.Add(field);
                        }
                    }
                    //string sql = "update {gen} set ({feilds}) = (select {feilds} from {tb} left join {3})";
                    tableName = System.IO.Path.GetFileName(ap.out_table.ToString());
                    ITable table = (m_application.Workspace.Workspace as IFeatureWorkspace).OpenTable(tableName);
                    IFeatureCursor cursor = fcGen.Update(null, true);
                    IFeature feature = null;
                    while ((feature = cursor.NextFeature())!= null)
                    {
                        int id = feature.OID;
                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = "OUTPUT_FID = " + id.ToString();
                        ICursor cursor2 = table.Search(qf, true);
                        IRow row = null;
                        IFeature maxFeature = null;
                        while ((row = cursor2.NextRow())!=null)
                        {
                            IFeature f = fc.GetFeature((int)row.get_Value(row.Fields.FindField("INPUT_FID")));
                            if (maxFeature == null)
                            {
                                maxFeature = f;
                            }
                            else if ((f.Shape as IArea).Area > (maxFeature.Shape as IArea).Area)
                            { 
                                maxFeature =f;                                
                            }
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor2);
                        foreach (IField fi in fields)
	                    {
                            feature.set_Value(feature.Fields.FindField(fi.Name)
                                ,maxFeature.get_Value(maxFeature.Fields.FindField(fi.Name)));
	                    }
                        cursor.UpdateFeature(feature);
                    }
                    cursor.Flush();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(table);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fcGen);
                    m_application.Workspace.LayerManager.AddExistLayer(featureName, layer);
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("融合出错");
            }
            m_application.SetBusy(false);

        }

    }
}
