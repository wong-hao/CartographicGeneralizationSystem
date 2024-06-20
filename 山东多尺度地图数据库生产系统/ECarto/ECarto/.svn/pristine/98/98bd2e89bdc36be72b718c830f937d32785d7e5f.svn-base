using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using System.IO;
using System.Xml.Linq;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class ClipDataByInnerborderForm : Form
    {
        #region 属性
        public List<string> FCNameList
        {
            set;
            get;
        }
        #endregion

        private GApplication _app;
        public ClipDataByInnerborderForm(GApplication app, List<string> fcNameList)
        {
            InitializeComponent();

            _app = app;

            foreach (var item in fcNameList)
            {
                chkFCList.Items.Add(item, true);
            }
        }

        private void ClipDataByInnerborderForm_Load(object sender, EventArgs e)
        {
            
        }

        private void btnSelAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < chkFCList.Items.Count; i++)
            {
                chkFCList.SetItemChecked(i, true);
            }
        }

        private void btnUnSelAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < chkFCList.Items.Count; i++)
            {
                chkFCList.SetItemChecked(i, false);
            }
            chkFCList.ClearSelected();
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (chkFCList.CheckedItems.Count == 0)
            {
                MessageBox.Show("请选择至少一个要素类！");
                return;
            }

            FCNameList = new List<string>();
            foreach (var item in chkFCList.CheckedItems)
            {
                FCNameList.Add(item.ToString());
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
