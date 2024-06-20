using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Plugin.CollaborativeWork
{
    public partial class frmInputField : Form
    {
        public frmInputField()
        {
            InitializeComponent();
        }

        private void frmInputField_Load(object sender, EventArgs e)
        {
            tbInfo.Text="";
            tbInfo.Focus();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (tbInfo.Text.Trim()=="")
            {
                MessageBox.Show("字段名不能为空");
                return;
            }
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
