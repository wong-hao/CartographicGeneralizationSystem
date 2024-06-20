using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;

namespace BuildingGen {
    class BuildingRenderer:IFeatureRenderer {
        Dictionary<object, int> dic;
        Random r ;
        public BuildingRenderer() {
            symbol = new SimpleFillSymbolClass();
            lineSymbol = new SimpleLineSymbolClass();
            fillColor = new HsvColorClass();
            lineColor = new HsvColorClass();
            setColor(fillColor, dColor[0], dColor[1], dColor[2]);
            setColor(lineColor, 0, 0, 0);
            symbol.Color = fillColor;
            lineSymbol.Color = lineColor;
            symbol.Outline = lineSymbol;

            defaultSymbol = (symbol as ESRI.ArcGIS.esriSystem.IClone).Clone() as ISimpleFillSymbol;
            dic = new Dictionary<object, int>();
            dic.Add(0, dColor[0]);
            dic.Add(-1, 10);
            dic.Add(-2, 50);
            dic.Add(-3, 50);
            r = new Random();
        }
        #region IFeatureRenderer 成员

        bool IFeatureRenderer.CanRender(ESRI.ArcGIS.Geodatabase.IFeatureClass featClass, ESRI.ArcGIS.Display.IDisplay Display) {
            return featClass != null
                && featClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon;
        }

        void IFeatureRenderer.Draw(ESRI.ArcGIS.Geodatabase.IFeatureCursor cursor, ESRI.ArcGIS.esriSystem.esriDrawPhase DrawPhase, ESRI.ArcGIS.Display.IDisplay Display, ESRI.ArcGIS.esriSystem.ITrackCancel TrackCancel) {
            int groupID = cursor.FindField("G_BuildingGroup");
            int structID = cursor.FindField("建筑结构");
            IFeature f = null;
            
            setColor(fillColor, dColor[0], dColor[1], dColor[2]);
            setColor(lineColor, 0, 0, 0);
            lineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;

            dic[-1] = 0;
            dic[-2] = 330;
            dic[-3] = 330;

            while ((f = cursor.NextFeature()) != null) {
                symbol = (defaultSymbol as ESRI.ArcGIS.esriSystem.IClone).Clone() as ISimpleFillSymbol;

                if (exclusionSet == null || !exclusionSet.get_Contains(f.OID)) {                    
                    if ((Mode & FeatureRenderMode.强化分组显示) != 0 && groupID != -1) {
                        object g = f.get_Value(f.Fields.FindField("G_BuildingGroup"));
                        if (g == null  ||  g == DBNull.Value) {
                            g = 0;
                        }
                        if (!dic.ContainsKey(g)) { 
                            dic.Add(g,r.Next(0, 360));
                        }
                        int s = dColor[1];
                        if ((int)g == -1) {
                            s = 10;
                        }
                        setColor(fillColor,dic[g] , s, dColor[2]);
                        symbol.Color = fillColor;
                    }
                    if ((Mode & FeatureRenderMode.强化建筑结构显示) != 0 && structID != -1) {
                        object v = f.get_Value(f.Fields.FindField("建筑结构"));
                        if (v == null || v == DBNull.Value) {
                            v = "破";
                        }
                        switch (v.ToString()) {
                        case "A":
                        case "B":
                            lineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
                            lineSymbol.Width = 2;
                            setColor(lineColor, 0, 0, 0);
                            break;
                        case "C":
                        case"D":
                            lineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
                            lineSymbol.Width = 2;
                            setColor(lineColor, 0, 0, 60);
                            break;
                        case "棚":
                            lineSymbol.Style = esriSimpleLineStyle.esriSLSDash;
                            lineSymbol.Width = 1;
                            setColor(lineColor, 0, 0, 0);
                            break;
                        default:
                            lineSymbol.Style = esriSimpleLineStyle.esriSLSDash;
                            lineSymbol.Width = 1;
                            setColor(lineColor, 0, 0, 60);
                            break;
                        }
                        lineSymbol.Color = lineColor;
                        symbol.Outline = lineSymbol;                        
                    }

                    Display.SetSymbol(symbol as ISymbol);
                    Display.DrawPolygon(f.Shape);
                }
            }
        }

        IFeatureIDSet exclusionSet;
        IFeatureIDSet IFeatureRenderer.ExclusionSet {
            set { exclusionSet = value; }
        }

        void IFeatureRenderer.PrepareFilter(ESRI.ArcGIS.Geodatabase.IFeatureClass fc, ESRI.ArcGIS.Geodatabase.IQueryFilter queryFilter) {
            queryFilter.AddField("*");
            return;
        }

        bool IFeatureRenderer.get_RenderPhase(ESRI.ArcGIS.esriSystem.esriDrawPhase DrawPhase) {
            return DrawPhase == ESRI.ArcGIS.esriSystem.esriDrawPhase.esriDPGeography;
        }

        ESRI.ArcGIS.Display.ISymbol IFeatureRenderer.get_SymbolByFeature(ESRI.ArcGIS.Geodatabase.IFeature Feature) {
            return symbol as ISymbol;
        }

        #endregion
        public static int[] dColor = new int[] { 60, 40, 100 };
        public FeatureRenderMode Mode;
        private ISimpleFillSymbol symbol;
        private ISimpleFillSymbol defaultSymbol;
        private ISimpleLineSymbol lineSymbol;
        private IHsvColor fillColor;
        private IHsvColor lineColor;
        private void setColor(IHsvColor color, int h, int s, int v) {
            color.Hue = h;
            color.Saturation = s;
            color.Value = v;
        }
    }
}
