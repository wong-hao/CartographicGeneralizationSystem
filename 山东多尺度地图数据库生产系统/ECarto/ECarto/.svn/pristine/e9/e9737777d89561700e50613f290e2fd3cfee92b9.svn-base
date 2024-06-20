using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.EmergencyMap
{
    public class SymbolConflictProcess
    {
        private GApplication _app;

        private double ptBufferVal;//在非制图表达情况下，点符号的默认大小（mm）

        private string _selStateFN;

        public string SelStateFieldName
        {
            get { return _selStateFN; }
        }

        

        public SymbolConflictProcess(GApplication app)
        {
            _app = app;
            ptBufferVal = 4.0;
            _selStateFN = "selectstate";
        }

        public int DupSymbolProcess(IGeoFeatureLayer geoLayer, string levelFieldName, double scale, WaitOperation wo = null)
        {
            int conflicSymbolNum = 0;

            IFeatureClass fc = geoLayer.FeatureClass;
            int classIndex = fc.Fields.FindField(levelFieldName);
            if (-1 == classIndex)
            {
                MessageBox.Show(string.Format("要素类【{0}】中不存在字段：{1}", fc.AliasName, levelFieldName));
                return 0;
            }

            int selStateIndex = fc.FindField(_selStateFN);
            if (selStateIndex == -1)
            {
                AddField(fc, _selStateFN);
                selStateIndex = fc.FindField(_selStateFN);
            }

            List<int> conflictFeatureOID = new List<int>();//被标记为冲突的要素OID(总)

            var mc = new MapContextClass();
            mc.InitFromDisplay(_app.ActiveView.ScreenDisplay.DisplayTransformation);




            IFeatureCursor pFeaCursor1 = geoLayer.Search(new QueryFilterClass { WhereClause = "RuleID>1" }, false);
            IFeature f1 = null;
            IRepresentationClass repClass = ((IRepresentationRenderer)geoLayer.Renderer).RepresentationClass;
            Dictionary<int, double> ruleSizeDic = new Dictionary<int, double>();
            while ((f1 = pFeaCursor1.NextFeature()) != null)
            {
                if(wo != null)
                    wo.SetText(string.Format("正在处理要素【{0}】...", f1.OID));
                ptBufferVal = 0;
                if (conflictFeatureOID.Contains(f1.OID))//已被标记为冲突要素，将不显示，从而不再参与后面的冲突处理
                {
                    continue;
                }

                string cls1 = f1.get_Value(classIndex).ToString();


                if (f1.Shape.GeometryType == esriGeometryType.esriGeometryPoint &&
                    geoLayer.Renderer is RepresentationRenderer)//制图表达情况下，取制图表达几何
                {
                    int ruleID=int.Parse(f1.get_Value(fc.FindField("RuleID")).ToString());
                    if (!ruleSizeDic.ContainsKey(ruleID))
                    {
                        IRepresentationRule repRule = repClass.RepresentationRules.get_Rule(ruleID);
                        IBasicSymbol pBasicSymbol = repRule.Layer[0];//取一个图层
                        if (pBasicSymbol is IBasicMarkerSymbol)
                        {
                            IGraphicAttributes graphicAttributes = pBasicSymbol as IGraphicAttributes;
                            ptBufferVal = Convert.ToDouble(graphicAttributes.Value[2]) / 2.8345;//磅转毫米(考虑到与其冲突的符号也有大小，这里直接用其直径)
                            ruleSizeDic[ruleID] = ptBufferVal;
                        }
                    }
                    ptBufferVal = ruleSizeDic[ruleID];
                     
                    
                    
                    
                }
                IPoint ptAGNP=f1.ShapeCopy as IPoint;
                ITopologicalOperator topo = ptAGNP as ITopologicalOperator;
                if (ptBufferVal == 0)
                    ptBufferVal = 1;
                double dis=ptBufferVal * scale * 1e-3;
                IEnvelope geo = new EnvelopeClass();
                geo.PutCoords(ptAGNP.X - dis, ptAGNP.Y - dis, ptAGNP.X + dis, ptAGNP.Y + dis);
                ISpatialFilter inSpatialFilter = new SpatialFilterClass();
                inSpatialFilter.Geometry = geo;
                inSpatialFilter.WhereClause = "RuleID>1";
                inSpatialFilter.GeometryField = fc.ShapeFieldName;
                inSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                int ncount = fc.FeatureCount(inSpatialFilter);

                if (ncount > 1)
                {
                    #region
                    List<int> oids = new List<int>();//与f1相冲突的要素OID集合

                    IFeatureCursor pFeaCursor2 = fc.Search(inSpatialFilter, false);
                    IFeature f2 = null;
                    while ((f2 = pFeaCursor2.NextFeature()) != null)
                    {
                        if (conflictFeatureOID.Contains(f2.OID) || f2.OID == f1.OID)
                        {
                            continue;
                        }

                        string cls2 = f2.get_Value(classIndex).ToString();
                        if ("PRIORITY" == levelFieldName)
                        {
                            if (int.Parse(cls2) < int.Parse(cls1))
                            {
                                conflictFeatureOID.Add(f1.OID);

                                //f1被标识为冲突要素，则与其冲突的要素将被释放
                                oids.Clear();
                                break;
                            }
                            else
                            {
                                oids.Add(f2.OID);
                            }
                        }
                        else
                        {
                            if (cls2.CompareTo(cls1) < 0)
                            {
                                conflictFeatureOID.Add(f1.OID);

                                //f1被标识为冲突要素，则与其冲突的要素将被释放
                                oids.Clear();

                                break;

                            }
                            else
                            {
                                oids.Add(f2.OID);
                            }
                        }
                        Marshal.ReleaseComObject(f2);
                    }

                    if (oids.Count > 0)
                    {
                        conflictFeatureOID.AddRange(oids.ToArray());
                    }
                    #endregion

                    Marshal.ReleaseComObject(pFeaCursor2);
                }
                Marshal.ReleaseComObject(ptAGNP);
                Marshal.ReleaseComObject(geo);
                Marshal.ReleaseComObject(inSpatialFilter);
                Marshal.ReleaseComObject(f1);

            }
            Marshal.ReleaseComObject(pFeaCursor1);

            foreach (var oid in conflictFeatureOID)
            {
                IFeature feature = fc.GetFeature(oid);
                feature.set_Value(selStateIndex, "符号冲突");
                feature.Store();
                #region 隐藏符号关联的注记
                int featureId = feature.OID;
                int annotationClassID = feature.Class.ObjectClassID;
                var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return (l is IFDOGraphicsLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "ANNO");
                })).FirstOrDefault();
                IFeatureClass annoFC = (lyr as IFeatureLayer).FeatureClass;
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = string.Format("FeatureID = {0} AND AnnotationClassID = {1}", featureId, annotationClassID);
                IFeatureCursor annoCursor = annoFC.Search(qf, false);
                IFeature annoFeature = null;
                while ((annoFeature = annoCursor.NextFeature()) != null)
                {
                    if (annoFeature is IAnnotationFeature2)
                    {
                        //隐藏注记
                        (annoFeature as IAnnotationFeature2).Status = esriAnnotationStatus.esriAnnoStatusUnplaced;
                        annoFeature.Store();
                    }
                }
                Marshal.ReleaseComObject(annoCursor);
                #endregion
                System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                conflicSymbolNum++;
            }

            return conflicSymbolNum;
        }

        public bool DupSymbolProcess(IGeoFeatureLayer geoLayer, string levelFieldName, double scale, List<IFeature> featurelist)
        {
            int conflicSymbolNum = 0;
            List<int> conflictFeatureOID=new List<int> ();

            IFeatureClass fc = geoLayer.FeatureClass;
            int classIndex = fc.Fields.FindField(levelFieldName);
            int selStateIndex = fc.FindField(_selStateFN);

            for (int i = 0; i < featurelist.Count; i++)
            {
                DupSymbolHandle(geoLayer, scale, featurelist[i], conflictFeatureOID, classIndex);
            }

            foreach (var oid in conflictFeatureOID)
            {
                IFeature feature = fc.GetFeature(oid);
                feature.set_Value(selStateIndex, "符号冲突");
                feature.Store();
                #region 清除冲突符号关联的注记
                int featureId = feature.OID;
                int annotationClassID = feature.Class.ObjectClassID;
                var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return (l is IFDOGraphicsLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "ANNO");
                })).FirstOrDefault();
                IFeatureClass annoFC = (lyr as IFeatureLayer).FeatureClass;
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = string.Format("FeatureID = {0} AND AnnotationClassID = {1}", featureId, annotationClassID);
                IFeatureCursor annoCursor = annoFC.Search(qf, false);
                IFeature annoFeature = null;
                while ((annoFeature = annoCursor.NextFeature()) != null)
                {
                    annoFeature.Delete();
                }
                Marshal.ReleaseComObject(annoCursor);
                #endregion
                conflicSymbolNum++;
            }

            return true;
        }

        private void DupSymbolHandle(IGeoFeatureLayer geoLayer, double scale, IFeature f1, List<int> conflictFeatureOID, int classIndex)
        {
            var mc = new MapContextClass();
            mc.InitFromDisplay(_app.ActiveView.ScreenDisplay.DisplayTransformation);
            IFeatureClass fc = geoLayer.FeatureClass;
            ////初始化冲突字段值
            //f1.set_Value(dupIndex, "0");
            //f1.Store();
           
            if (conflictFeatureOID.Contains(f1.OID))//已被标记为冲突要素，将不显示，从而不再参与后面的冲突处理
            {
                return;
            }

            string cls1 = f1.get_Value(classIndex).ToString();


            if (f1.Shape.GeometryType == esriGeometryType.esriGeometryPoint &&
                geoLayer.Renderer is RepresentationRenderer)//制图表达情况下，取制图表达几何
            {
                IRepresentationClass repClass = ((IRepresentationRenderer)geoLayer.Renderer).RepresentationClass;
                IRepresentation rep = repClass.GetRepresentation(f1, mc);

                //rep.r
                IRepresentationRule repRule = repClass.RepresentationRules.get_Rule(rep.RuleID);
                IBasicSymbol pBasicSymbol = repRule.Layer[0];//取一个图层
                if (pBasicSymbol is IBasicMarkerSymbol)
                {
                    IGraphicAttributes graphicAttributes = pBasicSymbol as IGraphicAttributes;
                    ptBufferVal = Convert.ToDouble(graphicAttributes.Value[2]) / 2.8345;//磅转毫米(考虑到与其冲突的符号也有大小，这里直接用其直径)
                }
            }

            ITopologicalOperator topo = f1.Shape as ITopologicalOperator;
            IGeometry geo = topo.Buffer(ptBufferVal * scale * 1e-3);

            ISpatialFilter inSpatialFilter = new SpatialFilterClass();
            inSpatialFilter.Geometry = geo;
            inSpatialFilter.GeometryField = fc.ShapeFieldName;
            inSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            int ncount = fc.FeatureCount(inSpatialFilter);

            if (ncount > 1)
            {
                List<int> oids = new List<int>();//与f1相冲突的要素OID集合

                IFeatureCursor pFeaCursor2 = fc.Search(inSpatialFilter, false);
                IFeature f2 = null;
                while ((f2 = pFeaCursor2.NextFeature()) != null)
                {
                    if (conflictFeatureOID.Contains(f2.OID) || f2.OID == f1.OID)
                    {
                        continue;
                    }

                    string cls2 = f2.get_Value(classIndex).ToString();
                    if (cls2.CompareTo(cls1) < 0)
                    {
                        conflictFeatureOID.Add(f1.OID);

                        //f1被标识为冲突要素，则与其冲突的要素将被释放
                        oids.Clear();

                        break;

                    }
                    else
                    {
                        oids.Add(f2.OID);
                    }
                }

                if (oids.Count > 0)
                {
                    conflictFeatureOID.AddRange(oids.ToArray());
                }

                Marshal.ReleaseComObject(pFeaCursor2);
            }
        
        }

        /// <summary>
        /// 增加一个文本字段
        /// </summary>
        /// <param name="fCls"></param>
        /// <param name="fieldName"></param>
        private void AddField(IFeatureClass fCls, string fieldName)
        {
            IFields pFields = fCls.Fields;
            IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;
            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Name_2 = fieldName;
            pFieldEdit.AliasName_2 = fieldName;
            pFieldEdit.Length_2 = 1;
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            IClass pTable = fCls as IClass;
            pTable.AddField(pField);
            pFieldsEdit = null;
            pField = null;
        }
    }
}
