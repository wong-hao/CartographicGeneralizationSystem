using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmRiverAutoGradual : Form
    {
       
        public double DeleteRate=0.1;
        public bool RiverRate = true;
        public FrmRiverAutoGradual()
        {
            InitializeComponent();
        }

        public double StartWidth
        {
            get;
            set;
        }

        public double EndWidth
        {
            get;
            set;
        }
        private void btOk_Click(object sender, EventArgs e)
        {
            bool res;
            double _StartWidth,_EndWidth;
            res = double.TryParse(txbStartWidth.Text, out _StartWidth);
            if (res)
            {
                StartWidth = _StartWidth;
            }
            else
            {
              
                DialogResult = DialogResult.Cancel;
                MessageBox.Show("请不要输入非法参数！", "提示");
            }
            res = double.TryParse(txbEndWidth.Text, out _EndWidth);
            if (res)
            {
                EndWidth = _EndWidth;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
                MessageBox.Show("请不要输入非法参数！", "提示");
            }
            DialogResult = DialogResult.OK;
            
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void FrmRiverAutoGradual_Load(object sender, EventArgs e)
        {
            this.txbEndWidth.Text = EndWidth.ToString();
            this.txbStartWidth.Text = StartWidth.ToString();
        }

     
    }
}
