using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMGI.Common {

    public class 基础地理信息要素分类 {

        public string 分类代码 { get; private set; }
        public string 要素名称 { get; private set; }
        public bool 大比例尺 { get; private set; }
        public bool 中比例尺 { get; private set; }
        public bool 小比例尺 { get; private set; }
        //public bool 几何特征 { get; private set; }
        internal 基础地理信息要素分类(string 分类代码, string 要素名称, bool 大比例尺, bool 中比例尺, bool 小比例尺) {
            this.分类代码 = 分类代码;
            this.要素名称 = 要素名称;
            this.大比例尺 = 大比例尺;
            this.中比例尺 = 中比例尺;
            this.小比例尺 = 小比例尺;
        }
        public 基础地理信息要素分类 Value { get { return this; } }
        static public class 定位基础 {
            static public 基础地理信息要素分类 Value = new 基础地理信息要素分类("100000", "定位基础", true, true, true);
            static public class 测量控制点 {
                static public 基础地理信息要素分类 Value = new 基础地理信息要素分类("110000", "测量控制点", true, true, true);
                static public class 平面控制点 {
                    static public 基础地理信息要素分类 Value = new 基础地理信息要素分类("110100", "平面控制点", true, true, true);
                    static public 基础地理信息要素分类 大地原点 = new 基础地理信息要素分类("110101", "大地原点", true, true, false);
                    static public 基础地理信息要素分类 三角点 = new 基础地理信息要素分类("110102", "三角点", true, true, true);
                    static public 基础地理信息要素分类 图跟点 = new 基础地理信息要素分类("110103", "图跟点", true, false, false);
                }
                static public class 高程控制点 {
                    static public 基础地理信息要素分类 Value = new 基础地理信息要素分类("110200", "高程控制点", true, true, true);
                    static public 基础地理信息要素分类 水准原点 = new 基础地理信息要素分类("110201", "水准原点", true, true, true);
                    static public 基础地理信息要素分类 水准点 = new 基础地理信息要素分类("110202", "水准点", true, true, false);
                }
                static public class 卫星定位控制点 {
                    static public 基础地理信息要素分类 Value = new 基础地理信息要素分类("110300", "卫星定位控制点", true, true, true);
                    static public 基础地理信息要素分类 卫星定位连续运行站点 = new 基础地理信息要素分类("110301", "卫星定位连续运行站点", false, true, true);
                    static public 基础地理信息要素分类 卫星定位等级点 = new 基础地理信息要素分类("110302", "卫星定位等级点", true, true, false);
                }
                static public class 其它测量控制点 {
                    static public 基础地理信息要素分类 Value = new 基础地理信息要素分类("110400", "其它测量控制点", true, true, true);
                    static public 基础地理信息要素分类 重力点 = new 基础地理信息要素分类("110201", "重力点", true, true, true);
                    static public 基础地理信息要素分类 独立天文点 = new 基础地理信息要素分类("110102", "独立天文点", true, true, true);
                }
                static public 基础地理信息要素分类 测量控制点注记 = new 基础地理信息要素分类("119000", "测量控制点注记", true, true, true);
            }
            static public class 数学基础 {
                static public 基础地理信息要素分类 Value = new 基础地理信息要素分类("120000", "数学基础", true, true, true);

                static public 基础地理信息要素分类 内轮廓线 = new 基础地理信息要素分类("120100", "内轮廓线", true, true, true);
                static public 基础地理信息要素分类 坐标网线 = new 基础地理信息要素分类("120200", "坐标网线", true, true, true);
                static public 基础地理信息要素分类 经线 = new 基础地理信息要素分类("120300", "经线", true, true, true);
                static public class 纬线 {
                    static public 基础地理信息要素分类 Value = new 基础地理信息要素分类("120400", "纬线", true, true, true);
                    static public 基础地理信息要素分类 北回归线 = new 基础地理信息要素分类("120300", "北回归线", false, true, true);
                }

            }

        }
        
    }
}
