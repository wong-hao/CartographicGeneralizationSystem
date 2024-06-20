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
using ESRI.ArcGIS.Controls;
using System.IO;
namespace SMGI.Plugin.EmergencyMap
{
    [SMGIAutomaticCommand]
    public class ScaleBarCmd: SMGICommand
    {
        public ScaleBarCmd()
        {
            m_caption = "比例尺设置";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
            
        }
        private bool processCmd(System.Xml.Linq.XElement args)
        {
            string sourceFileGDB = GApplication.Application.Workspace.FullName;
            try
            {
                string compassName = args.Element("ScaleBarName").Value;
                string scaleLocation = args.Element("ScalebarLocation").Value;
                string scaleUnit = args.Element("ScaleUnit").Value;
                List<string> scaleNote = new List<string>();
                if (args.Element("ScaleNote").Elements("Items") != null && args.Element("ScaleNote").Elements("Items").Count() != 0)
                {
                    foreach (var ele in args.Element("ScaleNote").Elements("Items"))
                    {
                        scaleNote.Add(ele.Value);
                    }
                }
                double scaleFont = double.Parse(args.Element("ScaleFont").Value);
                string GDBname = "SCALE";
                MapLayoutHelperLH mh = new MapLayoutHelperLH(GDBname);
                mh.ScaleLoaction = scaleLocation;
                mh.ScaleUnit = scaleUnit;
                mh.CreateScaleBar(compassName, GDBname, scaleNote, scaleFont);
                string dirName = new FileInfo(sourceFileGDB).DirectoryName;
                string filexml = dirName + "\\auto.xml";
                if (File.Exists(filexml))
                {
                    AutoMapHelper.UpdateState(filexml, "生成图例成功");
                }
                return true;
            }

            catch
            {
                string dirName = new FileInfo(sourceFileGDB).DirectoryName;
                string filexml = dirName + "\\auto.xml";
                if (File.Exists(filexml))
                {
                    AutoMapHelper.UpdateState(filexml, "生成图例失败");
                }
                return false;
            }
        }
        protected override bool DoCommand(System.Xml.Linq.XElement args, Action<string> messageRaisedAction)
        {
            string sourceFileGDB = GApplication.Application.Workspace.FullName;
            try
            {
                if(args.Element("ScaleNote")!=null)
                    return processCmd(args);
                string compassName = args.Element("ScaleBar").Value;
                string GDBname = "SCALE";
                MapLayoutHelperLH mh = new MapLayoutHelperLH(GDBname);
                mh.CreateScaleBar(compassName, GDBname);
               
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
        public override void OnClick()
        {
            FrmScaleBar frm = new FrmScaleBar();
            frm.StartPosition = FormStartPosition.CenterScreen;
            frm.ShowDialog();
        }

      
    }
}
