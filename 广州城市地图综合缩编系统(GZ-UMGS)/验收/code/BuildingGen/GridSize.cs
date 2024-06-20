using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BuildingGen {
    public partial class GridSize : Form {
        public GridSize() {
            InitializeComponent();
        }
        double x, y;
        public double GridX {
            get {
                return x;
            }
            set {
                x = value;
                tbX.Text = value.ToString();
            }
        }
        public double GridY {
            get {
                return y;
            }
            set {
                y = value;
                tbY.Text = value.ToString();
            }
        }
        public bool IsShowGrid {
            get {
                return checkBox1.Checked;
            }
            set {
                checkBox1.Checked = value;
            }
        }
        private void button1_Click(object sender, EventArgs e) {
            try {
                x = Convert.ToDouble(tbX.Text);
                y = Convert.ToDouble(tbY.Text);
                if (x <= 0 || y<= 0)
                    throw new Exception();
                DialogResult = DialogResult.OK;
            }
            catch {
                MessageBox.Show("需要一个大于0的数字o(╯□╰)o", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
    }
}
