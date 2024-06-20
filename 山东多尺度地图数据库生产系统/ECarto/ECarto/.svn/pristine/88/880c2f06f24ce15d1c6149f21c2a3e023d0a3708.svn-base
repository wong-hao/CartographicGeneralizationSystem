using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
namespace SMGI.Common
{
    public class GTemplate
    {
        /// <summary>
        /// Caption of this Template
        /// </summary>
        public string Caption { get; internal set; }

        public string[] Products
        {
            get
            {
                return Class.Products;
            }
        }

        public string ClassName { get; internal set; }
        public string CommandLayout { get; internal set; }

        public string LayoutStyle { get; internal set; }
        /// <summary>
        /// xml format content
        /// </summary>
        public XElement Content { get; internal set; }



        public string Root { get; set; }

        public void MatchCurrentWorkspace()
        {
            Class.MatchCurrentWorkspace(this);
        }

        internal FileInfo FileInfo { get; set; }
        internal GApplication Application { get; set; }
        internal ITemplateClass Class { get; set; }
        internal XDocument XCommandLayout { get; set; }
        internal GTemplate() { }
        internal XDocument ReadLayout()
        {
            if (Root == null)
                return null;
            if (CommandLayout == null)
                return null;

            string path = Root + "\\" + CommandLayout;
            if (!File.Exists(path))
            {
                return null;
            }
            try
            {
                using (var stream = File.Open(path, FileMode.Open))
                {
                    XDocument doc = XDocument.Load(stream);
                    return doc;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return null;
            }

        }
        public void SaveTemplate()
        {
            XDocument doc = ToXml(this);
            doc.Save(FileInfo.FullName);
        }

        internal static XDocument ToXml(GTemplate t)
        {
            return new XDocument(
                new XElement("Template",
                    new XElement("Caption", t.Caption),
                    new XElement("ClassName", t.ClassName),
                    new XElement("CommandLayout", t.CommandLayout),
                    t.Content
                )
                );
        }

        internal static GTemplate FromFileInfo(FileInfo f)
        {
            using (var fst = f.Open(FileMode.Open))
            {
                XDocument doc = XDocument.Load(fst);
                XElement xml = doc.Element("Template");
                return new GTemplate
                {
                    Caption = xml.Element("Caption").Value,
                    ClassName = xml.Element("ClassName").Value,
                    CommandLayout = xml.Element("CommandLayout").Value,
                    Content = xml.Element("Content"),
                    FileInfo = f
                };
            }
        }
    }

    internal interface ITemplateClass
    {
        string Name { get; }
        string[] Products { get; }
        bool IsValid(GTemplate t);
        bool IsValidWorkspace(ESRI.ArcGIS.Geodatabase.IWorkspace w);
        void MatchCurrentWorkspace(GTemplate t);
    }

    internal sealed class TemplateClassFactory
    {
        static TemplateClassFactory()
        {
            Factory = new TemplateClassFactory();

            RegistAssembly(typeof(ITemplateClass).Assembly);
        }

        public static void RegistAssembly(Assembly asm)
        {
            Type[] tps = asm.GetTypes();
            foreach (var tp in tps)
            {
                if (tp.IsAbstract || !tp.IsClass)
                {
                    continue;
                }

                Type itp = tp.GetInterface(typeof(ITemplateClass).Name);
                if (itp == null)
                {
                    continue;
                }
                try
                {
                    ITemplateClass tem = Activator.CreateInstance(tp) as ITemplateClass;
                    RegistTemplateClass(tem);
                }
                catch
                {
                }
            }
        }
        public static void RegistTemplateClass(ITemplateClass cls)
        {
            if (cls != null)
            {
                Factory.TemplateClasses[cls.Name] = cls;
            }
        }
        public static string[] RegistedClassNames()
        {
            return Factory.TemplateClasses.Keys.ToArray();
        }
        public static ITemplateClass TemplateClass(string name)
        {
            if (Factory.TemplateClasses.ContainsKey(name))
            {
                return Factory.TemplateClasses[name];
            }
            else
            {
                return null;
            }
        }

