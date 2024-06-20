using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesFile;

namespace SMGI.Plugin.CollaborativeWork
{
    /// <summary>
    /// 从服务器中导出所有要素(原始、修改、增加、删除)的最新版本数据
    /// 删除要素的feaid字段值不能为空（制图中心要求）
    /// </summary>
    public class ExportDataCmd : SMGICommand
    {
        public ExportDataCmd()
        {
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace == null;
            }
        }
       
        public override void OnClick()
        {
            ExportDataForm frm = new ExportDataForm(m_Application);
            frm.Text = "数据提取";
            if (DialogResult.OK == frm.ShowDialog())
            {
                bool res = false;
                using (var wo = m_Application.SetBusy())
                {
                    wo.SetText("正在导出数据...");

                    IGeometry rangeGeo = null;
                    if (frm.Range)
                        rangeGeo = frm.RangeGeometry;

                    SDEDataServer ds = new SDEDataServer(m_Application, frm.IPAdress, frm.UserName, frm.Password, frm.DataBase);
                    res = ds.ExtractAllData(frm.OutputGDB, rangeGeo, frm.FieldNameUpper, frm.DelCollaField, wo);
                }

                if (res)
                {
                    MessageBox.Show("数据导出成功！");
                }
            }
        }

    }
}
