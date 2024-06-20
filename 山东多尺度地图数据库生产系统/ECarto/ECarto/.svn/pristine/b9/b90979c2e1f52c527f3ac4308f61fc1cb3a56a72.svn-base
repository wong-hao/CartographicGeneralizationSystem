using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using SMGI.Common;


namespace SMGI.Plugin.GeneralEdit
{
    public class AttributesCommand:SMGI.Common.SMGICommand
    {
        public event EventHandler AttributesRefreshEvent;

        private AttributesForm attributesForm;

        public AttributesCommand()
        {
            m_caption = "属性窗口";
            m_category = "基础编辑";           
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
            app.MapControl.OnSelectionChanged += new EventHandler(MapControl_OnSelectionChanged);
            base.setApplication(app);
        }

        void MapControl_OnSelectionChanged(object sender, EventArgs e)
        {
            IEnumFeatureSetup enumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeatureSetup;
            enumFeature.AllFields = true;
            enumFeature.Recycling = false;
            if (enumFeature != null && AttributesRefreshEvent != null)
            {
                //激活事件
                AttributesRefreshEvent(this, EventArgs.Empty);
            }
        }

        public override void OnClick()
        {            
            if (attributesForm == null)
            {
                attributesForm = new AttributesForm(this, m_Application);
            }
            //attributesForm.FloatAt();

                //attributesForm.Show(m_Application.MainForm.DockPane);
                //attributesForm.DockState = UI.Docking.DockState.DockRight;
                m_Application.MainForm.ShowChild(attributesForm.Handle);

                IEnumFeatureSetup enumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeatureSetup;
                enumFeature.AllFields = true;
                enumFeature.Recycling = false;
                if (enumFeature != null && AttributesRefreshEvent != null)
                {
                    //激活事件
                    AttributesRefreshEvent(this, EventArgs.Empty);
                }
        }
        /*   
        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button == 1)
            {
                IEnvelope pEnv = m_Application.MapControl.TrackRectangle();
                if (pEnv.IsEmpty)
                {
                    //如果无拉框则点选
                    ESRI.ArcGIS.esriSystem.tagRECT r;
                    r.bottom = y + 5;
                    r.top = y - 5;
                    r.left = x - 5;
                    r.right = x + 5;
                    m_Application.ActiveView.ScreenDisplay.DisplayTransformation.TransformRect(pEnv, ref r,4);
                }
              
                m_Application.MapControl.Map.SelectByShape(pEnv, null, false);
                IEnumFeature enumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                (enumFeature as IEnumFeatureSetup).AllFields = true;
                (enumFeature as IEnumFeatureSetup).Recycling = false;
                if (enumFeature != null && AttributesRefreshEvent != null)
                {
                    //激活事件
                    AttributesRefreshEvent(this, EventArgs.Empty);
                    //刷新窗口
                    m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, m_Application.ActiveView.Extent);
                }
            }
        }
         * */
    }
}
