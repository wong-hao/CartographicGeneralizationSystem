using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Common
{
    public partial class SymboloryNameForm : Form
    {
        public string name;
        public string caot;
        IList<string> pList;
        public SymboloryNameForm(IList<string> pListCato)
        {
            InitializeComponent();
            pList = pListCato;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("请输入符号名字");
            }
            else
            {
                this.name = textBox1.Text;
                this.caot = comboBox1.Text; 
            }
            
        }

        private void SymboloryNameForm_Load(object sender, EventArgs e)
        {
            if (pList != null)
            {

                foreach (string s in pList)
                {
                    comboBox1.Items.Add(s);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
