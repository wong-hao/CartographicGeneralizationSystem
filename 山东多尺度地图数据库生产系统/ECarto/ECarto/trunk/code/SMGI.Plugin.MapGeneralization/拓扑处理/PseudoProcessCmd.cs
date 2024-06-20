using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.BaseClasses;
using System.Data;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geoprocessing;
using System.Collections;
namespace SMGI.Plugin.MapGeneralization
{
    public class PseudoProcessCmd : SMGI.Common.SMGICommand
    {
        public PseudoProcessCmd()
        {
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public override void OnClick()
        {
            var layerSelector =   new LayerSelectWithFiedsForm(m_Application);
            layerSelector.GeoTypeFilter = esriGeometryType.esriGeometryPolyline;
            if (layerSelector.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            IFeatureLayer inputFC = layerSelector.pSelectLayer as IFeatureLayer;
            var gp = m_Application.GPTool;

            //打开计时器
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();



            // using (var wo = m_Application.SetBusy())
            //   {
            //ProcessPseudo(m_Application.Workspace.EsriWorkspace, inputFC, layerSelector.FieldArray);
            ProcessPseudo2(m_Application.Workspace.EsriWorkspace, inputFC, layerSelector.FieldArray);
            //  }


            watch.Stop();
            MessageBox.Show("处理完成!\r\n" + "处理耗时：" + watch.Elapsed.Hours + ":" + watch.Elapsed.Minutes + ":" + watch.Elapsed.Seconds);
        }

        private void ProcessPseudo(IWorkspace ws, IFeatureLayer lyr, ArrayList fieldArray)
        {
            bool isEditing = m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing ? true : false;

            if (isEditing)
            {
                m_Application.EngineEditor.StartOperation();
            }

            ITopologicalOperator topo;
            while (true)
            {
                int nCount = 0;
                IFeatureCursor pFeatCursor = lyr.FeatureClass.Search(null, false);
                IFeature pFeature = null;
                ISpatialFilter pFilter = new SpatialFilterClass();
                pFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                pFilter.WhereClause = (lyr as IFeatureLayerDefinition).DefinitionExpression;
                pFilter.GeometryField = lyr.FeatureClass.ShapeFieldName;
                List<int> deleteIDs = new List<int>();
                while ((pFeature = pFeatCursor.NextFeature()) != null)
                {
                    try
                    {
                        if (deleteIDs.Contains(pFeature.OID))
                        {
                            continue;
                        }

                        IPolyline CheckLine = pFeature.Shape as IPolyline;
                        if (CheckLine.Length > 0)
                        {
                            List<IGeometry> geoList = new List<IGeometry>();
                            ////  起点

                            topo = CheckLine.FromPoint as ITopologicalOperator;
                            topo.Simplify();
                            pFilter.Geometry = CheckLine.FromPoint;

                            int FeatureCount = lyr.FeatureClass.FeatureCount(pFilter);
                            if (FeatureCount == 2)
                            {
                                IFeatureCursor pSelectCursor = lyr.Search(pFilter, true);
                                IFeature SelectFeature = null;
                                while ((SelectFeature = pSelectCursor.NextFeature()) != null)
                                {
                                    if (pFeature.OID != SelectFeature.OID)
                                    {
                                        IPolyline Selectline = SelectFeature.Shape as IPolyline;
                                        ESRI.ArcGIS.Geometry.IPoint Selectfrom = Selectline.FromPoint;
                                        ESRI.ArcGIS.Geometry.IPoint Selectto = Selectline.ToPoint;

                                        IProximityOperator ProxiOP = (pFeature.Shape) as IProximityOperator;
                                        if (ProxiOP.ReturnDistance(Selectfrom) < 0.0001 || ProxiOP.ReturnDistance(Selectto) < 0.0001)
                                        {
                                            bool judge = false;
                                            for (int i = 0; i < fieldArray.Count; i++)
                                            {
                                                int FieldIndex = lyr.FeatureClass.FindField(fieldArray[i].ToString());
                                                if (FieldIndex != -1)
                                                {
                                                    string psfeature = pFeature.get_Value(FieldIndex).ToString().Trim();
                                                    string psecfeature = SelectFeature.get_Value(FieldIndex).ToString().Trim();
                                                    if (psfeature != psecfeature)
                                                    {
                                                        judge = true;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (judge == false)//属性相同
                                            {
                                                nCount++;
                                                geoList.Add(SelectFeature.ShapeCopy);
                                                deleteIDs.Add(SelectFeature.OID);
                                                SelectFeature.Delete();
                                            }
                                        }
                                    }
                                }
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(pSelectCursor);
                            }

                            //// 终点
                            topo = CheckLine.ToPoint as ITopologicalOperator;
                            topo.Simplify();
                            pFilter.Geometry = CheckLine.ToPoint;

                            FeatureCount = lyr.FeatureClass.FeatureCount(pFilter);
                            if (FeatureCount == 2)
                            {
                                IFeatureCursor pSelectCursor = lyr.Search(pFilter, true);
                                IFeature SelectFeature = null;
                                while ((SelectFeature = pSelectCursor.NextFeature()) != null)
                                {

                                    if (pFeature.OID != SelectFeature.OID)
                                    {
                                        IPolyline Selectline = SelectFeature.Shape as IPolyline;
                                        ESRI.ArcGIS.Geometry.IPoint Selectfrom = Selectline.FromPoint;
                                        ESRI.ArcGIS.Geometry.IPoint Selectto = Selectline.ToPoint;

                                        IProximityOperator ProxiOP = (pFeature.Shape) as IProximityOperator;
                                        if (ProxiOP.ReturnDistance(Selectfrom) < 0.0001 || ProxiOP.ReturnDistance(Selectto) < 0.0001)
                                        {
                                            bool judge = false;
                                            for (int i = 0; i < fieldArray.Count; i++)
                                            {
                                                int FieldIndex = lyr.FeatureClass.FindField(fieldArray[i].ToString());
                                                if (FieldIndex != -1)
                                                {
                                                    string psfeature = pFeature.get_Value(FieldIndex).ToString().Trim();
                                                    string psecfeature = SelectFeature.get_Value(FieldIndex).ToString().Trim();
                                                    if (psfeature != psecfeature)
                                                    {
                                                        judge = true;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (judge == false && !deleteIDs.Contains(SelectFeature.OID))//属性相同
                                            {
                                                nCount++;
                                                geoList.Add(SelectFeature.ShapeCopy);
                                                deleteIDs.Add(SelectFeature.OID);
                                                SelectFeature.Delete();
                                            }
                                        }
                                    }

                                }
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(pSelectCursor);
                            }

                            if (geoList.Count > 0)
                            {
                                foreach (var geo in geoList)
                                {
                                    pFeature.Shape = (pFeature.Shape as ITopologicalOperator).Union(geo);
                                }
                                pFeature.Store();
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        if (isEditing)
                        {
                            MessageBox.Show(ex.Message);

                            m_Application.EngineEditor.AbortOperation();
                        }
                    }

                }
                Marshal.ReleaseComObject(pFeatCursor);
                if (nCount == 0)
                {
                    break;
                }
            }

            if (isEditing)
            {
                m_Application.EngineEditor.StopOperation("伪节点处理");
            }

        }


        //协同模式中，由于删除要素没有物理删除，若删除要素也参与到伪节点处理中，则将会造成死循环，需注意应在显示当前模式中进行
        private void ProcessPseudo2(IWorkspace ws, IFeatureLayer lyr, ArrayList fieldArray)
        {
            Process pro = new Process();
            bool isEditing = m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            if (!isEditing)
            {
                MessageBox.Show("请先开启编辑！");
                return;
            }

            if (isEditing)
            {
                m_Application.EngineEditor.StartOperation();
            }
            pro.Show();
            try
            {
                List<int> deleteIDs = new List<int>();
                List<IGeometry> geoList = new List<IGeometry>();
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = (lyr as IFeatureLayerDefinition).DefinitionExpression;
                IFeatureCursor pFeatCursor = lyr.FeatureClass.Search(qf, false);
                IFeature pFeature = pFeatCursor.NextFeature();
                while (pFeature != null)
                {
                    pro.label1.Text = "正在处理要素" + pFeature.OID;
                    if (deleteIDs.Contains(pFeature.OID))
                    {
                        pFeature = pFeatCursor.NextFeature();
                        continue;
                    }

                    IPolyline CheckLine = pFeature.Shape as IPolyline;
                    if (CheckLine.Length == 0)
                    {
                        pFeature = pFeatCursor.NextFeature();
                        continue;
                    }

                    geoList.Clear();
                    ITopologicalOperator topo;

                    ISpatialFilter pFilter = new SpatialFilterClass();
                    pFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    pFilter.WhereClause = (lyr as IFeatureLayerDefinition).DefinitionExpression;
                    pFilter.GeometryField = lyr.FeatureClass.ShapeFieldName;

                    #region 起点
                    topo = CheckLine.FromPoint as ITopologicalOperator;
                    topo.Simplify();
                    pFilter.Geometry = CheckLine.FromPoint;

                    int FeatureCount = lyr.FeatureClass.FeatureCount(pFilter);
                    if (FeatureCount == 2)
                    {
                        IFeatureCursor pSelectCursor = lyr.FeatureClass.Search(pFilter, false);
                        IFeature SelectFeature = null;
                        while ((SelectFeature = pSelectCursor.NextFeature()) != null)
                        {
                            if (deleteIDs.Contains(SelectFeature.OID))
                            {
                                continue;
                            }

                            if (pFeature.OID != SelectFeature.OID)
                            {
                                IPolyline Selectline = SelectFeature.Shape as IPolyline;
                                ESRI.ArcGIS.Geometry.IPoint Selectfrom = Selectline.FromPoint;
                                ESRI.ArcGIS.Geometry.IPoint Selectto = Selectline.ToPoint;

                                IProximityOperator ProxiOP = (pFeature.Shape) as IProximityOperator;
                                if (ProxiOP.ReturnDistance(Selectfrom) < 0.0001 || ProxiOP.ReturnDistance(Selectto) < 0.0001)
                                {
                                    bool judge = false;
                                    for (int i = 0; i < fieldArray.Count; i++)
                                    {
                                        int FieldIndex = lyr.FeatureClass.FindField(fieldArray[i].ToString());
                                        if (FieldIndex != -1)
                                        {
                                            string psfeature = pFeature.get_Value(FieldIndex).ToString().Trim();
                                            string psecfeature = SelectFeature.get_Value(FieldIndex).ToString().Trim();
                                            if (psfeature != psecfeature)
                                            {
                                                judge = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (judge == false)//属性相同
                                    {
                                        deleteIDs.Add(SelectFeature.OID);
                                        geoList.Add(SelectFeature.ShapeCopy);
                                        SelectFeature.Delete();
                                    }
                                }
                            }
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pSelectCursor);
                    }

                    #endregion

                    #region 终点
                    topo = CheckLine.ToPoint as ITopologicalOperator;
                    topo.Simplify();
                    pFilter.Geometry = CheckLine.ToPoint;

                    FeatureCount = lyr.FeatureClass.FeatureCount(pFilter);
                    if (FeatureCount == 2)
                    {
                        IFeatureCursor pSelectCursor = lyr.FeatureClass.Search(pFilter, false);
                        IFeature SelectFeature = null;
                        while ((SelectFeature = pSelectCursor.NextFeature()) != null)
                        {
                            if (deleteIDs.Contains(SelectFeature.OID))
                            {
                                continue;
                            }

                            if (pFeature.OID != SelectFeature.OID)
                            {
                                IPolyline Selectline = SelectFeature.Shape as IPolyline;
                                ESRI.ArcGIS.Geometry.IPoint Selectfrom = Selectline.FromPoint;
                                ESRI.ArcGIS.Geometry.IPoint Selectto = Selectline.ToPoint;

                                IProximityOperator ProxiOP = (pFeature.Shape) as IProximityOperator;
                                if (ProxiOP.ReturnDistance(Selectfrom) < 0.0001 || ProxiOP.ReturnDistance(Selectto) < 0.0001)
                                {
                                    bool judge = false;
                                    for (int i = 0; i < fieldArray.Count; i++)
                                    {
                                        int FieldIndex = lyr.FeatureClass.FindField(fieldArray[i].ToString());
                                        if (FieldIndex != -1)
                                        {
                                            string psfeature = pFeature.get_Value(FieldIndex).ToString().Trim();
                                            string psecfeature = SelectFeature.get_Value(FieldIndex).ToString().Trim();
                                            if (psfeature != psecfeature)
                                            {
                                                judge = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (judge == false)//属性相同
                                    {
                                        deleteIDs.Add(SelectFeature.OID);
                                        geoList.Add(SelectFeature.ShapeCopy);
                                        SelectFeature.Delete();
                                    }
                                }
                            }

                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pSelectCursor);
                    }
                    #endregion

                    if (geoList.Count > 0)
                    {
                        foreach (var geo in geoList)
                        {
                            pFeature.Shape = (pFeature.Shape as ITopologicalOperator).Union(geo);
                        }
                        pFeature.Store();

                        continue;//新的要素，继续判断
                    }
                    else
                    {
                        pFeature = pFeatCursor.NextFeature();
                    }
                    
                }
                Marshal.ReleaseComObject(pFeatCursor);


            }
            catch (Exception ex)
            {
                if (isEditing)
                {
                    MessageBox.Show(ex.Message);

                    m_Application.EngineEditor.AbortOperation();
                }
            }

            if (isEditing)
            {
                m_Application.EngineEditor.StopOperation("伪节点处理");
            }
            pro.Close();
        }

    }
}
