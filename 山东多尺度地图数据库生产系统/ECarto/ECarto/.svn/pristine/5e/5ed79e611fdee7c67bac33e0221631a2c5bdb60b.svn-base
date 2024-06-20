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
    public partial class FeatureParallelCopyToolForm : Form
    {

        public FeatureParallelCopyToolForm()
        {
            InitializeComponent();
        }
        public int OffsetDis
        {
            set;
            get;
        }
        public string Tolayer
        {
            set;
            get;
        }

        private void btStartCopy_Click(object sender, EventArgs e)
        {

            int tempx = 0;
            int.TryParse(tBX.Text, out tempx);
            OffsetDis = tempx;
            Tolayer = tolayer.Text.Trim();
            if (OffsetDis == 0 || Tolayer==null)
            {
                System.Windows.Forms.MessageBox.Show("请输入参数！");
                return;
            }
            DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
