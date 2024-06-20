using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using SMGI.Common;

namespace SMGI.Plugin.EmergencyMap
{

    public sealed class LabelElementRotateTool : SMGITool
    {
        

        

        public LabelElementRotateTool()
        {
           
        }
        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null;
            }
        }
        #region Overridden Class Methods

        /// <summary>
        /// Occurs when this tool is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
         

        IActiveView act = null;
        public override void OnClick()
        {
            act = m_Application.ActiveView;
            gc = LabelClass.Instance.GraphicsLayer as IGraphicsContainer;
           
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

        ILineElement lineSep = null;
        IGeometry geoLineSep = null;

        IGroupElement groupEle = null;
        IGraphicsContainer gc = null;
        ITextElement txtElementDown = null;
        IPoint txtGeometryDown = null;
        bool checkSep = false;
        double txtStep = 0;
        private void IntiPrams()
        {
             anchorEle = null;
             anchorGeo = null;


             line1 = null;
             geoLine1 = null;
             line2 = null;
             geoLine2 = null;
             polygon = null;
             geoPolygon = null;
             txtElement = null;
             txtGeometry = null;

             lineSep = null;
             geoLineSep = null;

             groupEle = null;
           
             txtElementDown = null;
             txtGeometryDown = null;
             checkSep = false;
             txtStep = 0;
        }
        private void ElementInit0()
        {

            for (int i = 0; i < groupEle.ElementCount; i++)
            {
                #region
                IElement ee = (groupEle.get_Element(i) as IClone).Clone() as IElement;
                switch ((ee as IElementProperties).Name)
                {
                    case "锚点":
                        anchorEle = ee as IElement;
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
                if (guid == string.Empty)
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
            string name = (groupEle as IElementProperties).Name;
            string type = (groupEle as IElementProperties).Type;
            groupEle = new GroupElementClass();
            (groupEle as IElementProperties).Name = name;
            (groupEle as IElementProperties).Type = type;
            gc.AddElement(line1 as IElement, 0);
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
          
            act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, act.Extent);


        }
        private void ElementInit()
        {

          


        }
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            IntiPrams();
            if (Button != 1)
                return;
            IPoint pp = act.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
       
            IEnumElement pEnumElement = gc.LocateElements(pp, 1);
            if (pEnumElement == null)
                return;
            pEnumElement.Reset();
            IElement pElement =null;
            IElement element = null;
            while ((element = pEnumElement.Next()) != null)
            {
                bool intertSet = false;
                groupEle = element as IGroupElement;
                if (groupEle==null)
                {
                    continue;
                }
                for (int i = 0; i < groupEle.ElementCount; i++)
                {
                    
                    IElement ee = groupEle.get_Element(i);
                    string name = (ee as IElementProperties).Name;
                    if (name == "内容框")
                    {
                        intertSet = !(pp as IRelationalOperator).Disjoint(ee.Geometry);
                        break;
                    } 
                }
                if (intertSet)
                {
                    pElement = element;
                    break;
                }
            }
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
                        checkSep = true;
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
                        labelLineLens = (geoLine2 as IPolyline).Length;
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
            txtStep = 0;
            if (txtGeometryDown != null)
            {
                txtStep = (txtGeometry.Y - txtGeometryDown.Y) * 0.5;
            }

        }
        IPoint center = new PointClass();
        double labelLineLens = 10;
        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {

            if (groupEle == null)
                return;
            if (geoLine1 == null)
                return;
            IPolyline anchorline = (geoLine1 as IClone).Clone() as IPolyline;
            ILine line0 = new LineClass();
            line0.FromPoint = anchorline.FromPoint;
            line0.ToPoint = anchorline.ToPoint ;


            IPoint ct = act.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            
            ILine line = new LineClass();
            line.FromPoint = center;
            line.ToPoint = ct;

            double dAngle = line.Angle - line0.Angle;
            //旋转锚点
            var trans = (anchorGeo as IClone).Clone() as ITransform2D;
            trans.Rotate(center, dAngle);
            anchorEle.Geometry = trans as IGeometry;
           // gc.UpdateElement(anchorEle as IElement);
            //旋转锚线
            ITransform2D trans1 = (geoLine1 as IClone).Clone() as ITransform2D;
            trans1.Rotate(center, dAngle);
            PolylineClass polyline = new PolylineClass();
            IPoint pptfrom = (trans1 as IPolyline).FromPoint;
        
          
            polyline.AddPoint(new PointClass { X = pptfrom.X, Y = pptfrom.Y });
            polyline.AddPoint(new PointClass { X = ct.X, Y = ct.Y });

            (line1 as IElement).Geometry = polyline as IGeometry;
          //  gc.UpdateElement(line1 as IElement);
           
            
            //旋转内容窗体
            #region
            //
            if (Math.Abs(line.Angle) < Math.PI / 2)
            {
                if (Math.Abs(line.Angle) > Math.PI / 2 * 0.8)//72度
                {
                    double reg = line.Angle / Math.Abs(line.Angle);
                    PolylineClass polyline1 = new PolylineClass();
                    polyline1.AddPoint(new PointClass { X = ct.X, Y = ct.Y });
                    polyline1.AddPoint(new PointClass { X = ct.X, Y = ct.Y + labelLineLens * reg });
                    (line2 as IElement).Geometry = polyline1 as IGeometry;
                   // gc.UpdateElement(line2 as IElement);

                    double dx = polyline1.ToPoint.X - (geoPolygon.Envelope.XMax + geoPolygon.Envelope.XMin) * 0.5;
                    double dy = polyline1.ToPoint.Y - (reg < 0 ? (geoPolygon.Envelope.YMax) : (geoPolygon.Envelope.YMin));
                    ITransform2D polygonTran = (geoPolygon as IClone).Clone() as ITransform2D;
                    polygonTran.Move(dx, dy);
                    (polygon as IElement).Geometry = polygonTran as IGeometry;
                   // gc.UpdateElement(polygon as IElement);
                    if (checkSep)
                    {
                         
                        IPoint lineCt = new PointClass();
                        (geoLineSep as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, lineCt);

                        dx = (polygonTran as IArea).Centroid.X - lineCt.X;
                        dy = (polygonTran as IArea).Centroid.Y - lineCt.Y;
                        ITransform2D sepLineTran = (geoLineSep as IClone).Clone() as ITransform2D;
                        sepLineTran.Move(dx, dy);
                        (lineSep as IElement).Geometry = sepLineTran as IGeometry;
                       // gc.UpdateElement(lineSep as IElement);

                    }
                }
                else
                {
                    //2
                    PolylineClass polyline1 = new PolylineClass();
                    polyline1.AddPoint(new PointClass { X = ct.X, Y = ct.Y });
                    polyline1.AddPoint(new PointClass { X = ct.X + labelLineLens, Y = ct.Y });
                    (line2 as IElement).Geometry = polyline1 as IGeometry;
                   // gc.UpdateElement(line2 as IElement);

                    double dx = polyline1.ToPoint.X - (geoPolygon.Envelope.XMin);
                    double dy = polyline1.ToPoint.Y - (geoPolygon.Envelope.YMin + geoPolygon.Envelope.YMax) * 0.5;
                    ITransform2D polygonTran = (geoPolygon as IClone).Clone() as ITransform2D;
                    polygonTran.Move(dx, dy);
                    (polygon as IElement).Geometry = polygonTran as IGeometry;
                    //gc.UpdateElement(polygon as IElement);
                    if (checkSep)
                    {

                        IPoint lineCt = new PointClass();
                        (geoLineSep as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, lineCt);

                        dx = (polygonTran as IArea).Centroid.X - lineCt.X;
                        dy = (polygonTran as IArea).Centroid.Y - lineCt.Y;
                        ITransform2D sepLineTran = (geoLineSep as IClone).Clone() as ITransform2D;
                        sepLineTran.Move(dx, dy);
                        (lineSep as IElement).Geometry = sepLineTran as IGeometry;
                        //gc.UpdateElement(lineSep as IElement);

                    }
                }
            }
            else
            {
                if (Math.Abs(line.Angle) < Math.PI / 2 + Math.PI / 2 * 0.2)//72度
                {
                    double reg = line.Angle / Math.Abs(line.Angle);
                    PolylineClass polyline1 = new PolylineClass();
                    polyline1.AddPoint(new PointClass { X = ct.X, Y = ct.Y });
                    polyline1.AddPoint(new PointClass { X = ct.X, Y = ct.Y + labelLineLens * reg });
                    (line2 as IElement).Geometry = polyline1 as IGeometry;
                   // gc.UpdateElement(line2 as IElement);


                    double dx = polyline1.ToPoint.X - (geoPolygon.Envelope.XMax + geoPolygon.Envelope.XMin) * 0.5;
                    double dy = polyline1.ToPoint.Y - (reg < 0 ? (geoPolygon.Envelope.YMax) : (geoPolygon.Envelope.YMin));
                    ITransform2D polygonTran = (geoPolygon as IClone).Clone() as ITransform2D;
                    polygonTran.Move(dx, dy);
                    (polygon as IElement).Geometry = polygonTran as IGeometry;
                   // gc.UpdateElement(polygon as IElement);
                    if (checkSep)
                    {

                        IPoint lineCt = new PointClass();
                        (geoLineSep as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, lineCt);

                        dx = (polygonTran as IArea).Centroid.X - lineCt.X;
                        dy = (polygonTran as IArea).Centroid.Y - lineCt.Y;
                        ITransform2D sepLineTran = (geoLineSep as IClone).Clone() as ITransform2D;
                        sepLineTran.Move(dx, dy);
                        (lineSep as IElement).Geometry = sepLineTran as IGeometry;
                       // gc.UpdateElement(lineSep as IElement);

                    }
                }
                else
                {
                    //2
                    PolylineClass polyline1 = new PolylineClass();
                    polyline1.AddPoint(new PointClass { X = ct.X, Y = ct.Y });
                    polyline1.AddPoint(new PointClass { X = ct.X - labelLineLens, Y = ct.Y });
                    (line2 as IElement).Geometry = polyline1 as IGeometry;
                   // gc.UpdateElement(line2 as IElement);


                    double dx = polyline1.ToPoint.X - (geoPolygon.Envelope.XMin) - geoPolygon.Envelope.Width;
                    double dy = polyline1.ToPoint.Y - (geoPolygon.Envelope.YMin + geoPolygon.Envelope.YMax) * 0.5;
                    ITransform2D polygonTran = (geoPolygon as IClone).Clone() as ITransform2D;
                    polygonTran.Move(dx, dy);
                    (polygon as IElement).Geometry = polygonTran as IGeometry;
                    //gc.UpdateElement(polygon as IElement);
                    if (checkSep)
                    {

                        IPoint lineCt = new PointClass();
                        (geoLineSep as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, lineCt);

                        dx = (polygonTran as IArea).Centroid.X - lineCt.X;
                        dy = (polygonTran as IArea).Centroid.Y - lineCt.Y;
                        ITransform2D sepLineTran = (geoLineSep as IClone).Clone() as ITransform2D;
                        sepLineTran.Move(dx, dy);
                        (lineSep as IElement).Geometry = sepLineTran as IGeometry;
                       // gc.UpdateElement(lineSep as IElement);

                    }
                }
            }
            #endregion

            IPoint txtTopoint = ((polygon as IElement).Geometry as IArea).Centroid;

            (txtElement as IElement).Geometry = new PointClass { X = txtTopoint.X, Y = txtTopoint.Y + txtStep };
            if (txtElementDown != null)
            {
                (txtElementDown as IElement).Geometry = new PointClass { X = txtTopoint.X, Y = txtTopoint.Y - txtStep }; ;
            }
            if ((txtElement as ISymbolCollectionElement).VerticalAlignment == ESRI.ArcGIS.Display.esriTextVerticalAlignment.esriTVACenter)
            {
                //纠正中心点位置
                IPolygon outline = new PolygonClass();
                (txtElement as IElement).QueryOutline(act.ScreenDisplay, outline);
                IPoint txtCenter = (outline as IArea).Centroid;
                (txtElement as ITransform2D).Move(txtTopoint.X - txtCenter.X, txtTopoint.Y - txtCenter.Y);
            }

           // gc.UpdateElement(txtElement as IElement);
            groupEle.ClearElements();
          
            groupEle.AddElement(line1 as IElement);
            groupEle.AddElement(line2 as IElement);
            groupEle.AddElement(anchorEle as IElement);
            groupEle.AddElement(polygon as IElement);
            groupEle.AddElement(txtElement as IElement);
            if (checkSep)
            {
                groupEle.AddElement(lineSep as IElement);
            }
            if (txtElementDown != null)
            {
                groupEle.AddElement(txtElementDown as IElement);
            }
            gc.UpdateElement(groupEle as IElement);
            act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }
        public override bool Deactivate()
        {
            LabelClass.Instance.ActiveDefaultLayer();
            groupEle = null;
            checkSep = false;
            return base.Deactivate();
        }
        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            checkSep = false;
            groupEle = null;
            
        }
        #endregion
    }
}
