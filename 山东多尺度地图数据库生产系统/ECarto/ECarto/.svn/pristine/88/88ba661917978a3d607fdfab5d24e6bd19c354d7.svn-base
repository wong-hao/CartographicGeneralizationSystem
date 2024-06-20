using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.BaseClasses;
using System.Windows.Forms;
using SMGI.Common;
using System.Linq;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    //饼状图属性监听
    public class PieAttrsCommand :SMGITool
    {
        private IActiveView pAc;
        private IFeatureLayer pTableLayer=null;//监听自由表达图层
        private double mapScale;
        private Dictionary<int, string> feAttrs = new Dictionary<int, string>();
        public PieAttrsCommand()
        {
        }

        public override bool Enabled
        {
            get
            {

                return m_Application != null && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
          
            IEngineEditEvents_Event engineEdit_Events = m_Application.EngineEditor as IEngineEditEvents_Event;
            engineEdit_Events.OnChangeFeature+=new IEngineEditEvents_OnChangeFeatureEventHandler(engineEdit_Events_OnChangeFeature);
            engineEdit_Events.OnCreateFeature+=new IEngineEditEvents_OnCreateFeatureEventHandler(engineEdit_Events_OnCreateFeature);
            m_Application.WorkspaceOpened+=new EventHandler(m_Application_WorkspaceOpened);
            //删除要素
            engineEdit_Events.OnDeleteFeature += (objectFeature) =>
            {
                IFeature deleteFeature = objectFeature as IFeature;
                if (deleteFeature.Class.AliasName.ToUpper() == "LPOINT")
                {
                    string id = deleteFeature.OID.ToString();
                    RelatedFeatuesMove(id);
                }
            };
        }
        private void RelatedFeatuesMove(string filter)
        {
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "FeatureID = " + filter + "";
            IFeature fe;

            ILayer annoLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LANNO"))).FirstOrDefault();
            IFeatureClass annoFcl = (annoLayer as IFeatureLayer).FeatureClass;
            int ct=  annoFcl.FeatureCount(qf);
            if (ct == 0)
            {
                 System.Runtime.InteropServices.Marshal.ReleaseComObject(qf);
                 return;
            }
            IFeatureCursor cursor = annoFcl.Update(qf, false);
            while ((fe = cursor.NextFeature()) != null)
            {
                cursor.DeleteFeature();
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(qf);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
           
        }
      
        private void m_Application_WorkspaceOpened(object sender, EventArgs e)
        {
           // feAttrs.Clear();
           // InitalFeAttrs();
        }
        private void InitalFeAttrs()
        {
            if (feAttrs.Count != 0)
                return;
            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LPOINT");
            })).ToArray();
            var pRepLayer = lyrs.First();
            IFeatureClass fcl = (pRepLayer as IFeatureLayer).FeatureClass;
            IFeature fe;
            IFeatureCursor cursor = fcl.Search(null, false);
            while ((fe = cursor.NextFeature()) != null)
            {
                int id = fe.OID;
                string val = fe.get_Value(fe.Fields.FindField("JsonTxt")).ToString();
                feAttrs[id] = val;
            }
        }
        private void engineEdit_Events_OnCreateFeature(IObject Object)
        {
            //IFeature fe = Object as IFeature;
            //if (fe.Class.AliasName != "LPOINT")
            //{
            //    return;
            //}
            //int id = fe.OID;
            //string val = fe.get_Value(fe.Fields.FindField("JsonTxt")).ToString();
            //feAttrs[id] = val;
        }
        private void engineEdit_Events_OnChangeFeature(IObject Object)
        {
           
            
            //IFeature fe = Object as IFeature;
            //if (fe.Class.AliasName != "LPOINT")
            //{
            //    return;
            //}
            //string val = fe.get_Value(fe.Fields.FindField("JsonTxt")).ToString();
            // int id = fe.OID;
            ////判断属性是否改变
            // if (val != feAttrs[id])
            // {
            //    feAttrs[id] = val;

            //    PieJsonHelper.UpdateFeature(fe, val);

            //    m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, m_Application.ActiveView.Extent);
      
            // }
        }

        public override void OnClick()
        {
            
           
        }
        
    }
}