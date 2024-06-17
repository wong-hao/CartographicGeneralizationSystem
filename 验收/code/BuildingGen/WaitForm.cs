using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BuildingGen
{
    public partial class WaitForm : Form
    {
        public WaitForm(GApplication app)
        {
            InitializeComponent();
            app.abort += new Action<GApplication>(app_abort);
            app.waitText += new Action<string>(app_waitText);
            //app.maxValue += new Action<int>(app_maxValue);
            app.step += new Action<int>(app_step);
            this.ShowInTaskbar = true;
        }
        
        void app_step(int obj)
        {
            Action<int> d = (v) =>
            {
                if (v > 0)
                {
                    if (v != this.progressBar1.Maximum)
                    {
                        this.progressBar1.Maximum = v;
                        this.progressBar1.Minimum = 0;
                        this.progressBar1.Value = 0;
                        this.progressBar1.Step = 1;
                    }
                    this.progressBar1.PerformStep();
                }
                else
                {
                    this.progressBar1.Value = 0;
                }
            };
            if (this.progressBar1.InvokeRequired)
            {
                this.Invoke(d, new object[] { obj });
            }
            else
            {
                d(obj);
            }
        }

        void app_maxValue(int obj)
        {
            Action<int> d = (v) => { this.progressBar1.Maximum = v; };
            if (this.progressBar1.InvokeRequired)
            {
                this.Invoke(d, new object[] { obj });
            }
            else
            {
                d(obj);
            }
        }

        void app_waitText(string text)
        {
            if (this.label1.InvokeRequired)
            {
                Action<string> d = new Action<string>(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.label1.Text = text;
            }

        }
        void SetText(string text)
        {
            this.label1.Text = text;
        }

        void app_abort(GApplication app)
        {

            app.abort -= new Action<GApplication>(app_abort);
            app.waitText -= new Action<string>(app_waitText);
            //app.maxValue -= new Action<int>(app_maxValue);
            app.step -= new Action<int>(app_step);
            if (this.InvokeRequired) {
                Action<bool> d = (v) => { this.Hide(); };
                this.Invoke(d, new object[] { true });
            }
            else {
                this.Hide();
            }
            this.DialogResult = DialogResult.OK;
        }
    }

}
