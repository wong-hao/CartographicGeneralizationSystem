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
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Data;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using System.IO;
 
namespace SMGI.Plugin.EmergencyMap.GX
{
  
    public class SDMSideCmd : SMGI.Common.SMGICommand
    {
        Dictionary<string, int> ColorRules = new Dictionary<string,int>();//色带颜色->ruleID；
        IFeatureClass SDMfcl = null;
        public SDMSideCmd()
        {
             
            m_caption = "单侧色带";
            m_category = "单侧色带";
            m_toolTip = "单侧色带";
        }

        public override bool Enabled
        {
            get
            {

                return m_Application != null && m_Application.Workspace != null;
            }
        }

        private IFields CreatLayerAttribute(ISpatialReference sr, esriGeometryType geometryType = esriGeometryType.esriGeometryPolyline)
        {
            //设置字段集
            IFields fields = new FieldsClass();
            var fieldsEdit = (IFieldsEdit)fields;
            IField field = new FieldClass();
            var fieldEdit = (IFieldEdit)field;

            //创建主键
            fieldEdit.Name_2 = "OBJECTID";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
            fieldsEdit.AddField(field);


            //创建图形字段
            IGeometryDef geometryDef = new GeometryDefClass();
            var geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = geometryType;
            geometryDefEdit.SpatialReference_2 = sr;

            field = new FieldClass();
            fieldEdit = (IFieldEdit)field;
            fieldEdit.Name_2 = "SHAPE";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            fieldEdit.GeometryDef_2 = geometryDef;
            fieldsEdit.AddField(field);
            return fields;
        }
        private string GetAppDataPath()
        {
            if (System.Environment.OSVersion.Version.Major <= 5)
            {
                return System.IO.Path.GetFullPath(
                    System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + @"\..");
            }

            var dp = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var di = new System.IO.DirectoryInfo(dp);
            var ds = di.GetDirectories("SMGI");
            if (ds == null || ds.Length == 0)
            {
                var sdi = di.CreateSubdirectory("SMGI");
                return sdi.FullName;
            }
            else
            {
                return ds[0].FullName;
            }
        }

