using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Plugin.GeneralEdit
{
    public partial class VegaGbUpdateForm : Form
    {
        public string VegaName { get; set; }

        public string VegpName { get; set; }

        public VegaGbUpdateForm()
        {
            InitializeComponent();
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txt_vega.Text) || string.IsNullOrEmpty(txt_vegp.Text)) return;
            VegaName = txt_vega.Text.Trim();
            VegpName = txt_vegp.Text.Trim();
            DialogResult = DialogResult.OK;
        }
    }
}
