using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.EmergencyMap
{

    public sealed class AnchorMoveTool : SMGITool
    {
        
        public AnchorMoveTool()
        {
            
        }
        
        public override bool Deactivate()
        {
            selected = false;
            LabelClass.Instance.ActiveDefaultLayer();
            return  base.Deactivate();
            
        }
        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null;
            }
        }
        

        #region Overridden Class Methods

        
        IGraphicsContainerSelect gs = null;
        IActiveView act = null;
        public override void OnClick()
        {
            
            act = m_Application.ActiveView;
            
            gc = LabelClass.Instance.GraphicsLayer as IGraphicsContainer;
            gs = gc as IGraphicsContainerSelect;
            
        }
        private void BuildingMergCmd_AfterDraw(IDisplay Display, esriViewDrawPhase phase)
        {
           

        }
        IElement anchorEle = null;
        IGeometry anchorGeo = null;
       

        ILineElement line1 = null;
        IGeometry geoLine1 = null;
        ILineElement line2 = null;
        IGeometry geoLine2 = null;
        IPolygonElement polygon = null;
        IGeometry geoPolygon = null;
        ITextElement txtElement = null;
        IPoint txtGeometry = null;

        ITextElement txtElementDown = null;
        IPoint txtGeometryDown = null;

        ILineElement lineSep = null;
        IGeometry geoLineSep = null;

        IGroupElement groupEle = null;
        IGraphicsContainer gc = null;
        bool selected = false;
        private void ElementInit0()
        {
            
            for (int i = 0; i < groupEle.ElementCount; i++)
            {
                #region
                IElement ee = (groupEle.get_Element(i) as IClone).Clone() as IElement;
                switch ((ee as IElementProperties).Name)
                {
                    case "锚点":
                        anchorEle =ee as IElement;
                        anchorGeo = (ee.Geometry as IClone).Clone() as IGeometry;
                        if (anchorGeo is IPolygon)
                        {
                            center = (anchorGeo as IArea).Centroid as IPoint;
                        }
                        else if (anchorGeo is IPolyline)
                        {
                            var gcs = anchorGeo as IGeometryCollection;
                            var pcs = anchorGeo as IPointCollection;
                            if (pcs.PointCount != 2)
                            {
                                center = pcs.get_Point(1);
                            }
                            else
                            {
                                (anchorGeo as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);
                            }

                        }
                        break;
                    case "分割线":
                        lineSep = ee as ILineElement; ;
                        geoLineSep = (ee.Geometry as IClone).Clone() as IGeometry; ;
                        break;
                    case "锚线":
                        line1 = ee as ILineElement;
                        geoLine1 = (ee.Geometry as IClone).Clone() as IGeometry;
                        break;
                    case "连接线":
                        line2 = ee as ILineElement;
                        geoLine2 = (ee.Geometry as IClone).Clone() as IGeometry;
                        break;
                    case "内容框":
                        polygon = ee as IPolygonElement;
                        geoPolygon = (ee.Geometry as IClone).Clone() as IGeometry;
                        break;
                    case "文本":
                        txtElement = ee as ITextElement;
                        txtGeometry = (ee.Geometry as IClone).Clone() as IPoint;
                        break;
                    case "下标文本":
                        txtElementDown = ee as ITextElement;
                        txtGeometryDown = (ee.Geometry as IClone).Clone() as IPoint;
                        break;
                    default:
                        break;

                }
                #endregion
            }

            
          
           
            IGraphicsContainer pGraphicsContainer = gc as IGraphicsContainer;
            IGraphicsContainerSelect pGraphicsContainerSelect = gc as IGraphicsContainerSelect;
            IEnumElement pEnumElemen = pGraphicsContainerSelect.SelectedElements;
            gc.Reset();
            IElement pElement = null;
            while ((pElement = gc.Next()) != null)
            {
                if ((pElement as IElementProperties).CustomProperty == null)
                    continue;
                string guid = (pElement as IElementProperties).CustomProperty.ToString();
                if(guid==string.Empty)
                {
                    continue;
                }

                try
                {

                    bool flag = false;
                    for (int i = 0; i < groupEle.ElementCount; i++)
                    {
                        string guid_ = (groupEle.get_Element(i) as IElementProperties).CustomProperty.ToString();
                        if (guid == guid_)
                        {
                            flag = true;
                            break;

                        }
                    }
                    if (flag)
                    {
                        pGraphicsContainer.DeleteElement(pElement as IElement);
                    }
                   
                }
                catch
                {
                }
            }
          
            pGraphicsContainer.DeleteElement(groupEle as IElement);
            string name=(groupEle as IElementProperties).Name;
            string type=(groupEle as IElementProperties).Type;
            groupEle = new GroupElementClass();
            (groupEle as IElementProperties).Name = name;
            (groupEle as IElementProperties).Type = type;
            gc.AddElement(line1 as IElement,0);
            gc.AddElement(line2 as IElement, 0);
            gc.AddElement(anchorEle as IElement, 0);
            gc.AddElement(polygon as IElement, 0);
            gc.AddElement(txtElement as IElement, 0);
            if (txtElementDown != null)
                gc.AddElement(txtElementDown as IElement, 0);
            if (lineSep != null)
                gc.AddElement(lineSep as IElement, 0);

            groupEle.AddElement(line1 as IElement);
            groupEle.AddElement(line2 as IElement);
            groupEle.AddElement(anchorEle as IElement);
            groupEle.AddElement(polygon as IElement);
            groupEle.AddElement(txtElement as IElement);
            if (txtElementDown != null)
                groupEle.AddElement(txtElementDown as IElement);
            if (lineSep != null)
                groupEle.AddElement(lineSep as IElement);
            gc.AddElement(groupEle as IElement, 0);
            gs.UnselectAllElements();
            act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, act.Extent);
            
           
        }
        private void ElementInit()
        {
           

        }
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            
            if (Button != 1)
                return;
           
            IPoint pp = act.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
             selected = false;
            IEnvelope en = new EnvelopeClass();
            en.PutCoords(pp.X - 0.5, pp.Y - 0.5, pp.X + 0.5, pp.Y + 0.5);

            IEnumElement pEnumElement =gc.LocateElements(pp, 1);
            if (pEnumElement == null)
                return;
            pEnumElement.Reset();
            IElement pElement = pEnumElement.Next();
            Marshal.ReleaseComObject(pEnumElement);
            if (pElement == null)
            {
                return;
            }
            if (!(pElement is IGroupElement))
            {
                pElement = null;
                return;
            }
            groupEle = pElement as IGroupElement;
            if ((groupEle as IElementProperties).Type == LabelType.BallCallout.ToString())
            {
                pElement = null;
                groupEle = null;
                return;
            }
            groupEle = pElement as IGroupElement;
            ElementInit();

            act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, act.Extent);
            for (int i = 0; i < groupEle.ElementCount; i++)
            {
                #region
                IElement ee = groupEle.get_Element(i);
                switch ((ee as IElementProperties).Name)
                {
                    case "锚点":
                        anchorEle = ee;
                        anchorGeo = (ee.Geometry as IClone).Clone() as IGeometry;
                        if (!(en as IRelationalOperator).Disjoint(anchorGeo.Envelope))
                        {
                            selected = true;
                        }

                        if (anchorGeo is IPolygon)
                        {
                            center = (anchorGeo as IArea).Centroid as IPoint;
                        }
                        else if (anchorGeo is IPolyline)
                        {
                            var gcs=anchorGeo as IGeometryCollection ;
                            var pcs = anchorGeo as IPointCollection;
                            if (pcs.PointCount != 2)
                            {
                                center = pcs.get_Point(1);
                            }
                            else
                            {
                                (anchorGeo as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);
                            }

                        }
                        break;
                    case "分割线":
                        lineSep =  ee as ILineElement;;
                        geoLineSep =  (ee.Geometry as IClone).Clone() as IGeometry;;
                        break;
                    case "锚线":
                        line1 = ee as ILineElement;
                        geoLine1 = (ee.Geometry as IClone).Clone() as IGeometry;
                        break;
                    case "连接线":
                        line2 = ee as ILineElement;
                        geoLine2 = (ee.Geometry as IClone).Clone() as IGeometry;
                        break;
                    case "内容框":
                        polygon = ee as IPolygonElement;
                        geoPolygon = (ee.Geometry as IClone).Clone() as IGeometry;
                        break;
                    case "文本":
                        txtElement = ee as ITextElement;
                        txtGeometry = (ee.Geometry as IClone).Clone() as IPoint;
                        break;
                    case "下标文本":
                        txtElementDown = ee as ITextElement;
                        txtGeometryDown = (ee.Geometry as IClone).Clone() as IPoint;
                        break;
                    default:
                        break;

                }
                #endregion
            }
            if (selected)
            {
                gs.SelectElement(anchorEle);
            }
           
        }
        IPoint center = new PointClass();
        double labelLineLens = 10;
        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
           
            if (!selected)
                return;
            try
            {
                IPoint ct = act.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                double dx = ct.X - center.X;
                double dy = ct.Y - center.Y;
                //锚点方向
                ILine Dirline0 = new LineClass();
                Dirline0.FromPoint = ((geoLine1 as IClone).Clone() as IPolyline).FromPoint;
                Dirline0.ToPoint = ((geoLine1 as IClone).Clone() as IPolyline).ToPoint;
                ILine Dirline1 = new LineClass();
                Dirline1.FromPoint = ct;
                Dirline1.ToPoint = ((geoLine1 as IClone).Clone() as IPolyline).ToPoint;

                //移动锚点
                var trans = (anchorGeo as IClone).Clone() as ITransform2D;
                trans.Move(dx, dy);
                trans.Rotate(ct, Dirline1.Angle - Dirline0.Angle);
                anchorEle.Geometry = trans as IGeometry;
               // gc.UpdateElement(anchorEle as IElement);
                //移动锚线
                IPolyline anchorLine = (geoLine1 as IClone).Clone() as IPolyline;

                PolylineClass polyline = new PolylineClass();
             
                IPoint pptfrom = ct;
                polyline.AddPoint(new PointClass { X = pptfrom.X, Y = pptfrom.Y });
                polyline.AddPoint(anchorLine.ToPoint);
                (line1 as IElement).Geometry = polyline as IGeometry;
                //gc.UpdateElement(line1 as IElement);


                groupEle.ClearElements();

                groupEle.AddElement(line1 as IElement);
                groupEle.AddElement(line2 as IElement);
                groupEle.AddElement(anchorEle as IElement);
                groupEle.AddElement(polygon as IElement);
                groupEle.AddElement(txtElement as IElement);
                if (txtElementDown != null)
                    groupEle.AddElement(txtElementDown as IElement);
                if (lineSep != null)
                    groupEle.AddElement(lineSep as IElement);
                gc.UpdateElement(groupEle as IElement);


                act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, act.Extent);
            }
            catch
            {
            }
        }

        
        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
           
            if (!selected)
                return;
            try
            {
                selected = false;
                gs.UnselectElement(anchorEle);
                act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, act.Extent);
            }
            catch
            {
            }
        }
        #endregion
    }
}
