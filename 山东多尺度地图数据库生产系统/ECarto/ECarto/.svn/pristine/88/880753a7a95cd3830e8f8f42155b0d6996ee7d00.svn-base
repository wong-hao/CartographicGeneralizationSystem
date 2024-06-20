using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using ESRI.ArcGIS;
using ESRI.ArcGIS.esriSystem;
using System.Xml.Linq;

namespace SMGI.DxForm
{
    static class Program
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

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //初始化皮肤库
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DevExpress.Skins.SkinManager.EnableFormSkins();
            DevExpress.UserSkins.BonusSkins.Register();

            string logPath = AppDataPath + "\\log";
            if (!System.IO.Directory.Exists(logPath))
            {
                System.IO.Directory.CreateDirectory(logPath);
            }
            logPath += "\\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".log";
            var twl = new System.Diagnostics.TextWriterTraceListener(logPath);

            System.Diagnostics.Trace.Listeners.Add(twl);
            Timer tr = new Timer { 
                Interval = 500
            };
            tr.Tick += (o, e) => { twl.Flush(); };
            tr.Start();
            Application.ApplicationExit += (o, e) => { tr.Dispose(); twl.Flush(); twl.Dispose(); };

            System.Diagnostics.Trace.WriteLine("！！！程序启动！！！");
            //string logPath = AppDataPath + "\\log";
            //判断当前登录用户是否为管理员
            System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
            if (!principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show("需要管理员权限运行");

                return;
            }


            //检查Arcgis授权
            if (!RuntimeManager.Bind(ProductCode.Desktop))
            {
                MessageBox.Show("Unable to bind to ArcGIS runtime. Application will be shut down.");
                Application.Exit();
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

                //if (aoi.IsProductCodeAvailable(esriLicenseProductCode.esriLicenseProductCodeEngineGeoDB) == esriLicenseStatus.esriLicenseAvailable)
                //{
                //    aoi.Initialize(esriLicenseProductCode.esriLicenseProductCodeEngineGeoDB);
                //}
            }

            MainForm main = new MainForm();

            //弹出模板选择窗口,选择模板
            SMGI.Common.TemplateManager TemplateManager;
            if (args == null || args.Length == 0)
            {
#if DEBUG
                TemplateManager = new SMGI.Common.TemplateManager();
#else//发布模式下，不允许直接运行程序
                MessageBox.Show("没有指定参数");
                Application.Exit();
                return;
#endif
            }
            else if (args.Length == 1)
            {
                if (args[0] == "-platform")
                {
                    TemplateManager = new SMGI.Common.TemplateManager();
                }
                else
                {
                    TemplateManager = new SMGI.Common.TemplateManager(args[0]);
                }
            }
            else
            {
                TemplateManager = new SMGI.Common.TemplateManager(args[0], args[1]);
            }
            if (TemplateManager.Template == null)
            {
                MessageBox.Show("没有可以使用的系统模板");
                Application.Exit();
                return;
            }

            XElement contentXEle = TemplateManager.Template.Content;
            //根据选择的模板获取皮肤样式
            XElement styleXEle = contentXEle.Element("SkinStyle");
            string skinStyle = styleXEle.Value;
            UserLookAndFeel.Default.SetSkinStyle(skinStyle);
            

            //根据选择的模板获取启动图片
            XElement imgXEle = contentXEle.Element("SatrtImage");
            string imgFullFileName = TemplateManager.Template.Root + "\\" + imgXEle.Value;
            System.Drawing.Image startImg = new System.Drawing.Bitmap(imgFullFileName);

            //初始化启动图片            
            SMGI.Common.InitForm initForm = new SMGI.Common.InitForm(startImg);

            initForm.Info("正在初始化应用程序……");
            SMGI.Common.GApplication app = new SMGI.Common.GApplication(main, initForm, TemplateManager);
            main.App = app;
            Application.Run(main);

        }
    }
}