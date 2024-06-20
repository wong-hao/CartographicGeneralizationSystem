using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.Xml.Linq;
using System.Drawing;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using stdole;
using ESRI.ArcGIS.ADF.COMSupport;
namespace SMGI.Plugin.EmergencyMap
{
    [SMGIAutomaticCommand]
    public class MapNameSetCmd: SMGICommand
    {
        public MapNameSetCmd()
        {
            m_caption = "图名设置";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
            
        }
        FrmMapNameset frm = null;
        public override void OnClick()
        {
            if (frm == null || frm.IsDisposed)
            {
                frm = new FrmMapNameset();
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.Show();
                frm.TopMost = true;
                frm.Activate();
            }
            else 
            {
                frm.WindowState = FormWindowState.Normal;
                frm.TopMost = true;
                frm.Activate();
            }
        }

        protected override bool DoCommand(XElement args, Action<string> messageRaisedAction)
        {

            //解析参数
            try
            {
                string mapName = args.Element("MapName").Value.Trim();
                double mapNameSize = double.Parse(args.Element("MapNameSize").Value.Trim());
                string fontName = args.Element("FontName").Value.Trim(); ;//方正宋黑简体
                double mapNameInterval = double.Parse(args.Element("MapNameInterval").Value.Trim());

                string productName = args.Element("ProductName").Value.Trim();
                double productSize = double.Parse(args.Element("ProductSize").Value.Trim());

                string productTime = args.Element("ProductTime").Value.Trim();
                double timeSize = double.Parse(args.Element("TimeSize").Value.Trim());

                MapLayoutHelperLH mh = new MapLayoutHelperLH();
                IQueryFilter qf = new QueryFilterClass();
                messageRaisedAction("正在生成图名...");
                var lyr = m_Application.Workspace.LayerManager.GetLayer(l => (l is IFDOGraphicsLayer) && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == "LANNO").FirstOrDefault();
                var fclanno = (lyr as IFeatureLayer).FeatureClass;
                qf.WhereClause = "TYPE = '图名'";
                mh.ClearFeatures(fclanno, qf);
                IRgbColor color = new RgbColorClass { RGB = int.Parse(args.Element("MapNameColor").Value) };
                bool isArtStyle = true;
                if (args.Element("MapNameColor") != null)
                {
                    color = new RgbColorClass { RGB = int.Parse(args.Element("MapNameColor").Value) };
                    isArtStyle = Convert.ToBoolean(args.Element("IsArtStyle").Value);
                    mh.MapNameSpace = Convert.ToDouble(args.Element("MapNameSpace").Value);
                    mh.MapNameWidth = Convert.ToDouble(args.Element("MapNameWidth").Value);
                    mh.MapNameDis = Convert.ToDouble(args.Element("MapNameDis").Value);
                }
                //图名
                if (mapName != "")
                {

                    var font = (stdole.IFontDisp)OLE.GetIFontDispFromFont(new System.Drawing.Font(fontName, (float)(mapNameSize * 2.83)));
                    mh.CreateMapName(isArtStyle, mapName.Trim(), font, color, mapNameInterval);
                }
                //其他采用默认的黑色
                color = new RgbColorClass { Red = 0, Green = 0, Blue = 0 };
                messageRaisedAction("正在生成制作单位...");
                qf.WhereClause = "TYPE = '生产商'";
                mh.ClearFeatures(fclanno, qf);
                if (productName != "")
                {
                    var font = (stdole.IFontDisp)OLE.GetIFontDispFromFont(new System.Drawing.Font(fontName, (float)(mapNameSize * 2.83)));
                    mh.CreateMapProducer(productName, (stdole.IFontDisp)OLE.GetIFontDispFromFont(new System.Drawing.Font("黑体", (float)(productSize * 2.83))), color, "生产商");
                }
                messageRaisedAction("正在生成生产时间...");
                qf.WhereClause = "TYPE = '生产时间'";
                mh.ClearFeatures(fclanno, qf);
                mh.CreateMapProducer(productTime, (stdole.IFontDisp)OLE.GetIFontDispFromFont(new System.Drawing.Font("黑体", (float)(timeSize * 2.83))), color, "生产时间");

                messageRaisedAction("正在生成出版形式标签...");
                qf.WhereClause = "TYPE = '出版形式'";
                mh.ClearFeatures(fclanno, qf);
                var publishFont = new System.Drawing.Font("黑体", 25);
                mh.CreateMapPublishType("内部用图", (stdole.IFontDisp)OLE.GetIFontDispFromFont(publishFont), color, 3, false);

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
    }
}
