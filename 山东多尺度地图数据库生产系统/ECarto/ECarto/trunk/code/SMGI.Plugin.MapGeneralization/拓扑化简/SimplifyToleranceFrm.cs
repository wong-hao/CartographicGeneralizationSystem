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
    public partial class SimplifyToleranceFrm : Form
    {
        public SimplifyToleranceFrm()
        {
            InitializeComponent();
        }

        private double tolerance;

        public double Tolerance
        {
            get { return tolerance; }
            set { tolerance = value; }
        }
      
        
        private void button1_Click(object sender, EventArgs e)
        {
            double width, depth;
            if (!double.TryParse(textBox1.Text, out width) || !double.TryParse(textBox2.Text, out depth))
            {
                MessageBox.Show("请输入有效开口值");
                //this.DialogResult = DialogResult.Cancel;
            }
            else
            {
                if (width > depth)
                {
                    tolerance = width;
                }
                else
                {
                    tolerance = depth;
                }
                this.DialogResult = DialogResult.OK;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
