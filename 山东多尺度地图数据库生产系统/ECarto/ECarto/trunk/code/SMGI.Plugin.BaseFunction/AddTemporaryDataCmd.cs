using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.Catalog;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.DataSourcesRaster;

namespace SMGI.Plugin.BaseFunction
{
    public class AddTemporaryDataCmd : SMGICommand
    {

        public AddTemporaryDataCmd()
        {
            m_caption = "添加临时数据";
            m_toolTip = "向工作区添加临时数据";
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null;

            }
        }
        public override void OnClick()
        {
            IGxDialog gxDialog = new GxDialog();
            //(gxDialog as IGxObjectFilterCollection).AddFilter(new GxFilterFeatureDatasetsClass(), false);
            (gxDialog as IGxObjectFilterCollection).AddFilter(new GxFilterFeatureClassesClass(), false);
            //(gxDialog as IGxObjectFilterCollection).AddFilter(new GxFilterTopologiesClass(), false);
            (gxDialog as IGxObjectFilterCollection).AddFilter(new GxFilterRasterDatasetsClass(), false);
            (gxDialog as IGxObjectFilterCollection).AddFilter(new GxFilterLayersClass(), false);

            gxDialog.AllowMultiSelect = true;
            gxDialog.RememberLocation = true;
            gxDialog.Title = m_caption;

            IEnumGxObject enumObj;
            if (!gxDialog.DoModalOpen(0, out enumObj))
                return;

            if (enumObj == null)
                return;

            using (var wo = m_Application.SetBusy())
            {
                enumObj.Reset();
                IGxObject gxObj = null;
                while ((gxObj = enumObj.Next()) != null)
                {
                    wo.SetText(string.Format("正在导入数据......"));

                    if (gxObj is IGxDataset)
                    {
                        #region IGxDataset
                        IGxDataset gxDataset = gxObj as IGxDataset;
                        IDataset dataset = gxDataset.Dataset;
                        switch (dataset.Type)
                        {
                            case esriDatasetType.esriDTFeatureDataset:
                                IFeatureDataset featureDt = dataset as IFeatureDataset;
                                IEnumDataset enumSubDataset = featureDt.Subsets;
                                enumSubDataset.Reset();
                                IDataset subDataset = null;
                                while ((subDataset = enumSubDataset.Next()) != null)
                                {
                                    if (subDataset is IFeatureClass)//要素类
                                    {
                                        IFeatureLayer sublayer = null;
                                        if ((subDataset as IFeatureClass).FeatureType == esriFeatureType.esriFTAnnotation)
                                        {
                                            sublayer = new FDOGraphicsLayerClass();
                                        }
                                        else
                                        {
                                            sublayer = new FeatureLayerClass();
                                        }
                                        sublayer.FeatureClass = subDataset as IFeatureClass;
                                        sublayer.Name = subDataset.Name + "_临时";

                                        if (m_Application.Workspace != null)
                                        {
                                            m_Application.Workspace.LayerManager.Map.AddLayer(sublayer);
                                            m_Application.Workspace.LayerManager.Map.MoveLayer(sublayer, 0);
                                        }
                                        else
                                        {
                                            m_Application.MapControl.AddLayer(sublayer);
                                        }
                                    }
                                }
                                Marshal.ReleaseComObject(enumSubDataset);
                                break;
                            case esriDatasetType.esriDTFeatureClass:
                                IFeatureLayer layer = null;
                                if ((dataset as IFeatureClass).FeatureType == esriFeatureType.esriFTAnnotation)
                                {
                                    layer = new FDOGraphicsLayerClass();
                                }
                                else
                                {
                                    layer = new FeatureLayerClass();
                                }
                                layer.FeatureClass = dataset as IFeatureClass;
                                layer.Name = dataset.Name + "_临时";

                                if (m_Application.Workspace != null)
                                {
                                    m_Application.Workspace.LayerManager.Map.AddLayer(layer);
                                    m_Application.Workspace.LayerManager.Map.MoveLayer(layer, 0);
                                }
                                else
                                {
                                    m_Application.MapControl.AddLayer(layer);
                                }
                                break;
                            case esriDatasetType.esriDTTopology:
                                ITopologyLayer topoLayer = new TopologyLayerClass();
                                topoLayer.Topology = dataset as ITopology;
                                (topoLayer as ILayer).Name = dataset.Name;

                                if (m_Application.Workspace != null)
                                {
                                    m_Application.Workspace.LayerManager.Map.AddLayer(topoLayer as ILayer);
                                    m_Application.Workspace.LayerManager.Map.MoveLayer(topoLayer as ILayer, m_Application.Workspace.LayerManager.Map.LayerCount - 1);
                                }
                                else
                                {
                                    m_Application.MapControl.AddLayer(topoLayer as ILayer, m_Application.MapControl.LayerCount);
                                }
                                break;
                            case esriDatasetType.esriDTRasterDataset:
                                IRasterLayer rasterLayer = new RasterLayerClass();
                                if ((dataset as IRasterBandCollection).Count > 3)//默认只读取了前三个波段的数据，这里需做处理，@LZ
                                {
                                    IRaster raster = new RasterClass();
                                    (raster as IRasterBandCollection).AppendBands(dataset as IRasterBandCollection);

                                    rasterLayer.CreateFromRaster(raster);
                                }
                                else
                                {
                                    rasterLayer.CreateFromDataset(dataset as IRasterDataset);
                                }

                                if (m_Application.Workspace != null)
                                {
                                    m_Application.Workspace.LayerManager.Map.AddLayer(rasterLayer);
                                    m_Application.Workspace.LayerManager.Map.MoveLayer(rasterLayer, m_Application.Workspace.LayerManager.Map.LayerCount - 1);
                                }
                                else
                                {
                                    m_Application.MapControl.AddLayer(rasterLayer, m_Application.MapControl.LayerCount);
                                }
                                break;
                            default:
                                break;

                        }
                        #endregion
                    }
                    else if (gxObj is IGxLayer)
                    {
                        IGxLayer gxLayer = gxObj as IGxLayer;
                        ILayer layer = gxLayer.Layer;

                        if (m_Application.Workspace != null)
                        {
                            m_Application.Workspace.LayerManager.Map.AddLayer(layer);
                            m_Application.Workspace.LayerManager.Map.MoveLayer(layer, m_Application.Workspace.LayerManager.Map.LayerCount - 1);
                        }
                        else
                        {
                            m_Application.MapControl.AddLayer(layer, m_Application.MapControl.LayerCount);
                        }
                    }
                }
            }
        }
    }
}
