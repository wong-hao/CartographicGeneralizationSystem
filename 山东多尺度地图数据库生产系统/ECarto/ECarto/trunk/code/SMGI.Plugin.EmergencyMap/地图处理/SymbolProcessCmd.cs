using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using System.Xml.Linq;
namespace SMGI.Plugin.EmergencyMap
{
    [SMGIAutomaticCommand]
    public class SymbolProcessCmd : SMGI.Common.SMGICommand
    {
        public SymbolProcessCmd()
        {
            m_category = "符号冲突处理";
            m_caption = "符号冲突处理";
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
            //从配置文件获取地名图层名
            XElement expertiseContent = ExpertiseDatabase.getContentElement(m_Application);
            XElement sp = expertiseContent.Element("SymbolProcess");
            string targetFCName = sp.Element("FCName").Value;
            string levelFieldName = sp.Element("LevelFieldName").Value;

            var lyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == targetFCName.ToUpper();
            })).FirstOrDefault();
            if (lyr == null)
            {
                MessageBox.Show(string.Format("没有找到要素类【{0}】!",targetFCName));
                return ;
            }

            IGeoFeatureLayer geoLayer = lyr as IGeoFeatureLayer;

            
            m_Application.EngineEditor.StartOperation();
            try
            {
                int num = 0;
                using (var wo = m_Application.SetBusy())
                {
                    SymbolConflictProcess p = new SymbolConflictProcess(m_Application);
                    num = p.DupSymbolProcess(geoLayer, levelFieldName, m_Application.MapControl.ReferenceScale, wo);
                    //POI处理
                    lyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                    {
                        return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == "POI";
                    })).FirstOrDefault();
                    if (lyr != null)
                    {
                        geoLayer = lyr as IGeoFeatureLayer;
                        num += p.DupSymbolProcess(geoLayer, "PRIORITY", m_Application.MapControl.ReferenceScale, wo);
                    }
                }
                m_Application.EngineEditor.StopOperation("冲突处理");

                if (num > 0)
                {
                    m_Application.ActiveView.Refresh();

                    MessageBox.Show(string.Format("发现并处理了【{0}】个冲突要素！", num));
                }
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

        protected override bool DoCommand(XElement args, Action<string> messageRaisedAction)
        {
            try
            {
                //从配置文件获取地名图层名
                messageRaisedAction("开始符号冲突处理...");
                XElement expertiseContent = ExpertiseDatabase.getContentElement(m_Application);
                XElement sp = expertiseContent.Element("SymbolProcess");
                string targetFCName = sp.Element("FCName").Value;
                string levelFieldName = sp.Element("LevelFieldName").Value;
                var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == targetFCName.ToUpper();
                })).ToArray();
                IGeoFeatureLayer geoLayer = lyrs[0] as IGeoFeatureLayer;
                if (null == geoLayer)
                {
                    messageRaisedAction(string.Format("没找到要素类【{0}】！", targetFCName));
                    return false;
                }
                int num = 0;
                SymbolConflictProcess p = new SymbolConflictProcess(m_Application);
                num = p.DupSymbolProcess(geoLayer, levelFieldName, m_Application.MapControl.ReferenceScale);
                if (num > 0)
                {
                    messageRaisedAction(string.Format("发现并处理了【{0}】个冲突要素！", num));
                }
                m_Application.Workspace.Save();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return false;
            }
        }
    }
}
