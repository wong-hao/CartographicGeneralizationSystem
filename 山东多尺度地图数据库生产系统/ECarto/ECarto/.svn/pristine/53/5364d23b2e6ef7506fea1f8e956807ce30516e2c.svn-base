using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using SMGI.Common;
using System.Linq;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.GeneralEdit
{
    public class BuildingConvert : SMGICommand
    {
        public BuildingConvert()
        {
            m_caption = "面状房屋转点线";
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
            System.Diagnostics.Trace.WriteLine("面状房屋转点线");

            var resa = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == "RESA")).FirstOrDefault();
            var resl = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == "RESL")).FirstOrDefault();
            var resp = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == "RESP")).FirstOrDefault();
            if (resa == null || resl == null || resp == null) return;

            var gds = (IGeoDataset)resa;
            var xyr = ((ISpatialReferenceTolerance)gds.SpatialReference).XYTolerance;

            using (var wo = m_Application.SetBusy())
            {
                try
                {
                    var resaFc = ((IFeatureLayer)resa).FeatureClass;
                    var reslFc = ((IFeatureLayer)resl).FeatureClass;
                    var respFc = ((IFeatureLayer)resp).FeatureClass;
                    IQueryFilter qf = new QueryFilterClass();
                    var feCu = resaFc.Search(qf, false);
                    var alCo = resaFc.FeatureCount(qf);
                    int doCo = 0;
                    IFeature fe;
                    m_Application.EngineEditor.StartOperation();
                    while ((fe = feCu.NextFeature()) != null)
                    {
                        wo.SetText("正在处理(" + (doCo++) + "/" + alCo + ")...");

                        var lines = new List<IPolyline>();
                        var pc = (IPointCollection)fe.Shape;
                        for (var i = 1; i < pc.PointCount; i++)
                        {
                            IPolyline line = new PolylineClass();
                            var lpc = (IPointCollection)line;
                            lpc.AddPoint(pc.Point[i - 1]);
                            lpc.AddPoint(pc.Point[i]);
                            lines.Add(line);
                        }
                        lines = lines.Where(i => i.Length > xyr).ToList();

                        var maxLength = lines.Max(i => i.Length);
                        var minLength = lines.Min(i => i.Length);
                        var area = ((IArea)fe.Shape).Area;
                        var maxLine = lines.First(i => Math.Abs(i.Length - maxLength) <= 0);
                        if (maxLength > 10 && minLength < 7 && lines.Count == 4)
                        {
                            var index = lines.IndexOf(maxLine);
                            var hindex = index == 0 ? lines.Count - 1 : index - 1;
                            var qindex = index == lines.Count - 1 ? 0 : index + 1;

                            IPolyline line = new PolylineClass();
                            var lpc = (IPointCollection)line;
                            lpc.AddPoint(new PointClass { X = (lines[hindex].FromPoint.X + lines[hindex].ToPoint.X) / 2, Y = (lines[hindex].FromPoint.Y + lines[hindex].ToPoint.Y) / 2 });
                            lpc.AddPoint(new PointClass { X = (lines[qindex].FromPoint.X + lines[qindex].ToPoint.X) / 2, Y = (lines[qindex].FromPoint.Y + lines[qindex].ToPoint.Y) / 2 });

                            var lfc = reslFc.Insert(true);
                            var fb = reslFc.CreateFeatureBuffer();
                            fb.Shape = line;
                            object rfid = lfc.InsertFeature(fb);
                            lfc.Flush();
                            Marshal.ReleaseComObject(lfc);
                        }
                        else if (maxLength < 10 || (area < 70 && lines.Count > 4))
                        {
                            var point = ((IArea)fe.Shape).LabelPoint;

                            var pfc = respFc.Insert(true);
                            var fb = respFc.CreateFeatureBuffer();
                            fb.Shape = point;
                            var angle = Math.Atan2((maxLine.ToPoint.Y - maxLine.FromPoint.Y), (maxLine.ToPoint.X - maxLine.FromPoint.X)) * 180 / Math.PI;
                            angle = angle > 0 ? 360 - angle : Math.Abs(angle);
                            fb.Value[fb.Fields.FindField("ANGLE")] = angle;
                            object rfid = pfc.InsertFeature(fb);
                            pfc.Flush();
                            Marshal.ReleaseComObject(pfc);
                        }
                    }
                    m_Application.EngineEditor.StopOperation("面状房屋转点线");
                    m_Application.ActiveView.Refresh();
                    wo.Dispose();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "错误");
                    wo.Dispose();
                }
            }
            MessageBox.Show("处理完成！");
        }
    }
}
