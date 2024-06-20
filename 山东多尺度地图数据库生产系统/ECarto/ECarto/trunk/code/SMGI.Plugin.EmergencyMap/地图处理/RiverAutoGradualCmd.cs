using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using SMGI.Common;
using System.Runtime.InteropServices;
using System.Diagnostics;
using SMGI.Common.Algrithm;
using System.Xml.Linq;

namespace SMGI.Plugin.EmergencyMap
{
    [SMGIAutomaticCommand]
    public class RiverAutoGradualCmd : SMGICommand
    {
        public RiverAutoGradualCmd()
        {
            m_caption = "河流渐变（自动）";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {

                    string envFileName = m_Application.Template.Content.Element("Expertise").Value;
                    string fileName = m_Application.Template.Root + @"\专家库\" + envFileName;
                    XDocument doc = XDocument.Load(fileName);
                    XElement expertiseContent = doc.Element("Expertise").Element("Content");
                    XElement autoGradual = expertiseContent.Element("RiverAutoGradual");
                    double startWidth = double.Parse(autoGradual.Element("StartWidth").Value);
                    double endWidth = double.Parse(autoGradual.Element("EndWidth").Value);
                    FrmRiverAutoGradual frmRiverAutoGrual = new FrmRiverAutoGradual();
                    frmRiverAutoGrual.StartWidth = startWidth;
                    frmRiverAutoGrual.EndWidth = endWidth;//设置初始参数

                    if (frmRiverAutoGrual.ShowDialog() == DialogResult.OK)
                    {

                        using (var wo = m_Application.SetBusy())
                        {
                            wo.SetText("正在进行河流自动渐变...");
                            try
                            {
                                startWidth = frmRiverAutoGrual.StartWidth;
                                endWidth = frmRiverAutoGrual.EndWidth;
                                startWidth *= 2.83;
                                endWidth *= 2.83;//转换成pt                 
                                m_Application.EngineEditor.StartOperation();
                                RiverAutoGradual rag = new RiverAutoGradual(m_Application);
                                string err = rag.autoGradualFork2(startWidth, endWidth);
                                if (err != "")
                                {
                                    MessageBox.Show(err);
                                    err = "";
                                }
                                //更新expertise中参数
                                #region 
                                if (double.Parse(autoGradual.Element("StartWidth").Value) != frmRiverAutoGrual.StartWidth || double.Parse(autoGradual.Element("EndWidth").Value) != frmRiverAutoGrual.EndWidth)
                                {
                                    autoGradual.SetElementValue("StartWidth", frmRiverAutoGrual.StartWidth.ToString());
                                    autoGradual.SetElementValue("EndWidth", frmRiverAutoGrual.EndWidth.ToString());
                                    doc.Save(fileName);
                                }
                                #endregion
                                m_Application.EngineEditor.StopOperation(this.Caption);
                                m_Application.ActiveView.Refresh();
                                MessageBox.Show("河流自动渐变成功！", "提示");
                                frmRiverAutoGrual.Close();
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Trace.WriteLine(ex.Message);
                                System.Diagnostics.Trace.WriteLine(ex.Source);
                                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                                MessageBox.Show(ex.ToString());
                            }
                        }

                    }


            
        }

        protected override bool DoCommand(XElement args, Action<string> messageRaisedAction)
        {
            
            try
            {
                #region 解析参数并执行
                messageRaisedAction("正在进行河流自动渐变...");
                XElement expertiseContent = ExpertiseDatabase.getContentElement(m_Application);
                XElement autoGradual = expertiseContent.Element("RiverAutoGradual");
                double startWidth = double.Parse(autoGradual.Element("StartWidth").Value);
                startWidth *= 2.83;
                double endWidth = double.Parse(autoGradual.Element("EndWidth").Value);
                endWidth *= 2.83;
                RiverAutoGradual rag = new RiverAutoGradual(m_Application);
                string err = rag.autoGradualFork2(startWidth, endWidth);
                if (err != "")
                {
                    messageRaisedAction(err);
                    err = "";
                    return false;
                }
                m_Application.Workspace.Save();
                return true;
                #endregion
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
