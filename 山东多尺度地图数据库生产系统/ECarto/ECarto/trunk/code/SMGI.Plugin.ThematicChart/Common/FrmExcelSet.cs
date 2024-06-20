using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.SystemUI;

namespace SMGI.Plugin.ThematicChart.Common
{
    public partial class FrmExcelSet : Form
    {
        public int SheetIndex= 1;
        public bool XToY = false;
        public FrmExcelSet(List<string>list)
        {
            InitializeComponent();
            cmbShs.Items.AddRange(list.ToArray());
            cmbShs.SelectedIndex = 0;
            //【行列互换】复选框显示问题,所有的饼图和堆积条形图不显示
            if (GApplication.Application.MapControl.CurrentTool != null)
            {
                ICommand cmd = GApplication.Application.MapControl.CurrentTool as ICommand;
                string classFullName = cmd.ToString();
                string cmdName = classFullName.Substring(classFullName.LastIndexOf('.') + 1);
                if (cmdName == "BarSingleCommand" || cmdName.Contains("Pie"))
                {
                    this.cbRowOrColumn.Visible = false;
                }
            }
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            SheetIndex = cmbShs.SelectedIndex+1;
            XToY = cbRowOrColumn.Checked;
            DialogResult = DialogResult.OK;
        }
    }
}
