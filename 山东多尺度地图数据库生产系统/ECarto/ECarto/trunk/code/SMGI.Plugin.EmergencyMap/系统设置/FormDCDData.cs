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
    public partial class MainData : Form
    {
        GApplication app;
        public MainData(GApplication _app)
        {
            app = _app;
            InitializeComponent();
        }

        private void MainData_Load(object sender, EventArgs e)
        {
            this.btAddDCD.Tag = this.lvDataBase;
            this.btDeleteDCD.Tag = this.lvDataBase;
            this.btEditDCD.Tag = this.lvDataBase;
            this.btServerDCD.Tag = this.gbDCD;
            LoadDataBase();
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
            if (currentListView.Name=="lvDataBase")
            {
                form = new DataBaseForm(currentListView, FormType.Add);
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
            if (currentListView.Name =="lvDataBase")
            {
                form = new DataBaseForm(currentListView, FormType.Edit);
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
            string ip, userName, password, dbName, layer;
            IWorkspace sdeWorkspace = null;
            //获取相应服务器、图层信息进行连接验证
            if (gb.Name=="gbDCD")
            {
                ip = tbMainIP.Text;
                userName = tbMainUserUame.Text;
                password = tbMainPassword.Text;
                foreach (ListViewItem item in lvDataBase.Items)
                {
                    dbName = item.SubItems[4].Text;
                    sdeWorkspace = app.GetWorkspacWithSDEConnection(ip, userName, password, dbName);
                    if (sdeWorkspace == null)
                    {
                        MessageBox.Show(string.Format("无法访问多尺度服务器！请确认ip[{0}]、用户名[{1}]、密码[{2}]、数据库[{3}]", ip, userName, password, dbName));
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
            //多尺度服务
            ChangeDataBase();
            #endregion
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// 加载多尺度数据信息
        /// </summary>
        private void LoadDataBase()
        {
            string fileName = app.Template.Root + @"\EnvironmentSettings.xml";
            if (File.Exists(fileName))
            {
                #region
                try
                {

                    XDocument doc = XDocument.Load(fileName);
                   
                    {
                       
                        var server = doc.Root.Element("Content").Element("Server");
                        tbMainIP.Text = server.Element("IPAddress").Value;
                        tbMainUserUame.Text = server.Element("UserName").Value;
                        tbMainPassword.Text = server.Element("Password").Value;
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }
                #endregion
            }

            lvDataBase.Columns.Add(new ColumnHeader { Text = "序号", Name = "id" });
            lvDataBase.Columns.Add(new ColumnHeader { Text = "基本比例尺", Name = "Scale" });
            lvDataBase.Columns.Add(new ColumnHeader { Text = "最大比例尺", Name = "Max" });
            lvDataBase.Columns.Add(new ColumnHeader { Text = "最小比例尺", Name = "Min" });
            lvDataBase.Columns.Add(new ColumnHeader { Text = "数据库", Name = "DatabaseName" });
            lvDataBase.Columns.Add(new ColumnHeader { Text = "模板", Name = "MapTemplate" });
            lvDataBase.Columns[0].Width = 50;
            lvDataBase.Columns[1].Width = 100;
            lvDataBase.Columns[2].Width = 100;
            lvDataBase.Columns[3].Width = 100;
            lvDataBase.Columns[4].Width = 80;
            lvDataBase.Columns[5].Width = 70;

            fileName = app.Template.Root + @"\专家库\Expertise.xml";
            if (File.Exists(fileName))
            {
                try
                {
                   
                    XDocument doc = XDocument.Load(fileName);
                    
                    {
                       
                        XElement mapService = doc.Root.Element("Content").Element("MapScaleRule");
                        int ct = 1;
                        foreach (var item in mapService.Elements("Item"))
                        {

                            string Scale = item.Element("Scale").Value;
                            string Max = item.Element("Max").Value;
                            string Min = item.Element("Min").Value;
                            string DatabaseName = item.Element("DatabaseName").Value;
                            string MapTemplate = item.Element("MapTemplate").Value;
                            ListViewItem lvi = new ListViewItem(ct.ToString());
                            ct++;
                            lvi.SubItems.AddRange(new string[] { Scale, Max, Min, DatabaseName, MapTemplate });
                            lvDataBase.Items.Add(lvi);
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
        /// 更改多尺度数据信息
        /// </summary>
        private void ChangeDataBase()
        {
            string fileName = app.Template.Root + @"\EnvironmentSettings.xml";
            if (File.Exists(fileName))
            {
                try
                {
                   
                    XDocument doc = XDocument.Load(fileName);
                  
                    {
                        
                        var server = doc.Root.Element("Content").Element("Server");
                        server.Element("IPAddress").Value = tbMainIP.Text.Trim();
                        server.Element("UserName").Value = tbMainUserUame.Text.Trim();
                        server.Element("Password").Value = tbMainPassword.Text.Trim();
                    }
                    doc.Save(fileName);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            fileName = app.Template.Root + @"\专家库\Expertise.xml";
            if (File.Exists(fileName))
            {
                try
                {
                   
                    XDocument doc = XDocument.Load(fileName);
                    
                    {
                       
                        XElement mapScaleRule = doc.Root.Element("Content").Element("MapScaleRule");
                        mapScaleRule.RemoveNodes();
                        foreach (ListViewItem LVItem in lvDataBase.Items)
                        {
                            string scale = LVItem.SubItems[1].Text;
                            string maxScale = LVItem.SubItems[2].Text;
                            string minScale = LVItem.SubItems[3].Text;
                            string databaseName = LVItem.SubItems[4].Text;
                            string template = LVItem.SubItems[5].Text;
                            XElement item = new XElement("Item");
                            XElement scaleNode = new XElement("Scale", scale);
                            item.Add(scaleNode);
                            XElement maxScaleNode = new XElement("Max", maxScale);
                            item.Add(maxScaleNode);
                            XElement minScaleNode = new XElement("Min", minScale);
                            item.Add(minScaleNode);
                            XElement databaseNameNode = new XElement("DatabaseName", databaseName);
                            item.Add(databaseNameNode);
                            XElement templateNode = new XElement("MapTemplate", template);
                            item.Add(templateNode);
                            mapScaleRule.Add(item);
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
    public enum FormType
    {
        Add = 0,
        Edit = 1
    }
}
