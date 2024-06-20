using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GISClient;
using ESRI.ArcGIS.esriSystem;
using SMGI.Common;

namespace SMGI.Plugin.EmergencyMap.DataSource
{
    public partial class ThematicData : Form
    {
        GApplication app;
        public ThematicData(GApplication _app)
        {
            app = _app;
            InitializeComponent();
        }

        private void ThematicData_Load(object sender, EventArgs e)
        {
            this.btAddThematic.Tag = this.lvThematic;
            this.btDeleteThematic.Tag = this.lvThematic;
            this.btEditThematic.Tag = this.lvThematic;
            this.btServerThematic.Tag = this.gbTheMatic;
            LoadThematic();
        }

        private void groupBoxLeave(object sender, EventArgs s)
        {
            GroupBox currentGB = sender as GroupBox;
            foreach (Control c in currentGB.Controls)
            {
                if (c is ListView)
                {
                    ListView lv = c as ListView;
                    lv.SelectedItems.Clear();
                }
            }

        }

        private void btnAdd(object sender, EventArgs e)
        {
            SimpleButton currentBtn = sender as SimpleButton;
            ListView currentListView = currentBtn.Tag as ListView;
            Form form = null;
            if (currentListView.Name=="lvThematic")
            {
                form = new ThematicForm(currentListView, FormType.Add);
            }
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();
        }

        private void btnEdit(object sender, EventArgs e)
        {
            SimpleButton currentBtn = sender as SimpleButton;
            ListView currentListView = currentBtn.Tag as ListView;
            if (currentListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择一行进行编辑！");
                return;
            }
            Form form = null;
            if (currentListView.Name=="lvThematic")
            {
                form = new ThematicForm(currentListView, FormType.Edit);
            }
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();
        }

        private void btnDelete(object sender, EventArgs e)
        {
            SimpleButton currentBtn = sender as SimpleButton;
            ListView currentListView = currentBtn.Tag as ListView;
            foreach (ListViewItem item in currentListView.SelectedItems)
            {
                currentListView.Items.Remove(item);
            }
        }

        private void btnValidServer(object sender, EventArgs e)
        {
            SimpleButton currentBtn = sender as SimpleButton;
            GroupBox gb = currentBtn.Tag as GroupBox;
            string ip, userName, password, dbName;
            IWorkspace sdeWorkspace = null;
            //获取相应服务器、图层信息进行连接验证
            if(gb.Name=="gbTheMatic")
            {
                foreach (ListViewItem item in lvThematic.Items)
                {
                    string name = item.SubItems[1].Text;
                    ip = item.SubItems[2].Text;
                    userName = item.SubItems[3].Text;
                    password = item.SubItems[4].Text;
                    dbName = item.SubItems[5].Text;
                    sdeWorkspace = app.GetWorkspacWithSDEConnection(ip, userName, password, dbName);
                    if (sdeWorkspace == null)
                    {
                        MessageBox.Show(string.Format("无法访问专题服务器！请确认ip[{0}]、用户名[{1}]、密码[{2}]、数据库[{3}]", ip, userName, password, dbName));
                        return;
                    }
                }
            }
            MessageBox.Show("验证通过！");
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            #region 验证输入合法性
            foreach (Control gb in this.Controls)
            {
                if (gb is GroupBox)
                {
                    GroupBox currentGB = gb as GroupBox;
                    foreach (Control c in currentGB.Controls)
                    {
                        if (c is TextBox)
                        {
                            TextBox currentTB = c as TextBox;
                            if (string.IsNullOrEmpty(currentTB.Text))
                            {
                                MessageBox.Show(string.Format("{0}的信息不能有空值！", currentGB.Text));
                                return;
                            }
                        }
                        else if (c is ListView)
                        {
                            ListView lv = c as ListView;
                            if (lv.Items.Count == 0)
                            {
                                MessageBox.Show(string.Format("{0}的列表内容不能为空！", currentGB.Text));
                                return;
                            }
                        }
                    }
                }
            }
            #endregion
            #region 更改xml参数配置
            //专题服务
            ChangeThematic();
            #endregion
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// 加载专题数据信息
        /// </summary>
        private void LoadThematic()
        {


            lvThematic.Columns.Add(new ColumnHeader { Text = "序号", Name = "id" });
            lvThematic.Columns.Add(new ColumnHeader { Text = "专题名称", Name = "thematicName" });
            lvThematic.Columns.Add(new ColumnHeader { Text = "IP", Name = "ip" });
            lvThematic.Columns.Add(new ColumnHeader { Text = "用户名", Name = "userName" });
            lvThematic.Columns.Add(new ColumnHeader { Text = "密码", Name = "password" });
            lvThematic.Columns.Add(new ColumnHeader { Text = "数据库", Name = "database" });

            lvThematic.Columns[0].Width = 50;
            lvThematic.Columns[1].Width = 100;
            lvThematic.Columns[2].Width = 100;
            lvThematic.Columns[3].Width = 100;
            lvThematic.Columns[4].Width = 80;
            lvThematic.Columns[5].Width = 70;
            string fileName = app.Template.Root + @"\专家库\ThematicMapRule.xml";
            if (File.Exists(fileName))
            {
                try
                {
                    
                    XDocument doc = XDocument.Load(fileName);
                  
                    {
                        
                        XElement mapService = doc.Root.Element("Content");
                        int ct = 1;
                        foreach (var item in mapService.Elements("ThematicData"))
                        {
                            string name = item.Attribute("name").Value;
                            string IPAddress = item.Element("IPAddress").Value;
                            string UserName = item.Element("UserName").Value;
                            string Password = item.Element("Password").Value;
                            string DataBase = item.Element("DataBase").Value;


                            ListViewItem lvi = new ListViewItem(ct.ToString());
                            ct++;
                            lvi.SubItems.AddRange(new string[] { name, IPAddress, UserName, Password, DataBase });
                            lvThematic.Items.Add(lvi);
                        }

                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 更改专题数据信息
        /// </summary>
        private void ChangeThematic()
        {
            string fileName = app.Template.Root + @"\专家库\ThematicMapRule.xml";
            if (File.Exists(fileName))
            {
                try
                {

                    XDocument doc = XDocument.Load(fileName);
                   
                    {
                       
                        var content = doc.Root.Element("Content");
                        content.RemoveNodes();
                        foreach (ListViewItem LVItem in lvThematic.Items)
                        {
                            string name = LVItem.SubItems[1].Text;
                            string IPAddress = LVItem.SubItems[2].Text;
                            string UserName = LVItem.SubItems[3].Text;
                            string Password = LVItem.SubItems[4].Text;
                            string DataBase = LVItem.SubItems[5].Text;
                            XElement ThematicData = new XElement("ThematicData");
                            ThematicData.SetAttributeValue("name", name);
                            XElement IPAddressNode = new XElement("IPAddress", IPAddress);
                            ThematicData.Add(IPAddressNode);
                            XElement UserNameNode = new XElement("UserName", UserName);
                            ThematicData.Add(UserNameNode);
                            XElement PasswordNode = new XElement("Password", Password);
                            ThematicData.Add(PasswordNode);
                            XElement DataBaseNode = new XElement("DataBase", DataBase);
                            ThematicData.Add(DataBaseNode);
                            content.Add(ThematicData);
                        }
                    }
                    doc.Save(fileName);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
