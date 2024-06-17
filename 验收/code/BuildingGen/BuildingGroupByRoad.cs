using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using GENERALIZERLib;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.esriSystem;


namespace BuildingGen
{
    public class BuildingGroupByRoad : BaseGenTool
    {
        INewPolygonFeedback fb;
        GLayerInfo info;
        Generalizer gen;
        public BuildingGroupByRoad()
        {
            base.m_category = "GBuilding";
            base.m_caption = "根据道路分组";
            base.m_message = "根据道路给建筑物自动分组";
            base.m_toolTip = "根据道路给建筑物自动分组";
            base.m_name = "BuildingGBR";
            //gen = new Generalizer();
            statistics = new DataStatisticsClass();
        }

        public override bool Enabled
        {
            get
            {
                return (m_application.Workspace != null);
            }
        }

        private void ClearGroup(IFeatureClass fc, int index)
        {
            IFeatureCursor cursor = fc.Update(null, true);
            IFeature f = null;
            while ((f = cursor.NextFeature()) != null)
            {
                object v = f.get_Value(index);
                try
                {
                    if (Convert.ToInt32(v) == -1)
                        continue;
                }
                catch
                {
                }
                f.set_Value(index, 0);
                cursor.UpdateFeature(f);
            }
            cursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
        }
        private int CheckField(GLayerInfo info)
        {
            IFeatureClass fc = (info.Layer as IFeatureLayer).FeatureClass;
            int index = fc.FindField("G_BuildingGroup");
            if (index == -1)
            {
                IFieldEdit2 field = new FieldClass();
                field.Name_2 = "G_BuildingGroup";
                field.Type_2 = esriFieldType.esriFieldTypeInteger;
                field.DefaultValue_2 = 0;
                fc.AddField(field);
                index = fc.FindField("G_BuildingGroup");
                ClearGroup(fc, index);
            }
            (info.Layer as IGeoFeatureLayer).DisplayAnnotation = true;
            ESRI.ArcGIS.Carto.IAnnotateLayerPropertiesCollection alp = (info.Layer as IGeoFeatureLayer).AnnotationProperties;
            alp.Clear();
            ESRI.ArcGIS.Carto.ILabelEngineLayerProperties lelp = new ESRI.ArcGIS.Carto.LabelEngineLayerPropertiesClass();
            lelp.Expression = "[G_BuildingGroup]";
            lelp.BasicOverposterLayerProperties.NumLabelsOption = esriBasicNumLabelsOption.esriOneLabelPerShape;
            alp.Add(lelp as ESRI.ArcGIS.Carto.IAnnotateLayerProperties);
            //(info.Layer as IGeoFeatureLayer).DisplayField = "G_BuildingGroup";
            return index;
        }
        public override void OnClick()
        {
            info = null;
            foreach (GLayerInfo tempInfo in m_application.Workspace.LayerManager.Layers)
            {
                if (tempInfo.LayerType == GCityLayerType.建筑物
                    && (tempInfo.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon
                    && tempInfo.OrgLayer != null
                    )
                {
                    info = tempInfo;
                    break;
                }
            }
            if (info == null)
            {
                System.Windows.Forms.MessageBox.Show("没有找到建筑物图层");
                return;
            }
            IMap map = m_application.Workspace.Map;
            IFeatureLayer layer = (info.Layer as IFeatureLayer);
            if (layer == null)
            {
                return;
            }
            IFeatureClass fc = layer.FeatureClass;

            fieldID = CheckField(info);
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "G_BuildingGroup >0";
            statistics.Cursor = (info.Layer as IFeatureLayer).Search(qf, true) as ICursor;
            statistics.Field = "G_BuildingGroup";

            try
            {
                maxValue = Convert.ToInt32(statistics.Statistics.Maximum);
            }
            catch
            {
                maxValue = 0;
            }

        }
        int maxValue;
        int fieldID;
        IDataStatistics statistics;
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button == 4)
                return;
            if (info == null)
                return;

            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

            if (fb == null)
            {
                fb = new NewPolygonFeedbackClass();
                fb.Display = m_application.MapControl.ActiveView.ScreenDisplay;
                fb.Start(p);
            }
            else
            {
                fb.AddPoint(p);
            }
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            if (fb != null)
            {
                IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                fb.MoveTo(p);
            }
        }

        public override void OnDblClick()
        {
            if (fb == null)
                return;

            IPolygon p = fb.Stop();
            fb = null;
            Gen(p);

            m_application.MapControl.Refresh();
        }

        private void Densify(List<IFeature> featureList)
        {
            IPolyline poly = new PolylineClass();
            foreach (IFeature oriFeature in featureList)
            {
                poly = oriFeature.Shape as IPolyline;
                poly.Densify(7, 0);
                oriFeature.Shape = poly;
                oriFeature.Store();
            }

        }

