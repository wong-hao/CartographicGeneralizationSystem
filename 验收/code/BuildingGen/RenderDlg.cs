using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BuildingGen
{
    public partial class RenderDlg : Form
    {
        public bool isGroup_Render
        {
            get { return isGroup_Render2; }

        }
        bool isGroup_Render2 = false;

        public RenderDlg()
        {
            InitializeComponent();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.Checked == true)
            {
                isGroup_Render2 = true;
            }
            else
            {
                isGroup_Render2 = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
