using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using SMGI.Common;
using System.Data;
 
namespace SMGI.Plugin.EmergencyMap
{
    public class BufferQColorCmdXJ : SMGI.Common.SMGICommand
    {
        Dictionary<string, int> ColorRules = new Dictionary<string,int>();//色带颜色->ruleID；
        IFeatureClass QJMfcl = null;
        string fclName = "QJA";
        public BufferQColorCmdXJ()
        {
             
            m_caption = "缓冲骑界";
            m_category = "缓冲骑界";
            m_toolTip = "缓冲骑界";
        }

        public override bool Enabled
        {
            get
            {
               
                return m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateNotEditing; ;
            }
        }
        //BOUA的ruleid->新的BOUA的ruleID
        Dictionary<int, int> ruleBouAIDs = new Dictionary<int, int>();
        //BOUA的ruleid->骑界的ruleID
        Dictionary<int, int> ruleQBouAIDs = new Dictionary<int, int>();
        string bouaName = "BOUA";

        private IColor GetOverrideColor(IRepresentation rep)
        {
            var ruleOrg = rep.RepresentationClass.RepresentationRules.get_Rule(rep.RuleID);
            IBasicFillSymbol fillSym = ruleOrg.get_Layer(0) as IBasicFillSymbol;
            IGraphicAttributes ga = fillSym.FillPattern as IGraphicAttributes;
            IColor pColor = null;
            if (fillSym != null)
            {
                if (ga.ClassName == "SolidColorPattern")
                {
                    int id = ga.get_IDByName("Color");
                    pColor= rep.get_Value(ga, id) as IColor;
                }

            }
            return pColor;
        }
        public void OverrideColorValueSet(IRepresentation rep, IColor pColor)
        {

            var ruleOrg = rep.RepresentationClass.RepresentationRules.get_Rule(rep.RuleID);
            IBasicFillSymbol fillSym = ruleOrg.get_Layer(0) as IBasicFillSymbol;
            IGraphicAttributes ga = fillSym.FillPattern as IGraphicAttributes;
            if (fillSym != null)
            {
                if (ga.ClassName == "SolidColorPattern")
                {
                    int id = ga.get_IDByName("Color");
                    rep.set_Value(ga, id, pColor);
                }
                if (ga.ClassName == "GradientPattern")
                {
                    int id1 = ga.get_IDByName("Color1");
                    rep.set_Value(ga, id1, pColor);
                    int id2 = ga.get_IDByName("Color2");
                    rep.set_Value(ga, id2, pColor);
                }

            }
            rep.RepresentationClass.RepresentationRules.set_Rule(rep.RuleID, ruleOrg);
            rep.UpdateFeature();
            rep.Feature.Store();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(rep);
        }
        private IRepresentationRenderer SwitchRender(IGeoFeatureLayer geoFlyr)
        {
            IFeatureClass fc = geoFlyr.FeatureClass;
            IRepresentationRenderer repRenderer = new RepresentationRendererClass();
            IRepresentationWorkspaceExtension pRepWksExt = m_Application.Workspace.RepersentationWorkspaceExtension;
            bool hasRep = pRepWksExt.get_FeatureClassHasRepresentations(fc);
            if (hasRep)
            {
                IEnumDatasetName enumDatasetName = pRepWksExt.get_FeatureClassRepresentationNames(fc);
                enumDatasetName.Reset();
                IDatasetName pDatasetName = enumDatasetName.Next();
                if (pDatasetName != null)
                {
                    repRenderer.RepresentationClass = pRepWksExt.OpenRepresentationClass(pDatasetName.Name);
                }
                geoFlyr.Renderer = repRenderer as IFeatureRenderer;
                return repRenderer;
            }
            else
            {


                MessageBox.Show("请先设置图层为制图表达!");
                return null;
            }
        }
        public override void OnClick()
        {


            FrmQBOUA frm = new FrmQBOUA("XJ");
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;
            
            WaitOperation wo = m_Application.SetBusy();
            try
            {
                string sqlText = frm.SQL;
                double dis = frm.BufferDis;
                bouaName = frm.LyrName;
                //BOUA的RuldID ->对应的颜色
                Dictionary<int, ICmykColor> colors = GetBOUARuleColor();
                if (colors.Count == 0)
                {
                    MessageBox.Show("所选境界未普色!");
                    return;
                }
                dis = dis * m_Application.ActiveView.FocusMap.ReferenceScale * 1e-3;
                QColorSet(colors);

                wo.SetText("正在生成骑界....");
                ClearLayer();
                IQueryFilter qf = null;
                if (sqlText != "")
                {
                    qf = new QueryFilterClass();
                    qf.WhereClause = sqlText;
                }
                ILayer boualayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && (l.Name.ToUpper() == (bouaName))).FirstOrDefault();
                IGeoFeatureLayer geoFlyr = boualayer as IGeoFeatureLayer;
                IRepresentationRenderer repRender = geoFlyr.Renderer as IRepresentationRenderer;
                if (repRender == null)
                {
                    repRender = SwitchRender(geoFlyr);
                }
                ILayer sdmlayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == (fclName))).FirstOrDefault();
                IGeoFeatureLayer geoQFlyr = sdmlayer as IGeoFeatureLayer;
                IRepresentationRenderer repQRender = geoQFlyr.Renderer as IRepresentationRenderer;
                if (repQRender == null)
                {
                    repQRender = SwitchRender(geoQFlyr);
                }
                
                
                IMapContext mctx = new MapContextClass();
                mctx.Init(m_Application.ActiveView.FocusMap.SpatialReference, m_Application.ActiveView.FocusMap.ReferenceScale, geoFlyr.AreaOfInterest);
             
