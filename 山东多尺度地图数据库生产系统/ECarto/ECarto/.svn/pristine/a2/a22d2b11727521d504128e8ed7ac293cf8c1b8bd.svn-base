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
    public partial class DataSymbolAndAnnotationUpdateForm : Form
    {
        private GApplication _app;

        /// <summary>
        /// 比例尺
        /// </summary>
        public string Mapscale
        {
            get
            {
                return tbMapScale.Text.Trim();
            }
        }

        /// <summary>
        /// 模板风格：一般，水利，影像等
        /// </summary>
        public string BaseMapEle
        {
            get
            {
                return cbBaseMapTemplate.Text.Trim();
            }
        }

        /// <summary>
        /// 地图开本：双全开，全开，对开等
        /// </summary>
        public string MapSize
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

        public bool AttachMap
        {
            set;
            get;
        }

        public Dictionary<string, string> EnvConfigString
        {
            set;
            get;
        }

        public DataSymbolAndAnnotationUpdateForm(GApplication app)
        {
            InitializeComponent();

            this._app = app;
            EnvConfigString = null;
        }

        private void DataSymbolAndAnnotationUpdateForm_Load(object sender, EventArgs e)
        {
            List<string> baseMapNames = getBaseMapTemplateNames();
            cbBaseMapTemplate.Items.AddRange(baseMapNames.ToArray());

            List<string> mapSizeTypes = getMapSizeTypes();
            cmbMapSize.Items.AddRange(mapSizeTypes.ToArray());
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

                txtTarget.Text = fbd.SelectedPath;
                LoadSourceGDB(txtTarget.Text);

                string gdbName = System.IO.Path.GetFileNameWithoutExtension(txtTarget.Text) + "_update.gdb";
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

        private void btOK_Click(object sender, EventArgs e)
        {
            if (txtExport.Text.Trim() == "" || txtTarget.Text.Trim() == "" || 
                tbMapScale.Text.Trim() == "" || cbBaseMapTemplate.Text.Trim() == "")
            {
                MessageBox.Show("输入不能为空！");
                return;
            }
            
            if (Directory.Exists(txtExport.Text.Trim()))
            {
                MessageBox.Show("导出数据文件已经存在！");
                return;
            }

            int mapscale = 1;
            int.TryParse(tbMapScale.Text.Trim(), out mapscale);
            if (mapscale < 1)
            {
                MessageBox.Show("输入的比例尺不合法!");
                return;
            }

            //更新配置表
            EnvironmentSettings.updateMapScale(_app, mapscale);
            EnvironmentSettings.updateBaseMap(_app, BaseMapEle);
            EnvironmentSettings.updateMapSizeStyle(_app, MapSize);

            if (EnvConfigString != null)
            {
                if (EnvConfigString.ContainsKey("ThemDataBase"))
                {
                    CommonMethods.ThemDataBase = EnvConfigString["ThemDataBase"];

                }
                if (EnvConfigString.ContainsKey("ThemExist"))
                {
                    CommonMethods.ThemData = bool.Parse(EnvConfigString["ThemExist"]);
                }
            }
           
            DialogResult = DialogResult.OK;
        }

        #region 私有方法
        /// <summary>
        /// 获取底图模板类型名称集合
        /// </summary>
        /// <returns></returns>
        private List<string> getBaseMapTemplateNames()
        {
            List<string> baseMapNames = new List<string>();

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
                    baseMapNames.Add(xmlContent.Element("Name").Value);
                }
                catch
                {
                    continue;
                }

            }
            return baseMapNames;
        }

        private List<string> getMapSizeTypes()
        {
            List<string> mapSizeTypes = new List<string>();
            mapSizeTypes.Add("空");

            var path = _app.Template.Root + @"\专家库\尺寸模板\MapSize.xml";
            if (File.Exists(path))
            {
                XDocument doc = XDocument.Load(path);
                var items = doc.Root.Elements("Item");
                foreach (var item in items)
                {
                    mapSizeTypes.Add(item.Value);
                }
            }

            return mapSizeTypes;
        }

        private void LoadSourceGDB(string gdbpath)
        {
            txtTarget.Text = gdbpath;

            using (var wo = _app.SetBusy())
            {
                wo.SetText("正在获取源数据库的配置信息......");

                //更新系统的环境配置文件
                Dictionary<string, string> envString = null;
                IWorkspaceFactory pWorkspaceFactory = new FileGDBWorkspaceFactoryClass();
                IWorkspace ws = pWorkspaceFactory.OpenFromFile(gdbpath, 0);
                try
                {
                    var config = Config.Open(ws as IFeatureWorkspace);
                    envString = config["EMEnvironment"] as Dictionary<string, string>;
                    if (envString == null)
                    {
                        envString = EnvironmentSettings.GetConfigVal(config, "EMEnvironmentXML");
                    }
                    if (envString != null)
                    {
                        EnvironmentSettings.updateElementbyKV(GApplication.Application, envString);
                        EnvConfigString = envString;
                    }
                   
                }
                catch
                {

                }
                
                //更新对话框控件值
                if (envString != null)
                {
                    if (envString.ContainsKey("MapScale") && tbMapScale.Text != envString["MapScale"])
                    {
                        tbMapScale.Text = envString["MapScale"];
                        updateTemplateAndRule();
                    }

                    if (envString.ContainsKey("BaseMap") && cbBaseMapTemplate.Items.Contains(envString["BaseMap"]))
                    {
                        cbBaseMapTemplate.SelectedIndex = cbBaseMapTemplate.Items.IndexOf(envString["BaseMap"]);
                    }
                    else
                    {
                        cbBaseMapTemplate.SelectedIndex = -1;
                    }

                    
                    if (envString.ContainsKey("MapSizeStyle") && cmbMapSize.Items.Contains(envString["MapSizeStyle"]))
                    {
                        cmbMapSize.SelectedIndex = cmbMapSize.Items.IndexOf(envString["MapSizeStyle"]);
                    }
                    else
                    {
                        cmbMapSize.SelectedIndex = cmbMapSize.Items.Count > 0 ? 0 : -1;
                    }

                    if (envString.ContainsKey("AttachMap"))
                    {
                        AttachMap = bool.Parse(envString["AttachMap"]);
                    }  
                }
                  
            }
        }
        #endregion

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
                tbAnnoRuleFile.Text = "";

                if (cbBaseMapTemplate.Text.Trim() == "")
                    return;

                int mapscale = 1;
                int.TryParse(tbMapScale.Text.Trim(), out mapscale);
                if (mapscale < 1)
                    return;

                //更新配置表
                EnvironmentSettings.updateMapScale(_app, mapscale);
                EnvironmentSettings.updateBaseMap(_app, BaseMapEle);
                EnvironmentSettings.updateMapSizeStyle(_app, MapSize);

                if (EnvConfigString != null)
                {
                    if (EnvConfigString.ContainsKey("ThemDataBase"))
                    {
                        CommonMethods.ThemDataBase = EnvConfigString["ThemDataBase"];

                    }
                    if (EnvConfigString.ContainsKey("ThemExist"))
                    {
                        CommonMethods.ThemData = bool.Parse(EnvConfigString["ThemExist"]);
                    }
                }

                tbMxdFile.Text = EnvironmentSettings.getMxdFullFileName(_app);
                tbLayerRuleFile.Text = EnvironmentSettings.getLayerRuleDBFileName(_app);
                tbAnnoRuleFile.Text = EnvironmentSettings.getAnnoRuleDBFileName(_app);
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
