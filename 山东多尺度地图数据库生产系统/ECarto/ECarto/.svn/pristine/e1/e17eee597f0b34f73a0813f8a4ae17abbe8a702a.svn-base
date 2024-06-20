using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Output;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Output;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using System.IO;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmExportDataMap : Form
    {
        public string m_FileName;
        public long Resolution;
        public esriExportColorspace ColorMode  = esriExportColorspace.esriExportColorspaceCMYK;
        public double PageWidth;
        public double PageHeight;
        public FrmExportDataMap()
        {
            InitializeComponent();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtFileName.Text == "")
                {
                    MessageBox.Show("选择输出文件！");
                    return;
                }
                m_FileName = txtFileName.Text;

                Resolution = (long)this.nudResolution.Value;
                if (rbCMYK.Checked)
                {
                    ColorMode = esriExportColorspace.esriExportColorspaceCMYK;
                }
                else
                {
                    ColorMode = esriExportColorspace.esriExportColorspaceRGB;
                }
                double.TryParse(txtHeight.Text, out PageHeight);
                double.TryParse(txtWidth.Text, out PageWidth);
              
                var envFileName = GApplication.Application.Template.Content.Element("EnvironmentSettings").Value;
           
                XDocument doc = XDocument.Load(GApplication.Application.Template.Root + @"\" + envFileName); 
              
                {
                     
                    var content = doc.Element("Template").Element("Content");

                    //页面尺寸
                    var page = content.Element("PageSize");
                    page.SetElementValue("Width", PageWidth);
                    page.SetElementValue("Height", PageHeight);

                }
                doc.Save(GApplication.Application.Template.Root + @"\" + envFileName);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show("设置错误："+ ex.Message);
            }
        }

        private void FrmExportDataMap_Load(object sender, EventArgs e)
        {
            var paramContent = EnvironmentSettings.getContentElement(GApplication.Application);
            var pagesize = paramContent.Element("PageSize");//页面大小
            double width = double.Parse(pagesize.Element("Width").Value);
            double height = double.Parse(pagesize.Element("Height").Value);

            Dictionary<string, string> envString = GApplication.Application.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
            if (envString == null)
            {
                envString = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
            }
            if (envString.ContainsKey("Width"))
                width = double.Parse(envString["Width"]);
            if (envString.ContainsKey("Height"))
                height = double.Parse(envString["Height"]);

            txtHeight.Text = height.ToString();
            txtWidth.Text = width.ToString();
            this.btView_Click(null, null);                  //默认预览模式 
        }

        private void cmdFileName_Click(object sender, EventArgs e)
        {
            SaveFileDialog pSaveFileDialog = new SaveFileDialog();
            pSaveFileDialog.Title = "输出文件";
            pSaveFileDialog.Filter = "PDF(*.pdf)|*.pdf|JPEG(*.jpg)|*.jpg|TIFF(*.tif)|*.tif|AI(*.ai)|*.ai";
            pSaveFileDialog.FilterIndex = 0;
            if (!string.IsNullOrEmpty(txtFileName.Text))
            {
                pSaveFileDialog.FileName = txtFileName.Text;
            }
            string mapName = "";
            //输出地图名称默认按LANNO地图名称，如没有则按GDB名称去掉ECARTO
            try
            {
                mapName = CommonMethods.GetMapName();
                if (mapName == "")
                {
                    mapName = CommonMethods.GetGdbName();
                }
                pSaveFileDialog.FileName = mapName;
            }
            catch
            {

            }

            if (pSaveFileDialog.ShowDialog() == DialogResult.OK)
            {
               
                txtFileName.Text = pSaveFileDialog.FileName;
                m_FileName = pSaveFileDialog.FileName;

                string fileExt = System.IO.Path.GetExtension(m_FileName).Trim().ToUpper();
                if(fileExt.EndsWith("PDF") || fileExt.EndsWith("AI"))
                {
                    if (!panelColorMode.Visible)
                    {
                        panelColorMode.Visible = true;
                        this.Height += panelColorMode.Height;
                    }
                }
                else
                {
                    if (panelColorMode.Visible)
                    {
                        panelColorMode.Visible = false;
                        this.Height -= panelColorMode.Height;
                    }
                }
            }


        }

        private void btView_Click(object sender, EventArgs e)
        {     
            double.TryParse(txtHeight.Text, out PageHeight);
            double.TryParse(txtWidth.Text, out PageWidth);
            setPageSize(PageWidth,PageHeight);
        }
        public IMapFrame setPageSize( double pagewidth, double pageheight)
        {
            try
            {
                GApplication app = GApplication.Application;
                double ms = app.Workspace.Map.ReferenceScale;//最新比例尺

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


                extent.CenterAt(pt);


                //设置纸张大小
                var gContainer = pg as IGraphicsContainer;
                IPage page = pg.Page;
                var units = page.Units;
                page.Units = ESRI.ArcGIS.esriSystem.esriUnits.esriMillimeters;
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
                if (mf == null)
                {
                    gContainer.Reset();
                    while (true)
                    {
                        el = gContainer.Next();
                        if (el == null)
                            break;

                        if (mf == null)
                            mf = el as IMapFrame;

                        if (el is IMapFrame && (el as IMapFrame).Map.Description == app.PageLayoutControl.ActiveView.FocusMap.Description)
                        {
                            mf = el as IMapFrame;
                            break;
                        }
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
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                throw ex;
            }
            
        }


    }
}