                int ct = (boualayer as IFeatureLayer).FeatureClass.FeatureCount(qf);
                IFeature fe;
                IFeatureCursor fcursor = (boualayer as IFeatureLayer).FeatureClass.Search(qf, false);
                int Ridnex = (boualayer as IFeatureLayer).FeatureClass.FindField("RULEID");
                int flag = 0;
                while ((fe = fcursor.NextFeature()) != null)
                {
                    string msg = string.Format("{0}/{1}", flag++, ct);
                    wo.SetText("正在生成骑界...." + msg);
                    var rep = repRender.RepresentationClass.GetRepresentation(fe, mctx);
                    if (rep == null)
                        continue;
                    IColor colorOrg = GetOverrideColor(rep);
                    if (colorOrg == null)
                        continue;
                    ICmykColor cmyk = new CmykColorClass { CMYK = colorOrg.CMYK };

                    //生成内层
                    int ruleid = int.Parse(fe.get_Value(Ridnex).ToString());
                    if (!ruleQBouAIDs.ContainsKey(ruleid))//未普色
                    {
                        continue;
                    }
                    IGeometry clipGeo = fe.ShapeCopy;
                    IGeometry bufferGeo = null;
                    IGeometry inner = (clipGeo as ITopologicalOperator).Buffer(-dis);
                    bufferGeo = (inner as IClone).Clone() as IGeometry;
                    inner = (clipGeo as ITopologicalOperator).Difference(inner);

                    IFeature feinner = QJMfcl.CreateFeature();
                    feinner.set_Value(QJMfcl.FindField("TYPE"), "骑界");
                    feinner.set_Value(QJMfcl.FindField("RuleID"), ruleQBouAIDs[ruleid]);
                    feinner.Shape = inner;
                    feinner.Store();
                    {
                        int c = cmyk.Cyan;
                        c = c * 2 > 100 ? 100 : c * 2;
                        int y = cmyk.Yellow;
                        y = y * 2 > 100 ? 100 : y * 2;
                        int m = cmyk.Magenta;
                        m = m * 2 > 100 ? 100 : m * 2;
                        ICmykColor cmycolor = new CmykColorClass();
                        cmycolor.Cyan = c;
                        cmycolor.Black = 0;
                        cmycolor.Yellow = y;
                        cmycolor.Magenta = m;
                        OverrideColorValueSet(repQRender.RepresentationClass.GetRepresentation(feinner, mctx), cmycolor);
                    }
                   
                    feinner = QJMfcl.CreateFeature();
                    feinner.set_Value(QJMfcl.FindField("TYPE"), "境界");
                    feinner.set_Value(QJMfcl.FindField("RuleID"), ruleBouAIDs[ruleid]);
                    feinner.Shape = bufferGeo;
                    feinner.Store();
                    OverrideColorValueSet(repQRender.RepresentationClass.GetRepresentation(feinner, mctx), colorOrg);

                    Marshal.ReleaseComObject(inner);
                    Marshal.ReleaseComObject(bufferGeo);
                    Marshal.ReleaseComObject(clipGeo);
                    Marshal.ReleaseComObject(fe);

                }
                Marshal.ReleaseComObject(fcursor);
                GC.Collect();
                m_Application.ActiveView.Refresh();
                wo.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                wo.Dispose();

