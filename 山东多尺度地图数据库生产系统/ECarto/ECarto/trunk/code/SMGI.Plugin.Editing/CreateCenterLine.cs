using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;

namespace SMGI.Plugin.GeneralEdit
{
    public class CreateCenterLine : SMGICommand
    {
        public CreateCenterLine()
        {
            m_caption = "生成中心线";
            m_toolTip = "生成中心线";
            m_category = "生成中心线";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing && 
                       m_Application.Workspace != null;
                     
            }
        }
        public override void OnClick()
        {
            var view = m_Application.ActiveView;
            ILayer pLayer = m_Application.TOCSelectItem.Layer;
            if (!(pLayer is FeatureLayer))
            {
                MessageBox.Show("请选择要素层！");
                return;
            }
            if (((IFeatureLayer)pLayer).FeatureClass.ShapeType != esriGeometryType.esriGeometryPolyline)
            {
                MessageBox.Show("请选择线要素层！");
                return;
            }

            var map = view.FocusMap;
            var wo = GApplication.Application.SetBusy();
            try
            {
                var hydlFc = ((IFeatureLayer)pLayer).FeatureClass;
                //var clh = new CenterLineHelper();
                var clFactory = new CenterLineCut.CenterLineFactory();
                var enFe = (IEnumFeature)map.FeatureSelection;
                enFe.Reset();
                IFeature fe;
                while ((fe = enFe.Next()) != null)
                {
                    //var cl = clh.Create(fe.Shape as IPolygon).Line;
                    var cl = clFactory.Create2(fe.Shape as IPolygon).Line;
                    var gc = (IGeometryCollection)cl;
                    for (var i = 0; i < gc.GeometryCount; i++)
                    {
                        var pl = new PolylineClass();
                        pl.AddGeometry(gc.Geometry[i]);
                        var fci = hydlFc.Insert(true);
                        var fb = hydlFc.CreateFeatureBuffer();
                        fb.Shape = pl;
                        fci.InsertFeature(fb);
                        fci.Flush();
                    }
                }
                view.Refresh();
                wo.Dispose();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "错误");
                wo.Dispose();
            }
        }
    }
}
