// Copyright 2012 ESRI
// 
// All rights reserved under the copyright laws of the United States
// and applicable international laws, treaties, and conventions.
// 
// You may freely redistribute and use this sample code, with or
// without modification, provided you include the original copyright
// notice and use restrictions.
// 
// See the use restrictions at <your ArcGIS install location>/DeveloperKit10.1/userestrictions.txt.
// 

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.ADF.CATIDs;
using SMGI.Common;

namespace SMGI.Plugin.BaseFunction
{
    public partial class SnapEditorSetForm : Form
    {
        IEngineEditor editor;
        IEngineSnapEnvironment snapEnvironment;
        GApplication application;
        BindingSource bs;
        public SnapEditorSetForm(GApplication app)
        {
            InitializeComponent();
            application = app;

            //get the snapEnvironment
            editor = new EngineEditorClass();
            snapEnvironment = editor as IEngineSnapEnvironment;


            snapTolerance.Text = snapEnvironment.SnapTolerance.ToString();
            snapTolUnits.SelectedIndex = (int)snapEnvironment.SnapToleranceUnits;
            SetBinding();
        }

        void SetBinding()
        {
            if (application.Workspace == null)
                return;

            BindingList<LayerSnap> sl = new BindingList<LayerSnap>();
            
            var map = application.Workspace.Map;
            var ls = map.get_Layers();
            ls.Reset();
            ILayer l = null;
            while ((l = ls.Next())!=null)
            {
                if(l is IFeatureLayer && !(l is IFDOGraphicsLayer))
                    sl.Add(new LayerSnap(l as IFeatureLayer, editor));
            }

            bs = new BindingSource();
            bs.DataSource = sl;
            dgFeatrueLayer.DataSource = bs;
            dgFeatrueLayer.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }


        #region Snap Tips and Tolerance Handlers

        private void snapTolerance_TypeValidationEventHandler(object sender, TypeValidationEventArgs e)
        {
            try
            {
                snapEnvironment.SnapTolerance = Convert.ToDouble(snapTolerance.Text);
            }
            catch
            {
                snapTolerance.Text = snapEnvironment.SnapTolerance.ToString();
            }
        }

