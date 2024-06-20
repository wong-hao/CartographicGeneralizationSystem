using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.CartoUI;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;

namespace SMGI.Common
{
    class MapControlManager
    {
        AxMapControl ctrl;
        IPluginHost frm;
        IComPropertySheet myPropertySheet;
        GApplication app;

        SimpleLineSymbolClass featureLineSymbol;
        SimpleLineSymbolClass repLineSymbol;
        SimpleMarkerSymbolClass feautureMakerSymbol;
        SimpleMarkerSymbolClass repMakerSymbol;
        SimpleFillSymbolClass sfs;

        internal MapControlManager(GApplication app)
        {
            this.app = app;
            ctrl = app.MapControl;
            
            frm = app.MainForm;
            CreateNewPropertyPage();
            ctrl.OnMouseDown += new IMapControlEvents2_Ax_OnMouseDownEventHandler(ctrl_OnMouseDown);
            ctrl.OnMouseMove += new IMapControlEvents2_Ax_OnMouseMoveEventHandler(ctrl_OnMouseMove);
            //ctrl.OnMouseUp += new IMapControlEvents2_Ax_OnMouseUpEventHandler(ctrl_OnMouseUp);
            ctrl.OnAfterDraw += new IMapControlEvents2_Ax_OnAfterDrawEventHandler(ctrl_OnAfterDraw);
            initSymbol();
        }

        void initSymbol()
        {
            featureLineSymbol = new SimpleLineSymbolClass
            {
                Color = new RgbColorClass { Red = 0, Green = 0, Blue = 255 },
                Width = 1,
            };
            repLineSymbol = new SimpleLineSymbolClass
            {
                Color = new RgbColorClass { Red = 255, Green = 0, Blue = 0 },
                Width = 1,
            };
            feautureMakerSymbol = new SimpleMarkerSymbolClass
            {
                Size = 6,
                Color = new RgbColorClass { Red = 0, Green = 0, Blue = 255 },
                Style = esriSimpleMarkerStyle.esriSMSSquare,
            };
            repMakerSymbol = new SimpleMarkerSymbolClass
            {
                Size = 6,
                Color = new RgbColorClass { Red = 255, Green = 0, Blue = 0 },
                Style = esriSimpleMarkerStyle.esriSMSCircle,
            };
            sfs = new SimpleFillSymbolClass
            {
                Color = new RgbColorClass { Red = 100, Green = 100, Blue = 100 },
                Style = esriSimpleFillStyle.esriSFSNull
            };
        }

