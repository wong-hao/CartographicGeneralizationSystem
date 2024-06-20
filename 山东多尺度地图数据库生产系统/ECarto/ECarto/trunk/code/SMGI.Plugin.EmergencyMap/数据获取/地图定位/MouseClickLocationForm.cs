using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using System.Xml.Linq;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class MouseClickLocationForm : Form
    {
        private GApplication _app;

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
       public IPoint centerPoint;
        public MouseClickLocationForm(GApplication app)
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
        }
        bool PaperMosueClick;
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

            DataRow itemSel = CommonLocationMethods.CaluateMapAndInlineSize(paperWidth, paperHeight);
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
                    e.Handled = true;   //小数点不能在第一位
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
                cbPaperSize.Text = "自定义尺寸";
        }

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
                cbPaperSize.Text = "自定义尺寸";
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            bool res;
         

            //比例尺
            double refScale;
            res = double.TryParse(cbRefSclae.Text, out refScale);
            if (!res || refScale <= 0)
            {
                MessageBox.Show("输入的比例尺不合法！");

                return;
            }

            //宽度
            double paperWidth;
            res = double.TryParse(tbPaperWidth.Text, out paperWidth);
            if (!res || paperWidth <= 0)
            {
                MessageBox.Show("输入的纸张宽度不合法！");

                return;
            }

            //高度
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
            CommonMethods.SetMapPar(paperWidth, paperHeight, MapSizeWidth, MapSizeHeight, InlineWidth, InlineHeight, InOutLineWidth, refScale);
            //更新配置表
            EnvironmentSettings.updateMapScale(_app, refScale);
            EnvironmentSettings.updatePageSize(_app, paperWidth, paperHeight);



            PaperSize ps = this.ClipPaperSize;
            #region 自动计算空间参考
            IMap pTempMap = new MapClass();
            CommonMethods.clipSpatialRefFileName = GApplication.ExePath + @"\..\Projection\JL_GCS2000_Lambert_Conformal_Conic.prj";
            var targetSpatialReference = CommonMethods.getClipSpatialRef();
            pTempMap.SpatialReference = targetSpatialReference;
            UnitConverterClass unitConverter = new UnitConverterClass();
            double width = unitConverter.ConvertUnits((CommonMethods.PaperWidth) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//内图廓宽度
            double height = unitConverter.ConvertUnits((CommonMethods.PaperHeight) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//内图廓宽度
            var ct = (centerPoint as IClone).Clone() as IPoint;
            (ct as IGeometry).Project(targetSpatialReference);//对裁切中心点进行投影变换
            //生成边框多边形
            double xMax = ct.X + width / 2;
            double yMax = ct.Y + height / 2;
            double xMin = ct.X - width / 2;
            double yMin = ct.Y - height / 2;
            IPoint pt1 = new PointClass { X = xMin, Y = yMin };
            IPoint pt2 = new PointClass { X = xMin, Y = yMax };
            IPoint pt3 = new PointClass { X = xMax, Y = yMax };
            IPoint pt4 = new PointClass { X = xMax, Y = yMin };

            IPointCollection pPtsCol = new PolygonClass();
            pPtsCol.AddPoint(pt1);
            pPtsCol.AddPoint(pt2);
            pPtsCol.AddPoint(pt3);
            pPtsCol.AddPoint(pt4);

            IPolygon ply = pPtsCol as IPolygon;
            ply.Close();
            ply.SpatialReference = targetSpatialReference;
            ply.Project(_app.MapControl.Map.SpatialReference);
            CommonMethods.CalculateSptialRef(ply.Envelope);
            #endregion
            IGroupElement clipELe = ClipElement.createClipGroupElement(_app, centerPoint, ps.PaperWidth, ps.PaperHeight, this.RefScale);
            ClipElement.SetClipGroupElement(_app, clipELe);
            CommonMethods.clipEx = false;
            this.Close();
            //DialogResult = DialogResult.OK;
        }

        private void btn_Preview_Click(object sender, EventArgs e)
        {
            bool res;


            //比例尺
            double refScale;
            res = double.TryParse(cbRefSclae.Text, out refScale);
            if (!res || refScale <= 0)
            {
                MessageBox.Show("输入的比例尺不合法！");

                return;
            }

            //宽度
            double paperWidth;
            res = double.TryParse(tbPaperWidth.Text, out paperWidth);
            if (!res || paperWidth <= 0)
            {
                MessageBox.Show("输入的纸张宽度不合法！");

                return;
            }

            //高度
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
            CommonMethods.SetMapPar(paperWidth, paperHeight, MapSizeWidth, MapSizeHeight, InlineWidth, InlineHeight, InOutLineWidth, refScale);
            //更新配置表
            EnvironmentSettings.updateMapScale(_app, refScale);
            EnvironmentSettings.updatePageSize(_app, paperWidth, paperHeight);



            PaperSize ps = this.ClipPaperSize;


           
            #region 自动计算空间参考
            IMap pTempMap = new MapClass();
           
            CommonMethods.clipSpatialRefFileName= GApplication.ExePath + @"\..\Projection\JL_GCS2000_Lambert_Conformal_Conic.prj";
            var targetSpatialReference = CommonMethods.getClipSpatialRef();
            pTempMap.SpatialReference = targetSpatialReference;
            UnitConverterClass unitConverter = new UnitConverterClass();
            double width = unitConverter.ConvertUnits((CommonMethods.PaperWidth) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//内图廓宽度
            double height = unitConverter.ConvertUnits((CommonMethods.PaperHeight) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//内图廓宽度
        
            //生成边框多边形
            var ct = (centerPoint as IClone).Clone() as IPoint;
            (ct as IGeometry).Project(targetSpatialReference);//对裁切中心点进行投影变换
            double xMax = ct.X + width / 2;
            double yMax = ct.Y + height / 2;
            double xMin = ct.X - width / 2;
            double yMin = ct.Y - height / 2;
            IPoint pt1 = new PointClass { X = xMin, Y = yMin };
            IPoint pt2 = new PointClass { X = xMin, Y = yMax };
            IPoint pt3 = new PointClass { X = xMax, Y = yMax };
            IPoint pt4 = new PointClass { X = xMax, Y = yMin };

            IPointCollection pPtsCol = new PolygonClass();
            pPtsCol.AddPoint(pt1);
            pPtsCol.AddPoint(pt2);
            pPtsCol.AddPoint(pt3);
            pPtsCol.AddPoint(pt4);

            IPolygon ply = pPtsCol as IPolygon;
            ply.Close();
            ply.SpatialReference = targetSpatialReference;
            ply.Project(_app.MapControl.Map.SpatialReference);
            CommonMethods.CalculateSptialRef(ply.Envelope);
            #endregion
            IGroupElement clipELe = ClipElement.createClipGroupElement(_app, centerPoint, ps.PaperWidth, ps.PaperHeight, this.RefScale);
            ClipElement.SetClipGroupElement(_app, clipELe);
            CommonMethods.clipEx = false;
        }

        private void cbRefSclae_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

       
    }
}
