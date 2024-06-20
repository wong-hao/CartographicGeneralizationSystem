using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.IO;
using ESRI.ArcGIS.Display;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmMapLine : Form
    {
        GApplication app;
        double mapScale;
        double maplineOutWidth = 0;
        double maplineInWidth = 0;
        public FrmMapLine()
        {
            InitializeComponent();
            app = GApplication.Application;
        }


        private void btOK_Click(object sender, EventArgs e)
        {
            GApplication.Application.EngineEditor.StartOperation();
            execute();
            GApplication.Application.EngineEditor.StopOperation("图廓线定位");
            SaveParams();
            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == "LPOLY").FirstOrDefault();
            if (lyr != null)
                lyr.Visible = true;//恢复LPOLY图层的可见性
            MapLineInfo.OutUpWidth = outUpWidth.Text;
            MapLineInfo.OutDownWidth = outDownWidth.Text;
            MapLineInfo.OutRightWidth = outRightWidth.Text;
            MapLineInfo.OutLeftWidth = outLeftWidth.Text;
            MapLineInfo.InWidth = inWidth.Text;
            this.Close();
        }

        //加载参数
        private void LoadOutParams(string fileName)
        {
            //string planStr = @"专家库\经验方案\经验方案.xml";
            //string fileName = app.Template.Root + @"\" + planStr;
           
            XDocument doc;
            
            {
                doc = XDocument.Load(fileName);
                var content = doc.Element("Template").Element("Content");
                var mapline = content.Element("MapLine");
                var item = mapline.Element("Top");
                outUpWidth.Text = item.Value;
                item = mapline.Element("Bottom");
                outDownWidth.Text = item.Value;
                item = mapline.Element("Right");
                outRightWidth.Text = item.Value;
                item = mapline.Element("Left");
                outLeftWidth.Text = item.Value;
                item = mapline.Element("Interval");
                inWidth.Text = item.Value;
            }
        }
        private void execute()
        {
            var wo = app.SetBusy();
            string GDBname = "LLINE";
            MapLayoutHelperLH mh = new MapLayoutHelperLH(GDBname);
            mh.CreateMapBorderLine(outUpWidth.Text, outDownWidth.Text, outLeftWidth.Text, outRightWidth.Text, inWidth.Text, cbClip.Checked);
            OverrideMapLineWidth();
            wo.Dispose();
        }
        private void SaveParams()
        {
            XElement root = new XElement("MapLine");
            var item = new XElement("Top");
            item.Value =outUpWidth.Text;
            root.Add(item);
            item = new XElement("Bottom");
            item.Value = outDownWidth.Text;
            root.Add(item);
            item = new XElement("Right");
            item.Value = outRightWidth.Text;
            root.Add(item);
            item = new XElement("Left");
            item.Value = outLeftWidth.Text;
            root.Add(item);
            item = new XElement("Interval");
            item.Value = inWidth.Text;
            root.Add(item);
            ExpertiseParamsClass.UpdateMapLine(GApplication.Application, root);
        
        }
        private void FrmMapLine_Load(object sender, EventArgs e)
        {
            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == "LPOLY").FirstOrDefault();
            if (lyr != null)
                lyr.Visible = false;//设置LPOLY图层为不可见，便于图廓线定位            
            string fileName = app.Template.Root + @"\专家库\MapMargin.xml";
           // LoadOutParams(fileName);
            var maplineItem = GetNearestSize();
            if (maplineItem != null)
            {
                outUpWidth.Text = maplineItem["外图廓距纸张上边缘"].ToString();   
                outDownWidth.Text = maplineItem["外图廓距纸张下边缘"].ToString();
                outRightWidth.Text = maplineItem["外图廓距纸张右边缘"].ToString();  
                outLeftWidth.Text = maplineItem["外图廓距纸张左边缘"].ToString();
                inWidth.Text = maplineItem["内外图廓间距"].ToString();
                maplineOutWidth =double.Parse(maplineItem["外图廓线宽"].ToString());
                maplineInWidth = double.Parse(maplineItem["内图廓线宽"].ToString());

            }
            if (MapLineInfo.InWidth != "")
            {
                outUpWidth.Text = MapLineInfo.OutUpWidth;
                outDownWidth.Text = MapLineInfo.OutDownWidth;
                outLeftWidth.Text = MapLineInfo.OutLeftWidth;
                outRightWidth.Text = MapLineInfo.OutRightWidth;
                inWidth.Text = MapLineInfo.InWidth;
                return;
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void outLeftWidth_TextChanged(object sender, EventArgs e)
        {

        }

        private void btn_apply_Click(object sender, EventArgs e)
        {
            execute();
            SaveParams();
        }

        //最近参数
        private void btLast_Click(object sender, EventArgs e)
        {
            string planStr = @"专家库\经验方案\经验方案.xml";
            string fileName = app.Template.Root + @"\" + planStr;
            LoadOutParams(fileName);
        }
        //获取最近的【纸张开本】
        IPolygon pageGeo = null;
        DataTable ruleDatatable = null;
        private DataRow GetNearestSize()
        {
            DataRow itemSel = null;
            try
            {
                GApplication m_Application = GApplication.Application;
                IWorkspace2 pws2 = m_Application.Workspace.EsriWorkspace as IWorkspace2;
                IFeatureWorkspace fws = pws2 as IFeatureWorkspace;
                if (pageGeo == null)
                {
                    if (pws2.get_NameExists(esriDatasetType.esriDTFeatureClass, "ClipBoundary"))
                    {
                        #region
                        IFeatureClass clipfcl = fws.OpenFeatureClass("ClipBoundary");
                        if (clipfcl.FeatureCount(null) != 0)
                        {
                            IFeature fe = null;
                            IQueryFilter qf = new QueryFilterClass();
                            qf.WhereClause = "TYPE = '页面'";
                            IFeatureCursor cursor = clipfcl.Search(qf, false);
                            fe = cursor.NextFeature();
                            pageGeo = fe.ShapeCopy as IPolygon;
                            Marshal.ReleaseComObject(cursor);
                        }
                        #endregion
                    }
                }
                double ms = m_Application.ActiveView.FocusMap.ReferenceScale;
                double width = pageGeo.Envelope.Width / ms * 1000; //毫米
                double height = pageGeo.Envelope.Height / ms * 1000;//毫米

                double min = Math.Min(width, height);
                double max = Math.Max(width, height);
                string filemdb = m_Application.Template.Root + @"\专家库\纸张整饰参数\整饰参数经验值.mdb";
                if (ruleDatatable == null)
                {
                 
                    ruleDatatable = CommonMethods.ReadToDataTable(filemdb, "纸张整饰参数经验值");  
                }

               
                for(int i=0;i<ruleDatatable.Rows.Count; i++)
                {
                    #region 考虑两边
                    DataRow row = ruleDatatable.Rows[i];
                    double pagewidth = double.Parse(row["纸张宽"].ToString());
                    double pageheight = double.Parse(row["纸张高"].ToString());
                    double pagemin = Math.Min(pagewidth, pageheight);
                    double pagemax = Math.Max(pagewidth, pageheight);
                    if ((Math.Abs(pagemin - min) / min) < 0.2 && Math.Abs(pagemax - max) / max < 0.2)
                    {
                        itemSel = row;
                        break;
                    }
                    #endregion
                }
                if (itemSel == null)
                {
                    #region 考虑单边
                    for (int i = 0; i < ruleDatatable.Rows.Count; i++)
                    {
                        DataRow row = ruleDatatable.Rows[i];
                        double pagewidth = double.Parse(row["纸张宽"].ToString());
                        double pageheight = double.Parse(row["纸张高"].ToString());
                        double pagemin = Math.Min(pagewidth, pageheight);
                        double pagemax = Math.Max(pagewidth, pageheight);
                        if ((Math.Abs(pagemin - min) / min) < 0.2 || Math.Abs(pagemin - max) / max < 0.2)
                        {
                            itemSel = row;
                            break;
                        }
                        if ((Math.Abs(pagemax - min) / min) < 0.2 || Math.Abs(pagemax - max) / max < 0.2)
                        {
                            itemSel = row;
                            break;
                        }
                    }
                    #endregion
                }
               
                return itemSel;
            }
            catch(Exception ex)
            {
                return null;
            }

        }
        private DataTable LoadDataFromExcel(string filePath)
        {
            try
            {
                string strConn;
               strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties='Excel 8.0;HDR=YES;IMEX=1'";

               //strConn = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + filePath + ";" + "Extended Properties=Excel 8.0;";
               using (System.Data.OleDb.OleDbConnection OleConn = new System.Data.OleDb.OleDbConnection(strConn))
               {
                   OleConn.Open();

                   String sql = "SELECT * FROM  [Sheet1$]";
                   System.Data.OleDb.OleDbDataAdapter OleDaExcel = new System.Data.OleDb.OleDbDataAdapter(sql, OleConn);
                   DataSet OleDsExcle = new DataSet();
                   OleDaExcel.Fill(OleDsExcle, "Sheet1");
                   OleConn.Close();
                   return OleDsExcle.Tables[0];
               }
            }
            catch (Exception err)
            {
                MessageBox.Show("加载Excel失败!失败原因：" + err.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
        }
        public void OverrideMapLineWidth()
        {
            if (maplineInWidth == 0 || maplineOutWidth == 0)
                return;
            GApplication m_Application=GApplication.Application;
            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == "LLINE";
            })).ToArray();
            IGeoFeatureLayer geolyr = lyrs[0] as IGeoFeatureLayer;
            IRepresentationRenderer repRender = geolyr.Renderer as IRepresentationRenderer;
            IRepresentationClass repClass = repRender.RepresentationClass;
           
            IFeature fe = null;
            IFeatureCursor cursor=geolyr.FeatureClass.Search(null,false);
            ESRI.ArcGIS.Display.IMapContext mapContext = new ESRI.ArcGIS.Display.MapContextClass();
            mapContext.Init(m_Application.ActiveView.FocusMap.SpatialReference, m_Application.ActiveView.FocusMap.ReferenceScale, geolyr.AreaOfInterest);
            while ((fe = cursor.NextFeature()) != null)
            {
                string type = fe.get_Value(fe.Fields.FindField("TYPE")).ToString();
                if (!type.Contains("图廓"))
                    continue;
                double width = type == "外图廓" ? maplineOutWidth : maplineInWidth;
                IRepresentation repFeature = repClass.GetRepresentation(fe, mapContext);

                var ruleOrg = repFeature.RepresentationClass.RepresentationRules.get_Rule(repFeature.RuleID);

                IBasicLineSymbol basicLineSymbol = ruleOrg.get_Layer(0) as IBasicLineSymbol;
                IGraphicAttributes lineAttributes = basicLineSymbol.Stroke as IGraphicAttributes;
                int id = lineAttributes.get_IDByName("Width");
                repFeature.set_Value(lineAttributes, id, width*2.83);//设置宽度
                repFeature.RepresentationClass.RepresentationRules.set_Rule(repFeature.RuleID, ruleOrg);
                repFeature.UpdateFeature();
                repFeature.Feature.Store();
               
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
        }
  
    }

    public static class MapLineInfo
    {
        public static string OutUpWidth = "";
        public static string OutDownWidth = "";
        public static string OutLeftWidth = "";
        public static string OutRightWidth = "";
        public static string InWidth = "";
    }
}
