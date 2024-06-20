using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SMGI.Common;
using System.Xml.Linq;
using SMGI.Plugin.EmergencyMap.DataSource;
using DevExpress.XtraWizard;
using ESRI.ArcGIS.Output;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using System.IO;
using System.Reflection;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Framework;
using SMGI.Plugin.EmergencyMap.OneKey;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmAutoMap : Form
    {
        GApplication m_Application = null;
        
        #region 数据获取参数
        public string UpgradeGDB
        {
            get
            {
                return txtUpgradeGDB.Text;
            }
        }
        public string SourceGDB
        {
            get
            {
                return tboutputGDB.Text;
            }
        }
        public bool AttachMap
        {
            get
            {
                return cbAttach.Checked;
            }
        }
        public string BaseMapStyle
        {
            get
            {
                return cbBaseMapTemplate.SelectedItem.ToString();
            }
        }
        public List<ThematicDataInfo> ThematicList = new List<ThematicDataInfo>();
        #endregion
        #region 地图整饰
     
        public Dictionary<string, ICmykColor> SDMColors = new Dictionary<string, ICmykColor>();
        public double SDMDis = 2.5;
        public string MapName
        {
            get { return tbMapName.Text; }
        }
        public string MapProductName
        {
            get { return tbProductFactory.Text; }
        }
        public MapNameInfo MapNameInfos = null;
        #endregion
        #region 地图输出
        public string m_FileName = string.Empty;
        
        #endregion
        public FrmAutoMap(GApplication _Application)
        {
            InitializeComponent();
            m_Application = _Application;
          
            
            
            FrmSDMSet sdm = new FrmSDMSet();
            this.SDMColors = sdm.SDMColors;
            this.SDMDis = sdm.SDMDis;
            FrmNameSet mapNameSt = new FrmNameSet();
            MapNameInfos = mapNameSt.MapNameInfos;
        }

        public void UpdatePrams()
        {
            CreateBOUAElement();
            CreateBOULSkipElement();
            CreateAnnoElement();
            CreateFootBorderElement();
            CreateScaleBarEle();
            CreateCompassEle();
            CreateSDMElement();
            CreateFigureMapEle();
        }

        private void wizardAutoMap_FinishClick(object sender, CancelEventArgs e)
        {
            if (m_FileName == "")
            {
                MessageBox.Show("请选择输出文件");
                e.Cancel = true;
                return;
            }
            GetThematicInfo();
            UpdatePrams();
            EnvironmentSettings.updateBaseMap(m_Application, BaseMapStyle);
            DialogResult = DialogResult.OK;
            
        }

        private void wizardAutoMap_NextClick(object sender, DevExpress.XtraWizard.WizardCommandButtonClickEventArgs e)
        {
            if (wizardAutoMap.SelectedPage.Name == "PageDataDownload")
            {
            }
           
        }

        private void wizardAutoMap_SelectedPageChanged(object sender, DevExpress.XtraWizard.WizardPageChangedEventArgs e)
        {

        }

        private void FrmAutoMap_Load(object sender, EventArgs e)
        {
            List<string> names = getBaseMapTemplateNames();
            cbAttach.Checked = CommonMethods.clipEx;
            cbBaseMapTemplate.Items.AddRange(names.ToArray());
            cbBaseMapTemplate.SelectedIndex = 0;
            timer.Enabled = true;
        }
        private List<string> getBaseMapTemplateNames()
        {
            List<string> names = new List<string>();

            string TemplatesFileName = "MapStyle.xml";
            string thematicPath = m_Application.Template.Root + @"/底图";
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
                    XElement xmlContent = FromFileInfo(f);
                    names.Add(xmlContent.Element("Name").Value);
                }
                catch
                {
                    continue;
                }

            }

            return names;
        }
        private XElement FromFileInfo(FileInfo f)
        {
           
            {
                XDocument doc = XDocument.Load(f.FullName);
                return doc.Element("Template").Element("Content");
            }
        }

        private void btDataSet_Click(object sender, EventArgs e)
        {
          
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
                string gdbName = System.IO.Path.GetFileNameWithoutExtension(tboutputGDB.Text) + "_Ecarto.gdb";
                string savegdb = System.IO.Path.GetDirectoryName(tboutputGDB.Text) + "\\" + gdbName;
                this.txtUpgradeGDB.Text = savegdb;

            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (tboutputGDB.Text != ""&&txtUpgradeGDB.Text!="")
            {
                PageDataDownload.AllowNext = true;
            }
            else
            {
                PageDataDownload.AllowNext = false;
            }

        }

        private void btnOutputFile_Click(object sender, EventArgs e)
        {
            SaveFileDialog pSaveFileDialog = new SaveFileDialog();
            pSaveFileDialog.Title = "输出文件";
            pSaveFileDialog.Filter = "PDF(*.pdf)|*.pdf|JPEG(*.jpg)|*.jpg|TIFF(*.tif)|*.tif|AI(*.ai)|*.ai";
            pSaveFileDialog.FilterIndex = 0;
            if (!string.IsNullOrEmpty(txtFileName.Text))
            {
                pSaveFileDialog.FileName = txtFileName.Text;
            }
            if (pSaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtFileName.Text = pSaveFileDialog.FileName;
                m_FileName = pSaveFileDialog.FileName;
            }
        }

        private void wizardAutoMap_CancelClick(object sender, CancelEventArgs e)
        {
            this.Close();
        }

        

         

        private void btSave1_Click(object sender, EventArgs e)
        {
            SaveFileDialog pDialog = new SaveFileDialog();
            pDialog.AddExtension = true;
            pDialog.DefaultExt = "gdb";
            pDialog.Filter = "文件地理数据库|*.gdb";
            pDialog.FilterIndex = 0;
            if (pDialog.ShowDialog() == DialogResult.OK)
            {
                txtUpgradeGDB.Text = pDialog.FileName;
            }
        }

       

     

        private void button2_Click(object sender, EventArgs e)
        {
            FrmNameSet mapName = new FrmNameSet();
            if (mapName.ShowDialog() == DialogResult.OK)
            {
                MapNameInfos= mapName.MapNameInfos;
            }
        }
        #region 境界普色相关
       
        public XElement BouaColorEle = null;
        //邻区颜色
        private Dictionary<string, IRgbColor> attachColors = new Dictionary<string, IRgbColor>();
        //主区颜色
        private List<ICmykColor> mainColors = new List<ICmykColor>();
        string mainBOUAFCName = string.Empty;
        string pacProv = string.Empty;
        private void btZoneColorCmd_Click(object sender, EventArgs e)
        {
            var frm = new ZoneColorForm(AttachMap);
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;
            mainColors = frm.CMYKColors;
            mainBOUAFCName = frm.FCName;
            pacProv = frm.ProvPAC;
            attachColors = frm.AttachColors;
        }
        private void CreateBOUAElement()
        {

            if (mainColors.Count==0)
            {
                var frm = new ZoneColorForm();
                mainColors = frm.CMYKColors;
                mainBOUAFCName = frm.FCName;
                pacProv = frm.ProvPAC;
                attachColors = frm.AttachColors;
                frm.Dispose();
            }
            BouaColorEle = new XElement("SMGI.Plugin.EmergencyMap.ZoneColorCmd");
            var bouacolors = new XElement("BOUAColors");
            foreach (var c in mainColors)
            {
                var color = new XElement("Color",c.CMYK);
                bouacolors.Add(color);
            }
            var lyr = new XElement("BOUAFCName", mainBOUAFCName);
            var pac = new XElement("ProPAC", pacProv);
            var attachColor = new XElement("AttachColors");
            foreach (var c in attachColors)
            {
                var color = new XElement("Color", c.Value.RGB);
                color.Add(new XAttribute("Name",c.Key));
                attachColor.Add(color);
            }
            BouaColorEle.Add(bouacolors);
            BouaColorEle.Add(pac);
            BouaColorEle.Add(lyr);
            BouaColorEle.Add(attachColor);
        }
        #endregion
        #region 专题数据相关
        private List<ThematicDataInfo> _thematicList;
        public ThematicDataInfo ThematicInfo = null;
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
            if (!cbThematic.Checked)
            {
                cmbThematicType.SelectedIndex = -1;
            }
        }

        private void GetThematicInfo()
        {
            ThematicInfo = null;
            if (!cbThematic.Checked)
            {
                return;
            }
            ThematicInfo = cmbThematicType.SelectedItem as ThematicDataInfo;
            if (ThematicInfo == null)
                return;

            List<string> fcList = null;
            #region 从图层对照规则表里获取所有需要下载的图层名列表
            string rulepath = GApplication.Application.Template.Root + "\\专题\\" + ThematicInfo.Name + "\\规则对照.mdb";
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

            var dic = ThematicInfo.Lyrs;
            var typedic = ThematicInfo.LyrsType;
            for (int i = 0; i < dic.Keys.ToArray().Length; i++)
            {
                string key = dic.Keys.ToArray()[i];
                dic[key] = false;
                if (fcList != null && !fcList.Contains(key.ToLower()))
                    continue;
                dic[key] = true;
            }
             
        }
        #endregion

        private void cmbThematicType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbThematicType.SelectedIndex == -1)
            {
                cbBaseMapTemplate.SelectedIndex = cbBaseMapTemplate.Items.IndexOf("一般");
            }
            else
            {
                cbBaseMapTemplate.SelectedIndex = cbBaseMapTemplate.Items.IndexOf(cmbThematicType.SelectedItem.ToString());
            }
        }


        #region 注记相关
        public XElement AnnoEle = null;
        FrmAnnoLyr frmAnnoLyr = null;
        DataTable annoTable = null;
        private void btAnno_Click(object sender, EventArgs e)
        {
            string ruleMatchFileName = EnvironmentSettings.getAnnoRuleDBFileName(m_Application);
            DataTable ruleTable = CommonMethods.ReadToDataTable(ruleMatchFileName, "注记规则");
            //添加专题注记规则
            frmAnnoLyr = new FrmAnnoLyr(ruleTable,ruleMatchFileName);
            if (frmAnnoLyr.ShowDialog() != DialogResult.OK)
                return;
            annoTable = frmAnnoLyr.targetDt;
        }
        private void CreateAnnoElement()
        {
            AnnoEle = new XElement("SMGI.Plugin.EmergencyMap.MaplexAnnotateCmd");
            if (annoTable == null)
            {
                string ruleMatchFileName = EnvironmentSettings.getAnnoRuleDBFileName(m_Application);
                annoTable = CommonMethods.ReadToDataTable(ruleMatchFileName, "注记规则");
            }
            for (int i = 0; i < annoTable.Rows.Count; i++)
            {
                DataRow dr = annoTable.Rows[i];
                string lyr = dr["图层"].ToString();
                string id = dr["ID"].ToString();
                string val = (lyr + "_" + id);
                var item = new XElement("RuleItem");
                item.Add(new XAttribute("Name", val));
               
                AnnoEle.Add(item);
            }
        }
        #endregion
        #region 境界跳绘处理
        public XElement BOULSkipEle = null;
        FrmBOULTHSet frmBOULSkip = null;
        double boulDis = 0;
        Dictionary<string, string> boulList = new Dictionary<string, string>();
        Dictionary<string, BoulDrawInfo> boulDrawInfos = new Dictionary<string, BoulDrawInfo>();
        private void CreateBOULSkipElement()
        {
            if (boulList.Count == 0)
            {
                frmBOULSkip = new FrmBOULTHSet(m_Application, "OneKey");
                boulDis = frmBOULSkip.tolerance;
                boulList = frmBOULSkip.layerList;
                boulDrawInfos = frmBOULSkip.BoulDrawDic;
                frmBOULSkip.Dispose();
            }
            BOULSkipEle = new XElement("SMGI.Plugin.EmergencyMap.BoulSkipDrawCmd");
            var skipTol = new XElement("BufferValue", frmBOULSkip.tolerance);
            var skipLayer = new XElement("Layers");
            foreach (var kv in boulList)
            {
                var lyr = new XElement("Layer");
                lyr.Add(new XElement("Name", kv.Key));
                lyr.Add(new XElement("SQL", kv.Value));
                skipLayer.Add(lyr);
            }
            var items = new XElement("BoulInfos");
            foreach (var kv in boulDrawInfos)
            {
                if (kv.Value.Checked)
                {
                    var item = new XElement("Item");
                    item.Add(new XAttribute("Name", kv.Key));
                    
                    BoulDrawInfo obj=kv.Value;
                    item.Add(new XElement("Level", obj.Level));
                    string gblist=string.Empty;
                    foreach(var gb in obj.GBList)
                    {
                        gblist+=gb+",";
                    }
                    gblist = gblist.Substring(0, gblist.Length - 1);
                    item.Add(new XElement("GBList", gblist));
                    item.Add(new XElement("BlankValue", obj.BlankValue));
                    item.Add(new XElement("SolidValue", obj.SolidValue));
                    item.Add(new XElement("PointStep", obj.PointStep));
                    item.Add(new XElement("SymbolGroup", obj.SymbolGroup));
                    items.Add(item);
                }
            }
            BOULSkipEle.Add(skipTol);
            BOULSkipEle.Add(skipLayer);
            BOULSkipEle.Add(items);
        }
        private void btBOULSkip_Click(object sender, EventArgs e)
        {
            frmBOULSkip = new FrmBOULTHSet(m_Application, "OneKey");
            if (DialogResult.OK == frmBOULSkip.ShowDialog())
            {
                boulDis = frmBOULSkip.tolerance;
                boulList = frmBOULSkip.layerList;
                boulDrawInfos = frmBOULSkip.BoulDrawDic;
            }
        }
        #endregion 
        #region  整饰
       
        public XElement FootBorderEle = null;
        double borderWidth = 0;
        double borderStep = 0;
        string borderName = string.Empty;
        string cornerName = string.Empty;
        private void CreateFootBorderElement()
        {
            if (borderName == string.Empty)
            {
                FrmFootBorder frmFootBorder = new FrmFootBorder(true);
                borderWidth = frmFootBorder.BorderWidth;
                borderStep = frmFootBorder.BorderStep;
                borderName = frmFootBorder.LaceList[0].Name;
                cornerName = frmFootBorder.CornerLace.Name;
                frmFootBorder.Dispose();
            }
            FootBorderEle = new XElement("SMGI.Plugin.EmergencyMap.FootBorderCmd");
            //宽度
            FootBorderEle.Add(new XElement("BorderWidth", borderWidth));
            //间距
            FootBorderEle.Add(new XElement("BorderInterval", borderStep));
            //花边元素
            FootBorderEle.Add(new XElement("BorderName", borderName));
            //花边角元素
            FootBorderEle.Add(new XElement("CornerName", cornerName));
        }
        private void btFootBorderCmd_Click(object sender, EventArgs e)
        {
            var frmFootBorder = new FrmFootBorder(true);
            if (DialogResult.OK == frmFootBorder.ShowDialog())
            {
                borderWidth = frmFootBorder.BorderWidth;
                borderStep = frmFootBorder.BorderStep;
                borderName = frmFootBorder.LaceList[0].Name;
                cornerName = frmFootBorder.CornerLace.Name;
            }
        }

        public XElement ScaleBarEle = null;
        string scalebarLocation = string.Empty;
        string scaleUnit = string.Empty;
        List<string> notebar = null;
        double scalebarFont = 1;
        string scaleName = string.Empty;
        private void CreateScaleBarEle ()
        {
            if (scaleName == string.Empty)
            {
                FrmScaleBar frm = new FrmScaleBar();
                {
                    scalebarLocation = frm.ScalebarLocation;
                    scaleUnit = frm.ScaleUnit;
                    notebar = frm.Notebar;
                    scalebarFont = frm.ScalebarFont;
                    scaleName = frm.ScaleBarName;

                }
                frm.Dispose();
            }

            ScaleBarEle = new XElement("SMGI.Plugin.EmergencyMap.ScaleBarCmd");
            ScaleBarEle.Add(new XElement("ScaleBarName", scaleName));
            ScaleBarEle.Add(new XElement("ScalebarLocation", scalebarLocation));
            ScaleBarEle.Add(new XElement("ScaleUnit", scaleUnit));
            ScaleBarEle.Add(new XElement("ScaleFont", scalebarFont));
            var note = new XElement("ScaleNote");
            foreach (var str in notebar)
            {
                note.Add(new XElement("Items",str));
            }
            ScaleBarEle.Add(note);
        }

        private void btScaleBarCmd_Click(object sender, EventArgs e)
        {
            FrmScaleBar frm = new FrmScaleBar(true);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                scalebarLocation = frm.ScalebarLocation;
                scaleUnit = frm.ScaleUnit;
                notebar = frm.Notebar;
                scalebarFont = frm.ScalebarFont;
                scaleName = frm.ScaleBarName;

            }
        }

        //指北针
        public XElement CompassCmdEle = null;
        string compassName=string.Empty;
        double compassSize;
        string anchorLocation = "左上";
        private void CreateCompassEle()
        {
            if (compassName == string.Empty)
            {
                FrmCompassSet frm = new FrmCompassSet();
                {
                    compassName = frm.CompassName;
                    compassSize = frm.CompassSize;
                    anchorLocation = frm.AnchorLocation; 
                }
                frm.Dispose();
            }
            CompassCmdEle = new XElement("SMGI.Plugin.EmergencyMap.CompassCmd");
            CompassCmdEle.Add(new XElement("CompassName", compassName));
            CompassCmdEle.Add(new XElement("CompassSize", compassSize));
            CompassCmdEle.Add(new XElement("AnchorLocation", anchorLocation));
            
        }
        private void btNorthCmd_Click(object sender, EventArgs e)
        {
            FrmCompassSet frm = new FrmCompassSet(true);
            if(frm.ShowDialog()==DialogResult.OK)
            {
                compassName = frm.CompassName;
                compassSize = frm.CompassSize;
                anchorLocation = frm.AnchorLocation;
            }
            
        }
        //位置图
        public XElement AddFigureMapCmdEle = null;
        double figureMapSize =1;
        string figureMapPath =string.Empty;
        string figureMapDir = string.Empty;
        private void CreateFigureMapEle()
        {
            if (figureMapPath == string.Empty)
            {
                FrmFigureMapSet frm = new FrmFigureMapSet();
                { 
                    figureMapSize = frm.FigureMapSize;
                    figureMapPath = frm.MapLocation;
                    figureMapDir = frm.Orientation;
                }
                frm.Dispose();
            }

            AddFigureMapCmdEle = new XElement("SMGI.Plugin.EmergencyMap.AddFigureMapCmdXJ");
            AddFigureMapCmdEle.Add(new XElement("MapSize", figureMapSize));
            AddFigureMapCmdEle.Add(new XElement("MapPath", figureMapPath));
            AddFigureMapCmdEle.Add(new XElement("MapDirection", figureMapDir));

        }
        private void btAddFigureMapCmdXJ_Click(object sender, EventArgs e)
        {
            FrmFigureMapSet frm = new FrmFigureMapSet();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                figureMapSize = frm.FigureMapSize;
                figureMapPath = frm.MapLocation;
                figureMapDir = frm.Orientation;
            }
        }
        //色带处理
        public XElement SDMElement = null;
        private int smdNum = 0;
        private double sdmSum = 0;
        private Dictionary<string, ICmykColor> sdmCmyk = new Dictionary<string, ICmykColor>();
        private void CreateSDMElement()
        {
            if (sdmCmyk.Count == 0)
            {
                SDMCreateFrom frm = new SDMCreateFrom();
                sdmCmyk = frm.SDMColors;
                sdmSum = frm.SDMTotalWidth;
                smdNum = frm.SDMLayerNum;
                frm.Dispose();
            }
            SDMElement = new XElement("SMGI.Plugin.EmergencyMap.SDMColorCmd");
           
            SDMElement.Add(new XElement("SdmSum", sdmSum));
           
            SDMElement.Add(new XElement("SmdNum", smdNum));

            SDMElement.Add(new XElement("ColorIn", sdmCmyk["外层"].CMYK));

            SDMElement.Add(new XElement("ColorOut", sdmCmyk["内层"].CMYK));
        }
        private void btSDM_Click(object sender, EventArgs e)
        {
            SDMCreateFrom frm = new SDMCreateFrom();
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;
            sdmCmyk = frm.SDMColors;
            sdmSum = frm.SDMTotalWidth;
            smdNum = frm.SDMLayerNum;
             

        }
        #endregion

        private void cbBaseMapTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnvironmentSettings.updateBaseMap(GApplication.Application, cbBaseMapTemplate.SelectedItem.ToString());
        }
    }
}
