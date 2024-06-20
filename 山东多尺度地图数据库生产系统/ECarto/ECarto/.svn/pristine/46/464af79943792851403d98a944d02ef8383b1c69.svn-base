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
using ESRI.ArcGIS.Geodatabase;
using System.IO;
using System.Xml.Linq;
namespace SMGI.Plugin.EmergencyMap.Mask
{
    public partial class FrmMaskingSet : Form
    {
        public string MaskingLyr = "";
        public string MaskedLyr = "";
        public bool UsingMask = true;
        public FrmMaskingSet()
        {
            InitializeComponent();
        }

        private void FrmMaskingSet_Load(object sender, EventArgs e)
        {
            GApplication m_Application = GApplication.Application;
            var fileName = m_Application.Template.Root + @"\专家库\消隐\水系消隐.xml" ;
            string hyda = "HYDA";
            string hydl = "HYDL";
            if (File.Exists(fileName))
            {
                XDocument doc = XDocument.Load(fileName);
                var content = doc.Element("Template").Element("Content");
                var mask = content.Element("MakedLayer");
                if(mask!=null)
                    hyda = mask.Value;
                var masked = content.Element("MakingLayer");
                if (masked != null)
                    hydl = masked.Value;
            }
            var lyrs= m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(x =>
            {
                return x is IFeatureLayer && (x as IFeatureLayer).FeatureClass.AliasName == hyda && ((x as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName == m_Application.Workspace.EsriWorkspace.PathName;

            })).ToArray();

            for(int i=0;i<lyrs.Length;i++)
            {
                var lyr=lyrs[i];
                maskLyrs.Items.Add(lyr.Name);
                if (lyr.Name == CommonMethods.MaskLayer)
                {
                    maskLyrs.SetItemChecked(i,true);
                }
            }

            lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(x =>
            {
                return x is IFeatureLayer && (x as IFeatureLayer).FeatureClass.AliasName == hydl && ((x as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName == m_Application.Workspace.EsriWorkspace.PathName;

            })).ToArray();

            
            for(int i=0;i<lyrs.Length;i++)
            {
                var lyr=lyrs[i];
                maskedLyrs.Items.Add(lyr.Name);
                if (lyr.Name == CommonMethods.MaskedLayer)
                {
                    maskedLyrs.SetItemChecked(i, true);
                }
            }
            cbMask.Checked = CommonMethods.UsingMask;
        }

        private void cbMask_CheckedChanged(object sender, EventArgs e)
        {
            gbMask.Enabled = cbMask.Checked;
            gbMasked.Enabled = cbMask.Checked;
            UsingMask = cbMask.Checked;
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (!UsingMask)
            {
                DialogResult = DialogResult.OK;
            }
            if (maskLyrs.CheckedItems.Count == 0 || maskedLyrs.CheckedItems.Count==0)
            {
                MessageBox.Show("请选择图层","提示");
                return;
            }

            MaskedLyr = maskLyrs.CheckedItems[0].ToString();
            MaskingLyr = maskedLyrs.CheckedItems[0].ToString();
            DialogResult = DialogResult.OK;
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void maskLyrs_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                for (int i = 0; i < maskLyrs.Items.Count; i++)
                {
                    if (i != e.Index)
                    {
                        maskLyrs.SetItemChecked(i, false);
                    }
                }
            }
        }

        private void maskedLyrs_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                for (int i = 0; i < maskedLyrs.Items.Count; i++)
                {
                    if (i != e.Index)
                    {
                        maskedLyrs.SetItemChecked(i, false);
                    }
                }
            }

        }

        
    }
}