        private void snapTolUnits_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                snapEnvironment.SnapToleranceUnits = (esriEngineSnapToleranceUnits)snapTolUnits.SelectedIndex;
            }
            catch
            {
                snapTolUnits.SelectedIndex = (int)snapEnvironment.SnapToleranceUnits;
            }
        }
        #endregion

        private void dgFeatrueLayer_KeyPress(object sender, KeyPressEventArgs e)
        {
            var c = e.KeyChar;
            LayerSnap f = bs.Current as LayerSnap;
            LayerSnap cs = f;
            do
            {
                bs.MoveNext();
                if (cs == bs.Current)
                    bs.MoveFirst();
                cs = bs.Current as LayerSnap;

                if (cs.LayerName.ToLower()[0] == c || cs.LayerName.ToUpper()[0] == c)
                {
                    return;
                }

            }
            while (cs != f);
        }

        private void snapTolerance_TextChanged(object sender, EventArgs e)
        {
            try
            {
                snapEnvironment.SnapTolerance = Convert.ToDouble(snapTolerance.Text);
            }
            catch
            {
                snapTolerance.Text = snapEnvironment.SnapTolerance.ToString();
            }
        }

        private void dgFeatrueLayer_ColumnHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var col = dgFeatrueLayer.Columns[e.ColumnIndex];
            if (col.HeaderText == "图层名")
            {
                return;
            }
            foreach (var item in bs)
            {
                if (col.HeaderText == "节点")
                    (item as LayerSnap).VertexSnapping = !(item as LayerSnap).VertexSnapping;
                if (col.HeaderText == "边")
                    (item as LayerSnap).EdgeSnapping = !(item as LayerSnap).EdgeSnapping;
                if (col.HeaderText == "端点")
                    (item as LayerSnap).EndSnapping = !(item as LayerSnap).EndSnapping;
            }
            dgFeatrueLayer.Refresh();
        }

        private void SnapEditor_Load(object sender, EventArgs e)
        {

        }
    }

    public class LayerSnap
    {
        IFeatureLayer layer;
        IEngineEditor editor;
        public LayerSnap(IFeatureLayer l, IEngineEditor e)
        {
            layer = l;
            editor = e;
        }

        private bool IsCurrentSnapAgent(IEngineFeatureSnapAgent snapAgent)
        {
            if (snapAgent == null)
            {
                return false;
            }

            if ((layer as IDataLayer2).InWorkspace((snapAgent.FeatureClass as IDataset).Workspace)
              && snapAgent.FeatureClass.FeatureClassID == layer.FeatureClass.FeatureClassID)
            {
                return true;
            }
            return false;
        }

        private IEngineFeatureSnapAgent CurrentSnapAgent
        {
            get
            {
                IEngineSnapEnvironment env = editor as IEngineSnapEnvironment;

                for (int i = 0; i < env.SnapAgentCount; i++)
                {
                    IEngineFeatureSnapAgent snapAgent = env.get_SnapAgent(i) as IEngineFeatureSnapAgent;
                    if (IsCurrentSnapAgent(snapAgent))
                    {
                        return snapAgent;
                    }
                }
                return null;
            }
        }

        private void SetSnapAgent(bool vertex, bool edge, bool end)
        {
            IEngineFeatureSnapAgent cs = null;
            int currentIdx = -1;
            IEngineSnapEnvironment env = editor as IEngineSnapEnvironment;

            List<IEngineSnapAgent> sas = new List<IEngineSnapAgent>();
            for (int i = 0; i < env.SnapAgentCount; i++)
            {
                IEngineSnapAgent snapAgent = env.get_SnapAgent(i);
                if (IsCurrentSnapAgent(snapAgent as IEngineFeatureSnapAgent))
                {
                    cs = snapAgent as IEngineFeatureSnapAgent;
                    currentIdx = i;
                }
                else
                {
                    sas.Add(snapAgent);
                }
            }

            if (cs == null)
            {
                if (!vertex && !edge && !end)
                {
                    return;
                }
                cs = new EngineFeatureSnapClass
                {
                    FeatureClass = layer.FeatureClass,
                    HitType = (esriGeometryHitPartType)GetSnapType(vertex, edge, end)
                };
                env.AddSnapAgent(cs);
                return;
            }
            else
            {
                env.RemoveSnapAgent(currentIdx);
                if (!vertex && !edge && !end)
                {
                    return;
                }
                env.AddSnapAgent(new EngineFeatureSnapClass
                {
                    FeatureClass = layer.FeatureClass,
                    HitType = (esriGeometryHitPartType)GetSnapType(vertex, edge, end)
                });
            }

        }

        private int GetSnapType(bool vertex, bool edge, bool end)
        {
            int tp = 0;
            if (vertex)
                tp = tp | (int)esriGeometryHitPartType.esriGeometryPartVertex;
            if (edge)
                tp = tp | (int)esriGeometryHitPartType.esriGeometryPartBoundary;
            if (end)
                tp = tp | (int)esriGeometryHitPartType.esriGeometryPartEndpoint;
            return tp;
        }
        [DisplayName("图层名")]
        public string LayerName
        {
            get { return layer.Name; }
        }
        [DisplayName("节点")]
        public bool VertexSnapping
        {
            get
            {
                var s = CurrentSnapAgent;
                if (s == null)
                {
                    return false;
                }
                else
                {
                    return (s.HitType & esriGeometryHitPartType.esriGeometryPartVertex) != 0;
                }
            }
            set
            {
                SetSnapAgent(value, EdgeSnapping, EndSnapping);
            }
        }
        [DisplayName("边")]
        public bool EdgeSnapping
        {
            get
            {
                var s = CurrentSnapAgent;
                if (s == null)
                {
                    return false;
                }
                else
                {
                    return (s.HitType & esriGeometryHitPartType.esriGeometryPartBoundary) != 0;
                }
            }
            set
            {
                SetSnapAgent(VertexSnapping, value, EndSnapping);
            }
        }
        [DisplayName("端点")]
        public bool EndSnapping
        {
            get
            {
                var s = CurrentSnapAgent;
                if (s == null)
                {
                    return false;
                }
                else
                {
                    return (s.HitType & esriGeometryHitPartType.esriGeometryPartEndpoint) != 0;
                }
            }
            set
            {
                SetSnapAgent(VertexSnapping, EdgeSnapping, value);
            }
        }


    }

    public class SnapCommand : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            SnapEditorSetForm ed = new SnapEditorSetForm(m_Application);
            ed.ShowDialog(m_Application.MainForm as IWin32Window);
        }
    }
}