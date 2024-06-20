using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SMGI.Plugin.GeneralEdit
{
    public class BranchMapHelper
    {
        //椭球长半径
        private const double LongRadius = 6378137.00;

        //椭球扁率
        private const double OblatusRatio = 1 / 298.257222101;

        //起始X
        private const double StartX = 66;

        //起始Y
        private const double StartY = 60;

        //百万行号
        private List<string> _millionRows;

        //百万列号
        private List<string> _millionPaths;

        //百万以外分幅图信息
        private List<BranchMapInfo> _branchMapInfos;

        public BranchMapHelper()
        {
            _millionRows = new List<string> { "O", "N", "M", "L", "K", "J", "I", "H", "G", "F", "E", "D", "C", "B", "A" };
            _millionPaths = new List<string> { "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54" };
            _branchMapInfos = new List<BranchMapInfo>();
            _branchMapInfos.Add(new BranchMapInfo { BranchMapName = "1:500000", BranchMapCode = "B", GridCount = 2 });
            _branchMapInfos.Add(new BranchMapInfo { BranchMapName = "1:250000", BranchMapCode = "C", GridCount = 4 });
            _branchMapInfos.Add(new BranchMapInfo { BranchMapName = "1:100000", BranchMapCode = "D", GridCount = 12 });
            _branchMapInfos.Add(new BranchMapInfo { BranchMapName = "1:50000", BranchMapCode = "E", GridCount = 24 });
            _branchMapInfos.Add(new BranchMapInfo { BranchMapName = "1:25000", BranchMapCode = "F", GridCount = 48 });
            _branchMapInfos.Add(new BranchMapInfo { BranchMapName = "1:10000", BranchMapCode = "G", GridCount = 96 });
            _branchMapInfos.Add(new BranchMapInfo { BranchMapName = "1:5000", BranchMapCode = "H", GridCount = 192 });
            _branchMapInfos.Add(new BranchMapInfo { BranchMapName = "1:2000", BranchMapCode = "I", GridCount = 576 });
            _branchMapInfos.Add(new BranchMapInfo { BranchMapName = "1:1000", BranchMapCode = "J", GridCount = 1152 });
            _branchMapInfos.Add(new BranchMapInfo { BranchMapName = "1:500", BranchMapCode = "K", GridCount = 2304 });
        }

        /// <summary>
        /// 获取图幅比例尺
        /// </summary>
        /// mapName:图幅号比如：H48G001002
        public string GetScale(string mapName)
        {
            if (mapName.Length == 3)
                return "1:1000000";
            if (mapName.Length == 10 || mapName.Length == 12)
            {
                var type = mapName.Substring(3, 1);
                var branchMapInfo = _branchMapInfos.Single(i => i.BranchMapCode == type);
                return branchMapInfo.BranchMapName;
            }
            return "0:0";
        }

        /// <summary>
        /// 获取周围图幅
        /// </summary>
        public List<string> GetNearMap(string mapName, double jl = 0)
        {
            var nearMaps = new List<string>();

            if (jl > 0)//非标准分幅, 如1：1000 图名为21.0-81.0, jl = 图幅距离/1000
            {
                var x = Convert.ToDouble(mapName.Split('-').First());
                var y = Convert.ToDouble(mapName.Split('-').Last());

                for (var i = x + jl; i >= x - jl; i = i - jl)
                {
                    for (var j = y - jl; j <= y + jl; j = j + jl)
                    {
                        nearMaps.Add(i.ToString("F1") + "-" + j.ToString("F1"));
                    }
                }
            }
            else if (mapName.Length == 3) //1:1000000
            {
                //百万图幅行序号
                var millionRowIndex = _millionRows.IndexOf(mapName.Substring(0, 1));

                //百万图幅列序号
                var millionPathIndex = _millionPaths.IndexOf(mapName.Substring(1, 2));
                for (var i = millionRowIndex - 1; i <= millionRowIndex + 1; i++)
                {
                    for (var j = millionPathIndex - 1; j <= millionPathIndex + 1; j++)
                    {
                        if (i < 0 || i > _millionRows.Count - 1 || j < 0 || j > _millionPaths.Count - 1) continue;
                        nearMaps.Add(_millionRows[i] + _millionPaths[j]);
                    }
                }
            }
            else if (mapName.Length == 10 || mapName.Length == 12)
            {
                const string tempString = "0000";
                var millionRowIndex = _millionRows.IndexOf(mapName.Substring(0, 1));
                var millionPathIndex = _millionPaths.IndexOf(mapName.Substring(1, 2));

                var branchType = mapName.Substring(3, 1);
                var branchRowIndex = Convert.ToInt32(mapName.Substring(4, (mapName.Length - 4) / 2));
                var branchPathIndex = Convert.ToInt32(mapName.Substring(4 + (mapName.Length - 4) / 2, (mapName.Length - 4) / 2));
                var branchMapInfo = _branchMapInfos.Single(i => i.BranchMapCode == branchType);

                for (var i = branchRowIndex - 1; i <= branchRowIndex + 1; i++)
                {
                    for (var j = branchPathIndex - 1; j <= branchPathIndex + 1; j++)
                    {
                        var millionRow = _millionRows[millionRowIndex];
                        var branchRow = tempString.Substring(0, (mapName.Length - 4) / 2 - i.ToString().Length) + i.ToString();
                        int tempIndex;
                        if (i < 1)
                        {
                            millionRow = _millionRows[millionRowIndex - 1];
                            tempIndex = branchMapInfo.GridCount;
                            branchRow = tempString.Substring(0, (mapName.Length - 4) / 2 - tempIndex.ToString().Length) + tempIndex.ToString();
                        }
                        if (i > branchMapInfo.GridCount)
                        {
                            millionRow = _millionRows[millionRowIndex + 1];
                            tempIndex = 1;
                            branchRow = tempString.Substring(0, (mapName.Length - 4) / 2 - tempIndex.ToString().Length) + tempIndex.ToString();
                        }

                        var millionPath = _millionPaths[millionPathIndex];
                        var branchPath = tempString.Substring(0, (mapName.Length - 4) / 2 - j.ToString().Length) + j.ToString();
                        if (j < 1)
                        {
                            millionPath = _millionPaths[millionPathIndex - 1];
                            tempIndex = branchMapInfo.GridCount;
                            branchPath = tempString.Substring(0, (mapName.Length - 4) / 2 - tempIndex.ToString().Length) + tempIndex.ToString();
                        }
                        if (j > branchMapInfo.GridCount)
                        {
                            millionPath = _millionPaths[millionPathIndex + 1];
                            tempIndex = 1;
                            branchPath = tempString.Substring(0, (mapName.Length - 4) / 2 - tempIndex.ToString().Length) + tempIndex.ToString();
                        }

                        nearMaps.Add(millionRow + millionPath + branchType + branchRow + branchPath);
                    }
                }
            }

            return nearMaps;
        }

        /// <summary>
        /// 获取图幅范围（经纬度）:图廓
        /// mapName:根据mdb名称。
        /// </summary>
        public DataRange GetRange(string mapName)
        {
            var range = new DataRange();

            //百万图幅行序号
            var millionRowIndex = _millionRows.IndexOf(mapName.Substring(0, 1));

            //百万图幅列序号
            var millionPathIndex = _millionPaths.IndexOf(mapName.Substring(1, 2));

            range.MinX = StartX + millionPathIndex * 6;
            range.MaxX = StartX + (millionPathIndex + 1) * 6;
            range.MinY = StartY - (millionRowIndex + 1) * 4;
            range.MaxY = StartY - millionRowIndex * 4;

            if (mapName.Length == 10 || mapName.Length == 12)
            {
                var branchType = mapName.Substring(3, 1);
                var branchRowIndex = Convert.ToDouble(mapName.Substring(4, (mapName.Length - 4) / 2)) - 1;
                var branchPathIndex = Convert.ToDouble(mapName.Substring(4 + (mapName.Length - 4) / 2, (mapName.Length - 4) / 2)) - 1;
                var branchMapInfo = _branchMapInfos.Single(i => i.BranchMapCode == branchType);

                var startX = range.MinX;
                var startY = range.MaxY;

                range.MinX = startX + branchPathIndex * 6 / branchMapInfo.GridCount;
                range.MaxX = startX + (branchPathIndex + 1) * 6 / branchMapInfo.GridCount;
                range.MinY = startY - (branchRowIndex + 1) * 4 / branchMapInfo.GridCount;
                range.MaxY = startY - branchRowIndex * 4 / branchMapInfo.GridCount;
            }
            return range;
        }

        /// <summary>
        /// 获取图幅范围（UTM）
        /// </summary>
        public DataRange GetUtmRange(string mapName)
        {
            var range = GetRange(mapName);
            var xs = new List<double>();
            var ys = new List<double>();
            var cm = GetCentralMederian(mapName);

            var xy = GetUtmXy(range.MinX, range.MaxY, cm);
            xs.Add(xy[0]);
            ys.Add(xy[1]);
            xy = GetUtmXy(range.MaxX, range.MaxY, cm);
            xs.Add(xy[0]);
            ys.Add(xy[1]);
            xy = GetUtmXy(range.MaxX, range.MinY, cm);
            xs.Add(xy[0]);
            ys.Add(xy[1]);
            xy = GetUtmXy(range.MinX, range.MinY, cm);
            xs.Add(xy[0]);
            ys.Add(xy[1]);

            range.MinX = xs.Min(i => i);
            range.MaxX = xs.Max(i => i);
            range.MinY = ys.Min(i => i);
            range.MaxY = ys.Max(i => i);
            return range;
        }

        /// <summary>
        /// 经纬度转UTM
        /// cm:GetCentralMederian(mapName);
        /// </summary>
        public double[] GetUtmXy(double x, double y, double cm, int size = 6)
        {
            var pa = 2 * OblatusRatio - OblatusRatio * OblatusRatio;
            var pb = 1.0;

            var pc = x * Math.PI / 180;
            var pd = y * Math.PI / 180;
            var pe = cm * Math.PI / 180;

            var pf = pa / (1 - pa);
            var pg = LongRadius / Math.Sqrt(1 - pa * Math.Sin(pd) * Math.Sin(pd));
            var ph = Math.Tan(pd) * Math.Tan(pd);
            var pi = pf * Math.Cos(pd) * Math.Cos(pd);
            var pj = Math.Cos(pd) * (pc - pe);
            var pk = LongRadius * ((1 - pa / 4 - 3 * pa * pa / 64 - 5 * pa * pa * pa / 256) * pd - (3 * pa / 8 + 3 * pa * pa / 32 + 45 * pa * pa * pa / 1024) * Math.Sin(2 * pd) + (15 * pa * pa / 256 + 45 * pa * pa * pa / 1024) * Math.Sin(4 * pd) - 35 * pa * pa * pa / 3072 * Math.Sin(6 * pd));

            var ox = pb * pg * (pj + (1 - ph + pi) * pj * pj * pj / 6 + (5 - 18 * ph + ph * ph + 72 * pi - 58 * pf) * Math.Pow(pj, 5) / 120) + 500000;
            var oy = pb * (pk + (pg * Math.Tan(pd) * (pj * pj / 2 + (5 - ph + 9 * pi + 4 * pi * pi) * Math.Pow(pj, 4) / 24) + (61 - 58 * ph + ph * ph + 600 * pi - 330 * pf) * Math.Pow(pj, 6) / 720));
            ox = Math.Round(ox, size);
            oy = Math.Round(oy, size);
            return new[] { ox, oy };
        }

        /// <summary>
        /// 获取中央经线
        /// </summary>
        public double GetCentralMederian(double longitude, int type = 6)
        {
            double centralMederian = 0;
            if (type == 6)
            {
                var prjNo = ((int)longitude) / 6 + 1;
                centralMederian = (prjNo * 6) - 3;
            }
            else if (type == 3)
            {
                var prjNo = (int)(longitude - 1.5) / 3 + 1;
                centralMederian = prjNo * 3;
            }
            return centralMederian;
        }

        /// <summary>
        /// 获取中央经线
        /// </summary>
        public double GetCentralMederian(string mapName)
        {
            var range = GetRange(mapName);
            var fd = 6;
            if (Convert.ToInt32(GetScale(mapName).Split(':').Last()) <= 10000) fd = 3;
            var cm = GetCentralMederian((range.MinX + range.MaxX) / 2, fd);
            return cm;
        }

        /// <summary>
        /// 获取图名
        /// </summary>
        public string GetMapName(double x, double y, string mapScale)
        {
            var xIndex = (int)((x - StartX) / 6);
            var yIndex = (int)((StartY - y) / 4);
            var mapName = _millionRows[yIndex] + _millionPaths[xIndex];
            var minX = StartX + xIndex * 6;
            var maxY = StartY - yIndex * 4;

            var mapInfo = _branchMapInfos.SingleOrDefault(i => i.BranchMapName == mapScale);
            if (mapInfo != null)
            {
                var tempString = "0000";
                var xi = ((int)((x - minX) * mapInfo.GridCount / 6) + 1).ToString();
                var yi = ((int)((maxY - y) * mapInfo.GridCount / 4) + 1).ToString();

                var count = mapInfo.GridCount > 1000 ? 4 : 3;
                xi = tempString.Substring(0, count - xi.Count()) + xi;
                yi = tempString.Substring(0, count - yi.Count()) + yi;
                mapName = mapName + mapInfo.BranchMapCode + yi + xi;
            }
            return mapName;
        }

        /// <summary>
        /// 获取范围集
        /// </summary>
        public List<string> GetMapsByRange(DataRange dr, string mapScale)
        {
            var bmap = GetMapName(dr.MinX, dr.MaxY, mapScale);
            var emap = GetMapName(dr.MaxX, dr.MinY, mapScale);

            var bxi = _millionRows.IndexOf(bmap.Substring(0, 1));
            var exi = _millionRows.IndexOf(emap.Substring(0, 1));
            var byi = _millionPaths.IndexOf(bmap.Substring(1, 2));
            var eyi = _millionPaths.IndexOf(emap.Substring(1, 2));

            int bxc = 0, exc = 0, byc = 0, eyc = 0;
            if (bmap.Length == 10 || bmap.Length == 12)
            {
                bxc = Convert.ToInt32(bmap.Substring(4, (bmap.Length - 4) / 2));
                byc = Convert.ToInt32(bmap.Substring(4 + (bmap.Length - 4) / 2, (bmap.Length - 4) / 2));
                exc = Convert.ToInt32(emap.Substring(4, (emap.Length - 4) / 2));
                eyc = Convert.ToInt32(emap.Substring(4 + (emap.Length - 4) / 2, (emap.Length - 4) / 2));
            }

            var maps = new List<string>();
            var tempString = "0000";
            for (var i = bxi; i <= exi; i++)
            {
                for (var j = byi; j <= eyi; j++)
                {
                    var mapName = _millionRows[i] + _millionPaths[j];
                    var mapInfo = _branchMapInfos.SingleOrDefault(k => k.BranchMapName == mapScale);
                    if (mapInfo != null)
                    {
                        int bx = 1, ex = mapInfo.GridCount, by = 1, ey = mapInfo.GridCount;
                        if (i == bxi) bx = bxc;
                        if (i == exi) ex = exc;
                        if (j == byi) by = byc;
                        if (j == eyi) ey = eyc;
                        for (var k = bx; k <= ex; k++)
                        {
                            for (var l = by; l <= ey; l++)
                            {
                                var xi = k.ToString();
                                var yi = l.ToString();

                                var count = mapInfo.GridCount > 1000 ? 4 : 3;
                                xi = tempString.Substring(0, count - xi.Count()) + xi;
                                yi = tempString.Substring(0, count - yi.Count()) + yi;
                                maps.Add(mapName + mapInfo.BranchMapCode + xi + yi);
                            }
                        }
                    }
                    else
                    {
                        maps.Add(mapName);
                    }
                }
            }

            return maps;
        }

        /// <summary>
        /// 获取投影文件
        /// </summary>
        /// <param name="mapNo"></param>
        /// <param name="isZone"></param>
        /// <returns></returns>
        public string GetPrjUrl(string mapNo, bool isZone = false)
        {
            string prjUrl;
            if (string.IsNullOrEmpty(mapNo)) return "\\Projection\\GCS China Geodetic Coordinate System 2000.prj";
            var cm = GetCentralMederian(mapNo);

            var fd = 6;
            if (Convert.ToInt32(GetScale(mapNo).Split(':').Last()) > 10000)
            {
                prjUrl = "\\Projection\\CGCS2000 GK ";
            }
            else
            {
                prjUrl = "\\Projection\\CGCS2000 3 Degree GK ";
                fd = 3;
            }

            if (isZone)
            {
                prjUrl = prjUrl + "Zone " + (fd == 3 ? cm / fd : (cm + 3) / fd) + ".prj";
            }
            else
            {
                prjUrl = prjUrl + "CM " + cm + "E.prj";
            }

            return prjUrl;
        }

        //度转度分秒
        public string GetJwd(double x, string xsw = "F1")
        {
            var y = (int)x;
            var z = (x - y) * 60;
            var l = (int)z;
            var m = (z - l) * 60;
            var xx = Convert.ToDouble(m.ToString(xsw));
            if (Math.Abs(xx - 60) <= 0)
            {
                m = 0;
                l = l + 1;
            }
            if (l == 60)
            {
                l = 0;
                y = y + 1;
            }
            return y.ToString() + (l < 10 ? "0" : "") + l.ToString() + (m < 10 ? "0" : "") + m.ToString(xsw);
        }
    }

    public class BranchMapInfo
    {
        /// <summary>
        /// 分幅图名
        /// </summary>
        public string BranchMapName { get; set; }

        /// <summary>
        /// 分幅图代号
        /// </summary>
        public string BranchMapCode { get; set; }

        /// <summary>
        /// 分幅图在百万图幅中的行列数
        /// </summary>
        public int GridCount { get; set; }
    }

    public class DataRange
    {
        /// <summary>
        /// X最小
        /// </summary>
        public double MinX { get; set; }

        /// <summary>
        /// Y最小
        /// </summary>
        public double MinY { get; set; }

        /// <summary>
        /// X最大
        /// </summary>
        public double MaxX { get; set; }

        /// <summary>
        /// Y最大
        /// </summary>
        public double MaxY { get; set; }
    }
}
