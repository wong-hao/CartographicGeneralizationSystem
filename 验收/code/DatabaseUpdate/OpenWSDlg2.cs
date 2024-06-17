﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;

namespace DatabaseUpdate
{
    public partial class OpenWSDlg2 : Form
    {
        public OpenWSDlg2()
        {
            InitializeComponent();
            shpWorkspaceFactory = new ShapefileWorkspaceFactoryClass();
            gdbWorkspaceFactory = new FileGDBWorkspaceFactoryClass();
            accessWorkspaceFactory = new AccessWorkspaceFactoryClass();
            arcInfoWorkspaceFactory = new ArcInfoWorkspaceFactoryClass();
            InitDirTree();
            DomapRegistry reg = new DomapRegistry("OpenWorkspace");
            object path = reg.Load("Path");
            if (path == null || path.ToString() == string.Empty)
            {
              path = Environment.CurrentDirectory;//设置为当前工作环境
            }
            string currentDir = path.ToString(); 
            CurrentDirectory = currentDir;
            dirTree.ExpandAll();
        }

        public string LayerType
        {
            get { return layerTypeTextbox.Text; }
            set
            {
                layerTypeTextbox.Text = value;
                layerTypeTextbox.Enabled = false;
            }
        }
        public string LayerName
        {
            get { return LayerNameTextbox.Text; }
            set
            {
                LayerNameTextbox.Text = value;
                layerTypeTextbox.Enabled = false;
            }
        }
        public string LayerPath
        {
            get 
            {
                return layerPath;
            }
        }
        private string layerPath;

        private void InitDirTree()
        {
            string[] drivers = Directory.GetLogicalDrives();//c,d,e,f

            foreach (string driver in drivers)
            {
                TreeNode node = CreateDirNode(driver);
                if (node != null)
                {
                    dirTree.Nodes.Add(node);
                }
            }

        }

