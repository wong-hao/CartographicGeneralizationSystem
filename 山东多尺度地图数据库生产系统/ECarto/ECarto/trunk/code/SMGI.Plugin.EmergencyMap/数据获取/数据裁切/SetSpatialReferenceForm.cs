using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using System.IO;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class SetSpatialReferenceForm : Form
    {
        private GApplication _app;

        public ISpatialReference targetSpatialReference
        {
            get;
            set;
        }

        public SetSpatialReferenceForm(GApplication app)
        {
            InitializeComponent();

            _app = app;
        }

        private void SetSpatialReferenceForm_Load(object sender, EventArgs e)
        {
            string prjFolderPath = GApplication.ExePath + @"\..\Projection";
            DirectoryInfo dir = new DirectoryInfo(prjFolderPath);
            var files = dir.GetFiles();

            for (int i = 0; i < files.Length; i++)
            {
                cbProjection.Items.Add(new KeyValuePair<string, string>(files[i].FullName, files[i].Name));
            }
            cbProjection.ValueMember = "Key";
            cbProjection.DisplayMember = "Value";

            if (cbProjection.Items.Count > 0)
            {
                string srFileName = CommonMethods.clipSpatialRefFileName;
                srFileName = srFileName.Substring(srFileName.LastIndexOf("\\") + 1);

                int index = 0;
                for (int i = 0; i < cbProjection.Items.Count; ++i)
                {
                    var item = (KeyValuePair<string, string>)cbProjection.Items[i];
                    if (item.Value == srFileName)
                    {
                        index = i;
                        break;
                    }
                }

                cbProjection.SelectedIndex = index;
            }
            var selectedItem = (KeyValuePair<string, string>)cbProjection.SelectedItem;
            setCurrentCoordinateSystemInfo(selectedItem.Key);
        }

        private void btnSelectPrj_Click(object sender, EventArgs e)
        {
            OpenFileDialog pOpenFileDialog = new OpenFileDialog();
            pOpenFileDialog.Filter = "坐标参考文件(*.prj)|*.prj";
            pOpenFileDialog.Title = "选择目标空间参考";
            pOpenFileDialog.InitialDirectory = GApplication.RootPath + @"\Projection";
            DialogResult pDiglogR = pOpenFileDialog.ShowDialog();
            if (pDiglogR == DialogResult.OK)
            {
                cbProjection.SelectedIndex = cbProjection.Items.Add(new KeyValuePair<string, string>(pOpenFileDialog.FileName, pOpenFileDialog.FileName.Substring(pOpenFileDialog.FileName.LastIndexOf("\\") + 1)));
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            var selectedItem = (KeyValuePair<string, string>)cbProjection.SelectedItem;
            string prjFullName = selectedItem.Key;
            //  ISpatialReferenceFactory pSpatialRefFactory = new SpatialReferenceEnvironmentClass();
            //  targetSpatialReference = pSpatialRefFactory.CreateESRISpatialReferenceFromPRJFile(prjFullName);
            if (null == targetSpatialReference)
            {
                MessageBox.Show("无效的投影文件!");
                return;
            }
            //   setCurrentCoordinateSystemInfo(prjFullName);
            //更新裁切数据的空间参考
            CommonMethods.clipSpatialRefFileName = prjFullName;

            //更新裁切元素
            ClipElement.updateClipGroupElement(_app, targetSpatialReference);

            DialogResult = DialogResult.OK;
        }

        private void setCurrentCoordinateSystemInfo(string prjFullName)
        {
            textBox.Clear();
            Dictionary<string, string> projInfo = new Dictionary<string, string>();
            ISpatialReferenceFactory pSpatialRefFactory = new SpatialReferenceEnvironmentClass();
            targetSpatialReference = pSpatialRefFactory.CreateESRISpatialReferenceFromPRJFile(prjFullName);
            if (null == targetSpatialReference)
            {
                MessageBox.Show("无效的投影文件!");
                return;
            }
            //   textBox.AppendText("");
            string projname = prjFullName.Substring(prjFullName.LastIndexOf("\\") + 1);
            projInfo.Add("Name", projname);

            ISpatialReferenceAuthority proAu = (targetSpatialReference as ISpatialReferenceAuthority);
            if (proAu != null)
            {
                projInfo.Add("WKID", proAu.Code.ToString());
                projInfo.Add("Authority", proAu.AuthorityName);
            }

            IProjectedCoordinateSystem2 pProjectionsys = (targetSpatialReference as IProjectedCoordinateSystem2);//投影坐标系
            if (pProjectionsys != null)
            {
                string sProjection = pProjectionsys.Projection.Name;//投影坐标系
                projInfo.Add("Projection", sProjection);
                projInfo.Add("False_Easting", pProjectionsys.FalseEasting.ToString());
                projInfo.Add("False_Northing", pProjectionsys.FalseNorthing.ToString());

                projInfo.Add("Central_Meridian", pProjectionsys.get_CentralMeridian(true).ToString());
                insertDic(projInfo, pProjectionsys, "Scale_Factor");
                insertDic(projInfo, pProjectionsys, "Latitude_Of_Origin");
                insertDic(projInfo, pProjectionsys, "Standard_Parallel_1");
                insertDic(projInfo, pProjectionsys, "Standard_Parallel_2");
                //projInfo.Add("Scale_Factor", pProjectionsys.ScaleFactor.ToString());
                // projInfo.Add("Latitude_Of_Origin", pProjectionsys.LatitudeOfOrigin.ToString());

                ILinearUnit pLinearUnit = pProjectionsys.CoordinateUnit;//计量单位
                string sUnit = pLinearUnit.Name + "(" + pLinearUnit.MetersPerUnit + ")";
                projInfo.Add("Linear Unit", sUnit);

            }

            IGeographicCoordinateSystem pGeoCoordSys;
            if (pProjectionsys != null)
            {
                pGeoCoordSys = (targetSpatialReference as IProjectedCoordinateSystem).GeographicCoordinateSystem;//地理坐标

            }
            else
            {
                pGeoCoordSys = targetSpatialReference as IGeographicCoordinateSystem;//地理坐标
            }
            if (pGeoCoordSys != null)
            {
                projInfo.Add("Geographic Coordinate System", pGeoCoordSys.Name);
                IAngularUnit pLinearUnit = pGeoCoordSys.CoordinateUnit;//计量单位
                string sUnit = pLinearUnit.Name + "(" + pLinearUnit.RadiansPerUnit + ")";
                projInfo.Add("Angular  Unit", sUnit);
                string sMeridia = pGeoCoordSys.PrimeMeridian.Name + "(" + pGeoCoordSys.PrimeMeridian.Longitude + ")";
                projInfo.Add("Prime Meridia", sMeridia);
                projInfo.Add("Datum", pGeoCoordSys.Datum.Name);
                projInfo.Add("Spheroid", pGeoCoordSys.Datum.Spheroid.Name);
                projInfo.Add("Semimajor Axis", pGeoCoordSys.Datum.Spheroid.SemiMajorAxis.ToString());
                projInfo.Add("Semiminor Axis", pGeoCoordSys.Datum.Spheroid.SemiMinorAxis.ToString());
                projInfo.Add("Inverse Flattening", pGeoCoordSys.Datum.Spheroid.Flattening.ToString());
            }

            foreach (var item in projInfo)
            {
                var result2 = item.Key + ":" + item.Value;
                textBox.AppendText(result2 + "\r\n");

                if (item.Key == "Authority" || item.Key == "Linear Unit")
                {
                    textBox.AppendText("\r\n");
                }
            }

        }
        private void insertDic(Dictionary<string, string> projInfo, IProjectedCoordinateSystem2 pProjectionsys, string para)
        {
            try
            {
                switch (para)
                {
                    case "Scale_Factor":
                        projInfo.Add("Scale_Factor", pProjectionsys.ScaleFactor.ToString());
                        break;
                    case "Latitude_Of_Origin":
                        projInfo.Add("Latitude_Of_Origin", pProjectionsys.LatitudeOfOrigin.ToString());
                        break;
                    case "Standard_Parallel_1":
                        projInfo.Add("Standard_Parallel_1", pProjectionsys.StandardParallel1.ToString());
                        break;
                    case "Standard_Parallel_2":
                        projInfo.Add("Standard_Parallel_2", pProjectionsys.StandardParallel2.ToString());
                        break;
                }
            }
            catch (Exception ex) { }
        }
        private void cbProjection_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = (KeyValuePair<string, string>)cbProjection.SelectedItem;
            setCurrentCoordinateSystemInfo(selectedItem.Key);
        }
    }
}
