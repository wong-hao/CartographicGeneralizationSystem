using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Carto;

namespace SMGI.Common
{
    public interface ISMGIContextMenu
    {
        object CurrentContextItem { get; }
    }

    public class SMGIContextItem 
    {
        public int x, y;
        public IActiveView view;
    }

    public class SMGIContextMenu : SMGICommand, ISMGIContextMenu
    {
        public virtual TOCSeleteItem CurrentContextItemEx
        {
            get
            {
                return m_Application.PluginManager.CurrentContextItem;
            }
            set
            {
                m_Application.PluginManager.CurrentContextItem = value;
            }
        }

        public virtual object CurrentContextItem 
        {
            get
            {
                switch (CurrentContextItemEx.Type)
                {
                    case ESRI.ArcGIS.Controls.esriTOCControlItem.esriTOCControlItemHeading:
                        return CurrentContextItemEx.Group;
                    case ESRI.ArcGIS.Controls.esriTOCControlItem.esriTOCControlItemLayer:
                        return CurrentContextItemEx.Layer;
                    case ESRI.ArcGIS.Controls.esriTOCControlItem.esriTOCControlItemLegendClass:
                        return CurrentContextItemEx.Class;
                    case ESRI.ArcGIS.Controls.esriTOCControlItem.esriTOCControlItemMap:
                        return CurrentContextItemEx.Map;
                    case ESRI.ArcGIS.Controls.esriTOCControlItem.esriTOCControlItemNone:
                    default:
                        return null;
                };
            }
        }
    }
}
