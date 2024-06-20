using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS;
using ESRI.ArcGIS.esriSystem;
using System.Xml.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using System.IO;
namespace SMGI.Shell
{
    class Program
    {
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
        [STAThread]
        static void Main(string[] args)
        {
            string logPath = AppDataPath + "\\log";
            if (!System.IO.Directory.Exists(logPath))
            {
                System.IO.Directory.CreateDirectory(logPath);
            }
            logPath += "\\Shell_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".log";
            var twl = new System.Diagnostics.TextWriterTraceListener(logPath);

            System.Diagnostics.Trace.Listeners.Add(twl);
            Timer tr = new Timer
            {
                Interval = 500
            };
            tr.Tick += (o, e) => { twl.Flush(); };
            tr.Start();
            Application.ApplicationExit += (o, e) => { tr.Dispose(); twl.Flush(); twl.Dispose(); };


            Action<string> messageRaised = (msg) =>
            {
                System.Diagnostics.Trace.WriteLine(string.Format("[{0}]:{1}", DateTime.Now.ToString(), msg));
            };

            Action<string> errMessageRaised = (msg) =>
            {
                System.Diagnostics.Trace.WriteLine(string.Format("[{0}]:{1}", DateTime.Now.ToString(), msg));
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
            string arg = args[0];
            if (arg.StartsWith("\""))
            {
                arg = arg.Substring(1, arg.Length - 1);
            }
            System.IO.TextReader sr = null;
            if (System.IO.File.Exists(arg))
            {
                sr = new System.IO.StreamReader(arg);
            }
            else
            {
                sr = new System.IO.StringReader(arg);
            }
            XDocument doc = XDocument.Load(sr);
            sr.Dispose();
            var root = doc.Element("Arg");

            MainForm form = null;
            try
            {
                //messageRaised("=================参数=====================");
                //messageRaised(root.ToString());
                //messageRaised("==========================================\n");

                //弹出模板选择窗口,选择模板
                messageRaised("=================正在选择模板====================\n");

                SMGI.Common.TemplateManager TemplateManager =
                    new SMGI.Common.TemplateManager(root.Element("Product").Value, root.Element("Template").Value);
                if (TemplateManager.Template == null)
                {
                    errMessageRaised("没有可以使用的系统模板");
                    return;
                }
                messageRaised("=================完成模板选择====================\n");
                messageRaised("=================正在初始化====================\n");
                form = new MainForm();
                SMGI.Common.GApplication app = new Common.GApplication(form, new InitInfo(), TemplateManager);
                messageRaised("=================完成初始化====================\n");

                XElement el = root.Element("Commands");
                foreach (var e in el.Elements())
                {
                    messageRaised(string.Format("======执行【{0}】======\n", e.Name.ToString()));
                    bool Issucess =app.DoCommand(e.Name.ToString(), e, messageRaised);
                    if (!Issucess)
                    {
                        messageRaised(string.Format("======执行失败：【{0}】======\n", e.Name.ToString()));
                    }
                    else
                    {
                        messageRaised(string.Format("======完成【{0}】=======\n", e.Name.ToString()));
                    }
                    try
                    {
                        if (app.Workspace != null)
                        {
                            IWorkspace ws = app.Workspace.EsriWorkspace;
                            if (ws != null)
                            {
                                string logFile = ws.PathName.Substring(0, ws.PathName.LastIndexOf("\\"));
                                string name = System.IO.Path.GetFileNameWithoutExtension(ws.PathName);
                                logFile += "\\" + name + "执行日志.txt";
                                System.IO.FileStream fs =null;
                                if (File.Exists(logFile))
                                    fs = new System.IO.FileStream(logFile, FileMode.Append);
                                else
                                    fs = new System.IO.FileStream(logFile, System.IO.FileMode.Create);
                                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fs))
                                {
                                    string msg = Issucess ? "成功" : "失败";
                                    sw.WriteLine(e.Name.ToString() + "执行：" + msg);//
                                    sw.Flush();
                                }
                                fs.Close();
                              
                            }

                        }
                    }
                    catch
                    {
                    }

                    
                }
              
            }
            catch (Exception ex)
            {
                errMessageRaised(ex.Message);
                messageRaised("执行失败：" + ex.StackTrace);
               
            }
            finally
            {
               
                if (form != null)
                    form.Dispose();
            }
            
        }
    }
}
