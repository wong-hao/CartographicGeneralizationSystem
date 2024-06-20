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
    public partial class MarkerPictureSymbolDailog : Form
    {
        public string picFilePath = "";
        public double symbolSize = 10;
        public MarkerPictureSymbolDailog()
        {
            InitializeComponent();
        }

        private void FormMarPicSymbol_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "图片";
            openFileDialog1.Filter = "JPG文件(*.jpg）|*.jpg|BMP文件（*.bmp）|*.bmp|PNG文件（*.png)|*.png|" +
                "EMF文件(*.emf)|*.emf";
               
            openFileDialog1.ShowDialog();
            picFilePath = openFileDialog1.FileName;
            this.textBox1.Text = picFilePath;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (!double.TryParse(this.textBox2.Text, out symbolSize))
            {
                MessageBox.Show("非法~");
                this.textBox2.Text = symbolSize.ToString();
                return;
            }
            
            

        }

    }
}
