using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using DevExpress.XtraBars.Docking;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 全局事件监听
    /// </summary>
    public class AppEventListenerCmd : SMGI.Common.SMGICommand
    {
        public AppEventListenerCmd()
        {
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null;
            }
        }

        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
            m_Application.WorkspaceOpened += new EventHandler(m_Application_WorkspaceOpened);
          
        }
        //打开数据库
        void m_Application_WorkspaceOpened(object sender, EventArgs e)
        {

            #region//增加默认绘图
            IGraphicsLayer basicLayer = GApplication.Application.ActiveView.FocusMap.BasicGraphicsLayer;
            ICompositeGraphicsLayer cgLayer = basicLayer as ICompositeGraphicsLayer;
            ICompositeLayer comLayer = cgLayer as ICompositeLayer;
            IGraphicsLayer sublayer = null;
            if (comLayer.Count > 0)
            {
                sublayer = cgLayer.FindLayer("Default");//查找CompositeGraphicsLayer中有没有名为SubLayerName的GraphicsSubLayer  
            }
            if (sublayer == null)
            {
                sublayer = cgLayer.AddLayer("Default", null);//ICompositeGraphicsLayer.AddLayer方法其实返回的是一个GraphicsSubLayer的实例对象  
            }
            #endregion
            m_Application.Workspace.WorkspaceSaved += new EventHandler(m_Application_WorkspaceSaved);
            Dictionary<string, string> envString = m_Application.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
            if (envString == null || !envString.ContainsKey("UsingMasking"))
            {
                envString = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
            }
            if (envString != null)
            {
                if (envString.ContainsKey("UsingMasking"))
                {
                    string maskingLyr = envString["MaskingLyr"];
                    string maskedLyr = envString["MaskedLyr"];
                    #region

                    IFeatureClass HYDAfcl = null;
                    IFeatureClass HYDLfcl = null;
                    IGroupLayer groupLyr = null;

                    var HYDLlyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(x =>
                    {
                        return x is IFeatureLayer && (x as IFeatureLayer).Name == maskingLyr && ((x as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName == m_Application.Workspace.EsriWorkspace.PathName;

                    })).FirstOrDefault() as IFeatureLayer;

                    //判断不是临时数据
                    var HYDAlyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(x =>
                    {
                        return x is IFeatureLayer && (x as IFeatureLayer).Name == maskedLyr && ((x as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName == m_Application.Workspace.EsriWorkspace.PathName;

                    })).FirstOrDefault() as IFeatureLayer;
                    var groups = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(x =>
                    {
                        return (x is IGroupLayer);

                    }));

                    if (HYDLlyr != null)
                    {
                        HYDLfcl = HYDLlyr.FeatureClass;
                    }
                    else
                    {
                        return;
                    }
                    if (HYDAlyr != null)
                    {
                        HYDAfcl = HYDAlyr.FeatureClass;

                    }
                    else
                    {
                        return;
                    }
                    IGroupLayer groupLyr1 = null;
                    IGroupLayer groupLyr2 = null;
                    foreach (var group in groups)
                    {
                        ICompositeLayer g = group as ICompositeLayer;
                        for (int i = 0; i < g.Count; i++)
                        {
                            var l = g.get_Layer(i);
                            if (l is IFeatureLayer)
                            {
                                if ((l as IFeatureLayer).Name == maskingLyr)
                                {
                                    groupLyr1 = g as IGroupLayer;
                                }
                                if ((l as IFeatureLayer).Name == maskedLyr)
                                {
                                    groupLyr2 = g as IGroupLayer;
                                }
                            }
                        }
                    }
                    if (groupLyr1.Equals(groupLyr2))
                    {
                        groupLyr = groupLyr1;

                    }
                    else
                    {
                        return;
                    }
                    //增加定义查询：不显示要素
                    string queryExpression = string.Format("RuleID <> {0}", 1);
                    var fd = HYDAlyr as ESRI.ArcGIS.Carto.IFeatureLayerDefinition;
                    string finitionExpression = fd.DefinitionExpression;
                    fd.DefinitionExpression = queryExpression;
                    ILayerMasking lyrMask = m_Application.ActiveView.FocusMap as ILayerMasking;
                    if (groupLyr != null)
                        lyrMask = groupLyr as ILayerMasking;
                    lyrMask.ClearMasking(HYDLlyr);
                    bool usingMask =bool.Parse(envString["UsingMasking"].ToString());
                    if (usingMask)
                    {
                        lyrMask.UseMasking = true;
                        ESRI.ArcGIS.esriSystem.ISet pSet = new ESRI.ArcGIS.esriSystem.SetClass();
                        pSet.Add(HYDAlyr);
                        lyrMask.set_MaskingLayers(HYDLlyr, pSet);
                    }
                    else
                    {
                        lyrMask.UseMasking = false;
                    }

                  
                    m_Application.ActiveView.Refresh();
                    #endregion
                }
            }
        }
        //保存数据库
        void m_Application_WorkspaceSaved(object sender, EventArgs e)
        {
            if(CommonMethods.UsingMask)
            {
                Dictionary<string, string> envString = m_Application.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
                if (envString == null)
                {
                    envString = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
                }

                //追加到配置表中
                envString["UsingMasking"] = "true";
                envString["MaskingLyr"] = CommonMethods.MaskedLayer;
                envString["MaskedLyr"] = CommonMethods.MaskLayer;
                EnvironmentSettings.UpdateEnvironmentToConfig(envString);
                   
            }
        }
        public override void OnClick()
        {
           

        }
    }
}
