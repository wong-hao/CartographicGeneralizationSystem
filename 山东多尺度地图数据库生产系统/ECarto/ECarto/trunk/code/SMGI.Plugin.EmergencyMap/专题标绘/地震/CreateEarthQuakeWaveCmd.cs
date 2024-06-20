using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using SMGI.Common;
using stdole;

namespace SMGI.Plugin.EmergencyMap
{
    public class CreateEarthQuakeWaveCmd : SMGI.Common.SMGICommand
    {
        
        public CreateEarthQuakeWaveCmd()
        {
            m_caption = "创建地震波";
          
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null;
            }
        }
        public override void OnClick()
        {
            if (m_Application.MapControl.Map.ReferenceScale <= 0)
            {
                MessageBox.Show("请设置当前地图的参考比例尺！");
                return;
            }

            if (m_Application.ActiveView.FocusMap.SpatialReference == null)
            {
                MessageBox.Show("请设置当前地图的空间参考！");
                return;
            }


            var frm = new CreateEarthQuakeWaveForm(m_Application);
            if (frm.ShowDialog() != DialogResult.OK)
            {
                CreateEarthQuakeWaveForm.DeleteEarthQuakeGroupElement(m_Application.MapControl.ActiveView.GraphicsContainer, frm.QuakeCenterCoordText);
                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, m_Application.ActiveView.Extent);
            }
        }
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
          
        }         
    }
}
