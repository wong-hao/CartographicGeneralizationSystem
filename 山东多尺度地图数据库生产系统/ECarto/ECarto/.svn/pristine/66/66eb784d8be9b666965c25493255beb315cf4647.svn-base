using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using DevExpress.XtraBars.Docking;

namespace SMGI.Plugin.EmergencyMap
{
    public class DataSearchTable
    {
        #region 字段、属性
        private static DataSearchTable _instance = null;
        public static DataSearchTable Instance
        {
            get
            {
                if (null == DataSearchTable._instance)
                {
                    DataSearchTable._instance = new DataSearchTable();
                }

                return DataSearchTable._instance;
            }
        }

        private DataSearchForm _searchForm;
        #endregion

        private DataSearchTable()
        {
            _searchForm = new DataSearchForm();
        }

        public void Show()
        {
            GApplication.Application.MainForm.ShowChild2(_searchForm.Handle, FormLocation.Right);
        }
    }
}
