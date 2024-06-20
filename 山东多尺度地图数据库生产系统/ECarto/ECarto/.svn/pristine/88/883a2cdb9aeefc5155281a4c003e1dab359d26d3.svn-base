using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.ADF.BaseClasses;
using System.Drawing;
using ESRI.ArcGIS.Framework;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
namespace SMGI.Plugin.CollaborativeWork
{
    public class ExportDataCommand : SMGICommand
    {
        public ExportDataCommand()
        {
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState== ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateNotEditing;
            }
        }
       
        public override void OnClick()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "GDB文件|*.gdb|Shape文件|*.shp|MDB文件|*.mdb";
            if (sfd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            int lyNum = 0;
            using (var wo = m_Application.SetBusy())
            {
                wo.SetText("正在创建并初始化数据库......");
                var fullname = sfd.FileName;
                var wext = System.IO.Path.GetExtension(fullname);
                var wdir = System.IO.Path.GetDirectoryName(fullname);
                var wname = System.IO.Path.GetFileNameWithoutExtension(fullname);
                IWorkspaceFactory wf = null;
                switch (wext.ToLower())
                {
                    case ".gdb":
                        wf = new ESRI.ArcGIS.DataSourcesGDB.FileGDBWorkspaceFactoryClass();
                        break;
                    case ".mdb":
                        wf = new ESRI.ArcGIS.DataSourcesGDB.AccessWorkspaceFactoryClass();
                        break;
                    case ".shp":
                        wf = new ESRI.ArcGIS.DataSourcesFile.ShapefileWorkspaceFactoryClass();
                        break;
                    default:
                        return;
                }
                IWorkspace w = CreateWorkspace(wf, wdir, wname);
                var view = m_Application.ActiveView;
                var map = view.FocusMap;
                var ls = map.get_Layers();
                IEnumFeature ef = map.FeatureSelection as IEnumFeature;
                SpatialFilterClass sf = null;
                IFeature f = ef.Next();
                if (f != null)
                {
                    sf = new SpatialFilterClass();
                    sf.Geometry = f.Shape;
                    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                }
                
                for (var layer = ls.Next(); layer != null; layer = ls.Next())
                {
                    wo.SetText("正在导出【" + layer.Name + "】......");
                    if (!(layer is ESRI.ArcGIS.Carto.IFeatureLayer))
                        continue;
                    if (!layer.Visible)
                        continue;
                    ExportLayer(w, layer as IFeatureLayer, sf);
                    lyNum++;
                }
            }
            MessageBox.Show("导出成功！\n共导出【" + lyNum.ToString() + "】个图层");
        }

        static IWorkspace CreateWorkspace(IWorkspaceFactory f, string dir, string name)
        {
            IWorkspaceName wname = f.Create(dir, name, null, 0);
            return (wname as IName).Open() as IWorkspace;
        }

        static void ExportLayer(IWorkspace ws, IFeatureLayer layer, IQueryFilter qf = null)
        {
            IFeatureClassLoad pload = null;
            IFeatureClass featureClass = null;
            IFeatureCursor writeCursor = null;
            string name = layer.FeatureClass.AliasName;
            var idx = name.LastIndexOf('.');
            if (idx != -1)
            {
                name = name.Substring(idx + 1);
            }
            else
            {
                name = name.Replace('.', '_');
            }
            try
            {
                IFields expFields;
                expFields = (layer.FeatureClass.Fields as IClone).Clone() as IFields;
                for (int i = 0; i < expFields.FieldCount; i++)
                {
                    IField field = expFields.get_Field(i);
                    IFieldEdit fieldEdit = field as IFieldEdit;
                    fieldEdit.Name_2 = field.Name.ToUpper();
                }
                featureClass = CreateFeatureClass(ws, name, expFields);

                pload = featureClass as IFeatureClassLoad;
                if (pload != null)
                    pload.LoadOnlyMode = true;

                IFeatureCursor readCursor = layer.Search(qf, true);
                writeCursor = featureClass.Insert(true);
                IFeature feature = null;
                int count = 0;
                while ((feature = readCursor.NextFeature()) != null)
                {
                    count++;
                    IFeatureBuffer fb = featureClass.CreateFeatureBuffer();
                    for (int i = 0; i < fb.Fields.FieldCount; i++)
                    {
                        IField field = fb.Fields.get_Field(i);
                        if (!(field as IFieldEdit).Editable)
                        {
                            continue;
                        }
                        if (field.Type == esriFieldType.esriFieldTypeGeometry)
                        {
                            fb.Shape = feature.ShapeCopy;
                            continue;
                        }
                        fb.set_Value(i, feature.get_Value(feature.Fields.FindField(field.Name)));
                    }
                    writeCursor.InsertFeature(fb);
                }
                writeCursor.Flush();
                if (featureClass != null)
                {
                    (featureClass as IFeatureClassManage).UpdateExtent();
                }
            }
            catch
            {

            }
            finally
            {
                if (writeCursor != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(writeCursor);
                }
                if (pload != null)
                {
                    pload.LoadOnlyMode = false;
                }
            }
        }

        static public IFeatureClass CreateFeatureClass(IWorkspace ws, string name, IFields org_fields)
        {
            IObjectClassDescription featureDescription = new FeatureClassDescriptionClass();

            IFieldsEdit fields = featureDescription.RequiredFields as IFieldsEdit;


            for (int i = 0; i < org_fields.FieldCount; i++)
            {
                IField field = org_fields.get_Field(i);
                if (!(field as IFieldEdit).Editable)
                {
                    continue;
                }
                if (field.Type == esriFieldType.esriFieldTypeGeometry)
                {
                    (fields as IFieldsEdit).set_Field(fields.FindFieldByAliasName((featureDescription as IFeatureClassDescription).ShapeFieldName),
                        (field as ESRI.ArcGIS.esriSystem.IClone).Clone() as IField);
                    continue;
                }
                if (fields.FindField(field.Name) >= 0)
                {
                    continue;
                }
                //剔除协同字段
                if (ServerDataInitializeCommand.CollabGUID == field.Name.ToUpper() ||
                    ServerDataInitializeCommand.CollabVERSION == field.Name.ToUpper() ||
                    ServerDataInitializeCommand.CollabDELSTATE == field.Name.ToUpper() || 
                    ServerDataInitializeCommand.CollabOPUSER == field.Name.ToUpper())
                {
                    continue;
                }

                IField field_new = (field as ESRI.ArcGIS.esriSystem.IClone).Clone() as IField;
                (fields as IFieldsEdit).AddField(field_new);
            }

            IFeatureWorkspace fws = ws as IFeatureWorkspace;

            System.String strShapeField = string.Empty;

            return fws.CreateFeatureClass(name, fields,
                  featureDescription.InstanceCLSID, featureDescription.ClassExtensionCLSID,
                  esriFeatureType.esriFTSimple,
                  (featureDescription as IFeatureClassDescription).ShapeFieldName,
                  string.Empty);

        }
    }
}
