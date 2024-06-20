using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using System.Data;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Maplex;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using System.IO;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.DataManagementTools;
using System.Xml.Linq;
using ESRI.ArcGIS.Geoprocessor;


namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// li:根据内图廓裁切指定要素类（仅保留内图廓范围内的要素）
    /// </summary>
    public class ClipDataByInnerborderCmd : SMGICommand
    {
        public ClipDataByInnerborderCmd()
        {
            m_caption = "内图廓裁切";
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
            List<string> configFCNameList = new List<string>();
            IPolygon clipGeo = null;
            #region 读取配置表
            string fileName = string.Format("{0}\\专家库\\InnerborderClip.xml", m_Application.Template.Root);
            if (!File.Exists(fileName))
            {
                MessageBox.Show(string.Format("没有找到配置文件：{0}", fileName));
                return;
            }
            var ruleDoc = XDocument.Load(fileName);
            var paramsItem = ruleDoc.Element("params");
            if(paramsItem == null || paramsItem.Element("innerborder") == null)
            {
                MessageBox.Show(string.Format("配置文件：{0}结构不正确！", fileName));
                return;
            }
            var innerborderItem = paramsItem.Element("innerborder");
            string fcName = innerborderItem.Attribute("FCName").Value.ToString().ToUpper();
            string sql = innerborderItem.Attribute("SQL").Value.ToString();
            IFeatureLayer innerLayer = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IFeatureLayer && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName;

            })).FirstOrDefault() as IFeatureLayer;
            if(innerLayer == null || innerLayer.FeatureClass == null)
            {
                MessageBox.Show(string.Format("没有找到内图廓所在图层：{0}", fcName));
                return;
            }
            IQueryFilter qf = new QueryFilterClass() { WhereClause = sql };
            IFeatureCursor feCursor = innerLayer.FeatureClass.Search(qf, true);
            IFeature fe = null;
            while ((fe = feCursor.NextFeature()) != null)
            {
                IPolyline shape = fe.Shape as IPolyline;
                if (shape == null || !shape.IsClosed)
                    continue;//非封闭线

                IGeometryCollection geometryCol = shape as IGeometryCollection;

                clipGeo = new PolygonClass();
                for (int i = 0; i < geometryCol.GeometryCount; i++)
                {
                    ISegmentCollection ring = new RingClass();
                    ring.AddSegmentCollection(geometryCol.get_Geometry(i) as ISegmentCollection);//取出Path所以用AddSegmentCollection
                    (clipGeo as IGeometryCollection).AddGeometry(ring as IGeometry);
                }
                break;
            }
            Marshal.ReleaseComObject(feCursor);

            //配置文件中待裁切的要素类
            var items = paramsItem.Element("clipFC").Elements("fcName");
            foreach (XElement ele in items)
            {
                fcName = ele.Value.ToString();

                if (!configFCNameList.Contains(fcName))
                    configFCNameList.Add(fcName);
            }
            #endregion

            Dictionary<string, IFeatureClass> fcName2FC = new Dictionary<string,IFeatureClass>();
            #region 获取可能参与到裁切的要素类集合
            var layers = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer));
            foreach (var lyr in layers)
            {
                IFeatureLayer feLayer = lyr as IFeatureLayer;
                IFeatureClass fc = feLayer.FeatureClass;
                if (fc == null)
                    continue;//空图层

                if ((fc as IDataset).Workspace.PathName != m_Application.Workspace.EsriWorkspace.PathName)
                    continue;//临时数据

                if(!configFCNameList.Contains(fc.AliasName.ToUpper()))
                    continue;//没在配置表中

                if (!fcName2FC.ContainsKey(fc.AliasName))
                {
                    fcName2FC.Add(fc.AliasName, fc);
                }
            }
            #endregion

            if(fcName2FC.Count ==0)
            {
                MessageBox.Show("配置文件中的要素类在当前工作空间中全部没有找到！");
                return ;
            }

            ClipDataByInnerborderForm frm = new ClipDataByInnerborderForm(m_Application, fcName2FC.Keys.ToList());
            if (DialogResult.OK != frm.ShowDialog())
                return;

            m_Application.EngineEditor.StartOperation();
            try
            {
                using (var wo = m_Application.SetBusy())
                {
                    List<IFeatureClass> fcList = new List<IFeatureClass>();
                    foreach (var kv in fcName2FC)
                    {
                        if (frm.FCNameList.Contains(kv.Key))
                            fcList.Add(kv.Value);
                    }

                    clipDataByGeo(fcList, clipGeo, wo);
                }

                m_Application.EngineEditor.StopOperation("内图廓裁切");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                m_Application.EngineEditor.AbortOperation();
            }
        }

        private void clipDataByGeo(List<IFeatureClass> fcList, IPolygon clipGeo, WaitOperation wo = null)
        {
            try
            {
                //裁切
                foreach (var fc in fcList)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在处理要素类【{0}】......", fc.AliasName));

                    IFeature fe = null;
                    IFeatureCursor feCursor = null;
                    

                    ISpatialFilter sf = new SpatialFilterClass();
                    sf.Geometry = clipGeo;
                    #region 要素打断（不能用IFeatureEdit.Split方法，会造成关联注记删除等问题）
                    IPolyline boundary = (clipGeo as ITopologicalOperator).Boundary as IPolyline;
                    if (fc.ShapeType == esriGeometryType.esriGeometryPolyline)
                    {
                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
                        feCursor = fc.Search(sf, true);
                        while ((fe = feCursor.NextFeature()) != null)
                        {
                            IPolyline interLine = (clipGeo as ITopologicalOperator2).Intersect(fe.Shape, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                            if (null == interLine || true == interLine.IsEmpty || 0 == interLine.Length)
                                continue;

                            (interLine as ITopologicalOperator).Simplify();

                            fe.Shape = interLine;
                            fe.Store();
                        }
                        Marshal.ReleaseComObject(feCursor);
                    }
                    else if (fc.ShapeType == esriGeometryType.esriGeometryPolygon)
                    {
                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelOverlaps;
                        feCursor = fc.Search(sf, true);
                        while ((fe = feCursor.NextFeature()) != null)
                        {
                            IPolygon interPolygon = (clipGeo as ITopologicalOperator2).Intersect(fe.Shape, esriGeometryDimension.esriGeometry2Dimension) as IPolygon;
                            if (null == interPolygon || true == interPolygon.IsEmpty || 0 == interPolygon.Length)
                                continue;

                            (interPolygon as ITopologicalOperator).Simplify();

                            fe.Shape = interPolygon;
                            fe.Store();
                        }
                        Marshal.ReleaseComObject(feCursor);
                    }
                    #endregion

                    #region 删除面范围外的要素
                    string relation = "F********";

                    //方法1：结果不正确
                    //sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelRelation;
                    //sf.SpatialRelDescription = relation;
                    //(fc as ITable).DeleteSearchedRows(sf);

                    //方法2：速度较慢些
                    IRelationalOperator ro = clipGeo as IRelationalOperator;
                    feCursor = fc.Search(null, false);
                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        if (ro.Relation(fe.Shape, String.Format("RELATE(G1,G2,'{0}')", relation)))
                        {
                            fe.Delete();
                        }

                        Marshal.ReleaseComObject(fe);
                    }
                    Marshal.ReleaseComObject(feCursor);
                    #endregion
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }
    }
}
