using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Common
{

    /// <summary>
    ///  地图分幅用到的相关函数
    /// </summary>
    public class MapSubdivision
    {
        private double longitude = 0;
        private double latitude = 0;

        /// <summary>
        /// 根据比例尺得到经差
        /// </summary>
        /// <param name="scale_"></param>
        /// <returns></returns>
        public static double LongitudeGap(long scale_)
        {

            double gap = 0;
            if (scale_ > 100000)
            {
                gap = 6 * (scale_ / 1000000.0);
            }
            else if (scale_ <= 100000 && scale_ > 10000)
            {
                gap = 0.5 * (scale_ / 100000.0);
            }
            else if (scale_ <= 10000)
            {
                gap = 3.75 / 60.0;
            }
            return gap;
        }
        /// <summary>
        /// 根据比例尺得到纬差
        /// </summary>
        /// <param name="scale_"></param>
        /// <returns></returns>
        public static double LatitudeGap(long scale_)
        {
            double gap = 0;
            if (scale_ > 100000)
            {
                gap = 4 * (scale_ / 1000000.0);
            }
            else if (scale_ <= 100000 && scale_ > 10000)
            {
                gap = (1.0 / 3.0) * (scale_ / 100000.0);
            }
            else if (scale_ <= 10000)
            {
                gap = 2.5 / 60.0;
            }
            return gap;
        }

        /// <summary>
        /// 计算100W的分幅图号，返回的结果类似 J50
        /// </summary>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        /// <returns></returns>
        public static string Calculate100WNumber(double longitude, double latitude)
        {
            string s_Number = "";
            double a = 0;//纬度
            double b = 0;//经度

            a = Math.Floor(latitude / 4) + 1;
            b = Math.Floor(longitude / 6) + 31;

            Char a_ = (Char)(a + 64);
            s_Number = a_.ToString() + b.ToString();

            return s_Number;
        }

        /// <summary>
        /// 计算某点经纬度在指定比例尺下的分幅号，结果类似 "J50D002002"
        /// </summary>
        /// <param name="scale_"></param>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        /// <returns></returns>
        public static string CalculateSubdivisionNumber(long scale_, double longitude, double latitude)
        {
            double c = 0;//行号
            double d = 0;//列号

            double deltaJ = LongitudeGap(scale_); //longitude gap
            double deltaW = LatitudeGap(scale_);  //latitude gap
            c = 4 / deltaW - Math.Floor((latitude % 4) / deltaW);
            d = Math.Floor((longitude % 6) / deltaJ) + 1;
            string scale_No = "";
            string Sub_No = "";

            if (scale_ == 10000) scale_No = "G";
            else if (scale_ == 25000) scale_No = "F";
            else if (scale_ == 50000) scale_No = "E";
            else if (scale_ == 100000) scale_No = "D";
            else if (scale_ == 250000) scale_No = "C";
            else if (scale_ == 500000) scale_No = "B";
            else scale_No = "";
            string rowString = "00" + c.ToString();
            rowString = rowString.Substring(rowString.Length - 3, 3);
            string columnString = "00" + d.ToString();
            columnString = columnString.Substring(columnString.Length - 3, 3);

            Sub_No = Calculate100WNumber(longitude, latitude) + scale_No + rowString + columnString;
            if (scale_ == 2000)
            {
                string no_str1 = Math.Floor(longitude / 1000).ToString();
                no_str1 = no_str1.Substring(no_str1.Length - 2, 2);
                string no_str2 = Math.Floor(latitude / 1000).ToString();
                no_str2 = no_str2.Substring(no_str2.Length - 2, 2);
                Sub_No = no_str1 + ".0-" + no_str2 + ".0";
            }

            return Sub_No;
        }


        /// <summary>
        /// 计算该点所在标准图幅的西南图廓点经纬度 WSBL == West south Longitude Latitude
        /// </summary>
        /// <param name="scale_"></param>
        /// <param name="longitude_"></param>
        /// <param name="latitude_"></param>
        public static void CalculateWSBLFromArbitraryBL(long scale_, double longitude, double latitude, out double longitude_, out double latitude_)
        {
            double deltaJ = LongitudeGap(scale_); //longitude gap
            double deltaW = LatitudeGap(scale_);  //latitude gap
            latitude_ = latitude - latitude % deltaW;
            longitude_ = longitude - longitude % deltaJ;
        }
        /// <summary>
        /// 由图幅号计算该图幅西南图廓点的经、纬度
        /// </summary>
        /// <param name="SubdivisionNumber"></param>
        /// <param name="longitude_"></param>
        /// <param name="latitude_"></param>
        public static void CalculateBLFromSubdivisionNumber(string SubdivisionNumber, out double longitude_, out double latitude_, out long scale_long)
        {
            string row_100W = SubdivisionNumber.Substring(0, 1);
            string column_100W = SubdivisionNumber.Substring(1, 2);
            string scale_str = SubdivisionNumber.Substring(3, 1);
            string row_scale = SubdivisionNumber.Substring(4, 3);
            string column_scale = SubdivisionNumber.Substring(7, 3);

            int i_row_100W = row_100W[0] - 64;
            int i_column_100W = System.Convert.ToInt16(column_100W);
            long scale_ = 0;
            int row_ = System.Convert.ToInt16(row_scale);
            int column_ = System.Convert.ToInt16(column_scale);

            //这应该提成一个函数，有更多的判断条件
            if (scale_str == "B") scale_ = 500000;
            else if (scale_str == "C") scale_ = 250000;
            else if (scale_str == "D") scale_ = 100000;
            else if (scale_str == "E") scale_ = 50000;
            else if (scale_str == "F") scale_ = 50000;
            else if (scale_str == "G") scale_ = 10000;
            else scale_ = 10000;


            longitude_ = (i_column_100W - 31) * 6 + (column_ - 1) * LongitudeGap(scale_);
            double delta_b = LatitudeGap(scale_);
            latitude_ = (i_row_100W - 1) * 4 + (4.0 / delta_b - row_) * delta_b;
            scale_long = scale_;
        }

        public static long ParseScale(string scale_str)
        {
            scale_str = scale_str.Substring(3, 1);
            long scale_ = 0;
            if (scale_str == "B") scale_ = 500000;
            else if (scale_str == "C") scale_ = 250000;
            else if (scale_str == "D") scale_ = 100000;
            else if (scale_str == "E") scale_ = 50000;
            else if (scale_str == "F") scale_ = 50000;
            else if (scale_str == "G") scale_ = 10000;
            else scale_ = 10000;
            return scale_;
        }

        /// <summary>
        /// 返回中央经线
        /// </summary>
        /// <param name="longitude_"></param>
        /// <param name="three_six"></param>
        /// <returns></returns>
        public static double ComputeCentralMeridian(double longitude_, int three_six, out double ZoneNumber)
        {
            double centralMeridian = 0;
            double m = 0;


            if (three_six == 3)
            {
                if ((longitude_ - 1.5) % 3 == 0)
                {
                    m = (longitude_ - 1.5) / 3;
                }
                else
                {
                    m = Math.Floor((longitude_ - 1.5) / 3) + 1;
                }
                centralMeridian = 3 * m;

            }
            else if (three_six == 6)
            {

                if (longitude_ % 6 == 0)
                    m = longitude_ / 6;
                else
                {
                    m = Math.Floor(longitude_ / 6) + 1;
                }

                centralMeridian = 6 * m - 3;

            }
            ZoneNumber = m;
            return centralMeridian;

        }
        /// <summary>
        /// 返回中央经线
        /// </summary>
        /// <param name="longitude_"></param>
        /// <param name="three_six"></param>
        /// <returns></returns>
        public static double ComputeCentralMeridian(double longitude_, int three_six)
        {
            double centralMeridian = 0;
            double m = 0;

            if (three_six == 3)
            {
                if ((longitude_ - 1.5) % 3 == 0)
                {
                    m = (longitude_ - 1.5) / 3;
                }
                else
                {
                    m = Math.Floor((longitude_ - 1.5) / 3) + 1;
                }
                centralMeridian = 3 * m;

            }
            else if (three_six == 6)
            {

                if (longitude_ % 6 == 0)
                    m = longitude_ / 6;
                else
                {
                    m = Math.Floor(longitude_ / 6) + 1;
                }

                centralMeridian = 6 * m - 3;
            }
            return centralMeridian;

        }



       

        /// <summary>
        /// 通过经纬度指定范围，计算该范围所占的图幅范围，将其所有的点记录下来,以输出参数的形式传出
        /// </summary>
        /// <param name="scale_"></param>
        /// <param name="longitude_min"></param>
        /// <param name="latitude_min"></param>
        /// <param name="longitude_max"></param>
        /// <param name="latitude_max"></param>
        /// <param name="Subdivision_B"></param>
        /// <param name="Subdivision_L"></param>
        public static void CalculateSubdivisionFromExtent(long scale_,
            double longitude_min,
            double latitude_min,
            double longitude_max,
            double latitude_max,
            out double[] Subdivision_B,
            out double[] Subdivision_L)
        {
            double deltaJ = LongitudeGap(scale_); //longitude gap
            double deltaW = LatitudeGap(scale_);  //latitude gap

            double longitude_min_ws;
            double latitude_min_ws;
            double longitude_max_ws;
            double latitude_max_ws;

            CalculateWSBLFromArbitraryBL(scale_, longitude_min, latitude_min, out longitude_min_ws, out latitude_min_ws);
            CalculateWSBLFromArbitraryBL(scale_, longitude_max, latitude_max, out longitude_max_ws, out latitude_max_ws);

            long row_number = (long)Math.Round((latitude_max_ws - latitude_min_ws) / deltaW) + 1;
            long column_number = (long)Math.Round((longitude_max_ws - longitude_min_ws) / deltaJ) + 1;

            Subdivision_B = new double[column_number + 1];     //longitude
            for (int j = 0; j < column_number + 1; j++)
            {
                Subdivision_B[j] = longitude_min_ws + deltaJ * j;
            }

            Subdivision_L = new double[row_number + 1];        //latitude
            for (int i = 0; i < row_number + 1; i++)
            {
                Subdivision_L[i] = latitude_min_ws + deltaW * i;
            }

        }



    }

    public struct DMS
    {
        public double doubleDD;     //度
        public double doubleMM;     //分
        public double doubleSS;     //秒

        public double decimal_dms;   //度分秒的十进制格式

        public DMS(double DD, double MM, double SS)
        {
            doubleDD = DD;
            doubleMM = MM;
            doubleSS = SS;
            decimal_dms = ((SS / 60.0) + MM) / 60.0 + DD;
        }
        public DMS(double d_dms, bool ifup,int i)
        {
            decimal_dms = d_dms;
            doubleDD = Math.Floor(decimal_dms);
            doubleMM = Math.Floor((decimal_dms - doubleDD) * 60.0);
            doubleSS = Math.Round(((decimal_dms - doubleDD) * 60.0 - doubleMM) * 60);

            if (ifup)
            {
                doubleSS = (Math.Ceiling(doubleSS / i)) * i;
                if (doubleSS == 60)
                {
                    doubleSS = 0;
                    doubleMM++;
                }
                if (doubleMM == 60)
                {
                    doubleMM = 0;
                    doubleDD++;
                }
            }
            else
            {
                doubleSS = (Math.Floor(doubleSS / i)) * i;
                if (doubleSS == 60)
                {
                    doubleSS = 0;
                    doubleMM++;
                }
                if (doubleMM == 60)
                {
                    doubleMM = 0;
                    doubleDD++;
                }
            }

          
        }
        public DMS(double d_dms)
        {
            decimal_dms = d_dms;
            doubleDD = Math.Floor(decimal_dms);
            doubleMM = Math.Floor((decimal_dms - doubleDD) * 60.0);
            doubleSS = Math.Round(((decimal_dms - doubleDD) * 60.0 - doubleMM) * 60);
            if (doubleSS == 60)
            {
                doubleSS = 0;
                doubleMM++;
            }
            if (doubleMM == 60)
            {
                doubleMM = 0;
                doubleDD++;
            }
        }
    }




}
