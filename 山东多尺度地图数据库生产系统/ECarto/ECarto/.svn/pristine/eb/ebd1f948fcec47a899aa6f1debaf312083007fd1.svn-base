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
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Controls;
namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 添加附图
    /// </summary>
    [SMGIAutomaticCommand]
    public class AddFigureMapCmdXJ: SMGICommand
    {

        public AddFigureMapCmdXJ()
        {
            m_caption = "添加附图";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && 
                    m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }
        
        public override void OnClick()      
        {
            string err = "";
            var lyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l => { return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == "LLINE"); })).First();
            IFeatureClass fclclip = (lyr as IFeatureLayer).FeatureClass;//lline：内图廓
            
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "TYPE='内图廓'";
            int ct = fclclip.FeatureCount(qf);
            if (ct == 0)
            {
                MessageBox.Show("请先生成内图廓");
                return;
            }

            IFeatureCursor fcursor = fclclip.Search(qf, false);
            IFeature felline = fcursor.NextFeature();
            IEnvelope feEnv = felline.ShapeCopy.Envelope;
            Marshal.ReleaseComObject(fcursor);
            Marshal.ReleaseComObject(qf);
            var paramContent = EnvironmentSettings.getContentElement(m_Application);
            var pagesize = paramContent.Element("PageSize");//页面大小
            double w = double.Parse(pagesize.Element("Width").Value);
            double h = double.Parse(pagesize.Element("Height").Value);
            double size = w < h ? w : h;
            size = size * 0.2;//默认纸张最小边长的五分之一
            FrmFigureMapSet frm = new FrmFigureMapSet(feEnv, size);
            DialogResult dr = frm.ShowDialog();
            if (dr != DialogResult.OK)
                return ;

            m_Application.EngineEditor.StartOperation();

            using (var wo = m_Application.SetBusy())
            {
                wo.SetText("正在添加附图......");

                AddFigureMap addFigMap = new AddFigureMap(m_Application);
                IPoint pt = CommonMethods.GetFigureMapPoint(feEnv, frm.Orientation, frm.FigureMapSize * 1e-3 * m_Application.ActiveView.FocusMap.ReferenceScale);
                err = addFigMap.addFigureMapXJ(frm.FigureMapSize, pt, frm.Orientation, feEnv);//导入附图

            }

            if (string.IsNullOrEmpty(err))
            {
                m_Application.EngineEditor.StopOperation("添加位置图");
            }
            else
            {
                m_Application.EngineEditor.AbortOperation();

                MessageBox.Show(err);
            }
        }
        protected override bool DoCommand(System.Xml.Linq.XElement args, Action<string> messageRaisedAction)
        {
            string err = "";
            try
            {
                messageRaisedAction("正在生成位置图...");
                var lyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l => { return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == "LLINE"); })).First();
                IFeatureClass fclclip = (lyr as IFeatureLayer).FeatureClass;//lline：内图廓

                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = "TYPE='内图廓'";
                int ct = fclclip.FeatureCount(qf);
                if (ct == 0)
                {
                    MessageBox.Show("请先生成内图廓");
                    return false;
                }
                IFeatureCursor fcursor = fclclip.Search(qf, false);
                IFeature felline = fcursor.NextFeature();
                IEnvelope feEnv = felline.ShapeCopy.Envelope;
           

                //图例外边线
                IEnvelope polygonEnvelop = null;
                IFeatureClass fclPolygon = null;
                lyr = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LPOLY"))).FirstOrDefault();
                if (lyr != null)
                {
                    fclPolygon = (lyr as IFeatureLayer).FeatureClass;
                }
                qf.WhereClause = "TYPE = '图例外边线'";
                fcursor = fclPolygon.Search(qf, false);
                IFeature lgfeature = fcursor.NextFeature();
                if (lgfeature != null)
                {
                    polygonEnvelop = lgfeature.ShapeCopy.Envelope;
                }
                //裁切面
                IFeatureClass pagefcl = null;
                IGeometry pageGeo = null;
                lyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "ClipBoundary");
                })).FirstOrDefault() as IFeatureLayer;
                if (lyr != null)
                {
                    pagefcl = (lyr as IFeatureLayer).FeatureClass;
                }
                qf.WhereClause = "TYPE = '裁切面'";
                fcursor = pagefcl.Search(qf, false);
                IFeature  sdmfeature = fcursor.NextFeature();
                if (lgfeature != null)
                {
                    pageGeo = sdmfeature.ShapeCopy;
                }

                Marshal.ReleaseComObject(fcursor);
                Marshal.ReleaseComObject(qf);
                var paramContent = EnvironmentSettings.getContentElement(m_Application);
                var pagesize = paramContent.Element("PageSize");//页面大小
                double w = double.Parse(pagesize.Element("Width").Value);
                double h = double.Parse(pagesize.Element("Height").Value);
                double size = w < h ? w : h;
                size = size * 0.18;//默认纸张大小
                if (args.Element("Location") != null && args.Element("Size") != null)
                {
                    string location = args.Element("Location").Value;
                    double mapSize = double.Parse(args.Element("Size").Value);
                    AddFigureMap addFigMap = new AddFigureMap(m_Application);
                    IPoint pt = CommonMethods.GetFigureMapPoint(feEnv, location, mapSize * 1e-3 * m_Application.ActiveView.FocusMap.ReferenceScale);
                    err = addFigMap.addFigureMapXJ(mapSize, pt, location, feEnv);//导入附图
                }
                else 
                {
                    FrmFigureMapSet frm = new FrmFigureMapSet(feEnv, size); 
                    AddFigureMap addFigMap = new AddFigureMap(m_Application);
                    IPoint pt = CommonMethods.GetFigureMapPoint(feEnv, frm.Orientation, frm.FigureMapSize * 1e-3 * m_Application.ActiveView.FocusMap.ReferenceScale);
                    err = addFigMap.addFigureMapXJ(frm.FigureMapSize, pt, frm.Orientation, feEnv);//导入附图
                }
                

                if (!string.IsNullOrEmpty(err))
                {
                    MessageBox.Show(err);
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return false;
            }
            return true;
        }

    }
}
