using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.DataSourcesGDB;
using SMGI.Common;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.GeneralEdit
{
    public class LineAnglePro : SMGICommand
    {
        public LineAnglePro()
        {
            m_caption = "线打折处理";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public override void OnClick()
        {
            //获取线要素
            var gls = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && l.Visible && (l as IGeoFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline);

            m_Application.EngineEditor.StartOperation();
            foreach (var gl in gls)
            {
                var feCu = ((IFeatureLayer)gl).Search(null, false);
                IFeature fe;
                while ((fe = feCu.NextFeature()) != null)
                {
                    var isChange = false;

                    IGeometryCollection geos = new PolylineClass();
                    var gc = (IGeometryCollection)fe.ShapeCopy;
                    for (var i = 0; i < gc.GeometryCount; i++)
                    {
                        //核心处理
                        var pc = (IPointCollection) gc.Geometry[i];
                        IPointCollection npc = new PathClass();
                        npc.AddPoint(pc.Point[0]);

                        for (var j = 1; j < pc.PointCount - 1; j++)
                        {
                            var a = Math.Sqrt((pc.Point[j].X - npc.Point[npc.PointCount - 1].X) * (pc.Point[j].X - npc.Point[npc.PointCount - 1].X) + (pc.Point[j].Y - npc.Point[npc.PointCount - 1].Y) * (pc.Point[j].Y - npc.Point[npc.PointCount - 1].Y));
                            var b = Math.Sqrt((pc.Point[j + 1].X - pc.Point[j].X) * (pc.Point[j + 1].X - pc.Point[j].X) + (pc.Point[j + 1].Y - pc.Point[j].Y) * (pc.Point[j + 1].Y - pc.Point[j].Y));
                            var c = Math.Sqrt((pc.Point[j + 1].X - npc.Point[npc.PointCount - 1].X) * (pc.Point[j + 1].X - npc.Point[npc.PointCount - 1].X) + (pc.Point[j + 1].Y - npc.Point[npc.PointCount - 1].Y) * (pc.Point[j + 1].Y - npc.Point[npc.PointCount - 1].Y));
                            var angle = Math.Acos((a * a + b * b - c * c) / (2 * a * b)) * 180 / Math.PI;
                            if (angle > 20) npc.AddPoint(pc.Point[j]);
                            else isChange = true;
                        }
                        npc.AddPoint(pc.Point[pc.PointCount - 1]);
                        geos.AddGeometry(npc as IGeometry);
                    }

                    if (isChange)
                    {
                        fe.Shape = geos as IGeometry;
                        fe.Store();
                    }
                }
            }

            m_Application.EngineEditor.StopOperation("线打折处理");
            m_Application.ActiveView.Refresh();
            MessageBox.Show("处理完成！");
        }
    }
}
