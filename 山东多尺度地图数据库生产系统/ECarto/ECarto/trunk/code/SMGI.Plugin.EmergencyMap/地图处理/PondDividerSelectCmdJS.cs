using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using System.Xml.Linq;
using ESRI.ArcGIS.Geometry;
using System.Data;
using ESRI.ArcGIS.Display;

namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 根据池塘（GB=230102）的选取状态，更新与其相关（包含）的池塘分割线（GB=230102）的显示状态：详见文档《快速制图软件需求讨论后0904》
    /// </summary>
    public class PondDividerSelectCmdJS : SMGICommand
    {
        public PondDividerSelectCmdJS()
        {
            m_caption = "池塘分割线处理";
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
            int pondGB = 230102, pondDividerGB = 230102;
            string selStateFN = "selectstate";
            string gbFN = "GB";

            IFeatureLayer hydaLayer = null, hfclLayer = null;
            int hydaSelStateIndex = -1, hfclSelStateIndex = -1;
            int hydaGBIndex = -1, hfclGBIndex = -1;
            #region 获取相关图层
            string fcName = "HYDA";
            hydaLayer = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);
            })).FirstOrDefault() as IFeatureLayer;
            if (hydaLayer == null)
            {
                MessageBox.Show(string.Format("未找到要素类{0}！", fcName));
                return;
            }
            hydaSelStateIndex = hydaLayer.FeatureClass.FindField(selStateFN);
            if (hydaSelStateIndex == -1)
            {
                MessageBox.Show(string.Format("要素类【{0}】中找不到字段【{1}】", fcName, selStateFN));
                return;
            }
            hydaGBIndex = hydaLayer.FeatureClass.FindField(gbFN);
            if (hydaGBIndex == -1)
            {
                MessageBox.Show(string.Format("要素类【{0}】中找不到字段【{1}】", fcName, gbFN));
                return;
            }

            fcName = "HFCL";
            hfclLayer = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);
            })).FirstOrDefault() as IFeatureLayer;
            if (hfclLayer == null)
            {
                MessageBox.Show(string.Format("未找到要素类{0}！", fcName));
                return;
            }
            hfclSelStateIndex = hfclLayer.FeatureClass.FindField(selStateFN);
            if (hfclSelStateIndex == -1)
            {
                MessageBox.Show(string.Format("要素类【{0}】中找不到字段【{1}】", fcName, selStateFN));
                return;
            }
            hfclGBIndex = hfclLayer.FeatureClass.FindField(gbFN);
            if (hfclGBIndex == -1)
            {
                MessageBox.Show(string.Format("要素类【{0}】中找不到字段【{1}】", fcName, gbFN));
                return;
            }
            #endregion

            m_Application.EngineEditor.StartOperation();
            try
            {
                using (var wo = m_Application.SetBusy())
                {
                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = string.Format("GB = {0}", pondGB);
                    IFeatureCursor hydaCursor = hydaLayer.FeatureClass.Search(qf, true);
                    IFeature hydaFe = null;
                    while ((hydaFe = hydaCursor.NextFeature()) != null)//遍历池塘，处理相关的池塘分割线
                    {
                        if (hydaFe.Shape == null || hydaFe.Shape.IsEmpty)
                            continue;
                        IPolygon pondPolygon = hydaFe.ShapeCopy as IPolygon;
                        if (pondPolygon == null || pondPolygon.IsEmpty)
                            continue;

                        bool isSelected = false;//池塘的选取状态
                        object selStateVal = hydaFe.get_Value(hydaSelStateIndex);
                        if (Convert.IsDBNull(selStateVal))//已选取
                        {
                            isSelected = true;
                        }

                        #region 处理该池塘相关联的池塘分割线
                        ISpatialFilter sf = new SpatialFilterClass();
                        sf.WhereClause = string.Format("GB={0}", pondDividerGB);
                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;//包含
                        sf.Geometry = pondPolygon;

                        IFeatureCursor hfclCursor = hfclLayer.FeatureClass.Update(sf, true);
                        IFeature hfclFe = null;
                        while ((hfclFe = hfclCursor.NextFeature()) != null)
                        {
                            if (isSelected)
                            {
                                hfclFe.set_Value(hfclSelStateIndex, DBNull.Value);//设置为选取状态
                            }
                            else
                            {
                                hfclFe.set_Value(hfclSelStateIndex, "未选取");//设置为未选取状态
                            }

                            hfclCursor.UpdateFeature(hfclFe);
                        }
                        hfclCursor.Flush();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(hfclCursor);
                        #endregion
                    }
                    Marshal.ReleaseComObject(hydaCursor);
                }

                MessageBox.Show("已完成池塘分割线处理！");

                m_Application.EngineEditor.StopOperation("池塘分割线处理");

                m_Application.ActiveView.Refresh();
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
    }
}
