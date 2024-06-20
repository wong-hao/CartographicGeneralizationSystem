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
    public partial class FeatureParallelCopyForm : Form
    {
        
        public FeatureParallelCopyForm()
        {
            InitializeComponent();
        }
        public int OffsetDis
        {
            set;
            get;
        }
       
        private void btStartCopy_Click(object sender, EventArgs e)
        {
            
            int tempx = 0;
            int.TryParse(tBX.Text, out tempx);
            OffsetDis = tempx;
           
            if (OffsetDis == 0 )
            {
                System.Windows.Forms.MessageBox.Show("请输入移动距离！");
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
