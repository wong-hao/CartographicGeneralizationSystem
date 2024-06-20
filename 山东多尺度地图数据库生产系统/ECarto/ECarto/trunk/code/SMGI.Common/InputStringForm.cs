using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SMGI.Common
{
    public partial class InputStringForm : Form
    {
        public InputStringForm()
        {
            InitializeComponent();
            AcceptKey = null;
        }

        public string Value {
            get {
                return tbText.Text;
            }
            set {
                tbText.Text = value;
            }
        }

        public Func<char, bool> AcceptKey
        {
            get;
            set;
        }


        private void tbText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (AcceptKey != null) {
                e.Handled = !AcceptKey(e.KeyChar);
            }
        }
        
    }
}
