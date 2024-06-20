using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
namespace SMGI.Plugin.BaseFunction
{
    public class MXDOpenCmd : SMGICommand
    {
        public MXDOpenCmd()
        {
            m_caption = "打开MXD";
        }
        public override bool Enabled
        {
            get
            {
                return m_Application!=null && m_Application.Workspace==null;
            }
        }
        public override void OnClick()
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "mxd文件(*.mxd)|*.mxd";

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    m_Application.MapControl.LoadMxFile(openFileDialog1.FileName, null, null);
                }

            }
            catch (Exception)
            {
            }
        }
    }
}
