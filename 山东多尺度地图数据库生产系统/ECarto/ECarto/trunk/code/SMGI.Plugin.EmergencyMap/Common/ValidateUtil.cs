using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SMGI.Plugin.EmergencyMap
{

        /// <summary>
        /// 输入验证
        /// </summary>
    public  class ValidateUtil
    {
        //正则
        public static Regex RegNumber = new Regex("^[0-9]+$");                          //正整数
        public static Regex RegPositiveNumber = new Regex(@"^\d+(\.{0,1}\d+){0,1}$");   //0、正数


        private bool isGo = true;
        /// <summary>
        /// 遍历TextBox
        /// </summary>
        /// <param name="Controls">控件集</param>
        public bool TraversalTextBox(Control.ControlCollection Controls)
        {
            //判断控件类型
            foreach (Control ctl in Controls)
            {
                System.Diagnostics.Debug.WriteLine( ctl.GetType().ToString().Split('.').Last());
                var TestExpr = ctl.GetType().ToString().Split('.').Last();
                switch(TestExpr)
                {
                    case "TextBox":
                        string s = ((TextBox)ctl).Text;
                        if (!IsPositiveNumber(s))
                        {
                            MessageBox.Show("请输入正确的数值！");
                            isGo = false;
                            return isGo;
                        }
                        break;
                    case "GroupBox":
                    case "SplitContainer":
                    case "SplitterPanel":
                        TraversalTextBox(ctl.Controls);//递归
                        if (!isGo)
                        {
                            return isGo;
                        }
                        break;
                }
            }
            return isGo;
        }



        #region 数字字符串检查
        /// <summary>
        /// 数字字符串检查
        /// </summary>
        /// <param name="inputData">TextBox.text</param>
        /// <returns></returns>
        public static bool IsPositiveNumber(string inputData)
        {
            Match m = RegPositiveNumber.Match(inputData);
            return m.Success;
        }
        #endregion

    }
}
