using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;

namespace SMGI.Plugin.GeneralEdit
{
    public class SnapMove : SMGITool
    {
        private INewLineFeedback _feedback;

        public SnapMove()
        {
            m_caption = "捕点移动";
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

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1) return;
            if (m_Application.ActiveView.FocusMap.SelectionCount != 1) return; //限定移动一个要素

            if (_feedback == null)
            {
                _feedback = new NewLineFeedbackClass {Display = m_Application.ActiveView.ScreenDisplay};
                _feedback.Start(ToSnapedMapPoint(x, y));
            }
        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            if (_feedback != null) _feedback.MoveTo(ToSnapedMapPoint(x, y));
        }

        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            if (_feedback != null) _feedback.AddPoint(ToSnapedMapPoint(x, y));
        }

        public override void OnDblClick()
        {
            if (_feedback == null) return;
            var lineGeo = _feedback.Stop();
            _feedback = null;

            if (m_Application.ActiveView.FocusMap.SelectionCount != 1) return;
            if (lineGeo == null || lineGeo.IsEmpty) return;

            var pc = (IPointCollection) lineGeo;
            var dx = pc.Point[pc.PointCount - 1].X - pc.Point[0].X;
            var dy = pc.Point[pc.PointCount - 1].Y - pc.Point[0].Y;

            m_Application.EngineEditor.StartOperation();
            var feSe = (IEnumFeature)m_Application.ActiveView.FocusMap.FeatureSelection;
            IFeature fe;
            while ((fe = feSe.Next()) != null)
            {
                var geo = fe.ShapeCopy;
                ((ITransform2D)geo).Move(dx, dy);
                fe.Shape = geo;
                fe.Store();
            }
            m_Application.EngineEditor.StopOperation("捕点移动");
            m_Application.ActiveView.Refresh();
        }
    }
}
