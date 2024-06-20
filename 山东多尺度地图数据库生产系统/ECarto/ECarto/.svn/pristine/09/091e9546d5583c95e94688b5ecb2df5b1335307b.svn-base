using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using System.Xml.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.InteropServices;
using SMGI.Plugin.EmergencyMap.Common;
namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 点击执行后，按住空格下一跳记录，Ctrl上一条记录
    /// </summary>
    public class AnnoConflictTool:SMGI.Common.SMGITool
    {
       
        
        INewPolygonFeedback fb=null;
        IFeatureClass fclAnno = null;
        public AnnoConflictTool()
        {

            currentTool = new ControlsEditingEditToolClass();
          
        }
        /// <summary>
        /// 编辑工具
        /// </summary>
        ControlsEditingEditToolClass currentTool;
        public override bool Enabled{
            get{
                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }
        IActiveView view = null;
        bool annoChecked = false;
        IFeatureClass fclIm = null;
        string shpfileName = string.Empty;
        public List<ILayer> listLyrs = new List<ILayer>();
        AnnoConflictType conflictType = AnnoConflictType.Envelop;
        public override void OnClick(){
            
            annoChecked = false;
            listLyrs.Clear();
          
            currentTool.OnClick();
            view = m_Application.ActiveView;
            (view as IActiveViewEvents_Event).AfterDraw += new IActiveViewEvents_AfterDrawEventHandler(view_AfterDraw);
            var lyrs= m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFDOGraphicsLayer);
            })).ToArray();
            FrmAnnoConflict frm = new FrmAnnoConflict(lyrs);
            if (DialogResult.OK == frm.ShowDialog())
            {
                conflictType = frm.conflictType;
                shpfileName = frm.ShpFile;
                annoChecked=true;
                listLyrs = frm.ListLyrs;
                Process();
            }
            
        }
        private void view_AfterDraw(IDisplay Display, esriViewDrawPhase phase)
        {
            if (fb != null)
            {
                fb.Refresh(0);
            }


        }
        public override bool Deactivate()
        {
            shpfileName = "";
            fb = null;
            if(fclAnno!=null)
            {
                (fclAnno as IDataset).Delete();
            }
            fclAnno = null;
            annoChecked = false;
            dic.Clear();
            listLyrs.Clear();
            //(view as IGraphicsContainer).DeleteAllElements();
            view.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            (view as IActiveViewEvents_Event).AfterDraw -= new IActiveViewEvents_AfterDrawEventHandler(view_AfterDraw);
            currentTool.Deactivate();
            return base.Deactivate();
        }
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);

            currentTool.OnCreate(m_Application.MapControl.Object);
            
        }

        Dictionary<string, IGeometry> dic = new Dictionary<string, IGeometry>();
        Dictionary<string, string> dicInfo = new Dictionary<string, string>();
        private void InialImfeatures()
        {
            if (fclAnno == null)
            {
                fclAnno = CreateImFeatureClass("ImAnno", m_Application.ActiveView.FocusMap.SpatialReference);
            }
            else
            {
                (fclAnno as ITable).DeleteSearchedRows(null);
            }
            IFeatureCursor editCursor = fclAnno.Insert(true);
            IFeatureBuffer fbuffer = fclAnno.CreateFeatureBuffer();
            //获取内图廓面
            //获取内图廓线范围
            string mapBorderLineLayerName = "LLine";
            IEnvelope en = null;
            if ((m_Application.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, mapBorderLineLayerName))
            {
                IFeatureClass mapBorderFeatureClass = (m_Application.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(mapBorderLineLayerName);
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = "TYPE='内图廓'";
                IFeatureCursor pCursor = mapBorderFeatureClass.Search(qf, false);
                IFeature fLine = pCursor.NextFeature();
                IPolyline extentGeo = fLine.ShapeCopy as IPolyline;
                en = extentGeo.Envelope;
                Marshal.ReleaseComObject(pCursor);

            }
          
            
            //构建要素
            foreach (var lyr in listLyrs)
            {
                IFeature fe;
                var fc = (lyr as IFeatureLayer).FeatureClass;
                ISpatialFilter sf = new SpatialFilter();
                sf.Geometry = en;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                sf.WhereClause = "Status = 0 and SELECTSTATE is NULL";
                IFeatureCursor cursor = fc.Search(sf, false);
                while ((fe = cursor.NextFeature()) != null)
                {
                    IAnnotationFeature annoFe = fe as IAnnotationFeature;
                    ITextElement textElement = annoFe.Annotation as ITextElement;
                    IDisplay currentDisplay = m_Application.ActiveView.ScreenDisplay as IDisplay;
                    IPolygon polygon = new PolygonClass();
                    (textElement as IElement).QueryOutline(currentDisplay, polygon);
                    fbuffer.Shape = polygon;
                    fbuffer.set_Value(fclAnno.FindField("LyrName"), lyr.Name);
                    fbuffer.set_Value(fclAnno.FindField("ReID"), fe.OID);
                    editCursor.InsertFeature(fbuffer);
                }
                Marshal.ReleaseComObject(cursor);
                editCursor.Flush();
            }
            Marshal.ReleaseComObject(editCursor);
        }
        public override void OnDblClick()
        {
            currentTool.OnDblClick();
             
        }
        private void DrawElements()
        {
            index = 0;
            (view as IGraphicsContainer).DeleteAllElements();
            ShapeFileWriter resultFile = null;
            foreach (var kv in dic)
            {
                if (kv.Value == null)
                    continue;
                PolygonElementClass polygon = new PolygonElementClass();
                polygon.Geometry =kv.Value;
                polygon.Symbol = new SimpleFillSymbolClass
                {
                    Style = esriSimpleFillStyle.esriSFSNull,
                    Outline = new SimpleLineSymbolClass
                    {
                        Style = esriSimpleLineStyle.esriSLSSolid,
                        Width=1,
                        Color = new RgbColorClass { Red = 200 }
                    }
                };
                (view as IGraphicsContainer).AddElement(polygon, 0);
                if (resultFile == null)
                {
                    //建立结果文件
                    resultFile = new ShapeFileWriter();
                    Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                    fieldName2Len.Add("检查项", 50);
                    fieldName2Len.Add("信息", 50);
                    resultFile.createErrorResutSHPFile(shpfileName, m_Application.ActiveView.FocusMap.SpatialReference, esriGeometryType.esriGeometryPolygon, fieldName2Len);
                }

                Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                fieldName2FieldValue.Add("检查项", "注记冲突检查");
                fieldName2FieldValue.Add("信息", kv.Key);
                resultFile.addErrorGeometry(kv.Value, fieldName2FieldValue);
            }
            //保存结果文件
            if (resultFile != null)
            {
                resultFile.saveErrorResutSHPFile();
            }
            view.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

        }
        public override void OnKeyDown(int keyCode, int shift)
        {

            if (keyCode == 17)//Ctrl键
            {
            }
        }
        int index = 0;
        private void UpdateElement()
        {
            var kv = dic.ElementAt(index);
            IPoint center = (kv.Value as IArea).Centroid;
            var enumes = (m_Application.ActiveView as IGraphicsContainer).LocateElements(center, 1);
            IElement ele= enumes.Next();
            if (ele != null)
            {
                (m_Application.ActiveView as IGraphicsContainerSelect).UnselectAllElements();
                (m_Application.ActiveView as IGraphicsContainerSelect).SelectElement(ele);
            }

        }
        public override void OnKeyUp(int keyCode, int shift)
        {
            switch (keyCode)
            {
                case (int)System.Windows.Forms.Keys.ControlKey://上一条记录
                    if (dic.Count > 0)
                    {
                        index--;
                        KeyValuePair<string,IGeometry> kv;
                        while(true)
                        {
                           if(index<0)
                               index = dic.Count - 1;
                           kv =dic.ElementAt(index);
                           if(kv.Value!=null)
                               break;
                           index--;
                        }

                        IPoint center = (kv.Value as IArea).Centroid;
                        m_Application.MapControl.CenterAt(center);
                        UpdateElement();
                        m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                       

                    }
                    break;
                case (int)System.Windows.Forms.Keys.Space://下一条记录
                    if (dic.Count > 0)
                    {
                        index++;
                        KeyValuePair<string, IGeometry> kv;
                        while (true)
                        {
                            if (index > dic.Count - 1)
                                index = 0;
                            kv = dic.ElementAt(index);
                            if (kv.Value != null)
                                break;
                            index++;
                        }

                        IPoint center = (kv.Value as IArea).Centroid;
                        m_Application.MapControl.CenterAt(center);
                        UpdateElement();
                        m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                       
                    }
                    break;
                case (int)System.Windows.Forms.Keys.Escape://清空
                    break;
                default:
                    break;
            }
        }
        private void Process()
        {
            dic.Clear();
            WaitOperation wo = m_Application.SetBusy();
            try
            {
                InialImfeatures();
                IFeatureCursor fcursor = fclAnno.Search(null, true);
                IFeature feature = null;
                
                //id=>geo
                ISpatialFilter filter = new SpatialFilterClass();
                filter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                while ((feature = fcursor.NextFeature()) != null)
                {
                    string fid = feature.get_Value(fclAnno.FindField("ReID")).ToString();
                    string lyrName = feature.get_Value(fclAnno.FindField("LyrName")).ToString();
                    filter.Geometry = feature.Shape;
                    filter.WhereClause = "ReID <> " + fid;
                    string key1 = string.Format(lyrName + "[{0}]", fid);
                    IFeature fe;
                    IFeatureCursor cursor = fclAnno.Search(filter, false);
                    while ((fe = cursor.NextFeature()) != null)
                    {
                        string id = fe.get_Value(fclAnno.FindField("ReID")).ToString();
                        string lyrName1 = fe.get_Value(fclAnno.FindField("LyrName")).ToString();
                        string key2 = string.Format(lyrName + "[{0}]", id);
                        if (dic.ContainsKey(key1 + "_" + key2))
                        {
                            continue;
                        }

                        if (conflictType == AnnoConflictType.Envelop)
                        {
                            ITopologicalOperator to = feature.ShapeCopy as ITopologicalOperator;
                            to.Clip(fe.Shape.Envelope);
                            dic[key1 + "_" + key2] = to as IGeometry;
                            dicInfo[key1 + "_" + key2] = fid + "_" + id;
                            dic[key2 + "_" + key1] = null;
                            dicInfo[key2 + "_" + key1] = id + "_" + fid;
                        }
                        else
                        {
                            ITopologicalOperator to = feature.ShapeCopy as ITopologicalOperator;
                            IGeometry geo = to.Intersect(fe.ShapeCopy, esriGeometryDimension.esriGeometry2Dimension);
                            if (geo != null && !geo.IsEmpty)
                            {
                                dic[key1 + "_" + key2] = geo as IGeometry;
                                dicInfo[key1 + "_" + key2] = fid + "_" + id;
                                dic[key2 + "_" + key1] = null;
                                dicInfo[key2 + "_" + key1] = id + "_" + fid;
                            }
                        }
                    }
                    Marshal.ReleaseComObject(cursor);
                }
                Marshal.ReleaseComObject(fcursor);
                //DrawElements();
                if (dic.Count > 0)
                {
                    if (System.IO.File.Exists(shpfileName))
                    {
                        //if (MessageBox.Show("是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            IFeatureWorkspace featureWS = GApplication.ShpFactory.OpenFromFile(System.IO.Path.GetDirectoryName(shpfileName), 0) as IFeatureWorkspace;
                            IFeatureClass fc = featureWS.OpenFeatureClass(System.IO.Path.GetFileNameWithoutExtension(shpfileName));
                            IFeatureLayer fLayer = new FeatureLayerClass();
                            fLayer.FeatureClass = fc;
                            fLayer.Name = fc.AliasName;
                            ISimpleRenderer sm = new SimpleRendererClass();
                            sm.Symbol = new SimpleFillSymbolClass
                            {
                                Style = esriSimpleFillStyle.esriSFSNull,
                                Outline = new SimpleLineSymbolClass
                                {
                                    Style = esriSimpleLineStyle.esriSLSSolid,
                                    Width = 1,
                                    Color = new RgbColorClass { Red = 200 }
                                }
                            };
                           // sm.Label = label;
                            (fLayer as IGeoFeatureLayer).Renderer = sm as IFeatureRenderer;
                            m_Application.TOCControl.Update();
                            m_Application.TOCControl.Refresh();
                            //加载临时文件
                            m_Application.ActiveView.FocusMap.AddLayer(fLayer);
                            m_Application.ActiveView.FocusMap.MoveLayer(fLayer, 0);
                        }
                    }
                    else
                    {
                        MessageBox.Show("检查完毕，没有发现冲突要素！");
                    }
                }
                wo.Dispose();
               
            }
            catch(Exception ex)
            {
                MessageBox.Show("处理异常！");
                wo.Dispose();
            }
            MessageBox.Show("处理完成!");
        }
        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            currentTool.OnMouseDown(button, shift, x, y);
            if (!annoChecked)
                return;
            if (button == 1)
            {
                IPoint p = view.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            }
            if (button == 2)
            {
                try
                {
                    //if (System.Windows.Forms.MessageBox.Show("将全图范围进行注记冲突检查，请确认", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                    //{
                    //    Process();
                    //}
                }
                catch (Exception ex)
                {
                }
            }
        }
    
        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            currentTool.OnMouseMove(button, shift, x, y);
            IPoint p = view.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);

            if (fb != null)
            {
                fb.MoveTo(p);
            }
             
          
        }
        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            currentTool.OnMouseUp(button, shift, x, y);
        }

        private IFields CreatLayerAttribute(ISpatialReference sr, esriGeometryType geometryType = esriGeometryType.esriGeometryPolygon)
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
            //图层
            field = new FieldClass();
            fieldEdit = (IFieldEdit)field;
            fieldEdit.Name_2 = "LyrName";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldsEdit.AddField(field);
            //
            field = new FieldClass();
            fieldEdit = (IFieldEdit)field;
            fieldEdit.Name_2 = "ReID";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
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
      
        //创建内存要素类
        private IFeatureClass CreateImFeatureClass(string name, ISpatialReference sr, esriGeometryType geometryType = esriGeometryType.esriGeometryPolygon)
        {
            try
            {
                //IFeatureWorkspace fws =  GApplication.Application.MemoryWorkspace as IFeatureWorkspace;
                string fullPath =CommonMethods.GetAppDataPath() + "\\MyWorkspace.gdb";
                IWorkspace ws = CommonMethods.createTempWorkspace(fullPath);
                //IWorkspace ws = m_Application.MemoryWorkspace;
                IFeatureWorkspace fws = ws as IFeatureWorkspace;
                if ((fws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, name))
                {
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
       
       
    }
}
