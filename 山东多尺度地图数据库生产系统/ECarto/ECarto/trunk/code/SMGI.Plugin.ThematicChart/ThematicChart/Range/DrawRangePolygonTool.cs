using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using System.Collections;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using stdole;
using System.Collections.Generic;
using SMGI.Common;
using System.Linq;
namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    public class DrawRangePolygonTool : SMGITool
    {
        private IActiveView pAc;
   
        private double mapScale;
        INewLineFeedback lineFeedback;
        private ILayer pRepLayer = null;
        ISimpleLineSymbol lineSymbol;
        ICmykColor CmykColor=null;
        public DrawRangePolygonTool()
        {
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }
        private IPoint EndPoint = null;
        List<IPoint> linepoints = new List<IPoint>();
        private int m_Step = 0;
        public int Step
        {
            get { return m_Step; }
            set { m_Step = value; }
        }
        public INewLineFeedback LineFeedback
        {
            get { return lineFeedback; }
            set { lineFeedback = value; }
        }
        private bool m_IsUsed = false;
        public bool IsUsed
        {
            get
            {
                return m_IsUsed;
            }
            set
            {
                m_IsUsed = value;
            }
        }
        public bool disEnble = true;
        public override void OnClick()
        {
            FrmRangeLine frm= new FrmRangeLine();
            DialogResult dr= frm.ShowDialog();
            if (dr != DialogResult.OK)
                return;
            LineWidth = frm.LineWidth;
            LineLength = frm.MarkerSize;
            SymSizeInt = frm.MarkerSizeInt;
            CmykColor=frm.CMYKColors;
            disEnble = false;
            pAc = m_Application.ActiveView;
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LPOINT");
            })).ToArray();
            pRepLayer = lyrs.First();

            pAc = m_Application.ActiveView;
            mapScale = (m_Application.ActiveView as IMap).ReferenceScale;
            lineSymbol = new SimpleLineSymbolClass();
            IRgbColor color = new RgbColorClass();	 //red
            color.Red = 255;
            color.Green = 0;
            color.Blue = 0;
            lineSymbol.Color = color;
            lineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
            lineSymbol.Width = 1.5;
            (lineSymbol as ISymbol).ROP2 = esriRasterOpCode.esriROPNotXOrPen;//这个属性很重要
            lineFeedback = null;
            //用于解决在绘制feedback过程中进行地图平移出现线条混乱的问题
            m_Application.MapControl.OnAfterScreenDraw += new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);
           
        }
        private void MapControl_OnAfterScreenDraw(object sender, IMapControlEvents2_OnAfterScreenDrawEvent e)
        {
            if (lineFeedback != null)
            {
                lineFeedback.Refresh(m_Application.ActiveView.ScreenDisplay.hDC);
            }
        }

        public override void OnMouseDown(int Button, int shift, int X, int Y)
        {
            

            if (Button != 1 || disEnble)
            {
                this.Step = 0;
                this.IsUsed = false;
                this.LineFeedback = null;
                EndPoint = null;
                (pAc as IGraphicsContainer).DeleteAllElements();
                pAc.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, pAc.Extent);
                return;
            }
          
            if (pAc == null) return;
            IPoint tpPoint = null;
            if (Button == 1)    //MouseDown左键 开始画点/连续画点  
            {

                if (this.Step <= 0)
                {
                    linepoints.Clear();
                    //开始画点  
                    tpPoint = pAc.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                    this.LineFeedback = new NewLineFeedbackClass();
                    LineFeedback.Symbol = lineSymbol as ISymbol;
                    this.LineFeedback.Start(tpPoint);
                    this.Step += 1;
                    this.LineFeedback.Display = pAc.ScreenDisplay;
                    this.IsUsed = true;
                    linepoints.Add(tpPoint);
                }
                else
                {
                    //连续画点  
                    tpPoint = pAc.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

                    this.LineFeedback.Display = pAc.ScreenDisplay;
                    if (Step+1 == 3)
                    {
                        EndPoint = tpPoint;
                       // DrawEllipse();
                    }
                    if (this.Step <= 1)
                    {
                        this.LineFeedback.AddPoint(tpPoint);
                        linepoints.Add(tpPoint);
                        this.Step += 1;
                    }
                   
                }
            }  //--  

            if (Button == 4)
            {
                IPoint tpPoint1 = pAc.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                pAc.ScreenDisplay.PanStart(tpPoint1);
            }
           
           
        }
        public override void OnMouseMove(int Button, int shift, int X, int Y)
        {
            if (Button == 4)
            {
                IPoint tpPoint1 = pAc.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                pAc.ScreenDisplay.PanMoveTo(tpPoint1);
            }
            else
            {
                if (this.IsUsed == true)
                {
                    IPoint mappoint = pAc.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                    this.LineFeedback.MoveTo(mappoint);
                    if (Step == 2)
                    {
                        EndPoint = mappoint;
                        DrawPreEllipse();
                     
                    }
                    
                   
                }
            }
        }
        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            if (this.LineFeedback != null)
                this.LineFeedback.Display = pAc.ScreenDisplay;
            if (Button == 4)
            {
                IPoint tpPoint1 = pAc.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                pAc.ScreenDisplay.PanStop();
            }

            
        }
        public override void OnDblClick()
        {
            try
            {
                  
                if (this.IsUsed == false) return;
                if (this.LineFeedback == null) return;
                if (this.Step > 1)
                {
                    if (EndPoint == null)
                        return;
                    this.LineFeedback.AddPoint(EndPoint);
                    IPolyline tpPolyline = this.LineFeedback.Stop();
                    if (tpPolyline == null)
                    {
                        this.Step = 0;
                        this.IsUsed = false;
                        this.LineFeedback = null;
                        EndPoint = null;
                        return;
                    }
                    this.Step = 0;
                    this.IsUsed = false;
                    this.LineFeedback = null;
                  
                    IGeometry tpLine = tpPolyline;
                    double a,b;
                    using (WaitOperation wo = m_Application.SetBusy())
                    {
                        wo.SetText("正在生成范围面....");
                        DrawEllipseEdge();
                        //IPoint orgin = GetEllipseAB(out a, out b);

                       // DrawEllipseEdge(a, b, orgin);
                        EndPoint = null;
                    }
                  
                  
                     
                   
                }
            }
            catch (Exception ex)
            {
                EndPoint = null;
            }




        }
        public override bool Deactivate()
        {
            //卸掉该事件
            m_Application.MapControl.OnAfterScreenDraw -= new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);
            this.Step = 0;
            this.IsUsed = false;
            this.LineFeedback = null;
            EndPoint = null;
            return base.Deactivate();
        }
        private IPoint GetEllipseAB(out double a, out double b)
        {
            GraphicsHelper gh = new GraphicsHelper(pAc);
            IPolyline pline = gh.ContructPolyLine(linepoints[0], linepoints[1]);
            ILine plineAngle = new LineClass();

            plineAngle.PutCoords(pline.FromPoint, pline.ToPoint);
            double angle = plineAngle.Angle;

            IPolygon polygon = ContructPolygon(linepoints, EndPoint);

            double area = (polygon as IArea).Area;

            IPoint centerpoint = new PointClass();
            pline.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, centerpoint);
            a = pline.Length;
            a = a * 0.5;
            b = (2 * Math.Abs(area) / pline.Length);
            //将ab转为毫米
            a = a / pAc.FocusMap.ReferenceScale * 1000;
            b = b / pAc.FocusMap.ReferenceScale * 1000;
            return centerpoint;
        }

        private void DrawPreEllipse()
        {

            GraphicsHelper gh = new GraphicsHelper(pAc);

            IPolyline pline = gh.ContructPolyLine(linepoints[0], linepoints[1]);
            ILine plineAngle = new LineClass();

            plineAngle.PutCoords(pline.FromPoint, pline.ToPoint);
            double angle = plineAngle.Angle;

            IPolygon polygon = ContructPolygon(linepoints, EndPoint);

            double area = (polygon as IArea).Area;

            IPoint centerpoint = new PointClass();
            pline.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, centerpoint);
            double a = pline.Length;
            a = a * 0.5;
            double b = (2 * Math.Abs(area) / pline.Length);

            IEnvelope en = new EnvelopeClass();
            en.PutCoords(0, 0, a * 2, b * 2);
            en.CenterAt(centerpoint);

            IConstructEllipticArc constructEllipticArc = new EllipticArcClass();
            constructEllipticArc.ConstructEnvelope(en);

            ISegment pSegment = constructEllipticArc as ISegment;
            ISegmentCollection pSegmentCollection = new RingClass();
           
            pSegmentCollection.AddSegment(pSegment);
            IRing pRing = pSegmentCollection as IRing;
            pRing.Close();
            IGeometryCollection pGeometryColl = new PolygonClass();
            pGeometryColl.AddGeometry(pRing);




            ITransform2D ptrans = pGeometryColl as ITransform2D;
            ptrans.Rotate(centerpoint, angle);
            IGraphicsContainer pgc = pAc as IGraphicsContainer;
            pgc.DeleteAllElements();
            var ele = gh.DrawPolygon(pGeometryColl as IPolygon);
            (pAc as IGraphicsContainer).AddElement(ele, 0);
            pAc.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, this.pAc.Extent);
          

        }

        private void DrawEllipse()
        {

            GraphicsHelper gh = new GraphicsHelper(pAc);

            IPolyline pline = gh.ContructPolyLine(linepoints[0], linepoints[1]);
            ILine plineAngle = new LineClass();

            plineAngle.PutCoords(pline.FromPoint, pline.ToPoint);
            double angle = plineAngle.Angle;

            IPolygon polygon = ContructPolygon(linepoints, EndPoint);
         
            double area = (polygon as IArea).Area;

            IPoint centerpoint = new PointClass();
            pline.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, centerpoint);
            double a = pline.Length;
            a = a * 0.5;
            double b = (2 * Math.Abs(area) / pline.Length);
            IGeometryCollection pGeoco = new PolygonClass();
            IPointCollection pc = new RingClass();
            for (int i = 0; i <= 50; i++)
            {
                double x = -a + 2 * a * i / 50;
                double y = b * Math.Pow(1 - x * x / (a * a), 0.5);
                IPoint p = new PointClass() { X = x + centerpoint.X, Y = y + centerpoint.Y };
                pc.AddPoint(p);
            }
            for (int i = 0; i <= 50; i++)
            {
                double x = a - 2 * a * i / 50;
                double y = -b * Math.Pow(1 - x * x / (a * a), 0.5);
                IPoint p = new PointClass() { X = x + centerpoint.X, Y = y + centerpoint.Y };
                pc.AddPoint(p);
            }
            (pc as IRing).Close();
            (pc as IRing).Smooth(0);
            pGeoco.AddGeometry(pc as IGeometry);
            if (pGeoco.GeometryCount==0)
                return;
            (pGeoco as ITopologicalOperator).Simplify();
            ITransform2D ptrans = pGeoco as ITransform2D;
            ptrans.Rotate(centerpoint, angle);
            IGraphicsContainer pgc = pAc as IGraphicsContainer;
            pgc.DeleteAllElements();
            var ele= gh.DrawPolygon(pGeoco as IPolygon);
           // (pAc as IGraphicsContainer).AddElement(ele, 0);
            IEnvelope pEnvelop = (pGeoco as IPolygon).Envelope;
            pEnvelop.Expand(2, 2, true);
         
            pAc.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, pEnvelop);
            Marshal.ReleaseComObject(pGeoco);
            Marshal.ReleaseComObject(pc);
 
        }
        private IPolygon ContructPolygon(List<IPoint> pline, IPoint end)
        {
            IGeometryCollection gc = new PolygonClass();
            IPointCollection ring = new RingClass();

            foreach (var p in pline)
            {
                ring.AddPoint(p);
            }
            ring.AddPoint(end);
            (ring as IRing).Close();
            gc.AddGeometry(ring as IGeometry);
            (gc as ITopologicalOperator).Simplify();
            (gc as IGeometry).Project(pAc.FocusMap.SpatialReference);
            return gc as IPolygon;
        }
        List<IElement> eles = new List<IElement>();
        double LineWidth = 3;//线宽
        double LineLength = 3;
        double SymSizeInt = 1;
       
        //绘制椭圆环
        private IPolygon DrawEllipse(double a, double b)
        {


        
            IGeometryCollection pGeoco = new PolygonClass();

            double ds = LineWidth;

            for (int r = 0; r < 2; r++)
            {
                a = a - r * ds;
                b = b - r * ds;
                IPointCollection pc = new RingClass();
                for (int i = 0; i <= 50; i++)
                {
                    double x = -a + 2 * a * i / 50;
                    double y = b * Math.Pow(1 - x * x / (a * a), 0.5);
                    IPoint p = new PointClass() { X = x, Y = y };
                    pc.AddPoint(p);
                }
                for (int i = 0; i <= 50; i++)
                {
                    double x = a - 2 * a * i / 50;
                    double y = -b * Math.Pow(1 - x * x / (a * a), 0.5);
                    IPoint p = new PointClass() { X = x, Y = y };
                    pc.AddPoint(p);
                }
                (pc as IRing).Close();
                (pc as IRing).Smooth(0);
                pGeoco.AddGeometry(pc as IGeometry);
            }

            
            IGraphicsContainer pgc = pAc as IGraphicsContainer;
            IElement pele = null;
            IFillShapeElement fillEle = new PolygonElementClass();
           
            ISimpleFillSymbol pfl = new SimpleFillSymbolClass();
            pfl.Style = esriSimpleFillStyle.esriSFSNull;
            ISimpleLineSymbol psl = new SimpleLineSymbolClass();
            psl.Width = 0.1;
            pfl.Outline = psl;
            fillEle.Symbol = pfl;

            pele = fillEle as IElement;
            pele.Geometry = pGeoco as IPolygon;
          
            pAc.Refresh();
            (pGeoco as ITopologicalOperator).Simplify();
          
            return pGeoco as IPolygon;
        }
        private void DrawEllipseEdge()
        {
            eles.Clear();
            GraphicsHelper gh = new GraphicsHelper(pAc);

            IPolyline pline = gh.ContructPolyLine(linepoints[0], linepoints[1]);
            ILine plineAngle = new LineClass();

            plineAngle.PutCoords(pline.FromPoint, pline.ToPoint);
            double angle = plineAngle.Angle;

            IPolygon polygon = ContructPolygon(linepoints, EndPoint);

            double area = (polygon as IArea).Area;

            IPoint centerpoint = new PointClass();
            pline.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, centerpoint);
            double a = pline.Length;
            a = a * 0.5;
            double b = (2 * Math.Abs(area) / pline.Length);

            //将长短半轴转为毫米
            a = a / pAc.FocusMap.ReferenceScale * 1000;
            b = b / pAc.FocusMap.ReferenceScale * 1000;

            IEnvelope en = new EnvelopeClass();
            en.PutCoords(0, 0, a * 2, b * 2);
            en.CenterAt(new PointClass { X = 0, Y = 0 });

            IConstructEllipticArc constructEllipticArc = new EllipticArcClass();
            constructEllipticArc.ConstructEnvelope(en);
         

            double rectWidth =LineWidth;
            double rectLength = LineLength ;
            double rectStep = SymSizeInt;

            double lenght = (constructEllipticArc as IEllipticArc).Length;


            double ct = Math.Floor((lenght / (rectLength + rectStep)));
            rectLength = (lenght - ct * rectStep) / ct;
           
            en = new EnvelopeClass();
            en.PutCoords(0, 0, a * 2 - 2 * rectWidth, b * 2 - 2 * rectWidth);
            en.CenterAt(new PointClass { X = 0, Y = 0 });
            IConstructEllipticArc innerArc = new EllipticArcClass();
            innerArc.ConstructEnvelope(en);


            //(constructEllipticArc as ITransform2D).Rotate(new PointClass { X = 0, Y = 0 }, angle);
            //(innerArc as ITransform2D).Rotate(new PointClass { X = 0, Y = 0 }, angle);
            double lenght1 = (innerArc as IEllipticArc).Length;

            double scaleX = lenght1 / lenght;
            double len1 = rectLength * scaleX;
            double intv1 = rectStep * scaleX;
            len1 = (lenght1 - ct * intv1) / ct;

            for(int i=0;i<ct;i++)
            {    
                //每次提取0*(intv+len),0*(intv+len)+len 这个长度的弧段
                ICurve outCurve = new PolylineClass();
                ICurve outCurveInner = new PolylineClass();
                (constructEllipticArc as IEllipticArc).GetSubcurve(i * (rectStep + rectLength), i * (rectStep + rectLength) + rectLength, false, out outCurve);
                (innerArc as IEllipticArc).GetSubcurve(i*(intv1+len1),i*(intv1+len1)+len1,false,out outCurveInner);
                ISegmentCollection pSegmentCollection = new RingClass();
                pSegmentCollection.AddSegment(outCurve as ISegment);
                outCurveInner.ReverseOrientation();
                ILine line1 = new LineClass { FromPoint = outCurve.ToPoint, ToPoint = outCurveInner.FromPoint };
                ILine line2 = new LineClass { FromPoint = outCurveInner.ToPoint, ToPoint = outCurve.FromPoint };
                pSegmentCollection.AddSegment(line1 as ISegment);
                pSegmentCollection.AddSegment(outCurveInner as ISegment);
                pSegmentCollection.AddSegment(line2 as ISegment);
                IRing pRing = pSegmentCollection as IRing;
                pRing.Close();
                IGeometryCollection pGeometryColl = new PolygonClass();
                pGeometryColl.AddGeometry(pRing);
                (pGeometryColl as ITopologicalOperator).Simplify();

                IFillShapeElement polygonElement = new PolygonElementClass();
                ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
                smsymbol.Color = CmykColor;
                ISimpleLineSymbol linesym = new SimpleLineSymbolClass();
                linesym.Style = esriSimpleLineStyle.esriSLSNull;

                smsymbol.Outline = linesym;
                polygonElement.Symbol = smsymbol;
                IElement pE = polygonElement as IElement;
                pE.Geometry = pGeometryColl as IGeometry;
                //(pAc as IGraphicsContainer).AddElement(pE, 0);
                pAc.Refresh();
                eles.Add(pE);

            }
           
            angle = (180 * plineAngle.Angle) / Math.PI;
            double size = 2 * a;
            string jsonText = "";
            int obj = 0;
            m_Application.EngineEditor.StartOperation();
            try
            {
                if(eles.Count>0)
                {
                   var remarker = ChartsToRepHelper.CreateFeature(angle, pAc, eles, pRepLayer, centerpoint, jsonText, out obj, size);
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                (pAc as IGraphicsContainer).DeleteAllElements();
            }
            m_Application.EngineEditor.StopOperation("范围面");
        }

        private void DrawEllipseEdge(double a, double b, IPoint ctpoint)
        {
            eles.Clear();
            
            IPolygon pEllipse = DrawEllipse(a, b);

            ITopologicalOperator pTo = (pEllipse as IClone).Clone() as ITopologicalOperator;

           
            IGeometryCollection pGeoco = new PolygonClass();




            IPointCollection pc = new PolylineClass();
            for (int i = 0; i <= 50; i++)
            {
                double x = -a + 2 * a * i / 50;
                double y = b * Math.Pow(1 - x * x / (a * a), 0.5);
                IPoint p = new PointClass() { X = x, Y = y };
                pc.AddPoint(p);
            }
            for (int i = 0; i <= 50; i++)
            {
                double x = a - 2 * a * i / 50;
                double y = -b * Math.Pow(1 - x * x / (a * a), 0.5);
                IPoint p = new PointClass() { X = x, Y = y };
                pc.AddPoint(p);
            }

            (pc as IPolyline).Smooth(0);
            double ct = 50.0;
            IPolyline ppolyline = (pc as IPolyline);
            double lenght = ppolyline.Length;
            double ds = LineWidth;
            double len = LineLength;
            double intv = SymSizeInt;
            ct = Math.Floor((lenght / (len + intv)));
            len = (lenght - ct * intv) / ct;
            IGeometryCollection pPolyline;
            IPointCollection pCl;
            ILine normal;
            ILine normal2;
            GraphicsHelper gh = new GraphicsHelper(pAc);
         
            for (int i = 0; i < ct; i++)
            {
                #region

                double dis1 = (len + intv) * i;
                double dis2 = (len + intv) * i + len;

                double x, y = 0;

                pPolyline = new PolylineClass();

                pCl = new PathClass();
                normal = new LineClass();
                (pc as IPolyline).QueryNormal(esriSegmentExtension.esriNoExtension, dis1, false, 1.2 * ds, normal);
                x = 2 * normal.FromPoint.X - normal.ToPoint.X;
                y = 2 * normal.FromPoint.Y - normal.ToPoint.Y;
                pCl.AddPoint(new PointClass() { X = x, Y = y });
                pCl.AddPoint(normal.ToPoint);
             
                normal2 = new LineClass();
                (pc as IPolyline).QueryNormal(esriSegmentExtension.esriNoExtension, dis2, false, 1.2 * ds, normal2);
                x = 2 * normal2.FromPoint.X - normal2.ToPoint.X;
                y = 2 * normal2.FromPoint.Y - normal2.ToPoint.Y;
                pCl.AddPoint(normal2.ToPoint);
                pCl.AddPoint(new PointClass() { X = x, Y = y });


                IGeometry left, right;
                pPolyline.AddGeometry(pCl as IGeometry);
                (pPolyline as ITopologicalOperator).Simplify();
                if (pPolyline.GeometryCount > 1)
                {
                    MessageBox.Show("轮廓尺寸太小，请重新设置");
                    eles.Clear();
                    return;
                }
             
              

                pTo.Cut(pPolyline as IPolyline, out left, out right);


              
                IFillShapeElement polygonElement = new PolygonElementClass();
                ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
                smsymbol.Color=CmykColor;
                ISimpleLineSymbol linesym = new SimpleLineSymbolClass();
                linesym.Style = esriSimpleLineStyle.esriSLSNull;

                smsymbol.Outline = linesym;
                polygonElement.Symbol = smsymbol;
                IElement pEl = polygonElement as IElement;
                pEl.Geometry = left as IPolygon as IGeometry;
                eles.Add(pEl);
                #endregion
            }
            IPolyline pline = gh.ContructPolyLine(linepoints[0], linepoints[1]);
            ILine plineAngle = new LineClass();
            plineAngle.PutCoords(pline.FromPoint, pline.ToPoint);
            double angle = plineAngle.Angle;
            angle = (180 * plineAngle.Angle) / Math.PI;
            double size = 2 * a;
            string jsonText = "";
            int obj = 0;
            m_Application.EngineEditor.StartOperation();
            try
            {
                var remarker = ChartsToRepHelper.CreateFeature(angle, pAc, eles, pRepLayer, ctpoint, jsonText, out obj, size);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                (pAc as IGraphicsContainer).DeleteAllElements();
            }
            m_Application.EngineEditor.StopOperation("范围面"); 
        }
    }
}