        private void Gen(IPolygon range)
        {
            IFeatureLayer buildingLayer = new FeatureLayerClass();
            IFeatureLayer roadLayer = new FeatureLayerClass();

            foreach (GLayerInfo tempInfo in m_application.Workspace.LayerManager.Layers)
            {
                if (tempInfo.LayerType == GCityLayerType.建筑物)
                {
                    buildingLayer = tempInfo.Layer as IFeatureLayer;
                }
                if (tempInfo.LayerType == GCityLayerType.道路)
                {
                    roadLayer = tempInfo.Layer as IFeatureLayer;
                }
            }
            IFeatureClass buildingClass = buildingLayer.FeatureClass;
            IFeatureClass roadClass = roadLayer.FeatureClass;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeatureCursor BFeatCur = buildingClass.Search(sf, false);
            IFeature bFeat = null;
            Dictionary<int, IFeature> build_idfeat = new Dictionary<int, IFeature>();
            while ((bFeat = BFeatCur.NextFeature()) != null)
            {
                build_idfeat.Add(bFeat.OID, bFeat);
            }


            WaitOperation wait = m_application.SetBusy(true);
            try
            {
                TinClass tin;
                tin = new TinClass();
                tin.InitNew(m_application.MapControl.FullExtent);
                tin.StartInMemoryEditing();
                foreach (IFeature feat in build_idfeat.Values)
                {
                    IPoint p = (feat.Shape as IArea).Centroid;
                    p.Z = 0;
                    tin.AddPointZ(p, feat.OID);
                }
                IFeatureCursor roadCur = roadClass.Update(sf, false);
                IFeature roadFeat = null;
                while ((roadFeat = roadCur.NextFeature()) != null)
                {
                    IPolyline road = roadFeat.Shape as IPolyline;
                    road.Densify(8, 0);
                    roadFeat.Shape = road;
                    roadFeat.Store();
                    IPointCollection pc = road as IPointCollection;
                    object z = 0;
                    for (int j = 0; j < pc.PointCount; j++)
                    {
                        tin.AddShape(pc.get_Point(j), esriTinSurfaceType.esriTinMassPoint, -1, ref z);
                    }
                }
                List<int> IDs = new List<int>();
                ID_Group.Clear();
                for (int m = 1; m < tin.NodeCount + 1; m++)
                {
                    wait.Step(tin.NodeCount);
                    IDs.Clear();
                    ITinNode node = tin.GetNode(m);
                    if (node.TagValue == -1 || !node.IsInsideDataArea)
                    {
                        continue;
                    }
                    GetAGroupFromANode(tin, node, ref IDs);
                    maxValue++;
                    foreach (int id in IDs)
                    {
                        ID_Group.Add(id, maxValue);
                        IFeature featt = null;
                        build_idfeat.TryGetValue(id, out featt);
                        featt.set_Value(fieldID, maxValue);
                        featt.Store();
                    }

                }
                m_application.MapControl.Refresh();
            }
            catch (Exception ex)
            {

            }
            m_application.SetBusy(false);
        }

        Dictionary<int, int> ID_Group = new Dictionary<int, int>();
        Stack<ITinNode> stackTinNode = new Stack<ITinNode>();

        Dictionary<int, bool> isExisted = new Dictionary<int, bool>();
        private void GetAGroupFromANode(TinClass tin, ITinNode tinNodeFrom, ref List<int> groupID)
        {

            ITinEdit tinEdit = tin as ITinEdit;
            ITinAdvanced2 tinAd = tin as ITinAdvanced2;
            //int fidFrom = tinNodeFrom.TagValue;
            ITinEdgeArray tinEdgeArray = tinNodeFrom.GetIncidentEdges();
            for (int j = 0; j < tinEdgeArray.Count; j++)
            {
                ITinEdge tinEdge = tinEdgeArray.get_Element(j);
                ITinNode tinNodeTo = tinEdge.ToNode;
                if (tinNodeTo.TagValue == -1 || !tinNodeTo.IsInsideDataArea)
                {
                    continue;
                }
                if (!isExisted.ContainsKey(tinNodeTo.TagValue))
                {
                    stackTinNode.Push(tinNodeTo);
                    isExisted.Add(tinNodeTo.TagValue, true);
                }
            }

            groupID.Add(tinNodeFrom.TagValue);
            tinEdit.SetNodeTagValue(tinNodeFrom.Index, -1);

            while (stackTinNode.Count != 0)
            {
                ITinNode tinNode2 = stackTinNode.Pop();
                isExisted.Remove(tinNode2.TagValue);
                if (tinNode2.TagValue != -1)
                {
                    GetAGroupFromANode(tin, tinNode2, ref groupID);
                }
            }

        }

    }

}