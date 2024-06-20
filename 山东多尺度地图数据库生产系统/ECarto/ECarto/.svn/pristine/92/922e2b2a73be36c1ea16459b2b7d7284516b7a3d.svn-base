using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using System.IO;
using stdole;
using System.Windows.Forms;
using SMGI.Common;
namespace SMGI.Plugin.ThematicChart.ThematicChart
{

    /// <summary>
    /// 将Excel表格导入自由制图表达
    /// </summary>
    public  class ExcelToRep
    {

        private  IActiveView pAc=null;
        private ESRI.ArcGIS.Geometry.IPoint pBasePoint = new ESRI.ArcGIS.Geometry.PointClass() { X = 0, Y = 0 };
        private ESRI.ArcGIS.Geometry.IPoint pBasePoint2 =null;//表格锚点
        private double MapScale = 10000;
        private double markerSize = 200;
        public double outBorderSpacing;//外边框间距
        public string outBorderStyle;//外边框样式
        public IColor outBorderColor;//外边框颜色
        public IColor innerLineColor;//内部分割线颜色
        public double innerLineWidth;//内部分割线宽度
        public bool isCorner;
        public string tableLocation;//表格位置，默认锚点
        IEnvelope clipGeoIn = null;//内图廓矩形
        private double totalWidth = 0;
        private double totalHeight = 0;
        int horizontalCount = 2;//水平间隔数量
        int verticalCount = 2;//垂直间隔数量

