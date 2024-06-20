using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.Xml.Linq;

using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.InteropServices;

using ESRI.ArcGIS.Display;
using stdole;
using  ESRI.ArcGIS.ADF.COMSupport;
using System.IO;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmMapNameset : Form
    {
        GApplication m_Application=null;
        public FrmMapNameset()
        {
            InitializeComponent();
            m_Application = GApplication.Application;
        }

       public IFeatureClass fclanno = null;
       public bool TxtShadow = false;
    
       private Color  textColor = Color.FromName("Black");
       private System.Drawing.Font   textFontName = new System.Drawing.Font("汉仪大宋简",50);
       private Color textColorname = Color.FromName("Black");
       private System.Drawing.Font textFontUnit = new System.Drawing.Font("黑体", 20);
       private Color textColortime = Color.FromName("Black");
       private System.Drawing.Font textFontTime = new System.Drawing.Font("黑体", 20);
       private Color textColorPublish = Color.FromName("Black");
       private System.Drawing.Font textFontPublish = new System.Drawing.Font("黑体", 20);

       IColor pColor = null;
       IFontDisp pFont = null;
       private void cmdOK_Click(object sender, EventArgs e)
       {
           if (!isClipGeoOut())
           {
               MessageBox.Show("请先生成图廓线！");
               return;
           }

           if (txt_CartoName.Text == "")
           {
               MessageBox.Show("请设置地图名称");
               return;
           }
              
           string NameContent = txt_CartoName.Text;

           bool MapNameHorizon = true;
           if (rb_MapNameHorizon.Checked)
           {
               MapNameHorizon = true;
           }
           else
           {
               MapNameHorizon = false;
           }

          // using (WaitOperation wo = m_Application.SetBusy())
           m_Application.EngineEditor.StartOperation();
           {
               pColor = ConvertColorToIColor(textColor);
               pFont = (stdole.IFontDisp)OLE.GetIFontDispFromFont(textFontName);

               MapLayoutHelperLH mh = new MapLayoutHelperLH();
               mh.MapNameSpace = double.Parse(txtWordSpace.Text);//图名字符间距
               mh.MapNameWidth = double.Parse(txtWordWidth.Text);
               mh.MapNameDis = double.Parse(txtTopDis.Text);//图名图廓间距
               IQueryFilter qf = new QueryFilterClass();
               if (NameContent.Trim() != "")
               {
                   mh.CreateMapName(cbShadow.Checked, NameContent.Trim(), pFont, pColor, mh.MapNameDis,MapNameHorizon);
               }
               if (txt_InstitutionName.Text != "")
               {
                   mh.CreateMapProducer(txt_InstitutionName.Text.Trim(), (stdole.IFontDisp)OLE.GetIFontDispFromFont(textFontUnit), ConvertColorToIColor(textColorname), "生产商");
               }
               else
               {
                   qf.WhereClause = "TYPE = '生产商'";
                   mh.ClearFeatures(fclanno, qf);
               }
               if (txt_Time.Text != "")
               {
                   mh.CreateMapProducer(txt_Time.Text.Trim(), (stdole.IFontDisp)OLE.GetIFontDispFromFont(textFontTime), ConvertColorToIColor(textColortime), "生产时间");
               }
               else
               {
                   qf.WhereClause = "TYPE = '生产时间'";
                   mh.ClearFeatures(fclanno, qf);                                  
               }

               qf.WhereClause = "TYPE = '出版形式'";
               mh.ClearFeatures(fclanno, qf); 
               if (cbPublishType.Text != "")
               {
                   double dist = 0;
                   double.TryParse(tbPublishDist.Text, out dist);
                   mh.CreateMapPublishType(cbPublishType.Text, (stdole.IFontDisp)OLE.GetIFontDispFromFont(textFontPublish), ConvertColorToIColor(textColorPublish), dist, rbH.Checked);
               }

               m_Application.ActiveView.Refresh();
               Marshal.ReleaseComObject(qf);
               mh = null;
               GC.Collect();
           }
           m_Application.EngineEditor.StopOperation("图名设置");
           SaveParams();
           Close();
        }
        //保存参数
       private void SaveParams()
       {
           var mapName = new XElement("MapName");
           var cartoName = new XElement("CartoName",txt_CartoName.Text);//图名
           mapName.Add(cartoName);
         
           var institutionName = new XElement("InstitutionName",txt_InstitutionName.Text);//制图单位名
           mapName.Add(institutionName);
           var productionTime = new XElement("ProductionTime", txt_Time.Text);//制图日期
           mapName.Add(productionTime);
           var art = new XElement("ArtChecked", cbShadow.Checked.ToString());
           mapName.Add(art);
           #region 字体相关
           //地图图名
           var title = new XElement("Title");
           title.Add(new XElement("FontSize",((stdole.IFontDisp)OLE.GetIFontDispFromFont(textFontName)).Size));
           title.Add(new XElement("FontName", ((stdole.IFontDisp)OLE.GetIFontDispFromFont(textFontName)).Name));
           title.Add(new XElement("TopDis", txtTopDis.Text));
           var color = new XElement("FontColor");
           var irgbColor = ConvertColorToIColor(textColor) as IRgbColor;
           color.Add(new XElement("Red", irgbColor.Red));
           color.Add(new XElement("Green", irgbColor.Green));
           color.Add(new XElement("Blue", irgbColor.Blue));
           title.Add(color);
           mapName.Add(title);
           //制作单位
           var productUnit = new XElement("ProductUnit");
           productUnit.Add(new XElement("TopDis",this.txtProducerDis.Text));
           productUnit.Add(new XElement("FontSize", ((stdole.IFontDisp)OLE.GetIFontDispFromFont(textFontUnit)).Size));
           productUnit.Add(new XElement("FontName", ((stdole.IFontDisp)OLE.GetIFontDispFromFont(textFontUnit)).Name));
           productUnit.Add(new XElement("TopDis", txtProducerDis.Text));
           color = new XElement("FontColor");
           irgbColor = ConvertColorToIColor(textColorname) as IRgbColor;
           color.Add(new XElement("Red", irgbColor.Red));
           color.Add(new XElement("Green", irgbColor.Green));
           color.Add(new XElement("Blue", irgbColor.Blue));
           productUnit.Add(color);
           mapName.Add(productUnit);
           //制作时间
           var mapTime = new XElement("MapTime");
           mapTime.Add(new XElement("TopDis", txtTimeDis.Text));
           mapTime.Add(new XElement("FontSize", ((stdole.IFontDisp)OLE.GetIFontDispFromFont(textFontTime)).Size));
           mapTime.Add(new XElement("FontName", ((stdole.IFontDisp)OLE.GetIFontDispFromFont(textFontTime)).Name));
           color = new XElement("FontColor");
           irgbColor = ConvertColorToIColor(textColortime) as IRgbColor;
           color.Add(new XElement("Red", irgbColor.Red));
           color.Add(new XElement("Green", irgbColor.Green));
           color.Add(new XElement("Blue", irgbColor.Blue));
           mapTime.Add(color);
           mapName.Add(mapTime);
           //出版形式
           var mappublish = new XElement("MapPublish");
           mappublish.Add(new XElement("TopDis", tbPublishDist.Text));
           mappublish.Add(new XElement("Content", cbPublishType.Text));
           mappublish.Add(new XElement("Direction",rbH.Checked));
           mapName.Add(mappublish);
           #endregion
           ExpertiseParamsClass.UpdateMapName(GApplication.Application, mapName);
       }
      
        private void cmdCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        //.net color转为Icolor
        public IColor ConvertColorToIColor(Color p_Color)
        {
            IColor pColor = new RgbColorClass { RGB = p_Color.B * 65536 + p_Color.G * 256 + p_Color.R };
            return pColor;
        }

        private void cmdFont_Click(object sender, EventArgs e)
        {
            FontDialog pFontDialog = new FontDialog();
            pFontDialog.Font = textFontName;
            if (pFontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                lbfont.Text = "字体：" + pFontDialog.Font.Name + "," + pFontDialog.Font.Size + "pt";             
                //lbfont.Font = pFontDialog.Font;
                textFontName = pFontDialog.Font;
            }
        }

        private void cmdColor_Click(object sender, EventArgs e)
        {
            ColorDialog pColorDialog = new ColorDialog();
            pColorDialog.Color = textColor;
            if (pColorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                lbcolor.ForeColor = pColorDialog.Color;
                lbcolor.Text = pColorDialog.Color.Name;
                textColor = pColorDialog.Color;
            }
        }
       
        //加载外部参数
        private void LoadOutParams(string fileName,bool flag = false)
        {
            //string planStr = @"专家库\经验方案\经验方案.xml";
            //string fileName = m_Application.Template.Root + @"\" + planStr;
            
            XDocument doc;
            
            {
                doc = XDocument.Load(fileName);
                try
                {
                    var content = doc.Element("Template").Element("Content");
                    var mapname = content.Element("MapName");
                    
                    if (flag)
                    {

                        var cartoName = mapname.Element("CartoName");
                        if(cartoName != null)
                            txt_CartoName.Text = cartoName.Value;
                        var institutionName = mapname.Element("InstitutionName");
                        if(institutionName != null)
                            txt_InstitutionName.Text = institutionName.Value;
                        var productionTime = mapname.Element("ProductionTime");
                        if(productionTime != null)
                            txt_Time.Text = productionTime.Value;
                    }
                    var art = bool.Parse(mapname.Element("ArtChecked").Value);
                    cbShadow.Checked = art;
                    // MapTitle
                    var title = mapname.Element("Title");
                    if (title.Element("TopDis") != null)
                    {
                        this.txtTopDis.Text = title.Element("TopDis").Value;
                    }
                    //font
                    float fontsize = float.Parse(title.Element("FontSize").Value);
                    string fontname = title.Element("FontName").Value;
                    textFontName = new System.Drawing.Font(fontname, fontsize);
                    lbfont.Text = "字体：" + textFontName.Name + "," + textFontName.Size + "pt";
                    //color
                    int r = int.Parse(title.Element("FontColor").Element("Red").Value);
                    int g = int.Parse(title.Element("FontColor").Element("Green").Value);
                    int b = int.Parse(title.Element("FontColor").Element("Blue").Value);
                    var icolor = new RgbColorClass { Red = r, Green = g, Blue = b };
                    textColor = ColorTranslator.FromOle(icolor.RGB);
                    //map unit
                    var unit = mapname.Element("ProductUnit");
                    if (unit.Element("TopDis") != null)
                    {
                        this.txtProducerDis.Text = unit.Element("TopDis").Value;
                    }
                    //font
                    fontsize = float.Parse(unit.Element("FontSize").Value);
                    fontname = unit.Element("FontName").Value;
                    textFontUnit = new System.Drawing.Font(fontname, fontsize);
                    //color
                    r = int.Parse(unit.Element("FontColor").Element("Red").Value);
                    g = int.Parse(unit.Element("FontColor").Element("Green").Value);
                    b = int.Parse(unit.Element("FontColor").Element("Blue").Value);
                    icolor = new RgbColorClass { Red = r, Green = g, Blue = b };
                    textColorname = ColorTranslator.FromOle(icolor.RGB);
                    // map Time
                    var time = mapname.Element("MapTime");
                    if (time.Element("TopDis") != null)
                    {
                        this.txtTimeDis.Text = time.Element("TopDis").Value;
                    }
                    //font
                    fontsize = float.Parse(time.Element("FontSize").Value);
                    fontname = time.Element("FontName").Value;
                    textFontTime = new System.Drawing.Font(fontname, fontsize);
                    //color
                    r = int.Parse(time.Element("FontColor").Element("Red").Value);
                    g = int.Parse(time.Element("FontColor").Element("Green").Value);
                    b = int.Parse(time.Element("FontColor").Element("Blue").Value);
                    icolor = new RgbColorClass { Red = r, Green = g, Blue = b };
                    textColortime = ColorTranslator.FromOle(icolor.RGB);
                    //出版
                    var mappublish = mapname.Element("MapPublish");
                    if (mappublish != null)
                    {
                        tbPublishDist.Text = mappublish.Element("TopDis").Value;
                        cbPublishType.SelectedIndex = cbPublishType.Items.IndexOf(mappublish.Element("Content").Value);
                        bool check = bool.Parse(mappublish.Element("Direction").Value);
                        rbH.Checked = check;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Source);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                    MessageBox.Show(string.Format("经验方案xml中【图名生成】配置错误：{0}", ex.Message));
                    return;
                }
                
            }
        }
        private void FrmMapNameset_Load(object sender, EventArgs e)
        {  
            string planxml = "";
            if (ExpertiseParamsClass.LoadOutParams(out planxml))//加载外部参数
            {
                if (File.Exists(planxml))
                    LoadOutParams(planxml);
            }
           //时间
            DateTime dt = DateTime.Now;
           
            txt_Time.Text =  dt.Year+"年"+dt.Month+"月";
            var lyr= GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IFDOGraphicsLayer)&&(l as IFeatureLayer).FeatureClass.AliasName.ToUpper()=="LANNO").FirstOrDefault();
            if (lyr == null)
                return;
            fclanno = (lyr as IFeatureLayer).FeatureClass;
            IQueryFilter qf = new QueryFilterClass();
           
            IFeature fe;
            IFeatureCursor cursor = null;
            //图名
            qf.WhereClause = "TYPE = '图名'";
            cursor = fclanno.Search(qf, false);
            fe = cursor.NextFeature();
            if (fe != null)
            {
                var namefe = fe as IAnnotationFeature2;
                var txtEle = namefe.Annotation as ITextElement;
                txt_CartoName.Text = txtEle.Text;
            }
            else//如果图名为空，将gdb名称设为默认
            {
                string gdbpath = GApplication.Application.Workspace.EsriWorkspace.PathName;
                string mapname = System.IO.Path.GetFileNameWithoutExtension(gdbpath);
                mapname = mapname.Replace("_Ecarto", "");
                txt_CartoName.Text = mapname;
               
            }
            //生成商
            qf.WhereClause = "TYPE = '生产商'";
            cursor = fclanno.Search(qf, false);
            fe = cursor.NextFeature();
            if (fe != null)
            {
                var namefe = fe as IAnnotationFeature2;
                var txtEle = namefe.Annotation as ITextElement;
                txt_InstitutionName.Text = txtEle.Text;
            }
            //生产时间
            qf.WhereClause = "TYPE = '生产时间'";
            cursor = fclanno.Search(qf, false);
            fe = cursor.NextFeature();
            if (fe != null)
            {
                var namefe = fe as IAnnotationFeature2;
                var txtEle = namefe.Annotation as ITextElement;
                txt_Time.Text = txtEle.Text;
            }
            Marshal.ReleaseComObject(cursor);
           DataRow mapnameItem = GetNearestSize();
           if (mapnameItem != null)
           {
               if (mapnameItem.Table.Columns.Contains("出版形式"))
               {
                   if (mapnameItem["出版形式"] != DBNull.Value)
                   {
                       double fontSize = 0;
                       double.TryParse(mapnameItem["出版形式"].ToString(), out fontSize);
                       if (fontSize > 0)
                       {
                           textFontPublish = new System.Drawing.Font("黑体", (float)(fontSize * 2.83));
                       }
                       
                   }
               }
               if (mapnameItem["图名文字尺寸"] != DBNull.Value)
               {

                   double size = double.Parse(mapnameItem["图名文字尺寸"].ToString());
                   textFontName = new System.Drawing.Font("黑体", (float)(size * 2.83));
               }
               if (mapnameItem["制作单位文字尺寸"] != DBNull.Value)
               {
                   double size = double.Parse(mapnameItem["制作单位文字尺寸"].ToString());
                   textFontUnit = new System.Drawing.Font("黑体", (float)(size * 2.83));
               }
               if (mapnameItem["制作时间文字尺寸"] != DBNull.Value)
               {
                   double size = double.Parse(mapnameItem["制作时间文字尺寸"].ToString());
                   textFontTime = new System.Drawing.Font("黑体", (float)(size * 2.83));
               }
               if (mapnameItem["图名与外图廓间距"] != DBNull.Value)
               {
                   double size = double.Parse(mapnameItem["图名与外图廓间距"].ToString());
                   txtTopDis.Text = size.ToString();
               }
           }

        }



        //获取最近的【纸张开本】
        IPolygon pageGeo = null;
        DataTable ruleDatatable = null;
        private DataRow GetNearestSize()
        {
            DataRow itemSel = null;
            try
            {
                GApplication m_Application = GApplication.Application;
                IWorkspace2 pws2 = m_Application.Workspace.EsriWorkspace as IWorkspace2;
                IFeatureWorkspace fws = pws2 as IFeatureWorkspace;
                if (pageGeo == null)
                {
                    if (pws2.get_NameExists(esriDatasetType.esriDTFeatureClass, "ClipBoundary"))
                    {
                        #region
                        IFeatureClass clipfcl = fws.OpenFeatureClass("ClipBoundary");
                        if (clipfcl.FeatureCount(null) != 0)
                        {
                            IFeature fe = null;
                            IQueryFilter qf = new QueryFilterClass();
                            qf.WhereClause = "TYPE = '页面'";
                            IFeatureCursor cursor = clipfcl.Search(qf, false);
                            fe = cursor.NextFeature();
                            pageGeo = fe.ShapeCopy as IPolygon;
                            Marshal.ReleaseComObject(cursor);
                        }
                        #endregion
                    }
                }
                double ms = m_Application.ActiveView.FocusMap.ReferenceScale;
                double width = pageGeo.Envelope.Width / ms * 1000; //毫米
                double height = pageGeo.Envelope.Height / ms * 1000;//毫米

                double min = Math.Min(width, height);
                double max = Math.Max(width, height);
                string filemdb = m_Application.Template.Root + @"\专家库\纸张整饰参数\整饰参数经验值.mdb";
                if (ruleDatatable == null)
                {
                    
                    ruleDatatable = CommonMethods.ReadToDataTable(filemdb, "纸张整饰参数经验值");  
                }


                for (int i = 0; i < ruleDatatable.Rows.Count; i++)
                {
                    #region 考虑两边
                    DataRow row = ruleDatatable.Rows[i];
                    double pagewidth = double.Parse(row["纸张宽"].ToString());
                    double pageheight = double.Parse(row["纸张高"].ToString());
                    double pagemin = Math.Min(pagewidth, pageheight);
                    double pagemax = Math.Max(pagewidth, pageheight);
                    if ((Math.Abs(pagemin - min) / min) < 0.2 && Math.Abs(pagemax - max) / max < 0.2)
                    {
                        itemSel = row;
                        break;
                    }
                    #endregion
                }
                if (itemSel == null)
                {
                    #region 考虑单边
                    for (int i = 0; i < ruleDatatable.Rows.Count; i++)
                    {
                        DataRow row = ruleDatatable.Rows[i];
                        double pagewidth = double.Parse(row["纸张宽"].ToString());
                        double pageheight = double.Parse(row["纸张高"].ToString());
                        double pagemin = Math.Min(pagewidth, pageheight);
                        double pagemax = Math.Max(pagewidth, pageheight);
                        if ((Math.Abs(pagemin - min) / min) < 0.2 || Math.Abs(pagemin - max) / max < 0.2)
                        {
                            itemSel = row;
                            break;
                        }
                        if ((Math.Abs(pagemax - min) / min) < 0.2 || Math.Abs(pagemax - max) / max < 0.2)
                        {
                            itemSel = row;
                            break;
                        }
                    }
                    #endregion
                }

                return itemSel;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return null;
            }

        }
        private DataTable LoadDataFromExcel(string filePath)
        {
            try
            {
                string strConn;
                strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties='Excel 8.0;HDR=YES;IMEX=1'";
                System.Data.OleDb.OleDbConnection OleConn = new System.Data.OleDb.OleDbConnection(strConn);
                OleConn.Open();
                String sql = "SELECT * FROM  [Sheet1$]";
                System.Data.OleDb.OleDbDataAdapter OleDaExcel = new System.Data.OleDb.OleDbDataAdapter(sql, OleConn);
                DataSet OleDsExcle = new DataSet();
                OleDaExcel.Fill(OleDsExcle, "Sheet1");
                OleConn.Close();
                return OleDsExcle.Tables[0];
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLine(err.Message);
                System.Diagnostics.Trace.WriteLine(err.Source);
                System.Diagnostics.Trace.WriteLine(err.StackTrace);
                MessageBox.Show("加载Excel失败!失败原因：" + err.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
        }

        private void cmdFontname_Click(object sender, EventArgs e)
        {
            FontDialog pFontDialog = new FontDialog();
            pFontDialog.Font = textFontUnit;
            if (pFontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                lbfontname.Text = "字体：" + pFontDialog.Font.Name + "," + pFontDialog.Font.Size + "pt";
              //  lbfontname.Font = pFontDialog.Font;
                textFontUnit = pFontDialog.Font;
            }
        }

        private void cmdColorname_Click(object sender, EventArgs e)
        {
            ColorDialog pColorDialog = new ColorDialog();
            pColorDialog.Color = textColorname;
            if (pColorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                lbcolorname.ForeColor = pColorDialog.Color;
                lbcolorname.Text = pColorDialog.Color.Name;
                textColorname = pColorDialog.Color;
            }
        }

        private void cmdFonttime_Click(object sender, EventArgs e)
        {
            FontDialog pFontDialog = new FontDialog();
            pFontDialog.Font = textFontTime;
            if (pFontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                lbfonttime.Text = "字体：" + pFontDialog.Font.Name + "," + pFontDialog.Font.Size+"pt"; 
               // lbfonttime.Font = pFontDialog.Font;
                textFontTime = pFontDialog.Font;
            }
        }

        private void cmdColortime_Click(object sender, EventArgs e)
        {
            ColorDialog pColorDialog = new ColorDialog();
            pColorDialog.Color = textColortime;
            if (pColorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                lbcolortime.ForeColor = pColorDialog.Color;
                lbcolortime.Text = pColorDialog.Color.Name;
                textColortime = pColorDialog.Color;
            }
        }

        private void btPublishFont_Click(object sender, EventArgs e)
        {
            FontDialog pFontDialog = new FontDialog();
            pFontDialog.Font = textFontPublish;
            if (pFontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                lbPublishFont.Text = "字体：" + pFontDialog.Font.Name + "," + pFontDialog.Font.Size + "pt";
                textFontPublish = pFontDialog.Font;
            }
        }

        private void btPublishColor_Click(object sender, EventArgs e)
        {
            ColorDialog pColorDialog = new ColorDialog();
            pColorDialog.Color = textColorPublish;
            if (pColorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                lbPublishColor.ForeColor = pColorDialog.Color;
                lbPublishColor.Text = pColorDialog.Color.Name;
                textColorPublish = pColorDialog.Color;
            }
        }

        private void cbShadow_CheckedChanged(object sender, EventArgs e)
        {
            if (cbShadow.Checked)
            {
                textFontName = new System.Drawing.Font("方正宋黑简体", 80);
            }
        }

        private void btView_Click(object sender, EventArgs e)
        {
            if (!isClipGeoOut())
            {
                MessageBox.Show("请先生成图廓线！");
                return;
            }

            if (txt_CartoName.Text == "")
            {
                MessageBox.Show("请设置地图名称");
                return;
            }
               
            string NameContent = txt_CartoName.Text;

            pColor = ConvertColorToIColor(textColor);
            pFont = (stdole.IFontDisp)OLE.GetIFontDispFromFont(textFontName);
            //using (WaitOperation wo = m_Application.SetBusy())
            {

                bool MapNameHorizon = true;
                if (rb_MapNameHorizon.Checked)
                {
                    MapNameHorizon = true;
                }
                else
                {
                    MapNameHorizon = false;
                }

                MapLayoutHelperLH mh = new MapLayoutHelperLH();
                mh.MapNameSpace = double.Parse(txtWordSpace.Text);//图名字符间距
                mh.MapNameWidth = double.Parse(txtWordWidth.Text);
                mh.MapNameDis =  double.Parse(txtTopDis.Text);//图名图廓间距
                mh.ProducerDis = double.Parse(txtProducerDis.Text);//生产者与图廓间距
                mh.TimeDis = double.Parse(txtTimeDis.Text);//生产时间与图廓间距
                if (NameContent.Trim() != "")
                {
                    mh.CreateMapName(cbShadow.Checked, NameContent.Trim(), pFont, pColor, mh.MapNameDis,MapNameHorizon);
                }
                if (txt_InstitutionName.Text != "")
                {
                    mh.CreateMapProducer(txt_InstitutionName.Text.Trim(), (stdole.IFontDisp)OLE.GetIFontDispFromFont(textFontUnit), ConvertColorToIColor(textColorname), "生产商");
                }
                if (txt_Time.Text != "")
                {
                    mh.CreateMapProducer(txt_Time.Text.Trim(), (stdole.IFontDisp)OLE.GetIFontDispFromFont(textFontTime), ConvertColorToIColor(textColortime), "生产时间");
                }

                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = "TYPE = '出版形式'";
                mh.ClearFeatures(fclanno, qf);
                if (cbPublishType.Text != "")
                {
                    double dist = 0;
                    double.TryParse(tbPublishDist.Text, out dist);
                    mh.CreateMapPublishType(cbPublishType.Text, (stdole.IFontDisp)OLE.GetIFontDispFromFont(textFontPublish), ConvertColorToIColor(textColorPublish), dist, rbH.Checked);
                }

                m_Application.ActiveView.Refresh();
            }
            SaveParams();
            //MessageBox.Show("预览生成！");
        }


        private bool isClipGeoOut()
        {
            var lyr = m_Application.Workspace.LayerManager.GetLayer(l => (l is IFeatureLayer) && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == "LLINE").FirstOrDefault();

            IQueryFilter qf = new QueryFilterClass();

            IFeature fe;
            IFeatureCursor cursor = null;
            //图名
            qf.WhereClause = "TYPE = '内图廓'";
            cursor = (lyr as IFeatureLayer).Search(qf, false);

            fe = cursor.NextFeature();
            if (fe != null) 
            {
                return true;
            }
            return false;
        }


        private void btLast_Click(object sender, EventArgs e)
        {
            string planStr = @"专家库\经验方案\经验方案.xml";
            string fileName = m_Application.Template.Root + @"\" + planStr;
            LoadOutParams(fileName,true);
            #region 字体、颜色可视化
            lbfont.Text = "字体：" + textFontName.Name + "," + textFontName.Size + "pt";
            lbcolor.ForeColor = textColor;
            lbcolor.Text = textColor.Name;
            lbfontname.Text = "字体：" + textFontUnit.Name + "," + textFontUnit.Size + "pt";
            lbcolorname.ForeColor = textColorname;
            lbcolorname.Text = textColorname.Name;
            lbfonttime.Text = "字体：" + textFontTime.Name + "," + textFontTime.Size + "pt";
            lbcolortime.ForeColor = textColortime;
            lbcolortime.Text = textColortime.Name;
            #endregion
        }

    }
}
