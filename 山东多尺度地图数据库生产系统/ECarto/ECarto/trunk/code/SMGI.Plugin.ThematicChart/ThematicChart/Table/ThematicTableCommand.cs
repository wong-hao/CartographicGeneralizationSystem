using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.BaseClasses;
using System.Windows.Forms;
using SMGI.Common;
using System.Linq;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    public class ThematicTableCommand :SMGITool
    {
        private IActiveView pAc;
        private double mapScale;
        public ThematicTableCommand()
        {
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public override void OnClick()
        {
            pAc = m_Application.ActiveView;
            mapScale = (m_Application.ActiveView as IMap).ReferenceScale;
        }
        public Dictionary<int, IRgbColor> FontColorDic = new Dictionary<int, IRgbColor>();
        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
                return;
            if (mapScale == 0)
            {
                MessageBox.Show("请先设置参考比例尺！");
                return;

            }
          
            m_Application.EngineEditor.StartOperation();
            //锚点
            IPoint anchorPoint = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            //ExcelToRep_Anno er = new ExcelToRep_Anno(pAc, anchorPoint, mapScale);
            ExcelToRep er = new ExcelToRep(pAc, anchorPoint, mapScale);
            if (FontColorDic.Count == 0)
            {
                FontColorDic = er.LoadExcelFontColor();
            }
            er.FontColorDic = FontColorDic;
            er.ExcelToElement();
            m_Application.EngineEditor.StopOperation("专题表格");
        }
    }
}