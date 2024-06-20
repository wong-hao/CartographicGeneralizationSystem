using System;
using System.Linq;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Collections;
using System.Runtime.InteropServices;
namespace ShellTBDivided
{

    public  class GApplication
    {

        private static GApplication sApp = null;

        public static GApplication Application
        {
            get
            {
                if (sApp == null)
                    sApp = new GApplication();
                return sApp;
            }
        }
        public Geoprocessor GPTool=new Geoprocessor();

        public void AddField(IFeatureClass fCls, string fieldName)
        {
            int index = fCls.FindField(fieldName);
            if (index != -1)
            {
                return;
            }
            IFields pFields = fCls.Fields;
            IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;
            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Name_2 = fieldName;
            pFieldEdit.AliasName_2 = fieldName;
            pFieldEdit.Length_2 = 1;
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            IClass pTable = fCls as IClass;
            pTable.AddField(pField);
            pFieldsEdit = null;
            pField = null;

        }
        public List<string> UniqueValue(IFeatureClass fcl, string field)
        {
            List<string> list=new List<string>();
           
            ITable pTable = fcl as ITable;
            ICursor pCusor;
            IQueryFilter sf = new QueryFilterClass();
            sf.AddField(field);
            pCusor = pTable.Search(sf, true);//获取字段
            IEnumerator pEnumerator;

            //获取字段中各要素属性唯一值
            IDataStatistics pDataStatistics = new DataStatisticsClass();
            pDataStatistics.Field = field;//获取统计字段
            pDataStatistics.Cursor = pCusor;
            pEnumerator = pDataStatistics.UniqueValues;
            while (pEnumerator.MoveNext())
            {
                string valuet = pEnumerator.Current.ToString();
                if (valuet == null)
                {
                    continue;
                }
                list.Add(valuet);
            }
            Marshal.ReleaseComObject(pCusor);
            return list;
        }
        public IFeatureClass QueryFeatureClass(string gdbPath, string fclName)
        {
            IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();
            IWorkspace workspace = wsFactory.OpenFromFile(gdbPath, 0);
            IWorkspaceFactoryLockControl ipWsFactoryLock = (IWorkspaceFactoryLockControl)wsFactory;//注意在java api中不能强转切记需要IWorkspaceFactoryLockControl ipWsFactoryLock = new IWorkspaceFactoryLockControlProxy(pwf);
            if (ipWsFactoryLock.SchemaLockingEnabled)
            {
                ipWsFactoryLock.DisableSchemaLocking();
            }
            if ((workspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, fclName))
            {
                return (workspace as IFeatureWorkspace).OpenFeatureClass(fclName);
            }
            return null;
           
        }
         

    }
}
