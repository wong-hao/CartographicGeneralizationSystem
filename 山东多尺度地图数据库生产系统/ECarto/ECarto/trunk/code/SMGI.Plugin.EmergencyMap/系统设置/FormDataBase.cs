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
    public partial class DataBaseForm : Form
    {
        ListView listView;
        FormType type;
        public DataBaseForm(ListView listView, FormType type)
        {
            InitializeComponent();
            this.listView = listView;
            this.type = type;
            if (type == FormType.Edit)
            { 
                ListViewItem LVItem = listView.SelectedItems[0];
                tbScale.Text = LVItem.SubItems[1].Text.ToString();
                tbMaxScale.Text=LVItem.SubItems[2].Text.ToString();
                tbMinScale.Text = LVItem.SubItems[3].Text.ToString();
                tbDataBase.Text = LVItem.SubItems[4].Text.ToString();
                tbTemplate.Text = LVItem.SubItems[5].Text.ToString();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            bool flag = false;
            double scale;
            flag = double.TryParse(tbScale.Text.Trim(), out scale);
            if (!flag)
            {
                MessageBox.Show("比例尺填写不规范！");
                return;
            }
            double maxScale;
            flag = double.TryParse(tbMaxScale.Text.Trim(), out maxScale);
            if (!flag)
            {
                MessageBox.Show("最大比例尺填写不规范！");
                return;
            }
            double minScale;
            flag = double.TryParse(tbMinScale.Text.Trim(), out minScale);
            if (!flag)
            {
                MessageBox.Show("最小比例尺填写不规范！");
                return;
            }
            string database = tbDataBase.Text.Trim();
            string template = tbTemplate.Text.Trim();
            if (string.IsNullOrEmpty(database) || string.IsNullOrEmpty(template))
            {
                MessageBox.Show("数据库或模板信息填写不全！");
                return;
            }
            if (type == FormType.Add)
            {
                ListViewItem LVItem = new ListViewItem((listView.Items.Count + 1).ToString());
                LVItem.SubItems.AddRange(new string[] { scale.ToString(), maxScale.ToString(), minScale.ToString(), database, template });
                listView.Items.Add(LVItem);
                LVItem.EnsureVisible();
            }
            else if (type == FormType.Edit)
            {
                ListViewItem LVItem = listView.SelectedItems[0];
                LVItem.SubItems[1].Text = scale.ToString();
                LVItem.SubItems[2].Text = maxScale.ToString();
                LVItem.SubItems[3].Text = minScale.ToString();
                LVItem.SubItems[4].Text = database;
                LVItem.SubItems[5].Text = template;
                LVItem.EnsureVisible();
            }
            DialogResult = DialogResult.OK;
        }
    }
}
