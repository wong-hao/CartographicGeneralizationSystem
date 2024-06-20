using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Plugin.EmergencyMap.DataSource
{
    public partial class ThematicForm : Form
    {
        ListView listView;
        FormType type;
        public ThematicForm(ListView listView, FormType type)
        {
            InitializeComponent();
            this.listView = listView;
            this.type = type;
            if (type == FormType.Edit || type == FormType.Add)
            {
                if(listView.SelectedItems.Count==0)
                    return;
                ListViewItem LVItem = listView.SelectedItems[0];
                tbThematicName.Text = LVItem.SubItems[1].Text.ToString();
                tbIP.Text = LVItem.SubItems[2].Text.ToString();
                tbUserName.Text = LVItem.SubItems[3].Text.ToString();
                tbPassword.Text = LVItem.SubItems[4].Text.ToString();
                tbDatabase.Text = LVItem.SubItems[5].Text.ToString();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string thematicName = tbThematicName.Text.Trim();
            string ip = tbIP.Text.Trim();
            string userName = tbUserName.Text.Trim();
            string password = tbPassword.Text.Trim();
            string database = tbDatabase.Text.Trim();
            if (string.IsNullOrEmpty(thematicName) || string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(database))
            {
                MessageBox.Show("专题服务信息填写不全！");
                return;
            }
            if (type == FormType.Add)
            {
                ListViewItem LVItem = new ListViewItem((listView.Items.Count + 1).ToString());
                LVItem.SubItems.AddRange(new string[] { thematicName, ip, userName, password, database });
                listView.Items.Add(LVItem);
                LVItem.EnsureVisible();
            }
            else if (type == FormType.Edit)
            {
                ListViewItem LVItem = listView.SelectedItems[0];
                LVItem.SubItems[1].Text = thematicName;
                LVItem.SubItems[2].Text = ip;
                LVItem.SubItems[3].Text = userName;
                LVItem.SubItems[4].Text = password;
                LVItem.SubItems[5].Text = database;
                LVItem.EnsureVisible();
            }
            DialogResult = DialogResult.OK;
        }
    }
}
