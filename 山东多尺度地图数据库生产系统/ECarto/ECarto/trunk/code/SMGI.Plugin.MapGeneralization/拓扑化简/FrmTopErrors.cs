using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using System.IO;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesFile;
namespace SMGI.Plugin.MapGeneralization.Top
{
    public partial class FrmTopErrors : Form
    {
        List<IGeometry> errorsDic = new List<IGeometry>();
        public FrmTopErrors(List< IGeometry> errorsDic_)
        {
            InitializeComponent();
            errorsDic = errorsDic_;
        }

        private void btView_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "选择Shp保存文件夹";
            fbd.ShowNewFolderButton = false;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtPath.Text = fbd.SelectedPath;
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        public string shppath = "";
        private void btOk_Click(object sender, EventArgs e)
        {
            if (txtPath.Text != "")
            {
                if (Directory.Exists(txtPath.Text))
                {
                    shppath = txtPath.Text;
                    ExportShp(shppath);
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    try
                    {
                        var dirinfo = Directory.CreateDirectory(txtPath.Text);
                        shppath = dirinfo.FullName;
                        ExportShp(shppath);
                        DialogResult = DialogResult.OK;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("文件夹创建失败：" + ex.Message);
                        return;
                    }
                }
            }
        }

        private void ExportShp(string folderPath)
        {
            //创建点线面错误要素类
            Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
            fieldName2Len.Add("错误要素UID", 50);
            GApplication app = GApplication.Application;

            if (errorsDic[0].Dimension == esriGeometryDimension.esriGeometry1Dimension)
            {
                ShapeFileWriter ErrorLine = new ShapeFileWriter();
                ErrorLine.createErrorResutSHPFile(folderPath + "\\错误线要素", app.ActiveView.FocusMap.SpatialReference, esriGeometryType.esriGeometryPolyline, fieldName2Len);


                for (int i = 0; i < errorsDic.Count; i++)
                {          
                    Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                    fieldName2FieldValue.Add("错误要素ID", i.ToString());
                    ErrorLine.addErrorGeometry(errorsDic[i], fieldName2FieldValue);                    
                }
                ErrorLine.saveErrorResutSHPFile();
            }

            if (errorsDic[0].Dimension == esriGeometryDimension.esriGeometry2Dimension)
            {
                ShapeFileWriter ErrorPolygon = new ShapeFileWriter();
                ErrorPolygon.createErrorResutSHPFile(folderPath + "\\错误面要素", app.ActiveView.FocusMap.SpatialReference, esriGeometryType.esriGeometryPolygon, fieldName2Len);

                for (int i = 0; i < errorsDic.Count; i++)
                {
                    Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                    fieldName2FieldValue.Add("错误要素ID", i.ToString());
                    ErrorPolygon.addErrorGeometry(errorsDic[i], fieldName2FieldValue);
                }

                ErrorPolygon.saveErrorResutSHPFile();
            }

            #region
            //for (int i=0;i<errorsDic.Count;i++)
            //{              
            //    //var lyr = app.Workspace.LayerManager.GetLayer(l =>
            //    //{
            //    //    return l is IFeatureLayer && l.Name.ToUpper() == lyrName.ToUpper();
            //    //}).First();
            //    //var fcl = (app.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(lyrName);
            //   // IFeatureClass fcl = (lyr as IFeatureLayer).FeatureClass;

            //    Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
            //    fieldName2FieldValue.Add("错误要素ID", i.ToString());
            //    ErrorLine.addErrorGeometry(errorsDic[i], fieldName2FieldValue);
            //    //for (int j = 0; j < idslist.Count; j++)
            //    //{
            //    //    IGeometry geo = idslist[j];
                    
            //    //    IFeature fe = fcl.GetFeature(feid);
            //    //    Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
            //    //    fieldName2FieldValue.Add("图层名", lyrName);
            //    //    fieldName2FieldValue.Add("错误要素ID", feid.ToString());

            //    //    switch (fcl.ShapeType)
            //    //    {
                        
            //    //        case esriGeometryType.esriGeometryLine:
            //    //            ErrorLine.addErrorGeometry(fe.ShapeCopy as IGeometry, fieldName2FieldValue);
            //    //            break;
            //    //        case esriGeometryType.esriGeometryPolygon:
            //    //            ErrorPolygon.addErrorGeometry(fe.ShapeCopy as IGeometry, fieldName2FieldValue);
            //    //            break;
            //    //    }
            //    //}
            //}
            #endregion
            MessageBox.Show("导出成功！");
        }
    }
    public class ShapeFileWriter
    {
        public ShapeFileWriter()
        {
            _errorWorkspace = null;
            _errorFeatureClass = null;
            _errorFeatureCursor = null;
            _errNum = 0;
        }

