using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Output;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using System.Drawing;
namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 地图输出辅助类
    /// </summary>
    public class MapExportHelper
    {
        [DllImport("GDI32.dll")]
        public static extern int GetDeviceCaps(int hdc, int nIndex);

        [DllImport("user32.dll")]
        public static extern int GetDesktopWindow();

        /* User32 delegates to getDC and ReleaseDC */
        [DllImport("User32.dll")]
        public static extern int GetDC(int hWnd);

        [DllImport("User32.dll")]
        public static extern int ReleaseDC(int hWnd, int hDC);

        //[DllImport("user32.dll", SetLastError = true)]
        //static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref int pvParam, uint fWinIni);

        /* constants used for user32 calls */
        const uint SPI_GETFONTSMOOTHING = 74;
        const uint SPI_SETFONTSMOOTHING = 75;
        const uint SPIF_UPDATEINIFILE = 0x1;

        private int DoubleToInt(double v)
        {
            return (int)Math.Floor(v + 0.5);
        }

        private long GetScreenDPI()
        {
            /* Get the device context of the screen */
            long tmpDC = GetDC(GetDesktopWindow());
            /* Get the screen resolution. */
            long screenResolution = GetDeviceCaps((int)tmpDC, 88); //88 is the win32 const for Logical pixels/inch in X) 
            ReleaseDC(0, (int)tmpDC);
            return screenResolution;
        }


        #region 辅助函数
        //设置页面大小
        public IMapFrame setPageSize(GApplication app, double pagewidth, double pageheight, ref IPoint ct)
        {
            double ms = app.ActiveView.FocusMap.ReferenceScale;
            //激活布局视图
            if (app.LayoutState == LayoutState.PageLayoutControl)
            {
                app.MapControl.Map = new MapClass();
            }
            var pg = app.PageLayoutControl.PageLayout as IPageLayout;
            var av = pg as IActiveView;


            double mapwidth = pagewidth * 1e-3 * ms;
            double mapheight = pageheight * 1e-3 * ms;
            IEnvelope extent = new EnvelopeClass { XMin = 0, YMin = 0, XMax = mapwidth, YMax = mapheight };
            //居中地图     


            var layer = app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName == "ClipBoundary")).FirstOrDefault();
            if (layer == null)
            {
                MessageBox.Show("PAGE图层为空，无法计算出图范围", "提示");
                return null;
            }
            IFeatureClass fc = (layer as IFeatureLayer).FeatureClass;
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "TYPE = '页面'";
            IFeatureCursor pCursor = fc.Search(qf, true);
            IFeature f = pCursor.NextFeature();
            if (null == f)
            {
                MessageBox.Show("无法找到页面要素，无法计算页面中心点", "提示");
                return null;
            }
            Marshal.ReleaseComObject(pCursor);

            IPoint pt = (f.Shape as IArea).Centroid;

            ct = pt;
            extent.CenterAt(pt);


            //设置纸张大小
            var gContainer = pg as IGraphicsContainer;
            IPage page = pg.Page;
           
            page.Units =  esriUnits.esriMillimeters;
            page.PutCustomSize(pagewidth, pageheight);
            //
            IElement el = null;
            IMapFrame mf = null;
            gContainer.Reset();
            while (true)
            {
                el = gContainer.Next();
                if (el == null)
                    break;

                if (el is IMapFrame && (el as IMapFrame).Map.Name == app.PageLayoutControl.ActiveView.FocusMap.Name)
                {
                    mf = el as IMapFrame;
                    break;
                }
            }
            app.PageLayoutControl.ActiveView.FocusMap = mf.Map;
            //设置比例尺
            IEnvelope pEnvelope = new EnvelopeClass();
            pEnvelope.PutCoords(0, 0, pagewidth, pageheight);
            el.Geometry = pEnvelope;
            gContainer.UpdateElement(el);


            mf.Map.ReferenceScale = ms;
            mf.MapScale = ms * 2;
            mf.Map.MapScale = ms * 2;
            (mf.Map as IActiveView).Extent = extent;
            (mf.Map as IActiveView).Refresh();

            av.Refresh();
            return mf;
        }

        public string PrintPageLayoutMap(GApplication p_Application, string printerName, int iOutputResolution, esriExportColorspace colorMode, bool tfwflag, double pageWidth = 780, double pageHeight = 610)
        {
            string err = "";

        


            ITrackCancel pCancel = new CancelTrackerClass();
            //pg.Page.GetPageBounds(printer, 0, 0, pMapExtEnv);

            //IPaper paper = new PaperClass();
            //IPrinter printer = new EmfPrinterClass();



            //paper.PrinterName = printerName;



            //尺寸
            IPoint centerPoint = new PointClass();
            IMapFrame mf = setPageSize(p_Application, pageWidth, pageHeight, ref centerPoint);
            return "";
            //try
            //{

            //    //如果纸张不够 就裁切
            //    //pPage.PageToPrinterMapping = esriPageToPrinterMapping.esriPageMappingCrop;


            //    var pg = p_Application.PageLayoutControl.PageLayout as IPageLayout;
            //    IActiveView pageActiview = pg as IActiveView;

            //    IEnvelope PixelBoundsEnv;
            //    tagRECT exportRECT = new tagRECT();

            //    IPageLayout pPageLayout;
            //    IEnvelope pMapExtEnv = new EnvelopeClass();

            //    IEnvelope pGraphicsExtentEnv;
            //    IUnitConverter pUnitConvertor;

            //    PixelBoundsEnv = new EnvelopeClass();

            //    pGraphicsExtentEnv = GetGraphicsExtent(pageActiview);
            //    pPageLayout = pageActiview as PageLayout;
            //    pUnitConvertor = new UnitConverter();

            //    double inchWidth = pUnitConvertor.ConvertUnits(pGraphicsExtentEnv.XMax, pPageLayout.Page.Units,
            //        esriUnits.esriInches) - pUnitConvertor.ConvertUnits(pGraphicsExtentEnv.XMin, pPageLayout.Page.Units, esriUnits.esriInches);
            //    double inchHeight =
            //        pUnitConvertor.ConvertUnits(pGraphicsExtentEnv.YMax, pPageLayout.Page.Units, esriUnits.esriInches) -
            //        pUnitConvertor.ConvertUnits(pGraphicsExtentEnv.YMin, pPageLayout.Page.Units, esriUnits.esriInches);
            //    //assign the x and y values representing the clipped area to the PixelBounds envelope 
            //    PixelBoundsEnv.XMin = 0;
            //    PixelBoundsEnv.YMin = 0;
            //    PixelBoundsEnv.XMax = inchWidth * iOutputResolution ;
            //    PixelBoundsEnv.YMax =inchHeight * iOutputResolution;

            //    //'assign the x and y values representing the clipped export extent to the exportRECT
            //    exportRECT.bottom = (int)(PixelBoundsEnv.YMax) + 1;
            //    exportRECT.left = (int)(PixelBoundsEnv.XMin);
            //    exportRECT.top = (int)(PixelBoundsEnv.YMin);
            //    exportRECT.right = (int)(PixelBoundsEnv.XMax) + 1;

            //    //since we're clipping to graphics extent, set the visible bounds.
            //    pMapExtEnv = pGraphicsExtentEnv;







            //    int hdc = printer.StartPrinting(pMapExtEnv, 0);


            //    p_Application.PageLayoutControl.ActiveView.Output(hdc, iOutputResolution, ref exportRECT, null, pCancel);
            //    //p_Application.ActiveView.Output(hdc, iOutputResolution, ref exportRECT, null, pCancel);
            //    printer.FinishPrinting();

            //    //SetOutputQuality(pageActiview, pPrevOutputImageQuality);


            //    //Printer.DeleteCustomPaperSize(printerName, customPaper);
            //}
            //catch (Exception ex)
            //{
            //    err = ex.Message;
            //}

            //////最后赋值回去
            //if (p_Application.LayoutState == LayoutState.PageLayoutControl)
            //{
            //    p_Application.MapControl.Map = mf.Map;

            //}
            //return err;
        }

        //输出pageLayoutMap
        public string ExportPageLayoutMap_tmp(GApplication p_Application, long iOutputResolution, string FileName, esriExportColorspace colorMode, bool tfwflag, double pageWidth = 780, double pageHeight = 610)
        {
            string err = "";

            //尺寸
            IPoint centerPoint = new PointClass();
            IMapFrame mf = setPageSize(p_Application, pageWidth, pageHeight, ref centerPoint);

            try
            {
                //输出
                #region
                var pg = p_Application.PageLayoutControl.PageLayout as IPageLayout;
                IActiveView pageActiview = pg as IActiveView;

                IExport pExporter = null;
                IEnvelope PixelBoundsEnv;
                IPageLayout pPageLayout;
                IEnvelope pGraphicsExtentEnv;
                IUnitConverter pUnitConvertor;
                IExportColorspaceSettings pExportColorSpaceSetting = null;

                ExportPDFClass ep = new ExportPDFClass();
                pExporter = ep;
                ep.EmbedFonts = true;
                ep.PolygonizeMarkers = true;
                ep.MaxVertexNumber = int.MaxValue;
                ep.Compressed = true;

                pExportColorSpaceSetting = (IExportColorspaceSettings)pExporter;
                pExportColorSpaceSetting.Colorspace = colorMode;
                pExporter.ExportFileName = FileName;
                pExporter.Resolution = iOutputResolution;

                PixelBoundsEnv = new EnvelopeClass();
                pGraphicsExtentEnv = GetGraphicsExtent(pageActiview);
                pPageLayout = pageActiview as PageLayout;
                pUnitConvertor = new UnitConverter();

                PixelBoundsEnv.XMin = 0;
                PixelBoundsEnv.YMin = 0;
                PixelBoundsEnv.XMax = pUnitConvertor.ConvertUnits(pGraphicsExtentEnv.XMax, pPageLayout.Page.Units, esriUnits.esriInches) * pExporter.Resolution - pUnitConvertor.ConvertUnits(pGraphicsExtentEnv.XMin, pPageLayout.Page.Units, esriUnits.esriInches) * pExporter.Resolution;
                PixelBoundsEnv.YMax = pUnitConvertor.ConvertUnits(pGraphicsExtentEnv.YMax, pPageLayout.Page.Units, esriUnits.esriInches) * pExporter.Resolution - pUnitConvertor.ConvertUnits(pGraphicsExtentEnv.YMin, pPageLayout.Page.Units, esriUnits.esriInches) * pExporter.Resolution;

                pExporter.PixelBounds = PixelBoundsEnv;

                var docPrintExport = new PrintAndExportClass();
                docPrintExport.Export(pageActiview, pExporter, pExporter.Resolution, true, null);

                #endregion

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                err = ex.Message;
            }

            //最后赋值回去
            if (p_Application.LayoutState == LayoutState.PageLayoutControl)
            {
                p_Application.MapControl.Map = mf.Map;

            }

            return err;

        }


        //输出pageLayoutMap
        public string ExportPageLayoutMap(GApplication p_Application, long iOutputResolution, string FileName, esriExportColorspace colorMode, bool tfwflag, double pageWidth = 780, double pageHeight = 610)
        {
            string err = "";

            //尺寸
            IPoint centerPoint = new PointClass();
            IMapFrame mf = setPageSize(p_Application, pageWidth, pageHeight, ref centerPoint);

            try
            {
                //输出
                #region
                var pg = p_Application.PageLayoutControl.PageLayout as IPageLayout;
                IActiveView pageActiview = pg as IActiveView;

                IExport pExporter = null;
                long pPrevOutputImageQuality;
                IOutputRasterSettings pOutputRasterSettings;

                IEnvelope PixelBoundsEnv;
                tagRECT exportRECT;
                tagRECT DisplayBounds;
                IDisplayTransformation pDisplayTransformation;
                IPageLayout pPageLayout;
                IEnvelope pMapExtEnv;

                long tmpDC;
                long iScreenResolution;
                bool FontSmoothingReenable = false;

                IEnvelope pGraphicsExtentEnv;
                IUnitConverter pUnitConvertor;
                IExportColorspaceSettings pExportColorSpaceSetting = null;

                if (GetFontSmoothing())
                {
                    FontSmoothingReenable = true;
                    DisableFontSmoothing();
                    if (GetFontSmoothing())
                    {
                        err = "Unable to enable Font Smoothing";

                        return err;
                    }
                }
                string _outputFileExtension = System.IO.Path.GetExtension(FileName);
                if (_outputFileExtension == ".ai")
                {
                    pExporter = new ExportAIClass();
                    pExportColorSpaceSetting = (IExportColorspaceSettings)pExporter;
                    pExportColorSpaceSetting.Colorspace = colorMode;

                }
                else if (_outputFileExtension == ".jpg")
                {
                    pExporter = new ExportJPEGClass();
                }
                else if (_outputFileExtension == ".pdf")
                {
                    ExportPDFClass ep = new ExportPDFClass();
                    pExporter = ep;
                    ep.EmbedFonts = true;
                    ep.PolygonizeMarkers = true;
                    ep.MaxVertexNumber = int.MaxValue;
                    ep.Compressed = true;

                    pExportColorSpaceSetting = (IExportColorspaceSettings)pExporter;
                    pExportColorSpaceSetting.Colorspace = colorMode;

                }
                else if (_outputFileExtension == ".tif")
                {
                    var pTifExporter = new ExportTIFFClass();
                    pTifExporter.GeoTiff = true;
                    pExporter = pTifExporter;
                }
                //var ppExporter = new ExportTIFFClass();
                //ppExporter.GeoTiff = true;

                #region 设置参数
                //ExportPDFClass ep = new ExportPDFClass();
                //pExporter = ep;
                //ep.EmbedFonts = true;
                //ep.PolygonizeMarkers = true;
                //ep.MaxVertexNumber = int.MaxValue;
                //ep.Compressed = true;



                #endregion

                pOutputRasterSettings = pageActiview.ScreenDisplay.DisplayTransformation as IOutputRasterSettings;
                pPrevOutputImageQuality = pOutputRasterSettings.ResampleRatio;

                if (pExporter is IExportImage)
                {
                    SetOutputQuality(pageActiview, 1);
                }

                pExporter.ExportFileName = FileName;

                /* Get the device context of the screen */
                tmpDC = GetDC(0);
                /* Get the screen resolution. */
                iScreenResolution = GetDeviceCaps((int)tmpDC, 88); //88 is the win32 const for Logical pixels/inch in X)
                //iScreenResolution = 72; //zhouqi, for arcgis, the screen resolution is aways set to 72 points/inch
                /* release the DC. */
                ReleaseDC(0, (int)tmpDC);
                pExporter.Resolution = iOutputResolution;

                if (pageActiview is IPageLayout)
                {
                    DisplayBounds = pageActiview.ExportFrame;
                    pGraphicsExtentEnv = GetGraphicsExtent(pageActiview);
                }
                else
                {
                    pDisplayTransformation = pageActiview.ScreenDisplay.DisplayTransformation;
                    DisplayBounds = pDisplayTransformation.get_DeviceFrame();
                }

                PixelBoundsEnv = new EnvelopeClass();
                if (pageActiview is IPageLayout)
                {
                    pGraphicsExtentEnv = GetGraphicsExtent(pageActiview);
                    pPageLayout = pageActiview as PageLayout;
                    pUnitConvertor = new UnitConverter();

                    //assign the x and y values representing the clipped area to the PixelBounds envelope
                    PixelBoundsEnv.XMin = 0;
                    PixelBoundsEnv.YMin = 0;
                    PixelBoundsEnv.XMax = pUnitConvertor.ConvertUnits(pGraphicsExtentEnv.XMax, pPageLayout.Page.Units, esriUnits.esriInches) * pExporter.Resolution - pUnitConvertor.ConvertUnits(pGraphicsExtentEnv.XMin, pPageLayout.Page.Units, esriUnits.esriInches) * pExporter.Resolution;
                    PixelBoundsEnv.YMax = pUnitConvertor.ConvertUnits(pGraphicsExtentEnv.YMax, pPageLayout.Page.Units, esriUnits.esriInches) * pExporter.Resolution - pUnitConvertor.ConvertUnits(pGraphicsExtentEnv.YMin, pPageLayout.Page.Units, esriUnits.esriInches) * pExporter.Resolution;

                    //'assign the x and y values representing the clipped export extent to the exportRECT
                    exportRECT.bottom = (int)(PixelBoundsEnv.YMax) + 1;
                    exportRECT.left = (int)(PixelBoundsEnv.XMin);
                    exportRECT.top = (int)(PixelBoundsEnv.YMin);
                    exportRECT.right = (int)(PixelBoundsEnv.XMax) + 1;

                    //since we're clipping to graphics extent, set the visible bounds.
                    pMapExtEnv = pGraphicsExtentEnv;
                }


                pExporter.PixelBounds = PixelBoundsEnv;

                var docPrintExport = new PrintAndExportClass();
                docPrintExport.Export(pageActiview, pExporter, pExporter.Resolution, true, null);

                SetOutputQuality(pageActiview, pPrevOutputImageQuality);
                if (FontSmoothingReenable)
                {
                    /* reenable font smoothing if we disabled it before */
                    EnableFontSmoothing();
                    FontSmoothingReenable = false;
                    if (!GetFontSmoothing())
                    {
                        //error: cannot reenable font smoothing.
                        err = "Unable to reenable Font Smoothing";
                    }
                }

                //对于TIFF数据设置投影
                if (_outputFileExtension == ".tif")
                {
                    #region
                    string tfwFileName = FileName.Substring(0, FileName.Length - 4) + ".tfw";
                    using (System.IO.FileStream fs = new System.IO.FileStream(tfwFileName, System.IO.FileMode.Create))
                    {
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fs))
                        {
                            double cellSize = 25.4 / iOutputResolution / 1000.0 * mf.Map.ReferenceScale;
                            sw.WriteLine(cellSize);//
                            sw.WriteLine(0);
                            sw.WriteLine(0);
                            sw.WriteLine(-cellSize);
                            var ex = (mf.Map as IActiveView).Extent;
                            sw.WriteLine(ex.XMin);
                            sw.WriteLine(ex.YMax);
                        }
                    }
                    //定义投影
                    string folder = System.IO.Path.GetDirectoryName(FileName);
                    string rasterName = System.IO.Path.GetFileName(FileName);
                    IWorkspaceFactory factory = new ESRI.ArcGIS.DataSourcesRaster.RasterWorkspaceFactoryClass();
                    ESRI.ArcGIS.DataSourcesRaster.IRasterWorkspace prs = factory.OpenFromFile(folder, 0) as ESRI.ArcGIS.DataSourcesRaster.IRasterWorkspace;
                    IRasterDataset pds = prs.OpenRasterDataset(rasterName);

                    IGeoDatasetSchemaEdit pGeoDatasetSchemaEdit = (pds as IGeoDataset) as IGeoDatasetSchemaEdit;
                    if (p_Application.ActiveView.FocusMap.SpatialReference != null)
                    {
                        pGeoDatasetSchemaEdit.AlterSpatialReference(p_Application.ActiveView.FocusMap.SpatialReference);
                    }
                    Marshal.ReleaseComObject(factory);
                    Marshal.ReleaseComObject(pds);
                    //if (!tfwflag)
                    {
                        System.IO.File.Delete(tfwFileName);
                    }
                    #endregion
                }
                #endregion

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                err = ex.Message;
            }

            //最后赋值回去
            if (p_Application.LayoutState == LayoutState.PageLayoutControl)
            {
                p_Application.MapControl.Map = mf.Map;

            }

            return err;

        }
        //ces
        public string ExportMapView(IMap map, string fileName,IEnvelope en)
        {
            string err = "";
             
                
               
                
                IActiveView pageActiview = map as IActiveView;
                tagRECT rect = new tagRECT();
                rect.left = rect.top = 0;
                rect.right = 256;
                rect.bottom = 256;
                System.Drawing.Image image = new System.Drawing.Bitmap(rect.right, rect.bottom, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(image);
                g.FillRectangle(Brushes.Transparent, 0, 0, rect.right, rect.bottom);
                pageActiview.Output(g.GetHdc().ToInt32(), 96*3, ref rect, en, null);
                g.ReleaseHdc();
                image.Save(fileName);
             
            return err;
        }
        private void ExportMap(string FileName, IMap map)
        {
            long iOutputResolution=96;
            long lResampleRatio=1;
            IExport pExporter = null;
            long pPrevOutputImageQuality;
            IOutputRasterSettings pOutputRasterSettings;
            IExportColorspaceSettings pExportColorSpaceSetting;
            IEnvelope PixelBoundsEnv;
            tagRECT exportRECT;
            tagRECT DisplayBounds;
            IDisplayTransformation pDisplayTransformation;
            IPageLayout pPageLayout;
            IEnvelope pMapExtEnv;
            long hdc;
            long tmpDC;
            long iScreenResolution;
            bool FontSmoothingReenable = false;

            IEnvelope pGraphicsExtentEnv;
            IUnitConverter pUnitConvertor;

            if (GetFontSmoothing())
            {
                FontSmoothingReenable = true;
                DisableFontSmoothing();
                if (GetFontSmoothing())
                {
                    return;
                }
            }

           
            {
                pExporter = new ExportJPEGClass();
            }

            IActiveView m_ActiveView = map as IActiveView;
            pOutputRasterSettings = m_ActiveView.ScreenDisplay.DisplayTransformation as IOutputRasterSettings;
            pPrevOutputImageQuality = pOutputRasterSettings.ResampleRatio;

            if (pExporter is IExportImage)
            {
                SetOutputQuality(m_ActiveView, 1);
            }
            else
            {
                SetOutputQuality(m_ActiveView, lResampleRatio);
            }
            pExporter.ExportFileName = FileName;

            /* Get the device context of the screen */
            tmpDC = GetDC(0);
            /* Get the screen resolution. */
            iScreenResolution = GetDeviceCaps((int)tmpDC, 88); //88 is the win32 const for Logical pixels/inch in X)
            //iScreenResolution = 72; //zhouqi, for arcgis, the screen resolution is aways set to 72 points/inch
            /* release the DC. */
            ReleaseDC(0, (int)tmpDC);
            pExporter.Resolution = iOutputResolution;

            if (m_ActiveView is IPageLayout)
            {
                DisplayBounds = m_ActiveView.ExportFrame;
                pGraphicsExtentEnv = GetGraphicsExtent(m_ActiveView);
            }
            else
            {
                pDisplayTransformation = m_ActiveView.ScreenDisplay.DisplayTransformation;
                DisplayBounds = pDisplayTransformation.get_DeviceFrame();
            }

            PixelBoundsEnv = new EnvelopeClass();
            {
                double tempratio = iOutputResolution / iScreenResolution;
                double tempbottom = DisplayBounds.bottom * tempratio;
                double tempright = DisplayBounds.right * tempratio;
                //'The values in the exportRECT tagRECT correspond to the width
                //and height to export, measured in pixels with an origin in the top left corner.
                exportRECT.bottom = (int)Math.Truncate(tempbottom);
                exportRECT.left = 0;
                exportRECT.top = 0;
                exportRECT.right = (int)Math.Truncate(tempright);


                //populate the PixelBounds envelope with the values from exportRECT.
                // We need to do this because the exporter object requires an envelope object
                // instead of a tagRECT structure.
                PixelBoundsEnv.PutCoords(exportRECT.left, exportRECT.top, exportRECT.right, exportRECT.bottom);
                PixelBoundsEnv.PutCoords(exportRECT.left, exportRECT.top,256,256);

                //since it's a page layout or an unclipped page layout we don't need docMapExtEnv.
                pMapExtEnv = null;
            }

            pExporter.PixelBounds = PixelBoundsEnv;
            //IStepProgressor pStepPprogressor ;
            //pStepPprogressor.MinRange = 0;
            //pStepPprogressor.MaxRange = 100;
            //pStepPprogressor.Position = 0;
            //pStepPprogressor.StepValue = 5;
            //pExporter.StepProgressor = pStepPprogressor;
            var docPrintExport = new PrintAndExportClass();
            docPrintExport.Export(m_ActiveView, pExporter, pExporter.Resolution, true, null);

            //hdc = pExporter.StartExporting();
            //m_ActiveView.Output((int)hdc, (int)pExporter.Resolution, ref exportRECT,pMapExtEnv, null);
            //pExporter.FinishExporting();
            //pExporter.Cleanup();            
            //set the output quality back to the previous value
            SetOutputQuality(m_ActiveView, pPrevOutputImageQuality);
            if (FontSmoothingReenable)
            {
                /* reenable font smoothing if we disabled it before */
                EnableFontSmoothing();
                FontSmoothingReenable = false;
                if (!GetFontSmoothing())
                {
                    //error: cannot reenable font smoothing.
                    MessageBox.Show("Unable to reenable Font Smoothing", "Font Smoothing error");
                }
            }

        }

        private IMap CopyAndOverwriteMap(IMap map_)
        {
            //Get IObjectCopy interface
            IObjectCopy objectCopy = new ObjectCopyClass();

            //Get IUnknown interface (map to copy)
            object toCopyMap = map_;

           
            IMap map = toCopyMap as IMap;
            map.IsFramed = false;

            //Get IUnknown interface (copied map)
            object copiedMap = objectCopy.Copy(toCopyMap);

            return copiedMap as IMap;
        }

        //测试：输出Leatlet地图切片
        public string ExportPageLayoutTest(WaitOperation  wo,GApplication p_Application, long iOutputResolution, IPoint leftup,double width,double height)
        {
            
            double ms = p_Application.ActiveView.FocusMap.ReferenceScale;
            IMap copymap = CopyAndOverwriteMap(p_Application.ActiveView.FocusMap);
            MapView frm = new MapView();
            ESRI.ArcGIS.Controls.AxMapControl mapcontrol = frm.mapControl;
         
          
            object toOverwriteMap = mapcontrol.Map;
            IObjectCopy objectCopy = new ObjectCopyClass(); 
          
            objectCopy.Overwrite(copymap, ref toOverwriteMap);
            for (int s = 0; s < 4; s++)
            {
                double mapscale=ms*Math.Pow(2,3-s);
               
                IMapFrame mf = null;
                double dis = mapscale * 0.0254 * 256 / (96 * 3);
                mapcontrol.MapScale = mapscale;
                int msg = 1;
                double column = (int)(Math.Ceiling(mapcontrol.ActiveView.FullExtent.Width / dis));
                double row = (int)(Math.Ceiling(mapcontrol.ActiveView.FullExtent.Height / dis));
                for (int i = 1; i <= row; i++)
                {
                    for (int j = 1; j <= column; j++)
                    {
                        wo.SetText("L"+s.ToString()+msg.ToString());
                        msg++;
                        string fileName = @"D:\图知四川\L"+s.ToString() +"\\"+ i.ToString() + "_" + j.ToString() + ".jpg";
                        //尺寸
                        IPoint centerPoint = new PointClass();

                        centerPoint.X = leftup.X + dis * (j - 0.5);
                        centerPoint.Y = leftup.Y - dis * (i - 0.5);
                        //设置页面
                        //  setPageSizeTest(mapcontrol.Map, centerPoint, width, height);

                        mapcontrol.CenterAt(centerPoint);
                        mapcontrol.Refresh();
                        //ExportMap(fileName, mapcontrol.Map);

                        IEnvelope extent = new EnvelopeClass { XMin = 0, YMin = 0, XMax = dis, YMax = dis };
                        //居中地图     
                        extent.CenterAt(centerPoint);
                        ExportMapView(mapcontrol.Map, fileName, extent);

                    }
                }
            }
           // mapForm.Dispose();
            return "";

        }
        //输出arcgis 格式切片
        public string ExportPageLayoutTile(WaitOperation wo, GApplication p_Application, long iOutputResolution, IPoint leftup, double width, double height)
        {

            double ms = p_Application.ActiveView.FocusMap.ReferenceScale;
            IMap copymap = CopyAndOverwriteMap(p_Application.ActiveView.FocusMap);
            MapView frm = new MapView();
            ESRI.ArcGIS.Controls.AxMapControl mapcontrol = frm.mapControl;


            object toOverwriteMap = mapcontrol.Map;
            IObjectCopy objectCopy = new ObjectCopyClass();

            objectCopy.Overwrite(copymap, ref toOverwriteMap);
            for (int s = 0; s < 4; s++)
            {
                double mapscale = ms * Math.Pow(2, 3 - s);

                IMapFrame mf = null;
                double dis = mapscale * 0.0254 * 256 / (96 * 3);
                mapcontrol.MapScale = mapscale;
                int msg = 1;
                double column = (int)(Math.Ceiling(mapcontrol.ActiveView.FullExtent.Width / dis));
                double row = (int)(Math.Ceiling(mapcontrol.ActiveView.FullExtent.Height / dis));
                for (int i = 1; i <= row; i++)
                {
                    for (int j = 1; j <= column; j++)
                    {
                        wo.SetText("L" + s.ToString() + msg.ToString());
                        msg++;
                        string row0 = (i - 1).ToString("x8").PadLeft(8, '0');
                        string column0 = (j - 1).ToString("x8").PadLeft(8, '0');
                        string folder = @"D:\瓦片\L" + s.ToString();
                        if (!System.IO.Directory.Exists(folder))
                        {
                            System.IO.Directory.CreateDirectory(folder);
                        }
                        if (!System.IO.Directory.Exists(folder + "\\" + row0))
                        {
                            System.IO.Directory.CreateDirectory(folder + "\\" + row0);
                        }
                       //string fileName = @"D:\图知四川1\L" + s.ToString() + "\\" + i.ToString() + "_" + j.ToString() + ".jpg";
                        string fileName = folder + "\\" + row0 + "\\" + column0 + ".jpg";
                        //尺寸
                        IPoint centerPoint = new PointClass();

                        centerPoint.X = leftup.X + dis * (j - 0.5);
                        centerPoint.Y = leftup.Y - dis * (i - 0.5);
                        //设置页面
                        //  setPageSizeTest(mapcontrol.Map, centerPoint, width, height);

                        mapcontrol.CenterAt(centerPoint);
                        mapcontrol.Refresh();
                        //ExportMap(fileName, mapcontrol.Map);

                        IEnvelope extent = new EnvelopeClass { XMin = 0, YMin = 0, XMax = dis, YMax = dis };
                        //居中地图     
                        extent.CenterAt(centerPoint);
                        ExportMapView(mapcontrol.Map, fileName, extent);

                    }
                }
            }
            // mapForm.Dispose();
            return "";

        }
     
        public void setPageSizeTest(IMap map, IPoint center, double pagewidth, double pageheight)
        {
            GApplication app = GApplication.Application;
            double ms = app.Workspace.Map.ReferenceScale;//最新比例尺




            double dis = ms * 0.0254 * 256 / 96;

            double mapwidth = pagewidth * 1e-3 * ms;
            double mapheight = pageheight * 1e-3 * ms;
            IEnvelope extent = new EnvelopeClass { XMin = 0, YMin = 0, XMax = dis, YMax = dis };
            //居中地图     
            IPoint pt = center;
            extent.CenterAt(pt);
            //设置比例尺
            map.MapScale = ms;
            //extent = (map as IActiveView).Extent;
            //extent.CenterAt(pt);
            (map as IActiveView).Extent = extent;
          
            (map as IActiveView).Refresh();
          
        }
        private void SetOutputQuality(IActiveView docActiveView, long iResampleRatio)
        {
            /* This function sets OutputImageQuality for the active view.  If the active view is a pagelayout, then
             * it must also set the output image quality for EACH of the Maps in the pagelayout.
             */
            IGraphicsContainer oiqGraphicsContainer;
            IElement oiqElement;
            IOutputRasterSettings docOutputRasterSettings;
            IMapFrame docMapFrame;
            IActiveView TmpActiveView;

            if (docActiveView is IMap)
            {
                docOutputRasterSettings = docActiveView.ScreenDisplay.DisplayTransformation as IOutputRasterSettings;
                docOutputRasterSettings.ResampleRatio = (int)iResampleRatio;
            }
            else if (docActiveView is IPageLayout)
            {
                //assign ResampleRatio for PageLayout
                docOutputRasterSettings = docActiveView.ScreenDisplay.DisplayTransformation as IOutputRasterSettings;
                docOutputRasterSettings.ResampleRatio = (int)iResampleRatio;
                //and assign ResampleRatio to the Maps in the PageLayout
                oiqGraphicsContainer = docActiveView as IGraphicsContainer;
                oiqGraphicsContainer.Reset();

                oiqElement = oiqGraphicsContainer.Next();
                while (oiqElement != null)
                {
                    if (oiqElement is IMapFrame)
                    {
                        docMapFrame = oiqElement as IMapFrame;
                        TmpActiveView = docMapFrame.Map as IActiveView;
                        docOutputRasterSettings = TmpActiveView.ScreenDisplay.DisplayTransformation as IOutputRasterSettings;
                        docOutputRasterSettings.ResampleRatio = (int)iResampleRatio;
                    }
                    oiqElement = oiqGraphicsContainer.Next();
                }

                docMapFrame = null;
                oiqGraphicsContainer = null;
                TmpActiveView = null;
            }
            // docOutputRasterSettings = null;

        }

        public IEnvelope GetGraphicsExtent(IActiveView docActiveView)
        {
            IEnvelope GraphicsBounds = new EnvelopeClass();
            IEnvelope GraphicsEnvelope = new EnvelopeClass();
            IPageLayout docPageLayout = docActiveView as IPageLayout;
            IDisplay GraphicsDisplay = docActiveView.ScreenDisplay;
            IGraphicsContainer oiqGraphicsContainer = docActiveView as IGraphicsContainer;
            oiqGraphicsContainer.Reset();

            IElement oiqElement = oiqGraphicsContainer.Next();
            while (oiqElement != null)
            {
                oiqElement.QueryBounds(GraphicsDisplay, GraphicsEnvelope);
                GraphicsBounds.Union(GraphicsEnvelope);
                oiqElement = oiqGraphicsContainer.Next();
            }

            return GraphicsBounds;

        }

        private void DisableFontSmoothing()
        {
            bool iResult;
            int pv = 0;

            /* call to systemparametersinfo to set the font smoothing value */
            iResult = SystemParametersInfo(SPI_SETFONTSMOOTHING, 0, ref pv, SPIF_UPDATEINIFILE);
        }

        private void EnableFontSmoothing()
        {
            bool iResult;
            int pv = 0;

            /* call to systemparametersinfo to set the font smoothing value */
            iResult = SystemParametersInfo(SPI_SETFONTSMOOTHING, 1, ref pv, SPIF_UPDATEINIFILE);

        }

        private Boolean GetFontSmoothing()
        {
            bool iResult;
            int pv = 0;

            /* call to systemparametersinfo to get the font smoothing value */
            iResult = SystemParametersInfo(SPI_GETFONTSMOOTHING, 0, ref pv, 0);

            if (pv > 0)
            {
                //pv > 0 means font smoothing is ON.
                return true;
            }
            else
            {
                //pv == 0 means font smoothing is OFF.
                return false;
            }

        }
        #endregion
    }
}