        /// <summary>
        /// 创建临时工作空间
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        private IWorkspace createTempWorkspace(string fullPath)
        {
            IWorkspace pWorkspace = null;
            IWorkspaceFactory2 wsFactory = new FileGDBWorkspaceFactoryClass();

            if (!Directory.Exists(fullPath))
            {

                IWorkspaceName pWorkspaceName = wsFactory.Create(System.IO.Path.GetDirectoryName(fullPath),
                    System.IO.Path.GetFileName(fullPath), null, 0);
                IName pName = (IName)pWorkspaceName;
                pWorkspace = (IWorkspace)pName.Open();
            }
            else
            {
                pWorkspace = wsFactory.OpenFromFile(fullPath, 0);
            }



            return pWorkspace;
        }
        //创建要素类
        private IFeatureClass CreateFeatureClass(string name, ISpatialReference sr, esriGeometryType geometryType = esriGeometryType.esriGeometryPolyline)
        {
            try
            {
                //IFeatureWorkspace fws =  GApplication.Application.MemoryWorkspace as IFeatureWorkspace;
                string fullPath = GetAppDataPath() + "\\MyWorkspace.gdb";
                IWorkspace ws = createTempWorkspace(fullPath);
                IFeatureWorkspace fws = createTempWorkspace(fullPath) as IFeatureWorkspace;
                if ((ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable, name))
                {
                    (fws.OpenTable(name) as IDataset).Delete();
                }
                if ((ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, name))
                {
                    //IFeatureClass fcl = fws.OpenFeatureClass(name);
                    //(fcl as ITable).DeleteSearchedRows(null);
                    //return fcl;
                   (fws.OpenFeatureClass(name) as IDataset).Delete();
                }
                IFields org_fields = CreatLayerAttribute(sr, geometryType);

                return fws.CreateFeatureClass(name, org_fields, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");
            }
            catch
            {
                return null;
            }
        }
       
        public override void OnClick()
        {
            var lys = m_Application.Workspace.LayerManager.GetLayer(l => l.Visible && l is IGeoFeatureLayer && ((IGeoFeatureLayer)l).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline).ToList();
            var fes = new List<IFeature>();
            var fas = (IEnumFeature)m_Application.ActiveView.FocusMap.FeatureSelection;
            IFeature fa;
            while ((fa = fas.Next()) != null)
            {
                if (fa.Shape is IPolyline)
                    fes.Add(fa);
            }
            if (fes.Count == 0)
            {
                MessageBox.Show("请选中线要素！");
                return;
            }
            FrmSideSDM frm = new FrmSideSDM();
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;
            Dictionary<string, ICmykColor> colors = frm.CMYKColors;
            double dis = frm.SMDWidth;
            int count = frm.SMDNum;
            dis =Math.Round(dis / count,3);
            dis = dis * m_Application.ActiveView.FocusMap.ReferenceScale * 1e-3;
            SDMColorSet();
            if (m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
            {
                m_Application.EngineEditor.StartOperation();
            }
            WaitOperation wo = m_Application.SetBusy();
            try
            {
               
                
               IFeatureClass sideSDMfcl = CreateFeatureClass("SideSDM", GApplication.Application.MapControl.SpatialReference);
               IWorkspace ws=(sideSDMfcl as IDataset).Workspace;
                string bufferType = frm.BufferType;
                Dictionary<int, string> dic = new Dictionary<int, string>();
                dic[2] = "外层";
                dic[1] = "内层";
                ICmykColor startColor = colors["外层"];
                ICmykColor endColor = colors["内层"];
                //1.创建临时要素类
                //2.gp缓冲
                foreach (var fe in fes)
                {
                   
                    var feNew=  sideSDMfcl.CreateFeature();
                    feNew.Shape = fe.Shape;
                    feNew.Store();
                    ESRI.ArcGIS.AnalysisTools.Buffer gpBuffer = new ESRI.ArcGIS.AnalysisTools.Buffer();
                    if (bufferType == "DOUBLE")
                    {
                        gpBuffer.line_side = "LEFT";
                       
                        for (int i = count-1; i >= 0; i--)
                        {
                            //切缓冲两头部分
                            gpBuffer.buffer_distance_or_field = (i + 1) * dis + " Meters";
                            gpBuffer.line_end_type = "FLAT";
                            gpBuffer.in_features = ws.PathName + "\\" + sideSDMfcl.AliasName;
                            gpBuffer.out_feature_class = ws.PathName + "\\" + sideSDMfcl.AliasName + "_B";
                            SMGI.Common.Helper.ExecuteGPTool(GApplication.Application.GPTool, gpBuffer, null);
                            var fc = (ws as IFeatureWorkspace).OpenFeatureClass(sideSDMfcl.AliasName + "_B");
                            var cursor = fc.Search(null, false);
                            IGeometry polygon = cursor.NextFeature().ShapeCopy;
                            Marshal.ReleaseComObject(cursor);
                            if (polygon == null)
                            {
                                MessageBox.Show("色带面缓冲出错！");
                                return;
                            }
                            string type = dic[2];
                            IFeature feinner = SDMfcl.CreateFeature();
                            feinner.set_Value(SDMfcl.FindField("TYPE"), type);
                            feinner.set_Value(SDMfcl.FindField("RuleID"), ColorRules[type]);
                            feinner.Shape = polygon as IGeometry;
                            feinner.Store();
                            CmykColorClass color = new CmykColorClass();
                            color.Cyan = (int)(endColor.Cyan - i * (endColor.Cyan - startColor.Cyan) * 1.0 / (count - 1));
                            color.Magenta = (int)(endColor.Magenta - i * (endColor.Magenta - startColor.Magenta) * 1.0 / (count - 1));
                            color.Yellow = (int)(endColor.Yellow - i * (endColor.Yellow - startColor.Yellow) * 1.0 / (count - 1));
                            color.Black = (int)(endColor.Black - i * (endColor.Black - startColor.Black) * 1.0 / (count - 1));
                            OverrideSizeValueSet(feinner, color);
                            (fc as IDataset).Delete();
                        }
                        gpBuffer.line_side = "RIGHT";
                        for (int i = count-1; i >= 0; i--)
                        {
                            //切缓冲两头部分
                            gpBuffer.buffer_distance_or_field = (i + 1) * dis + " Meters";
                            gpBuffer.line_end_type = "FLAT";
                            gpBuffer.in_features = ws.PathName + "\\" + sideSDMfcl.AliasName;
                            gpBuffer.out_feature_class = ws.PathName + "\\" + sideSDMfcl.AliasName + "_B";
                            SMGI.Common.Helper.ExecuteGPTool(GApplication.Application.GPTool, gpBuffer, null);
                            var fc = (ws as IFeatureWorkspace).OpenFeatureClass(sideSDMfcl.AliasName + "_B");
                            var cursor = fc.Search(null, false);
                            IGeometry polygon = cursor.NextFeature().ShapeCopy;
                            Marshal.ReleaseComObject(cursor);
                            if (polygon == null)
                            {
                                MessageBox.Show("色带面缓冲出错！");
                                return;
                            }


                            string type = dic[2];
                            IFeature feinner = SDMfcl.CreateFeature();
                            feinner.set_Value(SDMfcl.FindField("TYPE"), type);
                            feinner.set_Value(SDMfcl.FindField("RuleID"), ColorRules[type]);
                            feinner.Shape = polygon as IGeometry;
                            feinner.Store();
                            CmykColorClass color = new CmykColorClass();
                            color.Cyan = (int)(endColor.Cyan - i * (endColor.Cyan - startColor.Cyan) * 1.0 / (count - 1));
                            color.Magenta = (int)(endColor.Magenta - i * (endColor.Magenta - startColor.Magenta) * 1.0 / (count - 1));
                            color.Yellow = (int)(endColor.Yellow - i * (endColor.Yellow - startColor.Yellow) * 1.0 / (count - 1));
                            color.Black = (int)(endColor.Black - i * (endColor.Black - startColor.Black) * 1.0 / (count - 1));
                            OverrideSizeValueSet(feinner, color);
                            (fc as IDataset).Delete();
                        }
                    }
                    else
                    {
                        gpBuffer.line_side = bufferType;
                        
                        for (int i = count-1; i >= 0; i--)
                        {
                            //切缓冲两头部分
                            gpBuffer.buffer_distance_or_field = (i+1) * dis + " Meters";
                            gpBuffer.line_end_type = "FLAT";
                            gpBuffer.in_features = ws.PathName + "\\" + sideSDMfcl.AliasName;
                            gpBuffer.out_feature_class = ws.PathName + "\\" + sideSDMfcl.AliasName + "_B";
                            SMGI.Common.Helper.ExecuteGPTool(GApplication.Application.GPTool, gpBuffer, null);
                            var fc = (ws as IFeatureWorkspace).OpenFeatureClass(sideSDMfcl.AliasName + "_B");
                            var cursor = fc.Search(null, false);
                            IGeometry polygon = cursor.NextFeature().ShapeCopy;
                            Marshal.ReleaseComObject(cursor);
                            if (polygon == null)
                            {
                                MessageBox.Show("色带面缓冲出错！");
                                return;
                            }


                            string type = dic[2];
                            IFeature feinner = SDMfcl.CreateFeature();
                            feinner.set_Value(SDMfcl.FindField("TYPE"), type);
                            feinner.set_Value(SDMfcl.FindField("RuleID"), ColorRules[type]);
                            feinner.Shape = polygon as IGeometry;
                            feinner.Store();
                            CmykColorClass color = new CmykColorClass();
                            color.Cyan = (int)(endColor.Cyan - i * (endColor.Cyan - startColor.Cyan) * 1.0 / (count-1));
                            color.Magenta = (int)(endColor.Magenta - i * (endColor.Magenta - startColor.Magenta) * 1.0 / (count - 1));
                            color.Yellow = (int)(endColor.Yellow - i * (endColor.Yellow - startColor.Yellow) * 1.0 / (count - 1));
                            color.Black = (int)(endColor.Black - i * (endColor.Black - startColor.Black) * 1.0 / (count - 1));
                            OverrideSizeValueSet(feinner, color);
                            (fc as IDataset).Delete();
                        }
                    }

                }
                m_Application.ActiveView.Refresh();
                if (m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
                {
                    m_Application.EngineEditor.StopOperation("色带面");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                if (m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
                {
                    m_Application.EngineEditor.AbortOperation();
                }
                MessageBox.Show(ex.Message);
            }
            finally
            {             
                GC.Collect();
                wo.Dispose();              
            }
        }
        private void OverrideSizeValueSet(IFeature fe, IColor color)
        {
            var   mctx = new MapContextClass();
            mctx.InitFromDisplay(m_Application.ActiveView.ScreenDisplay.DisplayTransformation);
            ILayer sdmlayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("SDM"))).FirstOrDefault();
            IRepresentationRenderer repRender = (sdmlayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass rpc = repRender.RepresentationClass;
            IRepresentation rep = rpc.GetRepresentation(fe, mctx);

            var ruleOrg = rep.RepresentationClass.RepresentationRules.get_Rule(rep.RuleID);

            for (int k = 0; k < ruleOrg.LayerCount; k++)
            {
                IBasicFillSymbol fillSym = ruleOrg.get_Layer(k) as IBasicFillSymbol;
                if (fillSym != null)
                {
                    IFillPattern fillPattern = fillSym.FillPattern;
                    IGraphicAttributes fillAttrs = fillPattern as IGraphicAttributes;
                    rep.set_Value(fillAttrs, 0, color);

                }
            }
            rep.RepresentationClass.RepresentationRules.set_Rule(rep.RuleID, ruleOrg);
            rep.UpdateFeature();
            rep.Feature.Store();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(rep);
        }
        /// <summary>
        /// CMYK字符串得到CMYK颜色值
        /// </summary>
        /// <param name="cmyk">cmyk字符串（形如：C100M200Y100K50）</param>
        /// <returns>CMYK颜色值</returns>
        private IColor GetColorByString(string cmyk)
        {
            char[] D = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            StringBuilder sb = new StringBuilder();
            //新建一个CMYK颜色，然后各项值付为0
            ICmykColor CMYK_Color = new CmykColorClass();
            CMYK_Color.Cyan = 0;
            CMYK_Color.Magenta = 0;
            CMYK_Color.Yellow = 0;
            CMYK_Color.Black = 0;
            try
            {
                for (int i = 0; i <= cmyk.Length; i++)
                {
                    if (i == cmyk.Length)
                    {
                        string sbs = sb.ToString();
                        if (sbs.Contains('C'))
                        {
                            CMYK_Color.Cyan = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('M'))
                        {
                            CMYK_Color.Magenta = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('Y'))
                        {
                            CMYK_Color.Yellow = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('K'))
                        {
                            CMYK_Color.Black = int.Parse(sbs.Substring(1));
                        }
                        break;
                    }
                    else
                    {
                        char C = cmyk[i];
                        if (D.Contains(C))
                        {
                            sb.Append(C);
                        }
                        else
                        {
                            string sbs = sb.ToString();
                            if (sbs.Contains('C'))
                            {
                                CMYK_Color.Cyan = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('M'))
                            {
                                CMYK_Color.Magenta = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('Y'))
                            {
                                CMYK_Color.Yellow = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('K'))
                            {
                                CMYK_Color.Black = int.Parse(sbs.Substring(1));
                            }
                            sb.Clear();
                            sb.Append(C);
                        }
                    }
                }
                return CMYK_Color;
            }
            catch
            {
                return null;
            }

        }
        
        
        private IGeometry CreateBufferGeo(IGeometry polygon,double dis)
        {
            IGeometry buffer = null;
            try
            {
                //IPolyline boundary = (polygon as ITopologicalOperator).Boundary as IPolyline;
                buffer = (polygon as ITopologicalOperator).Buffer(dis);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                buffer = CreateBufferEffect(polygon, dis);
            }
            return buffer;
        }
        private IGeometry CreateBufferEffect(IGeometry polygon,double dis)
        {
            
            var helper = new GeometricEffectBufferClass();
            IGraphicAttributes attrs = helper as IGraphicAttributes;
            for (int i = 0; i < attrs.GraphicAttributeCount; i++)
            {
                int attrid = attrs.get_ID(i);
                string name = attrs.get_Name(attrid);

            }
            helper.set_Value(0, dis);
            helper.Reset(polygon);
            while (true)
            {
                var g = helper.NextGeometry();
                if (g == null)
                    break;
                return g;
            }
            return null;
        }
        //生成内存图层
        public IFeatureClass CreatePolygonMemoryLayer(ISpatialReference sp)
        {
            //设置字段集
            IFields fields = new FieldsClass();
            var fieldsEdit = (IFieldsEdit)fields;
            IField field = new FieldClass();
            var fieldEdit = (IFieldEdit)field;

            //创建主键
            fieldEdit.Name_2 = "FID";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
            fieldsEdit.AddField(field);

            //创建图形字段
            IGeometryDef geometryDef = new GeometryDefClass();
            var geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;
            geometryDefEdit.SpatialReference_2 = sp;

            field = new FieldClass();
            fieldEdit = (IFieldEdit)field;
            fieldEdit.Name_2 = "Shape";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            fieldEdit.GeometryDef_2 = geometryDef;
            fieldsEdit.AddField(field);

            IWorkspaceFactory wf = new InMemoryWorkspaceFactoryClass();
            var wn = wf.Create("", "MemoryWorkspace", null, 0);
            var na = (IName)wn;
            var fw = (IFeatureWorkspace)(na.Open()); //打开内存空间
            var featureClass = fw.CreateFeatureClass("MW_LineConsPolygon", fields, null, null, esriFeatureType.esriFTSimple, "Shape", "");
            return featureClass;
        }
       
        private void ClearLayer()
        {
            IFeature fe;
            IFeatureCursor cursor = SDMfcl.Update(null, false);
            while ((fe = cursor.NextFeature()) != null)
            {
                cursor.DeleteFeature();
            }
            Marshal.ReleaseComObject(cursor);
        }
        private void SDMColorSet()
        {
          
            ILayer sdmlayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("SDM"))).FirstOrDefault();
            SDMfcl = (sdmlayer as IFeatureLayer).FeatureClass;
            IRepresentationWorkspaceExtension pRepWksExt = m_Application.Workspace.RepersentationWorkspaceExtension;
            IEnumDatasetName enumDatasetName = pRepWksExt.get_FeatureClassRepresentationNames(SDMfcl);
            enumDatasetName.Reset();
            IDatasetName pDatasetName = enumDatasetName.Next();
            IRepresentationClass g_RepClass = pRepWksExt.OpenRepresentationClass(pDatasetName.Name);
            //修改sdm图层 rule的颜色
            IRepresentationRules rules = g_RepClass.RepresentationRules;
            rules.Reset();
            IRepresentationRule rule = null;
            int ruleID;
            while (true)
            {
                rules.Next(out ruleID, out rule);
                if (rule == null) break;
                if (rules.get_Name(ruleID) != "不显示要素")
                {
                    //修改颜色
                    string rulename = rules.get_Name(ruleID);
                    ColorRules[rulename] = ruleID;

                }
            }
           
        }

        private ESRI.ArcGIS.Geodatabase.IRepresentationWorkspaceExtension RepresentationWorkspaceExtensionClass()
        {
            throw new NotImplementedException();
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
