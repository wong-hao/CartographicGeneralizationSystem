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
using System.Xml.Linq;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class DataBaseStructUpgradeForm : Form
    {
        #region 属性
        /// <summary>
        /// 比例尺
        /// </summary>
        public int Mapscale
        {
            get
            {
                return int.Parse(tbMapScale.Text.Trim());
            }
        }

        /// <summary>
        /// 底图模板风格：一般，水利，影像
        /// </summary>
        public string BaseMapStyle
        {
            get
            {
                return cbBaseMapTemplate.Text.Trim();
            }
        }

        /// <summary>
        /// 地图开本：双全开，全开，对开
        /// </summary>
        public string MapFolio
        {
            get
            {
                return cmbMapSize.Text.Trim();
            }
        }

        /// <summary>
        /// 源数据库
        /// </summary>
        public string SourceGDBFile
        {
            get
            {
                return txtTarget.Text.Trim();
            }
        }

        /// <summary>
        /// 输出数据库
        /// </summary>
        public string OutputGDBFile
        {
            get
            {
                return txtExport.Text.Trim();
            }
        }

        /// <summary>
        /// 是否需要区分主邻区
        /// </summary>
        public bool NeedAttachMap
        {
            get
            {
                return cbAttach.Checked;
            }
        }
        #endregion

        private GApplication _app;
        Dictionary<string, string> _environmentDic;
        public DataBaseStructUpgradeForm(GApplication app)
        {
            InitializeComponent();

            _app = app;
            _environmentDic = null;

            List<string> names = getBaseMapTemplateNames();
            cbBaseMapTemplate.Items.AddRange(names.ToArray());
        }

        public DataBaseStructUpgradeForm(GApplication app, string SourceGDBFile)
        {
            InitializeComponent();

            _app = app;
            _environmentDic = null;

            List<string> names = getBaseMapTemplateNames();
            cbBaseMapTemplate.Items.AddRange(names.ToArray());

            btnTarget.Visible = false;
            LoadSourceGDB(SourceGDBFile);

            string gdbName = System.IO.Path.GetFileNameWithoutExtension(txtTarget.Text) + "_Ecarto.gdb";
            string savegdb = System.IO.Path.GetDirectoryName(txtTarget.Text) + "\\" + gdbName;
            txtExport.Text = savegdb;
        }

        private void DataBaseStructUpgradeForm_Load(object sender, EventArgs e)
        {
            var paramContent = EnvironmentSettings.getContentElement(_app);
            var mapScale = paramContent.Element("MapScale");//比例尺
            var baseMapEle = paramContent.Element("BaseMap");//模板风格
            tbMapScale.Text = mapScale.Value;  
            if (cbBaseMapTemplate.Items.Contains(baseMapEle.Value))
            {
                cbBaseMapTemplate.SelectedIndex = cbBaseMapTemplate.Items.IndexOf(baseMapEle.Value);
            }
            else
            {
                cbBaseMapTemplate.SelectedIndex = -1;
            }


            //判断专家库是否存在MapSize.xml
            cmbMapSize.Items.Add("空");
            var path = GApplication.Application.Template.Root + @"\专家库\尺寸模板\MapSize.xml";
            if (File.Exists(path))
            {
                XDocument doc = XDocument.Load(path);
                var items = doc.Root.Elements("Item");
                foreach (var item in items)
                {
                    cmbMapSize.Items.Add(item.Value);
                }
            }
            cmbMapSize.SelectedIndex = 0;
        }

        private void btnTarget_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "选择GDB工程文件夹";
            fbd.ShowNewFolderButton = false;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!GApplication.GDBFactory.IsWorkspace(fbd.SelectedPath))
                {
                    MessageBox.Show("不是有效地GDB文件");
                    return;
                }

                LoadSourceGDB(fbd.SelectedPath);

                string gdbName = System.IO.Path.GetFileNameWithoutExtension(txtTarget.Text) + "_Ecarto.gdb";
                string savegdb = System.IO.Path.GetDirectoryName(txtTarget.Text) + "\\" + gdbName;

                txtExport.Text = savegdb;
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog pDialog = new SaveFileDialog();
            pDialog.AddExtension = true;
            pDialog.DefaultExt = "gdb";
            pDialog.Filter = "文件地理数据库|*.gdb";
            pDialog.FilterIndex = 0;
            if (pDialog.ShowDialog() == DialogResult.OK)
            {
                txtExport.Text = pDialog.FileName;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            #region 参数合法性判断
            int scale = 0;
            int.TryParse(tbMapScale.Text.Trim(), out scale);
            if (scale < 1)
            {
                MessageBox.Show("请指定一个合理的比例尺！");
                return;
            }

            if (cbBaseMapTemplate.Text.Trim() == "")
            {
                MessageBox.Show("请指定一个底图模板！");
                return;
            }

            if (txtTarget.Text.Trim() == "" || !Directory.Exists(txtTarget.Text.Trim()))
            {
                MessageBox.Show("请指定一个有效的源数据库文件！");
                return;
            }

            if (txtExport.Text.Trim() == "")
            {
                MessageBox.Show("请指定输出数据库！");
                return;
            }

            if (Directory.Exists(txtExport.Text.Trim()))
            {
                MessageBox.Show("导出数据文件已经存在！");
                return;
            }
            #endregion

            //更新配置表
            EnvironmentSettings.updateMapScale(_app, scale);
            EnvironmentSettings.updateMapSizeStyle(_app, MapFolio);
            EnvironmentSettings.updateBaseMap(_app, cbBaseMapTemplate.Text);
            if (_environmentDic != null)
            {
                if (_environmentDic.ContainsKey("ThemDataBase"))
                {
                    CommonMethods.ThemDataBase = _environmentDic["ThemDataBase"];

                }
                if (_environmentDic.ContainsKey("ThemExist"))
                {
                    CommonMethods.ThemData = bool.Parse(_environmentDic["ThemExist"]);

                }
            }
            
            DialogResult = DialogResult.OK;
        }

        private List<string> getBaseMapTemplateNames()
        {
            List<string> names = new List<string>();

            const string TemplatesFileName = "MapStyle.xml";
            string thematicPath = _app.Template.Root + @"/底图";
            DirectoryInfo dir = new DirectoryInfo(thematicPath);
            var dirs = dir.GetDirectories();

            foreach (var d in dirs)
            {
                var fs = d.GetFiles(TemplatesFileName);
                if (fs.Length != 1)
                {
                    continue;
                }
                var f = fs[0];
                try
                {
                    XDocument doc = XDocument.Load(f.FullName);
                    XElement xmlContent = doc.Element("Template").Element("Content");

                    names.Add(xmlContent.Element("Name").Value);
                }
                catch
                {
                    continue;
                }

            }
            return names;
        }

        private void LoadSourceGDB(string gdbpath)
        {
            txtTarget.Text = gdbpath;
            _environmentDic = null;

            using (var wo = _app.SetBusy())
            {
                wo.SetText("正在获取源数据库的配置信息......");

                //更新系统的环境配置文件（有什么更好的方法没？？？）
                IWorkspaceFactory pWorkspaceFactory = new FileGDBWorkspaceFactoryClass();
                IWorkspace ws = pWorkspaceFactory.OpenFromFile(gdbpath, 0);
                try
                {
                    var config = Config.Open(ws as IFeatureWorkspace);
                    _environmentDic = config["EMEnvironment"] as Dictionary<string, string>;
                    if (_environmentDic == null)
                    {
                        _environmentDic = EnvironmentSettings.GetConfigVal(config, "EMEnvironmentXML");
                    }
                    
                }
                catch
                {
                }

                //更新相关控件的值
                if (_environmentDic != null)
                {
                    if(_environmentDic.ContainsKey("MapScale") && tbMapScale.Text != _environmentDic["MapScale"])
                    {
                        tbMapScale.Text = _environmentDic["MapScale"];

                        updateTemplateAndRule();
                    }

                    if (_environmentDic.ContainsKey("BaseMap") && cbBaseMapTemplate.Items.Contains(_environmentDic["BaseMap"]))
                    {
                        cbBaseMapTemplate.SelectedIndex = cbBaseMapTemplate.Items.IndexOf(_environmentDic["BaseMap"]);
                    }
                    else
                    {
                        cbBaseMapTemplate.SelectedIndex = -1;
                    }

                    
                    if (_environmentDic.ContainsKey("MapSizeStyle") && cmbMapSize.Items.Contains(_environmentDic["MapSizeStyle"]))
                    {
                        cmbMapSize.SelectedIndex = cmbMapSize.Items.IndexOf(_environmentDic["MapSizeStyle"]);
                    }
                    else
                    {
                        cmbMapSize.SelectedIndex = cmbMapSize.Items.Count > 0 ? 0 : -1;
                    }

                    if (_environmentDic.ContainsKey("AttachMap"))
                    {
                        cbAttach.Checked = bool.Parse(_environmentDic["AttachMap"]);
                        if (!cbAttach.Checked)
                            cbAttach.Enabled = false;
                    } 

                    //更新配置文件
                    EnvironmentSettings.updateElementbyKV(GApplication.Application, _environmentDic);
                }

            }
        }

        private void tbMapScale_Leave(object sender, EventArgs e)
        {
            updateTemplateAndRule();
        }

        private void tbMapScale_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                updateTemplateAndRule();
            }
        }

        private void cbBaseMapTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateTemplateAndRule();
        }

        private void cmbMapSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateTemplateAndRule();
        }


        private void updateTemplateAndRule()
        {
            try
            {
                tbMxdFile.Text = "";
                tbLayerRuleFile.Text = "";

                if (cbBaseMapTemplate.Text.Trim() == "")
                    return;

                int mapscale = 1;
                int.TryParse(tbMapScale.Text.Trim(), out mapscale);
                if (mapscale < 1)
                    return;


                //更新配置表
                EnvironmentSettings.updateMapScale(_app, mapscale);
                EnvironmentSettings.updateMapSizeStyle(_app, MapFolio);
                EnvironmentSettings.updateBaseMap(_app, cbBaseMapTemplate.Text);
                if (_environmentDic != null)
                {
                    if (_environmentDic.ContainsKey("ThemDataBase"))
                    {
                        CommonMethods.ThemDataBase = _environmentDic["ThemDataBase"];

                    }
                    if (_environmentDic.ContainsKey("ThemExist"))
                    {
                        CommonMethods.ThemData = bool.Parse(_environmentDic["ThemExist"]);

                    }
                }

                tbMxdFile.Text = EnvironmentSettings.getMxdFullFileName(_app);
                tbLayerRuleFile.Text = EnvironmentSettings.getLayerRuleDBFileName(_app);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
            }
        }
    }

}
