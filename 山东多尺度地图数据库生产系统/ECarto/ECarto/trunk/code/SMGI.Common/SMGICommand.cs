using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.ADF.BaseClasses;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System.IO;
using System.Security.Cryptography;
using ESRI.ArcGIS.Controls;
using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters.Binary;

namespace SMGI.Common
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class SMGIParameterAttribute : Attribute
    {
        // This is a positional argument
        public SMGIParameterAttribute(string name, object defaultValue, string infomation = "未进行描述")
        {
            this.Name = name;
            this.Infomation = infomation;
            this.DefaultValue = defaultValue;
        }

        public string Name
        {
            get;
            private set;
        }
        public string Infomation
        {
            get;
            private set;
        }
        public object DefaultValue
        {
            get;
            private set;
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public sealed class SMGISubSystemAttribute : Attribute
    {

        // This is a positional argument
        public SMGISubSystemAttribute(SMGISubSystem sys)
        {
            this.ProductName = sys;
        }

        public SMGISubSystem ProductName { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public sealed class SMGIProductAttribute : Attribute
    {

        // This is a positional argument
        public SMGIProductAttribute(string sys)
        {
            this.SubSystem = sys;
        }

        public string SubSystem { get; private set; }
    }

    public class SMGICommand : ICommand, ISMGIPlugin,ISMGIAutomaticCommand
    {
        //for ITool
        #region 兼容BaseCommand
        protected Bitmap m_bitmap;

        protected string m_caption;

        protected string m_category;

        protected bool m_checked;

        protected bool m_enabled;

        protected string m_helpFile;

        protected int m_helpID;

        protected string m_message;

        protected string m_name;

        protected string m_toolTip;
        #endregion

        #region ICommand
        public virtual int Bitmap
        {
            get
            {
                return BaseCommand.GetHBitmap(m_bitmap).ToInt32();
            }
        }
        public virtual string Caption { get { return m_caption.ToSafeString(); } }
        public virtual string Category { get { return m_category.ToSafeString();  } }
        public virtual bool Checked { get { return m_checked; } }
        public virtual bool Enabled { get { return m_enabled; } }
        public virtual int HelpContextID { get { return m_helpID; } }
        public virtual string HelpFile { get { return m_helpFile.ToSafeString(); } }
        public virtual string Message { get { return m_message.ToSafeString(); } }
        public virtual string Name { get { return m_name.ToSafeString(); } }
        public virtual string Tooltip { get { return Tooltip.ToSafeString(); } }

        //使用Sealed关键字
        public void OnCreate(object hook)
        {
        }

        static string[] cc;
        static DateTime dtStart;
        static DateTime dtStop;
        
        static bool viii(string name)
        {
            try
            {
                bool a = cc.Contains(name);
                a = a & DateTime.Now > dtStart;
                a = a & DateTime.Now < dtStop;
                return a;
            }
            catch
            {
                return false;
            }
        }

        static void riii(string name)
        {
            #region check
            if (!viii(name))
            {
                try
                {
                    if (RegistryHelper.IsRegistryExist(Registry.LocalMachine, "SOFTWARE", "SMGI"))
                    {
                        RegistryKey rsg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\SMGI", true); //true表可修改
                        if (rsg.GetValue("SMGI") != null)  //如果值不为空
                        {
                            string rv = rsg.GetValue("SMGI").ToString();
                            StringReader sr = new StringReader(rv);
                            List<string> regValue = new List<string>();
                            regValue.Add(sr.ReadLine());
                            regValue.Add(sr.ReadLine());
                            regValue.Add(sr.ReadLine());
                            regValue.Add(sr.ReadLine());
                            sr.Dispose();
                            //string[] regValue = rv.Split('\n','\r');
                            if (regValue.Count == 4)
                            {
                                //读取值
                                string sysMachineName;
                                BinaryFormatter formatter = new BinaryFormatter();
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    formatter.Serialize(ms, System.Environment.MachineName);
                                    sysMachineName = Convert.ToBase64String(ms.ToArray());
                                }

                                string mN = sysMachineName;
                                //string mN = SoftRegister.CalculateSeialNum(origNum);
                                string k = regValue[0];
                                StringBuilder sb = new StringBuilder();
                                dtStart = DateTime.Parse(regValue[1]);
                                dtStop = DateTime.Parse(regValue[2]);
                                string product = regValue[3];
                                cc = product.Split(new char[] { ';' });

                                sb.AppendLine(regValue[1]);
                                sb.AppendLine(regValue[2]);
                                sb.AppendLine(product);
                                string info = mN + sb.ToString();

                                bool a = false;
                                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                                {
                                    RSA.ImportCspBlob(pubKey);

                                    a = RSA.VerifyData(System.Text.Encoding.Unicode.GetBytes(info), new SHA1CryptoServiceProvider(), Convert.FromBase64String(k));
                                }
                                if (!a)
                                {
                                    cc = null;
                                }
                                rsg.Close();
                            }
                        }
                    }

                }
                catch
                {
                }
            }
            #endregion

        }

        void ICommand.OnClick()
        {
            if (Clicked != null)
                Clicked();
            riii(this.Product);
            if (viii(this.Product))
            {                
                this.OnClick();
            }
            else
            {
                RegFrom reg = new RegFrom();
                reg.ShowDialog();
            }
        }
        
        #endregion

        bool ISMGIAutomaticCommand.DoCommand(System.Xml.Linq.XElement args, Action<string> messageRaisedAction)
        {
            riii(this.Product);
            if (viii(this.Product))
            {
                var att = this.GetType().GetCustomAttributes(typeof(SMGIAutomaticCommandAttribute), false);
                if (att == null || att.Length == 0)
                {
                    messageRaisedAction("该工具不是自动化工具！");
                    return false;
                }
                return this.DoCommand(args, messageRaisedAction);
            }
            else
            {
                messageRaisedAction("该工具没有注册，请在主窗口注册后使用！");
                return false;
            }
        }
        internal event Action Clicked;
        protected GApplication m_Application = null;
        protected SMGICommand()
        {
            m_enabled = false;
            m_name = this.GetType().FullName;
            m_category = "未分类";
            m_toolTip = "没有提示";
            m_message = "没有消息";
            m_bitmap = AutoResource.制图选择;
        }

        protected virtual bool DoCommand(System.Xml.Linq.XElement args, Action<string> messageRaisedAction)
        {
            throw new NotImplementedException("该工具未实现自动化方法");
        }
        public virtual void OnClick()
        { 
        }
        public virtual void setApplication(Common.GApplication app)
        {
            m_Application = app;
            RegistParameters();
        }

        protected void LoadParameters() {
            Type t = this.GetType();
            var infos = t.GetProperties();
            foreach (var i in infos)
            {
                var attributes = i.GetCustomAttributes(typeof(SMGIParameterAttribute), false);
                if (attributes != null)
                {
                    foreach (var pi in attributes)
                    {
                        SMGIParameterAttribute pai = pi as SMGIParameterAttribute;
                        i.SetValue(this, m_Application.GParameters[pai.Name, this.Caption],null);
                    }
                }
            }
        }
        protected void SaveParameters() {
            Type t = this.GetType();
            var infos = t.GetProperties();
            foreach (var i in infos)
            {
                var attributes = i.GetCustomAttributes(typeof(SMGIParameterAttribute), false);
                if (attributes != null)
                {
                    foreach (var pi in attributes)
                    {
                        SMGIParameterAttribute pai = pi as SMGIParameterAttribute;
                        m_Application.GParameters[pai.Name, this.Caption] = i.GetValue(this,null);
                    }
                }
            }
        }

        private void RegistParameters()
        {
            Type t = this.GetType();
            var infos = t.GetProperties();
            foreach (var i in infos)
            {
                var attributes = i.GetCustomAttributes(typeof(SMGIParameterAttribute), false);
                if (attributes != null)
                {
                    foreach (var pi in attributes)
                    {
                        SMGIParameterAttribute pai = pi as SMGIParameterAttribute;
                        m_Application.GParameters.RegistParameter(pai.Name, this.Caption, pai.DefaultValue, pai.Infomation);
                    }
                }
            }
        }

        protected object this[string parameterName]
        {
            get { return m_Application.GParameters[parameterName, m_category]; }
            set { m_Application.GParameters[parameterName, m_category] = value; ; }
        }



        public event EventHandler PluginChanged;


        public SMGISubSystem SubSystem
        {
            get {
                var t= this.GetType().Assembly;
                var ass = t.GetCustomAttributes(typeof(SMGISubSystemAttribute),false);
                if (ass == null || ass.Length <= 0)
                    return SMGISubSystem.其他功能;
                return (ass[0] as SMGISubSystemAttribute).ProductName;
            }
        }

        public string Product
        {
            get
            {
                var t = this.GetType().Assembly;
                var ass = t.GetCustomAttributes(typeof(SMGIProductAttribute), false);
                if (ass == null || ass.Length <= 0)
                    return "ECarto";
                return (ass[0] as SMGIProductAttribute).SubSystem;
            }
        }

        static byte[] pubKey = {6,2,0,0,0,164,0,0,82,83,
                                65,49,0,4,0,0,1,0,1,0,
                                193,32,12,206,92,114,242,214,168,189,
                                136,34,82,178,118,254,229,85,15,40,
                                122,59,60,204,198,213,13,72,131,139,
                                28,126,217,191,53,143,138,112,171,153,
                                89,170,117,225,136,76,39,191,88,200,
                                192,77,26,101,132,181,209,42,109,229,
                                105,63,11,9,52,211,225,96,147,161,
                                83,97,73,63,16,78,236,79,113,108,
                                134,106,41,114,49,222,247,141,170,234,
                                197,90,55,139,26,95,61,32,196,139,
                                146,60,215,21,88,84,83,111,29,218,
                                203,105,3,216,76,119,254,161,223,40,
                                109,6,163,206,181,188,105,176};



    }
}
