using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace ECartoCommandManager
{
    public partial class CommandManager : Form
    {
        /// <summary>
        /// xml文件
        /// </summary>
        string xmlpath = @"..\Template\宁夏多尺度地图库生产\Commands2.xml";

        /// <summary>
        /// 图标资源文件目录
        /// </summary>
        string imgDir = @"..\Resource\";

        /// <summary>
        /// 右键鼠标点击位置
        /// </summary>
        System.Drawing.Point clickPt; 
        public Dictionary<string, Command> cmdD = new Dictionary<string, Command>();
        
        public CommandManager()
        {            
            InitializeComponent();
            cmdD.Clear();
            textBoxXMLpath.Text = xmlpath;
            LoadXML(xmlpath);
        }

        public CommandManager(string setPath)
        {            
            xmlpath = setPath; //传递外置参数
            InitializeComponent();
            cmdD.Clear();
            textBoxXMLpath.Text = xmlpath;
            LoadXML(xmlpath);
        }
        /// <summary>
        /// 加载XML文件，更新treeViewCommand
        /// </summary>
        /// <param name="xmlpath"></param>
        public bool LoadXML(string xmlpath)
        {
            try
            {
                cmdD.Clear();
                treeViewCommand.Nodes.Clear();
                SetNoInfo();

                XDocument xmlDoc = XDocument.Load(xmlpath);
                XElement group1 = xmlDoc.Element("Group");
                string caption1 = group1.Element("Caption").Value;
                TreeNode treeNode1 = new TreeNode(caption1);
                treeViewCommand.Nodes.Add(treeNode1);
                XElement Children1 = group1.Element("Children");
                var group2s = Children1.Elements("Group");
                foreach (XElement group2 in group2s)
                {
                    string caption2 = group2.Element("Caption").Value;
                    TreeNode treeNode2 = new TreeNode(caption2);
                    treeNode1.Nodes.Add(treeNode2);
                    XElement Children2 = group2.Element("Children");
                    var group3s = Children2.Elements("Group");
                    foreach (XElement group3 in group3s)
                    {
                        string caption3 = group3.Element("Caption").Value;
                        TreeNode treeNode3 = new TreeNode(caption3);
                        treeNode2.Nodes.Add(treeNode3);
                        var commands = group3.Element("Children").Elements("Command");
                        foreach (XElement command in commands)
                        {
                            string caption4 = command.Element("Caption").Value;
                            TreeNode treeNode4 = new TreeNode(caption4);
                            treeNode3.Nodes.Add(treeNode4);

                            string className = command.Element("ClassName").Value;

                            string toolTip = null;
                            if (command.Element("ToolTip") != null)
                                toolTip = command.Element("ToolTip").Value;

                            string hotKey = null;
                            if (command.Element("HotKey") != null)
                                hotKey = command.Element("HotKey").Value;

                            string image = null;
                            if (command.Element("Image") != null)
                                image = command.Element("Image").Value;

                            string qOrder = null;
                            if (command.Element("QOrder") != null)
                                qOrder = command.Element("QOrder").Value;
                            Command cmd = new Command(className, caption4, toolTip, hotKey, image, qOrder);
                            cmdD.Add(caption4, cmd);
                        }
                    }
                }
                return true;
            }
            catch
            {
                textBoxXMLpath.Text = "";
                treeViewCommand.Nodes.Clear();
                SetNoInfo();
                return false;
            }            
        }

        /// <summary>
        /// 保存cmd信息到XML文件
        /// </summary>
        /// <param name="cmdSet"></param>
        /// <param name="xmlpath"></param>
        public void XMLUpdateCmd(Command cmdSet,string xmlpath)
        {
            if (cmdSet == null)
                return;            

            XDocument xmlDoc = XDocument.Load(xmlpath);
            XElement group1 = xmlDoc.Element("Group");
            string caption1 = group1.Element("Caption").Value;
            TreeNode treeNode1 = new TreeNode(caption1);
            
            XElement Children1 = group1.Element("Children");
            var group2s = Children1.Elements("Group");
            foreach (XElement group2 in group2s)
            {
                string caption2 = group2.Element("Caption").Value;
                TreeNode treeNode2 = new TreeNode(caption2);                
                XElement Children2 = group2.Element("Children");
                var group3s = Children2.Elements("Group");
                foreach (XElement group3 in group3s)
                {
                    string caption3 = group3.Element("Caption").Value;
                    TreeNode treeNode3 = new TreeNode(caption3);
                    
                    var commands = group3.Element("Children").Elements("Command");
                    foreach (XElement command in commands)
                    {
                        string caption4 = command.Element("Caption").Value;
                        if (caption4 != cmdSet.Caption)
                            continue;
                        else
                        {
                            bool change = false;
                            string className = command.Element("ClassName").Value;
                            if (className != cmdSet.ClassName)
                            {
                                command.Element("ClassName").Value = cmdSet.ClassName;
                                change = true; 
                            }

                            if(TryUpdateXMLNode(command, "ToolTip", cmdSet.ToolTip))
                                change=true;
                            if (TryUpdateXMLNode(command, "HotKey", cmdSet.HotKey))
                                change = true;
                            if (TryUpdateXMLNode(command, "Image", cmdSet.Image))
                                change = true;
                            if (TryUpdateXMLNode(command, "QOrder", cmdSet.QOrder))
                                change = true;
                            if(change)
                                xmlDoc.Save(xmlpath);                             
                            break;
                        }                        
                    }                        
                }
            }            
        }

        /// <summary>
        /// 修改XML的cmd节点值（属性、属性值），如有修改则返回true否则false
        /// </summary>
        /// <param name="ele"></param>
        /// <param name="attName"></param>
        /// <param name="setValue"></param>
        /// <returns></returns>
        private bool TryUpdateXMLNode(XElement ele,string attName,string setValue)
        {
            bool change=false;
            var toolTip = ele.Element(attName);
            if (String.IsNullOrEmpty(setValue))
            {                                
                if (toolTip != null && toolTip.Value != "")
                {
                    toolTip.Value = "";
                    change = true;
                }
            }
            else
            {
                if (toolTip == null)
                {
                    ele.Add(new XElement(attName,setValue));
                    change = true;
                }
                else
                {
                    if (toolTip.Value != setValue)
                    {
                        toolTip.Value = setValue;
                        change = true; 
                    }
                } 
            }
            return change; 
        }

        /// <summary>
        /// treeViewCommand的鼠标点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeViewCommand_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            clickPt = new System.Drawing.Point(e.X, e.Y);
            TreeNode node = e.Node;
            if (node.Level == 3)
            {
                string nodeText = node.Text;
                Command cmd = null;
                if (cmdD.TryGetValue(nodeText, out cmd))
                {
                    SetInfo(cmd);
                }
                else
                {
                    MessageBox.Show("无效命令");                    
                }
            }
            else
            {
                SetNoInfo();
            }
            if (e.Button == MouseButtons.Right)
            {   
                node.ContextMenuStrip = contextMenuStrip1;

                //能删除的节点条件（叶子节点 && 非根节点）                
                this.toolStripMenuItem1.Enabled = node.GetNodeCount(true) == 0 && node.Level != 0;
                
                //根节点不能改名                
                this.toolStripMenuItem2.Enabled = node.Level != 0; 
                
                //叶子节点不能插入
                this.toolStripMenuItem3.Enabled = node.Level != 3;
                
                contextMenuStrip1.Show();                
            }
        }

        /// <summary>
        /// 第一条右键命令-删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            TreeNode node = treeViewCommand.GetNodeAt(clickPt);
            TreeNode parent = node;

            List<string> nodeTexts = new List<string>();
            while (parent != null)
            {
                nodeTexts.Insert(0,parent.Text);
                parent = parent.Parent; 
            }
            XDocument xmlDoc = XDocument.Load(xmlpath);
            XElement ele;
            if (LocateXMLNote(xmlDoc, nodeTexts, out ele))
            {
                ele.Remove();
                xmlDoc.Save(xmlpath);//xml删除
                if (node.Level == 3)
                    cmdD.Remove(node.Text);//命令字典删除
                node.Remove();//页面节点删除
                MessageBox.Show("已删除");
            }           
        }

        /// <summary>
        /// 第二条右键命令-修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            TreeNode node = treeViewCommand.GetNodeAt(clickPt);
            string oldName = node.Text;

            TreeNode parent = node;            
            List<string> nodeTexts = new List<string>();
            while (parent != null)
            {
                nodeTexts.Insert(0, parent.Text);
                parent = parent.Parent;
            }

            XDocument xmlDoc = XDocument.Load(xmlpath);
            XElement ele;
            if (LocateXMLNote(xmlDoc, nodeTexts, out ele))
            {
                FormCaption formCaption = new FormCaption(oldName);
                formCaption.Location = clickPt;
                if(formCaption.ShowDialog()==DialogResult.OK)
                {
                    string newName = formCaption.inputString;                    
                    if (!String.IsNullOrEmpty(newName) && newName != oldName)
                    {
                        if (node.Level == 3)
                        { 
                            textBoxCaption.Text = newName;
                            Command cmd = cmdD[oldName];
                            cmd.Caption = newName;
                            cmdD.Remove(oldName);
                            cmdD.Add(newName, cmd);
                        }

                        ele.Element("Caption").Value = newName;
                        xmlDoc.Save(xmlpath);

                        node.Text = newName;

                        MessageBox.Show("已修改");
                    }
                }
            }
        }

        /// <summary>
        /// 第三条右键命令-插入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            TreeNode node = treeViewCommand.GetNodeAt(clickPt);
            string oldName = node.Text;

            TreeNode parent = node;
            List<string> nodeTexts = new List<string>();
            while (parent != null)
            {
                nodeTexts.Insert(0, parent.Text);
                parent = parent.Parent;
            }

            XDocument xmlDoc = XDocument.Load(xmlpath);
            XElement ele;
            if (LocateXMLNote(xmlDoc, nodeTexts, out ele))
            {
                if (node.Level >= 2) //插入命令
                {
                    FormCommand formCommand = new FormCommand();
                    formCommand.Text = "插入命令";
                    formCommand.Location = clickPt;
                    if (formCommand.ShowDialog() == DialogResult.OK)
                    {
                        if (String.IsNullOrEmpty(formCommand.ClassName))
                            return;
                        if (String.IsNullOrEmpty(formCommand.Caption))
                            return;

                        if (cmdD.ContainsKey(formCommand.Caption))
                        {
                            MessageBox.Show("已存在：" + formCommand.Caption);
                            return;
                        }
                        else
                        {
                            Command cmd = new Command(formCommand.ClassName,
                                                    formCommand.Caption,
                                                    formCommand.ToolTip,
                                                    formCommand.HotKey,
                                                    formCommand.Image,
                                                    formCommand.QOrder);
                            cmdD.Add(formCommand.Caption, cmd); //cmdD增加内容
                            if (node.Level == 2)
                                node.Nodes.Add(formCommand.Caption); //视图内增加节点
                            else if (node.Level == 3)
                            {
                                node.Parent.Nodes.Add(formCommand.Caption);
                            }

                            //xml文件内增加节点
                            XElement xele = new XElement("Command");
                            xele.Add(new XElement("ClassName", formCommand.ClassName));
                            xele.Add(new XElement("Caption", formCommand.Caption));
                            xele.Add(new XElement("ToolTip", formCommand.ToolTip));
                            xele.Add(new XElement("HotKey", formCommand.HotKey));
                            xele.Add(new XElement("Image", formCommand.Image));
                            xele.Add(new XElement("QOrder", formCommand.QOrder));
                            if (node.Level == 2)
                                ele.Element("Children").Add(xele);
                            else if (node.Level == 3)
                                ele.AddAfterSelf(xele);//命令处，增加在后面
                            xmlDoc.Save(xmlpath);
                        }
                        
                        

                        //
                    }
                }
                else //插入目录（0/1）
                {
                    FormCaption formCaption = new FormCaption();
                    formCaption.Text = "插入项名称";
                    formCaption.Location = clickPt;
                    if (formCaption.ShowDialog() == DialogResult.OK)
                    {
                        string newDir = formCaption.inputString;
                        if (!String.IsNullOrEmpty(newDir))
                        {
                            XElement xele = new XElement("Group");
                            xele.Add(new XElement("Caption", newDir));
                            xele.Add(new XElement("Children", ""));
                            ele.Element("Children").Add(xele);
                            xmlDoc.Save(xmlpath);

                            TreeNode newNode = new TreeNode(newDir);
                            node.Nodes.Add(newNode);

                            MessageBox.Show("已插入");
                        }
                    }
                }                
            }                      
        }
       
        /// <summary>
        /// 鼠标按下的时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeViewCommand_MouseDown(object sender, MouseEventArgs e)
        {

        }

        /// <summary>
        /// 置命令页面元素-为空
        /// </summary>
        public void SetNoInfo()
        {
            textBoxClassName.Text = "";
            textBoxCaption.Text = "";
            textBoxToolTip.Text = "";
            textBoxHotKey.Text = "";
            textBoxQOrder.Text = "";
            textBoxImage.Text = "";
            pictureBox1.Image = null;
            
        }

        /// <summary>
        /// 设置命令页面元素-正常显示
        /// </summary>
        /// <param name="cmd"></param>
        public void SetInfo(Command cmd)
        {
            textBoxClassName.Text = cmd.ClassName;
            textBoxCaption.Text = cmd.Caption;
            if (!String.IsNullOrEmpty(cmd.ToolTip))
                textBoxToolTip.Text = cmd.ToolTip;
            else
                textBoxToolTip.Text = "";
            if (!String.IsNullOrEmpty(cmd.HotKey))
                textBoxHotKey.Text = cmd.HotKey;
            else
                textBoxHotKey.Text = "";
            if (!String.IsNullOrEmpty(cmd.QOrder))
                textBoxQOrder.Text = cmd.QOrder;
            else
                textBoxQOrder.Text = "";
            if (!String.IsNullOrEmpty(cmd.Image))
            {
                textBoxImage.Text = cmd.Image;
                string imgPath = System.IO.Path.Combine(imgDir, textBoxImage.Text);
                if (File.Exists(imgPath))
                {
                    pictureBox1.Image = Image.FromFile(imgPath);
                }
                else
                {
                    pictureBox1.Image = null;
                }
            }
            else
            {
                textBoxImage.Text = "";
                pictureBox1.Image = null;
            }
            
        }

        /// <summary>
        /// 从命令页面元素获得Command对象
        /// </summary>
        /// <returns></returns>
        public Command GetInfo()
        {
            if (String.IsNullOrEmpty(textBoxClassName.Text))
                return null;
            if (String.IsNullOrEmpty(textBoxCaption.Text))
                return null;
            return new Command(textBoxClassName.Text,
                               textBoxCaption.Text,
                               textBoxToolTip.Text,
                               textBoxHotKey.Text,
                               textBoxImage.Text,
                               textBoxQOrder.Text); 
        }

        /// <summary>
        /// 保存按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            Command cmdSet = GetInfo();
            XMLUpdateCmd(cmdSet,xmlpath);
            if (cmdD.ContainsKey(cmdSet.Caption))
            {
                cmdD[cmdSet.Caption] = cmdSet;
            }
        }

        /// <summary>
        /// 节点选中状态切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeViewCommand_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;
            if (node.Level == 3)
            {
                string nodeText = node.Text;
                Command cmd = null;
                if (cmdD.TryGetValue(nodeText, out cmd))
                {
                    SetInfo(cmd);
                }               
            }
            else
            {
                SetNoInfo();
            }  
        }

        private bool LocateXMLNote(XDocument xmlDoc,List<string>locater,out XElement ele)
        {
            ele = null;
            if (locater.Count == 0)
                return false;
            XElement eleCurrent = xmlDoc.Element("Group");//所有命令节点
            if (locater.Count >= 1)
            {

                if (eleCurrent.Element("Caption").Value == locater[0])
                {
                    ele = eleCurrent;
                    if(locater.Count == 1)
                        return true;
                }
                else
                {
                    return false;
                }               
            }
            if (locater.Count >= 2)
            {
                bool findDone = false;
                foreach (XElement xele in eleCurrent.Element("Children").Elements("Group"))
                {                    
                    if (xele.Element("Caption").Value == locater[1])
                    {
                        findDone = true;
                        if (locater.Count == 2)
                        {
                            ele = xele;
                            return true;
                        }
                        else
                        {
                            findDone = true;
                            eleCurrent = xele;
                            break;
                        }                       
                    }                    
                }
                if (!findDone)
                    return false; 
            }
            if (locater.Count >= 3)
            {
                bool findDone = false;
                foreach (XElement xele in eleCurrent.Element("Children").Elements("Group"))
                {
                    if (xele.Element("Caption").Value == locater[2])
                    {
                        findDone = true;
                        if (locater.Count == 3)
                        {
                            ele = xele;
                            return true;
                        }
                        else
                        {
                            findDone = true;
                            eleCurrent = xele;
                            break;
                        }
                    }
                }
                if (!findDone)
                    return false;                
            }
            if (locater.Count == 4)
            {
                bool findDone = false;
                foreach (XElement xele in eleCurrent.Element("Children").Elements("Command"))
                {
                    if (xele.Element("Caption").Value == locater[3])
                    {
                        findDone = true;
                        ele = xele;
                        return true;
                    }
                }
                if (!findDone)
                    return false;
                
            }            
            return false;
        }

        private void buttonOpenXML_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog() { 
                Filter = "命令配置文件(*.XML)|*.XML" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (LoadXML(ofd.FileName))
                {
                    xmlpath = ofd.FileName;
                    textBoxXMLpath.Text = ofd.FileName;
                }
                else
                {
                    MessageBox.Show("无效的XML文件"); 
                }
            }
        }

        private void button_up_Click(object sender, EventArgs e)
        {
            TreeNode treeNode = treeViewCommand.SelectedNode;

            List<string> nodeTexts = new List<string>();
            TreeNode parentEle = treeNode;
            while (parentEle != null)
            {
                nodeTexts.Insert(0, parentEle.Text);
                parentEle = parentEle.Parent;
            }

            if (treeNode == null) //没选择，不移动
                return;
            if (treeNode.Level == 0) //根节点，不移动
                return;
            if (treeNode.PrevNode == null) //已是首节点，不移动
                return;

            TreeNode parent = treeNode.Parent;
            int i = treeNode.Index;
            TreeNode tempTreeNode = (TreeNode)treeNode.Clone();
            parent.Nodes.Insert(i - 1, tempTreeNode);           
            treeNode.Remove();
            treeViewCommand.SelectedNode = tempTreeNode;

            XDocument xmlDoc = XDocument.Load(xmlpath);           

            XElement ele;
            if (LocateXMLNote(xmlDoc, nodeTexts, out ele))
            {
                XNode preNode = ele.PreviousNode;
                preNode.AddBeforeSelf(ele);
                ele.Remove();
                xmlDoc.Save(xmlpath);
            }
        }

        private void button_down_Click(object sender, EventArgs e)
        {
            TreeNode treeNode = treeViewCommand.SelectedNode;

            List<string> nodeTexts = new List<string>();
            TreeNode parentEle = treeNode;
            while (parentEle != null)
            {
                nodeTexts.Insert(0, parentEle.Text);
                parentEle = parentEle.Parent;
            }

            if (treeNode == null) //没选择，不移动
                return;
            if (treeNode.Level == 0) //根节点，不移动
                return;
            if (treeNode.NextNode == null) //已是尾节点，不移动
                return;

            TreeNode parentNode = treeNode.Parent;
            int i = treeNode.Index;
            TreeNode tempTreeNode = (TreeNode)treeNode.Clone();
            parentNode.Nodes.Insert(i + 2, tempTreeNode);
            treeNode.Remove();
            treeViewCommand.SelectedNode = tempTreeNode;

            XDocument xmlDoc = XDocument.Load(xmlpath);            
            
            XElement ele;
            if (LocateXMLNote(xmlDoc, nodeTexts, out ele))
            {
                XNode nextNode = ele.NextNode;
                nextNode.AddAfterSelf(ele);
                ele.Remove();
                xmlDoc.Save(xmlpath);
            }
        }

        private void textBoxHotKey2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back)
            {
                textBoxHotKey2.Text = "";
                return;
            }
            string text="";
            if(e.Shift)
            {
                text = "Shift";
                /*
                if (e.Modifiers.CompareTo(Keys.ShiftKey) == 0)
                {
                    text+="Shift";
                }
                else if(e.Modifiers.CompareTo(Keys.RControlKey) == 0)
                {
                    text+="Right Shift";
                }
                */
            }
            if(e.Control)
            {
                if (text != "")
                    text += "+Ctrl";
                else
                    text = "Ctrl";

            }
            if (e.Alt)
            {
                if (text != "")
                    text += "+Alt";
                else
                    text = "Alt"; 
            }            
            if (e.Modifiers.CompareTo(e.KeyCode) == 0)
            {
                textBoxHotKey2.Text = text;
            }
            else
            {
                String v;
                if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
                {
                    v = e.KeyCode.ToString();
                }
                else if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9)
                {
                    v = e.KeyCode.ToString();
                }
                else if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z)
                {
                    v = e.KeyCode.ToString();
                }
                else
                { 
                    v = e.KeyCode.ToString(); 
                }
                if(text!="")
                    textBoxHotKey2.Text = text+"+"+v;
                else
                    textBoxHotKey2.Text = v; 
            }

         
            
        }

        private void textBoxHotKey2_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

    }
}
