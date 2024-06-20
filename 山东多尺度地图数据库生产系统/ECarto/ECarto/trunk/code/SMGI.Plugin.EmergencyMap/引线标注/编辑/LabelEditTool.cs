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
using SMGI.Plugin.EmergencyMap.LabelSym;

namespace SMGI.Plugin.EmergencyMap
{

    public sealed class LabelEditTool : SMGITool
    {
        ControlsSelectToolClass currentTool = null;
        public LabelEditTool()
        {
            currentTool = new ControlsSelectToolClass();
        }
        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null;
            }
        }  

        

        #region Overridden Class Methods
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
            currentTool.OnCreate(m_Application.MapControl.Object);
        }
        public override void Refresh(int hdc)
        {
            currentTool.Refresh(hdc);
        }
        public override int Cursor
        {
            get
            {
                return currentTool.Cursor;
            }

        }
        public override bool Deactivate()
        {
            LabelClass.Instance.ActiveDefaultLayer();
            return currentTool.Deactivate();
        }
        IGraphicsContainerSelect gs = null;
        IActiveView act = null;
        public override void OnClick()
        {
             
            currentTool.OnClick();
            act = m_Application.ActiveView;
            gc = LabelClass.Instance.GraphicsLayer as IGraphicsContainer;
            gs = gc as IGraphicsContainerSelect;
            
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
       
      
        IPoint currentPoint = null;
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
            if (Button == 4)
                return;
            currentPoint = act.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            currentTool.OnMouseDown(Button, Shift, X, Y);
        }

        public override void OnKeyDown(int keyCode, int shift)
        {
            if (keyCode == 46)
            {
                try
                {
                    IGraphicsContainer pGraphicsContainer = gc as IGraphicsContainer;
                    IGraphicsContainerSelect pGraphicsContainerSelect = gc as IGraphicsContainerSelect;
                    //遍历Element
                    IEnumElement pEnumElemen = pGraphicsContainerSelect.SelectedElements;
                    pEnumElemen.Reset();
                    IElement pElement;
                    while ((pElement = pEnumElemen.Next()) != null)
                    {
                        pGraphicsContainer.DeleteElement(pElement);
                        var gpEle = pElement as IGroupElement;
                        if (gpEle != null)
                        {
                            for (int i = 0; i < gpEle.ElementCount; i++)
                            {
                                IElement ee = gpEle.get_Element(i);
                                pGraphicsContainer.DeleteElement(ee);
                            }
                        }
                    }
                    Marshal.ReleaseComObject(pEnumElemen);
                }
                catch
                {
                }
            }
            currentTool.OnKeyDown(keyCode, shift);
            act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

        }
        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            currentTool.OnMouseMove(Button, Shift, X, Y);
           
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            currentTool.OnMouseUp(Button, Shift, X, Y);

            
        }
        IPoint center = new PointClass();
        private void ConnectLineProperty()
        {
          
            for (int i = 0; i < groupEle.ElementCount; i++)
            {
                IElement ee = groupEle.get_Element(i);
                switch ((ee as IElementProperties).Name)
                {
                    case "锚点":
                        anchorEle = (ee as IClone).Clone() as IElement;
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
                        lineSep = ee as ILineElement;
                        geoLineSep = (ee.Geometry as IClone).Clone() as IGeometry;
                        break;
                    case "锚线":
                        line1 = (ee as IClone).Clone() as ILineElement;
                        break;
                    case "连接线":
                        line2 = (ee as IClone).Clone() as ILineElement;
                        break;
                    case "内容框":
                        polygon = (ee as IClone).Clone() as IPolygonElement;
                        break;
                    case "文本":
                        txtElement = (ee as IClone).Clone() as ITextElement;
                        break;
                    case "下标文本":
                        txtElementDown = (ee as IClone).Clone() as ITextElement;
                        break;
                    default:
                        break;
                    
                }
            }
            IElementProperties eleProperty = groupEle as IElementProperties;
            string name = eleProperty.Name;
            string type = eleProperty.Type;
            if (type != LabelType.ConnectLine.ToString())
                return;


            LabelJson json = LabelClass.GetLabelInfo(name);
            FrmLabelPropertySet frm = new FrmLabelPropertySet(json, groupEle);
            if (DialogResult.OK != frm.ShowDialog())
            {

                #region
                IGraphicsContainer pGraphicsContainer = gc as IGraphicsContainer;
                IGraphicsContainerSelect pGraphicsContainerSelect = gc as IGraphicsContainerSelect;
                //遍历Element
                IEnumElement pEnumElemen = pGraphicsContainerSelect.SelectedElements;
                pEnumElemen.Reset();

                IElement pElement;
                while ((pElement = pEnumElemen.Next()) != null)
                {
                
                    var gpEle = pElement as IGroupElement;
                    if (gpEle != null)
                    {
                        gpEle.ClearElements();
                    }
                    
                }
                Marshal.ReleaseComObject(pEnumElemen);
                gc.DeleteElement(groupEle as IElement);
                groupEle = new GroupElementClass();
                (groupEle as IElementProperties).Name = name;
                (groupEle as IElementProperties).Type = type;
                gc.AddElement(line1 as IElement, 0);
                gc.AddElement(line2 as IElement, 0);
                gc.AddElement(anchorEle as IElement, 0);
                gc.AddElement(polygon as IElement, 0);
                gc.AddElement(txtElement as IElement, 0);
                if(lineSep!=null)
                    gc.AddElement(lineSep as IElement, 0);
                gc.MoveElementToGroup(line1 as IElement, groupEle);
                gc.MoveElementToGroup(line2 as IElement, groupEle);
                gc.MoveElementToGroup(anchorEle as IElement, groupEle);
                gc.MoveElementToGroup(polygon as IElement, groupEle);
                gc.MoveElementToGroup(txtElement as IElement, groupEle);
                //groupEle.AddElement(line1 as IElement);
                //groupEle.AddElement(line2 as IElement);
                //groupEle.AddElement(anchorEle);
                //groupEle.AddElement(polygon as IElement);
                //groupEle.AddElement(txtElement as IElement);
                if (lineSep != null)
                {
                    gc.MoveElementToGroup(lineSep as IElement, groupEle);
                   // groupEle.AddElement(lineSep as IElement);
                }
                gc.AddElement(groupEle as IElement,0);
                pGraphicsContainerSelect.SelectElement(groupEle as IElement);
                #endregion
            }
            else
            {
                LabelClass lbHelper = new LabelClass();
                lbHelper.UpdateLabelElement(frm.TxtContent, center);
            }
            act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, act.Extent);
        }

        private void PolyLineProperty()
        {

            for (int i = 0; i < groupEle.ElementCount; i++)
            {
                IElement ee = groupEle.get_Element(i);
                switch ((ee as IElementProperties).Name)
                {
                    case "锚点":
                        anchorEle = (ee as IClone).Clone() as IElement;
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
                        lineSep = ee as ILineElement;
                        geoLineSep = (ee.Geometry as IClone).Clone() as IGeometry;
                        break;
                    case "锚线":
                        line1 = (ee as IClone).Clone() as ILineElement;
                        break;
                    case "连接线":
                        line2 = (ee as IClone).Clone() as ILineElement;
                        break;
                    case "内容框":
                        polygon = (ee as IClone).Clone() as IPolygonElement;
                        break;
                    case "文本":
                        txtElement = (ee as IClone).Clone() as ITextElement;
                        break;
                    case "下标文本":
                        txtElementDown = (ee as IClone).Clone() as ITextElement;
                        break;
                    default:
                        break;

                }
            }
            IElementProperties eleProperty = groupEle as IElementProperties;
            string name = eleProperty.Name;
            string type = eleProperty.Type;
            if (type != LabelType.NormalLine.ToString())
                return;


            LabelJson json = LabelClass.GetLabelInfo(name);
            FrmPolyLinePropertySet frm = new FrmPolyLinePropertySet(json, groupEle);
            if (DialogResult.OK != frm.ShowDialog())
            {

                #region
                IGraphicsContainer pGraphicsContainer = gc as IGraphicsContainer;
                IGraphicsContainerSelect pGraphicsContainerSelect = gc as IGraphicsContainerSelect;
                //遍历Element
                IEnumElement pEnumElemen = pGraphicsContainerSelect.SelectedElements;
                pEnumElemen.Reset();

                IElement pElement;
                while ((pElement = pEnumElemen.Next()) != null)
                {
                  
                    var gpEle = pElement as IGroupElement;
                    if (gpEle != null)
                    {
                        gpEle.ClearElements();
                    }
                   // groupEle = gpEle;
                    gc.DeleteElement(gpEle as IElement);
                }
                Marshal.ReleaseComObject(pEnumElemen);
             
                groupEle = new GroupElementClass();
                (groupEle as IElementProperties).Name = name;
                (groupEle as IElementProperties).Type = type;
                gc.AddElement(line1 as IElement, 0);
                gc.AddElement(line2 as IElement, 0);
                gc.AddElement(anchorEle as IElement, 0); ;
                gc.AddElement(polygon as IElement, 0);
                gc.AddElement(txtElement as IElement, 0);
                if (lineSep != null)
                    gc.AddElement(lineSep as IElement, 0);
                if (txtElementDown != null)
                {
                    gc.AddElement(txtElementDown as IElement, 0);
                }
              
                gc.MoveElementToGroup(line1 as IElement, groupEle);
                gc.MoveElementToGroup(line2 as IElement, groupEle);
                gc.MoveElementToGroup(anchorEle as IElement, groupEle);
                gc.MoveElementToGroup(polygon as IElement, groupEle);
                gc.MoveElementToGroup(txtElement as IElement, groupEle);
                if (lineSep != null)
                {
                    gc.MoveElementToGroup(lineSep as IElement, groupEle);
                  
                }
                if (txtElementDown != null)
                {
                    gc.MoveElementToGroup(txtElementDown as IElement, groupEle);
                   
                }
                gc.AddElement(groupEle as IElement, 0);
                pGraphicsContainerSelect.SelectElement(groupEle as IElement);
                #endregion
            }
            else
            {
                LabelClass lbHelper = new LabelClass();
                lbHelper.UpdatePolylineElement(frm.TxtContent, frm.TxtValueDown, center);
            }
            act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, act.Extent);
        }
        private void AttributeProperty()
        {

            for (int i = 0; i < groupEle.ElementCount; i++)
            {
                IElement ee = groupEle.get_Element(i);
                switch ((ee as IElementProperties).Name)
                {
                    case "锚点":
                        anchorEle = (ee as IClone).Clone() as IElement;
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
                        lineSep = ee as ILineElement;
                        geoLineSep = (ee.Geometry as IClone).Clone() as IGeometry;
                        break;
                    case "锚线":
                        line1 = (ee as IClone).Clone() as ILineElement;
                        break;
                    case "连接线":
                        line2 = (ee as IClone).Clone() as ILineElement;
                        break;
                    case "内容框":
                        polygon = (ee as IClone).Clone() as IPolygonElement;
                        break;
                    case "文本":
                        txtElement = (ee as IClone).Clone() as ITextElement;
                        break;
                    case "下标文本":
                        txtElementDown = (ee as IClone).Clone() as ITextElement;
                        break;
                    default:
                        break;

                }
            }
            IElementProperties eleProperty = groupEle as IElementProperties;
            string name = eleProperty.Name;
            string type = eleProperty.Type;
           


            LabelJson json = LabelClass.GetLabelInfo(name);
            FrmAttriPropertySet frm = new FrmAttriPropertySet(json, groupEle);
            if (DialogResult.OK != frm.ShowDialog())
            {

                #region
                IGraphicsContainer pGraphicsContainer = gc as IGraphicsContainer;
                IGraphicsContainerSelect pGraphicsContainerSelect = gc as IGraphicsContainerSelect;
                //遍历Element
                IEnumElement pEnumElemen = pGraphicsContainerSelect.SelectedElements;
                pEnumElemen.Reset();

                IElement pElement;
                while ((pElement = pEnumElemen.Next()) != null)
                {

                    var gpEle = pElement as IGroupElement;
                    if (gpEle != null)
                    {
                        gpEle.ClearElements();
                    }
                   
                    gc.DeleteElement(gpEle as IElement);
                }
                Marshal.ReleaseComObject(pEnumElemen);

                groupEle = new GroupElementClass();
                (groupEle as IElementProperties).Name = name;
                (groupEle as IElementProperties).Type = type;
                gc.AddElement(line1 as IElement, 0);
                gc.AddElement(line2 as IElement, 0);
                gc.AddElement(anchorEle as IElement, 0); ;
                gc.AddElement(polygon as IElement, 0);
                gc.AddElement(txtElement as IElement, 0);
                if (lineSep != null)
                    gc.AddElement(lineSep as IElement, 0);
                if (txtElementDown != null)
                {
                    gc.AddElement(txtElementDown as IElement, 0);
                }

                gc.MoveElementToGroup(line1 as IElement, groupEle);
                gc.MoveElementToGroup(line2 as IElement, groupEle);
                gc.MoveElementToGroup(anchorEle as IElement, groupEle);
                gc.MoveElementToGroup(polygon as IElement, groupEle);
                gc.MoveElementToGroup(txtElement as IElement, groupEle);
                if (lineSep != null)
                {
                    gc.MoveElementToGroup(lineSep as IElement, groupEle);

                }
                if (txtElementDown != null)
                {
                    gc.MoveElementToGroup(txtElementDown as IElement, groupEle);

                }
                gc.AddElement(groupEle as IElement, 0);
                pGraphicsContainerSelect.SelectElement(groupEle as IElement);
                #endregion
            }
            else
            {
                LabelClass lbHelper = new LabelClass();
                lbHelper.UpdateAttributeElement(frm.TxtContent, frm.TxtValueDown, center);
            }
            act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, act.Extent);
        }
        public override void OnDblClick()
        {
            currentTool.OnDblClick();
         
         
            IEnumElement pEnumElement = gc.LocateElements(currentPoint, 1);
            if (pEnumElement == null)
                return;
            pEnumElement.Reset();
            IElement pElement = pEnumElement.Next();
            if (pElement == null)
            {
                return;
            }
            if ((pElement as IElementProperties).Type == LabelType.SymbolLine.ToString())
            {
                IElement orginEle = (pElement as IClone).Clone() as IElement;

                string elementParameter = (pElement as IElementProperties3).Name;
                ITextSymbol texSy = new TextSymbolClass();
                LabelJson json = LabelClass.GetLabelInfo(elementParameter);
                FrmSymLabelProperty frm = new FrmSymLabelProperty(json, pElement as IGroupElement, gc);
                frm.StartPosition = FormStartPosition.CenterScreen;
                if (DialogResult.OK != frm.ShowDialog())
                {
                    //返回
                    gc.DeleteElement(pElement);
                    gc.AddElement(orginEle, 0);
                    act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, act.Extent);
                }
            }
            else if ((pElement as IElementProperties).Type == LabelType.BallCallout.ToString())
            {
                IElement orginEle = (pElement as IClone).Clone() as IElement;

                string elementParameter = (pElement as IElementProperties3).Name;
                ITextSymbol texSy = new TextSymbolClass();
                FrmLableBall frm = new FrmLableBall(elementParameter, texSy, act, gc, pElement);
                frm.StartPosition = FormStartPosition.CenterScreen;
                if (DialogResult.OK != frm.ShowDialog())
                {
                    //返回
                    gc.DeleteElement(pElement);
                    gc.AddElement(orginEle, 0);
                    act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, act.Extent);
                }

            }
            else if ((pElement as IElementProperties).Type == LabelType.ConnectLine.ToString())
            {
                if (!(pElement is IGroupElement))
                {
                    pElement = null;
                    return;
                }
                groupEle = pElement as IGroupElement;
                ElementInit();
                gs.UnselectAllElements();
                gs.SelectElement(groupEle as IElement);
                act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, act.Extent);
                ConnectLineProperty();
            }
            else if ((pElement as IElementProperties).Type == LabelType.NormalLine.ToString())
            {
                if (!(pElement is IGroupElement))
                {
                    pElement = null;
                    return;
                }
                groupEle = pElement as IGroupElement;
                ElementInit();
                gs.UnselectAllElements();
                gs.SelectElement(groupEle as IElement);
                act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, act.Extent);
                PolyLineProperty();
            }
            else if ((pElement as IElementProperties).Type == LabelType.AttrLabel.ToString())
            {
                if (!(pElement is IGroupElement))
                {
                    pElement = null;
                    return;
                }
                groupEle = pElement as IGroupElement;
                ElementInit();
                gs.UnselectAllElements();
                gs.SelectElement(groupEle as IElement);
                act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, act.Extent);
                AttributeProperty();
            }
        }
        #endregion
    }
}
