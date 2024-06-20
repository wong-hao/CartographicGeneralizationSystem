using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace BuildingGen
{
    public class RoadAutoSelect : BaseGenCommand
    {
        public RoadAutoSelect()
        {
            base.m_category = "GRoad";
            base.m_caption = "自动选取";
            base.m_message = "道路自动选取";
            base.m_toolTip = "道路自动选取";
            base.m_name = "RoadRank";
            m_usedParas = new GenDefaultPara[] 
            {
                new GenDefaultPara("道路完全选取等级",(int)3),
                new GenDefaultPara("道路选取连通度",(int)5),
                new GenDefaultPara("道路选取长度",(double)100),
            };
        }
        public override bool Enabled
        {
            get
            {
                return m_application.Workspace != null;
            }
        }

        public override void OnClick()
        {
            GLayerInfo roadLayer = null;
            foreach (GLayerInfo info in m_application.Workspace.LayerManager.Layers)
            {
                if (info.LayerType == GCityLayerType.道路
                    && (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
                    && info.OrgLayer != null
                    )
                {
                    roadLayer = info;
                    break;
                }
            }
            if (roadLayer == null)
            {
                System.Windows.Forms.MessageBox.Show("缺少道路中心线图层！");
                return;
            }
            IFeatureClass fc = (roadLayer.Layer as IFeatureLayer).FeatureClass;

            int rankID = fc.Fields.FindField("道路等级");
            int strokeID = fc.Fields.FindField("道路分组");
            if (rankID == -1 || strokeID == -1)
            {
                System.Windows.Forms.MessageBox.Show("没有计算道路等级！");
                return;
            }

            int connectID = fc.FindField("道路连通度");
            if (connectID == -1)
            {
                IFieldEdit2 field = new FieldClass();
                field.Name_2 = "道路连通度";
                field.Type_2 = esriFieldType.esriFieldTypeInteger;
                fc.AddField(field as IField);
                connectID = fc.Fields.FindField("道路连通度");
            }
            int usedID = fc.FindField("_GenUsed");
            if (usedID == -1)
            {
                IFieldEdit2 field = new FieldClass();
                field.Name_2 = "_GenUsed";
                field.Type_2 = esriFieldType.esriFieldTypeSmallInteger;
                fc.AddField(field as IField);
                usedID = fc.Fields.FindField("_GenUsed");
            }


            IFeatureLayerDefinition fdefinition = roadLayer.Layer as IFeatureLayerDefinition;
            fdefinition.DefinitionExpression = "";

            int allRank = (int)m_application.GenPara["道路完全选取等级"];

            WaitOperation wo = m_application.SetBusy(true);
            ISpatialFilter sf = new SpatialFilterClass();

            List<int> usedIDs = new List<int>();
            Dictionary<int, List<IFeature>> strokes = new Dictionary<int,List<IFeature>>();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            int fCount = fc.FeatureCount(null);
            IFeatureCursor updateCursor = fc.Search(null, false);
            IFeature updateFeature = null;
            #region 处理非strok路段
            while ((updateFeature = updateCursor.NextFeature()) != null)
            {
                wo.SetText("正在处理：[" + updateFeature.OID + "]");
                int sID = (int)updateFeature.get_Value(strokeID);
                int rank = (int)updateFeature.get_Value(rankID);
                if (rank <= allRank)
                {
                    updateFeature.set_Value(usedID, 1);
                    updateFeature.Store();
                    wo.Step(fCount);
                    continue;
                }
                else if (sID != -1)
                {
                    if (!strokes.ContainsKey(sID))
                    {
                        strokes.Add(sID, new List<IFeature>());
                    }
                    strokes[sID].Add(updateFeature);
                }
                else
                {
                    int connectDegree = 0;
                    sf.Geometry = updateFeature.Shape;
                    IFeatureCursor findCursor = fc.Search(sf, true);
                    IFeature fe = null;
                    bool needCheck = true;

                    while ((fe = findCursor.NextFeature()) != null)
                    {                        
                        if (fe.OID == updateFeature.OID)
                        {
                            continue;
                        }
                        if (sID != -1)
                        {
                            int cSID = (int)fe.get_Value(strokeID);
                            if (cSID == sID)
                            {
                                continue;
                            }
                        }
                        int cRank = (int)fe.get_Value(rankID);
                        if (cRank >= allRank)
                            needCheck = false;
                        connectDegree += cRank;
                    }
                    bool canSelect = CanSelect(connectDegree, (updateFeature.Shape as IPolyline).Length);
                    if (canSelect && needCheck)
                        usedIDs.Add(updateFeature.OID);
                    updateFeature.set_Value(usedID, canSelect ? 1 : 0);
                    updateFeature.Store();
                    wo.Step(fCount);
                }
            }
            #endregion

            object miss = Type.Missing;
            int wCount = strokes.Count;
            foreach (int sid in strokes.Keys)
            {
                wo.SetText("正在处理特殊道路段[" + sid + "]");
                wo.Step(wCount);
                PolylineClass line = new PolylineClass();
                foreach (IFeature item in strokes[sid])
                {
                    IGeometryCollection gc = item.Shape as IGeometryCollection;
                    for (int i = 0; i < gc.GeometryCount; i++)
                    {
                        line.AddGeometry(gc.get_Geometry(i),ref miss,ref miss);
                    }
                }
                line.Simplify();
                sf.Geometry = line;

                int connectDegree = 0;
                bool needCheck = true;
                IFeature fe = null;
                IFeatureCursor fineCursor = fc.Search(sf,true);
                while ((fe = fineCursor.NextFeature())!= null)
                {
                    int cSID = (int)fe.get_Value(strokeID);
                    if (cSID == sid)
                    {
                        continue;
                    }
                    int cRank = (int)fe.get_Value(rankID);
                    if (cRank >= allRank)
                        needCheck = false;
                    connectDegree += cRank;
                }
                bool canSelect = CanSelect(connectDegree,line.Length);
                foreach (IFeature item in strokes[sid])
                {
                    if (canSelect && needCheck)
                        usedIDs.Add(usedID);
                    item.set_Value(usedID, canSelect ? 1 : 0);
                    item.Store();
                    wo.Step(fCount);
                }
            }

            wCount = usedIDs.Count;

            foreach (var item in usedIDs) {
                wo.SetText("正在检查道路联通关系[" + item + "]");
                wo.Step(wCount);
                
                TreeNode root = new TreeNode();
                root.id = item;
                Stack<TreeNode> currentNode = new Stack<TreeNode>();
                
                TreeNode findNode = null;
                Dictionary<int, TreeNode> usedNode = new Dictionary<int, TreeNode>();
                usedNode.Add(item,root);
                currentNode.Push(root);
                while (true) {
                    Stack<TreeNode> childNode = new Stack<TreeNode>();
                    while (currentNode.Count > 0) {
                        TreeNode node = currentNode.Pop();
                        IFeature feature = fc.GetFeature(node.id);
                        sf.Geometry = feature.Shape;
                        IFeatureCursor childCursor = fc.Search(sf, true);
                        IFeature childFeature = null;
                        while ((childFeature = childCursor.NextFeature()) != null) {
                            if (usedNode.ContainsKey(childFeature.OID))
                                continue;
                            int cRank = (int)childFeature.get_Value(rankID);                           

                            TreeNode cNode = new TreeNode();
                            cNode.id = childFeature.OID;
                            cNode.parent = node;
                            if(cRank >= allRank)
                            {
                                findNode = cNode;
                                goto breakpoint;
                            }
                            childNode.Push(cNode);
                        }
                    }
                    if (childNode.Count == 0)
                        goto breakpoint;
                    currentNode = childNode;                    
                }
            breakpoint:
                if (findNode == null) {
                    IFeature feature = fc.GetFeature(root.id);
                    feature.set_Value(usedID, 0);
                }
                else {
                    do {
                        IFeature feature = fc.GetFeature(findNode.id);
                        feature.set_Value(usedID, 1);
                        findNode = findNode.parent;
                    } while (findNode.parent != null);
                }
            }

            m_application.SetBusy(false);

            fdefinition.DefinitionExpression = "_GenUsed = 1";
            m_application.MapControl.Refresh();
        }

        internal class TreeNode {
            internal int id;
            internal TreeNode parent;
            //internal List<TreeNode> child;
            internal TreeNode() {
                id = -1;
                parent = null;
                //child = new List<TreeNode>();
            }
        }

        private bool CanSelect(int connectDegree,double length)
        {
            if (connectDegree > (int)m_application.GenPara["道路选取连通度"] && length > (double)m_application.GenPara["道路选取长度"])
                return true;
            if (connectDegree > (int)m_application.GenPara["道路选取连通度"] * 2)
                return true;
            if (length > (double)m_application.GenPara["道路选取长度"] * 2)
                return true;
            return false;
        }
    }
}
