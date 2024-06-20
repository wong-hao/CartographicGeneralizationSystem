using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.DisplayUI;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Controls;
using SMGI.Common;
using System.IO;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class AddThematicDataForm : Form
    {
        /// <summary>
        /// 输入的要素类
        /// </summary>
        public IFeatureClass InFeatureClass
        {
            get;
            protected set;
        }

        /// <summary>
        /// 图层符号系统
        /// </summary>
        public IFeatureRenderer FeatureRenderer
        {
            get;
            protected set;
        }

        private ISymbologyControl _symbologyControl;

        public AddThematicDataForm()
        {
            InitializeComponent();

            InFeatureClass = null;
            FeatureRenderer = null;

            
        }

        private void AddThematicDataForm_Load(object sender, EventArgs e)
        {
            string styleFile = string.Format("{0}\\专家库\\smgi.style", GApplication.Application.Template.Root);
            if (File.Exists(styleFile))
            {
                _symbologyControl = new SymbologyControlClass();
                _symbologyControl.Clear();
                _symbologyControl.LoadDesktopStyleFile(styleFile);

                //string esriInstall = ESRI.ArcGIS.RuntimeManager.ActiveRuntime.Path;//C:\\Program Files (x86)\\ArcGIS\\Desktop10.1\\
                //string path = System.IO.Path.Combine(esriInstall, "Styles");
            }
        }

        private void btnData_Click(object sender, EventArgs e)
        {
            IGxDialog gxDialog = new GxDialog();
            (gxDialog as IGxObjectFilterCollection).AddFilter(new GxFilterFeatureClassesClass(), false);

            gxDialog.AllowMultiSelect = false;
            gxDialog.RememberLocation = true;
            gxDialog.Title = "添加外部专题数据";

            IEnumGxObject enumObj;
            if (!gxDialog.DoModalOpen(0, out enumObj))
                return;

            if (enumObj == null)
                return;

            tbFeatureClass.Text = "";
            InFeatureClass = null;
            FeatureRenderer = null;
            btnSymbol.Image = null;

            enumObj.Reset();
            IGxObject gxObj = null;
            while ((gxObj = enumObj.Next()) != null)
            {
                if (gxObj is IGxDataset)
                {
                    IGxDataset gxDataset = gxObj as IGxDataset;
                    IDataset dataset = gxDataset.Dataset;
                    if (dataset.Type == esriDatasetType.esriDTFeatureClass && (dataset as IFeatureClass).FeatureType != esriFeatureType.esriFTAnnotation)
                    {
                        InFeatureClass = dataset as IFeatureClass;
                        tbFeatureClass.Text = gxObj.FullName;

                        Random rand = new Random();

                        ISymbol sym = null;
                        if ((dataset as IFeatureClass).ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint)
                        {
                            FeatureRenderer = new SimpleRendererClass();

                            sym = new SimpleMarkerSymbolClass();
                            (sym as ISimpleMarkerSymbol).Color = new RgbColorClass()
                            {
                                Red = rand.Next(255),
                                Green = rand.Next(255),
                                Blue = rand.Next(255)
                            };
                            (FeatureRenderer as ISimpleRenderer).Symbol = sym;

                            btnSymbol.Image = PreViewMarkerSymbol((sym as IClone).Clone() as ISymbol, btnSymbol.Height, btnSymbol.Width);
                        }
                        else if ((dataset as IFeatureClass).ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline)
                        {
                            FeatureRenderer = new SimpleRendererClass();

                            sym = new SimpleLineSymbolClass();
                            (sym as ISimpleLineSymbol).Color = new RgbColorClass()
                            {
                                Red = rand.Next(255),
                                Green = rand.Next(255),
                                Blue = rand.Next(255)
                            };
                            (FeatureRenderer as ISimpleRenderer).Symbol = sym;

                            btnSymbol.Image = PreViewLineSymbol((sym as IClone).Clone() as ISymbol, btnSymbol.Height, btnSymbol.Width);
                        }
                        else if ((dataset as IFeatureClass).ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon)
                        {
                            FeatureRenderer = new SimpleRendererClass();

                            sym = new SimpleFillSymbolClass();
                            (sym as ISimpleFillSymbol).Color = new RgbColorClass()
                            {
                                Red = rand.Next(255),
                                Green = rand.Next(255),
                                Blue = rand.Next(255)
                            };
                            (FeatureRenderer as ISimpleRenderer).Symbol = sym;

                            btnSymbol.Image = PreViewFillSymbol((sym as IClone).Clone() as ISymbol, btnSymbol.Height, btnSymbol.Width);
                        }
                    }
                }
            }
        }

        private void btnSymbol_Click(object sender, EventArgs e)
        {
            if(FeatureRenderer is ISimpleRenderer)
            {
                ISymbolSelector symbolSelector = new SymbolSelectorClass();
                symbolSelector.AddSymbol((FeatureRenderer as ISimpleRenderer).Symbol);
                symbolSelector.SelectSymbol(0);
                ISymbol sym = symbolSelector.GetSymbolAt(0);

                (FeatureRenderer as ISimpleRenderer).Symbol = sym;

                if (InFeatureClass != null)
                {
                    btnSymbol.Image = null;
                    if (InFeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint)
                    {
                        btnSymbol.Image = PreViewMarkerSymbol((sym as IClone).Clone() as ISymbol, btnSymbol.Height, btnSymbol.Width);
                    }
                    else if (InFeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline)
                    {
                        btnSymbol.Image = PreViewLineSymbol((sym as IClone).Clone() as ISymbol, btnSymbol.Height, btnSymbol.Width);
                    }
                    else if (InFeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon)
                    {
                        btnSymbol.Image = PreViewFillSymbol((sym as IClone).Clone() as ISymbol, btnSymbol.Height, btnSymbol.Width);
                    }
                }

            }
            
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (InFeatureClass == null)
            {
                MessageBox.Show("请输入一个外部数据！");
                return;
            }

            if (FeatureRenderer == null)
            {
                MessageBox.Show("请指定要素符号！");
                return;
            }

            DialogResult = DialogResult.OK;
        }


        private Image PreViewMarkerSymbol(ISymbol symbol, int height, int width)
        {
            IStyleGalleryClass styleGalleryClass = new MarkerSymbolStyleGalleryClass();
            Image img = new Bitmap(width, height);
            Graphics gc = Graphics.FromImage(img);
            IntPtr hdc = gc.GetHdc();
            tagRECT rect = new tagRECT();
            rect.left = 0;
            rect.top = 0;
            rect.right = width;
            rect.bottom = height;
            styleGalleryClass.Preview(symbol, hdc.ToInt32(), ref rect);
            gc.ReleaseHdc(hdc);
            gc.Dispose();

            return img;
        }

        private Image PreViewLineSymbol(ISymbol symbol, int height, int width)
        {
            IStyleGalleryClass styleGalleryClass = new LineSymbolStyleGalleryClass();
            Image img = new Bitmap(width, height);
            Graphics gc = Graphics.FromImage(img);
            IntPtr hdc = gc.GetHdc();
            tagRECT rect = new tagRECT();
            rect.left = 0;
            rect.top = 0;
            rect.right = width;
            rect.bottom = height;
            styleGalleryClass.Preview(symbol, hdc.ToInt32(), ref rect);
            gc.ReleaseHdc(hdc);
            gc.Dispose();

            return img;
        }

        private Image PreViewFillSymbol(ISymbol symbol, int height, int width)
        {
            IStyleGalleryClass styleGalleryClass = new FillSymbolStyleGalleryClass();
            Image img = new Bitmap(width, height);
            Graphics gc = Graphics.FromImage(img);
            IntPtr hdc = gc.GetHdc();
            tagRECT rect = new tagRECT();
            rect.left = 0;
            rect.top = 0;
            rect.right = width;
            rect.bottom = height;
            styleGalleryClass.Preview(symbol, hdc.ToInt32(), ref rect);
            gc.ReleaseHdc(hdc);
            gc.Dispose();

            return img;
        } 
        
    }
}
