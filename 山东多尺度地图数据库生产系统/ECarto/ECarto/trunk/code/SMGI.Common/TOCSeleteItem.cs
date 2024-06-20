using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;

namespace SMGI.Common
{
    public class TOCSeleteItem
    {
        public esriTOCControlItem Type { get; private set; }
        public IBasicMap Map { get; private set; }
        public ILayer Layer { get; private set; }
        public ILegendGroup Group { get; private set; }
        public ILegendClass Class { get; private set; }

        public TOCSeleteItem(esriTOCControlItem type,IBasicMap map,ILayer layer,ILegendGroup group,ILegendClass cls)
        {
            Type = type;
            Map = map;
            Layer = layer;
            Group = group;
            Class = cls;
        }
    }
}
