using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
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
using ESRI.ArcGIS.Display;
using SMGI.Common;
namespace SMGI.Plugin.EmergencyMap
{

    public sealed class LabelPropertyTool : SMGITool
    {



        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null;
            }
        }  

        public LabelPropertyTool()
        {
            
        }


        public override bool Deactivate()
        {
            LabelClass.Instance.ActiveDefaultLayer();
            return base.Deactivate();
        }

        #region Overridden Class Methods

         
        IGraphicsContainerSelect gs = null;
        IActiveView act = null;
        public override void OnClick()
        {
            act = m_Application.ActiveView;
            gc = act as IGraphicsContainer;
            gs = act as IGraphicsContainerSelect;
          
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

        IGroupElement groupEle = null;
        IGraphicsContainer gc = null;
        bool selected = false;
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            IGraphicsContainer pGraphicsContainer = gc as IGraphicsContainer;
            IGraphicsContainerSelect pGraphicsContainerSelect = gc as IGraphicsContainerSelect;
            //±È¿˙Element
            IEnumElement pEnumElemen = pGraphicsContainerSelect.SelectedElements;
            pEnumElemen.Reset();

            IElement pElement;
            while ((pElement = pEnumElemen.Next()) != null)
            {
                pGraphicsContainer.DeleteElement(pElement);
            }
            // TODO:  Add EleRotateTool.OnMouseDown implementation
            //IPoint pp = act.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            // selected = false;
            //IEnvelope en = new EnvelopeClass();
            //en.PutCoords(pp.X - 0.5, pp.Y - 0.5, pp.X + 0.5, pp.Y + 0.5);

            //IEnumElement pEnumElement = (act as IGraphicsContainer).LocateElements(pp, 1);
            //if (pEnumElement == null)
            //    return;
            //pEnumElement.Reset();
            //IElement pElement = pEnumElement.Next();
            //if (pElement == null)
            //{
            //    return;
            //}
            //groupEle = pElement as IGroupElement;
            
            //pElement = groupEle.get_Element(0);
            //IElement pElement1 = groupEle.get_Element(1);
            //IElement pElement2 = groupEle.get_Element(2);
            //IElement pElement3 = groupEle.get_Element(3);
           
            //groupEle.ClearElements();
            //gc.DeleteElement(pElement);
            //gc.DeleteElement(pElement1);
            //gc.DeleteElement(pElement2);
            //gc.DeleteElement(pElement3);
            //ITransform2D t2d = pElement.Geometry as ITransform2D;
            //t2d.Move(10, 0);
            //pElement.Geometry = t2d as IGeometry;
          //  gc.UpdateElement(pElement);

            
            //groupEle = pElement as IGroupElement;
            //IElement[] es = new IElement[groupEle.ElementCount];

            //for (int i = 0; i < groupEle.ElementCount; i++)
            //{
            //    IElement ee = groupEle.get_Element(i);
            //    es[i] = ee;
            //}

           // groupEle.ClearElements();

            try
            {
                //{
                //    ITransform2D t2d = es[0].Geometry as ITransform2D;
                //    t2d.Move(10, 0);
                //    es[0].Geometry = t2d as IGeometry;
                //    gc.UpdateElement(es[0]);
                //}
                //for (int i = 0; i < es.Length; i++)
                //{
                //    groupEle.AddElement(es[i]);
                //}
                //gc.UpdateElement(groupEle as IElement);
                act.Refresh();
            }
            catch
            {
            }
            //IElementProperties eleProperty = groupEle as IElementProperties;
            //string name = eleProperty.Name;
            //string type = eleProperty.Type;
            //if (type != LabelType.ConnectLine.ToString())
            //    return;
           
            //gs.SelectElement(groupEle as IElement);
            //LabelJson json = LabelClass.GetLabelInfo(name);
            //FrmLabelProperty frm = new FrmLabelProperty(json, groupEle, act);
            //frm.ShowDialog();

        }
        IPoint center = new PointClass();
        double labelLineLens = 10;
        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {

            
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            
        }
        #endregion
    }
}
