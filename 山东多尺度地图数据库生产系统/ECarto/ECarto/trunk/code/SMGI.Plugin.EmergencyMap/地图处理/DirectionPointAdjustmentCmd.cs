using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using System.Data;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geometry;
using System.Xml.Linq;

namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 有向点方向调整
    /// </summary>
    [SMGIAutomaticCommand]
    public class DirectionPointAdjustmentCmd : SMGI.Common.SMGICommand
    {
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
            string dbPath = m_Application.Template.Root + @"\专家库\有向点规则.mdb";
            string directRuleTable = "有向点规则";
            DataTable ruleDataTable = CommonMethods.ReadToDataTable(dbPath, directRuleTable);
            if (ruleDataTable.Rows.Count < 1)
            {
                MessageBox.Show(string.Format("有向点规则表【{0}】为空！", directRuleTable));
                return;
            }

            try
            {
                m_Application.EngineEditor.StartOperation();


                using (WaitOperation wo = GApplication.Application.SetBusy())
                {
                    AdjustmentDirectionPoint(ruleDataTable, wo);
                }

                m_Application.EngineEditor.StopOperation("有向点方向调整");

                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);

                MessageBox.Show("有向点方向调整完成！");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                m_Application.EngineEditor.AbortOperation();

                MessageBox.Show(ex.Message);
            }
        }

        protected override bool DoCommand(XElement args, Action<string> messageRaisedAction)
        {
            try
            {
                string dbPath = m_Application.Template.Root + @"\专家库\有向点规则.mdb";
                string directRuleTable = "有向点规则";
                DataTable ruleDataTable = CommonMethods.ReadToDataTable(dbPath, directRuleTable);
                if (ruleDataTable.Rows.Count < 1)
                {
                    return false;
                }

                AdjustmentDirectionPoint(ruleDataTable);

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

        public static void AdjustmentDirectionPoint(DataTable ruleDataTable, WaitOperation wo = null)
        {
            //有向点要素类
            IFeatureClass fc = null;
            IFeatureCursor feCursor = null;
            IFeature fe = null;
            //关联要素类
            IFeatureClass conFC = null;

            IQueryFilter qf = new QueryFilterClass();
            for (int i = 0; i < ruleDataTable.Rows.Count; i++)
            {
                string lyrName = ruleDataTable.Rows[i]["目标图层"].ToString().Trim().ToUpper();
                string filter = ruleDataTable.Rows[i]["有向点SQL文本"].ToString().Trim();
                double orginAngle = 0;//有向点符号初始角度值
                double.TryParse(ruleDataTable.Rows[i]["符号初始角度值"].ToString().Trim(), out orginAngle);

                string angleFN = ruleDataTable.Rows[i]["角度字段名"].ToString().Trim();

                string conLyrName = ruleDataTable.Rows[i]["关联图层"].ToString().Trim().ToUpper();
                string conFilter = ruleDataTable.Rows[i]["关联要素SQL文本"].ToString().Trim();
                double conAngle = 0;//有向点与关联要素的角度值关系
                double.TryParse(ruleDataTable.Rows[i]["关联角度值"].ToString().Trim(), out conAngle);


                var feLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) &&
                    ((l as IGeoFeatureLayer).FeatureClass.AliasName.Trim().ToUpper() == lyrName)).FirstOrDefault() as IFeatureLayer;
                if (feLayer == null)
                {
                    //throw new Exception(string.Format("当前工作空间中没有找到图层【{0}】!", lyrName));
                    System.Diagnostics.Trace.WriteLine(string.Format("当前工作空间中没有找到图层【{0}】!", lyrName));
                    continue;
                }
                fc = feLayer.FeatureClass;

                angleFN = GApplication.Application.TemplateManager.getFieldAliasName(angleFN, fc.AliasName);
                int angleIndex = fc.FindField(angleFN);
                if (angleIndex == -1)
                {
                    throw new Exception(string.Format("图层【{0}】中没有找到字段【{1}】!", lyrName, angleFN));
                }

                feLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) &&
                    ((l as IGeoFeatureLayer).FeatureClass.AliasName.Trim().ToUpper() == conLyrName)).FirstOrDefault() as IFeatureLayer;
                if (feLayer == null)
                {
                    //throw new Exception(string.Format("当前工作空间中没有找到图层【{0}】!", conLyrName));
                    System.Diagnostics.Trace.WriteLine(string.Format("当前工作空间中没有找到图层【{0}】!", conLyrName));
                    continue;
                }
                conFC = feLayer.FeatureClass;


                qf.WhereClause = filter;
                feCursor = fc.Search(qf, false);
                while ((fe = feCursor.NextFeature()) != null)
                {
                    if(wo != null)
                        wo.SetText("正在处理要素【" + fe.OID + "】.......");

                    IRelationalOperator ro = fe.Shape as IRelationalOperator;

                    SpatialFilterClass sf = new SpatialFilterClass();
                    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    sf.Geometry = fe.Shape;
                    sf.WhereClause = conFilter;
                    IFeatureCursor conFeCursor = conFC.Search(sf, true);
                    IFeature conFe = null;
                    if ((conFe = conFeCursor.NextFeature()) != null)
                    {
                        ISegmentCollection segs = conFe.Shape as ISegmentCollection;
                        if (segs == null)
                            continue;

                        ILine lineDir = null;

                        for (int s = 0; s < segs.SegmentCount; s++)
                        {
                            ILine line = segs.get_Segment(s) as ILine;
                            if (line == null || line.IsEmpty)
                                continue;

                            ISegmentCollection gc = new PolylineClass();
                            gc.AddSegment(segs.get_Segment(s));
                            if (ro.Within(gc as IGeometry))
                            {
                                lineDir = line;
                                break;
                            }
                        }

                        if (lineDir != null)
                        {
                            fe.set_Value(angleIndex, GetAngle(lineDir, orginAngle, conAngle));
                            fe.Store();
                        }

                    }
                    Marshal.ReleaseComObject(conFeCursor);
                }
                Marshal.ReleaseComObject(feCursor);
            }
        }

        public static double GetAngle(ILine line, double orginAngle, double conAngle)
        {
            Double radian = line.Angle;
            Double angle = radian * 180 / Math.PI;
            angle = orginAngle + conAngle + angle;
            while (angle < 0)
            {
                angle = angle + 360;
            }

            angle = angle % 360;

            return angle;
        }
    }
}
