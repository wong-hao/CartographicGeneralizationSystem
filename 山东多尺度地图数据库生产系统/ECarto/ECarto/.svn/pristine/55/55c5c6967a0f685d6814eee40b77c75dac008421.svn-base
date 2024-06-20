using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.SystemUI;
using stdole;

namespace SMGI.Common
{


    public partial class SymbolForm : Form
    {
        private const int LOGPIXELSY = 90;
        private const int COLORONCOLOR = 3;
        private const int HORZSIZE = 4;
        private const int VERTSIZE = 6;
        private const int HORZRES = 8;
        private const int VERTRES = 10;
        private const int ASPECTX = 40;
        private const int ASPECTY = 42;
        private const int LOGPIXELSX = 88;

        private enum PictureTypeConstants
        {
            picTypeNone = 0,
            picTypeBitmap = 1,
            picTypeMetafile = 2,
            picTypeIcon = 3,
            picTypeEMetafile = 4
        }
        private struct PICTDESC
        {
            public int cbSizeOfStruct;
            public int picType;
            public IntPtr hPic;
            public IntPtr hpal;
            public int _pad;
        }
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("olepro32.dll", EntryPoint = "OleCreatePictureIndirect")]
        private static extern int OleCreatePictureIndirect(
            ref PICTDESC pPictDesc, ref Guid riid, int fOwn, [MarshalAs(UnmanagedType.IDispatch)] ref object ppvObj);


        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll", EntryPoint = "DeleteDC", ExactSpelling = true, SetLastError = true)]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", EntryPoint = "SelectObject", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject", ExactSpelling = true, SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap", ExactSpelling = true,
            SetLastError = true)]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hObject, int width, int height);

        [DllImport("user32.dll", EntryPoint = "GetDC", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr ptr);

        [DllImport("user32.dll", EntryPoint = "ReleaseDC", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("gdi32", EntryPoint = "CreateSolidBrush", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr CreateSolidBrush(int crColor);

        [DllImport("user32", EntryPoint = "FillRect", ExactSpelling = true, SetLastError = true)]
        private static extern int FillRect(IntPtr hdc, ref RECT lpRect, IntPtr hBrush);

        [DllImport("GDI32.dll", EntryPoint = "GetDeviceCaps", ExactSpelling = true, SetLastError = true)]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32", EntryPoint = "GetClientRect", ExactSpelling = true, SetLastError = true)]
        private static extern int GetClientRect(IntPtr hwnd, ref RECT lpRect);







        string stylePath = "";
        public SymbolForm(string spath)
        {
            InitializeComponent();
            stylePath = spath;
            Init();
        }
        public void Init()
        {
            try
            {
                this.axSymbologyControl1.LoadStyleFile(stylePath);
            }
            catch
            {
                MessageBox.Show("符号库加载出现异常，请检查符号库路径!");
            }
        }
        public void AddSymbol(ESRI.ArcGIS.Controls.esriSymbologyStyleClass styleClass, ISymbol sym)
        {
            m_styleGalleryItem = null;
            if (sym != null)
            {
                this.axSymbologyControl1.StyleClass = styleClass;

                ISymbologyStyleClass symbologyStyleClass = axSymbologyControl1.GetStyleClass(styleClass);
                IStyleGalleryItem styleGalleryItem = new ServerStyleGalleryItem();
                styleGalleryItem.Item = sym;
                styleGalleryItem.Name = "CurrentSymbol";

                //symbologyStyleClass.AddItem(styleGalleryItem, 0);
                //symbologyStyleClass.SelectItem(0);
                this.ShowDialog();
            }

        }
        public IStyleGalleryItem m_styleGalleryItem = null;


        private void axSymbologyControl1_OnItemSelected(object sender, ESRI.ArcGIS.Controls.ISymbologyControlEvents_OnItemSelectedEvent e)
        {
            m_styleGalleryItem = (IStyleGalleryItem)e.styleGalleryItem;
            ISymbologyStyleClass symbologyStyleClass = axSymbologyControl1.GetStyleClass(
                esriSymbologyStyleClass.esriStyleClassMarkerSymbols);
                
            //Preview an image of the symbol
            
            stdole.IPictureDisp picture = symbologyStyleClass.PreviewItem(m_styleGalleryItem, 100, 40);
            System.Drawing.Image image = System.Drawing.Image.FromHbitmap(new System.IntPtr(picture.Handle));


            DataGridViewRow dr = new DataGridViewRow();

            Graphics g = this.CreateGraphics();

            //Image img = SymbolToBitmap(m_styleGalleryItem.Item as ISymbol,
            //    new System.Drawing.Size(100, 40),
            //    g,
            //    ColorTranslator.ToWin32(Color.White));
            Dictionary<ISymbol, IGeometry> syms = new Dictionary<ISymbol, IGeometry>();

            syms.Add(m_styleGalleryItem.Item as ISymbol, null);
            Image img = ConvertSymbols2Image(syms, 100, 40, 5);

            int i = dataGridView1.Rows.Add(new object[3] { img, null, null });
            dataGridView1.Rows[i].Height = 50;
        }

        public static Image ConvertSymbols2Image(Dictionary<ISymbol, IGeometry> symInfo, int w, int h, int lGap, double scaleRatio = 1)
        {
            Bitmap bitmap = new Bitmap(w, h);
            try
            {
                Graphics g = Graphics.FromImage(bitmap);
                IntPtr hdc = g.GetHdc();
                if (!Clear(hdc, ColorTranslator.ToWin32(Color.Transparent), 0, 0, w, h))
                {
                    throw new Exception("Could not clear the Device Context.");
                }
                ITransformation pTransformation = CreateTransFromDC(
                       hdc, w, h, scaleRatio);
                foreach (ISymbol sym in symInfo.Keys)
                {
                    if (sym != null)
                    {
                        IGeometry geo = symInfo[sym];
                        if (geo == null || geo.IsEmpty)
                        {
                            IEnvelope pEnvelope = new EnvelopeClass();
                            pEnvelope.PutCoords(lGap, lGap, w - lGap, h - lGap);
                            geo = CreateSymShape(sym, pEnvelope);
                        }
                        sym.SetupDC(hdc.ToInt32(), pTransformation);
                        sym.Draw(geo);
                        sym.ResetDC();
                    }
                }

                g.ReleaseHdc(hdc);
                g.Dispose();
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return bitmap as Image;
        }

        public static Image ImageFromSymbol(ISymbol symbol, Int32 width, Int32 height, int gap, double scaleratio = 1)
        {
            try
            {
                Bitmap bitmap = new Bitmap(width, height);
                //bitmap.SetResolution(150, 150);
                Graphics g = Graphics.FromImage(bitmap);

                IntPtr hdc = g.GetHdc();
                #region 创建绘制点、线、面
                IGeometry geometry = null;
                if (symbol is IMarkerSymbol)
                {
                    IPoint point = new PointClass();
                    point.X = (width / 2);
                    point.Y = (height / 2);
                    geometry = point;
                }
                else if (symbol is ILineSymbol)
                {
                    IPolyline line = new PolylineClass();
                    IPoint ptFrom = new PointClass();
                    IPoint ptTo = new PointClass();
                    ptFrom.X = gap; ptFrom.Y = (height / 2);
                    ptTo.X = (width - gap); ptTo.Y = ptFrom.Y;
                    line.FromPoint = ptFrom;
                    line.ToPoint = ptTo;
                    geometry = line;
                }
                else
                {
                    IEnvelope bounds = new EnvelopeClass();
                    bounds.XMin = gap; bounds.XMax = width - gap;
                    bounds.YMin = gap; bounds.YMax = height - gap;
                    geometry = bounds;
                }
                #endregion
                // First clear the existing device context.[如果不加这句话，绘制出的图片效果不好，xq.cheng]
                if (!Clear(hdc, ColorTranslator.ToWin32(Color.Transparent), 0, 0, width, height))
                {
                    throw new Exception("Could not clear the Device Context.");
                }

                ITransformation pTransformation = CreateTransFromDC(
                       hdc, width, height, scaleratio);
                symbol.SetupDC(hdc.ToInt32(), pTransformation);
                symbol.Draw(geometry);
                symbol.ResetDC();

                g.ReleaseHdc(hdc);
                g.Dispose();

                return bitmap as Image;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return null;
            }
        }


        public static Bitmap SymbolToBitmap(ISymbol userSymbol, Size size, Graphics gr, int backColor)
        {
            IntPtr graphicsHdc = gr.GetHdc();
            IntPtr hBitmap = IntPtr.Zero;
            CreatePictureFromSymbol(
                graphicsHdc, ref hBitmap, userSymbol, size, 1, backColor);

            //Bitmap newBitmap = Bitmap.FromHbitmap(hBitmap);
            Bitmap newBitmap = Bitmap.FromHbitmap(hBitmap);

            gr.ReleaseHdc(graphicsHdc);

            return newBitmap;
        }
        private static IPictureDisp CreatePictureFromSymbol(IntPtr hDCOld, ref IntPtr hBmpNew,
                ISymbol pSymbol, Size size, int lGap, int backColor)
        {
            IntPtr hDCNew = IntPtr.Zero;
            IntPtr hBmpOld = IntPtr.Zero;
            try
            {
                //Create memory Hdc compatible with assigned hdc
                hDCNew = CreateCompatibleDC(hDCOld);
                //Create a Bitmap
                hBmpNew = CreateCompatibleBitmap(hDCOld, size.Width, size.Height);

                hBmpOld = SelectObject(hDCNew, hBmpNew);

                // Draw the symbol to the new device context.
                bool lResult = DrawToDC(hDCNew, size, pSymbol, lGap, backColor);

                hBmpNew = SelectObject(hDCNew, hBmpOld);

                DeleteDC(hDCNew);

                // Return the Bitmap as an OLE Picture.
                return null;//CreatePictureFromBitmap(hBmpNew);
            }
            catch (Exception error)
            {
                if (pSymbol != null)
                {
                    pSymbol.ResetDC();
                    if ((hBmpNew != IntPtr.Zero) && (hDCNew != IntPtr.Zero) && (hBmpOld != IntPtr.Zero))
                    {
                        hBmpNew = SelectObject(hDCNew, hBmpOld);
                        DeleteDC(hDCNew);
                    }
                }

                throw error;
            }
        }
        private static IPictureDisp CreatePictureFromBitmap(IntPtr hBmpNew)
        {
            try
            {
                Guid iidIPicture = new Guid("7BF80980-BF32-101A-8BBB-00AA00300CAB");

                PICTDESC picDesc = new PICTDESC();
                picDesc.cbSizeOfStruct = Marshal.SizeOf(picDesc);
                picDesc.picType = (int)PictureTypeConstants.picTypeBitmap;
                picDesc.hPic = (IntPtr)hBmpNew;
                picDesc.hpal = IntPtr.Zero;

                // Create Picture object.
                IPictureDisp newPic;
                //OleCreatePictureIndirect(ref picDesc, ref iidIPicture, true, out newPic);
                Object objNewPic = null;
                OleCreatePictureIndirect(ref picDesc, ref iidIPicture, 1, ref objNewPic);
                newPic = (IPictureDisp)objNewPic;

                // Return the new Picture object.
                return newPic;
            }
            catch (Exception error)
            {
                throw error;
            }
        }
        private static IGeometry CreateSymShape(ISymbol pSymbol, IEnvelope pEnvelope)
        {
            // This function returns an appropriate Geometry type depending on the
            // Symbol type passed in.
            try
            {
                if (pSymbol is IMarkerSymbol)
                {
                    // For a MarkerSymbol return a Point.
                    IArea pArea = (IArea)pEnvelope;
                    return pArea.Centroid;
                }
                else if ((pSymbol is ILineSymbol) || (pSymbol is ITextSymbol))
                {
                    // For a LineSymbol or TextSymbol return a Polyline.
                    IPolyline pPolyline = new PolylineClass();
                    pPolyline.FromPoint = pEnvelope.LowerLeft;
                    pPolyline.ToPoint = pEnvelope.UpperRight;
                    return pPolyline;
                }
                else
                {
                    // For any FillSymbol return an Envelope.
                    return pEnvelope;
                }
            }
            catch
            {
                return null;
            }
        }
        private static bool DrawToDC(IntPtr hDC, Size size, ISymbol pSymbol, int lGap, int backColor)
        {
            try
            {
                if (hDC != IntPtr.Zero)
                {
                    // First clear the existing device context.
                    if (!Clear(hDC, backColor, 0, 0, size.Width, size.Height))
                    {
                        throw new Exception("Could not clear the Device Context.");
                    }

                    // Create the Transformation and Geometry required by ISymbol::Draw.
                    ITransformation pTransformation = CreateTransFromDC(hDC, size.Width, size.Height);
                    IEnvelope pEnvelope = new EnvelopeClass();
                    pEnvelope.PutCoords(lGap, lGap, size.Width - lGap, size.Height - lGap);
                    IGeometry pGeom = CreateSymShape(pSymbol, pEnvelope);

                    // Perform the Draw operation.
                    if ((pTransformation != null) && (pGeom != null))
                    {
                        pSymbol.SetupDC(hDC.ToInt32(), pTransformation);
                        pSymbol.Draw(pGeom);
                        pSymbol.ResetDC();
                    }
                    else
                    {
                        throw new Exception("Could not create required Transformation or Geometry.");
                    }
                }
            }
            catch
            {
                if (pSymbol != null)
                {
                    pSymbol.ResetDC();
                }
                return false;
            }

            return true;
        }
        private static bool Clear(IntPtr hDC, int backgroundColor, int xmin, int ymin, int xmax, int ymax)
        {
            // This function fill the passed in device context with a solid brush,
            // based on the OLE color passed in.
            IntPtr hBrushBackground = IntPtr.Zero;
            int lResult;
            bool ok;

            try
            {
                RECT udtBounds;
                udtBounds.Left = xmin;
                udtBounds.Top = ymin;
                udtBounds.Right = xmax;
                udtBounds.Bottom = ymax;

                hBrushBackground = CreateSolidBrush(backgroundColor);
                if (hBrushBackground == IntPtr.Zero)
                {
                    throw new Exception("Could not create GDI Brush.");
                }
                lResult = FillRect(hDC, ref udtBounds, hBrushBackground);
                if (hBrushBackground == IntPtr.Zero)
                {
                    throw new Exception("Could not fill Device Context.");
                }
                ok = DeleteObject(hBrushBackground);
                if (hBrushBackground == IntPtr.Zero)
                {
                    throw new Exception("Could not delete GDI Brush.");
                }
            }
            catch
            {
                if (hBrushBackground != IntPtr.Zero)
                {
                    ok = DeleteObject(hBrushBackground);
                }
                return false;
            }

            return true;
        }

        private static ITransformation CreateTransFromDC(IntPtr hDC,
                   int lWidth, int lHeight, double ratio = 1)
        {
            // Calculate the parameters for the new transformation,
            // based on the dimensions passed to this function.
            try
            {
                IEnvelope pBoundsEnvelope = new EnvelopeClass();
                pBoundsEnvelope.PutCoords(0.0, 0.0, (double)lWidth,
                    (double)lHeight);

                tagRECT deviceRect;
                deviceRect.left = 0;
                deviceRect.top = 0;
                deviceRect.right = lWidth;
                deviceRect.bottom = lHeight;

                int dpi = GetDeviceCaps(hDC, LOGPIXELSY);
                if (dpi == 0)
                {
                    throw new Exception(
                      "Could not retrieve Resolution from device context.");
                }

                // Create a new display transformation and set its properties
                IDisplayTransformation newTrans =
                    new DisplayTransformationClass();
                newTrans.VisibleBounds = pBoundsEnvelope;
                newTrans.Bounds = pBoundsEnvelope;
                newTrans.set_DeviceFrame(ref deviceRect);
                newTrans.Resolution = dpi * ratio;

                return newTrans;
            }
            catch
            {
                return null;
            }
        }

        private void axSymbologyControl1_OnDoubleClick(object sender, ISymbologyControlEvents_OnDoubleClickEvent e)
        {
            m_styleGalleryItem = this.axSymbologyControl1.HitTest(e.x, e.y);
            if (m_styleGalleryItem != null)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Hide();
            }
        }

    }
}
