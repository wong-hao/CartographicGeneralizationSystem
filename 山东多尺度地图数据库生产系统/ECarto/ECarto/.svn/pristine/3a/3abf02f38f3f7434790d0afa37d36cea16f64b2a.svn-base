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
    public partial class FrmIPSet : Form
    {
        public string IP;
        public string User;
        public string Password;
        public FrmIPSet(string ip,string user,string pass)
        {
            InitializeComponent();
            txtIP.Text = ip;
            txtPass.Text = pass;
            txtUser.Text = user;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (txtIP.Text == string.Empty || txtPass.Text == string.Empty || txtUser.Text == string.Empty)
            {
                MessageBox.Show("文本框不能为空！");
                return;
            }
            IP = txtIP.Text.Trim();
            User = txtUser.Text.Trim();
            Password = txtPass.Text.Trim();
            DialogResult = DialogResult.OK;
        }

        private void DataBaseForm_Load(object sender, EventArgs e)
        {

        }
    }
}
