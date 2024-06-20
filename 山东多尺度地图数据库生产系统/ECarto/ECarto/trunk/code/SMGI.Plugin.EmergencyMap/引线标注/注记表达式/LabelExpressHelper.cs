using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.SystemUI;
using System.Security.Cryptography;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
namespace SMGI.Plugin.EmergencyMap.LabelSym
{
    public  class LabelExpressHelper
    {

        public static string MapLableFromLyr(ILayer lyr,IFeature fe,string express)
        {
            try
            {
                FrmAnnoMap frm = new FrmAnnoMap(lyr, fe.OID);
                frm.ConvertLabelsToAnnotationSingleLayerMapAnno(0, express);
                string infos = frm.MapLabels;
                frm.Close();
                frm.Dispose();
                return infos;
            }
            catch(Exception ex)
            {
                return "";
            }
        }
    }
}
