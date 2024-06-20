using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.IO;
using System.Xml.Serialization;
using SMGI.Common;

namespace SMGI.Common
{

    public class CartoRenderer
    {
        private string _whereClause = "0000";
        public string WhereClause
        {
            get { return _whereClause; }
            set { _whereClause = value; }
        }

        private string _symbolValue = "0000";

        public string SymbolValue
        {
            get { return _symbolValue; }
            set { _symbolValue = value; }
        }


        public CartoRenderer()
        {

        }
    }

    public class CartoRendererInfo
    {
        private int _fieldCount = 1;

        public int FieldCount
        {
            get { return _fieldCount; }
            set { _fieldCount = value; }
        }
        private string _renderField = "field";

        public string RenderField
        {
            get { return _renderField; }
            set { _renderField = value; }
        }
        private List<CartoRenderer> _renderInfo;

        public List<CartoRenderer> RenderInfo
        {
            get { return _renderInfo; }
            set { _renderInfo = value; }
        }
        private bool _isConvertRep = false;

        public bool IsConvertRep
        {
            get { return _isConvertRep; }
            set { _isConvertRep = value; }
        }
        private List<string> _repValues = null;

        public List<string> RepValues
        {
            get { return _repValues; }
            set { _repValues = value; }
        }




        public CartoRendererInfo()
        {
            FieldCount = 0;
            RenderField = "0000";
            RenderInfo = new List<CartoRenderer>();
            IsConvertRep = false;
            _repValues = new List<string>();
        }
    }


    public class CartoLabelInfo
    {
        private string _classWhereClause = "0000";
        public string ClassWhereClause
        {
            get { return _classWhereClause; }
            set { _classWhereClause = value; }
        }

        private string _classLabelExpression = "0000";
        public string ClassLabelExpression
        {
            get { return _classLabelExpression; }
            set { _classLabelExpression = value; }
        }

        private string _classLabelSymbol = "0000";
        public string ClassLabelSymbol
        {
            get { return _classLabelSymbol; }
            set { _classLabelSymbol = value; }
        }

        private bool inStyleGallery = false;
        public bool InStyleGallery
        {
            get { return inStyleGallery; }
            set { inStyleGallery = value; }
        }

        private string _fontName = "宋体";
        public string FontName
        {
            get { return _fontName; }
            set { _fontName = value; }
        }

        private double _fontSize = 8;
        public double FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; }
        }



        public CartoLabelInfo()
        {
            ClassWhereClause = "0000";
            ClassLabelExpression = "0000";
            ClassLabelSymbol = "0000";
            InStyleGallery = false;
            FontName = "宋体";
            FontSize = 8;
        }
    }




    [Serializable]
    public class CartoLayerConfig
    {
        // FeatureLayer对象用以存储静态的配置信息
        private ILayer featLayer = null;
        public ILayer FeatLayer
        {
            get { return featLayer; }
        }
        
        //图层的动态信息实现表现形式的动态变化
        private IFeatureClass featCls = null;
        private StyleManager styleMgr = null;

        private string layerName = "0000";
        //Geodatabase中图层的名称，可以用作匹配依据
        public string LayerName
        {
            get { return layerName; }
            set { layerName = value; }
        }

        private string layerCaption = "图层中文名";

        public string LayerCaption
        {
            get { return layerCaption; }
            set { layerCaption = value; }
        }
        private string layerGroup = "图层分组";

        public string LayerGroup
        {
            get { return layerGroup; }
            set { layerGroup = value; }
        }
        private string layerType = "图层类型";

        public string LayerType
        {
            get { return layerType; }
            set { layerType = value; }
        }

        public string LayerString
        {
            get
            {
                if (featLayer == null)
                {
                    return "";
                }
                else
                { 
                    return Common.GConvert.ObjectToBase64(featLayer);
                }

            }
            set {

                featLayer = Common.GConvert.Base64ToObject(value) as ILayer;
            }
        }

        private CartoRendererInfo layerRenderInfo = null;

        public CartoRendererInfo LayerRenderInfo
        {
            get { return layerRenderInfo; }
            set { layerRenderInfo = value; }
        }
        private string labelField = "0000";

        public string LabelField
        {
            get { return labelField; }
            set { labelField = value; }
        }
        private List<CartoLabelInfo> labelInfo = null;

        public List<CartoLabelInfo> LabelInfo
        {
            get { return labelInfo; }
            set { labelInfo = value; }
        }
        private bool isLabel = false;

        public bool IsLabel
        {
            get { return isLabel; }
            set { isLabel = value; }
        }

        private bool isConvertAnno = false;
        public bool IsConvertAnno
        {
            get { return isConvertAnno; }
            set { isConvertAnno = value; }
        }

        public void ToString2()
        {

        }

        public CartoLayerConfig()
        {

        }

        public CartoLayerConfig(ILayer fl)
        {
            Init();
            featLayer = fl;
        }

        public void Init()
        {
            layerName = "00";
            layerCaption = "00";
            layerGroup = "00";
            layerType = "00";

            layerRenderInfo = new CartoRendererInfo();
            labelField = "00";
            labelInfo = new List<CartoLabelInfo>();
            isLabel = false;
            isConvertAnno = false;
        }
    }
}