        #region 私有变量
        static TemplateClassFactory Factory;
        Dictionary<string, ITemplateClass> TemplateClasses;
        TemplateClassFactory()
        {
            TemplateClasses = new Dictionary<string, ITemplateClass>();
        }
        #endregion
    }

    public class TemplateManager
    {
        const string TemplatesFileName = "Template.xml";
        const string ProductsFileName = "Products.xml";
        public List<GTemplate> Templates { get; set; }

        public GTemplate Template
        {
            get;
            private set;
        }

        Dictionary<string,Dictionary<string,string>> FieldNameMapping = null;
        public string getFieldAliasName(string fieldName, string fcName = "")
        {
            if (FieldNameMapping == null)
                return fieldName;

            fcName = fcName.ToUpper();
            fieldName = fieldName.ToUpper();
            if (!FieldNameMapping.ContainsKey(fcName) || !FieldNameMapping[fcName].ContainsKey(fieldName))
                return fieldName;

            return FieldNameMapping[fcName][fieldName];
        }

        TemplateSelectForm dlg;
        TemplatesDialog dlg2;
        public TemplateManager()
        {
            var platformInfo = LoadProduct();
            LoadTemplate();

            if (this.Templates.Count == 1)//只有一个模板文件时,直接弹出系统界面
            {
                Template = this.Templates[0];
            }
            else
            {
                dlg = new TemplateSelectForm(platformInfo);
                dlg.TemplateManager = this;
                dlg.ShowDialog();

                if (dlg.CurrentTemplates.Count == 0)
                {
                    System.Windows.Forms.MessageBox.Show("没有安装相应的模板");
                    Application.Exit();
                }
                else if (dlg.CurrentTemplates.Count == 1)
                {
                    Template = dlg.CurrentTemplates[0];
                }
                else
                {
                    Templates = dlg.CurrentTemplates;
                    dlg2 = new TemplatesDialog();
                    dlg2.TemplateManager = this;
                    dlg2.ShowDialog();

                    Template = dlg2.CurrentTemplate;
                }
            }
           

            LoadFieldConfig();
        }

        public TemplateManager(string productName)
        {
            var platformInfo = LoadProduct();
            LoadTemplate();

            if (this.Templates.Count == 1)//只有一个模板文件时,直接弹出系统界面
            {
                Template = this.Templates[0];
            }
            else
            {
                List<GTemplate> curTemplates = new List<GTemplate>();
                foreach (var t in Templates)
                {
                    if (t.ClassName == productName)
                    {
                        curTemplates.Add(t);
                    }
                }

                if (curTemplates.Count == 0)
                {
                    System.Windows.Forms.MessageBox.Show("没有安装相应的模板");
                    Application.Exit();
                }
                else if (curTemplates.Count == 1)
                {
                    Template = curTemplates[0];
                }
                else
                {
                    Templates = curTemplates;
                    dlg2 = new TemplatesDialog();
                    dlg2.TemplateManager = this;
                    dlg2.ShowDialog();

                    Template = dlg2.CurrentTemplate;
                }
            }


            LoadFieldConfig();
        }

        public TemplateManager(string productName,string templateName)
        {
            var platformInfo = LoadProduct();
            LoadTemplate();
            foreach (var t in Templates)
            {
                if (t.ClassName == productName && t.Caption == templateName)
                {
                    Template = t;
                    return;
                }
            }
            if (Template == null)
            {
                System.Windows.Forms.MessageBox.Show("没有安装相应的模板");

                Application.Exit();
            }

            LoadFieldConfig();
        }

