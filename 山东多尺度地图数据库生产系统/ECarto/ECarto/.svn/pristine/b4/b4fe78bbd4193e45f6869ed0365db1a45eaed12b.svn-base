using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.EmergencyMap
{
    //输出专题点
    public sealed class LablePointExportCmd :SMGICommand
    {

        public LablePointExportCmd()
        {
            
        }

        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null;
            }
        }
        

        #region Overridden Class Methods

        
        IGraphicsContainerSelect gs = null;
        IGraphicsContainer gc = null;
        IActiveView act = null;
      
        public override void OnClick()
        {
            act = m_Application.ActiveView;
            gc = act as IGraphicsContainer;
            gs = act as IGraphicsContainerSelect;
            FrmLbExport frm = new FrmLbExport();
            frm.ShowDialog();
        }
       
        #endregion
    }
}
