using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;


namespace SMGI.Plugin.BaseFunction
{

    public partial class GDBProjectForm : Form
    {
        string dataSource = "GDB";
        string prjFolderPath = "";
        string targetPrjFilePath = "";
        string[] paras;
        bool flag = false;
        private Dictionary<string, IWorkspace> workspace_array = new Dictionary<string, IWorkspace>();
        private ISpatialReference targetSpatialRef = null;//目标空间参考
        private Geoprocessor m_Geoprocessor;
        private GApplication m_App;
        public GDBProjectForm(GApplication app)
        {
            InitializeComponent();
            m_Geoprocessor = app.GPTool;
            m_App = app;
        }
        private void buttonOpen_Click(object sender, EventArgs e)
        {
            //#region 批量导入GDB
            switch (dataSource)
            {
                case "GDB":
                    OpenGDB();
                    break;
                case "MDB":
                    OpenMDB();
                    break;
                default:
                    OpenIMG();
                    break;
            }
        }
        IWorkspaceFactory pWfMdb = new AccessWorkspaceFactoryClass();
        private void OpenMDB()
        {
            FolderBrowserDialog pFolder = new FolderBrowserDialog();
            pFolder.Description = "选择MDB数据存放的文件夹";
            pFolder.ShowNewFolderButton = false;
            if (pFolder.ShowDialog() == DialogResult.OK)
            {


                DirectoryInfo theFolder = new DirectoryInfo(pFolder.SelectedPath);
                FileInfo[] fileInfos = theFolder.GetFiles();
                //遍历文件
                foreach (FileInfo fileInfo in fileInfos)
                {
                    if (System.IO.Path.GetExtension(fileInfo.FullName).ToLower() == ".mdb")//
                    {
                        string pfile = fileInfo.FullName;
                        if (!workspace_array.ContainsKey(pfile))
                        {
                            IWorkspace pWorkspace = pWfMdb.OpenFromFile(pfile, 0);
                            if (hasOrNotSpatialRefInMDB(pWorkspace) == 1)
                            {
                                listBox_project_GDB.Items.Add(pfile);
                                workspace_array.Add(pfile, pWorkspace);
                            }
                            else
                            {
                                MessageBox.Show(fileInfo.Name + "没有定义空间参考或者部分定义空间参考，请定义后再进行操作！");
                            }

                        }

                    }
                }
            }
        }

