using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SMGI.Common
{
   [Serializable]  
    public class MarginSettings   //图廓信息配置
    {
       /// <summary>
       /// 内图廓与图廓线的间距，单位为mm
       /// </summary>
        public double InnerDistance;  //mm
       /// <summary>
       /// 外图廓外与图廓线间距,单位mm
       /// </summary>
        public double OuterDistance; 
       /// <summary>
       /// 方里网的间距，单位M
       /// </summary>
        public double XYGridGap;  //m
       /// <summary>
       /// 经纬网短线间距,单位分
       /// </summary>
        public double LonlatGap;  //分
       /// <summary>
       /// 方里网标注字体
       /// </summary>
        public Font XYGridLabelFont = null;
       /// <summary>
       /// 邻带方里网标注字体
       /// </summary>
        public Font AdjGridLabelFont = null;
       /// <summary>
       /// 经纬度字体
       /// </summary>
        public Font LonlatLabelFont = null;
       /// <summary>
       /// 投影带号字体
       /// </summary>
        public Font ProjectionNoFont = null;
       /// <summary>
       /// 整百公里数字体
       /// </summary>
        public Font XYLabelPreFont = null;
       /// <summary>
       /// 是否添加图内方里网
       /// </summary>
        public bool IfAddXYInsideMap = true;

        public MarginSettings()
        {
            InnerDistance = 6;  //mm
            OuterDistance = 8;  //mm
            XYGridGap = 2000;   //m
            LonlatGap = 1;      //分
            XYGridLabelFont = new Font("方正中等线简体", (float)2.5, FontStyle.Regular, GraphicsUnit.Millimeter);
            LonlatLabelFont = new Font("方正中等线简体", (float)1.6, FontStyle.Regular, GraphicsUnit.Millimeter);
            ProjectionNoFont = new Font("方正中等线简体", (float)1.6, FontStyle.Regular, GraphicsUnit.Millimeter);
            XYLabelPreFont = new Font("方正中等线简体", (float)1.5, FontStyle.Regular, GraphicsUnit.Millimeter);
            AdjGridLabelFont = new Font("方正中等线简体", (float)1.8, FontStyle.Regular, GraphicsUnit.Millimeter);
        }
        
       
    }
   [Serializable]
   public class PageAndPaperSettings
   {
       public string PrinterName;
       public string PaperName;
       public string SourceName;
       public int Orientation;
       public int PageWidth;
       public int PageHeight;

       public PageAndPaperSettings()
       {
           PrinterName = "Deflault";
           PaperName = "Deflault";
           SourceName = "Deflault";
           Orientation = 2;
           PageWidth = 500;
           PageHeight = 800;
       }

   }
}
