using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;
namespace SMGI.Plugin.ThematicChart
{
   
    public enum  pageType
    {
        PageEmptyS = 1,
        PageEmptyD = 2,
        PageSingle = 3,
        PageDouble = 4         
    }
    
    //地图页面相关属性
    public class PageInfo
    {
        public string PageID;//每个页面都有一个唯一ID
        public string Title;///标题
        public pageType MapPageType;///页面类型
        
        public double MapScale;
        public IPoint MapCenter;
        public double Height;
        public double Width;
        public string GDBPath;//对应
        public int PageNum;
        public string DataSource;//数据来源
        public string MapTemplateStyle;//模板风格 ：一般
        public string MapTemplate;//数据升级模板比例尺 ：5万
        public string MapTemplateName;//数据升级模板名称：多尺度模板
        public string DatabaseName;
    }

    //地图集工程相关信息
    public class AtlasApplication
    {
        public static string[] PageType = new string[] { "单空页", "双空页","单页", "双页" };
        public static PageInfo CurrentPage;//当前页面
        public static string GUIDPath;
        public double BookWidth;
        public double BookHeight;
        public static string ProjectPath;
        public static string ProjectName;
        public static string ProjectIndexFile;//xml
        public static string ProjectFullName;

    }
  
}
