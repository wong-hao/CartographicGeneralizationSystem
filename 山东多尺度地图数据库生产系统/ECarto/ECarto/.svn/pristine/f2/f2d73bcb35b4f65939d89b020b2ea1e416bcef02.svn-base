using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
using SMGI.Common;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geoprocessing;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.MapGeneralization
{
    public partial class GeneralizeForm : Form
    {
        private double _refScale;
        public double RefScale
        {
            get
            {
                return _refScale;
            }
        }

        public string SimplifyAlgorithm
        {
            get
            {
                return BendComboBox.Text;
            }
        }

        public double SimplifyTolerance
        {
            get
            {
                return (double)numSimpTol.Value * 0.001 * RefScale;
            }
        }

        public bool EnableSmooth
        {
            get
            {
                return cbEnableSmooth.Checked;
            }
        }

        public string SmoothAlgorithm
        {
            get
            {
                return SmoothComboBox.Text;
            }
        }

        public double SmoothTolerance
        {
            get
            {
                if (SmoothComboBox.Text == "BEZIER_INTERPOLATION")
                {
                    return 0;
                }
                else
                {
                    return (double)numSmoothTol.Value * 0.001 * RefScale;
                }
            }
        }

        public GeneralizeForm(double refScale)
        {
            InitializeComponent();

            cmbReferScale.Text = GApplication.Application.ActiveView.FocusMap.ReferenceScale.ToString();


            string[] simplifyTypes = { "POINT_REMOVE", "BEND_SIMPLIFY" };
            foreach (var oneType in simplifyTypes)
            {
                BendComboBox.Items.Add(oneType);
                if (oneType == "POINT_REMOVE")
                {
                    BendComboBox.SelectedItem = oneType;
                }
            }
            BendComboBox.SelectedIndex = 1;

            numSimpTol.Controls[0].Visible = false;

            string[] soomthTypes = { "PAEK", "BEZIER_INTERPOLATION" };
            foreach (var oneType in soomthTypes)
            {
                SmoothComboBox.Items.Add(oneType);
                if (oneType == "PAEK")
                {
                    SmoothComboBox.SelectedItem = oneType;
                }
            }
            SmoothComboBox.SelectedIndex = 0;

            numSmoothTol.Controls[0].Visible = false;
        }


        public GeneralizeForm(double refScale, string simplyAlgorithm, double simplifyTolerance,bool enableSmooth, string smoothAlgorithm, double smoothTolerance)
        {
            InitializeComponent();

            cmbReferScale.Text = refScale.ToString();


            string[] simplifyTypes = { "POINT_REMOVE", "BEND_SIMPLIFY" };
            foreach (var oneType in simplifyTypes)
            {
                BendComboBox.Items.Add(oneType);
                if (oneType == "POINT_REMOVE")
                {
                    BendComboBox.SelectedItem = oneType;
                }
            }
            BendComboBox.SelectedIndex = BendComboBox.Items.IndexOf(simplyAlgorithm);

            numSimpTol.Controls[0].Visible = false;
            if (simplifyTolerance >= (double)numSimpTol.Minimum && simplifyTolerance <= (double)numSimpTol.Maximum && refScale > 0)
            {
                numSimpTol.Value = (decimal)(simplifyTolerance * 1000.0 / refScale); 
            }

            cbEnableSmooth.Checked = enableSmooth;

            string[] soomthTypes = { "PAEK", "BEZIER_INTERPOLATION" };
            foreach (var oneType in soomthTypes)
            {
                SmoothComboBox.Items.Add(oneType);
                if (oneType == "PAEK")
                {
                    SmoothComboBox.SelectedItem = oneType;
                }
            }
            SmoothComboBox.SelectedIndex = SmoothComboBox.Items.IndexOf(smoothAlgorithm);

            numSmoothTol.Controls[0].Visible = false;
            if (smoothTolerance >= (double)numSmoothTol.Minimum && smoothTolerance <= (double)numSmoothTol.Maximum && refScale > 0)
            {
                numSmoothTol.Value = (decimal)(smoothTolerance * 1000.0 / refScale);
            }

            smoothGroup.Enabled = cbEnableSmooth.Checked;
            if (smoothGroup.Enabled && SmoothComboBox.Text == "BEZIER_INTERPOLATION")
            {
                smoothGroup.Enabled = false;
            }
            
        }


        private void cbEnableSmooth_CheckedChanged(object sender, EventArgs e)
        {
            smoothGroup.Enabled = cbEnableSmooth.Checked;
            if (smoothGroup.Enabled && SmoothComboBox.Text == "BEZIER_INTERPOLATION")
            {
                smoothGroup.Enabled = false;
            }
        }

        private void SmoothComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SmoothComboBox.Text == "BEZIER_INTERPOLATION")
            {
                smoothGroup.Enabled = false;
            }
            else
            {
                smoothGroup.Enabled = true;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            bool res = double.TryParse(cmbReferScale.Text, out _refScale);
            if (!res || _refScale <= 0)
            {
                MessageBox.Show("参考比例尺输入不正确，请重新输入！");
                return;
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }


        public static string VerifySQL(IFeatureClass fc, string filter)
        {
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = filter;
            try
            {
                int count = fc.FeatureCount(qf);
            }
            catch (Exception ex1)
            {
                System.Diagnostics.Trace.WriteLine(ex1.Message);
                System.Diagnostics.Trace.WriteLine(ex1.StackTrace);
                System.Diagnostics.Trace.WriteLine(ex1.Source);

                return string.Format("定义查询语句【{0}】错误:{1}!", filter, ex1.Message);
            }

            return "";

        }
        
    }
}
