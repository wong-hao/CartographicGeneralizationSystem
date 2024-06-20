using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Display;
namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    /// <summary>
    /// 专题图表数据源
    /// </summary>
    public class ChartsDataSource
    {
        /// <summary>
        /// 获取Txt数据
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, double>> ObtainTxtData(string dataSource)
        {
            Dictionary<string, Dictionary<string, double>> ChartDatas = new Dictionary<string, Dictionary<string, double>>();
            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(dataSource, FileMode.Open))
                {
                    using (StreamReader sw = new StreamReader(fs))
                    {
                        string line = sw.ReadLine();//系列
                        string[] titles = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        Dictionary<string, List<double>> datas = new Dictionary<string, List<double>>();
                        int ct = 0;
                        while ((line = sw.ReadLine()) != null)
                        {
                            ct++;
                            if (line.Trim() == "")
                                continue;
                            string[] infos = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            List<double> list = new List<double>();
                            for (int i = 1; i < infos.Length; i++)
                            {
                                list.Add(double.Parse(infos[i]));

                            }
                            datas[infos[0]] = list;

                        }
                        //整理数据
                        for (int i = 0; i < titles.Length; i++)
                        {
                            Dictionary<string, double> tem = new Dictionary<string, double>();
                            foreach (var kv in datas)
                            {
                                tem[kv.Key] = kv.Value[i];
                            }
                            ChartDatas[titles[i]] = tem;
                        }
                    }
                }
                return ChartDatas;
            }
            catch (Exception ex)
            {
                MessageBox.Show("获取专题数据出错：" + ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 获取Excel多系列数据源
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, double>> ObtainExcelData(string excelpath)
        {
            Dictionary<string, Dictionary<string, double>> ChartDatas = new Dictionary<string, Dictionary<string, double>>();
            ExcelGenerator eg = new ExcelGenerator();
            eg.CreateNewDocumentByTemp(excelpath);
            bool XtoY = eg.XToY;
            WaitOperation wo= GApplication.Application.SetBusy();
            try
            {

                wo.SetText("正在获取专题数据....");
                //地名标题
                int column = 1;
                int row = 1;
                List<string> geoNames = new List<string>();
                while ((eg.worksheet.Cells[1, column + 1]).Value != null)//右边值不为空
                {
                    Microsoft.Office.Interop.Excel.Range cell0 = eg.worksheet.Cells[1, column + 1];
                    string val = Convert.ToString((cell0.Value));
                    geoNames.Add(val.Trim());
                    column++;
                }
                //标题
                List<string> titles = new List<string>();
                while ((eg.worksheet.Cells[row + 1, 1]).Value != null)//右边值不为空
                {
                    Microsoft.Office.Interop.Excel.Range cell0 = eg.worksheet.Cells[row + 1, 1];
                    string val = Convert.ToString((cell0.Value));
                    titles.Add(val.Trim());
                    row++;
                }
                //值
                row = 2; column = 2;
                if(!XtoY)
                {
                    for (int c = 0; c < geoNames.Count; c++)//列
                    {

                        Dictionary<string, double> datas = new Dictionary<string, double>();
                        for (int r = 0; r < titles.Count; r++)//行
                        {
                            Microsoft.Office.Interop.Excel.Range cell0 = eg.worksheet.Cells[r + 2, c + 2];
                            datas[titles[r]] = 0;
                            if (cell0.Value != null)//值不为空
                            {
                                string val =Convert.ToString((cell0.Value));
                                double ret=0;
                                double.TryParse(val, out ret);
                                datas[titles[r]] = ret;
                            }
                        }

                        ChartDatas[geoNames[c]] = datas;

                    }
                }
                else
                {
                    for (int r = 0; r < titles.Count; r++)//行
                    {

                        Dictionary<string, double> datas = new Dictionary<string, double>();
                        for (int c = 0; c < geoNames.Count; c++)//列
                        {
                            Microsoft.Office.Interop.Excel.Range cell0 = eg.worksheet.Cells[r + 2, c + 2];
                            datas[geoNames[c]] = 0;
                            if (cell0.Value != null)//值不为空
                            {
                                string val = Convert.ToString((cell0.Value));
                                double ret = 0;
                                double.TryParse(val, out ret);
                                datas[geoNames[c]] = ret;
                            }
                        }

                        ChartDatas[titles[r]] = datas;

                    }
                }
                eg.CloseExcel();
                wo.Dispose();
                return ChartDatas;
            }
            catch (Exception ex)
            {
                wo.Dispose();
                eg.CloseExcel();
                MessageBox.Show("获取数据源失败！"+ex.Message,"提示");
                return ChartDatas;
            }
           

        }

        public static Dictionary<string, IPoint> ObtainGeoRelated(string layerName)
        {
            Dictionary<string,IPoint> dic = new Dictionary<string, IPoint>();
            if (layerName == "")
                return dic;
            IMap pMap = GApplication.Application.ActiveView as IMap;
            if (pMap == null)
                return null;
            IFeatureClass fcl = null;
            var bouaLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName == layerName)).ToArray();
            if (bouaLayer.Length == 0)
            {
                bouaLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name == layerName)).ToArray();
            }
            
            fcl = (bouaLayer[0] as IFeatureLayer).FeatureClass;
          
            IFeature fe;
           
            int nameindex=fcl.FindField("NAME");
            if (nameindex == -1)
            {
                MessageBox.Show("缺少name字段");
                return dic;
            }
            IFeatureCursor cursor = fcl.Search(null, false);
            while ((fe = cursor.NextFeature()) != null)
            {
                IArea area=fe.Shape as IArea;               
                dic[fe.get_Value(nameindex).ToString()] = area.LabelPoint;
            }
            Marshal.ReleaseComObject(cursor);
            return dic;
        }
        /// <summary>
        /// 分级数据
        /// </summary>
        public static void ObtainGrade0(string excelpath, ref Dictionary<string, double> gdb, ref Dictionary<string, string> grade, ref List<double> breaks)
        {
            ExcelGenerator eg = new ExcelGenerator();
            eg.CreateNewDocumentByTemp(excelpath);
            WaitOperation wo= GApplication.Application.SetBusy();
            try
            {
                int column = 1;
                int row = 1;
                wo.SetText("正在获取专题数据....");
                 Microsoft.Office.Interop.Excel.Range cell0=null;
                //GDP值,分级
               
                while ((eg.worksheet.Cells[1, column + 1]).Value != null)//右边值不为空
                {
                    cell0 = eg.worksheet.Cells[1, column + 1];
                    string name=cell0.Value;
                    name = name.Trim();
                    cell0 = eg.worksheet.Cells[2, column + 1];
                    double val=cell0.Value;
                    gdb[name] = val; 
                    cell0 = eg.worksheet.Cells[3, column + 1];
                    string gra=cell0.Value;
                    grade[name] = gra;
                    column++;
                }
                column = 1;
               //中断值
                while ((eg.worksheet.Cells[4, column + 1]).Value != null)//右边值不为空
                {
                    cell0 = eg.worksheet.Cells[4, column + 1];
                    double breaksval = cell0.Value;
                    breaks.Add(breaksval);
                    column++;
                }
                eg.CloseExcel();
                wo.Dispose();
                
            }
            catch (Exception ex)
            {
                wo.Dispose();
                eg.CloseExcel();
                MessageBox.Show("获取数据源失败！"+ex.Message,"提示");
                
            }
        }
        public static void ObtainGrade(string excelpath, ref Dictionary<string, double> gdb, ref Dictionary<string, string> grade, ref List<double> breaks)
        {
            ExcelGenerator eg = new ExcelGenerator();
            eg.CreateNewDocumentByTemp(excelpath);
            WaitOperation wo = GApplication.Application.SetBusy();
            try
            {
                int column = 1;
                int row = 1;
                wo.SetText("正在获取专题数据....");
                Microsoft.Office.Interop.Excel.Range cell0 = null;
                //GDP值,分级

                while ((eg.worksheet.Cells[1, column + 1]).Value != null)//右边值不为空
                {
                    cell0 = eg.worksheet.Cells[1, column + 1];
                    string name = cell0.Value;
                    name = name.Trim();
                    cell0 = eg.worksheet.Cells[2, column + 1];
                    double val = cell0.Value;
                    gdb[name] = val;
                    //cell0 = eg.worksheet.Cells[3, column + 1];
                    //string gra=cell0.Value;
                    //grade[name] = gra;
                    column++;
                }
                column = 1;
                //中断值
                while ((eg.worksheet.Cells[3, column + 1]).Value != null)//右边值不为空
                {
                    cell0 = eg.worksheet.Cells[3, column + 1];
                    double breaksval = cell0.Value;
                    breaks.Add(breaksval);
                    column++;
                }
                //处理grade
                foreach (var kv in gdb)
                {
                    for (int i = 0; i < breaks.Count; i++)
                    {
                        if (kv.Value <= breaks[i])
                        {
                            if (i == 0)
                            {
                                grade[kv.Key] = "0~" + breaks[i];
                            }
                            else
                            {
                                grade[kv.Key] = breaks[i - 1] + "~" + breaks[i];
                            }
                            break;

                        }
                    }
                    if (!grade.ContainsKey(kv.Key))
                    {
                        grade[kv.Key] = breaks[breaks.Count - 1] + "~";
                    }
                }


                eg.CloseExcel();
                wo.Dispose();

            }
            catch (Exception ex)
            {
                wo.Dispose();
                eg.CloseExcel();
                MessageBox.Show("获取数据源失败！" + ex.Message, "提示");

            }
        }
  
    }
}
