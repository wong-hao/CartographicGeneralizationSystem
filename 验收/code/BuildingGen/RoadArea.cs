using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;

namespace BuildingGen
{
    public class RoadArea : BaseGenCommand
    {
        public RoadArea()
        {
            base.m_category = "GRoad";
            base.m_caption = "道路宽度";
            base.m_message = "计算道路宽度和面积";
            base.m_toolTip = "计算道路宽度和面积";
            base.m_name = "RoadArea";
        }
        public override bool Enabled
        {
            get
            {
                return (m_application.Workspace != null)
                    //&& (m_application.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing)
                    //&& (m_application.Workspace.EditLayer != null)
                    ;
            }
        }
        public override void OnClick()
        {
            GLayerInfo areaInfo = null;
            GLayerInfo lineInfo = null;
            foreach (GLayerInfo item in m_application.Workspace.LayerManager.Layers)
            {
                if (item.LayerType == GCityLayerType.道路
                    && item.OrgLayer != null
                    && (item.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon)
                {
                    areaInfo = item;
                    continue;
                }
                if (item.LayerType == GCityLayerType.道路
                    && item.OrgLayer != null
                    && (item.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
                {
                    lineInfo = item;
                    continue;
                }
            }
            if (areaInfo == null || lineInfo == null)
            {
                System.Windows.Forms.MessageBox.Show("缺少道路中心线或者道路面图层");
                return;
            }
            IFeatureClass fc = (lineInfo.Layer as IFeatureLayer).FeatureClass;
            int areaID = fc.Fields.FindField("面积");
            if (areaID == -1)
            {
                IFieldEdit2 field = new FieldClass();
                field.Name_2 = "面积";
                field.Type_2 = esriFieldType.esriFieldTypeDouble;
                fc.AddField(field as IField);
                areaID = fc.Fields.FindField("面积");
            }
            int widthID = fc.Fields.FindField("宽度");
            if (widthID == -1)
            {
                IFieldEdit2 field = new FieldClass();
                field.Name_2 = "宽度";
                field.Type_2 = esriFieldType.esriFieldTypeDouble;
                fc.AddField(field as IField);
                widthID = fc.Fields.FindField("宽度");
            }
            int rankID = fc.Fields.FindField("道路等级");
            if (rankID == -1)
            {
                IFieldEdit2 field = new FieldClass();
                field.Name_2 = "道路等级";
                field.Type_2 = esriFieldType.esriFieldTypeInteger;
                fc.AddField(field as IField);
                rankID = fc.Fields.FindField("道路等级");
            }

            area(areaInfo, lineInfo, areaID,widthID,rankID);
            
        }

        private void area(GLayerInfo areaInfo, GLayerInfo lineInfo, int areaID,int widthID,int rankID)
        {
            IFeatureClass areaFc = (areaInfo.Layer as IFeatureLayer).FeatureClass;
            IFeatureClass lineFc = (lineInfo.Layer as IFeatureLayer).FeatureClass;

            if (areaID == -1)
            {
                return;
            }

            WaitOperation wo = m_application.SetBusy(true);
            IFeatureCursor lineCursor = lineFc.Update(null, true);
            IFeature lineFeature;
            int wCount = lineFc.FeatureCount(null);
            wo.Step(0);
            while ((lineFeature = lineCursor.NextFeature()) != null)
            {
                wo.SetText("正在清空现有面积[" + lineFeature.OID + "]");
                wo.Step(wCount);
                lineFeature.set_Value(areaID, 0);
                //lineFeature.set_Value(widthID, 0);
                lineCursor.UpdateFeature(lineFeature);
            }
            lineCursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(lineCursor);

            IFeatureCursor areaCursor = areaFc.Search(null, true);
            IFeature areaFeature;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            Dictionary<IFeature, double> lineFeatures = new Dictionary<IFeature, double>();

            wCount = areaFc.FeatureCount(null);
            wo.Step(0);
            while ((areaFeature = areaCursor.NextFeature()) != null)
            {
                wo.SetText("正在计算占用面积["+ areaFeature.OID +"]");
                wo.Step(wCount);
                //System.Diagnostics.Debug.WriteLine("["+System.DateTime.Now.ToString()+"]" + areaFeature.OID);
                sf.Geometry = areaFeature.Shape;
                lineCursor = lineFc.Search(sf, false);

                lineFeatures.Clear();
                double length = 0;
                while ((lineFeature = lineCursor.NextFeature()) != null)
                {
                    IPolyline intersectLine = null;
                    try
                    {
                        ITopologicalOperator opa = areaFeature.ShapeCopy as ITopologicalOperator;
                        if (!opa.IsSimple)
                        {
                            opa.Simplify();
                        }
                        ITopologicalOperator opl = lineFeature.ShapeCopy as ITopologicalOperator;
                        if (!opl.IsSimple)
                        {
                            opl.Simplify();
                        }
                        intersectLine = (opa).Intersect(opl as IGeometry, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                    }
                    catch
                    {
                    }
                    if (intersectLine != null && !intersectLine.IsEmpty)
                    {
                        lineFeatures.Add(lineFeature, intersectLine.Length);
                        length += intersectLine.Length;
                    }
                }
                double area = (areaFeature.Shape as IArea).Area;
                foreach (IFeature item in lineFeatures.Keys)
                {
                    double a = (double)item.get_Value(areaID);
                    item.set_Value(areaID, a + area * lineFeatures[item] / length);
                    //lineCursor.UpdateFeature(item);
                    item.Store();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(item);
                }
                lineFeatures.Clear();
                //lineCursor.Flush();                
                System.Runtime.InteropServices.Marshal.ReleaseComObject(lineCursor);
            }
            lineCursor = lineFc.Update(null, true);
            wCount = lineFc.FeatureCount(null);
            wo.Step(0);
            while ((lineFeature = lineCursor.NextFeature()) != null)
            {
                wo.SetText("正在计算道路宽度[" + lineFeature.OID + "]");
                wo.Step(wCount);
                //System.Diagnostics.Debug.WriteLine(lineFeature.OID);
                try
                {
                    double a = (double)lineFeature.get_Value(areaID);
                    double length = (lineFeature.Shape as IPolyline).Length;
                    double width = a/length;
                    lineFeature.set_Value(widthID, width);
                    lineFeature.set_Value(rankID, RoadRank.GetRank(m_application, width, length));
                    lineCursor.UpdateFeature(lineFeature);
                }
                catch
                {
                }
            }
            lineCursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(lineCursor);
            m_application.SetBusy(false);
        }
    }
}
