using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
namespace SMGI.Common {
    public class InitForm : SMGI.Common.IInitInfo {
        internal class InternalForm : Form {
            Image img;
            string info;
            internal InternalForm(InitForm form,Image img) {
                if (img != null) {
                    this.img = img;
                }
                else {
                    this.img = new Bitmap(640, 480);
                    var g = Graphics.FromImage(this.img);
                    g.Clear(Color.FromArgb(0xc0, 0xc0, 0xc0));
                    g.Dispose();
                }
                this.ShowInTaskbar = false;
                this.FormBorderStyle = FormBorderStyle.None;
                this.ClientSize = this.img.Size;
                this.CenterToScreen();
                this.info = "正在启动……";
                this.Paint += new PaintEventHandler(InternalForm_Paint);
                form.hide = new Action<bool>(this.Abort);
                form.info = new Action<string>(this.Info);
            }
            internal void Abort(bool aaa)
            {
                Action<bool> d = (v) => { this.Close(); };
                if (this.InvokeRequired) {                    
                    this.Invoke(d, new object[] { true });
                }
                else {
                    d(aaa);
                }
            }
            internal void Info(string info) {
                Action<string> d = (strinfo) => { this.info = strinfo; this.Refresh(); };
                if (this.InvokeRequired) {
                    this.Invoke(d, new object[] { info });
                }
                else {
                    d(info);
                }
            }
            void InternalForm_Paint(object sender, PaintEventArgs e) {
                e.Graphics.DrawImage(this.img, this.ClientRectangle);
                e.Graphics.DrawRectangle(new Pen(Color.Black, 1.0f), new Rectangle(0,0,this.ClientSize.Width -1,this.ClientSize.Height - 1));
                Font f =  new Font("微软雅黑", 10);
                var size = e.Graphics.MeasureString(info, f);
                e.Graphics.DrawString(info, f, new SolidBrush(Color.Black), this.ClientSize.Width - size.Width - 20, this.ClientSize.Height - 34);
            }
        }
       
        Thread thread;
        public InitForm(Image img) {

            thread = new Thread(() => {
                InternalForm form = new InternalForm(this,img);
                form.ShowDialog();
            });
            thread.Start();
        }
        public void Hide()
        {
            if (hide != null)
                hide(true);
        }

        public void Info(string info) {
            if (this.info != null)
                this.info(info);
        }
        
        internal Action<bool> hide;
        internal Action<string> info;
    }
}
