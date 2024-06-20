using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using System.Drawing;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Common
{

    [Serializable]
    public class PageLayoutSettings
    {
        private string publisherStr;
        /// <summary>
        /// 设置或者获取出版商信息
        /// </summary>
        public string PublisherStr
        {
            get
            {
                return publisherStr;
            }
            set
            {
                publisherStr = value;
            }
        }

        private string mapNameStr;
        /// <summary>
        /// 设置或者获取地图名
        /// </summary>
        public string MapNameStr
        {
            get
            {
                return mapNameStr;
            }
            set
            {
                mapNameStr = value;
            }
        }

        private string mapTypeStr;
        /// <summary>
        /// 设置或者获取地图类型信息
        /// </summary>
        public string MapTypeStr
        {
            get
            {
                return mapTypeStr;
            }
            set
            {
                mapTypeStr = value;
            }
        }
        private string[] mapInstructionStrs;
        /// <summary>
        /// 获取或者设置多行图件说明
        /// </summary>
        public string[] MapInstructionStrs
        {
            get
            {
                return mapInstructionStrs; 
            }
            set
            {
                mapInstructionStrs = value;
            }
        }
        private string  mapInstructionStr;
        /// <summary>
        /// 获取或者设置图件说明
        /// </summary>
        public string MapInstructionStr
        {
            get
            {
                return mapInstructionStr;
            }
            set
            {
                mapInstructionStr = value;
            }
        }

        private string adjunctInformation;
        /// <summary>
        /// 设置或者获取附注
        /// </summary>
        public string AdjunctInformation
        {
            get
            {
                return adjunctInformation;
            }
            set
            {
                adjunctInformation = value;
            }
                
        }

        private Font fourCornerTFFont;
        /// <summary>
        /// 设置或者获取四角图幅号字体
        /// </summary>
        public Font FourCornerTFFont
        {
            get
            {
                return fourCornerTFFont;
            }
            set
            {
                fourCornerTFFont = value;
            }
        }


        private Font fourCornerMapNameFont;
        /// <summary>
        /// 设置或者获取四角图名字体
        /// </summary>
        public Font FourCornerMapNameFont
        {
            get
            {
                return fourCornerMapNameFont;
            }
            set
            {
                fourCornerMapNameFont = value;
            }
        }


        private Font mapNameFont;
        /// <summary>
        /// 获取或者设置地图名字体
        /// </summary>
        public Font MapNameFont
        {
            get
            {
                return mapNameFont;
            }
            set
            {
                mapNameFont = value;
            }
        }

        private Font tfFont;
        /// <summary>
        /// 获取或者设置图幅号字体
        /// </summary>
        public Font TFFont
        {
            get
            {
                return tfFont;
            }
            set
            {
                tfFont = value;
            }
        }


        private Font publisherFont;
        /// <summary>
        /// 获取或者设置出版商信息
        /// </summary>
        public Font PublisherFont
        {
            get
            {
                return publisherFont;
            }
            set
            {
                publisherFont = value;
            }
        }

        private Font mapInstructionFont;
        /// <summary>
        /// 获取或者设置图件说明字体
        /// </summary>
        public Font MapInstructionFont
        {
            get
            {
                return mapInstructionFont;
            }
            set
            {
                mapInstructionFont = value;
            }
        }

        private Font adjunctInformationFont;
        /// <summary>
        /// 获取或者设置附注
        /// </summary>
        public Font AdjunctInformationFont
        {
            get
            {
                return adjunctInformationFont;
            }
            set
            {
                adjunctInformationFont = value;
            }
        }


        private Font mapTypeFont ;
        /// <summary>
        /// 获取或者设置地图类型字体
        /// </summary>
        public Font MapTypeFont
        {
            get
            {
                return mapTypeFont;
            }
            set
            {
                mapTypeFont = value;
            }

        }

        private  Font mapScaleFont;
        /// <summary>
        /// 获取或者设置地图比例尺信息
        /// </summary>
        public Font MapScaleFont
        {
            get
            {
                return mapScaleFont;
            }
            set
            {
                mapScaleFont = value;
            }
        }

        public int iItemSelectBit;

        public PageLayoutSettings()
        {
            SetToDefault();
        }

        public void SetToDefault()
        {
            fourCornerMapNameFont = new Font("方正中等线简体",(float)4.0,
                            FontStyle.Bold,GraphicsUnit.Millimeter);

            fourCornerTFFont = new Font("方正中等线简体",(float)3.0,FontStyle.Regular,
                 GraphicsUnit.Millimeter);

            mapNameFont = new Font("方正中等线简体",(float)7.0,FontStyle.Regular,
                GraphicsUnit.Millimeter);

            tfFont = new Font("方正中等线简体",(float)4.0,FontStyle.Regular,
                GraphicsUnit.Millimeter);

            publisherFont = new Font("方正中等线简体",(float)4.0,FontStyle.Regular,
                GraphicsUnit.Millimeter);

            mapInstructionFont = new Font("宋体",(float)2.0,FontStyle.Regular,
                GraphicsUnit.Millimeter);

            adjunctInformationFont = new Font("宋体",(float)2.0,FontStyle.Regular,
                GraphicsUnit.Millimeter);

            mapScaleFont = new Font("宋体",(float)3.0,FontStyle.Regular,
                GraphicsUnit.Millimeter);

            mapTypeFont = new Font("方正中等线简体",(float)4.0,FontStyle.Regular,
                GraphicsUnit.Millimeter);

            publisherStr = "武汉大学资环学院506室";

            mapTypeStr = "中华人民共和国基本比例尺地形图";

            mapInstructionStr = "图件说明";
            mapInstructionStrs=new string[3];
            mapInstructionStrs[0] = "该图为武汉大学资环学院506室，4D项目组编绘，依据标准规范";
            mapInstructionStrs[1] = "";
            mapInstructionStrs[2] = "";


            adjunctInformation = "附注";
            iItemSelectBit = 0;

            mapNameStr = "德阳市";
            
        }


    }
    /*2013.5.21 何建清*/
}