        PlatformInfo LoadProduct()
        {
            PlatformInfo platInfo = new PlatformInfo();

            try
            {                
                string templatePath = GApplication.TemplatePath;

                string productXmlPath = templatePath + "\\" + ProductsFileName;

                using (var f = File.Open(productXmlPath, FileMode.Open))
                {
                    var doc = XDocument.Load(f);

                    platInfo.BGImage = doc.Element("Platform").Element("BGImage").Value;

                    var productInfo = doc.Element("Platform").Element("Products").Elements("ProductInfo");
                    platInfo.ProList = new List<ProductInfo>();
                    foreach (var el in productInfo)
                    {
                        ProductInfo pi = new ProductInfo();
                        pi.Name = el.Element("Name").Value;
                        pi.ImagePath = el.Element("ImagePath").Value;
                        pi.Usable = Convert.ToBoolean(el.Element("Usable").Value);
                        
                        string[] locationStr = el.Element("Location").Value.Split(',');
                        pi.LocationX = int.Parse(locationStr[0]);
                        pi.LocationY = int.Parse(locationStr[1]);

                        string[] SizeStr = el.Element("Size").Value.Split(',');
                        try
                        {
                            pi.Width = int.Parse(SizeStr[0]);
                            pi.Height = int.Parse(SizeStr[1]);
                        }
                        catch
                        {
                            pi.Width = 180;
                            pi.Height = 86;
                        }

                        platInfo.ProList.Add(pi);
                    }
                }
                
            }
            catch
            {
                MessageBox.Show("配置文件错误");
                System.Windows.Forms.Application.Exit();
            }
            return platInfo;
        }

        void LoadTemplate()
        {
            string templatePath = GApplication.TemplatePath; 
            if (!Directory.Exists(templatePath))
            {
                Directory.CreateDirectory(templatePath);
            }
            DirectoryInfo dir = new DirectoryInfo(templatePath);
            var dirs = dir.GetDirectories();

            Templates = new List<GTemplate>();
            foreach (var d in dirs)
            {
                var fs = d.GetFiles(TemplatesFileName);
                if (fs.Length != 1)
                {
                    continue;
                }
                var f = fs[0];
                try
                {
                    GTemplate t = GTemplate.FromFileInfo(f);

                    t.Root = d.FullName;
                    var g = t.ReadLayout();
                    if (g == null)
                    {
                        continue;
                    }
                    else
                    {
                        t.XCommandLayout = g;
                    }

                    ITemplateClass cls = TemplateClassFactory.TemplateClass(t.ClassName);
                    if (cls != null && cls.IsValid(t))
                    {
                        t.Class = cls;
                        Templates.Add(t);
                    }
                    else if (t.ClassName == "地图服务信息管理")//特例
                    {
                        Templates.Add(t);
                    }

                }
                catch (Exception ex)
                {
                    //System.Diagnostics.Debug.WriteLine(ex);
                    continue;
                }

            }
        }

        void LoadFieldConfig()
        {
            if (Template == null)
                return;

            XElement contentXEle = Template.Content;
            XElement ele = contentXEle.Element("FieldNameMapping");
            if (ele == null)
                return;

            string val = ele.Value;
            string cfgFileName = Template.Root + "\\" + val;
            if (!System.IO.File.Exists(cfgFileName))
                return;

            FieldNameMapping = new Dictionary<string, Dictionary<string, string>>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(cfgFileName);
            XmlNodeList nodes = xmlDoc.SelectNodes("/FieldNameMapping/Field");

            foreach (XmlNode xmlnode in nodes)
            {
                if (xmlnode.NodeType != XmlNodeType.Element)
                    continue;

                string fieldName = (xmlnode as XmlElement).GetAttribute("name").ToUpper();
                string aliasName = (xmlnode as XmlElement).GetAttribute("alias").ToUpper();
                string fcName = (xmlnode as XmlElement).GetAttribute("fcname").ToUpper();

                if (!FieldNameMapping.ContainsKey(fcName))
                {
                    FieldNameMapping.Add(fcName, new Dictionary<string, string>());
                }

                if (!FieldNameMapping[fcName].ContainsKey(fieldName))
                {
                    FieldNameMapping[fcName].Add(fieldName, aliasName);
                }
                else
                {
                    //若出现重复的项，取最后一个配置
                    FieldNameMapping[fcName][fieldName] = aliasName;
                }

            }
            
        }
    }

    internal sealed class ProductInfo 
    {
        internal string Name { get; set; }
        internal string ImagePath { get; set; }
        internal bool Usable { get; set; }
        internal int LocationX { get; set; }
        internal int LocationY { get; set; }
        internal int Width { get; set; }
        internal int Height { get; set; }
    }

    internal sealed class PlatformInfo
    {
        internal List<ProductInfo> ProList { get; set; }
        internal string BGImage { get; set; }
    }
}
