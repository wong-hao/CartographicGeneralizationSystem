using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
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
using System.IO;
namespace SMGI.Plugin.EmergencyMap
{
    [SMGIAutomaticCommand]
    public class POIAutoSelectionCmd : SMGI.Common.SMGICommand
    {
        public POIAutoSelectionCmd()
        {
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState== esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            var view = m_Application.ActiveView;
            var editor = m_Application.EngineEditor;
            //获取应急快速制图数据环境变量配置
            Dictionary<string, string> envString = m_Application.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
            bool attachMap = false;
            if(envString==null)
            {
                envString = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
            }
            if(envString.ContainsKey("AttachMap"))
                attachMap = bool.Parse(envString["AttachMap"]);
            if (attachMap)
            {
                 string err = "";
                 var frm = new POISelectionFormEx(m_Application);
                 if (DialogResult.OK == frm.ShowDialog())
                 {
                     #region
                     using (var wo = m_Application.SetBusy())
                     {
                         wo.SetText("正在读取过滤文件...");
                         string filterFileName = frm.FilterFileName;
                         Dictionary<string, List<string>> fcName2filterNames = POIHelper.getFilterNamesOfPOISelection(filterFileName);
                         editor.StartOperation();
                         wo.SetText("正在选取...");
                         POISelection poiSel = new POISelection(m_Application, frm.fclDisplayDic);
                         //err = poiSel.autoSelectEx(frm.POISelectionInfoList, frm.POISelectionInfoListEx, fcName2filterNames);
                         err = poiSel.POISelect(frm.POISelectionInfoList, frm.POISelectionInfoListEx, fcName2filterNames, frm.WeightScale);
                         editor.StopOperation("POI自动选取");
                         view.Refresh();
                     }

                     if (!string.IsNullOrEmpty(err))
                     {
                         MessageBox.Show(err);
                     }
                     #endregion
                 }
            }
            else
            {
                string err = "";
                var frm = new POISelectionForm(m_Application);
                if (DialogResult.OK == frm.ShowDialog())
                {
                    #region
                    using (var wo = m_Application.SetBusy())
                    {
                        wo.SetText("正在读取过滤文件...");
                        string filterFileName = frm.FilterFileName;
                        Dictionary<string, List<string>> fcName2filterNames = POIHelper.getFilterNamesOfPOISelection(filterFileName);
                        editor.StartOperation();
                        wo.SetText("正在选取...");
                        POISelection poiSel = new POISelection(m_Application, frm.fclDisplayDic);
                        //err = poiSel.autoSelect(frm.POISelectionInfoList, fcName2filterNames);
                        err = poiSel.POISelect(frm.POISelectionInfoList, null, fcName2filterNames, frm.WeightScale);
                        editor.StopOperation("POI自动选取");                      
                        view.Refresh();
                    }

                    if (err == "")
                    {
                        MessageBox.Show("已成功完成POI自动选取");
                    }

                    if (!string.IsNullOrEmpty(err))
                    {
                        MessageBox.Show(err);
                    }
                    #endregion
                }
            }
          
        }



        protected override bool DoCommand(XElement args, Action<string> messageRaisedAction)
        {
            string sourceFileGDB = GApplication.Application.Workspace.FullName;
            try
            {
                //获取应急快速制图数据环境变量配置
                Dictionary<string, string> envString = m_Application.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
                if (envString == null)
                {
                    envString = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
                }
                bool attachMap = false;
                if (envString.ContainsKey("AttachMap"))
                    attachMap = bool.Parse(envString["AttachMap"]);
                if (attachMap)
                {
                    string err = "";
                    var frm = new POISelectionFormEx(m_Application);
                    frm.SetAutoParas();
                    #region
                    messageRaisedAction("正在读取过滤文件...");
                    string filterFileName = frm.FilterFileName;
                    Dictionary<string, List<string>> fcName2filterNames = POIHelper.getFilterNamesOfPOISelection(filterFileName);
                    messageRaisedAction("正在选取...");
                    POISelection poiSel = new POISelection(m_Application, frm.fclDisplayDic);
                    string selectXmlPath = GApplication.Application.Template.Root + @"\专家库\POI选取\POISelectRule.xml";
                    frm.LoadOutParams(selectXmlPath);
                    frm.btOK_Click(null, null);

                    //err = poiSel.autoSelectEx(frm.POISelectionInfoList, frm.POISelectionInfoListEx, fcName2filterNames);
                    err = poiSel.POISelect(frm.POISelectionInfoList, frm.POISelectionInfoListEx, fcName2filterNames, frm.WeightScale);
                    if (!string.IsNullOrEmpty(err))
                    {
                        messageRaisedAction(err);
                        return false;
                    }
                    m_Application.Workspace.Save();
                    return true;
                    #endregion
                }
                else
                {
                    string err = "";
                    var frm = new POISelectionForm(m_Application);
                    frm.SetAutoParas();
                    #region
                    messageRaisedAction("正在读取过滤文件...");
                    string filterFileName = frm.FilterFileName;
                    Dictionary<string, List<string>> fcName2filterNames = POIHelper.getFilterNamesOfPOISelection(filterFileName);
                    messageRaisedAction("正在选取...");
                    POISelection poiSel = new POISelection(m_Application, frm.fclDisplayDic);
                    string selectXmlPath = GApplication.Application.Template.Root + @"\专家库\POI选取\POISelectRule.xml";
                    frm.LoadOutParams(selectXmlPath);
                    frm.btOK_Click(null, null);
                    //err = poiSel.autoSelect(frm.POISelectionInfoList, fcName2filterNames);
                    err = poiSel.POISelect(frm.POISelectionInfoList, null, fcName2filterNames, frm.WeightScale);
                    if (!string.IsNullOrEmpty(err))
                    {
                        messageRaisedAction(err);
                        return false;
                    }
                    m_Application.Workspace.Save();                 
                    #endregion
                }
                string dirName = new FileInfo(sourceFileGDB).DirectoryName;
                string filexml = dirName + "\\auto.xml";
                if (File.Exists(filexml))
                {
                    AutoMapHelper.UpdateState(filexml, "POI选取成功");
                }
                return true;
            }

            catch
            {
                string dirName = new FileInfo(sourceFileGDB).DirectoryName;
                string filexml = dirName + "\\auto.xml";
                if (File.Exists(filexml))
                {
                    AutoMapHelper.UpdateState(filexml, "POI选取失败");
                }
                return false;
            }    
        }
    

        
        }
    }