        void ctrl_OnAfterDraw(object sender, IMapControlEvents2_OnAfterDrawEvent e)
        {
            if (app.Workspace == null || app.Workspace.SelectedRepresentations == null)
            {
                return;
            }
            if ((e.viewDrawPhase & 4) == 0)
            {
                return;
            }
            app.Workspace.LayerManager.UpdateCurrentRepresentation();

            var dis = new SimpleDisplayClass
            {
                DisplayTransformation = app.ActiveView.ScreenDisplay.DisplayTransformation,
            };
            dis.DisplayTransformation.ReferenceScale = 0;
            var env = app.ActiveView.Extent;
            env.Expand(1.1, 1.1, true);
            //dis.ClipGeometry = app.ActiveView.Extent;
            dis.StartDrawing((e.display as IDisplay).hDC, -1);

            foreach (var rep in app.Workspace.SelectedRepresentations)
            {
                #region draw feature shape

                using (var d = new FastDrawer(dis, true))
                {
                    var shape = rep.Feature.ShapeCopy;
                    (shape as ITopologicalOperator).Clip(env);
                    switch (shape.GeometryType)
                    {
                        case esriGeometryType.esriGeometryPoint:
                            d.Draw(shape, feautureMakerSymbol);
                            break;
                        case esriGeometryType.esriGeometryPolygon:
                            sfs.Outline = featureLineSymbol;
                            d.Draw(shape, sfs);
                            break;
                        case esriGeometryType.esriGeometryPolyline:
                            d.Draw(shape, featureLineSymbol);
                            break;
                        default:
                            break;
                    }


                    if (false && shape is IPointCollection)
                    {
                        for (int i = 0; i < (shape as IPointCollection).PointCount; i++)
                        {
                            d.Draw((shape as IPointCollection).Point[i], feautureMakerSymbol);
                        }
                    }
                }
                #endregion

                if (true) //画
                {
                    #region draw rep shape or graphics


                    if (rep.Graphics == null)
                    {
                        var shape = rep.ShapeCopy;
                        (shape as ITopologicalOperator).Clip(env);
                        using (var d = new FastDrawer(dis, true))
                        {
                            switch (shape.GeometryType)
                            {
                                case esriGeometryType.esriGeometryPoint:
                                    d.Draw(shape, repMakerSymbol);
                                    break;
                                case esriGeometryType.esriGeometryPolygon:
                                    sfs.Outline = repLineSymbol;
                                    d.Draw(shape, sfs);
                                    break;
                                case esriGeometryType.esriGeometryPolyline:
                                    d.Draw(shape, repLineSymbol);
                                    break;
                                default:
                                    break;
                            }

                            
                            if (false && shape is IPointCollection)
                            {
                                for (int i = 0; i < (shape as IPointCollection).PointCount; i++)
                                {
                                    var pt = (shape as IPointCollection).Point[i];
                                    d.Draw(pt, repMakerSymbol);
                                }
                            }
                        }
                    }
                    else
                    {
                        int id;
                        IGeometry geo = null;
                        var g = rep.Graphics;
                        g.ResetGeometry();
                        while (true)
                        {
                            g.NextGeometry(out id, out geo);
                            if (geo == null)
                                break;

                            var shape = (geo as IClone).Clone() as IGeometry;
                            (shape as ITopologicalOperator).Clip(env);
                            using (var d = new FastDrawer(dis, true))
                            {
                                switch (shape.GeometryType)
                                {
                                    case esriGeometryType.esriGeometryPoint:
                                        d.Draw(shape, repMakerSymbol);
                                        break;
                                    case esriGeometryType.esriGeometryPolygon:
                                        sfs.Outline = repLineSymbol;
                                        d.Draw(shape, sfs);
                                        break;
                                    case esriGeometryType.esriGeometryPolyline:
                                        d.Draw(shape, repLineSymbol);
                                        break;
                                    default:
                                        break;
                                }

                                if (false && shape is IPointCollection)
                                {
                                    for (int i = 0; i < (shape as IPointCollection).PointCount; i++)
                                    {
                                        var pt = (shape as IPointCollection).Point[i];
                                        d.Draw(pt, repMakerSymbol);
                                    }
                                }

                            }

                        }

                    }
                    #endregion
                }

            }

            dis.FinishDrawing();

        }

        private void CreateNewPropertyPage()
        {
            myPropertySheet = new ComPropertySheetClass();
            myPropertySheet.Title = "图形元素符号";
            myPropertySheet.HideHelpButton = true;
            //(myPropertySheet as ESRI.ArcGIS.SystemUI.IComPropertySheetEvents_Event).
            myPropertySheet.ClearCategoryIDs();
            myPropertySheet.AddCategoryID(new UIDClass()); //a dummy empty UID
            myPropertySheet.AddPage(new TextElementPropertyPageClass());
            myPropertySheet.AddPage(new FillShapeElementPropertyPageClass());
        }
        void ctrl_OnMouseUp(object sender, IMapControlEvents2_OnMouseUpEvent e)
        {
            IGraphicsContainer gc = ctrl.Map as IGraphicsContainer;
            IGraphicsContainerSelect gcs = ctrl.Map as IGraphicsContainerSelect;
            IPoint pt = ctrl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);
            bool hitElement = false;

            if (e.button == 2)//右击
            {
                //是否有选中
                int icount = gcs.ElementSelectionCount;
                //检测是否右击到某个Element
                IEnumElement eu = gc.LocateElements(pt, 1);
                if (eu != null)
                {
                    eu.Reset();
                    if (eu.Next() != null) { hitElement = true; }
                }
                if (hitElement && icount > 0)
                {
                    ISet propertyObjects = new SetClass();
                    propertyObjects.Add(gcs.SelectedElement(0));
                    if (myPropertySheet.CanEdit(propertyObjects))
                        myPropertySheet.EditProperties(propertyObjects, ctrl.hWnd);
                    ctrl.Refresh(esriViewDrawPhase.esriViewGraphicSelection, null, null);
                }

            }

        }

        void ctrl_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            string status = string.Format("{0}, {1}  {2}", e.mapX.ToString("#######.0000"), e.mapY.ToString("#######.0000"), ctrl.MapUnits.ToString().Substring(4));
            frm.ShowStatus(status);
            
        }

        void ctrl_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            if (e.button != 4)
            {
                return;
            }
            ctrl.Pan();
        }
    }
}
