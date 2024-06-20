using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using SMGI.Common;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.GeneralEdit
{
    public class ReShapePolygonsWithFreeHand : SMGI.Common.SMGITool
    {
        INewLineFeedback lineFB;
        ISimpleLineSymbol sym;
        double genValue = 0.5;
        public ReShapePolygonsWithFreeHand()
        {
            sym = new SimpleLineSymbolClass();
            IRgbColor color = new RgbColorClass();
            color.Red = 255;
            color.Green = 0;
            color.Blue = 0;

            sym.Color = color;
            sym.Style = esriSimpleLineStyle.esriSLSSolid;
            sym.Width = 2;
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
            {
                return;
            }
            if (lineFB == null)
            {
                IActiveView view = m_Application.ActiveView;
                IMap map = view.FocusMap;
                IScreenDisplay screenDisplay = view.ScreenDisplay;
                lineFB = new NewLineFeedbackClass { Display = screenDisplay };
                lineFB.Symbol = sym as ISymbol;
                IPoint pt = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                lineFB.Start(pt);
            }
        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            if (lineFB != null)
            {
                lineFB.AddPoint(m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y));
            }
        }

        public override void OnKeyUp(int keyCode, int shift)
        {
            switch (keyCode)
            {
                case 27:
                    if (lineFB != null)
                    {
                        lineFB.Stop();
                        lineFB = null;
                        IActiveView view = m_Application.ActiveView;
                        view.Refresh();
                    }
                    break;
                case 32:
                    InputStringForm dlg = new InputStringForm();
                    dlg.Text = "输入化简参数";
                    dlg.Value = genValue.ToString();
                    dlg.AcceptKey = (key) => { return (key >= '0' && key <= '9') || key == '.'; };
                    try
                    {
                        if (dlg.ShowDialog() == DialogResult.OK)
                            genValue = double.Parse(dlg.Value);
                    }
                    catch
                    {
                    }
                    break;
            }
        }


        public override void OnDblClick()
        {
            if (lineFB != null)
            {
                var pl = lineFB.Stop();
                lineFB = null;

                if (pl == null || pl.IsEmpty)
                {
                    return;
                }

                (pl as IPolyline).Generalize(genValue);
                IActiveView view = m_Application.ActiveView;
                IMap map = view.FocusMap;
                IScreenDisplay screenDisplay = view.ScreenDisplay;

                ITopologicalOperator trackTopo = pl as ITopologicalOperator;
                trackTopo.Simplify();

                IRelationalOperator trackRel = pl as IRelationalOperator;

                ISpatialFilter sf = new SpatialFilter();
                sf.Geometry = pl;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                IPointCollection geoReshapePath = new PathClass();
                geoReshapePath.AddPointCollection(pl as IPointCollection);

                var editor = m_Application.EngineEditor;
                editor.StartOperation();
                for (int k = 0; k < map.LayerCount; k++)
                {
                    var item = map.get_Layer(k);
                    if (!item.Visible || !(item is IGeoFeatureLayer))
                    {
                        continue;
                    }

                    IGeoFeatureLayer geoFealyr = item as IGeoFeatureLayer;
                    IFeatureClass fc = geoFealyr.FeatureClass;
                    if (!(fc.ShapeType == esriGeometryType.esriGeometryPolygon))
                    {
                        continue;
                    }

                    IFeatureCursor pCursor = fc.Search(sf, false);
                    IFeature pFeature;
                    while ((pFeature = pCursor.NextFeature()) != null)
                    {
                        IPolygon4 pg = pFeature.Shape as IPolygon4;
                        IGeometryBag tempGeoBag = pg.ConnectedComponentBag;
                        //如果该要素的几何为多部件要素，则循环遍历该多部件要素
                        if ((tempGeoBag as IGeometryCollection).GeometryCount > 1)
                        {
                            IGeometryCollection pgCol = tempGeoBag as IGeometryCollection;
                            for (int j = 0; j < pgCol.GeometryCount; j++)
                            {
                                IGeometry geoItem = pgCol.get_Geometry(j);
                                if (!trackRel.Disjoint(geoItem))
                                {
                                    IGeometryCollection geoItemCol = geoItem as IGeometryCollection;
                                    IPolygon pgItem = new PolygonClass();
                                    for (int i = 0; i < geoItemCol.GeometryCount; i++)
                                    {
                                        IRing r = geoItemCol.get_Geometry(i) as IRing;
                                        (pgItem as IGeometryCollection).AddGeometry(r);
                                        if (!trackRel.Disjoint(pgItem))
                                        {
                                            try
                                            {
                                                r.Reshape(geoReshapePath as IPath);
                                            }
                                            catch (Exception)
                                            {
                                                (pgItem as IGeometryCollection).RemoveGeometries(0, (pgItem as IGeometryCollection).GeometryCount - 1);
                                                continue;
                                            }
                                        }
                                        (pgItem as IGeometryCollection).RemoveGeometries(0, (pgItem as IGeometryCollection).GeometryCount - 1);
                                    }
                                }
                            }
                        }
                        else
                        {
                            IGeometryCollection pgCol = pg as IGeometryCollection;
                            IPolygon pgItem = new PolygonClass();
                            for (int i = 0; i < pgCol.GeometryCount; i++)
                            {
                                IRing r = pgCol.get_Geometry(i) as IRing;
                                (pgItem as IGeometryCollection).AddGeometry(r);
                                if (!trackRel.Disjoint(pgItem))
                                {
                                    try
                                    {
                                        r.Reshape(geoReshapePath as IPath);
                                    }
                                    catch (Exception)
                                    {
                                        (pgItem as IGeometryCollection).RemoveGeometries(0, (pgItem as IGeometryCollection).GeometryCount - 1);
                                        continue;
                                    }
                                }
                                (pgItem as IGeometryCollection).RemoveGeometries(0, (pgItem as IGeometryCollection).GeometryCount - 1);
                            }
                        }
                        pFeature.Shape = pg;
                        pFeature.Store();
                    }
                    Marshal.ReleaseComObject(pCursor);
                }
                editor.StopOperation("自由修图斑");
                view.Refresh();
            }
        }
    }
}
