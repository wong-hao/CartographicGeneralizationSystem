using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.Xml.Linq;
using System.Windows.Forms;

namespace SMGI.Plugin.BaseFunction
{
    public class AddMapCommand : SMGICommand
    {
        public AddMapCommand()
        {
            m_caption = "添加附图";
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && m_Application.LayoutState == LayoutState.PageLayoutControl;
            }
        }

        public override void OnClick()
        {
            //读取项目配置信息
            XElement contentXEle = m_Application.Template.Content;
            XElement mxdXEle = contentXEle.Element("LocationMxd");
            string mxdFullFileName = m_Application.Template.Root + "\\" + mxdXEle.Value;

            XElement tpXEle = contentXEle.Element("TP10W");
            XElement dataXEle = tpXEle.Element("Data");

            string dataFullFileName = m_Application.Template.Root + "\\" + dataXEle.Value;

            IMapDocument pMapDoc = new MapDocumentClass();
            pMapDoc.Open(mxdFullFileName, "");

            //如果地图模板为空
            if (pMapDoc.MapCount == 0)
            {
                MessageBox.Show("位置模板不能为空！");
                return;
            }

            using (var wo = m_Application.SetBusy())
            {
                wo.SetText("正在准备......");
                Guid mapID = m_Application.Workspace.AddMap();

                var lyrMgr = m_Application.Workspace.GetMapByGuid(mapID);
                IMap map = lyrMgr.Map;

                IMap templateMap = pMapDoc.get_Map(0);

                map.SpatialReference = templateMap.SpatialReference;
                map.ReferenceScale = templateMap.ReferenceScale;

                wo.SetText("正在匹配模板......");
                map.ClearLayers();
                List<ILayer> layers = new List<ILayer>();
                for (int i = templateMap.LayerCount - 1; i >= 0; i--)
                {
                    var l = templateMap.get_Layer(i);
                    layers.Add(l);
                }
                templateMap.ClearLayers();

                foreach (var item in layers)
                {
                    MatchLayer(item, null);
                }

                wo.SetText("正在更新视图......");
                var lyrs = lyrMgr.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return l is IGeoFeatureLayer;
                })).ToArray();

                for (int i = 0; i < lyrs.Length; i++)
                {
                    IFeatureClass fc = (lyrs[i] as IFeatureLayer).FeatureClass;
                    IFeatureClassManage fcMgr = fc as IFeatureClassManage;
                    fcMgr.UpdateExtent();
                    if (lyrs[i].Name.StartsWith("BOUA"))
                    {
                        m_Application.MapControl.Extent = (fc as IGeoDataset).Extent;
                    }
                }
            }
            m_Application.Workspace.FocusMapAt(0);
        }

        //模板匹配
        private void MatchLayer(ILayer layer, IGroupLayer parent)
        {
            if (parent == null)
            {
                m_Application.Workspace.Map.AddLayer(layer);
            }
            else
            {
                (parent as IGroupLayer).Add(layer);
            }

            if (layer is IGroupLayer)
            {
                var l = (layer as ICompositeLayer);

                List<ILayer> layers = new List<ILayer>();
                for (int i = 0; i < l.Count; i++)
                {
                    layers.Add(l.get_Layer(i));
                }
                (layer as IGroupLayer).Clear();
                foreach (var item in layers)
                {
                    MatchLayer(item, layer as IGroupLayer);
                }
            }
            else
            {
                string name = ((layer as IDataLayer2).DataSourceName as IDatasetName).Name;
                if (layer is IFeatureLayer)
                {
                    //然后匹配
                    if ((m_Application.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, name))
                    {
                        IFeatureClass fc = (m_Application.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(name);
                        (layer as IFeatureLayer).FeatureClass = fc;
                    }
                }
                else if (layer is IRasterLayer)
                {
                    if ((m_Application.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTRasterDataset, name))
                    {
                        (layer as IRasterLayer).CreateFromRaster((m_Application.Workspace.EsriWorkspace as IRasterWorkspaceEx).OpenRasterDataset(name).CreateDefaultRaster());
                    }
                }
            }
        }
    }
}
