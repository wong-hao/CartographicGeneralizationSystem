using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Plugin.GeneralEdit
{
    public partial class AutoConnectionPolylineForm : Form
    {
        public double connectdis { get; set; }
        public double closedis { get; set; }
        public AutoConnectionPolylineForm()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (condis.Text.ToString().Trim() == "")
            {
                MessageBox.Show("连接距离不能为空");
                return;
            }

            if (clodis.Text.ToString().Trim() == "")
            {
                MessageBox.Show("闭合距离不能为空");
                return;
            }
            connectdis = double.Parse(condis.Text.ToString().Trim());
            closedis = double.Parse(clodis.Text.ToString().Trim());
            this.Close();
        }
    }
}
