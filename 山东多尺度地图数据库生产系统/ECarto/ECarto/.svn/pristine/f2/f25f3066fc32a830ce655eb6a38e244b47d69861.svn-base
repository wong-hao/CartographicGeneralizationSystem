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
namespace SMGI.Plugin.MapGeneralization
{
    public class POIAutoSelection : SMGI.Common.SMGICommand
    {
        public POIAutoSelection()
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
            IMap map = view.FocusMap;
            var editor = m_Application.EngineEditor;


            POIHelper _poiHelp = new POIHelper(m_Application);
            double _ratio = 0;
            ILayer _layer = null;
            List<string> _ruleNames = new List<string>();
            var dlg = new POISelectionForm(m_Application);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _ratio = dlg._selectRatio;
                _layer = dlg._selectLayer;
                _ruleNames = dlg._selectedRuleNames[_layer.Name];

                IQueryFilter qf = new QueryFilterClass();

                editor.StartOperation();

                List<ILayer> ls = new List<ILayer>();
                MultipointClass mp = new MultipointClass();
                List<int> features = new List<int>();
                List<int> feaCounts = new List<int>();

                EnvironmentHelper envHelper = new EnvironmentHelper(m_Application);
                POIHelper poiHelper = new POIHelper(m_Application);
                using (var wo = m_Application.SetBusy())
                {
                    #region
                    //遍历处理要素类别
                    for (int i = 0; i < _ruleNames.Count; i++)
                    {
                        IFeatureClass fc = (_layer as IFeatureLayer).FeatureClass;
                        qf.WhereClause = poiHelper.GetWhereClauseFromTemplate(_layer.Name, _ruleNames[i], envHelper.TPXElement);

                        wo.SetText("正在读取数据" + _ruleNames[0]);
                        IFeatureCursor fCuror = fc.Search(qf, true);

                        object miss = Type.Missing;
                        IFeature f = null;
                        while ((f = fCuror.NextFeature()) != null)
                        {
                            if (f.OID % 1000 == 0)
                                wo.SetText(string.Format("正在读取数据:" + _ruleNames[0] + "[{0}]", f.OID));

                            IPoint p = null;
                            p = f.ShapeCopy as IPoint;
                            mp.AddGeometry(p, ref miss, ref miss);
                            features.Add(f.OID);
                        }
                        feaCounts.Add(features.Count);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(fCuror);
                    }
                    #endregion
                    //得到选取结果
                    bool[] selected = _poiHelp.MultiPointSelection(mp, _ratio, wo);
                    if (selected == null)
                    {
                        return;
                    }
                    //处理
                    #region
                    for (int i = 0; i < _ruleNames.Count; i++)
                    {

                        wo.SetText("正在处理数据 :" + _ruleNames[i]);
                        IFeature f = null;
                        IFeatureClass fc = (_layer as IFeatureLayer).FeatureClass;
                        qf.WhereClause = poiHelper.GetWhereClauseFromTemplate(_layer.Name, _ruleNames[i], envHelper.TPXElement);
                        IFeatureCursor fCuror = fc.Update(qf, true);

                        Dictionary<int, bool> feaSelected = new Dictionary<int, bool>();
                        bool[] subSelected = new bool[feaCounts[i]];
                        List<int> subFeatures = new List<int>();

                        if (i == 0)
                        {
                            for (int j = 0; j < feaCounts[0]; j++)
                            {
                                subSelected[j] = selected[j];
                                subFeatures.Add(features[j]);
                            }
                        }
                        else
                        {
                            for (int j = feaCounts[i - 1]; j < feaCounts[i]; j++)
                            {
                                subSelected[j - feaCounts[i - 1]] = selected[j];
                                subFeatures.Add(features[j]);
                            }
                        }


                        for (int j = 0; j < subFeatures.Count; j++)
                        {
                            feaSelected.Add(subFeatures[j], subSelected[j]);
                        }

                        while ((f = fCuror.NextFeature()) != null)
                        {
                            if (f.OID % 1000 == 0)
                                wo.SetText(string.Format("正在读取数据:" + _ruleNames[i] + "[{0}]", f.OID));
                            if (!feaSelected[f.OID])
                                fCuror.DeleteFeature();
                        }
                    }
                    #endregion
                }
                editor.StopOperation("POI选取");
                view.Refresh();
            }
        }
       
     
       
    }
}
