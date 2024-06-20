using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.EmergencyMap
{
    public class CoordinateLocationCmd : SMGICommand
    {
        public CoordinateLocationCmd()
        {
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace==null && m_Application.MapControl.Map.LayerCount > 0;
            }
        }

        public override void OnClick()
        {
            LocationForm frm = new LocationForm(m_Application);
            if (DialogResult.OK == frm.ShowDialog())
            {
                IPoint centerPoint = new PointClass { X = frm.Longitude, Y = frm.Latitude };//中心点
                PaperSize ps = frm.ClipPaperSize;

                ISpatialReferenceFactory srcFactory = new SpatialReferenceEnvironmentClass();
                centerPoint.SpatialReference = srcFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCS3Type.esriSRGeoCS_Xian1980);//固定的???

                IGroupElement clipELe = ClipElement.createClipGroupElement(m_Application, centerPoint, ps.PaperWidth, ps.PaperHeight, frm.RefScale);
                ClipElement.SetClipGroupElement(m_Application, clipELe);
            }
        }
    }
}
