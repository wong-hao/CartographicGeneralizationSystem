using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.Diagnostics;
using ESRI.ArcGIS.Geometry;
using SMGI.River.Algrithm;

namespace SMGI.Plugin.EmergencyMap
{
    public class RiverAutoGradual
    {
        private GApplication _app;

        public RiverAutoGradual(GApplication app)
        {
            _app = app;
        }

        public string autoGradual(double startWidth, double endWidth)
        {
            string msg = "";

            try
            {
                var pActiveView = _app.ActiveView;
                var pMap = pActiveView as IMap;
                var hydlLayer = _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(x =>
                {
                    return x is IGeoFeatureLayer && (x as IGeoFeatureLayer).FeatureClass.AliasName.StartsWith("HYDL");

                })).ToArray();

                var hydaLayer = _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(x =>
                {
                    return x is IGeoFeatureLayer && (x as IGeoFeatureLayer).FeatureClass.AliasName.StartsWith("HYDA");

                })).ToArray();

                if (hydlLayer.Length == 0 || hydaLayer.Length == 0)
                {
                    return msg ;
                }
                ILayer hydl = hydlLayer[0];
                ILayer hyda = hydaLayer[0];

                IFeatureClass hydlFC;
                IFeatureClass hydaFC;

                hydlFC = (hydl as IFeatureLayer).FeatureClass;
                hydaFC = (hyda as IFeatureLayer).FeatureClass;

