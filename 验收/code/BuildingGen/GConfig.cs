using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;

namespace BuildingGen
{
    public class GConfig
    {
        internal ITable table;
        private GConfig(ITable t)
        {
            this.table = t;
        }
        public static GConfig Create(GWorkspace workspace)
        {
            return new GConfig( (workspace.Workspace as IFeatureWorkspace).OpenTable("_gconfig"));
        }
        public static GConfig Create(GApplication app)
        {
            string ConfigPath = app.ExePath+ "\\config.gdb";
            if (System.IO.Directory.Exists(ConfigPath) && GApplication.GDBFactory.IsWorkspace(ConfigPath))
            {
                IWorkspace w = GApplication.GDBFactory.OpenFromFile(ConfigPath, 0);
                if (!GConfig.ExistConfigTable(w))
                {
                    GConfig.CreateConfigTable(w);
                }
                ITable t = (w as IFeatureWorkspace).OpenTable("_gconfig");
                return new GConfig(t);
            }
            else
            {
                if (System.IO.Directory.Exists(ConfigPath))
                {
                    System.IO.Directory.Delete(ConfigPath, true);
                }
                IWorkspaceName wName = GApplication.GDBFactory.Create(app.ExePath, "config", null, 0);
                IWorkspace w = (wName as ESRI.ArcGIS.esriSystem.IName).Open() as IWorkspace;
                GConfig.CreateConfigTable(w);
                return new GConfig((w as IFeatureWorkspace).OpenTable("_gconfig"));
            }

        }
        public object this[string key]
        {
            get
            {
                try
                {
                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = "name = '" + key + "'";
                    ICursor c = table.Search(qf, true);
                    IRow r = c.NextRow();
                    if (r == null)
                    {
                        return null;
                    }
                    else
                    {
                        string xml = r.get_Value(r.Fields.FindField("val")).ToString();
                        return GConvert.XmlToObject(xml);
                    }
                }
                catch
                {
                    return null;
                }

            }
            set
            {
                string xml = GConvert.ObjectToXml(value);

                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = "name = '" + key + "'";
                if (table.RowCount(qf) > 0)
                {
                    ICursor c = table.Search(qf, true);
                    IRow r = c.NextRow();
                    r.set_Value(r.Fields.FindField("val"), xml);
                    r.Store();
                }
                else
                {
                    IRow r = table.CreateRow();
                    
                    r.set_Value(r.Fields.FindField("name"), key);
                    r.set_Value(r.Fields.FindField("val"), xml);
                    r.Store();
                }
            }
        }
        public static void CreateConfigTable(IWorkspace workspace)
        {
            FieldsClass fields = new FieldsClass();
            IFieldEdit field = new FieldClass();
            field.Name_2 = "OID";
            field.Type_2 = esriFieldType.esriFieldTypeOID;
            fields.AddField(field);

            field = new FieldClass();
            field.Name_2 = "name";
            field.Type_2 = esriFieldType.esriFieldTypeString;
            field.IsNullable_2 = false;
            field.DefaultValue_2 = "test";
            field.Editable_2 = true;
            field.Length_2 = 100;
            fields.AddField(field);

            field = new FieldClass();
            field.Name_2 = "val";
            field.Type_2 = esriFieldType.esriFieldTypeString;
            field.IsNullable_2 = false;
            field.DefaultValue_2 = "test";
            field.Editable_2 = true;
            field.Length_2 = Int32.MaxValue;
            fields.AddField(field);

            ESRI.ArcGIS.esriSystem.UID uid = new ESRI.ArcGIS.esriSystem.UIDClass();
            uid.Value = "esriGeoDatabase.Object";

            (workspace as IFeatureWorkspace).CreateTable("_gconfig", fields, uid, null, "");
        }

        public static void CreateConfigTableBak(IWorkspace workspace)
        {
            workspace.ExecuteSQL("create table _gconfig (name text NOT NULL,val Memo ,UNIQUE(name))");
            ITable table = (workspace as IFeatureWorkspace).OpenTable("_gconfig");
            IFieldEdit field = new FieldClass();
            field.Name_2 = "OID";
            field.Type_2 = esriFieldType.esriFieldTypeOID;
            table.AddField(field as IField);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(table);
        }
        public static bool ExistConfigTable(IWorkspace workspace)
        {
            return (workspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable, "_gconfig");
        }
    }
}
