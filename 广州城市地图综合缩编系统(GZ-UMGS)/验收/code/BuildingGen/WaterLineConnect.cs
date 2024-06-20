using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;

using BuildingGen.Road;

namespace BuildingGen
{
    class WaterLineConnect : BaseGenCommand 
    {
        public WaterLineConnect()
        {
            base.m_category = "GWater";
            base.m_caption = "水系成网";
            base.m_message = "对水系中的悬挂线进行处理，默认已经做过水系打散";
            base.m_toolTip = "对水系中的悬挂线进行处理\n默认已经做过水系打散";
            base.m_name = "WaterLineConnect";
            base.m_usedParas = new GenDefaultPara[]{
        new GenDefaultPara("最小水系出头长度",(double)70)
        ,new GenDefaultPara("最大水系挂接距离",(double)50)
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
            GLayerInfo info = GetLayer();
            CheckLayer(info);
            m_application.MapControl.Refresh();
        }

        /// <summary>
        /// 删除短悬挂线
        /// </summary>
        /// <param name="layer"></param>
        private void CheckLayer(GLayerInfo layer)
        {
            if (System.Windows.Forms.MessageBox.Show("将进行水系成网操作，请确认", "提示", System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            IFeatureLayer flayer = layer.Layer as IFeatureLayer;
            IFeatureClass waterClass = flayer.FeatureClass;
            IFeatureClass fc = GetWaterLineClass();

            Graph g = new Graph(fc);
            List<GraphNode> hangNodes = g.GetHangNode();
//#if DEBUG
//            ESRI.ArcGIS.Display.IDisplay dis = m_application.MapControl.ActiveView.ScreenDisplay;
//            ESRI.ArcGIS.Display.SimpleMarkerSymbolClass sms = new ESRI.ArcGIS.Display.SimpleMarkerSymbolClass();
//            sms.Style = ESRI.ArcGIS.Display.esriSimpleMarkerStyle.esriSMSCircle;
//            sms.Size = 5;
//            dis.StartDrawing(dis.hDC, 0);
//            dis.SetSymbol(sms);
//            foreach (var item in hangNodes)
//            {
//                dis.DrawPoint(item.point);
//            }
//            dis.FinishDrawing();
//            System.Windows.Forms.MessageBox.Show("test");
//            //return;
//#endif

            double minLength = (double)m_application.GenPara["最小水系出头长度"];
            double minDistance = (double)m_application.GenPara["最大水系挂接距离"];
            Dictionary<int, bool> deleteFeatures = new Dictionary<int, bool>();
            ISpatialFilter sf = new SpatialFilterClass();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            WaitOperation ww = m_application.SetBusy(true);
            double c = 0;
            foreach (var item in hangNodes)
            {
                try
                {
                    c++;
                    ww.SetText("数据处理中..." + c);
                    ww.Step(hangNodes.Count);
                    int oidvalue = item.conectFeatures[0];
                    int oid = GraphNode.GetOID(oidvalue);
                    bool isFromPoint = GraphNode.GetIsFromPoint(oidvalue);

                    IFeature feautre = fc.GetFeature(oid);
                    IPolyline line = feautre.Shape as IPolyline;
                    //长度过短的直接删除
                    /*if (line.Length < minDistance)
                    {
                        if (!deleteFeatures.ContainsKey(oid))
                            deleteFeatures.Add(oid, isFromPoint);
                        continue;
                    }*/
                    IPoint p = isFromPoint ? line.FromPoint : line.ToPoint;
                    IGeometry geo = (p as ITopologicalOperator).Buffer(minDistance);
                    sf.Geometry = geo;
                    if (fc.FeatureCount(sf) < 2 && waterClass.FeatureCount(sf) < 1)
                    {
                        if (line.Length < minLength)
                            if (!deleteFeatures.ContainsKey(oid))
                                deleteFeatures.Add(oid, isFromPoint);
                        continue;
                    }
                    int isFromPoi = 0;//
                    bool findExtent = false;
                    IPolyline nearExtentLine = null;
                    esriCurveExtension flag = isFromPoint ? esriCurveExtension.esriNoExtendAtTo : esriCurveExtension.esriNoExtendAtFrom;
                    IFeatureCursor fCursor = fc.Search(sf, true);
                    IFeature nearFeature = null;
                    while ((nearFeature = fCursor.NextFeature()) != null)
                    {
                        if (nearFeature.OID == feautre.OID)
                            continue;
                        PolylineClass extentLine = new PolylineClass();
                        bool extensionsPerformed = false;
                        extentLine.ConstructExtended(line, nearFeature.Shape as IPolyline, (int)flag, ref extensionsPerformed);
                        if (extensionsPerformed && (extentLine.Length - line.Length < minDistance * 2))
                        {
                            isFromPoi = 0;
                            findExtent = true;
                            if (nearExtentLine == null)
                            {
                                nearExtentLine = extentLine;
                            }
                            else
                            {
                                if (nearExtentLine.Length > extentLine.Length)
                                {
                                    nearExtentLine = extentLine;
                                }//if
                            }//else
                        }//if
                        //之前找到了延伸线就不找最近点了,否则接着找最近点
                        if (findExtent)
                            continue;
                        IPoint nearPoint = new PointClass();
                        double distanceAlongCurve = 0;
                        double distanceFromCurve = 0;
                        bool bRightSide = false;
                        (nearFeature.Shape as IPolyline).QueryPointAndDistance(esriSegmentExtension.esriNoExtension, p, false
                          , nearPoint, ref distanceAlongCurve, ref distanceFromCurve, ref bRightSide);
                        IPointCollection newLine = feautre.ShapeCopy as IPointCollection;

                        if (isFromPoint)
                            newLine.InsertPoints(0, 1, ref nearPoint);
                        else
                            newLine.AddPoints(1, ref nearPoint);

                        if ((nearExtentLine == null) || nearExtentLine.Length > (newLine as IPolyline).Length)
                        {
                            nearExtentLine = newLine as IPolyline;
                            isFromPoi = isFromPoint ? 1 : -1;
                        }//if          
                    }//while
                    int isFromPoi2 = 0;
                    bool findExtent2 = false;
                    IPolyline nearExtentLine2 = null;
                    IFeatureCursor fCursor2 = waterClass.Search(sf, true);
                    IFeature nearFeature2 = null;
                    while ((nearFeature2 = fCursor2.NextFeature()) != null)
                    {
                        PolylineClass extentLine = new PolylineClass();
                        bool extensionsPerformed = false;
                        extentLine.ConstructExtended(line, nearFeature2.Shape as IPolycurve, (int)flag, ref extensionsPerformed);
                        if (extensionsPerformed && (extentLine.Length - line.Length < minDistance * 2))
                        {
                            isFromPoi2 = 0;
                            findExtent2 = true;
                            if (nearExtentLine2 == null)
                            {
                                nearExtentLine2 = extentLine;
                            }
                            else
                            {
                                if (nearExtentLine2.Length > extentLine.Length)
                                {
                                    nearExtentLine2 = extentLine;
                                }//if
                            }//else
                        }//if
                        //之前找到了延伸线就不找最近点了,否则接着找最近点
                        if (findExtent2)
                            continue;
                        IPoint nearPoint = new PointClass();
                        double distanceAlongCurve = 0;
                        double distanceFromCurve = 0;
                        bool bRightSide = false;
                        (nearFeature2.Shape as IPolygon).QueryPointAndDistance(esriSegmentExtension.esriNoExtension, p, false
                          , nearPoint, ref distanceAlongCurve, ref distanceFromCurve, ref bRightSide);
                        IPointCollection newLine = feautre.ShapeCopy as IPointCollection;

                        if (isFromPoint)
                            newLine.InsertPoints(0, 1, ref nearPoint);
                        else
                            newLine.AddPoints(1, ref nearPoint);

                        if ((nearExtentLine2 == null) || nearExtentLine2.Length > (newLine as IPolyline).Length)
                        {
                            nearExtentLine2 = newLine as IPolyline;
                            isFromPoi2 = isFromPoint ? 1 : -1;
                        }//if          
                    }//while
                    if (nearExtentLine != null && nearExtentLine2 != null)
                    {
                        if (findExtent && findExtent2)
                        {
                            feautre.Shape = nearExtentLine.Length > nearExtentLine2.Length ? nearExtentLine2 : nearExtentLine;
                            feautre.Store();
                        }
                        if (findExtent && !findExtent2)
                        {
                            feautre.Shape = nearExtentLine;
                            feautre.Store();
                        }
                        if (findExtent2 && !findExtent)
                        {
                            feautre.Shape = nearExtentLine2;
                            feautre.Store();
                        }
                        if (!findExtent && !findExtent2)
                        {
                            //bool isCan = false;
                            IPolyline nearExtentLine3 = nearExtentLine.Length > nearExtentLine2.Length ? nearExtentLine2 : nearExtentLine;
                            int isFromPoi3 = nearExtentLine.Length > nearExtentLine2.Length ? isFromPoi2 : isFromPoi;

                            if (isFromPoi3 == 1)
                            {
                                ILine line1 = new LineClass();
                                line1.PutCoords((nearExtentLine3 as IPointCollection).get_Point(1), (nearExtentLine3 as IPointCollection).get_Point(0));
                                double angle1 = line1.Angle;
                                //line1 = null;
                                line1.PutCoords((nearExtentLine3 as IPointCollection).get_Point(2), (nearExtentLine3 as IPointCollection).get_Point(1));
                                double angle2 = line1.Angle;
                                if (3.14 * 3 / 4 < Math.Abs(angle1 - angle2) && Math.Abs(angle1 - angle2) < 3.14 * 5 / 4)
                                {
                                }
                                else
                                {
                                    feautre.Shape = nearExtentLine.Length > nearExtentLine2.Length ? nearExtentLine2 : nearExtentLine;
                                    feautre.Store();
                                }
                            }
                            if (isFromPoi3 == -1)
                            {
                                ILine line2 = new LineClass();
                                IPointCollection nearColl = nearExtentLine3 as IPointCollection;
                                line2.PutCoords(nearColl.get_Point(nearColl.PointCount - 2), nearColl.get_Point(nearColl.PointCount - 1));
                                double angle1 = line2.Angle;
                                //line2 = null;
                                line2.PutCoords(nearColl.get_Point(nearColl.PointCount - 3), nearColl.get_Point(nearColl.PointCount - 2));
                                double angle2 = line2.Angle;
                                if (3.14 * 3 / 4 < Math.Abs(angle1 - angle2) && Math.Abs(angle1 - angle2) < 3.14 * 5 / 4)
                                {
                                }
                                else
                                {
                                    feautre.Shape = nearExtentLine.Length > nearExtentLine2.Length ? nearExtentLine2 : nearExtentLine;
                                    feautre.Store();
                                }
                            }

                            //feautre.Shape = nearExtentLine.Length > nearExtentLine2.Length ? nearExtentLine2 : nearExtentLine;
                            //feautre.Store();
                        }
                    }
                    if (nearExtentLine2 == null && nearExtentLine != null)
                    {
                        IPolyline nearExtentLine3 = nearExtentLine;
                        int isFromPoi3 = isFromPoi;
                        if (isFromPoi3 == 0)
                        {
                            feautre.Shape = nearExtentLine;
                            feautre.Store();
                        }
                        if (isFromPoi3 == 1)
                        {
                            ILine line1 = new LineClass();
                            line1.PutCoords((nearExtentLine3 as IPointCollection).get_Point(1), (nearExtentLine3 as IPointCollection).get_Point(0));
                            double angle1 = line1.Angle;
                            //line1 = null;
                            line1.PutCoords((nearExtentLine3 as IPointCollection).get_Point(2), (nearExtentLine3 as IPointCollection).get_Point(1));
                            double angle2 = line1.Angle;
                            if (3.14 * 3 / 4 < Math.Abs(angle1 - angle2) && Math.Abs(angle1 - angle2) < 3.14 * 5 / 4)
                            {
                            }
                            else
                            {
                                feautre.Shape = nearExtentLine;
                                feautre.Store();
                            }
                        }
                        if (isFromPoi3 == -1)
                        {
                            ILine line2 = new LineClass();
                            IPointCollection nearColl = nearExtentLine3 as IPointCollection;
                            line2.PutCoords(nearColl.get_Point(nearColl.PointCount - 2), nearColl.get_Point(nearColl.PointCount - 1));
                            double angle1 = line2.Angle;
                            //line2 = null;
                            line2.PutCoords(nearColl.get_Point(nearColl.PointCount - 3), nearColl.get_Point(nearColl.PointCount - 2));
                            double angle2 = line2.Angle;
                            if (3.14 * 3 / 4 < Math.Abs(angle1 - angle2) && Math.Abs(angle1 - angle2) < 3.14 * 5 / 4)
                            {
                            }
                            else
                            {
                                feautre.Shape = nearExtentLine;
                                feautre.Store();
                            }
                        }
                        //feautre.Shape = nearExtentLine;
                        //feautre.Store();
                    }
                    if (nearExtentLine == null && nearExtentLine2 != null)
                    {
                        IPolyline nearExtentLine3 = nearExtentLine2;
                        int isFromPoi3 = isFromPoi2;
                        if (isFromPoi3 == 0)
                        {
                            feautre.Shape = nearExtentLine2;
                            feautre.Store();
                        }
                        if (isFromPoi3 == 1)
                        {
                            ILine line1 = new LineClass();
                            line1.PutCoords((nearExtentLine3 as IPointCollection).get_Point(1), (nearExtentLine3 as IPointCollection).get_Point(0));
                            double angle1 = line1.Angle;
                            //line1 = null;
                            line1.PutCoords((nearExtentLine3 as IPointCollection).get_Point(2), (nearExtentLine3 as IPointCollection).get_Point(1));
                            double angle2 = line1.Angle;
                            if (3.14 * 3 / 4 < Math.Abs(angle1 - angle2) && Math.Abs(angle1 - angle2) < 3.14 * 5 / 4)
                            {
                            }
                            else
                            {
                                feautre.Shape = nearExtentLine2;
                                feautre.Store();
                            }
                        }
                        if (isFromPoi3 == -1)
                        {
                            ILine line2 = new LineClass();
                            IPointCollection nearColl = nearExtentLine3 as IPointCollection;
                            line2.PutCoords(nearColl.get_Point(nearColl.PointCount - 2), nearColl.get_Point(nearColl.PointCount - 1));
                            double angle1 = line2.Angle;
                            //line2 = null;
                            line2.PutCoords(nearColl.get_Point(nearColl.PointCount - 3), nearColl.get_Point(nearColl.PointCount - 2));
                            double angle2 = line2.Angle;
                            if (3.14 * 3 / 4 < Math.Abs(angle1 - angle2) && Math.Abs(angle1 - angle2) < 3.14 * 5 / 4)
                            {
                            }
                            else
                            {
                                feautre.Shape = nearExtentLine2;
                                feautre.Store();
                            }
                        }
                        //feautre.Shape = nearExtentLine2;
                        //feautre.Store();
                    }
                }
                catch
                {
                    continue;
                }
            }//foreach (var item in hangNodes)
            m_application.SetBusy(false);
            foreach (var item in deleteFeatures.Keys)
            {
                IFeature fe = fc.GetFeature(item);
                fe.Delete();
            }//foreach      
        }//CheckLayer

        private GLayerInfo GetLayer()
        {
            foreach (GLayerInfo info in m_application.Workspace.LayerManager.Layers)
            {
                if (info.LayerType == GCityLayerType.水系
                    && (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon
                    && info.OrgLayer != null
                    )
                {
                    return info;
                }
            }
            return null;
        }

        private IFeatureClass GetWaterLineClass()
        {
            IFeatureClass centralizedDitch = null;
            IFeatureWorkspace FeatWS = m_application.Workspace.Workspace as IFeatureWorkspace;
            IWorkspace2 WS2 = FeatWS as IWorkspace2;
            if (WS2.get_NameExists(esriDatasetType.esriDTFeatureClass, "沟渠中心线"))
            {
                centralizedDitch = FeatWS.OpenFeatureClass("沟渠中心线");
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("没有找到水系中心线层！");
            }
            return centralizedDitch;
        }
    }
}
