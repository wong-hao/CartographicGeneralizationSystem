using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.Xml.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.InteropServices;
using  ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Controls;
namespace SMGI.Plugin.EmergencyMap
{
    [SMGIAutomaticCommand]
    public class FrmMapLineCmd: SMGICommand
    {
        public FrmMapLineCmd()
        {
            m_caption = "图廓线生成";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }
        FrmMapLine frm = null;
        public override void OnClick()
        {
            if ( frm == null||frm.IsDisposed)
            {
                frm = new FrmMapLine();
                frm.StartPosition = FormStartPosition.CenterScreen;
                frm.Show();
            }
            else
            {
                frm.Activate();
                frm.WindowState = FormWindowState.Normal;
            }
        }

        protected override bool DoCommand(System.Xml.Linq.XElement args, Action<string> messageRaisedAction)
        {
            if (m_Application.Workspace == null)
                return false;
            messageRaisedAction("图廓线生成....");
            string GDBname = "LLINE";
            MapLayoutHelperLH mh = new MapLayoutHelperLH(GDBname);
            double  outUpWidth = 65;
            double  outDownWidth = 35;
            double  outRightWidth = 30;
            double  outLeftWidth = 30;
            double  inWidth =15;
            //计算是否与色带面相交
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "ClipBoundary");
            })).FirstOrDefault() as IFeatureLayer;
            IFeature f;
            IFeatureCursor cursor = lyrs.FeatureClass.Search(new QueryFilterClass { WhereClause = "TYPE = '裁切面'" }, false);
            f = cursor.NextFeature();
            if (f == null)
            {
                MessageBox.Show("不存在裁切面要素！");
                return false;
            }
            var clipGeo = f.Shape.Envelope;

            double mapScale=m_Application.ActiveView.FocusMap.ReferenceScale;
            bool inserect=true;
            do{
            //内图廓
                double widthup = Convert.ToDouble(outUpWidth);//
                widthup = widthup / 1000 * mapScale;
                double widthdown = Convert.ToDouble(outDownWidth);//
                widthdown = widthdown / 1000 * mapScale;
                double widthleft = Convert.ToDouble(outLeftWidth);//
                widthleft = widthleft / 1000 * mapScale;
                double widthright = Convert.ToDouble(outRightWidth);//
                widthright = widthright / 1000 * mapScale;
                double interval=(inWidth+5+2.5) / 1000 * mapScale;//5毫米色带+2.5毫米间隔
                double xmin =mh.pageGeo.Envelope.XMin + widthleft+interval;
                double xmax =mh.pageGeo.Envelope.XMax - widthright-interval;
                double ymin =mh.pageGeo.Envelope.YMin + widthdown+interval;
                double ymax =mh.pageGeo.Envelope.YMax - widthup-interval;
                if (ymin >= clipGeo.YMin || ymax <= clipGeo.YMax || xmin >= clipGeo.XMin || xmax <= clipGeo.XMax)
                {
                    if (ymin >= clipGeo.YMin )
                    {
                        outDownWidth -= 5;
                    }
                    if ( ymax <= clipGeo.YMax )
                    {
                        outUpWidth -= 5;
                    }
                    if ( xmin >= clipGeo.XMin )
                    {
                        outLeftWidth -= 5;
                    }
                    if ( xmax <= clipGeo.XMax)
                    {
                        outRightWidth -= 5;
                    }
                   
                }
                else
                {
                    inserect = false;
                }
            } while (inserect);

            var  maplineOutWidth = 1;
            var maplineInWidth = 0.2;
            try
            {
                //if mh.cl
                messageRaisedAction("正在处理图廓线生成!");
                bool flag = (clipGeo.Height == mh.pageGeo.Envelope.Height && clipGeo.Width == mh.pageGeo.Envelope.Width) ? true : false;//判断裁切面大小是否等于页面大小(单击定位)
                mh.CreateMapBorderLine(outUpWidth.ToString(), outDownWidth.ToString(), outLeftWidth.ToString(), outRightWidth.ToString(), inWidth.ToString(), flag);
                OverrideMapLineWidth(maplineInWidth, maplineOutWidth);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return false;
            }
            return true;
        }
        public void OverrideMapLineWidth(double maplineInWidth, double maplineOutWidth)
        {
            if (maplineInWidth == 0 || maplineOutWidth == 0)
                return;
            GApplication m_Application = GApplication.Application;
            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == "LLINE";
            })).ToArray();
            IGeoFeatureLayer geolyr = lyrs[0] as IGeoFeatureLayer;
            IRepresentationRenderer repRender = geolyr.Renderer as IRepresentationRenderer;
            IRepresentationClass repClass = repRender.RepresentationClass;

            IFeature fe = null;
            IFeatureCursor cursor = geolyr.FeatureClass.Search(null, false);
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
                repFeature.set_Value(lineAttributes, id, width * 2.83);//设置宽度
                repFeature.RepresentationClass.RepresentationRules.set_Rule(repFeature.RuleID, ruleOrg);
                repFeature.UpdateFeature();
                repFeature.Feature.Store();

            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
        }
      
    }
}
