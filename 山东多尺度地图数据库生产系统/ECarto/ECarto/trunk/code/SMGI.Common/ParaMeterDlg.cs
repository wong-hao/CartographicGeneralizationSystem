using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
namespace SMGI.Common
{
    public partial class ParaMeterDlg : Form
    {
        Parameter para;
        //Dictionary<string, byte[]> datas;
        OpenFileDialog ofd;
        SaveFileDialog sfd;
        internal ParaMeterDlg(Parameter para)
        {
            InitializeComponent();
            this.para = para;
            SetData();
            ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Title = "打开参数表";
            ofd.Filter = "综合参数表(*.gpt)|*.gpt";
            sfd = new SaveFileDialog();
            sfd.Title = "保存参数表";
            sfd.Filter = "综合参数表(*.gpt)|*.gpt";
            sfd.OverwritePrompt = true;
            //datas = new Dictionary<string, byte[]>();
        }
        public void SetData()
        {
            this.propertyGrid1.SelectedObject = new DictionaryPropertyGridAdapter(para.data); 
        }

        private void GenParaDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing
                || e.CloseReason == CloseReason.None)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void Load_Click(object sender, EventArgs e)
        {
            try
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(ofd.FileName, FileMode.Open);
                    long length = fs.Length;
                    byte[] data = new byte[length];
                    fs.Read(data, 0, (int)length);
                    para.Load(data);
                    fs.Close();
                    SetData();
                }
            }
            catch
            {
                MessageBox.Show("无效的参数文件");
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            try
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                    //int length = fs.Length;
                    byte[] data = para.Save();
                    fs.Write(data, 0, data.Length);
                    fs.Flush();
                    fs.Close();
                }
            }
            catch
            {
                MessageBox.Show("保存出错");
            }
        }
    }
}