                MessageBox.Show(ex.Message);
            }
           

        }
        private void ClearLayer()
        {
            IFeature fe;
            IFeatureCursor cursor = QJMfcl.Update(null, false);
            while ((fe = cursor.NextFeature()) != null)
            {
                cursor.DeleteFeature();
            }
            Marshal.ReleaseComObject(cursor);
        }

        private Dictionary<int, ICmykColor> GetBOUARuleColor()
        {
            Dictionary<int, ICmykColor> colorDics = new Dictionary<int, ICmykColor>();
            ILayer layer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && (l.Name.ToUpper() == (bouaName))).FirstOrDefault();       
            //修改sdm图层 rule的颜色
            var rp = (layer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            IRepresentationRules rules = m_RepClass.RepresentationRules;
            rules.Reset();
            IRepresentationRule rule = null;
            int ruleID;
            while (true)
            {
                rules.Next(out ruleID, out rule);
                if (rule == null) break;
                if (rules.get_Name(ruleID) != "不显示要素")
                {
                    IBasicFillSymbol fillSym = rule.get_Layer(0) as IBasicFillSymbol;
                     
                    IFillPattern fillPattern = fillSym.FillPattern;
                    IGraphicAttributes fillAttrs = fillPattern as IGraphicAttributes;
                    IColor pcolor=fillAttrs.get_Value(0) as IColor;
                    System.Drawing.Color c = ColorHelper.ConvertIColorToColor(pcolor);
                    ICmykColor cmyk = ColorHelper.ConvertColorToCMYK(c);
                    colorDics[ruleID] = cmyk;
                 
                }
            }
            return colorDics;
        }
        private void QColorSet(Dictionary<int, ICmykColor> colors)
        {
            ruleBouAIDs.Clear();
            ruleQBouAIDs.Clear();
            List<ICmykColor> colorsList = new List<ICmykColor>();
            List<int> rulesList = new List<int>();
            //BOUA的ruleid->新boua的ruleID
            foreach (var kv in colors)
            {
                #region
                ICmykColor cmyk = (kv.Value as IClone).Clone() as ICmykColor;
                colorsList.Add(cmyk);
                int c = cmyk.Cyan;
                c = c * 2 > 100 ? 100 : c * 2;
                int y = cmyk.Yellow;
                y = y * 2 > 100 ? 100 : y * 2;
                int m = cmyk.Magenta;
                m = m * 2 > 100 ? 100 : m * 2;
                ICmykColor cmycolor=new CmykColorClass();
                cmycolor.Cyan=c;
                cmycolor.Black=0;
                cmycolor.Yellow=y;
                cmycolor.Magenta=m;
                colorsList.Add(cmycolor);
                rulesList.Add(kv.Key);
                rulesList.Add(kv.Key);
                #endregion
            }
            ILayer sdmlayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == (fclName))).FirstOrDefault();
            QJMfcl = (sdmlayer as IFeatureLayer).FeatureClass;
            //修改sdm图层 rule的颜色
            
            var rp = (sdmlayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            IRepresentationRules rules = m_RepClass.RepresentationRules;
         
            rules.Reset();
            IRepresentationRule rule = null;
            int ruleID;
            int index = 0;
            while (true)
            {
                rules.Next(out ruleID, out rule);
                if (rule == null) break;

                if (index >= colorsList.Count())
                    break;

                //修改颜色
                IBasicFillSymbol fillSym = rule.get_Layer(0) as IBasicFillSymbol;
                if (fillSym != null)
                {   
                    IFillPattern fillPattern = fillSym.FillPattern;
                    IGraphicAttributes fillAttrs = fillPattern as IGraphicAttributes;
                    fillAttrs.set_Value(0, colorsList[index]);
                }
                if (index % 2 == 0)//boua
                {
                    ruleBouAIDs[rulesList[index]] = ruleID;
                }
                if (index % 2 == 1)//骑界
                {
                    ruleQBouAIDs[rulesList[index]] = ruleID;
                }
                index++;
            }
            m_RepClass.RepresentationRules = rules;
           
        }
        private IRepresentationWorkspaceExtension GetRepersentationWorkspace(IWorkspace workspace)
        {
            IWorkspaceExtensionManager wem = workspace as IWorkspaceExtensionManager;
            UID uid = new UIDClass();
            uid.Value = "{FD05270A-8E0B-4823-9DEE-F149347C32B6}";
            IRepresentationWorkspaceExtension rwe = wem.FindExtension(uid) as IRepresentationWorkspaceExtension;
            return rwe;
        }

       }
}
