using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;


namespace DatabaseUpdate
{
    public static class FindClass
    {

        public static List<int> findUpdatesWithObjects(IFeatureLayer MxLayer, IFeatureLayer McLayer)
        {
            IFeatureClass MxClass = MxLayer.FeatureClass;
            IFeatureClass McClass = McLayer.FeatureClass;

            GeometryBagClass gb = new GeometryBagClass();
            IFeatureCursor fcur = MxClass.Search(null, false);
            IFeature feat = null;
            object miss = Type.Missing;
            while ((feat = fcur.NextFeature()) != null)
            {
                gb.AddGeometry(feat.ShapeCopy, ref miss, ref miss);
            }
            IPolygon poly = new PolygonClass();
            (poly as ITopologicalOperator).ConstructUnion(gb);

            List<int> McObjects = new List<int>();
            ISpatialFilter sf = new SpatialFilterClass();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelOverlaps;
            sf.Geometry = poly;
            IFeatureCursor fcur2 = McClass.Search(sf, false);
            while ((feat = fcur2.NextFeature()) != null)
            {
                McObjects.Add(feat.OID);
            }
            return McObjects;
            System.Runtime.InteropServices.Marshal.ReleaseComObject(fcur);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(fcur2);
        }
        
     
    }

}