        private void OpenGDB()
        {
            FolderBrowserDialog pFolder = new FolderBrowserDialog();
            pFolder.Description = "选择GDB存放的文件夹";
            pFolder.ShowNewFolderButton = false;
            if (pFolder.ShowDialog() == DialogResult.OK)
            {
                string folderpath = pFolder.SelectedPath;
                //表明当前文件夹就是一个GDB
                if (System.IO.Path.GetExtension(folderpath) == ".gdb")
                {
                    string pfile = folderpath;
                    if (!workspace_array.ContainsKey(pfile))
                    {
                        IWorkspaceFactory pWorkspaceFactory = new FileGDBWorkspaceFactoryClass();
                        IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(pfile, 0);
                        FileInfo pFileInfo = new FileInfo(pfile);
                        if (hasOrNotSpatialRefInMDB(pWorkspace) == 1)
                        {
                            listBox_project_GDB.Items.Add(pfile);
                            workspace_array.Add(pfile, pWorkspace);
                        }
                        else
                        {
                            MessageBox.Show(pFileInfo.Name + "没有定义空间参考或者部分定义空间参考，请定义后再进行操作！");
                        }
                    }
                }
                else
                {
                    DirectoryInfo theFolder = new DirectoryInfo(folderpath);
                    DirectoryInfo[] dirInfo = theFolder.GetDirectories();
                    //遍历文件夹
                    foreach (DirectoryInfo NextFolder in dirInfo)
                    {
                        if (NextFolder.Name.Substring(NextFolder.Name.LastIndexOf(".") + 1) == "gdb")
                        {
                            string pfile = folderpath + "\\" + NextFolder.Name;
                            if (!workspace_array.ContainsKey(pfile))
                            {
                                IWorkspaceFactory pWorkspaceFactory = new FileGDBWorkspaceFactoryClass();
                                IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(pfile, 0);
                                FileInfo pFileInfo = new FileInfo(pfile);
                                if (hasOrNotSpatialRefInMDB(pWorkspace) == 1)
                                {
                                    listBox_project_GDB.Items.Add(pfile);
                                    workspace_array.Add(pfile, pWorkspace);
                                }
                                else
                                {
                                    MessageBox.Show(pFileInfo.Name + "没有定义空间参考或者部分定义空间参考，请定义后再进行操作！");
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OpenIMG()
        {
            FolderBrowserDialog pFolder = new FolderBrowserDialog();
            pFolder.Description = "选择影像数据存放的文件夹";
            pFolder.ShowNewFolderButton = false;
            if (pFolder.ShowDialog() == DialogResult.OK)
            {
                string folderpath = pFolder.SelectedPath;
                {
                    #region
                    DirectoryInfo theFolder = new DirectoryInfo(folderpath);
                    DirectoryInfo[] dirInfo = theFolder.GetDirectories();
                    FileInfo[] fileInfos = theFolder.GetFiles();
                    //遍历文件
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        if (System.IO.Path.GetExtension(fileInfo.FullName).ToLower() == ".tif" || System.IO.Path.GetExtension(fileInfo.FullName).ToLower() == ".img")//
                        {
                            string pfile = fileInfo.FullName;
                            if (!listBox_project_GDB.Items.Contains(pfile))
                            {
                                listBox_project_GDB.Items.Add(pfile);
                            }

                        }
                    }

                    #endregion
                }
            }
        }
        /// <summary>
        /// 判断数据库中是否有没有投影坐标系的
        /// </summary>
        /// <param name="m_workspace"></param>
        /// <returns></returns>
        private int hasOrNotSpatialRefInMDB(IWorkspace m_workspace)
        {
            IList<ISpatialReference> m_spatialRef = new List<ISpatialReference>();//临时存储MDB中包含的空间参考
            int nothaveSpatialRef = 0;
            int haveSpatilRef = 0;
            IEnumDataset enumFeatureclass = m_workspace.get_Datasets(esriDatasetType.esriDTFeatureClass);
            IDataset fDataset = enumFeatureclass.Next();
            while (fDataset != null)
            {
                ISpatialReference fSpatialRef = (fDataset as IGeoDataset).SpatialReference;
                m_spatialRef.Add(fSpatialRef);
                fDataset = enumFeatureclass.Next();
            }
            IEnumDataset enumEDataset = m_workspace.get_Datasets(esriDatasetType.esriDTFeatureDataset);
            IDataset enumDataset = enumEDataset.Next();
            while (enumDataset != null)
            {
                ISpatialReference dSpatialRef = (enumDataset as IGeoDataset).SpatialReference;
                m_spatialRef.Add(dSpatialRef);
                enumDataset = enumEDataset.Next();
            }
            //遍历判断空间参考
            for (int i = 0; i < m_spatialRef.Count; i++)
            {
                if (m_spatialRef[i].Name == "Unknown")
                {
                    nothaveSpatialRef = nothaveSpatialRef + 1;
                }
                else
                {
                    haveSpatilRef = haveSpatilRef + 1;
                }
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(enumFeatureclass);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(enumEDataset);
            if (haveSpatilRef == 0)
            {
                return 0;
            }
            if (nothaveSpatialRef == 0)
            {
                return 1;
            }
            else
            {
                return 2;
            }

        }

        private void buttonOpenPro_Click(object sender, EventArgs e)
        {
            OpenFileDialog pOpenFileDialog = new OpenFileDialog();
            pOpenFileDialog.Filter = "坐标参考文件(*.prj)|*.prj";
            pOpenFileDialog.Title = "选择目标空间参考";
            DialogResult pDiglogR = pOpenFileDialog.ShowDialog();
            if (pDiglogR == DialogResult.OK)
            {
                string spatialRefFileName = pOpenFileDialog.FileName;
                cbProjectionFileList.SelectedIndex = cbProjectionFileList.Items.Add(spatialRefFileName.Substring(spatialRefFileName.LastIndexOf("\\") + 1));
                prjFolderPath = spatialRefFileName.Substring(0, spatialRefFileName.LastIndexOf("\\"));
            }
        }

        private void buttonOutput_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog pFolder = new FolderBrowserDialog();
            pFolder.Description = "选择投影后数据输出的位置";
            pFolder.ShowNewFolderButton = false;
            if (pFolder.ShowDialog() == DialogResult.OK)
            {
                ReprjPositionTextBox.Text = pFolder.SelectedPath;
            }
        }
        /// <summary>
        /// 创建数据库GDB
        /// </summary>
        /// <param name="path"></param>
        /// <param name="databasename"></param>
        /// <returns></returns>
        private IWorkspace createGDB(string path, string databasename)
        {
            IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactoryClass();
            IWorkspaceName workspaceName = workspaceFactory.Create(path, databasename, null, 0);
            IName name = workspaceName as IName;
            IWorkspace m_workspace = name.Open() as IWorkspace;
            return m_workspace;
        }
        // 创建数据库MDB
        private IWorkspace createMDB(string path, string mdbname)
        {
            if (File.Exists(path + "\\" + mdbname))
            {
                DialogResult dr = MessageBox.Show("文件已存在，是否覆盖?", "提示", MessageBoxButtons.YesNo);
                if (dr != DialogResult.Yes)
                    return null;
                File.Delete(path + "\\" + mdbname);
            }
            Type factoryType = Type.GetTypeFromProgID(
                "esriDataSourcesGDB.AccessWorkspaceFactory");
            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance
                (factoryType);
            IWorkspaceName workspaceName = workspaceFactory.Create(path, mdbname, null,
                0);

            //Cast the workspace name object to the IName interface and open the workspace.
            IName name = (IName)workspaceName;
            IWorkspace workspace = (IWorkspace)name.Open();
            return workspace;


        }
        /// <summary>
        /// 进行坐标系投影
        /// </summary>
        /// <param name="m_workspace"></param>
        /// <param name="out_workspace"></param>
        private void reProjectMDB(IWorkspace m_workspace, IWorkspace out_workspace, SMGI.Common.WaitOperation wo)
        {
            IEnumDataset enumFeatureclass = m_workspace.get_Datasets(esriDatasetType.esriDTFeatureClass);
            IDataset fDataset = enumFeatureclass.Next();
            while (fDataset != null)
            {
                string feaName = fDataset.Name;
                wo.SetText("投影转换" + System.IO.Path.GetFileName(m_workspace.PathName) + "要素" + feaName + "……");
                IFeatureClass featureclass = fDataset as IFeatureClass;
                reProjectFeatureclass(featureclass, feaName, "", targetSpatialRef, out_workspace.PathName);
                fDataset = enumFeatureclass.Next();
            }
            IEnumDataset enumEDataset = m_workspace.get_Datasets(esriDatasetType.esriDTFeatureDataset);
            IDataset enumDataset = enumEDataset.Next();
            while (enumDataset != null)
            {
                string dname = enumDataset.Name;
                wo.SetText("投影转换" + System.IO.Path.GetFileName(m_workspace.PathName) + "要素集" + dname + "……");
                reProjectFeaturedataset(enumDataset, dname, targetSpatialRef, out_workspace);
                enumDataset = enumEDataset.Next();
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(enumFeatureclass);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(enumEDataset);
        }
        public ICoordinateFrameTransformation creatCustomGT(string[] paras, IFeatureClass inFeatureClass, ISpatialReference outSpatialReference)
        {
            IDataset fDataset = inFeatureClass.FeatureDataset;
            ISpatialReference fSpatialRef = (fDataset as IGeoDataset).SpatialReference;
            ICoordinateFrameTransformation pCFT = new CoordinateFrameTransformationClass();
            //   pCFT.PutParameters();
            //string fileFullPath = prjFolderPath + "\\" + cbProjectionFileList.SelectedItem.ToString();
            //ISpatialReferenceFactory pSpatialRefFactory = new SpatialReferenceEnvironmentClass();
            //targetSpatialRef = pSpatialRefFactory.CreateESRISpatialReferenceFromPRJFile(fileFullPath);
            //if (targetSpatialRef == null)
            //{
            //    MessageBox.Show("投影失败！");
            //    return pCFT;
            //}

            pCFT.PutParameters(double.Parse(paras[0]), double.Parse(paras[1]), double.Parse(paras[2]), double.Parse(paras[3]), double.Parse(paras[4]), double.Parse(paras[5]), double.Parse(paras[6]));
            pCFT.PutSpatialReferences(fSpatialRef, outSpatialReference);
            pCFT.Name = "Custom GeoTran";
            return pCFT;

        }
        private void CustomGT()
        {

            //// Initialize a new spatial reference environment.
            //// SpatialReferenceEnvironment is a singleton object and needs to use the Activator class.

            //Type factoryType = Type.GetTypeFromProgID(
            //    "esriGeometry.SpatialReferenceEnvironment");
            //System.Object obj = Activator.CreateInstance(factoryType);
            //ISpatialReferenceFactory2 pSRF = obj as ISpatialReferenceFactory2;

            //// Initialize and create the input and output coordinate systems.
            //IProjectedCoordinateSystem2 pPCSin = new
            //    ESRI.ArcGIS.Geometry.ProjectedCoordinateSystemClass();
            //IProjectedCoordinateSystem2 pPCSout = new
            //    ESRI.ArcGIS.Geometry.ProjectedCoordinateSystemClass();
            //pPCSin = (IProjectedCoordinateSystem2)pSRF.CreateProjectedCoordinateSystem((int)
            //    esriSRProjCSType.esriSRProjCS_Abidjan1987UTM_30N);
            //pPCSout = (IProjectedCoordinateSystem2)pSRF.CreateProjectedCoordinateSystem((int)
            //    esriSRProjCSType.esriSRProjCS_WGS1984UTM_30N);

            //// Retrieve the geographic coordinate systems from the two projected 
            //// coordinate systems.
            //IGeographicCoordinateSystem2 pGCSto = (IGeographicCoordinateSystem2)
            //    pPCSout.GeographicCoordinateSystem;
            //IGeographicCoordinateSystem2 pGCSfrom = (IGeographicCoordinateSystem2)
            //    pPCSin.GeographicCoordinateSystem;

            // Initialize and create an appropriate geographic transformation.
            //ICoordinateFrameTransformation pCFT = new CoordinateFrameTransformationClass();
            //pCFT.PutParameters(1.234, -2.345, 658.3, 4.3829, -2.48591, 2.18943, 2.48585);
            //pCFT.PutSpatialReferences(pGCSfrom, pGCSto);
            //pCFT.Name = "Custom GeoTran";

            // The SpatialReferenceEnvironment has a GeoTransformationOperationSet that you
            // can use to maintain a list of active geographic transformations. 
            // Once you add a geographic transformation to the operation set, many operations
            // can access the transformations. 
            // Add the transformation to the operation set.
            //IGeoTransformationOperationSet pGTSet = pSRF.GeoTransformationDefaults;

            //// Always add a geographic transformation in both directions. 
            //pGTSet.Set(esriTransformDirection.esriTransformForward, pCFT);
            //pGTSet.Set(esriTransformDirection.esriTransformReverse, pCFT);

        }
        /// <summary>
        /// 要素集坐标系投影
        /// </summary>
        /// <param name="inFeatureClass"></param>
        /// <param name="outFeaName"></param>
        /// <param name="outputDatasetName"></param>
        /// <param name="outSpatialReference"></param>
        /// <param name="out_workspace"></param>
        private void reProjectFeatureclass(IFeatureClass inFeatureClass, string outFeaName, string outputDatasetName, ISpatialReference outSpatialReference, string out_workspace)
        {
            ICoordinateFrameTransformation ICoordinateFrameTransformation = null;

            m_Geoprocessor.SetEnvironmentValue("workspace", out_workspace);
            m_Geoprocessor.OverwriteOutput = true;
            Project project = new Project();
            project.in_dataset = inFeatureClass;
            project.out_coor_system = outSpatialReference;
            project.out_dataset = outFeaName;
            if (flag) { ICoordinateFrameTransformation = creatCustomGT(paras, inFeatureClass, outSpatialReference); project.transform_method = ICoordinateFrameTransformation; }

            try
            {
                Helper.ExecuteGPTool(m_Geoprocessor, project, null);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        /// <summary>
        /// 要素集合坐标系投影
        /// </summary>
        /// <param name="inFeatureDataset"></param>
        /// <param name="outDatasetName"></param>
        /// <param name="outSpatialReference"></param>
        /// <param name="out_workspace"></param>
        private void reProjectFeaturedataset(IDataset inFeatureDataset, string outDatasetName, ISpatialReference outSpatialReference, IWorkspace out_workspace)
        {
            m_Geoprocessor.SetEnvironmentValue("workspace", out_workspace.PathName);
            m_Geoprocessor.OverwriteOutput = true;
            Project project = new Project();
            project.in_dataset = inFeatureDataset;
            project.out_dataset = outDatasetName;
            project.out_coor_system = outSpatialReference;
            try
            {
                Helper.ExecuteGPTool(m_Geoprocessor,project, null);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }
        private void ProjectOK_Click(object sender, EventArgs e)
        {
            string fileFullPath = prjFolderPath + "\\" + cbProjectionFileList.SelectedItem.ToString();
            if (checkpara.Checked)
            {
                flag = true;
                paras[0] = xm.Text.ToSafeString().Trim();
                paras[1] = ym.Text.ToSafeString().Trim();
                paras[2] = zm.Text.ToSafeString().Trim();
                paras[3] = xr.Text.ToSafeString().Trim();
                paras[4] = yr.Text.ToSafeString().Trim();
                paras[5] = zr.Text.ToSafeString().Trim();
                paras[6] = ks.Text.ToSafeString().Trim();
            }
            switch (dataSource)
            {
                case "GDB":
                    ProjectGDB();
                    break;
                case "MDB":
                    ProjectMDB();
                    break;
                default:
                    ProjectIMG();
                    break;
            }
            MessageBox.Show("数据投影转换完成！");
        }
        private void ProjectGDB()
        {
            string fileFullPath = prjFolderPath + "\\" + cbProjectionFileList.SelectedItem.ToString();
            if (workspace_array.Count == 0 || !File.Exists(fileFullPath) || ReprjPositionTextBox.Text.Trim() == string.Empty)
            {
                MessageBox.Show("请检查输入数据库集、输出数据库路径及投影坐标系是否存在！");
                return;
            }
            using (var wo = m_App.SetBusy())
            {
                if (workspace_array.Count > 0)
                {

                    ISpatialReferenceFactory pSpatialRefFactory = new SpatialReferenceEnvironmentClass();
                    targetSpatialRef = pSpatialRefFactory.CreateESRISpatialReferenceFromPRJFile(fileFullPath);
                    if (targetSpatialRef == null)
                    {
                        MessageBox.Show("投影失败！");
                        return;
                    }
                    foreach (var kv in workspace_array)
                    {
                        FileInfo pfileInfo = new FileInfo(kv.Key);
                        IWorkspace outWorkspace = createGDB(ReprjPositionTextBox.Text, pfileInfo.Name.Substring(0, pfileInfo.Name.LastIndexOf(".")));
                        reProjectMDB(kv.Value, outWorkspace, wo);
                    }
                }
            }

        }
        private void ProjectMDB()
        {
            string fileFullPath = prjFolderPath + "\\" + cbProjectionFileList.SelectedItem.ToString();
            if (workspace_array.Count == 0 || !File.Exists(fileFullPath) || ReprjPositionTextBox.Text.Trim() == string.Empty)
            {
                MessageBox.Show("请检查输入数据库集、输出数据库路径及投影坐标系是否存在！");
                return;
            }
            using (var wo = m_App.SetBusy())
            {
                if (workspace_array.Count > 0)
                {

                    ISpatialReferenceFactory pSpatialRefFactory = new SpatialReferenceEnvironmentClass();
                    targetSpatialRef = pSpatialRefFactory.CreateESRISpatialReferenceFromPRJFile(fileFullPath);
                    if (targetSpatialRef == null)
                    {
                        MessageBox.Show("投影失败！");
                        return;
                    }
                    foreach (var kv in workspace_array)
                    {
                        FileInfo pfileInfo = new FileInfo(kv.Key);
                        string mdbName = System.IO.Path.GetFileName(pfileInfo.FullName);
                        IWorkspace outWorkspace = createMDB(ReprjPositionTextBox.Text, mdbName);
                        reProjectMDB(kv.Value, outWorkspace, wo);
                    }
                }
            }

        }
        //投影影像
        private void ProjectIMG()
        {
            string fileFullPath = prjFolderPath + "\\" + cbProjectionFileList.SelectedItem.ToString();
            if (listBox_project_GDB.Items.Count == 0 || !File.Exists(fileFullPath) || ReprjPositionTextBox.Text.Trim() == string.Empty)
            {
                MessageBox.Show("请检查输入数据库集、输出数据库路径及投影坐标系是否存在！");
                return;
            }

            m_Geoprocessor.OverwriteOutput = true;

            try
            {

                using (var wo = m_App.SetBusy())
                {
                    ISpatialReferenceFactory pSpatialRefFactory = new SpatialReferenceEnvironmentClass();
                    targetSpatialRef = pSpatialRefFactory.CreateESRISpatialReferenceFromPRJFile(fileFullPath);
                    if (targetSpatialRef == null)
                    {
                        MessageBox.Show("投影失败！");
                        return;
                    }
                    for (int i = 0; i < listBox_project_GDB.Items.Count; i++)
                    {
                        string input = listBox_project_GDB.Items[i].ToString();
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(input);
                        //首先判断是否NULL
                        wo.SetText("正在处理影像:" + fileName);
                        IWorkspaceFactory wsfct = new RasterWorkspaceFactoryClass();
                        IWorkspace pws = wsfct.OpenFromFile(System.IO.Path.GetDirectoryName(input), 0);
                        if (pws != null)
                        {
                            IRasterWorkspace prasterws = pws as IRasterWorkspace;
                            IRasterDataset prastDs = prasterws.OpenRasterDataset(System.IO.Path.GetFileName(input));
                            if (prastDs == null)
                                return;

                            if ((prastDs as IGeoDataset).SpatialReference.Name == "Unknown")
                            {
                                DefineProjection pro = new DefineProjection();
                                pro.in_dataset = input;
                                pro.coor_system = targetSpatialRef;
                                try
                                {
                                    Helper.ExecuteGPTool(m_Geoprocessor, pro, null);
                                }
                                catch (Exception err)
                                {
                                    MessageBox.Show(err.Message);
                                }
                            }
                            else
                            {

                                ProjectRaster project = new ProjectRaster();
                                project.in_raster = input;
                                project.out_coor_system = targetSpatialRef;
                                string folder = System.IO.Path.GetDirectoryName(input);
                                string extension = System.IO.Path.GetExtension(input);
                                project.out_raster = ReprjPositionTextBox.Text + "\\" + fileName + extension;
                                try
                                {
                                    Helper.ExecuteGPTool(m_Geoprocessor, project, null);
                                }
                                catch (Exception err)
                                {
                                    MessageBox.Show(err.Message);
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }


        private void FrmProjectGDB_Load(object sender, EventArgs e)
        {
            prjFolderPath = GApplication.ExePath + @"\..\Projection";
            DirectoryInfo dir = new DirectoryInfo(prjFolderPath);
            var files = dir.GetFiles();

            for (int i = 0; i < files.Length; i++)
            {
                cbProjectionFileList.Items.Add(files[i].Name);
            }

            if (cbProjectionFileList.Items.Count > 0)
            {
                cbProjectionFileList.SelectedIndex = 0;
            }
        }

        private void rbImg_CheckedChanged(object sender, EventArgs e)
        {
            if (rbMdb.Checked)
            {
                dataSource = rbMdb.Text;
            }
            if (rbGDB.Checked)
            {
                dataSource = rbGDB.Text;
            }
            if (rbImg.Checked)
            {
                dataSource = rbImg.Text;
                lbInfo.Text = "注：影像目前支持tif,img格式";
            }

        }

        private void listBox_project_GDB_SelectedValueChanged(object sender, EventArgs e)
        {
            btnDel.Enabled = listBox_project_GDB.SelectedItems.Count > 0;
            
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            foreach (var item in listBox_project_GDB.SelectedItems)  //选中项遍历  
            {
                listBox_project_GDB.Items.Remove(item); // 移除  
                if (workspace_array.ContainsKey(item.ToString()))
                {
                    workspace_array.Remove(item.ToString());
                }
                if (listBox_project_GDB.SelectedItems.Count == 0)
                    break;
            }
        }


    }
}
