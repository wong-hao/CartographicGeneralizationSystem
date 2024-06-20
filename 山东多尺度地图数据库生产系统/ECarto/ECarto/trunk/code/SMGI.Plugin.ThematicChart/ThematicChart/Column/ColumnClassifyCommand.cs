using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;

namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    public class ColumnClassifyCmd : SMGITool
    {
        private IActiveView pAc;
        private double mapScale;
        public ColumnClassifyCmd()
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
        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
                return;
            //锚点
            IPoint anchorPoint = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            FrmClassifySet frm = new FrmClassifySet(anchorPoint,"二维分类柱状图");
            frm.Text = "二维分类柱状图";

        }
    }
}
