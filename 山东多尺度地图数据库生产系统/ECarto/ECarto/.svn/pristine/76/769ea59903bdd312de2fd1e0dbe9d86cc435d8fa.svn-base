using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// Maplex引擎的关键属性
    /// </summary>
    public class MaplexAttributeOfFeature
    {

        /// <summary>
        /// 根据注记规则解析注记的文本
        /// </summary>
        /// <param name="annoContent"></param>
        public MaplexAttributeOfFeature(string annoText, IFeature feature, int groupSymbolID, int featureWeight, int ClassIndex = -1)
        {
            this.FeatureClassName = feature.Class.AliasName;
            this.AnnotationClassID = feature.Class.ObjectClassID;
            this.FeatureID = feature.OID;
            this.GroupSymbolID = groupSymbolID;
            this.ClassIndex = ClassIndex;
            this.TextPathList = new List<IGeometry>();
            this.TextBoundList = new List<IPolygon>();
            this.FeatrueWeight = featureWeight;
            this.AnnoText = annoText;
        }
        /// <summary>
        /// 要素权重
        /// </summary>
        public int FeatrueWeight
        {
            set;
            get;
        }

        /// <summary>
        /// 要素集名称
        /// </summary>
        public string FeatureClassName
        {
            get;
            set;
        }
        /// <summary>
        /// 要素ClassID
        /// </summary>
        public int AnnotationClassID
        {
            get;
            set;
        }
        /// <summary>
        /// 需要创建注记的要素
        /// </summary>
        public int FeatureID
        {
            get;
            set;
        }
        /// <summary>
        /// 注记文本
        /// </summary>
        public string AnnoText
        {
            set;
            get;
        }

        /// <summary>
        /// 创建注记的文本路径,针对线性注记有多个的情况，一般情况下都是一个
        /// </summary>
        public List<IGeometry> TextPathList
        {
            get;
            set;
        }
        /// <summary>
        /// 创建注记的边框多边形,针对线性注记有多个的情况，一般情况下都是一个
        /// </summary>
        public List<IPolygon> TextBoundList
        {
            get;
            set;
        }


        /// <summary>
        /// 样式规则的索引
        /// </summary>
        public int ClassIndex
        {
            set;
            get;
        }
        /// <summary>
        /// 分组符号ID
        /// </summary>
        public int GroupSymbolID
        {
            get;
            set;
        }
    }
}
