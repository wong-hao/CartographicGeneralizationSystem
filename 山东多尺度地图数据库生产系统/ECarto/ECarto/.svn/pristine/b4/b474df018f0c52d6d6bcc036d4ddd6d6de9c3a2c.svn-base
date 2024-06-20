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
namespace SMGI.Plugin.EmergencyMap
{
    [SMGIAutomaticCommand]
    public class CompassCmd : SMGICommand
    {
        public CompassCmd()
        {
            m_caption = "指北针设置";
            m_toolTip = "指北针设置";
            m_category = "图廓整饰";

        }
      
        public override void OnClick()
        {
            FrmCompassSet frm = new FrmCompassSet();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                string GDBname = "COMPASS";
                m_Application.EngineEditor.StartOperation();
                MapLayoutHelperLH mh = new MapLayoutHelperLH(GDBname);
                mh.CreateCompass(frm.CompassName, GDBname, frm.CompassSize.ToString(),frm.AnchorLocation);
                m_Application.EngineEditor.StopOperation(this.Caption);
                mh = null;
            }
            GC.Collect();
        }
        public override bool Enabled
        {
            get
            {

                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }
        protected override bool DoCommand(System.Xml.Linq.XElement args, Action<string> messageRaisedAction)
        {
            string sourceFileGDB = GApplication.Application.Workspace.FullName;
            try
            {
                string compassName = "指北针1";
                string anchorLocation = "右上";
                string GDBname = "COMPASS";
             
                var paramContent = EnvironmentSettings.getContentElement(m_Application);
                var pagesize = paramContent.Element("PageSize");//页面大小
                double w = double.Parse(pagesize.Element("Width").Value);
                double h = double.Parse(pagesize.Element("Height").Value);
                double compassSize = w < h ? w : h;
                compassSize = (int)(compassSize * 0.01) * 5.0;//默认纸张最小边长的20分之一

                if (args.Element("CompassName") != null)
                {
                    compassName = args.Element("CompassName").Value;
                }
                if (args.Element("CompassSize") != null)
                {
                    compassSize =double.Parse(args.Element("CompassSize").Value);
                }
                if (args.Element("AnchorLocation") != null)
                {
                    anchorLocation = args.Element("AnchorLocation").Value;
                }

                MapLayoutHelperLH mh = new MapLayoutHelperLH(GDBname);
                mh.CreateCompass(compassName, GDBname, compassSize.ToString(), anchorLocation);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return false;
            }
        }
    }
}
