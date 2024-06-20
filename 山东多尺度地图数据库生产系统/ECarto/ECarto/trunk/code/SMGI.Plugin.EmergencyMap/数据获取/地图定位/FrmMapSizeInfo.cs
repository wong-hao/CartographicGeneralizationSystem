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
    public partial class FrmMapSizeInfo : Form
    {
        private GApplication _app;

       

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
       public IPoint centerPoint=null;
       public IPoint centerPoint0 = null;
       public double refScale = 1;
       bool Intial = true;
       public FrmMapSizeInfo(GApplication app)
        {
            InitializeComponent();

            _app = app;

            //根据环境设置，更新控件值
            var content = EnvironmentSettings.getContentElement(_app);
            var pagesize = content.Element("PageSize");
            var mapScale = content.Element("MapScale");


            refScale = double.Parse(mapScale.Value);
         
            tbPaperWidth.Text = pagesize.Element("Width").Value;
            tbPaperHeight.Text = pagesize.Element("Height").Value;
            double width = Convert.ToDouble(tbPaperWidth.Text);
            double height = Convert.ToDouble(tbPaperHeight.Text);

            //纸张尺寸
            this.tbPaperHeight.Text = CommonMethods.PaperHeight.ToString();
            this.tbPaperWidth.Text = CommonMethods.PaperWidth.ToString();
            //成图尺寸
            this.tbMapSizeHeight.Text = CommonMethods.MapSizeHeight.ToString();
            this.tbMapSizeWidth.Text = CommonMethods.MapSizeWidth.ToString();

            
            //外图廓
            this.tbOutlineHeight.Text = (CommonMethods.InlineHeight+2* CommonMethods.InOutLineWidth).ToString();
            this.tbOutlineWidth.Text = (CommonMethods.InlineWidth + 2 * CommonMethods.InOutLineWidth).ToString();
            //内图廓
            this.tbInlineHeight.Text = CommonMethods.InlineHeight.ToString();
            this.tbInlineWidth.Text = CommonMethods.InlineWidth.ToString();

            //内外间距
            this.tbInOutInterval.Value =Math.Round((decimal) CommonMethods.InOutLineWidth,1);
            //上间距
            this.tbMapTop.Value =Math.Round( (decimal)(CommonMethods.MapSizeTopInterval),1);
            //下间距
            this.tbMapDown.Value = Math.Round((decimal)(CommonMethods.MapSizeDownInterval),1);
           //水平间距
            this.tbMapHorial.Value = Math.Round((decimal)((CommonMethods.MapSizeWidth - double.Parse(this.tbOutlineWidth.Text)) * 0.5),1);
            Intial = false;
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

        
        }

        private void btOK_Click(object sender, EventArgs e)
        {

            UpdateParms();
            ClipElement.UpdateClipGroupElement(centerPoint, refScale);
            //纸张尺寸
            CommonMethods.PaperHeight= MapSizeInfoClass.PaperHeight ;
             CommonMethods.PaperWidth=MapSizeInfoClass.PaperWidth ;
            //成图尺寸
            CommonMethods.MapSizeHeight= MapSizeInfoClass.MapSizeHeight ;
            CommonMethods.MapSizeWidth = MapSizeInfoClass.MapSizeWidth;
            //内图廓
            CommonMethods.InlineHeight= MapSizeInfoClass.InlineHeight ;
            CommonMethods.InlineWidth = MapSizeInfoClass.InlineWidth;
            //内外间距
            CommonMethods.InOutLineWidth = MapSizeInfoClass.InOutLineWidth;
            //上间距
            CommonMethods.MapSizeTopInterval=MapSizeInfoClass.MapSizeTopInterval ;
            //下间距
            CommonMethods.MapSizeDownInterval= MapSizeInfoClass.MapSizeDownInterval;
            this.Close();
            
        }

        private void btn_Preview_Click(object sender, EventArgs e)
        {
            
        }

        private void cbRefSclae_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void MouseClickLocationForm_Load(object sender, EventArgs e)
        {
            IGroupElement ge = ClipElement.getClipGroupElement(GApplication.Application.MapControl.ActiveView.GraphicsContainer);
            if (ge == null)
                return;
            List<IElement> txtEles = new List<IElement>();
            for (int i = 0; i < ge.ElementCount; i++)
            {
                IElement el = ge.get_Element(i);
                IElementProperties3 eleProp = el as IElementProperties3;

                if ("内图廓" == eleProp.Name)
                {
                    centerPoint = (el.Geometry as IArea).Centroid;
                    centerPoint0 = (centerPoint as IClone).Clone() as IPoint;
                }
                if ("裁切范围" == eleProp.Name)
                {
                    pageEn = el.Geometry.Envelope;
                }
                
            }
        }
       
        //内外图廓间距
        private void tbInOutWidth_ValueChanged(object sender, EventArgs e)
        {
            if (Intial)
                return;
            //修改内图廓
            this.tbInlineHeight.Text = (decimal.Parse(this.tbOutlineHeight.Text) - 2 * this.tbInOutInterval.Value).ToString();
            this.tbInlineWidth.Text = (decimal.Parse(this.tbOutlineWidth.Text) - 2 * this.tbInOutInterval.Value).ToString();
            UpdateParms();
            ClipElement.UpdateClipGroupElement(centerPoint, refScale);
        }
        //左右间距
        private void tbMapHorial_ValueChanged(object sender, EventArgs e)
        {
            if (Intial)
                return;
            //外图廓宽度变换-内外间距不变        
            this.tbOutlineWidth.Text = (decimal.Parse(this.tbMapSizeWidth.Text) - 2 * this.tbMapHorial.Value).ToString();
            //内图廓高度变换-内外间距不变
            this.tbInlineWidth.Text = (decimal.Parse(this.tbOutlineWidth.Text) - 2 * this.tbInOutInterval.Value).ToString();
            UpdateParms();
            ClipElement.UpdateClipGroupElement(centerPoint, refScale);
        }
        //上下间距
        private void tbMapDown_ValueChanged(object sender, EventArgs e)
        {
            if (Intial)
                return;
            //外图廓高度变换-内外间距不变
            this.tbOutlineHeight.Text = (decimal.Parse(this.tbMapSizeHeight.Text) -  this.tbMapTop.Value-this.tbMapDown.Value).ToString();
            //内图廓高度变换-内外间距不变
            this.tbInlineHeight.Text = (decimal.Parse(this.tbOutlineHeight.Text) - 2*this.tbInOutInterval.Value).ToString();
            MapCenterChange();
            UpdateParms();
            ClipElement.UpdateClipGroupElement(centerPoint, refScale);
        }
        private void tbMapTop_ValueChanged(object sender, EventArgs e)
        {
            if (Intial)
                return;
            //外图廓高度变换-内外间距不变
            this.tbOutlineHeight.Text = (decimal.Parse(this.tbMapSizeHeight.Text) - this.tbMapTop.Value - this.tbMapDown.Value).ToString();
            //内图廓高度变换-内外间距不变
            this.tbInlineHeight.Text = (decimal.Parse(this.tbOutlineHeight.Text) - 2 * this.tbInOutInterval.Value).ToString();
            MapCenterChange();
            UpdateParms();
            ClipElement.UpdateClipGroupElement(centerPoint, refScale);
        }

        IEnvelope pageEn = new EnvelopeClass();
        private void MapCenterChange()
        {
          ISpatialReference targetSpatialReference = CommonMethods.getClipSpatialRef();
          if (null == targetSpatialReference)
              targetSpatialReference = GApplication.Application.MapControl.Map.SpatialReference;
          //在目标空间参考系中生成裁切元素
          IMap pTempMap = new MapClass();
          pTempMap.SpatialReference = targetSpatialReference;
          UnitConverterClass unitConverter = new UnitConverterClass();
          double top=  unitConverter.ConvertUnits((CommonMethods.MapSizeTopInterval) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;
          double down = unitConverter.ConvertUnits((CommonMethods.MapSizeDownInterval) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;
          double HSpace = unitConverter.ConvertUnits((CommonMethods.MapSizeWidth - CommonMethods.InlineWidth)*0.5 / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;
          pageEn.Project(targetSpatialReference);
          IEnvelope en = new EnvelopeClass();
          en.PutCoords(pageEn.XMin + HSpace, pageEn.YMin + down, pageEn.XMax - HSpace, pageEn.YMax - top);
          en.SpatialReference = targetSpatialReference;
          en.Project(GApplication.Application.MapControl.Map.SpatialReference);
          centerPoint=(en as IArea).Centroid;
          centerPoint.Project(GApplication.Application.MapControl.Map.SpatialReference);
          //ClipElement.UpdateClipGroupElement(centerPoint, refScale);
        
        }

        private void UpdateParms()
        {
            //纸张尺寸
            MapSizeInfoClass.PaperHeight= double.Parse(this.tbPaperHeight.Text);
            MapSizeInfoClass.PaperWidth =double.Parse( tbPaperWidth.Text);
            //成图尺寸
            MapSizeInfoClass.MapSizeHeight=  double.Parse( this.tbMapSizeHeight.Text);
            MapSizeInfoClass.MapSizeWidth= double.Parse( this.tbMapSizeWidth.Text);
            //内图廓
            MapSizeInfoClass.InlineHeight= double.Parse(   this.tbInlineHeight.Text);
            MapSizeInfoClass.InlineWidth= double.Parse( this.tbInlineWidth.Text );
            //内外间距
            MapSizeInfoClass.InOutLineWidth= double.Parse(this.tbInOutInterval.Value.ToString());
            //上间距
            MapSizeInfoClass.MapSizeTopInterval= (double)(this.tbMapTop.Value);
            //下间距
            MapSizeInfoClass.MapSizeDownInterval=  (double)( this.tbMapDown.Value);

        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            
            //纸张尺寸
            MapSizeInfoClass.PaperHeight = CommonMethods.PaperHeight;
            MapSizeInfoClass.PaperWidth = CommonMethods.PaperWidth;
            //成图尺寸
            MapSizeInfoClass.MapSizeHeight = CommonMethods.MapSizeHeight;
            MapSizeInfoClass.MapSizeWidth = CommonMethods.MapSizeWidth;
            //内图廓
            MapSizeInfoClass.InlineHeight = CommonMethods.InlineHeight;
            MapSizeInfoClass.InlineWidth = CommonMethods.InlineWidth;
            //内外间距
            MapSizeInfoClass.InOutLineWidth = CommonMethods.InOutLineWidth;
            //上间距
            MapSizeInfoClass.MapSizeTopInterval = CommonMethods.MapSizeTopInterval;
            //下间距
            MapSizeInfoClass.MapSizeDownInterval = CommonMethods.MapSizeDownInterval;
            ClipElement.UpdateClipGroupElement(centerPoint0, refScale);
            this.Close();
        }
    }
    public class MapSizeInfoClass
    {
        /// <summary>
        /// 纸张尺寸宽
        /// </summary>
        public static double PaperWidth;
        /// <summary>
        /// 纸张尺寸宽
        /// </summary>
        public static double PaperHeight;
        /// <summary>
        /// 成图尺寸宽
        /// </summary>
        public static double MapSizeWidth;
        /// <summary>
        /// 成图尺寸高
        /// </summary>
        public static double MapSizeHeight;
        /// <summary>
        /// 内图廓宽
        /// </summary>
        public static double InlineWidth;
        /// <summary>
        /// 内图廓高
        /// </summary>
        public static double InlineHeight;
        /// <summary>
        /// 内外图廓间距
        /// </summary>
        public static double InOutLineWidth;
        /// <summary>
        /// 上下间距
        /// </summary>
        public static double MapSizeTopInterval;
        public static double MapSizeDownInterval;
    }
}
