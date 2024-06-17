using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
namespace BuildingGen
{
    internal class GMapControlManager
    {
        AxMapControl ctrl;
        internal GMapControlManager(AxMapControl mapControl)
        {
            ctrl = mapControl;            
            ctrl.OnMouseDown += new IMapControlEvents2_Ax_OnMouseDownEventHandler(ctrl_OnMouseDown);            
        }

        void ctrl_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            if (e.button != 4)
            {
                return;
            }
            ctrl.Pan();
        }
    }
}
