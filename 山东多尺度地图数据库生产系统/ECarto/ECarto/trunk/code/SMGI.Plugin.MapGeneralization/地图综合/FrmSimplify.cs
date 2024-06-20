﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace SMGI.Plugin.MapGeneralization
{
    public partial class FrmSimplify : DevExpress.XtraEditors.XtraForm
    {
        public double width;
        public double heigth;
        public FrmSimplify(double w,double h)
        {
            InitializeComponent();
            if (w != 0)
                txtBendDeepth.Text = h.ToString();
            if (h != 0)
                txtBendWidth.Text = w.ToString();
            
        }
        public FrmSimplify()
        {
            InitializeComponent();


        }
        private void FrmSimplify_Load(object sender, EventArgs e)
        {
            this.AcceptButton = btOK;
            this.CancelButton = btCancel;
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            double.TryParse(txtBendDeepth.Text, out heigth);
            double.TryParse(txtBendWidth.Text, out width);
            if (width == 0)
            {
                MessageBox.Show("最小开口宽度设置错误！");
                return;
            }
            if (heigth == 0)
            {
                MessageBox.Show("最小弯曲深度设置错误！");
                return;
            }
            this.DialogResult = DialogResult.OK;
            return;
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}