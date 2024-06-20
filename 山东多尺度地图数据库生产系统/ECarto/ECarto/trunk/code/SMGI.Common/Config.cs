using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using System.Xml.Linq;
namespace SMGI.Common
{
    public class Config : SMGI.Common.IConfig
    {
        private static string ConfigTableName = "econfig";
        internal ITable table;
        private Config(ITable t)
        {
            this.table = t;
        }
        public static Config Open(IFeatureWorkspace ws)
        {
            return new Config((ws as IFeatureWorkspace).OpenTable(ConfigTableName));
        }
        public static Config Create(GWorkspace workspace)
        {
            return new Config((workspace.EsriWorkspace as IFeatureWorkspace).OpenTable(ConfigTableName));
        }
        public static Config Create(GApplication app)
        {
            string ConfigPath = GApplication.AppDataPath + @"\ConfigV5.gdb";
            if (System.IO.Directory.Exists(ConfigPath) && GApplication.GDBFactory.IsWorkspace(ConfigPath))
            {
                IWorkspace w = GApplication.GDBFactory.OpenFromFile(ConfigPath, 0);
                if (!Config.ExistConfigTable(w))
                {
                    Config.CreateConfigTable(w);
                }
                ITable t = (w as IFeatureWorkspace).OpenTable(ConfigTableName);
                return new Config(t);
            }
            else
            {
                if (System.IO.Directory.Exists(ConfigPath))
                {
                    System.IO.Directory.Delete(ConfigPath, true);
                }
                IWorkspaceName wName = GApplication.GDBFactory.Create(GApplication.AppDataPath, "ConfigV5", null, 0);
                IWorkspace w = (wName as ESRI.ArcGIS.esriSystem.IName).Open() as IWorkspace;
                Config.CreateConfigTable(w);
                return new Config((w as IFeatureWorkspace).OpenTable(ConfigTableName));
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
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(c);
                        return null;
                    }
                    else
                    {
                        string xml = r.get_Value(r.Fields.FindField("val")).ToString();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(c);
                        return GConvert.Base64ToObject(xml);
                    }
                    
                }
                catch
                {
                    return null;
                }

            }
            set
            {
                if (value == null)
                    return;

                string xml = GConvert.ObjectToBase64(value);

                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = "name = '" + key + "'";
                var wsEdit = (table as IDataset).Workspace as IWorkspaceEdit;
                if (wsEdit.IsBeingEdited())
                {
                    wsEdit.StartEditOperation();
                }
                if (table.RowCount(qf) > 0)
                {
                    ICursor c = table.Search(qf, false);
                    IRow r = c.NextRow();
                    r.set_Value(r.Fields.FindField("val"), xml);
                    r.Store();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(c);
                }
                else
                {
                    IRow r = table.CreateRow();

                    r.set_Value(r.Fields.FindField("name"), key);
                    r.set_Value(r.Fields.FindField("val"), xml);
                    r.Store();
                }
                if (wsEdit.IsBeingEdited())
                {
                    wsEdit.StopEditOperation();
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

            (workspace as IFeatureWorkspace).CreateTable(ConfigTableName, fields, uid, null, "");
        }

        public static void CreateConfigTableBak(IWorkspace workspace)
        {
            workspace.ExecuteSQL("create table _gconfig (name text NOT NULL,val Memo ,UNIQUE(name))");
            ITable table = (workspace as IFeatureWorkspace).OpenTable(ConfigTableName);
            IFieldEdit field = new FieldClass();
            field.Name_2 = "OID";
            field.Type_2 = esriFieldType.esriFieldTypeOID;
            table.AddField(field as IField);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(table);
        }
        public static bool ExistConfigTable(IWorkspace workspace)
        {
            return (workspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable, ConfigTableName);
        }


        public string GetOriginValue(string key)
        {
            try
            {
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = "name = '" + key + "'";
                ICursor c = table.Search(qf, true);
                IRow r = c.NextRow();
                if (r == null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(c);
                    return null;
                }
                else
                {
                    string value = r.get_Value(r.Fields.FindField("val")).ToString();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(c);
                    return value;
                }

            }
            catch
            {
                return null;
            }
        }

        public void SetOriginValue(string key, string value)
        {
            if (value == null)
                return;

            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "name = '" + key + "'";
            var wsEdit = (table as IDataset).Workspace as IWorkspaceEdit;
            if (wsEdit.IsBeingEdited())
            {
                wsEdit.StartEditOperation();
            }
            if (table.RowCount(qf) > 0)
            {
                ICursor c = table.Search(qf, false);
                IRow r = c.NextRow();
                r.set_Value(r.Fields.FindField("val"), value);
                r.Store();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(c);
            }
            else
            {
                IRow r = table.CreateRow();

                r.set_Value(r.Fields.FindField("name"), key);
                r.set_Value(r.Fields.FindField("val"), value);
                r.Store();
            }
            if (wsEdit.IsBeingEdited())
            {
                wsEdit.StopEditOperation();
            }
        }
    }

    public class XmlConfig : SMGI.Common.IConfig
    {
        static readonly string ConfigFileName = "Config.xml";
        static readonly string ConfigFilePath = GApplication.AppDataPath + "\\" + ConfigFileName; 
        public object this[string key]
        {
            get
            {
                try
                {
                    var doc = GetDocument();
                    var el = doc.Element("Config").Element(key) ;

                    return GConvert.Base64ToObject(el.Value);
                }
                catch
                {
                    return null;
                }

            }
            set
            {
                if (value == null)
                    return;

                string xml = GConvert.ObjectToBase64(value);
                var doc = GetDocument();
                var el = doc.Element("Config").Element(key);
                if (el == null)
                {
                    doc.Element("Config").Add(new XElement(key, xml));
                }
                else
                {
                    el.Value = xml;
                }
                doc.Save(ConfigFilePath);
            }
        }

        public static bool ExistConfigFile()
        {
            return System.IO.File.Exists(ConfigFilePath);
        }
        public string GetOriginValue(string key)
        {
            try
            {
                var doc = GetDocument();
                var el = doc.Element("Config").Element(key);

                return (el.Value);
            }
            catch
            {
                return null;
            }
        }

        public void SetOriginValue(string key, string value)
        {
            if (value == "")
                return;

            string xml = value;
            var doc = GetDocument();
            var el = doc.Element("Config").Element(key);
            if (el == null)
            {
                doc.Element("Config").Add(new XElement(key, xml));
            }
            else
            {
                el.Value = xml;
            }
            doc.Save(ConfigFilePath);
        }
        XDocument GetDocument()
        {
            XDocument doc = null;
            if (ExistConfigFile())
            {
                doc = XDocument.Load(ConfigFilePath);
            }
            else
            {
                doc = new XDocument(new XElement("Config"));
            }
            return doc;
        }
    }
}
