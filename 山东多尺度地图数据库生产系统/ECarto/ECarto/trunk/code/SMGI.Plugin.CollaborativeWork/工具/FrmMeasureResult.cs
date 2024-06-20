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
    public partial class FrmMeasureResult : Form
    {
        public delegate void FrmClosedEventHandler();
        public event FrmClosedEventHandler frmClosed = null;

        public FrmMeasureResult()
        {
            InitializeComponent();
        }

        public string ResultText
        {
            set
            {
                this.lbResut.Text = value;
            }
        }

        //窗口关闭后引发委托事件
        private void FrmMeasureResultcs_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (frmClosed != null)
            {
                frmClosed();
            }
        }
    }
}
