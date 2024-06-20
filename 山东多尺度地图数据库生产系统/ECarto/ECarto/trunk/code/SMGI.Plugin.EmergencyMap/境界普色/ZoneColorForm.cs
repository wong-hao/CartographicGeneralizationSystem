using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using System.Xml.Linq;
using System.IO;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class ZoneColorForm: Form
    {
        private GApplication app = GApplication.Application;
        private IMap map;
        private Dictionary<string, string> sql2Level;
        Dictionary<string, List<BOUAColor>> listsColorDic = new Dictionary<string, List<BOUAColor>>();
        Dictionary<string, List<BOUAColor>> listsColorDicEx = new Dictionary<string, List<BOUAColor>>();

        string mdbpath = "";
        //邻区颜色
        public  Dictionary<string, IRgbColor> AttachColors = new Dictionary<string, IRgbColor>();
        //主区颜色
        public List<ICmykColor> CMYKColors = new List<ICmykColor>();
        public string FCName
        {
            get;
            set;
        }
        public ILayer Layer
        {
            get;
            set;
        }
        public string ColorFieldName
        {
            get;
            set;
        }
        public int ColorNum
        {
            get;
            set;
        }

        public string SQLText
        {
            get;
            set;
        }

        public string ProvPAC
        {
            get;
            set;
        }
        public IColor BOUAAtColor=null;
       
        public ZoneColorForm()
        {
            InitializeComponent();
            IntiParms();
        }
        public ZoneColorForm(bool attach_)
        {
            InitializeComponent();
            IntiParms(attach_);
        }
        private void IntiParms(bool mapAt=false)
        {
            ProvPAC = app.Template.Content.Element("ProvincePAC").Value;
            mdbpath = app.Template.Root + @"\专家库\Colors.mdb";
            //加载颜色方案;
            LoadColors();
            foreach (var kv in listsColorDic)
            {
                cmbColor.Items.Add(kv.Key);
            }
            cmbColor.SelectedIndex = 0;
            LoadColorsEx();
            foreach (var kv in listsColorDicEx)
            {
                cmbEx.Items.Add(kv.Key);
            }
            cmbEx.SelectedIndex = 0;
            map = app.ActiveView.FocusMap as IMap;

            cmbLayers.ValueMember = "Key";
            cmbLayers.DisplayMember = "Value";
            try
            {
                var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper().StartsWith("BOUA"));
                })).ToArray();
                for(int i = lyrs.Count()-1; i>=0; --i)
                {
                    cmbLayers.Items.Add(new KeyValuePair<string, string>((lyrs[i] as IFeatureLayer).FeatureClass.AliasName, lyrs[i].Name));
                }
            }
            catch
            {
                cmbLayers.Items.Add(new KeyValuePair<string, string>("BOUA2", "省级行政区面"));
                cmbLayers.Items.Add(new KeyValuePair<string, string>("BOUA4", "地级行政区面"));
                cmbLayers.Items.Add(new KeyValuePair<string, string>("BOUA5", "县级行政区面"));
                cmbLayers.Items.Add(new KeyValuePair<string, string>("BOUA6", "乡级行政区面"));
               
            }
            if (cmbLayers.Items.Count > 2)
            {
                cmbLayers.SelectedIndex = 2;  //设置默认普色图层
            }
            else
            {
                cmbLayers.SelectedIndex = 0;
            }
            var obj = (KeyValuePair<string, string>)cmbLayers.SelectedItem;
            FCName = obj.Key;
            
            nameDic[0] = "省内";
            nameDic[1] = "省外";
            nameDic[2] = "国外";
            bool attachMap = false;
            try
            {
                //是否存在附区
                Dictionary<string, string> envString = app.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
                if (envString == null)
                {
                    envString = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
                }

                if (envString != null && envString.ContainsKey("AttachMap"))
                {
                    attachMap = bool.Parse(envString["AttachMap"]);

                }
            }
            catch
            {
                attachMap = mapAt;
            } 
            if (!attachMap)
            {
                panelColorSet.Visible = false;
                this.Height -= panelColorSet.Height;
            }
            foreach (Control control in ColorPanel.Controls)
            {
                if (control is Panel)
                {
                    ICmykColor color = ColorHelper.ConvertColorToCMYK(control.BackColor);
                    CMYKColors.Add(color);
                }


            }
            int index = 0;
            foreach (Control control in ColorPanelEx.Controls)
            {
                if (control is Panel)
                {
                    IRgbColor color = ColorHelper.ConvertColorToIColor(control.BackColor);
                    AttachColors[nameDic[index]] = color;
                    index++;
                }

            }
        }
        private void FrmZoneColor_Load(object sender, EventArgs e)
        {
           
            
           
        }

        private void cmbLayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var obj = (KeyValuePair<string, string>)cmbLayers.SelectedItem;

                FCName = obj.Key;
                IWorkspace pws = app.Workspace.EsriWorkspace;
                IFeatureClass pfcl = GetFclViaWs(pws, FCName);
                if (pfcl != null)
                {
                    cmbField.Items.Clear();
                    for (int i = 0; i < pfcl.Fields.FieldCount; i++)
                    {
                        IField pf = pfcl.Fields.Field[i];
                        if (pf.Type != esriFieldType.esriFieldTypeOID && pf.Type != esriFieldType.esriFieldTypeGeometry && pf.Name != pfcl.LengthField.Name && pf.Name != pfcl.AreaField.Name)
                            cmbField.Items.Add(pfcl.Fields.Field[i].Name);
                    }
                    if (cmbField.Items.Contains("color")) { cmbField.SelectedText = "color"; }
                }
            }
            catch
            {
            }

        }
     
        

        private void tbSQL_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }


        Dictionary<int, string> nameDic = new Dictionary<int, string>();
        
        private void btOK_Click(object sender, EventArgs e)
        {
            Layer = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == FCName.ToUpper());
            })).FirstOrDefault();

            ColorNum = 5;
            ColorFieldName = "color";
            
            CMYKColors.Clear();
            if(cmbColor.SelectedItem!=null)
            {
                try
                {
                    foreach (Control control in ColorPanel.Controls)
                    {
                        if (control is Panel)
                        {
                            ICmykColor color = ColorHelper.ConvertColorToCMYK(control.BackColor);
                            CMYKColors.Add(color);
                        }
                        

                    }
                    int index = 0;
                    foreach (Control control in ColorPanelEx.Controls)
                    {
                        if (control is Panel)
                        {
                            IRgbColor color = ColorHelper.ConvertColorToIColor(control.BackColor);
                            AttachColors[nameDic[index]] = color;
                            index++;
                        }


                    }
                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = SQLText;
                    //只处理主区数据
                   
                    
                    qf.WhereClause = "ATTACH is NULL";
                    SQLText = "ATTACH is NULL";
                    try
                    {
                        //是否存在附区
                        Dictionary<string, string> envString = app.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
                        if (envString == null)
                        {
                            envString = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
                        }
                        bool attachMap = false;
                        if (envString != null && envString.ContainsKey("AttachMap"))
                        {
                            attachMap = bool.Parse(envString["AttachMap"]);

                        }
                    }
                    catch
                    {
                    }
                    if (Layer != null)
                    {
                        int featureCount = (Layer as IFeatureLayer).FeatureClass.FeatureCount(qf);
                        if (featureCount == 0)
                        {
                            MessageBox.Show("当前图层要素为空！");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Source);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                    MessageBox.Show(ex.Message);
                    return;
                }
            }
            
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        /// <summary>
        /// 获取要素类
        /// </summary>
        /// <param name="pws"></param>
        /// <param name="fclName"></param>
        /// <returns></returns>
        private IFeatureClass GetFclViaWs(IWorkspace pws, string fclName)
        {
            try
            {
                IFeatureClass fcl = null;
                var lyr=  GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                        {
                            return (l is IFeatureLayer) && ((l.Name == fclName));
                        })).FirstOrDefault() as IFeatureLayer;
                if (lyr != null)
                {
                    fcl = lyr.FeatureClass;

                }
                return fcl;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return null;
            }
        }
        #region //颜色处理
        private void ClearControls()
        {
            foreach (Control control in ColorPanel.Controls)
            {

                ColorPanel.Controls.Remove(control);

            }
            if (ColorPanel.Controls.Count > 0)
                ClearControls();
        }
        private void ClearControlsEx()
        {
            foreach (Control control in ColorPanelEx.Controls)
            {

                ColorPanelEx.Controls.Remove(control);

            }
            if (ColorPanelEx.Controls.Count > 0)
                ClearControlsEx();
        }
        private void LoadColors()
        {
            ClearControls();
            listsColorDic.Clear();
            DataTable dt = CommonMethods.ReadToDataTable(mdbpath, "境界填色");
          
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];

                string planName = "方案" + dr["填色方案"].ToString();
                BOUAColor b = new BOUAColor();
                b.FID = int.Parse(dr["ID"].ToString());
                b.ColorPlan = planName;
                b.ColorIndex = dr["优先顺序"].ToString();

                int c1 = int.Parse(dr["C"].ToString());
                int m = int.Parse(dr["M"].ToString());
                int y = int.Parse(dr["Y"].ToString());
                int k = int.Parse(dr["K"].ToString());
                CmykColorClass pcolor = new CmykColorClass();
                pcolor.Cyan = c1;
                pcolor.Magenta = m;
                pcolor.Yellow = y;
                pcolor.Black = k;
                b.CmykColor = pcolor;
                if (listsColorDic.ContainsKey(planName))
                {
                    List<BOUAColor> colorlist = listsColorDic[planName];
                    colorlist.Add(b);
                    listsColorDic[planName] = colorlist;
                }
                else
                {
                    List<BOUAColor> colorlist = new List<BOUAColor>();
                    colorlist.Add(b);
                    listsColorDic[planName] = colorlist;
                }

            }
           
        }
        //加载邻区颜色
        private void LoadColorsEx()
        {
            ClearControlsEx();
            listsColorDicEx.Clear();
            DataTable dt = CommonMethods.ReadToDataTable(mdbpath, "邻区境界填色");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];

                string planName = "方案" + dr["填色方案"].ToString();
                BOUAColor b = new BOUAColor();
                b.FID = int.Parse(dr["ID"].ToString());
                b.ColorPlan = planName;
                b.Name = dr["TYPE"].ToString();

                int c1 = int.Parse(dr["C"].ToString());
                int m = int.Parse(dr["M"].ToString());
                int y = int.Parse(dr["Y"].ToString());
                int k = int.Parse(dr["K"].ToString());
                CmykColorClass pcolor = new CmykColorClass();
                pcolor.Cyan = c1;
                pcolor.Magenta = m;
                pcolor.Yellow = y;
                pcolor.Black = k;
                b.CmykColor = pcolor;
                if (listsColorDicEx.ContainsKey(planName))
                {
                    List<BOUAColor> colorlist = listsColorDicEx[planName];
                    colorlist.Add(b);
                    listsColorDicEx[planName] = colorlist;
                }
                else
                {
                    List<BOUAColor> colorlist = new List<BOUAColor>();
                    colorlist.Add(b);
                    listsColorDicEx[planName] = colorlist;
                }

            }

        }
        private void DrawColorBtEx(string type)
        {
            int row = 0;
            var list = listsColorDicEx[type];

            int colomn = 0;

            Label lbinfo = new Label();
            lbinfo.Top = 25 + 55 * (row);
            lbinfo.Left = 5;
            lbinfo.AutoSize = true;
            lbinfo.Text = type;
            ColorPanelEx.Controls.Add(lbinfo);

            foreach (var l in list)
            {

                int x = colomn++;
                int y = row;

                Color pcolor = ColorHelper.ConvertICMYKColorToColor(l.CmykColor);
                Panel panel = new Panel();
                panel.Width = 35;
                panel.Tag = l.FID;
                panel.Height = 30;
                panel.Location = new System.Drawing.Point(45 + x * 55, 15 + 55 * y);
                panel.BackColor = pcolor;
                ColorPanelEx.Controls.Add(panel);

                Label lb = new Label();
                lb.Top = 50 + 55 * y;
                lb.Left = 45 + x * 55;
                lb.AutoSize = true;
                lb.Text = (l.Name).Trim();
                ColorPanelEx.Controls.Add(lb);

                panel.MouseClick += new MouseEventHandler(panel_MouseClick);
                panel.MouseDoubleClick += new MouseEventHandler(panel_MouseDoubleClick);

            }


        }

        private void DrawColorBt(string type)
        {
            int row = 0;
            var list = listsColorDic[type];
             
            int colomn = 0;

            Label lbinfo = new Label();
            lbinfo.Top = 25 + 55 * (row);
            lbinfo.Left = 5;
            lbinfo.AutoSize = true;
            lbinfo.Text = type;
            ColorPanel.Controls.Add(lbinfo);
            var listOrder = list.OrderBy(t => int.Parse(t.ColorIndex));
            foreach (var l in listOrder)
            {

                int x = colomn++;
                int y = row;

                Color pcolor = ColorHelper.ConvertICMYKColorToColor(l.CmykColor);
                Panel panel = new Panel();
                panel.Width = 35;
                panel.Tag = l.FID;
                panel.Height = 30;
                panel.Location = new System.Drawing.Point(45 + x * 55, 15 + 55 * y);
                panel.BackColor = pcolor;
                ColorPanel.Controls.Add(panel);

                Label lb = new Label();
                lb.Top = 50 + 55 * y;
                lb.Left = 45 + x * 55;
                lb.AutoSize = true;
                lb.Text = ("颜色:" + l.ColorIndex).Trim();
                ColorPanel.Controls.Add(lb);

                panel.MouseClick += new MouseEventHandler(panel_MouseClick);
                panel.MouseDoubleClick += new MouseEventHandler(panel_MouseDoubleClick);
          
            }
                
            
        }
        private void panel_MouseClick(object sender, MouseEventArgs e)
        {
            foreach (Control c in ColorPanel.Controls)
            {
                if (c.GetType() == typeof(System.Windows.Forms.Panel))
                {
                    (c as Panel).BorderStyle = BorderStyle.None;
                }
            }
            (sender as Panel).BorderStyle = BorderStyle.Fixed3D;
           //PanelSe = (sender as Panel);
        }
        private void panel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            foreach (Control c in (sender as Panel).Parent.Controls)
            {
                if (c.GetType() == typeof(System.Windows.Forms.Panel))
                {
                    (c as Panel).BorderStyle = BorderStyle.None;
                }
            }
            (sender as Panel).BorderStyle = BorderStyle.Fixed3D;
          
            //弹出颜色窗体
            IColorPalette colorPalette;
            colorPalette = new ColorPalette();
            IColor color = ColorHelper.ConvertColorToIColor((sender as Panel).BackColor);
            ESRI.ArcGIS.esriSystem.tagRECT tagRect = new ESRI.ArcGIS.esriSystem.tagRECT();
            tagRect.left = (this.Left * 2 + this.Width) / 2 - 100;           
            tagRect.bottom = (this.Top * 2 + this.Height) / 2 - 100;
            if (colorPalette.TrackPopupMenu(ref tagRect, color, false, 0))
            {
               
                (sender as Panel).BackColor = ColorHelper.ConvertIColorToColor(colorPalette.Color);
                //ICmykColor cmyk = ColorHelper.ConvertColorToCMYK((sender as Panel).BackColor);
                //var sql = "UPDATE "+tableName+" SET " +
                //      "C = '" + cmyk.Cyan.ToString() + "'" +
                //     ",Y = '" + cmyk.Yellow.ToString() + "'" +
                //     ",M = '" + cmyk.Magenta.ToString() + "'" +
                //     ",K = '" + cmyk.Black.ToString() + "' where ID=" + (sender as Panel).Tag.ToString();

                //IWorkspaceFactory awf = new AccessWorkspaceFactory();
                //var ws = awf.OpenFromFile(mdbpath, 0);
                //ws.ExecuteSQL(sql);
                //Marshal.ReleaseComObject(ws);
                //Marshal.ReleaseComObject(awf);
            }

        }
      
        #endregion

        private void cmbColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadColors();
            String type = cmbColor.SelectedItem.ToString();
            DrawColorBt(type);
        }

        private void colorDetail_Click(object sender, EventArgs e)
        { 
            String type = cmbColor.SelectedItem.ToString();
            ColorForm frm = new ColorForm(type);
            frm.StartPosition = FormStartPosition.CenterParent;        
            DialogResult dr=  frm.ShowDialog();
            if (dr == DialogResult.OK)
            {
                LoadColors();
                cmbColor.Items.Clear();
                foreach (var kv in listsColorDic)
                {
                    cmbColor.Items.Add(kv.Key);
                }
                cmbColor.SelectedItem = type;
               // DrawColorBt(type);
            }
        }

        private void btCancel_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
 

        private void cmbField_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        //选择颜色
        private void cmbColors_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }

        private void cmbEx_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadColorsEx();
            String type = cmbEx.SelectedItem.ToString();
            DrawColorBtEx(type);
        }

        private void colorExSet_Click(object sender, EventArgs e)
        {
            String type = cmbEx.SelectedItem.ToString();
            ColorForm frm = new ColorForm(type, "邻区境界填色");
            frm.StartPosition = FormStartPosition.CenterParent;
            DialogResult dr = frm.ShowDialog();
            if (dr == DialogResult.OK)
            {
                LoadColorsEx();
                cmbEx.Items.Clear();
                foreach (var kv in listsColorDicEx)
                {
                    cmbEx.Items.Add(kv.Key);
                }
                cmbEx.SelectedItem = type;
                // DrawColorBt(type);
            }
        }

        

        
    }
}