        TreeNode CreateDirNode(string path)
        {
            if (path.Contains("|"))
            {
                //char[] split = new char[] { '|' };
                //string[] p = path.Split(split);
                //TreeNode node = new TreeNode();
                //node.Text = p[p.Length - 1];
                //node.Tag = path;
                //node.ImageIndex = 1;
                //return node;
                return null;
            }
            else
            {
                //文件夹或gdb
                if (Directory.Exists(path))
                {
                    DirectoryInfo di = new DirectoryInfo(path);

                    TreeNode node = new TreeNode();
                    node.Text = di.Name;
                    node.Tag = path;
                    int index = 0;
                    if (!gdbWorkspaceFactory.IsWorkspace(path))
                    {
                        dirImages.Images.Add(path, IconGeter.GetIcon(path, false).ToBitmap());
                        index = dirImages.Images.IndexOfKey(path);
                    }
                    node.ImageIndex = index;
                    return node;


                }
                //mdb
                else if (File.Exists(path))
                {
                    if (accessWorkspaceFactory.IsWorkspace(path))
                    {
                        //FileInfo fi = new FileInfo(path);
                        //TreeNode node = new TreeNode(); 
                        //node.Text = fi.Name;
                        //node.Tag = path;
                        //node.ImageIndex = 0;
                        //return node;
                        return null;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }            
        }

        private void ClearImageList()
        {
            Image gdbImg = dirImages.Images[0].Clone() as Image;
            Image dsImg = dirImages.Images[1].Clone() as Image;
            Image lyrImg = dirImages.Images[2].Clone() as Image;

            foreach (Image im in dirImages.Images)
            {
                im.Dispose();
            }

            dirImages.Images.Clear();
            dirImages.Images.Add(gdbImg);
            dirImages.Images.Add(dsImg);
            dirImages.Images.Add(lyrImg);
        }

        private void AddItemsToDirTree(string path)
        {
            string dirPath = path;
            string[] p = null;
            if (path.Contains("|"))
            {
                //char[] split = new char[] { '|' };
                //p = path.Split(split);
                //dirPath = p[0];
                return;
            }
            DirectoryInfo di = new DirectoryInfo(dirPath);

            foreach (TreeNode node in dirTree.Nodes)
            {
                node.Nodes.Clear();

                if (di.Root.FullName == node.Tag.ToString()) //node:c\d\e\f
                {
                    TreeNode childNode = null;
                    TreeNode endNode = node;

                    //生成目录部分
                    while (di.FullName != di.Root.FullName)//到c/d/e/f时相等
                    {
                        TreeNode parentNode = CreateDirNode(di.FullName);

                        if (childNode != null)
                        {
                            parentNode.Nodes.Add(childNode);
                        }
                        else
                        {
                            endNode = parentNode;
                        }

                        childNode = parentNode;
                        di = di.Parent; //上一层
                    }
                    if (childNode != null)
                    {
                        node.Nodes.Add(childNode);
                    }

                    //生成数据集部分
                    if (p != null)
                    {
                        //for (int i = 1; i < p.Length; i++)
                        //{
                        //    TreeNode dsNode = new TreeNode();
                        //    dsNode.Text = p[i];
                        //    string tag = "";
                        //    for (int j = 0; j < i; j++)
                        //    {
                        //        if (j == 0)
                        //        {
                        //            tag = p[j];
                        //        }
                        //        else
                        //        {
                        //            tag += "|" + p[j];
                        //        }
                        //    }
                        //    dsNode.Tag = tag;
                        //    dsNode.ImageIndex = 1;
                        //    endNode.Nodes.Add(dsNode);
                        //    endNode = dsNode;
                        //}
                        if (p[1] != null)
                        {
                            return;
                        }
                    }
                    dirTree.SelectedNode = endNode;
                    dirTree.SelectedImageIndex = endNode.ImageIndex;
                }
            }
        }

        public string CurrentDirectory
        {
            get
            {
                return currentDirectory;
            }
            set
            {
                if (!System.IO.Directory.Exists(value))
                    return;
                currentDirectory = value;
                comboBox1.Items.Clear();
                comboBox1.Items.Add(value);           //当前目录改变时，combox1的text和layerview也都跟着改变
                comboBox1.SelectedIndex = 0;
                AddItemsToLayerView(value);                
            }
        }
        private string currentDirectory;

        private void AddItemsToLayerView(string path)
        {
            layerList.Items.Clear();
            //如果包含|:说明这是gdb、mdb内部的dataset
            if (path.Contains("|"))
            {
                //char[] split = new char[]{'|'};
                //string[] p = path.Split(split);
                //IWorkspace workspace = null;
                //if (gdbWorkspaceFactory.IsWorkspace(p[0]))
                //{
                //    workspace = gdbWorkspaceFactory.OpenFromFile(p[0], 0);                    
                //}
                //else
                //{
                //    workspace = accessWorkspaceFactory.OpenFromFile(p[0], 0);
                //}
                //IDataset ds = workspace as IDataset;
                //for (int i = 1; i < p.Length; i++)
                //{
                //    IEnumDataset eds = ds.Subsets;  
                //    while ((ds = eds.Next()) != null)  //ds下一级中是否有p【i】
                //    {
                //        if (ds.Name == p[i] && ds is IFeatureDataset)
                //        {
                //            break;
                //        }
                //    }
                //    if (ds == null)  //ds下一级中如果匹配p【i】，则不会为空
                //    {
                //        return;
                //    }
                //}
                //AddDatasetItemsToLayerView(ds);
                return;
            }
            else
            {
                if (gdbWorkspaceFactory.IsWorkspace(path))
                {
                    //IWorkspace gdbWorkspace = gdbWorkspaceFactory.OpenFromFile(path, 0);
                    //AddDatasetItemsToLayerView(gdbWorkspace as IDataset);
                    AddGDBItemsToLayerView(path);
                    return;
                }
                else if (accessWorkspaceFactory.IsWorkspace(path))
                {
                    //IWorkspace accessWorkspace = accessWorkspaceFactory.OpenFromFile(path, 0);
                    //AddDatasetItemsToLayerView(accessWorkspace as IDataset);
                    return;
                }
                else
                {
                    try
                    {
                        DirectoryInfo d1 = new DirectoryInfo(path);
                        foreach (DirectoryInfo di in d1.GetDirectories()) //遍历子文件
                        {
                            layerList.Items.Add(CreateDirItem(di.FullName));
                        }
                        //foreach (FileInfo fi in d1.GetFiles())
                        //{
                        //    ListViewItem item = CreateFileItem(fi.FullName);
                        //    if (item != null)
                        //    {
                        //        layerList.Items.Add(item);
                        //    }
                        //}
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("文件夹不可访问：权限不够！");
                    }
                }
            }
        }

        private void AddGDBItemsToLayerView(string path)
        {
            if (!gdbWorkspaceFactory.IsWorkspace(path))
            {
                layerList.Items.Add(CreateDirItem(path));
            }
        }

        //private void AddDatasetItemsToLayerView(IDataset ds)
        //{
        //    IEnumDataset eds = ds.Subsets;
        //    IDataset d = null;
        //    while ((d = eds.Next()) != null)
        //    {
        //        if (d is IFeatureClass || d is IFeatureDataset)
        //        {
        //            layerList.Items.Add(CreateDatasetItem(d));
        //        }
        //    }
        //}

        //private ListViewItem CreateDatasetItem(IDataset ds)
        //{
        //    ListViewItem item = new ListViewItem();
        //    item.Text = ds.Name;
        //    item.Tag = CurrentDirectory + "|" + ds.Name; // "|":自己定义的
        //    if (ds is IFeatureClass)
        //    {
        //        item.ImageIndex = 2;
        //    }
        //    else
        //    {
        //        item.ImageIndex = 1;
        //    }
        //    return item;
        //}
        private ListViewItem CreateDirItem(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            ListViewItem item = new ListViewItem();
            item.Text = di.Name;
            item.Tag = path;
            int index = 0;
            if (!gdbWorkspaceFactory.IsWorkspace(path))
            {
                dirImages.Images.Add(path, IconGeter.GetIcon(path, false).ToBitmap());
                index = dirImages.Images.IndexOfKey(path);
            }
            item.ImageIndex = index;
            return item;
        }
        //private ListViewItem CreateFileItem(string path)
        //{
        //    FileInfo fi = new FileInfo(path);
        //    string ext = fi.Extension.ToLower();
        //    if (ext == ".mdb")
        //    {
        //        if (!accessWorkspaceFactory.IsWorkspace(path))
        //        {
        //            return null;
        //        }
        //        else
        //        {
        //            ListViewItem item = new ListViewItem();
        //            item.Text = fi.Name;
        //            item.Tag = path;
        //            item.ImageIndex = 0;
        //            return item;
        //        }
        //    }
        //    else if (ext == ".shp")
        //    {
        //        ListViewItem item = new ListViewItem();
        //        item.Text = fi.Name;
        //        item.Tag = path;
        //        item.ImageIndex = 2;
        //        return item;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        private IWorkspaceFactory shpWorkspaceFactory;
        private IWorkspaceFactory gdbWorkspaceFactory;
        private IWorkspaceFactory accessWorkspaceFactory;
        private IWorkspaceFactory arcInfoWorkspaceFactory;

        #region 事件响应

        private void comboBox1_Click(object sender, EventArgs e) //点击combobox1下拉键后。。。
        {
            AddItemsToDirTree(comboBox1.Text);
            dirTree.Visible = true;
            this.ActiveControl = dirTree;
        }

        private void AddDataDlg_Load(object sender, EventArgs e)
        {
            //clearDirImage();
            
        }

        private void dirTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)  //点击tree的节点
        {
            dirTree.Visible = false;
            CurrentDirectory = e.Node.Tag.ToString();
        }

        private void layerList_DoubleClick(object sender, EventArgs e)
        {
            ListViewItem item = layerList.FocusedItem;
            if (item == null)
            {
                return;
            }
            if (gdbWorkspaceFactory.IsWorkspace(item.Tag.ToString()))
            {
                layerPath = item.Tag.ToString();
                if (Check())
                {
                  DomapRegistry reg = new DomapRegistry("OpenWorkspace");
                  reg.Save("Path", layerPath);
                    DialogResult = DialogResult.OK;
                }
            }
            else
            {
                CurrentDirectory = item.Tag.ToString();
            }
            //if (item.ImageIndex != 2)
            //{
            //    CurrentDirectory = item.Tag.ToString();
            //}
            //else
            //{
            //    layerPath = item.Tag.ToString();
            //    if (Check())
            //    {
            //        DialogResult = DialogResult.OK;
            //    }
            //}
        }

        private void dirTree_Leave(object sender, EventArgs e)
        {
            dirTree.Visible = false;
        }

        private void upButton_Click(object sender, EventArgs e)
        {
            if (!CurrentDirectory.Contains("|"))
            {
                DirectoryInfo di = new DirectoryInfo(CurrentDirectory);
                if (di.Parent != null)
                    CurrentDirectory = di.Parent.FullName;
            }
            else
            {
                char[] split = new char[] { '|' };
                string[] p = CurrentDirectory.Split(split);
                string parent = "";
                for (int i = 0; i < p.Length - 1; i++)
                {
                    if (i == 0)
                    {
                        parent += p[i];
                    }
                    else
                    {
                        parent += "|" + p[i];
                    }
                }
                CurrentDirectory = parent;
            }
        }
        #endregion

        private void okButton_Click(object sender, EventArgs e)
        {
            ListViewItem item = layerList.FocusedItem;
            if (item == null)
            {
                return;
            }
            //if (item.ImageIndex != 2) //如果不是层，则继续进入下一级目录
            //{
            //    CurrentDirectory = item.Tag.ToString();
            //}
            //else
            //{
            //    layerPath = item.Tag.ToString();
            //    if (Check())
            //    {
            //        DialogResult = DialogResult.OK;
            //    }
            //}
            if (item.ImageIndex == 0)
            {
                layerPath = item.Tag.ToString();
                if (Check())
                {
                  DomapRegistry reg = new DomapRegistry("OpenWorkspace");
                  reg.Save("Path", layerPath);
                    DialogResult = DialogResult.OK;
                }
            }
            else
            {
                return;
            }
        }

        private bool Check()
        {
            string nameNull = "工作区名称为空!";
            string nameErro = "工作区名不能包括“./\\:*?\"<>|”";
            string typeErro = "类型名不能包括“/\\:*?\"<>|”";
            string[] notContain = new string[] { "/","\\",":","*","?","<",">","|"};
            //if (LayerName == "")
            //{
            //    MessageBox.Show(nameNull);
            //    return false;
            //}
            if (LayerName.Contains("."))
            {
                MessageBox.Show(nameErro);
                return false;
            }
            foreach (string n in notContain)
            {
                if (LayerName.Contains(n))
                {
                    MessageBox.Show(nameErro);
                    return false;
                }
            }
            foreach (string n in notContain)
            {
                if (LayerType.Contains(n))
                {
                    MessageBox.Show(typeErro);
                    return false;
                }
            }
            return true;
        }



    }
}
