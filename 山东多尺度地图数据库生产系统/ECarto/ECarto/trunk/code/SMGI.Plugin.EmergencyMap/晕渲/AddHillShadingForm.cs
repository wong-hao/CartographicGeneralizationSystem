using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using System.Xml.Linq;
using SMGI.Common;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class AddHillShadingForm : Form
    {
        /// <summary>
        /// 输入的DEM栅格数据
        /// </summary>
        public IRasterLayer DEMRasterLayer
        {
            get;
            protected set;
        }

        /// <summary>
        /// 方位角[0,360]
        /// </summary>
        public double Azimuth
        {
            get;
            protected set;
        }

        /// <summary>
        /// 高度角[0,90]
        /// </summary>
        public double Altitude
        {
            get;
            protected set;
        }

        /// <summary>
        /// 是否开启模拟阴影
        /// </summary>
        public bool InModelShadows
        {
            get;
            protected set;
        }

        /// <summary>
        /// Z因子（0,]
        /// </summary>
        public object ZFactor
        {
            get;
            protected set;
        }


        public AddHillShadingForm()
        {
            InitializeComponent();
        }

        private void AddHillShadingForm_Load(object sender, EventArgs e)
        {
            cbInputRaster.ValueMember = "Key";
            cbInputRaster.DisplayMember = "Value";

            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IRasterLayer;

            })).ToArray();
            if (lyrs != null && lyrs.Count() > 0)
            {
                foreach (var item in lyrs)
                {
                    var rasterLayer = item as IRasterLayer;
                    cbInputRaster.Items.Add(new KeyValuePair<IRasterLayer, string>(rasterLayer, rasterLayer.Name));
                }
            }

            try
            {
                XElement expertiseContent = ExpertiseDatabase.getContentElement(GApplication.Application);
                XElement hillShading = expertiseContent.Element("HillShading");

                tbAzimuth.Text = hillShading.Element("Azimuth").Value;//方位角;
                tbAltitude.Text = hillShading.Element("Altitude").Value;//高度角
                cbInModelShadows.Checked = (hillShading.Element("InModelShadows").Value == "1");//模拟阴影
                tbZFactor.Text = hillShading.Element("ZFactor").Value;//Z因子
            }
            catch
            {
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (cbInputRaster.Text == "")
            {
                MessageBox.Show("请指定输入栅格!");
                return;
            }

            double azimuth = 315, altitude = 45, zFactor = 1;
            if (!double.TryParse(tbAzimuth.Text, out azimuth) || azimuth < 0 || azimuth > 360)
            {
                MessageBox.Show("方位角输入不合法，请输入一个 0 到 360 度之间的正度数!");
                return;
            }
            if (!double.TryParse(tbAltitude.Text, out altitude) || altitude < 0 || altitude > 90)
            {
                MessageBox.Show("方位角输入不合法，请输入一个 0 到 90 度之间的正度数!");
                return;
            }
            if (!double.TryParse(tbZFactor.Text, out zFactor) || zFactor <= 0)
            {
                MessageBox.Show("方位角输入不合法，请输入一个大于 0 的正数!");
                return;
            }
            bool inMoelShadows = cbInModelShadows.Checked;

            var obj = (KeyValuePair<IRasterLayer, string>)cbInputRaster.SelectedItem;
            DEMRasterLayer = obj.Key;

            Azimuth = azimuth;
            Altitude = altitude;
            InModelShadows = inMoelShadows;
            ZFactor = zFactor;

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }


    }
}
