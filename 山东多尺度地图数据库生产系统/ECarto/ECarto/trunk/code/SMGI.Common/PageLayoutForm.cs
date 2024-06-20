using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using SMGI.Common;


namespace SMGI.Common
{
    public partial class PageLayoutForm : Form, IPluginHost
    {
        public Control pluginParent
        {
            get
            {
                return toolStripContainer1.TopToolStripPanel;
            }
        }
        public HostType hostFormType
        {
            get
            {
                return HostType.PageLayoutFrom;
            }
        }
        public ContextMenuStrip contextMenu
        {
            get { return contextMenuStrip1; }
        }


        public IPageLayoutControl3 esriPageLayoutControl
        {
            get
            {
                return axPageLayoutControl1.Object as IPageLayoutControl3;
            }
        }
        public PageLayoutForm()
        {
            InitializeComponent();
        }

        private void PageLayoutForm_Load(object sender, EventArgs e)
        {


        }

        private void PageLayoutForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void axPageLayoutControl1_OnMouseMove(object sender, IPageLayoutControlEvents_OnMouseMoveEvent e)
        {
            //this.toolTip1.SetToolTip(this.axPageLayoutControl1,e.pageX+" "+e.pageY);

        }

        private void axPageLayoutControl1_OnMouseDown(object sender, IPageLayoutControlEvents_OnMouseDownEvent e)
        {
            if (e.button == 4)
            {
                axPageLayoutControl1.Pan();
            }
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
