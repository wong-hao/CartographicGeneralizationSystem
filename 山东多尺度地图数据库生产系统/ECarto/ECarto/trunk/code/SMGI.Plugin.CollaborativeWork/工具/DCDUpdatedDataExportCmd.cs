using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.CollaborativeWork
{
    /// <summary>
    /// 级联更新增量数据包导出工具：从某一较大比例尺服务器数据库中导出下一较小比例尺数据库的增量数据包
    /// 基于较大比例尺的更新成果服务器数据库DB2（更新后，包含删除状态的要素）、较大比例尺的原始成果数据库DB1（更新前，不包含删除状态的要素），对比提取数据更新的增量包，并输出到同结构的本地数据库（相对于DB2，每个要素类都增加了一个STACOD字段（更新状态）：“删除”、“修改”、“增加”），并标记每个增量数据的更新状态
    /// 新增：DB2数据库中某一要素（CollabGUID）为非删除状态，而DB1中不存在该要素，则提取该增量要素，并标记为新增状态。
    /// 修改：DB2数据库中某一要素（CollabGUID）为非删除状态，而DB1中存在该要素，且该要素在DB2中的最大版本号大于DB1的数据库基版本号，则提取该增量要素，并标记为修改状态。
    /// 删除：DB2数据库中某一要素（CollabGUID）为删除状态，而DB1中存在该要素，则提取该增量要素，并标记为删除状态。
    /// </summary>
    public class DCDUpdatedDataExportCmd : SMGICommand
    {
        public DCDUpdatedDataExportCmd()
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
            DCDUpdatedDataExportForm frm = new DCDUpdatedDataExportForm(m_Application);
            frm.Text = "导出增量包";
            if (frm.ShowDialog() != DialogResult.OK)
                return;

            bool res = false;
            using (var wo = m_Application.SetBusy())
            {
                wo.SetText("正在导出数据...");

                IGeometry rangeGeo = null;
                if (frm.Range)
                    rangeGeo = frm.RangeGeometry;

                SDEDataServer ds = new SDEDataServer(m_Application, frm.IPAdress, frm.UserName, frm.Password, frm.DataBase);
                res = ds.DCDUpdatedDataExport(frm.ReferGDB, frm.OutputGDB, rangeGeo, wo, frm.cbnextDatabase,frm.NextGDB);
            }

            if (res)
            {
                MessageBox.Show("增量包导出成功！");
            }
            else
            {
                MessageBox.Show("增量包导出失败！");
            }
        }
    }
}