        public ILayer pLayer = null;
        private IFeatureClass annoFcl = null;
        public Dictionary<int, IRgbColor> FontColorDic = new Dictionary<int, IRgbColor>();
        public  Dictionary<int, IRgbColor> LoadExcelFontColor()
        {
     
            FontColorDic.Clear();
            string rulegdb = GApplication.Application.Template.Content.Element("ThematicRule").Value;
            string template = GApplication.Application.Template.Root + "\\" + rulegdb;
            Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance
                (factoryType);
           IWorkspace ws  =workspaceFactory.OpenFromFile(template, 0);

         
            ITable pTable = (ws as IFeatureWorkspace).OpenTable("ExcelFontColor");
            ICursor cursor = pTable.Search(null, false);
            IRow prow = null;
            int id=pTable.FindField("索引号");
            int idr=pTable.FindField("R");
            int idg=pTable.FindField("G");
            int idb=pTable.FindField("B");
            while ((prow = cursor.NextRow()) != null)
            {
                int index = Convert.ToInt32(prow.get_Value(id).ToString());
                int r = Convert.ToInt32(prow.get_Value(idr).ToString());
                int g = Convert.ToInt32(prow.get_Value(idg).ToString());
                int b = Convert.ToInt32(prow.get_Value(idb).ToString());
                IRgbColor rgb = new RgbColorClass() { Red = r, Green = g, Blue = b };
                FontColorDic[index] = rgb;
            }
            FontColorDic[-4105] = new RgbColorClass() { Red = 0, Green = 0, Blue = 0 };
            FontColorDic[-4142] = new RgbColorClass() { Red = 250, Green = 250, Blue = 250 };
         
            Marshal.ReleaseComObject(cursor);
            return FontColorDic;
        }
        List<IElement> elements = new List<IElement>();
        public ExcelToRep(IActiveView pAc_, ESRI.ArcGIS.Geometry.IPoint pBasePoint_,double ms=10000)
        {
           
            pAc = pAc_;
            pBasePoint2 = pBasePoint_;
            //MapScale = ms;
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LPOINT");
            })).ToArray();
            pLayer = lyrs.First();
            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFDOGraphicsLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LANNO");
            })).ToArray();
            IFeatureLayer annoly = lyr.First() as IFeatureLayer;
            annoFcl = annoly.FeatureClass;
            #region 获取内图廓矩形
            var lyrLLINE = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LLINE");
            })).ToArray();
            IFeatureLayer llineLayer = lyrLLINE.First() as IFeatureLayer;
            IFeatureClass llineFCL = llineLayer.FeatureClass;
            (llineFCL as IFeatureClassLoad).LoadOnlyMode = false;
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "TYPE like '%内图廓%'";
            if (llineFCL.FeatureCount(qf) > 0)
            {
                IFeature fe = null;
                IFeatureCursor cursor = llineFCL.Search(qf, false);
                while ((fe = cursor.NextFeature()) != null)
                {
                    string type = fe.get_Value(llineFCL.FindField("TYPE")).ToString();
                    if (type == "内图廓")
                    {
                        clipGeoIn = fe.ShapeCopy.Envelope;
                    }
                }
                Marshal.ReleaseComObject(cursor);
            }
            #endregion
            // LoadExcelFontColor();
        }
       
        List<cellInfos> CellInofs = new List<cellInfos>();//记录每个单元格高度，宽度信息
        List<cellInfos> MergeCellInofs = new List<cellInfos>();//记录列被合并每个单元格高度，宽度信息
        //外部调用方法
        public void ExcelToElement()
        {
            Table.FrmThematicTableSet frm = new Table.FrmThematicTableSet();
            DialogResult dr = frm.ShowDialog();
            if (dr != DialogResult.OK)
                return;
            markerSize = frm.markerSize;
            outBorderSpacing = frm.outBorderSpacing * 1.0e-3 * MapScale;
            outBorderStyle = frm.outBorderStyle;
            outBorderColor = frm.outBorderColor;
            innerLineColor = frm.innerLineColor;
            innerLineWidth = frm.innerLineWidth;
            isCorner = frm.isCorner;
            tableLocation = frm.tableLocation;
            bool cusBorder = frm.cusBorder;
            Table.FrmThematicTableSet.TableBorder tableBorder = frm.tableBorder;
            WaitOperation wo = GApplication.Application.SetBusy();
            try
            {
                wo.SetText("正在生成专题表格");
                string excelpath = frm.dataSource;
                
                elements.Clear();
                ExcelGenerator eg = new ExcelGenerator();
                eg.CreateNewDocumentByTemp(excelpath,frm.sheetName);


               
                Range cell0 = (eg.worksheet.Cells[1, 1]);
                cellInfos cellinfo = new cellInfos() { row = 1, column = 1, width = 0, height = 0 };
                CellInofs.Add(cellinfo);
             
                DrawCellTitleBgEle(cell0, pBasePoint); 
                ExcelCellExpand(cell0, eg);
                DrawSplitLine(eg);
                //绘制整个表格的花边样式，默认单实线
                if (isCorner)//边角边框样式
                {
                    #region 内边线
                    IGeometry inLineGeo = CreateRectangle(pBasePoint, totalWidth, totalHeight);
                    //内边线样式
                    IFillShapeElement polygonElementIn = new PolygonElementClass();
                    ISimpleFillSymbol smsymbol2 = new SimpleFillSymbolClass();
                    smsymbol2.Style = esriSimpleFillStyle.esriSFSNull;
                    ISimpleLineSymbol linesymIn = new SimpleLineSymbolClass();
                    linesymIn.Style = esriSimpleLineStyle.esriSLSSolid;
                    linesymIn.Color = outBorderColor;
                    linesymIn.Width = 0.3;
                    smsymbol2.Outline = linesymIn;
                    polygonElementIn.Symbol = smsymbol2;
                    IElement pElIn = polygonElementIn as IElement;
                    pElIn.Geometry = inLineGeo as IGeometry;
                    elements.Add(pElIn);
                    #endregion                    
                    if (outBorderStyle == "双实线")
                    {
                        #region 外边线
                        IGeometry outLineGeo = CreateRectangle(pBasePoint, totalWidth + 2 * outBorderSpacing, totalHeight + 2 * outBorderSpacing);
                        ITopologicalOperator topologicalOperator = outLineGeo as ITopologicalOperator;
                        topologicalOperator.Simplify();
                        ITransform2D pTrans = outLineGeo as ITransform2D;
                        switch (tableLocation)
                        {
                            case "左下":
                                pTrans.Move(0, 2*outBorderSpacing);
                                break;
                            case "右上":
                                pTrans.Move(-2*outBorderSpacing, 0);
                                break;
                            case "右下":
                                pTrans.Move(-2*outBorderSpacing, 2*outBorderSpacing);
                                break;
                            default://"左上"
                                break;
                        }
                        //外边线样式
                        IFillShapeElement polygonElementOut = new PolygonElementClass();
                        ISimpleFillSymbol smsymbol1 = new SimpleFillSymbolClass();
                        smsymbol1.Style = esriSimpleFillStyle.esriSFSNull;
                        ISimpleLineSymbol linesymOut = new SimpleLineSymbolClass();
                        linesymOut.Style = esriSimpleLineStyle.esriSLSSolid;
                        linesymOut.Color = outBorderColor;
                        linesymOut.Width = 0.3;
                        smsymbol1.Outline = linesymOut;
                        polygonElementOut.Symbol = smsymbol1;
                        IElement pElOut = polygonElementOut as IElement;
                        pElOut.Geometry = outLineGeo as IGeometry;
                        elements.Add(pElOut);
                        #endregion
                    }            
                }
                else       //自定义边框样式
                {
                    #region 内边框
                    IFillShapeElement polygonElementIn = new PolygonElementClass();
                    ISimpleFillSymbol smsymbolIn = new SimpleFillSymbolClass();
                    smsymbolIn.Style = esriSimpleFillStyle.esriSFSNull;
                    ISimpleLineSymbol linesymIn = new SimpleLineSymbolClass();
                    linesymIn.Style = esriSimpleLineStyle.esriSLSSolid;
                    linesymIn.Color = outBorderColor;
                    linesymIn.Width = 0.3;
                    smsymbolIn.Outline = linesymIn;
                    polygonElementIn.Symbol = smsymbolIn;
                    IElement pElIn = polygonElementIn as IElement;
                    IGeometry polygonIn = CreateRectangle(pBasePoint, totalWidth, totalHeight);
                    pElIn.Geometry = polygonIn as IGeometry;
                    elements.Add(pElIn);
                    horizontalCount = 0;
                    verticalCount = 0;
                    #endregion
                    if (outBorderStyle == "双实线")
                    {
                        #region 外边框
                        IFillShapeElement polygonElementOut = new RectangleElementClass();
                        ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
                        smsymbol.Style = esriSimpleFillStyle.esriSFSNull;
                        ISimpleLineSymbol linesymOut = new SimpleLineSymbolClass();
                        linesymOut.Style = esriSimpleLineStyle.esriSLSSolid;
                        linesymOut.Color = outBorderColor;
                        linesymOut.Width = 0.3;
                        smsymbol.Outline = linesymOut;
                        polygonElementOut.Symbol = smsymbol;
                        IElement pElOut = polygonElementOut as IElement;
                        #region 上右下左四个边框
                        ESRI.ArcGIS.Geometry.IPoint topBasePoint = new ESRI.ArcGIS.Geometry.PointClass() { X = pBasePoint.X, Y = pBasePoint.Y + outBorderSpacing };
                        ESRI.ArcGIS.Geometry.IPoint rightBasePoint = new ESRI.ArcGIS.Geometry.PointClass() { X = pBasePoint.X + totalWidth, Y = pBasePoint.Y};
                        ESRI.ArcGIS.Geometry.IPoint bottomBasePoint = new ESRI.ArcGIS.Geometry.PointClass() { X = pBasePoint.X, Y = pBasePoint.Y - totalHeight };
                        ESRI.ArcGIS.Geometry.IPoint leftBasePoint = new ESRI.ArcGIS.Geometry.PointClass() { X = pBasePoint.X - outBorderSpacing, Y = pBasePoint.Y  };
                        IGeometry outplyTop = null;
                        IGeometry outplyRight = null;
                        IGeometry outplyBottom = null;
                        IGeometry outplyLeft = null;
                        IGeometryCollection borderGeoBag = new GeometryBagClass();
                        borderGeoBag.AddGeometry(polygonIn);
                        if (tableBorder.topBorder)
                        {
                            outplyTop = CreateRectangle(topBasePoint,totalWidth,outBorderSpacing);
                            borderGeoBag.AddGeometry(outplyTop);
                            verticalCount +=1;
                        }
                        if (tableBorder.rightBorder)
                        {
                            outplyRight = CreateRectangle(rightBasePoint, outBorderSpacing, totalHeight);
                            borderGeoBag.AddGeometry(outplyRight);
                            horizontalCount += 1;
                        }
                        if (tableBorder.bottomBorder)
                        {
                            outplyBottom = CreateRectangle(bottomBasePoint,totalWidth,outBorderSpacing);
                            borderGeoBag.AddGeometry(outplyBottom);
                            verticalCount += 1;
                        }
                        if (tableBorder.leftBorder)
                        {
                            outplyLeft = CreateRectangle(leftBasePoint, outBorderSpacing, totalHeight);
                            borderGeoBag.AddGeometry(outplyLeft);
                            horizontalCount += 1;
                        }
                        ITopologicalOperator unionPolygon = new PolygonClass();
                        unionPolygon.ConstructUnion(borderGeoBag as IEnumGeometry);
                        unionPolygon.Simplify();
                        IGeometry polygonOut = (unionPolygon as IPolygon).Envelope as IGeometry;
                        #endregion
                        pElOut.Geometry = polygonOut as IGeometry;
                        elements.Add(pElOut);
                        #endregion
                    }  
                }
                
                eg.CloseExcel();
                int obj = 0;
               
                var remarker=CreateFeatures(elements, pLayer,out obj);
                CreateAnnotion(remarker, markerSize, obj);
                pAc.Refresh();
                wo.Dispose();
                MessageBox.Show("专题图表生成完成");
            }
            catch( Exception ex)
            {
                wo.Dispose();
            }
          
          
           
        }
        Dictionary<string, bool> dic = new Dictionary<string, bool>();
        private void DrawCellLine(Range cell0)
        {
            double unitx = 1 * 1e-3 * MapScale;
            double unity = 1 * 1e-3 * MapScale;
            Border border1 = cell0.Borders.get_Item(XlBordersIndex.xlDiagonalDown);
            Border border2 = cell0.Borders.get_Item(XlBordersIndex.xlDiagonalUp);
            if ((XlLineStyle)(border1.LineStyle) != XlLineStyle.xlLineStyleNone)
            {
                try
                {
                    IColor bgcolor1 = new RgbColorClass { RGB = int.Parse(border1.Color.ToString()) };
                    Console.WriteLine("左上");
                    ESRI.ArcGIS.Geometry.IPoint to = new ESRI.ArcGIS.Geometry.PointClass();
                    double x = double.Parse(cell0.Left.ToString()) * unitx;
                    double y = -double.Parse(cell0.Top.ToString()) * unitx;
                    to.PutCoords(x, y);
                    ESRI.ArcGIS.Geometry.IPoint from = new ESRI.ArcGIS.Geometry.PointClass();
                    x = double.Parse(cell0.Left.ToString()) * unitx + double.Parse(cell0.MergeArea.Width.ToString()) * unitx;
                    y = -double.Parse(cell0.Top.ToString()) * unitx - double.Parse(cell0.MergeArea.Height.ToString()) * unitx;
                    from.PutCoords(x, y);
                    PolylineClass line = new PolylineClass();
                    line.AddPoint(to);
                    line.AddPoint(from); line.Simplify();
                    ISimpleLineSymbol sls = new SimpleLineSymbolClass { Color = bgcolor1, Width = double.Parse(border1.Weight.ToString()) };
                    IGraphicsContainer pContainer = pAc as IGraphicsContainer;
                    double boderwidth = 0.1;
                    switch ((XlBorderWeight)border1.Weight)
                    {
                        case XlBorderWeight.xlHairline:
                            boderwidth = 0.1;
                            break;
                        case XlBorderWeight.xlThin:
                            boderwidth = 0.2;
                            break;
                        case XlBorderWeight.xlMedium:
                            boderwidth = 0.3;
                            break;
                        case XlBorderWeight.xlThick:
                            boderwidth = 0.4;
                            break;
                    }
                    sls.Width = boderwidth;
                    LineElementClass ele = new LineElementClass();
                    switch ((int)(border1.LineStyle))
                    {
                        case (int)XlLineStyle.xlContinuous:
                            sls.Style = esriSimpleLineStyle.esriSLSSolid;
                            break;
                        case (int)XlLineStyle.xlDash:
                            sls.Style = esriSimpleLineStyle.esriSLSDash;
                            break;
                        case (int)XlLineStyle.xlDashDot:
                        case (int)XlLineStyle.xlSlantDashDot:
                            sls.Style = esriSimpleLineStyle.esriSLSDashDot;
                            break;
                        case (int)XlLineStyle.xlDashDotDot:
                            sls.Style = esriSimpleLineStyle.esriSLSDashDotDot;
                            break;
                        case (int)XlLineStyle.xlDot:

                            sls.Style = esriSimpleLineStyle.esriSLSDot;
                            break;
                        default:
                            sls.Style = esriSimpleLineStyle.esriSLSSolid;
                            break;
                    }
                    ele.Symbol = sls;
                    ele.Geometry = line;
                    elements.Add(ele);
                }
                catch
                {
                }
            }

            if ((XlLineStyle)(border2.LineStyle) != XlLineStyle.xlLineStyleNone)
            {
                IColor bgcolor1 = new RgbColorClass { RGB = int.Parse(border2.Color.ToString()) };
                Console.WriteLine("左下");
                ESRI.ArcGIS.Geometry.IPoint to = new ESRI.ArcGIS.Geometry.PointClass();
                double x =double.Parse(cell0.Left.ToString()) * unitx;
                double y = -double.Parse(cell0.Top.ToString()) * unitx - double.Parse(cell0.MergeArea.Height.ToString()) * unitx;
                to.PutCoords(x, y);
                ESRI.ArcGIS.Geometry.IPoint from = new ESRI.ArcGIS.Geometry.PointClass();
                x = double.Parse(cell0.Left.ToString()) * unitx + double.Parse(cell0.MergeArea.Width.ToString()) * unitx;
                y = -double.Parse(cell0.Top.ToString()) * unitx;
                from.PutCoords(x,y);
                PolylineClass line = new PolylineClass();
                line.AddPoint(to);
                line.AddPoint(from); line.Simplify();
                IGraphicsContainer pContainer = pAc as IGraphicsContainer;
                double boderwidth = 0.1;
                LineElementClass ele = new LineElementClass();
                ISimpleLineSymbol sls = new SimpleLineSymbolClass { Color = bgcolor1, Width = double.Parse(border2.Weight.ToString()) };
                switch ((XlBorderWeight)border1.Weight)
                {
                    case XlBorderWeight.xlHairline:
                        boderwidth = 0.1;
                        break;
                    case XlBorderWeight.xlThin:
                        boderwidth = 0.2;
                        break;
                    case XlBorderWeight.xlMedium:
                        boderwidth = 0.3;
                        break;
                    case XlBorderWeight.xlThick:
                        boderwidth = 0.4;
                        break;
                }
                sls.Width = boderwidth;


                switch ((int)(border2.LineStyle))
                {
                    case (int)XlLineStyle.xlContinuous:
                        sls.Style = esriSimpleLineStyle.esriSLSSolid;
                        break;
                    case (int)XlLineStyle.xlDash:
                        sls.Style = esriSimpleLineStyle.esriSLSDash;
                        break;
                    case (int)XlLineStyle.xlDashDot:
                    case (int)XlLineStyle.xlSlantDashDot:
                        sls.Style = esriSimpleLineStyle.esriSLSDashDot;
                        break;
                    case (int)XlLineStyle.xlDashDotDot:
                        sls.Style = esriSimpleLineStyle.esriSLSDashDotDot;
                        break;
                    case (int)XlLineStyle.xlDot:
                        sls.Style = esriSimpleLineStyle.esriSLSDot;
                        break;
                    default:
                        sls.Style = esriSimpleLineStyle.esriSLSSolid;
                        break;
                }
                ele.Symbol = sls;
                ele.Geometry = line;
                elements.Add(ele);
               // pContainer.AddElement(ele, 0);
               
            }
        }
        private void DrawSplitLine(ExcelGenerator eg)
        {

            Shapes  excelShapes =eg.worksheet.Shapes;
            int ct = excelShapes.Count;
            double unitx =1 * 1e-3 * MapScale;
            double unity = 1 * 1e-3 * MapScale;
            #region 形状
            foreach (Microsoft.Office.Interop.Excel.Shape item in excelShapes)
            {
                string type = item.Type.ToString();
                string name = item.AlternativeText;
                int rgb1 = item.Line.ForeColor.RGB;

                IColor bgcolor = new RgbColorClass { RGB = rgb1 };
                Console.WriteLine("原始：" + name);
                if (item.Type == Microsoft.Office.Core.MsoShapeType.msoLine)
                {
                    if (item.HorizontalFlip == item.VerticalFlip)
                    {
                        if (item.VerticalFlip == Microsoft.Office.Core.MsoTriState.msoFalse)
                        {
                            //左下到右上
                            Console.WriteLine("左上");
                            ESRI.ArcGIS.Geometry.IPoint to = new ESRI.ArcGIS.Geometry.PointClass();
                            to.PutCoords(item.Left * unitx, -item.Top * unitx);
                            ESRI.ArcGIS.Geometry.IPoint from = new ESRI.ArcGIS.Geometry.PointClass();
                            from.PutCoords(item.Left * unitx + item.Width * unitx, -item.Top * unitx - item.Height * unitx);
                            PolylineClass line = new PolylineClass();
                            line.AddPoint(to);
                            line.AddPoint(from); line.Simplify();
                            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
                            LineElementClass ele = new LineElementClass();
                            ele.Symbol = new SimpleLineSymbolClass { Color = bgcolor };
                            ele.Geometry = line;
                            elements.Add(ele);
                            //pContainer.AddElement(ele, 0);
                        }
                        else
                        {
                            //上到左
                            Console.WriteLine("左上");
                            ESRI.ArcGIS.Geometry.IPoint to = new ESRI.ArcGIS.Geometry.PointClass();
                            to.PutCoords(item.Left * unitx, -item.Top * unitx);
                            ESRI.ArcGIS.Geometry.IPoint from = new ESRI.ArcGIS.Geometry.PointClass();
                            from.PutCoords(item.Left * unitx + item.Width * unitx, -item.Top * unitx - item.Height * unitx);
                            PolylineClass line = new PolylineClass();
                            line.AddPoint(to);
                            line.AddPoint(from); line.Simplify();
                            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
                            LineElementClass ele = new LineElementClass(); ele.Symbol = new SimpleLineSymbolClass { Color = bgcolor };
                            ele.Geometry = line;
                            elements.Add(ele);
                           // pContainer.AddElement(ele, 0);
                        }

                    }
                    else if (item.HorizontalFlip == Microsoft.Office.Core.MsoTriState.msoFalse)
                    {
                        Console.WriteLine("左下");
                        ESRI.ArcGIS.Geometry.IPoint to = new ESRI.ArcGIS.Geometry.PointClass();
                        to.PutCoords(item.Left * unitx, -item.Top * unitx - item.Height * unitx);
                        ESRI.ArcGIS.Geometry.IPoint from = new ESRI.ArcGIS.Geometry.PointClass();
                        from.PutCoords(item.Left * unitx + item.Width * unitx, -item.Top * unitx);
                        PolylineClass line = new PolylineClass();
                        line.AddPoint(to);
                        line.AddPoint(from); line.Simplify();
                        IGraphicsContainer pContainer = pAc as IGraphicsContainer;
                        LineElementClass ele = new LineElementClass(); ele.Symbol = new SimpleLineSymbolClass { Color = bgcolor };
                        ele.Geometry = line;
                        elements.Add(ele);
                     //   pContainer.AddElement(ele, 0);
                    }
                    else
                    { //down -left
                        Console.WriteLine("左下");
                        ESRI.ArcGIS.Geometry.IPoint to = new ESRI.ArcGIS.Geometry.PointClass();
                        to.PutCoords(item.Left * unitx, -item.Top * unitx - item.Height * unitx);
                        ESRI.ArcGIS.Geometry.IPoint from = new ESRI.ArcGIS.Geometry.PointClass();
                        from.PutCoords(item.Left * unitx + item.Width * unitx, -item.Top * unitx);
                        PolylineClass line = new PolylineClass();
                        line.AddPoint(to);
                        line.AddPoint(from); line.Simplify();
                        IGraphicsContainer pContainer = pAc as IGraphicsContainer;
                        LineElementClass ele = new LineElementClass(); ele.Symbol = new SimpleLineSymbolClass { Color = bgcolor };
                        ele.Geometry = line;
                        elements.Add(ele);
                       // pContainer.AddElement(ele, 0);
                    }







                }
            }
            #endregion
            Range cell_ =eg.worksheet.Cells[1, 1] as Range;
            dic.Clear();
            //单元格
            Queue<Range> cellQueues = new Queue<Range>();
            cellQueues.Enqueue(cell_);
            while (cellQueues.Count > 0)
            {
                Range cell=cellQueues.Dequeue();
                int row = cell.Row;
                int column = cell.Column;
                string key = row.ToString() + "_" + column.ToString();
                if (dic.ContainsKey(key))
                {
                    continue;
                }
                dic[key] = true;
                int neighborx = 1;
                int neighbory = 1;
                if(bool.Parse( cell.MergeCells.ToString()))
                {
                    neighbory =cell.MergeArea.Cells.Rows.Count; 
                    neighborx= cell.MergeArea.Cells.Columns.Count;
                    for (int j = 0; j <= neighbory - 1; j++)
                    {
                        for (int i = 0; i <= neighborx-1; i++)
                        {
                           
                            key = (row+j).ToString() + "_" + (column+i).ToString();
                            dic[key] = true;
                        }
                    }

                }
                try
                {
                    DrawCellLine(cell);

                }
                catch
                {

                } 
                Range cellright =eg.worksheet.Cells[row, column + neighborx] as Range;
                if (cellright.Value != null)//右边值不为空
                {
                    cellQueues.Enqueue(cellright);
                }  
                Range celldown = eg.worksheet.Cells[row + neighbory, column] as Range;
                if (celldown.Value != null)//下值不为空
                {
                    cellQueues.Enqueue(celldown);

                }  
            }
            
           
        }

        /// <summary>
        /// 递归扩展算法:向下和向右扩张
        /// </summary>
        private void ExcelCellExpand(Range cell,ExcelGenerator eg)
        {
            try
            {
                int row = cell.Row;
                int column = cell.Column;
                if (row == 2 && column == 2)
                {
                }
                var mergeInfo = MergeCellInofs.Where(x => (x.row == row && x.column == column));
                if (mergeInfo.Count() > 0)//被合并的不处理
                    return;

                var infos = CellInofs.Where(x => (x.row == row && x.column == column));
                if (infos.Count() == 0)
                    return;
                cellInfos cellinfo = infos.First();//当前的单元格       
                DrawCellElement(cellinfo, eg);
                //获取邻居单元格
                int neighborx = 1;
                int neighbory = 1;

                int index = (int)XlLineStyle.xlLineStyleNone;

                if (cell.MergeArea.Cells.Count > 1)//合并的单元格
                {
                    if (cell.MergeArea.Cells.Rows.Count > 1)//行
                    {

                        neighbory = cell.MergeArea.Cells.Rows.Count;
                        for (int i = 1; i < neighbory; i++)
                        {
                            cellInfos cellmerge = new cellInfos() { row = row + i, column = column };
                            MergeCellInofs.Add(cellmerge);
                        }
                    }
                    else if (cell.MergeArea.Cells.Columns.Count > 1)//列
                    {

                        neighborx = cell.MergeArea.Cells.Columns.Count;
                        
                    }
                }

                if (!CheckProcess(row, column + neighborx))
                {
                    Range cell0 = eg.worksheet.Cells[row, column + neighborx];
                    if ((eg.worksheet.Cells[row, column + neighborx]).Value != null)//右边值不为空
                    {
                        double width = cell.MergeArea.Width;
                        double height = cellinfo.height;
                        width += cellinfo.width;
                        cellInfos cellright = new cellInfos() { row = row, column = column + neighborx, width = width, height = height };
                        CellInofs.Add(cellright);
                        ExcelCellExpand(cell0, eg);
                    }
                    else if (cell0.Borders.get_Item(XlBordersIndex.xlEdgeTop).LineStyle != index
                         && cell0.Borders.get_Item(XlBordersIndex.xlEdgeBottom).LineStyle != index
                         && cell0.Borders.get_Item(XlBordersIndex.xlEdgeLeft).LineStyle != index
                         && cell0.Borders.get_Item(XlBordersIndex.xlEdgeRight).LineStyle != index)
                    {
                        double width = cell.MergeArea.Width;
                        double height = cellinfo.height;
                        width += cellinfo.width;
                        cellInfos cellright = new cellInfos() { row = row, column = column + neighborx, width = width, height = height };
                        CellInofs.Add(cellright);
                        ExcelCellExpand(cell0, eg);
                        //double width = cellinfo.width;
                        //double height = cell.MergeArea.Height;
                        //height += cellinfo.height;
                        //cellInfos celldown = new cellInfos() { row = row, column = column + neighborx, width = width, height = height };
                        //CellInofs.Add(celldown);
                        //ExcelCellExpand(cell0, eg);
                    }
                }
                if (!CheckProcess(row + neighbory, column))
                {
                    Range cell0 = eg.worksheet.Cells[row + neighbory, column];
                    if ((eg.worksheet.Cells[row + neighbory, column]).Value != null)//左边值不为空
                    {
                        double width = cellinfo.width;
                        double height = cell.MergeArea.Height;
                        height += cellinfo.height;
                        cellInfos celldown = new cellInfos() { row = row + neighbory, column = column, width = width, height = height };
                        CellInofs.Add(celldown);
                        ExcelCellExpand(cell0, eg);

                    }
                    else if (cell0.Borders.get_Item(XlBordersIndex.xlEdgeTop).LineStyle != index
                          && cell0.Borders.get_Item(XlBordersIndex.xlEdgeBottom).LineStyle != index
                          && cell0.Borders.get_Item(XlBordersIndex.xlEdgeLeft).LineStyle != index
                          && cell0.Borders.get_Item(XlBordersIndex.xlEdgeRight).LineStyle != index)
                    {
                        double width = cellinfo.width;
                        double height = cell.MergeArea.Height;
                        height += cellinfo.height;
                        cellInfos celldown = new cellInfos() { row = row + neighbory, column = column, width = width, height = height };
                        CellInofs.Add(celldown);
                        ExcelCellExpand(cell0, eg);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        private bool CheckProcess(int row,int column)
        {
            var infos = CellInofs.Where(x => (x.row == row && x.column == column));
            if(infos.Count()==0)
            { 
                return false;
            }
            return true;
        }
        private void getFontColor(int colorIndex)
        {
             
        }
        private IColor getColor(int c)
        {
            IRgbColor co = new RgbColorClass();
            int r = 0xff & c;
            int g = 0xFF00 & c;
            g >>= 8;
            int b = 0xFF0000 & c;
            b >>= 16;
            co.Red = r;
            co.Green = g;
            co.Blue = b;
            return co;

        }
        //绘制第一个标题单元格
        private void DrawCellTitleBgEle(Range cell,ESRI.ArcGIS.Geometry.IPoint originpt)
        {
            try
            {
                int row = cell.Row;
                int column = cell.Column;
                var infos = CellInofs.Where(x => (x.row == row && x.column == column));
                cellInfos cellinfo = infos.First();//当前的单元格       
                //是否有边线
                bool borderflag = true;

                if (cell.Borders.get_Item(XlBordersIndex.xlEdgeTop).LineStyle == (int)XlLineStyle.xlLineStyleNone)
                {
                    borderflag = false;
                }
                if (borderflag)
                {
                    return;
                }
                //绘制空的背景色
                //获取邻居单元格

                double width = cell.MergeArea.Width * 1e-3 * MapScale;
                double height = cell.MergeArea.Height * 1e-3 * MapScale;
                IFillShapeElement polygonElement = new PolygonElementClass();
                ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
                smsymbol.Style = esriSimpleFillStyle.esriSFSNull; 
                ISimpleLineSymbol linesym = new SimpleLineSymbolClass();
                linesym.Style = esriSimpleLineStyle.esriSLSNull;
                smsymbol.Outline = linesym;
                polygonElement.Symbol = smsymbol;

                IElement pEl = polygonElement as IElement;
                IGeometry polygon = CreateRectangle(originpt, width, height);
                pEl.Geometry = polygon as IGeometry;

                elements.Add(pEl);
            }
            catch (Exception ex)
            {
                MessageBox.Show("绘制标题出错："+ex.Message);
            }
          
        }
        private void DrawCellElement(cellInfos cellinfo, ExcelGenerator eg)
        {
            try
            {
                ESRI.ArcGIS.Geometry.IPoint point = new ESRI.ArcGIS.Geometry.PointClass();
                double dx = cellinfo.width * 1e-3 * MapScale;
                double dy = cellinfo.height * 1e-3 * MapScale;
                point.PutCoords(pBasePoint.X + dx, pBasePoint.Y - dy);
                double row = cellinfo.row;
                double column = cellinfo.column;

                Range cell0 = eg.worksheet.Cells[row, column];
                //是否有边线
                bool borderflag = true;
                double borderwidth = 0.01;
                if (cell0.Borders.get_Item(XlBordersIndex.xlEdgeTop).LineStyle == (int)XlLineStyle.xlLineStyleNone && cell0.Borders.get_Item(XlBordersIndex.xlEdgeBottom).LineStyle == (int)XlLineStyle.xlLineStyleNone)
                {
                    borderflag = false;
                }
                int g = cell0.Borders.get_Item(XlBordersIndex.xlEdgeTop).Weight;
                if (cell0.Borders.get_Item(XlBordersIndex.xlEdgeTop).Weight == (int)XlBorderWeight.xlThick)
                {
                    borderwidth = 0.03;
                }
                var gbcolor = cell0.Interior.Color;//单元格背景色

                int ct = Convert.ToInt32(gbcolor);
                IColor prgb = getColor(ct);



                double width = cell0.MergeArea.Width * 1e-3 * MapScale;
                double height = cell0.MergeArea.Height * 1e-3 * MapScale;
                if (cell0.Row == cell0.MergeArea.Row && cell0.Column != cell0.MergeArea.Column)
                {
                    //说明是行合并的单元格。并且不是第一个。不处理
                    return;
                }
                
                if (row == 1)
                {
                    totalWidth += width;
                }
                if (column == 1)
                {
                    totalHeight += height;
                }
                CreateElement(point, width, height, prgb, borderflag);
                if (cell0.Value != null)
                {
                    CreateTxtElement(cell0, point, width, height, Convert.ToString(cell0.Value));
                }
            }
            catch(Exception ex)
            {
            }
        }
        //绘制excel单元格
        private void CreateElement(ESRI.ArcGIS.Geometry.IPoint upleft, double width, double height, IColor bgcolor,bool border)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            try
            {
                IFillShapeElement polygonElement = new PolygonElementClass();
                ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
                smsymbol.Style = esriSimpleFillStyle.esriSFSSolid;
                smsymbol.Color = bgcolor;
                ISimpleLineSymbol linesym = new SimpleLineSymbolClass();
                linesym.Color = innerLineColor;
                linesym.Width = innerLineWidth;
                if (!border)
                {
                    linesym.Color = bgcolor;
                    //linesym.Width = 0.01;
                    linesym.Style = esriSimpleLineStyle.esriSLSNull;
                    IRgbColor rgb = bgcolor as IRgbColor;
                    if (rgb.Red == 255 && rgb.Blue == 255 && rgb.Green == 255)
                    {
                        return;
                    }
                }
                smsymbol.Outline = linesym;
                polygonElement.Symbol = smsymbol;

                pEl = polygonElement as IElement;
                IGeometry polygon = CreateRectangle(upleft, width, height);
                pEl.Geometry = polygon as IGeometry;
            
                elements.Add(pEl);
               
            }
            catch
            {

            }
        }
        //绘制单元格文字
        private void CreateTxtElement(Range cell0, ESRI.ArcGIS.Geometry.IPoint upleft, double width, double height, string val)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            try
            {
                int indx=1;//默认 黑色
                if (cell0.Font.ColorIndex is DBNull)
                {
                    indx = 1;
                }
                else
                {
                   indx = cell0.Font.ColorIndex;
                }
                double size = 15;
                string strsize = cell0.Font.Size.ToString();
                double r=0;
                double.TryParse(strsize,out r);
                if (r!=0)
                {
                    size = r;
                }
               
              
                //size=size*0.15;
                System.Drawing.FontStyle fs = System.Drawing.FontStyle.Regular;
                string fontname = string.Empty;
                if (cell0.Font.Name is DBNull)
                {
                    fontname = "宋体";
                }
                else
                {
                    fontname = cell0.Font.Name;
                }
                if (cell0.Font.Bold)
                {
                    fs = System.Drawing.FontStyle.Bold;
                }

                IColor colorfont = null;
                if (FontColorDic.ContainsKey(indx))
                {
                    colorfont = FontColorDic[indx];
                }
                else//如果没有默认为黑色
                {
                    colorfont = FontColorDic[1];
                }
                System.Drawing.Font font = new System.Drawing.Font(fontname, 12, fs);
                IFontDisp pFont = ESRI.ArcGIS.ADF.COMSupport.OLE.GetIFontDispFromFont(font) as IFontDisp;
                ITextSymbol pTextSymbol = new TextSymbolClass()
                {
                    Color = colorfont,
                    Font = pFont,
                    Size = size
                };


                ITextElement ptxt = new TextElementClass();
                ptxt.Text = val.Trim();
                ptxt.ScaleText = true;
                ptxt.Symbol = pTextSymbol;
                pEl = ptxt as IElement;

                int textRows = Regex.Matches(val.Trim(), @"\n").Count;
                double fontheight = size * 12;
                int txtH = cell0.HorizontalAlignment;
                int txtV = cell0.VerticalAlignment;
                ////计算文字长度
                //double txtnum = GetStrLen(val.Trim()) / 2;
                //double fontwidth = size * 12 * txtnum;
                //double dx = 21.6;
                //ESRI.ArcGIS.Geometry.IPoint pt = txtstyle(upleft, width, height, txtH, txtV, fontheight, fontwidth, dx, textRows);
                //List<object> listobj = new List<object>();
                //listobj.Add(FontColorDic[indx]);
                //listobj.Add(val.Trim());
                //numAnno.Add(pt, listobj);
                //fontstyle.Add(ptxt);

                //单元格多行文字按换行符分割
                string[] strs = val.Split('\n');
                for (int i = 0; i < strs.Length; i++)
                {
                    //计算文字长度
                    string str = strs[i];
                    double txtnum = GetStrLen(str) / 2;
                    double fontwidth = size * 12 * txtnum;
                    double dx = 21.6;
                    ESRI.ArcGIS.Geometry.IPoint pt = txtstyleEx(upleft, width, height, txtH, txtV, fontheight, fontwidth, dx, i, strs.Length);
                    List<object> listobj = new List<object>();
                    listobj.Add(FontColorDic[indx]);
                    listobj.Add(str);
                    numAnno.Add(pt, listobj);
                    fontstyle.Add(ptxt);
                }
            }
            catch (Exception ex)
            {

            }
        }
       
        //获取文字对齐方式
        private ESRI.ArcGIS.Geometry.IPoint txtstyleEx(ESRI.ArcGIS.Geometry.IPoint upleft, double width, double height, int H, int V, double fontheight, double fontwidth, double dx, int textRow,int rowCount)
        {
            double totalTxtHeight = rowCount * fontheight;
            double exHeight = (height - totalTxtHeight)/2;
            ESRI.ArcGIS.Geometry.IPoint pt = new ESRI.ArcGIS.Geometry.PointClass();
            double x = 0; double y = 0;
            if (H == -4131 && V == -4160)//左上
            {
                x = upleft.X + fontwidth * 0.5;
                y = upleft.Y - fontheight - 0.1 * fontheight -textRow*fontheight;
            }
            else if (H == -4131 && V == -4108)//左中
            {
                x = upleft.X + fontwidth * 0.5;
                y = upleft.Y - exHeight -1* fontheight - textRow * fontheight;
            }
            else if (H == -4131 && V == -4107)//左下
            {
                x = upleft.X + fontwidth * 0.5;
                y = upleft.Y - height + totalTxtHeight - fontheight - textRow * fontheight;
            }
            else if (H == -4152 && V == -4160)//右上
            {
                x = upleft.X + width - fontwidth * 0.5;
                y = upleft.Y - fontheight - 0.1 * fontheight - textRow * fontheight;
            }
            else if (H == -4152 && V == -4108)//右中
            {
                x = upleft.X + width - fontwidth * 0.5;
                y = upleft.Y - exHeight - 1* fontheight - textRow * fontheight;
            }
            else if (H == -4152 && V == -4107)//右下
            {
                x = upleft.X + width - fontwidth * 0.5;
                y = upleft.Y - height + totalTxtHeight - fontheight - textRow * fontheight;
            }
            else if (H == -4108 && V == -4160)//中上
            {
                x = upleft.X + width * 0.5;
                y = upleft.Y - fontheight - 0.1 * fontheight - textRow * fontheight;
            }
            else if (H == -4108 && V == -4108)//中中
            {
                x = upleft.X + width * 0.5;
                y = upleft.Y - exHeight - 1 * fontheight - textRow * fontheight;
            }
            else if (H == -4108 && V == -4107)//中下
            {
                x = upleft.X + width * 0.5;
                y = upleft.Y - height + totalTxtHeight - fontheight - textRow * fontheight;
            }
            else//其他:默认居中
            {
                x = upleft.X + width * 0.5;
                y = upleft.Y - exHeight - 0.5 * fontheight - 0.1 * fontheight - textRow * fontheight;
            }
            pt.PutCoords(x, y);
            return pt;
        }
        //判断文字居中方式
        private ESRI.ArcGIS.Geometry.IPoint txtstyle(ESRI.ArcGIS.Geometry.IPoint upleft, double width, double height, int H, int V, double fontheight, double fontwidth,double dx,int textRows)
        {
            ESRI.ArcGIS.Geometry.IPoint pt = new ESRI.ArcGIS.Geometry.PointClass();
            double x = 0; double y = 0;
            if (H == -4131 && V == -4160)//左上
            {
                x = upleft.X + fontwidth * 0.5 ;
                y = upleft.Y - fontheight - 0.1 * fontheight;
            }
            else if (H == -4131 && V == -4108)//左中
            {
                x = upleft.X + fontwidth * 0.5 ;
                y = upleft.Y - height * 0.5 - 0.5 * fontheight - 0.1 * fontheight;
                if (textRows > 0)
                {
                    y = y + textRows * 0.5 * fontheight;
                }
            }
            else if (H == -4131 && V == -4107)//左下
            {
                x = upleft.X + fontwidth * 0.5 ;
                y = upleft.Y - height;
                if (textRows > 0)
                {
                    y = y + textRows * 1 * fontheight;
                }
            }
            else if (H == -4152 && V == -4160)//右上
            {
                x = upleft.X + width - fontwidth * 0.5;
                y = upleft.Y - fontheight - 0.1 * fontheight;
            }
            else if (H == -4152 && V == -4108)//右中
            {
                x = upleft.X + width - fontwidth * 0.5 ;
                y = upleft.Y - height * 0.5 - 0.5 * fontheight - 0.1 * fontheight;
                if (textRows > 0)
                {
                    y = y + textRows * 0.5 * fontheight;
                }
            }
            else if (H == -4152 && V == -4107)//右下
            {
                x = upleft.X + width - fontwidth * 0.5 ;
                y = upleft.Y - height;
                if (textRows > 0)
                {
                    y = y + textRows * 1 * fontheight;
                }
            }
            else if (H == -4108 && V == -4160)//中上
            {
                x = upleft.X + width * 0.5;
                y = upleft.Y - fontheight - 0.1 * fontheight;
            }
            else if (H == -4108 && V == -4108)//中中
            {
                x = upleft.X + width * 0.5;
                y = upleft.Y - height * 0.5 - 0.5 * fontheight - 0.1 * fontheight;
                if (textRows > 0)
                {
                    y = y + textRows * 0.5 * fontheight;
                }
            }
            else if (H == -4108 && V == -4107)//中下
            {
                x = upleft.X + width * 0.5;
                y = upleft.Y - height;
                if (textRows > 0)
                {
                    y = y + textRows * 1 * fontheight;
                }
            }
            else//其他:默认居中
            {
                x = upleft.X + width * 0.5;
                y = upleft.Y - height * 0.5 - 0.5 * fontheight - 0.1 * fontheight;
                if (textRows > 0)
                {
                    y = y + textRows * 0.5 * fontheight;
                }
            }
            pt.PutCoords(x, y);
            return pt;
        }
        Dictionary<ESRI.ArcGIS.Geometry.IPoint, List<object>> numAnno = new Dictionary<ESRI.ArcGIS.Geometry.IPoint, List<object>>();
        List<ITextElement> fontstyle = new List<ITextElement>();
       
        List<ESRI.ArcGIS.Geometry.IPoint> xy = new List<ESRI.ArcGIS.Geometry.IPoint>();
        private IPolygon CreateRectangle(ESRI.ArcGIS.Geometry.IPoint upleftpoint, double width, double height)
        {
            try
            {
                IGeometryCollection pClipRec = new PolygonClass();
                IPointCollection pCl = new RingClass();
                double cx = upleftpoint.X;
                double cy = upleftpoint.Y;
                double x = cx + width;
                //double x2 = cx + width *0.5;
                double y = cy - height;
                //double y2 = cy - height*0.8;
                ESRI.ArcGIS.Geometry.IPoint pt = new ESRI.ArcGIS.Geometry.PointClass() { X = x, Y = y };
                //ESRI.ArcGIS.Geometry.IPoint pt2 = new ESRI.ArcGIS.Geometry.PointClass() { X = x2, Y = y2 };
                //txtpoint.Add(pt2);
                xy.Add(pt);
                pCl.AddPoint(upleftpoint);
                pCl.AddPoint(new PointClass() { X = cx + width, Y = cy });
                pCl.AddPoint(new PointClass() { X = cx + width, Y = cy - height });
                pCl.AddPoint(new PointClass() { X = cx, Y = cy - height });
                (pCl as IRing).Close();
                pClipRec.AddGeometry(pCl as IGeometry);
                (pClipRec as ITopologicalOperator).Simplify();
                return pClipRec as IPolygon;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private ILineSymbol pLineSymbol()
        {
            ILineSymbol linesym = new SimpleLineSymbolClass();
            linesym.Width = 0.01;
            linesym.Color = null;
            return null;
        }
        //将element写自由表达
        private IRepresentationMarker CreateFeatures(List<IElement> eles, ILayer player, out int id)
        {
            
            IFeatureClass pointfcl = (player as IFeatureLayer).FeatureClass;

            IFeature fe = pointfcl.CreateFeature();
            if (isCorner)
            {
                ESRI.ArcGIS.Geometry.IPoint orginPoint = null;
                double curms = pAc.FocusMap.ReferenceScale;
                double size = curms * 1.0e-3 * markerSize;
                double contentWidthScale = totalWidth / (totalWidth + horizontalCount * outBorderSpacing);
                double contentHeightScale = totalHeight / (totalHeight + verticalCount * outBorderSpacing);
                //double geoColinWidth = curms * 1.0e-3 * 0.3;    //内图廓线宽度
                //1.确定excel表格长度和高度
                double width;
                double height;
                if (totalHeight < totalWidth)
                {
                    width = size * contentWidthScale;
                    height = (size * totalHeight / totalWidth) * contentHeightScale;
                }
                else
                {
                    height = size * contentHeightScale;
                    width = (size * totalWidth / totalHeight)*contentWidthScale;
                }
                switch (tableLocation)
                {
                    case "左上":
                        orginPoint = clipGeoIn.UpperLeft;
                        break;
                    case "右上":
                        orginPoint = clipGeoIn.UpperRight;
                        orginPoint.X -= width;
                        break;
                    case "左下":
                        orginPoint = clipGeoIn.LowerLeft;
                        orginPoint.Y += height; 
                        break;
                    case "右下":
                        orginPoint = clipGeoIn.LowerRight;
                        orginPoint.X -= width;
                        orginPoint.Y += height; 
                        break;
                    default:
                        break;
                }
                pBasePoint2 = orginPoint;
            }
            fe.Shape = pBasePoint2;
            fe.set_Value(pointfcl.FindField("TYPE"),"专题表格");
            // fe.set_Value(pointfcl.FindField("ruleID"), 1);
            fe.Store();
            id = fe.OID;
            //获取当前要素的制图表达

            // var rpr = (axMapControl1.get_Layer(2) as IGeoFeatureLayer).Renderer as IRepresentationRenderer;

            IRepresentationGraphics g = new RepresentationMarkerClass();

            IMapContext mc = new MapContextClass();
            mc.InitFromDisplay(pAc.ScreenDisplay.DisplayTransformation);
            ColorNULL = ObtainNULLColor();
            foreach (var e in eles)
            {
                FromElement(e, g);

            }
            IRepresentationRule r = new RepresentationRuleClass();
            var bs = new BasicMarkerSymbolClass();
            bs.set_Value(1, g); //marker
            bs.set_Value(2, markerSize*2.8345); //size
            bs.set_Value(3, 0);//angle
            bs.set_Value(4, false);
            r.InsertLayer(0, bs);
            //自由
            //GetRule();
            var rp = (player as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            IRepresentation rep = m_RepClass.GetRepresentation(fe, mc);
            IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
            repGraphics.Add(pBasePoint2, r);
            rep.Graphics = repGraphics;
            rep.UpdateFeature();
            rep.Feature.Store();
            return g as IRepresentationMarker;
        }
      
        void FromElement(IElement el, IRepresentationGraphics g)
        {
            if (el == null || g == null)
                return;

            IGeometry geo1 = el.Geometry;
            IMapContext mc = new MapContextClass();

            mc.InitFromDisplay(pAc.ScreenDisplay.DisplayTransformation);
            //var geomap = mc.FromGeographyToMap(geo1);
            //var geocenter = mc.FromGeographyToMap(pBasePoint) as ESRI.ArcGIS.Geometry.IPoint;
            //IGeometry geo = (geomap as IClone).Clone() as IGeometry;
            IGeometry geo = geo1;
            //ITransform2D ptrans2d = geo as ITransform2D;
            //ptrans2d.Move(-geocenter.X, -geocenter.Y);
            if (el is IGroupElement)
            {
                var gl = el as IGroupElement;
                for (int i = 0; i < gl.ElementCount; i++)
                {
                    IElement ell = gl.Element[i];
                    FromElement(ell, g);
                }
            }
            else if (el is IFillShapeElement)
            {
                var symbol = (el as IFillShapeElement).Symbol;
                var rule = new RepresentationRuleClass();

                rule.InitWithSymbol(symbol as ISymbol);
                CheckSymbolNUll(symbol as IFillSymbol, rule as IRepresentationRule);
                g.Add(geo, rule);
            }
            else if (el is ILineElement)
            {
                var symbol = (el as ILineElement).Symbol;
               
                var rule = new RepresentationRuleClass();
                rule.InitWithSymbol(symbol as ISymbol);
                g.Add(geo, rule);
            }
            else if (el is IMarkerElement)
            {
                var symbol = (el as IMarkerElement).Symbol;
               
                var rule = new RepresentationRuleClass();
                rule.InitWithSymbol(symbol as ISymbol);
                g.Add(geo, rule);
            }
            else if (el is ITextElement)
            {
                var te = el as ITextElement;
                var text = te.Text;
                var symbol = te.Symbol;
                var xx = symbol as ISimpleTextSymbol;

                if (!(geo1 is ESRI.ArcGIS.Geometry.IPoint) || text == null || text.Length < 0)
                {
                    return;
                }
                var texts = text.Split('\n', '\r');
                AddTxtElement(geo1 as ESRI.ArcGIS.Geometry.IPoint, text, te.Symbol, g);
                


            }
        }
        IColor ColorNULL = null;
        //获取空颜色测试
        private IColor ObtainNULLColor()
        {
            ICmykColor pnullcolor = new CmykColorClass();
            pnullcolor.CMYK = 6;
            pnullcolor.Black = 6;
            pnullcolor.Cyan = 0;
            pnullcolor.Magenta = 0;
            pnullcolor.NullColor = true;
            pnullcolor.RGB = 15790320;
            pnullcolor.Transparency = 0;
            pnullcolor.UseWindowsDithering = true;
            pnullcolor.Yellow = 0;
            return pnullcolor;


        }
        private bool CheckSymbolNUll(IFillSymbol symbol, IRepresentationRule rule)
        {
            bool flag = false;
            IFillSymbol pfillsym = symbol as IFillSymbol;
            ISimpleLineSymbol psls = pfillsym.Outline as ISimpleLineSymbol;
            if (psls != null)
            {
                if (psls.Style == esriSimpleLineStyle.esriSLSNull)
                {

                    int index = 0;
                    for (int i = 0; i < rule.LayerCount; i++)
                    {
                        IBasicSymbol pbasic = rule.get_Layer(i);
                        if (pbasic is IBasicLineSymbol)
                        {
                            break;
                        }
                        index++;
                    }
                    rule.RemoveLayer(index);
                    flag = true;
                }
            }
            ISimpleFillSymbol psfs = symbol as ISimpleFillSymbol;
            if (psfs != null)
            {
                if (psfs.Style == esriSimpleFillStyle.esriSFSNull)
                {

                    int index = 0;
                    for (int i = 0; i < rule.LayerCount; i++)
                    {
                        IBasicSymbol pbasic = rule.get_Layer(i);
                        if (pbasic is IBasicFillSymbol)
                        {

                            IBasicFillSymbol pfsym = pbasic as IBasicFillSymbol;
                            IFillPattern fillpattern = pfsym.FillPattern;
                            IGraphicAttributes fillAttrs = fillpattern as IGraphicAttributes;
                            fillAttrs.set_Value(0, ColorNULL);
                            // IColor color = fillAttrs.get_Value(0) as IColor;
                            // break;
                        }
                        index++;
                    }
                    // rule.RemoveLayer(index);
                }
            }

            return flag;
        }
       
        private void TransMapPoint(char c, ESRI.ArcGIS.Geometry.IPoint txtpoint, ITextSymbol symbol, IRepresentationGraphics g)
        {
            IMapContext mc = new MapContextClass();

            mc.InitFromDisplay(pAc.ScreenDisplay.DisplayTransformation);
            var geomap = mc.FromGeographyToMap(txtpoint);
            var geocenter = mc.FromGeographyToMap(pBasePoint) as ESRI.ArcGIS.Geometry.IPoint;
            IGeometry geo = (geomap as IClone).Clone() as IGeometry;
            ITransform2D ptrans2d = geo as ITransform2D;
            ptrans2d.Move(-geocenter.X, -geocenter.Y);
            var xx = symbol as ISimpleTextSymbol;
            var cms = new CharacterMarkerSymbolClass();

            cms.CharacterIndex = (int)c;
            cms.Color = xx.Color;
            cms.Size = xx.Size;
            cms.Font = xx.Font;
            cms.Angle = xx.Angle;


            var rule = new RepresentationRuleClass();
            rule.InitWithSymbol(cms as ISymbol);
            var pt = ptrans2d as ESRI.ArcGIS.Geometry.IPoint;
            //获取每个文字的坐标
            g.Add(pt, rule);
            
        }
        private void AddTxtElement(ESRI.ArcGIS.Geometry.IPoint txtpoint, string txt, ITextSymbol symbol, IRepresentationGraphics g_)
        {
            double fsize = (symbol as ISimpleTextSymbol).Size;
            double dx = 0;
            var splits = txt.Split('\n', '\r');
            List<string> lists = new List<string>();
            foreach (string str in splits)
            {
                if (str.Trim() != "")
                {
                    lists.Add(str);
                }
            }
            double txtunit = 3.52778;//1号字体和宽度
            txtunit = MapScale * txtunit * 1e-4;
            double heightunit = 3.97427;//1号字体1万的高度
            heightunit = MapScale * heightunit * 1e-4;
           
            string[] texts = lists.ToArray();
            int row = texts.Length - 1;//行
            foreach (var t in texts)
            {
                dx = txtpoint.X;
                int col = 0;
                double dis = 0;
                double tedis = 0;
                string tt = (string)t;
                double ttnum = GetStrLen(tt);
                dis = ttnum / 2 * txtunit * fsize / 2;//当前文字一半长度
                dx = dx - dis;
                for (int i = 0; i < t.Length; i++, col++)
                {
                    string temp = t[i].ToString();
                    double tempnum = GetStrLen(temp);
                    //当前的位置
                    double x = dx + tedis + tempnum * txtunit * fsize / 2 / 2;
                    double y = txtpoint.Y + row * heightunit * fsize;
                    //y += heightunit * fsize / 4;//修正，上移半个字体
                    ESRI.ArcGIS.Geometry.IPoint p = new ESRI.ArcGIS.Geometry.PointClass() { X = x, Y = y };
                    //将当前点添加到制图表达中
                    TransMapPoint(t[i], p, symbol, g_);
                    tedis += tempnum * txtunit * fsize / 2;
                }
                row--;
            }
        }
        //获取文字总长度
        private double GetStrLen(string str)
        {
            string [] strs = str.Split('\n');
            int maxCharacterLength = 0;                
            //遍历每一行，取最大的字符长度
            foreach (var item in strs)
            {
                int ENcount = 0;                        //中文字符
                int notENcount = 0;                     //非中文字符
                for (int i = 0; i < item.Length; i++)
                {
                    if ((int)item[i] > 127)
                    {
                        ENcount++;
                    }
                    else
                    {
                        notENcount++;
                    }
                }
                if (notENcount + ENcount * 2 > maxCharacterLength)
                {
                    maxCharacterLength = notENcount + ENcount * 2;        //中文2个字符长度，非中文1个字符长度
                }
            }
            return maxCharacterLength;
            //for (int i = 0; i < str.Length; i++)
            //{
            //    if ((int)str[i] == 10)                  
            //    {
            //        break;                          
            //    }
            //    if ((int)str[i] > 127)
            //    {
            //        ENcount++;
            //    }
            //    else
            //    {
            //        notENcount++;
            //    }
                    
            //}
            //return notENcount + ENcount * 2;
        }

        private void InsertAnnoFea(IGeometry pGeometry, string annoName,ITextElement txtstyle, double fontsize, int id,IColor pcolor)
        {
            IFontDisp font = new StdFont() { Name = txtstyle.Symbol.Font.Name, Size = 2 } as IFontDisp;
            ITextElement pTextElement = CreateTextElement(pGeometry, annoName, font, fontsize, pcolor);

            IElement pElement = pTextElement as IElement;
            pElement.Geometry = pGeometry;
            IFeature pFeature = annoFcl.CreateFeature();
            IAnnotationFeature2 pAnnoFeature = pFeature as IAnnotationFeature2;
            pAnnoFeature.Annotation = pElement;
            pAnnoFeature.AnnotationClassID = (pLayer as IFeatureLayer).FeatureClass.FeatureClassID;
            pAnnoFeature.LinkedFeatureID = id;
            pFeature.Store();
            //return true;
        }


        private ITextElement CreateTextElement(IGeometry pGeoTxt, string txt, IFontDisp pFont, double size, IColor pColor)
        {
            //IRgbColor pColor = new RgbColorClass()
            //{
            //    Red = 0,
            //    Blue = 0,
            //    Green = 0
            //};

            ITextSymbol pTextSymbol = new TextSymbolClass()
            {
                Color = pColor,
                Font = pFont,
                VerticalAlignment = esriTextVerticalAlignment.esriTVABaseline,
                HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter,
                Size = size
            };
            ITextElement pTextElment = null;
            IElement pEle = null;
            pTextElment = new TextElementClass()
            {
                Symbol = pTextSymbol,
                ScaleText = true,
                Text = txt
            };
            pEle = pTextElment as IElement;

            pEle.Geometry = pGeoTxt;
            return pTextElment;
        }
        private void CreateAnnotion(IRepresentationMarker RepMarker,  double markersize,int id)
        {
            double curms = pAc.FocusMap.ReferenceScale;
            double height;
            double width;
            double contentWidthScale = totalWidth / (totalWidth + horizontalCount * outBorderSpacing);
            double contentHeightScale = totalHeight / (totalHeight + verticalCount * outBorderSpacing);
            double size = curms * 1.0e-3 * markersize;
            //1.确定excel表格长度和高度
            if (RepMarker.Height < RepMarker.Width)
            {
                width = size * contentWidthScale;
                height = (size * RepMarker.Height / RepMarker.Width) * contentHeightScale;
            }
            else
            {
                height = size * contentHeightScale;
                width = (size * RepMarker.Width / RepMarker.Height) * contentWidthScale;
            }
          

            int i = 0;
            //double dx = xy[xy.Count - 1].X - pBasePoint.X;
            //double dy = xy[xy.Count - 2].Y - pBasePoint.Y;
            double dy = -totalHeight;//表格的相对高度
            foreach (var k in numAnno)
            {
                //2.确定文字实际高度确定当前比例尺实际尺寸大小
                double txtheight = fontstyle[i].Symbol.Size * 12;
                txtheight = txtheight /Math.Abs(dy) * height;
                double dh = 0.2 * txtheight;
                //k.Value[1].ToString();
                //int textRows = Regex.Matches(k.Value[1].ToString(), @"\n").Count;
                //if (textRows > 0)
                //{
                //    dh = 0.2 * txtheight * (textRows + 1);
                //}                    
                
                double heightunit = 3.97427;//1号字体1万的高度
                double fontsize = txtheight / (curms * heightunit * 1e-4);
               
                double cx = (k.Key.X - pBasePoint.X) / dy;
                double cy = (k.Key.Y - pBasePoint.Y) / dy;
                //double cx = (txtpoint[i].X- pBasePoint.X) / dy2;
                //double cy = (txtpoint[i].Y - pBasePoint.Y) / dy2;
                ESRI.ArcGIS.Geometry.Point p = new ESRI.ArcGIS.Geometry.PointClass() { X = pBasePoint2.X - cx * height, Y = pBasePoint2.Y - cy * height + dh };
                //左边对齐
                //GraphicsHelper gh = new GraphicsHelper(pAc);
                var list = k.Value;
               //double txtwidth = gh.GetStrWidth(list[1].ToString(), curms, fontsize);
               
                fontstyle[i].Symbol.Size = fontsize;
              
                InsertAnnoFea(p, list[1].ToString(), fontstyle[i], fontsize, id, list[0] as IColor);
                i++;
            }


        }

    }
    public class cellInfos
    {
        public int row;
        public int column;
        public double height;//距离最上边距离
        public double width;//距离最左边的距离
    }
}
