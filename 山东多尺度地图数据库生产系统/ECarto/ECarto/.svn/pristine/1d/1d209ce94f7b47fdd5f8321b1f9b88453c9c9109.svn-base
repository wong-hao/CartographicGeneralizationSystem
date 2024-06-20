using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoAnalyst;

namespace SMGI.Plugin.EmergencyMap
{
    public class HillShading
    {
        public HillShading()
        {
        }


        /// <summary>
        /// 提取山体阴影
        /// </summary>
        /// <param name="dem"></param>
        /// <param name="azimuth"></param>
        /// <param name="altitude"></param>
        /// <param name="inModelShadows"></param>
        /// <param name="zFactor"></param>
        public IRaster ExtractHillShade(IRaster dem, double azimuth, double altitude, bool inModelShadows, object zFactor)
        {
            ISurfaceOp surfaceOP = new RasterSurfaceOpClass();
            IGeoDataset hillshade = surfaceOP.HillShade(dem as IGeoDataset, azimuth, altitude, inModelShadows, ref zFactor);

            return hillshade as IRaster;
        }
    }
}
