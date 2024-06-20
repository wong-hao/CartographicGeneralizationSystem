using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Plugin.GeneralEdit
{
    public partial class GeneralizeForm : Form
    {
        public double maxAllowableOffset=0;
        public GeneralizeForm()
        {
            InitializeComponent();
            this.tbMaxAllowableOffset.KeyUp += (o, e) =>
            {
                if (double.TryParse((o as TextBox).Text, out maxAllowableOffset))
                    this.btnConfirm.Enabled = true;
                else
                    this.btnConfirm.Enabled = false;

            };
            this.btnConfirm.Click += (o, e) =>
            {
                this.DialogResult =System.Windows.Forms.DialogResult.OK;
            };
            this.btnCancel.Click += (o, e) =>
            {
                this.DialogResult =System.Windows.Forms.DialogResult.Cancel;
            };
        }

        //若已设置过容差，窗口显示该容差
        public GeneralizeForm(double maxOffset)
        {
            InitializeComponent();

            this.tbMaxAllowableOffset.Text = maxOffset.ToString();

            this.tbMaxAllowableOffset.KeyUp += (o, e) =>
            {
                if (double.TryParse((o as TextBox).Text, out maxAllowableOffset))
                    this.btnConfirm.Enabled = true;
                else
                    this.btnConfirm.Enabled = false;

            };
            this.btnConfirm.Click += (o, e) =>
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            };
            this.btnCancel.Click += (o, e) =>
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            };
        }
    }
}
