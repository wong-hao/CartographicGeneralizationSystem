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
using ESRI.ArcGIS.DataSourcesGDB;

namespace SMGI.Plugin.CollaborativeWork
{
    public partial class ModifyServerIPForm : Form
    {
        /// <summary>
        /// 本地状态表表名
        /// </summary>
        public string LocalStateTableName
        {
            get
            {
                return "SMGILocalState";
            }
        }

        /// <summary>
        /// 本地状态表中服务器IP地址字段名
        /// </summary>
        public string IPFieldName
        {
            get
            {
                return "IPADDRESS";
            }
        }

        /// <summary>
        /// 本地数据库
        /// </summary>
        public IWorkspace LocalDataBase
        {
            get
            {
                return _ws;
            }
        }
        private IWorkspace _ws;

        public ModifyServerIPForm()
        {
            InitializeComponent();

            _ws = null;
        }

        private void ModifyServerIPForm_Load(object sender, EventArgs e)
        {

        }

        private void btnLocalDB_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "选择GDB工程文件夹";
            fbd.ShowNewFolderButton = false;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!GApplication.GDBFactory.IsWorkspace(fbd.SelectedPath))
                {
                    MessageBox.Show("不是有效地GDB文件");
                    return;
                }

                IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();
                _ws = wsFactory.OpenFromFile(fbd.SelectedPath, 0);

                //获取SMGILocalState表
                DataTable localDT = commonMethod.ReadToDataTable(_ws, LocalStateTableName);
                if (null == localDT || 0 == localDT.Rows.Count)
                {
                    MessageBox.Show(string.Format("数据库中没有找到表【{0}】或者表中的内容为空！", LocalStateTableName));
                    return;
                }

                //获取原服务器IP地址
                if (localDT.Columns.IndexOf(IPFieldName) == -1)
                {
                    MessageBox.Show(string.Format("数据库表【{0}】中没有找到字段【{1}】", IPFieldName));
                    return;
                }

                txtLocalDataBase.Text = fbd.SelectedPath;
                tbIPAdress.Text = localDT.AsEnumerable().Select(t => t.Field<string>(IPFieldName)).FirstOrDefault();
                
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (txtLocalDataBase.Text == "")
            {
                MessageBox.Show("请选择需更新的本地数据库");
                return;
            }

            if (tbIPAdress.Text == "")
            {
                MessageBox.Show("服务器地址不能为空！");
                return;
            }

            //修改表中的服务器IP地址内容
            try
            {
                ITable tb = getDTTable(LocalDataBase, LocalStateTableName);
                ICursor cursor = tb.Update(null, false);
                IRow row = null;
                int index = tb.Fields.FindField(IPFieldName);
                while ((row = cursor.NextRow()) != null)
                {
                    row.set_Value(index, tbIPAdress.Text);
                    cursor.UpdateRow(row);
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
                return;
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        /// <summary>
        /// 从工作空间中获取指定的表
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private ITable getDTTable(IWorkspace ws, string tableName)
        {
            ITable tb = null;

            IEnumDataset tableEnumDataset = ws.get_Datasets(esriDatasetType.esriDTTable);
            tableEnumDataset.Reset();
            IDataset tableDataset = null;
            while ((tableDataset = tableEnumDataset.Next()) != null)
            {
                if (tableDataset.Name.Trim().ToUpper() == LocalStateTableName.Trim().ToUpper())
                {
                    tb = tableDataset as ITable;
                    break;
                }
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(tableEnumDataset);

            return tb;
        }

        
    }
}
