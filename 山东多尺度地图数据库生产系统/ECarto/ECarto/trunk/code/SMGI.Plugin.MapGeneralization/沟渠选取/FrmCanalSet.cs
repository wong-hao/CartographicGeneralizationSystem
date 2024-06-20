using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Plugin.MapGeneralization
{
    public partial class FrmCanalSet : Form
    {
        public FrmCanalSet()
        {
            InitializeComponent();
        }

        private void btHelp_Click(object sender, EventArgs e)
        {
            int height = this.Height;
            this.Height =height== 536?229:536;  
        }

        private void FrmCanalSet_Load(object sender, EventArgs e)
        {
            txtEndLength.Text = PlanarGraph.DANGLELENGTH.ToString();
            txtAngle.Text = PlanarGraph.STROKEANGLE.ToString();
            txtArea.Text = PlanarGraph.SELECTAREA.ToString();
            txtLenMax.Text = PlanarGraph.DANGLENMAX.ToString();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private void btOK_Click(object sender, EventArgs e)
        {
            double r = 0;
            double.TryParse(txtEndLength.Text, out r);
            if (r != 0)
            {
                PlanarGraph.DANGLELENGTH = r;
            }
            r = 0;
            double.TryParse(txtAngle.Text, out r);
            if (r != 0)
            {
                PlanarGraph.STROKEANGLE = r;
            }
            r = 0;
            double.TryParse(txtArea.Text, out r);
            if (r != 0)
            {
                PlanarGraph.SELECTAREA = r;
            }
            r = 0;
            double.TryParse(txtLenMax.Text, out r);
            if (r != 0)
            {
                PlanarGraph.DANGLENMAX = r;
            }
            DialogResult = DialogResult.OK;
        }
    }
}
