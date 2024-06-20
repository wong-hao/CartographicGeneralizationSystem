using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NetDesign.Com
{
    public partial class FrmClassDetail : Form
    {
        TextBox txtEdit = new TextBox();
        /// <summary>
        /// KeyDown事件定义
        /// </summary>
        private void txtEdit_KeyDown(object sender, KeyEventArgs e)
        {
          
            //Enter键 更新项并隐藏编辑框
            if (e.KeyCode == Keys.Enter)
            {
                double r = 0;
                double.TryParse(txtEdit.Text, out r);
                if (r == 0)
                    return;
                listBox1.Items[listBox1.SelectedIndex] = txtEdit.Text;
                breaksDic[listBox1.SelectedIndex] = txtEdit.Text;
                txtEdit.Visible = false;
              
            }
            //Esc键 直接隐藏编辑框
            if (e.KeyCode == Keys.Escape)
                txtEdit.Visible = false;
        }
        //areaName-val
        Dictionary<string, double> regionGDB = new Dictionary<string, double>();
        //areaName-classval
        public   Dictionary<string, string> regionBreaks = new Dictionary<string, string>();
        public decimal NumClass
        {
            get
            {
                return (this.numBreak.Value);
            }
        }
        Dictionary<int, string> breaksDic = new Dictionary<int, string>();
        bool inti = true;
        public FrmClassDetail(Dictionary<string, double> regionGDB_, Dictionary<string, string> regionBreaks_)
        {
            InitializeComponent();
            regionGDB = regionGDB_;
            regionBreaks = regionBreaks_;
            txtEdit.KeyDown += new KeyEventHandler(txtEdit_KeyDown);
            txtEdit.Leave += (o, e) => {
               
                double r = 0;
                double.TryParse(txtEdit.Text, out r);
                if (r == 0)
                    return;

                listBox1.Items[listBox1.SelectedIndex] = txtEdit.Text;
                breaksDic[listBox1.SelectedIndex] = txtEdit.Text;
                var dic = breaksDic.OrderBy(t => double.Parse(t.Value)).ToDictionary(p => p.Key, p => p.Value);
                int index = 0;
                breaksDic.Clear();
                foreach (var kv in dic)
                {
                    breaksDic[index] = kv.Value;
                    listBox1.Items[index] = kv.Value; 
                    index++;
                }
                txtEdit.Visible = false;
               
            };
        }

        private void FrmClassDetail_Load(object sender, EventArgs e)
        {
            listBox1.DrawMode = DrawMode.OwnerDrawFixed;
            listBox1.ItemHeight = 18;

            int index = 0;
            foreach (var kv in regionBreaks)
            {
               string[] vals = kv.Value.Split(new char[] { '—' }, StringSplitOptions.RemoveEmptyEntries);
              
               foreach (var str in vals)
               {
                   int num = breaksDic.Where(t => t.Value == str).Count();
                   if (num == 0 && str != "0")
                   {
                       breaksDic[index] = str;
                       index++;
                   }
               }
            }
            var dic = breaksDic.OrderBy(t => double.Parse(t.Value)).ToDictionary(p => p.Key, p => p.Value);
            index = 0;
            breaksDic.Clear();
            foreach (var kv in dic)
            {
                breaksDic[index] = kv.Value;
                listBox1.Items.Add(kv.Value);
                index++;
            }
            this.numBreak.Value = breaksDic.Count + 1;
            //计算
            lbNum.Text = regionGDB.Count.ToString();
            lbMax.Text = regionGDB.Max(t => t.Value).ToString();
            lbMin.Text = (regionGDB.Min(t => t.Value)).ToString();
            lbSum.Text = regionGDB.Sum(t => t.Value).ToString();
            lbAver.Text = (regionGDB.Average(t=>t.Value)).ToString();
            lbMiddle.Text = (regionGDB.OrderBy(t => t.Value).ToDictionary(p => p.Key, p => p.Value).Values.ToArray()[regionGDB.Count/2]).ToString();
            double sumOfSquare = 0; //平方总和
            foreach (var item in regionGDB.Values.ToArray())
            {
                double numberValue = item;
                sumOfSquare += Math.Pow((numberValue - double.Parse(lbAver.Text)), 2);
            }
            double stdDeviation = Math.Sqrt(sumOfSquare / (regionGDB.Count - 1));
            stdDeviation = Math.Round(stdDeviation, 2);
            lbSa.Text = stdDeviation.ToString();

        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (!breaksDic.ContainsKey(e.Index))
                return;
            e.DrawBackground();
            e.DrawFocusRectangle();
            e.Graphics.DrawString(breaksDic[e.Index], e.Font, new SolidBrush(e.ForeColor), e.Bounds);
        }
        private void listBox1_MouseClick(object sender, MouseEventArgs e)
        {
            txtEdit.Visible = false;
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            int itemSelected = listBox1.SelectedIndex;
            string itemText = listBox1.Items[itemSelected].ToString();

            Rectangle rect = listBox1.GetItemRectangle(itemSelected);
            txtEdit.Parent = listBox1;
            txtEdit.Text = string.Empty;
            txtEdit.Bounds = rect;
            txtEdit.Multiline = false;
            txtEdit.Visible = true;
            txtEdit.Text = itemText;
            txtEdit.Focus();
            txtEdit.SelectAll();
        }

        private void numBreak_ValueChanged(object sender, EventArgs e)
        {
            if (inti)
            {
                inti = false;
                return;
            }
            regionBreaks = BreakClass.Application.BreakValues(regionGDB, (int)numBreak.Value);
            int index = 0;
            breaksDic.Clear();
            foreach (var kv in regionBreaks)
            {
                string[] vals = kv.Value.Split(new char[] { '—' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var str in vals)
                {
                    int num = breaksDic.Where(t => t.Value == str).Count();
                    if (num == 0 && str != "0")
                    {
                        breaksDic[index] = str;
                        index++;
                    }
                }
            }
            listBox1.Items.Clear();
            var dic = breaksDic.OrderBy(t => double.Parse(t.Value)).ToDictionary(p => p.Key, p => p.Value);
            index = 0;
            breaksDic.Clear();
            foreach (var kv in dic)
            {
                breaksDic[index] = kv.Value;
                listBox1.Items.Add(kv.Value);
                index++;
            }
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            List<double> breaks=new List<double>();
            foreach(var kv in  breaksDic)
            {
               breaks.Add(double.Parse(kv.Value));
            }
            regionBreaks= BreakClass.Application.RegionBreakValues(regionGDB, breaks);
            DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
           
        }
    }
}
