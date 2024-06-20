using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using System.Xml.Linq;
using System.IO;
using ESRI.ArcGIS.esriSystem;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class ShpLocationForm : Form
    {
        private GApplication _app;
        public double Dx;//偏移
        public double Dy;//偏移
        public bool IsApply = false;//应用
        IGeometry shpPolygon = null;//shp范围文件
        public string ShapeFileName
        {
            get
            {
                return tbShpFileName.Text;
            }
        }

        /// <summary>
        /// 比例尺
        /// </summary>
        public double RefScale
        {
            get
            {
                return double.Parse(cbRefSclae.Text);
            }
        }

        /// <summary>
        /// 纸张尺寸
        /// </summary>
        public PaperSize ClipPaperSize
        {
            get
            {
                return new PaperSize { PaperWidth = double.Parse(tbPaperWidth.Text), PaperHeight = double.Parse(tbPaperHeight.Text) };

            }
        }

        bool _scaleChangedState;//由于比例尺的变化会触发纸张大小的更新，通过这个变量来保证该事件触发纸张的变换不会导致重新更新比例尺

        public ShpLocationForm(GApplication app)
        {
            InitializeComponent();

            _app = app;

            //根据环境设置，更新控件值
            var content = EnvironmentSettings.getContentElement(_app);
            var pagesize = content.Element("PageSize");
            var mapScale = content.Element("MapScale");

            cbRefSclae.Text = mapScale.Value;
            tbPaperWidth.Text = pagesize.Element("Width").Value;
            tbPaperHeight.Text = pagesize.Element("Height").Value;
            double width = Convert.ToDouble(tbPaperWidth.Text);
            double height = Convert.ToDouble(tbPaperHeight.Text);

            //纸张尺寸
            List<PaperInfo> pis = PageSizeRules.getPageSizeInfos(app);
            int index = -1;
            for (int i = 0; i < pis.Count; ++i)
            {
                cbPaperSize.Items.Add(pis[i]);
                if (pis[i].Width == width && pis[i].Height == height)
                {
                    index = i;
                }
            }

            cbPaperSize.SelectedIndex = index;
            if (-1 == index)
            {
                cbPaperSize.Text = "自定义尺寸";
            }

            //比例尺
            List<string> scaleList = EnvironmentSettings.getMapScaleFromExpertiseConfig();
            if (scaleList.Count > 0)
            {
                cbRefSclae.Items.AddRange(scaleList.ToArray());

                cbRefSclae.SelectedIndex = 0;
            }

            _scaleChangedState = false;
        }

        private void btnShpFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "选择一个范围文件";
            dlg.AddExtension = true;
            dlg.DefaultExt = "shp";
            dlg.Filter = "选择文件|*.shp";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                tbShpFileName.Text = dlg.FileName;
                IGeometry polygon = GetShpGeometry(tbShpFileName.Text);
                if (polygon == null)
                {
                    tbShpFileName.Text = "";
                    MessageBox.Show("不存在范围文件！");
                    return;
                }
                shpPolygon = polygon;
                bool res=false;
                //内图廓宽度
                double InlineWidth;
                res = double.TryParse(tbInlineWidth.Text, out InlineWidth);
                if (!res || InlineWidth <= 0)
                {
                    MessageBox.Show("输入的内图廓尺寸宽度不合法！");
                    return;
                }

                //内图廓高度
                double InlineHeight;
                res = double.TryParse(tbInlineHeight.Text, out InlineHeight);
                if (!res || InlineHeight <= 0)
                {
                    MessageBox.Show("输入的内图廓尺寸高度不合法！");
                    return;
                }
                double scale= CommonLocationMethods.CaluateMapScale(InlineWidth, InlineHeight, polygon);
                cbRefSclae.Text = scale.ToString();
                //自动计算空间参考
                CommonMethods.CalculateSptialRef(polygon.Envelope);
            }
        }
       


        private  XElement getMapInnerElement()
        {

            string fileName = _app.Template.Root + @"\专家库\MapMargin.xml";

            XDocument doc = XDocument.Load(fileName);
           
            {
                
                return doc.Element("MapMargin").Element("Content").Element("MapInner");
            }
        }
        IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
        private IPolygon GetShpGeometry(string fileName)
        {
           
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(System.IO.Path.GetDirectoryName(fileName), 0);
            IFeatureWorkspace pFeatureWorkspace = pWorkspace as IFeatureWorkspace;
            IFeatureClass shapeFC = pFeatureWorkspace.OpenFeatureClass(System.IO.Path.GetFileName(fileName));

            //是否为多边形几何体
            if (shapeFC.ShapeType != esriGeometryType.esriGeometryPolygon)
            {
                MessageBox.Show("范围文件应为多边形几何体，请重新指定范围文件！");
                return null;
            }

            //空间参考是否一致
            ISpatialReference shapeSpatialreference = ((shapeFC as IDataset) as IGeoDataset).SpatialReference;
            if (null == shapeSpatialreference)
            {
                MessageBox.Show("范围文件没有定义空间参考！");
                return null;
            }

            //默认为第一个要素的几何体
            IPolygon result = null;
            IFeatureCursor featureCursor = shapeFC.Search(null, false);
            IFeature pFeature = featureCursor.NextFeature();
            if (pFeature == null)
            {
                MessageBox.Show("范围文件不存在多边形几何体，请重新指定范围文件！");
                return null;
            }
            if (pFeature != null && pFeature.Shape is IPolygon)
            {
                result = pFeature.Shape as IPolygon;
                if (_app.MapControl.Map.SpatialReference.Name != shapeSpatialreference.Name)
                {
                    result.Project(_app.MapControl.Map.SpatialReference);
                }
            }
            Marshal.ReleaseComObject(featureCursor);


            //设置目标空间坐标系
            ISpatialReference targetSpatialReference = CommonMethods.getClipSpatialRef();
            if (null == targetSpatialReference)
                targetSpatialReference = _app.MapControl.Map.SpatialReference;
             

            if (result.SpatialReference.Name != targetSpatialReference.Name)
            {
                (result as IGeometry).Project(targetSpatialReference);//对定位几何进行投影变换
            }
            //释放内存
            Marshal.ReleaseComObject(pFeatureWorkspace);
            Marshal.ReleaseComObject(shapeFC);
            return result;
        }
        private void cbPaperSize_SelectedIndexChanged(object sender, EventArgs e)
        {
           
            PaperInfo pi = cbPaperSize.SelectedItem as PaperInfo;
            tbPaperWidth.Text = pi.Width.ToString();
            tbPaperHeight.Text = pi.Height.ToString();

            //宽度
            double paperWidth;
            bool res = double.TryParse(tbPaperWidth.Text, out paperWidth);
            if (!res || paperWidth <= 0)
            {
                MessageBox.Show("纸张宽度不合法!");
            }
            //高度
            double paperHeight;
            res = double.TryParse(tbPaperHeight.Text, out paperHeight);
            if (!res || paperHeight <= 0)
            {
                MessageBox.Show("纸张高度不合法!");
            }

          DataRow itemSel=CommonLocationMethods.CaluateMapAndInlineSize(paperWidth,paperHeight);
          #region 成图尺寸、内图廓尺寸、内外图廓间距写入textbox
          if (itemSel != null)
          {
              //横版
              if (paperWidth > paperHeight)
              {
                  tbMapSizeWidth.Text = itemSel["成图尺寸一边"].ToString();
                  tbMapSizeHeight.Text = itemSel["成图尺寸另一边"].ToString();
                  tbInlineWidth.Text = itemSel["横版内图廓尺寸宽"].ToString();
                  tbInlineHeight.Text = itemSel["横版内图廓尺寸高"].ToString();

              }
              //竖版
              else
              {
                  tbMapSizeHeight.Text = itemSel["成图尺寸一边"].ToString();
                  tbMapSizeWidth.Text = itemSel["成图尺寸另一边"].ToString();
                  tbInlineWidth.Text = itemSel["竖版内图廓尺寸宽"].ToString();
                  tbInlineHeight.Text = itemSel["竖版内图廓尺寸高"].ToString();

              }
              tbInOutWidth.Text = itemSel["内外图廓间距"].ToString();

          }
          #endregion
          if (!PaperMosueClick) return;
          if (tbShpFileName.Text == "") return;
          IGeometry polygon = GetShpGeometry(tbShpFileName.Text);
          if (polygon == null)
          {
              tbShpFileName.Text = "";
              MessageBox.Show("不存在范围文件！");
              return;
          }
          shpPolygon = polygon;

          //内图廓宽度
          double InlineWidth;
          res = double.TryParse(tbInlineWidth.Text, out InlineWidth);
          if (!res || InlineWidth <= 0)
          {
              MessageBox.Show("输入的内图廓尺寸宽度不合法！");
              return;
          }

          //内图廓高度
          double InlineHeight;
          res = double.TryParse(tbInlineHeight.Text, out InlineHeight);
          if (!res || InlineHeight <= 0)
          {
              MessageBox.Show("输入的内图廓尺寸高度不合法！");
              return;
          }

          if (!_scaleChangedState)
          {
              double scale = CommonLocationMethods.CaluateMapScale(InlineWidth, InlineHeight, polygon);
              cbRefSclae.Text = scale.ToString();
          }
          //重置
          _scaleChangedState = false;

          PaperMosueClick = false;
         

        }

        private void tbWidth_KeyPress(object sender, KeyPressEventArgs e)
        {
            //判断按键是不是要输入的类型。
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8 && (int)e.KeyChar != 46)
                e.Handled = true;

            //小数点的处理。
            if ((int)e.KeyChar == 46) //小数点
            {
                if ((sender as TextBox).Text.Length <= 0)
                {
                    e.Handled = true; //小数点不能在第一位
                }
                else
                {
                    float f;
                    float oldf;
                    bool b1 = false, b2 = false;
                    b1 = float.TryParse((sender as TextBox).Text, out oldf);
                    b2 = float.TryParse((sender as TextBox).Text + e.KeyChar.ToString(), out f);
                    if (b2 == false)
                    {
                        if (b1 == true)
                            e.Handled = true;
                        else
                            e.Handled = false;
                    }
                }
            }

            if (!e.Handled)
            {
                cbPaperSize.Text = "自定义尺寸";
                return;
               
            }
        }
        //纸张
        private void tbHeight_KeyPress(object sender, KeyPressEventArgs e)
        {
           
            //判断按键是不是要输入的类型。
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8 && (int)e.KeyChar != 46)
                e.Handled = true;

            //小数点的处理。
            if ((int)e.KeyChar == 46) //小数点
            {
                if ((sender as TextBox).Text.Length <= 0)
                {
                    e.Handled = true; //小数点不能在第一位
                }
                else
                {
                    float f;
                    float oldf;
                    bool b1 = false, b2 = false;
                    b1 = float.TryParse((sender as TextBox).Text, out oldf);
                    b2 = float.TryParse((sender as TextBox).Text + e.KeyChar.ToString(), out f);
                    if (b2 == false)
                    {
                        if (b1 == true)
                            e.Handled = true;
                        else
                            e.Handled = false;
                    }
                }
            }

            if (!e.Handled)
            {
                cbPaperSize.Text = "自定义尺寸";
                if (tbShpFileName.Text == "") return;
                IGeometry polygon = GetShpGeometry(tbShpFileName.Text);
                if (polygon == null)
                {
                    tbShpFileName.Text = "";
                    MessageBox.Show("不存在范围文件！");
                    return;
                }
                shpPolygon = polygon;

                //内图廓宽度
                double InlineWidth;
                bool res;
                res = double.TryParse(tbInlineWidth.Text, out InlineWidth);
                if (!res || InlineWidth <= 0)
                {
                    MessageBox.Show("输入的内图廓尺寸宽度不合法！");
                    return;
                }

                //内图廓高度
                double InlineHeight;
                res = double.TryParse(tbInlineHeight.Text, out InlineHeight);
                if (!res || InlineHeight <= 0)
                {
                    MessageBox.Show("输入的内图廓尺寸高度不合法！");
                    return;
                }

             double scale=CommonLocationMethods.CaluateMapScale(InlineWidth, InlineHeight, polygon);
             cbRefSclae.Text = scale.ToString();
                return;
                
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            m_Timer.Stop();
            if (string.IsNullOrEmpty(tbShpFileName.Text))
            {
                MessageBox.Show("请指定定位范围文件！");
                return;
            }

            bool res;

            //比例尺
            double refScale;
            res = double.TryParse(cbRefSclae.Text, out refScale);
            if (!res || refScale <= 0)
            {
                MessageBox.Show("输入的比例尺不合法！");

                return;
            }

            //纸张宽度
            double paperWidth;
            res = double.TryParse(tbPaperWidth.Text, out paperWidth);
            if (!res || paperWidth <= 0)
            {
                MessageBox.Show("输入的宽度不合法！");

                return;
            }

            //纸张高度
            double paperHeight;
            res = double.TryParse(tbPaperHeight.Text, out paperHeight);
            if (!res || paperHeight <= 0)
            {
                MessageBox.Show("输入的高度不合法！");

                return;
            }

            //成图尺寸宽度
            double MapSizeWidth;
            res = double.TryParse(tbMapSizeWidth.Text, out MapSizeWidth);
            if (!res || MapSizeWidth <= 0)
            {
                MessageBox.Show("输入的成图尺寸宽度不合法！");
                return;
            }
            //成图尺寸高度
            double MapSizeHeight;
            res = double.TryParse(tbMapSizeHeight.Text, out MapSizeHeight);
            if (!res || MapSizeHeight <= 0)
            {
                MessageBox.Show("输入的成图尺寸高度不合法！");
                return;
            }

            //内图廓宽度
            double InlineWidth;
            res = double.TryParse(tbInlineWidth.Text, out InlineWidth);
            if (!res || InlineWidth <= 0)
            {
                MessageBox.Show("输入的内图廓尺寸宽度不合法！");
                return;
            }

            //内图廓高度
            double InlineHeight;
            res = double.TryParse(tbInlineHeight.Text, out InlineHeight);
            if (!res || InlineHeight <= 0)
            {
                MessageBox.Show("输入的内图廓尺寸高度不合法！");
                return;
            }

            //内外图廓间距
            double InOutLineWidth;
            res = double.TryParse(tbInOutWidth.Text, out InOutLineWidth);
            if (!res || InOutLineWidth < 0)
            {
                MessageBox.Show("输入的内图廓尺寸高度不合法！");
                return;
            }
            if (paperWidth < MapSizeWidth || paperHeight < MapSizeHeight)
            {
                MessageBox.Show("纸张尺寸不能小于成图尺寸！");
                return;
            }
            if (InlineWidth + InOutLineWidth > MapSizeWidth || InlineHeight + InOutLineWidth > MapSizeHeight)
            {
                MessageBox.Show("成图尺寸不能小于外图廓尺寸！");
                return;
            }


            //存储地图各尺寸
            CommonMethods.SetMapPar(paperWidth, paperHeight, MapSizeWidth, MapSizeHeight, InlineWidth, InlineHeight, InOutLineWidth, refScale);
            //更新配置表
            EnvironmentSettings.updateMapScale(_app, refScale);
            EnvironmentSettings.updatePageSize(_app, paperWidth, paperHeight);

            IPolygon ply = getClipPolygonByShapeFile(ShapeFileName);
            if (ply == null)
            {
                return;
            }
            double dx = Dx;
            double dy = Dy;

            IGroupElement clipELe = ClipElement.createClipGroupElementEx2(_app, ply, InlineWidth, InlineHeight, RefScale, dx, dy);
            ClipElement.SetClipGroupElement(_app, clipELe);
            CommonMethods.clipEx = true;
            this.Close();
        }

        private void ShpLocationForm_Load(object sender, EventArgs e)
        {

        }
        //出图比例尺更改->计算相应的纸张尺寸
        bool scaleChange = false;
        bool ChangePage = false, ChangeScale=false;
        //根据比例尺修改纸张
        private void ModifyPageByScale()
        {
            
           
            if (string.IsNullOrEmpty(tbShpFileName.Text))
            {
                return;
            }
            bool res=false;
            double scale;
            res = double.TryParse(cbRefSclae.Text,out scale);
            if (!res||scale<=0)
            {
                return;
            }
            if (shpPolygon == null)
                return;
            IGeometry polygon = shpPolygon;

            double width = polygon.Envelope.Width;
            double height = polygon.Envelope.Height;

            double mapwidth = width * 1.1 / scale * 1000;//毫米,裁切面扩展1.1倍
            double mapheight = height * 1.1 / scale * 1000;//毫米，裁切面扩展1.1倍
            Dictionary<int, PaperInfo> infoDic = new Dictionary<int, PaperInfo>();
            string keyword = "横";
            if (mapwidth < mapheight) //纵向
            {
                keyword = "纵";
            }
            for (int i = 0; i < cbPaperSize.Items.Count; i++)
            {
                PaperInfo p = cbPaperSize.Items[i] as PaperInfo;
                if (p.ToString().IndexOf(keyword) != -1)
                {
                    infoDic[i] = p;
                }
            }
            var list = infoDic.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                PaperInfo p = list[i].Value as PaperInfo;
                int inext = i < list.Count - 1 ? i + 1 : list.Count - 1;
                PaperInfo p1 = list[inext].Value as PaperInfo;
                if (mapwidth > mapheight) //横版
                {
                    DataRow itemSel =CommonLocationMethods.CaluateMapAndInlineSize(p.Width, p.Height);
                    DataRow itemSel1 =CommonLocationMethods.CaluateMapAndInlineSize(p1.Width, p1.Height);
                    double inlineWidth = 0, inlineHeight = 0, inlineWidth1 = 0, inlineHeight1 = 0;
                    #region 计算内图廓尺寸
                    if (itemSel != null)
                    {
                        //横版
                        if (p.Width > p.Height)
                        {

                            inlineWidth = double.Parse(itemSel["横版内图廓尺寸宽"].ToString());
                            inlineHeight = double.Parse(itemSel["横版内图廓尺寸高"].ToString());

                        }
                        //竖版
                        else
                        {
                            inlineWidth = double.Parse(itemSel["竖版内图廓尺寸宽"].ToString());
                            inlineHeight = double.Parse(itemSel["竖版内图廓尺寸高"].ToString());
                        }
                        //横版
                        if (p1.Width > p1.Height)
                        {

                            inlineWidth1 = double.Parse(itemSel1["横版内图廓尺寸宽"].ToString());
                            inlineHeight1 = double.Parse(itemSel1["横版内图廓尺寸高"].ToString());

                        }
                        //竖版
                        else
                        {
                            inlineWidth1 = double.Parse(itemSel1["竖版内图廓尺寸宽"].ToString());
                            inlineHeight1 = double.Parse(itemSel1["竖版内图廓尺寸高"].ToString());
                        }
                    }
                    #endregion
                    //匹配到内图廓尺寸则按内图廓计算，否则按纸张计算
                    if (itemSel != null && itemSel1 != null)
                    {
                        if (inlineWidth - mapwidth > 0 && inlineWidth1 - mapwidth < 0)
                        {
                            _scaleChangedState = true;
                            //double a = p.Height - mapheight; double b = p1.Height - mapheight;
                            cbPaperSize.SelectedIndex = list[i].Key;
                            break;
                        }
                        //最后一个：最小出图尺寸
                        if (i == list.Count - 1)
                        {
                            if (inlineWidth - mapwidth > 0)
                            {
                                _scaleChangedState = true;
                                cbPaperSize.SelectedIndex = list[i].Key;
                                break;
                            }
                        }

                    }
                    else
                    {
                        if (p.Width - mapwidth > 0 && p1.Width - mapwidth < 0)
                        {
                            _scaleChangedState = true;
                            //double a = p.Height - mapheight; double b = p1.Height - mapheight;
                            cbPaperSize.SelectedIndex = list[i].Key;
                            break;
                        }
                        //最后一个：最小出图尺寸
                        if (i == list.Count - 1)
                        {
                            if (p.Width - mapwidth > 0)
                            {
                                _scaleChangedState = true;
                                cbPaperSize.SelectedIndex = list[i].Key;
                                break;
                            }
                        }
                    }
                }
                else//纵版
                {
                    DataRow itemSel =CommonLocationMethods.CaluateMapAndInlineSize(p.Width, p.Height);
                    DataRow itemSel1 = CommonLocationMethods.CaluateMapAndInlineSize(p1.Width, p1.Height);
                    double inlineWidth = 0, inlineHeight = 0, inlineWidth1 = 0, inlineHeight1 = 0;
                    #region 计算内图廓尺寸
                    if (itemSel != null)
                    {
                        //横版
                        if (p.Width > p.Height)
                        {

                            inlineWidth = double.Parse(itemSel["横版内图廓尺寸宽"].ToString());
                            inlineHeight = double.Parse(itemSel["横版内图廓尺寸高"].ToString());

                        }
                        //竖版
                        else
                        {
                            inlineWidth = double.Parse(itemSel["竖版内图廓尺寸宽"].ToString());
                            inlineHeight = double.Parse(itemSel["竖版内图廓尺寸高"].ToString());
                        }
                        //横版
                        if (p1.Width > p1.Height)
                        {

                            inlineWidth1 = double.Parse(itemSel1["横版内图廓尺寸宽"].ToString());
                            inlineHeight1 = double.Parse(itemSel1["横版内图廓尺寸高"].ToString());

                        }
                        //竖版
                        else
                        {
                            inlineWidth1 = double.Parse(itemSel1["竖版内图廓尺寸宽"].ToString());
                            inlineHeight1 = double.Parse(itemSel1["竖版内图廓尺寸高"].ToString());
                        }
                    }
                    #endregion
                    //匹配到内图廓尺寸则按内图廓计算，否则按纸张计算
                    if (itemSel != null && itemSel1 != null)
                    {
                        if (inlineHeight - mapheight > 0 && inlineHeight1 - mapheight < 0)
                        {
                            _scaleChangedState = true;
                            //double a = p.Height - mapheight; double b = p1.Height - mapheight;
                            cbPaperSize.SelectedIndex = list[i].Key;
                            break;
                        }
                        //最后一个：最小出图尺寸
                        if (i == list.Count - 1)
                        {
                            if (inlineHeight - mapheight > 0)
                            {
                                _scaleChangedState = true;
                                cbPaperSize.SelectedIndex = list[i].Key;
                                break;
                            }
                        }

                    }
                    else
                    {
                        if (p.Height - mapheight > 0 && p1.Height - mapheight < 0)
                        {
                            _scaleChangedState = true;
                            cbPaperSize.SelectedIndex = list[i].Key;
                            break;
                        }
                        //最后一个：最小出图尺寸
                        if (i == list.Count - 1)
                        {
                            if (p.Height - mapheight > 0)
                            {
                                _scaleChangedState = true;
                                cbPaperSize.SelectedIndex = list[i].Key;
                                break;
                            }
                        }
                    }
                }
            }
        }
        private void cbRefSclae_SelectedIndexChanged(object sender, EventArgs e)
        {
            ModifyPageByScale();    

        }

        private void btn_Preview_Click(object sender, EventArgs e)
        {
            m_Timer.Stop();
            if (string.IsNullOrEmpty(tbShpFileName.Text))
            {
                MessageBox.Show("请指定定位范围文件！");
                return;
            }

            bool res;

            //比例尺
            double refScale;
            res = double.TryParse(cbRefSclae.Text, out refScale);
            if (!res || refScale <= 0)
            {
                MessageBox.Show("输入的比例尺不合法！");

                return;
            }

            //纸张宽度
            double paperWidth;
            res = double.TryParse(tbPaperWidth.Text, out paperWidth);
            if (!res || paperWidth <= 0)
            {
                MessageBox.Show("输入的纸张宽度不合法！");

                return;
            }

            //纸张高度
            double paperHeight;
            res = double.TryParse(tbPaperHeight.Text, out paperHeight);
            if (!res || paperHeight <= 0)
            {
                MessageBox.Show("输入的纸张高度不合法！");

                return;
            }

            //成图尺寸宽度
            double MapSizeWidth;
            res = double.TryParse(tbMapSizeWidth.Text, out MapSizeWidth);
            if (!res || MapSizeWidth <= 0)
            {
                MessageBox.Show("输入的成图尺寸宽度不合法！");
                return;
            }
            //成图尺寸高度
            double MapSizeHeight;
            res = double.TryParse(tbMapSizeHeight.Text, out MapSizeHeight);
            if (!res || MapSizeHeight <= 0)
            {
                MessageBox.Show("输入的成图尺寸高度不合法！");
                return;
            }

            //内图廓宽度
            double InlineWidth;
            res = double.TryParse(tbInlineWidth.Text, out InlineWidth);
            if (!res || InlineWidth <= 0)
            {
                MessageBox.Show("输入的内图廓尺寸宽度不合法！");
                return;
            }

            //内图廓高度
            double InlineHeight;
            res = double.TryParse(tbInlineHeight.Text, out InlineHeight);
            if (!res || InlineHeight <= 0)
            {
                MessageBox.Show("输入的内图廓尺寸高度不合法！");
                return;
            }

            //内外图廓间距
            double InOutLineWidth;
            res = double.TryParse(tbInOutWidth.Text, out InOutLineWidth);
            if (!res || InOutLineWidth < 0)
            {
                MessageBox.Show("输入的内图廓尺寸高度不合法！");
                return;
            }
            if (paperWidth < MapSizeWidth || paperHeight < MapSizeHeight)
            {
                MessageBox.Show("纸张尺寸不能小于成图尺寸！");
                return;
            }
            if (InlineWidth + InOutLineWidth > MapSizeWidth || InlineHeight + InOutLineWidth > MapSizeHeight)
            {
                MessageBox.Show("成图尺寸不能小于外图廓尺寸！");
                return;
            }
  

            //存储地图各尺寸
            CommonMethods.SetMapPar(paperWidth, paperHeight, MapSizeWidth, MapSizeHeight, InlineWidth, InlineHeight, InOutLineWidth,refScale);
            //更新配置表
            EnvironmentSettings.updateMapScale(_app, refScale);
            EnvironmentSettings.updatePageSize(_app, paperWidth, paperHeight);

            IPolygon ply = getClipPolygonByShapeFile(ShapeFileName);
            if (ply == null)
            {
                return;
            }
            double dx = Dx;
            double dy = Dy;

            IGroupElement clipELe = ClipElement.createClipGroupElementEx2(_app, ply, InlineWidth, InlineHeight, RefScale, dx, dy);
            ClipElement.SetClipGroupElement(_app, clipELe);
            CommonMethods.clipEx = true;
        }

        //读取shp文件,获取裁切几何体
        private IPolygon getClipPolygonByShapeFile(string fileName)
        {
            IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(System.IO.Path.GetDirectoryName(fileName), 0);
            IFeatureWorkspace pFeatureWorkspace = pWorkspace as IFeatureWorkspace;
            IFeatureClass shapeFC = pFeatureWorkspace.OpenFeatureClass(System.IO.Path.GetFileName(fileName));

            //是否为多边形几何体
            if (shapeFC.ShapeType != esriGeometryType.esriGeometryPolygon)
            {
                MessageBox.Show("范围文件应为多边形几何体，请重新指定范围文件！");
                return null;
            }

            //空间参考是否一致
            ISpatialReference shapeSpatialreference = ((shapeFC as IDataset) as IGeoDataset).SpatialReference;
            if (null == shapeSpatialreference)
            {
                MessageBox.Show("范围文件没有定义空间参考！");
                return null;
            }

            //默认为第一个要素的几何体
            IPolygon result = null;
            IFeatureCursor featureCursor = shapeFC.Search(null, false);
            IFeature pFeature = featureCursor.NextFeature();
            if (pFeature != null && pFeature.Shape is IPolygon)
            {
                result = pFeature.Shape as IPolygon;
                if (_app.MapControl.Map.SpatialReference.Name != shapeSpatialreference.Name)
                {
                    result.Project(_app.MapControl.Map.SpatialReference);
                }
            }
            Marshal.ReleaseComObject(featureCursor);
            //释放内存
            Marshal.ReleaseComObject(pFeatureWorkspace);
            Marshal.ReleaseComObject(shapeFC);
            return result;
        }

        private void tbInlineWidth_TextChanged(object sender, EventArgs e)
        {
             
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        Timer m_Timer=new Timer();

        private void cbRefSclae_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                m_Timer.Stop();
                ModifyPageByScale();
            }
          
            //判断按键是不是要输入的类型。
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8 && (int)e.KeyChar != 46)
                e.Handled = true;

            //小数点的处理。
            if ((int)e.KeyChar == 46) //小数点
            {
                if ((sender as TextBox).Text.Length <= 0)
                {
                    e.Handled = true; //小数点不能在第一位
                }
                else
                {
                    float f;
                    float oldf;
                    bool b1 = false, b2 = false;
                    b1 = float.TryParse((sender as TextBox).Text, out oldf);
                    b2 = float.TryParse((sender as TextBox).Text + e.KeyChar.ToString(), out f);
                    if (b2 == false)
                    {
                        if (b1 == true)
                            e.Handled = true;
                        else
                            e.Handled = false;
                    }
                }
            }

        }
        private void cbRefSclae_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ModifyPageByScale();
            }
        }
        bool PaperMosueClick = false;
        private void cbPaperSize_MouseClick(object sender, MouseEventArgs e)
        {
            PaperMosueClick = true;
        }

        
            


    }
}
