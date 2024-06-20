using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using SMGI.Common;

namespace SMGI.Plugin.EmergencyMap
{

   

    public partial class LocationForm : Form
    {
        private GApplication _app;

        #region 属性
        /// <summary>
        /// 经度
        /// </summary>
        private double longitude;
        public double Longitude
        {
            get
            {
                return longitude;
            }

            set
            {
                longitude = value;

                int d = (int)longitude;
                double md = (longitude - d) * 60;
                int m = (int)md;
                int s = (int)((md - m) * 60);

                tbLonDu.Text = d.ToString();
                tbLonMin.Text = m.ToString();
                tbLonSec.Text = s.ToString();
            }
        }
        /// <summary>
        /// 纬度
        /// </summary>
        private double latitude;
        public double Latitude
        {
            get
            {
                return latitude;
            }
            set
            {
                latitude = value;

                int d = (int)latitude;
                double md = (latitude - d) * 60;
                int m = (int)md;
                int s = (int)((md - m) * 60);

                tbLatDu.Text = d.ToString();
                tbLatMin.Text = m.ToString();
                tbLatSec.Text = s.ToString();
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

        #endregion

        public LocationForm(GApplication app)
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
            for(int i = 0; i < pis.Count; ++i)
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

        

        #region 事件响应函数
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

            //经度验证
            double lonD, lonM, lonS;
            res = double.TryParse(tbLonDu.Text, out lonD);
            if (!res || lonD < -180 || lonD > 180)
            {
                MessageBox.Show("输入的经度不合法！");
                tbLonDu.Focus();

                return;
            }

            res = double.TryParse(tbLonMin.Text, out lonM);
            if (!res || lonM < 0 || lonM > 59)
            {
                MessageBox.Show("输入的经度不合法！");
                tbLonMin.Focus();

                return;
            }

            res = double.TryParse(tbLonSec.Text, out lonS);
            if (!res || lonS < 0 || lonS > 59)
            {
                MessageBox.Show("输入的经度不合法！");
                tbLonSec.Focus();

                return;
            }

            longitude = lonD + lonM / 60 + lonS / 3600;
            if (longitude < -180 || longitude > 180)
            {
                MessageBox.Show("输入的经度不合法！");

                return;
            }

            //纬度验证
            double latD, latM, latS;
            res = double.TryParse(tbLatDu.Text, out latD);
            if (!res || latD < -90 || latD > 90)
            {
                MessageBox.Show("输入的纬度不合法！");
                tbLatDu.Focus();

                return;
            }

            res = double.TryParse(tbLatMin.Text, out latM);
            if (!res || latM < 0 || latM > 59)
            {
                MessageBox.Show("输入的纬度不合法！");
                tbLatMin.Focus();

                return;
            }

            res = double.TryParse(tbLatSec.Text, out latS);
            if (!res || latS < 0 || latS > 59)
            {
                MessageBox.Show("输入的纬度不合法！");
                tbLatSec.Focus();

                return;
            }

            latitude = latD + latM / 60 + latS / 3600;
            if (latitude < -90 || latitude > 90)
            {
                MessageBox.Show("输入的纬度不合法！");

                return;
            }
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

            IPoint centerPoint = new PointClass { X = Longitude, Y = Latitude };//中心点
            PaperSize ps = ClipPaperSize;

            ISpatialReferenceFactory srcFactory = new SpatialReferenceEnvironmentClass();
            centerPoint.SpatialReference = srcFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCS3Type.esriSRGeoCS_Xian1980);//固定的???

            IGroupElement clipELe = ClipElement.createClipGroupElement(_app, centerPoint, ps.PaperWidth, ps.PaperHeight, RefScale);
            ClipElement.SetClipGroupElement(_app, clipELe);

            //更新配置表
            EnvironmentSettings.updateMapScale(_app, refScale);
            EnvironmentSettings.updatePageSize(_app, paperWidth, paperHeight);
            CommonMethods.clipEx = false;
           
        }
        #endregion

        private void cbPaperSize_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void cbRefSclae_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

  
    }
    /// <summary>
    /// 页面设置尺寸
    /// </summary>
    public class PaperSize
    {
        public double PaperWidth;//纸张宽度
        public double PaperHeight;//纸张高度
    }

}
