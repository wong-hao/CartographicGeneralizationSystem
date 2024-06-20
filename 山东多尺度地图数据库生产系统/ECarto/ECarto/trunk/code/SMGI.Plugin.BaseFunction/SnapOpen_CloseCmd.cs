using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.BaseFunction
{
    public class SnapOpen_CloseCmd:SMGI.Common.SMGICommand
    {
        IEngineEditor editor;
        public SnapOpen_CloseCmd() 
        { 
            //打开/关闭捕捉
            editor = new EngineEditorClass();
        }

        public override void OnClick()
        {
            List<LayerSnap> lls = new List<LayerSnap>();
            var map = m_Application.Workspace.Map;
            var layers = map.get_Layers();
            layers.Reset();
            ILayer l = null;
            while ((l = layers.Next()) != null)
            {
                if (l is IFeatureLayer && !(l is IFDOGraphicsLayer))
                    lls.Add(new LayerSnap(l as IFeatureLayer, editor));
            }
            for (int i = 0; i < lls.Count; i++) {
                LayerSnap ls = lls[i];
                ls.VertexSnapping = !ls.VertexSnapping;
                ls.EdgeSnapping = !ls.EdgeSnapping;
                ls.EndSnapping = !ls.EndSnapping;

            }
            base.OnClick();
        }
        public override bool Enabled
        {
            get
            {
                return m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }

    }
}
