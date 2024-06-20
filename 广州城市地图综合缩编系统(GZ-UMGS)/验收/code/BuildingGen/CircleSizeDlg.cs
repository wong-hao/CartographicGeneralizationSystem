using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BuildingGen {
    public partial class CircleSizeDlg : Form {
        public CircleSizeDlg() {
            InitializeComponent();
        }
        double r;
        public double R {
            get { return r; }
            set {
                r = value;
                tbX.Text = r.ToString();
            }
        }
        public bool IsShowCircle {
            get {
                return checkBox1.Checked;
            }
            set {
                checkBox1.Checked = value;
            }
        }
        private void button1_Click(object sender, EventArgs e) {
            try {
                r = Convert.ToDouble(tbX.Text);
                if (r <= 0 )
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
