using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Xml.Linq;

namespace SMGI.Plugin.GeneralEdit
{
    public partial class EditOptionForm : Form
    {
        public int StickyMoveTolerance { get; set; }

        private GApplication _app;
        private IEngineEditProperties2 _editorProp;

        public EditOptionForm()
        {
            InitializeComponent();

            _app = GApplication.Application;
        }

        private void EditOptionForm_Load(object sender, EventArgs e)
        {
            _editorProp = _app.EngineEditor as IEngineEditProperties2;

            //更新控件
            tbStickyMoveTolerance.Text = _editorProp.StickyMoveTolerance.ToString();
            if (_editorProp.StickyMoveTolerance == 0)
            {
                #region 读取配置表
                string cfgFileName = GApplication.Application.Template.Root + "\\EnvironmentConfig.xml";
                if (System.IO.File.Exists(cfgFileName))
                {
                    try
                    {
                        XDocument xmlDoc = XDocument.Load(cfgFileName);
                        var EditOptionItem = xmlDoc.Element("Option").Element("EditOption");
                        int val = int.Parse(EditOptionItem.Element("StickeyMoveTolerance").Value);
                        if (val > 0)
                        {
                            tbStickyMoveTolerance.Text = val.ToString();
                        }

                    }
                    catch (Exception ex)
                    {
                    }
                }
                #endregion
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            int tol = 0;
            bool b = int.TryParse(tbStickyMoveTolerance.Text, out tol);
            if (!b || tol < 0)
            {
                MessageBox.Show("粘滞容差输入不合法!");
                return;
            }

            //更新容差
            _editorProp.StickyMoveTolerance = tol;
            
            #region 将配置写入到配置文件中
            string cfgFileName = GApplication.Application.Template.Root + "\\EnvironmentConfig.xml";
            if (System.IO.File.Exists(cfgFileName))
            {
                try
                {
                    XDocument xmlDoc = XDocument.Load(cfgFileName);
                    var EditOptionItem = xmlDoc.Element("Option").Element("EditOption");

                    var TolItem = EditOptionItem.Element("StickeyMoveTolerance");
                    TolItem.SetValue(tol);

                    xmlDoc.Save(cfgFileName);
                }
                catch
                {
                }
            }
            #endregion

            DialogResult = DialogResult.OK;
        }
    }
}
