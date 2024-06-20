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
    public partial class InfoForm : Form
    {
        public int Time { get; set; }
        public string Message
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }
        public string Caption { get; set; }
        public string FullCaption
        {
            get {
                return string.Format("{1}【{0}s后关闭】", (int)Math.Round(Time / 1000.0), Caption);
            }
        }
        public InfoForm()            
        {
            InitializeComponent();
            
        }

        private void InfoForm_Load(object sender, EventArgs e)
        {
            this.Text = FullCaption;
            Timer t = new Timer();
            t.Interval = 100;
            DateTime last = DateTime.Now;
            t.Tick += (s, ee) =>
            {
                var a = DateTime.Now - last;
                Time -= (int)a.TotalMilliseconds;
                last = DateTime.Now;
                this.Text = FullCaption;
                if(Time < 0)
                    this.Close();
            };

            this.FormClosing += (s, ee) =>
            {
                t.Stop();
                t.Dispose();
            };
            t.Start();            
        }

    }
}