                ISpatialFilter pQueryFilter = new SpatialFilterClass();
                pQueryFilter.WhereClause = "gb<=220000";
                IFeatureCursor pFeatureCursor = hydlFC.Search(pQueryFilter, false);
                int feaCount = hydlFC.FeatureCount(pQueryFilter);
                IFeature pFeature = null;
            //   HydroAlgorithm.HydroGraph hydroGraph = new HydroAlgorithm.HydroGraph();
                var hydroGraph = new HydroAlgorithm.HydroGraph();
                //读取数据
                List<int> tempOIDs = new List<int>();
                while ((pFeature = pFeatureCursor.NextFeature()) != null)
                {
                    ISpatialFilter sp = new SpatialFilterClass();
                    sp.GeometryField = hydaFC.ShapeFieldName;

                    int startFeaCount = 0;
                    int endFeaCount = 0;

                    IPolyline pl = pFeature.Shape as IPolyline;
                    IPoint ptStart = pl.FromPoint;
                    IPoint ptEnd = pl.ToPoint;

                    sp.Geometry = (ptStart as ITopologicalOperator).Buffer(5);
                    sp.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    startFeaCount = hydaFC.FeatureCount(sp);

                    sp.Geometry = (ptEnd as ITopologicalOperator).Buffer(5);
                    sp.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    endFeaCount = hydaFC.FeatureCount(sp);

                    //排除所有双线河内的河流
                    if (startFeaCount == 0 && endFeaCount >= 0)
                    {
                        hydroGraph.Add(pFeature);
                    }
                    else if (startFeaCount >= 0 && endFeaCount == 0)
                    {
                        hydroGraph.Add(pFeature);
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);

                //获取Path列表
                var hydroPaths = hydroGraph.BuildHydroPaths();

                //计算河流总长度
                double allHydroLength = 0;
                foreach (var path in hydroPaths)
                {
                    path.CalLength();
                    allHydroLength += path.Lenght;
                }

                //对河流长度排序.
                hydroPaths.Sort();
                //测试 计算path起点到河源的距离
                for (int i = hydroPaths.Count-1; i >= 0; i--)
                {
                    var path0 = hydroPaths[i].Edges[0];
                }


                //提取被双线河压盖的树根
                double leftLength = allHydroLength;
                int currentLevel = 4;
                double currentLevelLength = allHydroLength * (1 - Math.Sqrt(1.0 / ((currentLevel) * 2)));
                //处理要素
                foreach (var path in hydroPaths)
                {
                    if (currentLevel == 0)
                    {
                        path.CalculateWidth(startWidth, endWidth);
                    }
                    else
                    {
                        double temEnd = endWidth * Math.Sqrt(1.0 / ((currentLevel) * 2));
                        path.CalculateWidth(startWidth, temEnd > startWidth ? temEnd : startWidth);
                    }

                    leftLength -= path.Lenght;

                    if (leftLength < currentLevelLength)
                    {
                        currentLevel--;
                        if (currentLevel == 0)
                        {
                            currentLevelLength = 0;
                        }
                        else
                        {
                            currentLevelLength = allHydroLength * (1 - Math.Sqrt(1.0 / ((currentLevel) * 2)));
                        }
                    }
                }

                //修改属性表
                for (int i = 0; i < hydroPaths.Count; i++)
                {
                    foreach (var item in hydroPaths[i].Edges)
                    {
                        AppendLineWidthInfo(item.Feature, item.StartWidth, item.EndWidth);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                msg = ex.Message;
            }

            return msg;
        }
        /// <summary>
        /// 考虑到分叉清空
        /// </summary>
        public string autoGradualFork(double startWidth, double endWidth)
        {
            var m_Application = _app;
            string msg = "";

            try
            {

                var hydlLayer = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(x =>
                {
                    return x is IGeoFeatureLayer && (x as IGeoFeatureLayer).FeatureClass.AliasName.StartsWith("HYDL") && ((x as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName == _app.Workspace.EsriWorkspace.PathName;

                })).ToArray();
                var hydaLayer = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(x =>
                {
                    return x is IGeoFeatureLayer && (x as IGeoFeatureLayer).FeatureClass.AliasName.StartsWith("HYDA") && ((x as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName == _app.Workspace.EsriWorkspace.PathName;

                })).ToArray();

                if (hydlLayer.Length == 0 || hydaLayer.Length == 0)
                {
                    return "水系线或面图层为空";
                }
                ILayer hydl = hydlLayer[0];
                IFeatureClass hydlFC = (hydl as IFeatureLayer).FeatureClass;
              
               
                ILayer hyda = hydaLayer[0];
                IFeatureClass hydaFC = (hyda as IFeatureLayer).FeatureClass;

                ISpatialFilter pQueryFilter = new SpatialFilterClass();
                pQueryFilter.WhereClause = "gb<=220000  and RuleID <>1";
                IFeatureCursor pFeatureCursor = hydlFC.Search(pQueryFilter, false);
                int feaCount = hydlFC.FeatureCount(pQueryFilter);
                IFeature pFeature = null;

                var hydroGraph = new HydroAlgorithmEx.HydroGraph();
             
                List<int> tempOIDs = new List<int>();
                while ((pFeature = pFeatureCursor.NextFeature()) != null)
                {
                    ISpatialFilter sp = new SpatialFilterClass();
                    sp.GeometryField = hydaFC.ShapeFieldName;

                    int startFeaCount = 0;
                    int endFeaCount = 0;

                    IPolyline pl = pFeature.Shape as IPolyline;
                    IPoint ptStart = pl.FromPoint;
                    IPoint ptEnd = pl.ToPoint;

                    sp.Geometry = (ptStart as ITopologicalOperator).Buffer(5);
                    sp.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    startFeaCount = hydaFC.FeatureCount(sp);

                    sp.Geometry = (ptEnd as ITopologicalOperator).Buffer(5);
                    sp.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    endFeaCount = hydaFC.FeatureCount(sp);

                    //排除所有双线河内的河流
                    if (startFeaCount == 0 && endFeaCount >= 0)
                    {
                        hydroGraph.Add(pFeature);
                    }
                    else if (startFeaCount >= 0 && endFeaCount == 0)
                    {
                        hydroGraph.Add(pFeature);
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);

               
                var hydroPaths = hydroGraph.BuildHydroPaths();

               
                double allHydroLength = 0;
                double maxHydroLenght = 0;
                foreach (var path in hydroPaths)
                {
                    path.CalLength();
                    allHydroLength += path.Lenght;
                    if (maxHydroLenght < path.Lenght)
                        maxHydroLenght = path.Lenght;
                }

              
                hydroPaths.Sort();


                List<HydroAlgorithmEx.HydroPath> forkRivers = new List<HydroAlgorithmEx.HydroPath>();
                foreach (var path in hydroPaths)
                {
                   
                    if (path.IsForkRiver)
                    {
                        forkRivers.Add(path);
                        continue;
                    }
                    var currentStartWidth = startWidth;

                    var currentEndWidth = HydroAlgorithmEx.HydroPath.CalculateWidth(maxHydroLenght, path.Lenght, startWidth, endWidth);
                    path.CalculateWidth(startWidth, currentEndWidth);

                }
                int count = 0;
                var leftRivers = new List<HydroAlgorithmEx.HydroPath>();
                while (forkRivers.Count > 0)
                {
                    count = forkRivers.Count;
                    leftRivers = new List<HydroAlgorithmEx.HydroPath>();
                    #region
                    foreach (var path in forkRivers)
                    {
                        int pathid = path.Index;
                        try
                        {

                            if (path.FromNode.DownStreamEdge == null)
                            {
                                leftRivers.Add(path);
                                continue;
                            }
                            var currentStartWidth = path.FromNode.DownStreamEdge.StartWidth;
                            if (currentStartWidth == 0)//说明前面的分叉的河流还没有处理
                            {
                                leftRivers.Add(path);
                                continue;
                            }
                            var currentEndWidth = currentStartWidth;
                            if (path.RootNode.DownStreamEdge == null)
                            {
                                currentEndWidth += HydroAlgorithmEx.HydroPath.CalculateWidth(maxHydroLenght, path.Lenght, startWidth, endWidth) - startWidth;
                            }
                            else
                            {
                                currentEndWidth = path.RootNode.DownStreamEdge.StartWidth;
                            }
                            path.CalculateWidth(currentStartWidth, currentEndWidth);
                        }
                        catch
                        {

                            continue;
                        }
                    }
                    forkRivers = leftRivers;
                    if (leftRivers.Count == count)//死循环
                    {
                        break;
                    }
                    #endregion
                }
                //处理leftriver
                for (int i = 0; i < leftRivers.Count; i++)//处理单独河流
                {
                    var currentStartWidth = 0.283;
                    double currentEndWidth = 0.283;
                    var path = leftRivers[i];
                    if (path.RootNode.DownStreamEdge == null)
                    {
                        currentEndWidth += HydroAlgorithmEx.HydroPath.CalculateWidth(maxHydroLenght, path.Lenght, startWidth, endWidth) - startWidth;
                    }
                    else
                    {
                        currentEndWidth = path.RootNode.DownStreamEdge.StartWidth;
                    }
                    path.CalculateWidth(currentStartWidth, currentEndWidth);

                }
                for (int i = 0; i < hydroPaths.Count; i++)//赋值
                {
                    foreach (var item in hydroPaths[i].Edges)
                    {
                       if(item.StartWidth==0&&item.EndWidth==0)
                       {
                           item.StartWidth = 0.283;
                           item.EndWidth = 0.283;
                       }
                        AppendLineWidthInfo(item.Feature, item.StartWidth, item.EndWidth);
                    }
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                msg = ex.Message;
            }
            return msg;
        }

        /// <summary>
        /// 双线河内的河流参与渐变
        /// </summary>
        /// <param name="startWidth"></param>
        /// <param name="endWidth"></param>
        /// <returns></returns>
        public string autoGradualFork2(double startWidth, double endWidth)
        {
            var m_Application = _app;
            string msg = "";

            try
            {

                var hydlLayer = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(x =>
                {
                    return x is IGeoFeatureLayer && (x as IGeoFeatureLayer).FeatureClass.AliasName.StartsWith("HYDL") && ((x as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName == _app.Workspace.EsriWorkspace.PathName;

                })).FirstOrDefault();
                if (hydlLayer == null)
                {
                    return "找不到水系线图层！";
                }
                IFeatureClass hydlFC = (hydlLayer as IFeatureLayer).FeatureClass;
                string gbFN = "";
                if (hydlFC.FindField("hgb") != -1)
                {
                    gbFN = "hgb";
                }
                else if (hydlFC.FindField("gb") != -1)
                {
                    gbFN = "gb";
                }

                ISpatialFilter pQueryFilter = new SpatialFilterClass();
                pQueryFilter.WhereClause = "RuleID <>1";
                if(gbFN !="")
                    pQueryFilter.WhereClause += string.Format("  and {0} < 220000", gbFN);
                IFeatureCursor pFeatureCursor = hydlFC.Search(pQueryFilter, true);
                int feaCount = hydlFC.FeatureCount(pQueryFilter);
                IFeature pFeature = null;

                var hydroGraph = new HydroAlgorithmEx.HydroGraph();

                List<int> tempOIDs = new List<int>();
                while ((pFeature = pFeatureCursor.NextFeature()) != null)
                {
                    hydroGraph.Add(pFeature);
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);


                var hydroPaths = hydroGraph.BuildHydroPaths();


                double allHydroLength = 0;
                double maxHydroLenght = 0;
                foreach (var path in hydroPaths)
                {
                    path.CalLength();
                    allHydroLength += path.Lenght;
                    if (maxHydroLenght < path.Lenght)
                        maxHydroLenght = path.Lenght;
                }


                hydroPaths.Sort();


                List<HydroAlgorithmEx.HydroPath> forkRivers = new List<HydroAlgorithmEx.HydroPath>();
                foreach (var path in hydroPaths)
                {

                    if (path.IsForkRiver)
                    {
                        forkRivers.Add(path);
                        continue;
                    }
                    var currentStartWidth = startWidth;

                    var currentEndWidth = HydroAlgorithmEx.HydroPath.CalculateWidth(maxHydroLenght, path.Lenght, startWidth, endWidth);
                    path.CalculateWidth(startWidth, currentEndWidth);

                }
                int count = 0;
                var leftRivers = new List<HydroAlgorithmEx.HydroPath>();
                while (forkRivers.Count > 0)
                {
                    count = forkRivers.Count;
                    leftRivers = new List<HydroAlgorithmEx.HydroPath>();
                    #region
                    foreach (var path in forkRivers)
                    {
                        int pathid = path.Index;
                        try
                        {

                            if (path.FromNode.DownStreamEdge == null)
                            {
                                leftRivers.Add(path);
                                continue;
                            }
                            var currentStartWidth = path.FromNode.DownStreamEdge.StartWidth;
                            if (currentStartWidth == 0)//说明前面的分叉的河流还没有处理
                            {
                                leftRivers.Add(path);
                                continue;
                            }
                            var currentEndWidth = currentStartWidth;
                            if (path.RootNode.DownStreamEdge == null)
                            {
                                currentEndWidth += HydroAlgorithmEx.HydroPath.CalculateWidth(maxHydroLenght, path.Lenght, startWidth, endWidth) - startWidth;
                            }
                            else
                            {
                                currentEndWidth = path.RootNode.DownStreamEdge.StartWidth;
                            }
                            path.CalculateWidth(currentStartWidth, currentEndWidth);
                        }
                        catch
                        {

                            continue;
                        }
                    }
                    forkRivers = leftRivers;
                    if (leftRivers.Count == count)//死循环
                    {
                        break;
                    }
                    #endregion
                }
                //处理leftriver
                for (int i = 0; i < leftRivers.Count; i++)//处理单独河流
                {
                    double currentEndWidth = startWidth;
                    var path = leftRivers[i];
                    if (path.RootNode.DownStreamEdge == null)
                    {
                        currentEndWidth = HydroAlgorithmEx.HydroPath.CalculateWidth(maxHydroLenght, path.Lenght, startWidth, endWidth);
                    }
                    else
                    {
                        currentEndWidth = path.RootNode.DownStreamEdge.StartWidth;
                    }
                    path.CalculateWidth(startWidth, currentEndWidth);

                }
                for (int i = 0; i < hydroPaths.Count; i++)//赋值
                {
                    foreach (var item in hydroPaths[i].Edges)
                    {
                        if (item.StartWidth == 0 && item.EndWidth == 0)
                        {
                            item.StartWidth = startWidth;
                            item.EndWidth = startWidth;
                        }
                        AppendLineWidthInfo(item.Feature, item.StartWidth, item.EndWidth);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                msg = ex.Message;
            }
            return msg;
        }

        public void AppendLineWidthInfo(IFeature pRiver, double startWidth, double endWidth)
        {
            int indexfrom = pRiver.Fields.FindField("FromWidth");
            int indexto = pRiver.Fields.FindField("ToWidth");
            pRiver.set_Value(indexfrom, startWidth);
            pRiver.set_Value(indexto, endWidth);
            pRiver.Store();
        } 
    }
}
