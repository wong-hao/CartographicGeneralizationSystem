using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Common
{
    public partial class CustomToolsDailog : Form
    {
        GApplication application;

        internal CustomToolsDailog(GApplication app)
        {
            application = app;
            InitializeComponent();
        }

        private void CustomToolsDailog_Load(object sender, EventArgs e)
        {
            this.ShowTreeView(application.HotkeyManager.Commands.Values.ToArray());
        }

        internal void ShowTreeView(CommandInfo[] infos)
        {
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            Dictionary<string, int> rootIdx = new Dictionary<string, int>();

            if (treeView1.ImageList == null)
            {
                treeView1.ImageList = new ImageList();
                treeView1.ImageList.Images.Add(AutoResource.制图工具箱);
            }

            foreach (CommandInfo info in infos)
            {
                if (!rootIdx.ContainsKey(info.Command.Category))
                {
                    int idx = treeView1.Nodes.Add(new TreeNode(info.Command.Category));
                    rootIdx.Add(info.Command.Category, idx);
                }

                int i = rootIdx[info.Command.Category];
                string caption = info.Command.Caption;
                if (info.Hotkey != null)
                {
                    caption += "(" + info.Hotkey.ToString() + ")";
                }
                var node = new TreeNode(caption);
                node.Tag = info;
                //node.Checked = true;
                int j = treeView1.Nodes[i].Nodes.Add(node);

                Image img = Image.FromHbitmap(new IntPtr(info.Command.Bitmap));
                if (img == null)
                    continue;
                treeView1.ImageList.Images.Add(img);
                treeView1.Nodes[i].Nodes[j].ImageIndex = treeView1.ImageList.Images.Count - 1;
                treeView1.Nodes[i].Nodes[j].SelectedImageIndex = treeView1.ImageList.Images.Count - 1;
            }

            treeView1.EndUpdate();
            treeView1.ExpandAll();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            buttonUp.Enabled = true;
            buttonDown.Enabled = true;

            TreeNode selectedNode = treeView1.SelectedNode;
            if (selectedNode != null)
            {
                TreeNode parentNode = selectedNode.Parent;
                if (parentNode == null)
                    return;
                int index = parentNode.Nodes.IndexOf(selectedNode);
                if (index == 0)
                {
                    buttonUp.Enabled = false;
                }
                if (index == parentNode.Nodes.Count - 1)
                {
                    buttonDown.Enabled = false;
                }
            }
            
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {   
            //application.PluginManager.CatagoryInfos.Values.
    
            foreach (TreeNode treenodeparent in treeView1.Nodes)
            {
                foreach (TreeNode treenodechild in treenodeparent.Nodes)
                {
                    if (!treenodechild.Checked)
                    {
                        //for(int i=0;i<toolItems.count;i++)
                        //{
                        //    if (treenodechild.Text == toolItems[i].Text)
                        //    {
                        //        toolItems.remove(toolItems[i]);
                        //    }
                        //}
                    }
                }
            }
        }

        private void buttonUp_Click(object sender, EventArgs e)
        {   
            TreeNode selectedNode = treeView1.SelectedNode;
            if (selectedNode != null)
            {
                TreeNode parentNode = selectedNode.Parent;
                if (parentNode == null)
                    return;
                int index = parentNode.Nodes.IndexOf(selectedNode);

                parentNode.Nodes.Remove(selectedNode);
                index -= 1;
                parentNode.Nodes.Insert(index, selectedNode);
                treeView1.SelectedNode = selectedNode;
            }
        }

        private void buttonDown_Click(object sender, EventArgs e)
        {                     
            TreeNode selectedNode = treeView1.SelectedNode;
            if (selectedNode != null)
            {
                TreeNode parentNode = selectedNode.Parent;
                if (parentNode == null)
                    return;
                int index = parentNode.Nodes.IndexOf(selectedNode);

                parentNode.Nodes.Remove(selectedNode);
                index += 1;
                parentNode.Nodes.Insert(index, selectedNode);
                treeView1.SelectedNode = selectedNode;
            }
        }

        //private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        //{           
        //    if (e.Action != TreeViewAction.Unknown)
        //    {
        //        SetNodeCheckStatus(e.Node, e.Node.Checked);
        //        SetNodeStyle(e.Node);
        //    }  
        //}
       
        //private void SetNodeCheckStatus(TreeNode tn, bool Checked)
        //{

        //    if (tn == null) return;
        //    foreach (TreeNode tnChild in tn.Nodes)
        //    {

        //        tnChild.Checked = Checked;

        //        SetNodeCheckStatus(tnChild, Checked);

        //    }
        //    TreeNode tnParent = tn;
        //}
        
        //private void SetNodeStyle(TreeNode Node)
        //{
        //    int nNodeCount = 0;
        //    if (Node.Nodes.Count != 0)
        //    {
        //        foreach (TreeNode tnTemp in Node.Nodes)
        //        {

        //            if (tnTemp.Checked == true)

        //                nNodeCount++;
        //        }

        //        if (nNodeCount == Node.Nodes.Count)
        //        {
        //            Node.Checked = true;
        //            //Node.ExpandAll();
        //            Node.ForeColor = Color.Black;
        //        }
        //        else if (nNodeCount == 0)
        //        {
        //            Node.Checked = false;
        //            //Node.Collapse();
        //            Node.ForeColor = Color.Black;
        //        }
        //        else
        //        {
        //            Node.Checked = true;
        //            Node.ForeColor = Color.Gray;
        //        }
        //    }
        //    //当前节点选择完后，判断父节点的状态，调用此方法递归。  
        //    if (Node.Parent != null)
        //        SetNodeStyle(Node.Parent);
        //}

    }
}