        #region 成员、属性

        //错误工作空间
        IWorkspace _errorWorkspace;

        //错误点集要素类
        IFeatureClass _errorFeatureClass;

        //错误要素指针
        IFeatureCursor _errorFeatureCursor;

        //错误点数
        int _errNum;
        public int ErrNum
        {
            get
            {
                return _errNum;
            }
        }

        #endregion



        /// <summary>
        /// 创建一个shape文件
        /// </summary>
        /// <param name="fullFileName">D:\\Test\\居民地_点重叠检查.shp</param>
        /// <param name="sr"></param>
        /// <param name="geoType"></param>
        /// <param name="fieldName2Len"></param>
        /// <param name="bOverride"></param>
        /// <returns></returns>
        public bool createErrorResutSHPFile(string fullFileName, ISpatialReference sr, esriGeometryType geoType, Dictionary<string, int> fieldName2Len = null, bool bOverride = true)
        {
            var exteion = System.IO.Path.GetExtension(fullFileName);
            if (exteion == "")
            {
                fullFileName = fullFileName + ".shp";
            }
            if (System.IO.File.Exists(fullFileName))
            {
                if (!bOverride)
                {
                    if (MessageBox.Show(string.Format("文件【{0}】已经存在,是否直接覆盖？", fullFileName), "提示", MessageBoxButtons.YesNo) == DialogResult.No)
                        return false;
                }
                string path = fullFileName.Substring(0, fullFileName.LastIndexOf("\\"));
                string shpName = System.IO.Path.GetFileNameWithoutExtension(fullFileName);//fullFileName.Substring(fullFileName.LastIndexOf("\\"));
                IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactoryClass();
                IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile(path, 0);
                IFeatureClass pFeatureClass;
                string strShapeFile = shpName;
                if (File.Exists(path + "\\" + shpName + ".shp"))
                {
                    pFeatureClass = pFeatureWorkspace.OpenFeatureClass(strShapeFile);
                    IDataset pDataset = (IDataset)pFeatureClass;
                    pDataset.Delete();
                }


                //System.IO.File.Delete(fullFileName);
            }

            IWorkspaceFactory pWorkspaceFac = new ShapefileWorkspaceFactoryClass();
            _errorWorkspace = pWorkspaceFac.OpenFromFile(System.IO.Path.GetDirectoryName(fullFileName), 0);
            IFeatureWorkspace pSHPFeatWorkspace = (IFeatureWorkspace)_errorWorkspace;

            if (sr == null)
            {
                sr = new UnknownCoordinateSystemClass();
            }

            _errorFeatureClass = CreateCheckResultFeatureClass(pSHPFeatWorkspace, System.IO.Path.GetFileNameWithoutExtension(fullFileName), sr, geoType, fieldName2Len);

            return true;
        }


