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
using System.IO;
namespace SMGI.Plugin.EmergencyMap
{
   
    [SMGIAutomaticCommand]
    public class FootBorderCmd : SMGICommand
    {
         
        public FootBorderCmd()
        {
            m_caption = "花边设置";
            m_toolTip = "花边设置";
            m_category = "图廓整饰";

        }
        FrmFootBorder frm = null;
        public override void OnClick()
        {
            if (frm == null || frm.IsDisposed)
            {
                frm = new FrmFootBorder();
                frm.Show();
            }
            else
            {
                frm.WindowState = FormWindowState.Normal;
                frm.Activate();
            }
           
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
            messageRaisedAction("正在生成花边....");
            try
            {
                double borderWidth = double.Parse(args.Element("BorderWidth").Value);
                double borderInterval = double.Parse(args.Element("BorderInterval").Value);
                //默认为第一个花边
                var cornerLace = new LaceParam { Angle = 0, Flip = false, Name = args.Element("CornerName").Value };
                var laceList = new List<LaceParam> { new LaceParam { Angle = 0, Flip = false, Name = args.Element("BorderName").Value } };            
                var GDBname = "LACE";
                var mh = new MapLayoutHelperLH("LACE");
                mh.CreateFootBorder2(laceList, cornerLace, borderWidth, borderInterval, GDBname);

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
