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
    public partial class HotKeyDialog : Form
    {
        GApplication application;
        internal HotKeyDialog(GApplication app)
        {
            application = app;
            InitializeComponent();
            
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var info = e.Node.Tag as CommandInfo;
            if (info != null)
            {
                if (info.Hotkey != null)
                {
                    tBHotKey.Text = info.Hotkey.ToString();
                    buttonDelete.Enabled = true;
                    buttonDelete.Tag = e.Node;
                    //buttonEdit.Enabled = false;
                    buttonEdit.Tag = e.Node;
                    //tBHotKey.Enabled = false;
                }
                else
                {
                    tBHotKey.Text = string.Empty;
                    buttonDelete.Enabled = false;
                    buttonDelete.Tag = e.Node;
                    //buttonEdit.Enabled = true;
                    buttonEdit.Tag = e.Node;
                    //tBHotKey.Enabled = true;
                }
                labelExplanation.Text = "名称：" + info.Command.Caption 
                    + "\n\n说明：" + info.Command.Tooltip;
                cbShowInQuickAcessToolBar.Checked = info.Context.IndexInQuickAcessToolbar >= 0;
            }          
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
                if (info.Command is ISMGIContextMenu)
                    continue;
                if (!rootIdx.ContainsKey(info.Command.Category))
                {
                    int idx = treeView1.Nodes.Add(new TreeNode(info.Command.Category));
                    rootIdx.Add(info.Command.Category, idx);
                }

                int i = rootIdx[info.Command.Category];
                string caption = info.Command.Caption;
                if (info.Hotkey != null) {
                    caption += "(" + info.Hotkey.ToString() + ")";
                }
                var node = new TreeNode(caption);
                node.Tag = info;
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

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            var n = buttonDelete.Tag as TreeNode;
            var info = n.Tag as CommandInfo;
            if(info.Hotkey != null)
            {
                application.HotkeyManager.DeleteHotKey(info);
                n.Text = info.Command.Caption;
                tBHotKey.Text = string.Empty;
                //tBHotKey.Enabled = true;
                buttonDelete.Enabled = false;
                //buttonEdit.Enabled = true;
            }      
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            if (tBHotKey.Text == string.Empty)
                return;

            var n = buttonEdit.Tag as TreeNode;
            var info = n.Tag as CommandInfo;
            HotKeyInfo hKInfo = new HotKeyInfo(tBHotKey.Text);
            bool isOk = application.HotkeyManager.RegistHotKey(hKInfo, info);
            if (isOk)
            {
                n.Text = info.Command.Caption + "(" + hKInfo.ToString() + ")";
                //tBHotKey.Enabled = false;
                buttonDelete.Enabled = true;
                //buttonEdit.Enabled = false;
            }
            else
            {
                MessageBox.Show("编辑快捷键失败！");
            }
        }

        private void tBHotKey_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey
                || e.KeyCode == Keys.RShiftKey
                || e.KeyCode == Keys.ShiftKey
                || e.KeyCode == Keys.RControlKey
                || e.KeyCode == Keys.ProcessKey
                ||e.KeyCode == Keys.Menu
            )
                return;
            
            HotKeyInfo hKInfo = new HotKeyInfo(e);
            tBHotKey.Text = hKInfo.ToString();
            e.SuppressKeyPress = true;
        }

        private void HotKeyDialog_Activated(object sender, EventArgs e)
        {
            treeView1.ExpandAll();
        }

        private void cbShowInQuickAcessToolBar_CheckedChanged(object sender, EventArgs e)
        {
            var n = treeView1.SelectedNode;
            if (n == null)
                return;
            var info = n.Tag as CommandInfo;
            if (info == null)
                return;
            if (cbShowInQuickAcessToolBar.Checked)
            {
                application.PluginManager.AddToQuickAcessToolBar(info);
            }
            else
            {
                application.PluginManager.RemoveFromQuickAcessToolBar(info);
            }
            application.MainForm.Ribbon.QuickAcessToolbar.Visible = true;
        }
    }
}
