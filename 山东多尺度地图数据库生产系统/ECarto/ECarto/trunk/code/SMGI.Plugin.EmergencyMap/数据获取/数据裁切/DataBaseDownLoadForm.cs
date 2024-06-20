using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using System.IO;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class DataDownLoadForm : Form
    {
        #region 属性
        public string IPAdress
        {
            get
            {
                return tbIPAdress.Text;
            }
        }
        public string UserName
        {
            get
            {
                return tbUserName.Text;
            }
        }
        public string Password
        {
            get
            {
                return tbPassword.Text;
            }
        }
        public string DataBase
        {
            get
            {
                return tbDataBase.Text;
            }
        }

        /// <summary>
        /// 输出的要素类
        /// </summary>
        public List<string> FeatureClassNameList
        {
            get
            {
                if (null == _fcName2Checked)
                    return null;

                List<string> result = new List<string>();
                foreach (var item in _fcName2Checked)
                {
                    if (item.Value)
                        result.Add(item.Key);
                }
                return result;
            }
        }
        private Dictionary<string, bool> _fcName2Checked;//输出要素类列表

        /// <summary>
        /// 选择的专题数据列表
        /// </summary>
        public List<ThematicDataInfo> SelectedThematicList
        {
            protected set;
            get;
        }

        public bool NeedClipDEM
        {
            get
            {
                return cbClipDEM.Checked;
            }
        }
        public bool NeedClipAttach
        {
            get
            {
                return cbClipExc.Checked;
            }
        }
        public bool NeedCutLine
        {
            get
            {
                return cbCutLine.Checked;
            }
        }
        public string OutputGDB
        {
            get
            {
                return tboutputGDB.Text;
            }
        }

        #endregion

        private GApplication _app;
        private List<ThematicDataInfo> _thematicList;
        public DataDownLoadForm(GApplication app, string ipAddress, string userName, string passWord, string databaseName, double scale)
        {
            InitializeComponent();

            _app = app;
            _fcName2Checked = null;//初始化选择列表
            _thematicList = null;

            tbIPAdress.Text = ipAddress;
            tbUserName.Text = userName;
            tbPassword.Text = passWord;
            tbDataBase.Text = databaseName;

            lbScale.Text += "1:" + scale;
        }

        private void DataDownLoadForm_Load(object sender, EventArgs e)
        {
            cbClipExc.Checked = CommonMethods.clipEx;
            this.cbCutLine.Checked = true;

            //裁切附区与否
            if (CommonMethods.clipEx)
            {
                cbClipExc.Checked = true;
            }
            else
            {
                cbClipExc.Visible = false;
                labelOption.Visible = false;
            }
        }


        private void cbSomeFeatureExport_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSomeFeatureExport.Checked)
            {
                if (null == _fcName2Checked)
                {
                    using (var wo = _app.SetBusy())
                    {
                        wo.SetText("正在连接数据库...");
                        List<string> fcNames = DataServerClass.getFeatureClassNames(_app, tbIPAdress.Text, tbUserName.Text, tbPassword.Text, tbDataBase.Text);

                        //默认全部选择
                        _fcName2Checked = new Dictionary<string, bool>();
                        foreach (var name in fcNames)
                        {
                            _fcName2Checked.Add(name, true);
                        }
                    }
                }

                //更新控件
                updateFCSelectTable(_fcName2Checked);
            }
            else
            {
                dgSelectRule.Rows.Clear();
            }

            
        }

        private void dgSelectRule_ColumnHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dgSelectRule.CurrentCell = null;
            int row = dgSelectRule.RowCount;
            int count = 0;
            for (int i = 0; i < row; i++)
            {
                if (((DataGridViewCheckBoxCell)dgSelectRule.Rows[i].Cells["Selected"]).Value.ToString().ToLower() == "true")
                {
                    count++;
                    continue;
                }
                ((DataGridViewCheckBoxCell)dgSelectRule.Rows[i].Cells["Selected"]).Value = true;
            }
            if (count == row)
            {
                for (int i = 0; i < row; i++)
                {
                    ((DataGridViewCheckBoxCell)dgSelectRule.Rows[i].Cells["Selected"]).Value = false;
                }
            }
        }

        private void cbThematic_CheckedChanged(object sender, EventArgs e)
        {
            //获取专题数据库信
            if (_thematicList == null && cbThematic.Checked)
            {
                using (var wo = GApplication.Application.SetBusy())
                {
                    _thematicList = ThematicDataClass.GetThemticElement(GApplication.Application, wo); 

                    //初始化
                    foreach (var info in _thematicList)
                    {
                        cmbThematicType.Items.Add(info);
                    }
                }
            }           

            cmbThematicType.Enabled = cbThematic.Checked;
            dgvThematic.Enabled = cbThematic.Checked;
            btnSelAllThematicFC.Enabled = cbThematic.Checked;
            btnUnSelAllThematicFC.Enabled = cbThematic.Checked;

            if (!cbThematic.Checked)
            {
                cmbThematicType.SelectedIndex = -1;
            }
        }

        private void cmbThematicType_SelectedIndexChanged(object sender, EventArgs e)
        {
            dgvThematic.Rows.Clear();

            ThematicDataInfo thematicInfo = cmbThematicType.SelectedItem as ThematicDataInfo;
            if (thematicInfo == null)
                return;

            List<string> fcList = null;
            #region 从图层对照规则表里获取所有需要下载的图层名列表
            string rulepath = GApplication.Application.Template.Root + "\\专题\\" + thematicInfo.Name + "\\规则对照.mdb";
            if (File.Exists(rulepath))
            {
                DataTable dtLayerRule = CommonMethods.ReadToDataTable(rulepath, "图层对照规则");
                if (dtLayerRule.Rows.Count > 0)
                {
                    fcList = new List<string>();

                    DataTable dtLayers = dtLayerRule.AsDataView().ToTable(true, new string[] { "图层" });//distinct
                    for (int i = 0; i < dtLayers.Rows.Count; ++i)
                    {
                        //图层名
                        string LayerName = dtLayers.Rows[i]["图层"].ToString().Trim().ToLower();
                        fcList.Add(LayerName);
                    }
                }
            }
            #endregion

            var dic = thematicInfo.Lyrs;
            var typedic = thematicInfo.LyrsType;
            foreach (var item in dic)
            {
                if (fcList != null && !fcList.Contains(item.Key.ToLower()))
                    continue;

                int rowIndex = dgvThematic.Rows.Add();
                dgvThematic.Rows[rowIndex].Cells["thematicFCName"].Value = item.Key;
                ((DataGridViewCheckBoxCell)dgvThematic.Rows[rowIndex].Cells["SelectedState"]).Value = item.Value;
            }
        }

        private void btnSelAllThematicFC_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvThematic.Rows)
            {
                row.Cells["SelectedState"].Value = true;
            }
        }

        private void btnUnSelAllThematicFC_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvThematic.Rows)
            {
                row.Cells["SelectedState"].Value = false;
            }
        }   

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog pDialog = new SaveFileDialog();
            pDialog.AddExtension = true;
            pDialog.DefaultExt = "gdb";
            pDialog.Filter = "文件地理数据库|*.gdb";
            pDialog.FilterIndex = 0;
            if (pDialog.ShowDialog() == DialogResult.OK)
            {
                tboutputGDB.Text = pDialog.FileName;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(tbIPAdress.Text))
            {
                MessageBox.Show("请指定服务器地址！");
                return;
            }
            if (string.IsNullOrEmpty(tbDataBase.Text))
            {
                MessageBox.Show("请指定服务器数据库！");
                return;
            }
            if (string.IsNullOrEmpty(tbUserName.Text))
            {
                MessageBox.Show("请指定服务器数据库用户名！");
                return;
            }
            if (string.IsNullOrEmpty(tbPassword.Text))
            {
                MessageBox.Show("请指定服务器数据库用户密码！");
                return;
            }

            if (cbThematic.Checked && cmbThematicType.SelectedItem == null)
            {
                MessageBox.Show("请指定需下载的专题类型！");
                return;
            }

            if (string.IsNullOrEmpty(tboutputGDB.Text))
            {
                MessageBox.Show("请指定输出位置！");
                return;
            }

            //处理专题数据
            SelectedThematicList = new List<ThematicDataInfo>();
            if (cbThematic.Checked && cmbThematicType.SelectedItem != null)
            {
                ThematicDataInfo thematicInfo = cmbThematicType.SelectedItem as ThematicDataInfo;
                Dictionary <string,bool> layer2SelState = thematicInfo.Lyrs;
                for (int i = 0; i < layer2SelState.Count; i++)
                {
                    layer2SelState[layer2SelState.ElementAt(i).Key] = false;
                }
                for (int i = 0; i < dgvThematic.Rows.Count; i++)
                {
                    var fcName = dgvThematic.Rows[i].Cells["thematicFCName"].Value.ToString();
                    var bSelected = (bool)dgvThematic.Rows[i].Cells["SelectedState"].Value;

                    if (bSelected)
                        layer2SelState[fcName] = true;
                }
                thematicInfo.Lyrs = layer2SelState;

                SelectedThematicList.Add(thematicInfo);
            }
            
            //处理基础数据
            if (cbSomeFeatureExport.Checked)
            {
                if (_fcName2Checked != null && dgSelectRule.Rows.Count > 0)
                {
                    for (int i = 0; i < _fcName2Checked.Count; i++)
                    {
                        bool flag = (from DataGridViewRow row in dgSelectRule.Rows where Convert.ToString(row.Cells["fcName"].Value) == _fcName2Checked.ElementAt(i).Key select Convert.ToBoolean(row.Cells["Selected"].Value)).First();
                        _fcName2Checked[_fcName2Checked.ElementAt(i).Key] = flag;
                    }
                }

                if (FeatureClassNameList.Count == 0)
                {
                    MessageBox.Show("请指定至少一个底图要素类！");
                    return;
                }
            }
            else
            {
                _fcName2Checked = null;
            }


            DialogResult = DialogResult.OK;
        }

        private void updateFCSelectTable(Dictionary<string, bool> fcName2Checked)
        {
            dgSelectRule.Rows.Clear();

            foreach (var item in fcName2Checked)
            {
                object[] datas = new object[2];
                datas[0] = item.Key;
                datas[1] = item.Value;

                dgSelectRule.Rows.Insert(dgSelectRule.Rows.Count, datas);
            }
        }

             

    }
}
