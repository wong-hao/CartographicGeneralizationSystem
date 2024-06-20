using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;

namespace BuildingGen
{
    public class GTestCommand : BaseGenTool
    {
        public GTestCommand()
        {
            base.m_category = "GTest";
            base.m_caption = "测试专用按钮";
            base.m_message = "测试专用按钮";
            base.m_toolTip = "测试专用按钮";
            base.m_name = "GTestCommand";
            points = new List<IPoint>();
            polygons = new List<IPolygon>();
        }
        public override bool Enabled
        {
            get
            {
                return (m_application.Workspace != null)
                    //&& (m_application.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing)
                    && (m_application.Workspace.EditLayer != null);
            }
        }
        TinClass tin;
        List<IPoint> points;
        List<IPolygon> polygons;
        IPolygon buffer;
        IPolygon buffer2;
        public override void Refresh(int hDC)
        {
            IDisplay display = new SimpleDisplayClass();
            display.DisplayTransformation = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation;
            display.StartDrawing(hDC, 0);

            SimpleFillSymbolClass sf = new SimpleFillSymbolClass();
            sf.Color = ColorFromRGB(0x80, 0x80, 0x80, true);

            SimpleLineSymbolClass sl = new SimpleLineSymbolClass();
            sl.Color = ColorFromRGB(255, 0, 0, false);
            sl.Width = 1;
            sf.Outline = sl;
            SimpleMarkerSymbolClass sm = new SimpleMarkerSymbolClass();
            sm.Style = esriSimpleMarkerStyle.esriSMSCircle;
            sm.Size = 4;
            sm.Color = ColorFromRGB(0, 0, 255, false);

            if (tin != null)
            {
                display.SetSymbol(sm);
                foreach (IPoint p in points)
                {
                    display.DrawPoint(p);
                }
            }
            display.SetSymbol(sf);
            if (buffer != null && !buffer.IsEmpty)
            {
                display.DrawPolygon(buffer);
            }
            sl.Color = ColorFromRGB(0, 255, 0, false);
            sf.Outline = sl;
            display.SetSymbol(sf);
            if (buffer2 != null && !buffer2.IsEmpty)
            {
                display.DrawPolygon(buffer2);
            }
            display.SetSymbol(sf);
            foreach (IPolygon poly in polygons)
            {
                //display.DrawPolygon(poly);
            }

            TextSymbolClass ts = new TextSymbolClass();
            display.SetSymbol(ts);
            IPoint ps = new PointClass();
            foreach (IPolygon poly in polygons)
            {
                IGeometryCollection gc = poly as IGeometryCollection;
                for (int i = 0; i < gc.GeometryCount ; i++)
                {
                    IPointCollection pc = gc.get_Geometry(i) as IPointCollection;
                    for (int j = 0; j < pc.PointCount -1; j++)
                    {
                        ps = pc.get_Point(j);
                        display.DrawText(ps, /*i.ToString() + "," + */j.ToString());
                    }
                }
            }

            display.FinishDrawing();

        }
        public override void OnClick()
        {
            double distance = (double)m_application.GenPara["建筑物融合_融合距离"];

            IScreenDisplay display = m_application.MapControl.ActiveView.ScreenDisplay;
            IMap map = m_application.Workspace.Map;
            GLayerInfo info = m_application.Workspace.EditLayer;
            IFeatureLayer layer = (info.Layer as IFeatureLayer);
            if (layer == null)
            {
                return;
            }

            IFeatureClass fc = layer.FeatureClass;
            try
            {
                ISelectionSet set = (layer as IFeatureSelection).SelectionSet;
                IEnumIDs ids = set.IDs;
                int id = -1;
                object zv = (double)0;
                object miss = Type.Missing;
                object how = (int)esriConstructOffsetEnum.esriConstructOffsetSimple + (int)esriConstructOffsetEnum.esriConstructOffsetMitered;

                buffer = null;
                polygons.Clear();
                while ((id = ids.Next()) != -1)
                {
                    IFeature feature = fc.GetFeature(id);
                    if (buffer == null)
                    {
                        buffer = (feature.Shape as IPolygon);
                    }
                    else
                    {
                        IPolygon py = (feature.Shape as IPolygon);
                        if (py != null)
                            buffer = (buffer as ITopologicalOperator).Union(py) as IPolygon;
                    }
                }
                IPolygon4 org = buffer as IPolygon4;
                buffer = null;
                List<IPoint> intersectPoints = new List<IPoint>();
                IGeometryCollection gc = org.ConnectedComponentBag as IGeometryCollection;
                for (int i = 0; i < gc.GeometryCount; i++)
                {
                    IPolygon item = gc.get_Geometry(i) as IPolygon;
                    IPolygon bufferItem = GBuffer.Buffer(item, distance);
                    if (buffer == null)
                    {
                        buffer = bufferItem;
                        continue;
                    }
                    IGeometry ips = (bufferItem as ITopologicalOperator).Intersect(buffer, esriGeometryDimension.esriGeometry0Dimension);
                    if (ips is IPoint)
                    {
                        intersectPoints.Add(ips as IPoint);
                    }
                    else if (ips is IMultipoint)
                    {
                        for (int j = 0; j < (ips as IPointCollection).PointCount; j++)
                        {
                            intersectPoints.Add((ips as IPointCollection).get_Point(j));
                        }
                    }
                    else
                    {
                    }
                    buffer = (bufferItem as ITopologicalOperator).Union(buffer) as IPolygon;
                }
                polygons.Add(buffer);
                //polygons.Add((org as ITopologicalOperator).Buffer(distance) as IPolygon);
                //buffer.Generalize(distance * 0.1);
                IPoint hitPoint = new PointClass();
                double hitdistance = 0;
                int hitPartIndex = -1;
                int hitSegIndex = -1;
                bool hitRightSite = false;
                GeometryEnvironmentClass ge = new GeometryEnvironmentClass();

                foreach (IPoint ip in intersectPoints)
                {
                    if ((buffer as IHitTest).HitTest(ip, 0.1, esriGeometryHitPartType.esriGeometryPartVertex, hitPoint, ref hitdistance, ref hitPartIndex, ref hitSegIndex, ref hitRightSite))
                    {
                        ISegmentCollection hitRing = (buffer as IGeometryCollection).get_Geometry(hitPartIndex) as ISegmentCollection;
                        ISegment sn1 = hitRing.get_Segment(hitSegIndex);
                        ISegment sp1 = hitRing.get_Segment((hitSegIndex == 0) ? hitRing.SegmentCount - 1 : hitSegIndex - 1);
                        double hitAngel = ge.ConstructThreePoint(sp1.FromPoint, sn1.FromPoint, sn1.ToPoint);


                        if (Math.Abs(hitAngel) < Math.PI / 6)
                        {
                            ISegment s1 = null;
                            ISegment s2 = null;
                            if (sn1.Length > sp1.Length)
                            {
                                s1 = sn1;
                                s2 = hitRing.get_Segment(((hitSegIndex - 2) >= 0) ? (hitSegIndex - 2) : (hitRing.SegmentCount + hitSegIndex - 2));
                            }
                            else
                            {
                                s1 = sp1;
                                s2 = hitRing.get_Segment(((hitSegIndex + 1) < hitRing.SegmentCount) ? (hitSegIndex + 1) : (hitSegIndex + 1 - hitRing.SegmentCount));
                            }
                            PointClass movePoint = new PointClass();
                            movePoint.ConstructAngleIntersection(s1.FromPoint, (s1 as ILine).Angle, s2.FromPoint, (s2 as ILine).Angle);
                            IPoint[] mps = new IPoint[] { movePoint };
                            ge.ReplacePoints((hitRing as IPointCollection4), hitSegIndex, 1, ref mps);
                            if (Math.Abs((hitRing as IArea).Area) < distance)
                            {
                                (buffer as IGeometryCollection).RemoveGeometries(hitPartIndex, 1);
                            }
                        }
                    }
                }

                buffer2 = GBuffer.Buffer(buffer, -distance);
                polygons.Add(buffer);
            }
            catch
            {
                //m_application.EngineEditor.AbortOperation();
            }
            m_application.MapControl.Refresh();


        }
        private IColor ColorFromRGB(int r, int g, int b, bool nullColor)
        {
            RgbColorClass rgb = new RgbColorClass();
            rgb.Red = r;
            rgb.Green = g;
            rgb.Blue = b;
            rgb.NullColor = nullColor;
            return rgb;
        }
        private IPolygon AddPoints(IPolygon polygon, double minDistance)
        {
            IPolygon poly = (polygon as IClone).Clone() as IPolygon;
            IGeometryCollection gc = poly as IGeometryCollection;

            for (int i = 0; i < gc.GeometryCount; i++)
            {
                IRing ring = gc.get_Geometry(i) as IRing;
                IPointCollection pc = ring as IPointCollection;

                IPoint currentPoint = new PointClass();
                for (int j = 0; j < pc.PointCount; j++)
                {
                    IPoint p1 = pc.get_Point(j);
                    double x = p1.X;
                    double y = p1.Y;
                    if (j != 0)
                    {
                        int addCount = 0;

                        double distane = Math.Sqrt((x - currentPoint.X) * (x - currentPoint.X) + (y - currentPoint.Y) * (y - currentPoint.Y));
                        addCount = Convert.ToInt32(distane / minDistance);
                        if (addCount < 2)
                        {
                            addCount = 2;
                        }
                        for (int k = 1; k <= addCount; k++)
                        {
                            IPoint p = new PointClass();
                            p.X = (x * k + currentPoint.X * (addCount - k)) / addCount;
                            p.Y = (y * k + currentPoint.Y * (addCount - k)) / addCount;
                            pc.InsertPoints(j + k, 1, ref p);
                        }
                        j += addCount;

                    }
                    currentPoint.X = x;
                    currentPoint.Y = y;
                }
            }

            return poly;
        }


    }
}
