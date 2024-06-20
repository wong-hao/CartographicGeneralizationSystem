#if zhouqi
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.GeneralEdit
{
    class RoadLayerSelect : SMGI.Common.SMGICommand, IToolControl
    {
        private ComboBox cb;
        private Dictionary<string, ILayer> dclayers;

        public RoadLayerSelect()
        {
            m_caption = "当前编辑图层";
            m_message = "选择当前编辑图层";
            m_toolTip = "选择当前编辑图层";
            m_category = "基础编辑";
            m_bitmap = AutoResource.图层管理;
            cb = new ComboBox();
            cb.DropDownStyle = ComboBoxStyle.DropDownList;
            cb.Font = new System.Drawing.Font("微软雅黑", 9.0F);
            cb.Size = new System.Drawing.Size(90, 24);
            cb.SelectedIndexChanged += new EventHandler(cb_SelectedIndexChanged);
            cb.Click += new EventHandler(cb_Click);
            cb.DropDownWidth = 200;
            
            dclayers = new Dictionary<string, ILayer>();
        }

        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
        }

        void cb_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_Application.Workspace.CurrentLayer = cb.SelectedValue as LayerInfo;
        }

        void cb_Click(object sender, EventArgs e)
        {
            this.OnClick();
            this.cb.BackColor = System.Drawing.Color.AliceBlue;
        }

        #region Overridden Class Methods
        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null;
            }
        }
        public override void OnClick()
        {
            //base.OnClick();
            if (m_Application.Workspace != null)
            {
                LayerInfo[] lyrs = m_Application.Workspace.LayerManager.GetLayer(findLyr);
                if (lyrs == null || lyrs.Length < 1)
                    return;
                dclayers = new Dictionary<string, LayerInfo>();
                foreach (var item in lyrs)
                {
                    if (!dclayers.ContainsKey(item.ToString()))
                        dclayers.Add(item.ToString(), item);
                }

                BindingSource bs = new BindingSource();
                bs.DataSource = dclayers;
                this.cb.DataSource = bs;
                this.cb.DisplayMember = "Key";
                this.cb.ValueMember = "Value";

                if (this.cb.Items.Count > 0)
                {
                    this.cb.SelectedIndex = 0;
                    cb_SelectedIndexChanged(this.cb, new EventArgs());
                }
            }
        }

        bool findLyr(LayerInfo l)
        {
            return l.Layer is IFeatureLayer && /*l.Layer.Visible == true &&*/
                (l.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline;
        }

        #endregion

        #region IToolControl 成员
        public bool OnDrop(esriCmdBarType barType)
        {
            return true;
            //throw new NotImplementedException();
        }

        public void OnFocus(ICompletionNotify complete)
        {
            //throw new NotImplementedException();
        }

        public int hWnd
        {
            get { return cb.Handle.ToInt32(); }
        }
        #endregion
    }

}
#endif