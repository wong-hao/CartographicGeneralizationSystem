using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;
using System.Drawing;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using SMGI.Plugin.EmergencyMap.MapDeOut;
using System.Runtime.InteropServices;
using System.IO;
namespace SMGI.Plugin.EmergencyMap
{
    [SMGIAutomaticCommand]
    public class LengendDyCreateCmd : SMGICommand
    {
       
        public LengendDyCreateCmd()
        {
            m_caption = "图例动态生成";
            m_toolTip = "图例动态生成";
            m_category = "图廓整饰";

        }
        public List<TreeNode> LyrTreeNodes = new List<TreeNode>();
        FrmLgSet frm = null;
        public override void OnClick()
        {
            if (frm == null || frm.IsDisposed)
            {
                string path = m_Application.Template.Root + @"\整饰\图例专家库\LengendRule.xml";
                LengendModel frmState = LengendModel.MapView;
                var repLyr = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LLINE"))).FirstOrDefault();
                var linefcl = (repLyr as IFeatureLayer).FeatureClass;
                IQueryFilter qf = new QueryFilterClass();
                (linefcl as IFeatureClassLoad).LoadOnlyMode = false;
                qf.WhereClause = "TYPE like '%图廓%'";
                IGeometry clipGeoIn = null;
                IGeometry clipGeoOut = null;
                if (linefcl.FeatureCount(qf) > 0)
                {
                    IFeature fe = null;
                    IFeatureCursor cursor = linefcl.Search(qf, false);
                    while ((fe = cursor.NextFeature()) != null)
                    {
                        string type = fe.get_Value(linefcl.FindField("TYPE")).ToString();
                        if (type == "内图廓")
                        {
                            clipGeoIn = fe.ShapeCopy.Envelope;
                        }
                        if (type == "外图廓")
                        {
                            clipGeoOut = fe.ShapeCopy.Envelope;
                        }
                        Marshal.ReleaseComObject(fe);
                    }
                    Marshal.ReleaseComObject(cursor);
                }
                if (clipGeoIn == null)
                {
                    MessageBox.Show("不存在内图廓，请先生成");
                    return;
                }

                frm = new FrmLgSet(frmState, clipGeoIn.Envelope, clipGeoOut.Envelope);
                frm.cmd = this;
                frm.Show();
            }
            else
            {
                frm.WindowState = FormWindowState.Normal;
                frm.Activate();
            }
        }
      
        
        protected override bool DoCommand(System.Xml.Linq.XElement args, Action<string> messageRaisedAction)
        {
            try
            {
                string sourceFileGDB = GApplication.Application.Workspace.FullName;
                messageRaisedAction("正在生成图例...");
                #region
                var repLyr = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LLINE"))).FirstOrDefault();
                var linefcl = (repLyr as IFeatureLayer).FeatureClass;
                IQueryFilter qf = new QueryFilterClass();
                (linefcl as IFeatureClassLoad).LoadOnlyMode = false;
                qf.WhereClause = "TYPE like '%图廓%'";
                IGeometry clipGeoIn = null;
                IGeometry clipGeoOut = null;
                if (linefcl.FeatureCount(qf) > 0)
                {
                    IFeature fe = null;
                    IFeatureCursor cursor = linefcl.Search(qf, false);
                    while ((fe = cursor.NextFeature()) != null)
                    {
                        string type = fe.get_Value(linefcl.FindField("TYPE")).ToString();
                        if (type == "内图廓")
                        {
                            clipGeoIn = fe.ShapeCopy.Envelope;
                        }
                        if (type == "外图廓")
                        {
                            clipGeoOut = fe.ShapeCopy.Envelope;
                        }
                        Marshal.ReleaseComObject(fe);
                    }
                    Marshal.ReleaseComObject(cursor);
                }
                if (clipGeoIn == null)
                {
                    MessageBox.Show("生成图例失败：不存在图廓线");
                    return false;
                }
                #endregion

                FrmLgSet frm = new FrmLgSet(LengendModel.MapView, clipGeoIn.Envelope, clipGeoOut.Envelope);
                frm.cmd = this;
                if (args.Element("Location") != null)
                {
                    frm.LGLocation = args.Element("Location").Value;
                }
                frm.ViewLengend();
                frm.Dispose();

                return true;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return false;
            }
        }

        public override bool Enabled
        {
            get
            {

                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing; ;
            }
        }
    
    }
}
