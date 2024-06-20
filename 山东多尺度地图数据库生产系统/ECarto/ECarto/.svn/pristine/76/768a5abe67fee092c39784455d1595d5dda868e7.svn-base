using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmFigureMapSet : Form
    {
        private IEnvelope pLineEnvelope;

        string orientation;

        public string Orientation
        {
            get { return orientation; }
            set { orientation = value;

            foreach (Control c in gbPosition.Controls)
            {
                if (c.GetType() == typeof(System.Windows.Forms.Button))
                {
                    c.BackColor = Color.Transparent;
                    if (c.Name.Contains(orientation))
                    {
                        c.BackColor = Color.DimGray;
                        c.Focus();
                    }
                }
            } 
          
         }
        }
      
        public string MapLocation
        {
            get
            {
                
                return string.Empty;
            }
        }
        double figureMapSize;

        public double FigureMapSize
        {
            get { return figureMapSize; }
            set { 
                figureMapSize = value;
                txtsize.Text = figureMapSize.ToString();
            }
        }
        
        public FrmFigureMapSet(double size = 50)
        {
            InitializeComponent();

            orientation = "TopLeft";
            figureMapSize = size;

            txtsize.Text = size.ToString();
            figureMapSize = size;

        }
        public FrmFigureMapSet(IEnvelope penv, double size = 50)
        {
            InitializeComponent();

            pLineEnvelope = penv;

            orientation = "TopLeft";
            figureMapSize = size;

            txtsize.Text = size.ToString();
            figureMapSize = size;
            txtsize.Text = figureMapSize.ToString();
            foreach (Control c in gbPosition.Controls)
            {
                if (c.GetType() == typeof(System.Windows.Forms.Button))
                {
                    c.Click += new EventHandler(btn_Click);
                    c.BackColor = Color.Transparent;
                }
            }
            btnTopLeft.BackColor = Color.DimGray;
            btnTopLeft.Focus();

            txtsize.Focus();
            this.ActiveControl = txtsize;

        }

        private void FrmFigureMapSet_Load(object sender, EventArgs e)
        {
          
        }

        

        /// <summary>
        /// 此处已经设置完毕了位置的方向属性
        /// </summary>
        private void btn_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button btn = sender as System.Windows.Forms.Button;
            orientation = btn.Name.Substring(3);
            //清除按钮的背景色
            foreach (Control c in gbPosition.Controls)
            {
                if (c.GetType() == typeof(System.Windows.Forms.Button))
                {
                    c.BackColor = Color.Transparent;
                }
            }
            //设置按钮的背景色
            btn.BackColor = Color.DimGray;
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            if (orientation == "")
            {
                MessageBox.Show("请选择附图位置");
                return;
            }
            double re = 0;
            double.TryParse(txtsize.Text, out re);
            if (re == 0)
            {
                MessageBox.Show("请设置附图尺寸");
                return;
            }
            figureMapSize = re; 

            DialogResult = DialogResult.OK;
        }


       
        
    }
}
