using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS;
using ESRI.ArcGIS.esriSystem;
using System.Xml.Linq;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
namespace ShellTBDivided
{
    class Program
    {

        public static int majorityStep = 0;
        public static string AppDataPath
        {
            get
            {
                if (System.Environment.OSVersion.Version.Major <= 5)
                {
                    return System.IO.Path.GetFullPath(
                        System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + @"\..");
                }

                var dp = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var di = new System.IO.DirectoryInfo(dp);
                var ds = di.GetDirectories("SMGI");
                if (ds == null || ds.Length == 0)
                {
                    var sdi = di.CreateSubdirectory("SMGI");
                    return sdi.FullName;
                }
                else
                {
                    return ds[0].FullName;
                }
            }
        }
        private struct RECT { public int left, top, right, bottom; }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rc);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int w, int h, bool repaint);
   
        /// <summary>
        /// 控制台窗体居中
        /// </summary>
        public static void SetWindowPositionCenter(int i)
        {

            IntPtr hWin = GetConsoleWindow();
            RECT rc;
            GetWindowRect(hWin, out rc);
            Screen scr = Screen.FromPoint(new System.Drawing.Point(rc.left, rc.top));
            int width = scr.WorkingArea.Width / 3;
            int height = scr.WorkingArea.Height / 3;
            int nw = ((i % 3) == 0 ? 3 : i % 3);
            int nh = (i - 1) / 3 + 1;
            int xW = (nw - 1) * width;
            int yH = (nh - 1) * height;
            MoveWindow(hWin, xW, yH, width, height, true);
        }
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Console.Title = "三调地类图斑制图综合引擎";
                string logPath = AppDataPath + "\\log";
                if (!System.IO.Directory.Exists(logPath))
                {
                    System.IO.Directory.CreateDirectory(logPath);
                }
                logPath += "\\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".log";
                // var stream = new System.IO.StreamWriter(logPath);
                Action<string> messageRaised = (msg) =>
                {
                    Console.WriteLine(string.Format("[{0}]:{1}", DateTime.Now.ToString(), msg));
                };

                Action<string> errMessageRaised = (msg) =>
                {
                    Console.Error.WriteLine(msg);
                    //stream.WriteLine(string.Format("[{0}]:{1}", DateTime.Now.ToString(), msg));
                };

                if (args == null || args.Length <= 0)
                {
                    errMessageRaised("参数不够");
                    return;
                }
                //判断当前登录用户是否为管理员
                System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
                if (!principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
                {
                    errMessageRaised("需要管理员权限运行");
                    return;
                }


                //检查Arcgis授权
                if (!RuntimeManager.Bind(ProductCode.Desktop))
                {
                    errMessageRaised("Unable to bind to ArcGIS runtime. Application will be shut down.");
                    return;
                }
                AoInitialize aoi = new AoInitializeClass();
                const esriLicenseProductCode productCode = esriLicenseProductCode.esriLicenseProductCodeAdvanced;
                if (aoi.IsProductCodeAvailable(productCode) == esriLicenseStatus.esriLicenseAvailable)
                {
                    aoi.Initialize(productCode);
                    foreach (var el in Enum.GetValues(typeof(esriLicenseExtensionCode)))
                    {
                        aoi.CheckOutExtension((esriLicenseExtensionCode)el);
                    }

                }
              
                XDocument doc = null;
                try
                {
                  
                    string arg = args[0];
                    if (arg.StartsWith("\""))
                    {
                        arg = arg.Substring(1, arg.Length - 1);
                    }

                    if (System.IO.File.Exists(arg))
                    {

                        doc = XDocument.Load(arg);
                    }
                    else
                    {
                        System.IO.TextReader sr = null;
                        sr = new System.IO.StringReader(arg);
                        doc = XDocument.Load(sr);
                        sr.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("参数解析失败:" + ex.Message);
                }
                XElement cmd = null;
                try
                {
                    var root = doc.Element("Arg");
                    cmd = root.Elements().FirstOrDefault();
                    if (cmd == null)
                        return;
                    if (root.Attribute("JopName") != null)
                    {
                        Console.Title = "三调地类图斑制图综合引擎-" + root.Attribute("JopName").Value;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Doc解析失败:" + ex.Message);
                }
                switch (cmd.Name.LocalName)
                {
                    #region 中心线提取
                    case "TBCenterLine"://2.图斑剖分
                        try
                        {
                            int pos = int.Parse(cmd.Element("POS").Value);
                            SetWindowPositionCenter(pos);
                            Console.WriteLine(pos.ToString());
                            string gdbPath = cmd.Element("GDB").Value;
                            string oid = cmd.Element("FIDs").Value;
                            string fclName = cmd.Element("FclName").Value;
                            int start = int.Parse(cmd.Element("Start").Value);
                            int limit = int.Parse(cmd.Element("Limit").Value);
                            string gdb = (cmd.Element("TempGDB").Value);
                            TBDivideClass tb = new TBDivideClass(gdbPath, oid, fclName);
                            tb.ExcuteCenterLine(start, limit, oid, pos, gdb);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("执行失败:" + ex.Message);
                        }
                        break;
                    case "TBCenterLinePre"://1.剖分预处理:合并，赋值
                        try
                        {
                            string gdbPath = cmd.Element("GDB").Value;
                            string sql = cmd.Element("SQL").Value;
                            string fclName = cmd.Element("FclName").Value;
                            string tempPath = cmd.Element("TempPath").Value;
                            TBDivideClass tb = new TBDivideClass(gdbPath, sql, fclName);
                            tb.TBCenterLinePre(tempPath, "SplitLine");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("执行失败:" + ex.Message);
                        }
                        break;
                    #endregion
                    #region 狭长图斑剖分融合处理
                    case "TBDivideSplit"://2.图斑剖分
                        try
                        {
                            int pos = int.Parse(cmd.Element("POS").Value);
                            SetWindowPositionCenter(pos);
                            Console.WriteLine(pos.ToString());
                            string gdbPath = cmd.Element("GDB").Value;
                            string oid = cmd.Element("FIDs").Value;
                            string fclName = cmd.Element("FclName").Value;
                            int start = int.Parse(cmd.Element("Start").Value);
                            int limit = int.Parse(cmd.Element("Limit").Value);
                            string gdb = (cmd.Element("TempGDB").Value);
                            TBDivideClass tb = new TBDivideClass(gdbPath, oid, fclName);
                            tb.ExcuteSplit(start, limit, oid, pos, gdb);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("执行失败:" + ex.Message);
                        }
                        break;
                    case "TBDivideDelete"://3.删除
                        try
                        {
                            string itemFile = cmd.Element("Item").Value;
                            string gdbPath = cmd.Element("GDB").Value;
                            string fclName = cmd.Element("FclName").Value;
                            TBDivideClass tb = new TBDivideClass(gdbPath, fclName);
                            tb.TBDivideDel(itemFile);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("执行失败:" + ex.Message);
                        }
                        break;
                    case "TBDividePre"://1.剖分预处理:合并，赋值
                        try
                        {
                            string gdbPath = cmd.Element("GDB").Value;
                            string sql = cmd.Element("SQL").Value;
                            string fclName = cmd.Element("FclName").Value;
                            string tempPath = cmd.Element("TempPath").Value;
                            TBDivideClass tb = new TBDivideClass(gdbPath, sql, fclName);
                            tb.TBDividePre(tempPath, "SplitPoly");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("执行失败:" + ex.Message);
                        }
                        break;
                    case "TBDivide"://5.融合
                        try
                        {
                            string gdbPath = cmd.Element("GDB").Value;
                            string fclName = cmd.Element("FclName").Value;
                            string sql = cmd.Element("SQL").Value;
                            TBDivideClass tb = new TBDivideClass(gdbPath,sql, fclName);
                            tb.ExcuteDivideMerge();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("执行失败:" + ex.Message);
                        }
                        break;
                    case "TBDivideAppend"://4.追加
                        try
                        {
                            string infeature = cmd.Element("InFeature").Value;
                            string outfeature = cmd.Element("OutFeature").Value;
                            TBGPClass gpTb = new TBGPClass();
                            gpTb.AppendFeature(infeature, outfeature);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("执行失败:" + ex.Message);
                        }
                        break;
                    case "TBDivTopConstruct"://拓扑构建
                        try
                        {

                            string infeature = cmd.Element("InFeature").Value;
                            string outfeature = cmd.Element("OutFeature").Value;
                            TBGPClass gpClass = new TBGPClass();
                            gpClass.PolygonToLine(infeature, outfeature);

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("执行失败:" + ex.Message);
                        }
                        break;
                    #endregion
                    case "TBDissolve":
                        try
                        {

                            string infeature = cmd.Element("InFeature").Value;
                            string outfeature = cmd.Element("OutFeature").Value;
                            string field = cmd.Element("Field").Value;
                            string field2 = null;
                            if (cmd.Element("Field2") != null)
                            {
                                field2 = cmd.Element("Field2").Value;
                            }
                            string gdb = cmd.Element("GDB").Value;
                            TBGPClass gpClass = new TBGPClass();
                            bool write = true;
                            try
                            {
                                if (cmd.Element("OverWrite") != null)
                                {
                                    bool flag=true;
                                    if(bool.TryParse(cmd.Element("OverWrite").Value,out flag))
                                    {
                                        write = flag;
                                    }
                                
                                }
                            }
                            catch
                            {
                            }
                            gpClass.DisOverWrite = write;
                            gpClass.Dissolve(gdb, infeature, outfeature, field,field2);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("执行失败:" + ex.Message);
                        }
                        break;
                    case "TBErase":
                        try
                        {
                            
                            string inFeature = cmd.Element("InFeature").Value;
                            string eraseFeature = cmd.Element("EraseFeature").Value;
                            string gdb = cmd.Element("GDB").Value;
                            TBGPClass gpClass = new TBGPClass();
                            gpClass.ExcuteTBErase(inFeature, eraseFeature, gdb);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("执行失败:" + ex.Message);
                        }
                        break;
                    case "TBElimate":
                        try
                        {
                            string infeature = cmd.Element("InFeature").Value;
                            string outfeature = cmd.Element("OutFeature").Value;
                            string gdbPath = cmd.Element("GDB").Value;
                            double mapScale =double.Parse(cmd.Element("MapScale").Value);
                            TBGPClass gpClass = new TBGPClass();
                            gpClass.ElimateTB(gdbPath, mapScale);//先消除面积小于10的小面
                            
                            
                        }
                        catch
                        {
                        }
                        break;
                    case "TBTopConstruct"://拓扑构建
                        try
                        {
                            
                            string infeature = cmd.Element("InFeature").Value;
                            string outfeature = cmd.Element("OutFeature").Value;
                            string gdbPath = cmd.Element("GDB").Value;
                            TBGPClass gpClass = new TBGPClass();
                            //gpClass.ElimateDltb(gdbPath, 10);//先消除面积小于10的小面
                            gpClass.PolygonToLine(infeature, outfeature);

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("执行失败:" + ex.Message);
                        }
                        break;
                    case "TBMerge":
                        try
                        {
                            
                            string gdbPath = cmd.Element("GDB").Value;
                            string ruleFile = cmd.Element("RuleFile").Value;
                            string fclName = cmd.Element("FclName").Value;
                            double mapScale = double.Parse(cmd.Element("MapScale").Value);
                            string areaParms = cmd.Element("AreaParms").Value;
                            TBMergeClass mergeClass = new TBMergeClass(ruleFile, gdbPath, fclName);
                            mergeClass.TBMergeExcute(mapScale, areaParms, messageRaised);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("执行失败:" + ex.Message);
                        }
                        break;
                    case "TBBuilding":
                        try
                        {
                            
                            TBGPClass gpClass = new TBGPClass();
                            gpClass.MAggregateDis = double.Parse(cmd.Element("AggregateDis").Value);
                            gpClass.MConflict = bool.Parse(cmd.Element("Conflict").Value);
                            gpClass.MField = cmd.Element("Field").Value;
                            gpClass.MHoleArea = double.Parse(cmd.Element("HoleArea").Value);
                            gpClass.MMinArea = double.Parse(cmd.Element("MinArea").Value);
                            gpClass.MMinAreaRESA = double.Parse(cmd.Element("MinAreaRESA").Value);
                            gpClass.MOrthogonality = bool.Parse(cmd.Element("Orthogonality").Value);
                            gpClass.MSimplifyDis = double.Parse(cmd.Element("SimplifyDis").Value);
                            double scale = double.Parse(cmd.Element("Scale").Value); ;
                            string gdb = cmd.Element("GDB").Value;
                            string fclName = cmd.Element("FclName").Value;
                            string sql = string.Empty;
                            try
                            {
                                if (cmd.Element("SQL") != null)
                                {
                                    sql = cmd.Element("SQL").Value;
                                }
                            }
                            catch
                            {
                            }
                            gpClass.BuildingGeneralize(scale, fclName, gdb, sql);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("执行失败:" + ex.Message);
                        }
                        break;
                    case "TBBuildingAfter":
                        try
                        {
                           
                            TBGPClass gpClass = new TBGPClass();
                            gpClass.MField = cmd.Element("Field").Value;
                            string gdb = cmd.Element("GDB").Value;
                            string fclName = cmd.Element("FclName").Value;
                            gpClass.BuildingGenAfter(fclName, gdb);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("执行失败:" + ex.Message);
                        }
                        break;
                    case "TBRasterGeneralize":
                        try
                        {
                            
                            string exportPath = cmd.Element("ExportGDB").Value;
                            double mapscale = double.Parse(cmd.Element("MapScale").Value);
                            bool overwrite = bool.Parse(cmd.Element("OverWrite").Value);
                            bool defaultGDB = bool.Parse(cmd.Element("DefaultGDB").Value);
                            string folder = System.IO.Path.GetDirectoryName(exportPath);
                            List<string> gdbs = new List<string>();
                            var items = cmd.Element("GDB");
                            foreach (var item in items.Elements("Item"))
                            {
                                string inGDB = item.Value;
                                gdbs.Add(inGDB);
                            }
                            string fclName = cmd.Element("FclName").Value;
                            TBRasterGeneralize64 gpClass = new TBRasterGeneralize64(exportPath, mapscale, folder, gdbs,overwrite);
                            MessageBox.Show("测试");
                            gpClass.LandToRasterAndSimplyfy(defaultGDB);                 
                            Application.Exit();


                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("执行失败:" + ex.Message);
                        }
                        break;
                    case "TBSimplify":
                        #region
                        try
                        {
                            
                            string inPath = cmd.Element("GDB").Value;
                            double mapscale = double.Parse(cmd.Element("MapScale").Value);
                            double bendwidth = double.Parse(cmd.Element("BendWidth").Value);
                            double smooth = double.Parse(cmd.Element("Smooth").Value);
                            TBSimplifyClass gpClass = new TBSimplifyClass(inPath, mapscale, bendwidth, smooth);
                            gpClass.ExcuteTBSimplify();
                        }
                        catch
                        {
                        }
                        #endregion
                        break;
                    case "TBSimplifyPre":
                        #region
                        try
                        {

                            string inPath = cmd.Element("GDB").Value;
                            double mapscale = double.Parse(cmd.Element("MapScale").Value);
                            double bendwidth = double.Parse(cmd.Element("BendWidth").Value);
                            double smooth = double.Parse(cmd.Element("Smooth").Value);
                            TBSimplifyClass gpClass = new TBSimplifyClass(inPath, mapscale, bendwidth, smooth);
                            gpClass.ExcuteTBSimplifyOne();
                        }
                        catch
                        {
                        }
                        #endregion
                        break;
                    case "TBSimPolygon"://化简多边形创建
                        try
                        {
                            int pos = int.Parse(cmd.Element("POS").Value);
                            SetWindowPositionCenter(pos);
                            Console.WriteLine(pos.ToString());
                            string gdbPath = cmd.Element("GDB").Value;
                            string fileName = cmd.Element("FileName").Value;
                            string fclNewName = cmd.Element("FclNewName").Value;
                            string tempPoly = cmd.Element("TempPoly").Value;
                            string tempPolyBig = cmd.Element("TempPolyBig").Value;
                            int start = int.Parse(cmd.Element("Start").Value);
                            
                            

                            TBSimplifyClass gpClass = new TBSimplifyClass(gdbPath);
                            gpClass.ExcuteTBSimPolygon(fileName, fclNewName, tempPoly, tempPolyBig, start,pos);
                            
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("执行失败:" + ex.Message);
                        }
                        break;
                    case "TBSimplifyProcess":
                        #region
                        try
                        {

                            string inPath = cmd.Element("GDB").Value;
                            double mapscale = double.Parse(cmd.Element("MapScale").Value);
                            double bendwidth = double.Parse(cmd.Element("BendWidth").Value);
                            double smooth = double.Parse(cmd.Element("Smooth").Value);
                            TBSimplifyClass gpClass = new TBSimplifyClass(inPath, mapscale, bendwidth, smooth);
                            gpClass.ExcuteTBSimplifyTwo();
                        }
                        catch
                        {
                        }
                        #endregion
                        break;
                    case "TBSimplifyAfter":
                        #region
                        try
                        {

                            string inPath = cmd.Element("GDB").Value;
                            double mapscale = double.Parse(cmd.Element("MapScale").Value);
                            double bendwidth = double.Parse(cmd.Element("BendWidth").Value);
                            double smooth = double.Parse(cmd.Element("Smooth").Value);
                            TBSimplifyClass gpClass = new TBSimplifyClass(inPath, mapscale, bendwidth, smooth);
                            gpClass.ExcuteTBSimplifyThree();
                        }
                        catch
                        {
                        }
                        #endregion
                        break;
                    case "TBGap":
                        #region
                        try
                        {
                            
                            string gdb = cmd.Element("GDB").Value;
                            string infeature = (cmd.Element("InFeature").Value);
                            string bouafeature = (cmd.Element("BOUAFeature").Value);
                            double scale = double.Parse(cmd.Element("Scale").Value);
                            TBGPClass gpClass = new TBGPClass();
                            gpClass.ExcuteTBGap(gdb, infeature, bouafeature,scale);
                        }
                        catch
                        {
                        }
                        #endregion
                        break;
                    case "TBUnion":
                        #region
                        try
                        {
                            
                            string gdb = cmd.Element("GDB").Value;
                            bool hyda = bool.Parse(cmd.Element("HYDA").Value);
                            bool building = bool.Parse(cmd.Element("Building").Value);
                            TBOtherClass gpClass = new TBOtherClass();
                            gpClass.UnionBuildingHYDA(gdb, hyda, building);
                        }
                        catch
                        {
                        }
                        #endregion
                        break;
                    case "HYDAGen":
                        #region
                        try
                        {

                            string inPath = cmd.Element("GDB").Value;
                            double mapscale = double.Parse(cmd.Element("MapScale").Value);
                            double bendwidth = double.Parse(cmd.Element("BendWidth").Value);
                            double smooth = double.Parse(cmd.Element("Smooth").Value);
                            double area = double.Parse(cmd.Element("Area").Value);
                            double width = double.Parse(cmd.Element("Width").Value);
                            string fclName = cmd.Element("FclName").Value;//水系数据的原数据
                            TBOtherClass gpClass = new TBOtherClass();
                            gpClass.Area = area;
                            gpClass.Width = width;
                            gpClass.Smoothwidth = smooth;
                            gpClass.Bendwidth = bendwidth;
                            gpClass.Scale = mapscale;
                            bool deleteFlag = false;
                            try
                            {
                                deleteFlag = bool.Parse(cmd.Element("Delete").Value);
                            }
                            catch
                            {
                            }
                            gpClass.ExcuteHYDAGen(inPath, fclName, deleteFlag);
                           
                        }
                        catch
                        {
                        }
                        #endregion
                        break;
                    #region 栅格缩编
                    case "RasterPre":
                        {
                            #region
                            string exportPath = cmd.Element("ExportGDB").Value;
                            double mapscale = double.Parse(cmd.Element("MapScale").Value);
                            bool overwrite = bool.Parse(cmd.Element("OverWrite").Value);
                            bool defaultGDB = bool.Parse(cmd.Element("DefaultGDB").Value);
                            string folder = System.IO.Path.GetDirectoryName(exportPath);
                            List<string> gdbs = new List<string>();
                            var items = cmd.Element("GDB");
                            foreach (var item in items.Elements("Item"))
                            {
                                string inGDB = item.Value;
                                gdbs.Add(inGDB);
                            }
                            string fclName = cmd.Element("FclName").Value;
                            TBRasterGeneralize64 gpClass = new TBRasterGeneralize64(exportPath, mapscale, folder, gdbs, overwrite);
                            gpClass.ExcuteRasterPre();
                            #endregion
                        }
                        break;
                    case "ToRaster":
                        {
                            #region
                            string exportPath = cmd.Element("ExportGDB").Value;
                            double mapscale = double.Parse(cmd.Element("MapScale").Value);
                            bool overwrite = bool.Parse(cmd.Element("OverWrite").Value);
                            bool defaultGDB = bool.Parse(cmd.Element("DefaultGDB").Value);
                            string folder = System.IO.Path.GetDirectoryName(exportPath);
                            List<string> gdbs = new List<string>();
                            var items = cmd.Element("GDB");
                            foreach (var item in items.Elements("Item"))
                            {
                                string inGDB = item.Value;
                                gdbs.Add(inGDB);
                            }
                            string fclName = cmd.Element("FclName").Value;
                            string CurrentGDB = cmd.Element("CurrentGDB").Value;
                            TBRasterGeneralize64 gpClass = new TBRasterGeneralize64(exportPath, mapscale, folder, gdbs, overwrite);
                            gpClass.ExcuteRasterToRaster(CurrentGDB);
                            #endregion
                        }
                        break;
                    case "RasterToGen":
                        {
                            #region
                            string exportPath = cmd.Element("ExportGDB").Value;
                            double mapscale = double.Parse(cmd.Element("MapScale").Value);
                            bool overwrite = bool.Parse(cmd.Element("OverWrite").Value);
                            bool defaultGDB = bool.Parse(cmd.Element("DefaultGDB").Value);
                            string folder = System.IO.Path.GetDirectoryName(exportPath);
                            List<string> gdbs = new List<string>();
                            var items = cmd.Element("GDB");
                            foreach (var item in items.Elements("Item"))
                            {
                                string inGDB = item.Value;
                                gdbs.Add(inGDB);
                            }
                            string fclName = cmd.Element("FclName").Value;
                            string currentGDB = cmd.Element("CurrentGDB").Value;
                            double nowScale =double.Parse(cmd.Element("NowScale").Value);
                            TBRasterGeneralize64 gpClass = new TBRasterGeneralize64(exportPath, mapscale, folder, gdbs,majorityStep, overwrite);
                            gpClass.ExcuteRasterToGen(currentGDB,nowScale);

                            majorityStep = gpClass.mmajorityStep;
                            #endregion
                        }
                        break;
                    case "RasterMerge":
                        {
                            #region
                            string exportPath = cmd.Element("ExportGDB").Value;
                            double mapscale = double.Parse(cmd.Element("MapScale").Value);
                            bool overwrite = bool.Parse(cmd.Element("OverWrite").Value);
                            bool defaultGDB = bool.Parse(cmd.Element("DefaultGDB").Value);
                            string folder = System.IO.Path.GetDirectoryName(exportPath);
                            List<string> gdbs = new List<string>();
                            var items = cmd.Element("GDB");
                            foreach (var item in items.Elements("Item"))
                            {
                                string inGDB = item.Value;
                                gdbs.Add(inGDB);
                            }
                            string fclName = cmd.Element("FclName").Value;
                            TBRasterGeneralize64 gpClass = new TBRasterGeneralize64(exportPath, mapscale, folder, gdbs, overwrite);
                            gpClass.ExcuteRasterMerge();
                            #endregion
                        }
                        break;
                    case "RasterToVector":
                         {
                            #region
                            string exportPath = cmd.Element("ExportGDB").Value;
                            double mapscale = double.Parse(cmd.Element("MapScale").Value);
                            bool overwrite = bool.Parse(cmd.Element("OverWrite").Value);
                            bool defaultGDB = bool.Parse(cmd.Element("DefaultGDB").Value);
                            string folder = System.IO.Path.GetDirectoryName(exportPath);
                            List<string> gdbs = new List<string>();
                            var items = cmd.Element("GDB");
                            foreach (var item in items.Elements("Item"))
                            {
                                string inGDB = item.Value;
                                gdbs.Add(inGDB);
                            }
                            string fclName = cmd.Element("FclName").Value;
                            TBRasterGeneralize64 gpClass = new TBRasterGeneralize64(exportPath, mapscale, folder, gdbs, overwrite);
                            gpClass.ExcuteRasterToVector();
                            #endregion
                        }
                        break;
                    case "RasterFliter":
                        {
                            #region
                            string exportPath = cmd.Element("ExportGDB").Value;
                            double mapscale = double.Parse(cmd.Element("MapScale").Value);
                            bool overwrite = bool.Parse(cmd.Element("OverWrite").Value);
                            bool defaultGDB = bool.Parse(cmd.Element("DefaultGDB").Value);
                            string folder = System.IO.Path.GetDirectoryName(exportPath);
                            List<string> gdbs = new List<string>();
                            var items = cmd.Element("GDB");
                            foreach (var item in items.Elements("Item"))
                            {
                                string inGDB = item.Value;
                                gdbs.Add(inGDB);
                            }
                            string fclName = cmd.Element("FclName").Value;
                            TBRasterGeneralize64 gpClass = new TBRasterGeneralize64(exportPath, mapscale, folder, gdbs, overwrite);
                            gpClass.ExcuteRasterFliter();
                            #endregion
                        }
                        break;
                    case "RasterEliminate":
                        {
                            #region
                            string exportPath = cmd.Element("ExportGDB").Value;
                            double mapscale = double.Parse(cmd.Element("MapScale").Value);
                            bool overwrite = bool.Parse(cmd.Element("OverWrite").Value);
                            bool defaultGDB = bool.Parse(cmd.Element("DefaultGDB").Value);
                            int step = int.Parse(cmd.Element("NowStep").Value);
                            string folder = System.IO.Path.GetDirectoryName(exportPath);
                            List<string> gdbs = new List<string>();
                            var items = cmd.Element("GDB");
                            foreach (var item in items.Elements("Item"))
                            {
                                string inGDB = item.Value;
                                gdbs.Add(inGDB);
                            }
                            string fclName = cmd.Element("FclName").Value;
                            TBRasterGeneralize64 gpClass = new TBRasterGeneralize64(exportPath, mapscale, folder, gdbs, overwrite);
                            gpClass.ExcuteRasterEliminate(step);
                            #endregion
                        }
                        break;
                    case "RasterCopy":
                        {
                            #region
                            string exportPath = cmd.Element("ExportGDB").Value;
                            double mapscale = double.Parse(cmd.Element("MapScale").Value);
                            bool overwrite = bool.Parse(cmd.Element("OverWrite").Value);
                            bool defaultGDB = bool.Parse(cmd.Element("DefaultGDB").Value);
                            string folder = System.IO.Path.GetDirectoryName(exportPath);
                            List<string> gdbs = new List<string>();
                            var items = cmd.Element("GDB");
                            foreach (var item in items.Elements("Item"))
                            {
                                string inGDB = item.Value;
                                gdbs.Add(inGDB);
                            }
                            string fclName = cmd.Element("FclName").Value;
                            TBRasterGeneralize64 gpClass = new TBRasterGeneralize64(exportPath, mapscale, folder, gdbs, overwrite);
                            gpClass.ExcuteRasterCopy();
                            #endregion
                        }
                        break;
                   
                    #endregion

                    case "TBBackup":
                         TBSymbolizeClass tbSymClass = new TBSymbolizeClass();
                        tbSymClass.BackupDLTB(cmd.Element("GDB").Value, cmd.Element("InFeature").Value,cmd.Element("OutFeature").Value);
                        break;
                    case "TBSymbolizeCmd":
                         TBSymbolizeClass tbSymClass1 = new TBSymbolizeClass();
                         tbSymClass1.SymbolizeDLTB(cmd.Element("Root").Value, cmd.Element("GDB").Value, cmd.Element("InFeature").Value);
                        
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("失败:" + ex.Message);
            }
            finally
            {
                Application.Exit();
            }
        }
    }
}
