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
using ESRI.ArcGIS.DataSourcesFile;
namespace SMGI.Plugin.MapGeneralization
{
    public class PseudoProcessBigdata : SMGI.Common.SMGICommand
    {
        string refName;
        Dictionary<string, string> Grid = new Dictionary<string, string>();
        List<string> tempName;
        Dictionary<int, string> fileds;
        double tolerance = 0.0001;
        private IGeometry _rangeGeometry;
        public PseudoProcessBigdata()
        {
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }
        public override void OnClick()
        {
            var layerSelector = new LayerSelectWithFiedsForm(m_Application);
            layerSelector.GeoTypeFilter = esriGeometryType.esriGeometryPolyline;
            if (layerSelector.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            if (layerSelector.pSelectLayer == null) { return; }
            refName = getRangeGeometryReference(layerSelector.Shapetxt);
            if (string.IsNullOrEmpty(refName))
            {
                MessageBox.Show("范围文件没有空间参考！");
                return;
            }


            IFeatureLayer inputFC = layerSelector.pSelectLayer as IFeatureLayer;
            var gp = m_Application.GPTool;

            //打开计时器
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();


            if (layerSelector.FieldArray.Count == 0) { return; }
            // using (var wo = m_Application.SetBusy())
            //   {
            //ProcessPseudo(m_Application.Workspace.EsriWorkspace, inputFC, layerSelector.FieldArray);
            ProcessPseudo2(m_Application.Workspace.EsriWorkspace, inputFC, layerSelector.FieldArray);
            //  }


            watch.Stop();
            MessageBox.Show("处理完成!\r\n" + "处理耗时：" + watch.Elapsed.Hours + ":" + watch.Elapsed.Minutes + ":" + watch.Elapsed.Seconds);
        } //协同模式中，由于删除要素没有物理删除，若删除要素也参与到伪节点处理中，则将会造成死循环，需注意应在显示当前模式中进行
        private void ProcessPseudo2(IWorkspace ws, IFeatureLayer lyr, ArrayList fieldArray)
        {
            IRelationalOperator relationalOperator = _rangeGeometry as IRelationalOperator;
            List<int> pCheckint = new List<int>();
            Process pro = new Process();
            Process pro1 = new Process();
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
                Dictionary<int, int> deleteID = new Dictionary<int, int>();
                List<IFeature> geoList = new List<IFeature>();

                IFeatureClass CheckClass = lyr.FeatureClass;
                CreateAdjacencyRelation grid = new CreateAdjacencyRelation(pro);
                Grid = grid.CreateAdjacencyRelation1(CheckClass);
                pro1.Show();
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = (lyr as IFeatureLayerDefinition).DefinitionExpression;
                IFeatureCursor pFeatCursor = lyr.FeatureClass.Search(qf, false);
                IFeature pFeature = pFeatCursor.NextFeature();
                int count = 0;
                while (pFeature != null)
                {
                    count++;
                    if (count == 1)
                    {
                        ISpatialReference ISpatialReference = pFeature.Shape.SpatialReference;
                        if (ISpatialReference.Name != refName) { MessageBox.Show("范围文件与GDB空间参考不一致"); break; }
                    }
                    pro1.label1.Text = "正在处理要素" + pFeature.OID;
                    Application.DoEvents();
                    int oidt = pFeature.OID;
                    if (deleteIDs.Contains(pFeature.OID))
                    {
                        int oid = pFeature.OID;
                        while (deleteID.ContainsKey(oid)) { oid = deleteID[oid]; }
                        pFeature = CheckClass.GetFeature(oid);
                        // pFeature = pFeatCursor.NextFeature();

                    }
                    tempName = new List<string>();
                    geoList.Clear();
                    try
                    {
                        IPolyline CheckLine = CheckClass.GetFeature(oidt).Shape as IPolyline;

                        if (CheckLine.Length > 0)
                        {
                            fileds = new Dictionary<int, string>();
                            double x = ((int)(CheckLine.FromPoint.X * 10)) / 10;
                            double y = ((int)(CheckLine.FromPoint.Y * 10)) / 10;
                            string curitem1 = x + "_" + y;

                            double x2 = ((int)(CheckLine.ToPoint.X * 10)) / 10;
                            double y2 = ((int)(CheckLine.ToPoint.Y * 10)) / 10;
                            string curitem2 = x2 + "_" + y2;

                            if (Grid.ContainsKey(curitem1))
                            {
                                string temp = Grid[curitem1];
                                string[] temp1 = temp.Split(',');
                                if (temp1.Length == 2)
                                {
                                    for (int i = 0; i < temp1.Length; i++)
                                    {
                                        //if (deleteIDs.Contains(int.Parse(temp1[i])))
                                        //{
                                        //    continue;
                                        //}

                                        if (!tempName.Contains(temp1[i]))
                                        { //if (deleteIDs.Contains(int.Parse(temp1[i])))
                                            //    {
                                            //        continue;
                                            //    }
                                            int idt = int.Parse(temp1[i]);
                                            //  if (deleteID.ContainsKey(idt)) { idt = deleteID[idt]; }

                                            while (deleteID.ContainsKey(idt)) { idt = deleteID[idt]; }

                                            IFeature CheckFeature = CheckClass.GetFeature(idt);
                                            IPolyline polyline = CheckFeature.Shape as IPolyline;
                                            string xy = polyline.FromPoint.X + "," + polyline.FromPoint.Y + "," + polyline.ToPoint.X + "," + polyline.ToPoint.Y;
                                            string filed = null;
                                            for (int j = 0; j < fieldArray.Count; j++)
                                            {
                                                int FieldIndex = CheckClass.FindField(fieldArray[j].ToString());
                                                if (FieldIndex != -1)
                                                {
                                                    if (filed != null)
                                                    {
                                                        filed = filed + "," + CheckFeature.get_Value(FieldIndex).ToString();
                                                    }
                                                    else
                                                    {
                                                        filed = CheckFeature.get_Value(FieldIndex).ToString();
                                                    }
                                                }
                                            }
                                            fileds.Add(tempName.Count, idt + "," + xy + "," + filed);

                                            tempName.Add(idt + "");

                                        }
                                    }

                                }
                            }

                            if (Grid.ContainsKey(curitem2))
                            {
                                string temp = Grid[curitem2];
                                string[] temp1 = temp.Split(',');
                                if (temp1.Length == 2)
                                {
                                    for (int i = 0; i < temp1.Length; i++)
                                    {
                                        //if (deleteIDs.Contains(int.Parse(temp1[i])))
                                        //{
                                        //    continue;
                                        //}
                                        if (!tempName.Contains(temp1[i]))
                                        {
                                            int idt = int.Parse(temp1[i]);
                                            //    if (deleteID.ContainsKey(idt)) { idt = deleteID[idt]; }
                                            while (deleteID.ContainsKey(idt)) { idt = deleteID[idt]; }
                                            IFeature CheckFeature = CheckClass.GetFeature(idt);
                                            IPolyline polyline = CheckFeature.Shape as IPolyline;
                                            string xy = polyline.FromPoint.X + "," + polyline.FromPoint.Y + "," + polyline.ToPoint.X + "," + polyline.ToPoint.Y;
                                            string filed = null;

                                            for (int j = 0; j < fieldArray.Count; j++)
                                            {
                                                int FieldIndex = CheckClass.FindField(fieldArray[j].ToString());
                                                if (FieldIndex != -1)
                                                {
                                                    if (filed != null)
                                                    {
                                                        filed = filed + "," + CheckFeature.get_Value(FieldIndex).ToString();
                                                    }
                                                    else
                                                    {
                                                        filed = CheckFeature.get_Value(FieldIndex).ToString();
                                                    }
                                                }
                                            }
                                            fileds.Add(tempName.Count, idt + "," + xy + "," + filed);
                                            tempName.Add(idt + "");
                                        }
                                    }
                                }
                            }
                            ////  起点
                            for (int k = 0; k < tempName.Count; k++)
                            {
                                string[] str = fileds[k].Split(',');

                                string oid = str[0];

                                if (pFeature.OID.ToString() != oid && pCheckint.Contains(int.Parse(oid)) == false)
                                {
                                    IPoint Selectfrom = new PointClass();
                                    Selectfrom.X = Convert.ToDouble(str[1]);
                                    Selectfrom.Y = Convert.ToDouble(str[2]);
                                    IPoint Selectto = new PointClass();
                                    Selectto.X = Convert.ToDouble(str[3]);
                                    Selectto.Y = Convert.ToDouble(str[4]);
                                    IPolyline pFeatureline = pFeature.Shape as IPolyline;
                                    IGeometry pGeo1 = pFeatureline.FromPoint;
                                    if (!relationalOperator.Contains(pGeo1)) { continue; }
                                    IProximityOperator ProxiOP = (pGeo1) as IProximityOperator;
                                    if (ProxiOP.ReturnDistance(Selectfrom) < tolerance || ProxiOP.ReturnDistance(Selectto) < tolerance)
                                    {
                                        bool judge = false;
                                        for (int i = 0; i < fieldArray.Count; i++)
                                        {
                                            int FieldIndex = CheckClass.FindField(fieldArray[i].ToString());
                                            if (FieldIndex != -1)
                                            {
                                                string psfeature = pFeature.get_Value(FieldIndex).ToString();
                                                string psecfeature = str[5 + i];
                                                if (psfeature != psecfeature)
                                                {
                                                    judge = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (judge == false)//属性相同
                                        {
                                            int id = int.Parse(oid);
                                            deleteIDs.Add(id);
                                            while (deleteID.ContainsKey(id)) { id = deleteID[id]; }
                                            if ((id + "") == pFeature.OID.ToString()) { continue; }
                                            //  if (deleteID.ContainsKey(id)) { id = deleteID[id]; }
                                            geoList.Add(CheckClass.GetFeature(id));
                                            // geoList.Add(CheckClass.GetFeature(id).ShapeCopy);
                                            //   CheckClass.GetFeature(id).Delete();
                                            if (deleteID.ContainsKey(id))
                                            {
                                                deleteID[id] = pFeature.OID;
                                            }
                                            else
                                            {
                                                deleteID.Add(id, pFeature.OID);
                                            }
                                        }
                                    }
                                    //System.Runtime.InteropServices.Marshal.ReleaseComObject(ProxiOP);
                                }

                            }

                            for (int k = 0; k < tempName.Count; k++)
                            {
                                string[] str = fileds[k].Split(',');
                                string oid = str[0];
                                //if (deleteIDs.Contains(int.Parse(oid)))
                                //{
                                //    continue;
                                //}
                                if (pFeature.OID.ToString() != oid && pCheckint.Contains(int.Parse(oid)) == false)
                                {
                                    IPoint Selectfrom = new PointClass();
                                    Selectfrom.X = Convert.ToDouble(str[1]);
                                    Selectfrom.Y = Convert.ToDouble(str[2]);
                                    IPoint Selectto = new PointClass();
                                    Selectto.X = Convert.ToDouble(str[3]);
                                    Selectto.Y = Convert.ToDouble(str[4]);

                                    IPolyline pFeatureline = pFeature.Shape as IPolyline;
                                    IGeometry pGeo1 = pFeatureline.ToPoint;
                                    if (!relationalOperator.Contains(pGeo1)) { continue; }
                                    IProximityOperator ProxiOP = (pGeo1) as IProximityOperator;
                                    if (ProxiOP.ReturnDistance(Selectfrom) < tolerance || ProxiOP.ReturnDistance(Selectto) < tolerance)
                                    {
                                        bool judge = false;
                                        for (int i = 0; i < fieldArray.Count; i++)
                                        {
                                            int FieldIndex = CheckClass.FindField(fieldArray[i].ToString());
                                            if (FieldIndex != -1)
                                            {
                                                string psfeature = pFeature.get_Value(FieldIndex).ToString();
                                                string psecfeature = str[5 + i];
                                                if (psfeature != psecfeature)
                                                {
                                                    judge = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (judge == false)//属性相同
                                        {
                                            int id = int.Parse(oid);
                                            deleteIDs.Add(id);
                                            while (deleteID.ContainsKey(id)) { id = deleteID[id]; }
                                            if ((id + "") == pFeature.OID.ToString()) { continue; }//针对环情况
                                            //      if (deleteID.ContainsKey(id)) { id = deleteID[id]; }
                                            geoList.Add(CheckClass.GetFeature(id));
                                            //   geoList.Add(CheckClass.GetFeature(id).ShapeCopy);
                                            //   CheckClass.GetFeature(id).Delete();
                                            if (deleteID.ContainsKey(id))
                                            {
                                                deleteID[id] = pFeature.OID;
                                            }
                                            else
                                            {
                                                deleteID.Add(id, pFeature.OID);
                                            }

                                        }
                                    }
                                    //System.Runtime.InteropServices.Marshal.ReleaseComObject(ProxiOP);
                                }

                            }

                            if (geoList.Count > 0)
                            {
                                for (int g = 0; g < geoList.Count; g++)
                                {
                                    IFeature geo = geoList[g];
                                    ITopologicalOperator topologicalOperator = pFeature.Shape as ITopologicalOperator;
                                    pFeature.Shape = topologicalOperator.Union(geo.Shape);
                                    pFeature.Store();
                                    geo.Delete();
                                    //                System.Runtime.InteropServices.Marshal.ReleaseComObject(topologicalOperator);
                                }
                                //foreach (var geo in geoList)
                                //{
                                //    pFeature.Shape = (pFeature.Shape as ITopologicalOperator).Union(geo);
                                //}
                                //pFeature.Store();

                                //  continue;//新的要素，继续判断
                            }

                        }
                        tempName.Clear();
                        pFeature = pFeatCursor.NextFeature();
                    }
                    catch (Exception ex)
                    {
                        string err = ex.Message;
                        //errorExcelFile.RecordError("意外错误", "", "该项检查未完成");
                    }
                }
                //List<int> nlist = new List<int>();
                //for (int i = 0; i < deleteIDs.Count; i++)
                //{
                //    int id = deleteIDs[i];
                //    if (nlist.Contains(id)) { continue; }
                //    IFeature temp = CheckClass.GetFeature(id);
                //    pro1.label1.Text = "正在处理要素" + id;
                //    //int FieldIndex = CheckClass.FindField("smgiversion");
                //    //temp.set_Value(FieldIndex,-int.MaxValue);
                //    //temp.Store();
                //    temp.Delete();
                //    nlist.Add(id);

                //}


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
            pro1.Close();
        }
        //读取shp文件,获取范围几何体并返回空间参考名称
        private string getRangeGeometryReference(string fileName)
        {
            string refName = "";

            IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(System.IO.Path.GetDirectoryName(fileName), 0);
            IFeatureWorkspace pFeatureWorkspace = pWorkspace as IFeatureWorkspace;
            IFeatureClass shapeFC = pFeatureWorkspace.OpenFeatureClass(System.IO.Path.GetFileName(fileName));

            //是否为多边形几何体
            if (shapeFC.ShapeType != esriGeometryType.esriGeometryPolygon)
            {
                MessageBox.Show("范围文件应为多边形几何体，请重新指定范围文件！");
                return refName;
            }

            //默认为第一个要素的几何体
            IFeatureCursor featureCursor = shapeFC.Search(null, false);
            IFeature pFeature = featureCursor.NextFeature();
            if (pFeature != null && pFeature.Shape is IPolygon)
            {
                _rangeGeometry = pFeature.Shape;
                refName = _rangeGeometry.SpatialReference.Name;
            }
            Marshal.ReleaseComObject(featureCursor);

            return refName;
        }
    }
}
