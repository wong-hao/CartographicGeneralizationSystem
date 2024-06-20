using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using SMGI.Common;
using stdole;
using System.Drawing;
using NetDesign;

namespace SMGI.Plugin.EmergencyMap
{
    public class TaxRegionColorCmd : SMGI.Common.SMGICommand
    {

        public TaxRegionColorCmd()
        {
            m_caption = "税源普色";
          
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null&&m_Application.EngineEditor.EditState== esriEngineEditState.esriEngineStateEditing;
            }
        }
         
        string field = "NSJE";
        string lyrTax = "SYP";
        string nameField = "NAME";
        public override void OnClick()
        {

            var bouaLyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper().StartsWith("BOUA");
            }));
            if (bouaLyrs.Count() == 0)
            {
                MessageBox.Show("缺少境界图层");
                return;
            }
            var sypLyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper()==("SYP");
            }));
            if (sypLyrs.Count() == 0)
            {
                MessageBox.Show("缺少税源点图层:" + lyrTax);
                return;
            }
            if ((sypLyrs.FirstOrDefault() as IFeatureLayer).FeatureClass.FindField(field) == -1)
            {
                MessageBox.Show("税源点图层缺少字段:" + field);
                return;
            }

            FrmClass frm = new FrmClass(lyrTax, field);
            if (frm.ShowDialog() != DialogResult.OK)
                return;
            Dictionary<string, int> RegionColors = frm.RegionColors;
            Dictionary<int, ICmykColor> ColorsDic = frm.ColorsDic;
            string boua = frm.BOUAName;

            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper().StartsWith("BOUA");
            }));
            foreach (var lyr in lyrs)
            {
                lyr.Visible = false;
            }
            IFeatureLayer bouaLyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && (l as IFeatureLayer).Name == boua;
            })).FirstOrDefault() as IFeatureLayer;
            bouaLyr.Visible = true;
            IGeoFeatureLayer geoFlyr = bouaLyr as IGeoFeatureLayer;
            IMapContext mctx = new MapContextClass();
            mctx.Init(m_Application.ActiveView.FocusMap.SpatialReference, m_Application.ActiveView.FocusMap.ReferenceScale, geoFlyr.AreaOfInterest);
            IRepresentationRenderer repRender = geoFlyr.Renderer as IRepresentationRenderer;
            if (repRender == null)
            {
                MessageBox.Show(geoFlyr.Name+"图层缺少制图表达!");
                return;
            }
            IRepresentationClass rpc = repRender.RepresentationClass;
            IFeatureCursor cursor = bouaLyr.Search(null, false);
            IFeature fe;
            try
            {
                m_Application.EngineEditor.StartOperation();
                while ((fe = cursor.NextFeature()) != null)
                {
                    try
                    {
                        string name = fe.get_Value(fe.Class.FindField(nameField)).ToString();
                        var rep = rpc.GetRepresentation(fe, mctx);
                        if (RegionColors.ContainsKey(name))
                        {
                            IColor pcolor = ColorsDic[RegionColors[name]];
                            OverrideColorValueSet(rep, pcolor);
                        }
                    }
                    catch
                    {
                    }
                }
                m_Application.EngineEditor.StopOperation("");
            }
            catch
            {

                m_Application.EngineEditor.AbortOperation();
            }
            m_Application.ActiveView.Refresh();
            
        }
        public void OverrideColorValueSet(IRepresentation rep, IColor pColor)
        {

            var ruleOrg = rep.RepresentationClass.RepresentationRules.get_Rule(rep.RuleID);
            IBasicFillSymbol fillSym = ruleOrg.get_Layer(0) as IBasicFillSymbol;
            IGraphicAttributes ga = fillSym.FillPattern as IGraphicAttributes;
            if (fillSym != null)
            {
                int id = ga.get_IDByName("Color");
                rep.set_Value(ga, id, pColor);
            }
            rep.RepresentationClass.RepresentationRules.set_Rule(rep.RuleID, ruleOrg);
            rep.UpdateFeature();
            rep.Feature.Store();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(rep);
        }

        
    }
}
