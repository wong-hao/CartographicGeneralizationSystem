using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using System.Runtime.InteropServices;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmEleMove : Form
    {
        IGroupElement ge0 = null;
        public FrmEleMove()
        {
            InitializeComponent();
        }

        private void btUp_Click(object sender, EventArgs e)
        {
            double dis =double.Parse (txtStep.Text);
            ClipElement.MoveClipGroupElement1(dis, 0, GApplication.Application);
        }

        private void btRight_Click(object sender, EventArgs e)
        {
            double dis = double.Parse(txtStep.Text);
            ClipElement.MoveClipGroupElement1(0, dis, GApplication.Application);
        }

        private void btDown_Click(object sender, EventArgs e)
        {
            double dis = double.Parse(txtStep.Text);
            ClipElement.MoveClipGroupElement1(-dis, 0, GApplication.Application);
        }

        private void btLeft_Click(object sender, EventArgs e)
        {
            double dis = double.Parse(txtStep.Text);
            ClipElement.MoveClipGroupElement1(0, -dis, GApplication.Application);
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            GApplication app = GApplication.Application;
            IGroupElement ge = ClipElement.getClipGroupElement(app.MapControl.ActiveView.GraphicsContainer);
            if (ge != null)
            {
                //清理裁切图形元素
                app.MapControl.ActiveView.GraphicsContainer.DeleteElement(ge as IElement);
            }

            if (ge0 == null)
                return;

            app.MapControl.ActiveView.GraphicsContainer.AddElement(ge0 as IElement, 0);
            app.MapControl.ActiveView.Refresh();
            Close();
        }

        private void FrmEleMove_Load(object sender, EventArgs e)
        {
            var groupEl= ClipElement.getClipGroupElement(GApplication.Application.MapControl.ActiveView.GraphicsContainer);
            ge0 = (groupEl as IClone).Clone() as IGroupElement;
        }

        private void FrmEleMove_FormClosed(object sender, FormClosedEventArgs e)
        {
            Marshal.ReleaseComObject(ge0);
        }
    }
}