        /// <summary>
        /// 插入错误点
        /// </summary>
        /// <param name="errorGeo"></param>
        /// <param name="fieldName2FieldValue"></param>
        public void addErrorGeometry(IGeometry errorGeo, Dictionary<string, string> fieldName2FieldValue = null)
        {
            if (_errorFeatureClass == null)
                return;

            if (_errorFeatureCursor == null)
                _errorFeatureCursor = _errorFeatureClass.Insert(true);

            IFeatureBuffer featureBuf = _errorFeatureClass.CreateFeatureBuffer();
            if (fieldName2FieldValue != null)
            {
                foreach (var kv in fieldName2FieldValue)
                {
                    string fieldName = kv.Key;
                    string fieldValue = kv.Value;

                    int index = _errorFeatureClass.FindField(fieldName);
                    if (index != -1)
                    {
                        featureBuf.set_Value(index, fieldValue);
                    }
                }
            }
            //to remove the Zvalue
            IZAware pZAware = errorGeo as IZAware;
            pZAware.DropZs();
            pZAware.ZAware = false;
            featureBuf.Shape = errorGeo;

            _errorFeatureCursor.InsertFeature(featureBuf);

            _errNum++;

            if (0 == _errNum % 5000)
            {
                _errorFeatureCursor.Flush();
            }
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        public void saveErrorResutSHPFile()
        {
            if (_errorFeatureCursor != null && _errNum % 10000 != 0)
                _errorFeatureCursor.Flush();

            if (_errorFeatureCursor != null)
                System.Runtime.InteropServices.Marshal.ReleaseComObject(_errorFeatureCursor);
            if (_errorFeatureClass != null)
                System.Runtime.InteropServices.Marshal.ReleaseComObject(_errorFeatureClass);
            if (_errorWorkspace != null)
                System.Runtime.InteropServices.Marshal.ReleaseComObject(_errorWorkspace);
        }

        /// <summary>
        /// 创建一个检查结果要素类
        /// </summary>
        /// <param name="featureWorkspace"></param>
        /// <param name="shpName"></param>
        /// <param name="spatialReference"></param>
        /// <param name="geoType"></param>
        /// <param name="fieldName2Len"></param>
        /// <returns></returns>
        private IFeatureClass CreateCheckResultFeatureClass(IFeatureWorkspace featureWorkspace, String shpName, ISpatialReference spatialReference, esriGeometryType geoType, Dictionary<string, int> fieldName2Len)
        {
            IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
            IObjectClassDescription ocDescription = (IObjectClassDescription)fcDescription;
            IFields fields = ocDescription.RequiredFields;
            IFieldsEdit pFieldsEdit = (IFieldsEdit)fields;

            if (fieldName2Len != null)
            {
                foreach (var kv in fieldName2Len)
                {
                    IField ErrTypeField = new FieldClass();
                    IFieldEdit ErrFieldEdit = (IFieldEdit)ErrTypeField;
                    ErrFieldEdit.Name_2 = kv.Key;
                    ErrFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                    ErrFieldEdit.Length_2 = kv.Value;
                    pFieldsEdit.AddField(ErrTypeField);
                }
            }

            IFieldChecker fieldChecker = new FieldCheckerClass();
            IEnumFieldError enumFieldError = null;
            IFields validatedFields = null;
            fieldChecker.ValidateWorkspace = (IWorkspace)featureWorkspace;
            fieldChecker.Validate(fields, out enumFieldError, out validatedFields);

            int shapeFieldIndex = fields.FindField(fcDescription.ShapeFieldName);
            IField Shapefield = fields.get_Field(shapeFieldIndex);
            IGeometryDef geometryDef = Shapefield.GeometryDef;
            IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = geoType;
            geometryDefEdit.SpatialReference_2 = spatialReference;

            IFeatureClass featureClass = featureWorkspace.CreateFeatureClass(shpName, fields, ocDescription.InstanceCLSID,
                ocDescription.ClassExtensionCLSID, esriFeatureType.esriFTSimple, fcDescription.ShapeFieldName, "");

            return featureClass;
        }


    }
}
