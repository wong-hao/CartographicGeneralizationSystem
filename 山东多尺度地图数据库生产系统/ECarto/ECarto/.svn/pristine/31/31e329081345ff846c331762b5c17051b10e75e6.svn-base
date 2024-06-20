using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMGI.Common
{
    [Serializable]
    public enum MapExtentType
    {
        BZTF1W,
        BZTF2W5,
        BZTF5W,
        BZTF10W,
        BZTF25W,
        BZTF50W,
        CUSTOM
    }

    [Serializable]
    public enum renderMode
    {
        高速渲染,
        精细渲染
    }


        //public List<ESRI.ArcGIS.Geodatabase.IWorkspace> Datalist
        //{
        //    get { return datalist; }
        //    set { datalist = value; }
        //}

    [Serializable]
    public class Auto4DMapInfo
    {
        private MapExtentType extentType = MapExtentType.BZTF5W;

        public MapExtentType ExtentType
        {
            get { return extentType; }
            set { extentType = value; }
        }
        private string extentLayer = "BOUA";

        public string ExtentLayer
        {
            get { return extentLayer; }
            set { extentLayer = value; }
        }
        private long mapReferenceScale = 50000;

        public long MapReferenceScale
        {
            get { return mapReferenceScale; }
            set { mapReferenceScale = value; }
        }

        private renderMode mapRenderMode = renderMode.精细渲染;

        public renderMode MapRenderMode
        {
            get { return mapRenderMode; }
            set { mapRenderMode = value; }
        }

        private Product product = null;
        public Product Product
        {
            get { return product; }
            set { product = value; }
        }

        private List<Product> producthistory = new List<Product>();
        public List<Product> Producthistory
        {
            get { return producthistory; }
            set { producthistory = value; }
        }

        private content content;
        public content Content
        {
            get { return content; }
            set { content = value; }
        }

        private List<string> process = new List<string>();
        public List<string> Process
        {
            get { return process; }
            set { process = value; }
        }

        public Auto4DMapInfo()
        {
            mapRenderMode = renderMode.精细渲染;
        }
    }
}
