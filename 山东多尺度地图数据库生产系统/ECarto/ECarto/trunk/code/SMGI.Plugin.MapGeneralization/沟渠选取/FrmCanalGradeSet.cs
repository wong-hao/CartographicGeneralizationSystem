using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Plugin.MapGeneralization
{
    public partial class FrmCanalGradeSet : Form
    {
        public FrmCanalGradeSet()
        {
            InitializeComponent();
        }

        private void FrmCanalGradeSet_Load(object sender, EventArgs e)
        {
            double area = 337500;
            Dictionary<int, string> scalesDic = new Dictionary<int, string>();
            scalesDic[1] = "1:5万";
            scalesDic[2] = "1:10万";
            scalesDic[3] = "1:25万";
            scalesDic[4] = "1:50万";
            scalesDic[5] = "1:100万";

            Dictionary<int, int> powDic = new Dictionary<int, int>();
            powDic[1] = 1;
            powDic[2] = 2;
            powDic[3] = 5;
            powDic[4] = 10;
            powDic[5] = 20;
            for (int i = 1; i <=5; i++)
            {
                object[] objs = new object[3];
                objs[0] = 6-i;
                objs[1] = scalesDic[i];
                objs[2] = area * powDic[i] * powDic[i];
                int rowIndex = dgSelectRule.Rows.Add();
                 dgSelectRule.Rows[rowIndex].SetValues(objs);
                
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

       
    }
}
