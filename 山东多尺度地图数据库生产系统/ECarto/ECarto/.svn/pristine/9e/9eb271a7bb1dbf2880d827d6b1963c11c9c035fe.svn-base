using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;

using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml.Linq;

namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// excel操作帮助类
    /// </summary>
  public class ExcelGenerator
    {
        #region 字段

        private dynamic app = null;//Excel应用程序
        public dynamic workbook = null;//Excel工作簿(文件)
        public dynamic worksheet = null;//Excel文件中的sheet页
        public dynamic workSheet_range = null;//Excel单元格
        public bool XToY;//是否行列互换
        //private Application app = null;//Excel应用程序
        //public Workbook workbook = null;//Excel工作簿(文件)
        //public dynamic worksheet = null;//Excel文件中的sheet页
        //public Range workSheet_range = null;//Excel单元格
        #endregion

        #region 方法

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);
        public static void KillExcel(Microsoft.Office.Interop.Excel.Application excel)
        {
            IntPtr t = new IntPtr(excel.Hwnd); //得到这个句柄，具体作用是得到这块内存入口 
            int k = 0;
            GetWindowThreadProcessId(t, out k); //得到本进程唯一标志k 
            System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(k); //得到对进程k的引用
            p.Kill(); //关闭进程k 
        }
        /// <summary>
        /// 打开模板
        /// </summary>
        /// <param name="templateName"></param>
        public void CreateNewDocumentByTemp(string templateName, string sheetName = "")
        {
            try
            {
                app = new Application();
                app.Visible = false;
                app.DisplayAlerts = false;
                app.AlertBeforeOverwriting = false;
                object objTemplateName = templateName;
                object objMissing = System.Reflection.Missing.Value;
                workbook = app.Workbooks.Open(templateName);
                if (sheetName != "")
                {
                    worksheet = (dynamic)workbook.Sheets[sheetName];
                }
                else
                {
                  
                    worksheet = (dynamic)workbook.Sheets[0];
                   
                }
            }
            catch (Exception ex)
            {
            }
        }
        //获取sheetName
        public List<string> getSheetName(string excelPath)
        {
            List<string> shName = new List<string>();
            try
            {
                app = new Application();
                app.Visible = false;
                app.DisplayAlerts = false;
                app.AlertBeforeOverwriting = false;
                object objMissing = System.Reflection.Missing.Value;
                workbook = app.Workbooks.Open(excelPath);
                for (int i = 1; i < workbook.Sheets.Count + 1; i++)
                {
                    dynamic sheet = workbook.Sheets[i];
                    string s = sheet.Name;
                    shName.Add(s);
                }
            }
            catch (Exception ex)
            { 
            }
            return shName;
        }

        public bool CreateNewDocument()
        {
            try
            {
                app = new Application();
                app.Visible = false;
                workbook = app.Workbooks.Add();
                worksheet = (dynamic)workbook.Sheets[1];

            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }
        public void CloseExcel()
        {
            try
            {
                if (workbook != null && app != null && worksheet != null)
                {
                    workbook.Save();
                    workbook.Close(true, Type.Missing, Type.Missing);
                    worksheet = null;
                    workbook = null;
                    app.Quit();
                    KillExcel(app);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void CloseExcelEx()
        {
            try
            {
                if (workbook != null && app != null)
                {
                    workbook.Save();
                    workbook.Close(true, Type.Missing, Type.Missing);
                    worksheet = null;
                    workbook = null;
                    app.Quit();
                    KillExcel(app);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void SaveExcel()
        {
            try
            {
                workbook.Save();

            }
            catch (Exception ex)
            {
            }
        }
        public bool SaveDocument(string fileName)
        {
            try
            {
                worksheet.SaveAs(fileName, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                    XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing);
                workbook.Close(false, Type.Missing, Type.Missing);
                app.Quit();
                KillExcel(app);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="htext"></param>
        /// <param name="cell1"></param>
        /// <param name="cell2"></param>
        /// <param name="size"></param>
        /// <param name="columnWidth"></param>
        public void addHeader(int row, int col, string htext, string cell1, string cell2, int size = 12, int columnWidth = 20)
        {
            worksheet.Cells[row, col] = htext;
            workSheet_range = worksheet.get_Range(cell1, cell2);
            workSheet_range.Merge();

            workSheet_range.Font.Size = size; //设置字体大小
            workSheet_range.Font.Bold = true; //设置字体加粗
            workSheet_range.ColumnWidth = columnWidth;
            //workSheet_range.EntireColumn.AutoFit(); //设置自动调整列宽
            workSheet_range.WrapText = false;//文本是否自动换行


            workSheet_range.HorizontalAlignment = Constants.xlCenter; // 设置文本水平居中方式     
            workSheet_range.VerticalAlignment = Constants.xlCenter; // 设置文本垂直居中方式


            //workSheet_range.Font.Name = "黑体"; //设置字体的种类
            //workSheet_range.Font.Color = System.Drawing.Color.Black.ToArgb();//字体颜色
            //workSheet_range.Interior.Color = System.Drawing.Color.Gray.ToArgb();//设置背景色  
            //workSheet_range.Borders.Color = System.Drawing.Color.Black.ToArgb();//设置边框的颜色    
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="data"></param>
        /// <param name="cell1"></param>
        /// <param name="cell2"></param>
        /// <param name="size"></param>
        public void addData(int row, int col, string data, string cell1, string cell2, int size = 10)
        {
            worksheet.Cells[row, col] = data;
            workSheet_range = worksheet.get_Range(cell1, cell2);
            workSheet_range.Merge();

            workSheet_range.Font.Size = size; //设置字体大小 

            workSheet_range.HorizontalAlignment = Constants.xlCenter; // 设置文本水平居中方式     
            workSheet_range.VerticalAlignment = Constants.xlCenter; // 设置文本垂直居中方式 
        }

        #endregion

    }
    public  class OLEDBExcel
    {
   
        public static  DataSet ExcelToDS(string ExcelPath,string Sql)
        {
            string strConn = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + ExcelPath + ";" + "Extended Properties=Excel 8.0;";
            OleDbConnection conn = new OleDbConnection(strConn);
            conn.Open();
            OleDbDataAdapter myAdatpter = null;
            DataSet ds = null;
            myAdatpter = new OleDbDataAdapter(Sql, strConn);
            ds = new DataSet();
            myAdatpter.Fill(ds, "table");
            conn.Close();
            return ds;
        }
        /// <summary>
        /// 更新Excel
        /// </summary>
        /// <param name="Path">Excel路径</param>
        /// <param name="CommandText">操作字符串</param>
        /// <returns>受影响的行数</returns>
        public static int UpdataExcel(string ExcelPath, string CommandText, Dictionary<string, string> vals)
        {
            string strConn = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + ExcelPath + ";" + "Extended Properties=Excel 8.0;";
            OleDbConnection conn = new OleDbConnection(strConn);
            conn.Open();
            OleDbCommand myCommand = new OleDbCommand(CommandText, conn);
            foreach (var kv in vals)
            {
                myCommand.Parameters.Add(new OleDbParameter(kv.Key, kv.Value));
            }

            int n = myCommand.ExecuteNonQuery();
            conn.Close();
            return n;
        }

        /// <summary>
        /// 查找列名索引
        /// </summary>
        /// <param name="columnname">列名</param>
        /// <param name="dt">表格</param>
        /// <returns>索引</returns>
        public static int FindColumn(string columnname, System.Data.DataTable dt)
        {
            int n = -1;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (dt.Columns[i].ColumnName == columnname)
                {
                    n = i;
                    break;
                }
            }
            return n;
        }


    }

    public  class AutoMapHelper
    {
        public static void UpdateState(string filexml,string state)
        {
          
            if (File.Exists(filexml))
            {
                try
                {
                    XDocument dc = XDocument.Load(filexml);
                    var content = dc.Element("Arg");
                    if (content.Element("Index") == null)
                        return;
                    if (content.Element("Excel") == null)
                        return;
                    string id = content.Element("Index").Value;
                    string excel = content.Element("Excel").Value;
                    content.SetElementValue("State", state);
                    dc.Save(filexml);
                    if (string.IsNullOrEmpty(excel)) return;
                    if (excel == "") return;
                    string sql = "update [Sheet1$] set 处理结果 = @result where 序号=@id";
                    var pars = new Dictionary<string, string>();
                    pars["result"] = state;
                    pars["id"] = id;
                    OLEDBExcel.UpdataExcel(excel, sql, pars);
                }
                catch
                {
                }
            }
        }

        public static void InsertMapinfoToExcel(string filexml, string scale,string pageType)
        {

            if (File.Exists(filexml))
            {
                try
                {
                    XDocument dc = XDocument.Load(filexml);
                    var content = dc.Element("Arg");
                    string id = string.Empty;
                    string excel = string.Empty;
                    if (content.Element("Index") == null)
                        return;
                    if (content.Element("Excel") == null)
                        return;
                    id = content.Element("Index").Value;
                    excel = content.Element("Excel").Value;

                    string sql = "update [Sheet1$] set 比例尺 = @scale where 序号=@id";
                    var pars = new Dictionary<string, string>();
                    pars["scale"] = scale;
                    pars["id"] = id;
                    OLEDBExcel.UpdataExcel(excel, sql, pars);
                    sql = "update [Sheet1$] set 横竖版 = @pageType where 序号=@id";
                    var pars2 = new Dictionary<string, string>();
                    pars2["pageType"] = pageType;
                    pars2["id"] = id;
                    OLEDBExcel.UpdataExcel(excel, sql, pars2);
                }
                catch
                {
                }
            }
        }
    }
}
